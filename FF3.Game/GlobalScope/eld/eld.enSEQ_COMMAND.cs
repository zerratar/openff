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
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
	public static partial class eld
	{
		public enum enSEQ_COMMAND
		{
			enSEQ_CMD_WAIT,
			enSEQ_CMD_POSITION,
			enSEQ_CMD_BOOT,
			enSEQ_CMD_HALT,
			enSEQ_CMD_MOVE,
			enSEQ_CMD_END,
			enSEQ_CMD_MAX
		}
	}
}
