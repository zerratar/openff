// The engine's root: what every script reaches first.
//
// Game.Services holds the whole-run singletons (GameService), Game.Events the typed
// event bus, Game.World the loaded scenes and their objects, Game.Saves the mod-extended
// save chunks, Game.Time the clock. The host (the OpenFF client) creates the engine once,
// hands it a logger and the loaded mods, and calls Update once per frame after the
// legacy game has run its own tick. See Docs/OpenFF-Engine.md, "The object model and
// scripting".

using System;
using System.Collections.Generic;

namespace OpenFF
{
	public static partial class Game
	{
		/// <summary>The engine API version mods compile against.</summary>
		public static readonly Version ApiVersion = typeof(Game).Assembly.GetName().Version;

		/// <summary>Every registered service, the game's and the mods'; Get&lt;T&gt; finds the current implementation of an interface.</summary>
		public static ServiceRegistry Services { get; } = new ServiceRegistry();
		/// <summary>The typed event bus: what the game and the mods publish as they run (see OpenFF.Events).</summary>
		public static EventBus Events { get; } = new EventBus();
		/// <summary>The object model: scenes and their game objects; the running game is the scene called "legacy".</summary>
		public static World World { get; } = new World();
		/// <summary>Save chunks: one per ISaveable per slot, kept across mod changes.</summary>
		public static SaveChunks Saves { get; } = new SaveChunks();
		/// <summary>The engine clock: frame count, delta and total seconds.</summary>
		public static GameTime Time { get; } = new GameTime();

		/// <summary>Where engine messages go; the host points this at its log.</summary>
		public static Action<string> Log { get; set; } = _ => { };

		/// <summary>Where warnings and caught exceptions go; the host points this at its log.</summary>
		public static Action<string> Warn { get; set; } = message => Log(message);

		/// <summary>The mods whose code is loaded, in load order.</summary>
		public static IReadOnlyList<Modding.LoadedMod> Mods => Modding.ModLoader.Loaded;

		/// <summary>True once Start has run.</summary>
		public static bool Started { get; private set; }

		/// <summary>
		/// Starts the engine: every registered service hears OnGameStart, then GameStarted
		/// is published. The host calls this once the content is open and the mods loaded.
		/// </summary>
		public static void Start()
		{
			if (Started)
			{
				return;
			}
			Started = true;
			Services.StartAll();
			Events.Publish(new Events.GameStarted());
		}

		/// <summary>One frame: the clock, the services that asked for updates, then the world.</summary>
		public static void Update(double deltaSeconds)
		{
			Time.Advance(deltaSeconds);
			Services.UpdateAll();
			World.Update();
			Coroutines.Update();
		}

		/// <summary>The host is closing: services hear OnQuit, in reverse order.</summary>
		public static void Quit()
		{
			Services.QuitAll();
		}

		/// <summary>Runs a piece of script code, keeping a script's exception from taking the game down.</summary>
		public static void Guard(string what, Action action)
		{
			try
			{
				action();
			}
			catch (Exception ex)
			{
				Warn(what + ": " + ex.GetType().Name + ": " + ex.Message);
			}
		}
	}

	/// <summary>The engine clock: frames since start, seconds since start, the last frame's length.</summary>
	public sealed class GameTime
	{
		public long Frame { get; private set; }
		public double Total { get; private set; }
		public double Delta { get; private set; }

		internal void Advance(double deltaSeconds)
		{
			Frame++;
			Delta = deltaSeconds;
			Total += deltaSeconds;
		}
	}
}
