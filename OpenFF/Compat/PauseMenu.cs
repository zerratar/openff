// The client's own menu over the game: Esc on the keyboard or Start (Options) on a pad opens
// it anywhere - the title, the field, a battle - with Resume, Settings and Exit game. It is
// not one of the game's menus (those are the phone's layouts and pictures); it is drawn the
// way the mod list and the text entry are, with the game's font over a panel, and it edits
// what %LocalAppData%\OpenFF\settings.json holds: the window and its size, anti-aliasing,
// vsync, how the stick runs, and which pad button is which DS button (press one to bind).
// Display changes apply as they are made; the file is written on the way out.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace OpenFF.Client
{
	// MonoGame's types by name: the engine's OpenFF.Game, GameTime, Color and vectors sit a namespace up.
	using Game = Microsoft.Xna.Framework.Game;
	using GameTime = Microsoft.Xna.Framework.GameTime;
	using Color = Microsoft.Xna.Framework.Color;

	internal sealed class PauseMenu : DrawableGameComponent
	{
		private const float W = 800f, H = 480f;
		private const int TitleSize = 14, RowSize = 10;
		private const float ListTop = 110f, ListLeft = 120f, ListWidth = 560f;
		private float RowHeight => Rows() > 10 ? 21f : 24f;   // the buttons page has twelve rows to fit

		public static PauseMenu Instance { get; private set; }
		public static bool IsOpen => Instance != null && Instance._page != Page.Closed;

		private enum Page { Closed, Main, Settings, Buttons, Quit }
		private Page _page = Page.Closed;
		private int _selected;
		private string _binding;            // the DS button being rebound, while a press is awaited
		private int _previousPad, _previousPadRaw;
		private KeyboardState _previousKeys;
		private bool _mouseWasDown;
		private bool _openEdge;             // Esc/Start held since the open, so the press that opened does not also close
		private SpriteBatch _batch;
		private Texture2D _pixel;
		private string _note = "";

		private static readonly (int W, int H)[] Sizes = { (800, 480), (1200, 720), (1600, 960), (2000, 1200), (2400, 1440), (3200, 1920) };
		private static readonly string[] Modes = { "windowed", "borderless", "fullscreen" };
		private static readonly int[] Msaas = { 0, 2, 4, 8 };
		private static readonly (string Key, string Label)[] DsButtons =
		{
			("a", "A  (confirm)"), ("b", "B  (cancel, run)"), ("x", "X  (menu)"), ("y", "Y"), ("l", "L"), ("r", "R"),
			("start", "Start"), ("select", "Select"), ("run", "Run (held)"), ("fast", "Fast-forward (held)")
		};

		private PauseMenu(Game game) : base(game)
		{
			DrawOrder = int.MaxValue - 3;
			UpdateOrder = int.MaxValue - 3;
		}

		public static void Attach(Game game)
		{
			Instance = new PauseMenu(game);
			game.Components.Add(Instance);
		}

		/// <summary>Whether Esc or a pad's Start is down: what opens the menu, read each frame while it is closed.</summary>
		private static bool OpenKeyDown()
		{
			KeyboardState keys = Keyboard.GetState();
			if (Down(keys, Keys.Escape)) return true;
			return (DesktopInput.PadOnlyBits() & 8) != 0;
		}

		public static void Open()
		{
			if (Instance == null || IsOpen) return;
			Instance._page = Page.Main;
			Instance._selected = 0;
			Instance._note = "";
			Instance._openEdge = true;
			Instance._previousKeys = Keyboard.GetState();
			Instance._previousPad = DesktopInput.RawPadBits();
			Instance._wasDown.Clear();
			foreach (Keys k in new[] { Keys.Up, Keys.Down, Keys.Left, Keys.Right, Keys.W, Keys.A, Keys.S, Keys.D, Keys.Enter, Keys.Space, Keys.Z, Keys.X, Keys.Back, Keys.Escape }) if (Down(Instance._previousKeys, k)) Instance._wasDown.Add(k);
			Log.Write(LogChannel.General, "menu: opened (Esc / Start)");
		}

		private void Close()
		{
			if (_page == Page.Closed) return;
			_page = Page.Closed;
			_binding = null;
			DisplaySettings.Current.Save();
			Log.Write(LogChannel.General, "menu: closed; settings.json written");
		}

		// A drive's injected keys count as held, so the menu can be tried unattended.
		private static bool Down(KeyboardState keys, Keys key) => keys.IsKeyDown(key) || DesktopInput.Injected.Contains(key);
		private bool Pressed(KeyboardState keys, Keys key) => Down(keys, key) && !_wasDown.Contains(key);
		private readonly HashSet<Keys> _wasDown = new HashSet<Keys>();

		public override void Update(GameTime gameTime)
		{
			if (!Game.IsActive) return;
			// Nothing else may own the keyboard: the text entry, the mod list, a mod's capture.
			bool othersOwn = (TextEntry.Instance != null && TextEntry.Instance.IsActive) || ModListScreen.IsOpen || EngineInput.Captured;
			if (_page == Page.Closed)
			{
				if (othersOwn || RenderTest.Active) { _openEdge = OpenKeyDown(); return; }
				bool down = OpenKeyDown();
				if (down && !_openEdge) Open();
				_openEdge = down;
				return;
			}

			KeyboardState keys = Keyboard.GetState();
			int pad = DesktopInput.RawPadBits(), edge = pad & ~_previousPad;
			_previousPad = pad;
			bool openDown = OpenKeyDown();
			bool up = Pressed(keys, Keys.Up) || Pressed(keys, Keys.W) || (edge & 64) != 0;
			bool downKey = Pressed(keys, Keys.Down) || Pressed(keys, Keys.S) || (edge & 128) != 0;
			bool left = Pressed(keys, Keys.Left) || Pressed(keys, Keys.A) || (edge & 32) != 0;
			bool right = Pressed(keys, Keys.Right) || Pressed(keys, Keys.D) || (edge & 16) != 0;
			bool confirm = Pressed(keys, Keys.Enter) || Pressed(keys, Keys.Space) || Pressed(keys, Keys.Z) || (edge & 1) != 0;
			bool cancel = Pressed(keys, Keys.X) || Pressed(keys, Keys.Back) || (edge & 2) != 0 || (openDown && !_openEdge);
			_openEdge = openDown;
			_previousKeys = keys;
			_wasDown.Clear();
			foreach (Keys k in new[] { Keys.Up, Keys.Down, Keys.Left, Keys.Right, Keys.W, Keys.A, Keys.S, Keys.D, Keys.Enter, Keys.Space, Keys.Z, Keys.X, Keys.Back, Keys.Escape }) if (Down(keys, k)) _wasDown.Add(k);

			if (_binding != null)
			{
				// Awaiting a pad button for a DS button: the first one pressed is it; B on the keyboard or Esc gives up.
				string pressed = NewlyPressedPadButton();
				if (pressed != null) { Bind(_binding, pressed); _binding = null; _note = "bound"; }
				else if (Pressed(keys, Keys.Escape) || Pressed(keys, Keys.X)) { _binding = null; _note = ""; }
				return;
			}

			int count = Rows();
			if (up) _selected = (_selected + count - 1) % count;
			if (downKey) _selected = (_selected + 1) % count;

			// The mouse: a row under the pointer is selected; a click on it confirms.
			bool mouseDown = DesktopInput.MouseInView(out int mx, out int my);
			if (mouseDown && !_mouseWasDown)
			{
				int row = (int)((my - ListTop) / RowHeight);
				if (mx >= ListLeft && mx <= ListLeft + ListWidth && row >= 0 && row < count) { _selected = row; confirm = true; }
			}
			_mouseWasDown = mouseDown;

			switch (_page)
			{
				case Page.Main:
					if (cancel) { Close(); return; }
					if (confirm)
					{
						if (_selected == 0) Close();
						else if (_selected == 1) { _page = Page.Settings; _selected = 0; _note = ""; }
						else { _page = Page.Quit; _selected = 1; }
					}
					break;
				case Page.Settings:
					if (cancel) { _page = Page.Main; _selected = 1; DisplaySettings.Current.Save(); return; }
					if (left || right || confirm) ChangeSetting(_selected, left ? -1 : 1, confirm);
					break;
				case Page.Buttons:
					if (cancel) { _page = Page.Settings; _selected = 5; return; }
					if (confirm)
					{
						if (_selected < DsButtons.Length) { _binding = DsButtons[_selected].Key; _note = "press the pad button for " + DsButtons[_selected].Label + "  (Esc gives up)"; }
						else if (_selected == DsButtons.Length) { DisplaySettings.Current.Pad = new DisplaySettings.PadMap(); _note = "the defaults are back"; }
						else { _page = Page.Settings; _selected = 5; }
					}
					break;
				case Page.Quit:
					if (cancel) { _page = Page.Main; _selected = 2; return; }
					if (confirm)
					{
						if (_selected == 0) { DisplaySettings.Current.Save(); Log.Write(LogChannel.General, "menu: exit game"); Game.Exit(); }
						else { _page = Page.Main; _selected = 2; }
					}
					break;
			}
		}

		private int Rows()
		{
			switch (_page)
			{
				case Page.Main: return 3;
				case Page.Settings: return 7;
				case Page.Buttons: return DsButtons.Length + 2;
				case Page.Quit: return 2;
				default: return 1;
			}
		}

		/// <summary>One settings row turned: left/right cycle, confirm goes forward (or opens the buttons page / goes back).</summary>
		private void ChangeSetting(int row, int by, bool confirm)
		{
			DisplaySettings s = DisplaySettings.Current;
			GraphicsDeviceManager gdm = GlobalScope.m_Graphics?.GetGraphicsDeviceManager();
			switch (row)
			{
				case 0:
				{
					int i = Array.IndexOf(Modes, s.Mode); if (i < 0) i = 0;
					s.Mode = Modes[(i + by + Modes.Length) % Modes.Length];
					ApplyDisplay(gdm);
					break;
				}
				case 1:
				{
					int i = -1;
					for (int k = 0; k < Sizes.Length; k++) if (Sizes[k].W == s.Width && Sizes[k].H == s.Height) i = k;
					i = i < 0 ? (by > 0 ? 0 : Sizes.Length - 1) : (i + by + Sizes.Length) % Sizes.Length;
					s.Width = Sizes[i].W; s.Height = Sizes[i].H;
					if (s.Mode == "windowed") ApplyDisplay(gdm);
					else _note = "the size is the window's; borderless and fullscreen use the desktop's";
					break;
				}
				case 2:
				{
					int i = Array.IndexOf(Msaas, s.Msaa); if (i < 0) i = 0;
					s.Msaa = Msaas[(i + by + Msaas.Length) % Msaas.Length];
					_note = "anti-aliasing applies at the next start";
					break;
				}
				case 3:
					s.VSync = !s.VSync;
					if (gdm != null) { gdm.SynchronizeWithVerticalRetrace = s.VSync; try { gdm.ApplyChanges(); } catch (Exception) { } }
					break;
				case 4:
					s.Run = s.Run == "stick" ? "hold" : "stick";
					break;
				case 5:
					if (confirm) { _page = Page.Buttons; _selected = 0; _note = ""; }
					break;
				case 6:
					if (confirm) { _page = Page.Main; _selected = 1; s.Save(); }
					break;
			}
		}

		private void ApplyDisplay(GraphicsDeviceManager gdm)
		{
			if (gdm == null) return;
			DisplaySettings s = DisplaySettings.Current;
			DisplayMode desktop = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
			try
			{
				gdm.IsFullScreen = s.Mode != "windowed";
				gdm.HardwareModeSwitch = s.Mode == "fullscreen";
				gdm.PreferredBackBufferWidth = s.Mode == "borderless" ? desktop.Width : s.Width;
				gdm.PreferredBackBufferHeight = s.Mode == "borderless" ? desktop.Height : s.Height;
				gdm.ApplyChanges();
				_note = "";
			}
			catch (Exception ex) { _note = "the display did not switch: " + ex.Message; }
		}

		private static void Bind(string dsButton, string padButton)
		{
			DisplaySettings.PadMap m = DisplaySettings.Current.Pad ??= new DisplaySettings.PadMap();
			switch (dsButton)
			{
				case "a": m.A = padButton; break;
				case "b": m.B = padButton; break;
				case "x": m.X = padButton; break;
				case "y": m.Y = padButton; break;
				case "l": m.L = padButton; break;
				case "r": m.R = padButton; break;
				case "start": m.Start = padButton; break;
				case "select": m.Select = padButton; break;
				case "run": m.RunButton = padButton; break;
				case "fast": m.Fast = padButton; break;
			}
		}

		private static string Bound(string dsButton)
		{
			DisplaySettings.PadMap m = DisplaySettings.Current.Pad ?? new DisplaySettings.PadMap();
			switch (dsButton)
			{
				case "a": return m.A; case "b": return m.B; case "x": return m.X; case "y": return m.Y;
				case "l": return m.L; case "r": return m.R; case "start": return m.Start; case "select": return m.Select;
				case "run": return m.RunButton; case "fast": return m.Fast;
				default: return "";
			}
		}

		private static readonly string[] PadButtonNames = { "cross", "circle", "square", "triangle", "l1", "r1", "l2", "r2", "l3", "r3", "options", "share", "touchpad" };

		/// <summary>The pad button that went down since the last look, by its name; null for none.</summary>
		private string NewlyPressedPadButton()
		{
			for (int i = 0; i < 4; i++)
			{
				GamePadState pad;
				try { pad = GamePad.GetState((PlayerIndex)i); } catch (Exception) { continue; }
				if (!pad.IsConnected) continue;
				int now = 0;
				for (int b = 0; b < PadButtonNames.Length; b++) if (DisplaySettings.Held(pad, PadButtonNames[b])) now |= 1 << b;
				int fresh = now & ~_previousPadRaw;
				_previousPadRaw = now;
				for (int b = 0; b < PadButtonNames.Length; b++) if ((fresh & (1 << b)) != 0) return PadButtonNames[b];
				return null;
			}
			return null;
		}

		public override void Draw(GameTime gameTime)
		{
			if (_page == Page.Closed || RenderTest.Active) return;
			GlobalScope.Graphics graphics = GlobalScope.m_Graphics;
			if (graphics == null) return;
			EnsureResources();
			Ui.Ensure(GraphicsDevice);
			Viewport view = GraphicsDevice.Viewport;
			int count = Rows();
			float rowH = RowHeight;
			Rectangle panel = new Rectangle(150, 48, 500, 384);
			int left = panel.X + 24, right = panel.Right - 24;
			const int titleSize = 14, rowSize = 11, hintSize = 9;
			float hintY = panel.Bottom - 22;

			_batch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
			Ui.PanelPlate(_batch, panel, view);
			for (int row = 0; row < count; row++)
			{
				Rectangle r = new Rectangle(left, (int)(ListTop + row * rowH), right - left, (int)rowH - 3);
				if (row == _selected) Ui.Key(_batch, r, true, view);
			}
			// A rule under the title.
			Rectangle rule = Ui.Scale(new Rectangle(left, panel.Y + 44, right - left, 1), view);
			Ui.Fill(_batch, rule, new Color(70, 96, 170, 255));
			float hx = left;
			foreach ((Ui.PadButton button, string word) in Hints())
			{
				Ui.HintShape(_batch, button, hx, hintY, 18, view);
				hx += Ui.HintWidth(graphics, word, hintSize, 18) + 22;
			}
			_batch.End();

			graphics.SetImageOrigin(0f, 0f);
			graphics.SetImageRotation(0f);
			graphics.SetImageScale(1f, 1f);
			graphics.DrawStringStart();
			string title = _page == Page.Main ? "OpenFF" : _page == Page.Settings ? "Settings" : _page == Page.Buttons ? "Pad buttons" : "Exit game?";
			Ui.Left(graphics, title, left, panel.Y + 10, Ui.LineHeight(titleSize), titleSize, Ui.Text);
			string sub = _page == Page.Main ? "The game goes on behind this."
				: _page == Page.Settings ? "Kept in settings.json under %LocalAppData%\\OpenFF"
				: _page == Page.Buttons ? "Pick a DS button, then press the pad button for it."
				: "Anything not saved is lost.";
			Ui.Left(graphics, sub, left + Ui.Width(graphics, title, titleSize) + 14, panel.Y + 10, Ui.LineHeight(titleSize), 9, Ui.Muted);
			for (int row = 0; row < count; row++)
			{
				Rectangle r = new Rectangle(left, (int)(ListTop + row * rowH), right - left, (int)rowH - 3);
				bool on = row == _selected;
				RowText(row, out string label, out string value);
				Ui.Left(graphics, label, r.X + 12, r.Y, r.Height, rowSize, on ? Ui.TextOnLit : Ui.Text);
				if (value != null)
				{
					bool arrows = on && _page == Page.Settings && row < 5;
					string shown = arrows ? "<  " + value + "  >" : value;
					float w = Ui.Width(graphics, shown, rowSize);
					Ui.Left(graphics, shown, r.Right - 12 - w, r.Y, r.Height, rowSize, on ? Ui.TextOnLit : Ui.Muted);
				}
			}
			if (!string.IsNullOrEmpty(_note)) Ui.Left(graphics, _note, left, panel.Bottom - 52, Ui.LineHeight(9), 9, Ui.Accent);
			hx = left;
			foreach ((Ui.PadButton button, string word) in Hints()) hx = Ui.HintText(graphics, button, word, hx, hintY, 18, hintSize) - 4;
			graphics.DrawStringEnd();
		}

		private (Ui.PadButton, string)[] Hints()
		{
			if (_page == Page.Settings) return new[] { (Ui.PadButton.A, "Next value"), (Ui.PadButton.B, "Back") };
			if (_page == Page.Buttons) return new[] { (Ui.PadButton.A, _binding == null ? "Bind" : "Press a button"), (Ui.PadButton.B, "Back") };
			return new[] { (Ui.PadButton.A, "Select"), (Ui.PadButton.B, "Back") };
		}
		private void RowText(int row, out string label, out string value)
		{
			DisplaySettings s = DisplaySettings.Current;
			value = null;
			switch (_page)
			{
				case Page.Main:
					label = row == 0 ? "Resume" : row == 1 ? "Settings" : "Exit game";
					return;
				case Page.Settings:
					switch (row)
					{
						case 0: label = "Window"; value = s.Mode == "windowed" ? "Windowed" : s.Mode == "borderless" ? "Borderless full screen" : "Full screen"; return;
						case 1: label = "Window size"; value = s.Width + " x " + s.Height; return;
						case 2: label = "Anti-aliasing"; value = s.Msaa == 0 ? "Off" : s.Msaa + "x"; return;
						case 3: label = "VSync"; value = s.VSync ? "On" : "Off"; return;
						case 4: label = "Run"; value = s.Run == "stick" ? "By the stick's push" : "Hold the run button"; return;
						case 5: label = "Pad buttons..."; return;
						default: label = "Back"; return;
					}
				case Page.Buttons:
					if (row < DsButtons.Length) { label = DsButtons[row].Label; value = _binding == DsButtons[row].Key ? "press a button..." : Bound(DsButtons[row].Key); return; }
					label = row == DsButtons.Length ? "Reset to the defaults" : "Back";
					return;
				default:
					label = row == 0 ? "Yes, exit" : "No, keep playing";
					return;
			}
		}

		private void Outline(Rectangle r, Color colour, int thickness)
		{
			_batch.Draw(_pixel, new Rectangle(r.X, r.Y, r.Width, thickness), colour);
			_batch.Draw(_pixel, new Rectangle(r.X, r.Bottom - thickness, r.Width, thickness), colour);
			_batch.Draw(_pixel, new Rectangle(r.X, r.Y, thickness, r.Height), colour);
			_batch.Draw(_pixel, new Rectangle(r.Right - thickness, r.Y, thickness, r.Height), colour);
		}

		private void EnsureResources()
		{
			if (_batch == null) _batch = new SpriteBatch(GraphicsDevice);
			if (_pixel == null || _pixel.IsDisposed)
			{
				_pixel = new Texture2D(GraphicsDevice, 1, 1);
				_pixel.SetData(new[] { Color.White });
			}
		}
	}
}
