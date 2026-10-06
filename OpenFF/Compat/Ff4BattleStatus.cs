// FF4's battle statuses as libff4.so runs them (ys::Condition, condition_parameter.bbd): a u64 of conditions on each
// fighter; what an action brings or takes away is worked out with it (a weapon's or a monster's blow by its own chance, a
// spell by the magic hit roll, a recovery by what the target has) and committed with its result (doCondition): a bit
// already on goes off, one off comes on - clearing what it replaces, starting its timer, cutting the victim's turn when
// its flag says so. The timers run with the gauges; Poison ticks every 60 frames, Doom counts down, Sleep ends on a hurt.

using System;
using System.Collections.Generic;
using OpenFF.Data;

namespace OpenFF.Client
{
	internal sealed partial class Ff4Battle
	{
		// ys::Condition ids (their names: babil_battle 70300 + id)
		private const int CParalyze = 0, CSleep = 1, CConfuse = 2, CStone = 3, CToad = 4, CSilence = 5, CMini = 6, CBlind = 7, CPoison = 8,
			CKO = 9, CCritical = 0xA, CPetrify = 0xB, CPig = 0xC, CCurse = 0xD, CBerserk = 0xE, CFloat = 0xF, CStop = 0x10, CSlow = 0x11,
			CHaste = 0x12, CReflect = 0x14, CProtect = 0x15, CShell = 0x16, CBlink = 0x18, CSap = 0x20, CMagnetize = 0x21, CDoom = 0x25;
		private const int ConditionCount = 39;

		private static bool Has(Fighter f, int id) => (f.Conditions >> id & 1) != 0;

		private static ConditionParameter ConditionOf(int id) => Ff4Party.Tables != null && Ff4Party.Tables.Conditions.TryGetValue(id, out ConditionParameter c) ? c : null;

		/// <summary>A condition's name (babil_battle 70300 + id).</summary>
		private static string ConditionName(int id) => ConditionOf(id)?.Name ?? ("condition " + id);

		/// <summary>Whether any condition it has carries the flag bit.</summary>
		private static bool AnyFlag(Fighter f, int bit)
		{
			for (int id = 0; id < ConditionCount; id++) if (Has(f, id) && (ConditionOf(id)?.Is(bit) ?? false)) return true;
			return false;
		}

		/// <summary>The gauge stands (canChargeATG: Paralyze, Sleep, Stone, Stop, Magnetize - the flag's bit 3).</summary>
		private static bool GaugeStands(Fighter f) => AnyFlag(f, 3);

		/// <summary>It can act at all (isCanAction: not Paralyze, Sleep, Stone, KO).</summary>
		private static bool CanAct(Fighter f) => f.Alive && (f.Conditions & 0x20BUL) == 0;

		/// <summary>Out of the fight (isNotBattleCondition: Stone, KO, Magnetize) - a side all out is beaten.</summary>
		private static bool OutOfFight(Fighter f) => !f.Alive || (f.Conditions & 0x200000208UL) != 0;

		/// <summary>The gauge's pace under Slow (x0.5) and Haste (x1.5) - atpAddValue.</summary>
		private static float PaceOf(Fighter f) => Has(f, CSlow) ? 0.5f : Has(f, CHaste) ? 1.5f : 1f;

		/// <summary>The statuses it is safe from: a monster's record +0x58, a member's armour (+0x24 of each piece).</summary>
		private static ulong ImmuneTo(Fighter f)
		{
			if (f.IsMonster) return f.Monster?.Raw != null && f.Monster.Raw.Length >= 0x60 ? BitConverter.ToUInt64(f.Monster.Raw, 0x58) : 0;
			ulong mask = 0;
			if (f.Member == null) return 0;
			foreach (int id in f.Member.Equipment)
			{
				ItemDefinition item = id != 0 ? Ff4Party.Tables?.Item(id) : null;
				if (item?.Raw != null && item.Kind == ItemKind.Armour && item.Raw.Length >= 0x2C) mask |= BitConverter.ToUInt64(item.Raw, 0x24);
			}
			return mask;
		}

		/// <summary>Whether the condition may come on (isEnableAddCondition: none of its "blocked by" - itself among them - is on, and it is not resisted).</summary>
		private bool CanAdd(Fighter t, int id, bool checkImmunity = true)
		{
			ConditionParameter c = ConditionOf(id);
			if (c != null && (c.BlockedBy & t.Conditions) != 0) return false;
			if (checkImmunity && !BattleParameterFlag(5) && (ImmuneTo(t) >> id & 1) != 0) return false;
			return true;
		}

