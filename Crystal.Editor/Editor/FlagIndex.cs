// Which flags the game already uses, so a new one can be picked that nothing else touches.
//
// A chest remembers being opened by setting a flag, and that flag is what makes it stay
// open. Two chests sharing a flag is not a small bug: opening one empties the other.
// There is no list of free flags anywhere in the data, so the only honest way to find
// one is to read every script and see what is taken.
//
// The array is flags[3, 1000] - three groups of a thousand - and group 10 is an alias
// for group 2, which FlagManager.get and .set both do on the way in. So a reference to
// (10, n) and one to (2, n) are the same flag and both are counted here.
//
// What the shipped game actually uses, counted rather than assumed:
//
//   group 0    608 indices        story progress, mostly
//   group 1    478 indices        every one of the 423 treasure chests is in here
//   group 2/10   9 indices
//
// So group 1 is where a chest flag belongs, and it has around 520 free.
//
// Reading all 357 scripts takes about a third of a second, and the answer only changes
// when a script does, so it is worked out once and thrown away when something is written.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Crystal.Editor
{
	internal sealed class FlagIndex
	{
		/// <summary>Every command that names a flag as its first two arguments.</summary>
		private static readonly Regex Uses = new Regex(
			@"\b(?:flagOn|flagOff|flagOnEnd|flagOffEnd|waitFlagOn|waitFlagOff|flagOnJump|"
			+ @"flagOffJump|flagOnCall|flagOffCall|flagOnReturn|flagOffReturn|flagReverse|"
			+ @"flag)\s*\(\s*(\d+)\s*,\s*(\d+)",
			RegexOptions.Compiled);

		/// <summary>The two treasure commands, whose third and fourth arguments are one.</summary>
		private static readonly Regex Treasure = new Regex(
			@"\bsetTreasure(?:Item|Money)\s*\(\s*\d+\s*,\s*\d+\s*,\s*(\d+)\s*,\s*(\d+)",
			RegexOptions.Compiled);

		private readonly Workspace _workspace;
		private readonly Func<uint, string> _lookupMessage;
		private Dictionary<int, HashSet<int>> _used;
		private readonly List<string> _unreadable = new List<string>();

		public FlagIndex(Workspace workspace, Func<uint, string> lookupMessage)
		{
			_workspace = workspace;
			_lookupMessage = lookupMessage;
		}

		/// <summary>Forget what was counted - something has been written.</summary>
		public void Invalidate()
		{
			_used = null;
		}

		/// <summary>Every index taken in a group, the alias folded in.</summary>
		public IReadOnlyCollection<int> Used(int group)
		{
			Build();
			return _used.TryGetValue(Real(group), out HashSet<int> found)
				? (IReadOnlyCollection<int>)found
				: Array.Empty<int>();
		}

		/// <summary>
		/// The lowest index in a group that nothing uses. Index 0 is skipped: it is the
		/// one a mistake lands on, and leaving it alone costs nothing when there are
		/// hundreds free.
		/// </summary>
		public int Free(int group)
		{
			Build();
			HashSet<int> taken = _used.TryGetValue(Real(group), out HashSet<int> found)
				? found
				: new HashSet<int>();

			for (int index = 1; index < 1000; index++)
			{
				if (!taken.Contains(index)) return index;
			}
			return -1;
		}

		/// <summary>Group 10 is group 2 - FlagManager rewrites it on the way in.</summary>
		private static int Real(int group)
		{
			return group == 10 ? 2 : group;
		}

		private void Build()
		{
			if (_used != null) return;
			_used = new Dictionary<int, HashSet<int>>();
			_unreadable.Clear();
			Read = 0;

			foreach (WorkspaceEntry entry in _workspace.List(".script"))
			{
				string source;
				try
				{
					ScriptFile script = ScriptFile.Read(_workspace.Read(entry.Name), _workspace.Ops);
					using StringWriter writer = new StringWriter();
					Ffs.SourceWriter.Write(writer, script, entry.Name, _lookupMessage);
					source = writer.ToString();
				}
				catch (Exception)
				{
					// A script that will not read is one this cannot see the flags of.
					// One shipped script is in an older format this does not read
					// (s01_01), so this is not hypothetical - the names are kept and
					// the caller says which, rather than quietly claiming a flag is
					// free when nothing has looked.
					_unreadable.Add(entry.Name);
					continue;
				}

				foreach (Match use in Uses.Matches(source))
				{
					Add(use.Groups[1].Value, use.Groups[2].Value);
				}
				foreach (Match chest in Treasure.Matches(source))
				{
					Add(chest.Groups[1].Value, chest.Groups[2].Value);
				}
				Read++;
			}
		}

		private void Add(string group, string index)
		{
			if (!int.TryParse(group, out int g) || !int.TryParse(index, out int i)) return;
			g = Real(g);
			if (g < 0 || g > 2 || i < 0 || i > 999) return;
			if (!_used.TryGetValue(g, out HashSet<int> set))
			{
				set = new HashSet<int>();
				_used[g] = set;
			}
			set.Add(i);
		}

		/// <summary>How many scripts were read.</summary>
		public int Read { get; private set; }

		/// <summary>The ones that would not, whose flags are therefore unknown.</summary>
		public IReadOnlyList<string> UnreadableNames
		{
			get
			{
				Build();
				return _unreadable;
			}
		}

		public int Unreadable => UnreadableNames.Count;
	}
}
