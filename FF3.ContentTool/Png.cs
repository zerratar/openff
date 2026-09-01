// Minimal 8-bit RGBA PNG writer.
//
// Deliberately dependency-free: the content tool has to run headless, so it cannot
// borrow MonoGame's Texture2D.SaveAsPng (that needs a GraphicsDevice), and pulling
// in an imaging package for one function is not worth it.

using System;
using System.IO;
using System.IO.Compression;

namespace FF3.ContentTool
{
	internal static class Png
	{
		public static void Write(string path, int width, int height, byte[] rgba)
		{
			if (rgba.Length < width * height * 4)
			{
				throw new ArgumentException("pixel buffer too small for " + width + "x" + height);
			}

			using FileStream file = File.Create(path);
			file.Write(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, 0, 8);

			byte[] header = new byte[13];
			WriteBigEndian(header, 0, width);
			WriteBigEndian(header, 4, height);
			header[8] = 8;   // bit depth
			header[9] = 6;   // colour type: RGBA
			header[10] = 0;  // deflate
			header[11] = 0;  // no filter preset
			header[12] = 0;  // no interlace
			WriteChunk(file, "IHDR", header);

			// Each scanline is prefixed with its filter type; 0 = none.
			byte[] raw = new byte[height * (width * 4 + 1)];
			int src = 0, dst = 0;
			for (int y = 0; y < height; y++)
			{
				raw[dst++] = 0;
				Buffer.BlockCopy(rgba, src, raw, dst, width * 4);
				src += width * 4;
				dst += width * 4;
			}

			WriteChunk(file, "IDAT", ZlibCompress(raw));
			WriteChunk(file, "IEND", Array.Empty<byte>());
		}

		private static byte[] ZlibCompress(byte[] data)
		{
			using MemoryStream output = new MemoryStream();
			output.WriteByte(0x78);  // CM/CINFO: deflate, 32K window
			output.WriteByte(0x01);  // FLG
			using (DeflateStream deflate = new DeflateStream(output, CompressionLevel.Optimal, leaveOpen: true))
			{
				deflate.Write(data, 0, data.Length);
			}
			uint adler = Adler32(data);
			output.WriteByte((byte)(adler >> 24));
			output.WriteByte((byte)(adler >> 16));
			output.WriteByte((byte)(adler >> 8));
			output.WriteByte((byte)adler);
			return output.ToArray();
		}

		private static uint Adler32(byte[] data)
		{
			uint a = 1, b = 0;
			foreach (byte value in data)
			{
				a = (a + value) % 65521;
				b = (b + a) % 65521;
			}
			return (b << 16) | a;
		}

		private static void WriteChunk(Stream stream, string type, byte[] data)
		{
			byte[] length = new byte[4];
			WriteBigEndian(length, 0, data.Length);
			stream.Write(length, 0, 4);

			byte[] typeAndData = new byte[4 + data.Length];
			for (int i = 0; i < 4; i++)
			{
				typeAndData[i] = (byte)type[i];
			}
			Buffer.BlockCopy(data, 0, typeAndData, 4, data.Length);
			stream.Write(typeAndData, 0, typeAndData.Length);

			byte[] crc = new byte[4];
			WriteBigEndian(crc, 0, unchecked((int)Crc32(typeAndData)));
			stream.Write(crc, 0, 4);
		}

		private static void WriteBigEndian(byte[] buffer, int offset, int value)
		{
			buffer[offset] = (byte)(value >> 24);
			buffer[offset + 1] = (byte)(value >> 16);
			buffer[offset + 2] = (byte)(value >> 8);
			buffer[offset + 3] = (byte)value;
		}

		private static readonly uint[] CrcTable = BuildCrcTable();

		private static uint[] BuildCrcTable()
		{
			uint[] table = new uint[256];
			for (uint n = 0; n < 256; n++)
			{
				uint c = n;
				for (int k = 0; k < 8; k++)
				{
					c = ((c & 1) != 0) ? (0xEDB88320u ^ (c >> 1)) : (c >> 1);
				}
				table[n] = c;
			}
			return table;
		}

		private static uint Crc32(byte[] data)
		{
			uint c = 0xFFFFFFFFu;
			foreach (byte value in data)
			{
				c = CrcTable[(c ^ value) & 0xFF] ^ (c >> 8);
			}
			return c ^ 0xFFFFFFFFu;
		}
	}
}
