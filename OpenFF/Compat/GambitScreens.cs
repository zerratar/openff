// The Gambits screens: where a hero's auto-battle rules (Gambits) are set, FFXII's way, in the game's own menus.
//
//   Gambits      Data/menus/gambits.xml - from the main menu, after the hero is picked. Twelve rules,
//                seven rows on screen (the cursor off the end scrolls), each a window of its own: the
//                number with ON / OFF, the condition (red for a foe, blue for an ally or the hero), the action. A on ON / OFF turns the rule on or off; on the
//                condition or the action it opens the picker. X empties the row, Y moves it up one
//                (the rules are read top to bottom), L / R go to the next hero. B leaves.
//   The picker   Data/menus/gambit-pick.xml - the conditions or the actions on the left (L / R turn
//                the pages), the hero's rules on the right with the one being set in yellow. A takes
//                the choice, B goes back to the rules unchanged.
//
// The screens are the client's own, shipped beside it (Data/menus) and built by the same menu system
// as a mod's screens (ModMenus): the game's window art, its hand cursor and its lettering.

using System;
using System.Collections.Generic;
using System.Linq;
using OpenFF;

namespace OpenFF.Client
{
	/// <summary>What the two screens share: the hero, the row being set, what is being picked, where the cursor goes back to.</summary>
	internal static class GambitEditor
	{
		public static int Hero = -1;
		public static int Slot;
		public static bool PickingAction;
		/// <summary>Where the cursor goes back to: the column ("on" | "cond" | "act") and the slot (0..11).</summary>
		public static string ReturnColumn;
		public static int ReturnSlot = -1;
		/// <summary>The slot on the Gambits screen's first row (seven rows show twelve slots).</summary>
		public static int Top;

		/// <summary>The heroes in the party, in its order.</summary>
		public static List<PartyMember> Heroes()
		{
			try { return Game.Party.Members.Where(m => m != null).ToList(); } catch (Exception) { return new List<PartyMember>(); }
		}

		public static string HeroName(int hero)
		{
			try { return Game.Party.Member(hero)?.Name ?? ""; } catch (Exception) { return ""; }
		}

		// ---- the choices ----

		/// <summary>One thing to choose: a condition (with its parameter) or an action (with its spell or item).</summary>
		public sealed class Choice
		{
			public string Key;
			public int Param;
			public string Name;
			public string Help;
			public MenuColour Colour = MenuColour.White;
			public bool Remove;
		}

		private static readonly (string Key, int[] Params)[] ConditionList =
		{
			("foe.any", null), ("foe.hp-lowest", null), ("foe.hp-highest", null), ("foe.hp-below", new[] { 90, 70, 50, 30, 10 }),
			("ally.any", null), ("ally.hp-below", new[] { 90, 70, 50, 30, 10 }), ("ally.ko", null),
			("ally.status", new[] { (int)GambitStatus.Poison, (int)GambitStatus.Blind, (int)GambitStatus.Silence, (int)GambitStatus.Mini, (int)GambitStatus.Toad, (int)GambitStatus.Stone }),
			("self", null), ("self.hp-below", new[] { 70, 50, 30, 10 }),
			("self.status", new[] { (int)GambitStatus.Poison, (int)GambitStatus.Blind, (int)GambitStatus.Silence, (int)GambitStatus.Mini, (int)GambitStatus.Toad }),
		};

		/// <summary>Every condition to choose from, each parameter its own entry as FFXII lists them ("Ally: HP < 70%").</summary>
		public static List<Choice> Conditions()
		{
			List<Choice> list = new List<Choice> { new Choice { Name = "Remove", Help = "Empties this gambit's slot.", Remove = true, Colour = MenuColour.PaleYellow } };
			foreach ((string key, int[] ps) in ConditionList)
			{
				if (!Gambits.Conditions.TryGetValue(key, out GambitCondition c)) continue;
				foreach (int p in ps ?? new[] { 0 })
				{
					list.Add(new Choice { Key = key, Param = p, Name = c.Describe(p), Help = ConditionHelp(key, p), Colour = SideColour(c.Side) });
				}
			}
			return list;
		}

