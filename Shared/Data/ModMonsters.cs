// A mod's monsters and formations, as its items: definitions in JSON, composed into the
// game's own tables as the game reads them (the client's content-chain transforms) or
// written into a Steam project's files/ at Install (Crystal).
//
//   defs/monsters/<id>.json
//   {
//     "id": "goblin-chief",
//     "number": 1001,            // the monster id the game knows it by; Crystal gives it, from 1001
//     "base": 1,                 // the game's monster to start from (Goblin)
//     "name": "Goblin Chief",
//     "look": 1,                 // whose texture it wears (a monster of the same family); the base's when absent
//     "fields": { "maxHp": 60, "level": 4, "exp": 30, "gil": 80, "strength": 12 }
//   }
//
// The record is the base's 100 bytes (mon.MonsterParameter.parse) with the id, the name id
// and the fields written in, appended to monster.chaindata's chain 0; the base's offset
// record (chain 4, 160 bytes, keyed by monster id - where the cursor, the damage numbers
// and the shadow go) is appended under the new id too, since the battle looks one up for
// every monster it draws. The battle model is the family's (f<family>.nmdp) and a monster's
// own part is only its texture (f<family>_<id>.ntxp.lz) - a mod monster has none of its
// own, so the chain answers for it with the texture of the monster `look` names (the base's
// by default): a recolour of another family member is one number away. The name goes into
// eureka_battle.msd (every language's) at 2001 and up; the game's own end at 1373.
//
//   defs/formations/<id>.json
//   {
//     "id": "chief-and-goblins",
//     "number": 1001,            // the monster party id; Crystal gives it, from 1001
//     "name": "Goblin Chief and two Goblins",
//     "slots": [ { "monster": 1001, "min": 1, "max": 1 }, { "monster": 1, "min": 2, "max": 2 } ]
//   }
//
// An 18-byte record of monster_party_table.bbd: the party id s16, then four (monster id s16,
// min u8, max u8) slots, appended. A formation is what a battle starts with:
// Game.Battle.Start(number), the Encounter component, a startBattle in a CastScript.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using OpenFF.Content;

namespace OpenFF.Data
{
	internal sealed class ModMonster
	{
		public string Id;
		public int Number;
		public int Base;
		public string Name;
		/// <summary>The monster whose texture it wears (same family); 0 for the base's.</summary>
		public int Look;
		public Dictionary<string, int> Fields = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
		public string Source;

		public const int FirstNumber = 1001;
		public const int FirstNameId = 2001;

		public static ModMonster Parse(string json, string source = null)
		{
			JsonNode node = JsonNode.Parse(json);
			if (node == null) return null;
			ModMonster m = new ModMonster
			{
				Id = node["id"]?.GetValue<string>(),
				Number = node["number"]?.GetValue<int>() ?? 0,
				Base = node["base"]?.GetValue<int>() ?? -1,
				Name = node["name"]?.GetValue<string>(),
				Look = node["look"]?.GetValue<int>() ?? 0,
				Source = source
			};
			if (node["fields"] is JsonObject fields)
			{
				foreach (KeyValuePair<string, JsonNode> pair in fields)
				{
					if (pair.Value is JsonValue v && v.TryGetValue(out int n)) m.Fields[pair.Key] = n;
					else if (pair.Value is JsonValue s && s.TryGetValue(out string text) && int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int k)) m.Fields[pair.Key] = k;
				}
			}
			return m;
		}

		public string ToJson()
		{
			JsonObject node = new JsonObject { ["id"] = Id, ["number"] = Number, ["base"] = Base, ["name"] = Name ?? "" };
			if (Look > 0) node["look"] = Look;
			if (Fields.Count > 0)
			{
				JsonObject fields = new JsonObject();
				foreach (KeyValuePair<string, int> pair in Fields.OrderBy(p => p.Key, StringComparer.OrdinalIgnoreCase)) fields[pair.Key] = pair.Value;
				node["fields"] = fields;
			}
			return node.ToJsonString(new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
		}
	}

	internal sealed class ModFormationSlot
	{
		public int Monster;
		public int Min = 1;
		public int Max = 1;
	}

