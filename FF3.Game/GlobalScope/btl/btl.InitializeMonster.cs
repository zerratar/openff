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
		public class InitializeMonster
		{
			private short monsterId_;

			private short monsterPartyId_;

			private int monsterCountType_;

			public void initialize()
			{
				setMonsterId(INITIALIZE_MONSTER_ID);
				setMonsterPartyId(INITIALIZE_MONSTER_PARTY_ID);
				monsterCountType_ = 0;
			}

			public void setMonsterId(short _id)
			{
				monsterId_ = _id;
			}

			public short monsterId()
			{
				return monsterId_;
			}

			public void setMonsterPartyId(short _id)
			{
				monsterPartyId_ = _id;
			}

			public short monsterPartyId()
			{
				return monsterPartyId_;
			}

			public int monsterCountType()
			{
				return monsterCountType_;
			}

			public void monsterCountType_set(int arg0)
			{
				monsterCountType_ = arg0;
			}

			public void monsterCountType_inc()
			{
				monsterCountType_++;
			}

			public void monsterCountType_dec()
			{
				monsterCountType_--;
			}
		}
	}
}
