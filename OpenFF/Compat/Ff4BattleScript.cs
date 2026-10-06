// FF4's battle events: btl::BattleScriptEngine running battle_ai.bbd's scripts, as the monster party record names them
// (+0x7C the normal event, restarted every idle frame; +0x80 the one before an action; +0x84 the one after a normal
// action). Read from libff4.so's code (Tools/ff4_battle_ai.py disassembles the file with the command names).
//
// The engine (BattleScriptEngine::startEvent / execute): one event at a time, a single frame of state - where it is,
// the command's index and the script's count. A command's initialize runs as it is reached and says whether to go on;
// one that blocks (Wait, StartEventAction, DeathCharacter, EndBattle, ChangeBGM...) is then polled once a frame until
// it is done. Every command that does not block runs in the same frame; the event is over at an End. Jumps search
// forward for the first Label of the id. Ten registers (an operand -100000..-100009 reads var[k]), 32 flags, an action
// under construction and the event's actor persist for the whole battle.

using System;
using System.Collections.Generic;
using OpenFF.Data;

namespace OpenFF.Client
{
	internal sealed partial class Ff4Battle
	{
		private sealed class BattleScript
		{
			private readonly Ff4Battle _b;
			private readonly byte[] _data;
			private readonly int[] _lengths;
			private readonly Dictionary<int, (int Offset, int Count)> _scripts = new Dictionary<int, (int, int)>();

			private int _pc, _index, _count, _id = -1;
			private bool _finished = true;
			private Func<bool> _pending;

			// What persists through the battle (cleared at its start): the registers, the flags, the action being built.
			public readonly int[] Var = new int[10];
			public uint Flags;
			private int _ability, _team = -1, _eventActor = -1;
			private readonly List<int> _targets = new List<int>();

			private BattleScript(Ff4Battle b, byte[] data)
			{
				_b = b;
				_data = data;
				int commands = BitConverter.ToInt32(data, 0x14), scripts = BitConverter.ToInt32(data, 0x1C);
				int ctab = BitConverter.ToInt32(data, 0x18), stab = BitConverter.ToInt32(data, 0x20);
				_lengths = new int[commands];
				for (int i = 0; i < commands; i++) _lengths[i] = BitConverter.ToInt32(data, ctab + 16 * i + 4);
				for (int i = 0; i < scripts; i++)
				{
					int at = stab + 12 * i;
					_scripts[BitConverter.ToInt32(data, at)] = (BitConverter.ToInt32(data, at + 4), BitConverter.ToInt32(data, at + 8));
				}
			}

			/// <summary>battle_ai.bbd, or null (logged) when it is missing or not the file.</summary>
			public static BattleScript Load(Ff4Battle b)
			{
				byte[] data = Ff4Ui.ReadFile("battle_ai.bbd");
				if (data == null || data.Length < 0x30 || data[0] != 'H' || data[1] != 'E' || data[2] != 'S' || data[3] != 'D')
				{
					Log.Write(LogChannel.General, "battle: battle_ai.bbd missing - no battle events");
					return null;
				}
				return new BattleScript(b, data);
			}

			/// <summary>A new battle (BattleScriptEngine::initialize): the registers, flags and action cleared.</summary>
			public void Reset()
			{
				Array.Clear(Var, 0, Var.Length);
				Flags = 0;
				_ability = 0;
				_team = -1;
				_eventActor = -1;
				_targets.Clear();
				_finished = true;
				_pending = null;
			}

			public bool Running => !_finished;
			public int EventId => _id;

			/// <summary>startEvent: the script from its Start, as far as it goes this frame; true when it reached its End.</summary>
			public bool Start(int id)
			{
				if (!_scripts.TryGetValue(id, out (int Offset, int Count) s)) { Log.First(LogChannel.General, "battle-event-" + id, 1, () => "battle: no event script " + id); return true; }
				_id = id;
				_pc = s.Offset;
				_index = 0;
				_count = s.Count;
				_finished = false;
				_pending = null;
				if (Command != 0) { Log.Write(LogChannel.General, "battle: event " + id + " does not open with Start"); _finished = true; return true; }
				Run(true);
				return _finished;
			}

			/// <summary>execute: the blocked command polled; once it is done, on as far as the script goes; true when it is over.</summary>
			public bool Tick()
			{
				if (_finished) return true;
				if (_pending != null && !_pending()) return false;
				_pending = null;
				Run(false);
				return _finished;
			}

