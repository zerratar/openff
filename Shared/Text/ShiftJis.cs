// Shift-JIS, over the game's own tables.
//
// Sjis.cs is compiled in from OpenFF rather than copied, and CP932 is
// deliberately not used instead: what matters is not that the tool agrees with
// Windows, but that it agrees with the game. Encoding is the exact inverse of
// StringUtil.SJISEncoding.GetChars, so anything written here reads back through
// the game's decoder unchanged.

using System;
using System.Collections.Generic;
using System.Text;

namespace OpenFF.Content
{
	internal static class ShiftJis
	{
		private static Dictionary<char, byte> _single;
		private static Dictionary<char, ushort> _double;

		/// <summary>Decodes a NUL-terminated string, following the game's rules.</summary>
		public static string Decode(byte[] data, int at, out int bytesRead)
		{
			StringBuilder text = new StringBuilder();
			int i = at;
			while (i < data.Length && data[i] != 0)
			{
				byte b = data[i];
				if (((b >= 129 && b <= 159) || b >= 224) && i + 1 < data.Length && data[i + 1] != 0)
				{
					ushort pair = (ushort)((b << 8) | data[i + 1]);
					if (!Sjis.Jis0208Table.TryGetValue(pair, out char wide))
					{
						throw new NotSupportedException(string.Format(
							"byte pair {0:X4} at offset {1} is not in the game's Shift-JIS table",
							pair, i));
					}
					text.Append(wide);
					i += 2;
					continue;
				}

				if (!Sjis.Jis0201Table.TryGetValue(b, out char narrow))
				{
					throw new NotSupportedException(string.Format(
						"byte {0:X2} at offset {1} is not in the game's Shift-JIS table", b, i));
				}
				text.Append(narrow);
				i++;
			}
			bytesRead = i - at;
			return text.ToString();
		}

		public static byte[] Encode(string text)
		{
			BuildReverseTables();
			List<byte> bytes = new List<byte>(text.Length + 8);
			foreach (char c in text)
			{
				// Single byte wins where a character is in both tables, which is what
				// the shipped files do and what keeps a round trip byte exact.
				if (_single.TryGetValue(c, out byte narrow))
				{
					bytes.Add(narrow);
				}
				else if (_double.TryGetValue(c, out ushort wide))
				{
					bytes.Add((byte)(wide >> 8));
					bytes.Add((byte)wide);
				}
				else
				{
					throw new NotSupportedException(string.Format(
						"U+{0:X4} ({1}) has no Shift-JIS encoding in the game's table",
						(int)c, c));
				}
			}
			return bytes.ToArray();
		}

		public static int ByteCount(string text)
		{
			return Encode(text).Length;
		}

		private static void BuildReverseTables()
		{
			if (_single != null)
			{
				return;
			}

			Dictionary<char, byte> single = new Dictionary<char, byte>();
			foreach (KeyValuePair<byte, char> row in Sjis.Jis0201Table)
			{
				// The decoder only reaches these ranges as single bytes.
				if ((row.Key >= 32 && row.Key <= 128) || (row.Key >= 160 && row.Key <= 223))
				{
					single[row.Value] = row.Key;
				}
			}

			Dictionary<char, ushort> pairs = new Dictionary<char, ushort>();
			foreach (KeyValuePair<ushort, char> row in Sjis.Jis0208Table)
			{
				if (!pairs.ContainsKey(row.Value))
				{
					pairs[row.Value] = row.Key;
				}
			}

			_double = pairs;
			_single = single;
		}
	}
}
