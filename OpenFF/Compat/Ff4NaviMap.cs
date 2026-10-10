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

using System;
using System.Collections.Generic;
using OpenFF.Content;
using OpenFF;

namespace OpenFF.Client
{
	internal sealed class Ff4NaviMap : GameService
	{
		public override bool WantsUpdate => true;

		internal static Ff4NaviMap Instance { get; private set; }
		public Ff4NaviMap() { Instance = this; }

		// The seen cells of every map visited, by map name: 24 rows of 32 bits, cell c = bit 31 - c (NavimapSaveData's order).
		private static readonly Dictionary<string, uint[]> _seen = new Dictionary<string, uint[]>(StringComparer.OrdinalIgnoreCase);

		private sealed class Icon { public string Bank; public int Cell = -1; public int Anim; public float Fx, Fz; public int Flag = -1; public int OpenCell = -1; public bool Always; }

		private string _stage;              // the field stage the data is for
		private string _name;               // the navimap's map name (after the rewrites), null: no Map here
		private char _kind;                 // 'd' dungeon, 't' town, 'f' world
		private bool _showPlayer, _track;
		private int _stageW = 1, _stageH = 1, _corrX, _corrZ, _completeFlag = -1, _total;
		private readonly bool[,] _walk = new bool[24, 32];
		private readonly bool[,] _cells = new bool[24, 32];
		private readonly List<Icon> _icons = new List<Icon>();
		private bool _open;
		private int _frame, _idle;
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

		// ---- the map's data ----

		private static bool Flag(int group, int index)
		{
			byte[,] flags = GlobalScope.flags;
			return flags != null && group < flags.GetLength(0) && index >= 0 && index < flags.GetLength(1) && flags[group, index] != 0;
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
			_name = NaviName(stage, out _showPlayer);
			_icons.Clear();
			_completeFlag = -1;
			_track = false;
			Array.Clear(_walk, 0, _walk.Length);
			if (_name == null || Ff4Ui.ReadFile(_name + "_00.NSCR") == null || Ff4Ui.Sheet(_name + ".NCGR") == null) { _name = null; return; }
			_kind = _name[0];
			byte[] nmi = Ff4Ui.ReadFile(_name + ".nmi");
			if (nmi != null) ReadNmi(nmi);
			byte[] nmd = _kind != 't' ? Ff4Ui.ReadFile(_name + ".nmd") : null;
			_total = 0;
			if (nmd != null && nmd.Length >= 2 + 2 * 768)
			{
				_total = S16(nmd, 0);
				for (int r = 0; r < 24; r++) for (int c = 0; c < 32; c++) _walk[r, c] = S16(nmd, 2 + 2 * (r * 32 + c)) == 1;
			}
			// The world (but its underworld) wraps at its edges; a town is seen whole; a map not tracked is seen afresh.
			Array.Clear(_cells, 0, _cells.Length);
			if (_kind == 't') { for (int r = 0; r < 24; r++) for (int c = 0; c < 32; c++) _cells[r, c] = true; }
			else if (_seen.TryGetValue(_name, out uint[] rows)) { for (int r = 0; r < 24; r++) for (int c = 0; c < 32; c++) _cells[r, c] = (rows[r] >> (31 - c) & 1) != 0; }
			Log.Write(LogChannel.File, "navimap: " + stage + " -> " + _name + " (" + _icons.Count + " icon(s), " + _total + " cell(s) to see)");
		}

