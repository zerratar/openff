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
		public class MBConfig : MenuBehavior
		{
			public static dgs.UniqueNumber MBConfig_UN = new dgs.UniqueNumber();

			private MBConfigCommon m_MBCCommon = new MBConfigCommon();

			private ButtonWindow m_ButtonWindow = new ButtonWindow();

			private dgs.DGSMessage m_pMsg;

			private bool m_Init;

			public MBConfig()
			{
				m_pMsg = null;
			}

			~MBConfig()
			{
				m_MBCCommon.ConfigEnd(ref m_pMsg);
			}

			public override void bmInitialize(Medget M)
			{
				m_MBCCommon.bmccInitialize(M, ref m_pMsg);
				m_ButtonWindow.Initialize();
				ds.Vector2<short> ul = new ds.Vector2<short>(M.x(), M.y());
				ds.Vector2<short> size = new ds.Vector2<short>(M.width(), M.height());
				m_ButtonWindow.bwCreateUL(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN3D, ul, size, 3);
				ColorUpdate(M);
				m_Init = false;
			}

			public override void bmPostInitialize(Medget M)
			{
				if ((sbyte)M.work() == 8 || (sbyte)M.work() == 7)
				{
					M.setPriority(3);
				}
			}

			public override void bmBehave(Medget M)
			{
				if (!m_Init)
				{
					m_Init = true;
				}
				ColorUpdate(M);
			}

			public override void bmFinalize(Medget M)
			{
				m_MBCCommon.ConfigEnd(ref m_pMsg);
				m_ButtonWindow.Release();
			}

			public override bool bmDecide(Medget M)
			{
				if (mbNotifier != null)
				{
					mbNotifier.mbnNotify(this, 1u, 0u);
				}
				return true;
			}

			public override bool bmCancel(Medget M)
			{
				return false;
			}

			public override bool bmDirection(Medget M, int key)
			{
				if ((ds.g_Pad.edge() & 0x40) != 0)
				{
					MenuManager.getSingleton().playSEMoveCursor();
					return m_MBCCommon.HeightMove(M, M.up());
				}
				if ((ds.g_Pad.edge() & 0x80) != 0)
				{
					MenuManager.getSingleton().playSEMoveCursor();
					return m_MBCCommon.HeightMove(M, M.down());
				}
				if ((ds.g_Pad.edge() & 0x10) != 0)
				{
					MenuManager.getSingleton().playSEMoveCursor();
					return CursorMove(M, M.right());
				}
				if ((ds.g_Pad.edge() & 0x20) != 0)
				{
					MenuManager.getSingleton().playSEMoveCursor();
					return CursorMove(M, M.left());
				}
				return false;
			}

			public override void bmActivate(Medget M)
			{
				if (m_Init && (sbyte)M.parentNode().work() != 9)
				{
					m_MBCCommon.SetExplanation(M);
					m_MBCCommon.SetNowMenu((OPTION_LINE)(sbyte)M.parentNode().work(), (sbyte)M.work());
				}
			}

			public override void bmDeactivate(Medget M)
			{
			}

			public bool CursorMove(Medget M, string p_name)
			{
				if (p_name == null)
				{
					return false;
				}
				Medget medget = null;
				medget = M.parentNode().getNodeByIDFromChildren(TRANSCODE(p_name));
				if (medget == null)
				{
					return false;
				}
				MenuManager.getSingleton().initFocus(medget.myTag());
				m_MBCCommon.SetNowMenu((OPTION_LINE)(sbyte)M.parentNode().work(), (sbyte)medget.work());
				return true;
			}

			public void ColorUpdate(Medget M)
			{
				if ((sbyte)M.work1() != 0)
				{
					if ((sbyte)M.work() == m_MBCCommon.GetNowMenu((OPTION_LINE)(sbyte)M.parentNode().work()))
					{
						m_pMsg.setMessageColor(dgs.TXT_COLOR.TXT_COLOR_WHITE);
						m_ButtonWindow.bwSetState(ButtonWindow.BW_STATE.BWS_ON);
					}
					else
					{
						m_pMsg.setMessageColor(dgs.TXT_COLOR.TXT_UCOLOR_3);
						m_ButtonWindow.bwSetState(ButtonWindow.BW_STATE.BWS_OFF);
					}
				}
			}

			public new static int classIdentifier()
			{
				return MBConfig_UN.number();
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
