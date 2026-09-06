// FF4 3D's tables: their shape from the files, their names from the engine.
//
// FF4 keeps the same containers as FF3 (item_parameter.pak, monster.chaindata,
// player.chaindata, one .pak per map) but different records in them: 97 weapons of 88
// bytes where FF3 has 56, 252 monsters of 152 where FF3 has 100, and a map .pak of four
// single records where FF3 has seven chains. FF3's layouts must not be applied - they
// would type the wrong bytes and a save would rewrite the table wrongly.
//
// There is no FF4 source, so every name here has one of three kinds of evidence, and the
// comment on each chain says which:
//
//   engine   libff4.so (unstripped) reads the field at that offset in a getter of that
//            name: itm::EquipParameter::aggressivity is `ldrh w0, [x0, #0x34]`, so
//            aggressivity is the 16-bit word at 0x34. itm::ItemManager finds a record by
//            the 16-bit id at offset 2 in tables of stride 0x30/0x58/0x54/0x20;
//            mon::MonsterManager by the id at offset 8 in a table of stride 0x98.
//   FF3      the same field, same offset, in FF3's decompiled class, and FF4's values
//            fit it (a Potion that costs 30 to buy and 15 to sell has buy at 0x1C and
//            price at 0x20, as FF3 does).
//   stride   only the record size is known: an id field counts up record after record.
//
// A field nobody has named yet is called by its offset - x1A is the 16-bit word at 0x1A
// - so the column is still there to edit, and its name says exactly how much is known.
// Two chains are two bytes short of a whole last record in Square's own files
// (consumables: 60 x 48 - 2; monsters: 252 x 152 - 2); Pak.Read pads that last record
// for display and Pak.Write trims it again, so they rebuild byte for byte.
//
// Hand-written. Tools/ff4_fields.py is the disassembly helper that produced the engine
// evidence (python Tools/ff4_fields.py <libff4.so> '^_ZNK?3itm14EquipParameter').

