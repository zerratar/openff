// The game's own files as a release has them: a list of every file of an install OpenFF reads (its path, size and
// SHA-1), made from a clean install by `crystal game-files`, shipped in the client's Data/game-files/, and checked
// against the player's install - so a game folder whose files were replaced (a Steam mod installed into it, files
// from the phone version, a damaged download) is said as such, before it shows as a menu laid out wrong or a crash.
//
// What is listed: everything under the install but the programs (.exe, .dll) and Qt's platforms/ and plugins/,
// which OpenFF never reads. A check reads each file's size first and hashes only those whose size matches; the
// hashes are kept beside the client by size and time written, so a second check reads nothing it read before.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

namespace OpenFF.Content
{
	internal static class GameFiles
	{
		/// <summary>A release's list: which game and copy it is, and each file's size and SHA-1 by its path (forward slashes, as the install has it).</summary>
		public sealed class Manifest
		{
			[JsonPropertyName("game")] public string Game { get; set; }
			[JsonPropertyName("source")] public string Source { get; set; }
			[JsonPropertyName("made")] public string Made { get; set; }
			[JsonPropertyName("files")] public Dictionary<string, Entry> Files { get; set; } = new Dictionary<string, Entry>(StringComparer.OrdinalIgnoreCase);
		}

		public sealed class Entry
		{
			[JsonPropertyName("size")] public long Size { get; set; }
			[JsonPropertyName("sha1")] public string Sha1 { get; set; }
		}

		/// <summary>What a check found: the files that differ from the release, those missing, those it does not list, and how many it read.</summary>
		public sealed class Result
		{
			public string Game { get; set; }
			public string Source { get; set; }
			public int Listed { get; set; }
			public List<string> Changed { get; } = new List<string>();
			public List<string> Missing { get; } = new List<string>();
			public List<string> Extra { get; } = new List<string>();
			public bool Clean => Changed.Count == 0 && Missing.Count == 0;
			/// <summary>So many files differ that this is likely another version of the game, not a modded one.</summary>
			public bool OtherVersion => Listed > 0 && (Changed.Count + Missing.Count) * 2 > Listed;
			/// <summary>A word for the files that differ, the same for the same set: what "don't warn again" remembers.</summary>
			public string Signature
			{
				get
				{
					using SHA1 sha = SHA1.Create();
					string all = string.Join("\n", Changed.Concat(Missing.Select(m => "-" + m)).OrderBy(s => s, StringComparer.OrdinalIgnoreCase));
					return Convert.ToHexString(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(all))).ToLowerInvariant();
				}
			}
		}

		private static readonly JsonSerializerOptions Json = new JsonSerializerOptions { WriteIndented = false };

		/// <summary>Whether OpenFF reads this file of an install (the rule both making a list and checking follow).</summary>
		public static bool Listed(string relative)
		{
			string r = relative.Replace('\\', '/');
			string ext = Path.GetExtension(r).ToLowerInvariant();
			if (ext == ".exe" || ext == ".dll") return false;
			string first = r.Contains('/') ? r.Substring(0, r.IndexOf('/')) : "";
			if (first.Equals("platforms", StringComparison.OrdinalIgnoreCase) || first.Equals("plugins", StringComparison.OrdinalIgnoreCase)) return false;
			// What a Steam mod's install leaves beside the game's files (Crystal's record of it, the backups it keeps).
			if (r.EndsWith("/installed.json", StringComparison.OrdinalIgnoreCase) || r.Contains(".backup/", StringComparison.OrdinalIgnoreCase)) return false;
			return true;
		}

		/// <summary>Every file of an install the rule lists, by relative path with forward slashes.</summary>
		public static IEnumerable<string> FilesOf(string root)
		{
			foreach (string file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
			{
				string relative = Path.GetRelativePath(root, file).Replace('\\', '/');
				if (Listed(relative)) yield return relative;
			}
		}

		public static string Sha1Of(string path, Action<long> progress = null)
		{
			using SHA1 sha = SHA1.Create();
			using FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 1 << 16);
			byte[] buffer = new byte[1 << 16];
			int read;
			while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
			{
				sha.TransformBlock(buffer, 0, read, null, 0);
				progress?.Invoke(read);
			}
			sha.TransformFinalBlock(buffer, 0, 0);
			return Convert.ToHexString(sha.Hash).ToLowerInvariant();
		}

