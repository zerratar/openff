// The client starting itself again, straight to the title.
//
// What a mod brings is taken in once, as the client starts: its files into the content chain, its
// code into the engine, its items, characters, menus, title entries and models into a dozen caches
// on either side, some of which cannot be taken out again. So a changed mod set (the mod list's
// toggles) is applied the one way that is right whatever a mod does: this process starts a new one
// with the same command line and ends, and the new one reads mods/loadorder.json afresh.
//
// The one-shot start options (a map, a cutscene, a test party) are left off, so the new process
// comes up on the title the old one was showing (--start=title skips the logos). A run under a
// drive or a parity trace is not restarted: something is waiting on this process, and the change
// applies at its next start.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace OpenFF.Client
{
	internal static class Restart
	{
		/// <summary>The options that say where a run starts, or what it starts with; the restart leaves them off.</summary>
		private static readonly HashSet<string> OneShot = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			"start", "map", "cutscene", "load", "party", "gil", "set-ability", "equip"
		};

		/// <summary>Whether this run can restart itself: not under a drive or a trace, which something is waiting on.</summary>
		public static bool Possible => string.IsNullOrEmpty(Options.Get("drive")) && string.IsNullOrEmpty(Options.Get("trace"));

		/// <summary>
		/// Starts a new process of this client on the title, with this one's command line less the
		/// one-shot options. True when it started; the caller then ends this one (Game.Exit).
		/// </summary>
		public static bool ToTitle(string reason)
		{
			if (!Possible)
			{
				Log.Write(LogChannel.General, "restart: not under --drive or --trace (" + reason + "); applies at the next start");
				return false;
			}
			try
			{
				string executable = Environment.ProcessPath;
				string[] args = Environment.GetCommandLineArgs();
				ProcessStartInfo start = new ProcessStartInfo(executable)
				{
					UseShellExecute = false,
					// Relative --content, --mod and --project paths were resolved against where this run started.
					WorkingDirectory = Options.LaunchDirectory
				};
				// Under "dotnet OpenFF.dll" the first argument is the dll; under OpenFF.exe it is the program itself.
				if (string.Equals(Path.GetFileNameWithoutExtension(executable), "dotnet", StringComparison.OrdinalIgnoreCase) && args.Length > 0)
				{
					start.ArgumentList.Add(args[0]);
				}
				foreach (string arg in Kept(args.Skip(1).ToArray()))
				{
					start.ArgumentList.Add(arg);
				}
				start.ArgumentList.Add("--start=title");
				// The environment's versions of the one-shot options are left off too.
				foreach (string name in OneShot)
				{
					start.Environment.Remove("FF3_" + name.ToUpperInvariant().Replace('-', '_'));
				}
				Process.Start(start);
				Log.Write(LogChannel.General, "restart: " + reason + " - started " + executable + " " + string.Join(" ", start.ArgumentList));
				return true;
			}
			catch (Exception ex)
			{
				Log.Write(LogChannel.General, "restart: could not start a new process (" + ex.Message + "); applies at the next start");
				return false;
			}
		}

		/// <summary>The arguments less the one-shot options, in either form (--name=value, or --name value).</summary>
		private static IEnumerable<string> Kept(string[] args)
		{
			for (int i = 0; i < args.Length; i++)
			{
				string arg = args[i];
				if (arg.Length > 0 && arg[0] == '-')
				{
					string body = arg.TrimStart('-');
					int equals = body.IndexOf('=');
					string name = equals >= 0 ? body.Substring(0, equals) : body;
					if (OneShot.Contains(name))
					{
						// "--map d01_05": its value is the next token unless that is another option.
						if (equals < 0 && i + 1 < args.Length && args[i + 1].Length > 0 && args[i + 1][0] != '-')
						{
							i++;
						}
						continue;
					}
				}
				yield return arg;
			}
		}
	}
}
