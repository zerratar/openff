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
// source, and mixed content is the point (Karl, 2026-09-04), so every enabled OpenFF mod
// applies whichever game's assets are in front.
//
// Shared by the client (which loads) and the editor (which writes and, later, lists).

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FF3.Content
{
	/// <summary>What mod.json holds.</summary>
	internal sealed class ModManifest
	{
		public const string TargetOpenFF = "openff";

		[JsonPropertyName("name")] public string Name { get; set; }
		[JsonPropertyName("version")] public string Version { get; set; }
		[JsonPropertyName("author")] public string Author { get; set; }
		[JsonPropertyName("description")] public string Description { get; set; }

		/// <summary>"openff" (default), "steam" (FF3 on Steam) or "ff4steam".</summary>
		[JsonPropertyName("target")] public string Target { get; set; } = TargetOpenFF;

		/// <summary>The subfolder with the game-named files; "files" unless said otherwise.</summary>
		[JsonPropertyName("files")] public string Files { get; set; } = "files";

		[JsonIgnore]
		public bool ForOpenFF => string.IsNullOrEmpty(Target) || string.Equals(Target, TargetOpenFF, StringComparison.OrdinalIgnoreCase);
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
		public string DisplayName => string.IsNullOrWhiteSpace(Manifest?.Name) ? Key : Manifest.Name;
	}

	internal static class ModsFolder
	{
		public const string FolderName = "mods";
		public const string ManifestName = "mod.json";
		public const string LoadOrderName = "loadorder.json";

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
		/// The mods the client should put in front of the shipped content, in load order:
		/// enabled, targeting OpenFF, with a files folder.
		/// </summary>
		public static List<InstalledMod> Active(IEnumerable<InstalledMod> mods)
		{
			return mods.Where(m => m.Enabled && m.Manifest.ForOpenFF && Directory.Exists(m.FilesDirectory)).ToList();
		}

		/// <summary>
		/// Files more than one active mod carries, with the mods that carry them in load
		/// order (the first wins). Names are the game's, with forward slashes.
		/// </summary>
		public static Dictionary<string, List<InstalledMod>> Conflicts(IEnumerable<InstalledMod> active)
		{
			Dictionary<string, List<InstalledMod>> owners = new Dictionary<string, List<InstalledMod>>(StringComparer.OrdinalIgnoreCase);
			foreach (InstalledMod mod in active)
			{
				string root = mod.FilesDirectory;
				if (!Directory.Exists(root))
				{
					continue;
				}
				foreach (string file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
				{
					string name = Path.GetRelativePath(root, file).Replace('\\', '/');
					if (!owners.TryGetValue(name, out List<InstalledMod> list))
					{
						owners[name] = list = new List<InstalledMod>();
					}
					list.Add(mod);
				}
			}
			return owners.Where(pair => pair.Value.Count > 1).ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);
		}

		public static int FileCount(InstalledMod mod)
		{
			return Directory.Exists(mod.FilesDirectory) ? Directory.EnumerateFiles(mod.FilesDirectory, "*", SearchOption.AllDirectories).Count() : 0;
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
