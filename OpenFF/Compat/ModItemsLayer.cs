// The mods' item definitions (defs/items/*.json in each enabled mod and in the --project)
// composed into item_parameter.pak and eureka_item.msd as the game reads them, through
// the content chain's transform step. With no definitions anywhere the step is not even
// registered: FF3 reads its files as shipped. FF3 only for now - FF4 keeps its items in
// other files (Ff4Tables) and gets its own composer when its tables are the engine's.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenFF.Content;
using OpenFF.Data;

namespace OpenFF.Client
{
	internal static class ModItemsLayer
	{
		/// <summary>The mods' text (defs/text/*.json), composed into eureka_permanent.msd - the file every map falls back to - as it is read.</summary>
		private static void RegisterText(ContentChain chain, List<string> roots)
		{
			List<string> notes = new List<string>();
			Dictionary<uint, string> lines = ModText.Load(roots, notes);
			foreach (string note in notes) Log.Write(LogChannel.General, "text: " + note);
			if (lines.Count == 0) return;
			Log.Write(LogChannel.General, "text: " + lines.Count + " line(s) of the mods' own (" + lines.Keys.Min() + ".." + lines.Keys.Max() + ")");
			// A line written per language follows the game's language setting, which the player
			// may change after this runs: the lines are picked per language the first time the
			// file is read in it (the map's load), English by default.
			Dictionary<string, Dictionary<uint, string>> byLanguage = new Dictionary<string, Dictionary<uint, string>> { ["en"] = lines };
			chain.AddTransform((name, data) =>
			{
				if (!ModText.IsPermanent(name)) return data;
				// The game keeps a copy per language (en.lproj/eureka_permanent.msd): the folder says
				// which is being read; without one, the game's setting.
				string language = FolderLanguage(name) ?? GameLanguage();
				if (!byLanguage.TryGetValue(language, out Dictionary<uint, string> picked))
					byLanguage[language] = picked = ModText.Load(roots, null, language);
				byte[] composed = ModText.Compose(data, picked);
				Log.Write(LogChannel.File, "text: eureka_permanent.msd composed (" + language + "), " + data.Length + " -> " + composed.Length + " bytes");
				return composed;
			});
		}

		/// <summary>"de.lproj/eureka_permanent.msd" -> "de"; null when the name has no language folder.</summary>
		private static string FolderLanguage(string name)
		{
			string folder = Path.GetDirectoryName((name ?? "").Replace('\\', '/'));
			if (string.IsNullOrEmpty(folder)) return null;
			string last = Path.GetFileName(folder);
			return last.EndsWith(".lproj", StringComparison.OrdinalIgnoreCase) && last.Length > 6 ? last.Substring(0, last.Length - 6) : null;
		}

		/// <summary>The game's language setting as a code the text files use: en, ja, fr, de, it, es, zh-CN, zh-TW, ko.</summary>
		private static string GameLanguage()
		{
			string[] codes = { "ja", "en", "fr", "de", "it", "es", "zh-CN", "zh-TW", "ko" };
			try
			{
				int index = AppShell.getLanguage();
				return index >= 0 && index < codes.Length ? codes[index] : "en";
			}
			catch (Exception) { return "en"; }
		}

		/// <summary>The mods' monsters and formations in play, in load order.</summary>
		public static IReadOnlyList<ModMonster> Monsters { get; private set; } = new List<ModMonster>();
		public static IReadOnlyList<ModFormation> Formations { get; private set; } = new List<ModFormation>();

		/// <summary>Whether a monster party id is one of the mods' formations (the map's encounter tables may name one).</summary>
		public static bool HasFormation(int number)
		{
			foreach (ModFormation f in Formations) if (f.Number == number) return true;
			return false;
		}

