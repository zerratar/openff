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
		public class Mania
		{
			private ys.ParameterPoint<int> clearTime_ = new ys.ParameterPoint<int>(0, 9999999);

			private ys.ParameterPoint<int> clearNumber_ = new ys.ParameterPoint<int>(0, 9999999);

			private ys.ParameterPoint<int> treasureHuntRate_ = new ys.ParameterPoint<int>(0, 100);

			private ys.ParameterPoint<int> enemyBreakNumber_ = new ys.ParameterPoint<int>(0, 9999999);

			private ys.ParameterPoint<int> escapeNumber_ = new ys.ParameterPoint<int>(0, 9999999);

			private ys.ParameterPoint<int> maxDamage_ = new ys.ParameterPoint<int>(0, 9999999);

			private ys.ParameterPoint<int> maxHitNumber_ = new ys.ParameterPoint<int>(0, 9999999);

			private ys.ParameterPoint<int> tresureCount_ = new ys.ParameterPoint<int>(0, ys.TREASURE_BOX_MAX);

			public void initialize()
			{
				clearTime().min();
				clearNumber().min();
				treasureHuntRate().min();
				enemyBreakNumber().min();
				escapeNumber().min();
				maxDamage().min();
				maxHitNumber().min();
				tresureCount_.min();
			}

			public void setMaxDamage(int max)
			{
				maxDamage().set(ds.max(maxDamage().get(), max));
			}

			public void setMaxHitNumber(int max)
			{
				maxHitNumber().set(ds.max(maxHitNumber().get(), max));
			}

			public void countTresureBox()
			{
				tresureCount_.add(1);
				treasureHuntRate().set((byte)(100 * tresureCount_.get() / ys.TREASURE_BOX_MAX));
				int num = PlayerParty.instance().mania().treasureHuntRate()
					.get();
				if (num >= 50)
				{
					UserInfo.AwardAchievement(10);
					if (num >= 100)
					{
						UserInfo.AwardAchievement(11);
					}
				}
			}

			public void setTreasureHuntRate(ys.ParameterPoint<int> rate)
			{
				treasureHuntRate_ = rate;
				tresureCount_.set(treasureHuntRate_.get() * ys.TREASURE_BOX_MAX / 100);
			}

			public ys.ParameterPoint<int> clearTime()
			{
				return clearTime_;
			}

			public ys.ParameterPoint<int> clearNumber()
			{
				return clearTime_;
			}

			public ys.ParameterPoint<int> treasureHuntRate()
			{
				return treasureHuntRate_;
			}

			public ys.ParameterPoint<int> enemyBreakNumber()
			{
				return enemyBreakNumber_;
			}

			public ys.ParameterPoint<int> escapeNumber()
			{
				return escapeNumber_;
			}

			public ys.ParameterPoint<int> maxDamage()
			{
				return maxDamage_;
			}

			public ys.ParameterPoint<int> maxHitNumber()
			{
				return maxHitNumber_;
			}

			public void setDefault()
			{
				clearTime_.set(0);
				clearNumber_.set(0);
				treasureHuntRate_.set(0);
				enemyBreakNumber_.set(0);
				escapeNumber_.set(0);
				maxDamage_.set(0);
				maxHitNumber_.set(0);
				tresureCount_.set(0);
			}

			public void copy(Mania src)
			{
				clearTime_.set(src.clearTime_.get());
				clearNumber_.set(src.clearNumber_.get());
				treasureHuntRate_.set(src.treasureHuntRate_.get());
				enemyBreakNumber_.set(src.enemyBreakNumber_.get());
				escapeNumber_.set(src.escapeNumber_.get());
				maxDamage_.set(src.maxDamage_.get());
				maxHitNumber_.set(src.maxHitNumber_.get());
				tresureCount_.set(src.tresureCount_.get());
			}

			public void parse(ArrayReader reader)
			{
				clearTime_.set(reader.readInt32());
				clearNumber_.set(reader.readInt32());
				treasureHuntRate_.set(reader.readInt32());
				enemyBreakNumber_.set(reader.readInt32());
				escapeNumber_.set(reader.readInt32());
				maxDamage_.set(reader.readInt32());
				maxHitNumber_.set(reader.readInt32());
				tresureCount_.set(reader.readInt32());
			}

			public void store(ArrayWriter writer)
			{
				writer.writeInt32(clearTime_.get());
				writer.writeInt32(clearNumber_.get());
				writer.writeInt32(treasureHuntRate_.get());
				writer.writeInt32(enemyBreakNumber_.get());
				writer.writeInt32(escapeNumber_.get());
				writer.writeInt32(maxDamage_.get());
				writer.writeInt32(maxHitNumber_.get());
				writer.writeInt32(tresureCount_.get());
			}
		}
	}
}
