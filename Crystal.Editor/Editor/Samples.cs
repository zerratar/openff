// The sample mods, and opening one as a project of your own.
//
// The repository's Samples/ (Showcase, HelloMod, Survivors) ships in the release zip beside
// crystal.exe. Each is a finished mod folder - mod.json, scenes/, defs/, assets/, and for the
// ones with code the C# sources and a csproj. "Sample projects…" in Crystal lists them and
// copies one into a fresh project in the projects folder, laid out as a project is (scenes/,
// defs/, assets/, code/ with a csproj Crystal writes against the client's engine), so it opens
// in the editor exactly like something you made: read it, change it, Run in OpenFF. The
// sample itself is never touched.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Crystal.Editor
{
	internal static class Samples
	{
		public sealed class Sample
		{
			public string Id { get; set; }
			public string Name { get; set; }
			public string Description { get; set; }
			public string Directory { get; set; }
			public bool Code { get; set; }
			public int Scenes { get; set; }
			public int Definitions { get; set; }
			public int Assets { get; set; }
			public List<string> Games { get; set; } = new List<string>();
		}

		/// <summary>The Samples folder: beside the executable (a release), else up from it to the repository's.</summary>
		public static string Folder()
		{
			foreach (string root in Roots())
			{
				string candidate = Path.Combine(root, "Samples");
				if (System.IO.Directory.Exists(candidate) && System.IO.Directory.EnumerateFiles(candidate, "mod.json", SearchOption.AllDirectories).Any())
				{
					return candidate;
				}
			}
			return null;
		}

		private static IEnumerable<string> Roots()
		{
			string at = AppContext.BaseDirectory;
			for (int i = 0; i < 6 && !string.IsNullOrEmpty(at); i++)
			{
				yield return at;
				at = Path.GetDirectoryName(at.TrimEnd(Path.DirectorySeparatorChar));
			}
			yield return System.IO.Directory.GetCurrentDirectory();
		}

		public static List<Sample> All()
		{
			List<Sample> list = new List<Sample>();
			string folder = Folder();
			if (folder == null) return list;
			foreach (string dir in System.IO.Directory.EnumerateDirectories(folder).OrderBy(d => d, StringComparer.OrdinalIgnoreCase))
			{
				string manifest = Path.Combine(dir, "mod.json");
				if (!File.Exists(manifest)) continue;
				Sample sample = new Sample { Id = Path.GetFileName(dir), Name = Path.GetFileName(dir), Directory = dir };
				try
				{
					using JsonDocument doc = JsonDocument.Parse(File.ReadAllText(manifest));
					if (doc.RootElement.TryGetProperty("name", out JsonElement name) && name.ValueKind == JsonValueKind.String) sample.Name = name.GetString();
					if (doc.RootElement.TryGetProperty("description", out JsonElement description) && description.ValueKind == JsonValueKind.String) sample.Description = description.GetString();
					if (doc.RootElement.TryGetProperty("games", out JsonElement games) && games.ValueKind == JsonValueKind.Array)
						sample.Games = games.EnumerateArray().Where(g => g.ValueKind == JsonValueKind.String).Select(g => g.GetString().ToLowerInvariant()).ToList();
				}
				catch (Exception) { /* a manifest that does not parse still lists by its folder */ }
				sample.Code = System.IO.Directory.EnumerateFiles(dir, "*.csproj", SearchOption.TopDirectoryOnly).Any() || System.IO.Directory.Exists(Path.Combine(dir, "code"));
				sample.Scenes = CountFiles(Path.Combine(dir, "scenes"), "*.json");
				sample.Definitions = CountFiles(Path.Combine(dir, "defs"), "*.json") + CountFiles(Path.Combine(dir, "menus"), "*.json");
				sample.Assets = CountFiles(Path.Combine(dir, "assets"), "*.*");
				list.Add(sample);
			}
			return list;
		}

		private static int CountFiles(string dir, string pattern)
		{
			return System.IO.Directory.Exists(dir) ? System.IO.Directory.EnumerateFiles(dir, pattern, SearchOption.AllDirectories).Count() : 0;
		}

		/// <summary>
		/// A sample as a new project: created for the OpenFF FF3 target (and FF4's when the
		/// sample says it plays there), its scenes, definitions, assets and game files copied
		/// in, its C# under code/ with a csproj against the client's engine. Returns the project.
		/// </summary>
		public static Project OpenAsProject(string id, string projectName)
		{
			Sample sample = All().FirstOrDefault(s => string.Equals(s.Id, id, StringComparison.OrdinalIgnoreCase))
				?? throw new ArgumentException("no sample called " + id);
			string name = string.IsNullOrWhiteSpace(projectName) ? sample.Name : projectName.Trim();
			List<string> targets = new List<string> { Targets.Ours };
			if (sample.Games.Contains("ff4")) targets.Add(Targets.OursFf4);
			if (sample.Games.Count > 0 && !sample.Games.Contains("ff3")) targets.Remove(Targets.Ours);
			if (targets.Count == 0) targets.Add(Targets.Ours);
			Project project = Project.Create(name, targets);
			try
			{
				project.File.Description = string.IsNullOrEmpty(sample.Description) ? "From the " + sample.Name + " sample." : sample.Description;
				project.Save();
				// The content folders the two layouts share by name.
				foreach (string folder in new[] { "scenes", "defs", "assets", "menus" })
				{
					CopyTree(Path.Combine(sample.Directory, folder), Path.Combine(project.Directory, folder));
				}
				// The game files: <game>/files/ per game, or a plain files/ (FF3) from before that layout.
				foreach (string target in targets)
				{
					string game = Targets.GameOf(target);
					string source = Path.Combine(sample.Directory, game, "files");
					if (!System.IO.Directory.Exists(source) && game == "ff3") source = Path.Combine(sample.Directory, "files");
					CopyTree(source, project.FilesFor(target));
				}
				// The code: the .cs files under code/, with Crystal's own csproj so it builds against
				// the engine beside the client - the sample's csproj is written for the repository.
				List<string> sources = System.IO.Directory.EnumerateFiles(sample.Directory, "*.cs", SearchOption.TopDirectoryOnly).ToList();
				string codeIn = Path.Combine(sample.Directory, "code");
				if (System.IO.Directory.Exists(codeIn)) sources.AddRange(System.IO.Directory.EnumerateFiles(codeIn, "*.cs", SearchOption.AllDirectories));
				if (sources.Count > 0)
				{
					string codeOut = ModCode.CodeDirectory(project);
					System.IO.Directory.CreateDirectory(codeOut);
					foreach (string file in sources)
					{
						File.Copy(file, Path.Combine(codeOut, Path.GetFileName(file)), overwrite: true);
					}
					try { ModCode.Create(project); }
					catch (InvalidOperationException)
					{
						// No client found for the engine reference: the sources are there; Add C# code
						// later writes the csproj once the client has been started.
					}
					// Create writes a starter Mod.cs; a sample with code of its own does not want a second entry point.
					string starter = Path.Combine(codeOut, "Mod.cs");
					if (File.Exists(starter) && !sources.Any(s => string.Equals(Path.GetFileName(s), "Mod.cs", StringComparison.OrdinalIgnoreCase)))
					{
						File.Delete(starter);
					}
				}
				// A README beside the project, saying where it came from.
				File.WriteAllText(Path.Combine(project.Directory, "SAMPLE.txt"),
					"This project was made from the " + sample.Name + " sample (" + sample.Directory + ").\r\n" +
					"It is a copy: change anything. The sample itself is untouched.\r\n", new UTF8Encoding(false));
			}
			catch (Exception)
			{
				try { System.IO.Directory.Delete(project.Directory, recursive: true); } catch (Exception) { }
				throw;
			}
			return project;
		}

		private static void CopyTree(string from, string to)
		{
			if (!System.IO.Directory.Exists(from)) return;
			foreach (string file in System.IO.Directory.EnumerateFiles(from, "*", SearchOption.AllDirectories))
			{
				string relative = Path.GetRelativePath(from, file);
				string destination = Path.Combine(to, relative);
				System.IO.Directory.CreateDirectory(Path.GetDirectoryName(destination));
				File.Copy(file, destination, overwrite: true);
			}
		}
	}
}
