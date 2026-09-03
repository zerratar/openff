// Putting a mod into a Steam install, and taking it back out.
//
// Our own build reads the override directory itself, so there is nothing to install:
// a change is live the next time the game starts. The Steam executable has no such
// idea - it reads files/ and that is all - so for that one the edits have to be copied
// over the originals.
//
// Which makes this the one operation in the tool that writes somewhere the person did
// not choose, so it is built to be undone:
//
//   - the original of every file it overwrites is copied out first, once, to a backup
//     beside the override directory, and a file already backed up is never backed up
//     again - the first copy is the pristine one and a second install must not replace
//     it with modded bytes;
//   - what was written is recorded by hash, and uninstall only restores a file that
//     still holds exactly those bytes. If the game has been updated or verified since,
//     the file is left alone and said so, rather than a stale original being put back
//     over a newer one;
//   - a file the mod adds, that the install did not have, is recorded as new and
//     removed on uninstall rather than restored.
//
// The override directory stays the master copy throughout. That is deliberate: it
// means a mod is a directory that can be zipped and handed to somebody, and that
// Steam validating its own files costs the work rather than the mod.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;

namespace FF3.ContentTool.Editor
{
	internal sealed class InstalledFile
	{
		/// <summary>Content name, as the game asks for it.</summary>
		public string Name { get; set; }

		/// <summary>Hash of what we wrote, so we know it is still ours to take back.</summary>
		public string Wrote { get; set; }

		/// <summary>Whether the install had this file before. If not, uninstall deletes it.</summary>
		public bool Replaced { get; set; }
	}

	internal sealed class ModStatus
	{
		public string Kind { get; set; }
		public string Content { get; set; }
		public string Override { get; set; }
		public string Backup { get; set; }
		public bool CanInstall { get; set; }
		public string Why { get; set; }

		/// <summary>Files in the override directory.</summary>
		public List<string> Edited { get; set; } = new List<string>();

		/// <summary>Of those, the ones currently in the install as we wrote them.</summary>
		public List<string> Installed { get; set; } = new List<string>();

		/// <summary>Edited but not installed, or installed and since edited again.</summary>
		public List<string> Pending { get; set; } = new List<string>();

		/// <summary>Installed once, but what is there now is not what we wrote.</summary>
		public List<string> Changed { get; set; } = new List<string>();
	}

	internal sealed class ModResult
	{
		public bool Ok { get; set; }
		public string Error { get; set; }
		public List<string> Wrote { get; set; } = new List<string>();
		public List<string> Restored { get; set; } = new List<string>();
		public List<string> Removed { get; set; } = new List<string>();
		public List<string> Skipped { get; set; } = new List<string>();

		/// <summary>Edits thrown away - the file is back to what the game shipped.</summary>
		public List<string> Reverted { get; set; } = new List<string>();

		public List<string> Notes { get; set; } = new List<string>();
	}

	internal static class ModInstall
	{
		private static readonly JsonSerializerOptions Json =
			new JsonSerializerOptions { WriteIndented = true };

