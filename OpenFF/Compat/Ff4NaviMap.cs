// FF4's field map as Steam's (M: "Map", then "Quit Map"; Back shuts it too) - libff4's map2d::NaviMap, read for the port
// (Docs/FF4-Internals.md, "The field map"). Over the live field, at 77 % (setAlpha 24 of 31): the parchment frame
// (mapwaku), the floor's picture (<map>.NCGR, 512 x 384) where the party has been and the blank sheet (dxx / field_xx)
// where it has not, a cell of 16 x 16 at a time; the icons on top - the party's pin (w_map_mark), chests closed and
// opened (map_mark_common), the save point's ring and the exits' bobbing arrows (d_map_obj), a town's shops (t_map_obj),
// the world's places once their flags are set (w_map_obj). Drawn in Steam's 1080p places (traced with ff4hook: the
// picture 864 x 648 from (528.2, 155.2), the sheets' pixels at 1.6875).
//
// What has been seen is a 32 x 24 grid per map (the .nmd says which cells count): each step the party is on a map, the
// 31 cells about it (PassagePointChange's blob) are seen, kept in the save (NavimapSaveData's per-map rows; a town is
// always all seen). A floor all seen is complete: its flag 1:<completeFlag> is set (wsmNaviMapComp). Which picture a
// stage shows (ws_prepare_setup_navimap, SetMapData): dNN_MM its own (d12_17 after flag 0:0xFF: d12_99), tNN_MM its own
// or its town's main map (a shop's, the player hidden), fNN... the world's field_NN_00; no picture (or t23): no Map.
// The Sight spell's screen shows the same pictures another way (Ff4NaviMapSight.cs).

using System;
using System.Collections.Generic;
using OpenFF.Content;
using OpenFF;

namespace OpenFF.Client
{
	internal sealed partial class Ff4NaviMap : GameService
	{
		public override bool WantsUpdate => true;

		internal static Ff4NaviMap Instance { get; private set; }
		public Ff4NaviMap() { Instance = this; }

		// The seen cells of every map visited, by map name: 24 rows of 32 bits, cell c = bit 31 - c (NavimapSaveData's order).
		private static readonly Dictionary<string, uint[]> _seen = new Dictionary<string, uint[]>(StringComparer.OrdinalIgnoreCase);

		private sealed class Icon { public string Bank; public int Cell = -1; public int Anim; public float Fx, Fz; public int Flag = -1; public int FlagGroup = 1; public int OpenCell = -1; public bool Always; public bool Place; }

		/// <summary>One map's picture and data (SetMapData): its .nmi geometry and icons, its .nmd walkable cells, what is seen.</summary>
		private sealed class MapData
		{
			public string Name;               // the navimap's map name (after the rewrites)
			public char Kind;                 // 'd' dungeon, 't' town, 'f' world
			public bool Track;
			public int StageW = 1, StageH = 1, CorrX, CorrZ, CompleteFlag = -1, Total;
			public readonly bool[,] Walk = new bool[24, 32];
			public readonly bool[,] Cells = new bool[24, 32];
			public readonly List<Icon> Icons = new List<Icon>();

			/// <summary>The map's files read: null when it has no picture (no Map there).</summary>
			public static MapData Read(string name)
			{
				if (name == null || Ff4Ui.ReadFile(name + "_00.NSCR") == null || Ff4Ui.Sheet(name + ".NCGR") == null) return null;
				MapData m = new MapData { Name = name, Kind = name[0] };
				byte[] nmi = Ff4Ui.ReadFile(name + ".nmi");
				if (nmi != null) m.ReadNmi(nmi);
				byte[] nmd = m.Kind != 't' ? Ff4Ui.ReadFile(name + ".nmd") : null;
				if (nmd != null && nmd.Length >= 2 + 2 * 768)
				{
					m.Total = S16(nmd, 0);
					for (int r = 0; r < 24; r++) for (int c = 0; c < 32; c++) m.Walk[r, c] = S16(nmd, 2 + 2 * (r * 32 + c)) == 1;
				}
				return m;
			}

			public void SeeAll() { for (int r = 0; r < 24; r++) for (int c = 0; c < 32; c++) Cells[r, c] = true; }

