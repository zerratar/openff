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
	public enum LANGUAGE_CODE
	{
		LANGUAGE_CODE_ENGLISH = 0,
		LANGUAGE_CODE_GERMAN = 1,
		LANGUAGE_CODE_SPANISH = 2,
		LANGUAGE_CODE_FRENCH = 3,
		LANGUAGE_CODE_ITALIAN = 4,
		LANGUAGE_CODE_DEFAULT = LANGUAGE_CODE_ENGLISH
	}
}
