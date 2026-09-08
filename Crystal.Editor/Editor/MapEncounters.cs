// A map's random encounters, for the map editor's terrain card: the map's .pak holds, in
// chain 2 (CMapMonsterPartyParameter, one 40-byte record), five groups of four monster
// party ids; the ground's collision material says which group a step is on (its attribute
// flags 20-24 -> groups 1-5, chr.CCharacterEureka.getMonsterGroupId), and map.CEnCountManager
// draws one of the group's four parties (0 skipped) when a fight comes.
// Read and written through the Tables view's own Pak reader, so the grid and this card
// agree byte for byte; a mod's formation number is a party like any other.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using OpenFF.Data;

namespace Crystal.Editor
{
	internal static class MapEncounters
	{
		public const int Groups = 5;
		public const int Slots = 4;

		private static string PakName(Workspace workspace, string map)
		{
			foreach (string name in new[] { "files/" + map + ".pak", map + ".pak" })
				if (workspace.Exists(name)) return name;
			return null;
		}

		private static PakFile Load(Workspace workspace, string name)
		{
			byte[] data = workspace.Read(name);
			int chains = data.Length >= 4 ? BitConverter.ToInt32(data, 0) : 0;
			return Pak.Read(data, Pak.FamilyOf(name, chains, workspace.Game));
		}

		/// <summary>A record's number whatever width the Pak reader gave it (s16 fields come as shorts).</summary>
		private static int Int(JsonNode node)
		{
			if (node is not JsonValue value) return 0;
			try { return Convert.ToInt32(value.GetValue<object>(), System.Globalization.CultureInfo.InvariantCulture); } catch (Exception) { return 0; }
		}

		private static string Field(int group, int slot) => group + "." + slot + ".unnamed0";

		public static object Read(Workspace workspace, string map)
		{
			if (workspace == null || string.IsNullOrEmpty(map)) return new { ok = false, error = "no map" };
			string name = PakName(workspace, map);
			if (name == null) return new { ok = false, error = "no " + map + ".pak" };
			PakFile file;
			try { file = Load(workspace, name); }
			catch (Exception ex) { return new { ok = false, error = ex.Message }; }
			PakChainData chain = file.Chains.FirstOrDefault(c => c.Label == "monsterParties");
			JsonObject record = chain?.Records?.FirstOrDefault();
			if (record == null) return new { ok = false, error = map + ".pak has no monster party chain the editor knows" };
			int[][] groups = new int[Groups][];
			for (int g = 0; g < Groups; g++)
			{
				groups[g] = new int[Slots];
				for (int s = 0; s < Slots; s++) groups[g][s] = Int(record[Field(g, s)]);
			}
			return new { ok = true, name, overridden = workspace.IsOverridden(name), groups };
		}

		public static object Write(Workspace workspace, string map, int[][] groups)
		{
			if (workspace == null || string.IsNullOrEmpty(map)) return new { ok = false, error = "no map" };
			if (groups == null || groups.Length != Groups) return new { ok = false, error = "five groups of four parties" };
			string name = PakName(workspace, map);
			if (name == null) return new { ok = false, error = "no " + map + ".pak" };
			PakFile file = Load(workspace, name);
			PakChainData chain = file.Chains.FirstOrDefault(c => c.Label == "monsterParties");
			JsonObject record = chain?.Records?.FirstOrDefault();
			if (record == null) return new { ok = false, error = map + ".pak has no monster party chain the editor knows" };
			for (int g = 0; g < Groups; g++)
				for (int s = 0; s < Slots; s++)
					// As the reader made them (a short for an s16 field), so Pak.Write reads them back.
					record[Field(g, s)] = JsonValue.Create((short)Math.Max(0, Math.Min(short.MaxValue, s < groups[g].Length ? groups[g][s] : 0)));
			byte[] data = Pak.Write(file);
			workspace.Write(name, data);
			return new { ok = true, name, bytes = data.Length, overridden = true };
		}
	}
}
