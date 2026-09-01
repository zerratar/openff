using System;

namespace java.nio;

public abstract class ByteBuffer : Buffer
{
	private byte[] m_abyData;

	public override object array()
	{
		return m_abyData;
	}

	public ByteBuffer put(byte[] src, int srcOffset, int byteCount)
	{
		m_abyData = new byte[byteCount];
		System.Buffer.BlockCopy(src, srcOffset, m_abyData, 0, byteCount);
		return this;
	}
}
