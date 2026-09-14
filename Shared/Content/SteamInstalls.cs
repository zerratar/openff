// Finding a Steam copy of the game, so nobody has to type a path.
//
// Shared by the editor and the client (compiled into both from Shared/). The editor is
// meant to be usable by somebody who has the game and not much else, and
// "point --content at your install" is a worse first step than it sounds: the path has
// a space and a bracket in it, Steam puts libraries on whichever drive had room, and
// the folder is not named after the game the store sells.
//
// Steam records enough to work it out:
//
//   HKCU\Software\Valve\Steam\SteamPath          where Steam itself is
//   <steam>/steamapps/libraryfolders.vdf         every library, including that one
//   <library>/steamapps/appmanifest_239120.acf   the game, if it is in that library
//   its "installdir"                             the folder under steamapps/common
//
// The appmanifest is what settles it. libraryfolders.vdf also carries an "apps" list
// per library, and on the machine this was written on that list did not mention 239120
// even though the manifest sat right beside it - so the list is a hint and the
// manifest is the fact.
//
// GOG sells the same two games (the same files, no steam_api.dll), and both its installer
// and Galaxy record every installed game under
//
//   HKLM\SOFTWARE\WOW6432Node\GOG.com\Games\<productId>   path, gameName, exe
//
// The product ids are not relied on: every key's path is looked at, and the folder is the
// game when it holds the game's own executable (FF3_Win32.exe / FF4.exe - the pixel
// remasters on the same store are named otherwise) and its files. Galaxy's default game
// folders are looked at too, for a copy installed on a machine whose registry was cleaned.
// A Steam copy is listed first when both are present.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace OpenFF.Content
{
	internal sealed class SteamInstall
	{
		/// <summary>The directory holding the executable and the files folder.</summary>
		public string Path { get; set; }

		/// <summary>What the store calls it - "Final Fantasy III (3D Remake)"; a GOG copy's says "(GOG)".</summary>
		public string Name { get; set; }

		/// <summary>"Steam" or "GOG".</summary>
		public string Store { get; set; } = "Steam";

		public override string ToString()
		{
			return Name == null ? Path : Name + "  " + Path;
		}
	}

	internal static class SteamInstalls
	{
		/// <summary>Final Fantasy III (3D Remake) on Steam.</summary>
		public const string AppId = "239120";

		/// <summary>Final Fantasy IV (3D Remake) on Steam - the same engine, a year on.</summary>
		public const string Ff4AppId = "312750";

		/// <summary>Every copy of FF3 this machine has, best guess first. Empty if none.</summary>
		public static List<SteamInstall> Find()
		{
			return Find(AppId);
		}

		/// <summary>Every copy of one game, by Steam app id.</summary>
		public static List<SteamInstall> Find(string appId)
		{
			List<SteamInstall> found = new List<SteamInstall>();
			HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			foreach (string library in Libraries())
			{
				string manifest = Path.Combine(library, "steamapps", "appmanifest_" + appId + ".acf");
				if (!File.Exists(manifest))
				{
					continue;
				}

				string text;
				try
				{
					text = File.ReadAllText(manifest);
				}
				catch (IOException)
				{
					continue;
				}

				string directory = Value(text, "installdir");
				if (string.IsNullOrEmpty(directory))
				{
					continue;
				}

				string path = Path.Combine(library, "steamapps", "common", directory);
				if (!Directory.Exists(path) || !LooseContentSource.Looks(path))
				{
					// Listed but not actually there: an uninstall that left the manifest,
					// or a library on a drive that is not plugged in.
					continue;
				}

				if (seen.Add(Path.GetFullPath(path)))
				{
					found.Add(new SteamInstall { Path = path, Name = Value(text, "name") });
				}
			}

			foreach (SteamInstall gog in Gog(appId))
			{
				if (seen.Add(Path.GetFullPath(gog.Path)))
				{
					found.Add(gog);
				}
			}

			return found;
		}

		// ---- GOG

		/// <summary>The game's own executable, which tells the two apart and both from anything else GOG sells.</summary>
		private static string Executable(string appId)
		{
			return appId == Ff4AppId ? "FF4.exe" : "FF3_Win32.exe";
		}

		/// <summary>Every GOG copy of one game: the registry's, then Galaxy's usual folders.</summary>
		private static IEnumerable<SteamInstall> Gog(string appId)
		{
			if (!OperatingSystem.IsWindows())
			{
				yield break;
			}
			string exe = Executable(appId);
			string fallbackName = appId == Ff4AppId ? "Final Fantasy IV (3D Remake)" : "Final Fantasy III (3D Remake)";

			foreach ((string path, string name) in GogRegistry())
			{
				if (IsGame(path, exe))
				{
					yield return new SteamInstall { Path = path, Name = (string.IsNullOrWhiteSpace(name) ? fallbackName : name) + " (GOG)", Store = "GOG" };
				}
			}

			foreach (string folder in GogFolders())
			{
				string[] children;
				try
				{
					children = Directory.Exists(folder) ? Directory.GetDirectories(folder) : Array.Empty<string>();
				}
				catch (Exception)
				{
					continue;
				}
				foreach (string child in children)
				{
					if (IsGame(child, exe))
					{
						yield return new SteamInstall { Path = child, Name = fallbackName + " (GOG)", Store = "GOG" };
					}
				}
			}
		}

		/// <summary>Whether a folder is this game: its executable is there, and its files.</summary>
		private static bool IsGame(string path, string exe)
		{
			try
			{
				return !string.IsNullOrEmpty(path) && Directory.Exists(path)
					&& File.Exists(Path.Combine(path, exe)) && LooseContentSource.Looks(path);
			}
			catch (Exception)
			{
				return false;
			}
		}

		/// <summary>Every game GOG's installer or Galaxy has recorded: (path, gameName).</summary>
		private static IEnumerable<(string Path, string Name)> GogRegistry()
		{
			List<(string, string)> games = new List<(string, string)>();
			foreach ((RegistryKey root, string subkey) in new[]
			{
				(Registry.LocalMachine, @"SOFTWARE\WOW6432Node\GOG.com\Games"),
				(Registry.LocalMachine, @"SOFTWARE\GOG.com\Games"),
				(Registry.CurrentUser, @"SOFTWARE\GOG.com\Games")
			})
			{
				try
				{
					using RegistryKey key = root.OpenSubKey(subkey);
					if (key == null)
					{
						continue;
					}
					foreach (string id in key.GetSubKeyNames())
					{
						using RegistryKey game = key.OpenSubKey(id);
						string path = game?.GetValue("path") as string ?? game?.GetValue("PATH") as string;
						if (!string.IsNullOrEmpty(path))
						{
							games.Add((path.TrimEnd('\\', '/'), game.GetValue("gameName") as string));
						}
					}
				}
				catch (Exception)
				{
					// An unreadable hive is not a reason to stop looking.
				}
			}
			return games;
		}

		/// <summary>Where Galaxy puts games unless told otherwise, and the old installer's default.</summary>
		private static IEnumerable<string> GogFolders()
		{
			foreach (string program in new[] { Environment.GetEnvironmentVariable("ProgramFiles(x86)"), Environment.GetEnvironmentVariable("ProgramFiles") })
			{
				if (!string.IsNullOrEmpty(program))
				{
					yield return Path.Combine(program, "GOG Galaxy", "Games");
					yield return Path.Combine(program, "GOG.com");
				}
			}
			foreach (DriveInfo drive in SafeDrives())
			{
				yield return Path.Combine(drive.RootDirectory.FullName, "GOG Games");
				yield return Path.Combine(drive.RootDirectory.FullName, "Games");
			}
		}

		private static IEnumerable<DriveInfo> SafeDrives()
		{
			DriveInfo[] drives;
			try
			{
				drives = DriveInfo.GetDrives();
			}
			catch (Exception)
			{
				return Array.Empty<DriveInfo>();
			}
			List<DriveInfo> fixedOnes = new List<DriveInfo>();
			foreach (DriveInfo d in drives)
			{
				try
				{
					if (d.DriveType == DriveType.Fixed && d.IsReady) fixedOnes.Add(d);
				}
				catch (Exception) { }
			}
			return fixedOnes;
		}

		/// <summary>The first FF3, or null. What a default wants.</summary>
		public static string FindOne()
		{
			return FindOne(AppId);
		}

		public static string FindOne(string appId)
		{
			List<SteamInstall> all = Find(appId);
			return all.Count > 0 ? all[0].Path : null;
		}

		/// <summary>Every Steam library root, starting with Steam's own.</summary>
		private static IEnumerable<string> Libraries()
		{
			string steam = SteamRoot();
			if (string.IsNullOrEmpty(steam))
			{
				yield break;
			}

			yield return steam;

			string vdf = Path.Combine(steam, "steamapps", "libraryfolders.vdf");
			if (!File.Exists(vdf))
			{
				yield break;
			}

			string text;
			try
			{
				text = File.ReadAllText(vdf);
			}
			catch (IOException)
			{
				yield break;
			}

			// Every "path" in the file. The format nests, but the only thing wanted from
			// it is that one repeated key, so this reads them rather than parsing VDF.
			foreach (Match match in Regex.Matches(text, "\"path\"\\s*\"([^\"]*)\""))
			{
				string path = match.Groups[1].Value.Replace("\\\\", "\\");
				if (path.Length > 0)
				{
					yield return path;
				}
			}
		}

		/// <summary>Where Steam is installed, from the registry. Null off Windows.</summary>
		private static string SteamRoot()
		{
			if (!OperatingSystem.IsWindows())
			{
				return null;
			}

			// Per user first: it is the one Steam keeps current, and it is right even
			// when the machine has both a 32 and a 64 bit registry view of the other.
			foreach ((RegistryKey root, string subkey, string name) in new[]
			{
				(Registry.CurrentUser, @"Software\Valve\Steam", "SteamPath"),
				(Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Valve\Steam", "InstallPath"),
				(Registry.LocalMachine, @"SOFTWARE\Valve\Steam", "InstallPath")
			})
			{
				try
				{
					using RegistryKey key = root.OpenSubKey(subkey);
					if (key?.GetValue(name) is string path && Directory.Exists(path))
					{
						return path;
					}
				}
				catch (Exception)
				{
					// An unreadable key is not worth failing over; try the next.
				}
			}

			return null;
		}

		/// <summary>One "key" "value" pair out of an acf file.</summary>
		private static string Value(string text, string key)
		{
			Match match = Regex.Match(text, "\"" + Regex.Escape(key) + "\"\\s*\"([^\"]*)\"");
			return match.Success ? match.Groups[1].Value.Replace("\\\\", "\\") : null;
		}
	}
}
