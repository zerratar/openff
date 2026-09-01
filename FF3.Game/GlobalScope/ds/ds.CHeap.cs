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
		public class CHeap
		{
			public static int m_SysHeap;

			public static int m_AppHeap;

			public static NNSFndAllocator m_Allocator = new NNSFndAllocator();

			public static uint m_pLoArena;

			public static uint m_pHiArena;

			public static uint m_AllocNum;

			public static int DEF_SYSTEM_HEAP_SIZE = 65536;

			public static void initialize(uint unSysHeapSize)
			{
				Array arena = OS_AllocFromMainArenaLo(unSysHeapSize, 16u);
				uint num = ROUND_UP(OS_GetMainArenaLo(), 16);
				uint num2 = ROUND_DOWN(OS_GetMainArenaHi(), 16);
				uint size = num2 - num;
				Array arena2 = OS_AllocFromMainArenaLo(size, 16u);
				m_pLoArena = num;
				m_pHiArena = num2;
				_impSys.initialize(arena, unSysHeapSize, 32u, 256u);
				_impApp.initialize(arena2, size, 16u, 1024u);
				m_AllocNum = 0u;
			}

			public void destroyHeap()
			{
				_impSys.cleanup();
				_impApp.cleanup();
				_impDTCM.cleanup();
			}

			public static Array alloc_sys(ulong size)
			{
				return _impSys.allocate(size);
			}

			public static object alloc_sys(Type size)
			{
				return _impSys.allocate(size);
			}

			public static void free_sys(Array addr)
			{
				_impSys.deallocate(addr);
			}

			public static void free_sys(object addr)
			{
				_impSys.deallocate(addr);
			}

			public static Array alloc_app(uint size)
			{
				m_AllocNum++;
				return _impApp.allocate(size);
			}

			public static object alloc_app(Type size)
			{
				m_AllocNum++;
				return _impApp.allocate(size);
			}

			public static void chmode_app(bool b)
			{
				if (b && _impApp.align() < 0)
				{
					_impApp.realign(_impApp.align() * -1);
				}
				else if (!b && _impApp.align() > 0)
				{
					_impApp.realign(_impApp.align() * -1);
				}
			}

			public ulong resize_sys(Array addr, ulong size)
			{
				return _impSys.resize(addr, size);
			}

			public static ulong resize_app(Array addr, ulong size)
			{
				return _impApp.resize(addr, size);
			}

			public static int align_app()
			{
				return _impApp.align();
			}

			public static void realign_app(int a)
			{
				_impApp.realign(a);
			}

			public static void free_app(Array addr)
			{
				m_AllocNum--;
				_impApp.deallocate(addr);
			}

			public static void free_app(object addr)
			{
				m_AllocNum--;
				_impApp.deallocate(addr);
			}

			public Array alloc_dtcm(ulong size)
			{
				return _impDTCM.allocate(size);
			}

			public void free_dtcm(Array addr)
			{
				_impDTCM.deallocate(addr);
			}

			public static void setID_app(ushort _id)
			{
				if (255 >= _id)
				{
					NNS_FndSetGroupIDForExpHeap(_impApp.heapHandle(), _id);
				}
			}

			public ushort getID_app()
			{
				return NNS_FndGetGroupIDForExpHeap(_impApp.heapHandle());
			}

			public static ushort setAllocMode_app(ushort mode)
			{
				return NNS_FndSetAllocModeForExpHeap(_impApp.heapHandle(), mode);
			}

			public void freeAllBlockByID_app(ushort _id)
			{
				NNS_FndVisitAllocatedForExpHeap(_impApp.heapHandle(), HVFreeAllBlockByID, _id);
			}

			public NNSFndAllocator getSysAllocator()
			{
				return _impSys.allocator();
			}

			public static NNSFndAllocator getAppAllocator()
			{
				return _impApp.allocator();
			}

			public static int getHeapHandle()
			{
				return _impApp.heapHandle();
			}

			public NNSFndAllocator getDTCMAllocator()
			{
				return _impDTCM.allocator();
			}

			public static uint getAllocatableSize()
			{
				return _impApp.getAllocatableSize();
			}

			public static uint getAllocNum()
			{
				return m_AllocNum;
			}

			public static uint GetLoArena()
			{
				return m_pLoArena;
			}

			public static uint GetHiArena()
			{
				return m_pHiArena;
			}
		}
	}
}
