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

			Rectangle panel = new Rectangle(
				(int)(150 * scaleX), (int)(150 * scaleY),
				(int)(500 * scaleX), (int)(180 * scaleY));

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
				(int)(168 * scaleX), (int)(258 * scaleY),
				(int)(200 * scaleX), (int)(2 * scaleY)), new Color(200, 200, 200, 255));
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
			graphics.DrawString(_title, 170f, 168f, FontSize);
			graphics.DrawString(_description, 170f, 196f, FontSize);
			graphics.DrawString(_text.ToString() + "_", 170f, 236f, FontSize);
			graphics.SetColor(190, 190, 190, 255);
			graphics.DrawString("Type a name, then press Enter", 170f, 284f, FontSize);
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
