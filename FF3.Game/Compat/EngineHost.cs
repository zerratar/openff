// Where the OpenFF engine meets the legacy game.
//
// The engine (OpenFF.Engine, the assembly mods reference) knows nothing of MonoGame or
// of the decompiled game. This host creates it once the content is open, loads the code
// of the mods the mods folder enabled, ticks it once per game tick after the legacy
// frame has run, and tells it what the legacy game did: which part is running, which
// map was entered or left (the legacy scene's SceneInfo), and when a save slot was
// written or read back (so the mods' save chunks follow). Docs/OpenFF-Engine.md, "The
// object model and scripting"; the legacy game is, for now, the engine's one scene.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenFF;
using OpenFF.Modding;

namespace FF3
{
	internal static class EngineHost
	{
		private static bool _attached;
		private static string _lastPart;
		private static string _lastStage;
		private static bool _entered;
		private static DateTime _lastTick;

		/// <summary>Whether mods' code is loaded at all (--nomods turns it off; assets still apply).</summary>
		public static bool CodeEnabled => Options.Get("nomods") == null;

		/// <summary>Whether the engine exists yet (the hooks stay quiet before that).</summary>
		public static bool Attached => _attached;

		/// <summary>Creates the engine and loads the enabled mods' code. Called once the content is open.</summary>
		public static void Attach()
		{
			if (_attached)
			{
				return;
			}
			_attached = true;
			OpenFF.Game.Log = message => Log.Write(LogChannel.General, "engine: " + message);
			OpenFF.Game.Warn = message => Log.Write(LogChannel.General, "engine: WARNING " + message);
			OpenFF.Game.Saves.StorePath = Path.Combine(Path.GetDirectoryName(Launch.SettingsPath), "saves", "mods.json");
			// The API on the legacy game, before any mod so a mod's OnGameStart can reach it.
			EngineApi.Register();
			OpenFF.SceneLoader.ResolveNpc = (kind, index) => kind == "object" ? EngineApi.Npcs.Existing(index) : null;
			// A rebuilt mod: its objects went with its old code; make them again from its scene file.
			OpenFF.Game.Events.Subscribe<OpenFF.Events.ModReloaded>(e =>
			{
				if (!_entered || _lastStage == null) return;
				OpenFF.Modding.LoadedMod mod = OpenFF.Game.Mods.FirstOrDefault(m => string.Equals(m.Id, e.ModId, StringComparison.OrdinalIgnoreCase));
				if (mod != null) OpenFF.Game.Guard("scenes reload " + mod.Id, () => OpenFF.SceneLoader.Apply(mod, _lastStage));
			});

			int withCode = 0;
			if (CodeEnabled)
			{
				foreach (InstalledMod mod in GameArchive.ActiveMods)
				{
					ModDefinition definition = Define(mod);
					if (definition == null)
					{
						continue;
					}
					if (ModLoader.Load(definition) != null)
					{
						withCode++;
					}
				}
				ModWatcher.Start();
			}
			else
			{
				Log.Write(LogChannel.General, "engine: --nomods, no mod code loaded");
			}
			OpenFF.Game.Start();
			_lastTick = DateTime.Now;
			Log.Write(LogChannel.General, "engine: OpenFF " + OpenFF.Game.ApiVersion + " started - " + withCode + " mod(s) with code, "
				+ OpenFF.Game.Services.All.Count + " service(s), saves in " + OpenFF.Game.Saves.StorePath
				+ (ModWatcher.Enabled ? ", watching for rebuilt assemblies" : ""));
		}