			private void ReadNmi(byte[] b)
			{
				if (Kind == 'd')
				{
					int chests = Math.Min(30, (int)S16(b, 0)), exits = Math.Min(20, (int)S16(b, 4));
					StageW = Math.Max(1, (int)S16(b, 6)); StageH = Math.Max(1, (int)S16(b, 8));
					CorrX = S16(b, 0xA); CorrZ = S16(b, 0xC);
					int spX = S16(b, 0xE), spZ = S16(b, 0x10);
					CompleteFlag = S16(b, 0x14);
					Track = S16(b, 0x16) == 1;
					for (int i = 0; i < chests; i++)
						Icons.Add(At(new Icon { Bank = "map_mark_common", Cell = 1, OpenCell = 2, Flag = S16(b, 0x18 + 6 * i + 4) }, S16(b, 0x18 + 6 * i), S16(b, 0x18 + 6 * i + 2)));
					for (int i = 0; i < exits; i++)
						Icons.Add(At(new Icon { Bank = "d_map_obj", Anim = 6 }, S16(b, 0xCC + 4 * i), S16(b, 0xCC + 4 * i + 2)));
					if (spX != 0 || spZ != 0) Icons.Add(At(new Icon { Bank = "d_map_obj", Cell = 0 }, spX, spZ));
				}
				else if (Kind == 't')
				{
					int n = S16(b, 0), hide = S16(b, 0x1A);
					StageW = Math.Max(1, (int)S16(b, 0x1E)); StageH = Math.Max(1, (int)S16(b, 0x20));
					CorrX = S16(b, 0x22); CorrZ = S16(b, 0x24);
					int[] shopCells = { 3, 0, 6, 1, 2, 4 };
					for (int i = 0; i < 6; i++)
					{
						if ((hide >> i & 1) != 0 || i == 4) continue;
						Icons.Add(At(new Icon { Bank = "t_map_obj", Cell = shopCells[i], Always = true }, S16(b, 2 + 2 * i), S16(b, 0xE + 2 * i)));
					}
					for (int i = 0; i < n && 0x26 + 8 * i + 8 <= b.Length; i++)
					{
						if (S16(b, 0x26 + 8 * i + 6) != 0) continue;   // NMIHiddenItem: never drawn
						Icons.Add(At(new Icon { Bank = "map_mark_common", Cell = 1, OpenCell = 2, Flag = S16(b, 0x26 + 8 * i + 4), Always = true }, S16(b, 0x26 + 8 * i), S16(b, 0x26 + 8 * i + 2)));
					}
				}
				else
				{
					int n = S16(b, 0);
					StageW = Math.Max(1, (int)S16(b, 2)); StageH = Math.Max(1, (int)S16(b, 4));
					CorrX = S16(b, 6); CorrZ = S16(b, 8);
					int[] placeCells = { 2, 0, 3, 4 };
					for (int i = 0; i < n && 10 + 8 * i + 8 <= b.Length; i++)
					{
						int kind = S16(b, 10 + 8 * i + 4);
						Icon icon = At(new Icon { Bank = "w_map_obj", Cell = placeCells[Math.Clamp(kind, 0, 3)], Place = true }, S16(b, 10 + 8 * i), S16(b, 10 + 8 * i + 2));
						icon.Flag = S16(b, 10 + 8 * i + 6);   // shown once flag 0:id is set
						Icons.Add(icon);
					}
				}
			}

			/// <summary>A field position as the picture's fraction across and down (nmi_visit_update_position).</summary>
			public (float Fx, float Fz) Fraction(float x, float z) => ((x + CorrX + StageW / 2f) / StageW, (z + CorrZ + StageH / 2f) / StageH);

			/// <summary>The party's place in the game's own terms: the port's world map is FF4's mirrored in z (FieldMirror).</summary>
			public (float Fx, float Fz) PartyFraction(Vector3 at, string stage)
			{
				bool mirrored = Kind == 'f' && Ff4Exits.MirroredField(stage);
				(float fx, float fz) = Fraction(at.X, mirrored ? -at.Z : at.Z);
				if (Kind == 'f') { fx -= (float)Math.Floor(fx); fz -= (float)Math.Floor(fz); }   // the world wraps
				return (fx, fz);
			}