		/// <summary>
		/// The mods' monsters (defs/monsters) into monster.chaindata and eureka_battle.msd, their
		/// formations (defs/formations) into monster_party_table.bbd, as the game reads them; and a
		/// mod monster's texture answered with the one it wears (Shared/Data/ModMonsters.cs).
		/// </summary>
		private static void RegisterMonsters(ContentChain chain, List<string> roots)
		{
			List<string> notes = new List<string>();
			List<ModMonster> monsters = ModMonsters.Load(roots, notes);
			List<ModFormation> formations = ModMonsters.LoadFormations(roots, notes);
			foreach (string note in notes) Log.Write(LogChannel.General, "monsters: " + note);
			HashSet<int> numbers = new HashSet<int>();
			List<ModMonster> kept = new List<ModMonster>();
			foreach (ModMonster m in monsters)
			{
				if (!numbers.Add(m.Number)) { Log.Write(LogChannel.General, "monsters: " + m.Id + " (" + m.Source + ") has number " + m.Number + ", already taken - skipped"); continue; }
				kept.Add(m);
			}
			HashSet<int> parties = new HashSet<int>();
			List<ModFormation> keptFormations = new List<ModFormation>();
			foreach (ModFormation f in formations)
			{
				if (!parties.Add(f.Number)) { Log.Write(LogChannel.General, "monsters: formation " + f.Id + " (" + f.Source + ") has number " + f.Number + ", already taken - skipped"); continue; }
				keptFormations.Add(f);
			}
			Monsters = kept;
			Formations = keptFormations;
			if (kept.Count == 0 && keptFormations.Count == 0) return;
			if (kept.Count > 0) Log.Write(LogChannel.General, "monsters: " + kept.Count + " of the mods' own: " + string.Join(", ", kept.Select(m => m.Number + " " + (m.Name ?? m.Id) + " (from " + m.Base + ")")));
			if (keptFormations.Count > 0) Log.Write(LogChannel.General, "monsters: " + keptFormations.Count + " formation(s): " + string.Join(", ", keptFormations.Select(f => f.Number + " " + (f.Name ?? f.Id))));
			chain.AddTransform((name, data) =>
			{
				if (kept.Count > 0 && ModMonsters.IsChain(name))
				{
					List<string> problems = new List<string>();
					byte[] composed = ModMonsters.ComposeChain(data, kept, problems);
					foreach (string p in problems) Log.Write(LogChannel.General, "monsters: " + p);
					Log.Write(LogChannel.File, "monsters: monster.chaindata composed, " + data.Length + " -> " + composed.Length + " bytes");
					return composed;
				}
				if (kept.Count > 0 && ModMonsters.IsMsd(name))
				{
					byte[] composed = ModMonsters.ComposeMsd(data, kept);
					Log.Write(LogChannel.File, "monsters: " + name + " composed, " + data.Length + " -> " + composed.Length + " bytes");
					return composed;
				}
				if (keptFormations.Count > 0 && ModMonsters.IsParties(name))
				{
					byte[] composed = ModMonsters.ComposeParties(data, keptFormations);
					Log.Write(LogChannel.File, "monsters: monster_party_table.bbd composed, " + data.Length + " -> " + composed.Length + " bytes");
					return composed;
				}
				return data;
			});
			if (kept.Count > 0) chain.AddAlias(name => ModMonsters.TextureAlias(name, kept));
		}

		/// <summary>The mods' model definitions (defs/models): a game model drawn as a glTF of the mod's (CharacterMeshes).</summary>
		private static void RegisterModels(List<string> roots)
		{
			List<string> notes = new List<string>();
			List<ModModel> models = ModModels.Load(roots, notes);
			foreach (string note in notes) Log.Write(LogChannel.General, "models: " + note);
			CharacterMeshes.Register(models);
			Roots = roots.ToList();
			// The mods' PNGs in place of the game's textures, at any size (textures/<name>.png).
			TextureOverrides.Register(roots);
		}

		/// <summary>The mod roots in play, in load order: where a mod-relative file (assets/x.glb) is looked for when no mod is named.</summary>
		public static IReadOnlyList<string> Roots { get; private set; } = new List<string>();

		/// <summary>A mod-relative path (assets/x.glb) as a file on disk: under the mod given, else the first root that has it; null when none does.</summary>
		public static string ResolveAsset(string relative, string modDirectory)
		{
			if (string.IsNullOrWhiteSpace(relative)) return null;
			if (System.IO.Path.IsPathRooted(relative)) return System.IO.File.Exists(relative) ? relative : null;
			string tail = relative.Replace('/', System.IO.Path.DirectorySeparatorChar);
			if (!string.IsNullOrEmpty(modDirectory))
			{
				string under = System.IO.Path.Combine(modDirectory, tail);
				if (System.IO.File.Exists(under)) return under;
			}
			foreach (string root in Roots)
			{
				string under = System.IO.Path.Combine(root, tail);
				if (System.IO.File.Exists(under)) return under;
			}
			return null;
		}

