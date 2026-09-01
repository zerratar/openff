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
						public class NameEntry : wld.CParallelWorldSystem
						{
							public static NameEntry instance_ = new NameEntry();

							private static string[] filename = new string[4] { "profile0", "profile1", "profile2", "profile3" };

							private sys2d.Bg bg_ = new sys2d.Bg();

							private sys2d.Bg profile_ = new sys2d.Bg();

							private sys2d.Cell name_cell_ = new sys2d.Cell();

							private sys2d.Cell ok_cell_ = new sys2d.Cell();

							private dgs.SmartPtr<dgs.DGSMessage> profile_str_;

							private dgs.SmartPtr<dgs.DGSMessage> name_length_str_;

							private dgs.SmartPtr<dgs.DGSMessage> name_str_;

							private pl.PLAYER_ID currentPlayerID_;

							private int step_;

							public NameEntry()
							{
								currentPlayerID_ = pl.PLAYER_ID.PLAYER_1;
								endFlag_ = false;
								step_ = 0;
							}

							public override void initialize(wld.CBaseSystem BS)
							{
								_flipScreen = GX_GetFlipScreen() != 0;
								evt.CEventManager.getInstance().setEventStop(_EventStop: true);
								endFlag_ = false;
								step_ = 0;
								dgs.CFade.Main().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
								dgs.CFade.Sub().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
								BS.World2DMng().refWorldMap().hideMapMarker();
							}

							public override void terminate(wld.CBaseSystem BS)
							{
								menu.MenuManager.getSingleton().SetActivateButtonState(1);
								menu.MenuManager.getSingleton().ClearBehaviorButton();
								sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(name_cell_);
								name_cell_.Release();
								NNS_G2dReleaseImageProxy(name_cell_.GetImageProxy());
								sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(ok_cell_);
								ok_cell_.Release();
								NNS_G2dReleaseImageProxy(ok_cell_.GetImageProxy());
								profile_str_.release();
								name_length_str_.release();
								name_str_.release();
								evt.CEventManager.getInstance().setEventStop(_EventStop: false);
								GX_FixScreen(_flipScreen ? 1 : 0);
							}

							public override bool execute(wld.CBaseSystem BS)
							{
								switch (step_)
								{
								case 0:
									if (dgs.CFade.Main().isFaded() && dgs.CFade.Sub().isFaded())
									{
										GX_FixScreen(0);
										GX_Power3D(0);
										TexDivideLoader.getSingleton().tdlCancel();
										GX_SetDispSelect(GXDispSelect.GX_DISP_SELECT_MAIN_SUB);
										BS.World2DMng().terminate();
										changeCompanyDirectory();
										menu.MenuManager.getSingleton().ReleaseXbnFile();
										menu.MenuManager.getSingleton().LoadXbnFile("NameEntry.xbn");
										menu.MenuManager.getSingleton().Set2d3dMode(2);
										menu.MenuManager.getSingleton().CreateNeedObject(menu.MenuManager.MENU_CREATE_PARAMETER.CREATE_MENU_MODE_2D);
										menu.MenuManager.getSingleton().SetUsingMenuType(1);
										changeCompanyDirectory();
										menu.MenuManager.getSingleton().CreateMenuDataText(0);
										menu.MenuManager.getSingleton().buildMenu("name_entry");
										menu.MenuManager.getSingleton().GetCursor2d().SetPositionI(36, 190);
										changeCompanyDirectory();
										profile_.bgLoad2(const_cast<string>(filename[(int)getSingleton().currentPlayerID_]));
										profile_.bgSetUp(NNSG2dBGSelect.NNS_G2D_BGSELECT_MAIN2);
										profile_.bgRelease();
										profile_.bgSetShow(show: true);
										changeGlobalDirectory();
										name_cell_.Load(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_SUB2D, "name_i.NCER", null, "name_i.NCGR", "name_i.NCLR");
										name_cell_.SetCell(0);
										name_cell_.SetShow(show: true);
										name_cell_.SetPositionI(18, 150);
										sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(name_cell_);
										ok_cell_.Load(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_SUB2D, "ok.NCER", null, "ok.NCGR", "ok.NCLR");
										ok_cell_.SetCell(0);
										ok_cell_.SetShow(show: true);
										ok_cell_.SetPositionI(150, 266);
										sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(ok_cell_);
										dgs.DGSMessageManager dGSMessageManager2 = dgs.msg.CMessageSys.getInstance().Main();
										profile_str_ = new dgs.SmartPtr<dgs.DGSMessage>(dGSMessageManager2.createMessage((uint)(50050 + getSingleton().currentPlayerID_), dgs.INVALID_MSDHANDLE, 1));
										profile_str_.get().setVSpace((OS_GetLanguage() != 3) ? 5 : 4);
										profile_str_.get().setPosition(16, (short)(isIPad() ? (-8) : 8), erase: true);
										profile_str_.get().setDisplaySpeed(byte.MaxValue);
										profile_str_.get().setDisplayWait(0);
										profile_str_.get().setMessageColor(dgs.TXT_COLOR.TXT_COLOR_BLACK);
										profile_str_.get().setShadow(b: false);
										name_length_str_ = new dgs.SmartPtr<dgs.DGSMessage>(dGSMessageManager2.createMessage(50054u, dgs.INVALID_MSDHANDLE, 1));
										name_length_str_.get().setVSpace(5);
										name_length_str_.get().setPosition(18, 230, erase: true);
										name_length_str_.get().setStyle(520u);
										name_length_str_.get().setDisplaySpeed(byte.MaxValue);
										name_length_str_.get().setDisplayWait(0);
										name_length_str_.get().setMessageColor(dgs.TXT_COLOR.TXT_UCOLOR_4);
										name_length_str_.get().setShadow(b: false);
										strcpy(out name_, pl.PlayerParty.instance().playerForId((byte)getSingleton().currentPlayerID_).name());
										name_str_ = new dgs.SmartPtr<dgs.DGSMessage>(dGSMessageManager2.createMessage(name_, 0));
										name_str_.get().setPosition(45, 187, erase: true);
										name_str_.get().setStyle(520u);
										name_str_.get().setDisplaySpeed(byte.MaxValue);
										name_str_.get().setDisplayWait(0);
										name_str_.get().setMessageColor(dgs.TXT_COLOR.TXT_COLOR_BLACK);
										name_str_.get().setShadow(b: false);
										ds.CVram.setSubPlaneVisiblity(bg0: true, bg1: false, bg2: true, bg3: false, obj: true);
										dgs.CFade.Main().fadeIn(15);
										dgs.CFade.Sub().fadeIn(15);
										step_ = 1;
									}
									break;
								case 1:
									if (dgs.CFade.Main().isCleared() && dgs.CFade.Sub().isCleared())
									{
										step_ = 2;
									}
									break;
								case 2:
								{
									ds.g_TouchPanel.getPoint(out var x, out var y);
									if (!endFlag_ && ds.g_TouchPanel.isRelease() && x >= 12 && x < 180 && y >= 150 && y < 230)
									{
										menu.MenuManager.getSingleton().playSEDecide();
										MainActivity.createEditText(name_);
										break;
									}
									if (!endFlag_)
									{
										string editText = MainActivity.getEditText();
										if (editText != null)
										{
											strcpy(out name_, editText);
											name_str_.release();
											dgs.DGSMessageManager dGSMessageManager = dgs.msg.CMessageSys.getInstance().Main();
											name_str_ = new dgs.SmartPtr<dgs.DGSMessage>(dGSMessageManager.createMessage(name_, 0));
											name_str_.get().setPosition(45, 187, erase: true);
											name_str_.get().setStyle(520u);
											name_str_.get().setDisplaySpeed(byte.MaxValue);
											name_str_.get().setDisplayWait(0);
											name_str_.get().setMessageColor(dgs.TXT_COLOR.TXT_COLOR_BLACK);
											name_str_.get().setShadow(b: false);
										}
									}
									bool flag = x >= 150 && x < 249 && y >= 266 && y < 306;
									ok_cell_.SetColor((ds.g_TouchPanel.isTouch() && flag) ? 12632256u : 16777215u);
									if (!endFlag_ && ds.g_TouchPanel.isRelease() && flag)
									{
										if (name_.Length != 0)
										{
											menu.MenuManager.getSingleton().playSEDecide();
											endFlag_ = true;
										}
										else
										{
											menu.MenuManager.getSingleton().playSEBeep();
										}
									}
									menu.MenuManager.getSingleton().execute();
									if (endFlag_)
									{
										dgs.CFade.Main().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
										dgs.CFade.Sub().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
										step_ = 3;
									}
									break;
								}
								case 3:
									if (dgs.CFade.Main().isFaded() && dgs.CFade.Sub().isFaded())
									{
										GX_Power3D(1);
										bg_.bgSetShow(show: false);
										profile_.bgSetShow(show: false);
										pl.PlayerParty.instance().playerForId((byte)currentPlayerID_).setName(name_);
										changeGlobalDirectory();
										menu.MenuManager.getSingleton().DeleteNeedObject(menu.MenuManager.MENU_CREATE_PARAMETER.CREATE_MENU_MODE_3D);
										menu.MenuManager.getSingleton().release();
										menu.MenuManager.getSingleton().ReleaseMenuDataText();
										menu.MenuManager.getSingleton().ReleaseXbnFile();
										menu.MenuManager.getSingleton().LoadXbnFile("WorldDefine.xbn");
										GX_SetDispSelect(GXDispSelect.GX_DISP_SELECT_SUB_MAIN);
										dgs.CFade.Main().fadeIn(15);
										dgs.CFade.Sub().fadeIn(15);
										BS.World2DMng().initialize();
										BS.World2DMng().setShowOnePicture(b: false);
										step_ = 4;
									}
									break;
								case 4:
									if (dgs.CFade.Main().isCleared())
									{
										dgs.CFade.Sub().isCleared();
									}
									return false;
								}
								return true;
							}

							public static NameEntry getSingleton()
							{
								return instance_;
							}

							public void setTargetPlayer(pl.PLAYER_ID ID)
							{
								currentPlayerID_ = ID;
							}
						}
}
