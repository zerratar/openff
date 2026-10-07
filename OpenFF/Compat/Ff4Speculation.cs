// The lead's thought over the field menu (world::Balloon, the "speculation"): babil_speculation.bbd's records, 24 bytes
// each - the field symbol (the leading character: 0 Cecil the Dark Knight .. 13 FuSoYa, the player types' order), a flag
// that must be set (0: the symbol's default), the message, a flag to set and one to clear when it is taken, a flag that
// must be clear, and a map's first three letters and its rest (when given: the map must start so, and not be the rest
// on a town or dungeon). Balloon::blnCreate takes the first of the symbol's records that holds, else message 52000 ("...");
// the text is in MSG_SPECULATION.dat's babil_spc_plr_<symbol>.msd.

using System;
using System.Collections.Generic;
using System.Text;

namespace OpenFF.Client
{
	internal static class Ff4Speculation
	{
		private sealed class Record
		{
			public int Symbol, NeedSet, Message, Sets, Clears, NeedClear;
			public string Prefix, Rest;
		}

		private static List<Record> _records;
		private static readonly Dictionary<int, Dictionary<int, string>> _texts = new Dictionary<int, Dictionary<int, string>>();

		/// <summary>The field symbol scripts set (SetSymbolCharacter), or -1: the leader's own then.</summary>
		public static int FieldSymbol = -1;

		/// <summary>The thought for the party as it stands, or null when the game's files are not there.</summary>
		public static string Thought()
		{
			Load();
			int symbol = FieldSymbol >= 0 ? FieldSymbol : Ff4Party.Party.Leader?.Id ?? 0;
			int message = 52000;
			string map = Game.Field.Map ?? "";
			if (_records != null)
			{
				foreach (Record r in _records)
				{
					if (r.Symbol != symbol) continue;
					if (r.NeedSet == 0) { message = r.Message; break; }
					if (!Flag(r.NeedSet) || Flag(r.NeedClear)) continue;
					if (r.Prefix.Length > 0)
					{
						if (!map.StartsWith(r.Prefix.Length > 3 ? r.Prefix.Substring(0, 3) : r.Prefix, StringComparison.OrdinalIgnoreCase)) continue;
						if (r.Rest.Length > 0 && (map.StartsWith("f", StringComparison.OrdinalIgnoreCase) || string.Equals(map.Length > 4 ? map.Substring(4) : "", r.Rest, StringComparison.OrdinalIgnoreCase))) continue;
					}
					if (r.Sets > 0) SetFlag(r.Sets, true);
					if (r.Clears > 0) SetFlag(r.Clears, false);
					message = r.Message;
					break;
				}
			}
			return Text(symbol, message);
		}

		private static void Load()
		{
			if (_records != null) return;
			byte[] data = Ff4Ui.ReadFile("babil_speculation.bbd");
			if (data == null) { _records = new List<Record>(); return; }
			List<Record> records = new List<Record>();
			for (int at = 0; at + 24 <= data.Length; at += 24)
			{
				records.Add(new Record
				{
					Symbol = data[at],
					NeedSet = BitConverter.ToInt16(data, at + 2),
					Message = BitConverter.ToInt32(data, at + 4),
					Sets = BitConverter.ToUInt16(data, at + 8),
					Clears = BitConverter.ToUInt16(data, at + 10),
					NeedClear = BitConverter.ToInt16(data, at + 12),
					Prefix = Ascii(data, at + 14, 4),
					Rest = Ascii(data, at + 18, 6),
				});
			}
			_records = records;
		}

		private static string Ascii(byte[] data, int at, int length)
		{
			int end = at;
			while (end < at + length && data[end] != 0) end++;
			return Encoding.ASCII.GetString(data, at, end - at);
		}

		/// <summary>A message of the symbol's .msd (MSDA: a count at 8, then 12-byte records from 16 - id, pages, offset -
		/// and NUL-ended UTF-16).</summary>
		private static string Text(int symbol, int message)
		{
			if (!_texts.TryGetValue(symbol, out Dictionary<int, string> texts))
			{
				texts = new Dictionary<int, string>();
				byte[] raw = Ff4Ui.ReadFile("MSG_SPECULATION.dat/babil_spc_plr_" + symbol.ToString("00") + ".msd");
				if (raw != null && raw.Length > 16 && raw[0] == 'M' && raw[1] == 'S' && raw[2] == 'D' && raw[3] == 'A')
				{
					int count = BitConverter.ToInt32(raw, 8);
					for (int i = 0; i < count && 16 + 12 * i + 12 <= raw.Length; i++)
					{
						int id = BitConverter.ToInt32(raw, 16 + 12 * i);
						int offset = BitConverter.ToInt32(raw, 16 + 12 * i + 8);
						int end = offset;
						while (end + 1 < raw.Length && (raw[end] != 0 || raw[end + 1] != 0)) end += 2;
						if (offset >= 0 && offset <= raw.Length) texts[id] = Encoding.Unicode.GetString(raw, offset, Math.Max(0, end - offset));
					}
				}
				_texts[symbol] = texts;
			}
			return texts.TryGetValue(message, out string text) ? text : null;
		}

		private static bool Flag(int index)
		{
			byte[,] flags = GlobalScope.flags;
			return flags != null && index >= 0 && index < flags.GetLength(1) && flags[0, index] != 0;
		}

		private static void SetFlag(int index, bool on)
		{
			byte[,] flags = GlobalScope.flags;
			if (flags != null && index >= 0 && index < flags.GetLength(1)) flags[0, index] = (byte)(on ? 1 : 0);
		}
	}
}
