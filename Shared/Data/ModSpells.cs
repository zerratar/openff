// The mods' spell looks (defs/spells/*.json): which of the game's effects a spell plays, and
// its sound - composed into player.chaindata's chain 12 (pl.PlayerParty.normalMagic) as the
// game reads it. The battle loads a spell's effect pack by the category its record names
// (btl.PlayerTurnSystem, BattleEffect.addEfp), so a record changed is all it takes: Fire can
// play Flare's effect, or one of a summon's.
//
//   { "spell": "Fire", "effect": "Flare" }                          Flare's effect, its timing too
//   { "spell": 4101, "effect": "game:389/1", "frame": 60 }          a pack and member by number
//   { "spell": "Fire", "effect": "Firaga", "sound": "Thundaga" }    a sound of another spell's
//   { "spell": "Cure", "cast": "black" }                            the caster's glow of another school
//
// A record is 32 bytes: magicId s16 @0, offset s16 @2, the effect @4 (frameCounter s32, type s16
// @8, category s16 @10, member s16 @12, loop u8 @14, pad), the sound @16 in the same shape (its
// category @22, member @24), motionStartFrame s16 @28, effectPlayFrame s16 @30 (how long the effect runs before the damage shows). A mod's own magic
// item (defs/items with a magic base) has none of its own: it gets a copy of its base's, so
// the battle finds one - and a definition here may change it like any other.
//
// The cast - the glow on the caster as a spell begins - is not in the record: the battle picks it by
// the spell's school (btl.TurnSystem.magicStartEffect: 407 black, 408 white, 243 summon). "cast"
// names another school's, "none", or a pack as game:<pack>; the client answers for it there.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;

namespace OpenFF.Data
{
	public sealed class ModSpell
	{
		public string Id;
		/// <summary>The spell: its number (4101 Fire, a mod's 20001...), or its name as written.</summary>
		public string Spell;
		/// <summary>Another spell (its effect, timing and place), or "game:category/member".</summary>
		public string Effect;
		/// <summary>Another spell's sound, or "game:category/member".</summary>
		public string Sound;
		/// <summary>effectPlayFrame: how many frames the effect runs before the damage shows.</summary>
		public int? Frame;
		/// <summary>The caster's glow as the spell begins: "black", "white", "summon", "none" or "game:pack".</summary>
		public string Cast;
		public string Source;

		public static ModSpell Parse(string json, string source = null)
		{
			JsonNode node = JsonNode.Parse(json);
			if (node == null) return null;
			return new ModSpell
			{
				Id = node["id"]?.GetValue<string>(),
				Spell = Text(node["spell"]),
				Effect = Text(node["effect"]),
				Sound = Text(node["sound"]),
				Frame = node["frame"] is JsonValue f && f.TryGetValue(out int frame) ? frame : (int?)null,
				Cast = Text(node["cast"]),
				Source = source,
			};
		}

		/// <summary>A value that may be written as a number or a string, as a string.</summary>
		private static string Text(JsonNode node)
		{
			if (node is not JsonValue v) return null;
			if (v.TryGetValue(out string s)) return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
			if (v.TryGetValue(out int n)) return n.ToString(CultureInfo.InvariantCulture);
			return null;
		}

		/// <summary>The pack a cast names: a school's (407 black, 408 white, 243 summon), -1 for "none", or game:pack; null when it names none.</summary>
		public static int? CastPack(string text)
		{
			switch (text?.Trim().ToLowerInvariant())
			{
				case null: return null;
				case "black": return 407;
				case "white": return 408;
				case "summon": return 243;
				case "none": return -1;
			}
			string t = text.Trim();
			return t.StartsWith("game:", StringComparison.OrdinalIgnoreCase)
				&& int.TryParse(t.Substring(5), NumberStyles.Integer, CultureInfo.InvariantCulture, out int pack) && pack >= 0 ? pack : (int?)null;
		}

