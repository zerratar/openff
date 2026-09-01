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
		public enum BW_FRAME_KIND
		{
			BWFK_UL0,
			BWFK_UCL0,
			BWFK_UCL1,
			BWFK_UCL2,
			BWFK_UCL3,
			BWFK_UCR0,
			BWFK_UR0,
			BWFK_CLU0,
			BWFK_CLU1,
			BWFK_CLD0,
			BWFK_CR0,
			BWFK_CR1,
			BWFK_DL0,
			BWFK_DC0,
			BWFK_DC1,
			BWFK_DC2,
			BWFK_DC3,
			BWFK_DR0,
			BWFK_MAX
		}
	}
}
