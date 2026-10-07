// FF4's target selector for a spell, an item or Fight (btl::BattleTargetSelector, read from libff4.so, its keys and
// window as Steam's frames show them): the window lists the targets of one side - the foes, or the party - with a
// last row "Target All" when the ability may spread there (ability.bbd +0x26: 0x04 all foes, 0x40 all allies, 0x400 all
// with Omnicasting) and two or more stand; Up and Down move along it, Left goes to the foes and Right to the party
// when the ability may reach that side (0x106 / 0x170), and a key switches between one and all (Steam's Z, "Switch to
// All" / "Switch to Solo"; V here, Z being confirm). It opens where ability.bbd +0x28 says: Cure on the most hurt
// member, Fire on a foe, a Red Fang on every foe (no single allowed: all, not to be narrowed).

using System;
using System.Collections.Generic;
using OpenFF.Data;

namespace OpenFF.Client
{
	internal sealed partial class Ff4Battle
	{
		private int _targetBits;    // the ability's target bits while this selector runs (0: the commands' own picks)
		private int _targetAbility;
		private bool _targetAll;

		// DAT_002bb25c: +0x28's type -> the selector's state (0 one ally, 1 all allies, 2 one foe, 3 all foes, 4 fixed, 5 everyone).
		private static readonly int[] DefaultStates = { 2, 2, 0, 0, 0, 3, 1, 4, 5, 2, 0, 0 };

		/// <summary>The selector opened for an ability (a spell, an item, Fight), where its default type puts it.</summary>
		private void BeginTargeting(int ability, bool revives = false)
		{
			GameTables t = Ff4Party.Tables;
			_targetAbility = ability;
			_targetBits = t.AbilityTargets(ability);
			if (_targetBits == 0) _targetBits = 0x02;
			int type = t.AbilityDefaultTarget(ability);
			int state = type >= 0 && type < DefaultStates.Length ? DefaultStates[type] : 2;
			bool foes = state == 2 || state == 3 || state == 5;
			if (revives && _party.Exists(f => !f.Alive)) foes = false;   // a revival opens on the fallen
			if (foes && (_targetBits & 0x106) == 0) foes = false;
			if (!foes && (_targetBits & 0x170) == 0) foes = true;
			_pick = foes ? Pick.Target : Pick.Ally;
			bool wantAll = state == 1 || state == 3 || state == 5;
			_targetAll = wantAll && (Forced(foes) || CanSpread(foes));
			// The member's last decision (initialize 96576-96613): all of that side again while two or more stand there, else
			// the one it picked if it still can be - when it lies on the side this ability opens on.
			Fighter last = _acting?.LastTarget;
			if (_acting != null && _acting.LastTargetFoes == foes && !Forced(foes) && (_acting.LastTargetAll || last != null))
			{
				if (_acting.LastTargetAll && CanSpread(foes)) { _targetAll = true; _cursor = foes ? FirstAliveFoe() : Math.Max(0, _party.IndexOf(_acting)); return; }
				if (!_acting.LastTargetAll && last != null && SideList(foes).Contains(last) && (last.Alive || !foes) && !wantAll)
				{
					_cursor = foes ? _foes.IndexOf(last) : _party.IndexOf(last);
					return;
				}
			}
			if (foes) _cursor = FirstAliveFoe();
			else
			{
				List<Fighter> side = SideList(false);
				Fighter pick = revives ? side.Find(f => !f.Alive)
					: type == 2 || type == 7 ? _acting
					: side.FindAll(f => f.Alive).Count > 0 ? MostHurt(side.FindAll(f => f.Alive), type == 11) : _acting;
				_cursor = Math.Max(0, _party.IndexOf(pick ?? _acting));
			}
		}

		private static Fighter MostHurt(List<Fighter> side, bool byShare)
		{
			Fighter best = side[0];
			foreach (Fighter f in side)
			{
				bool worse = byShare ? (long)f.Hp * best.MaxHp < (long)best.Hp * f.MaxHp : f.MaxHp - f.Hp > best.MaxHp - best.Hp;
				if (worse) best = f;
			}
			return best;
		}

		private void EndTargeting()
		{
			_targetBits = 0;
			_targetAll = false;
		}

		/// <summary>The side's targets as the window lists them.</summary>
		private List<Fighter> SideList(bool foes) => foes ? _foes.FindAll(f => f.Alive && !OutOfFight(f)) : _party.FindAll(f => !Untargetable(f));