		private void ReadNmi(byte[] b)
		{
			if (_kind == 'd')
			{
				int chests = Math.Min(30, (int)S16(b, 0)), exits = Math.Min(20, (int)S16(b, 4));
				_stageW = Math.Max(1, (int)S16(b, 6)); _stageH = Math.Max(1, (int)S16(b, 8));
				_corrX = S16(b, 0xA); _corrZ = S16(b, 0xC);
				int spX = S16(b, 0xE), spZ = S16(b, 0x10);
				_completeFlag = S16(b, 0x14);
				_track = S16(b, 0x16) == 1;
				for (int i = 0; i < chests; i++)
					_icons.Add(At(new Icon { Bank = "map_mark_common", Cell = 1, OpenCell = 2, Flag = S16(b, 0x18 + 6 * i + 4) }, S16(b, 0x18 + 6 * i), S16(b, 0x18 + 6 * i + 2)));
				for (int i = 0; i < exits; i++)
					_icons.Add(At(new Icon { Bank = "d_map_obj", Anim = 6 }, S16(b, 0xCC + 4 * i), S16(b, 0xCC + 4 * i + 2)));
				if (spX != 0 || spZ != 0) _icons.Add(At(new Icon { Bank = "d_map_obj", Cell = 0 }, spX, spZ));
			}
			else if (_kind == 't')
			{
				int n = S16(b, 0), hide = S16(b, 0x1A);
				_stageW = Math.Max(1, (int)S16(b, 0x1E)); _stageH = Math.Max(1, (int)S16(b, 0x20));
				_corrX = S16(b, 0x22); _corrZ = S16(b, 0x24);
				int[] shopCells = { 3, 0, 6, 1, 2, 4 };
				for (int i = 0; i < 6; i++)
				{
					if ((hide >> i & 1) != 0 || i == 4) continue;
					_icons.Add(At(new Icon { Bank = "t_map_obj", Cell = shopCells[i], Always = true }, S16(b, 2 + 2 * i), S16(b, 0xE + 2 * i)));
				}
				for (int i = 0; i < n && 0x26 + 8 * i + 8 <= b.Length; i++)
				{
					if (S16(b, 0x26 + 8 * i + 6) != 0) continue;   // NMIHiddenItem: never drawn
					_icons.Add(At(new Icon { Bank = "map_mark_common", Cell = 1, OpenCell = 2, Flag = S16(b, 0x26 + 8 * i + 4), Always = true }, S16(b, 0x26 + 8 * i), S16(b, 0x26 + 8 * i + 2)));
				}
			}
			else
			{
				int n = S16(b, 0);
				_stageW = Math.Max(1, (int)S16(b, 2)); _stageH = Math.Max(1, (int)S16(b, 4));
				_corrX = S16(b, 6); _corrZ = S16(b, 8);
				int[] placeCells = { 2, 0, 3, 4 };
				for (int i = 0; i < n && 10 + 8 * i + 8 <= b.Length; i++)
				{
					int kind = S16(b, 10 + 8 * i + 4);
					Icon icon = At(new Icon { Bank = "w_map_obj", Cell = placeCells[Math.Clamp(kind, 0, 3)] }, S16(b, 10 + 8 * i), S16(b, 10 + 8 * i + 2));
					icon.Flag = S16(b, 10 + 8 * i + 6);   // shown once flag 0:id is set
					_icons.Add(icon);
				}
			}
		}

		/// <summary>A field position as the picture's fraction across and down (nmi_visit_update_position).</summary>
		private (float Fx, float Fz) Fraction(float x, float z) => ((x + _corrX + _stageW / 2f) / _stageW, (z + _corrZ + _stageH / 2f) / _stageH);

		private Icon At(Icon icon, int x, int z)
		{
			(icon.Fx, icon.Fz) = Fraction(x, z);
			return icon;
		}

		private bool SeenAt(float fx, float fz)
		{
			int c = (int)Math.Floor(fx * 32), r = (int)Math.Floor(fz * 24);
			return c >= 0 && c < 32 && r >= 0 && r < 24 && _cells[r, c];
		}

		/// <summary>PassagePointChange: the 31 cells about the party's are seen where they count; kept, and a floor all seen complete.</summary>
		private void See(float fx, float fz)
		{
			if (_kind == 't') return;
			int cx = (int)Math.Floor(fx * 32), cy = (int)Math.Floor(fz * 24);
			bool wrap = _kind == 'f' && _name.Length > 7 && _name[7] != '1', changed = false;
			foreach ((int dx, int dy) in Blob)
			{
				int c = cx + dx, r = cy + dy;
				if (wrap) { c = (c % 32 + 32) % 32; r = (r % 24 + 24) % 24; }
				else { c = Math.Clamp(c, 0, 31); r = Math.Clamp(r, 0, 23); }
				if (_walk[r, c] && !_cells[r, c]) { _cells[r, c] = true; changed = true; }
			}
			if (!changed) return;
			int seen = 0;
			for (int r = 0; r < 24; r++) for (int c = 0; c < 32; c++) if (_cells[r, c] && _walk[r, c]) seen++;
			if (_total > 0 && seen * 100 / _total >= 100)
			{
				// MapPercentUpDate at 100: the whole picture shown; the floor complete (wsmNaviMapComp sets its flag).
				for (int r = 0; r < 24; r++) for (int c = 0; c < 32; c++) _cells[r, c] = true;
				byte[,] flags = GlobalScope.flags;
				if (_kind == 'd' && _completeFlag >= 0 && flags != null && 1 < flags.GetLength(0) && _completeFlag < flags.GetLength(1) && flags[1, _completeFlag] == 0)
				{
					flags[1, _completeFlag] = 1;
					Log.Write(LogChannel.General, "navimap: " + _name + " complete (flag 1:" + _completeFlag + ")");
				}
			}
			if (_kind == 'd' && !_track) return;   // this floor's sight is not kept (d04_04)
			uint[] rows = new uint[24];
			for (int r = 0; r < 24; r++) for (int c = 0; c < 32; c++) if (_cells[r, c]) rows[r] |= 1u << (31 - c);
			_seen[_name] = rows;
		}