			private void Run(bool initializeFirst)
			{
				if (initializeFirst && !Initialize()) return;
				while (!_finished)
				{
					if (!Advance()) return;
					if (!Initialize()) return;
				}
			}

			private bool Advance()
			{
				_pc += Length;
				_index++;
				if (_index >= _count || Command == 1) { _finished = true; return false; }
				return true;
			}

			private int Command => BitConverter.ToInt32(_data, _pc);
			private int Length => Command >= 0 && Command < _lengths.Length ? _lengths[Command] : 4;
			private int Op(int k) => BitConverter.ToInt32(_data, _pc + 4 * k);

			/// <summary>convertCastVariable: -100000..-100009 read the registers, in a chain.</summary>
			private int Resolve(int v)
			{
				for (int k = 0; k < 10; k++) if (v == -100000 - k) v = Var[k];
				return v;
			}

			private int R(int k) => Resolve(Op(k));

			/// <summary>setCastVariable: the destination and the value both resolved.</summary>
			private void Set(int dst, int value)
			{
				int i = Resolve(dst);
				if (i < 0 || i > 9) { Log.Write(LogChannel.General, "battle: event " + _id + " stores to register " + i); return; }
				Var[i] = Resolve(value);
			}

			/// <summary>jumpLabel: forward to the first Label of the id; one not found ends the event.</summary>
			private void Jump(int label)
			{
				label = Resolve(label);
				while (true)
				{
					_index++;
					if (_index >= _count) { _finished = true; return; }
					_pc += Length;
					if (Command == 10 && Op(1) == label) return;
				}
			}

			private void Branch(bool taken, int whenTrue, int whenFalse)
			{
				int to = taken ? whenTrue : whenFalse;
				if (to >= 0) Jump(to);
			}

			private void Block(Func<bool> done) => _pending = done;

