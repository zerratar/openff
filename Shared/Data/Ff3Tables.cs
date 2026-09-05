// FF3's tables into the unified shape, from the same files the decompiled game reads
// (itm.ItemManager, pl.PlayerParty) with the field offsets the editor's PakRecords lists:
//
//   item_parameter.pak, 5 chains: consumables 44 bytes, weapons 56, armour 60, magic 52,
//   key items 28. Every record starts with system u8, pad, itemId s16 @2, nameId @4,
//   captionId @6, graphId @8, five attribute bytes @0xA (strength, vitality, dexterity,
//   intellect, mind - bonuses while worn), weight @0xF, useBattle/useField/allTarget,
//   useItemId s16 @0x14, target words; buy s32 @0x1C and price s32 @0x20 on all but key
//   items. Weapons: equipJob u32 @0x24, aggressivity s16 @0x28, hitProbability u8 @0x2A.
//   Armour: equipJob @0x24, phylacticPower s16 @0x28, magicPhylacticPower @0x2A,
//   avoidanceProbability u8 @0x2C, magicAvoidanceProbability u8 @0x2D.
//   eureka_item.msd names them by name id.
//   player.chaindata: chain 0 the experience curve (99 u32). FF3 grows by job, not by
//   character (GrowUp per job); the four characters therefore carry no growth table here yet.

using System;
using System.Collections.Generic;
using FF3.Content;

namespace OpenFF.Data
{
	internal static class Ff3Tables
	{
		private static readonly (string Key, string Name)[] Characters =
		{
			("luneth", "Luneth"), ("arc", "Arc"), ("refia", "Refia"), ("ingus", "Ingus"),
		};

		public static GameTables Read(ContentChain chain)
		{
			GameTables tables = new GameTables { Game = "ff3" };
			ReadPlayers(chain, tables);
			ReadItems(chain, tables);
			ReadMonsters(chain, tables);
			ReadMonsterParties(chain, tables);
			return tables;
		}

		/// <summary>monster_party_table.bbd: 18-byte records - party id s16, then four (monster id s16, min u8, max u8).</summary>
		private static void ReadMonsterParties(ContentChain chain, GameTables tables)
		{
			if (!TableFiles.ReadAny(chain, "monster_party_table.bbd", out byte[] data))
			{
				tables.Notes.Add("monster_party_table.bbd not found");
				return;
			}
			for (int i = 0; i + 18 <= data.Length; i += 18)
			{
				MonsterParty party = new MonsterParty { Id = ChainPack.S16(data, i) };
				for (int s = 0; s < 4; s++)
				{
					int id = ChainPack.S16(data, i + 2 + 4 * s);
					int min = data[i + 4 + 4 * s], max = data[i + 5 + 4 * s];
					if (id < 0 || max == 0) continue;
					party.Slots.Add(new MonsterPartySlot { MonsterId = id, Count = max, X = (s - 1.5f) * 12f });
				}
				tables.MonsterParties.Add(party);
			}
		}

		/// <summary>
		/// monster.chaindata chain 0: 255 records of 100 bytes (mon.MonsterParameter.parse): nameId,
		/// textId, familyId, modelId, monsterId at 8, level, size, maxHp s32 at 0xC, then the body,
		/// attack and defence blocks (not read yet), and DroppingDataParameter at 0x54: probability
		/// s16, table id s16, gold s32 at 0x58, exp s32 at 0x5C (Goblin: 10 gil, 1 exp).
		/// eureka_battle.msd names them by name id.
		/// </summary>
		private static void ReadMonsters(ContentChain chain, GameTables tables)
		{
			if (!TableFiles.ReadAny(chain, "monster.chaindata", out byte[] data))
			{
				tables.Notes.Add("monster.chaindata not found");
				return;
			}
			ChainPack pack;
			try { pack = ChainPack.Read(data); }
			catch (Exception ex) { tables.Notes.Add("monster.chaindata: " + ex.Message); return; }
			Dictionary<uint, string> names = TableFiles.ReadNames(chain, "eureka_battle.msd", tables);
			int records = pack.Size(0) / 100;
			for (int i = 0; i < records; i++)
			{
				byte[] r = pack.Record(0, 100, i);
				MonsterDefinition m = new MonsterDefinition
				{
					Id = ChainPack.S16(r, 8),
					NameId = ChainPack.S16(r, 0),
					TextId = ChainPack.S16(r, 2),
					Family = ChainPack.S16(r, 4),
					ModelId = ChainPack.S16(r, 6),
					Level = r[0xA],
					Size = r[0xB],
					MaxHp = ChainPack.S32(r, 0xC),
					DropProbability = ChainPack.S16(r, 0x54),
					DropTable = ChainPack.S16(r, 0x56),
					Gil = ChainPack.S32(r, 0x58),
					Experience = ChainPack.S32(r, 0x5C),
					Raw = r,
				};
				if (names != null && m.NameId > 0 && names.TryGetValue((uint)m.NameId, out string name)) m.Name = name;
				tables.Monsters.Add(m);
			}
		}

