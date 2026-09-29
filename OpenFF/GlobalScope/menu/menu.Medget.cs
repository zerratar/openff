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
using OpenFF.Platform;
using OpenFF.Resources;

internal static partial class GlobalScope
{
	public static partial class menu
	{
		public class Medget : dgs.DGSLinkedList<Medget>
		{
			public class WORK_SPACE
			{
				// PORT: work is only ever a number (a layout's <work>, or set by code), so it is kept as one: the readers' casts to
				// sbyte, byte, short or an enum are conversions, where unboxing an object threw unless it held exactly that type
				// (Config's volume slider cast a layout's sbyte to int - issue #1). work1..3 hold a number or a widget: work1<T>().
				public int work_;

				public object work1_;

				public object work2_;

				public object work3_;
			}

			public Medget prevSibling_;

			public Medget nextSibling_;

			public Medget parentNode_;

			public Medget childNode_;

			public XbnNode thisNode_;

			public string id_;

			public WORK_SPACE space = new WORK_SPACE();

			public short x_;

			public short y_;

			public short width_;

			public short height_;

			public sbyte display_;

			public sbyte myTag_;

			public string up_;

			public string down_;

			public string left_;

			public string right_;

			public MenuBehavior behavior_;

			public static void allocatePool(int num)
			{
			}

			public static void allocatePool(ds.Vector<Medget, ds.FastErasePolicy<Medget>> pool)
			{
			}

			public static void freePool()
			{
			}

			public Medget()
			{
				clear();
				dgsllLink();
			}

			public Medget(Medget M)
			{
			}

			~Medget()
			{
				destruct();
			}

			public new void destruct()
			{
				dgsllUnlink();
				if (behavior_ != null)
				{
					behavior_.mbDelete();
					behavior_ = null;
				}
			}

			public void clear()
			{
				prevSibling_ = null;
				nextSibling_ = null;
				parentNode_ = null;
				childNode_ = null;
				behavior_ = null;
				thisNode_ = null;
				id_ = default_id;
				x_ = 0;
				y_ = 0;
				width_ = 0;
				height_ = 0;
				space.work_ = 0;
				space.work1_ = (space.work2_ = 0);
				display_ = 0;
				myTag_ = 0;
				up_ = (down_ = (left_ = (right_ = default_id)));
			}

			public bool _id(string _id)
			{
				return IdEquals(id_, _id);
			}

			/// <summary>
			/// PORT: frame ids compare without regard to case. Steam's MenuDefine names the
			/// quicksave dialog's answers YES and NO where the phone's (and the code that
			/// listens for them) say yes and no; no menu has two ids that differ only in case.
			/// </summary>
			public static bool IdEquals(string a, string b)
			{
				return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
			}

			public void setPosX(short posX)
			{
				x_ = posX;
			}

			public void setPosition(short x, short y)
			{
				short num = (short)(x - x_);
				short num2 = (short)(y - y_);
				x_ = x;
				y_ = y;
				if (behavior_ != null)
				{
					behavior_.mbSetPosition(x, y);
				}
				for (Medget medget = childNode(); medget != null; medget = medget.nextSibling())
				{
					short num3 = (short)(medget.x() + num);
					short num4 = (short)(medget.y() + num2);
					medget.setPosition(num3, num4);
				}
			}

			public Medget getNodeByIDFromChildren(string _id)
			{
				for (Medget medget = childNode(); medget != null; medget = medget.nextSibling())
				{
					if (IdEquals(medget._id(), _id))
					{
						return medget;
					}
				}
				return null;
			}

			public Medget getNodeByID(string _id)
			{
				Medget medget = null;
				for (Medget medget2 = childNode(); medget2 != null; medget2 = medget2.nextSibling())
				{
					if (IdEquals(medget2._id(), _id))
					{
						return medget2;
					}
					if ((medget = medget2.getNodeByID(_id)) != null)
					{
						return medget;
					}
				}
				return null;
			}

			public void setPriority(int priority)
			{
				if (behavior_ != null)
				{
					behavior_.bmSetPriority(priority);
				}
				for (Medget medget = childNode(); medget != null; medget = medget.nextSibling())
				{
					medget.setPriority(priority);
				}
			}

			public int cursorX()
			{
				if (cursorFrame() is Medget at) return at.x_;
				return x_ + ((behavior_ != null) ? behavior_.bmGetCursorX(this) : 0);
			}

			/// <summary>PORT: whether a press at (px, py) is on the frame: inside it, or inside a frame of it (not the hand's own &lt;cursor/&gt; mark).</summary>
			public bool hitTest(int px, int py)
			{
				if (x_ < px && px <= x_ + width_ && y_ < py && py <= y_ + height_) return true;
				for (Medget m = childNode(); m != null; m = m.nextSibling())
				{
					if (m.node()?.getFirstNodeByTagNameFromChildren("cursor") != null) continue;
					if (m.hitTest(px, py)) return true;
				}
				return false;
			}

