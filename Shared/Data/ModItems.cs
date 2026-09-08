// A mod's items: definitions as data (defs/items/<id>.json), composed into the game's own
// tables when the game reads them.
//
// The game knows an item by a record in item_parameter.pak (five chains: consumables,
// weapons, armour, magic, key items - a fixed layout per chain) and by its name and caption
// in eureka_item.msd. A mod's item is a definition with a base item (which record to start
// from: a Hi-Potion for a stronger potion, a Mythril Sword for a new sword), the name, the
// caption, the prices and any of the record's fields by name; when the client reads either
// file, every loaded mod's definitions are appended - one record per item after the base
// chain's, one message per name and caption - so the menus, the shops, the chests and
// Game.Items all see them as the game's own. Nothing changes for a game with no mod items:
// the bytes go through as they are.
//
//   {
//     "id": "hi-potion-plus",       the definition's own name: a file name, a word a mod uses
//     "number": 20001,              the game's item id for it - given once by Crystal, kept
//                                   forever (saves hold it)
//     "base": 5002,                 the item whose record it starts from
//     "name": "Hi-Potion+",
//     "caption": "Restores 999 HP.",
//     "buy": 1500, "sell": 750,
//     "fields": { "usedPower": 999 }   fields of the record by the game's own names
//   }
//
// Numbers: the game's ids run to the 9000s (FF3: 4001 magic, 5001 consumables, 6001
// weapons, 8001 armour, 9001 key items); a mod's start at 20001. Names take message ids
// from 31001, captions from 32001 - eureka_item.msd's own stop at 30217 and a record's
// name id is a signed short.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using OpenFF.Content;

namespace OpenFF.Data
{
	internal sealed class ModItem
	{
		public string Id;
		public int Number;
		public int Base;
		public string Name;
		public string Caption;
		public int? Buy;
		public int? Sell;
		public Dictionary<string, int> Fields = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
		/// <summary>Where it was read from, for a message.</summary>
		public string Source;

		public const int FirstNumber = 20001;
		public const int FirstNameId = 31001;
		public const int FirstCaptionId = 32001;

		public static ModItem Parse(string json, string source = null)
		{
			JsonNode node = JsonNode.Parse(json);
			if (node == null) return null;
			ModItem item = new ModItem
			{
				Id = node["id"]?.GetValue<string>(),
				Number = node["number"]?.GetValue<int>() ?? 0,
				Base = node["base"]?.GetValue<int>() ?? 0,
				Name = node["name"]?.GetValue<string>(),
				Caption = node["caption"]?.GetValue<string>(),
				Buy = node["buy"]?.GetValue<int>(),
				Sell = node["sell"]?.GetValue<int>(),
				Source = source
			};
			if (node["fields"] is JsonObject fields)
			{
				foreach (KeyValuePair<string, JsonNode> pair in fields)
				{
					if (pair.Value is JsonValue v && v.TryGetValue(out int n)) item.Fields[pair.Key] = n;
					else if (pair.Value is JsonValue s && s.TryGetValue(out string text) && int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int m)) item.Fields[pair.Key] = m;
				}
			}
			return item;
		}

		public string ToJson()
		{
			JsonObject node = new JsonObject
			{
				["id"] = Id,
				["number"] = Number,
				["base"] = Base,
				["name"] = Name ?? "",
				["caption"] = Caption ?? ""
			};
			if (Buy.HasValue) node["buy"] = Buy.Value;
			if (Sell.HasValue) node["sell"] = Sell.Value;
			if (Fields.Count > 0)
			{
				JsonObject fields = new JsonObject();
				foreach (KeyValuePair<string, int> pair in Fields.OrderBy(p => p.Key, StringComparer.OrdinalIgnoreCase)) fields[pair.Key] = pair.Value;
				node["fields"] = fields;
			}
			return node.ToJsonString(new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
		}
	}

	internal static class ModItems
	{
		/// <summary>The folder inside a mod (or a Crystal project) the definitions live in.</summary>
		public const string Folder = "defs/items";

