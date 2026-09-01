namespace java.io;

public class DataInputStream : InputStream
{
	private static byte[] m_abyReadCache = new byte[4];

	private InputStream m_In;

	public DataInputStream(InputStream @in)
	{
		m_In = @in;
	}

	public override int available()
	{
		return m_In.available();
	}

	public override void close()
	{
		m_In.close();
	}

	public override int read(byte[] buffer, int offset, int length)
	{
		return m_In.read(buffer, 0, buffer.Length);
	}

	public int readInt()
	{
		m_In.read(m_abyReadCache);
		return intOf(m_abyReadCache);
	}

	public override long skip(long byteCount)
	{
		return m_In.skip(byteCount);
	}

	private static int intOf(byte[] abyData)
	{
		int num = 0;
		num |= (abyData[0] & 0xFF) << 24;
		num |= (abyData[1] & 0xFF) << 16;
		num |= (abyData[2] & 0xFF) << 8;
		return num | (abyData[3] & 0xFF);
	}
}
