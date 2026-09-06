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
		public enum enVRAM_BANK
		{
			enVRAM_A,
			enVRAM_B,
			enVRAM_C,
			enVRAM_D,
			enVRAM_E,
			enVRAM_F,
			enVRAM_G,
			enVRAM_H,
			enVRAM_I
		}
	}
}
