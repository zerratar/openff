// The OpenFF menu on FF4: a status screen drawn by the engine from the unified party.
//
// FF3's menu part reads FF3's party, jobs and item tables and cannot show FF4's; FF4's own
// menu (the MenuLayout_*.xbn layouts) is not ported. Until it is, the pad's menu button on
// an FF4 map opens this: the line-up with levels, hit and magic points, attributes with
// equipment, what each wears, and the bag - all from OpenFF.Data through Game.Party and
// Game.Items, the way a mod would draw it - and the Save and Load pages, three slots each
// through Ff4Saves. Up/Down pick a member or item, Left/Right switch pages, A on a member
// opens their equipment (pick a slot, then something from the bag that fits: the item's
// position bits name the slot - 1 right hand, 2 left hand, 4 head, 8 body, 16 arms - and its
// canEquip mask the character types), A on a usable item picks whom to use it on, A saves or
// loads a slot; B backs out, then closes. Input is captured while it is open.

using System;
using System.Collections.Generic;
using OpenFF;
using OpenFF.Data;

namespace FF3
{
	internal sealed class Ff4Menu : GameService
	{
		private enum Mode { Browse, EquipSlot, EquipItem, ItemTarget }

		private bool _open;
		private int _page;      // 0 party, 1 bag, 2 save, 3 load
		private const int Pages = 4;
		private int _cursor;
		private int _scroll;
		private Mode _mode;
		private int _slot;                 // the equipment slot under the cursor
		private int _pick;                 // the cursor in the equip or target list
		private readonly List<int> _equipChoices = new List<int>();
		private int _usingItem;
		private static readonly string[] SlotNames = { "Right hand", "Left hand", "Head", "Body", "Arms" };
		private static readonly int[] SlotBits = { 1, 2, 4, 8, 16 };

		public override bool WantsUpdate => true;

		public bool IsOpen => _open;

		public override void OnUpdate()
		{
			InputState input = Game.Input;
			if (!_open)
			{
				if (EngineApi.InWorld && !Ff4Cutscene.Active && !Game.Dialogue.IsOpen && !Game.Battle.InBattle && !Ff4Battle.Active
					&& !(Ff4Shop.Instance?.IsOpen ?? false) && (input.Pressed(Pad.X) || input.KeyPressed("M")))
				{
					_open = true;
					_page = 0;
					_cursor = 0;
					_scroll = 0;
					_mode = Mode.Browse;
					input.Capture = true;
					Log.Write(LogChannel.File, "menu: open - " + Ff4Party.Party.Describe().Replace("\n", " | "));
				}
				return;
			}
			if (Ff4Saves.NoticeFrames > 0) Ff4Saves.NoticeFrames--;
			if (_mode != Mode.Browse)
			{
				UpdateMode(input);
				Draw();
				return;
			}
			if (input.Pressed(Pad.B) || input.Pressed(Pad.X) || input.KeyPressed("M") || input.KeyPressed("Escape"))
			{
				Log.Write(LogChannel.File, "menu: close - pad " + (int)input.Held + " keys " + string.Join("+", input.KeysHeld));
				_open = false;
				input.Capture = false;
				return;
			}
			if (input.Pressed(Pad.Left)) { _page = Math.Max(0, _page - 1); _cursor = 0; _scroll = 0; Log.Write(LogChannel.File, "menu: page " + _page + " (left) pad " + (int)input.Held); }
			if (input.Pressed(Pad.Right)) { _page = Math.Min(Pages - 1, _page + 1); _cursor = 0; _scroll = 0; Log.Write(LogChannel.File, "menu: page " + _page + " (right) pad " + (int)input.Held); }
			int count = _page == 0 ? Game.Party.Members.Count : _page == 1 ? Game.Party.Items.Count : Ff4Saves.SlotCount;
			if (input.Pressed(Pad.Up)) _cursor = Math.Max(0, _cursor - 1);
			if (input.Pressed(Pad.Down)) _cursor = Math.Min(Math.Max(0, count - 1), _cursor + 1);
			if (input.Pressed(Pad.A))
			{
				if (_page == 0 && count > 0)
				{
					_mode = Mode.EquipSlot;
					_slot = 0;
				}
				else if (_page == 1 && count > 0)
				{
					int id = Game.Party.Items[_cursor].ItemId;
					if (UsableEffect(id) != null) { _usingItem = id; _mode = Mode.ItemTarget; _pick = 0; }
					else Notice("That cannot be used here.");
				}
				else if (_page >= 2)
				{
					int slot = _cursor + 1;
					if (_page == 2)
					{
						Notice(Ff4Saves.Save(slot) ? "Saved to slot " + slot + "." : "Could not save here.");
					}
					else if (Ff4Saves.Exists(slot))
					{
						_open = false;
						input.Capture = false;
						if (!Ff4Saves.Load(slot, false)) Game.Dialogue.Say("Slot " + slot + " could not be loaded.");
						return;
					}
					else Notice("Slot " + slot + " is empty.");
				}
			}
			Draw();
		}

