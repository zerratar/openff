// The OpenFF menu on FF4, drawn as the Steam game draws its own: FF4's window art (Ff4Ui -
// Steam's window.png and point.png, or the phone's frames), its texts (babil_menu.msd through
// Ff4Layouts), the command list in the order MenuLayout_Root gives it, the Status screen's
// rows where MenuLayout_Status puts them - over the unified party (OpenFF.Data through
// Ff4Party), the way a mod would draw it.
//
// FF4's own menu is world::WSMenu with a sub-state per screen (Docs/FF4-Internals.md); the
// layouts are DS-unit rectangles and the phone stretches them over its 16:9 screen in code
// not read yet, so the windows here stand where Karl's Steam screenshots show them: the
// party's rows on the left and the commands on the right (Root), a title bar, a main window
// and a footer of key hints (every other screen). The Status screen keeps the layout's
// geometry: a DS unit is two of our pixels down the main window.
//
// Keys: the pad's menu button (keyboard C or M) opens and closes; Up/Down move the glove; A
// picks (a command that needs a member first sends the glove to the party's rows); B goes
// back; Left/Right on a member's screen switch member. Input is captured while it is open.
// Inventory: A on a usable item asks whom; Equipment: A on a slot lists what fits (position
// bits 1 right hand, 2 left hand, 4 head, 8 body, 16 arms and the character-type mask), the
// old piece returns to the bag; Party: A twice swaps two members; Save and Load: three slots
// through Ff4Saves.

using System;
using System.Collections.Generic;
using OpenFF;
using OpenFF.Data;

namespace OpenFF.Client
{
	internal sealed class Ff4Menu : GameService
	{
		private enum Screen { Root, Status, Inventory, Equipment, Magic, Abilities, Party, Save, Load }
		private enum Mode { Browse, PickMember, EquipSlot, EquipItem, ItemTarget, SwapMember }

		private sealed class Command
		{
			public uint Text;
			public Screen Screen;
			public bool NeedsMember;
			public bool Later;   // named but not built: a notice instead
		}

		private bool _open;
		private Screen _screen;
		private Mode _mode;
		private readonly List<Command> _commands = new List<Command>();
		private int _command, _commandScroll;
		private int _member;              // the member a screen is about
		private int _cursor, _scroll;     // the list cursor of the screen
		private int _slot, _pick, _usingItem, _swapFrom = -1;
		private readonly List<int> _equipChoices = new List<int>();
		private static readonly int[] SlotBits = { 1, 2, 4, 8, 16 };
		private static readonly uint[] SlotTexts = { 50204, 50205, 50206, 50207, 50208 };
		private static readonly string[] SlotFallback = { "Right", "Left", "Head", "Body", "Arms" };

		public override bool WantsUpdate => true;

		public bool IsOpen => _open;

		// ---- the Steam arrangement, in the port's 800 x 480 ----
		private const float PlaneX = 14f, PlaneW = 464f, PlaneRow = 94f, PlaneRowH = 90f;
		private const float ColX = 488f, ColW = 272f, ColRow = 68f, ColRowH = 64f, ColVisible = 6;
		private const float TitleX = 64f, TitleW = 674f, TitleH = 34f, MainY = 38f, MainH = 386f, FooterY = 428f, FooterH = 52f;

		private static readonly Color Dim = new Color(186, 190, 218);
		private static readonly Color Gold = new Color(255, 232, 110);
		private static readonly Color Low = new Color(255, 120, 110);
		private static readonly Color Line = new Color(170, 176, 230, 110);

		// ---- opening and the root ----

		private void Open()
		{
			_open = true;
			_screen = Screen.Root;
			_mode = Mode.Browse;
			_command = _commandScroll = 0;
			_member = 0;
			_swapFrom = -1;
			Game.Input.Capture = true;
			BuildCommands();
			Log.Write(LogChannel.File, "menu: open - " + Ff4Party.Party.Describe().Replace("\n", " | "));
		}

		private void Close()
		{
			_open = false;
			Game.Input.Capture = false;
			Log.Write(LogChannel.File, "menu: close");
		}

