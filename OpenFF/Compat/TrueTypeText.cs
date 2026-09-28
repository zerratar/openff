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
//   --title-font=<file>  the title screen's face (default: Windows' Times New Roman)
//
// Faces are looked for in the Steam install the content came from (it ships Arial,
// Arial Unicode, TBUDRGothic, Eye glass Condensed, unifont), then beside our Content in
// a Fonts folder, then Windows' own. Several are loaded; FontStashSharp falls back from
// one to the next per glyph, which is what covers the Japanese and Korean text with a
// Latin face first.
//
// The title screen's commands are a second face: Steam draws them as pictures in a serif,
// and here they are text, in Times New Roman - Windows' own copy, read where it is, since
// the face is not ours to ship. TitleFace switches to it for what the title draws; the
// game's faces stand behind it for any glyph it lacks, Steam's Japanese face (TBUDRGothic)
// first, so Japanese in the title's face is drawn in the game's own Japanese lettering.
//
// A menu text's own lettering (Crystal Style Sheets' font-family, font-weight, font-style,
// letter-spacing, text-decoration: OpenFF.Content.MenuText) is Style while it is measured and
// drawn: a face of the mod's own (a file beside its layout), serif (Times New Roman), monospace
// (Consolas), a face of Windows' by name - each read where it is, with the game's faces behind it
// for the glyphs it lacks - and its bold and italic where Windows has them, else drawn heavier
// (twice, a little apart) or slanted (Graphics.DrawString skews the batch).

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OpenFF.Client
{
	// MonoGame's types by name: the engine's OpenFF.Game, GameTime, Color and vectors sit a namespace up.
	using Game = Microsoft.Xna.Framework.Game;
	using Color = Microsoft.Xna.Framework.Color;
	using Vector2 = Microsoft.Xna.Framework.Vector2;

	internal static class TrueTypeText
	{
		private static FontSystem _system;
		private static float _viewportScale = 1f;
		private static readonly Dictionary<int, DynamicSpriteFont> _fonts = new Dictionary<int, DynamicSpriteFont>();
		private static FontSystem _titleSystem;
		private static readonly Dictionary<int, DynamicSpriteFont> _titleFonts = new Dictionary<int, DynamicSpriteFont>();
		private static readonly List<string> _loaded = new List<string>();

		/// <summary>Whether text is drawn from TrueType. False until Initialise finds a face, or when --text=atlas.</summary>
		public static bool Enabled { get; private set; }

		/// <summary>The faces in use, for the log and the about text.</summary>
		public static IReadOnlyList<string> Faces => _loaded;

		/// <summary>
		/// Whether the title's face was found: the title's commands are drawn as text in it. Asking loads
		/// the faces if no text has been drawn yet - a run started on the title (--start=title, as a
		/// restart is) builds its commands before the first text pass would have.
		/// </summary>
		public static bool HasTitleFace
		{
			get
			{
				Initialise(null);
				return Enabled && _titleSystem != null;
			}
		}

		/// <summary>While true, text is drawn and measured in the title's face (ModListScreen sets it around the title's labels).</summary>
		public static bool TitleFace;

		/// <summary>The lettering of what is measured and drawn now - a menu text's style - or null for the game's own (the menu's text pass sets it around each styled text).</summary>
		public static OpenFF.Content.MenuText Style;

		/// <summary>A shadow's blur being drawn (menu units): the face's blurry effect.</summary>
		public static float Blur;

		// Faces by key ("game", "title", a file's path) and their fonts by key and size.
		private static readonly Dictionary<string, FontSystem> _faces = new Dictionary<string, FontSystem>(StringComparer.OrdinalIgnoreCase);
		private static readonly HashSet<string> _missing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		private static readonly Dictionary<(string, int), DynamicSpriteFont> _styled = new Dictionary<(string, int), DynamicSpriteFont>();
		private static readonly Dictionary<string, (string Key, bool Bold, bool Italic)> _resolved = new Dictionary<string, (string, bool, bool)>(StringComparer.Ordinal);

		/// <summary>
		/// Glyph size relative to the atlases. The atlas glyphs were drawn a little
		/// larger than their nominal size; without this, TrueType at 12 reads small.
		/// </summary>
		public const float SizeFactor = 2f;//1.15f;

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
				_system = NewSystem();
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
				if (Enabled)
				{
					LoadTitleFace();
				}
			}
			catch (Exception ex)
			{
				Log.Write(LogChannel.General, "text: TrueType unavailable (" + ex.Message + "); atlas fonts");
				Enabled = false;
			}
		}

		private static FontSystem NewSystem()
		{
			return new FontSystem(new FontSystemSettings
			{
				// Rasterised at up to 4x the nominal size, so a 16px glyph in a
				// fullscreen window has real pixels behind it.
				TextureWidth = 2048,
				TextureHeight = 2048,
				PremultiplyAlpha = true,
				KernelWidth = 0,
				KernelHeight = 0
			});
		}

		/// <summary>The title's face, then the game's behind it; none found, the title keeps its pictures.</summary>
		private static void LoadTitleFace()
		{
			string file = Options.Get("title-font");
			if (string.IsNullOrEmpty(file))
			{
				string windows = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
				file = string.IsNullOrEmpty(windows) ? null : Path.Combine(windows, "times.ttf");
			}
			if (string.IsNullOrEmpty(file) || !File.Exists(file))
			{
				Log.Write(LogChannel.General, "text: no title face (" + (file ?? "no fonts folder") + "); the title keeps its pictures");
				return;
			}
			try
			{
				FontSystem system = NewSystem();
				system.AddFont(File.ReadAllBytes(file));
				foreach (string fallback in _loaded.OrderBy(f => Path.GetFileName(f).StartsWith("TBUDRGo", StringComparison.OrdinalIgnoreCase) ? 0 : 1))
				{
					system.AddFont(File.ReadAllBytes(fallback));
				}
				_titleSystem = system;
				Log.Write(LogChannel.General, "text: title face " + Path.GetFileName(file));
			}
			catch (Exception ex)
			{
				Log.Write(LogChannel.General, "text: title face " + file + " could not be loaded: " + ex.Message);
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
				_titleFonts.Clear();
				_styled.Clear();
			}
		}

		private static DynamicSpriteFont FontFor(int size)
		{
			int key = size;
			bool title = TitleFace && _titleSystem != null;
			Dictionary<int, DynamicSpriteFont> fonts = title ? _titleFonts : _fonts;
			if (!fonts.TryGetValue(key, out DynamicSpriteFont font))
			{
				font = (title ? _titleSystem : _system).GetFont(size * SizeFactor * _viewportScale);
				fonts[key] = font;
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
			// Before a face is loaded (the overlay's first frames come before the game's first text) the atlas glyphs' width stands in.
			if (_system == null) return GlobalScope.m_Graphics != null ? GlobalScope.m_Graphics.StringWidth(text, size) : text.Length * size * 0.5f;
			if (Style != null)
			{
				(DynamicSpriteFont font, bool bold, bool _) = Styled(size);
				float spacing = Spacing(1f);
				float width = font.MeasureString(Clean(text), null, spacing).X / _viewportScale;
				return width + (bold ? BoldOffset(size) : 0f);
			}
			return FontFor(size).MeasureString(Clean(text)).X / _viewportScale;
		}

		// ---- a menu text's own lettering ----

		/// <summary>The styled font for a size: the face the style's families come to, and whether bold and italic are to be made up.</summary>
		private static (DynamicSpriteFont Font, bool FakeBold, bool FakeItalic) Styled(int size)
		{
			(string key, bool bold, bool italic) = Resolve(Style);
			FontSystem system = Face(key) ?? _system;
			if (!_styled.TryGetValue((key, size), out DynamicSpriteFont font))
			{
				font = system.GetFont(size * SizeFactor * _viewportScale);
				_styled[(key, size)] = font;
			}
			return (font, Style.Bold && !bold, Style.Italic && !italic);
		}

		/// <summary>Whether the text drawn now is to be slanted by its batch (italic with no italic face), and by how much (x per y).</summary>
		public static bool Slanted(out float slant)
		{
			slant = 0.2f;
			return Style != null && Style.Italic && _system != null && !Resolve(Style).Italic;
		}

		/// <summary>The letter spacing in the font's own pixels (the style's is in menu units), for a draw at a scale.</summary>
		private static float Spacing(float scaleX)
		{
			if (Style == null || Style.LetterSpacing == 0) return 0f;
			float textSpace = Style.LetterSpacing * 800f / Math.Max(1, GlobalScope.LCD_WIDTH);
			return textSpace * _viewportScale / Math.Max(0.01f, scaleX);
		}

		/// <summary>How far apart a made-up bold's two draws are, in text-space units.</summary>
		private static float BoldOffset(int size) => Math.Max(0.5f, size * 0.045f);

		/// <summary>The face a style's families come to: the first that is found (the game's when none is), and whether it is bold / italic itself.</summary>
		private static (string Key, bool Bold, bool Italic) Resolve(OpenFF.Content.MenuText style)
		{
			string id = string.Join("|", style.Families) + (style.Bold ? "|b" : "") + (style.Italic ? "|i" : "");
			if (_resolved.TryGetValue(id, out (string, bool, bool) known)) return known;
			(string, bool, bool) found = ("game", false, false);
			foreach (string family in style.Families)
			{
				if (family.StartsWith("file:", StringComparison.Ordinal))
				{
					string path = family.Substring(5);
					if (Face(path) != null) { found = (path, false, false); break; }
					continue;
				}
				if (family == "game" || family == "sans-serif" || family == "system-ui") { found = ("game", false, false); break; }
				string file = WindowsFace(family == "serif" ? "times new roman" : family == "monospace" ? "consolas" : family, style.Bold, style.Italic, out bool b, out bool it);
				if (file != null && Face(file) != null) { found = (file, b, it); break; }
			}
			_resolved[id] = found;
			return found;
		}

		/// <summary>A face by key: the game's, the title's, or a file (loaded once, the game's faces behind it); null when it will not load.</summary>
		private static FontSystem Face(string key)
		{
			if (key == "game") return _system;
			if (key == "title") return _titleSystem ?? _system;
			if (_faces.TryGetValue(key, out FontSystem system)) return system;
			if (_missing.Contains(key)) return null;
			try
			{
				if (!File.Exists(key)) { _missing.Add(key); Log.Write(LogChannel.General, "text: no face " + key); return null; }
				system = NewSystem();
				system.AddFont(File.ReadAllBytes(key));
				foreach (string fallback in _loaded) system.AddFont(File.ReadAllBytes(fallback));
				_faces[key] = system;
				Log.Write(LogChannel.General, "text: face " + Path.GetFileName(key) + " for a menu's style");
				return system;
			}
			catch (Exception ex)
			{
				_missing.Add(key);
				Log.Write(LogChannel.General, "text: face " + key + " could not be loaded: " + ex.Message);
				return null;
			}
		}

		// Windows' faces by family name: the file and its bold, italic and bold italic.
		private static readonly Dictionary<string, string[]> WindowsFamilies = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
		{
			["arial"] = new[] { "arial", "arialbd", "ariali", "arialbi" },
			["times new roman"] = new[] { "times", "timesbd", "timesi", "timesbi" },
			["times"] = new[] { "times", "timesbd", "timesi", "timesbi" },
			["georgia"] = new[] { "georgia", "georgiab", "georgiai", "georgiaz" },
			["verdana"] = new[] { "verdana", "verdanab", "verdanai", "verdanaz" },
			["tahoma"] = new[] { "tahoma", "tahomabd", null, null },
			["trebuchet ms"] = new[] { "trebuc", "trebucbd", "trebucit", "trebucbi" },
			["segoe ui"] = new[] { "segoeui", "segoeuib", "segoeuii", "segoeuiz" },
			["calibri"] = new[] { "calibri", "calibrib", "calibrii", "calibriz" },
			["cambria"] = new[] { "cambria", "cambriab", "cambriai", "cambriaz" },
			["candara"] = new[] { "candara", "candarab", "candarai", "candaraz" },
			["constantia"] = new[] { "constan", "constanb", "constani", "constanz" },
			["corbel"] = new[] { "corbel", "corbelb", "corbeli", "corbelz" },
			["consolas"] = new[] { "consola", "consolab", "consolai", "consolaz" },
			["courier new"] = new[] { "cour", "courbd", "couri", "courbi" },
			["courier"] = new[] { "cour", "courbd", "couri", "courbi" },
			["lucida console"] = new[] { "lucon", null, null, null },
			["comic sans ms"] = new[] { "comic", "comicbd", "comici", "comicz" },
			["impact"] = new[] { "impact", null, null, null },
			["palatino linotype"] = new[] { "pala", "palab", "palai", "palabi" },
			["book antiqua"] = new[] { "bkant", "antquab", "antquai", "antquabi" },
			["garamond"] = new[] { "gara", "garabd", "garait", null },
			["century gothic"] = new[] { "gothic", "gothicb", "gothici", "gothicbi" },
			["franklin gothic medium"] = new[] { "framd", null, "framdit", null },
			["arial black"] = new[] { "ariblk", null, null, null },
			["sylfaen"] = new[] { "sylfaen", null, null, null },
		};

		/// <summary>
		/// A face of Windows' for a family name: its bold and italic where the family has them (b, it say which it is), else
		/// the plain one; a name not in the list is looked for as a file of that name. Null when there is none.
		/// </summary>
		private static string WindowsFace(string family, bool bold, bool italic, out bool b, out bool it)
		{
			b = it = false;
			string fonts = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
			if (string.IsNullOrEmpty(fonts)) return null;
			string File_(string name)
			{
				if (name == null) return null;
				foreach (string ext in new[] { ".ttf", ".otf", ".ttc" })
				{
					string path = Path.Combine(fonts, name + ext);
					if (File.Exists(path)) return path;
				}
				return null;
			}
			if (WindowsFamilies.TryGetValue(family.Trim(), out string[] files))
			{
				int want = (bold ? 1 : 0) + (italic ? 2 : 0);
				if (want == 3 && File_(files[3]) is string bi) { b = it = true; return bi; }
				if (italic && File_(files[2]) is string i) { it = true; return i; }
				if (bold && File_(files[1]) is string bd) { b = true; return bd; }
				return File_(files[0]);
			}
			return File_(family.Trim()) ?? File_(family.Replace(" ", ""));
		}

		/// <summary>Draws a string the way the atlas path did: top-left at (x, y), the caller's scale, rotation, origin and depth.</summary>
		public static void Draw(SpriteBatch batch, string text, float x, float y, Color color,
			float rotation, Vector2 origin, Vector2 scale, SpriteEffects flip, float depth, int size)
		{
			if (string.IsNullOrEmpty(text) || _system == null)
			{
				return;
			}
			Vector2 drawScale = new Vector2(scale.X / _viewportScale, scale.Y / _viewportScale);
			// The atlas glyphs sat a little below the line's top; keep text where it was.
			Vector2 position = new Vector2(x, y - size * 0.1f);
			if (Style != null)
			{
				(DynamicSpriteFont styled, bool bold, bool _) = Styled(size);
				TextStyle decoration = Style.Underline ? TextStyle.Underline : Style.LineThrough ? TextStyle.Strikethrough : TextStyle.None;
				FontSystemEffect effect = Blur > 0 ? FontSystemEffect.Blurry : FontSystemEffect.None;
				int amount = Blur > 0 ? Math.Max(1, (int)Math.Round(Blur * 800f / Math.Max(1, GlobalScope.LCD_WIDTH) * _viewportScale / 2)) : 0;
				float spacing = Spacing(scale.X);
				string clean = Clean(text);
				styled.DrawText(batch, clean, position, color, rotation, origin * _viewportScale, drawScale, depth, spacing, 0f, decoration, effect, amount);
				if (bold) styled.DrawText(batch, clean, position + new Vector2(BoldOffset(size) * scale.X, 0), color, rotation, origin * _viewportScale, drawScale, depth, spacing, 0f, decoration, effect, amount);
				return;
			}
			DynamicSpriteFont font = FontFor(size);
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

			// The mods' own faces first: fonts/*.ttf|*.otf in a mod's files. The first face loaded
			// is the one text is drawn from; the game's follow as fallbacks for the glyphs it lacks.
			foreach (string directory in GameArchive.Chain?.Overrides ?? Enumerable.Empty<string>())
			{
				string fonts = Path.Combine(directory, "fonts");
				if (!Directory.Exists(fonts)) continue;
				foreach (string file in Directory.EnumerateFiles(fonts).Where(f =>
					f.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".otf", StringComparison.OrdinalIgnoreCase)).OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
				{
					Add(file);
				}
			}

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