		public static MenuColour SideColour(GambitSide side) => side == GambitSide.Foe ? MenuColour.PaleRed : MenuColour.PaleBlue;

		public static string ConditionHelp(string key, int p)
		{
			string status = ((GambitStatus)p).ToString();
			switch (key)
			{
				case "foe.any": return "Target any foe, the front row first.";
				case "foe.hp-lowest": return "Target the foe with the lowest HP.";
				case "foe.hp-highest": return "Target the foe with the highest HP.";
				case "foe.hp-below": return "Target a foe whose HP is below " + p + "%.";
				case "ally.any": return "Target any ally, the hero included.";
				case "ally.hp-below": return "Target an ally whose HP is below " + p + "%, the lowest first.";
				case "ally.ko": return "Target an ally who is KO'd.";
				case "ally.status": return "Target an ally with " + status + ".";
				case "self": return "Target the hero themself.";
				case "self.hp-below": return "Target the hero when their HP is below " + p + "%.";
				case "self.status": return "Target the hero when they have " + status + ".";
			}
			return "";
		}

		/// <summary>Every action for the hero: Attack and Guard, the spells they have set, the battle items in the bag, Run Away.</summary>
		public static List<Choice> Actions(int hero)
		{
			List<Choice> list = new List<Choice>
			{
				new Choice { Name = "Remove", Help = "Empties this gambit's slot.", Remove = true, Colour = MenuColour.PaleYellow },
				new Choice { Key = "attack", Name = "Attack", Help = "Attack the target with what is in hand." },
				new Choice { Key = "guard", Name = "Guard", Help = "Guard for the round; the target does not matter." },
			};
			try
			{
				PartyMember m = Game.Party.Member(hero);
				foreach (int id in (m?.Spells ?? new List<int>()).Distinct())
				{
					Spell spell = Game.Magic.Find(id);
					if (spell == null || !spell.InBattle) continue;
					list.Add(new Choice { Key = "magic", Param = id, Name = spell.Name, Help = spell.Caption ?? "", Colour = MenuColour.White });
				}
			}
			catch (Exception) { }
			try
			{
				GlobalScope.itm.ItemUse use = new GlobalScope.itm.ItemUse();
				HashSet<int> seen = new HashSet<int>();
				for (int i = 0; i < 384; i++)
				{
					GlobalScope.itm.PossessionItem held = GlobalScope.pl.PlayerParty.instance().item().normalItem(i);
					if (held == null) continue;
					int id = held.itemId();
					if (id <= 0 || held.itemNumber() <= 0 || !seen.Add(id)) continue;
					if (!use.isUseInBattle(id) || GlobalScope.itm.ItemManager.instance().consumptionParameter((short)id) == null) continue;
					Item item = Game.Items.Find(id);
					list.Add(new Choice { Key = "item", Param = id, Name = (item?.Name ?? ("item " + id)) + "  x" + held.itemNumber(), Help = item?.Caption ?? "", Colour = MenuColour.PaleYellow });
				}
			}
			catch (Exception) { }
			list.Add(new Choice { Key = "run", Name = "Run Away", Help = "Flee the battle." });
			return list;
		}

		/// <summary>What an action does, for the help line.</summary>
		public static string ActionHelp(Gambit rule)
		{
			switch (rule.Action)
			{
				case "attack": return "Attack the target with what is in hand.";
				case "guard": return "Guard for the round.";
				case "run": return "Flee the battle.";
				case "magic": try { return Game.Magic.Find(rule.ActionParam)?.Caption ?? ""; } catch (Exception) { return ""; }
				case "item": try { return Game.Items.Find(rule.ActionParam)?.Caption ?? ""; } catch (Exception) { return ""; }
			}
			return "";
		}