		/// <summary>
		/// A blow's statuses (NewAttackFormula::addCondition, only when it lands): the weapon's in each hand (+0x24 the mask,
		/// +0x56 the chance) or the monster's two (+0x30 / +0x38, +0x3C / +0x44) - each its own chance, then every bit of
		/// the low 16 that the target is not safe from and that may come on. A Toad's or Pig's blow brings none.
		/// </summary>
		private ulong BlowConditions(Fighter attacker, Fighter target)
		{
			if (Has(attacker, CToad) || Has(attacker, CPig)) return 0;
			List<(ulong Mask, int Chance)> options = new List<(ulong, int)>();
			if (attacker.IsMonster && attacker.Monster?.Raw != null && attacker.Monster.Raw.Length >= 0x48)
			{
				byte[] r = attacker.Monster.Raw;
				options.Add((BitConverter.ToUInt64(r, 0x30), BitConverter.ToInt16(r, 0x38)));
				options.Add((BitConverter.ToUInt64(r, 0x3C), BitConverter.ToInt16(r, 0x44)));
			}
			else if (attacker.Member != null)
			{
				foreach (int slot in new[] { (int)EquipSlot.RightHand, (int)EquipSlot.LeftHand })
				{
					ItemDefinition item = attacker.Member.Equipment[slot] != 0 ? Ff4Party.Tables?.Item(attacker.Member.Equipment[slot]) : null;
					if (item?.Raw != null && item.Kind == ItemKind.Weapon && item.Raw.Length >= 0x58) options.Add((BitConverter.ToUInt64(item.Raw, 0x24), BitConverter.ToInt16(item.Raw, 0x56)));
				}
			}
			ulong pending = 0;
			foreach ((ulong mask, int chance) in options)
			{
				if (mask == 0 || _random.Next(100) >= chance) continue;
				for (int id = 0; id < 16; id++)
				{
					if ((mask >> id & 1) == 0) continue;
					if (CanAdd(target, id)) pending |= 1UL << id;
				}
			}
			return pending;
		}

		/// <summary>A blow has landed: a confused target comes to (affectActionResult), and the blow's statuses are committed with its damage.</summary>
		private void BlowLands(Fighter attacker, Fighter target)
		{
			ulong pending = BlowConditions(attacker, target);
			if (Has(target, CConfuse)) pending |= 1UL << CConfuse;
			if (pending != 0) Commit(target, pending);
		}

		/// <summary>NewMagicFormula's SPLIT: the hit roll's share for a spell spread over n targets.</summary>
		private static readonly int[] SplitShare = { 4096, 4096, 3277, 2867, 2458, 2048, 1638 };

		/// <summary>
		/// A spell's statuses on one target. A recovery takes away the ones the target has (KO aside - Raise is its own).
		/// Otherwise each condition it names: not on and it must be able to come (not resisted, not blocked - so a second
		/// cast of one already on fails), but a Toad, Pig or Float on comes off again, and Mini on a friend; the ailments
		/// (flag bit 9) at a foe then roll (caster Int + the spell's rate - target Spi - magic evasion, its share when
		/// spread), the rest land.
		/// </summary>
		private ulong SpellConditions(Fighter caster, Fighter t, SpellDefinition spell, int count)
		{
			ulong mask = spell.Conditions;
			if (mask == 0 || t.Mist) return 0;
			ulong pending = 0;
			if (spell.Kind == 1)
			{
				for (int id = 0; id < ConditionCount; id++)
				{
					if (id == CKO || (mask >> id & 1) == 0 || !Has(t, id)) continue;
					pending |= 1UL << id;
				}
				return pending;
			}
			bool foe = caster.IsMonster != t.IsMonster;
			for (int id = 0; id < ConditionCount; id++)
			{
				if ((mask >> id & 1) == 0) continue;
				bool on = Has(t, id);
				if (id == CMini && on && !foe) { pending |= 1UL << id; continue; }
				bool toggle = on && (id == CToad || id == CPig || id == CFloat || id == CMini);
				if (!toggle && !CanAdd(t, id)) continue;
				bool roll = foe || !(id == CToad || id == CPig || id == CMini);
				if (roll && (ConditionOf(id)?.Is(9) ?? false) && !BattleParameterFlag(5))
				{
					int share = (spell.TargetFlags & 0x40) != 0 ? SplitShare[Math.Min(count, SplitShare.Length - 1)] : 4096;
					int chance = share * (caster.Intellect + spell.HitRate - (t.Spirit + t.MagicEvasion)) >> 12;
					if (_random.Next(100) >= chance) continue;
				}
				pending |= 1UL << id;
			}
			return pending;
		}

