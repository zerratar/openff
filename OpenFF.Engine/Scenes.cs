// Scene files: behaviours attached to a map's objects from the editor, not from code.
//
// Crystal lets a modder pick a map object (a character, an exit, the map itself) and
// attach one of the mod's Behaviour types to it, filling in its public fields - the way
// Unity's inspector does. That is saved as scenes/<map>.json in the mod:
//
//   { "map": "d01_05", "attachments": [
//       { "target": "object:3", "behaviour": "Greeter", "fields": { "Text": "Hello", "Radius": 12 } },
//       { "target": "map", "behaviour": "Welcome" } ] }
//
// When the engine enters that map it makes a GameObject per target in the legacy scene,
// gives it a MapObject component saying what it stands for (and, for a character, the Npc
// handle to move and talk through), and adds the behaviours with their fields set. The
// objects belong to the mod, so a hot reload of its code destroys and remakes them.
//
// A scene file also carries the mod's own objects - things that are not in the game's map
// data at all, and need no script of the game's to exist:
//
//   "objects": [ { "name": "Chest", "x": 12, "y": 0, "z": -20, "yaw": 180, "scale": 1,
//                  "model": "o001", "tags": [ "chest" ],
//                  "children": [ { "name": "Trigger", "x": 0, "z": 3, "tags": [] } ] } ]
//
// Each is a GameObject named <map>/<path> (Chest, Chest/Trigger), tagged "scene" and its
// own tags, with a MapObject of kind "scene". One with a model is shown through the host
// as a plain character (Npc), without a cast or a script - just the model standing there,
// the mod's behaviours making it do whatever it does; one without a model is a spot with
// logic on it. Children sit under their parent (GameObject.Parent) and their x/y/z, yaw
// and scale are relative to it: the loader works out the world transform, since Transform
// itself has no hierarchy. Behaviours attach to them by path: "target": "chest/trigger".
// Older files' "points" read as objects without a model; "point:<name>" still targets them.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace OpenFF
{
	/// <summary>What a scene-file object stands for on the legacy map.</summary>
	public sealed class MapObject : Component
	{
		/// <summary>"object" (a character by its index in the map's cast list), "exit" (a slot), "scene" (the mod's own object, placed in the editor), or "map".</summary>
		public string Kind { get; internal set; }
		public int Index { get; internal set; } = -1;
		/// <summary>A scene object's name from the editor (the last part of its path); null for the rest.</summary>
		public string Name { get; internal set; }
		/// <summary>A scene object's path in the file: "chest", "chest/trigger"; what an attachment targets.</summary>
		public string Path { get; internal set; }
		/// <summary>A scene object's model, when it has one; null for a spot with logic only.</summary>
		public string Model { get; internal set; }
		/// <summary>With a model: whether it is a walking character (talked to, wandering) rather than a plain figure.</summary>
		public bool Character { get; internal set; }
		/// <summary>With Character: spawned as the scripts' bootPlainCharacter does - the light walker with the model's own scale and kind.</summary>
		public bool Plain { get; internal set; }
		/// <summary>For a character, or a scene object with a model: the handle to move, turn, hide and talk through; null otherwise.</summary>
		public Npc Npc { get; internal set; }
		/// <summary>The map the object is on.</summary>
		public string Map { get; internal set; }
		/// <summary>Whether the Npc was spawned for this object (and goes with it), rather than being the map's own.</summary>
		internal bool OwnsNpc;
		public override string ToString() => Kind + (Index >= 0 ? ":" + Index : Path != null ? ":" + Path : "") + " on " + Map;

		internal override void OnDetached()
		{
			if (OwnsNpc && Npc != null)
			{
				Npc handle = Npc;
				Npc = null;
				Game.Guard("scene object " + Path + " remove", handle.Remove);
			}
		}
	}

	/// <summary>
	/// A spot inside which the hero counts as present: a behaviour for the editor to put on
	/// a scene object, so a mod's code (or another behaviour on the same object) hears when
	/// the hero walks in or out without polling distances itself. Fires Entered and Left on
	/// this instance and publishes Events.TriggerEntered / TriggerLeft for services.
	/// </summary>
	public sealed class Trigger : Behaviour
	{
		/// <summary>How close the hero has to come, in world units (two characters side by side are about 8 apart).</summary>
		public float Radius = 8f;
		/// <summary>Fire Entered once and then disable; off, it fires every time the hero comes back.</summary>
		public bool Once;
		/// <summary>True while the hero is inside.</summary>
		public bool HeroInside { get; private set; }
		/// <summary>The hero came within Radius.</summary>
		public event Action<Trigger> Entered;
		/// <summary>The hero went out of Radius.</summary>
		public event Action<Trigger> Left;

		protected override void Update()
		{
			if (Transform == null || !Game.Hero.Present) return;
			bool inside = Vector3.FlatDistance(Transform.WorldPosition, Game.Hero.Position) <= Radius;
			if (inside == HeroInside) return;
			HeroInside = inside;
			// Said in the log, so a trigger placed in the editor can be seen to work before any code hears it.
			Game.Log("trigger " + (GameObject?.Name ?? "?") + ": hero " + (inside ? "entered" : "left"));
			if (inside)
			{
				Entered?.Invoke(this);
				Game.Events.Publish(new Events.TriggerEntered { Trigger = this, Object = GameObject });
				if (Once) Enabled = false;
			}
			else
			{
				Left?.Invoke(this);
				Game.Events.Publish(new Events.TriggerLeft { Trigger = this, Object = GameObject });
			}
		}

		protected override void OnDisable()
		{
			HeroInside = false;
		}
	}

	/// <summary>
	/// What the built-in components remember across saves: which chests have been opened,
	/// which once-only things have happened, by the object's name (&lt;map&gt;/&lt;path&gt;). One
	/// chunk in the engine's save store; a mod's own code may use it too.
	/// </summary>
	public sealed class SceneMemory : ISaveable
	{
		/// <summary>The one instance, registered with the saves at Game.Start.</summary>
		public static SceneMemory Instance { get; } = new SceneMemory();
		private readonly HashSet<string> _done = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		/// <summary>The chunk's key in the save store.</summary>
		public string ChunkId => "openff/scene";
		/// <summary>The shape written: 1, a list of keys.</summary>
		public int ChunkVersion => 1;

		/// <summary>Whether a key has been marked (a chest opened, an event done).</summary>
		public bool Has(string key) => key != null && _done.Contains(key);
		/// <summary>Remembers a key.</summary>
		public void Mark(string key) { if (!string.IsNullOrEmpty(key)) _done.Add(key); }
		/// <summary>Forgets a key (a chest closes again).</summary>
		public void Forget(string key) { if (key != null) _done.Remove(key); }
		/// <summary>How many keys are remembered.</summary>
		public int Count => _done.Count;

		/// <summary>The keys, sorted; nothing when there are none.</summary>
		public object Save() => _done.Count == 0 ? null : _done.OrderBy(k => k, StringComparer.Ordinal).ToArray();

		/// <summary>The keys back from a save.</summary>
		public void Load(int version, JsonElement data)
		{
			_done.Clear();
			if (data.ValueKind != JsonValueKind.Array) return;
			foreach (JsonElement e in data.EnumerateArray())
			{
				if (e.ValueKind == JsonValueKind.String) _done.Add(e.GetString());
			}
		}
	}

	/// <summary>
	/// The common ground of the built-in components an editor places on a scene object: the
	/// object's character when it has a model (to hear the player talk to it), or the hero
	/// walking into a radius when it has none. Derive from it for a component of your own
	/// that works the same way; override Activate.
	/// </summary>
	public abstract class Interactable : Behaviour, INeedsNpc
	{
		/// <summary>Without a model to talk to: how close the hero comes, in world units, for the object to act.</summary>
		[Header("Without a model")]
		[Tooltip("Without a model to talk to: how close the hero must stand for A to act on it (two characters side by side are about 8 apart)")]
		public float Radius = 8f;
		/// <summary>
		/// Without a model: act the moment the hero walks in (on), or when the player presses A
		/// standing within Radius (off, the default) - approach, then interact, as with any
		/// character. With a model the game's own talk applies: face it and press A.
		/// </summary>
		[Tooltip("Act the moment the hero walks in; off, it waits for A pressed within Radius - approach, then interact")]
		public bool OnWalkIn;

		private bool _near;
		private Npc _npc;

		/// <summary>The object's character, when it has a model.</summary>
		protected Npc Npc => _npc;

		/// <summary>The player talked to it, or walked into it: what the component does.</summary>
		protected abstract void Activate();

		/// <summary>
		/// Whether this one applies right now (a Talk under a flag). Of the Interactables on
		/// one object, the first that applies is the one that acts - so a converted villager
		/// with three Talks, one per flag branch, says the right one.
		/// </summary>
		protected virtual bool Applies() => true;

		/// <summary>A flag list as the editor writes it - "0:14 !0:11" - all of which must hold.</summary>
		protected static bool FlagsHold(string when)
		{
			if (string.IsNullOrWhiteSpace(when)) return true;
			foreach (string part in when.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries))
			{
				bool wantOff = part.StartsWith("!", StringComparison.Ordinal);
				string[] pair = part.TrimStart('!').Split(':');
				if (pair.Length != 2 || !uint.TryParse(pair[0], out uint group) || !uint.TryParse(pair[1], out uint index)) continue;
				if (Game.Flags.Get(group, index) == wantOff) return false;
			}
			return true;
		}

		/// <summary>Sets a flag list as the editor writes it - "0:13 !1:2".</summary>
		protected static void SetFlags(string then)
		{
			if (string.IsNullOrWhiteSpace(then)) return;
			foreach (string part in then.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries))
			{
				bool off = part.StartsWith("!", StringComparison.Ordinal);
				string[] pair = part.TrimStart('!').Split(':');
				if (pair.Length != 2 || !uint.TryParse(pair[0], out uint group) || !uint.TryParse(pair[1], out uint index)) continue;
				Game.Flags.Set(group, index, !off);
			}
		}

		private bool IsTheOne()
		{
			if (!Enabled || GameObject == null || !GameObject.ActiveInHierarchy) return false;
			foreach (Interactable other in GameObject.GetComponents<Interactable>())
			{
				if (!other.Enabled) continue;
				if (other == this) return Applies();
				if (other.Applies()) return false;
			}
			return false;
		}

		protected override void Start()
		{
			MapObject link = GetComponent<MapObject>();
			if (link?.Npc != null) NpcReady(link);
		}

		public virtual void NpcReady(MapObject link)
		{
			if (link?.Npc == null || _npc == link.Npc) return;
			if (_npc != null) _npc.Interacted -= OnInteracted;
			_npc = link.Npc;
			_npc.Interacted += OnInteracted;
		}

		private void OnInteracted(Npc npc)
		{
			bool mine = IsTheOne();
			Game.Log("engine: " + (GameObject?.Name ?? "?") + " talked to: " + Name + (mine ? " acts" : " passes (another applies, or disabled)"));
			if (mine) Game.Guard(Name + ".Activate", Activate);
		}

		protected override void Update()
		{
			if (Transform == null || !Game.Hero.Present) return;
			// With a model the game's own talk applies (face it, press A) - unless it acts on the
			// hero walking in, which a figure can too: a monster met by touching it.
			if (_npc != null && !OnWalkIn) return;
			bool near = Vector3.FlatDistance(Transform.WorldPosition, Game.Hero.Position) <= Radius;
			if (OnWalkIn)
			{
				if (near && !_near && IsTheOne()) Game.Guard(Name + ".Activate", Activate);
			}
			else if (near && !Game.Dialogue.IsOpen && Game.Input.Pressed(Pad.A) && IsTheOne())
			{
				Game.Guard(Name + ".Activate", Activate);
			}
			_near = near;
		}

		protected override void OnDestroy()
		{
			if (_npc != null) _npc.Interacted -= OnInteracted;
		}
	}

	/// <summary>
	/// A treasure chest, placed from the editor: an item (with a count) and/or gil, given
	/// when the player opens it, said in the message window, remembered across saves when
	/// Once. Give the object the chest's model (o001) and it opens on talking to it; without
	/// a model, on A within Radius (or on walking in, with OnWalkIn). Derive and override
	/// OnOpened to add to it.
	/// </summary>
	public class Chest : Interactable
	{
		/// <summary>The item inside, by id; 0 to give only gil.</summary>
		[Header("Contents")]
		[ItemField, Tooltip("The item inside; none to give only gil")]
		public int Item;
		/// <summary>How many of the item.</summary>
		[Range(1, 99)]
		public int Count = 1;
		/// <summary>Gil inside, on top of the item or instead of it.</summary>
		[Tooltip("Gil inside, on top of the item or instead of it")]
		public int Gil;

		/// <summary>Opens once and stays open, across saves (SceneMemory); off, it gives its contents every time.</summary>
		[Header("Opening")]
		[Tooltip("Opens once and stays open, across saves; off, it gives its contents every time")]
		public bool Once = true;
		/// <summary>
		/// What the window says on opening. "@" (the default) is the game's own chest message
		/// for the contents - "The chest contained Potion." / "... 250 gil.", "You find Potion."
		/// for an item spot (o000) or a chest without a model - from the .msd, with the
		/// item and the gil filled in as the game fills them, in every language; "@1000142" is
		/// any line of the .msd by its id; anything else is said as written, with {what} the
		/// contents ("Potion x2 and 100 gil"). Empty says nothing.
		/// </summary>
		[Tooltip("What the window says: @ for the game's own chest message (any language), @<id> for a line of the .msd by id, or text with {what} for the contents; empty for nothing")]
		public string Message = "@";
		/// <summary>What the window says when it is already open; empty (the default) says nothing, as the game's opened chests do. "@&lt;id&gt;" for a line of the .msd.</summary>
		[Tooltip("What the window says when it is already open; empty says nothing, as the game's opened chests do; @<id> for a line of the .msd")]
		public string EmptyMessage = "";
		/// <summary>The game's own flag for this chest ("1:22"), as its setTreasureItem named it: set when opened and read at start, so the game's treasure count and anything else reading it agree. Empty for a chest of the mod's own.</summary>
		[Tooltip("The game's flag for the chest, group:index (a converted chest keeps its own); set when opened, read at start")]
		[FlagField]
		public string Flag = "";
		/// <summary>Play the opening as the game's chests do - the lid shut, swinging open, then open; the sound; the sparkle. For a chest model (o001); off for a model with no lid, or for an opening of your own in OnOpened.</summary>
		[Tooltip("The lid motions, the sound and the sparkle on opening, as the game's chests - for a chest model; off for a model without a lid")]
		public bool Animate = true;

		/// <summary>Whether it has been opened (this visit, or ever when Once).</summary>
		public bool Opened { get; private set; }

		private string Key => GameObject?.Name;
		private bool _opening;

		protected override void Start()
		{
			base.Start();
			if (Once && (SceneMemory.Instance.Has(Key) || (!string.IsNullOrWhiteSpace(Flag) && FlagsHold(Flag)))) Opened = true;
			if (Npc != null) Lid();
		}

		public override void NpcReady(MapObject link)
		{
			base.NpcReady(link);
			// A chest model is a treasure box to the game, which would open it itself (its own
			// flag, "It's locked." for an empty one); this component runs the opening instead.
			link.Npc.OwnChest();
			_spot = string.Equals(link.Model, "o000", StringComparison.OrdinalIgnoreCase);
			if (StartedFlag) Lid();
		}

		// An item lying about (the game's o000, its INVISIBLE object) says "You find ..." where
		// a box says "The chest contained ..."; so does a chest with no model at all.
		private bool _spot = true;

		/// <summary>
		/// The lid as the game shows it: 1003 shut, 1002 open (map.CMapObject's acts 0 and 6).
		/// The shut lid is held at the motion's end: the model's own pose is the open one, and
		/// the game's chests play 1003 before the screen fades in, where a scene object appears
		/// on a map already in view - played, the lid would be seen swinging shut.
		/// </summary>
		private void Lid()
		{
			if (!Animate || Npc == null) return;
			if (Opened) Npc.PlayMotion(1002, true);
			else Npc.HoldMotion(1003);
		}

		protected override void Activate()
		{
			if (Opened)
			{
				if (!string.IsNullOrEmpty(EmptyMessage)) Game.Dialogue.Say(EmptyMessage);
				return;
			}
			List<string> got = new List<string>();
			if (Item > 0)
			{
				int count = Math.Max(1, Count);
				Game.Party.AddItem(Item, count);
				string name = Game.Items.Find(Item)?.Name ?? ("item " + Item);
				got.Add(count > 1 ? name + " x" + count : name);
			}
			if (Gil > 0)
			{
				Game.Party.Gil += Gil;
				got.Add(Gil + " gil");
			}
			Opened = true;
			if (Once) SceneMemory.Instance.Mark(Key);
			if (!string.IsNullOrWhiteSpace(Flag)) SetFlags(Flag);
			if (Animate && Npc != null)
			{
				// As the game opens one: the sound (archive 1, 36), the lid's opening motion, the
				// sparkle (effect 102) a little above it; the open lid once the motion is done.
				Game.Guard(Name + ".look", () =>
				{
					Game.Audio.PlaySe(1, 36, 192, 127);
					Npc.PlayMotion(1001);
					Vector3 at = Npc.Position;
					Game.Effects.Spawn(102, 1, new Vector3(at.X, at.Y + 6f, at.Z));
				});
				_opening = true;
			}
			string what = got.Count == 0 ? "nothing" : string.Join(" and ", got);
			if (Message == "@")
			{
				// The game's own words (map.CMapObject's act 4, in the chests' gold colour 9): a
				// box says 1000142 "The chest contained ..." for an item, 1000141 for gold,
				// 1000140 for nothing; an item spot (o000, or no model) says 1000146 "You find ...".
				// The item and the gold go to the message's control codes. The game's chests hold
				// one or the other; one of ours with both is said in the game's phrasing, as text.
				if (Item > 0 && Gil > 0) Game.Dialogue.Say((_spot ? "You find " : "The chest contained ") + what + ".");
				else if (Item > 0) Game.Dialogue.Say((_spot ? "@1000146" : "@1000142") + " item=" + Item + " color=9");
				else if (Gil > 0) Game.Dialogue.Say("@1000141 gold=" + Gil + " color=9");
				else Game.Dialogue.Say("@1000140 color=9");
			}
			else if (!string.IsNullOrEmpty(Message)) Game.Dialogue.Say(Message.Replace("{what}", what));
			Game.Log("chest " + Key + ": " + what);
			OnOpened(what);
		}

		protected override void Update()
		{
			base.Update();
			if (_opening && Npc != null && Npc.MotionDone)
			{
				_opening = false;
				Npc.PlayMotion(1002, true);
			}
		}

		/// <summary>After the contents are given and said: a hook for a derived chest (a sound, a flag, a spawn).</summary>
		protected virtual void OnOpened(string what) { }
	}

	/// <summary>
	/// Someone (or something) to talk to, placed from the editor: lines said one after the
	/// other in the message window when the player talks to the object (or walks into it,
	/// without a model). Derive and override OnSaid for what happens after the last line.
	/// </summary>
	public class Talk : Interactable
	{
		/// <summary>The name over the window; empty for none.</summary>
		[Header("Dialogue")]
		[Tooltip("The name over the window; empty for none")]
		public string Speaker;
		/// <summary>The lines, said in turn, one window each; A goes on to the next.</summary>
		[Tooltip("Said in turn, one window each; A goes on to the next")]
		public string[] Lines = { "Hello." };
		/// <summary>Turn to the hero while talking (the model keeps its facing otherwise).</summary>
		[Tooltip("Turn to the hero while talking (a model's own facing otherwise)")]
		public bool FaceHero = true;

		/// <summary>Game flags that must hold for this Talk to be the one that speaks: "0:14 !0:11" (group:index, ! for off); empty for always.</summary>
		[Header("Flags")]
		[Tooltip("Flags that must hold for this one to speak: group:index, ! for off, e.g. 0:14 !0:11. Empty: always. Of several Talks on one object the first that holds speaks")]
		[FlagField]
		public string When = "";
		/// <summary>Game flags set after the last line: "0:13 !1:2".</summary>
		[Tooltip("Flags set after the last line: group:index, ! to clear, e.g. 0:13")]
		[FlagField]
		public string Then = "";

		private int _next = -1;
		private long _saidAt;

		protected override bool Applies() => FlagsHold(When);

		protected override void Activate()
		{
			if (Lines == null || Lines.Length == 0 || _next >= 0) return;
			if (Npc != null)
			{
				// As the game's talkBegin: a wanderer stops and turns to the player for the talk,
				// and walks on after (talkEnd).
				if (GetComponent<Wander>() != null) { Npc.Stop(); Npc.EndWander(); }
				if (FaceHero) Npc.LookAt(Game.Hero.Position);
			}
			_next = 0;
			SayNext();
		}

		protected override void Update()
		{
			base.Update();
			// The next line once the window has closed on the last one - a frame later at
			// the least, since the window may open on the frame after Say.
			if (_next > 0 && !Game.Dialogue.IsOpen && Game.Time.Frame > _saidAt + 1) SayNext();
		}

		private void SayNext()
		{
			if (_next < 0) return;
			if (_next >= Lines.Length)
			{
				_next = -1;
				SetFlags(Then);
				GetComponent<Wander>()?.Resume(Npc);
				Game.Guard(Name + ".OnSaid", OnSaid);
				return;
			}
			_saidAt = Game.Time.Frame;
			Game.Dialogue.Say(Lines[_next++], string.IsNullOrWhiteSpace(Speaker) ? null : Speaker);
		}

		/// <summary>After the last line has been dismissed.</summary>
		protected virtual void OnSaid() { }
	}

	/// <summary>
	/// A monster on the map: walk into it (or press A at it) and the game's battle begins with
	/// a formation - one of the game's monster parties, or one of the mod's own
	/// (defs/formations). Won once, it is gone for good (SceneMemory) unless Once is off; run
	/// from, it stays. The model is any the map can show; a monster's own battle model is the
	/// battle's, so a field figure stands for it here as the game's own visible foes do.
	/// Inherit and override OnWon / OnLost for what follows.
	/// </summary>
	public class Encounter : Interactable
	{
		/// <summary>The monster party to fight: the game's (monster_party_table.bbd) or the mod's own formation by number.</summary>
		[Header("Battle")]
		[FormationField, Tooltip("The formation: one of the game's monster parties, or one of the mod's own (Formations in the project)")]
		public int Formation;
		/// <summary>The battle background, as the game's battleMap ids; 0 for the map's default.</summary>
		[Tooltip("The battle background by the game's id; 0 for the default")]
		public int BattleMap;
		/// <summary>Fought and won once: the object is gone, across saves. Off, it is there again every visit.</summary>
		[Tooltip("Won once and gone for good, across saves; off, it comes back every visit")]
		public bool Once = true;
		/// <summary>Whether the party may run from this battle.</summary>
		[Tooltip("Whether the party may run from this battle")]
		public bool CanEscape = true;

		public Encounter()
		{
			// A monster is met by walking into it; A at it works too. Two figures stop about 8
			// apart, so the reach is a little more than that.
			OnWalkIn = true;
			Radius = 10f;
		}

		private string Key => "encounter:" + (GameObject?.Name ?? "?");
		private bool _fighting;

		/// <summary>Whether this one has been beaten (this visit, or ever when Once).</summary>
		public bool Beaten { get; private set; }

		// The battle takes the game out of the map and back: the scene is cleared on the way
		// out and placed again on the way in, so the component that started the fight is gone
		// when BattleEnded comes. The fight in progress is kept here, by the object's key; the
		// result waits for the object's next Start.
		private static string _pendingKey;
		private static bool _pendingOnce;
		private static IDisposable _watch;
		private static readonly Dictionary<string, BattleResult> _results = new Dictionary<string, BattleResult>();

		private static void Watch()
		{
			if (_watch != null) return;
			_watch = Game.Events.Subscribe<Events.BattleEnded>(e =>
			{
				if (_pendingKey == null) return;
				Game.Log("encounter " + _pendingKey + ": battle " + e.Result);
				if (e.Result == BattleResult.Won && _pendingOnce) SceneMemory.Instance.Mark(_pendingKey);
				_results[_pendingKey] = e.Result;
				_pendingKey = null;
			});
		}

		protected override void Start()
		{
			base.Start();
			if (Once && SceneMemory.Instance.Has(Key)) Gone();
			if (_results.TryGetValue(Key, out BattleResult result))
			{
				_results.Remove(Key);
				if (result == BattleResult.Won) { Gone(); OnWon(); }
				else if (result == BattleResult.Lost) OnLost();
			}
		}

		public override void NpcReady(MapObject link)
		{
			base.NpcReady(link);
			if (Beaten && link.Npc != null) { link.Npc.Hidden = true; link.Npc.Solid = false; }
		}

		protected override bool Applies() => !Beaten && !_fighting;

		protected override void Activate()
		{
			if (Beaten || _fighting || Formation <= 0) return;
			if (Game.Battle.InBattle) return;
			_fighting = true;
			Game.Log("encounter " + (GameObject?.Name ?? "?") + ": formation " + Formation);
			Watch();
			_pendingKey = Key;
			_pendingOnce = Once;
			Game.Battle.EscapeAllowed = CanEscape;
			Game.Battle.Start(Formation, BattleMap);
		}

		/// <summary>The figure off the map: hidden and walked through, so nothing is left to bump into or talk to.</summary>
		private void Gone()
		{
			Beaten = true;
			MapObject link = GetComponent<MapObject>();
			if (link?.Npc == null) return;
			link.Npc.Hidden = true;
			link.Npc.Solid = false;
		}

		/// <summary>The party won: the figure is gone (called on the object as the map comes back). Override for a reward, a flag, a line.</summary>
		protected virtual void OnWon() { }
		/// <summary>The party lost (the game's own game over follows).</summary>
		protected virtual void OnLost() { }
	}

	/// <summary>
	/// The object is there only while game flags hold: hidden and inactive otherwise, shown
	/// again when they change. What a map's boot does with flagOnJump around a boot - a
	/// villager who is home only after the elders have spoken - on the mod's own object.
	/// </summary>
	public sealed class WhenFlags : Behaviour
	{
		/// <summary>Flags that must hold: "0:14 !0:11" (group:index, ! for off).</summary>
		[Tooltip("Flags that must hold for the object to be there: group:index, ! for off, e.g. !0:14")]
		[FlagField]
		public string When = "";

		/// <summary>
		/// Follow the flags as they change while the map is up (on): the object comes and goes
		/// with them. Off, the flags count once, at the map's start - as the game's boot tests
		/// them: a character booted behind a flag stays or stays away until the map is entered
		/// again. Crystal's conversions set this off.
		/// </summary>
		[Tooltip("Follow the flags while the map is up; off, they count once at the map's start, as the boot's own tests do")]
		public bool Live = true;

		private bool? _shown;

		protected override void Update()
		{
			if (_shown.HasValue && !Live) return;
			bool hold = Holds(When);
			if (_shown == hold) return;
			_shown = hold;
			MapObject link = GetComponent<MapObject>();
			if (hold && link != null && link.Npc == null && link.Model != null)
			{
				// Not spawned at the map's start because the flags did not hold then - the
				// game would not have booted it either; now they do, so it comes.
				SceneLoader.SpawnModel(GameObject.Owner, link.Map, GameObject);
			}
			else if (link?.Npc != null)
			{
				link.Npc.Hidden = !hold;
			}
			foreach (Behaviour b in GameObject.GetComponents<Behaviour>())
			{
				if (b != this && !(b is ModelFollow)) b.Enabled = hold;
			}
		}

		/// <summary>
		/// Whether a flag expression as the editor writes it holds: flags that must all hold
		/// ("0:14 !0:11" - group:index, ! for off), or several such lists as alternatives with |
		/// ("!0:14 | 0:14 !0:11" - either), as a boot reached by more than one path has.
		/// </summary>
		public static bool Holds(string when)
		{
			if (string.IsNullOrWhiteSpace(when)) return true;
			foreach (string alternative in when.Split('|'))
			{
				if (All(alternative)) return true;
			}
			return false;
		}

		private static bool All(string when)
		{
			foreach (string part in when.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries))
			{
				bool wantOff = part.StartsWith("!", StringComparison.Ordinal);
				string[] pair = part.TrimStart('!').Split(':');
				if (pair.Length != 2 || !uint.TryParse(pair[0], out uint group) || !uint.TryParse(pair[1], out uint index)) continue;
				if (Game.Flags.Get(group, index) == wantOff) return false;
			}
			return true;
		}
	}

	/// <summary>
	/// A component that wants the object's character (MapObject.Npc) and may be added before
	/// it exists - an object whose WhenFlags do not hold yet is spawned when they do. The
	/// loader calls NpcReady after a spawn; a component's Awake calls it itself when the
	/// character is already there.
	/// </summary>
	internal interface INeedsNpc
	{
		void NpcReady(MapObject link);
	}

	/// <summary>
	/// The game's own cast on one of the mod's objects: talking to the object runs the map
	/// script's cast&lt;N&gt;_main, and every command the script addresses to that cast lands on
	/// the object - the stand-in is the original as far as the script can tell, 1:1. What
	/// Crystal's conversions put on a stand-in by default; swap it for Talk, Chest and the
	/// rest when the behaviour should become the mod's to edit. A chest's contents come from
	/// the boot's setTreasureItem/Money, so they are fields here.
	/// </summary>
	public sealed class GameCast : Behaviour, INeedsNpc
	{
		/// <summary>The cast number in the map's script (the .hich row's).</summary>
		[Tooltip("The cast in the map's script this object runs: talking to it runs cast<N>_main")]
		public int Cast;

		/// <summary>
		/// A scene's actor: not spawned with the map, but when the script boots the cast - where
		/// it boots it, facing as it does - and the script drives it from there. The object's own
		/// place is where Crystal shows it; the scene decides in play.
		/// </summary>
		[Tooltip("Appears when the game's script boots the cast (a scene's actor), where it boots it, and the script drives it from there")]
		public bool OnBoot;

		/// <summary>A chest: the boot's setTreasureItem/setTreasureMoney for this cast.</summary>
		[Header("Treasure (a chest's cast)")]
		[Tooltip("Set up as the game's treasure chest, from the boot's setTreasureItem/Money")]
		public bool Treasure;
		[ItemField, Tooltip("The item inside (setTreasureItem)")]
		public int Item;
		[Tooltip("Gil inside instead (setTreasureMoney)")]
		public int Gil;
		[Tooltip("The chest's own flag, group:index - set when opened; an opened chest shows its open lid")]
		[FlagField]
		public string Flag = "";

		/// <summary>The boot's changeColorCharacter for this cast: a texture variant (n024 on n021).</summary>
		[Header("Look")]
		[Tooltip("The boot's changeColorCharacter: the texture variant, e.g. n024")]
		public string Recolour = "";

		/// <summary>
		/// The boot's own commands on this cast, replayed on the stand-in once it runs the cast
		/// (Npc.RunScript): one per line as the disassembly writes them - setTreasureItem,
		/// bindMotion, startMotionCharacter, setCharacterDetectionRadius, setSignEffect, whatever
		/// the boot did. A line may start with a flag condition in brackets, "[!0:14] ...", for a
		/// command the boot ran under a test of its own. Crystal's exact conversion fills this
		/// with everything the boot did to the character, so nothing is approximated.
		/// </summary>
		[Header("Boot setup (the script's own commands)")]
		[Tooltip("The boot's commands on this cast, one per line, replayed on the stand-in; \"[flags] command(...)\" for one under a flag test")]
		public string[] Setup = new string[0];

		protected override void Awake()
		{
			MapObject link = GetComponent<MapObject>();
			if (link == null) return;
			if (link.Npc != null) NpcReady(link);
			else if (link.Model == null && Cast > 0) Game.Warn("GameCast " + Cast + " on " + (GameObject?.Name ?? "?") + ": the object has no model - a model spawned as a character is what runs a cast");
		}

		public void NpcReady(MapObject link)
		{
			if (link?.Npc == null) return;
			// With a CastScript beside it, the cast's code is the engine's to run: bind the row
			// (so the setup below and the code's commands land here) but leave the game's logic
			// out, or the talk would run the original as well.
			if (Cast > 0)
			{
				if (GetComponent<CastScript>() != null) link.Npc.BindCast(Cast); else link.Npc.RunCast(Cast);
			}
			if (Treasure)
			{
				string[] pair = (Flag ?? "").Split(':');
				int group = 0, index = 0;
				if (pair.Length == 2) { int.TryParse(pair[0], out group); int.TryParse(pair[1], out index); }
				link.Npc.SetTreasure(Item, Gil, group, index);
			}
			if (!string.IsNullOrWhiteSpace(Recolour)) link.Npc.Recolour(Recolour.Trim());
			foreach (string raw in Setup ?? new string[0])
			{
				string line = (raw ?? "").Trim();
				if (line.Length == 0 || line.StartsWith("//", StringComparison.Ordinal)) continue;
				if (line.StartsWith("[", StringComparison.Ordinal))
				{
					int close = line.IndexOf(']');
					if (close < 0) continue;
					string when = line.Substring(1, close - 1);
					line = line.Substring(close + 1).Trim();
					if (!WhenFlags.Holds(when)) continue;
				}
				link.Npc.RunScript(line);
			}
		}
	}

	/// <summary>
	/// The cast's own code, run by the engine: the lines of its main function as Crystal's
	/// disassembly writes them (talkBegin through the library call, a message window, its
	/// lines, flag tests and jumps to labels, end) - every command meaning what it means in
	/// the game, since the game's interpreter runs it, but the code living here, in the scene
	/// file, where a modder reads and changes it. With a GameCast beside it for the boot's
	/// setup, a converted talker is the game's talker to the last flag, and its words are a
	/// text field. Talking to the object (or the hero walking in, without a model) starts the
	/// code, as the game's talk would have started the cast's main.
	/// </summary>
	public sealed class CastScript : Interactable
	{
		/// <summary>The cast number the code runs as - a converted character's original number, so the commands that name it (talkBegin(23)) reach this object through the bound row; 0 for an object of the mod's own, which is given a number of its own (from 5000) and may write "@me" in its lines.</summary>
		[Tooltip("The cast the code runs as: a converted character's own number, or 0 for an object of the mod's own (a number is given, \"@me\" in the lines stands for it)")]
		public int Cast;

		/// <summary>The main function's lines, one per entry: commands, "label:" lines, comments after //. "@me" is this cast's number.</summary>
		[Header("Code (the game's script language)")]
		[Tooltip("The cast's main function, one line per entry: commands as the disassembly writes them, labels as \"loc_419E:\", // comments, @me for this cast's number; ends with end()")]
		public string[] Main = new string[0];

		private bool _defined;
		private int _cast;

		/// <summary>The number the code runs as: Cast, or the one given to an object of the mod's own.</summary>
		public int RunsAs => _cast;

		protected override void Start()
		{
			base.Start();
			Define();
		}

		private void Define()
		{
			if (_defined || Main == null || Main.Length == 0) return;
			IScripts scripts = Game.Scripts;
			if (scripts == null) return;
			if (_cast <= 0) _cast = Cast > 0 ? Cast : scripts.Allocate();
			_defined = scripts.Define(_cast, Main);
			if (!_defined) Game.Warn("CastScript " + _cast + " on " + (GameObject?.Name ?? "?") + ": the code did not compile - see the log");
			// An object of the mod's own: its row claimed now that the number is known.
			MapObject link = GetComponent<MapObject>();
			if (_defined && Cast <= 0 && link?.Npc != null) link.Npc.BindCast(_cast);
		}

		public override void NpcReady(MapObject link)
		{
			base.NpcReady(link);
			// The row bound here even without a GameCast beside: the code's commands name the cast.
			if (GetComponent<GameCast>() != null) return;
			if (Cast > 0) link.Npc.BindCast(Cast);
			else if (_cast > 0) link.Npc.BindCast(_cast);
		}

		protected override void Activate()
		{
			Define();
			IScripts scripts = Game.Scripts;
			if (scripts == null || !_defined) return;
			if (scripts.IsRunning(_cast)) return;
			if (!scripts.Start(_cast)) Game.Warn("CastScript " + _cast + ": did not start");
		}
	}

	/// <summary>
	/// A motion set bound to the object's model and the motion it plays from the start - what
	/// a map's boot does with bindMotion and startMotionCharacter (the villagers' idle sway,
	/// "w_light_old" 1001). Set may be empty for a motion the model has of its own.
	/// </summary>
	public sealed class Motion : Behaviour, INeedsNpc
	{
		/// <summary>The motion set to bind (w_light_man, w_light_old, b_b01...); empty for none.</summary>
		[Tooltip("The motion set to bind: w_light_man, w_light_old...; empty for the model's own")]
		public string Set = "";
		/// <summary>The motion to play, by index (1001 is the idle).</summary>
		public int Index = 1001;
		public bool Loop = true;

		protected override void Start()
		{
			MapObject link = GetComponent<MapObject>();
			if (link?.Npc != null) NpcReady(link);
		}

		public void NpcReady(MapObject link)
		{
			if (link?.Npc == null) return;
			Game.Guard("motion " + link.Path, () =>
			{
				if (!string.IsNullOrWhiteSpace(Set)) link.Npc.BindMotions(Set.Trim());
				if (Index > 0) link.Npc.PlayMotion(Index, Loop);
			});
		}
	}

	/// <summary>
	/// Makes the object's character wander about its spot (or stand, or follow the hero), as
	/// the map scripts' moveCharacter_StartRandom does. The object needs a model marked as a
	/// character; a plain model has no walker to drive.
	/// </summary>
	public sealed class Wander : Behaviour, INeedsNpc
	{
		/// <summary>Still, Wander or Follow.</summary>
		public NpcAi Ai = NpcAi.Wander;
		/// <summary>The walk's pattern and pace, as the scripts name it (moveCharacter_StartRandom's second operand): Man, Woman, Boy, Girl, Uncle, Aunt, OldMan, OldWoman.</summary>
		[Tooltip("The walk's pattern and pace, as the map scripts name it: a boy's, an old woman's...")]
		public WanderGait Gait = WanderGait.Default;

		protected override void Start()
		{
			MapObject link = GetComponent<MapObject>();
			if (link?.Npc != null) NpcReady(link);
		}

		public void NpcReady(MapObject link)
		{
			if (link?.Npc != null) Resume(link.Npc);
		}

		/// <summary>The walk as set: the scripts' StartRandom with the gait for Wander, EndRandom for Still, the follow AI otherwise.</summary>
		internal void Resume(Npc npc)
		{
			if (npc == null) return;
			Game.Guard("wander " + (GameObject?.Name ?? "?"), () =>
			{
				if (Ai == NpcAi.Wander) npc.StartWander(Gait);
				else if (Ai == NpcAi.Still) npc.EndWander();
				else npc.SetAi(Ai);
			});
		}
	}

	/// <summary>
	/// Takes the game's own character this is attached to off the map when the map is
	/// entered - what Crystal's "Convert to OpenFF object" leaves on the original, so the
	/// mod's object stands in its place. The map's script still has the cast; nothing
	/// talks to it any more. Nothing happens on a scene object of the mod's own.
	/// </summary>
	public sealed class Removed : Behaviour
	{
		/// <summary>Hide it instead of removing it (it still blocks and can be talked to); off by default.</summary>
		public bool HideOnly;
		/// <summary>The mod's object that stands in for it (its path): spawned first, then the original goes - a model only this character used stays loaded for the stand-in.</summary>
		[Tooltip("The mod's object standing in for it, by path; it is spawned before this one is removed")]
		public string StandIn = "";

		// Awake, not Start: the loader attaches these as it goes - after the stand-in has
		// spawned (a model the removed character alone used would be unloaded with it), and
		// before the rest, so the slot it held is free for the next.
		protected override void Awake()
		{
			MapObject link = GetComponent<MapObject>();
			if (link == null || link.Kind != "object" || link.Npc == null) return;
			if (HideOnly) link.Npc.Hidden = true;
			else Game.Guard("removed " + link, link.Npc.Remove);
			Game.Log("removed: the map's " + link + " (replaced by the mod)");
		}
	}

	/// <summary>
	/// Keeps a scene object's spawned model where its Transform says: move the object (or
	/// its parent) from code and the model comes along. Added by the loader to every object
	/// with a model; a mod need not touch it.
	/// </summary>
	/// <summary>
	/// A model of the mod's own on a scene object: its Model names a glTF file (assets/hut.glb)
	/// and the client draws it with the field's camera, no game character behind it. Follows
	/// the Transform; hidden with the object. Added by the loader; a mod may add one itself
	/// with a Path.
	/// </summary>
	public sealed class Mesh : Behaviour
	{
		/// <summary>The file, relative to the mod's folder (assets/hut.glb).</summary>
		[Tooltip("The glTF file in the mod's assets folder")]
		public string Path;
		/// <summary>Stand the model on its feet: its lowest point on the object's position rather than its origin.</summary>
		[Tooltip("Lift the model so its lowest point stands on the object's position")]
		public bool OnGround = true;
		/// <summary>An animation clip of the file (a Blender action by name) to play, looping, from the start; empty for the bind pose.</summary>
		[Tooltip("The file's animation clip to play, looping (a Blender action by its name); empty for the bind pose")]
		public string Clip = "";
		/// <summary>The clip's speed; 1 is the file's own.</summary>
		[Range(0.1f, 4f), Tooltip("The clip's speed; 1 is the file's own")]
		public float Speed = 1f;
		/// <summary>Its triangles are ground and walls to the characters: a floor to stand on, a wall to bump into. Off, it is walked through.</summary>
		[Tooltip("Its triangles are ground and walls: stand on it, bump into it; off, it is walked through")]
		public bool Solid;

		/// <summary>The client's handle while it stands.</summary>
		public MeshHandle Handle { get; private set; }

		private Vector3 _at;
		private float _yaw, _scale;
		private bool _hidden;

		protected override void Start()
		{
			Spawn();
		}

		private void Spawn()
		{
			if (Handle != null || string.IsNullOrWhiteSpace(Path) || Game.Meshes == null) return;
			string file = Path;
			Modding.LoadedMod mod = GameObject?.Owner;
			if (mod != null && !System.IO.Path.IsPathRooted(file)) file = System.IO.Path.Combine(mod.Directory, file);
			Handle = Game.Meshes.Spawn(file, Place(), Transform.WorldYaw, Transform.WorldScale);
			if (Handle == null) { Game.Warn("Mesh " + Path + " on " + (GameObject?.Name ?? "?") + ": not spawned"); return; }
			if (Handle.Problem != null) Game.Warn("Mesh " + Path + ": " + Handle.Problem);
			else if (OnGround) Handle.Position = Place();
			Handle.Solid = Solid;
			if (!string.IsNullOrWhiteSpace(Clip) && Handle.Problem == null && !Handle.Play(Clip, true, Speed))
				Game.Warn("Mesh " + Path + ": no clip '" + Clip + "' (it has: " + string.Join(", ", Handle.Clips) + ")");
			_at = Transform.WorldPosition; _yaw = Transform.WorldYaw; _scale = Transform.WorldScale;
			_hidden = !GameObject.ActiveInHierarchy;
			Handle.Hidden = _hidden;
		}

		private Vector3 Place()
		{
			Vector3 at = Transform.WorldPosition;
			if (OnGround && Handle != null && Handle.Problem == null) at.Y -= Handle.Min.Y * Transform.WorldScale;
			return at;
		}

		protected override void LateUpdate()
		{
			if (Handle == null) { Spawn(); if (Handle == null) return; if (OnGround) Handle.Position = Place(); }
			Vector3 at = Transform.WorldPosition;
			float yaw = Transform.WorldYaw, scale = Transform.WorldScale;
			if (at != _at || scale != _scale) { _at = at; _scale = scale; Handle.Scale = scale; Handle.Position = Place(); }
			if (yaw != _yaw) { Handle.Yaw = yaw; _yaw = yaw; }
			bool hidden = !GameObject.ActiveInHierarchy;
			if (hidden != _hidden) { Handle.Hidden = hidden; _hidden = hidden; }
			if (Handle.Solid != Solid) Handle.Solid = Solid;
		}

		protected override void OnDisable()
		{
			if (Handle != null) { Handle.Hidden = true; _hidden = true; }
		}

		protected override void OnDestroy()
		{
			Handle?.Remove();
			Handle = null;
		}
	}

	internal sealed class ModelFollow : Behaviour
	{
		private Vector3 _at;
		private float _yaw, _scale;

		protected override void Start()
		{
			_at = Transform.WorldPosition;
			_yaw = Transform.WorldYaw;
			_scale = Transform.WorldScale;
		}

		protected override void LateUpdate()
		{
			MapObject link = GetComponent<MapObject>();
			Npc npc = link?.Npc;
			if (npc == null) return;
			Vector3 at = Transform.WorldPosition;
			float yaw = Transform.WorldYaw, scale = Transform.WorldScale;
			if (at != _at) { npc.Teleport(at); _at = at; }
			if (yaw != _yaw) { npc.Face(yaw); _yaw = yaw; }
			if (scale != _scale) { npc.Scale = scale; _scale = scale; }
		}
	}

	/// <summary>
	/// A reference from a behaviour's field to another of the mod's scene objects, by its
	/// path on the map ("chest", "gate/left"). Crystal offers the map's objects to pick
	/// from; Resolve gives the GameObject when the map is up.
	/// </summary>
	public sealed class ObjectRef
	{
		/// <summary>The object's path in the scene file; empty for none.</summary>
		public string Path { get; set; } = "";

		public ObjectRef() { }
		public ObjectRef(string path) { Path = path ?? ""; }

		public bool IsSet => !string.IsNullOrWhiteSpace(Path);

		/// <summary>The object on the current map, or null when there is none of that path.</summary>
		public GameObject Resolve() => IsSet ? SceneObjects.Find(Path) : null;

		/// <summary>The object's MapObject, for its model handle (Npc) and the like.</summary>
		public MapObject Link => Resolve()?.GetComponent<MapObject>();

		public override string ToString() => Path;
	}

	/// <summary>
	/// The mod's scene objects on the current map, by path, and copies of them: what a scene
	/// file authored in Crystal offers to code. Spawn makes a new object from a definition in
	/// the file - its model, tags and behaviours - at a spot of the code's choosing, so one
	/// authored object serves as the template for many.
	/// </summary>
	public static class SceneObjects
	{
		private sealed class Definition
		{
			public SceneObject Object;
			public string Path;
			public Modding.LoadedMod Mod;
			public List<SceneAttachment> Attachments = new List<SceneAttachment>();
		}

		private static string _map;
		private static readonly Dictionary<string, Definition> _definitions = new Dictionary<string, Definition>(StringComparer.OrdinalIgnoreCase);
		private static int _spawned;

		/// <summary>The map the definitions are for.</summary>
		public static string Map => _map;

		internal static void Remember(string map, Modding.LoadedMod mod, SceneFile file)
		{
			if (!string.Equals(_map, map, StringComparison.OrdinalIgnoreCase))
			{
				_definitions.Clear();
				_spawned = 0;
				_map = map;
			}
			foreach (SceneObject root in file.AllObjects()) Walk(root, null, mod);
			foreach (SceneAttachment a in file.Attachments ?? new List<SceneAttachment>())
			{
				if (a?.Target == null) continue;
				string key = a.Target.Trim().ToLowerInvariant();
				if (key.StartsWith("point:", StringComparison.Ordinal)) key = key.Substring(6);
				if (_definitions.TryGetValue(key, out Definition d)) d.Attachments.Add(a);
			}
		}

		private static void Walk(SceneObject item, string parentPath, Modding.LoadedMod mod)
		{
			if (item == null || string.IsNullOrWhiteSpace(item.Name)) return;
			string path = parentPath == null ? item.Name.Trim() : parentPath + "/" + item.Name.Trim();
			_definitions[path] = new Definition { Object = item, Path = path, Mod = mod };
			foreach (SceneObject child in item.Children ?? new List<SceneObject>()) Walk(child, path, mod);
		}

		/// <summary>A scene object on the current map by its path ("chest", "gate/left"); null when there is none.</summary>
		public static GameObject Find(string path)
		{
			if (string.IsNullOrWhiteSpace(path) || _map == null) return null;
			return Game.World.Legacy.Find(_map + "/" + path.Trim());
		}

		/// <summary>Every scene object on the current map (the ones with a MapObject of kind scene).</summary>
		public static IEnumerable<GameObject> All()
		{
			return Game.World.Legacy.All().Where(o => o.GetComponent<MapObject>()?.Kind == "scene");
		}

		/// <summary>The paths the current map's scene file defines - what a mod may Spawn.</summary>
		public static IEnumerable<string> Defined => _definitions.Keys;

		/// <summary>
		/// A new object from the scene file's definition at a path - the same model, tags and
		/// behaviours (with the file's field values) - at a spot, facing a yaw, at the top
		/// level of the scene. Named &lt;map&gt;/&lt;path&gt;#N. Null when the path is not defined.
		/// </summary>
		public static GameObject Spawn(string path, Vector3 at, float yaw = 0f, GameObject parent = null)
		{
			if (string.IsNullOrWhiteSpace(path) || !_definitions.TryGetValue(path.Trim(), out Definition d)) return null;
			string name = d.Path + "#" + (++_spawned);
			SceneObject copy = new SceneObject
			{
				Name = name, X = at.X, Y = at.Y, Z = at.Z, Yaw = yaw, Scale = d.Object.Scale,
				Model = d.Object.Model, Character = d.Object.Character, Tags = new List<string>(d.Object.Tags ?? new List<string>())
			};
			GameObject o = SceneLoader.Build(_map, name, name, copy);
			o.Owner = d.Mod;
			o.Tags.Add("spawned");
			if (parent != null) o.SetParent(parent, keepWorld: false);
			Game.World.Legacy.Add(o);
			SceneLoader.SpawnModel(d.Mod, _map, o);
			foreach (SceneAttachment a in d.Attachments)
			{
				SceneLoader.Attach(d.Mod, _map, o, a);
			}
			return o;
		}

		/// <summary>Takes a spawned (or any scene) object off the map, its model with it.</summary>
		public static void Destroy(GameObject o)
		{
			if (o != null) Game.World.Legacy.Destroy(o);
		}
	}

	/// <summary>
	/// A behaviour whose state rides in the saves: derive, keep the state in Save/Load, and
	/// it is written with the game's own save and back on load, keyed by the object it is
	/// on and its type (one chest, one key). Registered when it wakes, dropped when it goes.
	/// </summary>
	public abstract class SavedBehaviour : Behaviour, ISaveable
	{
		/// <summary>&lt;mod&gt;/&lt;object name&gt;/&lt;type&gt;, unless overridden.</summary>
		public virtual string ChunkId => (GameObject?.Owner?.Id ?? "scene") + "/" + (GameObject?.Name ?? "?") + "/" + GetType().Name;
		public virtual int ChunkVersion => 1;
		/// <summary>What to keep: anything System.Text.Json can serialise; null for nothing.</summary>
		public abstract object Save();
		/// <summary>What was kept, back.</summary>
		public abstract void Load(int version, JsonElement data);

		protected override void Awake() => Game.Saves.Register(this);
		protected override void OnDestroy() => Game.Saves.Unregister(this);
	}

	/// <summary>The mod's own object in a scene file: a spot, or a model standing there, with children under it.</summary>
	public sealed class SceneObject
	{
		/// <summary>The name; unique among its siblings, not / or :.</summary>
		public string Name { get; set; }
		/// <summary>Position; for a child, relative to its parent (turned by the parent's yaw, scaled by its scale).</summary>
		public float X { get; set; }
		/// <summary>Height; relative to the parent for a child.</summary>
		public float Y { get; set; }
		/// <summary>Forward; relative to the parent for a child.</summary>
		public float Z { get; set; }
		/// <summary>Facing in degrees, the engine's yaw (0 = +Z, 90 = +X); for a child, added to the parent's.</summary>
		public float Yaw { get; set; }
		/// <summary>Uniform scale, 1 = the model's own size; for a child, multiplied by the parent's.</summary>
		public float Scale { get; set; } = 1f;
		/// <summary>A model name (o001, n011...) to show, or null for a spot with logic only.</summary>
		public string Model { get; set; }
		/// <summary>With a model: a character (walks, turns to the player, can wander) rather than a plain figure. What a converted villager is.</summary>
		public bool Character { get; set; }
		/// <summary>With Character: made as the scripts' bootPlainCharacter makes one - the light walker with the model's own scale and kind (a child's model at 0.8). What a plain-booted villager is.</summary>
		public bool Plain { get; set; }
		/// <summary>Words a mod finds it by (GameObject.Tags).</summary>
		public List<string> Tags { get; set; } = new List<string>();
		/// <summary>Objects under this one, their transforms relative to it.</summary>
		public List<SceneObject> Children { get; set; } = new List<SceneObject>();
	}

	/// <summary>One attachment in a scene file.</summary>
	public sealed class SceneAttachment
	{
		public string Target { get; set; }
		public string Behaviour { get; set; }
		public Dictionary<string, JsonElement> Fields { get; set; }
	}

	/// <summary>The older files' spot placed in the editor; read as a SceneObject without a model. New files write objects.</summary>
	public sealed class ScenePoint
	{
		/// <summary>The name, unique on the map.</summary>
		public string Name { get; set; }
		/// <summary>World position.</summary>
		public float X { get; set; }
		/// <summary>World height.</summary>
		public float Y { get; set; }
		/// <summary>World forward.</summary>
		public float Z { get; set; }
		/// <summary>Facing in degrees, the engine's yaw (0 = +Z, 90 = +X).</summary>
		public float Yaw { get; set; }
		/// <summary>Words a mod finds it by (GameObject.Tags).</summary>
		public List<string> Tags { get; set; } = new List<string>();
	}

	public sealed class SceneFile
	{
		public string Map { get; set; }
		/// <summary>The mod's own objects, a tree; each becomes a GameObject named &lt;map&gt;/&lt;path&gt;, behaviours or not.</summary>
		public List<SceneObject> Objects { get; set; } = new List<SceneObject>();
		/// <summary>The older shape: points, read as objects without a model.</summary>
		public List<ScenePoint> Points { get; set; } = new List<ScenePoint>();
		public List<SceneAttachment> Attachments { get; set; } = new List<SceneAttachment>();

		/// <summary>The objects with the points folded in (a point whose name an object already has is dropped).</summary>
		public List<SceneObject> AllObjects()
		{
			List<SceneObject> all = new List<SceneObject>(Objects ?? new List<SceneObject>());
			foreach (ScenePoint point in Points ?? new List<ScenePoint>())
			{
				if (point == null || string.IsNullOrWhiteSpace(point.Name)) continue;
				if (all.Any(o => o != null && string.Equals(o.Name?.Trim(), point.Name.Trim(), StringComparison.OrdinalIgnoreCase))) continue;
				all.Add(new SceneObject { Name = point.Name, X = point.X, Y = point.Y, Z = point.Z, Yaw = point.Yaw, Tags = point.Tags ?? new List<string>() });
			}
			return all;
		}

		public static SceneFile Read(string path)
		{
			JsonSerializerOptions options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, ReadCommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true };
			return JsonSerializer.Deserialize<SceneFile>(File.ReadAllText(path), options);
		}
	}

	/// <summary>Applies the mods' scene files to the map the game is on.</summary>
	public static class SceneLoader
	{
		/// <summary>The host's way from a target to a character handle: kind and index in, an Npc (or null) out.</summary>
		public static Func<string, int, Npc> ResolveNpc;

		/// <summary>Every loaded mod's scene file for a map. What an earlier map's files made goes first.</summary>
		public static int ApplyAll(string map)
		{
			Clear();
			int made = 0;
			foreach (Modding.LoadedMod mod in Game.Mods.ToArray())
			{
				made += Apply(mod, map);
			}
			return made;
		}

		/// <summary>
		/// Destroys every object a scene file made (anything carrying a MapObject), with the
		/// models spawned for them. Called when a map is left and before the next one's files
		/// apply: the objects stand for things on one map and used to outlive it.
		/// </summary>
		public static int Clear()
		{
			Scene scene = Game.World.Legacy;
			GameObject[] gone = scene.All().Where(o => o.GetComponent<MapObject>() != null).ToArray();
			foreach (GameObject o in gone)
			{
				scene.Destroy(o);
			}
			return gone.Length;
		}

		private static IDisposable _boots;

		/// <summary>The game's character a Removed attachment means (its target, object:&lt;row&gt;), when it is on the map.</summary>
		private static Npc OriginalOf(SceneAttachment removal)
		{
			string key = removal?.Target?.Trim().ToLowerInvariant() ?? "";
			if (!key.StartsWith("object:", StringComparison.Ordinal) || !int.TryParse(key.Substring(7), out int row) || ResolveNpc == null) return null;
			Npc npc = null;
			Game.Guard("original " + key, () => npc = ResolveNpc("object", row));
			return npc;
		}

		/// <summary>
		/// The script booted a cast (Events.CastBooted): when one of the mod's objects on this
		/// map carries a GameCast for it, the object takes over - spawned (or moved) to where the
		/// script put the character, facing as it does, the game's own taken off, and RunCast so
		/// the script drives the stand-in from here. A scene's actor comes when the scene says,
		/// where it says, exactly as the original would have.
		/// </summary>
		private static void WatchBoots()
		{
			if (_boots != null) return;
			_boots = Game.Events.Subscribe<Events.CastBooted>(e =>
			{
				if (e?.Character == null || e.Cast <= 0) return;
				foreach (GameObject o in Game.World.Legacy.All().ToArray())
				{
					MapObject link = o.GetComponent<MapObject>();
					GameCast cast = o.GetComponent<GameCast>();
					if (link == null || cast == null || cast.Cast != e.Cast || !string.Equals(link.Map, e.Map, StringComparison.OrdinalIgnoreCase)) continue;
					if (link.Npc != null && ReferenceEquals(link.Npc, e.Character)) continue;
					Game.Guard("cast " + e.Cast + " booted", () =>
					{
						Vector3 at = e.Character.Position;
						float yaw = e.Character.Yaw;
						o.Transform.WorldPosition = at;
						o.Transform.WorldYaw = yaw;
						if (link.Npc == null)
						{
							// Spawned beside the original, then the original goes: a model only it
							// used stays loaded that way.
							SpawnModel(o.Owner, link.Map, o);
							e.Character.Remove();
						}
						else
						{
							e.Character.Remove();
							link.Npc.Teleport(at);
							link.Npc.Face(yaw);
							cast.NpcReady(link);
						}
						Game.Log("engine: " + o.Name + " took over cast " + e.Cast + " as the script booted it at " + at);
					});
					return;
				}
			});
		}

		/// <summary>The engine's own behaviours a scene file may name without the mod's code having them: Trigger.</summary>
		public static Type EngineBehaviour(string name)
		{
			return typeof(Trigger).Assembly.GetTypes()
				.FirstOrDefault(t => typeof(Behaviour).IsAssignableFrom(t) && !t.IsAbstract && t.IsPublic
					&& string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase));
		}

		/// <summary>One mod's scene file for a map; the number of objects made.</summary>
		public static int Apply(Modding.LoadedMod mod, string map)
		{
			if (mod?.Definition?.Scenes == null || string.IsNullOrEmpty(map)) return 0;
			string path = Path.Combine(mod.Definition.Scenes, map + ".json");
			if (!File.Exists(path)) return 0;
			SceneFile file;
			try
			{
				file = SceneFile.Read(path);
			}
			catch (Exception ex)
			{
				Game.Warn("mod " + mod.Id + ": scenes/" + map + ".json does not read: " + ex.Message);
				return 0;
			}
			if (file == null) return 0;
			Scene scene = Game.World.Legacy;
			Dictionary<string, GameObject> objects = new Dictionary<string, GameObject>(StringComparer.OrdinalIgnoreCase);
			int made = 0;
			// Every object in the tree, with or without behaviours: a mod finds them by name or tag.
			// The definitions are kept, so SceneObjects.Spawn can make more of one later.
			SceneObjects.Remember(map, mod, file);
			List<SceneAttachment> attachments = (file.Attachments ?? new List<SceneAttachment>())
				.Where(a => a != null && !string.IsNullOrEmpty(a.Behaviour) && !string.IsNullOrEmpty(a.Target)).ToList();
			// The game's characters the mod takes off the map, by the stand-in that replaces
			// each: the stand-in spawns first (a model only the original used would go with it),
			// then the original goes, freeing its slot for the next. The rest go at the end.
			List<SceneAttachment> removals = attachments.Where(a => string.Equals(a.Behaviour, "Removed", StringComparison.OrdinalIgnoreCase)).ToList();
			attachments.RemoveAll(a => removals.Contains(a));
			Dictionary<string, SceneAttachment> byStandIn = new Dictionary<string, SceneAttachment>(StringComparer.OrdinalIgnoreCase);
			foreach (SceneAttachment r in removals)
			{
				string standIn = r.Fields != null && r.Fields.TryGetValue("StandIn", out JsonElement s) && s.ValueKind == JsonValueKind.String ? s.GetString() : null;
				if (!string.IsNullOrWhiteSpace(standIn) && !byStandIn.ContainsKey(standIn.Trim())) byStandIn[standIn.Trim()] = r;
			}
			void Remove(SceneAttachment removal)
			{
				string key = removal.Target.Trim().ToLowerInvariant();
				if (!objects.TryGetValue(key, out GameObject target))
				{
					target = MakeTarget(mod, map, key);
					if (target == null) return;
					objects[key] = target;
					made++;
				}
				Attach(mod, map, target, removal);
				removals.Remove(removal);
			}
			// An object whose WhenFlags do not hold is not spawned yet - the game would not have
			// booted it; WhenFlags spawns it when they come true. Its Removed still goes through.
			// A GameCast with OnBoot (a scene's actor) is not spawned either: it comes when the
			// script boots its cast, where it boots it (CastBooted).
			Dictionary<string, string> gates = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			HashSet<string> onBoot = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach (SceneAttachment a in attachments)
			{
				if (a.Fields == null) continue;
				if (string.Equals(a.Behaviour, "WhenFlags", StringComparison.OrdinalIgnoreCase)
					&& a.Fields.TryGetValue("When", out JsonElement w) && w.ValueKind == JsonValueKind.String) gates[a.Target.Trim()] = w.GetString();
				if (string.Equals(a.Behaviour, "GameCast", StringComparison.OrdinalIgnoreCase)
					&& a.Fields.TryGetValue("OnBoot", out JsonElement b) && b.ValueKind == JsonValueKind.True) onBoot.Add(a.Target.Trim());
			}
			foreach (SceneObject root in file.AllObjects())
			{
				made += Place(mod, map, scene, root, null, null, objects, path =>
				{
					if (!byStandIn.TryGetValue(path, out SceneAttachment removal) || !removals.Contains(removal)) return;
					if (onBoot.Contains(path) && objects.TryGetValue(path.ToLowerInvariant(), out GameObject standIn))
					{
						// The scene already booted its actor (a scene that opens with the map runs
						// before the files apply): the stand-in takes over where it stands, now.
						Npc original = OriginalOf(removal);
						if (original != null && original.Alive)
						{
							standIn.Transform.WorldPosition = original.Position;
							standIn.Transform.WorldYaw = original.Yaw;
							SpawnModel(mod, map, standIn);
						}
					}
					Remove(removal);
				}, path => !onBoot.Contains(path) && (!gates.TryGetValue(path, out string when) || WhenFlags.Holds(when)));
			}
			WatchBoots();
			foreach (SceneAttachment removal in removals.ToList())
			{
				Remove(removal);
			}
			foreach (SceneAttachment attachment in attachments)
			{
				string key = attachment.Target.Trim().ToLowerInvariant();
				// The older files' "point:<name>" is the object called <name>.
				if (key.StartsWith("point:", StringComparison.Ordinal)) key = key.Substring(6);
				if (!objects.TryGetValue(key, out GameObject target))
				{
					target = MakeTarget(mod, map, key);
					if (target == null) continue;
					objects[key] = target;
					made++;
				}
				Attach(mod, map, target, attachment);
			}
			if (made > 0)
			{
				Game.Log("engine: mod " + mod.Id + ": " + made + " object(s) from scenes/" + map + ".json");
			}
			return made;
		}

		/// <summary>
		/// One scene object and its children as GameObjects. A child's Transform is relative
		/// to its parent's, as the file has it; Transform works the world values out
		/// (WorldPosition, WorldYaw, WorldScale), so a parent moved by code takes its children
		/// along, and a model spawned for an object follows its transform (ModelFollow).
		/// </summary>
		private static int Place(Modding.LoadedMod mod, string map, Scene scene, SceneObject item, GameObject parent, string parentPath, Dictionary<string, GameObject> objects, Action<string> placed = null, Func<string, bool> spawnNow = null)
		{
			if (item == null || string.IsNullOrWhiteSpace(item.Name)) return 0;
			string name = item.Name.Trim();
			string path = parentPath == null ? name : parentPath + "/" + name;
			string key = path.ToLowerInvariant();
			if (objects.ContainsKey(key) || key == "map" || key.StartsWith("object:", StringComparison.Ordinal) || key.StartsWith("exit:", StringComparison.Ordinal))
			{
				Game.Warn("mod " + mod.Id + ": scenes/" + map + ".json has two objects at '" + path + "', or a reserved name; the second is skipped");
				return 0;
			}
			GameObject o = Build(map, path, name, item);
			o.Owner = mod;
			if (parent != null) o.SetParent(parent, keepWorld: false);
			scene.Add(o);
			if (spawnNow == null || spawnNow(path)) SpawnModel(mod, map, o);
			objects[key] = o;
			placed?.Invoke(path);
			int made = 1;
			foreach (SceneObject child in item.Children ?? new List<SceneObject>())
			{
				made += Place(mod, map, scene, child, o, path, objects, placed, spawnNow);
			}
			return made;
		}

		/// <summary>One attachment onto an object: the behaviour (the mod's or the engine's) with its fields set, then added so Awake sees them.</summary>
		internal static Behaviour Attach(Modding.LoadedMod mod, string map, GameObject target, SceneAttachment attachment)
		{
			if (target == null || attachment == null || string.IsNullOrEmpty(attachment.Behaviour)) return null;
			Type type = null;
			if (mod == null || !mod.BehaviourTypes.TryGetValue(attachment.Behaviour, out type))
			{
				type = EngineBehaviour(attachment.Behaviour);
			}
			if (type == null)
			{
				Game.Warn("mod " + (mod?.Id ?? "?") + ": scenes/" + map + ".json names behaviour " + attachment.Behaviour + ", which neither the mod's code nor the engine has");
				return null;
			}
			Behaviour behaviour = null;
			Game.Guard("new " + type.Name, () => behaviour = (Behaviour)Activator.CreateInstance(type));
			if (behaviour == null) return null;
			SetFields(behaviour, attachment.Fields, mod?.Id ?? "scene");
			target.AddComponent(behaviour);
			return behaviour;
		}

		/// <summary>A GameObject from a definition: name, tags, the local transform, the MapObject link; not yet in a scene.</summary>
		internal static GameObject Build(string map, string path, string name, SceneObject item)
		{
			float scale = item.Scale <= 0 ? 1f : item.Scale;
			GameObject o = new GameObject(map + "/" + path);
			o.Tags.Add("scene");
			foreach (string tag in item.Tags ?? new List<string>()) if (!string.IsNullOrWhiteSpace(tag)) o.Tags.Add(tag.Trim());
			o.Transform.Position = new Vector3(item.X, item.Y, item.Z);
			o.Transform.Rotation = new Vector3(0, item.Yaw, 0);
			o.Transform.Scale = new Vector3(scale, scale, scale);
			o.AddComponent(new MapObject { Kind = "scene", Name = name, Path = path, Map = map, Model = string.IsNullOrWhiteSpace(item.Model) ? null : item.Model.Trim(), Character = item.Character, Plain = item.Plain });
			return o;
		}

		/// <summary>o and w models are the game's map objects (chests, signs, crates); the rest are people and creatures.</summary>
		internal static bool IsObjectModel(string model)
		{
			char first = string.IsNullOrEmpty(model) ? ' ' : char.ToLowerInvariant(model[0]);
			return first == 'o' || first == 'w';
		}

		/// <summary>The model for a scene object that has one: a plain character without a cast, following the transform, gone with the object. Components waiting for the character (INeedsNpc) hear of it.</summary>
		/// <summary>Whether a model name is a file of the mod's own (glTF) rather than one of the game's models.</summary>
		public static bool IsMeshFile(string model) => model != null && (model.EndsWith(".glb", StringComparison.OrdinalIgnoreCase) || model.EndsWith(".gltf", StringComparison.OrdinalIgnoreCase));

		internal static void SpawnModel(Modding.LoadedMod mod, string map, GameObject o)
		{
			MapObject link = o.GetComponent<MapObject>();
			if (link == null || link.Model == null || link.Npc != null) return;
			// A model of the mod's own (a glTF in its assets): drawn by the client, no character behind it.
			if (IsMeshFile(link.Model))
			{
				if (o.GetComponent<Mesh>() == null) o.AddComponent(new Mesh { Path = link.Model });
				return;
			}
			Game.Guard("scene object " + link.Path + " model " + link.Model, () =>
			{
				// A character has the walker behind it (turns to the player, can wander, is talked to
				// the game's way) - the scripts' bootCharacter kind, or their bootPlainCharacter kind
				// (Plain: the model's own scale and kind); a plain figure is just the model standing there.
				// An object model (o000, o001, w...) is one of the game's map objects whichever way:
				// that is where a chest's lid motions and its opening come from. The character
				// tick is about people.
				bool asCharacter = link.Character || IsObjectModel(link.Model);
				link.Npc = asCharacter
					? (link.Plain && !IsObjectModel(link.Model) ? Game.Npcs.SpawnPlain(link.Model, o.Transform.WorldPosition, o.Transform.WorldYaw) : Game.Npcs.Spawn(link.Model, o.Transform.WorldPosition, o.Transform.WorldYaw))
					: Game.Npcs.SpawnModel(link.Model, o.Transform.WorldPosition, o.Transform.WorldYaw, o.Transform.WorldScale);
				if (link.Npc != null && asCharacter && !link.Plain && o.Transform.WorldScale != 1f) link.Npc.Scale = o.Transform.WorldScale;
				link.OwnsNpc = link.Npc != null;
			});
			if (link.Npc == null)
			{
				Game.Warn("mod " + (mod?.Id ?? "?") + ": scenes/" + map + ".json: the model " + link.Model + " for '" + link.Path + "' did not spawn");
				return;
			}
			if (o.GetComponent<ModelFollow>() == null) o.AddComponent(new ModelFollow());
			foreach (INeedsNpc waiting in o.Components.OfType<INeedsNpc>().ToArray())
			{
				Game.Guard(o.Name + " npc ready", () => waiting.NpcReady(link));
			}
		}

		private static GameObject MakeTarget(Modding.LoadedMod mod, string map, string key)
		{
			string kind = key;
			int index = -1;
			int colon = key.IndexOf(':');
			if (colon > 0)
			{
				kind = key.Substring(0, colon);
				if (!int.TryParse(key.Substring(colon + 1), out index)) index = -1;
			}
			if (kind != "map" && kind != "object" && kind != "exit")
			{
				Game.Warn("mod " + mod.Id + ": scenes/" + map + ".json attaches to '" + key + "', which is not among its objects (use map, object:<index>, exit:<slot>, or an object's path)");
				return null;
			}
			GameObject o = Game.World.Legacy.Add(map + "/" + key);
			o.Owner = mod;
			o.Tags.Add("scene");
			o.Tags.Add(kind);
			MapObject link = new MapObject { Kind = kind, Index = index, Map = map };
			if (kind == "object" && index >= 0 && ResolveNpc != null)
			{
				Game.Guard("scene npc " + key, () => link.Npc = ResolveNpc(kind, index));
				if (link.Npc != null)
				{
					o.Transform.Position = link.Npc.Position;
				}
			}
			o.AddComponent(link);
			return o;
		}

		/// <summary>Public fields (and settable properties) by name, from JSON: numbers, booleans, strings, enums, Vector3 ({x,y,z} or [x,y,z]), Color ({r,g,b,a} or "#rrggbb").</summary>
		public static void SetFields(object target, Dictionary<string, JsonElement> fields, string modId)
		{
			if (target == null || fields == null) return;
			Type type = target.GetType();
			foreach (KeyValuePair<string, JsonElement> pair in fields)
			{
				try
				{
					FieldInfo field = type.GetField(pair.Key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
					if (field != null && !field.IsInitOnly)
					{
						field.SetValue(target, Convert(pair.Value, field.FieldType));
						continue;
					}
					PropertyInfo property = type.GetProperty(pair.Key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
					if (property != null && property.CanWrite)
					{
						property.SetValue(target, Convert(pair.Value, property.PropertyType));
						continue;
					}
					Game.Warn("mod " + modId + ": " + type.Name + " has no field " + pair.Key);
				}
				catch (Exception ex)
				{
					Game.Warn("mod " + modId + ": " + type.Name + "." + pair.Key + ": " + ex.Message);
				}
			}
		}

		private static object Convert(JsonElement value, Type type)
		{
			if (type == typeof(string)) return value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();
			if (type == typeof(bool)) return value.ValueKind == JsonValueKind.True || (value.ValueKind == JsonValueKind.String && bool.TryParse(value.GetString(), out bool b) && b) || (value.ValueKind == JsonValueKind.Number && value.GetDouble() != 0);
			if (type.IsEnum)
			{
				if (value.ValueKind == JsonValueKind.Number) return Enum.ToObject(type, value.GetInt64());
				return Enum.Parse(type, value.GetString(), ignoreCase: true);
			}
			if (type == typeof(int)) return value.ValueKind == JsonValueKind.String ? int.Parse(value.GetString()) : (int)value.GetDouble();
			if (type == typeof(long)) return value.ValueKind == JsonValueKind.String ? long.Parse(value.GetString()) : (long)value.GetDouble();
			if (type == typeof(float)) return value.ValueKind == JsonValueKind.String ? float.Parse(value.GetString(), System.Globalization.CultureInfo.InvariantCulture) : (float)value.GetDouble();
			if (type == typeof(double)) return value.ValueKind == JsonValueKind.String ? double.Parse(value.GetString(), System.Globalization.CultureInfo.InvariantCulture) : value.GetDouble();
			if (type == typeof(Vector3))
			{
				if (value.ValueKind == JsonValueKind.Array)
				{
					float[] a = value.EnumerateArray().Select(e => (float)e.GetDouble()).ToArray();
					return new Vector3(a.Length > 0 ? a[0] : 0, a.Length > 1 ? a[1] : 0, a.Length > 2 ? a[2] : 0);
				}
				return new Vector3(Number(value, "x"), Number(value, "y"), Number(value, "z"));
			}
			if (type == typeof(Vector2))
			{
				if (value.ValueKind == JsonValueKind.Array)
				{
					float[] a = value.EnumerateArray().Select(e => (float)e.GetDouble()).ToArray();
					return new Vector2(a.Length > 0 ? a[0] : 0, a.Length > 1 ? a[1] : 0);
				}
				return new Vector2(Number(value, "x"), Number(value, "y"));
			}
			if (type == typeof(Color))
			{
				if (value.ValueKind == JsonValueKind.String)
				{
					string s = value.GetString().TrimStart('#');
					if (s.Length >= 6)
					{
						byte r = System.Convert.ToByte(s.Substring(0, 2), 16), g = System.Convert.ToByte(s.Substring(2, 2), 16), bl = System.Convert.ToByte(s.Substring(4, 2), 16);
						byte a = s.Length >= 8 ? System.Convert.ToByte(s.Substring(6, 2), 16) : (byte)255;
						return new Color(r, g, bl, a);
					}
				}
				return new Color((byte)Number(value, "r"), (byte)Number(value, "g"), (byte)Number(value, "b"), value.TryGetProperty("a", out JsonElement alpha) ? (byte)alpha.GetDouble() : (byte)255);
			}
			if (type == typeof(string[]) || type == typeof(List<string>))
			{
				List<string> list = value.ValueKind == JsonValueKind.Array ? value.EnumerateArray().Select(e => e.ToString()).ToList() : new List<string> { value.ToString() };
				return type == typeof(string[]) ? (object)list.ToArray() : list;
			}
			if (type == typeof(ObjectRef))
			{
				// The editor writes the path as a string; {"path": ...} reads too.
				if (value.ValueKind == JsonValueKind.String) return new ObjectRef(value.GetString());
				if (value.ValueKind == JsonValueKind.Object && value.TryGetProperty("path", out JsonElement p)) return new ObjectRef(p.GetString());
				return new ObjectRef();
			}
			return JsonSerializer.Deserialize(value.GetRawText(), type);
		}

		private static float Number(JsonElement o, string name)
		{
			return o.ValueKind == JsonValueKind.Object && o.TryGetProperty(name, out JsonElement e) && e.ValueKind == JsonValueKind.Number ? (float)e.GetDouble() : 0f;
		}
	}
}
