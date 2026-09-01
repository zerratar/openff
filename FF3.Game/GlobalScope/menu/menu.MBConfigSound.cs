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
		public class MBConfigSound : MenuBehavior
		{
			public enum VAR_CELL
			{
				VC_VAR,
				VC_CURSOR,
				VC_MAX
			}

			public const VAR_CELL VC_VAR = VAR_CELL.VC_VAR;

			public const VAR_CELL VC_CURSOR = VAR_CELL.VC_CURSOR;

			public const VAR_CELL VC_MAX = VAR_CELL.VC_MAX;

			public static dgs.UniqueNumber MBConfigSound_UN = new dgs.UniqueNumber();

			private dgs.DGSMessage m_pMsg;

			private MBConfigCommon m_MBCCommon = new MBConfigCommon();

			private bool m_Init;

			private sys2d.Cell[] m_Cell = new sys2d.Cell[2];

			public MBConfigSound()
			{
				for (int i = 0; i < m_Cell.Length; i++)
				{
					m_Cell[i] = new sys2d.Cell();
				}
				m_pMsg = null;
				m_Init = false;
			}

			~MBConfigSound()
			{
				m_MBCCommon.ConfigEnd(ref m_pMsg);
				int i = 0;
				for (; i < 2; i++)
				{
					m_Cell[i].Release();
					sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(m_Cell[i]);
				}
			}

			public override void bmInitialize(Medget M)
			{
				m_MBCCommon.bmccInitialize(M, ref m_pMsg);
				if (M.width() != 0)
				{
					m_Cell[0].Load(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_SUB2D, "m008_volume.NCER", null, "m008_volume.NCBR", null);
					m_Cell[0].ceReleaseCgCl();
					m_Cell[1].Load(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_SUB2D, "m008_volume_2.NCER", null, "m008_volume_2.NCBR", null);
					m_Cell[1].ceReleaseCgCl();
					sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(m_Cell[1]);
					sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(m_Cell[0]);
					m_Cell[0].SetPositionI(M.x() + SPR_POS_PLUS, M.y() + SPR_POS_PLUS_Y);
					m_Cell[0].SetPriority(3);
					m_Cell[1].SetPositionI(M.x() + m_MBCCommon.GetNowMenu((OPTION_LINE)(sbyte)M.parentNode().work()) * M.width() / 127, M.y() + SPR_POS_PLUS_Y);
					m_Cell[1].SetPriority(3);
					m_Cell[1].SetDepth(-65536);
				}
				m_Init = false;
			}

			public override void bmPostInitialize(Medget M)
			{
				MBText mBText = (MBText)M.nextSibling().behavior().queryInterface(MBText.classIdentifier());
				if (mBText != null)
				{
					mBText.mbSetBufferNumber(m_MBCCommon.GetNowMenu((OPTION_LINE)(sbyte)M.parentNode().work()) * 10 / 127);
					mBText.getMessage().setPriority(3);
				}
			}

			public override void bmBehave(Medget M)
			{
				if (dv.CDeviceManager.getInstance().Tp().isTouch() && MenuManager.getSingleton().getFocuseMedget() == M)
				{
					int x = 0;
					int y = 0;
					dv.CDeviceManager.getInstance().Tp().TouchPanel_2d(out x, out y);
					if (x >= M.x() - 32 && x < M.x() + M.width() + 32 && y >= M.y() && y < M.y() + M.height())
					{
						int num = MATH_CLAMP((x - M.x()) * 127 / M.width(), 0, 127);
						m_MBCCommon.SetNowMenu((OPTION_LINE)(sbyte)M.parentNode().work(), (short)num);
						m_Cell[1].SetPositionI(M.x() + num * M.width() / 127, M.y() + SPR_POS_PLUS_Y);
						bmPostInitialize(M);
						TP_CancelTap();
					}
				}
			}

			public override void bmFinalize(Medget M)
			{
				m_MBCCommon.ConfigEnd(ref m_pMsg);
				for (int i = 0; i < 2; i++)
				{
					NNS_G2dReleaseImageProxy(m_Cell[i].GetImageProxy());
					m_Cell[i].Release();
					sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(m_Cell[i]);
				}
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
					return CursorMove(M, 4);
				}
				if ((ds.g_Pad.edge() & 0x20) != 0)
				{
					MenuManager.getSingleton().playSEMoveCursor();
					return CursorMove(M, -4);
				}
				return false;
			}

			public override void bmActivate(Medget M)
			{
				m_MBCCommon.SetExplanation(M);
				if (!m_Init)
				{
					m_Init = true;
				}
			}

			public override void bmDeactivate(Medget M)
			{
			}

			public bool CursorMove(Medget M, short point)
			{
				if ((int)M.work() == 0)
				{
					short num = (short)(m_MBCCommon.GetNowMenu((OPTION_LINE)M.parentNode().work()) + point);
					if (num < 0)
					{
						num = 0;
					}
					if (num > 127)
					{
						num = 127;
					}
					m_MBCCommon.SetNowMenu((OPTION_LINE)M.parentNode().work(), num);
					m_Cell[1].SetPositionI(M.x() + num * M.width() / 127, M.y() + SPR_POS_PLUS_Y);
					return true;
				}
				return false;
			}

			public new static int classIdentifier()
			{
				return MBConfigSound_UN.number();
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
