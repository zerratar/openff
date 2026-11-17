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
		public class CPlayerHumanWalk : CPlayerHumanAction
		{
			public override void start()
			{
				CPlayerHuman cPlayerHuman = static_cast<CPlayerHuman>(Player());
				if (cPlayerHuman.getHumanType() == PLAYER_HUMAN_TYPE.PLAYER_HUMAN_TYPE_FAIRY)
				{
					if (Player().getMotionIndex() != 1001)
					{
						Player().startMotion(1001, _Loop: true, 5u);
					}
				}
				else if (Player().getMotionIndex() != 1004)
				{
					Player().startMotion(1004, _Loop: true, 5u);
				}
				if (!Player().isOperater() && Player().NPCAiManager().AiKind() == CNPCAiManager.AI_KIND.AI_KIND_AUTO_FOLLOW)
				{
					if (Player().getTransparencyRate() == 0)
					{
						Player().setSucAlpha(100);
						Player().setAutoAlphaFrame(5);
						Player().setWorkAutoAlphaFrame(0);
					}
					if (Player().getShadowAlpha() == 0)
					{
						Player().setSucShadowAlpha(10);
						Player().setAutoShadowAlphaFrame(5);
						Player().setWorkAutoShadowAlphaFrame(0);
					}
				}
			}

			public override void update()
			{
				partyNPCBoard();
				if (!Player().isOperater())
				{
					return;
				}
				// PORT: on the world map a run is a walk at the field's pace (the act below turns it so) - drawn as a run, its
				// motion at that pace (CPlayerHuman.pacedMass), while the player runs; the walk's when they hold to walk.
				if (Player().getInPutMode() == INPUT_MODE.INPUT_MODE_FIELD && static_cast<CPlayerHuman>(Player()).getHumanType() != PLAYER_HUMAN_TYPE.PLAYER_HUMAN_TYPE_FAIRY && Player().getPlayerMoveType() != PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_FROG)
				{
					bool held = (dv.CDeviceManager.getInstance().Pad().pad_trs(0) & 0x20) != 0;   // B flips the setting (isRun, which also wants the d-pad - not the stick)
					bool running = opt.COptionManager.getSingleton().gameOption().worldMoveType() == opt.WORLD_MOVE_TYPE.WORLD_MOVE_TYPE_RUN ? !held : held;
					int wanted = running ? 1005 : 1004;
					if (Player().getMotionIndex() != wanted) { Player().startMotion(wanted, _Loop: true, 5u); OpenFF.Client.Log.First(OpenFF.Client.LogChannel.File, "field-run-" + wanted, 2, () => "field: the world map's walk drawn as " + (wanted == 1005 ? "a run" : "a walk")); }
				}
				if (CCastCommandTransit.getInstance().cast_BaseSystem().Mode() == wld.CBaseSystem.WORLD_MODE.WORLD_MODE_FIELD)
				{
					if (Player().getLandFormIndex() == 4 && evt.CEventManager.getInstance().FlagMng().get(0u, (uint)evt.EVENT_VEHICLE_FLAG[0]) == 1 && Player().isOperater())
					{
						Player().setNextAct(9);
						return;
					}
					if (Player().getTarget() != null && Player().getTarget() is CPlayerVehicle cPlayerVehicle && (cPlayerVehicle.VehicleType() == PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_ENTERP || cPlayerVehicle.VehicleType() == PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_ENTERP_CTM) && Player().isOperater() && canBoardVehicle(Player()))
					{
						Player().setNextAct(7);
						return;
					}
				}
				if (Player().AutoRun())
				{
					if (Player().isOperater() && m_Counter++ >= 5)
					{
						m_Counter = 6;
						if (dv.CDeviceManager.getInstance().Tp().isTouch() || (dv.CDeviceManager.getInstance().Pad().pad_trs(0) & 0xF) != 0)
						{
							Player().AutoRun_set(arg0: false);
							Player().setNextAct(1);
							return;
						}
					}
					VecFx32 b = new VecFx32(Player().getPosition());
					VecFx32 vecFx = new VecFx32(Player().getTarget().getPosition());
					VEC_Subtract(vecFx, b, vecFx);
					VEC_Normalize(vecFx, vecFx);
					vecFx.x /= 682;
					vecFx.y /= 682;
					vecFx.z /= 682;
					Player().setTargetDirection(vecFx);
					int num = 0;
					bool flag = true;
					if (Player().getTarget().CharaKind() == chr.CHARACTER_KIND.CHARACTER_KIND_PLAYER_VEHICLE)
					{
						CPlayerVehicle cPlayerVehicle2 = (CPlayerVehicle)Player().getTarget();
						if (cPlayerVehicle2.VehicleType() == PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_ENTERP || cPlayerVehicle2.VehicleType() == PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_ENTERP_CTM)
						{
							num = 57344;
							flag = false;
						}
						else
						{
							num = 20480;
						}
						VecFx32 vecFx2 = new VecFx32(Player().getPosition());
						VecFx32 vecFx3 = new VecFx32(Player().getTarget().getPosition());
						vecFx2.y = 0;
						vecFx3.y = 0;
						int num2 = VEC_Distance(vecFx2, vecFx3);
						if (num2 < num)
						{
							if (flag)
							{
								checkAction();
							}
						}
						else if (num2 >= 81920)
						{
							Player().setNextAct(2);
						}
					}
					else if (canCheckAction(Player(), Player().getTarget()))
					{
						checkAction();
					}
				}
				else
				{
					if (!Player().isOperater() || !Player().InputPermission() || touchPanelAction())
					{
						return;
					}
					if (checkActionTrigger())
					{
						checkAction();
					}
					else if (canWorldTalk(Player()))
					{
						gotoWorldTalk(Player());
					}
					else if ((dv.CDeviceManager.getInstance().Pad().pad_trs(0) & 0xF) == 0)
					{
						Player().setNextAct(0);
					}
					else if (isRun())
					{
						Player().setNextAct(2);
						if (Player().getPlayerMoveType() == PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_FROG)
						{
							Player().setNextAct(1);
						}
					}
				}
				if (Player().getInPutMode() == INPUT_MODE.INPUT_MODE_FIELD && Player().getNextAct() == 2)
				{
					Player().setNextAct(1);
				}
			}

			public override void end()
			{
				m_Counter = 0;
			}
		}
	}
}
