// Where the OpenFF client is, so Crystal can write mods straight into its mods folder.
//
// The client records its own location every time it runs, in the same settings file
// that remembers which game to start (%LocalAppData%\OpenFF\launch.json, written by the
// client's Compat/Launch.cs). That is the first answer. When the client has never run
// on this machine, a development checkout is the second: the OpenFF build beside this
// repository.

using System;
using System.IO;
using System.Text.Json;

namespace Crystal.Editor
{
	internal static class OpenFFClient
	{
		public static string SettingsPath => Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OpenFF", "launch.json");

		/// <summary>The client's mods folder (it need not exist yet), or null when no client is known.</summary>
		public static string ModsFolder()
		{
			string recorded = Recorded();
			if (recorded != null)
			{
				return recorded;
			}
			string executable = DevelopmentBuild();
			return executable != null ? Path.Combine(Path.GetDirectoryName(executable), OpenFF.Content.ModsFolder.FolderName) : null;
		}

		/// <summary>The client's OpenFF.Engine.dll (beside its executable), or null when no client is known or built.</summary>
		public static string EngineAssembly()
		{
			string mods = ModsFolder();
			if (mods != null)
			{
				string beside = Path.Combine(Path.GetDirectoryName(mods), "OpenFF.Engine.dll");
				if (File.Exists(beside))
				{
					return beside;
				}
			}
			// A development checkout: the engine's own build.
			foreach (string start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
			{
				DirectoryInfo directory = new DirectoryInfo(start);
				for (int up = 0; up < 6 && directory != null; up++, directory = directory.Parent)
				{
					foreach (string configuration in new[] { "Debug", "Release" })
					{
						string candidate = Path.Combine(directory.FullName, "OpenFF.Engine", "bin", configuration, "net8.0", "OpenFF.Engine.dll");
						if (File.Exists(candidate))
						{
							return candidate;
						}
					}
				}
			}
			return null;
		}

		/// <summary>The client's OpenFF.exe: the one that last ran (launch.json), else the development build; null when neither is found.</summary>
		public static string Executable()
		{
			try
			{
				if (File.Exists(SettingsPath))
				{
					using JsonDocument document = JsonDocument.Parse(File.ReadAllText(SettingsPath));
					if (document.RootElement.TryGetProperty("exe", out JsonElement exe) && exe.ValueKind == JsonValueKind.String && File.Exists(exe.GetString()))
					{
						return exe.GetString();
					}
				}
			}
			catch (Exception)
			{
			}
			return DevelopmentBuild();
		}

		/// <summary>Whether the client is running now (by process name, on this machine).</summary>
		public static bool IsRunning()
		{
			try { return System.Diagnostics.Process.GetProcessesByName("FF3").Length > 0; }
			catch (Exception) { return false; }
		}

		/// <summary>
		/// Starts the client from its own folder, with switches when given (--game=ff3
		/// --map=d01_05 --pos=x,y,z for "play here"). Throws when there is none.
		/// </summary>
		public static void Launch(params string[] arguments)
		{
			string exe = Executable();
			if (exe == null)
			{
				throw new InvalidOperationException("the OpenFF client was not found - start OpenFF.exe once (it records where it is), or build OpenFF beside this repository");
			}
			System.Diagnostics.ProcessStartInfo start = new System.Diagnostics.ProcessStartInfo(exe)
			{
				UseShellExecute = true,
				WorkingDirectory = Path.GetDirectoryName(exe),
			};
			foreach (string argument in arguments ?? Array.Empty<string>())
			{
				if (!string.IsNullOrWhiteSpace(argument)) start.ArgumentList.Add(argument);
			}
			System.Diagnostics.Process.Start(start);
		}

		private static string Recorded()
		{
			try
			{
				if (!File.Exists(SettingsPath))
				{
					return null;
				}
				using JsonDocument document = JsonDocument.Parse(File.ReadAllText(SettingsPath));
				if (document.RootElement.TryGetProperty("mods", out JsonElement mods) && mods.ValueKind == JsonValueKind.String)
				{
					string folder = mods.GetString();
					// The client that wrote it may have moved; its parent must still be there.
					if (!string.IsNullOrEmpty(folder) && Directory.Exists(Path.GetDirectoryName(folder)))
					{
						return folder;
					}
				}
			}
			catch (Exception)
			{
			}
			return null;
		}

		/// <summary>OpenFF/bin/{Debug,Release}/net8.0/OpenFF.exe, looked for upward from where Crystal runs.</summary>
		private static string DevelopmentBuild()
		{
			foreach (string start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
			{
				DirectoryInfo directory = new DirectoryInfo(start);
				for (int up = 0; up < 6 && directory != null; up++, directory = directory.Parent)
				{
					foreach (string configuration in new[] { "Debug", "Release" })
					{
						string candidate = Path.Combine(directory.FullName, "OpenFF", "bin", configuration, "net8.0", "OpenFF.exe");
						if (File.Exists(candidate))
						{
							return candidate;
						}
					}
				}
			}
			return null;
		}
	}
}
