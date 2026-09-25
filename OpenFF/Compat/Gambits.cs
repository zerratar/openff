// Each hero's auto-battle rules - FFXII's gambits, in FF3.
//
// A hero has an ordered list of rules. A rule is on or off, and pairs a condition, which finds
// targets ("Foe: lowest HP", "Ally: HP < 50%", "Self"), with an action ("Attack", a spell, an item,
// "Guard"). When auto battle has the hero's turn (AutoBattle), the rules are read top to bottom:
// the first whose condition finds a target the action can be used on is what the hero does. None
// does - the hero attacks the first foe.
//
// Conditions and actions are registered by key, with a name for a screen to show and a parameter
// (the percentage of "HP < n%", the spell or item an action uses), so a rule is plain data: it
// saves as keys and numbers, a configuration screen can offer the registered ones, and more can be
// added (a mod, a later rule) without the rules changing shape. The rules keep in the save, per hero
// (an engine chunk, "openff/gambits"); a save without them, or a new game, starts from the defaults.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace OpenFF.Client
{
	/// <summary>One of a hero's rules: on or off, the condition that finds a target, the action used on it.</summary>
	internal sealed class Gambit
	{
		public bool On { get; set; } = true;
		/// <summary>A registered condition's key, e.g. "foe.any" or "ally.hp-below".</summary>
		public string Condition { get; set; } = "foe.any";
		/// <summary>The condition's parameter: the percentage of an HP rule, the status of a status rule.</summary>
		public int ConditionParam { get; set; }
		/// <summary>A registered action's key: "attack", "guard", "magic", "item", "run".</summary>
		public string Action { get; set; } = "attack";
		/// <summary>The action's parameter: the spell's or the item's id.</summary>
		public int ActionParam { get; set; }

		public Gambit Copy() => (Gambit)MemberwiseClone();

		/// <summary>An unused slot: no condition and no action (a screen shows "-"; the battle passes over it).</summary>
		public bool IsEmpty => string.IsNullOrEmpty(Condition) && string.IsNullOrEmpty(Action);

		/// <summary>An unused slot's rule.</summary>
		public static Gambit Empty() => new Gambit { On = false, Condition = "", Action = "" };

		/// <summary>How a screen or the log shows it: "Ally: HP < 50% -> Cure".</summary>
		public override string ToString()
		{
			string condition = Gambits.Conditions.TryGetValue(Condition, out GambitCondition c) ? c.Describe(ConditionParam) : Condition;
			string action = Gambits.Actions.TryGetValue(Action, out GambitAction a) ? a.Describe(ActionParam) : Action;
			return (On ? "" : "(off) ") + condition + " -> " + action;
		}
	}

	/// <summary>Who a rule can be about.</summary>
	internal enum GambitSide { Foe, Ally, Self }

	/// <summary>A combatant as the rules see it: the battle's character, which side, and what a condition asks of it.</summary>
	internal sealed class GambitTarget
	{
		public GlobalScope.btl.BaseBattleCharacter Character;
		public GambitSide Side;
		public int Hp, MaxHp;
		public bool Alive;          // can be fought or helped as it is (not KO'd, not stone)
		public bool Dead, Stone;
		public int Status;          // GambitStatus bits
		public int Order;           // the game's own order for "any": the front row first, as its target windows list them

		public int HpPercent => MaxHp > 0 ? Hp * 100 / MaxHp : 0;
	}

	/// <summary>The statuses a rule can ask about, as bits.</summary>
	[Flags]
	internal enum GambitStatus
	{
		None = 0, Poison = 1, Blind = 2, Silence = 4, Mini = 8, Toad = 16, Stone = 32, KO = 64, Sleep = 128, Paralysis = 256, Confusion = 512
	}

	/// <summary>A condition: its side, its name for a screen, and the targets it finds, best first.</summary>
	internal sealed class GambitCondition
	{
		public string Key;
		public GambitSide Side;
		/// <summary>The name with its parameter in it, e.g. "Ally: HP < {0}%".</summary>
		public string Name;
		/// <summary>The parameter a new rule starts with (a screen's default), and whether there is one.</summary>
		public int DefaultParam;
		public bool HasParam;
		/// <summary>From everyone on this side, the ones the condition holds for, in the order to try them.</summary>
		public Func<IEnumerable<GambitTarget>, int, IEnumerable<GambitTarget>> Find;

		public string Describe(int param) => HasParam ? string.Format(Name, Gambits.ParamText(this, param)) : Name;
	}

	/// <summary>An action: its name, and (in AutoBattle) whether it can be used on a target and how it is committed.</summary>
	internal sealed class GambitAction
	{
		public string Key;
		public string Name;
		/// <summary>The name of a use of it, e.g. the spell's own name for "magic".</summary>
		public Func<int, string> Describe;
	}

	internal static class Gambits
	{
		public static readonly Dictionary<string, GambitCondition> Conditions = new Dictionary<string, GambitCondition>(StringComparer.OrdinalIgnoreCase);
		public static readonly Dictionary<string, GambitAction> Actions = new Dictionary<string, GambitAction>(StringComparer.OrdinalIgnoreCase);

		/// <summary>How many rules a hero has room for, as in FFXII.</summary>
		public const int Slots = 12;

		private static readonly Dictionary<int, List<Gambit>> _heroes = new Dictionary<int, List<Gambit>>();

		static Gambits()
		{
			// Foes.
			Condition("foe.any", GambitSide.Foe, "Foe: any", t => t.Where(x => x.Alive).OrderBy(x => x.Order));
			Condition("foe.hp-lowest", GambitSide.Foe, "Foe: lowest HP", t => t.Where(x => x.Alive).OrderBy(x => x.Hp).ThenBy(x => x.Order));
			Condition("foe.hp-highest", GambitSide.Foe, "Foe: highest HP", t => t.Where(x => x.Alive).OrderByDescending(x => x.Hp).ThenBy(x => x.Order));
			Condition("foe.hp-below", GambitSide.Foe, "Foe: HP < {0}%", (t, p) => t.Where(x => x.Alive && x.HpPercent < p).OrderBy(x => x.HpPercent).ThenBy(x => x.Order), 50);
			// Allies (the hero among them).
			Condition("ally.any", GambitSide.Ally, "Ally: any", t => t.Where(x => x.Alive).OrderBy(x => x.Order));
			Condition("ally.hp-below", GambitSide.Ally, "Ally: HP < {0}%", (t, p) => t.Where(x => x.Alive && x.HpPercent < p).OrderBy(x => x.HpPercent).ThenBy(x => x.Order), 50);
			Condition("ally.ko", GambitSide.Ally, "Ally: status = KO", t => t.Where(x => x.Dead).OrderBy(x => x.Order));
			Condition("ally.status", GambitSide.Ally, "Ally: status = {0}", (t, p) => t.Where(x => (x.Status & p) != 0 && (x.Alive || (p & (int)(GambitStatus.KO | GambitStatus.Stone)) != 0)).OrderBy(x => x.Order), (int)GambitStatus.Poison);
			// The hero.
			Condition("self", GambitSide.Self, "Self", t => t.Where(x => x.Alive));
			Condition("self.hp-below", GambitSide.Self, "Self: HP < {0}%", (t, p) => t.Where(x => x.Alive && x.HpPercent < p), 30);
			Condition("self.status", GambitSide.Self, "Self: status = {0}", (t, p) => t.Where(x => x.Alive && (x.Status & p) != 0), (int)GambitStatus.Poison);

			Action("attack", "Attack", _ => "Attack");
			Action("guard", "Guard", _ => "Guard");
			Action("magic", "Magic", id => SpellName(id) ?? ("spell " + id));
			Action("item", "Item", id => ItemName(id) ?? ("item " + id));
			Action("run", "Run Away", _ => "Run Away");
		}

		private static void Condition(string key, GambitSide side, string name, Func<IEnumerable<GambitTarget>, IEnumerable<GambitTarget>> find)
		{
			Conditions[key] = new GambitCondition { Key = key, Side = side, Name = name, Find = (t, _) => find(t) };
		}

		private static void Condition(string key, GambitSide side, string name, Func<IEnumerable<GambitTarget>, int, IEnumerable<GambitTarget>> find, int defaultParam)
		{
			Conditions[key] = new GambitCondition { Key = key, Side = side, Name = name, Find = find, HasParam = true, DefaultParam = defaultParam };
		}

		private static void Action(string key, string name, Func<int, string> describe)
		{
			Actions[key] = new GambitAction { Key = key, Name = name, Describe = describe };
		}

		/// <summary>A condition's parameter as its name shows it: a percentage, or the status's name.</summary>
		public static string ParamText(GambitCondition condition, int param)
		{
			if (condition.Key.EndsWith(".status", StringComparison.Ordinal))
			{
				return ((GambitStatus)param).ToString();
			}
			return param.ToString();
		}

		/// <summary>A new hero's rules, and a save's without any: attack the first foe.</summary>
		public static List<Gambit> Defaults() => new List<Gambit> { new Gambit { On = true, Condition = "foe.any", Action = "attack" } };

		/// <summary>A hero's rules (the defaults until a save or a screen gives others).</summary>
		public static List<Gambit> For(int hero)
		{
			if (!_heroes.TryGetValue(hero, out List<Gambit> rules))
			{
				rules = Defaults();
				_heroes[hero] = rules;
			}
			return rules;
		}

		/// <summary>A hero's rules as the twelve slots a screen shows: theirs, then unused slots.</summary>
		public static List<Gambit> Slotted(int hero)
		{
			List<Gambit> slots = For(hero).Take(Slots).Select(r => r.Copy()).ToList();
			while (slots.Count < Slots) slots.Add(Gambit.Empty());
			return slots;
		}

		/// <summary>Replaces a hero's rules (a configuration screen's save; kept to the slots there are).</summary>
		public static void Set(int hero, IEnumerable<Gambit> rules)
		{
			List<Gambit> list = rules.Where(r => r != null).Take(Slots).Select(r => r.Copy()).ToList();
			while (list.Count > 0 && list[list.Count - 1].IsEmpty) list.RemoveAt(list.Count - 1);   // unused slots at the end are not kept
			_heroes[hero] = list;
		}

		/// <summary>Everyone back to the defaults: the title (a new game follows), or a save about to be read (its own rules, if any, come next).</summary>
		public static void Reset()
		{
			_heroes.Clear();
		}

		private static string SpellName(int id)
		{
			try { return OpenFF.Game.Magic.Find(id)?.Name; } catch (Exception) { return null; }
		}

		private static string ItemName(int id)
		{
			try { return OpenFF.Game.Items.Find(id)?.Name; } catch (Exception) { return null; }
		}

		// ---- the save ----

		/// <summary>The rules in the save: a chunk of the engine's, per hero.</summary>
		public sealed class Chunk : OpenFF.ISaveable
		{
			public string ChunkId => "openff/gambits";
			public int ChunkVersion => 1;

			public object Save()
			{
				Dictionary<string, List<Gambit>> heroes = new Dictionary<string, List<Gambit>>();
				foreach (KeyValuePair<int, List<Gambit>> hero in _heroes) heroes[hero.Key.ToString()] = hero.Value;
				return heroes.Count == 0 ? null : new Dictionary<string, object> { ["heroes"] = heroes };
			}

			public void Load(int version, JsonElement data)
			{
				_heroes.Clear();
				try
				{
					if (!data.TryGetProperty("heroes", out JsonElement heroes)) return;
					foreach (JsonProperty hero in heroes.EnumerateObject())
					{
						if (!int.TryParse(hero.Name, out int id)) continue;
						List<Gambit> rules = JsonSerializer.Deserialize<List<Gambit>>(hero.Value.GetRawText());
						if (rules != null) Set(id, rules);
					}
					Log.Write(LogChannel.File, "gambits: " + _heroes.Count + " hero(es)' rules from the save");
				}
				catch (Exception ex)
				{
					Log.Write(LogChannel.General, "gambits: the save's rules could not be read (" + ex.Message + "); the defaults");
					_heroes.Clear();
				}
			}
		}
	}
}