		// ---- each step ----

		private static bool FreeToWalk() => EngineApi.InWorld && Game.Hero.Present && !Ff4Battle.Active && !Ff4Cutscene.Active && !Game.Dialogue.IsOpen
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
			if (_name != null && Game.Hero.Present && !Ff4Cutscene.Active)
			{
				(float fx, float fz) = Fraction(at.X, at.Z);
				See(fx, fz);
			}
			InputState input = Game.Input;
			bool free = FreeToWalk();
			if (_open)
			{
				if (!free) Shut(false);   // the map is free walking's only (WSMove, WSVehicleMove): an event, a message, the menu end it
				else if (input.KeyPressed("M") || input.Pressed(Pad.B)) Shut(true);
			}
			else if (free && _name != null && input.KeyPressed("M"))
			{
				_open = true;
				Game.Audio.PlaySe(0, 1);
				Log.Write(LogChannel.File, "navimap: open - " + _name);
			}
			if (_open) Draw(at);
			// Steam's hints over the field: "M Map / Tab Menu", gone while the party walks or anything else is up.
			if (free && _idle >= 4 && !Flag(0, 4)) DrawKeys();
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
			Texture map = Ff4Ui.Sheet(_name + ".NCGR");
			Texture fog = _kind == 'd' ? Ff4Ui.Sheet("dxx.NCGR") : _kind == 'f' ? Ff4Ui.Sheet("field_xx.NCGR") : null;
			float cw = 16 * Px / 2.4f, ch = 16 * Px / 2.25f;
			for (int r = 0; r < 24; r++)
			{
				for (int c = 0; c < 32; c++)
				{
					Texture sheet = _cells[r, c] ? map : fog;
					if (sheet == null) continue;
					d.Sprite(sheet, X(MapLeft + 16 * c * Px), Y(MapTop + 16 * r * Px), cw, ch, fade, 0f, 16 * c, 16 * r, 16, 16);
				}
			}
			foreach (Icon icon in _icons)
			{
				if (!icon.Always && !SeenAt(icon.Fx, icon.Fz)) continue;
				if (icon.Bank == "w_map_obj" && !Flag(0, icon.Flag)) continue;
				int cell = icon.Cell;
				if (icon.OpenCell >= 0 && Flag(1, icon.Flag)) cell = icon.OpenCell;
				float y = MapTop + icon.Fz * 384 * Px;
				if (icon.Anim == 6)
				{
					// The exit's arrow: cells 14..17 six steps each, the third two sheet pixels up.
					int k = _frame / 6 % 4;
					cell = 14 + k;
					if (k == 2) y -= 2 * Px;
				}
				Ff4Ui.Cell(d, icon.Bank + ".NCER", icon.Bank + ".NCGR", cell, X(MapLeft + icon.Fx * 512 * Px), Y(y), Ff4Ui.Scale);
			}
			if (_showPlayer)
			{
				(float fx, float fz) = Fraction(at.X, at.Z);
				Ff4Ui.Cell(d, "w_map_mark.NCER", "w_map_mark.NCGR", _frame / 7 % 4, X(MapLeft + fx * 512 * Px), Y(MapTop + fz * 384 * Px), Ff4Ui.Scale);
			}
		}

		private void DrawKeys()
		{
			if (!Ff4MenuHud.Available) return;
			Ff4MenuHud.Data h = _hud;
			h.FieldKeys = true;
			h.MapOpen = _open;
			h.HasMap = _name != null;
			h.MapClosed = h.HasMap && !_open;
			// Steam's own hint lines (babil_menu.msd 60204 "%key_assign09%Map", 60213 "... Quit Map", 60205 "%key_assign11%Menu").
			h.MapLabel = _open ? Ff4Menu.KeyText(60213, "Quit Map") : Ff4Menu.KeyText(60204, "Map");
			h.MenuLabel = Ff4Menu.KeyText(60205, "Menu");
			Ff4MenuHud.Draw(h);
		}
	}
}
