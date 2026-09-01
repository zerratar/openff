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
						public class MogNet : wld.CParallelWorldSystem
						{
							public static MogNet instance_ = new MogNet();

							private mognet.MNSMediator mnsMediator;

							private int step_;

							public MogNet()
							{
								mnsMediator = null;
								step_ = 0;
							}

							public override void initialize(wld.CBaseSystem BS)
							{
								evt.CEventManager.getInstance().setEventStop(_EventStop: true);
								step_ = 0;
								BS.World2DMng().refWorldMap().hideMapMarker();
							}

							public override void terminate(wld.CBaseSystem BS)
							{
								menu.MenuManager.getSingleton().SetActivateButtonState(1);
								menu.MenuManager.getSingleton().ClearBehaviorButton();
								evt.CEventManager.getInstance().setEventStop(_EventStop: false);
							}

							public override bool execute(wld.CBaseSystem BS)
							{
								switch (step_)
								{
								case 0:
									BS.World2DMng().MessageWindow().createWindow(0);
									BS.World2DMng().MessageWindow().createMessage(1002058, 0, 0);
									BS.World2DMng().MessageWindow().setProgressIconActivity(_SendMessage: true);
									step_ = 1;
									break;
								case 1:
									if (!BS.World2DMng().MessageWindow().isMadeMessage())
									{
										BS.World2DMng().MessageWindow().release();
										dgs.CFade.Main().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
										dgs.CFade.Sub().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
										step_ = 2;
									}
									break;
								case 2:
									if (dgs.CFade.Main().isFaded() && dgs.CFade.Sub().isFaded())
									{
										TexDivideLoader.getSingleton().tdlCancel();
										SVC_WaitVBlankIntr();
										SVC_WaitVBlankIntr();
										if (mnsMediator != null)
										{
											mnsMediator.destruct();
										}
										mnsMediator = new mognet.MNSMediator();
										GX_SetDispSelect(GXDispSelect.GX_DISP_SELECT_MAIN_SUB);
										ds.CDevice.setup();
										GX_SetBGCharOffset(0);
										GX_SetBGScrOffset(0);
										G2_BlendNone();
										GX_DispOn();
										GXS_DispOn();
										G3X_SetClearColor(GX_RGB(0, 8, 26), 31, 32767, 1, 0);
										GX_SetBankForTex(GXVRamTex.GX_VRAM_TEX_NONE);
										GX_SetBankForTexPltt(GXVRamTexPltt.GX_VRAM_TEXPLTT_NONE);
										GX_SetBankForBG(GXVRamBG.GX_VRAM_BG_NONE);
										GX_SetBankForBGExtPltt(GXVRamBGExtPltt.GX_VRAM_BGEXTPLTT_NONE);
										GX_SetBankForOBJ(GXVRamOBJ.GX_VRAM_OBJ_NONE);
										GX_SetBankForOBJExtPltt(GXVRamOBJExtPltt.GX_VRAM_OBJEXTPLTT_NONE);
										GX_SetBankForSubBG(GXVRamSubBG.GX_VRAM_SUB_BG_NONE);
										GX_SetBankForSubBGExtPltt(GXVRamSubBGExtPltt.GX_VRAM_SUB_BGEXTPLTT_NONE);
										GX_SetBankForSubOBJ(GXVRamSubOBJ.GX_VRAM_SUB_OBJ_NONE);
										GX_SetBankForSubOBJExtPltt(GXVRamSubOBJExtPltt.GX_VRAM_SUB_OBJEXTPLTT_NONE);
										GX_DisableBankForTex();
										GX_DisableBankForTexPltt();
										GX_DisableBankForBG();
										GX_DisableBankForBGExtPltt();
										GX_DisableBankForOBJ();
										GX_DisableBankForOBJExtPltt();
										GX_DisableBankForSubBG();
										GX_DisableBankForSubBGExtPltt();
										GX_DisableBankForSubOBJ();
										GX_DisableBankForSubOBJExtPltt();
										mnsMediator.mnsMediatorVRAMSetting();
										G2_SetBG0Offset(0, 0);
										G2_SetBG1Offset(0, 0);
										G2_SetBG2Offset(0, 0);
										G2_SetBG3Offset(0, 0);
										G2S_SetBG0Offset(0, 0);
										G2S_SetBG1Offset(0, 0);
										G2S_SetBG2Offset(0, 0);
										G2S_SetBG3Offset(0, 0);
										changeCompanyDirectory();
										menu.MenuManager.getSingleton().LoadXbnFile("MogNet.xbn");
										mnsMediator.mnsMediatorInitialize();
										mognet.MNMemento.getSingleton().mnmInitialize();
										mognet.MNNPCMailData.getSingleton().initialize();
										GX_Power3D(0);
										step_ = 3;
									}
									break;
								case 3:
									if (dgs.CFade.Main().isCleared() && dgs.CFade.Sub().isCleared())
									{
										step_ = 4;
									}
									break;
								case 4:
									menu.MenuManager.getSingleton().execute();
									if (!mnsMediator.mnsMediatorProcess())
									{
										dgs.CFade.Main().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
										dgs.CFade.Sub().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
										step_ = 5;
									}
									break;
								case 5:
									if (dgs.CFade.Main().isFaded() && dgs.CFade.Sub().isFaded())
									{
										GX_Power3D(1);
										mognet.MNMemento.getSingleton().mnmUpdateUserDataBackup();
										mognet.MNMemento.getSingleton().mnmUpdateFriendListBackup();
										ds.CDevice.setProhibitSleepMode(flag: false);
										mnsMediator.mnsMediatorTerminate();
										mnsMediator.destruct();
										mnsMediator = null;
										mognet.MNNPCMailData.getSingleton().finalize();
										changeGlobalDirectory();
										menu.MenuManager.getSingleton().DeleteNeedObject(menu.MenuManager.MENU_CREATE_PARAMETER.CREATE_MENU_MODE_3D);
										menu.MenuManager.getSingleton().release();
										menu.MenuManager.getSingleton().ReleaseMenuDataText();
										menu.MenuManager.getSingleton().ReleaseXbnFile();
										menu.MenuManager.getSingleton().LoadXbnFile("WorldDefine.xbn");
										GX_SetDispSelect(GXDispSelect.GX_DISP_SELECT_SUB_MAIN);
										ds.CVram.setMainBGPriority(3, 2, 1, 0);
										ds.CVram.setSubBGPriority(1, 2, 0, 3);
										NNS_G2dBGSetupCell(4, null, NNSG2dBGSelect.NNS_G2D_BGSELECT_SUB0);
										dgs.CFade.Main().fadeIn(15);
										dgs.CFade.Sub().fadeIn(15);
										step_ = 6;
									}
									break;
								case 6:
									if (dgs.CFade.Main().isCleared() && dgs.CFade.Sub().isCleared())
									{
										step_ = 7;
									}
									break;
								case 7:
								{
									for (byte b = 0; b < 4; b++)
									{
										if (pl.PlayerParty.instance().player(b).isEnable() && !pl.PlayerParty.instance().player(b).condition()
											.isDeath() && !pl.PlayerParty.instance().player(b).condition()
											.isStone())
										{
											pl.PlayerParty.instance().player(b).playerId();
											break;
										}
									}
									int[] array = new int[18]
									{
										3, 0, 1002051, 8, 1, 1002052, 12, 2, 1002053, 16,
										3, 1002054, 20, 4, 1002055, 24, 5, 1002056
									};
									int[] array2 = new int[10];
									int num = 0;
									bool flag = true;
									for (int i = 0; i < LENGTH(array); i += 3)
									{
										if (mognet.MNNPCMailData.getSingleton().getNPCMailState(array[i] - 1) == mognet.NPCMailState.NPC_MAIL_YET_READ && mognet.MNEvent.checkPartyCondition(array[i + 1]) && !mognet.MNEvent.checkSpecialCondition(array[i + 1]))
										{
											array2[num++] = array[i + 2];
										}
										if (mognet.MNNPCMailData.getSingleton().getNPCMailState(array[i]) == mognet.NPCMailState.NPC_MAIL_NOT_ARRIVED)
										{
											flag = false;
										}
									}
									int mesNum = 1002050;
									if (num != 0)
									{
										mesNum = array2[ds.RandomNumber.rand32((uint)num)];
									}
									if (flag)
									{
										mesNum = 1002057;
									}
									BS.World2DMng().MessageWindow().createWindow(0);
									BS.World2DMng().MessageWindow().createMessage(mesNum, 0, 0);
									BS.World2DMng().MessageWindow().setProgressIconActivity(_SendMessage: true);
									step_ = 8;
									break;
								}
								case 8:
									if (!BS.World2DMng().MessageWindow().isMadeMessage())
									{
										BS.World2DMng().MessageWindow().release();
										return false;
									}
									break;
								}
								return true;
							}

							public static MogNet getSingleton()
							{
								return instance_;
							}
						}
}
