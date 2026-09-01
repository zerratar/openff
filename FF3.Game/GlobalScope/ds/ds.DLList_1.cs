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
		public class DLList<T> : DLBaseList
		{
			~DLList()
			{
			}

			public new void initialize()
			{
				base.initialize();
			}

			public void insert(DLNode<T> pPos, DLNode<T> pNode, uint nNode)
			{
				insert((DLBaseNode)pPos, (DLBaseNode)pNode, nNode);
			}

			public void insertFront(DLNode<T> pNode, uint nNode)
			{
				insert((DLBaseNode)front(), (DLBaseNode)pNode, nNode);
			}

			public void insertBack(DLNode<T> pNode, uint nNode)
			{
				insert((DLBaseNode)null, (DLBaseNode)pNode, nNode);
			}

			public void erase(DLNode<T> pNode)
			{
				erase((DLBaseNode)pNode);
			}

			public new void eraseAll()
			{
				base.eraseAll();
			}

			public new DLNode<T> front()
			{
				return (DLNode<T>)base.front();
			}

			public new DLNode<T> back()
			{
				return (DLNode<T>)base.back();
			}

			public new DLNode<T> get(int index)
			{
				return (DLNode<T>)base.get(index);
			}

			public new uint size()
			{
				return (uint)base.size();
			}
		}
	}
}
