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
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
	public static partial class menu
	{
		public class MBConfigCommon
		{
			~MBConfigCommon()
			{
			}

			public void bmccInitialize(Medget M, ref dgs.DGSMessage _msg)
			{
				XbnNode firstNodeByTagNameFromChildren = M.node().getFirstNodeByTagNameFromChildren(TRANSCODE("behavior"));
				XbnNodeList xbnNodeList = new XbnNodeList();
				firstNodeByTagNameFromChildren.getNodesByTagNameFromChildren(TRANSCODE("parameter"), xbnNodeList);
				int msg_number = -1;
				if (xbnNodeList.size() > 0)
				{
					msg_number = xbnNodeList[0].nodeValueInt();
				}
				int font = 1;
				if (xbnNodeList.size() > 1)
				{
					int num = xbnNodeList[1].nodeValueInt();
					if (num >= 12)
					{
						font = 0;
					}
				}
				int num2 = 0;
				if (xbnNodeList.size() > 2)
				{
					num2 = xbnNodeList[2].nodeValueInt();
				}
				dgs.DGSMessageManager dGSMessageManager = null;
				dGSMessageManager = ((M.display() != 1) ? dgs.msg.CMessageSys.getInstance().Sub() : dgs.msg.CMessageSys.getInstance().Main());
				_msg = dGSMessageManager.createMessage((uint)msg_number, dgs.INVALID_MSDHANDLE, font);
				if (_msg != null)
				{
					_msg.setPosition(M.x(), M.y(), erase: true);
					_msg.setDisplaySpeed(byte.MaxValue);
					_msg.setDisplayWait(0);
					_msg.setPriority(3);
					_msg.progress();
					_msg.getDisplayTextSize(out var rect);
					int num3 = num2;
					if (num3 == 2)
					{
						_msg.setPosition((short)(M.x() + (M.width() - rect.width) / 2), (short)(M.y() + (M.height() - 12) / 2), erase: true);
					}
					else
					{
						_msg.setPosition(M.x(), (short)(M.y() + (M.height() - 12) / 2), erase: true);
					}
				}
			}

			public short GetNowMenu(OPTION_LINE ol)
			{
				return ol switch
				{
					OPTION_LINE.OL_MES => (short)opt.COptionManager.getSingleton().messageOption().messageSpeed(), 
					OPTION_LINE.OL_CUR => (short)opt.COptionManager.getSingleton().cursorOption().cursorPosition(), 
					OPTION_LINE.OL_BGM => (short)opt.COptionManager.getSingleton().soundOption().bgmVolume(), 
					OPTION_LINE.OL_SE => (short)opt.COptionManager.getSingleton().soundOption().seVolume(), 
					OPTION_LINE.OL_SMD => (short)opt.COptionManager.getSingleton().gameOption().summonsMagicDirect(), 
					OPTION_LINE.OL_MOV => (short)opt.COptionManager.getSingleton().gameOption().worldMoveType(), 
					OPTION_LINE.OL_MENU => (short)opt.COptionManager.getSingleton().gameOption().menuZoomSetting(), 
					_ => 0, 
				};
			}

			public void SetNowMenu(OPTION_LINE ol, short num)
			{
				switch (ol)
				{
				case OPTION_LINE.OL_MES:
					opt.COptionManager.getSingleton().messageOption().setMessageSpeed((opt.MESSAGE_SPEED)num);
					break;
				case OPTION_LINE.OL_CUR:
					opt.COptionManager.getSingleton().cursorOption().setCursorPosition((opt.CURSOR_POSITION)num);
					break;
				case OPTION_LINE.OL_BGM:
					opt.COptionManager.getSingleton().soundOption().setBgmVolume(num);
					break;
				case OPTION_LINE.OL_SE:
					opt.COptionManager.getSingleton().soundOption().setSeVolume(num);
					break;
				case OPTION_LINE.OL_SMD:
					opt.COptionManager.getSingleton().gameOption().setSummonsMagicDirect((opt.SUMMONS_MAGIC_DIRECT)num);
					break;
				case OPTION_LINE.OL_MOV:
					opt.COptionManager.getSingleton().gameOption().setWorldMoveType((opt.WORLD_MOVE_TYPE)num);
					break;
				}
				card.SaveOption();
			}

			public bool HeightMove(Medget M, string p_name)
			{
				Medget medget = null;
				Medget medget2 = null;
				medget2 = M.parentNode().parentNode().getNodeByID(TRANSCODE(p_name));
				medget = medget2.childNode();
				if ((sbyte)medget2.work() == 2 || (sbyte)medget2.work() == 3)
				{
					MenuManager.getSingleton().initFocus(medget.myTag());
					SetExplanation(medget);
					return true;
				}
				while ((sbyte)medget.work() != GetNowMenu((OPTION_LINE)(sbyte)medget2.work()))
				{
					medget = medget.nextSibling();
				}
				MenuManager.getSingleton().initFocus(medget.myTag());
				SetExplanation(medget);
				return true;
			}

			public void ConfigEnd(ref dgs.DGSMessage msg)
			{
				if (msg != null)
				{
					msg.release();
					msg = null;
				}
			}

			public void SetExplanation(Medget M)
			{
				Medget medget = null;
				medget = M.parentNode().parentNode().parentNode()
					.getNodeByID(TRANSCODE("m_explanation"));
				medget.setWork((sbyte)M.parentNode().work());
			}

			public void SetDefault()
			{
				opt.COptionManager.getSingleton().initialize();
				card.SaveOption();
				Medget nodeByID = MenuManager.getSingleton().root().getNodeByID(TRANSCODE("m_normal_mes"));
				if (nodeByID != null)
				{
					MenuManager.getSingleton().initFocus(nodeByID.myTag());
				}
			}
		}
	}
}