		/// <summary>The definitions in play, in load order (a --project's first, then the mods').</summary>
		public static IReadOnlyList<ModItem> Items { get; private set; } = new List<ModItem>();

		// The spells' names, read once from the game's tables when a look names a spell by name; while
		// they are read the transform below passes player.chaindata through (the tables read it too).
		private static Dictionary<string, int> _spellNames;
		private static bool _readingNames;

		/// <summary>
		/// The mods' spell looks (defs/spells) and a record for each mod item whose base has one,
		/// composed into player.chaindata's effect table as the game reads it (Shared/Data/ModSpells.cs).
		/// </summary>
		private static void RegisterSpells(ContentChain chain, List<string> roots, List<ModItem> items)
		{
			List<string> notes = new List<string>();
			List<ModSpell> spells = ModSpells.Load(roots, notes);
			foreach (string note in notes) Log.Write(LogChannel.General, "spells: " + note);
			List<ModSpell> casts = spells.Where(s => s.Cast != null).ToList();
			if (casts.Count > 0) _castsFrom = () =>
			{
				// Resolved as a battle first asks: by then the spells' names can be read.
				Dictionary<int, int> map = new Dictionary<int, int>();
				foreach (ModSpell s in casts)
				{
					// A school's, none, a pack of the game's, or a mod's own effect (defs/effects).
					int? pack = ModSpell.CastPack(s.Cast) ?? ModEffects.Category(s.Cast);
					if (pack == null) Log.Write(LogChannel.General, "spells: " + s.Id + ": no cast '" + s.Cast + "' (black, white, summon, none, game:pack or a mod's effect)");
					else if (SpellId(chain, s.Spell, items) is int id) map[id] = pack.Value;
					else Log.Write(LogChannel.General, "spells: " + s.Id + ": no spell '" + s.Spell + "' for its cast");
				}
				return map;
			};
			List<(int, int)> copies = items.Select(i => (i.Number, i.Base)).ToList();
			if (spells.Count == 0 && copies.Count == 0) return;
			if (spells.Count > 0) Log.Write(LogChannel.General, "spells: " + spells.Count + " look(s) of the mods': " + string.Join(", ", spells.Select(s => s.Spell + " -> " + (s.Effect ?? "its own") + (s.Sound != null ? ", sound " + s.Sound : "") + (s.Cast != null ? ", cast " + s.Cast : ""))));
			chain.AddTransform((name, data) =>
			{
				if (_readingNames || !ModSpells.IsChaindata(name)) return data;
				List<string> problems = new List<string>();
				byte[] composed = ModSpells.Compose(data, spells, copies, text => SpellId(chain, text, items), problems, ModEffects.Category);
				foreach (string p in problems) Log.Write(LogChannel.General, "spells: " + p);
				if (!ReferenceEquals(composed, data)) Log.Write(LogChannel.File, "spells: player.chaindata composed, " + data.Length + " -> " + composed.Length + " bytes");
				return composed;
			});
		}