		public static string ConditionText(Gambit rule) => string.IsNullOrEmpty(rule.Condition) ? "-" : Gambits.Conditions.TryGetValue(rule.Condition, out GambitCondition c) ? c.Describe(rule.ConditionParam) : rule.Condition;

		public static string ActionText(Gambit rule) => string.IsNullOrEmpty(rule.Action) ? "-" : Gambits.Actions.TryGetValue(rule.Action, out GambitAction a) ? a.Describe(rule.ActionParam) : rule.Action;

		public static MenuColour ConditionColour(Gambit rule)
		{
			if (rule.IsEmpty || !rule.On) return MenuColour.Disabled;
			return Gambits.Conditions.TryGetValue(rule.Condition ?? "", out GambitCondition c) ? SideColour(c.Side) : MenuColour.Disabled;
		}
	}

	/// <summary>The Gambits screen: a hero's twelve rules.</summary>
	public sealed class GambitsScreen : MenuBehaviour
	{
		private const int Rows = 7;
		private List<Gambit> _slots = new List<Gambit>();
		// The menu tells a direction after the cursor has taken it: set when this key moved the cursor, so only a key
		// that could not (the top or the bottom row's neighbour that way is "dummy") scrolls.
		private bool _moved;

		public override void OnOpen()
		{
			if (Menu.Hero >= 0) GambitEditor.Hero = Menu.Hero;
			if (GambitEditor.Hero < 0) GambitEditor.Hero = GambitEditor.Heroes().FirstOrDefault()?.Id ?? 0;
			string column = GambitEditor.ReturnColumn ?? "cond";
			int slot = GambitEditor.ReturnSlot >= 0 ? GambitEditor.ReturnSlot : 0;
			if (GambitEditor.ReturnSlot < 0) GambitEditor.Top = 0;
			GambitEditor.ReturnColumn = null;
			GambitEditor.ReturnSlot = -1;
			FocusSlot(column, slot);
		}

		/// <summary>The cursor on a slot's column, the rows scrolled so that it shows.</summary>
		private void FocusSlot(string column, int slot)
		{
			if (slot < GambitEditor.Top) GambitEditor.Top = slot;
			if (slot >= GambitEditor.Top + Rows) GambitEditor.Top = slot - Rows + 1;
			GambitEditor.Top = Math.Clamp(GambitEditor.Top, 0, Gambits.Slots - Rows);
			Fill();
			Menu.Focus(column + (slot - GambitEditor.Top));
			_moved = false;
			Describe();
		}

		private void Fill()
		{
			int hero = GambitEditor.Hero;
			_slots = Gambits.Slotted(hero);
			Menu.SetText("hero", "Gambits  -  " + GambitEditor.HeroName(hero) + "      ( L / R: another hero )");
			for (int r = 0; r < Rows; r++)
			{
				int i = GambitEditor.Top + r;
				Gambit rule = _slots[i];
				Menu.SetText("on" + r, (i + 1).ToString().PadLeft(2) + "   " + (rule.IsEmpty ? "-" : rule.On ? "ON" : "OFF"));
				Menu.SetText("cond" + r, GambitEditor.ConditionText(rule));
				Menu.SetText("act" + r, GambitEditor.ActionText(rule));
				Colour("on" + r, rule.IsEmpty || !rule.On ? MenuColour.Disabled : MenuColour.Yellow);
				Colour("cond" + r, GambitEditor.ConditionColour(rule));
				Colour("act" + r, rule.IsEmpty || !rule.On || string.IsNullOrEmpty(rule.Action) ? MenuColour.Disabled : MenuColour.White);
			}
		}

		private void Colour(string id, MenuColour colour)
		{
			IMenuWidget w = Menu.Widget(id);
			if (w != null) w.Colour = colour;
		}

