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
		public class MovieHandleDS
		{
			private FSFile fileUp_ = new FSFile();

			private FSFile fileLo_ = new FSFile();

			private int handleUp_;

			private int handleLo_;

			private SoundBuffer sndBufL_ = new SoundBuffer();

			private SoundBuffer sndBufR_ = new SoundBuffer();

			private uint soundPacketsPerFrame_;

			private uint soundPacketsPushed_;

			public uint soundPacketsStreamed_;

			private uint soundPacketsBuffer_;

			private bool soundStarted_;

			private bool frameSkipDone_;

			private int frameBuffered_;

			private int frameBufferedMaxUpper_;

			private int frameBufferedMaxLower_;

			private int frameTotal_;

			private int frameCurrent_;

			private bool loopEnable_;

			private bool playing_;

			public bool sleeping_;

			private MHDSNotifier pNotifier_;

			private PMSleepCallbackInfo preSleepCallbackInfo_;

			private PMSleepCallbackInfo postSleepCallbackInfo_;

			public MovieHandleDS()
			{
				handleUp_ = 0;
				handleLo_ = 0;
				soundPacketsPerFrame_ = 0u;
				soundPacketsStreamed_ = 0u;
				soundPacketsBuffer_ = 0u;
				soundStarted_ = false;
				frameSkipDone_ = false;
				frameBuffered_ = 0;
				frameBufferedMaxUpper_ = 0;
				frameBufferedMaxLower_ = 0;
				frameTotal_ = 0;
				frameCurrent_ = 0;
				loopEnable_ = false;
				playing_ = false;
				pNotifier_ = null;
				sndBufL_.buffer_ = (sndBufR_.buffer_ = null);
			}

			~MovieHandleDS()
			{
			}

			public void destruct()
			{
				if (sndBufL_.buffer_ != null)
				{
					DSVX_SoundFree(sndBufL_.buffer_);
				}
				if (sndBufR_.buffer_ != null)
				{
					DSVX_SoundFree(sndBufR_.buffer_);
				}
			}

			public bool init(string SrcUp, string SrcLo, MHDSNotifier pNotifier, bool Loop)
			{
				if (SrcUp == null || SrcLo == null)
				{
					return false;
				}
				OS_GetConsoleType();
				if (0 == 0)
				{
					return false;
				}
				FS_InitFile(fileUp_);
				FS_InitFile(fileLo_);
				if (FS_OpenFile(fileUp_, SrcUp) == 0 || FS_OpenFile(fileLo_, SrcLo) == 0)
				{
					FS_CloseFile(fileUp_);
					FS_CloseFile(fileLo_);
					return false;
				}
				if (Loop)
				{
					handleUp_ = VX_OpenMovieFromFile(fileUp_, 0, 0);
					handleLo_ = VX_OpenMovieFromFile(fileLo_, 0, 0);
					loopEnable_ = true;
				}
				else
				{
					handleUp_ = VX_OpenMovieFromFile(fileUp_, 9, 0);
					handleLo_ = VX_OpenMovieFromFile(fileLo_, 9, 0);
					loopEnable_ = false;
				}
				if (handleUp_ == 0 || handleLo_ == 0)
				{
					FS_CloseFile(fileUp_);
					FS_CloseFile(fileLo_);
					return false;
				}
				frameBuffered_ = 0;
				frameBufferedMaxUpper_ = 9;
				frameBufferedMaxLower_ = 9;
				frameTotal_ = (int)VX_GetNbFrame(handleUp_);
				frameCurrent_ = 0;
				frameSkipDone_ = false;
				long num = (long)VX_GetAudioFrequency(handleUp_) * 65536L;
				VX_GetVideoFps(handleUp_);
				soundPacketsPerFrame_ = (uint)(num / 0u + 1);
				soundPacketsBuffer_ = (uint)(soundPacketsPerFrame_ * (frameBufferedMaxUpper_ + 1));
				soundPacketsPushed_ = 0u;
				soundPacketsStreamed_ = 0u;
				soundStarted_ = false;
				if (1 == VX_GetNbAudioTrack(handleUp_))
				{
					sndBufL_.bufferSize_ = 0u;
					sndBufL_.bufferIndex_ = 0u;
					sndBufL_.buffer_ = (short[])DSVX_SoundMalloc(sndBufL_.bufferSize_);
					MI_CpuClearFast(sndBufL_.buffer_, (int)sndBufL_.bufferSize_);
				}
				else
				{
					sndBufL_.bufferSize_ = 0u;
					sndBufL_.bufferIndex_ = 0u;
					sndBufL_.buffer_ = (short[])DSVX_SoundMalloc(sndBufL_.bufferSize_);
					MI_CpuClearFast(sndBufL_.buffer_, (int)sndBufL_.bufferSize_);
					sndBufR_.bufferSize_ = 0u;
					sndBufR_.bufferIndex_ = 0u;
					sndBufR_.buffer_ = (short[])DSVX_SoundMalloc(sndBufR_.bufferSize_);
					MI_CpuClearFast(sndBufR_.buffer_, (int)sndBufR_.bufferSize_);
				}
				loopEnable_ = Loop;
				playing_ = false;
				sleeping_ = false;
				if (pNotifier != null)
				{
					pNotifier_ = pNotifier;
				}
				return true;
			}

			public void final()
			{
				stop();
				VX_CloseMovie(handleUp_);
				VX_CloseMovie(handleLo_);
				FS_CloseFile(fileUp_);
				FS_CloseFile(fileLo_);
				if (sndBufL_.buffer_ != null)
				{
					memset(sndBufL_.buffer_, 0, (int)sndBufL_.bufferSize_);
					DSVX_SoundFree(sndBufL_.buffer_);
					sndBufL_.bufferSize_ = 0u;
					sndBufL_.bufferIndex_ = 0u;
					sndBufL_.buffer_ = null;
				}
				if (sndBufR_.buffer_ != null)
				{
					memset(sndBufR_.buffer_, 0, (int)sndBufR_.bufferSize_);
					DSVX_SoundFree(sndBufR_.buffer_);
					sndBufR_.bufferSize_ = 0u;
					sndBufR_.bufferIndex_ = 0u;
					sndBufR_.buffer_ = null;
				}
			}

			public void play()
			{
				OS_CreateAlarm(g_DSVXAlarm);
				OS_SetPeriodicAlarm(g_DSVXAlarm, (int)(OS_GetTick() + ALARM_COUNT(10uL)), (int)ALARM_COUNT((ulong)(65536000000L / (long)VX_GetVideoFps(handleUp_))), DSVX_AlarmIntr, null);
				playing_ = true;
				while (VX_ReadFrame(handleUp_) != 0 && VX_ReadFrame(handleLo_) != 0 && playing_)
				{
					if (PAD_DetectFold() == 1)
					{
						if (!sleeping_ && PM_GetLCDPower() == 0)
						{
							playing_ = false;
						}
					}
					else if (sleeping_ && PM_GetLCDPower() == 0 && PM_SetLCDPower(0))
					{
						sleeping_ = false;
					}
					VX_UnpackFrameImage(handleUp_);
					VX_UnpackFrameImage(handleLo_);
					frameBuffered_++;
					ulong num = VX_GetFrameNbAudioPacket(handleUp_);
					for (int i = 0; i < (int)num; i++)
					{
						if (soundStarted_)
						{
							ulong num2 = 0uL;
							do
							{
								num2 = soundPacketsPushed_ - soundPacketsStreamed_;
								num2 = soundPacketsBuffer_ - num2;
							}
							while (num2 == 0);
						}
						if (1 == VX_GetNbAudioTrack(handleUp_))
						{
							SoundBuffer soundBuffer = sndBufL_;
							soundBuffer.bufferIndex_ = soundBuffer.bufferIndex_;
							if (sndBufL_.bufferIndex_ == 0)
							{
								sndBufL_.bufferIndex_ = 0u;
							}
						}
						else
						{
							SoundBuffer soundBuffer2 = sndBufL_;
							soundBuffer2.bufferIndex_ = soundBuffer2.bufferIndex_;
							SoundBuffer soundBuffer3 = sndBufR_;
							soundBuffer3.bufferIndex_ = soundBuffer3.bufferIndex_;
							if (sndBufL_.bufferIndex_ == 0)
							{
								sndBufL_.bufferIndex_ = 0u;
							}
							if (sndBufR_.bufferIndex_ == 0)
							{
								sndBufR_.bufferIndex_ = 0u;
							}
						}
						soundPacketsPushed_++;
					}
					if (!soundStarted_ && frameBuffered_ >= frameBufferedMaxUpper_)
					{
						startSound();
					}
					if (frameBuffered_ >= frameBufferedMaxUpper_)
					{
						int num3 = (int)(soundPacketsPushed_ - soundPacketsStreamed_);
						if ((num3 <= soundPacketsPerFrame_ * 3 && !frameSkipDone_) || sleeping_)
						{
							VX_SkipFrameImage(handleUp_);
							VX_SkipFrameImage(handleLo_);
							frameSkipDone_ = true;
						}
						else
						{
							while (DSVX_getFlipStatus() == 0)
							{
							}
							while (DSVX_getBlitImageFlag() != 1)
							{
							}
							DSVX_resetBlitImageFlag();
							VX_BlitFrameImage(handleUp_, DSVX_GetMainBackBuffer(), 256);
							VX_BlitFrameImage(handleLo_, DSVX_GetSubBackBuffer(), 256);
							frameSkipDone_ = false;
							DSVX_resetFlipStatus();
						}
					}
					if (++frameCurrent_ == frameTotal_)
					{
						if (!loopEnable_)
						{
							break;
						}
						frameCurrent_ = 0;
						VX_JumpBeginning(handleUp_);
						VX_JumpBeginning(handleLo_);
					}
					if (pNotifier_ != null)
					{
						pNotifier_.updateHandler();
					}
				}
				stopSound();
				OS_CancelAlarm(g_DSVXAlarm);
			}

			public void stop()
			{
				playing_ = false;
			}

			public void startSound()
			{
				if (1 == VX_GetNbAudioTrack(handleUp_))
				{
					startSoundMono();
				}
				else
				{
					startSoundSte();
				}
				soundStarted_ = true;
			}

			public void startSoundMono()
			{
				_ = soundPacketsStreamed_ % soundPacketsBuffer_;
				uint num = 0u;
				if (num != 0)
				{
					for (int num2 = 0; num2 < num; num2 = num2)
					{
						short[] p = new short[0];
						MI_CpuCopy8(sndBufL_.buffer_, p, 0);
						for (int i = 0; i < 0u; i++)
						{
						}
					}
					soundPacketsPushed_ -= soundPacketsStreamed_;
					soundPacketsStreamed_ = 0u;
					sndBufL_.bufferIndex_ = 0u;
				}
				uint arg = 0 / VX_GetAudioFrequency(handleUp_);
				uint num3 = 0u;
				DC_StoreRange(sndBufL_.buffer_, 0u);
				SND_LockChannel(1uL, 0);
				SND_SetupChannelPcm(0, 0, sndBufL_.buffer_, 0, 0, 0, 127, 0, (int)arg, 64);
				SND_SetupAlarm(0, (int)num3, (int)num3, soundCallback, this);
				SND_StartTimer(1, 0, 1, 0);
				SND_FlushCommand(0);
			}

			public void startSoundSte()
			{
				_ = soundPacketsStreamed_ % soundPacketsBuffer_;
				uint num = 0u;
				if (num != 0)
				{
					for (int num2 = 0; num2 < num; num2 = num2)
					{
						short[] p = new short[0];
						MI_CpuCopy8(sndBufL_.buffer_, p, 0);
						for (int i = 0; i < 0u; i++)
						{
						}
						short[] p2 = new short[0];
						MI_CpuCopy8(sndBufR_.buffer_, p2, 0);
						for (int j = 0; j < 0u; j++)
						{
						}
					}
					soundPacketsPushed_ -= soundPacketsStreamed_;
					soundPacketsStreamed_ = 0u;
					sndBufL_.bufferIndex_ = (sndBufR_.bufferIndex_ = 0u);
				}
				else
				{
					MI_CpuClear8(sndBufL_.buffer_, 0);
					MI_CpuClear8(sndBufR_.buffer_, 0);
				}
				uint arg = 0 / VX_GetAudioFrequency(handleUp_);
				uint num3 = 0u;
				DC_StoreRange(sndBufL_.buffer_, 0u);
				DC_StoreRange(sndBufR_.buffer_, 0u);
				SND_LockChannel(3uL, 0);
				SND_SetupChannelPcm(0, 0, sndBufL_.buffer_, 0, 0, 0, 110, 0, (int)arg, 34);
				SND_SetupChannelPcm(1, 0, sndBufR_.buffer_, 0, 0, 0, 110, 0, (int)arg, 94);
				SND_SetupAlarm(0, (int)num3, (int)num3, soundCallback, this);
				SND_SetupAlarm(1, (int)num3, (int)num3, soundCallback, this);
				SND_StartTimer(3, 0, 1, 0);
				SND_FlushCommand(0);
			}

			public void stopSound()
			{
				if (1 == VX_GetNbAudioTrack(handleUp_))
				{
					SND_StopTimer(1, 0, 1, 0);
					SND_UnlockChannel(1, 0);
				}
				else
				{
					SND_StopTimer(3, 0, 1, 0);
					SND_UnlockChannel(3, 0);
				}
				SND_FlushCommand(0);
			}
		}
	}
}
