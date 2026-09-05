// FF4 3D's tables into the unified shape. What is read, and the evidence for it:
//
//   player.chaindata (37 chains; pl::PlayerParty::load, levelParameter, normalMagic,
//   normalAttack in libff4.so): chain 0 the experience curve (99 u32); chains 2..16 one
//   growth table per PLAYER_TYPE, 99 rows of 12 bytes - u16 least and u16 most hp gained,
//   u16 mp (a total, tentatively), then five attribute bytes (absolute per level; Player::setParameter moves a
//   stat by new row minus old row) and one unnamed byte; chain 32 the spells, 32-byte
//   records with the id first; chain 1 the normal attacks, 20-byte records, id first.
//   item_parameter.pak (4 chains; itm::ItemManager and the editor's PakRecordsFf4):
//   consumables 48 bytes, weapons 88, armour 84, key items 32; id at 2, name id at 4,
//   caption id at 6, graph at 8, efficacy at 0x16, buy at 0x1C, price at 0x20; the
//   equipment half from 0x2C (see EquipStats).
//   babil_item.msd names the items by their name id.
//
// Character names and classes are not in these files by type; the list here is what the
// game is known to have, in the PLAYER_TYPES order the scripts imply (0 is Cecil: the new
// game adds member 0), and every entry is marked tentative until a file confirms it.

using System;
using System.Collections.Generic;
using FF3.Content;

namespace OpenFF.Data
{
	internal static class Ff4Tables
	{
		public const int CharacterTypes = 15;

		private static readonly (string Key, string Name, string Class)[] KnownCharacters =
		{
			("cecil", "Cecil", "Dark Knight"), ("kain", "Kain", "Dragoon"), ("rydia", "Rydia", "Summoner"),
			("tellah", "Tellah", "Sage"), ("edward", "Edward", "Bard"), ("rosa", "Rosa", "White Mage"),
			("yang", "Yang", "Monk"), ("palom", "Palom", "Black Mage"), ("porom", "Porom", "White Mage"),
			("cid", "Cid", "Engineer"), ("edge", "Edge", "Ninja"), ("rydia-adult", "Rydia", "Summoner"),
			("fusoya", "FuSoYa", "Lunarian"), ("cecil-paladin", "Cecil", "Paladin"), ("golbez", "Golbez", null),
		};

		public static GameTables Read(ContentChain chain)
		{
			GameTables tables = new GameTables { Game = "ff4" };
			ReadPlayers(chain, tables);
			ReadItems(chain, tables);
			ReadMonsters(chain, tables);
			ReadMonsterParties(chain, tables);
			return tables;
		}

		/// <summary>
		/// monster_party_table.bbd: 520 records of 140 bytes (mon::MonsterPartyManager::load divides
		/// by 0x8C; monsterParty(id) walks them comparing the s16 at 0). Then up to six slots of 20
		/// bytes from offset 4 - monster id s16, flag s16, x, y, z fx32 and a fourth fx32 word - the
		/// list ending at a -1 id; the tail holds words not yet named. Party 1 is two Goblins at x
		/// -37 and -15, z -50; the scripted battles 900..934 are here too.
		/// </summary>
		private static void ReadMonsterParties(ContentChain chain, GameTables tables)
		{
			if (!TableFiles.ReadAny(chain, "monster_party_table.bbd", out byte[] data))
			{
				tables.Notes.Add("monster_party_table.bbd not found");
				return;
			}
			const int stride = 140;
			for (int i = 0; i + stride <= data.Length; i += stride)
			{
				MonsterParty party = new MonsterParty { Id = ChainPack.S16(data, i), Flags = ChainPack.S16(data, i + 2) };
				for (int s = 0; s < 6; s++)
				{
					int at = i + 4 + 20 * s;
					int id = ChainPack.S16(data, at);
					if (id < 0) break;
					party.Slots.Add(new MonsterPartySlot
					{
						MonsterId = id,
						Flag = ChainPack.S16(data, at + 2),
						X = ChainPack.S32(data, at + 4) / 4096f,
						Y = ChainPack.S32(data, at + 8) / 4096f,
						Z = ChainPack.S32(data, at + 12) / 4096f,
						W = ChainPack.S32(data, at + 16) / 4096f,
					});
				}
				tables.MonsterParties.Add(party);
			}
		}

