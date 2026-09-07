// Input as a mod sees it: the pad, the pointer, the keyboard - and the switch that
// takes them away from the game.
//
// The host fills this in once per frame, before the engine's Update, from whatever the
// player has (keyboard and mouse today; a controller reads as the pad). A mod that runs
// its own game - a shooter, a real-time fight - sets Capture: the legacy game then sees
// no input at all (no walking, no menu, no taps), and the mod reads everything here.
// Screen positions are in the engine's screen units: 800 wide, 480 tall, whatever the
// window is (the same space Game.Draw draws in).

using System;
using System.Collections.Generic;

namespace OpenFF
{
	[Flags]
	public enum Pad
	{
		None = 0,
		A = 1,
		B = 2,
		Select = 4,
		Start = 8,
		Right = 16,
		Left = 32,
		Up = 64,
		Down = 128,
		R = 256,
		L = 512,
		X = 1024,
		Y = 2048,
	}

	public sealed class InputState
	{
		/// <summary>The screen's width in the engine's units.</summary>
		public const float ScreenWidth = 800f;
		/// <summary>The screen's height in the engine's units.</summary>
		public const float ScreenHeight = 480f;

		private Pad _now, _before;
		private readonly HashSet<string> _keysNow = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		private readonly HashSet<string> _keysBefore = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// While true the game itself receives no input; the mod that set it has the player.
		/// Freeze the hero as well if the field should stand still under a mod's own game.
		/// </summary>
		public bool Capture { get; set; }

		/// <summary>Every pad button down this frame.</summary>
		public Pad Held => _now;
		/// <summary>Whether a button is down.</summary>
		public bool IsHeld(Pad button) => (_now & button) != 0;
		/// <summary>Whether a button went down this frame.</summary>
		public bool Pressed(Pad button) => (_now & button) != 0 && (_before & button) == 0;
		/// <summary>Whether a button came up this frame.</summary>
		public bool Released(Pad button) => (_now & button) == 0 && (_before & button) != 0;

		/// <summary>The direction the pad gives, as a unit vector on the ground plane (x right, y up on screen).</summary>
		public Vector3 Direction
		{
			get
			{
				float x = (IsHeld(Pad.Right) ? 1 : 0) - (IsHeld(Pad.Left) ? 1 : 0);
				float y = (IsHeld(Pad.Up) ? 1 : 0) - (IsHeld(Pad.Down) ? 1 : 0);
				float len = (float)Math.Sqrt(x * x + y * y);
				return len > 0 ? new Vector3(x / len, y / len, 0) : Vector3.Zero;
			}
		}

		/// <summary>The pointer (mouse or finger) in screen units.</summary>
		public float PointerX { get; private set; }
		/// <summary>The pointer's y in screen units.</summary>
		public float PointerY { get; private set; }
		/// <summary>Whether the pointer (mouse button, finger) is down.</summary>
		public bool PointerDown { get; private set; }
		/// <summary>Whether the pointer went down this frame.</summary>
		public bool PointerPressed { get; private set; }
		/// <summary>Whether the pointer came up this frame.</summary>
		public bool PointerReleased { get; private set; }
		/// <summary>Mouse wheel movement this frame, in notches.</summary>
		public int Wheel { get; private set; }

		/// <summary>A keyboard key by its name (Space, Enter, F, D1, Left...), for mods that want the keyboard itself.</summary>
		public bool KeyHeld(string key) => _keysNow.Contains(key);
		/// <summary>Whether a key went down this frame.</summary>
		public bool KeyPressed(string key) => _keysNow.Contains(key) && !_keysBefore.Contains(key);
		/// <summary>Every key down this frame, by name.</summary>
		public IEnumerable<string> KeysHeld => _keysNow;

		/// <summary>Host entry: the frame's pad bits (the same layout the game's pad uses).</summary>
		public void SetPad(int bits)
		{
			_before = _now;
			_now = (Pad)bits;
		}

		/// <summary>Host entry: the pointer in screen units and whether it is down.</summary>
		public void SetPointer(float x, float y, bool down, int wheel)
		{
			PointerPressed = down && !PointerDown;
			PointerReleased = !down && PointerDown;
			PointerDown = down;
			PointerX = x;
			PointerY = y;
			Wheel = wheel;
		}

		/// <summary>Host entry: the keys held this frame, by name.</summary>
		public void SetKeys(IEnumerable<string> held)
		{
			_keysBefore.Clear();
			foreach (string k in _keysNow) _keysBefore.Add(k);
			_keysNow.Clear();
			foreach (string k in held) _keysNow.Add(k);
		}
	}

	public static partial class Game
	{
		/// <summary>The pad, pointer and keyboard as the mod sees them this frame; Capture takes them away from the game.</summary>
		public static InputState Input { get; } = new InputState();
	}
}
