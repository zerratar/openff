// Draws what mods asked Game.Draw to draw, over the frame.
//
// A DrawableGameComponent after the game and the mod list, before the debug overlay and
// the screenshot capture. Text goes through the game's own font (TrueType, the 800x480
// text space); rectangles, lines and sprites through a SpriteBatch scaled to the same
// space. Textures a mod loads are Texture2Ds wrapped for the engine; they live until the
// device goes.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FF3
{
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

		public override void Draw(GameTime gameTime)
		{
			OpenFF.DrawList list = OpenFF.Game.Draw;
			try
			{
				if (list.Commands.Count > 0 && !RenderTest.Active)
				{
					DrawAll(list);
				}
			}
			catch (Exception ex)
			{
				Log.First(LogChannel.General, "mod-draw", 3, () => "engine: draw failed: " + ex.GetType().Name + ": " + ex.Message);
			}
			finally
			{
				list.Clear();
			}
		}

		private void DrawAll(OpenFF.DrawList list)
		{
			EnsureResources();
			Viewport view = GraphicsDevice.Viewport;
			float sx = view.Width / TextSpaceWidth;
			float sy = view.Height / TextSpaceHeight;
			bool anyText = false;

			_batch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
			foreach (OpenFF.DrawCommand c in list.Commands)
			{
				Microsoft.Xna.Framework.Color colour = new Microsoft.Xna.Framework.Color(c.Color.R, c.Color.G, c.Color.B, c.Color.A);
				switch (c.Kind)
				{
					case OpenFF.DrawKind.Rect:
					{
						Rectangle r = new Rectangle((int)Math.Round(c.X * sx), (int)Math.Round(c.Y * sy), (int)Math.Round(c.W * sx), (int)Math.Round(c.H * sy));
						if (c.Filled)
						{
							_batch.Draw(_pixel, r, colour);
						}
						else
						{
							_batch.Draw(_pixel, new Rectangle(r.X, r.Y, r.Width, 1), colour);
							_batch.Draw(_pixel, new Rectangle(r.X, r.Bottom - 1, r.Width, 1), colour);
							_batch.Draw(_pixel, new Rectangle(r.X, r.Y, 1, r.Height), colour);
							_batch.Draw(_pixel, new Rectangle(r.Right - 1, r.Y, 1, r.Height), colour);
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
						Rectangle src = new Rectangle((int)c.SrcX, (int)c.SrcY, (int)c.SrcW, (int)c.SrcH);
						Rectangle dst = new Rectangle((int)Math.Round((c.X + c.W / 2f) * sx), (int)Math.Round((c.Y + c.H / 2f) * sy), (int)Math.Round(c.W * sx), (int)Math.Round(c.H * sy));
						_batch.Draw(texture.Texture2D, dst, src, colour, c.Rotation, new Vector2(src.Width / 2f, src.Height / 2f), SpriteEffects.None, 0f);
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
			foreach (OpenFF.DrawCommand c in list.Commands)
			{
				if (c.Kind != OpenFF.DrawKind.Text) continue;
				graphics.SetColor(c.Color.R, c.Color.G, c.Color.B, c.Color.A);
				graphics.DrawString(c.Text, c.X, c.Y, c.Size);
			}
			graphics.DrawStringEnd();
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
