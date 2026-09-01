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
	public class BPIronChopper : sys.CBlankTask, Performer
	{
		protected int gap;

		protected int dir;

		protected ushort cline;

		public static void setting(int speed, int frames)
		{
			GAP_LIMIT = frames * speed;
			GAP_PER_FRAME = speed;
		}

		public void target(int ctrl_id)
		{
			VecFx32 vecFx = new VecFx32();
			ds.sys3d.BoundingBox boundingBox = characterMng.getBoundingBox(ctrl_id);
			characterMng.getPosition(ctrl_id, vecFx);
			vecFx.y += FX_Mul(boundingBox.size.y, boundingBox.scale >> 1);
			NNS_G3dWorldPosToScrPos(vecFx, out var _, out var py);
			g_Targets.push_back(new ICTARGET(ctrl_id, (ushort)py));
		}

		public void prepare()
		{
			gap = 0;
			cline = 0;
			dir = 1;
			for (int num = g_Targets.size() - 1; num >= 0; num--)
			{
				for (int num2 = num; num2 >= 0; num2--)
				{
					if (g_Targets[num].line > g_Targets[num2].line)
					{
						ICTARGET value = g_Targets[num2];
						g_Targets[num2] = g_Targets[num];
						g_Targets[num] = value;
					}
				}
			}
			beginVTask();
			beginHTask();
		}

		public override void vbTask()
		{
			cline = 0;
			dir = 1;
			G3X_SetHOffset(gap * dir);
		}

		public override void hbTask(ushort line)
		{
			if (line > 128)
			{
				G3X_SetHOffset(0);
				return;
			}
			if (line == 0)
			{
				G3X_SetHOffset(gap * dir);
				return;
			}
			for (int num = g_Targets.size() - 1; num >= 0; num--)
			{
				if (g_Targets[num].line <= line && cline < g_Targets[num].line)
				{
					cline = g_Targets[num].line;
					dir *= -1;
					G3X_SetHOffset(gap * dir);
					break;
				}
			}
		}

		public bool progress()
		{
			gap += GAP_PER_FRAME;
			return GAP_LIMIT > gap;
		}

		public void draw()
		{
		}

		public void finish()
		{
			G3X_SetHOffset(0);
			endVTask();
			endHTask();
			g_Targets.clear();
		}
	}
}
