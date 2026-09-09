// The player's display settings: %LocalAppData%\OpenFF\settings.json.
//
//   {
//     "width": 1600, "height": 960,        the window's size (windowed mode)
//     "mode": "windowed",                   windowed | borderless (the desktop's size, no frame) | fullscreen
//     "msaa": 4,                            anti-aliasing samples: 0 (off), 2, 4, 8
//     "vsync": true                         wait for the monitor
//   }
//
// Written with its defaults the first time the client runs, so there is a file to edit.
// The command line still wins for one run (--size=WxH, --fullscreen, --msaa=off|2|4|8,
// --windowed); Alt+Enter switches windowed and full screen while playing and remembers
// the choice here. A settings screen in the game, or a launcher, is the plan; this file
// is what either will write.

using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace OpenFF.Client
{
	// MonoGame's types by name: the engine's OpenFF.Game sits a namespace up.
	using Game = Microsoft.Xna.Framework.Game;

	internal sealed class DisplaySettings
	{
		[JsonPropertyName("width")] public int Width { get; set; } = 1600;
		[JsonPropertyName("height")] public int Height { get; set; } = 960;
		/// <summary>windowed, borderless or fullscreen.</summary>
		[JsonPropertyName("mode")] public string Mode { get; set; } = "windowed";
		/// <summary>Anti-aliasing samples: 0, 2, 4 or 8.</summary>
		[JsonPropertyName("msaa")] public int Msaa { get; set; } = 4;
		[JsonPropertyName("vsync")] public bool VSync { get; set; } = true;
		[JsonPropertyName("_help")] public string Help { get; set; } =
			"width/height: the window (windowed mode). mode: windowed | borderless | fullscreen. msaa: 0, 2, 4 or 8. vsync: true/false. Alt+Enter in the game switches windowed and full screen and saves it here. The command line (--size=WxH, --fullscreen, --windowed, --msaa=n) wins for one run.";

		public static string Path => System.IO.Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OpenFF", "settings.json");

		public static DisplaySettings Current { get; private set; } = new DisplaySettings();

		private static readonly JsonSerializerOptions Json = new JsonSerializerOptions { WriteIndented = true };

		/// <summary>Reads the file, writing one with the defaults when there is none; the command line's one-run overrides on top.</summary>
		public static DisplaySettings Load()
		{
			DisplaySettings settings = null;
			try
			{
				if (File.Exists(Path)) settings = JsonSerializer.Deserialize<DisplaySettings>(File.ReadAllText(Path));
			}
			catch (Exception ex)
			{
				Log.Write(LogChannel.General, "settings: " + Path + " could not be read (" + ex.Message + "); the defaults stand");
			}
			if (settings == null)
			{
				settings = new DisplaySettings();
				settings.Save();
			}
			settings.Width = Math.Clamp(settings.Width, 320, 16384);
			settings.Height = Math.Clamp(settings.Height, 200, 16384);
			settings.Msaa = NormaliseMsaa(settings.Msaa);
			settings.Mode = NormaliseMode(settings.Mode);

			// The command line, for this run only.
			string size = Options.Get("size");
			if (!string.IsNullOrEmpty(size))
			{
				string[] parts = size.Split('x', 'X');
				if (parts.Length == 2 && int.TryParse(parts[0], out int w) && int.TryParse(parts[1], out int h) && w > 0 && h > 0) { settings.Width = w; settings.Height = h; }
				else Log.Write(LogChannel.General, "ignoring --size=" + size + ", which is not <width>x<height>");
			}
			if (Options.Get("fullscreen") != null) settings.Mode = "fullscreen";
			if (Options.Get("windowed") != null) settings.Mode = "windowed";
			if (Options.Get("borderless") != null) settings.Mode = "borderless";
			string msaa = Options.Get("msaa");
			if (!string.IsNullOrEmpty(msaa)) settings.Msaa = string.Equals(msaa, "off", StringComparison.OrdinalIgnoreCase) ? 0 : int.TryParse(msaa, out int n) ? NormaliseMsaa(n) : settings.Msaa;
			if (Options.Get("novsync") != null) settings.VSync = false;
			Current = settings;
			return settings;
		}

		private static int NormaliseMsaa(int n) => n >= 8 ? 8 : n >= 4 ? 4 : n >= 2 ? 2 : 0;

		private static string NormaliseMode(string mode)
		{
			switch ((mode ?? "").Trim().ToLowerInvariant())
			{
				case "fullscreen": case "full": return "fullscreen";
				case "borderless": case "windowed fullscreen": case "windowed-fullscreen": return "borderless";
				default: return "windowed";
			}
		}

		public void Save()
		{
			try
			{
				Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path));
				File.WriteAllText(Path, JsonSerializer.Serialize(this, Json));
			}
			catch (Exception ex)
			{
				Log.Write(LogChannel.General, "settings: could not write " + Path + ": " + ex.Message);
			}
		}

		/// <summary>Lays the settings on the device manager before the device is made.</summary>
		public void Apply(GraphicsDeviceManager gdm)
		{
			DisplayMode desktop = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
			bool full = Mode != "windowed";
			gdm.IsFullScreen = full;
			// Borderless: the desktop's own mode, no frame, no mode switch - what most players want
			// from "fullscreen" today. Fullscreen proper switches the display to the size asked.
			gdm.HardwareModeSwitch = Mode == "fullscreen";
			gdm.PreferredBackBufferWidth = Mode == "borderless" ? desktop.Width : Width;
			gdm.PreferredBackBufferHeight = Mode == "borderless" ? desktop.Height : Height;
			gdm.PreferMultiSampling = Msaa > 0;
			gdm.SynchronizeWithVerticalRetrace = VSync;
			gdm.PreparingDeviceSettings += (_, e) =>
			{
				e.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount = Msaa;
			};
			Log.Write(LogChannel.General, "settings: " + Width + "x" + Height + " " + Mode + " msaa=" + Msaa + " vsync=" + VSync + " (" + Path + ")");
		}

		// ---- Alt+Enter while playing ----

		private static bool _altEnterDown;

		/// <summary>Alt+Enter: windowed and full screen (borderless) in turn, remembered in the file. Called once a frame.</summary>
		public static void Poll(Game game, GraphicsDeviceManager gdm)
		{
			if (game == null || gdm == null || !game.IsActive) return;
			KeyboardState keys = Keyboard.GetState();
			bool down = (keys.IsKeyDown(Keys.LeftAlt) || keys.IsKeyDown(Keys.RightAlt)) && keys.IsKeyDown(Keys.Enter);
			if (down && !_altEnterDown) Toggle(gdm);
			_altEnterDown = down;
		}

		/// <summary>Windowed to full screen (the file's fullscreen or borderless; borderless when it says windowed) and back.</summary>
		public static void Toggle(GraphicsDeviceManager gdm)
		{
			DisplaySettings s = Current;
			DisplayMode desktop = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
			if (gdm.IsFullScreen)
			{
				s.Mode = "windowed";
				gdm.IsFullScreen = false;
				gdm.HardwareModeSwitch = false;
				gdm.PreferredBackBufferWidth = s.Width;
				gdm.PreferredBackBufferHeight = s.Height;
			}
			else
			{
				if (s.Mode == "windowed") s.Mode = "borderless";
				gdm.HardwareModeSwitch = s.Mode == "fullscreen";
				gdm.PreferredBackBufferWidth = s.Mode == "borderless" ? desktop.Width : s.Width;
				gdm.PreferredBackBufferHeight = s.Mode == "borderless" ? desktop.Height : s.Height;
				gdm.IsFullScreen = true;
			}
			try { gdm.ApplyChanges(); }
			catch (Exception ex) { Log.Write(LogChannel.General, "settings: the display did not switch: " + ex.Message); }
			Log.Write(LogChannel.General, "settings: " + (gdm.IsFullScreen ? s.Mode : "windowed") + " (Alt+Enter)");
			s.Save();
		}

		/// <summary>The window's size as the player left it, kept for next time (windowed mode only).</summary>
		public static void WindowResized(GraphicsDeviceManager gdm, Game game)
		{
			if (gdm == null || gdm.IsFullScreen || game == null) return;
			Rectangle bounds = game.Window.ClientBounds;
			if (bounds.Width < 320 || bounds.Height < 200) return;
			if (bounds.Width == Current.Width && bounds.Height == Current.Height) return;
			Current.Width = bounds.Width;
			Current.Height = bounds.Height;
			Current.Save();
		}
	}
}
