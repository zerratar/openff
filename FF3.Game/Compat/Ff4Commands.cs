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
		};

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
