// Warp and Teleport from the Magic menu, as libff4 casts them (world::MSSMagic::stMSSMPrcMagicSelect): no target - the
// MP taken (the record's cost, not halved), the menu shut (WSMenu::shiftRoot with AFTER_STATE 3 "desion" for Warp 4512,
// 4 "telepo" for Teleport 4022) and the party sent away by the field's direction (WSTelepoDirection, which
// WSDesionDirection shares): the music stopped over 15 frames, effect 489 (efp 0x15E) on the leader with the common
// SE 0x1A, the leader drawn up thin and tall over eight frames (the counter 1, 2, 4 ... 128: x and z by 1 - c/128, y by
// 1 + c/128, its shadow alike), hidden, 30 frames, then the map jump's own fade.
//   - Teleport goes where the dungeon's script said (setTelepo: WSCMapJump's telepo point, kept in the save).
//   - Warp goes back a floor (world::MapJumpHistory, 15 deep): an exit into a d map pushes its arrival
//     (MapJumpObject::setMapJump), addDesionList pushes one, clearDesionList and a world map (WSPrepare's field stage)
//     clear it; Warp pops the newest and goes to the one under it, and needs two (mjh_history_is_exist). The save keeps
//     the newest two, as GameParameter's desion positions.
// Where they may be cast is the game's global script's (restrictionItemUse 4022 in towns, on the world and in some rooms;
// 4512 in towns and on the world), kept by the event restriction list, which every map jump empties (WSMapJump's
// clearItemValidity).

using System;
using System.Collections.Generic;

namespace OpenFF.Client
{
	internal sealed class Ff4Warp : GameService
	{
		public override bool WantsUpdate => true;

		internal static Ff4Warp Instance { get; private set; }
		public Ff4Warp() { Instance = this; }

		public const int SpellTeleport = 4022, SpellWarp = 4512;

		/// <summary>A place to be sent to: the map, x, y, z (the map jump's units) and the facing in eighths.</summary>
		public sealed class Place
		{
			public string Map;
			public int X, Y, Z, Facing;
			public override string ToString() => Map + " at " + X / 4096 + ", " + Y / 4096 + ", " + Z / 4096;
		}

		/// <summary>The history, newest first.</summary>
		private static readonly List<Place> _history = new List<Place>();
		private const int HistoryMax = 15;

		public static void Push(Place p)
		{
			if (p == null || string.IsNullOrEmpty(p.Map)) return;
			_history.Insert(0, p);
			if (_history.Count > HistoryMax) _history.RemoveAt(_history.Count - 1);
		}

		public static void ClearHistory() => _history.Clear();

		/// <summary>mjh_history_is_exist: somewhere to go back to.</summary>
		public static bool CanWarp => _history.Count > 1;

		/// <summary>addDesionList(map, x, y, z, facing 0..7).</summary>
		public static void AddDesionList(GlobalScope.ScriptEngine engine)
		{
			string map = engine.getString();
			int x = (int)engine.getDword(), y = (int)engine.getDword(), z = (int)engine.getDword();
			int facing = (int)engine.getDword();
			Push(new Place { Map = map, X = x, Y = y, Z = z, Facing = facing & 7 });
		}

		/// <summary>clearDesionList(?, ?, ?).</summary>
		public static void ClearDesionList(GlobalScope.ScriptEngine engine)
		{
			engine.getDword(); engine.getDword(); engine.getDword();
			ClearHistory();
		}

		/// <summary>An exit taken (MapJumpObject::setMapJump): one into a dungeon's map is remembered for Warp.</summary>
		public static void ExitTaken(string destination, int x, int y, int z, int facing)
		{
			if (destination != null && destination.Length > 0 && destination[0] == 'd') Push(new Place { Map = destination, X = x, Y = y, Z = z, Facing = facing & 7 });
		}

