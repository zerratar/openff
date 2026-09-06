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
	public static partial class ds
	{
		public class SEHandle
		{
			private NNSSndHandle m_SndHandle = new NNSSndHandle();

			public SEHandle()
			{
				NNS_SndHandleInit(m_SndHandle);
			}

			~SEHandle()
			{
				if (NNS_SndHandleIsValid(m_SndHandle) == 1)
				{
					NNS_SndHandleReleaseSeq(m_SndHandle);
				}
			}

			public bool IsPlaying()
			{
				if (NNS_SndHandleIsValid(m_SndHandle) == 1)
				{
					return true;
				}
				return false;
			}

			public bool Play(int nSeqArcNo, int nIndex, int nVolume, int nPan)
			{
				if (NNS_SndArcPlayerStartSeqArc(m_SndHandle, nSeqArcNo, nIndex) == 0)
				{
					OS_Printf("Sound : PlaySE( SEQARC = %d, INDEX = %d ) failed.\n", nSeqArcNo, nIndex);
					return false;
				}
				OS_Printf("Sound : PlaySE( SEQARC = %d, INDEX = %d ) succeed.\n", nSeqArcNo, nIndex);
				NNS_SndPlayerSetVolume(m_SndHandle, nVolume);
				NNS_SndPlayerSetTrackPan(m_SndHandle, ushort.MaxValue, nPan);
				return true;
			}

			public void Stop(int nFadeOutFrame)
			{
				NNS_SndPlayerStopSeq(m_SndHandle, nFadeOutFrame);
			}

			public void SetVolume(int nVolume)
			{
				NNS_SndPlayerSetVolume(m_SndHandle, nVolume);
			}
		}
	}
}
