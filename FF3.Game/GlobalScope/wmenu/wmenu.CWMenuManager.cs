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
	public static partial class wmenu
	{
							public class CWMenuManager
							{
								public class MORE_STRENGTH
								{
									public int more_id;

									public int more_index;
								}

								public const int SUB_BG_RIGHT_USE = 0;

								public const int SUB_BG_LEFT_USE = 1;

								public const int SUB_BG_END_SAVE = 2;

								public const int SUB_BG_PLANE = 3;

								public const int SUB_MENU_BG_MAX = 4;

								public const int MORE_EQUIP_HAND = 0;

								public const int MORE_EQUIP_HEAD = 1;

								public const int MORE_EQUIP_BODY = 2;

								public const int MORE_EQUIP_GUNT = 3;

								public const int MORE_EQUIP_SHIELD = 4;

								public const int FORMATION_STATE_SELECT = 0;

								public const int FORMATION_STATE_DECIDE = 1;

								public const int FORMATION_STATE_DRAG = 2;

								public static CWMenuManager c_Instance = new CWMenuManager();

								private NNSG2dSVec2[] CharFace_Pos = new NNSG2dSVec2[4]
								{
									new NNSG2dSVec2(1, 1),
									new NNSG2dSVec2(1, 10),
									new NNSG2dSVec2(1, 19),
									new NNSG2dSVec2(1, 28)
								};

								private static NNSG2dBGSelect primaryBgSelect = NNSG2dBGSelect.NNS_G2D_BGSELECT_SUB3;

								private static NNSG2dBGSelect secondlyBgSelect = NNSG2dBGSelect.NNS_G2D_BGSELECT_SUB1;

								private static string[] main_bg_nscr = new string[15]
								{
									"menu_001_item.NSCR", "menu_002_magic.NSCR", "menu_003_soubi_01.NSCR", "menu_004_status.NSCR", null, "menu_006_job.NSCR", "menu_010_config.NSCR", "menu_008_tyudan.NSCR", "menu_009_save.NSCR", "menu_000_main.NSCR",
									"menu_012_plane.NSCR", "menu_002_magic_02.NSCR", "menu_010_config_2.NSCR", "tips_00.NSCR", "tips_01.NSCR"
								};

								private static string[] sub_bg_nscr = new string[4] { "menu_001_item_01.NSCR", "menu_002_magic_01.NSCR", "menu_011_save02.NSCR", "menu_013_plane.NSCR" };

								private CWMenuMemberBase.WMENU_PROCESS m_ProcState;

								private CWMenuMemberBase.WMENU_KIND m_Kind;

								private CWMenuMemberBase.WMENU_KIND m_Next;

								private CWMenuMemberBase.WMENU_KIND m_Prev;

								private CWMenuMemberBase.WMENU_KIND m_StartupKind;

								private int focusePrevNo;

								private bool myState;

								private sys2d.Cell dummyCursor = new sys2d.Cell();

								private sys2d.Cell formationCursor = new sys2d.Cell();

								private CWMenuPCFaceManager _pcFace = new CWMenuPCFaceManager();

								private Array scrDataPtr;

								private sys2d.Bg primaryBG = new sys2d.Bg();

								private sys2d.Bg secondlyBG = new sys2d.Bg();

								private bool finalize;

								private bool bgFinalize;

								private CWMenuButton menuButton = new CWMenuButton();

								private int main_menu_cursor_memory;

								private int LoadCampGroup_;

								private CWMenuMemberBase[] m_pCurrent = new CWMenuMemberBase[15];

								private CWMenuMain m_MenuMain = new CWMenuMain();

								private CWMenuJob m_MenuJob = new CWMenuJob();

								private CWMenuItem m_MenuItem = new CWMenuItem();

								private CWMenuMagic m_MenuMagic = new CWMenuMagic();

								private CWMenuEquip m_MenuEquip = new CWMenuEquip();

								private CWMenuStatus m_MenuStatus = new CWMenuStatus();

								private CWMenuConfig m_MenuConfig = new CWMenuConfig();

								private CWMenuSuspend m_MenuSuspend = new CWMenuSuspend();

								private CWMenuSave m_MenuSave = new CWMenuSave();

								private CWMenuFormation m_MenuFormation = new CWMenuFormation();

								public void SetPrimaryBG(int no)
								{
									if (ds.g_File.load(scrDataPtr, main_bg_nscr[no]))
									{
										OS_Printf("File[%s] loaded.\n", main_bg_nscr[no]);
									}
									NNSG2dCellDataBank nNSG2dCellDataBank = new NNSG2dCellDataBank();
									NNS_G2dGetUnpackedCellBank(scrDataPtr, nNSG2dCellDataBank);
									if (nNSG2dCellDataBank != null)
									{
										NNS_G2dBGSetupCell((int)primaryBgSelect, nNSG2dCellDataBank, primaryBgSelect);
									}
								}

								public void SetSecondlyBG(int no)
								{
									OS_Printf("scrDataPtr = 0x%08x\n", scrDataPtr);
									ds.g_File.load(scrDataPtr, sub_bg_nscr[no]);
									NNSG2dCellDataBank nNSG2dCellDataBank = new NNSG2dCellDataBank();
									NNS_G2dGetUnpackedCellBank(scrDataPtr, nNSG2dCellDataBank);
									if (nNSG2dCellDataBank != null)
									{
										NNS_G2dBGSetupCell((int)secondlyBgSelect, nNSG2dCellDataBank, secondlyBgSelect);
									}
								}

								public void SetupSecondlyBG(int no)
								{
									secondlyBG.bgLoad(sub_bg_nscr[no], "menu_bg_01.NCGR", "new_menu_bg.NCLR");
									secondlyBG.bgSetUp(secondlyBgSelect);
									secondlyBG.bgRelease();
									secondlyBG.bgSetShow(show: false);
								}

								public void clear()
								{
									myState = true;
									m_Kind = m_StartupKind;
									m_Next = CWMenuMemberBase.WMENU_KIND.WMENU_KIND_STATUS;
									m_Prev = CWMenuMemberBase.WMENU_KIND.WMENU_KIND_STATUS;
									menu.MenuManager.getSingleton().SetItemListPatern(0);
									main_menu_cursor_memory = 0;
									menu.MenuManager.getSingleton().SetActivateButtonState(1);
									menu.MenuManager.getSingleton().ClearBehaviorButton();
									SetProcState(CWMenuMemberBase.WMENU_PROCESS.WMENU_PROCESS_RUN);
									m_MenuSave.endingSaveSetting(b: false);
								}

								public void initialize()
								{
									clear();
									int size = 16384;
									scrDataPtr = ds.CHeap.alloc_app((uint)size);
									finalize = false;
									menu.MenuManager.getSingleton().GetCursor2d().SetCell(0);
									menu.MenuManager.getSingleton().GetCursor2d().SetShow(show: true);
									menu.MenuManager.getSingleton().GetCursor2d().SetPriority(0);
									menu.CMenuSaveLoad.singleton().setupClearMark();
									GetMenuButton().initialize();
									formationCursor.copy(menu.MenuManager.getSingleton().GetCursor2d());
									formationCursor.SetCell(3);
									formationCursor.SetShow(show: false);
									formationCursor.SetDepth(1);
									formationCursor.SetAnimation(anm: false);
									sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(formationCursor);
									dummyCursor.copy(menu.MenuManager.getSingleton().GetCursor2d());
									dummyCursor.SetPositionI(LCD_WIDTH, LCD_HEIGHT);
									dummyCursor.SetCell(3);
									dummyCursor.SetShow(show: false);
									dummyCursor.SetAnimation(anm: false);
									sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(dummyCursor);
									primaryBG.bgLoad("menu_000_main.NSCR", "menu_bg_01.NCGR", "new_menu_bg.NCLR");
									primaryBG.bgSetUp(primaryBgSelect);
									primaryBG.bgRelease();
									primaryBG.bgSetShow(show: false);
									secondlyBG.bgLoad("menu_000_main.NSCR", "menu_bg_01.NCGR", "new_menu_bg.NCLR");
									secondlyBG.bgSetUp(secondlyBgSelect);
									secondlyBG.bgRelease();
									secondlyBG.bgSetShow(show: false);
									GetWMenuMain().setUpCharScrPos();
									pCurrent().initialize();
								}

								public void run()
								{
									switch (m_ProcState)
									{
									case CWMenuMemberBase.WMENU_PROCESS.WMENU_PROCESS_CSELECTRUN:
										CSelectRun();
										break;
									case CWMenuMemberBase.WMENU_PROCESS.WMENU_PROCESS_CSELECTTERMINATE:
										if (m_Kind != CWMenuMemberBase.WMENU_KIND.WMENU_KIND_MAIN_MENU)
										{
											ds.g_Pad.disable();
											ds.g_TouchPanel.disable();
											dgs.CFade.Sub().fadeOut(4, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
											CSelectTerminate();
											m_ProcState = CWMenuMemberBase.WMENU_PROCESS.WMENU_PROCESS_CSELECTFADEOUT;
										}
										else
										{
											menu.MenuManager.getSingleton().initFocus(Instance().GetMainMenuMemoryCursor());
											Instance().GetDummyCursor().SetShow(show: false);
											m_ProcState = CWMenuMemberBase.WMENU_PROCESS.WMENU_PROCESS_RUN;
										}
										break;
									case CWMenuMemberBase.WMENU_PROCESS.WMENU_PROCESS_CSELECTFADEOUT:
										if (dgs.CFade.Sub().isFaded())
										{
											menu.MenuManager.getSingleton().GetCursor2d().SetShow(show: false);
											menu.MenuManager.getSingleton().GetCursor2d().SetShow(show: true);
											pCurrent().initialize();
											dgs.CFade.Sub().fadeIn(4);
											ds.g_Pad.enable();
											ds.g_TouchPanel.enable();
											m_ProcState = CWMenuMemberBase.WMENU_PROCESS.WMENU_PROCESS_RUN;
										}
										break;
									case CWMenuMemberBase.WMENU_PROCESS.WMENU_PROCESS_INITIALIZE:
										pCurrent().initialize();
										m_ProcState = CWMenuMemberBase.WMENU_PROCESS.WMENU_PROCESS_RUN;
										break;
									case CWMenuMemberBase.WMENU_PROCESS.WMENU_PROCESS_RUN:
										pCurrent().run();
										break;
									case CWMenuMemberBase.WMENU_PROCESS.WMENU_PROCESS_TERMINATE:
									{
										pCurrent().terminate();
										m_Prev = m_Kind;
										m_Kind = m_Next;
										m_ProcState = CWMenuMemberBase.WMENU_PROCESS.WMENU_PROCESS_CSELECTINITIALIZE;
										bool flag = pCurrent().cSelectInitialize();
										if (m_Prev == CWMenuMemberBase.WMENU_KIND.WMENU_KIND_MAIN_MENU && Instance().GetMainMenuMemoryCursor() == 3 && menu.MenuManager.getSingleton().getFocuseMedget().myTag() >= 9 && pl.PlayerParty.instance().player((byte)menu.MenuManager.getSingleton().getFocuseMedget().work()).isEnable())
										{
											flag = false;
										}
										if (flag)
										{
											CSelectInitialize();
											if (m_Prev != CWMenuMemberBase.WMENU_KIND.WMENU_KIND_MAIN_MENU)
											{
												sys2d.DS2DManager.d2dGetInstance().d2dDraw();
											}
											m_ProcState = CWMenuMemberBase.WMENU_PROCESS.WMENU_PROCESS_CSELECTRUN;
										}
										else
										{
											ds.g_Pad.disable();
											ds.g_TouchPanel.disable();
											dgs.CFade.Sub().fadeOut(4, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
											CSelectTerminate();
											m_ProcState = CWMenuMemberBase.WMENU_PROCESS.WMENU_PROCESS_FADEOUT;
										}
										break;
									}
									case CWMenuMemberBase.WMENU_PROCESS.WMENU_PROCESS_FADEOUT:
										if (dgs.CFade.Sub().isFaded())
										{
											menu.MenuManager.getSingleton().GetCursor2d().SetShow(show: false);
											menu.MenuManager.getSingleton().GetCursor2d().SetShow(show: true);
											pCurrent().initialize();
											dgs.CFade.Sub().fadeIn(4);
											ds.g_Pad.enable();
											ds.g_TouchPanel.enable();
											m_ProcState = CWMenuMemberBase.WMENU_PROCESS.WMENU_PROCESS_RUN;
										}
										break;
									case CWMenuMemberBase.WMENU_PROCESS.WMENU_PROCESS_END:
										pCurrent().terminate();
										myState = false;
										break;
									}
									menu.MenuManager.getSingleton().ClearBehaviorButton();
									menu.MenuManager.getSingleton().SetActivateButtonState(1);
								}

								public void terminate()
								{
									if (!finalize)
									{
										menu.CMenuSaveLoad.singleton().releaseClearMark();
										menu.MenuManager.getSingleton().GetCursor2d().SetShow(show: false);
										sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(dummyCursor);
										dummyCursor.Release();
										sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(formationCursor);
										formationCursor.Release();
										Instance().GetMenuButton().terminate();
										menu.MenuManager.getSingleton().releaseWindowAll();
										menu.MenuManager.getSingleton().release();
										m_ProcState = CWMenuMemberBase.WMENU_PROCESS.WMENU_PROCESS_INITIALIZE;
										m_Kind = CWMenuMemberBase.WMENU_KIND.WMENU_KIND_ITEM;
										m_Next = CWMenuMemberBase.WMENU_KIND.WMENU_KIND_ITEM;
										if (scrDataPtr != null)
										{
											ds.CHeap.free_app(scrDataPtr);
											scrDataPtr = null;
										}
										m_StartupKind = CWMenuMemberBase.WMENU_KIND.WMENU_KIND_MAIN_MENU;
										finalize = true;
									}
								}

								public void RegisterMenuKind()
								{
									m_pCurrent[5] = m_MenuJob;
									m_pCurrent[0] = m_MenuItem;
									m_pCurrent[1] = m_MenuMagic;
									m_pCurrent[2] = m_MenuEquip;
									m_pCurrent[4] = m_MenuFormation;
									m_pCurrent[6] = m_MenuConfig;
									m_pCurrent[7] = m_MenuSuspend;
									m_pCurrent[8] = m_MenuSave;
									m_pCurrent[3] = m_MenuStatus;
									m_pCurrent[9] = m_MenuMain;
								}

								public void CSelectInitialize()
								{
									int focusedCursor = 9;
									menu.MenuManager.getSingleton().initFocus(0);
									menu.Medget medget = null;
									GetWMenuFormation().SetFormationState(0);
									for (medget = menu.MenuManager.getSingleton().getFocuseMedget().parentNode()
										.childNode(); medget != null; medget = medget.nextSibling())
									{
										if ((sbyte)medget.work() == Instance().GetMainMenuMemoryCursor())
										{
											dummyCursor.SetShow(show: true);
											dummyCursor.SetPositionI(medget.cursorX(), medget.cursorY() + 2);
											break;
										}
									}
									menu.Medget medget2 = menu.MenuManager.getSingleton().getFocuseMedget().parentNode()
										.nextSibling();
									for (medget = medget2.childNode(); medget != null; medget = medget.nextSibling())
									{
										menu.MenuManager.getSingleton().joinFocusList(medget);
									}
									menu.MenuManager.getSingleton().initFocus(focusedCursor);
									menu.MenuManager.getSingleton().GetCursor2d().SetShow(show: true);
									focusePrevNo = focusedCursor;
								}

								public void CSelectRun()
								{
									menu.MenuManager.getSingleton().execute();
									if (Instance().GetKind() == CWMenuMemberBase.WMENU_KIND.WMENU_KIND_FORMATION)
									{
										GetWMenuFormation().CSelectRun();
									}
									if (strcmp(menu.MenuManager.getSingleton().getFocuseMedget().parentNode()
										._id(), TRANSCODE("char_select")) == 0)
									{
										focusePrevNo = menu.MenuManager.getSingleton().getFocuseMedget().myTag();
									}
									if (menu.MenuManager.getSingleton().GetCancelButtonState() == 0 || Instance().GetMenuButton().TouchButtonB())
									{
										menu.MenuManager.getSingleton().playSECancel();
										if (GetWMenuFormation().GetFormationState() == 1)
										{
											GetWMenuFormation().Cancel();
											return;
										}
										menu.Medget medget = menu.MenuManager.getSingleton().getFocuseMedget().parentNode();
										for (menu.Medget medget2 = medget.childNode(); medget2 != null; medget2 = medget2.nextSibling())
										{
											menu.MenuManager.getSingleton().leaveFocusList(medget2);
										}
										Instance().SetNextKind(Instance().GetPrevKind());
										Instance().SetPrevKind(Instance().GetKind());
										Instance().SetKind(Instance().GetNextKind());
										Instance().SetProcState(CWMenuMemberBase.WMENU_PROCESS.WMENU_PROCESS_CSELECTTERMINATE);
									}
									else if (menu.MenuManager.getSingleton().GetActivateButtonState() == 0 && strcmp(menu.MenuManager.getSingleton().getFocuseMedget().parentNode()
										._id(), TRANSCODE("main_command")) == 0)
									{
										menu.MenuManager.getSingleton().playSECancel();
										formationCursor.SetShow(show: false);
										menu.Medget medget3 = menu.MenuManager.getSingleton().getFocuseMedget().parentNode()
											.nextSibling();
										menu.Medget medget4 = medget3.childNode();
										for (int i = 0; i < 4; i++)
										{
											if (pl.PlayerParty.instance().player((byte)i).isEnable())
											{
												menu.MenuManager.getSingleton().leaveFocusList(medget4);
											}
											menu.Medget medget5 = medget4.nextSibling();
											if (medget5 != null)
											{
												medget4 = medget5;
											}
										}
										Instance().GetDummyCursor().SetShow(show: false);
										Instance().SetMainMenuMemoryCursor((sbyte)menu.MenuManager.getSingleton().getFocuseMedget().work());
										Instance().SetNextKind(Instance().GetPrevKind());
										Instance().SetPrevKind(Instance().GetKind());
										Instance().SetKind(Instance().GetNextKind());
										Instance().SetProcState(CWMenuMemberBase.WMENU_PROCESS.WMENU_PROCESS_CSELECTTERMINATE);
									}
									if (menu.MenuManager.getSingleton().GetDecideButtonState() == 0)
									{
										if (Instance().GetKind() != CWMenuMemberBase.WMENU_KIND.WMENU_KIND_FORMATION)
										{
											if (strcmp(menu.MenuManager.getSingleton().getFocuseMedget().parentNode()
												._id(), TRANSCODE("char_select")) == 0)
											{
												if (!pl.PlayerParty.instance().player((byte)(sbyte)menu.MenuManager.getSingleton().getFocuseMedget().work()).isEnable())
												{
													menu.MenuManager.getSingleton().playSEBeep();
													return;
												}
												if ((pl.PlayerParty.instance().player((byte)(sbyte)menu.MenuManager.getSingleton().getFocuseMedget().work()).condition()
													.isNotBattleCondition() || pl.PlayerParty.instance().player((byte)(sbyte)menu.MenuManager.getSingleton().getFocuseMedget().work()).condition()
													.isStone()) && Instance().GetKind() == CWMenuMemberBase.WMENU_KIND.WMENU_KIND_JOB)
												{
													menu.MenuManager.getSingleton().playSEBeep();
													return;
												}
												menu.MenuManager.getSingleton().playSEDecide();
												menu.MenuManager.getSingleton().SetTargetCharNo((sbyte)menu.MenuManager.getSingleton().getFocuseMedget().work());
												Instance().GetDummyCursor().SetShow(show: false);
												Instance().SetProcState(CWMenuMemberBase.WMENU_PROCESS.WMENU_PROCESS_CSELECTTERMINATE);
											}
										}
										else if (strcmp(menu.MenuManager.getSingleton().getFocuseMedget().parentNode()
											._id(), TRANSCODE("char_select")) == 0)
										{
											ProcessingFormation();
										}
									}
									menu.MenuManager.getSingleton().ClearBehaviorButton();
								}

								public void CSelectTerminate()
								{
									menu.MenuManager.getSingleton().release();
									Instance().SetProcState(CWMenuMemberBase.WMENU_PROCESS.WMENU_PROCESS_INITIALIZE);
								}

								public void ProcessingFormation()
								{
									GetWMenuFormation().Decide();
								}

								public void SetItemBoxToFindAllItem(int[] pAnyMask, int l, int lNum, int r, int rNum, int h, int a, int g)
								{
									for (int i = 0; i < 384; i++)
									{
										if (l > 0 && l != 1000 && l == pl.PlayerParty.instance().item().normalItem(i)
											.itemId())
										{
											int num = pl.PlayerParty.instance().item().normalItem(i)
												.itemNumber();
											pl.PlayerParty.instance().item().normalItem(i)
												.setItemNumber(num + lNum);
											pAnyMask[0] |= 1;
											l = -99;
										}
										if (r > 0 && r != 1000 && r == pl.PlayerParty.instance().item().normalItem(i)
											.itemId())
										{
											int num2 = pl.PlayerParty.instance().item().normalItem(i)
												.itemNumber();
											pl.PlayerParty.instance().item().normalItem(i)
												.setItemNumber(num2 + rNum);
											pAnyMask[0] |= 2;
											r = -99;
										}
										if (h > 0 && h == pl.PlayerParty.instance().item().normalItem(i)
											.itemId())
										{
											int num3 = pl.PlayerParty.instance().item().normalItem(i)
												.itemNumber();
											pl.PlayerParty.instance().item().normalItem(i)
												.setItemNumber(++num3);
											pAnyMask[0] |= 4;
											h = -99;
										}
										if (a > 0 && a == pl.PlayerParty.instance().item().normalItem(i)
											.itemId())
										{
											int num4 = pl.PlayerParty.instance().item().normalItem(i)
												.itemNumber();
											pl.PlayerParty.instance().item().normalItem(i)
												.setItemNumber(++num4);
											pAnyMask[0] |= 8;
											a = -99;
										}
										if (g > 0 && g == pl.PlayerParty.instance().item().normalItem(i)
											.itemId())
										{
											int num5 = pl.PlayerParty.instance().item().normalItem(i)
												.itemNumber();
											pl.PlayerParty.instance().item().normalItem(i)
												.setItemNumber(++num5);
											pAnyMask[0] |= 16;
											g = -99;
										}
									}
								}

								public void SetItemBoxToNoneAllItem(int[] pAnyMask, int l, int lNum, int r, int rNum, int h, int a, int g)
								{
									for (int i = 0; i < 384; i++)
									{
										if (pl.PlayerParty.instance().item().normalItem(i)
											.itemId() > 0)
										{
											continue;
										}
										if ((pAnyMask[0] & 1) == 0 && l != 1000 && l >= 0)
										{
											pl.PlayerParty.instance().item().normalItem(i)
												.setItemId((short)l);
											int num = pl.PlayerParty.instance().item().normalItem(i)
												.itemNumber();
											pl.PlayerParty.instance().item().normalItem(i)
												.setItemNumber(lNum + num);
											pAnyMask[0] |= 1;
											l = -99;
											if (r != l)
											{
												continue;
											}
										}
										if ((pAnyMask[0] & 2) == 0 && r != 1000 && r >= 0)
										{
											pl.PlayerParty.instance().item().normalItem(i)
												.setItemId((short)r);
											int num2 = pl.PlayerParty.instance().item().normalItem(i)
												.itemNumber();
											pl.PlayerParty.instance().item().normalItem(i)
												.setItemNumber(rNum + num2);
											pAnyMask[0] |= 2;
											r = -99;
										}
										else if ((pAnyMask[0] & 4) == 0 && h >= 0)
										{
											pl.PlayerParty.instance().item().normalItem(i)
												.setItemId((short)h);
											pl.PlayerParty.instance().item().normalItem(i)
												.setItemNumber(1);
											pAnyMask[0] |= 4;
											h = -99;
										}
										else if ((pAnyMask[0] & 8) == 0 && a >= 0)
										{
											pl.PlayerParty.instance().item().normalItem(i)
												.setItemId((short)a);
											pl.PlayerParty.instance().item().normalItem(i)
												.setItemNumber(1);
											pAnyMask[0] |= 8;
											a = -99;
										}
										else if ((pAnyMask[0] & 0x10) == 0 && g >= 0)
										{
											pl.PlayerParty.instance().item().normalItem(i)
												.setItemId((short)g);
											pl.PlayerParty.instance().item().normalItem(i)
												.setItemNumber(1);
											pAnyMask[0] |= 16;
											g = -99;
										}
									}
								}

								public int SetEquipMoreStrength(int flag, int myJob)
								{
									int num = 0;
									int[] pBox = new int[384];
									MORE_STRENGTH mORE_STRENGTH = new MORE_STRENGTH();
									switch (flag)
									{
									case 0:
										CorrectSelectItemType(pBox, ref num, 0, myJob);
										if (num > 0)
										{
											mORE_STRENGTH = SearchMoreStrengthItem(pBox, num, itm.CATEGORY.CATEGORY_WEAPON);
										}
										else
										{
											CorrectSelectItemType(pBox, ref num, 4, myJob);
											mORE_STRENGTH = SearchMoreStrengthItem(pBox, num, itm.CATEGORY.CATEGORY_PROTECTION);
										}
										if (mORE_STRENGTH.more_id == -1)
										{
											return -1;
										}
										break;
									case 1:
										CorrectSelectItemType(pBox, ref num, 1, myJob);
										mORE_STRENGTH = SearchMoreStrengthItem(pBox, num, itm.CATEGORY.CATEGORY_PROTECTION);
										if (mORE_STRENGTH.more_id == -1)
										{
											return -1;
										}
										break;
									case 2:
										CorrectSelectItemType(pBox, ref num, 2, myJob);
										mORE_STRENGTH = SearchMoreStrengthItem(pBox, num, itm.CATEGORY.CATEGORY_PROTECTION);
										if (mORE_STRENGTH.more_id == -1)
										{
											return -1;
										}
										break;
									case 3:
										CorrectSelectItemType(pBox, ref num, 3, myJob);
										mORE_STRENGTH = SearchMoreStrengthItem(pBox, num, itm.CATEGORY.CATEGORY_PROTECTION);
										if (mORE_STRENGTH.more_id == -1)
										{
											return -1;
										}
										break;
									}
									for (int i = 0; i < 384; i++)
									{
										if (mORE_STRENGTH.more_id == pl.PlayerParty.instance().item().normalItem(i)
											.itemId())
										{
											int num2 = pl.PlayerParty.instance().item().normalItem(i)
												.itemNumber();
											num2--;
											pl.PlayerParty.instance().item().normalItem(i)
												.setItemNumber(num2);
											if (num2 == 0)
											{
												pl.PlayerParty.instance().item().normalItem(i)
													.setItemId(-1);
											}
										}
									}
									return mORE_STRENGTH.more_id;
								}

								public void CorrectSelectItemType(int[] pBox, ref int num, int flag, int myJob)
								{
									itm.CATEGORY cATEGORY = itm.CATEGORY.CATEGORY_CONSUMPTION;
									itm.PROTECTION_SYSTEM pROTECTION_SYSTEM = itm.PROTECTION_SYSTEM.PROTECTION_SHIELD;
									switch (flag)
									{
									case 0:
										cATEGORY = itm.CATEGORY.CATEGORY_WEAPON;
										break;
									case 1:
										cATEGORY = itm.CATEGORY.CATEGORY_PROTECTION;
										pROTECTION_SYSTEM = itm.PROTECTION_SYSTEM.PROTECTION_HELMET;
										break;
									case 2:
										cATEGORY = itm.CATEGORY.CATEGORY_PROTECTION;
										pROTECTION_SYSTEM = itm.PROTECTION_SYSTEM.PROTECTION_ARMOR;
										break;
									case 3:
										cATEGORY = itm.CATEGORY.CATEGORY_PROTECTION;
										pROTECTION_SYSTEM = itm.PROTECTION_SYSTEM.PROTECTION_GAUNTLET;
										break;
									case 4:
										cATEGORY = itm.CATEGORY.CATEGORY_PROTECTION;
										pROTECTION_SYSTEM = itm.PROTECTION_SYSTEM.PROTECTION_SHIELD;
										break;
									}
									for (int i = 0; i < 384; i++)
									{
										if (pl.PlayerParty.instance().item().normalItem(i)
											.itemId() <= 0)
										{
											continue;
										}
										switch (cATEGORY)
										{
										case itm.CATEGORY.CATEGORY_WEAPON:
										{
											itm.CATEGORY cATEGORY3 = itm.ItemManager.instance().itemCategory(pl.PlayerParty.instance().item().normalItem(i)
												.itemId());
											if (cATEGORY3 == itm.CATEGORY.CATEGORY_WEAPON)
											{
												int num3 = itm.ItemManager.instance().weaponParameter(pl.PlayerParty.instance().item().normalItem(i)
													.itemId()).equipJob();
												OS_Printf("ジョブの装備可能フラグ  %d \n", num3);
												if ((num3 & (1 << myJob)) != 0)
												{
													pBox[num] = pl.PlayerParty.instance().item().normalItem(i)
														.itemId();
													num++;
												}
											}
											break;
										}
										case itm.CATEGORY.CATEGORY_PROTECTION:
										{
											itm.CATEGORY cATEGORY2 = itm.ItemManager.instance().itemCategory(pl.PlayerParty.instance().item().normalItem(i)
												.itemId());
											if (cATEGORY2 == itm.CATEGORY.CATEGORY_PROTECTION && itm.ItemManager.instance().itemParameter(pl.PlayerParty.instance().item().normalItem(i)
												.itemId()).system() == (byte)pROTECTION_SYSTEM)
											{
												int num2 = itm.ItemManager.instance().protectionParameter(pl.PlayerParty.instance().item().normalItem(i)
													.itemId()).equipJob();
												if ((num2 & (1 << myJob)) != 0)
												{
													pBox[num] = pl.PlayerParty.instance().item().normalItem(i)
														.itemId();
													num++;
												}
											}
											break;
										}
										}
									}
								}

								public MORE_STRENGTH SearchMoreStrengthItem(int[] pBox, int num, itm.CATEGORY cgry)
								{
									MORE_STRENGTH mORE_STRENGTH = new MORE_STRENGTH();
									mORE_STRENGTH.more_id = -1;
									mORE_STRENGTH.more_index = 0;
									int num2 = -1;
									for (int i = 0; i < num; i++)
									{
										if (cgry == itm.CATEGORY.CATEGORY_WEAPON)
										{
											itm.CATEGORY cATEGORY = itm.ItemManager.instance().itemCategory((short)pBox[i]);
											if (cATEGORY == itm.CATEGORY.CATEGORY_WEAPON && num2 <= itm.ItemManager.instance().weaponParameter((short)pBox[i]).aggressivity())
											{
												num2 = itm.ItemManager.instance().weaponParameter((short)pBox[i]).aggressivity();
												mORE_STRENGTH.more_id = pBox[i];
												mORE_STRENGTH.more_index = i;
											}
										}
										else
										{
											itm.CATEGORY cATEGORY2 = itm.ItemManager.instance().itemCategory((short)pBox[i]);
											if (cATEGORY2 == itm.CATEGORY.CATEGORY_PROTECTION && num2 <= itm.ItemManager.instance().protectionParameter((short)pBox[i]).phylacticPower())
											{
												num2 = itm.ItemManager.instance().protectionParameter((short)pBox[i]).phylacticPower();
												mORE_STRENGTH.more_id = pBox[i];
												mORE_STRENGTH.more_index = i;
											}
										}
									}
									return mORE_STRENGTH;
								}

								public void SetUpDummyCursor(int x, int y, bool act)
								{
									dummyCursor.SetPositionI(x, y);
									dummyCursor.SetShow(act);
								}

								public void ResetDummyCursor()
								{
									dummyCursor.SetShow(show: false);
									dummyCursor.SetPositionI(LCD_WIDTH, LCD_HEIGHT);
								}

								public void SetUpFormationCursor(int x, int y, bool act)
								{
									formationCursor.SetPositionI(x, y + 2);
									formationCursor.SetShow(act);
								}

								public void ResetFormationCursor()
								{
									formationCursor.SetShow(show: false);
									formationCursor.SetPositionI(LCD_WIDTH, LCD_HEIGHT);
								}

								public void ChainJoinFocuseList(menu.Medget pCurrent)
								{
									while (pCurrent != null)
									{
										menu.MenuManager.getSingleton().joinFocusList(pCurrent);
										pCurrent = pCurrent.nextSibling();
									}
								}

								public void ChainLeaveFocuseList(menu.Medget pCurrent)
								{
									while (pCurrent != null)
									{
										menu.MenuManager.getSingleton().leaveFocusList(pCurrent);
										pCurrent = pCurrent.nextSibling();
									}
								}

								public void SetActiveCharShow()
								{
									for (int i = 0; i < 4; i++)
									{
										SetShowPcFace(i, pl.PlayerParty.instance().playerForId((byte)i).isEnable());
									}
								}

								public void SetOneCharActiveShow(int currentPlayer)
								{
									for (int i = 0; i < 4; i++)
									{
										if (currentPlayer == i)
										{
											SetShowPcFace(i, show: true);
										}
										else
										{
											SetShowPcFace(i, show: false);
										}
									}
								}

								public void SetCharScrMovement(int x, int y, int currentPlayer)
								{
									if (pl.PlayerParty.instance().player((byte)currentPlayer).isEnable())
									{
										GetPcFace().pcfmSetPosition(pl.PlayerParty.instance().player((byte)currentPlayer).playerId(), (short)x, (short)y, clear: true);
									}
								}

								public void swapCharFirstPosition()
								{
									GetPcFace().pcfmClear();
									GetPcFace().pcfmSetPosition(0, 0);
									for (int i = 0; i < 4; i++)
									{
										pl.Player player = pl.PlayerParty.instance().player((byte)i);
										NNSG2dSVec2 nNSG2dSVec = new NNSG2dSVec2(CharFace_Pos[i]);
										if (player.formationType() == 1)
										{
											nNSG2dSVec.x += 2;
										}
										GetPcFace().pcfmSetPosition(player.playerId(), nNSG2dSVec, clear: false);
										Instance().SetShowPcFace(player.playerId(), player.isEnable());
									}
								}

								public CWMenuManager()
								{
									m_ProcState = CWMenuMemberBase.WMENU_PROCESS.WMENU_PROCESS_INITIALIZE;
									m_Kind = CWMenuMemberBase.WMENU_KIND.WMENU_KIND_ITEM;
									m_Next = CWMenuMemberBase.WMENU_KIND.WMENU_KIND_ITEM;
									m_StartupKind = CWMenuMemberBase.WMENU_KIND.WMENU_KIND_MAIN_MENU;
									for (int i = 0; i < 15; i++)
									{
										m_pCurrent[i] = null;
									}
									RegisterMenuKind();
									LoadCampGroup_ = 0;
								}

								public static CWMenuManager Instance()
								{
									return c_Instance;
								}

								public void SetProcState(CWMenuMemberBase.WMENU_PROCESS stat)
								{
									m_ProcState = stat;
								}

								public CWMenuMemberBase.WMENU_PROCESS GetProcState()
								{
									return m_ProcState;
								}

								public void SetKind(CWMenuMemberBase.WMENU_KIND no)
								{
									m_Kind = no;
								}

								public CWMenuMemberBase.WMENU_KIND GetKind()
								{
									return m_Kind;
								}

								public void SetNextKind(CWMenuMemberBase.WMENU_KIND no)
								{
									m_Next = no;
								}

								public CWMenuMemberBase.WMENU_KIND GetNextKind()
								{
									return m_Next;
								}

								public void SetPrevKind(CWMenuMemberBase.WMENU_KIND no)
								{
									m_Prev = no;
								}

								public CWMenuMemberBase.WMENU_KIND GetPrevKind()
								{
									return m_Prev;
								}

								public void SetMyState(bool val)
								{
									myState = val;
								}

								public bool GetMyState()
								{
									return myState;
								}

								public CWMenuMemberBase pCurrent()
								{
									return m_pCurrent[(int)m_Kind];
								}

								public CWMenuButton GetMenuButton()
								{
									return menuButton;
								}

								public int GetMainMenuMemoryCursor()
								{
									return main_menu_cursor_memory;
								}

								public void SetMainMenuMemoryCursor(int val)
								{
									main_menu_cursor_memory = val;
								}

								public sys2d.Cell GetDummyCursor()
								{
									return dummyCursor;
								}

								public sys2d.Cell GetFormationCursor()
								{
									return formationCursor;
								}

								public void SetShowPcFaceAll(bool show)
								{
									_pcFace.pcfmSetShow(show);
								}

								public void SetShowPcFace(int no, bool show)
								{
									_pcFace.pcfmSetShow((uint)no, show);
								}

								public CWMenuPCFaceManager GetPcFace()
								{
									return _pcFace;
								}

								public void SetPrimaryBGVisibility(bool b)
								{
									primaryBG.bgSetShow(b);
								}

								public void SetSecondlyBGVisibility(bool b)
								{
									secondlyBG.bgSetShow(b);
								}

								public void ClearPrimaryBG()
								{
									primaryBG.bgClearScr();
								}

								public void ClearSecondlyBG()
								{
									secondlyBG.bgClearScr();
								}

								public void SetStartupKind(CWMenuMemberBase.WMENU_KIND Kind)
								{
									m_StartupKind = Kind;
								}

								public CWMenuMain GetWMenuMain()
								{
									return m_MenuMain;
								}

								public CWMenuJob GetWMenuJob()
								{
									return m_MenuJob;
								}

								public CWMenuItem GetWMenuItem()
								{
									return m_MenuItem;
								}

								public CWMenuMagic GetWMenuMagic()
								{
									return m_MenuMagic;
								}

								public CWMenuEquip GetWMenuEquip()
								{
									return m_MenuEquip;
								}

								public CWMenuStatus GetWMenuStatus()
								{
									return m_MenuStatus;
								}

								public CWMenuConfig GetWMenuConfig()
								{
									return m_MenuConfig;
								}

								public CWMenuSuspend GetWMenuSuspend()
								{
									return m_MenuSuspend;
								}

								public CWMenuSave GetWMenuSave()
								{
									return m_MenuSave;
								}

								public CWMenuFormation GetWMenuFormation()
								{
									return m_MenuFormation;
								}
							}
	}
}
