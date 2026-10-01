// The in-game mod list, on the title screen where the phone's network entry was.
//
// The title's fourth command (network / achievements on the phone) opens this instead:
// every mod in mods/ beside the executable, in load order, with its enabled flag, what
// it brings (files, code), and why it is skipped when it is. Up/Down select, Space or
// Enter toggle, Shift+Up/Down move a mod in the order, Esc (or the Back button) closes
// and writes mods/loadorder.json. The mouse does the same: a row toggles, the arrows
// beside it move it. What a mod brings is taken in as the client starts, so closing the
// list with a different set of mods restarts the client on the title (Restart).
//
// Drawn with the game's own font and a SpriteBatch panel, like the text entry, because
// there is no picture for any of this in either game's banks. The title's text labels
// are drawn here too, at the positions the title gives (ttl.TitleLabels), in the title's
// face: "Mods" (Steam's title bank has no picture in that cell), the mods' entries, and
// the game's own commands when they are text.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using OpenFF.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace OpenFF.Client
{
	// MonoGame's types by name: the engine's OpenFF.Game, GameTime, Color and vectors sit a namespace up.
	using Game = Microsoft.Xna.Framework.Game;
	using GameTime = Microsoft.Xna.Framework.GameTime;
	using Color = Microsoft.Xna.Framework.Color;

	internal sealed class ModListScreen : DrawableGameComponent
	{
		private const float TextSpaceWidth = 800f;
		private const float TextSpaceHeight = 480f;
		private const int RowSize = 10;   // the title's own labels' note

		public static ModListScreen Instance { get; private set; }

		/// <summary>Whether the title should offer the entry at all: always, on this client.</summary>
		public static bool Available => Instance != null;

		public static bool IsOpen => Instance != null && Instance._open;

		private bool _open;
		private List<InstalledMod> _mods = new List<InstalledMod>();
		private int _selected;
		private int _scroll;
		private bool _dirty;
		private string _folder;
		private int _conflicts;
		private KeyboardState _previousKeys;
		private int _previousPad;
		private MouseState _previousMouse;
		private SpriteBatch _batch;
		private Texture2D _pixel;

		private ModListScreen(Game game)
			: base(game)
		{
			// Over the game, under the debug overlay and the screenshot capture.
			DrawOrder = int.MaxValue - 4;
			UpdateOrder = int.MaxValue - 4;
		}

		public static void Attach(Game game)
		{
			Instance = new ModListScreen(game);
			game.Components.Add(Instance);
		}

		/// <summary>Opens the list, reading the mods folder afresh.</summary>
		public static void Open()
		{
			if (Instance == null || Instance._open) return;
			Instance._folder = ModsFolder.Beside(AppContext.BaseDirectory);
			Instance._mods = ModsFolder.Load(Instance._folder);
			Instance.Refresh();
			Instance._selected = 0;
			Instance._scroll = 0;
			Instance._dirty = false;
			Instance._open = true;
			Instance._previousKeys = Keyboard.GetState();
			Instance._previousMouse = Mouse.GetState();
			Log.Write(LogChannel.General, "mod list: opened, " + Instance._mods.Count + " mod(s) in " + Instance._folder);
		}

		private void Refresh()
		{
			List<InstalledMod> active = ModsFolder.Active(_mods);
			_conflicts = ModsFolder.Conflicts(active).Count;
		}

		private void Close()
		{
			if (!_open) return;
			_open = false;
			if (_dirty)
			{
				try
				{
					ModsFolder.SaveOrder(_folder, _mods);
					Log.Write(LogChannel.General, "mod list: loadorder.json written - " + string.Join(", ", _mods.Select(m => m.Key + (m.Enabled ? "" : " (off)"))));
				}
				catch (Exception ex)
				{
					Log.Write(LogChannel.General, "mod list: loadorder.json not written: " + ex.Message);
					return;
				}
				// What a mod brings is taken in as the client starts, so a different set of mods (or order)
				// than this run's is applied by starting again, on the title (Restart).
				IEnumerable<string> running = GameArchive.ActiveMods.Select(m => m.Key);
				IEnumerable<string> wanted = ModsFolder.Active(_mods).Select(m => m.Key);
				if (!running.SequenceEqual(wanted, StringComparer.OrdinalIgnoreCase) && Restart.ToTitle("the mod list changed"))
				{
					DisplaySettings.Current.Save();
					Game.Exit();
				}
			}
			else
			{
				Log.Write(LogChannel.General, "mod list: closed, nothing changed");
			}
		}

		public override void Update(GameTime gameTime)
		{
			if (!_open || !Game.IsActive)
			{
				return;
			}
			KeyboardState keys = Keyboard.GetState();
			MouseState mouse = Mouse.GetState();
			bool shift = keys.IsKeyDown(Keys.LeftShift) || keys.IsKeyDown(Keys.RightShift);
			// A game pad drives the list as the keys do: the d-pad or stick, A toggles, B closes, the shoulders move a mod.
			int pad = DesktopInput.RawPadBits(), padEdge = pad & ~_previousPad;
			_previousPad = pad;
			bool padUp = (padEdge & 64) != 0, padDown = (padEdge & 128) != 0, padA = (padEdge & 1) != 0, padB = (padEdge & 2) != 0, padL = (padEdge & 512) != 0, padR = (padEdge & 256) != 0;

			if (Pressed(keys, Keys.Escape) || Pressed(keys, Keys.Back) || Pressed(keys, Keys.X) || padB)
			{
				Close();
			}
			else if (_mods.Count > 0)
			{
				if (Pressed(keys, Keys.Up) || Pressed(keys, Keys.W) || padUp)
				{
					if (shift) Move(-1); else _selected = (_selected + _mods.Count - 1) % _mods.Count;
				}
				else if (Pressed(keys, Keys.Down) || Pressed(keys, Keys.S) || padDown)
				{
					if (shift) Move(1); else _selected = (_selected + 1) % _mods.Count;
				}
				else if (Pressed(keys, Keys.Space) || Pressed(keys, Keys.Enter) || Pressed(keys, Keys.Z) || padA)
				{
					Toggle(_selected);
				}
				else if (Pressed(keys, Keys.PageUp) || padL)
				{
					Move(-1);
				}
				else if (Pressed(keys, Keys.PageDown) || padR)
				{
					Move(1);
				}
			}

			if (mouse.LeftButton == ButtonState.Pressed && _previousMouse.LeftButton == ButtonState.Released)
			{
				Click(mouse.X, mouse.Y);
			}
			if (mouse.ScrollWheelValue != _previousMouse.ScrollWheelValue && _mods.Count > RowsPerPage)
			{
				_scroll = Math.Clamp(_scroll - Math.Sign(mouse.ScrollWheelValue - _previousMouse.ScrollWheelValue), 0, Math.Max(0, _mods.Count - RowsPerPage));
			}
			if (_selected < _scroll) _scroll = _selected;
			if (_selected >= _scroll + RowsPerPage) _scroll = _selected - RowsPerPage + 1;

			_previousKeys = keys;
			_previousMouse = mouse;
		}

		private bool Pressed(KeyboardState now, Keys key) => now.IsKeyDown(key) && !_previousKeys.IsKeyDown(key);

		private void Toggle(int index)
		{
			if (index < 0 || index >= _mods.Count) return;
			_mods[index].Enabled = !_mods[index].Enabled;
			_dirty = true;
			Refresh();
		}

		private void Move(int by)
		{
			int target = _selected + by;
			if (target < 0 || target >= _mods.Count) return;
			InstalledMod mod = _mods[_selected];
			_mods.RemoveAt(_selected);
			_mods.Insert(target, mod);
			_selected = target;
			_dirty = true;
			Refresh();
		}

		// The look (UiTheme, as the Esc menu's): the panel and its rows in the 800x480 text space.
		private static readonly Rectangle PanelRect = new Rectangle(130, 44, 540, 398);
		private const float ListTop = 134f, RowStep = 35f, RowH = 30f;
		private const int RowsPerPage = 6;
		private static Rectangle RowRect(int row) => new Rectangle(PanelRect.X + 22, (int)(ListTop + row * RowStep), PanelRect.Width - 44, (int)RowH);
		private static Rectangle UpButton(Rectangle r) => new Rectangle(r.Right - 70, r.Y + 4, 28, (int)RowH - 8);
		private static Rectangle DownButton(Rectangle r) => new Rectangle(r.Right - 36, r.Y + 4, 28, (int)RowH - 8);
		private static Rectangle FooterRect => new Rectangle(PanelRect.X + 20, (int)(ListTop + RowsPerPage * RowStep + 2), PanelRect.Width - 40, 84);
		private static Rectangle BackButton => new Rectangle(400 - 76, FooterRect.Bottom - 29, 152, 23);

		/// <summary>A click in viewport pixels: on a row toggles it, on its arrows moves it, on Back closes.</summary>
		private void Click(int px, int py)
		{
			Viewport view = GraphicsDevice.Viewport;
			int tx = (int)(px / (float)view.Width * Ui.W);
			int ty = (int)(py / (float)view.Height * Ui.H);
			if (BackButton.Contains(tx, ty))
			{
				Close();
				return;
			}
			for (int row = 0; row < RowsPerPage; row++)
			{
				int index = _scroll + row;
				if (index >= _mods.Count) break;
				Rectangle r = RowRect(row);
				if (!r.Contains(tx, ty)) continue;
				_selected = index;
				if (UpButton(r).Contains(tx, ty)) Move(-1);
				else if (DownButton(r).Contains(tx, ty)) Move(1);
				else Toggle(index);
				return;
			}
		}

		public override void Draw(GameTime gameTime)
		{
			if (RenderTest.Active)
			{
				return;
			}
			GlobalScope.Graphics graphics = GlobalScope.m_Graphics;
			if (graphics == null)
			{
				return;
			}
			if (!_open)
			{
				if (TitleEntries.Showing && !TextEntry.Instance?.IsActive == true)
				{
					DrawTitleLabel(graphics);
				}
				return;
			}
			EnsureResources();
			UiTheme.Ensure(GraphicsDevice);
			Viewport view = GraphicsDevice.Viewport;
			Rectangle panel = PanelRect, footer = FooterRect, back = BackButton;
			bool pad = Ui.PadConnected;
			const int titleSize = 16, subSize = 8, rowSize = 11, descSize = 10, footSize = 9;

			_batch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
			UiTheme.Panel(_batch, panel, view);
			UiTheme.IconAt(_batch, UiTheme.Icon.Settings, panel.X + 48, panel.Y + 32, 40, view);
			UiTheme.Divider(_batch, panel.Center.X, panel.Y + 80, panel.Width - 60, view);
			for (int row = 0; row < RowsPerPage; row++)
			{
				int index = _scroll + row;
				if (index >= _mods.Count) break;
				Rectangle r = RowRect(row);
				bool lit = index == _selected, on = _mods[index].Enabled;
				UiTheme.Row(_batch, r, lit, view);
				// The checkbox: a gold tick on the lit row, a blue one on an enabled mod, empty when it is off.
				Rectangle box = new Rectangle(r.X + 10, r.Y + 5, (int)RowH - 10, (int)RowH - 10);
				Color edge = lit ? UiTheme.GoldBright : on ? new Color(110, 160, 240, 255) : new Color(110, 118, 140, 255);
				UiTheme.Box(_batch, box, edge, new Color(6, 14, 36, 240), view);
				if (on) Ui.IconAt(_batch, Ui.Icon.Check, box.Center.X, box.Center.Y, box.Height * 1.05f, lit ? UiTheme.GoldBright : new Color(120, 180, 255, 255), view);
				// The order's arrows.
				foreach ((Rectangle b, Ui.Icon icon, bool can) in new[] { (UpButton(r), Ui.Icon.Up, index > 0), (DownButton(r), Ui.Icon.Down, index < _mods.Count - 1) })
				{
					UiTheme.Box(_batch, b, new Color(120, 132, 168, 230), new Color(8, 16, 40, 240), view);
					Ui.IconAt(_batch, icon, b.Center.X, b.Center.Y, b.Height * 0.8f, can ? UiTheme.Ink : new Color(110, 118, 140, 255), view);
				}
			}
			// The footer: its hints and note, a rule, the Back button with a diamond at each end.
			UiTheme.NoteBox(_batch, footer, view);
			UiTheme.Divider(_batch, footer.Center.X, footer.Bottom - 35, footer.Width - 120, view, 8f);
			UiTheme.Box(_batch, back, UiTheme.Gold, new Color(14, 30, 78, 245), view, 8f);
			UiTheme.Diamond(_batch, back.X, back.Center.Y, 10, UiTheme.GoldBright, view);
			UiTheme.Diamond(_batch, back.Right, back.Center.Y, 10, UiTheme.GoldBright, view);
			(Ui.PadButton, string)[] padHints = { (Ui.PadButton.A, "Toggle"), (Ui.PadButton.L, "Up"), (Ui.PadButton.R, "Down"), (Ui.PadButton.B, "Back") };
			float[] padX = new float[padHints.Length];
			if (pad)
			{
				float total = 0;
				TrueTypeText.TitleFace = true;
				try
				{
					foreach ((Ui.PadButton b, string w) in padHints) total += Ui.HintWidth(graphics, w, footSize, 18, b) + 22;
					float x = footer.Center.X - (total - 22) / 2;
					for (int i = 0; i < padHints.Length; i++) { padX[i] = x; x += Ui.HintWidth(graphics, padHints[i].Item2, footSize, 18, padHints[i].Item1) + 22; }
				}
				finally { TrueTypeText.TitleFace = false; }
				for (int i = 0; i < padHints.Length; i++) Ui.HintShape(_batch, padHints[i].Item1, padX[i], footer.Y + 12, 18, view);
			}
			_batch.End();

			graphics.SetImageOrigin(0f, 0f);
			graphics.SetImageRotation(0f);
			graphics.SetImageScale(1f, 1f);
			graphics.DrawStringStart();
			TrueTypeText.TitleFace = true;
			try
			{
				Ui.Left(graphics, "Mods", panel.X + 78, panel.Y + 12, 34, titleSize, UiTheme.Ink);
				string sub = _mods.Count == 0
					? "No mods yet. A mod is a folder in the mods folder beside the game."
					: _mods.Count + " mod(s) in the mods folder beside the game" + (_conflicts > 0 ? "     " + _conflicts + " shared file(s), the earlier wins" : "");
				Ui.Left(graphics, sub, panel.X + 80, panel.Y + 46, 20, subSize, UiTheme.Sub);
				if (_mods.Count > RowsPerPage)
				{
					string page = (_scroll + 1) + "-" + Math.Min(_mods.Count, _scroll + RowsPerPage) + " of " + _mods.Count;
					Ui.Left(graphics, page, panel.Right - 30 - Ui.Width(graphics, page, subSize), panel.Y + 46, 20, subSize, UiTheme.Sub);
				}
				for (int row = 0; row < RowsPerPage; row++)
				{
					int index = _scroll + row;
					if (index >= _mods.Count) break;
					InstalledMod mod = _mods[index];
					Rectangle r = RowRect(row);
					bool lit = index == _selected, on = mod.Enabled;
					string name = (index + 1) + ".  " + Fit(mod.DisplayName, 26) + (string.IsNullOrWhiteSpace(mod.Manifest.Version) ? "" : "   " + mod.Manifest.Version);
					Ui.Left(graphics, name, r.X + 40, r.Y, r.Height, rowSize, lit ? Color.White : on ? UiTheme.Ink : UiTheme.Hint);
					Ui.Left(graphics, Fit(Describe(mod), 34), r.X + (int)(r.Width * 0.54f), r.Y, r.Height, descSize, lit ? new Color(255, 236, 190, 255) : UiTheme.Hint);
				}
				// The hints: the keyboard's keys in gold with what they do, or the pad's buttons.
				if (pad)
				{
					for (int i = 0; i < padHints.Length; i++) Ui.HintText(graphics, padHints[i].Item1, padHints[i].Item2, padX[i], footer.Y + 12, 18, footSize);
				}
				else
				{
					(string Key, string What)[] keys = { ("Up/Down", "select"), ("Space", "toggle"), ("Shift+Up/Down", "move"), ("Esc", "back") };
					float total = 0;
					foreach ((string k, string w) in keys) total += Ui.Width(graphics, k, footSize) + 6 + Ui.Width(graphics, w, footSize) + 22;
					float x = footer.Center.X - (total - 22) / 2;
					foreach ((string k, string w) in keys)
					{
						Ui.Left(graphics, k, x, footer.Y + 4, 18, footSize, UiTheme.GoldBright);
						x += Ui.Width(graphics, k, footSize) + 6;
						Ui.Left(graphics, w, x, footer.Y + 4, 18, footSize, UiTheme.Ink);
						x += Ui.Width(graphics, w, footSize) + 22;
					}
				}
				string note = Restart.Possible ? "Closing the list restarts the game with your changes." : "Changes apply at the next start.";
				Ui.Centred(graphics, note, new Rectangle(footer.X, footer.Y + 23, footer.Width, 18), footSize - 1, UiTheme.Sub);
				Ui.Centred(graphics, "Back", back, 11, UiTheme.Ink);
			}
			finally { TrueTypeText.TitleFace = false; }
			graphics.DrawStringEnd();
		}

		private static string Fit(string text, int max)
		{
			return string.IsNullOrEmpty(text) || text.Length <= max ? text : text.Substring(0, max - 1) + "…";
		}

		private static string Describe(InstalledMod mod)
		{
			List<string> parts = new List<string>();
			int files = ModsFolder.FileCount(mod);
			if (files > 0) parts.Add(files + " file" + (files == 1 ? "" : "s"));
			if (ModsFolder.HasCode(mod)) parts.Add("code");
			if (!mod.Manifest.ForOpenFF) parts.Add("for " + mod.Manifest.Target);
			string text = parts.Count == 0 ? "empty" : string.Join(", ", parts);
			if (mod.Enabled && mod.Skipped != null && mod.Skipped != "disabled")
			{
				text += "  - skipped: " + mod.Skipped;
			}
			return text;
		}

		/// <summary>The title's text labels where its commands sit, in the title's own LCD units and its face.</summary>
		private void DrawTitleLabel(GlobalScope.Graphics graphics)
		{
			// LCD units to the game's 800x480 text space, the way the ortho projection maps them.
			float ox = (480 - GlobalScope.LCD_WIDTH) / 2f;
			float oy = (320 - GlobalScope.LCD_HEIGHT) / 2f;
			graphics.SetImageOrigin(0f, 0f);
			graphics.SetImageRotation(0f);
			graphics.SetImageScale(1f, 1f);
			graphics.DrawStringStart();
			TrueTypeText.TitleFace = true;
			graphics.SetImageScale(GlobalScope.ttl.TITLE_LABEL_SCALE_X, 1f);
			try
			{
				foreach (GlobalScope.ttl.TitleLabel label in GlobalScope.ttl.TitleLabels)
				{
					float tx = (label.X - ox) / GlobalScope.LCD_WIDTH * TextSpaceWidth + GlobalScope.ttl.TITLE_LABEL_NUDGE_X;
					float ty = (label.Y - oy) / GlobalScope.LCD_HEIGHT * TextSpaceHeight + GlobalScope.ttl.TITLE_LABEL_DROP;
					// The field_hud layout's look for the title's rows (title/row; :focus the hand's, :disabled a Continue with nothing to continue).
					MenuStyles.Look look = FieldHud.TitleLook(label.Row == GlobalScope.ttl.TitleFocus, label.Dim);
					if (look != null && look.Hidden) continue;
					// The title's own dark lettering; a Continue with nothing to continue greyed, as its picture is.
					uint rgba = label.Dim ? 0xA0A0A0FFu : 0x282828FFu;
					if (look != null && ModMenus.StyleRgb(look.Colour) is uint rgb) rgba = rgb << 8 | 0xFF;
					else if (look != null && !string.IsNullOrWhiteSpace(look.Colour) && GlobalScope.TextPaletteColour((int)ModMenus.ColourWord(look.Colour)) is uint word) rgba = word;
					float opacity = look == null ? 1f : (float)Math.Clamp(look.Opacity, 0, 1);
					int size = look != null && int.TryParse(look.Font, out int n) && n >= 6 && n <= 31 ? n : GlobalScope.ttl.TITLE_LABEL_SIZE;
					MenuText lettering = FieldHud.TitleLettering(look);
					// A face of the layout's in place of the title's; its shadows under the words, in the text space's units.
					TrueTypeText.TitleFace = lettering == null || lettering.Families.Count == 0;
					TrueTypeText.Style = lettering;
					string text = lettering != null ? lettering.Cased(label.Text) : label.Text;
					float unit = TextSpaceWidth / GlobalScope.LCD_WIDTH;
					try
					{
						if (lettering?.Shadows != null)
						{
							for (int k = lettering.Shadows.Count - 1; k >= 0; k--)
							{
								MenuText.Shadow shadow = lettering.Shadows[k];
								SetColour(graphics, shadow.Colour, opacity);
								TrueTypeText.Blur = shadow.Blur;
								graphics.DrawString(text, tx + shadow.X * unit, ty + shadow.Y * unit, size);
								TrueTypeText.Blur = 0;
							}
						}
						SetColour(graphics, rgba, opacity);
						graphics.DrawString(text, tx, ty, size);
					}
					finally
					{
						TrueTypeText.Style = null;
						TrueTypeText.Blur = 0;
					}
				}
			}
			finally
			{
				TrueTypeText.TitleFace = false;
				graphics.SetImageScale(1f, 1f);
			}
			// The client's own menu, said once where a new player looks first.
			graphics.SetColor(90, 90, 90, 255);
			graphics.DrawString("Esc / hold Start: settings", 12f, TextSpaceHeight - 22f, RowSize);
			graphics.DrawStringEnd();
		}

		/// <summary>A colour (0xRRGGBBAA) at an opacity for the next strings.</summary>
		private static void SetColour(GlobalScope.Graphics graphics, uint rgba, float opacity)
		{
			graphics.SetColor((int)(rgba >> 24 & 0xFF), (int)(rgba >> 16 & 0xFF), (int)(rgba >> 8 & 0xFF), (int)Math.Round((rgba & 0xFF) * opacity));
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
			if (_batch == null)
			{
				_batch = new SpriteBatch(GraphicsDevice);
			}
			if (_pixel == null || _pixel.IsDisposed)
			{
				_pixel = new Texture2D(GraphicsDevice, 1, 1);
				_pixel.SetData(new[] { Color.White });
			}
		}
	}
}
