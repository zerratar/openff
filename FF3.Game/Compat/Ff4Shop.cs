// FF4's shops on the unified layer: the engine draws them from the unified party and items.
//
// FF4 opens a shop with bootShop(row, ?): world::WSMove::wsmOpenShop hands the row to the
// menu state, and world::MSSShop::mssInitialize reads record row x 124 of MENU/babil_shop.bbd
// - a 32-byte label ("Baron Weapon"), the shopkeeper's title (51220 Weaponsmith, 51221
// Armorer, 51222 Sundries in babil_menu.msd), six line ids (51260 "How might I be of
// service?", 51261 "Which item?", 51262 "How many?", 51263 "Thank you!", 51264 short on gil,
// 51265 cannot hold more) and up to sixteen item ids. FF4's own shop screen is not ported;
// this service draws one over the field with Game.Draw: Buy (the row's wares at the record's
// buying price), Sell (the bag at the record's selling price), Leave; Left/Right change the
// count, L/R by ten, A confirms, B backs out.
// The script that called bootShop holds until the shop closes. Game.Shops on FF4.

using System;
using System.Collections.Generic;
using OpenFF;
using OpenFF.Data;

namespace FF3
{
	internal sealed class Ff4Shop : GameService, IShops
	{
		private sealed class Row
		{
			public int Index;
			public string Label;
			public int TitleId;
			public int[] LineIds = new int[6];
			public List<int> Items = new List<int>();
		}

		private enum Page { Menu, Buy, Sell }

		private static Ff4Shop _instance;
		public static Ff4Shop Instance => _instance;

		private List<Row> _rows;
		private Dictionary<uint, string> _texts;
		private bool _open;
		private Row _row;
		private Page _page;
		private int _cursor, _scroll, _count = 1;
		private string _notice;
		private int _noticeFrames;
		private readonly List<int> _sellChoices = new List<int>();

		public Ff4Shop()
		{
			_instance = this;
		}

		public override bool WantsUpdate => true;

		public bool IsOpen => _open;

		// ---- the table ----

		private List<Row> Rows
		{
			get
			{
				if (_rows != null) return _rows;
				_rows = new List<Row>();
				try
				{
					if (GameArchive.Chain != null && TableFiles.ReadAny(GameArchive.Chain, "babil_shop.bbd", out byte[] data))
					{
						for (int at = 0; at + 124 <= data.Length; at += 124)
						{
							Row row = new Row { Index = at / 124 };
							int end = Array.IndexOf(data, (byte)0, at);
							row.Label = System.Text.Encoding.ASCII.GetString(data, at, Math.Max(0, Math.Min(32, (end < 0 ? at + 32 : end) - at)));
							row.TitleId = ChainPack.S32(data, at + 32);
							for (int i = 0; i < 6; i++) row.LineIds[i] = ChainPack.S32(data, at + 36 + 4 * i);
							for (int i = 0; i < 16; i++)
							{
								int id = ChainPack.S32(data, at + 60 + 4 * i);
								if (id > 0) row.Items.Add(id);
							}
							_rows.Add(row);
						}
					}
					else Log.Write(LogChannel.General, "shop: babil_shop.bbd not found");
					_texts = TableFiles.ReadNames(GameArchive.Chain, "babil_menu.msd", Ff4Party.Tables);
					Log.Write(LogChannel.File, "shop: " + _rows.Count + " shop rows, " + (_texts?.Count ?? 0) + " menu texts");
				}
				catch (Exception ex)
				{
					Log.Write(LogChannel.General, "shop: table: " + ex.Message);
				}
				return _rows;
			}
		}

		private string Text(int id, string fallback) => _texts != null && id > 0 && _texts.TryGetValue((uint)id, out string s) ? s : fallback;

		// ---- IShops ----

		public bool Open(int index, string table = null)
		{
			if (!EngineApi.InWorld || _open) return false;
			Row row = index >= 0 && index < Rows.Count ? Rows[index] : null;
			if (row == null) { Log.Write(LogChannel.General, "shop: no row " + index); return false; }
			_row = row;
			_open = true;
			_page = Page.Menu;
			_cursor = 0; _scroll = 0; _count = 1;
			Game.Input.Capture = true;
			Log.Write(LogChannel.General, "shop: open row " + index + " '" + row.Label + "' (" + Text(row.TitleId, "shop") + "): " + string.Join(", ", row.Items.ConvertAll(id => Ff4Party.Tables?.Item(id)?.Name ?? id.ToString())));
			return true;
		}