			public Icon Place(Icon icon, int x, int z) => At(icon, x, z);

			private Icon At(Icon icon, int x, int z)
			{
				(icon.Fx, icon.Fz) = Fraction(x, z);
				return icon;
			}

			public bool SeenAt(float fx, float fz)
			{
				int c = (int)Math.Floor(fx * 32), r = (int)Math.Floor(fz * 24);
				return c >= 0 && c < 32 && r >= 0 && r < 24 && Cells[r, c];
			}

			/// <summary>MapPercentUpDate: the walkable cells seen of all of them, 0..100.</summary>
			public int Percent()
			{
				if (Total <= 0) return 0;
				int seen = 0;
				for (int r = 0; r < 24; r++) for (int c = 0; c < 32; c++) if (Cells[r, c]) seen++;
				return Math.Min(100, seen * 100 / Total);
			}
		}

		private string _stage;              // the field stage the data is for
		private MapData _map;               // its map, null: no Map here
		private bool _showPlayer;
		private bool _open;
		private int _frame, _idle;
		private long _lastStep, _stillSince;
		private float _barDrop = BarHeight;
		private const float BarHeight = 108f, BarStep = 40.5f;   // 1080's pixels; a step's slide (Steam's 20.25 a frame)
		private const int BarWait = 15;                          // steps stood before it comes back (Steam's 30 frames)
		// showMapPercent: the explored percentage on a dungeon's map, as the stage's script leaves it (on for each new stage).
		private static bool _showPercent = true;
		private static string _percentStage;
		private Vector3 _lastAt;
		private readonly Ff4MenuHud.Data _hud = new Ff4MenuHud.Data();

		public bool IsOpen => _open;

		// Steam's 1080p places: the picture's top left and its scale (a sheet pixel), the frame's middle.
		// (The frame's cell hangs from the screen's middle, as libff4's sprite at (240, 160): its art 952 x 560 from (-476, -316).)
		private const float MapLeft = 528.2f, MapTop = 155.2f, Px = 1.6875f, FrameX = 960f, FrameY = 540.85f, Alpha = 24f / 31f;

		// PassagePointChange's blob (DAT_002bd5cc): rows -2 and +2 five wide, rows -1..1 seven wide.
		private static readonly (int Dx, int Dy)[] Blob = BuildBlob();
		private static (int, int)[] BuildBlob()
		{
			List<(int, int)> list = new List<(int, int)>();
			for (int dy = -2; dy <= 2; dy++)
			{
				int w = Math.Abs(dy) == 2 ? 2 : 3;
				for (int dx = -w; dx <= w; dx++) list.Add((dx, dy));
			}
			return list.ToArray();
		}

		// ---- the save ----

		public static Dictionary<string, uint[]> SeenForSave()
		{
			Dictionary<string, uint[]> copy = new Dictionary<string, uint[]>();
			foreach (KeyValuePair<string, uint[]> pair in _seen) copy[pair.Key] = (uint[])pair.Value.Clone();
			return copy;
		}

		public static void Restore(Dictionary<string, uint[]> seen)
		{
			_seen.Clear();
			if (seen != null) foreach (KeyValuePair<string, uint[]> pair in seen) if (pair.Value != null && pair.Value.Length == 24) _seen[pair.Key] = (uint[])pair.Value.Clone();
			if (Instance != null) Instance._stage = null;   // read again
		}

		public static void NewGame() { _seen.Clear(); if (Instance != null) Instance._stage = null; }

		/// <summary>is_visited_map: a map whose kept sight has anything in it.</summary>
		private static bool Visited(string name)
		{
			if (!_seen.TryGetValue(name, out uint[] rows)) return false;
			foreach (uint row in rows) if (row != 0) return true;
			return false;
		}

		// ---- the map's data ----

		private static bool Flag(int group, int index)
		{
			byte[,] flags = GlobalScope.flags;
			return flags != null && group >= 0 && group < flags.GetLength(0) && index >= 0 && index < flags.GetLength(1) && flags[group, index] != 0;
		}

