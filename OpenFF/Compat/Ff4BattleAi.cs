// FF4's monster AI as btl::MonsterActionThinker runs it, read from libff4.so's code: the conditions a monster's AI
// record names (isEnableCondition, every check of a condition's mask holding), the targets an action's type picks
// (calculationTarget) and the counters every monster weighs as an action ends (cheakCounter / calculationCounter).
//
// "The last action" is the one being resolved or just resolved (the system's actor and its ActionParameter): who acted,
// on whom, and its command - Fight 1, Jump 0x1F, a magic command for a spell (the school's: white 6, black 5, summon
// 13...), a monster's special as itself (3000 on). Statuses are not in the battle yet, so a check of one does not hold
// (and one that asks for its absence does).

using System;
using System.Collections.Generic;
using OpenFF.Data;

namespace OpenFF.Client
{
	internal sealed partial class Ff4Battle
	{
		private SpellDefinition _lastSpell;   // the spell the last action cast, if it was one
		private readonly Dictionary<Action, int> _counterAbilities = new Dictionary<Action, int>();   // a queued counter's ability, for the before event
		private bool _isCounter;              // the action under way is a counter (BattleSystem +0x20EC; the events' IsCounter)

		/// <summary>The command a spell's school is cast with (setMonsterAbility's table: white 6, black 5, summon 13, 18, item 6, enemy 9, ninjutsu 0x53).</summary>
		private static readonly int[] SchoolCommand = { 6, 5, 13, 18, 6, 9, 0x53, 0 };

		/// <summary>The last action's command (ActionParameter::abilityId): a spell's school's command, else the ability as it was.</summary>
		private int LastCommand => _lastSpell != null ? SchoolCommand[Math.Clamp((int)_lastSpell.School, 0, 7)] : _lastAbility;

		/// <summary>Whether the last action was struck at this one (it was among the targets; nothing reflects yet).</summary>
		private bool HitBy(Fighter self) => _lastTargets.Contains(self);

		/// <summary>The last action's spell when it came by a magic command (white or black - SPELLCMD; a summon or a monster's spell does not count), and someone else cast it on this one.</summary>
		private SpellDefinition SpellOn(Fighter self)
		{
			if (_attacker == null || _attacker == self || _lastSpell == null || !HitBy(self)) return null;
			int command = LastCommand;
			return command == 5 || command == 6 ? _lastSpell : null;
		}

