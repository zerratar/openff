// FF4's escape as libff4.so runs it (BattleActiveTimeMain::checkEscape, EscapeFormula::calcEscapePlayer,
// BattlePlayerBehavior::initializeEscape / executeEscape, BattleActionEscape): while the run control is on (L and R
// held, or Steam's "Run away" toggled - M here) every member able takes the flee motion (1115) where it stands; a group that
// forbids it (its record's 0x88 bit 0 clear) says "Can't escape!" once; otherwise every 45 frames the gauges run, a roll:
// a party below the monsters' average level gets away 89 times in 100, else 49. Away: the line and its sound (0x65 5),
// the members turned about and running off 3 units a frame for 40 frames, half the group's gil dropped one time in two (the line for it
// at frame 23), the music stopping and the screens fading, and the field again.

using System;
using System.Collections.Generic;
using OpenFF.Data;

namespace OpenFF.Client
{
	internal sealed partial class Ff4Battle
	{
		private bool _runOn, _runReady, _cantEscapeShown, _escaping;
		private int _runFrames;

		/// <summary>The group's escape word (monster_party_table +0x88): bit 0 escape allowed, bit 2 the control does nothing.</summary>
		private int EscapeFlags => _eventParty?.EscapeFlags ?? 1;

		/// <summary>A frame of checkEscape: the control read, the members readied or let go, the roll every 45 frames the gauges run.</summary>
		private void CheckEscape(bool gaugesRun)
		{
			if (_escaping) return;
			if (Game.Input.KeyPressed("M")) { _runOn = !_runOn; Note("run away " + (_runOn ? "on" : "off")); }
			bool held = _runOn && (EscapeFlags & 4) == 0 && !BattleParameterFlag(0) && !BattleParameterFlag(3);
			if (!held)
			{
				if (_runReady) { foreach (Fighter f in _party) if (f.Alive && !f.Acted) { Play(f, f.IdleMotion, true, 4); f.Acted = false; } }
				_runReady = false;
				_runFrames = 0;
				_cantEscapeShown = false;
				return;
			}
			if (!_runReady)
			{
				// startEscape: each member that may, in the flee motion where it stands (setConditionMotion's checkMotionEscape:
				// 1115, no turn - the turn about is the escape's own, BattleActionEscape).
				foreach (Fighter f in _party)
				{
					if (!f.Alive || !CanAct(f) || f == _executing) continue;
					Play(f, 1115, true, 3);
					f.Acted = false;
				}
				_runReady = true;
			}
			if ((EscapeFlags & 1) == 0 || !_party.Exists(f => f.Alive && CanAct(f)))
			{
				if (!_cantEscapeShown)
				{
					// BABImpossiveEscape: "Can't escape!" for 20 frames.
					_cantEscapeShown = true;
					_help = BattleText(0x1124E, "Can't escape!");
					_helpUntil = _clock + 20;
				}
				return;
			}
			if (!gaugesRun) return;
			if (++_runFrames <= 44) return;
			_runFrames = 0;
			if (_random.Next(100) >= (PartyLevel() < FoesLevel() ? 11 : 51))
			{
				_escaping = true;
				_queue.Insert(0, (_party.Find(f => f.Alive) ?? _party[0], Escape));
			}
		}

		private int PartyLevel()
		{
			List<Fighter> members = _party.FindAll(f => f.Member != null);
			int sum = 0;
			foreach (Fighter f in members) sum += f.Level;
			return members.Count == 0 ? 0 : sum / members.Count;
		}

		private int FoesLevel()
		{
			int sum = 0;
			foreach (Fighter f in _foes) sum += f.Level;
			return _foes.Count == 0 ? 0 : sum / _foes.Count;
		}

		/// <summary>The party gets away (initializeEscape, executeEscape, BattleActionEscape).</summary>
		private void Escape()
		{
			_help = BattleText(0x11231, "The party escaped!");
			_helpUntil = -1;
			Note("the party escapes: " + _help);
			Game.Audio.PlaySe(0x65, 5);
			List<Fighter> running = _party.FindAll(f => f.Alive && CanAct(f));
			foreach (Fighter f in running) { Face(f, f.Facing + 180f); Play(f, 1115, true, 3); }
			for (int k = 1; k <= 40; k++)
			{
				After(k, () =>
				{
					foreach (Fighter f in running)
					{
						if (f.Npc == null) continue;
						f.Npc.Teleport(f.Npc.Position - Ff4BattleStage.Facing(f.Facing) * 3f);   // away from the foes
						Face(f, f.Facing + 180f);   // turned about again: a teleport lets the turn system swing the model back
					}
				});
			}
			After(23, () =>
			{
				int gil = 0;
				foreach (Fighter f in _foes) gil += f.Monster?.Gil ?? 0;   // MonsterParty::gold: the group's
				gil /= 2;
				gil = Math.Min(gil, Ff4Party.Party.Gil);
				if (gil > 0 && _random.Next(2) == 1)
				{
					Ff4Party.Party.Gil -= gil;
					_help = BattleText(0x92, "Dropped %d gil.").Replace("%d", gil.ToString()).Replace("%SCC00%", gil.ToString());
					Note("dropped " + gil + " gil");
				}
			});
			After(40, () =>
			{
				_help = null;
				// BattlePlayerEscape: the music stops over 15 frames; the part fades out over 15 and the field comes.
				try { GlobalScope.MatrixSound.MtxSoundBGM.getSingleton().stop(15, GlobalScope.MatrixSound.enMtxBGMSlot.enMTX_BGM_SLOT0); } catch (Exception) { }
				GlobalScope.dgs.CFade.Main().fadeOut(15, GlobalScope.dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
				GlobalScope.dgs.CFade.Sub().fadeOut(15, GlobalScope.dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
				_queue.Clear();
				_phase = Phase.Outro;
				_timer = 0;
				_fledFade = true;
			});
		}

		private bool _fledFade;
	}
}
