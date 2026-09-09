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

		// ---- The on-screen keys, for a pad (and the mouse), laid out as a console's: ----
		// four rows of ten - digits, then qwerty - and a row of wide keys: Shift, Space,
		// Backspace, Done. The d-pad or stick moves the lit key, A picks it, B rubs one out,
		// X is a space, Y turns Shift, Start is Done. The mouse clicks a key. A keyboard types
		// as before. Shown when a pad is connected (or --onscreen-keys).
		private static readonly string[] KeyRows =
		{
			"1234567890",
			"qwertyuiop",
			"asdfghjkl'",
			"zxcvbnm-.?",
		};
		private static readonly (string Label, int Width)[] WideKeys = { ("Shift", 88), ("Space", 176), ("Backspace", 88), ("Done", 88) };
		private int _row, _col;          // the lit key; row == KeyRows.Length is the wide row
		private bool _shift;
		private int _previousPad;
		private bool _mouseWasDown;
		private bool _keysUsed;
		private const int GridX = 180, GridY = 176, CellW = 44, CellH = 36, Gap = 4;

		private bool ShowKeys => DesktopInput.PadConnected || _keysUsed || Options.Get("onscreen-keys") != null;

		private Rectangle CellRect(int row, int col)
		{
			if (row < KeyRows.Length) return new Rectangle(GridX + col * CellW, GridY + row * CellH, CellW - Gap, CellH - Gap);
			int x = GridX;
			for (int i = 0; i < col; i++) x += WideKeys[i].Width;
			return new Rectangle(x, GridY + KeyRows.Length * CellH + 4, WideKeys[col].Width - Gap, CellH - Gap);
		}

		private int ColumnsIn(int row) => row < KeyRows.Length ? KeyRows[row].Length : WideKeys.Length;

		private string KeyLabel(int row, int col)
		{
			if (row >= KeyRows.Length) return WideKeys[col].Label;
			char c = KeyRows[row][col];
			return (_shift ? char.ToUpperInvariant(c) : c).ToString();
		}

		private void Pick(int row, int col)
		{
			_keysUsed = true;
			if (row < KeyRows.Length)
			{
				char c = KeyRows[row][col];
				if (_shift) c = char.ToUpperInvariant(c);
				if (_text.Length < _maxLength) _text.Append(c);
				return;
			}
			switch (col)
			{
				case 0: _shift = !_shift; break;
				case 1: if (_text.Length < _maxLength) _text.Append(' '); break;
				case 2: if (_text.Length > 0) _text.Length--; break;
				case 3: Complete(_text.ToString()); break;
			}
		}

		/// <summary>The lit key moved to the column under the same x in another row (a wide key takes the digit columns it spans).</summary>
		private void MoveRow(int to)
		{
			Rectangle from = CellRect(_row, _col);
			int cx = from.X + from.Width / 2;
			_row = to;
			int best = 0, bestD = int.MaxValue;
			for (int c = 0; c < ColumnsIn(_row); c++)
			{
				Rectangle r = CellRect(_row, c);
				int d = Math.Abs(r.X + r.Width / 2 - cx);
				if (d < bestD) { bestD = d; best = c; }
			}
			_col = best;
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
			if ((edge & 64) != 0) MoveRow((_row + KeyRows.Length) % (KeyRows.Length + 1));
			if ((edge & 128) != 0) MoveRow((_row + 1) % (KeyRows.Length + 1));
			if ((edge & 32) != 0) _col = (_col + ColumnsIn(_row) - 1) % ColumnsIn(_row);
			if ((edge & 16) != 0) _col = (_col + 1) % ColumnsIn(_row);
			if ((edge & 1) != 0) Pick(_row, _col);
			else if ((edge & 2) != 0 && _text.Length > 0) _text.Length--;
			else if ((edge & 1024) != 0 && _text.Length < _maxLength) _text.Append(' ');
			else if ((edge & 2048) != 0) _shift = !_shift;
			else if ((edge & 8) != 0) Complete(_text.ToString());
			if (!IsActive) return;

			// The mouse on the grid.
			bool down = DesktopInput.MouseInView(out int mx, out int my);
			if (down && !_mouseWasDown && ShowKeys)
			{
				for (int r = 0; r <= KeyRows.Length; r++)
					for (int c = 0; c < ColumnsIn(r); c++)
						if (CellRect(r, c).Contains(mx, my)) { _row = r; _col = c; Pick(r, c); }
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
			Ui.Ensure(GraphicsDevice);
			Viewport view = GraphicsDevice.Viewport;
			bool keysShown = ShowKeys;
			const int titleSize = 14, fieldSize = 16, keySize = 11, hintSize = 9;

			// With the keys the panel reaches down to hold them; without, a small field.
			Rectangle panel = keysShown ? new Rectangle(150, 56, 500, 372) : new Rectangle(150, 150, 500, 180);
			int left = panel.X + 24;
			int titleY = panel.Y + 14;
			Rectangle field = new Rectangle(left, titleY + 34, panel.Width - 48, 40);

			_batch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
			Ui.PanelPlate(_batch, panel, view);
			// The field: a darker well with a light rule under the text.
			Rectangle well = Ui.Scale(field, view);
			Ui.RoundPlate(_batch, well, new Color(8, 16, 44, 255), view);
			Ui.Fill(_batch, new Rectangle(well.X + 8, well.Bottom - 2, well.Width - 16, 1), Ui.PanelEdge);
			if (keysShown)
			{
				for (int r = 0; r <= KeyRows.Length; r++)
					for (int c = 0; c < ColumnsIn(r); c++)
						Ui.Key(_batch, CellRect(r, c), r == _row && c == _col, view);
				// The hints' discs along the bottom.
				float hx = left, hy = panel.Bottom - 22;
				foreach ((Ui.PadButton button, string word) in Hints())
				{
					Ui.HintShape(_batch, button, hx, hy, 18, view);
					hx += HintAdvance(word, hintSize);
				}
			}
			_batch.End();

			GlobalScope.Graphics graphics = GlobalScope.m_Graphics;
			if (graphics == null)
			{
				return;
			}
			graphics.SetImageOrigin(0f, 0f);
			graphics.SetImageRotation(0f);
			graphics.SetImageScale(1f, 1f);
			graphics.DrawStringStart();
			Ui.Left(graphics, _title, left, titleY, Ui.LineHeight(titleSize), titleSize, Ui.Text);
			if (!string.IsNullOrEmpty(_description)) Ui.Left(graphics, _description, left + Ui.Width(graphics, _title, titleSize) + 16, titleY, Ui.LineHeight(titleSize), 10, Ui.Muted);
			// The name so far, and a caret; the count of letters left at the right.
			Ui.Left(graphics, _text.ToString() + (Environment.TickCount / 500 % 2 == 0 ? "|" : " "), field.X + 12, field.Y, field.Height, fieldSize, Ui.Text);
			string left_ = (_maxLength - _text.Length).ToString();
			Ui.Left(graphics, left_, field.Right - 12 - Ui.Width(graphics, left_, 10), field.Y, field.Height, 10, Ui.Muted);
			if (keysShown)
			{
				for (int r = 0; r <= KeyRows.Length; r++)
				{
					for (int c = 0; c < ColumnsIn(r); c++)
					{
						bool lit = r == _row && c == _col;
						bool wide = r >= KeyRows.Length;
						Ui.Centred(graphics, KeyLabel(r, c), CellRect(r, c), wide ? 10 : keySize, lit ? Ui.TextOnLit : (wide && c == 0 && _shift ? Ui.Accent : Ui.Text));
					}
				}
				float hx = left, hy = panel.Bottom - 22;
				foreach ((Ui.PadButton button, string word) in Hints())
				{
					Ui.HintText(graphics, button, word, hx, hy, 18, hintSize);
					hx += HintAdvance(word, hintSize);
				}
			}
			else
			{
				Ui.Left(graphics, "Type a name, then press Enter", left, field.Bottom + 16, Ui.LineHeight(10), 10, Ui.Muted);
			}
			graphics.DrawStringEnd();
		}

		private static (Ui.PadButton, string)[] Hints() => new[]
		{
			(Ui.PadButton.A, "Select"), (Ui.PadButton.B, "Backspace"), (Ui.PadButton.X, "Space"), (Ui.PadButton.Y, "Shift"), (Ui.PadButton.Start, "Done")
		};

		private float HintAdvance(string word, int size) => Ui.HintWidth(GlobalScope.m_Graphics, word, size, 18) + 22;

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