// The two games' table readers behind one door, and what they share: finding a file by
// either game's name and reading an .msd into names.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using FF3.Content;

namespace OpenFF.Data
{
	internal static class TableFiles
	{
		/// <summary>The tables of whichever game the chain holds ("ff3", "ff4", or null to look at the item pack's chain count).</summary>
		public static GameTables Read(ContentChain chain, string game = null)
		{
			if (string.IsNullOrEmpty(game))
			{
				game = "ff3";
				if (ReadAny(chain, "item_parameter.pak", out byte[] data))
				{
					try { if (ChainPack.Read(data).Count == 4) game = "ff4"; } catch (Exception) { }
				}
			}
			return string.Equals(game, "ff4", StringComparison.OrdinalIgnoreCase) ? Ff4Tables.Read(chain) : Ff3Tables.Read(chain);
		}

		/// <summary>The file by FF3's name (under files/, as the chain lays the games out) or FF4's compressed one, decompressed.</summary>
		public static bool ReadAny(ContentChain chain, string name, out byte[] data)
		{
			foreach (string candidate in new[] { "files/" + name, name })
			{
				if (chain.TryRead(candidate, out data) && data != null && data.Length > 0) return true;
				if (chain.TryRead(candidate + ".lz", out byte[] packed) && packed != null && packed.Length > 0)
				{
					data = Lz.IsCompressed(packed) ? Lz.Decompress(packed) : packed;
					return true;
				}
			}
			data = null;
			return false;
		}

		/// <summary>An .msd's messages by id, first page only and without icon glyphs, or null when the file is missing.</summary>
		public static Dictionary<uint, string> ReadNames(ContentChain chain, string file, GameTables tables)
		{
			if (!ReadAny(chain, file, out byte[] data))
			{
				tables.Notes.Add(file + " not found; items keep their ids");
				return null;
			}
			try
			{
				Dictionary<uint, string> names = new Dictionary<uint, string>();
				foreach (MsdMessage message in Msd.Read(data).Messages)
				{
					if (message.Pages.Count > 0 && !names.ContainsKey(message.Id)) names[message.Id] = Plain(message.Pages[0]);
				}
				return names;
			}
			catch (Exception ex)
			{
				tables.Notes.Add(file + ": " + ex.Message);
				return null;
			}
		}

		/// <summary>A message without its icon glyphs and control characters (item names start with one).</summary>
		public static string Plain(string text)
		{
			StringBuilder sb = new StringBuilder(text.Length);
			foreach (char c in text)
			{
				UnicodeCategory category = char.GetUnicodeCategory(c);
				if (char.IsControl(c) || category == UnicodeCategory.PrivateUse || category == UnicodeCategory.OtherNotAssigned || c == '�') continue;
				sb.Append(c);
			}
			return sb.ToString().Trim();
		}
	}
}
