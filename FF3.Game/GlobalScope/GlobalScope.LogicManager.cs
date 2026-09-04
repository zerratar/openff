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
	public class LogicManager
	{
		public static uint MAX_NUM_MAP;

		public static uint MAX_NUM_LOGIC;

		private static LogicManager instance_ = new LogicManager();

		private ScriptData[] scriptDatas_;

		private uint numMap_;

		private Logic[] logicHandles_;

		private Logic[] logicDatas_;

		private uint numLogic_;

		private ScriptEngine scriptEngine_ = new ScriptEngine();

		public static LogicManager singleton()
		{
			return instance_;
		}

		public void init(uint maxNumMap, uint maxNumLogic, ScriptData[] scriptDatas, Logic[] logicHandles, Logic[] logicDatas)
		{
			MAX_NUM_MAP = maxNumMap;
			MAX_NUM_LOGIC = maxNumLogic;
			scriptDatas_ = scriptDatas;
			logicHandles_ = logicHandles;
			logicDatas_ = logicDatas;
			for (uint num = 0u; num < MAX_NUM_LOGIC; num++)
			{
				logicHandles_[num] = logicDatas_[num];
			}
		}

		public LogicManager()
		{
			numMap_ = 0u;
			numLogic_ = 0u;
		}

		~LogicManager()
		{
		}

		public void removeAllLogic()
		{
			numLogic_ = 0u;
		}

		public void registGlobalScriptData(ScriptData scriptData)
		{
			scriptEngine_.registGlobalScriptData(scriptData);
		}

		public void registScriptData(ScriptData scriptData)
		{
			if (isRegistScriptData(scriptData.getMapNo()) == 0)
			{
				scriptDatas_[numMap_] = scriptData;
				numMap_++;
			}
		}

		public void removeScriptData(uint mapNo)
		{
			uint num;
			for (num = 0u; num < numMap_ && scriptDatas_[num].getMapNo() != mapNo; num++)
			{
			}
			if (num < numMap_)
			{
				numMap_--;
				for (; num < numMap_; num++)
				{
					scriptDatas_[num] = scriptDatas_[num + 1];
				}
			}
		}

		public int isRegistScriptData(uint mapNo)
		{
			for (uint num = 0u; num < numMap_; num++)
			{
				if (scriptDatas_[num].getMapNo() == mapNo)
				{
					return 1;
				}
			}
			return 0;
		}

		public void startLogic(uint mapNo, uint castNo)
		{
			if (isEnableLogic(mapNo, castNo) == 0)
			{
				CastInfo castInfoArray = getCastInfoArray(mapNo, castNo);
				if (castInfoArray == null)
				{
					// PORT: a character with no cast in the map's script (one the engine API put
					// there) has nothing to run when talked to.
					return;
				}
				Logic logic = logicHandles_[numLogic_];
				numLogic_++;
				uint scriptDataIdxByMapNo = getScriptDataIdxByMapNo(mapNo);
				logic.setCastInfo(scriptDatas_[scriptDataIdxByMapNo], castInfoArray);
				logic.setEnable();
			}
		}

		public void stopLogic(uint mapNo, uint castNo)
		{
			getLogic(mapNo, castNo)?.setDisable();
		}

		public int isEnableLogic(uint mapNo, uint castNo)
		{
			return getLogic(mapNo, castNo)?.isEnable() ?? 0;
		}

		public void execute()
		{
			for (uint num = 0u; num < numLogic_; num++)
			{
				if (logicHandles_[num].isEnable() != 0)
				{
					scriptEngine_.execute(logicHandles_[num]);
				}
			}
			removeLogic();
		}

		public Logic getLogic(uint mapNo, uint castNo)
		{
			Logic logic = null;
			for (uint num = 0u; num < numLogic_; num++)
			{
				logic = logicHandles_[num];
				if (logic.getMapNo() == mapNo && logic.getCastNo() == castNo)
				{
					return logic;
				}
			}
			return null;
		}

		public void removeLogic()
		{
			Logic logic = null;
			for (int num = (int)(numLogic_ - 1); num >= 0; num--)
			{
				if (logicHandles_[num].isEnable() == 0)
				{
					logic = logicHandles_[num];
					for (int i = num; i < (int)(numLogic_ - 1); i++)
					{
						logicHandles_[i] = logicHandles_[i + 1];
					}
					numLogic_--;
					logicHandles_[numLogic_] = logic;
				}
			}
		}

		public int isScriptingCast(uint mapNo, uint castNo)
		{
			CastInfo castInfoArray = getCastInfoArray(mapNo, castNo);
			if (castInfoArray != null)
			{
				if (castInfoArray.getConstructor() != CastInfo.INVALID_SCRIPT)
				{
					return 1;
				}
				if (castInfoArray.getNormal() != CastInfo.INVALID_SCRIPT)
				{
					return 1;
				}
				if (castInfoArray.getDestructor() != CastInfo.INVALID_SCRIPT)
				{
					return 1;
				}
				return 0;
			}
			return 0;
		}

		public uint getScriptDataIdxByMapNo(uint mapNo)
		{
			for (uint num = 0u; num < numMap_; num++)
			{
				if (scriptDatas_[num].getMapNo() == mapNo)
				{
					return num;
				}
			}
			return 0u;
		}

		public CastInfo getCastInfoArray(uint mapNo, uint castNo)
		{
			uint scriptDataIdxByMapNo = getScriptDataIdxByMapNo(mapNo);
			return scriptDatas_[scriptDataIdxByMapNo].getCastInfo(castNo);
		}
	}
}
