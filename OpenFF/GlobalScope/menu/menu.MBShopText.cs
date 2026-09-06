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
		public class MBShopText : MenuBehavior
		{
			public static dgs.UniqueNumber MBShopText_UN = new dgs.UniqueNumber();

			private dgs.SmartPtr<dgs.DGSMessage> _msg;

			~MBShopText()
			{
			}

			public override void bmInitialize(Medget M)
			{
				XbnNode firstNodeByTagNameFromChildren = ownerMedget.node().getFirstNodeByTagNameFromChildren(TRANSCODE("behavior"));
				XbnNodeList xbnNodeList = new XbnNodeList();
				firstNodeByTagNameFromChildren.getNodesByTagNameFromChildren(TRANSCODE("parameter"), xbnNodeList);
				int msg_number = -1;
				if (xbnNodeList.size() > 0)
				{
					msg_number = xbnNodeList[0].nodeValueInt();
				}
				int font = 1;
				dgs.DGSMessageManager dGSMessageManager = ((M.display() == 0) ? dgs.msg.CMessageSys.getInstance().Sub() : dgs.msg.CMessageSys.getInstance().Main());
				_msg = new dgs.SmartPtr<dgs.DGSMessage>(dGSMessageManager.createMessage((uint)msg_number, dgs.INVALID_MSDHANDLE, font));
				if (_msg.get() != null)
				{
					_msg.get().setDisplaySpeed(byte.MaxValue);
					_msg.get().setDisplayWait(0);
					_msg.get().setPosition(M.x(), (short)(M.y() + (M.height() - 12) / 2), erase: true);
				}
			}

			public override void bmBehave(Medget M)
			{
			}

			public override void bmSuspend(Medget M)
			{
				if (_msg.get() != null)
				{
					if (_msg.get().visibility())
					{
						flagOn(1);
					}
					_msg.get().setVisibility(b: false);
				}
			}

			public override void bmResume(Medget M)
			{
				if (_msg.get() != null)
				{
					_msg.get().setVisibility(flagCheck(1));
					_msg.get().pageChange(0);
				}
			}

			public override void bmFinalize(Medget M)
			{
				_msg.release();
			}

			public void mbChangePage(byte page)
			{
				_msg.get().pageChange(page);
			}

			public new static int classIdentifier()
			{
				return MBShopText_UN.number();
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
