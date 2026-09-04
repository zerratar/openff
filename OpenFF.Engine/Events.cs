// The event bus: how a script reacts to what it does not own.
//
// Game.Events.Subscribe<MapEntered>(e => ...) returns a token; dispose it to stop. A
// handler that throws is reported and skipped, never allowed to take the game down or
// to starve the handlers after it. The engine publishes the events below as the host
// tells it what the legacy game did; the legacy script dialects will raise the same
// events as they run, so a C# mod reacts to an FF3 script without editing it.

using System;
using System.Collections.Generic;

namespace OpenFF
{
	public sealed class EventBus
	{
		private readonly Dictionary<Type, List<Delegate>> _handlers = new Dictionary<Type, List<Delegate>>();

		public IDisposable Subscribe<T>(Action<T> handler)
		{
			if (handler == null)
			{
				throw new ArgumentNullException(nameof(handler));
			}
			if (!_handlers.TryGetValue(typeof(T), out List<Delegate> list))
			{
				_handlers[typeof(T)] = list = new List<Delegate>();
			}
			list.Add(handler);
			return new Subscription(() => list.Remove(handler));
		}

		public void Publish<T>(T evt)
		{
			if (!_handlers.TryGetValue(typeof(T), out List<Delegate> list) || list.Count == 0)
			{
				return;
			}
			foreach (Delegate handler in list.ToArray())
			{
				Game.Guard("event " + typeof(T).Name, () => ((Action<T>)handler)(evt));
			}
		}

		/// <summary>Drops every subscription whose target lives in the given assembly (a mod being unloaded).</summary>
		internal void RemoveFrom(System.Reflection.Assembly assembly)
		{
			foreach (List<Delegate> list in _handlers.Values)
			{
				list.RemoveAll(d => d.Method.DeclaringType != null && d.Method.DeclaringType.Assembly == assembly);
			}
		}

		public int HandlerCount
		{
			get
			{
				int n = 0;
				foreach (List<Delegate> list in _handlers.Values)
				{
					n += list.Count;
				}
				return n;
			}
		}

		private sealed class Subscription : IDisposable
		{
			private Action _dispose;
			public Subscription(Action dispose) { _dispose = dispose; }
			public void Dispose()
			{
				_dispose?.Invoke();
				_dispose = null;
			}
		}
	}

	/// <summary>What is known about a scene (today: a legacy map) when it is entered or left.</summary>
	public sealed class SceneInfo
	{
		/// <summary>The map's name as the game names it: d01_05, f00, t24_01...</summary>
		public string Name { get; set; }
		/// <summary>DUNGEON, FIELD, TOWN, BATTLE, SHOP... as the legacy engine classifies it.</summary>
		public string Type { get; set; }
		/// <summary>Which game's assets the scene came from: ff3 or ff4.</summary>
		public string Source { get; set; }
		public override string ToString() => Name + " (" + Type + ", " + Source + ")";
	}

	namespace Events
	{
		/// <summary>The engine has started: services are up, mods loaded.</summary>
		public sealed class GameStarted { }

		/// <summary>A map has been entered.</summary>
		public sealed class MapEntered
		{
			public SceneInfo Scene { get; set; }
		}

		/// <summary>The current map is being left.</summary>
		public sealed class MapLeaving
		{
			public SceneInfo Scene { get; set; }
		}

		/// <summary>The legacy game moved to another part: TITLE, WORLD, BATTLE, ...</summary>
		public sealed class PartChanged
		{
			public string From { get; set; }
			public string To { get; set; }
		}

		/// <summary>The legacy game wrote a save (a slot or the quicksave) to its save file.</summary>
		public sealed class SaveWritten
		{
			/// <summary>The slot: the offset written in the 64 KB save file.</summary>
			public int Offset { get; set; }
			public int Length { get; set; }
		}

		/// <summary>The legacy game read back a save it had written (a load).</summary>
		public sealed class SaveRead
		{
			public int Offset { get; set; }
			public int Length { get; set; }
		}

		/// <summary>A mod's code was reloaded from a new build.</summary>
		public sealed class ModReloaded
		{
			public string ModId { get; set; }
		}

		// ---- raised by the game's own scripts and systems as they run ----

		/// <summary>A flag in the scripts' flag space changed (quest progress lives there).</summary>
		public sealed class FlagChanged
		{
			public uint Group { get; set; }
			public uint Index { get; set; }
			public bool Value { get; set; }
		}

		/// <summary>A script showed a message from the game's text (by its number).</summary>
		public sealed class MessageShown
		{
			public int Number { get; set; }
		}

		/// <summary>A script took control for a scene (EventStart); Ended when it gave it back.</summary>
		public sealed class CutsceneStarted { }
		public sealed class CutsceneEnded { }

		/// <summary>A battle is about to begin (a script's, or an encounter).</summary>
		public sealed class BattleStarting { }

		/// <summary>The party received an item from a script or a chest.</summary>
		public sealed class ItemGained
		{
			public int ItemId { get; set; }
			public int Count { get; set; }
		}

		/// <summary>A script asked for another map.</summary>
		public sealed class WarpRequested
		{
			public string Map { get; set; }
			public Vector3 Position { get; set; }
			public int Facing { get; set; }
		}

		/// <summary>The player answered a question the API asked.</summary>
		public sealed class Answered
		{
			public bool Yes { get; set; }
		}
	}
}
