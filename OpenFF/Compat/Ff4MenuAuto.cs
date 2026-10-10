// Abilities' auto-battle command as Steam's (MSSAbility): Enter on it lists, in two columns over the auto and command
// rows, the commands the member has learned that auto battle can use (readyEquipableAutoIDList: ability.bbd +0x24 bit 7,
// the current one left out unless it is a list), sorted by id - Steam's Dark Knight: Defend, Items, Darkness. A command
// that is a list (+0x14: White / Black Magic, Items, Summon, Bardsong, Ninjutsu) opens its spells or the bag's battle
// items in three columns (FUN_004d46cc, state 7), and the one picked is the auto command (list 5); Back returns a step.

using System;
using System.Collections.Generic;
using OpenFF.Data;

namespace OpenFF.Client
{
	internal sealed partial class Ff4Menu
	{
		private bool _autoPicking;
		private int _autoSubOf;                      // the list command whose spells or items are up, 0 for the commands
		private readonly List<int> _autoList = new List<int>();
		private int _autoAt, _autoTop;
		private const int AutoRows = 4;

		/// <summary>Player::learningAbility's commands, as the port keeps them: Attack, the slots, the class's by level, the augments.</summary>
		private static SortedSet<int> LearnedCommands(Character c)
		{
			SortedSet<int> learned = new SortedSet<int> { 1 };
			foreach (int id in Ff4Augments.Slots(c)) if (id > 0 && id < 256) learned.Add(id);
			try { foreach (int id in c.Definition.CommandsAt(c.Level)) if (id > 0 && id < 256) learned.Add(id); } catch (Exception) { }
			foreach (int id in c.Abilities) if (id > 0 && id < 256) learned.Add(id);
			return learned;
		}

		/// <summary>readyEquipableAutoIDList: the learned commands auto battle can use, the current one out unless it is a list.</summary>
		private static List<int> AutoCommands(Character c)
		{
			GameTables t = Ff4Party.Tables;
			List<int> list = new List<int>();
			foreach (int id in LearnedCommands(c))
			{
				if ((t.AbilityFlags(id) & 0x80) == 0) continue;
				if (id == c.AutoCommand && t.AbilityListKind(id) == 0) continue;
				list.Add(id);
			}
			return list;
		}

		/// <summary>FUN_004d46cc for the auto command: a list command's spells (the member's book of the school) or the bag's
		/// battle items, the current auto command left out.</summary>
		private static List<int> AutoSubList(Character c, int command)
		{
			GameTables t = Ff4Party.Tables;
			List<int> list = new List<int>();
			int kind = t.AbilityListKind(command);
			if (kind == 4)
			{
				foreach (OpenFF.Data.ItemStack s in Ff4Party.Party.Inventory)
				{
					ItemDefinition item = t.Item(s.ItemId);
					if (item != null && item.Kind == ItemKind.Consumable && Ff4Battle.ItemUsable(s.ItemId) && s.ItemId != c.AutoCommand && !list.Contains(s.ItemId)) list.Add(s.ItemId);
				}
				return list;
			}
			OpenFF.Data.MagicSchool? school = kind switch { 1 => OpenFF.Data.MagicSchool.White, 2 => OpenFF.Data.MagicSchool.Black, 8 => OpenFF.Data.MagicSchool.Summon, 16 => OpenFF.Data.MagicSchool.Song, 32 => OpenFF.Data.MagicSchool.Ninjutsu, _ => (OpenFF.Data.MagicSchool?)null };
			if (school == null) return list;
			foreach (int id in c.Spells) if (t.Spell(id)?.School == school && id != c.AutoCommand && !list.Contains(id)) list.Add(id);
			foreach (int id in c.Abilities) if (id >= 1500 && t.Spell(id)?.School == school && id != c.AutoCommand && !list.Contains(id)) list.Add(id);
			return list;
		}

