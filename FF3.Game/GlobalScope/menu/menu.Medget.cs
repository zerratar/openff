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
	public static partial class menu
	{
		public class Medget : dgs.DGSLinkedList<Medget>
		{
			public class WORK_SPACE
			{
				public object work_;

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
				space.work_ = (space.work1_ = (space.work2_ = 0));
				display_ = 0;
				myTag_ = 0;
				up_ = (down_ = (left_ = (right_ = default_id)));
			}

			public bool _id(string _id)
			{
				if (strcmp(id_, _id) == 0)
				{
					return true;
				}
				return false;
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
					if (strcmp(medget._id(), _id) == 0)
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
					if (strcmp(medget2._id(), _id) == 0)
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
				return x_ + ((behavior_ != null) ? behavior_.bmGetCursorX(this) : 0);
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

			public object work()
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

			public void setWork(object w)
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
				return y_ + height_ / 2;
			}
		}
	}
}
