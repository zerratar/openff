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
	public static partial class btl
	{
		public class InitializeNPC
		{
			private int npcId_;

			private int attackType_;

			public void initialize()
			{
				npcId_ = -1;
				attackType_ = -1;
			}

			public void setNpcId(int _id)
			{
				npcId_ = _id;
			}

			public int npcId()
			{
				return npcId_;
			}

			public void setAttackType(int type)
			{
				attackType_ = type;
			}

			public int attackType()
			{
				return attackType_;
			}
		}
	}
}
