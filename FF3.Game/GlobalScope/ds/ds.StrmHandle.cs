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
		public class StrmHandle
		{
			private static int STRM_CHANNEL_0 = 0;

			private static int STRM_CHANNEL_1 = 1;

			private NNSSndStrmHandle m_StrmHandle;

			public int m_bLoopFlag;

			private int m_bPauseFlag;

			private uint m_u32Offset;

			private int m_nStrmNo;

			private int m_nPlayerNo;

			private int m_nPlayerPrio;

			public StrmHandle()
			{
				NNS_SndStrmHandleInit(m_StrmHandle);
				if (OS_IsThreadAvailable() == 0)
				{
					OS_InitThread();
				}
			}

			~StrmHandle()
			{
				Stop(0);
				NNS_SndStrmHandleRelease(m_StrmHandle);
			}

			public void Play()
			{
				NNS_SndArcStrmStartPrepared(m_StrmHandle);
			}

			public void Pause()
			{
				if (m_bPauseFlag == 1)
				{
					Play();
					m_bPauseFlag = 0;
					return;
				}
				m_u32Offset = NNS_SndArcStrmGetCurrentPlayingPos(m_StrmHandle);
				Stop(0);
				Prepare(m_nStrmNo, bWaitPrepared: true, m_nPlayerNo, m_nPlayerPrio, m_u32Offset);
				m_bPauseFlag = 1;
			}

			public bool Prepare(int nStrmNo, bool bWaitPrepared, int nPlayerNo, int nPlayerPrio, uint nOffset)
			{
				bool result = false;
				if (IsValid() == 1)
				{
					Stop(0);
				}
				if (NNS_SndArcStrmPrepareEx2(m_StrmHandle, nPlayerNo, nPlayerPrio, nStrmNo, nOffset, null, null, SndArcStrmCallback, reinterpret_cast<object>(this)) == 1)
				{
					m_u32Offset = nOffset;
					m_nStrmNo = nStrmNo;
					m_nPlayerNo = nPlayerNo;
					m_nPlayerPrio = nPlayerPrio;
					OS_Printf("Sound : Stream Handle Prepared. StrmNo( %d ) \n", nStrmNo);
					if (bWaitPrepared)
					{
						WaitPrepare();
					}
					result = true;
				}
				else
				{
					OS_Printf("Sound : Stream Handle Prepare Failed. StrmNo( %d ) \n", nStrmNo);
				}
				return result;
			}

			public void WaitPrepare()
			{
				while (NNS_SndArcStrmIsPrepared(m_StrmHandle) != 1)
				{
				}
			}

			public void SetLoop(int bLoopFlag)
			{
				m_bLoopFlag = bLoopFlag;
			}

			public void Work()
			{
			}

			public void Stop(int nFadeOutFrame)
			{
				m_bLoopFlag = 0;
				NNS_SndArcStrmStop(m_StrmHandle, nFadeOutFrame);
			}

			public void SetVolume(int nVolume, int nFrame)
			{
				NNS_SndArcStrmMoveVolume(m_StrmHandle, nVolume, nFrame);
			}

			public void SetPan(int nChNo, int nPan)
			{
				NNS_SndArcStrmSetChannelPan(m_StrmHandle, nChNo, nPan);
			}

			public int IsValid()
			{
				return NNS_SndStrmHandleIsValid(m_StrmHandle);
			}

			public int IsPlaying()
			{
				if (NNS_SndArcStrmGetCurrentPlayingPos(m_StrmHandle) == 0)
				{
					return 0;
				}
				return 1;
			}
		}
	}
}
