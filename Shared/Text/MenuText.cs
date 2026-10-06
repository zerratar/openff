// A menu text's own lettering (Crystal Style Sheets): its shadow, outline, face, weight, slant,
// spacing and case, as CSS has them.
//
//   #title   { text-shadow: 2px 2px 3px #000000c0; font-family: serif; font-weight: bold; letter-spacing: 1px; }
//   .hint    { font-style: italic; text-transform: uppercase; }
//   .banner  { -ff-text-stroke: 1px #203060; text-shadow: none; }
//
//   text-shadow         none (not even the game's own drop shadow), or x y [blur] colour, ... (menu units; the
//                       first on top). Left unsaid, the game's own shadow stays.
//   font-family         url("fonts/x.ttf") (a face beside the layout), serif (Times New Roman), sans-serif or game
//                       (the game's own face), monospace (Consolas), or a face of Windows' by name ("Georgia") - in
//                       order, the first found
//   font-weight         normal, bold, or 100..900 (600 and over bold): the face's bold if Windows has one, else drawn heavier
//   font-style          normal, italic (oblique): the face's italic if Windows has one, else slanted
//   letter-spacing      menu units between the letters (normal: 0)
//   line-height         a multi-line text's lines this far apart: menu units, or a number times the size (normal)
//   text-decoration     none, underline, line-through
//   text-transform      none, uppercase, lowercase, capitalize
//   -ff-text-stroke     width colour: an outline round the letters (-webkit-text-stroke too)
//
// All of them are inherited, as CSS inherits them. MenuStyles bakes them into the frame's <textstyle>;
// the client draws them (ModMenus: the text's canvas; the text pass: the shadows and the outline as
// draws of their own, the rest by the face - TrueTypeText), and Crystal's preview draws the same.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace OpenFF.Content
{
	internal sealed class MenuText
	{
		public static readonly string[] Properties =
		{
			"text-shadow", "font-family", "font-weight", "font-style", "letter-spacing", "line-height",
			"text-decoration", "text-decoration-line", "text-transform", "-ff-text-stroke", "-webkit-text-stroke"
		};

		public struct Shadow
		{
			public float X, Y, Blur;
			public uint Colour;   // 0xRRGGBBAA
		}

		/// <summary>The declarations it was made from: equal texts are equal styles.</summary>
		public string Key;
		/// <summary>The shadows, the first on top; null when the style does not say (the game's own shadow stays); empty for none.</summary>
		public List<Shadow> Shadows;
		/// <summary>The faces in order: "url:path" (a file; the host resolves it), serif, sans-serif, game, monospace, or a name.</summary>
		public List<string> Families = new List<string>();
		public bool Bold, Italic;
		public float LetterSpacing;
		/// <summary>Menu units from one line to the next, or a factor of the size (LineFactor); null for the game's own.</summary>
		public float? LineHeight;
		public bool LineFactor;
		public bool Underline, LineThrough;
		public string Transform;
		public float StrokeWidth;
		public uint StrokeColour = 0x000000FF;

		public bool Plain => Shadows == null && Families.Count == 0 && !Bold && !Italic && LetterSpacing == 0 && LineHeight == null
			&& !Underline && !LineThrough && Transform == null && StrokeWidth <= 0;

		/// <summary>The style its declarations make ("text-shadow: 1px 1px #000; font-weight: bold"); null when they say nothing it draws.</summary>
		public static MenuText Parse(string declarations)
		{
			if (string.IsNullOrWhiteSpace(declarations)) return null;
			MenuText t = new MenuText { Key = declarations.Trim() };
			foreach (KeyValuePair<string, string> d in MenuStyles.Declarations(declarations))
			{
				string v = d.Value.Trim();
				string low = v.ToLowerInvariant();
				switch (d.Key)
				{
					case "text-shadow":
						t.Shadows = new List<Shadow>();
						if (low == "none") break;
						foreach (string item in MenuAnimation.CommaList(v))
						{
							List<float> numbers = new List<float>();
							uint? colour = null;
							foreach (string word in MenuAnimation.Words(item))
							{
								if (Length(word) is float n) numbers.Add(n);
								else colour = Colour(word) ?? colour;
							}
							if (numbers.Count < 2) continue;
							t.Shadows.Add(new Shadow { X = numbers[0], Y = numbers[1], Blur = numbers.Count > 2 ? Math.Max(0, numbers[2]) : 0, Colour = colour ?? 0x000000FF });
						}
						break;
					case "font-family":
						t.Families.Clear();
						foreach (string item in MenuAnimation.CommaList(v))
						{
							(string kind, string path) = MenuBackground.ParseImage(item);
							if (kind == "url") t.Families.Add("url:" + path);
							else
							{
								string name = item.Trim().Trim('"', '\'').Trim();
								if (name.Length > 0) t.Families.Add(name.ToLowerInvariant());
							}
						}
						break;
					case "font-weight":
						t.Bold = low == "bold" || low == "bolder" || (int.TryParse(low, NumberStyles.Integer, CultureInfo.InvariantCulture, out int w) && w >= 600);
						break;
					case "font-style":
						t.Italic = low.StartsWith("italic", StringComparison.Ordinal) || low.StartsWith("oblique", StringComparison.Ordinal);
						break;
					case "letter-spacing":
						t.LetterSpacing = low == "normal" ? 0 : Length(low) ?? 0;
						break;
					case "line-height":
						if (low == "normal") { t.LineHeight = null; break; }
						if (float.TryParse(low, NumberStyles.Float, CultureInfo.InvariantCulture, out float factor)) { t.LineHeight = factor; t.LineFactor = true; }
						else if (low.EndsWith("%", StringComparison.Ordinal) && float.TryParse(low.TrimEnd('%'), NumberStyles.Float, CultureInfo.InvariantCulture, out float pc)) { t.LineHeight = pc / 100; t.LineFactor = true; }
						else if (Length(low) is float px) { t.LineHeight = px; t.LineFactor = false; }
						break;
					case "text-decoration":
					case "text-decoration-line":
						t.Underline = low.Contains("underline");
						t.LineThrough = low.Contains("line-through");
						break;
					case "text-transform":
						t.Transform = low == "uppercase" || low == "lowercase" || low == "capitalize" ? low : null;
						break;
					case "-ff-text-stroke":
					case "-webkit-text-stroke":
						t.StrokeWidth = 0;
						foreach (string word in MenuAnimation.Words(v))
						{
							if (Length(word) is float n) t.StrokeWidth = Math.Max(0, n);
							else if (Colour(word) is uint c) t.StrokeColour = c;
						}
						break;
				}
			}
			return t.Plain ? null : t;
		}

		/// <summary>The text as the style's case has it.</summary>
		public string Cased(string text)
		{
			if (string.IsNullOrEmpty(text) || Transform == null) return text;
			switch (Transform)
			{
				case "uppercase": return text.ToUpperInvariant();
				case "lowercase": return text.ToLowerInvariant();
				case "capitalize": return Regex.Replace(text, @"(^|[\s\-(""'])(\p{Ll})", m => m.Groups[1].Value + m.Groups[2].Value.ToUpperInvariant());
			}
			return text;
		}

		/// <summary>The distance from one line to the next for a size (menu units); the game's own when the style does not say.</summary>
		public float LineStep(int size, float own)
		{
			if (LineHeight == null) return own;
			return LineFactor ? size * LineHeight.Value : LineHeight.Value;
		}

		/// <summary>12px / 12 / .5em (of 12) as menu units; null for a word that is not a length.</summary>
		public static float? Length(string text)
		{
			string t = text?.Trim().ToLowerInvariant();
			if (string.IsNullOrEmpty(t)) return null;
			float scale = 1;
			if (t.EndsWith("px", StringComparison.Ordinal)) t = t.Substring(0, t.Length - 2);
			else if (t.EndsWith("em", StringComparison.Ordinal)) { t = t.Substring(0, t.Length - 2); scale = 12; }
			return float.TryParse(t, NumberStyles.Float, CultureInfo.InvariantCulture, out float v) ? v * scale : (float?)null;
		}

		/// <summary>A colour: #hex, rgb()/rgba(), transparent, or one of the game's palette words (MenuAnimation.Palette).</summary>
		public static uint? Colour(string text)
		{
			uint? c = MenuBackground.ParseColour(text);
			if (c.HasValue) return c;
			string w = text?.Trim().ToLowerInvariant();
			return w != null && MenuAnimation.Palette != null ? MenuAnimation.Palette(w) : null;
		}
	}
}
