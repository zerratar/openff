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
using java.io;
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
	public static partial class btl
	{
		public class BattleSystem
		{
			private BATTLE_SYSTEM_STATE state_;

			private BATTLE_SYSTEM_STATE nextState_;

			private bool isEnd_;

			private uint time_;

			private bool stop_;

			public uint turnCount_;

			private byte[] rootFormation_ = new byte[4];

			private BattleCommandWindow battleWindow_ = new BattleCommandWindow();

			private PlayerWindow playerWindow_ = new PlayerWindow();

			private menu.TargetWindow[] targetWindow_ = new menu.TargetWindow[8];

			private BattleCharacterManager characterManager_ = new BattleCharacterManager();

			private BattleNpcDataManager npcDataManager_ = new BattleNpcDataManager();

			private SummonDataManager summonDataManager_ = new SummonDataManager();

			private GeographyManager geographyManager_ = new GeographyManager();

			private BaseBattle[] battle_ = new BaseBattle[3];

			private BattleOpening opening_ = new BattleOpening();

			private BattleMain main_ = new BattleMain();

			private BattleEnding ending_ = new BattleEnding();

			public BattleSystem()
			{
				for (int i = 0; i < targetWindow_.Length; i++)
				{
					targetWindow_[i] = new menu.TargetWindow();
				}
				for (int j = 0; j < 3; j++)
				{
					battle_[j] = null;
				}
			}

			public void destruct()
			{
			}

			public void initialize()
			{
				menu.MenuManager.getSingleton().setBattleMode(flag: true);
				mon.MonsterManager.instance().load();
				mon.MonsterPartyManager.instance().load();
				npcDataManager().load();
				geographyManager().load();
				summonDataManager().load();
				if (!pl.PlayerParty.instance().isEnablePlayer())
				{
					pl.PlayerParty.instance().player(0).onIsEnable();
				}
				for (int i = 0; i < 4; i++)
				{
					pl.PlayerParty.instance().player((byte)i).updateParameter();
					rootFormation_[i] = pl.PlayerParty.instance().player((byte)i).formationType();
				}
				registerBattle();
				initializePhase();
				characterManager().initialize();
				BattleMonsterParty battleMonsterParty = characterManager().monsterParty();
				battleDisplay.setMonsterOffsetId(-1);
				int minBattleMonsterId = battleMonsterParty.getMinBattleMonsterId();
				if (minBattleMonsterId >= 0)
				{
					short num = battleMonsterParty.battleMonster(minBattleMonsterId).monsterId();
					if (mon.MonsterManager.instance().offset(num) != null && mon.MonsterManager.instance().offset(num).isSettingCamera())
					{
						battleDisplay.setMonsterOffsetId(num);
						battleDisplay.readyOpeningCamera();
					}
				}
				BattleSE.instance().loadBattleCommonSE();
				BattleSE.instance().initialize();
				BattleBGM.instance().startBattleBGM();
				battleCommandWindow().offIsCreated();
				playerWindow().setup();
				state_ = BATTLE_SYSTEM_STATE.OPENING;
				nextState_ = state_;
				time_ = 0u;
				isEnd_ = false;
				stop_ = false;
			}

			public void terminate()
			{
				dgs.CCtrlCodeInterface.instance().clear();
				characterManager().terminate();
				pl.PlayerParty.instance().clearBattleCondition();
				for (int i = 0; i < 4; i++)
				{
					if (pl.PlayerParty.instance().player((byte)i).isEnable() && !pl.PlayerParty.instance().player((byte)i).condition()
						.isDeath() && !pl.PlayerParty.instance().player((byte)i).condition()
						.isStone())
					{
						pl.PlayerParty.instance().player((byte)i).subJobPenaltyTime();
					}
					pl.PlayerParty.instance().player((byte)i).setFormationType(rootFormation_[i]);
					pl.PlayerParty.instance().player((byte)i).updateParameter();
				}
				if (opt.COptionManager.getSingleton().cursorOption().cursorPosition() == opt.CURSOR_POSITION.CURSOR_POSITION_INITIALIZE)
				{
					pl.PlayerParty.instance().clearMemory();
				}
				mon.MonsterManager.instance().free();
				mon.MonsterPartyManager.instance().free();
				npcDataManager().free();
				geographyManager().free();
				summonDataManager().free();
				pl.PlayerParty.instance().item().resetItemId();
				playerWindow().release();
				battleCommandWindow().release();
				main_.releaseData();
				Battle2DManager.instance().helpWindow().releaseHelpWindow();
				BattleEffect.instance().endEfp();
				BattleBGM.instance().free();
				BattleSE.instance().freeBattleCommonSE();
				OutsideToBattle.getInstance().setBattleCamera(BATTLE_CAMERA.OPENING_CAMERA);
				OutsideToBattle.getInstance().offEscape();
				OutsideToBattle.getInstance().initializeMonster().initialize();
				menu.MenuManager.getSingleton().setBattleMode(flag: false);
			}

			public void execute()
			{
				if (!stop_)
				{
					time_++;
					if (!isEnd() && battle() != null)
					{
						if (battle().phase() == BaseBattle.Phase.Initialize)
						{
							battle().initialize(this);
							battle().setPhase(BaseBattle.Phase.Execute);
						}
						if (battle().phase() == BaseBattle.Phase.Execute)
						{
							battle().execute(this);
						}
						if (battle().phase() == BaseBattle.Phase.Terminate)
						{
							battle().terminate(this);
							battle().setPhase(BaseBattle.Phase.Initialize);
							setState(nextState());
						}
					}
				}
				characterManager().execute();
			}

			public void preExecute()
			{
				characterManager().preExecute();
			}

			public void setNextState(BATTLE_SYSTEM_STATE state)
			{
				nextState_ = state;
				battle().setPhase(BaseBattle.Phase.Terminate);
			}

			public void registerBattle()
			{
				battle_[0] = opening_;
				battle_[1] = main_;
				battle_[2] = ending_;
			}

			public void initializePhase()
			{
				battle_[0].setPhase(BaseBattle.Phase.Initialize);
				battle_[1].setPhase(BaseBattle.Phase.Initialize);
				battle_[2].setPhase(BaseBattle.Phase.Initialize);
			}

			public void controlStop()
			{
				if (stop_)
				{
					stop_ = false;
				}
				else
				{
					stop_ = true;
				}
			}

			public bool isEdgeAButtonAndTouchEdge()
			{
				if ((ds.g_Pad.edge() & 1) == 0 && !ds.g_TouchPanel.isEdge())
				{
					return false;
				}
				return true;
			}

			public bool isEnd()
			{
				return isEnd_;
			}

			public void onEnd()
			{
				isEnd_ = true;
			}

			public void offEnd()
			{
				isEnd_ = false;
			}

			public void setState(BATTLE_SYSTEM_STATE state)
			{
				state_ = state;
			}

			public BATTLE_SYSTEM_STATE state()
			{
				return state_;
			}

			public BATTLE_SYSTEM_STATE nextState()
			{
				return nextState_;
			}

			public BaseBattle battle()
			{
				return battle_[(int)state_];
			}

			public BattleCharacterManager characterManager()
			{
				return characterManager_;
			}

			public BattleCommandWindow battleCommandWindow()
			{
				return battleWindow_;
			}

			public menu.TargetWindow[] targetWindow()
			{
				return targetWindow_;
			}

			public PlayerWindow playerWindow()
			{
				return playerWindow_;
			}

			public BattleNpcDataManager npcDataManager()
			{
				return npcDataManager_;
			}

			public GeographyManager geographyManager()
			{
				return geographyManager_;
			}

			public SummonDataManager summonDataManager()
			{
				return summonDataManager_;
			}
		}
	}
}
