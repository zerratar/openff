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
		public class CDebugPad : CPad
		{
			public override void initialize()
			{
				m_Debug = true;
				m_Delay = CPad.AUTO_DELAY;
				m_Repeat = CPad.REPEATINTERVAL;
			}

			public override void update()
			{
				do
				{
					read();
					if ((edge() & 0x2000) != 0)
					{
						m_Debug = !m_Debug;
					}
					if (m_Debug && (pad() & 0x200) != 0 && pad() != SOFTRESET_KEYDEF)
					{
						SVC_WaitVBlankIntr();
						read();
						continue;
					}
					break;
				}
				while ((edge() & 0x100) == 0);
			}

			public CDebugPad()
			{
				initialize();
			}
		}
	}
}
