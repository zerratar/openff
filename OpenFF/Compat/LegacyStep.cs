// The game's own steps, counted one by one. A display frame can run several (render()'s catch-up after a stall, --speed),
// and the engine's frame (OpenFF.Game.Time.Frame) counts the call that ran them once; what must keep pace with the
// game's scripts - a scene's frame count, a camera motion's frame, a fade - is ticked here, after each step's NitroMain,
// and the traces that set this game beside the Steam one count steps here too.

namespace OpenFF.Client
{
	internal static class LegacyStep
	{
		/// <summary>Steps the game has run.</summary>
		public static long Count;

		/// <summary>After each step of the game (GlobalScope.render, once a NitroMain).</summary>
		// --drawburst-at=<step>[,<draws>]: the draw calls of that step's frame dumped in full (GlDiag), as F9 does.
		private static readonly long _burstAt = BurstAt(out _burstDraws);
		private static readonly int _burstDraws;

		private static long BurstAt(out int draws)
		{
			draws = 400;
			string arg = Options.Get("drawburst-at");
			if (string.IsNullOrEmpty(arg)) return -1;
			string[] parts = arg.Split(',');
			if (parts.Length > 1 && int.TryParse(parts[1], out int n) && n > 0) draws = n;
			return long.TryParse(parts[0], out long step) ? step : -1;
		}

		public static void After()
		{
			Count++;
			if (Count == _burstAt) GlDiag.ArmBurst(_burstDraws);
			if (!GameProfile.IsFf4) return;
			Ff4CameraMotion.Tick();
			Ff4EventCamera.Tick();
			Ff4Cutscene.Tick();
		}
	}
}
