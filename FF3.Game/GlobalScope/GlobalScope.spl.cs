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
						public static class spl
						{
							public class SpecialPart : sys.FF3GamePart
							{
								public static SpecialPart instance_ = new SpecialPart();

								private int partID_;

								public static void registerPart()
								{
									sys.GGlobal.registerPart(GAMEPART.GAMEPART_SPECIAL, instance_);
									instance_.setPartID(0);
								}

								public SpecialPart()
								{
									partID_ = 0;
								}

								~SpecialPart()
								{
								}

								public void setPartID(int i)
								{
									partID_ = i;
								}

								protected override void doInitialize()
								{
									TexDivideLoader.getSingleton().tdlCancel();
									mon.MonsterManager.instance();
									ovl.overlayRegister.ChangeOverlay(ovl.OVERLAYINDEX.PART_SPECIAL);
									GX_SetDispSelect(GXDispSelect.GX_DISP_SELECT_MAIN_SUB);
									ds.CDevice.setup();
									ds.CDevice.setup_main();
									ds.CDevice.setup_sub();
									GX_SetBGCharOffset(0);
									GX_SetBGScrOffset(0);
									ds.CVram.clear();
									GXVRamTex gXVRamTex = GXVRamTex.GX_VRAM_TEX_012_ABD;
									GXVRamTexPltt gXVRamTexPltt = GXVRamTexPltt.GX_VRAM_TEXPLTT_01_FG;
									GX_SetBankForTex(gXVRamTex);
									GX_SetBankForTexPltt(gXVRamTexPltt);
									ds.CVram.getInstance().setBankForTex(gXVRamTex);
									ds.CVram.getInstance().setBankForPltt(gXVRamTexPltt);
									GX_SetBankForBG(GXVRamBG.GX_VRAM_BG_64_E);
									GX_SetBankForBGExtPltt(GXVRamBGExtPltt.GX_VRAM_BGEXTPLTT_NONE);
									GX_SetBankForOBJ(GXVRamOBJ.GX_VRAM_OBJ_NONE);
									GX_SetBankForOBJExtPltt(GXVRamOBJExtPltt.GX_VRAM_OBJEXTPLTT_NONE);
									GX_SetBankForSubBG(GXVRamSubBG.GX_VRAM_SUB_BG_128_C);
									GX_SetBankForSubBGExtPltt(GXVRamSubBGExtPltt.GX_VRAM_SUB_BGEXTPLTT_0123_H);
									GX_SetBankForSubOBJ(GXVRamSubOBJ.GX_VRAM_SUB_OBJ_16_I);
									GX_SetBankForSubOBJExtPltt(GXVRamSubOBJExtPltt.GX_VRAM_SUB_OBJEXTPLTT_NONE);
									GX_SetGraphicsMode(GXDispMode.GX_DISPMODE_GRAPHICS, GXBGMode.GX_BGMODE_0, 0);
									GXS_SetGraphicsMode(GXBGMode.GX_BGMODE_0);
									ds.CVram.setMainPlaneVisiblity(bg0: true, bg1: false, bg2: false, bg3: false, obj: false);
									ds.CVram.setSubPlaneVisiblity(bg0: true, bg1: true, bg2: false, bg3: false, obj: true);
									ds.CVram.setMainBGPriority(3, 2, 1, 0);
									ds.CVram.setSubBGPriority(1, 0, 2, 3);
									ds.CVram.getInstance().setupTexVramMng(393216u, 32768u, 64u, 0);
									ds.CVram.getInstance().setupPlttVramMng(32768u, 64u, 0);
									G3X_SetClearColor(GX_RGB(0, 0, 0), 31, 32767, 1, 0);
									G2S_SetBG0Control(0, 0, GXBGScrBase.GX_BG_SCRBASE_0x0000, GXBGCharBase.GX_BG_CHARBASE_0x04000, 0);
									G2S_SetBG1Control(0, 0, GXBGScrBase.GX_BG_SCRBASE_0x0800, GXBGCharBase.GX_BG_CHARBASE_0x0c000, 0);
									sys2d.DS2DManager.d2dGetInstance().d2dInitialize();
									dgs.msg.CMessageSys.getInstance().initialize();
									dgs.msg.CMessageSys.getInstance().Sub().assignBG(1, 0, 0, 32, 24);
									changeGlobalDirectory();
									menu.MenuManager.getSingleton().Set2d3dMode(2);
									menu.MenuManager.getSingleton().initialize();
									changeCompanyDirectory();
									menu.MenuManager.getSingleton().LoadXbnFile("SpecialDefine.xbn");
									menu.MenuManager.getSingleton().CreateNeedObject(menu.MenuManager.MENU_CREATE_PARAMETER.CREATE_MENU_MODE_2D);
									menu.MenuManager.getSingleton().SetUsingMenuType(1);
									changeCompanyDirectory();
									menu.MenuManager.getSingleton().CreateItemDataText();
									menu.MenuManager.getSingleton().CreateSpecialDataText();
									SCManager.getSingleton().initialize();
									dgs.CCurtain.initialize();
									dgs.CCurtain.Bottom().setEnable(enable: true);
									dgs.CCurtain.Bottom().setColor(0, GX_RGB(0, 0, 0));
									dgs.CCurtain.Bottom().setAlpha(0, 31);
									_hEnd = false;
									partEnd = false;
									GX_DispOn();
									GXS_DispOn();
									dgs.CFade.Main().fadeIn(15);
									dgs.CFade.Sub().fadeIn(15);
								}

								protected override void doUninitialize()
								{
									menu.MenuManager.getSingleton().ReleaseItemDataText();
									menu.MenuManager.getSingleton().ReleaseSpecialDataText();
									SCManager.getSingleton().terminate();
									menu.MenuManager.getSingleton().ReleaseXbnFile();
									menu.MenuManager.getSingleton().release();
									menu.MenuManager.getSingleton().terminate();
									menu.MenuManager.getSingleton().ResetWindowSystem();
									menu.MenuManager.getSingleton().Set2d3dMode(3);
									dgs.msg.CMessageSys.getInstance().terminate();
									dgs.CCurtain.Bottom().terminate();
									GX_SetGraphicsMode(GXDispMode.GX_DISPMODE_GRAPHICS, GXBGMode.GX_BGMODE_0, 0);
									ds.CVram.getInstance().releaseTexVramMng();
									ds.CVram.getInstance().releasePlttVramMng();
									sys.GGlobal.setNextPart(sys.GGlobal.getPreviousPart());
								}

								protected override void doSleep()
								{
								}

								protected override void doWakeUp()
								{
								}

								protected override void onDrawPart()
								{
									dgs.CCurtain.Bottom().draw();
									dgs.msg.CMessageSys.getInstance().draw();
								}

								protected override void onExecutePart()
								{
									menu.MenuManager.getSingleton().execute();
									dgs.CCurtain.execute();
									sys2d.DS2DManager.d2dGetInstance().d2dExecute();
									SCManager.getSingleton().execute();
									if (!SCManager.getSingleton().GetMyState())
									{
										abort();
									}
								}

								protected override void onDrawEffector()
								{
									sys2d.DS2DManager.d2dGetInstance().d2dDraw();
									sys2d.DS2DManager.d2dGetInstance().d2dDrawScreen(depthtest: false);
								}

								protected override void onUpdatePart()
								{
									sys2d.DS2DManager.d2dGetInstance().d2dUpdate();
								}

								public bool doKeyProcess(bool __hEnd)
								{
									return true;
								}

								public static SpecialPart getInstance()
								{
									return instance_;
								}
							}

							public class SpecialContentsBase
							{
								public enum SP_CONTENTS_KIND
								{
									CONTENTS_MONSTER_BOOK,
									CONTENTS_THOROUGH,
									CONTENTS_MAX
								}

								public enum SP_CONTENTS_PROCESS
								{
									CONTENTS_INITIALIZE,
									CONTENTS_EXECUTE,
									CONTENTS_TERMINATE,
									CONTENTS_EP,
									CONTENTS_END,
									CONTENTS_HALT
								}

								public class SPECIAL_MSG_DATA
								{
									public bool bEnable;

									public dgs.DGSMessage pMsg;
								}

								public const SP_CONTENTS_KIND CONTENTS_MONSTER_BOOK = SP_CONTENTS_KIND.CONTENTS_MONSTER_BOOK;

								public const SP_CONTENTS_KIND CONTENTS_THOROUGH = SP_CONTENTS_KIND.CONTENTS_THOROUGH;

								public const SP_CONTENTS_KIND CONTENTS_MAX = SP_CONTENTS_KIND.CONTENTS_MAX;

								public const SP_CONTENTS_PROCESS CONTENTS_INITIALIZE = SP_CONTENTS_PROCESS.CONTENTS_INITIALIZE;

								public const SP_CONTENTS_PROCESS CONTENTS_EXECUTE = SP_CONTENTS_PROCESS.CONTENTS_EXECUTE;

								public const SP_CONTENTS_PROCESS CONTENTS_TERMINATE = SP_CONTENTS_PROCESS.CONTENTS_TERMINATE;

								public const SP_CONTENTS_PROCESS CONTENTS_EP = SP_CONTENTS_PROCESS.CONTENTS_EP;

								public const SP_CONTENTS_PROCESS CONTENTS_END = SP_CONTENTS_PROCESS.CONTENTS_END;

								public const SP_CONTENTS_PROCESS CONTENTS_HALT = SP_CONTENTS_PROCESS.CONTENTS_HALT;

								public static bool isEnd;

								public int SearchUseMessageNo(SPECIAL_MSG_DATA[] pT, int indexMax)
								{
									for (int i = 0; i < indexMax; i++)
									{
										if (!pT[i].bEnable)
										{
											pT[i].bEnable = true;
											pT[i].pMsg = null;
											return i;
										}
									}
									return -1;
								}

								public void ClearUseMessageNo(SPECIAL_MSG_DATA[] pT, int indexMax)
								{
									for (int i = 0; i < indexMax; i++)
									{
										if (pT[i].bEnable)
										{
											if (pT[i].pMsg != null)
											{
												pT[i].pMsg.release();
												pT[i].pMsg = null;
											}
											pT[i].bEnable = false;
										}
									}
								}

								public void ClearUseMessageNo(SPECIAL_MSG_DATA[] pT, int indexMax, int indexNo)
								{
									if (0 <= indexNo && indexMax >= indexNo && pT[indexNo].pMsg != null && pT[indexNo].bEnable)
									{
										pT[indexNo].pMsg.release();
										pT[indexNo].pMsg = null;
										pT[indexNo].bEnable = false;
									}
								}

								public void createSpecialMsg(SPECIAL_MSG_DATA[] pMessage, int indexNo, string pStr, ds.Vector2<short> pos)
								{
									ds.Vector2<short> vector = new ds.Vector2<short>(0, 0);
									dgs.DGSMessageManager dGSMessageManager = dgs.msg.CMessageSys.getInstance().Sub();
									pMessage[indexNo].pMsg = dGSMessageManager.createMessage(pStr, 1);
									if (pMessage[indexNo].pMsg != null)
									{
										pMessage[indexNo].pMsg.getTextSize(vector);
										pMessage[indexNo].pMsg.setPosition((short)(pos.vx - vector.vx), pos.vy, erase: true);
										pMessage[indexNo].pMsg.setDisplaySpeed(byte.MaxValue);
										pMessage[indexNo].pMsg.setDisplayWait(0);
									}
								}

								public void createSpecialMsg(SPECIAL_MSG_DATA[] pMessage, int indexNo, int msgID, ds.Vector2<short> pos)
								{
									ds.Vector2<short> vector = new ds.Vector2<short>(0, 0);
									dgs.DGSMessageManager dGSMessageManager = dgs.msg.CMessageSys.getInstance().Sub();
									pMessage[indexNo].pMsg = dGSMessageManager.createMessage((uint)msgID, dgs.INVALID_MSDHANDLE, 1);
									if (pMessage[indexNo].pMsg != null)
									{
										pMessage[indexNo].pMsg.getTextSize(vector);
										pMessage[indexNo].pMsg.setPosition((short)(pos.vx - vector.vx), pos.vy, erase: true);
										pMessage[indexNo].pMsg.setDisplaySpeed(byte.MaxValue);
										pMessage[indexNo].pMsg.setDisplayWait(0);
									}
								}

								public virtual void initialize()
								{
								}

								public virtual void execute()
								{
								}

								public virtual void terminate()
								{
								}

								public virtual void bmRefresh()
								{
								}

								public virtual void finish()
								{
								}
							}

							public class MonsterBook : SpecialContentsBase
							{
								public enum MBOOK_WND
								{
									MBOOK_WND_BASE,
									MBOOK_WND_MAX
								}

								public enum MBOOK_STATE
								{
									STATE_TOP,
									STATE_SELECT,
									STATE_FADEOUT,
									STATE_CHANGE,
									STATE_LOAD,
									STATE_FADEIN
								}

								public const MBOOK_STATE STATE_TOP = MBOOK_STATE.STATE_TOP;

								public const MBOOK_STATE STATE_SELECT = MBOOK_STATE.STATE_SELECT;

								public const MBOOK_STATE STATE_FADEOUT = MBOOK_STATE.STATE_FADEOUT;

								public const MBOOK_STATE STATE_CHANGE = MBOOK_STATE.STATE_CHANGE;

								public const MBOOK_STATE STATE_LOAD = MBOOK_STATE.STATE_LOAD;

								public const MBOOK_STATE STATE_FADEIN = MBOOK_STATE.STATE_FADEIN;

								private int myState;

								private int weekCount;

								private int[] weekPoint = new int[WeekPointMax];

								private SPECIAL_MSG_DATA[] mBookMsg = new SPECIAL_MSG_DATA[MBookMsgMax];

								private sys2d.Bg BgMenu_ = new sys2d.Bg();

								private int targetItemNo_;

								private ds.sys3d.Scene scene_ = new ds.sys3d.Scene();

								private ds.sys3d.CCamera camera_ = new ds.sys3d.CCamera();

								private int ctrl_;

								private string motname_;

								private int page_;

								private int nextPage_;

								public override void initialize()
								{
									page_ = (nextPage_ = 0);
									ctrl_ = -1;
									myState = 1;
									motname_ = "";
									scene_.initialize();
									characterMng.initialize(scene_, null);
									stageMng.initialize(scene_);
									camera_.initialize();
									camera_.setPosition(CameraBattlePosition);
									camera_.setTarget(CameraBattleTarget);
									camera_.setCamUp(0, 4096, 0);
									camera_.setAngle(CameraBattleAngle.x, CameraBattleAngle.y, CameraBattleAngle.z);
									camera_.setDistance(CameraBattleDistance);
									camera_.setClip(40960, 12288000);
									camera_.setFOV(852, 4006);
									camera_.setMoveMode(1);
									scene_.setCamera(camera_);
									if (SCManager.getSingleton().GetPrevKind() != SP_CONTENTS_KIND.CONTENTS_MONSTER_BOOK)
									{
										menu.MenuManager.getSingleton().buildMenu("monster_book");
										menu.MenuManager.getSingleton().GetCursor2d().SetShow(show: false);
										menu.MenuManager.getSingleton().SetTargetItemNo(1);
										menu.MenuManager.getSingleton().initFocus(0);
										changeGlobalDirectory();
										BgMenu_.bgLoad("monsterbook.NSCR", "menu_bg_01.NCGR", "new_menu_bg.NCLR");
										BgMenu_.bgSetUp(NNSG2dBGSelect.NNS_G2D_BGSELECT_SUB0);
										BgMenu_.bgRelease();
									}
									enableList();
									if (SCManager.getSingleton().GetPrevKind() != SP_CONTENTS_KIND.CONTENTS_MONSTER_BOOK)
									{
										menu.Medget nodeByID = menu.MenuManager.getSingleton().root().getNodeByID("one");
										if (nodeByID != null)
										{
											menu.MenuManager.getSingleton().setFocuseMedget(nodeByID);
										}
									}
									menu.MenuManager.getSingleton().GetCursor2d().SetShow(show: true);
									menu.MenuManager.getSingleton().SetScrollType(menu.MenuManager.SCROLL_TYPE.TYPE_WAIT);
									menu.MenuManager.getSingleton().SetImposibleScrollFlag(val: false);
									menu.MenuManager.getSingleton().SetDecideButtonState(1);
									drawMonsterRate();
									SCManager.getSingleton().SetProcState(SP_CONTENTS_PROCESS.CONTENTS_EXECUTE);
								}

								public override void execute()
								{
									characterMng.execute();
									stageMng.execute();
									camera_.execute();
									scene_.draw(bVBlank: true);
									if (myState == 0)
									{
										executeTop();
									}
									else if (1 == myState)
									{
										executeSelect();
									}
									else if (2 == myState)
									{
										executeFadeout();
									}
									else if (3 == myState)
									{
										executeChange();
									}
									else if (4 == myState)
									{
										executeLoad();
									}
									else if (5 == myState)
									{
										executeFadein();
									}
									menu.MenuManager.getSingleton().ClearBehaviorButton();
								}

								public void executeTop()
								{
									SCManager.getSingleton().GetSCTab().Executioner();
								}

								public void executeSelect()
								{
									if (page_ == 1)
									{
										if ((ds.g_Pad.edge() & 2) != 0 || SCManager.getSingleton().BottonIcon().TouchButtonB())
										{
											dgs.CFade.Main().fadeOut(10, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
											myState = 2;
											nextPage_ = 0;
											menu.MenuManager.getSingleton().playSECancel();
										}
										else if ((ds.g_Pad.edge() & 0x200) != 0 || SCManager.getSingleton().BottonIcon().TouchButtonL() || (ds.g_Pad.edge() & 0x100) != 0 || SCManager.getSingleton().BottonIcon().TouchButtonR())
										{
											int num = (((ds.g_Pad.edge() & 0x200) == 0 && !SCManager.getSingleton().BottonIcon().TouchButtonL()) ? 1 : (-1));
											int num2 = menu.MenuManager.getSingleton().GetTargetItemNo();
											do
											{
												num2 = (num2 + num + 256) % 256;
											}
											while (!menu.MBMonsterList.isMobItemVisible(num2) || isMobItemEnable(num2) == 0);
											menu.MenuManager.getSingleton().SetTargetItemNo(num2);
											mobEntryFinish(menu.MenuManager.getSingleton().GetTargetItemNo());
											dgs.CFade.Main().fadeOut(10, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
											myState = 2;
											nextPage_ = 1;
											menu.MenuManager.getSingleton().playSEMoveCursor();
										}
									}
									else if (SCManager.getSingleton().GetSCTab().TouchTopTabIconArea(1))
									{
										SCManager.getSingleton().GetSCTab().SetTopCursorPos(1);
										SCManager.getSingleton().SetNextKind(SP_CONTENTS_KIND.CONTENTS_THOROUGH);
										SCManager.getSingleton().SetProcState(SP_CONTENTS_PROCESS.CONTENTS_TERMINATE);
										menu.MenuManager.getSingleton().playSEMoveCursor();
									}
									else if ((ds.g_Pad.edge() & 2) != 0 || SCManager.getSingleton().BottonIcon().TouchButtonB())
									{
										SCManager.getSingleton().SetProcState(SP_CONTENTS_PROCESS.CONTENTS_EP);
										menu.MenuManager.getSingleton().playSECancel();
									}
									else if (menu.MenuManager.getSingleton().GetDecideButtonState() == 0)
									{
										if (isMobItemEnable(menu.MenuManager.getSingleton().GetTargetItemNo()) != 0)
										{
											mobEntryFinish(menu.MenuManager.getSingleton().GetTargetItemNo());
											dgs.CFade.Main().fadeOut(10, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
											myState = 2;
											nextPage_ = 1;
											disableList(b: false);
											menu.MenuManager.getSingleton().playSEDecide();
										}
										else
										{
											menu.MenuManager.getSingleton().playSEBeep();
										}
									}
								}

								public void executeFadeout()
								{
									if (dgs.CFade.Main().isFaded())
									{
										myState = 3;
									}
								}

								public void executeChange()
								{
									releaseModelResource();
									modelRefresh();
									stageRefresh();
									menu.Medget nodeByID = menu.MenuManager.getSingleton().root().getNodeByID("list");
									if (nodeByID != null)
									{
										((menu.MBMonsterList)nodeByID.behavior().queryInterface(menu.MBMonsterList.classIdentifier()))?.setRefresh(b: true);
									}
									myState = 4;
								}

								public void executeLoad()
								{
									if (!characterMng.isLoadedOrgTex(ctrl_))
									{
										return;
									}
									page_ = nextPage_;
									if (page_ == 1)
									{
										menu.MenuManager.getSingleton().GetCursor2d().SetShow(show: false);
										changeGlobalDirectory();
										BgMenu_.bgLoad("monsterbook_2.NSCR", "menu_bg_01.NCGR", "new_menu_bg.NCLR");
										BgMenu_.bgSetUp(NNSG2dBGSelect.NNS_G2D_BGSELECT_SUB0);
										BgMenu_.bgRelease();
										if (198 == menu.MenuManager.getSingleton().GetTargetItemNo())
										{
											bmRefreshDisableCondition();
										}
										else if (isMobItemEnable(TRANSMOBID(menu.MenuManager.getSingleton().GetTargetItemNo())) != 0)
										{
											bmRefresh();
										}
										else
										{
											bmRefreshDisableCondition();
										}
										targetItemNo_ = menu.MenuManager.getSingleton().GetTargetItemNo();
										SCManager.getSingleton().GetSCTab().DeleteTabData();
										SCManager.getSingleton().BottonIcon().SetUpNormalVer();
										GX_Power3D(1);
									}
									else
									{
										menu.MenuManager.getSingleton().release();
										menu.MenuManager.getSingleton().buildMenu("monster_book");
										menu.MenuManager.getSingleton().GetCursor2d().SetShow(show: true);
										menu.MenuManager.getSingleton().initFocus(0);
										changeGlobalDirectory();
										BgMenu_.bgLoad("monsterbook.NSCR", "menu_bg_01.NCGR", "new_menu_bg.NCLR");
										BgMenu_.bgSetUp(NNSG2dBGSelect.NNS_G2D_BGSELECT_SUB0);
										BgMenu_.bgRelease();
										drawMonsterRate();
										int i = 0;
										int num = 0;
										for (; i < 256; i++)
										{
											if (!menu.MBMonsterList.isMobItemVisible(i))
											{
												continue;
											}
											if (i == targetItemNo_)
											{
												menu.Medget nodeByID = menu.MenuManager.getSingleton().root().getNodeByID(TRANSCODE("list"));
												if (nodeByID != null && nodeByID.behavior() != null)
												{
													((menu.MBMonsterList)nodeByID.behavior().queryInterface(menu.MBMonsterList.classIdentifier()))?.setCursor(nodeByID, num);
												}
											}
											num++;
										}
										SCManager.getSingleton().GetSCTab().SetTabData();
										SCManager.getSingleton().GetSCTab().GetTopCursor()
											.SetAnimation(anm: false);
										SCManager.getSingleton().BottonIcon().SetUpSpecialVer();
										GX_Power3D(0);
									}
									dgs.CFade.Main().fadeIn(10);
									myState = 5;
								}

								public void executeFadein()
								{
									if (dgs.CFade.Main().isCleared())
									{
										myState = 1;
										enableList();
									}
								}

								public override void terminate()
								{
									if (SCManager.getSingleton().GetNextKind() != SP_CONTENTS_KIND.CONTENTS_MONSTER_BOOK)
									{
										menu.MenuManager.getSingleton().release();
										clearDisplay();
									}
								}

								public void mobookSetTextVisibility(string strID, bool bVisibility)
								{
									if (menu.MenuManager.getSingleton().root() != null)
									{
										menu.Medget nodeByID = menu.MenuManager.getSingleton().root().getNodeByID(TRANSCODE(strID));
										if (nodeByID != null && nodeByID.behavior() != null)
										{
											((menu.MBText)nodeByID.behavior().queryInterface(menu.MBText.classIdentifier()))?.bmTextVisibility(bVisibility);
										}
									}
								}

								public static int getMonsterRate()
								{
									int num = 0;
									int num2 = 0;
									for (int i = 0; i < 256; i++)
									{
										if (menu.MBMonsterList.isMobItemVisible(i))
										{
											if (isMobItemEnable(i) != 0)
											{
												num2++;
											}
											num++;
										}
									}
									return num2 * 100 / num;
								}

								public static void setMonsterRate(int rate)
								{
									int num = 0;
									int num2 = 0;
									if (mon.MonsterManager.instance().monsterManiaManager().monsterManiaForMonsterID(1)
										.monsterId() == 0)
									{
										mon.MonsterManager.instance().load();
									}
									for (int i = 0; i < 256; i++)
									{
										if (menu.MBMonsterList.isMobItemVisible(i))
										{
											if (isMobItemEnable(i) != 0)
											{
												num2++;
											}
											num++;
										}
									}
									for (int j = 0; j < 256; j++)
									{
										if (!menu.MBMonsterList.isMobItemVisible(j))
										{
											continue;
										}
										if (isMobItemEnable(j) != 0)
										{
											if (num2 * 100 / num > rate)
											{
												mon.MonsterManager.instance().monsterManiaManager().monsterManiaForMonsterID(j)
													.deadCount()
													.set(0);
											}
											if (isMobItemEnable(j) == 0)
											{
												num2--;
											}
										}
										else
										{
											if (num2 * 100 / num < rate)
											{
												mon.MonsterManager.instance().monsterManiaManager().monsterManiaForMonsterID(j)
													.deadCount()
													.set(1);
											}
											if (isMobItemEnable(j) != 0)
											{
												num2++;
											}
										}
									}
								}

								public void drawMonsterRate()
								{
									sprintf(out var arg, "%d%%", getMonsterRate());
									menu.Medget nodeByID = menu.MenuManager.getSingleton().root().getNodeByID(TRANSCODE("rate"));
									if (nodeByID != null && nodeByID.behavior() != null)
									{
										((menu.MBText)nodeByID.behavior().queryInterface(menu.MBText.classIdentifier()))?.mbSetBufferMsg(arg, decWidth: false);
									}
								}

								public override void bmRefresh()
								{
									menu.MenuManager.getSingleton().release();
									menu.MenuManager.getSingleton().buildMenu("monster_status_enable");
									menu.Medget medget = menu.MenuManager.getSingleton().root();
									int num = 0;
									int targetItemNo = menu.MenuManager.getSingleton().GetTargetItemNo();
									menu.Medget nodeByID = medget.getNodeByID(TRANSCODE("enemy_name"));
									if (nodeByID != null && nodeByID.behavior() != null)
									{
										((menu.MBText)nodeByID.behavior().queryInterface(menu.MBText.classIdentifier()))?.mbSetTextMsgNo(mon.MonsterManager.instance().monsterParameter(targetItemNo).nameId());
									}
									menu.Medget nodeByID2 = medget.getNodeByID(TRANSCODE("enemy_life_value"));
									if (nodeByID2 != null && nodeByID2.behavior() != null)
									{
										menu.MBText mBText = (menu.MBText)nodeByID2.behavior().queryInterface(menu.MBText.classIdentifier());
										if (mBText != null)
										{
											string arg = "";
											sprintf(out arg, "%8d", mobookGetLife(TRANSMOBID(targetItemNo)));
											mBText.mbSetBufferMsg(arg, decWidth: false);
										}
									}
									menu.Medget nodeByID3 = medget.getNodeByID(TRANSCODE("enemy_lv_value"));
									if (nodeByID3 != null && nodeByID3.behavior() != null)
									{
										((menu.MBText)nodeByID3.behavior().queryInterface(menu.MBText.classIdentifier()))?.mbSetBufferNumber(mobookGetLv(TRANSMOBID(targetItemNo)));
									}
									menu.Medget nodeByID4 = medget.getNodeByID(TRANSCODE("enemy_money_value"));
									if (nodeByID4 != null && nodeByID4.behavior() != null)
									{
										menu.MBText mBText2 = (menu.MBText)nodeByID4.behavior().queryInterface(menu.MBText.classIdentifier());
										if (mBText2 != null)
										{
											string arg2 = "";
											sprintf(out arg2, "%8d", mobookGetMoney(TRANSMOBID(targetItemNo)));
											mBText2.mbSetBufferMsg(arg2, decWidth: false);
										}
									}
									menu.Medget nodeByID5 = medget.getNodeByID(TRANSCODE("enemy_attack_value"));
									if (nodeByID5 != null && nodeByID5.behavior() != null)
									{
										((menu.MBText)nodeByID5.behavior().queryInterface(menu.MBText.classIdentifier()))?.mbSetBufferNumber(mobookGetAttack(TRANSMOBID(targetItemNo)));
									}
									menu.Medget nodeByID6 = medget.getNodeByID(TRANSCODE("enemy_exp_value"));
									if (nodeByID6 != null && nodeByID6.behavior() != null)
									{
										menu.MBText mBText3 = (menu.MBText)nodeByID6.behavior().queryInterface(menu.MBText.classIdentifier());
										if (mBText3 != null)
										{
											string arg3 = "";
											sprintf(out arg3, "%8d", mobookGetExp(TRANSMOBID(targetItemNo)));
											mBText3.mbSetBufferMsg(arg3, decWidth: false);
										}
									}
									menu.Medget nodeByID7 = medget.getNodeByID(TRANSCODE("enemy_def_value"));
									if (nodeByID7 != null && nodeByID7.behavior() != null)
									{
										((menu.MBText)nodeByID7.behavior().queryInterface(menu.MBText.classIdentifier()))?.mbSetBufferNumber(mobookGetDef(TRANSMOBID(targetItemNo)));
									}
									SearchMonsterWeekPointToSeiton();
									menu.Medget medget2 = null;
									menu.MBText mBText4 = null;
									if ((medget2 = medget.getNodeByID(TRANSCODE("enemy_weak_point"))) == null)
									{
										return;
									}
									medget2 = medget2.childNode();
									int num2 = 0;
									for (int i = 0; i < WeekPointMax; i++)
									{
										if (-1 != weekPoint[i])
										{
											num2++;
										}
									}
									if (0 < num2)
									{
										for (int j = 0; j < WeekPointMax; j++)
										{
											mBText4 = (menu.MBText)medget2.behavior().queryInterface(menu.MBText.classIdentifier());
											if (mBText4 != null)
											{
												num = weekPoint[j];
												if (num != -1)
												{
													mBText4.mbSetTextMsgNo(num);
												}
												else
												{
													mBText4.mbSetBufferMsg("", decWidth: false);
												}
											}
											menu.Medget medget3 = medget2.nextSibling();
											if (medget3 != null)
											{
												medget2 = medget3;
											}
										}
									}
									else
									{
										((menu.MBText)medget2.behavior().queryInterface(menu.MBText.classIdentifier()))?.mbSetTextMsgNo(60125);
									}
								}

								public void bmRefreshDisableCondition()
								{
									menu.MenuManager.getSingleton().release();
									menu.MenuManager.getSingleton().buildMenu("monster_status_disable");
									menu.Medget medget = menu.MenuManager.getSingleton().root();
									int targetItemNo = menu.MenuManager.getSingleton().GetTargetItemNo();
									menu.Medget nodeByID = medget.getNodeByID(TRANSCODE("enemy_name"));
									if (nodeByID != null && nodeByID.behavior() != null)
									{
										((menu.MBText)nodeByID.behavior().queryInterface(menu.MBText.classIdentifier()))?.mbSetTextMsgNo(mon.MonsterManager.instance().monsterParameter(targetItemNo).nameId());
									}
								}

								public override void finish()
								{
									menu.MenuManager.getSingleton().release();
									BgMenu_.bgRelease();
									if (-1 != ctrl_)
									{
										characterMng.delCharacter(ctrl_);
										characterMng.removeMotion(ctrl_, motname_);
										motname_ = "";
										ctrl_ = -1;
									}
									characterMng.terminate();
									stageMng.delStage();
									stageMng.terminate();
								}

								public void releaseModelResource()
								{
									if (-1 != ctrl_)
									{
										characterMng.delCharacter(ctrl_);
										motname_ = "";
										ctrl_ = -1;
									}
									stageMng.delStage();
								}

								public void modelRefresh()
								{
									string mobModelName = getMobModelName(TRANSMOBID(menu.MenuManager.getSingleton().GetTargetItemNo()));
									string mobTextureName = getMobTextureName(TRANSMOBID(menu.MenuManager.getSingleton().GetTargetItemNo()));
									string mobMotionName = getMobMotionName(TRANSMOBID(menu.MenuManager.getSingleton().GetTargetItemNo()));
									ctrl_ = characterMng.setCharacter(mobModelName, CCharacterMng.PRI_SCENE.PRI_SCENE_FIRST);
									if (-1 != ctrl_)
									{
										characterMng.addMotion(ctrl_, mobMotionName);
										characterMng.startMotion(ctrl_, 101, fLoop: true, 0u);
										string arg = "";
										sprintf(out arg, "/OBJ/MONSTER/%s.ntxp.lz", mobTextureName);
										if (ds.g_File.getSize(arg) != 0)
										{
											characterMng.bindReplaceTex(ctrl_, mobTextureName);
										}
										strcpy(out motname_, mobMotionName);
									}
									VecFx32 vecFx = mon.MonsterManager.instance().offset(TRANSMOBID(menu.MenuManager.getSingleton().GetTargetItemNo())).initializePosition();
									VecFx32 vecFx2 = new VecFx32();
									if (vecFx.x == 0 && vecFx.y == 0 && vecFx.z == 0)
									{
										vecFx2.x = btl.MonsterPosition[0].x;
										vecFx2.y = btl.MonsterPosition[0].y;
										vecFx2.z = btl.MonsterPosition[0].z;
										characterMng.setPosition(ctrl_, vecFx2);
									}
									else
									{
										vecFx.y += 4096 * mon.MonsterManager.instance().offset(TRANSMOBID(menu.MenuManager.getSingleton().GetTargetItemNo())).height();
										characterMng.setPosition(ctrl_, vecFx);
									}
									int i = mon.MonsterManager.instance().offset(TRANSMOBID(menu.MenuManager.getSingleton().GetTargetItemNo())).rotate();
									characterMng.setRotation(ctrl_, 0, ds.DEGto65536(i), 0);
									VecFx32 vecFx3 = new VecFx32(0, 0, 0);
									vecFx3.x = mon.MonsterManager.instance().offset(TRANSMOBID(menu.MenuManager.getSingleton().GetTargetItemNo())).scale();
									vecFx3.y = mon.MonsterManager.instance().offset(TRANSMOBID(menu.MenuManager.getSingleton().GetTargetItemNo())).scale();
									vecFx3.z = mon.MonsterManager.instance().offset(TRANSMOBID(menu.MenuManager.getSingleton().GetTargetItemNo())).scale();
									characterMng.setScale(ctrl_, vecFx3);
									vecFx3.x = mon.MonsterManager.instance().offset(TRANSMOBID(menu.MenuManager.getSingleton().GetTargetItemNo())).shadowX();
									vecFx3.y = 4096;
									vecFx3.z = mon.MonsterManager.instance().offset(TRANSMOBID(menu.MenuManager.getSingleton().GetTargetItemNo())).shadowZ();
									characterMng.setShadowScale(ctrl_, vecFx3);
									characterMng.setShadowHeight(ctrl_, 2048);
								}

								public void stageRefresh()
								{
									stageMng.setStage(getMobMapName(TRANSMOBID(menu.MenuManager.getSingleton().GetTargetItemNo())));
								}

								public void SearchMonsterWeekPointToSeiton()
								{
									int targetItemNo = menu.MenuManager.getSingleton().GetTargetItemNo();
									mon.MonsterParameter monsterParameter = mon.MonsterManager.instance().monsterParameter(TRANSMOBID(targetItemNo));
									int pattern = monsterParameter.magicDefense().weakType();
									for (int i = 0; i < WeekPointMax; i++)
									{
										weekPoint[i] = -1;
									}
									weekCount = 0;
									AddWeekPoint(pattern, 8);
									AddWeekPoint(pattern, 32);
									AddWeekPoint(pattern, 16);
									AddWeekPoint(pattern, 64);
									AddWeekPoint(pattern, 128);
									AddWeekPoint(pattern, 512);
									AddWeekPoint(pattern, 256);
									AddWeekPoint(pattern, 1);
									AddWeekPoint(pattern, 1024);
								}

								public bool CheckRepeatWeakPoint(int MsgID)
								{
									bool result = false;
									for (int i = 0; weekPoint.Length > i; i++)
									{
										if (weekPoint[i] == MsgID)
										{
											result = true;
										}
									}
									return result;
								}

								public void AddWeekPoint(int pattern, int flag)
								{
									if ((pattern & flag) == 0)
									{
										return;
									}
									switch (flag)
									{
									case 8:
										weekPoint[weekCount] = 60113;
										break;
									case 32:
										weekPoint[weekCount] = 60112;
										break;
									case 16:
										weekPoint[weekCount] = 60114;
										break;
									case 64:
										weekPoint[weekCount] = 60115;
										break;
									case 128:
										weekPoint[weekCount] = 60116;
										break;
									case 512:
										weekPoint[weekCount] = 60117;
										break;
									case 256:
										if (!CheckRepeatWeakPoint(60119))
										{
											weekPoint[weekCount] = 60119;
										}
										else
										{
											weekCount--;
										}
										break;
									case 1:
										if (!CheckRepeatWeakPoint(60119))
										{
											weekPoint[weekCount] = 60119;
										}
										else
										{
											weekCount--;
										}
										break;
									case 1024:
										weekPoint[weekCount] = 60118;
										break;
									}
									weekCount++;
								}

								public string getMobModelName(int ID)
								{
									string arg = "";
									sprintf(out arg, "f%03d", mon.MonsterManager.instance().monsterParameter(ID).familyId());
									return arg;
								}

								public string getMobTextureName(int ID)
								{
									string arg = "";
									sprintf(out arg, "f%03d_%03d", mon.MonsterManager.instance().monsterParameter(ID).familyId(), mon.MonsterManager.instance().monsterParameter(ID).monsterId());
									return arg;
								}

								public string getMobMotionName(int ID)
								{
									string arg = "";
									sprintf(out arg, "b_f%03d", mon.MonsterManager.instance().monsterParameter(ID).familyId());
									return arg;
								}

								public string getMobMapName(int ID)
								{
									byte b = mon.MonsterManager.instance().monsterParameter(ID).drawMapId();
									string arg = "";
									sprintf(out arg, "b%02d", b);
									return arg;
								}

								public static int isMobItemEnable(int ID)
								{
									if (198 == ID)
									{
										return 1;
									}
									if (mon.MonsterManager.instance().monsterManiaManager().monsterManiaForMonsterID(ID)
										.deadCount()
										.get() > 0)
									{
										return 1;
									}
									if (mon.MonsterManager.instance().monsterManiaManager().monsterManiaForMonsterID(ID)
										.deadCount()
										.get() <= 0)
									{
										return 0;
									}
									return -1;
								}

								public void mobEntryForceEnable(int ID)
								{
								}

								public bool isMobEntryFinish(int ID)
								{
									return mon.MonsterManager.instance().monsterManiaManager().monsterManiaForMonsterID(ID)
										.isEntryState(mon.MonsterMania.FINISH_ENTRY);
								}

								public void mobEntryFinish(int ID)
								{
									mon.MonsterManager.instance().monsterManiaManager().monsterManiaForMonsterID(ID)
										.setEntryState(mon.MonsterMania.FINISH_ENTRY);
								}

								public void enableList()
								{
									menu.Medget nodeByID = menu.MenuManager.getSingleton().root().getNodeByID(TRANSCODE("list"));
									if (nodeByID != null && nodeByID.behavior() != null)
									{
										menu.MBMonsterList mBMonsterList = (menu.MBMonsterList)nodeByID.behavior().queryInterface(menu.MBMonsterList.classIdentifier());
										if (mBMonsterList != null)
										{
											mBMonsterList.bmResume(nodeByID);
											mBMonsterList.getScrollBar().sbPartsActivateProcess(1);
											mBMonsterList.getScrollBar().sbRestrainCheck();
										}
										for (menu.Medget medget = nodeByID.childNode(); medget != null; medget = medget.nextSibling())
										{
											menu.MenuManager.getSingleton().joinFocusList(medget);
										}
									}
								}

								public void disableList(bool b)
								{
									menu.Medget nodeByID = menu.MenuManager.getSingleton().root().getNodeByID(TRANSCODE("list"));
									if (nodeByID == null || nodeByID.behavior() == null)
									{
										return;
									}
									menu.MBMonsterList mBMonsterList = (menu.MBMonsterList)nodeByID.behavior().queryInterface(menu.MBMonsterList.classIdentifier());
									if (mBMonsterList != null)
									{
										mBMonsterList.bmSuspend(nodeByID);
										menu.MenuManager.getSingleton().leaveFocusList(nodeByID);
										if (b)
										{
											mBMonsterList.getScrollBar().sbPartsActivateProcess(0);
											mBMonsterList.getScrollBar().sbRestrainCheck((ScrollBar.AREA_FLAG)3);
										}
										for (menu.Medget medget = nodeByID.childNode(); medget != null; medget = medget.nextSibling())
										{
											menu.MenuManager.getSingleton().leaveFocusList(medget);
										}
									}
								}

								public void clearDisplay()
								{
									if (-1 != ctrl_)
									{
										characterMng.delCharacter(ctrl_);
										characterMng.removeMotion(ctrl_, motname_);
										motname_ = "";
										ctrl_ = -1;
									}
									characterMng.terminate();
									stageMng.delStage();
									stageMng.terminate();
								}

								public void shiftTop()
								{
									menu.MenuManager.getSingleton().SetImposibleScrollFlag(val: true);
									menu.MenuManager.getSingleton().GetCursor2d().SetShow(show: false);
									menu.MenuManager.getSingleton().playSECancel();
									SCManager.getSingleton().GetSCTab().GetTopCursor()
										.SetCell(0);
									SCManager.getSingleton().GetSCTab().GetTopCursor()
										.SetAnimation(anm: true);
									clearDisplay();
									disableList(b: true);
									myState = 0;
								}
							}

							public class SCManager
							{
								public static SCManager instance_ = new SCManager();

								private SpecialContentsBase.SP_CONTENTS_PROCESS m_ProcState;

								private SpecialContentsBase.SP_CONTENTS_KIND m_Kind;

								private SpecialContentsBase.SP_CONTENTS_KIND m_PrevKind;

								private SpecialContentsBase.SP_CONTENTS_KIND m_NextKind;

								private bool myState;

								private SpecialContentsBase[] m_pCurrent = new SpecialContentsBase[2];

								private ThoroughData m_thorough = new ThoroughData();

								private MonsterBook m_monsterBook = new MonsterBook();

								private dgs.MSDINFO pMsdAddrBattle_;

								private dgs.MSDINFO pMsdAddrMenu_;

								private int battleMsdNo;

								private SCTab scTab = new SCTab();

								private wmenu.CWMenuButton Button_ = new wmenu.CWMenuButton();

								public SCManager()
								{
									m_ProcState = SpecialContentsBase.SP_CONTENTS_PROCESS.CONTENTS_INITIALIZE;
									m_Kind = SpecialContentsBase.SP_CONTENTS_KIND.CONTENTS_THOROUGH;
									m_PrevKind = SpecialContentsBase.SP_CONTENTS_KIND.CONTENTS_THOROUGH;
									m_NextKind = SpecialContentsBase.SP_CONTENTS_KIND.CONTENTS_THOROUGH;
								}

								public void initialize()
								{
									myState = true;
									m_ProcState = SpecialContentsBase.SP_CONTENTS_PROCESS.CONTENTS_INITIALIZE;
									m_Kind = SpecialContentsBase.SP_CONTENTS_KIND.CONTENTS_MONSTER_BOOK;
									m_PrevKind = SpecialContentsBase.SP_CONTENTS_KIND.CONTENTS_THOROUGH;
									m_NextKind = SpecialContentsBase.SP_CONTENTS_KIND.CONTENTS_THOROUGH;
									menu.MenuManager.getSingleton().GetCursor2d().SetShow(show: false);
									GX_Power3D(0);
									changeGlobalDirectory();
									mon.MonsterManager.instance().load();
									changeCompanyDirectory();
									LoadNeedMsdData();
									changeGlobalDirectory();
									menu.MBMonsterNewMark.setupMark();
									menu.MBMonsterBossMark.setupMark();
									m_pCurrent[1] = m_thorough;
									m_pCurrent[0] = m_monsterBook;
									pCurrent().initialize();
									GetSCTab().SetTabData();
									GetSCTab().GetTopCursor().SetAnimation(anm: true);
									Button_.initialize();
									Button_.SetUpSpecialVer();
									Button_.SetButtonBActivity(b: true);
									Button_.SetButtonAActivity(b: false);
									getSingleton().SetProcState(SpecialContentsBase.SP_CONTENTS_PROCESS.CONTENTS_EXECUTE);
								}

								public void execute()
								{
									switch (m_ProcState)
									{
									case SpecialContentsBase.SP_CONTENTS_PROCESS.CONTENTS_EXECUTE:
										OS_AssignBackButton(1);
										pCurrent().execute();
										break;
									case SpecialContentsBase.SP_CONTENTS_PROCESS.CONTENTS_TERMINATE:
										pCurrent().terminate();
										m_PrevKind = m_Kind;
										m_Kind = m_NextKind;
										pCurrent().initialize();
										m_ProcState = SpecialContentsBase.SP_CONTENTS_PROCESS.CONTENTS_EXECUTE;
										break;
									case SpecialContentsBase.SP_CONTENTS_PROCESS.CONTENTS_EP:
										if (dgs.CFade.Main().isCleared() && dgs.CFade.Sub().isCleared())
										{
											dgs.CFade.Main().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
											dgs.CFade.Sub().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
										}
										if (dgs.CFade.Main().isFaded() && dgs.CFade.Sub().isFaded())
										{
											SetProcState(SpecialContentsBase.SP_CONTENTS_PROCESS.CONTENTS_END);
										}
										break;
									case SpecialContentsBase.SP_CONTENTS_PROCESS.CONTENTS_END:
										pCurrent().terminate();
										SetMyState(val: false);
										SetProcState(SpecialContentsBase.SP_CONTENTS_PROCESS.CONTENTS_HALT);
										break;
									case SpecialContentsBase.SP_CONTENTS_PROCESS.CONTENTS_HALT:
										break;
									}
								}

								public void terminate()
								{
									m_pCurrent[1].finish();
									m_pCurrent[0].finish();
									GetSCTab().DeleteTabData();
									ReleaseNeedMsdData();
									mon.MonsterManager.instance().free();
									menu.MenuManager.getSingleton().GetCursor2d().SetShow(show: false);
									menu.MenuManager.getSingleton().releaseWindowAll();
									menu.MenuManager.getSingleton().release();
									GX_Power3D(1);
									menu.MBMonsterNewMark.releaseMark();
									menu.MBMonsterBossMark.releaseMark();
									Button_.terminate();
								}

								public void LoadNeedMsdData()
								{
									dgs.msg.CMessageMng cMessageMng = dgs.msg.CMessageSys.getInstance().Sub();
									pMsdAddrBattle_ = null;
									Array array = null;
									string filename = "eureka_battle.msd";
									uint size = ds.g_File.getSize(filename);
									if (size != 0)
									{
										array = ds.CHeap.alloc_app(size);
										if (array != null)
										{
											ds.g_File.load(array, filename);
										}
										pMsdAddrBattle_ = (dgs.MSDINFO)array;
									}
									if (pMsdAddrBattle_ != null)
									{
										cMessageMng.setUpMSD(pMsdAddrBattle_, dgs.msg.CMessageMng.MSD_HANDLE_KIND.MSD_HANDLE_KIND_GAME_PART);
										battleMsdNo = cMessageMng.m_MsdHandle[1];
									}
									pMsdAddrMenu_ = null;
									Array array2 = null;
									string filename2 = "eureka_menu.msd";
									uint size2 = ds.g_File.getSize(filename2);
									if (size2 != 0)
									{
										array2 = ds.CHeap.alloc_app(size2);
										if (array2 != null)
										{
											ds.g_File.load(array2, filename2);
										}
										pMsdAddrMenu_ = (dgs.MSDINFO)array2;
									}
									if (pMsdAddrMenu_ != null)
									{
										cMessageMng.setUpMSD(pMsdAddrMenu_, dgs.msg.CMessageMng.MSD_HANDLE_KIND.MSD_HANDLE_KIND_GAME_PART);
									}
								}

								public void ReleaseNeedMsdData()
								{
									if (pMsdAddrBattle_ != null)
									{
										dgs.msg.CMessageSys.getInstance().Sub().removeMSD(pMsdAddrBattle_);
										ds.CHeap.free_app(pMsdAddrBattle_);
										pMsdAddrBattle_ = null;
									}
									if (pMsdAddrMenu_ != null)
									{
										dgs.msg.CMessageSys.getInstance().Sub().removeMSD(pMsdAddrMenu_);
										ds.CHeap.free_app(pMsdAddrMenu_);
										pMsdAddrMenu_ = null;
									}
								}

								public static SCManager getSingleton()
								{
									return instance_;
								}

								public void SetProcState(SpecialContentsBase.SP_CONTENTS_PROCESS stat)
								{
									m_ProcState = stat;
								}

								public SpecialContentsBase.SP_CONTENTS_PROCESS GetProcState()
								{
									return m_ProcState;
								}

								public void SetKind(SpecialContentsBase.SP_CONTENTS_KIND stat)
								{
									m_Kind = stat;
								}

								public SpecialContentsBase.SP_CONTENTS_KIND GetKind()
								{
									return m_Kind;
								}

								public void SetPrevKind(SpecialContentsBase.SP_CONTENTS_KIND stat)
								{
									m_PrevKind = stat;
								}

								public SpecialContentsBase.SP_CONTENTS_KIND GetPrevKind()
								{
									return m_PrevKind;
								}

								public void SetNextKind(SpecialContentsBase.SP_CONTENTS_KIND stat)
								{
									m_NextKind = stat;
								}

								public SpecialContentsBase.SP_CONTENTS_KIND GetNextKind()
								{
									return m_NextKind;
								}

								public void SetMyState(bool val)
								{
									myState = val;
								}

								public bool GetMyState()
								{
									return myState;
								}

								public SpecialContentsBase pCurrent()
								{
									return m_pCurrent[(int)m_Kind];
								}

								public SCTab GetSCTab()
								{
									return scTab;
								}

								public int GetSpecialBattleMsdNo()
								{
									return battleMsdNo;
								}

								public wmenu.CWMenuButton BottonIcon()
								{
									return Button_;
								}

								public ThoroughData GetThoroughData()
								{
									return m_thorough;
								}

								public MonsterBook GetMonsterBook()
								{
									return m_monsterBook;
								}
							}

							public class SCTab
							{
								public enum SP_TAB
								{
									SP_TAB_MONSTER_BOOK,
									SP_TAB_THOROUGH,
									SP_TAB_MAX
								}

								public class SP_TAB_MEMBER
								{
									public int indexNo;

									public ds.Vector2<short> pos = new ds.Vector2<short>();

									public ds.Vector2<short> size = new ds.Vector2<short>();

									public dgs.DGSMessage pMsg;
								}

								public class SP_TAB_CURSOR
								{
									public int targetNo;

									public sys2d.Cell cell = new sys2d.Cell();
								}

								public const SP_TAB SP_TAB_MONSTER_BOOK = SP_TAB.SP_TAB_MONSTER_BOOK;

								public const SP_TAB SP_TAB_THOROUGH = SP_TAB.SP_TAB_THOROUGH;

								public const SP_TAB SP_TAB_MAX = SP_TAB.SP_TAB_MAX;

								private SP_TAB_MEMBER[] s_Tab = new SP_TAB_MEMBER[2];

								private SP_TAB_CURSOR tCursor = new SP_TAB_CURSOR();

								public void SetTabData()
								{
									dgs.DGSMessageManager dGSMessageManager = dgs.msg.CMessageSys.getInstance().Sub();
									s_Tab[0].indexNo = 0;
									s_Tab[0].pos.vx = (short)MOBOOK_POS_X;
									s_Tab[0].pos.vy = (short)MOBOOK_POS_Y;
									s_Tab[0].size.vx = (short)MOBOOK_W;
									s_Tab[0].size.vy = (short)MOBOOK_H;
									s_Tab[0].pMsg = null;
									s_Tab[0].pMsg = dGSMessageManager.createMessage(60001u, menu.MenuManager.getSingleton().GetSpecialDataTextNo(), 1);
									if (s_Tab[0].pMsg != null)
									{
										ds.Vector2<short> vector = new ds.Vector2<short>(0, 0);
										ds.Vector2<short> vector2 = new ds.Vector2<short>(0, 0);
										s_Tab[0].pMsg.getCompleteTextSize(vector);
										vector2.vx = (short)(s_Tab[0].pos.vx + s_Tab[0].size.vx / 2 - vector.vx / 2);
										vector2.vy = (short)(s_Tab[0].pos.vy + s_Tab[0].size.vy / 2 - vector.vy / 2);
										s_Tab[0].pMsg.setPosition(vector2.vx, vector2.vy, erase: true);
										s_Tab[0].pMsg.setDisplaySpeed(byte.MaxValue);
										s_Tab[0].pMsg.setDisplayWait(0);
									}
									s_Tab[1].indexNo = 1;
									s_Tab[1].pos.vx = (short)THROUGH_POS_X;
									s_Tab[1].pos.vy = (short)THROUGH_POS_Y;
									s_Tab[1].size.vx = (short)THROUGH_W;
									s_Tab[1].size.vy = (short)THROUGH_H;
									s_Tab[1].pMsg = null;
									s_Tab[1].pMsg = dGSMessageManager.createMessage(60002u, menu.MenuManager.getSingleton().GetSpecialDataTextNo(), 1);
									if (s_Tab[1].pMsg != null)
									{
										ds.Vector2<short> vector3 = new ds.Vector2<short>(0, 0);
										ds.Vector2<short> vector4 = new ds.Vector2<short>(0, 0);
										s_Tab[1].pMsg.getCompleteTextSize(vector3);
										vector4.vx = (short)(s_Tab[1].pos.vx + s_Tab[1].size.vx / 2 - vector3.vx / 2);
										vector4.vy = (short)(s_Tab[1].pos.vy + s_Tab[1].size.vy / 2 - vector3.vy / 2);
										s_Tab[1].pMsg.setPosition(vector4.vx, vector4.vy, erase: true);
										s_Tab[1].pMsg.setDisplaySpeed(byte.MaxValue);
										s_Tab[1].pMsg.setDisplayWait(0);
									}
									ds.Vector2<short> vec = new ds.Vector2<short>(0, 0);
									ds.Vector2<short> vector5 = new ds.Vector2<short>(0, 0);
									s_Tab[0].pMsg.getCompleteTextSize(vec);
									vector5.vx = (short)(s_Tab[0].pos.vx + CURSOR_X);
									vector5.vy = (short)(s_Tab[0].pos.vy + s_Tab[0].size.vy / 2);
									tCursor.cell.copy(menu.MenuManager.getSingleton().GetCursor2d());
									tCursor.cell.SetPositionI(vector5.vx, vector5.vy);
									tCursor.cell.SetShow(show: true);
									tCursor.cell.SetCell(0);
									tCursor.cell.SetAnimation(anm: true);
									tCursor.cell.SetPriority(0);
									sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(tCursor.cell);
									tCursor.targetNo = 0;
								}

								public void DeleteTabData()
								{
									if (s_Tab[0].pMsg != null)
									{
										s_Tab[0].pMsg.release();
										s_Tab[0].pMsg = null;
									}
									if (s_Tab[1].pMsg != null)
									{
										s_Tab[1].pMsg.release();
										s_Tab[1].pMsg = null;
									}
									sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(tCursor.cell);
									tCursor.cell.Release();
								}

								public void Executioner()
								{
									ds.Vector2<short> vector = new ds.Vector2<short>(0, 0);
									if ((ds.g_Pad.repeat() & 0x10) != 0)
									{
										if (++tCursor.targetNo > 1)
										{
											tCursor.targetNo = 0;
											vector.vx = (short)(s_Tab[0].pos.vx + CURSOR_X);
											vector.vy = (short)(s_Tab[0].pos.vy + s_Tab[0].size.vy / 2);
										}
										else
										{
											vector.vx = (short)(s_Tab[1].pos.vx + CURSOR_X);
											vector.vy = (short)(s_Tab[1].pos.vy + s_Tab[1].size.vy / 2);
										}
										tCursor.cell.SetPositionI(vector.vx, vector.vy);
										menu.MenuManager.getSingleton().playSEMoveCursor();
									}
									else if ((ds.g_Pad.repeat() & 0x20) != 0)
									{
										if (--tCursor.targetNo < 0)
										{
											tCursor.targetNo = 1;
											vector.vx = (short)(s_Tab[1].pos.vx + CURSOR_X);
											vector.vy = (short)(s_Tab[1].pos.vy + s_Tab[1].size.vy / 2);
										}
										else
										{
											vector.vx = (short)(s_Tab[0].pos.vx + CURSOR_X);
											vector.vy = (short)(s_Tab[0].pos.vy + s_Tab[0].size.vy / 2);
										}
										tCursor.cell.SetPositionI(vector.vx, vector.vy);
										menu.MenuManager.getSingleton().playSEMoveCursor();
									}
									else if ((ds.g_Pad.edge() & 1) != 0)
									{
										SCManager.getSingleton().SetNextKind((SpecialContentsBase.SP_CONTENTS_KIND)tCursor.targetNo);
										SCManager.getSingleton().SetProcState(SpecialContentsBase.SP_CONTENTS_PROCESS.CONTENTS_TERMINATE);
										menu.MenuManager.getSingleton().playSEDecide();
									}
									else if ((ds.g_Pad.edge() & 2) != 0 || SCManager.getSingleton().BottonIcon().TouchButtonB())
									{
										SCManager.getSingleton().SetProcState(SpecialContentsBase.SP_CONTENTS_PROCESS.CONTENTS_EP);
										menu.MenuManager.getSingleton().playSECancel();
									}
									else
									{
										if (!ds.g_TouchPanel.isEdge())
										{
											return;
										}
										int x = 0;
										int y = 0;
										ds.g_TouchPanel.getLastPoint(out x, out y);
										bool flag = false;
										for (int i = 0; i < 2; i++)
										{
											if (!HitArea(x, y, s_Tab[i].pos.vx, s_Tab[i].pos.vy, s_Tab[i].size.vx, s_Tab[i].size.vy))
											{
												continue;
											}
											ds.Vector2<short> vector2 = new ds.Vector2<short>(0, 0);
											if (i == 0)
											{
												if (tCursor.targetNo == 0)
												{
													SCManager.getSingleton().SetNextKind(SpecialContentsBase.SP_CONTENTS_KIND.CONTENTS_MONSTER_BOOK);
													SCManager.getSingleton().SetProcState(SpecialContentsBase.SP_CONTENTS_PROCESS.CONTENTS_TERMINATE);
													menu.MenuManager.getSingleton().playSEDecide();
												}
												else
												{
													vector2.vx = (short)(s_Tab[0].pos.vx + CURSOR_X);
													vector2.vy = (short)(s_Tab[0].pos.vy + s_Tab[0].size.vy / 2);
													tCursor.cell.SetPositionI(vector2.vx, vector2.vy);
													tCursor.targetNo = 0;
													menu.MenuManager.getSingleton().playSEMoveCursor();
												}
											}
											else if (tCursor.targetNo == 1)
											{
												SCManager.getSingleton().SetNextKind(SpecialContentsBase.SP_CONTENTS_KIND.CONTENTS_THOROUGH);
												SCManager.getSingleton().SetProcState(SpecialContentsBase.SP_CONTENTS_PROCESS.CONTENTS_TERMINATE);
												menu.MenuManager.getSingleton().playSEDecide();
											}
											else
											{
												vector2.vx = (short)(s_Tab[1].pos.vx + CURSOR_X);
												vector2.vy = (short)(s_Tab[1].pos.vy + s_Tab[1].size.vy / 2);
												menu.MenuManager.getSingleton().GetCursor2d().SetShow(show: false);
												tCursor.cell.SetPositionI(vector2.vx, vector2.vy);
												tCursor.targetNo = 1;
												menu.MenuManager.getSingleton().playSEMoveCursor();
											}
										}
									}
								}

								public void TouchTopTabIcon()
								{
									if (!ds.g_TouchPanel.isRelease())
									{
										return;
									}
									int x = 0;
									int y = 0;
									ds.g_TouchPanel.getLastPoint(out x, out y);
									bool flag = false;
									for (int i = 0; i < 2; i++)
									{
										if (HitArea(x, y, s_Tab[i].pos.vx, s_Tab[i].pos.vy, s_Tab[i].size.vx, s_Tab[i].size.vy))
										{
											SCManager.getSingleton().SetNextKind((SpecialContentsBase.SP_CONTENTS_KIND)i);
											SCManager.getSingleton().SetProcState(SpecialContentsBase.SP_CONTENTS_PROCESS.CONTENTS_TERMINATE);
											tCursor.targetNo = i;
											ds.Vector2<short> vec = new ds.Vector2<short>(0, 0);
											ds.Vector2<short> vector = new ds.Vector2<short>(0, 0);
											if (SCManager.getSingleton().GetNextKind() == SpecialContentsBase.SP_CONTENTS_KIND.CONTENTS_MONSTER_BOOK)
											{
												s_Tab[0].pMsg.getCompleteTextSize(vec);
												vector.vx = (short)(s_Tab[0].pos.vx + CURSOR_X);
												vector.vy = (short)(s_Tab[0].pos.vy + s_Tab[0].size.vy / 2);
												tCursor.cell.SetPositionI(vector.vx, vector.vy);
												SCManager.getSingleton().GetMonsterBook().shiftTop();
											}
											else if (SCManager.getSingleton().GetNextKind() == SpecialContentsBase.SP_CONTENTS_KIND.CONTENTS_THOROUGH)
											{
												s_Tab[1].pMsg.getCompleteTextSize(vec);
												vector.vx = (short)(s_Tab[1].pos.vx + CURSOR_X);
												vector.vy = (short)(s_Tab[1].pos.vy + s_Tab[1].size.vy / 2);
												menu.MenuManager.getSingleton().GetCursor2d().SetShow(show: false);
												tCursor.cell.SetPositionI(vector.vx, vector.vy);
												menu.MenuManager.getSingleton().playSECancel();
											}
											break;
										}
									}
								}

								public bool TouchTopTabIconArea(int i)
								{
									bool result = false;
									if (ds.g_TouchPanel.isEdge())
									{
										int x = 0;
										int y = 0;
										ds.g_TouchPanel.getLastPoint(out x, out y);
										if (HitArea(x, y, s_Tab[i].pos.vx, s_Tab[i].pos.vy, s_Tab[i].size.vx, s_Tab[i].size.vy))
										{
											return true;
										}
									}
									return result;
								}

								public void SetTopCursorPos(int i)
								{
									int x = s_Tab[i].pos.vx + CURSOR_X;
									int y = s_Tab[i].pos.vy + s_Tab[i].size.vy / 2;
									tCursor.cell.SetPositionI(x, y);
								}

								public bool HitArea(int x, int y, int sx, int sy, int w, int h)
								{
									if (x > sx && x < sx + w && y > sy && y < sy + h)
									{
										return true;
									}
									return false;
								}

								public SCTab()
								{
									for (int i = 0; i < s_Tab.Length; i++)
									{
										s_Tab[i] = new SP_TAB_MEMBER();
									}
								}

								public sys2d.Cell GetTopCursor()
								{
									return tCursor.cell;
								}
							}

							public class ThoroughData : SpecialContentsBase
							{
								private SPECIAL_MSG_DATA[] thoroughMsg = new SPECIAL_MSG_DATA[ThoroughMsgMax];

								private sys2d.Bg BgMenu_ = new sys2d.Bg();

								public ThoroughData()
								{
									for (int i = 0; i < thoroughMsg.Length; i++)
									{
										thoroughMsg[i] = new SPECIAL_MSG_DATA();
									}
								}

								public override void initialize()
								{
									if (SCManager.getSingleton().GetPrevKind() != SP_CONTENTS_KIND.CONTENTS_THOROUGH)
									{
										BgMenu_.bgLoad("yarikomi.NSCR", "menu_bg_01.NCGR", "new_menu_bg.NCLR");
										BgMenu_.bgSetUp(NNSG2dBGSelect.NNS_G2D_BGSELECT_SUB0);
										BgMenu_.bgRelease();
										menu.MenuManager.getSingleton().buildMenu("thorough");
										menu.MenuManager.getSingleton().GetCursor2d().SetShow(show: false);
										ClearUseMessageNo(thoroughMsg, ThoroughMsgMax);
										bmRefresh();
									}
									SCManager.getSingleton().SetProcState(SP_CONTENTS_PROCESS.CONTENTS_EXECUTE);
								}

								public override void execute()
								{
									if (SCManager.getSingleton().GetSCTab().TouchTopTabIconArea(0))
									{
										SCManager.getSingleton().GetSCTab().SetTopCursorPos(0);
										SCManager.getSingleton().SetNextKind(SP_CONTENTS_KIND.CONTENTS_MONSTER_BOOK);
										SCManager.getSingleton().SetProcState(SP_CONTENTS_PROCESS.CONTENTS_TERMINATE);
										menu.MenuManager.getSingleton().playSEMoveCursor();
									}
									else if ((ds.g_Pad.edge() & 2) != 0 || SCManager.getSingleton().BottonIcon().TouchButtonB())
									{
										SCManager.getSingleton().SetProcState(SP_CONTENTS_PROCESS.CONTENTS_EP);
										menu.MenuManager.getSingleton().playSECancel();
									}
								}

								public override void terminate()
								{
									if (SCManager.getSingleton().GetNextKind() != SP_CONTENTS_KIND.CONTENTS_THOROUGH)
									{
										ClearUseMessageNo(thoroughMsg, ThoroughMsgMax);
										menu.MenuManager.getSingleton().release();
									}
								}

								public override void finish()
								{
									BgMenu_.bgRelease();
									ClearUseMessageNo(thoroughMsg, ThoroughMsgMax);
									menu.MenuManager.getSingleton().release();
								}

								public override void bmRefresh()
								{
									ds.Vector2<short> vector = new ds.Vector2<short>();
									ClearUseMessageNo(thoroughMsg, ThoroughMsgMax);
									int h = 0;
									int m = 0;
									int s = 0;
									string arg = "";
									fastestClearTime(out h, out m, out s);
									sprintf(out arg, "%02d%s%02d%s%02d", h, dgs.msg.CMessageSys.getInstance().Sub().getMessage(60127u), m, dgs.msg.CMessageSys.getInstance().Sub().getMessage(60127u), s);
									int num = SearchUseMessageNo(thoroughMsg, ThoroughMsgMax);
									if (-1 < num)
									{
										vector.vx = 408;
										vector.vy = 76;
										createSpecialMsg(thoroughMsg, num, arg, vector);
										thoroughMsg[num].pMsg.setStyle(2048u);
									}
									num = SearchUseMessageNo(thoroughMsg, ThoroughMsgMax);
									string after;
									if (num != -1)
									{
										dgs.msg.CMessageSys.getInstance().changeValueFont(treasureHuntRate(), out after);
										vector.vx = 396;
										vector.vy = 100;
										createSpecialMsg(thoroughMsg, num, after, vector);
									}
									num = SearchUseMessageNo(thoroughMsg, ThoroughMsgMax);
									if (num != -1)
									{
										vector.vx = 420;
										vector.vy = 100;
										createSpecialMsg(thoroughMsg, num, 60126, vector);
									}
									num = SearchUseMessageNo(thoroughMsg, ThoroughMsgMax);
									if (num != -1)
									{
										dgs.msg.CMessageSys.getInstance().changeValueFont(enemyBreakNumber(), out after);
										vector.vx = 408;
										vector.vy = 124;
										createSpecialMsg(thoroughMsg, num, after, vector);
									}
									num = SearchUseMessageNo(thoroughMsg, ThoroughMsgMax);
									if (num != -1)
									{
										dgs.msg.CMessageSys.getInstance().changeValueFont(escapeNumber(), out after);
										vector.vx = 408;
										vector.vy = 148;
										createSpecialMsg(thoroughMsg, num, after, vector);
									}
									num = SearchUseMessageNo(thoroughMsg, ThoroughMsgMax);
									if (num != -1)
									{
										dgs.msg.CMessageSys.getInstance().changeValueFont(maxDamage(), out after);
										vector.vx = 408;
										vector.vy = 172;
										createSpecialMsg(thoroughMsg, num, after, vector);
									}
									num = SearchUseMessageNo(thoroughMsg, ThoroughMsgMax);
									if (num != -1)
									{
										dgs.msg.CMessageSys.getInstance().changeValueFont(maxHitNumber(), out after);
										vector.vx = 408;
										vector.vy = 196;
										createSpecialMsg(thoroughMsg, num, after, vector);
									}
									num = SearchUseMessageNo(thoroughMsg, ThoroughMsgMax);
									if (num != -1)
									{
										sprintf(out after, "%d / 23", masterCardNumber());
										vector.vx = 408;
										vector.vy = 220;
										createSpecialMsg(thoroughMsg, num, after, vector);
									}
								}
							}

							private static bool _hEnd = false;

							private static bool partEnd = false;

							public static int MBookMsgMax = 16;

							public static int WeekPointMax = 5;

							public static int DoropItemMax = 9;

							private static int MOBOOK_POS_X = 0;

							private static int MOBOOK_POS_Y = 0;

							private static int MOBOOK_W = 144;

							private static int MOBOOK_H = 32;

							private static int THROUGH_POS_X = 144;

							private static int THROUGH_POS_Y = 0;

							private static int THROUGH_W = 144;

							private static int THROUGH_H = 32;

							private static int CURSOR_X = 24;

							public static int ThoroughMsgMax = 16;

							public static int mobookGetLife(int nMobID)
							{
								return mon.MonsterManager.instance().monsterParameter(nMobID).maxHp();
							}

							public static int mobookGetExp(int nMobID)
							{
								return mon.MonsterManager.instance().monsterParameter(nMobID).droppingParameter()
									.exp();
							}

							public static int mobookGetMoney(int nMobID)
							{
								return mon.MonsterManager.instance().monsterParameter(nMobID).droppingParameter()
									.gold();
							}

							public static int mobookGetLv(int nMobID)
							{
								return mon.MonsterManager.instance().monsterParameter(nMobID).level();
							}

							public static int mobookGetAttack(int nMobID)
							{
								return mon.MonsterManager.instance().monsterParameter(nMobID).physicsAttack()
									.aggressivity()
									.get();
							}

							public static int mobookGetDef(int nMobID)
							{
								return mon.MonsterManager.instance().monsterParameter(nMobID).physicsDefense()
									.phylacticPower()
									.get();
							}

							public static void mobookDebugSetting()
							{
							}

							public static void mobookDebugInitialize()
							{
							}

							public static void mobookDebugDump()
							{
							}
						}
}