		/// <summary>
		/// The mods' summons (defs/summons): their steps in place of the game's scripts, composed into
		/// summon_script_command.pack as the battle reads it (Shared/Data/ModSummons.cs); an effect a step names by
		/// its id is one of the mods' (defs/effects), put in by its category. A new summon - a mod item based on one of
		/// the eight summons' spells - plays its base's outcome, with the scripts the definitions give it ("spell").
		/// </summary>
		private static void RegisterSummons(ContentChain chain, List<string> roots, List<ModItem> items)
		{
			foreach (ModItem item in items) if (item.Base >= FirstSummon && item.Base < FirstSummon + 8) _summonBases[item.Number] = item.Base;
			List<string> notes = new List<string>();
			List<ModSummon> summons = ModSummons.Load(roots, notes);
			foreach (string note in notes) Log.Write(LogChannel.General, "summons: " + note);
			List<ModSummon> added = ModSummons.NewSummons(summons);
			for (int k = 0; k < added.Count; k++)
			{
				ModSummon s = added[k];
				ModItem mine = items.FirstOrDefault(i => string.Equals(i.Name, s.Spell, StringComparison.OrdinalIgnoreCase) || string.Equals(i.Id, s.Spell, StringComparison.OrdinalIgnoreCase) || i.Number.ToString(System.Globalization.CultureInfo.InvariantCulture) == s.Spell);
				if (mine == null || !_summonBases.ContainsKey(mine.Number)) { Log.Write(LogChannel.General, "summons: " + s.Id + ": no spell '" + s.Spell + "' of the mods' based on a summon (4201..4208)"); continue; }
				_summonChains[(mine.Number, s.Outcome)] = ModSummons.GameChains + k;
				if (s.Name != null) _summonNames[(mine.Number, s.Outcome)] = s.Name;
			}
			// A new summon's outcome without a name of its own is its spell's; one of the eight's may be renamed too.
			foreach (ModItem item in items) if (_summonBases.ContainsKey(item.Number) && !string.IsNullOrWhiteSpace(item.Name)) for (int o = 0; o < 3; o++) if (!_summonNames.ContainsKey((item.Number, o))) _summonNames[(item.Number, o)] = item.Name.Trim();
			foreach (ModSummon s in summons) if (s.Spell == null && s.Name != null && ModSummons.Level(s.Summon) >= 0) _summonNames[(-1 - ModSummons.Level(s.Summon), s.Outcome)] = s.Name;
			if (_summonBases.Count > 0) Log.Write(LogChannel.General, "summons: " + _summonBases.Count + " new of the mods': " + string.Join(", ", _summonBases.Select(b => b.Key + " (as " + b.Value + ", " + _summonChains.Keys.Count(c => c.Item1 == b.Key) + " script(s) of its own)")));
			if (summons.Count == 0) return;
			Log.Write(LogChannel.General, "summons: " + summons.Count + " of the mods': " + string.Join(", ", summons.Select(s => (s.Spell ?? s.Summon) + " " + ModSummons.Outcomes[s.Outcome] + " (" + s.Steps.Count + " steps)")));
			chain.AddTransform((name, data) =>
			{
				if (!ModSummons.IsPack(name)) return data;
				List<string> problems = new List<string>();
				byte[] composed = ModSummons.Compose(data, summons, ModEffects.Category, problems);
				foreach (string p in problems) Log.Write(LogChannel.General, "summons: " + p);
				if (!ReferenceEquals(composed, data)) Log.Write(LogChannel.File, "summons: " + ModSummons.Pack + " composed, " + data.Length + " -> " + composed.Length + " bytes");
				return composed;
			});
		}

		// The new summons: a mod spell's number to the summon spell it is based on (4201..4208), and a spell and
		// outcome to its own script's chain in the pack.
		private const int FirstSummon = 4201;
		private static readonly Dictionary<int, int> _summonBases = new Dictionary<int, int>();
		private static readonly Dictionary<(int, int), int> _summonChains = new Dictionary<(int, int), int>();
		// An outcome's name in battle: by a new summon's spell and outcome, or one of the eight's by (-1 - level, outcome).
		private static readonly Dictionary<(int, int), string> _summonNames = new Dictionary<(int, int), string>();
		// The new summon being cast: the outcome's record it plays as (its base's) and its own spell, whose look it casts with.
		private static (int Played, int Own)? _casting;

		/// <summary>The summon spell a mod's new summon is based on (btl.PlayerTurnSystem: its outcomes' records are its base's); null for any other spell.</summary>
		public static int? SummonBase(int magicId) => _summonBases.TryGetValue(magicId, out int b) ? b : (int?)null;

		/// <summary>The chain of summon_script_command.pack a new summon plays for an outcome; null for its base's.</summary>
		public static int? SummonScript(int magicId, int outcome) => _summonChains.TryGetValue((magicId, outcome), out int c) ? c : (int?)null;

		/// <summary>What the battle shows as a summon's outcome is cast when a mod names it (btl.PlayerTurnSystem's help window); null for the game's.</summary>
		public static string SummonName(int ownSpell, int level, int outcome)
		{
			if (ownSpell >= 0 && _summonNames.TryGetValue((ownSpell, outcome), out string mine)) return mine;
			return ownSpell < 0 && _summonNames.TryGetValue((-1 - level, outcome), out string renamed) ? renamed : null;
		}

		/// <summary>A new summon cast as its base's outcome record: its own spell's look gives the cast (CastEffect); -1, -1 when done.</summary>
		public static void CastingSummon(int played, int own) => _casting = own >= 0 ? (played, own) : null;

		// The looks' casts (their "cast"): a spell's id to the pack its caster plays as it begins, -1 for none.
		private static Func<Dictionary<int, int>> _castsFrom;
		private static Dictionary<int, int> _casts;