		/// <summary>Whether the event restriction list (restrictionItemUse) bars a spell or item here (WSCMenu::checkItem's validity 2).</summary>
		public static bool Restricted(int id)
		{
			try { return GlobalScope.evt.CEventRestriction.getSingleton().check(id); } catch (Exception) { return false; }
		}

		/// <summary>A map jump: the restrictions were the map's (clearItemValidity).</summary>
		public static void Jumping()
		{
			try { GlobalScope.evt.CEventRestriction.getSingleton().clear(); } catch (Exception) { }
		}

		// ---- the save: the telepo point and the newest two of the history ----

		public sealed class Saved
		{
			public Place Telepo;
			public List<Place> Desion;
		}

		public static Saved ForSave()
		{
			var t = Ff4PartyCommands.Telepo;
			return new Saved
			{
				Telepo = t is { } tp ? new Place { Map = tp.Map, X = tp.X, Y = tp.Y, Z = tp.Z, Facing = tp.Facing } : null,
				Desion = _history.Count > 0 ? _history.GetRange(0, Math.Min(2, _history.Count)) : null,
			};
		}

		public static void Restore(Saved s)
		{
			_history.Clear();
			Ff4PartyCommands.Telepo = s?.Telepo is Place t && !string.IsNullOrEmpty(t.Map) ? (t.Map, t.X, t.Y, t.Z, t.Facing) : null;
			if (s?.Desion != null) _history.AddRange(s.Desion);
		}

		/// <summary>A new game: no way out kept, no floors behind.</summary>
		public static void NewGame()
		{
			_history.Clear();
			Ff4PartyCommands.Telepo = null;
		}

		// ---- the cast ----

		/// <summary>Where the spell would send the party now, or null when it cannot (nothing to go back to, no way out set).</summary>
		public static Place Destination(int spellId)
		{
			if (spellId == SpellWarp) return CanWarp ? _history[1] : null;
			if (spellId == SpellTeleport && Ff4PartyCommands.Telepo is { } t && !string.IsNullOrEmpty(t.Map)) return new Place { Map = t.Map, X = t.X, Y = t.Y, Z = t.Z, Facing = t.Facing };
			return null;
		}

		private Place _going;
		private bool _warp;
		private int _step, _count;
		private GlobalScope.VecFx32 _scale, _shadow;

		/// <summary>The party being sent away: the field is the direction's.</summary>
		public static bool Busy => Instance?._going != null;

		/// <summary>The spell cast: the menu has shut; the direction starts on the next frame.</summary>
		public static void Start(int spellId)
		{
			Ff4Warp w = Instance;
			Place to = Destination(spellId);
			if (w == null || to == null) return;
			if (spellId == SpellWarp) _history.RemoveAt(0);   // mjh_pop_history: the newest let go, the one under it is where we go
			w._going = to;
			w._warp = spellId == SpellWarp;
			w._step = 0;
			Log.Write(LogChannel.File, "field: " + (w._warp ? "Warp" : "Teleport") + " to " + to);
		}

		private static GlobalScope.pl.CBasePlayer Hero => EngineApi.HeroPlayer;

		public override void OnUpdate()
		{
			if (!GameProfile.IsFf4) return;
			string map = Game.Field.Map;
			// WSPrepare: a world map entered forgets the floors behind (Teleport's arrival too).
			if (map != _lastMap && map != null && map.Length > 0 && map[0] == 'f') ClearHistory();
			_lastMap = map;
			if (_going == null) return;
			try { Step(map); }
			catch (Exception ex)
			{
				Log.Write(LogChannel.General, "field: warp: " + ex.Message);
				Show();
				_going = null;
			}
		}

		private string _lastMap;
		private bool _arrived;

		/// <summary>A map entered (Ff4MapChange.Enter).</summary>
		public static void Entered()
		{
			if (Instance != null) Instance._arrived = true;
		}

