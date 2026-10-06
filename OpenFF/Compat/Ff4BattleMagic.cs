// FF4's magic as a turn shows it, read from libff4.so's code: a spell's or a monster ability's numbers worked out as it
// starts (calcBattleParameter) and shown in BattleBehavior::executeCommonMagic's order - the name in the help line and
// the chant (its effect and sound, a member's chant motion) for 24 frames (15 for a counter), then the spell's effect on
// each target in turn, N/2 frames apart (normalMagic's period; a monster ability's from effectsInfo) or one wide effect
// for the side, and the numbers - every target's at once, HP changing with them - only when every effect has ended.

using System;
using System.Collections.Generic;
using System.Numerics;
using OpenFF.Data;

namespace OpenFF.Client
{
	internal sealed partial class Ff4Battle
	{
		private const int CastLead = 24, CounterLead = 15;

		/// <summary>A member's spell: the MP paid, the numbers worked out, then shown.</summary>
		private void Cast(Fighter caster, SpellDefinition spell, List<Fighter> targets)
		{
			Acted(caster, spell.Id, targets.ToArray());
			_lastSpell = spell;
			_casting = null;
			if (caster.Member != null) caster.Member.Mp = Math.Max(0, caster.Member.Mp - spell.MpCost);
			ShowSpell(caster, spell, targets);
			caster.Gauge = 0f;
		}

		/// <summary>A monster's spell or ability (school 5, calcSpecialAttack) on its targets.</summary>
		private void MonsterCasts(Fighter foe, SpellDefinition spell, List<Fighter> targets)
		{
			if (targets.Count == 0) return;
			Acted(foe, spell.Id, targets.ToArray());
			_lastSpell = spell;
			ShowSpell(foe, spell, targets);
		}

		/// <summary>
		/// What a spell does to one target, worked out now and done when the numbers show: healing, a revival, damage by
		/// the magic formula (Self-Destruct's the caster's own HP), or a status (only death carried out yet).
		/// </summary>
		private Action SpellResult(Fighter caster, Fighter t, SpellDefinition spell, int count)
		{
			string name = spell.Name ?? ("spell " + spell.Id);
			if (spell.Heals)
			{
				if (!t.Alive) return null;
				int value = HealingValue(caster, t, spell, count);
				return () =>
				{
					if (!t.Alive) return;
					int before = t.Hp;
					t.Hp = Math.Min(t.MaxHp, t.Hp + value);
					if (t.Member != null) t.Member.Hp = t.Hp;
					Pop(DamageSpot(t), t.Hp - before, true);
					Note(caster.Name + " casts " + name + ": " + t.Name + " +" + (t.Hp - before) + ".");
				};
			}
			if (spell.Revives)
			{
				if (t.Alive) return () => Note(name + " does nothing for " + t.Name + ".");
				return () =>
				{
					t.Hp = spell.Id == 4007 ? t.MaxHp : Math.Max(1, t.MaxHp / 4);
					if (t.Member != null) t.Member.Hp = t.Hp;
					Note(caster.Name + " casts " + name + ": " + t.Name + " rises.");
				};
			}
			if (!t.Alive) return null;
			int damage;
			if (spell.Id == 0x59 || spell.Id == 0x67) damage = caster.Hp;   // Self-Destruct: the caster's HP, and it goes
			else if (spell.Power > 0) damage = AttackMagicDamage(caster, t, spell, count);
			else
			{
				bool hit = _random.Next(100) < spell.HitRate;
				if (hit && (spell.Inflicts & 0x200) != 0 && t.IsMonster) return () => { if (!t.Alive) return; t.Hp = 0; Note(caster.Name + " casts " + name + ": " + t.Name + " is slain."); Fell(t); };
				return () => Note(caster.Name + " casts " + name + " on " + t.Name + (hit ? "." : ": it misses."));
			}
			if (BattleParameterFlag(2)) damage = 99999;
			return () =>
			{
				if (!t.Alive) return;
				t.Hp = Math.Max(0, t.Hp - damage);
				if (t.Member != null) t.Member.Hp = t.Hp;
				Pop(DamageSpot(t), damage);
				Note(caster.Name + " casts " + name + ": " + t.Name + " takes " + damage + ".");
				if (!t.Alive) Fell(t, damage);
			};
		}

