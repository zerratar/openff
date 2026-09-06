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
	public static partial class dgs
	{
		public class DGSBG1 : DGSPlane
		{
			public DGSBG1()
			{
				targetPlaneMask = GXPlaneMask.GX_PLANEMASK_BG1;
			}

			public override void Clear()
			{
				if (mode == 0)
				{
					G2_SetBG1Offset(0, 0);
				}
				else
				{
					G2S_SetBG1Offset(0, 0);
				}
			}

			public override ushort GetColorMode()
			{
				if (mode == 0)
				{
					return (ushort)G2_GetBG1Control().colorMode;
				}
				return (ushort)G2S_GetBG1Control().colorMode;
			}

			public override Array GetBGCharPtr()
			{
				if (mode == 0)
				{
					return G2_GetBG1CharPtr();
				}
				return G2S_GetBG1CharPtr();
			}

			public override Array GetBGScrPtr()
			{
				if (mode == 0)
				{
					return G2_GetBG1ScrPtr();
				}
				return G2S_GetBG1ScrPtr();
			}

			public override void LoadChar(Array pSrc, uint offset, uint szByte)
			{
				if (mode == 0)
				{
					GX_LoadBG1Char(pSrc, offset, szByte);
				}
				else
				{
					GXS_LoadBG1Char(pSrc, offset, szByte);
				}
			}

			public override void LoadExtPltt(Array pSrc, uint szByte)
			{
				if (mode == 0)
				{
					GX_BeginLoadBGExtPltt();
					GX_LoadBGExtPltt(pSrc, 8192u, szByte);
					GX_EndLoadBGExtPltt();
				}
				else
				{
					GXS_BeginLoadBGExtPltt();
					GXS_LoadBGExtPltt(pSrc, 8192u, szByte);
					GXS_EndLoadBGExtPltt();
				}
			}

			public override void LoadScr(Array pSrc, uint offset, uint szByte)
			{
				if (mode == 0)
				{
					GX_LoadBG1Scr(pSrc, offset, szByte);
				}
				else
				{
					GXS_LoadBG1Scr(pSrc, offset, szByte);
				}
			}
		}
	}
}
