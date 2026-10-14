// The shop as Steam's PC game has it (bootShop's screen, measured off its draws at Baron): it opens on Buy - the row's wares
// at their buying price - and C turns between Buy and Sell (the bag but its key items, at the selling price). The help
// line is the lit item's own (babil_item.msd: "Attack: 12   Element: Lightning", or what it does); beside the gil, how many
// of it the bag holds and how many the party wears (not for what cannot be worn). Enter on a row asks how many: "x n"
// between two arrows in the row - Up / Down by one (round at the ends), Right / Left by ten - and Enter again buys or sells
// at once; one that cannot be paid for, or that the bag cannot hold, is refused with the buzzer. Back leaves the count, or
// the shop. Ff4Shop keeps the rows and the prices; the script that called bootShop holds until the shop closes.

using System;
using System.Collections.Generic;
using OpenFF.Data;

namespace OpenFF.Client
{
	internal sealed partial class Ff4Menu
	{
		private const int ShopRows = 6;
		private Ff4Shop.Stock _shop;
		private bool _selling, _counting;
		private int _count = 1;
		private readonly List<int> _sellable = new List<int>();

		/// <summary>The shop's screen is up.</summary>
		internal bool ShopOpen => _open && _screen == Screen.Shop && _shop != null;

		/// <summary>bootShop: the shop's screen over the field, on Buy.</summary>
		internal void OpenShop(Ff4Shop.Stock stock)
		{
			_shop = stock;
			_selling = _counting = false;
			_count = 1;
			_open = true;
			_mode = Mode.Browse;
			_question = false;
			_fromTitle = _fromEnding = false;
			Game.Input.Capture = true;
			OpenScreen(Screen.Shop);
		}

		private List<int> ShopWares()
		{
			if (!_selling) return _shop.Items;
			_sellable.Clear();
			foreach (OpenFF.Data.ItemStack s in Ff4Party.Party.Inventory)
			{
				ItemDefinition item = Ff4Party.Tables?.Item(s.ItemId);
				if (s.Count > 0 && item != null && item.Kind != ItemKind.KeyItem && Ff4Shop.SellPriceOf(item) > 0) _sellable.Add(s.ItemId);
			}
			return _sellable;
		}

		/// <summary>The most of a ware the count can go to: what the gil pays for and the bag holds (99 at most), or how many
		/// the bag has to sell.</summary>
		private int ShopMost(int itemId)
		{
			Party party = Ff4Party.Party;
			if (_selling) return party.CountItem(itemId);
			int price = Ff4Shop.PriceOf(Ff4Party.Tables?.Item(itemId));
			int afford = price > 0 ? party.Gil / price : 99;
			return Math.Max(0, Math.Min(99 - party.CountItem(itemId), afford));
		}

		private void UpdateShop(InputState input)
		{
			List<int> wares = ShopWares();
			if (_counting)
			{
				if (_cursor >= wares.Count) { _counting = false; return; }
				int most = Math.Max(1, ShopMost(wares[_cursor]));
				if (input.Pressed(Pad.Up)) _count = _count >= most ? 1 : _count + 1;
				if (input.Pressed(Pad.Down)) _count = _count <= 1 ? most : _count - 1;
				if (input.Pressed(Pad.Right)) _count = Math.Min(most, _count + 10);
				if (input.Pressed(Pad.Left)) _count = Math.Max(1, _count - 10);
				if (input.Pressed(Pad.B)) { _counting = false; return; }
				if (input.Pressed(Pad.A))
				{
					bool done = _selling ? Ff4Shop.Instance.Sell(wares[_cursor], _count) : Ff4Shop.Instance.Buy(wares[_cursor], _count);
					if (!done) Game.Audio.PlaySe(0, 0);
					_counting = false;
					List<int> now = ShopWares();
					_cursor = Math.Max(0, Math.Min(_cursor, now.Count - 1));
					if (_scroll > Math.Max(0, now.Count - ShopRows)) _scroll = Math.Max(0, now.Count - ShopRows);
				}
				return;
			}
			if (input.Pressed(Pad.B)) { CloseShop(); return; }
			if (input.Pressed(Pad.X)) { _selling = !_selling; _cursor = _scroll = 0; return; }
			int n = wares.Count;
			if (n == 0) return;
			if (input.Pressed(Pad.Up)) _cursor = (_cursor + n - 1) % n;
			if (input.Pressed(Pad.Down)) _cursor = (_cursor + 1) % n;
			if (_cursor < _scroll) _scroll = _cursor;
			if (_cursor >= _scroll + ShopRows) _scroll = _cursor - ShopRows + 1;
			if (input.Pressed(Pad.A))
			{
				// Steam's: a ware the gil cannot pay for, or the bag cannot take, does not open the count.
				if (ShopMost(wares[_cursor]) <= 0) { Game.Audio.PlaySe(0, 0); return; }
				_counting = true;
				_count = 1;
			}
		}

		private void CloseShop()
		{
			_shop = null;
			_counting = false;
			Close();
		}

		private void FillShop(Ff4MenuHud.Data h)
		{
			GameTables tables = Ff4Party.Tables;
			Party party = Ff4Party.Party;
			List<int> wares = ShopWares();
			h.Title = Ff4Shop.Instance?.Text(_shop.TitleId, _shop.Label) ?? _shop.Label;
			h.ShopCounting = _counting;
			for (int k = 0; k < h.ShopRow.Count; k++)
			{
				int i = _scroll + k;
				Ff4MenuHud.CellRow row = h.ShopRow[k];
				row.Present = i < wares.Count;
				if (!row.Present) continue;
				ItemDefinition item = tables?.Item(wares[i]);
				row.Name = item?.Name ?? ("item " + wares[i]);
				// Counting, only the counted row keeps its price (Steam's).
				row.Count = _counting && i != _cursor ? "" : (_selling ? Ff4Shop.SellPriceOf(item) : Ff4Shop.PriceOf(item)).ToString();
				row.Icon = item?.Icon ?? -1;
				row.Lit = i == _cursor && !_counting;
				row.Gold = i == _cursor && _counting;   // the hand stays on the counted row, its band gone
				row.Dim = false;
			}
			h.ShopCountTop = (_cursor - _scroll) * 108f;
			h.ShopCount = _count.ToString();
			Ff4MenuHud.Scroll(h.ShopScroll, wares.Count, ShopRows, _scroll, 729f, true);
			int lit = _cursor < wares.Count ? wares[_cursor] : 0;
			ItemDefinition at = lit != 0 ? tables?.Item(lit) : null;
			h.Help = at?.Caption ?? "";
			h.ShopGil = party.Gil + T(50446, "Gil");
			h.ShopHave = lit != 0 ? party.CountItem(lit).ToString() : "";
			bool wearable = at != null && (at.Kind == ItemKind.Weapon || at.Kind == ItemKind.Armour);
			h.ShopWornShown = wearable;
			int worn = 0;
			if (wearable) foreach (Character c in party.Members) foreach (int e in c.Equipment) if (e == lit) worn++;
			h.ShopWorn = worn.ToString();
			h.InventoryLabel = T(51204, "Inventory");
			h.EquippedLabel = T(51205, "Equipped");
			h.ShopToggleLabel = _selling ? T(51201, "Buy") : T(51202, "Sell");
			FillMembers(h.ShopPlace, -1, false);
			for (int p = 0; p < h.ShopPlace.Count; p++) h.ShopPlace[p].Dim = false;
		}
	}
}
