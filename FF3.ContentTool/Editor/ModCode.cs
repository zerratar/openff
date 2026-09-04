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

namespace FF3.ContentTool.Editor
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

		/// <summary>Builds the code with the .NET SDK. Returns whether it succeeded and what the build said.</summary>
		public static bool Build(Project project, out string output)
		{
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
				output = Tidy(text.ToString());
				return process.ExitCode == 0;
			}
			catch (Exception ex)
			{
				output = "dotnet could not be started (" + ex.Message + ") - is the .NET SDK installed?";
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
				"}\n";
		}
	}
}
