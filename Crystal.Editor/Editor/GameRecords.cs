// The game's own records as forms: an item (a spell is an item of the magic chain), a
// monster, a monster party, read from the tables the workspace has - the project's
// override where there is one, the shipped file otherwise - and written back into the
// override, field by field, with the shipped value kept beside for the form to grey in.
// The same field tables the mod definitions use (Shared/Data/ModItems.cs, ModMonsters.cs),
// so a Steam mod edits a Goblin with the form a mod monster is made with. The Tables view's
// grid is the same bytes; this is the readable way in.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using OpenFF.Data;

namespace Crystal.Editor
{
	internal static class GameRecords
	{
		public const string ItemPak = "files/item_parameter.pak";
		public const string MonsterChain = "files/monster.chaindata";
		public const string PartyTable = "files/monster_party_table.bbd";

		private static byte[] Current(Workspace workspace, string name)
		{
			try { return workspace.Read(name); } catch (Exception) { return null; }
		}

		/// <summary>The form for one record: kind item|monster|party, the game's id.</summary>
		public static object Describe(Workspace workspace, string kind, int id, Func<uint, string> lookup)
		{
			switch (kind)
			{
				case "item": return DescribeItem(workspace, id, lookup);
				case "monster": return DescribeMonster(workspace, id, lookup);
				case "party": return DescribeParty(workspace, id);
				default: return new { ok = false, error = "no record kind '" + kind + "'" };
			}
		}

		private static object DescribeItem(Workspace workspace, int id, Func<uint, string> lookup)
		{
			byte[] current = Current(workspace, ItemPak), shipped = workspace.ReadShipped(ItemPak);
			if (current == null) return new { ok = false, error = "no item_parameter.pak" };
			ChainPack pack = ChainPack.Read(current);
			byte[] record = ModItems.RecordOf(pack, id, out int chain);
			if (record == null) return new { ok = false, error = "no item " + id };
			byte[] original = shipped != null ? ModItems.RecordOf(ChainPack.Read(shipped), id, out _) : null;
			List<object> fields = new List<object>();
			foreach (string name in ModItems.FieldNames(chain))
			{
				fields.Add(new
				{
					name, group = name == "buy" || name == "price" ? "shop" : chain == 3 && (name == "aggressivity" || name == "successProbability" || name == "magicClass") ? "magic" : "record",
					value = ModItems.Get(record, chain, name),
					shipped = original != null ? ModItems.Get(original, chain, name) : null
				});
			}
			int nameId = ChainPack.S16(record, 4), captionId = ChainPack.S16(record, 6);
			return new
			{
				ok = true, kind = "item", id, chain = ModItems.ChainNames[chain], file = ItemPak, overridden = workspace.IsOverridden(ItemPak),
				name = Text(lookup, nameId), caption = Text(lookup, captionId), nameId, captionId,
				fields
			};
		}

		private static object DescribeMonster(Workspace workspace, int id, Func<uint, string> lookup)
		{
			byte[] current = Current(workspace, MonsterChain), shipped = workspace.ReadShipped(MonsterChain);
			if (current == null) return new { ok = false, error = "no monster.chaindata" };
			byte[] record = ModMonsters.RecordOf(ChainPack.Read(current), id);
			if (record == null) return new { ok = false, error = "no monster " + id };
			byte[] original = shipped != null ? ModMonsters.RecordOf(ChainPack.Read(shipped), id) : null;
			List<object> fields = new List<object>();
			foreach (string name in ModMonsters.FieldNames())
			{
				fields.Add(new { name, group = ModMonsters.GroupOf(name), value = ModMonsters.Get(record, name), shipped = original != null ? ModMonsters.Get(original, name) : null });
			}
			int nameId = ChainPack.S16(record, 0);
			return new
			{
				ok = true, kind = "monster", id, file = MonsterChain, overridden = workspace.IsOverridden(MonsterChain),
				name = Text(lookup, nameId), nameId, family = ChainPack.S16(record, 4),
				fields
			};
		}

		private static object DescribeParty(Workspace workspace, int id)
		{
			byte[] current = Current(workspace, PartyTable), shipped = workspace.ReadShipped(PartyTable);
			if (current == null) return new { ok = false, error = "no monster_party_table.bbd" };
			int at = PartyOffset(current, id);
			if (at < 0) return new { ok = false, error = "no monster party " + id };
			int was = shipped != null ? PartyOffset(shipped, id) : -1;
			List<object> slots = new List<object>();
			for (int s = 0; s < ModFormation.SlotCount; s++)
			{
				int o = at + 2 + 4 * s;
				slots.Add(new
				{
					monster = ChainPack.S16(current, o), min = current[o + 2], max = current[o + 3],
					shippedMonster = was >= 0 ? ChainPack.S16(shipped, was + 2 + 4 * s) : -1,
					shippedMin = was >= 0 ? shipped[was + 4 + 4 * s] : 0, shippedMax = was >= 0 ? shipped[was + 5 + 4 * s] : 0
				});
			}
			return new { ok = true, kind = "party", id, file = PartyTable, overridden = workspace.IsOverridden(PartyTable), slots };
		}

