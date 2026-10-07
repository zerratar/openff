// The party's own commands as libff4.so runs them (BattlePlayerBehavior's PABs - PABNormalAttack for Aim, PABPrayer,
// PABKick, PABRollUp, PABEndure, PABBluff, PABPrtendToCry, PABExamine, PABRemodeing -, the steal, pitch and remember
// states, BABHide / BABShow, calcPairMagic, decideAbility's instant ones): what each works out as the action starts
// (calcBattleParameter) and how it is shown, frame by frame where the code says. The invoke stage (Invoke, battle_parameter
// chain 1) comes first for all but Defend, Cover and Cease Cover (instant: the gauge back to empty at the decision, no
// action), Hide and Return (their own 40-frame line), and a failure settled up front (Pray's one in ten, Recall's empty
// draw or short MP, a Twincast that fails, the silenced) - that shows its line alone.

using System;
using System.Collections.Generic;
using OpenFF.Data;

namespace OpenFF.Client
{
	internal sealed partial class Ff4Battle
	{
		private const int CmdSteal = 7, CmdCover = 10, CmdDualcast = 17, CmdBardsong = 18, CmdHide = 19, CmdReturn = 20, CmdSalve = 21,
			CmdAim = 25, CmdAnalyze = 28, CmdFocus = 37, CmdThrow = 42, CmdRecall = 52, CmdUpgrade = 54, CmdBrace = 56, CmdCeaseCover = 63,
			CmdPray = 64, CmdTwincast = 65, CmdCry = 66, CmdBluff = 67, CmdKick = 68, CmdBless = 69, CmdNinjutsu = 0x53;
		private const int CCry = 0x1D;

		/// <summary>btl::ROLLUP_RATE: Focus's charge, x1, x2, x3.3, x4.5 (fx12).</summary>
		private static readonly int[] RollUpRate = { 4096, 8192, 13517, 18432 };
		/// <summary>btl::KICK_RATE by the kick's targets (fx12): one or none x1, then x0.6 .. x0.2.</summary>
		private static readonly int[] KickRate = { 4096, 4096, 2458, 2048, 1638, 1229, 819 };

		private int _abilityCmd;   // the command whose target or item is being picked (0: none of these)

		/// <summary>The command as the slot shows it: Hide as Return while hidden, Cover as Cease Cover while covering (setAbilityIdList).</summary>
		private static int ShownCommand(Fighter f, int id)
		{
			if (id == CmdHide && f.Hiding) return CmdReturn;
			if (id == CmdCover && f.Covering) return CmdCeaseCover;
			return id;
		}

		/// <summary>
		/// The small line Steam's command window puts under a command (BattleStatus2DManager::helpVariable): Focus's rounds
		/// ("Rounds of focus: N"), Twincast's sync with the best partner (synchroLevel: 0, 50 or 100 %).
		/// </summary>
		private string CommandSub(Fighter who, int id)
		{
			if (who == null) return "";
			if (id == CmdFocus) return "Rounds of focus: " + who.FocusCharge;
			if (id == CmdTwincast)
			{
				int best = 0;
				foreach (Fighter p in _party)
				{
					if (p == who || !p.Alive || p.Member == null || who.Member == null || !p.Commands.Contains(CmdTwincast)) continue;
					float Ratio(int a, int b) => b > 0 ? a / (float)b : 0f;
					int level = (Math.Abs(Ratio(who.Hp, who.MaxHp) - Ratio(p.Hp, p.MaxHp)) < 0.2f ? 1 : 0)
						+ (Math.Abs(Ratio(who.Member.Mp, who.Member.MaxMp) - Ratio(p.Member.Mp, p.Member.MaxMp)) < 0.2f ? 1 : 0);
					best = Math.Max(best, level);
				}
				return "Sync: " + best * 50 + "%";
			}
			return "";
		}

		/// <summary>Whether a command can be chosen now (greyed in the window otherwise): hidden, only Return, Aim and Throw; Upgrade, Salve and Throw with something in the bag for them.</summary>
		private static bool CommandUsable(Fighter who, int id)
		{
			if (who == null) return true;
			if (who.Hiding && id != CmdReturn && id != CmdAim && id != CmdThrow) return false;
			if (id == CmdUpgrade && who.Member != null && !HasWeapon(who.Member)) return false;   // isUsefulAbility 0x36: a weapon in a hand
			if (id == CmdUpgrade || id == CmdSalve || id == CmdThrow)
			{
				foreach (OpenFF.Data.ItemStack s in Ff4Party.Party.Inventory) if (ItemFits(id, Ff4Party.Tables.Item(s.ItemId))) return true;
				return false;
			}
			return true;
		}

		/// <summary>The target window's line under the foe (BattleStatus2DManager's getHitPhysical / steal odds): Steal its chance
		/// (the thief's agility, 0 with nothing left to take), Aim 100 %, Throw its own hit rate, else the blow's accuracy.</summary>
		private string TargetLine(Fighter who, Fighter foe)
		{
			if (who == null) return "";
			if (_abilityCmd == CmdSteal)
			{
				bool has = StealSlots(foe, out _, out _) && !foe.Robbed;
				return "Chance of success: " + (has ? Math.Clamp(who.Agility, 0, 100) : 0) + "%";
			}
			if (_abilityCmd == CmdAim) return "Accuracy: 100%";
			if (_abilityCmd == CmdThrow && Ff4Party.Tables.Item(_usingItem)?.Equip is EquipStats e)
			{
				int rate = e.Hit + who.Agility + 20 - (foe.Defence + 4 + foe.Agility);
				if (Has(who, CBlind)) rate /= 10;
				return "Accuracy: " + Math.Clamp(rate, 0, 100) + "%";
			}
			return "Accuracy: " + Accuracy(who, foe) + "%";
		}

