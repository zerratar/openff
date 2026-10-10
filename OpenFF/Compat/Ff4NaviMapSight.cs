// The Sight spell's screen as Steam's (world::MSSSightro, SightroListDataManager): a floor's map whole in a window on the
// left - its chests, exits and save ring - and down the right the floors of the dungeon the party has been on (SIGHTRO.dat's
// dNN.bbd rows, named from babil_sightoro.msd), Up / Down showing another at once; the party's pin on its own floor's map
// and at the end of that floor's row. In a town or on the world: the world's map whole, the party (or the town) pinned,
// the list the world's places found (fNN.bbd), the lit one's icon alone on the map. The menu's Magic screen opens it
// (Ff4Menu's Screen.Sight); placed as Steam draws it (libff4's 480 x 320 layout x 3.375, 150 px in: the picture from
// (155.5, 155.2) at 1.6875, the row pin at (1662, 81 + 135 row)). No entry for the stage (towns t19..t21, t23 on, d18):
// no Sight, where libff4 would stop.

using System;
using System.Collections.Generic;
using System.Text;
using OpenFF;

namespace OpenFF.Client
{
	internal sealed partial class Ff4NaviMap
	{
		private sealed class SightRow { public string Text; public string Map; public int Place; }

		private static Dictionary<string, (int Offset, int Size)> _sightroEntries;
		private static byte[] _sightro;

		private bool _sightOpen, _sightWorld;
		private readonly List<SightRow> _sightRows = new List<SightRow>();
		private int _sightSelected = -1, _sightPinRow = -1;
		private MapData _sightMap;
		private (float Fx, float Fz)? _sightPin;
		private const float SightLeft = 155.5f;

		/// <summary>SIGHTRO.dat's entry by name (d01.bbd, f00.bbd, town.bbd): the SSAM index's data lies in the file itself.</summary>
		private static byte[] Sightro(string name)
		{
			if (_sightroEntries == null)
			{
				_sightroEntries = new Dictionary<string, (int, int)>(StringComparer.OrdinalIgnoreCase);
				_sightro = Ff4Ui.ReadFile("SIGHTRO.dat");
				if (_sightro != null && _sightro.Length >= 8 && Encoding.ASCII.GetString(_sightro, 0, 4) == "SSAM")
				{
					int count = BitConverter.ToInt32(_sightro, 4), data = 8 + 40 * count;
					for (int i = 0; i < count && 8 + 40 * i + 40 <= _sightro.Length; i++)
					{
						int off = BitConverter.ToInt32(_sightro, 8 + 40 * i), size = BitConverter.ToInt32(_sightro, 8 + 40 * i + 4);
						string n = Encoding.ASCII.GetString(_sightro, 8 + 40 * i + 8, 32).TrimEnd('\0');
						_sightroEntries[n] = (data + off, size);
					}
				}
			}
			if (!_sightroEntries.TryGetValue(name, out (int Offset, int Size) e) || e.Offset + e.Size > _sightro.Length) return null;
			byte[] b = new byte[e.Size];
			Buffer.BlockCopy(_sightro, e.Offset, b, 0, e.Size);
			return b;
		}

		/// <summary>town.bbd: a town's world and its place on it (fx32), t17's by the flags that move it.</summary>
		private static (int World, float X, float Z)? TownPlace(int town)
		{
			byte[] b = Sightro("town.bbd");
			if (b == null || b.Length < 2) return null;
			int count = b[0];
			for (int i = 0; i < count && 0x0C + 12 * i + 12 <= b.Length; i++)
			{
				int at = 0x0C + 12 * i;
				if (b[at] != town) continue;
				float x = BitConverter.ToInt32(b, at + 4) / 4096f, z = BitConverter.ToInt32(b, at + 8) / 4096f;
				if (town == 17)
				{
					if (Flag(0, 0x353)) (x, z) = (1475, -865);
					else if (Flag(0, 0x354)) (x, z) = (1265, 1089);
					else if (Flag(0, 0x355)) (x, z) = (-1165, -228);
					else if (Flag(0, 0x356)) (x, z) = (-448, -490);
				}
				return (b[at + 1], x, z);
			}
			return null;
		}

