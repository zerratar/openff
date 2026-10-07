// The augments' passives in a fight (Ff4Augments; each works while it sits in a member's command slots - isCommand),
// read from libff4.so: Last Stand (physicsDefense / magicDefense x2 at a quarter of HP or less), Adrenaline (damage and
// the critical chance x2 there), Limit Break (the 9999 limit 99999), Reach (the back row strikes as the front),
// Fast Talker (a magic command's wait halved), MP Efficiency (a cost over 1 halved), Piercing Magic (Reflect passed),
// Draw Attacks (a monster's one random member is one who has it), Item Lore (items x2), Counter (a counterable action
// on it struck back with Attack, every time), Auto-Potion (a Potion drunk when hit, while the bag has one), Phoenix
// (fallen with MP left: the other fallen up with HP by its MP's share, its MP spent), Gil Farmer, Level Lust and
// Treasure Hunter (the win's gil and experience x1.5, the drop odds x2).

using System;
using System.Collections.Generic;
using OpenFF.Data;

namespace OpenFF.Client
{
	internal sealed partial class Ff4Battle
	{
		/// <summary>isCommand: a member (not a monster, not one fighting as a member) with the augment in a slot.</summary>
		private static bool Augment(Fighter f, int ability) => f?.Member != null && Ff4Augments.Has(f.Member, ability);

		/// <summary>The augments' "low HP": a quarter of the maximum or less.</summary>
		private static bool LowHp(Fighter f) => f.Hp <= f.MaxHp / 4;

		/// <summary>limitDamage: 9999, Limit Break's 99999.</summary>
		private static int DamageLimit(Fighter attacker) => Augment(attacker, Ff4Augments.LimitBreak) ? 99999 : 9999;

		/// <summary>spendMp: MP Efficiency halves a cost over 1.</summary>
		private static int MpCostOf(Fighter f, SpellDefinition spell) => spell.MpCost > 1 && Augment(f, Ff4Augments.MpEfficiency) ? spell.MpCost >> 1 : spell.MpCost;

		/// <summary>provocationPlayer: Draw Attacks - a monster's one random member is one of those who have it.</summary>
		private List<Fighter> DrawnAttack(List<Fighter> members)
		{
			List<Fighter> drawing = members.FindAll(f => Augment(f, Ff4Augments.DrawAttacks));
			return drawing.Count > 0 ? new List<Fighter> { drawing[_random.Next(drawing.Count)] } : null;
		}

		// The party's HP as the action began, to know who it hurt (Auto-Potion).
		private readonly Dictionary<Fighter, int> _hpBefore = new Dictionary<Fighter, int>();

		private void NoteHpBefore()
		{
			_hpBefore.Clear();
			foreach (Fighter f in _party) _hpBefore[f] = f.Hp;
		}

		/// <summary>
		/// cheakPlayerCounter, cheakPlayerAutoPotion and cheakPhoneix as an action's turn ends: Counter - each member it
		/// was aimed at that has it strikes the monster back with Attack (no roll); Auto-Potion - each member it hurt
		/// drinks a Potion, while the bag has one; Phoenix - see FellPhoenix. Queued before anyone's next turn.
		/// </summary>
		private void CheckMemberReactions(Fighter actor, int ability)
		{
			List<(Fighter, Action)> reactions = new List<(Fighter, Action)>();
			bool counterable = actor != null && actor.IsMonster && actor.Alive && (Ff4Party.Tables.AbilityFlags(ability) & 0x100) != 0;
			HashSet<Fighter> seen = new HashSet<Fighter>();
			foreach (Fighter m in _lastTargets)
			{
				if (!seen.Add(m) || m.IsMonster || m == actor || !m.Alive || m.Hiding || m.Airborne || !CanAct(m)) continue;
				if (counterable && Augment(m, Ff4Augments.Counter))
				{
					Fighter who = m, foe = actor;
					Note(who.Name + " counters " + foe.Name);
					reactions.Add((who, () =>
					{
						float gauge = who.Gauge;   // a counter leaves its own turn where it was
						_isCounter = true;
						MemberAttacks(who, foe.Alive ? foe : FirstAlive(_foes));
						who.Gauge = gauge;
					}));
				}
			}
			int potions = Ff4Party.Party.CountItem(Potion);   // each drink reserves one
			foreach (Fighter m in _party)
			{
				if (potions <= 0) break;
				if (m == actor || !m.Alive || !Augment(m, Ff4Augments.AutoPotion) || !_hpBefore.TryGetValue(m, out int before) || m.Hp >= before) continue;
				potions--;
				Fighter who = m;
				Note(who.Name + " reaches for a Potion (Auto-Potion)");
				reactions.Add((who, () => { float gauge = who.Gauge; UseItem(who, Potion, who); who.Gauge = gauge; }));
			}
			if (reactions.Count > 0) _queue.InsertRange(0, reactions);
		}

		private const int Potion = 5001;

		/// <summary>
		/// cheakPhoneix / PhoenixBehavior: a fallen member with Phoenix and MP left raises the other fallen - each with its
		/// maximum HP x MP / maximum MP - and its MP is spent; it stays down. True when one did.
		/// </summary>
		private bool FellPhoenix()
		{
			foreach (Fighter holder in _party)
			{
				if (holder.Alive || !Augment(holder, Ff4Augments.Phoenix) || holder.Member == null || holder.Member.Mp <= 0 || holder.Member.MaxMp <= 0) continue;
				List<Fighter> fallen = _party.FindAll(f => f != holder && !f.Alive && f.Member != null);
				if (fallen.Count == 0) continue;
				int mp = holder.Member.Mp, maxMp = holder.Member.MaxMp;
				LoadEffect(0x128);
				Game.Audio.PlaySe(3, 0x8C);
				foreach (Fighter f in fallen)
				{
					PlayEffect(0x128, HitEffectSpot(f), 1, holdsTurn: false);
					Fighter up = f;
					After(10, () =>
					{
						int hp = (int)Math.Max(1, Math.Min(up.MaxHp, (long)up.MaxHp * mp / maxMp));
						up.Hp = hp;
						up.Member.Hp = hp;
						Pop(DamageSpot(up), hp, true);
						up.IdleMotion = EnemyPlayerIdle(up);
						Play(up, up.IdleMotion, true, 3);
						Note(up.Name + " rises with Phoenix (" + hp + " HP)");
					});
				}
				holder.Member.Mp = 0;
				Note(holder.Name + "'s Phoenix: " + fallen.Count + " raised");
				return true;
			}
			return false;
		}
	}
}
