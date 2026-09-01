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
	public static partial class menu
	{
		public class MBWeaponName : MenuBehavior
		{
			public static dgs.UniqueNumber MBWeaponName_UN = new dgs.UniqueNumber();

			private dgs.DGSMessage pMsgRightHandName;

			private dgs.DGSMessage rightWeaponNumber_;

			private dgs.DGSMessage pMsgLeftHandName;

			private dgs.DGSMessage leftWeaponNumber_;

			public MBWeaponName()
			{
				pMsgRightHandName = null;
				rightWeaponNumber_ = null;
				pMsgLeftHandName = null;
				leftWeaponNumber_ = null;
			}

			~MBWeaponName()
			{
				deleteMessageAll();
			}

			public override void bmInitialize(Medget M)
			{
				bmFinalize(M);
				int targetCharNo = MenuManager.getSingleton().GetTargetCharNo();
				dgs.DGSMessageManager dGSMessageManager = null;
				dGSMessageManager = ((M.display() != 1) ? dgs.msg.CMessageSys.getInstance().Sub() : dgs.msg.CMessageSys.getInstance().Main());
				Medget medget = M.childNode();
				int num = pl.PlayerParty.instance().player((byte)targetCharNo).equipParameter()
					.equipHand(pl.HAND_TYPE.RIGHT_HAND)
					.itemId();
				int num2 = pl.PlayerParty.instance().player((byte)targetCharNo).equipParameter()
					.equipHand(pl.HAND_TYPE.LEFT_HAND)
					.itemId();
				int num3 = pl.PlayerParty.instance().player((byte)targetCharNo).equipParameter()
					.equipHand(pl.HAND_TYPE.RIGHT_HAND)
					.equipNumber()
					.get();
				int num4 = pl.PlayerParty.instance().player((byte)targetCharNo).equipParameter()
					.equipHand(pl.HAND_TYPE.LEFT_HAND)
					.equipNumber()
					.get();
				if (num > 0)
				{
					int msg_number = itm.ItemManager.instance().itemParameter((short)num).nameId();
					pMsgRightHandName = dGSMessageManager.createMessage((uint)msg_number, MenuManager.getSingleton().GetItemDataTextNo(), 1);
					pMsgRightHandName.setPosition(medget.x(), (short)(medget.y() + (medget.height() - 12) / 2), erase: true);
					pMsgRightHandName.setDisplaySpeed(byte.MaxValue);
					pMsgRightHandName.setDisplayWait(0);
					if (num3 > 0)
					{
						string after = "";
						dgs.msg.CMessageSys.getInstance().changeValueFont(num3, out after);
						rightWeaponNumber_ = dGSMessageManager.createMessage(after, 1);
						ds.Vector2<short> vector = new ds.Vector2<short>();
						rightWeaponNumber_.getTextSize(vector);
						rightWeaponNumber_.setPosition((short)(medget.x() + 160 - vector.vx), (short)(medget.y() + (medget.height() - 12) / 2), erase: true);
						rightWeaponNumber_.setDisplaySpeed(byte.MaxValue);
						rightWeaponNumber_.setDisplayWait(0);
					}
					medget.setWork(num);
				}
				medget = medget.nextSibling();
				if (num2 > 0)
				{
					int msg_number2 = itm.ItemManager.instance().itemParameter((short)num2).nameId();
					pMsgLeftHandName = dGSMessageManager.createMessage((uint)msg_number2, MenuManager.getSingleton().GetItemDataTextNo(), 1);
					pMsgLeftHandName.setPosition(medget.x(), (short)(medget.y() + (medget.height() - 12) / 2), erase: true);
					pMsgLeftHandName.setDisplaySpeed(byte.MaxValue);
					pMsgLeftHandName.setDisplayWait(0);
					if (num4 > 0)
					{
						string after2 = "";
						dgs.msg.CMessageSys.getInstance().changeValueFont(num4, out after2);
						leftWeaponNumber_ = dGSMessageManager.createMessage(after2, 1);
						ds.Vector2<short> vector2 = new ds.Vector2<short>();
						leftWeaponNumber_.getTextSize(vector2);
						leftWeaponNumber_.setPosition((short)(medget.x() + 160 - vector2.vx), (short)(medget.y() + (medget.height() - 12) / 2), erase: true);
						leftWeaponNumber_.setDisplaySpeed(byte.MaxValue);
						leftWeaponNumber_.setDisplayWait(0);
					}
					medget.setWork(num2);
				}
			}

			public override void bmBehave(Medget M)
			{
				MenuManager.getSingleton().SetTargetItemNo((int)MenuManager.getSingleton().getFocuseMedget().work());
				MenuManager.getSingleton().saveBattleUseItemHand_set(MenuManager.getSingleton().GetTargetCharNo(), MenuManager.getSingleton().getFocuseMedget().myTag());
			}

			public void bmRefresh(Medget pM)
			{
			}

			public override void bmFinalize(Medget M)
			{
				deleteMessageAll();
			}

			public override bool bmDecide(Medget M)
			{
				MenuManager.getSingleton().SetDecideButtonState(0);
				return false;
			}

			public override void bmActivate(Medget M)
			{
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

			public void deleteMessageAll()
			{
				if (pMsgRightHandName != null)
				{
					pMsgRightHandName.release();
					pMsgRightHandName = null;
				}
				if (rightWeaponNumber_ != null)
				{
					rightWeaponNumber_.release();
					rightWeaponNumber_ = null;
				}
				if (pMsgLeftHandName != null)
				{
					pMsgLeftHandName.release();
					pMsgLeftHandName = null;
				}
				if (leftWeaponNumber_ != null)
				{
					leftWeaponNumber_.release();
					leftWeaponNumber_ = null;
				}
			}

			public new static int classIdentifier()
			{
				return MBWeaponName_UN.number();
			}

			public override object queryInterface(int class_id)
			{
				if (class_id == classIdentifier())
				{
					return this;
				}
				return null;
			}

			public override bool bmUseTap(Medget M)
			{
				return true;
			}
		}
	}
}
