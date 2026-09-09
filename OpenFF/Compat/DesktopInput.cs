// Desktop input for the port.
//
// The game is driven entirely through the Android-style touch callbacks
// (Android.onTouchDown / onTouchMove / onTouchUp) and a Back key. This turns real
// mouse and keyboard state into those callbacks directly, without going through
// MonoGame's TouchPanel: TouchPanel.EnableMouseTouchPoint depends on SDL emitting
// synthetic touch events, which it does not do for a plain mouse on Windows.
//
// Coordinates: AppShell.onTouchEvent divides by the view size that
// GLSurfaceView.setRenderer hard-codes as 800x480, so mouse positions are mapped
// from the window's client area into that same 800x480 space. This keeps input
// correct when the window is resized.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace OpenFF.Client
{
	// MonoGame's types by name: the engine's OpenFF.Game, GameTime, Color and vectors sit a namespace up.
	using Game = Microsoft.Xna.Framework.Game;

	internal static class DesktopInput
	{
		/// <summary>The coordinate space AppShell normalises against.</summary>
		private const int ViewWidth = 800;
		private const int ViewHeight = 480;

		// The game keeps an NDS-style pad register (GlobalScope.cont), read through
		// PAD_Read into ds.CPad, which is what every menu and field control actually
		// polls. The phone build only ever set the B bit, from the hardware Back button.
		// Feeding this register is what gives the Windows build keyboard control.
		private const int PadA = 1;
		private const int PadB = 2;
		private const int PadSelect = 4;
		private const int PadStart = 8;
		private const int PadRight = 16;
		private const int PadLeft = 32;
		private const int PadUp = 64;
		private const int PadDown = 128;
		private const int PadR = 256;
		private const int PadL = 512;
		private const int PadX = 1024;
		private const int PadY = 2048;

		/// <summary>Keyboard bindings, provisional - worth making configurable later.</summary>
		private static readonly (Keys Key, int Bit)[] PadBindings =
		{
			(Keys.Up, PadUp),          (Keys.W, PadUp),
			(Keys.Down, PadDown),      (Keys.S, PadDown),
			(Keys.Left, PadLeft),      (Keys.A, PadLeft),
			(Keys.Right, PadRight),    (Keys.D, PadRight),

			(Keys.Z, PadA),            (Keys.Space, PadA),      (Keys.Enter, PadA),
			(Keys.X, PadB),            (Keys.Back, PadB),       // Esc is the client's own menu (PauseMenu)

			(Keys.C, PadX),
			(Keys.V, PadY),
			(Keys.Q, PadL),
			(Keys.E, PadR),
			(Keys.RightControl, PadStart),
			(Keys.RightShift, PadSelect)
		};

		/// <summary>
		/// Pad bits held this frame. AppShell.getKeyEvent ORs this into the game's
		/// pad register once per rendered frame; edge and repeat detection then happen
		/// in ds.CPad exactly as they did on the DS.
		/// </summary>
		public static int PadBits
		{
			get
			{
				if (_game == null || (!_game.IsActive && Injected.Count == 0) || IsTyping)
				{
					return 0;
				}
				return RawPadBits();
			}
		}

		/// <summary>Keys a scripted drive (--drive, Compat/Drive.cs) holds this frame; read beside the keyboard, with or without focus.</summary>
		public static readonly HashSet<Keys> Injected = new HashSet<Keys>();

		/// <summary>The pad bits from the keyboard alone, ungated: what the engine's Game.Input gets even while a mod has captured input.</summary>
		public static int RawPadBits()
		{
			{
				if (_game == null || (!_game.IsActive && Injected.Count == 0))
				{
					return 0;
				}
				KeyboardState keys = Keyboard.GetState();
				bool real = _game.IsActive;
				int bits = 0;
				foreach ((Keys key, int bit) in PadBindings)
				{
					if ((real && keys.IsKeyDown(key)) || Injected.Contains(key))
					{
						bits |= bit;
					}
				}
				if (real) bits |= GamePadBits();
				// Run. The game has no dedicated run button: isRun() tests the B bit, and
				// whether B means run or walk depends on Config > movement type. Shift is
				// what a PC player expects, so alias it onto B - but only while a direction
				// is held, because B is also cancel and menus read it as an edge. A pad's
				// right trigger does the same.
				const int directions = PadUp | PadDown | PadLeft | PadRight;
				if ((bits & directions) != 0 && (keys.IsKeyDown(Keys.LeftShift) || _padRun))
				{
					bits |= PadB;
				}

				if (bits != 0)
				{
					Log.Sample(LogChannel.Input, "pad", 30, () => "bits=0x" + bits.ToString("x"));
				}
				return bits;
			}
		}

		/// <summary>Whether the pad's right trigger is held (run, like Shift), read with the pad bits.</summary>
		private static bool _padRun;
		private static bool _padSeen;

		/// <summary>
		/// The first connected game pad as DS pad bits: the d-pad and the left stick are the
		/// directions; the buttons are whatever settings.json's "pad" map says (by default
		/// Cross/Circle/Square/Triangle as the DS's A/B/X/Y, L1/R1, Options/Share, R2 to run, L2
		/// to fast-forward). With "run": "stick" the stick pushed all the way runs by itself.
		/// </summary>
		private static int GamePadBits()
		{
			_padRun = false;
			for (int i = 0; i < 4; i++)
			{
				GamePadState pad;
				try { pad = GamePad.GetState((PlayerIndex)i, GamePadDeadZone.Circular); }
				catch (Exception) { continue; }
				if (!pad.IsConnected) continue;
				if (!_padSeen)
				{
					_padSeen = true;
					Log.Write(LogChannel.General, "input: game pad " + (i + 1) + " connected (" + (GamePad.GetCapabilities((PlayerIndex)i).DisplayName ?? "unnamed") + ")");
				}
				int bits = 0;
				DisplaySettings settings = DisplaySettings.Current;
				DisplaySettings.PadMap map = settings.Pad ?? new DisplaySettings.PadMap();
				// The left stick is eight directions to the DS; how far it is pushed is the pace
				// when "run" is "stick" (the touch stick's way): part way walks, all the way runs.
				GamePadDPad d = pad.DPad;
				Microsoft.Xna.Framework.Vector2 stick = pad.ThumbSticks.Left;
				const float deadZone = 0.3f, runZone = 0.8f;
				float push = stick.Length();
				if (push < deadZone) stick = Microsoft.Xna.Framework.Vector2.Zero;
				// A direction from the stick's angle: within 22.5 degrees of an axis is that axis alone, else a diagonal.
				if (stick != Microsoft.Xna.Framework.Vector2.Zero)
				{
					double angle = Math.Atan2(stick.Y, stick.X) * 180.0 / Math.PI;   // 0 = right, 90 = up
					bool right = angle > -67.5 && angle < 67.5, left = angle > 112.5 || angle < -112.5;
					bool up = angle > 22.5 && angle < 157.5, down = angle < -22.5 && angle > -157.5;
					if (up) bits |= PadUp;
					if (down) bits |= PadDown;
					if (left) bits |= PadLeft;
					if (right) bits |= PadRight;
				}
				if (d.Up == ButtonState.Pressed) bits |= PadUp;
				if (d.Down == ButtonState.Pressed) bits |= PadDown;
				if (d.Left == ButtonState.Pressed) bits |= PadLeft;
				if (d.Right == ButtonState.Pressed) bits |= PadRight;
				if (DisplaySettings.Held(pad, map.A)) bits |= PadA;
				if (DisplaySettings.Held(pad, map.B)) bits |= PadB;
				if (DisplaySettings.Held(pad, map.X)) bits |= PadX;
				if (DisplaySettings.Held(pad, map.Y)) bits |= PadY;
				if (DisplaySettings.Held(pad, map.L)) bits |= PadL;
				if (DisplaySettings.Held(pad, map.R)) bits |= PadR;
				if (DisplaySettings.Held(pad, map.Start)) bits |= PadStart;
				if (DisplaySettings.Held(pad, map.Select)) bits |= PadSelect;
				_padRun = DisplaySettings.Held(pad, map.RunButton) || (settings.Run == "stick" && push >= runZone);
				return bits;
			}
			return 0;
		}

		/// <summary>The pad's bits alone, no keyboard: for a screen that types with the keyboard and steers with the pad (the name entry's on-screen keys).</summary>
		internal static int PadOnlyBits() => _game != null && _game.IsActive ? GamePadBits() : 0;

		/// <summary>Whether any pad is connected: the on-screen keys show for one.</summary>
		internal static bool PadConnected
		{
			get
			{
				for (int i = 0; i < 4; i++)
				{
					try { if (GamePad.GetState((PlayerIndex)i).IsConnected) return true; } catch (Exception) { }
				}
				return false;
			}
		}

		/// <summary>The mouse in the 800x480 view space, and whether its left button is down.</summary>
		internal static bool MouseInView(out int x, out int y)
		{
			MouseState mouse = Mouse.GetState();
			if (_game == null) { x = mouse.X; y = mouse.Y; return mouse.LeftButton == ButtonState.Pressed; }
			MapToViewSpace(mouse.X, mouse.Y, out x, out y);
			return mouse.LeftButton == ButtonState.Pressed;
		}

		/// <summary>Whether a connected pad holds the left trigger: fast-forward, like Tab.</summary>
		private static bool GamePadFast()
		{
			for (int i = 0; i < 4; i++)
			{
				try
				{
					GamePadState pad = GamePad.GetState((PlayerIndex)i);
					if (pad.IsConnected) return DisplaySettings.Held(pad, (DisplaySettings.Current.Pad ?? new DisplaySettings.PadMap()).Fast);
				}
				catch (Exception) { }
			}
			return false;
		}

		private static Game _game;
		private static bool _wasDown;
		private static int _lastX;
		private static int _lastY;

		/// <summary>Extra update passes per frame, on top of boost. Opt in with FF3_SPEED.</summary>
		private static int _fastForwardFactor = 1;

		public static void Attach(Game game)
		{
			_game = game;
			int speed = Options.GetInt("speed", 1);
			if (speed >= 1)
			{
				_fastForwardFactor = Math.Min(speed, 64);
			}
		}

		/// <summary>
		/// Fast-forward, called once per frame before the game updates.
		///
		/// GlobalScope.boost is the game's own speed switch: the frame-catch-up loop in
		/// render() multiplies its iteration count by 3 when it is set. It is declared
		/// and read but never assigned anywhere in the decompiled code - the Android
		/// build drove it over JNI - so setting it here is free and safe.
		///
		/// FF3_SPEED asks for more than boost gives by running extra update passes. That
		/// is cruder (it re-runs the whole render path), so it stays opt-in.
		/// </summary>
		public static int BeginFrame()
		{
			DisplaySettings.Poll(_game, GlobalScope.m_Graphics?.GetGraphicsDeviceManager());
			bool fast = _game != null && _game.IsActive && (Keyboard.GetState().IsKeyDown(Keys.Tab) || GamePadFast());
			GlobalScope.boost = fast ? 1 : 0;
			return (fast && _fastForwardFactor > 1) ? _fastForwardFactor : 1;
		}

		/// <summary>Translates this frame's mouse and keyboard state into game callbacks.</summary>
		public static void Update()
		{
			Drive.Update();
			if (_game == null || !_game.IsActive)
			{
				// Release a held button rather than stranding the game mid-drag.
				if (_wasDown)
				{
					_wasDown = false;
					TouchUp(_lastX, _lastY);
				}
				return;
			}

			// A text field owns the keyboard and mouse while it is up.
			if (IsTyping)
			{
				return;
			}

			UpdateMouse();
		}

		/// <summary>True while something other than the game owns input: a text field, the mod list, or a mod that captured it (Game.Input.Capture).</summary>
		private static bool IsTyping =>
			(TextEntry.Instance != null && TextEntry.Instance.IsActive) || ModListScreen.IsOpen || PauseMenu.IsOpen || EngineInput.Captured;

		private static void UpdateMouse()
		{
			MouseState mouse = Mouse.GetState();
			MapToViewSpace(mouse.X, mouse.Y, out int x, out int y);
			bool down = mouse.LeftButton == ButtonState.Pressed;

			if (down && !_wasDown)
			{
				Log.Write(LogChannel.Input, $"touch down {x},{y}");
				TouchDown(x, y);
			}
			else if (down && (x != _lastX || y != _lastY))
			{
				Log.Sample(LogChannel.Input, "touch move", 30, () => $"{x},{y}");
				TouchMove(x, y);
			}
			else if (!down && _wasDown)
			{
				Log.Write(LogChannel.Input, $"touch up {x},{y}");
				TouchUp(x, y);
			}

			_wasDown = down;
			_lastX = x;
			_lastY = y;
		}

		// The game reads touch through AppShell.onTouchEvent. These build the
		// MotionEvent it expects - action 0 down, 1 up, 2 move - which used to go
		// through the Android activity broadcast.
		private static void TouchDown(int x, int y) => Send(0, x, y);

		/// <summary>A drive's touch (action 0 down, 1 up, 2 move) at a point of the 800x480 view, the way a click arrives.</summary>
		internal static void InjectTouch(int action, int x, int y)
		{
			Log.Write(LogChannel.Input, $"touch {(action == 0 ? "down" : action == 1 ? "up" : "move")} {x},{y} (drive)");
			Send(action, x, y);
			if (action == 0) { _wasDown = false; }
		}

		private static void TouchUp(int x, int y) => Send(1, x, y);

		private static void TouchMove(int x, int y) => Send(2, x, y);

		private static void Send(int action, int x, int y)
		{
			AppShell game = GameHost.Game;
			if (game != null)
			{
				game.onTouchEvent(new OpenFF.Platform.MotionEvent(
					action, 1, new float[1] { x }, new float[1] { y }));
			}
		}

		/// <summary>Maps a window client-area point into the game's fixed 800x480 view.</summary>
		private static void MapToViewSpace(int windowX, int windowY, out int x, out int y)
		{
			Rectangle client = _game.Window.ClientBounds;
			if (client.Width <= 0 || client.Height <= 0)
			{
				x = windowX;
				y = windowY;
				return;
			}
			x = (int)((long)windowX * ViewWidth / client.Width);
			y = (int)((long)windowY * ViewHeight / client.Height);
		}
	}
}
