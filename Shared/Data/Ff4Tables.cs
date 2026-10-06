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
//   magic_parameter.bbd (common::BabilMagicParameterManager::load, 36-byte records): every
//   spell's power, school, MP cost, hit rate, element and target flags - see SpellDefinition.
//   efficacy.beld.lz (EfficacyDataConvection::loadBELD: "BELD", a section count at 4, then
//   counts and offsets): the potions' hit and magic points and the abilities items cast.
//   player.chaindata chains 17..31: one learn list per PLAYER_TYPE, u32 = ability << 16 |
//   level - the class's commands (1 fight, 3 item, 0x2e change, then its own) and, under a
//   magic command (6 white, 5 black, 13 summon, 4 sing, 0x53 ninjutsu), the spells with the
//   level each is learnt at. Every list ends with the eight songs (the Bardsong augment).
//
// Character names and classes are not in these files by type; the list here is what the
// game is known to have, in PLAYER_TYPES order. The learn lists settle the order: type 1
// has Cover and Paladin Cecil's white magic, 2 Jump (Kain), 3 Pray and Aim with Rosa's
// spells, 4 the child Rydia's white, black and Chocobo, 5 the adult Rydia's black and
// summons, 6 Recall (Tellah), 7 Cry and Twincast (Porom), 8 Bluff and Twincast (Palom),
// 9 Kick, Focus, Brace (Yang), 10 Cid's, 11 Hide, Salve, Sing (Edward), 12 ninjutsu and
// Throw (Edge), 13 every spell (FuSoYa), 14 every black spell (Golbez). The names stay
// marked tentative until a file confirms them.

using System;
using System.Collections.Generic;
using OpenFF.Content;

namespace OpenFF.Data
{
	internal static class Ff4Tables
	{
		public const int CharacterTypes = 15;

		private static readonly (string Key, string Name, string Class)[] KnownCharacters =
		{
			("cecil", "Cecil", "Dark Knight"), ("cecil-paladin", "Cecil", "Paladin"), ("kain", "Kain", "Dragoon"),
			("rosa", "Rosa", "White Mage"), ("rydia", "Rydia", "Summoner"), ("rydia-adult", "Rydia", "Summoner"),
			("tellah", "Tellah", "Sage"), ("porom", "Porom", "White Mage"), ("palom", "Palom", "Black Mage"),
			("yang", "Yang", "Monk"), ("cid", "Cid", "Engineer"), ("edward", "Edward", "Bard"),
			("edge", "Edge", "Ninja"), ("fusoya", "FuSoYa", "Lunarian"), ("golbez", "Golbez", null),
		};

		public static GameTables Read(ContentChain chain)
		{
			GameTables tables = new GameTables { Game = "ff4" };
			ReadPlayers(chain, tables);
			ReadMagic(chain, tables);
			ReadEfficacies(chain, tables);
			ReadItems(chain, tables);
			ReadAbilityWaits(chain, tables);
			ReadConditions(chain, tables);
			ReadMonsters(chain, tables);
			ReadMonsterParties(chain, tables);
			ReadBattleParameter(chain, tables);
			return tables;
		}

		/// <summary>magic_parameter.bbd: 36-byte records, id first (BabilMagicParameterManager divides the size by 0x24 and walks them comparing the s16 at 0).</summary>
		private static void ReadMagic(ContentChain chain, GameTables tables)
		{
			if (!TableFiles.ReadAny(chain, "magic_parameter.bbd", out byte[] data))
			{
				tables.Notes.Add("magic_parameter.bbd not found");
				return;
			}
			tables.Spells.Clear();
			for (int at = 0; at + 36 <= data.Length; at += 36)
			{
				int id = ChainPack.S16(data, at);
				if (id <= 0) continue;
				byte[] r = new byte[36];
				Array.Copy(data, at, r, 0, 36);
				tables.Spells.Add(new SpellDefinition
				{
					Id = id,
					Name = tables.AbilityName(id),
					Power = ChainPack.S16(r, 2),
					School = (MagicSchool)Math.Min(7, (int)r[4]),
					MpCost = r[5],
					HitRate = ChainPack.U16(r, 6),
					EffectGroup = ChainPack.U16(r, 10),
					EffectRank = ChainPack.U16(r, 12),
					Element = ChainPack.U16(r, 22),
					Inflicts = ChainPack.U16(r, 24),
					Grants = ChainPack.U16(r, 26),
					Grants2 = ChainPack.U16(r, 28),
					TargetFlags = r[32],
					Conditions = BitConverter.ToUInt64(r, 0x18),   // one u64 condition mask (bit n = ys::Condition n)
					Kind = r[0x14],
					Raw = r,
				});
			}
		}

