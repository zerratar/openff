// Quality of life, as the Pixel Remaster's boosters (and the Onion Proxy "PR inspired QOL" mod for
// the Steam build): the game's speed, random encounters on or off, EXP and job EXP multipliers,
// saving anywhere, and no adjustment period after a job change. Player options, not a mod - they
// are kept in settings.json ("qol") and work alongside any mods. The Esc menu's Quality of life page
// sets them; F8 cycles the speed and F11 turns random encounters on and off while playing (a pad
// button can be bound to either on the Pad buttons page), and a small indicator in the corner says
// what is on.
//
// The game's hooks: map.CEnCountManager.checkEncount (random battles; scripted ones stay),
// btl.BattleCharacterManager.getTrueExp (battle EXP), pl.PlayerJobManager.addJobSkillExp (job
// EXP), wmenu.CWMenuMain (Save on any map), pl.Player's penalty time (the job change's
// adjustment), DesktopInput.BeginFrame (the speed: that many of the game's steps a frame).
//
// A drive (--drive) runs with the defaults, so a test plays the same whatever the player has set;
// its qol verb sets them for the run.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace OpenFF.Client
{
	// MonoGame's types by name: the engine's OpenFF.Game, GameTime, Color and vectors sit a namespace up.
	using Game = Microsoft.Xna.Framework.Game;
	using GameTime = Microsoft.Xna.Framework.GameTime;
	using Color = Microsoft.Xna.Framework.Color;

	internal static class Qol
	{
		/// <summary>The speeds F8 and the menu go through.</summary>
		public static readonly int[] Speeds = { 1, 2, 3, 4 };

		/// <summary>The EXP and job EXP multipliers the menu goes through (the file may say any from 0 to 100).</summary>
		public static readonly double[] Multipliers = { 0, 0.5, 1, 2, 4 };

		public const int MostSpeed = 8;
		public const double MostMultiplier = 100;

		private static DisplaySettings.QolSettings _drive;

		/// <summary>The settings in force: the player's, or a drive's own (the defaults, and what its qol verb set).</summary>
		public static DisplaySettings.QolSettings S => Drive.Active ? (_drive ??= new DisplaySettings.QolSettings()) : (DisplaySettings.Current.Qol ??= new DisplaySettings.QolSettings());

		/// <summary>How many of the game's steps a frame runs: 1 as the game plays.</summary>
		public static int Speed => Math.Clamp(S.Speed, 1, MostSpeed);

		/// <summary>Whether random battles come (scripted ones always do).</summary>
		public static bool Encounters => S.Encounters;

		public static bool SaveAnywhere => S.SaveAnywhere;

		/// <summary>Whether a job change brings the adjustment period (stats lowered for a few battles), as the game has it.</summary>
		public static bool JobAdjustment => S.JobAdjustment;

		/// <summary>A battle's EXP for each hero, multiplied (0 gives none).</summary>
		public static int Exp(int exp) => Scale(exp, S.Exp, 9999999);

		/// <summary>An action's job EXP, multiplied.</summary>
		public static int JobExp(int points) => Scale(points, S.JobExp, 255);

		private static int Scale(int value, double by, int most)
		{
			double m = double.IsFinite(by) ? Math.Clamp(by, 0, MostMultiplier) : 1;
			if (m == 1) return value;
			return (int)Math.Clamp(Math.Round(value * m), 0, most);
		}

		/// <summary>A multiplier as the menu and the indicator write it: x0, x0.5, x2...</summary>
		public static string Times(double m) => "x" + m.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);

		/// <summary>The next of a list's values after the one given (by -1 or +1), the nearest when it is none of them.</summary>
		public static double Next(double[] list, double now, int by)
		{
			int at = 0;
			for (int i = 1; i < list.Length; i++) if (Math.Abs(list[i] - now) < Math.Abs(list[at] - now)) at = i;
			bool exact = Math.Abs(list[at] - now) < 1e-9;
			if (!exact && by > 0 && list[at] < now) at++;
			else if (!exact && by < 0 && list[at] > now) at--;
			else at += by;
			return list[(at % list.Length + list.Length) % list.Length];
		}

		/// <summary>A setting changed in the menu or by a key: written to the file (not a drive's).</summary>
		public static void Changed(string what)
		{
			Log.Write(LogChannel.General, "qol: " + what);
			if (!Drive.Active) DisplaySettings.Current.Save();
		}

		// ---- the keys: F8 the speed, F11 random encounters, or the pad buttons bound to them ----

		private static bool _speedWas, _encountersWas;

		/// <summary>Once a frame, before the game's step: the keys that change a setting while playing.</summary>
		public static void Tick(Game game)
		{
			if (game == null || (!game.IsActive && !Drive.Active) || PauseMenu.IsOpen || EngineInput.Captured || (TextEntry.Instance != null && TextEntry.Instance.IsActive)) { _speedWas = _encountersWas = true; return; }
			KeyboardState keys = Keyboard.GetState();
			DisplaySettings.PadMap map = DisplaySettings.Current.Pad ?? new DisplaySettings.PadMap();
			bool shift = keys.IsKeyDown(Keys.LeftShift) || keys.IsKeyDown(Keys.RightShift);
			bool speed = keys.IsKeyDown(Keys.F8) || DesktopInput.Injected.Contains(Keys.F8) || PadHeld(map.SpeedUp);
			bool encounters = keys.IsKeyDown(Keys.F11) || DesktopInput.Injected.Contains(Keys.F11) || PadHeld(map.Encounters);
			if (speed && !_speedWas)
			{
				S.Speed = (int)Next(Array.ConvertAll(Speeds, x => (double)x), Speed, shift ? -1 : 1);
				Toast("Speed " + Times(S.Speed));
				Changed("speed x" + S.Speed);
			}
			if (encounters && !_encountersWas)
			{
				S.Encounters = !S.Encounters;
				Toast(S.Encounters ? "Encounters on" : "No encounters");
				Changed("random encounters " + (S.Encounters ? "on" : "off"));
			}
			_speedWas = speed;
			_encountersWas = encounters;
		}

		private static bool PadHeld(string button)
		{
			if (string.IsNullOrWhiteSpace(button) || button == "none") return false;
			for (int i = 0; i < 4; i++)
			{
				try
				{
					GamePadState pad = GamePad.GetState((PlayerIndex)i);
					if (pad.IsConnected) return DisplaySettings.Held(pad, button);
				}
				catch (Exception) { }
			}
			return false;
		}

		// ---- what the indicator says ----

		private static string _toast;
		private static double _toastUntil;
		private static readonly System.Diagnostics.Stopwatch _clock = System.Diagnostics.Stopwatch.StartNew();

		/// <summary>A line shown for a moment in the corner whether the indicator is on or not (a key changed something).</summary>
		public static void Toast(string text)
		{
			_toast = text;
			_toastUntil = _clock.Elapsed.TotalSeconds + 2.5;
		}

		/// <summary>Whether the indicator's first line is a moment's note (drawn in the accent colour).</summary>
		public static bool Toasting => _toast != null && _clock.Elapsed.TotalSeconds < _toastUntil;

		/// <summary>The indicator's lines: what differs from the game as it plays (with the indicator on), and a moment's note of a key's change first.</summary>
		public static System.Collections.Generic.List<string> Lines()
		{
			System.Collections.Generic.List<string> lines = new System.Collections.Generic.List<string>();
			DisplaySettings.QolSettings s = S;
			if (s.Indicator)
			{
				if (Speed != 1) lines.Add("Speed " + Times(Speed));
				if (!s.Encounters) lines.Add("No encounters");
				if (Math.Abs(s.Exp - 1) > 1e-9) lines.Add("EXP " + Times(s.Exp));
				if (Math.Abs(s.JobExp - 1) > 1e-9) lines.Add("Job EXP " + Times(s.JobExp));
				if (s.SaveAnywhere) lines.Add("Save anywhere");
				if (!s.JobAdjustment) lines.Add("No job adjustment");
			}
			if (Toasting) { lines.Remove(_toast); lines.Insert(0, _toast); }
			return lines;
		}
	}

	/// <summary>The quality-of-life indicator: a small plate in the screen's corner over the game (not over the Esc menu).</summary>
	internal sealed class QolIndicator : DrawableGameComponent
	{
		private SpriteBatch _batch;

		private QolIndicator(Game game) : base(game)
		{
			DrawOrder = int.MaxValue - 5;   // over the game and the mods' drawing, under the Esc menu
		}

		public static void Attach(Game game) => game.Components.Add(new QolIndicator(game));

		public override void Update(GameTime gameTime) => Qol.Tick(Game);

		public override void Draw(GameTime gameTime)
		{
			if (PauseMenu.IsOpen || RenderTest.Active) return;
			System.Collections.Generic.List<string> lines = Qol.Lines();
			if (lines.Count == 0) return;
			GlobalScope.Graphics graphics = GlobalScope.m_Graphics;
			if (graphics == null) return;
			if (_batch == null) _batch = new SpriteBatch(GraphicsDevice);
			Ui.Ensure(GraphicsDevice);
			Viewport view = GraphicsDevice.Viewport;
			const int size = 9;
			float line = Ui.LineHeight(size);
			float width = 0;
			foreach (string l in lines) width = Math.Max(width, Ui.Width(graphics, l, size));
			Rectangle plate = new Rectangle(6, 6, (int)Math.Ceiling(width) + 16, (int)Math.Ceiling(line * lines.Count) + 8);
			_batch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
			Ui.Fill(_batch, Ui.Scale(plate, view), new Color(0, 0, 0, 150));
			_batch.End();
			graphics.SetImageOrigin(0f, 0f);
			graphics.SetImageRotation(0f);
			graphics.SetImageScale(1f, 1f);
			graphics.DrawStringStart();
			for (int i = 0; i < lines.Count; i++) Ui.Left(graphics, lines[i], plate.X + 8, plate.Y + 4 + i * line, line, size, i == 0 && Qol.Toasting ? Ui.Accent : Ui.Text);
			graphics.DrawStringEnd();
		}
	}
}
