// Sound from the content chain: Ogg Vorbis decoded to PCM, or FF4's AKB (an Ogg with a
// header on it), before falling back to the XNB the phone build shipped.
//
// The Steam build of FF3 keeps its music and effects as sound/BGMnn_0.ogg + _1.ogg
// (intro and loop) and sound/SEnnn_nn.ogg, the same names our XNBs use minus the
// extension. So a request for "BGM01_0" looks for sound/BGM01_0.ogg first, and the
// SoundManager above this - which already keys everything by name and reads the loop
// point from sound/<name>.dat - does not change. FF4's files/SOUND/BGM/BGM01.akb is the
// same again with a header: skip to the OggS page and decode.
//
// Decoded once and kept while there is room. A two-minute stereo track is ~20 MB of PCM;
// FF3's music comes to 428 MB decoded and its effects to 153 MB, so a long session that
// kept everything it played would hold most of that. A sound a player is playing stays;
// of the rest, the least recently asked for are let go past IdleBudget, and decoded
// again if they play again. What Load hands out is never let go: its callers keep it.
//
// Decoding costs about 1.7 ms per second of audio - 70 ms for a 40-second loop - and on
// the game thread that is the step the music changes in, held. So where the game's next
// tune is known before it plays (a battle's theme and fanfares as the battle loads, a
// map's music once its parameters are read, a loop while its intro decodes), SoundManager
// asks for it here with Prefetch. The file is read then, on the game thread (the content
// chain is not for two threads), and decoded to PCM on a worker; the SoundEffect is made
// from that PCM on the game thread when the sound is asked for, since making one calls
// OpenAL through MonoGame. Asked for while the worker is on it, the game thread waits for
// the rest; asked for before the worker has begun it, the game thread decodes it itself,
// as it always did - so asking ahead never makes a sound later than not asking.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework.Audio;
using NVorbis;

namespace OpenFF.Client
{
	// MonoGame's types by name: the engine's OpenFF.Game, GameTime, Color and vectors sit a namespace up.
	using Game = Microsoft.Xna.Framework.Game;

	internal static class OggSound
	{
		/// <summary>How much PCM the sounds nobody is playing may hold: a dozen tracks and the effects in reach.</summary>
		private const long IdleBudget = 128L << 20;

		/// <summary>How much PCM decodes asked for ahead may hold before they are asked for (a defeat's theme, decoded at every battle's start).</summary>
		private const long AheadBudget = 64L << 20;

		/// <summary>
		/// Frames the PCM is sized for past the stream's own length. Ten of FF3's 476 files
		/// decode longer than their last page's granule position says, by up to 448 frames.
		/// </summary>
		private const int SlackFrames = 4096;

		/// <summary>A decoded sound: its SoundEffect, what its PCM costs, and who holds it.</summary>
		private sealed class Cached
		{
			public SoundEffect Sound;

			public long Bytes;

			/// <summary>Players holding it now: Acquire counts one in, Release one out.</summary>
			public int Users;

			/// <summary>Handed out by Load, whose callers keep what they get: never let go.</summary>
			public bool Kept;

			/// <summary>When it was last asked for, on _clock: the least recent is let go first.</summary>
			public long Used;
		}

		/// <summary>16-bit PCM, little-endian and interleaved, and what it plays at.</summary>
		private sealed class Pcm
		{
			public byte[] Data;

			/// <summary>The bytes of Data that are sound: Data is sized a little long (SlackFrames).</summary>
			public int Count;

			public int Rate;

			public int Channels;
		}

		/// <summary>A decode asked for ahead: the file, read on the game thread, and in time its PCM.</summary>
		private sealed class Ahead
		{
			public string Entry;

			public byte[] Data;

			public int Start;

			/// <summary>0 until a thread takes it on: the worker, or the game thread wanting it before the worker got to it.</summary>
			public int Taken;

			/// <summary>Set, under the job's own lock, once Pcm or Error is.</summary>
			public volatile bool Done;

			public Pcm Pcm;

