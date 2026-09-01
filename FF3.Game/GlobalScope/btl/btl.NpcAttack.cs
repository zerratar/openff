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
	public static partial class btl
	{
		public class NpcAttack
		{
			public short attackId_;

			public short attackArea_;

			public byte target_;

			public byte attackOdds_;

			public static explicit operator NpcAttack(ArrayReader src)
			{
				NpcAttack npcAttack = new NpcAttack();
				npcAttack.attackId_ = src.readInt16();
				npcAttack.attackArea_ = src.readInt16();
				npcAttack.target_ = src.readByte();
				npcAttack.attackOdds_ = src.readByte();
				return npcAttack;
			}
		}
	}
}
