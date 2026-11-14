// Steam's achievements without Valve's steam_api DLL: the player's own Steam client (steamclient64.dll in the Steam
// folder) asked directly, the way Steam Achievement Manager does - nothing of Valve's is shipped with OpenFF. With the
// game's app id in the environment (SteamAppId, as Steam sets it for a game it starts) the client counts this process as
// that game: Steam shows the player in it, its achievements can be set, and the playtime runs.
//
// The two interfaces are C++ objects; their methods are called through their tables in the order the Steamworks headers
// declare them (MSVC's: overloads in reverse, which leaves these indices as they are):
//   ISteamClient (SteamClient017)                0 CreateSteamPipe, 1 BReleaseSteamPipe, 2 ConnectToGlobalUser,
//                                                4 ReleaseUser, 13 GetISteamUserStats
//   ISteamUserStats (STEAMUSERSTATS_INTERFACE_VERSION011)
//                                                0 RequestCurrentStats, 6 GetAchievement, 7 SetAchievement, 10 StoreStats
// Answers come back as callbacks the client queues on the pipe (Steam_BGetCallback / Steam_FreeLastCallback); 1101 is
// UserStatsReceived_t - the stats are in and achievements can be read and set.

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace OpenFF.Client
{
	internal static unsafe class SteamStats
	{
		[DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
		private static extern bool SetDllDirectory(string path);

		[DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
		private static extern IntPtr LoadLibraryEx(string path, IntPtr file, uint flags);

		[DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true)]
		private static extern IntPtr GetProcAddress(IntPtr module, string name);

		[StructLayout(LayoutKind.Sequential)]
		private struct CallbackMsg
		{
			public int User;
			public int Callback;
			public IntPtr Param;
			public int Size;
		}

		private const int UserStatsReceived = 1101;

		private static IntPtr _client, _stats;
		private static int _pipe, _user;
		private static delegate* unmanaged<int, CallbackMsg*, byte> _getCallback;
		private static delegate* unmanaged<int, byte> _freeCallback;

		/// <summary>Connected to Steam as the game; Ready once its stats are in.</summary>
		public static bool Connected => _stats != IntPtr.Zero;
		public static bool Ready { get; private set; }
		public static uint AppId { get; private set; }

		/// <summary>Steam's folder, from the registry (HKCU\Software\Valve\Steam, SteamPath).</summary>
		public static string SteamFolder()
		{
			try
			{
				using Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
				string path = key?.GetValue("SteamPath") as string;
				return string.IsNullOrEmpty(path) ? null : Path.GetFullPath(path.Replace('/', Path.DirectorySeparatorChar));
			}
			catch (Exception) { return null; }
		}

		private static IntPtr Method(IntPtr instance, int index) => (*(IntPtr**)instance)[index];

		/// <summary>Connects to the running Steam client as <paramref name="appId"/>; false (with why) when Steam is not
		/// installed, not running, or will not have it.</summary>
		public static bool Connect(uint appId, out string why)
		{
			why = null;
			if (!OperatingSystem.IsWindows()) { why = "Steam's client is asked on Windows only"; return false; }
			string folder = SteamFolder();
			string dll = folder == null ? null : Path.Combine(folder, Environment.Is64BitProcess ? "steamclient64.dll" : "steamclient.dll");
			if (dll == null || !File.Exists(dll)) { why = "Steam is not installed"; return false; }
			// The client reads the app id from the environment as it connects, as it does for a game Steam started.
			Environment.SetEnvironmentVariable("SteamAppId", appId.ToString());
			Environment.SetEnvironmentVariable("SteamGameId", appId.ToString());
			SetDllDirectory(folder + ";" + Path.Combine(folder, "bin"));
			IntPtr module = LoadLibraryEx(dll, IntPtr.Zero, 0x8);   // LOAD_WITH_ALTERED_SEARCH_PATH: its own tier0 / vstdlib beside it
			SetDllDirectory(null);
			if (module == IntPtr.Zero) { why = "steamclient would not load (" + Marshal.GetLastWin32Error() + ")"; return false; }
			var create = (delegate* unmanaged<byte*, int*, IntPtr>)GetProcAddress(module, "CreateInterface");
			_getCallback = (delegate* unmanaged<int, CallbackMsg*, byte>)GetProcAddress(module, "Steam_BGetCallback");
			_freeCallback = (delegate* unmanaged<int, byte>)GetProcAddress(module, "Steam_FreeLastCallback");
			if (create == null || _getCallback == null || _freeCallback == null) { why = "steamclient has no CreateInterface"; return false; }
			int code;
			fixed (byte* version = Ascii("SteamClient017")) _client = create(version, &code);
			if (_client == IntPtr.Zero) { why = "Steam's client has no SteamClient017"; return false; }
			_pipe = ((delegate* unmanaged<IntPtr, int>)Method(_client, 0))(_client);
			if (_pipe == 0) { why = "Steam is not running"; return false; }
			_user = ((delegate* unmanaged<IntPtr, int, int>)Method(_client, 2))(_client, _pipe);
			if (_user == 0) { Release(); why = "no one is logged in to Steam"; return false; }
			fixed (byte* version = Ascii("STEAMUSERSTATS_INTERFACE_VERSION011"))
				_stats = ((delegate* unmanaged<IntPtr, int, int, byte*, IntPtr>)Method(_client, 13))(_client, _user, _pipe, version);
			if (_stats == IntPtr.Zero) { Release(); why = "Steam has no user stats for this"; return false; }
			AppId = appId;
			RequestStats();
			return true;
		}

		/// <summary>Asks Steam for the game's stats; Ready once UserStatsReceived comes back (Pump).</summary>
		public static void RequestStats()
		{
			if (_stats == IntPtr.Zero) return;
			((delegate* unmanaged<IntPtr, byte>)Method(_stats, 0))(_stats);
		}

		/// <summary>Drains the pipe's callbacks; the stats' arrival makes it Ready.</summary>
		public static void Pump()
		{
			if (_pipe == 0 || _getCallback == null) return;
			CallbackMsg msg;
			for (int guard = 0; guard < 64 && _getCallback(_pipe, &msg) != 0; guard++)
			{
				// UserStatsReceived_t: the game's id (u64), then the result (1 = OK).
				if (msg.Callback == UserStatsReceived && msg.Param != IntPtr.Zero && msg.Size >= 12)
				{
					ulong game = *(ulong*)msg.Param;
					int result = *(int*)((byte*)msg.Param + 8);
					if ((uint)game == AppId && result == 1) Ready = true;
				}
				_freeCallback(_pipe);
			}
		}

		/// <summary>Whether an achievement is earned (null: not known - the stats are not in, or no such name).</summary>
		public static bool? Achieved(string name)
		{
			if (!Ready) return null;
			byte achieved = 0;
			fixed (byte* text = Ascii(name))
			{
				byte ok = ((delegate* unmanaged<IntPtr, byte*, byte*, byte>)Method(_stats, 6))(_stats, text, &achieved);
				if (ok == 0) return null;
			}
			return achieved != 0;
		}

		/// <summary>Earns an achievement and sends it (StoreStats); false when Steam would not take it.</summary>
		public static bool Unlock(string name)
		{
			if (!Ready) return false;
			byte ok;
			fixed (byte* text = Ascii(name)) ok = ((delegate* unmanaged<IntPtr, byte*, byte>)Method(_stats, 7))(_stats, text);
			if (ok == 0) return false;
			((delegate* unmanaged<IntPtr, byte>)Method(_stats, 10))(_stats);
			return true;
		}

		public static void Release()
		{
			try
			{
				if (_client != IntPtr.Zero && _pipe != 0)
				{
					if (_user != 0) ((delegate* unmanaged<IntPtr, int, int, void>)Method(_client, 4))(_client, _pipe, _user);
					((delegate* unmanaged<IntPtr, int, byte>)Method(_client, 1))(_client, _pipe);
				}
			}
			catch (Exception) { }
			_stats = IntPtr.Zero;
			_pipe = _user = 0;
			Ready = false;
		}

		private static byte[] Ascii(string text)
		{
			byte[] bytes = new byte[Encoding.ASCII.GetByteCount(text) + 1];
			Encoding.ASCII.GetBytes(text, 0, text.Length, bytes, 0);
			return bytes;
		}
	}
}
