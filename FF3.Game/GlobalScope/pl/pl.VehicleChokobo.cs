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
	public static partial class pl
	{
		public class VehicleChokobo : CPlayerVehicle
		{
			private bool running_;

			public override void initialize()
			{
				base.initialize();
				_actionList[0] = (act.CBaseAction)ds.CHeap.alloc_app(typeof(ChokoboActionWait));
				_actionList[1] = (act.CBaseAction)ds.CHeap.alloc_app(typeof(ChokoboActionNavigate));
				_actionList[5] = (act.CBaseAction)ds.CHeap.alloc_app(typeof(ChokoboActionDisappear));
				registerAction(ACTION_ID.ACTION_ID_WAIT, _actionList[0]);
				registerAction(ACTION_ID.ACTION_ID_NAVIGATE, _actionList[1]);
				registerAction(ACTION_ID.ACTION_ID_DISAPPEAR, _actionList[5]);
			}

			public override void into()
			{
				base.into();
				int characterId = getCharacterId();
				characterMng.removeAllMotion(characterId);
				characterMng.addMotion(characterId, "w_act_n441");
				characterMng.startMotion(characterId, CHOKOBO_MOTIONNO_WAIT, fLoop: true, 5u);
				characterMng.setMotionSpeed(characterId, 4096);
				setMCLCol(b: true);
				getColFlag_set(0);
				getColFlag_or(2);
				getColFlag_or(4);
				getColFlag_or(16);
				getColFlag_or(64);
				getColFlag_or(128);
				getColFlag_or(256);
				getColFlag_or(512);
				setGrv(_GrvFlag: false);
			}

			public override void update()
			{
				base.update();
			}

			public bool checkToRun()
			{
				if ((dv.CDeviceManager.getInstance().Pad().pad_trs(0) & (uint)CHOKOBO_KEY_RUN) != 0)
				{
					return true;
				}
				return false;
			}

			public bool checkToWalk()
			{
				if ((dv.CDeviceManager.getInstance().Pad().pad_trs(0) & (uint)CHOKOBO_KEY_RUN) == 0)
				{
					return true;
				}
				return false;
			}

			public bool checkNextActionToDisappear()
			{
				if ((dv.CDeviceManager.getInstance().Pad().edge_trs(0) & 0x20) != 0 || (_tpFlag & 2) != 0)
				{
					short landFormIndex = getLandFormIndex();
					if (1 == landFormIndex || 2 == landFormIndex || 3 == landFormIndex || 6 == landFormIndex || 7 == landFormIndex || 9 == landFormIndex)
					{
						return true;
					}
				}
				return false;
			}

			public override bool collisionWall(dgs.CRestrictor ror, VecFx32 nowPos, VecFx32 prePos)
			{
				VecFx32 vecFx = new VecFx32();
				VecFx32 vecFx2 = new VecFx32();
				bool result = false;
				if ((getColFlag() & 0x10) == 0)
				{
					return false;
				}
				byte b = 5;
				int[] array = new int[b];
				for (byte b2 = 0; b2 < b; b2++)
				{
					array[b2] = 0;
					int colFlag = getColFlag();
					int num = 32 << (int)b2;
					if ((colFlag & num) != 0)
					{
						array[b2] = 2 + b2;
					}
				}
				bool flag = false;
				if (strncmp(sceneMng.getStage(), "f03", 3) == 0)
				{
					int[] array2 = new int[2] { 4677373, 3426856 };
					int num2 = 32768;
					VecFx32 vecFx3 = new VecFx32();
					MtxFx43 mtxFx = new MtxFx43();
					stageMng.getWldMtx(mtxFx);
					MTX_MultVec43(nowPos, mtxFx, vecFx3);
					for (int i = 0; i < array2.Length; i += 2)
					{
						int num3 = vecFx3.x - array2[i];
						int num4 = vecFx3.z - array2[i + 1];
						if (num3 > -num2 && num3 < num2 && num4 > -num2 && num4 < num2)
						{
							flag = true;
						}
					}
				}
				int num5 = 40960;
				nowPos.y += num5;
				prePos.y += num5;
				mcl.CollisionResult pl_reuse_result = pl.pl_reuse_result;
				pl_reuse_result.clean();
				VecFx32 vecFx4 = new VecFx32(0, 0, 0);
				VecFx32 vecFx5 = new VecFx32(0, 0, 0);
				int num6 = 0;
				VEC_Subtract(nowPos, prePos, vecFx4);
				num6 = VEC_Mag(vecFx4);
				VEC_Normalize(vecFx4, vecFx5);
				int num7 = 8192;
				if (ror.rorEvaluateSphere2(nowPos, prePos, vecFx5, num7, array, b, pl_reuse_result))
				{
					VEC_MultAdd(num7 + 128 - pl_reuse_result.length, pl_reuse_result.normal, nowPos, nowPos);
					if (ror.rorEvaluateSphere2(nowPos, prePos, vecFx5, num7, array, b, pl_reuse_result))
					{
						VEC_MultAdd(num7 + 128 - pl_reuse_result.length, pl_reuse_result.normal, nowPos, nowPos);
						if (flag && ror.rorEvaluateSphere2(nowPos, prePos, vecFx5, num7, array, b, pl_reuse_result))
						{
							VEC_MultAdd(num7 + 128 - pl_reuse_result.length, pl_reuse_result.normal, nowPos, nowPos);
						}
					}
				}
				else
				{
					vecFx.copy(prePos);
					vecFx2.copy(nowPos);
					result = false;
					int num8 = num7 + 128;
					if (ror.rorEvaluateArrow2(vecFx, vecFx5, num6, array, b, pl_reuse_result))
					{
						result = true;
						int num9 = -VEC_DotProduct(pl_reuse_result.normal, vecFx4);
						VecFx32 vecFx6 = new VecFx32(0, 0, 0);
						int num10 = 0;
						VEC_Subtract(pl_reuse_result.v0, vecFx, vecFx6);
						num10 = -VEC_DotProduct(pl_reuse_result.normal, vecFx6);
						int a = num9 - num10;
						VEC_MultAdd(a, pl_reuse_result.normal, vecFx2, vecFx2);
						VEC_MultAdd(-num8, vecFx5, vecFx2, vecFx2);
						VEC_MultAdd(-num8, vecFx5, pl_reuse_result.pos, vecFx);
						VEC_Subtract(vecFx2, vecFx, vecFx4);
						num6 = VEC_Mag(vecFx4);
						VEC_Normalize(vecFx4, vecFx5);
						if (ror.rorEvaluateArrow2(vecFx, vecFx5, num6, array, b, pl_reuse_result))
						{
							VEC_MultAdd(num8, pl_reuse_result.normal, pl_reuse_result.pos, vecFx2);
						}
					}
					nowPos.copy(vecFx2);
				}
				nowPos.y -= num5;
				prePos.y -= num5;
				return result;
			}

			public override bool calculateWallCollision(dgs.CRestrictor ror, mcl.CollisionResult result, int attr, int radius, int revise, VecFx32 current_position, VecFx32 old_position)
			{
				int num = 40960;
				VecFx32 vecFx = new VecFx32(0, 0, 0);
				VEC_Subtract(current_position, old_position, vecFx);
				VEC_Normalize(vecFx, vecFx);
				VEC_MultAdd(num, vecFx, current_position, current_position);
				int num2 = 40960;
				current_position.y += num2;
				old_position.y += num2;
				bool result2 = base.calculateWallCollision(ror, result, attr, radius, revise, current_position, old_position);
				VEC_MultAdd(-num, vecFx, current_position, current_position);
				current_position.y -= num2;
				old_position.y -= num2;
				return result2;
			}

			public override void actionVehicleNavigate()
			{
				if (running_)
				{
					setMass((int)(CPlayerWorldParameterManager.Instance().PlayerWorldMoveParameter((int)PlayerMoveType()).SMveAcc() * 1.6f), 0, (int)(CPlayerWorldParameterManager.Instance().PlayerWorldMoveParameter((int)PlayerMoveType()).SMveMax() * 1.6f), (int)(CPlayerWorldParameterManager.Instance().PlayerWorldMoveParameter((int)PlayerMoveType()).STrnAcc() * 1.6f), 0, (int)(CPlayerWorldParameterManager.Instance().PlayerWorldMoveParameter((int)PlayerMoveType()).STrnMax() * 1.6f));
				}
				else
				{
					setMass((int)((int)CPlayerWorldParameterManager.Instance().PlayerWorldMoveParameter((int)PlayerMoveType()).NMveAcc() * (60 / chr.CCharacterEureka.m_CharaFps)), 0, (int)((int)CPlayerWorldParameterManager.Instance().PlayerWorldMoveParameter((int)PlayerMoveType()).NMveMax() * (60 / chr.CCharacterEureka.m_CharaFps)), (int)((int)CPlayerWorldParameterManager.Instance().PlayerWorldMoveParameter((int)PlayerMoveType()).NTrnAcc() * (60 / chr.CCharacterEureka.m_CharaFps)), 0, (int)((int)CPlayerWorldParameterManager.Instance().PlayerWorldMoveParameter((int)PlayerMoveType()).NTrnMax() * (60 / chr.CCharacterEureka.m_CharaFps)));
				}
			}

			public override byte getStartActionID()
			{
				return 0;
			}

			public bool isRunning()
			{
				return running_;
			}

			public void setRunning(bool running)
			{
				running_ = running;
			}
		}
	}
}
