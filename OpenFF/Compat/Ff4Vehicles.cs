// FF4's vehicles' state (world::WSCVehicle): where each one stands and how it is turned, the Lunar Whale's spot on the
// Earth and on the Moon, and where the black chocobo flies back to - what the scripts set (setVehiclePosition,
// setVehicleRot, storeMagishipPos, loadMagishipPos, setBackChocoboBackPoint) and the save keeps. A script's vehicle n
// (1..9) is WSCVehicle's slot {0, 1, 1, 1, 2, 2, 3, 4, 5}[n - 1] (the table at 0x2bbf68): the vehicle types 0 the
// hovercraft, 1 the Enterprise, 2 the Falcon, 3 the Lunar Whale (the magiship), 4 the chocobo, 5 the black chocobo. Each
// stays on one world (wscVehicleStayingMap: 0 the Earth, 1 the Underground, 2 the Moon - the Falcon starts below). Riding
// them is Ff4Ride's.

using System;
using System.Collections.Generic;

namespace OpenFF.Client
{
	internal static class Ff4Vehicles
	{
		private static readonly int[] SlotOf = { 0, 1, 1, 1, 2, 2, 3, 4, 5 };
		private const int Slots = 8, Magiship = 3;

		public sealed class Saved
		{
			/// <summary>Per slot: x, y, z (fx32) and the turn about x, y, z (16-bit angles).</summary>
			public int[][] Places;
			public int[] MagishipEarth, MagishipMoon, BlackChocoboBack;
			/// <summary>The world each vehicle type stays on (0 Earth, 1 Underground, 2 Moon).</summary>
			public int[] Stay;
			/// <summary>The Enterprise has the hovercraft on its hook (wscGetLiftHoverFlag).</summary>
			public bool LiftHover;
		}

		private static int[][] _places = NewPlaces();
		private static int[] _earth = new int[3], _moon = new int[3], _chocoboBack = new int[3];
		private static readonly int[] DefaultStay = { 0, 0, 1, 0, 0, 0 };
		private static int[] _stay = (int[])DefaultStay.Clone();

		/// <summary>The vehicle type the next world map is arrived at riding (wscGetVehicleMapjump: a worldMapWarp's vehicle),
		/// -1 for none.</summary>
		public static int ArriveOn = -1;

		/// <summary>The Enterprise carries the hovercraft (wscGetLiftHoverFlag).</summary>
		public static bool LiftHover;

		/// <summary>The magiship's trip between the worlds the next arrival ends (WSCMapJump's 0x208): 0 from the Earth to the
		/// Moon, 1 back; -1 for none.</summary>
		public static int MoonTrip = -1;

		/// <summary>WSPrepare::wsProcess's names for a jump's map: "back_from_magiship" (the world the magiship stays on, at
		/// its place), "goto_moon" / "goto_earth" (the trip's arrival), "come_from_moon" / "come_from_earth" (aboard it),
		/// "d12_99" (d12_17). False for any other map.</summary>
		public static bool ResolveMap(ref string map, ref GlobalScope.VecFx32 pos)
		{
			switch (map)
			{
				case "back_from_magiship":
					map = _stay[Magiship] == 2 ? "f02" : "f00";
					pos = new GlobalScope.VecFx32(_places[Magiship][0], _places[Magiship][1], _places[Magiship][2]);
					return true;
				case "goto_moon": map = "f02"; MoonTrip = 0; return true;
				case "goto_earth": map = "f00"; MoonTrip = 1; return true;
				case "come_from_moon": ArriveOn = Magiship; map = "f00"; return true;
				case "come_from_earth": ArriveOn = Magiship; map = "f02"; return true;
				case "d12_99": map = "d12_17"; return true;
				default: return false;
			}
		}

		/// <summary>A type's place: x, y, z (fx32) and the turn about x, y, z (16-bit angles).</summary>
		public static int[] Place(int type) => type >= 0 && type < Slots ? _places[type] : null;

		public static void SetPlace(int type, int x, int y, int z, int rotY)
		{
			if (type < 0 || type >= Slots) return;
			_places[type][0] = x; _places[type][1] = y; _places[type][2] = z;
			_places[type][4] = rotY & 0xFFFF;
		}

		/// <summary>The world a type stays on (wscVehicleStayingMap).</summary>
		public static int Stay(int type) => type >= 0 && type < 6 ? _stay[type] : -1;

		public static void SetStay(int type, int world)
		{
			if (type >= 0 && type < 6 && world >= 0 && world <= 2) _stay[type] = world;
		}

		/// <summary>A script's vehicle number (1..9) as a type, -1 for none.</summary>
		public static int TypeOf(uint n) => Slot(n);

		/// <summary>object::checkAcquiredVehicle: the flag (or flags) that say the party has the vehicle.</summary>
		public static bool Acquired(int type)
		{
			bool F(int i) => GlobalScope.FlagManager.singleton().get(0, (uint)i) != 0;
			switch (type)
			{
				case 0: return F(900);
				case 1: return F(901) || F(902) || F(903);
				case 2: return F(904) || F(905);
				case 3: return F(906);
				case 4: return F(907);
				case 5: return F(908);
				default: return false;
			}
		}

