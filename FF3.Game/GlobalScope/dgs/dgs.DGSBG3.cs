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
	public static partial class dgs
	{
		public class DGSBG3 : DGSPlane
		{
			public DGSBG3()
			{
				targetPlaneMask = GXPlaneMask.GX_PLANEMASK_BG3;
			}

			public override void Clear()
			{
				if (mode == 0)
				{
					G2_SetBG3Offset(0, 0);
				}
				else
				{
					G2S_SetBG3Offset(0, 0);
				}
			}

			public override ushort GetColorMode()
			{
				if (GX_GetDispCnt().bgMode == 0)
				{
					if (mode == 0)
					{
						return (ushort)G2_GetBG3ControlText().colorMode;
					}
					return (ushort)G2S_GetBG3ControlText().colorMode;
				}
				return 1;
			}

			public override Array GetBGCharPtr()
			{
				if (mode == 0)
				{
					return G2_GetBG3CharPtr();
				}
				return G2S_GetBG3CharPtr();
			}

			public override Array GetBGScrPtr()
			{
				if (mode == 0)
				{
					return G2_GetBG3ScrPtr();
				}
				return G2S_GetBG3ScrPtr();
			}

			public override void LoadChar(Array pSrc, uint offset, uint szByte)
			{
				if (mode == 0)
				{
					GX_LoadBG3Char(pSrc, offset, szByte);
				}
				else
				{
					GXS_LoadBG3Char(pSrc, offset, szByte);
				}
			}

			public override void LoadExtPltt(Array pSrc, uint szByte)
			{
				if (mode == 0)
				{
					GX_BeginLoadBGExtPltt();
					GX_LoadBGExtPltt(pSrc, 24576u, szByte);
					GX_EndLoadBGExtPltt();
				}
				else
				{
					GXS_BeginLoadBGExtPltt();
					GXS_LoadBGExtPltt(pSrc, 24576u, szByte);
					GXS_EndLoadBGExtPltt();
				}
			}

			public override void LoadScr(Array pSrc, uint offset, uint szByte)
			{
				if (mode == 0)
				{
					GX_LoadBG3Scr(pSrc, offset, szByte);
				}
				else
				{
					GXS_LoadBG3Scr(pSrc, offset, szByte);
				}
			}
		}
	}
}