		/// <summary>The pack a spell's caster plays as the spell begins when a mod's look sets one (btl.TurnSystem.magicStartEffect); null for the game's own.</summary>
		public static int? CastEffect(int magicId)
		{
			if (_casts == null && _castsFrom != null) { _casts = _castsFrom(); _castsFrom = null; }
			if (_casting is (int played, int own) && played == magicId && _casts != null && _casts.TryGetValue(own, out int mine)) return mine;
			return _casts != null && _casts.TryGetValue(magicId, out int pack) ? pack : (int?)null;
		}

		/// <summary>A spell's id from its number or its name (the game's, in any case, or a mod item's).</summary>
		private static int? SpellId(ContentChain chain, string text, List<ModItem> items)
		{
			if (string.IsNullOrWhiteSpace(text)) return null;
			if (int.TryParse(text, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int number)) return number;
			ModItem mine = items.FirstOrDefault(i => string.Equals(i.Name, text, StringComparison.OrdinalIgnoreCase) || string.Equals(i.Id, text, StringComparison.OrdinalIgnoreCase));
			if (mine != null) return mine.Number;
			if (_spellNames == null)
			{
				_spellNames = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
				_readingNames = true;
				try
				{
					GameTables tables = TableFiles.Read(chain, "ff3");
					foreach (SpellDefinition spell in tables.Spells) if (!string.IsNullOrEmpty(spell.Name) && !_spellNames.ContainsKey(spell.Name)) _spellNames[spell.Name] = spell.Id;
				}
				catch (Exception ex) { Log.Write(LogChannel.General, "spells: the spells' names were not read - " + ex.Message); }
				finally { _readingNames = false; }
			}
			return _spellNames.TryGetValue(text.Trim(), out int id) ? id : (int?)null;
		}

		public static void Register(ContentChain chain)
		{
			_casts = null;
			_castsFrom = null;
			_summonBases.Clear();
			_summonChains.Clear();
			_summonNames.Clear();
			_casting = null;
			ModEffects.Register(null);
			// --nomods: the game as shipped, definitions included - what Tools/parity.ps1 compares against.
			if (chain == null || chain.Game != "ff3" || Options.Get("nomods") != null) return;
			List<string> roots = new List<string>();
			if (!string.IsNullOrEmpty(GameArchive.ProjectDirectory)) roots.Add(GameArchive.ProjectDirectory);
			roots.AddRange(GameArchive.ActiveMods.Select(m => m.Directory).Where(d => !string.IsNullOrEmpty(d)));
			RegisterText(chain, roots);
			RegisterMonsters(chain, roots);
			RegisterModels(roots);
			ModEffects.Register(roots);
			List<string> notes = new List<string>();
			List<ModItem> items = ModItems.Load(roots, notes);
			foreach (string note in notes) Log.Write(LogChannel.General, "items: " + note);
			// Two definitions with one number: the first loaded keeps it, as a mod's files do.
			HashSet<int> numbers = new HashSet<int>();
			List<ModItem> kept = new List<ModItem>();
			foreach (ModItem item in items)
			{
				if (!numbers.Add(item.Number)) { Log.Write(LogChannel.General, "items: " + item.Id + " (" + item.Source + ") has number " + item.Number + ", already taken - skipped"); continue; }
				kept.Add(item);
			}
			Items = kept;
			WeaponMeshes.Register(kept);
			RegisterSpells(chain, roots, kept);
			RegisterSummons(chain, roots, kept);
			if (kept.Count == 0) return;
			Log.Write(LogChannel.General, "items: " + kept.Count + " of the mods' own: " + string.Join(", ", kept.Select(i => i.Number + " " + (i.Name ?? i.Id) + (i.Model != null ? " (" + i.Model + ")" : ""))));
			chain.AddTransform((name, data) =>
			{
				if (ModItems.IsPak(name))
				{
					List<string> problems = new List<string>();
					byte[] composed = ModItems.ComposePak(data, kept, problems);
					foreach (string p in problems) Log.Write(LogChannel.General, "items: " + p);
					Log.Write(LogChannel.File, "items: item_parameter.pak composed, " + data.Length + " -> " + composed.Length + " bytes");
					return composed;
				}
				if (ModItems.IsMsd(name))
				{
					byte[] composed = ModItems.ComposeMsd(data, kept);
					Log.Write(LogChannel.File, "items: eureka_item.msd composed, " + data.Length + " -> " + composed.Length + " bytes");
					return composed;
				}
				return data;
			});
		}
	}
}
