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

namespace OpenFF.Client
{
	// MonoGame's types by name: the engine's OpenFF.Game, GameTime, Color and vectors sit a namespace up.
	using Game = Microsoft.Xna.Framework.Game;
	using GameTime = Microsoft.Xna.Framework.GameTime;
	using Color = Microsoft.Xna.Framework.Color;

	internal sealed class ScreenCapture : DrawableGameComponent
	{
		private readonly string _directory;
		private readonly double _intervalSeconds;

		private double _nextAutoCapture;
		private bool _keyWasDown;
		private bool _burstWasDown;
		private int _index;
		// --screenshot-steps=<n>[,<from>,<to>]: a shot at every n-th game step (LegacyStep.Count), named by it - to set
		// beside the Steam game's frames (Tools/ff4hook's 'every'), which count the same thirty a second.
		private readonly int _stepEvery, _stepFrom, _stepTo;
		private long _lastStep = -1;
		private string _nextName;
		private Color[] _buffer;

		private ScreenCapture(Game game, string directory, double intervalSeconds)
			: base(game)
		{
			_directory = directory;
			_intervalSeconds = intervalSeconds;
			_nextAutoCapture = intervalSeconds;
			string steps = Options.Get("screenshot-steps");
			if (!string.IsNullOrEmpty(steps))
			{
				string[] parts = steps.Split(',');
				int.TryParse(parts[0], out _stepEvery);
				_stepFrom = parts.Length > 1 && int.TryParse(parts[1], out int from) ? from : 0;
				_stepTo = parts.Length > 2 && int.TryParse(parts[2], out int to) ? to : int.MaxValue;
			}
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

			if (Burst > 0)
			{
				// A burst: each frame's back buffer kept as it is, written once it is over - a PNG takes longer than a frame.
				Burst--;
				try
				{
					int w = GraphicsDevice.PresentationParameters.BackBufferWidth, h = GraphicsDevice.PresentationParameters.BackBufferHeight;
					Color[] frame = new Color[w * h];
					GraphicsDevice.GetBackBufferData(frame);
					_burst.Add((frame, w, h));
				}
				catch (Exception ex) { Log.Write(LogChannel.General, "screenshot failed: " + ex.Message); }
				if (Burst == 0) WriteBurst();
			}
			else if (pressed || auto)
			{
				Capture();
			}
			else if (_stepEvery > 0)
			{
				long step = LegacyStep.Count - 1;   // the step shown: its scripts ran as LegacyStep.Count - 1
				if ((_lastStep < 0 || step / _stepEvery != _lastStep / _stepEvery) && step >= _stepFrom && step <= _stepTo)   // a catch-up can step past the multiple
				{
					_lastStep = step;
					_nextName = string.Format(CultureInfo.InvariantCulture, "step{0:D6}.png", step);
					Capture();
				}
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

		/// <summary>So many of the next displayed frames captured, one each (the drive's shots).</summary>
		public static int Burst;

		private readonly System.Collections.Generic.List<(Color[] Frame, int Width, int Height)> _burst = new System.Collections.Generic.List<(Color[], int, int)>();

		private void WriteBurst()
		{
			foreach ((Color[] frame, int w, int h) in _burst) Write(frame, w, h);
			_burst.Clear();
		}

		/// <summary>When the last screenshot was written (Environment.TickCount64), for the stall log: the PNG is encoded on the game's thread.</summary>
		public static long LastCaptureTick = long.MinValue / 2;

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
				return Write(_buffer, width, height);
			}
			catch (Exception ex)
			{
				Log.Write(LogChannel.General, "screenshot failed: " + ex);
				return null;
			}
		}

		private string Write(Color[] pixels, int width, int height)
		{
			try
			{
				Directory.CreateDirectory(_directory);
				string path = Path.Combine(_directory, _nextName ?? string.Format(CultureInfo.InvariantCulture, "shot{0:D4}.png", ++_index));
				_nextName = null;

				// Round-trip through a texture so MonoGame's PNG writer does the encoding.
				using (Texture2D texture = new Texture2D(GraphicsDevice, width, height))
				{
					texture.SetData(pixels);
					using FileStream file = File.Create(path);
					texture.SaveAsPng(file, width, height);
				}

				LastCaptureTick = Environment.TickCount64;
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
