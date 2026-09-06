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
		public class JobBonusParameter
		{
			private ys.ParameterPoint<int> criticalProbability_ = new ys.ParameterPoint<int>(0, 99);

			private ys.ParameterPoint<int> criticalDamage_ = new ys.ParameterPoint<int>(0, 99);

			private ys.ParameterPoint<int> escapeProbability_ = new ys.ParameterPoint<int>(0, 99);

			private ys.ParameterPoint<int> healProbability_ = new ys.ParameterPoint<int>(0, 99);

			public void initialize()
			{
				criticalProbability_.min();
				criticalDamage_.min();
				escapeProbability_.min();
				healProbability_.min();
			}

			public ys.ParameterPoint<int> criticalProbability()
			{
				return criticalProbability_;
			}

			public ys.ParameterPoint<int> criticalDamage()
			{
				return criticalDamage_;
			}

			public ys.ParameterPoint<int> escapeProbability()
			{
				return escapeProbability_;
			}

			public ys.ParameterPoint<int> healProbability()
			{
				return healProbability_;
			}

			public void setDefault()
			{
				criticalProbability_.set(0);
				criticalDamage_.set(0);
				escapeProbability_.set(0);
				healProbability_.set(0);
			}

			public void copy(JobBonusParameter src)
			{
				criticalProbability_.set(src.criticalProbability_.get());
				criticalDamage_.set(src.criticalDamage_.get());
				escapeProbability_.set(src.escapeProbability_.get());
				healProbability_.set(src.healProbability_.get());
			}

			public void parse(ArrayReader reader)
			{
				criticalProbability_.set(reader.readInt32());
				criticalDamage_.set(reader.readInt32());
				escapeProbability_.set(reader.readInt32());
				healProbability_.set(reader.readInt32());
			}

			public void store(ArrayWriter writer)
			{
				writer.writeInt32(criticalProbability_.get());
				writer.writeInt32(criticalDamage_.get());
				writer.writeInt32(escapeProbability_.get());
				writer.writeInt32(healProbability_.get());
			}
		}
	}
}
