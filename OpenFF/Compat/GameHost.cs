// The single point where the MonoGame loop meets the game.
//
// PORT: this used to be an Android activity stack. Game1 broadcast each lifecycle
// callback to a list of Activity objects (Android.cs), which forwarded through
// Activity -> View -> GLSurfaceView -> Renderer to reach MainActivity. The list
// never held more than one activity, so all of that was ceremony around a single
// object. Boot went through a second activity as well, to decide whether the game
// data had been downloaded yet.
//
// The frame path is now:
//
//   Game1.Update -> DesktopInput           -> MainActivity.onTouchEvent
//                -> GameHost.Update        -> MainActivity.onDrawFrame
//   Game1.Draw   -> GameHost.Draw          -> (the same, once per frame)
//
// onDrawFrame is the game's whole tick: it feeds touch state in, runs the frame,
// and updates sound. That is the original design, not something introduced here.

using System;

namespace OpenFF.Client
{
	internal static class GameHost
	{
		private static MainActivity _game;

		/// <summary>The game instance, once Create has run.</summary>
		public static MainActivity Game => _game;

		/// <summary>Loads the archives and constructs the game. False if data is missing.</summary>
		public static bool Create()
		{
			if (!GameArchive.Load())
			{
				Log.Write(LogChannel.General,
					"FATAL: game archives could not be loaded from " + GameArchive.DataPath);
				return false;
			}

			_game = new MainActivity();
			_game.onCreate();
			// The OpenFF engine and the mods' code, now that the content (and the mods folder) is open.
			EngineHost.Attach();
			// A scripted key drive for headless tests (--drive=<file>).
			Drive.Initialise();
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
		/// One game tick. The original ran the whole frame - input, logic and drawing -
		/// from the surface callback, so update and draw are not separable here.
		/// </summary>
		public static void Tick()
		{
			_game?.onDrawFrame();
			FrameProbe.Tick();
			DevSay.Tick();
			// The engine's frame, after the legacy one.
			EngineHost.Tick();
		}
	}
}