		/// <summary>The column and the slot (not the row) the cursor is on: ("on" | "cond" | "act", 0..11), or (null, -1).</summary>
		private (string Column, int Row) At()
		{
			string id = Menu.Focused ?? "";
			foreach (string column in new[] { "cond", "act", "on" })
			{
				if (id.StartsWith(column, StringComparison.Ordinal) && int.TryParse(id.Substring(column.Length), out int row)) return (column, GambitEditor.Top + row);
			}
			return (null, -1);
		}

		private void Describe()
		{
			(string column, int row) = At();
			if (row < 0 || row >= _slots.Count) { Menu.SetText("help", ""); return; }
			Gambit rule = _slots[row];
			string text;
			if (column == "on") text = rule.IsEmpty ? "An empty slot.   A: choose a condition." : (rule.On ? "On" : "Off") + " - A: turn it " + (rule.On ? "off" : "on") + ".   X: remove   Y: move up";
			else if (column == "cond") text = rule.IsEmpty || string.IsNullOrEmpty(rule.Condition) ? "A: choose the target this gambit looks for." : GambitEditor.ConditionHelp(rule.Condition, rule.ConditionParam);
			else text = string.IsNullOrEmpty(rule.Action) ? "A: choose what to do to the target." : GambitEditor.ActionHelp(rule);
			Menu.SetText("help", text);
		}

		public override void OnFocus()
		{
			_moved = true;
			Describe();
		}

		public override bool OnPress()
		{
			(string column, int row) = At();
			if (row < 0) return false;
			Gambit rule = _slots[row];
			if (column == "on")
			{
				if (rule.IsEmpty) { Pick(row, action: false); return true; }
				rule.On = !rule.On;
				Save();
				Menu.SoundDecide();
				Fill();
				Describe();
				return true;
			}
			Pick(row, column == "act");
			return true;
		}

		private void Pick(int row, bool action)
		{
			GambitEditor.Slot = row;
			GambitEditor.PickingAction = action;
			GambitEditor.ReturnColumn = action ? "act" : "cond";
			GambitEditor.ReturnSlot = row;
			Menu.SoundDecide();
			Menu.Open("gambit-pick");
		}

		private void Save() => Gambits.Set(GambitEditor.Hero, _slots);

		public override bool OnKey(MenuKey key)
		{
			(string column, int row) = At();
			switch (key)
			{
				case MenuKey.L:
				case MenuKey.R:
				{
					List<PartyMember> heroes = GambitEditor.Heroes();
					if (heroes.Count < 2) { Menu.SoundBeep(); return true; }
					int at = Math.Max(0, heroes.FindIndex(m => m.Id == GambitEditor.Hero));
					at = (at + (key == MenuKey.R ? 1 : heroes.Count - 1)) % heroes.Count;
					GambitEditor.Hero = heroes[at].Id;
					Menu.SoundDecide();
					Fill();
					Describe();
					return true;
				}
				case MenuKey.X:
					if (row < 0 || _slots[row].IsEmpty) { Menu.SoundBeep(); return true; }
					_slots[row] = Gambit.Empty();
					Save();
					Menu.SoundCancel();
					Fill();
					Describe();
					return true;
				case MenuKey.Y:
					if (row <= 0 || _slots[row].IsEmpty) { Menu.SoundBeep(); return true; }
					(_slots[row - 1], _slots[row]) = (_slots[row], _slots[row - 1]);
					Save();
					Menu.SoundDecide();
					FocusSlot(column, row - 1);
					return true;
				case MenuKey.Up:
				case MenuKey.Down:
				{
					// Off the top or the bottom row (its neighbour that way is "dummy"): the rows scroll, and past the ends wrap round.
					bool moved = _moved;
					_moved = false;
					if (row < 0 || moved) return false;
					int onRow = row - GambitEditor.Top;
					if (key == MenuKey.Down && onRow == Rows - 1) { FocusSlot(column, (row + 1) % Gambits.Slots); return true; }
					if (key == MenuKey.Up && onRow == 0) { FocusSlot(column, (row + Gambits.Slots - 1) % Gambits.Slots); return true; }
					return false;
				}
			}
			return false;
		}
	}

