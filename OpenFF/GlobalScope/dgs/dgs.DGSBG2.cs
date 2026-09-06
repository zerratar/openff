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
		public class DGSBG2 : DGSPlane
		{
			public DGSBG2()
			{
				targetPlaneMask = GXPlaneMask.GX_PLANEMASK_BG2;
			}

			public override void Clear()
			{
				if (mode == 0)
				{
					G2_SetBG2Offset(0, 0);
				}
				else
				{
					G2S_SetBG2Offset(0, 0);
				}
			}

			public override ushort GetColorMode()
			{
				switch (GX_GetDispCnt().bgMode)
				{
				case 0:
				case 1:
				case 3:
					if (mode == 0)
					{
						return (ushort)G2_GetBG2ControlText().colorMode;
					}
					return (ushort)G2S_GetBG2ControlText().colorMode;
				default:
					return 1;
				}
			}

			public override Array GetBGCharPtr()
			{
				if (mode == 0)
				{
					return G2_GetBG2CharPtr();
				}
				return G2S_GetBG2CharPtr();
			}

			public override Array GetBGScrPtr()
			{
				if (mode == 0)
				{
					return G2_GetBG2ScrPtr();
				}
				return G2S_GetBG2ScrPtr();
			}

			public override void LoadChar(Array pSrc, uint offset, uint szByte)
			{
				if (mode == 0)
				{
					GX_LoadBG2Char(pSrc, offset, szByte);
				}
				else
				{
					GXS_LoadBG2Char(pSrc, offset, szByte);
				}
			}

			public override void LoadExtPltt(Array pSrc, uint szByte)
			{
				if (mode == 0)
				{
					GX_BeginLoadBGExtPltt();
					GX_LoadBGExtPltt(pSrc, 16384u, szByte);
					GX_EndLoadBGExtPltt();
				}
				else
				{
					GXS_BeginLoadBGExtPltt();
					GXS_LoadBGExtPltt(pSrc, 16384u, szByte);
					GXS_EndLoadBGExtPltt();
				}
			}

			public override void LoadScr(Array pSrc, uint offset, uint szByte)
			{
				if (mode == 0)
				{
					GX_LoadBG2Scr(pSrc, offset, szByte);
				}
				else
				{
					GXS_LoadBG2Scr(pSrc, offset, szByte);
				}
			}
		}
	}
}
