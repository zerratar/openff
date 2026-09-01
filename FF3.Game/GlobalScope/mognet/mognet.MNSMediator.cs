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
	public static partial class mognet
	{
		public class MNSMediator
		{
			public MogNetState currentState;

			protected MogNetState parallelState;

			public MogNetState prevState;

			public MogNetState nextState;

			protected MogNetState wifiPurpose;

			protected MogNetState wifiPurposeIncomplete;

			public MNSBridge MNSBridge_ = new MNSBridge();

			public MNSLetterBrowse MNSLetterBrowse_ = new MNSLetterBrowse();

			public MNSSelectPerson MNSSelectPerson_ = new MNSSelectPerson();

			protected sys2d.Bg mainBG_ = new sys2d.Bg();

			protected sys2d.Bg subBG_ = new sys2d.Bg();

			protected dgs.MSDINFO[] msd_ = new dgs.MSDINFO[2];

			protected bool ciActivity_;

			protected wmenu.CWMenuButton commonInterface_ = new wmenu.CWMenuButton();

			protected sys2d.Cell[] newMarkCell_ = new sys2d.Cell[NUM_NEW_MARK];

			protected sys2d.Cell dummyCursor_ = new sys2d.Cell();

			protected MatrixSound.MtxSEHandle mtxSEHandle_;

			public dgs.CFade.FADE_TYPE fadeColor_;

			protected byte[] buffer_;

			protected int savedAlignment_;

			protected bool showWiFiSettingBrokenFlag_;

			public MNSMediator()
			{
				for (int i = 0; i < newMarkCell_.Length; i++)
				{
					newMarkCell_[i] = new sys2d.Cell();
				}
				currentState = null;
				parallelState = null;
				msd_[0] = (msd_[1] = null);
			}

			public void destruct()
			{
			}

			public void mnsMediatorVRAMSetting()
			{
				GX_SetBankForTex(GXVRamTex.GX_VRAM_TEX_NONE);
				GX_SetBankForTexPltt(GXVRamTexPltt.GX_VRAM_TEXPLTT_NONE);
				GX_SetBankForBG(GXVRamBG.GX_VRAM_BG_128_A);
				GX_SetBankForBGExtPltt(GXVRamBGExtPltt.GX_VRAM_BGEXTPLTT_01_F);
				GX_SetBankForOBJ(GXVRamOBJ.GX_VRAM_OBJ_128_B);
				GX_SetBankForOBJExtPltt(GXVRamOBJExtPltt.GX_VRAM_OBJEXTPLTT_0_G);
				GX_SetBankForSubBG(GXVRamSubBG.GX_VRAM_SUB_BG_128_C);
				GX_SetBankForSubBGExtPltt(GXVRamSubBGExtPltt.GX_VRAM_SUB_BGEXTPLTT_NONE);
				GX_SetBankForSubOBJ(GXVRamSubOBJ.GX_VRAM_SUB_OBJ_128_D);
				GX_SetBankForSubOBJExtPltt(GXVRamSubOBJExtPltt.GX_VRAM_SUB_OBJEXTPLTT_0_I);
				ds.CVram.setMainPlaneVisiblity(bg0: false, bg1: false, bg2: false, bg3: false, obj: false);
				ds.CVram.setSubPlaneVisiblity(bg0: false, bg1: false, bg2: false, bg3: false, obj: false);
				ds.CVram.setMainBGPriority(3, 2, 1, 0);
				ds.CVram.setSubBGPriority(3, 2, 1, 0);
				GX_SetGraphicsMode(GXDispMode.GX_DISPMODE_GRAPHICS, GXBGMode.GX_BGMODE_0, 0);
				GXS_SetGraphicsMode(GXBGMode.GX_BGMODE_0);
				G2_SetBG0Control(0, 0, GXBGScrBase.GX_BG_SCRBASE_0x0000, GXBGCharBase.GX_BG_CHARBASE_0x04000, 0);
				G2_SetBG1Control(0, 0, GXBGScrBase.GX_BG_SCRBASE_0x1800, GXBGCharBase.GX_BG_CHARBASE_0x18000, 0);
				G2_SetBG2ControlText(0, 0, GXBGScrBase.GX_BG_SCRBASE_0x1000, GXBGCharBase.GX_BG_CHARBASE_0x14000);
				G2_SetBG3ControlText(0, 0, GXBGScrBase.GX_BG_SCRBASE_0x0800, GXBGCharBase.GX_BG_CHARBASE_0x10000);
				G2S_SetBG0Control(0, 0, GXBGScrBase.GX_BG_SCRBASE_0x0000, GXBGCharBase.GX_BG_CHARBASE_0x04000, 0);
				G2S_SetBG1Control(0, 0, GXBGScrBase.GX_BG_SCRBASE_0x1800, GXBGCharBase.GX_BG_CHARBASE_0x18000, 0);
				G2S_SetBG2ControlText(0, 0, GXBGScrBase.GX_BG_SCRBASE_0x1000, GXBGCharBase.GX_BG_CHARBASE_0x14000);
				G2S_SetBG3ControlText(0, 0, GXBGScrBase.GX_BG_SCRBASE_0x0800, GXBGCharBase.GX_BG_CHARBASE_0x10000);
				menu.MenuManager.getSingleton().Set2d3dMode(2);
				menu.MenuManager.getSingleton().CreateNeedObject(menu.MenuManager.MENU_CREATE_PARAMETER.CREATE_MENU_MODE_2D);
				menu.MenuManager.getSingleton().SetUsingMenuType(1);
			}

			public void mnsMediatorLoad()
			{
				changeGlobalDirectory();
				changeCompanyDirectory();
				newMarkCell_[0].Load(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_SUB2D, "new_i.NCER", "new_i.NANR", "new_i.NCGR", null);
				newMarkCell_[0].ceReleaseCgCl();
				newMarkCell_[0].SetShow(show: false);
				sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(newMarkCell_[0]);
				for (int i = 1; NUM_NEW_MARK > i; i++)
				{
					newMarkCell_[i].copy(newMarkCell_[0]);
					newMarkCell_[i].SetShow(show: false);
					sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(newMarkCell_[i]);
				}
				changeGlobalDirectory();
				dummyCursor_.copy(menu.MenuManager.getSingleton().GetCursor2d());
				dummyCursor_.SetCell(3);
				dummyCursor_.SetShow(show: false);
				dummyCursor_.SetAnimation(anm: false);
				dummyCursor_.SetPositionI(320, 240);
				sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(dummyCursor_);
				for (int j = 0; j < 8; j++)
				{
					uint size = ds.g_File.getSize(sub_bg_nscr[j]);
					if (size != 0)
					{
						Array array = ds.CHeap.alloc_app(size);
						if (array != null)
						{
							ds.g_File.load(array, sub_bg_nscr[j]);
							NNSG2dScreenData nNSG2dScreenData = null;
							NNS_G2dGetUnpackedScreenData(array, nNSG2dScreenData);
							NNSG2dCellDataBank nNSG2dCellDataBank = new NNSG2dCellDataBank();
							NNS_G2dGetUnpackedCellBank(array, nNSG2dCellDataBank);
							vSubNscrPtr.push_back(new SCRSTRUCT(array, nNSG2dScreenData, nNSG2dCellDataBank));
							OS_Printf("LoadScreenData[%s] 0x%08x\n", sub_bg_nscr[j], nNSG2dScreenData);
						}
					}
				}
				mainBG_.bgLoad2(main_bg_nscr[0]);
				mainBG_.bgSetUp(mainBgSelect);
				mainBG_.bgRelease();
				mainBG_.bgSetShow(show: true);
				subBG_.bgLoad(sub_bg_nscr[0], "menu_bg_01.NCGR", "new_menu_bg.NCLR");
				subBG_.bgSetUp(subBgSelect);
				subBG_.bgRelease();
				subBG_.bgSetShow(show: true);
				Array array2 = null;
				string[] array3 = new string[2] { "eureka_menu.msd", "eureka_mognet.msd" };
				changeCompanyDirectory();
				for (int k = 0; k < 2; k++)
				{
					if (msd_[k] != null)
					{
						continue;
					}
					uint size2 = ds.g_File.getSize(array3[k]);
					if (size2 != 0)
					{
						array2 = ds.CHeap.alloc_app(size2);
						if (array2 != null)
						{
							ds.g_File.load(array2, array3[k]);
						}
						msd_[k] = (dgs.MSDINFO)array2;
					}
					dgs.msg.CMessageSys.getInstance().Main().initMSD(msd_[k]);
					dgs.msg.CMessageSys.getInstance().Sub().initMSD(msd_[k]);
				}
				commonInterface_.initialize();
			}

			public void mnsMediatorUnload()
			{
				commonInterface_.terminate();
				for (int i = 0; i < 2; i++)
				{
					if (msd_[i] != null)
					{
						dgs.msg.CMessageSys.getInstance().Main().removeMSD(msd_[i]);
						dgs.msg.CMessageSys.getInstance().Sub().removeMSD(msd_[i]);
						ds.CHeap.free_app(msd_[i]);
						msd_[i] = null;
					}
				}
				while (!vSubNscrPtr.empty())
				{
					ds.CHeap.free_app(vSubNscrPtr[0].ptr);
					vSubNscrPtr.erase(0);
				}
				sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(dummyCursor_);
				for (int j = 0; NUM_NEW_MARK > j; j++)
				{
					sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(newMarkCell_[j]);
					NNS_G2dReleaseImageProxy(newMarkCell_[j].GetImageProxy());
				}
				dummyCursor_.Release();
				for (int k = 0; NUM_NEW_MARK > k; k++)
				{
					newMarkCell_[k].Release();
				}
			}

			public void mnsMediatorInitialize()
			{
				savedAlignment_ = ds.CHeap.align_app();
				ds.CHeap.realign_app(32);
				OS_SetThreadStackWarningOffset(OS_GetCurrentThread(), 256u);
				OS_EnableInterrupts();
				mnsMediatorLoad();
				mnsmCommonInterface(b: true, bLR: false);
				commonInterface_.SetButtonAActivity(b: false);
				commonInterface_.SetButtonLActivity(b: false);
				commonInterface_.SetButtonRActivity(b: false);
				mtxSEHandle_ = null;
				shiftDefaultState();
				ds.CVram.setMainPlaneVisiblity(bg0: false, bg1: false, bg2: false, bg3: true, obj: true);
				ds.CVram.setSubPlaneVisiblity(bg0: true, bg1: false, bg2: false, bg3: false, obj: true);
				dgs.CFade.Main().fadeIn(15);
				dgs.CFade.Sub().fadeIn(15);
			}

			public bool mnsMediatorProcess()
			{
				OS_AssignBackButton(1);
				if (ciActivity_)
				{
					if (commonInterface_.TouchButtonA())
					{
						if (currentState != null && currentState.mnsDecide(this))
						{
							return true;
						}
					}
					else if (commonInterface_.TouchButtonB() && currentState != null && currentState.mnsCancel(this))
					{
						return true;
					}
				}
				if (currentState != null)
				{
					return currentState.mnsProcess(this);
				}
				return false;
			}

			public void mnsMediatorTerminate()
			{
				if (currentState != null)
				{
					currentState.mnsTerminate(this);
				}
				currentState = null;
				mnsMediatorUnload();
				ds.CHeap.realign_app(savedAlignment_);
			}

			public void shiftDefaultState()
			{
				shiftState(MNSSelectPerson_);
			}

			public void shiftState(MogNetState new_state)
			{
				if (currentState != null)
				{
					currentState.mnsTerminate(this);
				}
				currentState = new_state;
				currentState.ownerMediator = this;
				if (currentState != null)
				{
					currentState.mnsInitialize(this);
				}
			}

			public void shiftStateBridge(MogNetState new_state)
			{
				new_state.ownerMediator = this;
				prevState = currentState;
				nextState = new_state;
				currentState = MNSBridge_;
				currentState.mnsInitialize(this);
			}

			public void changeMainBGScr(MAIN_BG_SCR scr)
			{
			}

			public void changeSubBGScr(SUB_BG_SCR scr)
			{
				if (vSubNscrPtr[(int)scr].cell != null)
				{
					NNS_G2dBGSetupCell((int)subBgSelect, vSubNscrPtr[(int)scr].cell, subBgSelect);
				}
			}

			public void mnsmDrawQuery(int msg_id)
			{
				dgs.msg.CMessageSys.getInstance().Sub().dgsMMAreaErase(56, 0, 200, 16);
				if (msg_id >= 0)
				{
					dgs.msg.CMessageSys.getInstance().Sub().writeCharacterString(64, 9, 0, 0, dgs.TXT_COLOR.TXT_COLOR_WHITE, 10u, (uint)msg_id, shadow: true, 1);
				}
			}

			public void mnsmDrawDeclaration(int msg_id)
			{
				string buffer = "";
				dgs.msg.CMessageSys.getInstance().Sub().dgsMMAreaErase(0, 16, 256, 144);
				if (msg_id >= 0)
				{
					dgs.msg.CMessageSys.getInstance().Sub().writeCharacterStringCC(128, 88, 0, 4, dgs.TXT_COLOR.TXT_COLOR_WHITE, 18u, (uint)msg_id, ref buffer, shadow: true, 0);
				}
			}

			public void mnsmDrawHelp(int msg_id)
			{
				dgs.msg.CMessageSys.getInstance().Sub().dgsMMAreaErase(0, 160, 256, 16);
				if (msg_id >= 0)
				{
					dgs.msg.CMessageSys.getInstance().Sub().writeCharacterString(8, 169, 0, 0, dgs.TXT_COLOR.TXT_COLOR_WHITE, 10u, (uint)msg_id, shadow: true, 0);
				}
			}

			public void mnsmCommonInterface(bool b, bool bLR)
			{
				ciActivity_ = b;
				commonInterface_.SetButtonBActivity(b);
				commonInterface_.SetButtonLActivity(bLR);
				commonInterface_.SetButtonRActivity(bLR);
			}

			public sys2d.Cell mnsmNewMarkIcon(int elem)
			{
				return newMarkCell_[elem];
			}

			public void mnsmDummyCursor(bool b, int x, int y)
			{
				dummyCursor_.SetShow(b);
				dummyCursor_.SetPositionI(x, y);
			}

			public wmenu.CWMenuButton mnsmButtonB()
			{
				return commonInterface_;
			}

			public void mnsmPlayConnectingSE()
			{
				if (!MatrixSound.MtxSENDS_isPlaying(mtxSEHandle_))
				{
					mtxSEHandle_ = MatrixSound.MtxSENDS_Play(96, 1, 192, 127);
				}
			}

			public void mnsmStopConnectingSE()
			{
				if (MatrixSound.MtxSENDS_isPlaying(mtxSEHandle_))
				{
					MatrixSound.MtxSENDS_Stop(mtxSEHandle_, 15);
					mtxSEHandle_ = null;
				}
			}

			public bool mnsIsShowWiFiSettingBroken()
			{
				return showWiFiSettingBrokenFlag_;
			}

			public void mnsSetShowWiFiSettingBroken(bool m)
			{
				showWiFiSettingBrokenFlag_ = m;
			}

			protected MogNetState getWiFiPurpose()
			{
				return wifiPurpose;
			}

			protected MogNetState getWiFiPurposeIncomplete()
			{
				return wifiPurposeIncomplete;
			}

			protected void setWiFiPurpose(MogNetState st, MogNetState incomp)
			{
				wifiPurpose = st;
				wifiPurposeIncomplete = incomp;
			}
		}
	}
}