		private static void Notice(string text)
		{
			Ff4Saves.Notice = text;
			Ff4Saves.NoticeFrames = 150;
		}

		// ---- equipment and item use ----

		private Character Picked => _cursor < Ff4Party.Party.Members.Count ? Ff4Party.Party.Members[_cursor] : null;

		private void UpdateMode(InputState input)
		{
			Character member = Picked;
			if (member == null) { _mode = Mode.Browse; return; }
			switch (_mode)
			{
				case Mode.EquipSlot:
					if (input.Pressed(Pad.Up)) _slot = (_slot + 4) % 5;
					if (input.Pressed(Pad.Down)) _slot = (_slot + 1) % 5;
					if (input.Pressed(Pad.B)) { _mode = Mode.Browse; return; }
					if (input.Pressed(Pad.A))
					{
						_equipChoices.Clear();
						if (member.Equipment[_slot] != 0) _equipChoices.Add(0);
						foreach (OpenFF.Data.ItemStack s in Ff4Party.Party.Inventory)
						{
							if (Fits(Ff4Party.Tables?.Item(s.ItemId), member, _slot)) _equipChoices.Add(s.ItemId);
						}
						if (_equipChoices.Count == 0) { Notice("Nothing in the bag fits there."); return; }
						_mode = Mode.EquipItem;
						_pick = 0;
					}
					break;
				case Mode.EquipItem:
					if (input.Pressed(Pad.Up)) _pick = (_pick + _equipChoices.Count - 1) % _equipChoices.Count;
					if (input.Pressed(Pad.Down)) _pick = (_pick + 1) % _equipChoices.Count;
					if (input.Pressed(Pad.B)) { _mode = Mode.EquipSlot; return; }
					if (input.Pressed(Pad.A))
					{
						int id = _equipChoices[_pick];
						if (id != 0) Ff4Party.Party.RemoveItem(id, 1);
						Ff4Party.Party.Equip(member.Id, (OpenFF.Data.EquipSlot)_slot, id);
						Log.Write(LogChannel.File, "menu: " + member.Name + " " + (id == 0 ? "takes off the " + SlotNames[_slot].ToLower() : "equips " + (Ff4Party.Tables?.Item(id)?.Name ?? id.ToString()) + " (" + SlotNames[_slot].ToLower() + ")"));
						_mode = Mode.EquipSlot;
					}
					break;
				case Mode.ItemTarget:
					int members = Ff4Party.Party.Members.Count;
					if (input.Pressed(Pad.Up)) _pick = (_pick + members - 1) % members;
					if (input.Pressed(Pad.Down)) _pick = (_pick + 1) % members;
					if (input.Pressed(Pad.B)) { _mode = Mode.Browse; return; }
					if (input.Pressed(Pad.A))
					{
						Character target = Ff4Party.Party.Members[_pick];
						string said = Use(_usingItem, target);
						Notice(said);
						if (Ff4Party.Party.CountItem(_usingItem) == 0 || Game.Party.Items.Count == 0) { _mode = Mode.Browse; _cursor = Math.Min(_cursor, Math.Max(0, Game.Party.Items.Count - 1)); }
					}
					break;
			}
		}

