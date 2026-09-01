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
		public class MBText : MenuBehavior
		{
			public enum ALIGNMENT
			{
				ALIGN_LEFT,
				ALIGN_RIGHT,
				ALIGN_CENTER,
				ALIGN_FLEXIBLE,
				ALIGN_BUTTON,
				ALIGN_MENU
			}

			public const ALIGNMENT ALIGN_LEFT = ALIGNMENT.ALIGN_LEFT;

			public const ALIGNMENT ALIGN_RIGHT = ALIGNMENT.ALIGN_RIGHT;

			public const ALIGNMENT ALIGN_CENTER = ALIGNMENT.ALIGN_CENTER;

			public const ALIGNMENT ALIGN_FLEXIBLE = ALIGNMENT.ALIGN_FLEXIBLE;

			public const ALIGNMENT ALIGN_BUTTON = ALIGNMENT.ALIGN_BUTTON;

			public const ALIGNMENT ALIGN_MENU = ALIGNMENT.ALIGN_MENU;

			public static dgs.UniqueNumber MBText_UN = new dgs.UniqueNumber();

			protected bool setup;

			protected ALIGNMENT alignment;

			protected dgs.DGSMessage message;

			protected ButtonWindow m_ButtonWindow = new ButtonWindow();

			public MBText()
			{
				setup = false;
				alignment = ALIGNMENT.ALIGN_LEFT;
				message = null;
			}

			~MBText()
			{
				if (message != null)
				{
					message.release();
					message = null;
				}
			}

			public override void bmInitialize(Medget M)
			{
				parameterSetUp();
			}

			public override void bmBehave(Medget M)
			{
			}

			public override void bmFinalize(Medget M)
			{
				mbtReleaseMessage();
				m_ButtonWindow.Release();
			}

			public override void bmActivate(Medget M)
			{
				for (Medget medget = M.parentNode(); medget != null; medget = medget.parentNode())
				{
					if (medget.behavior() != null)
					{
						medget.behavior().bmActivate(M);
						break;
					}
				}
				MenuManager.getSingleton().SetActivateButtonState(0);
			}

			public override void mbTPPush(Medget M)
			{
				if ((alignment != ALIGNMENT.ALIGN_BUTTON || m_ButtonWindow.IsShow()) && !flagCheck(32))
				{
					flagOn(32);
					if (message != null)
					{
						message.position(out var x, out var y);
						message.setPosition((short)(x + 1), (short)(y + 1), erase: true);
					}
					if (alignment == ALIGNMENT.ALIGN_BUTTON)
					{
						m_ButtonWindow.bwSetState(ButtonWindow.BW_STATE.BWS_ON);
					}
				}
			}

			public override void mbTPRelease(Medget M)
			{
				if (!flagCheck(32))
				{
					return;
				}
				flagOff(32);
				if (message != null)
				{
					message.position(out var x, out var y);
					message.setPosition((short)(x - 1), (short)(y - 1), erase: true);
				}
				if (alignment == ALIGNMENT.ALIGN_BUTTON)
				{
					m_ButtonWindow.bwSetState(ButtonWindow.BW_STATE.BWS_OFF);
				}
				if (alignment == ALIGNMENT.ALIGN_BUTTON || alignment == ALIGNMENT.ALIGN_MENU)
				{
					ds.g_TouchPanel.getPoint(out var x2, out var y2);
					if (ownerMedget.x() < x2 && x2 <= ownerMedget.x() + ownerMedget.width() && ownerMedget.y() < y2 && y2 <= ownerMedget.y() + ownerMedget.height() && ds.g_TouchPanel.getDispPoint().cancel == 0)
					{
						MenuManager.getSingleton().MedgetsDecide(ownerMedget);
					}
				}
			}

			public override void bmSuspend(Medget M)
			{
				if (message != null)
				{
					if (message.activity())
					{
						flagOn(2);
					}
					message.setActivity(b: false);
				}
			}

			public override void bmResume(Medget M)
			{
				if (message != null)
				{
					message.setActivity(flagCheck(2));
					message.setShadow(b: true);
				}
			}

			public void mbtReleaseMessage()
			{
				if (message != null)
				{
					message.release();
					message = null;
				}
			}

			public void bmTextVisibility(bool v)
			{
				if (message != null)
				{
					message.setVisibility(v);
				}
				if (alignment == ALIGNMENT.ALIGN_BUTTON)
				{
					m_ButtonWindow.SetShow(v, user: true);
				}
			}

			public bool bmGetTextVisibility()
			{
				if (message != null)
				{
					return message.visibility();
				}
				return false;
			}

			public void mbtSetAlignment()
			{
				if (message != null && ownerMedget != null)
				{
					message.progress();
					message.getDisplayTextSize(out var rect);
					int num = 0;
					if (ownerMedget.height() > 0)
					{
						num = (ownerMedget.height() - rect.height) / 2;
					}
					switch (alignment)
					{
					case ALIGNMENT.ALIGN_RIGHT:
					{
						int num7 = ownerMedget.width() - rect.width;
						message.setPosition((short)(ownerMedget.x() + num7), (short)(ownerMedget.y() + num), erase: true);
						break;
					}
					case ALIGNMENT.ALIGN_CENTER:
					case ALIGNMENT.ALIGN_BUTTON:
					case ALIGNMENT.ALIGN_MENU:
					{
						int num6 = (ownerMedget.width() - rect.width) / 2;
						message.setPosition((short)(ownerMedget.x() + num6), (short)(ownerMedget.y() + num), erase: true);
						break;
					}
					case ALIGNMENT.ALIGN_FLEXIBLE:
					{
						short num2 = 4;
						int num3 = (ownerMedget.width() - rect.width) / 2;
						ushort num4 = (ushort)(ownerMedget.x() + num3 - num2);
						ushort num5 = (ushort)(ownerMedget.y() + num);
						mbGetOwner().setPosition((short)num4, (short)num5);
						mbGetOwner().setWidth((short)(rect.width + num2 * 2));
						message.setPosition((short)(num4 + num2), (short)num5, erase: true);
						break;
					}
					default:
						message.setPosition(ownerMedget.x(), (short)(ownerMedget.y() + num), erase: true);
						break;
					}
				}
			}

			public void mbSetTextMsgNo(int number)
			{
				mbtReleaseMessage();
				int font = 1;
				if (flagCheck(4))
				{
					font = 0;
				}
				dgs.DGSMessageManager dGSMessageManager = (flagCheck(16) ? dgs.msg.CMessageSys.getInstance().Main() : dgs.msg.CMessageSys.getInstance().Sub());
				message = dGSMessageManager.createMessage((uint)number, dgs.INVALID_MSDHANDLE, font);
				if (message != null)
				{
					message.setPosition(ownerMedget.x(), ownerMedget.y(), erase: true);
					message.setVSpace(4);
					message.setDisplaySpeed(byte.MaxValue);
					message.setDisplayWait(0);
					mbtSetAlignment();
				}
			}

			public void mbSetBufferMsg(string pBuf, bool decWidth)
			{
				parameterSetUp();
				mbtReleaseMessage();
				dgs.DGSMessageManager dGSMessageManager = (flagCheck(16) ? dgs.msg.CMessageSys.getInstance().Main() : dgs.msg.CMessageSys.getInstance().Sub());
				message = dGSMessageManager.createMessage(pBuf, (!flagCheck(4)) ? 1 : 0);
				if (message != null)
				{
					if (!decWidth)
					{
						message.setPosition(ownerMedget.x(), ownerMedget.y(), erase: true);
					}
					else
					{
						ds.Vector2<short> vector = new ds.Vector2<short>();
						message.getTextSize(vector);
						message.setPosition((short)(ownerMedget.x() - vector.vx), ownerMedget.y(), erase: true);
					}
					message.setVSpace(4);
					message.setDisplaySpeed(byte.MaxValue);
					message.setDisplayWait(0);
					mbtSetAlignment();
				}
			}

			public void mbSetBufferNumber(int num)
			{
				if (message != null)
				{
					sprintf(out var arg, "%d", num);
					message.assignText(arg);
					mbtSetAlignment();
				}
			}

			public override bool bmDecide(Medget M)
			{
				MenuManager.getSingleton().SetDecideButtonState(0);
				return false;
			}

			public override bool bmCancel(Medget M)
			{
				MenuManager.getSingleton().SetCancelButtonState(0);
				return false;
			}

			public void mbtSetString(string str)
			{
				if (message != null)
				{
					message.assignText(str);
					mbtSetAlignment();
				}
			}

			public void mbtSetPage(byte page)
			{
				if (message != null)
				{
					message.pageChange(page);
					mbtSetAlignment();
				}
			}

			public void changeTextColor(dgs.TXT_COLOR color)
			{
				message.setMessageColor(color);
			}

			public override bool bmDirection(Medget M, int key)
			{
				if (flagCheck(8))
				{
					return false;
				}
				return MenuManager.getSingleton().MoveCursor(M, PlaySe: true);
			}

			public override void mbSetPosition(short x, short y)
			{
				if (message != null)
				{
					message.setPosition(x, y, erase: true);
				}
			}

			public void parameterSetUp()
			{
				if (setup)
				{
					return;
				}
				XbnNode firstNodeByTagNameFromChildren = ownerMedget.node().getFirstNodeByTagNameFromChildren(TRANSCODE("behavior"));
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
				if (xbnNodeList.size() > 1)
				{
					int num2 = xbnNodeList[1].nodeValueInt();
					if (num2 > 8)
					{
						flagOn(4);
					}
					else
					{
						flagOff(4);
					}
				}
				alignment = ALIGNMENT.ALIGN_LEFT;
				if (xbnNodeList.size() > 2)
				{
					alignment = (ALIGNMENT)xbnNodeList[2].nodeValueInt();
				}
				if (xbnNodeList.size() > 3 && xbnNodeList[3].nodeValueInt() != 0)
				{
					flagOn(8);
				}
				if (ownerMedget.display() == 0)
				{
					flagOff(16);
				}
				else
				{
					flagOn(16);
				}
				if (alignment == ALIGNMENT.ALIGN_BUTTON)
				{
					m_ButtonWindow.Initialize();
					ds.Vector2<short> ul = new ds.Vector2<short>(ownerMedget.x(), ownerMedget.y());
					ds.Vector2<short> size = new ds.Vector2<short>(ownerMedget.width(), ownerMedget.height());
					m_ButtonWindow.bwCreateUL(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN3D, ul, size, 0);
					m_ButtonWindow.bwSetState(ButtonWindow.BW_STATE.BWS_OFF);
				}
				dgs.DGSMessageManager dGSMessageManager = (flagCheck(16) ? dgs.msg.CMessageSys.getInstance().Main() : dgs.msg.CMessageSys.getInstance().Sub());
				if (message != null)
				{
					return;
				}
				if (num > 0)
				{
					if (num == 52080)
					{
						message = dGSMessageManager.createMessage(R.@string.ACHIEVEMENTS, (!flagCheck(4)) ? 1 : 0);
					}
					else
					{
						message = dGSMessageManager.createMessage((uint)num, dgs.INVALID_MSDHANDLE, (!flagCheck(4)) ? 1 : 0);
					}
				}
				else
				{
					XbnNode firstNodeByTagName = ownerMedget.node().getFirstNodeByTagName(TRANSCODE("data"));
					if (firstNodeByTagName != null)
					{
						message = dGSMessageManager.createMessage(firstNodeByTagName.nodeValueString(), (!flagCheck(4)) ? 1 : 0);
					}
				}
				if (message != null)
				{
					message.setVSpace(4);
					message.setDisplaySpeed(byte.MaxValue);
					message.setDisplayWait(0);
					mbtSetAlignment();
					if (xbnNodeList.size() > 4)
					{
						message.setHSpace(xbnNodeList[4].nodeValueInt());
					}
					if (xbnNodeList.size() > 5)
					{
						message.setVSpace(xbnNodeList[5].nodeValueInt());
					}
					setup = true;
				}
			}

			public override void bmSetPriority(int priority)
			{
				if (message != null)
				{
					message.setPriority(priority);
				}
				if (alignment == ALIGNMENT.ALIGN_BUTTON)
				{
					m_ButtonWindow.SetPriority((byte)priority);
				}
			}

			public override int bmGetCursorX(Medget M)
			{
				if (alignment == ALIGNMENT.ALIGN_CENTER)
				{
					return 12;
				}
				if (alignment == ALIGNMENT.ALIGN_MENU)
				{
					return 14;
				}
				if (alignment == ALIGNMENT.ALIGN_BUTTON)
				{
					return -LCD_WIDTH * 2;
				}
				return 0;
			}

			public override bool bmIsButton(Medget arg0)
			{
				if (alignment != ALIGNMENT.ALIGN_BUTTON)
				{
					return alignment == ALIGNMENT.ALIGN_MENU;
				}
				return true;
			}

			public new static int classIdentifier()
			{
				return MBText_UN.number();
			}

			public override object queryInterface(int class_id)
			{
				if (class_id == classIdentifier())
				{
					return this;
				}
				return null;
			}

			public dgs.DGSMessage getMessage()
			{
				return message;
			}

			public void mbSetTextColor(dgs.TXT_COLOR color)
			{
				if (message != null)
				{
					message.setMessageColor(color);
				}
			}

			public virtual bool bmIsPushed()
			{
				return flagCheck(32);
			}
		}
	}
}
