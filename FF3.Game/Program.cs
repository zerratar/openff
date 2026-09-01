// Desktop entry point.
//
// The original syrcusW.dll was a library: Windows Phone hosted syrcusW.Game1 from
// its own shell project, so the assembly had no Main. This supplies one.

using System;
using System.IO;
using System.Runtime.ExceptionServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;

namespace FF3
{
	internal static class Program
	{
		[STAThread]
		private static void Main(string[] args)
		{
			Log.Initialise();

			// Large parts of the original are wrapped in `catch (Exception) {}` - most of
			// the GL emulation layer, all of the achievement code. A port failure there
			// shows up as "nothing rendered" rather than a crash, so make the swallowed
			// exceptions visible on demand:  set FF3_LOG=Exception to record them.
			if (Log.IsEnabled(LogChannel.FirstChance))
			{
				EnableFirstChanceTracing();
			}

			// The game reaches for content two different ways: through the XNA
			// ContentManager ("Content/<asset>") and directly through its own
			// java.io.FileInputStream shim ("Content/Font12.glp", "Content/data000.bin").
			// Both are relative to the working directory, so point that at whichever
			// directory actually holds Content before anything else runs.
			string contentRoot = ContentLocator.FindContentRoot();
			if (contentRoot == null)
			{
				Console.Error.WriteLine(
					"FF3: could not locate the Content directory.\n" +
					"Expected a folder named 'Content' containing data000.bin next to the\n" +
					"executable, or at ..\\..\\Unpacked\\Content relative to the project.\n" +
					"Set the FF3_CONTENT environment variable to override.");
				return;
			}
			GameFiles.BaseDirectory = Directory.GetParent(contentRoot).FullName;
			Directory.SetCurrentDirectory(GameFiles.BaseDirectory);

			using (syrcusW.Game1 game = new syrcusW.Game1())
			{
				// Game1's constructor sets RootDirectory = "Content", which MonoGame
				// resolves against the executable's directory. Point it at the real one.
				game.Content.RootDirectory = contentRoot;

				GraphicsDeviceManager gdm = GlobalScope.m_Graphics.GetGraphicsDeviceManager();

				// The phone build ran fullscreen at the panel's native size. On desktop
				// start windowed at the game's own 800x480 design resolution.
				gdm.IsFullScreen = false;
				gdm.PreferredBackBufferWidth = 800;
				gdm.PreferredBackBufferHeight = 480;
				gdm.ApplyChanges();

				game.Window.AllowUserResizing = true;
				game.Window.Title = "Final Fantasy III";

				// Game1's constructor sets this to a full second, which is sensible on a
				// phone and miserable on a desktop: the game crawls whenever the window
				// loses focus. Keep running at normal speed instead.
				game.InactiveSleepTime = TimeSpan.Zero;

				// Windows build: real mouse and keyboard, not MonoGame's TouchPanel.
				DesktopInput.Attach(game);
				game.IsMouseVisible = true;

				// Character naming: replaces the phone's system keyboard.
				TextEntry.Attach(game);

				// F12 screenshots, or FF3_SCREENSHOT_EVERY=<seconds> for a filmstrip.
				ScreenCapture.Attach(game);

				// XNA raised SignedIn once the Guide had a profile. Nothing does that
				// here, so tell the game its local profile is ready; that is what makes
				// it load the achievement list.
				Gamer.SignalLocalSignIn();

				Log.Write(LogChannel.General, "content root: " + contentRoot);
				Log.Write(LogChannel.General, "backbuffer: "
					+ gdm.PreferredBackBufferWidth + "x" + gdm.PreferredBackBufferHeight
					+ " fullscreen=" + gdm.IsFullScreen);
				game.Activated += delegate
				{
					Log.Write(LogChannel.General, "adapter: "
						+ Microsoft.Xna.Framework.Graphics.GraphicsAdapter.DefaultAdapter.Description
						+ " profile=" + gdm.GraphicsProfile);
				};

				game.Run();
			}
		}

		private static void EnableFirstChanceTracing()
		{
			System.Collections.Generic.HashSet<string> seen = new System.Collections.Generic.HashSet<string>();
			AppDomain.CurrentDomain.FirstChanceException += delegate(object sender, FirstChanceExceptionEventArgs e)
			{
				// One entry per distinct throw site: this can fire thousands of times a frame.
				string site = e.Exception.GetType().Name + ": " + e.Exception.Message
					+ "\n" + e.Exception.StackTrace;
				lock (seen)
				{
					if (!seen.Add(site))
					{
						return;
					}
				}
				Log.Write(LogChannel.FirstChance, site);
			};
		}
	}
}
