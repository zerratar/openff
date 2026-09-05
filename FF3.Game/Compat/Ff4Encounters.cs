// FF4's random encounters, from the map's parameter pack.
//
// Each FF4 map has a .pak in MAPPARAMETER.dat with four chains, which world::
// MapParameterManager::load keeps in order (Tools/ff4_disasm.py): chain 0 the land-form
// parameters (landFormParameter, 50 bytes: twelve u16s at 0 and twelve at 0x18 - the ones at
// 0x18 are the encounter rate per land form, WSEncountSetting::wsProcess reads
// [0x18 + 2 x land form] and treats anything over 30 as none); chain 1 the encounter sets
// (monsterPartyParameter: 8-byte entries of four u16 group ids, 0xFFFF for none - the set
// the party's land form names is rolled among its groups, re-rolled up to five times when
// it repeats the last fight); chain 2 the encounter parameter (16 bytes: an s16 for
// world::attackType - back attacks and the like - and three floats); chain 3 eight bytes.
// The land form under the party is not read yet, so land form 0 stands for all: the Watery
// Pass (d01_00) gives Sword Rat + Goblin, Tiny Mages, Fangshells at rate 9; the Baron plain
// chip (f00_48) Floating Eye, Helldiver, three Goblins at rate 1. The overworld's pack holds
// one such four-chain pack per chip (256 x 192 bytes); the chip under the party picks it.

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
			/// <summary>The rate for land form 0 (chain 0 at 0x18); 0 means no fights.</summary>
			public int Rate;
			/// <summary>Every land form's rate (the twelve u16s at 0x18).</summary>
			public int[] Rates = new int[12];
			/// <summary>The encounter sets (chain 1): four group ids each, -1 for none.</summary>
			public List<int[]> Sets = new List<int[]>();
			/// <summary>The groups of set 0 that exist in the tables, for the log and the roll.</summary>
			public List<int> Parties = new List<int>();
			/// <summary>The attack-type word and the three percentages of chain 2 (back attacks and the like; not applied yet).</summary>
			public int AttackType;
			public float[] Thresholds = new float[3];
			private int _last = -1;

			/// <summary>A group from set 0 (the land form is not read yet), not the last one when there is a choice.</summary>
			public int Roll(Random random)
			{
				if (Parties.Count == 0) return -1;
				int pick = Parties[random.Next(Parties.Count)];
				for (int tries = 0; tries < 5 && pick == _last && Parties.Count > 1; tries++) pick = Parties[random.Next(Parties.Count)];
				_last = pick;
				return pick;
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
					if (pack != null && pack.Count == 4 && pack.Size(0) >= 0x30 && pack.Size(1) >= 8)
					{
						int land = pack.Offset(0);
						for (int i = 0; i < 12; i++)
						{
							int rate = ChainPack.U16(pack.Data, land + 0x18 + 2 * i);
							table.Rates[i] = rate > 30 ? 0 : rate;
						}
						table.Rate = table.Rates[0];
						int sets = pack.Offset(1);
						for (int e = 0; e + 8 <= pack.Size(1); e += 8)
						{
							int[] set = new int[4];
							for (int k = 0; k < 4; k++)
							{
								int id = ChainPack.U16(pack.Data, sets + e + 2 * k);
								set[k] = id == 0xFFFF ? -1 : id;
							}
							table.Sets.Add(set);
						}
						GameTables tables = Ff4Party.Tables;
						if (table.Sets.Count > 0)
						{
							foreach (int id in table.Sets[0])
							{
								MonsterParty party = id > 0 && tables != null ? tables.MonsterParty(id) : null;
								if (party != null && party.Slots.Count > 0 && !table.Parties.Contains(id)) table.Parties.Add(id);
							}
						}
						if (pack.Size(2) >= 16)
						{
							int at = pack.Offset(2);
							table.AttackType = ChainPack.S16(pack.Data, at);
							for (int i = 0; i < 3; i++) table.Thresholds[i] = BitConverter.ToSingle(pack.Data, at + 4 + 4 * i);
						}
					}
					Log.Write(LogChannel.File, "encounters: " + key + " rate " + table.Rate + " (by land form " + string.Join("/", table.Rates) + "), set 0 groups " + string.Join(",", table.Parties) + " of " + table.Sets.Count + " set(s); attack type " + table.AttackType + " at " + string.Join("/", table.Thresholds) + "%");
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
