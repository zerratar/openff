// Drawing from a mod: text, rectangles, lines and sprites over the frame.
//
// Game.Draw collects commands during the engine's frame; the host draws them after the
// game and clears the list, so a mod draws every frame it wants something shown (the
// immediate-mode habit: draw in Update). Coordinates are screen units, 800 wide and 480
// tall whatever the window is - the same space the game's own text uses and Game.Input
// reports the pointer in. Sprites come from PNG files a mod ships: Game.Draw.LoadTexture
// with a path, usually under the mod's own folder (Mod.Directory).
//
// This is enough for a HUD, a menu, a dialogue of the mod's own, or a whole 2D game
// drawn over the field. Drawing into the 3D scene is a later layer.

using System;
using System.Collections.Generic;

namespace OpenFF
{
	public struct Color
	{
		public byte R, G, B, A;
		public Color(byte r, byte g, byte b, byte a = 255) { R = r; G = g; B = b; A = a; }
		public static readonly Color White = new Color(255, 255, 255);
		public static readonly Color Black = new Color(0, 0, 0);
		public static readonly Color Red = new Color(220, 50, 50);
		public static readonly Color Green = new Color(60, 200, 90);
		public static readonly Color Blue = new Color(70, 110, 230);
		public static readonly Color Yellow = new Color(250, 220, 80);
		public static readonly Color Transparent = new Color(0, 0, 0, 0);
		public Color WithAlpha(byte a) => new Color(R, G, B, a);
	}

	/// <summary>A picture the host loaded for a mod; drawn with Game.Draw.Sprite.</summary>
	public abstract class Texture
	{
		public abstract int Width { get; }
		public abstract int Height { get; }
		public string Path { get; protected set; }
	}

	public enum DrawKind { Text, Rect, Line, Sprite }

	public struct DrawCommand
	{
		public DrawKind Kind;
		public float X, Y, W, H;
		public float X2, Y2;
		public Color Color;
		public string Text;
		public int Size;
		public bool Filled;
		public Texture Texture;
		public float Rotation;
		public float SrcX, SrcY, SrcW, SrcH;
		public float OriginX, OriginY;
	}

	public sealed class DrawList
	{
		public const float ScreenWidth = 800f;
		public const float ScreenHeight = 480f;

		private readonly List<DrawCommand> _commands = new List<DrawCommand>();

		/// <summary>The host's texture loader (a PNG, JPG or BMP file); set by the host.</summary>
		public Func<string, Texture> TextureLoader { get; set; }

		/// <summary>The host's texture loader for picture bytes already in hand (a PNG out of an archive), cached under a key; set by the host.</summary>
		public Func<string, byte[], Texture> TextureBytesLoader { get; set; }

		/// <summary>The host's text measure, in screen units at a size; set by the host.</summary>
		public Func<string, int, float> TextMeasure { get; set; }

		public IReadOnlyList<DrawCommand> Commands => _commands;

		/// <summary>Text at a position, in the game's own font. Sizes as the game's: 12 small, 16 normal.</summary>
		public void Text(string text, float x, float y, Color color, int size = 12)
		{
			if (string.IsNullOrEmpty(text)) return;
			_commands.Add(new DrawCommand { Kind = DrawKind.Text, X = x, Y = y, Text = text, Color = color, Size = size });
		}

		public float MeasureText(string text, int size = 12)
		{
			return TextMeasure == null || string.IsNullOrEmpty(text) ? 0f : TextMeasure(text, size);
		}

		public void Rect(float x, float y, float w, float h, Color color, bool filled = true)
		{
			_commands.Add(new DrawCommand { Kind = DrawKind.Rect, X = x, Y = y, W = w, H = h, Color = color, Filled = filled });
		}

		public void Line(float x1, float y1, float x2, float y2, Color color, float thickness = 1f)
		{
			_commands.Add(new DrawCommand { Kind = DrawKind.Line, X = x1, Y = y1, X2 = x2, Y2 = y2, Color = color, W = thickness });
		}

		/// <summary>A texture (or part of it) drawn into a rectangle, tinted, turned about its centre.</summary>
		public void Sprite(Texture texture, float x, float y, float w, float h, Color? tint = null, float rotation = 0f,
			float srcX = 0, float srcY = 0, float srcW = 0, float srcH = 0)
		{
			if (texture == null) return;
			_commands.Add(new DrawCommand
			{
				Kind = DrawKind.Sprite, Texture = texture, X = x, Y = y, W = w, H = h, Color = tint ?? Color.White, Rotation = rotation,
				SrcX = srcX, SrcY = srcY, SrcW = srcW <= 0 ? texture.Width : srcW, SrcH = srcH <= 0 ? texture.Height : srcH,
			});
		}

		/// <summary>Loads a picture from a file; null (and a warning) when it cannot.</summary>
		public Texture LoadTexture(string path)
		{
			if (TextureLoader == null)
			{
				Game.Warn("Draw.LoadTexture: the host has no texture loader");
				return null;
			}
			try { return TextureLoader(path); }
			catch (Exception ex) { Game.Warn("Draw.LoadTexture " + path + ": " + ex.Message); return null; }
		}

		/// <summary>A picture from bytes (PNG, JPG or BMP), cached under <paramref name="key"/>; null (and a warning) when it cannot.</summary>
		public Texture LoadTexture(string key, byte[] data)
		{
			if (TextureBytesLoader == null)
			{
				Game.Warn("Draw.LoadTexture: the host has no texture loader");
				return null;
			}
			if (data == null || data.Length == 0) return null;
			try { return TextureBytesLoader(key, data); }
			catch (Exception ex) { Game.Warn("Draw.LoadTexture " + key + ": " + ex.Message); return null; }
		}

		/// <summary>Host entry: the frame's commands were drawn.</summary>
		public void Clear() => _commands.Clear();
	}

	public static partial class Game
	{
		/// <summary>Immediate-mode drawing over the finished frame in 800x480 screen units: text, rectangles, lines, sprites.</summary>
		public static DrawList Draw { get; } = new DrawList();
	}
}