	internal sealed class ModFormation
	{
		public string Id;
		public int Number;
		public string Name;
		public List<ModFormationSlot> Slots = new List<ModFormationSlot>();
		public string Source;

		public const int FirstNumber = 1001;
		public const int SlotCount = 4;

		public static ModFormation Parse(string json, string source = null)
		{
			JsonNode node = JsonNode.Parse(json);
			if (node == null) return null;
			ModFormation f = new ModFormation
			{
				Id = node["id"]?.GetValue<string>(),
				Number = node["number"]?.GetValue<int>() ?? 0,
				Name = node["name"]?.GetValue<string>(),
				Source = source
			};
			if (node["slots"] is JsonArray slots)
			{
				foreach (JsonNode slot in slots)
				{
					if (slot == null) continue;
					f.Slots.Add(new ModFormationSlot
					{
						Monster = slot["monster"]?.GetValue<int>() ?? 0,
						Min = slot["min"]?.GetValue<int>() ?? 1,
						Max = slot["max"]?.GetValue<int>() ?? 1
					});
				}
			}
			return f;
		}

		public string ToJson()
		{
			JsonObject node = new JsonObject { ["id"] = Id, ["number"] = Number, ["name"] = Name ?? "" };
			JsonArray slots = new JsonArray();
			foreach (ModFormationSlot slot in Slots) slots.Add(new JsonObject { ["monster"] = slot.Monster, ["min"] = slot.Min, ["max"] = slot.Max });
			node["slots"] = slots;
			return node.ToJsonString(new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
		}
	}

	internal static class ModMonsters
	{
		public const string Folder = "defs/monsters";
		public const string FormationsFolder = "defs/formations";

		public const int Stride = 100;
		public const int OffsetStride = 160;
		public const int PartyStride = 18;

