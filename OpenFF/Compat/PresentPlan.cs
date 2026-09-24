// How the frames go out, decided from the settings and the display: how many refreshes each
// frame is held (the swap interval), how many frames a second that makes, and whether the
// display's VSync holds the loop to that rate or Hold has to pace it by the clock. Pure
// arithmetic, like FrameClock: FramePacer supplies the refresh it read from the system and
// applies what this decides, and Tools/FramePacerSim runs the same decisions against a
// simulated display.
//
// With VSync on and the refresh known, a frame is held the most whole refreshes that still
// give the rate asked for: "60" on a 60 Hz display is every refresh, on 120 Hz every second
// (the swap interval 2), on 240 every fourth; "30" on 60 Hz every second refresh. The swap
// then does the waiting, on the display's own beat, and Hold is only a ceiling a quarter
// above the rate, for a present that returns early (a render-ahead queue with room, a
// window the driver will not wait for), so it never fights the refresh. A display that
// sixty does not divide gets the nearest even rate above it - every second refresh of 144 Hz
// is 72 a second, of 165 Hz 82.5, and 75 or 100 Hz every refresh - since sixty frames spread
// over 144 refreshes would be held two or three refreshes by turns, a beat no blend can hide.
// A display so fast that sixty would want a frame held longer than a swap interval can be set
// (eight refreshes; 540 Hz wants nine) holds each frame the most it can - 67.5 a second at
// 540 Hz, the nearest even rate above sixty there is - never every refresh, which would draw
// eight times the frames for nothing. "30" is the game's own frames, so only an exact multiple
// a swap interval can hold holds it evenly; on 144 Hz it is paced by the clock, each frame
// held four or five refreshes as it must be, and so it is on 300 Hz, whose ten refreshes are
// more than a swap interval holds. "max" is every refresh.
//
// When two seconds' frames come faster than VSync would allow, it is not holding the loop
// (VSync forced off, or a present that never waits): a swap interval over one gives way to
// every refresh first, and if that does not hold either, Hold paces the same even rate by the
// clock (FramePacer's check). With VSync off Hold paces 30, 60, or for "max" the display's
// refresh or 240, whichever is more.
//
// A refresh is believed only between 20 and 1000 Hz. Anything else is a reading gone wrong
// (1 Hz is Windows' "the hardware's default") and is taken as not known, so the clock paces
// the frames as the first version did. Nothing is paced below thirty a second, the game's own
// rate. Hold's ceiling where VSync holds the loop, and the rate the clock paces where it does
// not, are thirty at least. A refresh read too low can then slow the frames only as far as the
// display itself does, and never below the rate the game steps at.

using System;
using System.Globalization;

