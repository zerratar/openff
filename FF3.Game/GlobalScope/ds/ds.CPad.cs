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
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
	public static partial class ds
	{
		public class CPad
		{
			public static uint AUTO_DELAY = 30u;

			public static uint REPEATINTERVAL = 8u;

			protected bool m_Activity;

			protected bool m_Debug;

			protected _Pad m_Pad = new _Pad();

			protected uint m_Delay;

			protected uint m_Repeat;

			public virtual void initialize()
			{
				m_Activity = true;
				m_Debug = false;
				m_Delay = AUTO_DELAY;
				m_Repeat = REPEATINTERVAL;
			}

			public void read()
			{
				ushort num = PAD_Read();
				m_Pad.trigger = (ushort)(num & (num ^ m_Pad.button));
				m_Pad.release = (ushort)(m_Pad.button & (num ^ m_Pad.button));
				m_Pad.press = 0;
				m_Pad.button_last = m_Pad.button;
				m_Pad.button = num;
				for (int i = 0; i < _Pad.AUTO_BUFF_MAX; i++)
				{
					int num2 = 1 << i;
					if ((num & num2) != 0)
					{
						m_Pad.PadAuto_t[i]++;
						if (m_Pad.PadAuto_t[i] >= 100000 + m_Repeat)
						{
							m_Pad.PadAuto_t[i] = 100000;
						}
						if (m_Pad.PadAuto_t[i] == m_Delay)
						{
							m_Pad.press |= (ushort)num2;
						}
						if (m_Pad.PadAuto_t[i] >= m_Delay && m_Pad.PadAuto_t[i] % m_Repeat == 0)
						{
							m_Pad.press |= (ushort)num2;
						}
						if ((m_Pad.button_last & num2) == 0)
						{
							m_Pad.press |= (ushort)num2;
						}
					}
					else
					{
						m_Pad.PadAuto_t[i] = 0;
					}
				}
			}

			public virtual void update()
			{
				if (m_Activity)
				{
					read();
				}
			}

			public ushort pad()
			{
				if (!m_Activity)
				{
					return 0;
				}
				return m_Pad.button;
			}

			public ushort edge()
			{
				if (!m_Activity)
				{
					return 0;
				}
				return m_Pad.trigger;
			}

			public ushort repeat()
			{
				if (!m_Activity)
				{
					return 0;
				}
				return m_Pad.press;
			}

			public ushort dblClick()
			{
				if (!m_Activity)
				{
					return 0;
				}
				return 0;
			}

			public uint setAutoDelay(uint val)
			{
				uint delay = m_Delay;
				m_Delay = val;
				return delay;
			}

			public uint getAutoDelay()
			{
				return m_Delay;
			}

			public uint setRepeatInterval(uint val)
			{
				uint result = m_Repeat;
				m_Repeat = val;
				return result;
			}

			public uint getRepeatInterval()
			{
				return m_Repeat;
			}

			public int getRepeatCount(ushort bit)
			{
				for (int i = 0; i < _Pad.AUTO_BUFF_MAX; i++)
				{
					int num = 1 << i;
					if ((bit & num) != 0)
					{
						return m_Pad.PadAuto_t[i];
					}
				}
				return 0;
			}

			public CPad()
			{
				initialize();
			}

			public void enable()
			{
				m_Activity = true;
			}

			public void disable()
			{
				m_Activity = false;
			}

			public bool isDebug()
			{
				return m_Debug;
			}

			public void setDebug(bool flag)
			{
				m_Debug = flag;
			}
		}
	}
}
