// FF4's own script commands, implemented on the FF3 engine.
//
// Everything FF4 shares with FF3 runs FF3's handler (ScriptCommands). What is here is
// what FF4 added and this client has an answer for, written against the same field,
// message and character systems the FF3 handlers use. Each entry reads exactly the
// operands Shared/Script/ScriptOpsFf4 lists for it.

using System.Collections.Generic;

namespace FF3
{
	internal static class Ff4Commands
	{
		/// <summary>The speaker named by the last openCharacterNameWindow, or -1.</summary>
		public static int PendingName = -1;

		public static readonly Dictionary<int, GlobalScope.SCRIPT_COMMAND> Table = new Dictionary<int, GlobalScope.SCRIPT_COMMAND>
		{
			{ 358, OpenCharacterNameWindow },   // (nameTextId, x, y)
			{ 359, CloseCharacterNameWindow },  // (x, y)
			{ 87, ChangeCameraMode },           // () - FF3's takes the mode; FF4's means "back to following"
			{ 333, SetInsideMapJump },          // (trigger, map, ax, ay, az, facing, x1, y1, z1, x2, y2, z2)
			{ 334, SetOutsideMapJump },         // the same, for leaving by the map's edge
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
