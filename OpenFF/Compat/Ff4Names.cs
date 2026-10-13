// FF4's names the player can set besides the party's: Whyt's (PlayerSummon::name, the summon kept in
// GameParameter::summonSaveParameter) and the ability's Lovingway names (GameParameter::abilityName), with the message codes
// that show them - libff4's "chr" control code: %chr50% the ability's name, %chr51% the cover count, %chr99% Whyt's name,
// %chr1%..%chr13% a member's (by the table at 0x2bbe30: Cecil, Rosa, Kain, Cid, Rydia, Tellah, Edward, Yang, Palom,
// Porom, Edge, FuSoYa, Golbez - the party's player types 1, 3, 2, 11, 5, 6, 9, 10, 8, 7, 12, 13, 14). And Steam's own
// lines beside FF4.exe (menu.txt: seven a language - Resume, Quit, Yes, No, the skip and quit questions, the name
// entry's keyboard line).

using System;
using System.Collections.Generic;
using System.IO;
using OpenFF;
using OpenFF.Data;

namespace OpenFF.Client
{
	internal static class Ff4Names
	{
		private static string _summon, _ability = "";

		/// <summary>Whyt's name: as set, or the game's (babil_menu.msd 55014) - libff4 starts it as ポーチカ, the English
		/// build's text names it.</summary>
		public static string Summon
		{
			get => string.IsNullOrEmpty(_summon) ? Ff4Layouts.Text(55014, "Whyt") : _summon;
			set => _summon = value;
		}

		public static string Ability
		{
			get => _ability ?? "";
			set => _ability = value ?? "";
		}

		public static int CoverCount;

		public static void NewGame()
		{
			_summon = null;
			_ability = "";
			CoverCount = 0;
		}

		private static readonly int[] MemberTypes = { 1, 3, 2, 11, 5, 6, 9, 10, 8, 7, 12, 13, 14 };

		/// <summary>The "chr" control code's text (FUN_0043c828): null for a code it does not know (60 and on are the battle's).</summary>
		public static string ControlCode(string code)
		{
			if (code == null || !code.StartsWith("chr", StringComparison.Ordinal)) return null;
			int digits = 3;
			while (digits < code.Length && char.IsDigit(code[digits])) digits++;
			if (!int.TryParse(code.Substring(3, digits - 3), out int n)) return null;
			if (n == 50) return Ability;
			if (n == 51) return CoverCount.ToString();
			if (n == 99) return Summon;
			if (n >= 1 && n <= MemberTypes.Length)
			{
				foreach (Character c in Ff4Party.Party.Members) if (c.Id == MemberTypes[n - 1]) return c.Name;
				return null;
			}
			return null;
		}

		// ---- Steam's menu.txt ----

		private static string[] _steamLines;

		/// <summary>Line <paramref name="index"/> (0..6) of Steam's menu.txt in the game's language (English the first seven).</summary>
		public static string SteamLine(int index, string fallback)
		{
			if (_steamLines == null)
			{
				_steamLines = Array.Empty<string>();
				try
				{
					string path = Ff4Title.SteamFile("menu.txt");
					if (path != null) _steamLines = File.ReadAllLines(path, System.Text.Encoding.UTF8);
				}
				catch (Exception ex) { Log.Write(LogChannel.General, "ff4: menu.txt: " + ex.Message); }
			}
			return index >= 0 && index < _steamLines.Length && _steamLines[index].Length > 0 ? _steamLines[index] : fallback;
		}
	}
}
