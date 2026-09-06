// The game's 2D art.
//
// The NDS formats these are named after - NCGR is Nitro character graphics, NCBR its
// bitmap cousin - hold indexed tiles and need a palette to mean anything. These do
// not: the phone port replaced every one of them with a PNG and kept the extension.
// All 542 of them, checked.
//
// Which makes this the one kind of art that can be looked at and replaced today, with
// no decoder in between. The game reads them through its own PNG path, so a PNG
// dropped in the override is a PNG the game will draw.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Crystal.Editor
{
	internal sealed class ImageInfo
	{
		public string Name { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }

		/// <summary>Bits per channel, from the PNG header.</summary>
		public int Depth { get; set; }

		/// <summary>"palette", "rgba", "grey" - PNG colour type, in words.</summary>
		public string Colour { get; set; }

		public int Bytes { get; set; }
		public bool Overridden { get; set; }

		/// <summary>Set when the file is not a PNG after all.</summary>
		public string Problem { get; set; }
	}

	internal static class Images
	{
		private static readonly byte[] Magic = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

		/// <summary>The extensions holding pictures, whatever their names suggest.</summary>
		public static readonly string[] Extensions = { ".NCGR", ".NCBR" };

		public static List<ImageInfo> List(Workspace workspace)
		{
			List<ImageInfo> images = new List<ImageInfo>();
			foreach (WorkspaceEntry entry in workspace.List(Extensions))
			{
				ImageInfo info = new ImageInfo
				{
					Name = entry.Name,
					Overridden = entry.Overridden
				};
				try
				{
					Describe(workspace.Read(entry.Name), info);
				}
				catch (Exception ex)
				{
					info.Problem = ex.Message;
				}
				images.Add(info);
			}
			return images;
		}

		public static ImageInfo Describe(byte[] data, ImageInfo info = null)
		{
			info = info ?? new ImageInfo();
			info.Bytes = data.Length;

			if (data.Length < 26 || !data.Take(Magic.Length).SequenceEqual(Magic))
			{
				throw new InvalidDataException("not a PNG");
			}

			// IHDR is always the first chunk: width, height, depth, colour type.
			info.Width = ReadBigEndian(data, 16);
			info.Height = ReadBigEndian(data, 20);
			info.Depth = data[24];
			info.Colour = ColourName(data[25]);
			return info;
		}

		private static string ColourName(int type)
		{
			switch (type)
			{
				case 0: return "grey";
				case 2: return "rgb";
				case 3: return "palette";
				case 4: return "grey+alpha";
				case 6: return "rgba";
				default: return "type " + type.ToString(CultureInfo.InvariantCulture);
			}
		}

		private static int ReadBigEndian(byte[] data, int at)
		{
			return (data[at] << 24) | (data[at + 1] << 16) | (data[at + 2] << 8) | data[at + 3];
		}
	}
}
