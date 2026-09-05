// FF4's own script commands, implemented on the FF3 engine.
//
// Everything FF4 shares with FF3 runs FF3's handler (ScriptCommands). What is here is
// what FF4 added and this client has an answer for, written against the same field,
// message and character systems the FF3 handlers use. Each entry reads exactly the
// operands Shared/Script/ScriptOpsFf4 lists for it.

using System;
using System.Collections.Generic;

namespace FF3
{
	internal static class Ff4Commands
	{
		/// <summary>The speaker named by the last openCharacterNameWindow, or -1.</summary>
		public static int PendingName = -1;

		/// <summary>
		/// FF4's commands by name, as Shared/Script/ScriptOpsFf4 spells them. By name and not by
		/// number: the table is generated from the FF4 binary and a number that moves between
		/// generations silently reads another command's operands - which is how the name window
		/// once swallowed cleanUpEffectData2's string and derailed the opening scene.
		/// </summary>
		public static readonly Dictionary<string, GlobalScope.SCRIPT_COMMAND> ByName = new Dictionary<string, GlobalScope.SCRIPT_COMMAND>(StringComparer.Ordinal)
		{
			{ "openCharacterNameWindow", OpenCharacterNameWindow },   // (nameTextId, x, y)
			{ "closeCharacterNameWindow", CloseCharacterNameWindow }, // (x, y)
			{ "changeCamera_Mode", ChangeCameraMode },                 // () - FF3's takes the mode; FF4's means "back to following"
			{ "setInsideMapJump", SetInsideMapJump },                 // (trigger, map, ax, ay, az, facing, x1, y1, z1, x2, y2, z2)
			{ "setOutsideMapJump", SetOutsideMapJump },               // the same, for leaving by the map's edge
			{ "confirm", Ff4FieldCommands.Confirm },                          // (a, b): the Yes/No box
			{ "confirmWait", Ff4FieldCommands.ConfirmWait },                  // (yesText, noText, jumpIfYes, jumpIfNo)
			{ "waitByLocale", Ff4FieldCommands.WaitByLocale },                // (japanese frames, other frames)
			{ "jumpByLocale", Ff4FieldCommands.JumpByLocale },                // (locale, ?, label)
			{ "setRewardMessage", Ff4FieldCommands.SetRewardMessage },        // (textId, 0, icon, 0, 0, 0)
			{ "setRewardMessageInterval", Ff4FieldCommands.SetRewardMessageInterval }, // (frames)
			{ "executeRewardMessageWindow", Ff4FieldCommands.ExecuteRewardMessageWindow }, // ()
			{ "setPlayerLevel", Ff4FieldCommands.SetPlayerLevel },            // (playerType, level)
			{ "clearCountJump", ClearCountJump },                             // (count, label): jumps when the game has been cleared `count` times
			{ "setChacterOffset", SetCharacterOffset },                       // (cast, x, y, z): a draw offset for a cast's model
			// The field event camera (Ff4EventCamera): FF3's handlers put these through MODE_FREE, which
			// rebuilds the position from a distance FF4's maps never set.
			{ "moveCamera_AbsoluteCoordination", MoveCameraAbsolute },       // (x, y, z, frames, alsoTarget, ?)
			{ "moveCamera_RelativeCoordination", MoveCameraRelative },       // (dx, dy, dz, frames, alsoTarget, ?)
			{ "setCamera_AbsoluteGaze", SetCameraGaze },                      // (x, y, z, frames, ?)
			{ "setCamera_RelativeGaze", SetCameraRelativeGaze },              // (dx, dy, dz, frames, ?)
			{ "setCameraOffset", SetCameraOffset },                           // (pos offset xyz, target offset xyz, ?, ?, ?): follow the leader
			{ "setCamera_PositionOffset", SetCameraPositionOffset },          // (hich, dx, dy, dz, frames, also target, ?): the event camera moves by an offset
			{ "setCamera_TargetOffset", SetCameraTargetOffset },              // (hich, dx, dy, dz, frames, ?): its target moves by an offset
			{ "cancelCameraControl", CancelCameraControl },                   // (?, ?): the field camera again
			{ "setCamera_BeforeEvent", SetCameraBeforeEvent },                // (x, y, z): the field camera again, FF3's setupCamera
			{ "moveCamera_LookPlayer2", MoveCameraLookPlayer2 },              // (cast, ?, ?, ?, ?): FF3's, after letting the event camera go
			{ "setWorldCameraPosAndTargetOffset", SetWorldCameraOffsets },   // (offset xyz, target-from-offset xyz, ?, ?): the follow camera's offsets
			// The roster and the bag: the unified party (Ff4Party on OpenFF.Data).
			{ "addItem", Ff4Party.AddItem },                                  // (item, count)
			{ "subItem", Ff4Party.SubItem },                                  // (item, count)
			{ "addPartyPC", Ff4Party.AddPartyPC },                            // (type, ?)
			{ "subPartyPC", Ff4Party.SubPartyPC },                            // (type, ?)
			{ "setPartyPCEquipItem", Ff4Party.SetPartyPCEquipItem },          // (type, right, left, head, body, arm)
			{ "addAbility", Ff4Party.AddAbility },                            // (type, ability)
			{ "bootShop", BootShop },                                         // (row, ?): babil_shop.bbd's row; the script holds while the shop is open
			{ "bootEventBattle", BootEventBattle },                           // (party, map, ?, ?, ?): the OpenFF battle on that encounter group
		};