			/// <summary>A command reached: what it does, and whether the script goes on this frame (false: it blocks).</summary>
			private bool Initialize()
			{
				int c = Command;
				switch (c)
				{
					case 0: case 2: case 3: case 5: case 8: case 10: return true;   // Start (and its no-op twins), Label
					case 11: Jump(Op(1)); return true;                               // JumpLabel
					case 20:                                                        // Comparison: A op B → L_true, else L_false
					{
						int a = R(1), op = Op(2), bb = R(3);
						bool holds = op switch { 0 => a == bb, 1 => a != bb, 2 => a >= bb, 3 => a <= bb, 4 => a > bb, 5 => a < bb, _ => false };
						Branch(holds, R(4), R(5));
						return true;
					}
					case 72:                                                        // Calculation: var[dst] = A op B
					{
						int a = R(2), op = Op(3), bb = R(4);
						int value = op switch { 6 => a + bb, 7 => a - bb, 8 => a * bb, 9 => bb == 0 ? 0 : a / bb, 10 => bb == 0 ? a : a - (a / bb) * bb, _ => 0 };
						Set(R(1), value);
						return true;
					}
					case 85: Set(R(1), R(2)); return true;                          // SetVariable
					case 4: Set(Op(1), _b._random.Next(Math.Max(0, R(3)) + 1)); return true;   // GetRandomNumbers
					case 31: Flags |= 1u << (R(1) & 31); return true;                // FlagOn
					case 32: Flags &= ~(1u << (R(1) & 31)); return true;             // FlagOff
					case 33: Branch((Flags >> (R(1) & 31) & 1) != 0, Op(2), Op(3)); return true;   // FlagCheak
					case 22: _b.SetBattleParameterFlag(R(1), R(2) != 0); return true;   // SetBattleFlag
					case 14: _b.ShowEventMessage(Op(1)); return true;               // ShowMessage
					case 15: _b.HideEventMessage(); return true;                    // HideMessage
					case 16:                                                        // Wait: on the Nth frame after
					{
						int n = R(1);
						Block(() => n-- < 2);
						return false;
					}
					case 41: _b.StartEventMode(); return true;                      // StartEventMode
					case 42: _b._eventMode = false; return true;                    // EndEventMode
					case 6: _ability = Op(1); return true;                          // SetAction
					case 39:                                                        // SetTarget team, id
					{
						_targets.Clear();
						_team = -1;
						int team = R(1), id = R(2);
						int found = team == 1 ? _b.BattleIdOfMonster(id) : team == 0 ? _b.BattleIdOfPlayer(id) : -1;
						if (team == 0 || team == 1) _team = team;
						if (found >= 0) _targets.Add(found);
						return true;
					}
					case 7:                                                         // SetTargetRandomEnemy
					{
						_targets.Clear();
						Fighter actor = _b.ByBattleId(_eventActor);
						int team = actor != null && actor.IsMonster ? 0 : 1;
						List<int> pool = _b.BattleIdsOfTeam(team, aliveOnly: true);
						if (pool.Count > 0) { _targets.Add(pool[_b._random.Next(pool.Count)]); _team = team; }
						return true;
					}
					case 30: case 79: case 80:                                      // EnemyAllTarget, FriendAllTarget, AllTarget
					{
						Fighter actor = _b.ByBattleId(_eventActor);
						if (actor == null && c != 80) return true;
						_targets.Clear();
						int actorTeam = actor == null ? -1 : actor.IsMonster ? 1 : 0;
						for (int id = 0; id <= 10; id++)
						{
							Fighter f = _b.ByBattleId(id);
							if (f == null) continue;
							int team = f.IsMonster ? 1 : 0;
							if (c == 30 && team == actorTeam) continue;
							if (c == 79 && team != actorTeam) continue;
							_targets.Add(id);
							_team = team;
						}
						return true;
					}
					case 38:                                                        // SetEventActor team, id
					{
						int team = R(1), id = R(2);
						int found = team == 0 ? _b.BattleIdOfPlayer(id) : team == 1 ? _b.BattleIdOfMonster(id, aliveOnly: true) : -1;
						if (found >= 0) _eventActor = found;
						return true;
					}
					case 37:                                                        // StartEventAction: the actor acts, the event waits
					{
						Fighter actor = _b.ByBattleId(_eventActor);
						List<Fighter> targets = new List<Fighter>();
						foreach (int id in _targets) { Fighter t = _b.ByBattleId(id); if (t != null) targets.Add(t); }
						if (actor == null || !_b.EventAction(actor, _ability, targets)) return true;
						Block(() => _b._cues.Count == 0 && !_b.TurnEffectsPlaying());
						return false;
					}
					case 78:                                                        // EventActionTargetCheck dst
					{
						Fighter t = _targets.Count > 0 ? _b.ByBattleId(_targets[0]) : null;
						Set(R(1), _b.ByBattleId(_eventActor) != null && t != null && t.Alive ? 1 : 0);
						return true;
					}
					case 9: Set(R(1), _b.BattleIdOf(_b._attacker)); return true;     // GetAttacker
					case 64: Set(R(1), _b._attacker == null ? -1 : _b._attacker.IsMonster ? 1 : 0); return true;   // GetAttackerBreed
					case 65: Set(R(1), _b._attacker == null ? -1 : _b._attacker.IsMonster ? (_b._attacker.Monster?.Id ?? -1) : (_b._attacker.Member?.Id ?? -1)); return true;   // GetAttackerId
					case 23: case 63: Set(R(1), -1); return true;                   // GetActor, GetTiming: never set in FF4
					case 28: Set(R(1), -1); return true;                            // GetMyCharacterId: no owner in a party event
					case 17: { Fighter f = _b.ByBattleId(R(1)); Set(R(2), f == null ? -1 : f.IsMonster ? 1 : 0); return true; }   // GetBreed
					case 18: { Fighter f = _b.ByBattleId(R(1)); Set(R(2), f != null && !f.IsMonster ? f.Member?.Id ?? -1 : -1); return true; }   // GetPlayerId
					case 19: { Fighter f = _b.ByBattleId(R(1)); Set(R(2), f != null && f.IsMonster ? f.Monster?.Id ?? -1 : -1); return true; }   // GetMonsterId
					case 73: Set(R(2), _b.BattleIdOfMonster(R(1))); return true;      // GetBattleCharacterId monster id, dst
					case 45: { Fighter f = _b.ByBattleId(_b.BattleIdOfPlayer(R(1))); Set(R(2), f?.Hp ?? 0); return true; }   // GetPlayerHP
					case 46: { Fighter f = _b.ByBattleId(_b.BattleIdOfMonster(R(1))); Set(R(2), f?.Hp ?? 0); return true; }  // GetMonsterHP
					case 27: Set(R(1), _b._lastAbility); return true;                // GetAbility
					case 36: Set(R(1), _b._clock); return true;                      // GetBattleTime
					case 104: _b._frameCounterFrom = _b._clock; return true;          // ResetFrameCounter
					case 105: Set(R(1), _b._clock - _b._frameCounterFrom); return true;   // GetFrameCounter (at the middle speed)
					case 24: { Fighter f = _b.ByBattleId(R(1)); int k = R(2); Set(R(3), f != null && k >= 0 && k < 5 ? f.Free[k] : 0); return true; }   // GetCharacterVariable
					case 25: { Fighter f = _b.ByBattleId(R(1)); int k = R(2); if (f != null && k >= 0 && k < 5) f.Free[k] = R(3); return true; }        // SetCharacterVariable
					case 26: { Fighter f = _b.ByBattleId(R(1)); int k = R(2); if (f != null && k >= 0 && k < 5) f.Free[k] += R(3); return true; }       // AddCharacterVariable
					case 108: { int k = R(1), v = R(2); foreach (Fighter f in _b._foes) if (k >= 0 && k < 5) f.Free[k] = v; return true; }               // SetCharacterVariableForMonsterAll
					case 29: Branch(false, R(1), R(2)); return true;                 // IsTargeted: no owner in a party event
					case 91:                                                         // IsMonsterTargeted monster id, L_true, L_false
					{
						Fighter m = _b.ByBattleId(_b.BattleIdOfMonster(R(1)));
						Branch(_b._attacker != null && m != null && (m.NotDeath || _b._lastTargets.Contains(m)), R(2), R(3));
						return true;
					}
					case 90:                                                         // SufferPhysical battle id, L_yes, L_no
					{
						Fighter t = _b.ByBattleId(R(1));
						Branch(_b._attacker != null && t != null && _b._lastTargets.Contains(t) && _b._lastAbility == 1, R(2), R(3));
						return true;
					}
					case 76: Set(R(2), -1); return true;                             // SufferDamage: not kept yet
					case 98: { int l = R(1); return true; }                          // IsCounter: no counters yet - falls through
					case 99: Branch(false, R(2), R(3)); return true;                 // IsConfusionForMonster: no statuses yet
					case 107: Branch(false, Op(3), Op(4)); return true;              // CheckCondition: no statuses yet
					case 21: Branch(false, R(3), R(4)); return true;                 // GetPlayerFlag
					case 87: Branch(false, R(2), R(1)); return true;                 // AllPlayerCatch: none caught
					case 88:                                                         // CheckGameover L_over, L_alive
					{
						bool over = _b._party.TrueForAll(f => !f.Alive);
						Branch(over, R(1), R(2));
						return true;
					}
					case 43: { Fighter f = _b.ByBattleId(_b.BattleIdOfPlayer(R(1))); Branch(f != null && f.Gauge >= 1f, Op(2), Op(3)); return true; }    // CheakPlayerATP
					case 70: { Fighter f = _b.ByBattleId(_b.BattleIdOfMonster(R(1))); Branch(f != null && f.Gauge >= 1f, Op(2), Op(3)); return true; }   // CheakMonsterATP
					case 66: { Fighter f = _b.ByBattleId(_b.BattleIdOfPlayer(R(1))); Branch(f != null && f.Queued && f.Pending == null, Op(2), Op(3)); return true; }   // CheakPlayerATW
					case 48: _b.ResetGauge(_b.ByBattleId(_b.BattleIdOfPlayer(R(1)))); return true;    // ResetATG
					case 71: _b.ResetGauge(_b.ByBattleId(_b.BattleIdOfMonster(R(1)))); return true;   // ResetMonsterATG
					case 67: { Fighter f = _b.ByBattleId(_b.BattleIdOfPlayer(R(1))); if (f != null) f.Gauge = Math.Clamp(R(2) / 100f, 0f, 1f); return true; }   // ChargePlayerATP
					case 68: return true;                                            // ChargePlayerATW: nothing in FF4 either
					case 81: { Fighter f = _b.ByBattleId(R(1)); if (f != null) f.Gauge = Math.Clamp(R(2) / 100f, 0f, 1f); return true; }   // SetATP
					case 84:                                                         // PlayerHpMax
					{
						Fighter f = _b.ByBattleId(_b.BattleIdOfPlayer(R(1)));
						if (f != null) { f.Hp = Math.Max(0, f.MaxHp); if (f.Member != null) f.Member.Hp = f.Hp; }
						return true;
					}
					case 83:                                                         // RevivalParty: the fallen back with a tenth
						foreach (Fighter f in _b._party) if (!f.Alive) { f.Hp = Math.Max(1, f.MaxHp / 10); if (f.Member != null) f.Member.Hp = f.Hp; }
						return true;
					case 74: { Fighter f = _b.ByBattleId(R(1)); if (f != null) f.NotDeath = true; return true; }    // NotDeathFlagOn
					case 75: { Fighter f = _b.ByBattleId(R(1)); if (f != null) f.NotDeath = false; return true; }   // NotDeathFlagOff
					case 69:                                                         // DeathMonster monster id
					{
						Fighter m = _b.ByBattleId(_b.BattleIdOfMonster(R(1)));
						if (m != null && m.Alive) { m.NotDeath = false; m.Hp = 0; _b.Fell(m); }
						return true;
					}
					case 40:                                                         // DeathCharacter: the fallen go, the event waits
						if (!_b.RunDeadProcess()) return true;
						Block(() => _b._cues.Count == 0);
						return false;
					case 44:                                                         // EndBattle: the battle closes, the event waits
						_b.EndByEvent();
						Block(() => _b._phase == Phase.Outro || _b._phase == Phase.Idle);
						return false;
					case 96: Game.Audio.PlaySe(R(1), R(2)); return true;          // PlaySE bank, number
					case 94: case 95: case 97: return true;                          // LoadAsyncSE, LoadingWaitSE, AllReleaseSE: the client loads on play
					case 47:                                                         // ChangeBGM
						Log.First(LogChannel.File, "battle-event-bgm", 3, () => "battle: event " + _id + " changes the music to " + R(1) + " - not yet");
						return true;
					case 92: _b.SetBattleParameterFlag(2, true); return true;        // OnForceMaxDamage
					case 93: _b.SetBattleParameterFlag(2, false); return true;       // OffForceMaxDamage
					case 100: _b.SetBattleParameterFlag(4, true); return true;       // OffInvokeProdaction (sets flag 4)
					case 101: _b.SetBattleParameterFlag(4, false); return true;      // OnInvokeProdaction
					case 102: _b.SetBattleParameterFlag(5, true); return true;       // OnSurelyCondition
					case 103: _b.SetBattleParameterFlag(5, false); return true;      // OffSurelyCondition
					case 106: _b.SetBattleParameterFlag(0xB, true); return true;     // HideGameoverMessage
					case 109: _b.SetBattleParameterFlag(0xE, true); return true;     // OnReflecThrough
					case 110: _b.SetBattleParameterFlag(0xE, false); return true;    // OffReflecThrough
					case 12: case 35: return true;                                   // DecideTurnAction, DecideCounterAction: no owner in a party event
					default:
						Log.First(LogChannel.File, "battle-event-command-" + c, 1, () => "battle: event " + _id + " command " + c + " (" + (c < CommandNames.Length ? CommandNames[c] : "?") + ") not in yet - passed over");
						return true;
				}
			}

