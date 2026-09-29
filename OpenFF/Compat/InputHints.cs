// The input in hand, for the layouts' bindings: the "input" root the menus and the field's HUD reach.
//
// A button hint in a layout says it the player's way - a PlayStation pad's marks, an Xbox pad's letters,
// or the keyboard's keys - as the client's own screens do (Ui.PlayStationPad, Ui.PadConnected, Ui.KeyName):
//
//   <frame bind-class="ps: input.device == 'playstation'; xbox: input.device == 'xbox'; keys: !input.pad">
//     <frame bind-text="{input.back}"> ...            <!-- Esc, B, or the circle -->
//
// Each of the game's buttons (ok, back, x, y, l, r, menu) has its label (input.back: "Esc", "B", "○") and the
// button it is on (input.backButton: the pad map's name - cross, circle, square, triangle, l1... - or the
// keyboard's key, lower case), so a sheet can colour a PlayStation mark by which one it is. Read at most every
// half second: a pad plugged in or taken out while a menu is up shows within that.

using System;

namespace OpenFF.Client
{
	internal sealed class InputHints
	{
		/// <summary>keyboard, playstation or xbox.</summary>
		public string Device { get; private set; } = "keyboard";
		/// <summary>Whether a pad is connected (the hints show its buttons; with none, the keyboard's keys).</summary>
		public bool Pad => Device != "keyboard";

		public string Ok { get; private set; }
		public string Back { get; private set; }
		public string X { get; private set; }
		public string Y { get; private set; }
		public string L { get; private set; }
		public string R { get; private set; }
		public string Menu { get; private set; }

		public string OkButton { get; private set; }
		public string BackButton { get; private set; }
		public string XButton { get; private set; }
		public string YButton { get; private set; }
		public string LButton { get; private set; }
		public string RButton { get; private set; }
		public string MenuButton { get; private set; }

		private static readonly InputHints _current = new InputHints();
		private static long _readAt;
		private static bool _read;

		/// <summary>Counts up each time what the hints show changes (another device, another pad map): what binds them lays out again.</summary>
		public static int Version { get; private set; }

		/// <summary>The hints as they stand, read again when half a second has gone by.</summary>
		public static InputHints Current
		{
			get
			{
				long now = Environment.TickCount64;
				if (!_read || now - _readAt >= 500) { _read = true; _readAt = now; _current.Read(); }
				return _current;
			}
		}

		private void Read()
		{
			string device = Ui.PadConnected ? (Ui.PlayStationPad ? "playstation" : "xbox") : "keyboard";
			DisplaySettings.PadMap map = DisplaySettings.Current.Pad ?? new DisplaySettings.PadMap();
			string was = Device + Ok + Back + X + Y + L + R + Menu + OkButton + BackButton;
			Device = device;
			(Ok, OkButton) = Hint(Ui.PadButton.A, map.A);
			(Back, BackButton) = Hint(Ui.PadButton.B, map.B);
			(X, XButton) = Hint(Ui.PadButton.X, map.X);
			(Y, YButton) = Hint(Ui.PadButton.Y, map.Y);
			(L, LButton) = Hint(Ui.PadButton.L, map.L);
			(R, RButton) = Hint(Ui.PadButton.R, map.R);
			(Menu, MenuButton) = Hint(Ui.PadButton.Start, map.Start);
			if (was != Device + Ok + Back + X + Y + L + R + Menu + OkButton + BackButton) Version++;
		}

		/// <summary>A game button's label and the button it is on: the keyboard's key, or the pad's button the map puts it on.</summary>
		private (string Label, string Button) Hint(Ui.PadButton button, string mapped)
		{
			if (Device == "keyboard") { string key = Ui.KeyName(button); return (key, key.ToLowerInvariant()); }
			string on = string.IsNullOrWhiteSpace(mapped) ? "none" : mapped.Trim().ToLowerInvariant();
			bool ps = Device == "playstation";
			string label = on switch
			{
				"cross" or "a" => ps ? "×" : "A",
				"circle" or "b" => ps ? "○" : "B",
				"square" or "x" => ps ? "□" : "X",
				"triangle" or "y" => ps ? "△" : "Y",
				"l1" => ps ? "L1" : "LB",
				"r1" => ps ? "R1" : "RB",
				"l2" => ps ? "L2" : "LT",
				"r2" => ps ? "R2" : "RT",
				"l3" => ps ? "L3" : "LS",
				"r3" => ps ? "R3" : "RS",
				"options" => ps ? "Options" : "Menu",
				"share" => ps ? "Share" : "View",
				"touchpad" => "Touchpad",
				_ => "",
			};
			// The name the pad map uses, whichever pad: a lettered pad's A is the map's cross.
			string name = on switch { "a" => "cross", "b" => "circle", "x" => "square", "y" => "triangle", _ => on };
			return (label, name);
		}
	}
}