		/// <summary>Commands that only dress the game - door swings, footstep dust, BGM ducking, the jump history - skipped without a word in the log.</summary>
		public static readonly HashSet<string> Cosmetic = new HashSet<string>(StringComparer.Ordinal)
		{
			"addDesionList", "clearDesionList",                          // the map-jump history
			"setMapjumpBGMOperation",
			"setRelationMapjumpToDoorAttr", "setDoor", "setRelationOfMapjumpobjAndFlag",   // door swings on exits
			"createEffectTaskWalk", "createEffectTaskRun", "createEffectTaskWait",        // footstep dust
			"setBGMDownParam", "startBGMDown", "reverseBGMDown",         // BGM ducking
			"setShadowScale",
			"setMessageAlignment",                                     // message alignment
			// Sound bookkeeping FF3's player has no slot for: the battle theme choice, the
			// division of BGM data, a reset.
			"setBattleBGM", "bgmContinueForConteEvent", "soundReset", "bgmDivideLoadDataTypeSpecific",
			// Jumps whose condition a new game never meets: decantLevelChekcJump(type, level, label)
			// jumps when the character's augment level equals `level` (2 in every script; it is 0
			// here), checkCharacterStatusJump when a member carries a status condition.
			"decantLevelChekcJump", "checkCharacterStatusJump",
			// Objects bound to a character's joint (a carried item, a torch), effect scaling, the
			// sub-plane visibility of the DS's second screen.
			"createBindObject", "setVisibleBindObject", "setEffect_Scale", "ce_CallProgParam",
		};

		/// <summary>
		/// setInsideMapJump: this map has an exit - a trigger box here, a destination and an
		/// arrival there. Declared when executed, so a branch declares it only when taken.
		/// </summary>
		/// <summary>clearCountJump(count, label): FF4 counts finished playthroughs; this is a first one.</summary>
		private static void ClearCountJump(GlobalScope.ScriptEngine engine)
		{
			int count = engine.getByte();
			uint label = engine.getDword();
			if (count == 0)
			{
				engine.jump(label);
			}
		}

		/// <summary>setChacterOffset(cast, x, y, z): FF4's setOffsetMtxPosition - the model drawn off its cast's position.</summary>
		private static void SetCharacterOffset(GlobalScope.ScriptEngine engine)
		{
			uint cast = engine.getWord();
			int x = (int)engine.getDword(), y = (int)engine.getDword(), z = (int)engine.getDword();
			int num = GlobalScope.CCastCommandTransit.getInstance().changeHichNumber(cast);
			if (num == -1) return;
			int characterId = GlobalScope.CCastCommandTransit.getInstance().cast_PlayerMng().Player(num).getCharacterId();
			if (characterId == -1) return;
			GlobalScope.MtxFx43 mtx = new GlobalScope.MtxFx43();
			GlobalScope.MTX_Identity43(mtx);
			mtx.a[9] = x;
			mtx.a[10] = y;
			mtx.a[11] = z;
			GlobalScope.characterMng.setPoseMtx(characterId, mtx);
		}

