// The unified game data for the editor: characters, jobs, spells, items, monsters, encounter
// groups and shops as OpenFF.Data reads them (Shared/Data), with named columns and the
// game's own names - the same objects the OpenFF client plays from. Read-only here: the
// raw records are edited under Tables; this view says what they mean. Built on the
// workspace's own content chain, so a saved override shows up on the next open.

using System;
using System.Collections.Generic;
using System.Linq;
using FF3.Content;
using OpenFF.Data;

namespace FF3.ContentTool.Editor
{
	internal static class GameData
	{
		/// <summary>The virtual documents under Game data, in the order they are listed.</summary>
		public static readonly string[] Pages = { "Characters", "Jobs", "Spells", "Items", "Monsters", "Encounter groups", "Shops", "Efficacies" };

		private static readonly Dictionary<Workspace, (GameTables Tables, int Version)> _cache = new Dictionary<Workspace, (GameTables, int)>();

		public static IEnumerable<string> PagesFor(Workspace workspace)
		{
			bool ff4 = workspace.Game == "ff4";
			foreach (string page in Pages)
			{
				if (page == "Jobs" && ff4) continue;
				if ((page == "Shops" || page == "Efficacies") && !ff4) continue;
				yield return page;
			}
		}

		public static GameTables Tables(Workspace workspace)
		{
			lock (_cache)
			{
				if (_cache.TryGetValue(workspace, out (GameTables Tables, int Version) have) && have.Version == workspace.Version) return have.Tables;
				ContentChain chain = ContentChain.Around(workspace.Source, workspace.ContentDirectory, new[] { workspace.OverrideDirectory });
				GameTables tables = TableFiles.Read(chain, workspace.Game);
				_cache[workspace] = (tables, workspace.Version);
				return tables;
			}
		}

