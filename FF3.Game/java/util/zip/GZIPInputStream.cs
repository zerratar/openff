using System.IO;
using java.io;

namespace java.util.zip;

public class GZIPInputStream : InputStream
{
	private MemoryStream m_Ms;

	public GZIPInputStream(InputStream @is)
	{
		byte[] array = new byte[@is.available()];
		@is.read(array);
		array = GlobalScope.GZIP.Decompress(array);
		m_Ms = new MemoryStream(array);
	}

	public override int available()
	{
		return (int)m_Ms.Length;
	}

	public override void close()
	{
		m_Ms.Close();
	}

	public override int read(byte[] buffer, int offset, int length)
	{
		return m_Ms.Read(buffer, offset, length);
	}

	public override long skip(long byteCount)
	{
		return m_Ms.Seek(byteCount, SeekOrigin.Current);
	}
}
