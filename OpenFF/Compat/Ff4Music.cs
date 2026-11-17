// FF4's music as its scripts and libff4's world states drive it, over the port's four BGM slots (WSCSound's handles at
// +0x1bc, 0x10 apart: the field's slot 0, the battle's slot 3):
//   setHalfWayBGM_Start   a map's theme - left playing when it already is, unless a battle was set up since (flag 0/2)
//   setHalfWayBGM_Play    the theme paused for a battle taken up again, its volume back over the frames given (30)
//   setMapjumpBGMOperation an exit that takes its slot's music down over 15 frames as the party goes (WSMapJumpDirection)
//   encounters            WSEncountSetting sets flag 0/2; WSEncountDirection1/2 bring the field's music down over 15
//                         frames and pause it as the battle comes; the battle plays its monster party's theme (record
//                         +0x8a, BattleBGM::startBattleBGM) in its own slot; a win stops it and plays the fanfare (BGM 9,
//                         BattleWin::changeBGMPhase) unless setBattleBGM said not to; its end takes it down over 15 frames
//                         (getPhaseEnd) unless told to leave it. setBattleBGM holds for one battle (BattlePart's
//                         doUninitialize puts OutsideToBattle back).
//   scenes                ContEventPart unloads the music as a scene starts and as it ends unless the field's script
//                         said to keep it (bgmContinueForConteEvent, ce_setSound: EventConteParameter +0x35 / +0x34).
//   soundReset            dssndUnloadBGMAll: whatever plays stops; the script starts its own after.
// The banks' loads and unloads (ce_SetupSE, ce_CleanupSE, ce_CleanupBGM, bgmDivideLoadDataTypeSpecific) read their
// operands only: the port loads a sound as it is first played.

using System;

namespace OpenFF.Client
{
	internal static class Ff4Music
	{
		private const int FieldSlot = 0, BattleSlot = 3, FanfareBgm = 9;

		private static GlobalScope.MatrixSound.MtxSoundBGM Bgm => GlobalScope.MatrixSound.MtxSoundBGM.getSingleton();

		private static GlobalScope.MatrixSound.enMtxBGMSlot Slot(int slot) => (GlobalScope.MatrixSound.enMtxBGMSlot)Math.Clamp(slot, 0, 3);

		private static bool Flag(uint index) => GlobalScope.FlagManager.singleton().get(0, index) != 0;

		private static void Guard(string what, Action a)
		{
			try { a(); } catch (Exception ex) { Log.First(LogChannel.General, "ff4music-" + what, 3, () => "music: " + what + ": " + ex.Message); }
		}

		// ---- the map's theme ----

		/// <summary>setHalfWayBGM_Start(bgm, volume, frames, slot): the theme, unless it already plays there and no battle
		/// came between (flag 0/2) - walking between a dungeon's floors keeps it going.</summary>
		public static void HalfWayStart(GlobalScope.ScriptEngine engine)
		{
			int bgm = engine.getWord(), volume = engine.getWord(), frames = engine.getWord(), slot = engine.getWord();
			if (frames == 0xFFFF) frames = 0;
			Guard("start", () =>
			{
				GlobalScope.MatrixSound.enMtxBGMSlot s = Slot(slot);
				if (GlobalScope.MatrixSound.MtxBGMNDS_GetPlayBGMNo(s) == bgm && !Flag(2))
				{
					Log.Write(LogChannel.File, "music: theme " + bgm + " plays on in slot " + slot);
					return;
				}
				Bgm.stop(0, s);
				Bgm.play(bgm, volume, frames, s);
			});
		}

		/// <summary>setHalfWayBGM_Play(volume, frames, slot, ?, ?): the slot's paused theme goes on, its volume back over the
		/// frames (30 when 0xFFFF).</summary>
		public static void HalfWayPlay(GlobalScope.ScriptEngine engine)
		{
			int volume = engine.getWord(), frames = engine.getWord(), slot = engine.getWord();
			engine.getWord();
			engine.getWord();
			if (frames == 0xFFFF) frames = 30;
			Guard("play", () =>
			{
				Bgm.pause(false, Slot(slot));
				Bgm.setVolume(volume, frames, Slot(slot));
			});
		}

		// ---- exits ----

		/// <summary>WSMapJumpDirection: the exit taken stops its slot's music over 15 frames (setMapjumpBGMOperation).</summary>
		public static void ExitTaken(int slot)
		{
			if (slot < 0) return;
			Guard("exit", () => Bgm.stop(15, Slot(slot)));
		}

		// ---- battles ----

		/// <summary>setBattleBGM(fanfare, stop at the end): OutsideToBattle flags 0 and 1, for the next battle.</summary>
		public static bool Fanfare = true, StopAtEnd = true;

		public static void SetBattleBgm(GlobalScope.ScriptEngine engine)
		{
			Fanfare = engine.getByte() != 0;
			StopAtEnd = engine.getByte() != 0;
		}

		/// <summary>WSEncountSetting / WSEventEncountSetting / WSTresureEncountSetting: a battle is set up (flag 0/2, which
		/// the map's script reads to take its theme up where it was).</summary>
		public static void BattleSetUp()
		{
			GlobalScope.FlagManager.singleton().set(0, 2);
		}