		/// <summary>How a spell or a monster's ability is shown: normalMagic's record, or for a monster ability effectsInfo's (by the monster first).</summary>
		private static SpellShow ShowOf(SpellDefinition spell, Fighter caster)
		{
			GameTables t = Ff4Party.Tables;
			if (spell.School == OpenFF.Data.MagicSchool.Enemy && caster.IsMonster && caster.Monster != null && t.MonsterAbilityShowsFor.TryGetValue((spell.Id, caster.Monster.Id), out SpellShow own)) return own;
			if (spell.School == OpenFF.Data.MagicSchool.Enemy && t.MonsterAbilityShows.TryGetValue(spell.Id, out SpellShow ability)) return ability;
			return t.SpellShows.TryGetValue(spell.Id, out SpellShow show) ? show : null;
		}

		/// <summary>The name in the help line for the lead's frames: the ability's own, or for a counter "Counter: name" (babil_battle 70251).</summary>
		private void ShowName(string name, int frames)
		{
			if (string.IsNullOrEmpty(name)) return;
			if (_isCounter) name = BattleText(70251, "Counter: %SCC00%").Replace("%SCC00%", name);
			_help = name;
			_helpUntil = _clock + frames;
			Note("shows " + name);
		}

		/// <summary>
		/// The cast shown (AbilityInvokeBehavior / BattleMonsterBehavior::initializeMagic, then executeCommonMagic): the
		/// name and the chant - 265 white at the caster's hit spot, 266 black at its feet, 286 for a monster's ability,
		/// the sound 100/1 white, 100/2 the rest, 100/0 an ability; a member's chant motion (4004 white, 4005 black) and
		/// its stance after, a monster's own cast motion (its record's 0x94) or the ability's - then after the lead the
		/// spell's effect on each target N/2 frames apart with its sound (one wide effect for a side when the spell takes
		/// it whole), and the results with the numbers once every effect has ended; the turn then waits on the numbers.
		/// </summary>
		private void ShowSpell(Fighter caster, SpellDefinition spell, List<Fighter> targets)
		{
			GameTables tables = Ff4Party.Tables;
			bool ability = spell.School == OpenFF.Data.MagicSchool.Enemy;
			int lead = _isCounter ? CounterLead : CastLead;
			List<Action> results = new List<Action>();
			foreach (Fighter t in targets)
			{
				Action result = SpellResult(caster, t, spell, targets.Count);
				if (result != null) results.Add(result);
			}
			if (spell.Id == 0x59 || spell.Id == 0x67) results.Add(() => { caster.Hp = 0; Fell(caster); });   // Self-Destruct: the caster falls
			ShowName(tables.AbilityTitle(spell.Id) ?? spell.Name, lead);

			int chant = ability ? 286 : spell.School == OpenFF.Data.MagicSchool.White ? 265 : spell.School == OpenFF.Data.MagicSchool.Black ? 266 : spell.School == OpenFF.Data.MagicSchool.Summon ? 267 : -1;
			LoadEffect(chant);
			PlayEffect(chant, chant == 266 ? Where(caster) : HitEffectSpot(caster));
			Game.Audio.PlaySe(100, ability ? 0 : spell.School == OpenFF.Data.MagicSchool.White ? 1 : 2);
			SpellShow show = ShowOf(spell, caster);
			try
			{
				if (!caster.IsMonster)
				{
					Play(caster, spell.School == OpenFF.Data.MagicSchool.White ? 4004 : 4005, false, 5);
					After(lead, () => { if (caster.Alive) Play(caster, _heroMotionIdle, true); });
				}
				else
				{
					int motion = ability ? show?.Motion ?? -1 : caster.Monster?.Raw != null && caster.Monster.Raw.Length >= 0x96 ? (short)BitConverter.ToInt16(caster.Monster.Raw, 0x94) : -1;
					if (motion > 0) { caster.Npc?.PlayMotion(motion, false, ability ? 10 : 2); caster.Acted = true; }
				}
			}
			catch (Exception) { }

			int step = Math.Max(0, (show?.Period ?? 0) / 2);
			if (show == null) Log.First(LogChannel.File, "battle-spell-show-" + spell.Id, 1, () => "battle: no effect record for " + spell.Id + " - shown without its effect");
			else LoadEffect(show.Pack);
			int last = lead;
			if (show != null && spell.HitsAll && targets.Count > 0)
			{
				// One wide effect for the side (drawAllMagicEffect): at (-25, 0, 0) for the monsters', (24, 0, 0) for the party's.
				Vector3 at = !Ff4BattleStage.Active ? HitEffectSpot(targets[0]) : targets[0].IsMonster ? new Vector3(-25f, 0f, 0f) : new Vector3(24f, 0f, 0f);
				After(lead, () => { PlayEffect(show.Pack, at); PlaySe(show); });
			}
			else if (show != null)
			{
				for (int i = 0; i < targets.Count; i++)
				{
					Fighter t = targets[i];
					After(lead + i * step, () => { PlayEffect(show.Pack, SpellSpot(t, show.Mode)); PlaySe(show); });
				}
				last = lead + Math.Max(0, targets.Count - 1) * step;
			}
			void Results()
			{
				if (TurnEffectsPlaying()) { After(1, Results); return; }
				foreach (Action r in results) r();
				WaitNumbers();
			}
			After(last + 1, Results);
		}

