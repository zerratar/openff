// Services: the scripts that live for the whole run.
//
// A GameService is created once, when its mod loads (or by the engine itself), and is
// never destroyed by a scene change. It is the home for anything that must outlive a map:
// a multiplayer host or client, a mod's global state, a quest log, a rule set. The
// engine's own systems will be services of the same shape, so a mod can replace one by
// registering its own implementation of the same interface later in the load order.
//
// Reached through Game.Services.Get<T>(); T may be the concrete type or any interface
// or base class it implements.

using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenFF
{
	public abstract class GameService
	{
		/// <summary>The mod that provided this service, or null for the engine's own.</summary>
		public Modding.LoadedMod Mod { get; internal set; }

		/// <summary>Set to true to receive OnUpdate every frame.</summary>
		public virtual bool WantsUpdate => false;

		/// <summary>Once, when the engine starts - or on registration, if the engine already runs.</summary>
		public virtual void OnGameStart() { }

		/// <summary>Every frame, when WantsUpdate is true.</summary>
		public virtual void OnUpdate() { }

		/// <summary>A scene (a map) is about to be left.</summary>
		public virtual void OnSceneUnloading(SceneInfo scene) { }

		/// <summary>A scene (a map) has been entered.</summary>
		public virtual void OnSceneLoaded(SceneInfo scene) { }

		/// <summary>The game is closing.</summary>
		public virtual void OnQuit() { }

		/// <summary>
		/// Hot reload: this instance is about to be replaced by one built from the mod's new
		/// assembly. Return what the new instance needs to continue (anything JSON can hold),
		/// or null. The default hands over the public fields and properties.
		/// </summary>
		public virtual object OnHandOver() => null;

		/// <summary>
		/// Hot reload: the new instance receives what the old one handed over, as JSON. The
		/// default applies a default hand-over (public fields and properties) by name.
		/// </summary>
		public virtual void OnTakeOver(System.Text.Json.JsonElement state)
		{
			Modding.ModLoader.ApplySnapshot(this, state);
		}

		/// <summary>Lines for the debug overlay (F1). Return null for none.</summary>
		public virtual IEnumerable<string> DebugLines() => null;
	}

	public sealed class ServiceRegistry
	{
		private readonly List<GameService> _services = new List<GameService>();

		public IReadOnlyList<GameService> All => _services;

		/// <summary>
		/// Adds a service. When the engine has already started, the service hears
		/// OnGameStart at once, so a hot-reloaded or late mod behaves like an early one.
		/// </summary>
		public void Register(GameService service)
		{
			if (service == null || _services.Contains(service))
			{
				return;
			}
			_services.Add(service);
			if (Game.Started)
			{
				Game.Guard(Name(service) + ".OnGameStart", service.OnGameStart);
			}
		}

		public void Unregister(GameService service)
		{
			_services.Remove(service);
		}

		/// <summary>The most recently registered service that is a T (so a later mod's replacement wins), or null.</summary>
		public T Get<T>() where T : class
		{
			for (int i = _services.Count - 1; i >= 0; i--)
			{
				if (_services[i] is T match)
				{
					return match;
				}
			}
			return null;
		}

		public IEnumerable<T> GetAll<T>() where T : class => _services.OfType<T>();

		internal void StartAll()
		{
			foreach (GameService service in _services.ToArray())
			{
				Game.Guard(Name(service) + ".OnGameStart", service.OnGameStart);
			}
		}

		internal void UpdateAll()
		{
			foreach (GameService service in _services.ToArray())
			{
				if (service.WantsUpdate)
				{
					Game.Guard(Name(service) + ".OnUpdate", service.OnUpdate);
				}
			}
		}

		/// <summary>Host entry: the legacy scene is being left.</summary>
		public void SceneUnloadingInternal(SceneInfo scene) => SceneUnloading(scene);

		/// <summary>Host entry: the legacy scene has been entered.</summary>
		public void SceneLoadedInternal(SceneInfo scene) => SceneLoaded(scene);

		internal void SceneUnloading(SceneInfo scene)
		{
			foreach (GameService service in _services.ToArray())
			{
				Game.Guard(Name(service) + ".OnSceneUnloading", () => service.OnSceneUnloading(scene));
			}
		}

		internal void SceneLoaded(SceneInfo scene)
		{
			foreach (GameService service in _services.ToArray())
			{
				Game.Guard(Name(service) + ".OnSceneLoaded", () => service.OnSceneLoaded(scene));
			}
		}

		internal void QuitAll()
		{
			for (int i = _services.Count - 1; i >= 0; i--)
			{
				GameService service = _services[i];
				Game.Guard(Name(service) + ".OnQuit", service.OnQuit);
			}
		}

		public static string Name(GameService service)
		{
			return service == null ? "?" : (service.Mod != null ? service.Mod.Id + "/" : "") + service.GetType().Name;
		}
	}
}
