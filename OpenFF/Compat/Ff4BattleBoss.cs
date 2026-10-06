// FF4's scripted boss actions (BattleMonsterBehavior's handlers for the specials 3000 on, read from libff4.so): most are
// a line in the help window for 60 frames, a fade to black, the monster or the whole encounter swapped for another, and
// the fade back; a few move the monster, count down, or change what it is weak to.

using System;
using System.Collections.Generic;
using OpenFF.Data;

namespace OpenFF.Client
{
	internal sealed partial class Ff4Battle
	{
		/// <summary>A boss's own action (3000..3038 but the mist's, done elsewhere). True when the ability was one.</summary>
		private bool BossTurn(Fighter foe, int ability)
		{
			int id = foe.Monster?.Id ?? -1;
			switch (ability)
			{
				case 3003:   // BABGrowingGigantic: Mom Bomb grows - the encounter becomes 954, the new one keeping the HP
					Game.Audio.PlaySe(120, 3);
					Swap(5, () => ReplaceEncounter(954, keepHp: foe.Hp, emptyGauge: true));
					return true;
				case 3005:   // MABDarkElfTransform: "It seems this form will not suffice!", the Dark Elf to the Dark Dragon
					Line(70014, 60, () =>
					{
						Game.Audio.PlaySe(120, 10);
						StopMusic(0);
						Swap(10, () => { ReplaceMonster(foe, 171, full: true); PlayMusic(0xD); });
					});
					return true;
				case 3006:   // MABDressedWind: Barbariccia's whirlwind on
				case 3007:   // MABLostWind: and off
				{
					bool on = ability == 3006;
					Game.Audio.PlaySe(124, on ? 1 : 3);
					int to = on ? (id == 200 ? 0xE2 : 0xB0) : (id == 0xE2 ? 200 : 0xAF);
					Swap(10, () => ReplaceMonster(foe, to, full: false));
					return true;
				}
				case 3008:   // MABFusionDoll: "The dolls are joining together as one!" - the encounter becomes 929
					Line(70127, 60, () => { Game.Audio.PlaySe(120, 6); StopMusic(0); Swap(10, () => { ReplaceEncounter(929); PlayMusic(0xD); }); });
					return true;
				case 3009:   // MABSeparateDoll: "The dolls lost the strength to stay together!" - back to 928
					Line(70128, 60, () => { Game.Audio.PlaySe(120, 7); Swap(10, () => { ReplaceEncounter(928); PlayMusic(0x26); }); });
					return true;
				case 3012:   // MABDocking: Lugae and Barnabas join - the encounter becomes 932, Barnabas-Z's gauge emptied
					Game.Audio.PlaySe(120, 8);
					Swap(5, () => { ReplaceEncounter(932); foreach (Fighter f in _foes) if (f.Monster?.Id == 0xB8) f.Gauge = 0f; });
					return true;
				case 3014:   // MABGuardMantle: Rubicante's cloak closed (its variable 0 to {1,2,1}[old])
				case 3015:   // MABFluttering: and opened
				{
					bool close = ability == 3014;
					int to = close ? (id == 0xE0 ? 0xC6 : 0xBD) : (id == 0xC6 ? 0xE0 : 0xBC);
					if (close && foe.Free[0] < 3) foe.Free[0] = new[] { 1, 2, 1 }[Math.Max(0, foe.Free[0])];
					ReplaceMonster(foe, to, full: false);
					After(20, () => { });
					return true;
				}
				case 3018:   // MABReturnDarkness: Odin falls (the trial's end)
					foe.NotDeath = false;
					ConditionOn(foe, CKO);
					return true;
				case 3019:   // MABApproach: the Demon Wall moves in - 120/1, the effect at its root, 0.1 a frame for 26 frames
				{
					Game.Audio.PlaySe(123, 1);
					LoadEffect(725);
					PlayEffect(725, foe.Home + new Vector3(13f, 1f, 0f));
					for (int k = 1; k <= 26; k++) After(k, () => { foe.Home += new Vector3(0.1f, 0f, 0f); foe.Npc?.Teleport(foe.Home); });
					return true;
				}
				case 3022:   // BABDefencePosture: "Withdrew into his shell!" - Cagnazzo in his shell
					Line(70072, 60, () =>
					{
						Game.Audio.PlaySe(120, 5);
						Swap(5, () => { ReplaceMonster(foe, id == 0xA8 ? 0xDF : 0xE1, full: false); foe.Free[1] = 0; foe.Free[0] = 1; foe.Gauge = 0f; });
					});
					return true;
				case 3023:   // BABBarrierBreak: "Water barrier blasted apart by lightning!"
					Line(70117, 60, () => { Game.Audio.PlaySe(129, 3); foe.Free[1] = 0; });
					return true;
				case 3024:   // MABChangeBigFour: the next of the four in the first one's place
				{
					Game.Audio.PlaySe(114, 3);
					Fighter first = _foes.Find(f => f.Alive) ?? foe;
					int from = first.Monster?.Id ?? -1;
					(int To, Vector3 At) next = from == 197 ? (199, new Vector3(-12f, 0f, -25f)) : from == 199 || from == 0xE1 ? (200, new Vector3(-12f, 0f, 20f)) : (0xC6, new Vector3(-12f, 0f, 18f));
					Swap(10, () => { first.Home = next.At; ReplaceMonster(first, next.To, full: true); first.Gauge = 0f; });
					return true;
				}
				case 3025:   // BAChargeWater: "Water is surging to Cagnazzo's feet!"
					Line(70071, 60, () =>
					{
						void Water() { Game.Audio.PlaySe(129, 1); foe.Free[1] = 1; foe.Free[0] = 0; }
						if (foe.Free[0] != 0) Swap(5, () => { ReplaceMonster(foe, id == 0xDF ? 0xA8 : 199, full: false); Water(); });
						else Water();
					});
					return true;
				case 3026:   // MABSearchInvader: "SCANNING FOR INTRUDERS..."
					Line(70009, 60, null);
					return true;
				case 3027:   // MABChangeEyeColor: the Antlion's eyes
					Game.Audio.PlaySe(122, 6);
					After(30, () => { });
					return true;
				case 3028:   // MABBarrierChange: "Barrier Shift" - a new weakness of lightning, ice or fire, the rest resisted
				{
					ShowName(Ff4Party.Tables.AbilityName(3176) ?? "Barrier Shift", CastLead);
					LoadEffect(286);
					PlayEffect(286, Where(foe));
					Game.Audio.PlaySe(136, 4);
					int[] kinds = { 0x08, 0x10, 0x20 };
					List<int> pick = new List<int>(Array.FindAll(kinds, k => (AffinityOf(foe).Weak & k) == 0));
					int weak = pick.Count > 0 ? pick[_random.Next(pick.Count)] : kinds[_random.Next(3)];
					After(CastLead, () =>
					{
						foe.WeakOverride = weak;
						foe.ResistOverride = 0xFFF - weak;
						Game.Audio.PlaySe(136, 5);
						LoadEffect(10);
						PlayEffect(10, HitEffectSpot(foe));
						Note(foe.Name + " is weak to " + weak.ToString("x") + " now");
					});
					return true;
				}
				case 3030:   // MABWarpOfDimension: Zeromus flickers - 26 frames of jitter, then back
				{
					Game.Audio.PlaySe(115, 2);
					for (int k = 1; k <= 26; k++)
						After(k, () => foe.Npc?.Teleport(foe.Home + new Vector3((float)(_random.NextDouble() * 2 - 1), (float)(_random.NextDouble() * 2 - 1), (float)(_random.NextDouble() * 2 - 1))));
					After(27, () => foe.Npc?.Teleport(foe.Home));
					return true;
				}
				case 3031:   // MABRevealOneself: "Zeromus reveals his true nature!", "Zeromus: Grr...gh...agh..." - the encounter becomes 953
					Line(70174, 60, () =>
					{
						StopMusic(0);
						Line(70171, 60, () => { Game.Audio.PlaySe(115, 3); Swap(10, () => { ReplaceEncounter(953); PlayMusic(0x2D); }); });
					});
					return true;
				case 3032:   // MABIncubation: the Mystery Egg hatches into its monster, keeping its HP
				{
					int[] hatch = { 86, 100, 121, 124, 85, 118 };
					int to = id >= 228 && id <= 233 ? hatch[id - 228] : -1;
					if (to >= 0) Swap(10, () => ReplaceMonster(foe, to, full: false));
					return true;
				}
				case 3033:   // MABCountDawn: Bahamut counts down (5, 4, 3, 2, 1, Megaflare)
				{
					_help = BattleText(70121 + Math.Clamp(foe.Free[0], 0, 5), "");
					_helpUntil = _clock + CastLead;
					foe.Free[0] = foe.Free[0] >= 4 ? 0 : foe.Free[0] + 1;
					After(CastLead, () => { });
					return true;
				}
				case 3038:   // MABAppearMonsterFromDoor: "A monster emerged from the door!" - a Chimera Brain or a Yellow Dragon
					Line(70140, 60, () => Swap(10, () => ReplaceMonster(foe, _random.Next(2) == 0 ? 0x78 : 0x76, full: true)));
					return true;
				case 3000:   // MABOctManmosLegErase: legs come off
					LegErase(foe);
					return true;
				case 3017: case 3021:
					Log.First(LogChannel.File, "battle-boss-" + ability, 1, () => "battle: boss action " + ability + " (" + foe.Name + ") not in yet - a pause in its place");
					After(30, () => { });
					return true;
			}
			return false;
		}