		/// <summary>
		/// isEnableCondition: every check the mask names must hold. HP against a fraction of the maximum or a number; the
		/// battle events' variables; what the last action was and whether it struck this one (a plain attack, Jump, a
		/// summon, a spell of an element or none, one it is weak to, a physical command, any hurt); who else is standing
		/// on either side. A status of its own does not hold while the battle has none.
		/// </summary>
		private bool ConditionHolds(Fighter foe, ulong checks)
		{
			float hp = foe.Hp, max = foe.MaxHp;
			int id = foe.Monster?.Id ?? -1;
			for (int bit = 0; bit < 63; bit++)
			{
				if ((checks >> bit & 1) == 0) continue;
				bool holds;
				switch (bit)
				{
					case 4: case 16: case 17: case 27: holds = true; break;
					// its own statuses (ys::Condition)
					case 0: holds = Has(foe, CKO); break;
					case 1: holds = Has(foe, CStone); break;
					case 2: holds = Has(foe, CToad); break;
					case 3: holds = Has(foe, CSilence); break;
					case 5: holds = Has(foe, CBlind); break;
					case 6: holds = Has(foe, CPoison); break;
					case 7: holds = Has(foe, CCritical); break;
					case 8: holds = Has(foe, CParalyze); break;
					case 9: holds = Has(foe, CSleep); break;
					case 10: holds = Has(foe, CConfuse); break;
					case 11: holds = Has(foe, CPetrify); break;
					case 44: holds = Has(foe, CReflect); break;
					case 52: holds = !Has(foe, CReflect); break;
					case 12: holds = foe.Alive && _foes.FindAll(f => f.Alive).Count == 1; break;   // the only monster left
					case 13: holds = hp <= max * 0.3f; break;
					case 14: holds = false; break;   // a spell with its +0x1A bit 0 cast on it: not read yet
					case 15: holds = LastCommand == 1 && HitBy(foe); break;   // struck at by a plain attack
					case 18: holds = _attacker != null && _attacker != foe && LastCommand == 13 && HitBy(foe); break;   // a summon
					case 19: holds = ((SpellOn(foe)?.Element ?? 0) & 0x20) != 0; break;    // fire
					case 20: holds = ((SpellOn(foe)?.Element ?? 0) & 0x10) != 0; break;    // ice
					case 21: holds = ((SpellOn(foe)?.Element ?? 0) & 0x08) != 0; break;    // lightning
					case 22: holds = ((SpellOn(foe)?.Element ?? 0) & 0x200) != 0; break;
					case 23: holds = ((SpellOn(foe)?.Element ?? 0) & 0x80) != 0; break;    // earth
					case 24: holds = ((SpellOn(foe)?.Element ?? 0) & 0x100) != 0; break;   // holy
					case 25: holds = hp <= max * 0.6f; break;
					case 26: { SpellDefinition s = SpellOn(foe); holds = s != null && s.Element == 0; break; }   // a spell of no element
					case 28: holds = LastCommand == 0x1F && HitBy(foe); break;   // struck at by Jump
					case 29: holds = foe.Hp <= 20000; break;
					case 30:
					{
						// the Octomammoth with a leg to lose: more legs than its HP's tenths less one (at least 1)
						int s = foe.MaxHp > 0 ? foe.Hp * 10 / foe.MaxHp - 1 : -1;
						if (s < 2) s = 1;
						holds = id == 0x9E && LegsLeft(foe) > s;
						break;
					}
					case 31: holds = foe.Mist; break;   // in mist form (flag 0x1e)
					case 32: holds = foe.Hp <= 10000; break;
					case 33: holds = foe.Free[0] == 1; break;
					case 34: holds = _foes.TrueForAll(f => !f.Alive || f.Monster?.Id == id); break;   // no other kind standing
					case 35: holds = SpellOn(foe) != null; break;   // any magic cast on it
					case 36: holds = HitBy(foe) && foe.Hp < foe.HpBefore; break;   // the last action hurt it
					case 37: holds = !_foes.Exists(f => f.Alive && f.Monster?.Id == 0xA5); break;
					case 38: holds = foe.Free[0] == 2; break;
					case 39: holds = foe.Free[1] == 1; break;
					case 40: holds = foe.Hp <= 700; break;
					case 41: holds = _attacker != null && _attacker != foe && HitBy(foe) && LastCommand == 1; break;   // a physical command (Fight)
					case 42: holds = foe.Hp <= 100; break;
					case 43: holds = _party.Exists(f => !f.Alive); break;   // a member is down
					case 45: holds = foe.Hp <= 40000; break;
					case 46: holds = foe.Hp <= 1; break;
					case 47: holds = foe.Free[0] == 0; break;
					case 48: holds = LastCommand == 7 && HitBy(foe); break;
					case 49: holds = !_foes.Exists(f => f != foe && f.Alive && f.Monster?.Id == id); break;   // the last of its kind
					case 50: holds = _party.TrueForAll(f => !f.Alive || Has(f, CDoom)); break;   // every member standing has Doom
					case 51:
					{
						// struck by an element it is weak to (its record's 0x64): the spell's, else the attacker's blow's
						if (_attacker == null || !HitBy(foe)) { holds = false; break; }
						int element = _lastSpell != null ? _lastSpell.Element : AffinityOf(_attacker).Elements;
						holds = (element & AffinityOf(foe).Weak) != 0;
						break;
					}
					case 53: holds = _attacker != null && HitBy(foe) && LastCommand == 0xBDA; break;
					case 54: holds = foe.Hp <= 1000; break;
					case 55: holds = foe.Free[0] != 2; break;
					case 56: holds = hp <= max * 0.2f || hp >= max * 0.5f; break;
					case 57: holds = hp <= max * 0.4f; break;
					case 58: holds = foe.Free[1] == 0; break;
					case 59: holds = hp > max * 0.2f; break;
					case 60: holds = hp <= max * 0.8f; break;
					case 61: holds = _foes.Exists(f => f.Npc != null && Has(f, CStone)); break;   // an ally turned to stone
					case 62: holds = hp <= max * 0.2f; break;
					default: holds = false; break;
				}
				if (!holds) return false;
			}
			return true;
		}

		/// <summary>
		/// calculationTarget: who an action of the type falls on. 0 no one; 1 a member (at random); 2 and 3 one of its own
		/// side; 4 every member; 5 the one whose action is being resolved (for a counter, who struck it); 6 its side but
		/// itself; 7 itself; 8 its side's fallen; 9 every 0xAE; 10 everyone; 11 its whole side; 12 the first 0xC9. Only the
		/// standing are picked but for 8, 9 and 12. Nobody to pick and the action comes to nothing.
		/// </summary>
		private List<Fighter> MonsterTargets(Fighter foe, int type)
		{
			List<Fighter> members = _party.FindAll(f => f.Alive), side = _foes.FindAll(f => f.Alive);
			Fighter remembered = foe.Remembered;   // read and let go on every pick, whatever the type
			foe.Remembered = null;
			if (type == 1 && remembered != null && remembered.Alive && !OutOfFight(remembered)) return new List<Fighter> { remembered };
			List<Fighter> one(List<Fighter> from) => from.Count == 0 ? new List<Fighter>() : new List<Fighter> { from[_random.Next(from.Count)] };
			switch (type)
			{
				case 0: return new List<Fighter>();
				case 1: return one(members);
				case 2: case 3: return one(side);
				case 4: return members;
				case 5: return _attacker != null && _attacker.Alive ? new List<Fighter> { _attacker } : new List<Fighter>();
				case 6: return side.FindAll(f => f != foe);
				case 7: return foe.Alive ? new List<Fighter> { foe } : new List<Fighter>();
				case 8: return _foes.FindAll(f => !f.Alive);
				case 9: return _foes.FindAll(f => f.Monster?.Id == 0xAE);
				case 10: { List<Fighter> all = new List<Fighter>(members); all.AddRange(side); return all; }
				case 11: return side;
				case 12: { Fighter f = _foes.Find(x => x.Monster?.Id == 0xC9); return f != null ? new List<Fighter> { f } : new List<Fighter>(); }
				default:
					Log.First(LogChannel.File, "monster-target-" + type, 1, () => "battle: monster target type " + type + " is not FF4's");
					return one(members);
			}
		}

