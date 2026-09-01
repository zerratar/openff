namespace java.io;

public abstract class InputStream
{
	public virtual int available()
	{
		return 0;
	}

	public virtual void close()
	{
	}

	public virtual int read(byte[] buffer)
	{
		return read(buffer, 0, buffer.Length);
	}

	public virtual int read(byte[] buffer, int offset, int length)
	{
		return length;
	}

	public virtual long skip(long byteCount)
	{
		return byteCount;
	}
}