			public Exception Error;

			/// <summary>How long the decode took, wherever it ran.</summary>
			public double Ms;

			/// <summary>When it was asked for, on _clock: the oldest unclaimed is let go first.</summary>
			public long Queued;

			/// <summary>Its place in the worker's queue, until the worker takes it out.</summary>
			public LinkedListNode<Ahead> Place;
		}

		private static readonly Dictionary<string, Cached> _cache =
			new Dictionary<string, Cached>(StringComparer.OrdinalIgnoreCase);
		private static readonly HashSet<string> _missing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		/// <summary>Decodes asked for ahead and not claimed yet - waiting, running or done. Under _cache's lock, as is _clock.</summary>
		private static readonly Dictionary<string, Ahead> _ahead =
			new Dictionary<string, Ahead>(StringComparer.OrdinalIgnoreCase);
		private static long _clock;

		/// <summary>The worker's queue, taken from the front. Under its own lock.</summary>
		private static readonly LinkedList<Ahead> _waiting = new LinkedList<Ahead>();
		private static Thread _worker;

		/// <summary>Whether the chain has an Ogg or AKB for this sound name.</summary>
		public static bool Has(string name)
		{
			return Find(name) != null;
		}

		/// <summary>
		/// The sound as a SoundEffect, or null if the chain has no Ogg/AKB for it - in
		/// which case the caller falls back to the XNB. Decode failures are logged and
		/// count as missing, so the game keeps its old path rather than going silent.
		/// What this hands out is kept for the session, for callers that hold on to it.
		/// </summary>
		public static SoundEffect Load(string name)
		{
			return Get(name, keep: true);
		}

		/// <summary>
		/// As Load, for a player that gives the sound back with Release once it has disposed
		/// of its instances: a sound nobody holds is kept only while there is room for it.
		/// On the game thread.
		/// </summary>
		public static SoundEffect Acquire(string name)
		{
			return Get(name, keep: false);
		}

		/// <summary>Gives back a sound Acquire handed out under this name.</summary>
		public static void Release(string name, SoundEffect sound)
		{
			if (string.IsNullOrEmpty(name) || sound == null)
			{
				return;
			}
			lock (_cache)
			{
				if (_cache.TryGetValue(name, out Cached have) && have.Sound == sound && have.Users > 0)
				{
					have.Users--;
				}
			}
		}

		/// <summary>
		/// Starts a sound the game will soon ask for decoding on the worker; on the game
		/// thread. One already decoded, already on its way or not in the chain is left as it
		/// is. With soon, it goes ahead of whatever else the worker has waiting: the game is
		/// about to wait for it. Never throws: asking ahead is only ever a head start.
		/// </summary>
		public static void Prefetch(string name, bool soon = false)
		{
			if (string.IsNullOrEmpty(name))
			{
				return;
			}
			try
			{
				Queue(name, soon);
			}
			catch (Exception ex)
			{
				Log.Write(LogChannel.File, "sound: " + name + " not decoded ahead: " + ex.Message);
			}
		}

		private static void Queue(string name, bool soon)
		{
			Ahead queued;
			lock (_cache)
			{
				if (_cache.TryGetValue(name, out Cached have))
				{
					// Wanted again soon: not the next to be let go.
					have.Used = ++_clock;
					return;
				}
				if (_missing.Contains(name))
				{
					return;
				}
				_ahead.TryGetValue(name, out queued);
			}
			if (queued != null)
			{
				if (soon)
				{
					// Already asked for, and still waiting: to the front.
					lock (_waiting)
					{
						if (queued.Place != null && queued.Place.List == _waiting && queued.Taken == 0)
						{
							_waiting.Remove(queued.Place);
							_waiting.AddFirst(queued.Place);
						}
					}
				}
				return;
			}

			string entry = Find(name);
			if (entry == null)
			{
				lock (_cache) _missing.Add(name);
				return;
			}
			byte[] data = GameArchive.Read(entry);
			int start = data == null || IsWav(data) ? -1 : OggStart(data);
			if (start < 0)
			{
				// A WAV, which MonoGame reads quickly itself, or a file Load will report.
				return;
			}

			Ahead job = new Ahead { Entry = entry, Data = data, Start = start };
			lock (_cache)
			{
				job.Queued = ++_clock;
				_ahead[name] = job;
				TrimAhead();
			}
			lock (_waiting)
			{
				job.Place = soon ? _waiting.AddFirst(job) : _waiting.AddLast(job);
				Monitor.Pulse(_waiting);
			}
			if (_worker == null)
			{
				// Below the game's own threads: it only decodes what is not needed yet, and a
				// game thread waiting on it leaves it the core.
				_worker = new Thread(Work) { IsBackground = true, Name = "OggSound decode", Priority = ThreadPriority.BelowNormal };
				_worker.Start();
			}
		}