		/// <summary>A monster's turn, as decided: its ability on the targets its type picks (none and it does nothing).</summary>
		private void MonsterActs(Fighter foe)
		{
			foe.Gauge = 0f;
			Perform(foe, foe.DecidedAbility, foe.DecidedTarget, null);
		}

		/// <summary>A monster's ability on its targets: a spell cast, else a plain attack on a member (an ability not in yet struck as one).</summary>
		private void Perform(Fighter foe, int ability, int targetType, List<Fighter> targets)
		{
			if (ability == 0) { Note(foe.Name + " does nothing."); return; }
			if (QuietTurn(foe, ability)) return;
			if (EnemySummon(foe, ability)) return;
			if (MistTurn(foe, ability)) return;
			if (BossTurn(foe, ability)) return;
			targets ??= MonsterTargets(foe, targetType);
			SpellDefinition spell = ability != 1 ? Ff4Party.Tables.Spell(ability) : null;
			if (spell != null)
			{
				if (targets.Count == 0) { Note(foe.Name + " has no one to cast " + spell.Name + " on."); return; }
				MonsterCasts(foe, spell, targets);
				return;
			}
			if (ability != 1 && ability != 134 && ability != 62) Log.First(LogChannel.File, "monster-ability-" + ability, 1, () => "battle: monster ability " + ability + " not in yet - a plain attack in its place");
			// A plain attack falls on its target - a member, or one of its own side for types 2 and 3 - else a member at
			// random (a type 0 attack's own pick); a confused monster's blow goes at its own side (calcNormalAttack).
			Fighter target = targetType == 2 || targetType == 3 ? targets.Find(f => f.Alive) : targets.Find(f => !f.IsMonster && f.Alive);
			if (ability == 1 && Has(foe, CConfuse))
			{
				List<Fighter> own = _foes.FindAll(f => f.Alive && !OutOfFight(f));
				if (own.Count > 0) target = own[_random.Next(own.Count)];
			}
			if (target == null && targetType == 0) target = MonsterTargets(foe, 1).Find(f => true);
			if (target == null) { Note(foe.Name + " has no one to strike."); return; }
			if (PiercingAttack(foe, ability, target)) return;
			MonsterAttack(foe, target);
		}

		/// <summary>
		/// cheakCounter, as an action's turn ends: every standing monster weighs its AI record's counter conditions (+0xC..
		/// +0x14, the first that holds), and that condition's counter (chain 10) has two entries, each taken by its chance
		/// - an ability and its targets, picked now - and run before the next turn. One already waiting is not queued again.
		/// </summary>
		private void CheckCounters()
		{
			GameTables t = Ff4Party.Tables;
			if (t == null) return;
			List<(Fighter, Action)> counters = new List<(Fighter, Action)>();
			foreach (Fighter foe in _foes)
			{
				if (!foe.Alive || foe.Monster == null || !t.MonsterAi.TryGetValue(foe.Monster.Id, out short[] ai)) continue;
				int condition = -1;
				for (int k = 6; k <= 10; k++)
				{
					if (ai[k] < 0 || !t.MonsterActionConditions.TryGetValue(ai[k], out (int TurnAction, ulong Checks) c)) continue;
					if (!ConditionHolds(foe, c.Checks)) continue;
					condition = ai[k];
					break;
				}
				if (condition < 0 || !t.MonsterCounters.TryGetValue(t.MonsterActionConditions[condition].TurnAction, out (int Ability, int Target, int Chance)[] entries)) continue;
				List<int> queued = new List<int>();
				foreach ((int ability, int targetType, int chance) in entries)
				{
					if (ability < 0 || queued.Contains(ability) || queued.Count >= 2) continue;
					if (_random.Next(100) >= chance) continue;
					List<Fighter> targets = MonsterTargets(foe, targetType);
					if (targets.Count == 0 && targetType != 0) continue;
					queued.Add(ability);
					Fighter who = foe;
					Note(foe.Name + " counters (condition " + condition + "): ability " + ability + " on target type " + targetType);
					Action counter = null;
					counter = () =>
					{
						_counterAbilities.Remove(counter);
						float gauge = who.Gauge;   // a counter leaves its own turn where it was
						_isCounter = true;
						Perform(who, ability, targetType, targets.FindAll(f => f.Alive || targetType == 8));
						who.Gauge = gauge;
					};
					_counterAbilities[counter] = ability;
					counters.Add((who, counter));
				}
			}
			_queue.InsertRange(0, counters);
		}
	}
}
