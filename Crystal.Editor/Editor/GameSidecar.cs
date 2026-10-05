// crystal-installs.json in a Steam or GOG copy's folder: what Crystal has installed into that copy, kept beside the game.
//
// The record that makes an install undoable (ModInstall: <edits>.backup/installed.json and the originals) lives with the
// edits, in the project. That is where it belongs - a mod is its project - but it means a project deleted while installed
// takes with it the only word on which of the game's files are its. So every time a record is saved its gist is written
// here too: the project, where its edits and backups are, and each file written with its hash. The game reads only the
// files it knows and passes this one over; Steam's Verify integrity leaves it alone. The Mods tab reads it to name an
// install whose project has gone (and uninstall it, while the backups are still where the record said), and the game
// file check counts its files as that install's, not as files another tool changed.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Crystal.Editor
{
	internal static class GameSidecar
	{
		public const string FileName = "crystal-installs.json";

		public sealed class Entry
		{
			/// <summary>The project's name, or null for the edits made with no project open.</summary>
			public string Name { get; set; }
			public string Project { get; set; }
			public string Edits { get; set; }
			public string Backup { get; set; }
			public string Updated { get; set; }
			public List<InstalledFile> Files { get; set; } = new List<InstalledFile>();
		}

		private sealed class Sheet
		{
			public string Note { get; set; } = "What Crystal (the OpenFF editor) installed into this copy of the game. Crystal's start page > Mods uninstalls it.";
			public List<Entry> Installs { get; set; } = new List<Entry>();
		}

		private static readonly JsonSerializerOptions Json = new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

		public static List<Entry> Read(string gameDirectory)
		{
			string path = Path.Combine(gameDirectory, FileName);
			try { return File.Exists(path) ? JsonSerializer.Deserialize<Sheet>(File.ReadAllText(path))?.Installs ?? new List<Entry>() : new List<Entry>(); }
			catch (Exception) { return new List<Entry>(); }
		}

		/// <summary>An install's record as saved (ModInstall), into the game's sheet: its entry replaced, or taken out when nothing is installed any more.</summary>
		public static void Record(Workspace workspace, IEnumerable<InstalledFile> files)
		{
			try
			{
				List<Entry> installs = Read(workspace.ContentDirectory);
				installs.RemoveAll(e => Same(e.Edits, workspace.OverrideDirectory));
				List<InstalledFile> list = files.OrderBy(f => f.Name).ToList();
				if (list.Count > 0)
				{
					(string name, string project) = ProjectOf(workspace.OverrideDirectory);
					installs.Add(new Entry
					{
						Name = name, Project = project, Edits = workspace.OverrideDirectory, Backup = ModInstall.BackupDirectory(workspace),
						Updated = DateTime.Now.ToString("yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture), Files = list,
					});
				}
				Write(workspace.ContentDirectory, installs);
			}
			catch (Exception ex) { Console.Error.WriteLine("  install   {0} in {1}: {2}", FileName, workspace.ContentDirectory, ex.Message); }
		}

		/// <summary>An install taken off the sheet without undoing it - one whose backups have gone, left for Steam's Verify to mend.</summary>
		public static void Forget(string gameDirectory, string edits)
		{
			List<Entry> installs = Read(gameDirectory);
			if (installs.RemoveAll(e => Same(e.Edits, edits)) > 0) Write(gameDirectory, installs);
		}

		private static void Write(string gameDirectory, List<Entry> installs)
		{
			string path = Path.Combine(gameDirectory, FileName);
			if (installs.Count == 0) { if (File.Exists(path)) File.Delete(path); return; }
			File.WriteAllText(path, JsonSerializer.Serialize(new Sheet { Installs = installs }, Json));
		}

		/// <summary>The project whose edits these are: project.json in a folder above them (files/, or targets/&lt;target&gt;/files/).</summary>
		private static (string Name, string Directory) ProjectOf(string edits)
		{
			string at = Path.GetDirectoryName(edits.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
			for (int i = 0; i < 4 && !string.IsNullOrEmpty(at); i++, at = Path.GetDirectoryName(at))
			{
				if (File.Exists(Path.Combine(at, "project.json")))
				{
					Project project = Project.TryOpen(at);
					return (project?.File.Name ?? Path.GetFileName(at), at);
				}
			}
			return (null, null);
		}

		public static bool Same(string a, string b)
		{
			if (a == null || b == null) return false;
			string Norm(string p) => Path.GetFullPath(p).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			try { return string.Equals(Norm(a), Norm(b), StringComparison.OrdinalIgnoreCase); } catch (Exception) { return false; }
		}
	}
}
