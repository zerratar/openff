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
	public static partial class eld
	{
		public class Node
		{
			private Node _next;

			private object _data;

			public void set(object data, Node next)
			{
				_data = data;
				_next = next;
			}

			~Node()
			{
			}

			public object value()
			{
				return _data;
			}

			public object setValue(object data)
			{
				_data = data;
				return _data;
			}

			public Node next()
			{
				return _next;
			}

			public void connect(Node pNext)
			{
				_next = pNext;
			}
		}
	}
}
