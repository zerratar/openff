// A fighter struck, as FF4 shows it (BattleBehavior::startDamageAction and playFlash, read from libff4.so). Only a blow
// does it - a monster's plain attack, a member's Fight, Jump's landing, Throw - never a spell or an item.
// A member (and a monster fighting as one) takes the damage action: 1117, or 1118 for a blow of 51% of its HP and more,
// then setConditionMotion snaps it back to the loop it stood in. Steam's clip is gone in a frame - its trace has 1117 on
// one frame and the stance from its first frame on the next, its screenshots the stance throughout - so the loop it stood
// in starts over from its first frame, as Steam draws it. Defending or braced, it takes the guard (2002) instead; Stop,
// nothing. A monster plays no motion. The struck one flashes for three frames (setDamageFlash) on a member's Fight, Jump's
// landing and Throw, and the one the target cursor marks blinks (startTargetFlash: on three frames, off three, three
// times) - both BaseBattleCharacter::setFlash, which Steam draws as the model washed half toward a light grey (the model
// draw's flash case); a blow's is mostly under its hit effect.

using System;
using System.Collections.Generic;

namespace OpenFF.Client
{
	internal sealed partial class Ff4Battle
	{
		/// <summary>BattleBehavior::startDamageAction on one struck by a blow (damage done, already off its HP).</summary>
		private void StartDamageAction(Fighter t, int damage)
		{
			if (t == null || t.Npc == null || t == _executing || (t.IsMonster && t.PlayerType < 0)) return;   // monsters: BaseBattleCharacter's, nothing
			if (!t.Alive || Has(t, CStop) || t.Airborne || t.Hiding) return;
			if (t.Defending || t.Braced) { Play(t, 2002, true, 0); return; }   // BattleActionGuard
			// BattleActionDamage / BigDamage: the clip, then setConditionMotion(0) - the loop it stood in from its first frame.
			int stood = t.Acted ? t.IdleMotion : CurrentMotion(t);
			if (stood <= 0) stood = t.IdleMotion;
			Fighter hurt = t;
			After(1, () => { if (hurt.Alive && hurt.Npc != null && !hurt.Airborne) { Play(hurt, stood, true, 0); hurt.Acted = false; } });
			Note(t.Name + (damage * 100 >= t.MaxHp * 51 ? " reels" : " flinches") + " (" + (damage * 100 >= t.MaxHp * 51 ? 1118 : 1117) + ")");
		}

		private static int CurrentMotion(Fighter f)
		{
			try { return f.Npc is LegacyNpc held && held.CharacterId >= 0 ? GlobalScope.characterMng.getMotionIndex(held.CharacterId) : -1; }
			catch (Exception) { return -1; }
		}

		// setFlash's state per fighter: a blow's frames left, the cursor's blink frame, whether it is drawn washed now.
		private readonly Dictionary<Fighter, int> _hitFlash = new Dictionary<Fighter, int>();
		private readonly Dictionary<Fighter, int> _targetBlink = new Dictionary<Fighter, int>();
		private readonly Dictionary<Fighter, uint[]> _washed = new Dictionary<Fighter, uint[]>();
		private Fighter _turnBlinked;   // startTurnFlash: the member whose command window opened, blinked as it did
		private int _turnBlink = 15;

		/// <summary>setDamageFlash: three frames washed (a second blow starts them again).</summary>
		private void DamageFlash(Fighter t)
		{
			if (t?.Npc == null) return;
			_hitFlash[t] = 3;
			Wash(t, true);
		}

		/// <summary>
		/// Each frame: the target cursor's blink on what it marks (BattleTargetSelector: startTargetFlash once a fighter is
		/// marked - on three frames, off three, three times - stopTargetFlash when it no longer is) and a blow's flash counted down.
		/// </summary>
		private void StepFlashes()
		{
			HashSet<Fighter> marked = MarkedTargets();
			foreach (Fighter f in new List<Fighter>(_targetBlink.Keys)) if (!marked.Contains(f)) _targetBlink.Remove(f);
			foreach (Fighter f in marked) if (!_targetBlink.ContainsKey(f)) _targetBlink[f] = 0;
			foreach (Fighter f in new List<Fighter>(_hitFlash.Keys)) if (--_hitFlash[f] <= 0) _hitFlash.Remove(f);
			HashSet<Fighter> on = new HashSet<Fighter>(_hitFlash.Keys);
			// startTurnFlash (PlayerTurnFlash 3, 2, 2): the member whose turn has come blinks as a marked target does.
			if (_acting != _turnBlinked) { _turnBlinked = _acting; _turnBlink = _acting != null ? 0 : 15; }
			if (_turnBlinked != null && _turnBlink < 15) { if ((_turnBlink / 3) % 2 == 0) on.Add(_turnBlinked); _turnBlink++; }
			foreach (Fighter f in new List<Fighter>(_targetBlink.Keys))
			{
				int k = _targetBlink[f];
				if (k < 15 && (k / 3) % 2 == 0) on.Add(f);
				if (k < 15) _targetBlink[f] = k + 1;
			}
			foreach (Fighter f in new List<Fighter>(_washed.Keys)) if (!on.Contains(f)) Wash(f, false);
			foreach (Fighter f in on) Wash(f, true);
		}

		/// <summary>Who the target cursor marks now: the foe or member under it, or all of them for a pick of everyone.</summary>
		private HashSet<Fighter> MarkedTargets()
		{
			HashSet<Fighter> marked = new HashSet<Fighter>();
			if (_phase != Phase.Fight || _acting == null) return marked;
			bool all = _targetBits != 0 && _targetAll;
			if (_pick == Pick.Target && _cursor >= 0 && _cursor < _foes.Count)
			{
				if (all) { foreach (Fighter f in SideList(true)) marked.Add(f); }
				else marked.Add(_foes[_cursor]);
			}
			else if (_pick == Pick.Ally && _cursor >= 0 && _cursor < _party.Count)
			{
				if (all) { foreach (Fighter f in SideList(false)) marked.Add(f); }
				else marked.Add(_party[_cursor]);
			}
			return marked;
		}

		/// <summary>setFlash(on / off): the model's materials marked for the washed draw, or put back as they were.</summary>
		private void Wash(Fighter f, bool on)
		{
			if (f.Npc is not LegacyNpc held || held.CharacterId < 0) { _washed.Remove(f); return; }
			try
			{
				if (on && !_washed.ContainsKey(f))
				{
					_washed[f] = GlobalScope.characterMng.saveMaterialColours(held.CharacterId);
					GlobalScope.characterMng.flashMaterials(held.CharacterId);
				}
				else if (!on && _washed.TryGetValue(f, out uint[] saved))
				{
					GlobalScope.characterMng.restoreMaterialColours(held.CharacterId, saved);
					_washed.Remove(f);
				}
			}
			catch (Exception) { _washed.Remove(f); }
		}
	}
}