	/// <summary>The picker: the conditions or the actions for the row being set.</summary>
	public sealed class GambitPickScreen : MenuBehaviour
	{
		private const int Rows = 10;
		private MenuList _list;
		private List<GambitEditor.Choice> _choices = new List<GambitEditor.Choice>();

		public override void OnOpen()
		{
			int hero = GambitEditor.Hero, slot = GambitEditor.Slot;
			bool action = GambitEditor.PickingAction;
			_choices = action ? GambitEditor.Actions(hero) : GambitEditor.Conditions();
			Menu.SetText("title", action ? "Actions" : "Conditions");
			Menu.SetText("rules_title", GambitEditor.HeroName(hero) + " - Gambits");
			List<Gambit> slots = Gambits.Slotted(hero);
			for (int i = 0; i < Gambits.Slots; i++)
			{
				Gambit r = slots[i];
				Menu.SetText("rule" + i, (i + 1) + "  " + (r.IsEmpty ? "-" : GambitEditor.ConditionText(r) + "  >  " + GambitEditor.ActionText(r)));
				IMenuWidget w = Menu.Widget("rule" + i);
				if (w != null) w.Colour = i == slot ? MenuColour.Yellow : r.IsEmpty || !r.On ? MenuColour.Disabled : MenuColour.White;
			}
			_list = new MenuList(Menu, "opt", Rows, "page")
			{
				Items = _choices.Select(c => c.Name).ToList(),
				ColourOf = i => _choices[i].Colour
			};
			// The cursor on what the row has now.
			Gambit current = slots[slot];
			int at = _choices.FindIndex(c => !c.Remove && (action ? c.Key == current.Action && (c.Key != "magic" && c.Key != "item" || c.Param == current.ActionParam)
				: c.Key == current.Condition && c.Param == current.ConditionParam));
			if (at < 0) at = Math.Min(1, _choices.Count - 1);
			_list.Top = at / Rows * Rows;
			_list.Show();
			Menu.Focus("opt" + (at % Rows));
			Describe();
		}

		private void Describe()
		{
			int index = _list.IndexAt(Menu.Focused);
			Menu.SetText("help", index >= 0 ? _choices[index].Help : "");
		}

		public override void OnFocus()
		{
			_list.OnFocus();
			Describe();
		}

		public override bool OnKey(MenuKey key)
		{
			if (!_list.OnKey(key)) return false;
			Describe();
			return true;
		}

		public override bool OnPress()
		{
			int index = _list.IndexAt(Menu.Focused);
			if (index < 0) { Menu.SoundBeep(); return true; }
			GambitEditor.Choice choice = _choices[index];
			List<Gambit> slots = Gambits.Slotted(GambitEditor.Hero);
			Gambit rule = slots[GambitEditor.Slot];
			if (choice.Remove) rule = Gambit.Empty();
			else
			{
				if (rule.IsEmpty) rule = new Gambit { On = true, Condition = "", Action = "" };
				if (GambitEditor.PickingAction) { rule.Action = choice.Key; rule.ActionParam = choice.Param; }
				else { rule.Condition = choice.Key; rule.ConditionParam = choice.Param; }
			}
			slots[GambitEditor.Slot] = rule;
			Gambits.Set(GambitEditor.Hero, slots);
			Menu.SoundDecide();
			// A new rule's condition set, the cursor goes on to its action; otherwise back where it was.
			if (!GambitEditor.PickingAction && !choice.Remove && string.IsNullOrEmpty(rule.Action)) GambitEditor.ReturnColumn = "act";
			else if (choice.Remove) GambitEditor.ReturnColumn = "on";
			Menu.Open("gambits");
			return true;
		}

		public override bool OnCancel()
		{
			Menu.SoundCancel();
			Menu.Open("gambits");
			return true;
		}
	}
}
