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
	public static partial class map
	{
							public class CMapObject : pl.CBasePlayer
							{
								public enum TREASURE_BOX_STATE
								{
									TREASURE_BOX_STATE_CLOSE_START,
									TREASURE_BOX_STATE_CLOSE_MOVE,
									TREASURE_BOX_STATE_CLOSE_END,
									TREASURE_BOX_STATE_OPEN_START,
									TREASURE_BOX_STATE_OPEN_MOVE,
									TREASURE_BOX_STATE_OPEN_MOVE2,
									TREASURE_BOX_STATE_OPEN_END,
									TREASURE_BOX_STATE_MAX_CLOSE_START,
									TREASURE_BOX_STATE_MAX_CLOSE_MOVE,
									TREASURE_BOX_STATE_MAX_CLOSE_END,
									TREASURE_BOX_STATE_MAX
								}

								public const TREASURE_BOX_STATE TREASURE_BOX_STATE_CLOSE_START = TREASURE_BOX_STATE.TREASURE_BOX_STATE_CLOSE_START;

								public const TREASURE_BOX_STATE TREASURE_BOX_STATE_CLOSE_MOVE = TREASURE_BOX_STATE.TREASURE_BOX_STATE_CLOSE_MOVE;

								public const TREASURE_BOX_STATE TREASURE_BOX_STATE_CLOSE_END = TREASURE_BOX_STATE.TREASURE_BOX_STATE_CLOSE_END;

								public const TREASURE_BOX_STATE TREASURE_BOX_STATE_OPEN_START = TREASURE_BOX_STATE.TREASURE_BOX_STATE_OPEN_START;

								public const TREASURE_BOX_STATE TREASURE_BOX_STATE_OPEN_MOVE = TREASURE_BOX_STATE.TREASURE_BOX_STATE_OPEN_MOVE;

								public const TREASURE_BOX_STATE TREASURE_BOX_STATE_OPEN_MOVE2 = TREASURE_BOX_STATE.TREASURE_BOX_STATE_OPEN_MOVE2;

								public const TREASURE_BOX_STATE TREASURE_BOX_STATE_OPEN_END = TREASURE_BOX_STATE.TREASURE_BOX_STATE_OPEN_END;

								public const TREASURE_BOX_STATE TREASURE_BOX_STATE_MAX_CLOSE_START = TREASURE_BOX_STATE.TREASURE_BOX_STATE_MAX_CLOSE_START;

								public const TREASURE_BOX_STATE TREASURE_BOX_STATE_MAX_CLOSE_MOVE = TREASURE_BOX_STATE.TREASURE_BOX_STATE_MAX_CLOSE_MOVE;

								public const TREASURE_BOX_STATE TREASURE_BOX_STATE_MAX_CLOSE_END = TREASURE_BOX_STATE.TREASURE_BOX_STATE_MAX_CLOSE_END;

								public const TREASURE_BOX_STATE TREASURE_BOX_STATE_MAX = TREASURE_BOX_STATE.TREASURE_BOX_STATE_MAX;

								private MAP_OBJECT_TYPE m_MapObjType;

								private uint m_FlagGroup;

								private uint m_FlagIndex;

								private uint m_EnCountIndex;

								private uint m_ItemId;

								private int m_ItemNum;

								private int m_Gold;

								private MapSignEffect m_pSignEffect;

								public override void initialize()
								{
									base.initialize();
									m_ActionMng.initialize();
									PlayerMoveType_set(pl.PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_ERR);
									NPCRandomMoveType_set(pl.NPC_RANDOM_MOVE_TYPE.NPC_RANDOM_MOVE_TYPE_DEFAULT);
									NPCAutoFollowType_set(pl.NPC_AUTO_FOLLOW_TYPE.NPC_AUTO_FOLLOW_TYPE_DEFAULT);
									m_Target = null;
									CharaKind_set(chr.CHARACTER_KIND.CHARACTER_KIND_MAP_OBJECT);
									CharaCheckType_set(chr.CHARACTER_CHECK_TYPE.CHARACTER_CHECK_TYPE_CHECK);
									m_MapObjType = MAP_OBJECT_TYPE.MAP_OBJECT_TYPE_ERR;
									m_FlagGroup = 0u;
									m_FlagIndex = 0u;
									m_EnCountIndex = 0u;
									m_ItemId = 0u;
									m_ItemNum = 0;
									m_Gold = 0;
									m_pSignEffect = null;
								}

								public override void execute()
								{
									if (m_pSignEffect != null)
									{
										m_pSignEffect.execute();
									}
									base.execute();
									if (getCharacterId() == -1)
									{
										return;
									}
									if (m_MapObjType == MAP_OBJECT_TYPE.TREASURE_BOX)
									{
										switch (m_NowAct)
										{
										case 0:
											startMotion(1003, _Loop: false, 5u);
											m_NowAct = 1;
											break;
										case 1:
											if (isEndOfMotion())
											{
												m_NowAct = 2;
											}
											break;
										case 3:
											if (FlagManager.singleton().get(m_FlagGroup, m_FlagIndex) == 0)
											{
												MatrixSound.MtxSENDS_Play(1, 36, 192, 127);
												startMotion(1001, _Loop: false, 5u);
												int num = eff.CEffectMng.instance().create(102, 1);
												if (num != -1)
												{
													VecFx32 vecFx = new VecFx32(getPosition());
													vecFx.y += 24576;
													eff.CEffectMng.instance().setPosition(num, vecFx);
												}
												m_NowAct = 4;
											}
											else
											{
												CCastCommandTransit.getInstance().cast_PlayerMng().Player(0)
													.setAutoPilot(_AutoPilot: false);
												evt.CEventManager.getInstance().setEvent(_Event: false);
												m_NowAct = 6;
											}
											break;
										case 4:
										{
											if (!isEndOfMotion())
											{
												break;
											}
											CCastCommandTransit.getInstance().cast_Field2D().MessageWindow()
												.createWindow(1);
											bool flag = true;
											int num2 = 0;
											num2 = 1000140;
											if (m_Gold > 0)
											{
												num2 = 1000141;
												if (pl.PlayerParty.instance().gold().get() >= 9999999)
												{
													num2 = 1000148;
													flag = false;
												}
												else if (m_EnCountIndex != 0)
												{
													num2 = 1000143;
												}
											}
											else if (m_ItemId != 0)
											{
												num2 = 1000142;
												itm.PossessionItem possessionItem = pl.PlayerParty.instance().item().serchNormalItem((short)m_ItemId);
												if (possessionItem != null && possessionItem.itemNumber() >= itm.LIMIT_OF_ITEM)
												{
													num2 = 1000145;
													flag = false;
												}
												else if (m_EnCountIndex != 0)
												{
													num2 = 1000144;
												}
											}
											if (m_Gold != 0)
											{
												dgs.CCtrlCodeInterface.instance().setGold(m_Gold);
											}
											if (m_ItemId != 0)
											{
												dgs.CCtrlCodeInterface.instance().setItemId(itm.ItemManager.instance().itemParameter((short)m_ItemId).nameId());
											}
											CCastCommandTransit.getInstance().cast_Field2D().MessageWindow()
												.setMessageColor(9);
											CCastCommandTransit.getInstance().cast_Field2D().MessageWindow()
												.createMessage(num2, 0, 0);
											if (!flag)
											{
												m_NowAct = 7;
												break;
											}
											addItem();
											addGold();
											m_NowAct = 5;
											break;
										}
										case 5:
											if (!CCastCommandTransit.getInstance().cast_Field2D().MessageWindow()
												.isMadeMessage())
											{
												CCastCommandTransit.getInstance().cast_Field2D().MessageWindow()
													.releaseWindow();
												CCastCommandTransit.getInstance().cast_Field2D().MessageWindow()
													.releaseMessage();
												CCastCommandTransit.getInstance().cast_Field2D().MessageWindow()
													.release();
												if (m_EnCountIndex != 0)
												{
													btl.OutsideToBattle.getInstance().setBattleType(btl.BATTLE_TYPE.EVENT_BATTLE);
													btl.OutsideToBattle.getInstance().initializeMonster().setMonsterPartyId((short)m_EnCountIndex);
													int battleMapId = ((!CMapParameterManager.Instance().isLoaded()) ? 1 : CMapParameterManager.Instance().MapLandFormParameter(0).BattleFieldIndex(0));
													btl.OutsideToBattle.getInstance().initializeBattleMap().setBattleMapId(battleMapId);
													wld.CBaseSystem.setBattle(b: true);
												}
												FlagManager.singleton().set(m_FlagGroup, m_FlagIndex);
												pl.PlayerParty.instance().mania().countTresureBox();
												CCastCommandTransit.getInstance().cast_PlayerMng().Player(0)
													.setAutoPilot(_AutoPilot: false);
												evt.CEventManager.getInstance().setEvent(_Event: false);
												m_NowAct = 6;
											}
											break;
										case 6:
											startMotion(1002, _Loop: true, 5u);
											break;
										case 7:
											if (!CCastCommandTransit.getInstance().cast_Field2D().MessageWindow()
												.isMadeMessage())
											{
												CCastCommandTransit.getInstance().cast_Field2D().MessageWindow()
													.releaseWindow();
												CCastCommandTransit.getInstance().cast_Field2D().MessageWindow()
													.releaseMessage();
												CCastCommandTransit.getInstance().cast_Field2D().MessageWindow()
													.release();
												startMotion(1003, _Loop: false, 5u);
												m_NowAct = 8;
											}
											break;
										case 8:
											if (isEndOfMotion())
											{
												CCastCommandTransit.getInstance().cast_PlayerMng().Player(0)
													.setAutoPilot(_AutoPilot: false);
												evt.CEventManager.getInstance().setEvent(_Event: false);
												m_NowAct = 2;
											}
											break;
										}
									}
									else if (m_MapObjType == MAP_OBJECT_TYPE.INVISIBLE)
									{
										switch (m_NowAct)
										{
										case 3:
											if (FlagManager.singleton().get(m_FlagGroup, m_FlagIndex) == 0)
											{
												m_NowAct = 4;
												break;
											}
											CCastCommandTransit.getInstance().cast_PlayerMng().Player(0)
												.setAutoPilot(_AutoPilot: false);
											evt.CEventManager.getInstance().setEvent(_Event: false);
											m_NowAct = 6;
											break;
										case 4:
											if (m_ItemId != 0)
											{
												CCastCommandTransit.getInstance().cast_Field2D().MessageWindow()
													.createWindow(1);
												int mesNum = 1000146;
												bool flag2 = true;
												itm.PossessionItem possessionItem2 = pl.PlayerParty.instance().item().serchNormalItem((short)m_ItemId);
												if (possessionItem2 != null && possessionItem2.itemNumber() >= itm.LIMIT_OF_ITEM)
												{
													mesNum = 1000147;
													flag2 = false;
												}
												if (m_ItemId != 0)
												{
													dgs.CCtrlCodeInterface.instance().setItemId(itm.ItemManager.instance().itemParameter((short)m_ItemId).nameId());
												}
												CCastCommandTransit.getInstance().cast_Field2D().MessageWindow()
													.setMessageColor(9);
												CCastCommandTransit.getInstance().cast_Field2D().MessageWindow()
													.createMessage(mesNum, 0, 0);
												if (!flag2)
												{
													m_NowAct = 7;
													break;
												}
												addItem();
												m_NowAct = 5;
											}
											else
											{
												CCastCommandTransit.getInstance().cast_PlayerMng().Player(0)
													.setAutoPilot(_AutoPilot: false);
												evt.CEventManager.getInstance().setEvent(_Event: false);
												m_NowAct = 6;
											}
											break;
										case 5:
											if (!CCastCommandTransit.getInstance().cast_Field2D().MessageWindow()
												.isMadeMessage())
											{
												CCastCommandTransit.getInstance().cast_Field2D().MessageWindow()
													.release();
												if (m_EnCountIndex != 0)
												{
													btl.OutsideToBattle.getInstance().setBattleType(btl.BATTLE_TYPE.EVENT_BATTLE);
													btl.OutsideToBattle.getInstance().initializeMonster().setMonsterPartyId((short)m_EnCountIndex);
													int battleMapId2 = ((!CMapParameterManager.Instance().isLoaded()) ? 1 : CMapParameterManager.Instance().MapLandFormParameter(0).BattleFieldIndex(0));
													btl.OutsideToBattle.getInstance().initializeBattleMap().setBattleMapId(battleMapId2);
													wld.CBaseSystem.setBattle(b: true);
												}
												FlagManager.singleton().set(m_FlagGroup, m_FlagIndex);
												CCastCommandTransit.getInstance().cast_PlayerMng().Player(0)
													.setAutoPilot(_AutoPilot: false);
												evt.CEventManager.getInstance().setEvent(_Event: false);
												m_NowAct = 6;
											}
											break;
										case 7:
											if (!CCastCommandTransit.getInstance().cast_Field2D().MessageWindow()
												.isMadeMessage())
											{
												CCastCommandTransit.getInstance().cast_Field2D().MessageWindow()
													.releaseWindow();
												CCastCommandTransit.getInstance().cast_Field2D().MessageWindow()
													.releaseMessage();
												CCastCommandTransit.getInstance().cast_Field2D().MessageWindow()
													.release();
												CCastCommandTransit.getInstance().cast_PlayerMng().Player(0)
													.setAutoPilot(_AutoPilot: false);
												evt.CEventManager.getInstance().setEvent(_Event: false);
												m_NowAct = 2;
											}
											break;
										}
									}
									m_ActionMng.execute();
								}

								public override void terminate()
								{
									if (m_pSignEffect != null)
									{
										m_pSignEffect.destruct();
										ds.CHeap.free_app(m_pSignEffect);
										m_pSignEffect = null;
									}
									base.terminate();
									m_ActionMng.terminate();
								}

								public override void into()
								{
									base.into();
									setMCLCol(b: false);
									getColFlag_not_and(1);
									getColFlag_or(2);
									getColFlag_or(4);
									getColFlag_not_and(8);
									getColFlag_not_and(16);
									getColFlag_not_and(4096);
									isGrv_set(arg0: false);
									getColRadius_set(MapObjectCollisionRadius[(int)m_MapObjType]);
									getCckRadius_set(MapObjectCheckRadius[(int)m_MapObjType]);
									getTchRadius_set(MapObjectTouchRadius[(int)m_MapObjType]);
									getColAabbRadius().x = MapObjectCollisionAABB[(int)m_MapObjType][0];
									getColAabbRadius().y = MapObjectCollisionAABB[(int)m_MapObjType][1];
									getColAabbRadius().z = MapObjectCollisionAABB[(int)m_MapObjType][2];
									if (m_MapObjType == MAP_OBJECT_TYPE.WIND_CRYSTAL)
									{
										VEC_Set(getCckOffset(), 0, 0, 40960);
										VEC_Set(getTchOffset(), 0, 81920, -61440);
									}
									setShadowType(MapObjectShadowType[(int)m_MapObjType]);
									setShadowScale(const_cast<VecFx32>(MapObjectShadowScale[(int)m_MapObjType]));
								}

								public override void update()
								{
									base.update();
								}

								public override void reset()
								{
									base.reset();
								}

								public override void checkCollisionCharacter(chr.CCharacterEureka _Target)
								{
								}

								public void addItem()
								{
									if (m_ItemId != 0)
									{
										if (itm.ItemManager.instance().itemCategory((short)m_ItemId) == itm.CATEGORY.CATEGORY_IMPORTANT)
										{
											MatrixSound.MtxSENDS_Play(1, 38, 192, 127);
										}
										else
										{
											MatrixSound.MtxSENDS_Play(1, 38, 192, 127);
										}
										pl.PlayerParty.instance().addItem((int)m_ItemId, m_ItemNum);
										m_ItemId = 0u;
										m_ItemNum = 0;
									}
								}

								public void addGold()
								{
									if (m_Gold <= 0)
									{
										return;
									}
									MatrixSound.MtxSENDS_Play(1, 38, 192, 127);
									pl.PlayerParty.instance().gold().add(m_Gold);
									if (pl.PlayerParty.instance().gold().get() >= 50000)
									{
										UserInfo.AwardAchievement(6);
										if (pl.PlayerParty.instance().gold().get() >= 500000)
										{
											UserInfo.AwardAchievement(7);
										}
									}
									m_Gold = 0;
								}

								public void setSignEffect(cmr.CWorldCamera pWldCam, VecFx32 offset, int eveFlagGroup, int eveFlagIndex)
								{
									if (m_pSignEffect == null)
									{
										m_pSignEffect = static_cast<MapSignEffect>(ds.CHeap.alloc_app(typeof(MapSignEffect)));
										m_pSignEffect.setup(pWldCam);
										VecFx32 vecFx = new VecFx32(getPosition());
										VEC_Add(offset, vecFx, vecFx);
										m_pSignEffect.setPos(vecFx);
										m_pSignEffect.setEventFlag(eveFlagGroup, eveFlagIndex);
									}
								}

								public void enableSignEffect(bool enable)
								{
									if (m_pSignEffect != null)
									{
										m_pSignEffect.setEnable(enable);
									}
								}

								public void eraseSignEffect()
								{
									if (m_pSignEffect != null)
									{
										m_pSignEffect.erase();
									}
								}

								public CMapObject()
								{
									m_MapObjType = MAP_OBJECT_TYPE.MAP_OBJECT_TYPE_ERR;
									m_FlagGroup = 0u;
									m_FlagIndex = 0u;
									m_EnCountIndex = 0u;
									m_ItemId = 0u;
									m_ItemNum = 0;
									m_Gold = 0;
								}

								public void setMapObjType(MAP_OBJECT_TYPE _MapObjType)
								{
									m_MapObjType = _MapObjType;
								}

								public MAP_OBJECT_TYPE MapObjType()
								{
									return m_MapObjType;
								}

								public void setFlag(uint _FlagGroup, uint _FlagIndex)
								{
									m_FlagGroup = _FlagGroup;
									m_FlagIndex = _FlagIndex;
								}

								public uint getFlagGroup()
								{
									return m_FlagGroup;
								}

								public uint getFlagIndex()
								{
									return m_FlagIndex;
								}

								public void setEnCountIndex(uint _EnCountIndex)
								{
									m_EnCountIndex = _EnCountIndex;
								}

								public void setItemId(uint _ItemId)
								{
									m_ItemId = _ItemId;
								}

								public uint getItemId()
								{
									return m_ItemId;
								}

								public uint itemId()
								{
									return m_ItemId;
								}

								public void setItemNum(int _ItemNum)
								{
									m_ItemNum = _ItemNum;
								}

								public int getItemNum()
								{
									return m_ItemNum;
								}

								public void setGold(int _Gold)
								{
									m_Gold = _Gold;
								}

								public int getGold()
								{
									return m_Gold;
								}

								public MapSignEffect getSignEffect()
								{
									return m_pSignEffect;
								}
							}
	}
}
