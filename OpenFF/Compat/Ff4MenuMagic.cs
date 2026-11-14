// The menu's casting (world::MSSMagic, itm::ItemUse::useMagicInField): Enter on a spell the menu can cast - Cure, Esuna,
// Raise, Float, Mini, Toad... - when the caster may (pl::Player::isUseMagic: the record's menu bit, its MP, the statuses it
// may be cast under) goes to the party's places (stMSSMPrcMgctoChr: the first lit, Left to all when the spell may spread -
// the record's +0x20 bit 3); Enter there casts it (ipMSSMBhvCharaDecide) and stays for the next. What it does is libff4's:
//   - a curing spell (+0x16 bit 0) takes its conditions (+0x18) off the target, Raise (4006) leaving a tenth of the HP and
//     Arise (4007) all of it; any other toggles them on and off (Float, Mini, Toad, Pig) on whoever is standing;
//   - a curing spell that does not lift KO heals too (useHpRecoverMagic): (the target's vitality / 8 + the caster's spirit
//     / 2) x the power - all of them, each by (90 - 5 x the members standing)% when more than one stands; nothing for the
//     fallen or stone, nor for anyone already full.
// Nothing taken: the error sound (SE 0, 0) and no MP spent. Taken: the cost off the caster (half with MP Efficiency, as in
// battle), the spell's sound (+10 bank, +12 id). Not done yet: the effect over the places (efp 0x3B6) and the field model a
// Toad, Mini or Pig turns into.

using System;
using OpenFF.Data;

namespace OpenFF.Client
{
	internal sealed partial class Ff4Menu
	{
		private int _usingSpell;
		private bool _spellAll;

		/// <summary>The MP a cast costs the member: the record's, halved by MP Efficiency (isUseMagic's ability 0x51) above 1.</summary>
		private static int MenuMpCost(Character c, SpellDefinition spell) =>
			spell.MpCost > 1 && Ff4Augments.Has(c, Ff4Augments.MpEfficiency) ? spell.MpCost >> 1 : spell.MpCost;

		/// <summary>pl::Player::isUseMagic for the menu: usable here, affordable, standing and allowed under its statuses.</summary>
		private static bool CanCastHere(Character c, SpellDefinition spell)
		{
			if (spell == null || !spell.UsableInMenu || !c.Alive || c.Mp < MenuMpCost(c, spell)) return false;
			if (c.Conditions == 0 || Ff4Party.Tables == null || !Ff4Party.Tables.AbilityUsableUnder.TryGetValue(spell.Id, out ulong allowed)) return true;
			return (c.Conditions & ~allowed & ((1UL << 39) - 1)) == 0;
		}

		private static int Flags(SpellDefinition spell) => spell.Raw != null && spell.Raw.Length >= 0x18 ? BitConverter.ToUInt16(spell.Raw, 0x16) : 0;

		/// <summary>Enter on a spell other than Sight: to the party's places, or the error sound.</summary>
		private bool StartSpell(int spellId)
		{
			SpellDefinition spell = Ff4Party.Tables?.Spell(spellId);
			if (!CanCastHere(Member, spell))
			{
				Game.Audio.PlaySe(0, 0);
				return true;
			}
			Game.Audio.PlaySe(0, 1);
			_usingSpell = spellId;
			_spellAll = spell.HitsAll;
			_pick = 0;
			_mode = Mode.SpellTarget;
			return true;
		}

		private void UpdateSpellTarget(InputState input)
		{
			SpellDefinition spell = Ff4Party.Tables?.Spell(_usingSpell);
			int n = Ff4Party.Party.Members.Count;
			if (input.Pressed(Pad.B) || spell == null || n == 0) { Game.Audio.PlaySe(0, 2); _mode = Mode.Browse; return; }
			if (!spell.HitsAll)
			{
				if (!_spellAll && (input.Pressed(Pad.Up) || input.Pressed(Pad.Down)))
				{
					_pick = (_pick + (input.Pressed(Pad.Up) ? n - 1 : 1)) % n;
					Game.Audio.PlaySe(0, 3);
				}
				if (spell.CanSpread && input.Pressed(Pad.Left) && !_spellAll) { _spellAll = true; Game.Audio.PlaySe(0, 3); }
				if (input.Pressed(Pad.Right) && _spellAll) { _spellAll = false; Game.Audio.PlaySe(0, 3); }
			}
			if (!input.Pressed(Pad.A)) return;
			Character caster = Member;
			string said = CanCastHere(caster, spell) ? CastSpell(caster, spell, _spellAll ? null : Ff4Party.Party.Members[_pick]) : null;
			if (said == null) { Game.Audio.PlaySe(0, 0); return; }
			caster.Mp = Math.Max(0, caster.Mp - Math.Min(MenuMpCost(caster, spell), caster.MaxMp));   // settingForUsingMagic
			if (spell.Raw != null && spell.Raw.Length >= 0x0E) Game.Audio.PlaySe(BitConverter.ToUInt16(spell.Raw, 10), BitConverter.ToUInt16(spell.Raw, 12));
			Notice(said);
			Log.Write(LogChannel.File, "menu: " + caster.Name + " casts " + said);
		}