		/// <summary>Beside the override directory, never inside it - a mod is what you publish.</summary>
		public static string BackupDirectory(Workspace workspace)
		{
			return workspace.OverrideDirectory.TrimEnd(
				Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + ".backup";
		}

		private static string ManifestPath(Workspace workspace)
		{
			return Path.Combine(BackupDirectory(workspace), "installed.json");
		}

		private static Dictionary<string, InstalledFile> Manifest(Workspace workspace)
		{
			string path = ManifestPath(workspace);
			if (!File.Exists(path))
			{
				return new Dictionary<string, InstalledFile>(StringComparer.OrdinalIgnoreCase);
			}
			try
			{
				List<InstalledFile> list = JsonSerializer.Deserialize<List<InstalledFile>>(
					File.ReadAllText(path)) ?? new List<InstalledFile>();
				return list.Where(f => f?.Name != null).ToDictionary(
					f => f.Name, f => f, StringComparer.OrdinalIgnoreCase);
			}
			catch (Exception)
			{
				// A manifest we cannot read is treated as nothing installed. Uninstall
				// then refuses to touch anything, which is the safe way round.
				return new Dictionary<string, InstalledFile>(StringComparer.OrdinalIgnoreCase);
			}
		}

		private static void SaveManifest(Workspace workspace, Dictionary<string, InstalledFile> files)
		{
			Directory.CreateDirectory(BackupDirectory(workspace));
			File.WriteAllText(ManifestPath(workspace),
				JsonSerializer.Serialize(files.Values.OrderBy(f => f.Name).ToList(), Json));
		}

		private static string Hash(byte[] data)
		{
			return Convert.ToHexString(SHA1.HashData(data)).ToLowerInvariant();
		}

		private static string HashOf(string path)
		{
			return File.Exists(path) ? Hash(File.ReadAllBytes(path)) : null;
		}

		/// <summary>Every file in the override directory, as content names.</summary>
		private static List<string> Edits(Workspace workspace)
		{
			string root = workspace.OverrideDirectory;
			if (!Directory.Exists(root))
			{
				return new List<string>();
			}
			return Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
				.Select(p => p.Substring(root.Length)
					.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
					.Replace(Path.DirectorySeparatorChar, '/'))
				// A README dropped in the override directory is not game content.
				.Where(n => !n.Equals("README.md", StringComparison.OrdinalIgnoreCase))
				.OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
				.ToList();
		}

		/// <summary>Where a content name lives inside the install.</summary>
		private static string InstallPath(Workspace workspace, string name)
		{
			string root = Path.GetFullPath(workspace.ContentDirectory);
			string full = Path.GetFullPath(Path.Combine(root, name));
			if (!full.StartsWith(root + Path.DirectorySeparatorChar,
					StringComparison.OrdinalIgnoreCase))
			{
				throw new ArgumentException("that name points outside the install: " + name);
			}
			return full;
		}

		private static string BackupPath(Workspace workspace, string name)
		{
			string root = Path.GetFullPath(BackupDirectory(workspace));
			string full = Path.GetFullPath(Path.Combine(root, "files", name));
			if (!full.StartsWith(root + Path.DirectorySeparatorChar,
					StringComparison.OrdinalIgnoreCase))
			{
				throw new ArgumentException("that name points outside the backup: " + name);
			}
			return full;
		}

		/// <summary>What is edited, what is installed, and whether installing is possible.</summary>
		public static ModStatus Status(Workspace workspace)
		{
			ModStatus status = new ModStatus
			{
				Kind = workspace.Kind,
				Content = workspace.ContentDirectory,
				Override = workspace.OverrideDirectory,
				Backup = BackupDirectory(workspace),
				Edited = Edits(workspace)
			};

			if (workspace.Kind != "loose files")
			{
				status.CanInstall = false;
				status.Why = "this build reads the override directory itself, "
					+ "so there is nothing to install";
				return status;
			}

			status.CanInstall = true;
			Dictionary<string, InstalledFile> manifest = Manifest(workspace);

			foreach (string name in status.Edited)
			{
				string mine = HashOf(Path.Combine(workspace.OverrideDirectory,
					name.Replace('/', Path.DirectorySeparatorChar)));
				string live = HashOf(InstallPath(workspace, name));

				if (live != null && live == mine)
				{
					status.Installed.Add(name);
				}
				else if (manifest.TryGetValue(name, out InstalledFile record)
					&& live != null && live != record.Wrote)
				{
					// Installed once; something else has written it since.
					status.Changed.Add(name);
				}
				else
				{
					status.Pending.Add(name);
				}
			}

			// Installed earlier, then deleted from the override - still in the game.
			foreach (string name in manifest.Keys)
			{
				if (!status.Edited.Contains(name, StringComparer.OrdinalIgnoreCase)
					&& !status.Installed.Contains(name, StringComparer.OrdinalIgnoreCase))
				{
					status.Installed.Add(name);
				}
			}

			return status;
		}

		/// <summary>Copies the override over the install, keeping the originals.</summary>
		public static ModResult Install(Workspace workspace)
		{
			ModResult result = new ModResult();

			if (workspace.Kind != "loose files")
			{
				result.Error = "this build reads the override directory itself, "
					+ "so there is nothing to install";
				return result;
			}

			List<string> edits = Edits(workspace);
			if (edits.Count == 0)
			{
				result.Error = "nothing in " + workspace.OverrideDirectory + " to install";
				return result;
			}

			Dictionary<string, InstalledFile> manifest = Manifest(workspace);

			foreach (string name in edits)
			{
				string from = Path.Combine(workspace.OverrideDirectory,
					name.Replace('/', Path.DirectorySeparatorChar));
				string to = InstallPath(workspace, name);
				byte[] data = File.ReadAllBytes(from);

				// Whether this replaced something the install shipped. Asked once and
				// then remembered, because after the first install the file exists
				// either way - a second install would otherwise decide that a file the
				// mod added was an original, and uninstall would leave it behind.
				bool known = manifest.TryGetValue(name, out InstalledFile was);
				bool replaced = known ? was.Replaced : File.Exists(to);

				string backup = BackupPath(workspace, name);

				// Once only. A second install must not copy modded bytes over the
				// pristine original that the first one saved.
				if (replaced && !File.Exists(backup) && File.Exists(to))
				{
					Directory.CreateDirectory(Path.GetDirectoryName(backup));
					File.Copy(to, backup);
				}

				Directory.CreateDirectory(Path.GetDirectoryName(to));
				File.WriteAllBytes(to, data);

				manifest[name] = new InstalledFile
				{
					Name = name,
					Wrote = Hash(data),
					Replaced = replaced
				};
				result.Wrote.Add(name);
			}

			SaveManifest(workspace, manifest);
			result.Ok = true;
			return result;
		}

		/// <summary>
		/// Undoes one installed file: the original back, or the file gone if the mod
		/// added it. Shared by uninstall, which does the lot, and revert, which does
		/// the ones that were picked.
		/// </summary>
		private static void TakeBack(Workspace workspace,
			Dictionary<string, InstalledFile> manifest, InstalledFile record, ModResult result)
		{
			string live = InstallPath(workspace, record.Name);
			string here = HashOf(live);

			if (here == null)
			{
				result.Notes.Add(record.Name + " is already gone");
				manifest.Remove(record.Name);
				return;
			}

			// Only take back what is still ours. A game update or a Steam verify puts
			// a newer original there, and restoring over that would undo it.
			if (here != record.Wrote)
			{
				result.Skipped.Add(record.Name);
				return;
			}

			if (!record.Replaced)
			{
				File.Delete(live);
				result.Removed.Add(record.Name);
				manifest.Remove(record.Name);
				return;
			}

			string backup = BackupPath(workspace, record.Name);
			if (!File.Exists(backup))
			{
				result.Skipped.Add(record.Name);
				result.Notes.Add("no backup kept for " + record.Name
					+ " - left as it is; Steam can restore it by verifying the files");
				return;
			}

			File.Copy(backup, live, true);
			result.Restored.Add(record.Name);
			manifest.Remove(record.Name);
		}

		/// <summary>
		/// Throws away the edits to the named files.
		///
		/// Two halves, and both are needed. Deleting the override is what makes the
		/// editor read the shipped file again. But if that edit had been installed, the
		/// game is still holding it - so the original goes back first, by the same rules
		/// uninstall uses, and only then is the edit deleted. Doing just the first half
		/// is the trap: the file would look reverted everywhere except in the game.
		/// </summary>
		public static ModResult Revert(Workspace workspace, IEnumerable<string> names)
		{
			ModResult result = new ModResult();
			List<string> wanted = (names ?? Enumerable.Empty<string>())
				.Where(n => !string.IsNullOrWhiteSpace(n)).Distinct(
					StringComparer.OrdinalIgnoreCase).ToList();
			if (wanted.Count == 0)
			{
				result.Error = "nothing was picked";
				return result;
			}

			bool installable = workspace.Kind == "loose files";
			Dictionary<string, InstalledFile> manifest = installable
				? Manifest(workspace)
				: new Dictionary<string, InstalledFile>(StringComparer.OrdinalIgnoreCase);

			foreach (string name in wanted)
			{
				if (manifest.TryGetValue(name, out InstalledFile record))
				{
					TakeBack(workspace, manifest, record, result);
					// Still in the manifest means it was left alone deliberately, so the
					// edit stays too rather than leaving the game holding bytes nothing
					// in the project explains any more.
					if (manifest.ContainsKey(name))
					{
						continue;
					}
				}

				if (workspace.Revert(name))
				{
					result.Reverted.Add(name);
				}
			}

			if (installable)
			{
				SaveManifest(workspace, manifest);
			}
			if (result.Skipped.Count > 0)
			{
				result.Notes.Add(result.Skipped.Count
					+ " file(s) hold bytes we did not write, so the game was left alone "
					+ "and the edit kept - the game was updated or verified since");
			}
			result.Ok = true;
			return result;
		}

		/// <summary>Puts the originals back, and removes files the mod added.</summary>
		public static ModResult Uninstall(Workspace workspace)
		{
			ModResult result = new ModResult();

			if (workspace.Kind != "loose files")
			{
				result.Error = "nothing was installed - this build reads the override "
					+ "directory itself";
				return result;
			}

			Dictionary<string, InstalledFile> manifest = Manifest(workspace);
			if (manifest.Count == 0)
			{
				result.Error = "no record of anything installed in " + BackupDirectory(workspace);
				return result;
			}

			foreach (InstalledFile record in manifest.Values.OrderBy(f => f.Name).ToList())
			{
				TakeBack(workspace, manifest, record, result);
			}

			SaveManifest(workspace, manifest);
			if (result.Skipped.Count > 0)
			{
				result.Notes.Add(result.Skipped.Count
					+ " file(s) hold bytes we did not write - the game was updated or "
					+ "verified since, so they were left alone");
			}
			result.Ok = true;
			return result;
		}
	}
}
