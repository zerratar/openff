// The data*.bin side of the content tool.
//
//   crystal archives         <content-dir>
//   crystal extract-archives <content-dir> <out-dir> [pattern ...]
//
// Extraction is deliberately flat: the game looks an override up as
// <override-dir>/<name>, so an extracted directory can be pointed at directly with
//   OpenFF.exe --content-override=<out-dir>
// and every file in it takes precedence over the archived copy. Edit one, keep it,
// throw the rest away - the game only reads what is there.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using OpenFF.Formats;

namespace Crystal
{
	internal static class Archives
	{
		/// <summary>Loads data000.bin from a Content directory.</summary>
		private static ArchiveIndex Load(string contentDir)
		{
			string table = Path.Combine(contentDir, "data000.bin");
			if (!File.Exists(table))
			{
				throw new FileNotFoundException("no data000.bin in " + Path.GetFullPath(contentDir));
			}
			return ArchiveIndex.Load(File.ReadAllBytes(table));
		}

		/// <summary>What is in the archives, grouped by extension.</summary>
		public static int Info(string contentDir)
		{
			ArchiveIndex index = Load(contentDir);
			Console.WriteLine("{0} files across {1} volumes", index.FileCount, index.ArchiveCount);

			int missing = 0;
			for (int i = 0; i < index.ArchiveCount; i++)
			{
				string name = ArchiveIndex.ArchiveName(contentDir, i);
				if (!File.Exists(name))
				{
					Console.WriteLine("  MISSING {0}", name);
					missing++;
				}
			}
			if (missing > 0)
			{
				Console.WriteLine("{0} archive(s) missing - extraction will be incomplete", missing);
			}

			Console.WriteLine();
			Dictionary<string, int> byExtension = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
			foreach (ArchiveEntry entry in index.Entries)
			{
				string extension = Path.GetExtension(entry.Name);
				if (string.IsNullOrEmpty(extension))
				{
					extension = "(none)";
				}
				byExtension.TryGetValue(extension, out int n);
				byExtension[extension] = n + 1;
			}
			foreach (KeyValuePair<string, int> row in byExtension.OrderByDescending(r => r.Value))
			{
				Console.WriteLine("{0,6}  {1}", row.Value, row.Key);
			}
			return 0;
		}

		/// <summary>
		/// Writes every archived file (or those matching the given patterns) into
		/// outputDir, alongside a manifest recording where each came from.
		/// </summary>
		public static int Extract(string contentDir, string outputDir, string[] patterns)
		{
			ArchiveIndex index = Load(contentDir);
			Directory.CreateDirectory(outputDir);
			string root = Path.GetFullPath(outputDir);

			// One open handle per volume, rather than reopening for all 6962 files.
			FileStream[] volumes = new FileStream[index.ArchiveCount];
			StringBuilder manifest = new StringBuilder();
			manifest.AppendLine("# name\tarchive\tindex\tbytes");

			int written = 0;
			int failed = 0;
			long bytes = 0;
			try
			{
				foreach (ArchiveEntry entry in index.Entries)
				{
					if (!Matches(entry.Name, patterns))
					{
						continue;
					}
					if (entry.Archive < 0 || entry.Archive >= volumes.Length)
					{
						Console.Error.WriteLine("bad archive index for " + entry.Name);
						failed++;
						continue;
					}

					if (volumes[entry.Archive] == null)
					{
						string path = ArchiveIndex.ArchiveName(contentDir, entry.Archive);
						if (!File.Exists(path))
						{
							failed++;
							continue;
						}
						volumes[entry.Archive] = File.OpenRead(path);
					}

					byte[] data = ArchiveIndex.ReadBlob(volumes[entry.Archive], entry.Index);
					if (data == null)
					{
						Console.Error.WriteLine("unreadable: " + entry.Name);
						failed++;
						continue;
					}

					string destination = Destination(root, entry.Name);
					if (destination == null)
					{
						Console.Error.WriteLine("refused unsafe name: " + entry.Name);
						failed++;
						continue;
					}

					Directory.CreateDirectory(Path.GetDirectoryName(destination));
					File.WriteAllBytes(destination, data);
					manifest.AppendFormat(CultureInfo.InvariantCulture, "{0}\t{1}\t{2}\t{3}\n",
						entry.Name, entry.Archive, entry.Index, data.Length);
					written++;
					bytes += data.Length;
				}
			}
			finally
			{
				foreach (FileStream volume in volumes)
				{
					volume?.Dispose();
				}
			}

			File.WriteAllText(Path.Combine(outputDir, "manifest.tsv"), manifest.ToString());

			Console.WriteLine("extracted: {0} file(s), {1:N0} bytes", written, bytes);
			if (failed > 0)
			{
				Console.WriteLine("failed:    {0}", failed);
			}
			Console.WriteLine("written to " + Path.GetFullPath(outputDir));
			Console.WriteLine();
			Console.WriteLine("to have the game read these instead of the archives:");
			Console.WriteLine("  OpenFF.exe --content-override=" + Path.GetFullPath(outputDir));
			return failed > 0 ? 1 : 0;
		}

		/// <summary>
		/// Where an archived name lands under the output root. Names are path
		/// qualified in the table ("en.lproj/ca_text_01.NCGR"), and that structure has
		/// to be kept: the localised copies share a base name, and the game asks for
		/// the qualified name, so an override only matches if the layout matches.
		/// Returns null for a name that would escape the root.
		/// </summary>
		private static string Destination(string root, string name)
		{
			if (string.IsNullOrEmpty(name) || Path.IsPathRooted(name))
			{
				return null;
			}
			string full = Path.GetFullPath(Path.Combine(root, name));
			return full.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
				? full : null;
		}

		/// <summary>No patterns means everything; otherwise a simple *?-glob on the name.</summary>
		private static bool Matches(string name, string[] patterns)
		{
			if (patterns == null || patterns.Length == 0)
			{
				return true;
			}
			foreach (string pattern in patterns)
			{
				if (Glob(name, pattern))
				{
					return true;
				}
			}
			return false;
		}

		private static bool Glob(string text, string pattern)
		{
			int t = 0, p = 0, star = -1, mark = 0;
			while (t < text.Length)
			{
				if (p < pattern.Length
					&& (pattern[p] == '?'
						|| char.ToUpperInvariant(pattern[p]) == char.ToUpperInvariant(text[t])))
				{
					t++;
					p++;
				}
				else if (p < pattern.Length && pattern[p] == '*')
				{
					star = p++;
					mark = t;
				}
				else if (star >= 0)
				{
					p = star + 1;
					t = ++mark;
				}
				else
				{
					return false;
				}
			}
			while (p < pattern.Length && pattern[p] == '*')
			{
				p++;
			}
			return p == pattern.Length;
		}
	}
}
