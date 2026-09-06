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
		public class VehicleCanoe : CPlayerVehicle
		{
			private class MapMarkerAccepterVehicleCanoe : MapMarkerAccepter
			{
				private VehicleCanoe m_Owner;

				public MapMarkerAccepterVehicleCanoe(VehicleCanoe owner)
				{
					m_Owner = owner;
				}

				public override bool acceptVisibility()
				{
					return m_Owner.visibility_;
				}
			}

			public bool visibility_;

			public VehicleCanoe()
			{
				composit2 = new MapMarkerAccepterVehicleCanoe(this);
				visibility_ = false;
			}

			public override void initialize()
			{
				base.initialize();
				_actionList[0] = (act.CBaseAction)ds.CHeap.alloc_app(typeof(CanoeActionWait));
				_actionList[1] = (act.CBaseAction)ds.CHeap.alloc_app(typeof(CanoeActionNavigate));
				_actionList[4] = (act.CBaseAction)ds.CHeap.alloc_app(typeof(CanoeActionAppear));
				_actionList[5] = (act.CBaseAction)ds.CHeap.alloc_app(typeof(CanoeActionDisappear));
				registerAction(ACTION_ID.ACTION_ID_WAIT, _actionList[0]);
				registerAction(ACTION_ID.ACTION_ID_NAVIGATE, _actionList[1]);
				registerAction(ACTION_ID.ACTION_ID_APPEAR, _actionList[4]);
				registerAction(ACTION_ID.ACTION_ID_DISAPPEAR, _actionList[5]);
			}

			public override void into()
			{
				base.into();
				characterMng.removeAllMotion(getCharacterId());
				characterMng.addMotion(getCharacterId(), "w_act_n461");
				characterMng.startMotion(getCharacterId(), 1001, fLoop: true, 5u);
				characterMng.setMotionSpeed(getCharacterId(), 4096);
				setGrv(_GrvFlag: false);
				getColFlag_set(0);
				getColFlag_or(2);
				getColFlag_or(4);
				getColFlag_or(16);
				getColFlag_or(128);
				getColFlag_or(256);
				getColFlag_or(512);
				setTransparency(0);
				setShadowType(2u);
			}

			public override void terminate()
			{
				base.terminate();
			}

			public override void execute()
			{
				base.execute();
			}

			public override void update()
			{
				base.update();
			}

			public override void setConditionOfAir()
			{
				getPosition().y = 0;
				setPreAct(0);
				setNowAct(0);
				setNextAct(0);
				setTransparency(31);
				CPlayerCharacter boardPlayer = getBoardPlayer();
				boardPlayer.setNextAct(9);
			}

			public bool checkNextActionToDisappear()
			{
				sbyte landFormIndex = getLandFormIndex();
				if (4 != landFormIndex)
				{
					return true;
				}
				return false;
			}

			public override bool canEncount()
			{
				return 1 == getNowAct();
			}

			public override int getBattleMapNo()
			{
				return 4;
			}

			public override bool collisionWall(dgs.CRestrictor ror, VecFx32 nowPos, VecFx32 prePos)
			{
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
				int num2 = 40960;
				nowPos.y += num2;
				prePos.y += num2;
				VecFx32 vecFx = new VecFx32(0, 0, 0);
				VEC_Subtract(nowPos, prePos, vecFx);
				VEC_Mag(vecFx);
				VEC_Normalize(vecFx, vecFx);
				mcl.CollisionResult pl_reuse_result = pl.pl_reuse_result;
				pl_reuse_result.clean();
				bool result = false;
				int num3 = 12288;
				if (ror.rorEvaluateSphere2(nowPos, prePos, vecFx, num3, array, b, pl_reuse_result))
				{
					result = true;
					VEC_MultAdd(num3 + 128 - pl_reuse_result.length, pl_reuse_result.normal, nowPos, nowPos);
					if (ror.rorEvaluateSphere2(nowPos, prePos, vecFx, num3, array, b, pl_reuse_result))
					{
						VEC_MultAdd(num3 + 128 - pl_reuse_result.length, pl_reuse_result.normal, nowPos, nowPos);
					}
				}
				nowPos.y -= num2;
				prePos.y -= num2;
				return result;
			}

			public override bool calculateWallCollision(dgs.CRestrictor ror, mcl.CollisionResult result, int attr, int radius, int revise, VecFx32 current_position, VecFx32 old_position)
			{
				int num = 40960;
				current_position.y += num;
				old_position.y += num;
				bool result2 = base.calculateWallCollision(ror, result, attr, radius, revise, current_position, old_position);
				current_position.y -= num;
				old_position.y -= num;
				return result2;
			}

			public override void playNaviSE()
			{
				if (canPlayNaviSE())
				{
					hNaviSE_ = MatrixSound.MtxSENDS_Play(1, 6, 192, 127);
				}
			}

			public override byte getStartActionID()
			{
				return 4;
			}
		}
	}
}