			private static readonly string[] CommandNames =
			{
				"Start", "End", "Start", "Start", "GetRandomNumbers", "Start", "SetAction", "SetTargetRandomEnemy", "Start", "GetAttacker", "Label", "JumpLabel",
				"DecideTurnAction", "CheakOctManmosEraseLeg", "ShowMessage", "HideMessage", "Wait", "GetBreed", "GetPlayerId", "GetMonsterId", "Comparison",
				"GetPlayerFlag", "SetBattleFlag", "GetActor", "GetCharacterVariable", "SetCharacterVariable", "AddCharacterVariable", "GetAbility",
				"GetMyCharacterId", "IsTargeted", "EnemyAllTarget", "FlagOn", "FlagOff", "FlagCheak", "GetLegNumber", "DecideCounterAction", "GetBattleTime",
				"StartEventAction", "SetEventActor", "SetTarget", "DeathCharacter", "StartEventMode", "EndEventMode", "CheakPlayerATP", "EndBattle",
				"GetPlayerHP", "GetMonsterHP", "ChangeBGM", "ResetATG", "CreateModel", "WaitLoadingModel", "WaitLoadingTexture", "WaitLoadingMotion",
				"AddMotion", "StartMotion", "SetPosition", "SetRotation", "SetAlpha", "SetScale", "SetShow", "DeleteModel", "SetShadowAlpha",
				"SetShadowShow", "GetTiming", "GetAttackerBreed", "GetAttackerId", "CheakPlayerATW", "ChargePlayerATP", "ChargePlayerATW", "DeathMonster",
				"CheakMonsterATP", "ResetMonsterATG", "Calculation", "GetBattleCharacterId", "NotDeathFlagOn", "NotDeathFlagOff", "SufferDamage",
				"AddPlayer", "EventActionTargetCheck", "FriendAllTarget", "AllTarget", "SetATP", "PairMagic", "RevivalParty", "PlayerHpMax", "SetVariable",
				"AllPlayerReverse", "AllPlayerCatch", "CheckGameover", "SetConditionAllMonster", "SufferPhysical", "IsMonsterTargeted", "OnForceMaxDamage",
				"OffForceMaxDamage", "LoadAsyncSE", "LoadingWaitSE", "PlaySE", "AllReleaseSE", "IsCounter", "IsConfusionForMonster", "OffInvokeProdaction",
				"OnInvokeProdaction", "OnSurelyCondition", "OffSurelyCondition", "ResetFrameCounter", "GetFrameCounter", "HideGameoverMessage",
				"CheckCondition", "SetCharacterVariableForMonsterAll", "OnReflecThrough", "OffReflecThrough",
			};
		}

