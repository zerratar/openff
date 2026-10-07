// A movie, played as Steam's FF4.exe plays opening.mkv: the picture over black, kept to its own shape in the window, and
// its sound at the music's volume. The file is read twice at once, each on a thread of its own - the pictures through the
// VP8 decoder (Vp8Decoder) into a short queue of RGBA frames, the sound's Vorbis packets through NVorbis into PCM - and the
// game's thread starts the clock when both have something, hands the sound to a DynamicSoundEffectInstance as it is
// wanted and puts up the newest picture whose time has come (Update, then Draw into the step's DrawList).
//
// Done is set when the last picture has had its time; Stop ends it early (a skip).

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.Xna.Framework.Audio;
using NVorbis;
using NVorbis.Contracts;

namespace OpenFF.Client
{
	using Color = OpenFF.Color;

	internal sealed class MoviePlayer : IDisposable
	{
		/// <summary>The movie on the screen, if one is: input meant for the game skips it instead (PauseMenu leaves Esc to it).</summary>
		public static MoviePlayer Current { get; private set; }

		private const int QueuedFrames = 4;

		private readonly string _path;
		private readonly Thread _videoThread, _audioThread;
		private volatile bool _stopping;

		// Pictures: decoded RGBA with its time, and spent buffers to decode the next into.
		private readonly ConcurrentQueue<(long TimeMs, byte[] Rgba)> _frames = new ConcurrentQueue<(long, byte[])>();
		private readonly ConcurrentBag<byte[]> _spare = new ConcurrentBag<byte[]>();
		private readonly SemaphoreSlim _room = new SemaphoreSlim(QueuedFrames, QueuedFrames);
		private volatile bool _videoDone;
		private int _width, _height;
		private long _lastTimeMs, _frameMs = 42;

		// Sound: PCM blocks (16-bit, interleaved) as the decoder makes them.
		private readonly ConcurrentQueue<byte[]> _pcm = new ConcurrentQueue<byte[]>();
		private volatile bool _audioDone;
		private int _sampleRate, _channels;
		private DynamicSoundEffectInstance _sound;

		private readonly Stopwatch _clock = new Stopwatch();
		private OpenFF.Texture _texture;
		private long _shownTimeMs = -1;

		public bool Done { get; private set; }
		public string Error { get; private set; }

		public MoviePlayer(string path)
		{
			_path = path;
			_videoThread = new Thread(VideoLoop) { IsBackground = true, Name = "movie video" };
			_audioThread = new Thread(AudioLoop) { IsBackground = true, Name = "movie audio" };
		}

		public void Start()
		{
			Current = this;
			_videoThread.Start();
			_audioThread.Start();
			Log.Write(LogChannel.General, "movie: " + _path);
		}

		// ---- the threads ----

		private void VideoLoop()
		{
			try
			{
				using Matroska mkv = new Matroska(_path);
				Matroska.Track video = mkv.Tracks.Find(t => t.Type == 1 && t.Codec == "V_VP8");
				if (video == null) throw new InvalidOperationException("no VP8 track");
				if (video.DefaultDurationNs > 0) _frameMs = video.DefaultDurationNs / 1000000;
				using Vp8Decoder decoder = new Vp8Decoder();
				foreach (Matroska.Block block in mkv.Blocks(t => t == video.Number))
				{
					foreach (byte[] frame in block.Frames)
					{
						if (_stopping) return;
						if (!decoder.Decode(frame, frame.Length) || !decoder.Shown) continue;
						_width = decoder.Width;
						_height = decoder.Height;
						_room.Wait();
						if (_stopping) return;
						if (!_spare.TryTake(out byte[] rgba) || rgba.Length != _width * _height * 4) rgba = new byte[_width * _height * 4];
						decoder.ToRgba(rgba);
						_lastTimeMs = block.TimeMs;
						_frames.Enqueue((block.TimeMs, rgba));
					}
				}
			}
			catch (Exception ex) { Error = "video: " + ex.Message; }
			finally { _videoDone = true; }
		}

