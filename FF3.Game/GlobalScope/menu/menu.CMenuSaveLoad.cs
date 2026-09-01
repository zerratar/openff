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
		public class CMenuSaveLoad
		{
			public enum STATE
			{
				STATE_SELECT_SLOT,
				STATE_SLOT_DATA_SELECT,
				STATE_SLOT_DATA_WAIT,
				STATE_SLOT_DATA_FINISHED,
				STATE_SLOT_BACKUP_ERR,
				STATE_MAX
			}

			public enum MODE
			{
				MODE_SAVE,
				MODE_LOAD,
				MODE_MAX
			}

			public enum SLOT
			{
				SLOT_1,
				SLOT_2,
				SLOT_3,
				SLOT_MAX
			}

			public const STATE STATE_SELECT_SLOT = STATE.STATE_SELECT_SLOT;

			public const STATE STATE_SLOT_DATA_SELECT = STATE.STATE_SLOT_DATA_SELECT;

			public const STATE STATE_SLOT_DATA_WAIT = STATE.STATE_SLOT_DATA_WAIT;

			public const STATE STATE_SLOT_DATA_FINISHED = STATE.STATE_SLOT_DATA_FINISHED;

			public const STATE STATE_SLOT_BACKUP_ERR = STATE.STATE_SLOT_BACKUP_ERR;

			public const STATE STATE_MAX = STATE.STATE_MAX;

			public const MODE MODE_SAVE = MODE.MODE_SAVE;

			public const MODE MODE_LOAD = MODE.MODE_LOAD;

			public const MODE MODE_MAX = MODE.MODE_MAX;

			public const SLOT SLOT_1 = SLOT.SLOT_1;

			public const SLOT SLOT_2 = SLOT.SLOT_2;

			public const SLOT SLOT_3 = SLOT.SLOT_3;

			public const SLOT SLOT_MAX = SLOT.SLOT_MAX;

			public static CMenuSaveLoad instance_ = new CMenuSaveLoad();

			private bool m_EndFlag;

			private STATE m_CurrentState;

			private MODE m_CurrentMode;

			private SLOT m_CurrentSlot;

			private sys2d.Cell clearMark_ = new sys2d.Cell();

			private int m_Counter;

			public static CMenuSaveLoad singleton()
			{
				return instance_;
			}

			public void initialize()
			{
				SaveDataMng.getSingleton().activate(0);
				SaveDataMng.getSingleton().activate(1);
				SaveDataMng.getSingleton().activate(2);
				int num = 0;
				for (int i = 1; 3 > i; i++)
				{
					card.OmitTime omitTime = SaveDataMng.getSingleton().SaveData(num).GetOmitTime();
					card.OmitTime omitTime2 = SaveDataMng.getSingleton().SaveData(i).GetOmitTime();
					if (omitTime.IsLess(omitTime2))
					{
						num = i;
					}
				}
				m_EndFlag = false;
				m_CurrentState = STATE.STATE_SELECT_SLOT;
				m_CurrentSlot = SLOT.SLOT_1;
				MenuManager.getSingleton().buildMenu("load_save");
				MenuManager.getSingleton().initFocus(num);
				MenuManager.getSingleton().GetCursor2d().SetShow(show: true);
				MenuManager.getSingleton().SetCancelButtonState(-1);
			}

			public void execute()
			{
				MenuManager.getSingleton().execute();
				if (getCurrentMode() == MODE.MODE_LOAD)
				{
					executeLoadMode();
				}
				else if (getCurrentMode() == MODE.MODE_SAVE)
				{
					executeSaveMode();
				}
			}

			public void executeSaveMode()
			{
				if (getCurrentState() == STATE.STATE_SLOT_DATA_SELECT)
				{
					if (wmenu.CWMenuManager.Instance().GetMenuButton().TouchButtonB() || MenuManager.getSingleton().GetCancelButtonState() == 0)
					{
						setCurrentState(STATE.STATE_SELECT_SLOT);
						MenuManager.getSingleton().playSECancel();
						MenuManager.getSingleton().Pop();
						MenuManager.getSingleton().SetCancelButtonState(1);
						wmenu.CWMenuManager.Instance().GetMenuButton().SetButtonBActivity(b: true);
						wmenu.CWMenuManager.Instance().SetSecondlyBGVisibility(b: false);
					}
				}
				else if (getCurrentState() == STATE.STATE_SLOT_DATA_FINISHED && 0 > m_Counter--)
				{
					singleton().setCurrentState(STATE.STATE_SELECT_SLOT);
					MenuManager.getSingleton().Pop();
					MenuManager.getSingleton().GetCursor2d().SetShow(show: true);
					wmenu.CWMenuManager.Instance().GetMenuButton().SetButtonBActivity(b: true);
					wmenu.CWMenuManager.Instance().SetSecondlyBGVisibility(b: false);
				}
			}

			public void executeLoadMode()
			{
				if (getCurrentState() == STATE.STATE_SELECT_SLOT)
				{
					if (wmenu.CWMenuManager.Instance().GetMenuButton().TouchButtonB() || (ds.g_Pad.edge() & 2) != 0)
					{
						sys.GGlobal.setNextPart(sys.GGlobal.getPreviousPart());
						onEndFlag();
					}
				}
				else if (getCurrentState() == STATE.STATE_SLOT_DATA_SELECT && (wmenu.CWMenuManager.Instance().GetMenuButton().TouchButtonB() || MenuManager.getSingleton().GetCancelButtonState() == 0))
				{
					setCurrentState(STATE.STATE_SELECT_SLOT);
					MenuManager.getSingleton().playSECancel();
					MenuManager.getSingleton().Pop();
					MenuManager.getSingleton().SetCancelButtonState(1);
					wmenu.CWMenuManager.Instance().GetMenuButton().SetButtonBActivity(b: true);
					wmenu.CWMenuManager.Instance().SetSecondlyBGVisibility(b: false);
				}
				if (getCurrentState() == STATE.STATE_SLOT_DATA_FINISHED && 0 > m_Counter--)
				{
					sys.GGlobal.setNextPart(load.LoadPart.getInstance().getAfterPart());
					onEndFlag();
				}
			}

			public void terminate()
			{
				MenuManager.getSingleton().releaseWindowAll();
				MenuManager.getSingleton().release();
				SaveDataMng.getSingleton().deactivate(0);
				SaveDataMng.getSingleton().deactivate(1);
				SaveDataMng.getSingleton().deactivate(2);
			}

			public void setCounter(int Count)
			{
				m_Counter = Count;
			}

			public void setClearMarkVisibility(bool b)
			{
				clearMark_.SetShow(b);
				if (b)
				{
					Medget nodeByID = MenuManager.getSingleton().root().getNodeByID(TRANSCODE("clear"));
					clearMark_.SetPositionI(nodeByID.x(), nodeByID.y());
				}
			}

			public bool setupClearMark()
			{
				changeGlobalDirectory();
				clearMark_.Load(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_SUB2D, "clear.NCER", "clear.NANR", "clear.NCGR", null);
				clearMark_.ceReleaseCgCl();
				clearMark_.SetCell(0);
				clearMark_.SetShow(show: false);
				if (!sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(clearMark_))
				{
					OS_Printf("maenu save load add sprite failed.\n");
					return false;
				}
				return true;
			}

			public void releaseClearMark()
			{
				if (!sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(clearMark_))
				{
					OS_Printf("maenu save load add sprite failed.\n");
				}
				clearMark_.Release();
				NNS_G2dReleaseImageProxy(clearMark_.GetImageProxy());
			}

			public void onEndFlag()
			{
				m_EndFlag = true;
			}

			public void offEndFlag()
			{
				m_EndFlag = false;
			}

			public bool isEndFlag()
			{
				return m_EndFlag;
			}

			public void setCurrentState(STATE _CurrentState)
			{
				m_CurrentState = _CurrentState;
			}

			public STATE getCurrentState()
			{
				return m_CurrentState;
			}

			public void setCurrentMode(MODE _CurrentMode)
			{
				m_CurrentMode = _CurrentMode;
			}

			public MODE getCurrentMode()
			{
				return m_CurrentMode;
			}

			public void setCurrentSlot(SLOT _CurrentSlot)
			{
				m_CurrentSlot = _CurrentSlot;
			}

			public SLOT getCurrentSlot()
			{
				return m_CurrentSlot;
			}
		}
	}
}