		// ---- what the events reach in the battle ----

		private BattleScript _script;
		private bool _scriptLoaded;
		private MonsterParty _eventParty;
		private enum EventKind { None, Normal, Before, After }
		private EventKind _eventKind = EventKind.None;
		private bool _eventMode;                      // StartEventMode: the gauges stand, nothing new is decided
		private uint _battleParameterFlags;           // BattleParameter's flags the events set (2 the force-max damage...)
		private Fighter _attacker;                    // who acted last (the behaviour manager's attacker)
		private readonly List<Fighter> _lastTargets = new List<Fighter>();
		private int _lastAbility;                     // GetAbility's: a spell's id, an item's id, else the command
		private int _frameCounterFrom;
		private string _help;                         // FF4's help line: an event's message, an ability's name
		private int _helpUntil = -1;                  // the frame a passing line goes (-1: until hidden)
		private Dictionary<uint, string> _battleTexts;

		private void BeginBattleEvents(MonsterParty party)
		{
			if (!_scriptLoaded) { _scriptLoaded = true; _script = BattleScript.Load(this); }
			_script?.Reset();
			_eventParty = party;
			_eventKind = EventKind.None;
			_eventMode = false;
			_battleParameterFlags = 0;
			_attacker = null;
			_lastTargets.Clear();
			_lastAbility = 0;
			_help = null;
			if (party != null && (party.NormalEvent >= 0 || party.BeforeEvent >= 0 || party.AfterEvent >= 0))
				Log.Write(LogChannel.File, "battle: events - normal " + party.NormalEvent + ", before " + party.BeforeEvent + ", after " + party.AfterEvent);
		}