		public static object Page(Workspace workspace, string name)
		{
			GameTables t = Tables(workspace);
			List<string> columns = new List<string>();
			List<object[]> rows = new List<object[]>();
			string note = null;
			switch (name)
			{
				case "Characters":
					columns.AddRange(new[] { "id", "key", "name", "class", "levels", "L1 hp", "L1 mp", "L1 str", "L1 agi", "L1 vit", "L1 int", "L1 spi", "L10 hp", "L30 hp", "L99 hp", "L99 mp", "field model", "battle model", "commands", "spells learnt" });
					foreach (CharacterDefinition c in t.Characters)
					{
						Stats s1 = c.StatsAt(1);
						rows.Add(new object[] { c.Id, c.Key, c.Name + (c.NameIsTentative ? "*" : ""), c.ClassName, c.Levels.Length,
							c.MaxHpAt(1), c.MaxMpAt(1), s1.Strength, s1.Agility, s1.Vitality, s1.Intellect, s1.Spirit,
							c.MaxHpAt(10), c.MaxHpAt(30), c.MaxHpAt(99), c.MaxMpAt(99), c.FieldModel, c.BattleModel,
							string.Join(" ", c.CommandsAt(99)), string.Join(", ", c.SpellsAt(99).Select(id => (t.Spell(id)?.Name ?? t.AbilityName(id) ?? id.ToString()) + " L" + c.Learning.First(l => l.Ability == id).Level)) });
					}
					note = t.Game == "ff4" ? "player.chaindata: growth per PLAYER_TYPE (chains 2..16), learn lists (17..31); names are tentative (*) until a file names them." : "FF3 grows by job - see Jobs; the four characters share the experience curve.";
					break;
				case "Jobs":
					columns.AddRange(new[] { "id", "key", "name", "curves (str vit agi int mind mp)", "L1 str", "L1 vit", "L1 agi", "L1 int", "L1 mind", "L30 str", "L30 vit", "L30 agi", "L30 int", "L30 mind", "L99 str", "L99 vit", "L99 agi", "L99 int", "L99 mind", "hp ~L30", "hp ~L99", "charges L30", "charges L99" });
					foreach (JobDefinition j in t.Jobs)
					{
						Stats a = j.StatsAt(1), b = j.StatsAt(30), c = j.StatsAt(99);
						LevelRow r30 = j.Levels.Length >= 30 ? j.Levels[29] : null, r99 = j.Levels.Length >= 99 ? j.Levels[98] : null;
						rows.Add(new object[] { j.Id, j.Key, j.Name + (j.NameIsTentative ? "*" : ""), string.Join(" ", j.GrowthTypes),
							a.Strength, a.Vitality, a.Agility, a.Intellect, a.Spirit, b.Strength, b.Vitality, b.Agility, b.Intellect, b.Spirit, c.Strength, c.Vitality, c.Agility, c.Intellect, c.Spirit,
							j.MaxHpAt(30), j.MaxHpAt(99), r30 != null && r30.Charges != null ? string.Join("/", r30.Charges) : "", r99 != null && r99.Charges != null ? string.Join("/", r99.Charges) : "" });
					}
					note = "player.chaindata chain 1 (growth types per job), chain 2 (the eight curves), chains 4..10 (charges); hit points are level + vitality (+ up to vitality/2) a level from 32.";
					break;
				case "Spells":
					columns.AddRange(new[] { "id", "name", "school", "level", "mp", "power", "hit %", "use", "group", "rank", "element", "inflicts", "grants", "targets", "in battle", "in menu", "can use" });
					foreach (SpellDefinition s in t.Spells)
					{
						rows.Add(new object[] { s.Id, s.Name, s.School.ToString(), s.Level, s.MpCost, s.Power, s.HitRate, s.UseKind, s.EffectGroup, s.EffectRank, "0x" + s.Element.ToString("X"), "0x" + s.Inflicts.ToString("X"), "0x" + s.Grants.ToString("X") + (s.Grants2 != 0 ? "/0x" + s.Grants2.ToString("X") : ""), "0x" + s.TargetFlags.ToString("X2"), s.UsableInBattle ? "yes" : "", s.UsableInMenu ? "yes" : "", s.CanUse != 0 ? "0x" + s.CanUse.ToString("X") : "" });
					}
					note = t.Game == "ff4" ? "magic_parameter.bbd: power, MP cost, hit rate, element and target bits; formulas in Docs/Client-Plan.md." : "item_parameter.pak chain 3 (52 bytes): level = magicClass + 1, power = aggressivity, hit = successProbability, use 0 attack 1 recovery 2 special 3 status.";
					break;
				case "Items":
					columns.AddRange(new[] { "id", "name", "kind", "buy", "sell", "attack", "hit", "defence", "evade", "m.def", "m.eva", "str", "agi", "vit", "int", "spi", "can equip", "efficacy", "caption" });
					foreach (ItemDefinition i in t.Items)
					{
						EquipStats e = i.Equip;
						rows.Add(new object[] { i.Id, i.Name, i.Kind.ToString(), i.BuyPrice, i.SellPrice, e?.Attack ?? 0, e?.Hit ?? 0, e?.Defence ?? 0, e?.Evade ?? 0, e?.MagicDefence ?? 0, e?.MagicEvade ?? 0,
							e?.Bonus.Strength ?? 0, e?.Bonus.Agility ?? 0, e?.Bonus.Vitality ?? 0, e?.Bonus.Intellect ?? 0, e?.Bonus.Spirit ?? 0, e != null ? "0x" + e.CanEquip.ToString("X") : "", i.EfficacyId, i.Caption });
					}
					note = "item_parameter.pak, named from the item text file; the raw records with every field are under Tables.";
					break;
				case "Monsters":
					columns.AddRange(new[] { "id", "name", "level", "hp", "attack", "hit", "defence", "evade", "m.def", "str", "agi", "vit", "int", "spi", "exp", "gil", "family", "model", "size", "drops" });
					foreach (MonsterDefinition m in t.Monsters)
					{
						rows.Add(new object[] { m.Id, m.Name, m.Level, m.MaxHp, m.Attack, m.Hit, m.Defence, m.Evade, m.MagicDefence, m.Stats.Strength, m.Stats.Agility, m.Stats.Vitality, m.Stats.Intellect, m.Stats.Spirit, m.Experience, m.Gil, m.Family, m.ModelId, m.Size,
							m.Drops.Count > 0 ? string.Join(", ", m.Drops.Select(d => (t.Item(d.ItemId)?.Name ?? d.ItemId.ToString()) + " " + (d.Chance * 100 / 4096) + "%")) : (m.DropTable != 0 ? "table " + m.DropTable + " (" + m.DropProbability + ")" : "") });
					}
					note = t.Game == "ff4" ? "monster.chaindata (152 bytes): attack at 0x20, defence block from 0x4C (tentative), drops from 0x6C, exp/gil at 0x88/0x8C." : "monster.chaindata (100 bytes): drop block at 0x54, gil at 0x58, exp at 0x5C.";
					break;
				case "Encounter groups":
					columns.AddRange(new[] { "id", "flags", "monsters", "placements" });
					foreach (MonsterParty p in t.MonsterParties)
					{
						rows.Add(new object[] { p.Id, p.Flags, string.Join(", ", p.Slots.Select(s => (t.Monster(s.MonsterId)?.Name ?? s.MonsterId.ToString()) + (s.Count > 1 ? " x" + s.Count : ""))),
							string.Join("; ", p.Slots.Select(s => s.X.ToString("0") + "," + s.Y.ToString("0") + "," + s.Z.ToString("0"))) });
					}
					note = t.Game == "ff4" ? "monster_party_table.bbd (140 bytes): up to six slots with positions; 900+ are the scripted battles." : "monster_party_table.bbd (18 bytes): four (monster, min, max) slots.";
					break;
				case "Efficacies":
					columns.AddRange(new[] { "id", "hp", "mp", "casts", "x10", "used by" });
					foreach (Efficacy e in t.Efficacies)
					{
						rows.Add(new object[] { e.Id, e.Hp, e.Mp, e.CastsAbility != 0 ? (t.Spell(e.CastsAbility)?.Name ?? t.AbilityName(e.CastsAbility) ?? e.CastsAbility.ToString()) : "", e.X10,
							string.Join(", ", t.Items.Where(i => i.EfficacyId == e.Id).Select(i => i.Name ?? i.Id.ToString())) });
					}
					note = "efficacy.beld: what an item does when used - hit and magic points back, or the ability it casts.";
					break;
				case "Shops":
					columns.AddRange(new[] { "row", "label", "keeper", "wares" });
					foreach (ShopRow s in Shops(workspace))
					{
						rows.Add(new object[] { s.Index, s.Label, s.Title, string.Join(", ", s.Items.Select(id => (t.Item(id)?.Name ?? id.ToString()) + " " + (t.Item(id)?.BuyPrice ?? 0))) });
					}
					note = "MENU/babil_shop.bbd, 124 bytes a row: the shop interiors' bootShop(row) opens these; row 0 is the debug shop.";
					break;
				default:
					return new { name, game = t.Game, columns, rows, note = "no such page" };
			}
			return new { name, game = t.Game, columns, rows, note, notes = t.Notes };
		}

