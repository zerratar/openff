// Where a game file comes from, in order: the mods, then the shipped content.
//
// The editor and the client used to answer that question separately - the editor with
// a Workspace over IContentSource, the client with GameArchive over data*.bin and one
// override directory. This is the one answer both use now. A chain is a list of
// override directories (a project's edits, a mod someone downloaded) in front of one
// shipped source, which is whichever shape the content came in: our archives, Steam
// FF3's loose files, or FF4's loose files plus mass files. Nothing above it knows which.
//
// Names are the game's own: "files/d01_01.script", "sound/BGM01.dat". Override
// directories hold files under the same names, so a mod is a folder that mirrors the
// game's, and dropping a file in is the whole installation.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FF3.Formats;

namespace FF3.Content
{
	internal sealed class ContentChain
	{
		private readonly List<string> _overrides = new List<string>();
		private readonly List<IContentSource> _fallbacks = new List<IContentSource>();
		private readonly HashSet<string> _reported = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		/// <summary>The shipped content, whatever shape it is in.</summary>
		public IContentSource Shipped { get; }

		/// <summary>
		/// Content asked for behind the shipped source when it does not have a name. The
		/// Steam build of FF3 leaves out what its build has no screen for - about.NCGR,
		/// for one - so booting from it wants our archives behind it. Mixed content is
		/// the same mechanism: FF4's files in front, FF3's behind, or the other way.
		/// </summary>
		public IReadOnlyList<IContentSource> Fallbacks => _fallbacks;

		/// <summary>Where the shipped content was opened from.</summary>
		public string Root { get; }

		/// <summary>Override directories, first one wins.</summary>
		public IReadOnlyList<string> Overrides => _overrides;

		/// <summary>"ff3" or "ff4": FF4 is the one with mass files.</summary>
		public string Game => Shipped is SsamContentSource ? "ff4" : "ff3";

		/// <summary>Called with a name the first time an override serves it, if set.</summary>
		public Action<string, string> OnOverrideUsed { get; set; }

		private ContentChain(string root, IContentSource shipped)
		{
			Root = root;
			Shipped = shipped;
		}

		/// <summary>
		/// Opens the content under a directory: our Content (data000.bin), a Steam FF3
		/// install (files/ beside the executable) or a Steam FF4 install (EXTRACTED_DATA
		/// with its .dat mass files). Throws with a readable message otherwise.
		/// </summary>
		public static ContentChain Open(string root, IEnumerable<string> overrides = null)
		{
			root = Path.GetFullPath(root);
			IContentSource shipped;
			string table = Path.Combine(root, "data000.bin");
			if (File.Exists(table))
			{
				shipped = new ArchiveContentSource(root, ArchiveIndex.Load(File.ReadAllBytes(table)));
			}
			else if (SsamContentSource.Looks(root))
			{
				shipped = new SsamContentSource(root);
			}
			else if (LooseContentSource.Looks(root))
			{
				shipped = new LooseContentSource(root);
			}
			else
			{
				throw new FileNotFoundException(
					"no data000.bin and no files directory in " + root
					+ " - point at our Content directory or at a game install");
			}
			ContentChain chain = new ContentChain(root, shipped);
			foreach (string directory in overrides ?? Enumerable.Empty<string>())
			{
				chain.AddOverride(directory);
			}
			return chain;
		}

		/// <summary>Adds content to ask when the shipped source has no such name.</summary>
		public void AddFallback(string root)
		{
			IContentSource source = OpenSource(Path.GetFullPath(root));
			if (source != null)
			{
				_fallbacks.Add(source);
			}
		}

		/// <summary>The source for a directory, or null when it is not content.</summary>
		private static IContentSource OpenSource(string root)
		{
			string table = Path.Combine(root, "data000.bin");
			if (File.Exists(table))
			{
				return new ArchiveContentSource(root, ArchiveIndex.Load(File.ReadAllBytes(table)));
			}
			if (SsamContentSource.Looks(root))
			{
				return new SsamContentSource(root);
			}
			if (LooseContentSource.Looks(root))
			{
				return new LooseContentSource(root);
			}
			return null;
		}