namespace OpenFF.Client
{
	internal readonly struct PresentPlan
	{
		/// <summary>Refreshes each frame is held: the swap interval, 0 with VSync off.</summary>
		public readonly int Swap;

		/// <summary>The frames a second meant.</summary>
		public readonly double Rate;

		/// <summary>True when the display's VSync holds the loop to the rate (Hold only a ceiling above it); false when Hold paces it by the clock.</summary>
		public readonly bool Held;

		/// <summary>The display's refresh the plan was made for, in Hz (0 when not known).</summary>
		public readonly double RefreshHz;

		/// <summary>The Fps setting the plan was made for: "30", "60" or "max".</summary>
		public readonly string Fps;

		/// <summary>How far above the rate Hold's ceiling stands when VSync holds the loop.</summary>
		public const double CeilingFactor = 1.25;

		/// <summary>The fraction a rate may miss the one asked for and still count as it (59.94 for 60, 29.97 for 30).</summary>
		public const double Tolerance = 0.02;

		/// <summary>The refreshes a reading is believed between, in Hz; outside them the refresh is taken as not known.</summary>
		public const double LeastRefresh = 20, MostRefresh = 1000;

		/// <summary>The fewest frames a second Hold paces, whatever the plan: the game's own thirty ("30" on 59.94 Hz still 29.97, which counts as it).</summary>
		public const double LeastRate = 30;

		/// <summary>Whether a refresh reading is one a display could have (LeastRefresh..MostRefresh).</summary>
		public static bool Believable(double refreshHz) => refreshHz >= LeastRefresh && refreshHz <= MostRefresh;

		/// <summary>The most whole refreshes a frame can be held on this display and still give this rate, 1 at least - the swap interval the rate would want.</summary>
		public static int RefreshesFor(double refreshHz, double rate) => Math.Max(1, (int)Math.Floor(refreshHz / rate * (1 + Tolerance)));

		public PresentPlan(int swap, double rate, bool held, double refreshHz, string fps)
		{
			Swap = swap;
			Rate = rate;
			Held = held;
			RefreshHz = refreshHz;
			Fps = fps;
		}

		/// <summary>The rate Hold paces to: the plan's own when the clock paces it, a quarter above when VSync holds - and never under thirty, so a refresh read too low cannot hold the frames back further than the display does.</summary>
		public double Ceiling => Held ? Math.Max(Rate * CeilingFactor, LeastRate) : Rate;

		/// <summary>The refreshes each frame is meant to stand, as a whole number the clock can snap to; 0 when the frames do not go out on the refresh.</summary>
		public int Cadence => Held && RefreshHz > 0 ? Math.Max(1, Swap) : 0;

		/// <summary>
		/// The plan for these settings on this display. fallback: 0 as chosen; 1 when a swap interval
		/// over one did not hold (every refresh instead); 2 when VSync does not hold the loop at all.
		/// mostSwap: the largest swap interval that can be set (1 when it cannot be set at all).
		/// </summary>
		public static PresentPlan Choose(bool vsync, string fps, double refreshHz, int fallback, int mostSwap)
		{
			string f = fps == "30" || fps == "max" ? fps : "60";
			// A refresh no display has is a reading gone wrong: not known.
			if (!Believable(refreshHz)) refreshHz = 0;
			if (!vsync) return new PresentPlan(0, f == "30" ? 30 : f == "60" ? 60 : Math.Max(240, refreshHz), false, refreshHz, f);
			// The refresh not known: paced by the clock, as the first version was (240 the ceiling for "max").
			if (refreshHz <= 0) return new PresentPlan(1, f == "30" ? 30 : f == "60" ? 60 : 240, false, 0, f);
			double target = f == "30" ? 30 : f == "60" ? 60 : refreshHz;
			int even = RefreshesFor(refreshHz, target);
			// VSync not holding at all: the clock paces the even rate the swap would have given (72 on
			// 144 Hz, not a flat 60 that 144 would show two and three refreshes by turns), thirty at least.
			if (fallback >= 2) return new PresentPlan(1, f == "30" ? (Math.Abs(refreshHz / even - 30) > 30 * Tolerance ? 30 : refreshHz / even) : Math.Max(LeastRate, refreshHz / even), false, refreshHz, f);
			// Held as many of those refreshes as a swap interval can be set to: the nearest rate above
			// the one asked for past that, not every refresh. Every refresh once a longer hold gave way.
			int k = fallback >= 1 ? 1 : Math.Min(even, Math.Max(1, mostSwap));
			double rate = refreshHz / k;
			// The game's own frames on a display thirty does not divide: paced by the clock.
			if (f == "30" && Math.Abs(rate - 30) > 30 * Tolerance) return new PresentPlan(1, 30, false, refreshHz, f);
			return new PresentPlan(k, rate, true, refreshHz, f);
		}

		public bool SameAs(in PresentPlan o) => Swap == o.Swap && Held == o.Held && Math.Abs(Rate - o.Rate) < 1e-6 && Math.Abs(RefreshHz - o.RefreshHz) < 1e-6 && Fps == o.Fps;

		/// <summary>The plan in words, for the log and the overlay.</summary>
		public string Describe()
		{
			if (Fps == null) return "no frame paced yet";
			CultureInfo c = CultureInfo.InvariantCulture;
			string display = RefreshHz > 0 ? RefreshHz.ToString("0.###", c) + " Hz display" : "display's refresh not known";
			if (Swap == 0) return display + ", VSync off: " + Rate.ToString("0.##", c) + " a second by the clock";
			if (!Held) return display + ": " + Rate.ToString("0.##", c) + " a second by the clock" + (RefreshHz > 0 && Fps != "30" ? " (VSync not holding the loop)" : "");
			string every = Swap == 1 ? "every refresh" : Swap == 2 ? "every second refresh" : Swap == 3 ? "every third refresh" : "every " + Swap + "th refresh";
			return display + ": " + every + ", " + Rate.ToString("0.##", c) + " a second, paced by VSync (swap interval " + Swap + ")";
		}
	}

	/// <summary>Hold's grid: the time a frame is to go out at, one interval after the last, a late frame going out at once.</summary>
	internal struct PresentGrid
	{
		private double _next;

		public void Reset() => _next = 0;

		/// <summary>The slot a frame begun now will go out at, if it is drawn in time - what Due will give it - without taking it.</summary>
		public double Peek(double now, double interval)
		{
			double next = (_next <= 0 ? now : _next) + interval;
			return now >= next ? now : next;
		}

		/// <summary>When the frame drawn by now should be presented, at this interval: the grid's next slot, or now when that has passed.</summary>
		public double Due(double now, double interval)
		{
			if (_next <= 0) _next = now;
			_next += interval;
			if (now >= _next)
			{
				// Late: out at once. Less than an interval late, the grid stands, so the next frame makes
				// the time up; later than that (a hitch), the grid starts again from now rather than let
				// a run of frames go out back to back.
				if (now - _next > interval) _next = now;
				return now;
			}
			return _next;
		}
	}

	/// <summary>Whether VSync is holding the loop: two seconds of frames in focus coming faster than the plan's rate allows say it is not.</summary>
	internal struct HoldCheck
	{
		private double _settle, _time;
		private int _frames, _fast;

		/// <summary>The mean interval of the last second counted, in seconds.</summary>
		public double MeanInterval;

		/// <summary>Starts again, the first second of frames not counted (a queue filling, a swap interval taking hold).</summary>
		public void Reset()
		{
			_settle = 1.0;
			_time = 0;
			_frames = 0;
			_fast = 0;
		}

		/// <summary>A frame's interval; true once frames have come over a tenth faster than the rate for two seconds running. Frames out of focus (a minimised window's present may not wait) start the count again.</summary>
		public bool Frame(double interval, bool focused, double rate)
		{
			if (!focused)
			{
				_time = 0;
				_frames = 0;
				_fast = 0;
				return false;
			}
			if (interval <= 0 || interval > 0.1 || rate <= 0) return false;
			if (_settle > 0)
			{
				_settle -= interval;
				return false;
			}
			_time += interval;
			_frames++;
			if (_time < 1.0) return false;
			MeanInterval = _time / _frames;
			bool fast = _frames >= 20 && MeanInterval < 1.0 / (rate * 1.1);
			_time = 0;
			_frames = 0;
			_fast = fast ? _fast + 1 : 0;
			return _fast >= 2;
		}
	}
}
