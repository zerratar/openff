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
			return tables;
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
