// Desktop entry point.
//
// The original game assembly was a library: Windows Phone hosted Game1 from
// its own shell project, so the assembly had no Main. This supplies one.

using System;
using System.IO;
using System.Runtime.ExceptionServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;

namespace OpenFF.Client
{
	// MonoGame's types by name: the engine's OpenFF.Game, GameTime, Color and vectors sit a namespace up.
	using Game = Microsoft.Xna.Framework.Game;

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
					"OpenFF: no game content found.\n" +
					"Expected a Steam install of Final Fantasy III or Final Fantasy IV (3D Remake),\n" +
					"or a directory named with --content=<dir> (also FF3_CONTENT).");
				return;
			}
			GameFiles.BaseDirectory = Directory.GetParent(contentRoot).FullName;
			Directory.SetCurrentDirectory(GameFiles.BaseDirectory);

			using (OpenFF.Client.Game1 game = new OpenFF.Client.Game1())
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

				// The player's display settings (%LocalAppData%\OpenFF\settings.json: size, windowed /
				// borderless / fullscreen, anti-aliasing, vsync), the command line on top for one run
				// (--size=WxH, --fullscreen, --windowed, --borderless, --msaa=off|2|4|8, --novsync).
				// The game lays out in a fixed 800x480 space and everything scales to the window.
				DisplaySettings.Load().Apply(gdm);

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
				game.Window.Title = GameProfile.Title;
				// A window the player resizes is the size next time; Alt+Enter is polled each frame (DesktopInput.BeginFrame).
				game.Window.ClientSizeChanged += delegate { DisplaySettings.WindowResized(gdm, game); };

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

				// The mod list, on the title where the phone's network entry was.
				ModListScreen.Attach(game);

				// What mods draw (Game.Draw) and what they read (Game.Input).
				ModDraw.Attach(game);
				EngineInput.Attach(game);

				// F1 debug overlay (boxes, sprites, world, stats); --debug=all starts with it on.
				DebugOverlay.Attach(game);

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
