using System;
using java.io;

public static class JavaSystem
{
	public static PrintStream @out = new PrintStream();

	public static void arraycopy(Array src, int srcPos, Array dst, int dstPos, int length)
	{
		Array.Copy(src, srcPos, dst, dstPos, length);
	}

	public static long currentTimeMillis()
	{
		return (long)(DateTime.Now - DateTime.Parse("1970/1/1")).TotalMilliseconds;
	}

	public static void exit(int code)
	{
		GlobalScope.m_Graphics.getGame().Exit();
	}
}
