// The OpenFF menu on FF4, drawn as the Steam game draws its own: FF4's window art (Ff4Ui -
// Steam's window.png and point.png, or the phone's frames), its texts (babil_menu.msd through
// Ff4Layouts), the command list in the order MenuLayout_Root gives it, the Status screen's
// rows where MenuLayout_Status puts them - over the unified party (OpenFF.Data through
// Ff4Party), the way a mod would draw it.
//
// FF4's own menu is world::WSMenu with a sub-state per screen (Docs/FF4-Internals.md); the
// layouts are DS-unit rectangles and the phone stretches them over its 16:9 screen in code
// not read yet, so the windows here stand where Steam screenshots taken for the port show them: the
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
using System.Linq;
using OpenFF;
using OpenFF.Data;

namespace OpenFF.Client
{
	internal sealed partial class Ff4Menu : GameService
	{
		private enum Screen { Root, Status, Inventory, Equipment, Magic, Abilities, Gambits, Party, Save, Load, Quicksave }
		private enum Mode { Browse, PickMember, EquipSlot, EquipItem, ItemTarget, SwapMember }

		private sealed class Command
		{
			public uint Text;
			public string Name;   // OpenFF's own command (Gambits): its word, no message of the game's
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
			_question = false;
			// Balloon::blnCreate picks the thought as the menu opens (and may set or clear its flags then).
			try { _thought = Ff4Speculation.Thought(); } catch (Exception) { _thought = null; }
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
					case 50011:
						_commands.Add(new Command { Text = id, Screen = Screen.Abilities, NeedsMember = true });
						_commands.Add(new Command { Name = "Gambits", Screen = Screen.Gambits, NeedsMember = true });   // OpenFF's auto-battle rules
						break;
					case 50005: _commands.Add(new Command { Text = id, Screen = Screen.Status, NeedsMember = true }); break;
					case 50010: _commands.Add(new Command { Text = id, Screen = Screen.Party }); break;
					case 50007: _commands.Add(new Command { Text = id, Screen = Screen.Save }); break;
					case 50009: _commands.Add(new Command { Text = id, Screen = Screen.Quicksave }); break;
					default: _commands.Add(new Command { Text = id, Later = true }); break;   // Settings
				}
			}
		}

		// ---- Steam's arrangement drawn as a layout (Ff4MenuHud) ----

		private readonly Ff4MenuHud.Data _hud = new Ff4MenuHud.Data();
		private string _thought;
		private bool _question, _questionYes;

		/// <summary>Whether Save may be chosen: FF4 saves on the world map and at save points (the points not read yet: the
		/// world map only), Save greyed elsewhere and choosing it does nothing - as Steam's menu does away from a point.</summary>
		private static bool SaveAllowed => (Game.Field.Map ?? "").StartsWith("f", StringComparison.OrdinalIgnoreCase);

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
			// The menu key closes it from the main menu; in a screen C and M are the screen's (Key Items, the next member).
			bool atRoot = _screen == Screen.Root && _mode == Mode.Browse && !_question;
			if (input.KeyPressed("Escape") || (atRoot && (input.Pressed(Pad.X) || input.KeyPressed("M")))) { Close(); return; }
			if (Ff4Party.Party.Members.Count == 0) { Close(); return; }
			_member = Math.Clamp(_member, 0, Ff4Party.Party.Members.Count - 1);
			switch (_screen)
			{
				case Screen.Root: UpdateRoot(input); break;
				case Screen.Status: UpdateMemberScreen(input, null); break;
				case Screen.Inventory: UpdateInventory(input); break;
				case Screen.Equipment: UpdateEquipment(input); break;
				case Screen.Magic: UpdateMemberScreen(input, ListCount()); break;
				case Screen.Gambits: UpdateGambits(input); break;
				case Screen.Abilities:
					if (Ff4MenuHud.Available) { UpdateAbilities(input); break; }
					UpdateMemberScreen(input, ListCount()); break;
				case Screen.Party:
					if (Ff4MenuHud.Available) { UpdatePartyLayout(input); break; }
					UpdateParty(input); break;
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
			if (_question) { UpdateQuestion(input); return; }
			if (input.Pressed(Pad.B)) { Close(); return; }
			if (input.Pressed(Pad.Up)) _command = (_command + _commands.Count - 1) % _commands.Count;
			if (input.Pressed(Pad.Down)) _command = (_command + 1) % _commands.Count;
			if (_command < _commandScroll) _commandScroll = _command;
			if (_command >= _commandScroll + ColVisible) _commandScroll = _command - (int)ColVisible + 1;
			if (input.Pressed(Pad.A))
			{
				Command c = _commands[_command];
				if (c.Later) { Notice(Ff4Layouts.Text(c.Text) + " comes later."); return; }
				if (c.Screen == Screen.Quicksave) { _question = true; _questionYes = false; return; }   // "Quicksave game and quit?", the hand on No
				if (c.Screen == Screen.Save && !SaveAllowed) return;
				if (c.NeedsMember) { _mode = Mode.PickMember; return; }
				OpenScreen(c.Screen);
			}
		}

		/// <summary>Quicksave's question (MSSSuspend): Yes saves the game as it stands and goes back to the title; No, or Back, closes it.</summary>
		private void UpdateQuestion(InputState input)
		{
			if (input.Pressed(Pad.Left) || input.Pressed(Pad.Right)) _questionYes = !_questionYes;
			if (input.Pressed(Pad.B)) { _question = false; return; }
			if (!input.Pressed(Pad.A)) return;
			_question = false;
			if (!_questionYes) return;
			if (!Ff4Saves.Suspend()) { Notice("Could not quicksave here."); return; }
			Close();
			// The field ends into the title part, as the game's own way back there does (ff3Command_GoToTitle's).
			try { GlobalScope.wld.CBaseSystem.setTitle(true); } catch (Exception ex) { Log.Write(LogChannel.General, "menu: quicksave, to the title: " + ex.Message); }
		}

		private void OpenScreen(Screen screen)
		{
			_screen = screen;
			_mode = screen == Screen.Equipment ? Mode.EquipSlot : Mode.Browse;
			_cursor = _scroll = 0;
			_slot = 0;
			_pick = 0;
			if (screen == Screen.Gambits) OpenGambits();
			Log.Write(LogChannel.File, "menu: " + screen + (NeedsMember(screen) ? " of " + Ff4Party.Party.Members[_member].Name : ""));
		}

		private static bool NeedsMember(Screen s) => s == Screen.Status || s == Screen.Equipment || s == Screen.Magic || s == Screen.Abilities || s == Screen.Gambits;

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
			if (_screen == Screen.Magic)
			{
				// Z and M: the member before and after (Steam's "Change Characters"); C: the next school.
				int n = Ff4Party.Party.Members.Count;
				if (input.Pressed(Pad.Y)) { _member = (_member + n - 1) % n; _cursor = _scroll = 0; }
				if (input.KeyPressed("M")) { _member = (_member + 1) % n; _cursor = _scroll = 0; }
				if (input.Pressed(Pad.X))
				{
					List<OpenFF.Data.MagicSchool> schools = SchoolsOf(Member);
					if (schools.Count > 1) { _school = schools[(schools.IndexOf(_school) + 1) % schools.Count]; _cursor = _scroll = 0; }
				}
				listCount = ListCount();
			}
			else SwitchMember(input);
			if (_screen == Screen.Status && input.Pressed(Pad.A)) { OpenScreen(Screen.Abilities); return; }
			if (listCount.HasValue && listCount.Value > 0)
			{
				int cols = 3, rows = _screen == Screen.Magic ? 5 : 9;
				if (input.Pressed(Pad.Left)) _cursor = Math.Max(0, _cursor - 1);
				if (input.Pressed(Pad.Right)) _cursor = Math.Min(listCount.Value - 1, _cursor + 1);
				if (input.Pressed(Pad.Up)) _cursor = Math.Max(0, _cursor - cols);
				if (input.Pressed(Pad.Down)) _cursor = Math.Min(listCount.Value - 1, _cursor + cols);
				int row = _cursor / cols, top = _scroll / cols;
				if (row < top) _scroll = row * cols;
				if (row >= top + rows) _scroll = (row - rows + 1) * cols;
			}
		}

		private int ListCount() => _screen == Screen.Magic ? SpellsShown().Count : Member.Abilities.Count;

		// ---- Steam's screens: the bag or the key items, a school of magic at a time ----

		private bool _keyItems;
		private OpenFF.Data.MagicSchool _school;

		/// <summary>What the Inventory lists: the bag without its key items, or the key items alone (C).</summary>
		private int _sortMode;
		private string _sortNote;

		/// <summary>MSSItem::mssiSortNormalItem: the consumables, the weapons and the armour each in their records' order (the
		/// short at 8, ascending), the three one after another as the order says (seitonTopItem, seitonTopWeapon,
		/// seitonTopArmer); the key items after them as they were.</summary>
		private static void SortBag(int order)
		{
			List<OpenFF.Data.ItemStack> bag = Ff4Party.Party.Inventory;
			GameTables tables = Ff4Party.Tables;
			int Key(OpenFF.Data.ItemStack st) { byte[] r = tables?.Item(st.ItemId)?.Raw; return r != null && r.Length >= 10 ? BitConverter.ToInt16(r, 8) : st.ItemId; }
			List<OpenFF.Data.ItemStack> Of(ItemKind kind) => bag.Where(st => tables?.Item(st.ItemId)?.Kind == kind).OrderBy(Key).ToList();
			List<OpenFF.Data.ItemStack> items = Of(ItemKind.Consumable), weapons = Of(ItemKind.Weapon), armour = Of(ItemKind.Armour);
			List<OpenFF.Data.ItemStack> rest = bag.Where(st => !items.Contains(st) && !weapons.Contains(st) && !armour.Contains(st)).ToList();
			List<OpenFF.Data.ItemStack>[] groups = order == 1 ? new[] { weapons, armour, items } : order == 2 ? new[] { armour, items, weapons } : new[] { items, weapons, armour };
			bag.Clear();
			foreach (List<OpenFF.Data.ItemStack> g in groups) bag.AddRange(g);
			bag.AddRange(rest);
		}

		private List<OpenFF.Data.ItemStack> Bag()
		{
			List<OpenFF.Data.ItemStack> bag = new List<OpenFF.Data.ItemStack>();
			foreach (OpenFF.Data.ItemStack s in Ff4Party.Party.Inventory)
			{
				bool key = Ff4Party.Tables?.Item(s.ItemId)?.Kind == ItemKind.KeyItem;
				if (key == _keyItems) bag.Add(s);
			}
			return bag;
		}

		/// <summary>The schools a member has spells of, in FF4's order (white, black, summons, ...).</summary>
		private static List<OpenFF.Data.MagicSchool> SchoolsOf(Character c)
		{
			List<OpenFF.Data.MagicSchool> schools = new List<OpenFF.Data.MagicSchool>();
			foreach (int id in c.Spells)
			{
				SpellDefinition s = Ff4Party.Tables?.Spell(id);
				if (s != null && !schools.Contains(s.School)) schools.Add(s.School);
			}
			schools.Sort();
			return schools;
		}

		/// <summary>The member's spells of the school shown.</summary>
		private List<int> SpellsShown()
		{
			List<OpenFF.Data.MagicSchool> schools = SchoolsOf(Member);
			if (schools.Count > 0 && !schools.Contains(_school)) _school = schools[0];
			List<int> list = new List<int>();
			foreach (int id in Member.Spells) if (Ff4Party.Tables?.Spell(id)?.School == _school) list.Add(id);
			return list;
		}

		/// <summary>C's word on Magic: babil_menu.msd's key hints (60231 Black Magic, 60232 White Magic, 60233 Summon, 60235 Ninjutsu).</summary>
		private static string SchoolName(OpenFF.Data.MagicSchool school) => school switch
		{
			OpenFF.Data.MagicSchool.White => KeyText(60232, "White Magic"),
			OpenFF.Data.MagicSchool.Black => KeyText(60231, "Black Magic"),
			OpenFF.Data.MagicSchool.Summon => KeyText(60233, "Summon"),
			OpenFF.Data.MagicSchool.Ninjutsu => KeyText(60235, "Ninjutsu"),
			_ => school.ToString(),
		};

		/// <summary>A key hint's word without the "%key_assign12%" naming the key (the layout draws the key's own cap).</summary>
		private static string KeyText(uint id, string fallback) => System.Text.RegularExpressions.Regex.Replace(T(id, fallback), "%[A-Za-z_0-9]+%", "");

		// ---- inventory ----

		private void UpdateInventory(InputState input)
		{
			IReadOnlyList<OpenFF.Data.ItemStack> items = Bag();
			if (input.Pressed(Pad.Up) || input.Pressed(Pad.Down) || input.Pressed(Pad.Left) || input.Pressed(Pad.Right) || input.Pressed(Pad.A) || input.Pressed(Pad.X)) _sortNote = null;
			if (_mode == Mode.Browse && input.Pressed(Pad.X)) { _keyItems = !_keyItems; _cursor = _scroll = 0; return; }   // C: Key Items
			if (_mode == Mode.Browse && !_keyItems && input.KeyPressed("Tab"))
			{
				// Tab: MSSItem's Sort - the line for the order (50110 consumables, 50111 weapons, 50112 armour at the top), the
				// bag sorted so, and the next press the next order.
				_sortNote = T((uint)(50110 + _sortMode));
				SortBag(_sortMode);
				_sortMode = (_sortMode + 1) % 3;
				_cursor = _scroll = 0;
				return;
			}
			if (_mode == Mode.ItemTarget)
			{
				int n = Ff4Party.Party.Members.Count;
				if (input.Pressed(Pad.Up)) _pick = (_pick + n - 1) % n;
				if (input.Pressed(Pad.Down)) _pick = (_pick + 1) % n;
				if (input.Pressed(Pad.B)) { _mode = Mode.Browse; return; }
				if (input.Pressed(Pad.A))
				{
					Notice(Ff4Augments.AbilityOf(_usingItem) > 0 ? Ff4Augments.Use(_usingItem, Ff4Party.Party.Members[_pick]) : Use(_usingItem, Ff4Party.Party.Members[_pick]));
					if (Ff4Party.Party.CountItem(_usingItem) == 0) _mode = Mode.Browse;
				}
				return;
			}
			if (input.Pressed(Pad.B)) { Back(); return; }
			if (items.Count == 0) return;
			const int cols = 2, rows = 7;
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
				if (FieldUsable(id) && (UsableEffect(id) != null || CuresOf(Ff4Party.Tables?.Item(id)) != 0 || Ff4Augments.AbilityOf(id) > 0)) { _usingItem = id; _mode = Mode.ItemTarget; _pick = 0; }   // an augment: on whom (mssdLearnAbility)
				else Notice("That cannot be used here.");
			}
		}

		// ---- equipment ----

		private void UpdateEquipment(InputState input)
		{
			Character member = Member;
			if (_mode == Mode.EquipItem)
			{
				if (input.Pressed(Pad.Up)) _pick = Math.Max(0, _pick - 1);
				if (input.Pressed(Pad.Down)) _pick = Math.Min(_equipChoices.Count - 1, _pick + 1);
				if (_pick < _scroll) _scroll = _pick;
				if (_pick >= _scroll + 5) _scroll = _pick - 4;
				if (input.Pressed(Pad.B)) { _mode = Mode.EquipSlot; return; }
				if (input.Pressed(Pad.A))
				{
					int id = _equipChoices[_pick];
					if (id != 0) Ff4Party.Party.RemoveItem(id, 1);
					Ff4Party.Party.Equip(member.Id, (OpenFF.Data.EquipSlot)_slot, id);
					Log.Write(LogChannel.File, "menu: " + member.Name + " " + (id == 0 ? "takes off the " + SlotName(_slot).ToLower() : "equips " + (Ff4Party.Tables?.Item(id)?.Name ?? id.ToString()) + " (" + SlotName(_slot).ToLower() + ")"));
					_mode = Mode.EquipSlot;
					_scroll = 0;
				}
				return;
			}
			if (input.Pressed(Pad.B)) { Back(); return; }
			SwitchMember(input);
			if (input.Pressed(Pad.Up)) { _slot = (_slot + 4) % 5; _scroll = 0; }
			if (input.Pressed(Pad.Down)) { _slot = (_slot + 1) % 5; _scroll = 0; }
			// C: the lit slot's piece back into the bag; Tab: the best the bag has for every slot.
			if (input.Pressed(Pad.X) && member.Equipment[_slot] != 0) { Ff4Party.Party.Equip(member.Id, (OpenFF.Data.EquipSlot)_slot, 0); return; }
			if (input.KeyPressed("Tab")) { Optimize(member); return; }
			if (input.Pressed(Pad.A))
			{
				_equipChoices.Clear();
				_equipChoices.AddRange(Candidates(member, _slot));
				if (_equipChoices.Count == 0) return;
				_mode = Mode.EquipItem;
				_pick = 0;
				_scroll = 0;
			}
		}

		private static string CommandName(int id) => id > 0 ? Ff4Party.Tables?.AbilityName(3000 + id)?.Trim() ?? "" : "";

		/// <summary>Abilities: the hand on the auto-battle command (0) or one of the five (1..5); Enter on one of the five picks it
		/// up and Enter on another sets it down there, the two swapping places; left and right the member before and after.</summary>
		private void UpdateAbilities(InputState input)
		{
			if (input.Pressed(Pad.B)) { if (_swapFrom >= 0) _swapFrom = -1; else Back(); return; }
			if (_swapFrom < 0) SwitchMember(input);
			if (input.Pressed(Pad.Up)) _cursor = Math.Max(_swapFrom >= 0 ? 1 : 0, _cursor - 1);
			if (input.Pressed(Pad.Down)) _cursor = Math.Min(5, _cursor + 1);
			if (!input.Pressed(Pad.A) || _cursor == 0) return;
			int at = _cursor - 1;
			if (_swapFrom < 0) { _swapFrom = at; return; }
			int[] slots = Ff4Augments.Slots(Member);
			(slots[_swapFrom], slots[at]) = (slots[at], slots[_swapFrom]);
			_swapFrom = -1;
		}

		/// <summary>Tab on Equipment: every slot takes the bag's best piece for it when that beats what is worn.</summary>
		private static void Optimize(Character member)
		{
			for (int slot = 0; slot < 5; slot++)
			{
				int best = member.Equipment[slot], worth = Worth(best);
				foreach (int id in Candidates(member, slot))
				{
					if (Worth(id) > worth) { best = id; worth = Worth(id); }
				}
				if (best == member.Equipment[slot]) continue;
				Ff4Party.Party.RemoveItem(best, 1);
				Ff4Party.Party.Equip(member.Id, (OpenFF.Data.EquipSlot)slot, best);
			}
			Log.Write(LogChannel.File, "menu: " + member.Name + " optimized");
		}

		/// <summary>Whether an item may go into a member's slot: worn, its position bits name the slot, and its mask names the character type.</summary>
		internal static bool Fits(ItemDefinition item, Character member, int slot)
		{
			if (item?.Equip == null) return false;
			if (item.Kind == ItemKind.Weapon && slot > 1) return false;
			if (item.Kind == ItemKind.Armour && slot <= 1 && (item.Equip.Position & 0xFFFF & 3) == 0) return false;
			int bits = item.Equip.Position & 0xFFFF;
			if ((bits & SlotBits[slot]) == 0) return false;
			return item.Equip.CanEquip == 0 || (item.Equip.CanEquip & (1u << member.Id)) != 0;
		}

		internal static string SlotName(int slot) => Ff4Layouts.Text(SlotTexts[slot], SlotFallback[slot]);

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

		/// <summary>Party (MSSFormation): the hand on Swap Rows (PlayerParty::changeFormation) or Party Formation, which puts it on
		/// the five places - Enter on one, then on another, and the two trade places (changeMemberForOrder).</summary>
		private void UpdatePartyLayout(InputState input)
		{
			if (_mode == Mode.SwapMember)
			{
				if (input.Pressed(Pad.B)) { if (_swapFrom >= 0) _swapFrom = -1; else _mode = Mode.Browse; return; }
				if (input.Pressed(Pad.Up)) _cursor = (_cursor + 4) % 5;
				if (input.Pressed(Pad.Down)) _cursor = (_cursor + 1) % 5;
				if (!input.Pressed(Pad.A)) return;
				if (_swapFrom < 0) { _swapFrom = _cursor; return; }
				if (_swapFrom != _cursor) Ff4Party.SwapPlaces(_swapFrom, _cursor);
				Log.Write(LogChannel.File, "menu: places " + _swapFrom + " and " + _cursor + " swapped");
				_swapFrom = -1;
				return;
			}
			if (input.Pressed(Pad.B)) { Back(); return; }
			if (input.Pressed(Pad.Up) || input.Pressed(Pad.Down)) _pick = 1 - _pick;
			if (!input.Pressed(Pad.A)) return;
			if (_pick == 0) { Ff4Party.Formation = 1 - Ff4Party.Formation; Log.Write(LogChannel.File, "menu: rows swapped (formation " + Ff4Party.Formation + ")"); }
			else { _mode = Mode.SwapMember; _cursor = 0; _swapFrom = -1; }
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

		/// <summary>Whether the menu offers the item (WSCMenu::checkItem): the record's flags at 0x12 - bit 2 usable in the field,
		/// bit 3 a camp item (a Tent: where the game may be saved); greyed otherwise.</summary>
		private static bool FieldUsable(int itemId)
		{
			ItemDefinition item = Ff4Party.Tables?.Item(itemId);
			if (item?.Raw == null || item.Raw.Length < 0x14) return false;
			int flags = BitConverter.ToUInt16(item.Raw, 0x12);
			return ((flags & 8) != 0 && SaveAllowed) || (flags & 4) != 0;
		}

		/// <summary>The conditions a consumable takes away (its record's mask at 0x24; itm::ItemUse::useConditionItem), 0 for none.</summary>
		private static ulong CuresOf(ItemDefinition item) =>
			item?.Kind == ItemKind.Consumable && item.Raw != null && item.Raw.Length >= 0x2E ? BitConverter.ToUInt64(item.Raw, 0x24) : 0;

		private static string Use(int itemId, Character target)
		{
			ItemDefinition item = Ff4Party.Tables?.Item(itemId);
			Efficacy e = UsableEffect(itemId);
			ulong cures = CuresOf(item) & target.Conditions;
			if (item == null || (e == null && CuresOf(item) == 0)) return "Nothing happens.";
			bool revive = e?.Id == 17;
			if (e == null ? cures == 0 : revive != !target.Alive) return item.Name + " does nothing for " + target.Name + ".";
			if (!Ff4Party.Party.RemoveItem(itemId, 1)) return "None left.";
			int hp = target.Hp, mp = target.Mp;
			target.Conditions &= ~cures;
			if (revive) target.Hp = Math.Max(1, target.MaxHp / 4);
			else if (e != null)
			{
				if (e.Hp > 0) target.Hp = Math.Min(target.MaxHp, target.Hp + e.Hp);
				if (e.Mp > 0) target.Mp = Math.Min(target.MaxMp, target.Mp + e.Mp);
			}
			string said = item.Name + ": " + target.Name + (revive ? " rises." : cures != 0 ? " is cured." : (target.Hp != hp ? " +" + (target.Hp - hp) + " HP" : "") + (target.Mp != mp ? " +" + (target.Mp - mp) + " MP" : "") + ".");
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
			// Steam's root leaves the field as it is under the menu; the screens not laid out yet keep the port's dimming.
			if (!(_screen == Screen.Root && Ff4MenuHud.Available)) d.Rect(0, 0, 800, 480, new Color(0, 0, 0, 90));
			switch (_screen)
			{
				case Screen.Root:
					if (Ff4MenuHud.Available) { DrawRootLayout(); break; }
					DrawRoot(d);
					break;
				case Screen.Status:
					if (Ff4MenuHud.Available) { DrawScreenLayout(); break; }
					DrawStatus(d); break;
				case Screen.Inventory:
					if (Ff4MenuHud.Available) { DrawScreenLayout(); break; }
					DrawInventory(d);
					break;
				case Screen.Equipment:
					if (Ff4MenuHud.Available) { DrawScreenLayout(); break; }
					DrawEquipment(d); break;
				case Screen.Magic:
					if (Ff4MenuHud.Available) { DrawScreenLayout(); break; }
					DrawList(d);
					break;
				case Screen.Gambits: DrawScreenLayout(); break;
				case Screen.Abilities:
					if (Ff4MenuHud.Available) { DrawScreenLayout(); break; }
					DrawList(d); break;
				case Screen.Party:
					if (Ff4MenuHud.Available) { DrawScreenLayout(); break; }
					DrawParty(d); break;
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

		/// <summary>The root and the member pick as Steam's menu has them, through the layout (Ff4MenuHud).</summary>
		private void DrawRootLayout()
		{
			Ff4MenuHud.Data h = _hud;
			bool picking = _mode == Mode.PickMember;
			h.Root = true;
			h.Full = h.Inventory = h.Magic = h.Using = false;
			h.Picking = picking;
			h.Bubble = !picking && !string.IsNullOrEmpty(_thought);
			h.Thought = _thought ?? "";
			for (int k = 0; k < h.Command.Count; k++)
			{
				int i = _commandScroll + k;
				Ff4MenuHud.CommandRow row = h.Command[k];
				row.Present = i < _commands.Count;
				if (!row.Present) continue;
				Command c = _commands[i];
				row.Name = c.Name ?? T(c.Text);
				row.Lit = i == _command && !picking && !_question;
				row.Disabled = c.Screen == Screen.Save && !SaveAllowed;
			}
			Ff4MenuHud.Scroll(h.Scroll, _commands.Count, h.Command.Count, _commandScroll);
			h.Location = PlaceName();
			h.Gil = Ff4Party.Party.Gil.ToString();
			h.GilLabel = T(50446, "Gil");
			h.LvLabel = T(50401, "Lv");
			h.HpLabel = T(50410, "HP");
			h.MpLabel = T(50411, "MP");
			// The party's five places, each member where the formation puts them.
			IReadOnlyList<Character> members = Ff4Party.Party.Members;
			Command chosen = _commands[Math.Clamp(_command, 0, _commands.Count - 1)];
			for (int p = 0; p < h.Member.Count; p++)
			{
				Ff4MenuHud.MemberRow row = h.Member[p];
				int index = MemberAt(p);
				row.Present = index >= 0;
				row.Lit = picking && index >= 0 && index == _member;
				if (!row.Present) { row.Name = row.Level = row.Hp = row.MaxHp = row.Mp = row.MaxMp = ""; row.Dim = row.Low = false; continue; }
				Character c = members[index];
				row.Name = c.Name;
				row.Level = c.Level.ToString();
				row.Hp = c.Hp.ToString();
				row.MaxHp = c.MaxHp.ToString();
				row.Mp = c.Mp.ToString();
				row.MaxMp = c.MaxMp.ToString();
				row.Face = c.Id;
				row.Low = c.Alive && c.Hp * 4 <= c.MaxHp;
				// Magic for a member with no spells is nothing to them: greyed, as Steam shows Cecil's.
				row.Dim = !c.Alive || (chosen.Screen == Screen.Magic && c.Spells.Count == 0);
			}
			h.Question = _question;
			h.Yes = _question && _questionYes;
			h.No = _question && !_questionYes;
			h.QuestionText = T(50818, "Quicksave game and quit?");
			Ff4MenuHud.Draw(h);
		}

		/// <summary>A full screen as Steam's menu has it (Inventory, Magic), through the layout.</summary>
		private void DrawScreenLayout()
		{
			Ff4MenuHud.Data h = _hud;
			GameTables tables = Ff4Party.Tables;
			h.Root = h.Picking = h.Bubble = h.Question = false;
			h.Full = true;
			h.Inventory = _screen == Screen.Inventory;
			h.Magic = _screen == Screen.Magic;
			h.Status = _screen == Screen.Status;
			h.Equipment = _screen == Screen.Equipment;
			h.Abilities = _screen == Screen.Abilities;
			h.Gambits = _screen == Screen.Gambits;
			h.Party = _screen == Screen.Party;
			if (h.Party)
			{
				bool places = _mode == Mode.SwapMember;
				FillMembers(h.Member, -1, false);
				for (int p = 0; p < h.Member.Count; p++)
				{
					h.Member[p].Lit = places && p == _cursor;
					h.Member[p].Picked = places && p == _swapFrom;
				}
				h.SwapRowsLit = !places && _pick == 0;
				h.FormationLit = !places && _pick == 1;
				h.SwapRowsLabel = T(50504, "Swap Rows");
				h.FormationLabel = T(50505, "Party Formation");
			}
			if (h.Gambits) FillGambits(h);
			h.LvLabel = T(50401, "Lv");
			h.HpLabel = T(50410, "HP");
			h.MpLabel = T(50411, "MP");
			h.UseLabel = T(50101, "Use");
			h.ChangeLabel = KeyText(60230, "Change Characters");
			if (h.Inventory)
			{
				h.KeyItemsLabel = T(50103, "Key Items");
				h.SortLabel = T(50102, "Sort");
				h.Title = _keyItems ? h.KeyItemsLabel : T(50002, "Inventory");
				List<OpenFF.Data.ItemStack> items = Bag();
				h.Using = _mode == Mode.ItemTarget;
				for (int k = 0; k < h.Item.Count; k++)
				{
					int i = _scroll + k;
					Ff4MenuHud.CellRow row = h.Item[k];
					row.Present = i < items.Count;
					if (!row.Present) continue;
					ItemDefinition item = tables?.Item(items[i].ItemId);
					row.Name = item?.Name ?? ("item " + items[i].ItemId);
					row.Count = items[i].Count.ToString();
					row.Icon = item?.Icon ?? -1;
					row.Lit = i == _cursor && _mode == Mode.Browse;
					// What cannot be used from the menu is grey (Steam's Red Fang, its key items).
					row.Dim = !FieldUsable(items[i].ItemId);
				}
				Ff4MenuHud.Scroll(h.ItemScroll, (items.Count + 1) / 2, 7, _scroll / 2, 675f, true);
				ItemDefinition picked = _cursor < items.Count ? tables?.Item(items[_cursor].ItemId) : null;
				if (h.Using) picked = tables?.Item(_usingItem);
				h.Help = !h.Using && _sortNote != null ? _sortNote : picked?.Caption ?? "";
				h.UseName = picked?.Name ?? "";
				h.UseIcon = picked?.Icon ?? -1;
				if (h.Using) h.ItemScroll.Shown = false;   // the party's places take the list's window, with no bar
				h.UseCount = h.Using ? Ff4Party.Party.CountItem(_usingItem).ToString() : "";
				FillMembers(h.Target, _mode == Mode.ItemTarget ? _pick : -1, false);
			}
			if (h.Magic)
			{
				h.Title = T(50003, "Magic");
				Character c = Member;
				FillHead(h.Head, c);
				List<int> spells = SpellsShown();
				for (int k = 0; k < h.Spell.Count; k++)
				{
					int i = _scroll + k;
					Ff4MenuHud.CellRow row = h.Spell[k];
					row.Present = i < spells.Count;
					if (!row.Present) continue;
					SpellDefinition spell = tables?.Spell(spells[i]);
					row.Name = spell?.Name ?? tables?.AbilityName(spells[i]) ?? spells[i].ToString();
					row.Count = "";
					row.Icon = tables?.AbilityIcon(spells[i]) ?? -1;
					row.Lit = i == _cursor;
					row.Dim = spell != null && spell.MpCost > c.Mp;
				}
				Ff4MenuHud.Scroll(h.SpellScroll, (spells.Count + 2) / 3, 5, _scroll / 3, 526.5f, true);
				SpellDefinition at = _cursor < spells.Count ? tables?.Spell(spells[_cursor]) : null;
				// "  3 MP    Restore a small amount of HP.": babil_ability.msd's line carries the cost (U+E03E before each of its
				// digits: a figure's width).
				h.Help = at == null ? "" : (tables.AbilityHelpIds.TryGetValue(at.Id, out int helpId) && helpId > 0 ? tables.AbilityName(helpId)?.TrimEnd() ?? "" : "").Replace("\ue03e", "");
				List<OpenFF.Data.MagicSchool> schools = SchoolsOf(c);
				h.SchoolLabel = schools.Count > 1 ? SchoolName(schools[(schools.IndexOf(_school) + 1) % schools.Count]) : "";
			}
			if (h.Status)
			{
				Character c = Member;
				h.Title = T(50005, "Status");
				FillHead(h.Head, c);
				h.Job = tables?.Character(c.Id)?.ClassName ?? "";
				FillFigures(h, c);
				h.ExpLabel = T(50451, "EXP");
				h.Exp = c.Experience.ToString();
				h.NextLabel = T(50402, "For next level");
				h.Next = NextLevel(c).ToString();
				h.AbilitiesLabel = T(50011, "Abilities");
				FillSlots(h.Slot, c, -1);
			}
			if (h.Equipment)
			{
				Character c = Member;
				h.Title = T(50004, "Equipment");
				FillHead(h.Head, c);
				FillFigures(h, c);
				FillSlots(h.Slot, c, _mode == Mode.EquipSlot ? _slot : -1);
				// The bag's pieces for the lit slot, five in view; the line is the lit piece's own (babil_item.msd: "Attack: 10
				// Element: Dark").
				List<int> fits = Candidates(c, _slot);
				for (int k = 0; k < h.Choice.Count; k++)
				{
					int i = _scroll + k;
					Ff4MenuHud.CellRow row = h.Choice[k];
					row.Present = i < fits.Count;
					if (!row.Present) continue;
					ItemDefinition item = tables?.Item(fits[i]);
					row.Name = item?.Name ?? ("item " + fits[i]);
					row.Count = Ff4Party.Party.CountItem(fits[i]).ToString();
					row.Icon = item?.Icon ?? -1;
					row.Lit = _mode == Mode.EquipItem && i == _pick;
					row.Dim = false;
				}
				Ff4MenuHud.Scroll(h.ChoiceScroll, fits.Count, 5, _scroll, 472.5f, true);
				int shown = _mode == Mode.EquipItem && _pick < fits.Count ? fits[_pick] : c.Equipment[_slot];
				h.Help = shown != 0 ? tables?.Item(shown)?.Caption ?? "" : "";
				h.OptimizeLabel = T(50203, "Optimize");
				h.RemoveLabel = T(50202, "Remove");
			}
			if (h.Abilities)
			{
				Character c = Member;
				h.Title = T(50011, "Abilities");
				FillHead(h.Head, c);
				h.AutoLabel = T(50453, "Auto-Battle Command");
				h.CommandsLabel = T(50450, "Battle Commands");
				int[] slots = Ff4Augments.Slots(c);
				// The auto-battle command: libff4 keeps it in the member's own list (abilityIDList 5), Attack from the start; Attack
				// here until that list is kept.
				int auto = Array.IndexOf(slots, 1) >= 0 ? 1 : slots[0];
				h.Auto.Name = CommandName(auto);
				h.Auto.Lit = _cursor == 0;
				for (int k = 0; k < 5; k++)
				{
					h.Slot[k].Name = CommandName(slots[k]);
					h.Slot[k].Lit = _cursor == k + 1;
					h.Slot[k].Picked = _swapFrom == k;
				}
				int lit = _cursor == 0 ? auto : slots[_cursor - 1];
				h.Help = lit > 0 ? (tables?.AbilityHelp(lit) ?? "").Replace("\n", " ") : "";
			}
			Ff4MenuHud.Draw(h);
		}

		/// <summary>The member's eleven figures in MenuLayout_Status's order: the five attributes, then attack, accuracy, defence,
		/// evasion and the magic pair.</summary>
		private static void FillFigures(Ff4MenuHud.Data h, Character c)
		{
			GameTables tables = Ff4Party.Tables;
			OpenFF.Data.Stats st = c.StatsWith(tables);
			int weapon = Ff4Battle.Weapon(c, tables);
			(uint text, int value)[] figures =
			{
				(50420, st.Strength), (50421, st.Agility), (50422, st.Vitality), (50423, st.Intellect), (50424, st.Spirit),
				(50425, Math.Max(1, weapon > 0 ? weapon : st.Strength / 2)), (50426, weapon > 0 ? Ff4Battle.WeaponHit(c, tables) : 90),
				(50427, Ff4Battle.Armour(c, tables)), (50428, Ff4Battle.Evasion(c, tables)), (50429, Ff4Battle.MagicArmour(c, tables)), (50430, 0),
			};
			for (int k = 0; k < h.Stat.Count; k++)
			{
				h.Stat[k].Label = T(figures[k].text);
				h.Stat[k].Value = figures[k].value.ToString();
			}
		}

		/// <summary>What in the bag fits <paramref name="c"/>'s slot, in the bag's order.</summary>
		private static List<int> Candidates(Character c, int slot)
		{
			List<int> fits = new List<int>();
			foreach (OpenFF.Data.ItemStack s in Ff4Party.Party.Inventory)
			{
				if (Fits(Ff4Party.Tables?.Item(s.ItemId), c, slot)) fits.Add(s.ItemId);
			}
			return fits;
		}

		/// <summary>How good a piece is for its slot (Optimize): a weapon's attack, armour's defence and magic defence.</summary>
		private static int Worth(int id)
		{
			EquipStats e = id != 0 ? Ff4Party.Tables?.Item(id)?.Equip : null;
			return e == null ? -1 : Ff4Party.Tables.Item(id).Kind == ItemKind.Weapon ? e.Attack : e.Defence + e.MagicDefence;
		}

		private static void FillHead(Ff4MenuHud.MemberRow head, Character c)
		{
			head.Present = true;
			head.Name = c.Name;
			head.Level = c.Level.ToString();
			head.Hp = c.Hp.ToString();
			head.MaxHp = c.MaxHp.ToString();
			head.Mp = c.Mp.ToString();
			head.MaxMp = c.MaxMp.ToString();
			head.Face = c.Id;
			head.Low = c.Alive && c.Hp * 4 <= c.MaxHp;
		}

		/// <summary>What <paramref name="c"/> wears into the five slot rows; <paramref name="lit"/> the slot the hand is on.</summary>
		private static void FillSlots(List<Ff4MenuHud.SlotRow> rows, Character c, int lit)
		{
			for (int i = 0; i < rows.Count; i++)
			{
				int id = c.Equipment[i];
				ItemDefinition item = id != 0 ? Ff4Party.Tables?.Item(id) : null;
				rows[i].Label = SlotName(i);
				rows[i].Name = id != 0 ? item?.Name ?? ("item " + id) : "";
				rows[i].Icon = item?.Icon ?? -1;
				rows[i].Lit = i == lit;
			}
		}

		/// <summary>The party's five places into <paramref name="rows"/>; <paramref name="lit"/> the member the hand is on.</summary>
		private static void FillMembers(List<Ff4MenuHud.MemberRow> rows, int lit, bool dimNoMagic)
		{
			IReadOnlyList<Character> members = Ff4Party.Party.Members;
			for (int p = 0; p < rows.Count; p++)
			{
				Ff4MenuHud.MemberRow row = rows[p];
				int index = MemberAt(p);
				row.Present = index >= 0;
				row.Lit = index >= 0 && index == lit;
				if (!row.Present) { row.Name = row.Level = row.Hp = row.MaxHp = row.Mp = row.MaxMp = ""; row.Dim = row.Low = false; continue; }
				Character c = members[index];
				row.Name = c.Name;
				row.Level = c.Level.ToString();
				row.Hp = c.Hp.ToString();
				row.MaxHp = c.MaxHp.ToString();
				row.Mp = c.Mp.ToString();
				row.MaxMp = c.MaxMp.ToString();
				row.Face = c.Id;
				row.Low = c.Alive && c.Hp * 4 <= c.MaxHp;
				row.Dim = !c.Alive || (dimNoMagic && c.Spells.Count == 0);
				row.Back = Ff4Party.RowOf(p) == 1;   // the back row's face stands further in
			}
		}

		/// <summary>The member standing in the party's place <paramref name="place"/> (0..4, top down), or -1.</summary>
		private static int MemberAt(int place)
		{
			IReadOnlyList<Character> members = Ff4Party.Party.Members;
			for (int i = 0; i < members.Count; i++)
			{
				int at = Ff4Party.PositionOf(members[i].Id);   // pl::PlayerParty's five positions (Cecil at 1 on a new game)
				if (at == place) return i;
			}
			return -1;
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
				Centred(d, _commands[i].Name ?? T(_commands[i].Text), ColX + ColW / 2, y + 20, _commands[i].Later ? Dim : Color.White, 18);
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

		/// <summary>The place, as the root shows it (WSMenu::wsmGetSavePointIndex): babil_savepoint.bbd's name for the map; on the
		/// world map, the name its own plate last showed (its areas' tables are compiled into the game), else the map's id.</summary>
		private static string PlaceName()
		{
			string map = Game.Field.Map ?? "";
			if (!map.StartsWith("f", StringComparison.OrdinalIgnoreCase) && SavePointMessage(map) is uint message) return T(message);
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

		private static List<(string Map, uint Message)> _savePoints;

		/// <summary>WorldSavePointManager::findSavePoint: babil_savepoint.bbd's 16-byte records (a map name of 12 bytes, a message
		/// of babil_menu.msd at 12). A town's map by its first three letters ("t00"), a dungeon's by its whole name and then
		/// those; anything unmatched the first record's ("not"). (After the game is cleared, flag 0x3db, the "clear" record's.)</summary>
		private static uint? SavePointMessage(string map)
		{
			if (_savePoints == null)
			{
				_savePoints = new List<(string, uint)>();
				byte[] data = Ff4Ui.ReadFile("babil_savepoint.bbd");
				for (int at = 0; data != null && at + 16 <= data.Length; at += 16)
				{
					int end = Array.IndexOf(data, (byte)0, at, 12);
					string name = System.Text.Encoding.ASCII.GetString(data, at, (end < 0 ? at + 12 : end) - at);
					_savePoints.Add((name, BitConverter.ToUInt32(data, at + 12)));
				}
			}
			if (_savePoints.Count == 0) return null;
			uint? Find(string name)
			{
				foreach ((string m, uint id) in _savePoints) if (m == name) return id == 0xFFFFFFFF ? null : id;
				return null;
			}
			string prefix = map.Length >= 3 ? map.Substring(0, 3) : map;
			uint? found = map.StartsWith("d", StringComparison.Ordinal) ? Find(map) ?? Find(prefix) : map.StartsWith("t", StringComparison.Ordinal) ? Find(prefix) : null;
			return found ?? _savePoints[0].Message;
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
