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
	public static partial class ds
	{
		public class DLNode<T> : DLBaseNode
		{
			public DLNode()
			{
			}

			public DLNode(T ptr)
				: base(ptr)
			{
			}

			~DLNode()
			{
			}

			public new DLNode<T> prev()
			{
				return (DLNode<T>)base.prev();
			}

			public new DLNode<T> next()
			{
				return (DLNode<T>)base.next();
			}

			public new T data()
			{
				return (T)base.data();
			}

			public void setData(T data)
			{
				setData((object)data);
			}
		}
	}
}
