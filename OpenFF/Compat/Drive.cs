// A scripted driver for headless tests: --drive=<file> plays key presses into the game
// from inside, so a test runs the same whether the window has focus, the desktop is locked,
// or nobody is at the machine. The script is one step per line:
//
//   wait <seconds>                 pause
//   press <key> [holdMs]           hold a key (XNA Keys names: K, Z, Down, Right, C, M...) - 120 ms unless said
//   tap <x> <y> [holdMs]           a touch at a point of the 800x480 view (a click on the window), released after the hold
//   stick <x> <y> [holdMs]         the pad's left stick held at (x, y), each -1..1 with y up, 1000 ms unless said
//   type <text>                    typed into the open text field (the name entry's); "type" alone clears it; submit / cancel are its Enter and Escape
//   flag <group>:<index> [on|off]  a game flag set (or cleared) - a story state without playing there
//   until <regex> [timeoutSeconds] wait for a log line matching the pattern (30 s unless said; "drive: timed out" if not);
//                                  a line written since the previous until was satisfied counts too
//   say <text>                     a line in the log ("drive: <text>") to mark progress
//   quit                           close the game
//   # comment
//
// Keys are injected at the same point the keyboard is read (DesktopInput.Injected), so pad
// bits, the engine's Game.Input and the mods see them as real presses. Timing is by frames
// at 60 per second, from the first frame after the world part is up. Tests in
// Docs/Testing.md name their drive files under Docs/Drives.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Input;

namespace OpenFF.Client
{
	// MonoGame's types by name: the engine's OpenFF.Game, GameTime, Color and vectors sit a namespace up.
	using Game = Microsoft.Xna.Framework.Game;

	internal static class Drive
	{
		private sealed class Step
		{
			public string Verb;
			public string Arg;
			public double Number;
		}

		private static readonly List<Step> _steps = new List<Step>();
		private static int _at = -1;
		private static int _framesLeft;
		private static Regex _waitFor;
		private static bool _matched;
		private static string _path;
		private static bool _done;

		/// <summary>True when --drive names a script; the input layer then accepts injected keys without focus.</summary>
		public static bool Active => _path != null && !_done;

		public static void Initialise()
		{
			string path = Options.Get("drive");
			if (string.IsNullOrEmpty(path)) return;
			try
			{
				foreach (string raw in File.ReadAllLines(path))
				{
					string line = raw.Trim();
					if (line.Length == 0 || line[0] == '#') continue;
					string[] parts = line.Split(new[] { ' ', '\t' }, 2, StringSplitOptions.RemoveEmptyEntries);
					Step step = new Step { Verb = parts[0].ToLowerInvariant(), Arg = parts.Length > 1 ? parts[1].Trim() : "" };
					_steps.Add(step);
				}
				_path = path;
				Log.Write(LogChannel.General, "drive: " + _steps.Count + " step(s) from " + path);
				Log.Written += OnLogLine;
			}
			catch (Exception ex)
			{
				Log.Write(LogChannel.General, "drive: " + path + " not read: " + ex.Message);
			}
		}

		// Lines written since the last until was satisfied: a step's effect often lands in the
		// log before the drive reaches the until that waits for it.
		private static readonly List<string> _recent = new List<string>();
		private static readonly object _lock = new object();

		private static void OnLogLine(string line)
		{
			lock (_lock)
			{
				if (line.StartsWith("drive:", StringComparison.Ordinal)) return;
				if (_recent.Count < 2000) _recent.Add(line);
				Regex waitFor = _waitFor;
				if (waitFor != null && !_matched && waitFor.IsMatch(line)) _matched = true;
			}
		}

