// Auto battle in FF4: with it on (C, the key Steam's FF4 shows, or F, as in FF3), a member whose gauge is full gets no
// command window. OpenFF's gambits (Gambits.cs) come first - its rules read top to bottom against the fight as it stands,
// the first whose condition finds a target the action can be used on is what it does: an attack, a defend, a spell, an
// item, the run. None holds (or it has none): its auto-battle command, as Steam's - the one Abilities sets (list 5,
// Attack from the start): a command on the first foe or on itself, a spell, an item.

using System;
using System.Collections.Generic;
using System.Linq;
using OpenFF.Data;

namespace OpenFF.Client
{
	internal sealed partial class Ff4Battle
	{
		/// <summary>The fighters as the rules see them: a side, HP, the statuses a rule asks about, the front row first.</summary>
		private List<GambitTarget> GambitSide(List<Fighter> side, bool foes)
		{
			List<GambitTarget> list = new List<GambitTarget>();
			for (int i = 0; i < side.Count; i++)
			{
				Fighter f = side[i];
				int status = 0;
				if (Has(f, CPoison)) status |= (int)GambitStatus.Poison;
				if (Has(f, CBlind)) status |= (int)GambitStatus.Blind;
				if (Has(f, CSilence)) status |= (int)GambitStatus.Silence;
				if (Has(f, CMini)) status |= (int)GambitStatus.Mini;
				if (Has(f, CToad)) status |= (int)GambitStatus.Toad;
				if (Has(f, CStone)) status |= (int)GambitStatus.Stone;
				if (!f.Alive) status |= (int)GambitStatus.KO;
				if (Has(f, CSleep)) status |= (int)GambitStatus.Sleep;
				if (Has(f, CParalyze)) status |= (int)GambitStatus.Paralysis;
				if (Has(f, CConfuse)) status |= (int)GambitStatus.Confusion;
				list.Add(new GambitTarget
				{
					Fighter = f, Side = foes ? OpenFF.Client.GambitSide.Foe : OpenFF.Client.GambitSide.Ally,
					Hp = f.Hp, MaxHp = f.MaxHp, Alive = f.Alive && !OutOfFight(f) && !Untargetable(f), Dead = !f.Alive, Stone = Has(f, CStone),
					Status = status, Order = i,
				});
			}
			return list;
		}

		/// <summary>Whether auto battle takes this member's turn: it is on (Steam's takes every member).</summary>
		private static bool AutoTakes(Fighter member) => AutoBattle.On && member.Member != null;

		/// <summary>The member's action by its rules, decided (as the command windows would decide it).</summary>
		private void AutoDecide(Fighter member)
		{
			List<GambitTarget> foes = GambitSide(_foes, true), allies = GambitSide(_party, false);
			GambitTarget self = allies.FirstOrDefault(a => a.Fighter == member);
			List<Gambit> rules = Gambits.For(member.Member.Id);
			for (int i = 0; i < rules.Count; i++)
			{
				Gambit rule = rules[i];
				if (!rule.On || rule.IsEmpty || !Gambits.Conditions.TryGetValue(rule.Condition, out GambitCondition condition)) continue;
				IEnumerable<GambitTarget> side = condition.Side == OpenFF.Client.GambitSide.Foe ? foes : condition.Side == OpenFF.Client.GambitSide.Ally ? allies : (self != null ? new[] { self } : Enumerable.Empty<GambitTarget>());
				foreach (GambitTarget target in condition.Find(side, rule.ConditionParam))
				{
					if (AutoCommit(member, rule, (Fighter)target.Fighter))
					{
						Note("auto battle: " + member.Name + " - rule " + (i + 1) + " " + rule + " on " + ((Fighter)target.Fighter).Name);
						return;
					}
				}
			}
			AutoCommand(member);
		}

		/// <summary>The member's auto-battle command (abilityIDList 5): a command on the first foe or on itself, a spell on the
		/// foes or the party as it is for, an item on the one it helps. One that cannot be used now: an attack.</summary>
		private void AutoCommand(Fighter member)
		{
			int id = member.Member.AutoCommand;
			Fighter foe = _foes.Find(f => f.Alive && !OutOfFight(f) && !Untargetable(f));
			GameTables t = Ff4Party.Tables;
			string what = null;
			if (foe != null && id >= 256 && t.Item(id) == null && t.Spell(id) is SpellDefinition spell) what = AutoSpell(member, spell, foe);
			else if (foe != null && id >= 256 && t.Item(id) != null) what = AutoItem(member, id, foe);
			else if (foe != null) what = AutoAbility(member, id, foe);
			if (what == null)
			{
				if (foe != null) { Decide(member, () => MemberAttacks(member, foe), 0, 1); what = "attack"; }
				else { Decide(member, () => Invoke(member, CmdDefend, () => Defend(member)), 0, CmdDefend); what = "defend"; }
			}
			Note("auto battle: " + member.Name + " - " + what);
		}

