// FF4's Gambits screen: a member's twelve auto-battle rules (Gambits.cs; Ff4BattleAuto plays them), in the menu's layout
// as Steam's FF4 screens are (Data/hud/ff4_menu.xml's #gambits), as FF3's are in its own windows (GambitScreens.cs).
//
//   The rules    six of the twelve in view, three cells each: the number with ON / OFF, the condition (red for a foe, blue
//                for an ally or the member), the action. Enter on ON / OFF turns the rule on or off (an empty one: its
//                condition); on the condition or the action it opens the picker. C empties the rule, Tab moves it up one
//                (the rules are read top to bottom), Z / M the member before and after, Backspace leaves.
//   The picker   the conditions or the actions in the rules' window, two columns: Use Magic and Use Item open the
//                member's spells for the condition's side and the bag's battle items. Enter takes the choice, Backspace
//                goes back unchanged. A new condition with no action yet goes straight on to the action.

using System;
using System.Collections.Generic;
using OpenFF.Data;

namespace OpenFF.Client
{
	internal sealed partial class Ff4Menu
	{
		private const int GambitRows = 6, PickRows = 6;
		private int _gRow, _gCol, _gTop, _gPick, _gPickTop;
		private bool _gPicking, _gPickAction;
		private string _gSub;
		private List<GambitEditor.Choice> _gChoices = new List<GambitEditor.Choice>();

		private void OpenGambits()
		{
			_gRow = _gCol = _gTop = 0;
			_gPicking = false;
		}

		private static GambitSide? SideOf(Gambit rule) => Gambits.Conditions.TryGetValue(rule.Condition ?? "", out GambitCondition c) ? c.Side : null;

		private void UpdateGambits(InputState input)
		{
			Character c = Member;
			List<Gambit> slots = Gambits.Slotted(c.Id);
			if (_gPicking) { UpdateGambitPick(input, c, slots); return; }
			if (input.Pressed(Pad.B)) { Back(); return; }
			int n = Ff4Party.Party.Members.Count;
			if (input.Pressed(Pad.Y)) { _member = (_member + n - 1) % n; OpenGambits(); return; }
			if (input.KeyPressed("M")) { _member = (_member + 1) % n; OpenGambits(); return; }
			if (input.Pressed(Pad.Up)) _gRow = (_gRow + Gambits.Slots - 1) % Gambits.Slots;
			if (input.Pressed(Pad.Down)) _gRow = (_gRow + 1) % Gambits.Slots;
			if (input.Pressed(Pad.Left)) _gCol = Math.Max(0, _gCol - 1);
			if (input.Pressed(Pad.Right)) _gCol = Math.Min(2, _gCol + 1);
			if (_gRow < _gTop) _gTop = _gRow;
			if (_gRow >= _gTop + GambitRows) _gTop = _gRow - GambitRows + 1;
			Gambit rule = slots[_gRow];
			if (input.Pressed(Pad.X)) { slots[_gRow] = Gambit.Empty(); Gambits.Set(c.Id, slots); return; }
			if (input.KeyPressed("Tab") && _gRow > 0)
			{
				(slots[_gRow - 1], slots[_gRow]) = (slots[_gRow], slots[_gRow - 1]);
				Gambits.Set(c.Id, slots);
				_gRow--;
				if (_gRow < _gTop) _gTop = _gRow;
				return;
			}
			if (!input.Pressed(Pad.A)) return;
			if (_gCol == 0 && !rule.IsEmpty) { rule.On = !rule.On; Gambits.Set(c.Id, slots); return; }
			OpenGambitPick(c, slots, _gCol == 2 && !rule.IsEmpty, null);
		}

		/// <summary>The picker for the rule under the hand: its conditions, or its actions (<paramref name="sub"/>: "magic" or
		/// "item" for those lists), the hand on what the rule has.</summary>
		private void OpenGambitPick(Character c, List<Gambit> slots, bool action, string sub)
		{
			Gambit rule = slots[_gRow];
			_gPicking = true;
			_gPickAction = action;
			_gSub = sub;
			_gPickTop = 0;
			_gChoices = action ? GambitActions(c, SideOf(rule), sub) : GambitEditor.Conditions();
			int at = action
				? _gChoices.FindIndex(x => x.Opens == null && !x.Remove && x.Key == rule.Action && (sub == null || x.Param == rule.ActionParam))
				: _gChoices.FindIndex(x => !x.Remove && x.Key == rule.Condition && x.Param == rule.ConditionParam);
			if (action && at < 0 && sub == null) at = _gChoices.FindIndex(x => x.Opens != null && x.Opens == rule.Action);
			_gPick = Math.Max(0, at);
			ScrollPick();
		}

