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
	public class BPSlantVanish : Performer
	{
		public static int NUMBER_OF_VERTICES = 4;

		public VecFx32 pos;

		public VecFx16[] vtx = new VecFx16[NUMBER_OF_VERTICES];

		public int radius;

		public int rr;

		public int scale;

		public void target(int ctrl_id)
		{
			characterMng.setTransparency(ctrl_id, 30);
		}

		public void prepare()
		{
			scale = 0;
			vtx[0].x = 4096;
			vtx[0].y = (vtx[0].z = 0);
			vtx[1].x = 4096;
			vtx[1].y = (vtx[1].z = 0);
			vtx[2].x = 4096;
			vtx[2].y = (vtx[1].z = 0);
			vtx[3].x = 4096;
			vtx[3].y = (vtx[2].z = 0);
		}

		public bool progress()
		{
			scale += PROGRESS_SPEED;
			if (vtx[1].y < 4096)
			{
				vtx[1].y += (short)PROGRESS_SPEED;
				vtx[2].y += (short)PROGRESS_SPEED;
			}
			else
			{
				vtx[2].x -= (short)PROGRESS_SPEED;
			}
			vtx[3].x -= (short)PROGRESS_SPEED;
			return vtx[2].x > 0;
		}

		public void draw()
		{
			NNS_G3dGlbFlushP();
			G3_PushMtx();
			G3_Ortho(0, 786432, 0, 1048576, 4096, 4096, null);
			G3_MtxMode(GXMtxMode.GX_MTXMODE_POSITION);
			G3_Identity();
			G3_Scale(1048576, EFFECT_AREA_HEIGHT, 0);
			G3_PolygonAttr(0, GXPolygonMode.GX_POLYGONMODE_MODULATE, GXCull.GX_CULL_NONE, 0, 1, 2048);
			G3_Begin(GXBegin.GX_BEGIN_QUADS);
			for (int i = 0; i < 4; i++)
			{
				G3_Color(GX_RGB(15, 15, 15));
				G3_Vtx(vtx[i].x, vtx[i].y, vtx[i].z);
			}
			G3_End();
			G3_PopMtx(1);
		}

		public void finish()
		{
		}
	}
}