		private string AutoAbility(Fighter member, int id, Fighter foe)
		{
			switch (id)
			{
				case CmdFight: return null;
				case CmdDefend: Decide(member, () => Invoke(member, CmdDefend, () => Defend(member)), 0, CmdDefend); return "defend";
				case CmdDarkness: Decide(member, () => Darkness(member), 0, CmdDarkness); return "darkness";
				case CmdJump: Decide(member, () => Invoke(member, CmdJump, () => JumpStart(member, foe)), 0, CmdJump); return "jump";
				case CmdAim: case CmdSteal: case CmdKick: case CmdCry: case CmdAnalyze: case CmdLove: case CmdEyeGouge:
					_abilityCmd = id;
					AbilityOnFoe(member, foe);
					return CommandName(id);
				case CmdFocus: case CmdBrace: case CmdBluff: case CmdPray: case CmdBless:
					_abilityCmd = id;
					AbilityOnSelf(member);
					return CommandName(id);
				case CmdRecall: case CmdCurse: case CmdTsunami: case CmdInferno: case CmdWhirlwind:
					if (!AbilityChosen(member, id) || _pick == Pick.Target) { _abilityCmd = 0; return null; }
					return CommandName(id);
			}
			return null;
		}

		private string AutoSpell(Fighter member, SpellDefinition spell, Fighter foe)
		{
			if (!member.Member.Spells.Contains(spell.Id) && !member.Member.Abilities.Contains(spell.Id)) return null;
			if (!spell.UsableInBattle || member.Member.Mp < MpCostOf(member, spell) || !UsableUnder(member, spell.Id)) return null;
			List<Fighter> targets;
			if (Helps(spell))
			{
				List<Fighter> side = _party.FindAll(f => !OutOfFight(f) && (spell.Revives ? !f.Alive : f.Alive));
				if (side.Count == 0) return null;
				side.Sort((a, b) => (a.Hp * 1000L / Math.Max(1, a.MaxHp)).CompareTo(b.Hp * 1000L / Math.Max(1, b.MaxHp)));
				targets = spell.HitsAll ? side : new List<Fighter> { side[0] };
			}
			else targets = spell.HitsAll ? _foes.FindAll(f => f.Alive && !OutOfFight(f)) : new List<Fighter> { foe };
			Decide(member, () => Cast(member, spell, targets), SpellWait(spell), spell.Id);
			return spell.Name;
		}

		private string AutoItem(Fighter member, int id, Fighter foe)
		{
			if (Ff4Party.Party.CountItem(id) <= 0) return null;
			ItemDefinition item = Ff4Party.Tables.Item(id);
			if (IsFang(id)) { Decide(member, () => UseFang(member, id, _foes.FindAll(f => f.Alive && !OutOfFight(f))), ItemWait(id), id); return item.Name; }
			if (CastOf(id) is SpellDefinition casts && !Helps(casts))
			{
				bool all = (Ff4Party.Tables.AbilityTargets(id) & 0x4) != 0;
				Decide(member, () => UseCastItem(member, id, casts, all ? _foes.FindAll(f => f.Alive && !OutOfFight(f)) : new List<Fighter> { foe }), ItemWait(id), id);
				return item.Name;
			}
			Efficacy effect = ItemEffect(item);
			if (effect == null) return null;
			bool revive = effect.Id == 17;
			List<Fighter> side = _party.FindAll(f => !OutOfFight(f) && (revive ? !f.Alive : f.Alive && f.Hp < f.MaxHp));
			if (side.Count == 0) return null;
			side.Sort((a, b) => (a.Hp * 1000L / Math.Max(1, a.MaxHp)).CompareTo(b.Hp * 1000L / Math.Max(1, b.MaxHp)));
			Fighter on = side[0];
			Decide(member, () => UseItem(member, id, on), ItemWait(id), id);
			return item.Name;
		}

		/// <summary>A rule's action on its target, when it can be used there: true once decided.</summary>
		private bool AutoCommit(Fighter member, Gambit rule, Fighter target)
		{
			switch (rule.Action)
			{
				case "attack":
					if (!target.IsMonster || !target.Alive) return false;
					Decide(member, () => MemberAttacks(member, target), 0, 1);
					return true;
				case "guard":
					Decide(member, () => Invoke(member, CmdDefend, () => Defend(member)), 0, CmdDefend);
					return true;
				case "run":
					_runOn = true;
					return false;   // the run control on; the member still acts by the next rule (FF4 holds the run, it is no command)
				case "magic":
				{
					SpellDefinition spell = Ff4Party.Tables.Spell(rule.ActionParam);
					if (spell == null || !member.Member.Spells.Contains(spell.Id) && !member.Member.Abilities.Contains(spell.Id)) return false;
					if (member.Member.Mp < MpCostOf(member, spell) || !UsableUnder(member, spell.Id)) return false;
					bool onFoes = target.IsMonster;
					if (spell.Revives ? target.Alive : !target.Alive) return false;
					List<Fighter> side = onFoes ? _foes.FindAll(f => f.Alive) : _party.FindAll(f => spell.Revives || f.Alive);
					List<Fighter> targets = spell.HitsAll ? side : new List<Fighter> { target };
					Decide(member, () => Cast(member, spell, targets), SpellWait(spell), spell.Id);
					return true;
				}
				case "item":
				{
					int id = rule.ActionParam;
					ItemDefinition item = Ff4Party.Tables.Item(id);
					Efficacy effect = item != null ? ItemEffect(item) : null;
					if (effect == null || target.IsMonster || Ff4Party.Party.CountItem(id) <= 0) return false;
					if ((effect.Id == 17) == target.Alive) return false;   // Phoenix Down on the fallen, the rest on the standing
					Decide(member, () => UseItem(member, id, target), ItemWait(id), id);
					return true;
				}
			}
			return false;
		}
	}
}
