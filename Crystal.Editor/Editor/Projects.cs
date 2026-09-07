// A project: one mod, and which game it is for.
//
// Editing has always written to an override directory. A project is that directory
// with a name on it and a note of what it targets, which is what turns "my edits"
// into "a thing I can hand to somebody".
//
//   <projects>/<name>/project.json     what it is and what it targets
//   <projects>/<name>/files/...        the edited content, as the game names it
//   <projects>/<name>.backup/          originals, when it has been installed
//
// Targets exist because there are two games. Ours reads an override directory itself,
// so testing is a matter of pointing it at the project. A Steam install reads files/
// and nothing else, so testing means copying in and keeping the originals. Same
// project either way - only the content it opens against and how it is tested differ.
//
// A project may target both. That is worth having for data - .pak, .msd and .script
// are largely byte identical between the releases - and worth being careful with for
// art, which is authored against a different virtual screen in each. See Docs/Editor.md.
//
// A target is two things at once, and the editor shows them apart (Karl, 2026-09-07):
// which game's content it opens (FF3 or FF4 - the tabs above the libraries), and what
// kind of mod comes out (an OpenFF mod the client plays, which may draw on both games;
// or a Steam mod, files copied into one Steam copy). "ours" is FF3 in OpenFF; "oursff4"
// is FF4 in OpenFF, opening the same install "ff4steam" does, because that is where the
// client reads FF4 from too - only what happens to the edits differs.

