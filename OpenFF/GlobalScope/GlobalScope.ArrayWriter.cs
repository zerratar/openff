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
	internal class ArrayWriter
	{
		private MemoryStream m_St;

		private long m_lMark;

		private static byte[] m_abyWriteBuffer = new byte[262144];

		public ArrayWriter()
		{
			m_St = new MemoryStream();
			m_lMark = 0L;
		}

		~ArrayWriter()
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

		public byte[] getBytes()
		{
			return m_St.ToArray();
		}

		public int write(float[] abyData, int iOffset, int iLength)
		{
			int num = 0;
			int num3;
			int num2;
			for (num2 = iLength * 4; num2 > m_abyWriteBuffer.Length; num2 -= num3)
			{
				Buffer.BlockCopy(abyData, iOffset * 4 + num, m_abyWriteBuffer, 0, m_abyWriteBuffer.Length);
				num3 = m_abyWriteBuffer.Length;
				m_St.Write(m_abyWriteBuffer, 0, num3);
				num += num3;
			}
			Buffer.BlockCopy(abyData, iOffset * 4 + num, m_abyWriteBuffer, 0, num2);
			num3 = num2;
			m_St.Write(m_abyWriteBuffer, 0, num3);
			num += num3;
			num2 -= num3;
			return num;
		}

		public int write(int[] abyData, int iOffset, int iLength)
		{
			int num = 0;
			int num3;
			int num2;
			for (num2 = iLength * 4; num2 > m_abyWriteBuffer.Length; num2 -= num3)
			{
				Buffer.BlockCopy(abyData, iOffset * 4 + num, m_abyWriteBuffer, 0, m_abyWriteBuffer.Length);
				num3 = m_abyWriteBuffer.Length;
				m_St.Write(m_abyWriteBuffer, 0, num3);
				num += num3;
			}
			Buffer.BlockCopy(abyData, iOffset * 4 + num, m_abyWriteBuffer, 0, num2);
			num3 = num2;
			m_St.Write(m_abyWriteBuffer, 0, num3);
			num += num3;
			num2 -= num3;
			return num;
		}

		public int write(uint[] abyData, int iOffset, int iLength)
		{
			int num = 0;
			int num3;
			int num2;
			for (num2 = iLength * 4; num2 > m_abyWriteBuffer.Length; num2 -= num3)
			{
				Buffer.BlockCopy(abyData, iOffset * 4 + num, m_abyWriteBuffer, 0, m_abyWriteBuffer.Length);
				num3 = m_abyWriteBuffer.Length;
				m_St.Write(m_abyWriteBuffer, 0, num3);
				num += num3;
			}
			Buffer.BlockCopy(abyData, iOffset * 4 + num, m_abyWriteBuffer, 0, num2);
			num3 = num2;
			m_St.Write(m_abyWriteBuffer, 0, num3);
			num += num3;
			num2 -= num3;
			return num;
		}

		public int write(short[] abyData, int iOffset, int iLength)
		{
			int num = 0;
			int num3;
			int num2;
			for (num2 = iLength * 2; num2 > m_abyWriteBuffer.Length; num2 -= num3)
			{
				Buffer.BlockCopy(abyData, iOffset * 2 + num, m_abyWriteBuffer, 0, m_abyWriteBuffer.Length);
				num3 = m_abyWriteBuffer.Length;
				m_St.Write(m_abyWriteBuffer, 0, num3);
				num += num3;
			}
			Buffer.BlockCopy(abyData, iOffset * 2 + num, m_abyWriteBuffer, 0, num2);
			num3 = num2;
			m_St.Write(m_abyWriteBuffer, 0, num3);
			num += num3;
			num2 -= num3;
			return num;
		}

		public int write(ushort[] abyData, int iOffset, int iLength)
		{
			int num = 0;
			int num3;
			int num2;
			for (num2 = iLength * 2; num2 > m_abyWriteBuffer.Length; num2 -= num3)
			{
				Buffer.BlockCopy(abyData, iOffset * 2 + num, m_abyWriteBuffer, 0, m_abyWriteBuffer.Length);
				num3 = m_abyWriteBuffer.Length;
				m_St.Write(m_abyWriteBuffer, 0, num3);
				num += num3;
			}
			Buffer.BlockCopy(abyData, iOffset * 2 + num, m_abyWriteBuffer, 0, num2);
			num3 = num2;
			m_St.Write(m_abyWriteBuffer, 0, num3);
			num += num3;
			num2 -= num3;
			return num;
		}

		public int write(sbyte[] abyData, int iOffset, int iLength)
		{
			int num = 0;
			int num3;
			int num2;
			for (num2 = iLength; num2 > m_abyWriteBuffer.Length; num2 -= num3)
			{
				Buffer.BlockCopy(abyData, iOffset + num, m_abyWriteBuffer, 0, m_abyWriteBuffer.Length);
				num3 = m_abyWriteBuffer.Length;
				m_St.Write(m_abyWriteBuffer, 0, num3);
				num += num3;
			}
			Buffer.BlockCopy(abyData, iOffset + num, m_abyWriteBuffer, 0, num2);
			num3 = num2;
			m_St.Write(m_abyWriteBuffer, 0, num3);
			num += num3;
			num2 -= num3;
			return num;
		}

		public int write(byte[] abyData, int iOffset, int iLength)
		{
			m_St.Write(abyData, iOffset, iLength);
			return iLength;
		}

		public void writeSingle(float abyData)
		{
			m_St.Write(BitConverter.GetBytes(abyData), 0, 4);
		}

		public void writeInt64(long abyData)
		{
			m_St.Write(BitConverter.GetBytes(abyData), 0, 8);
		}

		public void writeUInt64(ulong abyData)
		{
			m_St.Write(BitConverter.GetBytes(abyData), 0, 8);
		}

		public void writeInt32(int abyData)
		{
			m_St.Write(BitConverter.GetBytes(abyData), 0, 4);
		}

		public void writeUInt32(uint abyData)
		{
			m_St.Write(BitConverter.GetBytes(abyData), 0, 4);
		}

		public void writeInt16(short abyData)
		{
			m_St.Write(BitConverter.GetBytes(abyData), 0, 2);
		}

		public void writeUInt16(ushort abyData)
		{
			m_St.Write(BitConverter.GetBytes(abyData), 0, 2);
		}

		public void writeSByte(sbyte abyData)
		{
			m_St.WriteByte((byte)abyData);
		}

		public void writeByte(byte abyData)
		{
			m_St.WriteByte(abyData);
		}
	}
}
