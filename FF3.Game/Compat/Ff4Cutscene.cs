// FF4's cutscene engine, first cut: the character side.
//
// FF4's story scenes (e01_00, the Red Wings over Baron, and forty more maps) run on the
// `ce_*` command family: a scene sets up numbered character slots (model + texture), binds
// motion files to them, starts motions by id, places, turns, shows and hides them, plays
// camera motions from a .dsc set, changes expressions (face textures), lights and toon
// tables, voices and BGM slots. Read from the FF4 binary (Tools/ff4_calls.py), the
// character commands map straight onto CCharacterMng - the same manager FF3's field uses
// (setCharacterWithTexture, addMotion, startMotion, setCurrentFrame, setPosition,
// setRotation, setHidden, setTransparency, delCharacter), so those run here. Camera
// motions (CMS2 in EVT_CAMERA.dat), expressions, lights and the rest are skipped, quietly
// where they only dress the scene and with a log line where the scene loses something.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Audio;

namespace FF3
{
	internal static class Ff4Cutscene
	{
		// slot (the scripts' byte) -> the character manager's index
		private static readonly Dictionary<int, int> _slots = new Dictionary<int, int>();
		private static bool _active;

		public static bool Active => _active;

		/// <summary>Commands by name, as ScriptOpsFf4 spells them; ScriptCommands puts them in the table.</summary>
		public static readonly Dictionary<string, GlobalScope.SCRIPT_COMMAND> ByName = new Dictionary<string, GlobalScope.SCRIPT_COMMAND>(StringComparer.Ordinal)
		{
			{ "ce_StartEvent", StartEvent },                 // ()
			{ "ce_EndEvent", EndEvent },                     // ()
			{ "ce_SetupCharacter", SetupCharacter },         // (slot, model, texture)
			{ "ce_SetCharecterAsync", SetCharacterAsync },   // (slot, model, texture, ?)
			{ "ce_WaitSetCharacter", ReadByte },             // (slot) - loads are synchronous here
			{ "ce_CleanupCharacter", CleanupCharacter },     // (slot)
			{ "ce_DisplayCharacter", DisplayCharacter },     // (slot, shown)
			{ "ce_SetupMotion", SetupMotion },               // (slot, motion file)
			{ "ce_SetMotionAsync", SetMotionAsync },         // (slot, motion file, ?)
			{ "ce_WaitSetMotion", ReadByte },                // (slot)
			{ "ce_CleanupMotion", CleanupMotion },           // (slot, motion file)
			{ "ce_StartMotion", StartMotion },               // (slot, motion id, loop, frame, ?)
			{ "ce_WaitTillEndOfMotion", WaitTillEndOfMotion },   // (slot)
			{ "ce_EndMotionCharacter", WaitTillEndOfMotion },    // (slot)
			{ "ce_setPosition", SetPosition },               // (slot, x, y, z)
			{ "ce_setRotation", SetRotation },               // (slot, x, y, z)
			{ "ce_CharaAlpha", CharaAlpha },                 // (slot, alpha, frames)
			{ "ce_CameraPos", CameraPos },                   // (x, y, z, frames)
			{ "ce_CameraTarget", CameraTarget },             // (x, y, z, frames)
			{ "ce_SetMapMotion", SetMapMotion },             // (slot, pack name): an .ncap pack for the stage model
			{ "ce_MapStartMotion", MapStartMotion },         // (motion id, loop, ?, blend frames)
			{ "ce_StartMapAnimation", StartMapAnimation },   // (animation index, type): a .namp animation by index
			{ "ce_SetupCameraMotion", SetupCameraMotion },   // (slot, set name): a .dsc set from EVT_CAMERA
			{ "ce_CleanupCameraMotion", CleanupCameraMotion }, // (slot)
			{ "ce_PlayCameraMotion", PlayCameraMotion },     // (slot, motion id, ?, loop)
			{ "ce_WaitTillEndOfCameraMotion", WaitTillEndOfCameraMotion }, // ()
			{ "eventCameraSetFovyMove", SetFovyMove },       // (degrees, frames)
			{ "ce_SetupExpression", SetupExpression },       // (slot, face pack): p00_001 -> FACE.dat's p00_001.face.lz
			{ "ce_SetupExpressionAsync", SetupExpression },  // the same, loads are synchronous here
			{ "ce_CleanupExpression", CleanupExpression },   // (slot)
			{ "ce_ChangeExpression", ChangeExpression },     // (slot, 0 eye | 1 mouth, texture index) - FF3's ChangeFaceEye/Mouth
			{ "ce_setFrameWait", FrameWait },                // (frames): a wait
			{ "ce_SetMap", SetMap },                         // (map): the scene's stage - every script but one names its own map
			{ "ce_CreateToonTable", CreateToonTable },       // (index, r, g, b): entries 0..31; index 100 applies the table
			{ "effectLoadAsync", EffectLoad },               // (pack): /EFFECT/<pack>.efp, loaded at once
			{ "cleanUpEffectData2", EffectUnload },          // (pack)
			{ "ce_CallBattle", CallBattle },                 // (battle, ?, ?, return map, x, y, z): the OpenFF battle here, then the return map
			{ "conteEventJumpAndReturnMapJamp", ConteEventJump }, // (event, part, return map, x, y, z): play scene e<event>_<part>, come back here
			{ "ce_setConteNextPart", SetConteNextPart },     // (map, x, y, z): where the scene chain ends up
			{ "ce_SetupBGM", ReadDword },                    // (bgm): loads are on demand here
			{ "ce_PlayBGM", PlayBgm },                       // (bgm)
			{ "ce_SlotBGMPlay", SlotBgmPlay },               // (slot, bgm)
			{ "ce_SlotBGMStop", SlotBgmStop },               // (slot, fade frames)
			{ "ce_StopBGM", StopBgm },                       // (fade frames): every slot
			{ "ce_SetVolumeBGM", SetVolumeBgm },             // (volume, frames)
			{ "ce_PlaySE", PlaySe },                         // (bank, number, volume, pan)
			{ "ce_PlaySE_slot", PlaySeSlot },                // (slot, bank, number, volume, pan)
			{ "ce_StopSE_slot", StopSeSlot },                // (slot, fade)
			{ "ce_SlotBGMSetVolume", SlotBgmSetVolume },     // (slot, volume, frames)
			{ "ce_StopSE", StopSe },                         // (bank): stops what this scene played
			{ "ce_StartVoice", StartVoice },                 // (file.ahx): SOUND/VOICE/<lang>_<file>.akb
			{ "ce_StartVoice2", StartVoice2 },               // (file.ahx, ?, ?, ?, ?)
			{ "ce_EndVoice", EndVoice },                     // (): waits for the line to finish
			// The 2D plates come in a scene spelling (ce_3DS*) and a field one (3DS*, which
			// Simplify prefixes with "_"); same operands, same handlers.
			{ "ce_3DSSetup", SpriteSetup },                  // (slot, .ncer, .nanr, .ncbr, .nclr): a 2D plate over the scene
			{ "ce_3DSRelease", SpriteRelease },              // (slot)
			{ "ce_3DSSetAlpha", SpriteSetAlpha },            // (slot, from, to, frames): 0..31
			{ "ce_3DSSetPosition", SpriteSetPosition },      // (slot, x, y)
			{ "ce_3DSSetVisiblity", SpriteSetVisibility },   // (slot, shown)
			{ "_3DSSetup", SpriteSetup },
			{ "_3DSRelease", SpriteRelease },
			{ "_3DSSetAlpha", SpriteSetAlpha },
			{ "_3DSSetPosition", SpriteSetPosition },
			{ "_3DSSetVisiblity", SpriteSetVisibility },
			{ "ce_SetShadingMode", SetShadingMode },         // (slot, 0 flat-lit | 1 toon)
			{ "ce_SetLightForCharacter", SetLight },         // (slot, light 0..3, x, y, z, r, g, b): the global light
		};