		public ShopInfo Info(int index, string table = null)
		{
			Row row = index >= 0 && index < Rows.Count ? Rows[index] : null;
			if (row == null) return null;
			ShopInfo info = new ShopInfo { Index = index, Kind = row.TitleId == 51220 ? 0 : row.TitleId == 51221 ? 1 : 3 };
			info.ItemIds.AddRange(row.Items);
			return info;
		}

		private void Close()
		{
			_open = false;
			Game.Input.Capture = false;
			Log.Write(LogChannel.File, "shop: closed with " + Ff4Party.Party.Gil + " gil");
		}

		// ---- the frame ----

		public override void OnUpdate()
		{
			if (!_open)
			{
				// A test shop: O opens Baron's weaponsmith (row 1) on any FF4 map.
				if (EngineApi.InWorld && !Game.Input.Capture && !Ff4Cutscene.Active && !Ff4Battle.Active && Game.Input.KeyPressed("O")) Open(1);
				return;
			}
			if (_noticeFrames > 0) _noticeFrames--;
			InputState input = Game.Input;
			switch (_page)
			{
				case Page.Menu:
					if (input.Pressed(Pad.Up)) _cursor = (_cursor + 2) % 3;
					if (input.Pressed(Pad.Down)) _cursor = (_cursor + 1) % 3;
					if (input.Pressed(Pad.B)) { Close(); return; }
					if (input.Pressed(Pad.A))
					{
						if (_cursor == 0) { if (_row.Items.Count == 0) Notice("Nothing for sale."); else { _page = Page.Buy; _cursor = 0; _scroll = 0; _count = 1; } }
						else if (_cursor == 1)
						{
							_sellChoices.Clear();
							foreach (OpenFF.Data.ItemStack s in Ff4Party.Party.Inventory) if (Ff4Party.Tables?.Item(s.ItemId)?.Kind != ItemKind.KeyItem) _sellChoices.Add(s.ItemId);
							if (_sellChoices.Count == 0) Notice("Nothing to sell."); else { _page = Page.Sell; _cursor = 0; _scroll = 0; _count = 1; }
						}
						else Close();
					}
					break;
				case Page.Buy:
					List(input, _row.Items.Count);
					if (input.Pressed(Pad.B)) { _page = Page.Menu; _cursor = 0; break; }
					if (input.Pressed(Pad.A)) Buy(_row.Items[_cursor], _count);
					break;
				case Page.Sell:
					List(input, _sellChoices.Count);
					if (input.Pressed(Pad.B)) { _page = Page.Menu; _cursor = 1; break; }
					if (input.Pressed(Pad.A) && _sellChoices.Count > 0) Sell(_sellChoices[_cursor], _count);
					break;
			}
			if (_open) Draw();
		}

		private void List(InputState input, int count)
		{
			if (count == 0) return;
			int before = _cursor;
			if (input.Pressed(Pad.Up)) _cursor = (_cursor + count - 1) % count;
			if (input.Pressed(Pad.Down)) _cursor = (_cursor + 1) % count;
			if (_cursor != before) _count = 1;
			if (input.Pressed(Pad.Right)) _count = Math.Min(99, _count + 1);
			if (input.Pressed(Pad.Left)) _count = Math.Max(1, _count - 1);
			if (input.Pressed(Pad.R)) _count = Math.Min(99, _count + 10);
			if (input.Pressed(Pad.L)) _count = Math.Max(1, _count - 10);
			if (_cursor < _scroll) _scroll = _cursor;
			if (_cursor >= _scroll + 8) _scroll = _cursor - 7;
		}

		private static int PriceOf(ItemDefinition item) => item == null ? 0 : item.BuyPrice > 0 ? item.BuyPrice : item.SellPrice;
		/// <summary>The record's own selling price (the s32 at 0x20, half the buying price for FF4's wares), else half of what it costs.</summary>
		private static int SellPriceOf(ItemDefinition item) => item == null ? 0 : Math.Max(1, item.SellPrice > 0 && item.SellPrice < item.BuyPrice ? item.SellPrice : PriceOf(item) / 2);

		private void Buy(int itemId, int count)
		{
			Party party = Ff4Party.Party;
			ItemDefinition item = Ff4Party.Tables?.Item(itemId);
			if (item == null) return;
			int price = PriceOf(item) * count;
			if (party.Gil < price) { Notice(Text(_row.LineIds[4], "I'm afraid you're short on gil.")); return; }
			if (party.CountItem(itemId) + count > 99) { Notice(Text(_row.LineIds[5], "You cannot hold any more.")); return; }
			party.Gil -= price;
			party.AddItem(itemId, count);
			EngineHooks.ItemGained(itemId, count);
			Notice(Text(_row.LineIds[3], "Thank you!"));
			Log.Write(LogChannel.File, "shop: bought " + count + " x " + item.Name + " for " + price + " (" + party.Gil + " gil left)");
			_count = 1;
		}

