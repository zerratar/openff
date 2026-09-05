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
		};

		/// <summary>
		/// setInsideMapJump: this map has an exit - a trigger box here, a destination and an
		/// arrival there. Declared when executed, so a branch declares it only when taken.
		/// </summary>
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
			GlobalScope.CCastCommandTransit.getInstance().cast_FieldCamera()?.Mode_set(GlobalScope.cmr.CWorldCamera.MODE.MODE_AUTOFOLLOW_DEFAULT);
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
