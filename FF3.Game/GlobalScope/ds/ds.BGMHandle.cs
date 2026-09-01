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
		public class BGMHandle
		{
			public static bool m_BGMEnabler = true;

			private NNSSndHandle m_SndHandle = new NNSSndHandle();

			private bool m_PauseFlag;

			public BGMHandle()
			{
				NNS_SndHandleInit(m_SndHandle);
				m_PauseFlag = false;
			}

			~BGMHandle()
			{
				if (NNS_SndHandleIsValid(m_SndHandle) == 1)
				{
					NNS_SndHandleReleaseSeq(m_SndHandle);
					m_PauseFlag = false;
				}
			}

			public void Play(int nSeqNo)
			{
				if (m_BGMEnabler && NNS_SndHandleIsValid(m_SndHandle) == 0)
				{
					NNS_SndArcPlayerStartSeq(m_SndHandle, nSeqNo);
				}
			}

			public void Play(int nSeqNo, int nPlayerNo, int nBankNo, int nPlayerPrio)
			{
				if (m_BGMEnabler && NNS_SndHandleIsValid(m_SndHandle) == 0 && NNS_SndArcPlayerStartSeqEx(m_SndHandle, nPlayerNo, nBankNo, nPlayerPrio, nSeqNo) == 0)
				{
					OS_Printf("BGM play start failed. \n");
				}
			}

			public void Stop(int nFadeOutFrame)
			{
				NNS_SndPlayerStopSeq(m_SndHandle, nFadeOutFrame);
			}

			public void Pause(int bFlag)
			{
				NNS_SndPlayerPause(m_SndHandle, bFlag);
			}

			public void SetVolume(int nVolume)
			{
				NNS_SndPlayerSetVolume(m_SndHandle, nVolume);
			}

			public void MoveVolume(int nVolume, int nFrame)
			{
				NNS_SndPlayerMoveVolume(m_SndHandle, nVolume, nFrame);
			}

			public bool IsPlaying()
			{
				if (NNS_SndHandleIsValid(m_SndHandle) == 1)
				{
					return true;
				}
				return false;
			}

			public bool IsPausing()
			{
				return m_PauseFlag;
			}

			public void SetBGMEnable(bool b)
			{
				m_BGMEnabler = b;
			}

			public bool GetBGMEnable()
			{
				return m_BGMEnabler;
			}
		}
	}
}
