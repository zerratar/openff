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
	internal class BitReader
	{
		private static byte[] m_abyReadBuffer = new byte[4];

		public static byte[] convertByteArray(int[] abyData, int iOffset, int iLength)
		{
			byte[] array = new byte[iLength * 4];
			Buffer.BlockCopy(abyData, iOffset * 4, array, 0, array.Length);
			return array;
		}

		public static float convertSingle(int iData)
		{
			m_abyReadBuffer[0] = (byte)(iData & 0xFF);
			m_abyReadBuffer[1] = (byte)((iData >> 8) & 0xFF);
			m_abyReadBuffer[2] = (byte)((iData >> 16) & 0xFF);
			m_abyReadBuffer[3] = (byte)((iData >> 24) & 0xFF);
			return BitConverter.ToSingle(m_abyReadBuffer, 0);
		}
	}
}