		private void AudioLoop()
		{
			try
			{
				using Matroska mkv = new Matroska(_path);
				Matroska.Track audio = mkv.Tracks.Find(t => t.Type == 2 && t.Codec == "A_VORBIS");
				if (audio == null || audio.CodecPrivate == null) return;
				PacketQueue packets = new PacketQueue();
				foreach (byte[] header in XiphHeaders(audio.CodecPrivate)) packets.Add(header);
				IEnumerator<Matroska.Block> blocks = mkv.Blocks(t => t == audio.Number).GetEnumerator();
				// The decoder reads its three headers as it is made; the stream's packets follow as it wants them.
				packets.More = () =>
				{
					if (_stopping || !blocks.MoveNext()) return false;
					foreach (byte[] f in blocks.Current.Frames) packets.Add(f);
					return true;
				};
				using StreamDecoder decoder = new StreamDecoder(packets);
				_channels = decoder.Channels;
				_sampleRate = decoder.SampleRate;
				float[] buffer = new float[_sampleRate / 10 * _channels];
				while (!_stopping)
				{
					int read = decoder.Read(buffer, 0, buffer.Length);
					if (read <= 0) break;
					byte[] pcm = new byte[read * 2];
					for (int i = 0; i < read; i++)
					{
						int s = (int)(buffer[i] * 32767f);
						s = s < short.MinValue ? short.MinValue : s > short.MaxValue ? short.MaxValue : s;
						pcm[i * 2] = (byte)s;
						pcm[i * 2 + 1] = (byte)(s >> 8);
					}
					_pcm.Enqueue(pcm);
					// Keep a few seconds ahead of the clock at most.
					while (!_stopping && _pcm.Count > 60) Thread.Sleep(20);
				}
			}
			catch (Exception ex) { Error = "audio: " + ex.Message; }
			finally { _audioDone = true; }
		}

		/// <summary>The three Vorbis headers from Matroska's CodecPrivate (Xiph-laced: the count less one, two sizes, the data).</summary>
		private static List<byte[]> XiphHeaders(byte[] data)
		{
			List<byte[]> headers = new List<byte[]>();
			int count = data[0] + 1, pos = 1;
			int[] sizes = new int[count];
			for (int i = 0; i < count - 1; i++)
			{
				int s = 0, b;
				do { b = data[pos++]; s += b; } while (b == 255);
				sizes[i] = s;
			}
			int used = 0;
			for (int i = 0; i < count - 1; i++) used += sizes[i];
			sizes[count - 1] = data.Length - pos - used;
			foreach (int size in sizes)
			{
				byte[] h = new byte[size];
				Buffer.BlockCopy(data, pos, h, 0, size);
				headers.Add(h);
				pos += size;
			}
			return headers;
		}

		/// <summary>Vorbis packets for NVorbis's StreamDecoder, as Matroska's blocks give them.</summary>
		private sealed class PacketQueue : NVorbis.Contracts.IPacketProvider
		{
			private readonly Queue<byte[]> _queue = new Queue<byte[]>();
			public Func<bool> More;

			public void Add(byte[] packet) => _queue.Enqueue(packet);

			private bool Fill()
			{
				while (_queue.Count == 0)
					if (More == null || !More()) return false;
				return true;
			}

			public bool CanSeek => false;
			public int StreamSerial => 0;
			public IPacket GetNextPacket() => Fill() ? new Packet(_queue.Dequeue(), _queue.Count == 0 && !Peekable()) : null;
			public IPacket PeekNextPacket() => Fill() ? new Packet(_queue.Peek(), false) : null;
			public long SeekTo(long granulePos, int preRoll, GetPacketGranuleCount getPacketGranuleCount) => throw new NotSupportedException();
			public long GetGranuleCount() => throw new NotSupportedException();

			private bool Peekable() => Fill();
		}

		private sealed class Packet : DataPacket
		{
			private readonly byte[] _data;
			private int _at;

			public Packet(byte[] data, bool last)
			{
				_data = data;
				IsEndOfStream = last;
			}