		/// <summary>
		/// doCondition: the pending bits committed - one on goes off, one off comes on (what it replaces cleared first, its
		/// timer started, its turn cut when its flag's bit 0 says so). KO is the fall; while down only KO is weighed.
		/// </summary>
		private void Commit(Fighter t, ulong pending)
		{
			for (int id = 0; id < ConditionCount && pending != 0; id++)
			{
				if ((pending >> id & 1) == 0) continue;
				if (!t.Alive && id != CKO) continue;
				if (Has(t, id)) ConditionOff(t, id);
				else ConditionOn(t, id);
			}
		}

		private void ConditionOn(Fighter t, int id)
		{
			ConditionParameter c = ConditionOf(id);
			if (id == CKO)
			{
				if (!t.Alive) return;
				t.Hp = 0;
				if (t.Member != null) t.Member.Hp = 0;
				t.Conditions &= c?.Replaces ?? 0;   // KO keeps only what it does not replace (Toad, Mini, Pig, Reverse)
				Fell(t);
				return;
			}
			if (c != null) t.Conditions &= ~(c.Replaces & ~(1UL << id));
			t.Conditions |= 1UL << id;
			t.ConditionTimer[id] = c != null && c.Duration > 0 ? c.Duration : 0;
			if (id == CBlink) t.BlinkCount = 2;
			if (id == CDoom) t.DoomCount = 10 * 4096;
			if (id == CPetrify) t.ConditionTimer[id] = -1;
			Note(t.Name + ": " + ConditionName(id) + " on");
			if (id == CToad || id == CPig || id == CMini) ShowForm(t);
			if ((c?.Is(0) ?? false) || id == CToad) CutTurn(t);
		}

		private void ConditionOff(Fighter t, int id)
		{
			t.Conditions &= ~(1UL << id);
			t.ConditionTimer[id] = 0;
			Note(t.Name + ": " + ConditionName(id) + " off");
			if (id == CToad || id == CPig || id == CMini) ShowForm(t);
			if (t.IsMonster)
			{
				// A monster cured (selectChangeConditionEffect): e680 at its hit spot, by the status.
				int variant = id switch { CPoison => 1, CBlind => 2, CSilence => 3, CPetrify => 4, CConfuse => 5, CSleep => 6, CParalyze => 7, CCurse => 8, _ => 0 };
				if (variant > 0) PlayEffect(680, HitEffectSpot(t), variant);
			}
		}

		/// <summary>
		/// Toad, Pig and Mini as they look (changeFrog, changePig, changeLilliput): a frog (p25_00) or a pig (p41_00) stands
		/// in its place - a member's in its form's colours, p&lt;25 + form&gt;_00 / p&lt;41 + form&gt;_00, a monster's in the first -
		/// its own model hidden; Mini halves it. Back as it was when they go.
		/// </summary>
		private void ShowForm(Fighter t)
		{
			int want = Has(t, CToad) ? 25 : Has(t, CPig) ? 41 : 0;
			if (t.Form != want)
			{
				try { t.FormNpc?.Remove(); } catch (Exception) { }
				t.FormNpc = null;
				t.Form = want;
				if (t.Npc != null) t.Npc.Hidden = want != 0 || t.Mist;
				if (want != 0)
				{
					Vector3 at = t.Npc != null ? t.Npc.Position : Where(t);
					Npc form = Game.Npcs.SpawnModel("p" + want.ToString("00") + "_00", at, 0f);
					if (form != null)
					{
						form.Solid = false;
						int colours = t.Member != null ? PlayerForm[Math.Clamp(t.Member.Id, 0, PlayerForm.Length - 1)] : 0;
						if (colours > 0 && form is LegacyNpc frog) frog.Retexture("p" + (want + colours).ToString("00") + "_00");
						if (want == 41 && t.IsMonster) { try { form.BindMotions("b_monster_pig"); form.PlayMotion(101, true); } catch (Exception) { } }
						if (form is LegacyNpc facing) facing.FaceExactly(t.IsMonster ? t.Facing : t.Facing);
						t.FormNpc = form;
					}
				}
			}
			float scale = Has(t, CMini) ? 0.5f : 1f;
			try { if (t.Npc != null && Math.Abs(t.Npc.Scale - scale) > 0.01f) t.Npc.Scale = scale; } catch (Exception) { }
			try { if (t.FormNpc != null && Math.Abs(t.FormNpc.Scale - scale) > 0.01f) t.FormNpc.Scale = scale; } catch (Exception) { }
		}

