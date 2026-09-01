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
							public const int WMEM_COMMAND_SELECT = 0;

							public const int WMEM_SLOT_SELECT = 1;

							public const int WMEM_WAIT_WINDOW = 2;

							public const int WMEM_ITEM_SELECT = 3;

							public const int WMEM_MODE_EQUIP = 0;

							public const int WMEM_MODE_REMOVE = 1;

							public const int EQUIP_REFRESH = 1;

							public const int USING = 0;

							public const int SEITON = 1;

							public const int IMPORTANT = 2;

							public const int USE_FIRST_SELECT = 0;

							public const int USE_SECOND_SELECT = 1;

							public const int USE_DOUBLE_DECIDE_ITEM = 2;

							public const int USE_MASTER_CARD_START_FADEOUT = 3;

							public const int USE_MASTER_CARD_START_FADEIN = 4;

							public const int USE_MASTER_CARD = 5;

							public const int USE_MASTER_CARD_END_FADEOUT = 6;

							public const int USE_MASTER_CARD_END_FADEIN = 7;

							public const int SEITON_START = 0;

							public const int SEITON_LIST_REFRESH = 1;

							public const int SEITON_END = 2;

							public const int USE_WINDOW_WAIT = 0;

							public const int USE_PROCESS = 1;

							public const int USE_WINDOW_END = 2;

							public const int USE_ERR_STATE = 3;

							public const int PROCESS_END = 0;

							public const int PROCESS_ERR = 1;

							public const int PROCESS_CONTINUE = 2;

							public const int ERR_OVER_LEARNING = 0;

							public const int ERR_DOUBLE_LEARNING = 1;

							public const int LEARNING = 1;

							public const int REMOVE = 2;

							public const int CHANGE = 3;

							public const int REMOVE_PROCESS_MAIN = 0;

							public const int REMOVE_PROCESS_WINDOW_START = 1;

							public const int REMOVE_PROCESS_WINDOW_WAIT = 2;

							public const int REMOVE_PROCESS_WINDOW_END = 3;

							public const int CHANGE_PROCESS_WINDOW_CREATE = 0;

							public const int CHANGE_PROCESS_WINDOW_START = 1;

							public const int CHANGE_PROCESS_MAIN = 2;

							public const int CHANGE_PROCESS_WINDOW_END = 3;

							public const int CHANGE_PROCESS_UPDATE_MESSAGE = 4;

							public const int CHANGE_PROCESS_MAIN_MESSAGE = 5;

							public const int CHANGE_PROCESS_END_MESSAGE = 6;

							public const int LEARNING_PROCESS_START = 0;

							public const int LEARNING_PROCESS_WINDOW_CREATE = 1;

							public const int LEARNING_PROCESS_WINDOW_WAIT = 2;

							public const int LEARNING_PROCESS_MAIN = 3;

							public const int LEARNING_PROCESS_WINDOW_END = 4;

							public const int LEARNING_ERR = 5;

							public const int USE_WAIT = 0;

							public const int USE_CREATE = 1;

							public const int USE_IMPOSIBLE = 2;

							public const int USE_DELETE = 3;

							public static int Hit_Y = 288;

							public static int Hit_Height = 32;

							public static sbyte[] cellType = new sbyte[4] { 25, 8, 5, 6 };

							public static int[][] Cell_Pos = new int[4][]
							{
								new int[3] { 240, 288, 112 },
								new int[3] { 240, 288, 112 },
								new int[3] { 64, 288, 64 },
								new int[3] { 416, 288, 64 }
							};

							private static int[] itemNum_ = new int[2];

							private static int drawCounter_;

							private static int mode = 0;

							private static int JOB_MODEL_X = 0;

							private static int JOB_MODEL_Y = 40960000;

							private static int JOB_MODEL_Z = 0;

							private static int CAMERA_POS_OFFSET_X = 0;

							private static int CAMERA_POS_OFFSET_Y = 73728;

							private static int CAMERA_POS_OFFSET_Z = 114688;

							private static int CAMERA_TAR_X = 0;

							private static int CAMERA_TAR_Y = -65536;

							private static int CAMERA_TAR_Z = -176128;

							public static int USE_COMMAND_FIRST_TAG_NO = 50;

							private static byte[][][] PCF_SCREEN_DATA = new byte[4][][]
							{
								new byte[4][]
								{
									new byte[4] { 64, 65, 66, 67 },
									new byte[4] { 80, 81, 82, 83 },
									new byte[4] { 96, 97, 98, 99 },
									new byte[4] { 112, 113, 114, 115 }
								},
								new byte[4][]
								{
									new byte[4] { 68, 69, 70, 71 },
									new byte[4] { 84, 85, 86, 87 },
									new byte[4] { 100, 101, 102, 103 },
									new byte[4] { 116, 117, 118, 119 }
								},
								new byte[4][]
								{
									new byte[4] { 72, 73, 74, 75 },
									new byte[4] { 88, 89, 90, 91 },
									new byte[4] { 104, 105, 106, 107 },
									new byte[4] { 120, 121, 122, 123 }
								},
								new byte[4][]
								{
									new byte[4] { 76, 77, 78, 79 },
									new byte[4] { 92, 93, 94, 95 },
									new byte[4] { 108, 109, 110, 111 },
									new byte[4] { 124, 125, 126, 127 }
								}
							};

							internal static void setCitizenVisibility(bool bVisibility)
							{
								pl.CPlayerManager cPlayerManager = wld.WorldPart.getInstance().getWorldSystem().PlayerMng();
								if (cPlayerManager != null)
								{
									for (int i = 0; pl.FIELD_CHARACTER_NUM > i; i++)
									{
										cPlayerManager.Player(i).setHidden(!bVisibility);
									}
								}
							}

							internal static void setPlayerVisibility(int playerID, bool bVisibility)
							{
								wld.WorldPart.getInstance().getWorldSystem().PlayerMng()
									.Player(playerID)
									.setHidden(!bVisibility);
							}

							internal static int getTopPlayerId()
							{
								for (byte b = 0; b < 4; b++)
								{
									if (pl.PlayerParty.instance().player(b).isEnable() && !pl.PlayerParty.instance().player(b).condition()
										.isDeath() && !pl.PlayerParty.instance().player(b).condition()
										.isStone())
									{
										return pl.PlayerParty.instance().player(b).playerId();
									}
								}
								return -1;
							}

							internal static void cameraDebug(ds.sys3d.CCamera Camera)
							{
							}

							internal static void cameraSettingDebug(ds.sys3d.CCamera Camera)
							{
							}

							internal static void backupAccessFailedSetting()
							{
								menu.Medget nodeByID = menu.MenuManager.getSingleton().root().getNodeByID("suspend_message_2");
								if (nodeByID != null && nodeByID.behavior() != null)
								{
									menu.MBText mBText = (menu.MBText)nodeByID.behavior().queryInterface(menu.MBText.classIdentifier());
									if (mBText != null && mBText.getMessage() != null)
									{
										mBText.getMessage().setStyle(1024u);
										mBText.getMessage().setVSpace(4);
									}
								}
								ds.CVram.setSubPlaneVisiblity(bg0: true, bg1: false, bg2: false, bg3: true, obj: false);
								CWMenuManager.Instance().GetMenuButton().SetButtonAActivity(b: false);
								CWMenuManager.Instance().GetMenuButton().SetButtonBActivity(b: false);
								CWMenuManager.Instance().GetMenuButton().SetButtonLActivity(b: false);
								CWMenuManager.Instance().GetMenuButton().SetButtonRActivity(b: false);
								CWMenuManager.Instance().SetPrimaryBG(10);
								CWMenuManager.Instance().SetPrimaryBGVisibility(b: true);
								CWMenuManager.Instance().SetSecondlyBGVisibility(b: false);
								ds.Sound.Stop();
							}

							internal static void messageCentering(string ID)
							{
								menu.Medget nodeByID = menu.MenuManager.getSingleton().root().getNodeByID(TRANSCODE(ID));
								if (nodeByID != null && nodeByID.behavior() != null)
								{
									menu.MBText mBText = (menu.MBText)nodeByID.behavior().queryInterface(menu.MBText.classIdentifier());
									if (mBText != null && mBText.getMessage() != null)
									{
										mBText.getMessage().setVSpace(4);
										mBText.getMessage().setStyle(1024u);
									}
								}
							}

	}
}
