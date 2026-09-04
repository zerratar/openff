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
// And it acts on the game through the engine API (Game.Npcs, Game.Dialogue, Game.Party,
// Game.Audio...): on each map it puts a villager beside the hero who, when talked to,
// greets the player and hands over some gil.
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
		public int Talks;
		public string LastMap = "-";
		private IDisposable _mapSubscription;
		private IDisposable _partSubscription;
		private Npc _villager;

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
				// And a villager beside the hero, who talks when the player presses A near them.
				SpawnVillager();
			});
			_partSubscription = Game.Events.Subscribe<PartChanged>(e => Game.Log(Greeting + ": part " + e.From + " -> " + e.To));
		}

		private void SpawnVillager()
		{
			if (Game.Hero == null || !Game.Hero.Present || Game.Npcs == null)
			{
				return;
			}
			Vector3 at = Game.Hero.Position;
			Vector3 spot = new Vector3(at.X + 2f, at.Y, at.Z);
			_villager = Game.Npcs.Spawn("n011", spot, 0f);
			if (_villager == null)
			{
				return;
			}
			_villager.Owner = Mod;
			_villager.LookAt(at);
			_villager.Interacted += npc =>
			{
				Talks++;
				npc.LookAt(Game.Hero.Position);
				Game.Audio?.PlaySe(0, 1);
				Game.Party.Gil += 10;
				Game.Dialogue.Say("Hello from a mod! You have talked to me " + Talks + " time(s)." + (char)10 + "Here, have 10 gil. You now carry " + Game.Party.Gil + ".");
				Game.Log(Greeting + ": talked to the villager (" + Talks + "), gil now " + Game.Party.Gil);
			};
			Game.Log(Greeting + ": villager spawned at " + spot + " next to the hero at " + at);
		}

		public override void OnUpdate()
		{
			Frames++;
			if (Frames % 600 == 0)
			{
				Game.Log(Greeting + ": " + Frames + " frames, " + MapsEntered + " maps, last " + LastMap);
			}
			// The villager keeps up: when the hero has walked off, it comes over (MoveTo).
			if (Frames % 90 == 0 && _villager != null && _villager.Alive && !_villager.Moving && Game.Hero.Present && !Game.Dialogue.IsOpen)
			{
				Vector3 hero = Game.Hero.Position;
				Vector3 me = _villager.Position;
				float dx = hero.X - me.X, dz = hero.Z - me.Z;
				float distance = (float)Math.Sqrt(dx * dx + dz * dz);
				if (distance > 3.5f)
				{
					// Stop two units short of the hero, on the line between them.
					float k = (distance - 2f) / distance;
					Vector3 target = new Vector3(me.X + dx * k, hero.Y, me.Z + dz * k);
					_villager.MoveTo(target, 45);
					Game.Log(Greeting + ": villager walking over (" + distance.ToString("0.0") + " away) to " + target);
				}
				else
				{
					_villager.LookAt(hero);
				}
			}
		}

		public override void OnSceneLoaded(SceneInfo scene) => Game.Log(Greeting + ": OnSceneLoaded " + scene.Name);
		public override void OnSceneUnloading(SceneInfo scene) => Game.Log(Greeting + ": OnSceneUnloading " + scene.Name);
		public override void OnQuit() => Game.Log(Greeting + ": OnQuit after " + Frames + " frames");

		/// <summary>Shown by the F1 overlay's world layer.</summary>
		public override IEnumerable<string> DebugLines()
		{
			yield return Greeting + " frames " + Frames + " maps " + MapsEntered + " last " + LastMap + " talks " + Talks;
			if (_villager != null && _villager.Alive)
			{
				yield return "villager " + _villager.Model + " at " + _villager.Position + " yaw " + _villager.Yaw.ToString("0") + (Game.Dialogue.IsOpen ? "  (talking)" : "");
			}
		}

		// ISaveable: the counts travel with the save slot.
		public string ChunkId => "hello/HelloService";
		public int ChunkVersion => 2;
		public object Save() => new { Frames, MapsEntered, LastMap, Talks };
		public void Load(int version, JsonElement data)
		{
			Frames = data.GetProperty("Frames").GetInt32();
			MapsEntered = data.GetProperty("MapsEntered").GetInt32();
			LastMap = data.GetProperty("LastMap").GetString();
			// Version 1 had no Talks; an older chunk loads with the default.
			Talks = version >= 2 && data.TryGetProperty("Talks", out JsonElement talks) ? talks.GetInt32() : 0;
			Game.Log(Greeting + ": loaded chunk v" + version + " - frames " + Frames + " maps " + MapsEntered + " talks " + Talks);
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
