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
	public class ScrollBar
	{
		public enum AREA_FLAG
		{
			AREA_UP = 1,
			AREA_DOWN = 2,
			AREA_CLEAR = 0
		}

		public enum TOUCHSTATE
		{
			TPC_NORMAL,
			TPC_UP_BTN_TOUCH,
			TPC_DOWN_BTN_TOUCH,
			TPC_UP_BTN_KEEP,
			TPC_DOWN_BTN_KEEP,
			TPC_PAGE_UP_TOUCH,
			TPC_PAGE_DOWN_TOUCH,
			TPC_PAGE_UP_KEEP,
			TPC_PAGE_DOWN_KEEP,
			TPC_DRAG,
			TPC_INVALID
		}

		public class _parts_
		{
			public sys2d.Cell cell = new sys2d.Cell();

			public sys2d.Sprite3d sw_sprite = new sys2d.Sprite3d();
		}

		public const AREA_FLAG AREA_UP = AREA_FLAG.AREA_UP;

		public const AREA_FLAG AREA_DOWN = AREA_FLAG.AREA_DOWN;

		public const AREA_FLAG AREA_CLEAR = AREA_FLAG.AREA_CLEAR;

		public const int PARTS_DACT = 0;

		public const int PARTS_ACT = 1;

		public const TOUCHSTATE TPC_NORMAL = TOUCHSTATE.TPC_NORMAL;

		public const TOUCHSTATE TPC_UP_BTN_TOUCH = TOUCHSTATE.TPC_UP_BTN_TOUCH;

		public const TOUCHSTATE TPC_DOWN_BTN_TOUCH = TOUCHSTATE.TPC_DOWN_BTN_TOUCH;

		public const TOUCHSTATE TPC_UP_BTN_KEEP = TOUCHSTATE.TPC_UP_BTN_KEEP;

		public const TOUCHSTATE TPC_DOWN_BTN_KEEP = TOUCHSTATE.TPC_DOWN_BTN_KEEP;

		public const TOUCHSTATE TPC_PAGE_UP_TOUCH = TOUCHSTATE.TPC_PAGE_UP_TOUCH;

		public const TOUCHSTATE TPC_PAGE_DOWN_TOUCH = TOUCHSTATE.TPC_PAGE_DOWN_TOUCH;

		public const TOUCHSTATE TPC_PAGE_UP_KEEP = TOUCHSTATE.TPC_PAGE_UP_KEEP;

		public const TOUCHSTATE TPC_PAGE_DOWN_KEEP = TOUCHSTATE.TPC_PAGE_DOWN_KEEP;

		public const TOUCHSTATE TPC_DRAG = TOUCHSTATE.TPC_DRAG;

		public const TOUCHSTATE TPC_INVALID = TOUCHSTATE.TPC_INVALID;

		public const int BUTTON_UP = 0;

		public const int BUTTON_DOWN = 1;

		public const int NUMBER_OF_PARTS = 2;

		public static sys2d.DS2D_OBJ_PLANE SBTargetPlane = sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN3D;

		public static byte SBDelayToKeep = 10;

		public static byte SBRepeatInterval = 1;

		public static int PARTS_W = 98304;

		public static int BG_TOP_BTM_H = 98304;

		public static int BG_CENTER_H = 262144;

		public static int SB_TOP_BTM_H = 49152;

		public static int SB_CENTER_H = 262144;

		public static short MINIMUM_H = (short)(FX_Mul(BG_TOP_BTM_H + SB_TOP_BTM_H, 8192) >> 12);

		public static short MAXIMUM_H = (short)(FX_Mul(BG_TOP_BTM_H + SB_TOP_BTM_H, 8192) + FX_Mul(BG_CENTER_H, 8192) >> 12);

		protected TOUCHSTATE touchState;

		protected NNSG2dFVec2 prev_touch_position;

		protected NNSG2dFVec2 saved_sridebar_position;

		protected NNSG2dFVec2 sridebar_position;

		protected int counter;

		private _parts_[] parts_ = new _parts_[2];

		protected sys2d.Sprite[] parts = new sys2d.Sprite[2];

		protected short linesPerPage;

		protected short numberOfLines;

		protected short currentLine;

		protected short sbX;

		protected short sbY;

		protected short sbHeight;

		protected int restrictedArea;

		protected SBEventHandler handler;

		protected bool drag;

		private _parts_[] m_aCenterPartsBase;

		protected bool m_bUseCenter;

		protected bool m_bHideParts;

		protected int m_iReserveCount;

		protected sys2d.Sprite[] m_aCenterParts;

		private static int count = 0;

		public ScrollBar()
		{
			for (int i = 0; i < parts_.Length; i++)
			{
				parts_[i] = new _parts_();
			}
			touchState = TOUCHSTATE.TPC_NORMAL;
			counter = 0;
			for (int j = 0; j < 2; j++)
			{
				parts[j] = null;
			}
			currentLine = 0;
			linesPerPage = 1;
			numberOfLines = 1;
			restrictedArea = 0;
			sbHeight = (short)(BG_CENTER_H + FX_Mul(BG_TOP_BTM_H, 8192) >> 12);
			handler = null;
			m_aCenterPartsBase = null;
			m_bUseCenter = false;
			m_bHideParts = false;
			m_iReserveCount = 0;
			m_aCenterParts = null;
		}

		public static bool sbCheckTouchPanel()
		{
			for (int num = g_ActiveScrollBars.size() - 1; num >= 0; num--)
			{
				if (g_ActiveScrollBars[num].sbCheckTP())
				{
					return true;
				}
			}
			return false;
		}

		public void sbCreate()
		{
			SBTargetPlane = sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_SUB2D;
			switch (SBTargetPlane)
			{
			case sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN3D:
			{
				for (int j = 0; j < 2; j++)
				{
					parts_[j].sw_sprite.copy(menu.MenuManager.getSingleton().GetMenuButtonIcon3d());
					parts[j] = parts_[j].sw_sprite;
				}
				break;
			}
			case sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN2D:
			case sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_SUB2D:
			{
				for (int i = 0; i < 2; i++)
				{
					parts_[i].cell.copy(menu.MenuManager.getSingleton().GetMenuButtonIcon2d());
					parts[i] = parts_[i].cell;
				}
				break;
			}
			}
			ushort[] array = new ushort[2] { 9, 12 };
			for (int k = 0; k < 2; k++)
			{
				parts[k].SetCell(array[k]);
				parts[k].SetShow(show: true);
				sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(parts[k]);
			}
			sbSetPosition(256, 192);
			for (int num = g_ActiveScrollBars.size() - 1; num >= 0; num--)
			{
				if (g_ActiveScrollBars[num] == this)
				{
					return;
				}
			}
			g_ActiveScrollBars.push_back(this);
			sbRestrainCheck();
		}

		public void sbDestroy()
		{
			for (int i = 0; i < 2; i++)
			{
				if (parts[i] != null)
				{
					sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(parts[i]);
					parts[i].SetShow(show: false);
					parts[i].Release();
				}
				parts[i] = null;
			}
			for (int j = 0; j < m_iReserveCount; j++)
			{
				if (m_aCenterParts[j] != null)
				{
					sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(m_aCenterParts[j]);
					m_aCenterParts[j].SetShow(show: false);
					m_aCenterParts[j].Release();
				}
				m_aCenterParts[j] = null;
			}
			m_iReserveCount = 0;
			m_aCenterPartsBase = null;
			m_aCenterParts = null;
			for (int num = g_ActiveScrollBars.size() - 1; num >= 0; num--)
			{
				if (g_ActiveScrollBars[num] == this)
				{
					g_ActiveScrollBars.erase(num);
					break;
				}
			}
		}

		public void sbSetPosition(ds.Vector2<short> pos)
		{
			sbSetPosition(pos.vx, pos.vy);
		}

		public void sbSetPosition(short x, short y)
		{
			NNSG2dFVec2 nNSG2dFVec = new NNSG2dFVec2();
			NNSG2dFVec2 position = parts[0].GetPosition();
			nNSG2dFVec.x = (x << 12) - position.x;
			nNSG2dFVec.y = (y << 12) - position.y;
			for (int i = 0; i < 2; i++)
			{
				position = parts[i].GetPosition();
				position.x += nNSG2dFVec.x;
				position.y += nNSG2dFVec.y;
				parts[i].SetPosition(position);
			}
			sbX = x;
			sbY = y;
		}

		public void sbSetHeight(short height)
		{
			if (parts[0] != null)
			{
				sbHeight = height;
				if (parts[1] != null)
				{
					NNSG2dFVec2 position = parts[1].GetPosition();
					position.y = (sbY << 12) + (sbHeight << 12) - PARTS_W;
					parts[1].SetPosition(position);
				}
			}
		}

		public void sbSetDepth(int depth)
		{
			if (parts[0] != null)
			{
				for (int i = 0; i < 2; i++)
				{
					parts[i].SetDepth(depth);
				}
			}
		}

		public int sbGetDepth()
		{
			if (parts[0] == null)
			{
				return 0;
			}
			return parts[0].GetDepth();
		}

		public void sbSetCapacity(short _linesPerPage, short _numberOfLines)
		{
			currentLine = 0;
			linesPerPage = _linesPerPage;
			numberOfLines = _numberOfLines;
			sbRestrainCheck();
		}

		public void sbSetLine(short target_line)
		{
			if (currentLine != target_line)
			{
				currentLine = target_line;
				sbRestrainCheck();
				if (handler != null)
				{
					handler.sbehScrolled(currentLine);
				}
			}
		}

		public void sbFixedMove(short line)
		{
			if (line != 0 && numberOfLines >= linesPerPage && currentLine + line >= 0 && currentLine + line <= numberOfLines - linesPerPage)
			{
				currentLine += line;
				if (currentLine < 0)
				{
					currentLine = 0;
				}
				else if (currentLine > numberOfLines - linesPerPage)
				{
					currentLine = (short)(numberOfLines - linesPerPage);
				}
				sbRestrainCheck();
				if (line != 0 && handler != null)
				{
					handler.sbehScrolled(currentLine);
				}
			}
		}

		public void sbRestrainCheck(AREA_FLAG f)
		{
			restrictedArea = (int)f;
			if (parts[0] != null)
			{
				parts[0].SetShow((f & AREA_FLAG.AREA_UP) == 0);
			}
			if (parts[1] != null)
			{
				parts[1].SetShow((f & AREA_FLAG.AREA_DOWN) == 0);
			}
			setCenterParts();
		}

		public void sbRestrainCheck()
		{
			restrictedArea = 0;
			if (currentLine <= 0)
			{
				restrictedArea |= 1;
			}
			if (currentLine >= numberOfLines - linesPerPage)
			{
				restrictedArea |= 2;
			}
			sbRestrainCheck((AREA_FLAG)restrictedArea);
		}

		public bool sbCheckTP()
		{
			if (parts[0] == null)
			{
				return false;
			}
			if (numberOfLines < linesPerPage)
			{
				return false;
			}
			dv.tp.CPlayerTp cPlayerTp = dv.CDeviceManager.getInstance().Tp();
			cPlayerTp.TouchPanel_2d(out var x, out var y);
			x <<= 12;
			y <<= 12;
			if (drag)
			{
				if ((count++ & 3) == 0)
				{
					if (y < sbY << 12)
					{
						sbFixedMove(-1);
					}
					if (y > sbY + sbHeight << 12)
					{
						sbFixedMove(1);
					}
				}
			}
			else
			{
				int flickOffset = dv.CDeviceManager.getInstance().Tp().getDispPoint()
					.flickOffset;
				if (flickOffset != 0 && parts[(flickOffset > 0) ? 1u : 0u].IsShow())
				{
					sbFixedMove((short)((flickOffset >= 0) ? 1 : (-1)));
				}
			}
			switch (touchState)
			{
			case TOUCHSTATE.TPC_NORMAL:
			{
				int num = sbCheckButton(x, y);
				if ((restrictedArea & 1) == 0 && num < 0)
				{
					sbFixedMove(-1);
					touchState = TOUCHSTATE.TPC_UP_BTN_TOUCH;
					counter = SBDelayToKeep;
				}
				else if ((restrictedArea & 2) == 0 && num > 0)
				{
					sbFixedMove(1);
					touchState = TOUCHSTATE.TPC_DOWN_BTN_TOUCH;
					counter = SBDelayToKeep;
				}
				break;
			}
			case TOUCHSTATE.TPC_UP_BTN_TOUCH:
				if (sbCheckButton(x, y) < 0)
				{
					if (--counter < 0)
					{
						touchState = TOUCHSTATE.TPC_UP_BTN_KEEP;
					}
				}
				else if (!cPlayerTp.isTouch())
				{
					touchState = TOUCHSTATE.TPC_NORMAL;
				}
				break;
			case TOUCHSTATE.TPC_UP_BTN_KEEP:
				if (sbCheckButton(x, y) < 0)
				{
					counter++;
					if ((counter & (3 >> counter / 60)) == 0)
					{
						sbFixedMove(-1);
					}
				}
				else if (!cPlayerTp.isTouch())
				{
					touchState = TOUCHSTATE.TPC_NORMAL;
				}
				break;
			case TOUCHSTATE.TPC_DOWN_BTN_TOUCH:
				if (sbCheckButton(x, y) > 0)
				{
					if (--counter < 0)
					{
						touchState = TOUCHSTATE.TPC_DOWN_BTN_KEEP;
					}
				}
				else if (!cPlayerTp.isTouch())
				{
					touchState = TOUCHSTATE.TPC_NORMAL;
				}
				break;
			case TOUCHSTATE.TPC_DOWN_BTN_KEEP:
				if (sbCheckButton(x, y) > 0)
				{
					counter++;
					if ((counter & (3 >> counter / 60)) == 0)
					{
						sbFixedMove(1);
					}
				}
				else if (!cPlayerTp.isTouch())
				{
					touchState = TOUCHSTATE.TPC_NORMAL;
				}
				break;
			}
			return touchState != TOUCHSTATE.TPC_NORMAL;
		}

		public int sbCheckButton(int x, int y)
		{
			if (!ds.g_TouchPanel.isTouch())
			{
				return 0;
			}
			if (m_bUseCenter)
			{
				return 0;
			}
			NNSG2dFVec2 position = parts[0].GetPosition();
			if (position.x < x && x <= position.x + PARTS_W && position.y < y && y <= position.y + BG_TOP_BTM_H)
			{
				return -1;
			}
			position = parts[1].GetPosition();
			if (position.x < x && x <= position.x + PARTS_W && position.y < y && y <= position.y + BG_TOP_BTM_H)
			{
				return 1;
			}
			return 0;
		}

		public void sbPartsActivateProcess(int val)
		{
			bool show = ((val != 0) ? true : false);
			for (int i = 0; i < 2; i++)
			{
				parts[i].SetShow(show);
			}
		}

		public void sbPartsSetPriority(int val)
		{
			for (int i = 0; i < 2; i++)
			{
				parts[i].SetPriority((byte)val);
			}
		}

		public void sbSetUseCenter(bool bUse)
		{
			m_bUseCenter = bUse;
			m_bHideParts = !bUse;
			setCenterParts();
		}

		private void setCenterParts()
		{
			if (m_bUseCenter)
			{
				if (!m_bHideParts)
				{
					int num = 0;
					ushort[] array = new ushort[2] { 11, 14 };
					for (int i = 0; i < 2; i++)
					{
						if (parts[i] != null)
						{
							parts[i].SetCell(array[i]);
							num++;
						}
					}
					if (num == 2)
					{
						m_bHideParts = true;
					}
				}
				if (m_iReserveCount != numberOfLines - linesPerPage + 1)
				{
					for (int j = 0; j < m_iReserveCount; j++)
					{
						if (m_aCenterParts[j] != null)
						{
							sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(m_aCenterParts[j]);
							m_aCenterParts[j].SetShow(show: false);
							m_aCenterParts[j].Release();
						}
						m_aCenterParts[j] = null;
					}
					m_iReserveCount = numberOfLines - linesPerPage + 1;
					m_aCenterPartsBase = null;
					m_aCenterParts = null;
				}
				if (m_aCenterPartsBase == null)
				{
					m_aCenterPartsBase = new _parts_[m_iReserveCount];
					m_aCenterParts = new sys2d.Sprite[m_iReserveCount];
					switch (SBTargetPlane)
					{
					case sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN3D:
					{
						for (int l = 0; l < m_iReserveCount; l++)
						{
							m_aCenterPartsBase[l] = new _parts_();
							m_aCenterParts[l] = new sys2d.Sprite();
							m_aCenterPartsBase[l].sw_sprite.copy(menu.MenuManager.getSingleton().GetMenuButtonIcon3d_2());
							m_aCenterParts[l] = m_aCenterPartsBase[l].sw_sprite;
						}
						break;
					}
					case sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN2D:
					case sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_SUB2D:
					{
						for (int k = 0; k < m_iReserveCount; k++)
						{
							m_aCenterPartsBase[k] = new _parts_();
							m_aCenterParts[k] = new sys2d.Sprite();
							m_aCenterPartsBase[k].cell.copy(menu.MenuManager.getSingleton().GetMenuButtonIcon2d_2());
							m_aCenterParts[k] = m_aCenterPartsBase[k].cell;
						}
						break;
					}
					}
					for (int m = 0; m < m_iReserveCount; m++)
					{
						m_aCenterParts[m].SetShow(show: true);
						sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(m_aCenterParts[m]);
					}
					int num2 = (sbHeight - 20 << 12) / m_iReserveCount;
					int x = sbX + 16 << 12;
					int num3 = (sbY + 10 << 12) + num2 / 2;
					for (int n = 0; n < m_iReserveCount; n++)
					{
						m_aCenterParts[n].SetPositionF(x, num3);
						num3 += num2;
					}
				}
				int num4 = 1;
				for (int num5 = m_iReserveCount / linesPerPage; num5 >= 1; num5--)
				{
					int num6 = m_iReserveCount - (num5 + 1);
					if (num6 > 0 && num6 % num5 == 0)
					{
						num4 = num6 / num5 + 1;
						break;
					}
				}
				ushort[] array2 = new ushort[4] { 0, 1, 2, 3 };
				for (int num7 = 0; num7 < m_iReserveCount; num7++)
				{
					int num8 = ((num7 % num4 == 0) ? 2 : 0);
					if (num7 == currentLine)
					{
						num8++;
					}
					m_aCenterParts[num7].SetCell(array2[num8]);
				}
				return;
			}
			if (m_bHideParts)
			{
				int num9 = 0;
				ushort[] array3 = new ushort[2] { 9, 12 };
				for (int num10 = 0; num10 < 2; num10++)
				{
					if (parts[num10] != null)
					{
						parts[num10].SetCell(array3[num10]);
						num9++;
					}
				}
				if (num9 == 2)
				{
					m_bHideParts = false;
				}
			}
			for (int num11 = 0; num11 < m_iReserveCount; num11++)
			{
				if (m_aCenterParts[num11] != null)
				{
					sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(m_aCenterParts[num11]);
					m_aCenterParts[num11].SetShow(show: false);
					m_aCenterParts[num11].Release();
				}
				m_aCenterParts[num11] = null;
			}
			m_iReserveCount = 0;
			m_aCenterPartsBase = null;
			m_aCenterParts = null;
		}

		public static void sbSetDelayToKeep(byte dtk)
		{
			SBDelayToKeep = dtk;
		}

		public static byte sbGetDelayToKeep()
		{
			return SBDelayToKeep;
		}

		public static void sbSetRepeatInterval(byte ri)
		{
			SBRepeatInterval = ri;
		}

		public static byte sbGetRepeatInterval()
		{
			return SBRepeatInterval;
		}

		public void sbSetHandler(SBEventHandler _handler)
		{
			handler = _handler;
		}

		public short sbGetHeight()
		{
			return sbHeight;
		}

		public void sbSetDrag(bool b)
		{
			drag = b;
		}
	}
}
