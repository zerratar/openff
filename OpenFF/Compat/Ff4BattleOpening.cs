// FF4's battle openings (btl::BattleOpening and its six sub-openings, read from libff4.so): the roll a random encounter
// makes for its type (world::attackType), the gauges each type starts with (BattleCharacterManager::initializeATG), and
// the opening itself - a preemptive strike's "!" (effect 600) over every monster once the party has run in, a back
// attack's rows swapped (changeFormation, put back when the battle ends) with the party turned away, a "!" over each
// member, then the turn (motion 1132) to face the foes, a surprise's party stood in place under the "!" for at least
// 46 frames, and a boss's back attack the same after its entrance. Each type's help line stays up for the opening.

using System;
using System.Collections.Generic;
using OpenFF.Data;

namespace OpenFF.Client
{
	internal sealed partial class Ff4Battle
	{
		// btl::BATTLE_ENCOUNT_TYPES (0..3) and BattleOpening's sub-opening indices (4, 5: a boss group's own).
		public const int OpenNormal = 0, OpenPreemptive = 1, OpenBack = 2, OpenSurprise = 3, OpenBossNormal = 4, OpenBossBack = 5;

		/// <summary>The mark over a fighter as an opening shows it (BattleEffect 600).</summary>
		private const int OpeningMarkEffect = 600;

		/// <summary>BattleSurpriseAttack's least length: past frame 0x2C of its count.</summary>
		private const int SurpriseFrames = 46;

		/// <summary>The member's turn from facing away to facing the foes (0x46C).</summary>
		private const int TurnMotion = 1132;

		private int _nextOpening;              // the type the next battle opens with (0..3), taken as it starts
		private int _opening;                  // this battle's sub-opening (OpenNormal..OpenBossBack)
		private int _openingStage;             // 0 waiting for the marks, 1 the marks playing, 2 the turn, 3 done
		private int _formationBefore = -1;     // GameParameter::formation() as the battle began (BattleSystem+0x33e4)
		private string _openingHelp;
		private readonly List<int> _openingMarks = new List<int>();
		private readonly List<Fighter> _turning = new List<Fighter>();
		private readonly HashSet<Fighter> _turnEnded = new HashSet<Fighter>();

		/// <summary>
		/// world::attackType: three draws of 100 - Surprise, then Preemptive, then Back attack, else Normal - with odds by
		/// whether the party dashed into the fight and whether its average level is under the area's.
		/// </summary>
		public static int RollOpening(bool dashing, int averageLevel, int areaLevel, Random r)
		{
			int first = r.Next(100);
			bool under = averageLevel < areaLevel;
			(int surprise, int preemptive, int back) = dashing ? (under ? (3, 5, 3) : (7, 15, 7)) : (under ? (7, 5, 7) : (3, 7, 3));
			if (first < surprise) return OpenSurprise;
			if (r.Next(100) < preemptive) return OpenPreemptive;
			return r.Next(100) < back ? OpenBack : OpenNormal;
		}

		/// <summary>pl::PlayerParty::averageLevel(-1): the floor of the members' mean level, the fallen counted.</summary>
		private static int AverageLevel()
		{
			int sum = 0, n = 0;
			foreach (Character c in Ff4Party.Party.Members) { sum += c.Level; n++; }
			return n > 0 ? sum / n : 1;
		}

		/// <summary>The type the next battle opens with: a random encounter's roll, an event's (bootEventBattle's byte 0, 1, 2 = Normal, Back attack, Surprise).</summary>
		public void SetNextOpening(int type) => _nextOpening = Math.Clamp(type, 0, 3);

		/// <summary>BattleOpening::initialize: the type (or --ff4-opening's, WSCDebug+0x54's) to its sub-opening, a boss group's for 0 and 2.</summary>
		private int TakeOpening(bool boss)
		{
			int type = int.TryParse(Options.Get("ff4-opening"), out int forced) && forced >= 0 && forced <= 3 ? forced : _nextOpening;
			_nextOpening = OpenNormal;
			return type switch
			{
				OpenNormal => boss ? OpenBossNormal : OpenNormal,
				OpenBack => boss ? OpenBossBack : OpenBack,
				_ => type,
			};
		}

