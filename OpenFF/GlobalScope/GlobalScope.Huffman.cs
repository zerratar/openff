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
using OpenFF.Platform;
using OpenFF.Resources;

internal static partial class GlobalScope
{
	internal class Huffman
	{
		private readonly byte minCodeLength;

		private readonly byte maxCodeLength;

		private readonly int[] biggestCodeValuesFromCodeLength;

		private readonly int[] symbolsFromCodeValue;

		public Huffman(byte[] codeLengthsFromSymbol)
		{
			minCodeLength = codeLengthsFromSymbol.Where((byte len) => 0 < len).Min();
			maxCodeLength = codeLengthsFromSymbol.Max();
			int[] array = new int[maxCodeLength + 1];
			foreach (byte b in codeLengthsFromSymbol)
			{
				array[b]++;
			}
			biggestCodeValuesFromCodeLength = new int[maxCodeLength + 1];
			for (int num2 = 0; num2 < biggestCodeValuesFromCodeLength.Length; num2++)
			{
				biggestCodeValuesFromCodeLength[num2] = -1;
			}
			int num3 = 0;
			int num4 = 0;
			array[0] = 0;
			int[] array2 = new int[maxCodeLength + 1];
			for (int num5 = 1; num5 < array.Length; num5++)
			{
				int num6 = array[num5 - 1];
				num3 = (array2[num5] = num3 + num6 << 1);
				num4 = num3 + array[num5] - 1;
				biggestCodeValuesFromCodeLength[num5] = num4;
			}
			symbolsFromCodeValue = new int[num4 + 1];
			for (int num7 = 0; num7 < codeLengthsFromSymbol.Length; num7++)
			{
				byte b2 = codeLengthsFromSymbol[num7];
				if (b2 != 0)
				{
					int num8 = array2[b2]++;
					symbolsFromCodeValue[num8] = num7;
				}
			}
		}

		public int ReadSymbol(byte[] input, ref int bitIndex)
		{
			for (byte b = minCodeLength; b <= maxCodeLength; b++)
			{
				int num = biggestCodeValuesFromCodeLength[b];
				if (num >= 0)
				{
					int huffmanBits = GetHuffmanBits(input, bitIndex, b);
					if (num >= huffmanBits)
					{
						int result = symbolsFromCodeValue[huffmanBits];
						bitIndex += b;
						return result;
					}
				}
			}
			string message = $"Bad code at bit index {bitIndex}";
			throw new FormatException(message);
		}

		private static int GetHuffmanBits(byte[] input, int bitIndex, byte nBits)
		{
			int num = 0;
			int num2 = 1;
			int num3 = nBits - 1;
			while (0 <= num3)
			{
				if (GetBit(input, bitIndex + num3))
				{
					num += num2;
				}
				num3--;
				num2 *= 2;
			}
			return num;
		}

		private static bool GetBit(byte[] data, int bitIndex)
		{
			int num = bitIndex / 8;
			int num2 = bitIndex % 8;
			return (data[num] & (1 << num2)) != 0;
		}
	}
}
