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
		public float X, Y, Z;
		public Vector3(float x, float y, float z) { X = x; Y = y; Z = z; }
		public static readonly Vector3 Zero = new Vector3(0, 0, 0);
		public static readonly Vector3 One = new Vector3(1, 1, 1);
		public static Vector3 operator +(Vector3 a, Vector3 b) => new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
		public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
		public static Vector3 operator *(Vector3 a, float k) => new Vector3(a.X * k, a.Y * k, a.Z * k);
		public static Vector3 operator *(float k, Vector3 a) => new Vector3(a.X * k, a.Y * k, a.Z * k);
		public static Vector3 operator /(Vector3 a, float k) => new Vector3(a.X / k, a.Y / k, a.Z / k);
		public static Vector3 operator -(Vector3 a) => new Vector3(-a.X, -a.Y, -a.Z);
		public static bool operator ==(Vector3 a, Vector3 b) => a.X == b.X && a.Y == b.Y && a.Z == b.Z;
		public static bool operator !=(Vector3 a, Vector3 b) => !(a == b);
		public override bool Equals(object obj) => obj is Vector3 v && v == this;
		public override int GetHashCode() => X.GetHashCode() ^ (Y.GetHashCode() << 2) ^ (Z.GetHashCode() >> 2);
		public float Length => (float)Math.Sqrt(X * X + Y * Y + Z * Z);
		/// <summary>The same direction, length one (Zero stays Zero).</summary>
		public Vector3 Normalized { get { float l = Length; return l > 1e-6f ? this / l : Zero; } }
		/// <summary>The point without its height.</summary>
		public Vector3 Flat => new Vector3(X, 0, Z);
		public static float Distance(Vector3 a, Vector3 b) => (a - b).Length;
		/// <summary>Distance on the ground, ignoring height.</summary>
		public static float FlatDistance(Vector3 a, Vector3 b)
		{
			float dx = a.X - b.X, dz = a.Z - b.Z;
			return (float)Math.Sqrt(dx * dx + dz * dz);
		}
		public static float Dot(Vector3 a, Vector3 b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;
		public static Vector3 Cross(Vector3 a, Vector3 b) => new Vector3(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);
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
		public override string ToString() => X.ToString("0.##") + ", " + Y.ToString("0.##") + ", " + Z.ToString("0.##");
	}

	/// <summary>A point on the screen (800x480 units) or any pair.</summary>
	public struct Vector2
	{
		public float X, Y;
		public Vector2(float x, float y) { X = x; Y = y; }
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
		public override int GetHashCode() => X.GetHashCode() ^ (Y.GetHashCode() << 2);
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

	public sealed class Transform
	{
		public Vector3 Position = Vector3.Zero;
		/// <summary>Euler degrees.</summary>
		public Vector3 Rotation = Vector3.Zero;
		public Vector3 Scale = Vector3.One;
	}

	public abstract class Component
	{
		public GameObject GameObject { get; internal set; }
		public Transform Transform => GameObject?.Transform;
		public Scene Scene => GameObject?.Scene;
		public T GetComponent<T>() where T : Component => GameObject?.GetComponent<T>();
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

		public long Id { get; }
		public string Name { get; set; }
		public HashSet<string> Tags { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		public Transform Transform { get; } = new Transform();
		public bool Active { get; set; } = true;
		public Scene Scene { get; internal set; }
		public GameObject Parent { get; private set; }
		public IReadOnlyList<GameObject> Children => _children;
		public IReadOnlyList<Component> Components => _components;
		/// <summary>The mod that created this object, or null.</summary>
		public Modding.LoadedMod Owner { get; set; }
		internal bool Destroyed;

		public GameObject(string name)
		{
			Id = _nextId++;
			Name = name ?? "GameObject";
		}

		public bool ActiveInHierarchy => Active && !Destroyed && (Parent == null || Parent.ActiveInHierarchy);

		public T AddComponent<T>() where T : Component, new()
		{
			T component = new T();
			AddComponent(component);
			return component;
		}

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

		public T GetComponent<T>() where T : Component => _components.OfType<T>().FirstOrDefault();
		public IEnumerable<T> GetComponents<T>() where T : Component => _components.OfType<T>();

		public void RemoveComponent(Component component)
		{
			if (component == null || !_components.Remove(component)) return;
			if (component is Behaviour behaviour && Scene != null)
			{
				behaviour.RunDestroy();
			}
			component.GameObject = null;
		}

		public void SetParent(GameObject parent)
		{
			Parent?._children.Remove(this);
			Parent = parent;
			parent?._children.Add(this);
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
			Scene = null;
		}
	}

	public sealed class Scene
	{
		private readonly List<GameObject> _roots = new List<GameObject>();

		public string Name { get; }
		/// <summary>What the host knows about the legacy map this scene stands for, when it does.</summary>
		public SceneInfo Info { get; set; }
		public IReadOnlyList<GameObject> Roots => _roots;

		public Scene(string name)
		{
			Name = name;
		}

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

		public GameObject Add(string name)
		{
			return Add(new GameObject(name));
		}

		public void Destroy(GameObject gameObject)
		{
			if (gameObject == null || gameObject.Destroyed) return;
			gameObject.Detached();
			gameObject.SetParent(null);
			_roots.Remove(gameObject);
		}

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

		public IEnumerable<GameObject> WithTag(string tag) => All().Where(o => o.Tags.Contains(tag));
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