		public static List<ModMonster> Load(IEnumerable<string> roots, List<string> notes = null)
		{
			List<ModMonster> list = new List<ModMonster>();
			foreach (string root in roots ?? Enumerable.Empty<string>())
			{
				string directory = Path.Combine(root, Folder.Replace('/', Path.DirectorySeparatorChar));
				if (!Directory.Exists(directory)) continue;
				foreach (string file in Directory.EnumerateFiles(directory, "*.json").OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
				{
					try
					{
						ModMonster m = ModMonster.Parse(File.ReadAllText(file), file);
						if (m == null) continue;
						if (string.IsNullOrWhiteSpace(m.Id)) m.Id = Path.GetFileNameWithoutExtension(file);
						if (m.Number <= 0 || m.Base < 0) { notes?.Add(file + ": a monster needs a number and a base"); continue; }
						list.Add(m);
					}
					catch (Exception ex) { notes?.Add(file + ": " + ex.Message); }
				}
			}
			return list;
		}

		public static List<ModFormation> LoadFormations(IEnumerable<string> roots, List<string> notes = null)
		{
			List<ModFormation> list = new List<ModFormation>();
			foreach (string root in roots ?? Enumerable.Empty<string>())
			{
				string directory = Path.Combine(root, FormationsFolder.Replace('/', Path.DirectorySeparatorChar));
				if (!Directory.Exists(directory)) continue;
				foreach (string file in Directory.EnumerateFiles(directory, "*.json").OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
				{
					try
					{
						ModFormation f = ModFormation.Parse(File.ReadAllText(file), file);
						if (f == null) continue;
						if (string.IsNullOrWhiteSpace(f.Id)) f.Id = Path.GetFileNameWithoutExtension(file);
						if (f.Number <= 0) { notes?.Add(file + ": a formation needs a number"); continue; }
						if (f.Slots.Count == 0) { notes?.Add(file + ": a formation with no monsters"); continue; }
						list.Add(f);
					}
					catch (Exception ex) { notes?.Add(file + ": " + ex.Message); }
				}
			}
			return list;
		}

		public static int NextNumber(IEnumerable<ModMonster> monsters)
		{
			int max = ModMonster.FirstNumber - 1;
			foreach (ModMonster m in monsters ?? Enumerable.Empty<ModMonster>()) if (m.Number > max) max = m.Number;
			return max + 1;
		}

		public static int NextFormationNumber(IEnumerable<ModFormation> formations)
		{
			int max = ModFormation.FirstNumber - 1;
			foreach (ModFormation f in formations ?? Enumerable.Empty<ModFormation>()) if (f.Number > max) max = f.Number;
			return max + 1;
		}

		// mon.MonsterParameter.parse, in record order: the head, the body (ys.BodyParameter),
		// the attack (ys.PhysicsAttackParameter), the defence (ys.PhysicsDefenseParameter), the
		// magic defence, two special actions, the drops (DroppingDataParameter), the draw map.
		private sealed class Field { public int Offset; public int Size; public bool Signed; public string Group; public Field(int o, int s, bool signed, string group) { Offset = o; Size = s; Signed = signed; Group = group; } }

		private static readonly Dictionary<string, Field> Fields = new Dictionary<string, Field>(StringComparer.OrdinalIgnoreCase)
		{
			["nameId"] = new Field(0x00, 2, true, "head"), ["textId"] = new Field(0x02, 2, true, "head"), ["familyId"] = new Field(0x04, 2, true, "head"),
			["modelId"] = new Field(0x06, 2, true, "head"), ["monsterId"] = new Field(0x08, 2, true, "head"),
			["level"] = new Field(0x0A, 1, false, "head"), ["size"] = new Field(0x0B, 1, false, "head"), ["maxHp"] = new Field(0x0C, 4, true, "head"),
			["strength"] = new Field(0x10, 1, false, "body"), ["vitality"] = new Field(0x11, 1, false, "body"), ["dexterity"] = new Field(0x12, 1, false, "body"),
			["intelligence"] = new Field(0x13, 1, false, "body"), ["mind"] = new Field(0x14, 1, false, "body"),
			["aiLevel"] = new Field(0x15, 1, false, "body"), ["magicSkill"] = new Field(0x16, 1, false, "body"), ["weight"] = new Field(0x17, 1, false, "body"),
			["actionNumber"] = new Field(0x18, 2, true, "body"), ["devide"] = new Field(0x1A, 2, true, "body"),
			["aggressivity"] = new Field(0x1C, 4, true, "attack"), ["hitProbability"] = new Field(0x20, 1, false, "attack"), ["optionProbability"] = new Field(0x21, 1, false, "attack"),
			["optionMagicId"] = new Field(0x22, 2, true, "attack"), ["armsAttribute"] = new Field(0x24, 2, true, "attack"), ["attackType"] = new Field(0x26, 2, true, "attack"),
			["attackOption"] = new Field(0x28, 2, true, "attack"), ["equipOption"] = new Field(0x2A, 2, true, "attack"),
			["phylacticPower"] = new Field(0x2C, 4, true, "defence"), ["avoidanceNumber"] = new Field(0x30, 4, true, "defence"), ["armsWeakAttribute"] = new Field(0x34, 2, true, "defence"),
			["defenceArmsAttribute"] = new Field(0x36, 2, true, "defence"), ["antiType"] = new Field(0x38, 2, true, "defence"), ["antiOption"] = new Field(0x3A, 2, true, "defence"),
			["defenceEquipOption"] = new Field(0x3C, 2, true, "defence"),
			["weakType"] = new Field(0x40, 2, true, "magic defence"), ["magicPhylacticPower"] = new Field(0x42, 2, true, "magic defence"),
			["special1Id"] = new Field(0x44, 2, true, "special"), ["special1Probability"] = new Field(0x46, 2, true, "special"), ["special1StartHp"] = new Field(0x48, 4, true, "special"),
			["special2Id"] = new Field(0x4C, 2, true, "special"), ["special2Probability"] = new Field(0x4E, 2, true, "special"), ["special2StartHp"] = new Field(0x50, 4, true, "special"),
			["dropProbability"] = new Field(0x54, 2, true, "drops"), ["dropTable"] = new Field(0x56, 2, true, "drops"), ["gil"] = new Field(0x58, 4, true, "drops"), ["exp"] = new Field(0x5C, 4, true, "drops"),
			["drawMapId"] = new Field(0x60, 1, false, "head"),
		};

		/// <summary>The fields a definition may set, by the game's names, in record order; not the ids the composer writes.</summary>
		public static IEnumerable<string> FieldNames()
		{
			foreach (KeyValuePair<string, Field> pair in Fields.OrderBy(p => p.Value.Offset))
				if (pair.Key != "nameId" && pair.Key != "monsterId") yield return pair.Key;
		}

		/// <summary>Which block of the record a field is in: head, body, attack, defence, magic defence, special, drops.</summary>
		public static string GroupOf(string name) => Fields.TryGetValue(name, out Field f) ? f.Group : null;

		/// <summary>The index of a monster id in chain 0; -1 when the pack has none.</summary>
		public static int IndexOf(ChainPack pack, int monsterId)
		{
			int records = pack.Size(0) / Stride;
			for (int i = 0; i < records; i++) if (ChainPack.S16(pack.Record(0, Stride, i), 8) == monsterId) return i;
			return -1;
		}

		/// <summary>The base's record bytes, or null.</summary>
		public static byte[] RecordOf(ChainPack pack, int monsterId)
		{
			int i = IndexOf(pack, monsterId);
			return i < 0 ? null : pack.Record(0, Stride, i);
		}

		/// <summary>A field's value in a record by name; null for an unknown name.</summary>
		public static int? Get(byte[] record, string name)
		{
			if (!Fields.TryGetValue(name, out Field f) || f.Offset + f.Size > record.Length) return null;
			switch (f.Size)
			{
				case 1: return f.Signed ? (sbyte)record[f.Offset] : record[f.Offset];
				case 2: return f.Signed ? (int)ChainPack.S16(record, f.Offset) : (ushort)ChainPack.S16(record, f.Offset);
				default: return ChainPack.S32(record, f.Offset);
			}
		}

		private static void Put(byte[] record, Field f, int value)
		{
			for (int i = 0; i < f.Size; i++) record[f.Offset + i] = (byte)(value >> (8 * i));
		}

		/// <summary>The k-th definition's name id in eureka_battle.msd.</summary>
		public static int NameId(int k) => ModMonster.FirstNameId + k;

		/// <summary>A definition's record: the base's bytes with the id, the name id and the fields written in.</summary>
		public static byte[] Record(ChainPack pack, ModMonster m, int nameId, List<string> notes)
		{
			byte[] r = RecordOf(pack, m.Base);
			if (r == null) { notes?.Add(m.Id + ": no monster " + m.Base + " to start from"); return null; }
			byte[] record = (byte[])r.Clone();
			Put(record, Fields["monsterId"], m.Number);
			Put(record, Fields["nameId"], nameId);
			foreach (KeyValuePair<string, int> pair in m.Fields)
			{
				if (string.Equals(pair.Key, "monsterId", StringComparison.OrdinalIgnoreCase) || string.Equals(pair.Key, "nameId", StringComparison.OrdinalIgnoreCase)) continue;
				if (Fields.TryGetValue(pair.Key, out Field f)) Put(record, f, pair.Value);
				else notes?.Add(m.Id + ": no field '" + pair.Key + "' on a monster");
			}
			return record;
		}

		/// <summary>The base's offset record (chain 4) under the new id, or null when the base has none.</summary>
		private static byte[] OffsetRecord(ChainPack pack, ModMonster m)
		{
			if (pack.Count < 5) return null;
			int records = pack.Size(4) / OffsetStride;
			for (int i = 0; i < records; i++)
			{
				byte[] r = pack.Record(4, OffsetStride, i);
				if (ChainPack.S32(r, 0) != m.Base) continue;
				byte[] record = (byte[])r.Clone();
				Array.Copy(BitConverter.GetBytes(m.Number), 0, record, 0, 4);
				return record;
			}
			return null;
		}

		/// <summary>monster.chaindata with every definition appended: its record to chain 0, its offsets to chain 4; the other chains as they were.</summary>
		public static byte[] ComposeChain(byte[] data, IReadOnlyList<ModMonster> monsters, List<string> notes = null)
		{
			if (data == null || monsters == null || monsters.Count == 0) return data;
			ChainPack pack;
			try { pack = ChainPack.Read(data); }
			catch (Exception ex) { notes?.Add("monster.chaindata: " + ex.Message); return data; }
			if (pack.Count < 5) { notes?.Add("monster.chaindata has " + pack.Count + " chains, not the six FF3 has: no mod monsters"); return data; }
			List<byte[]> records = new List<byte[]>(), offsets = new List<byte[]>();
			for (int k = 0; k < monsters.Count; k++)
			{
				// Already there (a Steam project's files/ composed at Install and read again).
				if (IndexOf(pack, monsters[k].Number) >= 0) continue;
				byte[] record = Record(pack, monsters[k], NameId(k), notes);
				if (record == null) continue;
				records.Add(record);
				byte[] offset = OffsetRecord(pack, monsters[k]);
				if (offset != null) offsets.Add(offset); else notes?.Add(monsters[k].Id + ": the base has no offset record; the battle may not place its cursor");
			}
			if (records.Count == 0) return data;
			return Append(pack, new Dictionary<int, (int, List<byte[]>)> { [0] = (Stride, records), [4] = (OffsetStride, offsets) });
		}

		/// <summary>
		/// The pack laid out again with records appended to some chains: the header and the chain
		/// table as they were, each chain's bytes then its new records (from a whole stride, since
		/// Square's chains are sometimes two bytes short of their last record), four-byte aligned.
		/// </summary>
		public static byte[] Append(ChainPack pack, IReadOnlyDictionary<int, (int stride, List<byte[]> records)> extra)
		{
			// As the file was laid out: monster.chaindata keeps a 128-byte header and starts every
			// chain on a 128-byte line; a pack laid out tighter keeps its own line.
			int align = 4;
			foreach (int a in new[] { 128, 64, 32, 16 })
			{
				bool all = true;
				for (int c = 0; c < pack.Count && all; c++) all = pack.Offset(c) % a == 0;
				if (all) { align = a; break; }
			}
			int headerSize = pack.Count > 0 ? Math.Max(16 + 8 * pack.Count, pack.Offset(0)) : 16 + 8 * pack.Count;
			using (MemoryStream out_ = new MemoryStream())
			{
				byte[] header = new byte[headerSize];
				Array.Copy(pack.Data, header, Math.Min(headerSize, pack.Data.Length));
				out_.Write(header, 0, header.Length);
				int[] offsets = new int[pack.Count], sizes = new int[pack.Count];
				for (int c = 0; c < pack.Count; c++)
				{
					offsets[c] = (int)out_.Position;
					if (extra.TryGetValue(c, out (int stride, List<byte[]> records) add) && add.records.Count > 0)
					{
						int records = pack.Records(c, add.stride);
						for (int i = 0; i < records; i++) { byte[] r = pack.Record(c, add.stride, i); out_.Write(r, 0, r.Length); }
						foreach (byte[] r in add.records) out_.Write(r, 0, r.Length);
					}
					else
					{
						out_.Write(pack.Data, pack.Offset(c), pack.Size(c));
					}
					sizes[c] = (int)out_.Position - offsets[c];
					while (out_.Position % align != 0) out_.WriteByte(0);
				}
				byte[] result = out_.ToArray();
				for (int c = 0; c < pack.Count; c++)
				{
					Array.Copy(BitConverter.GetBytes(offsets[c]), 0, result, 16 + 8 * c, 4);
					Array.Copy(BitConverter.GetBytes(sizes[c]), 0, result, 20 + 8 * c, 4);
				}
				return result;
			}
		}

		/// <summary>eureka_battle.msd with a name per definition appended (any language's copy: the names are the mod's words as written).</summary>
		public static byte[] ComposeMsd(byte[] msd, IReadOnlyList<ModMonster> monsters, List<string> notes = null)
		{
			if (msd == null || monsters == null || monsters.Count == 0) return msd;
			MsdFile file;
			try { file = Msd.Read(msd); }
			catch (Exception ex) { notes?.Add("eureka_battle.msd: " + ex.Message); return msd; }
			HashSet<uint> have = new HashSet<uint>(file.Messages.Select(x => x.Id));
			bool any = false;
			for (int k = 0; k < monsters.Count; k++)
			{
				uint id = (uint)NameId(k);
				if (have.Contains(id)) continue;
				file.Messages.Add(new MsdMessage { Id = id, Pages = new List<string> { monsters[k].Name ?? monsters[k].Id ?? "?" } });
				any = true;
			}
			return any ? Msd.Write(file) : msd;
		}

		/// <summary>monster_party_table.bbd with a record per formation appended (one already there is left).</summary>
		public static byte[] ComposeParties(byte[] table, IReadOnlyList<ModFormation> formations, List<string> notes = null)
		{
			if (table == null || formations == null || formations.Count == 0) return table;
			HashSet<int> have = new HashSet<int>();
			for (int i = 0; i + PartyStride <= table.Length; i += PartyStride) have.Add(ChainPack.S16(table, i));
			using (MemoryStream out_ = new MemoryStream())
			{
				out_.Write(table, 0, table.Length - table.Length % PartyStride);
				bool any = false;
				foreach (ModFormation f in formations)
				{
					if (have.Contains(f.Number)) continue;
					byte[] record = new byte[PartyStride];
					record[0] = (byte)f.Number; record[1] = (byte)(f.Number >> 8);
					for (int s = 0; s < ModFormation.SlotCount; s++)
					{
						int at = 2 + 4 * s;
						if (s < f.Slots.Count && f.Slots[s].Monster >= 0 && f.Slots[s].Max > 0)
						{
							record[at] = (byte)f.Slots[s].Monster; record[at + 1] = (byte)(f.Slots[s].Monster >> 8);
							record[at + 2] = (byte)Math.Max(0, Math.Min(f.Slots[s].Min, f.Slots[s].Max));
							record[at + 3] = (byte)Math.Max(1, f.Slots[s].Max);
						}
						else
						{
							// An empty slot as the game's tables have them: monster -1, none.
							record[at] = 0xFF; record[at + 1] = 0xFF;
						}
					}
					out_.Write(record, 0, record.Length);
					any = true;
				}
				return any ? out_.ToArray() : table;
			}
		}

		/// <summary>
		/// The texture a mod monster wears: for f&lt;family&gt;_&lt;number&gt;.ntxp.lz (any folder, any
		/// extension) the same name with the number of the monster `look` names, or the base's.
		/// Null when the name is not a mod monster's texture.
		/// </summary>
		public static string TextureAlias(string name, IReadOnlyList<ModMonster> monsters)
		{
			if (string.IsNullOrEmpty(name) || monsters == null || monsters.Count == 0) return null;
			string file = Path.GetFileName(name);
			// f001_301.ntxp.lz -> family 1, monster 301
			if (file.Length < 9 || file[0] != 'f' || file[4] != '_') return null;
			int dot = file.IndexOf('.', 5);
			if (dot < 0) dot = file.Length;
			if (!int.TryParse(file.Substring(1, 3), NumberStyles.Integer, CultureInfo.InvariantCulture, out int family)) return null;
			if (!int.TryParse(file.Substring(5, dot - 5), NumberStyles.Integer, CultureInfo.InvariantCulture, out int number)) return null;
			foreach (ModMonster m in monsters)
			{
				if (m.Number != number) continue;
				int wear = m.Look > 0 ? m.Look : m.Base;
				string replaced = "f" + family.ToString("000", CultureInfo.InvariantCulture) + "_" + wear.ToString("000", CultureInfo.InvariantCulture) + file.Substring(dot);
				int cut = name.Length - file.Length;
				return name.Substring(0, cut) + replaced;
			}
			return null;
		}

		public static bool IsChain(string name) => string.Equals(Path.GetFileName(name ?? ""), "monster.chaindata", StringComparison.OrdinalIgnoreCase);
		public static bool IsMsd(string name) => string.Equals(Path.GetFileName(name ?? ""), "eureka_battle.msd", StringComparison.OrdinalIgnoreCase);
		public static bool IsParties(string name) => string.Equals(Path.GetFileName(name ?? ""), "monster_party_table.bbd", StringComparison.OrdinalIgnoreCase);
	}
}
