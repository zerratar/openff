// Re-equip, as Steam's battle offers it at the head of the Items list: the two hands (Right Arm, Left Arm) with what
// each holds; a hand picked, what the bag has that the member can hold there (and nothing, to take it off), the change's
// Attack and Defense shown under it; the pick made, the change is the member's action - the held models, the weapon's
// motions and the member's numbers as the new gear makes them.

using System;
using System.Collections.Generic;
using OpenFF.Data;

namespace OpenFF.Client
{
	internal sealed partial class Ff4Battle
	{
		private const int ReequipEntry = -2;
		private int _castItem;                            // an item that casts an ability (efficacy's CastsAbility), being aimed              // the Items list's head row (two cells of the two-column grid)
		private readonly List<int> _equipChoices = new List<int>();
		private int _hand;                                // 0 the right arm, 1 the left

		/// <summary>The Items list as Steam's: Re-equip first, over the whole first row.</summary>
		private void AddReequipRow()
		{
			_itemChoices.Insert(0, ReequipEntry);
			_itemChoices.Insert(1, ReequipEntry);
		}

		private void UpdateHand(InputState input)
		{
			if (input.Pressed(Pad.Left) || input.Pressed(Pad.Right)) { _hand = 1 - _hand; FillEquipChoices(); }
			if (input.Pressed(Pad.B)) { _pick = Pick.Item; _cursor = 0; _listScroll = 0; return; }
			if (input.Pressed(Pad.A) && _equipChoices.Count > 0) { _pick = Pick.EquipItem; _cursor = 0; _listScroll = 0; }
		}

		private void UpdateEquipItem(InputState input)
		{
			GridMove(input, _equipChoices.Count);
			if (input.Pressed(Pad.B)) { _pick = Pick.Hand; _cursor = 0; _listScroll = 0; return; }
			if (!input.Pressed(Pad.A) || _cursor < 0 || _cursor >= _equipChoices.Count) return;
			Fighter who = _acting;
			int slot = _hand, item = _equipChoices[_cursor];
			Decide(who, () => Reequip(who, slot, item), 0, CmdItems);
		}

		/// <summary>What the bag has for the hand being changed: nothing first (to take it off), then each item that fits there.</summary>
		private void FillEquipChoices()
		{
			_equipChoices.Clear();
			Character c = _acting?.Member;
			if (c == null) return;
			_equipChoices.Add(0);
			foreach (OpenFF.Data.ItemStack s in Ff4Party.Party.Inventory)
				if (Ff4Menu.Fits(Ff4Party.Tables.Item(s.ItemId), c, _hand) && !_equipChoices.Contains(s.ItemId)) _equipChoices.Add(s.ItemId);
		}

		/// <summary>The change made (the member's action): the item from the bag into the hand, what it held back into the bag.</summary>
		private void Reequip(Fighter who, int slot, int itemId)
		{
			Acted(who, CmdItems);
			EndTurn(who);
			Character c = who.Member;
			GameTables tables = Ff4Party.Tables;
			if (c == null || c.Equipment[slot] == itemId) return;
			if (itemId != 0 && !Ff4Party.Party.RemoveItem(itemId, 1)) return;
			WeaponMotionRecord before = tables.WeaponMotion(c.Definition.Id, WeaponSystem(c, tables) == 24 ? 0 : WeaponSystem(c, tables));
			Ff4Party.Party.Equip(c.Id, (OpenFF.Data.EquipSlot)slot, itemId);
			ApplyGear(who, c, tables);
			if (who.Npc is LegacyNpc held && held.CharacterId >= 0)
			{
				// The weapon's poise and swing sets go with the weapon; the held models are made again.
				if (before != null)
				{
					if (before.Poise > 0) EngineApi.UnbindMotions(held.CharacterId, "b_poise" + before.Poise);
					if (before.WeaponSet >= 0) EngineApi.UnbindMotions(held.CharacterId, "b_w" + before.WeaponSet.ToString("00"));
				}
				WeaponMotionRecord weapon = BindBattleMotions(who.Npc, c.Definition.Id, c, tables);
				if (weapon != null) { who.Poise = weapon.Poise; who.SwingA = weapon.Raw[3]; who.SwingB = weapon.Raw[2]; }
				int system = WeaponSystem(c, tables);
				who.HitEffect = HitEffectOf(system);
				if (system >= 0 && system < tables.WeaponSounds.Count && tables.WeaponSounds[system][0] >= 0) { who.HitBank = tables.WeaponSounds[system][0]; who.HitSound = tables.WeaponSounds[system][1]; }
				LoadEffect(who.HitEffect);
				try { Ff4Cutscene.UnbindAll(held.CharacterId); } catch (Exception) { }
				HoldEquipment(who.Npc, c, tables);
			}
			Note(who.Name + " re-equips: " + (itemId != 0 ? tables.Item(itemId)?.Name : "nothing") + " in the " + (slot == 0 ? "right" : "left") + " hand.");
		}

