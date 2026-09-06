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
	public static partial class menu
	{
		public class MBSaveLoad : MenuBehavior
		{
			public static dgs.UniqueNumber MBSaveLoad_UN = new dgs.UniqueNumber();

			private int counter_;

			~MBSaveLoad()
			{
			}

			public override void bmInitialize(Medget M)
			{
			}

			public override void bmPostInitialize(Medget M)
			{
				Medget medget = null;
				MBText mBText = null;
				switch (CMenuSaveLoad.singleton().getCurrentState())
				{
				case CMenuSaveLoad.STATE.STATE_SELECT_SLOT:
					if ((medget = M.getNodeByIDFromChildren(TRANSCODE("title"))) != null)
					{
						((MBText)medget.behavior().queryInterface(MBText.classIdentifier()))?.mbSetTextMsgNo((CMenuSaveLoad.singleton().getCurrentMode() == CMenuSaveLoad.MODE.MODE_SAVE) ? 50008 : 50009);
						medget = medget.nextSibling();
					}
					break;
				case CMenuSaveLoad.STATE.STATE_SLOT_DATA_SELECT:
					if ((medget = M.getNodeByIDFromChildren(TRANSCODE("confirmation"))) != null)
					{
						mBText = (MBText)medget.behavior().queryInterface(MBText.classIdentifier());
						if (mBText != null)
						{
							dgs.CCtrlCodeInterface.instance().setSlot((int)CMenuSaveLoad.singleton().getCurrentSlot());
							mBText.mbSetTextMsgNo((CMenuSaveLoad.singleton().getCurrentMode() == CMenuSaveLoad.MODE.MODE_SAVE) ? 50732 : 50734);
						}
					}
					break;
				case CMenuSaveLoad.STATE.STATE_SLOT_DATA_WAIT:
					if (CMenuSaveLoad.singleton().getCurrentMode() == CMenuSaveLoad.MODE.MODE_SAVE && (medget = M.getNodeByIDFromChildren(TRANSCODE("saving"))) != null)
					{
						mBText = (MBText)medget.behavior().queryInterface(MBText.classIdentifier());
						if (mBText != null)
						{
							dgs.CCtrlCodeInterface.instance().setSlot((int)CMenuSaveLoad.singleton().getCurrentSlot());
							mBText.mbSetTextMsgNo((CMenuSaveLoad.singleton().getCurrentMode() == CMenuSaveLoad.MODE.MODE_SAVE) ? 50736 : 50737);
						}
					}
					break;
				case CMenuSaveLoad.STATE.STATE_SLOT_DATA_FINISHED:
					if ((medget = M.getNodeByIDFromChildren(TRANSCODE("finished"))) != null)
					{
						mBText = (MBText)medget.behavior().queryInterface(MBText.classIdentifier());
						if (mBText != null)
						{
							dgs.msg.CMessageSys.getInstance().Sub().dgsMMAreaErase(medget.x(), medget.y(), medget.width(), medget.height());
							dgs.CCtrlCodeInterface.instance().setSlot((int)CMenuSaveLoad.singleton().getCurrentSlot());
							mBText.mbSetTextMsgNo((CMenuSaveLoad.singleton().getCurrentMode() == CMenuSaveLoad.MODE.MODE_SAVE) ? 50733 : 50735);
						}
					}
					break;
				}
			}

			public override void bmFinalize(Medget M)
			{
			}

			public override void bmBehave(Medget M)
			{
				if (CMenuSaveLoad.singleton().getCurrentState() == CMenuSaveLoad.STATE.STATE_SLOT_DATA_WAIT)
				{
					bmBehaveWait_(M);
				}
				if (CMenuSaveLoad.singleton().getCurrentState() == CMenuSaveLoad.STATE.STATE_SELECT_SLOT && ds.g_TouchPanel.isTap())
				{
					Medget nodeByID = M.getNodeByID("decide");
					ds.g_TouchPanel.getPoint(out var x, out var y);
					if (nodeByID != null && nodeByID.x() < x && x <= nodeByID.x() + nodeByID.width() && nodeByID.y() < y && y <= nodeByID.y() + nodeByID.height())
					{
						MenuManager.getSingleton().MedgetsTerminateBehave();
						bmDecide(MenuManager.getSingleton().getFocuseMedget());
					}
				}
			}

			public void bmBehaveWait_(Medget M)
			{
				bool flag = false;
				if (CMenuSaveLoad.singleton().getCurrentMode() == CMenuSaveLoad.MODE.MODE_LOAD)
				{
					flag = Load_();
				}
				if (flag && CMenuSaveLoad.MODE.MODE_LOAD == CMenuSaveLoad.singleton().getCurrentMode())
				{
					CMenuSaveLoad.singleton().setCurrentState(CMenuSaveLoad.STATE.STATE_SLOT_DATA_FINISHED);
					CMenuSaveLoad.singleton().setCounter(50);
					MenuManager.getSingleton().MedgetsTerminateBehave();
					MenuManager.getSingleton().Pop();
					MenuManager.getSingleton().Push("save_finished");
					MatrixSound.MtxSENDS_Play(98, 4, 192, 127);
				}
			}

			public void bmBehaveFin_(Medget M)
			{
			}

			public override void bmSuspend(Medget M)
			{
			}

			public override void bmResume(Medget M)
			{
			}

			public override bool bmDecide(Medget M)
			{
				switch (CMenuSaveLoad.singleton().getCurrentState())
				{
				case CMenuSaveLoad.STATE.STATE_SELECT_SLOT:
					bmDecideSelectSlot_(M);
					break;
				case CMenuSaveLoad.STATE.STATE_SLOT_DATA_SELECT:
					bmDecideDataSelect_(M);
					break;
				}
				return true;
			}

			public void bmDecideSelectSlot_(Medget M)
			{
				if (CMenuSaveLoad.singleton().getCurrentMode() == CMenuSaveLoad.MODE.MODE_LOAD)
				{
					if (card.Manager.GetInstance().GetState((int)CMenuSaveLoad.singleton().getCurrentSlot(), -1))
					{
						if (SaveDataMng.getSingleton().getSlotState((int)CMenuSaveLoad.singleton().getCurrentSlot()) == 1)
						{
							MenuManager.getSingleton().playSEDecide();
							if (M._id(TRANSCODE("slot1")) || M._id(TRANSCODE("slot2")) || M._id(TRANSCODE("slot3")))
							{
								CMenuSaveLoad.singleton().setCurrentState(CMenuSaveLoad.STATE.STATE_SLOT_DATA_SELECT);
								wmenu.CWMenuManager.Instance().SetSecondlyBGVisibility(b: true);
								MenuManager.getSingleton().Push("save_confirm");
								dgs.CCtrlCodeInterface.instance().setSlot((int)CMenuSaveLoad.singleton().getCurrentSlot());
							}
							wmenu.CWMenuManager.Instance().GetMenuButton().SetButtonBActivity(b: false);
						}
						else
						{
							MenuManager.getSingleton().playSEBeep();
						}
					}
					else
					{
						MenuManager.getSingleton().playSEBeep();
					}
				}
				else if (CMenuSaveLoad.singleton().getCurrentMode() == CMenuSaveLoad.MODE.MODE_SAVE)
				{
					MenuManager.getSingleton().playSEDecide();
					if (M._id(TRANSCODE("slot1")) || M._id(TRANSCODE("slot2")) || M._id(TRANSCODE("slot3")))
					{
						CMenuSaveLoad.singleton().setCurrentState(CMenuSaveLoad.STATE.STATE_SLOT_DATA_SELECT);
						wmenu.CWMenuManager.Instance().SetSecondlyBGVisibility(b: true);
						MenuManager.getSingleton().Push("save_confirm");
						MenuManager.getSingleton().initFocus(1);
						MenuManager.getSingleton().SetDecideButtonState(1);
						wmenu.CWMenuManager.Instance().GetMenuButton().SetButtonBActivity(b: false);
					}
				}
			}

			public void bmDecideDataSelect_(Medget M)
			{
				Medget nodeByIDFromChildren = M.parentNode().getNodeByIDFromChildren(TRANSCODE("confirmation"));
				if (nodeByIDFromChildren != null)
				{
					((MBText)nodeByIDFromChildren.behavior().queryInterface(MBText.classIdentifier()))?.mbtReleaseMessage();
				}
				if (M._id(TRANSCODE("YES")))
				{
					if (CMenuSaveLoad.singleton().getCurrentMode() == CMenuSaveLoad.MODE.MODE_SAVE)
					{
						MenuManager.getSingleton().SetDecideButtonState(0);
					}
					else if (CMenuSaveLoad.singleton().getCurrentMode() == CMenuSaveLoad.MODE.MODE_LOAD)
					{
						wmenu.CWMenuManager.Instance().GetMenuButton().SetButtonBActivity(b: false);
						wmenu.CWMenuManager.Instance().SetSecondlyBGVisibility(b: false);
						CMenuSaveLoad.singleton().setCurrentState(CMenuSaveLoad.STATE.STATE_SLOT_DATA_WAIT);
					}
					MenuManager.getSingleton().GetCursor2d().SetShow(show: false);
				}
				else if (M._id(TRANSCODE("NO")))
				{
					MenuManager.getSingleton().SetCancelButtonState(0);
				}
			}

			public override bool bmCancel(Medget M)
			{
				switch (CMenuSaveLoad.singleton().getCurrentState())
				{
				case CMenuSaveLoad.STATE.STATE_SELECT_SLOT:
					bmCancelSelectSlot_(M);
					break;
				case CMenuSaveLoad.STATE.STATE_SLOT_DATA_SELECT:
					bmCancelDataSelect_(M);
					break;
				}
				return true;
			}

			public void bmCancelSelectSlot_(Medget M)
			{
				MenuManager.getSingleton().playSECancel();
				CMenuSaveLoad.singleton().onEndFlag();
			}

			public void bmCancelDataSelect_(Medget M)
			{
				MenuManager.getSingleton().SetCancelButtonState(0);
			}

			public override bool bmDirection(Medget M, int Key)
			{
				return false;
			}

			public override void bmActivate(Medget M)
			{
				if (M._id(TRANSCODE("slot1")))
				{
					CMenuSaveLoad.singleton().setCurrentSlot(CMenuSaveLoad.SLOT.SLOT_1);
				}
				else if (M._id(TRANSCODE("slot2")))
				{
					CMenuSaveLoad.singleton().setCurrentSlot(CMenuSaveLoad.SLOT.SLOT_2);
				}
				else
				{
					if (!M._id(TRANSCODE("slot3")))
					{
						return;
					}
					CMenuSaveLoad.singleton().setCurrentSlot(CMenuSaveLoad.SLOT.SLOT_3);
				}
				if (card.Manager.GetInstance().GetState((int)CMenuSaveLoad.singleton().getCurrentSlot(), -1))
				{
					if (SaveDataMng.getSingleton().getSlotState((int)CMenuSaveLoad.singleton().getCurrentSlot()) == 1)
					{
						wmsRefresh(ownerMedget);
					}
					else if (SaveDataMng.getSingleton().getSlotState((int)CMenuSaveLoad.singleton().getCurrentSlot()) == 0)
					{
						wmsRefreshDataBroken(ownerMedget);
					}
				}
				else
				{
					wmsRefreshDataNone(ownerMedget);
				}
			}

			public override void bmDeactivate(Medget M)
			{
			}

			public void wmsRefresh(Medget M)
			{
				if (CMenuSaveLoad.singleton().getCurrentState() != CMenuSaveLoad.STATE.STATE_SELECT_SLOT)
				{
					return;
				}
				Medget medget = null;
				MBText mBText = null;
				if ((medget = M.getNodeByIDFromChildren(TRANSCODE("data_status"))) != null)
				{
					((MBText)medget.behavior().queryInterface(MBText.classIdentifier()))?.mbSetBufferMsg("", decWidth: false);
				}
				if ((medget = M.getNodeByIDFromChildren(TRANSCODE("name"))) != null)
				{
					medget = medget.childNode();
					for (int i = 0; i < 4; i++)
					{
						if (medget == null)
						{
							break;
						}
						MBText mBText2 = (MBText)medget.behavior().queryInterface(MBText.classIdentifier());
						if (mBText2 != null)
						{
							if (isChrEnable(i, (int)CMenuSaveLoad.singleton().getCurrentSlot()) == 0)
							{
								mBText2.mbSetBufferMsg(getChrNameStr(i, (int)CMenuSaveLoad.singleton().getCurrentSlot()), decWidth: false);
							}
							else
							{
								mBText2.mbSetBufferMsg("", decWidth: false);
							}
						}
						medget = medget.nextSibling();
					}
				}
				if ((medget = M.getNodeByIDFromChildren(TRANSCODE("level"))) != null)
				{
					medget = medget.childNode();
					for (int j = 0; j < 4; j++)
					{
						if (medget == null)
						{
							break;
						}
						MBText mBText3 = (MBText)medget.behavior().queryInterface(MBText.classIdentifier());
						if (mBText3 != null)
						{
							if (isChrEnable(j, (int)CMenuSaveLoad.singleton().getCurrentSlot()) == 0)
							{
								mBText3.mbSetBufferMsg(getChrLvStr(j, (int)CMenuSaveLoad.singleton().getCurrentSlot()), decWidth: false);
							}
							else
							{
								mBText3.mbSetBufferMsg("", decWidth: false);
							}
						}
						medget = medget.nextSibling();
					}
				}
				if ((medget = M.getNodeByIDFromChildren(TRANSCODE("job"))) != null)
				{
					medget = medget.childNode();
					for (int k = 0; k < 4; k++)
					{
						if (medget == null)
						{
							break;
						}
						MBText mBText4 = (MBText)medget.behavior().queryInterface(MBText.classIdentifier());
						if (mBText4 != null)
						{
							if (isChrEnable(k, (int)CMenuSaveLoad.singleton().getCurrentSlot()) == 0)
							{
								mBText4.mbSetBufferMsg(getChrJobStr(k, (int)CMenuSaveLoad.singleton().getCurrentSlot()), decWidth: false);
							}
							else
							{
								mBText4.mbSetBufferMsg("", decWidth: false);
							}
						}
						medget = medget.nextSibling();
					}
				}
				if ((medget = M.getNodeByIDFromChildren(TRANSCODE("skill"))) != null)
				{
					medget = medget.childNode();
					for (int l = 0; l < 4; l++)
					{
						if (medget == null)
						{
							break;
						}
						MBText mBText5 = (MBText)medget.behavior().queryInterface(MBText.classIdentifier());
						if (mBText5 != null)
						{
							if (isChrEnable(l, (int)CMenuSaveLoad.singleton().getCurrentSlot()) == 0)
							{
								mBText5.mbSetBufferMsg(getChrSkillStr(l, (int)CMenuSaveLoad.singleton().getCurrentSlot()), decWidth: false);
							}
							else
							{
								mBText5.mbSetBufferMsg("", decWidth: false);
							}
						}
						medget = medget.nextSibling();
					}
				}
				if ((medget = M.getNodeByIDFromChildren(TRANSCODE("skillsymbol"))) != null)
				{
					medget = medget.childNode();
					for (int m = 0; m < 4; m++)
					{
						if (medget == null)
						{
							break;
						}
						MBText mBText6 = (MBText)medget.behavior().queryInterface(MBText.classIdentifier());
						if (mBText6 != null)
						{
							if (isChrEnable(m, (int)CMenuSaveLoad.singleton().getCurrentSlot()) == 0)
							{
								mBText6.bmTextVisibility(v: true);
							}
							else
							{
								mBText6.bmTextVisibility(v: false);
							}
						}
						medget = medget.nextSibling();
					}
				}
				if ((medget = M.getNodeByIDFromChildren(TRANSCODE("hpsymbol"))) != null)
				{
					medget = medget.childNode();
					for (int n = 0; n < 4; n++)
					{
						if (medget == null)
						{
							break;
						}
						MBText mBText7 = (MBText)medget.behavior().queryInterface(MBText.classIdentifier());
						if (mBText7 != null)
						{
							if (isChrEnable(n, (int)CMenuSaveLoad.singleton().getCurrentSlot()) == 0)
							{
								mBText7.bmTextVisibility(v: true);
							}
							else
							{
								mBText7.bmTextVisibility(v: false);
							}
						}
						medget = medget.nextSibling();
					}
				}
				if ((medget = M.getNodeByIDFromChildren(TRANSCODE("hp"))) != null)
				{
					medget = medget.childNode();
					for (int num = 0; num < 4; num++)
					{
						if (medget == null)
						{
							break;
						}
						MBText mBText8 = (MBText)medget.behavior().queryInterface(MBText.classIdentifier());
						if (mBText8 != null)
						{
							if (isChrEnable(num, (int)CMenuSaveLoad.singleton().getCurrentSlot()) == 0)
							{
								card.CSaveData cSaveData = SaveDataMng.getSingleton().SaveData((int)CMenuSaveLoad.singleton().getCurrentSlot());
								if (cSaveData != null)
								{
									mBText8.mbSetBufferNumber(cSaveData.composit.getHP(num));
									mBText8.getMessage().setMessageColor(cSaveData.composit.getHPColor(num));
									mBText8.bmTextVisibility(v: true);
								}
							}
							else
							{
								mBText8.bmTextVisibility(v: false);
							}
						}
						medget = medget.nextSibling();
					}
				}
				if ((medget = M.getNodeByIDFromChildren(TRANSCODE("slash"))) != null)
				{
					medget = medget.childNode();
					for (int num2 = 0; num2 < 4; num2++)
					{
						if (medget == null)
						{
							break;
						}
						MBText mBText9 = (MBText)medget.behavior().queryInterface(MBText.classIdentifier());
						if (mBText9 != null)
						{
							if (isChrEnable(num2, (int)CMenuSaveLoad.singleton().getCurrentSlot()) == 0)
							{
								mBText9.bmTextVisibility(v: true);
							}
							else
							{
								mBText9.bmTextVisibility(v: false);
							}
						}
						medget = medget.nextSibling();
					}
				}
				if ((medget = M.getNodeByIDFromChildren(TRANSCODE("max_hp"))) != null)
				{
					medget = medget.childNode();
					for (int num3 = 0; num3 < 4; num3++)
					{
						if (medget == null)
						{
							break;
						}
						MBText mBText10 = (MBText)medget.behavior().queryInterface(MBText.classIdentifier());
						if (mBText10 != null)
						{
							if (isChrEnable(num3, (int)CMenuSaveLoad.singleton().getCurrentSlot()) == 0)
							{
								mBText10.mbSetBufferMsg(getChrHPStr(num3, (int)CMenuSaveLoad.singleton().getCurrentSlot()), decWidth: false);
								mBText10.bmTextVisibility(v: true);
								if (SaveDataMng.getSingleton().SaveData((int)CMenuSaveLoad.singleton().getCurrentSlot()) != null)
								{
									mBText10.getMessage().setMessageColor(SaveDataMng.getSingleton().SaveData((int)CMenuSaveLoad.singleton().getCurrentSlot()).composit.getHPColor(num3));
								}
							}
							else
							{
								mBText10.bmTextVisibility(v: false);
							}
						}
						medget = medget.nextSibling();
					}
				}
				if ((medget = M.getNodeByIDFromChildren(TRANSCODE("next_level_sym"))) != null)
				{
					medget = medget.childNode();
					for (int num4 = 0; num4 < 4; num4++)
					{
						if (medget == null)
						{
							break;
						}
						MBText mBText11 = (MBText)medget.behavior().queryInterface(MBText.classIdentifier());
						if (mBText11 != null)
						{
							if (isChrEnable(num4, (int)CMenuSaveLoad.singleton().getCurrentSlot()) == 0)
							{
								mBText11.bmTextVisibility(v: true);
							}
							else
							{
								mBText11.bmTextVisibility(v: false);
							}
						}
						medget = medget.nextSibling();
					}
				}
				if ((medget = M.getNodeByIDFromChildren(TRANSCODE("next_level"))) != null)
				{
					medget = medget.childNode();
					for (int num5 = 0; num5 < 4; num5++)
					{
						if (medget == null)
						{
							break;
						}
						MBText mBText12 = (MBText)medget.behavior().queryInterface(MBText.classIdentifier());
						if (mBText12 != null)
						{
							if (isChrEnable(num5, (int)CMenuSaveLoad.singleton().getCurrentSlot()) == 0)
							{
								card.CSaveData cSaveData2 = SaveDataMng.getSingleton().SaveData((int)CMenuSaveLoad.singleton().getCurrentSlot());
								if (cSaveData2 != null)
								{
									mBText12.mbSetBufferNumber(cSaveData2.composit.getNextLevelExp(num5));
									mBText12.bmTextVisibility(v: true);
								}
							}
							else
							{
								mBText12.bmTextVisibility(v: false);
							}
						}
						medget = medget.nextSibling();
					}
				}
				if ((medget = M.getNodeByID(TRANSCODE("money"))) != null)
				{
					mBText = (MBText)medget.behavior().queryInterface(MBText.classIdentifier());
					if (mBText != null)
					{
						string goldStr = getGoldStr((int)CMenuSaveLoad.singleton().getCurrentSlot());
						mBText.mbSetBufferMsg(goldStr, decWidth: false);
					}
				}
				if ((medget = M.getNodeByID(TRANSCODE("time"))) != null)
				{
					mBText = (MBText)medget.behavior().queryInterface(MBText.classIdentifier());
					string playTimeStr = getPlayTimeStr((int)CMenuSaveLoad.singleton().getCurrentSlot());
					mBText.mbSetBufferMsg(playTimeStr, decWidth: false);
				}
				CMenuSaveLoad.singleton().setClearMarkVisibility(isClearedData((int)CMenuSaveLoad.singleton().getCurrentSlot()));
				M.setPriority(3);
			}

			public void wmsRefreshDataNone(Medget M)
			{
				Medget medget = null;
				if ((medget = M.getNodeByIDFromChildren(TRANSCODE("data_status"))) != null)
				{
					((MBText)medget.behavior().queryInterface(MBText.classIdentifier()))?.mbSetTextMsgNo(50711);
				}
				if ((medget = M.getNodeByIDFromChildren(TRANSCODE("name"))) != null)
				{
					medget = medget.childNode();
					for (int i = 0; i < 4; i++)
					{
						if (medget == null)
						{
							break;
						}
						((MBText)medget.behavior().queryInterface(MBText.classIdentifier()))?.mbSetBufferMsg("", decWidth: false);
						medget = medget.nextSibling();
					}
				}
				if ((medget = M.getNodeByIDFromChildren(TRANSCODE("level"))) != null)
				{
					medget = medget.childNode();
					for (int j = 0; j < 4; j++)
					{
						if (medget == null)
						{
							break;
						}
						((MBText)medget.behavior().queryInterface(MBText.classIdentifier()))?.mbSetBufferMsg("", decWidth: false);
						medget = medget.nextSibling();
					}
				}
				if ((medget = M.getNodeByIDFromChildren(TRANSCODE("job"))) != null)
				{
					medget = medget.childNode();
					for (int k = 0; k < 4; k++)
					{
						if (medget == null)
						{
							break;
						}
						((MBText)medget.behavior().queryInterface(MBText.classIdentifier()))?.mbSetBufferMsg("", decWidth: false);
						medget = medget.nextSibling();
					}
				}
				if ((medget = M.getNodeByIDFromChildren(TRANSCODE("skill"))) != null)
				{
					medget = medget.childNode();
					for (int l = 0; l < 4; l++)
					{
						if (medget == null)
						{
							break;
						}
						((MBText)medget.behavior().queryInterface(MBText.classIdentifier()))?.mbSetBufferMsg("", decWidth: false);
						medget = medget.nextSibling();
					}
				}
				if ((medget = M.getNodeByIDFromChildren(TRANSCODE("skillsymbol"))) != null)
				{
					medget = medget.childNode();
					for (int m = 0; m < 4; m++)
					{
						if (medget == null)
						{
							break;
						}
						((MBText)medget.behavior().queryInterface(MBText.classIdentifier()))?.bmTextVisibility(v: false);
						medget = medget.nextSibling();
					}
				}
				if ((medget = M.getNodeByIDFromChildren(TRANSCODE("hpsymbol"))) != null)
				{
					medget = medget.childNode();
					for (int n = 0; n < 4; n++)
					{
						if (medget == null)
						{
							break;
						}
						((MBText)medget.behavior().queryInterface(MBText.classIdentifier()))?.bmTextVisibility(v: false);
						medget = medget.nextSibling();
					}
				}
				if ((medget = M.getNodeByIDFromChildren(TRANSCODE("hp"))) != null)
				{
					medget = medget.childNode();
					for (int num = 0; num < 4; num++)
					{
						if (medget == null)
						{
							break;
						}
						((MBText)medget.behavior().queryInterface(MBText.classIdentifier()))?.bmTextVisibility(v: false);
						medget = medget.nextSibling();
					}
				}
				if ((medget = M.getNodeByIDFromChildren(TRANSCODE("slash"))) != null)
				{
					medget = medget.childNode();
					for (int num2 = 0; num2 < 4; num2++)
					{
						if (medget == null)
						{
							break;
						}
						((MBText)medget.behavior().queryInterface(MBText.classIdentifier()))?.bmTextVisibility(v: false);
						medget = medget.nextSibling();
					}
				}
				if ((medget = M.getNodeByIDFromChildren(TRANSCODE("max_hp"))) != null)
				{
					medget = medget.childNode();
					for (int num3 = 0; num3 < 4; num3++)
					{
						if (medget == null)
						{
							break;
						}
						((MBText)medget.behavior().queryInterface(MBText.classIdentifier()))?.bmTextVisibility(v: false);
						medget = medget.nextSibling();
					}
				}
				if ((medget = M.getNodeByIDFromChildren(TRANSCODE("next_level"))) != null)
				{
					medget = medget.childNode();
					for (int num4 = 0; num4 < 4; num4++)
					{
						if (medget == null)
						{
							break;
						}
						((MBText)medget.behavior().queryInterface(MBText.classIdentifier()))?.bmTextVisibility(v: false);
						medget = medget.nextSibling();
					}
				}
				if ((medget = M.getNodeByIDFromChildren(TRANSCODE("next_level_sym"))) != null)
				{
					medget = medget.childNode();
					for (int num5 = 0; num5 < 4; num5++)
					{
						if (medget == null)
						{
							break;
						}
						((MBText)medget.behavior().queryInterface(MBText.classIdentifier()))?.bmTextVisibility(v: false);
						medget = medget.nextSibling();
					}
				}
				if ((medget = M.getNodeByID(TRANSCODE("money"))) != null)
				{
					((MBText)medget.behavior().queryInterface(MBText.classIdentifier()))?.mbSetBufferMsg("", decWidth: false);
				}
				bmSetVisiblity(M, b: false, "timesymbol");
				if ((medget = M.getNodeByID(TRANSCODE("time"))) != null)
				{
					((MBText)medget.behavior().queryInterface(MBText.classIdentifier()))?.mbSetBufferMsg("", decWidth: false);
				}
				CMenuSaveLoad.singleton().setClearMarkVisibility(b: false);
				M.setPriority(3);
			}

			public void wmsRefreshDataBroken(Medget M)
			{
				Medget medget = null;
				if ((medget = M.getNodeByIDFromChildren(TRANSCODE("data_status"))) != null)
				{
					MBText mBText = (MBText)medget.behavior().queryInterface(MBText.classIdentifier());
					if (mBText != null)
					{
						mBText.mbSetTextMsgNo(50712);
						if (mBText.getMessage() != null)
						{
							mBText.getMessage().setStyle(1024u);
						}
					}
				}
				if ((medget = M.getNodeByIDFromChildren(TRANSCODE("name"))) != null)
				{
					medget = medget.childNode();
					for (int i = 0; i < 4; i++)
					{
						if (medget == null)
						{
							break;
						}
						((MBText)medget.behavior().queryInterface(MBText.classIdentifier()))?.mbSetBufferMsg("", decWidth: false);
						medget = medget.nextSibling();
					}
				}
				if ((medget = M.getNodeByIDFromChildren(TRANSCODE("level"))) != null)
				{
					medget = medget.childNode();
					for (int j = 0; j < 4; j++)
					{
						if (medget == null)
						{
							break;
						}
						((MBText)medget.behavior().queryInterface(MBText.classIdentifier()))?.mbSetBufferMsg("", decWidth: false);
						medget = medget.nextSibling();
					}
				}
				if ((medget = M.getNodeByIDFromChildren(TRANSCODE("job"))) != null)
				{
					medget = medget.childNode();
					for (int k = 0; k < 4; k++)
					{
						if (medget == null)
						{
							break;
						}
						((MBText)medget.behavior().queryInterface(MBText.classIdentifier()))?.mbSetBufferMsg("", decWidth: false);
						medget = medget.nextSibling();
					}
				}
				if ((medget = M.getNodeByIDFromChildren(TRANSCODE("skill"))) != null)
				{
					medget = medget.childNode();
					for (int l = 0; l < 4; l++)
					{
						if (medget == null)
						{
							break;
						}
						((MBText)medget.behavior().queryInterface(MBText.classIdentifier()))?.mbSetBufferMsg("", decWidth: false);
						medget = medget.nextSibling();
					}
				}
				if ((medget = M.getNodeByIDFromChildren(TRANSCODE("skillsymbol"))) != null)
				{
					medget = medget.childNode();
					for (int m = 0; m < 4; m++)
					{
						if (medget == null)
						{
							break;
						}
						((MBText)medget.behavior().queryInterface(MBText.classIdentifier()))?.bmTextVisibility(v: false);
						medget = medget.nextSibling();
					}
				}
				if ((medget = M.getNodeByIDFromChildren(TRANSCODE("hpsymbol"))) != null)
				{
					medget = medget.childNode();
					for (int n = 0; n < 4; n++)
					{
						if (medget == null)
						{
							break;
						}
						((MBText)medget.behavior().queryInterface(MBText.classIdentifier()))?.bmTextVisibility(v: false);
						medget = medget.nextSibling();
					}
				}
				if ((medget = M.getNodeByIDFromChildren(TRANSCODE("hp"))) != null)
				{
					medget = medget.childNode();
					for (int num = 0; num < 4; num++)
					{
						if (medget == null)
						{
							break;
						}
						((MBText)medget.behavior().queryInterface(MBText.classIdentifier()))?.bmTextVisibility(v: false);
						medget = medget.nextSibling();
					}
				}
				if ((medget = M.getNodeByIDFromChildren(TRANSCODE("slash"))) != null)
				{
					medget = medget.childNode();
					for (int num2 = 0; num2 < 4; num2++)
					{
						if (medget == null)
						{
							break;
						}
						((MBText)medget.behavior().queryInterface(MBText.classIdentifier()))?.bmTextVisibility(v: false);
						medget = medget.nextSibling();
					}
				}
				if ((medget = M.getNodeByIDFromChildren(TRANSCODE("max_hp"))) != null)
				{
					medget = medget.childNode();
					for (int num3 = 0; num3 < 4; num3++)
					{
						if (medget == null)
						{
							break;
						}
						((MBText)medget.behavior().queryInterface(MBText.classIdentifier()))?.bmTextVisibility(v: false);
						medget = medget.nextSibling();
					}
				}
				if ((medget = M.getNodeByIDFromChildren(TRANSCODE("next_level"))) != null)
				{
					medget = medget.childNode();
					for (int num4 = 0; num4 < 4; num4++)
					{
						if (medget == null)
						{
							break;
						}
						((MBText)medget.behavior().queryInterface(MBText.classIdentifier()))?.bmTextVisibility(v: false);
						medget = medget.nextSibling();
					}
				}
				if ((medget = M.getNodeByIDFromChildren(TRANSCODE("next_level_sym"))) != null)
				{
					medget = medget.childNode();
					for (int num5 = 0; num5 < 4; num5++)
					{
						if (medget == null)
						{
							break;
						}
						((MBText)medget.behavior().queryInterface(MBText.classIdentifier()))?.bmTextVisibility(v: false);
						medget = medget.nextSibling();
					}
				}
				if ((medget = M.getNodeByID(TRANSCODE("money"))) != null)
				{
					((MBText)medget.behavior().queryInterface(MBText.classIdentifier()))?.mbSetBufferMsg("", decWidth: false);
				}
				bmSetVisiblity(M, b: false, "timesymbol");
				if ((medget = M.getNodeByID(TRANSCODE("time"))) != null)
				{
					((MBText)medget.behavior().queryInterface(MBText.classIdentifier()))?.mbSetBufferMsg("", decWidth: false);
				}
				CMenuSaveLoad.singleton().setClearMarkVisibility(b: false);
				M.setPriority(3);
			}

			public bool Load_()
			{
				SaveDataMng.getSingleton().SaveData((int)CMenuSaveLoad.singleton().getCurrentSlot()).sdReflect();
				return true;
			}

			public bool Save_()
			{
				return false;
			}

			public void bmSetVisiblity(Medget M, bool b, string _id)
			{
				Medget nodeByID = M.getNodeByID(TRANSCODE(_id));
				if (nodeByID != null)
				{
					MBText mBText = (MBText)nodeByID.behavior().queryInterface(MBText.classIdentifier());
					mBText.bmTextVisibility(b);
				}
			}

			public new static int classIdentifier()
			{
				return MBSaveLoad_UN.number();
			}

			public override object queryInterface(int class_id)
			{
				if (class_id == classIdentifier())
				{
					return this;
				}
				return null;
			}
		}
	}
}
