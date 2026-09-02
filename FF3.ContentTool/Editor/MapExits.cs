// Changing where an exit leads.
//
// An exit is two halves in two places, and only one of them is data:
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
// So this changes an exit; it cannot make one. A new row with no region to trigger it
// would sit in the file and never fire, which is worse than saying so. Making one means
// editing <map>_col.mcl.lz, and nothing here reads that yet.
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
		public List<string> Notes { get; set; } = new List<string>();
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
					result.Error = "a map name has " + NameLength
						+ " bytes to fit in, with room for the zero that ends it, and "
						+ wanted + " is " + wanted.Length;
					return result;
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

			byte[] written = Pak.Write(decoded);
			workspace.Write(name, written);
			result.Wrote = name;
			result.Ok = true;
			return result;
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
