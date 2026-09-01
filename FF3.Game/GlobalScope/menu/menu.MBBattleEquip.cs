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
		public class MBBattleEquip : MenuBehavior
		{
			public const int CURRENT_ATTACK = 0;

			public const int CURRENT_PROTECT = 1;

			public const int NEXT_ATTACK = 2;

			public const int NEXT_PROTECT = 3;

			public const int LEFT_ITEM = 4;

			public const int RIGHT_ITEM = 5;

			public const int LEFT_NUMBER = 6;

			public const int RIGHT_NUMBER = 7;

			public const int MESSAGE_MAX = 8;

			public static dgs.UniqueNumber MBBattleEquip_UN = new dgs.UniqueNumber();

			private dgs.DGSMessage[] message_ = new dgs.DGSMessage[8];

			private sys2d.Sprite3d attackCell = new sys2d.Sprite3d();

			private sys2d.Sprite3d protectCell = new sys2d.Sprite3d();

			public MBBattleEquip()
			{
				for (int i = 0; i < 8; i++)
				{
					message_[i] = null;
				}
			}

			~MBBattleEquip()
			{
				int i = 0;
				for (; i < 8; i++)
				{
					if (message_[i] != null)
					{
						message_[i].release();
						message_[i] = null;
					}
				}
			}

			public override void bmInitialize(Medget M)
			{
				for (int i = 0; i < 8; i++)
				{
					message_[i] = null;
				}
				dgs.DGSMessageManager dGSMessageManager = null;
				dGSMessageManager = ((M.display() != 1) ? dgs.msg.CMessageSys.getInstance().Sub() : dgs.msg.CMessageSys.getInstance().Main());
				attackCell.copy(MenuManager.getSingleton().GetSmallIcon3d());
				attackCell.SetCell(11);
				attackCell.SetShow(show: false);
				sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(attackCell);
				protectCell.copy(MenuManager.getSingleton().GetSmallIcon3d());
				protectCell.SetCell(11);
				protectCell.SetShow(show: false);
				sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(protectCell);
				CreateEquipmentWeapon(M, dGSMessageManager, MenuManager.getSingleton().GetTargetCharNo());
			}

			public override void bmBehave(Medget M)
			{
			}

			public void bmRefresh(Medget pM)
			{
			}

			public override void bmFinalize(Medget M)
			{
				for (int i = 0; i < 8; i++)
				{
					if (message_[i] != null)
					{
						message_[i].release();
						message_[i] = null;
					}
				}
				attackCell.Release();
				sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(attackCell);
				protectCell.Release();
				sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(protectCell);
			}

			public override bool bmDecide(Medget M)
			{
				MenuManager.getSingleton().SetDecideButtonState(0);
				return false;
			}

			public override void bmActivate(Medget M)
			{
				if (!ownerMedget._id(M._id()))
				{
					int num = pl.PlayerParty.instance().player((byte)MenuManager.getSingleton().GetTargetCharNo()).equipParameter()
						.equipHand((pl.HAND_TYPE)M.myTag())
						.itemId();
					if (num <= 0)
					{
						MenuManager.getSingleton().SetTargetItemNo(-1);
					}
					else
					{
						MenuManager.getSingleton().SetTargetItemNo(num);
					}
				}
			}

			public override void bmDeactivate(Medget M)
			{
			}

			public override bool bmCancel(Medget M)
			{
				MenuManager.getSingleton().SetCancelButtonState(0);
				return false;
			}

			public override bool bmDirection(Medget M, int key)
			{
				return MenuManager.getSingleton().MoveCursor(M, PlaySe: true);
			}

			public void CreateEquipmentWeapon(Medget M, dgs.DGSMessageManager pm, int charNo)
			{
				int num = pl.PlayerParty.instance().player((byte)charNo).equipParameter()
					.equipHand(pl.HAND_TYPE.RIGHT_HAND)
					.itemId();
				int num2 = pl.PlayerParty.instance().player((byte)charNo).equipParameter()
					.equipHand(pl.HAND_TYPE.RIGHT_HAND)
					.equipNumber()
					.get();
				Medget medget = M.childNode();
				string after;
				if (num > 0)
				{
					itm.ItemBaseParameter itemBaseParameter = itm.ItemManager.instance().itemParameter((short)num);
					int msg_number = itemBaseParameter.nameId();
					message_[5] = pm.createMessage((uint)msg_number, MenuManager.getSingleton().GetItemDataTextNo(), 1);
					message_[5].setPosition(medget.x(), (short)(medget.y() + (medget.height() - 12) / 2), erase: true);
					message_[5].setDisplaySpeed(byte.MaxValue);
					message_[5].setDisplayWait(0);
					dgs.msg.CMessageSys.getInstance().changeValueFont(num2, out after);
					if (num != 1000)
					{
						message_[7] = pm.createMessage(after, 1);
						ds.Vector2<short> vector = new ds.Vector2<short>();
						message_[7].getTextSize(vector);
						message_[7].setPosition((short)(medget.x() + 160 - vector.vx), (short)(medget.y() + (medget.height() - 12) / 2), erase: true);
						message_[7].setDisplaySpeed(byte.MaxValue);
						message_[7].setDisplayWait(0);
					}
					medget.setWork1(num);
					medget.setWork2(num2);
				}
				else
				{
					int msg_number2 = 50402;
					message_[5] = pm.createMessage((uint)msg_number2, MenuManager.getSingleton().GetMenuDataTextNo(), 1);
					message_[5].setPosition(medget.x(), (short)(medget.y() + (medget.height() - 12) / 2), erase: true);
					message_[5].setDisplaySpeed(byte.MaxValue);
					message_[5].setDisplayWait(0);
					medget.setWork1(-1);
					medget.setWork2(0);
				}
				medget = medget.nextSibling();
				int num3 = pl.PlayerParty.instance().player((byte)charNo).equipParameter()
					.equipHand(pl.HAND_TYPE.LEFT_HAND)
					.itemId();
				int num4 = pl.PlayerParty.instance().player((byte)charNo).equipParameter()
					.equipHand(pl.HAND_TYPE.LEFT_HAND)
					.equipNumber()
					.get();
				if (num3 > 0)
				{
					itm.ItemBaseParameter itemBaseParameter2 = itm.ItemManager.instance().itemParameter((short)num3);
					int msg_number3 = itemBaseParameter2.nameId();
					message_[4] = pm.createMessage((uint)msg_number3, MenuManager.getSingleton().GetItemDataTextNo(), 1);
					message_[4].setPosition(medget.x(), (short)(medget.y() + (medget.height() - 12) / 2), erase: true);
					message_[4].setDisplaySpeed(byte.MaxValue);
					message_[4].setDisplayWait(0);
					dgs.msg.CMessageSys.getInstance().changeValueFont(num4, out after);
					if (num3 != 1000)
					{
						message_[6] = pm.createMessage(after, 1);
						ds.Vector2<short> vector2 = new ds.Vector2<short>();
						message_[6].getTextSize(vector2);
						message_[6].setPosition((short)(medget.x() + 160 - vector2.vx), (short)(medget.y() + (medget.height() - 12) / 2), erase: true);
						message_[6].setDisplaySpeed(byte.MaxValue);
						message_[6].setDisplayWait(0);
					}
					medget.setWork1(num3);
					medget.setWork2(num4);
				}
				else
				{
					int msg_number4 = 50403;
					message_[4] = pm.createMessage((uint)msg_number4, MenuManager.getSingleton().GetMenuDataTextNo(), 1);
					message_[4].setPosition(medget.x(), (short)(medget.y() + (medget.height() - 12) / 2), erase: true);
					message_[4].setDisplaySpeed(byte.MaxValue);
					message_[4].setDisplayWait(0);
					medget.setWork1(-1);
					medget.setWork2(0);
				}
				medget = medget.nextSibling();
				int num5 = pl.PlayerParty.instance().player((byte)charNo).handAttack(pl.HAND_TYPE.RIGHT_HAND)
					.aggressivity()
					.get() + pl.PlayerParty.instance().player((byte)charNo).handAttack(pl.HAND_TYPE.LEFT_HAND)
					.aggressivity()
					.get();
				if ((pl.PlayerParty.instance().player((byte)charNo).condition()
					.isFrog() || pl.PlayerParty.instance().player((byte)charNo).condition()
					.isLilliput()) && num5 > 0)
				{
					num5 = 1;
				}
				dgs.msg.CMessageSys.getInstance().changeValueFont(num5, out var after2);
				message_[0] = pm.createMessage(after2, 1);
				ds.Vector2<short> vector3 = new ds.Vector2<short>();
				message_[0].getTextSize(vector3);
				message_[0].setPosition((short)(medget.x() + 24 - vector3.vx), medget.y(), erase: true);
				message_[0].setDisplaySpeed(byte.MaxValue);
				message_[0].setDisplayWait(0);
				medget = medget.nextSibling();
				int num6 = pl.PlayerParty.instance().player((byte)charNo).physicsDefense()
					.phylacticPower()
					.get();
				if ((pl.PlayerParty.instance().player((byte)charNo).condition()
					.isFrog() || pl.PlayerParty.instance().player((byte)charNo).condition()
					.isLilliput()) && num6 > 0)
				{
					num6 = 1;
				}
				dgs.msg.CMessageSys.getInstance().changeValueFont(num6, out after2);
				message_[1] = pm.createMessage(after2, 1);
				message_[1].getTextSize(vector3);
				message_[1].setPosition((short)(medget.x() + 24 - vector3.vx), medget.y(), erase: true);
				message_[1].setDisplaySpeed(byte.MaxValue);
				message_[1].setDisplayWait(0);
				medget = medget.nextSibling();
				int value = num5;
				dgs.msg.CMessageSys.getInstance().changeValueFont(value, out after2);
				message_[2] = pm.createMessage(after2, 1);
				message_[2].getTextSize(vector3);
				message_[2].setPosition((short)(medget.x() + 24 - vector3.vx), medget.y(), erase: true);
				message_[2].setDisplaySpeed(byte.MaxValue);
				message_[2].setDisplayWait(0);
				medget = medget.nextSibling();
				int value2 = num6;
				dgs.msg.CMessageSys.getInstance().changeValueFont(value2, out after2);
				message_[3] = pm.createMessage(after2, 1);
				message_[3].getTextSize(vector3);
				message_[3].setPosition((short)(medget.x() + 24 - vector3.vx), medget.y(), erase: true);
				message_[3].setDisplaySpeed(byte.MaxValue);
				message_[3].setDisplayWait(0);
				medget = medget.nextSibling();
				attackCell.SetPositionI(medget.x(), medget.y());
				medget = medget.nextSibling();
				protectCell.SetPositionI(medget.x(), medget.y());
			}

			public void updateUpDownTriangle(Medget M, dgs.DGSMessageManager pm, int points, short item_id)
			{
				int targetCharNo = MenuManager.getSingleton().GetTargetCharNo();
				int num = pl.PlayerParty.instance().player((byte)targetCharNo).handAttack(pl.HAND_TYPE.RIGHT_HAND)
					.aggressivity()
					.get() + pl.PlayerParty.instance().player((byte)targetCharNo).handAttack(pl.HAND_TYPE.LEFT_HAND)
					.aggressivity()
					.get();
				int num2 = pl.PlayerParty.instance().player((byte)targetCharNo).physicsDefense()
					.phylacticPower()
					.get();
				if (pl.PlayerParty.instance().player((byte)targetCharNo).condition()
					.isFrog() || pl.PlayerParty.instance().player((byte)targetCharNo).condition()
					.isLilliput())
				{
					if (num > 0)
					{
						num = 1;
					}
					if (num2 > 0)
					{
						num2 = 1;
					}
				}
				int num3 = 0;
				int num4 = 0;
				Medget nodeByIDFromChildren = M.getNodeByIDFromChildren(TRANSCODE("next_attack"));
				Medget nodeByIDFromChildren2 = M.getNodeByIDFromChildren(TRANSCODE("next_protect"));
				string after = "";
				ds.Vector2<short> vector = new ds.Vector2<short>();
				bool flag = false;
				itm.ItemBaseParameter itemBaseParameter = itm.ItemManager.instance().itemParameter(item_id);
				itm.CATEGORY cATEGORY = itm.ItemManager.instance().itemCategory(item_id);
				itm.WeaponParameter weaponParameter = itm.ItemManager.instance().weaponParameter(item_id);
				itm.ProtectionParameter protectionParameter = itm.ItemManager.instance().protectionParameter(item_id);
				short num5 = pl.PlayerParty.instance().item().serchNormalItem(item_id)
					.itemNumber();
				int jobFlag = 0;
				if (weaponParameter != null)
				{
					jobFlag = weaponParameter.equipJob();
				}
				else if (protectionParameter != null)
				{
					jobFlag = protectionParameter.equipJob();
				}
				if (itemBaseParameter == null || num5 == 0)
				{
					pl.EquipItemInfo itemInfo = pl.PlayerParty.instance().player((byte)targetCharNo).equipParameter()
						.equipPoint(points)
						.release();
					pl.PlayerParty.instance().player((byte)targetCharNo).updateParameter();
					num3 = pl.PlayerParty.instance().player((byte)targetCharNo).handAttack(pl.HAND_TYPE.RIGHT_HAND)
						.aggressivity()
						.get() + pl.PlayerParty.instance().player((byte)targetCharNo).handAttack(pl.HAND_TYPE.LEFT_HAND)
						.aggressivity()
						.get();
					num4 = pl.PlayerParty.instance().player((byte)targetCharNo).physicsDefense()
						.phylacticPower()
						.get();
					if (pl.PlayerParty.instance().player((byte)targetCharNo).condition()
						.isFrog() || pl.PlayerParty.instance().player((byte)targetCharNo).condition()
						.isLilliput())
					{
						if (num3 > 0)
						{
							num3 = 1;
						}
						if (num4 > 0)
						{
							num4 = 1;
						}
					}
					if (num == num3 && num2 == num4)
					{
						flag = true;
					}
					pl.PlayerParty.instance().player((byte)targetCharNo).equipParameter()
						.equipPoint(points)
						.equip(itemInfo);
					pl.PlayerParty.instance().player((byte)targetCharNo).updateParameter();
				}
				else if (cATEGORY == itm.CATEGORY.CATEGORY_PROTECTION && protectionParameter.system() != 0)
				{
					flag = true;
				}
				else if (!pl.PlayerParty.instance().player((byte)targetCharNo).isEquipItem(jobFlag))
				{
					flag = true;
				}
				else
				{
					pl.EquipItemInfo itemInfo2 = null;
					pl.EquipItemInfo itemInfo3 = pl.PlayerParty.instance().player((byte)targetCharNo).equipParameter()
						.equipPoint(points)
						.equipItemInfo();
					if (points == 0 || points == 1)
					{
						itemInfo2 = ((points != 0) ? pl.PlayerParty.instance().player((byte)targetCharNo).equipParameter()
							.equipPoint(0)
							.equipItemInfo() : pl.PlayerParty.instance().player((byte)targetCharNo).equipParameter()
							.equipPoint(1)
							.equipItemInfo());
					}
					pl.PlayerParty.instance().player((byte)targetCharNo).doEquip(points, item_id, sort: false);
					pl.PlayerParty.instance().player((byte)targetCharNo).updateParameter();
					num3 = pl.PlayerParty.instance().player((byte)targetCharNo).handAttack(pl.HAND_TYPE.RIGHT_HAND)
						.aggressivity()
						.get() + pl.PlayerParty.instance().player((byte)targetCharNo).handAttack(pl.HAND_TYPE.LEFT_HAND)
						.aggressivity()
						.get();
					num4 = pl.PlayerParty.instance().player((byte)targetCharNo).physicsDefense()
						.phylacticPower()
						.get();
					if (pl.PlayerParty.instance().player((byte)targetCharNo).condition()
						.isFrog() || pl.PlayerParty.instance().player((byte)targetCharNo).condition()
						.isLilliput())
					{
						if (num3 > 0)
						{
							num3 = 1;
						}
						if (num4 > 0)
						{
							num4 = 1;
						}
					}
					if (num == num3 && num2 == num4)
					{
						flag = true;
					}
					pl.PlayerParty.instance().player((byte)targetCharNo).equipParameter()
						.equipPoint(points)
						.equip(itemInfo3);
					if (points == 0 || points == 1)
					{
						if (points == 0)
						{
							itemInfo2 = pl.PlayerParty.instance().player((byte)targetCharNo).equipParameter()
								.equipPoint(1)
								.equip(itemInfo2);
						}
						else
						{
							itemInfo2 = pl.PlayerParty.instance().player((byte)targetCharNo).equipParameter()
								.equipPoint(0)
								.equip(itemInfo2);
						}
					}
					pl.PlayerParty.instance().player((byte)targetCharNo).updateParameter();
				}
				if (flag)
				{
					message_[2].release();
					message_[2] = null;
					dgs.msg.CMessageSys.getInstance().changeValueFont(num, out after);
					message_[2] = pm.createMessage(after, 1);
					message_[2].getTextSize(vector);
					message_[2].setPosition((short)(nodeByIDFromChildren.x() + 24 - vector.vx), nodeByIDFromChildren.y(), erase: true);
					message_[2].setDisplaySpeed(byte.MaxValue);
					message_[2].setDisplayWait(0);
					message_[3].release();
					message_[3] = null;
					dgs.msg.CMessageSys.getInstance().changeValueFont(num2, out after);
					message_[3] = pm.createMessage(after, 1);
					message_[3].getTextSize(vector);
					message_[3].setPosition((short)(nodeByIDFromChildren2.x() + 24 - vector.vx), nodeByIDFromChildren2.y(), erase: true);
					message_[3].setDisplaySpeed(byte.MaxValue);
					message_[3].setDisplayWait(0);
					attackCell.SetShow(show: false);
					protectCell.SetShow(show: false);
					return;
				}
				message_[2].release();
				message_[2] = null;
				dgs.msg.CMessageSys.getInstance().changeValueFont(num3, out after);
				message_[2] = pm.createMessage(after, 1);
				message_[2].getTextSize(vector);
				message_[2].setPosition((short)(nodeByIDFromChildren.x() + 24 - vector.vx), nodeByIDFromChildren.y(), erase: true);
				message_[2].setDisplaySpeed(byte.MaxValue);
				message_[2].setDisplayWait(0);
				message_[3].release();
				message_[3] = null;
				dgs.msg.CMessageSys.getInstance().changeValueFont(num4, out after);
				message_[3] = pm.createMessage(after, 1);
				message_[3].getTextSize(vector);
				message_[3].setPosition((short)(nodeByIDFromChildren2.x() + 24 - vector.vx), nodeByIDFromChildren2.y(), erase: true);
				message_[3].setDisplaySpeed(byte.MaxValue);
				message_[3].setDisplayWait(0);
				if (num < num3)
				{
					attackCell.SetCell(11);
					attackCell.SetShow(show: true);
				}
				else if (num > num3)
				{
					attackCell.SetCell(12);
					attackCell.SetShow(show: true);
				}
				else
				{
					attackCell.SetShow(show: false);
				}
				if (num2 < num4)
				{
					protectCell.SetCell(11);
					protectCell.SetShow(show: true);
				}
				else if (num2 > num4)
				{
					protectCell.SetCell(12);
					protectCell.SetShow(show: true);
				}
				else
				{
					protectCell.SetShow(show: false);
				}
			}

			public void releaseMessageAndHiddenTriangle(Medget M)
			{
				for (int i = 0; i < 8; i++)
				{
					if (message_[i] != null)
					{
						message_[i].release();
						message_[i] = null;
					}
				}
				attackCell.SetShow(show: false);
				protectCell.SetShow(show: false);
			}

			public new static int classIdentifier()
			{
				return MBBattleEquip_UN.number();
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