		private sealed class ShopRow
		{
			public int Index;
			public string Label;
			public string Title;
			public List<int> Items = new List<int>();
		}

		private static IEnumerable<ShopRow> Shops(Workspace workspace)
		{
			byte[] data;
			try { data = workspace.Read("files/babil_shop.bbd"); }
			catch (Exception)
			{
				try { data = workspace.Read("babil_shop.bbd"); } catch (Exception) { yield break; }
			}
			GameTables t = Tables(workspace);
			Dictionary<uint, string> menu = null;
			try { menu = TableFiles.ReadNames(ContentChain.Around(workspace.Source, workspace.ContentDirectory, new[] { workspace.OverrideDirectory }), "babil_menu.msd", t); } catch (Exception) { }
			for (int at = 0; at + 124 <= data.Length; at += 124)
			{
				ShopRow row = new ShopRow { Index = at / 124 };
				int end = Array.IndexOf(data, (byte)0, at);
				row.Label = System.Text.Encoding.ASCII.GetString(data, at, Math.Max(0, Math.Min(32, (end < 0 ? at + 32 : end) - at)));
				int title = ChainPack.S32(data, at + 32);
				row.Title = menu != null && menu.TryGetValue((uint)title, out string s) ? s : title.ToString();
				for (int i = 0; i < 16; i++)
				{
					int id = ChainPack.S32(data, at + 60 + 4 * i);
					if (id > 0) row.Items.Add(id);
				}
				yield return row;
			}
		}
	}
}
