// The pacer's arithmetic, with the time handed in: when the game steps, where between two
// of its steps each display frame stands, and what a window of frames looked like.
// FramePacer feeds it the Stopwatch and does the waiting; nothing here reads a clock, sleeps
// or calls the system, so a simulation (Tools/FramePacerSim compiles this very file) or a
// test can drive it with any timeline it likes.
//
// The clock. Each display frame brings the time since the one before. On a display whose
// VSync holds the loop, a frame is really held a whole number of refreshes, whatever the
// thread's wake-up jitter says, so an interval within a fifth of a refresh (two milliseconds
// at most) of its cadence - the refreshes a frame is held - or of one refresh more is taken
// as exactly the cadence. The difference is kept as drift: the jitter, and a missed refresh,
// which the screen has already shown as a picture held a refresh too long and which, counted
// at once, would make the next picture jump as far again. Up to a quarter of a refresh the
// drift is only kept - the jitter comes and goes, and the clock stays exact to the refresh -
// and beyond that the excess goes back a tenth a frame, so the clock follows the real one
// within a few milliseconds and never by more than half a step, the game keeps its thirty
// steps a second exactly, and a refresh the system reports a hair off (59 for 59.94) leaves
// only a standing offset. The game's own frames ("30", not smoothed) keep only the jitter as
// drift and take a missed refresh at once: a picture of theirs moves a whole step or none, so
// the refresh bled back would only bring the catch-up step some frames after the held
// picture, a second stutter of its own, where counted at once it lands on the picture right
// after the held one, as it did in 0.1.7. An interval that is no whole count, fewer
// refreshes than the cadence (a frame starting early after a late one), or more than one
// beyond it, is taken as measured, the drift with it; so is every interval when nothing holds
// the frames to the display (VSync off, or not holding). A stall of a quarter second or more
// is forgiven rather than caught up on.
//
// The step. The game steps once the time owed passes half a step, so what is owed stays
// within half a step either side of nought, and the display stands at 0.5 + owed / step
// between the frame before the step and the step's own: near 0 on the step's frame when the
// step has only just come due, 1 just before the next. That is even at every refresh,
// whether or not it divides the step - the picture moves by exactly the frame's own interval
// each frame - with the half step of latency the first version meant to have.
//
// Where it starts. When the refresh divides the step (60 Hz, 120, 240) and every interval is
// a whole number of refreshes, what is owed only ever takes a handful of values, and one of
// them could be the threshold itself, where a microsecond would decide whether a frame steps
// - by turns, a frame of none and a frame of two, which the game's own frames ("30") show as
// a stutter. So what is owed starts, and starts again after a stall or a change of refresh or
// cadence, half a refresh from the threshold: every value it takes then stays half a refresh
// clear of it. Smoothed, half a refresh short of it; the game's own frames, one step a frame,
// half a refresh past it - the thread only ever wakes late, never early, so a late frame then
// has a whole step of room before it could take two. A missed refresh they take at once
// moves what is owed by a whole refresh, so it stays half a refresh clear all the same.

using System;

namespace OpenFF.Client
{
	internal sealed class FrameClock
	{
		/// <summary>The game's step: 1/30 s.</summary>
		public const double Period = 1.0 / 30.0;

		/// <summary>Most steps run in one display frame to catch up; beyond that the time is forgiven.</summary>
		public const int MostCatchUp = 3;

		/// <summary>An interval this long or longer is a stall: taken as this much, and what it leaves after the catch-up is forgiven.</summary>
		public const double StallSeconds = 0.25;

		/// <summary>How near a whole number of refreshes an interval must be to be taken as exactly that: this fraction of a refresh, and never more than SnapMost.</summary>
		public const double SnapFraction = 0.2, SnapMost = 0.002;

		/// <summary>How much of the drift beyond the kept quarter refresh goes back into the clock each frame.</summary>
		public const double Bleed = 0.1;

		/// <summary>The most drift kept: beyond it the rest goes into the frame at once (a run of missed refreshes, a refresh consistently far from the one snapped to).</summary>
		public const double DriftBound = Period / 2;

		/// <summary>Whether the display is drawn between steps; false keeps Blend at 1.</summary>
		public bool Smooth = true;