		/// <summary>
		/// The list's description panel as Steam fills it: the name; a white spell's cost and the caster's Spirit, a black or
		/// summon's cost and Intellect, a ninjutsu's cost, power and element; a song's or an item's help (ability.bbd +0xC, the
		/// item's caption); then its targets - Single, Multiple, or Single/Multiple when it can be spread.
		/// </summary>
		private (string, string, string) ListDescription(int id)
		{
			GameTables t = Ff4Party.Tables;
			if (_pick == Pick.Spell && t.Spell(id) is SpellDefinition spell)
			{
				string line = spell.School switch
				{
					OpenFF.Data.MagicSchool.White => spell.MpCost + " MP    Spirit: " + _acting.Spirit,
					OpenFF.Data.MagicSchool.Ninjutsu => spell.MpCost + " MP    Attack: " + spell.Power + (ElementCell(spell.Element) >= 0 ? "    Element:" : ""),
					OpenFF.Data.MagicSchool.Song => t.AbilityHelp(id) ?? "",
					_ => spell.MpCost + " MP    Intellect: " + _acting.Intellect,
				};
				return (spell.Name ?? "", line, "Targets: " + (spell.CanSpread ? "Single/Multiple" : spell.HitsAll ? "Multiple" : "Single"));
			}
			ItemDefinition item = t.Item(id);
			if (item == null) return ("", "", "");
			string help = item.Caption ?? "";
			return (item.Name ?? "", help, "Targets: " + ((t.AbilityTargets(id) & 0x4) != 0 ? "Multiple" : "Single"));   // ability.bbd +0x26: 0x4 all foes
		}

		/// <summary>The icon of an element (the first of its bits WeakElement knows), -1 none.</summary>
		private static int ElementCell(int bits)
		{
			for (int k = 0; k < Ff4Ui.ElementBits.Length; k++) if ((bits & Ff4Ui.ElementBits[k]) != 0) return Ff4Ui.ElementCells[k];
			return -1;
		}

		/// <summary>An element's name for the description (the bits of magic_parameter +0x16; Steam draws its icon).</summary>
		private static string ElementName(int bits)
		{
			if ((bits & 0x20) != 0) return "Fire";
			if ((bits & 0x10) != 0) return "Ice";
			if ((bits & 0x08) != 0) return "Lightning";
			if ((bits & 0x40) != 0) return "Water";
			if ((bits & 0x80) != 0) return "Earth";
			if ((bits & 0x200) != 0) return "Air";
			if ((bits & 0x04) != 0) return "Holy";
			return null;
		}

		/// <summary>The target card for a fighter: name, HP and the elements it is weak to and absorbs (WeakElement's icons).</summary>
		private static void FillCard(CardData card, Fighter f)
		{
			card.Shown = f != null;
			foreach (IconSlot s in card.Weak) s.Cell = -1;
			foreach (IconSlot s in card.Absorb) s.Cell = -1;
			card.WeakText = card.AbsorbText = "";
			if (f == null) return;
			card.Name = f.Name;
			bool known = !f.IsMonster || f.Examined;
			card.Hp = known ? f.Hp.ToString() : "?????";
			if (!known) return;
			Affinity a = AffinityOf(f);
			int w = 0, b = 0;
			for (int k = 0; k < Ff4Ui.ElementBits.Length; k++)
			{
				if ((a.Weak & Ff4Ui.ElementBits[k]) != 0 && w < card.Weak.Count) card.Weak[w++].Cell = Ff4Ui.ElementCells[k];
				if ((a.Absorbs & Ff4Ui.ElementBits[k]) != 0 && b < card.Absorb.Count) card.Absorb[b++].Cell = Ff4Ui.ElementCells[k];
			}
			if (w == 0) card.WeakText = "None";
			if (b == 0) card.AbsorbText = "None";
		}

		/// <summary>Out of everyone's reach: in the air from a Jump, or hidden (isSelectable).</summary>
		private static bool Untargetable(Fighter f) => f.Airborne || f.Hiding;

		/// <summary>A party command chosen in the window (one of these): true when it is taken here.</summary>
		private bool AbilityChosen(Fighter who, int id)
		{
			_abilityCmd = 0;
			if (who.Hiding && id != CmdReturn && id != CmdAim && id != CmdThrow)
			{
				Say(CommandName(id) + " cannot be used while hidden.");   // Ability::isConditionUseful: the usable mask's bit 25
				return true;
			}
			switch (id)
			{
				case CmdAim:
				case CmdSteal:
					_abilityCmd = id; _pick = Pick.Target; _cursor = FirstAliveFoe();
					return true;
				case CmdThrow:
				case CmdSalve:
				case CmdUpgrade:
				{
					_itemChoices.Clear();
					foreach (OpenFF.Data.ItemStack s in Ff4Party.Party.Inventory)
						if (ItemFits(id, Ff4Party.Tables.Item(s.ItemId))) _itemChoices.Add(s.ItemId);
					if (_itemChoices.Count == 0) { Say("Nothing to use."); return true; }
					if (id == CmdUpgrade && !HasWeapon(who.Member)) { Say("No weapon equipped!"); return true; }
					_abilityCmd = id; _pick = Pick.Item; _cursor = 0; _listScroll = 0;
					return true;
				}
				case CmdCover:
					_abilityCmd = id; _pick = Pick.Ally; _cursor = _party.FindIndex(f => f != who && f.Alive);
					if (_cursor < 0) { _abilityCmd = 0; _pick = Pick.Command; _cursor = 0; Say("No one to cover."); }
					return true;
				case CmdCeaseCover:
					// decideAbility: flag 5 off, the gauge back to empty, no action.
					who.Covering = false; who.CoverTarget = null;
					Instant(who);
					Note(who.Name + " stops covering.");
					return true;
				case CmdFocus:
				case CmdBrace:
				case CmdBluff:
				case CmdPray:
				case CmdHide:
				case CmdReturn:
				case CmdBless:
					// The target window on the member alone (Steam: its name, HP, weaknesses), confirmed with A.
					_abilityCmd = id; _pick = Pick.Ally; _cursor = _party.IndexOf(who);
					return true;
				case CmdKick:
				case CmdCry:
				case CmdAnalyze:
					// Steam: the window on the foes ("Target All"), confirmed.
					_abilityCmd = id; _pick = Pick.Target; _cursor = FirstAliveFoe();
					return true;
				case CmdRecall: AbilityMotions(who, "b_pa_040"); Decide(who, () => Recall(who), AbilityWait(id), id); return true;
				case CmdTwincast: AbilityMotions(who, "b_pa_005"); TwincastChosen(who); return true;
			}
			return false;
		}

