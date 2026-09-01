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
	public class ScriptEngine
	{
		private LogicContext logicContext_;

		private ScriptData globalScriptData_;

		private ScriptData scriptData_;

		private uint pc_;

		private int suspend_;

		private uint resumePC_;

		~ScriptEngine()
		{
		}

		public uint getCastNo()
		{
			return logicContext_.getCastNo();
		}

		public void execute(Logic logic)
		{
			logicContext_ = (LogicContext)logic;
			if (logicContext_.isWaiting() == 0)
			{
				logicContext_.load(out scriptData_, out pc_);
				suspend_ = 0;
				while (suspend_ == 0)
				{
					resumePC_ = pc_;
					uint num = fetch();
					commandTable[num](this);
				}
				logicContext_.save(scriptData_, resumePC_);
			}
		}

		public uint fetch()
		{
			return getWord();
		}

		public void jump(uint toPC)
		{
			pc_ = toPC;
		}

		public void call(uint library, uint id)
		{
			logicContext_.push(scriptData_, pc_);
			if (library == 2)
			{
				scriptData_ = globalScriptData_;
			}
			pc_ = scriptData_.getFunctionOffset(id);
		}

		public void scriptReturn()
		{
			logicContext_.pop();
			logicContext_.load(out scriptData_, out pc_);
		}

		public void end()
		{
			suspend_ = ((logicContext_.nextStatus() == 0) ? 1 : 0);
			logicContext_.load(out scriptData_, out pc_);
		}

		public void suspendRedo()
		{
			suspend_ = 1;
		}

		public void wait(uint frame)
		{
			logicContext_.setWait(frame);
			suspend_ = 1;
			resumePC_ = pc_;
		}

		public byte getByte()
		{
			byte result = reinterpret_cast<byte[]>(scriptData_.m_abyData)[pc_];
			pc_++;
			return result;
		}

		public ushort getWord()
		{
			return getFromByteDatas_ushort(reinterpret_cast<byte[]>(scriptData_.m_abyData), ref pc_);
		}

		public uint getDword()
		{
			return getFromByteDatas_uint(reinterpret_cast<byte[]>(scriptData_.m_abyData), ref pc_);
		}

		public string getString()
		{
			ArrayReader arrayReader = new ArrayReader(scriptData_.m_abyData);
			arrayReader.setPosition(pc_);
			string text = reinterpret_cast<string>(arrayReader.readString("UTF-8"));
			pc_ += (uint)(strlen(text) + 1);
			return text;
		}

		public int isEnableLogic(uint castNo)
		{
			return LogicManager.singleton().isEnableLogic(logicContext_.getMapNo(), castNo);
		}

		public void startLogic(uint castNo)
		{
			LogicManager.singleton().startLogic(logicContext_.getMapNo(), castNo);
		}

		public void stopLogic(uint castNo)
		{
			LogicManager.singleton().stopLogic(logicContext_.getMapNo(), castNo);
		}

		public void registGlobalScriptData(ScriptData globalScriptData)
		{
			globalScriptData_ = globalScriptData;
		}

		public uint getPC()
		{
			return pc_;
		}
	}
}
