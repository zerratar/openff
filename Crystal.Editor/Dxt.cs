// DXT1 / DXT3 / DXT5 block decompression to RGBA8888.
//
// The shipped SpriteFont atlases are DXT3, so this is needed to get the glyph
// pages back out as editable images.

using System;

namespace Crystal
{
	internal static class Dxt
	{
		public static byte[] Decode(int format, int width, int height, byte[] data)
		{
			int blockBytes = format == SurfaceFormats.Dxt1 ? 8 : 16;
			int blocksX = Math.Max(1, (width + 3) / 4);
			int blocksY = Math.Max(1, (height + 3) / 4);
			byte[] rgba = new byte[width * height * 4];
			byte[] alpha = new byte[16];
			byte[] colours = new byte[16 * 4];

			int offset = 0;
			for (int by = 0; by < blocksY; by++)
			{
				for (int bx = 0; bx < blocksX; bx++, offset += blockBytes)
				{
					if (offset + blockBytes > data.Length)
					{
						return rgba;
					}

					int colourOffset = offset;
					if (format == SurfaceFormats.Dxt3)
					{
						DecodeAlpha4(data, offset, alpha);
						colourOffset = offset + 8;
					}
					else if (format == SurfaceFormats.Dxt5)
					{
						DecodeAlpha8(data, offset, alpha);
						colourOffset = offset + 8;
					}
					else
					{
						for (int i = 0; i < 16; i++)
						{
							alpha[i] = 255;
						}
					}

					DecodeColour(data, colourOffset, colours, format == SurfaceFormats.Dxt1, alpha);

					for (int py = 0; py < 4; py++)
					{
						int y = by * 4 + py;
						if (y >= height)
						{
							break;
						}
						for (int px = 0; px < 4; px++)
						{
							int x = bx * 4 + px;
							if (x >= width)
							{
								break;
							}
							int src = (py * 4 + px) * 4;
							int dst = (y * width + x) * 4;
							rgba[dst] = colours[src];
							rgba[dst + 1] = colours[src + 1];
							rgba[dst + 2] = colours[src + 2];
							rgba[dst + 3] = colours[src + 3];
						}
					}
				}
			}
			return rgba;
		}

		/// <summary>DXT3: 16 explicit 4-bit alpha values.</summary>
		private static void DecodeAlpha4(byte[] data, int offset, byte[] alpha)
		{
			for (int i = 0; i < 8; i++)
			{
				byte packed = data[offset + i];
				int low = packed & 0x0F;
				int high = (packed >> 4) & 0x0F;
				alpha[i * 2] = (byte)((low << 4) | low);
				alpha[i * 2 + 1] = (byte)((high << 4) | high);
			}
		}

		/// <summary>DXT5: two endpoints plus 3-bit interpolation indices.</summary>
		private static void DecodeAlpha8(byte[] data, int offset, byte[] alpha)
		{
			int a0 = data[offset];
			int a1 = data[offset + 1];
			int[] table = new int[8];
			table[0] = a0;
			table[1] = a1;
			if (a0 > a1)
			{
				for (int i = 0; i < 6; i++)
				{
					table[i + 2] = ((6 - i) * a0 + (1 + i) * a1) / 7;
				}
			}
			else
			{
				for (int i = 0; i < 4; i++)
				{
					table[i + 2] = ((4 - i) * a0 + (1 + i) * a1) / 5;
				}
				table[6] = 0;
				table[7] = 255;
			}

			ulong bits = 0;
			for (int i = 0; i < 6; i++)
			{
				bits |= (ulong)data[offset + 2 + i] << (8 * i);
			}
			for (int i = 0; i < 16; i++)
			{
				alpha[i] = (byte)table[(int)((bits >> (3 * i)) & 7)];
			}
		}

		private static void DecodeColour(byte[] data, int offset, byte[] output, bool dxt1, byte[] alpha)
		{
			int c0 = data[offset] | (data[offset + 1] << 8);
			int c1 = data[offset + 2] | (data[offset + 3] << 8);

			byte[,] palette = new byte[4, 3];
			Unpack565(c0, palette, 0);
			Unpack565(c1, palette, 1);

			bool punchThrough = dxt1 && c0 <= c1;
			if (!punchThrough)
			{
				for (int i = 0; i < 3; i++)
				{
					palette[2, i] = (byte)((2 * palette[0, i] + palette[1, i]) / 3);
					palette[3, i] = (byte)((palette[0, i] + 2 * palette[1, i]) / 3);
				}
			}
			else
			{
				for (int i = 0; i < 3; i++)
				{
					palette[2, i] = (byte)((palette[0, i] + palette[1, i]) / 2);
					palette[3, i] = 0;
				}
			}

			uint indices = (uint)(data[offset + 4]
				| (data[offset + 5] << 8)
				| (data[offset + 6] << 16)
				| (data[offset + 7] << 24));

			for (int i = 0; i < 16; i++)
			{
				int index = (int)((indices >> (2 * i)) & 3);
				output[i * 4] = palette[index, 0];
				output[i * 4 + 1] = palette[index, 1];
				output[i * 4 + 2] = palette[index, 2];
				output[i * 4 + 3] = (punchThrough && index == 3) ? (byte)0 : alpha[i];
			}
		}

		private static void Unpack565(int value, byte[,] palette, int slot)
		{
			int r = (value >> 11) & 0x1F;
			int g = (value >> 5) & 0x3F;
			int b = value & 0x1F;
			palette[slot, 0] = (byte)((r << 3) | (r >> 2));
			palette[slot, 1] = (byte)((g << 2) | (g >> 4));
			palette[slot, 2] = (byte)((b << 3) | (b >> 2));
		}
	}
}
