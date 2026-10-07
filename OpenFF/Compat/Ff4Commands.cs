// FF4's own script commands, implemented on the FF3 engine.
//
// Everything FF4 shares with FF3 runs FF3's handler (ScriptCommands). What is here is
// what FF4 added and this client has an answer for, written against the same field,
// message and character systems the FF3 handlers use. Each entry reads exactly the
// operands Shared/Script/ScriptOpsFf4 lists for it.

using System;
using System.Collections.Generic;

namespace OpenFF.Client
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
			{ "setEffect_Scale", SetEffectScale },                     // (index, x, y, z) - FF3's has a second word
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
			{ "displayCharacter", DisplayCharacter },                         // (cast, shown, ?): FF3's, and the hero's now the map's to show
			{ "startMessage", StartMessage },                                 // (who, text, style, delete frames): the line up, no wait
			{ "setMessagePosition", SetMessagePosition },                     // (setX, x, setY, y): where the next line's text starts
			{ "setMessageAlignment", SetMessageAlignment },                   // (align, ?, ?): 0 left, 1 centred, 2 right, until set again
			{ "messagePermission", MessagePermission },                       // (allow): whether a press may turn the message on
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
			// Scene characters moved and turned by frames, as libff4's object strategies (Ff4CharacterMoves)
			{ "moveCharacter_AbsoluteCoordination2", Ff4CharacterMoves.MoveAbsolute2 },    // (cast, x, y, z, frames)
			{ "moveCharacter_RelativeCoordination", Ff4CharacterMoves.MoveRelative },      // (cast, to, dx, dy, dz, frames)
			{ "moveCharacter_EndAutoIdle", Ff4CharacterMoves.MoveEndAutoIdle },            // (cast): waits for its moves
			{ "turnCharacter_AbsoluteAngle2", Ff4CharacterMoves.TurnAbsoluteAngle2 },      // (cast, degrees, frames, ?, keep motion)
			{ "turnCharacter_RelativeAngle2", Ff4CharacterMoves.TurnRelativeAngle2 },      // (cast, degrees, frames, ?, keep motion)
			{ "turnCharacter_AbsoluteCoordination2", Ff4CharacterMoves.TurnAbsoluteCoordination2 }, // (cast, x, y, z, frames, ?, keep motion)
			{ "turnCharacter_LookCharacter2", Ff4CharacterMoves.TurnLookCharacter2 },      // (cast, at, frames, ?, keep motion)
			{ "turnCharacter_EndAutoIdle", Ff4CharacterMoves.TurnEndAutoIdle },            // (cast): waits for its turns
			// The roster and the bag: the unified party (Ff4Party on OpenFF.Data).
			{ "addItem", Ff4Party.AddItem },                                  // (item, count)
			{ "subItem", Ff4Party.SubItem },                                  // (item, count)
			{ "addPartyPC", Ff4Party.AddPartyPC },                            // (type, ?)
			{ "subPartyPC", Ff4Party.SubPartyPC },                            // (type, ?)
			{ "setPartyPCEquipItem", Ff4Party.SetPartyPCEquipItem },          // (type, right, left, head, body, arm)
			{ "addAbility", Ff4Party.AddAbility },                            // (type, ability)
			{ "removeAbility", Ff4Augments.RemoveAbility },                   // (type, ability)
			{ "copyDecantAbility", Ff4Augments.CopyDecantAbility },           // (from, to, except x4): Cecil's and Rydia's other forms
			{ "decantLevelChekcJump", Ff4Augments.DecantLevelCheckJump },     // (type, level, label): the augments given
			{ "bootShop", BootShop },                                         // (row, ?): babil_shop.bbd's row; the script holds while the shop is open
			{ "bootInn", BootInn },                                           // (price, ?, ?): asks to stay the night and takes the gil
			{ "selectEndWait", SelectEndWait },                               // (label, ?, ?, ?, ?): jumps when the inn was declined
			{ "setRecovery2", SetRecovery2 },                                 // (member order or 0 = all, ?, ?, amount): hit and magic points back
			{ "setConditionRecovery", SetConditionRecovery },                 // (six condition words): statuses cured
			{ "bootEventBattle", BootEventBattle },                           // (party, map, ?, ?, ?): the OpenFF battle on that encounter group
			{ "setBGMDownParam", SetBgmDownParam },                           // (volume, frames, ?): how far and how fast the music ducks
			{ "startBGMDown", StartBgmDown },                                 // (?): the music down under a voice
			{ "reverseBGMDown", ReverseBgmDown },                             // (?): and back up
		};

		// ---- BGM ducking, as babilCommand_SetBGMDownParam / StartBGMDown / ReverseBGMDown: both BGM slots moved to the
		// volume over the frames, then back to 127 - the music drops under the lines a scene speaks ----

		private static int _bgmDownVolume = 127, _bgmDownFrames;

		private static void SetBgmDownParam(GlobalScope.ScriptEngine engine)
		{
			_bgmDownVolume = engine.getWord();
			_bgmDownFrames = engine.getWord();
			engine.getWord();
		}

		private static void StartBgmDown(GlobalScope.ScriptEngine engine)
		{
			engine.getWord();
			MoveBgmVolume(_bgmDownVolume, _bgmDownFrames);
		}

		private static void ReverseBgmDown(GlobalScope.ScriptEngine engine)
		{
			engine.getWord();
			MoveBgmVolume(127, _bgmDownFrames);
		}

		private static void MoveBgmVolume(int volume, int frames)
		{
			try
			{
				GlobalScope.MatrixSound.MtxSoundBGM.getSingleton().setVolume(volume, frames, GlobalScope.MatrixSound.enMtxBGMSlot.enMTX_BGM_SLOT0);
				GlobalScope.MatrixSound.MtxSoundBGM.getSingleton().setVolume(volume, frames, GlobalScope.MatrixSound.enMtxBGMSlot.enMTX_BGM_SLOT1);
			}
			catch (Exception ex) { Log.First(LogChannel.General, "bgm-down", 1, () => "script: BGM ducking: " + ex.Message); }
		}

		/// <summary>setEffect_Scale(index, x, y, z): the effect the script's index names (Ff4EffectIndex) scaled, z negated as FF4's has it.</summary>
		private static void SetEffectScale(GlobalScope.ScriptEngine engine)
		{
			int index = engine.getWord();
			int x = (int)engine.getDword(), y = (int)engine.getDword(), z = (int)engine.getDword();
			int effect = Ff4EffectIndex.Object(index);
			if (effect >= 0 && GlobalScope.eff.CEffectMng.instance().isEffectObject(effect))
			{
				GlobalScope.eff.CEffectMng.instance().setScale(effect, new GlobalScope.VecFx32(x, y, -z));
			}
		}

		/// <summary>Commands that only dress the game - door swings, footstep dust, BGM ducking, the jump history - skipped without a word in the log.</summary>
		public static readonly HashSet<string> Cosmetic = new HashSet<string>(StringComparer.Ordinal)
		{
			"addDesionList", "clearDesionList",                          // the map-jump history
			"setMapjumpBGMOperation",
			"setRelationMapjumpToDoorAttr", "setDoor", "setRelationOfMapjumpobjAndFlag",   // door swings on exits
			"createEffectTaskWalk", "createEffectTaskRun", "createEffectTaskWait",        // footstep dust
			"setShadowScale",
			// Sound bookkeeping FF3's player has no slot for: the battle theme choice, the
			// division of BGM data, a reset.
			"setBattleBGM", "bgmContinueForConteEvent", "soundReset", "bgmDivideLoadDataTypeSpecific",
			// checkCharacterStatusJump jumps when a member carries a status condition (none does here).
			"checkCharacterStatusJump",
			// Objects bound to a character's joint (a carried item, a torch), the sub-plane visibility
			// of the DS's second screen.
			"createBindObject", "setVisibleBindObject", "ce_CallProgParam",
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
			int frames = (int)engine.getDword();
			engine.getDword();
			engine.getDword();
			Ff4EventCamera.Follow(pos, trg, frames);
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

		// ---- the inn: babilCommand_BootInn opens the message, gil and confirm windows and holds; selectEndWait reads the answer ----

		private static bool _innAccepted;

		/// <summary>bootInn(price, ?, ?): "Stay the night?" - taken when the party can pay.</summary>
		private static void BootInn(GlobalScope.ScriptEngine engine)
		{
			int price = (int)engine.getWord();
			engine.getDword();
			engine.getDword();
			if (!Ff4FieldCommands.Ask(engine, price > 0 ? "Stay (" + price + " gil)" : "Stay", "No", out bool yes)) return;
			OpenFF.Data.Party party = Ff4Party.Party;
			if (yes && party.Gil < price)
			{
				OpenFF.Game.Dialogue.Say("I'm afraid you're short on gil.");
				yes = false;
			}
			if (yes) party.Gil -= price;
			_innAccepted = yes;
			Log.Write(LogChannel.General, "script: inn " + (yes ? "taken for " + price + " gil (" + party.Gil + " left)" : "declined"));
		}

		/// <summary>selectEndWait(label, ?, ?, ?, ?): after bootInn, the script jumps to the label when the party did not stay.</summary>
		private static void SelectEndWait(GlobalScope.ScriptEngine engine)
		{
			uint label = engine.getDword();
			engine.getDword(); engine.getDword(); engine.getDword(); engine.getDword();
			if (!_innAccepted && label != 0) engine.jump(label);
		}

		/// <summary>setRecovery2(order, ?, ?, amount): hit and magic points back for one member (order 1..) or all (0); 9999 is everything.</summary>
		private static void SetRecovery2(GlobalScope.ScriptEngine engine)
		{
			int order = (int)engine.getDword();
			engine.getDword();
			engine.getDword();
			int amount = (int)engine.getWord();
			int healed = 0;
			foreach (OpenFF.Data.Character m in Ff4Party.Party.Members)
			{
				if (order > 0 && m.Slot != order - 1) continue;
				if (amount >= 9999) { m.Hp = m.MaxHp; m.Mp = m.MaxMp; }
				else { m.Hp = Math.Min(m.MaxHp, m.Hp + amount); m.Mp = Math.Min(m.MaxMp, m.Mp + amount); }
				healed++;
			}
			Log.Write(LogChannel.File, "script: setRecovery2 - " + healed + " member(s) " + (amount >= 9999 ? "fully restored" : "+" + amount));
		}

		/// <summary>setConditionRecovery(...): the listed statuses are cured; the engine keeps its conditions in the party service, all of them go.</summary>
		private static void SetConditionRecovery(GlobalScope.ScriptEngine engine)
		{
			for (int i = 0; i < 6; i++) engine.getDword();
			(OpenFF.Game.Party as Ff4PartyService)?.CureAll();
		}

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

		private static int _eventBattle;   // bootEventBattle: 0 none, 1 the whirl playing, 2 the battle begun

		private static void BootEventBattle(GlobalScope.ScriptEngine engine)
		{
			int party = (int)engine.getWord();
			engine.getByte();
			int type = engine.getByte();   // the opening: 0 Normal, 1 Back attack, 2 Surprise (table at 0x1bbf50; 3 and up Normal)
			engine.getByte(); engine.getByte();
			// "world encount2" (WSEncountDirection2): the whirl to white, then the battle - the script held on this command
			// until the battle has begun (and, the field's logic standing while it is fought, until it is over).
			if (_eventBattle == 1 || (_eventBattle == 0 && BlurRotate.Playing)) { engine.suspendRedo(); return; }
			if (_eventBattle == 2) { _eventBattle = 0; return; }
			_eventBattle = 1;
			engine.suspendRedo();
			int opening = type == 1 ? Ff4Battle.OpenBack : type == 2 ? Ff4Battle.OpenSurprise : Ff4Battle.OpenNormal;
			BlurRotate.StartEncounter(() =>
			{
				_eventBattle = 2;
				Ff4Battle.Instance?.SetNextOpening(opening);
				if (Ff4Battle.Instance == null || !Ff4Battle.Instance.StartParty(party))
				{
					Log.Write(LogChannel.General, "script: bootEventBattle " + party + " could not start");
					Ff4Battle.Instance?.SetNextOpening(Ff4Battle.OpenNormal);
				}
			});
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

		/// <summary>
		/// moveCamera_LookPlayer2(cast, frames, ?, ?, ?), as libff4's: the event camera follows the cast at the world camera's
		/// offsets (EventCamera::setFollow), reached over the frames - and a setCameraOffset after it keeps following that cast
		/// (the castle corridor's camera behind the scripted Cecil, not the hidden hero). When the world camera keeps no offsets
		/// of FF4's (FF3's controller places itself by distance and angle), the field's camera takes over as FF3's handler has it.
		/// </summary>
		private static void MoveCameraLookPlayer2(GlobalScope.ScriptEngine engine)
		{
			byte[] code = engine.Code;
			uint pc = engine.getPC();
			bool readable = code != null && pc + 3 < code.Length;
			int cast = readable ? code[pc] | code[pc + 1] << 8 : -1;
			int frames = readable ? code[pc + 2] | code[pc + 3] << 8 : 0;
			GlobalScope.pl.CBasePlayer player = null;
			try
			{
				int index = cast < 0 ? -1 : GlobalScope.CCastCommandTransit.getInstance().changeHichNumber((uint)cast);
				if (index != -1) player = GlobalScope.CCastCommandTransit.getInstance().cast_PlayerMng().Player(index);
			}
			catch (Exception) { }
			Ff4EventCamera.FollowWhom(player);
			GlobalScope.cmr.CWorldCamera camera = GlobalScope.CCastCommandTransit.getInstance().cast_FieldCamera();
			GlobalScope.VecFx32 offset = camera?.m_PosOffset, trgOffset = camera?.m_TrgOffset;
			if (player != null && offset != null && trgOffset != null && (offset.x != 0 || offset.y != 0 || offset.z != 0))
			{
				engine.skip(12);   // the cast, the frames, a dword and two words
				GlobalScope.VecFx32 fromOffset = new GlobalScope.VecFx32(trgOffset.x - offset.x, trgOffset.y - offset.y, trgOffset.z - offset.z);
				Ff4EventCamera.Follow(player, new GlobalScope.VecFx32(offset), fromOffset, frames);
				return;
			}
			Ff4EventCamera.Release();
			GlobalScope.ff3Command_MoveCamera_LookPlayer2(engine);
			engine.skip(4);   // FF4's two trailing words
		}

		private static GlobalScope.wld.CMessageWindow Window =>
			GlobalScope.CCastCommandTransit.getInstance().cast_Field2D()?.MessageWindow();

		/// <summary>
		/// displayCharacter(cast, shown, ?): FF3's handler; when the cast is the hero, a story scene that hid them no longer owns
		/// that (Ff4Cutscene.HeroSetByMap) - the castle's arrival hides Cecil while its own Cecil walks in, and leaving the scene's
		/// map must not show him again over it.
		/// </summary>
		private static void DisplayCharacter(GlobalScope.ScriptEngine engine)
		{
			byte[] code = engine.Code;
			uint pc = engine.getPC();
			uint cast = code != null && pc + 1 < code.Length ? (uint)(code[pc] | code[pc + 1] << 8) : uint.MaxValue;
			GlobalScope.ff3Command_DisplayCharacter(engine);
			if (cast == uint.MaxValue) return;
			try
			{
				int index = GlobalScope.CCastCommandTransit.getInstance().changeHichNumber(cast);
				if (index != -1 && GlobalScope.CCastCommandTransit.getInstance().cast_PlayerMng().Player(index) == EngineApi.HeroPlayer) Ff4Cutscene.HeroSetByMap();
			}
			catch (Exception) { }
		}

		/// <summary>
		/// startMessage(who, text, style, delete frames), as libff4's: on the field the line goes up in the message
		/// window (MessageWindow::mwSetMessage) and the script runs on - its own messageWait holds it until the line is
		/// done - so a character can gesture as it speaks. FF3's waited for the line to be dismissed. A story scene's
		/// (the event conte's) keeps FF3's handler and its bar.
		/// </summary>
		private static void StartMessage(GlobalScope.ScriptEngine engine)
		{
			if (Ff4Cutscene.Active)
			{
				GlobalScope.ff3Command_StartMessage2(engine);
				return;
			}
			int who = engine.getWord();
			int text = (int)engine.getDword();
			engine.getByte();
			int deleteFrames = engine.getByte();
			GlobalScope.wld.CMessageWindow window = Window;
			if (window == null) return;
			GlobalScope.CCastCommandTransit.getInstance().cast_BaseSystem().lastMessage_set(text);
			EngineHooks.MessageShown(text);
			window.createMessage(text, 0, who);
			if (deleteFrames != 0) window.setMesDeleteFrame(deleteFrames);
			window.setProgressIconActivity(true);
		}

		/// <summary>
		/// setMessagePosition(setX, x, setY, y), as libff4's: the text's place in the 480 x 320 screen as given (MessageWindow
		/// +0x208; a centred line gives its middle, 240) - FF3's adds its own window's offsets (112, 64), which put FF4's
		/// centred lines (a yellow "handed over" line at 240, 282) below the screen and left an empty window. The window
		/// puts it back to 12, 252 when the line goes (CMessageWindow.releaseMessage).
		/// </summary>
		private static void SetMessagePosition(GlobalScope.ScriptEngine engine)
		{
			bool setX = engine.getDword() != 0;
			int x = engine.getWord();
			bool setY = engine.getDword() != 0;
			int y = engine.getWord();
			GlobalScope.wld.CMessageWindow window = Window;
			if (window == null) return;
			GlobalScope.ds.Vector2<short> at = window.getMessagePosition();
			if (setX) at.vx = (short)x;
			if (setY) at.vy = (short)y;
			window.setMessagePosition(at);
		}

		/// <summary>
		/// babilCommands_SetMessageAlignment(align, ?, ?): how the next lines stand on the text's place - libff4's three
		/// tables at 0x2bbf2c (0 left, 1 centred, 2 right): the text's origin across (0x8 / 0x10 / 0x20) and down (0x1 /
		/// 0x2 / 0x4) and its lines' alignment (0x40 / 0x80 / 0x100), with 0x200 - NitroSystem's text flags. So a centred
		/// line's place (240, 282) is its middle. The script sets 0 again after.
		/// </summary>
		private static void SetMessageAlignment(GlobalScope.ScriptEngine engine)
		{
			uint align = engine.getDword();
			engine.getDword();
			engine.getDword();
			if (align > 2) align = 0;
			uint[] across = { 0x8, 0x10, 0x20 }, down = { 0x1, 0x2, 0x4 }, lines = { 0x40, 0x80, 0x100 };
			Window?.setMessageAlignment((int)(across[align] | down[align] | lines[align] | 0x200));
		}

		/// <summary>
		/// messagePermission(allow), as libff4's: outside a story scene the message window's press is allowed or not
		/// (MessageWindow +0x200) - a scene holds its line up while a character gestures, and a press then does nothing
		/// (FF3's handler allowed it whatever the operand, so a press cleared the text and left the window empty). In a
		/// story scene it only marks the scene's message as allowed (EventConteManager +0x635), as FF3's did.
		/// </summary>
		private static void MessagePermission(GlobalScope.ScriptEngine engine)
		{
			bool allow = engine.getWord() != 0;
			Window?.setSendMessage(Ff4Cutscene.Active || allow);
		}

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
