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
		public class DSAllocator : IAllocator
		{
			private uint _unPoolSize;

			private ushort _usPoolTale;

			private ushort[] _pPoolBlank;

			private Node[] _pNodePool;

			public DSAllocator()
			{
				_unPoolSize = 0u;
				_usPoolTale = 0;
				_pPoolBlank = null;
				_pNodePool = null;
			}

			public void initializeNodePool()
			{
				if (_pNodePool != null)
				{
					cleanupNodePool();
				}
				ushort num = 128;
				_unPoolSize = (uint)(num * 8);
				_pNodePool = new Node[num];
				for (int i = 0; i < num; i++)
				{
					_pNodePool[i] = new Node();
				}
				_pPoolBlank = new ushort[num];
				_usPoolTale = num;
				for (ushort num2 = 0; num2 < num; num2++)
				{
					_pPoolBlank[num2] = num2;
				}
			}

			public void cleanupNodePool()
			{
				if (_pNodePool != null)
				{
					ds.CHeap.free_sys(_pNodePool);
				}
				if (_pPoolBlank != null)
				{
					ds.CHeap.free_sys(_pPoolBlank);
				}
				_unPoolSize = 0u;
				_usPoolTale = 0;
			}

			public Array allocateMemory(uint size)
			{
				return ds.CHeap.alloc_app(size);
			}

			public void deallocateMemory(Array mem)
			{
				ds.CHeap.free_app(mem);
			}

			public Node allocateListNode(uint size)
			{
				if (_usPoolTale == 0)
				{
					return null;
				}
				return _pNodePool[_pPoolBlank[--_usPoolTale]];
			}

			public void deallocateListNode(Node node)
			{
				uint num = uint.MaxValue;
				for (int i = 0; i < _pNodePool.Length; i++)
				{
					if (_pNodePool[i] == node)
					{
						num = (uint)(i * 8);
						break;
					}
				}
				if (0 <= num && num < _unPoolSize)
				{
					num /= 8;
					_pPoolBlank[_usPoolTale++] = (ushort)num;
				}
			}
		}
	}
}
