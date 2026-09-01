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
		public class PlayerJobManager
		{
			private int nowJob_;

			private int prevJob_;

			private JobParameter[] job_ = new JobParameter[23];

			private Command command_ = new Command();

			public PlayerJobManager()
			{
				for (int i = 0; i < job_.Length; i++)
				{
					job_[i] = new JobParameter();
				}
			}

			public void initialize()
			{
				nowJob_ = 0;
				prevJob_ = nowJob_;
				for (int i = 0; i < 23; i++)
				{
					job_[i].initialize();
				}
				command_.initialize();
			}

			public bool setNowJob(JOB_TYPE job)
			{
				prevJob_ = nowJob_;
				nowJob_ = (int)job;
				return true;
			}

			public void addJobSkillExp()
			{
				int num = JobSkillExp[nowJob()];
				if (nowJob() != 0 && nowJob() != 10 && job_[nowJob()].skill().skillLevel().get() < 14)
				{
					num = JobSkillExp[0];
				}
				job_[nowJob()].skill().addPoolSkillExp((byte)num);
			}

			public bool jobSkillExpPlusPoolSkillExp()
			{
				return job_[nowJob()].skill().skillExpPlusPoolSkillExp();
			}

			public void clearJobPoolSkillExp()
			{
				job_[nowJob()].skill().clearPoolSkillExp();
			}

			public bool checkRegisterPassiveAbility(int abilityId)
			{
				for (int i = 0; i < PASSIVE_ABILITY_MAX; i++)
				{
					if (nowJobParameter().ability().playerAbility().passive_[i] == abilityId)
					{
						return true;
					}
				}
				return false;
			}

			public int nowJob()
			{
				return nowJob_;
			}

			public int prevJob()
			{
				return prevJob_;
			}

			public JobParameter job(JOB_TYPE type)
			{
				return job_[(int)type];
			}

			public JobParameter nowJobParameter()
			{
				return job_[nowJob_];
			}

			public Command command()
			{
				return command_;
			}

			public void setDefault()
			{
				nowJob_ = 0;
				prevJob_ = 0;
				for (int i = 0; i < 23; i++)
				{
					job_[i].setDefault();
				}
				command_.setDefault();
			}

			public void copy(PlayerJobManager src)
			{
				nowJob_ = src.nowJob_;
				prevJob_ = src.prevJob_;
				for (int i = 0; i < 23; i++)
				{
					job_[i].copy(src.job_[i]);
				}
				command_.copy(src.command_);
			}

			public void parse(ArrayReader reader)
			{
				nowJob_ = reader.readInt32();
				prevJob_ = reader.readInt32();
				for (int i = 0; i < 23; i++)
				{
					job_[i].parse(reader);
				}
				command_.parse(reader);
			}

			public void store(ArrayWriter writer)
			{
				writer.writeInt32(nowJob_);
				writer.writeInt32(prevJob_);
				for (int i = 0; i < 23; i++)
				{
					job_[i].store(writer);
				}
				command_.store(writer);
			}
		}
	}
}
