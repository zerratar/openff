// Desktop input for the port.
//
// The game is driven entirely through the Android-style touch callbacks
// (Android.onTouchDown / onTouchMove / onTouchUp) and a Back key. This turns real
// mouse and keyboard state into those callbacks directly, without going through
// MonoGame's TouchPanel: TouchPanel.EnableMouseTouchPoint depends on SDL emitting
// synthetic touch events, which it does not do for a plain mouse on Windows.
//
// Coordinates: MainActivity.onTouchEvent divides by the view size that
// GLSurfaceView.setRenderer hard-codes as 800x480, so mouse positions are mapped
// from the window's client area into that same 800x480 space. This keeps input
// correct when the window is resized.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FF3
{
	internal static class DesktopInput
	{
		/// <summary>The coordinate space MainActivity normalises against.</summary>
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
			(Keys.X, PadB),            (Keys.Back, PadB),       (Keys.Escape, PadB),

			(Keys.C, PadX),
			(Keys.V, PadY),
			(Keys.Q, PadL),
			(Keys.E, PadR),
			(Keys.RightControl, PadStart),
			(Keys.RightShift, PadSelect)
		};

		/// <summary>
		/// Pad bits held this frame. MainActivity.getKeyEvent ORs this into the game's
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
				// Run. The game has no dedicated run button: isRun() tests the B bit, and
				// whether B means run or walk depends on Config > movement type. Shift is
				// what a PC player expects, so alias it onto B - but only while a direction
				// is held, because B is also cancel and menus read it as an edge.
				const int directions = PadUp | PadDown | PadLeft | PadRight;
				if ((bits & directions) != 0 && keys.IsKeyDown(Keys.LeftShift))
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
			bool fast = _game != null && _game.IsActive && Keyboard.GetState().IsKeyDown(Keys.Tab);
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
			(TextEntry.Instance != null && TextEntry.Instance.IsActive) || ModListScreen.IsOpen || EngineInput.Captured;

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

		// The game reads touch through MainActivity.onTouchEvent. These build the
		// MotionEvent it expects - action 0 down, 1 up, 2 move - which used to go
		// through the Android activity broadcast.
		private static void TouchDown(int x, int y) => Send(0, x, y);

		private static void TouchUp(int x, int y) => Send(1, x, y);

		private static void TouchMove(int x, int y) => Send(2, x, y);

		private static void Send(int action, int x, int y)
		{
			MainActivity game = GameHost.Game;
			if (game != null)
			{
				game.onTouchEvent(new android.view.MotionEvent(
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