		/// <summary>SightroListDataManager::analyze for the stage: the rows, the world's or the dungeon's; false: no Sight here.</summary>
		private bool BuildSight(string stage)
		{
			_sightRows.Clear();
			_sightPinRow = -1;
			_sightPin = null;
			if (string.IsNullOrEmpty(stage) || stage.Length < 3) return false;
			string s = stage.ToLowerInvariant();
			int world = -1;
			(float X, float Z)? townAt = null;
			if (s[0] == 't' && int.TryParse(s.Substring(1, 2), out int town))
			{
				if (!(TownPlace(town) is (int w, float x, float z))) return false;
				world = w;
				townAt = (x, z);
			}
			else if (s[0] == 'f' && int.TryParse(s.Substring(1, 2), out int f)) world = f;
			else if (s[0] != 'd') return false;
			string file = world >= 0 ? "f" + world.ToString("00") + ".bbd" : s.Substring(0, 3) + ".bbd";
			byte[] b = Sightro(file);
			if (b == null || b.Length < 8) return false;
			int rows = b[2];
			for (int i = 0; i < rows && 8 + 12 * i + 12 <= b.Length; i++)
			{
				int a = BitConverter.ToInt32(b, 8 + 12 * i), msg = BitConverter.ToInt32(b, 8 + 12 * i + 4), flag = BitConverter.ToInt32(b, 8 + 12 * i + 8);
				if (flag != -1 && !Flag(0, flag)) continue;
				SightRow row = new SightRow { Text = Ff4Layouts.SightText((uint)msg, "???") };
				if (world >= 0) row.Place = a;
				else
				{
					row.Map = s.Substring(0, 3) + "_" + a.ToString("00");
					if (row.Map == "d12_99" ? !Flag(0, 0x3D1) : !Visited(row.Map)) continue;
				}
				_sightRows.Add(row);
				if (_sightRows.Count >= 30) break;
			}
			_sightWorld = world >= 0;
			if (_sightWorld)
			{
				_sightMap = MapData.Read("field_" + world.ToString("00") + "_00");
				if (_sightMap == null) return false;
				_sightMap.SeeAll();
				if (townAt is (float tx, float tz)) _sightPin = _sightMap.Fraction(tx, tz);
				else if (Game.Hero.Present) _sightPin = _sightMap.PartyFraction(Game.Hero.Position, stage);
				return _sightRows.Count > 0;
			}
			if (_sightRows.Count == 0) return false;
			string here = NaviName(stage, out _);
			_sightPinRow = _sightRows.FindIndex(r => string.Equals(r.Map, here, StringComparison.OrdinalIgnoreCase));
			return true;
		}

		/// <summary>Whether Sight has anything to show here (the menu greys and refuses it otherwise).</summary>
		internal bool SightUsable() => EngineApi.InWorld && BuildSight(Game.Field.Map);

		/// <summary>The Sight screen up: its rows, the dungeon's own floor lit (else the first), the world with no place lit yet.</summary>
		internal bool OpenSight()
		{
			if (!BuildSight(Game.Field.Map)) return false;
			_sightOpen = true;
			_sightSelected = -1;
			SightSelect(_sightWorld ? -1 : Math.Max(0, _sightPinRow));
			Log.Write(LogChannel.File, "sight: " + _sightRows.Count + " row(s)" + (_sightWorld ? " of the world" : ", the party's floor " + _sightPinRow));
			return true;
		}

		internal void CloseSight() { _sightOpen = false; _sightMap = _sightWorld ? _sightMap : null; }

		internal int SightCount => _sightRows.Count;
		internal string SightText(int row) => row >= 0 && row < _sightRows.Count ? _sightRows[row].Text : "";
		internal int SightSelected => _sightSelected;

		/// <summary>A row lit: a dungeon's floor shown whole (FUN_004f60bc), or the world's place picked out (msssDecideTownField).</summary>
		internal void SightSelect(int row)
		{
			_sightSelected = row;
			if (_sightWorld || row < 0 || row >= _sightRows.Count) return;
			_sightMap = MapData.Read(_sightRows[row].Map);
			_sightMap?.SeeAll();
			_sightPin = null;
			if (_sightMap != null && row == _sightPinRow && Game.Hero.Present) _sightPin = _sightMap.PartyFraction(Game.Hero.Position, Game.Field.Map);
		}

		/// <summary>The map in the Sight screen's window (after its layout): the picture whole, its icons, the pins.</summary>
		internal void DrawSight(int topRow)
		{
			if (!_sightOpen || _sightMap == null) return;
			DrawList d = Game.Draw;
			MapData m = _sightMap;
			DrawPicture(d, m, SightLeft, Color.White, false);
			int place = _sightWorld && _sightSelected >= 0 && _sightSelected < _sightRows.Count ? _sightRows[_sightSelected].Place : -1;
			int placeIndex = 0;
			DrawIcons(d, m, SightLeft, icon => !icon.Place || ++placeIndex == place);
			if (_sightPin is (float fx, float fz)) DrawPin(d, m, SightLeft, (fx, fz));
			// The small pin at the end of the party's floor's row (w_map_mark at libff4's (448, 24 + 40 row)).
			int shownRow = _sightPinRow - topRow;
			if (!_sightWorld && shownRow >= 0 && shownRow < 7)
				Ff4Ui.Cell(d, "w_map_mark.NCER", "w_map_mark.NCGR", 0, X(150f + 448 * 3.375f), Y((24 + 40 * shownRow) * 3.375f), Ff4Ui.Scale);
		}
	}
}