		/// <summary>Commands that only dress a scene, skipped without a log line.</summary>
		public static readonly HashSet<string> Quiet = new HashSet<string>(StringComparer.Ordinal)
		{
			"ce_ShadowSetting", "ce_ShadowVisiblity", "ce_AddShadowVolume", "ce_ShadowVolumeONOFF",
			"ce_SetEnbleViewClip", "ce_SetSkip", "ce_StopSkip", "ce_EventSkipJump", "ce_VoiceSkipOn",
			"ce_setSound", "ce_CleanupBGM", "ce_StopBGM_Streaming",
			"ce_SetupSE", "ce_CleanupSE",
			"3DSSetup", "3DSRelease", "3DSSetAlpha", "3DSSetPosition", "3DSSetVisiblity",
			"_3DSSetup", "_3DSRelease", "_3DSSetAlpha", "_3DSSetPosition", "_3DSSetVisiblity",
			"ce_SetLightEnableForCharacter", "ce_SetToonTable", "ce_setFog",
			"ce_SetupExpression", "ce_SetupExpressionAsync", "ce_CleanupExpression", "ce_ChangeExpression",
			"ce_LoadBG", "ce_setBGAlpha", "ce_setTelopMassage",
			"ce_StartAnimation", "ce_PauseAnimation", "ce_SetPauseMotion", "ce_AutoRotation", "ce_setScale",
			"ce_CleanupMap",
			"ce_SetupCameraMotion", "ce_CleanupCameraMotion",
			"ce_SetBindObject", "ce_SetBindObject2", "ce_BindObjectVisiblity",
			"ce_3DSSetup", "ce_3DSRelease", "ce_3DSSetAlpha", "ce_3DSSetPosition", "ce_3DSSetVisiblity",
			"ce_ShowMessageWindow",
		};

		private static GlobalScope.CCharacterMng Characters => GlobalScope.characterMng;

		private static bool Slot(int slot, out int ctrl) => _slots.TryGetValue(slot, out ctrl);

		/// <summary>The stage the running scene started on: a stage-name change to this map is the scene's own arrival, not a departure.</summary>
		public static string SceneStage { get; private set; }

		private static void StartEvent(GlobalScope.ScriptEngine engine)
		{
			_active = true;
			SceneStage = GlobalScope.stg.CStageMng.CurrentName;
			Log.Write(LogChannel.General, "script: FF4 cutscene starts on " + GlobalScope.stg.CStageMng.CurrentName);
		}