		private static bool HasCondition(Character c, int id) => id == 9 ? !c.Alive || (c.Conditions >> 9 & 1) != 0 : (c.Conditions >> id & 1) != 0;

		/// <summary>useMagicInField on one member, or all of them (null): what it did, or null when it did nothing.</summary>
		private static string CastSpell(Character caster, SpellDefinition spell, Character target)
		{
			string name = Ff4Party.Tables.AbilityTitle(spell.Id) ?? spell.Name ?? ("spell " + spell.Id);
			bool curing = (Flags(spell) & 1) != 0;
			bool lifts = (spell.Conditions >> 9 & 1) != 0;
			var party = Ff4Party.Party.Members;
			int standing = 0;
			foreach (Character m in party) if (m.Alive) standing++;
			int share = target != null || standing <= 1 ? 0x1000 : (0x5A000 - standing * 0x5000) / 100;
			System.Text.StringBuilder did = new System.Text.StringBuilder();
			foreach (Character t in party)
			{
				if (target != null && t != target) continue;
				string what = null;
				// useConditionMagic
				bool changed = false;
				for (int id = 0; id < 39; id++)
				{
					if ((spell.Conditions >> id & 1) == 0) continue;
					if (curing)
					{
						if (!HasCondition(t, id)) continue;
						t.Conditions &= ~(1UL << id);
						changed = true;
						if (id == 9 && (spell.Id == 4006 || spell.Id == 4007)) t.Hp = Math.Max(1, spell.Id == 4007 ? t.MaxHp : t.MaxHp / 10);
					}
					else if (t.Alive)
					{
						t.Conditions ^= 1UL << id;   // isEnableAddCondition: on whoever stands
						changed = true;
					}
				}
				if (changed) what = curing ? (lifts ? " rises" : " is cured") : " is changed";
				// useHpRecoverMagic: not for the fallen or stone, nor the full.
				if (curing && !lifts && t.Alive && !HasCondition(t, 3) && t.Hp < t.MaxHp)
				{
					OpenFF.Data.Stats body = t.StatsWith(Ff4Party.Tables), mind = caster.StatsWith(Ff4Party.Tables);
					int vitality = HasCondition(t, 4) || HasCondition(t, 6) ? 1 : body.Vitality;   // bodyAndBonus: a Toad or Mini's is 1
					long value = (long)(vitality / 8 + mind.Spirit / 2) * spell.Power * share >> 12;
					if (value > 0)
					{
						int before = t.Hp;
						t.Hp = (int)Math.Min(t.MaxHp, t.Hp + value);
						what = (what ?? "") + " +" + (t.Hp - before) + " HP";
					}
				}
				if (what != null) did.Append(did.Length > 0 ? ", " : "").Append(t.Name).Append(what);
			}
			return did.Length == 0 ? null : name + ": " + did + ".";
		}

		/// <summary>The party's places for a spell, in the Inventory's using screen (Steam's MgctoChr: the list's window).</summary>
		private void FillSpellTarget(Ff4MenuHud.Data h)
		{
			GameTables tables = Ff4Party.Tables;
			SpellDefinition spell = tables?.Spell(_usingSpell);
			h.Title = T(50003, "Magic");
			h.Using = true;
			h.UseName = spell == null ? "" : tables.AbilityTitle(spell.Id) ?? spell.Name ?? "";
			h.UseIcon = tables?.AbilityIcon(_usingSpell) ?? -1;
			h.UseCount = Member.Mp + "/" + Member.MaxMp;
			h.Help = spell != null && tables.AbilityHelpIds.TryGetValue(spell.Id, out int helpId) && helpId > 0 ? (tables.AbilityName(helpId)?.TrimEnd() ?? "").Replace("", "") : "";
			h.ItemScroll.Shown = false;
			FillMembers(h.Target, _spellAll ? -1 : _pick, false);
			if (_spellAll) foreach (Ff4MenuHud.MemberRow row in h.Target) row.Lit = row.Present;
		}
	}
}