		/// <summary>An auto command's name: a command's, a spell's or an item's.</summary>
		private static string AutoName(int id)
		{
			GameTables t = Ff4Party.Tables;
			if (id <= 0 || t == null) return "";
			if (id < 256) return CommandName(id);
			if (t.Item(id) is ItemDefinition item) return item.Name;
			return t.Spell(id)?.Name ?? t.AbilityName(id)?.Trim() ?? id.ToString();
		}

		private static string AutoHelp(int id)
		{
			GameTables t = Ff4Party.Tables;
			if (id <= 0 || t == null) return "";
			if (id >= 256 && t.Item(id) is ItemDefinition item) return item.Caption ?? "";
			return (t.AbilityHelp(id) ?? "").Replace("\n", " ").Replace("", "");
		}

		private void OpenAutoList()
		{
			_autoPicking = true;
			_autoSubOf = 0;
			_autoList.Clear();
			_autoList.AddRange(AutoCommands(Member));
			_autoAt = _autoTop = 0;
		}

		private void UpdateAutoList(InputState input)
		{
			int cols = _autoSubOf != 0 ? 3 : 2, n = _autoList.Count;
			if (input.Pressed(Pad.B))
			{
				if (_autoSubOf != 0)
				{
					// Back to the commands, the hand on the list command.
					int of = _autoSubOf;
					OpenAutoList();
					_autoAt = Math.Max(0, _autoList.IndexOf(of));
					KeepAutoInView(2);
				}
				else _autoPicking = false;
				return;
			}
			if (n == 0) return;
			if (input.Pressed(Pad.Down)) _autoAt = _autoAt + cols < n ? _autoAt + cols : _autoAt % cols;
			if (input.Pressed(Pad.Up))
			{
				if (_autoAt - cols >= 0) _autoAt -= cols;
				else { int last = _autoAt; for (int i = _autoAt; i < n; i += cols) last = i; _autoAt = last; }
			}
			if (input.Pressed(Pad.Right) && _autoAt + 1 < n) _autoAt++;
			if (input.Pressed(Pad.Left) && _autoAt > 0) _autoAt--;
			KeepAutoInView(cols);
			if (!input.Pressed(Pad.A)) return;
			int id = _autoList[_autoAt];
			if (_autoSubOf == 0 && Ff4Party.Tables.AbilityListKind(id) != 0)
			{
				_autoSubOf = id;
				_autoList.Clear();
				_autoList.AddRange(AutoSubList(Member, id));
				_autoAt = _autoTop = 0;
				return;
			}
			Member.AutoCommand = id;
			Log.Write(LogChannel.File, "menu: " + Member.Name + "'s auto-battle command is " + AutoName(id) + " (" + id + ")");
			_autoPicking = false;
			_autoSubOf = 0;
		}

		private void KeepAutoInView(int cols)
		{
			int row = _autoAt / cols;
			if (row < _autoTop) _autoTop = row;
			if (row >= _autoTop + AutoRows) _autoTop = row - AutoRows + 1;
		}

		/// <summary>The list in the layout: two columns of commands or three of a command's spells or items, four rows in view.</summary>
		private void FillAutoList(Ff4MenuHud.Data h)
		{
			h.AutoPicking = _autoPicking;
			h.AutoPair = _autoPicking && _autoSubOf == 0;
			h.AutoTriple = _autoPicking && _autoSubOf != 0;
			if (!_autoPicking) return;
			int cols = _autoSubOf != 0 ? 3 : 2;
			for (int k = 0; k < h.AutoPick.Count; k++)
			{
				int i = _autoTop * cols + k;
				Ff4MenuHud.CellRow row = h.AutoPick[k];
				row.Present = k < cols * AutoRows && i < _autoList.Count;
				row.Name = row.Present ? AutoName(_autoList[i]) : "";
				row.Lit = row.Present && i == _autoAt;
				row.Dim = false;
				row.Icon = -1;
				row.Count = "";
			}
			Ff4MenuHud.Scroll(h.AutoScroll, (_autoList.Count + cols - 1) / cols, AutoRows, _autoTop, 526.5f, true);
			h.Help = _autoAt < _autoList.Count ? AutoHelp(_autoList[_autoAt]) : "";
		}
	}
}
