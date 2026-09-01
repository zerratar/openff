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
		public class MBQuestion : MenuBehavior
		{
			public const int MESSAGE_NUM = 3;

			public const int NOTIFY_YES = 0;

			public const int NOTIFY_NO = 1;

			public static dgs.UniqueNumber MBQuestion_UN = new dgs.UniqueNumber();

			private bool _cursorShow;

			private bool _SavedVisibility;

			private int _display;

			private dgs.DGSMessage[] _pMessage = new dgs.DGSMessage[3];

			public MBQuestion()
			{
				for (int i = 0; i < 3; i++)
				{
					_pMessage[i] = null;
				}
			}

			~MBQuestion()
			{
				int i = 0;
				for (; i < 3; i++)
				{
					if (_pMessage[i] != null)
					{
						_pMessage[i].release();
					}
					_pMessage[i] = null;
				}
			}

			public override void bmInitialize(Medget M)
			{
				dgs.DGSMessageManager dGSMessageManager = null;
				XbnNode xbnNode = null;
				XbnNodeList xbnNodeList = new XbnNodeList();
				XbnNode firstNodeByTagNameFromChildren = M.node().getFirstNodeByTagNameFromChildren(TRANSCODE("behavior"));
				firstNodeByTagNameFromChildren.getNodesByTagNameFromChildren(TRANSCODE("parameter"), xbnNodeList);
				if (xbnNodeList.size() > 0)
				{
					xbnNodeList[0].nodeValueInt();
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
				_SavedVisibility = false;
				for (int i = 0; i < 3; i++)
				{
					_pMessage[i] = null;
				}
				int num2 = 0;
				Medget medget = M.childNode();
				while (medget != null)
				{
					dGSMessageManager = ((medget.display() != 0) ? dgs.msg.CMessageSys.getInstance().Main() : dgs.msg.CMessageSys.getInstance().Sub());
					xbnNode = medget.node().getFirstNodeByTagName(TRANSCODE("data"));
					if (xbnNode == null)
					{
						break;
					}
					_pMessage[num2] = dGSMessageManager.createMessage(xbnNode.nodeValueString(), (int)font);
					if (_pMessage[num2] == null)
					{
						break;
					}
					_pMessage[num2].setPosition(medget.x(), medget.y(), erase: true);
					if (strcmp(medget._id(), "") == 0)
					{
						_cursorShow = false;
					}
					else
					{
						_cursorShow = true;
					}
					MenuManager.getSingleton().joinFocusList(medget);
					medget = medget.nextSibling();
					num2++;
				}
				MenuManager.getSingleton().initFocus(1);
			}

			public override void bmBehave(Medget M)
			{
			}

			public override void bmFinalize(Medget M)
			{
				for (int i = 0; i < 3; i++)
				{
					if (_pMessage[i] != null)
					{
						_pMessage[i].release();
					}
					_pMessage[i] = null;
				}
			}

			public override void bmSuspend(Medget M)
			{
				for (int i = 0; i < 3; i++)
				{
					if (_pMessage[i] != null)
					{
						_SavedVisibility = _pMessage[i].activity();
						_pMessage[i].setActivity(b: false);
					}
				}
			}

			public override void bmResume(Medget M)
			{
				for (int i = 0; i < 3; i++)
				{
					_pMessage[i].setActivity(_SavedVisibility);
				}
			}

			public override bool bmDecide(Medget M)
			{
				bool result = true;
				MenuManager.getSingleton().SetDecideButtonState(0);
				if (mbNotifier != null)
				{
					if (M.myTag() == 0)
					{
						mbNotifier.mbnNotify(this, 0u, 0u);
					}
					else
					{
						mbNotifier.mbnNotify(this, 1u, 0u);
					}
				}
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
				if (mbNotifier != null)
				{
					mbNotifier.mbnNotify(this, 1u, 0u);
				}
				return result;
			}

			public override bool bmDirection(Medget M, int key)
			{
				return MenuManager.getSingleton().MoveCursor(M, PlaySe: true);
			}

			public void bmqSetMessage(int nMessageIndex, int nMessageID)
			{
				if ((0 < nMessageIndex && 3 > nMessageIndex) || mbGetOwner() == null)
				{
					return;
				}
				if (_pMessage[nMessageIndex] != null)
				{
					_pMessage[nMessageIndex].release();
					_pMessage[nMessageIndex] = null;
				}
				dgs.DGSMessageManager dGSMessageManager = ((mbGetOwner().display() != 0) ? dgs.msg.CMessageSys.getInstance().Main() : dgs.msg.CMessageSys.getInstance().Sub());
				if (dGSMessageManager != null)
				{
					_pMessage[nMessageIndex] = dGSMessageManager.createMessage((uint)nMessageID, dgs.INVALID_MSDHANDLE, 0);
					if (_pMessage[nMessageIndex] != null)
					{
						ds.Vector2<short> vector = new ds.Vector2<short>();
						_pMessage[nMessageIndex].setDisplaySpeed(byte.MaxValue);
						_pMessage[nMessageIndex].setDisplayWait(0);
						_pMessage[nMessageIndex].setStyle(1024u);
						_pMessage[nMessageIndex].getTextSize(vector);
						_pMessage[nMessageIndex].setPosition((short)(mbGetOwner().x() + mbGetOwner().width() / 2 - vector.vx / 2), mbGetOwner().y(), erase: true);
					}
				}
			}

			public void bmqSetMessage(int nMessageIndex, string pString)
			{
			}

			public new static int classIdentifier()
			{
				return MBQuestion_UN.number();
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
