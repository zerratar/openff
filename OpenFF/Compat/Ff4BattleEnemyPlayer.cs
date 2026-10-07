// A monster that fights as a member (btl::BattleEnemyPlayer, read from libff4.so): the monster record's class 3 - the
// Dark Knight on Mt. Ordeals, Kain at Fabul, the Bard, the Girl, Yang - built as a BattlePlayer and a BattleMonster at
// once. Its look, motions, hands and behaviour are a member's: the player model p<type>_00 (Asano: p22_00) with the
// member's motion sets (addBasicMotion), the items battle_parameter chain 7 puts in its hands, the stance 2004, a member's
// Fight (poise, swing, the weapon's hit effect and sound - BattlePlayerBehavior, not the monster's attack record), the
// commands' invoke stage, the KO motion 2003 and the kneel when weak (never the Dark Knight's). Its stats, HP, statuses,
// gauge, AI and place are the monster's: the record, MonsterManager::ai, the party slot and its facing.

using System;
using System.Collections.Generic;
using OpenFF.Data;

namespace OpenFF.Client
{
	internal sealed partial class Ff4Battle
	{
		/// <summary>The Dark Knight (215): BattlePlayer::setConditionMotion never kneels him when weak.</summary>
		private const int DarkKnight = 215;

		/// <summary>Asano (244): drawn as p22_00 on the enemy side (BattlePlayer::registerModel).</summary>
		private const int Asano = 244;

		/// <summary>registerParty / registerMonster for class 3: the player model at the slot, its motions and hands, the stance.</summary>
		private Fighter SpawnEnemyPlayer(MonsterDefinition m, (int PlayerType, int Right, int Left) row, Vector3 at, float? facing, bool join)
		{
			GameTables tables = Ff4Party.Tables;
			CharacterDefinition definition = tables.Character(row.PlayerType);
			if (definition == null) return null;
			// The member it is drawn as: its hands from the table (the stats stay the monster's).
			Character look = new Character(definition, Math.Max(1, m.Level)) { Name = m.Name ?? definition.Name };
			look.Equipment[(int)EquipSlot.RightHand] = row.Right > 0 ? row.Right : 0;
			look.Equipment[(int)EquipSlot.LeftHand] = row.Left > 0 ? row.Left : 0;
			string model = m.Id == Asano ? "p22_00" : "p" + row.PlayerType.ToString("00") + "_00";
			Npc npc = null;
			try { npc = Game.Npcs.SpawnModel(model, at, 0f); } catch (Exception) { }
			if (npc == null) { Log.Write(LogChannel.General, "battle: no player model " + model + " for " + m.Name); return null; }
			npc.Solid = false;
			Fighter foe = new Fighter
			{
				Name = m.Name ?? ("monster " + m.Id), IsMonster = true, Monster = m, Npc = npc, Home = at, MagicEvasion = m.MagicEvasion,
				Hp = Math.Max(1, m.MaxHp), MaxHp = Math.Max(1, m.MaxHp),
				Attack = Math.Max(1, m.Attack), Defence = Math.Max(0, m.Defence), Agility = Math.Max(1, m.Stats.Agility),
				Level = Math.Max(1, m.Level), Intellect = m.Stats.Intellect, Spirit = m.Stats.Spirit, Vitality = m.Stats.Vitality, MagicDefence = Math.Max(0, m.MagicDefence),
				Strength = m.Stats.Strength, HitChance = m.Hit > 0 ? m.Hit : 90, Evade = Math.Max(0, m.Evade),
				Gauge = StartGauge(true),
				AtbRate = m.AtbRateMin + (float)_random.NextDouble() * Math.Max(0f, m.AtbRateMax - m.AtbRateMin),
				Facing = facing ?? 0f,
				PlayerType = row.PlayerType,
				Swings = 1,   // Steam's Dark Knight: his first blow the weapon's other swing (95)
			};
			try
			{
				WeaponMotionRecord weapon = BindBattleMotions(npc, row.PlayerType, look, tables);
				if (weapon != null) { foe.Poise = weapon.Poise; foe.SwingA = weapon.Raw[3]; foe.SwingB = weapon.Raw[2]; }
				int system = WeaponSystem(look, tables);
				foe.HitEffect = HitEffectOf(system);
				if (system >= 0 && system < tables.WeaponSounds.Count && tables.WeaponSounds[system][0] >= 0 && tables.WeaponSounds[system][1] >= 0)
				{
					foe.HitBank = tables.WeaponSounds[system][0];
					foe.HitSound = tables.WeaponSounds[system][1];
				}
				LoadEffect(foe.HitEffect);
			}
			catch (Exception ex) { Log.Write(LogChannel.General, "battle: " + m.Name + "'s motions: " + ex.Message); }
			HoldEquipment(npc, look, tables);
			if (facing.HasValue) Face(foe, facing.Value);
			foe.IdleMotion = _heroMotionIdle;
			Play(foe, foe.IdleMotion, true, 0);
			if (join) _foes.Add(foe);
			Log.Write(LogChannel.File, "battle: " + foe.Name + " fights as player type " + row.PlayerType + " (" + model + ")");
			return foe;
		}

		/// <summary>checkMotionHealth / setConditionMotion for one: the stance, or the kneel when weak (never the Dark Knight's).</summary>
		private int EnemyPlayerIdle(Fighter f) =>
			f.Monster?.Id != DarkKnight && (AnyFlag(f, 5) || f.Hp <= f.MaxHp / 4) ? 2001 : _heroMotionIdle;

		/// <summary>
		/// The AI's pick run as BattlePlayerBehavior runs a member's command (setMonsterAbility): Fight as a member's
		/// blow on a member, Darkness and Focus through their invoke stages. False for the rest (spells and the monster
		/// abilities go the monster's way).
		/// </summary>
		private bool EnemyPlayerTurn(Fighter foe, int ability, int targetType, Fighter target = null)
		{
			Fighter Target() => target != null && target.Alive ? target : MonsterTargets(foe, targetType == 0 ? 1 : targetType).Find(f => f.Alive && !f.IsMonster);
			switch (ability)
			{
				case 1:
				{
					Fighter struck = Target();
					if (struck == null) return false;
					MemberAttacks(foe, struck);
					return true;
				}
				case CmdJump:
				{
					// Kain's Jump at Fabul (the battle event's action): a member's leap, the landing on a member.
					Fighter struck = Target();
					if (struck == null) return false;
					JumpStart(foe, struck);
					return true;
				}
				case 32:
					Darkness(foe);
					return true;
				case CmdFocus:
					AbilityMotions(foe, "b_pa_021");
					Focus(foe);
					return true;
				default:
					return false;
			}
		}
	}
}