		/// <summary>"game:389/1" as a category and a member; false for anything else.</summary>
		public static bool GameEffect(string text, out int category, out int member)
		{
			category = member = 0;
			if (text == null || !text.StartsWith("game:", StringComparison.OrdinalIgnoreCase)) return false;
			string[] parts = text.Substring(5).Split('/');
			return parts.Length == 2
				&& int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out category)
				&& int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out member);
		}
	}

	public static class ModSpells
	{
		public const string Folder = "defs/spells";
		public const int Chain = 12;
		public const int Stride = 32;

		public static bool IsChaindata(string name) => string.Equals(Path.GetFileName(name ?? ""), "player.chaindata", StringComparison.OrdinalIgnoreCase);

		public static List<ModSpell> Load(IEnumerable<string> roots, List<string> notes = null)
		{
			List<ModSpell> spells = new List<ModSpell>();
			foreach (string root in roots ?? Enumerable.Empty<string>())
			{
				string directory = Path.Combine(root, Folder.Replace('/', Path.DirectorySeparatorChar));
				if (!Directory.Exists(directory)) continue;
				foreach (string file in Directory.EnumerateFiles(directory, "*.json").OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
				{
					try
					{
						ModSpell spell = ModSpell.Parse(File.ReadAllText(file), file);
						if (spell == null) continue;
						if (string.IsNullOrWhiteSpace(spell.Id)) spell.Id = Path.GetFileNameWithoutExtension(file);
						if (spell.Spell == null) { notes?.Add(file + ": a spell look needs \"spell\""); continue; }
						if (spell.Effect == null && spell.Sound == null && spell.Frame == null && spell.Cast == null) { notes?.Add(file + ": nothing to change (effect, sound, frame or cast)"); continue; }
						if (spell.Cast != null && ModSpell.CastPack(spell.Cast) == null) notes?.Add(file + ": no cast '" + spell.Cast + "' (black, white, summon, none or game:pack)");
						spells.Add(spell);
					}
					catch (Exception ex) { notes?.Add(file + ": " + ex.Message); }
				}
			}
			return spells;
		}

		/// <summary>
		/// player.chaindata with the looks put on: each definition's spell's record changed, and a
		/// record per mod magic item that has none (a copy of its base's). <paramref name="spellId"/>
		/// turns a name or a number into a spell's id (null when there is none). The file as it was
		/// when nothing changes.
		/// </summary>
		public static byte[] Compose(byte[] data, IReadOnlyList<ModSpell> spells, IReadOnlyList<(int Number, int Base)> modMagic,
			Func<string, int?> spellId, List<string> notes = null)
		{
			if (data == null || ((spells == null || spells.Count == 0) && (modMagic == null || modMagic.Count == 0))) return data;
			ChainPack pack;
			try { pack = ChainPack.Read(data); }
			catch (Exception ex) { notes?.Add("player.chaindata: " + ex.Message); return data; }
			if (pack.Count <= Chain) { notes?.Add("player.chaindata has " + pack.Count + " chains: no spell looks"); return data; }

			// The records by spell, as the game has them.
			int count = pack.Records(Chain, Stride);
			List<byte[]> records = new List<byte[]>();
			Dictionary<int, byte[]> byId = new Dictionary<int, byte[]>();
			for (int i = 0; i < count; i++)
			{
				byte[] r = pack.Record(Chain, Stride, i);
				records.Add(r);
				byId[ChainPack.S16(r, 0)] = r;
			}
			List<byte[]> added = new List<byte[]>();
			foreach ((int number, int baseId) in modMagic ?? Array.Empty<(int, int)>())
			{
				if (byId.ContainsKey(number) || !byId.TryGetValue(baseId, out byte[] from)) continue;
				byte[] copy = (byte[])from.Clone();
				Put16(copy, 0, number);
				byId[number] = copy;
				added.Add(copy);
			}

			bool changed = added.Count > 0;
			foreach (ModSpell s in spells ?? Array.Empty<ModSpell>())
			{
				if (s.Effect == null && s.Sound == null && s.Frame == null) continue;   // a cast alone: not in the record
				int? id = spellId(s.Spell);
				if (id == null || !byId.TryGetValue(id.Value, out byte[] record)) { notes?.Add(s.Id + ": no spell '" + s.Spell + "'"); continue; }
				if (s.Effect != null)
				{
					if (ModSpell.GameEffect(s.Effect, out int category, out int member))
					{
						// A pack and member by number: the effect alone; its start frame and loop as the spell had them.
						Put16(record, 10, category);
						Put16(record, 12, member);
					}
					else if (spellId(s.Effect) is int like && byId.TryGetValue(like, out byte[] other))
					{
						// Another spell's: its effect whole, where it plays (offset) and how long it runs.
						Array.Copy(other, 2, record, 2, 2);
						Array.Copy(other, 4, record, 4, 12);
						Array.Copy(other, 30, record, 30, 2);
					}
					else { notes?.Add(s.Id + ": no effect '" + s.Effect + "' (a spell, or game:category/member)"); continue; }
				}
				if (s.Sound != null)
				{
					if (ModSpell.GameEffect(s.Sound, out int category, out int member)) { Put16(record, 22, category); Put16(record, 24, member); }
					else if (spellId(s.Sound) is int like && byId.TryGetValue(like, out byte[] other)) Array.Copy(other, 16, record, 16, 12);
					else notes?.Add(s.Id + ": no sound '" + s.Sound + "'");
				}
				if (s.Frame is int frame) Put16(record, 30, frame);
				changed = true;
			}
			if (!changed) return data;

			// The records changed in place, the added ones after them.
			byte[] result = (byte[])data.Clone();
			for (int i = 0; i < records.Count; i++)
			{
				int at = pack.Offset(Chain) + Stride * i;
				int take = Math.Min(Stride, pack.Offset(Chain) + pack.Size(Chain) - at);
				if (take > 0) Array.Copy(records[i], 0, result, at, take);
			}
			if (added.Count == 0) return result;
			return ModMonsters.Append(ChainPack.Read(result), new Dictionary<int, (int, List<byte[]>)> { [Chain] = (Stride, added) });
		}

		private static void Put16(byte[] record, int at, int value)
		{
			short v = (short)value;
			record[at] = (byte)(v & 0xFF);
			record[at + 1] = (byte)((v >> 8) & 0xFF);
		}
	}
}
