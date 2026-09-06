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
	public class NNSG2dViewRect
	{
		public NNSG2dFVec2 posTopLeft = new NNSG2dFVec2();

		public NNSG2dFVec2 sizeView = new NNSG2dFVec2();

		public NNSG2dViewRect()
		{
		}

		public NNSG2dViewRect(int arg0, int arg1, int arg2, int arg3)
		{
			posTopLeft.x = arg0;
			posTopLeft.y = arg1;
			sizeView.x = arg2;
			sizeView.y = arg3;
		}
	}
}
