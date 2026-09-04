// The smallest OpenFF mod with code. Three things a mod can be made of:
//
//   - a GameService: one instance for the whole run, never destroyed by a map change.
//     It hears OnGameStart, OnUpdate (when it asks), the scene callbacks and OnQuit; it
//     subscribes to events; across a hot reload its public fields are handed to the new
//     instance (Frames, MapsEntered and LastMap below survive a rebuild).
//   - a Behaviour on a GameObject: Awake/Start/Update/OnDestroy, like Unity. Objects a mod
//     creates on the legacy scene are destroyed when the mod unloads.
//   - ISaveable: the service's counts travel with the save slot the game writes, in the
//     engine's chunk store, keyed "hello/HelloService", with a version for migration.
//
// Everything a script does is guarded: an exception is logged as "engine: WARNING ..."
// and the game goes on.

using System;
using System.Collections.Generic;
using System.Text.Json;
using OpenFF;
using OpenFF.Events;

namespace Hello
{
	public class HelloService : GameService, ISaveable
	{
		public const string Greeting = "hello v1";

		public int Frames;
		public int MapsEntered;
		public string LastMap = "-";
		private IDisposable _mapSubscription;
		private IDisposable _partSubscription;

		public override bool WantsUpdate => true;

		public override void OnGameStart()
		{
			Game.Log(Greeting + ": OnGameStart (frames so far " + Frames + ")");
			_mapSubscription = Game.Events.Subscribe<MapEntered>(e =>
			{
				MapsEntered++;
				LastMap = e.Scene.Name;
				Game.Log(Greeting + ": entered " + e.Scene);
				// An object with a behaviour on the legacy scene, each time a map is entered.
				GameObject marker = Game.World.Legacy.Add(new GameObject("hello-marker") { Owner = Mod });
				marker.AddComponent<Spinner>();
			});
			_partSubscription = Game.Events.Subscribe<PartChanged>(e => Game.Log(Greeting + ": part " + e.From + " -> " + e.To));
		}

		public override void OnUpdate()
		{
			Frames++;
			if (Frames % 600 == 0)
			{
				Game.Log(Greeting + ": " + Frames + " frames, " + MapsEntered + " maps, last " + LastMap);
			}
		}

		public override void OnSceneLoaded(SceneInfo scene) => Game.Log(Greeting + ": OnSceneLoaded " + scene.Name);
		public override void OnSceneUnloading(SceneInfo scene) => Game.Log(Greeting + ": OnSceneUnloading " + scene.Name);
		public override void OnQuit() => Game.Log(Greeting + ": OnQuit after " + Frames + " frames");

		/// <summary>Shown by the F1 overlay's world layer.</summary>
		public override IEnumerable<string> DebugLines()
		{
			yield return Greeting + " frames " + Frames + " maps " + MapsEntered + " last " + LastMap;
		}

		// ISaveable: the counts travel with the save slot.
		public string ChunkId => "hello/HelloService";
		public int ChunkVersion => 1;
		public object Save() => new { Frames, MapsEntered, LastMap };
		public void Load(int version, JsonElement data)
		{
			Frames = data.GetProperty("Frames").GetInt32();
			MapsEntered = data.GetProperty("MapsEntered").GetInt32();
			LastMap = data.GetProperty("LastMap").GetString();
			Game.Log(Greeting + ": loaded chunk v" + version + " - frames " + Frames + " maps " + MapsEntered);
		}
	}

	/// <summary>A behaviour: turns every frame and reports through the overlay.</summary>
	public class Spinner : Behaviour
	{
		private int _ticks;

		protected override void Start() => Game.Log(HelloService.Greeting + ": Spinner.Start on " + GameObject.Name + " #" + GameObject.Id);

		protected override void Update()
		{
			_ticks++;
			Transform.Rotation.Y = (Transform.Rotation.Y + 2f) % 360f;
		}

		protected override void OnDestroy() => Game.Log(HelloService.Greeting + ": Spinner.OnDestroy after " + _ticks + " ticks");

		public override IEnumerable<string> DebugLines()
		{
			yield return "yaw " + Transform.Rotation.Y.ToString("0") + " after " + _ticks + " ticks";
		}
	}
}
