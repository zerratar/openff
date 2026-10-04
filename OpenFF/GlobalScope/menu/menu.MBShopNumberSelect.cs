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
		public class MBShopNumberSelect : MenuBehavior
		{
			public enum CURSOR_LIST
			{
				CURSOR_ERR = -1,
				CURSOR_UP,
				CURSOR_DOWN,
				CURSOR_MAX
			}

			public class CURSOR
			{
				public ds.Vector2<short> m_TouchSize = new ds.Vector2<short>();

				public sys2d.Cell m_Cell = new sys2d.Cell();
			}

			public const CURSOR_LIST CURSOR_ERR = CURSOR_LIST.CURSOR_ERR;

			public const CURSOR_LIST CURSOR_UP = CURSOR_LIST.CURSOR_UP;

			public const CURSOR_LIST CURSOR_DOWN = CURSOR_LIST.CURSOR_DOWN;

			public const CURSOR_LIST CURSOR_MAX = CURSOR_LIST.CURSOR_MAX;

			public static dgs.UniqueNumber MBShopNumberSelect_UN = new dgs.UniqueNumber();

			private bool _cursorShow;

			private bool _SavedVisibility;

			private int _OldItemNum;

			private int _InputFrame;

			private CURSOR_LIST _PushCursor;

			private CURSOR[] m_Cursor = new CURSOR[2];

			private Medget m_NumberMedget;

			private Medget m_TotalMedget;

			private Medget m_IconMedget;

			private Medget m_NameMedget;

			public MBShopNumberSelect()
			{
				for (int i = 0; i < m_Cursor.Length; i++)
				{
					m_Cursor[i] = new CURSOR();
				}
			}

			~MBShopNumberSelect()
			{
			}

			public override void bmInitialize(Medget M)
			{
				_OldItemNum = shop.CShopManager.Instance().pCurrentShop().getItemNum();
				_InputFrame = 0;
				_PushCursor = CURSOR_LIST.CURSOR_ERR;
			}

			public override void bmPostInitialize(Medget M)
			{
				m_NumberMedget = ownerMedget.childNode().nextSibling();
				SET_MSG_NUM(m_NumberMedget, null);
				m_TotalMedget = m_NumberMedget.nextSibling();
				SET_MSG_TOTAL(m_TotalMedget, null);
				// The phone's layout has the item's icon and name after the total; the Steam release's shop_number has the
				// arrows (up, down) there and no icon or name - those are taken only when they are frames the code fills.
				m_IconMedget = m_TotalMedget?.nextSibling();
				m_NameMedget = m_IconMedget?.nextSibling();
				if (m_IconMedget?.behavior() == null || m_IconMedget.behavior().queryInterface(MBIcon.classIdentifier()) == null) m_IconMedget = null;
				if (m_NameMedget?.behavior() == null || m_NameMedget.behavior().queryInterface(MBText.classIdentifier()) == null) m_NameMedget = null;
				if (m_IconMedget == null && m_NameMedget == null) SteamPlacement();
				setupTotalMoney(M);
				setupItemNum(M);
				setupCursor(M);
				setupName(M);
				if (shop.CShopManager.Instance().pCurrentShop().getPreviousState() == shop.CBaseShop.SHOP_STATE.SHOP_STATE_SELL_ITEM)
				{
					Medget medget = ownerMedget.nextSibling();
					if (medget != null && medget.behavior() != null)
					{
						((MBShopText)medget.behavior().queryInterface(MBShopText.classIdentifier()))?.mbChangePage(1);
					}
				}
			}

			public override void bmBehave(Medget M)
			{
				if (inputItemNum(M))
				{
					MenuManager.getSingleton().playSEMoveCursor();
					setupTotalMoney(M);
					setupItemNum(M);
				}
				_OldItemNum = shop.CShopManager.Instance().pCurrentShop().getItemNum();
			}

			public override void bmFinalize(Medget M)
			{
				for (int i = 0; i < 2; i++)
				{
					m_Cursor[i].m_Cell.Release();
					sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(m_Cursor[i].m_Cell);
				}
				if (m_Shifted) { G2S_SetBG3Offset(0, 0); m_Shifted = false; }
			}

			public override void bmSuspend(Medget M)
			{
			}

			public override void bmResume(Medget M)
			{
			}

			public override bool bmDecide(Medget M)
			{
				bool result = true;
				MenuManager.getSingleton().SetDecideButtonState(0);
				return result;
			}

			public override bool bmCancel(Medget M)
			{
				bool result = true;
				for (Medget medget = M.childNode(); medget != null; medget = medget.nextSibling())
				{
					MenuManager.getSingleton().leaveFocusList(medget);
				}
				MenuManager.getSingleton().SetCancelButtonState(0);
				MenuManager.getSingleton().GetCursor3d().SetShow(show: false);
				return result;
			}

			public override bool bmDirection(Medget M, int key)
			{
				return true;
			}

			public void setupTotalMoney(Medget M)
			{
				MBText mBText = (MBText)m_TotalMedget.behavior().queryInterface(MBText.classIdentifier());
				itm.NotImportantParameter notImportantParameter = static_cast<itm.NotImportantParameter>(itm.ItemManager.instance().itemParameter((short)MenuManager.getSingleton().GetTargetItemNo()));
				if (mBText != null && notImportantParameter != null)
				{
					int num = 0;
					if (shop.CShopManager.Instance().pCurrentShop().getPreviousState() == shop.CBaseShop.SHOP_STATE.SHOP_STATE_BUY_ITEM)
					{
						int itemNum = shop.CShopManager.Instance().pCurrentShop().getItemNum();
						num = notImportantParameter.buy();
						num *= itemNum;
						num = shop.discount(num, itemNum);
					}
					else if (shop.CShopManager.Instance().pCurrentShop().getPreviousState() == shop.CBaseShop.SHOP_STATE.SHOP_STATE_SELL_ITEM)
					{
						num = notImportantParameter.price();
						num *= shop.CShopManager.Instance().pCurrentShop().getItemNum();
					}
					mBText.mbSetBufferNumber(num);
				}
			}

			public void setupItemNum(Medget M)
			{
				((MBText)m_NumberMedget.behavior().queryInterface(MBText.classIdentifier()))?.mbSetBufferNumber(shop.CShopManager.Instance().pCurrentShop().getItemNum());
			}

			public void setupCursor(Medget M)
			{
				Medget medget = null;
				medget = MenuManager.getSingleton().root().getNodeByID("up");
				sys2d.Cell menuButtonIcon2d = MenuManager.getSingleton().GetMenuButtonIcon2d();
				int num = 0;
				Medget medget2 = medget;
				while (medget2 != null && num < 2)
				{
					m_Cursor[num].m_Cell.copy(menuButtonIcon2d);
					sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(m_Cursor[num].m_Cell);
					m_Cursor[num].m_Cell.SetCell((ushort)cell_anim[num]);
					m_Cursor[num].m_Cell.SetPositionI(medget2.x() + (medget2.width() - 24) / 2, medget2.y() + (medget2.height() - 24) / 2);
					m_Cursor[num].m_TouchSize.set(24, 24);
					medget2 = medget2.nextSibling();
					num++;
				}
			}

			/// <summary>
			/// Steam's shop_number lays the count out on the title line, over the shop's question, and leaves the dialog
			/// shop_bg01 draws for this step empty (a panel, a column at its right, a strip under both, on the 480x320
			/// canvas). The frames go into it instead: the item's name - in the frame that held the "x" - and the count in
			/// the panel, the arrows in the column, the total in the strip.
			/// </summary>
			private void SteamPlacement()
			{
				Medget first = ownerMedget.childNode();
				Medget up = m_TotalMedget?.nextSibling();
				Medget down = up?.nextSibling();
				if (first == null || first.behavior()?.queryInterface(MBText.classIdentifier()) == null || m_NumberMedget == null || m_TotalMedget == null || up == null || down == null) return;
				// The dialog moved left over the item list, clear of the party panel's portrait it would cover.
				G2S_SetBG3Offset(DialogShift, 0);
				m_Shifted = true;
				int x = -DialogShift;
				Place(first, x + 140, 104, 156, 18);
				Place(m_NumberMedget, x + 236, 148, 56, 18);
				Place(m_TotalMedget, x + 140, 197, 200, 18);
				Place(up, x + 304, 100, 48, 36);
				Place(down, x + 304, 146, 48, 36);
				m_NameMedget = first;
			}

			private const int DialogShift = 80;
			private bool m_Shifted;

			private static void Place(Medget m, int x, int y, int width, int height)
			{
				m.setWidth((short)width);
				m.setHeight((short)height);
				m.setPosition((short)x, (short)y);
			}

			public void setupName(Medget M)
			{
				int targetItemNo = MenuManager.getSingleton().GetTargetItemNo();
				if (m_NameMedget != null) ((MBText)m_NameMedget.behavior().queryInterface(MBText.classIdentifier()))?.mbSetTextMsgNo(itm.ItemManager.instance().itemParameter((short)targetItemNo).nameId());
				MBIcon mBIcon = (MBIcon)m_IconMedget?.behavior().queryInterface(MBIcon.classIdentifier());
				if (mBIcon != null)
				{
					int idx = itm.ItemManager.instance().itemParameter((short)targetItemNo).system();
					itm.CATEGORY cATEGORY = itm.ItemManager.instance().itemCategory((short)targetItemNo);
					int num = 0;
					mBIcon.mbSetCell(cATEGORY switch
					{
						itm.CATEGORY.CATEGORY_WEAPON => convertIDXWeaponSysToIcon(idx), 
						itm.CATEGORY.CATEGORY_PROTECTION => convertIDXProtectionSysToIcon(idx), 
						itm.CATEGORY.CATEGORY_MAGIC => convertIDXMagicSysToIcon(idx), 
						_ => 45, 
					});
				}
			}

			public bool inputItemNum(Medget M)
			{
				int targetItemNo = MenuManager.getSingleton().GetTargetItemNo();
				int num = shop.CShopManager.Instance().pCurrentShop().getItemNum();
				int num2 = 99;
				CURSOR_LIST cURSOR_LIST = CURSOR_LIST.CURSOR_ERR;
				if (dv.CDeviceManager.getInstance().Tp().isTouch())
				{
					dv.CDeviceManager.getInstance().Tp().TouchPanel_2d(out var x, out var y);
					for (int i = 0; i < 2; i++)
					{
						Medget nodeByID = MenuManager.getSingleton().root().getNodeByID((i == 0) ? "up" : "down");
						if (nodeByID != null && x >= nodeByID.x() && x < nodeByID.x() + nodeByID.width() && y >= nodeByID.y() && y < nodeByID.y() + nodeByID.height())
						{
							cURSOR_LIST = (CURSOR_LIST)i;
							break;
						}
					}
				}
				for (int j = 0; j < 2; j++)
				{
					int num3 = 0;
					if (j == (int)_PushCursor)
					{
						num3--;
					}
					if (j == (int)cURSOR_LIST)
					{
						num3++;
					}
					if (num3 != 0)
					{
						NNSG2dSVec2 positionI = m_Cursor[j].m_Cell.GetPositionI();
						m_Cursor[j].m_Cell.SetPositionI(positionI.x + num3, positionI.y + num3);
					}
				}
				_PushCursor = cURSOR_LIST;
				if (!dv.CDeviceManager.getInstance().Tp().isTouch())
				{
					if ((ds.g_Pad.edge() & 1) != 0)
					{
						MenuManager.getSingleton().SetDecideButtonState(0);
						return false;
					}
					if ((ds.g_Pad.edge() & 2) != 0)
					{
						MenuManager.getSingleton().playSECancel();
						MenuManager.getSingleton().SetCancelButtonState(0);
						return false;
					}
					if ((ds.g_Pad.repeat() & 0x40) != 0)
					{
						num++;
					}
					else if ((ds.g_Pad.repeat() & 0x80) != 0)
					{
						num--;
					}
					else if ((ds.g_Pad.repeat() & 0x20) != 0)
					{
						num -= 10;
					}
					else
					{
						if ((ds.g_Pad.repeat() & 0x10) == 0)
						{
							return false;
						}
						num += 10;
					}
				}
				if (dv.CDeviceManager.getInstance().Tp().isRepeatTouch())
				{
					switch (cURSOR_LIST)
					{
					case CURSOR_LIST.CURSOR_UP:
						num++;
						break;
					case CURSOR_LIST.CURSOR_DOWN:
						num--;
						break;
					}
				}
				if (shop.CShopManager.Instance().pCurrentShop().getPreviousState() == shop.CBaseShop.SHOP_STATE.SHOP_STATE_BUY_ITEM)
				{
					num2 = 99;
					itm.PossessionItem possessionItem = pl.PlayerParty.instance().item().serchNormalItem((short)targetItemNo);
					if (possessionItem != null)
					{
						num2 -= possessionItem.itemNumber();
					}
					itm.NotImportantParameter notImportantParameter = static_cast<itm.NotImportantParameter>(itm.ItemManager.instance().itemParameter((short)MenuManager.getSingleton().GetTargetItemNo()));
					if (notImportantParameter != null)
					{
						int num4 = (int)((float)pl.PlayerParty.instance().gold().get() / ((float)notImportantParameter.buy() * 0.7f));
						if (num2 > num4)
						{
							num2 = num4;
						}
						while (shop.discount(notImportantParameter.buy() * num2, num2) > pl.PlayerParty.instance().gold().get())
						{
							num2--;
						}
					}
				}
				else if (shop.CShopManager.Instance().pCurrentShop().getPreviousState() == shop.CBaseShop.SHOP_STATE.SHOP_STATE_SELL_ITEM)
				{
					num2 = 99;
					itm.PossessionItem possessionItem2 = pl.PlayerParty.instance().item().serchNormalItem((short)targetItemNo);
					if (possessionItem2 != null)
					{
						num2 = possessionItem2.itemNumber();
					}
				}
				if (num > num2)
				{
					num = num2;
				}
				if (num < 1)
				{
					num = 1;
				}
				int itemNum = shop.CShopManager.Instance().pCurrentShop().getItemNum();
				shop.CShopManager.Instance().pCurrentShop().setItemNum(num);
				return itemNum != num;
			}

			public new static int classIdentifier()
			{
				return MBShopNumberSelect_UN.number();
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
