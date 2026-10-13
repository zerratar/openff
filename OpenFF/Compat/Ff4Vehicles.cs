// FF4's vehicles' state (world::WSCVehicle): where each one stands and how it is turned, the Lunar Whale's spot on the
// Earth and on the Moon, and where the black chocobo flies back to - what the scripts set (setVehiclePosition,
// setVehicleRot, storeMagishipPos, loadMagishipPos, setBackChocoboBackPoint) and the save keeps. A script's vehicle n
// (1..9) is WSCVehicle's slot {0, 1, 1, 1, 2, 2, 3, 4, 5}[n - 1] (the table at 0x2bbf68); slot 3 is the magiship. FF4's
// riding is not in yet: this is the state it will read.

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
		}

		private static int[][] _places = NewPlaces();
		private static int[] _earth = new int[3], _moon = new int[3], _chocoboBack = new int[3];

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

		/// <summary>setBackChocoboBackPoint(x, y, z, ...): where the black chocobo flies home to.</summary>
		public static void SetBackChocoboBackPoint(GlobalScope.ScriptEngine engine)
		{
			_chocoboBack = new[] { (int)engine.getDword(), (int)engine.getDword(), (int)engine.getDword() };
			engine.getDword(); engine.getDword();
		}

		public static Saved ForSave() => new Saved { Places = _places, MagishipEarth = _earth, MagishipMoon = _moon, BlackChocoboBack = _chocoboBack };

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
		}
	}
}