		private static SoundEffect Get(string name, bool keep)
		{
			if (string.IsNullOrEmpty(name))
			{
				return null;
			}
			Ahead ahead;
			lock (_cache)
			{
				if (_cache.TryGetValue(name, out Cached have))
				{
					Hold(have, keep);
					return have.Sound;
				}
				if (_missing.Contains(name))
				{
					return null;
				}
				if (_ahead.TryGetValue(name, out ahead))
				{
					_ahead.Remove(name);
				}
			}

			string entry = ahead != null ? ahead.Entry : Find(name);
			if (entry == null)
			{
				lock (_cache) _missing.Add(name);
				return null;
			}

			try
			{
				SoundEffect sound;
				long bytes;
				string how = "";
				if (ahead != null)
				{
					Pcm pcm = Claim(ahead, out how);
					sound = Make(pcm);
					bytes = pcm.Count;
					ahead.Pcm = null;
				}
				else
				{
					byte[] data = GameArchive.Read(entry);
					if (data == null)
					{
						throw new IOException("the chain could not read it");
					}
					if (IsWav(data))
					{
						// A WAV of a mod's own (PCM): MonoGame reads the RIFF itself.
						using MemoryStream wav = new MemoryStream(data, false);
						sound = SoundEffect.FromStream(wav);
						bytes = data.Length;
					}
					else
					{
						int start = OggStart(data);
						if (start < 0)
						{
							throw new InvalidDataException("no OggS page in " + entry);
						}
						Pcm pcm = Decode(data, start);
						sound = Make(pcm);
						bytes = pcm.Count;
					}
				}
				lock (_cache)
				{
					Cached made = new Cached { Sound = sound, Bytes = bytes };
					Hold(made, keep);
					_cache[name] = made;
					Trim();
				}
				Log.Write(LogChannel.File, "sound: " + entry + " -> " + sound.Duration.TotalSeconds.ToString("0.0") + "s" + how);
				return sound;
			}
			catch (Exception ex)
			{
				Log.Write(LogChannel.File, "sound: could not decode " + entry + ": " + ex.Message);
				lock (_cache) _missing.Add(name);
				return null;
			}
		}

		/// <summary>Counts one more holder of a cached sound, or keeps it for good; under _cache's lock.</summary>
		private static void Hold(Cached have, bool keep)
		{
			have.Used = ++_clock;
			if (keep)
			{
				have.Kept = true;
			}
			else
			{
				have.Users++;
			}
		}

		/// <summary>
		/// The PCM of a decode asked for ahead, now the sound is wanted: waited for while the
		/// worker is on it, decoded here if the worker has not begun it. Throws what the
		/// decode threw.
		/// </summary>
		private static Pcm Claim(Ahead job, out string how)
		{
			if (Interlocked.CompareExchange(ref job.Taken, 1, 0) == 0)
			{
				// Still waiting behind others: decoded here, as though never asked ahead.
				Run(job);
				how = "";
			}
			else
			{
				Stopwatch waited = Stopwatch.StartNew();
				lock (job)
				{
					while (!job.Done)
					{
						Monitor.Wait(job);
					}
				}
				double ms = waited.Elapsed.TotalMilliseconds;
				how = " (decoded ahead in " + job.Ms.ToString("0") + " ms" + (ms >= 1 ? ", waited " + ms.ToString("0") + " ms" : "") + ")";
			}
			if (job.Error != null)
			{
				throw job.Error;
			}
			return job.Pcm;
		}

