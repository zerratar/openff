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
	public static partial class map
	{
							public class CEnCountManager
							{
								private RareMonsterGroupMng rmg_ = new RareMonsterGroupMng();

								private bool m_CompulsoryFlag;

								private bool m_Flag;

								private bool m_IsEnCount;

								private uint m_AddRate;

								private uint m_Rate;

								private int m_WorkFrame;

								private int m_AddTiming;

								public void initialize()
								{
									m_CompulsoryFlag = false;
									m_Flag = false;
									m_IsEnCount = false;
									m_Rate = 0u;
									m_WorkFrame = 0;
									m_AddTiming = 0;
									int num = CMapParameterManager.Instance().MapEnCountParameter(0).AreaLevel();
									int num2 = pl.PlayerParty.instance().averageLevel();
									if (num2 <= 0)
									{
										num2 = 1;
									}
									int num3 = num2 - num;
									int num4 = -1;
									num4 = ((num3 >= 0) ? ((num3 <= 1) ? 1 : 2) : 0);
									m_AddTiming = (int)CMapParameterManager.Instance().MapEnCountParameter(0).EncountRevise(num4);
									rmg_.initialize(sceneMng.getStage());
								}

								public void SetFlag(bool b)
								{
									m_Flag = b;
								}

								public bool checkEncount(pl.CPlayerCharacter pPlayer)
								{
									if (!m_Flag)
									{
										return false;
									}
									if (!checkEnCountGround())
									{
										return false;
									}
									if (m_CompulsoryFlag)
									{
										return true;
									}
									VecFx32 prePosition = pPlayer.getPrePosition();
									VecFx32 position = pPlayer.getPosition();
									if (VEC_Distance(prePosition, position) <= 2500)
									{
										return false;
									}
									m_WorkFrame++;
									if (m_WorkFrame < m_AddTiming)
									{
										return false;
									}
									m_WorkFrame = 0;
									m_Rate = (uint)calculateRate();
									int num = (int)ds.RandomNumber.rand32(100u);
									if (m_Rate < num)
									{
										return false;
									}
									return true;
								}

								public bool setBattleField(pl.CPlayerCharacter pPlayer)
								{
									int num = pPlayer.getBattleMapNo();
									if (num <= 0)
									{
										num = 1;
									}
									if (num > 43)
									{
										num = 43;
									}
									btl.OutsideToBattle.getInstance().setBattleType(btl.BATTLE_TYPE.NORMAL_BATTLE);
									btl.OutsideToBattle.getInstance().initializeBattleMap().setBattleMapId(num);
									return true;
								}

								public bool setMonsterPartyId(pl.CPlayerCharacter pPlayer)
								{
									int num = rmg_.lottery();
									if (-1 == num)
									{
										int num2 = pPlayer.getMonsterPartyGroupNo() - 1;
										if (num2 < 0 || 4 < num2)
										{
											return false;
										}
										byte b = 0;
										for (byte b2 = 0; b2 < MAP_MONSTER_PARTY_PARAM_MAX; b2++)
										{
											if (CMapParameterManager.Instance().MapMonsterPartyParameter(0).MonsterParty(num2, b2) == 0)
											{
												b++;
											}
										}
										if (b >= MAP_MONSTER_PARTY_PARAM_MAX)
										{
											return false;
										}
										int t = (short)ds.RandomNumber.rand16((ushort)(MAP_MONSTER_PARTY_PARAM_MAX - b));
										t = ds.clamp(t, 0, MAP_MONSTER_PARTY_PARAM_MAX - 1);
										num = CMapParameterManager.Instance().MapMonsterPartyParameter(0).MonsterParty(num2, t);
										if (num2 == 0 && num == 0)
										{
											return false;
										}
										if (num <= 0 || mon.MONSTER_PARTY_MAX <= num)
										{
											return false;
										}
									}
									btl.OutsideToBattle.getInstance().setBattleType(btl.BATTLE_TYPE.NORMAL_BATTLE);
									btl.OutsideToBattle.getInstance().initializeMonster().setMonsterPartyId((short)num);
									return true;
								}

								public void terminate()
								{
									initialize();
									rmg_.terminate();
								}

								public bool checkEnCountGround()
								{
									if (!CMapParameterManager.Instance().isLoaded())
									{
										return false;
									}
									if (wld.CWorldOutSideData.getInstance().MapData().MonsterPartyIndex() > 0)
									{
										return true;
									}
									return false;
								}

								public int calculateRate()
								{
									m_Rate += 5u;
									if (m_Rate > 100)
									{
										m_Rate = 100u;
									}
									return (int)m_Rate;
								}

								public CEnCountManager()
								{
									m_CompulsoryFlag = false;
									m_Flag = false;
									m_IsEnCount = false;
									m_AddRate = 0u;
									m_Rate = 0u;
									m_WorkFrame = 0;
									m_AddTiming = 0;
								}

								public void setCompulsoryFlag(bool _CompulsoryFlag)
								{
									m_CompulsoryFlag = _CompulsoryFlag;
								}

								public bool IsFlag()
								{
									return m_Flag;
								}

								public void setIsEnCount(bool _IsEnCount)
								{
									m_IsEnCount = _IsEnCount;
								}

								public bool IsEnCount()
								{
									return m_IsEnCount;
								}

								public void setAddRate(uint _AddRate)
								{
									m_AddRate = _AddRate;
								}

								public uint AddRate()
								{
									return m_AddRate;
								}

								public uint Rate()
								{
									return m_Rate;
								}

								public void setForceEncount(bool b)
								{
									m_CompulsoryFlag = b;
								}

								public void clearEncountFrame()
								{
									m_WorkFrame = 0;
								}
							}
	}
}
