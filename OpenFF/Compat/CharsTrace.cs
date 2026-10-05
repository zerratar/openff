// --chars-trace=<file>[,<steps>]: every character the game has up, a line each every <steps> steps (15), in the
// columns Tools/ff4hook's 'chars' writes for the Steam FF4.exe - so where a character stands, whether it is hidden
// and which motion it plays can be set beside the real game's, frame by frame (Tools/ff4hook/chars_compare.py).

using System;
using System.IO;

namespace OpenFF.Client
{
	internal static class CharsTrace
	{
		private static readonly StreamWriter _file = Open(out _every);
		private static readonly int _every;

		private static StreamWriter Open(out int every)
		{
			every = 15;
			string arg = Options.Get("chars-trace");
			if (string.IsNullOrEmpty(arg)) return null;
			string[] parts = arg.Split(',');
			if (parts.Length > 1 && int.TryParse(parts[1], out int n) && n > 0) every = n;
			try { return new StreamWriter(parts[0], false); }
			catch (Exception ex) { Log.Write(LogChannel.General, "chars trace: " + parts[0] + ": " + ex.Message); return null; }
		}

		/// <summary>Once a game step.</summary>
		public static void Tick()
		{
			if (_file == null) return;
			long frame = OpenFF.Game.Time.Frame;
			if (frame % _every != 0) return;
			try
			{
				GlobalScope.characterMng.WriteState(_file, frame);
				_file.Flush();
			}
			catch (Exception ex) { Log.First(LogChannel.General, "chars-trace", 1, () => "chars trace: " + ex.Message); }
		}
	}
}
