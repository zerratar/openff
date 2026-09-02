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
			Options.Parse(args);
			if (Options.HelpRequested)
			{
				Console.WriteLine(Options.HelpText());
				return;
			}

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
				// The phone build targeted the Reach profile because that is all WP7 had.
				// Desktop hardware has no such limit: HiDef lifts texture-size and
				// vertex-count caps and allows better render-target formats.
				gdm.GraphicsProfile = Microsoft.Xna.Framework.Graphics.GraphicsProfile.HiDef;

				gdm.IsFullScreen = false;
				gdm.PreferredBackBufferWidth = 800;
				gdm.PreferredBackBufferHeight = 480;

				// --size=1600x960 or --fullscreen. The game lays out in a fixed space and
				// everything scales to the window, so being able to start at another size
				// is how that stays true rather than being assumed.
				string wanted = Options.Get("size");
				if (!string.IsNullOrEmpty(wanted))
				{
					string[] parts = wanted.Split('x', 'X');
					if (parts.Length == 2
						&& int.TryParse(parts[0], out int width)
						&& int.TryParse(parts[1], out int height)
						&& width > 0 && height > 0)
					{
						gdm.PreferredBackBufferWidth = width;
						gdm.PreferredBackBufferHeight = height;
					}
					else
					{
						Log.Write(LogChannel.General, "ignoring --size=" + wanted
							+ ", which is not <width>x<height>");
					}
				}

				if (Options.Get("fullscreen") != null)
				{
					gdm.IsFullScreen = true;
					gdm.PreferredBackBufferWidth = Microsoft.Xna.Framework.Graphics
						.GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
					gdm.PreferredBackBufferHeight = Microsoft.Xna.Framework.Graphics
						.GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
				}

				// The Graphics constructor asks for multisampling. On DesktopGL that
				// changes how the depth attachment is created, so make it switchable
				// while the 3D path is still being brought up: --msaa=off
				if (string.Equals(Options.Get("msaa"), "off", StringComparison.OrdinalIgnoreCase))
				{
					gdm.PreferMultiSampling = false;
				}

				// Report what the device actually gave us, not what we asked for.
				gdm.DeviceCreated += delegate
				{
					Microsoft.Xna.Framework.Graphics.PresentationParameters pp =
						gdm.GraphicsDevice.PresentationParameters;
					Log.Write(LogChannel.General, string.Format(
						"device: {0}x{1} colour={2} depth={3} msaa={4}",
						pp.BackBufferWidth, pp.BackBufferHeight, pp.BackBufferFormat,
						pp.DepthStencilFormat, pp.MultiSampleCount));
				};

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

				// --test=3d|2d : isolated render harness for the GL emulation.
				RenderTest.Attach(game);

				// F12 screenshots, or FF3_SCREENSHOT_EVERY=<seconds> for a filmstrip.
				ScreenCapture.Attach(game);

				// XNA raised SignedIn once the Guide had a profile. Nothing does that
				// here, so tell the game its local profile is ready; that is what makes
				// it load the achievement list.
				Gamer.SignalLocalSignIn();

				Log.Write(LogChannel.General, "renderer: "
					+ (NativeRenderer.Enabled ? "native" : "emulated (legacy GL path)"));
				RenderOverrides.LogState();
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
