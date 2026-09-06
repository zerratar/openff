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
	public static class MatrixSound
	{
		public class MtxSound
		{
			public static MtxSound _instance = new MtxSound();

			private MtxSoundImpl _pImplement;

			public MtxSound()
			{
				_pImplement = null;
			}

			public bool initialize(MtxSoundImpl pImplement, object pArg)
			{
				if (pImplement == null)
				{
					return false;
				}
				_pImplement = pImplement;
				return _pImplement.initialize(pArg);
			}

			public void finalize()
			{
				if (_pImplement != null)
				{
					_pImplement.finalize();
					_pImplement = null;
				}
			}

			public void update()
			{
				_pImplement.update();
				MtxSoundSE.getSingleton().update();
				MtxSoundBGM.getSingleton().update();
			}

			public static MtxSound getSingleton()
			{
				return _instance;
			}

			public MtxSoundImpl getImplement()
			{
				return _pImplement;
			}
		}

		public class MtxSoundImpl
		{
			public virtual bool initialize(object pArg)
			{
				return true;
			}

			public virtual void finalize()
			{
			}

			public virtual void update()
			{
			}
		}

		public class MtxSoundBGMImpl
		{
			public virtual bool initialize(Array pArg)
			{
				return true;
			}

			public virtual void finalize()
			{
			}

			public virtual void update()
			{
			}

			public virtual int play(int BGMNo, int Volume, int FadeinFrame, enMtxBGMSlot Slot)
			{
				return 1;
			}

			public virtual void stop(int FadeoutFrame, enMtxBGMSlot Slot)
			{
			}

			public virtual void pause(bool Flag, enMtxBGMSlot Slot)
			{
			}

			public virtual void setVolume(int Volume, int nFadeFrame, enMtxBGMSlot Slot)
			{
			}

			public virtual enMtxBGMState getState(enMtxBGMSlot Slot)
			{
				return enMtxBGMState.enMTX_BGM_STOP;
			}
		}

		public class MtxSoundBGM
		{
			public static MtxSoundBGM _instance = new MtxSoundBGM();

			private MtxSoundBGMImpl _pImplement;

			public MtxSoundBGM()
			{
				_pImplement = null;
			}

			public bool initialize(MtxSoundBGMImpl pImplement, Array pArg)
			{
				if (pImplement == null)
				{
					return false;
				}
				_pImplement = pImplement;
				return _pImplement.initialize(pArg);
			}

			public void finalize()
			{
				if (_pImplement != null)
				{
					_pImplement.finalize();
					_pImplement = null;
				}
			}

			public void update()
			{
				if (_pImplement != null)
				{
					_pImplement.update();
				}
			}

			public int play(int NbBGM, int Volume, int FadeinFrame, enMtxBGMSlot Slot)
			{
				int volume = MtxSound_Clamp(Volume, 0, 255);
				int fadeinFrame = MtxSound_Clamp(FadeinFrame, 0, 65535);
				if (_pImplement != null)
				{
					return _pImplement.play(NbBGM, volume, fadeinFrame, Slot);
				}
				return 0;
			}

			public void stop(int FadeoutFrame, enMtxBGMSlot Slot)
			{
				int fadeoutFrame = MtxSound_Clamp(FadeoutFrame, 0, 65535);
				if (_pImplement != null)
				{
					_pImplement.stop(fadeoutFrame, Slot);
				}
			}

			public void pause(bool Flag, enMtxBGMSlot Slot)
			{
				if (_pImplement != null)
				{
					_pImplement.pause(Flag, Slot);
				}
			}

			public void setVolume(int Volume, int FadeFrame, enMtxBGMSlot Slot)
			{
				if (_pImplement != null)
				{
					_pImplement.setVolume(Volume, FadeFrame, Slot);
				}
			}

			public enMtxBGMState getState(enMtxBGMSlot Slot)
			{
				if (_pImplement != null)
				{
					return _pImplement.getState(Slot);
				}
				return enMtxBGMState.enMTX_BGM_ERR;
			}

			public static MtxSoundBGM getSingleton()
			{
				return _instance;
			}

			public MtxSoundBGMImpl getImplement()
			{
				return _pImplement;
			}
		}

		public enum enMtxBGMState
		{
			enMTX_BGM_ERR = -1,
			enMTX_BGM_STOP,
			enMTX_BGM_PLAY,
			enMTX_BGM_PAUSE
		}

		public enum enMtxSEState
		{
			enMTX_SE_ERR = -1,
			enMTX_SE_STOP,
			enMTX_SE_PLAY
		}

		public enum enMtxVoiceState
		{
			enMTX_VOICE_ERR = -1,
			enMTX_VOICE_STOP,
			enMTX_VOICE_PLAY
		}

		public enum enMtxBGMSlot
		{
			enMTX_BGM_SLOT_EINVALID = -1,
			enMTX_BGM_SLOT0,
			enMTX_BGM_SLOT1,
			enMTX_BGM_SLOT2,
			enMTX_BGM_SLOT3,
			enMTX_BGM_MAX
		}

		public enum enMtxVoiceSlot
		{
			enMTX_VOICE_SLOT_EINVALID = -1,
			enMTX_VOICE_SLOT_0,
			enMTX_VOICE_SLOT_1,
			enMTX_VOICE_SLOT_2,
			enMTX_VOICE_SLOT_3,
			enMTX_VOICE_SLOT_MAX
		}

		public enum enMtxAmbientSlot
		{
			entMTX_AMB_SLOT_EINVALID = -1,
			entMTX_AMB_SLOT_0,
			entMTX_AMB_SLOT_1,
			entMTX_AMB_SLOT_2,
			entMTX_AMB_SLOT_3,
			entMTX_AMB_SLOT_MAX
		}

		public class MtxSEHandle
		{
		}

		public class MtxSoundSE
		{
			public static MtxSoundSE _instance = new MtxSoundSE();

			protected MtxSoundSEImpl _pImplement;

			public MtxSoundSE()
			{
				_pImplement = null;
			}

			public bool initialize(MtxSoundSEImpl pImplement, Array pArg)
			{
				if (pImplement == null)
				{
					return false;
				}
				_pImplement = pImplement;
				return _pImplement.initialize(pArg);
			}

			public void finalize()
			{
				if (_pImplement != null)
				{
					_pImplement.finalize();
					_pImplement = null;
				}
			}

			public void update()
			{
				if (_pImplement != null)
				{
					_pImplement.update();
				}
			}

			public MtxSEHandle play(int SENo, int Volume, int Pan)
			{
				int volume = MtxSound_Clamp(Volume, 0, 255);
				int pan = MtxSound_Clamp(Pan, 0, 255);
				if (_pImplement != null)
				{
					return _pImplement.play(SENo, volume, pan);
				}
				return null;
			}

			public void stop(MtxSEHandle Handle)
			{
				if (_pImplement != null)
				{
					_pImplement.stop(Handle, 0);
				}
			}

			public enMtxSEState getState(MtxSEHandle Handle)
			{
				if (Handle == null)
				{
					return enMtxSEState.enMTX_SE_ERR;
				}
				if (_pImplement != null)
				{
					return _pImplement.getState(Handle);
				}
				return enMtxSEState.enMTX_SE_ERR;
			}

			public static MtxSoundSE getSingleton()
			{
				return _instance;
			}

			public MtxSoundSEImpl getImplement()
			{
				return _pImplement;
			}
		}

		public class MtxSoundSEImpl
		{
			public virtual bool initialize(Array pArg)
			{
				return true;
			}

			public virtual void finalize()
			{
			}

			public virtual void update()
			{
			}

			public virtual MtxSEHandle play(int SENo, int Volume, int Pan)
			{
				return null;
			}

			public virtual void stop(MtxSEHandle Handle, int FadeoutFrame)
			{
			}

			public virtual enMtxSEState getState(MtxSEHandle Handle)
			{
				return enMtxSEState.enMTX_SE_STOP;
			}
		}

		public class MtxSoundAmbientImpl
		{
			public virtual bool initialize(Array arg0)
			{
				return true;
			}

			public virtual void finalize()
			{
			}

			public virtual void update()
			{
			}

			public virtual void play(int arg0, int arg1, int arg2, int arg3)
			{
			}

			public virtual void stop(int arg0, int arg1)
			{
			}

			public virtual void setVolume(int arg0, int arg1, int arg2)
			{
			}
		}

		public class MtxSoundAmbient
		{
			public static MtxSoundAmbient _instance = new MtxSoundAmbient();

			protected MtxSoundAmbientImpl _pImpl;

			public MtxSoundAmbient()
			{
				_pImpl = null;
			}

			public bool initialize(MtxSoundAmbientImpl pImpl, Array pArg)
			{
				if (pImpl != null)
				{
					pArg = pArg;
					return true;
				}
				return false;
			}

			public void finalize()
			{
			}

			public void update()
			{
			}

			public void play(int n, int nVol, int nFadein, int nSlot)
			{
			}

			public void stop(int nFadeout, int nSlot)
			{
			}

			public void setVolume(int nVol, int nFade, int nSlot)
			{
			}

			public MtxSoundAmbient singleton()
			{
				return _instance;
			}

			public MtxSoundAmbientImpl implement()
			{
				return _pImpl;
			}
		}

		public class MtxSoundBGMImplNDS : MtxSoundBGMImpl
		{
			public static int MtxBGM_NDS_NOT_PLAY = -1;

			private ds.BGMHandle[] _BGMHandle = new ds.BGMHandle[4];

			private int[] _PlayBGMNo = new int[4];

			public MtxSoundBGMImplNDS()
			{
				for (int i = 0; i < _BGMHandle.Length; i++)
				{
					_BGMHandle[i] = new ds.BGMHandle();
				}
			}

			public override bool initialize(Array pArg)
			{
				pArg = pArg;
				BGMInfoMng.getSingleton().initialize();
				for (int i = 0; 4 > i; i++)
				{
					_PlayBGMNo[i] = MtxBGM_NDS_NOT_PLAY;
				}
				return false;
			}

			public override void finalize()
			{
				for (int i = 0; 4 > i; i++)
				{
					_PlayBGMNo[i] = MtxBGM_NDS_NOT_PLAY;
				}
			}

			public override void update()
			{
			}

			private int computeBGMVolume(int Volume)
			{
				float num = opt.COptionManager.getSingleton().soundOption().bgmVolume();
				float num2 = opt.COptionManager.getSingleton().soundOption().bgmVolumeMax();
				return (int)((float)Volume * (num / num2) / 2f);
			}

			public override int play(int BGMNo, int Volume, int FadeinFrame, enMtxBGMSlot Slot)
			{
				if (_BGMHandle[(int)Slot].IsPlaying())
				{
					return 0;
				}
				BGMInfo bGMInfo = BGMInfoMng.getSingleton().getBGMInfo(BGMNo);
				if (bGMInfo == null)
				{
					return 0;
				}
				int seqNo = bGMInfo.getSeqNo();
				_BGMHandle[(int)Slot].Play(seqNo);
				_PlayBGMNo[(int)Slot] = BGMNo;
				_BGMHandle[(int)Slot].MoveVolume(Volume / 2, FadeinFrame);
				return 1;
			}

			public override void stop(int FadeoutFrame, enMtxBGMSlot Slot)
			{
				if (_BGMHandle[(int)Slot].IsPlaying())
				{
					_BGMHandle[(int)Slot].Stop(FadeoutFrame);
					_PlayBGMNo[(int)Slot] = MtxBGM_NDS_NOT_PLAY;
				}
			}

			public override void pause(bool Flag, enMtxBGMSlot Slot)
			{
				if (Flag)
				{
					_BGMHandle[(int)Slot].Pause(1);
				}
				if (!Flag)
				{
					_BGMHandle[(int)Slot].Pause(0);
				}
			}

			public override enMtxBGMState getState(enMtxBGMSlot Slot)
			{
				enMtxBGMState result = enMtxBGMState.enMTX_BGM_ERR;
				bool flag = _BGMHandle[(int)Slot].IsPlaying();
				bool flag2 = _BGMHandle[(int)Slot].IsPausing();
				if (!flag && !flag2)
				{
					result = enMtxBGMState.enMTX_BGM_STOP;
				}
				if (flag && !flag2)
				{
					result = enMtxBGMState.enMTX_BGM_PLAY;
				}
				if (flag && flag2)
				{
					result = enMtxBGMState.enMTX_BGM_PAUSE;
				}
				return result;
			}

			public override void setVolume(int Volume, int FadeFrame, enMtxBGMSlot Slot)
			{
				if (_BGMHandle[(int)Slot].IsPlaying())
				{
					_BGMHandle[(int)Slot].MoveVolume(Volume / 2, FadeFrame);
				}
			}

			public bool load(int BGMNo)
			{
				BGMInfo bGMInfo = BGMInfoMng.getSingleton().getBGMInfo(BGMNo);
				int num = ds.SoundArchive.LoadGroup(bGMInfo.getGroupNo());
				if (1 == num)
				{
					return true;
				}
				return false;
			}

			public bool load(int BGMNo, int Flag)
			{
				BGMInfo bGMInfo = BGMInfoMng.getSingleton().getBGMInfo(BGMNo);
				int num = NNS_SndArcLoadSeqEx(bGMInfo.getSeqNo(), (uint)Flag, ds.SoundHeap.GetHeapHandle());
				if (1 == num)
				{
					ds.SoundHeap.PushState();
					return true;
				}
				return false;
			}

			public void unload()
			{
				ds.SoundArchive.UnLoadGroup();
			}

			public int getPlayBGMNo(enMtxBGMSlot Slot)
			{
				return _PlayBGMNo[(int)Slot];
			}

			public void setBaseVolume(int Volume)
			{
				int playerNo = 0;
				NNS_SndPlayerSetPlayerVolume(playerNo, Volume);
			}
		}

		public class MtxSoundImplNDS : MtxSoundImpl
		{
			private MtxSoundImplNDSParam initParam_ = new MtxSoundImplNDSParam();

			public bool initialize(MtxSoundImplNDSParam pArg)
			{
				MtxSoundImplNDSParam mtxSoundImplNDSParam = reinterpret_cast<MtxSoundImplNDSParam>(pArg);
				NNS_SndInit();
				ds.SoundHeap.Init(mtxSoundImplNDSParam._pHeap, mtxSoundImplNDSParam._HeapSize);
				ds.SoundArchive.Init(mtxSoundImplNDSParam._ArcPath, mtxSoundImplNDSParam._StrmPrio);
				initParam_.copy(pArg);
				return true;
			}

			public override void finalize()
			{
				ds.SoundHeap.Final();
				initParam_.setDefault();
			}

			public override void update()
			{
				NNS_SndMain();
				ds.sound.SoundDivideLoader.getSingleton().updateRequests();
			}

			public void reset()
			{
				NNS_SndInit();
				ds.SoundHeap.Final();
				ds.SoundHeap.Init(initParam_._pHeap, initParam_._HeapSize);
				ds.SoundArchive.Init(initParam_._ArcPath, initParam_._StrmPrio);
			}

			public MtxSoundImplNDSParam getParam()
			{
				return initParam_;
			}
		}

		public class MtxSoundImplNDSParam
		{
			public Array _pHeap;

			public uint _HeapSize;

			public int _StrmPrio;

			public string _ArcPath;

			public MtxSoundImplNDSParam()
			{
			}

			public MtxSoundImplNDSParam(Array arg0, uint arg1, int arg2, string arg3)
			{
				_pHeap = arg0;
				_HeapSize = arg1;
				_StrmPrio = arg2;
				_ArcPath = arg3;
			}

			public void copy(MtxSoundImplNDSParam src)
			{
				_pHeap = src._pHeap;
				_HeapSize = src._HeapSize;
				_StrmPrio = src._StrmPrio;
				_ArcPath = src._ArcPath;
			}

			public void setDefault()
			{
				_pHeap = null;
				_HeapSize = 0u;
				_StrmPrio = 0;
				_ArcPath = "";
			}
		}

		public class MtxSEHandleNDS : MtxSEHandle
		{
			public ds.SEHandle _pSEObject;

			public MtxSEHandleNDS()
			{
				_pSEObject = null;
			}

			public void assignSEObject(ds.SEHandle pSEObject)
			{
				_pSEObject = const_cast<ds.SEHandle>(pSEObject);
			}
		}

		public class MtxSoundSEImplNDS : MtxSoundSEImpl
		{
			private const int MTX_SE_NDS_SE_MAX = 16;

			private ds.SEHandle[] _SEObject = new ds.SEHandle[16];

			private MtxSEHandleNDS[] _MtxSEHandle = new MtxSEHandleNDS[16];

			public MtxSoundSEImplNDS()
			{
				for (int i = 0; i < _SEObject.Length; i++)
				{
					_SEObject[i] = new ds.SEHandle();
				}
				for (int i = 0; i < _MtxSEHandle.Length; i++)
				{
					_MtxSEHandle[i] = new MtxSEHandleNDS();
				}
			}

			public override bool initialize(Array pArg)
			{
				pArg = pArg;
				for (int i = 0; i < 16; i++)
				{
					_MtxSEHandle[i].assignSEObject(_SEObject[i]);
				}
				return true;
			}

			public override void finalize()
			{
				for (int i = 0; i < 16; i++)
				{
					_MtxSEHandle[i].assignSEObject(null);
				}
			}

			public override void update()
			{
			}

			public override MtxSEHandle play(int SENo, int Volume, int Pan)
			{
				int num = -1;
				for (int i = 0; i < 16; i++)
				{
					if (!_SEObject[i].IsPlaying())
					{
						num = i;
						break;
					}
				}
				if (num < 0)
				{
					return null;
				}
				SEInfo sEInfo = SEInfoMng.getSingleton().getSEInfo(SENo);
				int nPan = Pan - 128;
				int num2 = 0;
				num2 = computeSEVolume();
				if (!_SEObject[num].Play(sEInfo.getSeqArcNo(), sEInfo.getSeqIndex(), num2, nPan))
				{
					return null;
				}
				return _MtxSEHandle[num];
			}

			public void stop(MtxSEHandle Handle)
			{
				if (Handle != null)
				{
					MtxSEHandleNDS mtxSEHandleNDS = (MtxSEHandleNDS)Handle;
					mtxSEHandleNDS._pSEObject.Stop(0);
				}
			}

			public override void stop(MtxSEHandle Handle, int FadeoutFrame)
			{
				if (Handle != null)
				{
					MtxSEHandleNDS mtxSEHandleNDS = (MtxSEHandleNDS)Handle;
					mtxSEHandleNDS._pSEObject.Stop(FadeoutFrame);
				}
			}

			public override enMtxSEState getState(MtxSEHandle Handle)
			{
				if (Handle == null)
				{
					return enMtxSEState.enMTX_SE_ERR;
				}
				MtxSEHandleNDS mtxSEHandleNDS = (MtxSEHandleNDS)Handle;
				bool flag = mtxSEHandleNDS._pSEObject.IsPlaying();
				enMtxSEState enMtxSEState2 = enMtxSEState.enMTX_SE_ERR;
				if (flag)
				{
					return enMtxSEState.enMTX_SE_PLAY;
				}
				return enMtxSEState.enMTX_SE_STOP;
			}

			public bool load(int SEGroupNo)
			{
				int num = ds.SoundArchive.LoadGroup(SEGroupNo);
				if (1 == num)
				{
					return true;
				}
				return false;
			}

			public bool loadAsync(int SEGroupNo)
			{
				if (1 == ds.SoundArchive.LoadGroupAsync(SEGroupNo))
				{
					return true;
				}
				return false;
			}

			public bool isLoadAsync()
			{
				if (ds.SoundArchive.IsLoadAsync() != 0)
				{
					return true;
				}
				return false;
			}

			public void unload()
			{
				ds.SoundArchive.UnLoadGroup();
			}

			public MtxSEHandle play(int SeqArcNo, int SeqNo, int Volume, int Pan)
			{
				int num = -1;
				for (int i = 0; i < 16; i++)
				{
					if (!_SEObject[i].IsPlaying())
					{
						num = i;
						break;
					}
				}
				if (num < 0)
				{
					return null;
				}
				int nPan = Pan - 128;
				int num2 = 0;
				num2 = computeSEVolume();
				if (!_SEObject[num].Play(SeqArcNo, SeqNo, num2, nPan))
				{
					return null;
				}
				return _MtxSEHandle[num];
			}

			public bool isPlaying(MtxSEHandle Handle)
			{
				if (Handle == null)
				{
					return false;
				}
				if (((MtxSEHandleNDS)Handle)._pSEObject != null)
				{
					return ((MtxSEHandleNDS)Handle)._pSEObject.IsPlaying();
				}
				return false;
			}
		}

		public const enMtxBGMState enMTX_BGM_ERR = enMtxBGMState.enMTX_BGM_ERR;

		public const enMtxBGMState enMTX_BGM_STOP = enMtxBGMState.enMTX_BGM_STOP;

		public const enMtxBGMState enMTX_BGM_PLAY = enMtxBGMState.enMTX_BGM_PLAY;

		public const enMtxBGMState enMTX_BGM_PAUSE = enMtxBGMState.enMTX_BGM_PAUSE;

		public const enMtxSEState enMTX_SE_ERR = enMtxSEState.enMTX_SE_ERR;

		public const enMtxSEState enMTX_SE_STOP = enMtxSEState.enMTX_SE_STOP;

		public const enMtxSEState enMTX_SE_PLAY = enMtxSEState.enMTX_SE_PLAY;

		public const enMtxBGMSlot enMTX_BGM_SLOT_EINVALID = enMtxBGMSlot.enMTX_BGM_SLOT_EINVALID;

		public const enMtxBGMSlot enMTX_BGM_SLOT0 = enMtxBGMSlot.enMTX_BGM_SLOT0;

		public const enMtxBGMSlot enMTX_BGM_SLOT1 = enMtxBGMSlot.enMTX_BGM_SLOT1;

		public const enMtxBGMSlot enMTX_BGM_SLOT2 = enMtxBGMSlot.enMTX_BGM_SLOT2;

		public const enMtxBGMSlot enMTX_BGM_SLOT3 = enMtxBGMSlot.enMTX_BGM_SLOT3;

		public const enMtxBGMSlot enMTX_BGM_MAX = enMtxBGMSlot.enMTX_BGM_MAX;

		public const int MTX_SOUND_VOL_MIN = 0;

		public const int MTX_SOUND_VOL_CENTER = 127;

		public const int MTX_SOUND_VOL_MAX = 255;

		public const int MTX_SOUND_PAN_MIN = 0;

		public const int MTX_SOUND_PAN_CENTER = 127;

		public const int MTX_SOUND_PAN_MAX = 255;

		public const int MTX_SOUND_FADE_MIN = 0;

		public const int MTX_SOUND_FADE_MAX = 65535;

		public static MtxSoundBGMImplNDS g_MtxBGMImplNDS = new MtxSoundBGMImplNDS();

		public static MtxSoundImplNDS g_MtxSoundImplNDS = new MtxSoundImplNDS();

		public static MtxSoundSEImplNDS g_MtxSEImplNDS = new MtxSoundSEImplNDS();

		internal static int MtxSound_Clamp(int Value, int Min, int Max)
		{
			if (Value < Min)
			{
				return Min;
			}
			if (Value > Max)
			{
				return Max;
			}
			return Value;
		}

		public static bool MtxBGMNDS_Load(int BGMNo)
		{
			MtxSoundBGMImplNDS mtxSoundBGMImplNDS = (MtxSoundBGMImplNDS)MtxSoundBGM.getSingleton().getImplement();
			return mtxSoundBGMImplNDS.load(BGMNo);
		}

		public static bool MtxBGMNDS_LoadEx(int BGMNo, int Flag)
		{
			MtxSoundBGMImplNDS mtxSoundBGMImplNDS = (MtxSoundBGMImplNDS)MtxSoundBGM.getSingleton().getImplement();
			return mtxSoundBGMImplNDS.load(BGMNo, Flag);
		}

		public static void MtxBGMNDS_Unload()
		{
			MtxSoundBGMImplNDS mtxSoundBGMImplNDS = (MtxSoundBGMImplNDS)MtxSoundBGM.getSingleton().getImplement();
			mtxSoundBGMImplNDS.unload();
		}

		public static int MtxBGMNDS_GetPlayBGMNo(enMtxBGMSlot Slot)
		{
			MtxSoundBGMImplNDS mtxSoundBGMImplNDS = (MtxSoundBGMImplNDS)MtxSoundBGM.getSingleton().getImplement();
			return mtxSoundBGMImplNDS.getPlayBGMNo(Slot);
		}

		public static void MtxBGMNDS_SetBaseVolume(int Volume)
		{
			((MtxSoundBGMImplNDS)MtxSoundBGM.getSingleton().getImplement())?.setBaseVolume(Volume);
		}

		internal static void MtxSoundNDS_Unload(int nLv)
		{
			ds.Sound.UnLoadGroup(nLv);
		}

		internal static int MtxSoundNDS_getHeapLV()
		{
			return ds.SoundHeap.GetStackLevel();
		}

		internal static void MtxSoundNDS_reset()
		{
			MtxSoundImplNDS mtxSoundImplNDS = (MtxSoundImplNDS)MtxSound.getSingleton().getImplement();
			mtxSoundImplNDS.reset();
		}

		internal static void MtxSoundNDS_StopAll()
		{
			ds.Sound.Stop();
		}

		internal static MtxSoundImplNDSParam MtxSoundNDS_getParam()
		{
			MtxSoundImplNDS mtxSoundImplNDS = (MtxSoundImplNDS)MtxSound.getSingleton().getImplement();
			return mtxSoundImplNDS.getParam();
		}

		public static int computeSEVolume()
		{
			float num = opt.COptionManager.getSingleton().soundOption().seVolume();
			float num2 = opt.COptionManager.getSingleton().soundOption().seVolumeMax();
			return (int)(192f * (num / num2) / 2f);
		}

		public static bool MtxSENDS_Load(int SEGroupNo)
		{
			return ((MtxSoundSEImplNDS)MtxSoundSE.getSingleton().getImplement())?.load(SEGroupNo) ?? false;
		}

		internal static bool MtxSENDS_LoadAsync(int SEGroupNo)
		{
			return ((MtxSoundSEImplNDS)MtxSoundSE.getSingleton().getImplement())?.loadAsync(SEGroupNo) ?? false;
		}

		internal static bool MtxSENDS_IsLoadAsync()
		{
			return ((MtxSoundSEImplNDS)MtxSoundSE.getSingleton().getImplement())?.isLoadAsync() ?? false;
		}

		internal static void MtxSENDS_Unload()
		{
			((MtxSoundSEImplNDS)MtxSoundSE.getSingleton().getImplement())?.unload();
		}

		internal static MtxSEHandle MtxSENDS_Play(int SeqArcNo, int SeqNo, int Volume, int Pan)
		{
			MtxSoundSEImplNDS mtxSoundSEImplNDS = (MtxSoundSEImplNDS)MtxSoundSE.getSingleton().getImplement();
			return mtxSoundSEImplNDS.play(SeqArcNo, SeqNo, Volume, Pan);
		}

		internal static void MtxSENDS_Stop(MtxSEHandle Handle, int FadeoutFrame)
		{
			if (Handle != null)
			{
				MtxSoundSEImplNDS mtxSoundSEImplNDS = (MtxSoundSEImplNDS)MtxSoundSE.getSingleton().getImplement();
				mtxSoundSEImplNDS.stop(Handle, FadeoutFrame);
			}
		}

		internal static bool MtxSENDS_isPlaying(MtxSEHandle Handle)
		{
			MtxSoundSEImplNDS mtxSoundSEImplNDS = (MtxSoundSEImplNDS)MtxSoundSE.getSingleton().getImplement();
			return mtxSoundSEImplNDS.isPlaying(Handle);
		}
	}
}
