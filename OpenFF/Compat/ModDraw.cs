// Draws what mods asked Game.Draw to draw, over the frame.
//
// A DrawableGameComponent after the game and the mod list, before the debug overlay and
// the screenshot capture. Text goes through the game's own font (TrueType, the 800x480
// text space); rectangles, lines and sprites through a SpriteBatch scaled to the same
// space. Textures a mod loads are Texture2Ds wrapped for the engine; they live until the
// device goes.
//
// The list is the engine's step's, and stands until the next; between two steps, with
// smoothing on, what is drawn is ModDrawBlend's - each command part of the way from where
// the step before had it, as the game's frame under it is drawn. Places are drawn as they
// come, in fractions of a pixel, so a slide is not rounded into a shake.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OpenFF.Client
{
	// MonoGame's types by name: the engine's OpenFF.Game, GameTime, Color and vectors sit a namespace up.
	using Game = Microsoft.Xna.Framework.Game;
	using GameTime = Microsoft.Xna.Framework.GameTime;
	using Color = Microsoft.Xna.Framework.Color;
	using Vector2 = Microsoft.Xna.Framework.Vector2;

	internal sealed class ModDraw : DrawableGameComponent
	{
		private const float TextSpaceWidth = 800f;
		private const float TextSpaceHeight = 480f;

		private SpriteBatch _batch;
		private Texture2D _pixel;
		private readonly Dictionary<string, ModTexture> _textures = new Dictionary<string, ModTexture>(StringComparer.OrdinalIgnoreCase);

		private sealed class ModTexture : OpenFF.Texture
		{
			public Texture2D Texture2D;
			public ModTexture(string path, Texture2D texture) { Path = path; Texture2D = texture; }
			public override int Width => Texture2D?.Width ?? 0;
			public override int Height => Texture2D?.Height ?? 0;
		}

		private ModDraw(Game game)
			: base(game)
		{
			// Over the game, under the debug overlay (int.MaxValue - 3) and the mod list (- 4).
			DrawOrder = int.MaxValue - 5;
		}

		public static void Attach(Game game)
		{
			ModDraw draw = new ModDraw(game);
			game.Components.Add(draw);
			OpenFF.Game.Draw.TextureLoader = draw.Load;
			OpenFF.Game.Draw.TextureBytesLoader = draw.LoadBytes;
			OpenFF.Game.Draw.TextMeasure = (text, size) => TrueTypeText.Width(text, size);
		}

		private OpenFF.Texture Load(string path)
		{
			string full = Path.GetFullPath(path);
			if (_textures.TryGetValue(full, out ModTexture cached) && cached.Texture2D != null && !cached.Texture2D.IsDisposed)
			{
				return cached;
			}
			using FileStream stream = File.OpenRead(full);
			Texture2D texture = Texture2D.FromStream(GraphicsDevice, stream);
			ModTexture wrapped = new ModTexture(full, texture);
			_textures[full] = wrapped;
			return wrapped;
		}

		private OpenFF.Texture LoadBytes(string key, byte[] data)
		{
			string full = "bytes:" + key;
			if (_textures.TryGetValue(full, out ModTexture cached) && cached.Texture2D != null && !cached.Texture2D.IsDisposed)
			{
				return cached;
			}
			using MemoryStream stream = new MemoryStream(data, false);
			Texture2D texture = Texture2D.FromStream(GraphicsDevice, stream);
			ModTexture wrapped = new ModTexture(full, texture);
			_textures[full] = wrapped;
			return wrapped;
		}

		public override void Draw(GameTime gameTime)
		{
			// The list stands until the engine's next step refills it (EngineHost.Tick keeps it for ModDrawBlend,
			// then clears it), so every display frame between two of the game's steps draws it - with smoothing on,
			// part of the way from the step before's, which is still drawn early on when this step's is empty.
			OpenFF.DrawList list = OpenFF.Game.Draw;
			try
			{
				if ((list.Commands.Count > 0 || ModDrawBlend.HasBefore) && !RenderTest.Active)
				{
					DrawAll(list);
				}
			}
			catch (Exception ex)
			{
				Log.First(LogChannel.General, "mod-draw", 3, () => "engine: draw failed: " + ex.GetType().Name + ": " + ex.Message);
			}
		}

		private void DrawAll(OpenFF.DrawList list)
		{
			EnsureResources();
			Viewport view = GraphicsDevice.Viewport;
			float sx = view.Width / TextSpaceWidth;
			float sy = view.Height / TextSpaceHeight;

			// The place-name window is asked for by the step's own list, whichever step's list is drawn: Banner.Tick
			// decides once a step whether it stays.
			bool anyText = false;
			IReadOnlyList<OpenFF.DrawCommand> asked = list.Commands;
			for (int i = 0; i < asked.Count; i++)
			{
				if (asked[i].Kind != OpenFF.DrawKind.Banner) continue;
				Banner.Keep(asked[i].Text);
				anyText = true;
			}
			// Where the display stands between the step before and this one, as the game's frame under it is drawn; without
			// the native renderer nothing is drawn in between (every display frame is a step's own), and neither is this.
			IReadOnlyList<OpenFF.DrawCommand> commands = ModDrawBlend.At(asked, FrameCapture.Supported ? FramePacer.Blend() : 1f);

			_batch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
			for (int i = 0; i < commands.Count; i++)
			{
				OpenFF.DrawCommand c = commands[i];
				Microsoft.Xna.Framework.Color colour = new Microsoft.Xna.Framework.Color(c.Color.R, c.Color.G, c.Color.B, c.Color.A);
				switch (c.Kind)
				{
					case OpenFF.DrawKind.Rect:
					{
						float x = c.X * sx, y = c.Y * sy, w = c.W * sx, h = c.H * sy;
						if (c.Filled)
						{
							Fill(x, y, w, h, colour);
						}
						else
						{
							// Four strips a pixel wide, just inside the rectangle's edges.
							Fill(x, y, w, 1f, colour);
							Fill(x, y + h - 1f, w, 1f, colour);
							Fill(x, y, 1f, h, colour);
							Fill(x + w - 1f, y, 1f, h, colour);
						}
						break;
					}
					case OpenFF.DrawKind.Line:
					{
						Vector2 a = new Vector2(c.X * sx, c.Y * sy);
						Vector2 b = new Vector2(c.X2 * sx, c.Y2 * sy);
						Vector2 d = b - a;
						float length = d.Length();
						if (length < 0.5f) break;
						float angle = (float)Math.Atan2(d.Y, d.X);
						_batch.Draw(_pixel, a, null, colour, angle, Vector2.Zero, new Vector2(length, Math.Max(1f, c.W * sy)), SpriteEffects.None, 0f);
						break;
					}
					case OpenFF.DrawKind.Sprite:
					{
						if (!(c.Texture is ModTexture texture) || texture.Texture2D == null || texture.Texture2D.IsDisposed) break;
						// The part of the picture (a texel at least: a gauge's fill cut to a sliver), stretched over the
						// rectangle and turned about its middle.
						Rectangle src = new Rectangle((int)c.SrcX, (int)c.SrcY, Math.Max(1, (int)c.SrcW), Math.Max(1, (int)c.SrcH));
						Vector2 middle = new Vector2((c.X + c.W / 2f) * sx, (c.Y + c.H / 2f) * sy);
						Vector2 stretch = new Vector2(c.W * sx / src.Width, c.H * sy / src.Height);
						_batch.Draw(texture.Texture2D, middle, src, colour, c.Rotation, new Vector2(src.Width / 2f, src.Height / 2f), stretch, SpriteEffects.None, 0f);
						break;
					}
					case OpenFF.DrawKind.Text:
						anyText = true;
						break;
				}
			}
			_batch.End();

			if (!anyText)
			{
				return;
			}
			GlobalScope.Graphics graphics = GlobalScope.m_Graphics;
			if (graphics == null)
			{
				return;
			}
			graphics.SetImageOrigin(0f, 0f);
			graphics.SetImageRotation(0f);
			graphics.SetImageScale(1f, 1f);
			graphics.DrawStringStart();
			for (int i = 0; i < commands.Count; i++)
			{
				OpenFF.DrawCommand c = commands[i];
				if (c.Kind != OpenFF.DrawKind.Text) continue;
				graphics.SetColor(c.Color.R, c.Color.G, c.Color.B, c.Color.A);
				graphics.DrawString(c.Text, c.X, c.Y, c.Size);
			}
			// The banner's words, centred in its window with the place name's shadow (the window itself is the game's 2D, drawn under).
			if (Banner.Shown is (float bx, float by, float bw, float bh, string text))
			{
				const int size = 13;
				float w = Ui.Width(graphics, text, size), h = Ui.LineHeight(size);
				float x = bx + (bw - w) / 2f, y = by + (bh - h) / 2f + size * 0.1f;
				graphics.SetColor(0, 0, 0, 200);
				graphics.DrawString(text, x + 1.5f, y + 1.5f, size);
				graphics.SetColor(255, 255, 255, 255);
				graphics.DrawString(text, x, y, size);
			}
			graphics.DrawStringEnd();
		}

		/// <summary>A rectangle of flat colour, in pixels, where it falls (not rounded to the pixel: a slide would shake).</summary>
		private void Fill(float x, float y, float w, float h, Microsoft.Xna.Framework.Color colour)
		{
			_batch.Draw(_pixel, new Vector2(x, y), null, colour, 0f, Vector2.Zero, new Vector2(w, h), SpriteEffects.None, 0f);
		}

		private void EnsureResources()
		{
			if (_batch == null)
			{
				_batch = new SpriteBatch(GraphicsDevice);
			}
			if (_pixel == null || _pixel.IsDisposed)
			{
				_pixel = new Texture2D(GraphicsDevice, 1, 1);
				_pixel.SetData(new[] { Microsoft.Xna.Framework.Color.White });
			}
		}
	}
}