		private const int Octomammoth = 0x9E;

		/// <summary>The Octomammoth's eight legs (BattleOctManmos::registerMonster): m&lt;family&gt;b each, in b_m&lt;family&gt;b's 101, placed by the table.</summary>
		private void SpawnLegs(Fighter foe)
		{
			foe.Legs = new Npc[8];
			string model = "m" + foe.Monster.Family.ToString("000") + "b", set = "b_m" + foe.Monster.Family.ToString("000") + "b";
			for (int i = 0; i < 8; i++)
			{
				Npc leg = Game.Npcs.SpawnModel(model, foe.Home, 0f);
				if (leg == null) continue;
				leg.Solid = false;
				try { leg.BindMotions(set); leg.PlayMotion(101, true); } catch (Exception) { }
				foe.Legs[i] = leg;
			}
			PlaceLegs(foe, 0f);
		}

		private static int LegsLeft(Fighter foe) => foe.Legs == null ? 0 : Array.FindAll(foe.Legs, l => l != null).Length;

		/// <summary>setLegPosture: each leg left at the body's place plus its offset for the count, turned as the table has it, sunk by the depth.</summary>
		private static void PlaceLegs(Fighter foe, float sunk)
		{
			if (foe.Legs == null) return;
			int count = LegsLeft(foe);
			for (int i = 0; i < 8; i++)
			{
				Npc leg = foe.Legs[i];
				if (leg == null || !Ff4Party.Tables.OctomammothLegs.TryGetValue((count, i), out float[] t)) continue;
				leg.Teleport(foe.Home + new Vector3(t[0], t[1] - sunk, t[2]));
				if (leg is LegacyNpc exact) exact.RotateExactly(t[3], t[4], t[5]);
			}
		}

