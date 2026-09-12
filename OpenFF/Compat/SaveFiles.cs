// Save-game storage.
//
// The phone build wrote through Android's per-app private storage, which the shim
// mapped onto IsolatedStorage. On Windows the natural home is the user's roaming
// application data, where saves survive reinstalls and are easy to find and back up.

using System;
using System.IO;

namespace OpenFF.Client
{
	internal static class SaveFiles
	{
		private static string _directory;

		/// <summary>%APPDATA%\FF3, created on first use.</summary>
		public static string Directory_
		{
			get
			{
				if (_directory == null)
				{
					_directory = Path.Combine(
						Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FF3");
					System.IO.Directory.CreateDirectory(_directory);
					Log.Write(LogChannel.File, "save directory: " + _directory);
					Recover();
				}
				return _directory;
			}
		}

		/// <summary>
		/// A save profile keeps a game's saves apart from the game's own: "fellowship" writes fellowship-save.bin,
		/// fellowship-save.progression.json and its own mod chunks (Game.Title.NewGame / Continue set it), so a game
		/// played together never touches a game played alone. Null is the game's own.
		/// </summary>
		public static string Profile
		{
			get => _profile;
			set
			{
				string p = string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();
				if (p == _profile) return;
				_profile = p;
				Log.Write(LogChannel.General, "save profile: " + (_profile ?? "the game's own"));
				EngineHost.SaveProfileChanged();
			}
		}
		private static string _profile;

		/// <summary>The file's path; FF4's card data is kept apart from FF3's (ff4-save.bin), so one game's saves never show as the other's; a save profile puts its own name in front.</summary>
		public static string PathFor(string name) => Path.Combine(Directory_, (GameProfile.IsFf4 ? "ff4-" : "") + (_profile != null ? _profile + "-" : "") + name);

		/// <summary>Whether a save file exists under a profile (null for the game's own).</summary>
		public static bool HasSave(string profile)
		{
			string p = string.IsNullOrWhiteSpace(profile) ? null : profile.Trim().ToLowerInvariant();
			string path = Path.Combine(Directory_, (GameProfile.IsFf4 ? "ff4-" : "") + (p != null ? p + "-" : "") + "save.bin");
			try { return File.Exists(path) && new FileInfo(path).Length > 0; } catch (Exception) { return false; }
		}

		/// <summary>
		/// Builds before 0.1.1 read the save through IsolatedStorage - a store keyed by the
		/// executable's path - while creating it here, so a save made from one folder was invisible
		/// to a build in another and the file here stayed blank. Once, when the file here is missing
		/// or all zeros, the newest save.bin with anything in it under %LocalAppData%\IsolatedStorage
		/// is brought over, so those saves are not lost.
		/// </summary>
		private static void Recover()
		{
			try
			{
				if (GameProfile.IsFf4) return;   // FF4 kept its data on the unified layer; nothing of its own to bring over
				string mine = Path.Combine(_directory, "save.bin");
				if (File.Exists(mine))
				{
					byte[] have = File.ReadAllBytes(mine);
					if (Array.Exists(have, b => b != 0)) return;
				}
				string isolated = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "IsolatedStorage");
				if (!System.IO.Directory.Exists(isolated)) return;
				FileInfo best = null;
				foreach (string file in System.IO.Directory.EnumerateFiles(isolated, "save.bin", SearchOption.AllDirectories))
				{
					FileInfo info = new FileInfo(file);
					if (info.Length != 65536) continue;
					if (!Array.Exists(File.ReadAllBytes(file), b => b != 0)) continue;
					if (best == null || info.LastWriteTimeUtc > best.LastWriteTimeUtc) best = info;
				}
				if (best == null) return;
				File.Copy(best.FullName, mine, overwrite: true);
				Log.Write(LogChannel.General, "save: recovered save.bin from " + best.FullName + " (" + best.LastWriteTime + ")");
			}
			catch (Exception ex)
			{
				Log.Write(LogChannel.File, "save: could not look for an older save: " + ex.Message);
			}
		}

		/// <summary>Creates a zero-filled save file of the given size, replacing any existing one.</summary>
		public static void Create(string name, int size)
		{
			try
			{
				File.WriteAllBytes(PathFor(name), new byte[size]);
				Log.Write(LogChannel.File, "created save file " + name + " (" + size + " bytes)");
			}
			catch (Exception ex)
			{
				Log.Write(LogChannel.File, "could not create save file " + name + ": " + ex.Message);
			}
		}
	}
}
