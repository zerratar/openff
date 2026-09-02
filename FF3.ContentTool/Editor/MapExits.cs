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
// That pairing is also why only the last exit can be removed. Taking one out of the
// middle shifts every row after it down a slot while the attributes that name them stay
// where they are, and every other map's arrival index pointing here would be wrong too.
//
// A script can turn an exit off and on - setMapJumpFlag names one of the same twelve
// slots - but that is switching a trigger that already exists.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json.Nodes;

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
				if (wanted.Length >= NameLength)
				{
					return "a map name has " + NameLength
						+ " bytes to fit in, with room for the zero that ends it, and "
						+ wanted + " is " + wanted.Length;
				}
				if (wanted.Length > 0 && !workspace.Exists("files/" + wanted + ".hich"))
				{
					// Not refused - a name can lead somewhere this copy of the game does
					// not have - but worth saying, because a typo looks exactly like this.
					result.Notes.Add("there is no map called " + wanted
						+ " in the content, so this exit leads nowhere that exists");
				}
				SetName(record, "nextMapName", wanted);
			}

			SetNumber(record, "nextMapIndex", edit.ToIndex);
			SetNumber(record, "conditionFlag", edit.ConditionFlag);
			SetNumber(record, "kind", edit.Kind);
			return null;
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

		/// <summary>Takes both halves out again: the row, and the region that fired it.</summary>
		public static MapExitResult Remove(Workspace workspace, string map, int slot)
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

			// Only the last one. A row in the middle cannot go: every slot after it would
			// shift down, and the collision attributes that name them would not - so the
			// triggers and the rows would come apart, and so would every other map's
			// arrival index pointing here.
			if (slot != jumps.Records.Count)
			{
				result.Error = "only the last exit can be removed. Exit " + slot + " of "
					+ jumps.Records.Count + " would shift every one after it down a slot, "
					+ "and the triggers in the mesh - and the arrival index of every exit "
					+ "on other maps that leads here - would still name the old numbers";
				return result;
			}

			jumps.Records.RemoveAt(slot - 1);

			bool hadRegion = false;
			if (workspace.Exists(meshName))
			{
				try
				{
					MclFile mesh = Mcl.Read(Lz.Decompress(workspace.Read(meshName)));
					hadRegion = Mcl.RemoveJumpRegion(mesh, slot);
					if (hadRegion)
					{
						workspace.Write(meshName, Lz.Compress(Mcl.Build(mesh)));
						result.Also = meshName;
					}
				}
				catch (Exception problem)
				{
					result.Error = "the row is still there, because the collision mesh "
						+ "would not read and removing one half is worse than neither: "
						+ problem.Message;
					return result;
				}
			}

			workspace.Write(pakName, Pak.Write(decoded));
			result.Wrote = pakName;
			result.Slot = slot;
			result.Ok = true;
			result.Notes.Add(hadRegion
				? "removed exit " + slot + " and the region that triggered it"
				: "removed exit " + slot + " - it had no trigger in the mesh anyway");
			return result;
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
