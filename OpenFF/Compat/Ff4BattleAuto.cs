// Auto battle in FF4 by OpenFF's gambits (Gambits.cs): with it on (C, the key Steam's FF4 shows, or F, as in FF3), a
// member whose gauge is full gets no command window - its rules are read top to bottom against the fight as it stands
// and the first whose condition finds a target the action can be used on is what it does: an attack, a defend, a spell
// (white, black or summon), an item, the run. None holds: the first foe is attacked. A member with no rule on is the
// player's, its window opening as ever.

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

		/// <summary>Whether auto battle takes this member's turn: it is on and the member has a rule on.</summary>
		private static bool AutoTakes(Fighter member)
		{
			if (!AutoBattle.On || member.Member == null) return false;
			try { return Gambits.For(member.Member.Id).Any(r => r.On && !r.IsEmpty); } catch (Exception) { return false; }
		}

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
			Fighter first = FirstAlive(_foes);
			if (first != null) Decide(member, () => MemberAttacks(member, first), 0, 1);
			else Decide(member, () => Defend(member), 0, 3);
			Note("auto battle: " + member.Name + " - no rule held; " + (first != null ? "attack" : "defend"));
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