		private static void EndEvent(GlobalScope.ScriptEngine engine)
		{
			_active = false;
			Ff4CameraMotion.Stop();
			Log.Write(LogChannel.General, "script: FF4 cutscene ends, " + _slots.Count + " character(s) still up");
			// FF4's event part hands back to the world at the return map; here the scene ran on the
			// world part all along, so the hand-back is a map jump.
			if (!string.IsNullOrEmpty(ReturnMap) && !string.Equals(ReturnMap, GlobalScope.stg.CStageMng.CurrentName, StringComparison.OrdinalIgnoreCase))
			{
				string map = ReturnMap;
				ReturnMap = null;
				JumpTo(map, ReturnPosition);
			}
		}

		// ---- the story-scene chain: EventConteParameter's return map and player position ----

		/// <summary>Where the scene chain returns to (conteEventJumpAndReturnMapJamp, ce_setConteNextPart, ce_CallBattle).</summary>
		public static string ReturnMap;
		public static GlobalScope.VecFx32 ReturnPosition = new GlobalScope.VecFx32(0, 0, 0);

		private static void ConteEventJump(GlobalScope.ScriptEngine engine)
		{
			int ev = engine.getByte();
			int part = engine.getByte();
			string returnMap = engine.getString();
			int x = (int)engine.getDword(), y = (int)engine.getDword(), z = (int)engine.getDword();
			ReturnMap = returnMap;
			ReturnPosition = new GlobalScope.VecFx32(x, y, z);
			string scene = "e" + ev.ToString("00") + "_" + part.ToString("00");
			Log.Write(LogChannel.General, "script: FF4 story scene " + scene + ", back to " + returnMap + " at (" + x / 4096 + "," + y / 4096 + "," + z / 4096 + ")");
			JumpTo(scene, new GlobalScope.VecFx32(0, 0, 0));
		}

		private static void SetConteNextPart(GlobalScope.ScriptEngine engine)
		{
			string map = engine.getString();
			int x = (int)engine.getDword(), y = (int)engine.getDword(), z = (int)engine.getDword();
			ReturnMap = map;
			if (x != 0 || y != 0 || z != 0)
			{
				ReturnPosition = new GlobalScope.VecFx32(x, y, z);
			}
			Log.Write(LogChannel.File, "script: FF4 scene chain ends at " + map);
		}

		private static void JumpTo(string map, GlobalScope.VecFx32 position)
		{
			Guard("jump to " + map, () =>
			{
				if (position.x == 0 && position.y == 0 && position.z == 0)
				{
					// The world camera reads an all-zero position as unset (black screen).
					position = new GlobalScope.VecFx32(0, 0, 4096);
				}
				GlobalScope.VecFx32 rot = new GlobalScope.VecFx32(0, 0, 0);
				GlobalScope.CCastCommandTransit.getInstance().castParam_MapJump().initialize();
				GlobalScope.CCastCommandTransit.getInstance().castParam_MapJump().setUp(map, 0, position, rot, true);
				GlobalScope.CCastCommandTransit.getInstance().cast_BaseSystem().setMapJump(true);
			});
		}

		private static void Nothing(GlobalScope.ScriptEngine engine) { }
		private static void ReadByte(GlobalScope.ScriptEngine engine) { engine.getByte(); }

		private static void SetupCharacter(GlobalScope.ScriptEngine engine)
		{
			int slot = engine.getByte();
			string model = engine.getString();
			string texture = engine.getString();
			Setup(slot, model, texture);
		}

		private static void SetCharacterAsync(GlobalScope.ScriptEngine engine)
		{
			int slot = engine.getByte();
			string model = engine.getString();
			string texture = engine.getString();
			engine.getDword();
			Setup(slot, model, texture);
		}

		private static void Setup(int slot, string model, string texture)
		{
			try
			{
				if (Slot(slot, out int old))
				{
					Characters.delCharacter(old);
					_slots.Remove(slot);
				}
				GlobalScope.TexDivideLoader.getSingleton().tdlForceLoad();
				int ctrl = Characters.setCharacterWithTexture(model, string.IsNullOrEmpty(texture) ? model : texture, GlobalScope.CCharacterMng.PRI_SCENE.PRI_SCENE_FIRST);
				if (ctrl < 0)
				{
					ctrl = Characters.setCharacter(model, GlobalScope.CCharacterMng.PRI_SCENE.PRI_SCENE_FIRST);
				}
				GlobalScope.TexDivideLoader.getSingleton().tdlForceLoad();
				if (ctrl < 0)
				{
					Log.Write(LogChannel.General, "script: FF4 cutscene: character " + model + " (" + texture + ") did not load for slot " + slot);
					return;
				}
				_slots[slot] = ctrl;
				Characters.setHidden(ctrl, false);
				Log.Write(LogChannel.File, "script: FF4 cutscene slot " + slot + " = " + model + " (" + texture + ") as character " + ctrl);
			}
			catch (Exception ex)
			{
				Log.Write(LogChannel.General, "script: FF4 cutscene: setting up " + model + ": " + ex.Message);
			}
		}

		private static void CleanupCharacter(GlobalScope.ScriptEngine engine)
		{
			int slot = engine.getByte();
			if (!Slot(slot, out int ctrl)) return;
			try { Characters.delCharacter(ctrl); } catch (Exception) { }
			_slots.Remove(slot);
		}