		/// <summary>The turn held until the numbers have gone (checkEnd2D).</summary>
		private void WaitNumbers()
		{
			if (_pops.Count > 0) After(1, WaitNumbers);
		}

		private static void PlaySe(SpellShow show)
		{
			if (show.SeBank >= 0 && show.SeNumber >= 0) Game.Audio.PlaySe(show.SeBank, show.SeNumber);
		}

		/// <summary>Where a spell's effect is put on a target (setHitEffectPosition): 0 its hit spot, 1 its feet, 2 its body.</summary>
		private Vector3 SpellSpot(Fighter t, int mode) => mode == 0 ? HitEffectSpot(t) : Where(t);

		/// <summary>
		/// The turns that are only a line or nothing: 60 an empty turn (MABDummyAbility - Mom Bomb's waiting), 3034 a
		/// leader's call for 60 frames (MABDummyMagic). True when the ability was one of them.
		/// </summary>
		private bool QuietTurn(Fighter foe, int ability)
		{
			if (ability == 60) { Note(foe.Name + " waits."); return true; }
			if (ability != 3034) return false;
			int line = (foe.Monster?.Id ?? -1) switch { 34 => 70082, 157 => 70083, 105 => 70139, 161 => 70008, _ => -1 };
			if (line >= 0) { _help = BattleText(line, ""); _helpUntil = _clock + 60; Note(foe.Name + ": " + _help); }
			After(60, () => { });
			return true;
		}

		/// <summary>
		/// Needles (134) and Pincers (62): a plain attack done twice as hard (reviseSting, reviseCounterHorn), its name and
		/// the 286 flash with 100/0 first for 24 frames, then the blow with its own effect and sound - Needles 339 with
		/// 122/1 on the monster's own frames, Pincers 340 with 122/0 at the target's feet (the effect at 4, the sound at 16,
		/// the number at 22) and the monster's motion 202.
		/// </summary>
		private bool PiercingAttack(Fighter foe, int ability, Fighter target)
		{
			if (ability != 134 && ability != 62) return false;
			ShowName(Ff4Party.Tables.AbilityTitle(ability), CastLead);
			LoadEffect(286);
			PlayEffect(286, Where(foe));
			Game.Audio.PlaySe(100, 0);
			MonsterAttackStyle style = ability == 134
				? new MonsterAttackStyle { Pack = 339, SeBank = 122, SeNumber = 1, Factor = 2 }
				: new MonsterAttackStyle { Pack = 340, SeBank = 122, SeNumber = 0, Feet = true, EffectFrame = 4, SeFrame = 16, NumberFrame = 22, Motion = 202, Factor = 2 };
			LoadEffect(style.Pack);
			After(CastLead, () => { if (foe.Alive) MonsterAttack(foe, target.Alive ? target : _party.Find(f => f.Alive) ?? target, style); });
			return true;
		}

