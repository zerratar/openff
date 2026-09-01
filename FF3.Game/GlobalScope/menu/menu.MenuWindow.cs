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
		public class MenuWindow
		{
			public enum MENU_WINDOW_MOVE_TYPE
			{
				WINDOW_SIZE_MOVING_LARGE,
				WINDOW_SIZE_MOVING_SMALL
			}

			public const MENU_WINDOW_MOVE_TYPE WINDOW_SIZE_MOVING_LARGE = MENU_WINDOW_MOVE_TYPE.WINDOW_SIZE_MOVING_LARGE;

			public const MENU_WINDOW_MOVE_TYPE WINDOW_SIZE_MOVING_SMALL = MENU_WINDOW_MOVE_TYPE.WINDOW_SIZE_MOVING_SMALL;

			private bool bEnableSb;

			private int bEnable;

			private ds.Vector2<int> oneRatio = new ds.Vector2<int>();

			private ds.Vector2<int> nSize = new ds.Vector2<int>();

			private ds.Vector2<short> Pos = new ds.Vector2<short>();

			private ds.Vector2<short> mSize = new ds.Vector2<short>();

			private BasicWindow window = new BasicWindow();

			public bool SizeMoving(MENU_WINDOW_MOVE_TYPE type)
			{
				bool flag = true;
				bool flag2 = true;
				ds.Vector2<int> vector = new ds.Vector2<int>();
				if (type == MENU_WINDOW_MOVE_TYPE.WINDOW_SIZE_MOVING_LARGE)
				{
					vector = oneRatio;
				}
				else
				{
					vector.vx = -oneRatio.vx;
					vector.vy = -oneRatio.vy;
				}
				nSize.vx += vector.vx;
				nSize.vy += vector.vy;
				if (type == MENU_WINDOW_MOVE_TYPE.WINDOW_SIZE_MOVING_LARGE)
				{
					if (nSize.vx >= mSize.vx * 4096)
					{
						nSize.vx = mSize.vx * 4096;
						flag = false;
					}
					if (nSize.vy >= mSize.vy * 4096)
					{
						nSize.vy = mSize.vy * 4096;
						flag2 = false;
					}
				}
				else
				{
					if (nSize.vx <= 0)
					{
						nSize.vx = 0;
						flag = false;
					}
					if (nSize.vy <= 0)
					{
						nSize.vy = 0;
						flag2 = false;
					}
				}
				ds.Vector2<short> vector2 = new ds.Vector2<short>();
				vector2.vx = (short)(nSize.vx >> 12);
				vector2.vy = (short)(nSize.vy >> 12);
				window.SetSize(vector2, update: false);
				vector2.vx = (short)(mSize.vx - (short)(nSize.vx >> 12));
				vector2.vy = (short)(mSize.vy - (short)(nSize.vy >> 12));
				vector2.vx /= 2;
				vector2.vy /= 2;
				vector2.vx += Pos.vx;
				vector2.vy += Pos.vy;
				window.SetPositionUL(vector2);
				if (!flag && !flag2)
				{
					return false;
				}
				return true;
			}

			public MenuWindow()
			{
				bEnable = -1;
				bEnableSb = false;
			}

			public void CalcOneRatio(int flameCount)
			{
				oneRatio.vx = FX_Div(mSize.vx * 4096, flameCount * 4096);
				oneRatio.vy = FX_Div(mSize.vy * 4096, flameCount * 4096);
				if (flameCount == 0)
				{
					oneRatio.vx = mSize.vx * 4096;
					oneRatio.vy = mSize.vy * 4096;
				}
			}

			public void SetMaxWindowPos(ds.Vector2<short> val)
			{
				Pos.copy(val);
			}

			public void SetMaxWindowSize(ds.Vector2<short> val)
			{
				mSize.copy(val);
			}

			public ds.Vector2<int> GetNowWindowSize()
			{
				return nSize;
			}

			public ds.Vector2<short> GetMaxWindowSize()
			{
				return mSize;
			}

			public void ClearNowWindowSize()
			{
				nSize.vx = 0;
				nSize.vy = 0;
			}

			public void SetEnable(int val)
			{
				bEnable = val;
			}

			public int GetEnable()
			{
				return bEnable;
			}

			public void SetScrollEnable(bool val)
			{
				bEnableSb = val;
			}

			public bool GetScrollEnable()
			{
				return bEnableSb;
			}

			public BasicWindow GetWindowHandle()
			{
				return window;
			}
		}
	}
}
