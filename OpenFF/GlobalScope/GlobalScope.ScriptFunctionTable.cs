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
	public class ScriptFunctionTable
	{
		private uint numFunction_;

		private FunctionRecord[] table_;

		public void parse(ArrayReader reader)
		{
			numFunction_ = reader.readUInt32();
			table_ = new FunctionRecord[numFunction_];
			for (int i = 0; i < numFunction_; i++)
			{
				table_[i] = new FunctionRecord();
				table_[i].parse(reader);
			}
		}

		public uint getOffset(uint id)
		{
			uint num = 0u;
			uint num2 = numFunction_ - 1;
			uint num3 = (num + num2) / 2;
			while (table_[num3].id_ != id)
			{
				if (table_[num3].id_ < id)
				{
					num = num3 + 1;
				}
				else
				{
					num2 = num3 - 1;
				}
				num3 = (num + num2) / 2;
			}
			return table_[num3].offset_;
		}
	}
}
