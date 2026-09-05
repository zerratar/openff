// The OpenFF menu on FF4: a status screen drawn by the engine from the unified party.
//
// FF3's menu part reads FF3's party, jobs and item tables and cannot show FF4's; FF4's own
// menu (the MenuLayout_*.xbn layouts) is not ported. Until it is, the pad's menu button on
// an FF4 map opens this: the line-up with levels, hit and magic points, attributes with
// equipment, what each wears, and the bag - all from OpenFF.Data through Game.Party and
// Game.Items, the way a mod would draw it. Up/Down pick a member, Left/Right switch pages,
// B or the menu button closes. Input is captured while it is open.

using System;
using System.Collections.Generic;
using OpenFF;

namespace FF3
{
	internal sealed class Ff4Menu : GameService
	{
		private bool _open;
		private int _page;      // 0 party, 1 bag
		private int _cursor;
		private int _scroll;

		public override bool WantsUpdate => true;

		public bool IsOpen => _open;

		public override void OnUpdate()
		{
			InputState input = Game.Input;
			if (!_open)
			{
				if (EngineApi.InWorld && !Ff4Cutscene.Active && !Game.Dialogue.IsOpen && !Game.Battle.InBattle && !Ff4Battle.Active
					&& (input.Pressed(Pad.X) || input.KeyPressed("M")))
				{
					_open = true;
					_page = 0;
					_cursor = 0;
					_scroll = 0;
					input.Capture = true;
					Log.Write(LogChannel.File, "menu: open - " + Ff4Party.Party.Describe().Replace("\n", " | "));
				}
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
			if (input.Pressed(Pad.Right)) { _page = Math.Min(1, _page + 1); _cursor = 0; _scroll = 0; Log.Write(LogChannel.File, "menu: page " + _page + " (right) pad " + (int)input.Held); }
			int count = _page == 0 ? Game.Party.Members.Count : Game.Party.Items.Count;
			if (input.Pressed(Pad.Up)) _cursor = Math.Max(0, _cursor - 1);
			if (input.Pressed(Pad.Down)) _cursor = Math.Min(Math.Max(0, count - 1), _cursor + 1);
			Draw();
		}

		private void Draw()
		{
			DrawList d = Game.Draw;
			Color panel = new Color(16, 24, 72, 235);
			Color frame = new Color(230, 230, 240);
			Color dim = new Color(170, 175, 200);
			d.Rect(0, 0, 800, 480, new Color(0, 0, 0, 110));
			d.Rect(40, 30, 720, 420, panel);
			d.Rect(40, 30, 720, 420, frame, false);
			string title = _page == 0 ? "Party" : "Items";
			d.Text(title, 60, 42, Color.White, 20);
			d.Text(Game.Party.Gil + " gil", 700 - d.MeasureText(Game.Party.Gil + " gil", 14), 46, Color.Yellow, 14);
			d.Text("Left/Right: Party - Items     Up/Down: choose     B: close", 60, 425, dim, 12);
			if (_page == 0) DrawParty(d, dim);
			else DrawBag(d, dim);
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
			string[] slots = { "Right hand", "Left hand", "Head", "Body", "Arms" };
			for (int s = 0; s < 5; s++)
			{
				int id = Game.Party.Equipped(pick.Id, (OpenFF.EquipSlot)s);
				Item item = id != 0 ? Game.Items.Find(id) : null;
				d.Text(slots[s], x, ty, dim, 12);
				d.Text(item?.Name ?? (id != 0 ? "item " + id : "-"), x + 110, ty, Color.White, 12);
				ty += 18;
			}
			if (pick.Spells.Count > 0)
			{
				ty += 10;
				List<string> names = new List<string>();
				foreach (int id in pick.Spells) names.Add(Ff4Party.Tables?.AbilityName(id) ?? id.ToString());
				d.Text("Abilities: " + string.Join(", ", names), x, ty, dim, 12);
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
			}
		}

		public override IEnumerable<string> DebugLines()
		{
			if (_open) yield return "OpenFF menu open (" + (_page == 0 ? "party" : "items") + ")";
		}
	}
}
