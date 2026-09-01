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
		public class CPlayerHumanUseItem : CPlayerHumanAction
		{
			public sbyte counter_;

			public int nextAction_;

			public override void start()
			{
				counter_ = 0;
				nextAction_ = 0;
				if (Player().isAutoPilot() || Player().getTarget() == null)
				{
					return;
				}
				if (!Player().getTarget().isItemEvent())
				{
					Player().setNextAct(0);
					return;
				}
				CPlayerCharacter cPlayerCharacter = null;
				if (Player().getTarget() != null)
				{
					cPlayerCharacter = static_cast<CPlayerCharacter>(Player().getTarget());
					if (cPlayerCharacter.NPCAiManager().AiKind() == CNPCAiManager.AI_KIND.AI_KIND_AUTO_FOLLOW)
					{
						Player().setNextAct(0);
						return;
					}
				}
				if (Player().getMotionIndex() != 1001)
				{
					Player().startMotion(1001, _Loop: true, 5u);
				}
				VecFx32 b = new VecFx32(Player().getPosition());
				VecFx32 vecFx = new VecFx32(Player().getTarget().getPosition());
				VEC_Subtract(vecFx, b, vecFx);
				VEC_Normalize(vecFx, vecFx);
				vecFx.x /= 682;
				vecFx.y /= 682;
				vecFx.z /= 682;
				Player().setTargetDirection(vecFx);
				Player().MoveSys().setFlag(_Flag: false);
				Player().setAutoPilot(_AutoPilot: true);
				if (Player().isOperater())
				{
					if (Player().getTarget().CharaKind() == chr.CHARACTER_KIND.CHARACTER_KIND_PLAYER_HUMAN || Player().getTarget().CharaKind() == chr.CHARACTER_KIND.CHARACTER_KIND_MAP_OBJECT)
					{
						Player().getTarget().setAutoPilot(_AutoPilot: true);
						Player().getTarget().setTarget(static_cast<chr.CCharacterEureka>(Player()));
						Player().getTarget().setNextAct(4);
						CCastCommandTransit.getInstance().cast_Field2D().ItemUseMenuManager()
							.create();
					}
					else
					{
						Player().setAutoPilot(_AutoPilot: false);
						Player().setNextAct(0);
					}
				}
			}

			public override void update()
			{
				sbyte b = 5;
				if (counter_ > 0 && --counter_ <= 0)
				{
					Player().setNextAct(nextAction_);
				}
				else
				{
					if (!Player().isOperater() || !Player().isAutoPilot())
					{
						return;
					}
					int num = 0;
					if ((dv.CDeviceManager.getInstance().Pad().edge_trs(0) & 0x80) != 0)
					{
						evt.CEventManager.getInstance().setItemEvent(_ItemEvent: true);
						num = menu.MenuManager.getSingleton().GetTargetItemNo();
						evt.CEventManager.getInstance().setUseItemId(num);
						menu.MenuManager.getSingleton().root().getNodeByID(TRANSCODE("item_use_window"))?.behavior().mbSetNotifier(null);
						CCastCommandTransit.getInstance().cast_Field2D().ItemUseMenuManager()
							.erase();
						if (Player().getTarget().isItemEvent())
						{
							if (Player().getTarget() != null)
							{
								if (Player().getTarget().CharaKind() == chr.CHARACTER_KIND.CHARACTER_KIND_PLAYER_HUMAN)
								{
									nextAction_ = 4;
									counter_ = b;
								}
								else
								{
									nextAction_ = 5;
									counter_ = b;
								}
								evt.CEventManager.getInstance().startLogic(Player().getTarget().LogicIndex());
							}
						}
						else
						{
							Player().setAutoPilot(_AutoPilot: false);
							nextAction_ = 0;
							counter_ = b;
							if (Player().getTarget() != null)
							{
								Player().getTarget().setAutoPilot(_AutoPilot: false);
								Player().getTarget().setNextAct(0);
							}
						}
						return;
					}
					ds.g_TouchPanel.getPoint(out var x, out var y);
					if ((dv.CDeviceManager.getInstance().Pad().edge_trs(0) & 0x20) != 0 || (ds.g_TouchPanel.isEdge() && menu.MenuManager.getSingleton().TouchWindowOutArea(x, y)))
					{
						menu.MenuManager.getSingleton().root().getNodeByID(TRANSCODE("item_use_window"))?.behavior().mbSetNotifier(null);
						CCastCommandTransit.getInstance().cast_Field2D().ItemUseMenuManager()
							.erase();
						Player().setAutoPilot(_AutoPilot: false);
						nextAction_ = 0;
						counter_ = b;
						if (Player().getTarget() != null)
						{
							Player().getTarget().setAutoPilot(_AutoPilot: false);
							Player().getTarget().setNextAct(0);
						}
					}
				}
			}

			public override void end()
			{
				Player().isAutoPilot_set(arg0: false);
				Player().AutoRun_set(arg0: false);
				Player().MoveSys().setStop(b: true);
			}
		}
	}
}
