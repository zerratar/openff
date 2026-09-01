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
		public static class Sound
		{
			public enum VOLUME_TYPE
			{
				VOLUME_MASTER,
				VOLUME_SLAVE,
				VOLUME_PROGRAM,
				VOLUME_OPTION
			}

			public const int SND_VOLUME_MAX = 127;

			public const int SND_VOLUME_MIN = 0;

			public const VOLUME_TYPE VOLUME_MASTER = VOLUME_TYPE.VOLUME_MASTER;

			public const VOLUME_TYPE VOLUME_SLAVE = VOLUME_TYPE.VOLUME_SLAVE;

			public const VOLUME_TYPE VOLUME_PROGRAM = VOLUME_TYPE.VOLUME_PROGRAM;

			public const VOLUME_TYPE VOLUME_OPTION = VOLUME_TYPE.VOLUME_OPTION;

			public static void Init(Array pHeapStart, uint u32HeapSize, string pArcPath, int nStrmPrio)
			{
				if (g_IsInit == 0)
				{
					NNS_SndInit();
					SoundHeap.Init(pHeapStart, u32HeapSize);
					SoundArchive.Init(pArcPath, nStrmPrio);
					g_OptionVolume = (g_ProgramVolume = (g_MasterVolume = (g_SlaveVolume = 128)));
					g_IsInit = 1;
				}
			}

			public static void Frame()
			{
				NNS_SndMain();
			}

			public static int ComputeSystemVolume()
			{
				return static_cast<int>(static_cast<float>(g_ProgramVolume) / 127f * static_cast<float>(g_OptionVolume) / 127f * static_cast<float>(g_SlaveVolume) / 127f * (float)g_MasterVolume);
			}

			public static void SetSystemVolume(VOLUME_TYPE VolumeType, ref int nVolume)
			{
				if (nVolume > 127)
				{
					nVolume = 127;
				}
				if (nVolume < 0)
				{
					nVolume = 0;
				}
				switch (VolumeType)
				{
				case VOLUME_TYPE.VOLUME_MASTER:
					g_MasterVolume = nVolume;
					break;
				case VOLUME_TYPE.VOLUME_SLAVE:
					g_SlaveVolume = nVolume;
					break;
				case VOLUME_TYPE.VOLUME_OPTION:
					g_OptionVolume = nVolume;
					break;
				case VOLUME_TYPE.VOLUME_PROGRAM:
					g_ProgramVolume = nVolume;
					break;
				}
				NNS_SndSetMasterVolume(ComputeSystemVolume());
			}

			public static int GetSystemVolume(VOLUME_TYPE VolumeType)
			{
				return VolumeType switch
				{
					VOLUME_TYPE.VOLUME_MASTER => g_MasterVolume, 
					VOLUME_TYPE.VOLUME_SLAVE => g_SlaveVolume, 
					VOLUME_TYPE.VOLUME_OPTION => g_OptionVolume, 
					VOLUME_TYPE.VOLUME_PROGRAM => g_ProgramVolume, 
					_ => 0, 
				};
			}

			public static void SetPlayerVolume(int nPlayerNo, int nVolume)
			{
				NNS_SndPlayerSetPlayerVolume(nPlayerNo, nVolume);
			}

			public static void Stop()
			{
				NNS_SndStopSoundAll();
			}

			public static void StopStream(int FadeFrame)
			{
				NNS_SndArcStrmStopAll(FadeFrame);
			}

			public static void StopPlayer(int nPlayerNo, int nFadeOutFrame)
			{
				NNS_SndPlayerStopSeqByPlayerNo(nPlayerNo, nFadeOutFrame);
			}

			public static bool LoadGroup(int nGroup)
			{
				if (SoundArchive.LoadGroup(nGroup) == 0)
				{
					OS_Printf("Sound : LoadGroup Failed. \n");
					return false;
				}
				return true;
			}

			public static void UnLoadGroup()
			{
				SoundArchive.UnLoadGroup();
			}

			public static void UnLoadGroup(int n)
			{
				if (n < 1)
				{
					n = 1;
				}
				while (SoundHeap.GetStackLevel() > n)
				{
					SoundArchive.UnLoadGroup();
				}
			}

			public static void ClearGroup()
			{
				while (SoundHeap.GetStackLevel() > 1)
				{
					SoundArchive.UnLoadGroup();
				}
			}

			public static int ReverbStart(Array pBuff, uint u32BuffSize, int Format, int nSampleRate, int nVolume)
			{
				return NNS_SndCaptureStartReverb(pBuff, u32BuffSize, Format, nSampleRate, nVolume);
			}

			public static void ReverbStop(int nFrame)
			{
				if (nFrame < 0)
				{
					OS_Printf("Sound : The value of the frame is illegal. Frame( %d )", nFrame);
				}
				else
				{
					NNS_SndCaptureStopReverb(nFrame);
				}
			}

			public static void SetReverbVolume(int nVolume, int nFrame)
			{
				if (nFrame < 0)
				{
					OS_Printf("Sound : The value of the frame is illegal. Frame( %d )", nFrame);
				}
				else
				{
					NNS_SndCaptureSetReverbVolume(nVolume, nFrame);
				}
			}

			public static float ComputeReverbDelay(uint BuffSize, int Format, int nSampleRate)
			{
				if (BuffSize < 0 || BuffSize > 524280)
				{
					OS_Printf("Sound : The value of buffer size is illegal. BuffSize( %d )", BuffSize);
					return -1f;
				}
				if (nSampleRate < 0)
				{
					OS_Printf("Sound : The value of sampling rate is illegal. SampleRate( %d )", nSampleRate);
					return -1f;
				}
				if (Format == 0)
				{
					return static_cast<float>(BuffSize) / static_cast<float>(nSampleRate);
				}
				if (Format == 0)
				{
					return static_cast<float>(BuffSize) / static_cast<float>(nSampleRate) / 2f;
				}
				return -1f;
			}
		}
	}
}
