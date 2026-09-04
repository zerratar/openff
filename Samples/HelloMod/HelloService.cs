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
// Game.Audio, Game.Camera...): on each map it puts a villager beside the hero who, when
// talked to, runs a small scene as a coroutine - a greeting, a yes/no question, a walk,
// a camera shake - and reacts to the game's own events (a flag set, a battle starting).
//
// Everything a script does is guarded: an exception is logged as "engine: WARNING ..."
// and the game goes on.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
			// The game's own story, heard as events.
			Game.Events.Subscribe<FlagChanged>(e => { if (e.Value) Game.Log(Greeting + ": flag " + e.Group + "/" + e.Index + " set"); });
			Game.Events.Subscribe<BattleStarting>(_ => Game.Log(Greeting + ": a battle starts - party " + string.Join(", ", Game.Party.Members)));
			Game.Events.Subscribe<CutsceneStarted>(_ => Game.Log(Greeting + ": cutscene started"));
			Game.Events.Subscribe<CutsceneEnded>(_ => Game.Log(Greeting + ": cutscene ended"));
			Game.Events.Subscribe<MessageShown>(e => Game.Log(Greeting + ": the game showed message " + e.Number));
			Game.Events.Subscribe<ItemGained>(e => Game.Log(Greeting + ": item " + e.ItemId + " x" + e.Count));
		}

		private void SpawnVillager()
		{
			if (Game.Hero == null || !Game.Hero.Present || Game.Npcs == null)
			{
				return;
			}
			Vector3 at = Game.Hero.Position;
			// Two characters together stand about 8 units apart; a little beyond that.
			Vector3 spot = new Vector3(at.X + 10f, at.Y, at.Z);
			_villager = Game.Npcs.Spawn("n011", spot, 0f);
			if (_villager == null)
			{
				return;
			}
			_villager.Owner = Mod;
			_villager.LookAt(at);
			_villager.Interacted += npc =>
			{
				if (_scene != null && _scene.Running)
				{
					return;
				}
				Talks++;
				_scene = Game.Run(TalkScene(npc), "hello talk");
			};
			Game.Log(Greeting + ": villager spawned at " + spot + " next to the hero at " + at);
		}

		private Coroutine _scene;

		/// <summary>The talk, as one routine: each yield waits for the player or the world.</summary>
		private IEnumerator TalkScene(Npc npc)
		{
			npc.LookAt(Game.Hero.Position);
			Game.Hero.Freeze();
			Game.Hero.LookAt(npc.Position);
			Game.Audio?.PlaySe(0, 1);
			Game.Dialogue.Say("Hello from a mod! You have talked to me " + Talks + " time(s).");
			yield return Wait.Dialogue();

			bool? answer = null;
			Game.Dialogue.Ask("Would you like 10 gil?", yes => answer = yes);
			yield return Wait.Until(() => answer != null);

			if (answer == true)
			{
				Game.Party.Gil += 10;
				Game.Camera.Shake(20, 0.5f);
				Game.Dialogue.Say("Here you are. You now carry " + Game.Party.Gil + " gil.");
			}
			else
			{
				npc.Balloon = true;
				Game.Dialogue.Say("Suit yourself.");
			}
			yield return Wait.Dialogue();
			npc.Balloon = false;

			// A little walk around the hero, then back to facing them.
			Vector3 hero = Game.Hero.Position;
			npc.MoveTo(hero + new Vector3(0, 0, 10), 40);
			yield return Wait.Walk(npc);
			npc.MoveTo(hero + new Vector3(10, 0, 0), 40);
			yield return Wait.Walk(npc);
			npc.LookAt(Game.Hero.Position);
			Game.Hero.Unfreeze();
			Game.Log(Greeting + ": talk " + Talks + " done, gil now " + Game.Party.Gil + ", party " + string.Join(", ", Game.Party.Members));
		}

		public bool Hud = true;

		public override void OnUpdate()
		{
			Frames++;
			// H toggles a HUD drawn by the mod; the HUD is redrawn every frame it is on. (Keys the pad
			// has are the game's: X and a shoulder button open the menu, Select toggles encounters.)
			if (Game.Input.KeyPressed("H"))
			{
				Hud = !Hud;
			}
			// J: the hero casts Fire at the villager - the game's own effect and
			// sound, the game's damage formula against a goblin's stats, the battle's floating number.
			if (Game.Input.KeyPressed("J") && Game.Hero.Present && _villager != null && _villager.Alive)
			{
				CastFire();
			}
			if (Hud && Game.Hero.Present)
			{
				string line = "hello mod  gil " + Game.Party.Gil + "  talks " + Talks + "  party " + Game.Party.Members.Count + "  (H hides, J casts)";
				float w = Game.Draw.MeasureText(line, 12) + 12;
				Game.Draw.Rect(800 - w - 8, 60, w, 20, new OpenFF.Color(0, 0, 0, 140));
				Game.Draw.Text(line, 800 - w - 2, 63, OpenFF.Color.Yellow, 12);
				if (_villager != null && _villager.Alive)
				{
					float d = Vector3.FlatDistance(_villager.Position, Game.Hero.Position);
					Game.Draw.Rect(800 - w - 8, 82, w, 4, new OpenFF.Color(0, 0, 0, 140));
					Game.Draw.Rect(800 - w - 8, 82, Math.Min(w, w * Math.Min(1f, d / 40f)), 4, d <= 14f ? OpenFF.Color.Green : OpenFF.Color.Red);
					// A name over the villager's head: the world point 12 units above the feet, projected.
					Vector2? head = Game.Camera.WorldToScreen(_villager.Position + new Vector3(0, 12, 0));
					if (head.HasValue)
					{
						string name = "Villager";
						float nw = Game.Draw.MeasureText(name, 12);
						Game.Draw.Rect(head.Value.X - nw / 2 - 4, head.Value.Y - 18, nw + 8, 16, new OpenFF.Color(0, 0, 0, 140));
						Game.Draw.Text(name, head.Value.X - nw / 2, head.Value.Y - 16, OpenFF.Color.White, 12);
						Game.Draw.Line(head.Value.X, head.Value.Y - 2, head.Value.X, head.Value.Y + 4, OpenFF.Color.White, 1);
					}
				}
				// The ground under the hero, from the map's collision, next to the hero's own height.
				float? ground = Game.Field.GroundHeight(Game.Hero.Position);
				string groundLine = "ground " + (ground.HasValue ? ground.Value.ToString("0.0") : "none") + "  hero y " + Game.Hero.Position.Y.ToString("0.0");
				Game.Draw.Text(groundLine, 800 - Game.Draw.MeasureText(groundLine, 12) - 2, 90, OpenFF.Color.White, 12);
			}
			if (Frames % 600 == 0)
			{
				Game.Log(Greeting + ": " + Frames + " frames, " + MapsEntered + " maps, last " + LastMap);
			}
			// The villager keeps up: when the hero has walked off, it comes over (MoveTo).
			if (Frames % 90 == 0 && _villager != null && _villager.Alive && !_villager.Moving && Game.Hero.Present && !Game.Dialogue.IsOpen && (_scene == null || !_scene.Running))
			{
				Vector3 hero = Game.Hero.Position;
				Vector3 me = _villager.Position;
				float dx = hero.X - me.X, dz = hero.Z - me.Z;
				float distance = (float)Math.Sqrt(dx * dx + dz * dz);
				if (distance > 16f)
				{
					// Stop ten units short of the hero, on the line between them.
					float k = (distance - 10f) / distance;
					Vector3 target = new Vector3(me.X + dx * k, hero.Y, me.Z + dz * k);
					_villager.MoveTo(target, 45);
					Game.Log(Greeting + ": villager walking over (" + distance.ToString("0.0") + " away) to " + target);
				}
				else
				{
					_villager.Stop();
					_villager.LookAt(hero);
				}
			}
		}

		public override void OnSceneLoaded(SceneInfo scene) => Game.Log(Greeting + ": OnSceneLoaded " + scene.Name);
		public override void OnSceneUnloading(SceneInfo scene) => Game.Log(Greeting + ": OnSceneUnloading " + scene.Name);
		public override void OnQuit() => Game.Log(Greeting + ": OnQuit after " + Frames + " frames");

		/// <summary>Shown by the F1 overlay's world layer.</summary>
		public int Casts;
		public int LastDamage;

		private void CastFire()
		{
			Spell fire = Game.Magic.Find("Fire") ?? Game.Magic.All.FirstOrDefault(s => s.School == MagicSchool.Black && s.Kind == MagicKind.Attack && s.Level == 1);
			if (fire == null)
			{
				Game.Log(Greeting + ": no Fire in the tables yet (" + Game.Magic.All.Count + " spells)");
				return;
			}
			PartyMember hero = Game.Party.Members.Count > 0 ? Game.Party.Members[0] : null;
			if (hero == null) return;
			// The villager takes it as a goblin would: the first monster in the table stands in.
			Monster goblin = Game.Monsters.All.FirstOrDefault(m => m.MaxHp > 0 && m.MaxHp < 9999);
			Stats target = goblin?.Stats ?? new Stats { Mind = 5, MagicDefense = 0 };
			int damage = Game.Magic.Damage(fire, hero.Stats, target);
			int effect = Game.Magic.CastOn(fire, _villager);
			Game.Screen.PopNumber(_villager.Position + new Vector3(0, 8, 0), damage);
			Game.Screen.Flash(new OpenFF.Color(255, 120, 60), 6, 2);
			_villager.Balloon = true;
			Casts++;
			LastDamage = damage;
			Game.Log(Greeting + ": " + hero.Name + " (" + hero.JobName + ", int " + hero.Stats.Intellect + ", skill " + hero.JobSkill + ") cast " + fire.Name
				+ " (power " + fire.Power + ", effect " + fire.EffectCategory + "/" + fire.EffectMember + " -> " + effect + ", sound " + fire.SoundArchive + "/" + fire.SoundNumber + ")"
				+ " at the villager as " + (goblin?.Name ?? "nobody") + " (L" + (goblin?.Level ?? 0) + ", " + (goblin?.MaxHp ?? 0) + " hp, weak " + target.Weakness + "): " + damage + " damage");
			if (Casts == 1)
			{
				Game.Log(Greeting + ": spells " + Game.Magic.All.Count + " - " + string.Join(", ", Game.Magic.All.Take(8).Select(s => s.Name + " L" + s.Level))
					+ "; monsters " + Game.Monsters.All.Count + " - " + string.Join(", ", Game.Monsters.All.Take(5).Select(m => m.Name + " " + m.MaxHp + "hp"))
					+ "; hero hp " + hero.Hp + "/" + hero.MaxHp + " charges " + string.Join("/", hero.Charges) + " spells " + string.Join(",", hero.Spells));
				MonsterGroup group = Game.Monsters.Group(1);
				if (group != null) Game.Log(Greeting + ": encounter group 1 = " + group);
			}
		}

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