		/// <summary>The victim's turn cut (doCondition's interrupt): the gauge emptied, a chosen command and a queued action dropped.</summary>
		private void CutTurn(Fighter t)
		{
			t.Gauge = 0f;
			t.Pending = null;
			t.AtwLeft = 0;
			if (_executing != t) _queue.RemoveAll(e => e.Actor == t);
			t.Queued = _queue.Exists(e => e.Actor == t);
			if (_acting == t) { _acting = null; _pick = Pick.None; }
		}

		/// <summary>
		/// A frame of the conditions (BattleActiveTimeMain, with the gauges): the timers run down and their conditions end;
		/// Poison's count to 60 queues a tick of a hundredth of the maximum; Sap takes 1 HP every 2 frames; Petrify turns to
		/// Stone after 300; Doom counts 10 down over 150 frames to a fall; a sleeper hurt wakes.
		/// </summary>
		private void TickConditions(Fighter f)
		{
			if (!f.Alive) { f.HpSeen = f.Hp; return; }
			if (Has(f, CSleep) && f.Hp < f.HpSeen) ConditionOff(f, CSleep);   // checkRecoverSleep: any hurt wakes
			f.HpSeen = f.Hp;
			if (f.Conditions == 0) return;
			int rate = (int)BattleSpeedRate;
			for (int id = 0; id < ConditionCount; id++)
			{
				if (!Has(f, id) || f.ConditionTimer[id] <= 0 || id == CPetrify) continue;
				f.ConditionTimer[id] = Math.Max(0, f.ConditionTimer[id] - rate);
				if (f.ConditionTimer[id] == 0) ConditionOff(f, id);
			}
			if (Has(f, CPoison) && ++f.PoisonCount >= 60)
			{
				f.PoisonCount = 0;
				if (!_queue.Exists(e => e.Actor == f && _poisonTicks.Contains(e.Act))) { Fighter who = f; Action tick = null; tick = () => { _poisonTicks.Remove(tick); PoisonTick(who); }; _poisonTicks.Add(tick); _queue.Add((f, tick)); }
			}
			if (Has(f, CSap) && (f.SapCount += rate * 2048) >= 4096)
			{
				f.SapCount = 0;
				f.Hp = Math.Max(0, f.Hp - 1);
				if (f.Member != null) f.Member.Hp = f.Hp;
				f.HpSeen = f.Hp;
				if (Has(f, CSleep)) ConditionOff(f, CSleep);
				if (!f.Alive) Fell(f);
			}
			if (Has(f, CPetrify) && ++f.PetrifyCount > 300) { f.PetrifyCount = 0; ConditionOff(f, CPetrify); ConditionOn(f, CStone); }
			if (Has(f, CDoom))
			{
				f.DoomCount -= rate * 4096 / 15;
				if (f.DoomCount <= 0) { f.Conditions &= ~(1UL << CDoom); ConditionOn(f, CKO); }
			}
		}

		/// <summary>A condition put on, or taken off, a fighter from outside (a test drive): committed as an action's would be.</summary>
		public bool ToggleCondition(bool foe, int index, int condition)
		{
			List<Fighter> side = foe ? _foes : _party;
			if (index < 0 || index >= side.Count || condition < 0 || condition >= ConditionCount) return false;
			Commit(side[index], 1UL << condition);
			return true;
		}

		/// <summary>The order the party window shows a member's statuses in (DISPLAY_CONDITION_ID), one at a time.</summary>
		private static readonly int[] DisplayOrder = { 0x21, 3, 0x10, 1, 0, 0x20, 8, 0xB, 5, 7, 0xD, 0x11, 0x1D, 2, 0xE, 0x25, 0x26, 4, 6, 0xC, 0xF, 0x14, 0x15, 0x16, 0x12, 0x17, 0x18, 0x1E, 0x22, 0x23, 0x24, 0x1F };

		/// <summary>The status the party window shows for a member now: the ones it has in FF4's order, the next each second (BattleStatus2DManager::updateCondition).</summary>
		private string StatusShown(Fighter f)
		{
			if (!f.Alive) return "";
			List<int> shown = new List<int>();
			foreach (int id in DisplayOrder) if (Has(f, id) || (id == 0x17 && f.DarkFrames > 0)) shown.Add(id);
			return shown.Count == 0 ? "" : ConditionName(shown[(_clock / 30) % shown.Count]);
		}

