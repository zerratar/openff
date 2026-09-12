// A project's C# code: a csproj Crystal writes, builds and hands to the editor of choice.
//
// A mod for OpenFF can carry code (Docs/OpenFF-Engine.md). Crystal does not want to be
// the IDE for it: it makes the project file and a starting class, builds it with the
// .NET SDK when asked, and opens the csproj in whatever Visual Studio, Rider or VS Code
// the machine associates with it. The build's output goes to <project>/build/, and Export
// to OpenFF carries the assemblies into the mod folder and names them in mod.json, where
// the client loads them (and hot-reloads them when they change).
//
// The csproj references the client's own OpenFF.Engine.dll (never copied into the mod:
// the client shares its engine with every mod), found through OpenFFClient.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Crystal.Editor
{
	internal static class ModCode
	{
		public static string CodeDirectory(Project project) => Path.Combine(project.Directory, "code");
		public static string BuildDirectory(Project project) => Path.Combine(project.Directory, "build");

		/// <summary>The assembly name and csproj stem: the project's name made safe for a file.</summary>
		public static string AssemblyName(Project project)
		{
			char[] bad = Path.GetInvalidFileNameChars();
			string cleaned = new string((project.File.Name ?? "Mod").Trim().Select(c => bad.Contains(c) || c == ' ' || c == '.' || c == '-' ? '_' : c).ToArray());
			if (cleaned.Length == 0 || char.IsDigit(cleaned[0]))
			{
				cleaned = "Mod" + cleaned;
			}
			return cleaned;
		}

		public static string ProjectFile(Project project) => Path.Combine(CodeDirectory(project), AssemblyName(project) + ".csproj");

		public static bool Has(Project project) => File.Exists(ProjectFile(project));

		/// <summary>The built assemblies (and their .pdb) the mod carries: every .dll in build/ but the engine's.</summary>
		public static List<string> Assemblies(Project project)
		{
			string build = BuildDirectory(project);
			if (!Directory.Exists(build))
			{
				return new List<string>();
			}
			return Directory.EnumerateFiles(build, "*.dll", SearchOption.TopDirectoryOnly)
				.Where(f => !Path.GetFileName(f).StartsWith("OpenFF.Engine", StringComparison.OrdinalIgnoreCase))
				.OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
				.ToList();
		}

		/// <summary>Writes code/&lt;Name&gt;.csproj and a starting Mod.cs. Refuses when the csproj exists.</summary>
		public static string Create(Project project)
		{
			string engine = OpenFFClient.EngineAssembly();
			if (engine == null)
			{
				throw new InvalidOperationException("the OpenFF client's OpenFF.Engine.dll was not found - build or start the client once");
			}
			string csproj = ProjectFile(project);
			if (File.Exists(csproj))
			{
				throw new IOException("the project already has code: " + csproj);
			}
			string directory = CodeDirectory(project);
			Directory.CreateDirectory(directory);
			string name = AssemblyName(project);
			File.WriteAllText(csproj, Csproj(name, engine), new UTF8Encoding(false));
			string source = Path.Combine(directory, "Mod.cs");
			if (!File.Exists(source))
			{
				File.WriteAllText(source, Starter(name, project.File.Name), new UTF8Encoding(false));
			}
			string ignore = Path.Combine(directory, ".gitignore");
			if (!File.Exists(ignore))
			{
				File.WriteAllText(ignore, "bin/\nobj/\n", new UTF8Encoding(false));
			}
			return csproj;
		}

		// ------------------------------------------------------------ the mod's own files
		//
		// What the project tree's "OpenFF mod" folder lists: the C# under code/, the scene
		// files under scenes/, and project.json. They are the project's files rather than a
		// game's, so they are the same whichever game the page is looking at, and they can be
		// read and written here as text - the IDE is one way in, this is the other.

		/// <summary>One entry of the mod folder as the page lists it.</summary>
		public sealed class Entry
		{
			public string Name { get; set; }
			public string Kind { get; set; }
			public long Bytes { get; set; }
			public DateTime Modified { get; set; }
			public bool ReadOnly { get; set; }
			/// <summary>A source file changed since the last build (false when there is no build to compare with).</summary>
			public bool Stale { get; set; }
		}

		/// <summary>The files a mod is made of, in the order the tree shows them: project.json, code/, scenes/.</summary>
		public static List<Entry> Tree(Project project)
		{
			List<Entry> entries = new List<Entry>();
			if (File.Exists(project.ManifestPath))
			{
				entries.Add(Describe(project, project.ManifestPath, readOnly: true));
			}
			DateTime built = DateTime.MinValue;
			foreach (string dll in Assemblies(project))
			{
				DateTime when = File.GetLastWriteTimeUtc(dll);
				if (when > built) built = when;
			}
			string code = CodeDirectory(project);
			if (Directory.Exists(code))
			{
				foreach (string file in Directory.EnumerateFiles(code, "*", SearchOption.AllDirectories)
					.Where(f => !Hidden(code, f))
					.OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
				{
					Entry entry = Describe(project, file, readOnly: false);
					if (entry.Kind == "cs" && built != DateTime.MinValue)
					{
						entry.Stale = File.GetLastWriteTimeUtc(file) > built;
					}
					entries.Add(entry);
				}
			}
			// The scene files are listed under Scenes, where each opens its map; their JSON
			// still reads and saves through ReadText/WriteText (Resolve allows scenes/).
			return entries;
		}

		/// <summary>A class in the mod's source that derives Behaviour or GameService, and where.</summary>
		public sealed class SourceType
		{
			public string Name { get; set; }
			/// <summary>behaviour or service.</summary>
			public string Kind { get; set; }
			/// <summary>The file as the tree names it: code/Greeter.cs.</summary>
			public string File { get; set; }
			public int Line { get; set; }
		}

		private static readonly Regex ClassDeclaration = new Regex(
			@"\bclass\s+([A-Za-z_]\w*)\s*(?:<[^>{]*>)?\s*:\s*([^{]+)",
			RegexOptions.Compiled);

		/// <summary>
		/// The Behaviour and GameService classes as the source declares them, built or not -
		/// read with a regular expression over code/**/*.cs, which is enough to find
		/// "class Greeter : Behaviour" and say which file it is in. The catalog knows the
		/// built types and their fields; this knows the files, and knows a class the moment
		/// it is written, so the inspector can offer it (and open it) before the first build.
		/// </summary>
		public static List<SourceType> Sources(Project project)
		{
			List<SourceType> found = new List<SourceType>();
			string code = CodeDirectory(project);
			if (!Directory.Exists(code))
			{
				return found;
			}
			foreach (string file in Directory.EnumerateFiles(code, "*.cs", SearchOption.AllDirectories)
				.Where(f => !Hidden(code, f))
				.OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
			{
				string text;
				try
				{
					text = File.ReadAllText(file);
				}
				catch (IOException)
				{
					continue;
				}
				// Line comments blanked (not removed, so the line numbers hold): the starter's
				// commented examples must not count as classes.
				text = Regex.Replace(text, @"//[^\r\n]*", m => new string(' ', m.Length));
				foreach (Match match in ClassDeclaration.Matches(text))
				{
					// The base list up to the brace: "Behaviour", "OpenFF.GameService, ISaveable"...
					string bases = match.Groups[2].Value;
					string kind = Regex.IsMatch(bases, @"(^|[\s,.])MenuBehaviour\b") ? "menu"
						: Regex.IsMatch(bases, @"(^|[\s,.])Behaviour\b") ? "behaviour"
						: Regex.IsMatch(bases, @"(^|[\s,.])GameService\b") ? "service" : null;
					if (kind == null)
					{
						continue;
					}
					int line = 1;
					for (int i = 0; i < match.Index; i++)
					{
						if (text[i] == '\n') line++;
					}
					found.Add(new SourceType
					{
						Name = match.Groups[1].Value,
						Kind = kind,
						File = Path.GetRelativePath(project.Directory, file).Replace('\\', '/'),
						Line = line
					});
				}
			}
			return found;
		}

		/// <summary>bin/, obj/, .vs/ and anything else that starts with a dot: the IDE's, not the mod's.</summary>
		private static bool Hidden(string root, string file)
		{
			string relative = Path.GetRelativePath(root, file);
			return relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
				.Any(part => part.StartsWith('.') || part.Equals("bin", StringComparison.OrdinalIgnoreCase) || part.Equals("obj", StringComparison.OrdinalIgnoreCase));
		}

		private static Entry Describe(Project project, string file, bool readOnly)
		{
			FileInfo info = new FileInfo(file);
			string extension = info.Extension.TrimStart('.').ToLowerInvariant();
			return new Entry
			{
				Name = Path.GetRelativePath(project.Directory, file).Replace('\\', '/'),
				Kind = extension == "cs" || extension == "csproj" || extension == "json" ? extension : "text",
				Bytes = info.Length,
				Modified = info.LastWriteTimeUtc,
				ReadOnly = readOnly,
			};
		}

		/// <summary>
		/// The full path of a mod file named the way the tree names it, checked to lie under
		/// code/ or scenes/ (or to be project.json). Anything else - a path that climbs out,
		/// a game file - is refused, since this reads and writes what it is given.
		/// </summary>
		public static string Resolve(Project project, string name, out bool readOnly)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException("which file?");
			}
			string full = Path.GetFullPath(Path.Combine(project.Directory, name.Replace('/', Path.DirectorySeparatorChar)));
			string root = Path.GetFullPath(project.Directory).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
			if (string.Equals(full, Path.GetFullPath(project.ManifestPath), StringComparison.OrdinalIgnoreCase))
			{
				readOnly = true;
				return full;
			}
			readOnly = false;
			bool inside = full.StartsWith(root, StringComparison.OrdinalIgnoreCase);
			string code = Path.GetFullPath(CodeDirectory(project)) + Path.DirectorySeparatorChar;
			string scenes = Path.GetFullPath(ProjectScenes.Directory(project)) + Path.DirectorySeparatorChar;
			string defs = Path.GetFullPath(Path.Combine(project.Directory, "defs")) + Path.DirectorySeparatorChar;
			if (!inside || !(full.StartsWith(code, StringComparison.OrdinalIgnoreCase) || full.StartsWith(scenes, StringComparison.OrdinalIgnoreCase) || full.StartsWith(defs, StringComparison.OrdinalIgnoreCase)))
			{
				throw new ArgumentException("not one of the mod's files: " + name);
			}
			if (Hidden(project.Directory, full))
			{
				throw new ArgumentException("not one of the mod's files: " + name);
			}
			return full;
		}

		/// <summary>A mod file's text.</summary>
		public static string ReadText(Project project, string name, out Entry entry)
		{
			string full = Resolve(project, name, out bool readOnly);
			if (!File.Exists(full))
			{
				throw new FileNotFoundException("no such file in the project: " + name);
			}
			entry = Describe(project, full, readOnly);
			return File.ReadAllText(full);
		}

		/// <summary>Writes a mod file's text. project.json is edited through the settings, not here.</summary>
		public static Entry WriteText(Project project, string name, string text)
		{
			string full = Resolve(project, name, out bool readOnly);
			if (readOnly)
			{
				throw new InvalidOperationException("project.json is edited through File > Project settings");
			}
			Directory.CreateDirectory(Path.GetDirectoryName(full));
			File.WriteAllText(full, text ?? string.Empty, new UTF8Encoding(false));
			return Describe(project, full, readOnly: false);
		}

		/// <summary>
		/// A new C# file under code/, from one of the starters: "behaviour" (a Behaviour for a
		/// map object), "service" (a GameService), or "empty" (the usings and a namespace).
		/// The class takes the file's name. Refuses a file that exists.
		/// </summary>
		public static string CreateFile(Project project, string name, string template)
		{
			if (!Has(project))
			{
				throw new InvalidOperationException("the project has no C# code yet - Add C# code first");
			}
			string stem = Path.GetFileNameWithoutExtension((name ?? string.Empty).Trim());
			char[] bad = Path.GetInvalidFileNameChars();
			if (stem.Length == 0 || stem.Any(c => bad.Contains(c)) || (name ?? string.Empty).Contains("..") )
			{
				throw new ArgumentException("a file needs a plain name, like Greeter or Quests/Fetch");
			}
			string className = new string(stem.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
			if (className.Length == 0 || char.IsDigit(className[0]))
			{
				className = "Class" + className;
			}
			string relative = Path.Combine("code", (name ?? string.Empty).Trim().Replace('/', Path.DirectorySeparatorChar));
			if (!relative.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
			{
				relative += ".cs";
			}
			string full = Resolve(project, relative, out _);
			if (File.Exists(full))
			{
				throw new IOException("there is already a " + relative.Replace('\\', '/'));
			}
			Directory.CreateDirectory(Path.GetDirectoryName(full));
			File.WriteAllText(full, StarterFile(AssemblyName(project), className, template), new UTF8Encoding(false));
			return Path.GetRelativePath(project.Directory, full).Replace('\\', '/');
		}

		/// <summary>Opens one of the mod's files with whatever the machine opens that kind with.</summary>
		public static void OpenFile(Project project, string name)
		{
			string full = Resolve(project, name, out _);
			if (!File.Exists(full))
			{
				throw new FileNotFoundException("no such file in the project: " + name);
			}
			Process.Start(new ProcessStartInfo(full) { UseShellExecute = true });
		}

		/// <summary>One line of a build's output that names a place: file, line, column, what.</summary>
		public sealed class Problem
		{
			public string File { get; set; }
			public int Line { get; set; }
			public int Column { get; set; }
			public string Kind { get; set; }
			public string Code { get; set; }
			public string Message { get; set; }
		}

		/// <summary>Builds the code with the .NET SDK. Returns whether it succeeded and what the build said.</summary>
		public static bool Build(Project project, out string output)
		{
			return Build(project, out output, out _);
		}

		/// <summary>Builds the code; the errors and warnings come back as a list too, each naming its file relative to the project.</summary>
		public static bool Build(Project project, out string output, out List<Problem> problems)
		{
			problems = new List<Problem>();
			string csproj = ProjectFile(project);
			if (!File.Exists(csproj))
			{
				output = "the project has no code yet";
				return false;
			}
			Directory.CreateDirectory(BuildDirectory(project));
			ProcessStartInfo start = new ProcessStartInfo("dotnet", "build \"" + csproj + "\" -c Debug -nologo -v q")
			{
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				CreateNoWindow = true,
				WorkingDirectory = CodeDirectory(project),
			};
			StringBuilder text = new StringBuilder();
			try
			{
				using Process process = Process.Start(start);
				process.OutputDataReceived += (_, e) => { if (e.Data != null) text.AppendLine(e.Data); };
				process.ErrorDataReceived += (_, e) => { if (e.Data != null) text.AppendLine(e.Data); };
				process.BeginOutputReadLine();
				process.BeginErrorReadLine();
				if (!process.WaitForExit(180000))
				{
					try { process.Kill(true); } catch (Exception) { }
					text.AppendLine("the build did not finish in three minutes");
					output = text.ToString();
					return false;
				}
				process.WaitForExit();
				problems = Problems(project, text.ToString());
				output = Tidy(text.ToString());
				return process.ExitCode == 0;
			}
			catch (Exception ex)
			{
				output = "dotnet could not be started (" + ex.Message + "). Building a mod's C# needs the .NET 8 SDK (the SDK, x64 - not just the runtime): https://dotnet.microsoft.com/download/dotnet/8.0 - then start Crystal again. A mod without C# never needs it.";
				return false;
			}
		}

		/// <summary>Opens the csproj with whatever the machine opens .csproj files with (Visual Studio, Rider, VS Code...).</summary>
		public static void Open(Project project)
		{
			string csproj = ProjectFile(project);
			if (!File.Exists(csproj))
			{
				throw new FileNotFoundException("the project has no code yet");
			}
			Process.Start(new ProcessStartInfo(csproj) { UseShellExecute = true });
		}

		// "C:\...\code\Mod.cs(12,9): error CS0103: The name 'x' does not exist [C:\...\Mod.csproj]"
		private static readonly System.Text.RegularExpressions.Regex ProblemLine = new System.Text.RegularExpressions.Regex(
			@"^(?<file>.+?)\((?<line>\d+),(?<column>\d+)\): (?<kind>error|warning) (?<code>\w+): (?<message>.*?)(?: \[[^\]]*\])?$",
			System.Text.RegularExpressions.RegexOptions.Compiled);

		/// <summary>The errors and warnings in a build's output, each once, the file named relative to the project.</summary>
		private static List<Problem> Problems(Project project, string output)
		{
			List<Problem> found = new List<Problem>();
			HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
			foreach (string raw in output.Replace("\r", "").Split('\n'))
			{
				System.Text.RegularExpressions.Match match = ProblemLine.Match(raw.Trim());
				if (!match.Success || !seen.Add(raw.Trim()))
				{
					continue;
				}
				string file = match.Groups["file"].Value;
				try
				{
					if (Path.IsPathRooted(file))
					{
						file = Path.GetRelativePath(project.Directory, file);
					}
				}
				catch (Exception) { }
				found.Add(new Problem
				{
					File = file.Replace('\\', '/'),
					Line = int.Parse(match.Groups["line"].Value),
					Column = int.Parse(match.Groups["column"].Value),
					Kind = match.Groups["kind"].Value,
					Code = match.Groups["code"].Value,
					Message = match.Groups["message"].Value,
				});
			}
			return found;
		}

		/// <summary>The build's lines a person wants: errors and warnings, then the last few lines.</summary>
		private static string Tidy(string output)
		{
			string[] lines = output.Replace("\r", "").Split('\n').Where(l => l.Trim().Length > 0).ToArray();
			List<string> kept = lines.Where(l => l.Contains(" error ") || l.Contains(" warning ")).Distinct().Take(12).ToList();
			if (kept.Count == 0)
			{
				kept = lines.Skip(Math.Max(0, lines.Length - 4)).ToList();
			}
			// The paths are long; the file name and the message are what matter.
			return string.Join("\n", kept.Select(l =>
			{
				int idx = l.IndexOf(": error ", StringComparison.Ordinal);
				if (idx < 0) idx = l.IndexOf(": warning ", StringComparison.Ordinal);
				if (idx < 0) return l.Trim();
				string where = l.Substring(0, idx);
				string file = Path.GetFileName(where.Split('(')[0]);
				string position = where.Contains('(') ? "(" + where.Split('(')[1] : "";
				string rest = l.Substring(idx + 2);
				int bracket = rest.IndexOf(" [", StringComparison.Ordinal);
				return file + position + ": " + (bracket > 0 ? rest.Substring(0, bracket) : rest).Trim();
			}));
		}

		private static string Csproj(string name, string engineDll)
		{
			return "<Project Sdk=\"Microsoft.NET.Sdk\">\n\n" +
				"  <!-- Made by Crystal. Build it here (Project > Build C# code), in Visual Studio, or with\n" +
				"       `dotnet build`: the output lands in ../build/, and Export to OpenFF carries it into\n" +
				"       the mod folder. The engine assembly is the client's; it is never copied. -->\n\n" +
				"  <PropertyGroup>\n" +
				"    <TargetFramework>net8.0</TargetFramework>\n" +
				"    <AssemblyName>" + name + "</AssemblyName>\n" +
				"    <RootNamespace>" + name + "</RootNamespace>\n" +
				"    <Nullable>disable</Nullable>\n" +
				"    <ImplicitUsings>disable</ImplicitUsings>\n" +
				"    <LangVersion>latest</LangVersion>\n" +
				"    <DebugType>portable</DebugType>\n" +
				"    <OutDir>$(MSBuildProjectDirectory)\\..\\build\\</OutDir>\n" +
				"    <AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>\n" +
				"    <GenerateDependencyFile>false</GenerateDependencyFile>\n" +
				"    <ProduceReferenceAssembly>false</ProduceReferenceAssembly>\n" +
				"    <GenerateDocumentationFile>true</GenerateDocumentationFile>\n" +
				"    <NoWarn>$(NoWarn);CS1591</NoWarn>\n" +
				"  </PropertyGroup>\n\n" +
				"  <ItemGroup>\n" +
				"    <Reference Include=\"OpenFF.Engine\">\n" +
				"      <HintPath>" + engineDll + "</HintPath>\n" +
				"      <Private>false</Private>\n" +
				"    </Reference>\n" +
				"  </ItemGroup>\n\n" +
				"</Project>\n";
		}

		private static string Starter(string name, string displayName)
		{
			string title = (displayName ?? name).Replace("\"", "'");
			return "// " + title + " - a mod for OpenFF.\n" +
				"//\n" +
				"// A GameService lives for the whole run: it hears OnGameStart, OnUpdate (when WantsUpdate\n" +
				"// is true), the map callbacks and OnQuit, and it reaches the game through Game.Dialogue,\n" +
				"// Game.Hero, Game.Npcs, Game.Party, Game.Audio, Game.Flags, Game.Screen and Game.Field.\n" +
				"// Public fields survive a hot reload: rebuild while the client runs and the new code takes\n" +
				"// over with the old state. Anything it does is guarded; an exception is logged as\n" +
				"// \"engine: WARNING ...\" in the client's log and the game goes on.\n" +
				"//\n" +
				"// A Behaviour (Sign, below) is a script for one object: Crystal lists the Behaviour classes\n" +
				"// of the built code in a map's inspector, where they are attached to a character, an exit\n" +
				"// or the map with their public fields filled in (saved as scenes/<map>.json).\n" +
				"\n" +
				"using System;\n" +
				"using System.Collections.Generic;\n" +
				"using OpenFF;\n" +
				"using OpenFF.Events;\n" +
				"\n" +
				"namespace " + name + "\n" +
				"{\n" +
				"\tpublic class " + name + "Service : GameService\n" +
				"\t{\n" +
				"\t\tpublic int MapsEntered;\n" +
				"\t\tprivate IDisposable _maps;\n" +
				"\n" +
				"\t\tpublic override void OnGameStart()\n" +
				"\t\t{\n" +
				"\t\t\tGame.Log(\"" + title + ": started\");\n" +
				"\t\t\t_maps = Game.Events.Subscribe<MapEntered>(e =>\n" +
				"\t\t\t{\n" +
				"\t\t\t\tMapsEntered++;\n" +
				"\t\t\t\tGame.Log(\"" + title + ": entered \" + e.Scene.Name);\n" +
				"\t\t\t\t// For example: a villager beside the hero who greets the player.\n" +
				"\t\t\t\t// Npc villager = Game.Npcs.Spawn(\"n011\", Game.Hero.Position + new Vector3(2, 0, 0));\n" +
				"\t\t\t\t// villager.Interacted += npc => Game.Dialogue.Say(\"Hello!\");\n" +
				"\t\t\t});\n" +
				"\t\t}\n" +
				"\n" +
				"\t\tpublic override IEnumerable<string> DebugLines()\n" +
				"\t\t{\n" +
				"\t\t\tyield return \"maps entered \" + MapsEntered;\n" +
				"\t\t}\n" +
				"\t}\n" +
				"\n" +
				"\t/// <summary>A sign: says its text when the hero comes near. Attach it to a map character or the map in Crystal (a map's inspector, Behaviours).</summary>\n" +
				"\tpublic class Sign : Behaviour\n" +
				"\t{\n" +
				"\t\t/// <summary>What it says.</summary>\n" +
				"\t\tpublic string Text = \"Hello from " + title + ".\";\n" +
				"\t\t/// <summary>How close the hero must come, in world units (two characters side by side are about 8 apart). 0 says it once when the map opens.</summary>\n" +
				"\t\tpublic float Radius = 12f;\n" +
				"\t\t/// <summary>Frames between repeats while the hero stays near.</summary>\n" +
				"\t\tpublic int Cooldown = 600;\n" +
				"\t\tprivate long _last = -1000000;   // long enough ago; not MinValue, which overflows the subtraction\n" +
				"\n" +
				"\t\tprotected override void Update()\n" +
				"\t\t{\n" +
				"\t\t\tif (Game.Dialogue.IsOpen || !Game.Hero.Present || Game.Time.Frame - _last < Cooldown) return;\n" +
				"\t\t\t// Where the sign stands: the character it is on, or the object's own position.\n" +
				"\t\t\tMapObject link = GetComponent<MapObject>();\n" +
				"\t\t\tVector3 at = link != null && link.Npc != null ? link.Npc.Position : Transform.Position;\n" +
				"\t\t\tif (Radius > 0 && Vector3.FlatDistance(at, Game.Hero.Position) > Radius) return;\n" +
				"\t\t\t_last = Game.Time.Frame;\n" +
				"\t\t\tif (link != null && link.Npc != null) link.Npc.LookAt(Game.Hero.Position);\n" +
				"\t\t\tGame.Dialogue.Say(Text);\n" +
				"\t\t\tif (Radius <= 0) Enabled = false;\n" +
				"\t\t}\n" +
				"\t}\n" +
				"}\n";
		}

		/// <summary>A new file's text: a Behaviour, a GameService, or just the frame.</summary>
		private static string StarterFile(string ns, string className, string template)
		{
			string head = "using System;\n" +
				"using System.Collections.Generic;\n" +
				"using OpenFF;\n" +
				"using OpenFF.Events;\n" +
				"\n" +
				"namespace " + ns + "\n" +
				"{\n";
			switch ((template ?? "behaviour").Trim().ToLowerInvariant())
			{
				case "service":
					return head +
						"\t/// <summary>" + className + ": one instance for the whole run; it hears the game's events and acts through Game.*.</summary>\n" +
						"\tpublic class " + className + " : GameService\n" +
						"\t{\n" +
						"\t\tpublic override void OnGameStart()\n" +
						"\t\t{\n" +
						"\t\t\tGame.Log(\"" + className + ": started\");\n" +
						"\t\t\tGame.Events.Subscribe<MapEntered>(e => Game.Log(\"" + className + ": entered \" + e.Scene.Name));\n" +
						"\t\t}\n" +
						"\t}\n" +
						"}\n";
				case "empty":
					return head + "}\n";
				case "menu":
					return head +
						"\t/// <summary>" + className + ": a script on a menu screen of the mod's own (menus/<id>.json). Attach it in Crystal (the Menus tab, a frame's or the screen's Behaviours).</summary>\n" +
						"\tpublic class " + className + " : MenuBehaviour\n" +
						"\t{\n" +
						"\t\t/// <summary>Public fields show up in Crystal as editable.</summary>\n" +
						"\t\tpublic string Greeting = \"Hello\";\n" +
						"\n" +
						"\t\t/// <summary>The screen has been built: write its texts (Menu.SetText, Menu.Widget(id)).</summary>\n" +
						"\t\tpublic override void OnOpen()\n" +
						"\t\t{\n" +
						"\t\t\tif (Widget != null) Widget.Text = Greeting;\n" +
						"\t\t}\n" +
						"\n" +
						"\t\t/// <summary>Confirm on the frame this is on (or, on the screen, on any frame - Menu.Focused says which). Return true when handled.</summary>\n" +
						"\t\tpublic override bool OnPress()\n" +
						"\t\t{\n" +
						"\t\t\tGame.Log(\"" + className + ": pressed \" + Menu.Focused);\n" +
						"\t\t\tMenu.SoundDecide();\n" +
						"\t\t\treturn true;\n" +
						"\t\t}\n" +
						"\t}\n" +
						"}\n";
				default:
					return head +
						"\t/// <summary>" + className + ": a script for one object. Attach it in Crystal (a map's inspector, Behaviours) or with AddComponent.</summary>\n" +
						"\tpublic class " + className + " : Behaviour\n" +
						"\t{\n" +
						"\t\t/// <summary>Public fields show up in Crystal as editable, and survive a hot reload.</summary>\n" +
						"\t\tpublic float Speed = 1f;\n" +
						"\n" +
						"\t\tprotected override void Start()\n" +
						"\t\t{\n" +
						"\t\t\tGame.Log(\"" + className + " on \" + GameObject.Name);\n" +
						"\t\t}\n" +
						"\n" +
						"\t\tprotected override void Update()\n" +
						"\t\t{\n" +
						"\t\t}\n" +
						"\t}\n" +
						"}\n";
			}
		}
	}
}
