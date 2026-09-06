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

namespace Crystal.Editor
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

		/// <summary>
		/// The message id each line came from, in the same order. Without these a line
		/// can be read in the panel but not found in the text file it lives in.
		/// </summary>
		public List<uint> LineIds { get; set; } = new List<uint>();

		/// <summary>Instructions in this cast's code, as a rough measure of how much it does.</summary>
		public int Instructions { get; set; }
	}

	/// <summary>One row's worth of changes, on the way back to the .hich.</summary>
	internal sealed class MapEdit
	{
		public int Index { get; set; }
		public int X { get; set; }
		public int Y { get; set; }
		public int Z { get; set; }
		public int RotationY { get; set; }

		/// <summary>Left null when the model is not being changed.</summary>
		public string Model { get; set; }

		/// <summary>Left null when the cast is not being changed.</summary>
		public int? Cast { get; set; }
	}

	internal sealed class MapExit
	{
		public int X { get; set; }
		public int Y { get; set; }
		public int Z { get; set; }

		/// <summary>
		/// Which way you are facing when you arrive, in degrees. The file keeps it as a
		/// 16 bit angle where a whole turn is 65536, not the degrees a .hich row uses -
		/// two formats for the same idea, in the same map.
		/// </summary>
		public int RotationY { get; set; }
		/// <summary>The map it leads to, without the model number.</summary>
		public string To { get; set; }

		/// <summary>
		/// The common model the destination loads, or -1 for none.
		///
		/// The field in the file is one string, and the game splits it on a '#':
		/// NextMapName is everything before, ModelNo is atoi of everything after. 55 of
		/// the game's exits use it - "t23_02#04" is Gysahl's inn - and reading the whole
		/// string as a map name makes all 55 look like they lead nowhere.
		/// </summary>
		public int ModelNo { get; set; } = -1;

		public int ToIndex { get; set; }
		public int ConditionFlag { get; set; }
		public int Kind { get; set; }

		/// <summary>
		/// Where the region that fires this exit is, as centre x, floor y, centre z and
		/// then width, height and depth - or null when the mesh has no region for this
		/// slot. The arrival position above is the other half and is somewhere else: the
		/// region is the doorway you walk into, the position is where you come out.
		/// </summary>
		public int[] Region { get; set; }
	}

	/// <summary>
	/// One map, joined up. A named type rather than an anonymous one because two views
	/// read it now - the plan and the scene - and an anonymous type can only be passed
	/// between them as `dynamic`, which moves the mistakes to run time.
	/// </summary>
	internal sealed class MapData
	{
		public string Map { get; set; }
		public string Hich { get; set; }
		public string Script { get; set; }
		public bool Overridden { get; set; }
		public List<MapCharacter> Characters { get; set; } = new List<MapCharacter>();
		public List<MapExit> Exits { get; set; } = new List<MapExit>();
	}

	internal static class MapModel
	{
		private const int StartMessage2 = 100;

		/// <summary>Everything the editor needs to draw one map.</summary>
		public static MapData Load(Workspace workspace, string map,
			Func<uint, string> lookupMessage)
		{
			string hichName = "files/" + map + ".hich";
			List<HichEntry> entries = Hich.Read(workspace.Read(hichName));

			Dictionary<int, (List<string> Lines, List<uint> Ids, int Count)> perCast =
				ReadCasts(workspace, map, lookupMessage);

			List<MapCharacter> characters = new List<MapCharacter>();
			for (int i = 0; i < entries.Count; i++)
			{
				HichEntry entry = entries[i];
				perCast.TryGetValue(entry.Cast,
					out (List<string> Lines, List<uint> Ids, int Count) code);

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
					LineIds = code.Ids ?? new List<uint>(),
					Instructions = code.Count
				});
			}

			return new MapData
			{
				Map = map,
				Hich = hichName,
				Script = "files/" + map + ".script",
				Overridden = workspace.IsOverridden(hichName),
				Characters = characters,
				Exits = ReadExits(workspace, map)
			};
		}

		/// <summary>
		/// Saves changed rows back into the map's .hich - where they stand, which way
		/// they face, which model they wear and which cast drives them.
		///
		/// Changing the model means changing its id as well: the name is what you read
		/// and the number is what the game loads, and a row with the two disagreeing
		/// loads the wrong thing without complaining.
		/// </summary>
		public static int Save(Workspace workspace, string map,
			IEnumerable<MapEdit> edits, CharacterIds ids)
		{
			string hichName = "files/" + map + ".hich";
			List<HichEntry> entries = Hich.Read(workspace.Read(hichName));

			int changed = 0;
			foreach (MapEdit edit in edits)
			{
				if (edit.Index < 0 || edit.Index >= entries.Count)
				{
					continue;
				}

				HichEntry entry = entries[edit.Index];
				entry.Position[0] = edit.X;
				entry.Position[1] = edit.Y;
				entry.Position[2] = edit.Z;
				entry.Posture[1] = edit.RotationY;

				if (edit.Cast.HasValue)
				{
					entry.Cast = edit.Cast.Value;
				}

				if (!string.IsNullOrWhiteSpace(edit.Model)
					&& !string.Equals(edit.Model, entry.Model, StringComparison.Ordinal))
				{
					uint? id = ids?.For(edit.Model);
					if (id != null)
					{
						entry.Model = edit.Model;
						entry.ModelRaw = null;
						entry.CharacterId = id.Value;
					}
				}

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
		private static Dictionary<int, (List<string> Lines, List<uint> Ids, int Count)>
			ReadCasts(Workspace workspace, string map, Func<uint, string> lookupMessage)
		{
			Dictionary<int, (List<string>, List<uint>, int)> perCast =
				new Dictionary<int, (List<string>, List<uint>, int)>();

			ScriptFile script;
			try
			{
				script = ScriptFile.Read(workspace.Read("files/" + map + ".script"), workspace.Ops);
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
				List<uint> ids = new List<uint>();
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
								ids.Add(id);
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

				perCast[(int)cast.Number] = (lines, ids, seen.Count);
			}

			return perCast;
		}

		/// <summary>
		/// Exits declared in the script, FF4's way. Each is
		/// setInsideMapJump(trigger, map, ax, ay, az, facing, x1, y1, z1, x2, y2, z2):
		/// positions in FX32, facing in eighths of a turn.
		/// </summary>
		private static List<MapExit> ReadScriptedExits(Workspace workspace, string map)
		{
			List<MapExit> exits = new List<MapExit>();
			string scriptName = "files/" + map + ".script";
			if (!workspace.Exists(scriptName))
			{
				return exits;
			}
			ScriptFile script;
			try
			{
				script = ScriptFile.Read(workspace.Read(scriptName), workspace.Ops);
			}
			catch (Exception)
			{
				return exits;
			}

			(List<ScriptInstruction> code, _) = ScriptDisassembler.Disassemble(script);
			foreach (ScriptInstruction instruction in code)
			{
				if (instruction.Opcode != ScriptOpsFf4.SetInsideMapJump
					|| instruction.Operands.Count < 12)
				{
					continue;
				}
				string to = instruction.Operands[1] as string ?? string.Empty;
				int Fx(int i) => (int)Math.Round(unchecked((int)(uint)instruction.Operands[i]) / 4096.0);
				exits.Add(new MapExit
				{
					// Where the door is on this map: the trigger box's centre.
					X = (Fx(6) + Fx(9)) / 2,
					Y = Math.Min(Fx(7), Fx(10)),
					Z = (Fx(8) + Fx(11)) / 2,
					RotationY = (int)((uint)instruction.Operands[5] * 45) % 360,
					To = to,
					ModelNo = -1,
					ToIndex = 0,
					ConditionFlag = 1,
					Kind = -1
				});
			}
			return exits;
		}

		/// <summary>The map's exits, from chain 0 of its .pak.</summary>
		/// <summary>This map's exits, for anything that needs them without the rest.</summary>
		public static List<MapExit> ReadExitsOf(Workspace workspace, string map)
		{
			return ReadExits(workspace, map);
		}

		private static List<MapExit> ReadExits(Workspace workspace, string map)
		{
			List<MapExit> exits = new List<MapExit>();

			// FF4 has no jumps chain. Its exits are declared by the map's own script,
			// one setInsideMapJump per door: the trigger object, the destination, the
			// arrival position and facing, and the door's box on this map. The box's
			// centre is where the exit is, for anything drawing it here.
			if (workspace.Game == "ff4")
			{
				return ReadScriptedExits(workspace, map);
			}

			string name = "files/" + map + ".pak";
			if (!workspace.Exists(name))
			{
				return exits;
			}

			try
			{
				byte[] data = workspace.Read(name);
				int chains = data.Length >= 4 ? BitConverter.ToInt32(data, 0) : 0;
				PakFile decoded = Pak.Read(data, Pak.FamilyOf(name, chains, workspace.Game));
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
						RotationY = (int)Math.Round(
							Number(record, "plRot") * 360.0 / 65536.0) % 360,
						To = Destination(record, out int modelNo),
						ModelNo = modelNo,
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

		/// <summary>
		/// The destination, split the way CMapJumpParameter splits it.
		/// </summary>
		private static string Destination(System.Text.Json.Nodes.JsonObject record,
			out int modelNo)
		{
			modelNo = -1;
			string raw = Name(record, "nextMapName");
			if (string.IsNullOrEmpty(raw)) return raw;

			int hash = raw.IndexOf('#');
			if (hash < 0) return raw;

			if (int.TryParse(raw.Substring(hash + 1), NumberStyles.Integer,
				CultureInfo.InvariantCulture, out int number))
			{
				modelNo = number;
			}
			return raw.Substring(0, hash);
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