		private static void RemoveLegs(Fighter foe)
		{
			if (foe.Legs == null) return;
			foreach (Npc leg in foe.Legs) { try { leg?.Remove(); } catch (Exception) { } }
			foe.Legs = null;
		}

		/// <summary>
		/// MABOctManmosLegErase: the legs from max(1, HP x 10 / max - 1) on fade out and go; the rest sink half a unit a
		/// frame for 30 frames with 120/2 and effect 710 on each, take their places for the new count, and rise over 30.
		/// </summary>
		private void LegErase(Fighter foe)
		{
			if (foe.Legs == null) return;
			int keep = Math.Max(1, foe.Hp * 10 / Math.Max(1, foe.MaxHp) - 1);
			List<int> going = new List<int>();
			for (int i = keep; i < 8; i++) if (foe.Legs[i] != null) going.Add(i);
			if (going.Count == 0) return;
			Note(foe.Name + " loses " + going.Count + " leg(s), " + keep + " left");
			for (int k = 1; k <= 5; k++)
			{
				int alpha = 100 - 20 * k;
				After(k, () => { foreach (int i in going) if (foe.Legs?[i] != null) foe.Legs[i].Alpha = alpha; });
			}
			After(6, () =>
			{
				foreach (int i in going) { try { foe.Legs?[i]?.Remove(); } catch (Exception) { } if (foe.Legs != null) foe.Legs[i] = null; }
				Game.Audio.PlaySe(120, 2);
				LoadEffect(710);
				for (int i = 0; i < 8; i++) if (foe.Legs?[i] != null) PlayEffect(710, foe.Legs[i].Position);
			});
			for (int k = 1; k <= 30; k++) { float down = 0.5f * k; After(6 + k, () => PlaceLegsAt(foe, down, keepOld: true)); }
			for (int k = 1; k <= 30; k++) { float down = 15f - 0.5f * k; After(36 + k, () => PlaceLegs(foe, down)); }
		}

		/// <summary>The legs as they stood, sunk (while they go down, before they take their new places).</summary>
		private void PlaceLegsAt(Fighter foe, float sunk, bool keepOld)
		{
			if (foe.Legs == null) return;
			int count = 8;   // where they stood: the places for all eight, the ones still there
			for (int i = 0; i < 8; i++)
			{
				Npc leg = foe.Legs[i];
				if (leg == null || !Ff4Party.Tables.OctomammothLegs.TryGetValue((count, i), out float[] t)) continue;
				leg.Teleport(foe.Home + new Vector3(t[0], t[1] - sunk, t[2]));
			}
		}

		/// <summary>A line of babil_battle in the help window for the frames, then what follows.</summary>
		private void Line(int message, int frames, Action then)
		{
			_help = BattleText(message, "(message " + message + ")");
			_helpUntil = _clock + frames;
			Note("line " + message + ": " + _help);
			After(frames, () => { if (then != null) then(); else { } });
		}

