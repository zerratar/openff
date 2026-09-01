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
	public static partial class ds
	{
		public class SLBaseNode
		{
			public SLBaseNode _pNext;

			private object _pData;

			public SLBaseNode()
			{
				_pNext = null;
				_pData = null;
			}

			public SLBaseNode(object ptr)
			{
				_pNext = null;
				_pData = ptr;
			}

			~SLBaseNode()
			{
			}

			public SLBaseNode next()
			{
				return _pNext;
			}

			public object data()
			{
				return _pData;
			}

			public void setData(object ptr)
			{
				_pData = ptr;
			}
		}
	}
}
