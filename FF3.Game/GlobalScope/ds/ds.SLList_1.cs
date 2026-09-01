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
		public class SLList<T> : SLBaseList
		{
			~SLList()
			{
			}

			public new void initialize()
			{
				base.initialize();
			}

			public void insert(SLNode<T> pPos, SLNode<T>[] pNode, uint nNode)
			{
				insert((SLBaseNode)pPos, (SLBaseNode[])pNode, nNode);
			}

			public void insertFront(SLNode<T>[] pNode, uint nNode)
			{
				insert((SLBaseNode)front(), (SLBaseNode[])pNode, nNode);
			}

			public void insertBack(SLNode<T>[] pNode, uint nNode)
			{
				insert((SLBaseNode)null, (SLBaseNode[])pNode, nNode);
			}

			public void erase(SLNode<T> pNode)
			{
				erase((SLBaseNode)pNode);
			}

			public new void eraseAll()
			{
				base.eraseAll();
			}

			public new SLNode<T> front()
			{
				return (SLNode<T>)base.front();
			}

			public new SLNode<T> back()
			{
				return (SLNode<T>)base.back();
			}

			public new SLNode<T> get(int index)
			{
				return (SLNode<T>)base.get(index);
			}

			public new uint size()
			{
				return (uint)base.size();
			}
		}
	}
}
