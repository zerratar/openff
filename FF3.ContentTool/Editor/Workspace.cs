// What the editor edits.
//
// The same rule the game plays by: a file is read from the override directory if it
// is there, and from the archives if it is not. Saving always writes to the override,
// so the shipped archives are never touched and reverting is a matter of deleting one
// file. Nothing has to be extracted first.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FF3.Formats;

namespace FF3.ContentTool.Editor
{
	internal sealed class WorkspaceEntry
	{
		public string Name { get; set; }
		public string Extension { get; set; }
		public bool Overridden { get; set; }
		public int Size { get; set; }
	}

	internal sealed class Workspace
	{
		private readonly string _contentDirectory;
		private readonly ArchiveIndex _index;
		private readonly Dictionary<int, string> _archivePaths = new Dictionary<int, string>();

		public string OverrideDirectory { get; }

		/// <summary>Where the archives and the XNBs live.</summary>
		public string ContentDirectory => _contentDirectory;

		public Workspace(string contentDirectory, string overrideDirectory)
		{
			_contentDirectory = Path.GetFullPath(contentDirectory);
			OverrideDirectory = Path.GetFullPath(overrideDirectory
				?? Path.Combine(contentDirectory, "Override"));

			string table = Path.Combine(_contentDirectory, "data000.bin");
			if (!File.Exists(table))
			{
				throw new FileNotFoundException(
					"no data000.bin in " + _contentDirectory
					+ " - point --content at the game's Content directory");
			}
			_index = ArchiveIndex.Load(File.ReadAllBytes(table));
			Directory.CreateDirectory(OverrideDirectory);
		}

		public int FileCount => _index.FileCount;

		/// <summary>Everything with one of the given extensions, archives and overrides.</summary>
		public List<WorkspaceEntry> List(params string[] extensions)
		{
			HashSet<string> wanted = new HashSet<string>(extensions, StringComparer.OrdinalIgnoreCase);
			List<WorkspaceEntry> entries = new List<WorkspaceEntry>();

			foreach (ArchiveEntry entry in _index.Entries)
			{
				string extension = Path.GetExtension(entry.Name);
				if (wanted.Count > 0 && !wanted.Contains(extension))
				{
					continue;
				}
				entries.Add(new WorkspaceEntry
				{
					Name = entry.Name,
					Extension = extension,
					Overridden = File.Exists(OverridePath(entry.Name))
				});
			}

			return entries.OrderBy(e => e.Name, StringComparer.OrdinalIgnoreCase).ToList();
		}

		public bool Exists(string name)
		{
			return File.Exists(OverridePath(name)) || _index.TryFind(name, out _);
		}

		public bool IsOverridden(string name)
		{
			return File.Exists(OverridePath(name));
		}

		/// <summary>The override if there is one, the archived copy otherwise.</summary>
		public byte[] Read(string name)
		{
			string loose = OverridePath(name);
			if (File.Exists(loose))
			{
				return File.ReadAllBytes(loose);
			}

			if (!_index.TryFind(name, out ArchiveEntry entry))
			{
				throw new FileNotFoundException("no file called " + name);
			}

			if (!_archivePaths.TryGetValue(entry.Archive, out string path))
			{
				path = ArchiveIndex.ArchiveName(_contentDirectory, entry.Archive);
				if (!File.Exists(path))
				{
					throw new FileNotFoundException("missing archive " + path);
				}
				_archivePaths[entry.Archive] = path;
			}

			using FileStream stream = File.OpenRead(path);
			byte[] data = ArchiveIndex.ReadBlob(stream, entry.Index);
			if (data == null)
			{
				throw new InvalidDataException("could not read " + name + " out of the archive");
			}
			return data;
		}

		/// <summary>The archived copy, ignoring any override - what "revert" goes back to.</summary>
		public byte[] ReadOriginal(string name)
		{
			string loose = OverridePath(name);
			bool had = File.Exists(loose);
			string moved = loose + ".reading";
			try
			{
				if (had)
				{
					File.Move(loose, moved);
				}
				return Read(name);
			}
			finally
			{
				if (had && File.Exists(moved))
				{
					File.Move(moved, loose, overwrite: true);
				}
			}
		}

		public void Write(string name, byte[] data)
		{
			string path = OverridePath(name);
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			File.WriteAllBytes(path, data);
		}

		/// <summary>Removes the override, so the archived copy is what the game sees.</summary>
		public bool Revert(string name)
		{
			string path = OverridePath(name);
			if (!File.Exists(path))
			{
				return false;
			}
			File.Delete(path);
			return true;
		}

		/// <summary>
		/// Where an override for this name lives, checked to stay inside the override
		/// directory - names come from game data and from a browser, not from us.
		/// </summary>
		private string OverridePath(string name)
		{
			if (string.IsNullOrEmpty(name) || Path.IsPathRooted(name))
			{
				throw new ArgumentException("not a content name: " + name, nameof(name));
			}
			string full = Path.GetFullPath(Path.Combine(OverrideDirectory, name));
			if (!full.StartsWith(OverrideDirectory + Path.DirectorySeparatorChar,
					StringComparison.OrdinalIgnoreCase))
			{
				throw new ArgumentException("that name points outside the override directory",
					nameof(name));
			}
			return full;
		}
	}
}
