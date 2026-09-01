using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using java.nio;

namespace android.graphics;

public sealed class Bitmap
{
	public enum Config
	{
		ALPHA_8,
		ARGB_4444,
		ARGB_8888,
		RGB_565
	}

	private Texture2D m_Texture2D;

	public static Bitmap createBitmap(int width, int height, Config config)
	{
		return null;
	}

	public void copyPixelsFromBuffer(Buffer src)
	{
		FF3.Log.First(FF3.LogChannel.Texture, "Bitmap.decode", 200, () => $"buffer={(src == null || src.array() == null ? -1 : ((byte[])src.array()).Length)} bytes"); /*FF3LOG*/
		byte[] bytes = (byte[])src.array();
		MemoryStream stream = new MemoryStream(bytes);
		m_Texture2D = Texture2D.FromStream(GlobalScope.m_Graphics.GetGraphicsDeviceManager().GraphicsDevice, stream);
		FF3.Log.First(FF3.LogChannel.Texture, "Bitmap.decode", 200, () =>
			$"{bytes.Length} bytes -> {(m_Texture2D == null ? "NULL" : m_Texture2D.Width + "x" + m_Texture2D.Height + " " + m_Texture2D.Format)}"
			+ " magic=" + FF3.Diagnostics.Hex(bytes, 16)
			+ FF3.Diagnostics.DumpSource(bytes, "png"));
	}

	public int getWidth()
	{
		return m_Texture2D.Width;
	}

	public int getHeight()
	{
		return m_Texture2D.Height;
	}

	public void getPixels(int[] pixels, int offset, int stride, int x, int y, int width, int height)
	{
		// Do NOT hand `offset` to Texture2D.GetData. On MonoGame's DesktopGL backend,
		// PlatformGetData takes a fast path whenever the requested rectangle covers the
		// whole texture, and that path calls GL.GetTexImage(..., data) directly, writing
		// from index 0 and ignoring startIndex entirely.
		//
		// The caller (MainActivity.loadTexture) stashes the image width and height in
		// pixels[0] and pixels[1] and asks for the pixels at offset 2. With the fast path
		// those two slots get overwritten by the first two pixels of the image - usually
		// transparent black - so LoadPNG reads the size back as 0x0, rounds it up to the
		// 8x8 minimum, and every background in the game becomes an 8x8 smear.
		//
		// Read into our own buffer at index 0, then place it ourselves.
		int count = width * height;
		int[] source = new int[count];
		m_Texture2D.GetData(0, new Rectangle(x, y, width, height), source, 0, count);

		if (stride == width)
		{
			System.Array.Copy(source, 0, pixels, offset, count);
			return;
		}
		for (int row = 0; row < height; row++)
		{
			System.Array.Copy(source, row * width, pixels, offset + row * stride, width);
		}
	}

	public void recycle()
	{
		m_Texture2D.Dispose();
	}
}
