// Exits: making them, changing them, and taking them away.
//
// An exit is two halves in two files, and both have to be there:
//
//   the trigger    a region of the map's collision mesh carrying a jump attribute.
//                  calculateJumpCollision walks attributes 0 to 11 and returns the
//                  first one the player is standing in, plus one - so a map has twelve
//                  exit slots and no more, and the shipped maps bear that out: the most
//                  any of them has is eleven rows.
//
//   the row        jumps in the map's .pak: where the player arrives, on which map,
//                  facing which way, and the flag the exit needs.
//
// Slot N's trigger fires row N, and nothing else connects them - which is why the two
// are written together here. A row with no trigger never fires, and a trigger with no
// row indexes past the end of the table. The shipped game has 37 of the first and 7 of
// the second, so neither is fatal, but neither is worth making on purpose.
//
// That pairing is also what makes removing one from the middle a cascade rather than a
// deletion: every slot above it shifts down, and three kinds of thing name a slot by
// number - the mesh attribute, this map's setMapJumpFlag calls, and the nextMapIndex of
// every exit on any map that arrives here. References.cs is what knows where they are,
// and all of them move together or none of them do.
//
// A script can turn an exit off and on - setMapJumpFlag names one of the same twelve
// slots - but that is switching a trigger that already exists.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace FF3.ContentTool.Editor
{
	internal sealed class MapExitEdit
	{
		public int Index { get; set; }
		public int X { get; set; }
		public int Y { get; set; }
		public int Z { get; set; }

		/// <summary>Degrees, as the panel shows them.</summary>
		public int RotationY { get; set; }
		public string To { get; set; }

		/// <summary>The common model the destination loads, written after a '#'.</summary>
		public int ModelNo { get; set; } = -1;
		public int ToIndex { get; set; }
		public int ConditionFlag { get; set; }
		public int Kind { get; set; }
	}

	internal sealed class MapExitResult
	{
		public bool Ok { get; set; }
		public string Error { get; set; }
		public string Wrote { get; set; }

		/// <summary>The other file, when a change touched both halves of an exit.</summary>
		public string Also { get; set; }

		public int Slot { get; set; }
		public List<string> Notes { get; set; } = new List<string>();
	}

	/// <summary>What a map's exits look like from both sides.</summary>
	internal sealed class MapExitState
	{
		/// <summary>Rows in the .pak - where each exit leads.</summary>
		public int Rows { get; set; }

		/// <summary>Slots the collision mesh has a trigger for.</summary>
		public List<int> Triggers { get; set; } = new List<int>();

		public int Slots { get; set; }
		public bool HasTable { get; set; }
		public bool HasMesh { get; set; }
		public string Note { get; set; }

		/// <summary>Whether another exit can be added at all.</summary>
		public bool CanAdd => HasTable && HasMesh && Rows < Slots;
	}

	internal static class MapExits
	{
		/// <summary>How many jump attributes the collision test walks.</summary>
		public const int SlotLimit = 12;

		/// <summary>The name field's width in the record, in bytes.</summary>
		private const int NameLength = 16;

		public static MapExitResult Save(Workspace workspace, string map, MapExitEdit edit)
		{
			MapExitResult result = new MapExitResult();
			string name = "files/" + map + ".pak";

			if (!workspace.Exists(name))
			{
				result.Error = map + " has no .pak, so it has no exits to change";
				return result;
			}

			byte[] data = workspace.Read(name);
			int chains = data.Length >= 4 ? BitConverter.ToInt32(data, 0) : 0;
			PakFile decoded;
			try
			{
				decoded = Pak.Read(data, Pak.FamilyOf(name, chains));
			}
			catch (Exception problem)
			{
				result.Error = "the map's .pak would not decode: " + problem.Message;
				return result;
			}

			PakChainData jumps = decoded.Chains.FirstOrDefault(c => c.Label == "jumps");
			if (jumps?.Records == null)
			{
				result.Error = map + " has no jumps chain";
				return result;
			}

			if (edit.Index < 0 || edit.Index >= jumps.Records.Count)
			{
				result.Error = map + " has " + jumps.Records.Count + " exit"
					+ (jumps.Records.Count == 1 ? string.Empty : "s")
					+ ", so there is no exit " + edit.Index;
				return result;
			}

			JsonObject record = jumps.Records[edit.Index];
			string trouble = Fill(workspace, record, edit, result);
			if (trouble != null)
			{
				result.Error = trouble;
				return result;
			}

			byte[] written = Pak.Write(decoded);
			workspace.Write(name, written);
			result.Wrote = name;
			result.Slot = edit.Index + 1;
			result.Ok = true;
			return result;
		}

		/// <summary>
		/// Puts one edit into one row. Returns why not, or null when it went in.
		/// </summary>
		private static string Fill(Workspace workspace, JsonObject record,
			MapExitEdit edit, MapExitResult result)
		{
			SetNumbers(record, "plPos", new[] { edit.X, edit.Y, edit.Z });

			// The file keeps the facing as a 16 bit angle where a whole turn is 65536,
			// not the degrees a .hich row uses - two formats for the same idea, in the
			// same map.
			int raw = (int)Math.Round(((edit.RotationY % 360) + 360) % 360 * 65536.0 / 360.0);
			SetNumber(record, "plRot", raw % 65536);

			if (edit.To != null)
			{
				string wanted = edit.To.Trim();
				if (edit.ModelNo >= 0)
				{
					// Two digits, zero padded. atoi does not care, but every one of the
					// 42 the game ships is written that way and byte-for-byte is the
					// bar everything else here is held to.
					wanted += "#" + edit.ModelNo.ToString("00", CultureInfo.InvariantCulture);
				}
				if (wanted.Length >= NameLength)
				{
					return "the destination has " + NameLength
						+ " bytes to fit in, with room for the zero that ends it, and "
						+ wanted + " is " + wanted.Length;
				}

				string trouble = Unknown(workspace, edit.To.Trim());
				if (trouble != null) result.Notes.Add(trouble);
				SetName(record, "nextMapName", wanted);
			}

			SetNumber(record, "nextMapIndex", edit.ToIndex);
			SetNumber(record, "conditionFlag", edit.ConditionFlag);
			SetNumber(record, "kind", edit.Kind);
			return null;
		}

		/// <summary>
		/// The three names that are not places but instructions, resolved when the
		/// player goes through: go back to the world map, the town, or the field you
		/// left the Invincible on.
		/// </summary>
		private static readonly string[] Sentinels =
			{ "back_field_map", "back_town_map", "back_from_inv" };

		/// <summary>
		/// Why a destination looks wrong, or null when it is fine.
		///
		/// Not every destination is a map with a .hich. Of the game's 666 exits, 542 are;
		/// 122 lead to a world tile, which is a .flsc.lz and is spelled in a different
		/// case in the two places it appears; and two are sentinels. Warning about any of
		/// those would be crying wolf on a fifth of the game.
		/// </summary>
		public static string Unknown(Workspace workspace, string name)
		{
			if (string.IsNullOrEmpty(name)) return null;
			if (Sentinels.Contains(name, StringComparer.OrdinalIgnoreCase)) return null;
			if (workspace.Exists("files/" + name + ".hich")) return null;

			// The world tiles are lower case on disk and upper case in the exits.
			if (workspace.List(".lz").Any(entry => string.Equals(entry.Name,
				"files/" + name + ".flsc.lz", StringComparison.OrdinalIgnoreCase)))
			{
				return null;
			}

			return "nothing in the content is called " + name
				+ " - not a map, not a world tile, and not one of the three names the "
				+ "game resolves at run time";
		}

		/// <summary>Where a map keeps the half of its exits that is geometry.</summary>
		public static string MeshName(string map)
		{
			return "files/" + map + "_col.mcl.lz";
		}

		/// <summary>
		/// What this map's exits look like from both sides: how many rows the .pak has,
		/// which slots the mesh has a trigger for, and therefore what can be added.
		/// </summary>
		public static MapExitState State(Workspace workspace, string map)
		{
			MapExitState state = new MapExitState { Slots = SlotLimit };

			string pak = "files/" + map + ".pak";
			if (workspace.Exists(pak))
			{
				try
				{
					PakChainData jumps = Jumps(workspace, pak, out _);
					state.Rows = jumps?.Records?.Count ?? 0;
					state.HasTable = jumps != null;
				}
				catch (Exception)
				{
					state.HasTable = false;
				}
			}

			string mesh = MeshName(map);
			state.HasMesh = workspace.Exists(mesh);
			if (state.HasMesh)
			{
				try
				{
					state.Triggers = Mcl.JumpSlotsUsed(Mcl.Read(Lz.Decompress(
						workspace.Read(mesh))));
				}
				catch (Exception problem)
				{
					state.HasMesh = false;
					state.Note = "the collision mesh would not read: " + problem.Message;
				}
			}

			return state;
		}

		/// <summary>
		/// Adds a whole exit: the row that says where it leads, and the region in the
		/// collision mesh that makes it fire. Both, or neither - half an exit is either
		/// a row nothing triggers or a trigger with nowhere to go.
		/// </summary>
		public static MapExitResult Add(Workspace workspace, string map, MapExitEdit edit,
			int width, int height, int depth)
		{
			MapExitResult result = new MapExitResult();

			string pakName = "files/" + map + ".pak";
			string meshName = MeshName(map);

			if (!workspace.Exists(pakName))
			{
				result.Error = map + " has no .pak, so there is nowhere to say where an "
					+ "exit leads";
				return result;
			}
			if (!workspace.Exists(meshName))
			{
				result.Error = map + " has no collision mesh, so there is nothing to put "
					+ "a trigger in. 23 maps are like this - they borrow another map's "
					+ "collision - and an exit cannot be added to one of them here";
				return result;
			}

			PakFile decoded;
			PakChainData jumps;
			try
			{
				jumps = Jumps(workspace, pakName, out decoded);
			}
			catch (Exception problem)
			{
				result.Error = "the map's .pak would not decode: " + problem.Message;
				return result;
			}
			if (jumps?.Records == null)
			{
				result.Error = map + " has no jumps chain";
				return result;
			}

			if (jumps.Records.Count >= SlotLimit)
			{
				result.Error = map + " already has " + jumps.Records.Count
					+ " exits, and the collision test only walks " + SlotLimit
					+ " attributes, so a thirteenth could never fire";
				return result;
			}

			MclFile mesh;
			try
			{
				mesh = Mcl.Read(Lz.Decompress(workspace.Read(meshName)));
			}
			catch (Exception problem)
			{
				result.Error = "the collision mesh would not read: " + problem.Message;
				return result;
			}

			int slot = jumps.Records.Count + 1;
			if (Mcl.HasJump(mesh, slot))
			{
				result.Error = "the collision mesh already has a trigger for exit " + slot
					+ ", which is the slot a new row would take. That is one of the "
					+ "handful of maps whose mesh and table disagree";
				return result;
			}

			try
			{
				Mcl.AddJumpRegion(mesh, slot, edit.X, edit.Y, edit.Z, width, height, depth);
			}
			catch (Exception problem)
			{
				result.Error = problem.Message;
				return result;
			}

			// Both halves are worked out before either is written.
			JsonObject row = NewRow();
			jumps.Records.Add(row);
			edit.Index = slot - 1;
			string trouble = Fill(workspace, row, edit, result);
			if (trouble != null)
			{
				result.Error = trouble;
				return result;
			}

			workspace.Write(pakName, Pak.Write(decoded));
			workspace.Write(meshName, Lz.Compress(Mcl.Build(mesh)));
			result.Wrote = pakName;
			result.Also = meshName;
			result.Slot = slot;
			result.Ok = true;

			result.Notes.Add("exit " + slot + " leads to "
				+ (string.IsNullOrEmpty(edit.To) ? "(nowhere)" : edit.To));
			result.Notes.Add("its trigger is a " + width + " by " + depth + " box "
				+ height + " tall at " + edit.X + ", " + edit.Z
				+ ", the size the game's own exits are");
			return result;
		}

		/// <summary>
		/// Takes an exit away, wherever it sits, and renumbers everything that named a
		/// slot after it.
		///
		/// Removing the last one touches nothing else. Removing one from the middle
		/// shifts every slot above it down by one, and three kinds of thing name a slot
		/// by number - see Editor/References.cs. All three move together here, or none
		/// of them do:
		///
		///   the mesh     each region's attribute, 24 + slot
		///   the script   setMapJumpFlag and setMapJumpFlagJump, whose argument is the
		///                slot minus one
		///   other maps   the nextMapIndex of every jumps row that arrives here
		///
		/// Anything that pointed at the exit being removed cannot be renumbered, because
		/// there is nothing left to point at. Those are reported rather than repointed at
		/// a neighbour, which would send the player somewhere nobody chose.
		/// </summary>
		public static MapExitResult Remove(Workspace workspace, string map, int slot,
			References references, Func<uint, string> lookupMessage)
		{
			MapExitResult result = new MapExitResult();
			string pakName = "files/" + map + ".pak";
			string meshName = MeshName(map);

			PakFile decoded;
			PakChainData jumps;
			try
			{
				jumps = Jumps(workspace, pakName, out decoded);
			}
			catch (Exception problem)
			{
				result.Error = "the map's .pak would not decode: " + problem.Message;
				return result;
			}
			if (jumps?.Records == null || slot < 1 || slot > jumps.Records.Count)
			{
				result.Error = map + " has no exit " + slot;
				return result;
			}

			int rows = jumps.Records.Count;
			int shifting = rows - slot;

			// Everything that named this exit, before it stops existing.
			List<ExitReference> orphaned = references?.ToExit(map, slot)
				?? new List<ExitReference>();

			// ---- the mesh, both its own region and the ones above it
			MclFile mesh = null;
			bool hadRegion = false;
			if (workspace.Exists(meshName))
			{
				try
				{
					mesh = Mcl.Read(Lz.Decompress(workspace.Read(meshName)));
				}
				catch (Exception problem)
				{
					result.Error = "nothing was written, because the collision mesh would "
						+ "not read and removing one half of an exit is worse than "
						+ "removing neither: " + problem.Message;
					return result;
				}

				hadRegion = Mcl.RemoveJumpRegion(mesh, slot);
				for (int above = slot + 1; above <= SlotLimit; above++)
				{
					if (Mcl.MoveJumpRegion(mesh, above, above - 1))
					{
						result.Notes.Add("moved the region for exit " + above + " down to "
							+ (above - 1));
					}
				}
			}

			// ---- this map's own script
			int scriptCalls = 0;
			byte[] compiled = null;
			string scriptName = "files/" + map + ".script";
			if (shifting > 0 && workspace.Exists(scriptName))
			{
				string edited = Reslot(workspace, scriptName, slot, lookupMessage,
					out scriptCalls, out string scriptError);
				if (scriptError != null)
				{
					result.Error = "nothing was written: " + scriptError;
					return result;
				}
				if (scriptCalls > 0)
				{
					try
					{
						compiled = Ffs.Compiler.Compile(Ffs.Parser.Parse(edited));
					}
					catch (Exception problem)
					{
						result.Error = "nothing was written, because the script would not "
							+ "compile after renumbering: " + problem.Message;
						return result;
					}
				}
			}

			// ---- every other map that arrives above this slot
			List<KeyValuePair<string, PakFile>> arrivals =
				new List<KeyValuePair<string, PakFile>>();
			int arrivalRows = 0;
			if (shifting > 0 && references != null)
			{
				foreach (string other in references.MapsArrivingAt(map))
				{
					string otherPak = "files/" + other + ".pak";
					PakChainData otherJumps;
					try
					{
						otherJumps = Jumps(workspace, otherPak, out PakFile otherFile);
						if (otherJumps?.Records == null) continue;

						bool touched = false;
						foreach (JsonObject row in otherJumps.Records)
						{
							if (!string.Equals(NameOf(row, "nextMapName"), map,
								StringComparison.OrdinalIgnoreCase))
							{
								continue;
							}
							int index = NumberOf(row, "nextMapIndex");
							if (index <= slot) continue;
							SetNumber(row, "nextMapIndex", index - 1);
							touched = true;
							arrivalRows++;
						}
						if (touched)
						{
							arrivals.Add(new KeyValuePair<string, PakFile>(otherPak, otherFile));
						}
					}
					catch (Exception problem)
					{
						result.Error = "nothing was written, because " + otherPak
							+ " arrives here and would not decode: " + problem.Message;
						return result;
					}
				}
			}

			// ---- past every check, so all of it goes out together
			jumps.Records.RemoveAt(slot - 1);
			workspace.Write(pakName, Pak.Write(decoded));
			result.Wrote = pakName;
			result.Slot = slot;

			if (mesh != null && (hadRegion || shifting > 0))
			{
				workspace.Write(meshName, Lz.Compress(Mcl.Build(mesh)));
				result.Also = meshName;
			}
			if (compiled != null)
			{
				workspace.Write(scriptName, compiled);
				result.Notes.Add("renumbered " + scriptCalls + " setMapJumpFlag call"
					+ (scriptCalls == 1 ? string.Empty : "s") + " in " + map + "'s script");
			}
			foreach (KeyValuePair<string, PakFile> pair in arrivals)
			{
				workspace.Write(pair.Key, Pak.Write(pair.Value));
			}

			result.Ok = true;
			result.Notes.Insert(0, hadRegion
				? "removed exit " + slot + " and the region that triggered it"
				: "removed exit " + slot + " - it had no trigger in the mesh");
			if (shifting > 0)
			{
				result.Notes.Add("exits " + (slot + 1) + " to " + rows
					+ " moved down a slot");
			}
			if (arrivalRows > 0)
			{
				result.Notes.Add("repointed " + arrivalRows + " arrival"
					+ (arrivalRows == 1 ? string.Empty : "s") + " on "
					+ arrivals.Count + " other map"
					+ (arrivals.Count == 1 ? string.Empty : "s"));
			}

			// What named the exit itself, which nothing can renumber.
			foreach (ExitReference gone in orphaned.Where(r => r.Kind != "mesh"))
			{
				result.Notes.Add(gone.Kind == "arrival"
					? "left alone: " + gone.What + ", and now arrives at an exit that is "
						+ "gone. Point it somewhere else"
					: "left alone: " + gone.What + " in " + gone.File
						+ " switched the exit that has just been removed");
			}

			return result;
		}

		/// <summary>
		/// Rewrites this map's setMapJumpFlag calls for a slot going away. Their argument
		/// is the slot minus one, so a call naming a slot above the one removed loses one.
		/// </summary>
		private static string Reslot(Workspace workspace, string scriptName, int removed,
			Func<uint, string> lookupMessage, out int changed, out string error)
		{
			changed = 0;
			error = null;

			string source;
			try
			{
				ScriptFile script = ScriptFile.Read(workspace.Read(scriptName));
				using StringWriter writer = new StringWriter();
				Ffs.SourceWriter.Write(writer, script, scriptName, lookupMessage);
				source = writer.ToString();
			}
			catch (Exception problem)
			{
				error = scriptName + " would not read: " + problem.Message;
				return null;
			}

			int count = 0;
			// setMapJump_SE looks the same and is not a slot, so the names are spelled out.
			string edited = Regex.Replace(source,
				@"\b(setMapJumpFlag|setMapJumpFlagJump)\s*\(\s*(\d+)\s*,",
				match =>
				{
					int zeroBased = int.Parse(match.Groups[2].Value,
						CultureInfo.InvariantCulture);
					if (zeroBased + 1 <= removed) return match.Value;
					count++;
					return match.Groups[1].Value + "("
						+ (zeroBased - 1).ToString(CultureInfo.InvariantCulture) + ",";
				});

			changed = count;
			return edited;
		}

		private static string NameOf(JsonObject record, string field)
		{
			if (!(record[field] is JsonArray array)) return null;
			System.Text.StringBuilder text = new System.Text.StringBuilder();
			foreach (JsonNode node in array)
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

		private static int NumberOf(JsonObject record, string field)
		{
			return record[field] is JsonValue value
				&& int.TryParse(value.ToJsonString(), NumberStyles.Integer,
					CultureInfo.InvariantCulture, out int number) ? number : 0;
		}

		private static PakChainData Jumps(Workspace workspace, string name,
			out PakFile decoded)
		{
			byte[] data = workspace.Read(name);
			int chains = data.Length >= 4 ? BitConverter.ToInt32(data, 0) : 0;
			decoded = Pak.Read(data, Pak.FamilyOf(name, chains));
			return decoded.Chains.FirstOrDefault(c => c.Label == "jumps");
		}

		/// <summary>A row with every field present, so the writer has all of them.</summary>
		private static JsonObject NewRow()
		{
			JsonObject row = new JsonObject();
			SetNumbers(row, "plPos", new[] { 0, 0, 0 });
			SetNumber(row, "plRot", 0);
			SetName(row, "nextMapName", string.Empty);
			SetNumber(row, "nextMapIndex", 0);
			SetNumber(row, "conditionFlag", 1);
			SetNumber(row, "kind", -1);
			return row;
		}

		private static void SetNumber(JsonObject record, string field, int value)
		{
			record[field] = JsonValue.Create(value);
		}

		private static void SetNumbers(JsonObject record, string field, int[] values)
		{
			JsonArray array = new JsonArray();
			foreach (int value in values)
			{
				array.Add(JsonValue.Create(value));
			}
			record[field] = array;
		}

		/// <summary>
		/// A name back into the fixed width byte field it came out of, zero padded. The
		/// game reads it as a C string, so what matters is the zero after the name; the
		/// bytes past that are never looked at, and writing zeros is the tidiest thing
		/// to leave behind.
		/// </summary>
		private static void SetName(JsonObject record, string field, string value)
		{
			// Bytes, and as bytes - the writer asks a value for the exact type its field
			// has, so an int here is a run time failure rather than a conversion.
			JsonArray array = new JsonArray();
			for (int i = 0; i < NameLength; i++)
			{
				array.Add(JsonValue.Create(i < value.Length ? (byte)value[i] : (byte)0));
			}
			record[field] = array;
		}
	}
}