		/// <summary>A foe picked for the command being chosen.</summary>
		private void AbilityOnFoe(Fighter who, Fighter foe)
		{
			int id = _abilityCmd, item = _usingItem;
			_abilityCmd = 0;
			switch (id)
			{
				case CmdAim: AbilityMotions(who, "b_aim"); Decide(who, () => Invoke(who, CmdAim, () => MemberAttacks(who, foe, aim: true)), AbilityWait(id), id); break;
				case CmdSteal: AbilityMotions(who, "b_pa_008"); Decide(who, () => Invoke(who, CmdSteal, () => Steal(who, foe)), AbilityWait(id), id); break;
				case CmdThrow: AbilityMotions(who, "b_pa_022"); Decide(who, () => Invoke(who, CmdThrow, () => Throw(who, foe, item)), AbilityWait(id), id); break;
				case CmdKick: AbilityMotions(who, "b_pa_057"); Decide(who, () => Invoke(who, CmdKick, () => Kick(who)), AbilityWait(id), id); break;
				case CmdCry: AbilityMotions(who, "b_pa_038"); Decide(who, () => Invoke(who, CmdCry, () => Cry(who)), AbilityWait(id), id); break;
				case CmdAnalyze: Decide(who, () => Invoke(who, CmdAnalyze, () => Analyze(who)), AbilityWait(id), id); break;
			}
		}

		/// <summary>The commands whose target is the member alone (ability.bbd target 0x0010): the window on it, nothing to move to.</summary>
		private static bool SelfOnly(int id) => id == CmdFocus || id == CmdBrace || id == CmdBluff || id == CmdPray || id == CmdHide || id == CmdReturn || id == CmdSalve || id == CmdBless;

		/// <summary>The member confirmed as the target of its own command.</summary>
		private void AbilityOnSelf(Fighter who)
		{
			int id = _abilityCmd;
			_abilityCmd = 0;
			switch (id)
			{
				case CmdFocus: AbilityMotions(who, "b_pa_021"); Decide(who, () => Invoke(who, CmdFocus, () => Focus(who)), AbilityWait(id), id); break;
				case CmdBrace: Decide(who, () => Invoke(who, CmdBrace, () => Brace(who)), AbilityWait(id), id); break;
				case CmdPray: AbilityMotions(who, "b_pa_035"); Decide(who, () => Pray(who), AbilityWait(id), id); break;
				case CmdBluff: AbilityMotions(who, "b_pa_039"); Decide(who, () => Invoke(who, CmdBluff, () => Bluff(who)), AbilityWait(id), id); break;
				case CmdHide: Decide(who, () => Hide(who), AbilityWait(id), id); break;
				case CmdBless: Decide(who, () => Invoke(who, CmdBless, () => Bless(who)), AbilityWait(id), id); break;
				case CmdReturn: Decide(who, () => Return(who), AbilityWait(id), id); break;
				case CmdSalve: { int item = _usingItem; Decide(who, () => Invoke(who, CmdSalve, () => Salve(who, item)), AbilityWait(id), id); break; }
			}
		}

		/// <summary>An ally picked for the command being chosen (Cover).</summary>
		private void AbilityOnAlly(Fighter who, Fighter ally)
		{
			_abilityCmd = 0;
			if (ally == who) { Say(who.Name + " cannot cover themself."); return; }
			// decideAbility: +0x48 the one covered, flag 5 on, the gauge back to empty - no action, no invoke.
			who.Covering = true; who.CoverTarget = ally;
			Instant(who);
			Note(who.Name + " covers " + ally.Name + ".");
		}

		/// <summary>An item picked for the command being chosen: Throw goes on to its target, Salve and Upgrade are decided.</summary>
		private void AbilityItem(Fighter who, int item)
		{
			int id = _abilityCmd;
			if (id == CmdThrow) { _usingItem = item; _pick = Pick.Target; _cursor = FirstAliveFoe(); return; }
			_abilityCmd = 0;
			if (id == CmdSalve) { _abilityCmd = CmdSalve; _usingItem = item; _pick = Pick.Ally; _cursor = _party.IndexOf(who); }   // Steam: "Target All", confirmed
			else if (id == CmdUpgrade) Decide(who, () => Invoke(who, CmdUpgrade, () => Upgrade(who, item)), AbilityWait(id), id);
		}

		/// <summary>addAbilityMotion: the action's own motions, b_pa_&lt;type:000&gt; (or a named set), bound to the member once.</summary>
		private static void AbilityMotions(Fighter who, string set)
		{
			if (who.Npc == null || !who.BoundSets.Add(set)) return;
			try { who.Npc.BindMotions(set); } catch (Exception ex) { Log.Write(LogChannel.General, "battle: motions " + set + ": " + ex.Message); }
		}

		private static int AbilityWait(int id) => Ff4Party.Tables?.AbilityWait(id) ?? 0;

		/// <summary>Defend, Cover, Cease Cover: decided and done at once - the gauge empty again (setATP 0, ATG state 0), nothing queued.</summary>
		private void Instant(Fighter who)
		{
			who.Gauge = 0f;
			who.Queued = false;
			who.Pending = null;
			_acting = null;
			_pick = Pick.None;
			_casting = null;
		}

		/// <summary>The list an item command opens: Throw the throwable weapons (+0x54 bit 0), Salve the consumables flagged 0x80 at +0x12, Upgrade those flagged 0x40.</summary>
		private static bool ItemFits(int command, ItemDefinition item)
		{
			if (item?.Raw == null) return false;
			if (command == CmdThrow) return item.Kind == ItemKind.Weapon && item.Raw.Length > 0x55 && (BitConverter.ToUInt16(item.Raw, 0x54) & 1) != 0;
			if (item.Kind != ItemKind.Consumable || item.Raw.Length < 0x14) return false;
			int flags = BitConverter.ToUInt16(item.Raw, 0x12);
			return command == CmdSalve ? (flags & 0x80) != 0 : command == CmdUpgrade && (flags & 0x40) != 0;
		}

		private static bool HasWeapon(Character c)
		{
			foreach (int slot in new[] { (int)EquipSlot.RightHand, (int)EquipSlot.LeftHand })
				if (c.Equipment[slot] != 0 && Ff4Party.Tables.Item(c.Equipment[slot])?.Kind == ItemKind.Weapon) return true;
			return false;
		}

		/// <summary>A help line held for some frames, the turn with it.</summary>
		private void HoldLine(string line, int frames)
		{
			_help = line;
			_helpUntil = _clock + frames;
			After(frames, () => { });
		}

		// ---- Cover (checkExecuteCover, BattleActionCover) ----

		/// <summary>Who takes a plain blow meant for <paramref name="target"/>: one at random of those covering it, or - it in Critical - of those with Cover.</summary>
		private Fighter CoverMan(Fighter target)
		{
			bool critical = target.Hp <= target.MaxHp / 4;
			List<Fighter> men = _party.FindAll(f => f != target && f.Alive && CanAct(f) && !Untargetable(f)
				&& (critical && f.Commands.Contains(CmdCover) || f.Covering && f.CoverTarget == target));
			return men.Count == 0 ? null : men[_random.Next(men.Count)];
		}