		/// <summary>
		/// A member's look under its statuses: the ailing kneel (2001: Paralyze, Sleep, Silence, Blind, Poison, Curse, Sap,
		/// Magnetize, Doom - the flag's bit 5 - or HP at a quarter or less; setConditionMotion) when it stands idle, and the
		/// one effect over it by FF4's priority (changeConditionEffect: e670 - Blink 11, Paralyze or Magnetize 7, Sleep 6,
		/// Petrify 4, Confuse 5, Silence 3, Poison 1, Blind 2; Darkness e260), made again as it ends, gone with the status.
		/// </summary>
		private void ShowConditions()
		{
			foreach (Fighter f in _foes) ShowTint(f);
			foreach (Fighter f in _party)
			{
				ShowTint(f);
				if (!f.Alive) { DropStatusEffect(f); continue; }
				int idle = AnyFlag(f, 5) || f.Hp <= f.MaxHp / 4 ? 2001 : _heroMotionIdle;
				if (!f.Acted && f.IdleMotion != idle) { Play(f, idle, true, 4); f.Acted = false; f.IdleMotion = idle; }
				int pack = 670, variant = Has(f, CBlink) ? 11 : Has(f, CParalyze) || Has(f, CMagnetize) ? 7 : Has(f, CSleep) ? 6 : Has(f, CPetrify) ? 4
					: Has(f, CConfuse) ? 5 : Has(f, CSilence) ? 3 : Has(f, CPoison) ? 1 : Has(f, CBlind) ? 2 : 0;
				if (variant == 0 && f.DarkFrames > 0) { pack = 260; variant = 1; }
				if (variant == 0) { DropStatusEffect(f); continue; }
				GlobalScope.eff.CEffectMng effects = GlobalScope.eff.CEffectMng.instance();
				bool playing = f.StatusEffect >= 0 && effects.isPlay(f.StatusEffect);
				if (playing && f.StatusEffectKind == pack * 100 + variant) { effects.setPosition(f.StatusEffect, Fx(Where(f) + new Vector3(0f, 12f, 0f))); continue; }
				DropStatusEffect(f);
				LoadEffect(pack);
				try
				{
					int made = effects.create(pack, variant);
					if (made < 0) continue;
					effects.enableBoxCulling(made, false);
					effects.setPosition(made, Fx(Where(f) + new Vector3(0f, 12f, 0f)));
					f.StatusEffect = made;
					f.StatusEffectKind = pack * 100 + variant;
				}
				catch (Exception) { }
			}
		}

		/// <summary>BATTLE_CHARACTER_COLOR: red, grey, orange, green, (white), blue - as 5-bit colours.</summary>
		private static readonly (int R, int G, int B)[] TintColours = { (0, 0, 0), (248, 120, 120), (120, 120, 120), (248, 168, 120), (120, 248, 120), (248, 248, 248), (120, 120, 248) };

		/// <summary>
		/// The colour a fighter wears for its statuses (changeColorCondition, the first that holds): a monster grey for
		/// Paralyze, Sleep, Confuse, Silence, Blind, Poison, Curse, Stop, Cry, Slow or Petrify, red Berserk, orange Haste,
		/// green Reflect, white Protect, blue Shell; a member only the last six (its ailments have their effect over it).
		/// </summary>
		private static int TintOf(Fighter f)
		{
			if (!f.Alive) return 0;
			if (f.IsMonster && (Has(f, CParalyze) || Has(f, CSleep) || Has(f, CConfuse) || Has(f, CSilence) || Has(f, CBlind) || Has(f, CPoison) || Has(f, CCurse) || Has(f, CStop) || Has(f, 0x1D))) return 2;
			if (Has(f, CBerserk)) return 1;
			if (Has(f, CSlow) || (f.IsMonster && Has(f, CPetrify))) return 2;
			if (Has(f, CHaste)) return 3;
			if (Has(f, CReflect)) return 4;
			if (Has(f, CProtect)) return 5;
			if (Has(f, CShell)) return 6;
			return 0;
		}

