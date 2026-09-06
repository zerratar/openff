// In-game screenshot capture.
//
// Grabs the backbuffer and writes a PNG. Useful for checking rendering without
// depending on desktop screen capture, which fails whenever the session is locked
// or another window is in front.
//
//   F12                       capture one frame
//   FF3_SCREENSHOT_EVERY=<s>  capture automatically every <s> seconds
//   FF3_SCREENSHOT_DIR=<path> where to write (default <exe dir>\screenshots)
//
// Registered as a DrawableGameComponent so it runs from base.Draw, which Game1
// calls after the game has finished drawing the frame.

using System;
using System.Globalization;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FF3
{
	internal sealed class ScreenCapture : DrawableGameComponent
	{
		private readonly string _directory;
		private readonly double _intervalSeconds;

		private double _nextAutoCapture;
		private bool _keyWasDown;
		private bool _burstWasDown;
		private int _index;
		private Color[] _buffer;

		private ScreenCapture(Game game, string directory, double intervalSeconds)
			: base(game)
		{
			_directory = directory;
			_intervalSeconds = intervalSeconds;
			_nextAutoCapture = intervalSeconds;
			// After everything the game draws.
			DrawOrder = int.MaxValue;
		}

		/// <summary>Adds the component to the game. Always available via F12.</summary>
		public static void Attach(Game game)
		{
			string dir = Options.Get("screenshot-dir");
			if (string.IsNullOrEmpty(dir))
			{
				dir = Path.Combine(AppContext.BaseDirectory, "screenshots");
			}

			double interval = Options.GetDouble("screenshot-every", 0);
			if (interval < 0)
			{
				interval = 0;
			}

			game.Components.Add(new ScreenCapture(game, dir, interval));
			Log.Write(LogChannel.General,
				"screenshots: " + dir + (interval > 0 ? (" every " + interval + "s") : " (F12)"));
		}

		public override void Draw(GameTime gameTime)
		{
			bool keyDown = Game.IsActive && Keyboard.GetState().IsKeyDown(Keys.F12);
			bool pressed = keyDown && !_keyWasDown;
			_keyWasDown = keyDown;

			bool auto = _intervalSeconds > 0
				&& gameTime.TotalGameTime.TotalSeconds >= _nextAutoCapture;
			if (auto)
			{
				_nextAutoCapture = gameTime.TotalGameTime.TotalSeconds + _intervalSeconds;
			}

			if (pressed || auto)
			{
				Capture();
			}

			// F9 dumps the next few hundred draw calls in full, unsampled, so a whole
			// frame's draw order can be read back.
			bool burstKey = Game.IsActive && Keyboard.GetState().IsKeyDown(Keys.F9);
			if (burstKey && !_burstWasDown)
			{
				GlDiag.ArmBurst(400);
			}
			_burstWasDown = burstKey;
		}

		/// <summary>Writes the current backbuffer to a PNG. Returns the path, or null on failure.</summary>
		public string Capture()
		{
			try
			{
				GraphicsDevice device = GraphicsDevice;
				int width = device.PresentationParameters.BackBufferWidth;
				int height = device.PresentationParameters.BackBufferHeight;
				int count = width * height;
				if (_buffer == null || _buffer.Length != count)
				{
					_buffer = new Color[count];
				}
				device.GetBackBufferData(_buffer);

				Directory.CreateDirectory(_directory);
				string path = Path.Combine(_directory,
					string.Format(CultureInfo.InvariantCulture, "shot{0:D4}.png", ++_index));

				// Round-trip through a texture so MonoGame's PNG writer does the encoding.
				using (Texture2D texture = new Texture2D(device, width, height))
				{
					texture.SetData(_buffer);
					using FileStream file = File.Create(path);
					texture.SaveAsPng(file, width, height);
				}

				Log.Write(LogChannel.General, "screenshot: " + path);
				return path;
			}
			catch (Exception ex)
			{
				Log.Write(LogChannel.General, "screenshot failed: " + ex);
				return null;
			}
		}
	}
}
