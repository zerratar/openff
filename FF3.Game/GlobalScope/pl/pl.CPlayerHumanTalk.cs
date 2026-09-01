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
	public static partial class pl
	{
		public class CPlayerHumanTalk : CPlayerHumanAction
		{
			protected VecFx32 savedDirection = new VecFx32();

			public override void start()
			{
				if (Player().getTarget() == null)
				{
					return;
				}
				if (!Player().isOperater())
				{
					if (!Player().getInvalidActionMotion() && Player().getMotionIndex() != 1001)
					{
						Player().startMotion(1001, _Loop: true, 5u);
					}
					savedDirection.copy(Player().getDirection());
					if (!Player().flagCheck(CBasePlayer.CBP_FLAG.NPC_NOT_TURN_TALKED))
					{
						VecFx32 b = new VecFx32(Player().getPosition());
						VecFx32 vecFx = new VecFx32(Player().getTarget().getPosition());
						VEC_Subtract(vecFx, b, vecFx);
						VEC_Normalize(vecFx, vecFx);
						vecFx.x /= 682;
						vecFx.y /= 682;
						vecFx.z /= 682;
						Player().setTargetDirection(vecFx);
						Player().MoveSys().setFlag(_Flag: false);
					}
				}
				else
				{
					if (Player().getMotionIndex() != 1001)
					{
						Player().startMotion(1001, _Loop: true, 5u);
					}
					CPlayerHuman cPlayerHuman = (CPlayerHuman)Player();
					if (cPlayerHuman.getNpc() != cPlayerHuman.getTarget())
					{
						VecFx32 b2 = new VecFx32(Player().getPosition());
						VecFx32 vecFx2 = new VecFx32(Player().getTarget().getPosition());
						VEC_Subtract(vecFx2, b2, vecFx2);
						VEC_Normalize(vecFx2, vecFx2);
						vecFx2.x /= 682;
						vecFx2.y /= 682;
						vecFx2.z /= 682;
						Player().setTargetDirection(vecFx2);
						Player().MoveSys().setFlag(_Flag: false);
						Player().setWaitCounter(5);
					}
				}
				if (Player().isAutoPilot() || !Player().isOperater())
				{
					return;
				}
				if (Player().getTarget().CharaKind() == chr.CHARACTER_KIND.CHARACTER_KIND_PLAYER_HUMAN)
				{
					CPlayerCharacter cPlayerCharacter = null;
					if (Player().getTarget() != null)
					{
						cPlayerCharacter = static_cast<CPlayerCharacter>(Player().getTarget());
						if (cPlayerCharacter.NPCAiManager().AiKind() == CNPCAiManager.AI_KIND.AI_KIND_AUTO_FOLLOW)
						{
							if (Player().isBalloon())
							{
								evt.CEventManager.getInstance().setPartyTalkEvent(_PartyTalkEvent: true);
							}
							return;
						}
					}
					Player().getTarget().setAutoPilot(_AutoPilot: true);
					Player().getTarget().setTarget(static_cast<chr.CCharacterEureka>(Player()));
					Player().getTarget().setNextAct(4);
				}
				evt.CEventManager.getInstance().startLogic(Player().getTarget().LogicIndex());
			}

			public override void update()
			{
				if (!Player().isAutoPilot())
				{
					Player().setNextAct(0);
				}
			}

			public override void end()
			{
				Player().isAutoPilot_set(arg0: false);
				Player().AutoRun_set(arg0: false);
				if (!Player().isOperater() && Player().flagCheck(CBasePlayer.CBP_FLAG.NPC_RETURN_TALK_ENDS))
				{
					Player().setTargetDirection(savedDirection);
					Player().MoveSys().setFlag(_Flag: false);
				}
			}
		}
	}
}
