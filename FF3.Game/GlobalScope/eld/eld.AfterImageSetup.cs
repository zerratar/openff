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
		public class AfterImageSetup
		{
			public ushort usNbAfters;

			public ds.Vector4<short> vColor;

			public static explicit operator AfterImageSetup(ArrayReader src)
			{
				AfterImageSetup afterImageSetup = new AfterImageSetup();
				afterImageSetup.usNbAfters = src.readUInt16();
				afterImageSetup.vColor = new ds.Vector4<short>();
				afterImageSetup.vColor.cr = src.readInt16();
				afterImageSetup.vColor.cg = src.readInt16();
				afterImageSetup.vColor.cb = src.readInt16();
				afterImageSetup.vColor.ca = src.readInt16();
				return afterImageSetup;
			}
		}
	}
}
