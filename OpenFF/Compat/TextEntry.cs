// In-game text entry, used for character naming.
//
// The phone build raised the system keyboard through Guide.BeginShowKeyboardInput.
// MonoGame has KeyboardInput.Show as the nominal replacement, but on DesktopGL its
// PlatformShow throws NotImplementedException - and because Show() sets IsVisible
// before awaiting it, IsVisible is left stuck at true forever. That made Guide look
// permanently visible, which makes Game1 skip all input and draw only a black clear.
//
// So this implements the prompt directly, using the window's TextInput event and the
// game's own font for rendering.

using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace OpenFF.Client
{
	// MonoGame's types by name: the engine's OpenFF.Game, GameTime, Color and vectors sit a namespace up.
	using Game = Microsoft.Xna.Framework.Game;
	using GameTime = Microsoft.Xna.Framework.GameTime;
	using Color = Microsoft.Xna.Framework.Color;

	internal sealed class TextEntry : DrawableGameComponent
	{
		private const int ViewWidth = 800;
		private const int ViewHeight = 480;
		private const int FontSize = 16;

		public static TextEntry Instance { get; private set; }

		private readonly StringBuilder _text = new StringBuilder();

		private SpriteBatch _batch;
		private Texture2D _panel;
		private Action<string> _onComplete;
		private string _title = string.Empty;
		private string _description = string.Empty;
		private int _maxLength = 6;
		private KeyboardState _previousKeys;

		public bool IsActive { get; private set; }

		private TextEntry(Game game)
			: base(game)
		{
			// Above the game, below the screenshot grabber.
			DrawOrder = int.MaxValue - 1;
		}

		public static void Attach(Game game)
		{
			Instance = new TextEntry(game);
			game.Components.Add(Instance);
			game.Window.TextInput += Instance.OnTextInput;
		}

		/// <summary>Opens the prompt. <paramref name="onComplete"/> gets null if cancelled.</summary>
		public void Show(string title, string description, string defaultText, int maxLength,
			Action<string> onComplete)
		{
			if (IsActive)
			{
				// Guide.IsVisible no longer covers text entry, so guard re-entry here.
				Log.Write(LogChannel.Input, "text entry already open; ignoring request");
				return;
			}
			_title = title ?? string.Empty;
			_description = description ?? string.Empty;
			_maxLength = maxLength > 0 ? maxLength : 6;
			_text.Clear();
			if (!string.IsNullOrEmpty(defaultText))
			{
				_text.Append(defaultText.Length > _maxLength
					? defaultText.Substring(0, _maxLength)
					: defaultText);
			}
			_onComplete = onComplete;
			_previousKeys = Keyboard.GetState();
			IsActive = true;
			Log.Write(LogChannel.Input, "text entry opened: \"" + _title + "\" default=\"" + _text + "\"");
		}

		private void Complete(string result)
		{
			if (!IsActive)
			{
				return;
			}
			IsActive = false;
			Action<string> callback = _onComplete;
			_onComplete = null;
			Log.Write(LogChannel.Input, "text entry closed: " + (result == null ? "<cancelled>" : "\"" + result + "\""));
			callback?.Invoke(result);
		}

		/// <summary>A drive typing into the open field (Drive's "type"): appended as the keyboard's characters would be.</summary>
		internal void Inject(string text)
		{
			if (!IsActive) return;
			// Empty: the field cleared (Backspace over everything).
			if (string.IsNullOrEmpty(text)) { _text.Clear(); return; }
			foreach (char c in text)
			{
				if (!char.IsControl(c) && _text.Length < _maxLength) _text.Append(c);
			}
		}

		/// <summary>A drive's Enter (submit: true) or Escape (submit: false) on the open field.</summary>
		internal void Finish(bool submit)
		{
			if (!IsActive) return;
			Complete(submit ? _text.ToString() : null);
		}

		private void OnTextInput(object sender, TextInputEventArgs e)
		{
			if (!IsActive)
			{
				return;
			}
			char c = e.Character;
			if (c == '\b' || c == '\r' || c == '\n' || c == '\t' || c == (char)27)
			{
				// Handled as key presses in Update so they cannot double-fire.
				return;
			}
			if (!char.IsControl(c) && _text.Length < _maxLength)
			{
				_text.Append(c);
			}
		}

		// ---- The on-screen keys, for a pad (and the mouse): rows of characters and a row of actions. ----
		// A keyboard types as before; the grid is steered with the d-pad or stick, A picks a key,
		// B rubs one out, X is a space, Start is Done. The mouse clicks a key.
		private static readonly string[] KeyRows =
		{
			"ABCDEFGHIJKLM",
			"NOPQRSTUVWXYZ",
			"abcdefghijklm",
			"nopqrstuvwxyz",
			"0123456789-'.",
		};
		private static readonly string[] Actions = { "Space", "Delete", "Done", "Cancel" };
		private int _row, _col;          // the highlighted key; row == KeyRows.Length is the action row
		private int _previousPad;
		private bool _mouseWasDown;
		private const int GridX = 130, GridY = 232, CellW = 40, CellH = 30, ActionW = 130;

		private bool ShowKeys => DesktopInput.PadConnected || _keysUsed || Options.Get("onscreen-keys") != null;
		private bool _keysUsed;

		private Rectangle CellRect(int row, int col)
		{
			if (row < KeyRows.Length) return new Rectangle(GridX + col * CellW, GridY + row * CellH, CellW, CellH);
			return new Rectangle(GridX + col * ActionW, GridY + KeyRows.Length * CellH + 6, ActionW, CellH);
		}

		private int ColumnsIn(int row) => row < KeyRows.Length ? KeyRows[row].Length : Actions.Length;

		private void Pick(int row, int col)
		{
			if (row < KeyRows.Length)
			{
				if (_text.Length < _maxLength) _text.Append(KeyRows[row][col]);
				return;
			}
			switch (col)
			{
				case 0: if (_text.Length < _maxLength) _text.Append(' '); break;
				case 1: if (_text.Length > 0) _text.Length--; break;
				case 2: Complete(_text.ToString()); break;
				case 3: Complete(null); break;
			}
		}

		public override void Update(GameTime gameTime)
		{
			if (!IsActive)
			{
				return;
			}

			KeyboardState keys = Keyboard.GetState();
			if (WasPressed(keys, Keys.Enter))
			{
				Complete(_text.ToString());
			}
			else if (WasPressed(keys, Keys.Escape))
			{
				Complete(null);
			}
			else if (WasPressed(keys, Keys.Back) && _text.Length > 0)
			{
				_text.Length--;
			}
			_previousKeys = keys;
			if (!IsActive) return;

			// The pad on the grid: edges of its bits (the keyboard is not read here - it types).
			int pad = DesktopInput.PadOnlyBits(), edge = pad & ~_previousPad;
			_previousPad = pad;
			if (edge != 0) _keysUsed = true;
			if ((edge & 64) != 0) { _row = (_row + KeyRows.Length) % (KeyRows.Length + 1); _col = Math.Min(_col, ColumnsIn(_row) - 1); }
			if ((edge & 128) != 0) { _row = (_row + 1) % (KeyRows.Length + 1); _col = Math.Min(_col, ColumnsIn(_row) - 1); }
			if ((edge & 32) != 0) _col = (_col + ColumnsIn(_row) - 1) % ColumnsIn(_row);
			if ((edge & 16) != 0) _col = (_col + 1) % ColumnsIn(_row);
			if ((edge & 1) != 0) Pick(_row, _col);
			else if ((edge & 2) != 0 && _text.Length > 0) _text.Length--;
			else if ((edge & 1024) != 0 && _text.Length < _maxLength) _text.Append(' ');
			else if ((edge & 8) != 0) Complete(_text.ToString());
			if (!IsActive) return;

			// The mouse on the grid.
			bool down = DesktopInput.MouseInView(out int mx, out int my);
			if (down && !_mouseWasDown && ShowKeys)
			{
				for (int r = 0; r <= KeyRows.Length; r++)
					for (int c = 0; c < ColumnsIn(r); c++)
						if (CellRect(r, c).Contains(mx, my)) { _row = r; _col = c; _keysUsed = true; Pick(r, c); }
			}
			_mouseWasDown = down;
		}

		private bool WasPressed(KeyboardState now, Keys key)
		{
			return now.IsKeyDown(key) && !_previousKeys.IsKeyDown(key);
		}

		public override void Draw(GameTime gameTime)
		{
			if (!IsActive)
			{
				return;
			}

			EnsureResources();

			int backWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
			int backHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;
			float scaleX = backWidth / (float)ViewWidth;
			float scaleY = backHeight / (float)ViewHeight;

			bool keysShown = ShowKeys;
			// With the on-screen keys the panel reaches down to hold them; the field sits above.
			Rectangle view = keysShown ? new Rectangle(110, 60, 580, 380) : new Rectangle(150, 150, 500, 180);
			Rectangle panel = new Rectangle(
				(int)(view.X * scaleX), (int)(view.Y * scaleY),
				(int)(view.Width * scaleX), (int)(view.Height * scaleY));
			int textY = keysShown ? 78 : 168;   // where the title starts, in view space

			// The scene keeps rendering behind this; dim it slightly so the field reads
			// as focused without hiding where you are.
			_batch.Begin();
			_batch.Draw(_panel, new Rectangle(0, 0, backWidth, backHeight), new Color(0, 0, 0, 110));
			_batch.Draw(_panel, panel, new Color(24, 40, 96, 235));
			_batch.Draw(_panel, new Rectangle(panel.X, panel.Y, panel.Width, 2), Color.White);
			_batch.Draw(_panel, new Rectangle(panel.X, panel.Bottom - 2, panel.Width, 2), Color.White);
			_batch.Draw(_panel, new Rectangle(panel.X, panel.Y, 2, panel.Height), Color.White);
			_batch.Draw(_panel, new Rectangle(panel.Right - 2, panel.Y, 2, panel.Height), Color.White);

			// Underline for the field itself, so it looks like somewhere you type.
			_batch.Draw(_panel, new Rectangle(
				(int)((view.X + 20) * scaleX), (int)((textY + 90) * scaleY),
				(int)(200 * scaleX), (int)(2 * scaleY)), new Color(200, 200, 200, 255));
			if (keysShown)
			{
				// The keys: a cell each, the highlighted one lit.
				for (int r = 0; r <= KeyRows.Length; r++)
				{
					for (int c = 0; c < ColumnsIn(r); c++)
					{
						Rectangle cell = CellRect(r, c);
						Rectangle at = new Rectangle((int)((cell.X + 2) * scaleX), (int)((cell.Y + 2) * scaleY), (int)((cell.Width - 4) * scaleX), (int)((cell.Height - 4) * scaleY));
						bool lit = r == _row && c == _col;
						_batch.Draw(_panel, at, lit ? new Color(230, 200, 90, 255) : new Color(40, 60, 130, 255));
					}
				}
			}
			_batch.End();

			// Draw the text with the game's own font so it matches everything else.
			GlobalScope.Graphics graphics = GlobalScope.m_Graphics;
			if (graphics == null)
			{
				return;
			}
			graphics.SetImageOrigin(0f, 0f);
			graphics.SetImageRotation(0f);
			graphics.SetImageScale(1f, 1f);
			graphics.DrawStringStart();
			graphics.SetColor(255, 255, 255, 255);
			float tx = view.X + 20;
			graphics.DrawString(_title, tx, textY, FontSize);
			graphics.DrawString(_description, tx, textY + 28, FontSize);
			graphics.DrawString(_text.ToString() + "_", tx, textY + 68, FontSize);
			graphics.SetColor(190, 190, 190, 255);
			graphics.DrawString(keysShown ? "Type, or pick a key: A picks, B deletes, Start is Done" : "Type a name, then press Enter", tx, textY + 116, FontSize);
			if (keysShown)
			{
				for (int r = 0; r <= KeyRows.Length; r++)
				{
					for (int c = 0; c < ColumnsIn(r); c++)
					{
						Rectangle cell = CellRect(r, c);
						bool lit = r == _row && c == _col;
						graphics.SetColor(lit ? (byte)20 : (byte)255, lit ? (byte)20 : (byte)255, lit ? (byte)20 : (byte)255, 255);
						string label = r < KeyRows.Length ? KeyRows[r][c].ToString() : Actions[c];
						float w = TrueTypeText.Enabled ? TrueTypeText.Width(label, FontSize) : label.Length * 9f;
						graphics.DrawString(label, cell.X + (cell.Width - w) / 2f, cell.Y + 6f, FontSize);
					}
				}
			}
			graphics.DrawStringEnd();
		}

		private void EnsureResources()
		{
			if (_batch == null)
			{
				_batch = new SpriteBatch(GraphicsDevice);
			}
			if (_panel == null || _panel.IsDisposed)
			{
				_panel = new Texture2D(GraphicsDevice, 1, 1);
				_panel.SetData(new[] { Color.White });
			}
		}
	}
}
