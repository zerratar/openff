// The game file check's panel (GameFileCheck), in the updates' look: over the title when the game's files differ from
// the release's - how many, a few of them by name, what that risks and Steam's way back - with Continue anyway and Don't
// warn about these files again; and, when the player asked (Settings > Check game files), the check's progress bar and
// its answer whatever it is. Before a game the game holds still under it, as under the update's offer.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace OpenFF.Client
{
	using Game = Microsoft.Xna.Framework.Game;
	using GameTime = Microsoft.Xna.Framework.GameTime;
	using Color = Microsoft.Xna.Framework.Color;

	internal sealed class GameFilesScreen : DrawableGameComponent
	{
		public static GameFilesScreen Instance { get; private set; }
		public static bool IsOpen => Instance != null && Instance._open;
		/// <summary>Up before any game: the logos and the title wait under it.</summary>
		public static bool HoldsGame => IsOpen && Instance._beforeGame;

		private bool _open, _beforeGame;
		private int _selected;
		private SpriteBatch _batch;
		private int _previousPad;
		private readonly HashSet<Keys> _wasDown = new HashSet<Keys>();
		private bool _mouseWasDown;
		private static readonly Keys[] Watched = { Keys.Up, Keys.Down, Keys.W, Keys.S, Keys.Enter, Keys.Space, Keys.Z, Keys.X, Keys.Back, Keys.Escape };
		private static readonly Rectangle PanelRect = new Rectangle(140, 40, 520, 400);

		private GameFilesScreen(Game game) : base(game)
		{
			DrawOrder = int.MaxValue - 3;     // over the title and the Esc menu, under the update's offer
			UpdateOrder = int.MaxValue - 6;
		}

		public static void Attach(Game game)
		{
			Instance = new GameFilesScreen(game);
			game.Components.Add(Instance);
		}

		private static bool BeforeGame
		{
			get
			{
				try
				{
					GlobalScope.GAMEPART part = (GlobalScope.GAMEPART)GlobalScope.sys.FF3PartSys.getCurrentPart();
					return part != GlobalScope.GAMEPART.GAMEPART_WORLD && part != GlobalScope.GAMEPART.GAMEPART_BATTLE && part != GlobalScope.GAMEPART.GAMEPART_LOAD
						&& part != GlobalScope.GAMEPART.GAMEPART_SUSPEND_LOAD && part != GlobalScope.GAMEPART.GAMEPART_SPECIAL;
				}
				catch (Exception) { return false; }
			}
		}

		private string[] Rows()
		{
			switch (GameFileCheck.Now)
			{
				case GameFileCheck.Stage.Checking: return new[] { "Hide" };
				case GameFileCheck.Stage.Done:
					return GameFileCheck.Result != null && !GameFileCheck.Result.Clean ? new[] { "Continue anyway", "Don't warn about these files again" } : new[] { "OK" };
				default: return new[] { "OK" };
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
			if (!_open)
			{
				// The start's check says something only before a game, and not over the update's offer; an asked one at once.
				bool asked = GameFileCheck.Asked;
				if (!GameFileCheck.Worth || UpdateScreen.IsOpen || ModListScreen.IsOpen || (!asked && (!BeforeGame || PauseMenu.IsOpen))) return;
				_open = true;
				_beforeGame = BeforeGame;
				_selected = 0;
				_wasDown.Clear();
				KeyboardState now = Keyboard.GetState();
				foreach (Keys k in Watched) if (Down(now, k)) _wasDown.Add(k);
				_previousPad = DesktopInput.RawPadBits();
				return;
			}
			if (!Game.IsActive && !Drive.Active) return;
			KeyboardState keys = Keyboard.GetState();
			int pad = DesktopInput.RawPadBits(), edge = pad & ~_previousPad;
			_previousPad = pad;
			bool up = Pressed(keys, Keys.Up) || Pressed(keys, Keys.W) || (edge & 64) != 0;
			bool down = Pressed(keys, Keys.Down) || Pressed(keys, Keys.S) || (edge & 128) != 0;
			bool confirm = Pressed(keys, Keys.Enter) || Pressed(keys, Keys.Space) || Pressed(keys, Keys.Z) || (edge & 1) != 0;
			bool cancel = Pressed(keys, Keys.Escape) || Pressed(keys, Keys.X) || Pressed(keys, Keys.Back) || (edge & 2) != 0;
			_wasDown.Clear();
			foreach (Keys k in Watched) if (Down(keys, k)) _wasDown.Add(k);

			string[] rows = Rows();
			if (up) _selected = (_selected + rows.Length - 1) % rows.Length;
			if (down) _selected = (_selected + 1) % rows.Length;
			_selected = Math.Clamp(_selected, 0, rows.Length - 1);
			bool mouse = DesktopInput.MouseInView(out int mx, out int my);
			if (mouse && !_mouseWasDown)
				for (int i = 0; i < rows.Length; i++) if (RowRect(i, rows.Length).Contains(mx, my)) { _selected = i; confirm = true; }
			_mouseWasDown = mouse;

			if (!(confirm || cancel)) return;
			if (GameFileCheck.Now == GameFileCheck.Stage.Checking) { _open = false; return; }   // Hide: the answer comes up when it is in
			if (confirm && rows.Length == 2 && _selected == 1) GameFileCheck.Dismiss();
			else GameFileCheck.Seen();
			_open = false;
		}

		private static bool Down(KeyboardState keys, Keys key) => keys.IsKeyDown(key) || DesktopInput.Injected.Contains(key);
		private bool Pressed(KeyboardState keys, Keys key) => Down(keys, key) && !_wasDown.Contains(key);

		public override void Draw(GameTime gameTime)
		{
			if (!_open || RenderTest.Active) return;
			GlobalScope.Graphics graphics = GlobalScope.m_Graphics;
			if (graphics == null) return;
			if (_batch == null) _batch = new SpriteBatch(GraphicsDevice);
			UiTheme.Ensure(GraphicsDevice);
			Viewport view = GraphicsDevice.Viewport;
			Rectangle panel = PanelRect;
			string[] rows = Rows();
			GameFileCheck.Stage stage = GameFileCheck.Now;
			OpenFF.Content.GameFiles.Result r = GameFileCheck.Result;
			bool differ = stage == GameFileCheck.Stage.Done && r != null && !r.Clean;
			string title = stage == GameFileCheck.Stage.Checking ? "Checking the game files"
				: differ ? (r.OtherVersion ? "These game files are another version" : "The game files have been changed")
				: stage == GameFileCheck.Stage.Done ? "The game files are as Steam has them"
				: stage == GameFileCheck.Stage.NoList ? "No list to check against" : "The check did not finish";
			string sub = GameFileCheck.GameName + (r != null ? "  -  " + (differ ? r.Changed.Count + " changed, " + r.Missing.Count + " missing, of " : "all ") + r.Listed.ToString("N0", System.Globalization.CultureInfo.InvariantCulture) + " files" : "");
			Rectangle bar = new Rectangle(panel.X + 50, panel.Y + 150, panel.Width - 100, 22);
			float fraction = GameFileCheck.Total > 0 ? (float)GameFileCheck.Got / GameFileCheck.Total : 0f;

			_batch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
			UiTheme.Panel(_batch, panel, view);
			UiTheme.IconAt(_batch, differ || stage == GameFileCheck.Stage.Failed ? UiTheme.Icon.Exit : UiTheme.Icon.Resume, panel.X + 48, panel.Y + 31, 36, view);
			UiTheme.Divider(_batch, panel.Center.X, panel.Y + 78, panel.Width - 60, view);
			if (stage == GameFileCheck.Stage.Checking) UiTheme.Progress(_batch, bar, fraction, view);
			for (int i = 0; i < rows.Length; i++) UiTheme.Row(_batch, RowRect(i, rows.Length), i == _selected, view, diamonds: i == _selected);
			_batch.End();

			graphics.SetImageOrigin(0f, 0f);
			graphics.SetImageRotation(0f);
			graphics.SetImageScale(1f, 1f);
			graphics.DrawStringStart();
			TrueTypeText.TitleFace = true;
			try
			{
				Ui.Left(graphics, title, panel.X + 74, panel.Y + 14, 34, 15, UiTheme.Ink);
				Ui.Left(graphics, sub, panel.X + 76, panel.Y + 46, 20, 8, UiTheme.Sub);
				float y = panel.Y + 92;
				List<string> lines = new List<string>();
				if (stage == GameFileCheck.Stage.Checking)
				{
					string percent = (int)Math.Round(fraction * 100) + "%";
					Ui.Left(graphics, "Reading the files...", panel.X + 50, panel.Y + 110, 26, 10, UiTheme.Ink);
					Ui.Left(graphics, percent, bar.Right - Ui.Width(graphics, percent, 12), panel.Y + 108, 30, 12, UiTheme.GoldBright);
				}
				else if (differ)
				{
					lines.Add(r.OtherVersion
						? "Most of them differ from the Steam release OpenFF knows: this looks like another version of the game (the phone's, or one OpenFF has no list for). Menus, text and scenes may look or behave wrong."
						: "Files a mod installed into the game folder, or from another version, are read as they are: menus, text and scenes may look or behave wrong, or crash.");
					lines.Add("");
					foreach (string f in r.Changed.Take(4)) lines.Add("changed:  " + f);
					foreach (string f in r.Missing.Take(Math.Max(0, 4 - Math.Min(4, r.Changed.Count)))) lines.Add("missing:  " + f);
					int more = r.Changed.Count + r.Missing.Count - Math.Min(4, r.Changed.Count + r.Missing.Count);
					if (more > 0) lines.Add("... and " + more + " more (the log lists them)");
					lines.Add("");
					lines.Add("To put the game's own back: Steam > " + GameFileCheck.GameName + " > Properties > Installed Files > Verify integrity of game files.");
				}
				else if (stage == GameFileCheck.Stage.Done) lines.Add("Every file OpenFF reads is the one the Steam release has.");
				else if (stage == GameFileCheck.Stage.NoList) lines.Add("The game is played from a folder OpenFF has no list of files for (not a Steam install).");
				else lines.Add(GameFileCheck.Error ?? "");
				foreach (string line in Wrap(graphics, lines, 8, panel.Width - 100, 11)) { Ui.Left(graphics, line, panel.X + 50, y, 18, 8, line.StartsWith("changed:") || line.StartsWith("missing:") ? UiTheme.Sub : UiTheme.Ink); y += 17; }
				for (int i = 0; i < rows.Length; i++) Ui.Centred(graphics, rows[i], RowRect(i, rows.Length), 11, i == _selected ? Color.White : UiTheme.Ink);
			}
			finally { TrueTypeText.TitleFace = false; }
			graphics.DrawStringEnd();
		}

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
	}
}