		/// <summary>A clean install's list (crystal game-files), written gzipped.</summary>
		public static Manifest Make(string root, string game, string source)
		{
			Manifest m = new Manifest { Game = game, Source = source, Made = DateTime.UtcNow.ToString("yyyy-MM-dd") };
			foreach (string relative in FilesOf(root).OrderBy(r => r, StringComparer.OrdinalIgnoreCase))
			{
				string full = Path.Combine(root, relative);
				m.Files[relative] = new Entry { Size = new FileInfo(full).Length, Sha1 = Sha1Of(full) };
			}
			return m;
		}

		public static void Write(Manifest m, string path)
		{
			Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
			using FileStream file = File.Create(path);
			using GZipStream zip = new GZipStream(file, CompressionLevel.Optimal);
			JsonSerializer.Serialize(zip, m, Json);
		}

		public static Manifest Read(string path)
		{
			using FileStream file = File.OpenRead(path);
			using GZipStream zip = new GZipStream(file, CompressionMode.Decompress);
			Manifest m = JsonSerializer.Deserialize<Manifest>(zip, Json);
			if (m != null && m.Files != null) m.Files = new Dictionary<string, Entry>(m.Files, StringComparer.OrdinalIgnoreCase);
			return m;
		}

		/// <summary>The hashes a check has read, by path, with the size and time written they were read at.</summary>
		private sealed class CacheEntry
		{
			[JsonPropertyName("size")] public long Size { get; set; }
			[JsonPropertyName("time")] public long Time { get; set; }
			[JsonPropertyName("sha1")] public string Sha1 { get; set; }
		}

		/// <summary>
		/// An install checked against a list: sizes first, hashes for the files whose size matches (from the cache where
		/// the file has not been written since). progress hears the bytes read and the bytes there are to read.
		/// </summary>
		public static Result Check(string root, Manifest m, string cachePath, Action<long, long> progress = null, CancellationToken cancel = default)
		{
			Result result = new Result { Game = m.Game, Source = m.Source, Listed = m.Files.Count };
			Dictionary<string, CacheEntry> cache = LoadCache(cachePath);
			var toHash = new List<(string Relative, FileInfo Info, Entry Want)>();
			foreach (KeyValuePair<string, Entry> e in m.Files)
			{
				FileInfo info = new FileInfo(Path.Combine(root, e.Key));
				if (!info.Exists) { result.Missing.Add(e.Key); continue; }
				if (info.Length != e.Value.Size) { result.Changed.Add(e.Key); continue; }
				if (cache.TryGetValue(e.Key, out CacheEntry c) && c.Size == info.Length && c.Time == info.LastWriteTimeUtc.Ticks)
				{
					if (!string.Equals(c.Sha1, e.Value.Sha1, StringComparison.OrdinalIgnoreCase)) result.Changed.Add(e.Key);
					continue;
				}
				toHash.Add((e.Key, info, e.Value));
			}
			long total = toHash.Sum(t => t.Info.Length), done = 0;
			progress?.Invoke(0, total);
			foreach ((string relative, FileInfo info, Entry want) in toHash)
			{
				cancel.ThrowIfCancellationRequested();
				string sha;
				try { sha = Sha1Of(info.FullName, n => { done += n; progress?.Invoke(done, total); }); }
				catch (IOException) { result.Changed.Add(relative); continue; }
				cache[relative] = new CacheEntry { Size = info.Length, Time = info.LastWriteTimeUtc.Ticks, Sha1 = sha };
				if (!string.Equals(sha, want.Sha1, StringComparison.OrdinalIgnoreCase)) result.Changed.Add(relative);
			}
			foreach (string relative in FilesOf(root))
				if (!m.Files.ContainsKey(relative)) result.Extra.Add(relative);
			result.Changed.Sort(StringComparer.OrdinalIgnoreCase);
			result.Missing.Sort(StringComparer.OrdinalIgnoreCase);
			result.Extra.Sort(StringComparer.OrdinalIgnoreCase);
			SaveCache(cachePath, cache);
			return result;
		}

		private static Dictionary<string, CacheEntry> LoadCache(string path)
		{
			try
			{
				if (path != null && File.Exists(path))
				{
					var read = JsonSerializer.Deserialize<Dictionary<string, CacheEntry>>(File.ReadAllText(path), Json);
					if (read != null) return new Dictionary<string, CacheEntry>(read, StringComparer.OrdinalIgnoreCase);
				}
			}
			catch (Exception) { }
			return new Dictionary<string, CacheEntry>(StringComparer.OrdinalIgnoreCase);
		}

		private static void SaveCache(string path, Dictionary<string, CacheEntry> cache)
		{
			if (path == null) return;
			try
			{
				Directory.CreateDirectory(Path.GetDirectoryName(path));
				File.WriteAllText(path, JsonSerializer.Serialize(cache, Json));
			}
			catch (Exception) { }
		}
	}
}