		private static void DisplayCharacter(GlobalScope.ScriptEngine engine)
		{
			int slot = engine.getByte();
			int shown = engine.getByte();
			if (Slot(slot, out int ctrl)) Characters.setHidden(ctrl, shown == 0);
		}

		private static void SetupMotion(GlobalScope.ScriptEngine engine)
		{
			int slot = engine.getByte();
			string motion = engine.getString();
			if (Slot(slot, out int ctrl)) Guard("motion " + motion, () => Characters.addMotion(ctrl, motion));
		}

		private static void SetMotionAsync(GlobalScope.ScriptEngine engine)
		{
			int slot = engine.getByte();
			string motion = engine.getString();
			engine.getDword();
			if (Slot(slot, out int ctrl)) Guard("motion " + motion, () => Characters.addMotion(ctrl, motion));
		}

		private static void CleanupMotion(GlobalScope.ScriptEngine engine)
		{
			int slot = engine.getByte();
			string motion = engine.getString();
			if (Slot(slot, out int ctrl)) Guard("remove motion " + motion, () => Characters.removeMotion(ctrl, motion));
		}

		private static void StartMotion(GlobalScope.ScriptEngine engine)
		{
			int slot = engine.getByte();
			uint motion = engine.getDword();
			int loop = engine.getByte();
			uint frame = engine.getDword();
			engine.getDword();
			if (!Slot(slot, out int ctrl)) return;
			Guard("start motion " + motion, () =>
			{
				Characters.startMotion(ctrl, (int)motion, loop != 0, 0u);
				if (frame != 0) Characters.setCurrentFrame(ctrl, frame);
			});
		}

		private static void WaitTillEndOfMotion(GlobalScope.ScriptEngine engine)
		{
			int slot = engine.getByte();
			if (!Slot(slot, out int ctrl)) return;
			bool done = true;
			try { done = Characters.isEndOfMotion(ctrl); } catch (Exception) { }
			if (!done) engine.suspendRedo();
		}

		private static void SetPosition(GlobalScope.ScriptEngine engine)
		{
			int slot = engine.getByte();
			int x = (int)engine.getDword(), y = (int)engine.getDword(), z = (int)engine.getDword();
			if (Slot(slot, out int ctrl)) Guard("position", () => Characters.setPosition(ctrl, x, y, z));
		}

		private static void SetRotation(GlobalScope.ScriptEngine engine)
		{
			int slot = engine.getByte();
			ushort x = (ushort)engine.getWord(), y = (ushort)engine.getWord(), z = (ushort)engine.getWord();
			if (Slot(slot, out int ctrl)) Guard("rotation", () => Characters.setRotation(ctrl, x, y, z));
		}

		private static void CharaAlpha(GlobalScope.ScriptEngine engine)
		{
			int slot = engine.getByte();
			int alpha = (int)engine.getWord();
			engine.getWord();
			if (Slot(slot, out int ctrl)) Guard("alpha", () => Characters.setTransparency(ctrl, Math.Clamp(alpha, 0, 31)));
		}

		// The scene camera: a free camera at a point, looking at a point (frames ignored).
		private static void CameraPos(GlobalScope.ScriptEngine engine)
		{
			int x = (int)engine.getDword(), y = (int)engine.getDword(), z = (int)engine.getDword();
			engine.getDword();
			Guard("camera position", () =>
			{
				GlobalScope.cmr.CWorldCamera camera = GlobalScope.CCastCommandTransit.getInstance().cast_FieldCamera();
				if (camera == null) return;
				camera.Mode_set(GlobalScope.cmr.CWorldCamera.MODE.MODE_FREE);
				camera.Pos_set(new GlobalScope.VecFx32(x, y, z));
				GlobalScope.VEC_Set(camera.PosOffset(), 0, 0, 0);
			});
		}

		private static void CameraTarget(GlobalScope.ScriptEngine engine)
		{
			int x = (int)engine.getDword(), y = (int)engine.getDword(), z = (int)engine.getDword();
			engine.getDword();
			Guard("camera target", () =>
			{
				GlobalScope.cmr.CWorldCamera camera = GlobalScope.CCastCommandTransit.getInstance().cast_FieldCamera();
				if (camera == null) return;
				camera.Mode_set(GlobalScope.cmr.CWorldCamera.MODE.MODE_FREE);
				camera.Trg_set(new GlobalScope.VecFx32(x, y, z));
			});
		}

		// ---- 2D plates (ce_3DS*): a cell sprite on the 3D plane, the "Baron" name plate and its kin ----

		private sealed class Plate
		{
			public GlobalScope.sys2d.Sprite3d Sprite;
			public int AlphaFrom, AlphaTo, AlphaFrames, AlphaTick;
		}

		private static readonly Dictionary<int, Plate> _plates = new Dictionary<int, Plate>();

