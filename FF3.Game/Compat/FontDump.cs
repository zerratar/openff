// Dumps SpriteFont pages as they are loaded, so the glyph atlases can be inspected.
//
// The game renders text as one tinted SpriteBatch.DrawString per glyph, with no
// outline pass anywhere in the code, so any outline has to be baked into these
// atlases. Enable with FF3_DUMP_FONTS=<directory>.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Graphics;

namespace FF3
{
	internal static class FontDump
	{
		private static readonly string _directory =
			Environment.GetEnvironmentVariable("FF3_DUMP_FONTS");

		private static readonly HashSet<string> _done = new HashSet<string>(StringComparer.Ordinal);

		public static bool Enabled => !string.IsNullOrEmpty(_directory);

		public static void Dump(string name, SpriteFont font)
		{
			if (!Enabled || font == null)
			{
				return;
			}
			lock (_done)
			{
				if (!_done.Add(name))
				{
					return;
				}
			}

			try
			{
				Directory.CreateDirectory(_directory);
				Texture2D texture = font.Texture;

				// Report the surface format: an Alpha8 or Bgra4444 atlas carries no colour
				// of its own, which would mean the tint is the only thing setting the
				// glyph colour and a baked outline could not survive.
				Log.Write(LogChannel.Content, string.Format(
					"font {0}: {1}x{2} {3}, lineSpacing={4}, glyphs={5}",
					name, texture.Width, texture.Height, texture.Format,
					font.LineSpacing, font.Characters.Count));

				using FileStream file = File.Create(Path.Combine(_directory, name + ".png"));
				texture.SaveAsPng(file, texture.Width, texture.Height);
			}
			catch (Exception ex)
			{
				Log.Write(LogChannel.Content, "font dump failed for " + name + ": " + ex.Message);
			}
		}
	}
}
