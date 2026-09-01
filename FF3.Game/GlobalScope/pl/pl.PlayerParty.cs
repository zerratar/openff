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
		public class PlayerParty
		{
			public class SaveBattleCommand
			{
				public int saveCommand_;

				public int saveCommandPosition_;

				public int saveStartCommand_;
			}

			public const int ERR_ID = -1;

			public static PlayerParty instance_ = new PlayerParty();

			public static Player InvalidPlayer = new Player();

			private Player[] player_ = new Player[4];

			private npc.NpcManager npc_ = new npc.NpcManager();

			private itm.PossessionItemManager item_ = new itm.PossessionItemManager();

			private itm.StoredItemManager storedItem_ = new itm.StoredItemManager();

			private ys.ParameterPoint<int> gold_ = new ys.ParameterPoint<int>(0, 9999999);

			private uint playTime_;

			private Mania mania_ = new Mania();

			private Array chaindata_;

			private PlayerExp[] exp_;

			private GrowUp[] growUp_;

			private JobGrowUpType[] jobGrowUpType_;

			private PlayerNormalAttackParameter[] normalAttack_;

			private PlayerNormalMagicParameter[] normalMagic_;

			private GrowUpMp[][] mp_ = new GrowUpMp[GROW_UP_MP_TYPE_MAX][];

			private JobEquipInfo[] jobEquipInfo_;

			private AbilityParameter[] abilityParameter_;

			private PlayerAbility[] initializeJobAbility_;

			private int normalMagicMaxSize_;

			private SaveBattleCommand[] saveBattleCommand_ = new SaveBattleCommand[4];

			public PlayerParty()
			{
				for (int i = 0; i < player_.Length; i++)
				{
					player_[i] = new Player();
				}
				for (int i = 0; i < saveBattleCommand_.Length; i++)
				{
					saveBattleCommand_[i] = new SaveBattleCommand();
				}
			}

			public void initialize()
			{
				for (byte b = 0; b < 4; b++)
				{
					player(b).initialize(b);
					player(b).updateParameter();
				}
				item_.initialize();
				storedItem_.initialize();
				npc_.initialize();
				gold_.min();
				playTime_ = 0u;
				mania_.initialize();
				clearBattleCommand();
			}

			public bool load()
			{
				free();
				uint size = ds.g_File.getSize("player.chaindata");
				chaindata_ = ds.CHeap.alloc_app(size);
				ds.g_File.load(chaindata_, "player.chaindata");
				exp_ = PlayerExp.ChainPointer((byte[])chaindata_, 0);
				growUp_ = GrowUp.ChainPointer((byte[])chaindata_, 2);
				jobGrowUpType_ = JobGrowUpType.ChainPointer((byte[])chaindata_, 1);
				normalAttack_ = PlayerNormalAttackParameter.ChainPointer((byte[])chaindata_, 3);
				mp_[0] = GrowUpMp.ChainPointer((byte[])chaindata_, 4);
				mp_[1] = GrowUpMp.ChainPointer((byte[])chaindata_, 5);
				mp_[2] = GrowUpMp.ChainPointer((byte[])chaindata_, 6);
				mp_[3] = GrowUpMp.ChainPointer((byte[])chaindata_, 7);
				mp_[4] = GrowUpMp.ChainPointer((byte[])chaindata_, 8);
				mp_[5] = GrowUpMp.ChainPointer((byte[])chaindata_, 9);
				mp_[6] = GrowUpMp.ChainPointer((byte[])chaindata_, 10);
				jobEquipInfo_ = JobEquipInfo.ChainPointer((byte[])chaindata_, 11);
				normalMagic_ = PlayerNormalMagicParameter.ChainPointer((byte[])chaindata_, 12);
				abilityParameter_ = AbilityParameter.ChainPointer((byte[])chaindata_, 13);
				initializeJobAbility_ = PlayerAbility.ChainPointer((byte[])chaindata_, 14);
				normalMagicMaxSize_ = (int)(pack.ChainPointerSize((byte[])chaindata_, 12u) / 32);
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

			public Player playerForId(byte playerId)
			{
				for (byte b = 0; b < 4; b++)
				{
					if (player(b).playerId() == playerId)
					{
						return player(b);
					}
				}
				return InvalidPlayer;
			}

			public void setPlayer(byte playerId, Player player)
			{
				int num = playerOrder(playerId);
				if (num != -1)
				{
					player_[num].copy(player);
				}
			}

			public void setPlayerForOrder(Player player, int orderId)
			{
				player_[orderId].copy(player);
			}

			public PlayerNormalAttackParameter normalAttack(int motionId)
			{
				for (int i = 0; i < 55; i++)
				{
					if (motionId == normalAttack_[i].motionId())
					{
						return normalAttack_[i];
					}
				}
				return null;
			}

			public PlayerNormalMagicParameter normalMagic(int magicId)
			{
				for (int i = 0; i < normalMagicMaxSize_; i++)
				{
					if (magicId == normalMagic_[i].magicId())
					{
						return normalMagic_[i];
					}
				}
				return null;
			}

			public AbilityParameter abilityList(int _id)
			{
				for (int i = 0; i < 50; i++)
				{
					if (_id == abilityParameter_[i].id_)
					{
						return abilityParameter_[i];
					}
				}
				return null;
			}

			public void clearBattleCommand()
			{
				for (int i = 0; i < 4; i++)
				{
					saveBattleCommand_[i].saveCommand_ = 0;
					saveBattleCommand_[i].saveCommandPosition_ = 0;
					saveBattleCommand_[i].saveStartCommand_ = 0;
				}
			}

			public void clearBattleCommandPlayer(int _id)
			{
				if (_id >= 0 && _id <= 3)
				{
					saveBattleCommand_[_id].saveCommand_ = 0;
					saveBattleCommand_[_id].saveCommandPosition_ = 0;
					saveBattleCommand_[_id].saveStartCommand_ = 0;
				}
			}

			public bool isEnablePlayer()
			{
				for (byte b = 0; b < 4; b++)
				{
					if (player(b).isEnable())
					{
						return true;
					}
				}
				return false;
			}

			public bool isPartyFull()
			{
				for (byte b = 0; b < 4; b++)
				{
					if (!player(b).isEnable())
					{
						return false;
					}
				}
				return true;
			}

			public void fineAll()
			{
				for (byte b = 0; b < 4; b++)
				{
					player(b).fine();
				}
			}

			public sbyte averageLevel()
			{
				int num = 0;
				int num2 = 0;
				for (byte b = 0; b < 4; b++)
				{
					if (player(b).isEnable())
					{
						num += player(b).level().get();
						num2++;
					}
				}
				sbyte b2 = 0;
				if (num2 == 0)
				{
					return 0;
				}
				return (sbyte)(num / num2);
			}

			public int playerOrder(byte playerId)
			{
				for (byte b = 0; b < 4; b++)
				{
					if (player(b).playerId() == playerId)
					{
						return b;
					}
				}
				return -1;
			}

			public void changePlayer(byte playerId1, byte playerId2)
			{
				Player player = new Player(playerForId(playerId1));
				Player player2 = new Player(playerForId(playerId2));
				byte orderId = (byte)playerOrder(player.playerId());
				byte orderId2 = (byte)playerOrder(player2.playerId());
				setPlayerForOrder(player, orderId2);
				setPlayerForOrder(player2, orderId);
			}

			public bool addPlayer(byte playerId)
			{
				if (isPartyFull())
				{
					return false;
				}
				if (playerForId(playerId).isEnable())
				{
					return false;
				}
				for (byte b = 0; b < 4; b++)
				{
					if (!player(b).isEnable())
					{
						byte playerId2 = player(b).playerId();
						changePlayer(playerId, playerId2);
						playerForId(playerId).onIsEnable();
						return true;
					}
				}
				for (byte b2 = 0; b2 < 4; b2++)
				{
				}
				return false;
			}

			public bool releasePlayer(byte playerId)
			{
				if (!playerForId(playerId).isEnable())
				{
					return false;
				}
				playerForId(playerId).offIsEnable();
				return true;
			}

			public int aliveNumber()
			{
				int num = 0;
				for (int i = 0; i < 4; i++)
				{
					if (player((byte)i).isHealing())
					{
						num++;
					}
				}
				return num;
			}

			public void setPlayerLevelPartyAverage(byte playerId)
			{
				playerForId(playerId).growParameter((byte)averageLevel());
			}

			public void clearBattleCondition()
			{
				for (int i = 0; i < 4; i++)
				{
					player((byte)i).condition().clearBattleCondition();
				}
			}

			public bool isFrogAll()
			{
				for (int i = 0; i < 4; i++)
				{
					if (player((byte)i).isEnable() && !player((byte)i).condition().isFrog())
					{
						return false;
					}
				}
				return true;
			}

			public bool isLilliputAll()
			{
				for (int i = 0; i < 4; i++)
				{
					if (player((byte)i).isEnable() && !player((byte)i).condition().isLilliput())
					{
						return false;
					}
				}
				return true;
			}

			public int partyMemberEnableNumber()
			{
				int num = 0;
				for (int i = 0; i < 4; i++)
				{
					if (player((byte)i).isEnable())
					{
						num++;
					}
				}
				return num;
			}

			public void clearMemory()
			{
				for (int i = 0; i < 4; i++)
				{
					player_[i].jobManager().command().initializeAfterLoad();
				}
				clearBattleCommand();
				for (int j = 0; j < 4; j++)
				{
					menu.MenuManager.getSingleton().saveBattlePitchLine_set(j, 0);
					menu.MenuManager.getSingleton().saveBattlePitchTarget_set(j, 0);
					menu.MenuManager.getSingleton().saveBattleEquipLine_set(j, 0);
					menu.MenuManager.getSingleton().saveBattleEquipTarget_set(j, 0);
					menu.MenuManager.getSingleton().saveBattleEquipHand_set(j, 0);
					menu.MenuManager.getSingleton().saveBattleItemLine_set(j, 0);
					menu.MenuManager.getSingleton().saveBattleItemTarget_set(j, 0);
					menu.MenuManager.getSingleton().saveBattleMagicLine_set(j, 0);
					menu.MenuManager.getSingleton().saveBattleMagicTarget_set(j, 0);
					menu.MenuManager.getSingleton().saveBattleUseItem_set(j, 0);
					menu.MenuManager.getSingleton().saveBattleUseItemHand_set(j, 0);
				}
			}

			public bool addNpc(byte npcId)
			{
				if (npc().isEnable())
				{
					return false;
				}
				if (!npc().isEnable())
				{
					npc().npcId_set(npcId);
					npc().onIsEnable();
					return true;
				}
				return false;
			}

			public bool releaseNpc(byte npcId)
			{
				if (!npc().isEnable())
				{
					return false;
				}
				npc().offIsEnable();
				return true;
			}

			public void addItem(int itemid, int ItemNum)
			{
				if (itemid == 1000 || itm.ItemManager.instance().itemParameter((short)itemid) == null)
				{
					return;
				}
				if (itm.ItemManager.instance().itemCategory((short)itemid) == itm.CATEGORY.CATEGORY_MAGIC)
				{
					itm.ItemBaseParameter itemBaseParameter = itm.ItemManager.instance().itemParameter((short)itemid);
					if (itemBaseParameter != null && (itemBaseParameter.system() == 3 || itemBaseParameter.system() == 4 || itemBaseParameter.system() == 5 || (itemBaseParameter.system() == 2 && itemid / 10 % 10 != 0)))
					{
						return;
					}
				}
				if (itm.ItemManager.instance().itemCategory((short)itemid) == itm.CATEGORY.CATEGORY_IMPORTANT)
				{
					int i;
					for (i = 0; i < 64 && item().importantItem(i).itemId() != itemid; i++)
					{
						if (ItemNum > 0 && item().importantItem(i).itemId() <= 0)
						{
							item().importantItem(i).setItemId((short)itemid);
							break;
						}
					}
					if (i < 64)
					{
						int num = item().importantItem(i).itemNumber();
						num += ItemNum;
						if (num > 99)
						{
							num = 99;
						}
						if (num < 0)
						{
							num = 0;
						}
						item().importantItem(i).setItemNumber(num);
						item().resetImportantItemId();
					}
				}
				else
				{
					item().storeItem((short)itemid, ItemNum);
					item().resetItemId();
				}
			}

			public void sendSaveData(PlayerSaveData saveData)
			{
				for (int i = 0; i < 4; i++)
				{
					saveData.player_[i].copy(player_[i]);
				}
				saveData.npc_.copy(npc_);
				saveData.item_.copy(item_);
				saveData.storedItem_.copy(storedItem_);
				saveData.gold_.set(gold_.get());
				saveData.playTime_ = ds.GlobalPlayTimeCounter.getSingleton().get();
				saveData.mania_.copy(mania_);
			}

			public void acceptSaveData(PlayerSaveData saveData)
			{
				for (int i = 0; i < 4; i++)
				{
					player_[i].copy(saveData.player_[i]);
				}
				npc_.copy(saveData.npc_);
				item_.copy(saveData.item_);
				storedItem_.copy(saveData.storedItem_);
				gold_.set(saveData.gold_.get());
				playTime_ = saveData.playTime_;
				mania_.copy(saveData.mania_);
				for (int j = 0; j < 4; j++)
				{
					player_[j].jobManager().command().initializeAfterLoad();
				}
				clearBattleCommand();
				for (int k = 0; k < 4; k++)
				{
					menu.MenuManager.getSingleton().saveBattlePitchLine_set(k, 0);
					menu.MenuManager.getSingleton().saveBattlePitchTarget_set(k, 0);
					menu.MenuManager.getSingleton().saveBattleEquipLine_set(k, 0);
					menu.MenuManager.getSingleton().saveBattleEquipTarget_set(k, 0);
					menu.MenuManager.getSingleton().saveBattleEquipHand_set(k, 0);
					menu.MenuManager.getSingleton().saveBattleItemLine_set(k, 0);
					menu.MenuManager.getSingleton().saveBattleItemTarget_set(k, 0);
					menu.MenuManager.getSingleton().saveBattleMagicLine_set(k, 0);
					menu.MenuManager.getSingleton().saveBattleMagicTarget_set(k, 0);
					menu.MenuManager.getSingleton().saveBattleUseItem_set(k, 0);
					menu.MenuManager.getSingleton().saveBattleUseItemHand_set(k, 0);
				}
			}

			public static PlayerParty instance()
			{
				return instance_;
			}

			public Player player(byte i)
			{
				return player_[i];
			}

			public npc.NpcManager npc()
			{
				return npc_;
			}

			public itm.PossessionItemManager item()
			{
				return item_;
			}

			public itm.StoredItemManager storedItem()
			{
				return storedItem_;
			}

			public ys.ParameterPoint<int> gold()
			{
				return gold_;
			}

			public uint playTime()
			{
				return playTime_;
			}

			public void playTime_set(uint arg0)
			{
				playTime_ = arg0;
			}

			public PlayerExp[] playerExp()
			{
				return exp_;
			}

			public GrowUp[] growUp()
			{
				return growUp_;
			}

			public JobGrowUpType[] jobGrowUpType()
			{
				return jobGrowUpType_;
			}

			public GrowUpMp[] growUpMp(int type)
			{
				return mp_[type];
			}

			public JobEquipInfo[] jobEquipInfo()
			{
				return jobEquipInfo_;
			}

			public PlayerAbility initializeJobAbility(int job)
			{
				return initializeJobAbility_[job];
			}

			public int saveCommand(int _id)
			{
				return saveBattleCommand_[_id].saveCommand_;
			}

			public void setSaveCommand(int _id, int command_id)
			{
				saveBattleCommand_[_id].saveCommand_ = command_id;
			}

			public int saveCommandPosition(int _id)
			{
				return saveBattleCommand_[_id].saveCommandPosition_;
			}

			public void setSaveCommandPosition(int _id, int pos)
			{
				saveBattleCommand_[_id].saveCommandPosition_ = pos;
			}

			public int saveStartCommand(int _id)
			{
				return saveBattleCommand_[_id].saveStartCommand_;
			}

			public void saveStartCommand_inc(int _id)
			{
				saveBattleCommand_[_id].saveStartCommand_++;
			}

			public void saveStartCommand_dec(int _id)
			{
				saveBattleCommand_[_id].saveStartCommand_--;
			}

			public void setSaveStartCommand(int _id, int start_command)
			{
				saveBattleCommand_[_id].saveStartCommand_ = start_command;
			}

			public Mania mania()
			{
				return mania_;
			}
		}
	}
}
