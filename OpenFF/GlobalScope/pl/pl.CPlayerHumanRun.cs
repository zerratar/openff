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
		public class CPlayerHumanRun : CPlayerHumanAction
		{
			public int m_Rec;

			public override void start()
			{
				if (Player().getMotionIndex() != 1005)
				{
					Player().startMotion(1005, _Loop: true, 5u);
				}
				m_Rec = 2;
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
						Player().setSucShadowAlpha(15);
						Player().setAutoShadowAlphaFrame(5);
						Player().setWorkAutoShadowAlphaFrame(0);
					}
				}
			}

			public override void update()
			{
				if (!Player().isOperater())
				{
					return;
				}
				if (Player().AutoRun())
				{
					if (Player().isOperater() && m_Counter++ >= 5)
					{
						m_Counter = 6;
						if (dv.CDeviceManager.getInstance().Tp().isTouch() || (dv.CDeviceManager.getInstance().Pad().pad_trs(0) & 0xF) != 0)
						{
							Player().AutoRun_set(arg0: false);
							Player().setNextAct(2);
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
					short num = 20;
					if (VEC_Distance(Player().getPosition(), Player().getTarget().getPosition()) / 4096 < num)
					{
						Player().setNextAct(1);
					}
				}
				else
				{
					if (!Player().isOperater())
					{
						return;
					}
					if (!Player().InputPermission())
					{
						Player().setNextAct(0);
					}
					if (touchPanelAction())
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
						m_Rec--;
						if (m_Rec <= 0)
						{
							m_Rec = 0;
							Player().setNextAct(0);
						}
					}
					else if (isWalk())
					{
						Player().setNextAct(1);
					}
					else
					{
						m_Rec = 2;
					}
				}
			}

			public override void end()
			{
				m_Counter = 0;
			}
		}
	}
}
