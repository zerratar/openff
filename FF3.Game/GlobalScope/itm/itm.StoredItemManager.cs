using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using android.content;
using android.text;
using android.widget;
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
	public static partial class itm
	{
		public class StoredItemManager
		{
			public const int ERR_ITEM_ID = -1;

			private PossessionItem[] item_ = new PossessionItem[384];

			public void initialize()
			{
				for (int i = 0; i < 384; i++)
				{
					item_[i].setItemId(-1);
					item_[i].setItemNumber(0);
				}
			}

			public PossessionItem searchItem(short itemId)
			{
				for (int i = 0; i < 384; i++)
				{
					if (item_[i].itemId() == itemId)
					{
						return item_[i];
					}
				}
				return null;
			}

			public void storeItem(short itemId, int number)
			{
				PossessionItem possessionItem = null;
				for (int i = 0; i < 384; i++)
				{
					if (possessionItem == null && item_[i].itemId() <= 0)
					{
						possessionItem = item_[i];
					}
					if (item_[i].itemId() == itemId)
					{
						item_[i].setItemNumber(item_[i].itemNumber() + number);
						return;
					}
				}
				if (possessionItem != null)
				{
					possessionItem.setItemId(itemId);
					possessionItem.setItemNumber(number);
				}
			}

			public PossessionItem item(short i)
			{
				return item_[i];
			}

			public StoredItemManager()
			{
				for (int i = 0; i < item_.Length; i++)
				{
					item_[i] = new PossessionItem();
				}
			}

			public void setDefault()
			{
				for (int i = 0; i < 384; i++)
				{
					item_[i].setDefault();
				}
			}

			public void copy(StoredItemManager src)
			{
				for (int i = 0; i < 384; i++)
				{
					item_[i].copy(src.item_[i]);
				}
			}

			public void parse(ArrayReader reader)
			{
				for (int i = 0; i < 384; i++)
				{
					item_[i].parse(reader);
				}
			}

			public void store(ArrayWriter writer)
			{
				for (int i = 0; i < 384; i++)
				{
					item_[i].store(writer);
				}
			}
		}
	}
}