using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Crystal
{
	internal static class PakRecordsFf4
	{
		public const string Item = "Ff4Item";
		public const string Monster = "Ff4Monster";
		public const string Player = "Ff4Player";
		public const string Map = "Ff4Map";

		private static PakField F(string name, FieldType type, int count = 1)
		{
			return new PakField(name, type, count);
		}

		/// <summary>An unnamed 16-bit word, called by its offset.</summary>
		private static PakField X(int offset)
		{
			return new PakField("x" + offset.ToString("X2", CultureInfo.InvariantCulture), FieldType.S16, 1);
		}

		/// <summary>Unnamed words from one offset up to (not including) another.</summary>
		private static IEnumerable<PakField> Words(int from, int to)
		{
			for (int at = from; at < to; at += 2)
			{
				yield return X(at);
			}
		}

		private static PakField[] Fields(params object[] parts)
		{
			List<PakField> fields = new List<PakField>();
			foreach (object part in parts)
			{
				if (part is PakField field) fields.Add(field);
				else if (part is IEnumerable<PakField> many) fields.AddRange(many);
			}
			return fields.ToArray();
		}

		/// <summary>
		/// The first 28 bytes of every FF4 item record. 0x00-0x09 and 0x16 from the
		/// engine (ItemManager matches the id at 2; ItemBaseParameter::usedPower reads
		/// the efficacy id at 0x16 and looks it up in EfficacyDataConvection), the
		/// names of 0x02-0x09 from FF3's identical prefix.
		/// </summary>
		private static IEnumerable<PakField> ItemBase()
		{
			return Fields(
				F("system", FieldType.U8), F("pad0", FieldType.U8),
				F("itemId", FieldType.S16), F("nameId", FieldType.S16),
				F("captionId", FieldType.S16), F("graphId", FieldType.S16),
				Words(0x0A, 0x16),
				F("efficacyId", FieldType.S16),
				Words(0x18, 0x1C));
		}

		/// <summary>buy and price: FF3's offsets, and a Potion costs 30 and sells for 15.</summary>
		private static IEnumerable<PakField> BuyPrice()
		{
			return Fields(F("buy", FieldType.S32), F("price", FieldType.S32));
		}

		/// <summary>
		/// The equipment half of a weapon or armour record, from itm::EquipParameter's
		/// getters: canEquip 0x2C (a job mask, u32), canEquipOnPosition 0x30,
		/// aggressivity 0x34, hitProbability 0x36, phylacticPower 0x38,
		/// avoidanceProbability 0x3A, magicPhylacticPower 0x3C,
		/// magicAvoidanceProbability 0x3E, powPlus 0x44, vitalPlus 0x46, speedPlus 0x48,
		/// intelPlus 0x4A, spiritPlus 0x4C. Runs from 0x24 to 0x4E.
		/// </summary>
		private static IEnumerable<PakField> Equip()
		{
			return Fields(
				F("x24", FieldType.S32), F("x28", FieldType.S32),
				F("canEquip", FieldType.U32),
				F("canEquipOnPosition", FieldType.S16), X(0x32),
				F("aggressivity", FieldType.S16), F("hitProbability", FieldType.S16),
				F("phylacticPower", FieldType.S16), F("avoidanceProbability", FieldType.S16),
				F("magicPhylacticPower", FieldType.S16), F("magicAvoidanceProbability", FieldType.S16),
				X(0x40), X(0x42),
				F("powPlus", FieldType.S16), F("vitalPlus", FieldType.S16), F("speedPlus", FieldType.S16),
				F("intelPlus", FieldType.S16), F("spiritPlus", FieldType.S16));
		}

		public static readonly PakChain[] Chains =
		{
			// ---- item_parameter.pak: ItemManager's four categories, in its order ------------
			new PakChain(Item, 0, "consumables", "itm::ConsumptionParameter", 48,
				Fields(ItemBase(), BuyPrice(), F("x24", FieldType.S32), F("x28", FieldType.S32), X(0x2C), X(0x2E)),
				"Usable items (60; ids 5001+). Names: engine for the id and efficacy, FF3 for the rest of the head and for buy/price; x.. fields not yet named."),
			new PakChain(Item, 1, "weapons", "itm::WeaponParameter", 88,
				Fields(ItemBase(), BuyPrice(), Equip(), Words(0x4E, 0x58)),
				"Weapons (97; ids 6000+). Equipment fields from itm::EquipParameter's getters in the engine; x.. fields not yet named."),
			new PakChain(Item, 2, "armour", "itm::ProtectionParameter", 84,
				Fields(ItemBase(), BuyPrice(), Equip(), Words(0x4E, 0x54)),
				"Armour, shields, helms, rings (84; ids 8000+). Same equipment fields as weapons; x.. fields not yet named."),
			new PakChain(Item, 3, "keyItems", "itm::ImportantParameter", 32,
				Fields(ItemBase(), X(0x1C), X(0x1E)),
				"Key items (62; ids 9001+)."),

			// ---- monster.chaindata: MonsterManager's tables ------------------------------------
			new PakChain(Monster, 0, "monsters", "mon::MonsterParameter", 152,
				Fields(
					F("nameId", FieldType.S16), F("textId", FieldType.S16), F("familyId", FieldType.S16),
					F("modelId", FieldType.S16), F("monsterId", FieldType.S16),
					F("level", FieldType.U8), F("size", FieldType.U8), F("maxHp", FieldType.S32),
					X(0x10),
					F("strength", FieldType.U8), F("vitality", FieldType.U8), F("dexterity", FieldType.U8),
					F("intelligence", FieldType.U8), F("mind", FieldType.U8), F("x17", FieldType.U8),
					Words(0x18, 0x92),
					F("flags", FieldType.U16), X(0x94), X(0x96)),
				"Every monster (252). Head named from FF3's MonsterParameter where FF4's values fit it (monsterId at 8 is what the engine looks up by); flags at 0x92 from mon::MonsterParameter::flag; x.. fields not yet named."),
			new PakChain(Monster, 1, "drops", "DropItemParameter", 18,
				Fields(F("droppingItemTableId", FieldType.S16), F("normalItem", FieldType.S16, 8)),
				"Drop tables (22): a table id and eight item ids. FF3's layout, same stride, and the values are item ids."),
			new PakChain(Monster, 2, "normalAttacks", "MonsterNormalAttackParameter", 28,
				Fields(
					F("0.effects.frameCounter", FieldType.S32), F("0.effects.type", FieldType.S16), F("0.effects.category", FieldType.S16),
					F("0.effects.member", FieldType.S16), F("0.effects.isLoop", FieldType.S8), F("0.effects.pad0", FieldType.U8),
					F("1.effects.frameCounter", FieldType.S32), F("1.effects.type", FieldType.S16), F("1.effects.category", FieldType.S16),
					F("1.effects.member", FieldType.S16), F("1.effects.isLoop", FieldType.S8), F("1.effects.pad0", FieldType.U8),
					F("damageMotion", FieldType.S16), F("damageValue", FieldType.S16)),
				"One per monster (252): the two effects a normal attack plays and its damage motion. FF3's layout, same stride (the engine steps this table by 0x1C)."),
			new PakChain(Monster, 4, "offsets", "MonsterOffsetParameter", 84,
				Fields(F("monsterId", FieldType.S16), Words(0x02, 0x54)),
				"Per-monster positions and camera offsets (251; MonsterManager::offset finds a row by the id at 0). FF3's version is 160 bytes, so its names do not carry over."),
			new PakChain(Monster, 7, "ai", "ai", 22,
				Fields(F("id", FieldType.S16), Words(0x02, 0x16)),
				"MonsterManager::ai: rows of 22 bytes found by the id at 0 (250)."),
			new PakChain(Monster, 8, "turnActions", "turnAction", 44,
				Fields(F("id", FieldType.S16), Words(0x02, 0x2C)),
				"MonsterManager::turnAction: rows of 44 bytes found by the id at 0 (189)."),
			new PakChain(Monster, 9, "actionConditions", "actionCondition", 12,
				Fields(F("id", FieldType.S16), Words(0x02, 0x0C)),
				"MonsterManager::actionCondition: rows of 12 bytes found by the id at 0 (189)."),
			new PakChain(Monster, 10, "counters", "counter", 14,
				Fields(F("id", FieldType.S16), Words(0x02, 0x0E)),
				"MonsterManager::counter: rows of 14 bytes found by the id at 0 (84)."),

			// ---- player.chaindata: 37 chains, most of them 1187 bytes and not fixed-stride -------
			new PakChain(Player, 33, "chain33", "ff4:chain33", 8, Fields(Words(0, 8)),
				"50 records of 8 bytes; fields not yet named."),
			new PakChain(Player, 34, "chain34", "ff4:chain34", 108, Fields(Words(0, 108)),
				"12 records of 108 bytes; fields not yet named."),

			// ---- <map>.pak: one record apiece, in MapParameterManager's order -------------------
			new PakChain(Map, 0, "encount", "world::MapParameterManager::encountParameter", 52, Fields(Words(0, 52)),
				"How dangerous the map is. One record; fields not yet named."),
			new PakChain(Map, 1, "landForm", "world::MapParameterManager::landFormParameter", 42, Fields(Words(0, 42)),
				"Terrain attributes. One record; fields not yet named."),
			new PakChain(Map, 2, "monsterParty", "world::MapParameterManager::monsterPartyParameter", 16, Fields(Words(0, 16)),
				"The monster groups this map can throw at you. One record; fields not yet named."),
			new PakChain(Map, 3, "environEffect", "world::MapParameterManager::environEffectParameter", 8, Fields(Words(0, 8)),
				"Environment effect. One record; fields not yet named."),
		};

		public static PakChain Find(string family, int index)
		{
			return Chains.FirstOrDefault(c => c.Family == family && c.Index == index);
		}

		/// <summary>The FF4 family for a table file, or null when it is not one FF4 has.</summary>
		public static string FamilyOf(string fileName, int chainCount)
		{
			string name = System.IO.Path.GetFileName(fileName);
			if (string.Equals(name, "item_parameter.pak", System.StringComparison.OrdinalIgnoreCase)) return Item;
			if (string.Equals(name, "monster.chaindata", System.StringComparison.OrdinalIgnoreCase)) return Monster;
			if (string.Equals(name, "player.chaindata", System.StringComparison.OrdinalIgnoreCase)) return Player;
			// A map's .pak is named after the map - d01_00.pak - and has four chains.
			// item_parameter.pak has four too, so the count alone would mistype it.
			return chainCount == 4 && System.Text.RegularExpressions.Regex.IsMatch(name, @"^[a-z]\d\d_\d\d(_[a-z0-9]+)?\.pak$", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
				? Map : null;
		}
	}
}