		/// <summary>efficacy.beld: "BELD", the section count at 4 (loadBELD reads it as a byte; three), then a u32 count per section and a u32 offset per section. Section 0 is one empty entry (id 0, what abilities without an efficacy point at); section 1 (12 bytes: id, hp, mp) the potions - 10 Potion 100, 11 Hi-Potion 500, 12 X-Potion 1000, 13 Ether 50 mp, 15 Elixir 9999/9999, 17 Phoenix Down 0/0; section 2 (20 bytes: id, 0, 0, ability, effect) the abilities items cast (40 casts 1501, the Goblin summon).</summary>
		private static void ReadEfficacies(ContentChain chain, GameTables tables)
		{
			if (!TableFiles.ReadAny(chain, "efficacy.beld", out byte[] data))
			{
				tables.Notes.Add("efficacy.beld not found");
				return;
			}
			if (data.Length < 8 || data[0] != (byte)'B' || data[1] != (byte)'E' || data[2] != (byte)'L' || data[3] != (byte)'D')
			{
				tables.Notes.Add("efficacy.beld: not a BELD file");
				return;
			}
			int sections = data[4];
			if (sections <= 0 || 8 + 8 * sections > data.Length) return;
			for (int s = 0; s < sections; s++)
			{
				int count = ChainPack.S32(data, 8 + 4 * s);
				int start = ChainPack.S32(data, 8 + 4 * sections + 4 * s);
				int end = s + 1 < sections ? ChainPack.S32(data, 8 + 4 * sections + 4 * (s + 1)) : data.Length;
				if (count <= 0 || start < 0 || end > data.Length || end <= start) continue;
				int stride = (end - start) / count;
				for (int i = 0; i < count; i++)
				{
					int at = start + stride * i;
					Efficacy e = new Efficacy { Id = ChainPack.S32(data, at) };
					if (stride >= 12) { e.Hp = ChainPack.S32(data, at + 4); e.Mp = ChainPack.S32(data, at + 8); }
					if (stride >= 20) { e.CastsAbility = ChainPack.S32(data, at + 12); e.X10 = ChainPack.S32(data, at + 16); e.Hp = 0; e.Mp = 0; }
					tables.Efficacies.Add(e);
				}
			}
		}

