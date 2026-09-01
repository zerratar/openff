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
	public static partial class pl
	{
		public class CPlayerHumanCheck : CPlayerHumanAction
		{
			public override void start()
			{
				if (Player().getTarget() == null)
				{
					return;
				}
				map.CMapObject cMapObject = static_cast<map.CMapObject>(Player().getTarget());
				bool flag = false;
				bool flag2 = true;
				if (Player().getMotionIndex() != 1001)
				{
					Player().startMotion(1001, _Loop: true, 5u);
				}
				if (cMapObject.CharaKind() == chr.CHARACTER_KIND.CHARACTER_KIND_MAP_OBJECT)
				{
					if (cMapObject.MapObjType() == map.MAP_OBJECT_TYPE.TREASURE_BOX)
					{
						if (FlagManager.singleton().get(cMapObject.getFlagGroup(), cMapObject.getFlagIndex()) == 0)
						{
							cMapObject.setNowAct(3);
							Player().setAutoPilot(_AutoPilot: true);
							evt.CEventManager.getInstance().setEvent(_Event: true);
							flag = true;
						}
						else
						{
							flag2 = false;
						}
					}
					else if (cMapObject.MapObjType() == map.MAP_OBJECT_TYPE.INVISIBLE && cMapObject.itemId() != 0)
					{
						if (cMapObject.getItemId() != 0 || cMapObject.getGold() != 0)
						{
							cMapObject.setNowAct(3);
							Player().setAutoPilot(_AutoPilot: true);
							evt.CEventManager.getInstance().setEvent(_Event: true);
							flag = true;
						}
						else
						{
							flag = false;
						}
					}
				}
				if (flag2)
				{
					VecFx32 b = new VecFx32(Player().getPosition());
					VecFx32 vecFx = new VecFx32(cMapObject.getPosition());
					VEC_Subtract(vecFx, b, vecFx);
					VEC_Normalize(vecFx, vecFx);
					vecFx.x /= 682;
					vecFx.y /= 682;
					vecFx.z /= 682;
					Player().setTargetDirection(vecFx);
					Player().setWaitCounter(5);
				}
				if (!Player().isAutoPilot() && !flag)
				{
					evt.CEventManager.getInstance().startLogic(cMapObject.LogicIndex());
				}
			}

			public override void update()
			{
				Player().setNextAct(0);
			}

			public override void end()
			{
				Player().isAutoPilot_set(arg0: false);
				Player().AutoRun_set(arg0: false);
			}
		}
	}
}