		/// <summary>
		/// Alarm (111) and Summon (120), MABEnemySummon: the name and the caller's effect and sound for 24 frames (its free
		/// variable 0 set to 1, but for 0xB4's), then a 10-frame fade to black, one of its candidates at random comes in at the
		/// encounter's slot (battle_parameter chain 24), and the screen comes back over 10. True when the ability was one.
		/// </summary>
		private bool EnemySummon(Fighter foe, int ability)
		{
			if (ability != 111 && ability != 120) return false;
			GameTables tables = Ff4Party.Tables;
			if (foe.Monster == null || !tables.MonsterSummons.TryGetValue(foe.Monster.Id, out MonsterSummon summon) || summon.Candidates.Count == 0)
			{
				Note(foe.Name + " has no one to call");
				return true;
			}
			ShowName(tables.AbilityTitle(ability), CastLead);
			if (foe.Monster.Id != 0xB4) foe.Free[0] = 1;
			if (summon.SeBank >= 0 && summon.SeNumber >= 0) Game.Audio.PlaySe(summon.SeBank, summon.SeNumber);
			LoadEffect(summon.Effect);
			PlayEffect(summon.Effect, Where(foe));
			const int fade = 10;
			After(CastLead, () =>
			{
				GlobalScope.dgs.CFade.Main().fadeOut(fade, GlobalScope.dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
				GlobalScope.dgs.CFade.Sub().fadeOut(fade, GlobalScope.dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
			});
			After(CastLead + fade, () =>
			{
				MonsterDefinition m = tables.Monster(summon.Candidates[_random.Next(summon.Candidates.Count)]);
				if (m != null)
				{
					// The encounter's slot (BattleMonsterParty::addMember): its placement and facing, else beside the caller.
					MonsterPartySlot slot = _eventParty != null && summon.Slot >= 0 && summon.Slot < 6 ? _eventParty.Places[summon.Slot] : null;
					Vector3 at = Ff4BattleStage.Active ? Ff4BattleStage.MonsterSpot(slot != null ? new Vector3(slot.X, slot.Y, slot.Z) : Vector3.Zero, summon.Slot, 3) : foe.Home + new Vector3(0f, 0f, 14f);
					// A spot already taken by a standing monster: the next free one beside it.
					for (int k = 0; k < 6 && _foes.Exists(f => f.Alive && Vector3.Distance(f.Home, at) < 10f); k++) at += new Vector3(0f, 0f, 20f);
					Fighter called = SpawnFoe(m, at, slot != null ? slot.W : (float?)null, Game.Hero.Position);
					if (called != null) { called.Gauge = 0f; Note(foe.Name + " calls " + called.Name + "."); }
				}
				GlobalScope.dgs.CFade.Main().fadeIn(fade);
				GlobalScope.dgs.CFade.Sub().fadeIn(fade);
			});
			After(CastLead + 2 * fade + 1, () => { });
			return true;
		}

		/// <summary>A monster's blow as an ability changes it: the effect pack and sound, where and when, the motion, and a damage factor.</summary>
		private sealed class MonsterAttackStyle
		{
			public int Pack = -1, SeBank = -1, SeNumber = -1, EffectFrame = -1, SeFrame = -1, NumberFrame = -1, Motion = -1, Factor = 1;
			public bool Feet;
		}
	}
}
