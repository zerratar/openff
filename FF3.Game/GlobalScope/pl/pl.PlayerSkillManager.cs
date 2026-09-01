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
	public static partial class pl
	{
		public class PlayerSkillManager
		{
			private Skill[] skill_ = new Skill[3];

			public PlayerSkillManager()
			{
				for (int i = 0; i < skill_.Length; i++)
				{
					skill_[i] = new Skill();
				}
			}

			public void initialize()
			{
				for (int i = 0; i < 3; i++)
				{
					skill_[i].initialize();
				}
			}

			public Skill skill(GET_SKILL_TYPE type)
			{
				return skill_[(int)type];
			}

			public void setDefault()
			{
				for (int i = 0; i < 3; i++)
				{
					skill_[i].setDefault();
				}
			}

			public void copy(PlayerSkillManager src)
			{
				for (int i = 0; i < 3; i++)
				{
					skill_[i].copy(src.skill_[i]);
				}
			}

			public void parse(ArrayReader reader)
			{
				for (int i = 0; i < 3; i++)
				{
					skill_[i].parse(reader);
				}
			}

			public void store(ArrayWriter writer)
			{
				for (int i = 0; i < 3; i++)
				{
					skill_[i].store(writer);
				}
			}
		}
	}
}
