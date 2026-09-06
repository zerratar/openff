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
	public static partial class wld
	{
							public class CWorld2DManager
							{
								private int m_wldMode;

								private CMenuButton m_MenuStartButton = new CMenuButton();

								private CMenuButton m_CameraButton = new CMenuButton();

								private CMenuButton m_TalkButton = new CMenuButton();

								private CItemUseMenuManager m_ItemUseMenuManager = new CItemUseMenuManager();

								private COnePicture m_OnePicture = new COnePicture();

								private CMessageWindow m_MessageWindow = new CMessageWindow();

								private menu.MapNameWindow m_MapNameWindow = new menu.MapNameWindow();

								private WorldMap m_WorldMap = new WorldMap();

								private int m_MapMarkerPlayer;

								private CConfirmWindow m_ConfirmWindow;

								private CMoneyWindow m_MoneyWindow;

								private sys2d.Sprite3d[] m_PadButton = new sys2d.Sprite3d[2];

								private sys2d.Sprite3d m_TalkIcon = new sys2d.Sprite3d();

								private bool m_visibleMap;

								public CWorld2DManager()
								{
									for (int i = 0; i < m_PadButton.Length; i++)
									{
										m_PadButton[i] = new sys2d.Sprite3d();
									}
									m_wldMode = -1;
									m_MapMarkerPlayer = -1;
								}

								public void initialize()
								{
									m_wldMode = -1;
									m_MapMarkerPlayer = -1;
									m_visibleMap = false;
									m_ItemUseMenuManager.setup();
									int num = atoi(sceneMng.getStage().Substring(1, 2));
									bool flag = true;
									switch (num)
									{
									case 5:
									case 7:
									case 10:
									case 11:
									case 14:
									case 15:
									case 16:
									case 25:
									case 27:
									case 28:
									case 30:
									case 31:
										flag = false;
										break;
									}
									if (sceneMng.getStage()[0] == 'f' || (flag && sceneMng.getStage()[0] == 't' && sceneMng.getStage()[4] == '0' && sceneMng.getStage()[5] == '1'))
									{
										changeGlobalDirectory();
										m_visibleMap = true;
										ds.CVram.setSubPlaneVisiblity(bg0: false, bg1: false, bg2: false, bg3: true, obj: true);
									}
									else
									{
										ds.CVram.setSubPlaneVisiblity(bg0: true, bg1: false, bg2: false, bg3: false, obj: false);
										G3X_SetClearColor(GX_RGB(0, 0, 0), 31, 32767, 1, 0);
									}
									m_MessageWindow.setup();
									for (int i = 0; i < 2; i++)
									{
										m_PadButton[i].Release();
										m_PadButton[i].Load2(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN3D, "pad");
										m_PadButton[i].SetCell((ushort)i);
										m_PadButton[i].SetDepth(1 - i);
										m_PadButton[i].SetShow(show: false);
									}
									sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(m_PadButton[1]);
									sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(m_PadButton[0]);
									m_TalkIcon.Release();
									m_TalkIcon.Load2(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN3D, "fukidashi");
									m_TalkIcon.SetCell(0);
									m_TalkIcon.SetShow(show: false);
									sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(m_TalkIcon);
								}

								public void execute()
								{
									m_MenuStartButton.execute(this);
									m_CameraButton.execute(this);
									m_TalkButton.execute(this);
									m_ItemUseMenuManager.execute();
									m_MessageWindow.execute();
									m_WorldMap.updateWMap();
									m_MapNameWindow.countdownToClose();
								}

								public void terminate()
								{
									m_MenuStartButton.erase();
									m_CameraButton.erase();
									m_TalkButton.erase();
									m_ItemUseMenuManager.cleanup();
									m_OnePicture.cleanup();
									m_MessageWindow.cleanup();
									for (int i = 0; i < 2; i++)
									{
										m_PadButton[i].Release();
										sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(m_PadButton[i]);
										NNS_G2dReleaseImageProxy(m_PadButton[i].GetImageProxy());
									}
									m_TalkIcon.Release();
									sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(m_TalkIcon);
									NNS_G2dReleaseImageProxy(m_TalkIcon.GetImageProxy());
								}

								public void setShowOnePicture(bool b)
								{
									if (b)
									{
										ds.CVram.setSubPlaneVisiblity(bg0: false, bg1: false, bg2: false, bg3: true, obj: true);
									}
									else
									{
										ds.CVram.setSubPlaneVisiblity(bg0: true, bg1: false, bg2: false, bg3: false, obj: false);
									}
								}

								public void createButton()
								{
									MenuStartButton().setup(CMenuButton.MENU_2D3D.MENU_3D, CMenuButton.MENU_POSITION.MENU_POSITION_RIGHT_UP, CMenuButton.MENU_PANEL_ANIM.MENU_CALL_BUTTON, 64, 40);
									CameraButton().setup(CMenuButton.MENU_2D3D.MENU_3D, CMenuButton.MENU_POSITION.MENU_POSITION_LEFT_UP, CMenuButton.MENU_PANEL_ANIM.MENU_CAMERA_BUTTON, 64, 40);
									TalkButton().setup(CMenuButton.MENU_2D3D.MENU_3D, CMenuButton.MENU_POSITION.MENU_POSITION_RIGHT_DOWN, CMenuButton.MENU_PANEL_ANIM.MENU_TALK_BUTTON, 64, 40);
									opt.MENU_ZOOM_SETTING setting = opt.COptionManager.getSingleton().gameOption().menuZoomSetting();
									switchMenuCameraButton(setting);
								}

								public void switchMenuCameraButton(opt.MENU_ZOOM_SETTING setting)
								{
									if (setting == opt.MENU_ZOOM_SETTING.MENU_R_ZOOM_L)
									{
										MenuStartButton().setPosition(CMenuButton.MENU_POSITION.MENU_POSITION_RIGHT_UP);
										CameraButton().setPosition(CMenuButton.MENU_POSITION.MENU_POSITION_LEFT_UP);
									}
									else
									{
										MenuStartButton().setPosition(CMenuButton.MENU_POSITION.MENU_POSITION_LEFT_UP);
										CameraButton().setPosition(CMenuButton.MENU_POSITION.MENU_POSITION_RIGHT_UP);
									}
								}

								public void setButtonShow(bool show)
								{
									MenuStartButton().setShow(show);
									CameraButton().setShow(show);
									TalkButton().setShow(show);
								}

								public CMenuButton MenuStartButton()
								{
									return m_MenuStartButton;
								}

								public CMenuButton CameraButton()
								{
									return m_CameraButton;
								}

								public CMenuButton TalkButton()
								{
									return m_TalkButton;
								}

								public CItemUseMenuManager ItemUseMenuManager()
								{
									return m_ItemUseMenuManager;
								}

								public COnePicture OnePicture()
								{
									return m_OnePicture;
								}

								public CMessageWindow MessageWindow()
								{
									return m_MessageWindow;
								}

								public menu.MapNameWindow refMapNameWindow()
								{
									return m_MapNameWindow;
								}

								public WorldMap refWorldMap()
								{
									return m_WorldMap;
								}

								public CConfirmWindow refConfirmWindow()
								{
									// PORT: made on first use; nothing in the phone build made one.
									if (m_ConfirmWindow == null)
									{
										m_ConfirmWindow = new CConfirmWindow();
									}
									return m_ConfirmWindow;
								}

								public CMoneyWindow refMoneyWindow()
								{
									return m_MoneyWindow;
								}

								public sys2d.Sprite3d[] PadButton()
								{
									return m_PadButton;
								}

								public sys2d.Sprite3d refTalkIcon()
								{
									return m_TalkIcon;
								}

								public bool visibleMap()
								{
									return m_visibleMap;
								}

								public void setWldMode(int wldMode)
								{
									m_wldMode = wldMode;
								}
							}
	}
}