		private void Step(string map)
		{
			GlobalScope.pl.CBasePlayer hero = Hero;
			switch (_step)
			{
				case 0:   // wsInitialize: the music stops; wsProcess 0/2: the effect and the sound
					try { Game.Hero.Freeze(); } catch (Exception) { }
					try { GlobalScope.MatrixSound.MtxSoundBGM.getSingleton().stop(15, GlobalScope.MatrixSound.enMtxBGMSlot.enMTX_BGM_SLOT0); } catch (Exception) { }
					_scale = hero?.getScale() is GlobalScope.VecFx32 s ? new GlobalScope.VecFx32(s.x, s.y, s.z) : new GlobalScope.VecFx32(4096, 4096, 4096);
					_shadow = hero?.getShadowScale() is GlobalScope.VecFx32 sh ? new GlobalScope.VecFx32(sh.x, sh.y, sh.z) : null;
					try
					{
						GlobalScope.eff.CEffectMng effects = GlobalScope.eff.CEffectMng.instance();
						effects.loadEfpNamed("e489", "/EFFECT/e489.efp");
						int made = effects.create(489, 1);
						if (made >= 0 && hero != null) effects.setPosition(made, hero.getPosition());
						if (made < 0) Log.Write(LogChannel.File, "field: warp: effect 489 not made");
					}
					catch (Exception) { }
					Game.Audio.PlaySe(0, 0x1A);
					_count = 1;
					_step = 1;
					return;
				case 1:   // wsProcessCharacterAct: drawn up thin and tall, then gone
					if (_count <= 0x80)
					{
						int part = _count * 4096 / 0x80;
						hero?.setScale(new GlobalScope.VecFx32(Fx(_scale.x, 4096 - part), Fx(_scale.y, 4096 + part), Fx(_scale.z, 4096 - part)));
						if (_shadow != null) hero?.setShadowScale(new GlobalScope.VecFx32(Fx(_shadow.x, 4096 - part), Fx(_shadow.y, 4096 + part), Fx(_shadow.z, 4096 - part)));
						_count <<= 1;
						return;
					}
					hero?.setHidden(true);
					_count = 30;
					_step = 2;
					return;
				case 2:   // the 30 frames, then wsFinalize: the map jump
					if (_count-- > 0) return;
					_arrived = false;
					Jump(_going);
					_step = 3;
					_count = 600;
					return;
				case 3:   // the jump made: on the new map the leader stands again at its size
					if ((_arrived || _count-- <= 0) && EngineApi.InWorld)
					{
						Show();
						_going = null;
					}
					return;
			}
		}

		private static int Fx(int value, int by) => (int)((long)value * by + 0x800 >> 12);

		private void Show()
		{
			GlobalScope.pl.CBasePlayer hero = Hero;
			if (hero == null) return;
			try
			{
				hero.setScale(_scale ?? new GlobalScope.VecFx32(4096, 4096, 4096));
				if (_shadow != null) hero.setShadowScale(_shadow);
				hero.setHidden(false);
				Log.Write(LogChannel.File, "field: warp: the leader stands on " + Game.Field.Map + " at " + Game.Hero.Position);
			}
			catch (Exception) { }
			try { Game.Hero.Unfreeze(); } catch (Exception) { }
		}

		private static void Jump(Place to)
		{
			GlobalScope.VecFx32 pos = new GlobalScope.VecFx32(to.X, to.Y, to.Z);
			GlobalScope.VecFx32 rot = new GlobalScope.VecFx32(0, (to.Facing & 7) * 0x2000, 0);
			string map = to.Map;
			if (map.Length == 3 && (map[0] == 'f' || map[0] == 'F')) map = JumpPart.WithChip(map, pos);
			GlobalScope.CCastCommandTransit transit = GlobalScope.CCastCommandTransit.getInstance();
			transit.castParam_MapJump().initialize();
			transit.castParam_MapJump().setUp(map, 0, pos, rot, _Flag: true);
			transit.cast_BaseSystem().setMapJump(b: true);
		}
	}
}
