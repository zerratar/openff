// The mods folder: mods/ beside the OpenFF executable, one mod per subfolder.
//
// A mod for OpenFF is a folder that mirrors the game's file names under files/, with a
// mod.json saying what it is. The client finds every such folder at start, puts the
// enabled ones in front of the shipped content in the order mods/loadorder.json gives
// (first wins on a file both carry), and writes that order back so a folder dropped in
// by hand is picked up, enabled, at the end. Crystal's "Export to OpenFF" writes a mod
// here straight from a project.
//
// Mods targeting the Steam builds are a different thing: those clients know nothing of
// a mods folder, so a Steam mod is installed by replacing the game's files (Crystal's
// ModInstall, with its backup). A mod.json can say "steam" or "ff4steam" as its target
// so Crystal knows; the OpenFF client only loads mods targeting "openff".
//
// An OpenFF mod is not for one game. Under OpenFF the game that was booted is an asset
// source, and mixed content is the point (direction, 2026-09-04), so every enabled OpenFF mod
// applies whichever game's assets are in front. The two games name their files alike,
// though - d01_05.script is a different map in each - so a mod keeps edits meant for one
// game apart: ff3/files/ applies when FF3 was booted, ff4/files/ when FF4 was, and
// files/ whichever it was (2026-09-07). Crystal writes the first two; a hand-made mod
// with only files/ reads as it always did.
//
// Shared by the client (which loads) and the editor (which writes and, later, lists).

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenFF.Content
{
	/// <summary>What mod.json holds.</summary>
	internal sealed class ModManifest
	{
		public const string TargetOpenFF = "openff";

		/// <summary>What other mods refer to; the folder's name when not given.</summary>
		[JsonPropertyName("id")] public string Id { get; set; }
		[JsonPropertyName("name")] public string Name { get; set; }
		[JsonPropertyName("version")] public string Version { get; set; }
		[JsonPropertyName("author")] public string Author { get; set; }
		[JsonPropertyName("description")] public string Description { get; set; }

		/// <summary>"openff" (default), "steam" (FF3 on Steam) or "ff4steam".</summary>
		[JsonPropertyName("target")] public string Target { get; set; } = TargetOpenFF;

		/// <summary>The subfolder with the game-named files; "files" unless said otherwise.</summary>
		[JsonPropertyName("files")] public string Files { get; set; } = "files";

		/// <summary>The mod's code: assembly files relative to the mod folder. Empty means every .dll at the mod's root.</summary>
		[JsonPropertyName("assemblies")] public List<string> Assemblies { get; set; } = new List<string>();

		/// <summary>The subfolder with scene files (behaviours attached to map objects, from Crystal); "scenes" unless said otherwise.</summary>
		[JsonPropertyName("scenes")] public string Scenes { get; set; } = "scenes";

		/// <summary>Mods this one needs enabled and loaded before it. Most mods have none.</summary>
		[JsonPropertyName("dependencies")] public List<ModDependency> Dependencies { get; set; } = new List<ModDependency>();

		[JsonIgnore]
		public bool ForOpenFF => string.IsNullOrEmpty(Target) || string.Equals(Target, TargetOpenFF, StringComparison.OrdinalIgnoreCase);
	}

	/// <summary>One dependency: another mod's id and, optionally, the lowest version that will do.</summary>
	internal sealed class ModDependency
	{
		[JsonPropertyName("id")] public string Id { get; set; }
		[JsonPropertyName("minVersion")] public string MinVersion { get; set; }
	}

	/// <summary>One entry of loadorder.json.</summary>
	internal sealed class LoadOrderEntry
	{
		[JsonPropertyName("name")] public string Name { get; set; }
		[JsonPropertyName("enabled")] public bool Enabled { get; set; } = true;
	}

	internal sealed class LoadOrder
	{
		[JsonPropertyName("mods")] public List<LoadOrderEntry> Mods { get; set; } = new List<LoadOrderEntry>();
	}

	/// <summary>A mod as found in the folder: where it is, what it says, whether it is on.</summary>
	internal sealed class InstalledMod
	{
		public string Directory { get; set; }
		/// <summary>The folder's name, which is what loadorder.json refers to.</summary>
		public string Key { get; set; }
		public ModManifest Manifest { get; set; }
		public bool Enabled { get; set; }
		public string FilesDirectory => Path.Combine(Directory, string.IsNullOrEmpty(Manifest?.Files) ? "files" : Manifest.Files);

		/// <summary>The folder with the files meant for one game only: &lt;mod&gt;/ff3/files or &lt;mod&gt;/ff4/files.</summary>
		public string FilesDirectoryFor(string game) => Path.Combine(Directory, game, "files");

		/// <summary>
		/// Every files folder the mod has, the booted game's first (it is the more specific
		/// and wins), then the one for either game. With game null, all of them.
		/// </summary>
		public IEnumerable<string> FilesDirectories(string game)
		{
			foreach (string g in game == null ? ModsFolder.Games : new[] { game })
			{
				string mine = FilesDirectoryFor(g);
				if (System.IO.Directory.Exists(mine)) yield return mine;
			}
			if (System.IO.Directory.Exists(FilesDirectory)) yield return FilesDirectory;
		}

		public string DisplayName => string.IsNullOrWhiteSpace(Manifest?.Name) ? Key : Manifest.Name;
		/// <summary>The id other mods depend on: mod.json's, or the folder's name.</summary>
		public string Id => string.IsNullOrWhiteSpace(Manifest?.Id) ? Key : Manifest.Id.Trim();
		/// <summary>Why the mod is not active, when it is not: "disabled", "targets steam", "needs x", ...; null when it is.</summary>
		public string Skipped { get; set; }
	}

	internal static class ModsFolder
	{
		public const string FolderName = "mods";
		public const string ManifestName = "mod.json";
		public const string LoadOrderName = "loadorder.json";

		/// <summary>The games a mod may keep a files folder for, as the folder names: ff3/files, ff4/files.</summary>
		public static readonly string[] Games = { "ff3", "ff4" };

		private static readonly JsonSerializerOptions Json = new JsonSerializerOptions
		{
			WriteIndented = true,
			PropertyNameCaseInsensitive = true,
			ReadCommentHandling = JsonCommentHandling.Skip,
			AllowTrailingCommas = true,
		};

		/// <summary>mods/ beside an executable (or any directory).</summary>
		public static string Beside(string executableDirectory)
		{
			return Path.Combine(executableDirectory, FolderName);
		}

		/// <summary>
		/// Every mod in the folder, in load order: the ones loadorder.json names first, in
		/// its order and with its enabled flags, then any folder it does not name, enabled,
		/// alphabetically. A folder without a mod.json still counts (a hand-made mod), with
		/// its folder name as its name. Returns an empty list when there is no folder.
		/// </summary>
		public static List<InstalledMod> Load(string modsDirectory)
		{
			List<InstalledMod> found = new List<InstalledMod>();
			if (string.IsNullOrEmpty(modsDirectory) || !Directory.Exists(modsDirectory))
			{
				return found;
			}
			foreach (string directory in Directory.EnumerateDirectories(modsDirectory).OrderBy(d => d, StringComparer.OrdinalIgnoreCase))
			{
				string key = Path.GetFileName(directory);
				if (key.StartsWith(".", StringComparison.Ordinal))
				{
					continue;
				}
				ModManifest manifest = ReadManifest(Path.Combine(directory, ManifestName)) ?? new ModManifest { Name = key };
				found.Add(new InstalledMod { Directory = directory, Key = key, Manifest = manifest, Enabled = true });
			}

			LoadOrder order = ReadOrder(Path.Combine(modsDirectory, LoadOrderName));
			List<InstalledMod> ordered = new List<InstalledMod>();
			foreach (LoadOrderEntry entry in order.Mods)
			{
				InstalledMod mod = found.FirstOrDefault(m => string.Equals(m.Key, entry.Name, StringComparison.OrdinalIgnoreCase));
				if (mod != null && !ordered.Contains(mod))
				{
					mod.Enabled = entry.Enabled;
					ordered.Add(mod);
				}
			}
			ordered.AddRange(found.Where(m => !ordered.Contains(m)));
			return ordered;
		}

		/// <summary>Writes loadorder.json for the list as it stands (order and enabled flags).</summary>
		public static void SaveOrder(string modsDirectory, IEnumerable<InstalledMod> mods)
		{
			LoadOrder order = new LoadOrder
			{
				Mods = mods.Select(m => new LoadOrderEntry { Name = m.Key, Enabled = m.Enabled }).ToList(),
			};
			Directory.CreateDirectory(modsDirectory);
			File.WriteAllText(Path.Combine(modsDirectory, LoadOrderName), JsonSerializer.Serialize(order, Json));
		}

		/// <summary>
		/// The mods the client should load, in load order: enabled, targeting OpenFF, with
		/// their dependencies active and earlier in the order (a mod whose dependency is
		/// missing, disabled, too old or later in the order is left out, and says why in
		/// Skipped). An assets-only mod needs a files folder; a code-only mod needs none.
		/// </summary>
		public static List<InstalledMod> Active(IEnumerable<InstalledMod> mods)
		{
			List<InstalledMod> active = new List<InstalledMod>();
			foreach (InstalledMod mod in mods)
			{
				mod.Skipped = null;
				if (!mod.Enabled)
				{
					mod.Skipped = "disabled";
				}
				else if (!mod.Manifest.ForOpenFF)
				{
					mod.Skipped = "targets " + mod.Manifest.Target;
				}
				else if (!mod.FilesDirectories(null).Any() && !HasCode(mod) && !HasContent(mod))
				{
					mod.Skipped = "no files folder, no scenes, definitions or menus, and no code";
				}
				else
				{
					foreach (ModDependency dependency in mod.Manifest.Dependencies ?? new List<ModDependency>())
					{
						if (string.IsNullOrWhiteSpace(dependency?.Id))
						{
							continue;
						}
						InstalledMod found = active.FirstOrDefault(a => string.Equals(a.Id, dependency.Id, StringComparison.OrdinalIgnoreCase));
						if (found == null)
						{
							InstalledMod anywhere = mods.FirstOrDefault(a => string.Equals(a.Id, dependency.Id, StringComparison.OrdinalIgnoreCase));
							mod.Skipped = "needs " + dependency.Id + (anywhere == null ? ", which is not installed"
								: anywhere.Skipped != null ? ", which is " + anywhere.Skipped
								: ", which must come before it in the load order");
							break;
						}
						if (!string.IsNullOrWhiteSpace(dependency.MinVersion) && CompareVersions(found.Manifest.Version, dependency.MinVersion) < 0)
						{
							mod.Skipped = "needs " + dependency.Id + " " + dependency.MinVersion + " or later, has " + (found.Manifest.Version ?? "no version");
							break;
						}
					}
				}
				if (mod.Skipped == null)
				{
					active.Add(mod);
				}
			}
			return active;
		}

		/// <summary>Whether a mod brings code: assemblies named in mod.json, or a .dll at its root.</summary>
		/// <summary>Whether the mod carries scenes or definitions - a mod of maps, items, monsters, text, with no game files and no code (the Showcase sample is one).</summary>
		public static bool HasContent(InstalledMod mod)
		{
			if (!Directory.Exists(mod.Directory)) return false;
			string scenes = Path.Combine(mod.Directory, string.IsNullOrWhiteSpace(mod.Manifest.Scenes) ? "scenes" : mod.Manifest.Scenes);
			if (Directory.Exists(scenes) && Directory.EnumerateFiles(scenes, "*.json", SearchOption.TopDirectoryOnly).Any()) return true;
			string defs = Path.Combine(mod.Directory, "defs");
			if (Directory.Exists(defs) && Directory.EnumerateFiles(defs, "*.json", SearchOption.AllDirectories).Any()) return true;
			string menus = Path.Combine(mod.Directory, "menus");
			return Directory.Exists(menus) && Directory.EnumerateFiles(menus, "*.json", SearchOption.TopDirectoryOnly).Any();
		}

		public static bool HasCode(InstalledMod mod)
		{
			if (mod.Manifest.Assemblies != null && mod.Manifest.Assemblies.Count > 0)
			{
				return true;
			}
			return Directory.Exists(mod.Directory) && Directory.EnumerateFiles(mod.Directory, "*.dll", SearchOption.TopDirectoryOnly).Any();
		}

		/// <summary>Dotted versions compared number by number; an unparseable part compares as text.</summary>
		public static int CompareVersions(string a, string b)
		{
			string[] pa = (a ?? "0").Split('.'), pb = (b ?? "0").Split('.');
			for (int i = 0; i < Math.Max(pa.Length, pb.Length); i++)
			{
				string sa = i < pa.Length ? pa[i].Trim() : "0", sb = i < pb.Length ? pb[i].Trim() : "0";
				int c = int.TryParse(sa, out int na) && int.TryParse(sb, out int nb) ? na.CompareTo(nb) : string.Compare(sa, sb, StringComparison.OrdinalIgnoreCase);
				if (c != 0)
				{
					return c;
				}
			}
			return 0;
		}

		/// <summary>
		/// Files more than one active mod carries, with the mods that carry them in load
		/// order (the first wins). Names are the game's, with forward slashes. For one game
		/// when given, since a name in ff3/files and the same in ff4/files never meet.
		/// </summary>
		public static Dictionary<string, List<InstalledMod>> Conflicts(IEnumerable<InstalledMod> active, string game = null)
		{
			Dictionary<string, List<InstalledMod>> owners = new Dictionary<string, List<InstalledMod>>(StringComparer.OrdinalIgnoreCase);
			foreach (InstalledMod mod in active)
			{
				HashSet<string> mine = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				foreach (string root in mod.FilesDirectories(game))
				{
					foreach (string file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
					{
						string name = Path.GetRelativePath(root, file).Replace('\\', '/');
						if (!mine.Add(name))
						{
							continue; // the same name in the mod's own ff3/files and files/ is not a conflict
						}
						if (!owners.TryGetValue(name, out List<InstalledMod> list))
						{
							owners[name] = list = new List<InstalledMod>();
						}
						list.Add(mod);
					}
				}
			}
			return owners.Where(pair => pair.Value.Count > 1).ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);
		}

		/// <summary>How many files the mod carries, over every folder (or one game's and the shared one).</summary>
		public static int FileCount(InstalledMod mod, string game = null)
		{
			return mod.FilesDirectories(game).Sum(root => Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories).Count());
		}

		public static ModManifest ReadManifest(string path)
		{
			try
			{
				if (!File.Exists(path))
				{
					return null;
				}
				return JsonSerializer.Deserialize<ModManifest>(File.ReadAllText(path), Json);
			}
			catch (Exception)
			{
				return null;
			}
		}

		public static void WriteManifest(string path, ModManifest manifest)
		{
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			File.WriteAllText(path, JsonSerializer.Serialize(manifest, Json));
		}

		private static LoadOrder ReadOrder(string path)
		{
			try
			{
				if (File.Exists(path))
				{
					return JsonSerializer.Deserialize<LoadOrder>(File.ReadAllText(path), Json) ?? new LoadOrder();
				}
			}
			catch (Exception)
			{
			}
			return new LoadOrder();
		}
	}
}
