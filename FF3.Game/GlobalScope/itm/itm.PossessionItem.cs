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
		public class PossessionItem
		{
			private short itemId_;

			private byte itemNumber_;

			public short itemId()
			{
				return itemId_;
			}

			public void setItemId(short val)
			{
				itemId_ = val;
			}

			public byte itemNumber()
			{
				return itemNumber_;
			}

			public void itemNumber_inc()
			{
				itemNumber_++;
			}

			public void setItemNumber(int val)
			{
				if (val < 0)
				{
					itemNumber_ = 0;
				}
				else if (val > LIMIT_OF_ITEM)
				{
					itemNumber_ = (byte)LIMIT_OF_ITEM;
				}
				else
				{
					itemNumber_ = (byte)val;
				}
				if (itemNumber_ > 0)
				{
					switch (itemId_)
					{
					case 4201:
					case 4202:
					case 4203:
					case 4204:
					case 4205:
					case 4206:
					case 4207:
					case 4208:
						UserInfo.AwardAchievement(8);
						evt.CEventManager.getInstance().FlagMng().set(0u, (uint)(500 + itemId_ - 4201));
						break;
					}
				}
			}

			public void setDefault()
			{
				itemId_ = 0;
				itemNumber_ = 0;
			}

			public void copy(PossessionItem src)
			{
				itemId_ = src.itemId_;
				itemNumber_ = src.itemNumber_;
			}

			public void parse(ArrayReader reader)
			{
				itemId_ = reader.readInt16();
				itemNumber_ = reader.readByte();
			}

			public void store(ArrayWriter writer)
			{
				writer.writeInt16(itemId_);
				writer.writeByte(itemNumber_);
			}
		}
	}
}
