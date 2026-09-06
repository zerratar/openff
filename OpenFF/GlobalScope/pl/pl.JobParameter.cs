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
using OpenFF.Platform;
using OpenFF.Resources;

internal static partial class GlobalScope
{
	public static partial class pl
	{
		public class JobParameter
		{
			private Skill skill_ = new Skill();

			private JobBonusParameter bonusParameter_ = new JobBonusParameter();

			private AbilityManager ability_ = new AbilityManager();

			public void initialize()
			{
				skill_.initialize();
				bonusParameter_.initialize();
			}

			public Skill skill()
			{
				return skill_;
			}

			public JobBonusParameter jobBonusParameter()
			{
				return bonusParameter_;
			}

			public AbilityManager ability()
			{
				return ability_;
			}

			public void setDefault()
			{
				skill_.setDefault();
				bonusParameter_.setDefault();
				ability_.setDefault();
			}

			public void copy(JobParameter src)
			{
				skill_.copy(src.skill_);
				bonusParameter_.copy(src.bonusParameter_);
				ability_.copy(src.ability_);
			}

			public void parse(ArrayReader reader)
			{
				skill_.parse(reader);
				bonusParameter_.parse(reader);
				ability_.parse(reader);
			}

			public void store(ArrayWriter writer)
			{
				skill_.store(writer);
				bonusParameter_.store(writer);
				ability_.store(writer);
			}
		}
	}
}