		private static void SpriteSetup(GlobalScope.ScriptEngine engine)
		{
			int slot = (int)engine.getWord();
			string ce = engine.getString(), an = engine.getString(), cb = engine.getString(), cl = engine.getString();
			Guard("2D plate " + ce, () =>
			{
				ReleasePlate(slot);
				GlobalScope.sys2d.Sprite3d sprite = new GlobalScope.sys2d.Sprite3d();
				sprite.Load(GlobalScope.sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN3D, Path.GetFileName(ce), Path.GetFileName(an), Path.GetFileName(cb), Path.GetFileName(cl));
				sprite.SetCell(0);
				sprite.SetShow(false);
				GlobalScope.sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(sprite);
				_plates[slot] = new Plate { Sprite = sprite, AlphaFrom = 31, AlphaTo = 31 };
				Log.Write(LogChannel.File, "script: FF4 2D plate " + Path.GetFileName(ce) + " in slot " + slot);
			});
		}

		private static void ReleasePlate(int slot)
		{
			if (!_plates.TryGetValue(slot, out Plate plate)) return;
			_plates.Remove(slot);
			try
			{
				GlobalScope.sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(plate.Sprite);
				plate.Sprite.Release();
			}
			catch (Exception) { }
		}

		private static void SpriteRelease(GlobalScope.ScriptEngine engine)
		{
			ReleasePlate((int)engine.getWord());
		}

		private static void SpriteSetAlpha(GlobalScope.ScriptEngine engine)
		{
			int slot = (int)engine.getWord();
			int from = (int)engine.getDword(), to = (int)engine.getDword(), frames = (int)engine.getDword();
			if (!_plates.TryGetValue(slot, out Plate plate)) return;
			plate.AlphaFrom = Math.Clamp(from, 0, 31);
			plate.AlphaTo = Math.Clamp(to, 0, 31);
			plate.AlphaFrames = Math.Max(0, frames);
			plate.AlphaTick = 0;
			Guard("2D plate alpha", () => plate.Sprite.SetAlpha((byte)plate.AlphaFrom));
		}

		private static void SpriteSetPosition(GlobalScope.ScriptEngine engine)
		{
			int slot = (int)engine.getWord();
			int x = (int)engine.getDword(), y = (int)engine.getDword();
			if (!_plates.TryGetValue(slot, out Plate plate)) return;
			Guard("2D plate position", () => plate.Sprite.SetPositionI(x, y));
		}

		private static void SpriteSetVisibility(GlobalScope.ScriptEngine engine)
		{
			int slot = (int)engine.getWord();
			int shown = (int)engine.getDword();
			if (!_plates.TryGetValue(slot, out Plate plate)) return;
			Guard("2D plate show", () => plate.Sprite.SetShow(shown != 0));
		}

		/// <summary>Each frame: the plates' alpha fades.</summary>
		public static void Tick()
		{
			foreach (Plate plate in _plates.Values)
			{
				if (plate.AlphaFrames <= 0) continue;
				plate.AlphaTick++;
				int alpha = plate.AlphaTick >= plate.AlphaFrames
					? plate.AlphaTo
					: plate.AlphaFrom + (plate.AlphaTo - plate.AlphaFrom) * plate.AlphaTick / plate.AlphaFrames;
				if (plate.AlphaTick >= plate.AlphaFrames) plate.AlphaFrames = 0;
				try { plate.Sprite.SetAlpha((byte)alpha); } catch (Exception) { }
			}
		}

		// ---- sound: BGM slots, sound effects and the voice lines ----

		private static void ReadDword(GlobalScope.ScriptEngine engine) { engine.getDword(); }

		private static GlobalScope.MatrixSound.enMtxBGMSlot BgmSlot(int slot)
		{
			return (GlobalScope.MatrixSound.enMtxBGMSlot)Math.Clamp(slot, 0, 3);
		}

		private static void PlayBgm(GlobalScope.ScriptEngine engine)
		{
			int bgm = (int)engine.getDword();
			Guard("bgm " + bgm, () => GlobalScope.MatrixSound.MtxSoundBGM.getSingleton().play(bgm, 127, 0, GlobalScope.MatrixSound.enMtxBGMSlot.enMTX_BGM_SLOT0));
		}

		private static void SlotBgmPlay(GlobalScope.ScriptEngine engine)
		{
			int slot = (int)engine.getDword();
			int bgm = (int)engine.getDword();
			Guard("bgm " + bgm, () => GlobalScope.MatrixSound.MtxSoundBGM.getSingleton().play(bgm, 127, 0, BgmSlot(slot)));
		}

		private static void SlotBgmStop(GlobalScope.ScriptEngine engine)
		{
			int slot = (int)engine.getDword();
			int frames = (int)engine.getDword();
			Guard("bgm stop", () => GlobalScope.MatrixSound.MtxSoundBGM.getSingleton().stop(frames, BgmSlot(slot)));
		}

		private static void StopBgm(GlobalScope.ScriptEngine engine)
		{
			int frames = (int)engine.getDword();
			Guard("bgm stop", () =>
			{
				for (int slot = 0; slot < 4; slot++)
				{
					GlobalScope.MatrixSound.MtxSoundBGM.getSingleton().stop(frames, BgmSlot(slot));
				}
			});
		}

		private static void SlotBgmSetVolume(GlobalScope.ScriptEngine engine)
		{
			int slot = (int)engine.getDword();
			int volume = (int)engine.getDword();
			int frames = (int)engine.getDword();
			Guard("bgm volume", () => GlobalScope.MatrixSound.MtxSoundBGM.getSingleton().setVolume(volume, frames, BgmSlot(slot)));
		}

