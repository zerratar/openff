// Feeds the engine's Game.Input once per frame from the desktop: the pad bits the game
// itself reads, the mouse as the pointer in screen units, the keyboard by key names.
// When a mod has set Game.Input.Capture the game stops receiving input (DesktopInput
// checks the same flag), and the mod has the player.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FF3
{
	internal static class EngineInput
	{
		private static Game _game;
		private static int _lastWheel;
		private static readonly List<string> _held = new List<string>();

		public static void Attach(Game game)
		{
			_game = game;
		}

		public static bool Captured => OpenFF.Game.Input.Capture;

		public static void Update()
		{
			OpenFF.InputState input = OpenFF.Game.Input;
			bool active = _game != null && _game.IsActive && !(TextEntry.Instance != null && TextEntry.Instance.IsActive) && !ModListScreen.IsOpen;
			if (!active && DesktopInput.Injected.Count == 0)
			{
				input.SetPad(0);
				input.SetPointer(input.PointerX, input.PointerY, false, 0);
				input.SetKeys(Array.Empty<string>());
				return;
			}
			input.SetPad(DesktopInput.RawPadBits());
			if (!active)
			{
				// A scripted drive without focus: its keys, nothing else.
				_held.Clear();
				foreach (Keys key in DesktopInput.Injected) _held.Add(key.ToString());
				input.SetKeys(_held);
				return;
			}

			MouseState mouse = Mouse.GetState();
			Rectangle client = _game.Window.ClientBounds;
			float x = client.Width > 0 ? mouse.X * OpenFF.InputState.ScreenWidth / client.Width : mouse.X;
			float y = client.Height > 0 ? mouse.Y * OpenFF.InputState.ScreenHeight / client.Height : mouse.Y;
			int wheel = mouse.ScrollWheelValue;
			input.SetPointer(x, y, mouse.LeftButton == ButtonState.Pressed, (wheel - _lastWheel) / 120);
			_lastWheel = wheel;

			_held.Clear();
			foreach (Keys key in Keyboard.GetState().GetPressedKeys())
			{
				_held.Add(key.ToString());
			}
			foreach (Keys key in DesktopInput.Injected)
			{
				if (!_held.Contains(key.ToString())) _held.Add(key.ToString());
			}
			input.SetKeys(_held);
		}
	}
}
