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
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
	public class G3DDemoGround
	{
		public ushort groundEnable;

		public ushort wireColor;

		public ushort backColor;

		public ushort backAlpha;

		public int scale;

		public int trans_x;

		public int trans_y;

		public int trans_z;
	}
}
