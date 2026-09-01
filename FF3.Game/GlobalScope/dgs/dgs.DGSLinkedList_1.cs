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
	public static partial class dgs
	{
		public class DGSLinkedList<T>
		{
			protected DGSLinkedList<T> dgsllPreviousePtr;

			protected DGSLinkedList<T> dgsllNextPtr;

			private static DGSLinkedList<T> ptr;

			public DGSLinkedList()
			{
				dgsllPreviousePtr = null;
				dgsllNextPtr = null;
			}

			~DGSLinkedList()
			{
				destruct();
			}

			public void destruct()
			{
				dgsllUnlink();
			}

			public static DGSLinkedList<T> dgsllBase()
			{
				return _dgsllBase();
			}

			public virtual DGSLinkedList<T> dgsllPreviouse()
			{
				return dgsllPreviousePtr;
			}

			public virtual DGSLinkedList<T> dgsllNext()
			{
				return dgsllNextPtr;
			}

			protected virtual void dgsllLink()
			{
				if (dgsllPreviousePtr == null && dgsllNextPtr == null)
				{
					if (_dgsllBase() == null)
					{
						_dgsllBase_set(this);
						dgsllNextPtr = (dgsllPreviousePtr = null);
						return;
					}
					_dgsllBase().dgsllPreviousePtr = this;
					dgsllNextPtr = _dgsllBase();
					_dgsllBase_set(this);
					dgsllPreviousePtr = null;
				}
			}

			protected virtual void dgsllUnlink()
			{
				if (dgsllPreviousePtr != null)
				{
					dgsllPreviousePtr.dgsllNextPtr = dgsllNextPtr;
				}
				if (dgsllNextPtr != null)
				{
					dgsllNextPtr.dgsllPreviousePtr = dgsllPreviousePtr;
				}
				if (_dgsllBase() == this)
				{
					_dgsllBase_set(dgsllNextPtr);
				}
				dgsllPreviousePtr = (dgsllNextPtr = null);
			}

			private static DGSLinkedList<T> _dgsllBase()
			{
				return ptr;
			}

			private static void _dgsllBase_set(DGSLinkedList<T> arg0)
			{
				ptr = arg0;
			}

			public void copy(DGSLinkedList<T> src)
			{
				dgsllPreviousePtr = src.dgsllPreviousePtr;
				dgsllNextPtr = src.dgsllNextPtr;
			}
		}
	}
}
