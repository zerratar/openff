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
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
	internal class ArrayReader
	{
		private MemoryStream m_St;

		private long m_lMark;

		private static byte[] m_abyReadBuffer = new byte[262144];

		public static int packInt32(byte[] abyData, int iOffset)
		{
			return abyData[iOffset] | (abyData[iOffset + 1] << 8) | (abyData[iOffset + 2] << 16) | (abyData[iOffset + 3] << 24);
		}

		public static uint packUInt32(byte[] abyData, int iOffset)
		{
			return (uint)(abyData[iOffset] | (abyData[iOffset + 1] << 8) | (abyData[iOffset + 2] << 16) | (abyData[iOffset + 3] << 24));
		}

		public static short packInt16(byte[] abyData, int iOffset)
		{
			return (short)(abyData[iOffset] | (abyData[iOffset + 1] << 8));
		}

		public static ushort packUInt16(byte[] abyData, int iOffset)
		{
			return (ushort)(abyData[iOffset] | (abyData[iOffset + 1] << 8));
		}

		public static int indexOf(byte[] abyData, int iOffset, int iLength, byte byData, int iCount)
		{
			int num = iOffset + iLength;
			for (int i = iOffset; i < num; i++)
			{
				if (abyData[i] == byData)
				{
					iCount--;
					if (iCount <= 0)
					{
						return i;
					}
				}
			}
			return -1;
		}

		public ArrayReader(Array a)
		{
			m_St = new MemoryStream((byte[])a);
			m_lMark = 0L;
		}

		~ArrayReader()
		{
			dispose();
		}

		public void dispose()
		{
			if (m_St != null)
			{
				m_St.Close();
				m_St = null;
			}
		}

		public void setPosition(long lPosition)
		{
			m_St.Position = lPosition;
		}

		public long getPosition()
		{
			return m_St.Position;
		}

		public void setMark(long lMark)
		{
			m_lMark = lMark;
		}

		public long getMark()
		{
			return m_lMark;
		}

		public void skip(long lSeek)
		{
			m_St.Seek(lSeek, SeekOrigin.Current);
		}

		public long rest()
		{
			return m_St.Length - m_St.Position;
		}

		public byte[] getBytes()
		{
			return m_St.ToArray();
		}

		public int getInt32()
		{
			long position = getPosition();
			int result = readInt32();
			setPosition(position);
			return result;
		}

		public int read(float[] abyData, int iOffset, int iLength)
		{
			int num = 0;
			int num3;
			int num2;
			for (num2 = iLength * 4; num2 > m_abyReadBuffer.Length; num2 -= num3)
			{
				num3 = m_St.Read(m_abyReadBuffer, 0, m_abyReadBuffer.Length);
				Buffer.BlockCopy(m_abyReadBuffer, 0, abyData, iOffset * 4 + num, num3);
				num += num3;
			}
			num3 = m_St.Read(m_abyReadBuffer, 0, num2);
			Buffer.BlockCopy(m_abyReadBuffer, 0, abyData, iOffset * 4 + num, num3);
			num += num3;
			num2 -= num3;
			return num;
		}

		public int read(int[] abyData, int iOffset, int iLength)
		{
			int num = 0;
			int num3;
			int num2;
			for (num2 = iLength * 4; num2 > m_abyReadBuffer.Length; num2 -= num3)
			{
				num3 = m_St.Read(m_abyReadBuffer, 0, m_abyReadBuffer.Length);
				Buffer.BlockCopy(m_abyReadBuffer, 0, abyData, iOffset * 4 + num, num3);
				num += num3;
			}
			num3 = m_St.Read(m_abyReadBuffer, 0, num2);
			Buffer.BlockCopy(m_abyReadBuffer, 0, abyData, iOffset * 4 + num, num3);
			num += num3;
			num2 -= num3;
			return num;
		}

		public int read(uint[] abyData, int iOffset, int iLength)
		{
			int num = 0;
			int num3;
			int num2;
			for (num2 = iLength * 4; num2 > m_abyReadBuffer.Length; num2 -= num3)
			{
				num3 = m_St.Read(m_abyReadBuffer, 0, m_abyReadBuffer.Length);
				Buffer.BlockCopy(m_abyReadBuffer, 0, abyData, iOffset * 4 + num, num3);
				num += num3;
			}
			num3 = m_St.Read(m_abyReadBuffer, 0, num2);
			Buffer.BlockCopy(m_abyReadBuffer, 0, abyData, iOffset * 4 + num, num3);
			num += num3;
			num2 -= num3;
			return num;
		}

		public int read(short[] abyData, int iOffset, int iLength)
		{
			int num = 0;
			int num3;
			int num2;
			for (num2 = iLength * 2; num2 > m_abyReadBuffer.Length; num2 -= num3)
			{
				num3 = m_St.Read(m_abyReadBuffer, 0, m_abyReadBuffer.Length);
				Buffer.BlockCopy(m_abyReadBuffer, 0, abyData, iOffset * 2 + num, num3);
				num += num3;
			}
			num3 = m_St.Read(m_abyReadBuffer, 0, num2);
			Buffer.BlockCopy(m_abyReadBuffer, 0, abyData, iOffset * 2 + num, num3);
			num += num3;
			num2 -= num3;
			return num;
		}

		public int read(ushort[] abyData, int iOffset, int iLength)
		{
			int num = 0;
			int num3;
			int num2;
			for (num2 = iLength * 2; num2 > m_abyReadBuffer.Length; num2 -= num3)
			{
				num3 = m_St.Read(m_abyReadBuffer, 0, m_abyReadBuffer.Length);
				Buffer.BlockCopy(m_abyReadBuffer, 0, abyData, iOffset * 2 + num, num3);
				num += num3;
			}
			num3 = m_St.Read(m_abyReadBuffer, 0, num2);
			Buffer.BlockCopy(m_abyReadBuffer, 0, abyData, iOffset * 2 + num, num3);
			num += num3;
			num2 -= num3;
			return num;
		}

		public int read(sbyte[] abyData, int iOffset, int iLength)
		{
			int num = 0;
			int num3;
			int num2;
			for (num2 = iLength; num2 > m_abyReadBuffer.Length; num2 -= num3)
			{
				num3 = m_St.Read(m_abyReadBuffer, 0, m_abyReadBuffer.Length);
				Buffer.BlockCopy(m_abyReadBuffer, 0, abyData, iOffset + num, num3);
				num += num3;
			}
			num3 = m_St.Read(m_abyReadBuffer, 0, num2);
			Buffer.BlockCopy(m_abyReadBuffer, 0, abyData, iOffset + num, num3);
			num += num3;
			num2 -= num3;
			return num;
		}

		public int read(byte[] abyData, int iOffset, int iLength)
		{
			return m_St.Read(abyData, iOffset, iLength);
		}

		public string readString(string strEncode)
		{
			MemoryStream memoryStream = new MemoryStream();
			int num;
			while ((num = m_St.ReadByte()) != 0)
			{
				memoryStream.WriteByte((byte)num);
			}
			if (m_St.Position % 2 != 0)
			{
				m_St.ReadByte();
			}
			byte[] array = memoryStream.ToArray();
			return StringUtil.createString(array, 0, array.Length, strEncode);
		}

		public string readStringNoAlign(string strEncode)
		{
			MemoryStream memoryStream = new MemoryStream();
			int num;
			while ((num = m_St.ReadByte()) != 0)
			{
				memoryStream.WriteByte((byte)num);
			}
			byte[] array = memoryStream.ToArray();
			return StringUtil.createString(array, 0, array.Length, strEncode);
		}

		public string[] readStringArray(int iWordCount, int iStringCount)
		{
			string[] array = new string[iStringCount];
			byte[] array2 = new byte[iWordCount];
			for (int i = 0; i < iStringCount; i++)
			{
				m_St.Read(array2, 0, array2.Length);
				array[i] = StringUtil.createString(array2);
			}
			return array;
		}

		public float readSingle()
		{
			m_St.Read(m_abyReadBuffer, 0, 4);
			return BitConverter.ToSingle(m_abyReadBuffer, 0);
		}

		public long readInt64()
		{
			m_St.Read(m_abyReadBuffer, 0, 8);
			return m_abyReadBuffer[0] | (m_abyReadBuffer[1] << 8) | (m_abyReadBuffer[2] << 16) | (m_abyReadBuffer[3] << 24) | m_abyReadBuffer[4] | (m_abyReadBuffer[5] << 8) | (m_abyReadBuffer[6] << 16) | (m_abyReadBuffer[7] << 24);
		}

		public ulong readUInt64()
		{
			m_St.Read(m_abyReadBuffer, 0, 8);
			return (ulong)(m_abyReadBuffer[0] | (m_abyReadBuffer[1] << 8) | (m_abyReadBuffer[2] << 16) | (m_abyReadBuffer[3] << 24) | m_abyReadBuffer[4] | (m_abyReadBuffer[5] << 8) | (m_abyReadBuffer[6] << 16) | (m_abyReadBuffer[7] << 24));
		}

		public int readInt32()
		{
			m_St.Read(m_abyReadBuffer, 0, 4);
			return m_abyReadBuffer[0] | (m_abyReadBuffer[1] << 8) | (m_abyReadBuffer[2] << 16) | (m_abyReadBuffer[3] << 24);
		}

		public uint readUInt32()
		{
			m_St.Read(m_abyReadBuffer, 0, 4);
			return (uint)(m_abyReadBuffer[0] | (m_abyReadBuffer[1] << 8) | (m_abyReadBuffer[2] << 16) | (m_abyReadBuffer[3] << 24));
		}

		public short readInt16()
		{
			m_St.Read(m_abyReadBuffer, 0, 2);
			return (short)(m_abyReadBuffer[0] | (m_abyReadBuffer[1] << 8));
		}

		public ushort readUInt16()
		{
			m_St.Read(m_abyReadBuffer, 0, 2);
			return (ushort)(m_abyReadBuffer[0] | (m_abyReadBuffer[1] << 8));
		}

		public sbyte readSByte()
		{
			return (sbyte)m_St.ReadByte();
		}

		public byte readByte()
		{
			return (byte)m_St.ReadByte();
		}
	}
}
