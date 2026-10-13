// FF4's shiftState(name, ...): world::WorldStateScheduler::wssAddStateSchedule - the field hands over to another world
// state once the event lets go: a part of the game of its own (the title, the bestiary, the music player, the theater,
// the name entry), the Fat Chocobo's storage, the ending's last part. Each name is one of the states part.c registers.

using System;
using System.Collections.Generic;

namespace OpenFF.Client
{
	internal static class Ff4ShiftState
	{
		private static readonly Dictionary<string, Action> States = new Dictionary<string, Action>(StringComparer.Ordinal)
		{
			// WSCallTitle: part 3, the title, after the fade (WSCallPart) - as the menu's Quit and a lost battle go there.
			{ "goto_title", GotoTitle },
		};

		public static void ShiftState(GlobalScope.ScriptEngine engine)
		{
			string name = engine.getString();
			for (int i = 0; i < 5; i++) engine.getDword();
			if (name != null && Ff4Extras.Request(name))
			{
				Log.Write(LogChannel.File, "script: FF4 shiftState " + name);
				return;
			}
			if (name != null && States.TryGetValue(name, out Action state))
			{
				Log.Write(LogChannel.File, "script: FF4 shiftState " + name);
				Ff4Cutscene.Guard("shiftState " + name, state);
				return;
			}
			Log.First(LogChannel.General, "ff4-shift-" + name, 1, () => "script: FF4 shiftState " + name + " - that state is not in yet");
		}

		private static void GotoTitle()
		{
			GlobalScope.wld.CBaseSystem.setTitle(true);
		}
	}
}
