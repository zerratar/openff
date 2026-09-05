// FF4's random encounters, from the map's parameter pack.
//
// Each FF4 map has a .pak in MAPPARAMETER.dat with four single records. The first
// (encountParameter, 52 bytes) holds the encounter rate as the u16 at 0 (11 in the Mist
// cave, 0 in a town); the third (monsterPartyParameter, 16 bytes) holds the first encounter
// group as a u32 and three cumulative percentages as floats: a roll under the first gives
// the first group, under the second the next, under the third the one after, else the
// fourth (d01_00: 10 with 45/55/65 - three Goblins, three Goblins, a Goblin, a Sword Rat).
// The twelve u16s from 0x18 of the first record (8 in the Mist cave) are not the groups.
// The overworld's pack holds one such four-record pack per chip (256 x 192 bytes); the chip
// under the party picks the record; its rate is not read yet (6 stands in).

using System;
using System.Collections.Generic;
using OpenFF.Data;

namespace FF3
{
	internal static class Ff4Encounters
	{
		public sealed class Table
		{
			public string Map;
			public int Rate;
			/// <summary>The first encounter group; the roll adds 0..3.</summary>
			public int BaseParty;
			/// <summary>Cumulative percentages for groups base, base+1, base+2; the rest is base+3.</summary>
			public float[] Thresholds = new float[3];
			public List<int> Parties = new List<int>();

			public int Roll(Random random)
			{
				if (Parties.Count == 0) return -1;
				double r = random.NextDouble() * 100.0;
				int k = r < Thresholds[0] ? 0 : r < Thresholds[1] ? 1 : r < Thresholds[2] ? 2 : 3;
				return Parties[Math.Min(k, Parties.Count - 1)];
			}
		}

		private static readonly Dictionary<string, Table> _cache = new Dictionary<string, Table>(StringComparer.OrdinalIgnoreCase);

		public static Table For(string map)
		{
			if (string.IsNullOrEmpty(map))
			{
				// The overworld's stage carries no name of its own; its chips do (f00_48).
				string chipName = null;
				try { chipName = GlobalScope.stageMng?.getChipName(); } catch (Exception) { }
				if (string.IsNullOrEmpty(chipName) || chipName.Length < 6) return null;
				map = chipName.Substring(0, 3);
			}
			// The overworld: one 192-byte record per chip, each a four-record pack of its own; the
			// chip under the party is the stage manager's (f00_48 -> record 0x48).
			string key = map;
			int chip = -1;
			if (map.Length == 3 && (map[0] == 'f' || map[0] == 'F'))
			{
				try
				{
					string chipName = GlobalScope.stageMng?.getChipName();
					if (!string.IsNullOrEmpty(chipName) && chipName.Length >= 6 && int.TryParse(chipName.Substring(4, 2), System.Globalization.NumberStyles.HexNumber, null, out int index))
					{
						chip = index;
						key = chipName;
					}
				}
				catch (Exception) { }
				if (chip < 0)
				{
					if (!_cache.ContainsKey(map)) { _cache[map] = null; Log.Write(LogChannel.File, "encounters: " + map + ": no chip name from the stage (" + (GlobalScope.stageMng?.getChipName() ?? "null") + ")"); }
					return null;
				}
			}
			if (_cache.TryGetValue(key, out Table have)) return have;
			Table table = new Table { Map = key };
			try
			{
				if (GameArchive.Chain != null && TableFiles.ReadAny(GameArchive.Chain, map + ".pak", out byte[] data))
				{
					ChainPack pack = ChainPack.Read(data);
					if (chip >= 0)
					{
						pack = chip < pack.Count ? ChainPack.Read(pack.Record(chip, pack.Size(chip), 0)) : null;
					}
					if (pack != null && pack.Count == 4 && pack.Size(0) >= 4 && pack.Size(2) >= 16)
					{
						table.Rate = ChainPack.U16(pack.Data, pack.Offset(0));
						int at = pack.Offset(2);
						table.BaseParty = ChainPack.S32(pack.Data, at);
						for (int i = 0; i < 3; i++) table.Thresholds[i] = BitConverter.ToSingle(pack.Data, at + 4 + 4 * i);
						if (table.BaseParty > 0 && table.Thresholds[0] >= 0f)
						{
							for (int k = 0; k < 4; k++) table.Parties.Add(table.BaseParty + k);
						}
						// The overworld chips keep no rate in the first record's first word (a land-form table
						// sits there instead); until it is read, a middling rate.
						if (chip >= 0 && table.Parties.Count > 0 && table.Rate == 0) table.Rate = 6;
					}
					Log.Write(LogChannel.File, "encounters: " + key + " rate " + table.Rate + ", groups " + string.Join(",", table.Parties) + " at " + string.Join("/", table.Thresholds) + "%");
				}
			}
			catch (Exception ex)
			{
				Log.Write(LogChannel.General, "encounters: " + map + ": " + ex.Message);
			}
			if (table.Parties.Count == 0) Log.Write(LogChannel.File, "encounters: " + key + ": none (map " + map + ", chip " + chip + ")");
			_cache[key] = table;
			return table;
		}
	}
}
