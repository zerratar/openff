// Where the legacy game tells the engine what its own scripts and systems did.
//
// A handful of one-line calls from the decompiled code - a flag set, a message shown, a
// scene started or ended, a battle about to begin, an item gained, a warp asked for -
// become typed events on Game.Events, so a mod reacts to the original story without
// editing its scripts. Every call is a no-op until the engine is attached, and never
// throws into the game.

using System;
using OpenFF;
using OpenFF.Events;

namespace OpenFF.Client
{
	internal static class EngineHooks
	{
		private static bool On => EngineHost.Attached;

		public static void FlagChanged(uint group, uint index, bool value)
		{
			if (!On) return;
			Publish(new FlagChanged { Group = group, Index = index, Value = value });
		}

		public static void MessageShown(int number)
		{
			if (!On) return;
			Publish(new MessageShown { Number = number });
		}

		public static void Cutscene(bool started)
		{
			if (!On) return;
			if (started) Publish(new CutsceneStarted()); else Publish(new CutsceneEnded());
		}

		public static void BattleStarting()
		{
			if (!On) return;
			Publish(new BattleStarting());
		}

		public static void ItemGained(int itemId, int count)
		{
			if (!On) return;
			Publish(new ItemGained { ItemId = itemId, Count = count });
		}

		public static void WarpRequested(string map, GlobalScope.VecFx32 position, int facing)
		{
			if (!On) return;
			Publish(new WarpRequested { Map = map, Position = EngineApi.ToUnits(position), Facing = facing });
		}

		private static void Publish<T>(T evt)
		{
			try
			{
				OpenFF.Game.Events.Publish(evt);
			}
			catch (Exception ex)
			{
				Log.First(LogChannel.General, "hook-" + typeof(T).Name, 3, () => "engine: hook " + typeof(T).Name + " failed: " + ex.Message);
			}
		}
	}
}
