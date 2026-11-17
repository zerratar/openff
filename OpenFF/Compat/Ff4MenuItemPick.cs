// The event's item window (menu::ItemWindow, WSCEvent's, opened by setItemUseMenuManager and read by checkUseItem_Id): a
// key item used at a spot - the Tower of Babil's doors, the crystal rooms, the Sealed Cave. iwCreateList with the event's
// filter 0x27 lists the bag (items, weapons, armour) and then the key items; Enter decides the lit one (SE 1), Back puts it
// away with none (-1, SE 2). Drawn as the menu's Inventory, both lists in one; Steam's own window is not compared yet (it
// only opens inside such an event).

using System;
using System.Collections.Generic;
using OpenFF.Data;

namespace OpenFF.Client
{
	internal sealed partial class Ff4Menu
	{
		private bool _eventPick;

		/// <summary>The item the event's window decided (ItemWindow's +0x390), -1 for none or put away.</summary>
		internal static int EventPickedItem { get; private set; } = -1;

		/// <summary>Whether the event's item window is up.</summary>
		internal bool EventPickOpen => _open && _eventPick;

		/// <summary>setItemUseMenuManager: the item window over the event (iwOpen).</summary>
		internal void OpenEventPick()
		{
			_open = true;
			_fromTitle = false;
			_eventPick = true;
			_keyItems = false;
			_mode = Mode.Browse;
			_cursor = _scroll = 0;
			_help = HelpView.None;
			_question = false;
			_sortNote = null;
			_lookAtOpen = null;
			EventPickedItem = -1;
			Game.Input.Capture = true;
			_screen = Screen.Inventory;
			Log.Write(LogChannel.File, "menu: the event's item window");
		}

		/// <summary>iwCreateList(0x27): the bag without its key items, then the key items.</summary>
		private List<OpenFF.Data.ItemStack> EventPickList()
		{
			List<OpenFF.Data.ItemStack> list = new List<OpenFF.Data.ItemStack>(), keys = new List<OpenFF.Data.ItemStack>();
			foreach (OpenFF.Data.ItemStack s in Ff4Party.Party.Inventory)
			{
				(Ff4Party.Tables?.Item(s.ItemId)?.Kind == ItemKind.KeyItem ? keys : list).Add(s);
			}
			list.AddRange(keys);
			return list;
		}

		/// <summary>iwExecute's decide and cancel: the item (or none) for checkUseItem_Id, the window put away.</summary>
		private void EndEventPick(int item)
		{
			EventPickedItem = item;
			Game.Audio.PlaySe(0, item < 0 ? 2 : 1);
			Log.Write(LogChannel.File, "menu: the event's item window - " + (item < 0 ? "none" : (Ff4Party.Tables?.Item(item)?.Name ?? item.ToString())));
			Close();
		}
	}

	/// <summary>The event commands of the item window.</summary>
	internal static class Ff4ItemPickCommands
	{
		private static int _state;

		/// <summary>setItemUseMenuManager(): opens the window and holds the script until it is put away.</summary>
		public static void SetItemUseMenuManager(GlobalScope.ScriptEngine engine)
		{
			Ff4Menu menu = Ff4Menu.Instance;
			if (menu == null) return;
			if (_state == 0)
			{
				menu.OpenEventPick();
				_state = 1;
				engine.suspendRedo();
				return;
			}
			if (menu.EventPickOpen) { engine.suspendRedo(); return; }
			_state = 0;
		}

		/// <summary>checkUseItem_Id(item, label): jumps when the window decided that item.</summary>
		public static void CheckUseItemId(GlobalScope.ScriptEngine engine)
		{
			int item = (int)engine.getWord();
			uint label = engine.getDword();
			if (Ff4Menu.EventPickedItem != -1 && Ff4Menu.EventPickedItem == item) engine.jump(label);
		}
	}
}
