// Diagnostic logging for the port.
//
// Large parts of the decompiled game swallow exceptions and draw through an
// OpenGL ES 1.x emulation layer, so a porting bug usually surfaces as "nothing
// happened" rather than an error. This writes a log file on every run so that
// behaviour can be inspected after the fact.
//
// Log file:  <exe dir>\logs\ff3.log   (previous run kept as ff3.prev.log)
//            override with FF3_LOG_FILE=<path>
// Channels:  FF3_LOG=gl,texture       (comma separated, or "all", or "none")
//            General and Exception are always on.
//
// Every call site is rate limited by a key, so leaving channels on does not
// produce gigabytes: use First() for "show me the first N of these" and
// Sample() for "show me one in every N".

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace FF3
{
	[Flags]
	internal enum LogChannel
	{
		None = 0,
		General = 1 << 0,
		Exception = 1 << 1,
		Gl = 1 << 2,
		Texture = 1 << 3,
		Content = 1 << 4,
		File = 1 << 5,
		Sound = 1 << 6,
		Input = 1 << 7,
		Event = 1 << 8,

		/// <summary>Every thrown exception, including ones the game catches and ignores.
		/// Off by default because it is expensive; enable with FF3_LOG=firstchance.</summary>
		FirstChance = 1 << 9,

		All = ~0
	}

	internal static class Log
	{
		private const int FlushEveryLines = 200;
		private static readonly TimeSpan FlushEveryTime = TimeSpan.FromMilliseconds(500);

		private static readonly object _sync = new object();
		private static readonly StringBuilder _pending = new StringBuilder();
		private static readonly Dictionary<string, int> _counts = new Dictionary<string, int>();
		private static readonly Stopwatch _clock = Stopwatch.StartNew();

		private static LogChannel _channels = LogChannel.General | LogChannel.Exception;
		private static string _path;
		private static int _pendingLines;
		private static System.Threading.Timer _flushTimer;
		private static TimeSpan _lastFlush;
		private static bool _failed;

		public static string Path => _path;

		/// <summary>Called once from Main, before anything else touches the game.</summary>
		public static void Initialise()
		{
			lock (_sync)
			{
				_channels |= ParseChannels(Environment.GetEnvironmentVariable("FF3_LOG"));
				_path = Environment.GetEnvironmentVariable("FF3_LOG_FILE");
				if (string.IsNullOrEmpty(_path))
				{
					string dir = System.IO.Path.Combine(AppContext.BaseDirectory, "logs");
					Directory.CreateDirectory(dir);
					_path = System.IO.Path.Combine(dir, "ff3.log");
					RotatePrevious(dir);
				}
				try
				{
					File.WriteAllText(_path, string.Empty);
				}
				catch (Exception ex)
				{
					_failed = true;
					Console.Error.WriteLine("Log: cannot write " + _path + ": " + ex.Message);
					return;
				}
			}

			AppDomain.CurrentDomain.ProcessExit += delegate { Summary(); };
			AppDomain.CurrentDomain.UnhandledException += delegate(object s, UnhandledExceptionEventArgs e)
			{
				Write(LogChannel.Exception, "UNHANDLED: " + e.ExceptionObject);
				Summary();
			};

			Write(LogChannel.General, "log file: " + _path);
			Write(LogChannel.General, "channels: " + _channels);
			Console.Error.WriteLine("FF3 log: " + _path);

			// Drain a quiet buffer so the tail of the log is never stuck in memory.
			_flushTimer = new System.Threading.Timer(delegate { Flush(); }, null, 500, 500);
		}

		private static void RotatePrevious(string dir)
		{
			try
			{
				string prev = System.IO.Path.Combine(dir, "ff3.prev.log");
				if (File.Exists(_path))
				{
					File.Copy(_path, prev, overwrite: true);
				}
			}
			catch (Exception)
			{
			}
		}

		private static LogChannel ParseChannels(string spec)
		{
			if (string.IsNullOrWhiteSpace(spec))
			{
				return LogChannel.None;
			}
			if (spec.Trim().Equals("all", StringComparison.OrdinalIgnoreCase))
			{
				return LogChannel.All;
			}
			if (spec.Trim().Equals("none", StringComparison.OrdinalIgnoreCase))
			{
				return LogChannel.None;
			}
			LogChannel result = LogChannel.None;
			foreach (string part in spec.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries))
			{
				if (Enum.TryParse(part, ignoreCase: true, out LogChannel one))
				{
					result |= one;
				}
			}
			return result;
		}

		public static bool IsEnabled(LogChannel channel)
		{
			return !_failed && (_channels & channel) != 0;
		}

		public static void Write(LogChannel channel, string message)
		{
			if (!IsEnabled(channel))
			{
				return;
			}
			Append(string.Format("{0,9:F3} {1,-9} {2}", _clock.Elapsed.TotalSeconds, channel, message));
		}

		/// <summary>Logs the first <paramref name="limit"/> hits on <paramref name="key"/>, then stays quiet.</summary>
		public static void First(LogChannel channel, string key, int limit, Func<string> message)
		{
			if (!IsEnabled(channel))
			{
				return;
			}
			int n = Tally(key);
			if (n < limit)
			{
				Append(string.Format("{0,9:F3} {1,-9} [{2}#{3}] {4}",
					_clock.Elapsed.TotalSeconds, channel, key, n, Safe(message)));
			}
		}

		/// <summary>Logs one hit in every <paramref name="every"/> on <paramref name="key"/>.</summary>
		public static void Sample(LogChannel channel, string key, int every, Func<string> message)
		{
			if (!IsEnabled(channel))
			{
				return;
			}
			int n = Tally(key);
			if (every > 0 && n % every == 0)
			{
				Append(string.Format("{0,9:F3} {1,-9} [{2}#{3}] {4}",
					_clock.Elapsed.TotalSeconds, channel, key, n, Safe(message)));
			}
		}

		/// <summary>Counts a call without ever logging it; shows up in the closing summary.</summary>
		public static void Count(LogChannel channel, string key)
		{
			if (IsEnabled(channel))
			{
				Tally(key);
			}
		}

		private static int Tally(string key)
		{
			lock (_sync)
			{
				_counts.TryGetValue(key, out int n);
				_counts[key] = n + 1;
				return n;
			}
		}

		private static string Safe(Func<string> message)
		{
			try
			{
				return message();
			}
			catch (Exception ex)
			{
				return "<log formatter threw " + ex.GetType().Name + ": " + ex.Message + ">";
			}
		}

		private static void Append(string line)
		{
			lock (_sync)
			{
				_pending.AppendLine(line);
				_pendingLines++;
				// Size-based flush only. A quiet buffer is drained by _flushTimer, so the
				// last few lines before a hang or a kill still reach the file.
				if (_pendingLines >= FlushEveryLines)
				{
					FlushLocked();
				}
			}
		}

		public static void Flush()
		{
			lock (_sync)
			{
				FlushLocked();
			}
		}

		private static void FlushLocked()
		{
			if (_failed || _pending.Length == 0)
			{
				return;
			}
			try
			{
				File.AppendAllText(_path, _pending.ToString());
			}
			catch (Exception)
			{
				_failed = true;
			}
			_pending.Clear();
			_pendingLines = 0;
			_lastFlush = _clock.Elapsed;
		}

		/// <summary>Appends the per-key call tallies. Safe to call more than once.</summary>
		public static void Summary()
		{
			lock (_sync)
			{
				if (_failed || _counts.Count == 0)
				{
					FlushLocked();
					return;
				}
				List<KeyValuePair<string, int>> rows = new List<KeyValuePair<string, int>>(_counts);
				rows.Sort((a, b) => b.Value.CompareTo(a.Value));
				_pending.AppendLine();
				_pending.AppendLine("---- call counts at " + _clock.Elapsed.TotalSeconds.ToString("F3") + "s ----");
				foreach (KeyValuePair<string, int> row in rows)
				{
					_pending.AppendLine(string.Format("{0,-28} {1,9}", row.Key, row.Value));
				}
				_counts.Clear();
				FlushLocked();
			}
		}
	}
}