		/// <summary>Decodes a job, on whichever thread took it, and wakes anyone waiting for it.</summary>
		private static void Run(Ahead job)
		{
			Stopwatch clock = Stopwatch.StartNew();
			try
			{
				job.Pcm = Decode(job.Data, job.Start);
			}
			catch (Exception ex)
			{
				job.Error = ex;
			}
			job.Data = null;
			job.Ms = clock.Elapsed.TotalMilliseconds;
			lock (job)
			{
				job.Done = true;
				Monitor.PulseAll(job);
			}
		}

		/// <summary>The worker: takes the queue from the front, skipping what the game thread took first.</summary>
		private static void Work()
		{
			while (true)
			{
				Ahead job;
				lock (_waiting)
				{
					while (_waiting.Count == 0)
					{
						Monitor.Wait(_waiting);
					}
					job = _waiting.First.Value;
					_waiting.RemoveFirst();
					job.Place = null;
				}
				if (Interlocked.CompareExchange(ref job.Taken, 1, 0) == 0)
				{
					Run(job);
					// It only lets go of references, so this thread may.
					lock (_cache) TrimAhead();
				}
			}
		}

		/// <summary>
		/// Lets go of the least recently asked-for sounds nobody holds while they come to more
		/// than IdleBudget, then of unclaimed decodes past AheadBudget. Under _cache's lock, on
		/// the game thread: it disposes what it lets go.
		/// </summary>
		private static void Trim()
		{
			long idle = 0;
			foreach (Cached c in _cache.Values)
			{
				if (c.Users == 0 && !c.Kept)
				{
					idle += c.Bytes;
				}
			}
			while (idle > IdleBudget)
			{
				string name = null;
				Cached oldest = null;
				foreach (KeyValuePair<string, Cached> pair in _cache)
				{
					Cached c = pair.Value;
					if (c.Users == 0 && !c.Kept && (oldest == null || c.Used < oldest.Used))
					{
						name = pair.Key;
						oldest = c;
					}
				}
				if (oldest == null)
				{
					break;
				}
				_cache.Remove(name);
				idle -= oldest.Bytes;
				oldest.Sound.Dispose();
				Log.Write(LogChannel.File, "sound: " + name + " let go (" + (oldest.Bytes / 1048576.0).ToString("0.0") + " MB), " + (idle / 1048576.0).ToString("0") + " MB of idle sound kept");
			}
			TrimAhead();
		}

		/// <summary>Lets go of the oldest finished decodes nobody has claimed while they hold more than AheadBudget; under _cache's lock.</summary>
		private static void TrimAhead()
		{
			long held = 0;
			foreach (Ahead a in _ahead.Values)
			{
				if (a.Done && a.Pcm != null)
				{
					held += a.Pcm.Data.Length;
				}
			}
			while (held > AheadBudget)
			{
				string name = null;
				Ahead oldest = null;
				foreach (KeyValuePair<string, Ahead> pair in _ahead)
				{
					Ahead a = pair.Value;
					if (a.Done && a.Pcm != null && (oldest == null || a.Queued < oldest.Queued))
					{
						name = pair.Key;
						oldest = a;
					}
				}
				if (oldest == null)
				{
					break;
				}
				_ahead.Remove(name);
				held -= oldest.Pcm.Data.Length;
				Log.Write(LogChannel.File, "sound: " + oldest.Entry + " was decoded ahead and not asked for; let go");
			}
		}

