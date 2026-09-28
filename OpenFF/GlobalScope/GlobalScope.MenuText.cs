using System;

// PORT: a menu text's own lettering (Crystal Style Sheets: OpenFF.Content.MenuText) in the text pass.
// Its shadows and its outline are draws of their own under the text - the shadows in the order CSS
// stacks them (the first on top), blurred by the face's blurry effect; the outline the text drawn
// round about in its colour - and the text last, in its face, weight, slant and spacing (TrueTypeText
// reads the style while it is drawn). With no text-shadow said, the game's own drop shadow stays.
internal static partial class GlobalScope
{
	internal static void DrawStyledText(TEXT_DATA t, int x, int y, uint colour, int alphaOf)
	{
		OpenFF.Content.MenuText style = t.style;
		if (font[t.size] == null) font[t.size] = new Font(t.size);
		Font f = font[t.size];
		float fade = alphaOf / 255f;
		OpenFF.Client.TrueTypeText.Style = style;
		try
		{
			if (style.Shadows == null)
			{
				if ((t.flags & 0x4000) != 0) f.drawString(t.text, x + 1, y + 1, (int)Premultiplied(0x000000FF, fade), 1);
			}
			else
			{
				for (int i = style.Shadows.Count - 1; i >= 0; i--)
				{
					OpenFF.Content.MenuText.Shadow shadow = style.Shadows[i];
					OpenFF.Client.TrueTypeText.Blur = shadow.Blur;
					f.drawString(t.text, x + shadow.X, y + shadow.Y, (int)Premultiplied(shadow.Colour, fade), 1);
					OpenFF.Client.TrueTypeText.Blur = 0;
				}
			}
			if (style.StrokeWidth > 0)
			{
				// The outline: the text round about at the stroke's width (and half of it, for a wide one), under the text.
				int stroke = (int)Premultiplied(style.StrokeColour, fade);
				float w = style.StrokeWidth;
				int steps = w > 1.5f ? 16 : 8;
				for (int ring = w > 1.5f ? 2 : 1; ring >= 1; ring--)
				{
					float r = w * ring / (w > 1.5f ? 2 : 1);
					for (int k = 0; k < steps; k++)
					{
						double a = k * Math.PI * 2 / steps;
						f.drawString(t.text, x + (float)Math.Cos(a) * r, y + (float)Math.Sin(a) * r, stroke, 1);
					}
				}
			}
			f.drawString(t.text, x, y, (int)colour, 1);
		}
		finally
		{
			OpenFF.Client.TrueTypeText.Style = null;
			OpenFF.Client.TrueTypeText.Blur = 0;
		}
	}

	/// <summary>One of the game's text palette's colours (0xRRGGBBAA) by its index (dgs.TXT_COLOR), for the styles' colours to move to and from its words.</summary>
	internal static uint? TextPaletteColour(int index) => index >= 0 && index < textColor.Length ? (uint)textColor[index] : (uint?)null;

	/// <summary>A colour (0xRRGGBBAA) faded as the text is, premultiplied - the text batch blends premultiplied.</summary>
	private static uint Premultiplied(uint rgba, float fade)
	{
		float a = (rgba & 0xFF) / 255f * Math.Clamp(fade, 0, 1);
		uint Ch(int shift) => (uint)Math.Clamp((int)Math.Round((rgba >> shift & 0xFF) * a), 0, 255);
		return Ch(24) << 24 | Ch(16) << 16 | Ch(8) << 8 | (uint)Math.Clamp((int)Math.Round(a * 255), 0, 255);
	}
}
