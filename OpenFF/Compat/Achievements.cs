// Steam achievements in OpenFF: earned as the game itself earns them, when the game plays from its Steam install
// (steam_api.dll beside FF3_Win32.exe / FF4.exe - a GOG copy has none) with Steam running, and the player has not
// turned them off (settings.json "achievements": false, or --no-achievements for a run). Mods do not turn them off.
// While they are on, Steam counts the run as the game (SteamStats): the player shows in it and the playtime runs.
//
// What earns each is the game's own rule - FF4's from achievement_param.bbd beside FF4.exe (Ff4Achievements). FF3's
// eighteen are not read yet: FF3_Win32.exe decides them in its code, with no table to read.

using System;
using System.Collections.Generic;
using System.IO;

namespace OpenFF.Client
{
	internal static class Achievements
	{
		public const uint Ff3AppId = 239120, Ff4AppId = 312750;

		private static bool _started;
		private static readonly Queue<string> _toUnlock = new Queue<string>();
		private static readonly HashSet<string> _earned = new HashSet<string>(StringComparer.Ordinal);
		private static long _nextPump, _requestedAt;
		private static bool _dry, _toldReady;

		/// <summary>Connected to Steam as the game, achievements being earned.</summary>
		public static bool Active { get; private set; }

		/// <summary>The Steam install the game's content comes from (the folder with steam_api.dll), or null.</summary>
		public static string SteamInstall()
		{
			try
			{
				string files = ContentLocator.FindContentRoot();
				if (string.IsNullOrEmpty(files)) return null;
				string dir = Path.GetFullPath(files);
				for (int up = 0; up < 3 && dir != null; up++)
				{
					if (File.Exists(Path.Combine(dir, "steam_api.dll")) || File.Exists(Path.Combine(dir, "steam_api64.dll"))) return dir;
					dir = Path.GetDirectoryName(dir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
				}
			}
			catch (Exception) { }
			return null;
		}

		private static void Start()
		{
			_started = true;
			if (!DisplaySettings.Current.Achievements || Options.Get("no-achievements") != null)
			{
				Log.Write(LogChannel.General, "achievements: off (settings.json \"achievements\": false or --no-achievements)");
				return;
			}
			string install = SteamInstall();
			if (install == null)
			{
				Log.Write(LogChannel.General, "achievements: off - the game's files are not a Steam install");
				return;
			}
			uint app = GameProfile.IsFf4 ? Ff4AppId : Ff3AppId;
			if (!SteamStats.Connect(app, out string why))
			{
				Log.Write(LogChannel.General, "achievements: off - " + why);
				return;
			}
			Active = true;
			_dry = Options.Get("achievements-dry") != null;
			_requestedAt = Environment.TickCount64;
			if (GameProfile.IsFf4) Ff4Achievements.Load(install);
			Log.Write(LogChannel.General, "achievements: on - Steam counts this as app " + app + (GameProfile.IsFf4 ? ", " + Ff4Achievements.Count + " rule(s)" : ", FF3's rules are not read yet"));
		}

		/// <summary>Earns an achievement by its Steam name (once; sent when Steam's stats are in).</summary>
		public static void Unlock(string name)
		{
			if (!Active || string.IsNullOrEmpty(name) || _earned.Contains(name)) return;
			_earned.Add(name);
			_toUnlock.Enqueue(name);
		}

		/// <summary>Whether an achievement is earned, as far as this run knows (Steam's record once its stats are in).</summary>
		public static bool IsEarned(string name)
		{
			if (_earned.Contains(name)) return true;
			if (SteamStats.Achieved(name) == true) { _earned.Add(name); return true; }
			return false;
		}

		/// <summary>Once a step: connects on the first, then keeps Steam's callbacks drained, sends what was earned and runs the
		/// game's checks that were asked for.</summary>
		public static void Tick()
		{
			if (!_started) Start();
			if (!Active) return;
			long now = Environment.TickCount64;
			if (now >= _nextPump)
			{
				_nextPump = now + 250;
				SteamStats.Pump();
				if (!SteamStats.Ready && now - _requestedAt > 10000) { _requestedAt = now; SteamStats.RequestStats(); }
			}
			if (!SteamStats.Ready) return;
			if (!_toldReady)
			{
				_toldReady = true;
				if (GameProfile.IsFf4) Log.Write(LogChannel.General, "achievements: Steam's record in - " + Ff4Achievements.EarnedCount() + " of " + Ff4Achievements.NameCount + " earned" + (_dry ? " (dry run: nothing will be sent)" : ""));
			}
			while (_toUnlock.Count > 0)
			{
				string name = _toUnlock.Dequeue();
				bool? had = SteamStats.Achieved(name);
				if (had == true) continue;
				if (had == null) { Log.Write(LogChannel.General, "achievements: Steam does not know " + name); continue; }
				if (_dry) { Log.Write(LogChannel.General, "achievements: " + name + " would be earned (dry run)"); continue; }
				Log.Write(LogChannel.General, "achievements: " + name + (SteamStats.Unlock(name) ? " earned" : " not taken by Steam"));
			}
			if (GameProfile.IsFf4) Ff4Achievements.Tick();
		}

		public static void Shutdown()
		{
			if (Active) SteamStats.Release();
			Active = false;
		}
	}
}