		private static void SetVolumeBgm(GlobalScope.ScriptEngine engine)
		{
			int volume = (int)engine.getDword();
			int frames = (int)engine.getDword();
			Guard("bgm volume", () =>
			{
				for (int slot = 0; slot < 4; slot++)
				{
					GlobalScope.MatrixSound.MtxSoundBGM.getSingleton().setVolume(volume, frames, BgmSlot(slot));
				}
			});
		}

		private static readonly Dictionary<int, GlobalScope.MatrixSound.MtxSEHandle> _seSlots = new Dictionary<int, GlobalScope.MatrixSound.MtxSEHandle>();
		private static readonly List<GlobalScope.MatrixSound.MtxSEHandle> _sePlayed = new List<GlobalScope.MatrixSound.MtxSEHandle>();

		private static void PlaySe(GlobalScope.ScriptEngine engine)
		{
			int bank = (int)engine.getDword(), number = (int)engine.getDword(), volume = (int)engine.getDword(), pan = (int)engine.getDword();
			Guard("se", () =>
			{
				GlobalScope.MatrixSound.MtxSEHandle handle = GlobalScope.MatrixSound.MtxSENDS_Play(bank, number, volume, pan);
				if (handle != null) _sePlayed.Add(handle);
			});
		}

		private static void PlaySeSlot(GlobalScope.ScriptEngine engine)
		{
			int slot = (int)engine.getDword();
			int bank = (int)engine.getDword(), number = (int)engine.getDword(), volume = (int)engine.getDword(), pan = (int)engine.getDword();
			Guard("se", () =>
			{
				GlobalScope.MatrixSound.MtxSEHandle handle = GlobalScope.MatrixSound.MtxSENDS_Play(bank, number, volume, pan);
				if (handle != null) { _seSlots[slot] = handle; _sePlayed.Add(handle); }
			});
		}

		private static void StopSeSlot(GlobalScope.ScriptEngine engine)
		{
			int slot = (int)engine.getDword();
			engine.getDword();
			if (_seSlots.TryGetValue(slot, out GlobalScope.MatrixSound.MtxSEHandle handle))
			{
				Guard("se stop", () => GlobalScope.MatrixSound.MtxSENDS_Stop(handle, 0));
				_seSlots.Remove(slot);
			}
		}

		private static void StopSe(GlobalScope.ScriptEngine engine)
		{
			engine.getDword();
			Guard("se stop", () =>
			{
				foreach (GlobalScope.MatrixSound.MtxSEHandle handle in _sePlayed) GlobalScope.MatrixSound.MtxSENDS_Stop(handle, 0);
				_sePlayed.Clear();
				_seSlots.Clear();
			});
		}

		// The voice lines: files/SOUND/VOICE/<lang>_<name>.akb, one SoundEffect at a time.
		private static SoundEffectInstance _voice;
		private static string _voiceLanguage = "en";

		public static bool VoicePlaying => _voice != null && _voice.State == SoundState.Playing;

		private static void StartVoice(GlobalScope.ScriptEngine engine)
		{
			string file = engine.getString();
			PlayVoice(file);
		}

		private static void StartVoice2(GlobalScope.ScriptEngine engine)
		{
			string file = engine.getString();
			engine.getByte();
			engine.getByte();
			engine.getDword();
			engine.getWord();
			PlayVoice(file);
		}

		private static void PlayVoice(string file)
		{
			if (string.IsNullOrEmpty(file) || FF3.Options.Get("novoice") != null) return;
			string name = Path.GetFileNameWithoutExtension(file);
			Guard("voice " + name, () =>
			{
				// Some lines exist only in Japanese (861 ja_ files to 698 en_): fall back rather than go silent.
				SoundEffect sound = FF3.OggSound.Load(_voiceLanguage + "_" + name) ?? FF3.OggSound.Load("ja_" + name) ?? FF3.OggSound.Load(name);
				if (sound == null)
				{
					Log.Write(LogChannel.File, "script: FF4 voice " + name + " not found");
					return;
				}
				if (_voice != null)
				{
					_voice.Stop();
					_voice.Dispose();
				}
				_voice = sound.CreateInstance();
				_voice.Play();
				Log.Write(LogChannel.File, "script: FF4 voice " + name + " (" + sound.Duration.TotalSeconds.ToString("0.0") + "s)");
			});
		}

		private static void EndVoice(GlobalScope.ScriptEngine engine)
		{
			if (VoicePlaying) engine.suspendRedo();
		}

		private static void StopVoice()
		{
			if (_voice != null)
			{
				try { _voice.Stop(); _voice.Dispose(); } catch (Exception) { }
				_voice = null;
			}
		}

		// ---- effect packs and the scene battle ----

		private static void EffectLoad(GlobalScope.ScriptEngine engine)
		{
			string pack = engine.getString();
			Guard("effect pack " + pack, () =>
			{
				if (!GlobalScope.eff.CEffectMng.instance().loadEfpNamed(pack, "/EFFECT/" + pack + ".efp"))
				{
					Log.Write(LogChannel.General, "script: FF4 effect pack " + pack + " did not load");
				}
			});
		}

		private static void EffectUnload(GlobalScope.ScriptEngine engine)
		{
			string pack = engine.getString();
			Guard("effect pack " + pack, () => GlobalScope.eff.CEffectMng.instance().unLoadEfpNamed(pack));
		}

