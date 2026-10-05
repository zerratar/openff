// Every mod in place, for the start page's Mods tab: Crystal as a mod manager.
//
// OpenFF has a MODS list of its own on the title; the Steam and GOG games have none, so what was put into them is said
// here, where it was put in from. Two kinds of place:
//
//   - the OpenFF client's mods folder: every mod in it, as the client loads it - its name, version, whether it is on, and
//     what kind it is (a sample installed by OpenFF or Crystal, with its standing against the shipped sample; a project's
//     export, with the project it came from; anything else copied in). On and off write loadorder.json as the MODS list
//     does; Remove takes the folder out of the way (a sample installed is uninstalled; anything else goes to the
//     Recycle Bin, so a mod not of Crystal's making is never lost).
//   - each Steam or GOG copy of the games: the projects (and the edits made with no project open) that Project > Install
//     put into it, from the record each keeps beside its edits (<edits>.backup/installed.json, ModInstall); Uninstall puts
//     the originals back, as Project > Remove does. And, for a Steam copy, the files that differ from the Steam release
//     that no install of Crystal's accounts for - another mod tool's, or a damaged download - counted from the release's
//     list (OpenFF.Content.GameFiles), with Steam's Verify integrity of game files as the way back.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Crystal.Editor
{
	internal static class InstalledMods
	{
		public sealed class OpenFFMod
		{
			public string Key { get; set; }
			public string Name { get; set; }
			public string Version { get; set; }
			public string Author { get; set; }
			public string Description { get; set; }
			public string Directory { get; set; }
			public bool Enabled { get; set; }
			/// <summary>sample (installed from the samples), project (a project's export), or mod (anything else).</summary>
			public string Kind { get; set; }
			public string Sample { get; set; }
			/// <summary>For a sample: installed, update, copy, build (as Samples.Install says).</summary>
			public string State { get; set; }
			public bool Managed { get; set; }
			public string Project { get; set; }
			/// <summary>The project's folder, when it is on this machine.</summary>
			public string ProjectDirectory { get; set; }
			public bool Code { get; set; }
		}

		public sealed class GameInstall
		{
			public string Game { get; set; }
			public string Name { get; set; }
			public string Store { get; set; }
			public string Path { get; set; }
			public List<GameMod> Mods { get; set; } = new List<GameMod>();
			/// <summary>Files that differ from the Steam release with no install of Crystal's behind them; null when not checked.</summary>
			public List<string> Other { get; set; }
			public int Missing { get; set; }
			public bool OtherVersion { get; set; }
		}

		public sealed class GameMod
		{
			/// <summary>The project's name, or null for the edits made with no project open.</summary>
			public string Name { get; set; }
			public string ProjectDirectory { get; set; }
			public string Edits { get; set; }
			public int Files { get; set; }
			/// <summary>Of those, files the game has since that are not what was written (an update, a verify, another tool).</summary>
			public int Changed { get; set; }
		}

		// ------------------------------------------------------------------ OpenFF

		public static List<OpenFFMod> InOpenFF(string modsFolder)
		{
			var list = new List<OpenFFMod>();
			if (modsFolder == null || !Directory.Exists(modsFolder)) return list;
			string samples = Samples.Folder();
			List<Project> projects = Project.All();
			foreach (OpenFF.Content.InstalledMod mod in OpenFF.Content.ModsFolder.Load(modsFolder))
			{
				OpenFF.Content.ModManifest m = mod.Manifest;
				var item = new OpenFFMod
				{
					Key = mod.Key, Directory = mod.Directory, Enabled = mod.Enabled,
					Name = string.IsNullOrEmpty(m?.Name) ? mod.Key : m.Name, Version = m?.Version, Author = m?.Author, Description = m?.Description,
					Code = OpenFF.Content.ModsFolder.HasCode(mod), Kind = "mod",
				};
				OpenFF.Content.SampleMods.Status s = null;
				try { s = OpenFF.Content.SampleMods.StatusOf(mod.Directory, samples); } catch (Exception) { }
				if (s != null && s.State != OpenFF.Content.SampleMods.State.None)
				{
					item.Kind = "sample";
					item.Sample = s.Sample;
					item.Managed = s.Managed;
					item.State = s.State switch
					{
						OpenFF.Content.SampleMods.State.UpToDate => "installed",
						OpenFF.Content.SampleMods.State.UpdateAvailable => "update",
						OpenFF.Content.SampleMods.State.OldCopy => "copy",
						_ => "build",
					};
				}
				else
				{
					// A project's export: mod.json names it, or its folder is the one Export to OpenFF makes of a project here.
					Project from = projects.FirstOrDefault(p => string.Equals(p.File.Name, m?.Project, StringComparison.OrdinalIgnoreCase))
						?? projects.FirstOrDefault(p => p.File.Targets.Any(Targets.IsOurs) && string.Equals(ProjectExport.ModFolderName(p), mod.Key, StringComparison.OrdinalIgnoreCase));
					if (!string.IsNullOrEmpty(m?.Project) || from != null)
					{
						item.Kind = "project";
						item.Project = m?.Project ?? from?.File.Name;
						item.ProjectDirectory = from?.Directory;
					}
				}
				list.Add(item);
			}
			return list;
		}

		/// <summary>A mod on or off, as the MODS list does it: loadorder.json written, the order kept.</summary>
		public static void SetEnabled(string modsFolder, string key, bool enabled)
		{
			List<OpenFF.Content.InstalledMod> mods = OpenFF.Content.ModsFolder.Load(modsFolder);
			OpenFF.Content.InstalledMod mod = mods.FirstOrDefault(m => string.Equals(m.Key, key, StringComparison.OrdinalIgnoreCase))
				?? throw new ArgumentException("no mod called " + key + " in " + modsFolder);
			mod.Enabled = enabled;
			OpenFF.Content.ModsFolder.SaveOrder(modsFolder, mods);
		}

		// ------------------------------------------------------------- Steam, GOG

		public static List<GameInstall> Games(bool checkFiles)
		{
			var installs = new List<GameInstall>();
			var found = new List<(OpenFF.Content.SteamInstall Install, string Game)>();
			foreach (OpenFF.Content.SteamInstall i in OpenFF.Content.SteamInstalls.Find()) found.Add((i, "ff3"));
			foreach (OpenFF.Content.SteamInstall i in OpenFF.Content.SteamInstalls.Find(OpenFF.Content.SteamInstalls.Ff4AppId)) found.Add((i, "ff4"));
			List<(string Name, string ProjectDirectory, string Edits, string Content)> records = Records();
			foreach ((OpenFF.Content.SteamInstall install, string game) in found)
			{
				var g = new GameInstall { Game = game, Name = install.Name ?? (game == "ff4" ? "Final Fantasy IV" : "Final Fantasy III"), Store = install.Store, Path = install.Path };
				var ours = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				foreach (var r in records.Where(r => SamePath(r.Content, install.Path)))
				{
					List<InstalledFile> files = Recorded(r.Edits);
					if (files.Count == 0) continue;
					foreach (InstalledFile f in files) { ours.Add(f.Name); if (f.Container != null) ours.Add(f.Container); }
					var mod = new GameMod { Name = r.Name, ProjectDirectory = r.ProjectDirectory, Edits = r.Edits, Files = files.Count };
					try { mod.Changed = ModInstall.Status(new Workspace(install.Path, r.Edits)).Changed.Count; } catch (Exception) { }
					g.Mods.Add(mod);
				}
				if (checkFiles && string.Equals(install.Store, "Steam", StringComparison.OrdinalIgnoreCase))
				{
					string list = GameFileList(game);
					if (list != null)
					{
						try
						{
							OpenFF.Content.GameFiles.Result result = OpenFF.Content.GameFiles.Check(install.Path, OpenFF.Content.GameFiles.Read(list),
								// The client's own cache of the check (sizes, times, hashes): what OpenFF has read already is not read again.
								System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OpenFF", "game-files-" + game + ".json"));
							g.Other = result.Changed.Where(f => !ours.Contains(f) && !ours.Contains(f.Replace('\\', '/'))).ToList();
							g.Missing = result.Missing.Count;
							g.OtherVersion = result.OtherVersion;
						}
						catch (Exception ex) { Console.Error.WriteLine("  mods      the game files of {0}: {1}", install.Path, ex.Message); }
					}
				}
				installs.Add(g);
			}
			return installs;
		}

		/// <summary>An install taken back out: the originals put back, as Project > Remove does.</summary>
		public static ModResult Uninstall(string content, string edits)
		{
			if (!Records().Any(r => SamePath(r.Edits, edits))) throw new ArgumentException(edits + " is not the edits of a project or of Crystal's");
			return ModInstall.Uninstall(new Workspace(content, edits));
		}

		/// <summary>Every place Crystal keeps edits for a Steam or GOG copy: each project's games of that kind, and the edits made with no project.</summary>
		private static List<(string Name, string ProjectDirectory, string Edits, string Content)> Records()
		{
			var records = new List<(string, string, string, string)>();
			foreach (Project p in Project.All())
				foreach (string target in p.File.Targets.Where(t => !Targets.IsOurs(t)))
				{
					string content = null;
					try { content = p.ContentDirectoryFor(target); } catch (Exception) { }
					if (content != null) records.Add((p.File.Name, p.Directory, p.FilesFor(target), content));
				}
			// The edits made with no project open: CrystalHome.Mods/<the game's folder name>, for the copy of that name.
			if (Directory.Exists(CrystalHome.Mods))
				foreach (OpenFF.Content.SteamInstall i in OpenFF.Content.SteamInstalls.Find().Concat(OpenFF.Content.SteamInstalls.Find(OpenFF.Content.SteamInstalls.Ff4AppId)))
				{
					string edits = System.IO.Path.Combine(CrystalHome.Mods, System.IO.Path.GetFileName(i.Path.TrimEnd(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar)));
					if (Directory.Exists(edits)) records.Add((null, null, edits, i.Path));
				}
			return records;
		}

		private static List<InstalledFile> Recorded(string edits)
		{
			string path = System.IO.Path.Combine(edits.TrimEnd(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar) + ".backup", "installed.json");
			try { return File.Exists(path) ? JsonSerializer.Deserialize<List<InstalledFile>>(File.ReadAllText(path))?.Where(f => f?.Name != null).ToList() ?? new List<InstalledFile>() : new List<InstalledFile>(); }
			catch (Exception) { return new List<InstalledFile>(); }
		}

		/// <summary>The release's list of a game's files (Data/game-files beside the client and Crystal, or a checkout's).</summary>
		private static string GameFileList(string game)
		{
			string name = System.IO.Path.Combine("Data", "game-files", game + "-steam.json.gz");
			for (string at = AppContext.BaseDirectory; !string.IsNullOrEmpty(at); at = System.IO.Path.GetDirectoryName(at.TrimEnd(System.IO.Path.DirectorySeparatorChar)))
			{
				foreach (string candidate in new[] { System.IO.Path.Combine(at, name), System.IO.Path.Combine(at, "OpenFF", name) })
					if (File.Exists(candidate)) return candidate;
			}
			return null;
		}

		private static bool SamePath(string a, string b)
		{
			if (a == null || b == null) return false;
			string Norm(string p) => System.IO.Path.GetFullPath(p).TrimEnd(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);
			try { return string.Equals(Norm(a), Norm(b), StringComparison.OrdinalIgnoreCase); } catch (Exception) { return false; }
		}
	}
}
