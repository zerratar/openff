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
	public static partial class btl
	{
		public class BattleTurnExecute : BaseBattle
		{
			public enum TURN_PHASE
			{
				TURN_IDLE,
				TURN_SORT_PHASE,
				TURN_COVER_FIRE,
				TURN_NPC_PHASE,
				TURN_EXECUTE_PHASE,
				TURN_PHASE_MAX
			}

			public enum RESULT_TYPE
			{
				RESULT_WIN = 1,
				RESULT_LOSE = 2,
				RESULT_ESCAPE = 4
			}

			public const TURN_PHASE TURN_IDLE = TURN_PHASE.TURN_IDLE;

			public const TURN_PHASE TURN_SORT_PHASE = TURN_PHASE.TURN_SORT_PHASE;

			public const TURN_PHASE TURN_COVER_FIRE = TURN_PHASE.TURN_COVER_FIRE;

			public const TURN_PHASE TURN_NPC_PHASE = TURN_PHASE.TURN_NPC_PHASE;

			public const TURN_PHASE TURN_EXECUTE_PHASE = TURN_PHASE.TURN_EXECUTE_PHASE;

			public const TURN_PHASE TURN_PHASE_MAX = TURN_PHASE.TURN_PHASE_MAX;

			public const RESULT_TYPE RESULT_WIN = RESULT_TYPE.RESULT_WIN;

			public const RESULT_TYPE RESULT_LOSE = RESULT_TYPE.RESULT_LOSE;

			public const RESULT_TYPE RESULT_ESCAPE = RESULT_TYPE.RESULT_ESCAPE;

			private TURN_PHASE turnPhase_;

			private int actionCharacterNumber_;

			private int nowActionCharacter_;

			private int startCounter_;

			private int result_;

			private bool playerSubWindowCreate_;

			private TurnSystem turn_ = new TurnSystem();

			private BattleNpcManager npcManager_ = new BattleNpcManager();

			public override void initialize(BattleSystem B)
			{
				turn_.initializeAll();
				turnPhase_ = TURN_PHASE.TURN_SORT_PHASE;
				actionCharacterNumber_ = 0;
				nowActionCharacter_ = 0;
				startCounter_ = 10;
				playerSubWindowCreate_ = false;
				turn_.setPhase(TurnSystem.Phase.Initialize);
				turn_.setPlayerWindow(B.playerWindow());
				turn_.setGeographyManager(B.geographyManager());
				turn_.setSummonDataManager(B.summonDataManager());
				turn_.setState(0);
				turn_.setCharacterManager(B.characterManager());
				if (!OutsideToBattle.getInstance().isFreeMode())
				{
					OutsideToBattle.getInstance().setBattleCamera(BATTLE_CAMERA.MOVE_CAMERA);
					battleDisplay.setMoveFrame(18);
				}
			}

			public override void terminate(BattleSystem B)
			{
			}

			public override void execute(BattleSystem B)
			{
				if (OutsideToBattle.getInstance().battleCamera() != BATTLE_CAMERA.MAIN_CAMERA && !OutsideToBattle.getInstance().isFreeMode())
				{
					return;
				}
				if (!playerSubWindowCreate_)
				{
					playerSubWindowCreate_ = true;
				}
				if (startCounter_-- != 0)
				{
					return;
				}
				startCounter_ = 0;
				switch (turnPhase_)
				{
				case TURN_PHASE.TURN_SORT_PHASE:
					sortCharacter(B.characterManager());
					turnPhase_ = TURN_PHASE.TURN_COVER_FIRE;
					break;
				case TURN_PHASE.TURN_COVER_FIRE:
					if (turn_.coverFire())
					{
						npcManager_.setup();
						turnPhase_ = TURN_PHASE.TURN_NPC_PHASE;
					}
					break;
				case TURN_PHASE.TURN_NPC_PHASE:
					actNpc(B);
					break;
				case TURN_PHASE.TURN_EXECUTE_PHASE:
					OutsideToBattle.getInstance().setBattleOpeningType(BATTLE_OPENING_TYPE.CALC_BATTLE_OPENING_TYPE);
					turnExecute(B);
					break;
				}
			}

			public void releaseData()
			{
				npcManager_.cleanup();
			}

			public void sortCharacter(BattleCharacterManager BCM)
			{
				BCM.clearOrder();
				ds.Vector<OrderParameter, ds.FastErasePolicy<OrderParameter>> vector = new ds.Vector<OrderParameter, ds.FastErasePolicy<OrderParameter>>(10);
				vector.clear();
				MoveOrderFormula moveOrderFormula = new MoveOrderFormula();
				BattleParty battleParty = BCM.playerParty();
				for (int i = 0; i < 4; i++)
				{
					if (!battleParty.battlePlayer(i).isEnable())
					{
						continue;
					}
					OrderParameter orderParameter = new OrderParameter();
					orderParameter.breed = battleParty.battlePlayer(i).breed();
					orderParameter._id = i;
					orderParameter.dex = moveOrderFormula.moveDexterityPlayer(battleParty.battlePlayer(i));
					orderParameter.dex += (int)ds.RandomNumber.rand32((uint)(orderParameter.dex + 1));
					if (battleParty.battlePlayer(i).actionId() == 3 || battleParty.battlePlayer(i).actionId() == 13 || battleParty.battlePlayer(i).actionId() == 12 || battleParty.battlePlayer(i).actionId() == 23 || battleParty.battlePlayer(i).actionId() == 20)
					{
						orderParameter.dex += 10000;
					}
					if (vector.empty())
					{
						vector.insert(0, orderParameter);
					}
					else
					{
						bool flag = false;
						for (int j = 0; j < vector.size(); j++)
						{
							if (orderParameter.dex > vector.at(j).dex)
							{
								vector.insert(j, orderParameter);
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							vector.push_back(orderParameter);
						}
					}
					actionCharacterNumber_++;
				}
				BattleMonsterParty battleMonsterParty = BCM.monsterParty();
				for (int i = 0; i < 6; i++)
				{
					if (!battleMonsterParty.battleMonster(i).isEnable())
					{
						continue;
					}
					OrderParameter orderParameter2 = new OrderParameter();
					orderParameter2.breed = battleMonsterParty.battleMonster(i).breed();
					orderParameter2._id = i;
					orderParameter2.dex = moveOrderFormula.moveDexterityMonster(battleMonsterParty.battleMonster(i));
					orderParameter2.dex += (int)ds.RandomNumber.rand32((uint)(orderParameter2.dex + 1));
					if (vector.empty())
					{
						vector.insert(0, orderParameter2);
					}
					else
					{
						bool flag2 = false;
						for (int j = 0; j < vector.size(); j++)
						{
							if (orderParameter2.dex > vector.at(j).dex)
							{
								vector.insert(j, orderParameter2);
								flag2 = true;
								break;
							}
						}
						if (!flag2)
						{
							vector.push_back(orderParameter2);
						}
					}
					actionCharacterNumber_++;
				}
				for (int i = 0; i < actionCharacterNumber_; i++)
				{
					if (vector.at(i).breed == 0)
					{
						BCM.setOrder(i, battleParty.battlePlayer(vector.at(i)._id));
					}
					else if (vector.at(i).breed == 1)
					{
						BCM.setOrder(i, battleMonsterParty.battleMonster(vector.at(i)._id));
					}
				}
				if (vector.size() != actionCharacterNumber_)
				{
					OS_Printf("リストのサイズと actionCharacterNumber_ が違う値です\n");
				}
			}

			public void actNpc(BattleSystem B)
			{
				npcManager_.execute(B);
				if (npcManager_.isEnd())
				{
					turnPhase_ = TURN_PHASE.TURN_EXECUTE_PHASE;
				}
			}

			public void turnExecute(BattleSystem B)
			{
				if (turn_.phase() == TurnSystem.Phase.Initialize)
				{
					if (turn_.counterCharacter() != null)
					{
						turn_.setNowCharacter(turn_.counterCharacter());
					}
					else
					{
						turn_.setNowCharacter(B.characterManager().order(nowActionCharacter_));
					}
					turn_.setCharacterManager(B.characterManager());
					turn_.initializeTurn();
					if (turn_.phase() != TurnSystem.Phase.Terminate)
					{
						turn_.setPhase(TurnSystem.Phase.Execute);
					}
				}
				else if (turn_.phase() == TurnSystem.Phase.Execute)
				{
					turn_.executeTurn();
				}
				else if (turn_.phase() == TurnSystem.Phase.MonsterExecute)
				{
					turn_.monsterExecute();
				}
				else if (turn_.phase() == TurnSystem.Phase.Terminate)
				{
					turn_.terminateTurn();
					turn_.clearCheckFlagAll();
					if (turn_.counterCharacter() != null)
					{
						if (turn_.counterCharacter().isActionEnd())
						{
							turn_.setCounterCharacter(null);
						}
					}
					else if (turn_.nowCharacter() != null)
					{
						if (turn_.nowCharacter().isActionEnd())
						{
							nowActionCharacter_++;
						}
						else if (turn_.nowCharacter().breed() == 1)
						{
							BattleSetupEnemy battleSetupEnemy = new BattleSetupEnemy();
							BattleMonster monster = static_cast<BattleMonster>(turn_.nowCharacter());
							battleSetupEnemy.selectAction(monster);
						}
					}
					else
					{
						nowActionCharacter_++;
					}
					if (turn_.isPlayerEscape())
					{
						turn_.terminateTurn();
						setPhase(Phase.Terminate);
						if (isTurnEnd(B.characterManager()))
						{
							sendResultTypeToOutside();
							B.setNextState(BATTLE_SYSTEM_STATE.ENDING);
						}
						else
						{
							BattleMain battleMain = static_cast<BattleMain>(B.battle());
							B.turnCount_++;
							battleMain.setNextBattleMainState(BATTLE_MAIN_STATE.SETUP_PLAYER);
						}
						return;
					}
					if (actionCharacterNumber_ == nowActionCharacter_)
					{
						turn_.setPhase(TurnSystem.Phase.CommonExecute);
					}
					else
					{
						turn_.setPhase(TurnSystem.Phase.Initialize);
					}
				}
				if (actionCharacterNumber_ == nowActionCharacter_ && turn_.phase() == TurnSystem.Phase.CommonExecute && turn_.commonExecute())
				{
					turn_.terminateTurn();
					setPhase(Phase.Terminate);
					if (isTurnEnd(B.characterManager()))
					{
						sendResultTypeToOutside();
						B.setNextState(BATTLE_SYSTEM_STATE.ENDING);
					}
					else
					{
						BattleMain battleMain2 = static_cast<BattleMain>(B.battle());
						B.turnCount_++;
						battleMain2.setNextBattleMainState(BATTLE_MAIN_STATE.SETUP_PLAYER);
					}
				}
			}

			public bool isTurnEnd(BattleCharacterManager BCM)
			{
				result_ = 0;
				for (int i = 0; i < 4; i++)
				{
					BattlePlayer battlePlayer = BCM.playerParty().battlePlayer(i);
					if (battlePlayer != null)
					{
						if (battlePlayer.isBattle())
						{
							result_ |= 1;
						}
						if (battlePlayer.flag(PLAYER_FLAG.PF_ESCAPE) && turn_.isPlayerEscape())
						{
							result_ |= 4;
							return true;
						}
					}
				}
				for (int i = 0; i < 6; i++)
				{
					BattleMonster battleMonster = BCM.monsterParty().battleMonster(i);
					if (battleMonster != null && battleMonster.isBattle())
					{
						result_ |= 2;
						break;
					}
				}
				if ((result_ & 1) != 0 && (result_ & 2) != 0)
				{
					return false;
				}
				if (result_ == 0)
				{
					result_ |= 2;
				}
				return true;
			}

			public void sendResultTypeToOutside()
			{
				BATTLE_RESULT battleResult = BATTLE_RESULT.ERR_RESULT;
				if ((result_ & 4) != 0)
				{
					battleResult = BATTLE_RESULT.PLAYER_ESCAPE;
				}
				else if ((result_ & 1) != 0)
				{
					battleResult = BATTLE_RESULT.WIN;
				}
				else if ((result_ & 2) != 0)
				{
					battleResult = BATTLE_RESULT.LOSE;
				}
				BattleToOutside.getInstance().setBattleResult(battleResult);
			}
		}
	}
}