		private bool BackOpening => _opening == OpenBack || _opening == OpenBossBack;

		/// <summary>Whether the members stand at their spots from the start (no run-in): the back attacks and the surprise.</summary>
		private bool StandInPlace => BackOpening || _opening == OpenSurprise;

		/// <summary>The facing a member stands with as the opening begins: away from the foes in a back attack (the root's negated, a boss's turned about).</summary>
		private float OpeningFacing(float root) => _opening == OpenBack ? -root : _opening == OpenBossBack ? root + 180f : root;

		/// <summary>initializeATG: 45..65 in a normal opening; the side that has the first move full, the other empty.</summary>
		private float StartGauge(bool monster)
		{
			if (_phase == Phase.Idle)
			{
				if (_opening == OpenPreemptive) return monster ? 0f : 1f;
				if (_opening == OpenBack || _opening == OpenSurprise || _opening == OpenBossBack) return monster ? 1f : 0f;
			}
			return (45 + _random.Next(21)) / 100f;
		}

		/// <summary>The opening's type taken as the battle begins: the rows swapped for a back attack, the help line up.</summary>
		private void BeginOpening(IEnumerable<int> monsterIds)
		{
			_opening = TakeOpening(BossOf(monsterIds).Camera != null);
			_openingStage = 0;
			_openingMarks.Clear();
			_turning.Clear();
			_turnEnded.Clear();
			_formationBefore = Ff4Party.Formation;
			if (BackOpening) Ff4Party.Formation = 1 - Ff4Party.Formation;   // changeFormation, before the members are placed
			_openingHelp = _opening switch
			{
				OpenPreemptive => BattleText(0x1122E, "Preemptive strike!"),
				OpenBack => BattleText(0x11230, "Back attack!"),
				OpenSurprise => BattleText(0x1122F, "Surprised!"),
				_ => null,
			};
			if (_openingHelp != null) { _help = _openingHelp; _helpUntil = -1; }
			if (_opening != OpenNormal) Note("opening: " + _opening switch { OpenPreemptive => "preemptive", OpenBack => "back attack", OpenSurprise => "surprised", OpenBossNormal => "boss", _ => "boss back attack" });
		}

		/// <summary>BattleSystem::terminate: the formation the battle began with comes back (a back attack's swap, a Change's).</summary>
		private void EndOpening()
		{
			if (_formationBefore >= 0) Ff4Party.Formation = _formationBefore;
			_formationBefore = -1;
			ReleaseOpeningMarks();
		}

