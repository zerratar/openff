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
	public static partial class ds
	{
		public class TouchPanel
		{
			public const int DBLCLICK_PROC_WAIT = 0;

			public const int DBLCLICK_PROC_FIRST_INPUT = 1;

			public const int DBLCLICK_PROC_RELEASE = 2;

			public const int DBLCLICK_PROC_TOUCH_KEEP = 3;

			public const int EDGE_IDLE = 0;

			public const int EDGE_FIRST_INPUT = 1;

			public const int EDGE_INPUT = 2;

			private static ushort REPEAT_DELAY = 30;

			private static ushort REPEAT_INTERVAL = 4;

			private static ushort DOUBLECLICK_DELAY = 8;

			private uint edgeTime;

			private TPData raw_point = new TPData();

			private TPData disp_point = new TPData();

			private TPCalibrateParam calibrate;

			private int[] old_point = new int[2];

			private ushort edgeState_;

			private uint auto_t;

			private uint dbclick_t;

			private ushort dbclick_m;

			private ushort repeatDelay;

			private ushort repeatInterval;

			private ushort doubleClickDelay;

			private bool enable__1;

			private bool touch_1;

			private bool touchLast_1;

			private bool touchRepeat_1;

			private bool touchDblclick_1;

			private bool touchEdge__1;

			public TouchPanel()
			{
				repeatDelay = REPEAT_DELAY;
				repeatInterval = REPEAT_INTERVAL;
				doubleClickDelay = DOUBLECLICK_DELAY;
				edgeState_ = 0;
				auto_t = 0u;
				dbclick_t = 0u;
				dbclick_m = 0;
				enable__1 = true;
				touch_1 = false;
				touchLast_1 = false;
				touchRepeat_1 = false;
				touchDblclick_1 = false;
				touchEdge__1 = false;
			}

			public void initialize()
			{
				edgeTime = 0u;
				TP_GetUserInfo(calibrate);
				TP_SetCalibrateParam(calibrate);
				old_point[0] = 0;
				old_point[1] = 0;
				edgeState_ = 0;
				auto_t = 0u;
				dbclick_t = 0u;
				dbclick_m = 0;
				enable__1 = true;
				touch_1 = false;
				touchLast_1 = false;
				touchRepeat_1 = false;
				touchDblclick_1 = false;
				touchEdge__1 = false;
			}

			public void update()
			{
				while (TP_RequestRawSampling(raw_point) != 0)
				{
				}
				TP_GetCalibratedPoint(disp_point, raw_point);
				touchRepeat_1 = false;
				touchDblclick_1 = false;
				touchEdge__1 = false;
				touchLast_1 = touch_1;
				touch_1 = isTouch();
				if (disp_point.touch == 0)
				{
					edgeTime = 0u;
				}
				if (touch_1)
				{
					getPoint(out old_point[0], out old_point[1]);
				}
				updateRepeat();
				updateDoubleClick();
				updateEdge();
			}

			public void updateRepeat()
			{
				if (touch_1)
				{
					auto_t++;
					if (auto_t >= 100000 + repeatInterval)
					{
						auto_t = 100000u;
					}
					if (auto_t == repeatDelay)
					{
						touchRepeat_1 = true;
					}
					if (auto_t >= repeatDelay && auto_t % repeatInterval == 0)
					{
						touchRepeat_1 = true;
					}
					if (!touchLast_1)
					{
						touchRepeat_1 = true;
					}
				}
				else
				{
					auto_t = 0u;
				}
			}

			public void updateDoubleClick()
			{
				switch (dbclick_m)
				{
				case 0:
					if (touchLast_1)
					{
						dbclick_m = 1;
						dbclick_t = 0u;
					}
					break;
				case 1:
					if (touch_1)
					{
						dbclick_t++;
						break;
					}
					if (dbclick_t < doubleClickDelay)
					{
						dbclick_m = 2;
					}
					else
					{
						dbclick_m = 0;
					}
					dbclick_t = 0u;
					break;
				case 2:
					if (!touch_1)
					{
						dbclick_t++;
						break;
					}
					if (dbclick_t < doubleClickDelay)
					{
						dbclick_m = 3;
					}
					else
					{
						dbclick_m = 0;
					}
					dbclick_t = 0u;
					break;
				case 3:
					if (!touch_1)
					{
						dbclick_m = 0;
						dbclick_t = 0u;
					}
					break;
				}
				if (dbclick_m == 3 && touch_1)
				{
					touchDblclick_1 = true;
				}
			}

			public void updateEdge()
			{
				switch (edgeState_)
				{
				case 0:
					if (touch_1)
					{
						edgeState_ = 1;
					}
					break;
				case 1:
					edgeState_ = (ushort)(touch_1 ? 2u : 0u);
					break;
				case 2:
					if (!touch_1)
					{
						edgeState_ = 0;
					}
					break;
				}
				if (edgeState_ == 1)
				{
					touchEdge__1 = true;
				}
			}

			public bool isTouch()
			{
				if (!enable__1)
				{
					return false;
				}
				if (disp_point.touch != 0)
				{
					switch (disp_point.validity)
					{
					case 0:
						return true;
					case 1:
					case 2:
					case 3:
						return false;
					}
				}
				return false;
			}

			public bool isEdgeTouch(int frame)
			{
				if (!enable__1)
				{
					return false;
				}
				if (disp_point.touch != 0)
				{
					switch (disp_point.validity)
					{
					case 0:
						if (edgeTime++ <= frame)
						{
							return true;
						}
						return false;
					case 1:
					case 2:
					case 3:
						edgeTime = 0u;
						return false;
					}
				}
				return false;
			}

			public bool isRelease()
			{
				if (!touchLast_1 || touch_1)
				{
					return false;
				}
				return true;
			}

			public void getPoint(out int x, out int y)
			{
				x = disp_point.x;
				y = disp_point.y;
			}

			public void getLastPoint(out int x, out int y)
			{
				x = old_point[0];
				y = old_point[1];
			}

			public ushort setRepeatDelay(ushort val)
			{
				ushort result = repeatDelay;
				repeatDelay = val;
				return result;
			}

			public ushort getRepeatDelay()
			{
				return repeatDelay;
			}

			public ushort setRepeatInterval(ushort val)
			{
				ushort result = repeatInterval;
				repeatInterval = val;
				return result;
			}

			public ushort getRepeatInterval()
			{
				return repeatInterval;
			}

			public ushort setDoubleClickDelay(ushort val)
			{
				ushort result = doubleClickDelay;
				doubleClickDelay = val;
				return result;
			}

			public ushort getDoubleClickDelay()
			{
				return doubleClickDelay;
			}

			public void enable()
			{
				enable__1 = true;
			}

			public void disable()
			{
				enable__1 = false;
			}

			public bool isRepeatTouch()
			{
				return touchRepeat_1;
			}

			public bool isDblClickTouch()
			{
				return touchDblclick_1;
			}

			public bool isEdge()
			{
				return touchEdge__1;
			}

			public bool isTap()
			{
				if (disp_point.tap != 0)
				{
					return enable__1;
				}
				return false;
			}

			public TPData getDispPoint()
			{
				return disp_point;
			}
		}
	}
}