		/// <summary>isValidTargetingAllEnemy / allies: the ability may spread there, and two or more stand on it.</summary>
		private bool CanSpread(bool foes)
		{
			bool omni = Augment(_acting, Ff4Augments.Omnicasting) && (_targetBits & 0x400) != 0;
			bool may = foes ? (_targetBits & 0x04) != 0 || omni : (_targetBits & 0x40) != 0 || omni;
			return may && (foes ? SideList(true).Count : SideList(false).FindAll(f => f.Alive).Count) > 1;
		}

		/// <summary>No single target allowed (the default all, +0x26 without 0x02 / 0x20): all, not to be narrowed.</summary>
		private bool Forced(bool foes) => (_targetBits & 0x22) == 0 && (foes ? (_targetBits & 0x04) != 0 : (_targetBits & 0x40) != 0);

		private bool OnFoes => _pick == Pick.Target;

		private Fighter TargetUnderCursor() => OnFoes ? (_cursor >= 0 && _cursor < _foes.Count ? _foes[_cursor] : null) : (_cursor >= 0 && _cursor < _party.Count ? _party[_cursor] : null);

		/// <summary>A frame of the selector: the rows, the sides, the switch, the decision.</summary>
		private void UpdateTargeting(InputState input)
		{
			bool foes = OnFoes;
			List<Fighter> side = SideList(foes);
			bool forced = Forced(foes), spread = !forced && CanSpread(foes);
			if (side.Count == 0) { EndTargeting(); _pick = Pick.Command; return; }
			if (!forced && (input.Pressed(Pad.Up) || input.Pressed(Pad.Down)))
			{
				int rows = side.Count + (spread ? 1 : 0);
				int row = _targetAll ? side.Count : Math.Max(0, side.IndexOf(TargetUnderCursor()));
				row = (row + (input.Pressed(Pad.Down) ? 1 : rows - 1)) % rows;
				_targetAll = row == side.Count;
				if (!_targetAll) _cursor = foes ? _foes.IndexOf(side[row]) : _party.IndexOf(side[row]);
			}
			if (input.Pressed(Pad.Left) && !foes && (_targetBits & 0x106) != 0 && SideList(true).Count > 0) { _pick = Pick.Target; _targetAll = false; _cursor = FirstAliveFoe(); return; }
			if (input.Pressed(Pad.Right) && foes && (_targetBits & 0x170) != 0) { _pick = Pick.Ally; _targetAll = false; _cursor = Math.Max(0, _party.IndexOf(_acting)); return; }
			if (input.Pressed(Pad.Y) && spread)
			{
				_targetAll = !_targetAll;
				Game.Audio.PlaySe(0, _targetAll ? 1 : 2);   // spreading the cursor's sound, narrowing the cancel's
			}
			if (forced) _targetAll = true;
			if (input.Pressed(Pad.B))
			{
				_pick = _casting != null ? Pick.Spell : _usingItem > 0 && _targetAbility == _usingItem ? Pick.Item : Pick.Command;
				_cursor = 0;
				_casting = null;
				_castItem = 0;
				EndTargeting();
				return;
			}
			if (!input.Pressed(Pad.A)) return;
			Fighter who = _acting, one = TargetUnderCursor();
			bool all = _targetAll;
			bool onFoes = foes;
			// The targets as the action begins: the whole side, or the one (another of its side if it has fallen).
			List<Fighter> Targets()
			{
				List<Fighter> now = SideList(onFoes);
				if (all) return now.FindAll(f => f.Alive || !onFoes);
				if (one != null && (one.Alive || !onFoes)) return new List<Fighter> { one };
				Fighter other = now.Find(f => f.Alive);
				return other != null ? new List<Fighter> { other } : new List<Fighter>();
			}
			SpellDefinition spell = _casting;
			int item = _usingItem, castItem = _castItem;
			int ability = _targetAbility;
			who.LastTarget = one;
			who.LastTargetAll = all;
			who.LastTargetFoes = foes;
			EndTargeting();
			_castItem = 0;
			if (castItem > 0 && IsFang(castItem)) Decide(who, () => UseFang(who, castItem, Targets()), ItemWait(castItem), castItem);
			else if (castItem > 0 && CastOf(castItem) is SpellDefinition casts) Decide(who, () => UseCastItem(who, castItem, casts, Targets()), ItemWait(castItem), castItem);
			else if (spell != null) DecideSpell(who, spell, () => Cast(who, spell, Targets()));
			else if (ability == item && item > 0) { if (one != null) Decide(who, () => UseItem(who, item, one), ItemWait(item), item); }
			else Decide(who, () => { List<Fighter> struck = Targets(); if (struck.Count > 0) MemberAttacks(who, struck[0]); }, 0, 1);
		}
	}
}