		/// <summary>A frame of the opening: the marks when their frame comes, the turn once they have played out.</summary>
		private void StepOpening()
		{
			switch (_openingStage)
			{
				case 0:
				{
					// The frame the marks come (the opening's own frame k; Steam's back attack: turned away on the frame after
					// the rows swap, the turn 18 frames on): a preemptive strike's on k = 6, as the camera settles; a back
					// attack's and a surprise's on k = 0; a boss's back attack's once its entrance camera is done.
					int at = _opening switch
					{
						OpenPreemptive => 7,
						OpenBack or OpenSurprise => 1,
						OpenBossBack => _bossCamera != null ? BossMoveFrom + _bossCamera.Frames + 1 : 2,
						_ => -1,
					};
					if (at < 0) { _openingStage = 3; return; }
					if (_timer < at) return;
					LoadEffect(OpeningMarkEffect);
					if (_opening == OpenPreemptive)
					{
						// drawExclamationEffect: over each monster, its height (by its battle scale) up and 3 more.
						foreach (Fighter f in _foes) if (f.Alive) OpeningMark(Where(f) + new Vector3(0f, (f.Monster != null ? f.Monster.Height * (f.Monster.Scale > 0.01f ? f.Monster.Scale : 1f) : 10f) + 3f, 0f));
					}
					else
					{
						// drawBackAttackEffect: 18 over each member standing.
						foreach (Fighter f in _party) if (f.Alive && f.Npc != null) OpeningMark(f.Home + new Vector3(0f, 18f, 0f));
					}
					if (_opening == OpenBossBack) { _openingHelp = _help = BattleText(0x11230, "Back attack!"); _helpUntil = -1; }
					_openingStage = 1;
					return;
				}
				case 1:
					if (OpeningMarksPlaying()) return;
					ReleaseOpeningMarks();
					if (!BackOpening) { _openingStage = 3; return; }
					// playTurnMotion: everyone to the root's facing, the turn played once.
					foreach (Fighter f in _party)
					{
						if (!f.Alive || f.Npc == null) continue;
						Face(f, f.Facing);
						Play(f, TurnMotion, false, 0);
						_turning.Add(f);
					}
					_openingStage = 2;
					return;
				case 2:
					// isEndTurnMotion: each member into its stance the frame after its turn ends (action 0x26; Steam's 16 frames).
					for (int i = _turning.Count - 1; i >= 0; i--)
					{
						Fighter f = _turning[i];
						if (f.Npc != null && _timer < 600 && (!f.Npc.MotionDone || _turnEnded.Add(f))) continue;
						f.IdleMotion = AnyFlag(f, 5) || f.Hp <= f.MaxHp / 4 ? 2001 : _heroMotionIdle;
						Play(f, f.IdleMotion, true, 4);
						_turning.RemoveAt(i);
					}
					if (_turning.Count == 0) _openingStage = 3;
					return;
			}
		}

		/// <summary>Whether the opening has played out: its marks and turn, the run-in or the camera's slide, a surprise's 46 frames.</summary>
		private bool OpeningDone => _openingStage == 3 && _timer > IntroFrames && (_opening != OpenSurprise || _timer > SurpriseFrames);

		/// <summary>The opening's help line taken down as the fight begins (terminate's releaseHelpWindow).</summary>
		private void FinishOpening()
		{
			if (_openingHelp != null && _help == _openingHelp) _help = null;
			_openingHelp = null;
			ReleaseOpeningMarks();
		}

		private void OpeningMark(Vector3 at)
		{
			try
			{
				GlobalScope.eff.CEffectMng effects = GlobalScope.eff.CEffectMng.instance();
				int made = effects.create(OpeningMarkEffect, 1);
				if (made < 0) { Log.First(LogChannel.File, "battle-effect-600", 2, () => "battle: the opening's mark could not be made"); return; }
				effects.enableBoxCulling(made, false);
				effects.setPosition(made, new GlobalScope.VecFx32((int)Math.Round(at.X * 4096), (int)Math.Round(at.Y * 4096), (int)Math.Round(at.Z * 4096)));
				_openingMarks.Add(made);
				Log.Write(LogChannel.File, string.Format(System.Globalization.CultureInfo.InvariantCulture, "battle: opening mark at {0:0.0}, {1:0.0}, {2:0.0} (step {3})", at.X, at.Y, at.Z, LegacyStep.Count));
			}
			catch (Exception ex) { Log.Write(LogChannel.General, "battle: the opening's mark: " + ex.Message); }
		}

		/// <summary>isClearAllEffect over the marks (300 frames at most, should one never end).</summary>
		private bool OpeningMarksPlaying()
		{
			if (_timer > 300) return false;
			GlobalScope.eff.CEffectMng effects = GlobalScope.eff.CEffectMng.instance();
			foreach (int made in _openingMarks) { try { if (effects.isPlay(made)) return true; } catch (Exception) { } }
			return false;
		}

		private void ReleaseOpeningMarks()
		{
			if (_openingMarks.Count == 0) return;
			GlobalScope.eff.CEffectMng effects = GlobalScope.eff.CEffectMng.instance();
			foreach (int made in _openingMarks) { try { effects.release(made); } catch (Exception) { } }
			_openingMarks.Clear();
		}
	}
}
