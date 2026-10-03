// The game's files checked against the release's list (OpenFF.Content.GameFiles, Data/game-files/<game>-<source>.json.gz):
// once at every start, in the background, for a game played from its Steam install, and again whenever the player asks
// (the Esc menu's Settings > Check game files). A first check reads the install's ~600 MB in about a second on an SSD;
// later ones read only the files written since (a cache by size and time written, beside the client's settings).
//
// What it finds goes to the log always. Files that differ from the release are said over the title (GameFilesScreen) -
// a game folder with a mod installed into it, the phone version's files or a damaged download is what makes a menu lay
// out wrong or a screen crash - with Steam's Verify integrity of game files as the way back, and "don't warn about these
// files again" for a player who changed them on purpose.

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenFF.Content;

namespace OpenFF.Client
{
	internal static class GameFileCheck
	{
		public enum Stage { Idle, Checking, Done, NoList, Failed }

		public static Stage Now { get; private set; } = Stage.Idle;
		public static OpenFF.Content.GameFiles.Result Result { get; private set; }
		/// <summary>The bytes read so far and in all, while checking.</summary>
		public static long Got, Total;
		/// <summary>Whether the player asked for this check (the answer is said then, whatever it is).</summary>
		public static bool Asked { get; private set; }
		/// <summary>The game's name for the panel ("Final Fantasy III").</summary>
		public static string GameName => Launch.Game == "ff4" ? "Final Fantasy IV" : "Final Fantasy III";
		public static string Error { get; private set; }

		private static string Folder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OpenFF");
		private static string CachePath => Path.Combine(Folder, "game-files-" + Launch.Game + ".json");
		private static string DismissedPath => Path.Combine(Folder, "game-files-" + Launch.Game + ".ok");

		/// <summary>The list for the game being played, from its install: Steam's (GOG's copy is checked against it too until it has a list of its own).</summary>
		private static string ListPath => Path.Combine(AppContext.BaseDirectory, "Data", "game-files", Launch.Game + "-steam.json.gz");

		/// <summary>A check in the background; asked is the player's (Settings), which says its answer even when the files are fine.</summary>
		public static void Start(bool asked)
		{
			if (Now == Stage.Checking) { Asked |= asked; return; }
			string root = Launch.ResolveRoot();
			Asked = asked;
			Result = null;
			Error = null;
			Got = Total = 0;
			if (root == null || !File.Exists(ListPath))
			{
				Now = Stage.NoList;
				if (asked) Log.Write(LogChannel.General, "game files: no list to check " + (root == null ? "the Content directory" : root) + " against");
				return;
			}
			Now = Stage.Checking;
			Task.Run(() =>
			{
				DateTime started = DateTime.UtcNow;
				try
				{
					OpenFF.Content.GameFiles.Manifest list = OpenFF.Content.GameFiles.Read(ListPath);
					OpenFF.Content.GameFiles.Result r = OpenFF.Content.GameFiles.Check(root, list, CachePath, (got, total) => { Got = got; Total = total; });
					Result = r;
					string took = (DateTime.UtcNow - started).TotalSeconds.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + " s";
					if (r.Clean) Log.Write(LogChannel.General, "game files: " + r.Listed + " of " + GameName + " as the " + list.Source + " release has them (" + took + ")");
					else
					{
						Log.Write(LogChannel.General, "game files: " + r.Changed.Count + " changed and " + r.Missing.Count + " missing of the " + r.Listed + " the " + list.Source + " release has"
							+ (r.OtherVersion ? " - likely another version of the game" : "") + " (" + took + "); Steam's Verify integrity of game files puts them back");
						foreach (string f in r.Changed.Take(20)) Log.Write(LogChannel.General, "game files:   changed " + f);
						foreach (string f in r.Missing.Take(20)) Log.Write(LogChannel.General, "game files:   missing " + f);
					}
					if (r.Extra.Count > 0) Log.Write(LogChannel.File, "game files: " + r.Extra.Count + " file(s) the release does not have, e.g. " + string.Join(", ", r.Extra.Take(5)));
					Now = Stage.Done;
				}
				catch (Exception ex)
				{
					Error = ex.Message;
					Log.Write(LogChannel.General, "game files: the check failed: " + ex.Message);
					Now = Stage.Failed;
				}
			});
		}

		/// <summary>Whether the panel has something to say: a check asked for (any answer), or files that differ and were not set aside.</summary>
		public static bool Worth
		{
			get
			{
				if (Asked) return Now != Stage.Idle;
				return Now == Stage.Done && Result != null && !Result.Clean && !Dismissed(Result.Signature);
			}
		}

		/// <summary>The panel closed: an asked check is answered.</summary>
		public static void Seen() { Asked = false; Now = Stage.Idle; }

		/// <summary>Don't warn about these files again: this set of differences remembered; a different set warns again.</summary>
		public static void Dismiss()
		{
			try
			{
				Directory.CreateDirectory(Folder);
				if (Result != null) File.WriteAllText(DismissedPath, Result.Signature);
				Log.Write(LogChannel.General, "game files: the differences are set aside; the check warns again if they change");
			}
			catch (Exception) { }
			Seen();
		}

		private static bool Dismissed(string signature)
		{
			try { return File.Exists(DismissedPath) && File.ReadAllText(DismissedPath).Trim() == signature; }
			catch (Exception) { return false; }
		}
	}
}