		/// <summary>The chain entry holding this sound, by the names the two games use, or null.</summary>
		private static string Find(string name)
		{
			if (GameArchive.Chain == null)
			{
				return null;
			}
			foreach (string candidate in new[]
			{
				"sound/" + name + ".ogg",
				"sound/" + name + ".wav",
				"files/SOUND/BGM/" + name + ".akb",
				"files/SOUND/SE/" + name + ".akb",
				"files/SOUND/VOICE/" + name + ".akb"
			})
			{
				if (GameArchive.Chain.Exists(candidate))
				{
					return candidate;
				}
			}
			// FF4 keeps a track in one .akb, loop points in its header, where FF3 asked for
			// an intro (_0) and a loop (_1): the loop half is the whole file; there is no intro.
			if (name.EndsWith("_1", StringComparison.Ordinal))
			{
				string whole = name.Substring(0, name.Length - 2);
				foreach (string candidate in new[]
				{
					"files/SOUND/BGM/" + whole + ".akb",
					"files/SOUND/SE/" + whole + ".akb",
					"files/SOUND/VOICE/" + whole + ".akb"
				})
				{
					if (GameArchive.Chain.Exists(candidate))
					{
						return candidate;
					}
				}
			}
			return null;
		}

		private static bool IsWav(byte[] data)
		{
			return data.Length > 12 && data[0] == 'R' && data[1] == 'I' && data[2] == 'F' && data[3] == 'F';
		}

		/// <summary>Where the Ogg stream begins: 0 for a plain file, after the header for AKB.</summary>
		private static int OggStart(byte[] data)
		{
			if (data == null || data.Length < 4)
			{
				return -1;
			}
			if (data[0] == 'O' && data[1] == 'g' && data[2] == 'g' && data[3] == 'S')
			{
				return 0;
			}
			for (int i = 0; i + 4 <= data.Length; i++)
			{
				if (data[i] == 'O' && data[i + 1] == 'g' && data[i + 2] == 'g' && data[i + 3] == 'S')
				{
					return i;
				}
			}
			return -1;
		}

		/// <summary>
		/// An Ogg stream as 16-bit PCM, on any thread: it touches nothing but its arguments,
		/// and NVorbis readers share nothing. The PCM is sized once, from the stream's own
		/// length and a little slack, so a track costs its PCM and no more - the MemoryStream
		/// this used to fill grew by doubling and was then copied out, two to three times the
		/// PCM in garbage and all of it on the large object heap.
		/// </summary>
		private static Pcm Decode(byte[] data, int start)
		{
			using MemoryStream stream = new MemoryStream(data, start, data.Length - start, false);
			using VorbisReader reader = new VorbisReader(stream, false);
			int channels = reader.Channels;
			int rate = reader.SampleRate;
			long frames = reader.TotalSamples;
			byte[] pcm = new byte[frames > 0 && frames < int.MaxValue / 4 / channels - SlackFrames ? (frames + SlackFrames) * channels * 2 : 1 << 20];
			float[] buffer = new float[channels * 4096];
			int count = 0;
			int read;
			while ((read = reader.ReadSamples(buffer, 0, buffer.Length)) > 0)
			{
				if (count + read * 2 > pcm.Length)
				{
					// Longer than the stream said, past the slack, or it did not say.
					Array.Resize(ref pcm, Math.Max(count + read * 2, pcm.Length * 2));
				}
				for (int i = 0; i < read; i++)
				{
					short s = (short)Math.Max(-32768, Math.Min(32767, (int)(buffer[i] * 32767f)));
					pcm[count++] = (byte)s;
					pcm[count++] = (byte)(s >> 8);
				}
			}
			return new Pcm { Data = pcm, Count = count, Rate = rate, Channels = channels };
		}

		/// <summary>
		/// The SoundEffect for decoded PCM, on the game thread. Count, not the array's length:
		/// MonoGame's OpenAL path hands OpenAL the count from the array's start.
		/// </summary>
		private static SoundEffect Make(Pcm pcm)
		{
			AudioChannels ac = pcm.Channels >= 2 ? AudioChannels.Stereo : AudioChannels.Mono;
			return new SoundEffect(pcm.Data, 0, pcm.Count, pcm.Rate, ac, 0, 0);
		}
	}
}