		private void ScrollPick()
		{
			int row = _gPick / 2;
			if (row < _gPickTop) _gPickTop = row;
			if (row >= _gPickTop + PickRows) _gPickTop = row - PickRows + 1;
		}

		private void UpdateGambitPick(InputState input, Character c, List<Gambit> slots)
		{
			if (input.Pressed(Pad.B))
			{
				if (_gSub != null) OpenGambitPick(c, slots, true, null);
				else _gPicking = false;
				return;
			}
			int count = _gChoices.Count;
			if (count == 0) return;
			if (input.Pressed(Pad.Left)) _gPick = Math.Max(0, _gPick - 1);
			if (input.Pressed(Pad.Right)) _gPick = Math.Min(count - 1, _gPick + 1);
			if (input.Pressed(Pad.Up) && _gPick >= 2) _gPick -= 2;
			if (input.Pressed(Pad.Down) && _gPick + 2 < count) _gPick += 2;
			ScrollPick();
			if (!input.Pressed(Pad.A)) return;
			GambitEditor.Choice choice = _gChoices[_gPick];
			if (choice.Opens != null) { OpenGambitPick(c, slots, true, choice.Opens); return; }
			Gambit rule = slots[_gRow];
			if (choice.Remove) rule = Gambit.Empty();
			else
			{
				if (rule.IsEmpty) rule = new Gambit { On = true, Condition = "", Action = "" };
				if (_gPickAction) { rule.Action = choice.Key; rule.ActionParam = choice.Param; }
				else { rule.Condition = choice.Key; rule.ConditionParam = choice.Param; }
			}
			slots[_gRow] = rule;
			Gambits.Set(c.Id, slots);
			_gPicking = false;
			if (!_gPickAction && !choice.Remove && string.IsNullOrEmpty(rule.Action)) { _gCol = 2; OpenGambitPick(c, slots, true, null); }
		}

		/// <summary>The actions for a rule on <paramref name="side"/>: Remove, Attack (on foes), Guard, Run Away, Use Magic and
		/// Use Item (on allies: FF4's items are for one's own side); "magic" the member's spells for that side, "item" the bag's
		/// battle items and, greyed after them, the rest of the game's.</summary>
		private static List<GambitEditor.Choice> GambitActions(Character c, GambitSide? side, string sub)
		{
			GameTables tables = Ff4Party.Tables;
			List<GambitEditor.Choice> list = new List<GambitEditor.Choice>();
			bool foes = side == null || side == GambitSide.Foe, allies = side != GambitSide.Foe;
			if (sub == null)
			{
				list.Add(new GambitEditor.Choice { Name = "Remove", Help = "Empties this gambit's slot.", Remove = true, Colour = MenuColour.PaleYellow });
				if (foes) list.Add(new GambitEditor.Choice { Key = "attack", Name = CommandName(1), Help = tables?.AbilityHelp(1) ?? "" });
				list.Add(new GambitEditor.Choice { Key = "guard", Name = "Guard", Help = "Guard for the round; the target does not matter." });
				list.Add(new GambitEditor.Choice { Key = "run", Name = "Run Away", Help = "Flee the battle." });
				if (GambitActions(c, side, "magic").Count > 0) list.Add(new GambitEditor.Choice { Opens = "magic", Name = "Use Magic", Help = "Cast one of " + c.Name + "'s spells on the target." });
				if (allies) list.Add(new GambitEditor.Choice { Opens = "item", Name = "Use Item", Help = "Use an item on the target - the ones not in the bag shown greyed." });
				return list;
			}
			if (sub == "magic")
			{
				foreach (int id in c.Spells)
				{
					SpellDefinition spell = tables?.Spell(id);
					if (spell == null) continue;
					bool helps = Ff4Battle.Helps(spell);
					if (side != null && helps != allies) continue;
					list.Add(new GambitEditor.Choice { Key = "magic", Param = id, Name = spell.Name, Help = SpellHelp(id) });
				}
				return list;
			}
			List<GambitEditor.Choice> rest = new List<GambitEditor.Choice>();
			foreach (ItemDefinition item in tables?.Items ?? new List<ItemDefinition>())
			{
				if (item.Kind != ItemKind.Consumable || UsableEffect(item.Id) == null || item.Raw == null || item.Raw.Length < 0x14) continue;
				if ((BitConverter.ToUInt16(item.Raw, 0x12) & 2) == 0) continue;   // itm::ItemUse::isUseInBattle
				int held = Ff4Party.Party.CountItem(item.Id);
				(held > 0 ? list : rest).Add(new GambitEditor.Choice
				{
					Key = "item", Param = item.Id, Name = item.Name, Help = item.Caption ?? "",
					Colour = held > 0 ? MenuColour.White : MenuColour.Disabled,
				});
			}
			list.AddRange(rest);
			return list;
		}

