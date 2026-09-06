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
	public static partial class chr
	{
		public class CCharacterEureka : CBaseCharacter
		{
			public const int STOCK_MOTION_NUM = 8;

			public static uint m_CharaFps = 60u;

			protected bool m_ItemEvent;

			protected bool m_Balloon;

			protected bool m_AutoRun;

			protected bool m_Operater;

			protected bool m_InvalidActionMotion;

			protected bool m_TargetLookFlag;

			protected sbyte m_LandFormIndex;

			protected int m_TargetLookFrame;

			protected uint m_LogicIndex;

			protected int m_StockMotionIndex;

			protected SStockMotionParameter[] m_StockMotionParameter = new SStockMotionParameter[8];

			protected VecFx32 m_AutoScaleSpeed = new VecFx32();

			protected int m_AutoScaleFrame;

			protected int m_SucAlpha;

			protected int m_AutoAlphaFrame;

			protected int m_WorkAutoAlphaFrame;

			protected int m_SucShadowAlpha;

			protected int m_AutoShadowAlphaFrame;

			protected int m_WorkAutoShadowAlphaFrame;

			protected int m_AutoDeleteFrame;

			protected int m_WorkAutoDeleteFrame;

			protected CHARACTER_KIND m_CharaKind;

			protected CHARACTER_CHECK_TYPE m_CharaCheckType;

			protected CCharacterMoveSys m_MoveSys = new CCharacterMoveSys();

			protected CCharacterTurnSys m_TurnSys = new CCharacterTurnSys();

			protected int m_PreAct;

			protected int m_NowAct;

			protected int m_NextAct;

			protected mcl.CollisionResult[] m_colResultWall = new mcl.CollisionResult[5];

			protected CCharacterEureka m_Target;

			protected bool sAttr_;

			protected int monPartyGroupNo_;

			protected bool m_bScriptWallCollision;

			public override void initialize()
			{
				base.reset();
				m_ItemEvent = false;
				m_Balloon = false;
				m_AutoRun = false;
				m_Operater = false;
				m_InvalidActionMotion = false;
				m_TargetLookFlag = false;
				m_TargetLookFrame = 0;
				m_LandFormIndex = -1;
				m_LogicIndex = 0u;
				m_StockMotionIndex = -1;
				for (int i = 0; i < 8; i++)
				{
					resetStockMotionParameter(i);
				}
				VEC_Set(m_AutoScaleSpeed, -1, -1, -1);
				m_AutoScaleFrame = -1;
				m_SucAlpha = -1;
				m_AutoAlphaFrame = -1;
				m_WorkAutoAlphaFrame = -1;
				m_SucShadowAlpha = -1;
				m_AutoShadowAlphaFrame = -1;
				m_WorkAutoShadowAlphaFrame = -1;
				m_AutoDeleteFrame = -1;
				m_WorkAutoDeleteFrame = -1;
				m_CharaKind = CHARACTER_KIND.CHARACTER_KIND_ERR;
				m_CharaCheckType = CHARACTER_CHECK_TYPE.CHARACTER_CHECK_TYPE_ERR;
				m_MoveSys.initialize();
				m_TurnSys.initialize();
				m_NowAct = 0;
				m_NextAct = 0;
				m_Target = null;
				sAttr_ = false;
				monPartyGroupNo_ = 1;
				setMCLCol(b: false);
				setScriptWallCollision(b: false);
			}

			public override void execute()
			{
				m_MoveSys.execute();
				m_TurnSys.execute();
				autoPlayStockMotion();
				autoLookTarget();
				autoScale();
				autoAlpha();
				autoShadowAlpha();
				autoDelete();
			}

			public override void terminate()
			{
				m_MoveSys.terminate();
				m_TurnSys.terminate();
				deleteCharacter();
			}

			public virtual void into()
			{
				m_MoveSys.initialize();
				m_TurnSys.initialize();
				m_MoveSys.setCharacter(this);
				m_TurnSys.setCharacter(this);
				setMCLCol(b: true);
				getColFlag_not_and(1);
				getColFlag_or(2);
				getColFlag_or(4);
				getColFlag_or(8);
				getColFlag_or(16);
				getColFlag_or(32);
				getColFlag_or(64);
				getColFlag_or(128);
				getColFlag_or(256);
				getColFlag_or(512);
				getColFlag_or(1024);
				getColFlag_or(2048);
				getColFlag_or(4096);
				getColFlag_or(8192);
				getColFlag_or(16384);
				getColFlag_or(32768);
				getColFlag_or(65536);
				getColFlag_or(131072);
				getColFlag_or(262144);
				getColFlag_or(524288);
				getColFlag_or(1048576);
				getColFlag_or(2097152);
				getColFlag_or(4194304);
				getColFlag_or(8388608);
				isGrv_set(arg0: true);
			}

			public void setMCLCol(bool b)
			{
				redSetActivity(b);
			}

			public void setScriptWallCollision(bool b)
			{
				m_bScriptWallCollision = b;
			}

			public bool isScriptWallCollision()
			{
				return m_bScriptWallCollision;
			}

			public override void update()
			{
				base.update();
				m_MoveSys.update();
				m_TurnSys.update();
				if (m_ShadowType == 4)
				{
					int num = 0;
					switch (m_LandFormIndex - 1)
					{
					case 4:
						num = 4096;
						break;
					case 7:
						num = 16384;
						break;
					case 9:
						num = 16384;
						break;
					}
					characterMng.setShadowHeight(getCharacterId(), -num - getPosition().y + 2048);
				}
			}

			public override void reset()
			{
				base.reset();
			}

			public override void dgsredAccept(dgs.CRestrictor ror)
			{
				if (!isMCLCol())
				{
					return;
				}
				mcl.CollisionResult chr_reuse_result = chr.chr_reuse_result;
				VecFx32 chr_reuse_dir_ = chr.chr_reuse_dir_;
				VecFx32 chr_reuse_currentPos_ = chr.chr_reuse_currentPos_;
				VecFx32 chr_reuse_oldPos_ = chr.chr_reuse_oldPos_;
				chr_reuse_currentPos_.copy(getPosition());
				chr_reuse_oldPos_.copy(getPrePosition());
				short num = -1;
				int num2 = -1;
				short num3 = -1;
				short num4 = -1;
				bool flag = false;
				VEC_Subtract(chr_reuse_currentPos_, chr_reuse_oldPos_, chr_reuse_dir_);
				VEC_Mag(chr_reuse_dir_);
				if (!IS_ZERO_NORM(chr_reuse_dir_))
				{
					VEC_Normalize(chr_reuse_dir_, chr_reuse_dir_);
				}
				MtxFx43 chr_reuse_invMtx = chr.chr_reuse_invMtx;
				MtxFx43 chr_reuse_mtx = chr.chr_reuse_mtx;
				stageMng.getInvWldMtx(chr_reuse_invMtx);
				stageMng.getWldMtx(chr_reuse_mtx);
				MTX_MultVec43(chr_reuse_currentPos_, chr_reuse_invMtx, chr_reuse_currentPos_);
				MTX_MultVec43(chr_reuse_oldPos_, chr_reuse_invMtx, chr_reuse_oldPos_);
				if (isOperater())
				{
					int jumpCollisionRadius = getJumpCollisionRadius();
					if ((wld.CWorldOutSideData.getInstance().MapData().isColFlag() & 0x800) != 0 && (getColFlag() & 0x1000) != 0)
					{
						mcl.CollisionResult chr_reuse_result2 = chr.chr_reuse_result;
						num4 = calculateJumpCollision(ror, chr_reuse_result2, getColFlag(), jumpCollisionRadius, chr_reuse_currentPos_, chr_reuse_oldPos_);
						wld.CWorldOutSideData.getInstance().MapData().MapJumpIndex_set((sbyte)num4);
						if (-1 != num4)
						{
							wld.CWorldOutSideData.getInstance().MapData().MapJumpNormal_set(chr_reuse_result2.normal);
						}
					}
				}
				if ((wld.CWorldOutSideData.getInstance().MapData().isColFlag() & 8) != 0)
				{
					collisionWall(ror, chr_reuse_currentPos_, chr_reuse_oldPos_);
				}
				VecFx32 chr_reuse_edgePos = chr.chr_reuse_edgePos;
				chr_reuse_edgePos.set(0, 0, 0);
				int length = 0;
				switch (CharaKind())
				{
				case CHARACTER_KIND.CHARACTER_KIND_PLAYER_HUMAN:
					length = 81920;
					chr_reuse_edgePos.copy(chr_reuse_currentPos_);
					chr_reuse_edgePos.y += 28672;
					break;
				case CHARACTER_KIND.CHARACTER_KIND_PLAYER_VEHICLE:
					length = 131072;
					chr_reuse_edgePos.copy(chr_reuse_currentPos_);
					chr_reuse_edgePos.y += 28672;
					break;
				}
				if ((wld.CWorldOutSideData.getInstance().MapData().isColFlag() & 4) != 0 && getBottomPolygon(ror, chr_reuse_result, 1, chr_reuse_edgePos, length))
				{
					getGrvAcc_set(0);
					if ((8 & getColFlag()) != 0)
					{
						if (chr_reuse_result.normal.y != 4096)
						{
							VecFx32 chr_reuse_toVert = chr.chr_reuse_toVert;
							VEC_Subtract(chr_reuse_result.v0, chr_reuse_currentPos_, chr_reuse_toVert);
							int a = VEC_DotProduct(chr_reuse_toVert, chr_reuse_result.normal);
							VEC_MultAdd(a, chr_reuse_result.normal, chr_reuse_currentPos_, chr_reuse_currentPos_);
						}
						else
						{
							chr_reuse_currentPos_.y = chr_reuse_result.pos.y;
						}
					}
					num3 = checkLandForm(chr_reuse_result);
					num2 = getMonsterGroupId(chr_reuse_result);
					flag = isSkyMonsterEncount(chr_reuse_result);
				}
				MTX_MultVec43(chr_reuse_currentPos_, chr_reuse_mtx, chr_reuse_currentPos_);
				setPosition(chr_reuse_currentPos_);
				if (-1 != num3)
				{
					m_LandFormIndex = (sbyte)num3;
				}
				if (isOperater())
				{
					wld.CWorldOutSideData.getInstance().MapData().MonsterPartyIndex_set((sbyte)num2);
					monPartyGroupNo_ = num2;
					sAttr_ = flag;
				}
				if (num >= 0)
				{
					wld.CWorldOutSideData.getInstance().MapData().WallIndex_set((sbyte)num);
				}
			}

			public bool calculateGroundCollision(dgs.CRestrictor ror, mcl.CollisionResult result, int attr, int radius, int revise, VecFx32 current_position, VecFx32 old_position)
			{
				int length = 81920;
				VecFx32 vecFx = new VecFx32(current_position);
				vecFx.y += 28672;
				bool result2 = false;
				if (ror.rorEvaluateArrow(vecFx, normDir, length, attr, result))
				{
					result2 = true;
					if (result.normal.y != 4096)
					{
						VecFx32 vecFx2 = new VecFx32(0, 0, 0);
						VEC_Subtract(result.v0, current_position, vecFx2);
						int a = VEC_DotProduct(vecFx2, result.normal);
						VEC_MultAdd(a, result.normal, current_position, current_position);
					}
					else
					{
						current_position.y = result.pos.y;
					}
				}
				return result2;
			}

			public virtual bool collisionWall(dgs.CRestrictor ror, VecFx32 nowPos, VecFx32 prePos)
			{
				if ((getColFlag() & 0x10) == 0)
				{
					return false;
				}
				bool result = false;
				for (int i = 0; i < 5; i++)
				{
					if ((wld.CWorldOutSideData.getInstance().MapData().isColFlag() & MAP_WALL_COLLISION_FLAG[i]) != 0 && (getColFlag() & CHARA_WALL_COLLISION_FLAG[i]) != 0)
					{
						m_colResultWall[i].clean();
						if (calculateWallCollision(ror, m_colResultWall[i], WALL_ATTRIBUTE[i], 12288, 10, nowPos, prePos))
						{
							wld.CWorldOutSideData.getInstance().MapData().WallIndex_set((sbyte)i);
							result = true;
						}
					}
				}
				return result;
			}

			public virtual bool calculateWallCollision(dgs.CRestrictor ror, mcl.CollisionResult result, int attr, int radius, int revise, VecFx32 current_position, VecFx32 old_position)
			{
				bool result2 = false;
				VecFx32 vecFx = new VecFx32(0, 0, 0);
				VEC_Subtract(current_position, old_position, vecFx);
				if (vecFx.x == 0 && vecFx.y == 0 && vecFx.z == 0)
				{
					return false;
				}
				VEC_Normalize(vecFx, vecFx);
				if (ror.rorEvaluateSphere(current_position, vecFx, radius, attr, result))
				{
					if (result.normal.x == 0 && result.normal.y == 0 && result.normal.z == 0)
					{
						result.hit = 0;
						return false;
					}
					VEC_MultAdd(radius - result.length, result.normal, current_position, current_position);
					result2 = true;
				}
				return result2;
			}

			public virtual short calculateJumpCollision(dgs.CRestrictor ror, mcl.CollisionResult result, int attr, int radius, VecFx32 current_position, VecFx32 old_position)
			{
				mcl.CollisionResult chr_reuse_result = chr_reuse_result2;
				short result2 = -1;
				VecFx32 chr_reuse_currentPos_ = chr_reuse_currentPos_2;
				chr_reuse_currentPos_.copy(current_position);
				chr_reuse_currentPos_.y += 20480;
				VecFx32 chr_reuse_dir_ = chr_reuse_dir_2;
				VEC_Subtract(current_position, old_position, chr_reuse_dir_);
				if (chr_reuse_dir_.x != 0 || chr_reuse_dir_.y != 0 || chr_reuse_dir_.z != 0)
				{
					VEC_Normalize(chr_reuse_dir_, chr_reuse_dir_);
				}
				for (byte b = 0; b < 12; b++)
				{
					if ((wld.CWorldOutSideData.getInstance().MapData().isColFlag() & MAP_JUMP_COLLISION_FLAG[b]) != 0 && (attr & (8192 << (int)b)) != 0 && ror.rorEvaluateSphere(chr_reuse_currentPos_, chr_reuse_dir_, radius, (int)attr_jump[b], chr_reuse_result))
					{
						result2 = (short)(b + 1);
						result.copy(chr_reuse_result);
						break;
					}
				}
				return result2;
			}

			public short calculateBottom(dgs.CRestrictor ror, mcl.CollisionResult result_)
			{
				int length = 0;
				VecFx32 vecFx = new VecFx32(0, 0, 0);
				VecFx32 vecFx2 = new VecFx32(getPosition());
				VecFx32 vecFx3 = new VecFx32(getPosition());
				short result = -1;
				vecFx3.y += 20480;
				vecFx2.y -= 102400;
				VEC_Subtract(vecFx2, vecFx3, vecFx);
				if (!IS_ZERO_NORM(vecFx))
				{
					length = VEC_Mag(vecFx);
					VEC_Normalize(vecFx, vecFx);
				}
				MtxFx43 mtxFx = new MtxFx43();
				MtxFx43 mtxFx2 = new MtxFx43();
				stageMng.getInvWldMtx(mtxFx);
				stageMng.getWldMtx(mtxFx2);
				MTX_MultVec43(vecFx2, mtxFx, vecFx2);
				MTX_MultVec43(vecFx3, mtxFx, vecFx3);
				if (ror.rorEvaluateArrow(vecFx3, vecFx, length, 1, result_))
				{
					result = checkLandForm(result_);
				}
				return result;
			}

			public bool getBottomPolygon(dgs.CRestrictor ror, mcl.CollisionResult ret, int attr, VecFx32 pt, int length)
			{
				return ror.rorEvaluateArrow(pt, normDir, length, attr, ret);
			}

			public short checkLandForm(mcl.CollisionResult col_result)
			{
				short result = -1;
				if (col_result.material.getAttribute().isEnableFlag(8u))
				{
					result = 1;
				}
				if (col_result.material.getAttribute().isEnableFlag(9u))
				{
					result = 2;
				}
				if (col_result.material.getAttribute().isEnableFlag(10u))
				{
					result = 3;
				}
				if (col_result.material.getAttribute().isEnableFlag(11u))
				{
					result = 4;
				}
				if (col_result.material.getAttribute().isEnableFlag(12u))
				{
					result = 5;
				}
				if (col_result.material.getAttribute().isEnableFlag(13u))
				{
					result = 6;
				}
				if (col_result.material.getAttribute().isEnableFlag(14u))
				{
					result = 7;
				}
				if (col_result.material.getAttribute().isEnableFlag(15u))
				{
					result = 8;
				}
				if (col_result.material.getAttribute().isEnableFlag(16u))
				{
					result = 9;
				}
				if (col_result.material.getAttribute().isEnableFlag(17u))
				{
					result = 10;
				}
				if (col_result.material.getAttribute().isEnableFlag(18u))
				{
					result = 11;
				}
				if (col_result.material.getAttribute().isEnableFlag(19u))
				{
					result = 12;
				}
				return result;
			}

			public int getMonsterGroupId(mcl.CollisionResult ret)
			{
				if (ret.material.getAttribute().isEnableFlag(20u))
				{
					return 1;
				}
				if (ret.material.getAttribute().isEnableFlag(21u))
				{
					return 2;
				}
				if (ret.material.getAttribute().isEnableFlag(22u))
				{
					return 3;
				}
				if (ret.material.getAttribute().isEnableFlag(23u))
				{
					return 4;
				}
				if (ret.material.getAttribute().isEnableFlag(24u))
				{
					return 5;
				}
				return -1;
			}

			public bool isSkyMonsterEncount(mcl.CollisionResult ret)
			{
				return ret.material.getAttribute().isEnableFlag(42u);
			}

			public void autoPlayStockMotion()
			{
				if (m_StockMotionIndex < 0 || 8 <= m_StockMotionIndex)
				{
					return;
				}
				bool flag = false;
				int num = m_StockMotionIndex;
				if (m_StockMotionIndex == 0)
				{
					flag = true;
				}
				else
				{
					num--;
					if (m_StockMotionParameter[m_StockMotionIndex].m_MotionIndex == -1)
					{
						if (isEndOfMotion())
						{
							return;
						}
					}
					else if (m_StockMotionParameter[num].m_PlayFrame == 0)
					{
						if (isEndOfMotion())
						{
							flag = true;
						}
					}
					else
					{
						m_StockMotionParameter[num].m_PlayFrame--;
						if (m_StockMotionParameter[num].m_PlayFrame <= 0)
						{
							flag = true;
						}
					}
				}
				if (!flag)
				{
					return;
				}
				if (m_StockMotionIndex >= 8)
				{
					m_StockMotionIndex = 8;
					return;
				}
				if (m_StockMotionParameter[num].m_MotionIndex == -1)
				{
					m_StockMotionIndex = 8;
					return;
				}
				if (m_StockMotionParameter[num].m_MotionIndex != 0)
				{
					startMotion(m_StockMotionParameter[m_StockMotionIndex].m_MotionIndex, m_StockMotionParameter[m_StockMotionIndex].m_Loop, (uint)m_StockMotionParameter[m_StockMotionIndex].m_MotionBlend);
				}
				m_StockMotionIndex++;
			}

			public void autoLookTarget()
			{
				if ((m_TargetLookFlag || m_TargetLookFrame-- > 0) && getTarget() != null)
				{
					VecFx32 b = new VecFx32(getPosition());
					VecFx32 vecFx = new VecFx32(getTarget().getPosition());
					VEC_Subtract(vecFx, b, vecFx);
					if (!IS_ZERO_NORM(vecFx))
					{
						VEC_Normalize(vecFx, vecFx);
					}
					setTargetDirection(vecFx);
				}
			}

			public void autoScale()
			{
				if (m_AutoScaleFrame != -1)
				{
					m_AutoScaleFrame--;
					if (m_AutoScaleFrame < 0)
					{
						m_AutoScaleFrame = -1;
					}
					VecFx32 vecFx = new VecFx32(getScale());
					VEC_Add(vecFx, m_AutoScaleSpeed, vecFx);
					setScale(vecFx);
				}
			}

			public void autoAlpha()
			{
				if (-1 == m_AutoAlphaFrame)
				{
					return;
				}
				if (getTransparencyRate() < m_SucAlpha)
				{
					m_WorkAutoAlphaFrame++;
				}
				else
				{
					m_WorkAutoAlphaFrame--;
				}
				if (m_WorkAutoAlphaFrame <= 0 || m_WorkAutoAlphaFrame >= m_AutoAlphaFrame)
				{
					setTransparencyRate(m_SucAlpha);
					m_SucAlpha = -1;
					m_WorkAutoAlphaFrame = -1;
					m_AutoAlphaFrame = -1;
					return;
				}
				int denom = m_AutoAlphaFrame << 12;
				int numer = m_WorkAutoAlphaFrame << 12;
				int num = m_SucAlpha << 12;
				int num2 = FX_Mul(409600, FX_Div(numer, denom));
				if (getTransparencyRate() < m_SucAlpha)
				{
					if (num2 > num)
					{
						num2 = num;
					}
				}
				else if (num2 < num)
				{
					num2 = num;
				}
				setTransparencyRate(num2 >> 12);
			}

			public void autoShadowAlpha()
			{
				if (m_AutoShadowAlphaFrame == -1)
				{
					return;
				}
				if (getShadowAlpha() < m_SucShadowAlpha)
				{
					m_WorkAutoShadowAlphaFrame++;
				}
				else
				{
					m_WorkAutoShadowAlphaFrame--;
				}
				int numer = m_AutoShadowAlphaFrame << 12;
				int denom = m_WorkAutoShadowAlphaFrame << 12;
				int numer2 = m_SucShadowAlpha << 12;
				numer2 = FX_Div(numer2, FX_Div(numer, denom));
				if (getShadowAlpha() < m_SucShadowAlpha)
				{
					if (numer2 > m_SucShadowAlpha << 12)
					{
						numer2 = m_SucShadowAlpha << 12;
					}
				}
				else if (numer2 < m_SucShadowAlpha << 12)
				{
					numer2 = m_SucShadowAlpha << 12;
				}
				setShadowAlpha(numer2 >> 12);
				if (m_WorkAutoShadowAlphaFrame <= 0 || m_WorkAutoShadowAlphaFrame >= m_AutoShadowAlphaFrame)
				{
					setShadowAlpha(m_SucShadowAlpha);
					m_SucShadowAlpha = -1;
					m_WorkAutoShadowAlphaFrame = -1;
					m_AutoShadowAlphaFrame = -1;
				}
			}

			public void autoDelete()
			{
				if (m_AutoDeleteFrame != -1)
				{
					m_WorkAutoDeleteFrame--;
					int numer = m_AutoDeleteFrame << 12;
					int denom = m_WorkAutoDeleteFrame << 12;
					int num = 409600;
					num = ((num >= 0) ? FX_Div(num, FX_Div(numer, denom)) : 0);
					setTransparencyRate(num >> 12);
					if (m_WorkAutoDeleteFrame <= 0)
					{
						m_WorkAutoDeleteFrame = -1;
						m_AutoDeleteFrame = -1;
						deleteCharacter();
					}
				}
			}

			public void deleteCharacter()
			{
				if (getCharacterId() >= 0)
				{
					setMCLCol(b: false);
					setScriptWallCollision(b: false);
					getColFlag_set(0);
					getColType_set(0);
					setGrv(_GrvFlag: false);
					characterMng.delCharacter(getCharacterId());
					setCharacterId(-1);
				}
			}

			public void setPreAct(int _PreAct)
			{
				m_PreAct = _PreAct;
			}

			public void setNowAct(int _NowAct)
			{
				m_NowAct = _NowAct;
			}

			public void setNextAct(int _NextAct)
			{
				setPreAct(m_NowAct);
				m_NextAct = _NextAct;
			}

			public void setOperater(bool _Operater)
			{
				m_Operater = _Operater;
			}

			public void setTarget(CCharacterEureka _Target)
			{
				m_Target = _Target;
			}

			public CCharacterEureka()
			{
				m_ItemEvent = false;
				m_Balloon = false;
				m_AutoRun = false;
				m_Operater = false;
				m_InvalidActionMotion = false;
				m_CharaKind = CHARACTER_KIND.CHARACTER_KIND_ERR;
				m_NowAct = 0;
				m_NextAct = 0;
				m_Target = null;
				for (int i = 0; i < m_StockMotionParameter.Length; i++)
				{
					m_StockMotionParameter[i] = new SStockMotionParameter();
				}
				for (int i = 0; i < m_colResultWall.Length; i++)
				{
					m_colResultWall[i] = new mcl.CollisionResult();
				}
			}

			public void setItemEvent(bool _ItemEvent)
			{
				m_ItemEvent = _ItemEvent;
			}

			public bool isItemEvent()
			{
				return m_ItemEvent;
			}

			public void setBalloon(bool _Balloon)
			{
				m_Balloon = _Balloon;
			}

			public bool isBalloon()
			{
				return m_Balloon;
			}

			public void setAutoRun(bool _AutoRun)
			{
				m_AutoRun = _AutoRun;
			}

			public bool AutoRun()
			{
				return m_AutoRun;
			}

			public void AutoRun_set(bool arg0)
			{
				m_AutoRun = arg0;
			}

			public bool isOperater()
			{
				return m_Operater;
			}

			public bool isMCLCol()
			{
				return redActivity();
			}

			public void setInvalidActionMotion(bool m_InvalidActionMotion)
			{
				this.m_InvalidActionMotion = m_InvalidActionMotion;
			}

			public bool getInvalidActionMotion()
			{
				return m_InvalidActionMotion;
			}

			public void setTargetLookFlag(bool _TargetLookFlag)
			{
				m_TargetLookFlag = _TargetLookFlag;
			}

			public void setTargetLookFrame(int _TargetLookFrame)
			{
				m_TargetLookFrame = _TargetLookFrame;
			}

			public sbyte getLandFormIndex()
			{
				return m_LandFormIndex;
			}

			public uint LogicIndex()
			{
				return m_LogicIndex;
			}

			public void LogicIndex_set(uint arg0)
			{
				m_LogicIndex = arg0;
			}

			public void setStockMotionIndex(int _StockMotionIndex)
			{
				m_StockMotionIndex = _StockMotionIndex;
			}

			public int getStockMotionIndex()
			{
				return m_StockMotionIndex;
			}

			public void setStockMotionParameter(int _Index, int _MotionIndex, int _PlayFrame, bool _Loop, int _MotionBlend)
			{
				m_StockMotionParameter[_Index].setup(_MotionIndex, _PlayFrame, _Loop, _MotionBlend);
			}

			public void resetStockMotionParameter(int _Index)
			{
				m_StockMotionParameter[_Index].initialize();
			}

			public void setAutoScaleSpeed(VecFx32 _AutoScaleSpeed)
			{
				m_AutoScaleSpeed.copy(_AutoScaleSpeed);
			}

			public VecFx32 AutoScaleSpeed()
			{
				return m_AutoScaleSpeed;
			}

			public void setAutoScaleFrame(int _AutoScaleFrame)
			{
				m_AutoScaleFrame = _AutoScaleFrame;
			}

			public int AutoScaleFrame()
			{
				return m_AutoScaleFrame;
			}

			public void setSucAlpha(int _SucAlpha)
			{
				m_SucAlpha = _SucAlpha;
			}

			public int SucAlpha()
			{
				return m_SucAlpha;
			}

			public void setAutoAlphaFrame(int _AutoAlphaFrame)
			{
				m_AutoAlphaFrame = _AutoAlphaFrame;
			}

			public int AutoAlphaFrame()
			{
				return m_AutoAlphaFrame;
			}

			public void setWorkAutoAlphaFrame(int _WorkAutoAlphaFrame)
			{
				m_WorkAutoAlphaFrame = _WorkAutoAlphaFrame;
			}

			public int WorkAutoAlphaFrame()
			{
				return m_WorkAutoAlphaFrame;
			}

			public void setSucShadowAlpha(int _SucShadowAlpha)
			{
				m_SucShadowAlpha = _SucShadowAlpha;
			}

			public int SucShadowAlpha()
			{
				return m_SucShadowAlpha;
			}

			public void setAutoShadowAlphaFrame(int _AutoShadowAlphaFrame)
			{
				m_AutoShadowAlphaFrame = _AutoShadowAlphaFrame;
			}

			public int AutoShadowAlphaFrame()
			{
				return m_AutoShadowAlphaFrame;
			}

			public void setWorkAutoShadowAlphaFrame(int _WorkAutoShadowAlphaFrame)
			{
				m_WorkAutoShadowAlphaFrame = _WorkAutoShadowAlphaFrame;
			}

			public int WorkAutoShadowAlphaFrame()
			{
				return m_WorkAutoShadowAlphaFrame;
			}

			public void setAutoDeleteFrame(int _AutoDeleteFrame)
			{
				m_WorkAutoDeleteFrame = (m_AutoDeleteFrame = _AutoDeleteFrame);
			}

			public int AutoDeleteFrame()
			{
				return m_AutoDeleteFrame;
			}

			public void setCharaKind(CHARACTER_KIND _CharaKind)
			{
				m_CharaKind = _CharaKind;
			}

			public CHARACTER_KIND CharaKind()
			{
				return m_CharaKind;
			}

			public void CharaKind_set(CHARACTER_KIND arg0)
			{
				m_CharaKind = arg0;
			}

			public void setCharaCheckType(CHARACTER_CHECK_TYPE _CharaCheckType)
			{
				m_CharaCheckType = _CharaCheckType;
			}

			public CHARACTER_CHECK_TYPE CharaCheckType()
			{
				return m_CharaCheckType;
			}

			public void CharaCheckType_set(CHARACTER_CHECK_TYPE arg0)
			{
				m_CharaCheckType = arg0;
			}

			public CCharacterMoveSys MoveSys()
			{
				return m_MoveSys;
			}

			public CCharacterTurnSys TurnSys()
			{
				return m_TurnSys;
			}

			public int getPreAct()
			{
				return m_PreAct;
			}

			public int getNowAct()
			{
				return m_NowAct;
			}

			public int getNextAct()
			{
				return m_NextAct;
			}

			public CCharacterEureka getTarget()
			{
				return m_Target;
			}

			public mcl.CollisionResult getColResultWall(int wallNo)
			{
				return m_colResultWall[wallNo];
			}

			protected virtual int getJumpCollisionRadius()
			{
				return 6144;
			}
		}
	}
}
