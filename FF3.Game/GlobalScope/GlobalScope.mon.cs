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
	public static class mon
	{
		public class MonsterManager
		{
			public static MonsterManager instance_ = new MonsterManager();

			private Array chaindata_;

			private MonsterParameter[] monster_;

			private DropItemParameter[] item_;

			private MonsterNormalAttackParameter[] normalAttack_;

			private MonsterSpecialAttackParameter[] specialAttack_;

			private MonsterOffsetParameter[] offset_;

			private MonsterSpecialAttackEffects[] effectsInfo_;

			private int monsterMaxSize_;

			private int itemMaxSize_;

			private int normalAttackMaxSize_;

			private int specialAttackMaxSize_;

			private int offsetMaxSize_;

			private int effectsMaxSize_;

			private MonsterManiaManager monsterManiaManager_ = new MonsterManiaManager();

			public bool load()
			{
				free();
				uint size = ds.g_File.getSize("monster.chaindata");
				chaindata_ = ds.CHeap.alloc_app(size);
				ds.g_File.load(chaindata_, "monster.chaindata");
				monster_ = MonsterParameter.ChainPointer((byte[])chaindata_, 0);
				monsterMaxSize_ = (int)(pack.ChainPointerSize((byte[])chaindata_, 0u) / 100);
				item_ = DropItemParameter.ChainPointer((byte[])chaindata_, 1);
				itemMaxSize_ = (int)(pack.ChainPointerSize((byte[])chaindata_, 1u) / 18);
				normalAttack_ = MonsterNormalAttackParameter.ChainPointer((byte[])chaindata_, 2);
				normalAttackMaxSize_ = (int)(pack.ChainPointerSize((byte[])chaindata_, 2u) / 28);
				specialAttack_ = MonsterSpecialAttackParameter.ChainPointer((byte[])chaindata_, 3);
				specialAttackMaxSize_ = (int)(pack.ChainPointerSize((byte[])chaindata_, 3u) / 16);
				offset_ = MonsterOffsetParameter.ChainPointer((byte[])chaindata_, 4);
				offsetMaxSize_ = (int)(pack.ChainPointerSize((byte[])chaindata_, 4u) / 160);
				effectsInfo_ = MonsterSpecialAttackEffects.ChainPointer((byte[])chaindata_, 5);
				effectsMaxSize_ = (int)(pack.ChainPointerSize((byte[])chaindata_, 5u) / 56);
				setMonsterIdForMonsterManaia();
				return true;
			}

			public void free()
			{
				if (chaindata_ != null)
				{
					ds.CHeap.free_app(chaindata_);
					chaindata_ = null;
				}
			}

			// PORT: the engine API lists the monsters; the battle only ever looked one up.
			public int monsterCount()
			{
				return monster_ == null ? 0 : monsterMaxSize_;
			}

			public MonsterParameter monsterAt(int index)
			{
				return monster_ != null && index >= 0 && index < monsterMaxSize_ ? monster_[index] : null;
			}

			public bool isLoaded()
			{
				return monster_ != null;
			}

			public MonsterParameter monsterParameter(int monsterId)
			{
				for (int i = 0; i < monsterMaxSize_; i++)
				{
					if (monster_[i].monsterId() == monsterId)
					{
						return monster_[i];
					}
				}
				return null;
			}

			public DropItemParameter dropItem(int _id)
			{
				for (int i = 0; i < itemMaxSize_; i++)
				{
					if (item_[i].droppingItemTableId() == _id)
					{
						return item_[i];
					}
				}
				return null;
			}

			public MonsterNormalAttackParameter normalAttack(int _id)
			{
				for (int i = 0; i < normalAttackMaxSize_; i++)
				{
					if (i == _id)
					{
						return normalAttack_[i];
					}
				}
				return null;
			}

			public MonsterSpecialAttackParameter specialAttack(int _id)
			{
				for (int i = 0; i < specialAttackMaxSize_; i++)
				{
					if (specialAttack_[i].specialAttackId() == _id)
					{
						return specialAttack_[i];
					}
				}
				return null;
			}

			public MonsterOffsetParameter offset(int monsterId)
			{
				for (int i = 0; i < offsetMaxSize_; i++)
				{
					if (offset_[i].monsterId() == monsterId)
					{
						return offset_[i];
					}
				}
				return null;
			}

			public MonsterSpecialAttackEffects effectsInfo(int _id)
			{
				for (int i = 0; i < effectsMaxSize_; i++)
				{
					if (effectsInfo_[i].specialAttackId() == _id)
					{
						return effectsInfo_[i];
					}
				}
				return null;
			}

			public void sendMonsterManiaData(MonsterManiaManager data)
			{
				data.copy(monsterManiaManager_);
			}

			public void acceptMonsterManiaData(MonsterManiaManager data)
			{
				monsterManiaManager_.copy(data);
			}

			public void setMonsterIdForMonsterManaia()
			{
				for (int i = 0; i < monsterMaxSize_ && i <= MONSTER_MAX; i++)
				{
					monsterManiaManager_.monsterMania(i).setMonsterId(monster_[i].monsterId());
				}
			}

			public static MonsterManager instance()
			{
				return instance_;
			}

			public MonsterManiaManager monsterManiaManager()
			{
				return monsterManiaManager_;
			}
		}

		public class MonsterManiaManager
		{
			public static MonsterMania invalidData_ = new MonsterMania();

			private MonsterMania[] monsterMania_ = new MonsterMania[MONSTER_MAX];

			public MonsterManiaManager()
			{
				for (int i = 0; i < monsterMania_.Length; i++)
				{
					monsterMania_[i] = new MonsterMania();
				}
			}

			public MonsterMania monsterManiaForMonsterID(int _id)
			{
				for (int i = 0; i < MONSTER_MAX; i++)
				{
					if (monsterMania_[i].monsterId() == _id)
					{
						return monsterMania_[_id];
					}
				}
				return invalidData_;
			}

			public bool setMonsterManiaForMonsterID(MonsterMania data)
			{
				for (int i = 0; i < MONSTER_MAX; i++)
				{
					if (monsterMania_[i].monsterId() == data.monsterId())
					{
						monsterMania_[i] = data;
						return true;
					}
				}
				return false;
			}

			public void clearMonsterMania()
			{
				for (int i = 0; i < MONSTER_MAX; i++)
				{
					monsterMania_[i].clearEntryState((byte)(MonsterMania.NEW_ENTRY | MonsterMania.FINISH_ENTRY));
					monsterMania_[i].offChecked();
					monsterMania_[i].deadCount().set(0);
				}
			}

			public MonsterMania monsterMania(int i)
			{
				return monsterMania_[i];
			}

			public void setMonsterMania(MonsterMania data, int i)
			{
				monsterMania_[i] = data;
			}

			public void copy(MonsterManiaManager src)
			{
				for (int i = 0; i < MONSTER_MAX; i++)
				{
					monsterMania_[i].copy(src.monsterMania_[i]);
				}
			}

			public void parse(ArrayReader reader)
			{
				for (int i = 0; i < MONSTER_MAX; i++)
				{
					monsterMania_[i].parse(reader);
				}
			}

			public void store(ArrayWriter writer)
			{
				for (int i = 0; i < MONSTER_MAX; i++)
				{
					monsterMania_[i].store(writer);
				}
			}
		}

		public class MonsterParameter
		{
			private short nameId_;

			private short textId_;

			private short familyId_;

			private short modelId_;

			private short monsterId_;

			private byte level_;

			private byte size_;

			private int maxHp_;

			private ys.BodyParameter body_;

			private byte aiLevel_;

			private byte magicSkill_;

			private byte weight_;

			private short actionNumber_;

			private short devide_;

			private ys.PhysicsAttackParameter physicsAttack_;

			private ys.PhysicsDefenseParameter physicsDefense_;

			private ys.MagicDefenseParameter magicDefense_;

			private MonsterSpecialActionParameter[] specialAction_ = new MonsterSpecialActionParameter[MONSTER_SPECIAL_ACTION_MAX];

			private DroppingDataParameter droppingParameter_;

			private byte drawMapId_;

			protected byte _pad0;

			protected byte _pad1;

			protected byte _pad2;

			public bool isSpecial()
			{
				for (int i = 0; i < MONSTER_SPECIAL_ACTION_MAX; i++)
				{
					if (specialAction_[i].specialActionId() != 0)
					{
						return true;
					}
				}
				return false;
			}

			public short nameId()
			{
				return nameId_;
			}

			public short textId()
			{
				return textId_;
			}

			public short familyId()
			{
				return familyId_;
			}

			public short modelId()
			{
				return modelId_;
			}

			public short monsterId()
			{
				return monsterId_;
			}

			public byte level()
			{
				return level_;
			}

			public byte size()
			{
				return size_;
			}

			public int maxHp()
			{
				return maxHp_;
			}

			public ys.BodyParameter body()
			{
				return body_;
			}

			public byte aiLevel()
			{
				return aiLevel_;
			}

			public byte magicSkill()
			{
				return magicSkill_;
			}

			public byte weight()
			{
				return weight_;
			}

			public short actionNumber()
			{
				return actionNumber_;
			}

			public short devide()
			{
				return devide_;
			}

			public ys.PhysicsAttackParameter physicsAttack()
			{
				return physicsAttack_;
			}

			public ys.PhysicsDefenseParameter physicsDefense()
			{
				return physicsDefense_;
			}

			public ys.MagicDefenseParameter magicDefense()
			{
				return magicDefense_;
			}

			public MonsterSpecialActionParameter specialAction(int i)
			{
				return specialAction_[i];
			}

			public DroppingDataParameter droppingParameter()
			{
				return droppingParameter_;
			}

			public byte drawMapId()
			{
				return drawMapId_;
			}

			public static MonsterParameter[] ChainPointer(byte[] abyData, int iId)
			{
				ArrayReader arrayReader = new ArrayReader(abyData);
				arrayReader.skip(16L);
				arrayReader.skip(8 * iId);
				uint num = arrayReader.readUInt32();
				uint num2 = arrayReader.readUInt32();
				uint num3 = num2 / 100;
				arrayReader.setPosition(num);
				MonsterParameter[] array = new MonsterParameter[num3];
				for (int i = 0; i < num3; i++)
				{
					array[i] = new MonsterParameter();
					array[i].parse(arrayReader);
				}
				return array;
			}

			public void parse(ArrayReader reader)
			{
				body_ = new ys.BodyParameter();
				physicsAttack_ = new ys.PhysicsAttackParameter();
				physicsDefense_ = new ys.PhysicsDefenseParameter();
				magicDefense_ = new ys.MagicDefenseParameter();
				droppingParameter_ = new DroppingDataParameter();
				nameId_ = reader.readInt16();
				textId_ = reader.readInt16();
				familyId_ = reader.readInt16();
				modelId_ = reader.readInt16();
				monsterId_ = reader.readInt16();
				level_ = reader.readByte();
				size_ = reader.readByte();
				maxHp_ = reader.readInt32();
				body_.parse(reader);
				aiLevel_ = reader.readByte();
				magicSkill_ = reader.readByte();
				weight_ = reader.readByte();
				actionNumber_ = reader.readInt16();
				devide_ = reader.readInt16();
				physicsAttack_.parse(reader);
				physicsDefense_.parse(reader);
				magicDefense_.parse(reader);
				for (int i = 0; i < MONSTER_SPECIAL_ACTION_MAX; i++)
				{
					specialAction_[i] = new MonsterSpecialActionParameter();
					specialAction_[i].parse(reader);
				}
				droppingParameter_.parse(reader);
				drawMapId_ = reader.readByte();
				_pad0 = reader.readByte();
				_pad1 = reader.readByte();
				_pad2 = reader.readByte();
			}
		}

		public class MonsterOffsetParameter
		{
			private int monsterId_;

			private EffectOffset hitEffect_;

			private EffectOffset conditionEffect_;

			private VecFx32 cursorPosition_;

			private VecFx32 damagePosition_;

			private VecFx32 criticalPosition_;

			private VecFx32 initializePosition_;

			private VecFx32 touchPosition_;

			private int touchRadius_;

			private int height_;

			private int rotate_;

			private int scale_;

			private int shadowX_;

			private int shadowZ_;

			private VecFx32 startCameraPosition_;

			private VecFx32 startCameraTarget_;

			private VecFx32 finishCameraPosition_;

			private VecFx32 finishCameraTarget_;

			public bool isSettingCamera()
			{
				if (startCameraPosition_.x != 0 || startCameraPosition_.y != 0 || startCameraPosition_.z != 0)
				{
					return true;
				}
				if (startCameraTarget_.x != 0 || startCameraTarget_.y != 0 || startCameraTarget_.z != 0)
				{
					return true;
				}
				if (finishCameraPosition_.x != 0 || finishCameraPosition_.y != 0 || finishCameraPosition_.z != 0)
				{
					return true;
				}
				if (finishCameraTarget_.x != 0 || finishCameraTarget_.y != 0 || finishCameraTarget_.z != 0)
				{
					return true;
				}
				return false;
			}

			public int monsterId()
			{
				return monsterId_;
			}

			public EffectOffset hitEffect()
			{
				return hitEffect_;
			}

			public EffectOffset conditionEffect()
			{
				return conditionEffect_;
			}

			public VecFx32 cursorPosition()
			{
				return cursorPosition_;
			}

			public VecFx32 damagePosition()
			{
				return damagePosition_;
			}

			public VecFx32 criticalPosition()
			{
				return criticalPosition_;
			}

			public VecFx32 initializePosition()
			{
				return initializePosition_;
			}

			public VecFx32 touchPosition()
			{
				return touchPosition_;
			}

			public int touchRadius()
			{
				return touchRadius_;
			}

			public int height()
			{
				return height_;
			}

			public int rotate()
			{
				return rotate_;
			}

			public int scale()
			{
				return scale_;
			}

			public int shadowX()
			{
				return shadowX_;
			}

			public int shadowZ()
			{
				return shadowZ_;
			}

			public VecFx32 startCameraPosition()
			{
				return startCameraPosition_;
			}

			public VecFx32 startCameraTarget()
			{
				return startCameraTarget_;
			}

			public VecFx32 finishCameraPosition()
			{
				return finishCameraPosition_;
			}

			public VecFx32 finishCameraTarget()
			{
				return finishCameraTarget_;
			}

			public static MonsterOffsetParameter[] ChainPointer(byte[] abyData, int iId)
			{
				ArrayReader arrayReader = new ArrayReader(abyData);
				arrayReader.skip(16L);
				arrayReader.skip(8 * iId);
				uint num = arrayReader.readUInt32();
				uint num2 = arrayReader.readUInt32();
				uint num3 = num2 / 160;
				arrayReader.setPosition(num);
				MonsterOffsetParameter[] array = new MonsterOffsetParameter[num3];
				for (int i = 0; i < num3; i++)
				{
					array[i] = new MonsterOffsetParameter();
					array[i].parse(arrayReader);
				}
				return array;
			}

			public void parse(ArrayReader reader)
			{
				hitEffect_ = new EffectOffset();
				conditionEffect_ = new EffectOffset();
				cursorPosition_ = new VecFx32();
				damagePosition_ = new VecFx32();
				criticalPosition_ = new VecFx32();
				initializePosition_ = new VecFx32();
				touchPosition_ = new VecFx32();
				startCameraPosition_ = new VecFx32();
				startCameraTarget_ = new VecFx32();
				finishCameraPosition_ = new VecFx32();
				finishCameraTarget_ = new VecFx32();
				monsterId_ = reader.readInt32();
				hitEffect_.parse(reader);
				conditionEffect_.parse(reader);
				cursorPosition_.parse(reader);
				damagePosition_.parse(reader);
				criticalPosition_.parse(reader);
				initializePosition_.parse(reader);
				touchPosition_.parse(reader);
				touchRadius_ = reader.readInt32();
				height_ = reader.readInt32();
				rotate_ = reader.readInt32();
				scale_ = reader.readInt32();
				shadowX_ = reader.readInt32();
				shadowZ_ = reader.readInt32();
				startCameraPosition_.parse(reader);
				startCameraTarget_.parse(reader);
				finishCameraPosition_.parse(reader);
				finishCameraTarget_.parse(reader);
			}
		}

		public class MonsterSpecialActionParameter
		{
			private short specialActionId_;

			private short specialActionProbability_;

			private int actStartHP_;

			public short specialActionId()
			{
				return specialActionId_;
			}

			public short specialActionProbability()
			{
				return specialActionProbability_;
			}

			public int actStartHP()
			{
				return actStartHP_;
			}

			public void parse(ArrayReader reader)
			{
				specialActionId_ = reader.readInt16();
				specialActionProbability_ = reader.readInt16();
				actStartHP_ = reader.readInt32();
			}
		}

		public class DroppingDataParameter
		{
			private short droppingItemProbability_;

			private short droppingItemTableId_;

			private int gold_;

			private int exp_;

			public short droppingItemProbability()
			{
				return droppingItemProbability_;
			}

			public short droppingItemTableId()
			{
				return droppingItemTableId_;
			}

			public int gold()
			{
				return gold_;
			}

			public int exp()
			{
				return exp_;
			}

			public void parse(ArrayReader reader)
			{
				droppingItemProbability_ = reader.readInt16();
				droppingItemTableId_ = reader.readInt16();
				gold_ = reader.readInt32();
				exp_ = reader.readInt32();
			}
		}

		public class DropItemParameter
		{
			private short droppingItemTableId_;

			private short[] normalItem_ = new short[8];

			public short droppingItemTableId()
			{
				return droppingItemTableId_;
			}

			public short itemId(int i)
			{
				return normalItem_[i];
			}

			public static DropItemParameter[] ChainPointer(byte[] abyData, int iId)
			{
				ArrayReader arrayReader = new ArrayReader(abyData);
				arrayReader.skip(16L);
				arrayReader.skip(8 * iId);
				uint num = arrayReader.readUInt32();
				uint num2 = arrayReader.readUInt32();
				uint num3 = num2 / 18;
				arrayReader.setPosition(num);
				DropItemParameter[] array = new DropItemParameter[num3];
				for (int i = 0; i < num3; i++)
				{
					array[i] = new DropItemParameter();
					array[i].parse(arrayReader);
				}
				return array;
			}

			public void parse(ArrayReader reader)
			{
				droppingItemTableId_ = reader.readInt16();
				reader.read(normalItem_, 0, 8);
			}
		}

		public class MonsterNormalAttackParameter
		{
			private ys.Effects[] effects_ = new ys.Effects[EFFECTS_MAX];

			private short damageMotion_;

			private short damageValue_;

			public ys.Effects effects(int i)
			{
				return effects_[i];
			}

			public short damageMotion()
			{
				return damageMotion_;
			}

			public short damageValue()
			{
				return damageValue_;
			}

			public static MonsterNormalAttackParameter[] ChainPointer(byte[] abyData, int iId)
			{
				ArrayReader arrayReader = new ArrayReader(abyData);
				arrayReader.skip(16L);
				arrayReader.skip(8 * iId);
				uint num = arrayReader.readUInt32();
				uint num2 = arrayReader.readUInt32();
				uint num3 = num2 / 28;
				arrayReader.setPosition(num);
				MonsterNormalAttackParameter[] array = new MonsterNormalAttackParameter[num3];
				for (int i = 0; i < num3; i++)
				{
					array[i] = new MonsterNormalAttackParameter();
					array[i].parse(arrayReader);
				}
				return array;
			}

			public void parse(ArrayReader reader)
			{
				for (int i = 0; i < EFFECTS_MAX; i++)
				{
					effects_[i] = new ys.Effects();
					effects_[i].parse(reader);
				}
				damageMotion_ = reader.readInt16();
				damageValue_ = reader.readInt16();
			}
		}

		public class MonsterSpecialAttackParameter
		{
			public const int PARAM_MAX = 6;

			public const int NORMAL = 0;

			public const int TABLE = 1;

			public const int STARE = 2;

			public const int SAME_MAGIC = 3;

			public const int AUGMENT = 4;

			public const int SUMMON = 5;

			private short specialAttackId_;

			private short command_;

			private short[] param_ = new short[6];

			public short specialAttackId()
			{
				return specialAttackId_;
			}

			public short command()
			{
				return command_;
			}

			public short param(int i)
			{
				return param_[i];
			}

			public static MonsterSpecialAttackParameter[] ChainPointer(byte[] abyData, int iId)
			{
				ArrayReader arrayReader = new ArrayReader(abyData);
				arrayReader.skip(16L);
				arrayReader.skip(8 * iId);
				uint num = arrayReader.readUInt32();
				uint num2 = arrayReader.readUInt32();
				uint num3 = num2 / 16;
				arrayReader.setPosition(num);
				MonsterSpecialAttackParameter[] array = new MonsterSpecialAttackParameter[num3];
				for (int i = 0; i < num3; i++)
				{
					array[i] = new MonsterSpecialAttackParameter();
					array[i].parse(arrayReader);
				}
				return array;
			}

			public void parse(ArrayReader reader)
			{
				specialAttackId_ = reader.readInt16();
				command_ = reader.readInt16();
				reader.read(param_, 0, 6);
			}
		}

		public class TimingInfo
		{
			public short motionIndex_;

			public short frame_;

			public void parse(ArrayReader reader)
			{
				motionIndex_ = reader.readInt16();
				frame_ = reader.readInt16();
			}
		}

		public class EffectInfo
		{
			public TimingInfo timingInfo_;

			public short category_;

			public short member_;

			public byte loop_;

			public byte trace_;

			public byte target_;

			public byte positionType_;

			public int playFrame_;

			public void parse(ArrayReader reader)
			{
				timingInfo_ = new TimingInfo();
				timingInfo_.parse(reader);
				category_ = reader.readInt16();
				member_ = reader.readInt16();
				loop_ = reader.readByte();
				trace_ = reader.readByte();
				target_ = reader.readByte();
				positionType_ = reader.readByte();
				playFrame_ = reader.readInt32();
			}
		}

		public class SeInfo
		{
			public TimingInfo timingInfo_;

			public short category_;

			public short member_;

			public void parse(ArrayReader reader)
			{
				timingInfo_ = new TimingInfo();
				timingInfo_.parse(reader);
				category_ = reader.readInt16();
				member_ = reader.readInt16();
			}
		}

		public class MonsterSpecialAttackEffects
		{
			private short specialAttackId_;

			private short changeMotionIndex_;

			private EffectInfo[] effectInfo_ = new EffectInfo[EFFECTS_MAX];

			private SeInfo[] seInfo_ = new SeInfo[EFFECTS_MAX];

			private TimingInfo damageTimingInfo_;

			public short specialAttackId()
			{
				return specialAttackId_;
			}

			public short changeMotionIndex()
			{
				return changeMotionIndex_;
			}

			public EffectInfo effectInfo(int i)
			{
				return effectInfo_[i];
			}

			public SeInfo seInfo(int i)
			{
				return seInfo_[i];
			}

			public TimingInfo damageTimingInfo()
			{
				return damageTimingInfo_;
			}

			public static MonsterSpecialAttackEffects[] ChainPointer(byte[] abyData, int iId)
			{
				ArrayReader arrayReader = new ArrayReader(abyData);
				arrayReader.skip(16L);
				arrayReader.skip(8 * iId);
				uint num = arrayReader.readUInt32();
				uint num2 = arrayReader.readUInt32();
				uint num3 = num2 / 56;
				arrayReader.setPosition(num);
				MonsterSpecialAttackEffects[] array = new MonsterSpecialAttackEffects[num3];
				for (int i = 0; i < num3; i++)
				{
					array[i] = new MonsterSpecialAttackEffects();
					array[i].parse(arrayReader);
				}
				return array;
			}

			public void parse(ArrayReader reader)
			{
				damageTimingInfo_ = new TimingInfo();
				specialAttackId_ = reader.readInt16();
				changeMotionIndex_ = reader.readInt16();
				for (int i = 0; i < EFFECTS_MAX; i++)
				{
					effectInfo_[i] = new EffectInfo();
					effectInfo_[i].parse(reader);
				}
				for (int i = 0; i < EFFECTS_MAX; i++)
				{
					seInfo_[i] = new SeInfo();
					seInfo_[i].parse(reader);
				}
				damageTimingInfo_.parse(reader);
			}
		}

		public class MonsterMania
		{
			public static byte NO_ENTRY = 0;

			public static byte NEW_ENTRY = 1;

			public static byte FINISH_ENTRY = 2;

			private short monsterId_;

			private byte entryState_;

			private byte checked_;

			private ys.ParameterPoint<int> deadCount_ = new ys.ParameterPoint<int>(0, 9999);

			public short monsterId()
			{
				return monsterId_;
			}

			public void setMonsterId(short _id)
			{
				monsterId_ = _id;
			}

			public byte entryState()
			{
				return entryState_;
			}

			public bool isEntryState(byte flag)
			{
				if ((entryState_ & flag) == 0)
				{
					return false;
				}
				return true;
			}

			public void setEntryState(byte flag)
			{
				entryState_ |= flag;
			}

			public void clearEntryState(byte flag)
			{
				entryState_ &= (byte)(~flag);
			}

			public byte @checked()
			{
				return checked_;
			}

			public void onChecked()
			{
				checked_ = 1;
			}

			public void offChecked()
			{
				checked_ = 0;
			}

			public ys.ParameterPoint<int> deadCount()
			{
				return deadCount_;
			}

			public void copy(MonsterMania src)
			{
				monsterId_ = src.monsterId_;
				entryState_ = src.entryState_;
				checked_ = src.checked_;
				deadCount_.set(src.deadCount_.get());
			}

			public void parse(ArrayReader reader)
			{
				monsterId_ = reader.readInt16();
				entryState_ = reader.readByte();
				checked_ = reader.readByte();
				deadCount_.set(reader.readInt32());
			}

			public void store(ArrayWriter writer)
			{
				writer.writeInt16(monsterId_);
				writer.writeByte(entryState_);
				writer.writeByte(checked_);
				writer.writeInt32(deadCount_.get());
			}
		}

		public class EffectOffset
		{
			public int cameraDistance_;

			public int offsetY_;

			public int bone_;

			public void parse(ArrayReader reader)
			{
				cameraDistance_ = reader.readInt32();
				offsetY_ = reader.readInt32();
				bone_ = reader.readInt32();
			}
		}

		public class MonsterPartyManager
		{
			public static MonsterPartyManager instance_ = new MonsterPartyManager();

			private MonsterParty[] monsterParty_;

			public bool load()
			{
				free();
				bool flag = false;
				string arg;
				if (btl.OutsideToBattle.getInstance().battleType() == btl.BATTLE_TYPE.NORMAL_BATTLE)
				{
					strcpy(out arg, "monster_party_table.bbd");
				}
				else
				{
					strcpy(out arg, "event_monster_party_table.bbd");
				}
				Array array = null;
				uint size = ds.g_File.getSize(arg);
				array = ds.CHeap.alloc_app(size);
				flag = ds.g_File.load(array, arg);
				monsterParty_ = MonsterParty.castArray(array);
				_ = size / 18;
				return flag;
			}

			public void free()
			{
				if (monsterParty_ != null)
				{
					ds.CHeap.free_app(monsterParty_);
					monsterParty_ = null;
				}
			}

			public MonsterParty monsterParty(int _id)
			{
				for (int i = 0; i < MONSTER_PARTY_MAX; i++)
				{
					if (_id == monsterParty_[i].monsterPartyId())
					{
						return monsterParty_[i];
					}
				}
				return monsterParty_[1];
			}

			// PORT: for the engine API, outside a battle: the normal encounter table by name, and
			// a lookup that says "unknown" instead of handing back party 1.
			public bool loadNormalTable()
			{
				free();
				uint size = ds.g_File.getSize("monster_party_table.bbd");
				if (size == 0)
				{
					return false;
				}
				Array array = ds.CHeap.alloc_app(size);
				bool flag = ds.g_File.load(array, "monster_party_table.bbd");
				monsterParty_ = MonsterParty.castArray(array);
				return flag;
			}

			public bool isLoaded()
			{
				return monsterParty_ != null;
			}

			public MonsterParty findMonsterParty(int _id)
			{
				if (monsterParty_ == null)
				{
					return null;
				}
				for (int i = 0; i < MONSTER_PARTY_MAX && i < monsterParty_.Length; i++)
				{
					if (monsterParty_[i] != null && _id == monsterParty_[i].monsterPartyId())
					{
						return monsterParty_[i];
					}
				}
				return null;
			}

			public static MonsterPartyManager instance()
			{
				return instance_;
			}
		}

		public class Monsters
		{
			private short monsterId_;

			private byte min_;

			private byte max_;

			public short monsterId()
			{
				return monsterId_;
			}

			public byte min()
			{
				return min_;
			}

			public byte max()
			{
				return max_;
			}

			public static explicit operator Monsters(ArrayReader src)
			{
				Monsters monsters = new Monsters();
				monsters.monsterId_ = src.readInt16();
				monsters.min_ = src.readByte();
				monsters.max_ = src.readByte();
				return monsters;
			}
		}

		public class MonsterParty
		{
			private short monsterPartyId_;

			private Monsters[] monsters_ = new Monsters[MONSTERS_MAX];

			public short monsterPartyId()
			{
				return monsterPartyId_;
			}

			public Monsters monsters(int i)
			{
				return monsters_[i];
			}

			public static MonsterParty[] castArray(Array src)
			{
				MonsterParty[] array = new MonsterParty[src.Length / 18];
				ArrayReader arrayReader = new ArrayReader(src);
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = (MonsterParty)arrayReader;
				}
				arrayReader.dispose();
				return array;
			}

			public static explicit operator MonsterParty(ArrayReader src)
			{
				MonsterParty monsterParty = new MonsterParty();
				monsterParty.monsterPartyId_ = src.readInt16();
				for (int i = 0; i < MONSTERS_MAX; i++)
				{
					monsterParty.monsters_[i] = (Monsters)src;
				}
				return monsterParty;
			}
		}

		public enum MONSTER_CHAINDATA_INDEX
		{
			INDEX_MONSTER_PARAMETER,
			INDEX_DROP_ITEM_TABLE,
			INDEX_MONSTER_NORMAL_ATTACK,
			INDEX_MONSTER_SPECIAL_ATTACK,
			INDEX_MONSTER_OFFSET,
			INDEX_MONSTER_EFFECTS_INFO,
			MONSTER_CHAINDATA_INDEX_MAX
		}

		public enum EFFECTS_TYPE
		{
			TYPE_EFFECT,
			TYPE_SE,
			EFFECTS_TYPE_MAX
		}

		public enum MONSTER_SIZE
		{
			S_SIZE,
			M_SIZE,
			L_SIZE,
			MONSTER_SIZE_MAX
		}

		public const int NORMAL_DROP_ITEM_MAX = 4;

		public const int BETTER_DROP_ITEM_MAX = 2;

		public const int RARE_DROP_ITEM_MAX = 1;

		public const int VERYRARE_DROP_ITEM_MAX = 1;

		public const int DROP_ITEM_MAX = 8;

		public const MONSTER_CHAINDATA_INDEX INDEX_MONSTER_PARAMETER = MONSTER_CHAINDATA_INDEX.INDEX_MONSTER_PARAMETER;

		public const MONSTER_CHAINDATA_INDEX INDEX_DROP_ITEM_TABLE = MONSTER_CHAINDATA_INDEX.INDEX_DROP_ITEM_TABLE;

		public const MONSTER_CHAINDATA_INDEX INDEX_MONSTER_NORMAL_ATTACK = MONSTER_CHAINDATA_INDEX.INDEX_MONSTER_NORMAL_ATTACK;

		public const MONSTER_CHAINDATA_INDEX INDEX_MONSTER_SPECIAL_ATTACK = MONSTER_CHAINDATA_INDEX.INDEX_MONSTER_SPECIAL_ATTACK;

		public const MONSTER_CHAINDATA_INDEX INDEX_MONSTER_OFFSET = MONSTER_CHAINDATA_INDEX.INDEX_MONSTER_OFFSET;

		public const MONSTER_CHAINDATA_INDEX INDEX_MONSTER_EFFECTS_INFO = MONSTER_CHAINDATA_INDEX.INDEX_MONSTER_EFFECTS_INFO;

		public const MONSTER_CHAINDATA_INDEX MONSTER_CHAINDATA_INDEX_MAX = MONSTER_CHAINDATA_INDEX.MONSTER_CHAINDATA_INDEX_MAX;

		public const EFFECTS_TYPE TYPE_EFFECT = EFFECTS_TYPE.TYPE_EFFECT;

		public const EFFECTS_TYPE TYPE_SE = EFFECTS_TYPE.TYPE_SE;

		public const EFFECTS_TYPE EFFECTS_TYPE_MAX = EFFECTS_TYPE.EFFECTS_TYPE_MAX;

		public const MONSTER_SIZE S_SIZE = MONSTER_SIZE.S_SIZE;

		public const MONSTER_SIZE M_SIZE = MONSTER_SIZE.M_SIZE;

		public const MONSTER_SIZE L_SIZE = MONSTER_SIZE.L_SIZE;

		public const MONSTER_SIZE MONSTER_SIZE_MAX = MONSTER_SIZE.MONSTER_SIZE_MAX;

		public static int MONSTER_SPECIAL_ACTION_MAX = 2;

		public static int MONSTER_MAX = 256;

		public static int MONSTERS_MAX = 4;

		public static int MONSTER_PARTY_MAX = 259;

		public static int DROP_ITEM_TABLE_MAX = 32;

		public static int EFFECTS_MAX = 2;

		public static int SPECIAL_EFFECTS_MAX = 4;
	}
}
