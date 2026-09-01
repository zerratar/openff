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
	public static partial class wmenu
	{
							public class CWMenuSave : CWMenuMemberBase, menu.MenuBehavedNotifier
							{
								public const int STATE_INIT = 0;

								public const int STATE_CONFIRM = 1;

								public const int STATE_END = 2;

								public const int STATE_END_WAIT = 3;

								private bool endingSave_;

								private int localState_;

								private uint confirm_;

								public override bool cSelectInitialize()
								{
									return false;
								}

								public override void initialize()
								{
									setFinalize(val: false);
									ds.CVram.setSubPlaneVisiblity(bg0: true, bg1: false, bg2: false, bg3: true, obj: true);
									if (endingSave_)
									{
										ds.SoundHeap.HeapClear();
										MatrixSound.MtxSENDS_Load(0);
										MatrixSound.MtxSENDS_Load(98);
									}
									byte[] pData = new byte[4];
									card.Manager.GetInstance().LoadData(pData, 4u, 0u);
									if (card.Manager.GetInstance().GetResult() != card.RESULT.RESULT_SUCCESS)
									{
										menu.CMenuSaveLoad.singleton().setCurrentState(menu.CMenuSaveLoad.STATE.STATE_SLOT_BACKUP_ERR);
										menu.CMenuSaveLoad.singleton().offEndFlag();
										menu.MenuManager.getSingleton().release();
										menu.MenuManager.getSingleton().buildMenu("suspend_failed");
										backupAccessFailedSetting();
									}
									else
									{
										menu.CMenuSaveLoad.singleton().setCurrentMode(menu.CMenuSaveLoad.MODE.MODE_SAVE);
										menu.CMenuSaveLoad.singleton().initialize();
										CWMenuManager.Instance().SetPrimaryBG(8);
										CWMenuManager.Instance().GetMenuButton().SetUpSpecialVer();
										CWMenuManager.Instance().SetProcState(WMENU_PROCESS.WMENU_PROCESS_RUN);
										CWMenuManager.Instance().SetSecondlyBG(2);
										CWMenuManager.Instance().SetSecondlyBGVisibility(b: false);
										localState_ = 0;
									}
								}

								public override void run()
								{
									menu.CMenuSaveLoad.singleton().execute();
									if (menu.CMenuSaveLoad.singleton().isEndFlag())
									{
										if (!endingSave_)
										{
											CWMenuManager.Instance().SetProcState(WMENU_PROCESS.WMENU_PROCESS_TERMINATE);
											CWMenuManager.Instance().SetNextKind(WMENU_KIND.WMENU_KIND_MAIN_MENU);
										}
										else
										{
											endingSaveProc();
										}
									}
									else if (menu.CMenuSaveLoad.singleton().getCurrentState() == menu.CMenuSaveLoad.STATE.STATE_SELECT_SLOT)
									{
										if (CWMenuManager.Instance().GetMenuButton().TouchButtonB())
										{
											menu.CMenuSaveLoad.singleton().onEndFlag();
										}
									}
									else if (menu.CMenuSaveLoad.singleton().getCurrentState() == menu.CMenuSaveLoad.STATE.STATE_SLOT_DATA_SELECT)
									{
										if (menu.MenuManager.getSingleton().GetDecideButtonState() == 0)
										{
											byte[] pData = new byte[4];
											card.Manager.GetInstance().LoadData(pData, 4u, 0u);
											if (card.Manager.GetInstance().GetResult() != card.RESULT.RESULT_SUCCESS)
											{
												menu.CMenuSaveLoad.singleton().setCurrentState(menu.CMenuSaveLoad.STATE.STATE_SLOT_BACKUP_ERR);
												menu.MenuManager.getSingleton().release();
												menu.MenuManager.getSingleton().buildMenu("suspend_write_failed");
												backupAccessFailedSetting();
											}
											else
											{
												menu.CMenuSaveLoad.singleton().setCurrentState(menu.CMenuSaveLoad.STATE.STATE_SLOT_DATA_WAIT);
												CWMenuManager.Instance().GetMenuButton().SetButtonBActivity(b: false);
												CWMenuManager.Instance().SetSecondlyBGVisibility(b: false);
												menu.MenuManager.getSingleton().Pop();
												menu.MenuManager.getSingleton().Push("now_saving");
												SaveDataMng.getSingleton().saveAsync((int)menu.CMenuSaveLoad.singleton().getCurrentSlot());
											}
											menu.MenuManager.getSingleton().playSEDecide();
										}
									}
									else if (menu.CMenuSaveLoad.singleton().getCurrentState() == menu.CMenuSaveLoad.STATE.STATE_SLOT_DATA_WAIT)
									{
										SaveDataMng.getSingleton().update();
										if (SaveDataMng.getSingleton().saveAsyncState() == 0)
										{
											byte[] pData2 = new byte[4];
											card.Manager.GetInstance().LoadData(pData2, 4u, 0u);
											if (card.Manager.GetInstance().GetResult() != card.RESULT.RESULT_SUCCESS)
											{
												menu.CMenuSaveLoad.singleton().setCurrentState(menu.CMenuSaveLoad.STATE.STATE_SLOT_BACKUP_ERR);
												menu.MenuManager.getSingleton().release();
												menu.MenuManager.getSingleton().buildMenu("suspend_write_failed");
												backupAccessFailedSetting();
											}
											else
											{
												SaveDataMng.getSingleton().setSlotState((int)menu.CMenuSaveLoad.singleton().getCurrentSlot(), SaveDataMng.getSingleton().getState());
												SaveDataMng.getSingleton().refreshActiveData((int)menu.CMenuSaveLoad.singleton().getCurrentSlot(), SaveDataMng.getSingleton().SaveData());
												menu.CMenuSaveLoad.singleton().setCurrentState(menu.CMenuSaveLoad.STATE.STATE_SLOT_DATA_FINISHED);
												menu.CMenuSaveLoad.singleton().setCounter(50);
												menu.MenuManager.getSingleton().Pop();
												menu.MenuManager.getSingleton().Push("save_finished");
												MatrixSound.MtxSENDS_Play(98, 4, 192, 127);
											}
										}
									}
									else if (menu.CMenuSaveLoad.singleton().getCurrentState() == menu.CMenuSaveLoad.STATE.STATE_SLOT_BACKUP_ERR && dgs.CFade.Sub().isCleared())
									{
										OS_Terminate();
									}
								}

								public override void terminate()
								{
									if (!isFinalize())
									{
										menu.CMenuSaveLoad.singleton().terminate();
										menu.CMenuSaveLoad.singleton().setClearMarkVisibility(b: false);
										setFinalize(val: true);
										OS_Printf("CWmenuSave . terminate : terminate world save menu.\n");
									}
								}

								public void endingSaveSetting(bool b)
								{
									endingSave_ = b;
								}

								public void endingSaveProc()
								{
									switch (localState_)
									{
									case 0:
									{
										CWMenuManager.Instance().SetSecondlyBGVisibility(b: true);
										CWMenuManager.Instance().GetMenuButton().SetButtonBActivity(b: false);
										menu.MenuManager.getSingleton().Push("confirm");
										menu.Medget medget = menu.MenuManager.getSingleton().root().childNode();
										if (medget != null && medget.behavior() != null)
										{
											menu.MBQuestion mBQuestion = (menu.MBQuestion)medget.behavior().queryInterface(menu.MBQuestion.classIdentifier());
											if (mBQuestion != null)
											{
												mBQuestion.bmqSetMessage(0, 50739);
												mBQuestion.mbSetNotifier(this);
											}
										}
										localState_ = 1;
										break;
									}
									case 1:
										if (menu.MenuManager.getSingleton().GetDecideButtonState() == 0 || menu.MenuManager.getSingleton().GetCancelButtonState() == 0)
										{
											localState_ = 2;
										}
										break;
									case 2:
										if (confirm_ == 0)
										{
											CWMenuManager.Instance().SetProcState(WMENU_PROCESS.WMENU_PROCESS_END);
											menu.MenuManager.getSingleton().release();
											menu.MenuManager.getSingleton().playSECancel();
											CWMenuManager.Instance().SetSecondlyBGVisibility(b: false);
											localState_ = 3;
										}
										else if (1 == confirm_)
										{
											menu.CMenuSaveLoad.singleton().offEndFlag();
											menu.MenuManager.getSingleton().Pop();
											menu.MenuManager.getSingleton().initFocus(0);
											menu.MenuManager.getSingleton().playSECancel();
											CWMenuManager.Instance().SetSecondlyBGVisibility(b: false);
											CWMenuManager.Instance().GetMenuButton().SetButtonBActivity(b: true);
											localState_ = 0;
										}
										break;
									case 3:
										break;
									}
								}

								public bool mbnNotify(menu.MenuBehavior Notifier, uint number, uint param)
								{
									confirm_ = number;
									return true;
								}

								public CWMenuSave()
								{
									endingSave_ = false;
									localState_ = 0;
								}
							}
	}
}