		/// <summary>Whether a directory is something Open would accept.</summary>
		public static bool Looks(string root)
		{
			try
			{
				return File.Exists(Path.Combine(root, "data000.bin"))
					|| SsamContentSource.Looks(root) || LooseContentSource.Looks(root);
			}
			catch (Exception)
			{
				return false;
			}
		}

		/// <summary>Adds an override directory in front of the ones already there. Missing directories are ignored.</summary>
		public void AddOverride(string directory)
		{
			if (string.IsNullOrEmpty(directory))
			{
				return;
			}
			string full = Path.GetFullPath(directory);
			if (Directory.Exists(full) && !_overrides.Contains(full, StringComparer.OrdinalIgnoreCase))
			{
				_overrides.Add(full);
			}
		}

		/// <summary>Whether an override exists for a name, and where.</summary>
		public string OverridePath(string name)
		{
			foreach (string directory in _overrides)
			{
				string path = Path.Combine(directory, name);
				string full;
				try
				{
					full = Path.GetFullPath(path);
				}
				catch (Exception)
				{
					continue;
				}
				// Names come from game data; one containing "..\" must not reach outside.
				if (full.StartsWith(directory + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
					&& File.Exists(full))
				{
					return full;
				}
			}
			return null;
		}

		public bool Exists(string name)
		{
			return !string.IsNullOrEmpty(name)
				&& (OverridePath(name) != null || Shipped.TryRead(name, out _) || _fallbacks.Any(f => f.TryRead(name, out _)));
		}

		/// <summary>The bytes of a file, from the first override that has it or the shipped content; null if nowhere.</summary>
		public byte[] Read(string name)
		{
			return TryRead(name, out byte[] data) ? data : null;
		}

		public bool TryRead(string name, out byte[] data)
		{
			data = null;
			if (string.IsNullOrEmpty(name))
			{
				return false;
			}
			string loose = OverridePath(name);
			if (loose != null)
			{
				data = File.ReadAllBytes(loose);
				lock (_reported)
				{
					if (_reported.Add(name))
					{
						OnOverrideUsed?.Invoke(name, loose);
					}
				}
				return true;
			}
			if (Shipped.TryRead(name, out data))
			{
				return true;
			}
			foreach (IContentSource fallback in _fallbacks)
			{
				if (fallback.TryRead(name, out data))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>Every name: the shipped ones plus anything the overrides add.</summary>
		public IEnumerable<string> Names
		{
			get
			{
				HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				foreach (string name in Shipped.Names)
				{
					if (seen.Add(name)) yield return name;
				}
				foreach (IContentSource fallback in _fallbacks)
				{
					foreach (string name in fallback.Names)
					{
						if (seen.Add(name)) yield return name;
					}
				}
				foreach (string directory in _overrides)
				{
					foreach (string path in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
					{
						string name = path.Substring(directory.Length)
							.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
							.Replace(Path.DirectorySeparatorChar, '/');
						if (seen.Add(name)) yield return name;
					}
				}
			}
		}

		/// <summary>One line for a log: what was opened, and the overrides in front of it.</summary>
		public string Describe()
		{
			string text = string.Format(System.Globalization.CultureInfo.InvariantCulture,
				"{0}: {1} files in {2}", Shipped.Kind, Shipped.Count, Root);
			foreach (IContentSource fallback in _fallbacks)
			{
				text += string.Format(System.Globalization.CultureInfo.InvariantCulture,
					"; then {0} ({1} files)", fallback.Kind, fallback.Count);
			}
			if (_overrides.Count > 0)
			{
				text += "; overrides: " + string.Join(", ", _overrides);
			}
			return text;
		}
	}
}
