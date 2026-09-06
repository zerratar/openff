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
	public class BPDivide : Performer
	{
		public static int NUMBER_OF_VERTICES = 4;

		public VecFx32 pos;

		public VecFx16[] vtx = new VecFx16[NUMBER_OF_VERTICES];

		public VecFx16[] vtx2 = new VecFx16[NUMBER_OF_VERTICES];

		public int radius;

		public int scale;

		public int step;

		public int target_cid;

		public int left;

		public int top;

		public int right;

		public int bottom;

		public int depth;

		public VecFx32 BBSize;

		public static void setting(int num_of_lines, int frames_per_line)
		{
			DIV_STEPS = num_of_lines;
			DIV_PROGRESS_SPEED = FX_Div(4096, 4096 * frames_per_line);
		}

		public void target(int ctrl_id)
		{
			target_cid = ctrl_id;
			characterMng.setTransparency(ctrl_id, 30);
		}

		public void prepare()
		{
			scale = 0;
			ds.sys3d.BoundingBox boundingBox = characterMng.getBoundingBox(target_cid);
			BBSize.x = FX_Mul(boundingBox.size.x, boundingBox.scale);
			BBSize.y = FX_Mul(boundingBox.size.y, boundingBox.scale);
			BBSize.z = FX_Mul(boundingBox.size.z, boundingBox.scale);
			radius = FX_Mul(BBSize.x, BBSize.x);
			radius += FX_Mul(BBSize.y, BBSize.y);
			radius += FX_Mul(BBSize.z, BBSize.z);
			radius = FX_Sqrt(radius);
			radius = FX_Mul(radius, 2048);
			radius += FX_Mul(radius, 2048);
			VecFx32[] array = new VecFx32[8]
			{
				new VecFx32(BBSize.x >> 1, BBSize.y, BBSize.z >> 1),
				new VecFx32(-(BBSize.x >> 1), BBSize.y, BBSize.z >> 1),
				new VecFx32(-(BBSize.x >> 1), BBSize.y, -(BBSize.z >> 1)),
				new VecFx32(BBSize.x >> 1, BBSize.y, -(BBSize.z >> 1)),
				new VecFx32(BBSize.x >> 1, 0, BBSize.z >> 1),
				new VecFx32(-(BBSize.x >> 1), 0, BBSize.z >> 1),
				new VecFx32(-(BBSize.x >> 1), 0, -(BBSize.z >> 1)),
				new VecFx32(BBSize.x >> 1, 0, -(BBSize.z >> 1))
			};
			MtxFx43 mtxFx = new MtxFx43(NNS_G3dGlbGetCameraMtx());
			int num = (mtxFx._32 = 0);
			int _ = (mtxFx._31 = num);
			mtxFx._30 = _;
			left = (top = (right = (bottom = (depth = 0))));
			for (int i = 0; i < 8; i++)
			{
				MTX_MultVec43(array[i], mtxFx, array[i]);
				if (left < array[i].x)
				{
					left = array[i].x;
				}
				else if (right > array[i].x)
				{
					right = array[i].x;
				}
				if (top < array[i].y)
				{
					top = array[i].y;
				}
				else if (bottom > array[i].y)
				{
					bottom = array[i].y;
				}
				if (depth < array[i].z)
				{
					depth = array[i].z;
				}
			}
			for (int j = 0; j < NUMBER_OF_VERTICES; j++)
			{
				VEC_Fx16Set(vtx2[j], 0, 0, 0);
			}
			step = 1;
			progress();
		}

		public bool progress()
		{
			scale += DIV_PROGRESS_SPEED;
			MtxFx43 mtxFx = new MtxFx43(NNS_G3dGlbGetCameraMtx());
			int num = (mtxFx._32 = 0);
			int _ = (mtxFx._31 = num);
			mtxFx._30 = _;
			MTX_Inverse43(mtxFx, mtxFx);
			characterMng.getPosition(target_cid, pos);
			if (scale <= 4096)
			{
				VecFx32[] array = new VecFx32[NUMBER_OF_VERTICES];
				int v = FX_Mul(left - right, 2048);
				int v2 = top - bottom;
				VEC_Set(array[0], left - FX_Mul(v, 4096 - scale), top, depth);
				VEC_Set(array[1], right + FX_Mul(v, 4096 - scale), top, depth);
				array[2].copy(array[1]);
				array[2].y -= FX_Mul(v2, FX_Div(4096 * step, 4096 * DIV_STEPS));
				array[3].copy(array[0]);
				array[3].y -= FX_Mul(v2, FX_Div(4096 * step, 4096 * DIV_STEPS));
				for (int i = 0; i < NUMBER_OF_VERTICES; i++)
				{
					MTX_MultVec43(array[i], mtxFx, array[i]);
					array[i].x = FX_Div(array[i].x, FX32_16_SCALE);
					array[i].y = FX_Div(array[i].y, FX32_16_SCALE);
					array[i].z = FX_Div(array[i].z, FX32_16_SCALE);
					VEC_Fx16Set(vtx[i], (short)array[i].x, (short)array[i].y, (short)array[i].z);
				}
			}
			else
			{
				scale = 0;
				step++;
				if (step > DIV_STEPS)
				{
					return false;
				}
				VecFx32[] array2 = new VecFx32[NUMBER_OF_VERTICES];
				int v3 = top - bottom;
				VEC_Set(array2[0], left, top, depth);
				VEC_Set(array2[1], right, top, depth);
				array2[2].copy(array2[1]);
				array2[2].y -= FX_Mul(v3, FX_Div(4096 * (step - 1), 4096 * DIV_STEPS));
				array2[3].copy(array2[0]);
				array2[3].y -= FX_Mul(v3, FX_Div(4096 * (step - 1), 4096 * DIV_STEPS));
				for (int j = 0; j < NUMBER_OF_VERTICES; j++)
				{
					MTX_MultVec43(array2[j], mtxFx, array2[j]);
					array2[j].x = FX_Div(array2[j].x, FX32_16_SCALE);
					array2[j].y = FX_Div(array2[j].y, FX32_16_SCALE);
					array2[j].z = FX_Div(array2[j].z, FX32_16_SCALE);
					VEC_Fx16Set(vtx2[j], (short)array2[j].x, (short)array2[j].y, (short)array2[j].z);
				}
			}
			return true;
		}

		public void draw()
		{
			NNS_G3dGlbFlushP();
			G3_PushMtx();
			G3_MtxMode(GXMtxMode.GX_MTXMODE_POSITION);
			G3_Translate(pos.x, pos.y, pos.z);
			G3_PolygonAttr(0, GXPolygonMode.GX_POLYGONMODE_MODULATE, GXCull.GX_CULL_NONE, 0, 1, 2048);
			G3_Scale(FX32_16_SCALE, FX32_16_SCALE, FX32_16_SCALE);
			G3_Begin(GXBegin.GX_BEGIN_QUADS);
			for (int i = 0; i < NUMBER_OF_VERTICES; i++)
			{
				G3_Color(GX_RGB(15, 15, 15));
				G3_Vtx(vtx2[i].x, vtx2[i].y, vtx2[i].z);
			}
			for (int j = 0; j < NUMBER_OF_VERTICES; j++)
			{
				G3_Color(GX_RGB(15, 15, 15));
				G3_Vtx(vtx[j].x, vtx[j].y, vtx[j].z);
			}
			G3_End();
			G3_PopMtx(1);
		}

		public void finish()
		{
		}
	}
}