		/// <summary>coverPosition: 6 in front of the one covered, facing as it does; the guard (2002) once.</summary>
		private void CoverStep(Fighter coverer, Fighter under)
		{
			if (coverer.Npc == null) return;
			float yaw = under.Facing * (float)Math.PI / 180f;
			coverer.Npc.Teleport(under.Home + new Vector3((float)Math.Sin(yaw), 0f, (float)Math.Cos(yaw)) * 6f);
			Face(coverer, under.Facing);
			Play(coverer, 2002, false, 3);
			coverer.Acted = false;   // held until it steps back (Steam's frames: 15 frames)
			Note(coverer.Name + " covers " + under.Name + ".");
		}

		private void CoverBack(Fighter coverer)
		{
			if (coverer.Npc == null || !coverer.Alive) return;
			coverer.Npc.Teleport(coverer.Home);
			Face(coverer, coverer.Facing);
			Play(coverer, coverer.IdleMotion, true, 8);
			coverer.Acted = false;
		}

		// ---- Pray (PABPrayer, calcPlayer) ----

		private void Pray(Fighter who)
		{
			Acted(who, CmdPray, _party.FindAll(f => f.Alive).ToArray());
			if (_random.Next(100) >= 90)
			{
				// The failure, settled first: no invoke stage, "Prayer unanswered." for 40 frames.
				HoldLine(BattleText(70204, "Prayer unanswered."), 40);
				Note(who.Name + " prays - unanswered.");
				EndTurn(who);
				return;
			}
			List<(Fighter F, int Hp, int Mp)> heals = new List<(Fighter, int, int)>();
			foreach (Fighter f in _party)
			{
				if (!f.Alive || Untargetable(f)) continue;
				int mp = f.Member != null ? (_random.Next(6) + 5) * f.Member.MaxMp / 100 : 0;
				heals.Add((f, (short)((_random.Next(6) + 10) * f.MaxHp / 100), mp));
			}
			Invoke(who, CmdPray, () =>
			{
				Face(who, who.Facing);
				// The HP numbers all at once; once they are gone, the HP and MP applied and the MP numbers.
				foreach (var h in heals) Pop(DamageSpot(h.F), h.Hp, true);
				void ThenMp()
				{
					if (_pops.Count > 0) { After(1, ThenMp); return; }
					foreach (var h in heals)
					{
						if (!h.F.Alive) continue;
						h.F.Hp = Math.Min(h.F.MaxHp, h.F.Hp + h.Hp);
						if (h.F.Member != null)
						{
							h.F.Member.Hp = h.F.Hp;
							h.F.Member.Mp = Math.Min(h.F.Member.MaxMp, h.F.Member.Mp + h.Mp);
						}
						if (h.Mp > 0) Pop(DamageSpot(h.F), h.Mp, true);
					}
					Note(who.Name + "'s prayer is answered.");
				}
				After(1, ThenMp);
			});
			EndTurn(who);
		}

		// ---- Focus, Brace, Bluff, Cry (PABRollUp, PABEndure, PABBluff, PABPrtendToCry) ----

		private void Focus(Fighter who)
		{
			// rollUpLevelUp: the charge up one, three at most; its effect (670, variant 10) on the member while it lasts.
			Acted(who, CmdFocus, who);
			who.FocusCharge = Math.Min(who.FocusCharge + 1, 3);
			Note(who.Name + " focuses (x" + (RollUpRate[who.FocusCharge] / 4096f).ToString("0.0") + ")");
			EndTurn(who);
		}

		private void Brace(Fighter who)
		{
			// PABEndure: flag 4 on and the guard pose; physical blows a quarter until the member's next decision.
			Acted(who, CmdBrace, who);
			who.Braced = true;
			Play(who, 2002, true, 2);
			Note(who.Name + " braces.");
			EndTurn(who);
		}

		private void Bluff(Fighter who)
		{
			Acted(who, CmdBluff, who);
			if (who.BluffCharge > 0)
			{
				PopWord(DamageSpot(who), Ff4Ui.WordNoEffect);   // createNoEffect: a second Bluff does nothing
				Note(who.Name + " bluffs again - no effect.");
			}
			else
			{
				who.BluffCharge = 2;   // kept through this action (0.4's exemption), spent by the next
				Note(who.Name + " bluffs: intellect doubled for the next action.");
			}
			EndTurn(who);
		}

		private void Cry(Fighter who)
		{
			// calcMagic with magic_parameter 66: Cry (0x1d) on every foe - defence and magic defence halved; no numbers.
			List<Fighter> foes = _foes.FindAll(f => f.Alive && !OutOfFight(f));
			Acted(who, CmdCry, foes.ToArray());
			foreach (Fighter f in foes)
			{
				if (Has(f, CCry)) { PopWord(DamageSpot(f), Ff4Ui.WordNoEffect); continue; }
				Commit(f, 1UL << CCry);
			}
			Note(who.Name + " cries: " + foes.Count + " foe(s) let their guard down.");
			EndTurn(who);
		}

		// ---- Kick (PABKick) ----

		private void Kick(Fighter who)
		{
			List<Fighter> targets = _foes.FindAll(f => f.Alive && !Untargetable(f));
			Acted(who, CmdKick, targets.ToArray());
			EndTurn(who);
			if (targets.Count == 0 || who.Npc == null) return;
			// Worked out first: each target the Attack formula, then x KICK_RATE[targets].
			List<(Fighter T, bool Hit, int Damage)> blows = new List<(Fighter, bool, int)>();
			_critical = false;
			foreach (Fighter t in targets)
			{
				bool hit = Hits(who, t);
				blows.Add((t, hit, hit ? Damage(who, t, kickTargets: targets.Count) : 0));
			}
			bool critical = _critical;
			LoadEffect(294);
			Vector3 home = who.Home;
			Vector3 p = KickPoint();
			// Steam's frames: the kick starts 2 frames after the invoke stage (the stage's end, PABKick's state 0xd).
			KickMoves(who, blows, critical, home, p);
		}

