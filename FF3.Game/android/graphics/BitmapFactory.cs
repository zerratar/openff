using java.nio;

namespace android.graphics;

public class BitmapFactory
{
	public class Options
	{
		public Bitmap inBitmap;

		public int inDensity;

		public bool inDither;

		public bool inInputShareable;

		public bool inJustDecodeBounds;

		public bool inMutable;

		public bool inPreferQualityOverSpeed;

		public Bitmap.Config inPreferredConfig;

		public bool inPurgeable;

		public int inSampleSize;

		public bool inScaled;

		public int inScreenDensity;

		public int inTargetDensity;

		public byte[] inTempStorage;

		public bool mCancel;

		public int outHeight;

		public string outMimeType;

		public int outWidth;
	}

	private class MyByteBuffer : ByteBuffer
	{
		public MyByteBuffer(byte[] abyData, int iOffset, int iLength)
		{
			put(abyData, iOffset, iLength);
		}
	}

	public static Bitmap decodeByteArray(byte[] data, int offset, int length, Options opts)
	{
		Bitmap bitmap = new Bitmap();
		bitmap.copyPixelsFromBuffer(new MyByteBuffer(data, offset, length));
		return bitmap;
	}
}
