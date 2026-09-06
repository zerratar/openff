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
	public static partial class ds
	{
		public class SLBaseList
		{
			private SLBaseNode _pFront;

			private SLBaseNode _pBack;

			private int _nbNode;

			public void insert(SLBaseNode pPos, SLBaseNode[] pNode, uint nNode)
			{
				if (pNode == null)
				{
					return;
				}
				SLBaseNode sLBaseNode = null;
				if (pPos != null && pPos != _pFront)
				{
					sLBaseNode = _pFront;
					while (sLBaseNode != null && sLBaseNode._pNext != pPos)
					{
						sLBaseNode = sLBaseNode._pNext;
					}
					if (sLBaseNode == null)
					{
						return;
					}
				}
				int num = 0;
				int i;
				for (i = 0; i < nNode; i++)
				{
					pNode[num]._pNext = ((num + 1 < nNode) ? pNode[num + 1] : null);
					num++;
				}
				if (i == 0)
				{
					return;
				}
				num--;
				pNode[num]._pNext = pPos;
				if (pPos == null)
				{
					if (_pBack != null)
					{
						_pBack._pNext = pNode[0];
					}
					_pBack = pNode[num];
				}
				else
				{
					if (sLBaseNode != null)
					{
						sLBaseNode._pNext = pNode[0];
					}
					if (_pBack == null)
					{
						_pBack = pNode[num];
					}
				}
				if (pPos == _pFront)
				{
					_pFront = pNode[0];
				}
				_nbNode += (int)nNode;
			}

			public void erase(SLBaseNode pNode)
			{
				if (pNode == null)
				{
					return;
				}
				if (_pFront == pNode)
				{
					if (_pBack == pNode)
					{
						_pFront = null;
						_pBack = null;
					}
					else
					{
						_pFront = pNode._pNext;
					}
				}
				else
				{
					SLBaseNode sLBaseNode = _pFront;
					while (sLBaseNode != null && sLBaseNode._pNext != pNode)
					{
						sLBaseNode = sLBaseNode._pNext;
					}
					if (sLBaseNode == null)
					{
						return;
					}
					sLBaseNode._pNext = pNode._pNext;
					if (_pBack == pNode)
					{
						_pBack = sLBaseNode;
					}
				}
				pNode._pNext = null;
				_nbNode--;
			}

			public void eraseAll()
			{
				SLBaseNode sLBaseNode = null;
				SLBaseNode sLBaseNode2 = null;
				sLBaseNode = _pFront;
				if (sLBaseNode != null)
				{
					sLBaseNode2 = sLBaseNode._pNext;
				}
				while (sLBaseNode2 != null)
				{
					sLBaseNode._pNext = null;
					sLBaseNode = sLBaseNode2;
					sLBaseNode2 = sLBaseNode2._pNext;
				}
				_pFront = null;
				_pBack = null;
				_nbNode = 0;
			}

			public SLBaseNode get(int index)
			{
				SLBaseNode sLBaseNode = _pFront;
				for (int i = 0; i < index; i++)
				{
					if (sLBaseNode == null)
					{
						break;
					}
					sLBaseNode = sLBaseNode._pNext;
				}
				return sLBaseNode;
			}

			public SLBaseList()
			{
				initialize();
			}

			~SLBaseList()
			{
			}

			public void initialize()
			{
				_pFront = (_pBack = null);
				_nbNode = 0;
			}

			public SLBaseNode front()
			{
				return _pFront;
			}

			public SLBaseNode back()
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
