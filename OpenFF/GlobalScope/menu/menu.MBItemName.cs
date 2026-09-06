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
		public class MBItemName : MenuBehavior
		{
			public static dgs.UniqueNumber MBItemName_UN = new dgs.UniqueNumber();

			protected int itemID;

			protected dgs.SmartPtr<dgs.DGSMessage> message = new dgs.SmartPtr<dgs.DGSMessage>();

			protected sys2d.Cell icon = new sys2d.Cell();

			public MBItemName()
			{
				itemID ^= itemID;
			}

			~MBItemName()
			{
				icon.Release();
			}

			public override void bmInitialize(Medget M)
			{
				XbnNode firstNodeByTagNameFromChildren = M.node().getFirstNodeByTagNameFromChildren(TRANSCODE("behavior"));
				if (firstNodeByTagNameFromChildren == null)
				{
					return;
				}
				XbnNodeList xbnNodeList = new XbnNodeList();
				firstNodeByTagNameFromChildren.getNodesByTagNameFromChildren(TRANSCODE("parameter"), xbnNodeList);
				int num = -1;
				if (xbnNodeList.size() > 0)
				{
					num = xbnNodeList[0].nodeValueInt();
				}
				if (xbnNodeList.size() > 1 && xbnNodeList[1].nodeValueInt() > 8)
				{
					flagOn(8);
				}
				if (xbnNodeList.size() > 2)
				{
					switch (xbnNodeList[2].nodeValueInt())
					{
					case 1:
						flagOn(16);
						break;
					case 2:
						flagOn(32);
						break;
					}
				}
				icon.copy(MenuManager.getSingleton().GetSmallIcon2d());
				icon.SetCell(14);
				sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(icon);
				if (num > 0)
				{
					mbiSetItemNumber((short)num);
				}
				else
				{
					icon.SetShow(show: false);
				}
				icon.SetPositionI(M.x(), M.y());
				itemID = num;
			}

			public override void bmBehave(Medget M)
			{
			}

			public override void bmActivate(Medget M)
			{
				MenuManager.getSingleton().SetActivateButtonState(0);
			}

			public override void bmFinalize(Medget M)
			{
				sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(icon);
				icon.Release();
				message.release();
			}

			public override void bmSuspend(Medget M)
			{
				if (message != null)
				{
					if (message.get().activity())
					{
						flagOn(2);
					}
					message.get().setActivity(b: false);
				}
				if (icon.IsShow())
				{
					flagOn(4);
				}
				else
				{
					flagOff(4);
				}
				icon.SetShow(show: false);
			}

			public override void bmResume(Medget M)
			{
				if (message != null)
				{
					message.get().setActivity(flagCheck(2));
				}
				icon.SetShow(flagCheck(4));
			}

			public void mbiSetBuffer(string buf, bool val)
			{
				if (message != null)
				{
					message.release();
					message = null;
				}
				dgs.DGSMessageManager dGSMessageManager = dgs.msg.CMessageSys.getInstance().Sub();
				message = new dgs.SmartPtr<dgs.DGSMessage>(dGSMessageManager.createMessage(buf, 1));
				if (message != null)
				{
					message.get().setPosition((short)(ownerMedget.x() + 16), ownerMedget.y(), erase: true);
					message.get().setDisplaySpeed(byte.MaxValue);
					message.get().setDisplayWait(0);
				}
				icon.SetShow(val);
			}

			public void mbSetTextMsgNo(int no)
			{
				message.release();
				icon.SetShow(show: false);
				dgs.DGSMessageManager dGSMessageManager = dgs.msg.CMessageSys.getInstance().Sub();
				message = new dgs.SmartPtr<dgs.DGSMessage>(dGSMessageManager.createMessage((uint)no, dgs.INVALID_MSDHANDLE, 1));
				if (message != null)
				{
					message.get().setPosition((short)(ownerMedget.x() + 16), ownerMedget.y(), erase: true);
					message.get().setDisplaySpeed(byte.MaxValue);
					message.get().setDisplayWait(0);
				}
			}

			public void mbiSetItemNumber(short number)
			{
				itemID = number;
				message.release();
				itm.ItemBaseParameter itemBaseParameter = itm.ItemManager.instance().itemParameter(number);
				if (itemBaseParameter == null)
				{
					icon.SetShow(show: false);
					return;
				}
				dgs.DGSMessageManager dGSMessageManager = ((ownerMedget.display() != 0) ? dgs.msg.CMessageSys.getInstance().Main() : dgs.msg.CMessageSys.getInstance().Sub());
				message = new dgs.SmartPtr<dgs.DGSMessage>(dGSMessageManager.createMessage((uint)itemBaseParameter.nameId(), dgs.INVALID_MSDHANDLE, (!flagCheck(8)) ? 1 : 0));
				if (message != null)
				{
					ds.Vector2<short> vector = new ds.Vector2<short>();
					short num = ownerMedget.x();
					short num2 = ownerMedget.y();
					mbiSetItemIcon(number);
					message.get().setDisplaySpeed(byte.MaxValue);
					message.get().setDisplayWait(0);
					if (flagCheck(16))
					{
						message.get().getCompleteTextSize(vector);
						num += (short)(ownerMedget.width() - vector.vx);
					}
					else if (flagCheck(32))
					{
						message.get().getCompleteTextSize(vector);
						num += (short)(ownerMedget.width() / 2 - (vector.vx + 16) / 2);
					}
					else
					{
						num += 16;
					}
					num2 += (short)((ownerMedget.height() - 12) / 2);
					message.get().setPosition(num, num2, erase: true);
					icon.SetShow(show: true);
					if (number == 1000)
					{
						icon.SetShow(show: false);
					}
					icon.SetPositionI(num - 16, num2 + -2);
				}
				else
				{
					icon.SetShow(show: false);
				}
			}

			public void mbiSetItemIcon(int number)
			{
				itm.CATEGORY cATEGORY = itm.ItemManager.instance().itemCategory((short)number);
				short idx = itm.ItemManager.instance().itemParameter((short)number).system();
				switch ((int)cATEGORY)
				{
				case 1:
					icon.SetCell((ushort)convertIDXWeaponSysToIcon(idx));
					break;
				case 2:
					icon.SetCell((ushort)convertIDXProtectionSysToIcon(idx));
					break;
				default:
					icon.SetShow(show: false);
					break;
				}
				icon.SetPriority(2);
			}

			public new static int classIdentifier()
			{
				return MBItemName_UN.number();
			}

			public override object queryInterface(int class_id)
			{
				if (class_id == classIdentifier())
				{
					return this;
				}
				return null;
			}

			public int mbiGetItemNumber()
			{
				return itemID;
			}
		}
	}
}
