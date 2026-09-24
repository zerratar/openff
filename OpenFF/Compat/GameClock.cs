// The game's own time: what its steps stand for, not what the wall clock says.
//
// The game lives by its steps - thirty a second, each three passes of NitroMain while Tab
// fast-forwards (render(), GlobalScope.boost) - and FramePacer takes them as they come due,
// the display drawing in between. What the client animates beside the game is timed by the
// same count: a mod's glTF clip on the map or in a hand (ModMeshes, WeaponMeshes), a dressed
// character's own clips (CharacterMeshes), the engine's Game.Time (EngineHost.Tick). A step
// moves this clock a thirtieth of a second, three under fast-forward, however long the step
// took to come round - so those keep pace with the game through a refresh rate that does not
// divide thirty, a catch-up after a stall and a fast-forward, --speed's too, and stand still
// when the game does (a stall the pacer forgives, the window in the background, a debugger's
// pause). Read during a step, the clock already counts it (GameHost.StepsTaken is counted
// before the game runs), so a pose taken inside the step is the pose for that step's time:
// one step on from the last one, exactly, and FrameCapture draws the frames in between.

using System;

namespace OpenFF.Client
{
	internal static class GameClock
	{
		/// <summary>
		/// The most one step of the engine's may stand for (EngineHost.Tick, and a clip's step in ModMeshes and
		/// WeaponMeshes): what the last step did stand for (StepSpan) - up to three of the game's steps caught up
		/// at once, each times --speed's multiple and three passes under fast-forward - and never less than nine
		/// steps' worth, so a clip shown again after a while picks up a few steps on, not where the time went. Nor
		/// more than render() runs in one frame (FramePacer.MostPending steps, three passes apiece: 19.2 s), the
		/// clock being the game's steps and not the wall's.
		/// </summary>
		public static double MaxAdvance => Math.Clamp(StepSpan, 9 * FramePacer.Period, FramePacer.MostPending * 3 * FramePacer.Period);

		private static long _counted;
		private static double _seconds;
		private static double _span = FramePacer.Period;

		/// <summary>The game's seconds since it started: its steps over thirty, each counted three times over while it fast-forwards.</summary>
		public static double Seconds
		{
			get
			{
				long steps = GameHost.StepsTaken;
				if (steps > _counted)
				{
					// The steps since the last reading, at the speed the game runs them now. The engine's frame
					// reads the clock every step (EngineHost.Tick), and the speed changes only between display
					// frames (DesktopInput), so each step is counted at its own speed - and what one step of the
					// pacer's stood for (GameHost.Step: the steps due, times --speed's multiple) is this reading's.
					_span = (steps - _counted) * FramePacer.Period * (GlobalScope.boost != 0 ? 3 : 1);
					_seconds += _span;
					_counted = steps;
				}
				return _seconds;
			}
		}

		/// <summary>The game's seconds the last step of the pacer's stood for (GameHost.Step, all its steps at their speed); read during a step, that step's.</summary>
		public static double StepSpan
		{
			get
			{
				_ = Seconds;
				return _span;
			}
		}
	}
}
