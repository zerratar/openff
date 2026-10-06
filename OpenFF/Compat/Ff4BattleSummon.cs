// A summon as FF4 casts it (BattlePlayerBehavior::stateSummonMagic and btl::CastEvent, read from libff4.so): the MP paid
// and the results worked out as the turn starts; no chant, no name. Once no number is on the screen it goes black at
// once, the party's and the monsters' models leave, the battle's 2D goes, and the summon's cast scene runs -
// CAST_SCRIPT.dat's s<NN>_00 (battle_parameter chain 6), on the field's own script engine with the BTL_* commands:
// Rydia's prelude (global.script) on the battle stage, then the summon's stage and its show. When the scene has
// ended: the battle stage, the models, the camera as it was, the 2D, a 15-frame fade in - and then, all together, the
// HP changes and every target's number.

using System;
using System.Collections.Generic;
using OpenFF.Data;

namespace OpenFF.Client
{
	internal sealed partial class Ff4Battle
	{
		private bool _summonScene;
		private int _summonMap = -1, _summonFrames;
		private Npc _castStage;
		private readonly List<Npc> _summonHidden = new List<Npc>();

		/// <summary>A member's summon, when it has a cast scene and the battle stands on its stage: true when it is taken over here.</summary>
		private bool TrySummon(Fighter caster, SpellDefinition spell, List<Fighter> targets)
		{
			if (caster.Member == null || spell.School != OpenFF.Data.MagicSchool.Summon || !Ff4BattleStage.Active) return false;
			if (Ff4Party.Tables == null || !Ff4Party.Tables.SummonCasts.TryGetValue(spell.Id, out int scene)) return false;
			if (!UsableUnder(caster, spell.Id))
			{
				// stateSummonMagic's failure: the line for 19 frames, nothing else.
				_help = BattleText(70198, "Cannot use voice!");
				_helpUntil = _clock + 19;
				After(19, () => { });
				return true;
			}
			List<Action> results = new List<Action>();
			foreach (Fighter t in targets)
			{
				Action result = SpellResult(caster, t, spell, targets.Count);
				if (result != null) results.Add(result);
			}
			Note(caster.Name + " summons " + (spell.Name ?? spell.Id.ToString()) + " (scene s" + scene.ToString("00") + "_00)");
			// State 1: once no number is on the screen, black at once.
			void WaitForNumbers()
			{
				if (_pops.Count > 0) { After(1, WaitForNumbers); return; }
				StartSummonScene(scene, results);
			}
			After(1, WaitForNumbers);
			return true;
		}