		/// <summary>Once per frame, before the input is read.</summary>
		public static void Update()
		{
			if (!Active) return;
			if (_framesLeft > 0)
			{
				_framesLeft--;
				if (_framesLeft == 0)
				{
					DesktopInput.Injected.Clear();
					DesktopInput.InjectedStick = null;
					if (_tapHeld)
					{
						_tapHeld = false;
						DesktopInput.InjectTouch(1, _tapX, _tapY);
					}
				}
				return;
			}
			if (_waitFor != null)
			{
				if (_matched) { _waitFor = null; _matched = false; lock (_lock) _recent.Clear(); }
				else if (_framesLeft == 0 && --_timeoutFrames <= 0)
				{
					Log.Write(LogChannel.General, "drive: timed out waiting for /" + _waitFor + "/");
					_waitFor = null;
				}
				else return;
			}
			_at++;
			if (_at >= _steps.Count)
			{
				_done = true;
				Log.Write(LogChannel.General, "drive: done");
				return;
			}
			Step step = _steps[_at];
			switch (step.Verb)
			{
				case "wait":
					_framesLeft = Math.Max(1, (int)Math.Round(Seconds(step.Arg, 1) * 60));
					break;
				case "press":
				{
					string[] bits = step.Arg.Split(' ', StringSplitOptions.RemoveEmptyEntries);
					if (bits.Length == 0) break;
					if (Enum.TryParse(bits[0], true, out Keys key))
					{
						DesktopInput.Injected.Add(key);
						int hold = bits.Length > 1 && int.TryParse(bits[1], out int ms) ? ms : 120;
						_framesLeft = Math.Max(2, hold * 60 / 1000);
						Log.Write(LogChannel.File, "drive: press " + key + " for " + _framesLeft + " frame(s)");
					}
					else
					{
						Log.Write(LogChannel.General, "drive: unknown key '" + bits[0] + "'");
					}
					break;
				}
				case "stick":
				{
					// The pad's left stick held at (x, y) - each -1..1, y up - for the hold, then let go:
					// the field's analog path (DesktopInput.LeftStick) and the eight-way bits read it as a pad's.
					string[] bits = step.Arg.Split(' ', StringSplitOptions.RemoveEmptyEntries);
					if (bits.Length < 2 || !float.TryParse(bits[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float sx) || !float.TryParse(bits[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float sy))
					{
						Log.Write(LogChannel.General, "drive: stick wants <x> <y> [holdMs], each -1..1");
						break;
					}
					int hold = bits.Length > 2 && int.TryParse(bits[2], out int ms) ? ms : 1000;
					_framesLeft = Math.Max(2, hold * 60 / 1000);
					DesktopInput.InjectedStick = new Microsoft.Xna.Framework.Vector2(sx, sy);
					Log.Write(LogChannel.File, "drive: stick " + sx.ToString(CultureInfo.InvariantCulture) + "," + sy.ToString(CultureInfo.InvariantCulture) + " for " + _framesLeft + " frame(s)");
					break;
				}
				case "tap":
				{
					// A touch at a point of the 800x480 view, held a moment then released - what a
					// click on the window does (DesktopInput.UpdateMouse).
					string[] bits = step.Arg.Split(' ', StringSplitOptions.RemoveEmptyEntries);
					if (bits.Length < 2 || !int.TryParse(bits[0], out _tapX) || !int.TryParse(bits[1], out _tapY))
					{
						Log.Write(LogChannel.General, "drive: tap wants <x> <y> [holdMs] in the 800x480 view");
						break;
					}
					int hold = bits.Length > 2 && int.TryParse(bits[2], out int ms) ? ms : 120;
					_framesLeft = Math.Max(2, hold * 60 / 1000);
					_tapHeld = true;
					DesktopInput.InjectTouch(0, _tapX, _tapY);
					Log.Write(LogChannel.File, "drive: tap " + _tapX + "," + _tapY + " for " + _framesLeft + " frame(s)");
					break;
				}
				case "type":
					// Into the text field that is open (the name entry's), as typing would.
					if (TextEntry.Instance != null && TextEntry.Instance.IsActive)
					{
						TextEntry.Instance.Inject(step.Arg);
						Log.Write(LogChannel.File, "drive: typed \"" + step.Arg + "\"");
					}
					else Log.Write(LogChannel.General, "drive: type - no text field is open");
					break;
				case "flag":
				{
					// A game flag set or cleared: "flag 0:14 on" - to put a map in a story state
					// without playing there (a scene that boots under it, a chest opened).
					string[] bits = step.Arg.Split(' ', StringSplitOptions.RemoveEmptyEntries);
					string[] pair = bits.Length > 0 ? bits[0].Split(':') : new string[0];
					if (pair.Length != 2 || !uint.TryParse(pair[0], out uint group) || !uint.TryParse(pair[1], out uint index))
					{
						Log.Write(LogChannel.General, "drive: flag wants <group>:<index> [on|off]");
						break;
					}
					bool on = bits.Length < 2 || !string.Equals(bits[1], "off", StringComparison.OrdinalIgnoreCase);
					try
					{
						if (on) GlobalScope.FlagManager.singleton().set(group, index); else GlobalScope.FlagManager.singleton().reset(group, index);
						Log.Write(LogChannel.File, "drive: flag " + group + ":" + index + (on ? " on" : " off"));
					}
					catch (Exception ex) { Log.Write(LogChannel.General, "drive: flag " + bits[0] + " failed: " + ex.Message); }
					break;
				}
				case "submit":
				case "cancel":
					// Enter or Escape on the open text field.
					if (TextEntry.Instance != null && TextEntry.Instance.IsActive)
					{
						TextEntry.Instance.Finish(step.Verb == "submit");
						Log.Write(LogChannel.File, "drive: " + step.Verb);
					}
					else Log.Write(LogChannel.General, "drive: " + step.Verb + " - no text field is open");
					break;
				case "until":
				{
					string pattern = step.Arg;
					double timeout = 30;
					int space = pattern.LastIndexOf(' ');
					if (space > 0 && double.TryParse(pattern.Substring(space + 1), NumberStyles.Float, CultureInfo.InvariantCulture, out double t))
					{
						timeout = t;
						pattern = pattern.Substring(0, space).Trim();
					}
					try { _waitFor = new Regex(pattern, RegexOptions.IgnoreCase); }
					catch (Exception ex) { Log.Write(LogChannel.General, "drive: bad pattern " + pattern + ": " + ex.Message); break; }
					_timeoutFrames = (int)(timeout * 60);
					lock (_lock)
					{
						_matched = false;
						foreach (string line in _recent)
						{
							if (_waitFor.IsMatch(line)) { _matched = true; break; }
						}
					}
					break;
				}
				case "say":
					Log.Write(LogChannel.General, "drive: " + step.Arg);
					Trace.Mark(step.Arg);
					break;
				case "quit":
					_done = true;
					Log.Write(LogChannel.General, "drive: quit");
					Log.Flush();
					try { GlobalScope.m_Graphics.getGame().Exit(); } catch (Exception) { Environment.Exit(0); }
					break;
				default:
					Log.Write(LogChannel.General, "drive: unknown step '" + step.Verb + "'");
					break;
			}
		}

		private static int _timeoutFrames;
		private static bool _tapHeld;
		private static int _tapX, _tapY;

		private static double Seconds(string text, double fallback)
		{
			return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double v) ? v : fallback;
		}
	}
}