		private double _refresh;
		private int _cadence = 1;
		private double _last = -1;
		private double _debt;
		private double _drift;
		private long _steps;

		// The display frame in hand, for the window: its interval, what the clock took it for, its
		// steps, and where it stood; it goes into the window when the next frame begins.
		private bool _open, _haveShown;
		private double _frameInterval, _frameIdeal, _shown, _previousShown;
		private int _frameRefreshes, _frameSteps;
		private bool _frameSnapped, _frameStall;
		private PacingWindow _window;

		/// <summary>Where the display stands between the step before and the one just taken: 0.5 + owed / step, 0..1; 1 always when not smoothing.</summary>
		public double Blend => Smooth ? Math.Clamp(0.5 + _debt / Period, 0.0, 1.0) : 1.0;

		/// <summary>Seconds from the last frame's time until the next step comes due.</summary>
		public double UntilStep => Period / 2 - _debt;

		/// <summary>The time of the last frame (or of the last wait taken within one).</summary>
		public double Last => _last;

		/// <summary>What the quantised clock still owes the real one, in seconds.</summary>
		public double Drift => _drift;

		/// <summary>Steps taken in all.</summary>
		public long Steps => _steps;

		/// <summary>The display's refresh in seconds while the frames go out on its refreshes (VSync holding the loop); 0 when every interval is taken as measured.</summary>
		public double Refresh => _refresh;

		/// <summary>Refreshes a frame is meant to be held (the swap interval): an interval of more than one refresh beyond it is a stall, not a missed refresh.</summary>
		public int Cadence => _cadence;

		/// <summary>
		/// The refresh to snap to (0: none) and the refreshes a frame is held. A new refresh or cadence
		/// moves what is owed to the nearest of the values a fresh start would take - at most half a
		/// frame either way, once - so the frames stay clear of the threshold on the new timing too.
		/// </summary>
		public void SetTiming(double refresh, int cadence)
		{
			cadence = Math.Max(1, cadence);
			refresh = Math.Max(0, refresh);
			bool moved = Math.Abs(refresh - _refresh) > 1e-12 || cadence != _cadence;
			_refresh = refresh;
			_cadence = cadence;
			if (!moved || refresh <= 0 || _last < 0) return;
			double frame = refresh * cadence;
			double off = (_debt - Centre) % frame;
			if (off > frame / 2) off -= frame;
			else if (off <= -frame / 2) off += frame;
			_debt -= off;
		}

		// What is owed on a fresh start, and after a stall: half a refresh from the threshold - short of
		// it when smoothing, past it for the game's own frames.
		private double Centre => _refresh <= 0 ? 0 : Smooth ? Period / 2 - _refresh / 2 : -Period / 2 + _refresh / 2;

		/// <summary>Starts again from nothing: the next frame is the first, and is one step.</summary>
		public void Reset()
		{
			_last = -1;
			_debt = 0;
			_drift = 0;
			_open = false;
			_haveShown = false;
		}

