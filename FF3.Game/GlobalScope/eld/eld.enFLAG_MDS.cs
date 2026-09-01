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
		public enum enFLAG_MDS
		{
			enFLAG_MDS_LOOP = 1,
			enFLAG_MDS_ANIME_MATERIAL = 2,
			enFLAG_MDS_ANIME_TEXTURE_SRT = 4,
			enFLAG_MDS_ANIME_TEXTURE_PATTERN = 8,
			enFLAG_MDS_ANIME_VISIBILITY = 16,
			enFLAG_MDS_END = 17
		}
	}
}
