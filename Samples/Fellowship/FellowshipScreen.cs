// The Fellowship screens: who is travelling (main menu ▸ Fellowship), what to do with one of
// them, and the two gifts - an item out of the bag, a sum of gil. Four layouts in menus/, four
// MenuBehaviours here; the lists are MenuLists over row frames.
//
// The layouts follow the game's own screens: the 480 x 288 canvas above the bottom bar,
// windows 4 px in from the edges, rows 24 px apart (36 where a row carries a second, smaller
// line), the hand cursor standing clear of the words (<align>menu</align>).

using System;
using System.Collections.Generic;
using System.Linq;
using OpenFF;

namespace Fellowship
{
	/// <summary>The travellers, a row each; a press opens the actions for one.</summary>
	public sealed class FellowshipScreen : MenuBehaviour
	{
		/// <summary>The traveller picked, for the screens after.</summary>
		public static string PickedId;
		private const int Rows = 4;
		private MenuList _list;
		private List<Traveller> _shown = new List<Traveller>();

		public override void OnOpen()
		{
			_list = new MenuList(Menu, "row", Rows, "page") { SubPrefix = "sub" };
			Fill();
			Menu.Focus("row0");
		}

		private void Fill()
		{
			FellowshipService s = FellowshipService.Instance;
			Menu.SetText("me", s == null ? "" : s.MyName + (s.AidReady ? "" : "   (aid resting)"));
			_shown = s?.Travellers.ToList() ?? new List<Traveller>();
			Menu.SetText("none", _shown.Count == 0 ? "No one else is travelling. Another OpenFF with this mod on your network appears here." : "");
			_list.Items = _shown.Select(t => t.Name + "   " + (t.Map == Game.Field.Map ? "here" : t.Where)).ToList();
			_list.SubItems = _shown.Select(t => t.Standing + "   Lv. " + t.Level + " " + t.Job + "   HP " + t.Hp + " / " + t.MaxHp).ToList();
			_list.ColourOf = i => _shown[i].State == 1 ? MenuColour.Red : _shown[i].Map == Game.Field.Map ? MenuColour.White : MenuColour.PaleBlue;
			_list.Show();
			Describe();
		}

		private void Describe()
		{
			int i = _list.Selected;
			if (i < 0 || i >= _shown.Count) { Menu.SetText("desc1", "F5..F8 speak to everyone; F9 hides the corner list."); Menu.SetText("desc2", "A traveller can be sent aid, given items or gil, and travelled to."); return; }
			Traveller t = _shown[i];
			Menu.SetText("desc1", t.Name + " is " + t.Standing + " on " + t.Where + (t.Said != null ? "   last said: \"" + t.Said + "\"" : ""));
			Menu.SetText("desc2", "A: travel to them, send aid, give an item or gil.");
		}

		public override void OnTick()
		{
			// The travellers move and speak while the screen is up: refresh every half second.
			if (Game.Time.Frame % 30 == 0) Fill();
		}

		public override void OnFocus() { _list.OnFocus(); Describe(); }
		public override bool OnKey(MenuKey key) { if (!_list.OnKey(key)) return false; Describe(); return true; }

		public override bool OnPress()
		{
			int i = _list.IndexAt(Menu.Focused);
			if (i < 0 || i >= _shown.Count) { Menu.SoundBeep(); return true; }
			PickedId = _shown[i].Id;
			Menu.SoundDecide();
			Menu.Open("fellowship-actions");
			return true;
		}

		/// <summary>The traveller the screens are about, or null once they are gone.</summary>
		internal static Traveller Picked => FellowshipService.Instance?.Travellers.FirstOrDefault(t => t.Id == PickedId);
	}

	/// <summary>One traveller: travel to them, send aid, give an item, give gil.</summary>
	public sealed class TravellerActions : MenuBehaviour
	{
		public override void OnOpen()
		{
			Traveller t = FellowshipScreen.Picked;
			Menu.SetText("title", t == null ? "Gone" : t.Name);
			Menu.SetText("about", t == null ? "That traveller is no longer heard from." : t.Name + " is " + t.Standing + " on " + t.Where + ".");
			Menu.SetText("about2", t == null ? "" : "Lv. " + t.Level + " " + t.Job + "   HP " + t.Hp + " / " + t.MaxHp);
			Describe();
			Menu.Focus("travel");
		}

		private void Describe()
		{
			FellowshipService s = FellowshipService.Instance;
			switch (Menu.Focused)
			{
				case "travel": Menu.SetText("desc1", "Warp to their map and stand beside them."); Menu.SetText("desc2", ""); break;
				case "aid": Menu.SetText("desc1", "Their party heals a quarter of its HP, at once - in battle or out of it."); Menu.SetText("desc2", s != null && s.AidReady ? "Ready." : "Resting: once every thirty seconds."); break;
				case "give": Menu.SetText("desc1", "Items from your bag into theirs."); Menu.SetText("desc2", "Pick the item, then how many."); break;
				case "gil": Menu.SetText("desc1", "Gil from your purse into theirs."); Menu.SetText("desc2", "You have " + Game.Party.Gil.ToString("N0", System.Globalization.CultureInfo.InvariantCulture) + " gil."); break;
				default: Menu.SetText("desc1", ""); Menu.SetText("desc2", ""); break;
			}
		}

		public override void OnFocus() => Describe();

