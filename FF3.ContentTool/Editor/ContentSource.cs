// Where the shipped bytes come from.
//
// Two releases of this game ship the same data two different ways. Ours packs it into
// data000.bin plus numbered archives, the way the phone build did. The Steam build
// leaves it loose in a files directory next to the executable - already unpacked,
// because a desktop install has no reason to pack it.
//
// The formats inside are the same either way, so nothing above this file needs to know
// which one it is reading. That is the whole point of the split: the editor, the
// reference index and every parser were written against a workspace, not against an
// archive, and they carry over untouched.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FF3.Formats;

namespace FF3.ContentTool.Editor
{
	/// <summary>One release's shipped content, whatever shape it is stored in.</summary>
	internal interface IContentSource
	{
		/// <summary>What this is, for the person looking at it: "archives" or "loose files".</summary>
		string Kind { get; }

		int Count { get; }

		/// <summary>Every content name, in the "files/x.pak" form the game asks for.</summary>
		IEnumerable<string> Names { get; }

		bool TryRead(string name, out byte[] data);
	}

	/// <summary>data000.bin and its numbered archives - our own build, and the phone's.</summary>
	internal sealed class ArchiveContentSource : IContentSource
	{
		private readonly string _directory;
		private readonly ArchiveIndex _index;
		private readonly Dictionary<int, string> _archivePaths = new Dictionary<int, string>();

		public ArchiveContentSource(string directory, ArchiveIndex index)
		{
			_directory = directory;
			_index = index;
		}

		public string Kind => "archives";

		public int Count => _index.FileCount;

		public IEnumerable<string> Names => _index.Entries.Select(e => e.Name);

		public bool TryRead(string name, out byte[] data)
		{
			data = null;
			if (!_index.TryFind(name, out ArchiveEntry entry))
			{
				return false;
			}

			if (!_archivePaths.TryGetValue(entry.Archive, out string path))
			{
				path = ArchiveIndex.ArchiveName(_directory, entry.Archive);
				if (!File.Exists(path))
				{
					throw new FileNotFoundException("missing archive " + path);
				}
				_archivePaths[entry.Archive] = path;
			}

			using FileStream stream = File.OpenRead(path);
			data = ArchiveIndex.ReadBlob(stream, entry.Index);
			if (data == null)
			{
				throw new InvalidDataException("could not read " + name + " out of the archive");
			}
			return true;
		}
	}

	/// <summary>
	/// A directory of loose files - the Steam build, where files/ sits next to the
	/// executable and the names are already the ones the game asks for.
	/// </summary>
	internal sealed class LooseContentSource : IContentSource
	{
		private readonly string _root;
		private readonly Dictionary<string, string> _paths;

		/// <summary>Directories that are content. Everything else in an install is not.</summary>
		private static readonly string[] Roots = { "files", "sound" };

		/// <summary>
		/// The directory the content directories actually hang off. FF3's install has
		/// files/ beside the executable; FF4's puts the whole tree one level down, in
		/// EXTRACTED_DATA - its executable says so itself, "./EXTRACTED_DATA/files/".
		/// Either way the person points at the install, and this finds the data.
		/// </summary>
		public static string Resolve(string root)
		{
			if (Roots.Any(r => Directory.Exists(Path.Combine(root, r))))
			{
				return root;
			}
			string nested = Path.Combine(root, "EXTRACTED_DATA");
			if (Roots.Any(r => Directory.Exists(Path.Combine(nested, r))))
			{
				return nested;
			}
			return root;
		}

		/// <summary>Where the content directories are - the install, or its EXTRACTED_DATA.</summary>
		public string Root => _root;

		public LooseContentSource(string root)
		{
			root = Resolve(root);
			_root = root;
			// Case-insensitively, because the names come from game data and from a
			// browser, and this half of the world does not agree with the other half
			// about whether MenuDefine.xbn and menudefine.xbn are the same file.
			_paths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			foreach (string top in Roots)
			{
				string directory = Path.Combine(root, top);
				if (!Directory.Exists(directory))
				{
					continue;
				}
				foreach (string path in Directory.EnumerateFiles(
					directory, "*", SearchOption.AllDirectories))
				{
					string name = path.Substring(root.Length)
						.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
						.Replace(Path.DirectorySeparatorChar, '/');
					_paths[name] = path;
				}
			}

			// A language folder is content too, and it is named rather than fixed.
			foreach (string directory in Directory.EnumerateDirectories(root, "*.lproj"))
			{
				foreach (string path in Directory.EnumerateFiles(
					directory, "*", SearchOption.AllDirectories))
				{
					string name = path.Substring(root.Length)
						.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
						.Replace(Path.DirectorySeparatorChar, '/');
					_paths[name] = path;
				}
			}
		}

		public string Kind => "loose files";

		public int Count => _paths.Count;

		public IEnumerable<string> Names => _paths.Keys;

		public bool TryRead(string name, out byte[] data)
		{
			data = null;
			if (!_paths.TryGetValue(name, out string path))
			{
				return false;
			}
			data = File.ReadAllBytes(path);
			return true;
		}

		/// <summary>Whether this looks like a loose install at all, before we commit to it.</summary>
		public static bool Looks(string root)
		{
			root = Resolve(root);
			return Roots.Any(r => Directory.Exists(Path.Combine(root, r)));
		}
	}
}