		/// <summary>A spell's line as Magic shows it ("  3 MP    Restore a small amount of HP.").</summary>
		private static string SpellHelp(int id)
		{
			GameTables tables = Ff4Party.Tables;
			return tables != null && tables.AbilityHelpIds.TryGetValue(id, out int help) && help > 0 ? (tables.AbilityName(help)?.Trim() ?? "").Replace("", "") : "";
		}

		private static string GambitActionText(Gambit rule) => rule.Action == "attack" ? CommandName(1) : GambitEditor.ActionText(rule);

		private void FillGambits(Ff4MenuHud.Data h)
		{
			Character c = Member;
			GameTables tables = Ff4Party.Tables;
			h.Title = "Gambits";
			FillHead(h.Head, c);
			h.GambitPicking = _gPicking;
			h.MoveUpLabel = "Move Up";
			List<Gambit> slots = Gambits.Slotted(c.Id);
			for (int k = 0; k < h.Rule.Count; k++)
			{
				int i = _gTop + k;
				Ff4MenuHud.RuleRow row = h.Rule[k];
				Gambit rule = slots[i];
				GambitSide? side = SideOf(rule);
				row.Number = (i + 1).ToString();
				row.State = rule.IsEmpty ? "" : rule.On ? "ON" : "OFF";
				row.Condition = rule.IsEmpty ? "" : GambitEditor.ConditionText(rule);
				row.Action = rule.IsEmpty ? "" : GambitActionText(rule);
				row.Foe = side == GambitSide.Foe;
				row.Ally = side == GambitSide.Ally || side == GambitSide.Self;
				row.On = !rule.IsEmpty && rule.On;
				row.Off = rule.IsEmpty || !rule.On;
				row.Picked = _gPicking && i == _gRow;
				row.LitOn = !_gPicking && i == _gRow && _gCol == 0;
				row.LitCondition = !_gPicking && i == _gRow && _gCol == 1;
				row.LitAction = !_gPicking && i == _gRow && _gCol == 2;
			}
			Ff4MenuHud.Scroll(h.RuleScroll, Gambits.Slots, GambitRows, _gTop, 526.5f, true);
			for (int k = 0; k < h.Pick.Count; k++)
			{
				int i = _gPickTop * 2 + k;
				Ff4MenuHud.CellRow cell = h.Pick[k];
				cell.Present = _gPicking && i < _gChoices.Count;
				if (!cell.Present) continue;
				GambitEditor.Choice choice = _gChoices[i];
				cell.Name = choice.Name;
				cell.Count = choice.Key == "item" && Ff4Party.Party.CountItem(choice.Param) > 0 ? Ff4Party.Party.CountItem(choice.Param).ToString() : "";
				cell.Icon = choice.Key == "item" ? tables?.Item(choice.Param)?.Icon ?? -1 : choice.Key == "magic" ? tables?.AbilityIcon(choice.Param) ?? -1 : -1;
				cell.Lit = i == _gPick;
				cell.Dim = choice.Colour == MenuColour.Disabled;
				cell.Foe = choice.Colour == MenuColour.PaleRed;
				cell.Ally = choice.Colour == MenuColour.PaleBlue;
				cell.Gold = choice.Colour == MenuColour.PaleYellow;
			}
			Ff4MenuHud.Scroll(h.PickScroll, (_gChoices.Count + 1) / 2, PickRows, _gPickTop, 526.5f, true);
			Gambit lit = slots[_gRow];
			if (_gPicking) h.Help = _gPick < _gChoices.Count ? _gChoices[_gPick].Help ?? "" : "";
			else if (_gCol == 0) h.Help = lit.IsEmpty ? "An empty slot: Enter chooses its condition." : lit.On ? "On: Enter turns it off." : "Off: Enter turns it on.";
			else if (_gCol == 1) h.Help = lit.IsEmpty || string.IsNullOrEmpty(lit.Condition) ? "Enter chooses the target this gambit looks for." : GambitEditor.ConditionHelp(lit.Condition, lit.ConditionParam);
			else h.Help = string.IsNullOrEmpty(lit.Action) ? "Enter chooses what to do to the target."
				: lit.Action == "magic" ? SpellHelp(lit.ActionParam) : lit.Action == "item" ? tables?.Item(lit.ActionParam)?.Caption ?? "" : lit.Action == "attack" ? tables?.AbilityHelp(1) ?? "" : "";
		}
	}
}
