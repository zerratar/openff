// FF4 3D's tables, as far as they are known - which is their shape, not their meaning.
//
// FF4 keeps the same containers as FF3 (item_parameter.pak, monster.chaindata,
// player.chaindata, one .pak per map) but different records in them: 97 weapons of 88
// bytes where FF3 has 56, 251 monsters of 152 where FF3 has 100, and a map .pak of four
// single records where FF3 has seven chains. There is no FF4 source to take field names
// from, so FF3's layouts must not be applied - they would type the wrong bytes and a
// save would rewrite the table wrongly. What is here instead:
//
//   - a record stride wherever the file itself proves one: an id field that counts up
//     record after record, for at least 80% of the chain, with no bytes left over. Those
//     chains are shown as rows of 16-bit words, unnamed. A chain that does not divide
//     cleanly (item chain 0 is 59 records of 48 bytes and then 46 bytes; monster chain 0
//     is two bytes short of 252) stays raw, because a typed rebuild would change its size.
//   - the map .pak's four records named after the accessors world::MapParameterManager
//     exposes, in order: encountParameter, landFormParameter, monsterPartyParameter,
//     environEffectParameter.
//
// Hand-written. When a field's meaning is pinned down, name it here.

using System.Linq;

namespace FF3.ContentTool
{
	internal static class PakRecordsFf4
	{
		public const string Item = "Ff4Item";
		public const string Monster = "Ff4Monster";
		public const string Player = "Ff4Player";
		public const string Map = "Ff4Map";

		private const string Unnamed = "FF4: fields not yet named; each row is one record, each column one 16-bit word.";

		/// <summary>
		/// A chain of fixed records with no known fields: every record is stride/2 words,
		/// one column each (w0, w1, ...) so the grid can sort and edit them singly.
		/// </summary>
		private static PakChain Words(string family, int index, string label, int stride, int records, string note = null)
		{
			PakField[] fields = Enumerable.Range(0, stride / 2)
				.Select(i => new PakField("w" + i.ToString(System.Globalization.CultureInfo.InvariantCulture), FieldType.S16, 1))
				.ToArray();
			return new PakChain(family, index, label, "ff4:" + label, stride, fields,
				note ?? string.Format(System.Globalization.CultureInfo.InvariantCulture,
					"{0} records of {1} bytes. {2}", records, stride, Unnamed));
		}

		public static readonly PakChain[] Chains =
		{
			// item_parameter.pak: 4 chains. Chain 0 (2878 bytes) is 59 x 48 plus 46 and stays raw.
			Words(Item, 1, "chain1", 88, 97),
			Words(Item, 2, "chain2", 84, 84),
			Words(Item, 3, "chain3", 32, 62),

			// monster.chaindata: 12 chains. Chain 0 (38302 bytes) is 152 x 252 less two bytes; raw.
			Words(Monster, 1, "chain1", 18, 22),
			Words(Monster, 4, "chain4", 84, 251),
			Words(Monster, 7, "chain7", 22, 250),
			Words(Monster, 8, "chain8", 44, 189),
			Words(Monster, 9, "chain9", 12, 189),
			Words(Monster, 10, "chain10", 14, 84),

			// player.chaindata: 37 chains, most of them 1187 bytes and not fixed-stride.
			Words(Player, 33, "chain33", 8, 50),
			Words(Player, 34, "chain34", 108, 12),

			// <map>.pak: one record apiece, in MapParameterManager's order.
			Words(Map, 0, "encount", 52, 1, "How dangerous the map is. FF4: one record; fields not yet named."),
			Words(Map, 1, "landForm", 42, 1, "Terrain attributes. FF4: one record; fields not yet named."),
			Words(Map, 2, "monsterParty", 16, 1, "The monster groups this map can throw at you. FF4: one record; fields not yet named."),
			Words(Map, 3, "environEffect", 8, 1, "Environment effect. FF4: one record; fields not yet named."),
		};

		public static PakChain Find(string family, int index)
		{
			return Chains.FirstOrDefault(c => c.Family == family && c.Index == index);
		}

		/// <summary>The FF4 family for a table file, or null when it is not one FF4 has.</summary>
		public static string FamilyOf(string fileName, int chainCount)
		{
			string name = System.IO.Path.GetFileName(fileName);
			if (string.Equals(name, "item_parameter.pak", System.StringComparison.OrdinalIgnoreCase)) return Item;
			if (string.Equals(name, "monster.chaindata", System.StringComparison.OrdinalIgnoreCase)) return Monster;
			if (string.Equals(name, "player.chaindata", System.StringComparison.OrdinalIgnoreCase)) return Player;
			// A map's .pak is named after the map - d01_00.pak - and has four chains.
			// item_parameter.pak has four too, so the count alone would mistype it.
			return chainCount == 4 && System.Text.RegularExpressions.Regex.IsMatch(name, @"^[a-z]\d\d_\d\d(_[a-z0-9]+)?\.pak$", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
				? Map : null;
		}
	}
}
