// A project as something to hand to somebody: a zip with a README.
//
// What ends up on Nexus is a folder of edited files and a note saying where they go.
// The project already is that folder; this writes it out with its manifest and a
// README that says which game, what changed, and how to put it in - so a modder does
// not assemble the upload by hand and forget the part that says which game it is for.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Crystal.Editor
{
	internal static class ProjectExport
	{
		/// <summary>Writes &lt;projects&gt;/&lt;name&gt;-&lt;version&gt;.zip and returns its path.</summary>
		public static string Write(Project project)
		{
			string version = string.IsNullOrWhiteSpace(project.File.Version) ? "1.0" : project.File.Version.Trim();
			string stem = Safe(project.File.Name) + "-" + Safe(version);
			string zipPath = Path.Combine(Path.GetDirectoryName(project.Directory) ?? project.Directory, stem + ".zip");
			if (File.Exists(zipPath))
			{
				File.Delete(zipPath);
			}

			using (ZipArchive zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
			{
				Add(zip, project.ManifestPath, stem + "/project.json");

				List<string> lines = new List<string>();
				foreach (string target in project.File.Targets)
				{
					string files = project.FilesFor(target);
					if (!Directory.Exists(files))
					{
						continue;
					}
					// One folder per game inside the zip, named the way the project keeps them.
					string folder = stem + "/" + (project.File.Targets.Count == 1 ? "files" : "targets/" + target + "/files");
					int count = 0;
					foreach (string file in Directory.EnumerateFiles(files, "*", SearchOption.AllDirectories))
					{
						string relative = Path.GetRelativePath(files, file).Replace('\\', '/');
						Add(zip, file, folder + "/" + relative);
						count++;
					}
					lines.Add(string.Format(CultureInfo.InvariantCulture, "- {0}: {1} file(s) under {2}",
						Targets.Describe(target), count, folder.Substring(stem.Length + 1)));
				}

				ZipArchiveEntry readme = zip.CreateEntry(stem + "/README.md");
				using Stream stream = readme.Open();
				byte[] text = new UTF8Encoding(false).GetBytes(Readme(project, lines));
				stream.Write(text, 0, text.Length);
			}
			return zipPath;
		}

		/// <summary>
		/// Writes the project's OpenFF files as a mod in the client's mods folder:
		/// mods/&lt;name&gt;/mod.json, ff3/files/ and ff4/files/ (one per OpenFF target the
		/// project has - the two games name their files alike, so the client applies only
		/// the booted game's), and a README. Replaces an earlier export of the same name
		/// (recognised by its mod.json); refuses to touch a folder that is not one.
		/// Returns the mod's directory.
		/// </summary>
		public static string WriteToOpenFF(Project project, string modsFolder)
		{
			List<string> ours = project.File.Targets.Where(Targets.IsOurs).ToList();
			if (ours.Count == 0)
			{
				throw new InvalidOperationException("the project is not an OpenFF mod - tick FF3 or FF4 under OpenFF in Project settings first");
			}
			string key = Safe(project.File.Name);
			string directory = Path.Combine(modsFolder, key);
			string manifestPath = Path.Combine(directory, OpenFF.Content.ModsFolder.ManifestName);
			if (Directory.Exists(directory))
			{
				if (!File.Exists(manifestPath))
				{
					throw new IOException(directory + " exists and is not a mod exported before (no mod.json) - move it away first");
				}
				// Every files folder an export writes or wrote: the per-game ones, and the
				// shared files/ an export before 2026-09-07 put the FF3 edits in.
				foreach (string old in OpenFF.Content.ModsFolder.Games.Select(g => Path.Combine(directory, g)).Append(Path.Combine(directory, "files")))
				{
					if (Directory.Exists(old))
					{
						Directory.Delete(old, recursive: true);
					}
				}
			}
			Directory.CreateDirectory(directory);
			List<string> contents = new List<string>();
			foreach (string target in ours)
			{
				string source = project.FilesFor(target);
				string files = Path.Combine(directory, Targets.GameOf(target), "files");
				int count = 0;
				if (Directory.Exists(source))
				{
					foreach (string file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
					{
						string relative = Path.GetRelativePath(source, file);
						string destination = Path.Combine(files, relative);
						Directory.CreateDirectory(Path.GetDirectoryName(destination));
						File.Copy(file, destination, overwrite: true);
						count++;
					}
				}
				contents.Add(string.Format(CultureInfo.InvariantCulture, "- {0}: {1} file(s) under {2}/files/",
					Targets.Describe(target), count, Targets.GameOf(target)));
			}
			// The code, when the project has some and it has been built: the assemblies and
			// their symbols, at the mod's root, named in mod.json.
			List<string> assemblies = new List<string>();
			foreach (string dll in ModCode.Assemblies(project))
			{
				File.Copy(dll, Path.Combine(directory, Path.GetFileName(dll)), overwrite: true);
				string pdb = Path.ChangeExtension(dll, ".pdb");
				if (File.Exists(pdb))
				{
					File.Copy(pdb, Path.Combine(directory, Path.GetFileName(pdb)), overwrite: true);
				}
				assemblies.Add(Path.GetFileName(dll));
			}
			// The scene files: behaviours the editor put on map objects.
			string scenesOut = Path.Combine(directory, ProjectScenes.FolderName);
			if (Directory.Exists(scenesOut))
			{
				Directory.Delete(scenesOut, recursive: true);
			}
			int scenes = 0;
			foreach (ProjectScenes.Summary map in ProjectScenes.Maps(project))
			{
				Directory.CreateDirectory(scenesOut);
				File.Copy(Path.Combine(ProjectScenes.Directory(project), map.Map + ".json"), Path.Combine(scenesOut, map.Map + ".json"), overwrite: true);
				scenes++;
			}
			// The definitions: the mod's own items (defs/items), composed into the game's tables
			// by the client as it reads them.
			string defsOut = Path.Combine(directory, "defs");
			if (Directory.Exists(defsOut))
			{
				Directory.Delete(defsOut, recursive: true);
			}
			string defsIn = Path.Combine(project.Directory, "defs");
			if (Directory.Exists(defsIn))
			{
				foreach (string file in Directory.EnumerateFiles(defsIn, "*.json", SearchOption.AllDirectories))
				{
					string relative = file.Substring(defsIn.Length).TrimStart(Path.DirectorySeparatorChar);
					string target = Path.Combine(defsOut, relative);
					Directory.CreateDirectory(Path.GetDirectoryName(target));
					File.Copy(file, target, overwrite: true);
				}
			}
			// The mod's own models (assets/*.glb and what a .gltf brings along), which the client
			// draws directly - the OpenFF target's own formats.
			string assetsOut = Path.Combine(directory, GltfBundle.Folder);
			if (Directory.Exists(assetsOut)) Directory.Delete(assetsOut, recursive: true);
			string assetsIn = Path.Combine(project.Directory, GltfBundle.Folder);
			if (Directory.Exists(assetsIn))
			{
				foreach (string file in Directory.EnumerateFiles(assetsIn, "*.*", SearchOption.AllDirectories))
				{
					string relative = file.Substring(assetsIn.Length).TrimStart(Path.DirectorySeparatorChar);
					string target = Path.Combine(assetsOut, relative);
					Directory.CreateDirectory(Path.GetDirectoryName(target));
					File.Copy(file, target, overwrite: true);
				}
			}
			// The mod's own textures at full size (textures/<name>.png), which the client draws in
			// place of the game's - the package copies alongside are the Steam game's downsized ones.
			string texturesOut = Path.Combine(directory, "textures");
			if (Directory.Exists(texturesOut)) Directory.Delete(texturesOut, recursive: true);
			string texturesIn = Path.Combine(project.Directory, "textures");
			int pictures = 0;
			if (Directory.Exists(texturesIn))
			{
				Directory.CreateDirectory(texturesOut);
				foreach (string file in Directory.EnumerateFiles(texturesIn, "*.png", SearchOption.TopDirectoryOnly))
				{
					File.Copy(file, Path.Combine(texturesOut, Path.GetFileName(file)), overwrite: true);
					pictures++;
				}
			}
			if (pictures > 0) contents.Add(string.Format(CultureInfo.InvariantCulture, "- textures: {0} PNG(s) drawn in place of the game's, at their own size, under textures/", pictures));
			OpenFF.Content.ModsFolder.WriteManifest(manifestPath, new OpenFF.Content.ModManifest
			{
				Id = key,
				Name = string.IsNullOrWhiteSpace(project.File.Name) ? key : project.File.Name.Trim(),
				Version = string.IsNullOrWhiteSpace(project.File.Version) ? "1.0" : project.File.Version.Trim(),
				Author = project.File.Author,
				Description = project.File.Description,
				Target = OpenFF.Content.ModManifest.TargetOpenFF,
				Assemblies = assemblies,
			});
			if (assemblies.Count > 0)
			{
				contents.Add("- code: " + string.Join(", ", assemblies));
			}
			if (scenes > 0)
			{
				contents.Add(string.Format(CultureInfo.InvariantCulture, "- scenes: {0} map(s) with behaviours attached under scenes/", scenes));
			}
			File.WriteAllText(Path.Combine(directory, "README.md"), Readme(project, contents, openff: true), new UTF8Encoding(false));
			return directory;
		}

		private static void Add(ZipArchive zip, string file, string entryName)
		{
			zip.CreateEntryFromFile(file, entryName, CompressionLevel.Optimal);
		}

		private static string Readme(Project project, List<string> contents, bool openff = false)
		{
			StringBuilder text = new StringBuilder();
			text.Append("# ").Append(project.File.Name).Append('\n');
			if (!string.IsNullOrWhiteSpace(project.File.Version))
			{
				text.Append("Version ").Append(project.File.Version.Trim());
				if (!string.IsNullOrWhiteSpace(project.File.Author))
				{
					text.Append(" by ").Append(project.File.Author.Trim());
				}
				text.Append("\n");
			}
			text.Append('\n');
			if (!string.IsNullOrWhiteSpace(project.File.Description))
			{
				text.Append(project.File.Description.Trim()).Append("\n\n");
			}
			text.Append("## What is in it\n\n");
			foreach (string line in contents)
			{
				text.Append(line).Append('\n');
			}
			text.Append('\n');
			text.Append("## Installing\n\n");
			if (openff)
			{
				text.Append("An OpenFF mod: put this folder under `mods/` beside `OpenFF.exe` and start the client;\n");
				text.Append("the title screen's MODS entry enables, disables and orders mods. `ff3/files/` applies\n");
				text.Append("when FF3 is played, `ff4/files/` when FF4 is; `scenes/` and the assemblies apply to both.\n");
				return text.ToString();
			}
			text.Append("Made with Crystal, the OpenFF editor. Open Crystal, choose File > Open project, and\n");
			text.Append("point it at this folder; then Project > Install into the game. Crystal keeps a backup of\n");
			text.Append("every file it replaces and Project > Remove puts the originals back.\n\n");
			text.Append("By hand: copy the files under `files/` over the game's own, keeping the folder\n");
			text.Append("structure. FF3 on Steam reads `files/` beside the executable; FF4 on Steam reads\n");
			text.Append("`EXTRACTED_DATA/files/`. Files that live inside a `.dat` mass file in FF4 cannot be\n");
			text.Append("copied by hand - use Crystal for those.\n");
			return text.ToString();
		}

		/// <summary>Opens a folder, or a folder with a file selected, in the file manager.</summary>
		public static void Reveal(string path)
		{
			if (!OperatingSystem.IsWindows())
			{
				throw new PlatformNotSupportedException("reveal is Windows-only for now");
			}
			ProcessStartInfo start = File.Exists(path)
				? new ProcessStartInfo("explorer.exe", "/select,\"" + path + "\"")
				: new ProcessStartInfo("explorer.exe", "\"" + path + "\"");
			start.UseShellExecute = false;
			Process.Start(start);
		}

		private static string Safe(string name)
		{
			char[] bad = Path.GetInvalidFileNameChars();
			string cleaned = new string((name ?? "project").Trim().Select(c => bad.Contains(c) || c == ' ' ? '-' : c).ToArray());
			return cleaned.Length == 0 ? "project" : cleaned;
		}
	}
}
