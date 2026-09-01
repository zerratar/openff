using System.IO;
using FF3;

namespace java.io;

public class FileInputStream : InputStream
{
	private Stream m_Fs;

	public FileInputStream(File file)
	{
	}

	public FileInputStream(string path)
	{
		FF3.Log.First(FF3.LogChannel.File, "open", 400, () => path); /*FF3LOG*/
		// Was TitleContainer.OpenStream, which only looks next to the executable.
		m_Fs = GameFiles.OpenRead(path);
	}

	public override int available()
	{
		return (int)m_Fs.Length;
	}

	public override void close()
	{
		m_Fs.Close();
	}

	public FileDescriptor getFD()
	{
		return new FileDescriptor();
	}

	public override int read(byte[] buffer, int offset, int length)
	{
		return m_Fs.Read(buffer, offset, length);
	}

	public override long skip(long byteCount)
	{
		return m_Fs.Seek(byteCount, SeekOrigin.Current);
	}
}