		/// <summary>The tint put on or taken off as the statuses change (setConditionColor / updateConditionColor).</summary>
		private static void ShowTint(Fighter f)
		{
			int want = TintOf(f);
			if (want == f.TintType || !(f.Npc is LegacyNpc model) || model.CharacterId < 0) return;
			try
			{
				GlobalScope.CCharacterMng characters = GlobalScope.characterMng;
				if (f.OwnColours == null) f.OwnColours = characters.saveMaterialColours(model.CharacterId);
				if (want == 0) characters.restoreMaterialColours(model.CharacterId, f.OwnColours);
				else
				{
					(int r, int g, int b) = TintColours[want];
					characters.restoreMaterialColours(model.CharacterId, f.OwnColours);
					characters.tintMaterials(model.CharacterId, (ushort)((r >> 3) | ((g >> 3) << 5) | ((b >> 3) << 10)));
				}
				f.TintType = want;
			}
			catch (Exception) { }
		}

		private static GlobalScope.VecFx32 Fx(Vector3 at) => new GlobalScope.VecFx32((int)Math.Round(at.X * 4096), (int)Math.Round(at.Y * 4096), (int)Math.Round(at.Z * 4096));

		private static void DropStatusEffect(Fighter f)
		{
			if (f.StatusEffect < 0) return;
			try { GlobalScope.eff.CEffectMng.instance().release(f.StatusEffect); } catch (Exception) { }
			f.StatusEffect = -1;
			f.StatusEffectKind = 0;
		}

		private readonly HashSet<Action> _poisonTicks = new HashSet<Action>();

		/// <summary>A poison tick (BABPoisonDamage): a hundredth of the maximum, at least 1, its number shown, between actions.</summary>
		private void PoisonTick(Fighter f)
		{
			if (!f.Alive || !Has(f, CPoison)) return;
			int damage = Math.Max(1, f.MaxHp / 100);
			f.Hp = Math.Max(0, f.Hp - damage);
			if (f.Member != null) f.Member.Hp = f.Hp;
			Pop(DamageSpot(f), damage);
			Note(f.Name + " takes " + damage + " from the poison.");
			if (!f.Alive) Fell(f, damage);
			After(3, () => { });
		}

		/// <summary>Whether it may use the ability with the statuses it has (Ability::isConditionUseful: ability.bbd +0x1C names the ones allowed).</summary>
		private static bool UsableUnder(Fighter f, int ability)
		{
			if (f.Conditions == 0 || Ff4Party.Tables == null || !Ff4Party.Tables.AbilityUsableUnder.TryGetValue(ability, out ulong allowed)) return true;
			return (f.Conditions & ~allowed & ((1UL << ConditionCount) - 1)) == 0;
		}

		/// <summary>
		/// A member who may not choose (Confuse, Berserk): the gauge full, it attacks at once - a Berserker a foe, the
		/// confused one of its own (setAbilityAndTargetForAutoMode, calcNormalAttack's retarget).
		/// </summary>
		private bool ActsAlone(Fighter member)
		{
			if (!Has(member, CConfuse) && !Has(member, CBerserk)) return false;
			List<Fighter> pool = Has(member, CConfuse) ? _party.FindAll(f => f.Alive) : _foes.FindAll(f => f.Alive && !OutOfFight(f));
			if (pool.Count == 0) return true;
			Fighter target = pool[_random.Next(pool.Count)];
			Note(member.Name + " acts on its own (" + (Has(member, CConfuse) ? "confused" : "berserk") + ")");
			Decide(member, () => MemberAttacks(member, target), 0, 1);
			return true;
		}

		/// <summary>
		/// checkRestrictionConditionAction: a monster a status drives - Confuse with Toad, Pig or Berserk strikes its own
		/// side; Berserk, Pig and Toad (all but monster 0x69) strike a member. Null when nothing is forced.
		/// </summary>
		private static (int Ability, int Target)? ForcedMonsterAction(Fighter foe)
		{
			bool confused = Has(foe, CConfuse), berserk = Has(foe, CBerserk), toad = Has(foe, CToad), pig = Has(foe, CPig);
			if (confused && (toad || pig || berserk)) return (1, 2);
			if (berserk || pig) return (1, 1);
			if (toad && foe.Monster?.Id != 0x69) return (1, 1);
			return null;
		}

		/// <summary>The conditions a member brings into a fight (the party's own, kept from the last), and what goes back when it ends (all but the ones the battle's end clears - flag bit 7).</summary>
		private static ulong KeptAfterBattle(ulong conditions)
		{
			ulong cleared = 0;
			for (int id = 0; id < ConditionCount; id++) if (ConditionOf(id)?.Is(7) ?? false) cleared |= 1UL << id;
			return conditions & ~cleared & ~(1UL << CKO);
		}
	}
}
