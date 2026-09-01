using System.IO;
using System.IO.IsolatedStorage;

namespace java.io;

public class FileOutputStream : OutputStream
{
	private Stream m_Fs;

	public FileOutputStream(string path)
	{
		IsolatedStorageFile userStoreForApplication = IsolatedStorageFile.GetUserStoreForApplication();
		m_Fs = userStoreForApplication.CreateFile(path);
	}

	public FileOutputStream(File file)
	{
		IsolatedStorageFile userStoreForApplication = IsolatedStorageFile.GetUserStoreForApplication();
		m_Fs = userStoreForApplication.CreateFile(file.getPath());
	}

	public override void close()
	{
		m_Fs.Close();
	}

	public override void write(byte[] buffer)
	{
		m_Fs.Write(buffer, 0, buffer.Length);
	}
}
