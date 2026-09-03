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

namespace FF3.ContentTool.Editor
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

		private static void Add(ZipArchive zip, string file, string entryName)
		{
			zip.CreateEntryFromFile(file, entryName, CompressionLevel.Optimal);
		}

		private static string Readme(Project project, List<string> contents)
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