		/// <summary>A member's numbers from its gear (the battle's start and a Re-equip).</summary>
		private static void ApplyGear(Fighter f, Character c, GameTables tables)
		{
			OpenFF.Data.Stats stats = c.StatsWith(tables);
			int weapon = Weapon(c, tables);
			f.Attack = Math.Max(1, weapon > 0 ? weapon : stats.Strength / 2);
			f.Defence = Armour(c, tables);
			f.Agility = Math.Max(1, stats.Agility);
			f.Intellect = stats.Intellect; f.Spirit = stats.Spirit; f.Vitality = stats.Vitality; f.Strength = stats.Strength;
			f.MagicDefence = MagicArmour(c, tables);
			f.HitChance = weapon > 0 ? WeaponHit(c, tables) : 90;
			f.Evade = Evasion(c, tables);
			f.MagicEvasion = MagicEvasionOf(c, tables);
			f.Commands = CommandList(c);
		}

		/// <summary>The hands' panel and the change's numbers (Attack and Defense now and with the picked item).</summary>
		private void FillEquipHud(HudData h)
		{
			EquipData e = h.Equip;
			bool shown = _acting?.Member != null && (_pick == Pick.Hand || _pick == Pick.EquipItem);
			e.Shown = shown;
			if (!shown) return;
			Character c = _acting.Member;
			GameTables tables = Ff4Party.Tables;
			for (int k = 0; k < 2; k++)
			{
				HandRow r = e.Hand[k];
				ItemDefinition held = c.Equipment[k] != 0 ? tables.Item(c.Equipment[k]) : null;
				r.Label = Ff4Menu.SlotName(k) + " Arm";
				r.Name = held?.Name ?? "";
				r.Icon = held?.Icon ?? -1;
				r.Lit = k == _hand;
			}
		}

		/// <summary>The description window's numbers for a candidate (Steam: "Attack: 10→0", the new value red when it is less): Attack and Defense now and with it worn.</summary>
		private void EquipChange(GridData g, int itemId)
		{
			Character c = _acting?.Member;
			GameTables tables = Ff4Party.Tables;
			g.EquipStats = c != null;
			if (c == null) return;
			int attack = Weapon(c, tables), defence = Armour(c, tables);
			int was = c.Equipment[_hand];
			c.Equipment[_hand] = itemId;
			int attack2 = Weapon(c, tables), defence2 = Armour(c, tables);
			c.Equipment[_hand] = was;
			g.AttackFrom = attack.ToString();
			g.AttackTo = attack2 != attack ? attack2.ToString() : "";
			g.AttackArrow = attack2 != attack ? "→" : "";
			g.AttackDown = attack2 < attack;
			g.DefenseFrom = defence.ToString();
			g.DefenseTo = defence2 != defence ? defence2.ToString() : "";
			g.DefenseArrow = defence2 != defence ? "→" : "";
			g.DefenseDown = defence2 < defence;
		}

