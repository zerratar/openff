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
			bool inside = Vector3.FlatDistance(Transform.Position, Game.Hero.Position) <= Radius;
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
	public abstract class Interactable : Behaviour
	{
		/// <summary>Without a model to talk to: how close the hero comes, in world units, for the object to act.</summary>
		[Header("Without a model")]
		[Tooltip("Without a model to talk to: how close the hero comes for it to act (two characters side by side are about 8 apart)")]
		public float Radius = 8f;

		private bool _near;
		private Npc _npc;

		/// <summary>The object's character, when it has a model.</summary>
		protected Npc Npc => _npc;

		/// <summary>The player talked to it, or walked into it: what the component does.</summary>
		protected abstract void Activate();

		protected override void Start()
		{
			MapObject link = GetComponent<MapObject>();
			_npc = link?.Npc;
			if (_npc != null)
			{
				_npc.Interacted += OnInteracted;
			}
		}

		private void OnInteracted(Npc npc)
		{
			if (Enabled && GameObject != null && GameObject.ActiveInHierarchy) Game.Guard(Name + ".Activate", Activate);
		}

		protected override void Update()
		{
			if (_npc != null || Transform == null || !Game.Hero.Present) return;
			bool near = Vector3.FlatDistance(Transform.Position, Game.Hero.Position) <= Radius;
			if (near && !_near) Game.Guard(Name + ".Activate", Activate);
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
	/// a model it opens when the hero walks in. Derive and override OnOpened to add to it.
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
		/// <summary>What the window says on opening; {what} is the contents ("Potion x2 and 100 gil").</summary>
		[Tooltip("What the window says; {what} is the contents (\"Potion x2 and 100 gil\")")]
		public string Message = "Found {what}!";
		/// <summary>What the window says when it is already open.</summary>
		[Tooltip("What the window says when it is already open")]
		public string EmptyMessage = "The chest is empty.";

		/// <summary>Whether it has been opened (this visit, or ever when Once).</summary>
		public bool Opened { get; private set; }

		private string Key => GameObject?.Name;

		protected override void Start()
		{
			base.Start();
			if (Once && SceneMemory.Instance.Has(Key)) Opened = true;
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
			string what = got.Count == 0 ? "nothing" : string.Join(" and ", got);
			if (!string.IsNullOrEmpty(Message)) Game.Dialogue.Say(Message.Replace("{what}", what));
			Game.Log("chest " + Key + ": " + what);
			OnOpened(what);
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

		private int _next = -1;
		private long _saidAt;

		protected override void Activate()
		{
			if (Lines == null || Lines.Length == 0 || _next >= 0) return;
			if (FaceHero && Npc != null) Npc.LookAt(Game.Hero.Position);
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
	/// Takes the game's own character this is attached to off the map when the map is
	/// entered - what Crystal's "Convert to OpenFF object" leaves on the original, so the
	/// mod's object stands in its place. The map's script still has the cast; nothing
	/// talks to it any more. Nothing happens on a scene object of the mod's own.
	/// </summary>
	public sealed class Removed : Behaviour
	{
		/// <summary>Hide it instead of removing it (it still blocks and can be talked to); off by default.</summary>
		public bool HideOnly;

		protected override void Start()
		{
			MapObject link = GetComponent<MapObject>();
			if (link == null || link.Kind != "object" || link.Npc == null) return;
			if (HideOnly) link.Npc.Hidden = true;
			else Game.Guard("removed " + link, link.Npc.Remove);
			Game.Log("removed: the map's " + link + " (replaced by the mod)");
		}
	}

	/// <summary>The mod's own object in a scene file: a spot, or a model standing there, with children under it.</summary>
	public sealed class SceneObject
	{
		public string Name { get; set; }
		/// <summary>Position; for a child, relative to its parent (turned by the parent's yaw, scaled by its scale).</summary>
		public float X { get; set; }
		public float Y { get; set; }
		public float Z { get; set; }
		/// <summary>Facing in degrees, the engine's yaw (0 = +Z, 90 = +X); for a child, added to the parent's.</summary>
		public float Yaw { get; set; }
		/// <summary>Uniform scale, 1 = the model's own size; for a child, multiplied by the parent's.</summary>
		public float Scale { get; set; } = 1f;
		/// <summary>A model name (o001, n011...) to show, or null for a spot with logic only.</summary>
		public string Model { get; set; }
		public List<string> Tags { get; set; } = new List<string>();
		public List<SceneObject> Children { get; set; } = new List<SceneObject>();
	}

	/// <summary>One attachment in a scene file.</summary>
	public sealed class SceneAttachment
	{
		public string Target { get; set; }
		public string Behaviour { get; set; }
		public Dictionary<string, JsonElement> Fields { get; set; }
	}

	/// <summary>A spot placed in the editor: a spawn point, a camera mark, a trigger centre - whatever a mod makes of it.</summary>
	public sealed class ScenePoint
	{
		public string Name { get; set; }
		public float X { get; set; }
		public float Y { get; set; }
		public float Z { get; set; }
		/// <summary>Facing in degrees, the engine's yaw (0 = +Z, 90 = +X).</summary>
		public float Yaw { get; set; }
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
			foreach (SceneObject root in file.AllObjects())
			{
				made += Place(mod, map, scene, root, null, null, 0f, 1f, Vector3.Zero, objects);
			}
			foreach (SceneAttachment attachment in file.Attachments ?? new List<SceneAttachment>())
			{
				if (attachment == null || string.IsNullOrEmpty(attachment.Behaviour) || string.IsNullOrEmpty(attachment.Target)) continue;
				if (!mod.BehaviourTypes.TryGetValue(attachment.Behaviour, out Type type))
				{
					type = EngineBehaviour(attachment.Behaviour);
				}
				if (type == null)
				{
					Game.Warn("mod " + mod.Id + ": scenes/" + map + ".json names behaviour " + attachment.Behaviour + ", which neither the mod's code nor the engine has");
					continue;
				}
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
				Behaviour behaviour = null;
				Game.Guard("new " + type.Name, () => behaviour = (Behaviour)Activator.CreateInstance(type));
				if (behaviour == null) continue;
				SetFields(behaviour, attachment.Fields, mod.Id);
				// Fields first, then the component: Awake sees them.
				target.AddComponent(behaviour);
			}
			if (made > 0)
			{
				Game.Log("engine: mod " + mod.Id + ": " + made + " object(s) from scenes/" + map + ".json");
			}
			return made;
		}

		/// <summary>
		/// One scene object and its children as GameObjects. A child's transform is relative
		/// to its parent's: its offset turned by the parent's yaw and scaled by the parent's
		/// scale, its yaw added, its scale multiplied. Transform has no hierarchy of its own,
		/// so the world values are what the objects get; GameObject.Parent holds the tree.
		/// </summary>
		private static int Place(Modding.LoadedMod mod, string map, Scene scene, SceneObject item, GameObject parent, string parentPath,
			float parentYaw, float parentScale, Vector3 parentAt, Dictionary<string, GameObject> objects)
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
			float scale = item.Scale <= 0 ? 1f : item.Scale;
			float worldScale = parentScale * scale;
			float worldYaw = parentYaw + item.Yaw;
			// The offset in the parent's frame: yaw 0 looks along +z, 90 along +x.
			float a = parentYaw * (float)Math.PI / 180f;
			float c = (float)Math.Cos(a), s = (float)Math.Sin(a);
			float ox = item.X * parentScale, oy = item.Y * parentScale, oz = item.Z * parentScale;
			Vector3 at = parent == null
				? new Vector3(item.X, item.Y, item.Z)
				: new Vector3(parentAt.X + c * ox + s * oz, parentAt.Y + oy, parentAt.Z - s * ox + c * oz);

			GameObject o = new GameObject(map + "/" + path);
			o.Owner = mod;
			o.Tags.Add("scene");
			foreach (string tag in item.Tags ?? new List<string>()) if (!string.IsNullOrWhiteSpace(tag)) o.Tags.Add(tag.Trim());
			o.Transform.Position = at;
			o.Transform.Rotation = new Vector3(0, worldYaw, 0);
			o.Transform.Scale = new Vector3(worldScale, worldScale, worldScale);
			MapObject link = new MapObject { Kind = "scene", Name = name, Path = path, Map = map, Model = string.IsNullOrWhiteSpace(item.Model) ? null : item.Model.Trim() };
			o.AddComponent(link);
			if (parent != null) o.SetParent(parent);
			scene.Add(o);
			if (link.Model != null)
			{
				// The model, standing there with nothing of the game's behind it: a plain
				// character without a cast. It goes when the object does (OnDetached).
				Game.Guard("scene object " + path + " model " + link.Model, () =>
				{
					link.Npc = Game.Npcs.SpawnModel(link.Model, at, worldYaw, worldScale);
					link.OwnsNpc = link.Npc != null;
				});
				if (link.Npc == null)
				{
					Game.Warn("mod " + mod.Id + ": scenes/" + map + ".json: the model " + link.Model + " for '" + path + "' did not spawn");
				}
			}
			objects[key] = o;
			int made = 1;
			foreach (SceneObject child in item.Children ?? new List<SceneObject>())
			{
				made += Place(mod, map, scene, child, o, path, worldYaw, worldScale, at, objects);
			}
			return made;
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
			return JsonSerializer.Deserialize(value.GetRawText(), type);
		}

		private static float Number(JsonElement o, string name)
		{
			return o.ValueKind == JsonValueKind.Object && o.TryGetProperty(name, out JsonElement e) && e.ValueKind == JsonValueKind.Number ? (float)e.GetDouble() : 0f;
		}
	}
}
