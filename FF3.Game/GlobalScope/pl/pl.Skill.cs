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
		public class Skill
		{
			private static byte SKILL_EXP_MAX = 99;

			private ys.ParameterPoint<int> skillLevel_ = new ys.ParameterPoint<int>(1, 99);

			private ys.ParameterPoint<int> skillExp_ = new ys.ParameterPoint<int>(0, 99);

			private ys.ParameterPoint<int> poolSkillExp_ = new ys.ParameterPoint<int>(0, 99);

			public void initialize()
			{
				skillLevel_.min();
				skillExp_.min();
				poolSkillExp_.min();
			}

			public void addPoolSkillExp(byte value)
			{
				poolSkillExp_.add(value);
			}

			public bool skillExpPlusPoolSkillExp()
			{
				byte b = (byte)poolSkillExp().get();
				byte b2 = (byte)skillExp().get();
				byte b3 = (byte)(b + b2);
				bool result = false;
				if (b3 >= SKILL_EXP_MAX)
				{
					byte value = (byte)(b3 - SKILL_EXP_MAX);
					skillExp().set(value);
					if (skillLevel().get() < SKILL_EXP_MAX)
					{
						skillLevel().add(1);
						if (skillLevel().get() >= 99)
						{
							UserInfo.AwardAchievement(16);
							bool flag = true;
							for (int i = 0; i < 4; i++)
							{
								for (int j = 0; j < 23; j++)
								{
									if (PlayerParty.instance().player((byte)i).jobManager()
										.job((JOB_TYPE)j)
										.skill()
										.skillLevel()
										.get() < 99)
									{
										flag = false;
										break;
									}
								}
								if (!flag)
								{
									break;
								}
							}
							if (flag)
							{
								UserInfo.AwardAchievement(17);
							}
						}
						result = true;
					}
				}
				else
				{
					skillExp().add(poolSkillExp().get());
				}
				clearPoolSkillExp();
				return result;
			}

			public ys.ParameterPoint<int> skillLevel()
			{
				return skillLevel_;
			}

			public ys.ParameterPoint<int> skillExp()
			{
				return skillExp_;
			}

			public ys.ParameterPoint<int> poolSkillExp()
			{
				return poolSkillExp_;
			}

			public void clearPoolSkillExp()
			{
				poolSkillExp_.min();
			}

			public void setDefault()
			{
				skillLevel_.set(0);
				skillExp_.set(0);
				poolSkillExp_.set(0);
			}

			public void copy(Skill src)
			{
				skillLevel_.set(src.skillLevel_.get());
				skillExp_.set(src.skillExp_.get());
				poolSkillExp_.set(src.poolSkillExp_.get());
			}

			public void parse(ArrayReader reader)
			{
				skillLevel_.set(reader.readInt32());
				skillExp_.set(reader.readInt32());
				poolSkillExp_.set(reader.readInt32());
			}

			public void store(ArrayWriter writer)
			{
				writer.writeInt32(skillLevel_.get());
				writer.writeInt32(skillExp_.get());
				writer.writeInt32(poolSkillExp_.get());
			}
		}
	}
}
