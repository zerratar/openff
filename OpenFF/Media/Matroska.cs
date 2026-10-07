// A Matroska (.mkv / WebM) reader, as much of one as a movie needs: the tracks (number, kind, codec, its private data, the
// picture's size or the sound's rate and channels) and then the blocks in file order - each a track's frames (unlaced
// from Xiph, fixed or EBML lacing) with their time in milliseconds. FF4's opening.mkv is VP8 and Vorbis in SimpleBlocks.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenFF.Client
{
	internal sealed class Matroska : IDisposable
	{
		public sealed class Track
		{
			public int Number, Type;   // Type 1 video, 2 audio
			public string Codec = "";
			public byte[] CodecPrivate;
			public long DefaultDurationNs;
			public int Width, Height, Channels = 1;
			public double SampleRate = 8000;
		}

		public struct Block
		{
			public int Track;
			public long TimeMs;
			public bool Keyframe;
			public List<byte[]> Frames;
		}

		private const uint IdEbml = 0x1A45DFA3, IdSegment = 0x18538067, IdInfo = 0x1549A966, IdTimecodeScale = 0x2AD7B1, IdDuration = 0x4489;
		private const uint IdTracks = 0x1654AE6B, IdTrackEntry = 0xAE, IdTrackNumber = 0xD7, IdTrackType = 0x83, IdCodecId = 0x86, IdCodecPrivate = 0x63A2;
		private const uint IdDefaultDuration = 0x23E383, IdVideo = 0xE0, IdPixelWidth = 0xB0, IdPixelHeight = 0xBA, IdAudio = 0xE1, IdSamplingFrequency = 0xB5, IdChannels = 0x9F;
		private const uint IdCluster = 0x1F43B675, IdTimecode = 0xE7, IdSimpleBlock = 0xA3, IdBlockGroup = 0xA0, IdBlock = 0xA1;

		private readonly FileStream _file;
		private readonly long _segmentStart, _segmentEnd;
		private long _timecodeScale = 1000000;

		public readonly List<Track> Tracks = new List<Track>();
		public double DurationMs { get; private set; }

		public Matroska(string path)
		{
			_file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 16);
			long end = _file.Length;
			while (_file.Position < end)
			{
				uint id = ReadId();
				long size = ReadSize(out bool unknown);
				long body = _file.Position;
				if (id == IdSegment)
				{
					_segmentStart = body;
					_segmentEnd = unknown ? end : Math.Min(end, body + size);
					break;
				}
				_file.Position = body + size;
			}
			if (_segmentEnd == 0) throw new InvalidDataException("no Matroska segment");
			// The segment's head: Info and Tracks, up to the first cluster.
			_file.Position = _segmentStart;
			while (_file.Position < _segmentEnd)
			{
				long at = _file.Position;
				uint id = ReadId();
				long size = ReadSize(out bool unknown);
				long body = _file.Position;
				if (id == IdCluster) { _file.Position = at; break; }
				if (id == IdInfo) ReadInfo(body + size);
				else if (id == IdTracks) ReadTracks(body + size);
				_file.Position = unknown ? body : body + size;
			}
			_clustersStart = _file.Position;
		}

		private readonly long _clustersStart;

		private void ReadInfo(long end)
		{
			double duration = 0;
			while (_file.Position < end)
			{
				uint id = ReadId();
				long size = ReadSize(out _);
				long body = _file.Position;
				if (id == IdTimecodeScale) _timecodeScale = (long)ReadUint(size);
				else if (id == IdDuration) duration = ReadFloat(size);
				_file.Position = body + size;
			}
			DurationMs = duration * _timecodeScale / 1e6;
		}

		private void ReadTracks(long end)
		{
			while (_file.Position < end)
			{
				uint id = ReadId();
				long size = ReadSize(out _);
				long body = _file.Position;
				if (id == IdTrackEntry)
				{
					Track t = new Track();
					while (_file.Position < body + size)
					{
						uint eid = ReadId();
						long esize = ReadSize(out _);
						long ebody = _file.Position;
						switch (eid)
						{
							case IdTrackNumber: t.Number = (int)ReadUint(esize); break;
							case IdTrackType: t.Type = (int)ReadUint(esize); break;
							case IdCodecId: t.Codec = Encoding.ASCII.GetString(ReadBytes(esize)).TrimEnd('\0'); break;
							case IdCodecPrivate: t.CodecPrivate = ReadBytes(esize); break;
							case IdDefaultDuration: t.DefaultDurationNs = (long)ReadUint(esize); break;
							case IdVideo:
							case IdAudio:
								while (_file.Position < ebody + esize)
								{
									uint vid = ReadId();
									long vsize = ReadSize(out _);
									long vbody = _file.Position;
									if (vid == IdPixelWidth) t.Width = (int)ReadUint(vsize);
									else if (vid == IdPixelHeight) t.Height = (int)ReadUint(vsize);
									else if (vid == IdSamplingFrequency) t.SampleRate = ReadFloat(vsize);
									else if (vid == IdChannels) t.Channels = (int)ReadUint(vsize);
									_file.Position = vbody + vsize;
								}
								break;
						}
						_file.Position = ebody + esize;
					}
					Tracks.Add(t);
				}
				_file.Position = body + size;
			}
		}

		/// <summary>The blocks of the tracks wanted (null: all), in file order. A block of another track is passed over unread.</summary>
		public IEnumerable<Block> Blocks(Func<int, bool> wanted = null)
		{
			_file.Position = _clustersStart;
			long clusterTime = 0;
			while (_file.Position < _segmentEnd)
			{
				uint id;
				long size, body;
				bool unknown;
				try
				{
					id = ReadId();
					size = ReadSize(out unknown);
					body = _file.Position;
				}
				catch (EndOfStreamException) { yield break; }
				if (id == IdCluster)
				{
					// A cluster's children are read in place: an unknown size runs to the next cluster.
					continue;
				}
				if (id == IdTimecode)
				{
					clusterTime = (long)ReadUint(size);
					_file.Position = body + size;
					continue;
				}
				if (id == IdBlockGroup)
				{
					long end = body + size;
					while (_file.Position < end)
					{
						uint gid = ReadId();
						long gsize = ReadSize(out _);
						long gbody = _file.Position;
						if (gid == IdBlock)
						{
							Block? block = ReadBlock(gsize, clusterTime, false, wanted);
							if (block.HasValue) yield return block.Value;
						}
						_file.Position = gbody + gsize;
					}
					_file.Position = end;
					continue;
				}
				if (id == IdSimpleBlock)
				{
					Block? block = ReadBlock(size, clusterTime, true, wanted);
					_file.Position = body + size;
					if (block.HasValue) yield return block.Value;
					continue;
				}
				_file.Position = unknown ? _segmentEnd : body + size;
			}
		}

		private Block? ReadBlock(long size, long clusterTime, bool simple, Func<int, bool> wanted)
		{
			long start = _file.Position;
			int track = (int)ReadVint(out int trackLength, false);
			if (wanted != null && !wanted(track)) return null;
			int b0 = _file.ReadByte(), b1 = _file.ReadByte(), flags = _file.ReadByte();
			short relative = (short)((b0 << 8) | b1);
			int header = trackLength + 3;
			byte[] data = ReadBytes(size - header);
			Block block = new Block
			{
				Track = track,
				TimeMs = (clusterTime + relative) * _timecodeScale / 1000000,
				Keyframe = simple && (flags & 0x80) != 0,
				Frames = Unlace(data, (flags >> 1) & 3)
			};
			return block;
		}

		private static List<byte[]> Unlace(byte[] data, int lacing)
		{
			List<byte[]> frames = new List<byte[]>();
			if (lacing == 0)
			{
				frames.Add(data);
				return frames;
			}
			int count = data[0] + 1, pos = 1;
			int[] sizes = new int[count];
			if (lacing == 1)
			{
				for (int i = 0; i < count - 1; i++)
				{
					int s = 0, b;
					do { b = data[pos++]; s += b; } while (b == 255);
					sizes[i] = s;
				}
			}
			else if (lacing == 3)
			{
				sizes[0] = (int)VintAt(data, ref pos, out _);
				for (int i = 1; i < count - 1; i++)
				{
					long raw = VintAt(data, ref pos, out int length);
					long bias = (1L << (7 * length - 1)) - 1;
					sizes[i] = sizes[i - 1] + (int)(raw - bias);
				}
			}
			int used = 0;
			if (lacing == 2)
			{
				int each = (data.Length - pos) / count;
				for (int i = 0; i < count; i++) sizes[i] = each;
			}
			else
			{
				for (int i = 0; i < count - 1; i++) used += sizes[i];
				sizes[count - 1] = data.Length - pos - used;
			}
			for (int i = 0; i < count; i++)
			{
				byte[] f = new byte[Math.Max(0, sizes[i])];
				Buffer.BlockCopy(data, pos, f, 0, f.Length);
				frames.Add(f);
				pos += f.Length;
			}
			return frames;
		}

		private static long VintAt(byte[] data, ref int pos, out int length)
		{
			int first = data[pos];
			length = 1;
			int mask = 0x80;
			while (length <= 8 && (first & mask) == 0) { length++; mask >>= 1; }
			long value = first & (mask - 1);
			for (int i = 1; i < length; i++) value = (value << 8) | data[pos + i];
			pos += length;
			return value;
		}

		// ---- EBML ----

		private long ReadVint(out int length, bool keepMarker)
		{
			int first = _file.ReadByte();
			if (first < 0) throw new EndOfStreamException();
			length = 1;
			int mask = 0x80;
			while (length <= 8 && (first & mask) == 0) { length++; mask >>= 1; }
			long value = keepMarker ? first : first & (mask - 1);
			for (int i = 1; i < length; i++)
			{
				int b = _file.ReadByte();
				if (b < 0) throw new EndOfStreamException();
				value = (value << 8) | (uint)b;
			}
			return value;
		}

		private uint ReadId() => (uint)ReadVint(out _, true);

		private long ReadSize(out bool unknown)
		{
			long size = ReadVint(out int length, false);
			unknown = size == (1L << (7 * length)) - 1;
			return unknown ? 0 : size;
		}

		private ulong ReadUint(long size)
		{
			ulong v = 0;
			for (long i = 0; i < size; i++) v = (v << 8) | (uint)_file.ReadByte();
			return v;
		}

		private double ReadFloat(long size)
		{
			byte[] b = ReadBytes(size);
			if (BitConverter.IsLittleEndian) Array.Reverse(b);
			return size == 4 ? BitConverter.ToSingle(b, 0) : size == 8 ? BitConverter.ToDouble(b, 0) : 0;
		}

		private byte[] ReadBytes(long size)
		{
			byte[] b = new byte[size];
			int read = 0;
			while (read < size)
			{
				int n = _file.Read(b, read, (int)size - read);
				if (n <= 0) throw new EndOfStreamException();
				read += n;
			}
			return b;
		}

		public void Dispose() => _file.Dispose();
	}
}
