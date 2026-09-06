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
	public static class ys
	{
		public class MPoint<value_type>
		{
			private value_type min_;

			private value_type max_;

			private value_type now_;

			private value_type limit_;

			public MPoint(value_type _min, value_type _max)
			{
				min_ = _min;
				max_ = _max;
				now_ = default(value_type);
				limit_ = default(value_type);
			}

			public value_type getNow()
			{
				return now_;
			}

			public void setNow(value_type value)
			{
				now_ = ds.clamp(value, min_, limit_);
			}

			public void minNow()
			{
				now_ = min_;
			}

			public void maxNow()
			{
				now_ = limit_;
			}

			public value_type addNow(value_type value)
			{
				value_type arg = ds.clamp(value, min_, limit_);
				if (!_isL(_sub(limit_, arg), now_))
				{
					return now_ = _add(now_, arg);
				}
				return now_ = limit_;
			}

			public value_type subNow(value_type value)
			{
				value_type arg = ds.clamp(value, min_, limit_);
				if (!_isG(_add(min_, arg), now_))
				{
					return now_ = _sub(now_, arg);
				}
				return now_ = min_;
			}

			public value_type getLimit()
			{
				return limit_;
			}

			public void setLimit(value_type value)
			{
				limit_ = ds.clamp(value, min_, max_);
			}

			public void minLimit()
			{
				limit_ = min_;
			}

			public void maxLimit()
			{
				limit_ = max_;
			}

			public value_type addLimit(value_type value)
			{
				value_type arg = ds.clamp(value, min_, max_);
				if (!_isL(_sub(max_, arg), limit_))
				{
					return limit_ = _add(limit_, arg);
				}
				return limit_ = max_;
			}

			public value_type subLimit(value_type value)
			{
				value_type arg = ds.clamp(value, min_, max_);
				if (!_isG(_add(min_, arg), limit_))
				{
					return limit_ = _sub(limit_, arg);
				}
				return limit_ = min_;
			}
		}

		public class ParameterPoint<value_type>
		{
			private value_type min_;

			private value_type max_;

			private value_type now_;

			public ParameterPoint(value_type _min, value_type _max)
			{
				min_ = _min;
				max_ = _max;
				now_ = default(value_type);
			}

			public value_type get()
			{
				return now_;
			}

			public void set(value_type value)
			{
				now_ = ds.clamp(value, min_, max_);
			}

			public void min()
			{
				now_ = min_;
			}

			public void max()
			{
				now_ = max_;
			}

			public value_type add(value_type value)
			{
				value_type arg = ds.clamp(value, min_, max_);
				if (!_isL(_sub(max_, arg), now_))
				{
					return now_ = _add(now_, arg);
				}
				return now_ = max_;
			}

			public value_type sub(value_type value)
			{
				value_type arg = ds.clamp(value, min_, max_);
				if (!_isG(_add(min_, arg), now_))
				{
					return now_ = _sub(now_, arg);
				}
				return now_ = min_;
			}
		}

		public class PhysicsAttackParameter
		{
			private ParameterPoint<int> aggressivity_ = new ParameterPoint<int>(0, 9999);

			private byte hitProbability_;

			private byte optionProbability_;

			private short optionMagicId_;

			private short armsAttribute_;

			private short attackType_;

			private short attackOption_;

			private short equipOption_;

			public void initialize()
			{
				aggressivity().min();
				hitProbability_ = 0;
				optionProbability_ = 0;
				optionMagicId_ = 0;
				armsAttribute_ = 0;
				attackType_ = 0;
				attackOption_ = 0;
				equipOption_ = 0;
			}

			public ParameterPoint<int> aggressivity()
			{
				return aggressivity_;
			}

			public byte hitProbability()
			{
				return hitProbability_;
			}

			public void hitProbability_set(byte arg0)
			{
				hitProbability_ = arg0;
			}

			public byte optionProbability()
			{
				return optionProbability_;
			}

			public void optionProbability_set(byte arg0)
			{
				optionProbability_ = arg0;
			}

			public short optionMagicId()
			{
				return optionMagicId_;
			}

			public void optionMagicId_set(short arg0)
			{
				optionMagicId_ = arg0;
			}

			public short armsAttribute()
			{
				return armsAttribute_;
			}

			public void armsAttribute_set(short arg0)
			{
				armsAttribute_ = arg0;
			}

			public short attackType()
			{
				return attackType_;
			}

			public void attackType_set(short arg0)
			{
				attackType_ = arg0;
			}

			public short attackOption()
			{
				return attackOption_;
			}

			public void attackOption_set(short arg0)
			{
				attackOption_ = arg0;
			}

			public short equipOption()
			{
				return equipOption_;
			}

			public void equipOption_set(short arg0)
			{
				equipOption_ = arg0;
			}

			public void setDefault()
			{
				aggressivity_.set(0);
				hitProbability_ = 0;
				optionProbability_ = 0;
				optionMagicId_ = 0;
				armsAttribute_ = 0;
				attackType_ = 0;
				attackOption_ = 0;
				equipOption_ = 0;
			}

			public void copy(PhysicsAttackParameter src)
			{
				aggressivity_.set(src.aggressivity_.get());
				hitProbability_ = src.hitProbability_;
				optionProbability_ = src.optionProbability_;
				optionMagicId_ = src.optionMagicId_;
				armsAttribute_ = src.armsAttribute_;
				attackType_ = src.attackType_;
				attackOption_ = src.attackOption_;
				equipOption_ = src.equipOption_;
			}

			public void parse(ArrayReader reader)
			{
				aggressivity_.set(reader.readInt32());
				hitProbability_ = reader.readByte();
				optionProbability_ = reader.readByte();
				optionMagicId_ = reader.readInt16();
				armsAttribute_ = reader.readInt16();
				attackType_ = reader.readInt16();
				attackOption_ = reader.readInt16();
				equipOption_ = reader.readInt16();
			}

			public void store(ArrayWriter writer)
			{
				writer.writeInt32(aggressivity_.get());
				writer.writeByte(hitProbability_);
				writer.writeByte(optionProbability_);
				writer.writeInt16(optionMagicId_);
				writer.writeInt16(armsAttribute_);
				writer.writeInt16(attackType_);
				writer.writeInt16(attackOption_);
				writer.writeInt16(equipOption_);
			}
		}

		public class PhysicsDefenseParameter
		{
			private ParameterPoint<int> phylacticPower_ = new ParameterPoint<int>(0, 9999);

			private int avoidanceNumber_;

			private short armsWeakAttribute_;

			private short armsAttribute_;

			private short antiType_;

			private short antiOption_;

			private short equipOption_;

			protected byte _pad0;

			protected byte _pad1;

			public void initialize()
			{
				phylacticPower_.min();
				avoidanceNumber_ = 0;
				armsAttribute_ = 0;
				antiType_ = 0;
				antiOption_ = 0;
				equipOption_ = 0;
			}

			public ParameterPoint<int> phylacticPower()
			{
				return phylacticPower_;
			}

			public int avoidanceNumber()
			{
				return avoidanceNumber_;
			}

			public void avoidanceNumber_set(int arg0)
			{
				avoidanceNumber_ = arg0;
			}

			public void avoidanceNumber_add(int arg0)
			{
				avoidanceNumber_ += arg0;
			}

			public short armsWeakAttribute()
			{
				return armsWeakAttribute_;
			}

			public void armsWeakAttribute_or(short arg0)
			{
				armsWeakAttribute_ |= arg0;
			}

			public short armsAttribute()
			{
				return armsAttribute_;
			}

			public void armsAttribute_set(short arg0)
			{
				armsAttribute_ = arg0;
			}

			public void armsAttribute_or(short arg0)
			{
				armsAttribute_ |= arg0;
			}

			public short antiType()
			{
				return antiType_;
			}

			public void antiType_set(short arg0)
			{
				antiType_ = arg0;
			}

			public void antiType_or(short arg0)
			{
				antiType_ |= arg0;
			}

			public short antiOption()
			{
				return antiOption_;
			}

			public void antiOption_set(short arg0)
			{
				antiOption_ = arg0;
			}

			public void antiOption_or(short arg0)
			{
				antiOption_ |= arg0;
			}

			public short equipOption()
			{
				return equipOption_;
			}

			public void equipOption_set(short arg0)
			{
				equipOption_ = arg0;
			}

			public void setDefault()
			{
				phylacticPower_.set(0);
				avoidanceNumber_ = 0;
				armsWeakAttribute_ = 0;
				armsAttribute_ = 0;
				antiType_ = 0;
				antiOption_ = 0;
				equipOption_ = 0;
				_pad0 = 0;
				_pad1 = 0;
			}

			public void copy(PhysicsDefenseParameter src)
			{
				phylacticPower_.set(src.phylacticPower_.get());
				avoidanceNumber_ = src.avoidanceNumber_;
				armsWeakAttribute_ = src.armsWeakAttribute_;
				armsAttribute_ = src.armsAttribute_;
				antiType_ = src.antiType_;
				antiOption_ = src.antiOption_;
				equipOption_ = src.equipOption_;
				_pad0 = src._pad0;
				_pad1 = src._pad1;
			}

			public void parse(ArrayReader reader)
			{
				phylacticPower_.set(reader.readInt32());
				avoidanceNumber_ = reader.readInt32();
				armsWeakAttribute_ = reader.readInt16();
				armsAttribute_ = reader.readInt16();
				antiType_ = reader.readInt16();
				antiOption_ = reader.readInt16();
				equipOption_ = reader.readInt16();
				_pad0 = reader.readByte();
				_pad1 = reader.readByte();
			}

			public void store(ArrayWriter writer)
			{
				writer.writeInt32(phylacticPower_.get());
				writer.writeInt32(avoidanceNumber_);
				writer.writeInt16(armsWeakAttribute_);
				writer.writeInt16(armsAttribute_);
				writer.writeInt16(antiType_);
				writer.writeInt16(antiOption_);
				writer.writeInt16(equipOption_);
				writer.writeByte(_pad0);
				writer.writeByte(_pad1);
			}
		}

		public class MagicDefenseParameter
		{
			private short weakType_;

			private short magicPhylacticPower_;

			public void initialize()
			{
				weakType_ = 0;
				magicPhylacticPower_ = 0;
			}

			public short weakType()
			{
				return weakType_;
			}

			public void weakType_set(short arg0)
			{
				weakType_ = arg0;
			}

			public void weakType_or(short arg0)
			{
				weakType_ |= arg0;
			}

			public short magicPhylacticPower()
			{
				return magicPhylacticPower_;
			}

			public void magicPhylacticPower_set(short arg0)
			{
				magicPhylacticPower_ = arg0;
			}

			public void magicPhylacticPower_add(short arg0)
			{
				magicPhylacticPower_ += arg0;
			}

			public void setDefault()
			{
				weakType_ = 0;
				magicPhylacticPower_ = 0;
			}

			public void copy(MagicDefenseParameter src)
			{
				weakType_ = src.weakType_;
				magicPhylacticPower_ = src.magicPhylacticPower_;
			}

			public void parse(ArrayReader reader)
			{
				weakType_ = reader.readInt16();
				magicPhylacticPower_ = reader.readInt16();
			}

			public void store(ArrayWriter writer)
			{
				writer.writeInt16(weakType_);
				writer.writeInt16(magicPhylacticPower_);
			}
		}

		public class BodyParameter
		{
			private ParameterPoint<int> strength_ = new ParameterPoint<int>(0, 99);

			private ParameterPoint<int> vitality_ = new ParameterPoint<int>(0, 99);

			private ParameterPoint<int> dexterity_ = new ParameterPoint<int>(0, 99);

			private ParameterPoint<int> intelligence_ = new ParameterPoint<int>(0, 99);

			private ParameterPoint<int> mind_ = new ParameterPoint<int>(0, 99);

			public void initialize()
			{
				strength_.min();
				vitality_.min();
				dexterity_.min();
				intelligence_.min();
				mind_.min();
			}

			public ParameterPoint<int> strength()
			{
				return strength_;
			}

			public ParameterPoint<int> vitality()
			{
				return vitality_;
			}

			public ParameterPoint<int> dexterity()
			{
				return dexterity_;
			}

			public ParameterPoint<int> intelligence()
			{
				return intelligence_;
			}

			public ParameterPoint<int> mind()
			{
				return mind_;
			}

			public void setDefault()
			{
				strength_.set(0);
				vitality_.set(0);
				dexterity_.set(0);
				intelligence_.set(0);
				mind_.set(0);
			}

			public void copy(BodyParameter src)
			{
				strength_.set(src.strength_.get());
				vitality_.set(src.vitality_.get());
				dexterity_.set(src.dexterity_.get());
				intelligence_.set(src.intelligence_.get());
				mind_.set(src.mind_.get());
			}

			public void parse(ArrayReader reader)
			{
				strength_.set(reader.readByte());
				vitality_.set(reader.readByte());
				dexterity_.set(reader.readByte());
				intelligence_.set(reader.readByte());
				mind_.set(reader.readByte());
			}

			public void store(ArrayWriter writer)
			{
				writer.writeByte((byte)strength_.get());
				writer.writeByte((byte)vitality_.get());
				writer.writeByte((byte)dexterity_.get());
				writer.writeByte((byte)intelligence_.get());
				writer.writeByte((byte)mind_.get());
			}
		}

		public class Condition
		{
			private byte normalCondition_;

			private byte battleCondition_;

			private byte nearStoneCounter_;

			private byte sleepTime_;

			private byte paralysisTime_;

			private byte confusionTime_;

			public Condition()
			{
				normalCondition_ = 0;
				battleCondition_ = 0;
				clearNearStoneCounter();
				clearConditionTime();
			}

			public void clearCondition()
			{
				normalCondition_ = 0;
				battleCondition_ = 0;
				clearNearStoneCounter();
				clearConditionTime();
			}

			public bool isHealth()
			{
				if (normalCondition() > 0)
				{
					return false;
				}
				if (battleCondition() > 0)
				{
					return false;
				}
				return true;
			}

			public void clearBattleCondition()
			{
				battleCondition_ = 0;
				clearNearStoneCounter();
				clearConditionTime();
			}

			public bool isNotBattleCondition()
			{
				if (!isDeath() && !isStone())
				{
					return false;
				}
				return true;
			}

			public bool isCanCommandSelect()
			{
				if (isDeath())
				{
					return false;
				}
				if (isStone())
				{
					return false;
				}
				if (isParalysis())
				{
					return false;
				}
				if (isSleep())
				{
					return false;
				}
				if (isConfusion())
				{
					return false;
				}
				return true;
			}

			public bool isCanTargetSelect()
			{
				if (isDeath())
				{
					return false;
				}
				if (isStone())
				{
					return false;
				}
				if (isParalysis())
				{
					return false;
				}
				if (isSleep())
				{
					return false;
				}
				return true;
			}

			public bool isCanCover()
			{
				if (isDeath())
				{
					return false;
				}
				if (isStone())
				{
					return false;
				}
				if (isParalysis())
				{
					return false;
				}
				if (isSleep())
				{
					return false;
				}
				if (isConfusion())
				{
					return false;
				}
				if (isFrog())
				{
					return false;
				}
				return true;
			}

			public bool isCanAction()
			{
				if (isDeath())
				{
					return false;
				}
				if (isStone())
				{
					return false;
				}
				if (isParalysis())
				{
					return false;
				}
				if (isSleep())
				{
					return false;
				}
				return true;
			}

			public bool isPoisonMotion()
			{
				if (isSleep())
				{
					return true;
				}
				if (isSilence())
				{
					return true;
				}
				if (isDarkness())
				{
					return true;
				}
				if (isPoison())
				{
					return true;
				}
				if (isParalysis())
				{
					return true;
				}
				return false;
			}

			public bool isCounter()
			{
				if (isDeath())
				{
					return false;
				}
				if (isStone())
				{
					return false;
				}
				if (isParalysis())
				{
					return false;
				}
				if (isSleep())
				{
					return false;
				}
				if (isConfusion())
				{
					return false;
				}
				if (isFrog())
				{
					return false;
				}
				return true;
			}

			public bool isCanEscape()
			{
				if (isDeath())
				{
					return false;
				}
				if (isStone())
				{
					return false;
				}
				if (isParalysis())
				{
					return false;
				}
				if (isSleep())
				{
					return false;
				}
				if (isConfusion())
				{
					return false;
				}
				return true;
			}

			public bool isRecoverItemOrMagic()
			{
				if (isStone())
				{
					return true;
				}
				if (isParalysis())
				{
					return true;
				}
				if (isSleep())
				{
					return true;
				}
				if (isConfusion())
				{
					return true;
				}
				return false;
			}

			public bool isBreak()
			{
				if (isParalysis())
				{
					return false;
				}
				if (isSleep())
				{
					return false;
				}
				if (isConfusion())
				{
					return false;
				}
				if (isFrog())
				{
					return false;
				}
				if (isLilliput())
				{
					return false;
				}
				return true;
			}

			public void clearDamageCondition()
			{
				offSleep();
				offConfusion();
			}

			public void clearDeadCondition()
			{
				offSilence();
				offDarkness();
				offPoison();
				offNearDeath();
				offParalysis();
				offSleep();
				offConfusion();
				offNearStone();
				clearNearStoneCounter();
			}

			public void offParalysis()
			{
				battleCondition_ &= 254;
				paralysisTime_ = 0;
			}

			public void offSleep()
			{
				battleCondition_ &= 253;
				sleepTime_ = 0;
			}

			public void offConfusion()
			{
				battleCondition_ &= 251;
				confusionTime_ = 0;
			}

			public void goStone()
			{
				nearStoneCounter_++;
				if (nearStoneCounter_ == 3)
				{
					onStone();
					offNearStone();
					clearNearStoneCounter();
				}
			}

			public void clearConditionTime()
			{
				sleepTime_ = 0;
				paralysisTime_ = 0;
				confusionTime_ = 0;
			}

			public void calcConditionTime()
			{
				if (sleepTime_ > 0)
				{
					sleepTime_--;
					if (sleepTime_ == 0)
					{
						offSleep();
					}
				}
				if (paralysisTime_ > 0)
				{
					paralysisTime_--;
					if (paralysisTime_ == 0)
					{
						offParalysis();
					}
				}
				if (confusionTime_ > 0)
				{
					confusionTime_--;
					if (confusionTime_ == 0)
					{
						offConfusion();
					}
				}
			}

			public byte normalCondition()
			{
				return normalCondition_;
			}

			public void normalCondition_and(byte arg0)
			{
				normalCondition_ &= arg0;
			}

			public void clearNormalCondition()
			{
				normalCondition_ = 0;
			}

			public byte battleCondition()
			{
				return battleCondition_;
			}

			public bool isDeath()
			{
				if ((normalCondition_ & 1) == 0)
				{
					return false;
				}
				return true;
			}

			public void onDeath()
			{
				normalCondition_ |= 1;
			}

			public void offDeath()
			{
				normalCondition_ &= 254;
			}

			public bool isStone()
			{
				if ((normalCondition_ & 2) == 0)
				{
					return false;
				}
				return true;
			}

			public void onStone()
			{
				normalCondition_ |= 2;
			}

			public void offStone()
			{
				normalCondition_ &= 253;
			}

			public bool isFrog()
			{
				if ((normalCondition_ & 4) == 0)
				{
					return false;
				}
				return true;
			}

			public void onFrog()
			{
				normalCondition_ |= 4;
			}

			public void offFrog()
			{
				normalCondition_ &= 251;
			}

			public bool isSilence()
			{
				if ((normalCondition_ & 8) == 0)
				{
					return false;
				}
				return true;
			}

			public void onSilence()
			{
				normalCondition_ |= 8;
			}

			public void offSilence()
			{
				normalCondition_ &= 247;
			}

			public bool isLilliput()
			{
				if ((normalCondition_ & 0x10) == 0)
				{
					return false;
				}
				return true;
			}

			public void onLilliput()
			{
				normalCondition_ |= 16;
			}

			public void offLilliput()
			{
				normalCondition_ &= 239;
			}

			public bool isDarkness()
			{
				if ((normalCondition_ & 0x20) == 0)
				{
					return false;
				}
				return true;
			}

			public void onDarkness()
			{
				normalCondition_ |= 32;
			}

			public void offDarkness()
			{
				normalCondition_ &= 223;
			}

			public bool isPoison()
			{
				if ((normalCondition_ & 0x40) == 0)
				{
					return false;
				}
				return true;
			}

			public void onPoison()
			{
				normalCondition_ |= 64;
			}

			public void offPoison()
			{
				normalCondition_ &= 191;
			}

			public bool isNearDeath()
			{
				if ((normalCondition_ & 0x80) == 0)
				{
					return false;
				}
				return true;
			}

			public void onNearDeath()
			{
				normalCondition_ |= 128;
			}

			public void offNearDeath()
			{
				normalCondition_ &= 127;
			}

			public bool isParalysis()
			{
				if ((battleCondition_ & 1) == 0)
				{
					return false;
				}
				return true;
			}

			public void onParalysis()
			{
				battleCondition_ |= 1;
			}

			public bool isSleep()
			{
				if ((battleCondition_ & 2) == 0)
				{
					return false;
				}
				return true;
			}

			public void onSleep()
			{
				battleCondition_ |= 2;
			}

			public bool isConfusion()
			{
				if ((battleCondition_ & 4) == 0)
				{
					return false;
				}
				return true;
			}

			public void onConfusion()
			{
				battleCondition_ |= 4;
			}

			public bool isNearStone()
			{
				if ((battleCondition_ & 8) == 0)
				{
					return false;
				}
				return true;
			}

			public void onNearStone()
			{
				battleCondition_ |= 8;
			}

			public void offNearStone()
			{
				battleCondition_ &= 247;
			}

			public void clearNearStoneCounter()
			{
				nearStoneCounter_ = 0;
			}

			public byte sleepTime()
			{
				return sleepTime_;
			}

			public void setSleepTime(byte time)
			{
				sleepTime_ = time;
			}

			public byte paralysisTime()
			{
				return paralysisTime_;
			}

			public void setParalysisTime(byte time)
			{
				paralysisTime_ = time;
			}

			public byte confusionTime()
			{
				return confusionTime_;
			}

			public void setConfusionTime(byte time)
			{
				confusionTime_ = time;
			}

			public void setDefault()
			{
				normalCondition_ = 0;
				battleCondition_ = 0;
				nearStoneCounter_ = 0;
				sleepTime_ = 0;
				paralysisTime_ = 0;
				confusionTime_ = 0;
			}

			public void copy(Condition src)
			{
				normalCondition_ = src.normalCondition_;
				battleCondition_ = src.battleCondition_;
				nearStoneCounter_ = src.nearStoneCounter_;
				sleepTime_ = src.sleepTime_;
				paralysisTime_ = src.paralysisTime_;
				confusionTime_ = src.confusionTime_;
			}

			public void parse(ArrayReader reader)
			{
				normalCondition_ = reader.readByte();
				battleCondition_ = reader.readByte();
				nearStoneCounter_ = reader.readByte();
				sleepTime_ = reader.readByte();
				paralysisTime_ = reader.readByte();
				confusionTime_ = reader.readByte();
			}

			public void store(ArrayWriter writer)
			{
				writer.writeByte(normalCondition_);
				writer.writeByte(battleCondition_);
				writer.writeByte(nearStoneCounter_);
				writer.writeByte(sleepTime_);
				writer.writeByte(paralysisTime_);
				writer.writeByte(confusionTime_);
			}
		}

		public class Effects
		{
			public int frameCounter_;

			public short type_;

			public short category_;

			public short member_;

			public bool isLoop_;

			protected byte _pad0;

			public Effects()
			{
			}

			public Effects(int arg0, short arg1, short arg2, short arg3, bool arg4)
			{
				frameCounter_ = arg0;
				type_ = arg1;
				category_ = arg2;
				member_ = arg3;
				isLoop_ = arg4;
			}

			public void parse(ArrayReader reader)
			{
				frameCounter_ = reader.readInt32();
				type_ = reader.readInt16();
				category_ = reader.readInt16();
				member_ = reader.readInt16();
				isLoop_ = reader.readSByte() != 0;
				_pad0 = reader.readByte();
			}
		}

		public class MotionEffects
		{
			private int motionId_;

			private short target_;

			private short offset_;

			private Effects effects_;
		}

		public enum NORMAL_CONDITION
		{
			DEATH = 1,
			STONE = 2,
			FROG = 4,
			SILENCE = 8,
			LILLIPUT = 0x10,
			DARKNESS = 0x20,
			POISON = 0x40,
			NEARDEATH = 0x80
		}

		public enum BATTLE_CONDITION
		{
			PARALYSIS = 1,
			SLEEP = 2,
			CONFUSION = 4,
			NEARSTONE = 8,
			NONE = 0x40
		}

		public const byte MP_MAX = 99;

		public const byte MP_MIN = 0;

		private const short HP_MAX = 9999;

		private const short HP_MIN = 0;

		public const int M_HP_MAX = 999999;

		public const int M_HP_MIN = 0;

		private const byte BODY_PARAM_MAX = 99;

		private const byte BODY_PARAM_MIN = 1;

		private const byte BATTLE_PARAM_MAX = byte.MaxValue;

		private const byte BATTLE_PARAM_MIN = 0;

		private const byte NUMBER_MAX = 16;

		private const byte NUMBER_MIN = 1;

		private const byte PROBABILITY_MAX = 99;

		private const byte PROBABILITY_MIN = 0;

		public const int GOLD_MAX = 9999999;

		private const int GOLD_MIN = 0;

		private const int EXP_MAX = 9999999;

		private const int EXP_MIN = 0;

		private const byte NEARSTONE_COUNT_MAX = 3;

		public const NORMAL_CONDITION DEATH = NORMAL_CONDITION.DEATH;

		public const NORMAL_CONDITION STONE = NORMAL_CONDITION.STONE;

		public const NORMAL_CONDITION FROG = NORMAL_CONDITION.FROG;

		public const NORMAL_CONDITION SILENCE = NORMAL_CONDITION.SILENCE;

		public const NORMAL_CONDITION LILLIPUT = NORMAL_CONDITION.LILLIPUT;

		public const NORMAL_CONDITION DARKNESS = NORMAL_CONDITION.DARKNESS;

		public const NORMAL_CONDITION POISON = NORMAL_CONDITION.POISON;

		public const NORMAL_CONDITION NEARDEATH = NORMAL_CONDITION.NEARDEATH;

		public const BATTLE_CONDITION PARALYSIS = BATTLE_CONDITION.PARALYSIS;

		public const BATTLE_CONDITION SLEEP = BATTLE_CONDITION.SLEEP;

		public const BATTLE_CONDITION CONFUSION = BATTLE_CONDITION.CONFUSION;

		public const BATTLE_CONDITION NEARSTONE = BATTLE_CONDITION.NEARSTONE;

		public const BATTLE_CONDITION NONE = BATTLE_CONDITION.NONE;

		public static int TREASURE_BOX_MAX = 375;
	}
}
