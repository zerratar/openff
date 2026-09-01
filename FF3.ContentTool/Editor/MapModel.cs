// A map, assembled from the four files that describe it.
//
//   <map>.hich     what stands there: model, position, and the cast that drives it
//   <map>.script   the casts themselves - what each one does when you talk to it
//   <map>.pak      the exits, and the rest of the per map data
//   *.msd          the words
//
// Nothing here is new information; it is the same data the other views show. The
// point is putting it together, because "an NPC" is not a row in any one file - it is
// a hich entry plus a script cast plus the messages that cast shows, and until those
// are joined up the editor can only ever be a table browser.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace FF3.ContentTool.Editor
{
	internal sealed class MapCharacter
	{
		public int Index { get; set; }
		public string Model { get; set; }
		public int Cast { get; set; }
		public int Kind { get; set; }
		public string KindName { get; set; }
		public int X { get; set; }
		public int Y { get; set; }
		public int Z { get; set; }
		public int RotationY { get; set; }

		/// <summary>Whether the map's script has any code for this cast.</summary>
		public bool HasScript { get; set; }

		/// <summary>The lines this character says, in the order the code shows them.</summary>
		public List<string> Lines { get; set; } = new List<string>();

		/// <summary>Instructions in this cast's code, as a rough measure of how much it does.</summary>
		public int Instructions { get; set; }
	}

	internal sealed class MapExit
	{
		public int X { get; set; }
		public int Y { get; set; }
		public int Z { get; set; }
		public string To { get; set; }
		public int ToIndex { get; set; }
		public int ConditionFlag { get; set; }
		public int Kind { get; set; }
	}

	internal static class MapModel
	{
		private const int StartMessage2 = 100;

		/// <summary>Everything the editor needs to draw one map.</summary>
		public static object Load(Workspace workspace, string map,
			Func<uint, string> lookupMessage)
		{
			string hichName = "files/" + map + ".hich";
			List<HichEntry> entries = Hich.Read(workspace.Read(hichName));

			Dictionary<int, (List<string> Lines, int Count)> perCast = ReadCasts(
				workspace, map, lookupMessage);

			List<MapCharacter> characters = new List<MapCharacter>();
			for (int i = 0; i < entries.Count; i++)
			{
				HichEntry entry = entries[i];
				perCast.TryGetValue(entry.Cast, out (List<string> Lines, int Count) code);

				characters.Add(new MapCharacter
				{
					Index = i,
					Model = entry.Model,
					Cast = entry.Cast,
					Kind = entry.Kind,
					KindName = entry.KindName,
					X = entry.Position[0],
					Y = entry.Position[1],
					Z = entry.Position[2],
					RotationY = entry.Posture[1],
					HasScript = code.Count > 0,
					Lines = code.Lines ?? new List<string>(),
					Instructions = code.Count
				});
			}

			return new
			{
				map,
				hich = hichName,
				script = "files/" + map + ".script",
				overridden = workspace.IsOverridden(hichName),
				characters,
				exits = ReadExits(workspace, map)
			};
		}

		/// <summary>Saves moved characters back into the map's .hich.</summary>
		public static int Save(Workspace workspace, string map,
			IEnumerable<(int Index, int X, int Y, int Z, int RotationY)> moves)
		{
			string hichName = "files/" + map + ".hich";
			List<HichEntry> entries = Hich.Read(workspace.Read(hichName));

			int changed = 0;
			foreach ((int index, int x, int y, int z, int rotation) in moves)
			{
				if (index < 0 || index >= entries.Count)
				{
					continue;
				}
				HichEntry entry = entries[index];
				entry.Position[0] = x;
				entry.Position[1] = y;
				entry.Position[2] = z;
				entry.Posture[1] = rotation;
				changed++;
			}

			workspace.Write(hichName, Hich.Write(entries));
			return changed;
		}

		/// <summary>
		/// What each cast in the map's script says and how much code it has.
		///
		/// A cast's code is whatever is reachable from its three entry points, so this
		/// walks from them the same way the disassembler does - following jumps,
		/// stopping where flow stops - and collects the messages on the way.
		/// </summary>
		private static Dictionary<int, (List<string> Lines, int Count)> ReadCasts(
			Workspace workspace, string map, Func<uint, string> lookupMessage)
		{
			Dictionary<int, (List<string>, int)> perCast =
				new Dictionary<int, (List<string>, int)>();

			ScriptFile script;
			try
			{
				script = ScriptFile.Read(workspace.Read("files/" + map + ".script"));
			}
			catch (Exception)
			{
				return perCast;                      // no script, or one we cannot read
			}

			Dictionary<uint, ScriptInstruction> byAddress = ScriptDisassembler
				.Disassemble(script).Code.ToDictionary(i => i.At);

			foreach (ScriptCast cast in script.Casts)
			{
				List<string> lines = new List<string>();
				HashSet<uint> seen = new HashSet<uint>();
				Queue<uint> pending = new Queue<uint>();

				foreach (uint entry in new[] { cast.Constructor, cast.Normal, cast.Destructor })
				{
					if (entry != ScriptFile.NoScript)
					{
						pending.Enqueue(entry);
					}
				}

				while (pending.Count > 0)
				{
					uint pc = pending.Dequeue();
					while (seen.Add(pc) && byAddress.TryGetValue(pc, out ScriptInstruction instruction))
					{
						if (instruction.Opcode == StartMessage2 && lookupMessage != null
							&& instruction.Operands.Count >= 2
							&& instruction.Operands[1] is uint id)
						{
							string text = lookupMessage(id);
							if (text != null && !lines.Contains(text))
							{
								lines.Add(text);
							}
						}

						foreach (uint target in instruction.Targets)
						{
							pending.Enqueue(target);
						}

						string name = instruction.Op?.Name;
						if (name == "endCommand" || name == "jumpCommand"
							|| name == "returnCommand" || instruction.Op == null)
						{
							break;
						}
						pc += Math.Max(instruction.Length, 1);
					}
				}

				perCast[(int)cast.Number] = (lines, seen.Count);
			}

			return perCast;
		}

		/// <summary>The map's exits, from chain 0 of its .pak.</summary>
		private static List<MapExit> ReadExits(Workspace workspace, string map)
		{
			List<MapExit> exits = new List<MapExit>();
			string name = "files/" + map + ".pak";
			if (!workspace.Exists(name))
			{
				return exits;
			}

			try
			{
				byte[] data = workspace.Read(name);
				int chains = data.Length >= 4 ? BitConverter.ToInt32(data, 0) : 0;
				PakFile decoded = Pak.Read(data, Pak.FamilyOf(name, chains));
				PakChainData jumps = decoded.Chains.FirstOrDefault(c => c.Label == "jumps");
				if (jumps?.Records == null)
				{
					return exits;
				}

				foreach (System.Text.Json.Nodes.JsonObject record in jumps.Records)
				{
					int[] position = Numbers(record, "plPos", 3);
					exits.Add(new MapExit
					{
						X = position[0],
						Y = position[1],
						Z = position[2],
						To = Name(record, "nextMapName"),
						ToIndex = Number(record, "nextMapIndex"),
						ConditionFlag = Number(record, "conditionFlag"),
						Kind = Number(record, "kind")
					});
				}
			}
			catch (Exception)
			{
				// A map whose pak will not decode still has characters worth showing.
			}
			return exits;
		}

		private static int Number(System.Text.Json.Nodes.JsonObject record, string field)
		{
			return record[field] is System.Text.Json.Nodes.JsonValue value
				&& int.TryParse(value.ToJsonString(), NumberStyles.Integer,
					CultureInfo.InvariantCulture, out int number) ? number : 0;
		}

		private static int[] Numbers(System.Text.Json.Nodes.JsonObject record, string field,
			int count)
		{
			int[] values = new int[count];
			if (record[field] is System.Text.Json.Nodes.JsonArray array)
			{
				for (int i = 0; i < count && i < array.Count; i++)
				{
					if (int.TryParse(array[i]?.ToJsonString(), NumberStyles.Integer,
							CultureInfo.InvariantCulture, out int number))
					{
						values[i] = number;
					}
				}
			}
			return values;
		}

		/// <summary>A fixed length byte field read back as the name it holds.</summary>
		private static string Name(System.Text.Json.Nodes.JsonObject record, string field)
		{
			if (!(record[field] is System.Text.Json.Nodes.JsonArray array))
			{
				return null;
			}
			System.Text.StringBuilder text = new System.Text.StringBuilder();
			foreach (System.Text.Json.Nodes.JsonNode node in array)
			{
				if (!int.TryParse(node?.ToJsonString(), NumberStyles.Integer,
						CultureInfo.InvariantCulture, out int value) || value == 0)
				{
					break;
				}
				text.Append((char)value);
			}
			return text.ToString();
		}
	}
}
