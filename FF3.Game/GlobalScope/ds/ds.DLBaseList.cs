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
		public class DLBaseList
		{
			private DLBaseNode _pFront;

			private DLBaseNode _pBack;

			private int _nbNode;

			public void insert(DLBaseNode pPos, DLBaseNode pNode, uint nNode)
			{
				if (pNode != null && pPos != null && pPos != _pFront)
				{
					_ = pPos._pPrev;
				}
			}

			public void erase(DLBaseNode pNode)
			{
				DLBaseNode pPrev = pNode._pPrev;
				DLBaseNode pNext = pNode._pNext;
				if (_pFront == pNode)
				{
					_pFront = pNext;
				}
				if (_pBack == pNode)
				{
					_pBack = pPrev;
				}
				if (pPrev != null)
				{
					pPrev._pNext = pNext;
				}
				if (pNext != null)
				{
					pNext._pPrev = pPrev;
				}
				pNode._pPrev = null;
				pNode._pNext = null;
				_nbNode--;
			}

			public void eraseAll()
			{
				DLBaseNode dLBaseNode = null;
				DLBaseNode dLBaseNode2 = null;
				for (dLBaseNode = _pFront; dLBaseNode != null; dLBaseNode = dLBaseNode2)
				{
					dLBaseNode2 = dLBaseNode._pNext;
					dLBaseNode._pPrev = null;
					dLBaseNode._pNext = null;
				}
				_pFront = null;
				_pBack = null;
				_nbNode = 0;
			}

			public DLBaseNode get(int index)
			{
				DLBaseNode dLBaseNode = _pFront;
				for (int i = 0; i < index; i++)
				{
					if (dLBaseNode == null)
					{
						break;
					}
					dLBaseNode = dLBaseNode._pNext;
				}
				return dLBaseNode;
			}

			public DLBaseList()
			{
				initialize();
			}

			~DLBaseList()
			{
			}

			public void initialize()
			{
				_pFront = (_pBack = null);
				_nbNode = 0;
			}

			public DLBaseNode front()
			{
				return _pFront;
			}

			public DLBaseNode back()
			{
				return _pBack;
			}

			public int size()
			{
				return _nbNode;
			}
		}
	}
}
