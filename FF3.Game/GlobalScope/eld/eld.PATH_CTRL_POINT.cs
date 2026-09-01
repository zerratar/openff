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
	public static partial class eld
	{
		public class PATH_CTRL_POINT
		{
			private float[] fPosition = new float[4];

			private float[] fVector1 = new float[4];

			private float[] fVector2 = new float[4];

			private float[] fFigure = new float[4];

			private float[] fFigure1 = new float[4];

			private float[] fFigure2 = new float[4];

			private uint uiTime;

			private uint[] res = new uint[7];
		}
	}
}