		private static int PartyOffset(byte[] table, int id)
		{
			for (int i = 0; i + ModMonsters.PartyStride <= table.Length; i += ModMonsters.PartyStride)
				if (ChainPack.S16(table, i) == id) return i;
			return -1;
		}

		private static string Text(Func<uint, string> lookup, int id)
		{
			if (id <= 0 || lookup == null) return null;
			try { return lookup((uint)id); } catch (Exception) { return null; }
		}

		/// <summary>Writes the fields given into the record and the file into the override; the form afterwards.</summary>
		public static object Save(Workspace workspace, string kind, int id, JsonObject fields, JsonArray slots, Func<uint, string> lookup)
		{
			switch (kind)
			{
				case "item":
				{
					byte[] data = Current(workspace, ItemPak);
					if (data == null) return new { ok = false, error = "no item_parameter.pak" };
					ChainPack pack = ChainPack.Read(data);
					int chain = ModItems.ChainOf(pack, id, out int index);
					if (chain < 0) return new { ok = false, error = "no item " + id };
					int stride = ModItems.StrideOf(chain);
					byte[] record = pack.Record(chain, stride, index);
					List<string> refused = new List<string>();
					foreach (KeyValuePair<string, JsonNode> pair in fields ?? new JsonObject())
						if (!ModItems.Set(record, chain, pair.Key, Int(pair.Value))) refused.Add(pair.Key);
					byte[] copy = (byte[])data.Clone();
					int start = pack.Offset(chain) + stride * index;
					Array.Copy(record, 0, copy, start, Math.Min(stride, copy.Length - start));
					workspace.Write(ItemPak, copy);
					return new { ok = true, refused, record = DescribeItem(workspace, id, lookup) };
				}
				case "monster":
				{
					byte[] data = Current(workspace, MonsterChain);
					if (data == null) return new { ok = false, error = "no monster.chaindata" };
					ChainPack pack = ChainPack.Read(data);
					int index = ModMonsters.IndexOf(pack, id);
					if (index < 0) return new { ok = false, error = "no monster " + id };
					byte[] record = pack.Record(0, ModMonsters.Stride, index);
					List<string> refused = new List<string>();
					foreach (KeyValuePair<string, JsonNode> pair in fields ?? new JsonObject())
						if (!ModMonsters.Set(record, pair.Key, Int(pair.Value))) refused.Add(pair.Key);
					byte[] copy = (byte[])data.Clone();
					Array.Copy(record, 0, copy, pack.Offset(0) + ModMonsters.Stride * index, ModMonsters.Stride);
					workspace.Write(MonsterChain, copy);
					return new { ok = true, refused, record = DescribeMonster(workspace, id, lookup) };
				}
				case "party":
				{
					byte[] data = Current(workspace, PartyTable);
					if (data == null) return new { ok = false, error = "no monster_party_table.bbd" };
					int at = PartyOffset(data, id);
					if (at < 0) return new { ok = false, error = "no monster party " + id };
					byte[] copy = (byte[])data.Clone();
					for (int s = 0; s < ModFormation.SlotCount; s++)
					{
						JsonNode slot = slots != null && s < slots.Count ? slots[s] : null;
						int o = at + 2 + 4 * s;
						int monster = slot?["monster"] != null ? Int(slot["monster"]) : -1;
						int max = slot?["max"] != null ? Int(slot["max"]) : 0;
						int min = slot?["min"] != null ? Int(slot["min"]) : 0;
						if (monster < 0 || max <= 0) { copy[o] = 0xFF; copy[o + 1] = 0xFF; copy[o + 2] = 0; copy[o + 3] = 0; continue; }
						copy[o] = (byte)monster; copy[o + 1] = (byte)(monster >> 8);
						copy[o + 2] = (byte)Math.Max(0, Math.Min(min, max)); copy[o + 3] = (byte)Math.Min(6, max);
					}
					workspace.Write(PartyTable, copy);
					return new { ok = true, record = DescribeParty(workspace, id) };
				}
				default: return new { ok = false, error = "no record kind '" + kind + "'" };
			}
		}

		private static int Int(JsonNode node)
		{
			if (node is not JsonValue value) return 0;
			if (value.TryGetValue(out int i)) return i;
			if (value.TryGetValue(out long l)) return (int)l;
			if (value.TryGetValue(out double d)) return (int)d;
			if (value.TryGetValue(out string s) && int.TryParse(s, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int n)) return n;
			return 0;
		}
	}
}