		private void StartSummonScene(int scene, List<Action> results)
		{
			GlobalScope.dgs.CFade.Main().fadeOut(0, GlobalScope.dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
			GlobalScope.dgs.CFade.Sub().fadeOut(0, GlobalScope.dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
			// unregisterCharacterMng / hideMonster: every fighter gone from the screen; the battle's 2D off.
			_summonHidden.Clear();
			foreach (Fighter f in AllFighters())
				foreach (Npc model in ModelsOf(f))
					if (model != null && !model.Hidden) { model.Hidden = true; _summonHidden.Add(model); }
			_summonScene = true;
			Ff4Cutscene.CastEnded = false;
			// The cast camera's clip, 2 to 8192 (its field of view the scene's own prelude sets).
			try { GlobalScope.CCastCommandTransit.getInstance().cast_FieldCamera()?.setClip(2 * 4096, 8192 * 4096); } catch (Exception) { }
			_summonMap = StartCastScript(scene);
			_summonFrames = 0;
			if (_summonMap < 0)
			{
				EndSummonScene(results);
				return;
			}
			void Watch()
			{
				_summonFrames++;
				bool running = false;
				try { running = GlobalScope.LogicManager.singleton().isEnableLogic((uint)_summonMap, 1) != 0; } catch (Exception) { }
				if (Ff4Cutscene.CastEnded || !running || _summonFrames > 1800)
				{
					if (_summonFrames > 1800) Log.Write(LogChannel.General, "battle: summon scene " + _summonMap + " ran too long - ended");
					After(1, () => EndSummonScene(results));   // CastEvent's terminate: a frame after BTL_EventEnd
					return;
				}
				After(1, Watch);
			}
			After(1, Watch);
		}

		/// <summary>CastEvent::initialize: s&lt;NN&gt;_00.script registered beside the map's and its cast 1 started (its map number 700 + NN).</summary>
		private static int StartCastScript(int scene)
		{
			string name = "s" + scene.ToString("00") + "_00.script";
			foreach (string path in new[] { "/SCRIPT/" + name, "/" + name, name })
			{
				try
				{
					uint size = GlobalScope.ds.g_File.getSize(path);
					if (size == 0) continue;
					Array data = GlobalScope.ds.CHeap.alloc_app(size);
					GlobalScope.ds.g_File.load(data, path);
					GlobalScope.ScriptData script = GlobalScope.ScriptData.cast(data);
					if (script == null) continue;
					int map = 700 + scene;
					GlobalScope.LogicManager manager = GlobalScope.LogicManager.singleton();
					if (manager.isRegistScriptData((uint)map) != 0) manager.removeScriptData((uint)map);
					manager.registScriptData(script);
					manager.startLogic((uint)map, 1);
					Log.Write(LogChannel.File, "battle: summon scene " + path + " started as map " + map);
					return map;
				}
				catch (Exception ex)
				{
					Log.Write(LogChannel.General, "battle: summon scene " + path + ": " + ex.Message);
				}
			}
			Log.Write(LogChannel.General, "battle: no summon scene " + name);
			return -1;
		}

		private void EndSummonScene(List<Action> results)
		{
			try
			{
				if (_summonMap >= 0)
				{
					GlobalScope.LogicManager manager = GlobalScope.LogicManager.singleton();
					if (manager.isEnableLogic((uint)_summonMap, 1) != 0) manager.stopLogic((uint)_summonMap, 1);
					if (manager.isRegistScriptData((uint)_summonMap) != 0) manager.removeScriptData((uint)_summonMap);
				}
			}
			catch (Exception ex) { Log.Write(LogChannel.General, "battle: summon scene end: " + ex.Message); }
			_summonMap = -1;
			Ff4Cutscene.CastActive = false;
			Ff4CameraMotion.Stop();
			CastStage(null);
			try { GlobalScope.stageMng.setHidden(false); } catch (Exception) { }
			// The battle camera back as it was (the display froze it), with its field of view and clip.
			Ff4EventCamera.Release();
			Ff4EventCamera.SetFov(Ff4BattleStage.CameraFov);
			Ff4EventCamera.SetClip(Ff4BattleStage.ClipNear, Ff4BattleStage.ClipFar);
			SetCamera(Ff4BattleStage.CameraPosition(_cameraType), Ff4BattleStage.CameraTarget(_cameraType));
			foreach (Npc model in _summonHidden) { try { model.Hidden = false; } catch (Exception) { } }
			_summonHidden.Clear();
			foreach (Fighter f in _party)
			{
				if (f.Npc == null || !f.Alive) continue;
				f.Npc.Teleport(f.Home);
				Face(f, f.Facing);
				Play(f, f.IdleMotion, true, 0);
				f.Acted = false;
			}
			_summonScene = false;
			_help = null;
			GlobalScope.dgs.CFade.Main().fadeIn(15);
			GlobalScope.dgs.CFade.Sub().fadeIn(15);
			// Once in: every result at once - the HP, the numbers, the falls.
			After(15, () =>
			{
				foreach (Action r in results) r();
				WaitNumbers();
			});
		}

		/// <summary>BTL_ShowHelpMessage / EraseHelpMessage: the battle's help line (babil_battle's text, or an ability's name).</summary>
		public void CastHelp(int message)
		{
			if (message < 0) { _help = null; return; }
			string text = BattleText(message, null) ?? Ff4Party.Tables?.AbilityName(message);
			_help = text ?? "(message " + message + ")";
			_helpUntil = -1;
		}

		/// <summary>BTL_SetMap / CleanupMap: the battle stage away and the summon's stage up (as a model at the origin), or that gone.</summary>
		public void CastStage(string stage)
		{
			try { _castStage?.Remove(); } catch (Exception) { }
			_castStage = null;
			if (stage == null) return;
			try { GlobalScope.stageMng.setHidden(true); } catch (Exception) { }
			try
			{
				_castStage = Game.Npcs.SpawnModel(stage, Vector3.Zero, 0f);
				if (_castStage != null) _castStage.Solid = false;
				else Log.Write(LogChannel.General, "battle: summon stage " + stage + " did not load");
			}
			catch (Exception ex) { Log.Write(LogChannel.General, "battle: summon stage " + stage + ": " + ex.Message); }
		}
	}
}
