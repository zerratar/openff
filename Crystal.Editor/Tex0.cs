// TEX0 - the textures inside an NMDP package.
//
//   crystal tex <file.lz | directory> [out]
//
// Every .nmdp and .ntxp is an NMDP wrapper around a standard NitroSDK file - BMD0 for
// a model with its textures, BTX0 for textures alone - and the TEX0 block inside holds
// the pictures. 1126 of the 1589 packages have one; the rest are geometry with no
// textures of their own.
//
// None of this is guessed. The layout is the game's own NNSG3dResTex parser and the
// pixel conversion is its LoadTexture, format for format, down to the interpolation
// weights of the 4x4 blocks. Where that code does something surprising - alpha coming
// out at 248 rather than 255 for interpolated pixels - this does the same thing,
// because the point is to show what the game shows.
//
// Layout, from NNSG3dResTex:
//   +0   "TEX0", size
//   +8   texture info:  size >> 3, dictionary offset, flags, data offset
//   +24  4x4 info:      the same, plus the palette index data offset
//   +44  palette info:  size >> 3, flags, dictionary offset, data offset
//   dictionaries hold one entry per texture or palette, plus 16 byte names
//
// A texture's entry is its texImageParam:
//   bits 0-19   where its data starts, in 8 byte units
//   bits 20-22  width  = 8 << value
//   bits 23-25  height = 8 << value
//   bits 26-28  format
//   bit 29      whether palette entry 0 is transparent

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Crystal
{
	internal sealed class Tex0Texture
	{
		public string Name { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public int Format { get; set; }
		public string FormatName { get; set; }
		public string Palette { get; set; }
		public bool Transparent0 { get; set; }

		/// <summary>Set when this one cannot be decoded, with the reason.</summary>
		public string Problem { get; set; }

		internal uint Param;
		internal int PaletteOffset;
	}

	internal sealed class Tex0File
	{
		public List<Tex0Texture> Textures { get; } = new List<Tex0Texture>();

		internal byte[] Data;
		internal int At;
		internal int TexData;
		internal int Tex4x4Data;
		internal int Tex4x4Index;
		internal int PaletteData;
	}

	internal static class Tex0
	{
		private static readonly string[] FormatNames =
		{
			"none", "a3i5", "pal4", "pal16", "pal256", "4x4", "a5i3", "rgb555"
		};

		/// <summary>Where the TEX0 block is, or -1 if this package has none.</summary>
		public static int Find(byte[] data)
		{
			if (data == null || data.Length < 48 || data[0] != 'N' || data[1] != 'M'
				|| data[2] != 'D' || data[3] != 'P')
			{
				return -1;
			}

			int start = (int)ReadUInt32(data, 28);
			if (start < 0 || start + 16 > data.Length)
			{
				return -1;
			}

			string signature = Encoding.ASCII.GetString(data, start, 4);
			if (signature != "BMD0" && signature != "BTX0")
			{
				return -1;
			}

			int blocks = ReadUInt16(data, start + 14);
			for (int i = 0; i < blocks; i++)
			{
				int at = start + (int)ReadUInt32(data, start + 16 + i * 4);
				if (at >= 0 && at + 4 <= data.Length
					&& Encoding.ASCII.GetString(data, at, 4) == "TEX0")
				{
					return at;
				}
			}
			return -1;
		}

		public static Tex0File Read(byte[] data)
		{
			int at = Find(data);
			if (at < 0)
			{
				throw new InvalidDataException("no TEX0 block in this package");
			}

			Tex0File file = new Tex0File
			{
				Data = data,
				At = at,
				TexData = at + (int)ReadUInt32(data, at + 20),
				Tex4x4Data = at + (int)ReadUInt32(data, at + 36),
				Tex4x4Index = at + (int)ReadUInt32(data, at + 40),
				PaletteData = at + (int)ReadUInt32(data, at + 56)
			};

			List<(string Name, byte[] Entry)> textures = ReadDictionary(data, at + ReadUInt16(data, at + 14));
			List<(string Name, byte[] Entry)> palettes = ReadDictionary(data, at + ReadUInt16(data, at + 52));

			for (int i = 0; i < textures.Count; i++)
			{
				uint param = ReadUInt32(textures[i].Entry, 0);
				int format = (int)((param >> 26) & 7);

				Tex0Texture texture = new Tex0Texture
				{
					Name = textures[i].Name,
					Width = 8 << (int)((param >> 20) & 7),
					Height = 8 << (int)((param >> 23) & 7),
					Format = format,
					FormatName = FormatNames[format],
					Transparent0 = ((param >> 29) & 1) != 0,
					Param = param
				};

				// Palettes are named after their texture - sougen01 and sougen01_pl -
				// so that is tried first, then the entry in the same position, which is
				// right for the 1111 files of 1126 that have one of each.
				int palette = palettes.FindIndex(p =>
					string.Equals(p.Name, texture.Name + "_pl", StringComparison.Ordinal));
				if (palette < 0 && i < palettes.Count)
				{
					palette = i;
				}
				if (palette < 0 && palettes.Count > 0)
				{
					palette = 0;
				}

				if (palette >= 0)
				{
					texture.Palette = palettes[palette].Name;
					texture.PaletteOffset = ReadUInt16(palettes[palette].Entry, 0) << 3;
				}
				else if (format != 7)
				{
					texture.Problem = "no palette in this package";
				}

				file.Textures.Add(texture);
			}

			return file;
		}

		/// <summary>NNSG3dResDict: entries of a fixed size, then 16 byte names.</summary>
		private static List<(string, byte[])> ReadDictionary(byte[] data, int at)
		{
			int count = data[at + 1];
			int entries = at + ReadUInt16(data, at + 6);
			int unit = ReadUInt16(data, entries);
			int names = entries + ReadUInt16(data, entries + 2);
			int values = entries + 4;

			List<(string, byte[])> list = new List<(string, byte[])>(count);
			for (int i = 0; i < count; i++)
			{
				byte[] entry = new byte[unit];
				Buffer.BlockCopy(data, values + unit * i, entry, 0, unit);

				list.Add((ReadName(data, names + 16 * i), entry));
			}
			return list;
		}

		/// <summary>
		/// A 16 byte name. Most are ASCII, but a couple were typed with a Japanese
		/// keyboard still in wide mode - n211 has a fullwidth n - so these are read
		/// through the game's own Shift-JIS table, the same one the text uses.
		/// </summary>
		private static string ReadName(byte[] data, int at)
		{
			byte[] name = new byte[17];
			Buffer.BlockCopy(data, at, name, 0, Math.Min(16, data.Length - at));

			try
			{
				return ShiftJis.Decode(name, 0, out int _).Trim();
			}
			catch (NotSupportedException)
			{
				int stop = 0;
				while (stop < 16 && name[stop] != 0)
				{
					stop++;
				}
				return Encoding.ASCII.GetString(name, 0, stop).Trim();
			}
		}

		/// <summary>
		/// One texture as RGBA. This is GlobalScope.LoadTexture, format for format -
		/// the game's own conversion, so what comes out is what the game draws.
		/// </summary>
		public static byte[] Decode(Tex0File file, Tex0Texture texture)
		{
			byte[] data = file.Data;
			int width = texture.Width;
			int height = texture.Height;
			byte[] pixels = new byte[width * height * 4];

			int address = (int)(texture.Param & 0xFFFFF) << 3;
			int texel = file.TexData + address;
			int palette = file.PaletteData + texture.PaletteOffset;
			int at = 0;

			switch (texture.Format)
			{
				case 1:                              // a3i5: 3 bits of alpha, 5 of index
					for (int i = 0; i < width * height; i++, at += 4)
					{
						int value = data[texel + i];
						int colour = ReadUInt16(data, palette + (value & 0x1F) * 2);
						Rgb(pixels, at, colour);
						pixels[at + 3] = (byte)((value >> 5) * 255 / 7);
					}
					break;

				case 2:                              // four colours
					for (int i = 0; i < width * height; i++, at += 4)
					{
						int index = (data[texel + (i >> 2)] >> ((i & 3) * 2)) & 3;
						Rgb(pixels, at, ReadUInt16(data, palette + index * 2));
						pixels[at + 3] = Opaque(index, texture.Transparent0);
					}
					break;

				case 3:                              // sixteen colours
					for (int i = 0; i < width * height; i++, at += 4)
					{
						int index = (data[texel + (i >> 1)] >> ((i & 1) * 4)) & 0xF;
						Rgb(pixels, at, ReadUInt16(data, palette + index * 2));
						pixels[at + 3] = Opaque(index, texture.Transparent0);
					}
					break;

				case 4:                              // 256 colours
					// This one alone scales to a full 255 rather than shifting up to
					// 248. That is what the game does, so it is what happens here.
					for (int i = 0; i < width * height; i++, at += 4)
					{
						int index = data[texel + i];
						int colour = ReadUInt16(data, palette + index * 2);
						pixels[at] = (byte)((colour & 0x1F) * 255 / 31);
						pixels[at + 1] = (byte)(((colour >> 5) & 0x1F) * 255 / 31);
						pixels[at + 2] = (byte)(((colour >> 10) & 0x1F) * 255 / 31);
						pixels[at + 3] = Opaque(index, texture.Transparent0);
					}
					break;

				case 5:
					Decode4x4(file, texture, pixels);
					break;

				case 6:                              // a5i3: 5 bits of alpha, 3 of index
					for (int i = 0; i < width * height; i++, at += 4)
					{
						int value = data[texel + i];
						Rgb(pixels, at, ReadUInt16(data, palette + (value & 7) * 2));
						pixels[at + 3] = (byte)((value >> 3) * 255 / 31);
					}
					break;

				case 7:                              // straight 15 bit colour
					for (int i = 0; i < width * height; i++, at += 4)
					{
						int colour = ReadUInt16(data, texel + i * 2);
						Rgb(pixels, at, colour);
						pixels[at + 3] = (byte)((colour & 0x8000) != 0 ? 255 : 0);
					}
					break;

				default:
					throw new InvalidDataException("format " + texture.Format + " holds no pixels");
			}

			return pixels;
		}

		/// <summary>
		/// The compressed format: each 4x4 block names two or four colours and packs
		/// sixteen two bit indices. The interpolation weights are eighths, and the
		/// game's own arithmetic is kept - including alpha landing on 248.
		/// </summary>
		private static void Decode4x4(Tex0File file, Tex0Texture texture, byte[] pixels)
		{
			byte[] data = file.Data;
			int width = texture.Width;
			int height = texture.Height;
			int address = (int)(texture.Param & 0xFFFFF) << 3;

			int blocks = file.Tex4x4Data + address;
			int indices = file.Tex4x4Index + (address >> 1);
			int palette = file.PaletteData + texture.PaletteOffset;

			int[] weights = { 8, 0, 0, 8, 4, 4, 0, 0, 8, 0, 0, 8, 5, 3, 3, 5 };
			byte[] block = new byte[16];
			int n = 0;

			for (int by = 0; by < height >> 2; by++)
			{
				for (int bx = 0; bx < width >> 2; bx++, n++)
				{
					int control = ReadUInt16(data, indices + n * 2);
					uint texels = ReadUInt32(data, blocks + n * 4);
					int colours = palette + ((control & 0xFFF) << 1) * 2;
					int at = 0;

					if ((control & 0x4000) == 0)
					{
						for (int i = 0; i < 4; i++, at += 4)
						{
							if ((control & 0x8000) == 0 && i == 3)
							{
								block[at] = block[at + 1] = block[at + 2] = block[at + 3] = 0;
							}
							else
							{
								int colour = ReadUInt16(data, colours + i * 2);
								block[at] = (byte)(colour << 3);
								block[at + 1] = (byte)((colour >> 5) << 3);
								block[at + 2] = (byte)((colour >> 10) << 3);
								block[at + 3] = 255;
							}
						}
					}
					else
					{
						int first = ReadUInt16(data, colours);
						int second = ReadUInt16(data, colours + 2);
						int weight = (control & 0x8000) >> 12;

						for (int i = 0; i < 4; i++, weight += 2, at += 4)
						{
							int a = weights[weight];
							int b = weights[weight + 1];
							block[at] = (byte)((first & 0x1F) * a + (second & 0x1F) * b);
							block[at + 1] = (byte)(((first >> 5) & 0x1F) * a + ((second >> 5) & 0x1F) * b);
							block[at + 2] = (byte)(((first >> 10) & 0x1F) * a + ((second >> 10) & 0x1F) * b);
							block[at + 3] = (byte)(31 * a + 31 * b);
						}
					}

					for (int y = 0; y < 4; y++)
					{
						int row = (((by << 2) + y) * width + (bx << 2)) * 4;
						for (int x = 0; x < 4; x++, row += 4)
						{
							int pick = (int)(texels & 3) * 4;
							pixels[row] = block[pick];
							pixels[row + 1] = block[pick + 1];
							pixels[row + 2] = block[pick + 2];
							pixels[row + 3] = block[pick + 3];
							texels >>= 2;
						}
					}
				}
			}
		}

		private static void Rgb(byte[] pixels, int at, int colour)
		{
			pixels[at] = (byte)((colour & 0x1F) << 3);
			pixels[at + 1] = (byte)(((colour >> 5) & 0x1F) << 3);
			pixels[at + 2] = (byte)(((colour >> 10) & 0x1F) << 3);
		}

		private static byte Opaque(int index, bool transparent0)
		{
			return (byte)(index != 0 || !transparent0 ? 255 : 0);
		}

		private static ushort ReadUInt16(byte[] data, int at)
		{
			return (ushort)(data[at] | (data[at + 1] << 8));
		}

		private static uint ReadUInt32(byte[] data, int at)
		{
			return (uint)(data[at] | (data[at + 1] << 8) | (data[at + 2] << 16) | (data[at + 3] << 24));
		}
	}
}