		/// <summary>ws_prepare_setup_navimap / SetMapData's name: the picture a stage shows, and whether the party's pin is on it.</summary>
		private static string NaviName(string stage, out bool player)
		{
			player = true;
			if (string.IsNullOrEmpty(stage) || stage.Length < 3) return null;
			string s = stage.ToLowerInvariant();
			if (s[0] == 'f' && char.IsDigit(s[1]) && char.IsDigit(s[2])) return "field_" + s.Substring(1, 2) + "_00";
			if (s.Length < 6) return null;
			if (s[0] == 'd') return s == "d12_17" && Flag(0, 0xFF) ? "d12_99" : s.Substring(0, 6);
			if (s[0] != 't') return null;
			s = s.Substring(0, 6);
			if (s.Substring(1, 2) == "23") return null;   // show_map_widget off
			if (s.Substring(4, 2) == "00") return s;
			if (!int.TryParse(s.Substring(1, 2), out int town)) return null;
			if (town == 14 && s.Substring(4, 2) != "01" && s.Substring(4, 2) != "02") { player = false; return "t14_02"; }
			if (town > 16 || (0x15a31 >> town & 1) == 0) { player = false; return "t" + s.Substring(1, 2) + "_00"; }
			return s;
		}

		private static short S16(byte[] b, int at) => at + 1 < b.Length ? BitConverter.ToInt16(b, at) : (short)0;

		private void Load(string stage)
		{
			_stage = stage;
			Ff4PartyCommands.ApplyLeaderLook();   // the hero made with the map: Mini's size, a scene's stone
			if (!string.Equals(_percentStage, stage, StringComparison.OrdinalIgnoreCase)) _showPercent = true;
			string navi = NaviName(stage, out _showPlayer);
			if (_resetName != null && string.Equals(_resetStage, stage, StringComparison.OrdinalIgnoreCase)) navi = _resetName;
			_map = MapData.Read(navi);
			if (_map == null) return;
			// A town is seen whole; elsewhere what was kept, or nothing yet.
			if (_map.Kind == 't') _map.SeeAll();
			else if (_seen.TryGetValue(_map.Name, out uint[] rows)) { for (int r = 0; r < 24; r++) for (int c = 0; c < 32; c++) _map.Cells[r, c] = (rows[r] >> (31 - c) & 1) != 0; }
			foreach ((string Stage, Icon Icon, int X, int Z) chest in _scriptChests) if (string.Equals(chest.Stage, stage, StringComparison.OrdinalIgnoreCase)) _map.Icons.Add(_map.Place(chest.Icon, chest.X, chest.Z));
			_scriptChests.RemoveAll(c => !string.Equals(c.Stage, stage, StringComparison.OrdinalIgnoreCase));
			Log.Write(LogChannel.File, "navimap: " + stage + " -> " + _map.Name + " (" + _map.Icons.Count + " icon(s), " + _map.Total + " cell(s) to see)");
		}

		// resetNaviMap's sheet for the stage it was given on (libff4 writes the name into the world state and sets the map
		// up again; the next map sets its own).
		private static string _resetName, _resetStage;

		/// <summary>resetNaviMap(sheet, ?, ?): the map's sheet swapped for another's (d12_17's floor before and after it falls).</summary>
		public static void ResetNaviMap(GlobalScope.ScriptEngine engine)
		{
			string name = engine.getString();
			engine.getDword();
			engine.getDword();
			if (string.IsNullOrEmpty(name)) return;
			_resetName = name;
			try { _resetStage = GlobalScope.stg.CStageMng.CurrentName; } catch (Exception) { _resetStage = null; }
			if (Instance != null) Instance._stage = null;   // read again
		}

		// A map's script's own chests (NewTBoxIcon), kept for the stage they were made on.
		private static readonly List<(string Stage, Icon Icon, int X, int Z)> _scriptChests = new List<(string, Icon, int, int)>();

