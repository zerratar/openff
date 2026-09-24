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
//
// The engine's frame is one of the game's steps, thirty a second, and the list stands until
// the next; a display that draws more often than that is shown the list part of the way from
// the step before's, each thing sliding from where it was to where it is, as the game's own
// frame is (the host's ModDrawBlend). A thing is followed from one list to the next by what it
// draws - its kind, its picture and the part of it, its words, size and colour - and among
// things that draw alike (the bar over every foe) by where it is, the nearest taken. Where
// alike things stand close enough to be taken for one another (the digits of two damage
// numbers, two tags the same width side by side), Group says which is which: the commands
// after it belong to that one thing, followed in the order drawn.

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

	public enum DrawKind { Text, Rect, Line, Sprite, Banner }

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
		/// <summary>The thing on the screen the command is part of (DrawList.Group); null for none.</summary>
		public object Group;
	}

	public sealed class DrawList
	{
		public const float ScreenWidth = 800f;
		public const float ScreenHeight = 480f;

		private readonly List<DrawCommand> _commands = new List<DrawCommand>();
		private object _group;

		/// <summary>The host's texture loader (a PNG, JPG or BMP file); set by the host.</summary>
		public Func<string, Texture> TextureLoader { get; set; }

		/// <summary>The host's texture loader for picture bytes already in hand (a PNG out of an archive), cached under a key; set by the host.</summary>
		public Func<string, byte[], Texture> TextureBytesLoader { get; set; }

		/// <summary>The host's text measure, in screen units at a size; set by the host.</summary>
		public Func<string, int, float> TextMeasure { get; set; }

		public IReadOnlyList<DrawCommand> Commands => _commands;

		/// <summary>
		/// The commands after this call, until the next call or the end of the frame, are one thing on the screen -
		/// a damage number, a tag over a character: whatever stands for it while it lasts (the object itself, or a
		/// number the mod keeps for it; Equals decides). Between two of the game's steps a thing slides from where
		/// it was to where it is, and things that draw alike are then never taken for one another. Null ends the group.
		/// </summary>
		public void Group(object thing) => _group = thing;

		/// <summary>Text at a position, in the game's own font. Sizes as the game's: 12 small, 16 normal.</summary>
		public void Text(string text, float x, float y, Color color, int size = 12)
		{
			if (string.IsNullOrEmpty(text)) return;
			_commands.Add(new DrawCommand { Kind = DrawKind.Text, X = x, Y = y, Text = text, Color = color, Size = size, Group = _group });
		}

		public float MeasureText(string text, int size = 12)
		{
			return TextMeasure == null || string.IsNullOrEmpty(text) ? 0f : TextMeasure(text, size);
		}

		public void Rect(float x, float y, float w, float h, Color color, bool filled = true)
		{
			_commands.Add(new DrawCommand { Kind = DrawKind.Rect, X = x, Y = y, W = w, H = h, Color = color, Filled = filled, Group = _group });
		}

		public void Line(float x1, float y1, float x2, float y2, Color color, float thickness = 1f)
		{
			_commands.Add(new DrawCommand { Kind = DrawKind.Line, X = x1, Y = y1, X2 = x2, Y2 = y2, Color = color, W = thickness, Group = _group });
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
				Group = _group,
			});
		}

		/// <summary>
		/// The game's own place-name window - the frame a map's name arrives in at the top of the screen - with this
		/// text in it, for this frame: call it every frame the banner should stay (as Text), and it goes when you stop.
		/// Over the field or a battle (FF3); one at a time, the last call's text.
		/// </summary>
		public void Banner(string text)
		{
			if (string.IsNullOrEmpty(text)) return;
			_commands.Add(new DrawCommand { Kind = DrawKind.Banner, Text = text, Color = Color.White, Group = _group });
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

		/// <summary>Host entry: the engine's next frame is about to draw; the last one's commands go, and any group with them.</summary>
		public void Clear()
		{
			_commands.Clear();
			_group = null;
		}
	}

	public static partial class Game
	{
		/// <summary>Immediate-mode drawing over the finished frame in 800x480 screen units: text, rectangles, lines, sprites.</summary>
		public static DrawList Draw { get; } = new DrawList();
	}
}
