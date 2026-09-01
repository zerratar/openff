using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using android.content;
using android.text;
using android.widget;
using java.io;
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
	internal class Deflate
	{
		private static readonly Huffman fixedLiteralLengthHuffman;

		private static readonly Huffman fixedDistanceHuffman;

		private static byte[] indicesFromCodeLengthOrder;

		static Deflate()
		{
			indicesFromCodeLengthOrder = new byte[19]
			{
				16, 17, 18, 0, 8, 7, 9, 6, 10, 5,
				11, 4, 12, 3, 13, 2, 14, 1, 15
			};
			fixedLiteralLengthHuffman = BuildFixedLiteralLengthHuffman();
			fixedDistanceHuffman = BuildFixedDistanceHuffman();
		}

		private static Huffman BuildFixedLiteralLengthHuffman()
		{
			byte[] array = new byte[288];
			int i;
			for (i = 0; i < 144; i++)
			{
				array[i] = 8;
			}
			for (; i < 256; i++)
			{
				array[i] = 9;
			}
			for (; i < 280; i++)
			{
				array[i] = 7;
			}
			for (; i < 288; i++)
			{
				array[i] = 8;
			}
			return new Huffman(array);
		}

		private static Huffman BuildFixedDistanceHuffman()
		{
			byte[] array = new byte[32];
			for (int i = 0; i < 32; i++)
			{
				array[i] = 5;
			}
			return new Huffman(array);
		}

		public static byte[] Decompress(byte[] input, int index)
		{
			int bitIndex = index * 8;
			MemoryStream memoryStream = new MemoryStream();
			while (InflateBlock(input, ref bitIndex, memoryStream))
			{
			}
			return memoryStream.ToArray();
		}

		private static bool InflateBlock(byte[] input, ref int bitIndex, MemoryStream output)
		{
			bool flag = ReadBit(input, ref bitIndex);
			switch (ReadBits(input, ref bitIndex, 2))
			{
			case 0:
				InflatePlainBlock(input, ref bitIndex, output);
				break;
			case 1:
				InflateFixedBlock(input, ref bitIndex, output);
				break;
			case 2:
				InflateDynamicBlock(input, ref bitIndex, output);
				break;
			default:
			{
				string format = "Bad compression type at bit index {0}";
				string message = string.Format(format, bitIndex);
				throw new FormatException(message);
			}
			}
			return !flag;
		}

		private static bool GetBit(byte[] data, int bitIndex)
		{
			int num = bitIndex / 8;
			int num2 = bitIndex % 8;
			return (data[num] & (1 << num2)) != 0;
		}

		private static int GetBits(byte[] data, int bitIndex, int nBits)
		{
			int num = 0;
			int num2 = 1;
			int num3 = 0;
			while (num3 < nBits)
			{
				if (GetBit(data, bitIndex + num3))
				{
					num += num2;
				}
				num3++;
				num2 *= 2;
			}
			return num;
		}

		private static bool ReadBit(byte[] data, ref int bitIndex)
		{
			return GetBit(data, bitIndex++);
		}

		private static int ReadBits(byte[] data, ref int bitIndex, int nBits)
		{
			int bits = GetBits(data, bitIndex, nBits);
			bitIndex += nBits;
			return bits;
		}

		private static void InflatePlainBlock(byte[] input, ref int bitIndex, MemoryStream output)
		{
			bitIndex = (bitIndex + 7) & -8;
			int num = bitIndex / 8;
			int num2 = input[num] + input[num + 1] * 256;
			num += 4;
			output.Write(input, num, num2);
			bitIndex = (num + num2) * 8;
		}

		private static void InflateFixedBlock(byte[] input, ref int bitIndex, MemoryStream output)
		{
			InflateData(input, ref bitIndex, output, fixedLiteralLengthHuffman, fixedDistanceHuffman);
		}

		private static void InflateDynamicBlock(byte[] input, ref int bitIndex, MemoryStream output)
		{
			int num = ReadBits(input, ref bitIndex, 5) + 257;
			int num2 = ReadBits(input, ref bitIndex, 5) + 1;
			int num3 = ReadBits(input, ref bitIndex, 4) + 4;
			byte[] array = new byte[19];
			for (int i = 0; i < num3; i++)
			{
				byte b = (byte)ReadBits(input, ref bitIndex, 3);
				int num4 = CodeLengthOrderToIndex(i);
				array[num4] = b;
			}
			Huffman codeLengthHuffman = new Huffman(array);
			byte[] array2 = new byte[num];
			ReadCodeLengths(input, ref bitIndex, array2, codeLengthHuffman);
			Huffman literalLengthHuffman = new Huffman(array2);
			byte[] array3 = new byte[num2];
			ReadCodeLengths(input, ref bitIndex, array3, codeLengthHuffman);
			Huffman distanceHuffman = new Huffman(array3);
			InflateData(input, ref bitIndex, output, literalLengthHuffman, distanceHuffman);
		}

		private static void InflateData(byte[] input, ref int bitIndex, MemoryStream output, Huffman literalLengthHuffman, Huffman distanceHuffman)
		{
			while (true)
			{
				int num = literalLengthHuffman.ReadSymbol(input, ref bitIndex);
				if (num == 256)
				{
					break;
				}
				if (0 <= num && num <= 255)
				{
					output.WriteByte((byte)num);
					continue;
				}
				int length = ReadLength(input, ref bitIndex, num);
				int distance = ReadDistance(input, ref bitIndex, distanceHuffman);
				Duplicate(length, distance, output);
			}
		}

		private static int CodeLengthOrderToIndex(int order)
		{
			return indicesFromCodeLengthOrder[order];
		}

		private static void ReadCodeLengths(byte[] input, ref int bitIndex, byte[] codeLengths, Huffman codeLengthHuffman)
		{
			for (int i = 0; i < codeLengths.Length; i++)
			{
				byte b = (byte)codeLengthHuffman.ReadSymbol(input, ref bitIndex);
				if (0 <= b && b <= 15)
				{
					codeLengths[i] = b;
					continue;
				}
				int num;
				switch (b)
				{
				case 16:
					b = codeLengths[i - 1];
					num = ReadBits(input, ref bitIndex, 2) + 3;
					break;
				case 17:
					b = 0;
					num = ReadBits(input, ref bitIndex, 3) + 3;
					break;
				case 18:
					b = 0;
					num = ReadBits(input, ref bitIndex, 7) + 11;
					break;
				default:
				{
					string format = "Bad code length {0} at bit index {1}";
					string message = string.Format(format, b, bitIndex);
					throw new FormatException(message);
				}
				}
				for (int j = 0; j < num; j++)
				{
					codeLengths[i + j] = b;
				}
				i += num - 1;
			}
		}

		private static int ReadLength(byte[] input, ref int bitIndex, int literalLength)
		{
			int num;
			int nBits;
			switch (literalLength)
			{
			case 257:
			case 258:
			case 259:
			case 260:
			case 261:
			case 262:
			case 263:
			case 264:
				return literalLength - 254;
			case 265:
				num = 11;
				nBits = 1;
				break;
			case 266:
				num = 13;
				nBits = 1;
				break;
			case 267:
				num = 15;
				nBits = 1;
				break;
			case 268:
				num = 17;
				nBits = 1;
				break;
			case 269:
				num = 19;
				nBits = 2;
				break;
			case 270:
				num = 23;
				nBits = 2;
				break;
			case 271:
				num = 27;
				nBits = 2;
				break;
			case 272:
				num = 31;
				nBits = 2;
				break;
			case 273:
				num = 35;
				nBits = 3;
				break;
			case 274:
				num = 43;
				nBits = 3;
				break;
			case 275:
				num = 51;
				nBits = 3;
				break;
			case 276:
				num = 59;
				nBits = 3;
				break;
			case 277:
				num = 67;
				nBits = 4;
				break;
			case 278:
				num = 83;
				nBits = 4;
				break;
			case 279:
				num = 99;
				nBits = 4;
				break;
			case 280:
				num = 115;
				nBits = 4;
				break;
			case 281:
				num = 131;
				nBits = 5;
				break;
			case 282:
				num = 163;
				nBits = 5;
				break;
			case 283:
				num = 195;
				nBits = 5;
				break;
			case 284:
				num = 227;
				nBits = 5;
				break;
			case 285:
				return 258;
			default:
			{
				string format = "Bad literal/length code {0} at bit index {1}";
				string message = string.Format(format, literalLength, bitIndex);
				throw new FormatException(message);
			}
			}
			int num2 = ReadBits(input, ref bitIndex, nBits);
			return num + num2;
		}

		private static int ReadDistance(byte[] input, ref int bitIndex, Huffman distanceHuffman)
		{
			int num = distanceHuffman.ReadSymbol(input, ref bitIndex);
			int num2;
			int nBits;
			switch (num)
			{
			case 0:
			case 1:
			case 2:
			case 3:
				return num + 1;
			case 4:
				num2 = 5;
				nBits = 1;
				break;
			case 5:
				num2 = 7;
				nBits = 1;
				break;
			case 6:
				num2 = 9;
				nBits = 2;
				break;
			case 7:
				num2 = 13;
				nBits = 2;
				break;
			case 8:
				num2 = 17;
				nBits = 3;
				break;
			case 9:
				num2 = 25;
				nBits = 3;
				break;
			case 10:
				num2 = 33;
				nBits = 4;
				break;
			case 11:
				num2 = 49;
				nBits = 4;
				break;
			case 12:
				num2 = 65;
				nBits = 5;
				break;
			case 13:
				num2 = 97;
				nBits = 5;
				break;
			case 14:
				num2 = 129;
				nBits = 6;
				break;
			case 15:
				num2 = 193;
				nBits = 6;
				break;
			case 16:
				num2 = 257;
				nBits = 7;
				break;
			case 17:
				num2 = 385;
				nBits = 7;
				break;
			case 18:
				num2 = 513;
				nBits = 8;
				break;
			case 19:
				num2 = 769;
				nBits = 8;
				break;
			case 20:
				num2 = 1025;
				nBits = 9;
				break;
			case 21:
				num2 = 1537;
				nBits = 9;
				break;
			case 22:
				num2 = 2049;
				nBits = 10;
				break;
			case 23:
				num2 = 3073;
				nBits = 10;
				break;
			case 24:
				num2 = 4097;
				nBits = 11;
				break;
			case 25:
				num2 = 6145;
				nBits = 11;
				break;
			case 26:
				num2 = 8193;
				nBits = 12;
				break;
			case 27:
				num2 = 12289;
				nBits = 12;
				break;
			case 28:
				num2 = 16385;
				nBits = 13;
				break;
			case 29:
				num2 = 24577;
				nBits = 13;
				break;
			default:
			{
				string format = "Bad distance code {0} at bit index {1}";
				string message = string.Format(format, num, bitIndex);
				throw new FormatException(message);
			}
			}
			int num3 = ReadBits(input, ref bitIndex, nBits);
			return num2 + num3;
		}

		private static void Duplicate(int length, int distance, MemoryStream output)
		{
			byte[] buffer = output.GetBuffer();
			int num = (int)output.Length;
			byte[] array = new byte[length];
			int num2 = num - distance;
			int num3 = num2;
			int num4 = 0;
			while (num4 < length)
			{
				if (num <= num3)
				{
					num3 = num2;
				}
				array[num4] = buffer[num3];
				num4++;
				num3++;
			}
			output.Write(array, 0, length);
		}
	}
}
