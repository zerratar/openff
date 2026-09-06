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
		public class SoundHeap
		{
			public static NNSSndHeap m_SndHeapHandle = new NNSSndHeap();

			public static Array m_pStart;

			public static int m_nStackLevel;

			public static void Init(Array pStart, uint u32Size)
			{
				m_pStart = const_cast<Array>(pStart);
				m_SndHeapHandle = NNS_SndHeapCreate(const_cast<Array>(pStart), u32Size);
				m_nStackLevel = 0;
			}

			public static void Final()
			{
				NNS_SndHeapDestroy(m_SndHeapHandle);
			}

			public static int PushState()
			{
				m_nStackLevel = NNS_SndHeapSaveState(m_SndHeapHandle);
				if (m_nStackLevel <= -1)
				{
					OS_Printf("Sound : Push Stack Failed. Level( %d ) \n", m_nStackLevel);
					return 0;
				}
				OS_Printf("Sound : Push Stack Succeed. Level( %d ) \n", m_nStackLevel);
				return 1;
			}

			public static void PopState()
			{
				if (m_nStackLevel != 0 && m_nStackLevel > 1)
				{
					m_nStackLevel--;
					NNS_SndHeapLoadState(m_SndHeapHandle, m_nStackLevel);
					if (m_nStackLevel == 0)
					{
						NNS_SndArcSetup(SoundArchive.GetArc(), m_SndHeapHandle, 0);
					}
					OS_Printf("pop stack ( %d )\n", m_nStackLevel);
				}
			}

			public static uint GetHeapSize()
			{
				return NNS_SndHeapGetSize(m_SndHeapHandle);
			}

			public static uint GetHeapFreeSize()
			{
				return NNS_SndHeapGetFreeSize(m_SndHeapHandle);
			}

			public static void HeapClear()
			{
				NNS_SndHeapClear(m_SndHeapHandle);
				m_nStackLevel = 0;
			}

			public static int GetStackLevel()
			{
				return m_nStackLevel;
			}

			public static void PrintHeapSize()
			{
				OS_Printf("SoundHeap : Size = 0x%x \n", GetHeapSize());
			}

			public static void PrintHeapFreeSize()
			{
				OS_Printf("SoundHeap : Free = 0x%x \n", GetHeapFreeSize());
			}

			public void PrintHeapStackLevel()
			{
				OS_Printf("SoundHeap : Stack Level = %d \n", m_nStackLevel);
			}

			public static NNSSndHeap GetHeapHandle()
			{
				return m_SndHeapHandle;
			}
		}
	}
}
