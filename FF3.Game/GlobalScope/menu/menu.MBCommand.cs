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
		public class MBCommand : MenuBehavior
		{
			public static dgs.UniqueNumber MBCommand_UN = new dgs.UniqueNumber();

			private dgs.DGSMessage message;

			public MBCommand()
			{
				message = null;
			}

			~MBCommand()
			{
				if (message != null)
				{
					message.release();
					message = null;
				}
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
				int msg_number = -1;
				if (xbnNodeList.size() > 0)
				{
					msg_number = xbnNodeList[0].nodeValueInt();
				}
				dgs.msg.CMessageMng.MSF_HANDLE_KIND font = dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_8x8;
				if (xbnNodeList.size() > 1)
				{
					int num = xbnNodeList[1].nodeValueInt();
					if (num >= 12)
					{
						font = dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_12x12;
					}
				}
				dgs.DGSMessageManager dGSMessageManager = null;
				dGSMessageManager = ((M.display() != 1) ? dgs.msg.CMessageSys.getInstance().Sub() : dgs.msg.CMessageSys.getInstance().Main());
				message = dGSMessageManager.createMessage((uint)msg_number, dgs.INVALID_MSDHANDLE, (int)font);
				if (message != null)
				{
					message.setDisplaySpeed(byte.MaxValue);
					message.setDisplayWait(0);
					message.progress();
					message.getDisplayTextSize(out var rect);
					message.setPosition((short)(M.x() + (M.width() - rect.width) / 2), (short)(M.y() + (M.height() - 12) / 2), erase: true);
				}
			}

			public override void bmBehave(Medget M)
			{
			}

			public override void bmFinalize(Medget M)
			{
				if (message != null)
				{
					message.release();
					message = null;
				}
			}

			public override bool bmDecide(Medget M)
			{
				MenuManager.getSingleton().SetDecideButtonState(0);
				return false;
			}

			public override void bmActivate(Medget M)
			{
				MenuManager.getSingleton().SetActivateButtonState(0);
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

			public new static int classIdentifier()
			{
				return MBCommand_UN.number();
			}

			public override object queryInterface(int class_id)
			{
				if (class_id == classIdentifier())
				{
					return this;
				}
				return null;
			}

			public override int bmGetCursorX(Medget unuse0)
			{
				return 12;
			}
		}
	}
}
