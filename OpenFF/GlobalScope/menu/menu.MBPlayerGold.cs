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
		public class MBPlayerGold : MenuBehavior
		{
			public const int MESSAGE_NUM = 2;

			public static dgs.UniqueNumber MBPlayerGold_UN = new dgs.UniqueNumber();

			private int old_gold;

			private dgs.SmartPtr<dgs.DGSMessage>[] pMsg = new dgs.SmartPtr<dgs.DGSMessage>[2];

			public MBPlayerGold()
			{
				for (int i = 0; i < pMsg.Length; i++)
				{
					pMsg[i] = new dgs.SmartPtr<dgs.DGSMessage>();
				}
			}

			~MBPlayerGold()
			{
			}

			public void mbgSetNumber(int number, int gillCheck)
			{
				for (int i = 0; i < 2; i++)
				{
					pMsg[i].release();
				}
				dgs.DGSMessageManager dGSMessageManager = null;
				dGSMessageManager = ((ownerMedget.display() != 0) ? dgs.msg.CMessageSys.getInstance().Main() : dgs.msg.CMessageSys.getInstance().Sub());
				string str = number.ToString();
				ds.Vector2<short> vector = new ds.Vector2<short>(0, 0);
				ds.Vector2<short> vector2 = new ds.Vector2<short>();
				if (gillCheck != 0)
				{
					pMsg[1] = new dgs.SmartPtr<dgs.DGSMessage>(dGSMessageManager.createMessage(50429u, MenuManager.getSingleton().GetMenuDataTextNo(), 1));
					if (pMsg[1] == null)
					{
						return;
					}
					pMsg[1].get().getTextSize(vector);
					pMsg[1].get().setPosition((short)(ownerMedget.x() + ownerMedget.width() - vector.vx), ownerMedget.y(), erase: true);
					pMsg[1].get().setDisplaySpeed(byte.MaxValue);
					pMsg[1].get().setDisplayWait(0);
				}
				pMsg[0] = new dgs.SmartPtr<dgs.DGSMessage>(dGSMessageManager.createMessage(str, 1));
				if (pMsg[0] != null)
				{
					pMsg[0].get().getTextSize(vector2);
					pMsg[0].get().setPosition((short)(ownerMedget.x() + ownerMedget.width() - vector2.vx - vector.vx), (short)(ownerMedget.y() + (ownerMedget.height() - 12) / 2), erase: true);
				}
			}

			public override void bmInitialize(Medget M)
			{
				XbnNode firstNodeByTagNameFromChildren = M.node().getFirstNodeByTagNameFromChildren(TRANSCODE("behavior"));
				if (firstNodeByTagNameFromChildren != null)
				{
					XbnNodeList xbnNodeList = new XbnNodeList();
					firstNodeByTagNameFromChildren.getNodesByTagNameFromChildren(TRANSCODE("parameter"), xbnNodeList);
					int gillCheck = 1;
					if (xbnNodeList.size() > 0)
					{
						gillCheck = xbnNodeList[0].nodeValueInt();
					}
					mbgSetNumber(pl.PlayerParty.instance().gold().get(), gillCheck);
				}
			}

			public override void bmBehave(Medget M)
			{
				int gillCheck = 1;
				XbnNode firstNodeByTagNameFromChildren = M.node().getFirstNodeByTagNameFromChildren(TRANSCODE("behavior"));
				if (firstNodeByTagNameFromChildren != null)
				{
					XbnNodeList xbnNodeList = new XbnNodeList();
					firstNodeByTagNameFromChildren.getNodesByTagNameFromChildren(TRANSCODE("parameter"), xbnNodeList);
					if (xbnNodeList.size() > 0)
					{
						gillCheck = xbnNodeList[0].nodeValueInt();
					}
				}
				if (old_gold != pl.PlayerParty.instance().gold().get())
				{
					mbgSetNumber(pl.PlayerParty.instance().gold().get(), gillCheck);
				}
				old_gold = pl.PlayerParty.instance().gold().get();
			}

			public override void bmFinalize(Medget M)
			{
				for (int i = 0; i < 2; i++)
				{
					pMsg[i].release();
				}
			}

			public new static int classIdentifier()
			{
				return MBPlayerGold_UN.number();
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
