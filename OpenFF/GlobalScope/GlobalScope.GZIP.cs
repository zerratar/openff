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
	internal class GZIP
	{
		public static byte[] Decompress(byte[] input)
		{
			int indexOfDeflate = GetIndexOfDeflate(input);
			return Deflate.Decompress(input, indexOfDeflate);
		}

		private static int GetIndexOfDeflate(byte[] input)
		{
			byte b = input[3];
			int num = 10;
			if ((b & 4) != 0)
			{
				int num2 = input[num] + input[num + 1] * 256;
				num += 2 + num2;
			}
			if ((b & 8) != 0)
			{
				while (input[num++] != 0)
				{
				}
			}
			if ((b & 0x10) != 0)
			{
				while (input[num++] != 0)
				{
				}
			}
			if ((b & 2) != 0)
			{
				num += 2;
			}
			return num;
		}
	}
}
