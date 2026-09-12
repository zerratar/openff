// Short notices the client shows over the game, top right, one under another, each for a few
// seconds: what the progression layer has to say after a battle (ABP won, a job's level,
// an ability learned), and anything a mod posts through Game.Screen.Notice. Drawn the way
// the client's own menus are (Ui), so they read the same in any window size; nothing the
// game's own message windows are involved in, so they never block a scene.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OpenFF.Client
{
	using Game = Microsoft.Xna.Framework.Game;
	using GameTime = Microsoft.Xna.Framework.GameTime;
	using Color = Microsoft.Xna.Framework.Color;

	internal sealed class Notices : DrawableGameComponent
	{
		private const float Seconds = 4f, Fade = 0.5f;
		private const int Most = 6, Size = 9;

		private sealed class Notice { public string Text; public float Age; }

		public static Notices Instance { get; private set; }
		private readonly List<Notice> _shown = new List<Notice>();
		private readonly Queue<string> _waiting = new Queue<string>();
		private SpriteBatch _batch;

		private Notices(Game game) : base(game)
		{
			DrawOrder = int.MaxValue - 4;
			UpdateOrder = int.MaxValue - 4;
		}

		public static void Attach(Game game)
		{
			Instance = new Notices(game);
			game.Components.Add(Instance);
		}

		/// <summary>Shows a line; when six are up already it waits its turn.</summary>
		public static void Post(string text)
		{
			if (string.IsNullOrWhiteSpace(text)) return;
			Log.Write(LogChannel.File, "notice: " + text);
			if (Instance == null) return;
			lock (Instance._waiting) Instance._waiting.Enqueue(text);
		}

		public override void Update(GameTime gameTime)
		{
			float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
			lock (_waiting)
			{
				while (_waiting.Count > 0 && _shown.Count < Most) _shown.Add(new Notice { Text = _waiting.Dequeue() });
			}
			for (int i = _shown.Count - 1; i >= 0; i--)
			{
				_shown[i].Age += dt;
				if (_shown[i].Age > Seconds) _shown.RemoveAt(i);
			}
		}

		public override void Draw(GameTime gameTime)
		{
			if (_shown.Count == 0 || RenderTest.Active) return;
			GlobalScope.Graphics graphics = GlobalScope.m_Graphics;
			if (graphics == null) return;
			if (_batch == null) _batch = new SpriteBatch(GraphicsDevice);
			Ui.Ensure(GraphicsDevice);
			Viewport view = GraphicsDevice.Viewport;
			// Under the field's Map / Menu buttons, clear of the battle's windows at the top and bottom.
			float rowH = Ui.LineHeight(Size) + 8, top = 64, right = Ui.W - 12;

			_batch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
			for (int i = 0; i < _shown.Count; i++)
			{
				float w = Ui.Width(graphics, _shown[i].Text, Size) + 24;
				Rectangle r = new Rectangle((int)(right - w), (int)(top + i * (rowH + 4)), (int)w, (int)rowH);
				Ui.RoundPlate(_batch, Ui.Scale(r, view), Alpha(Ui.Panel, _shown[i].Age), view);
			}
			_batch.End();

			graphics.SetImageOrigin(0f, 0f);
			graphics.SetImageRotation(0f);
			graphics.SetImageScale(1f, 1f);
			graphics.DrawStringStart();
			for (int i = 0; i < _shown.Count; i++)
			{
				float w = Ui.Width(graphics, _shown[i].Text, Size) + 24;
				Rectangle r = new Rectangle((int)(right - w), (int)(top + i * (rowH + 4)), (int)w, (int)rowH);
				Ui.Left(graphics, _shown[i].Text, r.X + 12, r.Y, r.Height, Size, Alpha(Ui.Text, _shown[i].Age));
			}
			graphics.DrawStringEnd();
		}

		/// <summary>The colour faded out over the last half second.</summary>
		private static Color Alpha(Color c, float age)
		{
			float left = Seconds - age;
			float a = left < Fade ? Math.Max(0f, left / Fade) : 1f;
			return new Color(c.R, c.G, c.B, (byte)(c.A * a));
		}
	}
}
