// --autokey=<command>[,<frames>[,<key>]]: a key pressed <frames> (60) game steps after a script reaches <command> at a
// place it was not at a moment before - StartMessage, the line that waits for one. The Steam game is driven the same
// way by Tools/ff4hook's 'autokey', so the two runs move through a scene's lines on the same frames and every
// command after them can be set side by side frame for frame (a press every few seconds of wall clock lands on a
// different frame of each line in each run, and everything after a line drifts with it).

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace OpenFF.Client
{
	internal static class AutoKey
	{
		private static readonly string _command;
		private static readonly int _frames = 60;
		private static readonly Keys _key = Keys.Z;
		private static readonly Dictionary<uint, long> _lastSeen = new Dictionary<uint, long>();
		private static readonly List<long> _pressAt = new List<long>();
		private static long _releaseAt = -1;

		static AutoKey()
		{
			string arg = Options.Get("autokey");
			if (string.IsNullOrEmpty(arg)) return;
			string[] parts = arg.Split(',');
			_command = parts[0];
			if (parts.Length > 1 && int.TryParse(parts[1], out int frames) && frames > 0) _frames = frames;
			if (parts.Length > 2 && Enum.TryParse(parts[2], true, out Keys key)) _key = key;
		}

		public static bool Active => _command != null;

		/// <summary>A script command about to run (ScriptCommands.Dispatch), by its simplified name.</summary>
		public static void Seen(string name, uint pc)
		{
			if (_command == null || !string.Equals(name, _command, StringComparison.OrdinalIgnoreCase)) return;
			long frame = LegacyStep.Count;
			bool fresh = !_lastSeen.TryGetValue(pc, out long last) || frame - last > 2;
			_lastSeen[pc] = frame;
			if (!fresh) return;
			_pressAt.Add(frame + _frames);
			Log.Write(LogChannel.File, "autokey: " + name + " at " + pc + ", " + _key + " at step " + (frame + _frames));
		}

		/// <summary>Once a game step.</summary>
		public static void Tick()
		{
			if (_command == null) return;
			long frame = LegacyStep.Count;
			if (_releaseAt >= 0 && frame >= _releaseAt)
			{
				DesktopInput.Injected.Remove(_key);
				_releaseAt = -1;
			}
			for (int i = _pressAt.Count - 1; i >= 0; i--)
			{
				if (_pressAt[i] > frame) continue;
				_pressAt.RemoveAt(i);
				DesktopInput.Injected.Add(_key);
				_releaseAt = frame + 4;
			}
		}
	}
}