		/// <summary>An event begins; true when it ran to its End in this frame (nothing left to wait for).</summary>
		private bool StartEvent(EventKind kind, int id)
		{
			if (_script == null || id < 0) return true;
			if (_script.Start(id)) return true;
			_eventKind = kind;
			Log.Write(LogChannel.File, "battle: " + kind.ToString().ToLowerInvariant() + " event " + id + " runs (step " + LegacyStep.Count + ")");
			return false;
		}

		/// <summary>The running event a frame on; true once it is over (or none runs).</summary>
		private bool TickEvent()
		{
			if (_eventKind == EventKind.None) return true;
			if (!_script.Tick()) return false;
			Log.Write(LogChannel.File, "battle: " + _eventKind.ToString().ToLowerInvariant() + " event over (step " + LegacyStep.Count + ")");
			_eventKind = EventKind.None;
			return true;
		}

		private Fighter ByBattleId(int id)
		{
			if (id >= 0 && id < 5) return id < _party.Count ? _party[id] : null;
			if (id >= 5 && id <= 10) return id - 5 < _foes.Count ? _foes[id - 5] : null;
			return null;
		}

		private int BattleIdOf(Fighter f)
		{
			if (f == null) return -1;
			int i = _party.IndexOf(f);
			if (i >= 0) return i;
			i = _foes.IndexOf(f);
			return i >= 0 ? 5 + i : -1;
		}

