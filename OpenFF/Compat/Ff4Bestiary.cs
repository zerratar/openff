// FF4's bestiary record and its book: mon::MonsterManiaManager and mr::MonsterRefDataManager.
//
// The record is a u16 per monster id (256): the kill count in the top 12 bits (to 999), and flags in the low four - bit 0
// met, bit 1 not looked at yet (the list's NEW; set with bit 0 on the first meeting, MonsterMania::onEncount). A battle
// meets each of its monsters (onEncount) and counts each one felled (kill); some monsters are one entry under several ids
// - an id of a group met or felled marks them all (the switch in onEncount / kill). The book is monster_refer.bbd: 32-byte
// records in the list's order - the monster id, its battle stage, its battle BGM, the camera's place and target for its
// picture, flags (+0x1c, bit 1: its level is not shown), the area's babil_monster_ref.msd line (+0x18). A monster is listed
// when its parameter's flag 0 is up (+0x92 of its 152 bytes); its name shows once met, its figures once felled.

using System;
using System.Collections.Generic;
using OpenFF;
using OpenFF.Data;

namespace OpenFF.Client
{
	internal static class Ff4Bestiary
	{
		private static readonly ushort[] _mania = new ushort[256];

		/// <summary>The ids that are one entry (MonsterManiaManager's switch).</summary>
		private static readonly int[][] Groups =
		{
			new[] { 96, 228, 229, 230, 231, 232, 233 }, new[] { 155, 156 }, new[] { 160, 234 }, new[] { 168, 223 }, new[] { 170, 171 },
			new[] { 175, 176 }, new[] { 188, 189 }, new[] { 198, 224 }, new[] { 199, 225 }, new[] { 200, 226 }, new[] { 210, 211 },
		};

		private static int[] Group(int id)
		{
			foreach (int[] g in Groups) if (Array.IndexOf(g, id) >= 0) return g;
			return new[] { id };
		}

		public static ushort Mania(int id) => id >= 0 && id < 256 ? _mania[id] : (ushort)0;

		public static int Kills(int id) => Mania(id) >> 4;

		public static bool Met(int id) => (Mania(id) & 1) != 0;

		public static bool New(int id) => (Mania(id) & 2) != 0;

		/// <summary>MonsterManiaManager::onEncount: a battle's monster met (its group with it).</summary>
		public static void Met(IEnumerable<int> ids)
		{
			foreach (int id in ids)
			{
				foreach (int g in Group(id))
				{
					if (g < 0 || g >= 256) continue;
					if ((_mania[g] & 1) == 0) _mania[g] |= 3;
				}
			}
		}

		/// <summary>MonsterManiaManager::kill: one felled (its group with it), each count held at 999.</summary>
		public static void Killed(int id)
		{
			foreach (int g in Group(id))
			{
				if (g < 0 || g >= 256) continue;
				if (_mania[g] >> 4 < 999) _mania[g] += 0x10;
			}
		}

		/// <summary>Its page looked at: NEW goes.</summary>
		public static void Seen(int id)
		{
			if (id >= 0 && id < 256) _mania[id] &= unchecked((ushort)~2);
		}

		public static void NewGame() => Array.Clear(_mania, 0, _mania.Length);

		public static Dictionary<int, int> ForSave()
		{
			Dictionary<int, int> d = new Dictionary<int, int>();
			for (int i = 0; i < 256; i++) if (_mania[i] != 0) d[i] = _mania[i];
			return d;
		}

		public static void Restore(Dictionary<int, int> d)
		{
			Array.Clear(_mania, 0, _mania.Length);
			if (d == null) return;
			foreach (KeyValuePair<int, int> e in d) if (e.Key >= 0 && e.Key < 256) _mania[e.Key] = (ushort)e.Value;
		}

		// ---- the book ----

		public sealed class Entry
		{
			public int Monster, Stage, Bgm, Flags;
			public uint Area;
			public int[] Camera = new int[4];   // the record's four words at +8 (the picture's camera)
			public byte[] Raw;
		}

		private static List<Entry> _book;

		/// <summary>monster_refer.bbd's records, the list's order.</summary>
		public static List<Entry> Book
		{
			get
			{
				if (_book != null) return _book;
				_book = new List<Entry>();
				try
				{
					if (GameArchive.Chain != null && TableFiles.ReadAny(GameArchive.Chain, "monster_refer.bbd", out byte[] d))
					{
						if (Lz.IsCompressed(d)) d = Lz.Decompress(d);
						for (int at = 0; at + 32 <= d.Length; at += 32)
						{
							byte[] r = new byte[32];
							Array.Copy(d, at, r, 0, 32);
							Entry e = new Entry
							{
								Monster = BitConverter.ToInt16(r, 0),
								Stage = BitConverter.ToInt16(r, 2),
								Bgm = BitConverter.ToInt16(r, 4),
								Area = BitConverter.ToUInt32(r, 0x18),
								Flags = BitConverter.ToUInt16(r, 0x1c),
								Raw = r,
							};
							for (int k = 0; k < 4; k++) e.Camera[k] = BitConverter.ToInt32(r, 8 + 4 * k);
							_book.Add(e);
						}
					}
				}
				catch (Exception ex) { Log.Write(LogChannel.General, "bestiary: monster_refer.bbd: " + ex.Message); }
				Log.Write(LogChannel.File, "bestiary: " + _book.Count + " entries");
				return _book;
			}
		}

		public static MonsterDefinition Definition(int id)
		{
			GameTables tables = Ff4Party.Tables;
			if (tables == null) return null;
			foreach (MonsterDefinition m in tables.Monsters) if (m.Id == id) return m;
			return null;
		}

		/// <summary>isEnableListup: the parameter's flag 0 (its u16 at +0x92).</summary>
		public static bool Listed(int id)
		{
			MonsterDefinition m = Definition(id);
			return m?.Raw != null && m.Raw.Length > 0x93 && (BitConverter.ToUInt16(m.Raw, 0x92) & 1) != 0;
		}

		/// <summary>setCompleteRate: the book's monsters met and felled, in percent of the whole book.</summary>
		public static int Complete()
		{
			List<Entry> book = Book;
			if (book.Count == 0) return 0;
			int done = 0;
			foreach (Entry e in book) if ((Mania(e.Monster) & 1) != 0 && Mania(e.Monster) > 0xF) done++;
			return done * 100 / book.Count;
		}
	}
}