		// Scenes whose battle is running: the script holds at ce_CallBattle (the battle part took
		// over in FF4; the story goes on from the return map, never from the line after).
		private static readonly HashSet<GlobalScope.ScriptEngine> _inBattle = new HashSet<GlobalScope.ScriptEngine>();

		private static void CallBattle(GlobalScope.ScriptEngine engine)
		{
			int battle = (int)engine.getWord();
			engine.getByte();
			engine.getByte();
			string returnMap = engine.getString();
			int x = (int)engine.getDword(), y = (int)engine.getDword(), z = (int)engine.getDword();
			GlobalScope.VecFx32 position = new GlobalScope.VecFx32(x, y, z);
			if (_inBattle.Contains(engine))
			{
				// Still fighting, or fought and waiting for the jump to take the scene away.
				engine.suspendRedo();
				return;
			}
			// The OpenFF battle fights the encounter group where the scene stands and then jumps on;
			// when it cannot (no field hero in this scene), the jump happens at once.
			if (Ff4Battle.Instance != null && Ff4Battle.Instance.StartParty(battle, true))
			{
				Log.Write(LogChannel.General, "script: FF4 scene battle " + battle + " - then on to " + returnMap);
				string map = returnMap;
				_inBattle.Add(engine);
				Ff4Battle.Instance.AfterBattle = () => { if (!string.IsNullOrEmpty(map)) JumpTo(map, position); };
				engine.suspendRedo();
				return;
			}
			Log.Write(LogChannel.General, "script: FF4 scene battle " + battle + " skipped - on to " + returnMap);
			if (string.IsNullOrEmpty(returnMap)) return;
			// The battle would return to this map; the chain's own return map stays for the scene after it.
			JumpTo(returnMap, position);
		}

		// ---- lights and shading, read from the FF4 handlers ----

		private static readonly ushort[] _toon = new ushort[32];

		private static void CreateToonTable(GlobalScope.ScriptEngine engine)
		{
			int index = (short)engine.getWord();
			uint r = engine.getWord(), g = engine.getWord(), b = engine.getWord();
			if (index >= 0 && index <= 31)
			{
				_toon[index] = (ushort)((r & 31) | ((g & 31) << 5) | ((b & 31) << 10));
				return;
			}
			Guard("toon table", () =>
			{
				GlobalScope.G3X_SetToonTable(_toon);
				GlobalScope.G3X_SetShading(0);
			});
		}

		private static void SetShadingMode(GlobalScope.ScriptEngine engine)
		{
			int slot = engine.getByte();
			int mode = engine.getByte();
			if (!Slot(slot, out int ctrl)) return;
			Guard("shading mode", () =>
			{
				if (mode == 1)
				{
					Characters.setPolygonMode(ctrl, GlobalScope.GXPolygonMode.GX_POLYGONMODE_TOON);
					Characters.setDiffuse(ctrl, 0x7fff);
					Characters.setAmbient(ctrl, 0);
					Characters.setSpecular(ctrl, 0);
					Characters.setEmission(ctrl, 0);
				}
				else
				{
					Characters.setPolygonMode(ctrl, GlobalScope.GXPolygonMode.GX_POLYGONMODE_MODULATE);
					Characters.setDiffuse(ctrl, 0x6739);
					Characters.setAmbient(ctrl, 0x7fff);
					Characters.setSpecular(ctrl, 0);
					Characters.setEmission(ctrl, 0x7fff);
				}
			});
		}

		private static void SetLight(GlobalScope.ScriptEngine engine)
		{
			engine.getByte();
			int light = engine.getByte();
			int x = (int)engine.getDword(), y = (int)engine.getDword(), z = (int)engine.getDword();
			int r = engine.getByte(), g = engine.getByte(), b = engine.getByte();
			if (light < 0 || light > 3 || (x == 0 && y == 0 && z == 0)) return;
			Guard("light", () =>
			{
				GlobalScope.NNS_G3dGlbLightVector((GlobalScope.GXLightId)light, (short)Math.Clamp(x, -4096, 4096), (short)Math.Clamp(y, -4096, 4096), (short)Math.Clamp(z, -4096, 4096));
				GlobalScope.NNS_G3dGlbLightColor((GlobalScope.GXLightId)light, (ushort)((r & 31) | ((g & 31) << 5) | ((b & 31) << 10)));
			});
		}

		// ---- expressions: the chain textures FF3's face commands use, eye/eye_pl and mouth/mouth_pl ----

		private static void SetupExpression(GlobalScope.ScriptEngine engine)
		{
			int slot = engine.getByte();
			string pack = engine.getString();
			if (!Slot(slot, out int ctrl)) return;
			Guard("expression pack " + pack, () =>
			{
				GlobalScope.TexDivideLoader.getSingleton().tdlForceLoad();
				if (!Characters.setChainTexture(ctrl, pack + ".face"))
				{
					Log.Write(LogChannel.General, "script: FF4 cutscene: face pack " + pack + " did not load for slot " + slot);
				}
				GlobalScope.TexDivideLoader.getSingleton().tdlForceLoad();
			});
		}