		/// <summary>babilCommand_NewTBoxIcon(x, y, z, flag group, flag index, ?): a chest's icon on this map's sheet (NMITBox, cell
		/// 1, cell 2 once its flag is set) - one the map's own table does not hold.</summary>
		public static void NewTBoxIcon(GlobalScope.ScriptEngine engine)
		{
			int x = (int)engine.getDword();
			engine.getDword();
			int z = (int)engine.getDword();
			int group = (int)engine.getDword(), index = (int)engine.getDword();
			engine.getDword();
			string stage = Game.Field.Map;
			Ff4NaviMap navi = Instance;
			Icon icon = new Icon { Bank = "map_mark_common", Cell = 1, OpenCell = 2, FlagGroup = group, Flag = index };
			if (navi?._map != null && string.Equals(navi._stage, stage, StringComparison.OrdinalIgnoreCase)) navi._map.Icons.Add(navi._map.Place(icon, x, z));
			else if (stage != null) _scriptChests.Add((stage, icon, x, z));
			Log.Write(LogChannel.File, "navimap: a script's chest at " + x + ", " + z + " (flag " + group + ":" + index + ")");
		}

		/// <summary>PassagePointChange: the 31 cells about the party's are seen where they count; kept, and a floor all seen complete.</summary>
		private void See(float fx, float fz)
		{
			MapData m = _map;
			if (m.Kind == 't') return;
			int cx = (int)Math.Floor(fx * 32), cy = (int)Math.Floor(fz * 24);
			bool wrap = m.Kind == 'f' && m.Name.Length > 7 && m.Name[7] != '1', changed = false;
			foreach ((int dx, int dy) in Blob)
			{
				int c = cx + dx, r = cy + dy;
				if (wrap) { c = (c % 32 + 32) % 32; r = (r % 24 + 24) % 24; }
				else { c = Math.Clamp(c, 0, 31); r = Math.Clamp(r, 0, 23); }
				if (m.Walk[r, c] && !m.Cells[r, c]) { m.Cells[r, c] = true; changed = true; }
			}
			if (!changed) return;
			int seen = 0;
			for (int r = 0; r < 24; r++) for (int c = 0; c < 32; c++) if (m.Cells[r, c] && m.Walk[r, c]) seen++;
			if (m.Total > 0 && seen * 100 / m.Total >= 100)
			{
				// MapPercentUpDate at 100: the whole picture shown; the floor complete (wsmNaviMapComp sets its flag).
				m.SeeAll();
				byte[,] flags = GlobalScope.flags;
				if (m.Kind == 'd' && m.CompleteFlag >= 0 && flags != null && 1 < flags.GetLength(0) && m.CompleteFlag < flags.GetLength(1) && flags[1, m.CompleteFlag] == 0)
				{
					flags[1, m.CompleteFlag] = 1;
					Log.Write(LogChannel.General, "navimap: " + m.Name + " complete (flag 1:" + m.CompleteFlag + ")");
				}
			}
			if (m.Kind == 'd' && !m.Track) return;   // this floor's sight is not kept (d04_04)
			uint[] rows = new uint[24];
			for (int r = 0; r < 24; r++) for (int c = 0; c < 32; c++) if (m.Cells[r, c]) rows[r] |= 1u << (31 - c);
			_seen[m.Name] = rows;
		}

		// ---- the scripts' commands ----

		/// <summary>babilCommand_NavimapFullDisclosure(?, ?): the current stage's map all seen, kept.</summary>
		public static void FullDisclosure(GlobalScope.ScriptEngine engine)
		{
			engine.getDword();
			engine.getDword();
			string name = NaviName(Game.Field.Map, out _);
			if (name == null || name[0] == 't') return;
			uint[] rows = new uint[24];
			for (int r = 0; r < 24; r++) rows[r] = 0xFFFFFFFF;
			_seen[name] = rows;
			if (Instance?._map != null && string.Equals(Instance._map.Name, name, StringComparison.OrdinalIgnoreCase)) Instance._stage = null;   // read again
			Log.Write(LogChannel.File, "navimap: " + name + " fully disclosed");
		}

		/// <summary>babilCommand_NavimapSetDisclosureRateVisibility(shown, ?, ?): showMapPercent.</summary>
		public static void SetDisclosureRateVisibility(GlobalScope.ScriptEngine engine)
		{
			uint shown = engine.getDword();
			engine.getDword();
			engine.getDword();
			_showPercent = shown != 0;
			_percentStage = Game.Field.Map;
		}

