// Runtime access to the game's content, through the same chain the editor uses.
//
// Shared/Content/ContentChain.cs answers "where does this file come from": the mods in
// front (a project's edits, a downloaded mod folder, the legacy Content/Override), then
// the shipped content in whatever shape it is - our data*.bin archives, a Steam FF3
// install's loose files/, a Steam FF4 install's files plus mass files. So the client
// boots from the game people bought as readily as from our archives, and a project made
// in the editor is a mod here with no copy step.
//
//   --content=<dir>[;<dir>]    our Content, or a Steam install; more directories are
//                              asked in turn for what the first lacks. Nothing is added
//                              behind a Steam install on its own: the client has to run
//                              from the game people bought and nothing else.
//   --mod=<dir>[;<dir>...]     mod folders mirroring the game's names, first wins
//   --project=<name|dir>       an editor project: its edits are the mods
//   --content-override=<dir>   the old single override directory; still honoured
//
// The names are the game's own, "files/d01_01.script", "sound/BGM01.dat".

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FF3.Content;

namespace FF3
{
	internal static class GameArchive
	{
		private static ContentChain _chain;

		public static bool IsLoaded => _chain != null;

		/// <summary>Where the shipped content was opened from. "Content" until Load has run.</summary>
		public static string DataPath { get; private set; } = "Content";

		/// <summary>The first override directory, or null if there is none.</summary>
		public static string OverrideDirectory => _chain?.Overrides.FirstOrDefault();

		/// <summary>The chain itself, for anything that wants to list or probe.</summary>
		public static ContentChain Chain => _chain;

		/// <summary>"ff3" or "ff4", from the shape of the content.</summary>
		public static string Game => _chain?.Game ?? "ff3";

		/// <summary>Opens the content. Returns false, with the reason logged, if it cannot.</summary>
		public static bool Load()
		{
			string root = ContentLocator.FindDataRoot();
			if (root == null)
			{
				Log.Write(LogChannel.File, "no content found: no Content/data000.bin and no game install");
				return false;
			}
			DataPath = root;
			try
			{
				_chain = ContentChain.Open(root, Overrides(root));
				foreach (string fallback in Fallbacks(root))
				{
					_chain.AddFallback(fallback);
				}
				// Last of all, what the program makes itself: the two movement tuning tables
				// the FF3 logic reads at start-up, which FF4 compiled into its executable.
				// The install always wins; nothing of either game's is shipped for this.
				MovementDefaults.Register(_chain);
			}
			catch (Exception ex)
			{
				Log.Write(LogChannel.File, "content could not be opened: " + ex.Message);
				_chain = null;
				return false;
			}
			_chain.OnOverrideUsed = (name, path) => Log.Write(LogChannel.File, "override in use: " + name);
			Log.Write(LogChannel.File, "content: " + _chain.Describe());
			// So Crystal can find this client's mods folder.
			Launch.RecordClient();
			return true;
		}

		/// <summary>
		/// The content roots behind the first one: the rest of --content's list, in order.
		/// Nothing is added on its own - a Steam install has to be enough by itself, and
		/// mixing content is something a person asks for explicitly.
		/// </summary>
		private static IEnumerable<string> Fallbacks(string root)
		{
			List<string> roots = new List<string>();
			string configured = Options.Get("content");
			if (!string.IsNullOrEmpty(configured))
			{
				roots.AddRange(configured.Split(';').Select(r => r.Trim().Trim('"')).Where(r => r.Length > 0).Skip(1));
			}
			return roots.Where(ContentChain.Looks);
		}

