// Reader for XNA 4.0 .xnb content files.
//
// The FF3 content is all "XNBm" (Windows Phone), version 5, uncompressed, so this
// handles that shape and reports anything else rather than guessing. Only the two
// reader types the game actually ships are decoded: SpriteFont and SoundEffect.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FF3.ContentTool
{
	internal sealed class XnbHeader
	{
		public char Platform;
		public byte Version;
		public byte Flags;
		public bool Compressed => (Flags & 0x80) != 0;
		public bool HiDef => (Flags & 0x01) != 0;
		public long FileSize;
		public List<string> ReaderNames = new List<string>();
		public int SharedResourceCount;
	}

	internal sealed class XnbTexture
	{
		public int SurfaceFormat;
		public int Width;
		public int Height;
		public List<byte[]> Mips = new List<byte[]>();
	}

	internal sealed class XnbSpriteFont
	{
		public XnbTexture Texture;
		public List<int[]> Glyphs = new List<int[]>();     // x, y, w, h
		public List<int[]> Cropping = new List<int[]>();
		public List<char> Characters = new List<char>();
		public int LineSpacing;
		public float Spacing;
		public List<float[]> Kerning = new List<float[]>(); // a, b, c
		public char? DefaultCharacter;
	}

	internal sealed class XnbSoundEffect
	{
		public byte[] Format;      // WAVEFORMATEX
		public byte[] Data;
		public int LoopStart;
		public int LoopLength;
		public int DurationMs;

		public int Channels => Format.Length >= 4 ? BitConverter.ToInt16(Format, 2) : 0;
		public int SampleRate => Format.Length >= 8 ? BitConverter.ToInt32(Format, 4) : 0;
		public int FormatTag => Format.Length >= 2 ? BitConverter.ToInt16(Format, 0) : 0;
	}

	internal sealed class Xnb
	{
		private readonly BinaryReader _reader;

		public XnbHeader Header { get; } = new XnbHeader();

		private Xnb(BinaryReader reader)
		{
			_reader = reader;
		}

		public static Xnb Open(string path)
		{
			BinaryReader reader = new BinaryReader(File.OpenRead(path), Encoding.UTF8);
			Xnb xnb = new Xnb(reader);
			xnb.ReadHeader(path);
			return xnb;
		}

		public void Close() => _reader.Dispose();

		private void ReadHeader(string path)
		{
			if (_reader.ReadByte() != 'X' || _reader.ReadByte() != 'N' || _reader.ReadByte() != 'B')
			{
				throw new InvalidDataException(Path.GetFileName(path) + ": not an XNB file");
			}
			Header.Platform = (char)_reader.ReadByte();
			Header.Version = _reader.ReadByte();
			Header.Flags = _reader.ReadByte();
			Header.FileSize = _reader.ReadUInt32();

			if (Header.Version != 5 && Header.Version != 4)
			{
				throw new InvalidDataException(Path.GetFileName(path)
					+ ": unsupported XNB version " + Header.Version);
			}
			if (Header.Compressed)
			{
				throw new NotSupportedException(Path.GetFileName(path)
					+ ": LZX-compressed XNB, decompression not implemented");
			}

			int readerCount = Read7BitEncodedInt();
			for (int i = 0; i < readerCount; i++)
			{
				Header.ReaderNames.Add(ReadString());
				_reader.ReadInt32();   // reader version
			}
			Header.SharedResourceCount = Read7BitEncodedInt();
		}

		/// <summary>Short name of the reader for the primary object, e.g. "SpriteFontReader".</summary>
		public string PrimaryReaderName
		{
			get
			{
				if (Header.ReaderNames.Count == 0)
				{
					return "<none>";
				}
				string full = Header.ReaderNames[0];
				int comma = full.IndexOf(',');
				string name = comma >= 0 ? full.Substring(0, comma) : full;
				int dot = name.LastIndexOf('.');
				return dot >= 0 ? name.Substring(dot + 1) : name;
			}
		}

		private int Read7BitEncodedInt()
		{
			int result = 0, shift = 0;
			while (true)
			{
				byte b = _reader.ReadByte();
				result |= (b & 0x7F) << shift;
				if ((b & 0x80) == 0)
				{
					return result;
				}
				shift += 7;
			}
		}

		private string ReadString()
		{
			int length = Read7BitEncodedInt();
			return Encoding.UTF8.GetString(_reader.ReadBytes(length));
		}

		/// <summary>Reads the type-id prefix that precedes every reference-typed object.</summary>
		private int ReadTypeId() => Read7BitEncodedInt();

		public XnbSoundEffect ReadSoundEffect()
		{
			ReadTypeId();
			XnbSoundEffect sound = new XnbSoundEffect();
			int formatSize = _reader.ReadInt32();
			sound.Format = _reader.ReadBytes(formatSize);
			int dataSize = _reader.ReadInt32();
			sound.Data = _reader.ReadBytes(dataSize);
			sound.LoopStart = _reader.ReadInt32();
			sound.LoopLength = _reader.ReadInt32();
			sound.DurationMs = _reader.ReadInt32();
			return sound;
		}

		public XnbSpriteFont ReadSpriteFont()
		{
			ReadTypeId();
			XnbSpriteFont font = new XnbSpriteFont();
			font.Texture = ReadTexture2D();

			font.Glyphs = ReadRectangleList();
			font.Cropping = ReadRectangleList();

			ReadTypeId();
			int charCount = _reader.ReadInt32();
			for (int i = 0; i < charCount; i++)
			{
				font.Characters.Add(_reader.ReadChar());
			}

			font.LineSpacing = _reader.ReadInt32();
			font.Spacing = _reader.ReadSingle();

			ReadTypeId();
			int kerningCount = _reader.ReadInt32();
			for (int i = 0; i < kerningCount; i++)
			{
				font.Kerning.Add(new[]
				{
					_reader.ReadSingle(), _reader.ReadSingle(), _reader.ReadSingle()
				});
			}

			if (_reader.BaseStream.Position < _reader.BaseStream.Length && _reader.ReadBoolean())
			{
				font.DefaultCharacter = _reader.ReadChar();
			}
			return font;
		}

		private List<int[]> ReadRectangleList()
		{
			ReadTypeId();
			int count = _reader.ReadInt32();
			List<int[]> list = new List<int[]>(count);
			for (int i = 0; i < count; i++)
			{
				list.Add(new[]
				{
					_reader.ReadInt32(), _reader.ReadInt32(),
					_reader.ReadInt32(), _reader.ReadInt32()
				});
			}
			return list;
		}

		public XnbTexture ReadTexture2D()
		{
			ReadTypeId();
			XnbTexture texture = new XnbTexture
			{
				SurfaceFormat = _reader.ReadInt32(),
				Width = (int)_reader.ReadUInt32(),
				Height = (int)_reader.ReadUInt32()
			};
			int levels = (int)_reader.ReadUInt32();
			for (int i = 0; i < levels; i++)
			{
				int size = (int)_reader.ReadUInt32();
				texture.Mips.Add(_reader.ReadBytes(size));
			}
			return texture;
		}
	}
}
