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
			{ "ce_WaitTillEndOfCameraMotion", Nothing },     // () - no camera motion plays yet, so nothing to wait for
		};

		/// <summary>Commands that only dress a scene, skipped without a log line.</summary>
		public static readonly HashSet<string> Quiet = new HashSet<string>(StringComparer.Ordinal)
		{
			"ce_ShadowSetting", "ce_ShadowVisiblity", "ce_AddShadowVolume", "ce_ShadowVolumeONOFF",
			"ce_SetEnbleViewClip", "ce_SetSkip", "ce_StopSkip", "ce_EventSkipJump", "ce_VoiceSkipOn",
			"ce_setSound", "ce_SetupBGM", "ce_CleanupBGM", "ce_PlayBGM", "ce_StopBGM", "ce_SetVolumeBGM", "ce_StopBGM_Streaming",
			"ce_SlotBGMPlay", "ce_SlotBGMStop", "ce_SlotBGMSetVolume",
			"ce_SetupSE", "ce_CleanupSE", "ce_PlaySE", "ce_PlaySE_slot", "ce_StopSE", "ce_StopSE_slot",
			"ce_StartVoice", "ce_StartVoice2", "ce_EndVoice",
			"ce_SetLightForCharacter", "ce_SetLightEnableForCharacter", "ce_SetShadingMode", "ce_SetToonTable", "ce_CreateToonTable", "ce_setFog",
			"ce_SetupExpression", "ce_SetupExpressionAsync", "ce_CleanupExpression", "ce_ChangeExpression",
			"ce_LoadBG", "ce_setBGAlpha", "ce_setTelopMassage", "ce_setFrameWait",
			"ce_StartAnimation", "ce_PauseAnimation", "ce_SetPauseMotion", "ce_AutoRotation", "ce_setScale",
			"ce_StartMapAnimation", "ce_MapStartMotion", "ce_SetMapMotion", "ce_CleanupMap",
			"ce_SetupCameraMotion", "ce_CleanupCameraMotion",
			"ce_SetBindObject", "ce_SetBindObject2", "ce_BindObjectVisiblity",
			"ce_3DSSetup", "ce_3DSRelease", "ce_3DSSetAlpha", "ce_3DSSetPosition", "ce_3DSSetVisiblity",
			"ce_ShowMessageWindow",
		};

		private static GlobalScope.CCharacterMng Characters => GlobalScope.characterMng;

		private static bool Slot(int slot, out int ctrl) => _slots.TryGetValue(slot, out ctrl);

		private static void StartEvent(GlobalScope.ScriptEngine engine)
		{
			_active = true;
			Log.Write(LogChannel.General, "script: FF4 cutscene starts on " + GlobalScope.stg.CStageMng.CurrentName);
		}

		private static void EndEvent(GlobalScope.ScriptEngine engine)
		{
			_active = false;
			Log.Write(LogChannel.General, "script: FF4 cutscene ends, " + _slots.Count + " character(s) still up");
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

		private static void Guard(string what, Action action)
		{
			try { action(); }
			catch (Exception ex) { Log.Write(LogChannel.General, "script: FF4 cutscene " + what + ": " + ex.Message); }
		}

		/// <summary>Leaving the map: the scene's characters go with it.</summary>
		public static void MapLeft()
		{
			_slots.Clear();
			_active = false;
		}
	}
}
