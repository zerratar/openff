// The in-game mod list, on the title screen where the phone's network entry was.
//
// The title's fourth command (network / achievements on the phone) opens this instead:
// every mod in mods/ beside the executable, in load order, with its enabled flag, what
// it brings (files, code), and why it is skipped when it is. Up/Down select, Space or
// Enter toggle, Shift+Up/Down move a mod in the order, Esc (or the Back button) closes
// and writes mods/loadorder.json. The mouse does the same: a row toggles, the arrows
// beside it move it. The content chain is built once at start, so the list says that
// changes apply at the next start.
//
// Drawn with the game's own font and a SpriteBatch panel, like the text entry, because
// there is no picture for any of this in either game's banks. The "Mods" label on the
// title is drawn here too, at the position the title gives (ShowTitleLabel), since
// Steam's title bank has no picture in that cell.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
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
		private const int TitleSize = 14;
		private const int RowSize = 10;
		private const float RowHeight = 20f;
		private const float ListTop = 92f;
		private const float ListLeft = 60f;
		private const float ListWidth = 680f;
		private const int RowsPerPage = 13;
		private const float FooterTop = 386f;
		private const float BackTop = 406f;

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
		private bool _labelShown;
		private int _labelX;
		private int _labelY;

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

		/// <summary>The title shows its commands: draw "Mods" at this LCD position until HideTitleLabel.</summary>
		public static void ShowTitleLabel(int x, int y)
		{
			if (Instance == null) return;
			Instance._labelShown = true;
			Instance._labelX = x;
			Instance._labelY = y;
		}

		public static void HideTitleLabel()
		{
			if (Instance != null) Instance._labelShown = false;
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
					Log.Write(LogChannel.General, "mod list: loadorder.json written - " + string.Join(", ", _mods.Select(m => m.Key + (m.Enabled ? "" : " (off)"))) + "; applies at the next start");
				}
				catch (Exception ex)
				{
					Log.Write(LogChannel.General, "mod list: loadorder.json not written: " + ex.Message);
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

		/// <summary>A click in viewport pixels: on a row toggles it, on its arrows moves it, on Back closes.</summary>
		private void Click(int px, int py)
		{
			Viewport view = GraphicsDevice.Viewport;
			float tx = px / (float)view.Width * TextSpaceWidth;
			float ty = py / (float)view.Height * TextSpaceHeight;
			if (ty >= BackTop && ty <= BackTop + 26 && tx >= TextSpaceWidth / 2 - 60 && tx <= TextSpaceWidth / 2 + 60)
			{
				Close();
				return;
			}
			for (int row = 0; row < RowsPerPage; row++)
			{
				int index = _scroll + row;
				if (index >= _mods.Count) break;
				float top = ListTop + row * RowHeight;
				if (ty < top || ty >= top + RowHeight || tx < ListLeft || tx > ListLeft + ListWidth) continue;
				_selected = index;
				if (tx >= ListLeft + ListWidth - 60 && tx < ListLeft + ListWidth - 30)
				{
					Move(-1);
				}
				else if (tx >= ListLeft + ListWidth - 30)
				{
					Move(1);
				}
				else
				{
					Toggle(index);
				}
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
				if (_labelShown && !TextEntry.Instance?.IsActive == true)
				{
					DrawTitleLabel(graphics);
				}
				return;
			}
			EnsureResources();
			Viewport view = GraphicsDevice.Viewport;
			float sx = view.Width / TextSpaceWidth;
			float sy = view.Height / TextSpaceHeight;

			_batch.Begin();
			_batch.Draw(_pixel, new Rectangle(0, 0, view.Width, view.Height), new Color(0, 0, 0, 200));
			Rectangle panel = new Rectangle((int)(40 * sx), (int)(40 * sy), (int)(720 * sx), (int)(400 * sy));
			_batch.Draw(_pixel, panel, new Color(24, 40, 96, 240));
			Outline(panel, Color.White, 2);
			for (int row = 0; row < RowsPerPage; row++)
			{
				int index = _scroll + row;
				if (index >= _mods.Count) break;
				Rectangle r = new Rectangle((int)(ListLeft * sx), (int)((ListTop + row * RowHeight) * sy), (int)(ListWidth * sx), (int)(RowHeight * sy));
				if (index == _selected)
				{
					// SpriteBatch wants premultiplied colours; a plain alpha here reads as solid white.
					_batch.Draw(_pixel, r, Color.White * 0.16f);
				}
			}
			// The Back button's plate.
			Rectangle back = new Rectangle((int)((TextSpaceWidth / 2 - 60) * sx), (int)(BackTop * sy), (int)(120 * sx), (int)(26 * sy));
			_batch.Draw(_pixel, back, Color.White * 0.12f);
			Outline(back, new Color(200, 200, 200), 1);
			_batch.End();

			graphics.SetImageOrigin(0f, 0f);
			graphics.SetImageRotation(0f);
			graphics.SetImageScale(1f, 1f);
			graphics.DrawStringStart();
			graphics.SetColor(255, 255, 255, 255);
			graphics.DrawString("Mods", 60f, 48f, TitleSize);
			graphics.SetColor(190, 190, 190, 255);
			graphics.DrawString(_mods.Count == 0
				? "No mods yet. A mod is a folder in the mods folder beside the game."
				: _mods.Count + " mod(s) in the mods folder beside the game" + (_conflicts > 0 ? "   " + _conflicts + " shared file(s), the earlier wins" : ""), 60f, 72f, RowSize);
			for (int row = 0; row < RowsPerPage; row++)
			{
				int index = _scroll + row;
				if (index >= _mods.Count) break;
				InstalledMod mod = _mods[index];
				float y = ListTop + row * RowHeight + 3;
				bool on = mod.Enabled;
				if (index == _selected) graphics.SetColor(255, 255, 160, 255);
				else if (!on) graphics.SetColor(140, 140, 140, 255);
				else graphics.SetColor(255, 255, 255, 255);
				graphics.DrawString((on ? "[x] " : "[ ] ") + (index + 1) + ". " + Fit(mod.DisplayName, 32) + (string.IsNullOrWhiteSpace(mod.Manifest.Version) ? "" : "  " + mod.Manifest.Version), ListLeft + 6, y, RowSize);
				string what = Fit(Describe(mod), 44);
				graphics.SetColor(170, 170, 170, 255);
				graphics.DrawString(what, ListLeft + 330, y, RowSize);
				graphics.SetColor(200, 200, 200, 255);
				graphics.DrawString("^", ListLeft + ListWidth - 52, y, RowSize);
				graphics.DrawString("v", ListLeft + ListWidth - 22, y, RowSize);
			}
			if (_mods.Count > RowsPerPage)
			{
				graphics.SetColor(170, 170, 170, 255);
				graphics.DrawString((_scroll + 1) + "-" + Math.Min(_mods.Count, _scroll + RowsPerPage) + " of " + _mods.Count, ListLeft + ListWidth - 120, ListTop - 18, RowSize);
			}
			graphics.SetColor(190, 190, 190, 255);
			graphics.DrawString("Up/Down select   Space toggle   Shift+Up/Down move   Esc back", 60f, FooterTop - 18, RowSize);
			graphics.DrawString("Changes apply at the next start.", 60f, FooterTop, RowSize);
			graphics.SetColor(255, 255, 255, 255);
			graphics.DrawString("Back", TextSpaceWidth / 2 - 14, BackTop + 5, RowSize);
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

		/// <summary>"Mods" where the title's fourth command sits, in the title's own LCD units.</summary>
		private void DrawTitleLabel(GlobalScope.Graphics graphics)
		{
			// LCD units to the game's 800x480 text space, the way the ortho projection maps them.
			float ox = (480 - GlobalScope.LCD_WIDTH) / 2f;
			float oy = (320 - GlobalScope.LCD_HEIGHT) / 2f;
			float tx = (_labelX - ox) / GlobalScope.LCD_WIDTH * TextSpaceWidth;
			float ty = (_labelY - oy) / GlobalScope.LCD_HEIGHT * TextSpaceHeight;
			graphics.SetImageOrigin(0f, 0f);
			graphics.SetImageRotation(0f);
			graphics.SetImageScale(1f, 1f);
			graphics.DrawStringStart();
			graphics.SetColor(40, 40, 40, 255);
			graphics.DrawString("MODS", tx, ty - 2, TitleSize);
			graphics.DrawStringEnd();
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
