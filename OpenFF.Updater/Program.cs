// OpenFF's updater: puts a downloaded release in place of the installed one, then starts the game again.
//
// The game downloads the release, checks it against its .sha256 and unpacks it into a staging folder,
// copies this program into the temp folder and runs it from there, then exits. Run from outside the
// install folder, nothing in it is in use: this replaces every file - the runtime's, Crystal's, the
// updater's own - and the new release's updater stands where the old one did.
//
//   OpenFF.Updater --from <unpacked release> --to <install folder> --wait <pid> --run OpenFF.exe [--arg <a>]... [--log <file>]
//
// What it does:
//   waits for the game (the --wait process) and for anything else running from the install folder (Crystal) to close;
//   copies the release's files over the install's, each file it replaces kept in <install>\.update-backup first;
//   leaves the player's things alone - mods\ keeps its files (a mod the release brings is added only when not there),
//   logs\ is not touched, and saves and settings are in %LocalAppData%\OpenFF, outside the folder anyway;
//   takes away what the release no longer has in Data\ (the client's own data, which it would read);
//   on any failure puts every file back as it was; then starts the game (--run, with its --arg's).

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace OpenFF.Updater
{
	internal static class Program
	{
		private static StreamWriter _log;

		private static int Main(string[] args)
		{
			string from = null, to = null, run = null, log = null;
			List<int> wait = new List<int>();
			List<string> runArgs = new List<string>();
			for (int i = 0; i < args.Length; i++)
			{
				string a = args[i], next = i + 1 < args.Length ? args[i + 1] : null;
				switch (a)
				{
					case "--from": from = next; i++; break;
					case "--to": to = next; i++; break;
					case "--run": run = next; i++; break;
					case "--arg": if (next != null) runArgs.Add(next); i++; break;
					case "--log": log = next; i++; break;
					case "--wait": if (int.TryParse(next, out int pid)) wait.Add(pid); i++; break;
				}
			}
			Console.Title = "OpenFF - updating";
			try
			{
				if (log != null) { Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(log))); _log = new StreamWriter(log, append: true) { AutoFlush = true }; }
			}
			catch (Exception) { }
			Say("OpenFF updater " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
			if (from == null || to == null || !Directory.Exists(from) || !Directory.Exists(to))
			{
				Say("usage: OpenFF.Updater --from <unpacked release> --to <install folder> --wait <pid> --run OpenFF.exe [--arg <a>]... [--log <file>]");
				return 2;
			}
			from = Path.GetFullPath(from);
			to = Path.GetFullPath(to);
			if (!File.Exists(Path.Combine(from, "OpenFF.exe")))
			{
				Say("The release has no OpenFF.exe - nothing was changed.");
				Pause();
				return 3;
			}

			WaitForExit(wait, to);
			bool ok = Install(from, to);
			if (ok) Say("OpenFF is up to date.");
			else Say("The update did not go in; OpenFF is as it was.");
			if (run != null)
			{
				try
				{
					ProcessStartInfo start = new ProcessStartInfo(Path.Combine(to, run)) { UseShellExecute = false, WorkingDirectory = to };
					foreach (string a in runArgs) start.ArgumentList.Add(a);
					Process.Start(start);
					Say("Started " + run + ".");
				}
				catch (Exception ex) { Say("Could not start " + run + ": " + ex.Message); Pause(); }
			}
			if (!ok) Pause();
			return ok ? 0 : 1;
		}

		private static void Say(string line)
		{
			Console.WriteLine(line);
			try { _log?.WriteLine(line); } catch (Exception) { }
		}

		/// <summary>A failure's message left on the screen a while, so it can be read.</summary>
		private static void Pause() => Thread.Sleep(8000);

		/// <summary>The game (and anything else running from the install folder - Crystal) closed, for up to two minutes each.</summary>
		private static void WaitForExit(List<int> pids, string folder)
		{
			foreach (int pid in pids)
			{
				try
				{
					Process p = Process.GetProcessById(pid);
					Say("Waiting for the game to close...");
					if (!p.WaitForExit(120000)) Say("It is still running; carrying on (a file in use is tried again).");
				}
				catch (ArgumentException) { }   // gone already
				catch (Exception ex) { Say("(" + ex.Message + ")"); }
			}
			DateTime until = DateTime.UtcNow.AddMinutes(2);
			bool said = false;
			while (DateTime.UtcNow < until)
			{
				List<string> running = RunningFrom(folder);
				if (running.Count == 0) return;
				if (!said) { Say("Waiting for " + string.Join(", ", running) + " to close..."); said = true; }
				Thread.Sleep(500);
			}
		}

		private static List<string> RunningFrom(string folder)
		{
			List<string> names = new List<string>();
			foreach (Process p in Process.GetProcesses())
			{
				try
				{
					string path = p.MainModule?.FileName;
					if (path != null && path.StartsWith(folder + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) names.Add(Path.GetFileName(path));
				}
				catch (Exception) { }   // another user's, or a system process
			}
			return names;
		}

		private enum Change { Replaced, Added, Removed }

		/// <summary>The release's files over the install's; every change undone if one fails.</summary>
		private static bool Install(string from, string to)
		{
			string backup = Path.Combine(to, ".update-backup");
			List<(Change What, string Rel)> done = new List<(Change, string)>();
			try
			{
				if (Directory.Exists(backup)) Directory.Delete(backup, true);
				string[] files = Directory.GetFiles(from, "*", SearchOption.AllDirectories);
				Say("Installing " + files.Length + " files...");
				HashSet<string> releaseData = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				foreach (string file in files)
				{
					string rel = Path.GetRelativePath(from, file);
					if (Under(rel, "Data")) releaseData.Add(rel);
					if (Under(rel, "logs")) continue;
					string dest = Path.Combine(to, rel);
					// mods\: the player's own; a mod the release brings (the Showcase) only when it is not there yet.
					if (Under(rel, "mods") && (File.Exists(dest) || rel.EndsWith("loadorder.json", StringComparison.OrdinalIgnoreCase))) continue;
					Directory.CreateDirectory(Path.GetDirectoryName(dest));
					if (File.Exists(dest))
					{
						string kept = Path.Combine(backup, rel);
						Directory.CreateDirectory(Path.GetDirectoryName(kept));
						Retry(() => File.Move(dest, kept));
						done.Add((Change.Replaced, rel));
					}
					else done.Add((Change.Added, rel));
					Retry(() => File.Copy(file, dest));
				}
				// What the client's own data no longer has: it would still be read.
				string data = Path.Combine(to, "Data");
				if (Directory.Exists(data) && releaseData.Count > 0)
				{
					foreach (string file in Directory.GetFiles(data, "*", SearchOption.AllDirectories))
					{
						string rel = Path.GetRelativePath(to, file);
						if (releaseData.Contains(rel)) continue;
						string kept = Path.Combine(backup, rel);
						Directory.CreateDirectory(Path.GetDirectoryName(kept));
						Retry(() => File.Move(file, kept));
						done.Add((Change.Removed, rel));
					}
				}
				try { Directory.Delete(backup, true); } catch (Exception) { }
				Say(done.Count(d => d.What == Change.Replaced) + " replaced, " + done.Count(d => d.What == Change.Added) + " added, " + done.Count(d => d.What == Change.Removed) + " removed.");
				return true;
			}
			catch (Exception ex)
			{
				Say("Failed: " + ex.Message);
				Say("Putting the " + done.Count + " changed file(s) back...");
				for (int i = done.Count - 1; i >= 0; i--)
				{
					(Change what, string rel) = done[i];
					string dest = Path.Combine(to, rel), kept = Path.Combine(backup, rel);
					try
					{
						if (what != Change.Removed && File.Exists(dest)) File.Delete(dest);
						if (what != Change.Added && File.Exists(kept)) File.Move(kept, dest);
					}
					catch (Exception e) { Say("  " + rel + ": " + e.Message + " (a copy is in .update-backup)"); }
				}
				return false;
			}
		}

		private static bool Under(string rel, string dir) => rel.StartsWith(dir + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);

		/// <summary>A file operation tried again while the file is in use (a scanner, a process still closing), for up to fifteen seconds.</summary>
		private static void Retry(Action action)
		{
			for (int attempt = 0; ; attempt++)
			{
				try { action(); return; }
				catch (IOException) when (attempt < 30) { Thread.Sleep(500); }
				catch (UnauthorizedAccessException) when (attempt < 30) { Thread.Sleep(500); }
			}
		}
	}
}