		/// <summary>
		/// A mod's code, when it has any: the assemblies mod.json lists, or every .dll at the
		/// mod's root when it lists none. Null for an assets-only mod.
		/// </summary>
		private static ModDefinition Define(InstalledMod mod)
		{
			List<string> assemblies = new List<string>();
			if (mod.Manifest.Assemblies != null && mod.Manifest.Assemblies.Count > 0)
			{
				foreach (string name in mod.Manifest.Assemblies)
				{
					string path = Path.IsPathRooted(name) ? name : Path.Combine(mod.Directory, name);
					if (File.Exists(path))
					{
						assemblies.Add(Path.GetFullPath(path));
					}
					else
					{
						Log.Write(LogChannel.General, "engine: mod " + mod.Id + " names " + name + ", which is not there");
					}
				}
			}
			else if (Directory.Exists(mod.Directory))
			{
				assemblies.AddRange(Directory.EnumerateFiles(mod.Directory, "*.dll", SearchOption.TopDirectoryOnly)
					.Where(f => !string.Equals(Path.GetFileName(f), "OpenFF.Engine.dll", StringComparison.OrdinalIgnoreCase))
					.OrderBy(f => f, StringComparer.OrdinalIgnoreCase));
			}
			if (assemblies.Count == 0)
			{
				return null;
			}
			string scenes = Path.Combine(mod.Directory, string.IsNullOrWhiteSpace(mod.Manifest.Scenes) ? "scenes" : mod.Manifest.Scenes);
			return new ModDefinition
			{
				Id = mod.Id,
				Name = mod.DisplayName,
				Version = mod.Manifest.Version,
				Directory = mod.Directory,
				Assemblies = assemblies,
				Scenes = Directory.Exists(scenes) ? scenes : null,
			};
		}

		/// <summary>Once per game tick, after the legacy frame: tell the engine what changed, reload rebuilt mods, run the engine's frame.</summary>
		public static void Tick()
		{
			if (!_attached)
			{
				return;
			}
			try
			{
				WatchLegacy();
				EngineInput.Update();
				EngineApi.Tick();
				ModWatcher.Drain();
				DateTime now = DateTime.Now;
				double delta = Math.Min(0.25, (now - _lastTick).TotalSeconds);
				_lastTick = now;
				OpenFF.Game.Update(delta);
			}
			catch (Exception ex)
			{
				Log.First(LogChannel.General, "engine-tick", 5, () => "engine: tick failed: " + ex.GetType().Name + ": " + ex.Message + " at " + FirstFrames(ex));
			}
		}

		/// <summary>The first few frames of a stack trace, on one line, for the log.</summary>
		private static string FirstFrames(Exception ex)
		{
			string[] lines = (ex.StackTrace ?? "").Split('\n');
			return string.Join(" | ", lines.Take(4).Select(l => l.Trim()));
		}

		/// <summary>The legacy game's part and stage, turned into scene events when they change.</summary>
		private static void WatchLegacy()
		{
			string part = CurrentPart();
			if (part != _lastPart)
			{
				OpenFF.Game.Events.Publish(new OpenFF.Events.PartChanged { From = _lastPart, To = part });
				if (_lastPart == "BATTLE")
				{
					OpenFF.Game.Events.Publish(new OpenFF.Events.BattleEnded { Result = BattleResult() });
				}
				_lastPart = part;
			}

			string stage = GlobalScope.stg.CStageMng.CurrentName;
			// The stage name changes when a map's files load; outside the world part there is no map.
			if (part != "WORLD")
			{
				stage = null;
			}
			Scene legacy = OpenFF.Game.World.Legacy;
			if (stage != _lastStage)
			{
				if (legacy.Info != null)
				{
					OpenFF.Game.Services.SceneUnloadingInternal(legacy.Info);
					OpenFF.Game.Events.Publish(new OpenFF.Events.MapLeaving { Scene = legacy.Info });
					legacy.Info = null;
				}
				_lastStage = stage;
				_entered = false;
			}
			// Entered only once the field is up and moving: before that the hero is not placed
			// and characters put on the map are swept away by the field's own setup.
			if (stage != null && !_entered && FieldReady())
			{
				legacy.Info = new SceneInfo { Name = stage, Type = StageType(), Source = GameArchive.Game };
				_entered = true;
				// The mods' scene files for this map: objects with behaviours from the editor.
				OpenFF.Game.Guard("scenes " + stage, () => OpenFF.SceneLoader.ApplyAll(stage));
				OpenFF.Game.Events.Publish(new OpenFF.Events.MapEntered { Scene = legacy.Info });
				OpenFF.Game.Services.SceneLoadedInternal(legacy.Info);
			}
		}

