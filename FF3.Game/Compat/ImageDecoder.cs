// PNG decoding for the game's texture loader.
//
// Replaces the android.graphics.Bitmap / BitmapFactory / java.nio.ByteBuffer chain,
// which existed only to hand a decoded image back as an int[]. That is all this does,
// directly.
//
// Layout of the returned array matches what GlobalScope.LoadPNG expects:
//   [0] width, [1] height, then width * height pixels in XNA's Color packing
//   (R in the low byte), which LoadPNG unpacks back into RGBA bytes.

using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FF3
{
	internal static class ImageDecoder
	{
		/// <summary>Decodes an encoded image. Returns null if it cannot be read.</summary>
		public static int[] Decode(byte[] encoded)
		{
			if (encoded == null || encoded.Length == 0)
			{
				return null;
			}

			GraphicsDevice device = GlobalScope.m_Graphics?.GetGraphicsDeviceManager()?.GraphicsDevice;
			if (device == null)
			{
				Log.Write(LogChannel.Texture, "image decode attempted before the device existed");
				return null;
			}

			try
			{
				using MemoryStream stream = new MemoryStream(encoded);
				using Texture2D texture = Texture2D.FromStream(device, stream);

				int width = texture.Width;
				int height = texture.Height;
				int count = width * height;

				// Read into our own buffer at index 0. Texture2D.GetData ignores
				// startIndex on the whole-texture fast path (see Docs/Porting-Notes),
				// so the width/height header has to be written afterwards.
				Color[] pixels = new Color[count];
				texture.GetData(pixels);

				int[] result = new int[count + 2];
				result[0] = width;
				result[1] = height;
				for (int i = 0; i < count; i++)
				{
					result[i + 2] = (int)pixels[i].PackedValue;
				}

				Log.First(LogChannel.Texture, "decode", 60,
					() => encoded.Length + " bytes -> " + width + "x" + height);
				return result;
			}
			catch (Exception ex)
			{
				Log.Write(LogChannel.Texture, "image decode failed: " + ex.Message);
				return null;
			}
		}
	}
}