		private void KickMoves(Fighter who, List<(Fighter T, bool Hit, int Damage)> blows, bool critical, Vector3 home, Vector3 p)
		{
			Play(who, 6108, true, 2);
			Face(who, (float)(Math.Atan2(p.X - home.X, p.Z - home.Z) * 180.0 / Math.PI));
			for (int n = 1; n <= 4; n++)
			{
				int k = n;
				After(k, () => who.Npc?.Teleport(home + (p - home) * (k / 5f)));
			}
			After(5, () =>
			{
				who.Npc?.Teleport(p);
				PlayEffect(294, p);
				Game.Audio.PlaySe(0x99, 5);
				if (critical) WhiteFlash();
				Play(who, 6109, false, 2);
				foreach (var b in blows)
				{
					if (!b.Hit) { PopWord(DamageSpot(b.T), Ff4Ui.WordMiss); continue; }
					b.T.Hp = Math.Max(0, b.T.Hp - b.Damage);
					if (b.T.Member != null) b.T.Member.Hp = b.T.Hp;
					BlowLands(who, b.T);
					Pop(DamageSpot(b.T), b.Damage);
					if (!b.T.Alive) Fell(b.T, b.Damage);
				}
				Note(who.Name + " kicks " + blows.Count + " foe(s).");
			});
			for (int n = 1; n <= 8; n++)
			{
				int k = n;
				After(5 + k, () =>
				{
					who.Npc?.Teleport(k < 8 ? p + (home - p) * (k / 8f) : home);
					if (k == 8) Face(who, who.Facing);
				});
			}
			void Settle()
			{
				if (who.Npc != null && !who.Npc.MotionDone) { After(1, Settle); return; }
				Play(who, who.IdleMotion, true, 2);
				who.Acted = false;
			}
			After(14, Settle);
		}

		/// <summary>The spot Kick lands on (battle_parameter chain 23's posture for 0x44): (-15, 0, -5), as Steam's FF4 puts Yang in a normal fight.</summary>
		private static Vector3 KickPoint() => new Vector3(-15f, 0f, -5f);

		// ---- Steal (initializeSteal, StealFormula::calcSteal, executeSteal) ----

		private void Steal(Fighter who, Fighter foe)
		{
			if (!foe.Alive) foe = FirstAlive(_foes);
			Acted(who, CmdSteal, foe != null ? new[] { foe } : new Fighter[0]);
			EndTurn(who);
			if (foe == null) return;
			if (Has(who, CToad)) return;
			int item = -1;
			bool hasItem = StealSlots(foe, out int[] items, out int[] odds);
			if (!foe.Robbed && hasItem && _random.Next(100) < who.Agility)
			{
				int r = _random.Next(100);
				for (int i = 0, sum = 0; i < 3; i++)
				{
					sum += odds[i];
					if (r < sum) { item = items[i]; break; }
				}
			}
			if (item > 0) foe.Robbed = true;
			LoadEffect(277);
			Play(who, 85, false, 3);
			PlayEffect(277, HitEffectSpot(foe));
			Game.Audio.PlaySe(158, 2);
			Fighter target = foe;
			void Message()
			{
				if (who.Npc != null && !who.Npc.MotionDone) { After(1, Message); return; }
				string line;
				if (item > 0)
				{
					Ff4Party.Party.AddItem(item, 1);
					line = BattleText(70217, "Stole %SCC00%!").Replace("%SCC00%", Ff4Party.Tables.Item(item)?.Name ?? ("item " + item));
				}
				else if (target.Robbed || !hasItem) line = BattleText(143, "Enemy has no items!");
				else line = BattleText(70215, "Couldn't steal!");
				Note(who.Name + " steals from " + target.Name + ": " + line);
				Play(who, who.IdleMotion, true, 3);
				who.Acted = false;
				HoldLine(line, 39);
			}
			After(1, Message);
		}

		/// <summary>The monster's three steal slots (AcquisitionParameter at +0x6c: item at +0x10 + 4i, odds at +0x12 + 4i); false when it has none.</summary>
		private static bool StealSlots(Fighter foe, out int[] items, out int[] odds)
		{
			items = new int[3]; odds = new int[3];
			byte[] r = foe.Monster?.Raw;
			bool any = false;
			for (int i = 0; i < 3; i++)
			{
				if (r == null || r.Length < 0x6C + 0x14 + 4 * i) { items[i] = -1; continue; }
				items[i] = BitConverter.ToInt16(r, 0x7C + 4 * i);
				odds[i] = BitConverter.ToInt16(r, 0x7E + 4 * i);
				any |= items[i] > 0;
			}
			return any;
		}

		// ---- Throw (PitchFormula, statePitch) ----

		private void Throw(Fighter who, Fighter foe, int itemId)
		{
			if (!foe.Alive) foe = FirstAlive(_foes);
			ItemDefinition weapon = Ff4Party.Tables.Item(itemId);
			Acted(who, CmdThrow, foe != null ? new[] { foe } : new Fighter[0]);
			EndTurn(who);
			if (foe == null || weapon?.Equip == null || !Ff4Party.Party.RemoveItem(itemId, 1)) return;   // no target: the item back, no turn
			int hitRate = weapon.Equip.Hit + who.Agility + 20 - (foe.Defence + 4 + foe.Agility);
			if (Has(who, CBlind)) hitRate /= 10;
			bool hit = !foe.Mist && _random.Next(100) < Math.Clamp(hitRate, 0, 100);
			int atk = weapon.Equip.Attack;
			long damage = ((long)(atk * 25 / 10) * 1010 + 101000) / 100 * (_random.Next(81) + 100) / 100;
			damage = Math.Min(9999, damage * 12 / 10);
			bool shuriken = itemId == 7401 || itemId == 7402;
			int effect = shuriken ? 224 : 223;
			LoadEffect(effect);
			Play(who, 84, false, 3);
			After(13, () =>
			{
				if (hit) { PlayEffect(effect, HitEffectSpot(foe)); DamageFlash(foe); }   // targetPitchDamageAction: the flash with the hit
				Game.Audio.PlaySe(hit ? 158 : 0x65, hit ? (shuriken ? 4 : 1) : 1);
			});
			void Number()
			{
				if (TurnEffectsPlaying()) { After(1, Number); return; }
				if (!hit) { PopWord(DamageSpot(foe), Ff4Ui.WordMiss); Note(who.Name + " throws " + weapon.Name + " and misses."); return; }
				foe.Hp = Math.Max(0, foe.Hp - (int)damage);
				if (foe.Member != null) foe.Member.Hp = foe.Hp;
				StartDamageAction(foe, (int)damage);
				Pop(DamageSpot(foe), (int)damage);
				Note(who.Name + " throws " + weapon.Name + " at " + foe.Name + " for " + damage + ".");
				if (!foe.Alive) Fell(foe, (int)damage);
			}
			After(14, Number);
			void Settle()
			{
				// Steam's frames: 84 over, the poise until the turn is (the stance back after it - AfterAction).
				if (who.Npc != null && !who.Npc.MotionDone) { After(1, Settle); return; }
				Play(who, who.Poise > 0 ? who.Poise : who.IdleMotion, true, 3);
				who.Acted = false;
			}
			After(14, Settle);
		}