		private static void SetInsideMapJump(GlobalScope.ScriptEngine engine)
		{
			DeclareExit(engine);
		}

		/// <summary>setOutsideMapJump: an exit by the map's edge onto the world map. The same box logic.</summary>
		private static void SetOutsideMapJump(GlobalScope.ScriptEngine engine)
		{
			DeclareExit(engine);
		}

		private static void DeclareExit(GlobalScope.ScriptEngine engine)
		{
			string trigger = engine.getString();
			string destination = engine.getString();
			int[] v = new int[10];
			for (int i = 0; i < v.Length; i++)
			{
				v[i] = unchecked((int)engine.getDword());
			}
			GlobalScope.VecFx32 leader = null;
			try
			{
				leader = GlobalScope.wld.WorldPart.getInstance()?.getWorldSystem()?.PlayerMng()
					?.Player(GlobalScope.chr.CBaseCharacter.getLookIndex())?.getPosition();
			}
			catch (System.Exception)
			{
				// No world yet: the exit is armed as declared.
			}
			Ff4Exits.Register(Ff4Exits.FromOperands(trigger, destination, v[0], v[1], v[2], v[3], v[4], v[5], v[6], v[7], v[8], v[9]), leader);
		}

		/// <summary>changeCamera_Mode(): the field camera follows the party again.</summary>
		private static void ChangeCameraMode(GlobalScope.ScriptEngine engine)
		{
			Ff4EventCamera.Release();
			GlobalScope.CCastCommandTransit.getInstance().cast_FieldCamera()?.Mode_set(GlobalScope.cmr.CWorldCamera.MODE.MODE_AUTOFOLLOW_DEFAULT);
		}

		private static void MoveCameraAbsolute(GlobalScope.ScriptEngine engine)
		{
			int x = (int)engine.getDword(), y = (int)engine.getDword(), z = (int)engine.getDword();
			int frames = (int)engine.getWord();
			int alsoTarget = (int)engine.getWord();
			engine.getDword();
			Ff4EventCamera.MoveTo(x, y, z, frames, alsoTarget == 1);
		}

		private static void MoveCameraRelative(GlobalScope.ScriptEngine engine)
		{
			int x = (int)engine.getDword(), y = (int)engine.getDword(), z = (int)engine.getDword();
			int frames = (int)engine.getWord();
			int alsoTarget = (int)engine.getWord();
			engine.getDword();
			Ff4EventCamera.MoveBy(x, y, z, frames, alsoTarget == 1);
		}

		private static void SetCameraGaze(GlobalScope.ScriptEngine engine)
		{
			int x = (int)engine.getDword(), y = (int)engine.getDword(), z = (int)engine.getDword();
			int frames = (int)engine.getWord();
			engine.getDword();
			Ff4EventCamera.LookAt(x, y, z, frames);
		}

		private static void SetCameraRelativeGaze(GlobalScope.ScriptEngine engine)
		{
			int x = (int)engine.getDword(), y = (int)engine.getDword(), z = (int)engine.getDword();
			int frames = (int)engine.getWord();
			engine.getDword();
			Ff4EventCamera.LookBy(x, y, z, frames);
		}

		/// <summary>babilCommand_SetCamera_PositionOffset: the event camera slides from where it is by (dx, dy, dz) over frames; with the flag its target follows by the same amount.</summary>
		private static void SetCameraPositionOffset(GlobalScope.ScriptEngine engine)
		{
			engine.getWord();
			int dx = (int)engine.getDword(), dy = (int)engine.getDword(), dz = (int)engine.getDword();
			int frames = (int)engine.getWord();
			int alsoTarget = (int)engine.getWord();
			engine.getDword();
			Ff4EventCamera.MoveBy(dx, dy, dz, Math.Max(1, frames), alsoTarget == 1);
		}

		/// <summary>babilCommand_SetCamera_TargetOffset: the event camera's target slides by (dx, dy, dz) over frames.</summary>
		private static void SetCameraTargetOffset(GlobalScope.ScriptEngine engine)
		{
			engine.getWord();
			int dx = (int)engine.getDword(), dy = (int)engine.getDword(), dz = (int)engine.getDword();
			int frames = (int)engine.getWord();
			engine.getDword();
			Ff4EventCamera.LookBy(dx, dy, dz, Math.Max(1, frames));
		}

