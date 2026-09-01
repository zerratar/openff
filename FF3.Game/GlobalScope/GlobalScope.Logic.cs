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
	public class Logic
	{
		public enum STATUS_TYPE
		{
			DISABLE,
			READY,
			CONSTRUCTOR,
			EXECUTE,
			DESTRUCTOR
		}

		public const STATUS_TYPE DISABLE = STATUS_TYPE.DISABLE;

		public const STATUS_TYPE READY = STATUS_TYPE.READY;

		public const STATUS_TYPE CONSTRUCTOR = STATUS_TYPE.CONSTRUCTOR;

		public const STATUS_TYPE EXECUTE = STATUS_TYPE.EXECUTE;

		public const STATUS_TYPE DESTRUCTOR = STATUS_TYPE.DESTRUCTOR;

		protected static uint STACK_SIZE = 8u;

		protected uint mapNo_;

		protected CastInfo castInfo_;

		protected ScriptData scriptData_;

		protected uint pc_;

		protected STATUS_TYPE status_;

		protected ScriptData[] scriptDataStack_ = new ScriptData[STACK_SIZE];

		protected uint[] pcStack_ = new uint[STACK_SIZE];

		protected uint sp_;

		protected uint wait_;

		~Logic()
		{
		}

		public uint getMapNo()
		{
			return mapNo_;
		}

		public uint getCastNo()
		{
			return castInfo_.getCastNo();
		}

		public int isEnable()
		{
			if (status_ == STATUS_TYPE.DISABLE)
			{
				return 0;
			}
			return 1;
		}

		public void setCastInfo(ScriptData scriptData, CastInfo castInfo)
		{
			mapNo_ = scriptData.getMapNo();
			castInfo_ = castInfo;
			scriptData_ = scriptData;
			status_ = STATUS_TYPE.READY;
			pc_ = CastInfo.INVALID_SCRIPT;
			sp_ = 0u;
			wait_ = 0u;
		}

		public void setEnable()
		{
			pc_ = castInfo_.getConstructor();
			status_ = STATUS_TYPE.CONSTRUCTOR;
			if (pc_ == CastInfo.INVALID_SCRIPT)
			{
				setExecute();
			}
		}

		public void setExecute()
		{
			status_ = STATUS_TYPE.EXECUTE;
			pc_ = castInfo_.getNormal();
		}

		public void setDisable()
		{
			status_ = STATUS_TYPE.DESTRUCTOR;
			pc_ = castInfo_.getDestructor();
			if (pc_ == CastInfo.INVALID_SCRIPT)
			{
				status_ = STATUS_TYPE.DISABLE;
			}
		}
	}
}
