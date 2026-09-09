// Loading mods' code, and reloading it while the game runs.
//
// Each mod's assemblies load into their own collectible AssemblyLoadContext, from bytes
// rather than from the file, so the file stays free for the next build to overwrite.
// The engine assembly itself is shared: a mod's reference to OpenFF.Engine resolves to
// the one the host runs, never to a copy in the mod folder. Isolation here is for
// unloading and versioning, not a sandbox (direction, 2026-09-04: C# mods are code, and that
// is the player's and the modder's business).
//
// Loading a mod means: find its GameService subclasses, make one of each and register
// them (services that are ISaveable register with the save chunks too); remember its
// Behaviour types for scenes to instantiate by name. Unloading undoes all of that, and
// drops the mod's objects and event handlers. A reload is an unload and a load with a
// hand-over: each old service gives the new one its state (by default, its public fields
// and properties as JSON). A change the state cannot survive is reported; the answer is
// a restart, which is expected, not a failure.
//
// The ModWatcher notices a rebuilt assembly and queues the reload; the host drains the
// queue on its main thread, so reloads never race the frame.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using System.Threading;

namespace OpenFF.Modding
{
	/// <summary>What the host tells the engine about a mod whose code is to be loaded.</summary>
	public sealed class ModDefinition
	{
		/// <summary>The mod's id from mod.json.</summary>
		public string Id { get; set; }
		/// <summary>The mod's name from mod.json.</summary>
		public string Name { get; set; }
		/// <summary>The mod's version from mod.json.</summary>
		public string Version { get; set; }
		/// <summary>The mod's folder.</summary>
		public string Directory { get; set; }
		/// <summary>The assemblies to load, full paths, the main one first.</summary>
		public List<string> Assemblies { get; set; } = new List<string>();
		/// <summary>The folder with the mod's scene files (scenes/&lt;map&gt;.json), or null.</summary>
		public string Scenes { get; set; }
	}

	public sealed class LoadedMod
	{
		public string Id => Definition.Id;
		public string Name => Definition.Name ?? Definition.Id;
		public string Version => Definition.Version ?? "";
		public string Directory => Definition.Directory;
		/// <summary>What mod.json said.</summary>
		public ModDefinition Definition { get; internal set; }
		/// <summary>The main assembly.</summary>
		public Assembly Assembly { get; internal set; }
		internal ModLoadContext Context;
		/// <summary>The mod's services, one instance each.</summary>
		public List<GameService> Services { get; } = new List<GameService>();
		/// <summary>The mod's Behaviour types, by simple name, for scenes to instantiate.</summary>
		public Dictionary<string, Type> BehaviourTypes { get; } = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
		/// <summary>How many times its code has been hot-reloaded this run.</summary>
		public int Reloads { get; internal set; }
		/// <summary>When its code was last loaded.</summary>
		public DateTime LoadedAt { get; internal set; }
		public override string ToString() => Id + " " + Version;
	}

	internal sealed class ModLoadContext : AssemblyLoadContext
	{
		private readonly string _directory;
		private readonly Dictionary<string, Assembly> _loaded = new Dictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase);

		public ModLoadContext(string name, string directory)
			: base(name, isCollectible: true)
		{
			_directory = directory;
		}

		/// <summary>Loads an assembly from its bytes (and its .pdb beside it, when there is one), leaving the file unlocked.</summary>
		public Assembly LoadFile(string path)
		{
			string key = Path.GetFileNameWithoutExtension(path);
			if (_loaded.TryGetValue(key, out Assembly already))
			{
				return already;
			}
			byte[] dll = File.ReadAllBytes(path);
			string pdbPath = Path.ChangeExtension(path, ".pdb");
			Assembly assembly;
			using (MemoryStream dllStream = new MemoryStream(dll))
			{
				if (File.Exists(pdbPath))
				{
					using MemoryStream pdbStream = new MemoryStream(File.ReadAllBytes(pdbPath));
					assembly = LoadFromStream(dllStream, pdbStream);
				}
				else
				{
					assembly = LoadFromStream(dllStream);
				}
			}
			_loaded[key] = assembly;
			return assembly;
		}

