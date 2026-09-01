using System;

namespace java.io;

public class ByteArrayInputStream : InputStream
{
	private byte[] m_abyData;

	private int m_iOffset;

	private int m_iLength;

	public ByteArrayInputStream(byte[] buf)
	{
		m_abyData = buf;
		m_iOffset = 0;
		m_iLength = buf.Length;
	}

	public override int available()
	{
		return m_abyData.Length;
	}

	public override int read(byte[] buffer, int offset, int length)
	{
		if (m_iOffset + length > m_iLength)
		{
			length = m_iLength - m_iOffset;
		}
		Array.Copy(m_abyData, m_iOffset, buffer, offset, length);
		m_iOffset += length;
		return length;
	}
}
