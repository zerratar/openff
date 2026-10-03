// The sample mods as installed mods: a sample (Samples/<id>, shipped beside the client and Crystal) copied into the mods
// folder as it stands, kept in step with the sample the client ships - the managed kind of mod, as against a project of
// the player's own (Crystal's, made from a sample or from nothing), which nothing ever overwrites.
//
// An install copies the sample's mod files - everything but its sources, tools, checks and build output: mod.json, the
// built assembly (the release's Samples carry each one's, built against that release's engine), menus, data, defs,
// scenes, assets, game files - and writes sample.json beside them: which sample, its version, and each file's SHA-1 as
// installed. A newer client's sample that differs from that record is an update; a file whose SHA-1 no longer matches the
// record is one the player changed, said before an update replaces it. A mod copied by hand from Samples (no record, the
// sample's folder name) is the sample's too, an older copy, and is offered the update with its differing files named.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenFF.Content
{
	internal static class SampleMods
	{
		public const string RecordName = "sample.json";

		/// <summary>What sample.json says: the sample, its version, when, and each installed file's SHA-1.</summary>
		public sealed class Record
		{
			[JsonPropertyName("sample")] public string Sample { get; set; }
			[JsonPropertyName("version")] public string Version { get; set; }
			[JsonPropertyName("installed")] public string Installed { get; set; }
			[JsonPropertyName("files")] public Dictionary<string, string> Files { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		}

		public enum State
		{
			/// <summary>Not a sample's mod.</summary>
			None,
			/// <summary>The sample as the client ships it.</summary>
			UpToDate,
			/// <summary>The shipped sample differs from what is installed: an update.</summary>
			UpdateAvailable,
			/// <summary>A copy made by hand of a sample (no record) that differs from the shipped one: an update, its changed files named.</summary>
			OldCopy,
			/// <summary>A sample with code whose shipped folder has no built assembly (a source checkout): it cannot be installed or updated from here.</summary>
			NeedsBuild,
		}

		public sealed class Status
		{
			public State State { get; set; }
			public string Sample { get; set; }
			public string Version { get; set; }
			public string Installed { get; set; }
			/// <summary>Files the player changed since the install (or, for a copy made by hand, those that differ from the sample): an update replaces them.</summary>
			public List<string> Changed { get; } = new List<string>();
			/// <summary>For a copy made by hand: files the shipped sample does not have (an older sample's) - an update takes them away.</summary>
			public List<string> Extra { get; } = new List<string>();
			public bool Managed { get; set; }
		}

		private static readonly JsonSerializerOptions Json = new JsonSerializerOptions { WriteIndented = true };
		private static readonly string[] SkipFolders = { "bin", "obj", "Tests", "tools", "code", "src" };
		private static readonly string[] SkipExtensions = { ".cs", ".csproj", ".sln", ".user", ".cmd", ".bat", ".py", ".md", ".ps1" };

		/// <summary>The samples folder: beside the program (a release), else up from it to a checkout's Samples.</summary>
		public static string Folder(string programDirectory)
		{
			string at = programDirectory;
			for (int i = 0; i < 7 && !string.IsNullOrEmpty(at); i++)
			{
				string candidate = Path.Combine(at, "Samples");
				if (Directory.Exists(candidate) && Directory.EnumerateFiles(candidate, "mod.json", SearchOption.AllDirectories).Any()) return candidate;
				at = Path.GetDirectoryName(at.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
			}
			return null;
		}

		/// <summary>The samples there are, by id (their folder's name).</summary>
		public static Dictionary<string, string> All(string samplesFolder)
		{
			var all = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			if (samplesFolder == null || !Directory.Exists(samplesFolder)) return all;
			foreach (string dir in Directory.EnumerateDirectories(samplesFolder))
				if (File.Exists(Path.Combine(dir, "mod.json"))) all[Path.GetFileName(dir)] = dir;
			return all;
		}

		/// <summary>Whether a sample has code (a csproj) and, if so, its built assembly beside its mod.json.</summary>
		public static bool HasCode(string sampleDir) => Directory.EnumerateFiles(sampleDir, "*.csproj", SearchOption.TopDirectoryOnly).Any();
		public static bool Built(string sampleDir)
		{
			if (!HasCode(sampleDir)) return true;
			Dictionary<string, string> sources = Sources(sampleDir);
			List<string> assemblies = Assemblies(sampleDir);
			return assemblies.Count > 0 ? assemblies.All(sources.ContainsKey) : sources.Keys.Any(f => f.EndsWith(".dll", StringComparison.OrdinalIgnoreCase));
		}

		/// <summary>The assemblies the sample's mod.json names.</summary>
		private static List<string> Assemblies(string sampleDir)
		{
			try { return ModsFolder.ReadManifest(Path.Combine(sampleDir, "mod.json"))?.Assemblies ?? new List<string>(); }
			catch (Exception) { return new List<string>(); }
		}

		/// <summary>
		/// The files an installed mod is made of, each with where it is read from: the sample's own (Files), and its
		/// assemblies - beside mod.json in a release, else a checkout's newest build of it (bin/&lt;configuration&gt;/&lt;framework&gt;/).
		/// </summary>
		public static Dictionary<string, string> Sources(string sampleDir)
		{
			var map = Files(sampleDir).ToDictionary(f => f, f => Path.Combine(sampleDir, f), StringComparer.OrdinalIgnoreCase);
			string bin = Path.Combine(sampleDir, "bin");
			if (!Directory.Exists(bin)) return map;
			foreach (string assembly in Assemblies(sampleDir))
			{
				if (map.ContainsKey(assembly)) continue;
				string built = Directory.EnumerateFiles(bin, Path.GetFileName(assembly), SearchOption.AllDirectories)
					.OrderByDescending(File.GetLastWriteTimeUtc).FirstOrDefault();
				if (built != null) map[assembly] = built;
			}
			return map;
		}

		/// <summary>The sample's files an installed mod is made of, by path relative to the sample (forward slashes).</summary>
		public static List<string> Files(string sampleDir)
		{
			var files = new List<string>();
			foreach (string file in Directory.EnumerateFiles(sampleDir, "*", SearchOption.AllDirectories))
			{
				string relative = Path.GetRelativePath(sampleDir, file).Replace('\\', '/');
				string[] parts = relative.Split('/');
				if (parts.Take(parts.Length - 1).Any(p => SkipFolders.Contains(p, StringComparer.OrdinalIgnoreCase))) continue;
				if (SkipExtensions.Contains(Path.GetExtension(relative), StringComparer.OrdinalIgnoreCase)) continue;
				if (string.Equals(parts[^1], RecordName, StringComparison.OrdinalIgnoreCase)) continue;
				// The sample's picture for Crystal's sample browser is not the mod's.
				if (parts.Length == 1 && Path.GetFileNameWithoutExtension(relative).Equals("preview", StringComparison.OrdinalIgnoreCase)) continue;
				files.Add(relative);
			}
			files.Sort(StringComparer.OrdinalIgnoreCase);
			return files;
		}

		public static string VersionOf(string modDir)
		{
			try { return ModsFolder.ReadManifest(Path.Combine(modDir, "mod.json"))?.Version; }
			catch (Exception) { return null; }
		}

		public static Record ReadRecord(string modDir)
		{
			string path = Path.Combine(modDir, RecordName);
			try { return File.Exists(path) ? JsonSerializer.Deserialize<Record>(File.ReadAllText(path), Json) : null; }
			catch (Exception) { return null; }
		}

		/// <summary>An installed mod against the samples shipped: whether it is one, and whether it is the shipped one.</summary>
		public static Status StatusOf(string modDir, string samplesFolder)
		{
			Status status = new Status { State = State.None };
			Record record = ReadRecord(modDir);
			// A project's export (Crystal's: mod.json names the project, or - before it did - its README beside it) is the player's own.
			if (record == null)
			{
				ModManifest manifest = null;
				try { manifest = ModsFolder.ReadManifest(Path.Combine(modDir, "mod.json")); } catch (Exception) { }
				if (!string.IsNullOrEmpty(manifest?.Project) || CrystalReadme(Path.Combine(modDir, "README.md"))) return status;
			}
			string id = record?.Sample ?? Path.GetFileName(modDir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
			if (!All(samplesFolder).TryGetValue(id, out string sampleDir)) return status;
			status.Sample = id;
			status.Managed = record != null;
			status.Version = VersionOf(sampleDir);
			status.Installed = record?.Version ?? VersionOf(modDir);
			if (!Built(sampleDir)) { status.State = State.NeedsBuild; return status; }
			bool differs = false;
			Dictionary<string, string> sources = Sources(sampleDir);
			List<string> shippedFiles = sources.Keys.ToList();
			if (record == null)
			{
				// What a copy made by hand has that the sample does not: an older sample's files (its own record tells a managed one's).
				var shippedSet = new HashSet<string>(shippedFiles, StringComparer.OrdinalIgnoreCase);
				foreach (string relative in Files(modDir))
					if (!shippedSet.Contains(relative)) { status.Extra.Add(relative); differs = true; }
			}
			foreach (string relative in shippedFiles)
			{
				string installed = Path.Combine(modDir, relative);
				string shipped = Sha1(sources[relative]);
				if (!File.Exists(installed)) { differs = true; continue; }
				string now = Sha1(installed);
				if (now != shipped) differs = true;
				// A file of the player's: changed since the install (by the record), or for a copy made by hand any that differs.
				if (record != null ? (record.Files.TryGetValue(relative, out string was) && was != now) : now != shipped) status.Changed.Add(relative);
			}
			status.State = !differs ? State.UpToDate : record != null ? State.UpdateAvailable : State.OldCopy;
			return status;
		}

		/// <summary>
		/// The sample installed, or brought up to the shipped one: its files over the mod's, the files an earlier install
		/// put there that the sample no longer has taken away, and the record written. Files of the player's own that no
		/// install put there are left as they are. Returns the files written.
		/// </summary>
		public static int Install(string sampleDir, string modsFolder, bool removeExtra = false)
		{
			if (!Built(sampleDir)) throw new InvalidOperationException(Path.GetFileName(sampleDir) + " has code and no built assembly beside it - build it first (Crystal, or the sample's install.cmd)");
			string id = Path.GetFileName(sampleDir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
			string modDir = Path.Combine(modsFolder, id);
			Record old = ReadRecord(modDir);
			Record record = new Record { Sample = id, Version = VersionOf(sampleDir), Installed = DateTime.Now.ToString("yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture) };
			int written = 0;
			foreach ((string relative, string from) in Sources(sampleDir))
			{
				string to = Path.Combine(modDir, relative);
				string sha = Sha1(from);
				record.Files[relative] = sha;
				if (File.Exists(to) && Sha1(to) == sha) continue;
				Directory.CreateDirectory(Path.GetDirectoryName(to));
				File.Copy(from, to, overwrite: true);
				written++;
			}
			// What an earlier install put there that the sample no longer has; for a copy made by hand (removeExtra), every
			// mod file of it the sample does not have - an older sample's.
			IEnumerable<string> gone = old != null ? old.Files.Keys.Where(f => !record.Files.ContainsKey(f))
				: removeExtra && Directory.Exists(modDir) ? Files(modDir).Where(f => !record.Files.ContainsKey(f)).ToList() : Enumerable.Empty<string>();
			foreach (string f in gone)
			{
				try { File.Delete(Path.Combine(modDir, f)); } catch (Exception) { }
			}
			if (Directory.Exists(modDir))
				foreach (string dir in Directory.EnumerateDirectories(modDir, "*", SearchOption.AllDirectories).OrderByDescending(d => d.Length))
					if (!Directory.EnumerateFileSystemEntries(dir).Any()) { try { Directory.Delete(dir); } catch (Exception) { } }
			File.WriteAllText(Path.Combine(modDir, RecordName), JsonSerializer.Serialize(record, Json));
			return written;
		}

		/// <summary>An installed sample taken out of the mods folder (a managed one: it is the shipped sample, installable again).</summary>
		public static void Uninstall(string modDir)
		{
			if (ReadRecord(modDir) == null) throw new InvalidOperationException(modDir + " is not a sample installed by OpenFF or Crystal - remove it by hand");
			Directory.Delete(modDir, recursive: true);
		}

		/// <summary>Whether a README is the one Crystal's Export to OpenFF writes (its "What is in it" heading).</summary>
		private static bool CrystalReadme(string path)
		{
			try { return File.Exists(path) && File.ReadAllText(path).Contains("## What is in it"); }
			catch (Exception) { return false; }
		}

		private static string Sha1(string path)
		{
			using SHA1 sha = SHA1.Create();
			using FileStream stream = File.OpenRead(path);
			return Convert.ToHexString(sha.ComputeHash(stream)).ToLowerInvariant();
		}
	}
}