		/// <summary>The commands in MenuLayout_Root's order (its FBText frames: 50002 Inventory .. 50007 Save), Load added after them.</summary>
		private void BuildCommands()
		{
			_commands.Clear();
			OpenFF.Content.Layout root = Ff4Layouts.Get("Root");
			List<uint> order = new List<uint>();
			if (root != null)
			{
				foreach (OpenFF.Content.LayoutFrame f in root.Frames) if (f.MessageId >= 50002 && f.MessageId <= 50011) order.Add((uint)f.MessageId);
			}
			if (order.Count == 0) order.AddRange(new uint[] { 50002, 50003, 50004, 50011, 50005, 50010, 50006, 50009, 50007 });
			foreach (uint id in order)
			{
				switch (id)
				{
					case 50002: _commands.Add(new Command { Text = id, Screen = Screen.Inventory }); break;
					case 50003: _commands.Add(new Command { Text = id, Screen = Screen.Magic, NeedsMember = true }); break;
					case 50004: _commands.Add(new Command { Text = id, Screen = Screen.Equipment, NeedsMember = true }); break;
					case 50011: _commands.Add(new Command { Text = id, Screen = Screen.Abilities, NeedsMember = true }); break;
					case 50005: _commands.Add(new Command { Text = id, Screen = Screen.Status, NeedsMember = true }); break;
					case 50010: _commands.Add(new Command { Text = id, Screen = Screen.Party }); break;
					case 50007: _commands.Add(new Command { Text = id, Screen = Screen.Save }); break;
					default: _commands.Add(new Command { Text = id, Later = true }); break;   // Settings, Quicksave
				}
			}
			_commands.Add(new Command { Text = 50008, Screen = Screen.Load });
		}

		public override void OnUpdate()
		{
			InputState input = Game.Input;
			if (!_open)
			{
				if (EngineApi.InWorld && !Ff4Cutscene.Active && !Game.Dialogue.IsOpen && !Game.Battle.InBattle && !Ff4Battle.Active
					&& !(Ff4Shop.Instance?.IsOpen ?? false) && (input.Pressed(Pad.X) || input.KeyPressed("M")))
				{
					Open();
				}
				return;
			}
			if (Ff4Saves.NoticeFrames > 0) Ff4Saves.NoticeFrames--;
			if (input.Pressed(Pad.X) || input.KeyPressed("M") || input.KeyPressed("Escape")) { Close(); return; }
			if (Ff4Party.Party.Members.Count == 0) { Close(); return; }
			_member = Math.Clamp(_member, 0, Ff4Party.Party.Members.Count - 1);
			switch (_screen)
			{
				case Screen.Root: UpdateRoot(input); break;
				case Screen.Status: UpdateMemberScreen(input, null); break;
				case Screen.Inventory: UpdateInventory(input); break;
				case Screen.Equipment: UpdateEquipment(input); break;
				case Screen.Magic:
				case Screen.Abilities: UpdateMemberScreen(input, ListCount()); break;
				case Screen.Party: UpdateParty(input); break;
				case Screen.Save:
				case Screen.Load: UpdateSlots(input); break;
			}
			if (_open) Draw();
		}

		private void Back()
		{
			_screen = Screen.Root;
			_mode = Mode.Browse;
			_cursor = _scroll = 0;
			_swapFrom = -1;
		}

		private void UpdateRoot(InputState input)
		{
			IReadOnlyList<Character> members = Ff4Party.Party.Members;
			if (_mode == Mode.PickMember)
			{
				if (input.Pressed(Pad.Up)) _member = (_member + members.Count - 1) % members.Count;
				if (input.Pressed(Pad.Down)) _member = (_member + 1) % members.Count;
				if (input.Pressed(Pad.B)) { _mode = Mode.Browse; return; }
				if (input.Pressed(Pad.A)) OpenScreen(_commands[_command].Screen);
				return;
			}
			if (input.Pressed(Pad.B)) { Close(); return; }
			if (input.Pressed(Pad.Up)) _command = (_command + _commands.Count - 1) % _commands.Count;
			if (input.Pressed(Pad.Down)) _command = (_command + 1) % _commands.Count;
			if (_command < _commandScroll) _commandScroll = _command;
			if (_command >= _commandScroll + ColVisible) _commandScroll = _command - (int)ColVisible + 1;
			if (input.Pressed(Pad.A))
			{
				Command c = _commands[_command];
				if (c.Later) { Notice(Ff4Layouts.Text(c.Text) + " comes later."); return; }
				if (c.NeedsMember) { _mode = Mode.PickMember; return; }
				OpenScreen(c.Screen);
			}
		}