		private static int[][] NewPlaces()
		{
			int[][] places = new int[Slots][];
			for (int i = 0; i < Slots; i++) places[i] = new int[6];
			return places;
		}

		public static void NewGame()
		{
			_places = NewPlaces();
			_earth = new int[3];
			_moon = new int[3];
			_chocoboBack = new int[3];
			_stay = (int[])DefaultStay.Clone();
			ArriveOn = -1;
			LiftHover = false;
			MoonTrip = -1;
		}

		private static int Slot(uint n) => n >= 1 && n <= SlotOf.Length ? SlotOf[n - 1] : -1;

		/// <summary>FX_Mod(degrees, 360) as FF4 turns it into a 16-bit angle.</summary>
		private static int Angle(int degreesFx)
		{
			long mod = ((long)degreesFx % 0x168000 + 0x168000) % 0x168000;
			return (int)(mod * 65536 / 0x168000) & 0xFFFF;
		}

		/// <summary>setVehiclePosition(n, x, y, z, rx, ry, rz): babilCommand_SetVehiclePosition - the place and the turn
		/// (degrees, z negated).</summary>
		public static void SetVehiclePosition(GlobalScope.ScriptEngine engine)
		{
			uint n = engine.getDword();
			int x = (int)engine.getDword(), y = (int)engine.getDword(), z = (int)engine.getDword();
			int rx = (int)engine.getDword(), ry = (int)engine.getDword(), rz = (int)engine.getDword();
			int slot = Slot(n);
			if (slot < 0) { Log.First(LogChannel.General, "ff4-vehicle-n", 3, () => "script: setVehiclePosition " + n + " - no such vehicle"); return; }
			_places[slot] = new[] { x, y, z, Angle(rx), Angle(ry), Angle(-rz) };
			Log.Write(LogChannel.File, "script: FF4 vehicle " + n + " (slot " + slot + ") at " + x / 4096 + "," + y / 4096 + "," + z / 4096);
		}

		/// <summary>setVehicleRot(n, rx, ry, rz): the turn alone, as 16-bit angles.</summary>
		public static void SetVehicleRot(GlobalScope.ScriptEngine engine)
		{
			uint n = engine.getDword();
			int rx = (int)engine.getDword() & 0xFFFF, ry = (int)engine.getDword() & 0xFFFF, rz = (int)engine.getDword() & 0xFFFF;
			int slot = Slot(n);
			if (slot < 0) return;
			_places[slot][3] = rx;
			_places[slot][4] = ry;
			_places[slot][5] = rz;
		}

		/// <summary>storeMagishipPos(select, ...): the magiship's place kept for the Earth (0) or the Moon (1).</summary>
		public static void StoreMagishipPos(GlobalScope.ScriptEngine engine)
		{
			uint select = engine.getDword();
			engine.getDword(); engine.getDword();
			int[] at = { _places[Magiship][0], _places[Magiship][1], _places[Magiship][2] };
			if (select == 0) _earth = at;
			else if (select == 1) _moon = at;
		}

		/// <summary>loadMagishipPos(select, ...): the magiship back where it was kept for the Earth (0) or the Moon (1).</summary>
		public static void LoadMagishipPos(GlobalScope.ScriptEngine engine)
		{
			uint select = engine.getDword();
			engine.getDword(); engine.getDword();
			int[] at = select == 0 ? _earth : select == 1 ? _moon : null;
			if (at == null) return;
			_places[Magiship][0] = at[0];
			_places[Magiship][1] = at[1];
			_places[Magiship][2] = at[2];
		}

		/// <summary>Where the black chocobo flies home to (wscGetBlackChocoboBackPosition).</summary>
		public static int[] BlackChocoboBack => _chocoboBack;

		/// <summary>setBackChocoboBackPoint(x, y, z, ...): where the black chocobo flies home to.</summary>
		public static void SetBackChocoboBackPoint(GlobalScope.ScriptEngine engine)
		{
			_chocoboBack = new[] { (int)engine.getDword(), (int)engine.getDword(), (int)engine.getDword() };
			engine.getDword(); engine.getDword();
		}

		public static Saved ForSave() => new Saved { Places = _places, MagishipEarth = _earth, MagishipMoon = _moon, BlackChocoboBack = _chocoboBack, Stay = _stay, LiftHover = LiftHover };

		public static void Restore(Saved s)
		{
			NewGame();
			if (s == null) return;
			if (s.Places != null)
			{
				for (int i = 0; i < Slots && i < s.Places.Length; i++)
				{
					if (s.Places[i] != null && s.Places[i].Length == 6) _places[i] = (int[])s.Places[i].Clone();
				}
			}
			if (s.MagishipEarth?.Length == 3) _earth = (int[])s.MagishipEarth.Clone();
			if (s.MagishipMoon?.Length == 3) _moon = (int[])s.MagishipMoon.Clone();
			if (s.BlackChocoboBack?.Length == 3) _chocoboBack = (int[])s.BlackChocoboBack.Clone();
			if (s.Stay?.Length == 6) _stay = (int[])s.Stay.Clone();
			LiftHover = s.LiftHover;
		}
	}
}