		protected override Assembly Load(AssemblyName assemblyName)
		{
			// The engine and everything the host already has are shared, never duplicated.
			if (string.Equals(assemblyName.Name, typeof(Game).Assembly.GetName().Name, StringComparison.OrdinalIgnoreCase))
			{
				return typeof(Game).Assembly;
			}
			if (_loaded.TryGetValue(assemblyName.Name, out Assembly already))
			{
				return already;
			}
			// A library the mod brought along.
			string beside = Path.Combine(_directory, assemblyName.Name + ".dll");
			if (File.Exists(beside))
			{
				return LoadFile(beside);
			}
			return null; // the default context: framework and host assemblies
		}
	}

	public static class ModLoader
	{
		private static readonly List<LoadedMod> _loaded = new List<LoadedMod>();

		public static IReadOnlyList<LoadedMod> Loaded => _loaded;

		private static readonly JsonSerializerOptions Json = new JsonSerializerOptions { IncludeFields = true, WriteIndented = false };

		/// <summary>Loads a mod's code and registers what it provides. Null, with the reason logged, when it cannot.</summary>
		public static LoadedMod Load(ModDefinition definition)
		{
			return Load(definition, null);
		}

		private static LoadedMod Load(ModDefinition definition, Dictionary<string, string> handedOver)
		{
			if (definition == null || definition.Assemblies == null || definition.Assemblies.Count == 0)
			{
				return null;
			}
			LoadedMod mod = new LoadedMod { Definition = definition, LoadedAt = DateTime.Now };
			mod.Context = new ModLoadContext("mod:" + definition.Id, definition.Directory);
			try
			{
				List<Assembly> assemblies = new List<Assembly>();
				foreach (string path in definition.Assemblies)
				{
					if (!File.Exists(path))
					{
						Game.Warn("mod " + definition.Id + ": " + path + " is missing");
						continue;
					}
					assemblies.Add(mod.Context.LoadFile(path));
				}
				if (assemblies.Count == 0)
				{
					mod.Context.Unload();
					return null;
				}
				mod.Assembly = assemblies[0];
				foreach (Assembly assembly in assemblies)
				{
					Scan(mod, assembly, handedOver);
				}
			}
			catch (Exception ex)
			{
				Game.Warn("mod " + definition.Id + " failed to load: " + ex.GetType().Name + ": " + ex.Message);
				Unload(mod);
				return null;
			}
			_loaded.Add(mod);
			Game.Log("mod " + definition.Id + " " + definition.Version + ": " + mod.Services.Count + " service(s), "
				+ mod.BehaviourTypes.Count + " behaviour type(s) from " + string.Join(", ", definition.Assemblies.Select(Path.GetFileName)));
			return mod;
		}

		private static void Scan(LoadedMod mod, Assembly assembly, Dictionary<string, string> handedOver)
		{
			Type[] types;
			try
			{
				types = assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException ex)
			{
				types = ex.Types.Where(t => t != null).ToArray();
				foreach (Exception inner in ex.LoaderExceptions.Where(e => e != null).Take(3))
				{
					Game.Warn("mod " + mod.Id + ": " + inner.Message);
				}
			}
			foreach (Type type in types)
			{
				if (type.IsAbstract || type.IsGenericTypeDefinition)
				{
					continue;
				}
				if (typeof(Behaviour).IsAssignableFrom(type))
				{
					mod.BehaviourTypes[type.Name] = type;
				}
				if (typeof(GameService).IsAssignableFrom(type) && type.GetConstructor(Type.EmptyTypes) != null)
				{
					GameService service = null;
					Game.Guard("mod " + mod.Id + ": new " + type.Name, () => service = (GameService)Activator.CreateInstance(type));
					if (service == null)
					{
						continue;
					}
					service.Mod = mod;
					mod.Services.Add(service);
					if (handedOver != null && handedOver.TryGetValue(type.FullName, out string state) && state != null)
					{
						Game.Guard(type.Name + ".OnTakeOver", () => service.OnTakeOver(JsonSerializer.Deserialize<JsonElement>(state)));
					}
					Game.Services.Register(service);
					if (service is ISaveable saveable)
					{
						Game.Saves.Register(saveable);
					}
				}
			}
		}

		/// <summary>Unloads a mod's code: its services, saveables, objects and event handlers go. Returns each service's hand-over state by type name.</summary>
		public static Dictionary<string, string> Unload(LoadedMod mod)
		{
			Dictionary<string, string> states = new Dictionary<string, string>();
			if (mod == null)
			{
				return states;
			}
			foreach (GameService service in mod.Services.ToArray())
			{
				string typeName = service.GetType().FullName;
				Game.Guard(ServiceRegistry.Name(service) + ".OnHandOver", () =>
				{
					object state = service.OnHandOver() ?? Snapshot(service);
					states[typeName] = state == null ? null : JsonSerializer.Serialize(state, state.GetType(), Json);
				});
				if (service is ISaveable saveable)
				{
					Game.Saves.Unregister(saveable);
				}
				Game.Services.Unregister(service);
			}
			mod.Services.Clear();
			if (mod.Assembly != null)
			{
				Game.World.RemoveOwned(mod);
				Game.Events.RemoveFrom(mod.Assembly);
				Game.Coroutines.RemoveFrom(mod.Assembly);
			}
			_loaded.Remove(mod);
			try
			{
				mod.Context?.Unload();
			}
			catch (Exception ex)
			{
				Game.Warn("mod " + mod.Id + ": unload: " + ex.Message);
			}
			mod.Context = null;
			mod.Assembly = null;
			return states;
		}

		/// <summary>Unloads and loads a mod again from its (rebuilt) assemblies, handing each service's state over.</summary>
		public static LoadedMod Reload(LoadedMod mod)
		{
			if (mod == null)
			{
				return null;
			}
			int reloads = mod.Reloads + 1;
			ModDefinition definition = mod.Definition;
			Game.Log("mod " + mod.Id + ": reloading");
			Dictionary<string, string> states = Unload(mod);
			GC.Collect();
			GC.WaitForPendingFinalizers();
			LoadedMod fresh = Load(definition, states);
			if (fresh == null)
			{
				Game.Warn("mod " + definition.Id + ": the new build did not load; the mod is out until the next successful build (or a restart)");
				return null;
			}
			fresh.Reloads = reloads;
			foreach (string typeName in states.Keys.Where(k => fresh.Services.All(s => s.GetType().FullName != k)))
			{
				Game.Log("mod " + definition.Id + ": service " + typeName + " is gone in the new build; its state was dropped");
			}
			Game.Events.Publish(new Events.ModReloaded { ModId = definition.Id });
			return fresh;
		}

		/// <summary>The default hand-over: the public fields and properties, as a dictionary.</summary>
		private static Dictionary<string, object> Snapshot(GameService service)
		{
			Dictionary<string, object> snapshot = new Dictionary<string, object>();
			Type type = service.GetType();
			foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
			{
				snapshot[field.Name] = field.GetValue(service);
			}
			foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
			{
				if (property.CanRead && property.CanWrite && property.GetIndexParameters().Length == 0)
				{
					snapshot[property.Name] = property.GetValue(service);
				}
			}
			return snapshot;
		}

		/// <summary>Applies a default snapshot to a service: each public field or property by name, when the JSON converts to its type.</summary>
		public static void ApplySnapshot(GameService service, JsonElement state)
		{
			if (state.ValueKind != JsonValueKind.Object)
			{
				return;
			}
			Type type = service.GetType();
			foreach (JsonProperty entry in state.EnumerateObject())
			{
				try
				{
					FieldInfo field = type.GetField(entry.Name, BindingFlags.Public | BindingFlags.Instance);
					if (field != null)
					{
						field.SetValue(service, entry.Value.Deserialize(field.FieldType, Json));
						continue;
					}
					PropertyInfo property = type.GetProperty(entry.Name, BindingFlags.Public | BindingFlags.Instance);
					if (property != null && property.CanWrite)
					{
						property.SetValue(service, entry.Value.Deserialize(property.PropertyType, Json));
					}
				}
				catch (Exception ex)
				{
					Game.Warn(type.Name + "." + entry.Name + " did not survive the reload (" + ex.Message + "); a restart gives it its default");
				}
			}
		}
	}

	/// <summary>Watches the loaded mods' assemblies and queues a reload when one is rebuilt.</summary>
	public static class ModWatcher
	{
		private static readonly List<FileSystemWatcher> _watchers = new List<FileSystemWatcher>();
		private static readonly Dictionary<string, DateTime> _pending = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
		private static readonly object _lock = new object();
		private static readonly TimeSpan Settle = TimeSpan.FromMilliseconds(600);

		public static bool Enabled { get; private set; }

		/// <summary>Starts watching every loaded mod's directory for assembly changes.</summary>
		public static void Start()
		{
			Stop();
			foreach (LoadedMod mod in ModLoader.Loaded)
			{
				Watch(mod);
			}
			Enabled = _watchers.Count > 0;
		}

		public static void Watch(LoadedMod mod)
		{
			if (mod?.Directory == null || !Directory.Exists(mod.Directory) || _watchers.Any(w => string.Equals(w.Path, mod.Directory, StringComparison.OrdinalIgnoreCase)))
			{
				return;
			}
			try
			{
				FileSystemWatcher watcher = new FileSystemWatcher(mod.Directory, "*.dll")
				{
					IncludeSubdirectories = false,
					NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size,
				};
				string id = mod.Id;
				FileSystemEventHandler changed = (_, __) => { lock (_lock) { _pending[id] = DateTime.Now; } };
				watcher.Changed += changed;
				watcher.Created += changed;
				watcher.Renamed += (_, __) => { lock (_lock) { _pending[id] = DateTime.Now; } };
				watcher.EnableRaisingEvents = true;
				_watchers.Add(watcher);
			}
			catch (Exception ex)
			{
				Game.Warn("mod " + mod.Id + ": not watching " + mod.Directory + " (" + ex.Message + ")");
			}
		}

		public static void Stop()
		{
			foreach (FileSystemWatcher watcher in _watchers)
			{
				watcher.Dispose();
			}
			_watchers.Clear();
			Enabled = false;
		}

		/// <summary>Called by the host on its main thread each frame: reloads the mods whose files settled.</summary>
		public static void Drain()
		{
			List<string> ready;
			lock (_lock)
			{
				if (_pending.Count == 0)
				{
					return;
				}
				DateTime now = DateTime.Now;
				ready = _pending.Where(p => now - p.Value >= Settle).Select(p => p.Key).ToList();
				foreach (string id in ready)
				{
					_pending.Remove(id);
				}
			}
			foreach (string id in ready)
			{
				LoadedMod mod = ModLoader.Loaded.FirstOrDefault(m => string.Equals(m.Id, id, StringComparison.OrdinalIgnoreCase));
				if (mod != null)
				{
					ModLoader.Reload(mod);
				}
			}
		}
	}
}
