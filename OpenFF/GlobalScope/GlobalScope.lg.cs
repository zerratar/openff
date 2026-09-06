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
	public static class lg
	{
		public class SLBaseList
		{
			private SLBaseNode _pFront;

			private SLBaseNode _pBack;

			private int _nbNode;

			public void insert(SLBaseNode pPos, SLBaseNode pNode, uint nNode)
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
				}
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

			public uint size()
			{
				return (uint)_nbNode;
			}
		}

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

			public uint size()
			{
				return (uint)_nbNode;
			}
		}

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

		public class DLBaseNode
		{
			public DLBaseNode _pPrev;

			public DLBaseNode _pNext;

			private object _pData;

			public DLBaseNode()
			{
				_pPrev = null;
				_pNext = null;
				_pData = null;
			}

			public DLBaseNode(object ptr)
			{
				_pPrev = null;
				_pNext = null;
				_pData = ptr;
			}

			public DLBaseNode prev()
			{
				return _pPrev;
			}

			public DLBaseNode next()
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

		public class SLNode<T> : SLBaseNode
		{
			public SLNode()
			{
			}

			public SLNode(T ptr)
				: base(ptr)
			{
			}

			public new SLNode<T> next()
			{
				return (SLNode<T>)base.next();
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

		public class SLList<T> : SLBaseList
		{
			public new void initialize()
			{
				base.initialize();
			}

			public void insert(SLNode<T> pPos, SLNode<T> pNode, uint nNode)
			{
				insert((SLBaseNode)pPos, (SLBaseNode)pNode, nNode);
			}

			public void insertFront(SLNode<T> pNode, uint nNode)
			{
				insert((SLBaseNode)front(), (SLBaseNode)pNode, nNode);
			}

			public void insertBack(SLNode<T> pNode, uint nNode)
			{
				insert((SLBaseNode)null, (SLBaseNode)pNode, nNode);
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
				return base.size();
			}
		}

		public class DLNode<T> : DLBaseNode
		{
			public DLNode()
			{
			}

			public DLNode(T ptr)
				: base(ptr)
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

		public class DLList<T> : DLBaseList
		{
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
				return base.size();
			}
		}
	}
}
