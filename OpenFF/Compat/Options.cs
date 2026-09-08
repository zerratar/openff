// Command-line options, with the environment variables kept as fallbacks.
//
// Parsed as the very first thing in Main, so every other component can read from
// here during its own initialisation. A command-line value always wins over the
// matching environment variable.
//
// Accepted forms:  --log=gl,input    --log gl,input    -log=gl,input

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace OpenFF.Client
{
	internal static class Options
	{
		private static readonly Dictionary<string, string> _values =
			new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		/// <summary>option name -> (environment variable, argument placeholder, help text)</summary>
		private static readonly (string Name, string Env, string Arg, string Help)[] Known =
		{
			("renderer", "FF3_RENDERER", "<mode>",
				"native (default), or emulated for the old GL translation\n" +
				"                              layer, kept only for A/B comparison"),
			("log", "FF3_LOG", "<channels>",
				"Log channels: all, or a comma separated list of\n" +
				"                              general, exception, gl, texture, content, file, sound,\n" +
				"                              input, event, firstchance"),
			("log-file", "FF3_LOG_FILE", "<path>", "Write the log somewhere other than logs/ff3.log"),
			("content", "FF3_CONTENT", "<path>", "Path to the Content directory or a game install (wins over --game/--source)"),
			("game", "FF3_GAME", "ff3|ff4", "Which game to start when no --content is given (remembered; default ff3)"),
			("source", "FF3_SOURCE", "steam|content", "The Steam install or our Content directory (remembered; default steam)"),
			("content-override", "FF3_CONTENT_OVERRIDE", "<dir>",
				"Loose files that replace archived ones (default Content/Override)"),
			("dump", "FF3_DUMP", "<dir>", "Dump decoded source blobs (images the game loads)"),
			("dump-fonts", "FF3_DUMP_FONTS", "<dir>", "Dump SpriteFont atlases as they are loaded"),
			("debug", "FF3_DEBUG", "all|<layers>", "Start with the debug overlay on (F1 toggles it): all, or boxes,labels,sprites,world,stats"),
			("nomods", "FF3_NOMODS", "", "Load no mod code or definitions (file overrides from the mods folder still apply)"),
			("screenshot-dir", "FF3_SCREENSHOT_DIR", "<dir>", "Where F12 screenshots are written"),
			("screenshot-every", "FF3_SCREENSHOT_EVERY", "<seconds>", "Capture a screenshot automatically every N seconds"),
			("speed", "FF3_SPEED", "<n>", "Extra update passes per frame while fast-forwarding"),
			("mod", "FF3_MOD", "<dir>[;dir]", "Mod folder(s) mirroring the game's file names; first wins"),
			("project", "FF3_PROJECT", "<name|dir>", "An editor project whose edits are the mods"),
			("steam-cells-off", "FF3_STEAM_CELLS_OFF", "", "Draw a Steam install's cell banks as they are, without the phone layout table"),
			("text", "FF3_TEXT", "atlas", "Text from the phone build's glyph atlases instead of TrueType"),
			("font", "FF3_FONT", "<file|dir>", "TrueType/OpenType face to use first (default: the Steam install's, then Content\\Fonts, then Windows)"),
			("size", "FF3_SIZE", "<WxH>", "Window size, e.g. 1600x960 (default 800x480)"),
			("fullscreen", "FF3_FULLSCREEN", "", "Start fullscreen at the desktop resolution"),
			("load", "FF3_LOAD", "<slot>", "FF4: start from a save slot (1-3) instead of the new game"),
			("party", "FF3_PARTY", "<type[:level],...>", "FF4: extra party members for a test start (4:10 is the child Rydia at level 10)"),
			("drive", "FF3_DRIVE", "<file>", "Play a scripted key drive from a file - wait/press/until/quit lines (headless tests; Docs/Drives)"),
			("gil", "FF3_GIL", "<n>", "FF4: gil for a test start"),
			("trace", "FF3_TRACE", "<file>", "Write a parity trace - flags, messages, sounds, maps, and everyone on the map at each drive say (Tools/parity.ps1 diffs two)")
		};

		public static bool HelpRequested { get; private set; }

		/// <summary>
		/// Options whose value is a path. Main moves the working directory to the content root's
		/// parent before the game runs, so a relative path typed on the command line is resolved
		/// here, against the directory the client was started from, while that is still current.
		/// </summary>
		private static readonly HashSet<string> PathOptions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			"log-file", "content", "content-override", "dump", "dump-fonts", "screenshot-dir", "mod", "project", "font", "drive", "trace",
		};

		private static readonly string LaunchDirectory = System.IO.Directory.GetCurrentDirectory();

		private static string Resolve(string name, string value)
		{
			if (value == null || value == "1" || !PathOptions.Contains(name)) return value;
			string[] parts = value.Split(';');
			for (int i = 0; i < parts.Length; i++)
			{
				string part = parts[i].Trim();
				if (part.Length == 0 || System.IO.Path.IsPathRooted(part)) continue;
				// A project is a name unless it looks like a path.
				if (name.Equals("project", StringComparison.OrdinalIgnoreCase) && part.IndexOfAny(new[] { '\\', '/' }) < 0 && !System.IO.Directory.Exists(System.IO.Path.Combine(LaunchDirectory, part))) continue;
				parts[i] = System.IO.Path.GetFullPath(System.IO.Path.Combine(LaunchDirectory, part));
			}
			return string.Join(";", parts);
		}

		public static void Parse(string[] args)
		{
			if (args == null)
			{
				return;
			}
			for (int i = 0; i < args.Length; i++)
			{
				string arg = args[i];
				if (string.IsNullOrEmpty(arg) || arg[0] != '-')
				{
					continue;
				}
				string body = arg.TrimStart('-');
				if (body.Equals("help", StringComparison.OrdinalIgnoreCase)
					|| body == "?" || body.Equals("h", StringComparison.OrdinalIgnoreCase))
				{
					HelpRequested = true;
					continue;
				}

				string name, value;
				int equals = body.IndexOf('=');
				if (equals >= 0)
				{
					name = body.Substring(0, equals);
					value = body.Substring(equals + 1);
				}
				else
				{
					name = body;
					// "--log gl,input": take the next token unless it is another option.
					value = (i + 1 < args.Length && args[i + 1].Length > 0 && args[i + 1][0] != '-')
						? args[++i]
						: "1";
				}
				_values[name] = Resolve(name, value.Trim('"'));
			}
		}

		/// <summary>Command line first, then the environment variable, then null.</summary>
		public static string Get(string name)
		{
			if (_values.TryGetValue(name, out string value))
			{
				return value;
			}
			foreach ((string known, string env, _, _) in Known)
			{
				if (string.Equals(known, name, StringComparison.OrdinalIgnoreCase))
				{
					string fromEnv = Environment.GetEnvironmentVariable(env);
					return string.IsNullOrEmpty(fromEnv) ? null : Resolve(name, fromEnv);
				}
			}
			return null;
		}

		public static int GetInt(string name, int fallback)
		{
			string raw = Get(name);
			return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value)
				? value : fallback;
		}

		public static double GetDouble(string name, double fallback)
		{
			string raw = Get(name);
			return double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out double value)
				? value : fallback;
		}

		public static string HelpText()
		{
			StringBuilder text = new StringBuilder();
			text.AppendLine("OpenFF - Final Fantasy III and IV (3D) from their Steam releases");
			text.AppendLine();
			text.AppendLine("Usage: OpenFF.exe [options]");
			text.AppendLine();
			foreach ((string name, string env, string arg, string help) in Known)
			{
				text.AppendLine(string.Format(CultureInfo.InvariantCulture,
					"  --{0,-16} {1,-11} {2}", name, arg, help));
				text.AppendLine(string.Format(CultureInfo.InvariantCulture,
					"  {0,-30} env: {1}", string.Empty, env));
			}
			text.AppendLine();
			text.AppendLine("Examples:");
			text.AppendLine("  OpenFF.exe --log=gl,firstchance");
			text.AppendLine("  OpenFF.exe --log=all --screenshot-every=5");
			text.AppendLine("  OpenFF.exe --content=..\\..\\..\\..\\Content");
			return text.ToString();
		}
	}
}