		/// <summary>Whether an item may go into a member's slot: worn, its position bits name the slot, and its mask names the character type.</summary>
		private static bool Fits(ItemDefinition item, Character member, int slot)
		{
			if (item?.Equip == null) return false;
			if (item.Kind == ItemKind.Weapon && slot > 1) return false;
			if (item.Kind == ItemKind.Armour && slot <= 1 && (item.Equip.Position & 0xFFFF & 3) == 0) return false;
			int bits = item.Equip.Position & 0xFFFF;
			if ((bits & SlotBits[slot]) == 0) return false;
			return item.Equip.CanEquip == 0 || (item.Equip.CanEquip & (1u << member.Id)) != 0;
		}

		/// <summary>What a consumable does from the menu: hit or magic points back, or a revival; null for anything else.</summary>
		private static Efficacy UsableEffect(int itemId)
		{
			ItemDefinition item = Ff4Party.Tables?.Item(itemId);
			if (item == null || item.Kind != ItemKind.Consumable || item.EfficacyId <= 0) return null;
			Efficacy e = Ff4Party.Tables.Efficacy(item.EfficacyId);
			if (e == null || e.CastsAbility > 0) return null;
			return e.Hp > 0 || e.Mp > 0 || e.Id == 17 ? e : null;
		}

		private static string Use(int itemId, Character target)
		{
			ItemDefinition item = Ff4Party.Tables?.Item(itemId);
			Efficacy e = UsableEffect(itemId);
			if (item == null || e == null) return "Nothing happens.";
			bool revive = e.Id == 17;
			if (revive != !target.Alive) return item.Name + " does nothing for " + target.Name + ".";
			if (!Ff4Party.Party.RemoveItem(itemId, 1)) return "None left.";
			int hp = target.Hp, mp = target.Mp;
			if (revive) target.Hp = Math.Max(1, target.MaxHp / 4);
			else
			{
				if (e.Hp > 0) target.Hp = Math.Min(target.MaxHp, target.Hp + e.Hp);
				if (e.Mp > 0) target.Mp = Math.Min(target.MaxMp, target.Mp + e.Mp);
			}
			string said = item.Name + ": " + target.Name + (revive ? " rises." : (target.Hp != hp ? " +" + (target.Hp - hp) + " HP" : "") + (target.Mp != mp ? " +" + (target.Mp - mp) + " MP" : "") + ".");
			Log.Write(LogChannel.File, "menu: " + said);
			return said;
		}

		// ---- drawing ----

		private void Draw()
		{
			DrawList d = Game.Draw;
			Color panel = new Color(16, 24, 72, 235);
			Color frame = new Color(230, 230, 240);
			Color dim = new Color(170, 175, 200);
			d.Rect(0, 0, 800, 480, new Color(0, 0, 0, 110));
			d.Rect(40, 30, 720, 420, panel);
			d.Rect(40, 30, 720, 420, frame, false);
			string[] titles = { "Party", "Items", "Save", "Load" };
			d.Text(titles[_page], 60, 42, Color.White, 20);
			d.Text(Game.Party.Gil + " gil", 700 - d.MeasureText(Game.Party.Gil + " gil", 14), 46, Color.Yellow, 14);
			string help = _mode == Mode.EquipSlot ? "Up/Down: slot   A: change   B: back"
				: _mode == Mode.EquipItem ? "Up/Down: choose   A: equip   B: back"
				: _mode == Mode.ItemTarget ? "Up/Down: whom   A: use   B: back"
				: "Left/Right: page   Up/Down: choose   " + (_page == 0 ? "A: equipment   " : _page == 1 ? "A: use   " : "A: " + titles[_page].ToLower() + "   ") + "B: close";
			d.Text(help, 60, 425, dim, 12);
			if (_page == 0) DrawParty(d, dim);
			else if (_page == 1) DrawBag(d, dim);
			else DrawSlots(d, dim, _page == 3);
			if (Ff4Saves.NoticeFrames > 0 && !string.IsNullOrEmpty(Ff4Saves.Notice)) d.Text(Ff4Saves.Notice, 60, 400, Color.Yellow, 14);
		}

