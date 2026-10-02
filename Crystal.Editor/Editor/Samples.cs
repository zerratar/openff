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
				project.File.Sample = sample.Id;
				project.Save();
				foreach ((string from, string to) in Plan(sample, project))
				{
					System.IO.Directory.CreateDirectory(Path.GetDirectoryName(to));
					File.Copy(from, to, overwrite: true);
				}
				List<(string File, string Relative)> sources = SampleSources(sample.Directory);
				if (sources.Count > 0)
				{
					string codeOut = ModCode.CodeDirectory(project);
					try { ModCode.Create(project); }
					catch (InvalidOperationException)
					{
						// No client found for the engine reference: the sources are there; Add C# code
						// later writes the csproj once the client has been started.
					}
					// Create writes a starter Mod.cs; a sample with code of its own does not want a second entry point.
					string starter = Path.Combine(codeOut, "Mod.cs");
					if (File.Exists(starter) && !sources.Any(s => string.Equals(Path.GetFileName(s.File), "Mod.cs", StringComparison.OrdinalIgnoreCase)))
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

		/// <summary>
		/// Every file of a sample and where it goes in a project: the content folders the two layouts share by name (data/
		/// is the mod's own files its code reads - Rogue Mode's acts and jobs), each game's files (&lt;game&gt;/files/, or a plain
		/// files/ for FF3 from before that layout), and the code - the .cs files at the sample's root and in its folders
		/// (Core/), or under code/, into the project's code/ at the same relative places; the sample's csproj is written for
		/// the repository, so the project keeps Crystal's own. A folder with a project of its own (Tests/) is not the mod's
		/// code, and neither is a build's output.
		/// </summary>
		private static List<(string From, string To)> Plan(Sample sample, Project project)
		{
			var plan = new List<(string, string)>();
			void Tree(string from, string to)
			{
				if (!System.IO.Directory.Exists(from)) return;
				foreach (string file in System.IO.Directory.EnumerateFiles(from, "*", SearchOption.AllDirectories))
					plan.Add((file, Path.Combine(to, Path.GetRelativePath(from, file))));
			}
			foreach (string folder in new[] { "scenes", "defs", "assets", "menus", "data" })
				Tree(Path.Combine(sample.Directory, folder), Path.Combine(project.Directory, folder));
			foreach (string target in project.File.Targets)
			{
				string game = Targets.GameOf(target);
				string source = Path.Combine(sample.Directory, game, "files");
				if (!System.IO.Directory.Exists(source) && game == "ff3") source = Path.Combine(sample.Directory, "files");
				Tree(source, project.FilesFor(target));
			}
			foreach ((string file, string relative) in SampleSources(sample.Directory))
				plan.Add((file, Path.Combine(ModCode.CodeDirectory(project), relative)));
			return plan;
		}

		/// <summary>The sample a project was made from: its manifest's, else (a project from before that was kept) what its SAMPLE.txt says.</summary>
		public static Sample SampleOf(Project project)
		{
			string id = project.File.Sample;
			if (string.IsNullOrEmpty(id))
			{
				string note = Path.Combine(project.Directory, "SAMPLE.txt");
				if (File.Exists(note))
				{
					string text = File.ReadAllText(note);
					foreach (Sample s in All())
						if (text.Contains("(" + s.Directory + ")", StringComparison.OrdinalIgnoreCase) || text.Contains("the " + s.Name + " sample", StringComparison.OrdinalIgnoreCase)) return s;
				}
				return null;
			}
			return All().FirstOrDefault(s => string.Equals(s.Id, id, StringComparison.OrdinalIgnoreCase));
		}

		/// <summary>What Update from the sample would do: the files it adds, the ones it replaces that the project has changed, and how many are already as the sample has them.</summary>
		public sealed class UpdatePlan
		{
			public string Sample { get; set; }
			public List<string> Added { get; set; } = new List<string>();
			public List<string> Changed { get; set; } = new List<string>();
			public int Same { get; set; }
		}

		public static UpdatePlan Compare(Project project)
		{
			Sample sample = SampleOf(project) ?? throw new InvalidOperationException("the project was not made from a sample");
			UpdatePlan result = new UpdatePlan { Sample = sample.Name };
			foreach ((string from, string to) in Plan(sample, project))
			{
				string shown = Path.GetRelativePath(project.Directory, to).Replace('\\', '/');
				if (!File.Exists(to)) result.Added.Add(shown);
				else if (!SameBytes(from, to)) result.Changed.Add(shown);
				else result.Same++;
			}
			return result;
		}

		/// <summary>
		/// The project brought up to its sample: every file of the sample's copied over the project's as a new project
		/// from it would have it - the files the project added of its own are kept, and so are its settings and Crystal's
		/// csproj. Returns the plan carried out.
		/// </summary>
		public static UpdatePlan Update(Project project)
		{
			Sample sample = SampleOf(project) ?? throw new InvalidOperationException("the project was not made from a sample");
			UpdatePlan done = Compare(project);
			foreach ((string from, string to) in Plan(sample, project))
			{
				if (File.Exists(to) && SameBytes(from, to)) continue;
				System.IO.Directory.CreateDirectory(Path.GetDirectoryName(to));
				File.Copy(from, to, overwrite: true);
			}
			if (string.IsNullOrEmpty(project.File.Sample)) { project.File.Sample = sample.Id; project.Save(); }
			return done;
		}

		private static bool SameBytes(string a, string b)
		{
			FileInfo fa = new FileInfo(a), fb = new FileInfo(b);
			if (fa.Length != fb.Length) return false;
			return File.ReadAllBytes(a).AsSpan().SequenceEqual(File.ReadAllBytes(b));
		}

		/// <summary>A sample's C# sources with their paths as the project's code/ keeps them: code/** as it is, else the sample's own tree.</summary>
		private static List<(string File, string Relative)> SampleSources(string root)
		{
			var found = new List<(string, string)>();
			string codeIn = Path.Combine(root, "code");
			if (System.IO.Directory.Exists(codeIn))
			{
				foreach (string file in System.IO.Directory.EnumerateFiles(codeIn, "*.cs", SearchOption.AllDirectories))
					if (!Skipped(codeIn, file)) found.Add((file, Path.GetRelativePath(codeIn, file)));
			}
			foreach (string file in System.IO.Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories))
			{
				if (file.StartsWith(codeIn + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) || Skipped(root, file)) continue;
				found.Add((file, Path.GetRelativePath(root, file)));
			}
			return found;
		}

		/// <summary>Whether a source is out of the mod's code: under bin/ or obj/, or in a folder below the root with a project of its own.</summary>
		private static bool Skipped(string root, string file)
		{
			for (string dir = Path.GetDirectoryName(file); dir != null && dir.Length > root.Length; dir = Path.GetDirectoryName(dir))
			{
				string name = Path.GetFileName(dir);
				if (string.Equals(name, "bin", StringComparison.OrdinalIgnoreCase) || string.Equals(name, "obj", StringComparison.OrdinalIgnoreCase)) return true;
				if (System.IO.Directory.EnumerateFiles(dir, "*.csproj", SearchOption.TopDirectoryOnly).Any()) return true;
			}
			return false;
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