		/// <summary>The override directories the options ask for, most specific first.</summary>
		private static IEnumerable<string> Overrides(string root)
		{
			List<string> directories = new List<string>();

			string project = Options.Get("project");
			if (!string.IsNullOrEmpty(project))
			{
				string directory = project.IndexOfAny(new[] { '/', '\\' }) >= 0 || Path.IsPathRooted(project)
					? project
					: Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
						"FF3ContentTool", "projects", project);
				// A project targeting several games keeps ours under targets/ours/files;
				// one targeting one game, or an older project, keeps it in files/.
				directories.Add(Path.Combine(directory, "targets", "ours", "files"));
				directories.Add(Path.Combine(directory, "files"));
				if (!Directory.Exists(directory))
				{
					Log.Write(LogChannel.File, "project not found: " + directory);
				}
			}

			string mods = Options.Get("mod");
			if (!string.IsNullOrEmpty(mods))
			{
				directories.AddRange(mods.Split(';').Select(m => m.Trim().Trim('"')).Where(m => m.Length > 0));
			}

			// The mods folder beside the executable, in load order (Shared/Content/Mods.cs):
			// after what the command line named, before the legacy Override directory.
			directories.AddRange(ModsFolderOverrides(root));

			string legacy = Options.Get("content-override");
			directories.Add(string.IsNullOrEmpty(legacy) ? Path.Combine(root, "Override") : legacy);

			foreach (string directory in directories.Where(Directory.Exists))
			{
				int count = Directory.GetFiles(directory, "*", SearchOption.AllDirectories).Length;
				Log.Write(LogChannel.File, string.Format("content overrides: {0} loose file(s) in {1}", count, directory));
			}
			return directories;
		}

		/// <summary>The mods the mods folder enabled, in load order, for the engine to load code from.</summary>
		public static IReadOnlyList<InstalledMod> ActiveMods { get; private set; } = new List<InstalledMod>();

		/// <summary>
		/// The enabled mods of mods/ beside the executable that target OpenFF, as their
		/// files directories in load order. Logs what was taken and which files more than
		/// one carries, and writes loadorder.json back so a folder dropped in by hand
		/// appears there, enabled, at the end.
		/// </summary>
		private static IEnumerable<string> ModsFolderOverrides(string root)
		{
			string folder = ModsFolder.Beside(AppContext.BaseDirectory);
			List<InstalledMod> installed = ModsFolder.Load(folder);
			if (installed.Count == 0)
			{
				return Enumerable.Empty<string>();
			}
			List<InstalledMod> active = ModsFolder.Active(installed);
			Log.Write(LogChannel.General, "mods: " + active.Count + " of " + installed.Count + " in " + folder + " apply"
				+ (active.Count > 0 ? ": " + string.Join(", ", active.Select(m => m.DisplayName + " (" + ModsFolder.FileCount(m) + " files)")) : ""));
			foreach (InstalledMod mod in installed.Where(m => !active.Contains(m)))
			{
				Log.Write(LogChannel.File, "mods: " + mod.DisplayName + " skipped (" + (mod.Skipped ?? "?") + ")");
			}
			ActiveMods = active;
			foreach (KeyValuePair<string, List<InstalledMod>> conflict in ModsFolder.Conflicts(active))
			{
				Log.Write(LogChannel.General, "mods: " + conflict.Key + " in " + string.Join(", ", conflict.Value.Select(m => m.DisplayName)) + " - " + conflict.Value[0].DisplayName + " wins");
			}
			try
			{
				ModsFolder.SaveOrder(folder, installed);
			}
			catch (Exception ex)
			{
				Log.Write(LogChannel.General, "mods: loadorder.json not written: " + ex.Message);
			}
			return active.Where(m => Directory.Exists(m.FilesDirectory)).Select(m => m.FilesDirectory);
		}

		/// <summary>Reads one file by name, or null if there is no such file anywhere.</summary>
		public static byte[] Read(string filename)
		{
			if (_chain == null || string.IsNullOrEmpty(filename))
			{
				return null;
			}
			try
			{
				return _chain.Read(filename);
			}
			catch (Exception ex)
			{
				Log.Write(LogChannel.File, "read failed for " + filename + ": " + ex.Message);
				return null;
			}
		}
	}
}
