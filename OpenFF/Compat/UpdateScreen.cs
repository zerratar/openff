// The updates' screen over the title (Updates): "Checking for updates..." in its corner while the
// client asks GitHub; a new version offered in a panel of the menus' look - its version, the first
// notes (scrolled with the wheel, Page Up / Down, Q / E or L / R when longer than their room), Update now / Later / Skip this version; the download's progress bar with its
// size, rate and time left; the check and the unpacking; a failure said plainly with OK. Once the
// updater is running the game closes, and the updater starts it again.
//
// Only before a game is started or loaded - over the logos, the prologue or the title - so nothing
// unsaved is lost to the restart; the game holds still under it there (GameHost), and a version found
// once playing waits for the next start. With settings.json's "updates": "auto" the download starts without asking.

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

	internal sealed class UpdateScreen : DrawableGameComponent
	{
		public static UpdateScreen Instance { get; private set; }
		public static bool IsOpen => Instance != null && Instance._open;

		private bool _open;
		private int _selected;
		private SpriteBatch _batch;
		private int _previousPad;
		private readonly HashSet<Keys> _wasDown = new HashSet<Keys>();
		private bool _mouseWasDown;
		private int _scroll, _wheel, _noteCount, _noteFit;
		private static readonly Keys[] Watched = { Keys.Up, Keys.Down, Keys.W, Keys.S, Keys.Enter, Keys.Space, Keys.Z, Keys.X, Keys.Back, Keys.Escape, Keys.PageUp, Keys.PageDown, Keys.Q, Keys.E };
		private const int NoteTop = 92, NoteStep = 17;

		private static readonly Rectangle PanelRect = new Rectangle(160, 40, 480, 400);

		private UpdateScreen(Game game) : base(game)
		{
			DrawOrder = int.MaxValue - 2;     // over the title, the mod list's label and the Esc menu
			UpdateOrder = int.MaxValue - 5;
		}

		public static void Attach(Game game)
		{
			Instance = new UpdateScreen(game);
			game.Components.Add(Instance);
		}

		/// <summary>No game started or loaded yet (the logos, the prologue, the title), and nothing else of the client's over the screen.</summary>
		private static bool AtStart => BeforeGame && !ModListScreen.IsOpen && !PauseMenu.IsOpen && !(TextEntry.Instance != null && TextEntry.Instance.IsActive);

		// A game started (a new one, a load, a --map test start): from then on, no offer this run.
		private static bool _started = GameProfile.StartStage != null;

		private static bool BeforeGame
		{
			get
			{
				if (_started) return false;
				GlobalScope.GAMEPART part;
				try { part = (GlobalScope.GAMEPART)GlobalScope.sys.FF3PartSys.getCurrentPart(); } catch (Exception) { return false; }
				if (part == GlobalScope.GAMEPART.GAMEPART_WORLD || part == GlobalScope.GAMEPART.GAMEPART_BATTLE || part == GlobalScope.GAMEPART.GAMEPART_LOAD
					|| part == GlobalScope.GAMEPART.GAMEPART_SUSPEND_LOAD || part == GlobalScope.GAMEPART.GAMEPART_SPECIAL) _started = true;
				return !_started;
			}
		}

		/// <summary>The panel is up before any game: the game holds still under it (GameHost takes no steps), the logos and the prologue waiting.</summary>
		public static bool HoldsGame => IsOpen && !_started;

		private string[] Rows()
		{
			switch (Updates.Now)
			{
				case Updates.Stage.Available: return new[] { "Update now", "Later", "Skip this version" };
				case Updates.Stage.Downloading: return new[] { "Cancel" };
				case Updates.Stage.Failed: return new[] { "OK" };
				default: return Array.Empty<string>();
			}
		}

		private static Rectangle RowRect(int row, int count)
		{
			const int h = 28, step = 34;
			int top = PanelRect.Bottom - 46 - count * step;
			return new Rectangle(PanelRect.X + 60, top + row * step, PanelRect.Width - 120, h);
		}

		public override void Update(GameTime gameTime)
		{
			if (Updates.ReadyToExit)
			{
				// The updater is waiting for this process to end.
				DisplaySettings.Current.Save();
				Log.Write(LogChannel.General, "updates: closing for the updater");
				Game.Exit();
				return;
			}
			Updates.Stage stage = Updates.Now;
			bool busy = stage == Updates.Stage.Downloading || stage == Updates.Stage.Verifying || stage == Updates.Stage.Unpacking || stage == Updates.Stage.Installing;
			if (!_open)
			{
				bool offer = stage == Updates.Stage.Available || (stage == Updates.Stage.Failed && Updates.Found != null);
				if (!AtStart || !(offer || busy)) return;
				_open = true;
				_selected = 0;
				_scroll = 0;
				_wheel = Mouse.GetState().ScrollWheelValue;
				_wasDown.Clear();
				KeyboardState now = Keyboard.GetState();
				foreach (Keys k in Watched) if (Down(now, k)) _wasDown.Add(k);
				_previousPad = DesktopInput.RawPadBits();
				if (stage == Updates.Stage.Available && Updates.Mode == "auto") Updates.Install();
				Log.Write(LogChannel.General, "updates: offered " + Updates.Found?.Tag);
				return;
			}
			if (!(stage == Updates.Stage.Available || stage == Updates.Stage.Failed || busy)) { _open = false; return; }
			if (!Game.IsActive && !Drive.Active) return;

			KeyboardState keys = Keyboard.GetState();
			int pad = DesktopInput.RawPadBits(), edge = pad & ~_previousPad;
			_previousPad = pad;
			bool up = Pressed(keys, Keys.Up) || Pressed(keys, Keys.W) || (edge & 64) != 0;
			bool down = Pressed(keys, Keys.Down) || Pressed(keys, Keys.S) || (edge & 128) != 0;
			bool confirm = Pressed(keys, Keys.Enter) || Pressed(keys, Keys.Space) || Pressed(keys, Keys.Z) || (edge & 1) != 0;
			bool cancel = Pressed(keys, Keys.Escape) || Pressed(keys, Keys.X) || Pressed(keys, Keys.Back) || (edge & 2) != 0;
			// The notes scrolled: the wheel a line a notch; Page Up / Down, Q / E and L / R a page.
			int wheel = Mouse.GetState().ScrollWheelValue, notch = (wheel - _wheel) / 120;
			if (notch != 0) _wheel = wheel;
			int page = Math.Max(1, NoteRows(Rows().Length) - 1);
			int by = -notch
				+ ((Pressed(keys, Keys.PageDown) || Pressed(keys, Keys.E) || (edge & 0x100) != 0) ? page : 0)
				- ((Pressed(keys, Keys.PageUp) || Pressed(keys, Keys.Q) || (edge & 0x200) != 0) ? page : 0);
			if (by != 0) _scroll = Math.Clamp(_scroll + by, 0, Math.Max(0, _noteCount - _noteFit));
			_wasDown.Clear();
			foreach (Keys k in Watched) if (Down(keys, k)) _wasDown.Add(k);

			string[] rows = Rows();
			if (rows.Length > 0)
			{
				if (up) _selected = (_selected + rows.Length - 1) % rows.Length;
				if (down) _selected = (_selected + 1) % rows.Length;
				_selected = Math.Clamp(_selected, 0, rows.Length - 1);
			}
			bool mouse = DesktopInput.MouseInView(out int mx, out int my);
			if (mouse && !_mouseWasDown)
			{
				for (int i = 0; i < rows.Length; i++) if (RowRect(i, rows.Length).Contains(mx, my)) { _selected = i; confirm = true; }
			}
			_mouseWasDown = mouse;

			switch (stage)
			{
				case Updates.Stage.Available:
					if (cancel) Updates.Later();
					else if (confirm)
					{
						if (_selected == 0) Updates.Install();
						else if (_selected == 1) Updates.Later();
						else Updates.Skip();
						_selected = 0;
					}
					break;
				case Updates.Stage.Downloading:
					if (cancel || confirm) Updates.Cancel();
					break;
				case Updates.Stage.Failed:
					if (cancel || confirm) Updates.Later();
					break;
			}
		}

		private static bool Down(KeyboardState keys, Keys key) => keys.IsKeyDown(key) || DesktopInput.Injected.Contains(key);
		private bool Pressed(KeyboardState keys, Keys key) => Down(keys, key) && !_wasDown.Contains(key);

		public override void Draw(GameTime gameTime)
		{
			if (RenderTest.Active) return;
			GlobalScope.Graphics graphics = GlobalScope.m_Graphics;
			if (graphics == null) return;
			Updates.Stage stage = Updates.Now;
			bool checking = stage == Updates.Stage.Checking && AtStart && !Updates.Asked;
			if (!_open && !checking) return;
			if (_batch == null) _batch = new SpriteBatch(GraphicsDevice);
			UiTheme.Ensure(GraphicsDevice);
			Viewport view = GraphicsDevice.Viewport;

			if (!_open)
			{
				// The corner's quiet note while the client asks.
				graphics.SetImageOrigin(0f, 0f);
				graphics.SetImageRotation(0f);
				graphics.SetImageScale(1f, 1f);
				graphics.DrawStringStart();
				string text = "Checking for updates...";
				Ui.Left(graphics, text, Ui.W - 16 - Ui.Width(graphics, text, 8), Ui.H - 30, 20, 8, new Color(120, 120, 130, 255));
				graphics.DrawStringEnd();
				return;
			}

			Rectangle panel = PanelRect;
			string[] rows = Rows();
			Updates.Release r = Updates.Found;
			string title = stage == Updates.Stage.Available ? "Update available"
				: stage == Updates.Stage.Failed ? "The update did not go in"
				: "Updating to " + (r?.Tag ?? "");
			string sub = r == null ? "" : stage == Updates.Stage.Available ? "OpenFF " + r.Version.ToString(3) + "  -  you have " + Updates.Current.ToString(3) : "";
			Rectangle bar = new Rectangle(panel.X + 50, panel.Y + 150, panel.Width - 100, 22);
			float fraction = Updates.Total > 0 ? (float)Updates.Got / Updates.Total : 0f;
			bool progress = stage == Updates.Stage.Downloading || stage == Updates.Stage.Verifying || stage == Updates.Stage.Unpacking || stage == Updates.Stage.Installing;

			_batch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
			UiTheme.Panel(_batch, panel, view);
			UiTheme.IconAt(_batch, stage == Updates.Stage.Failed ? UiTheme.Icon.Exit : UiTheme.Icon.Resume, panel.X + 48, panel.Y + 31, 36, view);
			UiTheme.Divider(_batch, panel.Center.X, panel.Y + 78, panel.Width - 60, view);
			if (progress) UiTheme.Progress(_batch, bar, stage == Updates.Stage.Downloading ? fraction : 1f, view);
			for (int i = 0; i < rows.Length; i++) UiTheme.Row(_batch, RowRect(i, rows.Length), i == _selected, view, diamonds: i == _selected);
			// The notes' scrollbar while they are longer than their room: the track, and the part in view.
			if (stage == Updates.Stage.Available && _noteCount > _noteFit && _noteFit > 0)
			{
				int top = panel.Y + NoteTop + 2, height = _noteFit * NoteStep - 4;
				Rectangle track = new Rectangle(panel.Right - 42, top, 4, height);
				int thumbH = Math.Max(14, height * _noteFit / _noteCount);
				int thumbY = top + (height - thumbH) * _scroll / Math.Max(1, _noteCount - _noteFit);
				UiTheme.Box(_batch, track, new Color(0, 0, 0, 0), new Color(0, 0, 0, 120), view, 2f);
				UiTheme.Box(_batch, new Rectangle(track.X - 1, thumbY, 6, thumbH), UiTheme.Gold, UiTheme.Gold, view, 3f);
			}
			_batch.End();

			graphics.SetImageOrigin(0f, 0f);
			graphics.SetImageRotation(0f);
			graphics.SetImageScale(1f, 1f);
			graphics.DrawStringStart();
			TrueTypeText.TitleFace = true;
			try
			{
				Ui.Left(graphics, title, panel.X + 74, panel.Y + 14, 34, 15, UiTheme.Ink);
				if (sub.Length > 0) Ui.Left(graphics, sub, panel.X + 76, panel.Y + 46, 20, 8, UiTheme.Sub);
				float y = panel.Y + 92;
				if (stage == Updates.Stage.Available)
				{
					// As many lines as fit above the buttons, from where the notes are scrolled to.
					List<string> notes = Wrap(graphics, Updates.NoteLines(400), 8, panel.Width - 110, int.MaxValue);
					_noteCount = notes.Count;
					_noteFit = NoteRows(rows.Length);
					_scroll = Math.Clamp(_scroll, 0, Math.Max(0, _noteCount - _noteFit));
					for (int i = _scroll; i < notes.Count && i < _scroll + _noteFit; i++) { Ui.Left(graphics, notes[i], panel.X + 50, y, 18, 8, UiTheme.Ink); y += NoteStep; }
				}
				else if (stage == Updates.Stage.Failed)
				{
					foreach (string line in Wrap(graphics, new List<string> { Updates.Error ?? "", "", "The game is as it was; the update is offered again at the next start." }, 8, panel.Width - 100, 7)) { Ui.Left(graphics, line, panel.X + 50, y, 18, 8, UiTheme.Ink); y += 17; }
				}
				else
				{
					string what = stage == Updates.Stage.Downloading ? "Downloading..." : stage == Updates.Stage.Verifying ? "Checking the download..." : stage == Updates.Stage.Unpacking ? "Unpacking..." : "The game restarts in a moment...";
					Ui.Left(graphics, what, panel.X + 50, panel.Y + 110, 26, 10, UiTheme.Ink);
					if (stage == Updates.Stage.Downloading)
					{
						string percent = (int)Math.Round(fraction * 100) + "%";
						Ui.Left(graphics, percent, bar.Right - Ui.Width(graphics, percent, 12), panel.Y + 108, 30, 12, UiTheme.GoldBright);
						string detail = Mb(Updates.Got) + " / " + Mb(Updates.Total) + " MB";
						if (Updates.Rate > 0) detail += "     " + Mb((long)Updates.Rate) + " MB/s     " + Left() + " left";
						Ui.Left(graphics, detail, bar.X, bar.Bottom + 8, 20, 8, UiTheme.Sub);
					}
				}
			}
			finally { TrueTypeText.TitleFace = false; }
			TrueTypeText.TitleFace = true;
			try
			{
				for (int i = 0; i < rows.Length; i++) Ui.Centred(graphics, rows[i], RowRect(i, rows.Length), 11, i == _selected ? Color.White : UiTheme.Ink);
			}
			finally { TrueTypeText.TitleFace = false; }
			graphics.DrawStringEnd();
		}

		private static string Mb(long bytes) => (bytes / 1048576.0).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);

		private static string Left()
		{
			if (Updates.Rate <= 0 || Updates.Total <= 0) return "";
			double s = (Updates.Total - Updates.Got) / Updates.Rate;
			return s >= 90 ? Math.Ceiling(s / 60) + " min" : Math.Max(1, (int)Math.Ceiling(s)) + " s";
		}

		/// <summary>Lines broken at the words that pass a width, at most so many.</summary>
		private static List<string> Wrap(GlobalScope.Graphics g, List<string> lines, int size, float width, int most)
		{
			List<string> result = new List<string>();
			foreach (string line in lines)
			{
				if (line.Length == 0) { result.Add(""); continue; }
				string current = "";
				foreach (string word in line.Split(' '))
				{
					string attempt = current.Length == 0 ? word : current + " " + word;
					if (current.Length > 0 && Ui.Width(g, attempt, size) > width) { result.Add(current); current = word; }
					else current = attempt;
				}
				if (current.Length > 0) result.Add(current);
			}
			if (result.Count > most) { result = result.GetRange(0, most); result[most - 1] += " ..."; }
			return result;
		}

		/// <summary>How many lines of notes fit between the divider and the first button.</summary>
		private static int NoteRows(int rows) =>
			Math.Max(1, (RowRect(0, Math.Max(1, rows)).Y - 10 - (PanelRect.Y + NoteTop)) / NoteStep);
	}
}
