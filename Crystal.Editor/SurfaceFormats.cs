// Converts XNA surface formats to straight 8-bit RGBA.

using System;

namespace Crystal
{
	internal static class SurfaceFormats
	{
		public const int Color = 0;
		public const int Bgr565 = 1;
		public const int Bgra5551 = 2;
		public const int Bgra4444 = 3;
		public const int Dxt1 = 4;
		public const int Dxt3 = 5;
		public const int Dxt5 = 6;
		public const int Alpha8 = 12;

		public static string Name(int format) => format switch
		{
			Color => "Color",
			Bgr565 => "Bgr565",
			Bgra5551 => "Bgra5551",
			Bgra4444 => "Bgra4444",
			Dxt1 => "Dxt1",
			Dxt3 => "Dxt3",
			Dxt5 => "Dxt5",
			Alpha8 => "Alpha8",
			_ => "format" + format
		};

		public static bool CanDecode(int format) =>
			format is Color or Bgr565 or Bgra5551 or Bgra4444 or Alpha8
				or Dxt1 or Dxt3 or Dxt5;

		/// <summary>Expands one mip level to RGBA8888.</summary>
		public static byte[] ToRgba(int format, int width, int height, byte[] data)
		{
			if (format is Dxt1 or Dxt3 or Dxt5)
			{
				return Dxt.Decode(format, width, height, data);
			}

			byte[] rgba = new byte[width * height * 4];
			int pixels = width * height;

			switch (format)
			{
				case Color:
					Buffer.BlockCopy(data, 0, rgba, 0, Math.Min(data.Length, rgba.Length));
					return rgba;

				case Alpha8:
					for (int i = 0; i < pixels && i < data.Length; i++)
					{
						rgba[i * 4] = 255;
						rgba[i * 4 + 1] = 255;
						rgba[i * 4 + 2] = 255;
						rgba[i * 4 + 3] = data[i];
					}
					return rgba;

				case Bgra4444:
					for (int i = 0; i < pixels && i * 2 + 1 < data.Length; i++)
					{
						ushort v = (ushort)(data[i * 2] | (data[i * 2 + 1] << 8));
						rgba[i * 4] = Expand4((v >> 8) & 0xF);
						rgba[i * 4 + 1] = Expand4((v >> 4) & 0xF);
						rgba[i * 4 + 2] = Expand4(v & 0xF);
						rgba[i * 4 + 3] = Expand4((v >> 12) & 0xF);
					}
					return rgba;

				case Bgra5551:
					for (int i = 0; i < pixels && i * 2 + 1 < data.Length; i++)
					{
						ushort v = (ushort)(data[i * 2] | (data[i * 2 + 1] << 8));
						rgba[i * 4] = Expand5((v >> 10) & 0x1F);
						rgba[i * 4 + 1] = Expand5((v >> 5) & 0x1F);
						rgba[i * 4 + 2] = Expand5(v & 0x1F);
						rgba[i * 4 + 3] = (byte)(((v >> 15) & 1) != 0 ? 255 : 0);
					}
					return rgba;

				case Bgr565:
					for (int i = 0; i < pixels && i * 2 + 1 < data.Length; i++)
					{
						ushort v = (ushort)(data[i * 2] | (data[i * 2 + 1] << 8));
						rgba[i * 4] = Expand5((v >> 11) & 0x1F);
						rgba[i * 4 + 1] = Expand6((v >> 5) & 0x3F);
						rgba[i * 4 + 2] = Expand5(v & 0x1F);
						rgba[i * 4 + 3] = 255;
					}
					return rgba;

				default:
					throw new NotSupportedException("cannot decode surface format " + Name(format));
			}
		}

		private static byte Expand4(int v) => (byte)((v << 4) | v);
		private static byte Expand5(int v) => (byte)((v << 3) | (v >> 2));
		private static byte Expand6(int v) => (byte)((v << 2) | (v >> 4));
	}
}