		private void DrawParty(DrawList d, Color dim)
		{
			IReadOnlyList<PartyMember> members = Game.Party.Members;
			if (members.Count == 0)
			{
				d.Text("Nobody is in the party.", 60, 90, dim, 14);
				return;
			}
			_cursor = Math.Min(_cursor, members.Count - 1);
			float y = 80;
			for (int i = 0; i < members.Count; i++)
			{
				PartyMember m = members[i];
				bool on = i == _cursor;
				if (on) d.Rect(52, y - 4, 330, 44, new Color(255, 255, 255, 28));
				d.Text((on ? "> " : "  ") + m.Name, 60, y, on ? Color.Yellow : Color.White, 16);
				d.Text("L" + m.Level, 240, y + 2, Color.White, 14);
				d.Text("HP " + m.Hp + "/" + m.MaxHp, 70, y + 20, m.Hp * 4 <= m.MaxHp ? Color.Red : dim, 12);
				d.Text("MP " + m.Charges[0] + "/" + m.MaxCharges[0], 220, y + 20, dim, 12);
				y += 50;
			}
			PartyMember pick = members[_cursor];
			float x = 410, ty = 80;
			string cls = Ff4Party.Tables?.Character(pick.Id)?.ClassName;
			d.Text(pick.Name + (cls != null ? "  -  " + cls : ""), x, ty, Color.White, 16); ty += 26;
			d.Text("Level " + pick.Level + "     Exp " + pick.Experience, x, ty, dim, 12); ty += 20;
			int next = NextLevelExp(pick);
			if (next > 0) { d.Text("Next level in " + next, x, ty, dim, 12); }
			ty += 26;
			d.Text("Strength  " + pick.Stats.Strength, x, ty, Color.White, 13); d.Text("Agility  " + pick.Stats.Agility, x + 170, ty, Color.White, 13); ty += 18;
			d.Text("Vitality  " + pick.Stats.Vitality, x, ty, Color.White, 13); d.Text("Wisdom   " + pick.Stats.Intellect, x + 170, ty, Color.White, 13); ty += 18;
			d.Text("Will      " + pick.Stats.Mind, x, ty, Color.White, 13); ty += 30;
			d.Text("Equipment", x, ty, Color.Yellow, 14); ty += 22;
			for (int s = 0; s < 5; s++)
			{
				int id = Game.Party.Equipped(pick.Id, (OpenFF.EquipSlot)s);
				Item item = id != 0 ? Game.Items.Find(id) : null;
				bool on = _mode != Mode.Browse && s == _slot;
				if (on) d.Rect(x - 6, ty - 2, 330, 18, new Color(255, 255, 255, 28));
				d.Text((on ? "> " : "") + SlotNames[s], x, ty, on ? Color.Yellow : dim, 12);
				d.Text(item?.Name ?? (id != 0 ? "item " + id : "-"), x + 110, ty, Color.White, 12);
				ty += 18;
			}
			if (_mode == Mode.EquipItem)
			{
				// The bag's fitting items, over the attributes.
				float bx = 400, by = 78, bw = 350, bh = 24 + 20 * Math.Min(8, _equipChoices.Count) + 8;
				d.Rect(bx, by, bw, bh, new Color(24, 32, 90, 245));
				d.Rect(bx, by, bw, bh, Color.White, false);
				d.Text(SlotNames[_slot] + ":", bx + 10, by + 6, Color.Yellow, 13);
				int first = Math.Max(0, Math.Min(_pick - 7, _equipChoices.Count - 8));
				for (int i = first; i < _equipChoices.Count && i < first + 8; i++)
				{
					int id = _equipChoices[i];
					ItemDefinition item = id != 0 ? Ff4Party.Tables?.Item(id) : null;
					string text = id == 0 ? "(take off)" : (item?.Name ?? ("item " + id)) + (item?.Equip != null ? (item.Kind == ItemKind.Weapon ? "   atk " + item.Equip.Attack : "   def " + item.Equip.Defence + " mdef " + item.Equip.MagicDefence) : "");
					d.Text((i == _pick ? "> " : "  ") + text, bx + 10, by + 26 + 20 * (i - first), i == _pick ? Color.Yellow : Color.White, 12);
				}
				return;
			}
			if (pick.Spells.Count > 0)
			{
				ty += 10;
				d.Text("Magic", x, ty, Color.Yellow, 14); ty += 20;
				int col = 0;
				foreach (int id in pick.Spells)
				{
					OpenFF.Data.SpellDefinition spell = Ff4Party.Tables?.Spell(id);
					string name = spell?.Name ?? Ff4Party.Tables?.AbilityName(id) ?? id.ToString();
					d.Text(name, x + 115 * (col % 3), ty, Color.White, 12);
					if (spell != null) d.Text(spell.MpCost.ToString(), x + 115 * (col % 3) + 88, ty, dim, 11);
					col++;
					if (col % 3 == 0) ty += 16;
					if (ty > 400) break;
				}
			}
		}

