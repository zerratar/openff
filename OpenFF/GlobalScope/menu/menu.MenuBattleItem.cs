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
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
	public static partial class menu
	{
		public class MenuBattleItem
		{
			public enum USING_TYPE
			{
				USING_ITEM,
				USING_WEAPON
			}

			public enum USING_ITEM_TYPE
			{
				U_LOCAL_NORMAL,
				U_LOCAL_WEAPON
			}

			public enum ITEM_SELECT_STATE
			{
				W_LOCAL_CHOICE,
				W_LOCAL_DECIDE
			}

			public class TOP_SPRITE
			{
				public int index;

				public bool bEnable;

				public sys2d.Sprite3d sprite = new sys2d.Sprite3d();

				public ds.Vector2<short> pos = new ds.Vector2<short>();

				public ds.Vector2<short> size;
			}

			public const USING_TYPE USING_ITEM = USING_TYPE.USING_ITEM;

			public const USING_TYPE USING_WEAPON = USING_TYPE.USING_WEAPON;

			public const USING_ITEM_TYPE U_LOCAL_NORMAL = USING_ITEM_TYPE.U_LOCAL_NORMAL;

			public const USING_ITEM_TYPE U_LOCAL_WEAPON = USING_ITEM_TYPE.U_LOCAL_WEAPON;

			public const ITEM_SELECT_STATE W_LOCAL_CHOICE = ITEM_SELECT_STATE.W_LOCAL_CHOICE;

			public const ITEM_SELECT_STATE W_LOCAL_DECIDE = ITEM_SELECT_STATE.W_LOCAL_DECIDE;

			public const int TAB_ITEM = 0;

			public const int TAB_WEAPON = 1;

			public const int TAB_MAX = 2;

			public const int BTL_DEPTH = 0;

			public const int BTL_POPUP = 1;

			public const int BTL_ITEM_WND_MAX = 2;

			public const int BTL_MSG_ITEM = 0;

			public const int BTL_MSG_WEAPON = 1;

			public const int BTL_MSG_MAX = 2;

			public const int USER_CONTACT_OFF = 0;

			public const int USER_CONTACT_ON = 1;

			public const int UP = 0;

			public const int DOWN = 1;

			public const int TRIANGLE_MAX = 2;

			public static MenuBattleItem instance_ = new MenuBattleItem();

			private USING_TYPE prev_menu_type;

			private USING_TYPE b_menu_type;

			private TOP_SPRITE weaponCursor = new TOP_SPRITE();

			private sys2d.Sprite3d[] triangle_ = new sys2d.Sprite3d[2];

			private int[] windowNo = new int[2];

			private USING_ITEM_TYPE u_LocalType;

			private ITEM_SELECT_STATE w_LocalType;

			private int prevItemNo;

			private int prevHandType;

			private int saveKeyState;

			private sbyte userContact;

			private bool changeWindow_;

			public MenuBattleItem()
			{
				for (int i = 0; i < triangle_.Length; i++)
				{
					triangle_[i] = new sys2d.Sprite3d();
				}
			}

			public void SetUpBItemStatus(bool equipMode, int windowId)
			{
				u_LocalType = USING_ITEM_TYPE.U_LOCAL_NORMAL;
				w_LocalType = ITEM_SELECT_STATE.W_LOCAL_CHOICE;
				userContact = 0;
				changeWindow_ = false;
				if (equipMode)
				{
					b_menu_type = USING_TYPE.USING_WEAPON;
				}
				else
				{
					b_menu_type = USING_TYPE.USING_ITEM;
				}
				windowNo[0] = windowId;
				prev_menu_type = b_menu_type;
				if (b_menu_type == USING_TYPE.USING_ITEM)
				{
					MenuManager.getSingleton().buildMenu("battle_item");
					MenuManager.getSingleton().GetCursor3d().SetShow(show: true);
					MenuManager.getSingleton().GetMenuWindowObj()[windowNo[0]].GetWindowHandle().SetBar(3);
				}
				else if (b_menu_type == USING_TYPE.USING_WEAPON)
				{
					MenuManager.getSingleton().release();
					AddWindowItemEquip();
					MenuManager.getSingleton().buildMenu("battle_equip");
					MenuManager.getSingleton().SetTouchPanelState(val: false);
					MenuManager.getSingleton().GetMenuWindowObj()[windowNo[0]].GetWindowHandle().SetBar(4);
					if (opt.COptionManager.getSingleton().cursorOption().cursorPosition() == opt.CURSOR_POSITION.CURSOR_POSITION_MEMORY)
					{
						MenuManager.getSingleton().initFocus(MenuManager.getSingleton().saveBattleEquipHand(MenuManager.getSingleton().GetTargetCharNo()));
					}
				}
				if (b_menu_type == USING_TYPE.USING_ITEM)
				{
					int[] array = new int[2] { 9, 12 };
					bool[] array2 = new bool[2] { true, false };
					NNSG2dFVec2[] array3 = new NNSG2dFVec2[2]
					{
						new NNSG2dFVec2(1851392, 524288),
						new NNSG2dFVec2(1851392, 1212416)
					};
					for (int i = 0; i < 2; i++)
					{
						triangle_[i].copy(MenuManager.getSingleton().GetMenuButtonIcon3d());
						triangle_[i].SetCell((ushort)array[i]);
						triangle_[i].SetShow(array2[i]);
						triangle_[i].SetPosition(array3[i]);
						triangle_[i].SetScaleF(4096, 4096);
						sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(triangle_[i]);
					}
					if (MenuManager.getSingleton().GetItemListPatern() == 5)
					{
						triangle_[0].SetShow(show: false);
					}
					else if (b_menu_type == USING_TYPE.USING_ITEM && opt.COptionManager.getSingleton().cursorOption().cursorPosition() == opt.CURSOR_POSITION.CURSOR_POSITION_MEMORY && MenuManager.getSingleton().saveBattleUseItem(MenuManager.getSingleton().GetTargetCharNo()) > 0)
					{
						userContact = 1;
						changeEquipItem();
					}
				}
			}

			public void ReleaseBItemStatus()
			{
				weaponCursor.bEnable = false;
				weaponCursor.sprite.Release();
				sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(weaponCursor.sprite);
				DecWindowItemEquip();
				MenuManager.getSingleton().releaseWindowAll();
				MenuManager.getSingleton().release();
				for (int i = 0; i < 2; i++)
				{
					triangle_[i].Release();
					sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(triangle_[i]);
				}
			}

			public bool ProcessingBItem()
			{
				saveKeyState = (int)MenuManager.getSingleton().getFocuseMedget().work();
				changeWindow_ = false;
				MenuManager.getSingleton().execute();
				switch (b_menu_type)
				{
				case USING_TYPE.USING_ITEM:
					userContact = 1;
					ProcessUsingItem();
					break;
				case USING_TYPE.USING_WEAPON:
					userContact = 0;
					ProcessChangeEquip();
					break;
				}
				MenuManager.getSingleton().SetActivateButtonState(1);
				MenuManager.getSingleton().ClearBehaviorButton();
				return true;
			}

			public bool ProcessUsingItem()
			{
				prev_menu_type = USING_TYPE.USING_ITEM;
				switch (u_LocalType)
				{
				case USING_ITEM_TYPE.U_LOCAL_NORMAL:
					ProcessUsingItemSelectNormal();
					break;
				case USING_ITEM_TYPE.U_LOCAL_WEAPON:
					ProcessUsingItemSelectWeapon();
					break;
				}
				return true;
			}

			public void ProcessUsingItemSelectNormal()
			{
				if (MenuManager.getSingleton().GetItemListPatern() != 5 && saveKeyState == (int)MenuManager.getSingleton().getFocuseMedget().work())
				{
					changeWindow_ = false;
					if (((int)MenuManager.getSingleton().getFocuseMedget().work() == 0 || (int)MenuManager.getSingleton().getFocuseMedget().work() == 1) && (ds.g_Pad.repeat() & 0x40) != 0)
					{
						changeWindow_ = true;
					}
					if ((int)MenuManager.getSingleton().getFocuseMedget().work() >= 0 && (int)MenuManager.getSingleton().getFocuseMedget().work() <= 7 && isTouchTriangle(0))
					{
						changeWindow_ = true;
					}
					if (changeWindow_)
					{
						MenuManager.getSingleton().saveBattleUseItem_set(MenuManager.getSingleton().GetTargetCharNo(), 1);
						MenuManager.getSingleton().playSEMoveCursor();
						changeEquipItem();
					}
				}
			}

			public void ProcessUsingItemSelectWeapon()
			{
				if ((ds.g_Pad.repeat() & 0x80) != 0 || isTouchTriangle(1))
				{
					MenuManager.getSingleton().saveBattleUseItem_set(MenuManager.getSingleton().GetTargetCharNo(), 0);
					MenuManager.getSingleton().playSEMoveCursor();
					MenuManager.getSingleton().release();
					MenuManager.getSingleton().buildMenu("battle_item");
					MenuManager.getSingleton().SetImposibleScrollFlag(val: false);
					u_LocalType = USING_ITEM_TYPE.U_LOCAL_NORMAL;
					triangle_[0].SetShow(show: true);
					triangle_[1].SetShow(show: false);
					MenuManager.getSingleton().GetMenuWindowObj()[windowNo[0]].GetWindowHandle().SetBar(3);
				}
			}

			public bool ProcessChangeEquip()
			{
				prev_menu_type = USING_TYPE.USING_WEAPON;
				ds.g_TouchPanel.getPoint(out var x, out var y);
				switch (w_LocalType)
				{
				case ITEM_SELECT_STATE.W_LOCAL_CHOICE:
					MenuManager.getSingleton().SetImposibleScrollFlag(val: true);
					MenuManager.getSingleton().saveBattleEquipHand_set(MenuManager.getSingleton().GetTargetCharNo(), MenuManager.getSingleton().getFocuseMedget().myTag());
					if (MenuManager.getSingleton().GetCancelButtonState() == 0)
					{
						w_LocalType = ITEM_SELECT_STATE.W_LOCAL_CHOICE;
						b_menu_type = USING_TYPE.USING_ITEM;
						userContact = 1;
						MenuManager.getSingleton().GetCursor3d().SetShow(show: false);
						MenuManager.getSingleton().playSECancel();
						return true;
					}
					if (MenuManager.getSingleton().GetDecideButtonState() == 0 || (ds.g_TouchPanel.isTap() && MenuManager.getSingleton().TouchWindowOutArea(x, y)))
					{
						if (MenuManager.getSingleton().getFocuseMedget() == null)
						{
							return false;
						}
						if (MenuManager.getSingleton().getFocuseMedget().myTag() == 0)
						{
							prevItemNo = pl.PlayerParty.instance().player((byte)MenuManager.getSingleton().GetTargetCharNo()).equipParameter()
								.equipHand(pl.HAND_TYPE.RIGHT_HAND)
								.itemId();
						}
						else
						{
							prevItemNo = pl.PlayerParty.instance().player((byte)MenuManager.getSingleton().GetTargetCharNo()).equipParameter()
								.equipHand(pl.HAND_TYPE.LEFT_HAND)
								.itemId();
						}
						prevHandType = MenuManager.getSingleton().getFocuseMedget().myTag();
						MenuManager.getSingleton().playSEDecide();
						CreateWeaponDummyCursor();
						ChangeConnectFocuseToItemList();
						MenuManager.getSingleton().SetImposibleScrollFlag(val: false);
						w_LocalType = ITEM_SELECT_STATE.W_LOCAL_DECIDE;
					}
					break;
				case ITEM_SELECT_STATE.W_LOCAL_DECIDE:
				{
					int points = ((prevHandType != 0) ? 1 : 0);
					if (MenuManager.getSingleton().GetCancelButtonState() == 0)
					{
						updateEquipMessage();
						ChangeConnectFocuseToWeapon();
						DeleteWeaponDummyCursor();
						MenuManager.getSingleton().SetImposibleScrollFlag(val: true);
						w_LocalType = ITEM_SELECT_STATE.W_LOCAL_CHOICE;
						MenuManager.getSingleton().playSECancel();
					}
					else if (MenuManager.getSingleton().GetDecideButtonState() == 0 || (ds.g_TouchPanel.isTap() && MenuManager.getSingleton().TouchWindowOutArea(x, y)))
					{
						if (pl.PlayerParty.instance().player((byte)MenuManager.getSingleton().GetTargetCharNo()).doEquip(points, (short)MenuManager.getSingleton().GetTargetItemNo(), sort: true))
						{
							MenuManager.getSingleton().playSEDecide();
						}
						else
						{
							MenuManager.getSingleton().playSEBeep();
						}
						updateEquipMessage();
						ChangeConnectFocuseToWeapon();
						DeleteWeaponDummyCursor();
						MenuManager.getSingleton().SetImposibleScrollFlag(val: true);
						w_LocalType = ITEM_SELECT_STATE.W_LOCAL_CHOICE;
					}
					else
					{
						Medget nodeByID = MenuManager.getSingleton().getFocuseMedget().parentNode()
							.parentNode()
							.getNodeByID(TRANSCODE("equip_list"));
						MBBattleEquip mBBattleEquip = static_cast<MBBattleEquip>(nodeByID.behavior().queryInterface(MBBattleEquip.classIdentifier()));
						mBBattleEquip.updateUpDownTriangle(nodeByID, dgs.msg.CMessageSys.getInstance().Main(), points, (short)MenuManager.getSingleton().GetTargetItemNo());
					}
					break;
				}
				}
				return false;
			}

			public void CreateWeaponDummyCursor()
			{
				weaponCursor.sprite.copy(MenuManager.getSingleton().GetCursor3d());
				weaponCursor.bEnable = true;
				weaponCursor.sprite.SetCell(3);
				sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(weaponCursor.sprite);
				NNSG2dSVec2 positionI = MenuManager.getSingleton().GetCursor3d().GetPositionI();
				weaponCursor.pos.vx = positionI.x;
				weaponCursor.pos.vy = positionI.y;
				weaponCursor.sprite.SetPositionI(weaponCursor.pos.vx, weaponCursor.pos.vy);
				weaponCursor.sprite.SetAnimation(anm: false);
				weaponCursor.sprite.SetDepth(1);
			}

			public void DeleteWeaponDummyCursor()
			{
				weaponCursor.sprite.Release();
				weaponCursor.bEnable = false;
				sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(weaponCursor.sprite);
			}

			public void updateEquipMessage()
			{
				Medget nodeByID = MenuManager.getSingleton().getFocuseMedget().parentNode()
					.parentNode()
					.getNodeByID(TRANSCODE("equip_list"));
				MBBattleEquip mBBattleEquip = static_cast<MBBattleEquip>(nodeByID.behavior().queryInterface(MBBattleEquip.classIdentifier()));
				mBBattleEquip.releaseMessageAndHiddenTriangle(nodeByID);
				mBBattleEquip.CreateEquipmentWeapon(nodeByID, dgs.msg.CMessageSys.getInstance().Main(), MenuManager.getSingleton().GetTargetCharNo());
				MenuManager.getSingleton().setUpdateMessageFlag(flag: true);
			}

			public void ChangeConnectFocuseToItemList()
			{
				Medget medget = MenuManager.getSingleton().getFocuseMedget().parentNode();
				Medget medget2 = medget.childNode();
				for (int i = 0; i < 2; i++)
				{
					MenuManager.getSingleton().leaveFocusList(medget2);
					medget2 = medget2.nextSibling();
				}
				Medget nodeByID = medget.parentNode().getNodeByID(TRANSCODE("item_list"));
				for (Medget medget3 = nodeByID.childNode(); medget3 != null; medget3 = medget3.nextSibling())
				{
					MenuManager.getSingleton().joinFocusList(medget3);
				}
				if (opt.COptionManager.getSingleton().cursorOption().cursorPosition() == opt.CURSOR_POSITION.CURSOR_POSITION_MEMORY)
				{
					MenuManager.getSingleton().initFocus(0);
				}
				else
				{
					MenuManager.getSingleton().initFocus(0);
				}
			}

			public void ChangeConnectFocuseToWeapon()
			{
				Medget medget = MenuManager.getSingleton().getFocuseMedget().parentNode();
				for (Medget medget2 = medget.childNode(); medget2 != null; medget2 = medget2.nextSibling())
				{
					MenuManager.getSingleton().leaveFocusList(medget2);
				}
				Medget nodeByID = medget.parentNode().getNodeByID(TRANSCODE("equip_list"));
				Medget medget3 = nodeByID.childNode();
				for (int i = 0; i < 2; i++)
				{
					MenuManager.getSingleton().joinFocusList(medget3);
					medget3 = medget3.nextSibling();
				}
				if (opt.COptionManager.getSingleton().cursorOption().cursorPosition() == opt.CURSOR_POSITION.CURSOR_POSITION_MEMORY)
				{
					MenuManager.getSingleton().initFocus(MenuManager.getSingleton().saveBattleEquipHand(MenuManager.getSingleton().GetTargetCharNo()));
				}
				else
				{
					MenuManager.getSingleton().initFocus(0);
				}
			}

			public void AddWindowItemEquip()
			{
				windowNo[1] = MenuManager.getSingleton().buildWindow("battle_equip", "m_main");
				MenuManager.getSingleton().UpdateWindowState(windowNo[1], MenuWindow.MENU_WINDOW_MOVE_TYPE.WINDOW_SIZE_MOVING_LARGE);
			}

			public void DecWindowItemEquip()
			{
				MenuManager.getSingleton().releaseWindow(windowNo[1]);
			}

			public bool HitArea(int x, int y, int sx, int sy)
			{
				ds.g_TouchPanel.getPoint(out var x2, out var y2);
				if (x2 > x && x2 < x + sx && y2 > y && y2 < y + sy)
				{
					return true;
				}
				return false;
			}

			public bool isTouchTriangle(int triangle)
			{
				if (triangle < 0)
				{
					return false;
				}
				if (triangle > 1)
				{
					return false;
				}
				if (!triangle_[triangle].IsShow())
				{
					return false;
				}
				int x = triangle_[triangle].GetPositionI().x;
				int y = triangle_[triangle].GetPositionI().y;
				if (triangle == 0 && ds.g_TouchPanel.getDispPoint().flickOffset < 0)
				{
					return true;
				}
				if (triangle == 1 && ds.g_TouchPanel.getDispPoint().flickOffset > 0)
				{
					return true;
				}
				if (triangle == 0 && !ds.g_TouchPanel.isEdge())
				{
					return false;
				}
				if (triangle == 1 && !ds.g_TouchPanel.isTap())
				{
					return false;
				}
				return HitArea(x, y, 24, 24);
			}

			public void changeEquipItem()
			{
				MenuManager.getSingleton().release();
				MenuManager.getSingleton().buildMenu("battle_use_weapon");
				MenuManager.getSingleton().SetImposibleScrollFlag(val: true);
				MenuManager.getSingleton().initFocus(0);
				u_LocalType = USING_ITEM_TYPE.U_LOCAL_WEAPON;
				triangle_[0].SetShow(show: false);
				triangle_[1].SetShow(show: true);
				if (opt.COptionManager.getSingleton().cursorOption().cursorPosition() == opt.CURSOR_POSITION.CURSOR_POSITION_MEMORY)
				{
					MenuManager.getSingleton().initFocus(MenuManager.getSingleton().saveBattleUseItemHand(MenuManager.getSingleton().GetTargetCharNo()));
				}
				MenuManager.getSingleton().GetMenuWindowObj()[windowNo[0]].GetWindowHandle().SetBar(0);
			}

			public static MenuBattleItem getSingleton()
			{
				return instance_;
			}

			public sbyte GetUserContact()
			{
				return userContact;
			}

			public void SetUserContact(sbyte val)
			{
				userContact = val;
			}

			public USING_ITEM_TYPE usingItemType()
			{
				return u_LocalType;
			}

			public ITEM_SELECT_STATE localType()
			{
				return w_LocalType;
			}

			public bool changeWindow()
			{
				return changeWindow_;
			}
		}
	}
}
