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
	public static partial class pl
	{
		public enum NPC_RANDOM_MOVE_TYPE
		{
			NPC_RANDOM_MOVE_TYPE_ERR = -1,
			NPC_RANDOM_MOVE_TYPE_DEFAULT,
			NPC_RANDOM_MOVE_TYPE_MAN,
			NPC_RANDOM_MOVE_TYPE_WOMAN,
			NPC_RANDOM_MOVE_TYPE_BOY,
			NPC_RANDOM_MOVE_TYPE_GIRL,
			NPC_RANDOM_MOVE_TYPE_UNCLE,
			NPC_RANDOM_MOVE_TYPE_AUNT,
			NPC_RANDOM_MOVE_TYPE_OLD_MAN,
			NPC_RANDOM_MOVE_TYPE_OLD_WOMAN,
			NPC_RANDOM_MOVE_TYPE_MAX
		}
	}
}