		/// <summary>
		/// monster_party_table.bbd: 520 records of 140 bytes (mon::MonsterPartyManager::load divides
		/// by 0x8C; monsterParty(id) walks them comparing the s16 at 0). Then up to six slots of 20
		/// bytes from offset 4 - monster id s16, flag s16, x, y, z fx32 and the facing in degrees as fx32 -
		/// a -1 id an empty slot, not the list's end (party 900, the opening's two Floating Eyes, fills
		/// slots 0 and 3); the tail holds words not yet named. Party 1 is three Goblins; the scripted
		/// battles 900..934 are here too. The positions and facings are the battle stage's own, as the
		/// Steam game stands its monsters.
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
				MonsterParty party = new MonsterParty { Id = ChainPack.S16(data, i), Flags = ChainPack.S16(data, i + 2), EscapeFlags = ChainPack.U16(data, i + 0x88), NormalEvent = ChainPack.S32(data, i + 0x7C), BeforeEvent = ChainPack.S32(data, i + 0x80), AfterEvent = ChainPack.S32(data, i + 0x84) };
				for (int s = 0; s < 6; s++)
				{
					int at = i + 4 + 20 * s;
					int id = ChainPack.S16(data, at);
					MonsterPartySlot slot = new MonsterPartySlot
					{
						MonsterId = id,
						Flag = ChainPack.S16(data, at + 2),
						X = ChainPack.S32(data, at + 4) / 4096f,
						Y = ChainPack.S32(data, at + 8) / 4096f,
						Z = ChainPack.S32(data, at + 12) / 4096f,
						W = ChainPack.S32(data, at + 16) / 4096f,
					};
					party.Places[s] = slot;   // an empty slot keeps its place: a monster called in later stands there
					if (id >= 0) party.Slots.Add(slot);
				}
				tables.MonsterParties.Add(party);
			}
		}

		/// <summary>
		/// battle_parameter.chain (29 chains; btl::BattleParameter). Chain 0 is the party's roots: records
		/// of 164 bytes - a u16 id, two bytes, then two rows of five 16-byte slots (x, y, z fx32 and a
		/// facing in degrees as fx32) - partyRoot(id) walks them by the id, position(row, slot) reads
		/// record + 4 + row x 80 + slot x 16. Record 0 is the normal fight: the front row at x 17..19,
		/// the back row at x 29..33, z -25, -5, 12, 35, 50 down the screen, facing -90 (towards -x, the
		/// monsters); record 1 the opening's airship deck (party 900 names it: the monster party's byte 2), record 2
		/// all on one spot. Chain 2 is each player type's battle parameter (24 bytes; +0x10 the b_p&lt;n&gt; motions,
		/// +0x12 the b_&lt;n&gt; ones - btl::BattlePlayer::addBasicMotion), chains 8.. one per player type of
		/// 16-byte weapon records by weapon system (+2 the poise motion, b_poise&lt;n&gt;; +0xe the b_w&lt;nn&gt;
		/// motions - playerPoiseMotionId, weaponMotionFileName). The other chains are not read yet.
		/// </summary>
		private static void ReadBattleParameter(ContentChain chain, GameTables tables)
		{
			if (!TableFiles.ReadAny(chain, "battle_parameter.chain", out byte[] data))
			{
				tables.Notes.Add("battle_parameter.chain not found");
				return;
			}
			ChainPack pack;
			try { pack = ChainPack.Read(data); }
			catch (Exception ex) { tables.Notes.Add("battle_parameter.chain: " + ex.Message); return; }
			if (pack.Count < 1) return;
			const int stride = 164;
			int off = pack.Offset(0);
			for (int i = 0; i + stride <= pack.Size(0); i += stride)
			{
				PartyRoot root = new PartyRoot { Id = ChainPack.U16(data, off + i) };
				for (int row = 0; row < 2; row++)
				{
					root.Rows[row] = new PartyRootSlot[5];
					for (int slot = 0; slot < 5; slot++)
					{
						int at = off + i + 4 + row * 80 + slot * 16;
						root.Rows[row][slot] = new PartyRootSlot
						{
							X = ChainPack.S32(data, at) / 4096f,
							Y = ChainPack.S32(data, at + 4) / 4096f,
							Z = ChainPack.S32(data, at + 8) / 4096f,
							Facing = ChainPack.S32(data, at + 12) / 4096f,
						};
					}
				}
				tables.PartyRoots.Add(root);
			}
			// Chain 24 (24 bytes, BattleParameter::monsterSummoningParameter): who a monster's Alarm or Summon calls - the
			// caller at 0, up to five candidates from 2 (-1 none), the effect at 0xC, the sound's bank at 0x12 and number at
			// 0x14, the encounter slot it comes in at at 0x16.
			if (pack.Count > 24)
			{
				for (int i = 0, n = pack.Records(24, 24); i < n; i++)
				{
					byte[] r = pack.Record(24, 24, i);
					MonsterSummon summon = new MonsterSummon { Caller = ChainPack.S16(r, 0), Effect = ChainPack.S16(r, 0xC), SeBank = ChainPack.S16(r, 0x12), SeNumber = ChainPack.S16(r, 0x14), Slot = r[0x16] };
					for (int k = 0; k < 5; k++) if (ChainPack.S16(r, 2 + 2 * k) >= 0) summon.Candidates.Add(ChainPack.S16(r, 2 + 2 * k));
					tables.MonsterSummons[summon.Caller] = summon;
				}
			}
			if (pack.Count > 2)
			{
				int at2 = pack.Offset(2);
				for (int i = 0; i + 24 <= pack.Size(2); i += 24)
				{
					tables.BattlePlayers.Add(new BattlePlayerMotions { Type = i / 24, PlayerSet = ChainPack.S16(data, at2 + i + 0x10), BasicSet = ChainPack.S16(data, at2 + i + 0x12) });
				}
			}
			// Chain 4 (32 bytes, BattleParameter::bossParameter): a boss's entrance - the monster at 0, the camera's start
			// position at 4 and target at 0x10 (fx32), the frames of its move to the standing shot at 0x1C (the last record
			// is two bytes short).
			if (pack.Count > 4)
			{
				int at4 = pack.Offset(4), size4 = pack.Size(4);
				for (int i = 0; i + 30 <= size4; i += 32)
				{
					float F(int o) => ChainPack.S32(data, at4 + i + o) / 4096f;
					tables.BossCameras[ChainPack.S16(data, at4 + i)] = new BossCamera
					{
						Position = new[] { F(4), F(8), F(0xC) }, Target = new[] { F(0x10), F(0x14), F(0x18) },
						Frames = ChainPack.S16(data, at4 + i + 0x1C),
					};
				}
			}
			// Chain 1 (44 bytes, BattleParameter::abilityInvokeParameter): a command's invoke - the chant motion by player
			// form at 2 (15 s16), the motion after at 0x20, the invoke effect at 0x22 (its parameter at 0x24, its place at
			// 0x26: 0 the hit spot, 1 the feet, 2 the body), the sound's bank and number at 0x28 and 0x2A.
			if (pack.Count > 1)
			{
				for (int i = 0, n = pack.Records(1, 44); i < n; i++)
				{
					byte[] r = pack.Record(1, 44, i);
					short[] row = new short[22];
					for (int k = 0; k < 22; k++) row[k] = (short)ChainPack.S16(r, 2 * k);
					if (row[0] > 0) tables.AbilityInvokes[row[0]] = row;
				}
			}
			if (pack.Count > 5)
			{
				int at5 = pack.Offset(5);
				for (int i = 0; i + 44 <= pack.Size(5); i += 44)
				{
					short[] sounds = new short[22];
					for (int k = 0; k < 22; k++) sounds[k] = (short)ChainPack.S16(data, at5 + i + 2 * k);
					tables.WeaponSounds.Add(sounds);
				}
			}
			for (int type = 0; type < 15 && 8 + type < pack.Count; type++)
			{
				int at = pack.Offset(8 + type);
				for (int i = 0; i + 16 <= pack.Size(8 + type); i += 16)
				{
					short[] raw = new short[8];
					for (int k = 0; k < 8; k++) raw[k] = (short)ChainPack.S16(data, at + i + 2 * k);
					tables.WeaponMotions.Add(new WeaponMotionRecord { PlayerType = type, WeaponSystem = raw[0], Poise = raw[1], WeaponSet = raw[7], Raw = raw });
				}
			}
		}

		/// <summary>
		/// monster.chaindata chain 0: 252 records of 152 bytes. The head is FF3's (nameId, textId,
		/// familyId, modelId, monsterId at 8, level, size, maxHp s32 at 0xC); five attribute bytes
		/// at 0x12; four (item, chance of 4096) drop pairs from 0x6C; gil s32 at 0x88 and
		/// experience at 0x8C (Steam's result window: two Floating Eyes give 14 gil and 300 EXP). babil_battle.msd names them by name id.
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
					AtbRateMin = ChainPack.S32(r, 0x18) / 4096f,
					AtbRateMax = ChainPack.S32(r, 0x1C) / 4096f,
					BackRowAttack = ChainPack.S32(r, 0x28) / 4096f,
					BackRowTarget = ChainPack.S32(r, 0x2C) / 4096f,
					Stats = new Stats { Strength = r[0x12], Vitality = r[0x13], Agility = r[0x14], Intellect = r[0x15], Spirit = r[0x16] },
					Attack = ChainPack.S16(r, 0x20),
					Hit = ChainPack.S16(r, 0x24),   // its ys::PhysicsAttackParameter at 0x20: attack s32, hit s16 at +4 (the Floating Eye's 8 and 105)
					Defence = ChainPack.S16(r, 0x4C),
					Evade = ChainPack.S16(r, 0x50),
					MagicDefence = ChainPack.S16(r, 0x66),   // its magic defence struct at 0x64: the defence at +2, the magic evasion at +4
					MagicEvasion = ChainPack.S16(r, 0x68),
					Gil = ChainPack.S32(r, 0x88),
					Experience = ChainPack.S32(r, 0x8C),
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
			// Chain 4: 84 bytes a monster (s16 monster id at 0), MonsterManager::offset's: the damage number's spot over
			// the monster (s32 x, y, z at 0x28, whole units; btl::BattleBehavior::createDamage adds it to its position).
			// Keyed by the id, not the model: Steam's frames put a Floating Eye's (id 3, model 2) number 15 over it, the
			// projection of the id's 15 - where its model's record says 6 - as Cecil's own 6 lands where ours does.
			Dictionary<int, byte[]> offsets = new Dictionary<int, byte[]>();
			for (int i = 0, n = pack.Records(4, 84); i < n; i++)
			{
				byte[] r = pack.Record(4, 84, i);
				offsets[ChainPack.S16(r, 0)] = r;
			}
			foreach (MonsterDefinition m in tables.Monsters)
			{
				if (!offsets.TryGetValue(m.Id, out byte[] r)) continue;
				m.DamageX = ChainPack.S32(r, 0x28);
				m.DamageY = ChainPack.S32(r, 0x2C);
				m.DamageZ = ChainPack.S32(r, 0x30);
				m.EffectToCamera = ChainPack.S32(r, 4);
				m.EffectHeight = ChainPack.S32(r, 8);
				m.Scale = ChainPack.S32(r, 0x44) / 4096f;   // the model's size in battle (fx32; changeLilliput halves it)
				m.ChantScale = ChainPack.S32(r, 0x50) / 4096f;
			}
			// The AI (btl::MonsterActionThinker::calculationAction): chain 7, a monster's record; chain 8, the action sets;
			// chain 9, the conditions that switch a monster to another set.
			for (int i = 0, n = pack.Records(7, 22); i < n; i++)
			{
				byte[] r = pack.Record(7, 22, i);
				short[] ai = new short[11];
				for (int k = 0; k < 11; k++) ai[k] = (short)ChainPack.S16(r, 2 * k);
				tables.MonsterAi[ai[0]] = ai;
			}
			for (int i = 0, n = pack.Records(8, 44); i < n; i++)
			{
				byte[] r = pack.Record(8, 44, i);
				MonsterTurnAction set = new MonsterTurnAction { Id = ChainPack.S16(r, 0), Random = r[2] == 1 };
				for (int k = 0; k < 10; k++)
				{
					int ability = ChainPack.S16(r, 4 + 4 * k);
					if (ability == -1) break;
					set.Entries.Add((ability, ChainPack.S16(r, 6 + 4 * k)));
				}
				tables.MonsterTurnActions[set.Id] = set;
			}
			for (int i = 0, n = pack.Records(9, 12); i < n; i++)
			{
				byte[] r = pack.Record(9, 12, i);
				tables.MonsterActionConditions[ChainPack.S16(r, 0)] = (ChainPack.S16(r, 2), BitConverter.ToUInt64(r, 4));
			}
			// Chains 5 (68 bytes, by ability) and 11 (72 bytes, by ability and the monster at 0x44): how a monster's
			// ability is shown (effectsInfo) - its motion at 2, the effect pack at 8 and parameter at 0xA, the position mode
			// at 0xF, the period N at 0x10, the sound's bank at 0x34 and number at 0x36.
			SpellShow Show(byte[] r) => new SpellShow
			{
				Motion = ChainPack.S16(r, 2), Pack = ChainPack.S16(r, 8), Param = ChainPack.S16(r, 0xA), Mode = r[0xF],
				Period = ChainPack.S32(r, 0x10), SeBank = ChainPack.S16(r, 0x34), SeNumber = ChainPack.S16(r, 0x36),
			};
			for (int i = 0, n = pack.Records(5, 68); i < n; i++)
			{
				byte[] r = pack.Record(5, 68, i);
				tables.MonsterAbilityShows[ChainPack.S16(r, 0)] = Show(r);
			}
			if (pack.Count > 11)
			{
				for (int i = 0, n = pack.Records(11, 72); i < n; i++)
				{
					byte[] r = pack.Record(11, 72, i);
					tables.MonsterAbilityShowsFor[(ChainPack.S16(r, 0), ChainPack.S16(r, 0x44))] = Show(r);
				}
			}
			// Chain 6 (28 bytes): the Octomammoth's legs - the legs left at 0, the leg at 1, its place from the body at 4..0xC
			// and its turn at 0x10..0x18 (fx32; degrees for the turn).
			for (int i = 0, n = pack.Records(6, 28); i < n; i++)
			{
				byte[] r = pack.Record(6, 28, i);
				float F(int at) => ChainPack.S32(r, at) / 4096f;
				tables.OctomammothLegs[(r[0], r[1])] = new[] { F(4), F(8), F(0xC), F(0x10), F(0x14), F(0x18) };
			}
			// Chain 10: the counters (14 bytes) - an id, then two of an ability, a target type and a chance in percent; a
			// counter condition of the AI record (its +0xC..+0x14) names one in its set field.
			for (int i = 0, n = pack.Records(10, 14); i < n; i++)
			{
				byte[] r = pack.Record(10, 14, i);
				tables.MonsterCounters[ChainPack.S16(r, 0)] = new[]
				{
					(ChainPack.S16(r, 2), ChainPack.S16(r, 4), ChainPack.S16(r, 6)),
					(ChainPack.S16(r, 8), ChainPack.S16(r, 0xA), ChainPack.S16(r, 0xC)),
				};
			}
			// Chain 2: 28 bytes a monster, by its id (MonsterManager::normalAttack) - its plain attack as ys::Effects: the
			// effect's frame (s32 at 0), its pack (s16 at 6) and member (s32 at 8), the sound's frame (s32 at 0xC), bank
			// (s16 at 0x12) and number (s16 at 0x14), the number's frame (s16 at 0x1A). The Floating Eye's: e160 at 7,
			// (103, 0) at 7, the number at 7.
			int attacks = pack.Records(2, 28);
			foreach (MonsterDefinition m in tables.Monsters)
			{
				if (m.Id < 0 || m.Id >= attacks) continue;
				byte[] r = pack.Record(2, 28, m.Id);
				m.AttackEffectFrame = ChainPack.S32(r, 0);
				m.AttackEffect = ChainPack.S16(r, 6);
				m.AttackSoundFrame = ChainPack.S32(r, 0xC);
				m.AttackSoundBank = ChainPack.S16(r, 0x12);
				m.AttackSound = ChainPack.S16(r, 0x14);
				m.AttackNumberFrame = ChainPack.S16(r, 0x1A);
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
					RollsHp = true,
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
				// The learn list: chains 17.., u32 = ability << 16 | level; a command (below 1500)
				// opens a group and the spells after it fall under it.
				int lc = 17 + type;
				if (lc < pack.Count)
				{
					int command = 0;
					int entries = pack.Size(lc) / 4;
					for (int i = 0; i < entries; i++)
					{
						uint w = ChainPack.U32(pack.Data, pack.Offset(lc) + 4 * i);
						int ability = (int)(w >> 16), level = (int)(w & 0xFFFF);
						if (ability < 1500) command = ability;
						// Every list carries Items (4) and after it the eight songs (4801..) for the Bardsong augment;
						// only the bard (type 11) has the songs from the start. Augments are not modelled yet.
						if (command == 4 && ability != 4 && type != 11) continue;
						def.Learning.Add(new Learned { Command = ability < 1500 ? ability : command, Ability = ability, Level = Math.Max(1, level) });
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
			// Chain 32 (32-byte records, id first; pl::PlayerParty::normalMagic) lists the spells
			// too, with an effect id at 10; magic_parameter.bbd carries the numbers (ReadMagic).
			// Chain 34: the victory layouts (pl::PlayerParty::layoutSceneParameter, 108-byte records by id: the party's
			// size less one, 5..9 again for another stage): the camera's position at 4 and target at 16 (fx32), then from
			// 0x1c each member's spot and facing (x, y, z fx32, degrees fx32) - btl::BattleWin::layout stands them there
			// and pl::layoutCharacterScene sets the camera. One member: (-1, 0, 10) facing -15, the camera at (-15, 13,
			// 100.5) looking at (6, 12.2, -18.2), as Steam's victory ends.
			if (pack.Count > 34)
			{
				int at = pack.Offset(34);
				for (int i = 0; i + 108 <= pack.Size(34); i += 108)
				{
					VictoryLayout v = new VictoryLayout { Id = ChainPack.S16(pack.Data, at + i) };
					v.CameraPosition = new float[] { ChainPack.S32(pack.Data, at + i + 4) / 4096f, ChainPack.S32(pack.Data, at + i + 8) / 4096f, ChainPack.S32(pack.Data, at + i + 12) / 4096f };
					v.CameraTarget = new float[] { ChainPack.S32(pack.Data, at + i + 16) / 4096f, ChainPack.S32(pack.Data, at + i + 20) / 4096f, ChainPack.S32(pack.Data, at + i + 24) / 4096f };
					for (int m = 0; m < 5; m++)
					{
						int o = at + i + 0x1c + 16 * m;
						v.Spots[m] = new PartyRootSlot { X = ChainPack.S32(pack.Data, o) / 4096f, Y = ChainPack.S32(pack.Data, o + 4) / 4096f, Z = ChainPack.S32(pack.Data, o + 8) / 4096f, Facing = ChainPack.S32(pack.Data, o + 12) / 4096f };
					}
					tables.VictoryLayouts.Add(v);
				}
			}
			// Chain 32 is also how each spell is shown (normalMagic, 32 bytes): the position mode at 2, the ys::Effects from 4
			// - the effect pack at 0xA and its parameter at 0xC, the sound's bank at 0x16 and number at 0x18 - and the
			// period N at 0x1E (Fire 18: its effects on several targets 9 frames apart).
			if (pack.Count > 32)
			{
				for (int i = 0, n = pack.Size(32) / 32; i < n; i++)
				{
					byte[] r = pack.Record(32, 32, i);
					tables.SpellShows[ChainPack.S16(r, 0)] = new SpellShow
					{
						Mode = ChainPack.S16(r, 2), Pack = ChainPack.S16(r, 0xA), Param = ChainPack.S16(r, 0xC),
						SeBank = ChainPack.S16(r, 0x16), SeNumber = ChainPack.S16(r, 0x18), Period = ChainPack.S16(r, 0x1E),
					};
				}
			}
			if (pack.Count > 32 && tables.Spells.Count == 0)
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

		/// <summary>
		/// condition_parameter.bbd (common::StatusConditionManager::load): 24 bytes a condition - the id, its duration at 2
		/// (-1 none; the timer is it in frames at the middle speed), the flag word at 4, the conditions it clears as it comes
		/// at 8 and the ones that keep it off at 0x10 (u64 masks). The names are babil_battle's 70300 + id.
		/// </summary>
		private static void ReadConditions(ContentChain chain, GameTables tables)
		{
			if (!TableFiles.ReadAny(chain, "condition_parameter.bbd", out byte[] data)) { tables.Notes.Add("condition_parameter.bbd not found"); return; }
			Dictionary<uint, string> names = TableFiles.ReadNames(chain, "babil_battle.msd", tables);
			for (int at = 0; at + 24 <= data.Length; at += 24)
			{
				ConditionParameter c = new ConditionParameter
				{
					Id = ChainPack.S16(data, at), Duration = ChainPack.S16(data, at + 2), Flags = ChainPack.U16(data, at + 4),
					Replaces = BitConverter.ToUInt64(data, at + 8), BlockedBy = BitConverter.ToUInt64(data, at + 0x10),
				};
				if (names != null && names.TryGetValue((uint)(70300 + c.Id), out string name)) c.Name = name;
				tables.Conditions[c.Id] = c;
			}
		}

		/// <summary>ability.bbd (common::AbilityManager::load): 44-byte records, the id s32 at 0, the wait before the action s32 at 0x18 - Attack's 0, Fire's (4501) 15, Firaga's (4503) 90.</summary>
		private static void ReadAbilityWaits(ContentChain chain, GameTables tables)
		{
			if (!TableFiles.ReadAny(chain, "ability.bbd", out byte[] data)) { tables.Notes.Add("ability.bbd not found"); return; }
			for (int at = 0; at + 44 <= data.Length; at += 44)
			{
				int wait = ChainPack.S32(data, at + 0x18);
				if (wait > 0) tables.AbilityWaits[ChainPack.S32(data, at)] = wait;
				tables.AbilityNameIds[ChainPack.S32(data, at)] = ChainPack.S32(data, at + 8);
				// The statuses it may be used under (+0x1C), when +0x24 bit 0 says the check applies (isConditionUseful).
				if ((ChainPack.U16(data, at + 0x24) & 1) != 0) tables.AbilityUsableUnder[ChainPack.S32(data, at)] = BitConverter.ToUInt64(data, at + 0x1C);
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
						ModelId = ChainPack.S16(r, 10),
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
