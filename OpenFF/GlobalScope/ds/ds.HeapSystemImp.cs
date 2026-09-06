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
		public class HeapSystemImp
		{
			public class HeapBlockInfo
			{
				public Array unAddress;

				public uint unSize;

				public object unObject;

				public HeapBlockInfo()
				{
				}

				public HeapBlockInfo(Array arg0, uint arg1, object arg2)
				{
					unAddress = arg0;
					unSize = arg1;
					unObject = arg2;
				}

				public void copy(HeapBlockInfo src)
				{
					unAddress = src.unAddress;
					unSize = src.unSize;
					unObject = src.unObject;
				}
			}

			private uint _unHeapSize;

			private int _nAlign;

			private int _hHeap;

			private NNSFndAllocator _alcHeap;

			public HeapSystemImp()
			{
				_unHeapSize = 0u;
				_nAlign = 0;
				_hHeap = 0;
			}

			~HeapSystemImp()
			{
			}

			public bool initialize(Array arena, uint size, uint align, uint nbBlocks)
			{
				_hHeap = NNS_FndCreateExpHeap(arena, size);
				NNS_FndInitAllocatorForExpHeap(_alcHeap, _hHeap, 4);
				_unHeapSize = size;
				_nAlign = (ushort)align;
				return true;
			}

			public void cleanup()
			{
				if (_hHeap != 0)
				{
					NNS_FndDestroyExpHeap(_hHeap);
					_hHeap = 0;
				}
			}

			public Array allocate(ulong size)
			{
				return NNS_FndAllocFromExpHeapEx(_hHeap, (uint)size, _nAlign);
			}

			public object allocate(Type size)
			{
				return NNS_FndAllocFromExpHeapEx(_hHeap, size, _nAlign);
			}

			public void deallocate(Array pBlock)
			{
				if (pBlock != null)
				{
					NNS_FndFreeToExpHeap(_hHeap, pBlock);
				}
			}

			public void deallocate(object pBlock)
			{
				if (pBlock != null)
				{
					NNS_FndFreeToExpHeap(_hHeap, pBlock);
				}
			}

			public uint resize(Array pBlock, ulong size)
			{
				if (pBlock == null)
				{
					return 0u;
				}
				return NNS_FndResizeForMBlockExpHeap(_hHeap, pBlock, (uint)size);
			}

			public uint getAllocatableSize()
			{
				if (_hHeap != 0)
				{
					return NNS_FndGetAllocatableSizeForExpHeap(_hHeap);
				}
				return 0u;
			}

			public int align()
			{
				return _nAlign;
			}

			public void realign(int a)
			{
				_nAlign = a;
			}

			public NNSFndAllocator allocator()
			{
				return _alcHeap;
			}

			public int heapHandle()
			{
				return _hHeap;
			}
		}
	}
}
