// The Gambits screens: where a hero's auto-battle rules (Gambits) are set, FFXII's way, in the game's own menus.
//
//   Gambits      Data/menus/gambits.xml - from the main menu, after the hero is picked. Twelve rules,
//                seven rows on screen (the cursor off the end scrolls), each a window of its own: the
//                number with ON / OFF, the condition (red for a foe, blue for an ally or the hero), the action. A on ON / OFF turns the rule on or off; on the
//                condition or the action it opens the picker. X empties the row, Y moves it up one
//                (the rules are read top to bottom), L / R go to the next hero. B leaves. On the keyboard
//                X is C, Y is V, L / R are Q / E (its own X is B), and the help lines say so.
//   The picker   Data/menus/gambit-pick.xml - the conditions or the actions on the left (L / R turn
//                the pages), the hero's rules on the right with the one being set in yellow. A takes
//                the choice, B goes back to the rules unchanged. The actions are those that suit the
//                condition's side (no Attack on an ally) - every battle spell and item of the game's, what
//                the hero cannot use now greyed, or with Y only the usable ones.
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
			/// <summary>An entry that opens a list of its own ("magic", "item") instead of being chosen.</summary>
			public string Opens;
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

		/// <summary>The picker's filter: only the spells the hero has set and the items in the bag (Y), or everything the game has.</summary>
		public static bool OnlyUsable;

		/// <summary>
		/// The actions for the hero on the condition's side (null: any). The first list (list null): Remove, Attack (foes
		/// only), Guard, Run Away, and Use Magic / Use Item, each opening its own. "magic" / "item": the game's battle
		/// spells or battle items that can be used on that side - all of them, what the hero cannot use now greyed (a rule
		/// can be set before the spell is learnt or the item bought; the battle passes over it until then), or only the
		/// usable ones.
		/// </summary>
		public static List<Choice> Actions(int hero, GambitSide? side, bool onlyUsable, string list = null)
		{
			string name = HeroName(hero);
			if (list == null)
			{
				List<Choice> first = new List<Choice> { new Choice { Name = "Remove", Help = "Empties this gambit's slot.", Remove = true, Colour = MenuColour.PaleYellow } };
				if (side == null || side == GambitSide.Foe) first.Add(new Choice { Key = "attack", Name = "Attack", Help = "Attack the target with what is in hand." });
				first.Add(new Choice { Key = "guard", Name = "Guard", Help = "Guard for the round; the target does not matter." });
				first.Add(new Choice { Key = "run", Name = "Run Away", Help = "Flee the battle." });
				first.Add(new Choice { Opens = "magic", Name = "Use Magic", Help = "Cast a spell on the target - the ones " + name + " has not set shown greyed." });
				first.Add(new Choice { Opens = "item", Name = "Use Item", Help = "Use an item on the target - the ones not in the bag shown greyed." });
				return first;
			}
			List<Choice> result = new List<Choice>();
			// What the hero can use now first, then the greyed rest.
			List<Choice> spellsUsable = new List<Choice>(), spellsNot = new List<Choice>(), itemsUsable = new List<Choice>(), itemsNot = new List<Choice>();
			try
			{
				HashSet<int> known = new HashSet<int>(Game.Party.Member(hero)?.Spells ?? new List<int>());
				IEnumerable<Spell> spells = Game.Magic.All.Where(s => s != null && s.InBattle && s.School != MagicSchool.Enemy && !string.IsNullOrEmpty(s.Name))
					.OrderBy(s => s.Level).ThenBy(s => s.School).ThenBy(s => s.Id);
				foreach (Spell spell in spells)
				{
					bool have = known.Contains(spell.Id);
					if (onlyUsable && !have) continue;
					GlobalScope.itm.MagicParameter p = GlobalScope.itm.ItemManager.instance().magicParameter((short)spell.Id);
					if (p == null || !Fits(side, p.targetPosition())) continue;
					(have ? spellsUsable : spellsNot).Add(new Choice
					{
						Key = "magic", Param = spell.Id, Name = spell.Name,
						Help = (spell.Caption ?? "") + (have ? "" : "   (" + name + " has not set it)"),
						Colour = have ? MenuColour.White : MenuColour.Disabled
					});
				}
			}
			catch (Exception) { }
			try
			{
				GlobalScope.itm.ItemUse use = new GlobalScope.itm.ItemUse();
				foreach (Item item in Game.Items.All)
				{
					if (item == null || string.IsNullOrEmpty(item.Name)) continue;
					int id = item.Id;
					if (!use.isUseInBattle(id) || GlobalScope.itm.ItemManager.instance().consumptionParameter((short)id) == null) continue;
					GlobalScope.itm.ItemBaseParameter p = GlobalScope.itm.ItemManager.instance().itemParameter((short)id);
					if (p == null || !Fits(side, p.targetPosition())) continue;
					int count = Held(id);
					if (onlyUsable && count <= 0) continue;
					(count > 0 ? itemsUsable : itemsNot).Add(new Choice
					{
						Key = "item", Param = id, Name = item.Name + (count > 0 ? "  x" + count : ""),
						Help = (item.Caption ?? "") + (count > 0 ? "" : "   (none in the bag)"),
						Colour = count > 0 ? MenuColour.PaleYellow : MenuColour.Disabled
					});
				}
			}
			catch (Exception) { }
			if (list == "magic") { result.AddRange(spellsUsable); result.AddRange(spellsNot); }
			else { result.AddRange(itemsUsable); result.AddRange(itemsNot); }
			return result;
		}

		/// <summary>How many of an item the party holds.</summary>
		private static int Held(int id)
		{
			try { return GlobalScope.pl.PlayerParty.instance().item().serchNormalItem((short)id)?.itemNumber() ?? 0; } catch (Exception) { return 0; }
		}

		/// <summary>
		/// Whether a spell's or item's side suits the condition's (any for none): its targetPosition, where the game's
		/// target cursor starts - 0/1 on the foes, 2 on the user, 3/4 on an ally; the user's is the allies' side
		/// (Potion, Cure: any ally). Its targetPossible would not do: most spells can be aimed either way (Cure at
		/// the undead, Fire at a friend).
		/// </summary>
		private static bool Fits(GambitSide? side, short position)
		{
			switch (side)
			{
				case GambitSide.Foe: return position == 0 || position == 1;
				case GambitSide.Ally:
				case GambitSide.Self: return position >= 2 && position <= 4;
			}
			return true;
		}

		/// <summary>A button's name as the player presses it: the pad's, or the keyboard's key for it (DesktopInput).</summary>
		public static string Button(MenuKey key)
		{
			bool pad = DesktopInput.PadConnected;
			switch (key)
			{
				case MenuKey.X: return pad ? "X" : "C";
				case MenuKey.Y: return pad ? "Y" : "V";
				case MenuKey.L: return pad ? "L" : "Q";
				case MenuKey.R: return pad ? "R" : "E";
			}
			return key.ToString();
		}

		/// <summary>The confirm button's name: the pad's A, or the keyboard's Z.</summary>
		public static string Confirm => DesktopInput.PadConnected ? "A" : "Z";

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
			Menu.Portrait = GambitEditor.Hero;
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
			Menu.SetText("hero", "Gambits  -  " + GambitEditor.HeroName(hero) + "      ( " + GambitEditor.Button(MenuKey.L) + " / " + GambitEditor.Button(MenuKey.R) + ": another hero )");
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
			// Seven rows of the twelve slots: the scroll arrows at the rows' right, lit the ways there are more.
			Menu.ScrollArrows("w_row0", "w_row" + (Rows - 1), true, GambitEditor.Top > 0, GambitEditor.Top + Rows < Gambits.Slots);
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
			// The buttons by the names the player presses: the pad's, or the keyboard's (C and V - its X is the pad's B, back).
			string a = GambitEditor.Confirm, x = GambitEditor.Button(MenuKey.X), y = GambitEditor.Button(MenuKey.Y);
			if (column == "on") text = rule.IsEmpty ? "An empty slot.   " + a + ": choose a condition." : (rule.On ? "On" : "Off") + " - " + a + ": turn it " + (rule.On ? "off" : "on") + ".   " + x + ": remove   " + y + ": move up";
			else if (column == "cond") text = rule.IsEmpty || string.IsNullOrEmpty(rule.Condition) ? a + ": choose the target this gambit looks for." : GambitEditor.ConditionHelp(rule.Condition, rule.ConditionParam);
			else text = string.IsNullOrEmpty(rule.Action) ? a + ": choose what to do to the target." : GambitEditor.ActionHelp(rule);
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
					Menu.Portrait = GambitEditor.Hero;   // the face follows: a portrait frame of the layout's, or the game's own
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
		/// <summary>The actions' own list open: "magic" or "item" (Use Magic / Use Item); null for the first list.</summary>
		private string _sub;

		public override void OnOpen()
		{
			int hero = GambitEditor.Hero, slot = GambitEditor.Slot;
			Menu.SetText("rules_title", GambitEditor.HeroName(hero) + " - Gambits");
			List<Gambit> slots = Gambits.Slotted(hero);
			for (int i = 0; i < Gambits.Slots; i++)
			{
				Gambit r = slots[i];
				Menu.SetText("rule" + i, (i + 1) + "  " + (r.IsEmpty ? "-" : GambitEditor.ConditionText(r) + "  >  " + GambitEditor.ActionText(r)));
				IMenuWidget w = Menu.Widget("rule" + i);
				if (w != null) w.Colour = i == slot ? MenuColour.Yellow : r.IsEmpty || !r.On ? MenuColour.Disabled : MenuColour.White;
			}
			_list = new MenuList(Menu, "opt", Rows, "page") { ColourOf = i => _choices[i].Colour };
			_sub = null;
			// The cursor on what the row has now: a spell or an item on its list's entry.
			Gambit current = slots[slot];
			_choices = Choices();
			int at = GambitEditor.PickingAction
				? _choices.FindIndex(c => current.Action == "magic" || current.Action == "item" ? c.Opens == current.Action : !c.Remove && c.Opens == null && c.Key == current.Action)
				: _choices.FindIndex(c => !c.Remove && c.Key == current.Condition && c.Param == current.ConditionParam);
			ShowList(at);
		}

		/// <summary>The conditions, or the actions that suit the rule's condition (its side) - the first list or Magic's / Item's, as the filter has them.</summary>
		private List<GambitEditor.Choice> Choices()
		{
			if (!GambitEditor.PickingAction) return GambitEditor.Conditions();
			Gambit rule = Gambits.Slotted(GambitEditor.Hero)[GambitEditor.Slot];
			GambitSide? side = Gambits.Conditions.TryGetValue(rule.Condition ?? "", out GambitCondition c) ? c.Side : null;
			return GambitEditor.Actions(GambitEditor.Hero, side, GambitEditor.OnlyUsable, _sub);
		}

		/// <summary>The choices on the rows, the cursor on one (the second - past Remove - when it is not there).</summary>
		private void ShowList(int at)
		{
			Menu.SetText("title", !GambitEditor.PickingAction ? "Conditions" : _sub == "magic" ? "Actions  >  Magic" : _sub == "item" ? "Actions  >  Item" : "Actions");
			_list.Items = _choices.Select(c => c.Name).ToList();
			if (at < 0) at = _sub != null ? 0 : Math.Min(1, _choices.Count - 1);
			at = Math.Max(0, at);
			_list.Top = at / Rows * Rows;
			_list.Show();
			Menu.Focus("opt" + (at % Rows));
			Describe();
		}

		private void Describe()
		{
			int index = _list.IndexAt(Menu.Focused);
			string help = index >= 0 ? _choices[index].Help : "";
			if (_choices.Count == 0) help = "Nothing " + GambitEditor.HeroName(GambitEditor.Hero) + " can use here yet.   " + GambitEditor.Button(MenuKey.Y) + ": show all";
			Menu.SetText("help", help);
			// The page (the list writes "2 / 5" there), with the pages' keys, and the filter of Magic's and Item's lists.
			string page = _list.Pages > 1 ? GambitEditor.Button(MenuKey.L) + " / " + GambitEditor.Button(MenuKey.R) + ":  " + (_list.Top / _list.Rows + 1) + " / " + _list.Pages : "";
			if (_sub != null) page = GambitEditor.Button(MenuKey.Y) + ": " + (GambitEditor.OnlyUsable ? "show all" : "only usable") + (page.Length > 0 ? "     " + page : "");
			Menu.SetText("page", page);
		}

		public override void OnFocus()
		{
			_list.OnFocus();
			Describe();
		}

		public override bool OnKey(MenuKey key)
		{
			if (key == MenuKey.Y && _sub != null)
			{
				// The filter: everything the game has (what the hero cannot use now greyed), or only the usable. The cursor stays on its choice if it is still there.
				int index = _list.IndexAt(Menu.Focused);
				GambitEditor.Choice was = index >= 0 ? _choices[index] : null;
				GambitEditor.OnlyUsable = !GambitEditor.OnlyUsable;
				_choices = Choices();
				Menu.SoundDecide();
				ShowList(was == null ? -1 : _choices.FindIndex(c => c.Key == was.Key && c.Param == was.Param));
				return true;
			}
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
			if (choice.Opens != null)
			{
				// Use Magic / Use Item: its own list, the cursor on the rule's spell or item if it has one.
				_sub = choice.Opens;
				_choices = Choices();
				Menu.SoundDecide();
				ShowList(rule.Action == _sub ? _choices.FindIndex(c => c.Param == rule.ActionParam) : -1);
				return true;
			}
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
			if (_sub != null)
			{
				// Out of Magic's or Item's list: back to the first, on its entry.
				string was = _sub;
				_sub = null;
				_choices = Choices();
				ShowList(_choices.FindIndex(c => c.Opens == was));
				return true;
			}
			Menu.Open("gambits");
			return true;
		}
	}
}
