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
	public static class utl
	{
		private static VecFx32 utl_reuse_camPos = new VecFx32();

		private static VecFx32 utl_reuse_pos = new VecFx32();

		private static MtxFx33 utl_reuse_mtx = new MtxFx33();

		private static MtxFx33 utl_reuse_mtxX = new MtxFx33();

		private static MtxFx33 utl_reuse_mtxY = new MtxFx33();

		private static VecFx16 utl_reuse_rot = new VecFx16();

		internal static bool getTouchPanel3d(VecFx32 position, ds.sys3d.CCamera camera)
		{
			VecFx32 vecFx = utl_reuse_camPos;
			vecFx.copy(camera.getPosition());
			if (ds.g_TouchPanel.isTouch())
			{
				ushort num = 57344;
				ushort num2 = 0;
				VecFx32 vecFx2 = utl_reuse_pos;
				MtxFx33 mtxFx = utl_reuse_mtx;
				MtxFx33 mtxFx2 = utl_reuse_mtxX;
				MtxFx33 mtxFx3 = utl_reuse_mtxY;
				VecFx16 vecFx3 = utl_reuse_rot;
				VecFx32 direction = camera.getDirection();
				vecFx3.y = (short)FX_Atan2Idx(direction.x, direction.z);
				short sinVal = FX_SinIdx((ushort)(-vecFx3.y));
				short cosVal = FX_CosIdx((ushort)(-vecFx3.y));
				MTX_RotY33(mtxFx, sinVal, cosVal);
				MTX_MultVec33(direction, mtxFx, direction);
				vecFx3.x = (short)FX_Atan2Idx(direction.y, direction.z);
				vecFx3.x &= -1;
				num = (ushort)(-vecFx3.x);
				num2 = (ushort)vecFx3.y;
				short sinVal2 = FX_SinIdx(num);
				short cosVal2 = FX_CosIdx(num);
				MTX_RotX33(mtxFx2, sinVal2, cosVal2);
				sinVal = FX_SinIdx(num2);
				cosVal = FX_CosIdx(num2);
				MTX_RotY33(mtxFx3, sinVal, cosVal);
				MTX_Concat33(mtxFx2, mtxFx3, mtxFx);
				camera.getFOV(out var sin, out var cos);
				int num3 = 4096 * sin / cos;
				int num4 = num3 * 4 / 3;
				ds.g_TouchPanel.getPoint(out var x, out var y);
				x -= 128;
				y -= 96;
				y *= -1;
				int num5 = num4 * x / 128;
				int num6 = num3 * y / 96;
				vecFx2.x = num5 * 500;
				vecFx2.y = -num6 * 500;
				vecFx2.z = -2048000;
				VecFx32 vecFx4 = vecFx2;
				VEC_Normalize(vecFx4, vecFx4);
				MTX_MultVec33(vecFx4, mtxFx, vecFx4);
				int num7 = FX_Div(-vecFx.y, vecFx4.y);
				num7 >>= 12;
				vecFx4.x *= num7;
				vecFx4.y *= num7;
				vecFx4.z *= num7;
				vecFx4.x += vecFx.x;
				vecFx4.y += vecFx.y;
				vecFx4.z += vecFx.z;
				position.copy(vecFx4);
				return true;
			}
			return false;
		}
	}
}
