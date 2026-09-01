namespace java.io;

// PORT: was a java.io.OutputStream subclass wrapping System.out. The base stream
// shim is gone; this remains only because JavaSystem.out is still referenced, and
// the original println was already a no-op.
public class PrintStream
{
	public void println(string str)
	{
	}
}