		private static bool FieldReady()
		{
			try
			{
				GlobalScope.wld.CBaseSystem world = GlobalScope.wld.CBaseSystem.Current;
				return world != null && world.State() == GlobalScope.wld.CBaseSystem.WORLD_STATE.WORLD_STATE_MOVE
					&& GlobalScope.CCastCommandTransit.getInstance().cast_PlayerMng() != null;
			}
			catch (Exception)
			{
				return false;
			}
		}

		private static OpenFF.BattleResult BattleResult()
		{
			try
			{
				switch (GlobalScope.btl.BattleToOutside.getInstance().battleResult())
				{
					case GlobalScope.btl.BATTLE_RESULT.WIN: return OpenFF.BattleResult.Won;
					case GlobalScope.btl.BATTLE_RESULT.LOSE: return OpenFF.BattleResult.Lost;
					case GlobalScope.btl.BATTLE_RESULT.PLAYER_ESCAPE: return OpenFF.BattleResult.Escaped;
					default: return OpenFF.BattleResult.Unknown;
				}
			}
			catch (Exception)
			{
				return OpenFF.BattleResult.Unknown;
			}
		}

		private static string CurrentPart()
		{
			try
			{
				return ((GlobalScope.GAMEPART)GlobalScope.sys.FF3PartSys.getCurrentPart()).ToString().Replace("GAMEPART_", "");
			}
			catch (Exception)
			{
				return null;
			}
		}

		private static string StageType()
		{
			try
			{
				return GlobalScope.stageMng?.getStageType().ToString().Replace("STAGE_TYPE_", "");
			}
			catch (Exception)
			{
				return null;
			}
		}

		/// <summary>The legacy game wrote a save slot (CARD_WriteAndVerifyEeprom).</summary>
		public static void SaveWritten(int offset, int length)
		{
			if (!_attached)
			{
				return;
			}
			OpenFF.Game.Events.Publish(new OpenFF.Events.SaveWritten { Offset = offset, Length = length });
			OpenFF.Game.Saves.WriteSlot(offset, length);
		}

		/// <summary>The legacy game read from the save file (CARD_ReadEeprom); a slot it once wrote brings the mods' chunks back.</summary>
		public static void SaveRead(int offset, int length)
		{
			if (!_attached)
			{
				return;
			}
			if (OpenFF.Game.Saves.ReadSlot(offset, length))
			{
				OpenFF.Game.Events.Publish(new OpenFF.Events.SaveRead { Offset = offset, Length = length });
			}
		}

		/// <summary>The game is closing.</summary>
		public static void Quit()
		{
			if (!_attached)
			{
				return;
			}
			ModWatcher.Stop();
			OpenFF.Game.Quit();
		}

		/// <summary>Lines for the debug overlay: the engine's state, and what services and behaviours want shown.</summary>
		public static IEnumerable<string> DebugLines()
		{
			if (!_attached)
			{
				yield break;
			}
			yield return "engine " + OpenFF.Game.ApiVersion + "  mods " + OpenFF.Game.Mods.Count + "  services " + OpenFF.Game.Services.All.Count
				+ "  objects " + OpenFF.Game.World.ObjectCount + "  handlers " + OpenFF.Game.Events.HandlerCount + "  frame " + OpenFF.Game.Time.Frame
				+ (ModWatcher.Enabled ? "  hot reload on" : "");
			foreach (LoadedMod mod in OpenFF.Game.Mods)
			{
				yield return "  mod " + mod.Id + " " + mod.Version + (mod.Reloads > 0 ? "  reloaded x" + mod.Reloads : "");
			}
			foreach (GameService service in OpenFF.Game.Services.All)
			{
				IEnumerable<string> lines = null;
				try { lines = service.DebugLines(); } catch (Exception) { }
				if (lines == null) continue;
				foreach (string line in lines)
				{
					yield return "  " + ServiceRegistry.Name(service) + ": " + line;
				}
			}
			foreach (Scene scene in OpenFF.Game.World.Scenes)
			{
				foreach (GameObject o in scene.All())
				{
					foreach (Behaviour b in o.Components.OfType<Behaviour>())
					{
						IEnumerable<string> lines = null;
						try { lines = b.DebugLines(); } catch (Exception) { }
						if (lines == null) continue;
						foreach (string line in lines)
						{
							yield return "  " + o.Name + "." + b.GetType().Name + ": " + line;
						}
					}
				}
			}
		}
	}
}