			protected override int TotalBits => _data.Length * 8;
			protected override int ReadNextByte() => _at < _data.Length ? _data[_at++] : -1;
		}

		// ---- the game's thread ----

		/// <summary>Starts the clock once a picture and some sound are ready; feeds the sound; takes the picture now due.</summary>
		public void Update()
		{
			if (Done) return;
			if (!_clock.IsRunning)
			{
				bool soundReady = _audioDone || _pcm.Count >= 3;
				if (_frames.IsEmpty && !_videoDone) return;
				if (!soundReady) return;
				if (_sampleRate > 0 && _channels > 0)
				{
					try
					{
						_sound = new DynamicSoundEffectInstance(_sampleRate, _channels == 1 ? AudioChannels.Mono : AudioChannels.Stereo);
						_sound.Volume = Math.Clamp(GameOptions.Current.MusicLevel / 127f, 0f, 1f);
					}
					catch (Exception ex) { Error = "sound: " + ex.Message; _sound = null; }
				}
				FeedSound();
				_sound?.Play();
				_clock.Start();
			}
			FeedSound();
			long now = _clock.ElapsedMilliseconds;
			byte[] due = null;
			while (_frames.TryPeek(out var next) && next.TimeMs <= now)
			{
				_frames.TryDequeue(out next);
				if (due != null) Recycle(due);
				due = next.Rgba;
				_shownTimeMs = next.TimeMs;
			}
			if (due != null)
			{
				_texture ??= ModDraw.CreateDynamic(_width, _height);
				ModDraw.SetPixels(_texture, due);
				Recycle(due);
			}
			if (_videoDone && _frames.IsEmpty && now >= _lastTimeMs + _frameMs) Finish();
		}

		private void Recycle(byte[] rgba)
		{
			_spare.Add(rgba);
			try { _room.Release(); } catch (SemaphoreFullException) { }
		}

		private void FeedSound()
		{
			if (_sound == null) return;
			while (_sound.PendingBufferCount < 4 && _pcm.TryDequeue(out byte[] pcm)) _sound.SubmitBuffer(pcm);
		}

		/// <summary>The picture over black, as large as the window lets it be at its own shape.</summary>
		public void Draw(DrawList d)
		{
			d.Rect(0, 0, DrawList.ScreenWidth, DrawList.ScreenHeight, Color.Black);
			if (_texture == null || _shownTimeMs < 0 || _width == 0) return;
			// In the 800 x 480 space a window pixel is SquareX as wide as it is high (Ff4Ui.SquareX).
			float aspect = (float)_width / _height;
			float h = DrawList.ScreenHeight, w = h * aspect * Ff4Ui.SquareX;
			if (w > DrawList.ScreenWidth)
			{
				w = DrawList.ScreenWidth;
				h = w / (aspect * Ff4Ui.SquareX);
			}
			d.Sprite(_texture, (DrawList.ScreenWidth - w) / 2f, (DrawList.ScreenHeight - h) / 2f, w, h, null, 0f, 0, 0, _width, _height);
		}

		/// <summary>Ends it now (a skip): the sound stops, the threads let go.</summary>
		public void Stop() => Finish();

		private void Finish()
		{
			if (Done) return;
			Done = true;
			_stopping = true;
			try { _room.Release(QueuedFrames); } catch (SemaphoreFullException) { }
			try { _sound?.Stop(); } catch (Exception) { }
			if (Error != null) Log.Write(LogChannel.General, "movie: " + Error);
			Log.Write(LogChannel.General, "movie: done at " + _clock.ElapsedMilliseconds + " ms (picture " + _shownTimeMs + " of " + _lastTimeMs + " ms, " + (_videoDone ? "all decoded" : "decoding") + ")");
		}

		public void Dispose()
		{
			Finish();
			_videoThread.Join(2000);
			_audioThread.Join(2000);
			try { _sound?.Dispose(); } catch (Exception) { }
			_sound = null;
			ModDraw.Release(_texture);
			_texture = null;
			if (Current == this) Current = null;
		}
	}
}
