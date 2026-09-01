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
	public static partial class mognet
	{
		public enum SUB_BG_SCR
		{
			SBS_MAINMENU,
			SBS_LETTER_EDIT,
			SBS_LETTER_BROWSE,
			SBS_INPUT_FRIENDCODE,
			SBS_FRIENLIST,
			SBS_MAILLIST,
			SBS_SELECT_PERSON,
			SBS_MASTERCARD,
			NUMBER_OF_SBS
		}
	}
}
