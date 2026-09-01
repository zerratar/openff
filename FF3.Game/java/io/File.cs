using System.IO;
using FF3;

namespace java.io;

public class File
{
	private bool m_bIsDirectory;

	private string m_strParent;

	private string m_strPath;

	private long m_lLength;

	public File(File dir, string name)
	{
		m_strPath = dir.getPath() + name;
		checkPath();
		if (m_bIsDirectory)
		{
			m_lLength = 0L;
			return;
		}
		Stream stream = GameFiles.OpenRead(m_strPath);
		m_lLength = stream.Length;
		stream.Close();
	}

	public File(string path)
	{
		m_strPath = path;
		checkPath();
		if (m_bIsDirectory)
		{
			m_lLength = 0L;
			return;
		}
		Stream stream = GameFiles.OpenRead(m_strPath);
		m_lLength = stream.Length;
		stream.Close();
	}

	public bool delete()
	{
		return true;
	}

	public string getParent()
	{
		return m_strParent;
	}

	public string getPath()
	{
		return m_strPath;
	}

	public long length()
	{
		return m_lLength;
	}

	public bool mkdirs()
	{
		return true;
	}

	private void checkPath()
	{
		int num = m_strPath.LastIndexOf('.');
		if (num < 0)
		{
			m_bIsDirectory = true;
		}
		else
		{
			m_bIsDirectory = false;
		}
		num = m_strPath.LastIndexOf('/');
		if (num < 0)
		{
			m_strParent = "";
		}
		else
		{
			m_strParent = m_strPath.Substring(0, num + 1);
		}
	}
}