		/// <summary>The ability an item casts (Red Fang's fire, a Bomb Fragment's), or null for one that mends.</summary>
		private static SpellDefinition CastOf(int itemId)
		{
			ItemDefinition item = itemId > 0 ? Ff4Party.Tables.Item(itemId) : null;
			if (item?.Raw == null) return null;
			// The ability it invokes: its record's s16 at 0x14 (Red Fang's, a Bomb Fragment's), else its efficacy's CastsAbility.
			int ability = item.Raw.Length >= 0x16 ? BitConverter.ToInt16(item.Raw, 0x14) : 0;
			Efficacy e = ability <= 0 && item.EfficacyId > 0 ? Ff4Party.Tables.Efficacy(item.EfficacyId) : null;
			if (ability <= 0 && e != null) ability = e.CastsAbility;
			SpellDefinition spell = ability > 0 ? Ff4Party.Tables.Spell(ability) : null;
			return spell != null && ItemEffect(item) == null ? spell : null;
		}

		/// <summary>Whether a fight has a use for an item: it mends, revives, casts, or is a fang.</summary>
		private static bool ItemUsable(int itemId)
		{
			ItemDefinition item = itemId > 0 ? Ff4Party.Tables.Item(itemId) : null;
			return item != null && (ItemEffect(item) != null || CastOf(itemId) != null || IsFang(itemId));
		}

		/// <summary>calcItemDamage's fangs (ids 5035..5037 - Red, White, Blue): FangFormula, not a spell.</summary>
		private static bool IsFang(int itemId) => itemId >= 5035 && itemId <= 5037;

		/// <summary>FangFormula::damage: half the user's HP, each of the fang's elements the foe is weak to x1.5 and each it resists halved, x1.2 from a member onto a monster.</summary>
		private static int FangDamage(Fighter who, Fighter foe, int elements)
		{
			elements &= ~0x1807;
			Affinity a = AffinityOf(foe);
			long factor = 0x1000;
			for (int bit = 0; bit < 16; bit++)
			{
				if ((elements >> bit & 1) == 0) continue;
				if ((a.Weak >> bit & 1) != 0) factor = factor * 0x1800 >> 12;
				if ((a.Resists >> bit & 1) != 0) factor >>= 1;
			}
			long damage = factor * (Math.Max(0, who.Hp) / 2) >> 12;
			if (who.Member != null && foe.IsMonster) damage = damage * 12 / 10;
			return (int)Math.Clamp(damage, 0, 9999);
		}

		/// <summary>
		/// A fang as Steam shows it: out of the bag, the item motion (62), then its chain-32 record (Red Fang's effect 399
		/// over the foes, 144/4) on every foe as a spell would be shown - no chant, no cost - each foe's number FangFormula's.
		/// </summary>
		private void UseFang(Fighter who, int itemId, List<Fighter> targets)
		{
			ItemDefinition item = Ff4Party.Tables.Item(itemId);
			int elements = item?.Raw != null && item.Raw.Length >= 0x2E ? BitConverter.ToUInt16(item.Raw, 0x2C) : 0;
			SpellDefinition fang = new SpellDefinition { Id = itemId, Name = item?.Name ?? "", School = OpenFF.Data.MagicSchool.Item, TargetFlags = 0x11, Element = elements };
			UseCastItem(who, itemId, fang, targets);
		}

		/// <summary>An item that casts: out of the bag, the item motion (62), and its ability shown as a spell without a chant or a cost.</summary>
		private void UseCastItem(Fighter who, int itemId, SpellDefinition spell, List<Fighter> targets)
		{
			EndTurn(who);
			if (targets.Count == 0 || !Ff4Party.Party.RemoveItem(itemId, 1)) return;
			Acted(who, itemId, targets.ToArray());
			_lastSpell = spell;
			Play(who, 62, false, 3);
			who.Acted = false;
			ShowName(Ff4Party.Tables.Item(itemId)?.Name ?? spell.Name, CastLead);
			After(CastLead, () =>
			{
				Play(who, who.IdleMotion, true, 3);
				ShowSpell(who, spell, targets, invoked: true);
			});
			Note(who.Name + " uses " + (Ff4Party.Tables.Item(itemId)?.Name ?? "an item") + ": " + spell.Name);
		}

		public sealed class HandRow { public bool Lit; public string Label = "", Name = ""; public int Icon = -1; }

		public sealed class EquipData
		{
			public bool Shown;
			public List<HandRow> Hand = new List<HandRow> { new HandRow(), new HandRow() };
		}
	}
}
