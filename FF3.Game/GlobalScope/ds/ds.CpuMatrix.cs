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
	public static partial class ds
	{
		public class CpuMatrix
		{
			private static VecFx32 CpuMatrix_reuse_axisX = new VecFx32();

			private static VecFx32 CpuMatrix_reuse_axisY = new VecFx32();

			private static VecFx32 CpuMatrix_reuse_axisZ = new VecFx32();

			public static void getScale(out int x, out int y, out int z, MtxFx43 m)
			{
				x = VEC_Mag(m._00, m._01, m._02);
				y = VEC_Mag(m._10, m._11, m._12);
				z = VEC_Mag(m._20, m._21, m._22);
			}

			public static void normalize(MtxFx43 m)
			{
				VecFx32 cpuMatrix_reuse_axisX = CpuMatrix_reuse_axisX;
				VecFx32 cpuMatrix_reuse_axisY = CpuMatrix_reuse_axisY;
				VecFx32 cpuMatrix_reuse_axisZ = CpuMatrix_reuse_axisZ;
				cpuMatrix_reuse_axisX.set(m._00, m._01, m._02);
				cpuMatrix_reuse_axisY.set(m._10, m._11, m._12);
				cpuMatrix_reuse_axisZ.set(m._20, m._21, m._22);
				VEC_Normalize(cpuMatrix_reuse_axisX, cpuMatrix_reuse_axisX);
				VEC_Normalize(cpuMatrix_reuse_axisY, cpuMatrix_reuse_axisY);
				VEC_Normalize(cpuMatrix_reuse_axisZ, cpuMatrix_reuse_axisZ);
				m._00 = cpuMatrix_reuse_axisX.x;
				m._01 = cpuMatrix_reuse_axisX.y;
				m._02 = cpuMatrix_reuse_axisX.z;
				m._10 = cpuMatrix_reuse_axisY.x;
				m._11 = cpuMatrix_reuse_axisY.y;
				m._12 = cpuMatrix_reuse_axisY.z;
				m._20 = cpuMatrix_reuse_axisZ.x;
				m._21 = cpuMatrix_reuse_axisZ.y;
				m._22 = cpuMatrix_reuse_axisZ.z;
			}

			public static void setRotateX(MtxFx43 m, int rot)
			{
				MTX_RotX43(m, FX_SinIdx((ushort)rot), FX_CosIdx((ushort)rot));
			}

			public static void setRotateY(MtxFx43 m, int rot)
			{
				MTX_RotY43(m, FX_SinIdx((ushort)rot), FX_CosIdx((ushort)rot));
			}

			public static void setRotateZ(MtxFx43 m, int rot)
			{
				MTX_RotZ43(m, FX_SinIdx((ushort)rot), FX_CosIdx((ushort)rot));
			}

			public static void setRotate(MtxFx43 m, Vector3<int> vRot)
			{
				MtxFx43 mtxFx = new MtxFx43();
				setRotateX(m, vRot.vx);
				setRotateY(mtxFx, vRot.vy);
				MTX_Concat43(m, mtxFx, m);
				setRotateZ(mtxFx, vRot.vz);
				MTX_Concat43(m, mtxFx, m);
			}

			public void setRotate(MtxFx43 mtxDest, int rotx, int roty, int rotz)
			{
				Vector3<int> vRot = new Vector3<int>(rotx, roty, rotz);
				setRotate(mtxDest, vRot);
			}

			public static void getRotate(out int x, out int y, out int z, MtxFx43 m)
			{
				ushort num = 0;
				ushort num2 = 0;
				ushort num3 = 0;
				MtxFx43 m2 = new MtxFx43(m);
				Vector3<int> vector = new Vector3<int>();
				Vector3<int> vector2 = new Vector3<int>();
				VecFx32 vecFx = new VecFx32();
				normalize(m2);
				getOrientation(vector, vector2, m2);
				vecFx.copy(vector);
				vecFx.y = 0;
				VEC_Normalize(vecFx, vecFx);
				num2 = FX_Atan2Idx(vecFx.x, vecFx.z);
				num = (ushort)(-FX_Atan2Idx(vector.vy, FX_Sqrt(4096 - FX_Mul(vector.vy, vector.vy))));
				MtxFx43 mtxFx = new MtxFx43();
				MtxFx43 mtxFx2 = new MtxFx43();
				MtxFx43 mtxFx3 = new MtxFx43();
				Vector3<int> vector3 = new Vector3<int>();
				int sinVal = FX_SinIdx(num);
				int cosVal = FX_CosIdx(num);
				MTX_RotX43(mtxFx2, sinVal, cosVal);
				sinVal = FX_SinIdx(num2);
				cosVal = FX_CosIdx(num2);
				MTX_RotY43(mtxFx3, sinVal, cosVal);
				MTX_Concat43(mtxFx2, mtxFx3, mtxFx);
				getOrientation(vector, vector3, mtxFx);
				num3 = acosIdx(VEC_DotProduct(vector2, vector3));
				x = num << 12;
				y = num2 << 12;
				z = num3 << 12;
			}

			public void getAxisX(Vector3<int> vDest, MtxFx43 mtxSrc)
			{
				vDest.vx = mtxSrc._00;
				vDest.vy = mtxSrc._01;
				vDest.vz = mtxSrc._02;
			}

			public void getAxisY(Vector3<int> vDest, MtxFx43 mtxSrc)
			{
				vDest.vx = mtxSrc._10;
				vDest.vy = mtxSrc._11;
				vDest.vz = mtxSrc._12;
			}

			public void getAxisZ(Vector3<int> vDest, MtxFx43 mtxSrc)
			{
				vDest.vx = mtxSrc._20;
				vDest.vy = mtxSrc._21;
				vDest.vz = mtxSrc._22;
			}

			public void resetRotate(MtxFx43 m)
			{
				int num = (m._22 = 4096);
				int _ = (m._11 = num);
				m._00 = _;
				int num4 = (m._21 = 0);
				int num6 = (m._20 = num4);
				int num8 = (m._12 = num6);
				int num10 = (m._10 = num8);
				int _2 = (m._02 = num10);
				m._01 = _2;
			}

			public void resetRotate(MtxFx43 mtxDest, MtxFx43 mtxSrc)
			{
				int num = (mtxDest._22 = 4096);
				int _ = (mtxDest._11 = num);
				mtxDest._00 = _;
				int num4 = (mtxDest._21 = 0);
				int num6 = (mtxDest._20 = num4);
				int num8 = (mtxDest._12 = num6);
				int num10 = (mtxDest._10 = num8);
				int _2 = (mtxDest._02 = num10);
				mtxDest._01 = _2;
				mtxDest._30 = mtxSrc._30;
				mtxDest._31 = mtxSrc._31;
				mtxDest._32 = mtxSrc._32;
			}

			public static void resetTranslate(MtxFx43 m)
			{
				int num = (m._32 = 0);
				int _ = (m._31 = num);
				m._30 = _;
			}

			public static void getOrientation(Vector3<int> dir, Vector3<int> up, MtxFx43 m)
			{
				VecFx32 cpuMatrix_reuse_axisX = CpuMatrix_reuse_axisX;
				VecFx32 cpuMatrix_reuse_axisY = CpuMatrix_reuse_axisY;
				cpuMatrix_reuse_axisX.set(m._20, m._21, m._22);
				cpuMatrix_reuse_axisY.set(m._10, m._11, m._12);
				VEC_Normalize(cpuMatrix_reuse_axisX, cpuMatrix_reuse_axisX);
				VEC_Normalize(cpuMatrix_reuse_axisY, cpuMatrix_reuse_axisY);
				dir.vx = cpuMatrix_reuse_axisX.x;
				dir.vy = cpuMatrix_reuse_axisX.y;
				dir.vz = cpuMatrix_reuse_axisX.z;
				up.vx = cpuMatrix_reuse_axisY.x;
				up.vy = cpuMatrix_reuse_axisY.y;
				up.vz = cpuMatrix_reuse_axisY.z;
			}

			public void transpose(MtxFx43 m)
			{
				swap(ref m.a[1], ref m.a[3]);
				swap(ref m.a[2], ref m.a[6]);
				swap(ref m.a[5], ref m.a[7]);
			}

			public static void identity(MtxFx43 mtxDest)
			{
				MTX_Identity43(mtxDest);
			}

			public static void setScale(MtxFx43 mtxDest, Vector3<int> scl)
			{
				MTX_Scale43(mtxDest, scl.vx, scl.vy, scl.vz);
			}

			public static void getScale(Vector3<int> vDest, MtxFx43 mtxSrc)
			{
				getScale(out vDest.v[0], out vDest.v[1], out vDest.v[2], mtxSrc);
			}

			public static void getScale(VecFx32 vDest, MtxFx43 mtxSrc)
			{
				getScale(out vDest.x, out vDest.y, out vDest.z, mtxSrc);
			}

			public static void applyScale(MtxFx43 mtxDest, Vector3<int> scl)
			{
				MTX_ScaleApply43(g_mtxIdentity43, mtxDest, scl.vx, scl.vy, scl.vz);
			}

			public static void getRotate(Vector3<int> vDest, MtxFx43 mtxSrc)
			{
				getRotate(out vDest.v[0], out vDest.v[1], out vDest.v[2], mtxSrc);
			}

			public static void getRotate(VecFx32 vDest, MtxFx43 mtxSrc)
			{
				getRotate(out vDest.x, out vDest.y, out vDest.z, mtxSrc);
			}

			public static void setTranslate(MtxFx43 mtxDest, Vector3<int> trs)
			{
				MTX_TransApply43(g_mtxIdentity43, mtxDest, trs.vx, trs.vy, trs.vz);
			}

			public static void getTranslate(Vector3<int> trs, MtxFx43 mtxSrc)
			{
				trs.set(mtxSrc._30, mtxSrc._31, mtxSrc._32);
			}

			public static void getTranslate(VecFx32 trs, MtxFx43 mtxSrc)
			{
				trs.set(mtxSrc._30, mtxSrc._31, mtxSrc._32);
			}

			public static void applyTranslate(MtxFx43 mtxDest, MtxFx43 mtxSrc, Vector3<int> trs)
			{
				MTX_TransApply43(mtxSrc, mtxDest, trs.vx, trs.vy, trs.vz);
			}
		}
	}
}
