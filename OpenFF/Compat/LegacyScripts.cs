// The engine's own script (Game.Scripts, IScripts) on the legacy game: the casts' code the
// mod defines for a map - CastScript components, one function each - compiled with the
// script language's own compiler (Shared/Script/Ffs, the one Crystal's script-build uses)
// into a .script of the mod's, registered with the game's LogicManager beside the map's
// under a map number of its own, and started through the same startLogic the game's talk
// uses. The game's interpreter runs the code on its own logic loop: every command is the
// game's, the code is the mod's.
//
// The document compiled: "map 60000;" (a number no map has; the manager tells logics apart
// by map and cast), a cast entry per defined cast (init none, main the function, exit none),
// then each function as "castN_main:" followed by its lines. A cast's lines that name a
// label outside them, or call a function of the map's own script (call(1, ...)), do not
// compile or would not resolve, and the definition is refused with the reason in the log;
// the converter leaves such a cast a GameCast.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenFF;
using OpenFF.Script;
using Ffs = Crystal.Ffs;

namespace OpenFF.Client
{
	internal sealed class LegacyScripts : GameService, IScripts
	{
		/// <summary>The map number the mod's script carries: none of the game's, so its logics are its own.</summary>
		public const uint ModMap = Ffs.CastCode.ModMap;

		private readonly Dictionary<int, string[]> _code = new Dictionary<int, string[]>();
		private string _map;
		private GlobalScope.ScriptData _registered;
		private bool _dirty;

		public bool Define(int cast, IReadOnlyList<string> lines)
		{
			if (GameProfile.IsFf4) { Log.Write(LogChannel.General, "scripts: FF3's language only for now"); return false; }
			if (cast <= 0 || lines == null) return false;
			Fresh();
			string[] body = Ffs.CastCode.Substitute(cast, lines);
			// Compiled alone first, so a broken definition is refused and the others stand.
			if (!TryCompile(new Dictionary<int, string[]> { [cast] = body }, out _, out string problem))
			{
				Log.Write(LogChannel.General, "scripts: cast " + cast + " refused: " + problem);
				return false;
			}
			_code[cast] = body;
			_dirty = true;
			return true;
		}

		private int _nextOwn = 5000;

		public int Allocate()
		{
			Fresh();
			return _nextOwn++;
		}

		public bool IsRunning(int cast)
		{
			try { return GlobalScope.LogicManager.singleton().isEnableLogic(ModMap, (uint)cast) != 0; }
			catch (Exception) { return false; }
		}

		public bool Start(int cast)
		{
			Fresh();
			if (!_code.ContainsKey(cast) || !EngineApi.InWorld) return false;
			if (!Ensure()) return false;
			try
			{
				if (IsRunning(cast)) return false;
				GlobalScope.LogicManager.singleton().startLogic(ModMap, (uint)cast);
				Log.Write(LogChannel.File, "scripts: cast " + cast + " started (the engine's code) on " + _map);
				return IsRunning(cast);
			}
			catch (Exception ex)
			{
				EngineApi.Warn("scripts", "Start " + cast + ": " + ex.Message);
				return false;
			}
		}

		/// <summary>A new map: the definitions are the old map's. Called before any use.</summary>
		private void Fresh()
		{
			string map = null;
			try { map = GlobalScope.stg.CStageMng.CurrentName; } catch (Exception) { }
			if (map == _map) return;
			_map = map;
			_code.Clear();
			_registered = null;
			_dirty = false;
			_nextOwn = 5000;
		}

		/// <summary>The mod's script compiled and registered as it stands; true when there is one.</summary>
		private bool Ensure()
		{
			if (_registered != null && !_dirty) return true;
			if (_code.Count == 0) return false;
			if (!TryCompile(_code, out byte[] bytes, out string problem))
			{
				Log.Write(LogChannel.General, "scripts: the mod's script for " + _map + " did not compile: " + problem);
				return false;
			}
			try
			{
				GlobalScope.ScriptData data = GlobalScope.ScriptData.cast(bytes);
				if (data == null) { Log.Write(LogChannel.General, "scripts: the compiled script is not one the game reads"); return false; }
				GlobalScope.LogicManager manager = GlobalScope.LogicManager.singleton();
				// The map's registration is by map number; ours replaces ours.
				if (manager.isRegistScriptData(ModMap) != 0) manager.removeScriptData(ModMap);
				manager.registScriptData(data);
				_registered = data;
				_dirty = false;
				Log.Write(LogChannel.File, "scripts: the mod's script for " + _map + " registered - " + _code.Count + " cast(s): " + string.Join(", ", _code.Keys.OrderBy(k => k)));
				return true;
			}
			catch (Exception ex)
			{
				EngineApi.Warn("scripts", "register: " + ex.Message);
				return false;
			}
		}

		private static bool TryCompile(Dictionary<int, string[]> code, out byte[] bytes, out string problem)
		{
			// The frame and the compile are Shared (Ffs.CastCode): Crystal checks a cast's lines the same way as the modder types.
			bytes = Ffs.CastCode.Compile(code, ScriptOpTable.Ff3, out List<Ffs.CastCodeProblem> problems);
			problem = bytes == null ? string.Join("; ", problems.Select(p => p.ToString())) : null;
			return bytes != null;
		}
	}
}
