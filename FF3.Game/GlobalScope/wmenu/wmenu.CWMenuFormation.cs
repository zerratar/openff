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
							public class CWMenuFormation : CWMenuMemberBase
							{
								private int formationState;

								private int prevCharNo;

								private int prevCursorTag;

								private dgs.DGSMessage[] pMsg = new dgs.DGSMessage[20];

								public override bool cSelectInitialize()
								{
									return true;
								}

								public override void initialize()
								{
									for (int i = 0; i < pMsg.Length; i++)
									{
										pMsg[i] = null;
									}
								}

								public override void run()
								{
								}

								public override void terminate()
								{
									CWMenuManager.Instance().ResetFormationCursor();
									for (int i = 0; i < LENGTH(pMsg); i++)
									{
										if (pMsg[i] != null)
										{
											pMsg[i].release();
											pMsg[i] = null;
										}
									}
								}

								public void Decide()
								{
									if (formationState == 0)
									{
										if (!pl.PlayerParty.instance().player((byte)(sbyte)menu.MenuManager.getSingleton().getFocuseMedget().work()).isEnable())
										{
											menu.MenuManager.getSingleton().playSEBeep();
											return;
										}
										CWMenuManager.Instance().SetUpFormationCursor(menu.MenuManager.getSingleton().getFocuseMedget().cursorX(), menu.MenuManager.getSingleton().getFocuseMedget().cursorY() + 2, act: true);
										prevCursorTag = menu.MenuManager.getSingleton().getFocuseMedget().myTag();
										prevCharNo = (sbyte)menu.MenuManager.getSingleton().getFocuseMedget().work();
										menu.MenuManager.getSingleton().playSEDecide();
										formationState = 1;
									}
									else
									{
										if (formationState != 1)
										{
											return;
										}
										if (prevCharNo == (sbyte)menu.MenuManager.getSingleton().getFocuseMedget().work())
										{
											NNSG2dSVec2 nNSG2dSVec = CWMenuManager.Instance().GetPcFace().pcfmGetPosition(pl.PlayerParty.instance().player((byte)prevCharNo).playerId());
											pl.PlayerParty.instance().player((byte)prevCharNo).changeFormationType();
											if (pl.PlayerParty.instance().player((byte)prevCharNo).formationType() == 0)
											{
												nNSG2dSVec.x = 1;
											}
											else
											{
												nNSG2dSVec.x = 3;
											}
											CWMenuManager.Instance().SetCharScrMovement(nNSG2dSVec.x, nNSG2dSVec.y, prevCharNo);
										}
										else
										{
											int num = pl.PlayerParty.instance().player((byte)prevCharNo).playerId();
											int num2 = pl.PlayerParty.instance().player((byte)(sbyte)menu.MenuManager.getSingleton().getFocuseMedget().work()).playerId();
											pl.PlayerParty.instance().changePlayer((byte)num, (byte)num2);
											menu.Medget nodeByID = menu.MenuManager.getSingleton().GetBaseMedget().getNodeByID(TRANSCODE("char_status"));
											if (nodeByID != null)
											{
												((menu.MBStatus)nodeByID.behavior().queryInterface(menu.MBStatus.classIdentifier()))?.bmRefresh(nodeByID);
											}
											CWMenuManager.Instance().swapCharFirstPosition();
										}
										menu.MenuManager.getSingleton().playSEDecide();
										CWMenuManager.Instance().ResetFormationCursor();
										formationState = 0;
									}
								}

								public void Cancel()
								{
									NNSG2dSVec2 positionI = CWMenuManager.Instance().GetFormationCursor().GetPositionI();
									menu.MenuManager.getSingleton().GetCursor2d().SetPositionI(positionI.x, positionI.y);
									menu.MenuManager.getSingleton().initFocus(prevCursorTag);
									CWMenuManager.Instance().ResetFormationCursor();
									formationState = 0;
								}

								public void CSelectRun()
								{
									TPData dispPoint = ds.g_TouchPanel.getDispPoint();
									if (formationState != 2)
									{
										int num = (sbyte)menu.MenuManager.getSingleton().getFocuseMedget().work();
										if (ds.g_TouchPanel.isTouch() && abs(dispPoint.dragX - dispPoint.x) > 16 && pl.PlayerParty.instance().player((byte)num).isEnable() && (byte)((dispPoint.dragX <= dispPoint.x) ? 1 : 0) != pl.PlayerParty.instance().player((byte)num).formationType())
										{
											NNSG2dSVec2 nNSG2dSVec = CWMenuManager.Instance().GetPcFace().pcfmGetPosition(pl.PlayerParty.instance().player((byte)num).playerId());
											pl.PlayerParty.instance().player((byte)num).changeFormationType();
											if (pl.PlayerParty.instance().player((byte)num).formationType() == 0)
											{
												nNSG2dSVec.x = 1;
											}
											else
											{
												nNSG2dSVec.x = 3;
											}
											CWMenuManager.Instance().SetCharScrMovement(nNSG2dSVec.x, nNSG2dSVec.y, num);
											menu.MenuManager.getSingleton().playSEDecide();
											TP_CancelTap();
											if (formationState == 1)
											{
												CWMenuManager.Instance().ResetFormationCursor();
												formationState = 0;
											}
										}
										if (dispPoint.hold == 0 || !pl.PlayerParty.instance().player((byte)(sbyte)menu.MenuManager.getSingleton().getFocuseMedget().work()).isEnable())
										{
											return;
										}
										CWMenuManager.Instance().ResetFormationCursor();
										prevCursorTag = menu.MenuManager.getSingleton().getFocuseMedget().myTag();
										prevCharNo = (sbyte)menu.MenuManager.getSingleton().getFocuseMedget().work();
										menu.MenuManager.getSingleton().playSEDecide();
										int num2 = menu.MenuManager.getSingleton().getFocuseMedget().x() + 2 + 80 - 24;
										int num3 = menu.MenuManager.getSingleton().getFocuseMedget().y() + 2;
										ds.Vector2<short> vector = new ds.Vector2<short>();
										dgs.msg.CMessageMng.MSF_HANDLE_KIND font = dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_8x8;
										dgs.DGSMessageManager dGSMessageManager = dgs.msg.CMessageSys.getInstance().Sub();
										pMsg[0] = dGSMessageManager.createMessage(pl.PlayerParty.instance().player((byte)num).name(), (int)font);
										pMsg[0].setPosition((short)num2, (short)num3, erase: true);
										dgs.msg.CMessageSys.getInstance().changeValueFont(pl.PlayerParty.instance().player((byte)num).level()
											.get(), out var after);
										pMsg[1] = dGSMessageManager.createMessage(50414u, -1, (int)font);
										pMsg[1].setPosition((short)(num2 + 160), (short)num3, erase: true);
										pMsg[2] = dGSMessageManager.createMessage(after, (int)font);
										pMsg[2].getTextSize(vector);
										pMsg[2].setPosition((short)(num2 + 160 + 40 - vector.vx), (short)num3, erase: true);
										pMsg[3] = dGSMessageManager.createMessage((uint)(50105 + pl.PlayerParty.instance().player((byte)num).jobManager()
											.nowJob()), dgs.INVALID_MSDHANDLE, (int)font);
										pMsg[3].setPosition((short)(num2 + 74), (short)num3, erase: true);
										pMsg[4] = dGSMessageManager.createMessage(50415u, dgs.INVALID_MSDHANDLE, (int)font);
										pMsg[4].setPosition((short)(num2 + 64), (short)(num3 + 16), erase: true);
										pMsg[5] = dGSMessageManager.createMessage(50416u, dgs.INVALID_MSDHANDLE, (int)font);
										pMsg[5].setPosition((short)(num2 + 64), (short)(num3 + 16 + 16), erase: true);
										dgs.msg.CMessageSys.getInstance().changeValueFont(pl.PlayerParty.instance().player((byte)num).hp()
											.getNow(), out after);
										pMsg[6] = dGSMessageManager.createMessage(after, (int)font);
										pMsg[6].getTextSize(vector);
										pMsg[6].setPosition((short)(num2 + 128 - vector.vx), (short)(num3 + 16), erase: true);
										pMsg[7] = dGSMessageManager.createMessage(50418u, dgs.INVALID_MSDHANDLE, (int)font);
										pMsg[7].setPosition((short)(num2 + 128 + 3), (short)(num3 + 16), erase: true);
										dgs.msg.CMessageSys.getInstance().changeValueFont(pl.PlayerParty.instance().player((byte)num).hp()
											.getLimit(), out after);
										pMsg[8] = dGSMessageManager.createMessage(after, (int)font);
										pMsg[8].setPosition((short)(num2 + 128 + 11), (short)(num3 + 16), erase: true);
										for (int i = 0; i < 8; i++)
										{
											dgs.msg.CMessageSys.getInstance().changeValueFont(pl.PlayerParty.instance().player((byte)num).mp(i)
												.getNow(), out after);
											int num4;
											if (i != 3 && i != 7)
											{
												sprintf(out after, "%s /", after);
												num4 = 28;
											}
											else
											{
												num4 = 20;
											}
											pMsg[9 + i] = dGSMessageManager.createMessage(after, (int)font);
											pMsg[9 + i].getTextSize(vector);
											pMsg[9 + i].setPosition((short)(num2 + 90 + (i & 3) * 28 + num4 - vector.vx), (short)(num3 + 32 + i / 4 * 12), erase: true);
										}
										for (int j = 0; j < LENGTH(pMsg); j++)
										{
											if (pMsg[j] != null)
											{
												pMsg[j].setDisplaySpeed(byte.MaxValue);
												pMsg[j].setDisplayWait(0);
												pMsg[j].setStyle(pMsg[j].getStyle() | 0x1000);
											}
										}
										formationState = 2;
										return;
									}
									ds.g_TouchPanel.getPoint(out var x, out var y);
									menu.Medget nodeByID = menu.MenuManager.getSingleton().root().getNodeByID(TRANSCODE("char_select"));
									for (menu.Medget medget = nodeByID.childNode(); medget != null; medget = medget.nextSibling())
									{
										if (medget != menu.MenuManager.getSingleton().getFocuseMedget() && medget.x() < x && x <= medget.x() + medget.width() && medget.y() < y && y <= medget.y() + medget.height())
										{
											menu.MenuManager.getSingleton().initFocus(medget.myTag());
										}
									}
									if (ds.g_TouchPanel.isTouch())
									{
										return;
									}
									int num5 = pl.PlayerParty.instance().player((byte)prevCharNo).playerId();
									int num6 = pl.PlayerParty.instance().player((byte)(sbyte)menu.MenuManager.getSingleton().getFocuseMedget().work()).playerId();
									pl.PlayerParty.instance().changePlayer((byte)num5, (byte)num6);
									menu.Medget nodeByID2 = menu.MenuManager.getSingleton().GetBaseMedget().getNodeByID(TRANSCODE("char_status"));
									if (nodeByID2 != null)
									{
										((menu.MBStatus)nodeByID2.behavior().queryInterface(menu.MBStatus.classIdentifier()))?.bmRefresh(nodeByID2);
									}
									CWMenuManager.Instance().swapCharFirstPosition();
									menu.MenuManager.getSingleton().playSEDecide();
									formationState = 0;
									for (int k = 0; k < LENGTH(pMsg); k++)
									{
										if (pMsg[k] != null)
										{
											pMsg[k].release();
											pMsg[k] = null;
										}
									}
								}

								public int GetFormationState()
								{
									return formationState;
								}

								public void SetFormationState(int val)
								{
									formationState = val;
								}
							}
	}
}
