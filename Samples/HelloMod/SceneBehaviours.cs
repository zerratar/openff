// Behaviours meant for the editor: Crystal lists them (Project > map > a character or the
// map itself > Behaviours), the modder fills in the public fields, and scenes/<map>.json
// carries the result; the engine puts them on the objects when the map is entered.
//
// A behaviour on a map character reaches it through the MapObject component the engine
// adds beside it: GetComponent<MapObject>().Npc is the handle (position, walking, talk).

using System;
using System.Collections.Generic;
using OpenFF;

namespace Hello
{
	/// <summary>A line of dialogue when the map is entered, after a short pause. Attach to the map.</summary>
	public class Welcome : Behaviour
	{
		/// <summary>What the window says.</summary>
		public string Text = "Welcome.";
		/// <summary>Frames to wait after the map appears.</summary>
		public int Delay = 90;
		/// <summary>Say it again on every visit, or only the first time this run.</summary>
		public bool EveryTime = true;

		private static readonly HashSet<string> Said = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		private int _frames;
		private bool _done;

		protected override void Update()
		{
			if (_done) return;
			_frames++;
			if (_frames < Delay || Game.Dialogue.IsOpen) return;
			_done = true;
			string key = GameObject.Name + "|" + Text;
			if (!EveryTime && Said.Contains(key)) return;
			Said.Add(key);
			Game.Dialogue.Say(Text);
			Game.Log(HelloService.Greeting + ": Welcome on " + GameObject.Name + " said \"" + Text + "\"");
		}

		public override IEnumerable<string> DebugLines()
		{
			yield return (_done ? "said" : "waiting " + (Delay - _frames)) + ": " + Text;
		}
	}

	/// <summary>A map character who greets the hero when talked to, or when the hero comes near. Attach to a character.</summary>
	public class Greeter : Behaviour
	{
		/// <summary>What the character says.</summary>
		public string Text = "Hello there!";
		/// <summary>Say it when the hero comes within this many units (0: only when talked to).</summary>
		public float Radius = 0f;
		/// <summary>Turn to face the hero first.</summary>
		public bool FaceHero = true;
		/// <summary>Frames between greetings when the hero stays near.</summary>
		public int Cooldown = 300;

		private MapObject _link;
		private long _lastGreet = -100000;

		protected override void Start()
		{
			_link = GetComponent<MapObject>();
			if (_link?.Npc != null)
			{
				_link.Npc.Interacted += _ => Greet("talk");
				Game.Log(HelloService.Greeting + ": Greeter on " + GameObject.Name + " (character " + _link.Index + " at " + _link.Npc.Position + ")");
			}
			else
			{
				Game.Warn(HelloService.Greeting + ": Greeter on " + GameObject.Name + " has no character to speak through");
			}
		}

		protected override void Update()
		{
			if (Radius <= 0 || _link?.Npc == null || !Game.Hero.Present || Game.Dialogue.IsOpen) return;
			if (Game.Time.Frame - _lastGreet < Cooldown) return;
			if (Vector3.FlatDistance(_link.Npc.Position, Game.Hero.Position) <= Radius)
			{
				Greet("near");
			}
		}

		private void Greet(string why)
		{
			if (_link?.Npc == null || Game.Dialogue.IsOpen) return;
			_lastGreet = Game.Time.Frame;
			if (FaceHero) _link.Npc.LookAt(Game.Hero.Position);
			Game.Dialogue.Say(Text);
			Game.Log(HelloService.Greeting + ": Greeter " + GameObject.Name + " greeted (" + why + ")");
		}

		public override IEnumerable<string> DebugLines()
		{
			yield return "greeter: " + Text + (Radius > 0 ? " within " + Radius : " on talk");
		}
	}
}
