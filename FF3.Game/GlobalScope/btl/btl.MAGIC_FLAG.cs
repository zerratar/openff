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
	public static partial class btl
	{
		public enum MAGIC_FLAG
		{
			MF_PROTECT = 1,
			MF_HASTE = 2,
			MF_BAHAMUT = 4,
			MF_SONG1 = 8,
			MF_SONG2 = 0x10,
			MF_SONG5 = 0x20,
			MF_REFLECT = 0x40,
			MF_REFLECTED = 0x80,
			MF_SLEEP = 0x100,
			MF_RECOVER_SLEEP = 0x200,
			MF_CONFUSION = 0x400,
			MF_RECOVER_CONFUSION = 0x800
		}
	}
}
