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
		public class DGSPlane
		{
			protected uint mode;

			protected GXPlaneMask targetPlaneMask;

			public DGSPlane()
			{
				mode = 0u;
				targetPlaneMask = GXPlaneMask.GX_PLANEMASK_NONE;
			}

			public virtual Array GetBGCharPtr()
			{
				return null;
			}

			public virtual Array GetBGScrPtr()
			{
				return null;
			}

			public virtual void Clear()
			{
			}

			public virtual ushort GetColorMode()
			{
				return 0;
			}

			public virtual void LoadChar(Array pSrc, uint offset, uint szByte)
			{
			}

			public virtual void LoadExtPltt(Array pSrc, uint szByte)
			{
			}

			public virtual void LoadScr(Array pSrc, uint offset, uint szByte)
			{
			}

			public GXPlaneMask getPlaneMask()
			{
				return targetPlaneMask;
			}

			public void changeMode(uint m)
			{
				mode = m;
			}

			public uint getMode()
			{
				return mode;
			}
		}
	}
}
