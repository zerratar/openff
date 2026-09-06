// Every file the game asked for and did not get, once each, on the File channel.
//
// With a second game's install in front, the FF3 logic asks for tables that game never
// had. Rather than find each by its crash, the log lists them: `missing: <name>`.

using System;
using System.Collections.Generic;

namespace FF3
{
	internal static class MissingFiles
	{
		private static readonly HashSet<string> _seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		public static void Report(string filename)
		{
			if (string.IsNullOrEmpty(filename))
			{
				return;
			}
			lock (_seen)
			{
				if (!_seen.Add(filename))
				{
					return;
				}
			}
			Log.Write(LogChannel.File, "missing: " + filename);
		}

		/// <summary>The names reported so far, for a summary.</summary>
		public static string[] Names
		{
			get
			{
				lock (_seen)
				{
					string[] names = new string[_seen.Count];
					_seen.CopyTo(names);
					Array.Sort(names, StringComparer.OrdinalIgnoreCase);
					return names;
				}
			}
		}
	}
}
