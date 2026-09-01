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
		public class CPlayerHumanAction : act.CBaseAction
		{
			public static bool m_WaitNPCFlag;

			protected int m_Counter;

			public CPlayerCharacter GetPlayer()
			{
				return static_cast<CPlayerCharacter>(getCharacter());
			}

			public CPlayerCharacter Player()
			{
				return static_cast<CPlayerCharacter>(character());
			}

			public bool checkAction()
			{
				if (!Player().InputPermission())
				{
					return false;
				}
				int num = wld.CWorldOutSideData.getInstance().MapData().MapJumpIndex();
				if (0 <= num && canKeyDoorByDirection(Player()))
				{
					num--;
					map.CMapJumpParameter cMapJumpParameter = map.CMapParameterManager.Instance().MapJumpParameter(num);
					if (cMapJumpParameter != null && cMapJumpParameter.Kind() >= 800)
					{
						Player().setNextAct(11);
						Player().InputPermission_set(arg0: false);
						Player().MoveSys().setTargetPoint(0, 0, 0, 0);
						Player().MoveSys().setTargetPoint(1, 0, 0, 0);
						Player().MoveSys().setStop(b: true);
						Player().TurnSys().setStop(b: true);
						Player().getColFlag_not_and(4096);
						goto IL_023e;
					}
				}
				if (Player().getTarget() == null)
				{
					return false;
				}
				if (-1 == Player().getTarget().getCharacterId())
				{
					return false;
				}
				if (!Player().isOperater() && Player().getTarget().CharaKind() == chr.CHARACTER_KIND.CHARACTER_KIND_PLAYER_HUMAN)
				{
					Player().setNextAct(9);
				}
				else
				{
					if ((Player().getColType() & 2) == 0)
					{
						goto IL_023c;
					}
					Player().AutoRun_set(arg0: false);
					if (Player().getTarget().CharaKind() == chr.CHARACTER_KIND.CHARACTER_KIND_PLAYER_VEHICLE)
					{
						if (!canBoardVehicle(Player()))
						{
							Player().setNextAct(Player().getNowAct());
							Player().setTarget(null);
							return false;
						}
						Player().setNextAct(7);
					}
					else if (Player().getTarget() != null && (Player().getTarget().getNowAct() == 4 || Player().getTarget().getNowAct() == 5))
					{
						Player().setNextAct(0);
					}
					else if (Player().getTarget().CharaCheckType() == chr.CHARACTER_CHECK_TYPE.CHARACTER_CHECK_TYPE_TALK)
					{
						CPlayerCharacter cPlayerCharacter = (CPlayerCharacter)Player().getTarget();
						if (cPlayerCharacter.NPCAiManager().AiKind() == CNPCAiManager.AI_KIND.AI_KIND_AUTO_FOLLOW)
						{
							return false;
						}
						Player().setNextAct(4);
					}
					else
					{
						if (Player().getTarget().CharaCheckType() != chr.CHARACTER_CHECK_TYPE.CHARACTER_CHECK_TYPE_CHECK)
						{
							goto IL_023c;
						}
						Player().setNextAct(5);
					}
				}
				goto IL_023e;
				IL_023c:
				return false;
				IL_023e:
				CCastCommandTransit.getInstance().cast_Field2D().refMapNameWindow()
					.close();
				return true;
			}

			public bool touchPanelAction()
			{
				CPlayerHuman cPlayerHuman = static_cast<CPlayerHuman>(Player());
				if (!dv.CDeviceManager.getInstance().Tp().isTouch())
				{
					return false;
				}
				dv.CDeviceManager.getInstance().Tp().TouchPanel_2d(out var x, out var y);
				CPlayerHuman cPlayerHuman2 = (CPlayerHuman)Player();
				if (cPlayerHuman2.getMenuIcon() != null && ds.g_TouchPanel.isEdge() && cPlayerHuman2.getMenuIcon().isButtonTouch(x, y))
				{
					return false;
				}
				if (cPlayerHuman2.getTalkIcon() != null && ds.g_TouchPanel.isEdge() && cPlayerHuman2.getTalkIcon().isButtonTouch(x, y))
				{
					return false;
				}
				if (cPlayerHuman2.getCameraIcon() != null)
				{
					if (cPlayerHuman2.getCameraIcon().isEdgeAndRepeatTouch())
					{
						return false;
					}
					if (ds.g_TouchPanel.isEdge() && cPlayerHuman2.getCameraIcon().isButtonTouch(x, y))
					{
						return false;
					}
				}
				int num = 151552;
				int num2 = 147456;
				if (opt.COptionManager.getSingleton().gameOption().worldMoveType() == opt.WORLD_MOVE_TYPE.WORLD_MOVE_TYPE_RUN)
				{
					num = 0;
					num2 = 0;
				}
				int num3 = FX_Mul(num, num);
				int num4 = FX_Mul(num2, num2);
				TPData dispPoint = dv.CDeviceManager.getInstance().Tp().getDispPoint();
				VecFx32 vecFx = new VecFx32(dispPoint.x - dispPoint.dragX, 0, dispPoint.y - dispPoint.dragY);
				int num5 = (vecFx.x * vecFx.x + vecFx.z * vecFx.z) * 4096;
				if (num5 < 65536)
				{
					return false;
				}
				VEC_Add(cPlayerHuman.getPosition(), vecFx, vecFx);
				cPlayerHuman.MoveSys().setFlag(_Flag: true);
				cPlayerHuman.MoveSys().setTargetPoint(0, cPlayerHuman.getPosition());
				cPlayerHuman.MoveSys().setTargetPoint(1, vecFx);
				int nextAct = 1;
				int nextAct2 = 2;
				if (cPlayerHuman.getInPutMode() == INPUT_MODE.INPUT_MODE_FIELD || PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_FROG == cPlayerHuman.getPlayerMoveType())
				{
					nextAct2 = 1;
				}
				switch (cPlayerHuman.getNowAct())
				{
				case 0:
				case 3:
					if (num3 >= num5)
					{
						cPlayerHuman.setNextAct(nextAct);
					}
					else
					{
						cPlayerHuman.setNextAct(nextAct2);
					}
					break;
				case 1:
					if (num5 > num3)
					{
						cPlayerHuman.setNextAct(nextAct2);
					}
					break;
				case 2:
					if (num4 >= num5)
					{
						cPlayerHuman.setNextAct(nextAct);
					}
					break;
				}
				return true;
			}

			public void partyNPCBoard()
			{
				if (Player().isOperater() || Player().NPCAiManager().getAiKind() != CNPCAiManager.AI_KIND.AI_KIND_AUTO_FOLLOW || Player().AutoRun())
				{
					return;
				}
				if (Player().NPCAiManager().NPC().LookPlayer()
					.getNowAct() == 7)
				{
					Player().setTarget(Player().NPCAiManager().NPC().LookPlayer()
						.getTarget());
					Player().setAutoRun(_AutoRun: true);
					Player().setNextAct((Player().getInPutMode() == INPUT_MODE.INPUT_MODE_FIELD) ? 1 : 2);
					m_WaitNPCFlag = true;
				}
				else if (Player().NPCAiManager().NPC().LookPlayer()
					.getNowAct() != 8)
				{
					if (Player().NPCAiManager().NPC().LookPlayer()
						.getNowAct() == 9)
					{
						Player().setTarget(Player().NPCAiManager().NPC().LookPlayer());
						Player().setAutoRun(_AutoRun: true);
						Player().setNextAct((Player().getInPutMode() == INPUT_MODE.INPUT_MODE_FIELD) ? 1 : 2);
						m_WaitNPCFlag = true;
					}
					else if (Player().NPCAiManager().NPC().LookPlayer()
						.getNowAct() == 10)
					{
						Player().setNextAct(10);
					}
				}
			}
		}
	}
}
