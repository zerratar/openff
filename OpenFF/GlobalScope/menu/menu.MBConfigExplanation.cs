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
		public class MBConfigExplanation : MenuBehavior
		{
			public static dgs.UniqueNumber MBConfigExplanation_UN = new dgs.UniqueNumber();

			private int m_temp;

			private dgs.DGSMessage m_pMsg;

			private MBConfigCommon m_MBCCommon = new MBConfigCommon();

			public MBConfigExplanation()
			{
				m_pMsg = null;
				m_temp = -1;
			}

			~MBConfigExplanation()
			{
				m_MBCCommon.ConfigEnd(ref m_pMsg);
			}

			public override void bmInitialize(Medget M)
			{
				m_temp = -1;
			}

			public override void bmPostInitialize(Medget M)
			{
				m_temp = -1;
			}

			public override void bmBehave(Medget M)
			{
				ExplanationMessageCreate(M);
			}

			public override void bmFinalize(Medget M)
			{
				m_MBCCommon.ConfigEnd(ref m_pMsg);
			}

			public void ExplanationMessageCreate(Medget M)
			{
				if (m_pMsg != null)
				{
					if (m_temp == (sbyte)M.work())
					{
						return;
					}
					m_MBCCommon.ConfigEnd(ref m_pMsg);
				}
				dgs.msg.CMessageMng.MSF_HANDLE_KIND font = ((8 >= (sbyte)M.work()) ? dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_8x8 : dgs.msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_12x12);
				dgs.DGSMessageManager dGSMessageManager = null;
				dGSMessageManager = ((M.display() != 1) ? dgs.msg.CMessageSys.getInstance().Sub() : dgs.msg.CMessageSys.getInstance().Main());
				int[] array = new int[9] { 50650, 50651, 50652, 50653, 50654, 50655, 52073, 0, 0 };
				// PORT: the pad can put the focus on a row the touch build never reached (Up past the
				// first option lands on the page tabs), whose work() is outside the table: no line then.
				if ((sbyte)M.work() < 0 || (sbyte)M.work() >= array.Length || array[(sbyte)M.work()] == 0)
				{
					m_temp = (sbyte)M.work();
					return;
				}
				m_pMsg = dGSMessageManager.createMessage((uint)array[(sbyte)M.work()], dgs.INVALID_MSDHANDLE, (int)font);
				m_temp = (sbyte)M.work();
				if (m_pMsg != null)
				{
					m_pMsg.setPosition(M.x(), (short)(M.y() + (M.height() - 12) / 2), erase: true);
					m_pMsg.setDisplaySpeed(byte.MaxValue);
					m_pMsg.setDisplayWait(0);
				}
			}

			public new static int classIdentifier()
			{
				return MBConfigExplanation_UN.number();
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
