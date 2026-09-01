// Runtime access to the game's data*.bin archives, with a loose-file override.
//
// The format itself lives in Shared/ArchiveFormat.cs, compiled into both this and
// the content tool so the reader and the extractor cannot drift.
//
// Override: before touching the archives, a request for "foo.NCGR" looks for a
// loose file of that name under Content/Override (or --content-override=<dir>).
// That is what makes new and edited content possible without repacking - extract
// with `ff3content extract-archives`, drop an edited file in, and the game picks it
// up. Nothing has to be rebuilt.

using System;
using System.Collections.Generic;
using System.IO;
using FF3.Formats;

namespace FF3
{
	internal static class GameArchive
	{
		private static ArchiveIndex _index;
		private static string _overrideDirectory;
		private static readonly HashSet<string> _reportedOverrides =
			new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		public static bool IsLoaded => _index != null;

		/// <summary>Directory holding data000.bin, relative to the working directory.</summary>
		public static string DataPath => "Content";

		/// <summary>Where loose replacement files are looked for, or null if disabled.</summary>
		public static string OverrideDirectory => _overrideDirectory;

		/// <summary>Loads the file table. Returns false if it or any archive is missing.</summary>
		public static bool Load()
		{
			try
			{
				string table = GameFiles.Resolve(Path.Combine(DataPath, "data000.bin"));
				if (table == null)
				{
					Log.Write(LogChannel.File, "archive table not found under " + DataPath);
					return false;
				}
				_index = ArchiveIndex.Load(File.ReadAllBytes(table));
			}
			catch (Exception ex)
			{
				Log.Write(LogChannel.File, "archive table could not be read: " + ex.Message);
				_index = null;
				return false;
			}

			for (int i = 0; i < _index.ArchiveCount; i++)
			{
				if (GameFiles.Resolve(ArchiveIndex.ArchiveName(DataPath, i)) == null)
				{
					Log.Write(LogChannel.File, "missing archive "
						+ ArchiveIndex.ArchiveName(DataPath, i));
					_index = null;
					return false;
				}
			}

			SetUpOverrides();

			Log.Write(LogChannel.File, string.Format(
				"archives loaded: {0} files across {1} volumes",
				_index.FileCount, _index.ArchiveCount));
			return true;
		}

		private static void SetUpOverrides()
		{
			string configured = Options.Get("content-override");
			string directory = string.IsNullOrEmpty(configured)
				? Path.Combine(DataPath, "Override")
				: configured;

			try
			{
				string full = Path.GetFullPath(directory);
				if (Directory.Exists(full))
				{
					_overrideDirectory = full;
					int count = Directory.GetFiles(full, "*", SearchOption.AllDirectories).Length;
					Log.Write(LogChannel.File, string.Format(
						"content overrides: {0} loose file(s) in {1}", count, full));
				}
				else
				{
					_overrideDirectory = null;
				}
			}
			catch (Exception ex)
			{
				_overrideDirectory = null;
				Log.Write(LogChannel.File, "override directory unusable: " + ex.Message);
			}
		}

		/// <summary>Reads one file by name, or null if there is no such file.</summary>
		public static byte[] Read(string filename)
		{
			if (string.IsNullOrEmpty(filename))
			{
				return null;
			}

			byte[] loose = ReadOverride(filename);
			if (loose != null)
			{
				return loose;
			}

			if (_index == null || !_index.TryFind(filename, out ArchiveEntry entry))
			{
				return null;
			}

			try
			{
				string path = GameFiles.Resolve(ArchiveIndex.ArchiveName(DataPath, entry.Archive));
				if (path == null)
				{
					return null;
				}
				using FileStream stream = File.OpenRead(path);
				byte[] data = ArchiveIndex.ReadBlob(stream, entry.Index);
				if (data == null)
				{
					Log.Write(LogChannel.File, "bad archive entry for " + filename);
				}
				return data;
			}
			catch (Exception ex)
			{
				Log.Write(LogChannel.File, "archive read failed for " + filename + ": " + ex.Message);
				return null;
			}
		}

		/// <summary>A loose file standing in for an archived one, or null.</summary>
		private static byte[] ReadOverride(string filename)
		{
			if (_overrideDirectory == null)
			{
				return null;
			}
			try
			{
				string path = Path.Combine(_overrideDirectory, filename);

				// Keep the lookup inside the override directory: archive names come from
				// game data, and one containing "..\" should not reach outside it.
				string full = Path.GetFullPath(path);
				if (!full.StartsWith(_overrideDirectory + Path.DirectorySeparatorChar,
						StringComparison.OrdinalIgnoreCase)
					|| !File.Exists(full))
				{
					return null;
				}

				lock (_reportedOverrides)
				{
					if (_reportedOverrides.Add(filename))
					{
						Log.Write(LogChannel.File, "override in use: " + filename);
					}
				}
				return File.ReadAllBytes(full);
			}
			catch (Exception ex)
			{
				Log.Write(LogChannel.File, "override read failed for " + filename + ": " + ex.Message);
				return null;
			}
		}
	}
}