		private static void CleanupExpression(GlobalScope.ScriptEngine engine)
		{
			int slot = engine.getByte();
			if (!Slot(slot, out int ctrl)) return;
			Guard("expression cleanup", () =>
			{
				GlobalScope.TexDivideLoader.getSingleton().tdlForceLoad();
				Characters.delChainTexture(ctrl);
			});
		}

		private static void ChangeExpression(GlobalScope.ScriptEngine engine)
		{
			int slot = engine.getByte();
			int which = engine.getByte();
			uint index = engine.getDword();
			if (!Slot(slot, out int ctrl)) return;
			if (!GlobalScope.TexDivideLoader.getSingleton().tdlIsEmpty())
			{
				engine.suspendRedo();
				return;
			}
			Guard("expression", () =>
			{
				string part = which == 1 ? "mouth" : "eye";
				Characters.bindChainTexel(ctrl, index, part);
				Characters.bindChainPltt(ctrl, index, part + "_pl");
			});
		}

		// ---- waits, FOV, the stage ----

		private static readonly Dictionary<GlobalScope.ScriptEngine, int> _frameWaits = new Dictionary<GlobalScope.ScriptEngine, int>();

		private static void FrameWait(GlobalScope.ScriptEngine engine)
		{
			int frames = (int)engine.getDword();
			if (!_frameWaits.TryGetValue(engine, out int left))
			{
				left = frames;
				_frameWaits[engine] = left;
			}
			if (left > 0)
			{
				_frameWaits[engine] = left - 1;
				engine.suspendRedo();
				return;
			}
			_frameWaits.Remove(engine);
		}

		private static void SetFovyMove(GlobalScope.ScriptEngine engine)
		{
			int degrees = (int)engine.getDword();
			int frames = (int)engine.getDword();
			Ff4CameraMotion.SetFovy(degrees, frames);
		}

		/// <summary>
		/// Set when a scene swaps its stage (ce_SetMap to another map, e01_01 -> e01_18): the
		/// host must not treat the new stage name as leaving the map - the scene's characters
		/// and camera carry over. Consumed by EngineHost.
		/// </summary>
		public static bool StageSwapPending;

		private static void SetMap(GlobalScope.ScriptEngine engine)
		{
			string map = engine.getString();
			if (string.Equals(map, GlobalScope.stg.CStageMng.CurrentName, StringComparison.OrdinalIgnoreCase)) return;
			string from = GlobalScope.stg.CStageMng.CurrentName;
			Guard("ce_SetMap " + map, () =>
			{
				StageSwapPending = true;
				GlobalScope.sceneMng.gotoStage(map);
				GlobalScope.stageMng.setStage(map);
				Log.Write(LogChannel.General, "script: FF4 ce_SetMap " + map + " replaces " + from + " within the scene");
			});
		}

		// ---- map motions ----

		private static void SetMapMotion(GlobalScope.ScriptEngine engine)
		{
			engine.getDword();
			string name = engine.getString();
			Guard("map motion " + name, () => GlobalScope.stageMng.addMotion(name));
		}

		private static void MapStartMotion(GlobalScope.ScriptEngine engine)
		{
			int id = (int)engine.getDword();
			int loop = engine.getByte();
			engine.getDword();
			uint blend = engine.getDword();
			Guard("map motion " + id, () => GlobalScope.stageMng.startMotion(id, loop != 0, blend));
		}

		private static void StartMapAnimation(GlobalScope.ScriptEngine engine)
		{
			uint index = engine.getDword();
			int type = engine.getByte();
			Guard("map animation " + index, () => GlobalScope.stageMng.startAnimation(index, (GlobalScope.ds.sys3d.CAnimSet.enTYPE)type, 0));
		}

		// ---- camera motions ----

		private static void SetupCameraMotion(GlobalScope.ScriptEngine engine)
		{
			int slot = (int)engine.getDword();
			string name = engine.getString();
			Ff4CameraMotion.Setup(slot, name);
		}

		private static void CleanupCameraMotion(GlobalScope.ScriptEngine engine)
		{
			Ff4CameraMotion.Cleanup((int)engine.getDword());
		}

		private static void PlayCameraMotion(GlobalScope.ScriptEngine engine)
		{
			int slot = (int)engine.getDword();
			uint id = engine.getDword();
			engine.getDword();
			int loop = engine.getByte();
			Ff4CameraMotion.Play(slot, id, loop != 0);
		}

		private static void WaitTillEndOfCameraMotion(GlobalScope.ScriptEngine engine)
		{
			if (Ff4CameraMotion.Playing && !Ff4CameraMotion.Looping) engine.suspendRedo();
		}

		private static void Guard(string what, Action action)
		{
			try { action(); }
			catch (Exception ex) { Log.Write(LogChannel.General, "script: FF4 cutscene " + what + ": " + ex.Message); }
		}

		/// <summary>Leaving the map: the scene's characters go with it.</summary>
		public static void MapLeft()
		{
			_inBattle.Clear();
			_slots.Clear();
			_frameWaits.Clear();
			_active = false;
			SceneStage = null;
			Ff4EventCamera.Release();
			StopVoice();
			_seSlots.Clear();
			_sePlayed.Clear();
			foreach (int slot in new List<int>(_plates.Keys)) ReleasePlate(slot);
			Ff4CameraMotion.MapLeft();
			// ReturnMap survives: it is where the chain of scene maps ends up.
		}
	}
}
