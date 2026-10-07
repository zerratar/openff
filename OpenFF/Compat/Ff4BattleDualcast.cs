// Dualcast (command 17, read from libff4.so): the magic menu in its dual mode (BtlMagicMenu, commandAction case 0x11) -
// the member's White Magic then its Black Magic in one list, two spells picked, each with its target; the second only
// among those both costs together leave MP for (isCanUseDoubleMagic), the MP shown less the first's cost
// (getUseDoubleMagicMp). One decision (ability.bbd 17: wait 60); then the two run back to back - the member kept at
// the head of the queue for the second (startBehavior, BBM +0x1718), each paying its own MP.

using System;
using System.Collections.Generic;
using OpenFF.Data;

namespace OpenFF.Client
{
	internal sealed partial class Ff4Battle
	{
		private bool _dualcast;                 // the magic list is Dualcast's
		private SpellDefinition _dualFirst;     // its first pick, once made
		private Action _dualFirstCast;          // and the first spell's cast, its targets with it
		private Action _dualSecond;             // the second half, waiting at the head of the queue

		/// <summary>The dual list: White Magic's spells (if the member has the command), then Black Magic's.</summary>
		private void DualcastChoices()
		{
			_spellChoices.Clear();
			GameTables tables = Ff4Party.Tables;
			foreach (OpenFF.Data.MagicSchool school in new[] { OpenFF.Data.MagicSchool.White, OpenFF.Data.MagicSchool.Black })
			{
				int command = school == OpenFF.Data.MagicSchool.White ? CmdWhiteMagic : CmdBlackMagic;
				if (!_acting.Commands.Contains(command)) continue;
				foreach (int id in _acting.Member.Spells)
					if (tables.Spell(id) is SpellDefinition spell && spell.School == school) _spellChoices.Add(id);
			}
			_listSchool = OpenFF.Data.MagicSchool.White;
		}

		/// <summary>isUseMagic and, picking Dualcast's second, isCanUseDoubleMagic: both costs within the MP.</summary>
		private bool SpellAffordable(SpellDefinition spell) => _acting != null && _acting.Mp >= MpCostOf(_acting, spell) + (_dualcast && _dualFirst != null ? MpCostOf(_acting, _dualFirst) : 0);

		/// <summary>A spell and its target picked: decided - or, Dualcast's first, kept and the list again for the second.</summary>
		private void DecideSpell(Fighter who, SpellDefinition spell, Action cast)
		{
			if (!_dualcast) { Decide(who, cast, SpellWait(spell), spell.Id); return; }
			if (_dualFirst == null)
			{
				_dualFirst = spell;
				_dualFirstCast = cast;
				_casting = null;
				_pick = Pick.Spell;
				_cursor = Math.Max(0, _spellChoices.IndexOf(spell.Id));   // the list as it was left
				Note(who.Name + " dualcasts " + spell.Name + ", then...");
				return;
			}
			Action first = _dualFirstCast, second = cast;
			_dualcast = false;
			_dualFirst = null;
			_dualFirstCast = null;
			Decide(who, () =>
			{
				// The first now; the second next in line, before anyone else's turn - a counter's too (CheckCounters).
				Action half = null;
				half = () => { if (_dualSecond == half) _dualSecond = null; if (who.Alive) second(); };
				_dualSecond = half;
				_queue.Insert(0, (who, half));
				first();
			}, AbilityWait(CmdDualcast), CmdDualcast);
		}
	}
}
