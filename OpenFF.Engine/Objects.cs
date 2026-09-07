// The object model: scenes of game objects carrying components.
//
// This is the Unity shape, sized for us: a GameObject has a name, a stable id, tags, an
// active flag, a Transform and components; a Component is typed data on an object; a
// Behaviour is a component with a lifecycle. Nothing else lives on an object.
//
// Today the World has one scene, "legacy", standing for whatever map the decompiled game
// is running - the host keeps its SceneInfo current. Behaviours a mod spawns there run
// their Update alongside the legacy game. Serialisation of scenes (our scene format) and
// the engine's own components (renderer, collider, exit...) come with the scene-format
// step; the lifecycle below is what they will attach to.

using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenFF
{
	public struct Vector3
	{
		/// <summary>The components; x right, y up, z forward (0 yaw looks along +Z).</summary>
		public float X, Y, Z;
		public Vector3(float x, float y, float z) { X = x; Y = y; Z = z; }
		/// <summary>All zeros.</summary>
		public static readonly Vector3 Zero = new Vector3(0, 0, 0);
		/// <summary>All ones.</summary>
		public static readonly Vector3 One = new Vector3(1, 1, 1);
		public static Vector3 operator +(Vector3 a, Vector3 b) => new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
		public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
		public static Vector3 operator *(Vector3 a, float k) => new Vector3(a.X * k, a.Y * k, a.Z * k);
		public static Vector3 operator *(float k, Vector3 a) => new Vector3(a.X * k, a.Y * k, a.Z * k);
		public static Vector3 operator /(Vector3 a, float k) => new Vector3(a.X / k, a.Y / k, a.Z / k);
		public static Vector3 operator -(Vector3 a) => new Vector3(-a.X, -a.Y, -a.Z);
		public static bool operator ==(Vector3 a, Vector3 b) => a.X == b.X && a.Y == b.Y && a.Z == b.Z;
		public static bool operator !=(Vector3 a, Vector3 b) => !(a == b);
		/// <summary>Component-wise equality.</summary>
		public override bool Equals(object obj) => obj is Vector3 v && v == this;
		/// <summary>A hash of the three components.</summary>
		public override int GetHashCode() => X.GetHashCode() ^ (Y.GetHashCode() << 2) ^ (Z.GetHashCode() >> 2);
		/// <summary>How long the vector is.</summary>
		public float Length => (float)Math.Sqrt(X * X + Y * Y + Z * Z);
		/// <summary>The same direction, length one (Zero stays Zero).</summary>
		public Vector3 Normalized { get { float l = Length; return l > 1e-6f ? this / l : Zero; } }
		/// <summary>The point without its height.</summary>
		public Vector3 Flat => new Vector3(X, 0, Z);
		/// <summary>The straight-line distance between two points.</summary>
		public static float Distance(Vector3 a, Vector3 b) => (a - b).Length;
		/// <summary>Distance on the ground, ignoring height.</summary>
		public static float FlatDistance(Vector3 a, Vector3 b)
		{
			float dx = a.X - b.X, dz = a.Z - b.Z;
			return (float)Math.Sqrt(dx * dx + dz * dz);
		}
		/// <summary>The dot product.</summary>
		public static float Dot(Vector3 a, Vector3 b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;
		/// <summary>The cross product: a vector at right angles to both.</summary>
		public static Vector3 Cross(Vector3 a, Vector3 b) => new Vector3(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);
		/// <summary>The point t of the way from a to b (0 is a, 1 is b).</summary>
		public static Vector3 Lerp(Vector3 a, Vector3 b, float t) => a + (b - a) * t;
		/// <summary>A step of at most maxStep from a toward b.</summary>
		public static Vector3 MoveToward(Vector3 a, Vector3 b, float maxStep)
		{
			Vector3 d = b - a; float l = d.Length;
			return l <= maxStep || l < 1e-6f ? b : a + d * (maxStep / l);
		}
		/// <summary>A direction on the ground from a yaw in degrees (0 = +Z, 90 = +X), as Hero.Yaw and Npc.Yaw report it.</summary>
		public static Vector3 FromYaw(float degrees)
		{
			double r = degrees * Math.PI / 180.0;
			return new Vector3((float)Math.Sin(r), 0, (float)Math.Cos(r));
		}
		/// <summary>The yaw in degrees of a ground direction (0 = +Z, 90 = +X).</summary>
		public float Yaw => (float)(Math.Atan2(X, Z) * 180.0 / Math.PI);
		/// <summary>"x, y, z" to two decimals.</summary>
		public override string ToString() => X.ToString("0.##") + ", " + Y.ToString("0.##") + ", " + Z.ToString("0.##");
	}

	/// <summary>A point on the screen (800x480 units) or any pair.</summary>
	public struct Vector2
	{
		/// <summary>The components: x right, y up on screen (or whatever the pair stands for).</summary>
		public float X, Y;
		public Vector2(float x, float y) { X = x; Y = y; }
		/// <summary>Both zero.</summary>
		public static readonly Vector2 Zero = new Vector2(0, 0);
		public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.X + b.X, a.Y + b.Y);
		public static Vector2 operator -(Vector2 a, Vector2 b) => new Vector2(a.X - b.X, a.Y - b.Y);
		public static Vector2 operator *(Vector2 a, float k) => new Vector2(a.X * k, a.Y * k);
		public static Vector2 operator *(float k, Vector2 a) => new Vector2(a.X * k, a.Y * k);
		public static Vector2 operator /(Vector2 a, float k) => new Vector2(a.X / k, a.Y / k);
		public static Vector2 operator -(Vector2 a) => new Vector2(-a.X, -a.Y);
		public static bool operator ==(Vector2 a, Vector2 b) => a.X == b.X && a.Y == b.Y;
		public static bool operator !=(Vector2 a, Vector2 b) => !(a == b);
		public override bool Equals(object obj) => obj is Vector2 v && v == this;
		/// <summary>A hash of the three components.</summary>
		public override int GetHashCode() => X.GetHashCode() ^ (Y.GetHashCode() << 2);
		/// <summary>How long the vector is.</summary>
		public float Length => (float)Math.Sqrt(X * X + Y * Y);
		public Vector2 Normalized { get { float l = Length; return l > 1e-6f ? this / l : Zero; } }
		public static float Distance(Vector2 a, Vector2 b) => (a - b).Length;
		public static float Dot(Vector2 a, Vector2 b) => a.X * b.X + a.Y * b.Y;
		public static Vector2 Lerp(Vector2 a, Vector2 b, float t) => a + (b - a) * t;
		/// <summary>A direction from an angle in degrees (0 = +X, 90 = +Y, screen down).</summary>
		public static Vector2 FromAngle(float degrees)
		{
			double r = degrees * Math.PI / 180.0;
			return new Vector2((float)Math.Cos(r), (float)Math.Sin(r));
		}
		public float Angle => (float)(Math.Atan2(Y, X) * 180.0 / Math.PI);
		public override string ToString() => X.ToString("0.##") + ", " + Y.ToString("0.##");
	}

	/// <summary>
	/// Where an object is. Position, Rotation and Scale are relative to the parent object
	/// when there is one (Unity's local values) and the world's when there is not, so a
	/// child moves with its parent; WorldPosition, WorldYaw and WorldScale are the resolved
	/// values, readable and settable either way. The hierarchy turns about the vertical
	/// only (Rotation.Y), which is what the field's characters and the scene files do.
	/// </summary>
	public sealed class Transform
	{
		/// <summary>Relative to the parent's frame: turned by its yaw, scaled by its scale.</summary>
		public Vector3 Position = Vector3.Zero;
		/// <summary>Euler degrees; Y is the yaw (0 = +Z, 90 = +X), added to the parent's.</summary>
		public Vector3 Rotation = Vector3.Zero;
		/// <summary>Multiplied by the parent's.</summary>
		public Vector3 Scale = Vector3.One;

		internal GameObject Owner;

		private Transform ParentTransform => Owner?.Parent?.Transform;

		/// <summary>The position in the world, the parents' transforms applied; setting it keeps the object where you say and works the local value out.</summary>
		public Vector3 WorldPosition
		{
			get
			{
				Transform p = ParentTransform;
				if (p == null) return Position;
				Vector3 at = p.WorldPosition;
				float k = p.WorldScale;
				double a = p.WorldYaw * Math.PI / 180.0;
				float c = (float)Math.Cos(a), s = (float)Math.Sin(a);
				float ox = Position.X * k, oy = Position.Y * k, oz = Position.Z * k;
				return new Vector3(at.X + c * ox + s * oz, at.Y + oy, at.Z - s * ox + c * oz);
			}
			set
			{
				Transform p = ParentTransform;
				if (p == null) { Position = value; return; }
				Vector3 at = p.WorldPosition;
				float k = p.WorldScale;
				if (k == 0) k = 1;
				double a = p.WorldYaw * Math.PI / 180.0;
				float c = (float)Math.Cos(a), s = (float)Math.Sin(a);
				float dx = value.X - at.X, dy = value.Y - at.Y, dz = value.Z - at.Z;
				Position = new Vector3((c * dx - s * dz) / k, dy / k, (s * dx + c * dz) / k);
			}
		}

		/// <summary>The yaw in the world, in degrees: the parents' added to Rotation.Y.</summary>
		public float WorldYaw
		{
			get { Transform p = ParentTransform; return p == null ? Rotation.Y : p.WorldYaw + Rotation.Y; }
			set { Transform p = ParentTransform; Rotation = new Vector3(Rotation.X, p == null ? value : value - p.WorldYaw, Rotation.Z); }
		}

		/// <summary>The uniform scale in the world: the parents' multiplied by Scale.X.</summary>
		public float WorldScale
		{
			get { Transform p = ParentTransform; return p == null ? Scale.X : p.WorldScale * Scale.X; }
			set { Transform p = ParentTransform; float k = p == null ? value : (p.WorldScale == 0 ? value : value / p.WorldScale); Scale = new Vector3(k, k, k); }
		}

		/// <summary>The world direction the object faces, from WorldYaw.</summary>
		public Vector3 Forward => Vector3.FromYaw(WorldYaw);
	}

	public abstract class Component
	{
		/// <summary>The object this component is on; null once removed.</summary>
		public GameObject GameObject { get; internal set; }
		/// <summary>The object's Transform, for short.</summary>
		public Transform Transform => GameObject?.Transform;
		/// <summary>The scene the object is in, for short.</summary>
		public Scene Scene => GameObject?.Scene;
		/// <summary>Another component on the same object, or null.</summary>
		public T GetComponent<T>() where T : Component => GameObject?.GetComponent<T>();

		/// <summary>The object is leaving its scene: a plain component's chance to let go of what it holds (a Behaviour has OnDestroy).</summary>
		internal virtual void OnDetached() { }
	}

	/// <summary>A component with a lifecycle, the unit of script on an object.</summary>
	public abstract class Behaviour : Component
	{
		private bool _enabled = true;
		internal bool Awoken;
		internal bool StartedFlag;

		public bool Enabled
		{
			get => _enabled;
			set
			{
				if (_enabled == value) return;
				_enabled = value;
				if (Awoken && GameObject != null && GameObject.ActiveInHierarchy)
				{
					Game.Guard(Name + (value ? ".OnEnable" : ".OnDisable"), value ? OnEnable : OnDisable);
				}
			}
		}

		/// <summary>Called once, when the object is added to a scene (before Start).</summary>
		protected virtual void Awake() { }
		protected virtual void OnEnable() { }
		/// <summary>Called once, on the first frame the behaviour is active and enabled.</summary>
		protected virtual void Start() { }
		protected virtual void Update() { }
		protected virtual void LateUpdate() { }
		protected virtual void OnDisable() { }
		protected virtual void OnDestroy() { }
		/// <summary>Lines for the debug overlay (F1). Return null for none.</summary>
		public virtual IEnumerable<string> DebugLines() => null;

		/// <summary>Starts a coroutine (Game.Run) named after this behaviour.</summary>
		protected Coroutine StartCoroutine(System.Collections.IEnumerator routine) => Game.Run(routine, Name);

		internal string Name => (GameObject?.Name ?? "?") + "." + GetType().Name;

		internal void RunAwake()
		{
			if (Awoken) return;
			Awoken = true;
			Game.Guard(Name + ".Awake", Awake);
			if (_enabled)
			{
				Game.Guard(Name + ".OnEnable", OnEnable);
			}
		}

		internal void RunUpdate()
		{
			if (!_enabled) return;
			if (!StartedFlag)
			{
				StartedFlag = true;
				Game.Guard(Name + ".Start", Start);
			}
			Game.Guard(Name + ".Update", Update);
		}

		internal void RunLateUpdate()
		{
			if (_enabled && StartedFlag)
			{
				Game.Guard(Name + ".LateUpdate", LateUpdate);
			}
		}

		internal void RunDestroy()
		{
			if (Awoken && _enabled)
			{
				Game.Guard(Name + ".OnDisable", OnDisable);
			}
			Game.Guard(Name + ".OnDestroy", OnDestroy);
		}
	}

	public sealed class GameObject
	{
		private static long _nextId = 1;
		private readonly List<Component> _components = new List<Component>();
		private readonly List<GameObject> _children = new List<GameObject>();

		/// <summary>A number unique to this object for the run.</summary>
		public long Id { get; }
		/// <summary>The name: a scene file's objects are &lt;map&gt;/&lt;path&gt;; find one with Scene.Find.</summary>
		public string Name { get; set; }
		/// <summary>Words to find the object by (Scene.WithTag); case does not matter.</summary>
		public HashSet<string> Tags { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		/// <summary>Where it is: relative to the parent when it has one.</summary>
		public Transform Transform { get; } = new Transform();
		/// <summary>Whether its behaviours run; an inactive parent stops the children too (ActiveInHierarchy).</summary>
		public bool Active { get; set; } = true;
		/// <summary>The scene it is in; null before Add and after Destroy.</summary>
		public Scene Scene { get; internal set; }
		/// <summary>The object above it in the tree, or null at the top.</summary>
		public GameObject Parent { get; private set; }
		/// <summary>The objects under it.</summary>
		public IReadOnlyList<GameObject> Children => _children;
		/// <summary>Every component on it, in the order added.</summary>
		public IReadOnlyList<Component> Components => _components;
		/// <summary>The mod that created this object, or null.</summary>
		public Modding.LoadedMod Owner { get; set; }
		internal bool Destroyed;

		public GameObject(string name)
		{
			Id = _nextId++;
			Name = name ?? "GameObject";
			Transform.Owner = this;
		}

		/// <summary>Active, and every parent active, and not destroyed.</summary>
		public bool ActiveInHierarchy => Active && !Destroyed && (Parent == null || Parent.ActiveInHierarchy);

		/// <summary>Makes a component of that type and adds it (a Behaviour wakes if the object is in a scene).</summary>
		public T AddComponent<T>() where T : Component, new()
		{
			T component = new T();
			AddComponent(component);
			return component;
		}

		/// <summary>Adds a component made elsewhere (its fields set first, so Awake sees them).</summary>
		public Component AddComponent(Component component)
		{
			if (component == null) throw new ArgumentNullException(nameof(component));
			component.GameObject = this;
			_components.Add(component);
			if (Scene != null && component is Behaviour behaviour)
			{
				behaviour.RunAwake();
			}
			return component;
		}

		/// <summary>The first component of that type, or null.</summary>
		public T GetComponent<T>() where T : Component => _components.OfType<T>().FirstOrDefault();
		/// <summary>Every component of that type.</summary>
		public IEnumerable<T> GetComponents<T>() where T : Component => _components.OfType<T>();

		/// <summary>Takes a component off (a Behaviour hears OnDisable and OnDestroy).</summary>
		public void RemoveComponent(Component component)
		{
			if (component == null || !_components.Remove(component)) return;
			if (component is Behaviour behaviour && Scene != null)
			{
				behaviour.RunDestroy();
			}
			component.GameObject = null;
		}

		/// <summary>
		/// Puts the object under another (or at the top with null). With keepWorld, the
		/// default, it stays where it is in the world and its Transform is worked out in
		/// the new parent's frame; without, its Transform is kept as written and it moves.
		/// </summary>
		public void SetParent(GameObject parent, bool keepWorld = true)
		{
			if (parent == this) return;
			for (GameObject p = parent; p != null; p = p.Parent) if (p == this) return; // not under itself
			Vector3 at = Transform.WorldPosition;
			float yaw = Transform.WorldYaw, scale = Transform.WorldScale;
			Parent?._children.Remove(this);
			Parent = parent;
			parent?._children.Add(this);
			if (keepWorld)
			{
				Transform.WorldPosition = at;
				Transform.WorldYaw = yaw;
				Transform.WorldScale = scale;
			}
		}

		internal void Attached(Scene scene)
		{
			Scene = scene;
			foreach (Behaviour behaviour in _components.OfType<Behaviour>().ToArray())
			{
				behaviour.RunAwake();
			}
			foreach (GameObject child in _children.ToArray())
			{
				child.Attached(scene);
			}
		}

		internal void Detached()
		{
			Destroyed = true;
			foreach (GameObject child in _children.ToArray())
			{
				child.Detached();
			}
			foreach (Behaviour behaviour in _components.OfType<Behaviour>().ToArray())
			{
				behaviour.RunDestroy();
			}
			foreach (Component component in _components.Where(c => !(c is Behaviour)).ToArray())
			{
				Game.Guard(Name + "." + component.GetType().Name + ".OnDetached", component.OnDetached);
			}
			Scene = null;
		}
	}

	public sealed class Scene
	{
		private readonly List<GameObject> _roots = new List<GameObject>();

		/// <summary>The scene's name; the legacy map's is "legacy".</summary>
		public string Name { get; }
		/// <summary>What the host knows about the legacy map this scene stands for, when it does.</summary>
		public SceneInfo Info { get; set; }
		/// <summary>The objects at the top of the tree.</summary>
		public IReadOnlyList<GameObject> Roots => _roots;

		public Scene(string name)
		{
			Name = name;
		}

		/// <summary>Puts an object (and its children) into the scene; behaviours wake.</summary>
		public GameObject Add(GameObject gameObject)
		{
			if (gameObject == null) throw new ArgumentNullException(nameof(gameObject));
			if (gameObject.Parent == null && !_roots.Contains(gameObject))
			{
				_roots.Add(gameObject);
			}
			gameObject.Attached(this);
			return gameObject;
		}

		/// <summary>A new, empty object of that name in the scene.</summary>
		public GameObject Add(string name)
		{
			return Add(new GameObject(name));
		}

		/// <summary>Takes an object and its children out; behaviours hear OnDestroy.</summary>
		public void Destroy(GameObject gameObject)
		{
			if (gameObject == null || gameObject.Destroyed) return;
			gameObject.Detached();
			gameObject.SetParent(null);
			_roots.Remove(gameObject);
		}

		/// <summary>Every object in the scene, parents before children.</summary>
		public IEnumerable<GameObject> All()
		{
			foreach (GameObject root in _roots)
			{
				foreach (GameObject o in Walk(root))
				{
					yield return o;
				}
			}
		}

		/// <summary>Every object carrying a tag.</summary>
		public IEnumerable<GameObject> WithTag(string tag) => All().Where(o => o.Tags.Contains(tag));
		/// <summary>The object of that name (case does not matter), or null.</summary>
		public GameObject Find(string name) => All().FirstOrDefault(o => string.Equals(o.Name, name, StringComparison.OrdinalIgnoreCase));

		private static IEnumerable<GameObject> Walk(GameObject o)
		{
			yield return o;
			foreach (GameObject child in o.Children)
			{
				foreach (GameObject c in Walk(child))
				{
					yield return c;
				}
			}
		}

		internal void Update()
		{
			GameObject[] objects = All().Where(o => o.ActiveInHierarchy).ToArray();
			foreach (GameObject o in objects)
			{
				foreach (Behaviour b in o.Components.OfType<Behaviour>().ToArray())
				{
					b.RunUpdate();
				}
			}
			foreach (GameObject o in objects)
			{
				foreach (Behaviour b in o.Components.OfType<Behaviour>().ToArray())
				{
					b.RunLateUpdate();
				}
			}
		}

		internal void Clear()
		{
			foreach (GameObject root in _roots.ToArray())
			{
				Destroy(root);
			}
		}

		/// <summary>Destroys every object a mod created (its code is going away).</summary>
		internal void RemoveOwned(Modding.LoadedMod mod)
		{
			foreach (GameObject o in All().Where(o => o.Owner == mod).ToArray())
			{
				Destroy(o);
			}
			// Components from the mod's assembly on other objects go too.
			foreach (GameObject o in All().ToArray())
			{
				foreach (Component c in o.Components.Where(c => c.GetType().Assembly == mod.Assembly).ToArray())
				{
					o.RemoveComponent(c);
				}
			}
		}
	}

	public sealed class World
	{
		private readonly List<Scene> _scenes = new List<Scene>();

		/// <summary>The scene that stands for the legacy game's current map. Always present.</summary>
		public Scene Legacy { get; }

		public IReadOnlyList<Scene> Scenes => _scenes;

		internal World()
		{
			Legacy = new Scene("legacy");
			_scenes.Add(Legacy);
		}

		public Scene Load(string name)
		{
			Scene scene = new Scene(name);
			_scenes.Add(scene);
			return scene;
		}

		public void Unload(Scene scene)
		{
			if (scene == null || scene == Legacy) return;
			scene.Clear();
			_scenes.Remove(scene);
		}

		public int ObjectCount => _scenes.Sum(s => s.All().Count());

		internal void Update()
		{
			foreach (Scene scene in _scenes.ToArray())
			{
				scene.Update();
			}
		}

		internal void RemoveOwned(Modding.LoadedMod mod)
		{
			foreach (Scene scene in _scenes.ToArray())
			{
				scene.RemoveOwned(mod);
			}
		}
	}
}
