// What the editor edits.
//
// The same rule the game plays by: a file is read from the override directory if it
// is there, and from the shipped content if it is not. Saving always writes to the
// override, so the shipped content is never touched and reverting is a matter of
// deleting one file. Nothing has to be extracted first.
//
// The shipped content is behind IContentSource, so this works the same over our own
// archives and over a Steam install's loose files directory. Overrides stay separate
// either way - the Steam build has no override mechanism of its own, and writing back
// into a directory Steam validates is not something to do behind someone's back.

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
		private readonly IContentSource _source;
		private readonly List<string> _names;
		private string _messagePrefix;

		public string OverrideDirectory { get; }

		/// <summary>Where the shipped content lives.</summary>
		public string ContentDirectory => _contentDirectory;

		/// <summary>"archives" or "loose files", for anything reporting what it opened.</summary>
		public string Kind => _source.Kind;

		public Workspace(string contentDirectory, string overrideDirectory)
		{
			_contentDirectory = Path.GetFullPath(contentDirectory);
			OverrideDirectory = Path.GetFullPath(overrideDirectory
				?? Path.Combine(contentDirectory, "Override"));

			// Packed first: our own Content directory has both a data000.bin and,
			// after an extract, possibly a files directory as well, and the archives
			// are the copy the game itself reads.
			string table = Path.Combine(_contentDirectory, "data000.bin");
			if (File.Exists(table))
			{
				_source = new ArchiveContentSource(
					_contentDirectory, ArchiveIndex.Load(File.ReadAllBytes(table)));
			}
			else if (LooseContentSource.Looks(_contentDirectory))
			{
				_source = new LooseContentSource(_contentDirectory);
			}
			else
			{
				throw new FileNotFoundException(
					"no data000.bin and no files directory in " + _contentDirectory
					+ " - point --content at our Content directory or at a game install");
			}

			_names = _source.Names.ToList();
			Directory.CreateDirectory(OverrideDirectory);
		}

		public int FileCount => _source.Count;

		/// <summary>
		/// Where the .msd files are, which is not the same in both releases and is not
		/// worth guessing at. Ours keeps a folder per language, en.lproj and its
		/// siblings. A Steam install is one language and puts them straight in files/.
		/// So: take the language folder if the content has one, and fall back to
		/// wherever the .msd files actually are if it does not.
		/// </summary>
		public string MessagePrefix(string language)
		{
			if (_messagePrefix != null)
			{
				return _messagePrefix;
			}

			string wanted = (language ?? "en") + ".lproj/";
			List<string> messages = _names
				.Where(n => n.EndsWith(".msd", StringComparison.OrdinalIgnoreCase))
				.ToList();

			if (messages.Any(n => n.StartsWith(wanted, StringComparison.OrdinalIgnoreCase)))
			{
				return _messagePrefix = wanted;
			}

			// Whatever directory holds the most of them. On a Steam install that is
			// files/; if some other build shipped a single lproj under another name,
			// this finds that too rather than showing every message as a bare id.
			string prefix = messages
				.Select(n => n.LastIndexOf('/') < 0 ? string.Empty : n.Substring(0, n.LastIndexOf('/') + 1))
				.GroupBy(s => s, StringComparer.OrdinalIgnoreCase)
				.OrderByDescending(g => g.Count())
				.Select(g => g.Key)
				.FirstOrDefault();

			return _messagePrefix = prefix ?? wanted;
		}

		/// <summary>Everything with one of the given extensions, archives and overrides.</summary>
		public List<WorkspaceEntry> List(params string[] extensions)
		{
			HashSet<string> wanted = new HashSet<string>(extensions, StringComparer.OrdinalIgnoreCase);
			List<WorkspaceEntry> entries = new List<WorkspaceEntry>();

			foreach (string name in _names)
			{
				string extension = Path.GetExtension(name);
				if (wanted.Count > 0 && !wanted.Contains(extension))
				{
					continue;
				}
				entries.Add(new WorkspaceEntry
				{
					Name = name,
					Extension = extension,
					Overridden = File.Exists(OverridePath(name))
				});
			}

			return entries.OrderBy(e => e.Name, StringComparer.OrdinalIgnoreCase).ToList();
		}

		public bool Exists(string name)
		{
			return File.Exists(OverridePath(name)) || _source.TryRead(name, out _);
		}

		public bool IsOverridden(string name)
		{
			return File.Exists(OverridePath(name));
		}

		/// <summary>The override if there is one, the shipped copy otherwise.</summary>
		public byte[] Read(string name)
		{
			string overridden = OverridePath(name);
			if (File.Exists(overridden))
			{
				return File.ReadAllBytes(overridden);
			}

			if (!_source.TryRead(name, out byte[] data))
			{
				throw new FileNotFoundException("no file called " + name);
			}
			return data;
		}

		public void Write(string name, byte[] data)
		{
			string path = OverridePath(name);
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			File.WriteAllBytes(path, data);
		}

		/// <summary>Removes the override, so the shipped copy is what the game sees.</summary>
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
