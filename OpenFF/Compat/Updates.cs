// Updates from GitHub's releases: the client asks, as it starts, whether a newer release than its own
// is out; on the title it offers it (UpdateScreen), downloads its zip with its progress, checks it
// against the release's .sha256, unpacks it into the temp folder, and hands over to OpenFF.Updater -
// a small native program, run from a copy in the temp folder so it can replace everything in the
// install folder, itself included - which puts the files in place and starts the game again.
//
// Only an installed release checks (OpenFF.Updater.exe beside the client; a source build has none),
// never under a drive or a trace, and never with settings.json's "updates": "off" or --no-update.
// "ask" (the default) offers each new version once - "Skip this version" keeps it quiet until the
// next - and "auto" downloads and installs it on the title without asking. Offline, nothing shows.
//
// --update-feed=<url or file> reads another release's description (GitHub's releases/latest JSON) in
// place of the repository's: a test's, whose assets may be files on disk. It checks under a drive.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OpenFF.Client
{
	internal static class Updates
	{
		public const string Feed = "https://api.github.com/repos/zerratar/openff/releases/latest";

		public enum Stage { Idle, Checking, UpToDate, Available, Downloading, Verifying, Unpacking, Installing, Failed }

		/// <summary>A release as GitHub describes it: its version, its notes, the zip and its .sha256.</summary>
		public sealed class Release
		{
			public Version Version;
			public string Tag, Name, Notes, ZipUrl, ShaUrl, ZipName;
			public long ZipSize;
		}

		public static Stage Now { get; private set; } = Stage.Idle;
		public static Release Found { get; private set; }
		public static string Error { get; private set; }
		/// <summary>The download's bytes so far and in all, and its rate in bytes a second.</summary>
		public static long Got, Total;
		public static double Rate;
		/// <summary>Whether the check was asked for (Settings' Check now): an up-to-date answer is said then.</summary>
		public static bool Asked { get; private set; }

		private static CancellationTokenSource _cancel;
		private static readonly object _sync = new object();

		/// <summary>The version running.</summary>
		public static Version Current => typeof(Updates).Assembly.GetName().Version ?? new Version(0, 0);

		private static string TestFeed => Options.Get("update-feed");

		/// <summary>Whether this run can update itself: an installed release (its updater beside it), not a test's run, not turned off.</summary>
		public static bool Possible
		{
			get
			{
				if (Options.Get("no-update") != null) return false;
				if (TestFeed != null) return File.Exists(UpdaterPath);
				if (!string.IsNullOrEmpty(Options.Get("drive")) || !string.IsNullOrEmpty(Options.Get("trace"))) return false;
				return File.Exists(UpdaterPath);
			}
		}

		public static string Mode => NormaliseMode(DisplaySettings.Current.Updates);

		public static string NormaliseMode(string mode)
		{
			string m = (mode ?? "").Trim().ToLowerInvariant();
			return m == "auto" || m == "automatic" ? "auto" : m == "off" || m == "never" ? "off" : "ask";
		}

		private static string UpdaterPath => Path.Combine(AppContext.BaseDirectory, "OpenFF.Updater.exe");

		private static string Work => Path.Combine(Path.GetTempPath(), "OpenFF-update");

		/// <summary>As the client starts: a check in the background, when this run can update and the player has not turned it off.</summary>
		public static void CheckAtStart()
		{
			if (!Possible || Mode == "off") return;
			Check(asked: false);
		}

		/// <summary>A check in the background (Settings' Check now: asked, so its answer is said whatever it is).</summary>
		public static void Check(bool asked)
		{
			lock (_sync)
			{
				if (Now == Stage.Checking || Now == Stage.Downloading || Now == Stage.Verifying || Now == Stage.Unpacking || Now == Stage.Installing) return;
				if (!Possible) { Asked = asked; Error = "only an installed release updates itself, not a source build"; Now = asked ? Stage.Failed : Stage.Idle; return; }
				Asked = asked;
				Now = Stage.Checking;
				Error = null;
			}
			Task.Run(async () =>
			{
				try
				{
					Release latest = await Latest();
					Found = latest;
					bool newer = latest != null && latest.Version > Current;
					bool skipped = !asked && latest != null && string.Equals(DisplaySettings.Current.SkipVersion, latest.Tag, StringComparison.OrdinalIgnoreCase);
					Now = newer && !skipped ? Stage.Available : Stage.UpToDate;
					Log.Write(LogChannel.General, "updates: " + (latest == null ? "no release found" : "latest " + latest.Tag + (newer ? skipped ? " (skipped)" : " - newer than " + Current.ToString(3) : " - up to date")));
				}
				catch (Exception ex)
				{
					// Offline, or GitHub not answering: said only when asked.
					Error = Plain(ex);
					Now = asked ? Stage.Failed : Stage.Idle;
					Log.Write(LogChannel.General, "updates: check failed - " + Error);
				}
			});
		}

		/// <summary>The latest release as the feed describes it, with its zip and .sha256; null when it has no zip.</summary>
		private static async Task<Release> Latest()
		{
			string json;
			string feed = TestFeed ?? Feed;
			if (IsLocal(feed)) json = await File.ReadAllTextAsync(LocalPath(feed));
			else
			{
				using HttpClient http = Client();
				http.Timeout = TimeSpan.FromSeconds(15);
				json = await http.GetStringAsync(feed);
			}
			using JsonDocument doc = JsonDocument.Parse(json);
			JsonElement root = doc.RootElement;
			string tag = Str(root, "tag_name");
			if (tag == null || !Version.TryParse(tag.TrimStart('v', 'V').Split('-')[0], out Version version)) return null;
			Release r = new Release { Tag = tag, Version = version, Name = Str(root, "name"), Notes = Str(root, "body") ?? "" };
			if (root.TryGetProperty("assets", out JsonElement assets))
			{
				foreach (JsonElement a in assets.EnumerateArray())
				{
					string name = Str(a, "name") ?? "", url = Str(a, "browser_download_url");
					if (name.EndsWith("-win-x64.zip", StringComparison.OrdinalIgnoreCase)) { r.ZipName = name; r.ZipUrl = url; r.ZipSize = a.TryGetProperty("size", out JsonElement s) && s.TryGetInt64(out long n) ? n : 0; }
					else if (name.EndsWith("-win-x64.zip.sha256", StringComparison.OrdinalIgnoreCase)) r.ShaUrl = url;
				}
			}
			return r.ZipUrl == null ? null : r;
		}

		private static string Str(JsonElement e, string name) => e.TryGetProperty(name, out JsonElement v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;

		private static HttpClient Client()
		{
			HttpClient http = new HttpClient();
			http.DefaultRequestHeaders.UserAgent.ParseAdd("OpenFF/" + Current.ToString(3));
			return http;
		}

		private static bool IsLocal(string where) => where != null && (where.StartsWith("file:", StringComparison.OrdinalIgnoreCase) || (!where.StartsWith("http:", StringComparison.OrdinalIgnoreCase) && !where.StartsWith("https:", StringComparison.OrdinalIgnoreCase)));

		private static string LocalPath(string where) => where.StartsWith("file:", StringComparison.OrdinalIgnoreCase) ? new Uri(where).LocalPath : where;

		/// <summary>"Skip this version": not offered again until a newer one.</summary>
		public static void Skip()
		{
			if (Found == null) return;
			DisplaySettings.Current.SkipVersion = Found.Tag;
			DisplaySettings.Current.Save();
			Now = Stage.UpToDate;
			Log.Write(LogChannel.General, "updates: " + Found.Tag + " skipped");
		}

		/// <summary>"Later": put away until the next start.</summary>
		public static void Later() { Now = Stage.UpToDate; Log.Write(LogChannel.General, "updates: later"); }

		public static void Cancel() => _cancel?.Cancel();

		/// <summary>Downloads, checks and unpacks the release in the background; on success the updater is started and ReadyToExit is set - the game then closes.</summary>
		public static void Install()
		{
			Release r = Found;
			if (r == null) return;
			lock (_sync)
			{
				if (Now == Stage.Downloading || Now == Stage.Verifying || Now == Stage.Unpacking || Now == Stage.Installing) return;
				Now = Stage.Downloading;
				Error = null;
				Got = 0; Total = r.ZipSize; Rate = 0;
				_cancel = new CancellationTokenSource();
			}
			CancellationToken token = _cancel.Token;
			Task.Run(async () =>
			{
				try
				{
					string dir = Path.Combine(Work, r.Tag);
					if (Directory.Exists(dir)) Directory.Delete(dir, true);
					Directory.CreateDirectory(dir);
					string zip = Path.Combine(dir, r.ZipName ?? "release.zip");
					await Download(r.ZipUrl, zip, token);

					Now = Stage.Verifying;
					if (r.ShaUrl == null) throw new InvalidDataException("the release has no .sha256 to check its download against");
					string wanted = (await ReadText(r.ShaUrl, token)).Trim().Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "";
					string have;
					using (FileStream f = File.OpenRead(zip)) have = Convert.ToHexString(await SHA256.HashDataAsync(f, token));
					if (!string.Equals(have, wanted, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("the download does not match the release's checksum - it was not installed");
					Log.Write(LogChannel.General, "updates: " + r.ZipName + " checked (" + have.Substring(0, 12) + "...)");

					Now = Stage.Unpacking;
					string files = Path.Combine(dir, "files");
					ZipFile.ExtractToDirectory(zip, files);
					// The zip holds an OpenFF folder (publish.cmd zips dist\OpenFF); the release is what is in it.
					string from = File.Exists(Path.Combine(files, "OpenFF.exe")) ? files : Directory.GetDirectories(files).FirstOrDefault(d => File.Exists(Path.Combine(d, "OpenFF.exe")));
					if (from == null) throw new InvalidDataException("the release's zip has no OpenFF.exe");

					Now = Stage.Installing;
					// The updater runs from the temp folder - the new release's own, else this one's.
					string updater = Path.Combine(Work, "OpenFF.Updater.exe");
					string fresh = Path.Combine(from, "OpenFF.Updater.exe");
					File.Copy(File.Exists(fresh) ? fresh : UpdaterPath, updater, overwrite: true);
					string to = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
					List<string> args = new List<string> { "--from", from, "--to", to, "--wait", Environment.ProcessId.ToString(), "--run", Path.GetFileName(Environment.ProcessPath ?? "OpenFF.exe") };
					foreach (string a in Restart.TitleArguments()) { args.Add("--arg"); args.Add(a); }
					args.Add("--log"); args.Add(Path.Combine(to, "logs", "update.log"));
					// Through the shell: a window of its own, which says what it does and outlives this process's console.
					Process.Start(new ProcessStartInfo(updater, string.Join(" ", args.Select(Quote))) { UseShellExecute = true, WorkingDirectory = Work });
					Log.Write(LogChannel.General, "updates: " + r.Tag + " unpacked; the updater takes over");
					ReadyToExit = true;
				}
				catch (OperationCanceledException)
				{
					Now = Stage.Available;
					Log.Write(LogChannel.General, "updates: download cancelled");
				}
				catch (Exception ex)
				{
					Error = Plain(ex);
					Now = Stage.Failed;
					Log.Write(LogChannel.General, "updates: install failed - " + Error);
				}
			});
		}

		/// <summary>An argument quoted for a Windows command line: in quotes, its quotes escaped, the backslashes before them doubled.</summary>
		private static string Quote(string arg)
		{
			if (arg.Length > 0 && arg.IndexOfAny(new[] { ' ', '\t', '"' }) < 0) return arg;
			System.Text.StringBuilder q = new System.Text.StringBuilder("\"");
			int slashes = 0;
			foreach (char c in arg)
			{
				if (c == '\\') { slashes++; continue; }
				q.Append('\\', c == '"' ? slashes * 2 + 1 : slashes);
				slashes = 0;
				q.Append(c);
			}
			q.Append('\\', slashes * 2).Append('"');
			return q.ToString();
		}

		/// <summary>Set once the updater is running: the game closes so it can put the files in place.</summary>
		public static volatile bool ReadyToExit;

		private static async Task Download(string url, string to, CancellationToken token)
		{
			Stopwatch clock = Stopwatch.StartNew();
			long lastBytes = 0;
			double lastTime = 0;
			Stream source;
			HttpClient http = null;
			HttpResponseMessage response = null;
			if (IsLocal(url))
			{
				source = File.OpenRead(LocalPath(url));
				Total = source.Length;
			}
			else
			{
				http = Client();
				http.Timeout = Timeout.InfiniteTimeSpan;
				response = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, token);
				response.EnsureSuccessStatusCode();
				if (response.Content.Headers.ContentLength is long n) Total = n;
				source = await response.Content.ReadAsStreamAsync(token);
			}
			try
			{
				using FileStream file = File.Create(to);
				byte[] buffer = new byte[81920];
				// --update-throttle=<KB/s>: a test's slow download, to see the progress.
				int throttle = Options.GetInt("update-throttle", 0);
				while (true)
				{
					int read = await source.ReadAsync(buffer, 0, buffer.Length, token);
					if (read == 0) break;
					await file.WriteAsync(buffer, 0, read, token);
					Got += read;
					if (throttle > 0) await Task.Delay((int)(read * 1000L / (throttle * 1024L)), token);
					double now = clock.Elapsed.TotalSeconds;
					if (now - lastTime >= 0.5)
					{
						double rate = (Got - lastBytes) / (now - lastTime);
						Rate = Rate <= 0 ? rate : Rate * 0.6 + rate * 0.4;
						lastBytes = Got; lastTime = now;
					}
				}
			}
			finally
			{
				source.Dispose();
				response?.Dispose();
				http?.Dispose();
			}
			Log.Write(LogChannel.General, "updates: downloaded " + Got + " bytes in " + clock.Elapsed.TotalSeconds.ToString("0.0") + " s");
		}

		private static async Task<string> ReadText(string url, CancellationToken token)
		{
			if (IsLocal(url)) return await File.ReadAllTextAsync(LocalPath(url), token);
			using HttpClient http = Client();
			http.Timeout = TimeSpan.FromSeconds(30);
			return await http.GetStringAsync(url, token);
		}

		private static string Plain(Exception ex) => ex is HttpRequestException || ex is TaskCanceledException ? "could not reach GitHub (" + ex.Message + ")" : ex.Message;

		/// <summary>The first lines of the release's notes worth showing: its words without the markdown's marks.</summary>
		public static List<string> NoteLines(int most)
		{
			List<string> lines = new List<string>();
			foreach (string raw in (Found?.Notes ?? "").Replace("\r", "").Split('\n'))
			{
				string line = raw.Trim();
				if (line.Length == 0 || line.StartsWith("#")) continue;
				line = line.Replace("**", "").Replace("`", "");
				if (line.StartsWith("- ") || line.StartsWith("* ")) line = "•  " + line.Substring(2);
				lines.Add(line);
				if (lines.Count >= most) break;
			}
			return lines;
		}
	}
}
