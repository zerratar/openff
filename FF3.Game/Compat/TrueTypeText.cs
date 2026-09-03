// Text from TrueType, at the window's resolution.
//
// The phone build baked its two text sizes, 12 and 16 pixels, into 256-page SpriteFont
// atlases indexed through .glp tables: every glyph a tiny bitmap, scaled up with the
// window. The Steam build draws from TrueType instead, and so does this when a face can
// be found: the text path in GlobalScope.Graphics asks here first, and only falls back to
// the atlases when nothing is enabled.
//
// Two things keep the game's layout intact. The face is rasterised at the requested
// size times the window's scale and drawn back down by the same factor, so it is sharp
// at any window size but occupies the space a 12- or 16-pixel glyph did. And widths are
// measured through the same face at the same size, because the menus position text by
// asking how wide it is - measure and draw have to agree, or nothing lines up.
//
//   --text=atlas         the old path, for A/B
//   --font=<file|dir>    a face to use first; a directory means look for the usual names
//
// Faces are looked for in the Steam install the content came from (it ships Arial,
// Arial Unicode, TBUDRGothic, Eye glass Condensed, unifont), then beside our Content in
// a Fonts folder, then Windows' own. Several are loaded; FontStashSharp falls back from
// one to the next per glyph, which is what covers the Japanese and Korean text with a
// Latin face first.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FF3
{
	internal static class TrueTypeText
	{
		private static FontSystem _system;
		private static float _viewportScale = 1f;
		private static readonly Dictionary<int, DynamicSpriteFont> _fonts = new Dictionary<int, DynamicSpriteFont>();
		private static readonly List<string> _loaded = new List<string>();

		/// <summary>Whether text is drawn from TrueType. False until Initialise finds a face, or when --text=atlas.</summary>
		public static bool Enabled { get; private set; }

		/// <summary>The faces in use, for the log and the about text.</summary>
		public static IReadOnlyList<string> Faces => _loaded;

		/// <summary>
		/// Glyph size relative to the atlases. The atlas glyphs were drawn a little
		/// larger than their nominal size; without this, TrueType at 12 reads small.
		/// </summary>
		private const float SizeFactor = 1.15f;

		private static bool _initialised;

		/// <summary>
		/// Finds and loads the faces. Called from the text path once the content is open,
		/// because the faces are looked for in the install the content came from; a call
		/// before that returns and the next one tries again.
		/// </summary>
		public static void Initialise(GraphicsDevice device)
		{
			if (_initialised || !GameArchive.IsLoaded)
			{
				return;
			}
			_initialised = true;
			if (string.Equals(Options.Get("text"), "atlas", StringComparison.OrdinalIgnoreCase))
			{
				Log.Write(LogChannel.General, "text: atlas fonts (--text=atlas)");
				return;
			}
			List<string> files = FindFaces();
			if (files.Count == 0)
			{
				Log.Write(LogChannel.General, "text: no TrueType face found; atlas fonts");
				return;
			}
			try
			{
				_system = new FontSystem(new FontSystemSettings
				{
					// Rasterised at up to 4x the nominal size, so a 16px glyph in a
					// fullscreen window has real pixels behind it.
					TextureWidth = 2048,
					TextureHeight = 2048,
					PremultiplyAlpha = true,
					KernelWidth = 0,
					KernelHeight = 0
				});
				foreach (string file in files)
				{
					try
					{
						_system.AddFont(File.ReadAllBytes(file));
						_loaded.Add(file);
					}
					catch (Exception ex)
					{
						Log.Write(LogChannel.General, "text: could not load " + file + ": " + ex.Message);
					}
				}
				Enabled = _loaded.Count > 0;
				Log.Write(LogChannel.General, Enabled
					? "text: TrueType - " + string.Join(", ", _loaded.Select(Path.GetFileName))
					: "text: no face loaded; atlas fonts");
			}
			catch (Exception ex)
			{
				Log.Write(LogChannel.General, "text: TrueType unavailable (" + ex.Message + "); atlas fonts");
				Enabled = false;
			}
		}

		/// <summary>The window's scale over the 800x480 text space; set at the start of each text pass.</summary>
		public static void SetViewportScale(float scale)
		{
			float wanted = Math.Max(1f, Math.Min(4f, (float)Math.Ceiling(scale)));
			if (Math.Abs(wanted - _viewportScale) > 0.01f)
			{
				_viewportScale = wanted;
				_fonts.Clear();
			}
		}

		private static DynamicSpriteFont FontFor(int size)
		{
			int key = size;
			if (!_fonts.TryGetValue(key, out DynamicSpriteFont font))
			{
				font = _system.GetFont(size * SizeFactor * _viewportScale);
				_fonts[key] = font;
			}
			return font;
		}

		/// <summary>The width of a string at a nominal size, in text-space units before the caller's scale.</summary>
		public static float Width(string text, int size)
		{
			if (string.IsNullOrEmpty(text))
			{
				return 0f;
			}
			return FontFor(size).MeasureString(Clean(text)).X / _viewportScale;
		}

		/// <summary>Draws a string the way the atlas path did: top-left at (x, y), the caller's scale, rotation, origin and depth.</summary>
		public static void Draw(SpriteBatch batch, string text, float x, float y, Color color,
			float rotation, Vector2 origin, Vector2 scale, SpriteEffects flip, float depth, int size)
		{
			if (string.IsNullOrEmpty(text))
			{
				return;
			}
			DynamicSpriteFont font = FontFor(size);
			Vector2 drawScale = new Vector2(scale.X / _viewportScale, scale.Y / _viewportScale);
			// The atlas glyphs sat a little below the line's top; keep text where it was.
			Vector2 position = new Vector2(x, y - size * 0.1f);
			font.DrawText(batch, Clean(text), position, color, rotation, origin * _viewportScale, drawScale, depth);
		}

		/// <summary>The control characters the game uses as spaces.</summary>
		private static string Clean(string text)
		{
			if (text.IndexOfAny(new[] { '', ' ', '­' }) < 0)
			{
				return text;
			}
			return text.Replace('', ' ').Replace(' ', ' ').Replace('­', ' ');
		}

		/// <summary>The faces to load, most specific first.</summary>
		private static List<string> FindFaces()
		{
			List<string> found = new List<string>();
			void Add(string path)
			{
				if (!string.IsNullOrEmpty(path) && File.Exists(path)
					&& !found.Contains(path, StringComparer.OrdinalIgnoreCase))
				{
					found.Add(path);
				}
			}

			string[] usual = { "arial.ttf", "arialuni.ttf", "TBUDRGoStd-Bold.otf", "unifont.ttf" };
			string configured = Options.Get("font");
			if (!string.IsNullOrEmpty(configured))
			{
				if (Directory.Exists(configured))
				{
					foreach (string name in usual) Add(Path.Combine(configured, name));
					foreach (string file in Directory.EnumerateFiles(configured).Where(f =>
						f.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".otf", StringComparison.OrdinalIgnoreCase)))
					{
						Add(file);
					}
				}
				else
				{
					Add(configured);
				}
			}

			// The install the content came from. FF3 keeps its faces beside the executable;
			// a chain opened on EXTRACTED_DATA (FF4) has them one level up.
			string root = GameArchive.Chain?.Root;
			if (!string.IsNullOrEmpty(root))
			{
				foreach (string directory in new[] { root, Path.GetDirectoryName(root) })
				{
					if (directory == null) continue;
					foreach (string name in usual) Add(Path.Combine(directory, name));
				}
			}

			// Beside our own content, so a build that ships its own faces works without Steam.
			string content = ContentLocator.FindContentRoot();
			if (content != null)
			{
				foreach (string name in usual) Add(Path.Combine(content, "Fonts", name));
			}

			// Windows' own Arial as the last resort: it covers Latin, and the others cover the rest.
			string windows = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
			if (!string.IsNullOrEmpty(windows) && found.Count == 0)
			{
				Add(Path.Combine(windows, "arial.ttf"));
				Add(Path.Combine(windows, "segoeui.ttf"));
			}
			return found;
		}
	}
}
