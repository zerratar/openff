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
using java.io;
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
	public static partial class itm
	{
		public class PossessionItemManager
		{
			public const int ERR_ITEM_ID = -1;

			public const int NORMAL_ITEM = 0;

			public const int IMPORTANT_ITEM = 1;

			private PossessionItem[] normalItem_ = new PossessionItem[384];

			private PossessionItem[] importantItem_ = new PossessionItem[64];

			public PossessionItemManager()
			{
				for (int i = 0; i < normalItem_.Length; i++)
				{
					normalItem_[i] = new PossessionItem();
				}
				for (int i = 0; i < importantItem_.Length; i++)
				{
					importantItem_[i] = new PossessionItem();
				}
			}

			public void initialize()
			{
				for (int i = 0; i < 384; i++)
				{
					normalItem_[i].setItemId(-1);
					normalItem_[i].setItemNumber(0);
				}
				for (int i = 0; i < 64; i++)
				{
					importantItem_[i].setItemId(-1);
					importantItem_[i].setItemNumber(0);
				}
			}

			public PossessionItem serchNormalItem(short itemId)
			{
				for (int i = 0; i < 384; i++)
				{
					if (normalItem(i).itemId() == itemId)
					{
						return normalItem(i);
					}
				}
				return null;
			}

			public void storeItem(short itemId, int number)
			{
				PossessionItem possessionItem = null;
				for (int i = 0; i < 384; i++)
				{
					if (possessionItem == null && normalItem_[i].itemId() <= 0)
					{
						possessionItem = normalItem_[i];
					}
					if (normalItem_[i].itemId() == itemId)
					{
						normalItem_[i].setItemNumber(normalItem_[i].itemNumber() + number);
						return;
					}
				}
				if (possessionItem != null)
				{
					possessionItem.setItemId(itemId);
					possessionItem.setItemNumber(number);
				}
			}

			public void resetItemId()
			{
				for (int i = 0; i < 384; i++)
				{
					if (normalItem(i).itemNumber() == 0 && normalItem(i).itemId() > 0)
					{
						normalItem(i).setItemId(-1);
					}
				}
			}

			public void resetImportantItemId()
			{
				for (int i = 0; i < 64; i++)
				{
					if (importantItem(i).itemNumber() == 0 && importantItem(i).itemId() > 0)
					{
						importantItem(i).setItemId(-1);
					}
				}
			}

			public PossessionItem normalItem(int i)
			{
				return normalItem_[i];
			}

			public void normalItem_set(int i, PossessionItem arg0)
			{
				normalItem_[i] = arg0;
			}

			public PossessionItem importantItem(int i)
			{
				return importantItem_[i];
			}

			public PossessionItem allItem(int i)
			{
				if (i < 64)
				{
					return importantItem_[i];
				}
				return normalItem_[i - 64];
			}

			public void setDefault()
			{
				for (int i = 0; i < 384; i++)
				{
					normalItem_[i].setDefault();
				}
				for (int i = 0; i < 64; i++)
				{
					importantItem_[i].setDefault();
				}
			}

			public void copy(PossessionItemManager src)
			{
				for (int i = 0; i < 384; i++)
				{
					normalItem_[i].copy(src.normalItem_[i]);
				}
				for (int i = 0; i < 64; i++)
				{
					importantItem_[i].copy(src.importantItem_[i]);
				}
			}

			public void parse(ArrayReader reader)
			{
				for (int i = 0; i < 384; i++)
				{
					normalItem_[i].parse(reader);
				}
				for (int i = 0; i < 64; i++)
				{
					importantItem_[i].parse(reader);
				}
			}

			public void store(ArrayWriter writer)
			{
				for (int i = 0; i < 384; i++)
				{
					normalItem_[i].store(writer);
				}
				for (int i = 0; i < 64; i++)
				{
					importantItem_[i].store(writer);
				}
			}
		}
	}
}
