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
	public static partial class menu
	{
		public class MBShopBuyList : MenuBehavior
		{
			public class LINE
			{
				public int _NowLine;

				public int _MaxLine;

				public int _OneLine;

				public void initialize()
				{
					_NowLine = 0;
					_MaxLine = 0;
					_OneLine = 0;
				}
			}

			public class ITEM_LIST
			{
				public int _ItemId;

				public int _ItemCellAnim;

				public sys2d.Cell _Icon = new sys2d.Cell();
			}

			public const int ITEM_NUM = 12;

			public const int MESSAGE_NUM = 12;

			public static dgs.UniqueNumber MBShopBuyList_UN = new dgs.UniqueNumber();

			private bool _Init;

			private bool _ScrollBarFlag;

			private bool _SavedVisibility;

			private LINE _Width = new LINE();

			private LINE _Height = new LINE();

			private int _MaxItemNum;

			private int _ItemNum;

			private int _PrevItem;

			private ITEM_LIST[] _ItemList = new ITEM_LIST[12];

			private Medget _savedMedget;

			private MBText _mbCaptionText;

			private ds.sys3d.CLightObject[] _savedLight = new ds.sys3d.CLightObject[4];

			public MBShopBuyList()
			{
				for (int i = 0; i < _ItemList.Length; i++)
				{
					_ItemList[i] = new ITEM_LIST();
				}
				for (int i = 0; i < _savedLight.Length; i++)
				{
					_savedLight[i] = new ds.sys3d.CLightObject();
				}
				reset();
				_savedMedget = null;
			}

			~MBShopBuyList()
			{
				releaseItemMessage();
			}

			public override void bmInitialize(Medget M)
			{
				_Init = false;
				GXS_SetVisibleWnd(0);
				ds.sys3d.CLightObject cLightObject = new ds.sys3d.CLightObject();
				for (int i = 0; i < 4; i++)
				{
					if (shop.CShopUpDisplayComposition.Instance().charIndex(i) >= 0)
					{
						characterMng.setEmission(shop.CShopUpDisplayComposition.Instance().charIndex(i), disable_emission);
						characterMng.getLight(shop.CShopUpDisplayComposition.Instance().charIndex(i), _savedLight[i]);
						cLightObject.copy(_savedLight[i]);
						for (int j = 0; j < 4; j++)
						{
							cLightObject.setLightColor(j, 0, 0, 0);
						}
						characterMng.setLight(shop.CShopUpDisplayComposition.Instance().charIndex(i), cLightObject);
					}
				}
				reset();
				flagOff(1);
			}

			public override void bmPostInitialize(Medget M)
			{
				Medget nodeByID = ownerMedget.parentNode().getNodeByID(TRANSCODE("influence"));
				nodeByID = nodeByID.childNode();
				if (shop.CShopManager.Instance().Kind() == shop.CShopManager.SHOP_KIND.SHOP_KIND_MAGIC)
				{
					G2S_SetWnd0Position(nodeByID.x(), nodeByID.y(), nodeByID.x() + nodeByID.width(), nodeByID.y() + nodeByID.height());
					G2S_SetWnd0InsidePlane(13, 0);
					G2S_SetWndOutsidePlane(31, 0);
					GXS_SetVisibleWnd(0);
				}
				Medget medget = nodeByID;
				for (int i = 0; i < 4; i++)
				{
					if (!pl.PlayerParty.instance().player((byte)i).isEnable())
					{
						Medget medget2 = medget.childNode().nextSibling();
						for (int j = 0; j < 2; j++)
						{
							((MBIcon)medget2.childNode().behavior().queryInterface(MBIcon.classIdentifier()))?.mbSetVisibility(v: false);
							medget2 = medget2.nextSibling();
						}
						MenuManager.getSingleton().MedgetsSuspend(medget);
					}
					else
					{
						((MBText)medget.childNode().behavior().queryInterface(MBText.classIdentifier()))?.mbSetBufferMsg(pl.PlayerParty.instance().player((byte)i).name(), decWidth: false);
						Medget medget3 = medget.childNode().nextSibling();
						for (int k = 0; k < 2; k++)
						{
							MBIcon mBIcon = (MBIcon)medget3.childNode().behavior().queryInterface(MBIcon.classIdentifier());
							if (mBIcon != null)
							{
								MBText mBText = (MBText)medget3.childNode().nextSibling().behavior()
									.queryInterface(MBText.classIdentifier());
								if (k > 0 && shop.CShopManager.Instance().Kind() == shop.CShopManager.SHOP_KIND.SHOP_KIND_ARMOR)
								{
									mBIcon.mbSetVisibility(v: false);
									mBText.bmTextVisibility(v: false);
									continue;
								}
								pl.EquipmentItem equipmentItem = pl.PlayerParty.instance().player((byte)i).equipParameter()
									.equipPoint(k);
								if (equipmentItem.checkCategory() == itm.CATEGORY.CATEGORY_WEAPON)
								{
									mBIcon.mbSetCell(convertIDXWeaponSysToIcon((int)equipmentItem.weaponSystem()));
									if (mBText != null)
									{
										itm.WeaponParameter weaponParameter = itm.ItemManager.instance().weaponParameter(equipmentItem.itemId());
										if (weaponParameter != null)
										{
											mBText.mbSetBufferNumber(weaponParameter.aggressivity());
										}
									}
								}
								else if (equipmentItem.checkCategory() == itm.CATEGORY.CATEGORY_PROTECTION)
								{
									mBIcon.mbSetCell(convertIDXProtectionSysToIcon((int)equipmentItem.protectionSystem()));
									if (mBText != null)
									{
										itm.ProtectionParameter protectionParameter = itm.ItemManager.instance().protectionParameter(equipmentItem.itemId());
										if (protectionParameter != null)
										{
											mBText.mbSetBufferNumber(protectionParameter.phylacticPower());
										}
									}
								}
								else
								{
									mBIcon.mbSetCell(43);
									mBText?.mbSetTextMsgNo(50436);
								}
							}
							medget3 = medget3.nextSibling();
						}
					}
					medget = medget.nextSibling();
				}
				for (Medget medget4 = ownerMedget.childNode(); medget4 != null; medget4 = medget4.nextSibling())
				{
					SET_TXT_NAME(medget4, (MBText)medget4.childNode().behavior().queryInterface(MBText.classIdentifier()));
					SET_TXT_NUM(medget4, (MBText)medget4.childNode().nextSibling().behavior()
						.queryInterface(MBText.classIdentifier()));
				}
				Medget nodeByID2 = ownerMedget.parentNode().getNodeByID(TRANSCODE("caption"));
				if (nodeByID2 != null)
				{
					_mbCaptionText = (MBText)nodeByID2.behavior().queryInterface(MBText.classIdentifier());
				}
				setupItemParameter();
				MenuManager.getSingleton().initFocus(0);
				MenuManager.getSingleton().SetTargetItemNo(_ItemList[MenuManager.getSingleton().getFocuseMedget().myTag()]._ItemId);
				updateDisplay();
				updateInfluence();
				_Init = true;
			}

			public override void bmBehave(Medget M)
			{
			}

			public override void bmFinalize(Medget M)
			{
				releaseItemMessage();
				influenceVisibility(v: false);
				for (int i = 0; i < 4; i++)
				{
					pl.Player player = pl.PlayerParty.instance().player((byte)i);
					if (player.isEnable() && !player.condition().isFrog() && characterMng.getMotionIndex(shop.CShopUpDisplayComposition.Instance().charIndex(i)) != 4101 + player.playerId())
					{
						characterMng.startMotion(shop.CShopUpDisplayComposition.Instance().charIndex(i), 4101 + player.playerId(), fLoop: true, 5u);
					}
				}
				for (int j = 0; j < 4; j++)
				{
					if (shop.CShopUpDisplayComposition.Instance().charIndex(j) >= 0)
					{
						characterMng.setEmission(shop.CShopUpDisplayComposition.Instance().charIndex(j), enable_emission);
						characterMng.setLight(shop.CShopUpDisplayComposition.Instance().charIndex(j), _savedLight[j]);
					}
				}
			}

			public override void bmSuspend(Medget M)
			{
				for (Medget medget = ownerMedget.childNode(); medget != null; medget = medget.nextSibling())
				{
					GET_TXT_NUM(medget).bmTextVisibility(v: false);
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
				flagOn(1);
			}

			public override void bmResume(Medget M)
			{
				for (Medget medget = ownerMedget.childNode(); medget != null; medget = medget.nextSibling())
				{
					GET_TXT_NUM(medget).bmTextVisibility(v: true);
					GET_TXT_NAME(medget).mbSetTextColor(dgs.TXT_COLOR.TXT_COLOR_WHITE);
				}
				flagOff(1);
				updatePossessionNumber();
				_savedMedget = null;
			}

			public override bool bmDecide(Medget M)
			{
				bool result = true;
				itm.ItemBaseParameter itemBaseParameter = itm.ItemManager.instance().itemParameter((short)MenuManager.getSingleton().GetTargetItemNo());
				if (itemBaseParameter == null)
				{
					return result;
				}
				itm.NotImportantParameter notImportantParameter = static_cast<itm.NotImportantParameter>(itemBaseParameter);
				if (notImportantParameter.buy() > pl.PlayerParty.instance().gold().get())
				{
					MenuManager.getSingleton().playSEBeep();
					return result;
				}
				itm.PossessionItem possessionItem = pl.PlayerParty.instance().item().serchNormalItem((short)MenuManager.getSingleton().GetTargetItemNo());
				if (possessionItem != null && possessionItem.itemNumber() >= 99)
				{
					MenuManager.getSingleton().playSEBeep();
					return result;
				}
				_savedMedget = MenuManager.getSingleton().getFocuseMedget();
				for (Medget medget = ownerMedget.childNode(); medget != null; medget = medget.nextSibling())
				{
					if (MenuManager.getSingleton().getFocuseMedget() != medget && GET_TXT_NAME(medget) != null)
					{
						GET_TXT_NAME(medget).mbSetTextColor(dgs.TXT_COLOR.TXT_UCOLOR_4);
					}
				}
				flagOn(1);
				MenuManager.getSingleton().SetDecideButtonState(0);
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
				if ((ds.g_Pad.repeat() & 0x40) != 0)
				{
					if (M.prevSibling() != null && _ItemList[M.prevSibling().myTag()]._ItemId > 0)
					{
						medget = MenuManager.getSingleton().initFocusM(M.prevSibling());
					}
					else
					{
						Medget medget2 = null;
						medget2 = ownerMedget.childNode();
						while (medget2 != null && medget2.nextSibling() != null && _ItemList[medget2.nextSibling().myTag()]._ItemId > 0)
						{
							medget2 = medget2.nextSibling();
						}
						medget = MenuManager.getSingleton().initFocusM(medget2);
					}
				}
				else
				{
					if ((ds.g_Pad.repeat() & 0x80) == 0)
					{
						return true;
					}
					medget = ((M.nextSibling() == null || _ItemList[M.nextSibling().myTag()]._ItemId <= 0) ? MenuManager.getSingleton().initFocusM(ownerMedget.childNode()) : MenuManager.getSingleton().initFocusM(M.nextSibling()));
				}
				_savedMedget = MenuManager.getSingleton().getFocuseMedget();
				MenuManager.getSingleton().playSEMoveCursor();
				MenuManager.getSingleton().SetTargetItemNo(_ItemList[MenuManager.getSingleton().getFocuseMedget().myTag()]._ItemId);
				updateInfluence();
				if (medget != null && GET_TXT_NAME(medget) != null)
				{
					GET_TXT_NAME(medget).mbSetTextColor(dgs.TXT_COLOR.TXT_COLOR_WHITE);
				}
				if (GET_TXT_NAME(MenuManager.getSingleton().getFocuseMedget()) != null)
				{
					GET_TXT_NAME(MenuManager.getSingleton().getFocuseMedget()).mbSetTextColor(dgs.TXT_COLOR.TXT_COLOR_YELLOW);
				}
				return true;
			}

			public void influenceVisibility(bool v)
			{
				Medget nodeByID = ownerMedget.parentNode().getNodeByID(TRANSCODE("influence"));
				nodeByID = nodeByID.childNode();
				for (int i = 0; i < 4; i++)
				{
					if (pl.PlayerParty.instance().player((byte)i).isEnable())
					{
						Medget medget = nodeByID.childNode();
						((MBText)medget.behavior().queryInterface(MBText.classIdentifier()))?.bmTextVisibility(v);
					}
					nodeByID = nodeByID.nextSibling();
				}
			}

			public void updateInfluence()
			{
				itm.CATEGORY cATEGORY = itm.ItemManager.instance().itemCategory((short)MenuManager.getSingleton().GetTargetItemNo());
				Medget nodeByID = ownerMedget.parentNode().getNodeByID(TRANSCODE("influence"));
				Medget focuseMedget = MenuManager.getSingleton().getFocuseMedget();
				shop.CShopManager.Instance().pCurrentShop().setItemPos(new ds.Vector2<short>(focuseMedget.x(), (short)(focuseMedget.y() + (focuseMedget.height() - 16) / 2)));
				if (_mbCaptionText != null)
				{
					int msg_number = itm.ItemManager.instance().itemParameter((short)MenuManager.getSingleton().GetTargetItemNo()).captionId();
					_mbCaptionText.mbSetBufferMsg(dgs.msg.CMessageSys.getInstance().Sub().getMessage((uint)msg_number), decWidth: false);
				}
				int num = -1;
				switch (cATEGORY)
				{
				case itm.CATEGORY.CATEGORY_WEAPON:
				{
					itm.WeaponParameter weaponParameter = itm.ItemManager.instance().weaponParameter((short)MenuManager.getSingleton().GetTargetItemNo());
					if (weaponParameter != null)
					{
						num = weaponParameter.equipJob();
					}
					break;
				}
				case itm.CATEGORY.CATEGORY_PROTECTION:
				{
					itm.ProtectionParameter protectionParameter = itm.ItemManager.instance().protectionParameter((short)MenuManager.getSingleton().GetTargetItemNo());
					if (protectionParameter == null)
					{
						break;
					}
					num = protectionParameter.equipJob();
					Medget medget = nodeByID.childNode();
					for (int i = 0; i < 4; i++)
					{
						if (pl.PlayerParty.instance().player((byte)i).isEnable())
						{
							pl.PlayerEquipParameter playerEquipParameter = pl.PlayerParty.instance().player((byte)i).equipParameter();
							Medget medget2 = medget.childNode().nextSibling().childNode();
							((MBIcon)medget2.behavior().queryInterface(MBIcon.classIdentifier()))?.mbSetCell(convertIDXProtectionSysToIcon(protectionParameter.system()));
							int num2 = 0;
							switch (protectionParameter.system())
							{
							case 0:
							{
								itm.ProtectionParameter protectionParameter5 = itm.ItemManager.instance().protectionParameter(playerEquipParameter.equipHand(pl.HAND_TYPE.RIGHT_HAND).itemId());
								if (protectionParameter5 != null)
								{
									num2 = protectionParameter5.phylacticPower();
								}
								protectionParameter5 = itm.ItemManager.instance().protectionParameter(playerEquipParameter.equipHand(pl.HAND_TYPE.LEFT_HAND).itemId());
								if (protectionParameter5 != null && num2 < protectionParameter5.phylacticPower())
								{
									num2 = protectionParameter5.phylacticPower();
								}
								break;
							}
							case 1:
							{
								itm.ProtectionParameter protectionParameter3 = itm.ItemManager.instance().protectionParameter(playerEquipParameter.equipHead().itemId());
								if (protectionParameter3 != null)
								{
									num2 = protectionParameter3.phylacticPower();
								}
								break;
							}
							case 2:
							{
								itm.ProtectionParameter protectionParameter4 = itm.ItemManager.instance().protectionParameter(playerEquipParameter.equipBody().itemId());
								if (protectionParameter4 != null)
								{
									num2 = protectionParameter4.phylacticPower();
								}
								break;
							}
							case 3:
							{
								itm.ProtectionParameter protectionParameter2 = itm.ItemManager.instance().protectionParameter(playerEquipParameter.equipArm().itemId());
								if (protectionParameter2 != null)
								{
									num2 = protectionParameter2.phylacticPower();
								}
								break;
							}
							}
							medget2 = medget2.nextSibling();
							MBText mBText = (MBText)medget2.behavior().queryInterface(MBText.classIdentifier());
							if (mBText != null)
							{
								if (num2 > 0)
								{
									mBText.mbSetBufferNumber(num2);
								}
								else
								{
									mBText.mbSetTextMsgNo(50436);
								}
							}
						}
						medget = medget.nextSibling();
					}
					break;
				}
				case itm.CATEGORY.CATEGORY_MAGIC:
				{
					itm.MagicParameter magicParameter = itm.ItemManager.instance().magicParameter((short)MenuManager.getSingleton().GetTargetItemNo());
					if (magicParameter != null)
					{
						num = magicParameter.equipJob();
					}
					break;
				}
				}
				if (num >= 0)
				{
					for (int j = 0; j < 4; j++)
					{
						pl.Player player = pl.PlayerParty.instance().player((byte)j);
						if (player.isEquipItem(num))
						{
							NNS_G2dSetBGCellColor(8 + player.playerId(), 255, 255, 255, 255);
							if (characterMng.getMotionIndex(shop.CShopUpDisplayComposition.Instance().charIndex(j)) != 4301 + player.playerId())
							{
								characterMng.startMotion(shop.CShopUpDisplayComposition.Instance().charIndex(j), 4301 + player.playerId(), fLoop: true, 5u);
							}
							characterMng.setEmission(shop.CShopUpDisplayComposition.Instance().charIndex(j), enable_emission);
						}
						else
						{
							NNS_G2dSetBGCellColor(8 + player.playerId(), 128, 128, 128, 255);
							if (characterMng.getMotionIndex(shop.CShopUpDisplayComposition.Instance().charIndex(j)) != 4101 + player.playerId())
							{
								characterMng.startMotion(shop.CShopUpDisplayComposition.Instance().charIndex(j), 4101 + player.playerId(), fLoop: true, 5u);
							}
							characterMng.setEmission(shop.CShopUpDisplayComposition.Instance().charIndex(j), disable_emission);
						}
					}
				}
				GXS_SetVisibleWnd(0);
				updatePossessionNumber();
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

			public override void bmActivate(Medget M)
			{
				if (!_Init || M == ownerMedget)
				{
					return;
				}
				if (MenuManager.getSingleton().GetTargetItemNo() != _ItemList[MenuManager.getSingleton().getFocuseMedget().myTag()]._ItemId)
				{
					MenuManager.getSingleton().SetTargetItemNo(_ItemList[MenuManager.getSingleton().getFocuseMedget().myTag()]._ItemId);
					MenuManager.getSingleton().playSEMoveCursor();
				}
				Medget medget = null;
				for (medget = ownerMedget.childNode(); medget != null; medget = medget.nextSibling())
				{
					itm.NotImportantParameter notImportantParameter = static_cast<itm.NotImportantParameter>(itm.ItemManager.instance().itemParameter((short)_ItemList[medget.myTag()]._ItemId));
					dgs.TXT_COLOR color = ((notImportantParameter != null && notImportantParameter.buy() <= pl.PlayerParty.instance().gold().get()) ? dgs.TXT_COLOR.TXT_COLOR_WHITE : dgs.TXT_COLOR.TXT_UCOLOR_4);
					if (GET_TXT_NAME(medget) != null)
					{
						GET_TXT_NAME(medget).mbSetTextColor(dgs.TXT_COLOR.TXT_COLOR_WHITE);
					}
					if (GET_TXT_NUM(medget) != null)
					{
						GET_TXT_NUM(medget).mbSetTextColor(color);
					}
				}
				updateInfluence();
				if (GET_TXT_NAME(MenuManager.getSingleton().getFocuseMedget()) != null)
				{
					GET_TXT_NAME(MenuManager.getSingleton().getFocuseMedget()).mbSetTextColor(dgs.TXT_COLOR.TXT_COLOR_YELLOW);
					_savedMedget = MenuManager.getSingleton().getFocuseMedget();
				}
			}

			public override void bmDeactivate(Medget M)
			{
			}

			public void reset()
			{
				_Init = false;
				_ScrollBarFlag = false;
				_SavedVisibility = false;
				_Width.initialize();
				_Height.initialize();
				_MaxItemNum = 0;
				_ItemNum = 0;
				_PrevItem = -1;
				for (int i = 0; i < 12; i++)
				{
					_ItemList[i]._ItemId = -1;
					_ItemList[i]._ItemCellAnim = -1;
				}
				_mbCaptionText = null;
			}

			public void updateDisplay()
			{
				releaseItemMessage();
				setupItemParameter();
				createItemMessage();
			}

			public void setupItemParameter()
			{
				_ItemNum = 0;
				for (int i = 0; i < 12; i++)
				{
					_ItemList[i]._ItemId = -1;
				}
				int num = 0;
				for (int j = 0; j < 12 && shop.CShopManager.Instance().ShopParameterMng().ShopParameter(shop.CShopManager.Instance().ShopIndex())
					.ItemId(j) > 0; j++)
				{
					_ItemList[_ItemNum]._ItemId = shop.CShopManager.Instance().ShopParameterMng().ShopParameter(shop.CShopManager.Instance().ShopIndex())
						.ItemId(j);
					num = itm.ItemManager.instance().itemParameter((short)_ItemList[_ItemNum]._ItemId).system();
					switch (itm.ItemManager.instance().itemCategory((short)_ItemList[_ItemNum]._ItemId))
					{
					case itm.CATEGORY.CATEGORY_WEAPON:
						_ItemList[_ItemNum]._ItemCellAnim = convertIDXWeaponSysToIcon(num);
						break;
					case itm.CATEGORY.CATEGORY_PROTECTION:
						_ItemList[_ItemNum]._ItemCellAnim = convertIDXProtectionSysToIcon(num);
						break;
					case itm.CATEGORY.CATEGORY_MAGIC:
						_ItemList[_ItemNum]._ItemCellAnim = convertIDXMagicSysToIcon(num);
						break;
					default:
						_ItemList[_ItemNum]._ItemCellAnim = 45;
						break;
					}
					_ItemNum++;
				}
				_Width._OneLine = 1;
				_Width._MaxLine = 1;
				_Height._OneLine = 8;
				_Height._MaxLine = _ItemNum / _Width._OneLine;
				if (_ItemNum % _Width._OneLine != 0)
				{
					_Height._MaxLine++;
				}
			}

			public void createItemMessage()
			{
				int num = 0;
				dgs.DGSMessageManager dGSMessageManager = null;
				int num2 = _Height._NowLine * _Width._OneLine;
				MenuManager.getSingleton().clearFocusList();
				int num3 = 0;
				Medget medget = ownerMedget.childNode();
				while (medget != null && _ItemList[num3 + num2]._ItemId != -1)
				{
					MenuManager.getSingleton().joinFocusList(medget);
					dGSMessageManager = dgs.msg.CMessageSys.getInstance().Sub();
					itm.ItemBaseParameter itemBaseParameter = itm.ItemManager.instance().itemParameter((short)_ItemList[num3 + num2]._ItemId);
					string message = dGSMessageManager.getMessage((uint)itemBaseParameter.nameId());
					GET_TXT_NAME(medget).mbSetBufferMsg(const_cast<string>(message), decWidth: true);
					GET_TXT_NAME(medget).bmTextVisibility(v: true);
					Medget medget2 = medget.childNode();
					_ItemList[num3 + num2]._Icon.copy(MenuManager.getSingleton().GetSmallIcon2d());
					_ItemList[num3 + num2]._Icon.SetCell((ushort)_ItemList[num3 + num2]._ItemCellAnim);
					sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(_ItemList[num3 + num2]._Icon);
					_ItemList[num3 + num2]._Icon.SetPositionI(medget2.x() - 16, medget2.y() + (medget2.height() - 16) / 2);
					_ItemList[num3 + num2]._Icon.SetShow(show: true);
					GET_TXT_NAME(medget).getMessage().setPriority(3);
					_ItemList[num3 + num2]._Icon.SetPriority(3);
					itm.NotImportantParameter notImportantParameter = static_cast<itm.NotImportantParameter>(itemBaseParameter);
					num = notImportantParameter.buy();
					GET_TXT_NUM(medget).mbSetBufferNumber(num);
					GET_TXT_NUM(medget).bmTextVisibility(v: true);
					num3++;
					medget = medget.nextSibling();
				}
			}

			public void releaseItemMessage()
			{
				for (int i = 0; i < 12; i++)
				{
					if (_ItemList[i]._ItemId != -1)
					{
						_ItemList[i]._Icon.Release();
						sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(_ItemList[i]._Icon);
					}
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

			public new static int classIdentifier()
			{
				return MBShopBuyList_UN.number();
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