		/// <summary>Every definition under the folders given (a mod's, several mods'), in file order; a broken file is a note, not a stop.</summary>
		public static List<ModItem> Load(IEnumerable<string> roots, List<string> notes = null)
		{
			List<ModItem> items = new List<ModItem>();
			foreach (string root in roots ?? Enumerable.Empty<string>())
			{
				string directory = Path.Combine(root, Folder.Replace('/', Path.DirectorySeparatorChar));
				if (!Directory.Exists(directory)) continue;
				foreach (string file in Directory.EnumerateFiles(directory, "*.json").OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
				{
					try
					{
						ModItem item = ModItem.Parse(File.ReadAllText(file), file);
						if (item == null) continue;
						if (string.IsNullOrWhiteSpace(item.Id)) item.Id = Path.GetFileNameWithoutExtension(file);
						if (item.Number <= 0 || item.Base <= 0) { notes?.Add(file + ": an item needs a number and a base"); continue; }
						items.Add(item);
					}
					catch (Exception ex) { notes?.Add(file + ": " + ex.Message); }
				}
			}
			return items;
		}

		/// <summary>The next number free after the game's own and every definition given.</summary>
		public static int NextNumber(IEnumerable<ModItem> items)
		{
			int max = ModItem.FirstNumber - 1;
			foreach (ModItem item in items ?? Enumerable.Empty<ModItem>()) if (item.Number > max) max = item.Number;
			return max + 1;
		}

		// FF3's item_parameter.pak: the chains and their strides (itm.ItemManager.load), and the
		// fields by the game's own names (each record type's parse(ArrayReader)). Every chain
		// shares the head: system u8 @0, itemId s16 @2, nameId @4, captionId @6, graphId @8,
		// five stat bytes @0xA..0xE, weight @0xF, useBattle @0x10, useField @0x11, allTarget
		// @0x12, useItemId s16 @0x14, targetPossible @0x16, targetPosition @0x18; buy s32 @0x1C
		// and price (sell) s32 @0x20 on all but key items.
		private static readonly int[] Strides = { 44, 56, 60, 52, 28 };
		public static readonly string[] ChainNames = { "consumable", "weapon", "armour", "magic", "key item" };

		private sealed class Field { public int Offset; public int Size; public bool Signed; public Field(int o, int s, bool signed = true) { Offset = o; Size = s; Signed = signed; } }

		private static readonly Dictionary<string, Field> Head = new Dictionary<string, Field>(StringComparer.OrdinalIgnoreCase)
		{
			["system"] = new Field(0, 1, false), ["itemId"] = new Field(2, 2), ["nameId"] = new Field(4, 2), ["captionId"] = new Field(6, 2), ["graphId"] = new Field(8, 2),
			["strength"] = new Field(0xA, 1, false), ["vitality"] = new Field(0xB, 1, false), ["dexterity"] = new Field(0xC, 1, false), ["intellect"] = new Field(0xD, 1, false), ["mind"] = new Field(0xE, 1, false),
			["weight"] = new Field(0xF, 1, false), ["useBattle"] = new Field(0x10, 1, false), ["useField"] = new Field(0x11, 1, false), ["allTarget"] = new Field(0x12, 1, false),
			["useItemId"] = new Field(0x14, 2), ["targetPossible"] = new Field(0x16, 2), ["targetPosition"] = new Field(0x18, 2),
		};
		private static readonly Dictionary<string, Field> Priced = new Dictionary<string, Field>(StringComparer.OrdinalIgnoreCase) { ["buy"] = new Field(0x1C, 4), ["price"] = new Field(0x20, 4) };
		private static readonly Dictionary<string, Field>[] PerChain =
		{
			// consumables (itm.ConsumptionParameter)
			new Dictionary<string, Field>(StringComparer.OrdinalIgnoreCase) { ["usedPower"] = new Field(0x24, 2), ["itemType"] = new Field(0x26, 2), ["changeCondition"] = new Field(0x28, 2) },
			// weapons (itm.WeaponParameter)
			new Dictionary<string, Field>(StringComparer.OrdinalIgnoreCase) { ["equipJob"] = new Field(0x24, 4, false), ["aggressivity"] = new Field(0x28, 2), ["hitProbability"] = new Field(0x2A, 1, false), ["optionProbability"] = new Field(0x2B, 1, false), ["optionMagicItemId"] = new Field(0x2C, 2), ["armsAttribute"] = new Field(0x2E, 2), ["atckType"] = new Field(0x30, 2), ["atckOption"] = new Field(0x32, 2), ["equipOption"] = new Field(0x34, 2) },
			// armour (itm.ProtectionParameter)
			new Dictionary<string, Field>(StringComparer.OrdinalIgnoreCase) { ["equipJob"] = new Field(0x24, 4, false), ["phylacticPower"] = new Field(0x28, 2), ["magicPhylacticPower"] = new Field(0x2A, 2), ["avoidanceProbability"] = new Field(0x2C, 1, false), ["magicAvoidanceProbability"] = new Field(0x2D, 1, false) },
			// magic (itm.MagicParameter)
			new Dictionary<string, Field>(StringComparer.OrdinalIgnoreCase) { ["equipJob"] = new Field(0x24, 4, false), ["magicClass"] = new Field(0x28, 1, false), ["aggressivity"] = new Field(0x2A, 2), ["successProbability"] = new Field(0x2C, 1, false), ["magicUseKind"] = new Field(0x2D, 1, false), ["magicType"] = new Field(0x2E, 2), ["changeCondition"] = new Field(0x30, 2), ["calculate"] = new Field(0x32, 1, false), ["reflect"] = new Field(0x33, 1, false) },
			// key items (itm.ImportantParameter): the head alone
			new Dictionary<string, Field>(StringComparer.OrdinalIgnoreCase),
		};

		/// <summary>The fields a definition may set for a chain, by the game's names, in record order.</summary>
		public static IEnumerable<string> FieldNames(int chain)
		{
			foreach (string name in Head.Keys) if (name != "itemId" && name != "nameId" && name != "captionId") yield return name;
			if (chain != 4) foreach (string name in Priced.Keys) yield return name;
			if (chain >= 0 && chain < PerChain.Length) foreach (string name in PerChain[chain].Keys) yield return name;
		}

		/// <summary>Which chain holds an item id in the pak; -1 when none does.</summary>
		public static int ChainOf(ChainPack pack, int itemId, out int index)
		{
			index = -1;
			if (pack == null || pack.Count != 5) return -1;
			for (int c = 0; c < 5; c++)
			{
				int records = pack.Records(c, Strides[c]);
				for (int i = 0; i < records; i++)
				{
					if (ChainPack.S16(pack.Record(c, Strides[c], i), 2) == itemId) { index = i; return c; }
				}
			}
			return -1;
		}

		/// <summary>The bytes of a record in the pak, as a definition starts from them.</summary>
		public static byte[] RecordOf(ChainPack pack, int itemId, out int chain)
		{
			chain = ChainOf(pack, itemId, out int index);
			return chain < 0 ? null : pack.Record(chain, Strides[chain], index);
		}

		/// <summary>A definition's record: the base's bytes with the id, the names, the prices and the fields written in.</summary>
		public static byte[] Record(ChainPack pack, ModItem item, int nameId, int captionId, out int chain, List<string> notes)
		{
			byte[] r = RecordOf(pack, item.Base, out chain);
			if (r == null) { notes?.Add(item.Id + ": no item " + item.Base + " to start from"); return null; }
			byte[] record = (byte[])r.Clone();
			Put(record, Head["itemId"], item.Number);
			Put(record, Head["nameId"], nameId);
			Put(record, Head["captionId"], captionId);
			if (chain != 4)
			{
				if (item.Buy.HasValue) Put(record, Priced["buy"], item.Buy.Value);
				if (item.Sell.HasValue) Put(record, Priced["price"], item.Sell.Value);
			}
			foreach (KeyValuePair<string, int> pair in item.Fields)
			{
				Field field;
				if (Head.TryGetValue(pair.Key, out field) || (chain != 4 && Priced.TryGetValue(pair.Key, out field)) || PerChain[chain].TryGetValue(pair.Key, out field))
				{
					if (string.Equals(pair.Key, "itemId", StringComparison.OrdinalIgnoreCase) || string.Equals(pair.Key, "nameId", StringComparison.OrdinalIgnoreCase) || string.Equals(pair.Key, "captionId", StringComparison.OrdinalIgnoreCase)) continue;
					Put(record, field, pair.Value);
				}
				else notes?.Add(item.Id + ": no field '" + pair.Key + "' on a " + ChainNames[chain]);
			}
			return record;
		}

		private static void Put(byte[] record, Field field, int value)
		{
			switch (field.Size)
			{
				case 1: record[field.Offset] = (byte)value; break;
				case 2: record[field.Offset] = (byte)value; record[field.Offset + 1] = (byte)(value >> 8); break;
				default: for (int i = 0; i < 4; i++) record[field.Offset + i] = (byte)(value >> (8 * i)); break;
			}
		}

		/// <summary>A field's value in a record, by name; null for a name the chain has not.</summary>
		public static int? Get(byte[] record, int chain, string name)
		{
			Field field;
			if (!(Head.TryGetValue(name, out field) || (chain != 4 && Priced.TryGetValue(name, out field)) || (chain >= 0 && chain < PerChain.Length && PerChain[chain].TryGetValue(name, out field)))) return null;
			if (field.Offset + field.Size > record.Length) return null;
			switch (field.Size)
			{
				case 1: return field.Signed ? (sbyte)record[field.Offset] : record[field.Offset];
				case 2: return field.Signed ? (int)ChainPack.S16(record, field.Offset) : (ushort)ChainPack.S16(record, field.Offset);
				default: return (int)ChainPack.U32(record, field.Offset);
			}
		}

		/// <summary>Message ids for the k-th definition's name and caption.</summary>
		public static void TextIds(int k, out int nameId, out int captionId) { nameId = ModItem.FirstNameId + k; captionId = ModItem.FirstCaptionId + k; }

		/// <summary>
		/// item_parameter.pak with every definition's record appended to its chain. The pak is
		/// rebuilt: the chains keep their order, the header's offsets and sizes follow.
		/// </summary>
		public static byte[] ComposePak(byte[] pak, IReadOnlyList<ModItem> items, List<string> notes = null)
		{
			if (pak == null || items == null || items.Count == 0) return pak;
			ChainPack pack;
			try { pack = ChainPack.Read(pak); }
			catch (Exception ex) { notes?.Add("item_parameter.pak: " + ex.Message); return pak; }
			if (pack.Count != 5) { notes?.Add("item_parameter.pak has " + pack.Count + " chains, not 5: no mod items"); return pak; }
			List<byte[]>[] extra = new List<byte[]>[5];
			for (int c = 0; c < 5; c++) extra[c] = new List<byte[]>();
			for (int k = 0; k < items.Count; k++)
			{
				TextIds(k, out int nameId, out int captionId);
				byte[] record = Record(pack, items[k], nameId, captionId, out int chain, notes);
				if (record != null) extra[chain].Add(record);
			}
			// The header (16 bytes, the count at 0) and the chain table as they were, the chains
			// laid out again after it with the new records at each one's end. Square's chains
			// are sometimes two bytes short of their last record: the appended ones start at a
			// whole stride so the game's fixed-step count stays right.
			using (MemoryStream out_ = new MemoryStream())
			{
				byte[] header = new byte[16 + 8 * 5];
				Array.Copy(pak, header, Math.Min(16, pak.Length));
				out_.Write(header, 0, header.Length);
				int[] offsets = new int[5], sizes = new int[5];
				for (int c = 0; c < 5; c++)
				{
					int stride = Strides[c];
					int records = pack.Records(c, stride);
					offsets[c] = (int)out_.Position;
					for (int i = 0; i < records; i++) { byte[] r = pack.Record(c, stride, i); out_.Write(r, 0, r.Length); }
					foreach (byte[] r in extra[c]) out_.Write(r, 0, r.Length);
					sizes[c] = (int)out_.Position - offsets[c];
					// Four-byte alignment between chains, as the files have.
					while (out_.Position % 4 != 0) out_.WriteByte(0);
				}
				byte[] result = out_.ToArray();
				for (int c = 0; c < 5; c++)
				{
					Array.Copy(BitConverter.GetBytes(offsets[c]), 0, result, 16 + 8 * c, 4);
					Array.Copy(BitConverter.GetBytes(sizes[c]), 0, result, 20 + 8 * c, 4);
				}
				return result;
			}
		}

		/// <summary>eureka_item.msd with a name and a caption message per definition appended.</summary>
		public static byte[] ComposeMsd(byte[] msd, IReadOnlyList<ModItem> items, List<string> notes = null)
		{
			if (msd == null || items == null || items.Count == 0) return msd;
			MsdFile file;
			try { file = Msd.Read(msd); }
			catch (Exception ex) { notes?.Add("eureka_item.msd: " + ex.Message); return msd; }
			HashSet<uint> have = new HashSet<uint>(file.Messages.Select(m => m.Id));
			for (int k = 0; k < items.Count; k++)
			{
				TextIds(k, out int nameId, out int captionId);
				if (!have.Contains((uint)nameId)) file.Messages.Add(new MsdMessage { Id = (uint)nameId, Pages = new List<string> { items[k].Name ?? items[k].Id ?? "?" } });
				if (!have.Contains((uint)captionId)) file.Messages.Add(new MsdMessage { Id = (uint)captionId, Pages = new List<string> { items[k].Caption ?? "" } });
			}
			return Msd.Write(file);
		}

		/// <summary>Whether a content name is one of the two files the definitions compose into.</summary>
		public static bool IsPak(string name) => string.Equals(Path.GetFileName(name ?? ""), "item_parameter.pak", StringComparison.OrdinalIgnoreCase);
		public static bool IsMsd(string name) => string.Equals(Path.GetFileName(name ?? ""), "eureka_item.msd", StringComparison.OrdinalIgnoreCase);
	}
}