		/// <summary>WSEncountDirection1/2's start: the field's music down to nothing over 15 frames.</summary>
		public static void EncounterFade()
		{
			BattleSetUp();
			Guard("encounter", () => Bgm.setVolume(0, 15, Slot(FieldSlot)));
		}

		private static int _battleBgm = -1;

		/// <summary>The battle begins: the field's music paused (WSEncountDirection's end) and the party's theme played
		/// (startBattleBGM - none for -1, the story's own music going on).</summary>
		public static void BattleStarts(int bgm)
		{
			BattleSetUp();
			_inBattle = true;
			_battleBgm = bgm;
			if (bgm < 0) return;
			Guard("battle", () =>
			{
				Bgm.pause(true, Slot(FieldSlot));
				Bgm.stop(0, Slot(BattleSlot));
				Bgm.play(bgm, 255, 0, Slot(BattleSlot));
			});
			Log.Write(LogChannel.File, "music: battle theme " + bgm);
		}

		private static int _fanfareIn = -1;

		/// <summary>BattleWin::initialize: the theme down over 15 frames for the fanfare, which changeBGMPhase plays once it
		/// has stopped.</summary>
		public static void BattleWon()
		{
			if (!Fanfare) return;
			Guard("win", () => Bgm.stop(15, Slot(BattleSlot)));
			_fanfareIn = 16;
		}

		/// <summary>Each battle frame: the fanfare when its moment comes.</summary>
		public static void Tick()
		{
			if (_fanfareIn < 0 || --_fanfareIn > 0) return;
			_fanfareIn = -1;
			Guard("fanfare", () => { Bgm.stop(0, Slot(BattleSlot)); Bgm.play(FanfareBgm, 255, 0, Slot(BattleSlot)); });
		}

		/// <summary>getPhaseEnd / BSCEndBattle / BattlePlayerEscape: the battle's music down over 15 frames (setBattleBGM
		/// may leave it); then OutsideToBattle as it was.</summary>
		public static void BattleEnds(bool escaped)
		{
			if (!_inBattle) return;
			_inBattle = false;
			_fanfareIn = -1;
			if (StopAtEnd || escaped) Guard("end", () => Bgm.stop(15, Slot(BattleSlot)));
			Fanfare = StopAtEnd = true;
			_battleBgm = -1;
		}

		private static bool _inBattle;

		// ---- scenes ----

		/// <summary>EventConteParameter +0x35 (into the scene) and +0x34 (out of it): the music kept.</summary>
		private static bool _keepIn, _keepOut;

		/// <summary>bgmContinueForConteEvent(out, ?, ?): the music kept into the next scene, and out of it as asked.</summary>
		public static void ContinueForScene(GlobalScope.ScriptEngine engine)
		{
			_keepOut = engine.getByte() != 0;
			engine.getByte();
			engine.getByte();
			_keepIn = true;
		}

		/// <summary>ce_setSound(out, in, ?, ?): the scene's own say on keeping the music past its ends.</summary>
		public static void SetSound(GlobalScope.ScriptEngine engine)
		{
			_keepOut = engine.getByte() != 0;
			_keepIn = engine.getByte() != 0;
			engine.getDword();
			engine.getDword();
		}

		/// <summary>ContEventPart::initialize: the music unloaded unless kept.</summary>
		public static void SceneStarts()
		{
			if (!_keepIn) StopAll(0, "scene start");
			_keepIn = false;
		}

		/// <summary>ContEventPart::unInitialize: the music unloaded unless kept.</summary>
		public static void SceneEnds()
		{
			if (!_keepOut) StopAll(0, "scene end");
			_keepOut = false;
		}

		/// <summary>ce_StopBGM_Streaming(frames): slots 0, 1 and 3 (the handles at +0x1bc, +0x1cc, +0x1ec) over the frames.</summary>
		public static void StopStreaming(GlobalScope.ScriptEngine engine)
		{
			int frames = (int)engine.getDword();
			Guard("streaming", () => { Bgm.stop(frames, Slot(0)); Bgm.stop(frames, Slot(1)); Bgm.stop(frames, Slot(3)); });
		}

		/// <summary>soundReset(heaps, ?, which, ?): dssndUnloadBGMAll and the sound heaps made again - what plays stops.</summary>
		public static void SoundReset(GlobalScope.ScriptEngine engine)
		{
			engine.getDword(); engine.getDword(); engine.getDword(); engine.getDword();
			StopAll(0, "sound reset");
		}

		private static void StopAll(int frames, string why)
		{
			Guard("stop", () => { for (int s = 0; s < 4; s++) Bgm.stop(frames, Slot(s)); });
			Log.Write(LogChannel.File, "music: all slots stopped (" + why + ")");
		}

		/// <summary>Bank loads and unloads: their operands, as many dwords as given.</summary>
		public static GlobalScope.SCRIPT_COMMAND Operands(int dwords) => engine => { for (int i = 0; i < dwords; i++) engine.getDword(); };
	}
}