		// ---- each step ----

		private static bool FreeToWalk() => EngineApi.InWorld && Game.Hero.Present && !Ff4Battle.Active && !Ff4Cutscene.Active && !Game.Dialogue.IsOpen
			&& !Ff4Chests.Busy && !Ff4Extras.Busy && !EncounterZoom.Playing && !BlurRotate.Playing
			&& !Ff4Menu.EventBusy() && !(Ff4Menu.Instance?.IsOpen ?? false) && !(Ff4Shop.Instance?.IsOpen ?? false);

		public override void OnUpdate()
		{
			if (!GameProfile.IsFf4) return;
			_frame++;
			if (!EngineApi.InWorld || Ff4Battle.Active) { if (_open) Shut(false); return; }
			string stage = Game.Field.Map;
			if (stage != _stage) { Load(stage); if (_open) Shut(false); }   // a map jump is no free walking: the map shuts
			Vector3 at = Game.Hero.Present ? Game.Hero.Position : _lastAt;
			bool moving = (at - _lastAt).Length > 0.01f;
			_lastAt = at;
			_idle = moving ? 0 : _idle + 1;
			if (moving || _idle == 1) _stillSince = LegacyStep.Count;
			if (_map != null && Game.Hero.Present && !Ff4Cutscene.Active)
			{
				(float fx, float fz) = _map.PartyFraction(at, _stage);
				See(fx, fz);
			}
			InputState input = Game.Input;
			bool free = FreeToWalk();
			if (_open)
			{
				if (!free) Shut(false);   // the map is free walking's only (WSMove, WSVehicleMove): an event, a message, the menu end it
				else if (input.KeyPressed("M") || input.Pressed(Pad.B)) Shut(true);
			}
			else if (free && _map != null && input.KeyPressed("M"))
			{
				_open = true;
				Game.Audio.PlaySe(0, 1);
				Log.Write(LogChannel.File, "navimap: open - " + _map.Name);
			}
			if (_open) Draw(at);
			// Steam's hints over the field: "M Map / Tab Menu", gone while the party walks or anything else is up.
			// Steam's bar (FF4.exe, measured frame by frame): the whole bar - its band and keys - slides down out of sight as the
			// party starts walking and back up once it has stood half a second (30 of Steam's 60 frames), 20.25 of 1080's pixels
			// (6 of the game's) a frame either way; gone at once when anything else is up, as it was.
			// Counted in the game's steps (two of Steam's frames each), as Steam counts its frames.
			long step = LegacyStep.Count, steps = Math.Clamp(step - _lastStep, 0, 4);
			_lastStep = step;
			bool keys = free && !Flag(0, 4);
			if (!keys) _barDrop = BarHeight;
			else
			{
				bool up = _idle > 0 && step - _stillSince >= BarWait;
				_barDrop = Math.Clamp(_barDrop + (up ? -1 : 1) * BarStep * steps, 0f, BarHeight);
				if (_barDrop < BarHeight) DrawKeys();
			}
		}

		private void Shut(bool sound)
		{
			_open = false;
			if (sound) Game.Audio.PlaySe(0, 2);
		}

		// ---- drawing ----

		// The port's 800 x 480 from Steam's 1080p.
		private static float X(float x) => x / 2.4f;
		private static float Y(float y) => y / 2.25f;