		private static void SetCameraOffset(GlobalScope.ScriptEngine engine)
		{
			GlobalScope.VecFx32 pos = new GlobalScope.VecFx32((int)engine.getDword(), (int)engine.getDword(), (int)engine.getDword());
			GlobalScope.VecFx32 trg = new GlobalScope.VecFx32((int)engine.getDword(), (int)engine.getDword(), (int)engine.getDword());
			engine.getDword();
			engine.getDword();
			engine.getDword();
			Ff4EventCamera.Follow(pos, trg);
		}

		/// <summary>WorldCamera::setOffset / setTrgFromOffset: the camera at leader + offset, looking at that point + target offset.</summary>
		private static void SetWorldCameraOffsets(GlobalScope.ScriptEngine engine)
		{
			int ox = (int)engine.getDword(), oy = (int)engine.getDword(), oz = (int)engine.getDword();
			int tx = (int)engine.getDword(), ty = (int)engine.getDword(), tz = (int)engine.getDword();
			engine.getDword();
			engine.getDword();
			Ff4EventCamera.Release();
			GlobalScope.cmr.CWorldCamera camera = GlobalScope.CCastCommandTransit.getInstance().cast_FieldCamera();
			if (camera == null) return;
			camera.setPosOffset(new GlobalScope.VecFx32(ox, oy, oz));
			camera.setTrgOffset(new GlobalScope.VecFx32(ox + tx, oy + ty, oz + tz));
		}

		private static readonly HashSet<GlobalScope.ScriptEngine> _shopping = new HashSet<GlobalScope.ScriptEngine>();

		/// <summary>bootShop(row, ?): opens the engine's shop for babil_shop.bbd's row and holds the script until it closes.</summary>
		private static void BootShop(GlobalScope.ScriptEngine engine)
		{
			int row = engine.getByte();
			engine.getByte();
			if (_shopping.Contains(engine))
			{
				if (Ff4Shop.Instance != null && Ff4Shop.Instance.IsOpen) { engine.suspendRedo(); return; }
				_shopping.Remove(engine);
				return;
			}
			if (Ff4Shop.Instance != null && Ff4Shop.Instance.Open(row))
			{
				_shopping.Add(engine);
				engine.suspendRedo();
				return;
			}
			Log.Write(LogChannel.General, "script: bootShop " + row + " could not open");
		}

		private static void BootEventBattle(GlobalScope.ScriptEngine engine)
		{
			int party = (int)engine.getWord();
			engine.getByte(); engine.getByte(); engine.getByte(); engine.getByte();
			if (Ff4Battle.Instance == null || !Ff4Battle.Instance.StartParty(party))
			{
				Log.Write(LogChannel.General, "script: bootEventBattle " + party + " could not start");
			}
		}

		private static void CancelCameraControl(GlobalScope.ScriptEngine engine)
		{
			engine.getDword();
			engine.getDword();
			Ff4EventCamera.Release();
		}

		private static void SetCameraBeforeEvent(GlobalScope.ScriptEngine engine)
		{
			Ff4EventCamera.Release();
			GlobalScope.ff3Command_SetCamera_BeforeEvent(engine);
		}

		private static void MoveCameraLookPlayer2(GlobalScope.ScriptEngine engine)
		{
			Ff4EventCamera.Release();
			GlobalScope.ff3Command_MoveCamera_LookPlayer2(engine);
			engine.skip(4);   // FF4's two trailing words
		}

		private static GlobalScope.wld.CMessageWindow Window =>
			GlobalScope.CCastCommandTransit.getInstance().cast_Field2D()?.MessageWindow();

		/// <summary>openCharacterNameWindow(id, x, y): the speaker's name, a text id in the map's .msd.</summary>
		private static void OpenCharacterNameWindow(GlobalScope.ScriptEngine engine)
		{
			int id = (int)engine.getDword();
			engine.getDword();
			engine.getDword();
			PendingName = id;
			Window?.setName(id);
		}

		/// <summary>closeCharacterNameWindow(x, y).</summary>
		private static void CloseCharacterNameWindow(GlobalScope.ScriptEngine engine)
		{
			engine.getDword();
			engine.getDword();
			PendingName = -1;
			Window?.clearName();
		}
	}
}