		private void OpenScreen(Screen screen)
		{
			_screen = screen;
			_mode = screen == Screen.Equipment ? Mode.EquipSlot : Mode.Browse;
			_cursor = _scroll = 0;
			_slot = 0;
			_pick = 0;
			Log.Write(LogChannel.File, "menu: " + screen + (NeedsMember(screen) ? " of " + Ff4Party.Party.Members[_member].Name : ""));
		}

		private static bool NeedsMember(Screen s) => s == Screen.Status || s == Screen.Equipment || s == Screen.Magic || s == Screen.Abilities;

		private Character Member => Ff4Party.Party.Members[_member];

		private void SwitchMember(InputState input)
		{
			int n = Ff4Party.Party.Members.Count;
			if (input.Pressed(Pad.Left)) { _member = (_member + n - 1) % n; _cursor = _scroll = 0; }
			if (input.Pressed(Pad.Right)) { _member = (_member + 1) % n; _cursor = _scroll = 0; }
		}

		private void UpdateMemberScreen(InputState input, int? listCount)
		{
			if (input.Pressed(Pad.B)) { Back(); return; }
			SwitchMember(input);
			if (_screen == Screen.Status && input.Pressed(Pad.A)) { OpenScreen(Screen.Abilities); return; }
			if (listCount.HasValue && listCount.Value > 0)
			{
				int cols = 3, rows = 9;
				if (input.Pressed(Pad.Left)) _cursor = Math.Max(0, _cursor - 1);
				if (input.Pressed(Pad.Right)) _cursor = Math.Min(listCount.Value - 1, _cursor + 1);
				if (input.Pressed(Pad.Up)) _cursor = Math.Max(0, _cursor - cols);
				if (input.Pressed(Pad.Down)) _cursor = Math.Min(listCount.Value - 1, _cursor + cols);
				int row = _cursor / cols, top = _scroll / cols;
				if (row < top) _scroll = row * cols;
				if (row >= top + rows) _scroll = (row - rows + 1) * cols;
			}
		}

		private int ListCount() => _screen == Screen.Magic ? Member.Spells.Count : Member.Abilities.Count;

		// ---- inventory ----

		private void UpdateInventory(InputState input)
		{
			IReadOnlyList<OpenFF.Data.ItemStack> items = Ff4Party.Party.Inventory;
			if (_mode == Mode.ItemTarget)
			{
				int n = Ff4Party.Party.Members.Count;
				if (input.Pressed(Pad.Up)) _pick = (_pick + n - 1) % n;
				if (input.Pressed(Pad.Down)) _pick = (_pick + 1) % n;
				if (input.Pressed(Pad.B)) { _mode = Mode.Browse; return; }
				if (input.Pressed(Pad.A))
				{
					Notice(Use(_usingItem, Ff4Party.Party.Members[_pick]));
					if (Ff4Party.Party.CountItem(_usingItem) == 0) _mode = Mode.Browse;
				}
				return;
			}
			if (input.Pressed(Pad.B)) { Back(); return; }
			if (items.Count == 0) return;
			const int cols = 2, rows = 11;
			if (input.Pressed(Pad.Left)) _cursor = Math.Max(0, _cursor - 1);
			if (input.Pressed(Pad.Right)) _cursor = Math.Min(items.Count - 1, _cursor + 1);
			if (input.Pressed(Pad.Up)) _cursor = Math.Max(0, _cursor - cols);
			if (input.Pressed(Pad.Down)) _cursor = Math.Min(items.Count - 1, _cursor + cols);
			_cursor = Math.Min(_cursor, items.Count - 1);
			int row = _cursor / cols, top = _scroll / cols;
			if (row < top) _scroll = row * cols;
			if (row >= top + rows) _scroll = (row - rows + 1) * cols;
			if (input.Pressed(Pad.A))
			{
				int id = items[_cursor].ItemId;
				if (UsableEffect(id) != null) { _usingItem = id; _mode = Mode.ItemTarget; _pick = 0; }
				else Notice("That cannot be used here.");
			}
		}

		// ---- equipment ----

