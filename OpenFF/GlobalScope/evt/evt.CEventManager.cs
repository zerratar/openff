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
	public static partial class evt
	{
		public class CEventManager
		{
			private const uint MAX_NUM_MAP = 1u;

			private const uint MAX_NUM_LOGIC = 16u;

			public static CEventManager m_Instance = new CEventManager();

			private bool m_EventStop;

			private bool m_MessageStop;

			private bool m_Event;

			private bool m_PartyTalkEvent;

			private bool m_ItemEvent;

			private int m_UseItemId;

			private LogicManager m_LogicMng;

			private FlagManager m_FlagMng = new FlagManager();

			private ValueManager m_ValueMng;

			private ScriptData[] scriptDatas_ = new ScriptData[1];

			private Logic[] logicHandles_ = new Logic[16];

			private Logic[] logicDatas_ = new Logic[16];

			private ScriptData m_GlobalScript;

			private ScriptData m_MapScript;

			public void initialize()
			{
				initializeLogic();
				initializeFlag(set_reset: true);
				initializeValue();
			}

			public void execute()
			{
				if (!m_EventStop)
				{
					m_LogicMng.execute();
				}
			}

			public void terminate()
			{
				cleanUpScriptData();
				m_LogicMng.removeAllLogic();
			}

			public void initializeLogic()
			{
				m_GlobalScript = (m_MapScript = null);
				m_LogicMng.init(1u, 16u, scriptDatas_, logicHandles_, logicDatas_);
			}

			public void initializeFlag(bool set_reset)
			{
				for (int i = 0; i < 3; i++)
				{
					for (int j = 0; j < 1000; j++)
					{
						if (!set_reset)
						{
							m_FlagMng.set((uint)i, (uint)j);
						}
						else
						{
							m_FlagMng.reset((uint)i, (uint)j);
						}
					}
				}
			}

			public void initializeLocalFlag(bool set_reset)
			{
				uint num = 10u;
				for (int i = 0; i < 1000; i++)
				{
					if (!set_reset)
					{
						m_FlagMng.set(num, (uint)i);
					}
					else
					{
						m_FlagMng.reset(num, (uint)i);
					}
				}
			}

			public void initializeValue()
			{
				for (uint num = 0u; num < 100; num++)
				{
					m_ValueMng.set(0u, num, 0);
				}
			}

			public void into(Array _AddrA, Array _AddrB)
			{
				m_EventStop = false;
				m_MessageStop = false;
				m_Event = false;
				m_ItemEvent = false;
				m_UseItemId = 0;
				initializeLocalFlag(set_reset: true);
				setUpGlobalData(_AddrA);
				setUpScriptData(_AddrB);
			}

			public void setUpGlobalData(Array _Addr)
			{
				m_GlobalScript = null;
				if (_Addr != null)
				{
					m_GlobalScript = ScriptData.cast(_Addr);
					m_LogicMng.registGlobalScriptData(m_GlobalScript);
				}
			}

			public void setUpScriptData(Array _Addr)
			{
				m_MapScript = null;
				if (_Addr != null)
				{
					m_MapScript = ScriptData.cast(_Addr);
					m_LogicMng.registScriptData(m_MapScript);
				}
			}

			public void startAllMapLogic()
			{
				for (uint num = 0u; num < 48; num++)
				{
					if (CHichParameterManager.getInstance().getHichManSubParam((int)num).m_Kind == CHichManParameter.KIND.KIND_MAP_LOGIC && isEnableLogic((uint)CHichParameterManager.getInstance().getHichManSubParam((int)num).m_Id) == 0)
					{
						startLogic((uint)CHichParameterManager.getInstance().getHichManSubParam((int)num).m_Id);
					}
				}
			}

			public CEventManager()
			{
				for (int i = 0; i < scriptDatas_.Length; i++)
				{
					scriptDatas_[i] = new ScriptData();
				}
				for (int i = 0; i < logicHandles_.Length; i++)
				{
					logicHandles_[i] = new LogicContext();
				}
				for (int i = 0; i < logicDatas_.Length; i++)
				{
					logicDatas_[i] = new LogicContext();
				}
				m_LogicMng = LogicManager.singleton();
				m_FlagMng = FlagManager.singleton();
				m_ValueMng = ValueManager.singleton();
				m_GlobalScript = null;
				m_MapScript = null;
				m_MessageStop = false;
				m_Event = false;
				m_PartyTalkEvent = false;
				m_ItemEvent = false;
			}

			public static CEventManager getInstance()
			{
				return m_Instance;
			}

			public void startLogic(uint castNo)
			{
				if (m_MapScript != null)
				{
					m_LogicMng.startLogic(m_MapScript.getMapNo(), castNo);
				}
			}

			public void stopLogic(uint castNo)
			{
				if (m_MapScript != null)
				{
					m_LogicMng.stopLogic(m_MapScript.getMapNo(), castNo);
				}
			}

			public int isEnableLogic(uint castNo)
			{
				if (m_MapScript == null)
				{
					return 0;
				}
				return m_LogicMng.isEnableLogic(m_MapScript.getMapNo(), castNo);
			}

			public LogicManager LogicMng()
			{
				return m_LogicMng;
			}

			public FlagManager FlagMng()
			{
				return m_FlagMng;
			}

			public ValueManager ValueMng()
			{
				return m_ValueMng;
			}

			public ScriptData getpScriptData(uint _Index)
			{
				return scriptDatas_[_Index];
			}

			public Logic getLogicHandles(uint _Index)
			{
				return logicHandles_[_Index];
			}

			public Logic getLogicDatas(uint _Index)
			{
				return logicDatas_[_Index];
			}

			public void setEventStop(bool _EventStop)
			{
				m_EventStop = _EventStop;
			}

			public bool EventStop()
			{
				return m_EventStop;
			}

			public void setMessageStop(bool _MessageStop)
			{
				m_MessageStop = _MessageStop;
			}

			public bool MessageStop()
			{
				return m_MessageStop;
			}

			public void MessageStop_set(bool arg0)
			{
				m_MessageStop = arg0;
			}

			public void setEvent(bool _Event)
			{
				if (m_Event != _Event)
				{
					OpenFF.Client.EngineHooks.Cutscene(_Event);
				}
				m_Event = _Event;
			}

			public bool isEvent()
			{
				return m_Event;
			}

			public void setPartyTalkEvent(bool _PartyTalkEvent)
			{
				m_PartyTalkEvent = _PartyTalkEvent;
			}

			public bool isPartyTalkEvent()
			{
				return m_PartyTalkEvent;
			}

			public void setItemEvent(bool _ItemEvent)
			{
				m_ItemEvent = _ItemEvent;
			}

			public bool isItemEvent()
			{
				return m_ItemEvent;
			}

			public void setUseItemId(int _UseItemId)
			{
				m_UseItemId = _UseItemId;
			}

			public int getUseItemId()
			{
				return m_UseItemId;
			}

			public void cleanUpGlobalData()
			{
				m_GlobalScript = null;
			}

			public void cleanUpScriptData()
			{
				if (m_MapScript != null)
				{
					m_LogicMng.removeScriptData(m_MapScript.getMapNo());
					m_MapScript = null;
				}
			}
		}
	}
}
