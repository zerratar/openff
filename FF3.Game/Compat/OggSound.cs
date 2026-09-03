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
// Decoded once and kept: a two-minute stereo track is ~20 MB of PCM, and the game plays
// a handful at a time.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Audio;
using NVorbis;

namespace FF3
{
	internal static class OggSound
	{
		private static readonly Dictionary<string, SoundEffect> _cache =
			new Dictionary<string, SoundEffect>(StringComparer.OrdinalIgnoreCase);
		private static readonly HashSet<string> _missing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		/// <summary>Whether the chain has an Ogg or AKB for this sound name.</summary>
		public static bool Has(string name)
		{
			return Find(name) != null;
		}

		/// <summary>
		/// The sound as a SoundEffect, or null if the chain has no Ogg/AKB for it - in
		/// which case the caller falls back to the XNB. Decode failures are logged and
		/// count as missing, so the game keeps its old path rather than going silent.
		/// </summary>
		public static SoundEffect Load(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				return null;
			}
			lock (_cache)
			{
				if (_cache.TryGetValue(name, out SoundEffect have))
				{
					return have;
				}
				if (_missing.Contains(name))
				{
					return null;
				}
			}

			string entry = Find(name);
			if (entry == null)
			{
				lock (_cache) _missing.Add(name);
				return null;
			}

			try
			{
				byte[] data = GameArchive.Read(entry);
				int start = OggStart(data);
				if (start < 0)
				{
					throw new InvalidDataException("no OggS page in " + entry);
				}
				SoundEffect sound = Decode(data, start);
				lock (_cache) _cache[name] = sound;
				Log.Write(LogChannel.File, "sound: " + entry + " -> " + sound.Duration.TotalSeconds.ToString("0.0") + "s");
				return sound;
			}
			catch (Exception ex)
			{
				Log.Write(LogChannel.File, "sound: could not decode " + entry + ": " + ex.Message);
				lock (_cache) _missing.Add(name);
				return null;
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
			return null;
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

		private static SoundEffect Decode(byte[] data, int start)
		{
			using MemoryStream stream = new MemoryStream(data, start, data.Length - start, false);
			using VorbisReader reader = new VorbisReader(stream, false);
			int channels = reader.Channels;
			int rate = reader.SampleRate;
			float[] buffer = new float[channels * 4096];
			using MemoryStream pcm = new MemoryStream();
			byte[] bytes = new byte[buffer.Length * 2];
			int read;
			while ((read = reader.ReadSamples(buffer, 0, buffer.Length)) > 0)
			{
				for (int i = 0; i < read; i++)
				{
					float f = buffer[i];
					short s = (short)Math.Max(-32768, Math.Min(32767, (int)(f * 32767f)));
					bytes[i * 2] = (byte)s;
					bytes[i * 2 + 1] = (byte)(s >> 8);
				}
				pcm.Write(bytes, 0, read * 2);
			}
			AudioChannels ac = channels >= 2 ? AudioChannels.Stereo : AudioChannels.Mono;
			return new SoundEffect(pcm.ToArray(), rate, ac);
		}
	}
}
