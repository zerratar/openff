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
		public class MBShopSellList : MenuBehavior, SBEventHandler
		{
			public class __icons
			{
				public sys2d.Cell d2 = new sys2d.Cell();

				public sys2d.Sprite3d d3 = new sys2d.Sprite3d();
			}

			public const int ITEM_NUM = 384;

			public const int MESSAGE_NUM = 16;

			public static dgs.UniqueNumber MBShopSellList_UN = new dgs.UniqueNumber();

			private bool _Init;

			private bool _ScrollBarFlag;

			private int _currentLine;

			private int _numberOfLines;

			private int _numberOfColumns;

			private int _numberOfRows;

			private int _targetLCD;

			private int _importantItem;

			private ScrollBar _ScrollBar = new ScrollBar();

			private MBText _mbCaptionText;

			private Medget _savedMedget;

			private int _saveCursor;

			private __icons[] _icons = new __icons[16];

			public MBShopSellList()
			{
				for (int i = 0; i < _icons.Length; i++)
				{
					_icons[i] = new __icons();
				}
				_savedMedget = null;
			}

			~MBShopSellList()
			{
				releaseItemMessage();
				releaseScrollBar();
			}

			public override void bmInitialize(Medget M)
			{
				_Init = false;
				reset();
				XbnNode firstNodeByTagNameFromChildren = M.node().getFirstNodeByTagNameFromChildren(TRANSCODE("behavior"));
				if (firstNodeByTagNameFromChildren != null)
				{
					XbnNodeList xbnNodeList = new XbnNodeList();
					firstNodeByTagNameFromChildren.getNodesByTagNameFromChildren(TRANSCODE("parameter"), xbnNodeList);
					bool flag = false;
					ds.Vector2<short> vector = new ds.Vector2<short>();
					flag = GET_DECIMAL_PARAMETER(xbnNodeList, 0) != 0;
					vector.vx = (short)GET_DECIMAL_PARAMETER(xbnNodeList, 1);
					vector.vy = (short)GET_DECIMAL_PARAMETER(xbnNodeList, 2);
					short height = (short)GET_DECIMAL_PARAMETER(xbnNodeList, 3);
					_numberOfColumns = GET_DECIMAL_PARAMETER(xbnNodeList, 4);
					_numberOfRows = GET_DECIMAL_PARAMETER(xbnNodeList, 5);
					_targetLCD = GET_DECIMAL_PARAMETER(xbnNodeList, 6);
					_importantItem = GET_DECIMAL_PARAMETER(xbnNodeList, 7);
					if (flag)
					{
						createScrollBar(vector, height);
					}
					_saveCursor = -1;
					MenuManager.getSingleton().initFocus(0);
				}
			}

			public override void bmPostInitialize(Medget M)
			{
				for (Medget medget = ownerMedget.childNode(); medget != null; medget = medget.nextSibling())
				{
					SET_TXT_NAME(medget, (MBText)medget.childNode().behavior().queryInterface(MBText.classIdentifier()));
					SET_TXT_NUM(medget, (MBText)medget.childNode().nextSibling().behavior()
						.queryInterface(MBText.classIdentifier()));
				}
				_mbCaptionText = null;
				Medget nodeByID = ownerMedget.parentNode().getNodeByID(TRANSCODE("caption"));
				if (nodeByID != null)
				{
					_mbCaptionText = (MBText)nodeByID.behavior().queryInterface(MBText.classIdentifier());
				}
				setupItemParameter();
				releaseItemMessage();
				createItemMessage();
				_Init = true;
			}

			public override void bmBehave(Medget M)
			{
			}

			public override void bmFinalize(Medget M)
			{
				releaseItemMessage();
				releaseScrollBar();
			}

			public override void bmSuspend(Medget M)
			{
				for (Medget medget = ownerMedget.childNode(); medget != null; medget = medget.nextSibling())
				{
					if (GET_TXT_NUM(medget) != null)
					{
						GET_TXT_NUM(medget).bmTextVisibility(v: false);
					}
				}
				for (Medget medget2 = ownerMedget.childNode(); medget2 != null; medget2 = medget2.nextSibling())
				{
					if (_savedMedget != medget2 && GET_TXT_NAME(medget2) != null)
					{
						GET_TXT_NAME(medget2).mbSetTextColor(dgs.TXT_COLOR.TXT_UCOLOR_4);
						GET_TXT_NAME(medget2).getMessage().setActivity(b: true);
					}
					else if (_savedMedget == medget2 && GET_TXT_NAME(medget2) != null)
					{
						GET_TXT_NAME(medget2).mbSetTextColor(dgs.TXT_COLOR.TXT_COLOR_YELLOW);
						GET_TXT_NAME(medget2).getMessage().setActivity(b: true);
					}
				}
				_ScrollBar.sbRestrainCheck((ScrollBar.AREA_FLAG)3);
				flagOn(1);
			}

			public override void bmResume(Medget M)
			{
				for (Medget medget = ownerMedget.childNode(); medget != null; medget = medget.nextSibling())
				{
					if (GET_TXT_NUM(medget) != null)
					{
						GET_TXT_NUM(medget).bmTextVisibility(v: true);
					}
					if (GET_TXT_NAME(medget) != null)
					{
						GET_TXT_NAME(medget).mbSetTextColor(dgs.TXT_COLOR.TXT_COLOR_WHITE);
					}
				}
				if (_savedMedget != null && GET_ITEM_PTR(_savedMedget) != null)
				{
					itm.PossessionItem possessionItem = (itm.PossessionItem)GET_ITEM_PTR(_savedMedget);
					if (possessionItem.itemNumber() > 0)
					{
						MenuManager.getSingleton().SetTargetItemNo(possessionItem.itemId());
					}
					else
					{
						MenuManager.getSingleton().SetTargetItemNo(-1);
					}
				}
				else
				{
					MenuManager.getSingleton().SetTargetItemNo(-1);
				}
				_ScrollBar.sbRestrainCheck();
				flagOff(1);
				_savedMedget = null;
				updateDisplay();
			}

			public override bool bmDecide(Medget M)
			{
				bool result = true;
				if (MenuManager.getSingleton().GetTargetItemNo() != -1)
				{
					_savedMedget = MenuManager.getSingleton().getFocuseMedget();
					int num = 0;
					for (Medget medget = ownerMedget.childNode(); medget != null; medget = medget.nextSibling())
					{
						if (_savedMedget != medget && GET_TXT_NAME(medget) != null)
						{
							GET_TXT_NAME(medget).mbSetTextColor(dgs.TXT_COLOR.TXT_UCOLOR_4);
						}
						else
						{
							_saveCursor = num;
						}
						num++;
					}
					_ScrollBar.sbRestrainCheck((ScrollBar.AREA_FLAG)3);
					flagOn(1);
					MenuManager.getSingleton().SetDecideButtonState(0);
				}
				return result;
			}

			public override bool bmCancel(Medget M)
			{
				bool result = false;
				MenuManager.getSingleton().SetCancelButtonState(0);
				return result;
			}

			public override bool bmDirection(Medget M, int key)
			{
				if (flagCheck(1))
				{
					return true;
				}
				Medget medget = null;
				bool flag = false;
				if ((ds.g_Pad.repeat() & 0x40) != 0)
				{
					flag = true;
					if (M.prevSibling() == null)
					{
						_ScrollBar.sbFixedMove(-1);
						return true;
					}
					medget = MenuManager.getSingleton().initFocusM(M.prevSibling());
					MenuManager.getSingleton().playSEMoveCursor();
				}
				else if ((ds.g_Pad.repeat() & 0x80) != 0)
				{
					flag = true;
					if (M.nextSibling() == null)
					{
						_ScrollBar.sbFixedMove(1);
						return true;
					}
					medget = MenuManager.getSingleton().initFocusM(M.nextSibling());
					MenuManager.getSingleton().playSEMoveCursor();
				}
				if (GET_ITEM_PTR(MenuManager.getSingleton().getFocuseMedget()) != null)
				{
					MenuManager.getSingleton().SetTargetItemNo(((itm.PossessionItem)GET_ITEM_PTR(MenuManager.getSingleton().getFocuseMedget())).itemId());
				}
				else
				{
					MenuManager.getSingleton().SetTargetItemNo(-1);
				}
				if (medget != null && GET_TXT_NAME(medget) != null)
				{
					GET_TXT_NAME(medget).mbSetTextColor(dgs.TXT_COLOR.TXT_COLOR_WHITE);
				}
				if (GET_TXT_NAME(MenuManager.getSingleton().getFocuseMedget()) != null)
				{
					GET_TXT_NAME(MenuManager.getSingleton().getFocuseMedget()).mbSetTextColor(dgs.TXT_COLOR.TXT_COLOR_YELLOW);
				}
				else
				{
					((itm.PossessionItem)GET_ITEM_PTR(MenuManager.getSingleton().getFocuseMedget()))?.setItemId(-1);
				}
				Medget focuseMedget = MenuManager.getSingleton().getFocuseMedget();
				shop.CShopManager.Instance().pCurrentShop().setItemPos(new ds.Vector2<short>(focuseMedget.x(), (short)(focuseMedget.y() + (focuseMedget.height() - 16) / 2)));
				if (!flag)
				{
					return true;
				}
				_savedMedget = MenuManager.getSingleton().getFocuseMedget();
				updatePossessionNumber();
				return true;
			}

			public override void bmActivate(Medget M)
			{
				if (!_Init || M == ownerMedget)
				{
					return;
				}
				if (flagCheck(1))
				{
					if (_saveCursor != -1)
					{
						int saveCursor = _saveCursor;
						_saveCursor = -1;
						MenuManager.getSingleton().setFocuseMedget(saveCursor);
					}
					return;
				}
				if (GET_ITEM_PTR(MenuManager.getSingleton().getFocuseMedget()) != null)
				{
					MenuManager.getSingleton().SetTargetItemNo(((itm.PossessionItem)GET_ITEM_PTR(MenuManager.getSingleton().getFocuseMedget())).itemId());
				}
				else
				{
					MenuManager.getSingleton().SetTargetItemNo(-1);
				}
				Medget focuseMedget = MenuManager.getSingleton().getFocuseMedget();
				shop.CShopManager.Instance().pCurrentShop();
				shop.CShopManager.Instance().pCurrentShop().setItemPos(new ds.Vector2<short>(focuseMedget.x(), (short)(focuseMedget.y() + (focuseMedget.height() - 16) / 2)));
				for (focuseMedget = ownerMedget.childNode(); focuseMedget != null; focuseMedget = focuseMedget.nextSibling())
				{
					if (GET_TXT_NAME(focuseMedget) != null)
					{
						GET_TXT_NAME(focuseMedget).mbSetTextColor(dgs.TXT_COLOR.TXT_COLOR_WHITE);
					}
				}
				if (GET_TXT_NAME(MenuManager.getSingleton().getFocuseMedget()) != null)
				{
					GET_TXT_NAME(MenuManager.getSingleton().getFocuseMedget()).mbSetTextColor(dgs.TXT_COLOR.TXT_COLOR_YELLOW);
					_savedMedget = MenuManager.getSingleton().getFocuseMedget();
				}
				updatePossessionNumber();
			}

			public override void bmDeactivate(Medget M)
			{
			}

			public void sbehScrolled(short currentLine)
			{
				if (!_ScrollBarFlag)
				{
					return;
				}
				if (flagCheck(1))
				{
					_ScrollBar.sbSetLine((short)_currentLine);
					return;
				}
				if (shop.CShopManager.Instance().pCurrentShop() != null && shop.CShopManager.Instance().pCurrentShop().getState() == shop.CBaseShop.SHOP_STATE.SHOP_STATE_SELECT_ITEM_NUM)
				{
					_ScrollBar.sbSetLine((short)_currentLine);
					return;
				}
				if (_currentLine != currentLine)
				{
					_currentLine = currentLine;
					setupItemParameter();
					updateDisplay();
					MenuManager.getSingleton().playSEMoveCursor();
				}
				if (GET_ITEM_PTR(MenuManager.getSingleton().getFocuseMedget()) != null)
				{
					MenuManager.getSingleton().SetTargetItemNo(((itm.PossessionItem)GET_ITEM_PTR(MenuManager.getSingleton().getFocuseMedget())).itemId());
				}
				else
				{
					MenuManager.getSingleton().SetTargetItemNo(-1);
				}
				if (GET_TXT_NAME(MenuManager.getSingleton().getFocuseMedget()) != null)
				{
					GET_TXT_NAME(MenuManager.getSingleton().getFocuseMedget()).mbSetTextColor(dgs.TXT_COLOR.TXT_COLOR_YELLOW);
				}
				updatePossessionNumber();
			}

			public void reset()
			{
				_ScrollBarFlag = false;
				_currentLine = 0;
				_numberOfLines = 256;
				_numberOfColumns = 0;
				_numberOfRows = 0;
				for (Medget medget = ownerMedget.childNode(); medget != null; medget = medget.nextSibling())
				{
					SET_ITEM_PTR(medget, -1);
					SET_CELL_IDX(medget, 35);
				}
			}

			public void updateDisplay()
			{
				releaseItemMessage();
				createItemMessage();
			}

			public void updatePossessionNumber()
			{
				Medget medget = ownerMedget.nextSibling().nextSibling();
				MBText mBText = null;
				MBText mBText2 = null;
				mBText2 = (MBText)medget.childNode().behavior().queryInterface(MBText.classIdentifier());
				mBText = (MBText)medget.childNode().nextSibling().nextSibling()
					.behavior()
					.queryInterface(MBText.classIdentifier());
				mBText2.mbSetBufferNumber(0);
				mBText.mbSetBufferNumber(0);
				if (MenuManager.getSingleton().GetTargetItemNo() <= 0)
				{
					if (_mbCaptionText != null)
					{
						_mbCaptionText.bmTextVisibility(v: false);
					}
					return;
				}
				if (_mbCaptionText != null)
				{
					itm.ItemBaseParameter itemBaseParameter = itm.ItemManager.instance().itemParameter((short)MenuManager.getSingleton().GetTargetItemNo());
					if (itemBaseParameter != null)
					{
						int msg_number = itemBaseParameter.captionId();
						_mbCaptionText.mbSetBufferMsg(dgs.msg.CMessageSys.getInstance().Sub().getMessage((uint)msg_number), decWidth: false);
						_mbCaptionText.bmTextVisibility(v: true);
					}
					else
					{
						_mbCaptionText.mbSetBufferMsg("", decWidth: false);
						_mbCaptionText.bmTextVisibility(v: false);
					}
				}
				for (int i = 0; i < 384; i++)
				{
					int num = pl.PlayerParty.instance().item().normalItem(i)
						.itemId();
					if (num != -1 && num == MenuManager.getSingleton().GetTargetItemNo())
					{
						mBText.mbSetBufferNumber(pl.PlayerParty.instance().item().normalItem(i)
							.itemNumber());
						break;
					}
				}
				int num2 = 0;
				itm.CATEGORY cATEGORY = itm.ItemManager.instance().itemCategory((short)MenuManager.getSingleton().GetTargetItemNo());
				if (cATEGORY == itm.CATEGORY.CATEGORY_MAGIC)
				{
					for (int j = 0; j < 4; j++)
					{
						pl.Player player = pl.PlayerParty.instance().player((byte)j);
						if (!player.isEnable())
						{
							continue;
						}
						for (int k = 0; k < 8; k++)
						{
							for (int l = 0; l < pl.MAGIC_ONCE_LEVEL_EQUIP_MAX; l++)
							{
								if (MenuManager.getSingleton().GetTargetItemNo() == player.equipParameter().equipMagic((pl.MAGIC_LEVEL)k).magicId(l))
								{
									num2++;
								}
							}
						}
					}
				}
				else
				{
					for (int m = 0; m < 4; m++)
					{
						pl.Player player2 = pl.PlayerParty.instance().player((byte)m);
						if (!player2.isEnable())
						{
							continue;
						}
						for (int n = 0; n < 5; n++)
						{
							if (player2.equipParameter().equipPoint(n).itemId() == MenuManager.getSingleton().GetTargetItemNo())
							{
								num2 += player2.equipParameter().equipPoint(n).equipNumber()
									.get();
							}
						}
					}
				}
				mBText2.mbSetBufferNumber(num2);
			}

			public void setupItemParameter()
			{
				int num = 0;
				itm.CATEGORY cATEGORY = itm.CATEGORY.CATEGORY_ERR;
				int num2 = _currentLine * _numberOfColumns;
				for (Medget medget = ownerMedget.childNode(); medget != null; medget = medget.nextSibling())
				{
					itm.PossessionItem possessionItem = null;
					possessionItem = ((_importantItem == 0) ? pl.PlayerParty.instance().item().normalItem(num2) : pl.PlayerParty.instance().item().allItem(num2));
					SET_ITEM_PTR(medget, null);
					SET_CELL_IDX(medget, 35);
					if (possessionItem != null && possessionItem.itemId() > 0 && possessionItem.itemNumber() > 0 && itm.ItemManager.instance().itemParameter(possessionItem.itemId()) != null && itm.ItemManager.instance().itemCategory(possessionItem.itemId()) != itm.CATEGORY.CATEGORY_ERR)
					{
						SET_ITEM_PTR(medget, possessionItem);
						num = itm.ItemManager.instance().itemParameter(possessionItem.itemId()).system();
						switch (itm.ItemManager.instance().itemCategory(possessionItem.itemId()))
						{
						case itm.CATEGORY.CATEGORY_WEAPON:
							SET_CELL_IDX(medget, convertIDXWeaponSysToIcon(num));
							break;
						case itm.CATEGORY.CATEGORY_PROTECTION:
							SET_CELL_IDX(medget, convertIDXProtectionSysToIcon(num));
							break;
						case itm.CATEGORY.CATEGORY_MAGIC:
							SET_CELL_IDX(medget, convertIDXMagicSysToIcon(num));
							break;
						default:
							SET_CELL_IDX(medget, 45);
							break;
						}
					}
					num2++;
				}
			}

			public void createItemMessage()
			{
				dgs.DGSMessageManager dGSMessageManager = null;
				int num = 0;
				for (Medget medget = ownerMedget.childNode(); medget != null; medget = medget.nextSibling())
				{
					if (GET_ITEM_PTR(medget) != null && ((itm.PossessionItem)GET_ITEM_PTR(medget)).itemId() >= 0)
					{
						dGSMessageManager = ((_targetLCD == 1) ? dgs.msg.CMessageSys.getInstance().Main() : dgs.msg.CMessageSys.getInstance().Sub());
						if (itm.ItemManager.instance().itemParameter(((itm.PossessionItem)GET_ITEM_PTR(medget)).itemId()) != null)
						{
							string message = dGSMessageManager.getMessage((uint)itm.ItemManager.instance().itemParameter(((itm.PossessionItem)GET_ITEM_PTR(medget)).itemId()).nameId());
							GET_TXT_NAME(medget).mbSetBufferMsg(const_cast<string>(message), decWidth: true);
							GET_TXT_NAME(medget).bmTextVisibility(v: true);
							GET_TXT_NAME(medget).getMessage().setPriority(3);
							if (num < 16)
							{
								Medget medget2 = medget.childNode();
								if (MenuManager.getSingleton().Get2d3dMode() == 2)
								{
									_icons[num].d2.copy(MenuManager.getSingleton().GetSmallIcon2d());
									_icons[num].d2.SetCell((ushort)GET_CELL_IDX(medget));
									sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(_icons[num].d2);
									_icons[num].d2.SetPositionI(medget2.x() - 16, medget2.y() + (medget2.height() - 16) / 2);
									_icons[num].d2.SetShow(show: true);
									_icons[num].d2.SetPriority(3);
								}
								else
								{
									_icons[num].d3.copy(MenuManager.getSingleton().GetSmallIcon3d());
									_icons[num].d3.SetCell((ushort)GET_CELL_IDX(medget));
									sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(_icons[num].d3);
									_icons[num].d3.SetPositionI(medget2.x() - 16, medget2.y() + (medget2.height() - 16) / 2);
									_icons[num].d3.SetShow(show: true);
									_icons[num].d3.SetPriority(3);
								}
							}
							num++;
							itm.NotImportantParameter notImportantParameter = static_cast<itm.NotImportantParameter>(itm.ItemManager.instance().itemParameter(((itm.PossessionItem)GET_ITEM_PTR(medget)).itemId()));
							GET_TXT_NUM(medget).mbSetBufferNumber(notImportantParameter.price());
							GET_TXT_NUM(medget).bmTextVisibility(v: true);
						}
					}
				}
			}

			public void releaseItemMessage()
			{
				for (int i = 0; i < 16; i++)
				{
					if (MenuManager.getSingleton().Get2d3dMode() == 2)
					{
						OS_Printf("0x%08x : d2dDeleteSprite \n", _icons[i].d2);
						sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(_icons[i].d2);
						_icons[i].d2.Release();
					}
					else if (MenuManager.getSingleton().Get2d3dMode() == 3)
					{
						sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(_icons[i].d3);
						_icons[i].d3.Release();
					}
					for (Medget medget = ownerMedget.childNode(); medget != null; medget = medget.nextSibling())
					{
						if (GET_TXT_NAME(medget) != null)
						{
							GET_TXT_NAME(medget).bmTextVisibility(v: false);
						}
						if (GET_TXT_NUM(medget) != null)
						{
							GET_TXT_NUM(medget).bmTextVisibility(v: false);
						}
					}
				}
			}

			public void createScrollBar(ds.Vector2<short> pos, short height)
			{
				_numberOfLines = 0;
				for (int i = 0; i < 384; i++)
				{
					if (pl.PlayerParty.instance().item().normalItem(i)
						.itemNumber() > 0)
					{
						_numberOfLines = i;
					}
				}
				_numberOfLines = _numberOfLines % 2 + (_numberOfLines >> 1) + 1;
				if (_numberOfLines > 192)
				{
					_numberOfLines = 192;
				}
				_ScrollBar.sbCreate();
				_ScrollBar.sbSetPosition(pos.vx, pos.vy);
				_ScrollBar.sbSetHeight(height);
				_ScrollBar.sbSetCapacity((short)_numberOfRows, (short)(_numberOfLines + 1));
				_ScrollBar.sbSetHandler(this);
				_ScrollBar.sbSetDepth(2);
				_ScrollBarFlag = true;
			}

			public void releaseScrollBar()
			{
				if (_ScrollBarFlag)
				{
					_ScrollBar.sbDestroy();
					_ScrollBar.sbSetHandler(null);
				}
			}

			public new static int classIdentifier()
			{
				return MBShopSellList_UN.number();
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
