// A scripted driver for headless tests: --drive=<file> plays key presses into the game
// from inside, so a test runs the same whether the window has focus, the desktop is locked,
// or nobody is at the machine. The script is one step per line:
//
//   wait <seconds>                 pause
//   press <key> [holdMs]           hold a key (XNA Keys names: K, Z, Down, Right, C, M...) - 120 ms unless said
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

namespace FF3
{
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
				if (_framesLeft == 0) DesktopInput.Injected.Clear();
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

		private static double Seconds(string text, double fallback)
		{
			return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double v) ? v : fallback;
		}
	}
}
