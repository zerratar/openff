// Text the client draws that no .msd carries, in the game's nine languages.
//
// The game's own words live in its message files. What the client adds, or draws as text where
// the game had a picture (the title's commands), lives here: keyed by name, with a string for each
// language whose words differ from English. A language without its own string reads as English -
// which is also how Square did the title for Japanese, Korean and both Chinese builds, whose title
// pictures keep the English words.
//
// The title's words are the phone build's, copied from its title pictures (<lang>.lproj/title_items_i).

using System.Collections.Generic;

namespace OpenFF.Client
{
	internal static class Localization
	{
		/// <summary>The game's languages, in AppShell's order (the value of its language index).</summary>
		public enum Language
		{
			Japanese,
			English,
			French,
			German,
			Italian,
			Spanish,
			ChineseSimplified,
			ChineseTraditional,
			Korean
		}

		/// <summary>The keys, so a caller names a string rather than spelling it.</summary>
		public static class Keys
		{
			public const string TitleContinue = "title.continue";
			public const string TitleNewGame = "title.new-game";
			public const string TitleLoadGame = "title.load-game";
			public const string TitleMods = "title.mods";
		}

		private static readonly Dictionary<string, Dictionary<Language, string>> _table = new Dictionary<string, Dictionary<Language, string>>
		{
			[Keys.TitleContinue] = Words("CONTINUE", fr: "CONTINUER", de: "FORTFAHREN", it: "CONTINUA", es: "CONTINUAR"),
			[Keys.TitleNewGame] = Words("NEW GAME", fr: "NOUVELLE PARTIE", de: "NEUES SPIEL", it: "NUOVA PARTITA", es: "NUEVA PARTIDA"),
			[Keys.TitleLoadGame] = Words("LOAD GAME", fr: "CHARGER PARTIE", de: "SPIEL LADEN", it: "CARICA PARTITA", es: "CARGAR PARTIDA"),
			// The client's own entry; no build of the game has one to copy, and "mods" reads the same in all of them.
			[Keys.TitleMods] = Words("MODS"),
		};

		/// <summary>The language the game runs in; English for an index it does not know.</summary>
		public static Language Current
		{
			get
			{
				int index = AppShell.getLanguage();
				return index >= 0 && index <= (int)Language.Korean ? (Language)index : Language.English;
			}
		}

		/// <summary>A string in the game's language.</summary>
		public static string Get(string key) => Get(key, Current);

		/// <summary>A string in a language: its own words, else English; the key itself when there is no such string.</summary>
		public static string Get(string key, Language language)
		{
			if (!_table.TryGetValue(key, out Dictionary<Language, string> words))
			{
				return key;
			}
			return words.TryGetValue(language, out string text) ? text : words[Language.English];
		}

		private static Dictionary<Language, string> Words(string en, string ja = null, string fr = null, string de = null, string it = null,
			string es = null, string zhCn = null, string zhTw = null, string ko = null)
		{
			Dictionary<Language, string> words = new Dictionary<Language, string> { [Language.English] = en };
			void Add(Language language, string text)
			{
				if (text != null) words[language] = text;
			}
			Add(Language.Japanese, ja);
			Add(Language.French, fr);
			Add(Language.German, de);
			Add(Language.Italian, it);
			Add(Language.Spanish, es);
			Add(Language.ChineseSimplified, zhCn);
			Add(Language.ChineseTraditional, zhTw);
			Add(Language.Korean, ko);
			return words;
		}
	}
}