		private int BattleIdOfPlayer(int playerId)
		{
			for (int i = 0; i < _party.Count; i++) if (_party[i].Member?.Id == playerId) return i;
			return -1;
		}

		private int BattleIdOfMonster(int monsterId, bool aliveOnly = false)
		{
			for (int i = 0; i < _foes.Count; i++) if (_foes[i].Monster?.Id == monsterId && (!aliveOnly || _foes[i].Alive)) return 5 + i;
			return -1;
		}

		private List<int> BattleIdsOfTeam(int team, bool aliveOnly)
		{
			List<int> ids = new List<int>();
			List<Fighter> side = team == 0 ? _party : _foes;
			for (int i = 0; i < side.Count; i++) if (!aliveOnly || side[i].Alive) ids.Add(team == 0 ? i : 5 + i);
			return ids;
		}

		private void SetBattleParameterFlag(int flag, bool on)
		{
			if (flag < 0 || flag > 31) return;
			if (on) _battleParameterFlags |= 1u << flag;
			else _battleParameterFlags &= ~(1u << flag);
		}

		private bool BattleParameterFlag(int flag) => (_battleParameterFlags >> flag & 1) != 0;

		/// <summary>ShowMessage: FF4's help window at the top, with a message of babil_battle.msd, until HideMessage.</summary>
		private void ShowEventMessage(int id)
		{
			_help = BattleText(id, "(message " + id + ")");
			_helpUntil = -1;
			Note("event message " + id + ": " + _help);
		}

		private void HideEventMessage() => _help = null;

		private void StartEventMode()
		{
			_eventMode = true;
			_queue.Clear();
			foreach (Fighter f in _party) if (f.Queued) { f.Queued = false; f.Pending = null; }
			foreach (Fighter f in _foes) f.Queued = false;
			_acting = null;
			_pick = Pick.None;
		}

		private void ResetGauge(Fighter f)
		{
			if (f == null) return;
			f.Gauge = 0f;
			f.Queued = false;
			f.Pending = null;
			_queue.RemoveAll(q => q.Actor == f);
		}

		/// <summary>StartEventAction: the event's actor does the action the event built, at once; false when there is nothing to do.</summary>
		private bool EventAction(Fighter actor, int ability, List<Fighter> targets)
		{
			Fighter target = targets.Find(t => t.Alive);
			actor.DecidedAbility = ability;
			Note("event action: " + actor.Name + " - ability " + ability + (target != null ? " on " + target.Name : ""));
			if (ability == 1)
			{
				if (target == null) return false;
				if (actor.IsMonster) { MonsterAttack(actor, target); return true; }
				MemberAttacks(actor, target);
				return true;
			}
			SpellDefinition spell = Ff4Party.Tables.Spell(ability);
			if (spell != null)
			{
				if (actor.IsMonster) { MonsterCasts(actor, spell, targets.Count > 0 ? targets.FindAll(t => t.Alive) : MonsterTargets(actor, spell.HitsAll ? 4 : 1)); return true; }
				Cast(actor, spell, targets.Count > 0 ? targets : _foes.FindAll(f => f.Alive));
				return true;
			}
			Log.First(LogChannel.File, "battle-event-ability-" + ability, 1, () => "battle: event action ability " + ability + " not in yet");
			return false;
		}

		/// <summary>DeathCharacter: the fallen fade now; false when no one has fallen.</summary>
		private bool RunDeadProcess()
		{
			if (_dying.Count == 0) return false;
			Game.Audio.PlaySe(0x65, 6);
			foreach (Fighter f in _dying) StartDeathFade(f);
			_dying.Clear();
			return true;
		}

		/// <summary>EndBattle: the battle closes as a story battle does - the fade out and back to the scene, no result.</summary>
		private void EndByEvent()
		{
			_closing = true;
			_queue.Clear();
			_phase = Phase.Outro;
			_timer = 0;
			GlobalScope.dgs.CFade.Main().fadeOut(WinEndFade, GlobalScope.dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
			GlobalScope.dgs.CFade.Sub().fadeOut(WinEndFade, GlobalScope.dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
			Note("an event ends the battle (step " + LegacyStep.Count + ")");
		}
	}
}
