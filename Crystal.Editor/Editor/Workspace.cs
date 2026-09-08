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
using OpenFF.Formats;

namespace Crystal.Editor
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

		/// <summary>Where the shipped content lives - the directory that was pointed at.</summary>
		public string ContentDirectory => _contentDirectory;

		/// <summary>
		/// Where a content name is actually a path under, for anything that writes into
		/// the install. The same as ContentDirectory except for FF4, whose files/ is a
		/// level down in EXTRACTED_DATA.
		/// </summary>
		public string LooseRoot => _source is SsamContentSource || _source is LooseContentSource
			? LooseContentSource.Resolve(_contentDirectory) : _contentDirectory;

		/// <summary>"archives" or "loose files", for anything reporting what it opened.</summary>
		public string Kind => _source.Kind;

		/// <summary>The shipped content this workspace reads (for readers that want a ContentChain of their own).</summary>
		public IContentSource Source => _source;

		/// <summary>Climbs at every write, so cached readings of the content can tell they are stale.</summary>
		public int Version { get; private set; }

		/// <summary>
		/// "ff3" or "ff4". The same engine shipped both, and nearly everything reads the
		/// same, but the .pak record schemas and the text encoding are per game, so the
		/// few places that care ask this rather than guessing from a path.
		/// </summary>
		public string Game => _source is SsamContentSource ? "ff4" : "ff3";

		/// <summary>The script command table for this game's bytecode.</summary>
		public ScriptOpTable Ops => ScriptOpTable.For(Game);

		/// <summary>
		/// Whether a content name is served out of a mass file rather than a loose
		/// file, and which. Anything that installs an edit has to know, because the
		/// edit goes back inside the container rather than beside it.
		/// </summary>
		public bool TryLocateInContainer(string name, out string container, out string entryName, out bool compressed)
		{
			if (_source is SsamContentSource packed)
			{
				return packed.TryLocate(name, out container, out entryName, out compressed);
			}
			container = null;
			entryName = null;
			compressed = false;
			return false;
		}

		/// <summary>Whether edits can be installed into this content at all - a game install, not our archives.</summary>
		public bool Installable => _source is LooseContentSource || _source is SsamContentSource;

		public Workspace(string contentDirectory, string overrideDirectory)
		{
			_contentDirectory = Path.GetFullPath(contentDirectory);

			// Packed first: our own Content directory has both a data000.bin and,
			// after an extract, possibly a files directory as well, and the archives
			// are the copy the game itself reads.
			string table = Path.Combine(_contentDirectory, "data000.bin");
			if (File.Exists(table))
			{
				_source = new ArchiveContentSource(
					_contentDirectory, ArchiveIndex.Load(File.ReadAllBytes(table)));
			}
			else if (SsamContentSource.Looks(_contentDirectory))
			{
				// FF4 on Steam: loose files plus the mass files its map data is
				// bundled into. Checked before the plain loose case, which it also is.
				_source = new SsamContentSource(_contentDirectory);
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

			// Only now, because where the edits go depends on which of the two this
			// is. Our own build reads Content/Override itself, so that is where they
			// belong. A game install must not be written into by default: that
			// executable does not read an override directory anyway, the folder often
			// needs administrator rights, and Steam validating its own files would
			// sweep the work away with it.
			//
			// So a game install gets local application data. Documents would be easier
			// to find and was the first choice, but Windows protects it - Controlled
			// Folder Access is on by default on plenty of machines, and it was on for
			// the one this was written on, where creating a folder there failed with
			// "could not find file" pointing at the folder being created. Not a error
			// worth handing somebody who only wanted to open the editor.
			OverrideDirectory = Path.GetFullPath(overrideDirectory ?? (_source.Kind == "archives"
				? Path.Combine(_contentDirectory, "Override")
				: Path.Combine(
					CrystalHome.Mods,
					Path.GetFileName(_contentDirectory.TrimEnd(
						Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)))));

			_names = _source.Names.ToList();
			// The project's own files with no shipped twin - a duplicated model, a new picture -
			// are content too: listed with the rest, so the libraries show them.
			try
			{
				if (Directory.Exists(OverrideDirectory))
				{
					HashSet<string> have = new HashSet<string>(_names, StringComparer.OrdinalIgnoreCase);
					foreach (string file in Directory.EnumerateFiles(OverrideDirectory, "*", SearchOption.AllDirectories))
					{
						string name = Path.GetRelativePath(OverrideDirectory, file).Replace(Path.DirectorySeparatorChar, '/');
						if (!have.Contains(name)) { _names.Add(name); have.Add(name); }
					}
				}
			}
			catch (Exception) { }

			try
			{
				Directory.CreateDirectory(OverrideDirectory);
			}
			catch (Exception ex)
			{
				// Whatever the reason - permissions, a protected folder, a full disk -
				// the useful thing to say is which folder and what to do about it.
				throw new IOException(
					"cannot create the mod directory " + OverrideDirectory + ": "
					+ ex.Message + " - pass --override=<dir> to put it somewhere else", ex);
			}
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

		/// <summary>The shipped copy, whether or not an override exists; null when the game has no such file.</summary>
		public byte[] ReadShipped(string name)
		{
			return _source.TryRead(name, out byte[] data) ? data : null;
		}

		public void Write(string name, byte[] data)
		{
			Version++;
			string path = OverridePath(name);
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			File.WriteAllBytes(path, data);
			if (!_names.Contains(name, StringComparer.OrdinalIgnoreCase)) _names.Add(name);
		}

		/// <summary>Whether the game shipped a file of this name (an override with no twin is the project's own).</summary>
		public bool IsShipped(string name) => _source.TryRead(name, out _);

		/// <summary>Removes the override, so the shipped copy is what the game sees.</summary>
		public bool Revert(string name)
		{
			string path = OverridePath(name);
			if (!File.Exists(path))
			{
				return false;
			}
			File.Delete(path);
			// A file of the project's own, gone: off the list too.
			if (!_source.TryRead(name, out _)) _names.RemoveAll(n => string.Equals(n, name, StringComparison.OrdinalIgnoreCase));
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