		private void Sell(int itemId, int count)
		{
			Party party = Ff4Party.Party;
			ItemDefinition item = Ff4Party.Tables?.Item(itemId);
			if (item == null) return;
			count = Math.Min(count, party.CountItem(itemId));
			if (count <= 0) return;
			int price = SellPriceOf(item) * count;
			party.RemoveItem(itemId, count);
			party.Gil += price;
			Notice(Text(_row.LineIds[3], "Thank you!"));
			Log.Write(LogChannel.File, "shop: sold " + count + " x " + item.Name + " for " + price + " (" + party.Gil + " gil)");
			_count = 1;
			if (party.CountItem(itemId) == 0)
			{
				_sellChoices.Remove(itemId);
				if (_sellChoices.Count == 0) { _page = Page.Menu; _cursor = 1; }
				else _cursor = Math.Min(_cursor, _sellChoices.Count - 1);
			}
		}

		private void Notice(string text)
		{
			_notice = text;
			_noticeFrames = 120;
		}

		// ---- drawing ----

		private void Draw()
		{
			DrawList d = Game.Draw;
			Color panel = new Color(16, 24, 72, 235);
			Color frame = new Color(230, 230, 240);
			Color dim = new Color(170, 175, 200);
			d.Rect(0, 0, 800, 480, new Color(0, 0, 0, 90));
			// The keeper's line, top.
			d.Rect(40, 20, 720, 56, panel);
			d.Rect(40, 20, 720, 56, frame, false);
			string line = _noticeFrames > 0 && _notice != null ? _notice : _page == Page.Menu ? Text(_row.LineIds[0], "How might I be of service?") : Text(_row.LineIds[1], "Which item?");
			d.Text(Text(_row.TitleId, _row.Label), 56, 28, Color.Yellow, 13);
			d.Text(line, 56, 46, Color.White, 15);
			// Gil, top right.
			string gil = Ff4Party.Party.Gil + " gil";
			d.Text(gil, 740 - d.MeasureText(gil, 14), 30, Color.Yellow, 14);
			if (_page == Page.Menu)
			{
				d.Rect(40, 90, 200, 110, panel);
				d.Rect(40, 90, 200, 110, frame, false);
				string[] names = { "Buy", "Sell", "Leave" };
				for (int i = 0; i < 3; i++) d.Text((i == _cursor ? "> " : "  ") + names[i], 56, 102 + 30 * i, i == _cursor ? Color.Yellow : Color.White, 16);
				return;
			}
			List<int> items = _page == Page.Buy ? _row.Items : _sellChoices;
			d.Rect(40, 90, 720, 330, panel);
			d.Rect(40, 90, 720, 330, frame, false);
			float y = 100;
			for (int i = _scroll; i < items.Count && i < _scroll + 8; i++)
			{
				ItemDefinition item = Ff4Party.Tables?.Item(items[i]);
				bool on = i == _cursor;
				int price = _page == Page.Buy ? PriceOf(item) : SellPriceOf(item);
				if (on) d.Rect(50, y - 3, 700, 24, new Color(255, 255, 255, 28));
				d.Text((on ? "> " : "  ") + (item?.Name ?? ("item " + items[i])), 56, y, on ? Color.Yellow : Color.White, 14);
				d.Text(price + " gil", 420, y + 1, dim, 13);
				d.Text("have " + Ff4Party.Party.CountItem(items[i]), 540, y + 1, dim, 12);
				y += 28;
			}
			ItemDefinition picked = items.Count > 0 ? Ff4Party.Tables?.Item(items[_cursor]) : null;
			if (picked != null)
			{
				float ty = 340;
				if (!string.IsNullOrEmpty(picked.Caption)) { d.Text(picked.Caption, 56, ty, Color.White, 12); ty += 20; }
				if (picked.Equip != null)
				{
					d.Text(picked.Kind == ItemKind.Weapon ? "Attack " + picked.Equip.Attack + "   Hit " + picked.Equip.Hit : "Defence " + picked.Equip.Defence + "   Magic defence " + picked.Equip.MagicDefence + "   Evade " + picked.Equip.Evade, 56, ty, dim, 12);
					ty += 20;
				}
				int unit = _page == Page.Buy ? PriceOf(picked) : SellPriceOf(picked);
				d.Text("x" + _count + "  =  " + unit * _count + " gil     Left/Right: count   A: " + (_page == Page.Buy ? "buy" : "sell") + "   B: back", 56, 396, Color.Yellow, 13);
			}
		}

		public override IEnumerable<string> DebugLines()
		{
			if (_open) yield return "OpenFF shop: " + _row.Label + " (" + _page + ")";
		}
	}
}
