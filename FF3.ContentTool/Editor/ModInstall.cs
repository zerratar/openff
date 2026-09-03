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

		/// <summary>
		/// The mass file this entry was written into, as a content name
		/// ("files/CAST_SCRIPT.dat"), or null for a loose file. For these, Wrote is the
		/// hash of the whole container as written, shared by every entry in it - the
		/// container is the unit that is compared, backed up and restored.
		/// </summary>
		public string Container { get; set; }

		/// <summary>The entry's own name inside the container, "d01_01.script.lz".</summary>
		public string EntryName { get; set; }

		public bool Compressed { get; set; }

		/// <summary>Hash of the project's bytes this entry was built from.</summary>
		public string EntryHash { get; set; }
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
			string root = Path.GetFullPath(workspace.LooseRoot);
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

			if (!workspace.Installable)
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
				manifest.TryGetValue(name, out InstalledFile record);

				if (workspace.TryLocateInContainer(name, out string container, out _, out _))
				{
					// Inside a mass file. Installed means the container is the one we
					// wrote and it was built with this edit's current bytes.
					string liveContainer = HashOf(InstallPath(workspace, container));
					string mine = HashOf(Path.Combine(workspace.OverrideDirectory,
						name.Replace('/', Path.DirectorySeparatorChar)));
					if (record != null && liveContainer == record.Wrote && record.EntryHash == mine)
					{
						status.Installed.Add(name);
					}
					else if (record != null && liveContainer != null && liveContainer != record.Wrote)
					{
						status.Changed.Add(name);
					}
					else
					{
						status.Pending.Add(name);
					}
					continue;
				}

				string mineLoose = HashOf(Path.Combine(workspace.OverrideDirectory,
					name.Replace('/', Path.DirectorySeparatorChar)));
				string live = HashOf(InstallPath(workspace, name));

				if (live != null && live == mineLoose)
				{
					status.Installed.Add(name);
				}
				else if (record != null && live != null && live != record.Wrote)
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

			if (!workspace.Installable)
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

			// Edits that live inside a mass file are installed a container at a time:
			// every such edit is recorded, then each affected container is rebuilt once
			// from its pristine copy plus everything recorded against it.
			HashSet<string> containers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			List<string> loose = new List<string>();
			foreach (string name in edits)
			{
				if (workspace.TryLocateInContainer(name, out string container, out string entryName, out bool compressed))
				{
					string from = Path.Combine(workspace.OverrideDirectory,
						name.Replace('/', Path.DirectorySeparatorChar));
					manifest[name] = new InstalledFile
					{
						Name = name,
						Container = container,
						EntryName = entryName,
						Compressed = compressed,
						EntryHash = Hash(File.ReadAllBytes(from)),
						Replaced = true
					};
					containers.Add(container);
				}
				else
				{
					loose.Add(name);
				}
			}
			foreach (string container in containers)
			{
				string problem = RebuildContainer(workspace, manifest, container);
				if (problem != null)
				{
					result.Notes.Add(problem);
					continue;
				}
				foreach (InstalledFile record in manifest.Values)
				{
					if (string.Equals(record.Container, container, StringComparison.OrdinalIgnoreCase))
					{
						result.Wrote.Add(record.Name);
					}
				}
			}

			foreach (string name in loose)
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
			if (record.Container != null)
			{
				TakeBackFromContainer(workspace, manifest, record, result);
				return;
			}

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

			bool installable = workspace.Installable;
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

		/// <summary>
		/// Writes a container as: its pristine copy, with every entry the manifest records
		/// against it replaced by the project's current bytes. The pristine copy is taken
		/// the first time and never again. Every record for the container gets the hash
		/// of what was written, so uninstall can tell whether the game has since replaced
		/// it. Returns a note on refusal, null on success.
		/// </summary>
		private static string RebuildContainer(Workspace workspace,
			Dictionary<string, InstalledFile> manifest, string container)
		{
			string live = InstallPath(workspace, container);
			string backup = BackupPath(workspace, container);
			if (!File.Exists(live))
			{
				return container + " is not in the install";
			}

			List<InstalledFile> mine = manifest.Values
				.Where(r => string.Equals(r.Container, container, StringComparison.OrdinalIgnoreCase))
				.ToList();

			// The live container must be either the shipped one or the one we wrote.
			// Anything else means the game changed it under us, and rebuilding from
			// our copy of the original would undo that.
			string here = HashOf(live);
			string wroteBefore = mine.Select(r => r.Wrote).FirstOrDefault(w => w != null);
			if (File.Exists(backup) && wroteBefore != null && here != wroteBefore && here != HashOf(backup))
			{
				return container + " has changed since it was installed into - left alone";
			}

			if (!File.Exists(backup))
			{
				byte[] shipped = File.ReadAllBytes(live);
				if (!Ssam.CanRepack(shipped))
				{
					return container + " is not laid out in a way that can be rewritten - left alone";
				}
				Directory.CreateDirectory(Path.GetDirectoryName(backup));
				File.WriteAllBytes(backup, shipped);
			}

			byte[] pristine = File.ReadAllBytes(backup);
			Dictionary<string, byte[]> replacements = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
			foreach (InstalledFile record in mine)
			{
				string from = Path.Combine(workspace.OverrideDirectory,
					record.Name.Replace('/', Path.DirectorySeparatorChar));
				if (!File.Exists(from))
				{
					continue;                            // reverted: falls back to the pristine entry
				}
				byte[] bytes = File.ReadAllBytes(from);
				replacements[record.EntryName] = record.Compressed ? Lz.Compress(bytes) : bytes;
			}

			byte[] rebuilt = replacements.Count > 0 ? Ssam.Repack(pristine, replacements) : pristine;
			File.WriteAllBytes(live, rebuilt);
			string wrote = Hash(rebuilt);
			foreach (InstalledFile record in mine)
			{
				record.Wrote = wrote;
			}
			return null;
		}

		private static void TakeBackFromContainer(Workspace workspace,
			Dictionary<string, InstalledFile> manifest, InstalledFile record, ModResult result)
		{
			string live = InstallPath(workspace, record.Container);
			string here = HashOf(live);
			if (here == null)
			{
				result.Notes.Add(record.Container + " is already gone");
				manifest.Remove(record.Name);
				return;
			}
			if (here != record.Wrote)
			{
				result.Skipped.Add(record.Name);
				return;
			}

			// Out of the set, then the container is rebuilt from the others that
			// remain - or put back exactly as shipped when this was the last.
			manifest.Remove(record.Name);
			bool others = manifest.Values.Any(r =>
				string.Equals(r.Container, record.Container, StringComparison.OrdinalIgnoreCase));
			if (others)
			{
				string problem = RebuildContainer(workspace, manifest, record.Container);
				if (problem != null)
				{
					result.Notes.Add(problem);
					manifest[record.Name] = record;      // could not take it out after all
					result.Skipped.Add(record.Name);
					return;
				}
			}
			else
			{
				string backup = BackupPath(workspace, record.Container);
				if (!File.Exists(backup))
				{
					result.Skipped.Add(record.Name);
					result.Notes.Add("no backup kept for " + record.Container);
					manifest[record.Name] = record;
					return;
				}
				File.Copy(backup, live, true);
			}
			result.Restored.Add(record.Name);
		}

		/// <summary>Puts the originals back, and removes files the mod added.</summary>
		public static ModResult Uninstall(Workspace workspace)
		{
			ModResult result = new ModResult();

			if (!workspace.Installable)
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
