// The single point where the MonoGame loop meets the game.
//
// PORT: this used to be an Android activity stack. Game1 broadcast each lifecycle
// callback to a list of Activity objects (Android.cs), which forwarded through
// Activity -> View -> GLSurfaceView -> Renderer to reach AppShell. The list
// never held more than one activity, so all of that was ceremony around a single
// object. Boot went through a second activity as well, to decide whether the game
// data had been downloaded yet.
//
// The frame path is now:
//
//   Game1.Update -> DesktopInput           -> AppShell.onTouchEvent
//                -> GameHost.Update        -> AppShell.onDrawFrame
//   Game1.Draw   -> GameHost.Draw          -> (the same, once per frame)
//
// onDrawFrame is the game's whole tick: it feeds touch state in, runs the frame,
// and updates sound. That is the original design, not something introduced here.

using System;

namespace OpenFF.Client
{
	internal static class GameHost
	{
		private static AppShell _game;

		/// <summary>The game instance, once Create has run.</summary>
		public static AppShell Game => _game;

		/// <summary>Loads the archives and constructs the game. False if data is missing.</summary>
		public static bool Create()
		{
			if (!GameArchive.Load())
			{
				Log.Write(LogChannel.General,
					"FATAL: game archives could not be loaded from " + GameArchive.DataPath);
				return false;
			}

			_game = new AppShell();
			_game.onCreate();
			// The OpenFF engine and the mods' code, now that the content (and the mods folder) is open.
			EngineHost.Attach();
			// --cutscene=<object path>: the Cutscene on that object plays the moment its map is up (Crystal's Play in OpenFF).
			OpenFF.Cutscene.AutoPlay = string.IsNullOrWhiteSpace(Options.Get("cutscene")) ? null : Options.Get("cutscene");
			// A scripted key drive for headless tests (--drive=<file>).
			Drive.Initialise();
			Trace.Initialise();
			return true;
		}

		/// <summary>
		/// onStart / onStop were Activity virtuals the game never overrode, so these
		/// were always no-ops. Kept as the obvious place to hang start/stop work.
		/// </summary>
		public static void Start()
		{
		}

		public static void Stop()
		{
		}

		public static void Resume() => _game?.onResume();

		public static void Pause() => _game?.onPause();

		public static void Destroy()
		{
			EngineHost.Quit();
			_game?.onDestroy();
		}

		/// <summary>
		/// --speed's multiple while fast-forwarding, set each frame by DesktopInput.BeginFrame: every
		/// step the pacer gives runs this many of the game's (each tripled by boost, as Tab alone
		/// does), so --speed=N is N times Tab's own speed whatever the display's rate. 1 otherwise.
		/// </summary>
		public static int Speed = 1;

		/// <summary>
		/// One frame of the display. The game steps when FramePacer says a step is due (thirty a
		/// second; two or three at once after a stall; each times --speed's multiple while
		/// fast-forwarding), its draws recorded by FrameCapture; then the recorded frame is drawn -
		/// as it is, or, between steps with smoothing on, part of the way back toward the frame
		/// before, so the display moves at its own rate while the game keeps its own. Without the
		/// native renderer there is no recording: the frame waits out the time to its step and the
		/// step draws, as the phone build did.
		/// </summary>
		public static void Frame(Microsoft.Xna.Framework.Graphics.GraphicsDevice device)
		{
			int steps = FramePacer.Steps();
			int speed = Math.Clamp(Speed, 1, FramePacer.MostSpeed);
			if (!FrameCapture.Supported)
			{
				// The wait is taken off the pacer's clock, so the next frame does not count it again.
				while (steps == 0) steps = FramePacer.WaitForStep();
				Step(steps * speed);
				return;
			}
			if (steps > 0 || !FrameCapture.HasFrame)
			{
				Step(Math.Max(1, steps) * speed);
			}
			FrameCapture.Replay(device, GlobalScope.m_Graphics, FramePacer.Blend());
		}

		/// <summary>Steps of the game taken so far (the overlay's steps-a-second).</summary>
		public static long StepsTaken;

		/// <summary>So many steps of the game, which render() runs as one frame of its own (drawing the last, skipping the rest's drawing as its catch-up does), recorded into FrameCapture when the native renderer is on; then the client's own per-step work and the engine's frame.</summary>
		private static void Step(int steps)
		{
			StepsTaken += steps;
			FramePacer.PendingSteps = steps;
			Drive.Update();
			bool record = FrameCapture.Supported;
			if (record) FrameCapture.Begin();
			try
			{
				_game?.onDrawFrame();
			}
			finally
			{
				if (record) FrameCapture.End();
				FramePacer.PendingSteps = 1;
			}
			Banner.Tick();   // the place-name window a mod asked for this step stays; one not asked for goes
			FrameProbe.Tick();
			DevSay.Tick();
			ProgressionLayer.Tick();
			Trace.Tick();
			// The engine's frame, after the legacy one.
			EngineHost.Tick();
		}
	}
}
