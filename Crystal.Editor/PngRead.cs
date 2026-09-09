// Reading a PNG into RGBA - the pictures a glTF carries for its textures, on the way into
// a texture package of the game's own (Mdl0Write). The browser decodes the PNGs the editor's
// pages send; this is for the converter, which runs on the server and at the command line.
// Non-interlaced PNGs of every colour type at 8 bits (16 keeps the high byte; 1, 2 and 4
// bit greys and palettes are unpacked), the five filters, a tRNS chunk for a palette's or a
// grey's transparency. Interlaced files are refused with the reason.

using System;
using System.IO;
using System.IO.Compression;

namespace Crystal
{
	internal static class PngRead
	{
		/// <summary>The picture's pixels as RGBA, row by row from the top. Throws with the reason on anything it does not read.</summary>
		public static byte[] Decode(byte[] png, out int width, out int height)
		{
			if (png == null || png.Length < 33 || png[0] != 0x89 || png[1] != 'P' || png[2] != 'N' || png[3] != 'G')
				throw new InvalidDataException("not a PNG");
			width = height = 0;
			int depth = 0, colourType = 0, interlace = 0;
			byte[] palette = null, trns = null;
			using MemoryStream idat = new MemoryStream();
			int at = 8;
			while (at + 8 <= png.Length)
			{
				int length = (png[at] << 24) | (png[at + 1] << 16) | (png[at + 2] << 8) | png[at + 3];
				string type = System.Text.Encoding.ASCII.GetString(png, at + 4, 4);
				int data = at + 8;
				if (length < 0 || data + length > png.Length) throw new InvalidDataException("a PNG chunk runs past the end");
				switch (type)
				{
					case "IHDR":
						width = (png[data] << 24) | (png[data + 1] << 16) | (png[data + 2] << 8) | png[data + 3];
						height = (png[data + 4] << 24) | (png[data + 5] << 16) | (png[data + 6] << 8) | png[data + 7];
						depth = png[data + 8]; colourType = png[data + 9]; interlace = png[data + 12];
						break;
					case "PLTE": palette = new byte[length]; Array.Copy(png, data, palette, 0, length); break;
					case "tRNS": trns = new byte[length]; Array.Copy(png, data, trns, 0, length); break;
					case "IDAT": idat.Write(png, data, length); break;
				}
				if (type == "IEND") break;
				at = data + length + 4;
			}
			if (width <= 0 || height <= 0) throw new InvalidDataException("a PNG with no size");
			if (interlace != 0) throw new InvalidDataException("an interlaced PNG - save it without interlacing");
			int channels = colourType switch { 0 => 1, 2 => 3, 3 => 1, 4 => 2, 6 => 4, _ => throw new InvalidDataException("PNG colour type " + colourType) };
			if (depth != 8 && depth != 16 && !(colourType == 0 || colourType == 3) && depth != 1 && depth != 2 && depth != 4)
				throw new InvalidDataException("PNG bit depth " + depth);
			int bitsPerPixel = channels * depth;
			int stride = (width * bitsPerPixel + 7) / 8;
			int bpp = Math.Max(1, bitsPerPixel / 8);

			byte[] raw = new byte[(stride + 1) * height];
			idat.Position = 0;
			using (ZLibStream inflate = new ZLibStream(idat, CompressionMode.Decompress))
			{
				int read = 0;
				while (read < raw.Length)
				{
					int n = inflate.Read(raw, read, raw.Length - read);
					if (n <= 0) break;
					read += n;
				}
				if (read < raw.Length) throw new InvalidDataException("the PNG's image data ends early");
			}

			// Unfilter in place, row by row.
			byte[] previous = new byte[stride];
			byte[] row = new byte[stride];
			byte[] rgba = new byte[width * height * 4];
			for (int y = 0; y < height; y++)
			{
				int filter = raw[y * (stride + 1)];
				Array.Copy(raw, y * (stride + 1) + 1, row, 0, stride);
				for (int i = 0; i < stride; i++)
				{
					int a = i >= bpp ? row[i - bpp] : 0, b = previous[i], c = i >= bpp ? previous[i - bpp] : 0;
					switch (filter)
					{
						case 1: row[i] = (byte)(row[i] + a); break;
						case 2: row[i] = (byte)(row[i] + b); break;
						case 3: row[i] = (byte)(row[i] + ((a + b) >> 1)); break;
						case 4:
						{
							int p = a + b - c, pa = Math.Abs(p - a), pb = Math.Abs(p - b), pc = Math.Abs(p - c);
							row[i] = (byte)(row[i] + (pa <= pb && pa <= pc ? a : pb <= pc ? b : c));
							break;
						}
					}
				}
				for (int x = 0; x < width; x++)
				{
					int o = (y * width + x) * 4;
					int r, g, bl, al = 255;
					switch (colourType)
					{
						case 0:
						{
							int v = Sample(row, x, depth, 0, 1);
							int max = (1 << Math.Min(depth, 16)) - 1;
							r = g = bl = depth == 16 ? v >> 8 : v * 255 / max;
							if (trns != null && trns.Length >= 2 && v == ((trns[0] << 8) | trns[1])) al = 0;
							break;
						}
						case 2:
							r = Sample(row, x, depth, 0, 3); g = Sample(row, x, depth, 1, 3); bl = Sample(row, x, depth, 2, 3);
							if (depth == 16) { r >>= 8; g >>= 8; bl >>= 8; }
							break;
						case 3:
						{
							int index = Sample(row, x, depth, 0, 1);
							if (palette == null || index * 3 + 2 >= palette.Length) throw new InvalidDataException("a PNG palette index with no palette");
							r = palette[index * 3]; g = palette[index * 3 + 1]; bl = palette[index * 3 + 2];
							if (trns != null && index < trns.Length) al = trns[index];
							break;
						}
						case 4:
							r = g = bl = Sample(row, x, depth, 0, 2); al = Sample(row, x, depth, 1, 2);
							if (depth == 16) { r >>= 8; g = bl = r; al >>= 8; }
							break;
						default:
							r = Sample(row, x, depth, 0, 4); g = Sample(row, x, depth, 1, 4); bl = Sample(row, x, depth, 2, 4); al = Sample(row, x, depth, 3, 4);
							if (depth == 16) { r >>= 8; g >>= 8; bl >>= 8; al >>= 8; }
							break;
					}
					rgba[o] = (byte)r; rgba[o + 1] = (byte)g; rgba[o + 2] = (byte)bl; rgba[o + 3] = (byte)al;
				}
				Array.Copy(row, previous, stride);
			}
			return rgba;
		}

		/// <summary>Sample `channel` of pixel `x` in a row of `channels`-channel pixels at `depth` bits.</summary>
		private static int Sample(byte[] row, int x, int depth, int channel, int channels)
		{
			switch (depth)
			{
				case 8: return row[x * channels + channel];
				case 16: { int i = (x * channels + channel) * 2; return (row[i] << 8) | row[i + 1]; }
				default:
				{
					int bit = (x * channels + channel) * depth;
					int shift = 8 - depth - (bit & 7);
					return (row[bit >> 3] >> shift) & ((1 << depth) - 1);
				}
			}
		}
	}
}
