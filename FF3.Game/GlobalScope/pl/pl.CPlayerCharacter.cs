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
		public class CPlayerCharacter : CBasePlayer
		{
			protected bool m_InputPermission;

			protected bool m_enableMoveVector;

			protected bool onVehicle_;

			protected bool touchMoveErr_;

			protected INPUT_MODE m_InPutMode;

			protected CNPCAiManager m_NPCAiManager = new CNPCAiManager();

			protected CPlayerCharacterSeEffect m_SeEffect = new CPlayerCharacterSeEffect();

			protected int m_EnvDamageCounter;

			protected int m_PoisonDamageCounter;

			protected sbyte m_WaitCounter_;

			public override void initialize()
			{
				base.initialize();
				m_ActionMng.initialize();
				PlayerMoveType_set(PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_ERR);
				NPCRandomMoveType_set(NPC_RANDOM_MOVE_TYPE.NPC_RANDOM_MOVE_TYPE_DEFAULT);
				NPCAutoFollowType_set(NPC_AUTO_FOLLOW_TYPE.NPC_AUTO_FOLLOW_TYPE_DEFAULT);
				m_Target = null;
				m_InputPermission = true;
				m_enableMoveVector = true;
				touchMoveErr_ = false;
				m_InPutMode = INPUT_MODE.INPUT_MODE_FIELD;
				m_NPCAiManager.initialize(this);
				m_NPCAiManager.AiKind_set(CNPCAiManager.AI_KIND.AI_KIND_DEFAULT);
				m_SeEffect.initialize();
				string text = "";
				text = sceneMng.getStage();
				if (text[0] == 'f' && getCharacterId() > 0)
				{
					setGrv(_GrvFlag: false);
				}
				m_EnvDamageCounter = 0;
				m_PoisonDamageCounter = 0;
				m_WaitCounter_ = 0;
			}

			public override void execute()
			{
				base.execute();
				if (getCharacterId() == -1)
				{
					return;
				}
				if (isAutoPilot())
				{
					getColFlag_not_and(16);
					getColFlag_not_and(4);
				}
				else
				{
					if (isOperater())
					{
						getColFlag_or(16);
					}
					if (isScriptWallCollision())
					{
						getColFlag_or(16);
					}
					getColFlag_or(4);
				}
				if (NPCAiManager().AiKind() == CNPCAiManager.AI_KIND.AI_KIND_AUTO_FOLLOW)
				{
					getColFlag_not_and(4);
					getColFlag_not_and(2);
				}
				if (canCalcDamage())
				{
					environmentDamage();
					poisonDamage();
				}
				stateMain();
				m_ActionMng.execute();
			}

			public override void terminate()
			{
				base.terminate();
				m_ActionMng.terminate();
				m_NPCAiManager.terminate();
				m_SeEffect.terminate();
			}

			public override void into()
			{
				base.into();
				if (!isOperater())
				{
					setMCLCol(b: false);
					getColFlag_or(1);
					getColFlag_not_and(8);
					getColFlag_not_and(16);
					getColFlag_not_and(4096);
					isGrv_set(arg0: false);
				}
				else
				{
					getColFlag_or(33554432);
					getColFlag_or(67108864);
					getColFlag_or(134217728);
					getColFlag_or(268435456);
					getColFlag_or(536870912);
					getColFlag_or(1073741824);
				}
				if (strcmp(sceneMng.getStage(), "debug01") != 0)
				{
					map.CMapParameterManager.Instance().isLoaded();
				}
				if (strcmp(getModelName(), "n211") == 0)
				{
					getColRadius_set(49152);
					getCckRadius_set(49152);
					getTchRadius_set(49152);
				}
				else if (strcmp(getModelName(), "n441") == 0 || strcmp(getModelName(), "n451") == 0 || strcmp(getModelName(), "n491") == 0 || strcmp(getModelName(), "n511") == 0)
				{
					getColRadius_set(16384);
					getCckRadius_set(24576);
					getTchRadius_set(65536);
				}
				else
				{
					getColRadius_set(16384);
					getCckRadius_set(24576);
					getTchRadius_set(32768);
				}
				VEC_Set(getTchOffset(), 0, 40960, -40960);
				m_NPCAiManager.reset();
				onVehicle_ = false;
			}

			public override void update()
			{
				base.update();
				int effect_add_y = 28672;
				if (strcmp(sceneMng.getStage(), "d07_01") == 0)
				{
					effect_add_y = 6144;
				}
				if (strcmp(sceneMng.getStage(), "d11_05") == 0)
				{
					effect_add_y = 4096;
				}
				m_SeEffect.execute(this, effect_add_y);
			}

			public override void reset()
			{
				base.reset();
			}

			public bool canCalcDamage()
			{
				if (CCastCommandTransit.getInstance().cast_BaseSystem().Mode() != wld.CBaseSystem.WORLD_MODE.WORLD_MODE_TALK && !evt.CEventManager.getInstance().isEvent() && CharaKind() == chr.CHARACTER_KIND.CHARACTER_KIND_PLAYER_HUMAN && InputPermission() && map.CMapParameterManager.Instance().isLoaded() && !isAutoPilot())
				{
					return isOperater();
				}
				return false;
			}

			public void environmentDamage()
			{
				if (!InputPermission() || !map.CMapParameterManager.Instance().isLoaded() || wld.WorldPart.getInstance().getWorldSystem().Mode() == wld.CBaseSystem.WORLD_MODE.WORLD_MODE_MENU || !isOperater())
				{
					return;
				}
				if (!flagCheck(CBP_FLAG.ENVIRONMENT_DAMAGING))
				{
					short num = map.CMapParameterManager.Instance().MapLandFormParameter(0).LandAttr(getLandFormIndex() - 1);
					if (num == 16)
					{
						m_EnvDamageCounter = (int)ENVIRONMENT_DAMAGE_INTERVAL;
						flagOn(CBP_FLAG.ENVIRONMENT_DAMAGING);
					}
					return;
				}
				dgs.ScreenFlash screenFlash = wld.WorldPart.getInstance().getWorldSystem().ScrFlash();
				short num2 = map.CMapParameterManager.Instance().MapLandFormParameter(0).LandAttr(getLandFormIndex() - 1);
				if (num2 != 16)
				{
					screenFlash.endFlash();
					flagOff(CBP_FLAG.ENVIRONMENT_DAMAGING);
				}
				else
				{
					if (++m_EnvDamageCounter <= ENVIRONMENT_DAMAGE_INTERVAL)
					{
						return;
					}
					m_EnvDamageCounter = 0;
					screenFlash.initialize();
					screenFlash.setFlash(1, (byte)ENVIRONMENT_DAMAGE_INTERVAL, GX_RGB(31, 15, 15));
					MatrixSound.MtxSENDS_Play(1, 13, 192, 127);
					for (int i = 0; i < 4; i++)
					{
						if (PlayerParty.instance().player((byte)i).isEnable() && !PlayerParty.instance().player((byte)i).condition()
							.isNotBattleCondition())
						{
							int num3 = PlayerParty.instance().player((byte)i).hp()
								.getNow() - 10;
							if (num3 <= 0)
							{
								num3 = 1;
							}
							PlayerParty.instance().player((byte)i).hp()
								.setNow(num3);
						}
					}
				}
			}

			public void poisonDamage()
			{
				if (!InputPermission() || !map.CMapParameterManager.Instance().isLoaded() || (1 != getNowAct() && 2 != getNowAct()) || !isOperater() || onVehicle_)
				{
					return;
				}
				bool flag = false;
				if (++m_PoisonDamageCounter > POISON_DAMAGE_INTERVAL)
				{
					flag = true;
					m_PoisonDamageCounter = 0;
				}
				bool flag2 = false;
				if (flag)
				{
					for (byte b = 0; b < 4; b++)
					{
						if (PlayerParty.instance().player(b).isEnable() && !PlayerParty.instance().player(b).condition()
							.isNotBattleCondition() && PlayerParty.instance().player(b).condition()
							.isPoison())
						{
							int limit = PlayerParty.instance().player(b).hp()
								.getLimit();
							int num = limit / 30;
							int num2 = PlayerParty.instance().player(b).hp()
								.getNow() - num;
							if (num2 <= 0)
							{
								num2 = 1;
							}
							PlayerParty.instance().player(b).hp()
								.setNow(num2);
							flag2 = true;
						}
					}
				}
				dgs.ScreenFlash screenFlash = wld.WorldPart.getInstance().getWorldSystem().ScrFlash();
				if (flag2 && !flagCheck(CBP_FLAG.ENVIRONMENT_DAMAGING))
				{
					screenFlash.initialize();
					screenFlash.setFlash(1, (byte)POISON_DAMAGE_INTERVAL, GX_RGB(31, 15, 15));
					MatrixSound.MtxSENDS_Play(1, 13, 192, 127);
				}
				else if (!flagCheck(CBP_FLAG.ENVIRONMENT_DAMAGING))
				{
					screenFlash.endFlash();
				}
			}

			public void stateMain()
			{
				if (!isAutoPilot())
				{
					if (!isOperater() && chr.CHARACTER_KIND.CHARACTER_KIND_PLAYER_VEHICLE != CharaKind() && !AutoRun())
					{
						m_NPCAiManager.execute();
						setNextAct(m_NPCAiManager.NPC().SendAct());
					}
					stateMoveVector();
				}
			}

			public void stateMoveVector()
			{
				if (!m_enableMoveVector || isAutoPilot())
				{
					return;
				}
				m_MoveSys.setFlag(_Flag: false);
				touchMoveErr_ = false;
				VecFx32 nitro_reuse_pos = GlobalScope.nitro_reuse_pos;
				nitro_reuse_pos.set(0, 0, 0);
				if (m_Operater)
				{
					if (dv.CDeviceManager.getInstance().Tp().isTouch())
					{
						dv.CDeviceManager.getInstance().Tp().TouchPanel_2d(out var x, out var y);
						if ((ds.g_TouchPanel.isEdge() && (CCastCommandTransit.getInstance().cast_Field2D().MenuStartButton()
							.isTouch() || CCastCommandTransit.getInstance().cast_Field2D().CameraButton()
							.isTouch() || CCastCommandTransit.getInstance().cast_Field2D().TalkButton()
							.isTouch())) || CCastCommandTransit.getInstance().cast_BaseSystem().World2DMng()
							.CameraButton()
							.isEdgeAndRepeatTouch())
						{
							return;
						}
						bool flag = false;
						if (CharaKind() == chr.CHARACTER_KIND.CHARACTER_KIND_PLAYER_VEHICLE)
						{
							flag = true;
						}
						if (!flag)
						{
							if (getNextAct() != 1 && getNextAct() != 2)
							{
								return;
							}
						}
						else
						{
							if (getNextAct() != 1 && getNextAct() != 0)
							{
								return;
							}
							TPData dispPoint = dv.CDeviceManager.getInstance().Tp().getDispPoint();
							int num = x - dispPoint.dragX;
							int num2 = y - dispPoint.dragY;
							if (!dv.CDeviceManager.getInstance().Pad().activity())
							{
								num = (num2 = 0);
							}
							if (num * num + num2 * num2 < 64)
							{
								touchMoveErr_ = true;
								return;
							}
							int num3 = 0;
							int num4 = 0;
							if (abs(num) * 2 > abs(num2))
							{
								num3 = ((num < 0) ? (-4096) : 4096);
							}
							if (abs(num2) * 2 > abs(num))
							{
								num4 = ((num2 < 0) ? (-4096) : 4096);
							}
							nitro_reuse_pos.copy(getPosition());
							nitro_reuse_pos.x += num3;
							nitro_reuse_pos.z += num4;
						}
					}
					else
					{
						if (getNextAct() != 1 && getNextAct() != 2)
						{
							return;
						}
						int num5 = 0;
						int num6 = 409600;
						VecFx32[] array = new VecFx32[9]
						{
							new VecFx32(0, 0, 0),
							new VecFx32(-num6, 0, -num6),
							new VecFx32(num6, 0, -num6),
							new VecFx32(-num6, 0, num6),
							new VecFx32(num6, 0, num6),
							new VecFx32(0, 0, -num6),
							new VecFx32(0, 0, num6),
							new VecFx32(-num6, 0, 0),
							new VecFx32(num6, 0, 0)
						};
						if ((dv.CDeviceManager.getInstance().Pad().pad_trs(0) & 1) != 0 && (dv.CDeviceManager.getInstance().Pad().pad_trs(0) & 4) != 0)
						{
							num5 = 1;
						}
						else if ((dv.CDeviceManager.getInstance().Pad().pad_trs(0) & 1) != 0 && (dv.CDeviceManager.getInstance().Pad().pad_trs(0) & 8) != 0)
						{
							num5 = 2;
						}
						else if ((dv.CDeviceManager.getInstance().Pad().pad_trs(0) & 2) != 0 && (dv.CDeviceManager.getInstance().Pad().pad_trs(0) & 4) != 0)
						{
							num5 = 3;
						}
						else if ((dv.CDeviceManager.getInstance().Pad().pad_trs(0) & 2) != 0 && (dv.CDeviceManager.getInstance().Pad().pad_trs(0) & 8) != 0)
						{
							num5 = 4;
						}
						else if ((dv.CDeviceManager.getInstance().Pad().pad_trs(0) & 1) != 0)
						{
							num5 = 5;
						}
						else if ((dv.CDeviceManager.getInstance().Pad().pad_trs(0) & 2) != 0)
						{
							num5 = 6;
						}
						else if ((dv.CDeviceManager.getInstance().Pad().pad_trs(0) & 4) != 0)
						{
							num5 = 7;
						}
						else if ((dv.CDeviceManager.getInstance().Pad().pad_trs(0) & 8) != 0)
						{
							num5 = 8;
						}
						if (num5 != 0)
						{
							VEC_Add(getPosition(), array[num5], nitro_reuse_pos);
						}
					}
				}
				else if (NPCAiManager().AiKind() == CNPCAiManager.AI_KIND.AI_KIND_RANDOM_MOVE)
				{
					if ((getColType() & 1) != 0 || NPCAiManager().NPC().isOver())
					{
						uint num7 = ds.RandomNumber.rand32(2u) << 12;
						VEC_Set(nitro_reuse_pos, (int)(MainPos().x + num7), MainPos().y, (int)(MainPos().z + num7));
					}
					if (m_NowAct != m_NextAct)
					{
						int num8 = 0;
						int num9 = 409600;
						VecFx32[] array2 = new VecFx32[8]
						{
							new VecFx32(-num9, 0, -num9),
							new VecFx32(num9, 0, -num9),
							new VecFx32(-num9, 0, num9),
							new VecFx32(num9, 0, num9),
							new VecFx32(0, 0, -num9),
							new VecFx32(0, 0, num9),
							new VecFx32(-num9, 0, 0),
							new VecFx32(num9, 0, 0)
						};
						num8 = ds.rand(8);
						if (num8 != 0)
						{
							VEC_Add(getPosition(), array2[num8], nitro_reuse_pos);
						}
					}
				}
				else if (NPCAiManager().AiKind() == CNPCAiManager.AI_KIND.AI_KIND_AUTO_FOLLOW)
				{
					if (NPCAiManager().NPC().LookPlayer() == null)
					{
						return;
					}
					nitro_reuse_pos.copy(NPCAiManager().NPC().LookPlayer().getPosition());
				}
				if (nitro_reuse_pos.x != 0 || nitro_reuse_pos.y != 0 || nitro_reuse_pos.z != 0)
				{
					m_MoveSys.setFlag(_Flag: true);
					m_MoveSys.setTargetPoint(0, getPosition());
					m_MoveSys.setTargetPoint(1, nitro_reuse_pos);
				}
			}

			public override void checkCollisionCharacter(chr.CCharacterEureka _Target)
			{
				if ((getColType() & 1) != 0)
				{
					getPosition().x = getPrePosition().x;
					getPosition().z = getPrePosition().z;
				}
			}

			public bool InputPermission()
			{
				return m_InputPermission;
			}

			public void InputPermission_set(bool arg0)
			{
				m_InputPermission = arg0;
			}

			public CNPCAiManager NPCAiManager()
			{
				return m_NPCAiManager;
			}

			public void setInPutMode(INPUT_MODE _InPutMode)
			{
				m_InPutMode = _InPutMode;
			}

			public INPUT_MODE getInPutMode()
			{
				return m_InPutMode;
			}

			public void enableMoveVector()
			{
				m_enableMoveVector = true;
			}

			public void disableMoveVector()
			{
				m_enableMoveVector = false;
			}

			public void setOnVehicle(bool b)
			{
				onVehicle_ = b;
			}

			public sbyte getWaitCounter()
			{
				return m_WaitCounter_;
			}

			public void setWaitCounter(sbyte counter)
			{
				m_WaitCounter_ = counter;
			}
		}
	}
}
