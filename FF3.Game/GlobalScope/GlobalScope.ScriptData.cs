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
	public class ScriptData
	{
		private sbyte[] header_ = new sbyte[4];

		private ushort majorNo_;

		private ushort minorNo_;

		private ScriptFunctionTable functionTable_;

		private ushort mapNo_;

		private ushort numCast_;

		private CastInfo[] castInfo_;

		public byte[] m_abyData;

		public static ScriptData cast(Array data)
		{
			if (memcmp(data, "MHCS", 4) != 0)
			{
				return null;
			}
			ScriptData scriptData = (ScriptData)data;
			if (scriptData.majorNo_ != 1 || scriptData.minorNo_ != 1)
			{
				return null;
			}
			return scriptData;
		}

		public uint getMapNo()
		{
			return mapNo_;
		}

		public CastInfo getCastInfo(uint castNo)
		{
			for (uint num = 0u; num < numCast_; num++)
			{
				if (castInfo_[num].getCastNo() == castNo)
				{
					return castInfo_[num];
				}
			}
			return null;
		}

		public uint getFunctionOffset(uint id)
		{
			return functionTable_.getOffset(id);
		}

		public static explicit operator ScriptData(Array src)
		{
			ScriptData scriptData = new ScriptData();
			ArrayReader arrayReader = new ArrayReader(src);
			arrayReader.read(scriptData.header_, 0, 4);
			scriptData.majorNo_ = arrayReader.readUInt16();
			scriptData.minorNo_ = arrayReader.readUInt16();
			uint num = arrayReader.readUInt32();
			scriptData.mapNo_ = arrayReader.readUInt16();
			scriptData.numCast_ = arrayReader.readUInt16();
			scriptData.castInfo_ = new CastInfo[scriptData.numCast_];
			for (int i = 0; i < scriptData.numCast_; i++)
			{
				scriptData.castInfo_[i] = new CastInfo();
				scriptData.castInfo_[i].parse(arrayReader);
			}
			arrayReader.setPosition(num);
			scriptData.functionTable_ = new ScriptFunctionTable();
			scriptData.functionTable_.parse(arrayReader);
			arrayReader.dispose();
			scriptData.m_abyData = (byte[])src;
			return scriptData;
		}
	}
}