		/// <summary>A display frame begins at this time: the steps now due, 0..3 (the first frame is one step).</summary>
		public int Advance(double now)
		{
			Close();
			if (_last < 0)
			{
				_last = now;
				_debt = Centre;
				_drift = 0;
				_steps++;
				Open(0, 0, 0, 1, false, false);
				return 1;
			}
			double real = now - _last;
			_last = now;
			bool stall = real >= StallSeconds;
			double dt = Math.Min(real, StallSeconds);
			int refreshes = 0;
			bool snapped = false;
			if (_refresh > 0 && !stall)
			{
				// A frame held for its cadence, or one refresh more (it missed one); fewer is a frame that
				// started early after a late one, which the display never shows as such.
				refreshes = (int)Math.Round(real / _refresh);
				if (refreshes > _cadence + 1) stall = true;
				else if (refreshes >= _cadence && Math.Abs(real - refreshes * _refresh) <= Math.Min(_refresh * SnapFraction, SnapMost))
				{
					// A missed refresh is time the picture stood still, already shown as a picture held
					// twice. Smoothed, counting it at once would make the next picture jump the same again,
					// so it goes into the drift instead and comes back over the frames after. The game's own
					// frames move a whole step or none whatever is done, and bled back it would only bring
					// the catch-up step some frames after the held picture, as a stutter of its own. They
					// count it at once, and only the jitter goes into the drift.
					dt = (Smooth ? _cadence : refreshes) * _refresh;
					snapped = true;
					_drift += real - dt;
				}
			}
			// The time the frame stood for, as the window sees it: its whole refreshes, missed ones too.
			double ideal = snapped ? refreshes * _refresh : dt;
			if (!snapped)
			{
				// Taken as measured, the clock catches the real one up whole: a late frame that snapped
				// and the early one after it that did not then add up to their two refreshes.
				dt += _drift;
				_drift = 0;
			}
			// A quarter refresh of drift is only kept; beyond that the excess goes back a tenth at a
			// time, so a hitch's worth of jitter bleeds away over a few dozen frames instead of landing
			// in one, and the clock never strays from the real one by more than DriftBound: over any
			// long run the game has exactly its thirty steps a second.
			double kept = _refresh / 4;
			double back = Math.Abs(_drift) > kept ? (_drift - Math.Sign(_drift) * kept) * Bleed : 0;
			if (Math.Abs(_drift - back) > DriftBound) back = _drift - Math.Sign(_drift) * DriftBound;
			_drift -= back;
			dt += back;
			if (dt < 0)
			{
				_drift += dt;
				dt = 0;
			}
			int steps = Take(dt);
			Open(real, ideal, refreshes, steps, snapped, stall);
			return steps;
		}

		/// <summary>Time waited within the frame in hand (the emulated renderer waits for its step): taken as measured, the steps now due returned; the frame stays the same one.</summary>
		public int AdvanceWaited(double now)
		{
			if (_last < 0) return Advance(now);
			double real = Math.Max(0, now - _last);
			_last = now;
			int steps = Take(Math.Min(real, StallSeconds));
			_frameSteps += steps;
			// The wait is part of the frame: the window's interval and the time the picture stood for
			// run to the frame's end, or the frames of a waiting loop read as a thousand a second.
			_frameInterval += real;
			_frameIdeal += Math.Min(real, StallSeconds);
			_shown = _steps + Blend;
			return steps;
		}

		private int Take(double dt)
		{
			_debt += dt;
			int steps = 0;
			while (_debt > Period / 2 && steps < MostCatchUp)
			{
				_debt -= Period;
				steps++;
			}
			if (_debt > Period / 2)
			{
				// Hopelessly behind (a stall, a debugger): the rest is forgiven, not run, and what is owed
				// starts again where a fresh start puts it.
				_debt = Centre;
			}
			_steps += steps;
			return steps;
		}

		private void Open(double interval, double ideal, int refreshes, int steps, bool snapped, bool stall)
		{
			_open = true;
			_frameInterval = interval;
			_frameIdeal = ideal;
			_frameRefreshes = refreshes;
			_frameSteps = steps;
			_frameSnapped = snapped;
			_frameStall = stall;
			_shown = _steps + Blend;
		}

		// The frame in hand is done (the next begins): into the window, with how far it moved from the frame before.
		private void Close()
		{
			if (!_open) return;
			_open = false;
			if (_haveShown && _frameInterval > 0)
			{
				_window.Frame(_frameInterval, _frameIdeal / Period, _shown - _previousShown, _frameSteps, _frameRefreshes, _cadence, _refresh > 0, _frameSnapped, _frameStall);
			}
			_previousShown = _shown;
			_haveShown = true;
		}

		/// <summary>The frames since the last call, and a fresh window begun. The frame in hand is not in it yet: it goes in when the next begins.</summary>
		public PacingWindow TakeWindow()
		{
			PacingWindow w = _window;
			w.DriftMs = _drift * 1000;
			_window = default;
			return w;
		}
	}