		private void Draw(Vector3 at)
		{
			DrawList d = Game.Draw;
			Color fade = Color.White.WithAlpha((byte)Math.Round(255 * Alpha));
			Ff4Ui.Cell(d, "mapwaku.NCER", "mapwaku.NCGR", 0, X(FrameX), Y(FrameY), Ff4Ui.Scale, fade);
			DrawPicture(d, _map, MapLeft, fade, true);
			DrawIcons(d, _map, MapLeft, icon => (icon.Always || _map.SeenAt(icon.Fx, icon.Fz)) && (!icon.Place || Flag(0, icon.Flag)));
			if (_map.Kind == 'd' && _showPercent && _map.Total > 0)
			{
				// d_map_obj's glyphs (3..12 the digits, 13 %, 2 blank) at libff4's (326 / 334 / 342 / 353, 224) on its 480 x 320,
				// Steam's: x 960 + (x - 240) * 3.375, y 540.85 + (y - 160) * 3.375 (the "6 %" traced on the Waterway's next floor).
				int pct = _map.Percent();
				int[] cells = { pct >= 100 ? 4 : 2, pct >= 10 ? 3 + pct / 10 % 10 : 2, 3 + pct % 10, 13 };
				int[] xs = { 326, 334, 342, 353 };
				for (int k = 0; k < 4; k++)
					Ff4Ui.Cell(d, "d_map_obj.NCER", "d_map_obj.NCGR", cells[k], X(960f + (xs[k] - 240) * 3.375f), Y(540.85f + (224 - 160) * 3.375f), Ff4Ui.Scale);
			}
			if (_showPlayer) DrawPin(d, _map, MapLeft, _map.PartyFraction(at, _stage));
		}

		/// <summary>The picture where seen and the blank sheet elsewhere (a town has none), a 16 x 16 cell at a time, from <paramref name="left"/>.</summary>
		private static void DrawPicture(DrawList d, MapData m, float left, Color tint, bool fog)
		{
			Texture map = Ff4Ui.Sheet(m.Name + ".NCGR");
			Texture blank = !fog ? null : m.Kind == 'd' ? Ff4Ui.Sheet("dxx.NCGR") : m.Kind == 'f' ? Ff4Ui.Sheet("field_xx.NCGR") : null;
			float cw = 16 * Px / 2.4f, ch = 16 * Px / 2.25f;
			for (int r = 0; r < 24; r++)
			{
				for (int c = 0; c < 32; c++)
				{
					Texture sheet = m.Cells[r, c] ? map : blank;
					if (sheet == null) continue;
					d.Sprite(sheet, X(left + 16 * c * Px), Y(MapTop + 16 * r * Px), cw, ch, tint, 0f, 16 * c, 16 * r, 16, 16);
				}
			}
		}

		private void DrawIcons(DrawList d, MapData m, float left, Func<Icon, bool> shown)
		{
			foreach (Icon icon in m.Icons)
			{
				if (!shown(icon)) continue;
				int cell = icon.Cell;
				if (icon.OpenCell >= 0 && Flag(icon.FlagGroup, icon.Flag)) cell = icon.OpenCell;
				float y = MapTop + icon.Fz * 384 * Px;
				if (icon.Anim == 6)
				{
					// The exit's arrow: cells 14..17 six steps each, the third two sheet pixels up.
					int k = _frame / 6 % 4;
					cell = 14 + k;
					if (k == 2) y -= 2 * Px;
				}
				Ff4Ui.Cell(d, icon.Bank + ".NCER", icon.Bank + ".NCGR", cell, X(left + icon.Fx * 512 * Px), Y(y), Ff4Ui.Scale);
			}
		}

		/// <summary>The party's pin (w_map_mark, four frames of seven) at a place on the picture.</summary>
		private void DrawPin(DrawList d, MapData m, float left, (float Fx, float Fz) at)
		{
			Ff4Ui.Cell(d, "w_map_mark.NCER", "w_map_mark.NCGR", _frame / 7 % 4, X(left + at.Fx * 512 * Px), Y(MapTop + at.Fz * 384 * Px), Ff4Ui.Scale);
		}

		private void DrawKeys()
		{
			if (!Ff4MenuHud.Available) return;
			Ff4MenuHud.Data h = _hud;
			h.FieldKeys = true;
			h.FieldKeysDrop = _barDrop;
			h.MapOpen = _open;
			h.HasMap = _map != null;
			h.MapClosed = h.HasMap && !_open;
			// Steam's own hint lines (babil_menu.msd 60204 "%key_assign09%Map", 60213 "... Quit Map", 60205 "%key_assign11%Menu").
			h.MapLabel = _open ? Ff4Menu.KeyText(60213, "Quit Map") : Ff4Menu.KeyText(60204, "Map");
			h.MenuLabel = Ff4Menu.KeyText(60205, "Menu");
			Ff4MenuHud.Draw(h);
		}
	}
}