		private static void ReadPlayers(ContentChain chain, GameTables tables)
		{
			for (int i = 0; i < Characters.Length; i++)
			{
				tables.Characters.Add(new CharacterDefinition { Id = i, Key = Characters[i].Key, Name = Characters[i].Name });
			}
			if (!TableFiles.ReadAny(chain, "player.chaindata", out byte[] data))
			{
				tables.Notes.Add("player.chaindata not found");
				return;
			}
			try
			{
				ChainPack pack = ChainPack.Read(data);
				int levels = pack.Size(0) / 4;
				tables.ExperienceToLevel = new int[levels];
				for (int i = 0; i < levels; i++) tables.ExperienceToLevel[i] = ChainPack.S32(pack.Data, pack.Offset(0) + 4 * i);
			}
			catch (Exception ex)
			{
				tables.Notes.Add("player.chaindata: " + ex.Message);
			}
			tables.Notes.Add("FF3 grows by job; the characters carry no growth table yet");
		}

		private static void ReadItems(ContentChain chain, GameTables tables)
		{
			if (!TableFiles.ReadAny(chain, "item_parameter.pak", out byte[] data))
			{
				tables.Notes.Add("item_parameter.pak not found");
				return;
			}
			ChainPack pack;
			try { pack = ChainPack.Read(data); }
			catch (Exception ex) { tables.Notes.Add("item_parameter.pak: " + ex.Message); return; }
			if (pack.Count != 5)
			{
				tables.Notes.Add("item_parameter.pak has " + pack.Count + " chains, not FF3's 5");
				return;
			}
			Dictionary<uint, string> names = TableFiles.ReadNames(chain, "eureka_item.msd", tables);
			(ItemKind Kind, int Stride)[] chains = { (ItemKind.Consumable, 44), (ItemKind.Weapon, 56), (ItemKind.Armour, 60), (ItemKind.Spell, 52), (ItemKind.KeyItem, 28) };
			for (int c = 0; c < 5; c++)
			{
				int stride = chains[c].Stride;
				int records = pack.Size(c) / stride;
				for (int i = 0; i < records; i++)
				{
					byte[] r = pack.Record(c, stride, i);
					int id = ChainPack.S16(r, 2);
					if (id <= 0) continue;
					int nameId = ChainPack.S16(r, 4), captionId = ChainPack.S16(r, 6);
					string name = names != null && nameId > 0 && names.TryGetValue((uint)nameId, out string n) ? n : null;
					string caption = names != null && captionId > 0 && names.TryGetValue((uint)captionId, out string cap) ? cap : null;
					if (chains[c].Kind == ItemKind.Spell)
					{
						tables.Spells.Add(new SpellDefinition { Id = id, Name = name, Raw = r });
						continue;
					}
					ItemDefinition item = new ItemDefinition
					{
						Id = id,
						Kind = chains[c].Kind,
						System = r[0],
						NameId = nameId,
						Name = name,
						CaptionId = captionId,
						Caption = caption,
						GraphId = ChainPack.S16(r, 8),
						EfficacyId = ChainPack.S16(r, 0x14),
						Raw = r,
					};
					if (stride >= 0x24)
					{
						item.BuyPrice = ChainPack.S32(r, 0x1C);
						item.SellPrice = ChainPack.S32(r, 0x20);
					}
					if (chains[c].Kind == ItemKind.Weapon || chains[c].Kind == ItemKind.Armour)
					{
						item.Equip = new EquipStats
						{
							CanEquip = ChainPack.U32(r, 0x24),
							Bonus = new Stats { Strength = r[0xA], Vitality = r[0xB], Agility = r[0xC], Intellect = r[0xD], Spirit = r[0xE] },
						};
						if (chains[c].Kind == ItemKind.Weapon)
						{
							item.Equip.Attack = ChainPack.S16(r, 0x28);
							item.Equip.Hit = r[0x2A];
						}
						else
						{
							item.Equip.Defence = ChainPack.S16(r, 0x28);
							item.Equip.MagicDefence = ChainPack.S16(r, 0x2A);
							item.Equip.Evade = r[0x2C];
							item.Equip.MagicEvade = r[0x2D];
						}
					}
					tables.Items.Add(item);
				}
			}
		}
	}
}