		/// <summary>
		/// monster.chaindata chain 0: 252 records of 152 bytes. The head is FF3's (nameId, textId,
		/// familyId, modelId, monsterId at 8, level, size, maxHp s32 at 0xC); five attribute bytes
		/// at 0x12; four (item, chance of 4096) drop pairs from 0x6C; experience s32 at 0x88 and
		/// gil at 0x8C, read off the records (they climb with the level; the last boss gives 12000
		/// and 100000). babil_battle.msd names them by name id.
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
			Dictionary<uint, string> names = TableFiles.ReadNames(chain, "babil_battle.msd", tables);
			int records = pack.Records(0, 152);
			for (int i = 0; i < records; i++)
			{
				byte[] r = pack.Record(0, 152, i);
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
					Stats = new Stats { Strength = r[0x12], Vitality = r[0x13], Agility = r[0x14], Intellect = r[0x15], Spirit = r[0x16] },
					Experience = ChainPack.S32(r, 0x88),
					Gil = ChainPack.S32(r, 0x8C),
					Raw = r,
				};
				for (int d = 0; d < 4; d++)
				{
					int item = ChainPack.S16(r, 0x6C + 4 * d), chance = ChainPack.S16(r, 0x6E + 4 * d);
					if (item > 0 && chance > 0) m.Drops.Add(new DropChance { ItemId = item, Chance = chance });
				}
				if (names != null && m.NameId > 0 && names.TryGetValue((uint)m.NameId, out string name)) m.Name = name;
				tables.Monsters.Add(m);
			}
		}

		private static void ReadPlayers(ContentChain chain, GameTables tables)
		{
			if (!TableFiles.ReadAny(chain, "player.chaindata", out byte[] data))
			{
				tables.Notes.Add("player.chaindata not found");
				return;
			}
			ChainPack pack;
			try { pack = ChainPack.Read(data); }
			catch (Exception ex) { tables.Notes.Add("player.chaindata: " + ex.Message); return; }
			if (pack.Count < 33)
			{
				tables.Notes.Add("player.chaindata has " + pack.Count + " chains, not FF4's 37");
				return;
			}
			int levels = pack.Size(0) / 4;
			tables.ExperienceToLevel = new int[levels];
			for (int i = 0; i < levels; i++) tables.ExperienceToLevel[i] = ChainPack.S32(pack.Data, pack.Offset(0) + 4 * i);

			for (int type = 0; type < CharacterTypes; type++)
			{
				int c = 2 + type;
				CharacterDefinition def = new CharacterDefinition
				{
					Id = type,
					Key = KnownCharacters[type].Key,
					Name = KnownCharacters[type].Name,
					ClassName = KnownCharacters[type].Class,
					NameIsTentative = true,
					FieldModel = "p" + type.ToString("00") + "_01",
					BattleModel = "b_p_player_" + type.ToString("00"),
				};
				if (c < pack.Count)
				{
					int rows = pack.Records(c, 12);
					def.Levels = new LevelRow[rows];
					for (int i = 0; i < rows; i++)
					{
						byte[] r = pack.Record(c, 12, i);
						def.Levels[i] = new LevelRow
						{
							Level = i + 1,
							HpGainMin = ChainPack.U16(r, 0),
							HpGainMax = ChainPack.U16(r, 2),
							Mp = ChainPack.U16(r, 4),
							Stats = new Stats { Strength = r[6], Agility = r[7], Vitality = r[8], Intellect = r[9], Spirit = r[10] },
							X0B = r[11],
						};
					}
				}
				tables.Characters.Add(def);
			}

			// babil_ability.msd names every ability, summon and spell under the ability's own id.
			Dictionary<uint, string> abilities = TableFiles.ReadNames(chain, "babil_ability.msd", tables);
			if (abilities != null)
			{
				foreach (KeyValuePair<uint, string> pair in abilities) tables.AbilityNames[(int)pair.Key] = pair.Value;
			}
			if (pack.Count > 32)
			{
				int spells = pack.Size(32) / 32;
				for (int i = 0; i < spells; i++)
				{
					byte[] r = pack.Record(32, 32, i);
					int id = ChainPack.S16(r, 0);
					tables.Spells.Add(new SpellDefinition { Id = id, Name = tables.AbilityName(id), Raw = r });
				}
			}
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
			if (pack.Count != 4)
			{
				tables.Notes.Add("item_parameter.pak has " + pack.Count + " chains, not FF4's 4");
				return;
			}
			Dictionary<uint, string> names = TableFiles.ReadNames(chain, "babil_item.msd", tables);
			(ItemKind Kind, int Stride)[] chains = { (ItemKind.Consumable, 48), (ItemKind.Weapon, 88), (ItemKind.Armour, 84), (ItemKind.KeyItem, 32) };
			for (int c = 0; c < 4; c++)
			{
				int stride = chains[c].Stride;
				int records = pack.Records(c, stride);
				for (int i = 0; i < records; i++)
				{
					byte[] r = pack.Record(c, stride, i);
					int id = ChainPack.S16(r, 2);
					if (id <= 0) continue;
					ItemDefinition item = new ItemDefinition
					{
						Id = id,
						Kind = chains[c].Kind,
						System = r[0],
						NameId = ChainPack.S16(r, 4),
						CaptionId = ChainPack.S16(r, 6),
						GraphId = ChainPack.S16(r, 8),
						EfficacyId = ChainPack.S16(r, 0x16),
						Raw = r,
					};
					if (stride > 0x24)
					{
						item.BuyPrice = ChainPack.S32(r, 0x1C);
						item.SellPrice = ChainPack.S32(r, 0x20);
					}
					if (chains[c].Kind == ItemKind.Weapon || chains[c].Kind == ItemKind.Armour)
					{
						item.Equip = new EquipStats
						{
							CanEquip = ChainPack.U32(r, 0x2C),
							Position = ChainPack.S16(r, 0x30),
							Attack = ChainPack.S16(r, 0x34),
							Hit = ChainPack.S16(r, 0x36),
							Defence = ChainPack.S16(r, 0x38),
							Evade = ChainPack.S16(r, 0x3A),
							MagicDefence = ChainPack.S16(r, 0x3C),
							MagicEvade = ChainPack.S16(r, 0x3E),
							Bonus = new Stats
							{
								Strength = ChainPack.S16(r, 0x44), Vitality = ChainPack.S16(r, 0x46), Agility = ChainPack.S16(r, 0x48),
								Intellect = ChainPack.S16(r, 0x4A), Spirit = ChainPack.S16(r, 0x4C),
							},
						};
					}
					if (names != null)
					{
						if (item.NameId > 0 && names.TryGetValue((uint)item.NameId, out string name)) item.Name = name;
						if (item.CaptionId > 0 && names.TryGetValue((uint)item.CaptionId, out string caption)) item.Caption = caption;
					}
					tables.Items.Add(item);
				}
			}
		}
	}
}