		private int NextLevelExp(PartyMember m)
		{
			int[] curve = Ff4Party.Tables?.ExperienceToLevel;
			if (curve == null || m.Level >= curve.Length) return 0;
			return Math.Max(0, curve[m.Level] - m.Experience);
		}

		private void DrawBag(DrawList d, Color dim)
		{
			IReadOnlyList<OpenFF.ItemStack> items = Game.Party.Items;
			if (items.Count == 0)
			{
				d.Text("The bag is empty.", 60, 90, dim, 14);
				return;
			}
			const int rows = 16;
			_cursor = Math.Min(_cursor, items.Count - 1);
			if (_cursor < _scroll) _scroll = _cursor;
			if (_cursor >= _scroll + rows) _scroll = _cursor - rows + 1;
			float y = 80;
			for (int i = _scroll; i < Math.Min(items.Count, _scroll + rows); i++)
			{
				OpenFF.ItemStack stack = items[i];
				Item item = Game.Items.Find(stack.ItemId);
				bool on = i == _cursor;
				if (on) d.Rect(52, y - 2, 340, 20, new Color(255, 255, 255, 28));
				d.Text((on ? "> " : "  ") + (item?.Name ?? ("item " + stack.ItemId)), 60, y, on ? Color.Yellow : Color.White, 13);
				d.Text("x" + stack.Count, 350, y, dim, 13);
				y += 21;
			}
			Item picked = Game.Items.Find(items[_cursor].ItemId);
			if (picked != null)
			{
				float x = 420, ty = 80;
				d.Text(picked.Name ?? "", x, ty, Color.White, 16); ty += 26;
				d.Text(picked.Category + (picked.Price > 0 ? "   " + picked.Price + " gil" : ""), x, ty, dim, 12); ty += 20;
				if (!string.IsNullOrEmpty(picked.Caption)) { d.Text(picked.Caption, x, ty, Color.White, 12); ty += 20; }
				if (picked.Category == ItemCategory.Weapon) d.Text("Attack " + picked.Attack + "   Hit " + picked.Accuracy, x, ty, dim, 12);
				if (picked.Category == ItemCategory.Armor) d.Text("Defence " + picked.Defense + "   Magic defence " + picked.MagicDefense + "   Evade " + picked.Evasion, x, ty, dim, 12);
				ty += 30;
				if (_mode == Mode.ItemTarget)
				{
					d.Text("Use on whom?", x, ty, Color.Yellow, 14); ty += 22;
					IReadOnlyList<Character> members = Ff4Party.Party.Members;
					for (int i = 0; i < members.Count; i++)
					{
						Character c = members[i];
						bool on = i == _pick;
						d.Text((on ? "> " : "  ") + c.Name + "   " + c.Hp + "/" + c.MaxHp + " HP   " + c.Mp + "/" + c.MaxMp + " MP", x, ty, on ? Color.Yellow : (c.Alive ? Color.White : dim), 13);
						ty += 20;
					}
				}
			}
		}

		private void DrawSlots(DrawList d, Color dim, bool loading)
		{
			d.Text(loading ? "Pick a slot to load. The party, the bag, the flags and the spot come back." : "Pick a slot to save the game as it stands.", 60, 80, dim, 13);
			float y = 120;
			for (int i = 0; i < Ff4Saves.SlotCount; i++)
			{
				bool on = i == _cursor;
				if (on) d.Rect(52, y - 6, 700, 40, new Color(255, 255, 255, 28));
				d.Text((on ? "> " : "  ") + "Slot " + (i + 1), 60, y, on ? Color.Yellow : Color.White, 16);
				d.Text(Ff4Saves.Describe(i + 1), 170, y + 2, Ff4Saves.Exists(i + 1) ? Color.White : dim, 13);
				y += 50;
			}
		}

		public override IEnumerable<string> DebugLines()
		{
			if (_open) yield return "OpenFF menu open (" + new[] { "party", "items", "save", "load" }[_page] + (_mode != Mode.Browse ? ", " + _mode : "") + ")";
		}
	}
}
