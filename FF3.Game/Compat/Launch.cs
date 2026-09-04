// Which game, from where - decided without arguments.
//
// Karl's note (2026-09-04): starting the client should not need a --content path typed
// in. Steam is where the games are, so it is the default: the client finds the installs
// the way the editor does (registry, library folders, app manifests), remembers what it
// found and what was chosen in a small file under local application data, and reads that
// first next time. Two options steer it and are remembered:
//
//   --game=ff3|ff4          which game (default: the last one chosen, else ff3)
//   --source=steam|content  the Steam install, or our extracted Content directory
//                           (default: steam when that game's install exists)
//
// An explicit --content still wins over all of this, for opening something once.
//
// The file: %LocalAppData%\OpenFF\launch.json - game, source, the two install paths,
// and when it was written. Delete it to start over.

using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using FF3.Content;

namespace FF3
{
	internal static class Launch
	{
		private sealed class Settings
		{
			[JsonPropertyName("game")] public string Game { get; set; }
			[JsonPropertyName("source")] public string Source { get; set; }
			[JsonPropertyName("ff3Steam")] public string Ff3Steam { get; set; }
			[JsonPropertyName("ff4Steam")] public string Ff4Steam { get; set; }
			/// <summary>Where the client last ran from, and its mods folder, so Crystal can export straight into it.</summary>
			[JsonPropertyName("exe")] public string Exe { get; set; }
			[JsonPropertyName("mods")] public string Mods { get; set; }
			[JsonPropertyName("updated")] public string Updated { get; set; }
		}

		public static string SettingsPath => Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OpenFF", "launch.json");

		/// <summary>What was decided, for the log and the title.</summary>
		public static string Game { get; private set; } = "ff3";
		public static string Source { get; private set; } = "content";

		/// <summary>
		/// The content root to open when no --content was given: a Steam install of the
		/// chosen game, or null to fall back to the Content directory found the usual way.
		/// </summary>
		public static string ResolveRoot()
		{
			// Asked twice at start (for the working directory, then for the content);
			// one answer, one log line, one write of the settings.
			if (_resolvedOnce)
			{
				return _resolved;
			}
			_resolvedOnce = true;
			_resolved = Resolve();
			return _resolved;
		}

		private static bool _resolvedOnce;
		private static string _resolved;

		private static string Resolve()
		{
			Settings settings = Read();
			bool explicitGame = Options.Get("game") != null;
			bool explicitSource = Options.Get("source") != null;
			string game = (Options.Get("game") ?? settings.Game ?? "ff3").Trim().ToLowerInvariant();
			if (game != "ff3" && game != "ff4")
			{
				Log.Write(LogChannel.General, "launch: unknown game '" + game + "', using ff3");
				game = "ff3";
			}
			string source = (Options.Get("source") ?? settings.Source ?? "steam").Trim().ToLowerInvariant();
			if (source != "steam" && source != "content")
			{
				Log.Write(LogChannel.General, "launch: unknown source '" + source + "', using steam");
				source = "steam";
			}

			string root = null;
			if (source == "steam")
			{
				root = SteamRoot(settings, game);
				if (root == null)
				{
					Log.Write(LogChannel.General, "launch: no Steam install of " + game.ToUpperInvariant() + " found; looking for the Content directory instead");
					source = "content";
				}
			}
			Game = game;
			Source = source;

			// Remember the choice when it was made on the command line, and the paths always.
			if (explicitGame) settings.Game = game;
			if (explicitSource) settings.Source = Options.Get("source").Trim().ToLowerInvariant();
			settings.Game ??= game;
			settings.Source ??= source;
			Write(settings);

			Log.Write(LogChannel.General, "launch: " + game.ToUpperInvariant() + " from " + source
				+ (root != null ? " at " + root : "") + " (--game=ff3|ff4, --source=steam|content; remembered in " + SettingsPath + ")");
			return root;
		}

		private static string SteamRoot(Settings settings, string game)
		{
			string remembered = game == "ff4" ? settings.Ff4Steam : settings.Ff3Steam;
			if (!string.IsNullOrEmpty(remembered) && Directory.Exists(remembered) && ContentChain.Looks(remembered))
			{
				return remembered;
			}
			string found;
			try
			{
				found = SteamInstalls.FindOne(game == "ff4" ? SteamInstalls.Ff4AppId : SteamInstalls.AppId);
			}
			catch (Exception ex)
			{
				Log.Write(LogChannel.General, "launch: Steam lookup failed: " + ex.Message);
				found = null;
			}
			if (found == null || !ContentChain.Looks(found))
			{
				return null;
			}
			if (game == "ff4") settings.Ff4Steam = found; else settings.Ff3Steam = found;
			return found;
		}

		private static Settings Read()
		{
			try
			{
				if (File.Exists(SettingsPath))
				{
					return JsonSerializer.Deserialize<Settings>(File.ReadAllText(SettingsPath)) ?? new Settings();
				}
			}
			catch (Exception ex)
			{
				Log.Write(LogChannel.General, "launch: " + SettingsPath + " unreadable (" + ex.Message + "), starting over");
			}
			return new Settings();
		}

		/// <summary>Writes the settings file with this executable's location and mods folder, keeping the rest.</summary>
		public static void RecordClient()
		{
			Write(Read());
		}

		private static void Write(Settings settings)
		{
			try
			{
				settings.Updated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
				settings.Exe = Environment.ProcessPath;
				settings.Mods = ModsFolder.Beside(AppContext.BaseDirectory);
				Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath));
				File.WriteAllText(SettingsPath, JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
			}
			catch (Exception ex)
			{
				Log.Write(LogChannel.General, "launch: could not write " + SettingsPath + ": " + ex.Message);
			}
		}
	}
}