		/// <summary>A fade to black over the frames, the change at black, the fade back over as many; the turn waits it out.</summary>
		private void Swap(int frames, Action change)
		{
			GlobalScope.dgs.CFade.Main().fadeOut(frames, GlobalScope.dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
			GlobalScope.dgs.CFade.Sub().fadeOut(frames, GlobalScope.dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
			After(frames, () =>
			{
				try { change(); } catch (Exception ex) { Log.Write(LogChannel.General, "battle: swap: " + ex.Message); }
				After(1, () =>
				{
					GlobalScope.dgs.CFade.Main().fadeIn(frames);
					GlobalScope.dgs.CFade.Sub().fadeIn(frames);
				});
			});
			After(2 * frames + 2, () => { });
		}

		private static void StopMusic(int frames)
		{
			try { GlobalScope.MatrixSound.MtxSoundBGM.getSingleton().stop(frames, GlobalScope.MatrixSound.enMtxBGMSlot.enMTX_BGM_SLOT0); } catch (Exception) { }
		}

		private static void PlayMusic(int bgm)
		{
			try { GlobalScope.MatrixSound.MtxSoundBGM.getSingleton().play(bgm, 127, 0, GlobalScope.MatrixSound.enMtxBGMSlot.enMTX_BGM_SLOT0); } catch (Exception) { }
		}

		/// <summary>
		/// A monster becomes another in its place (setMonster): the new one's record, model and motions where it stood, its
		/// statuses cleared and its AI from the start; full HP from the new record, or the old HP kept (capped).
		/// </summary>
		private void ReplaceMonster(Fighter foe, int monsterId, bool full)
		{
			MonsterDefinition m = Ff4Party.Tables.Monster(monsterId);
			if (m == null) { Note("no monster " + monsterId + " to become"); return; }
			Fighter made = SpawnFoe(m, foe.Home, foe.Facing, Game.Hero.Position, join: false);
			if (made == null) return;
			try { foe.Npc?.Remove(); } catch (Exception) { }
			int hp = full ? made.MaxHp : Math.Min(foe.Hp, made.MaxHp);
			Note(foe.Name + " becomes " + made.Name);
			foe.Name = made.Name; foe.Monster = m; foe.Npc = made.Npc; foe.MaxHp = made.MaxHp; foe.Hp = Math.Max(1, hp);
			foe.Attack = made.Attack; foe.Defence = made.Defence; foe.Agility = made.Agility; foe.Level = made.Level;
			foe.Intellect = made.Intellect; foe.Spirit = made.Spirit; foe.Vitality = made.Vitality; foe.Strength = made.Strength;
			foe.MagicDefence = made.MagicDefence; foe.MagicEvasion = made.MagicEvasion; foe.HitChance = made.HitChance; foe.Evade = made.Evade;
			foe.AtbRate = made.AtbRate; foe.Conditions = 0; foe.AiCondition = -2; foe.AiIndex = 0; foe.WeakOverride = foe.ResistOverride = -1;
		}

		/// <summary>
		/// The whole encounter becomes another (registerParty): the monsters there now gone, the new group's stood at its
		/// places, its events in play; one given HP keeps it (capped), and its gauge emptied when asked.
		/// </summary>
		private void ReplaceEncounter(int partyId, int keepHp = -1, bool emptyGauge = false)
		{
			MonsterParty party = Ff4Party.Tables?.MonsterParty(partyId);
			if (party == null) { Note("no encounter group " + partyId); return; }
			foreach (Fighter f in _foes) { try { f.Npc?.Remove(); } catch (Exception) { } try { f.MistNpc?.Remove(); } catch (Exception) { } RemoveLegs(f); }
			_queue.RemoveAll(e => e.Actor.IsMonster);
			_foes.Clear();
			int n = 0, count = 0;
			foreach (MonsterPartySlot slot in party.Slots) count += Math.Max(1, slot.Count);
			foreach (MonsterPartySlot slot in party.Slots)
			{
				for (int k = 0; k < Math.Max(1, slot.Count); k++, n++)
				{
					MonsterDefinition m = Ff4Party.Tables.Monster(slot.MonsterId);
					if (m == null) continue;
					Vector3 at = Ff4BattleStage.Active ? Ff4BattleStage.MonsterSpot(new Vector3(slot.X, slot.Y, slot.Z), n, count) : Game.Hero.Position + new Vector3(-26f, 0f, (n - (count - 1) / 2f) * 12f);
					Fighter made = SpawnFoe(m, at, slot.W, Game.Hero.Position);
					if (made == null) continue;
					if (keepHp > 0) made.Hp = Math.Min(keepHp, made.MaxHp);
					if (emptyGauge) made.Gauge = 0f;
				}
			}
			_eventParty = party;
			Note("the encounter becomes group " + partyId + ": " + string.Join(", ", _foes.ConvertAll(f => f.Name)));
		}
	}
}
