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
using OpenFF.Platform;
using OpenFF.Resources;

internal static partial class GlobalScope
{
	public static partial class eld
	{
		public class FadeSetup
		{
			public ushort usStart;

			public ushort usTime;

			public ds.Vector4<short> vColorSub;

			public ushort _pad0;

			public static explicit operator FadeSetup(ArrayReader src)
			{
				FadeSetup fadeSetup = new FadeSetup();
				fadeSetup.usStart = src.readUInt16();
				fadeSetup.usTime = src.readUInt16();
				fadeSetup.vColorSub = new ds.Vector4<short>();
				fadeSetup.vColorSub.cr = src.readInt16();
				fadeSetup.vColorSub.cg = src.readInt16();
				fadeSetup.vColorSub.cb = src.readInt16();
				fadeSetup.vColorSub.ca = src.readInt16();
				fadeSetup._pad0 = src.readUInt16();
				return fadeSetup;
			}
		}
	}
}
