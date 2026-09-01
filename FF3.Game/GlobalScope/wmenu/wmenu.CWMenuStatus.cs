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
							public class CWMenuStatus : CWMenuMemberBase
							{
								public const int STATUS_WINDOW_MAIN = 0;

								public const int STATUS_WINDOW_SUB = 1;

								public const int STATUS_WINDOW_MAX = 2;

								private int playerMCount;

								private int playerIndex;

								private int[] playerBox = new int[4];

								private int currentPlayer;

								private int[] windowNo = new int[2];

								private int slideTime;

								private int slideDir;

								public override bool cSelectInitialize()
								{
									return true;
								}

								public override void initialize()
								{
									setFinalize(val: false);
									SVC_WaitVBlankIntr();
									CWMenuManager.Instance().SetPrimaryBG(3);
									for (int i = 0; i < 4; i++)
									{
										pl.PlayerParty.instance().player((byte)i).updateParameter();
										CWMenuManager.Instance().SetShowPcFace(i, show: false);
										CWMenuManager.Instance().SetCharScrMovement(2, 7, i);
									}
									CWMenuManager.Instance().SetShowPcFace(pl.PlayerParty.instance().player((byte)menu.MenuManager.getSingleton().GetTargetCharNo()).playerId(), show: true);
									currentPlayer = menu.MenuManager.getSingleton().GetTargetCharNo();
									menu.MenuManager.getSingleton().ClearBehaviorButton();
									menu.MenuManager.getSingleton().buildMenu("status");
									menu.MenuManager.getSingleton().GetCursor2d().SetShow(show: false);
									playerMCount = 0;
									playerIndex = 0;
									for (int j = 0; j < 4; j++)
									{
										if (pl.PlayerParty.instance().player((byte)j).isEnable())
										{
											playerBox[playerMCount] = j;
											playerMCount++;
										}
									}
									for (int k = 0; k < playerMCount; k++)
									{
										if (menu.MenuManager.getSingleton().GetTargetCharNo() == playerBox[k])
										{
											playerIndex = k;
											currentPlayer = playerBox[k];
											break;
										}
									}
									if (playerMCount <= 1)
									{
										CWMenuManager.Instance().GetMenuButton().SetUpSpecialVer();
									}
									else
									{
										CWMenuManager.Instance().GetMenuButton().SetUpNormalVer();
									}
									slideTime = 0;
									slideDir = 0;
									CWMenuManager.Instance().SetProcState(WMENU_PROCESS.WMENU_PROCESS_RUN);
								}

								public override void run()
								{
									menu.MenuManager.getSingleton().execute();
									if (slideDir != 0)
									{
										slideTime++;
										G2_SetScreenOffset(((slideTime < 4) ? slideTime : (slideTime - 8)) * slideDir * 120, 0);
										if (slideTime == 4)
										{
											playerIndex = (playerIndex + slideDir + playerMCount) % playerMCount;
											currentPlayer = playerBox[playerIndex];
											wmsRefresh(ownerMedget);
											for (int i = 0; i < 4; i++)
											{
												CWMenuManager.Instance().SetShowPcFace(i, show: false);
											}
											CWMenuManager.Instance().SetCharScrMovement(2, 7, pl.PlayerParty.instance().player((byte)currentPlayer).playerId());
											CWMenuManager.Instance().SetShowPcFace(pl.PlayerParty.instance().player((byte)currentPlayer).playerId(), show: true);
										}
										if (slideTime == 8)
										{
											menu.MenuManager.getSingleton().inputPermission(b: true);
											slideTime = 0;
											slideDir = 0;
										}
									}
									else if ((ds.g_Pad.edge() & 2) != 0 || CWMenuManager.Instance().GetMenuButton().TouchButtonB())
									{
										menu.MenuManager.getSingleton().playSECancel();
										CWMenuManager.Instance().SetNextKind(WMENU_KIND.WMENU_KIND_MAIN_MENU);
										CWMenuManager.Instance().SetProcState(WMENU_PROCESS.WMENU_PROCESS_TERMINATE);
									}
								}

								public override void terminate()
								{
									if (!isFinalize())
									{
										menu.MenuManager.getSingleton().releaseWindowAll();
										menu.MenuManager.getSingleton().release();
										setFinalize(val: true);
									}
								}

								public override void mbDelete()
								{
								}

								public override menu.MenuBehavior mbfCreate()
								{
									return CWMenuManager.Instance().GetWMenuStatus();
								}

								public override void bmInitialize(menu.Medget M)
								{
								}

								public override void bmPostInitialize(menu.Medget M)
								{
									currentPlayer = menu.MenuManager.getSingleton().GetTargetCharNo();
									wmsRefresh(M);
									wmsRefresh(M);
								}

								public void wmsRefresh(menu.Medget M)
								{
									menu.MBText mBText = null;
									pl.Player player = pl.PlayerParty.instance().player((byte)currentPlayer);
									menu.Medget nodeByIDFromChildren = M.getNodeByIDFromChildren(TRANSCODE("mbs_name"));
									((menu.MBText)nodeByIDFromChildren.behavior().queryInterface(menu.MBText.classIdentifier()))?.mbSetBufferMsg(player.name(), decWidth: false);
									nodeByIDFromChildren = M.getNodeByIDFromChildren(TRANSCODE("mbs_job"));
									((menu.MBJobName)nodeByIDFromChildren.behavior().queryInterface(menu.MBJobName.classIdentifier()))?.mbjnChangePlayerNumber((byte)currentPlayer);
									nodeByIDFromChildren = M.getNodeByID(TRANSCODE("LV"));
									mBText = (menu.MBText)nodeByIDFromChildren.behavior().queryInterface(menu.MBText.classIdentifier());
									string after;
									if (mBText != null)
									{
										dgs.msg.CMessageSys.getInstance().changeValueFont(player.level().get(), out after);
										mBText.mbSetBufferMsg(after, decWidth: false);
									}
									nodeByIDFromChildren = M.getNodeByID(TRANSCODE("SKILL"));
									mBText = (menu.MBText)nodeByIDFromChildren.behavior().queryInterface(menu.MBText.classIdentifier());
									if (mBText != null)
									{
										int type = player.jobManager().nowJob();
										dgs.msg.CMessageSys.getInstance().changeValueFont(player.jobManager().job((pl.JOB_TYPE)type).skill()
											.skillLevel()
											.get(), out after);
										mBText.mbSetBufferMsg(after, decWidth: false);
									}
									nodeByIDFromChildren = M.getNodeByID(TRANSCODE("HP"));
									mBText = (menu.MBText)nodeByIDFromChildren.behavior().queryInterface(menu.MBText.classIdentifier());
									if (mBText != null)
									{
										dgs.msg.CMessageSys.getInstance().changeValueFont(player.hp().getNow(), out after);
										mBText.mbSetBufferMsg(after, decWidth: false);
										mBText.changeTextColor(player.checkHpColor());
									}
									nodeByIDFromChildren = M.getNodeByID(TRANSCODE("HPMax"));
									mBText = (menu.MBText)nodeByIDFromChildren.behavior().queryInterface(menu.MBText.classIdentifier());
									if (mBText != null)
									{
										dgs.msg.CMessageSys.getInstance().changeValueFont(player.hp().getLimit(), out after);
										mBText.mbSetBufferMsg(after, decWidth: false);
										mBText.changeTextColor(player.checkHpColor());
									}
									nodeByIDFromChildren = M.getNodeByID(TRANSCODE("HPSLASH"));
									if (nodeByIDFromChildren != null)
									{
										((menu.MBText)nodeByIDFromChildren.behavior().queryInterface(menu.MBText.classIdentifier()))?.mbSetTextColor(dgs.TXT_COLOR.TXT_COLOR_WHITE);
									}
									nodeByIDFromChildren = M.getNodeByID(TRANSCODE("MP"));
									nodeByIDFromChildren = nodeByIDFromChildren.childNode();
									for (int i = 0; i < 8; i++)
									{
										mBText = (menu.MBText)nodeByIDFromChildren.behavior().queryInterface(menu.MBText.classIdentifier());
										if (mBText != null)
										{
											dgs.msg.CMessageSys.getInstance().changeValueFont(player.mp(i).getNow(), out after);
											mBText.mbSetBufferMsg(after, decWidth: false);
										}
										nodeByIDFromChildren = nodeByIDFromChildren.nextSibling();
										mBText = (menu.MBText)nodeByIDFromChildren.behavior().queryInterface(menu.MBText.classIdentifier());
										if (mBText != null)
										{
											dgs.msg.CMessageSys.getInstance().changeValueFont(player.mp(i).getLimit(), out after);
											mBText.mbSetBufferMsg(after, decWidth: false);
										}
										nodeByIDFromChildren = nodeByIDFromChildren.nextSibling();
									}
									nodeByIDFromChildren = M.getNodeByID(TRANSCODE("MPSLASH"));
									for (int j = 0; j < 8; j++)
									{
										((menu.MBText)nodeByIDFromChildren.behavior().queryInterface(menu.MBText.classIdentifier()))?.mbSetTextColor(dgs.TXT_COLOR.TXT_COLOR_WHITE);
										nodeByIDFromChildren = nodeByIDFromChildren.nextSibling();
									}
									nodeByIDFromChildren = M.getNodeByID(TRANSCODE("EXP"));
									mBText = (menu.MBText)nodeByIDFromChildren.behavior().queryInterface(menu.MBText.classIdentifier());
									if (mBText != null)
									{
										dgs.msg.CMessageSys.getInstance().changeValueFont(player.exp().get(), out after);
										mBText.mbSetBufferMsg(after, decWidth: false);
									}
									nodeByIDFromChildren = M.getNodeByID(TRANSCODE("EXPMax"));
									mBText = (menu.MBText)nodeByIDFromChildren.behavior().queryInterface(menu.MBText.classIdentifier());
									if (mBText != null)
									{
										int num = player.level().get();
										int num2 = 0;
										num2 = ((num != 99) ? (pl.PlayerParty.instance().playerExp()[0].exp((byte)num) - player.exp().get()) : 0);
										dgs.msg.CMessageSys.getInstance().changeValueFont(num2, out after);
										mBText.mbSetBufferMsg(after, decWidth: false);
									}
									nodeByIDFromChildren = M.getNodeByID(TRANSCODE("STR"));
									nodeByIDFromChildren = nodeByIDFromChildren.childNode();
									nodeByIDFromChildren = nodeByIDFromChildren.nextSibling();
									mBText = (menu.MBText)nodeByIDFromChildren.behavior().queryInterface(menu.MBText.classIdentifier());
									if (mBText != null)
									{
										int num3 = player.bodyAndBonus().strength().get();
										int num4 = player.body().strength().get();
										dgs.msg.CMessageSys.getInstance().changeValueFont(num3, out after);
										mBText.mbSetBufferMsg(after, decWidth: false);
										if (num3 > num4)
										{
											mBText.mbSetTextColor(dgs.TXT_COLOR.TXT_COLOR_GREEN);
										}
										else if (num3 < num4)
										{
											mBText.mbSetTextColor(dgs.TXT_COLOR.TXT_COLOR_YELLOW);
										}
									}
									nodeByIDFromChildren = M.getNodeByID(TRANSCODE("AGI"));
									nodeByIDFromChildren = nodeByIDFromChildren.childNode();
									nodeByIDFromChildren = nodeByIDFromChildren.nextSibling();
									mBText = (menu.MBText)nodeByIDFromChildren.behavior().queryInterface(menu.MBText.classIdentifier());
									if (mBText != null)
									{
										int num3 = player.bodyAndBonus().dexterity().get();
										int num4 = player.body().dexterity().get();
										dgs.msg.CMessageSys.getInstance().changeValueFont(num3, out after);
										mBText.mbSetBufferMsg(after, decWidth: false);
										if (num3 > num4)
										{
											mBText.mbSetTextColor(dgs.TXT_COLOR.TXT_COLOR_GREEN);
										}
										else if (num3 < num4)
										{
											mBText.mbSetTextColor(dgs.TXT_COLOR.TXT_COLOR_YELLOW);
										}
									}
									nodeByIDFromChildren = M.getNodeByID(TRANSCODE("VIT"));
									nodeByIDFromChildren = nodeByIDFromChildren.childNode();
									nodeByIDFromChildren = nodeByIDFromChildren.nextSibling();
									mBText = (menu.MBText)nodeByIDFromChildren.behavior().queryInterface(menu.MBText.classIdentifier());
									if (mBText != null)
									{
										int num3 = player.bodyAndBonus().vitality().get();
										int num4 = player.body().vitality().get();
										dgs.msg.CMessageSys.getInstance().changeValueFont(num3, out after);
										mBText.mbSetBufferMsg(after, decWidth: false);
										if (num3 > num4)
										{
											mBText.mbSetTextColor(dgs.TXT_COLOR.TXT_COLOR_GREEN);
										}
										else if (num3 < num4)
										{
											mBText.mbSetTextColor(dgs.TXT_COLOR.TXT_COLOR_YELLOW);
										}
									}
									nodeByIDFromChildren = M.getNodeByID(TRANSCODE("INT"));
									nodeByIDFromChildren = nodeByIDFromChildren.childNode();
									nodeByIDFromChildren = nodeByIDFromChildren.nextSibling();
									mBText = (menu.MBText)nodeByIDFromChildren.behavior().queryInterface(menu.MBText.classIdentifier());
									if (mBText != null)
									{
										int num3 = player.bodyAndBonus().intelligence().get();
										int num4 = player.body().intelligence().get();
										dgs.msg.CMessageSys.getInstance().changeValueFont(num3, out after);
										mBText.mbSetBufferMsg(after, decWidth: false);
										if (num3 > num4)
										{
											mBText.mbSetTextColor(dgs.TXT_COLOR.TXT_COLOR_GREEN);
										}
										else if (num3 < num4)
										{
											mBText.mbSetTextColor(dgs.TXT_COLOR.TXT_COLOR_YELLOW);
										}
									}
									nodeByIDFromChildren = M.getNodeByID(TRANSCODE("MND"));
									nodeByIDFromChildren = nodeByIDFromChildren.childNode();
									nodeByIDFromChildren = nodeByIDFromChildren.nextSibling();
									mBText = (menu.MBText)nodeByIDFromChildren.behavior().queryInterface(menu.MBText.classIdentifier());
									if (mBText != null)
									{
										int num3 = player.bodyAndBonus().mind().get();
										int num4 = player.body().mind().get();
										dgs.msg.CMessageSys.getInstance().changeValueFont(num3, out after);
										mBText.mbSetBufferMsg(after, decWidth: false);
										if (num3 > num4)
										{
											mBText.mbSetTextColor(dgs.TXT_COLOR.TXT_COLOR_GREEN);
										}
										else if (num3 < num4)
										{
											mBText.mbSetTextColor(dgs.TXT_COLOR.TXT_COLOR_YELLOW);
										}
									}
									nodeByIDFromChildren = M.getNodeByID(TRANSCODE("ATK"));
									nodeByIDFromChildren = nodeByIDFromChildren.childNode();
									nodeByIDFromChildren = nodeByIDFromChildren.nextSibling();
									mBText = (menu.MBText)nodeByIDFromChildren.behavior().queryInterface(menu.MBText.classIdentifier());
									if (mBText != null)
									{
										int num5 = player.handAttack(pl.HAND_TYPE.RIGHT_HAND).aggressivity().get();
										int num6 = player.handAttack(pl.HAND_TYPE.LEFT_HAND).aggressivity().get();
										int num7 = num5 + num6;
										if (player.condition().isFrog() || player.condition().isLilliput())
										{
											num7 = ((num7 > 0) ? 1 : 0);
										}
										dgs.msg.CMessageSys.getInstance().changeValueFont(num7, out after);
										mBText.mbSetBufferMsg(after, decWidth: false);
									}
									nodeByIDFromChildren = M.getNodeByID(TRANSCODE("DEF"));
									nodeByIDFromChildren = nodeByIDFromChildren.childNode();
									nodeByIDFromChildren = nodeByIDFromChildren.nextSibling();
									mBText = (menu.MBText)nodeByIDFromChildren.behavior().queryInterface(menu.MBText.classIdentifier());
									if (mBText != null)
									{
										int num3 = player.physicsDefense().phylacticPower().get();
										if (player.condition().isFrog() || player.condition().isLilliput())
										{
											num3 = ((num3 > 0) ? 1 : 0);
										}
										dgs.msg.CMessageSys.getInstance().changeValueFont(num3, out after);
										mBText.mbSetBufferMsg(after, decWidth: false);
									}
									nodeByIDFromChildren = M.getNodeByID(TRANSCODE("MDF"));
									nodeByIDFromChildren = nodeByIDFromChildren.childNode();
									nodeByIDFromChildren = nodeByIDFromChildren.nextSibling();
									mBText = (menu.MBText)nodeByIDFromChildren.behavior().queryInterface(menu.MBText.classIdentifier());
									if (mBText != null)
									{
										int num3 = player.magicDefensePower();
										dgs.msg.CMessageSys.getInstance().changeValueFont(num3, out after);
										mBText.mbSetBufferMsg(after, decWidth: false);
									}
								}

								public override void bmBehave(menu.Medget M)
								{
									if (playerMCount != 1)
									{
										if ((ds.g_Pad.edge() & 0x200) != 0 || CWMenuManager.Instance().GetMenuButton().TouchButtonL())
										{
											slideDir = -1;
											slideTime = 0;
											menu.MenuManager.getSingleton().playSEMoveCursor();
											menu.MenuManager.getSingleton().inputPermission(b: false);
										}
										else if ((ds.g_Pad.edge() & 0x100) != 0 || CWMenuManager.Instance().GetMenuButton().TouchButtonR())
										{
											slideDir = 1;
											slideTime = 0;
											menu.MenuManager.getSingleton().playSEMoveCursor();
											menu.MenuManager.getSingleton().inputPermission(b: false);
										}
									}
									else if ((ds.g_Pad.edge() & 0x200) != 0 || CWMenuManager.Instance().GetMenuButton().TouchButtonL())
									{
										menu.MenuManager.getSingleton().playSEBeep();
									}
									else if ((ds.g_Pad.edge() & 0x100) != 0 || CWMenuManager.Instance().GetMenuButton().TouchButtonR())
									{
										menu.MenuManager.getSingleton().playSEBeep();
									}
								}

								public override void bmFinalize(menu.Medget M)
								{
								}

								public CWMenuStatus()
									: base("MBStatus")
								{
								}

								public new static int classIdentifier()
								{
									dgs.UniqueNumber uniqueNumber = new dgs.UniqueNumber();
									return uniqueNumber.number();
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
