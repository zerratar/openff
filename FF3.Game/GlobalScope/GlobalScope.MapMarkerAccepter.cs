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
	public class MapMarkerAccepter
	{
		public int id_;

		public int type_;

		public int dir_;

		public virtual int acceptDir()
		{
			return -1;
		}

		public virtual bool acceptVisibility()
		{
			return false;
		}

		public virtual bool acceptAnimationState()
		{
			return true;
		}

		public MapMarkerAccepter()
		{
			id_ = 0;
			dir_ = -1;
		}

		public virtual VecFx32 acceptPos()
		{
			return null;
		}
	}
}