			// PORT: a frame of a layout's marked <cursor/> inside this one says where the hand stands on it (OpenFF's: the
			// game's layouts have none, and their hands stay where the behaviours put them).
			private Medget cursorFrame()
			{
				for (Medget m = childNode(); m != null; m = m.nextSibling())
				{
					if (m.node()?.getFirstNodeByTagNameFromChildren("cursor") != null) return m;
				}
				return null;
			}

			public Medget prevSibling()
			{
				return prevSibling_;
			}

			public Medget nextSibling()
			{
				return nextSibling_;
			}

			public Medget parentNode()
			{
				return parentNode_;
			}

			public Medget childNode()
			{
				return childNode_;
			}

			public XbnNode node()
			{
				return thisNode_;
			}

			public void setPrevSibling(Medget pVal)
			{
				prevSibling_ = pVal;
			}

			public void setNextSibling(Medget pVal)
			{
				nextSibling_ = pVal;
			}

			public void setParentNode(Medget pVal)
			{
				parentNode_ = pVal;
			}

			public void setChildNode(Medget pVal)
			{
				childNode_ = pVal;
			}

			public string _id()
			{
				return id_;
			}

			public short x()
			{
				return x_;
			}

			public short y()
			{
				return y_;
			}

			public short width()
			{
				return width_;
			}

			public short height()
			{
				return height_;
			}

			public sbyte display()
			{
				return display_;
			}

			public void setX(short x)
			{
				x_ = x;
			}

			public void setY(short y)
			{
				y_ = y;
			}

			public void setHeight(short h)
			{
				height_ = h;
			}

			public void setWidth(short w)
			{
				width_ = w;
			}

			public MenuBehavior behavior()
			{
				return behavior_;
			}

			public int work()
			{
				return space.work_;
			}

			public object work1()
			{
				return space.work1_;
			}

			public object work2()
			{
				return space.work2_;
			}

			public object work3()
			{
				return space.work3_;
			}

			/// <summary>PORT: work1 as a number of type T (int, sbyte, an enum...), converted whatever it was boxed as.</summary>
			public T work1<T>() where T : struct => WorkAs<T>(space.work1_);

			public T work2<T>() where T : struct => WorkAs<T>(space.work2_);

			public T work3<T>() where T : struct => WorkAs<T>(space.work3_);

			/// <summary>
			/// PORT: a work slot's number as T. A layout's &lt;work1&gt; comes in as an sbyte and code sets ints, so the number is
			/// converted as the C++ cast did (unchecked: 255 to an sbyte is -1), not unboxed, which throws on any other type.
			/// </summary>
			private static T WorkAs<T>(object value) where T : struct
			{
				if (value is T same) return same;
				if (value is not IConvertible number) return default;
				long n = Convert.ToInt64(number, System.Globalization.CultureInfo.InvariantCulture);
				Type type = typeof(T);
				if (type.IsEnum) return (T)Enum.ToObject(type, n);
				object converted = Type.GetTypeCode(type) switch
				{
					TypeCode.SByte => unchecked((sbyte)n),
					TypeCode.Byte => unchecked((byte)n),
					TypeCode.Int16 => unchecked((short)n),
					TypeCode.UInt16 => unchecked((ushort)n),
					TypeCode.Int32 => unchecked((int)n),
					TypeCode.UInt32 => unchecked((uint)n),
					TypeCode.Int64 => n,
					TypeCode.UInt64 => unchecked((ulong)n),
					TypeCode.Boolean => n != 0,
					_ => null,
				};
				return converted is T t ? t : default;
			}

			public sbyte myTag()
			{
				return myTag_;
			}

			public string up()
			{
				return up_;
			}

			public string down()
			{
				return down_;
			}

			public string left()
			{
				return left_;
			}

			public string right()
			{
				return right_;
			}

			public void setTag(int t)
			{
				myTag_ = (sbyte)t;
			}

			public void setWork(int w)
			{
				space.work_ = w;
			}

			public void setWork1(object w)
			{
				space.work1_ = w;
			}

			public void setWork2(object w)
			{
				space.work2_ = w;
			}

			public void setWork3(object w)
			{
				space.work3_ = w;
			}

			public void setPosY(short posY)
			{
				y_ = posY;
			}

			public void setUp(string val)
			{
				up_ = val;
			}

			public void setDown(string val)
			{
				down_ = val;
			}

			public void setLeft(string val)
			{
				left_ = val;
			}

			public void setRight(string val)
			{
				right_ = val;
			}

			public int cursorY()
			{
				if (cursorFrame() is Medget at) return at.y_;
				// PORT: on Steam's layouts the hand is lifted to the text's middle (SteamLayout).
				return y_ + height_ / 2 + OpenFF.Client.SteamLayout.CursorLift;
			}
		}
	}
}
