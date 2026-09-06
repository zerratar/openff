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
		public class PolygonIDPublisher
		{
			private short _sNextID;

			private short _sMinID;

			private short _sModID;

			public PolygonIDPublisher()
			{
				_sNextID = (_sMinID = 21);
			}

			public short publishID()
			{
				if (++_sNextID >= 64)
				{
					_sNextID = _sMinID;
				}
				return _sNextID;
			}

			public void setMinimumID(short _id)
			{
				_sMinID = _id;
			}
		}
	}
}