		public override bool OnPress()
		{
			FellowshipService s = FellowshipService.Instance;
			Traveller t = FellowshipScreen.Picked;
			if (s == null || t == null) { Menu.SoundBeep(); return true; }
			switch (Menu.Focused)
			{
				case "travel":
					if (!s.TravelTo(t)) { Menu.SoundBeep(); return true; }
					Menu.SoundDecide(); Menu.Close(); return true;
				case "aid":
					if (s.SendAid(t)) Menu.SoundDecide(); else Menu.SoundBeep();
					Describe(); return true;
				case "give": Menu.SoundDecide(); Menu.Open("fellowship-gift"); return true;
				case "gil": Menu.SoundDecide(); Menu.Open("fellowship-gil"); return true;
				default: return false;
			}
		}

		public override bool OnCancel() { Menu.SoundCancel(); Menu.Open("fellowship"); return true; }
	}

	/// <summary>The bag's items for a traveller: pick one, left / right for how many, a press sends them.</summary>
	public sealed class GiftScreen : MenuBehaviour
	{
		private const int Rows = 7;
		private MenuList _list;
		private List<ItemStack> _bag = new List<ItemStack>();
		private int _count = 1;
		private int _lastIndex = -1;

		public override void OnOpen()
		{
			Traveller t = FellowshipScreen.Picked;
			Menu.SetText("title", t == null ? "Gone" : "Give to " + t.Name);
			_list = new MenuList(Menu, "item", Rows, "page") { SubPrefix = "cnt" };
			Fill();
			Menu.Focus("item0");
		}

		private void Fill()
		{
			// Everything but the key items, the bag's own order.
			_bag = Game.Party.Items.Where(s => s.Count > 0 && Game.Items.Find(s.ItemId)?.Category != ItemCategory.Key).ToList();
			Menu.SetText("none", _bag.Count == 0 ? "Nothing in the bag to give." : "");
			_list.Items = _bag.Select(s => Game.Items.Find(s.ItemId)?.Name ?? ("Item " + s.ItemId)).ToList();
			_list.SubItems = _bag.Select(s => s.Count.ToString()).ToList();
			_list.Show();
			Describe();
		}

		private void Describe()
		{
			int i = _list.Selected;
			if (i != _lastIndex) { _count = 1; _lastIndex = i; }
			if (i < 0 || i >= _bag.Count) { Menu.SetText("desc1", ""); Menu.SetText("desc2", ""); return; }
			Item item = Game.Items.Find(_bag[i].ItemId);
			Menu.SetText("desc1", item?.Caption ?? "");
			Menu.SetText("desc2", "Give " + FellowshipService.Counted(item?.Name ?? "item", _count) + "   (left / right: how many, of " + _bag[i].Count + ")");
		}

		public override void OnFocus() { _list.OnFocus(); Describe(); }

		public override bool OnKey(MenuKey key)
		{
			if (_list.OnKey(key)) { Describe(); return true; }
			int i = _list.Selected;
			if (i < 0 || (key != MenuKey.Left && key != MenuKey.Right)) return false;
			int was = _count;
			_count = Math.Clamp(_count + (key == MenuKey.Right ? 1 : -1), 1, _bag[i].Count);
			if (_count != was) Menu.SoundDecide(); else Menu.SoundBeep();
			Describe();
			return true;
		}

		public override bool OnPress()
		{
			FellowshipService s = FellowshipService.Instance;
			Traveller t = FellowshipScreen.Picked;
			int i = _list.Selected;
			if (s == null || t == null || i < 0 || !s.Give(t, _bag[i].ItemId, _count)) { Menu.SoundBeep(); return true; }
			Menu.SoundDecide();
			_lastIndex = -1;
			Fill();   // the bag has changed; the cursor stays where it was, or on the last item
			_list.OnFocus();
			Describe();
			return true;
		}

		public override bool OnCancel() { Menu.SoundCancel(); Menu.Open("fellowship-actions"); return true; }
	}

	/// <summary>A sum of gil for a traveller.</summary>
	public sealed class GilScreen : MenuBehaviour
	{
		private static readonly int[] Sums = { 100, 500, 1000, 5000 };

		public override void OnOpen()
		{
			Traveller t = FellowshipScreen.Picked;
			Menu.SetText("title", t == null ? "Gone" : "Gil for " + t.Name);
			Fill();
			Menu.Focus("gil100");
		}

		private void Fill()
		{
			Menu.SetText("about", "You have " + Game.Party.Gil.ToString("N0", System.Globalization.CultureInfo.InvariantCulture) + " gil.");
			foreach (int sum in Sums)
			{
				IMenuWidget w = Menu.Widget("gil" + sum);
				if (w != null) w.Colour = Game.Party.Gil >= sum ? MenuColour.White : MenuColour.Disabled;   // what the purse cannot cover is greyed
			}
			Describe();
		}

		private int Focused => int.TryParse((Menu.Focused ?? "").Replace("gil", ""), out int v) ? v : 0;

		private void Describe()
		{
			int sum = Focused;
			Traveller t = FellowshipScreen.Picked;
			if (sum == 0) { Menu.SetText("desc1", ""); Menu.SetText("desc2", ""); return; }
			Menu.SetText("desc1", sum.ToString("N0", System.Globalization.CultureInfo.InvariantCulture) + " gil to " + (t?.Name ?? "them") + ".");
			Menu.SetText("desc2", Game.Party.Gil >= sum ? "" : "More than you have.");
		}

		public override void OnFocus() => Describe();

		public override bool OnPress()
		{
			FellowshipService s = FellowshipService.Instance;
			Traveller t = FellowshipScreen.Picked;
			int sum = Focused;
			if (sum == 0) return false;
			if (s == null || t == null || !s.GiveGil(t, sum)) { Menu.SoundBeep(); return true; }
			Menu.SoundDecide();
			Fill();
			return true;
		}

		public override bool OnCancel() { Menu.SoundCancel(); Menu.Open("fellowship-actions"); return true; }
	}
}