		// ---- Hide / Return (BABHide, BABShow) ----

		private void Hide(Fighter who)
		{
			Acted(who, CmdHide, who);
			EndTurn(who);
			ShowName(CommandName(CmdHide), 40);
			After(40, () =>
			{
				// BattleActionEscape (Steam's frames): 1115, its facing as it was - the run is the motion's own - 3 a frame away
				// for 40 frames; then the stance where it ends, out of the shot (condition 0x19: no one's target).
				Game.Audio.PlaySe(156, 3);
				Play(who, 1115, true, 3);
				float yaw = (who.Facing + 180f) * (float)Math.PI / 180f;
				Vector3 step = new Vector3((float)Math.Sin(yaw), 0f, (float)Math.Cos(yaw)) * 3f;
				for (int k = 1; k <= 40; k++)
				{
					After(k, () => { if (who.Npc != null) who.Npc.Teleport(who.Npc.Position + step); });
				}
				After(41, () =>
				{
					who.Hiding = true;
					Play(who, who.IdleMotion, true, 3);
					who.Acted = false;
					Note(who.Name + " hides.");
				});
			});
		}

		private void Return(Fighter who)
		{
			Acted(who, CmdReturn, who);
			EndTurn(who);
			ShowName(CommandName(CmdReturn), 40);
			After(40, () =>
			{
				// PBAAppear: back in 5 frames from 5 steps of 5 behind its place, running in; then not hidden.
				Game.Audio.PlaySe(156, 4);
				float yaw = who.Facing * (float)Math.PI / 180f;
				Vector3 dir = new Vector3((float)Math.Sin(yaw), 0f, (float)Math.Cos(yaw)) * 5f;
				if (who.Npc != null) { who.Npc.Teleport(who.Home - dir * 5f); who.Npc.Hidden = false; }
				Face(who, who.Facing + 180f);   // Steam's frames: turned about (rootRotation + 0x8000) while it runs in
				Play(who, 1115, true, 3);
				for (int k = 1; k <= 5; k++)
				{
					int left = 5 - k;
					After(k, () => who.Npc?.Teleport(who.Home - dir * left));
				}
				After(6, () =>
				{
					who.Hiding = false;
					Face(who, who.Facing);
					Play(who, who.IdleMotion, true, 0);
					who.Acted = false;
					Note(who.Name + " returns.");
				});
			});
		}

		// ---- Salve (calcDrug: the item on every member, one each) ----

		private void Salve(Fighter who, int itemId)
		{
			List<Fighter> targets = _party.FindAll(f => f.Alive && !Untargetable(f));
			Acted(who, CmdSalve, targets.ToArray());
			ItemDefinition item = Ff4Party.Tables.Item(itemId);
			Efficacy effect = item != null ? ItemEffect(item) : null;
			EndTurn(who);
			if (effect == null) return;
			// Steam's frames: the item motion (62, 30 frames), then the stance; the item on each member as it ends.
			Play(who, 62, false, 3);
			who.Acted = false;   // held on its last frame (ItemLoop) - not the stance when the clip ends
			After(30, () =>
			{
				Play(who, who.IdleMotion, true, 3);
				int used = 0;
				foreach (Fighter t in targets)
				{
					if (!t.Alive || !Ff4Party.Party.RemoveItem(itemId, 1)) continue;
					used++;
					int before = t.Hp;
					if (effect.Hp > 0) t.Hp = Math.Min(t.MaxHp, t.Hp + effect.Hp);
					if (t.Member != null) { t.Member.Hp = t.Hp; if (effect.Mp > 0) t.Member.Mp = Math.Min(t.Member.MaxMp, t.Member.Mp + effect.Mp); }
					if (t.Hp != before) Pop(DamageSpot(t), t.Hp - before, true);
				}
				Note(who.Name + " salves the party with " + used + " " + item.Name + ".");
			});
		}

		// ---- Upgrade (PABRemodeing: the attack's element from the item) ----

		private void Upgrade(Fighter who, int itemId)
		{
			Acted(who, CmdUpgrade, who);
			EndTurn(who);
			ItemDefinition item = Ff4Party.Tables.Item(itemId);
			if (item?.Raw == null || item.Raw.Length < 0x2E || !Ff4Party.Party.RemoveItem(itemId, 1)) return;
			who.ElementOverride = BitConverter.ToUInt16(item.Raw, 0x2C);
			Note(who.Name + " upgrades the weapon with " + item.Name + " (element 0x" + who.ElementOverride.ToString("x") + ").");
		}

		// ---- Analyze (PABExamine: chain 32's record 28 on each target, then the line) ----

		private void Analyze(Fighter who)
		{
			List<Fighter> targets = _foes.FindAll(f => f.Alive && !OutOfFight(f));
			Acted(who, CmdAnalyze, targets.ToArray());
			EndTurn(who);
			foreach (Fighter t in targets) t.Examined = true;   // calcBattleParameter case 0xe: flag 0xd on each
			LoadEffect(278);
			for (int i = 0; i < targets.Count; i++)
			{
				Fighter t = targets[i];
				After(i * 16, () => { PlayEffect(278, HitEffectSpot(t)); Game.Audio.PlaySe(158, 3); });
			}
			After(Math.Max(0, targets.Count - 1) * 16 + 30, () =>
			{
				foreach (Fighter t in targets) Note("analyze: " + t.Name + " " + t.Hp + "/" + t.MaxHp + " HP, weak 0x" + AffinityOf(t).Weak.ToString("x"));
				HoldLine(targets.Count == 0 ? BattleText(70535, "Couldn't analyze!") : targets.Count == 1 ? BattleText(70533, "Analyzed target status!") : BattleText(70534, "Analyzed status of all targets!"), 60);
			});
		}

		// ---- Recall (calcRemember: battle_parameter chain 3's draws) ----