		private void UpdateEquipment(InputState input)
		{
			Character member = Member;
			if (_mode == Mode.EquipItem)
			{
				if (input.Pressed(Pad.Up)) _pick = (_pick + _equipChoices.Count - 1) % _equipChoices.Count;
				if (input.Pressed(Pad.Down)) _pick = (_pick + 1) % _equipChoices.Count;
				if (input.Pressed(Pad.B)) { _mode = Mode.EquipSlot; return; }
				if (input.Pressed(Pad.A))
				{
					int id = _equipChoices[_pick];
					if (id != 0) Ff4Party.Party.RemoveItem(id, 1);
					Ff4Party.Party.Equip(member.Id, (OpenFF.Data.EquipSlot)_slot, id);
					Log.Write(LogChannel.File, "menu: " + member.Name + " " + (id == 0 ? "takes off the " + SlotName(_slot).ToLower() : "equips " + (Ff4Party.Tables?.Item(id)?.Name ?? id.ToString()) + " (" + SlotName(_slot).ToLower() + ")"));
					_mode = Mode.EquipSlot;
				}
				return;
			}
			if (input.Pressed(Pad.B)) { Back(); return; }
			SwitchMember(input);
			if (input.Pressed(Pad.Up)) _slot = (_slot + 4) % 5;
			if (input.Pressed(Pad.Down)) _slot = (_slot + 1) % 5;
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

		private static string SlotName(int slot) => Ff4Layouts.Text(SlotTexts[slot], SlotFallback[slot]);

		// ---- party order ----

		private void UpdateParty(InputState input)
		{
			int n = Ff4Party.Party.Members.Count;
			if (input.Pressed(Pad.B)) { if (_swapFrom >= 0) _swapFrom = -1; else Back(); return; }
			if (input.Pressed(Pad.Up)) _cursor = (_cursor + n - 1) % n;
			if (input.Pressed(Pad.Down)) _cursor = (_cursor + 1) % n;
			if (input.Pressed(Pad.A))
			{
				if (_swapFrom < 0) { _swapFrom = _cursor; return; }
				if (_swapFrom != _cursor)
				{
					List<Character> members = Ff4Party.Party.Members;
					Character a = members[_swapFrom];
					members[_swapFrom] = members[_cursor];
					members[_cursor] = a;
					Log.Write(LogChannel.File, "menu: party order " + string.Join(", ", members.ConvertAll(m => m.Name)) + (_swapFrom == 0 || _cursor == 0 ? " (the leader changes at the next map)" : ""));
				}
				_swapFrom = -1;
			}
		}

		// ---- save and load ----

		private void UpdateSlots(InputState input)
		{
			if (input.Pressed(Pad.B)) { Back(); return; }
			if (input.Pressed(Pad.Up)) _cursor = Math.Max(0, _cursor - 1);
			if (input.Pressed(Pad.Down)) _cursor = Math.Min(Ff4Saves.SlotCount - 1, _cursor + 1);
			if (input.Pressed(Pad.A))
			{
				int slot = _cursor + 1;
				if (_screen == Screen.Save) Notice(Ff4Saves.Save(slot) ? "Saved to slot " + slot + "." : "Could not save here.");
				else if (Ff4Saves.Exists(slot))
				{
					Close();
					if (!Ff4Saves.Load(slot, false)) Game.Dialogue.Say("Slot " + slot + " could not be loaded.");
				}
				else Notice("Slot " + slot + " is empty.");
			}
		}

		// ---- items in the menu ----

		private static void Notice(string text)
		{
			Ff4Saves.Notice = text;
			Ff4Saves.NoticeFrames = 150;
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

		private static string T(uint id, string fallback = null) => Ff4Layouts.Text(id, fallback);

		private void Window(DrawList d, float x, float y, float w, float h)
		{
			if (Ff4Ui.Window(d, x, y, w, h)) return;
			d.Rect(x, y, w, h, new Color(20, 34, 74, 220));
			d.Rect(x, y, w, h, new Color(214, 218, 242), false);
		}

		private void Glove(DrawList d, float x, float y)
		{
			if (Ff4Ui.Glove(d, x, y)) return;
			for (int i = 0; i < 6; i++) d.Rect(x - 14 + 2 * i, y - 6 + i, 2, 12 - 2 * i, Color.White);
		}

		private void Text(DrawList d, string text, float x, float y, Color color, int size = 16)
		{
			if (string.IsNullOrEmpty(text)) return;
			d.Text(text, x + 1, y + 1, new Color(0, 0, 0, 160), size);
			d.Text(text, x, y, color, size);
		}

		private void Right(DrawList d, string text, float right, float y, Color color, int size = 16) => Text(d, text, right - d.MeasureText(text, size), y, color, size);

		private void Centred(DrawList d, string text, float centre, float y, Color color, int size = 16) => Text(d, text, centre - d.MeasureText(text, size) / 2, y, color, size);

		private void KeyHint(DrawList d, string key, string what, float x, float y)
		{
			d.Rect(x, y, 18, 18, new Color(30, 90, 150, 230));
			d.Rect(x, y, 18, 18, new Color(214, 218, 242), false);
			d.Text(key, x + 9 - d.MeasureText(key, 12) / 2, y + 2, Color.White, 12);
			Text(d, what, x + 24, y + 1, Color.White, 14);
		}

		/// <summary>A member's portrait from face.NCER (cell = player type), <paramref name="size"/> pixels square.</summary>
		private void Portrait(DrawList d, Character c, float x, float y, float size)
		{
			if (!Ff4Ui.Cell(d, "face.NCER", "face.NCGR", c.Id, x, y, size / 80f))
			{
				d.Rect(x, y, size, size, new Color(60, 64, 120, 255));
				d.Rect(x, y, size, size, new Color(214, 218, 242), false);
			}
		}

		private void Draw()
		{
			DrawList d = Game.Draw;
			d.Rect(0, 0, 800, 480, new Color(0, 0, 0, 90));
			switch (_screen)
			{
				case Screen.Root: DrawRoot(d); break;
				case Screen.Status: DrawStatus(d); break;
				case Screen.Inventory: DrawInventory(d); break;
				case Screen.Equipment: DrawEquipment(d); break;
				case Screen.Magic:
				case Screen.Abilities: DrawList(d); break;
				case Screen.Party: DrawParty(d); break;
				case Screen.Save:
				case Screen.Load: DrawSlots(d); break;
			}
			if (Ff4Saves.NoticeFrames > 0 && !string.IsNullOrEmpty(Ff4Saves.Notice))
			{
				float w = d.MeasureText(Ff4Saves.Notice, 15) + 40;
				Window(d, 400 - w / 2, 220, w, 36);
				Centred(d, Ff4Saves.Notice, 400, 229, Gold, 15);
			}
		}

		/// <summary>The party's rows on the left, as the Root and Party screens show them.</summary>
		private void DrawPlane(DrawList d, int gloveAt, int secondGlove = -1)
		{
			IReadOnlyList<Character> members = Ff4Party.Party.Members;
			GameTables tables = Ff4Party.Tables;
			for (int i = 0; i < 5; i++)
			{
				float y = 4 + i * PlaneRow;
				Window(d, PlaneX, y, PlaneW, PlaneRowH);
				if (i >= members.Count) continue;
				Character c = members[i];
				Portrait(d, c, PlaneX + 10, y + 12, 66);
				Text(d, c.Name, PlaneX + 104, y + 16, c.Alive ? Color.White : Dim, 17);
				Text(d, T(50401, "Lv"), PlaneX + 104, y + 46, Color.White, 16);
				Right(d, c.Level.ToString(), PlaneX + 190, y + 46, Color.White, 16);
				Text(d, T(50410, "HP"), PlaneX + 276, y + 16, Color.White, 16);
				Right(d, c.Hp + " / " + c.MaxHp, PlaneX + 440, y + 16, c.Hp * 4 <= c.MaxHp ? Low : Color.White, 16);
				Text(d, T(50411, "MP"), PlaneX + 276, y + 46, Color.White, 16);
				Right(d, c.Mp + " / " + c.MaxMp, PlaneX + 440, y + 46, Color.White, 16);
				if (i == gloveAt) Glove(d, PlaneX + 44, y + 42);
				if (i == secondGlove) Glove(d, PlaneX + 44, y + 70);
			}
		}

		private void DrawRoot(DrawList d)
		{
			DrawPlane(d, _mode == Mode.PickMember ? _member : -1);
			// The commands, a window each, six visible, a bar for the rest.
			for (int k = 0; k < ColVisible && _commandScroll + k < _commands.Count; k++)
			{
				int i = _commandScroll + k;
				float y = 4 + k * ColRow;
				Window(d, ColX, y, ColW, ColRowH);
				Centred(d, T(_commands[i].Text), ColX + ColW / 2, y + 20, _commands[i].Later ? Dim : Color.White, 18);
				if (i == _command && _mode == Mode.Browse) Glove(d, ColX + 40, y + 34);
			}
			if (_commands.Count > ColVisible)
			{
				float track = ColVisible * ColRow - 4, knob = Math.Max(20f, track * ColVisible / _commands.Count);
				d.Rect(768, 4, 24, track, new Color(20, 22, 60, 140));
				d.Rect(770, 4 + (track - knob) * _commandScroll / Math.Max(1, _commands.Count - (int)ColVisible), 20, knob, new Color(214, 218, 242, 220));
			}
			// The place and the gil.
			Window(d, ColX, 416, ColW, 60);
			Text(d, PlaceName(), ColX + 14, 424, Color.White, 16);
			Right(d, Ff4Party.Party.Gil + T(50446, "Gil"), ColX + ColW - 14, 448, Color.White, 16);
		}

		/// <summary>The place: the name the map's own plate last showed (a message of the common table), else the map's id.</summary>
		private static string PlaceName()
		{
			try
			{
				int no = GlobalScope.menu.MapNameWindow.LastMessageNo;
				if (no >= 0)
				{
					string text = GlobalScope.dgs.msg.CMessageSys.getInstance().Main().getMessage((uint)no);
					if (!string.IsNullOrEmpty(text)) return text.Replace("\n", " ").Trim();
				}
			}
			catch (Exception) { }
			return Game.Field.Map ?? "";
		}

		private void Frame(DrawList d, string title)
		{
			Window(d, TitleX, 0, TitleW, TitleH);
			Centred(d, title, TitleX + TitleW / 2, 8, Color.White, 17);
			Window(d, TitleX, MainY, TitleW, MainH);
			Window(d, TitleX, FooterY, TitleW, FooterH);
		}

		private void Header(DrawList d, Character c, float x, float y)
		{
			GameTables tables = Ff4Party.Tables;
			Portrait(d, c, x + 32, y + 12, 56);
			Text(d, c.Name, x + 100, y + 22, Color.White, 17);
			Text(d, T(50401, "Lv"), x + 100, y + 46, Color.White, 16);
			Right(d, c.Level.ToString(), x + 200, y + 46, Color.White, 16);
			Text(d, tables?.Character(c.Id)?.ClassName ?? "", x + 270, y + 22, Color.White, 17);
			Text(d, T(50410, "HP"), x + 460, y + 22, Color.White, 16);
			Right(d, c.Hp + " / " + c.MaxHp, x + 650, y + 22, c.Hp * 4 <= c.MaxHp ? Low : Color.White, 16);
			Text(d, T(50411, "MP"), x + 460, y + 46, Color.White, 16);
			Right(d, c.Mp + " / " + c.MaxMp, x + 650, y + 46, Color.White, 16);
		}

		private void DrawStatus(DrawList d)
		{
			Character c = Member;
			GameTables tables = Ff4Party.Tables;
			Frame(d, T(50005, "Status"));
			float x = TitleX, y = MainY;
			Header(d, c, x, y);
			// The attributes where MenuLayout_Status puts its rows (frames 4030.. and 4080..; a DS unit is two pixels here).
			OpenFF.Data.Stats s = c.StatsWith(tables);
			(uint text, int value, int frame, int fallbackY)[] rows =
			{
				(50420, s.Strength, 4030, 16), (50421, s.Agility, 4040, 28), (50422, s.Vitality, 4050, 40), (50423, s.Intellect, 4060, 52), (50424, s.Spirit, 4070, 64),
				(50425, Math.Max(1, Ff4Battle.Weapon(c, tables) > 0 ? Ff4Battle.Weapon(c, tables) : s.Strength / 2), 4080, 84), (50426, Ff4Battle.Weapon(c, tables) > 0 ? Ff4Battle.WeaponHit(c, tables) : 90, 4090, 96),
				(50427, Ff4Battle.Armour(c, tables), 4100, 108), (50428, Ff4Battle.Evasion(c, tables), 4110, 120), (50429, Ff4Battle.MagicArmour(c, tables), 4120, 132), (50430, 0, 4130, 144),
			};
			foreach (var r in rows)
			{
				float ry = y + 60 + Ff4Layouts.FrameY("Status", r.frame, r.fallbackY) * 2;
				Text(d, T(r.text), x + 32, ry, Color.White, 15);
				Right(d, r.value.ToString(), x + 250, ry, Color.White, 15);
			}
			Text(d, T(50451, "EXP"), x + 290, y + 92, Color.White, 16);
			Right(d, c.Experience.ToString(), x + 644, y + 92, Color.White, 16);
			Text(d, T(50402, "For next level"), x + 290, y + 116, Color.White, 16);
			Right(d, NextLevel(c).ToString(), x + 644, y + 116, Color.White, 16);
			// What is worn.
			Window(d, x + 270, y + 197, 370, 154);
			for (int i = 0; i < 5; i++)
			{
				float ry = y + 203 + 30 * i;
				Centred(d, SlotName(i), x + 320, ry + 3, Color.White, 15);
				Window(d, x + 364, ry, 274, 24);
				int id = c.Equipment[i];
				Text(d, id != 0 ? tables?.Item(id)?.Name ?? ("item " + id) : "", x + 374, ry + 3, Color.White, 15);
			}
			KeyHint(d, "Z", T(50011, "Abilities"), TitleX + 480, FooterY + 17);
			KeyHint(d, "X", "Back", TitleX + 590, FooterY + 17);
		}

		private int NextLevel(Character c)
		{
			int[] curve = Ff4Party.Tables?.ExperienceToLevel;
			if (curve == null || c.Level >= curve.Length) return 0;
			return Math.Max(0, curve[c.Level] - c.Experience);
		}

		private void DrawInventory(DrawList d)
		{
			IReadOnlyList<OpenFF.Data.ItemStack> items = Ff4Party.Party.Inventory;
			GameTables tables = Ff4Party.Tables;
			Window(d, TitleX, 0, TitleW, TitleH);
			Centred(d, T(50002, "Inventory"), TitleX + TitleW / 2, 8, Color.White, 17);
			Window(d, TitleX, MainY, TitleW, 40);
			ItemDefinition picked = items.Count > 0 && _cursor < items.Count ? tables?.Item(items[_cursor].ItemId) : null;
			Text(d, picked?.Caption ?? picked?.Name ?? "", TitleX + 16, MainY + 10, Color.White, 15);
			float listY = MainY + 44, listH = FooterY - listY - 4;
			Window(d, TitleX, listY, TitleW, listH);
			const int cols = 2, rows = 11;
			float colW = (TitleW - 20) / cols, rowH = 30;
			for (int k = 0; k < cols * rows && _scroll + k < items.Count; k++)
			{
				int i = _scroll + k;
				float cx = TitleX + 10 + colW * (k % cols), cy = listY + 6 + rowH * (k / cols);
				ItemDefinition item = tables?.Item(items[i].ItemId);
				Text(d, item?.Name ?? ("item " + items[i].ItemId), cx + 40, cy + 5, Color.White, 15);
				Right(d, items[i].Count.ToString(), cx + colW - 14, cy + 5, Dim, 15);
				if (i == _cursor && _mode == Mode.Browse) Glove(d, cx + 34, cy + 16);
			}
			if (items.Count == 0) Text(d, "Nothing in the bag.", TitleX + 50, listY + 16, Dim, 15);
			if (_mode == Mode.ItemTarget)
			{
				IReadOnlyList<Character> members = Ff4Party.Party.Members;
				float w = 320, h = 30 + 28 * members.Count, wx = 400 - w / 2, wy = 150;
				Window(d, wx, wy, w, h);
				Text(d, "Use on whom?", wx + 16, wy + 6, Gold, 14);
				for (int i = 0; i < members.Count; i++)
				{
					Character c = members[i];
					Text(d, c.Name, wx + 50, wy + 30 + 28 * i, c.Alive ? Color.White : Dim, 15);
					Right(d, c.Hp + " / " + c.MaxHp, wx + w - 16, wy + 30 + 28 * i, Color.White, 14);
					if (i == _pick) Glove(d, wx + 42, wy + 42 + 28 * i);
				}
			}
			Window(d, TitleX, FooterY, TitleW, FooterH);
			KeyHint(d, "Z", T(50101, "Use"), TitleX + 480, FooterY + 17);
			KeyHint(d, "X", "Back", TitleX + 590, FooterY + 17);
		}

		private void DrawEquipment(DrawList d)
		{
			Character c = Member;
			GameTables tables = Ff4Party.Tables;
			Frame(d, T(50004, "Equipment"));
			float x = TitleX, y = MainY;
			Header(d, c, x, y);
			for (int i = 0; i < 5; i++)
			{
				float ry = y + 100 + 34 * i;
				Text(d, SlotName(i), x + 48, ry, Color.White, 16);
				Window(d, x + 130, ry - 4, 200, 28);
				int id = c.Equipment[i];
				Text(d, id != 0 ? tables?.Item(id)?.Name ?? ("item " + id) : "", x + 142, ry, Color.White, 15);
				if (i == _slot && _mode == Mode.EquipSlot) Glove(d, x + 40, ry + 12);
			}
			if (_mode == Mode.EquipItem)
			{
				float lx = x + 350, ly = y + 92, lw = 300, lh = 288;
				Window(d, lx, ly, lw, lh);
				int first = Math.Max(0, Math.Min(_pick - 9, _equipChoices.Count - 10));
				for (int i = first; i < _equipChoices.Count && i < first + 10; i++)
				{
					int id = _equipChoices[i];
					ItemDefinition item = id != 0 ? tables?.Item(id) : null;
					float ry = ly + 8 + 27 * (i - first);
					Text(d, id == 0 ? T(50202, "Remove") : item?.Name ?? ("item " + id), lx + 44, ry, Color.White, 15);
					if (item?.Equip != null) Right(d, (item.Kind == ItemKind.Weapon ? T(50425, "Attack") + " " + item.Equip.Attack : T(50427, "Defense") + " " + item.Equip.Defence), lx + lw - 12, ry + 1, Dim, 13);
					if (i == _pick) Glove(d, lx + 38, ry + 11);
				}
			}
			KeyHint(d, "Z", _mode == Mode.EquipItem ? "Equip" : "Change", TitleX + 460, FooterY + 17);
			KeyHint(d, "X", "Back", TitleX + 590, FooterY + 17);
		}

		private void DrawList(DrawList d)
		{
			Character c = Member;
			GameTables tables = Ff4Party.Tables;
			bool magic = _screen == Screen.Magic;
			Frame(d, T(magic ? 50003u : 50011u, magic ? "Magic" : "Abilities"));
			float x = TitleX, y = MainY;
			Header(d, c, x, y);
			List<int> list = magic ? c.Spells : c.Abilities;
			const int cols = 3, rows = 9;
			float colW = (TitleW - 40) / cols;
			for (int k = 0; k < cols * rows && _scroll + k < list.Count; k++)
			{
				int i = _scroll + k;
				float cx = x + 20 + colW * (k % cols), cy = y + 96 + 30 * (k / cols);
				string name = magic ? tables?.Spell(list[i])?.Name ?? tables?.AbilityName(list[i]) : tables?.AbilityName(list[i]);
				Text(d, name ?? list[i].ToString(), cx + 40, cy, Color.White, 15);
				if (magic)
				{
					SpellDefinition spell = tables?.Spell(list[i]);
					if (spell != null) Right(d, spell.MpCost.ToString(), cx + colW - 12, cy + 2, Dim, 13);
				}
				if (i == _cursor) Glove(d, cx + 34, cy + 11);
			}
			if (list.Count == 0) Text(d, magic ? "No magic yet." : "No abilities.", x + 60, y + 100, Dim, 15);
			KeyHint(d, "X", "Back", TitleX + 590, FooterY + 17);
		}

		private void DrawParty(DrawList d)
		{
			DrawPlane(d, _cursor, _swapFrom);
			Window(d, ColX, 4, ColW, 100);
			Text(d, T(50010, "Party"), ColX + 16, 14, Color.White, 17);
			Text(d, _swapFrom < 0 ? "Pick a member, then the place to move them to." : "Move " + Ff4Party.Party.Members[_swapFrom].Name + " where?", ColX + 16, 44, Dim, 13);
			Window(d, ColX, 416, ColW, 60);
			KeyHint(d, "Z", "Pick", ColX + 16, 434);
			KeyHint(d, "X", "Back", ColX + 150, 434);
		}

		private void DrawSlots(DrawList d)
		{
			bool loading = _screen == Screen.Load;
			Frame(d, T(loading ? 50008u : 50007u, loading ? "Load" : "Save"));
			for (int i = 0; i < Ff4Saves.SlotCount; i++)
			{
				float y = MainY + 20 + 110 * i;
				Window(d, TitleX + 30, y, TitleW - 60, 96);
				Text(d, "Slot " + (i + 1), TitleX + 90, y + 16, Color.White, 17);
				Text(d, Ff4Saves.Describe(i + 1), TitleX + 90, y + 50, Ff4Saves.Exists(i + 1) ? Color.White : Dim, 14);
				if (i == _cursor) Glove(d, TitleX + 82, y + 30);
			}
			KeyHint(d, "Z", loading ? T(50008, "Load") : T(50007, "Save"), TitleX + 480, FooterY + 17);
			KeyHint(d, "X", "Back", TitleX + 590, FooterY + 17);
		}

		public override IEnumerable<string> DebugLines()
		{
			if (_open) yield return "OpenFF menu open (" + _screen + (_mode != Mode.Browse ? ", " + _mode : "") + ")";
		}
	}
}