using System;
using OpenFF.Content;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Crystal.Editor
{
	/// <summary>Which game a project is for.</summary>
	internal static class Targets
	{
		public const string Ours = "ours";
		public const string OursFf4 = "oursff4";
		public const string Steam = "steam";
		public const string Ff4Steam = "ff4steam";

		/// <summary>The kinds of mod a target makes.</summary>
		public const string KindOpenFF = "openff";
		public const string KindSteam = "steam";

		/// <summary>
		/// Every target. The Steam ones come first, so that with no project open (every
		/// install open once, under the first target that finds it) a Steam install is
		/// labelled as the Steam copy it is; the OpenFF targets open the same folders.
		/// </summary>
		public static readonly string[] All = { Steam, Ff4Steam, Ours, OursFf4 };

		public static bool Known(string target)
		{
			return All.Contains(target, StringComparer.OrdinalIgnoreCase);
		}

		/// <summary>"ff3" or "ff4": whose content the target opens.</summary>
		public static string GameOf(string target)
		{
			return string.Equals(target, Ff4Steam, StringComparison.OrdinalIgnoreCase)
				|| string.Equals(target, OursFf4, StringComparison.OrdinalIgnoreCase) ? "ff4" : "ff3";
		}

		/// <summary>"openff" when the OpenFF client plays the result, "steam" when it is installed into a Steam copy.</summary>
		public static string KindOf(string target)
		{
			return IsOurs(target) ? KindOpenFF : KindSteam;
		}

		/// <summary>Whether the target is one of the OpenFF ones ("ours", "oursff4").</summary>
		public static bool IsOurs(string target)
		{
			return string.Equals(target, Ours, StringComparison.OrdinalIgnoreCase)
				|| string.Equals(target, OursFf4, StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>The target for a game and a kind: ("ff4", "openff") is "oursff4".</summary>
		public static string For(string game, string kind)
		{
			bool ff4 = string.Equals(game, "ff4", StringComparison.OrdinalIgnoreCase);
			bool openff = string.Equals(kind, KindOpenFF, StringComparison.OrdinalIgnoreCase);
			return openff ? (ff4 ? OursFf4 : Ours) : (ff4 ? Ff4Steam : Steam);
		}

		/// <summary>What to show a person: "FF3 in OpenFF", "FF4 in OpenFF", "FF3 on Steam", "FF4 on Steam".</summary>
		public static string Describe(string target)
		{
			string game = GameOf(target).ToUpperInvariant();
			return IsOurs(target) ? game + " in OpenFF" : game + " on Steam";
		}

		/// <summary>
		/// Where that game's content is on this machine, or null. Steam is found through
		/// the registry; ours is a Content directory with a data000.bin in it, looked for
		/// from the working directory and then from beside the executable, because the
		/// tool is run both from the repository and from wherever it was unzipped to.
		/// </summary>
		public static string Find(string target)
		{
			if (string.Equals(target, Ff4Steam, StringComparison.OrdinalIgnoreCase)
				|| string.Equals(target, OursFf4, StringComparison.OrdinalIgnoreCase))
			{
				// FF4 has one source on this machine, the Steam install; the OpenFF client
				// plays it from there as well, so an OpenFF mod's FF4 side opens the same.
				return SteamInstalls.FindOne(SteamInstalls.Ff4AppId);
			}
			if (string.Equals(target, Steam, StringComparison.OrdinalIgnoreCase))
			{
				return SteamInstalls.FindOne();
			}

			foreach (string start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
			{
				DirectoryInfo directory = new DirectoryInfo(start);
				for (int up = 0; up < 6 && directory != null; up++, directory = directory.Parent)
				{
					string candidate = Path.Combine(directory.FullName, "Content");
					if (File.Exists(Path.Combine(candidate, "data000.bin")))
					{
						return candidate;
					}
				}
			}
			// No archives about: the OpenFF client plays FF3 from the Steam install then
			// (ContentLocator, Launch.ResolveRoot), so that is what FF3 in OpenFF opens.
			return SteamInstalls.FindOne();
		}
	}

	/// <summary>What project.json holds.</summary>
	internal sealed class ProjectFile
	{
		public string Name { get; set; }
		public string Author { get; set; }
		public string Version { get; set; }
		public string Description { get; set; }

		/// <summary>The games this is for, "ours" and/or "steam".</summary>
		public List<string> Targets { get; set; } = new List<string>();

		/// <summary>Which one is being edited against right now.</summary>
		public string Active { get; set; }

		/// <summary>
		/// Where each target's content was last found. A hint, not the truth - it is
		/// checked before it is used and re-found if it has moved, so a project stays
		/// openable after Steam is reinstalled somewhere else.
		/// </summary>
		public Dictionary<string, string> Content { get; set; } =
			new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
	}

	internal sealed class Project
	{
		private static readonly JsonSerializerOptions Json = new JsonSerializerOptions
		{
			WriteIndented = true,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
		};

		public string Directory { get; }
		public ProjectFile File { get; }

		private Project(string directory, ProjectFile file)
		{
			Directory = directory;
			File = file;
		}

		/// <summary>Where the edits live - what the workspace uses as its override.</summary>
		public string Files => Path.Combine(Directory, "files");

		/// <summary>
		/// Where a target's edits live. The two games name their files alike -
		/// files/d01_01.script is a Baron corridor in one and Ur in the other - so a
		/// project that targets both keeps them apart: targets/&lt;target&gt;/files. A
		/// project with one target, and the first target of an older project, keep
		/// using files/ so nothing already made moves.
		/// </summary>
		public string FilesFor(string target)
		{
			string perTarget = Path.Combine(Directory, "targets", target ?? string.Empty, "files");
			if (System.IO.Directory.Exists(perTarget))
			{
				return perTarget;
			}
			if (File.Targets.Count <= 1
				|| string.Equals(target, File.Targets[0], StringComparison.OrdinalIgnoreCase))
			{
				return Files;
			}
			return perTarget;
		}

		public string ManifestPath => Path.Combine(Directory, "project.json");

		/// <summary>Where projects live unless told otherwise.</summary>
		public static string Root
		{
			get
			{
				string given = Environment.GetEnvironmentVariable("FF3_PROJECTS");
				return !string.IsNullOrEmpty(given) ? given : Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
					"FF3ContentTool", "projects");
			}
		}

		/// <summary>Every project under a root, newest first.</summary>
		public static List<Project> All(string root = null)
		{
			root ??= Root;
			List<Project> found = new List<Project>();
			if (!System.IO.Directory.Exists(root))
			{
				return found;
			}
			foreach (string directory in System.IO.Directory.EnumerateDirectories(root))
			{
				Project project = TryOpen(directory);
				if (project != null)
				{
					found.Add(project);
				}
			}
			return found
				.OrderByDescending(p => System.IO.File.GetLastWriteTimeUtc(p.ManifestPath))
				.ToList();
		}

		/// <summary>Reads one, or null if that directory is not a project.</summary>
		public static Project TryOpen(string directory)
		{
			string manifest = Path.Combine(directory, "project.json");
			if (!System.IO.File.Exists(manifest))
			{
				return null;
			}
			try
			{
				ProjectFile file = JsonSerializer.Deserialize<ProjectFile>(
					System.IO.File.ReadAllText(manifest), Json);
				if (file == null)
				{
					return null;
				}
				file.Name ??= Path.GetFileName(directory);
				file.Targets ??= new List<string>();
				file.Content ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
				if (file.Targets.Count == 0)
				{
					file.Targets.Add(Targets.Steam);
				}
				if (!Targets.Known(file.Active))
				{
					file.Active = file.Targets[0];
				}
				return new Project(Path.GetFullPath(directory), file);
			}
			catch (Exception)
			{
				// Not a project we can read. Listing skips it rather than failing the
				// whole list because of one bad directory.
				return null;
			}
		}

		/// <summary>Makes one. The name is what the directory is called, so it is checked.</summary>
		public static Project Create(string name, IEnumerable<string> targets, string root = null)
		{
			string safe = Clean(name);
			if (safe.Length == 0)
			{
				throw new ArgumentException("a project needs a name");
			}

			List<string> wanted = (targets ?? Enumerable.Empty<string>())
				.Where(Targets.Known).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
			if (wanted.Count == 0)
			{
				throw new ArgumentException("a project needs at least one target");
			}

			string directory = Path.Combine(root ?? Root, safe);
			if (System.IO.Directory.Exists(directory)
				&& System.IO.File.Exists(Path.Combine(directory, "project.json")))
			{
				throw new IOException("there is already a project called " + safe);
			}

			ProjectFile file = new ProjectFile
			{
				Name = name,
				Targets = wanted,
				Active = wanted[0],
				Version = "1.0"
			};
			foreach (string target in wanted)
			{
				string content = Targets.Find(target);
				if (content != null)
				{
					file.Content[target] = content;
				}
			}

			Project project = new Project(Path.GetFullPath(directory), file);
			System.IO.Directory.CreateDirectory(project.Files);
			project.Save();
			return project;
		}

		public void Save()
		{
			System.IO.Directory.CreateDirectory(Directory);
			System.IO.File.WriteAllText(ManifestPath, JsonSerializer.Serialize(File, Json));
		}

		/// <summary>
		/// The content directory for the active target, re-found if the remembered one
		/// has gone. Throws with something worth reading when there is nothing to open.
		/// </summary>
		public string ContentDirectory()
		{
			return ContentDirectoryFor(File.Active);
		}

		/// <summary>The content directory for one target, on the same terms.</summary>
		public string ContentDirectoryFor(string target)
		{

			if (File.Content.TryGetValue(target, out string remembered)
				&& System.IO.Directory.Exists(remembered)
				&& (System.IO.File.Exists(Path.Combine(remembered, "data000.bin"))
					|| LooseContentSource.Looks(remembered)))
			{
				return remembered;
			}

			string found = Targets.Find(target);
			if (found == null)
			{
				throw new FileNotFoundException(string.Format(
					"cannot find the content for {0}. {1}",
					Targets.Describe(target),
					!string.Equals(target, Targets.Ours, StringComparison.OrdinalIgnoreCase)
						? "No Steam copy of the game was found on this machine - pass "
							+ "--content=<install> to say where it is."
						: "No Content directory with a data000.bin was found - pass "
							+ "--content=<dir> to say where it is."));
			}

			File.Content[target] = found;
			Save();
			return found;
		}

		/// <summary>Letters, digits, spaces and dashes. It becomes a directory name.</summary>
		private static string Clean(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				return string.Empty;
			}
			char[] kept = name.Trim()
				.Where(c => char.IsLetterOrDigit(c) || c == ' ' || c == '-' || c == '_')
				.ToArray();
			return new string(kept).Trim();
		}
	}
}