		private void Recall(Fighter who)
		{
			EndTurn(who);
			int spellId = 0;
			if (!Has(who, CSilence))
			{
				byte[] draws = Ff4Party.Tables.BattleChains.Count > 3 ? Ff4Party.Tables.BattleChains[3] : null;
				int r = _random.Next(100), sum = 0;
				for (int i = 0; draws != null && i + 4 <= draws.Length; i += 4)
				{
					sum += BitConverter.ToInt16(draws, i + 2);
					if (r < sum) { spellId = BitConverter.ToInt16(draws, i); break; }
				}
			}
			SpellDefinition spell = spellId > 0 ? Ff4Party.Tables.Spell(spellId) : null;
			if (spell == null || who.Member == null || who.Member.Mp < spell.MpCost)
			{
				// The failure (or too little MP), settled first: no invoke; 6107 once, effect 275 on its second frame, the line for 60 frames.
				Acted(who, CmdRecall);
				bool noMp = spell != null;
				_help = noMp ? BattleText(70189, "Not enough MP!") : BattleText(70000, "Could not recall any spells!");
				_helpUntil = _clock + 60;
				if (!noMp)
				{
					LoadEffect(275);
					Play(who, 6107, false, 3);
					Game.Audio.PlaySe(156, 2);
					After(2, () => PlayEffect(275, HitEffectSpot(who)));
				}
				After(60, () => { Play(who, who.IdleMotion, true, 3); who.Acted = false; });
				Note(who.Name + " recalls " + (noMp ? spell.Name + " - not enough MP" : "nothing"));
				return;
			}
			List<Fighter> targets = AutoTargets(spell);
			Note(who.Name + " recalls " + spell.Name);
			Invoke(who, CmdRecall, () => Cast(who, spell, targets, invoked: true));
		}

		/// <summary>setRememberRetarget: an ally spell on the party, a foe spell on the foes - all of them when it hits all, else one at random.</summary>
		private List<Fighter> AutoTargets(SpellDefinition spell)
		{
			bool allies = Helps(spell);
			List<Fighter> side = (allies ? _party : _foes).FindAll(f => (f.Alive || spell.Revives) && !Untargetable(f) && !OutOfFight(f));
			if (spell.HitsAll || side.Count == 0) return side;
			return new List<Fighter> { side[_random.Next(side.Count)] };
		}

		// ---- Twincast (PAIR_MAGIC_WAIT, cheakPairMagicPartner, calcPairMagic) ----

		private void TwincastChosen(Fighter who)
		{
			// The first to choose it waits, frozen, for a partner; the second pairs them and both wait Twincast's 150.
			Fighter partner = _party.Find(f => f != who && f.TwinWaiting && f.Alive);
			Play(who, 99, true, 3);   // Steam's frames: the pair's wait, from the decision to the invoke
			if (partner == null)
			{
				who.TwinWaiting = true;
				who.Queued = true;
				who.Pending = null;
				who.DecidedAbility = CmdTwincast;
				_acting = null;
				_pick = Pick.None;
				Note(who.Name + " waits for a Twincast partner.");
				return;
			}
			partner.TwinWaiting = false;
			who.TwinPartner = partner;
			Decide(who, () => Twincast(who, partner), AbilityWait(CmdTwincast), CmdTwincast);
		}

		private void Twincast(Fighter who, Fighter partner)
		{
			EndTurn(who);
			// battleBehaved resets the partner too.
			partner.Gauge = 0f;
			partner.Queued = false;
			who.TwinPartner = null;
			if (!partner.Alive || who.Member == null || partner.Member == null) return;
			if (Has(who, CSilence) || Has(partner, CSilence))
			{
				Acted(who, CmdTwincast);
				HoldLine(BattleText(70198, "Cannot use voice!"), 59);
				return;
			}
			float Ratio(int a, int b) => b > 0 ? a / (float)b : 0f;
			int level = (Math.Abs(Ratio(who.Hp, who.MaxHp) - Ratio(partner.Hp, partner.MaxHp)) < 0.2f ? 1 : 0)
				+ (Math.Abs(Ratio(who.Member.Mp, who.Member.MaxMp) - Ratio(partner.Member.Mp, partner.Member.MaxMp)) < 0.2f ? 1 : 0);
			(int spellId, int mp) = PairSpell(who.Member.Id, partner.Member.Id, level);
			SpellDefinition spell = spellId > 0 ? Ff4Party.Tables.Spell(spellId) : null;
			if (spell == null || who.Member.Mp < mp || partner.Member.Mp < mp)
			{
				Acted(who, CmdTwincast);
				HoldLine(spell == null ? BattleText(70205, "Twincasting failed.") : BattleText(70189, "Not enough MP!"), 59);
				Note(who.Name + " and " + partner.Name + ": Twincast fails (level " + level + ")");
				return;
			}
			who.Member.Mp -= mp;
			partner.Member.Mp -= mp;
			List<Fighter> targets = AutoTargets(spell);
			Note(who.Name + " and " + partner.Name + " twincast " + spell.Name + " (level " + level + ", " + mp + " MP each)");
			Invoke(who, CmdTwincast, () =>
			{
				Acted(who, spell.Id, targets.ToArray());
				_lastSpell = spell;
				ShowSpell(who, spell, targets, invoked: true);
				Play(partner, partner.IdleMotion, true, 3);
				partner.Acted = false;
			}, partner);
		}

		/// <summary>battle_parameter chain 26 (14 bytes: two player types, then three of spell s16, MP u16) for the pair, by the synchro level.</summary>
		private static (int Spell, int Mp) PairSpell(int a, int b, int level)
		{
			byte[] c = Ff4Party.Tables.BattleChains.Count > 26 ? Ff4Party.Tables.BattleChains[26] : null;
			for (int i = 0; c != null && i + 14 <= c.Length; i += 14)
			{
				if (!(c[i] == a && c[i + 1] == b || c[i] == b && c[i + 1] == a)) continue;
				int at = i + 2 + 4 * Math.Clamp(level, 0, 2);
				return (BitConverter.ToInt16(c, at), BitConverter.ToUInt16(c, at + 2));
			}
			return (0, 0);
		}

		// ---- Bardsong: the songs that go on (4804 Life's Anthem, 4805 Hastemarch, 4808 Hero's Rime) ----

		private const int SongFrames = 450;

		/// <summary>stateMagic for a continuing song: the singer sings (107, looped) for 450 frames and has no turn meanwhile.</summary>
		private void StartSong(Fighter who, SpellDefinition spell)
		{
			if (spell.Id != 4804 && spell.Id != 4805 && spell.Id != 4808) return;
			who.SongId = spell.Id;
			who.SongTimer = SongFrames * Tick;
			who.SongTick = 0;
			After(1, () => { Play(who, 107, true, 3); who.Acted = true; });
			Note(who.Name + " sings " + spell.Name + " (450 frames)");
		}

