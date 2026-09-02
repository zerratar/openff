// What points at what, so a thing can be renumbered without breaking the things that
// name it by number.
//
// Most of the game does not need this. Almost everything that looks like a reference is
// by name or by identity rather than by position:
//
//   a .hich row      is found by scanning for its cast number, so rows can be added,
//                    removed and reordered freely - getManCastIndex does a linear search
//   a message        is an id, not an offset into the file
//   a flag           is a (group, index) pair in a fixed array of 3 x 1000
//   an item          is an id
//   a map            is named by string, in nextMapName and in mapWarp
//   every .pak chain but one holds exactly one record in all 337 maps, so there is no
//                    row order in them to disturb
//
// Which leaves exactly one thing in the map data that is addressed by position: the
// jumps chain, whose rows are named by slot number. That is why a general reference
// graph over all 6,962 files would be almost entirely empty, and why this indexes the
// couplings that are real instead.
//
// There are two of them. Exit slots, below, which have to be renumbered when one is
// removed. And cast numbers, which do not - a cast is found by scanning for its number,
// so nothing shifts - but which are still worth being able to look up before deleting a
// character, because what is left behind is a script reaching for somebody who is not
// there. 73 commands take a cast number, and six take a second one as well; that list is
// generated from the game into CastArgs.cs rather than written out by hand.
//
// An exit slot N of map M is named in three places, and all three were counted rather
// than assumed:
//
//   the mesh     M's collision mesh, as a material carrying attribute 24 + N. This is
//                what fires it.
//
//   the script   setMapJumpFlag and setMapJumpFlagJump turn one on or off, and their
//                first argument is a zero based index into the twelve - so slot N is
//                written N - 1. 63 calls across 66 maps.
//
//                Worth being careful here: setMapJump_SE looks exactly the same and is
//                used 59 times, but its first argument selects a sound flag, not a slot.
//                Treating those as slot references would corrupt 59 scripts.
//
//   other maps   any map's jumps row whose nextMapName is M and whose nextMapIndex is N.
//                That is where the player comes out when they arrive.
//
// Building the whole picture reads every script, every map .pak and every collision
// mesh. It is held in memory and thrown away when anything is written, the same as
// CharacterIds and FlagIndex - see BuildMilliseconds for what that costs.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace FF3.ContentTool.Editor
{
	/// <summary>One place that names an exit slot by its number.</summary>
	internal sealed class ExitReference
	{
		/// <summary>"mesh", "script" or "arrival".</summary>
		public string Kind { get; set; }

		/// <summary>The file that holds the reference.</summary>
		public string File { get; set; }

		/// <summary>The map that file belongs to.</summary>
		public string Map { get; set; }

		/// <summary>Which of that map's exits, when the referrer is an arrival.</summary>
		public int FromSlot { get; set; }

		/// <summary>
		/// The shared interior the arriving exit asks for, or -1. A house does not have
		/// scenery of its own; the door that leads into it says which of the five to
		/// draw, so this is the only place that knows what a borrowed map looks like.
		/// </summary>
		public int Interior { get; set; } = -1;

		public string What { get; set; }
	}

	/// <summary>One place in a script that names a cast.</summary>
	internal sealed class CastReference
	{
		/// <summary>"declaration", "boot", "treasure", "code" or "command".</summary>
		public string Kind { get; set; }

		public int Line { get; set; }
		public string Text { get; set; }
		public string What { get; set; }
	}

	internal sealed class References
	{
		/// <summary>setMapJumpFlag and its jumping form. Both name a slot, zero based.</summary>
		private static readonly Regex JumpFlag = new Regex(
			@"\b(setMapJumpFlag|setMapJumpFlagJump)\s*\(\s*(\d+)\s*,",
			RegexOptions.Compiled);

		private readonly Workspace _workspace;
		private readonly Func<uint, string> _lookupMessage;

		/// <summary>Every arrival, as map -> slot -> who arrives there.</summary>
		private Dictionary<string, Dictionary<int, List<ExitReference>>> _arrivals;

		/// <summary>Every script that names a slot of its own map.</summary>
		private Dictionary<string, Dictionary<int, List<ExitReference>>> _scripts;

		public References(Workspace workspace, Func<uint, string> lookupMessage)
		{
			_workspace = workspace;
			_lookupMessage = lookupMessage;
		}

		public void Invalidate()
		{
			_arrivals = null;
			_scripts = null;
		}

		/// <summary>How long the last build took, so the cost is a measurement not a guess.</summary>
		public long BuildMilliseconds { get; private set; }

		public int MapsRead { get; private set; }

		public List<string> Unreadable { get; } = new List<string>();

		/// <summary>Everything that names this slot of this map.</summary>
		public List<ExitReference> ToExit(string map, int slot)
		{
			Build();
			List<ExitReference> found = new List<ExitReference>();

			if (_arrivals.TryGetValue(map, out Dictionary<int, List<ExitReference>> arrivals)
				&& arrivals.TryGetValue(slot, out List<ExitReference> who))
			{
				found.AddRange(who);
			}
			if (_scripts.TryGetValue(map, out Dictionary<int, List<ExitReference>> named)
				&& named.TryGetValue(slot, out List<ExitReference> calls))
			{
				found.AddRange(calls);
			}

			string mesh = MapExits.MeshName(map);
			if (_workspace.Exists(mesh))
			{
				try
				{
					if (Mcl.HasJump(Mcl.Read(Lz.Decompress(_workspace.Read(mesh))), slot))
					{
						found.Add(new ExitReference
						{
							Kind = "mesh",
							File = mesh,
							Map = map,
							What = "the region that fires it, attribute "
								+ Mcl.JumpAttribute(slot)
						});
					}
				}
				catch (Exception)
				{
					// A mesh that will not read cannot be spoken for, and Build has
					// already recorded that it would not.
				}
			}

			return found;
		}

		/// <summary>
		/// Every arrival pointing at a slot of this map that does not exist, which is a
		/// broken link somebody wants to know about.
		/// </summary>
		public List<ExitReference> Dangling(string map, int rows)
		{
			Build();
			if (!_arrivals.TryGetValue(map, out Dictionary<int, List<ExitReference>> arrivals))
			{
				return new List<ExitReference>();
			}
			return arrivals
				.Where(pair => pair.Key < 1 || pair.Key > rows)
				.SelectMany(pair => pair.Value)
				.ToList();
		}

		/// <summary>
		/// The maps whose jumps rows would have to change if this map's slots shifted -
		/// so a renumber knows which files to open.
		/// </summary>
		public List<string> MapsArrivingAt(string map)
		{
			Build();
			return _arrivals.TryGetValue(map, out Dictionary<int, List<ExitReference>> arrivals)
				? arrivals.Values.SelectMany(v => v).Select(r => r.Map).Distinct().ToList()
				: new List<string>();
		}

		/// <summary>
		/// The stage a map is actually drawn in, when it has no scenery of its own.
		///
		/// 21 maps are like this: the houses of a town, which share five interiors
		/// between them. Nothing in the map says which one - the door that leads into it
		/// does, as the number after the # in its destination - so the only way to find
		/// out is to ask what leads here.
		/// </summary>
		public string InteriorOf(string map)
		{
			Build();
			if (!_arrivals.TryGetValue(map, out Dictionary<int, List<ExitReference>> arrivals))
			{
				return null;
			}

			foreach (ExitReference arrival in arrivals.Values.SelectMany(v => v))
			{
				string stage = Mcl.SharedInterior(arrival.Interior);
				if (stage != null) return stage;
			}
			return null;
		}

		/// <summary>
		/// Every map drawn in the same interior. Editing that interior's collision is
		/// editing the doorway of all of them, so this is what makes the warning worth
		/// reading rather than vague.
		/// </summary>
		public List<string> DrawnIn(string interior)
		{
			Build();
			List<string> found = new List<string>();
			if (interior == null) return found;

			foreach (KeyValuePair<string, Dictionary<int, List<ExitReference>>> pair in _arrivals)
			{
				foreach (ExitReference arrival in pair.Value.Values.SelectMany(v => v))
				{
					if (Mcl.SharedInterior(arrival.Interior) == interior
						&& !found.Contains(pair.Key))
					{
						found.Add(pair.Key);
					}
				}
			}
			found.Sort(StringComparer.OrdinalIgnoreCase);
			return found;
		}

		/// <summary>Whether this map's own script names any of its slots.</summary>
		public bool ScriptNamesSlots(string map)
		{
			Build();
			return _scripts.TryGetValue(map, out Dictionary<int, List<ExitReference>> named)
				&& named.Count > 0;
		}

		/// <summary>
		/// Everything in a map's script that names one of its casts.
		///
		/// A cast number is how a script reaches a placed character, and 73 commands take
		/// one - see CastArgs, which is generated from the game rather than written out,
		/// because six of them take a second cast as well and a hand written list would
		/// have missed those.
		///
		/// This is not the same question as "what would deleting it break". A cast is
		/// found by scanning for its number, so nothing depends on where its row sits;
		/// what matters is what would be left pointing at a character that is gone.
		/// </summary>
		public List<CastReference> ToCast(string map, int cast)
		{
			List<CastReference> found = new List<CastReference>();
			string scriptName = "files/" + map + ".script";
			if (!_workspace.Exists(scriptName)) return found;

			string source;
			try
			{
				ScriptFile script = ScriptFile.Read(_workspace.Read(scriptName));
				using StringWriter writer = new StringWriter();
				Ffs.SourceWriter.Write(writer, script, scriptName, _lookupMessage);
				source = writer.ToString();
			}
			catch (Exception)
			{
				return found;
			}

			string number = cast.ToString(CultureInfo.InvariantCulture);
			string[] lines = source.Replace("\r\n", "\n").Split('\n');

			Regex castHead = new Regex(@"^\s*cast\s+" + number + @"\s*[{;]");
			Regex ownLabel = new Regex(@"^\s*cast" + number + @"_(\w+)\s*:");
			Regex call = new Regex(@"\b([A-Za-z_]\w*)\s*\(([^)]*)\)");

			for (int i = 0; i < lines.Length; i++)
			{
				string line = lines[i];

				if (castHead.IsMatch(line))
				{
					found.Add(new CastReference
					{
						Kind = "declaration",
						Line = i + 1,
						Text = line.Trim(),
						What = "the cast block that makes it reachable"
					});
					continue;
				}

				Match label = ownLabel.Match(line);
				if (label.Success)
				{
					found.Add(new CastReference
					{
						Kind = "code",
						Line = i + 1,
						Text = line.Trim(),
						What = "its " + label.Groups[1].Value + " block"
					});
					continue;
				}

				foreach (Match found_ in call.Matches(line))
				{
					string name = found_.Groups[1].Value;
					if (!CastArgs.Positions.TryGetValue(name, out int[] positions)) continue;

					string[] args = found_.Groups[2].Value.Split(',');
					foreach (int at in positions)
					{
						if (at >= args.Length) continue;
						if (args[at].Trim() != number) continue;

						found.Add(new CastReference
						{
							Kind = name.StartsWith("boot", StringComparison.Ordinal)
								? "boot"
								: name.StartsWith("setTreasure", StringComparison.Ordinal)
									? "treasure"
									: "command",
							Line = i + 1,
							Text = line.Trim(),
							What = name + ", argument " + (at + 1)
						});
						break;
					}
				}
			}

			return found;
		}

		/// <summary>
		/// The lines a cast says, found the same way DeleteCharacter finds them - by
		/// walking the code under its own labels.
		/// </summary>
		public List<uint> MessagesOf(string map, int cast)
		{
			List<uint> said = new List<uint>();
			string scriptName = "files/" + map + ".script";
			if (!_workspace.Exists(scriptName)) return said;

			string source;
			try
			{
				ScriptFile script = ScriptFile.Read(_workspace.Read(scriptName));
				using StringWriter writer = new StringWriter();
				Ffs.SourceWriter.Write(writer, script, scriptName, _lookupMessage);
				source = writer.ToString();
			}
			catch (Exception)
			{
				return said;
			}

			Regex ownLabel = new Regex(@"^\s*cast"
				+ cast.ToString(CultureInfo.InvariantCulture) + @"_\w+\s*:");
			Regex anyLabel = new Regex(@"^\s*[A-Za-z_]\w*\s*:");
			Regex message = new Regex(
				@"startMessage2?\s*\(\s*\d+\s*,\s*(0[xX][0-9a-fA-F]+|\d+)");

			bool inside = false;
			foreach (string line in source.Replace("\r\n", "\n").Split('\n'))
			{
				if (ownLabel.IsMatch(line)) inside = true;
				else if (inside && anyLabel.IsMatch(line)) inside = false;
				if (!inside) continue;

				foreach (Match hit in message.Matches(line))
				{
					string digits = hit.Groups[1].Value;
					bool hex = digits.StartsWith("0x", StringComparison.OrdinalIgnoreCase);
					if (uint.TryParse(hex ? digits.Substring(2) : digits,
						hex ? NumberStyles.HexNumber : NumberStyles.Integer,
						CultureInfo.InvariantCulture, out uint id))
					{
						said.Add(id);
					}
				}
			}

			return said.Distinct().ToList();
		}

		private void Build()
		{
			if (_arrivals != null) return;

			Stopwatch clock = Stopwatch.StartNew();
			_arrivals = new Dictionary<string, Dictionary<int, List<ExitReference>>>(
				StringComparer.OrdinalIgnoreCase);
			_scripts = new Dictionary<string, Dictionary<int, List<ExitReference>>>(
				StringComparer.OrdinalIgnoreCase);
			Unreadable.Clear();
			MapsRead = 0;

			foreach (WorkspaceEntry entry in _workspace.List(".hich"))
			{
				string map = Path.GetFileNameWithoutExtension(entry.Name);
				MapsRead++;
				ReadArrivals(map);
				ReadScript(map);
			}

			clock.Stop();
			BuildMilliseconds = clock.ElapsedMilliseconds;
		}

		/// <summary>Where this map's exits lead, recorded against the maps they land on.</summary>
		private void ReadArrivals(string map)
		{
			string name = "files/" + map + ".pak";
			if (!_workspace.Exists(name)) return;

			List<MapExit> exits;
			try
			{
				exits = MapModel.ReadExitsOf(_workspace, map);
			}
			catch (Exception)
			{
				Unreadable.Add(name);
				return;
			}

			for (int i = 0; i < exits.Count; i++)
			{
				MapExit exit = exits[i];
				if (string.IsNullOrEmpty(exit.To)) continue;

				Add(_arrivals, exit.To, exit.ToIndex, new ExitReference
				{
					Kind = "arrival",
					File = name,
					Map = map,
					FromSlot = i + 1,
					Interior = exit.ModelNo,
					What = map + " exit " + (i + 1) + " arrives here"
				});
			}
		}

		/// <summary>Which of its own slots a map's script switches on and off.</summary>
		private void ReadScript(string map)
		{
			string name = "files/" + map + ".script";
			if (!_workspace.Exists(name)) return;

			string source;
			try
			{
				ScriptFile script = ScriptFile.Read(_workspace.Read(name));
				using StringWriter writer = new StringWriter();
				Ffs.SourceWriter.Write(writer, script, name, _lookupMessage);
				source = writer.ToString();
			}
			catch (Exception)
			{
				Unreadable.Add(name);
				return;
			}

			foreach (Match call in JumpFlag.Matches(source))
			{
				if (!int.TryParse(call.Groups[2].Value, NumberStyles.Integer,
					CultureInfo.InvariantCulture, out int zeroBased))
				{
					continue;
				}
				Add(_scripts, map, zeroBased + 1, new ExitReference
				{
					Kind = "script",
					File = name,
					Map = map,
					What = call.Groups[1].Value + "(" + zeroBased + ", ...)"
				});
			}
		}

		private static void Add(Dictionary<string, Dictionary<int, List<ExitReference>>> into,
			string map, int slot, ExitReference reference)
		{
			if (!into.TryGetValue(map, out Dictionary<int, List<ExitReference>> slots))
			{
				slots = new Dictionary<int, List<ExitReference>>();
				into[map] = slots;
			}
			if (!slots.TryGetValue(slot, out List<ExitReference> list))
			{
				list = new List<ExitReference>();
				slots[slot] = list;
			}
			list.Add(reference);
		}
	}
}
