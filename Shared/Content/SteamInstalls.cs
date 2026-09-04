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

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace FF3.Content
{
	internal sealed class SteamInstall
	{
		/// <summary>The directory holding the executable and the files folder.</summary>
		public string Path { get; set; }

		/// <summary>What Steam calls it - "Final Fantasy III (3D Remake)".</summary>
		public string Name { get; set; }

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

			return found;
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
