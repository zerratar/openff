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
	public static partial class eld
	{
		public class SEQ_MOVE_PATH
		{
			private uint command;

			private uint pathNo;

			private uint target_id;

			private uint res;

			public static explicit operator SEQ_MOVE_PATH(ArrayReader src)
			{
				SEQ_MOVE_PATH sEQ_MOVE_PATH = new SEQ_MOVE_PATH();
				sEQ_MOVE_PATH.command = src.readUInt32();
				sEQ_MOVE_PATH.pathNo = src.readUInt32();
				sEQ_MOVE_PATH.target_id = src.readUInt32();
				sEQ_MOVE_PATH.res = src.readUInt32();
				return sEQ_MOVE_PATH;
			}
		}
	}
}
