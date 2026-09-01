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
	public static partial class btl
	{
		public class CBattleDisplay
		{
			public const int OPENING_CAMERA_MOVE_FRAME1_1 = 70;

			public const int OPENING_CAMERA_MOVE_FRAME1_2 = 0;

			public const int OPENING_CAMERA_MOVE_FRAME1_3 = 0;

			public const int OPENING_CAMERA_MOVE_FRAME2_1 = 70;

			public const int OPENING_CAMERA_MOVE_FRAME2_2 = 0;

			public const int OPENING_CAMERA_MOVE_FRAME2_3 = 0;

			public const int ENDING_CAMERA_MOVE_FRAME = 100;

			public const int OPENING_CAMERA_TYPE_MAX = 2;

			public const int OPENING_CAMERA_STEP_MAX = 3;

			public const int MOVE_CAMERA_FRAME = 18;

			public const int RETURN_CAMERA_FRAME = 20;

			public const int DEFAULT_MONSTER_ID = -1;

			private ds.sys3d.CCamera btlCamera = new ds.sys3d.CCamera();

			private ds.sys3d.Scene m_Scene1st = new ds.sys3d.Scene();

			private ds.sys3d.Scene m_Scene2nd = new ds.sys3d.Scene();

			private int cameraState_;

			private int moveFrame_;

			private int openingCameraStep_;

			private int openingCameraType_;

			private short monsterOffsetId_;

			private int shakeFrame_;

			private VecFx32 rootPosition_ = new VecFx32();

			private VecFx32 shakeWidth_ = new VecFx32();

			private int openingState_;

			public void initialize()
			{
				btlCamera.initialize();
				btlCamera.setFOV(852, 4006);
				btlCamera.setClip(40960, 8192000);
				m_Scene1st.initialize();
				m_Scene1st.setCamera(btlCamera);
				m_Scene2nd.initialize();
				m_Scene2nd.setCamera(btlCamera);
				characterMng.initialize(m_Scene1st, m_Scene2nd);
				characterMng.setFrameRate(4096);
				stageMng.initialize(m_Scene1st);
				entryStage();
				shakeFrame_ = -1;
				openingCameraStep_ = 0;
				openingCameraType_ = 0;
				setMonsterOffsetId(-1);
				openingState_ = 0;
				stateBattleCamera();
				BattlePerformer.getInstance().initialize();
			}

			public void terminate()
			{
				btlCamera.setFOV(214, 4090);
				stageMng.delStage();
				characterMng.terminate();
				stageMng.terminate();
			}

			public void execute()
			{
				if (cameraState_ != (int)OutsideToBattle.getInstance().battleCamera())
				{
					stateBattleCamera();
				}
				if (cameraState_ == 1)
				{
					switch (openingCameraStep_)
					{
					case 0:
					case 1:
						goOpeningCamera();
						break;
					case 2:
						endOpeningCamera();
						break;
					}
				}
				if (cameraState_ == 4)
				{
					moveFrame_--;
					moveCamera();
				}
				if (cameraState_ == 5)
				{
					moveFrame_--;
					returnCamera();
				}
				if (cameraState_ == 6)
				{
					goEndingCamera();
				}
				doShakeCamera();
				btlCamera.execute();
			}

			public void update()
			{
				characterMng.execute();
				stageMng.execute();
			}

			public void draw1st()
			{
				m_Scene1st.draw(bVBlank: false);
			}

			public void draw2nd()
			{
				m_Scene2nd.draw(bVBlank: true);
			}

			public void stateBattleCamera()
			{
				cameraState_ = (int)OutsideToBattle.getInstance().battleCamera();
				if (cameraState_ == 0)
				{
					btlCamera.setMoveMode(0);
					setBattleCameraPositionAndTarget();
					return;
				}
				btlCamera.setMoveMode(1);
				if (cameraState_ == 1)
				{
					readyOpeningCamera();
				}
				else if (cameraState_ == 2)
				{
					btlCamera.setPosition(CameraOpeningPosition);
					btlCamera.setTarget(CameraOpeningTarget);
					btlCamera.setAngle(CameraOpeningAngle.x, CameraOpeningAngle.y, CameraOpeningAngle.z);
					btlCamera.setDistance(CameraOpeningDistance);
				}
				else if (cameraState_ == 3)
				{
					btlCamera.setPosition(CameraBattlePosition);
					btlCamera.setTarget(CameraBattleTarget);
					btlCamera.setAngle(CameraBattleAngle.x, CameraBattleAngle.y, CameraBattleAngle.z);
					btlCamera.setDistance(CameraBattleDistance);
				}
			}

			public void moveCamera()
			{
				btlCamera.setPosition(calcCamera(btlCamera.getPosition(), CameraBattlePosition, CameraOpeningPosition, 18));
				btlCamera.setTarget(calcCamera(btlCamera.getTarget(), CameraBattleTarget, CameraOpeningTarget, 18));
				if (moveFrame_ == 0)
				{
					OutsideToBattle.getInstance().setBattleCamera(BATTLE_CAMERA.MAIN_CAMERA);
				}
			}

			public void returnCamera()
			{
				btlCamera.setPosition(calcCamera(btlCamera.getPosition(), CameraOpeningPosition, CameraBattlePosition, 20));
				btlCamera.setTarget(calcCamera(btlCamera.getTarget(), CameraOpeningTarget, CameraBattleTarget, 20));
				if (moveFrame_ == 0)
				{
					OutsideToBattle.getInstance().setBattleCamera(BATTLE_CAMERA.COMMAND_CAMERA);
				}
			}

			public void setAbilityCamera(pl.Player player)
			{
				byte b = player.formationType();
				byte b2 = (byte)pl.PlayerParty.instance().playerOrder(player.playerId());
				VecFx32 position = AbilityCameraPosition[b][b2];
				VecFx32 target = AbilityCameraTarget[b][b2];
				VecFx32 vecFx = AbilityCameraAngle[b][b2];
				int distance = AbilityCameraDistance[b][b2];
				btlCamera.setPosition(position);
				btlCamera.setTarget(target);
				btlCamera.setAngle(vecFx.x, vecFx.y, vecFx.z);
				btlCamera.setDistance(distance);
			}

			public VecFx32 calcCamera(VecFx32 pos, VecFx32 root, VecFx32 sub, int frame)
			{
				VecFx32 fnd_reuse_pos = GlobalScope.fnd_reuse_pos;
				VEC_Subtract(root, sub, fnd_reuse_pos);
				if (frame == 0)
				{
					return fnd_reuse_pos;
				}
				pos.x += fnd_reuse_pos.x / frame;
				pos.y += fnd_reuse_pos.y / frame;
				pos.z += fnd_reuse_pos.z / frame;
				return pos;
			}

			public int calcCameraDistance(int root, int sub, int frame)
			{
				int distance = btlCamera.getDistance();
				int num = root - sub;
				distance += num / frame;
				btlCamera.setDistance(0);
				btlCamera.addDistance(distance);
				return distance;
			}

			public void setBattleCameraPositionAndTarget()
			{
				btlCamera.setPosition(CameraBattlePosition);
				btlCamera.setTarget(CameraBattleTarget);
				btlCamera.setAngle(CameraBattleAngle.x, CameraBattleAngle.y, CameraBattleAngle.z);
				btlCamera.setDistance(65536);
			}

			public void setCommandCameraPositionAndTarget()
			{
				VecFx32 fnd_reuse_pos = GlobalScope.fnd_reuse_pos;
				VecFx32 fnd_reuse_pos2 = GlobalScope.fnd_reuse_pos2;
				fnd_reuse_pos.x = CameraOpeningPosition.x;
				fnd_reuse_pos.y = CameraOpeningPosition.y;
				fnd_reuse_pos.z = CameraOpeningPosition.z;
				fnd_reuse_pos2.x = CameraOpeningTarget.x;
				fnd_reuse_pos2.y = CameraOpeningTarget.y;
				fnd_reuse_pos2.z = CameraOpeningTarget.z;
				btlCamera.setPosition(fnd_reuse_pos);
				btlCamera.setTarget(fnd_reuse_pos2);
				btlCamera.setAngle(CameraOpeningAngle.x, CameraOpeningAngle.y, CameraOpeningAngle.z);
				btlCamera.setDistance(CameraOpeningDistance);
			}

			public void selectOpeningBCameraType()
			{
				if (OutsideToBattle.getInstance().battleType() == BATTLE_TYPE.NORMAL_BATTLE)
				{
					if (monsterOffsetId_ > -1)
					{
						openingCameraType_ = 0;
					}
					else
					{
						openingCameraType_ = (int)ds.RandomNumber.rand32(2u);
					}
				}
				else
				{
					openingCameraType_ = 0;
				}
			}

			public void readyOpeningCamera()
			{
				selectOpeningBCameraType();
				openingCameraStep_ = 0;
				VecFx32 position = OpeningStartCameraPosition[openingCameraType_][openingCameraStep_];
				VecFx32 target = OpeningStartCameraTarget[openingCameraType_][openingCameraStep_];
				if (openingCameraType_ == 0 && monsterOffsetId_ != -1 && mon.MonsterManager.instance().offset(monsterOffsetId_) != null)
				{
					position = mon.MonsterManager.instance().offset(monsterOffsetId_).startCameraPosition();
					target = mon.MonsterManager.instance().offset(monsterOffsetId_).startCameraTarget();
				}
				btlCamera.setPosition(position);
				btlCamera.setTarget(target);
				moveFrame_ = OpeningStartCameraFrame[openingCameraType_][openingCameraStep_];
			}

			public void goOpeningCamera()
			{
				if (moveFrame_ <= 0)
				{
					if (++openingCameraStep_ == 2)
					{
						moveFrame_ = 3;
						return;
					}
					moveFrame_ = OpeningStartCameraFrame[openingCameraType_][openingCameraStep_];
				}
				moveFrame_--;
				VecFx32 root = OpeningStartCameraPosition[openingCameraType_][openingCameraStep_ + 1];
				VecFx32 sub = OpeningStartCameraPosition[openingCameraType_][openingCameraStep_];
				VecFx32 root2 = OpeningStartCameraTarget[openingCameraType_][openingCameraStep_ + 1];
				VecFx32 sub2 = OpeningStartCameraTarget[openingCameraType_][openingCameraStep_];
				if (openingCameraType_ == 0 && monsterOffsetId_ != -1 && mon.MonsterManager.instance().offset(monsterOffsetId_) != null)
				{
					switch (openingCameraStep_)
					{
					case 0:
						root = mon.MonsterManager.instance().offset(monsterOffsetId_).finishCameraPosition();
						sub = mon.MonsterManager.instance().offset(monsterOffsetId_).startCameraPosition();
						root2 = mon.MonsterManager.instance().offset(monsterOffsetId_).finishCameraTarget();
						sub2 = mon.MonsterManager.instance().offset(monsterOffsetId_).startCameraTarget();
						break;
					case 1:
						root = OpeningStartCameraPosition[openingCameraType_][openingCameraStep_ + 1];
						sub = mon.MonsterManager.instance().offset(monsterOffsetId_).finishCameraPosition();
						root2 = OpeningStartCameraTarget[openingCameraType_][openingCameraStep_ + 1];
						sub2 = mon.MonsterManager.instance().offset(monsterOffsetId_).finishCameraTarget();
						break;
					}
				}
				btlCamera.setPosition(calcCamera(btlCamera.getPosition(), root, sub, OpeningStartCameraFrame[openingCameraType_][openingCameraStep_]));
				btlCamera.setTarget(calcCamera(btlCamera.getTarget(), root2, sub2, OpeningStartCameraFrame[openingCameraType_][openingCameraStep_]));
				if (openingCameraType_ == 0 && openingCameraStep_ == 0 && moveFrame_ == OpeningStartCameraFrame[openingCameraType_][openingCameraStep_] - 61)
				{
					dgs.CFade.Main().fadeOut(10, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
				}
				if (openingCameraType_ == 1 && openingCameraStep_ == 0 && moveFrame_ == OpeningStartCameraFrame[openingCameraType_][openingCameraStep_] - 61)
				{
					dgs.CFade.Main().fadeOut(10, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
				}
			}

			public void endOpeningCamera()
			{
				if (dgs.CFade.Main().isFaded())
				{
					setCommandCameraPositionAndTarget();
					openingState_ = 1;
					if (--moveFrame_ > 0)
					{
						return;
					}
					dgs.CFade.Main().fadeIn(5);
				}
				if (dgs.CFade.Main().isCleared())
				{
					OutsideToBattle.getInstance().setBattleCamera(BATTLE_CAMERA.COMMAND_CAMERA);
				}
			}

			public void readyEndingCamera()
			{
				OutsideToBattle.getInstance().setBattleCamera(BATTLE_CAMERA.ENDING_CAMERA);
				btlCamera.setPosition(EndingStartCameraPosition);
				btlCamera.setTarget(EndingStartCameraTarget);
				btlCamera.setAngle(EndingStartCameraAngle.x, EndingStartCameraAngle.y, EndingStartCameraAngle.z);
				btlCamera.setDistance(0);
				btlCamera.addDistance(EndingStartCameraDistance);
				moveFrame_ = 100;
			}

			public void goEndingCamera()
			{
				if (moveFrame_ >= 0)
				{
					moveFrame_--;
					btlCamera.setFOV(766, 4006);
					btlCamera.setPosition(calcCamera(btlCamera.getPosition(), EndingFinishCameraPosition, EndingStartCameraPosition, 100));
					btlCamera.setTarget(calcCamera(btlCamera.getTarget(), EndingFinishCameraTarget, EndingStartCameraTarget, 100));
				}
			}

			public void readyShakeCamera(int frame, VecFx32 width)
			{
				shakeFrame_ = frame;
				rootPosition_.copy(btlCamera.getPosition());
				shakeWidth_.copy(width);
			}

			public void doShakeCamera()
			{
				if (shakeFrame_ >= 0)
				{
					shakeFrame_--;
					if (shakeFrame_ <= 0)
					{
						btlCamera.setPosition(rootPosition_);
						return;
					}
					VecFx32 fnd_reuse_pos = GlobalScope.fnd_reuse_pos;
					fnd_reuse_pos.copy(btlCamera.getPosition());
					fnd_reuse_pos.x = (int)(rootPosition_.x + ds.RandomNumber.rand32((uint)shakeWidth_.x) - shakeWidth_.x / 2);
					fnd_reuse_pos.y = (int)(rootPosition_.y + ds.RandomNumber.rand32((uint)shakeWidth_.y) - shakeWidth_.y / 2);
					fnd_reuse_pos.z = (int)(rootPosition_.z + ds.RandomNumber.rand32((uint)shakeWidth_.z) - shakeWidth_.z / 2);
					btlCamera.setPosition(fnd_reuse_pos);
				}
			}

			public void entryStage()
			{
				OS_Printf("\n//-----------------------------------------------------\n");
				OS_Printf("バトルマップ番号は %d です\n", OutsideToBattle.getInstance().initializeBattleMap().battleMapId());
				sprintf(out var arg, "b%02d", OutsideToBattle.getInstance().initializeBattleMap().battleMapId());
				stageMng.setStage(arg);
			}

			public void changeStage()
			{
				stageMng.delStage();
				entryStage();
			}

			public ds.sys3d.Scene scene1st()
			{
				return m_Scene1st;
			}

			public ds.sys3d.Scene scene2nd()
			{
				return m_Scene2nd;
			}

			public ds.sys3d.CCamera getBattleCamera()
			{
				return btlCamera;
			}

			public void setMoveFrame(int frame)
			{
				moveFrame_ = frame;
			}

			public void setMonsterOffsetId(short _id)
			{
				monsterOffsetId_ = _id;
			}

			public short shakeFrame()
			{
				return (short)shakeFrame_;
			}

			public int openingState()
			{
				return openingState_;
			}

			public void setOpeningState(int i)
			{
				openingState_ = i;
			}
		}
	}
}
