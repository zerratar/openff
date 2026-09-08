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
	public static class sys
	{
		public class GamePartSystem<SPc>
		{
			public static int InvalidPart = -1;

			private GamePart<SPc> currentPart_;

			private int currentPartID_;

			private int previousPartID_;

			private int nextPartID_;

			private int softResetPartID_;

			private int numSleeps_;

			private GamePart<SPc>[] parts_ = new GamePart<SPc>[36];

			private int[] sleepParts_ = new int[4];

			private bool firstTransit_;

			public bool firstTransit()
			{
				return firstTransit_;
			}

			protected virtual void routineWork()
			{
			}

			protected virtual void changePart(bool bSoftReset)
			{
			}

			protected virtual bool isSoftResetKeyPushed()
			{
				return false;
			}

			public GamePartSystem()
			{
				currentPart_ = null;
				currentPartID_ = InvalidPart;
				previousPartID_ = InvalidPart;
				nextPartID_ = InvalidPart;
				softResetPartID_ = InvalidPart;
				numSleeps_ = 0;
			}

			public void run(int start_part)
			{
				currentPartID_ = start_part;
				currentPart_ = getPart(currentPartID_);
				firstTransit_ = false;
				if (currentPart_.isSleeping())
				{
					currentPart_.wakeUp();
				}
				else
				{
					currentPart_.start();
				}
			}

			public void loop(int start_part)
			{
				bool bSoftReset = false;
				if (currentPart_.isExecutable())
				{
					routineWork();
					currentPart_.execute();
					return;
				}
				if (currentPart_.isSleeping())
				{
					currentPart_.sleep();
					sleepParts_[numSleeps_++] = currentPartID_;
				}
				else
				{
					currentPart_.end();
				}
				changePart(bSoftReset);
				int num = InvalidPart;
				previousPartID_ = currentPartID_;
				if (nextPartID_ == InvalidPart)
				{
					if (numSleeps_ != 0)
					{
						numSleeps_--;
						num = sleepParts_[numSleeps_];
					}
				}
				else
				{
					num = nextPartID_;
				}
				if (num != InvalidPart)
				{
					currentPartID_ = num;
					currentPart_ = getPart(num);
					nextPartID_ = InvalidPart;
					firstTransit_ = false;
					if (currentPart_.isSleeping())
					{
						currentPart_.wakeUp();
					}
					else
					{
						currentPart_.start();
					}
				}
			}

			public int getCurrentPart()
			{
				return currentPartID_;
			}

			public int getPreviousPart()
			{
				return previousPartID_;
			}

			public int getNextPart()
			{
				if (nextPartID_ == InvalidPart && numSleeps_ != 0)
				{
					return sleepParts_[numSleeps_];
				}
				return nextPartID_;
			}

			public void setNextPart(int next)
			{
				nextPartID_ = next;
				firstTransit_ = true;
			}

			public int sendMessage(int part, MsgCode msg, uint param1, uint param2)
			{
				return parts_[part].receiveMessage(msg, param1, param2);
			}

			public void setPartAfterSoftReset(int part)
			{
				softResetPartID_ = part;
			}

			public void registerPart(int _id, GamePart<SPc> part)
			{
				parts_[_id] = part;
			}

			public void deregisterPart(int _id)
			{
				parts_[_id] = null;
			}

			public GamePart<SPc> getPart(int _id)
			{
				return parts_[_id];
			}
		}

		public class FF3PartSystem : GamePartSystem<FF3GamePartSystemPolicy>
		{
			public void softReset()
			{
			}

			public void setSoftResetProhibit(bool bProhibit)
			{
			}

			protected override void routineWork()
			{
			}

			protected override void changePart(bool bSoftReset)
			{
				GGlobal.changePart(bSoftReset);
			}

			protected override bool isSoftResetKeyPushed()
			{
				return false;
			}
		}

		public class CBlankTask
		{
			public static int LIMIT_OF_TASK = 8;

			public void btInitialize()
			{
				for (int num = g_tvVertical.size() - 1; num >= 0; num--)
				{
					g_tvVertical[num].btVNoticeForcedEnd();
				}
				g_tvVertical.clear();
				for (int num2 = g_tvHorizontal.size() - 1; num2 >= 0; num2--)
				{
					g_tvHorizontal[num2].btHNoticeForcedEnd();
				}
				g_tvHorizontal.clear();
			}

			public static void btVTask()
			{
				for (int num = g_tvVertical.size() - 1; num >= 0; num--)
				{
					g_tvVertical[num].vbTask();
				}
			}

			public void btHTask(ushort line)
			{
				for (int num = g_tvHorizontal.size() - 1; num >= 0; num--)
				{
					g_tvHorizontal[num].hbTask(line);
				}
			}

			~CBlankTask()
			{
				endVTask();
				endHTask();
			}

			public void beginVTask()
			{
				for (int num = g_tvVertical.size() - 1; num >= 0; num--)
				{
					if (g_tvVertical[num] == this)
					{
						return;
					}
				}
				g_tvVertical.push_back(this);
			}

			public void endVTask()
			{
				for (int num = g_tvVertical.size() - 1; num >= 0; num--)
				{
					if (g_tvVertical[num] == this)
					{
						g_tvVertical.erase(num);
					}
				}
			}

			public void beginHTask()
			{
				for (int num = g_tvHorizontal.size() - 1; num >= 0; num--)
				{
					if (g_tvHorizontal[num] == this)
					{
						return;
					}
				}
				g_tvHorizontal.push_back(this);
			}

			public void endHTask()
			{
				for (int num = g_tvHorizontal.size() - 1; num >= 0; num--)
				{
					if (g_tvHorizontal[num] == this)
					{
						g_tvHorizontal.erase(num);
					}
				}
			}

			public virtual void vbTask()
			{
			}

			public virtual void hbTask(ushort line)
			{
			}

			public virtual void btVNoticeForcedEnd()
			{
			}

			public virtual void btHNoticeForcedEnd()
			{
			}
		}

		public class GGlobal
		{
			public static void initialize()
			{
				setSoftResetProhibit(bProhibit: true);
				ds.CDevice.initialize();
				ds.CHeap.initialize((uint)ds.CHeap.DEF_SYSTEM_HEAP_SIZE);
				ds.CVram.initialize();
				ds.g_TouchPanel.setDoubleClickDelay(4);
				dv.CDeviceManager.getInstance().initialize();
				ds.CHeap.setID_app(255);
				ds.CHeap.setAllocMode_app(0);
				ovl.overlayRegister.initialize();
				part.partRegister.initialize();
				ds.RandomNumber.init(OS_GetVBlankCount(), OS_GetVBlankCount());
				ds.g_Pad.setAutoDelay(30u);
				ds.g_Pad.setRepeatInterval(2u);
				ds.g_TouchPanel.setRepeatDelay(10);
				ds.g_TouchPanel.setRepeatInterval(2);
				if (OpenFF.Client.GameProfile.Ff3Party)
				{
					pl.PlayerParty.instance().load();
				}
				pl.PlayerParty.instance().initialize();
				pl.PlayerParty.instance().addPlayer(0);
				// OpenFF: a mod's character definitions on the fresh party (defs/characters).
				OpenFF.Client.ModCharactersLayer.ApplyToNewParty();
				OptionSaveDataGlobal.getSingleton().setup();
				if (OptionSaveDataGlobal.getSingleton().isProper())
				{
					OptionSaveDataGlobal.getSingleton().reflect();
				}
				else
				{
					opt.COptionManager.getSingleton().initialize();
					card.SaveOption();
				}
				int num = 0;
				num = 524288;
				ds.CHeap.chmode_app(b: false);
				Array arg = ds.CHeap.alloc_app((uint)num);
				ds.CHeap.chmode_app(b: true);
				MatrixSound.MtxSoundImplNDSParam pArg = new MatrixSound.MtxSoundImplNDSParam(arg, (uint)num, 10, "sound_data.sdat");
				MatrixSound.MtxSound.getSingleton().initialize(MatrixSound.g_MtxSoundImplNDS, pArg);
				MatrixSound.MtxSoundSE.getSingleton().initialize(MatrixSound.g_MtxSEImplNDS, null);
				MatrixSound.MtxSoundBGM.getSingleton().initialize(MatrixSound.g_MtxBGMImplNDS, null);
				MatrixSound.MtxSENDS_Load(0);
				int[][] array = new int[2][]
				{
					new int[1] { 16 },
					new int[1] { 12 }
				};
				dgs.DGSMessageInitializeFont();
				int font = dgs.DGSMessageAssignFont(array[0]);
				int font2 = dgs.DGSMessageAssignFont(array[1]);
				dgs.msg.CMessageSys.getInstance().initialize();
				dgs.msg.CMessageSys.getInstance().Main().setUpMSF(font, dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_12x12);
				dgs.msg.CMessageSys.getInstance().Main().setUpMSF(font2, dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_8x8);
				dgs.msg.CMessageSys.getInstance().Sub().setUpMSF(font, dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_12x12);
				dgs.msg.CMessageSys.getInstance().Sub().setUpMSF(font2, dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_8x8);
				ds.CHeap.setID_app(0);
				CARDBackupType type = CARDBackupType.CARD_BACKUP_TYPE_EEPROM_512KBITS;
				card.Manager.GetInstance().Initialize(type, 13842u, 3, 1, null);
				ds.GlobalPlayTimeCounter.getSingleton().start();
				ds.fs.FileDivideLoader.getSingleton().beginning();
				ds.sound.SoundDivideLoader.getSingleton().beginning();
				MovieFileRegister.getSingleton().beginning();
				evt.CEventRestriction.getSingleton().beginning();
				menu.MenuManager.getSingleton().beginning();
			}

			public static void run(GAMEPART start_part)
			{
				FF3PartSys.run((int)part.partRegister.getGamePart());
			}

			public static void loop(GAMEPART start_part)
			{
				FF3PartSys.loop((int)start_part);
			}

			public GAMEPART getCurrentPart()
			{
				return (GAMEPART)FF3PartSys.getCurrentPart();
			}

			public static GAMEPART getPreviousPart()
			{
				return (GAMEPART)FF3PartSys.getPreviousPart();
			}

			public static GAMEPART getNextPart()
			{
				return (GAMEPART)FF3PartSys.getNextPart();
			}

			public static void setNextPart(GAMEPART next)
			{
				FF3PartSys.setNextPart((int)next);
			}

			public int sendMessage(GAMEPART part, int msg, uint param1, uint param2)
			{
				return FF3PartSys.sendMessage((int)part, (MsgCode)msg, param1, param2);
			}

			public static void setPartAfterSoftReset(GAMEPART part)
			{
				FF3PartSys.setPartAfterSoftReset((int)part);
			}

			public void iniitalizeGameState(bool bSoftReset)
			{
				SystemObserver.resetObserver(bSoftReset);
			}

			public static void changePart(bool bSoftReset)
			{
			}

			public void softReset()
			{
				FF3PartSys.softReset();
			}

			public static void setSoftResetProhibit(bool bProhibit)
			{
				FF3PartSys.setSoftResetProhibit(bProhibit);
			}

			public static void registerPart(GAMEPART _id, FF3GamePart part)
			{
				FF3PartSys.registerPart((int)_id, part);
			}

			public void deregisterPart(GAMEPART _id)
			{
				FF3PartSys.deregisterPart((int)_id);
			}

			public FF3GamePart getPart(GAMEPART _id)
			{
				return (FF3GamePart)FF3PartSys.getPart((int)_id);
			}
		}

		public class CommonData
		{
			public enum Index
			{
				kPGP,
				kCharacterParameter,
				kEffectFile,
				kFont,
				kLipsync,
				kLast
			}
		}

		public class GamePart<SPc>
		{
			private bool partSleepFlag_;

			private bool partEndFlag_;

			protected virtual void doInitialize()
			{
			}

			protected virtual void doUninitialize()
			{
			}

			protected virtual void doSleep()
			{
			}

			protected virtual void doWakeUp()
			{
			}

			protected virtual void initOnFrame()
			{
			}

			protected virtual void finishOnFrame()
			{
			}

			protected virtual void doSomething()
			{
			}

			public GamePart()
			{
				partSleepFlag_ = false;
				partEndFlag_ = true;
			}

			~GamePart()
			{
			}

			public void abort()
			{
				partEndFlag_ = true;
			}

			public void yield()
			{
				if (!partEndFlag_)
				{
					partSleepFlag_ = true;
				}
			}

			public bool isExecutable()
			{
				if (!partEndFlag_)
				{
					return !isSleeping();
				}
				return false;
			}

			public bool isSleeping()
			{
				return partSleepFlag_;
			}

			public void start()
			{
				partEndFlag_ = false;
				doInitialize();
			}

			public void end()
			{
				partSleepFlag_ = false;
				doUninitialize();
				partEndFlag_ = true;
			}

			public void sleep()
			{
				doSleep();
			}

			public void wakeUp()
			{
				partSleepFlag_ = false;
				doWakeUp();
			}

			public void execute()
			{
				initOnFrame();
				doSomething();
				finishOnFrame();
			}

			public int receiveMessage(MsgCode msg, uint param1, uint param2)
			{
				return onReceiveMessage(msg, param1, param2);
			}

			public int onReceiveMessage(MsgCode msg, uint param1, uint param2)
			{
				return 0;
			}
		}

		public class FF3GamePart : GamePart<FF3GamePartSystemPolicy>
		{
			public uint bufferMode;

			public void setBufferMode(int mode)
			{
				bufferMode = (uint)mode;
			}

			protected override void doSomething()
			{
				ds.CDevice.CheckSleepMode();
				bool flag = canExecute();
				bool flag2 = canDraw();
				if (flag)
				{
					onPreExecutePart();
				}
				if (flag2)
				{
					gameSystemDraw();
					onDrawPart();
				}
				if (flag)
				{
					gameSystemExecute();
					onExecutePart();
				}
				if (flag2)
				{
					onDraw2ndPart();
				}
				if (flag2)
				{
					gameSystemUpdate();
					onUpdatePart();
					gameSystemSyncDraw();
					waitDrawFinish();
				}
				if (flag2)
				{
					gameSystemDrawWindow();
					onDrawEffector();
				}
				if (flag)
				{
					onPostExecutePart();
					dgs.CCurtain.execute();
					dgs.CFade.execute();
					MatrixSound.MtxSound.getSingleton().update();
					ds.GlobalPlayTimeCounter.getSingleton().update();
				}
				waitDrawFinish();
			}

			public void waitDrawFinish()
			{
			}

			protected override void initOnFrame()
			{
				ds.CDevice.singleton().waitVBlank();
				ds.g_Pad.update();
				ds.g_TouchPanel.update();
				G3X_ClearFifo();
				G3X_Reset();
				G3X_ResetMtxStack();
				ds.fs.FileDivideLoader.getSingleton().updateRequests();
			}

			protected override void finishOnFrame()
			{
				ds.CDevice.singleton().present();
			}

			public void gameSystemDraw()
			{
			}

			public void gameSystemSyncDraw()
			{
			}

			public void gameSystemExecute()
			{
			}

			public void gameSystemUpdate()
			{
			}

			public void gameSystemDrawWindow()
			{
			}

			public bool canExecute()
			{
				return true;
			}

			public bool canDraw()
			{
				return true;
			}

			protected virtual void onPreExecutePart()
			{
			}

			protected virtual void onDrawPart()
			{
			}

			protected virtual void onExecutePart()
			{
			}

			protected virtual void onDraw2ndPart()
			{
			}

			protected virtual void onUpdatePart()
			{
			}

			protected virtual void onDrawEffector()
			{
			}

			protected virtual void onPostExecutePart()
			{
			}

			private static void sampleTime(uint section)
			{
			}

			private static void totalTime()
			{
			}
		}

		public class PMHGamePart : FF3GamePart
		{
		}

		public class DefaultGamePartSystemPolicy
		{
			public const int NumMaxParts = 24;

			public const int NumMaxSleepParts = 4;
		}

		public class MsgCode
		{
			private int code_;

			public MsgCode()
				: this(0)
			{
			}

			public static explicit operator MsgCode(int code)
			{
				return new MsgCode(code);
			}

			private MsgCode(int arg0)
			{
				code_ = arg0;
			}

			public void setCode(int code)
			{
				code_ = code;
			}

			public int code()
			{
				return code_;
			}
		}

		public class FF3GamePartSystemPolicy
		{
			public const int NumMaxParts = 36;

			public const int NumMaxSleepParts = 4;
		}

		public class SystemObserver
		{
			private ds.DLNode<SystemObserver> observerLink_;

			public SystemObserver()
			{
				observerLink_ = new ds.DLNode<SystemObserver>(this);
				GetObserverList().insertBack(observerLink_, 1u);
			}

			public SystemObserver(SystemObserver rhs)
			{
				observerLink_ = new ds.DLNode<SystemObserver>(this);
				GetObserverList().insertBack(observerLink_, 1u);
			}

			~SystemObserver()
			{
				GetObserverList().erase(observerLink_);
			}

			public static void resetObserver(bool bSoftRest)
			{
				for (ds.DLNode<SystemObserver> dLNode = GetObserverList().front(); dLNode != null; dLNode = dLNode.next())
				{
					dLNode.data().doReset(bSoftRest);
				}
			}

			protected virtual void doReset(bool bSoftRest)
			{
			}
		}

		public class ChainTextureManager
		{
			public class FileHeader
			{
				public byte[] ucFileType = new byte[4];

				public uint unVersion;

				public uint unNbReplaceTexel;

				public uint unNbReplacePltt;

				public uint unFlag;

				public uint[] unReserve = new uint[3];

				public static explicit operator FileHeader(ArrayReader src)
				{
					FileHeader fileHeader = new FileHeader();
					src.read(fileHeader.ucFileType, 0, 4);
					fileHeader.unVersion = src.readUInt32();
					fileHeader.unNbReplaceTexel = src.readUInt32();
					fileHeader.unNbReplacePltt = src.readUInt32();
					fileHeader.unFlag = src.readUInt32();
					src.read(fileHeader.unReserve, 0, 3);
					return fileHeader;
				}
			}

			public const int ObjectNameSize = 16;

			protected uint _unNbTextures;

			protected Array _pPackData;

			protected FileHeader _pHeader;

			protected string[] _szTexelName;

			protected string[] _szPlttName;

			protected ds.sys3d.CModelTexture[] _pModelTexture;

			public ChainTextureManager()
			{
				_pPackData = null;
				_pHeader = null;
				_pModelTexture = null;
				_unNbTextures = 0u;
			}

			~ChainTextureManager()
			{
				unloadTexturePack();
			}

			public bool loadTexturePack(string szPackfile)
			{
				ds.CFile cFile = new ds.CFile();
				unloadTexturePack();
				Array array = ds.CHeap.alloc_app(cFile.getSize(szPackfile));
				cFile.load(array, szPackfile);
				_pPackData = array;
				ArrayReader arrayReader = new ArrayReader(array);
				FileHeader fileHeader = (FileHeader)arrayReader;
				if (!validateIDCode(fileHeader.ucFileType, ReplaceTextureCode))
				{
					unloadTexturePack();
					return false;
				}
				_szTexelName = arrayReader.readStringArray(16, (int)fileHeader.unNbReplaceTexel);
				_szPlttName = arrayReader.readStringArray(16, (int)fileHeader.unNbReplacePltt);
				_pHeader = fileHeader;
				Array array2 = new byte[arrayReader.rest()];
				arrayReader.read((byte[])array2, 0, array2.Length);
				_unNbTextures = pack.ChainPointerCount((byte[])array2);
				ds.sys3d.CModelTexture[] array3 = (_pModelTexture = new ds.sys3d.CModelTexture[_unNbTextures]);
				for (int i = 0; i < _unNbTextures; i++)
				{
					array3[i] = new ds.sys3d.CModelTexture();
					array3[i].setup(ds.sys3d.CModelTexture.ChainPointer((byte[])array2, i), tdl: true);
				}
				return true;
			}

			public void unloadTexturePack()
			{
				if (_pModelTexture != null)
				{
					ds.sys3d.CModelTexture[] pModelTexture = _pModelTexture;
					for (int i = 0; i < _unNbTextures; i++)
					{
						pModelTexture[i].cleanup();
						pModelTexture[i].destruct();
					}
					ds.CHeap.free_app(_pModelTexture);
					_pModelTexture = null;
				}
				if (_pPackData != null)
				{
					ds.CHeap.free_app(_pPackData);
					_pPackData = null;
				}
				_pHeader = null;
				_unNbTextures = 0u;
			}

			public bool replaceTexel(ds.sys3d.CModelSet pModelSet, uint unDataIndex, string szName)
			{
				if (!validate(pModelSet, unDataIndex))
				{
					return false;
				}
				if (szName == null)
				{
					string[] szTexelName = _szTexelName;
					for (int i = 0; i < _pHeader.unNbReplaceTexel; i++)
					{
						pModelSet.bindReplaceTexelByName(_pModelTexture[unDataIndex], szTexelName[i]);
					}
				}
				else
				{
					pModelSet.bindReplaceTexelByName(_pModelTexture[unDataIndex], szName);
				}
				return true;
			}

			public bool replacePalette(ds.sys3d.CModelSet pModelSet, uint unDataIndex, string szName)
			{
				if (!validate(pModelSet, unDataIndex))
				{
					return false;
				}
				if (szName == null)
				{
					string[] szPlttName = _szPlttName;
					for (int i = 0; i < _pHeader.unNbReplacePltt; i++)
					{
						pModelSet.bindReplacePlttByName(_pModelTexture[unDataIndex], szPlttName[i]);
					}
				}
				else
				{
					pModelSet.bindReplacePlttByName(_pModelTexture[unDataIndex], szName);
				}
				return true;
			}

			public bool replaceTexture(ds.sys3d.CModelSet pModelSet, uint unDataIndex)
			{
				if (!validate(pModelSet, unDataIndex))
				{
					return false;
				}
				replaceTexel(pModelSet, unDataIndex, null);
				replacePalette(pModelSet, unDataIndex, null);
				return true;
			}

			public bool isLoadPackfile()
			{
				if (_pPackData == null)
				{
					return false;
				}
				return true;
			}

			public uint getNbTextures()
			{
				return _unNbTextures;
			}

			public string getReplaceTexelNodeName(uint unNodeIndex)
			{
				if (!isLoadPackfile() || unNodeIndex >= _pHeader.unNbReplaceTexel)
				{
					return null;
				}
				return _szTexelName[unNodeIndex];
			}

			public string getReplacePaletteNodeName(uint unNodeIndex)
			{
				if (!isLoadPackfile() || unNodeIndex >= _pHeader.unNbReplacePltt)
				{
					return null;
				}
				return _szPlttName[unNodeIndex];
			}

			public bool validate(ds.sys3d.CModelSet pModelSet, uint unDataIndex)
			{
				if (!isLoadPackfile())
				{
					return false;
				}
				if (pModelSet == null)
				{
					return false;
				}
				if (unDataIndex >= _unNbTextures)
				{
					return false;
				}
				return true;
			}
		}

		public static ds.Vector<CBlankTask, ds.FastErasePolicy<CBlankTask>> g_tvVertical = new ds.Vector<CBlankTask, ds.FastErasePolicy<CBlankTask>>(CBlankTask.LIMIT_OF_TASK);

		public static ds.Vector<CBlankTask, ds.FastErasePolicy<CBlankTask>> g_tvHorizontal = new ds.Vector<CBlankTask, ds.FastErasePolicy<CBlankTask>>(CBlankTask.LIMIT_OF_TASK);

		public static FF3PartSystem FF3PartSys = new FF3PartSystem();

		private static ds.DLList<SystemObserver> list;

		internal static void registerMovieFile()
		{
		}

		public static ds.DLList<SystemObserver> GetObserverList()
		{
			return list;
		}
	}
}