		/// <summary>The singing each advancing tick (calcConditionTime, songRhysicalFrameCount): Life's Anthem's mending every 15 frames; the end of the song.</summary>
		private void TickSong(Fighter f)
		{
			if (f.SongId == 0 || f.Hiding || f.Airborne) return;
			f.SongTimer -= SpeedRate;
			if (f.SongId == 4804 && (f.SongTick += SpeedRate) >= 15 * Tick)
			{
				f.SongTick = 0;
				Fighter singer = f;
				if (!_queue.Exists(e => e.Actor == f && _poisonTicks.Contains(e.Act)))
				{
					Action tick = null;
					tick = () => { _poisonTicks.Remove(tick); AnthemTick(singer); };
					_poisonTicks.Add(tick);
					_queue.Add((f, tick));
				}
			}
			if (f.SongTimer > 0) return;
			// applyTimeCondition: the song over - the turn ends (the gauge from empty), the poise, its status off the party.
			int status = f.SongId == 4804 ? 0x1A : f.SongId == 4805 ? 0x1B : 0x1C;
			f.SongId = 0;
			f.Gauge = 0f;
			f.Queued = false;
			Play(f, f.IdleMotion, true, 3);
			f.Acted = false;
			foreach (Fighter m in _party) if (Has(m, status)) ConditionOff(m, status);
			Note(f.Name + "'s song ends.");
		}

		/// <summary>BABSongRhysical: once no number is up, every member standing (not stone, hidden or in the air) mends a twentieth of their HP.</summary>
		private void AnthemTick(Fighter singer)
		{
			void Mend()
			{
				if (_pops.Count > 0) { After(1, Mend); return; }
				foreach (Fighter m in _party)
				{
					if (!m.Alive || Has(m, CStone) || Untargetable(m)) continue;
					int heal = Math.Max(1, m.MaxHp / 20);
					int before = m.Hp;
					m.Hp = Math.Min(m.MaxHp, m.Hp + heal);
					if (m.Member != null) m.Member.Hp = m.Hp;
					Pop(DamageSpot(m), m.Hp - before, true);
				}
			}
			Mend();
		}

		// ---- Bless (PABMentalWave, PABMentalWaveRecover) ----

		private const int BlessPeriod = 45;   // 0x2d000 of battleSpeedRate: a tick every 45 frames

		/// <summary>PABMentalWave: 155/3 and effect 279 at the party's side (24, 0, 0); the caster's flag 0x12 from then on.</summary>
		private void Bless(Fighter who)
		{
			Acted(who, CmdBless, _party.FindAll(f => f.Alive).ToArray());
			EndTurn(who);
			LoadEffect(279);
			PlayEffect(279, new Vector3(24f, 0f, 0f));
			Game.Audio.PlaySe(155, 3);
			who.Blessing = true;
			who.BlessCount = 0;
			Note(who.Name + " blesses the party.");
		}

		/// <summary>checkCondition's Bless: every 45 frames while a member blesses, once no number is up, each ally standing (not hidden, not in the air) gains 5..10 MP.</summary>
		private void TickBless()
		{
			Fighter blesser = _party.Find(f => f.Blessing && f.Alive);
			if (blesser == null || (blesser.BlessCount += SpeedRate) < BlessPeriod * Tick) return;
			blesser.BlessCount = 0;
			if (_queue.Exists(e => e.Actor == blesser && _poisonTicks.Contains(e.Act))) return;
			Action tick = null;
			tick = () =>
			{
				_poisonTicks.Remove(tick);
				void Mend()
				{
					if (_pops.Count > 0) { After(1, Mend); return; }
					foreach (Fighter m in _party)
					{
						if (!m.Alive || Untargetable(m) || m.Member == null) continue;
						int mp = _random.Next(6) + 5;
						int before = m.Member.Mp;
						m.Member.Mp = Math.Min(m.Member.MaxMp, m.Member.Mp + mp);
						if (m.Member.Mp != before) Pop(DamageSpot(m), m.Member.Mp - before, true);
					}
				}
				Mend();
			};
			_poisonTicks.Add(tick);
			_queue.Add((blesser, tick));
		}

		// ---- Smoke (4904): the party gets away, or "Can't escape!" ----

		private bool TrySmoke(Fighter who, SpellDefinition spell)
		{
			if (spell.Id != 4904 && spell.Id != 4022) return false;
			Acted(who, spell.Id);
			EndTurn(who);
			if ((EscapeFlags & 1) == 0)
			{
				HoldLine(BattleText(70222, "Can't escape!"), 59);
				return true;
			}
			if (who.Member != null) who.Member.Mp = Math.Max(0, who.Member.Mp - spell.MpCost);
			Invoke(who, CmdNinjutsu, Escape);   // onPlayerEscape: the party runs
			return true;
		}

		/// <summary>The turn is over: Bluff's charge spent by the action after it, Focus's by the blow that used it.</summary>
		private static void AfterAction(Fighter f)
		{
			// removeAbilityMotion: the action's own sets go with it (eight slots a character - Aim's b_aim left in place filled
			// Rosa's, and Pray's chant had nowhere to go).
			if (f.BoundSets.Count > 0 && f.Npc is LegacyNpc owner && owner.CharacterId >= 0 && !f.Airborne && f.SongId == 0)
			{
				foreach (string set in f.BoundSets) { try { EngineApi.UnbindMotions(owner.CharacterId, set); } catch (Exception) { } }
				f.BoundSets.Clear();
			}
			// Back to its stance (setConditionMotion) when the action left it in another one - the poise of a decision, an invoke's 9999.
			if ((f.Member != null || f.PlayerType >= 0) && f.Alive && !f.Acted && !f.Airborne && !f.Hiding && f.SongId == 0 && f.Npc is LegacyNpc held && held.CharacterId >= 0)
			{
				try { if (GlobalScope.characterMng.getMotionIndex(held.CharacterId) != (uint)f.IdleMotion) f.Npc.PlayMotion(f.IdleMotion, true, 3); } catch (Exception) { }
			}
			if (f.BluffCharge > 0) f.BluffCharge--;
			if (f.FocusSpent) { f.FocusSpent = false; f.FocusCharge = 0; }
		}
	}
}
