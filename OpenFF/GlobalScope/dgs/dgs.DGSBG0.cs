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
	public static partial class dgs
	{
		public class DGSBG0 : DGSPlane
		{
			public DGSBG0()
			{
				targetPlaneMask = GXPlaneMask.GX_PLANEMASK_BG0;
			}

			public override void Clear()
			{
				if (mode == 0)
				{
					G2_SetBG0Offset(0, 0);
				}
				else
				{
					G2S_SetBG0Offset(0, 0);
				}
			}

			public override ushort GetColorMode()
			{
				if (mode == 0)
				{
					return (ushort)G2_GetBG0Control().colorMode;
				}
				return (ushort)G2S_GetBG0Control().colorMode;
			}

			public override Array GetBGCharPtr()
			{
				if (mode == 0)
				{
					return G2_GetBG0CharPtr();
				}
				return G2S_GetBG0CharPtr();
			}

			public override Array GetBGScrPtr()
			{
				if (mode == 0)
				{
					return G2_GetBG0ScrPtr();
				}
				return G2S_GetBG0ScrPtr();
			}

			public override void LoadChar(Array pSrc, uint offset, uint szByte)
			{
				if (mode == 0)
				{
					GX_LoadBG0Char(pSrc, offset, szByte);
				}
				else
				{
					GXS_LoadBG0Char(pSrc, offset, szByte);
				}
			}

			public override void LoadExtPltt(Array pSrc, uint szByte)
			{
				if (mode == 0)
				{
					GX_BeginLoadBGExtPltt();
					GX_LoadBGExtPltt(pSrc, 0u, szByte);
					GX_EndLoadBGExtPltt();
				}
				else
				{
					GXS_BeginLoadBGExtPltt();
					GXS_LoadBGExtPltt(pSrc, 0u, szByte);
					GXS_EndLoadBGExtPltt();
				}
			}

			public override void LoadScr(Array pSrc, uint offset, uint szByte)
			{
				if (mode == 0)
				{
					GX_LoadBG0Scr(pSrc, offset, szByte);
				}
				else
				{
					GXS_LoadBG0Scr(pSrc, offset, szByte);
				}
			}
		}
	}
}
