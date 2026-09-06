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
	public static partial class ds
	{
		public static class pri
		{
			public class PrimitiveTest
			{
				public static int distPlanePoint(DSPlane m, VecFx32 p)
				{
					return VEC_DotProduct(p, m.n) - m.d;
				}

				public static bool testSpherePlane(DSSphere s, DSPlane m)
				{
					int num = VEC_DotProduct(s.c, m.n) - m.d;
					if (num < 0)
					{
						FX_Mul(num, -4096);
					}
					return num <= s.r;
				}

				public static bool insideSpherePlane(DSSphere s, DSPlane m)
				{
					int num = VEC_DotProduct(s.c, m.n) - m.d;
					return num < -s.r;
				}

				public static bool testSphereHalfSpace(DSSphere s, DSPlane m)
				{
					int num = VEC_DotProduct(s.c, m.n) - m.d;
					return num <= s.r;
				}

				public static bool testRaySphere(DSLine l, DSSphere s, int[] t, VecFx32 p)
				{
					VecFx32 pri_reuse_v = pri_reuse_v0;
					VEC_Subtract(l.p, s.c, pri_reuse_v);
					int num = VEC_DotProduct(pri_reuse_v, l.d);
					int num2 = VEC_DotProduct(pri_reuse_v, pri_reuse_v) - FX_Mul(s.r, s.r);
					if (num2 > 0 && num > 0)
					{
						return false;
					}
					int num3 = FX_Mul(num, num) - num2;
					if (num3 < 0)
					{
						return false;
					}
					if (t != null)
					{
						t[0] = -num - FX_Sqrt(num3);
					}
					if (p != null)
					{
						VEC_MultAdd(t[0], l.d, l.p, p);
					}
					return true;
				}

				public static bool testRayAABB(DSLine l, DSAABB aabb)
				{
					int num = 0;
					int num2 = int.MaxValue;
					if (l.d.x < 0)
					{
						if (l.p.x < aabb.c.x - aabb.r.x)
						{
							return false;
						}
					}
					else if (l.p.x > aabb.c.x + aabb.r.x)
					{
						return false;
					}
					int v = FX_Div(4096, l.d.x);
					int num3 = FX_Mul(aabb.c.x - aabb.r.x - l.p.x, v);
					int num4 = FX_Mul(aabb.c.x + aabb.r.x - l.p.x, v);
					if (num3 > num4)
					{
						int num5 = num3;
						num3 = num4;
						num4 = num5;
					}
					if (num3 > num)
					{
						num = num3;
					}
					if (num4 > num2)
					{
						num2 = num4;
					}
					if (num > num2)
					{
						return false;
					}
					if (l.d.y < 0)
					{
						if (l.p.y < aabb.c.y - aabb.r.y)
						{
							return false;
						}
					}
					else if (l.p.y > aabb.c.y + aabb.r.y)
					{
						return false;
					}
					v = FX_Div(4096, l.d.y);
					num3 = FX_Mul(aabb.c.y - aabb.r.y - l.p.y, v);
					num4 = FX_Mul(aabb.c.y + aabb.r.y - l.p.y, v);
					if (num3 > num4)
					{
						int num6 = num3;
						num3 = num4;
						num4 = num6;
					}
					if (num3 > num)
					{
						num = num3;
					}
					if (num4 > num2)
					{
						num2 = num4;
					}
					if (num > num2)
					{
						return false;
					}
					if (l.d.z < 0)
					{
						if (l.p.z < aabb.c.z - aabb.r.z)
						{
							return false;
						}
					}
					else if (l.p.z > aabb.c.z + aabb.r.z)
					{
						return false;
					}
					v = FX_Div(4096, l.d.z);
					num3 = FX_Mul(aabb.c.z - aabb.r.z - l.p.z, v);
					num4 = FX_Mul(aabb.c.z + aabb.r.z - l.p.z, v);
					if (num3 > num4)
					{
						int num7 = num3;
						num3 = num4;
						num4 = num7;
					}
					if (num3 > num)
					{
						num = num3;
					}
					if (num4 > num2)
					{
						num2 = num4;
					}
					if (num > num2)
					{
						return false;
					}
					return true;
				}

				public static bool testSphereTriangle(DSSphere s, DSTriangle t, int[] sqLen, VecFx32 clPt)
				{
					VecFx32 vecFx = closestPtPointTriangle(s.c, t);
					VecFx32 pri_reuse_v = pri_reuse_v0;
					VEC_Set(pri_reuse_v, vecFx.x - s.c.x, vecFx.y - s.c.y, vecFx.z - s.c.z);
					int num = VEC_DotProduct(pri_reuse_v, pri_reuse_v);
					int num2 = FX_Mul(s.r, s.r);
					if (num > num2)
					{
						return false;
					}
					if (sqLen != null)
					{
						sqLen[0] = num;
					}
					if (clPt != null)
					{
						clPt.x = vecFx.x;
						clPt.y = vecFx.y;
						clPt.z = vecFx.z;
					}
					return true;
				}

				public static bool insidePointTriangle(VecFx32 p, DSTriangle tri)
				{
					if (p.x == tri.v0.x && p.y == tri.v0.y && p.z == tri.v0.z)
					{
						return true;
					}
					if (p.x == tri.v1.x && p.y == tri.v1.y && p.z == tri.v1.z)
					{
						return true;
					}
					if (p.x == tri.v2.x && p.y == tri.v2.y && p.z == tri.v2.z)
					{
						return true;
					}
					VecFx32 pri_reuse_v = pri_reuse_v0;
					VecFx32 pri_reuse_v2 = pri_reuse_v1;
					VecFx32 pri_reuse_v3 = pri.pri_reuse_v2;
					VEC_Subtract(tri.v0, p, pri_reuse_v);
					VEC_Subtract(tri.v1, p, pri_reuse_v2);
					VEC_Subtract(tri.v2, p, pri_reuse_v3);
					VEC_Normalize(pri_reuse_v, pri_reuse_v);
					VEC_Normalize(pri_reuse_v2, pri_reuse_v2);
					VEC_Normalize(pri_reuse_v3, pri_reuse_v3);
					VecFx32 pri_reuse_v4 = pri.pri_reuse_v3;
					VecFx32 pri_reuse_v5 = pri.pri_reuse_v4;
					VecFx32 pri_reuse_v6 = pri.pri_reuse_v5;
					VEC_CrossProduct(pri_reuse_v2, pri_reuse_v3, pri_reuse_v4);
					VEC_CrossProduct(pri_reuse_v3, pri_reuse_v, pri_reuse_v5);
					VEC_CrossProduct(pri_reuse_v, pri_reuse_v2, pri_reuse_v6);
					VEC_Normalize(pri_reuse_v4, pri_reuse_v4);
					VEC_Normalize(pri_reuse_v5, pri_reuse_v5);
					VEC_Normalize(pri_reuse_v6, pri_reuse_v6);
					if (VEC_DotProduct(pri_reuse_v4, pri_reuse_v5) < 0)
					{
						return false;
					}
					if (VEC_DotProduct(pri_reuse_v4, pri_reuse_v6) < 0)
					{
						return false;
					}
					if (VEC_DotProduct(pri_reuse_v5, pri_reuse_v6) < 0)
					{
						return false;
					}
					return true;
				}

				public static VecFx32 closestPtPointTriangle(VecFx32 p, DSTriangle tri)
				{
					VecFx32 pri_reuse_v = pri_reuse_v0;
					VecFx32 pri_reuse_v2 = pri_reuse_v1;
					VecFx32 pri_reuse_v3 = pri.pri_reuse_v2;
					VEC_Subtract(tri.v1, tri.v0, pri_reuse_v);
					VEC_Subtract(tri.v2, tri.v0, pri_reuse_v2);
					VEC_Subtract(p, tri.v0, pri_reuse_v3);
					int num = VEC_DotProduct(pri_reuse_v, pri_reuse_v3);
					int num2 = VEC_DotProduct(pri_reuse_v2, pri_reuse_v3);
					if (num <= 0 && num2 <= 0)
					{
						return tri.v0;
					}
					VecFx32 pri_reuse_v4 = pri.pri_reuse_v3;
					VEC_Subtract(p, tri.v1, pri_reuse_v4);
					int num3 = VEC_DotProduct(pri_reuse_v, pri_reuse_v4);
					int num4 = VEC_DotProduct(pri_reuse_v2, pri_reuse_v4);
					if (num3 >= 0 && num3 >= num4)
					{
						return tri.v1;
					}
					int num5 = FX_Mul(num, num4) - FX_Mul(num3, num2);
					if (num5 <= 0 && num >= 0 && num3 <= 0)
					{
						VecFx32 pri_reuse_v5 = pri.pri_reuse_v6;
						int a = FX_Div(num, num - num3);
						VEC_MultAdd(a, pri_reuse_v, tri.v0, pri_reuse_v5);
						return pri_reuse_v5;
					}
					VecFx32 pri_reuse_v6 = pri.pri_reuse_v4;
					VEC_Subtract(p, tri.v2, pri_reuse_v6);
					int num6 = VEC_DotProduct(pri_reuse_v, pri_reuse_v6);
					int num7 = VEC_DotProduct(pri_reuse_v2, pri_reuse_v6);
					if (num7 >= 0 && num6 <= num7)
					{
						return tri.v2;
					}
					int num8 = FX_Mul(num6, num2) - FX_Mul(num, num7);
					if (num8 <= 0 && num2 >= 0 && num7 <= 0)
					{
						VecFx32 pri_reuse_v7 = pri.pri_reuse_v6;
						int a2 = FX_Div(num2, num2 - num7);
						VEC_MultAdd(a2, pri_reuse_v2, tri.v0, pri_reuse_v7);
						return pri_reuse_v7;
					}
					int num9 = FX_Mul(num3, num7) - FX_Mul(num6, num4);
					if (num9 <= 0 && num4 - num3 >= 0 && num6 - num7 >= 0)
					{
						VecFx32 pri_reuse_v8 = pri.pri_reuse_v6;
						VecFx32 pri_reuse_v9 = pri.pri_reuse_v5;
						int a3 = FX_Div(num4 - num3, num4 - num3 + (num6 - num7));
						VEC_Subtract(tri.v2, tri.v1, pri_reuse_v9);
						VEC_MultAdd(a3, pri_reuse_v9, tri.v1, pri_reuse_v8);
						return pri_reuse_v8;
					}
					DSPlane dSPlane = new DSPlane(tri.v0, tri.v1, tri.v2);
					int num10 = VEC_DotProduct(dSPlane.n, p) - dSPlane.d;
					VecFx32 pri_reuse_v10 = pri.pri_reuse_v6;
					VEC_MultAdd(-num10, dSPlane.n, p, pri_reuse_v10);
					return pri_reuse_v10;
				}

				public static int sqDistPointAABB(VecFx32 p, DSAABB aabb)
				{
					int num = 0;
					int num2 = 0;
					num2 = p.x;
					int num3 = aabb.c.x - aabb.r.x;
					int num4 = aabb.c.x + aabb.r.x;
					if (num2 < num3)
					{
						num += FX_Mul(num3 - num2, num3 - num2);
					}
					if (num2 > num4)
					{
						num += FX_Mul(num2 - num4, num2 - num4);
					}
					num2 = p.y;
					num3 = aabb.c.y - aabb.r.y;
					num4 = aabb.c.y + aabb.r.y;
					if (num2 < num3)
					{
						num += FX_Mul(num3 - num2, num3 - num2);
					}
					if (num2 > num4)
					{
						num += FX_Mul(num2 - num4, num2 - num4);
					}
					num2 = p.z;
					num3 = aabb.c.z - aabb.r.z;
					num4 = aabb.c.z + aabb.r.z;
					if (num2 < num3)
					{
						num += FX_Mul(num3 - num2, num3 - num2);
					}
					if (num2 > num4)
					{
						num += FX_Mul(num2 - num4, num2 - num4);
					}
					return num;
				}

				public static VecFx32 closestPtPointAABB(VecFx32 p, DSAABB aabb)
				{
					VecFx32 pri_reuse_v = pri_reuse_v0;
					int num = p.x;
					int num2 = aabb.c.x - aabb.r.x;
					int num3 = aabb.c.x + aabb.r.x;
					if (num < num2)
					{
						num = num2;
					}
					if (num > num3)
					{
						num = num3;
					}
					pri_reuse_v.x = num;
					num = p.y;
					num2 = aabb.c.y - aabb.r.y;
					num3 = aabb.c.y + aabb.r.y;
					if (num < num2)
					{
						num = num2;
					}
					if (num > num3)
					{
						num = num3;
					}
					pri_reuse_v.y = num;
					num = p.z;
					num2 = aabb.c.z - aabb.r.z;
					num3 = aabb.c.z + aabb.r.z;
					if (num < num2)
					{
						num = num2;
					}
					if (num > num3)
					{
						num = num3;
					}
					pri_reuse_v.z = num;
					return pri_reuse_v;
				}

				public static bool testSphereAABB(DSSphere s, DSAABB aabb)
				{
					int num = sqDistPointAABB(s.c, aabb);
					return num <= FX_Mul(s.r, s.r);
				}

				public static bool testSphereSphere(DSSphere s1, DSSphere s2)
				{
					int num = VEC_Distance(s1.c, s2.c);
					return num <= s1.r + s2.r;
				}

				public static bool testSegmentPlane(DSSegment seg, DSPlane plane, VecFx32 pCrossPt)
				{
					VecFx32 pri_reuse_v = pri_reuse_v0;
					VEC_Subtract(seg.p1, seg.p2, pri_reuse_v);
					int num = VEC_DotProduct(pri_reuse_v, plane.n);
					if (num <= 0)
					{
						return false;
					}
					int num2 = VEC_DotProduct(seg.p1, plane.n);
					if (num2 < 0)
					{
						return false;
					}
					if (num2 > num)
					{
						return false;
					}
					if (pCrossPt != null)
					{
						VecFx32 pri_reuse_v2 = pri_reuse_v1;
						VEC_Subtract(seg.p2, seg.p1, pri_reuse_v2);
						VEC_MultAdd(FX_Div(num2, num), pri_reuse_v2, seg.p1, pCrossPt);
					}
					return true;
				}

				public static bool testSegmentTriangle(DSSegment seg, DSTriangle tri, bool[] crossPlane)
				{
					if (crossPlane != null)
					{
						crossPlane[0] = false;
					}
					VecFx32 pri_reuse_v = pri_reuse_v0;
					VEC_Subtract(seg.p1, seg.p2, pri_reuse_v);
					int num = VEC_DotProduct(pri_reuse_v, tri.n);
					if (num <= 0)
					{
						return false;
					}
					VecFx32 pri_reuse_v2 = pri_reuse_v1;
					VEC_Subtract(seg.p1, tri.v0, pri_reuse_v2);
					int num2 = VEC_DotProduct(pri_reuse_v2, tri.n);
					if (num2 < 0)
					{
						return false;
					}
					if (num2 > num)
					{
						return false;
					}
					if (crossPlane != null)
					{
						crossPlane[0] = true;
					}
					VecFx32 pri_reuse_v3 = pri.pri_reuse_v2;
					VecFx32 pri_reuse_v4 = pri.pri_reuse_v3;
					VEC_Subtract(seg.p2, seg.p1, pri_reuse_v4);
					VEC_MultAdd(FX_Div(num2, num), pri_reuse_v4, seg.p1, pri_reuse_v3);
					if (!insidePointTriangle(pri_reuse_v3, tri))
					{
						return false;
					}
					return true;
				}

				public static int closestPtSegmentSegment(DSSegment seg1, DSSegment seg2, ref int s, ref int t, VecFx32 c1, VecFx32 c2)
				{
					Vector3<float> vector = VecFx32ToFVector3(seg1.p1);
					Vector3<float> vector2 = VecFx32ToFVector3(seg1.p2);
					Vector3<float> vector3 = VecFx32ToFVector3(seg2.p1);
					Vector3<float> vector4 = VecFx32ToFVector3(seg2.p2);
					Vector3<float> vector5 = new Vector3<float>();
					Vector3<float> vector6 = new Vector3<float>();
					float num = 0f;
					float num2 = 0f;
					Vector3<float> vector7 = new Vector3<float>();
					Vector3<float> vector8 = new Vector3<float>();
					Vector3<float> vector9 = new Vector3<float>();
					vector7.vx = vector2.vx - vector.vx;
					vector7.vy = vector2.vy - vector.vy;
					vector7.vz = vector2.vz - vector.vz;
					vector8.vx = vector4.vx - vector3.vx;
					vector8.vy = vector4.vy - vector3.vy;
					vector8.vz = vector4.vz - vector3.vz;
					vector9.vx = vector.vx - vector3.vx;
					vector9.vy = vector.vy - vector3.vy;
					vector9.vz = vector.vz - vector3.vz;
					float num3 = FX_FxDot(vector7, vector7);
					float num4 = FX_FxDot(vector8, vector8);
					float num5 = FX_FxDot(vector8, vector9);
					if (num3 <= 0f && num4 <= 0f)
					{
						num2 = 0f;
						vector5 = vector;
						vector6 = vector3;
						Vector3<float> vector10 = new Vector3<float>();
						vector10.vx = vector5.vx - vector6.vx;
						vector10.vy = vector5.vy - vector6.vy;
						vector10.vz = vector5.vz - vector6.vz;
						return FX_F32_TO_FX32(FX_FxDot(vector10, vector10));
					}
					if (num3 <= 0f)
					{
						num = 0f;
						num2 = num5 / num4;
						num2 = FX_FxClamp(num2, 0f, 1f);
					}
					else
					{
						float num6 = FX_FxDot(vector7, vector9);
						if (num4 <= 0f)
						{
							num2 = FX_FxClamp((0f - num6) / num3, 0f, 1f);
						}
						else
						{
							float num7 = FX_FxDot(vector7, vector8);
							float num8 = num3 * num4 - num7 * num7;
							num = ((num8 == 0f) ? 0f : FX_FxClamp((num7 * num5 - num6 * num4) / num8, 0f, 1f));
							num2 = (num7 * num + num5) / num4;
							if (num2 < 0f)
							{
								num2 = 0f;
								num = FX_FxClamp((0f - num6) / num3, 0f, 1f);
							}
							else if (num2 > 1f)
							{
								num2 = 1f;
								num = FX_FxClamp((num7 - num6) / num3, 0f, 1f);
							}
						}
					}
					vector5.vx = vector.vx + vector7.vx * num;
					vector5.vy = vector.vy + vector7.vy * num;
					vector5.vz = vector.vz + vector7.vz * num;
					vector6.vx = vector3.vx + vector8.vx * num2;
					vector6.vy = vector3.vy + vector8.vy * num2;
					vector6.vz = vector3.vz + vector8.vz * num2;
					s = FX_F32_TO_FX32(num);
					t = FX_F32_TO_FX32(num2);
					c1 = FVector3ToVecFx32(vector5);
					c2 = FVector3ToVecFx32(vector6);
					Vector3<float> vector11 = new Vector3<float>();
					vector11.vx = vector5.vx - vector6.vx;
					vector11.vy = vector5.vy - vector6.vy;
					vector11.vz = vector5.vz - vector6.vz;
					float num9 = FX_FxDot(vector11, vector11);
					if (num9 < 4000f)
					{
						return FX_F32_TO_FX32(num9);
					}
					return int.MaxValue;
				}
			}

			public class DSPlane
			{
				public VecFx32 n = new VecFx32();

				public int d;

				public DSPlane(VecFx32 a, VecFx32 b, VecFx32 c)
				{
					VEC_Set(n, 0, 0, 0);
					d = 0;
					if ((a.x != b.x || a.y != b.y || a.z != b.z) && (b.x != c.x || b.y != c.y || b.z != c.z) && (c.x != a.x || c.y != a.y || c.z != a.z))
					{
						VecFx32 pri_reuse_v = pri_reuse_v0;
						VecFx32 pri_reuse_v2 = pri_reuse_v1;
						VEC_Subtract(b, a, pri_reuse_v);
						VEC_Subtract(c, a, pri_reuse_v2);
						VEC_Normalize(pri_reuse_v, pri_reuse_v);
						VEC_Normalize(pri_reuse_v2, pri_reuse_v2);
						VEC_CrossProduct(pri_reuse_v, pri_reuse_v2, n);
						if (n.x != 0 || n.y != 0 || n.z != 0)
						{
							VEC_Normalize(n, n);
						}
						d = VEC_DotProduct(n, a);
					}
				}

				public DSPlane()
				{
				}

				public bool isValidate()
				{
					if (n.x == 0 && n.y == 0)
					{
						return n.z != 0;
					}
					return true;
				}

				public void set(VecFx32 a, VecFx32 b, VecFx32 c)
				{
					VEC_Set(n, 0, 0, 0);
					d = 0;
					if ((a.x != b.x || a.y != b.y || a.z != b.z) && (b.x != c.x || b.y != c.y || b.z != c.z) && (c.x != a.x || c.y != a.y || c.z != a.z))
					{
						VecFx32 pri_reuse_v = pri_reuse_v0;
						VecFx32 pri_reuse_v2 = pri_reuse_v1;
						VEC_Subtract(b, a, pri_reuse_v);
						VEC_Subtract(c, a, pri_reuse_v2);
						VEC_Normalize(pri_reuse_v, pri_reuse_v);
						VEC_Normalize(pri_reuse_v2, pri_reuse_v2);
						VEC_CrossProduct(pri_reuse_v, pri_reuse_v2, n);
						if (n.x != 0 || n.y != 0 || n.z != 0)
						{
							VEC_Normalize(n, n);
						}
						d = VEC_DotProduct(n, a);
					}
				}
			}

			public class DSLine
			{
				public VecFx32 d = new VecFx32();

				public VecFx32 p = new VecFx32();

				public DSLine(VecFx32 p1, VecFx32 p2)
				{
					p.copy(p1);
					d.x = p2.x - p1.x;
					d.y = p2.y - p1.y;
					d.z = p2.z - p1.z;
					VEC_Normalize(d, d);
				}

				public static DSLine createDSLineForPointDir(VecFx32 p, VecFx32 d)
				{
					VecFx32 pri_reuse_v = pri_reuse_v0;
					VEC_MultAdd(4096, d, p, pri_reuse_v);
					return new DSLine(p, pri_reuse_v);
				}

				public DSLine createDSLineForPointPoint(VecFx32 p1, VecFx32 p2)
				{
					return new DSLine(p1, p2);
				}

				public DSLine()
				{
				}

				public void set(VecFx32 p1, VecFx32 p2)
				{
					p.copy(p1);
					d.x = p2.x - p1.x;
					d.y = p2.y - p1.y;
					d.z = p2.z - p1.z;
					VEC_Normalize(d, d);
				}

				public DSLine setDSLineForPointDir(VecFx32 p, VecFx32 d)
				{
					VecFx32 pri_reuse_v = pri_reuse_v0;
					VEC_MultAdd(4096, d, p, pri_reuse_v);
					set(p, pri_reuse_v);
					return this;
				}
			}

			public class DSTriangle
			{
				public VecFx32 v0 = new VecFx32();

				public VecFx32 v1 = new VecFx32();

				public VecFx32 v2 = new VecFx32();

				public VecFx32 n = new VecFx32();

				public DSTriangle(VecFx32 p0, VecFx32 p1, VecFx32 p2)
				{
					VEC_Set(v0, p0.x, p0.y, p0.z);
					VEC_Set(v1, p1.x, p1.y, p1.z);
					VEC_Set(v2, p2.x, p2.y, p2.z);
					VecFx32 pri_reuse_v = pri_reuse_v0;
					VecFx32 pri_reuse_v2 = pri_reuse_v1;
					VEC_Subtract(v1, v0, pri_reuse_v);
					VEC_Subtract(v2, v0, pri_reuse_v2);
					VEC_CrossProduct(pri_reuse_v, pri_reuse_v2, n);
				}

				public DSTriangle(VecFx32 p0, VecFx32 p1, VecFx32 p2, VecFx32 normal)
				{
					VEC_Set(v0, p0.x, p0.y, p0.z);
					VEC_Set(v1, p1.x, p1.y, p1.z);
					VEC_Set(v2, p2.x, p2.y, p2.z);
					VEC_Set(n, normal.x, normal.y, normal.z);
				}

				public DSTriangle()
				{
				}

				public void set(VecFx32 p0, VecFx32 p1, VecFx32 p2)
				{
					VEC_Set(v0, p0.x, p0.y, p0.z);
					VEC_Set(v1, p1.x, p1.y, p1.z);
					VEC_Set(v2, p2.x, p2.y, p2.z);
					VecFx32 pri_reuse_v = pri_reuse_v0;
					VecFx32 pri_reuse_v2 = pri_reuse_v1;
					VEC_Subtract(v1, v0, pri_reuse_v);
					VEC_Subtract(v2, v0, pri_reuse_v2);
					VEC_CrossProduct(pri_reuse_v, pri_reuse_v2, n);
				}

				public void set(VecFx32 p0, VecFx32 p1, VecFx32 p2, VecFx32 normal)
				{
					VEC_Set(v0, p0.x, p0.y, p0.z);
					VEC_Set(v1, p1.x, p1.y, p1.z);
					VEC_Set(v2, p2.x, p2.y, p2.z);
					VEC_Set(n, normal.x, normal.y, normal.z);
				}
			}

			public class DSAABB
			{
				public VecFx32 c = new VecFx32();

				public VecFx32 r = new VecFx32();

				public DSAABB(VecFx32 c, int rx, int ry, int rz)
				{
					VEC_Set(this.c, c.x, c.y, c.z);
					VEC_Set(r, rx, ry, rz);
				}

				public DSSphere getSphere()
				{
					DSSphere dSSphere = new DSSphere();
					VecFx32 vecFx = new VecFx32();
					VecFx32 vecFx2 = new VecFx32();
					VEC_Set(vecFx, 0, 0, 0);
					VEC_Set(vecFx2, r.x, r.y, r.z);
					dSSphere.r = VEC_Distance(vecFx, vecFx2);
					dSSphere.c.copy(c);
					return dSSphere;
				}

				public DSAABB()
				{
				}
			}

			public class DSSegment
			{
				public VecFx32 p1;

				public VecFx32 p2;

				public DSSegment(VecFx32 p1, VecFx32 p2)
				{
					this.p1.copy(p1);
					this.p2.copy(p2);
				}

				public DSSegment()
				{
				}
			}

			public class DSSphere
			{
				public VecFx32 c = new VecFx32();

				public int r;

				public DSSphere()
				{
				}

				public DSSphere(VecFx32 c, int radius)
				{
					this.c.copy(c);
					r = radius;
				}

				public void set(VecFx32 c, int radius)
				{
					this.c.copy(c);
					r = radius;
				}
			}

			private static VecFx32 pri_reuse_v0 = new VecFx32();

			private static VecFx32 pri_reuse_v1 = new VecFx32();

			private static VecFx32 pri_reuse_v2 = new VecFx32();

			private static VecFx32 pri_reuse_v3 = new VecFx32();

			private static VecFx32 pri_reuse_v4 = new VecFx32();

			private static VecFx32 pri_reuse_v5 = new VecFx32();

			private static VecFx32 pri_reuse_v6 = new VecFx32();

			internal static int FX_Clamp(int n, int min, int max)
			{
				if (n < min)
				{
					return min;
				}
				if (n > max)
				{
					return max;
				}
				return n;
			}

			internal static float FX_FxClamp(float f, float min, float max)
			{
				if (f < min)
				{
					return min;
				}
				if (f > max)
				{
					return max;
				}
				return f;
			}

			internal static float FX_FxDot(Vector3<float> pVec1, Vector3<float> pVec2)
			{
				return pVec1.vx * pVec2.vx + pVec1.vy * pVec2.vy + pVec1.vz * pVec2.vz;
			}

			internal static void FX_FxCross(Vector3<float> pVec1, Vector3<float> pVec2, Vector3<float> pDest)
			{
				pDest.vx = pVec1.vy * pVec2.vz - pVec1.vz * pVec2.vy;
				pDest.vy = pVec1.vz * pVec2.vx - pVec1.vx * pVec2.vz;
				pDest.vz = pVec1.vx * pVec2.vy - pVec1.vy * pVec2.vx;
			}
		}
	}
}