	/// <summary>
	/// A window of display frames, kept as sums so two windows add: how many and how long, the
	/// game's steps, how evenly the frames went out, and how evenly the picture moved against
	/// the time each frame stood for - what tells a smooth display from a judder, which one
	/// frame's sample cannot.
	/// </summary>
	internal struct PacingWindow
	{
		public int Frames;
		public double Seconds;
		public long Steps;
		public double IntervalSq;
		/// <summary>Frames counted in the motion figures (not stalls).</summary>
		public int Moving;
		/// <summary>Each frame's motion in steps, and against the time the frame stood for.</summary>
		public double AdvanceSum, AdvanceSq, MotionSum, MotionSq;
		/// <summary>Frames whose picture stood still, or moved twice as far or more, against the time the frame stood for.</summary>
		public int Repeated, Doubled;
		/// <summary>Refreshes frames were held beyond their cadence: a picture shown a refresh too long.</summary>
		public int Missed;
		/// <summary>Intervals that were not taken as whole refreshes while the clock was snapping to them.</summary>
		public int Unsnapped;
		/// <summary>Hitches: frames more than a refresh later than their cadence, or a quarter second and more (a stall) - taken as measured, and left out of the motion figures.</summary>
		public int Stalls;
		public double SnappedSeconds;
		public long SnappedRefreshes;
		/// <summary>The clock's drift at the window's end, in milliseconds.</summary>
		public double DriftMs;

		public void Frame(double interval, double idealSteps, double advance, int steps, int refreshes, int cadence, bool snapping, bool snapped, bool stall)
		{
			Frames++;
			Seconds += interval;
			IntervalSq += interval * interval;
			Steps += steps;
			if (stall) Stalls++;
			else if (snapping && !snapped) Unsnapped++;
			if (snapped)
			{
				SnappedSeconds += interval;
				SnappedRefreshes += refreshes;
				if (refreshes > cadence) Missed += refreshes - cadence;
			}
			if (!stall && idealSteps > 1e-6)
			{
				double m = advance / idealSteps;
				Moving++;
				AdvanceSum += advance;
				AdvanceSq += advance * advance;
				MotionSum += m;
				MotionSq += m * m;
				if (m < 0.25) Repeated++;
				else if (m > 1.75) Doubled++;
			}
		}

		public void Add(PacingWindow o)
		{
			Frames += o.Frames;
			Seconds += o.Seconds;
			Steps += o.Steps;
			IntervalSq += o.IntervalSq;
			Moving += o.Moving;
			AdvanceSum += o.AdvanceSum;
			AdvanceSq += o.AdvanceSq;
			MotionSum += o.MotionSum;
			MotionSq += o.MotionSq;
			Repeated += o.Repeated;
			Doubled += o.Doubled;
			Missed += o.Missed;
			Unsnapped += o.Unsnapped;
			Stalls += o.Stalls;
			SnappedSeconds += o.SnappedSeconds;
			SnappedRefreshes += o.SnappedRefreshes;
			DriftMs = o.DriftMs;
		}

		public double Fps => Seconds > 0 ? Frames / Seconds : 0;
		public double StepsPerSecond => Seconds > 0 ? Steps / Seconds : 0;
		public double IntervalMs => Frames > 0 ? Seconds / Frames * 1000 : 0;
		public double IntervalSdMs => Frames > 1 ? Math.Sqrt(Math.Max(0, IntervalSq / Frames - (Seconds / Frames) * (Seconds / Frames))) * 1000 : 0;
		/// <summary>The spread of each frame's motion against the time it stood for, as a fraction: 0 is perfectly even.</summary>
		public double MotionSd => Moving > 1 ? Math.Sqrt(Math.Max(0, MotionSq / Moving - (MotionSum / Moving) * (MotionSum / Moving))) : 0;
		/// <summary>The spread of each frame's motion against the mean frame's, as a fraction: what an evenly refreshed display shows as judder.</summary>
		public double AdvanceSd
		{
			get
			{
				if (Moving < 2 || AdvanceSum <= 0) return 0;
				double mean = AdvanceSum / Moving;
				return Math.Sqrt(Math.Max(0, AdvanceSq / Moving - mean * mean)) / mean;
			}
		}
		/// <summary>The refresh the snapped intervals measure, in Hz (0 when none were snapped).</summary>
		public double MeasuredHz => SnappedSeconds > 0 ? SnappedRefreshes / SnappedSeconds : 0;
	}
}
