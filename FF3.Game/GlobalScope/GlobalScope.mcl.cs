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
	public static class mcl
	{
		public class CObject
		{
			public static bool ms_checkTwice = false;

			protected static uint msc_uiNameLength = 24u;

			protected string m_strName;

			protected CPolygonTableData m_cPolyTable;

			protected CBlockTableData m_cBlockTable;

			protected VecFx32[] m_Points;

			protected uint m_uiPointNum;

			protected CMaterialData[] m_acMaterial;

			protected uint m_uiMaterialNum;

			protected TVector3<int> m_fvBoxMin;

			protected uint[] padding = new uint[3];

			protected AABB m_AABB;

			public void initialize()
			{
				m_cPolyTable.initialize();
				m_cBlockTable.initialize();
			}

			public CBlockData getBlock(VecFx32 _pos)
			{
				VecFx32 vecFx = new VecFx32(_pos);
				VEC_Subtract(m_AABB.m_fxv4Max, vecFx, vecFx);
				int num = 0;
				int num2 = 0;
				int num3 = 0;
				if (vecFx.x > 0)
				{
					int num4 = (ushort)FX_Whole(FX_Div(vecFx.x, m_cBlockTable.m_vfxBlockSize.x));
					num = m_cBlockTable.m_usvBlockNum.x - 1 - num4;
					if (num < 0)
					{
						num = 0;
					}
					else if (num > m_cBlockTable.m_usvBlockNum.x)
					{
						num = m_cBlockTable.m_usvBlockNum.x;
					}
				}
				if (vecFx.y > 0)
				{
					int num5 = (ushort)FX_Whole(FX_Div(vecFx.y, m_cBlockTable.m_vfxBlockSize.y));
					num2 = m_cBlockTable.m_usvBlockNum.y - num5 - 1;
					if (num2 < 0)
					{
						num2 = 0;
					}
					else if (num2 > m_cBlockTable.m_usvBlockNum.y)
					{
						num2 = m_cBlockTable.m_usvBlockNum.y;
					}
				}
				if (vecFx.z > 0)
				{
					int num6 = (ushort)FX_Whole(FX_Div(vecFx.z, m_cBlockTable.m_vfxBlockSize.z));
					num3 = m_cBlockTable.m_usvBlockNum.z - num6 - 1;
					if (num3 < 0)
					{
						num3 = 0;
					}
					else if (num3 > m_cBlockTable.m_usvBlockNum.z)
					{
						num3 = m_cBlockTable.m_usvBlockNum.z;
					}
				}
				return m_cBlockTable.getBlock((ushort)num, (ushort)num2, (ushort)num3);
			}

			public bool evaluateArrowImp2(CBlockData pBlock, VecFx32 pos, VecFx32 dir, int len, int[] matList, byte matNum, CollisionResult ret)
			{
				for (int num = pBlock.getNumberOfPolygon() - 1; num >= 0; num--)
				{
					CPolygonData cPolygonData = const_cast<CPolygonData>(polygons()[pBlock.getPolygonIndex((ushort)num)]);
					byte b = 0;
					if (matNum > 0)
					{
						b = 0;
						while (b < matNum && !getMaterial(cPolygonData.getMaterial()).getAttribute().isEnableFlag((uint)matList[b]))
						{
							b++;
						}
					}
					if (b < matNum)
					{
						VecFx32 mcl_reuse_normal = mcl.mcl_reuse_normal;
						cPolygonData.getNormal(mcl_reuse_normal);
						VEC_Normalize(mcl_reuse_normal, mcl_reuse_normal);
						if (VEC_DotProduct(dir, mcl_reuse_normal) < 0)
						{
							VecFx32 vecFx = points()[cPolygonData.getVertex(0u)];
							VecFx32 vecFx2 = points()[cPolygonData.getVertex(1u)];
							VecFx32 vecFx3 = points()[cPolygonData.getVertex(2u)];
							if (vecFx != null && vecFx2 != null && vecFx3 != null)
							{
								VecFx32 mcl_reuse_toEdge = mcl.mcl_reuse_toEdge;
								VecFx32 mcl_reuse_toTri = mcl.mcl_reuse_toTri;
								mcl_reuse_toEdge.x = FX_Mul(dir.x, len);
								mcl_reuse_toEdge.y = FX_Mul(dir.y, len);
								mcl_reuse_toEdge.z = FX_Mul(dir.z, len);
								VEC_Subtract(vecFx, pos, mcl_reuse_toTri);
								int denom = VEC_DotProduct(mcl_reuse_toEdge, mcl_reuse_normal);
								int numer = VEC_DotProduct(mcl_reuse_toTri, mcl_reuse_normal);
								int num2 = FX_Mul32x64c(len, FX_DivFx64c(numer, denom));
								if (num2 > 0 && num2 < ret.length)
								{
									VecFx32 mcl_reuse_toCrossPt = mcl.mcl_reuse_toCrossPt;
									VecFx32 mcl_reuse_crossPt = mcl.mcl_reuse_crossPt;
									mcl_reuse_toCrossPt.x = FX_Mul(dir.x, num2);
									mcl_reuse_toCrossPt.y = FX_Mul(dir.y, num2);
									mcl_reuse_toCrossPt.z = FX_Mul(dir.z, num2);
									VEC_Add(pos, mcl_reuse_toCrossPt, mcl_reuse_crossPt);
									ds.pri.DSTriangle mcl_reuse_triangle = mcl.mcl_reuse_triangle;
									mcl_reuse_triangle.set(vecFx, vecFx2, vecFx3);
									if (ds.pri.PrimitiveTest.insidePointTriangle(mcl_reuse_crossPt, mcl_reuse_triangle))
									{
										ret.hit = 1;
										ret.normal.copy(mcl_reuse_normal);
										ret.length = num2;
										ret.pos.copy(mcl_reuse_crossPt);
										ret.v0.copy(vecFx);
										ret.v1.copy(vecFx2);
										ret.v2.copy(vecFx3);
										ret.material.copy(getMaterial(cPolygonData.getMaterial()));
									}
								}
							}
						}
					}
				}
				return ret.hit != 0;
			}

			public bool evaluateArrowImp(CBlockData pBlock, VecFx32 pos, VecFx32 dir, int len, int mat, CollisionResult ret)
			{
				for (int num = pBlock.getNumberOfPolygon() - 1; num >= 0; num--)
				{
					CPolygonData cPolygonData = const_cast<CPolygonData>(polygons()[pBlock.getPolygonIndex((ushort)num)]);
					if (mat != -1 && !getMaterial(cPolygonData.getMaterial()).getAttribute().isEnableFlag((uint)mat))
					{
						continue;
					}
					VecFx32 mcl_reuse_normal = mcl.mcl_reuse_normal;
					cPolygonData.getNormal(mcl_reuse_normal);
					VEC_Normalize(mcl_reuse_normal, mcl_reuse_normal);
					if (VEC_DotProduct(dir, mcl_reuse_normal) >= 0)
					{
						continue;
					}
					VecFx32 vecFx = points()[cPolygonData.getVertex(0u)];
					VecFx32 vecFx2 = points()[cPolygonData.getVertex(1u)];
					VecFx32 vecFx3 = points()[cPolygonData.getVertex(2u)];
					if (vecFx == null || vecFx2 == null || vecFx3 == null)
					{
						continue;
					}
					VecFx32 mcl_reuse_toEdge = mcl.mcl_reuse_toEdge;
					VecFx32 mcl_reuse_toTri = mcl.mcl_reuse_toTri;
					mcl_reuse_toEdge.x = FX_Mul(dir.x, len);
					mcl_reuse_toEdge.y = FX_Mul(dir.y, len);
					mcl_reuse_toEdge.z = FX_Mul(dir.z, len);
					VEC_Subtract(vecFx, pos, mcl_reuse_toTri);
					int denom = VEC_DotProduct(mcl_reuse_toEdge, mcl_reuse_normal);
					int numer = VEC_DotProduct(mcl_reuse_toTri, mcl_reuse_normal);
					int num2 = FX_Mul32x64c(len, FX_DivFx64c(numer, denom));
					if (num2 < 0 || num2 >= ret.length)
					{
						continue;
					}
					VecFx32 mcl_reuse_toCrossPt = mcl.mcl_reuse_toCrossPt;
					VecFx32 mcl_reuse_crossPt = mcl.mcl_reuse_crossPt;
					mcl_reuse_toCrossPt.x = FX_Mul(dir.x, num2);
					mcl_reuse_toCrossPt.y = FX_Mul(dir.y, num2);
					mcl_reuse_toCrossPt.z = FX_Mul(dir.z, num2);
					VEC_Add(pos, mcl_reuse_toCrossPt, mcl_reuse_crossPt);
					bool flag = false;
					if (ms_checkTwice)
					{
						VecFx32 mcl_reuse_d = mcl_reuse_d0;
						VecFx32 mcl_reuse_d2 = mcl_reuse_d1;
						VecFx32 mcl_reuse_d3 = mcl.mcl_reuse_d2;
						VEC_Subtract(vecFx, pos, mcl_reuse_d);
						VEC_Subtract(vecFx2, pos, mcl_reuse_d2);
						VEC_Subtract(vecFx3, pos, mcl_reuse_d3);
						VecFx32 mcl_reuse_c = mcl_reuse_c0;
						VecFx32 mcl_reuse_c2 = mcl_reuse_c1;
						VecFx32 mcl_reuse_c3 = mcl.mcl_reuse_c2;
						VEC_CrossProduct(mcl_reuse_d, mcl_reuse_d2, mcl_reuse_c);
						VEC_CrossProduct(mcl_reuse_d2, mcl_reuse_d3, mcl_reuse_c2);
						VEC_CrossProduct(mcl_reuse_d3, mcl_reuse_d, mcl_reuse_c3);
						if (VEC_DotProduct(mcl_reuse_c, dir) <= 0 && VEC_DotProduct(mcl_reuse_c2, dir) <= 0 && VEC_DotProduct(mcl_reuse_c3, dir) <= 0)
						{
							flag = true;
						}
					}
					if (!flag)
					{
						ds.pri.DSTriangle mcl_reuse_triangle = mcl.mcl_reuse_triangle;
						mcl_reuse_triangle.set(vecFx, vecFx2, vecFx3);
						if (!ds.pri.PrimitiveTest.insidePointTriangle(mcl_reuse_crossPt, mcl_reuse_triangle))
						{
							continue;
						}
					}
					ret.hit = 1;
					ret.normal.copy(mcl_reuse_normal);
					ret.length = num2;
					ret.pos.copy(mcl_reuse_crossPt);
					ret.v0.copy(vecFx);
					ret.v1.copy(vecFx2);
					ret.v2.copy(vecFx3);
					ret.material.copy(getMaterial(cPolygonData.getMaterial()));
				}
				return ret.hit != 0;
			}

			public bool evaluateArrow2(VecFx32 _position, VecFx32 _direction, int _length, int[] matList, byte matNum, CollisionResult _result)
			{
				_result.clean();
				_result.length = _length;
				int num = 5;
				CBlockData[] mcl_reuse_blockList = mcl.mcl_reuse_blockList;
				byte b = 0;
				for (int i = 0; i < num + 1; i++)
				{
					VecFx32 mcl_reuse_pos = mcl.mcl_reuse_pos;
					VEC_MultAdd(_length * i / num, _direction, _position, mcl_reuse_pos);
					if (m_AABB.evaluatePoint(mcl_reuse_pos))
					{
						CBlockData cBlockData = const_cast<CBlockData>(getBlock(mcl_reuse_pos));
						int num2 = 0;
						for (num2 = 0; num2 < b && mcl_reuse_blockList[num2] != cBlockData; num2++)
						{
						}
						if (num2 >= b)
						{
							mcl_reuse_blockList[b] = cBlockData;
							b++;
						}
					}
				}
				bool result = false;
				for (int j = 0; j < b; j++)
				{
					if (evaluateArrowImp2(mcl_reuse_blockList[j], _position, _direction, _length, matList, matNum, _result))
					{
						result = true;
					}
				}
				return result;
			}

			public bool evaluateArrow(VecFx32 _position, VecFx32 _direction, int _length, int mat, CollisionResult _result)
			{
				_result.clean();
				_result.length = _length;
				int num = 5;
				CBlockData[] mcl_reuse_blockList = mcl.mcl_reuse_blockList;
				byte b = 0;
				for (int i = 0; i < num + 1; i++)
				{
					VecFx32 mcl_reuse_pos = mcl.mcl_reuse_pos;
					VEC_MultAdd(_length * i / num, _direction, _position, mcl_reuse_pos);
					if (m_AABB.evaluatePoint(mcl_reuse_pos))
					{
						CBlockData cBlockData = const_cast<CBlockData>(getBlock(mcl_reuse_pos));
						int num2 = 0;
						for (num2 = 0; num2 < b && mcl_reuse_blockList[num2] != cBlockData; num2++)
						{
						}
						if (num2 >= b)
						{
							mcl_reuse_blockList[b] = cBlockData;
							b++;
						}
					}
				}
				bool result = false;
				for (int j = 0; j < b; j++)
				{
					if (evaluateArrowImp(mcl_reuse_blockList[j], _position, _direction, _length, mat, _result))
					{
						result = true;
					}
				}
				return result;
			}

			public bool evaluateSphereImp2(CBlockData pBlock, VecFx32 _center, VecFx32 prePos, VecFx32 _dir, int _radius, int[] matList, byte matNum, CollisionResult _result)
			{
				VecFx32 mcl_reuse_N = mcl.mcl_reuse_N;
				bool result = false;
				int num = int.MaxValue;
				for (int num2 = pBlock.getNumberOfPolygon() - 1; num2 >= 0; num2--)
				{
					CPolygonData cPolygonData = const_cast<CPolygonData>(polygons()[pBlock.getPolygonIndex((ushort)num2)]);
					byte b = 0;
					if (matNum > 0)
					{
						while (b < matNum && !getMaterial(cPolygonData.getMaterial()).getAttribute().isEnableFlag((uint)matList[b]))
						{
							b++;
						}
					}
					if (b < matNum)
					{
						cPolygonData.getNormal(mcl_reuse_N);
						VEC_Normalize(mcl_reuse_N, mcl_reuse_N);
						if (VEC_DotProduct(_dir, mcl_reuse_N) <= 0)
						{
							VecFx32 vecFx = points()[cPolygonData.getVertex(0u)];
							VecFx32 vecFx2 = points()[cPolygonData.getVertex(1u)];
							VecFx32 vecFx3 = points()[cPolygonData.getVertex(2u)];
							if (vecFx != null && vecFx2 != null && vecFx3 != null)
							{
								ds.pri.DSSphere mcl_reuse_sphere = mcl.mcl_reuse_sphere;
								ds.pri.DSTriangle mcl_reuse_triangle = mcl.mcl_reuse_triangle;
								ds.pri.DSPlane mcl_reuse_plane = mcl.mcl_reuse_plane;
								mcl_reuse_sphere.set(_center, _radius);
								mcl_reuse_triangle.set(vecFx, vecFx2, vecFx3);
								mcl_reuse_plane.set(vecFx, vecFx2, vecFx3);
								if (mcl_reuse_plane.isValidate())
								{
									int _n = ds.pri.PrimitiveTest.distPlanePoint(mcl_reuse_plane, _center);
									if (abs(_n) <= _radius)
									{
										int num3 = 5;
										mcl_reuse_sphere.c.x = FX_Div(mcl_reuse_sphere.c.x, 4096 << num3);
										mcl_reuse_sphere.c.y = FX_Div(mcl_reuse_sphere.c.y, 4096 << num3);
										mcl_reuse_sphere.c.z = FX_Div(mcl_reuse_sphere.c.z, 4096 << num3);
										mcl_reuse_sphere.r = FX_Div(mcl_reuse_sphere.r, 4096 << num3);
										mcl_reuse_triangle.v0.x = FX_Div(mcl_reuse_triangle.v0.x, 4096 << num3);
										mcl_reuse_triangle.v0.y = FX_Div(mcl_reuse_triangle.v0.y, 4096 << num3);
										mcl_reuse_triangle.v0.z = FX_Div(mcl_reuse_triangle.v0.z, 4096 << num3);
										mcl_reuse_triangle.v1.x = FX_Div(mcl_reuse_triangle.v1.x, 4096 << num3);
										mcl_reuse_triangle.v1.y = FX_Div(mcl_reuse_triangle.v1.y, 4096 << num3);
										mcl_reuse_triangle.v1.z = FX_Div(mcl_reuse_triangle.v1.z, 4096 << num3);
										mcl_reuse_triangle.v2.x = FX_Div(mcl_reuse_triangle.v2.x, 4096 << num3);
										mcl_reuse_triangle.v2.y = FX_Div(mcl_reuse_triangle.v2.y, 4096 << num3);
										mcl_reuse_triangle.v2.z = FX_Div(mcl_reuse_triangle.v2.z, 4096 << num3);
										int[] array = new int[1];
										int[] sqLen = array;
										if (ds.pri.PrimitiveTest.testSphereTriangle(mcl_reuse_sphere, mcl_reuse_triangle, sqLen, null))
										{
											ds.pri.DSPlane m = new ds.pri.DSPlane(vecFx, vecFx2, vecFx3);
											int _n2 = ds.pri.PrimitiveTest.distPlanePoint(m, prePos);
											int num4 = abs(_n2);
											if (num4 < num)
											{
												num = num4;
												result = true;
												_result.hit = 1;
												_result.normal.copy(mcl_reuse_N);
												_result.length = ds.pri.PrimitiveTest.distPlanePoint(m, _center);
												_result.material.copy(getMaterial(cPolygonData.getMaterial()));
											}
										}
									}
								}
							}
						}
					}
				}
				return result;
			}

			public bool evaluateSphereImp(CBlockData pBlock, VecFx32 _center, VecFx32 _dir, int _radius, int mat, CollisionResult _result)
			{
				VecFx32 mcl_reuse_N = mcl.mcl_reuse_N;
				bool flag = false;
				int num = 0;
				for (int num2 = pBlock.getNumberOfPolygon() - 1; num2 >= 0; num2--)
				{
					CPolygonData cPolygonData = const_cast<CPolygonData>(polygons()[pBlock.getPolygonIndex((ushort)num2)]);
					if (-1 == mat || getMaterial(cPolygonData.getMaterial()).getAttribute().isEnableFlag((uint)mat))
					{
						cPolygonData.getNormal(mcl_reuse_N);
						VEC_Normalize(mcl_reuse_N, mcl_reuse_N);
						if (VEC_DotProduct(_dir, mcl_reuse_N) <= 0)
						{
							VecFx32 vecFx = points()[cPolygonData.getVertex(0u)];
							VecFx32 vecFx2 = points()[cPolygonData.getVertex(1u)];
							VecFx32 vecFx3 = points()[cPolygonData.getVertex(2u)];
							if (vecFx != null && vecFx2 != null && vecFx3 != null)
							{
								ds.pri.DSSphere mcl_reuse_sphere = mcl.mcl_reuse_sphere;
								ds.pri.DSTriangle mcl_reuse_triangle = mcl.mcl_reuse_triangle;
								ds.pri.DSPlane mcl_reuse_plane = mcl.mcl_reuse_plane;
								mcl_reuse_sphere.set(_center, _radius);
								mcl_reuse_triangle.set(vecFx, vecFx2, vecFx3);
								mcl_reuse_plane.set(vecFx, vecFx2, vecFx3);
								if (mcl_reuse_plane.isValidate())
								{
									int _n = ds.pri.PrimitiveTest.distPlanePoint(mcl_reuse_plane, _center);
									if (abs(_n) <= _radius && ds.pri.PrimitiveTest.testSphereHalfSpace(mcl_reuse_sphere, mcl_reuse_plane))
									{
										int num3 = 5;
										mcl_reuse_sphere.c.x = FX_Div(mcl_reuse_sphere.c.x, 4096 << num3);
										mcl_reuse_sphere.c.y = FX_Div(mcl_reuse_sphere.c.y, 4096 << num3);
										mcl_reuse_sphere.c.z = FX_Div(mcl_reuse_sphere.c.z, 4096 << num3);
										mcl_reuse_sphere.r = FX_Div(mcl_reuse_sphere.r, 4096 << num3);
										mcl_reuse_triangle.v0.x = FX_Div(mcl_reuse_triangle.v0.x, 4096 << num3);
										mcl_reuse_triangle.v0.y = FX_Div(mcl_reuse_triangle.v0.y, 4096 << num3);
										mcl_reuse_triangle.v0.z = FX_Div(mcl_reuse_triangle.v0.z, 4096 << num3);
										mcl_reuse_triangle.v1.x = FX_Div(mcl_reuse_triangle.v1.x, 4096 << num3);
										mcl_reuse_triangle.v1.y = FX_Div(mcl_reuse_triangle.v1.y, 4096 << num3);
										mcl_reuse_triangle.v1.z = FX_Div(mcl_reuse_triangle.v1.z, 4096 << num3);
										mcl_reuse_triangle.v2.x = FX_Div(mcl_reuse_triangle.v2.x, 4096 << num3);
										mcl_reuse_triangle.v2.y = FX_Div(mcl_reuse_triangle.v2.y, 4096 << num3);
										mcl_reuse_triangle.v2.z = FX_Div(mcl_reuse_triangle.v2.z, 4096 << num3);
										int[] array = new int[1];
										int[] sqLen = array;
										if (ds.pri.PrimitiveTest.testSphereTriangle(mcl_reuse_sphere, mcl_reuse_triangle, sqLen, null))
										{
											flag = true;
											num += 4096;
											_result.hit = 1;
											VEC_Add(mcl_reuse_N, _result.normal, _result.normal);
											ds.pri.DSPlane m = new ds.pri.DSPlane(vecFx, vecFx2, vecFx3);
											_result.length += ds.pri.PrimitiveTest.distPlanePoint(m, _center);
											_result.material.copy(getMaterial(cPolygonData.getMaterial()));
										}
									}
								}
							}
						}
					}
				}
				if (flag)
				{
					_result.normal.x = FX_Div(_result.normal.x, num);
					_result.normal.y = FX_Div(_result.normal.y, num);
					_result.normal.z = FX_Div(_result.normal.z, num);
					VEC_Normalize(_result.normal, _result.normal);
					_result.length = FX_Div(_result.length, num);
				}
				return flag;
			}

			public bool evaluateSphere2(VecFx32 _center, VecFx32 prePos, VecFx32 _dir, int _radius, int[] matList, byte matNum, CollisionResult _result)
			{
				_result.clean();
				byte b = 0;
				CBlockData[] mcl_reuse_blockList = mcl.mcl_reuse_blockList;
				VecFx32[] mcl_reuse_vecList = mcl.mcl_reuse_vecList;
				for (int i = 0; i < 8; i++)
				{
					CBlockData cBlockData = null;
					VecFx32 mcl_reuse_pos = mcl.mcl_reuse_pos;
					VEC_MultAdd(_radius + FX_Mul(_radius, 2048), mcl_reuse_vecList[i], _center, mcl_reuse_pos);
					if (m_AABB.evaluatePoint(mcl_reuse_pos))
					{
						cBlockData = const_cast<CBlockData>(getBlock(mcl_reuse_pos));
						int num = 0;
						for (num = 0; num < b && mcl_reuse_blockList[num] != cBlockData; num++)
						{
						}
						if (num >= b)
						{
							mcl_reuse_blockList[b] = cBlockData;
							b++;
						}
					}
				}
				for (int j = 0; j < b; j++)
				{
					if (evaluateSphereImp2(mcl_reuse_blockList[j], _center, prePos, _dir, _radius, matList, matNum, _result))
					{
						return true;
					}
				}
				return false;
			}

			public bool evaluateSphere(VecFx32 _center, VecFx32 _dir, int _radius, int mat, CollisionResult _result)
			{
				_result.clean();
				byte b = 0;
				CBlockData[] mcl_reuse_blockList = mcl.mcl_reuse_blockList;
				VecFx32[] mcl_reuse_vecList = mcl.mcl_reuse_vecList;
				for (int i = 0; i < 8; i++)
				{
					CBlockData cBlockData = null;
					VecFx32 mcl_reuse_pos = mcl.mcl_reuse_pos;
					VEC_MultAdd(_radius + FX_Mul(_radius, 2048), mcl_reuse_vecList[i], _center, mcl_reuse_pos);
					if (m_AABB.evaluatePoint(mcl_reuse_pos))
					{
						cBlockData = const_cast<CBlockData>(getBlock(mcl_reuse_pos));
						int num = 0;
						for (num = 0; num < b && mcl_reuse_blockList[num] != cBlockData; num++)
						{
						}
						if (num >= b)
						{
							mcl_reuse_blockList[b] = cBlockData;
							b++;
						}
					}
				}
				for (int j = 0; j < b; j++)
				{
					if (evaluateSphereImp(mcl_reuse_blockList[j], _center, _dir, _radius, mat, _result))
					{
						return true;
					}
				}
				return false;
			}

			public bool evaluateSegmentImp(CBlockData pBlock, VecFx32 pos, VecFx32 nextPos, int mat, CollisionResult ret)
			{
				int num = int.MaxValue;
				for (int num2 = pBlock.getNumberOfPolygon() - 1; num2 >= 0; num2--)
				{
					CPolygonData cPolygonData = const_cast<CPolygonData>(polygons()[pBlock.getPolygonIndex((ushort)num2)]);
					if (-1 == mat || getMaterial(cPolygonData.getMaterial()).getAttribute().isEnableFlag((uint)mat))
					{
						VecFx32 mcl_reuse_normal = mcl.mcl_reuse_normal;
						cPolygonData.getNormal(mcl_reuse_normal);
						VEC_Normalize(mcl_reuse_normal, mcl_reuse_normal);
						VecFx32 mcl_reuse_pos = mcl.mcl_reuse_pos;
						VEC_Subtract(nextPos, pos, mcl_reuse_pos);
						if (VEC_DotProduct(mcl_reuse_pos, mcl_reuse_normal) < 0)
						{
							VecFx32 p = points()[cPolygonData.getVertex(0u)];
							VecFx32 p2 = points()[cPolygonData.getVertex(1u)];
							VecFx32 p3 = points()[cPolygonData.getVertex(2u)];
							ds.pri.DSTriangle mcl_reuse_triangle = mcl.mcl_reuse_triangle;
							mcl_reuse_triangle.set(p, p2, p3, mcl_reuse_normal);
							ds.pri.DSSegment seg = new ds.pri.DSSegment(pos, nextPos);
							if (ds.pri.PrimitiveTest.testSegmentTriangle(seg, mcl_reuse_triangle, null))
							{
								ds.pri.DSPlane m = new ds.pri.DSPlane(mcl_reuse_triangle.v0, mcl_reuse_triangle.v1, mcl_reuse_triangle.v2);
								int num3 = ds.pri.PrimitiveTest.distPlanePoint(m, pos);
								if (num3 < num)
								{
									ret.hit = 1;
									ret.normal.copy(mcl_reuse_triangle.n);
									ret.length = ds.pri.PrimitiveTest.distPlanePoint(m, nextPos);
								}
							}
						}
					}
				}
				return ret.hit != 0;
			}

			public bool evaluateCapsuleImp(CBlockData pBlock, VecFx32 pos, VecFx32 nextPos, VecFx32 dir, int len, int sqRad, int rad, int mat, CollisionResult ret)
			{
				int num = 0;
				for (int num2 = pBlock.getNumberOfPolygon() - 1; num2 >= 0; num2--)
				{
					bool flag = false;
					CPolygonData cPolygonData = const_cast<CPolygonData>(polygons()[pBlock.getPolygonIndex((ushort)num2)]);
					if (-1 == mat || getMaterial(cPolygonData.getMaterial()).getAttribute().isEnableFlag((uint)mat))
					{
						VecFx32 mcl_reuse_normal = mcl.mcl_reuse_normal;
						cPolygonData.getNormal(mcl_reuse_normal);
						VEC_Normalize(mcl_reuse_normal, mcl_reuse_normal);
						if (VEC_DotProduct(dir, mcl_reuse_normal) <= 0)
						{
							VecFx32 vecFx = points()[cPolygonData.getVertex(0u)];
							VecFx32 vecFx2 = points()[cPolygonData.getVertex(1u)];
							VecFx32 vecFx3 = points()[cPolygonData.getVertex(2u)];
							if (vecFx != null && vecFx2 != null && vecFx3 != null)
							{
								ds.pri.DSTriangle mcl_reuse_triangle = mcl.mcl_reuse_triangle;
								mcl_reuse_triangle.set(vecFx, vecFx2, vecFx3, mcl_reuse_normal);
								ds.pri.DSSegment dSSegment = new ds.pri.DSSegment(pos, nextPos);
								ds.pri.DSPlane m = new ds.pri.DSPlane(vecFx, vecFx2, vecFx3);
								ds.pri.DSSphere mcl_reuse_sphere = mcl.mcl_reuse_sphere;
								ds.pri.DSPlane mcl_reuse_plane = mcl.mcl_reuse_plane;
								ds.pri.DSTriangle mcl_reuse_triCopy = mcl.mcl_reuse_triCopy;
								mcl_reuse_sphere.set(nextPos, rad);
								mcl_reuse_plane.set(mcl_reuse_triangle.v0, mcl_reuse_triangle.v1, mcl_reuse_triangle.v2);
								if (ds.pri.PrimitiveTest.testSphereHalfSpace(mcl_reuse_sphere, mcl_reuse_plane))
								{
									int num3 = 5;
									mcl_reuse_sphere.c.x = FX_Div(mcl_reuse_sphere.c.x, 4096 << num3);
									mcl_reuse_sphere.c.y = FX_Div(mcl_reuse_sphere.c.y, 4096 << num3);
									mcl_reuse_sphere.c.z = FX_Div(mcl_reuse_sphere.c.z, 4096 << num3);
									mcl_reuse_sphere.r = FX_Div(mcl_reuse_sphere.r, 4096 << num3);
									mcl_reuse_triCopy.v0.x = FX_Div(mcl_reuse_triangle.v0.x, 4096 << num3);
									mcl_reuse_triCopy.v0.y = FX_Div(mcl_reuse_triangle.v0.y, 4096 << num3);
									mcl_reuse_triCopy.v0.z = FX_Div(mcl_reuse_triangle.v0.z, 4096 << num3);
									mcl_reuse_triCopy.v1.x = FX_Div(mcl_reuse_triangle.v1.x, 4096 << num3);
									mcl_reuse_triCopy.v1.y = FX_Div(mcl_reuse_triangle.v1.y, 4096 << num3);
									mcl_reuse_triCopy.v1.z = FX_Div(mcl_reuse_triangle.v1.z, 4096 << num3);
									mcl_reuse_triCopy.v2.x = FX_Div(mcl_reuse_triangle.v2.x, 4096 << num3);
									mcl_reuse_triCopy.v2.y = FX_Div(mcl_reuse_triangle.v2.y, 4096 << num3);
									mcl_reuse_triCopy.v2.z = FX_Div(mcl_reuse_triangle.v2.z, 4096 << num3);
									if (ds.pri.PrimitiveTest.testSphereTriangle(mcl_reuse_sphere, mcl_reuse_triCopy, null, null))
									{
										flag = true;
									}
								}
								else if (ds.pri.PrimitiveTest.testSegmentTriangle(dSSegment, mcl_reuse_triangle, null))
								{
									flag = true;
								}
								else
								{
									VecFx32 mcl_reuse_c = mcl_reuse_c1;
									VecFx32 mcl_reuse_c2 = mcl.mcl_reuse_c2;
									int s = 0;
									int t = 0;
									int num4 = 0;
									ds.pri.DSSegment[] array = new ds.pri.DSSegment[3];
									array[0].p1.copy(mcl_reuse_triangle.v0);
									array[0].p2.copy(mcl_reuse_triangle.v1);
									array[1].p1.copy(mcl_reuse_triangle.v1);
									array[1].p2.copy(mcl_reuse_triangle.v2);
									array[2].p1.copy(mcl_reuse_triangle.v2);
									array[2].p2.copy(mcl_reuse_triangle.v0);
									for (int i = 0; i < 3; i++)
									{
										num4 = ds.pri.PrimitiveTest.closestPtSegmentSegment(dSSegment, array[i], ref s, ref t, mcl_reuse_c, mcl_reuse_c2);
										if (num4 < sqRad)
										{
											flag = true;
											break;
										}
									}
								}
								if (flag)
								{
									num += 4096;
									ret.hit = 1;
									VEC_Add(mcl_reuse_triangle.n, ret.normal, ret.normal);
									int num5 = ds.pri.PrimitiveTest.distPlanePoint(m, nextPos);
									ret.length += num5;
									VEC_MultAdd(-num5, mcl_reuse_triangle.n, ret.pos, ret.pos);
								}
							}
						}
					}
				}
				if (ret.hit != 0)
				{
					ret.normal.x = FX_Div(ret.normal.x, num);
					ret.normal.y = FX_Div(ret.normal.y, num);
					ret.normal.z = FX_Div(ret.normal.z, num);
					VEC_Normalize(ret.normal, ret.normal);
					ret.length = FX_Div(ret.length, num);
				}
				return ret.hit != 0;
			}

			public bool evaluateCapsule(VecFx32 pos, VecFx32 nextPos, int rad, int mat, CollisionResult ret)
			{
				ret.clean();
				ret.pos.copy(nextPos);
				VecFx32 mcl_reuse_N = mcl.mcl_reuse_N;
				int num = 0;
				int sqRad = FX_Mul(rad, rad);
				VEC_Subtract(nextPos, pos, mcl_reuse_N);
				num = VEC_Mag(mcl_reuse_N);
				VEC_Normalize(mcl_reuse_N, mcl_reuse_N);
				byte b = 0;
				CBlockData[] mcl_reuse_blockList = mcl.mcl_reuse_blockList;
				VecFx32[] mcl_reuse_vecList = mcl.mcl_reuse_vecList;
				if (m_AABB.evaluatePoint(pos))
				{
					mcl_reuse_blockList[b] = const_cast<CBlockData>(getBlock(pos));
					b++;
				}
				for (int i = 0; i < 8; i++)
				{
					CBlockData cBlockData = null;
					VecFx32 mcl_reuse_pos = mcl.mcl_reuse_pos;
					VEC_MultAdd(rad + FX_Mul(rad, 2048), mcl_reuse_vecList[i], nextPos, mcl_reuse_pos);
					if (m_AABB.evaluatePoint(mcl_reuse_pos))
					{
						cBlockData = const_cast<CBlockData>(getBlock(mcl_reuse_pos));
						int num2 = 0;
						for (num2 = 0; num2 < b && mcl_reuse_blockList[num2] != cBlockData; num2++)
						{
						}
						if (num2 >= b)
						{
							mcl_reuse_blockList[b] = cBlockData;
							b++;
						}
					}
				}
				for (int j = 0; j < b; j++)
				{
					if (evaluateCapsuleImp(mcl_reuse_blockList[j], pos, nextPos, mcl_reuse_N, num, sqRad, rad, mat, ret))
					{
						return true;
					}
				}
				return false;
			}

			public CMaterialData getMaterial(uint uiIndex)
			{
				return m_acMaterial[uiIndex];
			}

			public uint getNumberOfMaterial()
			{
				return m_uiMaterialNum;
			}

			public uint numPoints()
			{
				return m_uiPointNum;
			}

			public VecFx32[] points()
			{
				return m_Points;
			}

			public uint numPolygons()
			{
				return m_cPolyTable.getNumberOfPolygon();
			}

			public CPolygonData[] polygons()
			{
				return m_cPolyTable.getPolygon();
			}

			public uint numMaterials()
			{
				return m_uiPointNum;
			}

			public CMaterialData[] materials()
			{
				return m_acMaterial;
			}

			public static void setCheckTwice(bool check)
			{
				ms_checkTwice = check;
			}

			public static explicit operator CObject(ArrayReader src)
			{
				CObject cObject = new CObject();
				byte[] array = new byte[msc_uiNameLength];
				cObject.m_fvBoxMin = new TVector3<int>();
				src.read(array, 0, array.Length);
				cObject.m_strName = StringUtil.createString(array);
				cObject.m_cPolyTable = (CPolygonTableData)src;
				cObject.m_cBlockTable = (CBlockTableData)src;
				uint num = src.readUInt32();
				cObject.m_uiPointNum = src.readUInt32();
				uint num2 = src.readUInt32();
				cObject.m_uiMaterialNum = src.readUInt32();
				cObject.m_fvBoxMin.x = src.readInt32();
				cObject.m_fvBoxMin.y = src.readInt32();
				cObject.m_fvBoxMin.z = src.readInt32();
				cObject.padding[0] = src.readUInt32();
				cObject.padding[1] = src.readUInt32();
				cObject.padding[2] = src.readUInt32();
				cObject.m_AABB = (AABB)src;
				long position = src.getPosition();
				src.setPosition(src.getMark() + num);
				cObject.m_Points = new VecFx32[cObject.m_uiPointNum];
				for (int i = 0; i < cObject.m_uiPointNum; i++)
				{
					cObject.m_Points[i] = new VecFx32();
					cObject.m_Points[i].x = src.readInt32();
					cObject.m_Points[i].y = src.readInt32();
					cObject.m_Points[i].z = src.readInt32();
					src.readInt32();
				}
				src.setPosition(src.getMark() + num2);
				cObject.m_acMaterial = new CMaterialData[cObject.m_uiMaterialNum];
				for (int i = 0; i < cObject.m_uiMaterialNum; i++)
				{
					cObject.m_acMaterial[i] = (CMaterialData)src;
				}
				src.setPosition(position);
				return cObject;
			}
		}

		public class AABB
		{
			public VecFx32 m_fxv4Min;

			public VecFx32 m_fxv4Max;

			public bool evaluatePoint(VecFx32 _point)
			{
				if (m_fxv4Min.vx <= _point.x && _point.x <= m_fxv4Max.vx && m_fxv4Min.vy <= _point.y && _point.y <= m_fxv4Max.vy && m_fxv4Min.vz <= _point.z && _point.z <= m_fxv4Max.vz)
				{
					return true;
				}
				return false;
			}

			public bool evaluateSphere(VecFx32 _center, int _radius)
			{
				int num = sqDistPoint(_center);
				return num <= FX_Mul(_radius, _radius);
			}

			public int sqDistPoint(VecFx32 _point)
			{
				int num = 0;
				if (_point.x < m_fxv4Min.vx)
				{
					num += FX_Mul(m_fxv4Min.vx - _point.x, m_fxv4Min.vx - _point.x);
				}
				if (_point.x > m_fxv4Max.vx)
				{
					num += FX_Mul(_point.x - m_fxv4Max.vx, _point.x - m_fxv4Max.vx);
				}
				if (_point.y < m_fxv4Min.vy)
				{
					num += FX_Mul(m_fxv4Min.vy - _point.y, m_fxv4Min.vy - _point.y);
				}
				if (_point.y > m_fxv4Max.vy)
				{
					num += FX_Mul(_point.y - m_fxv4Max.vy, _point.y - m_fxv4Max.vy);
				}
				if (_point.z < m_fxv4Min.vz)
				{
					num += FX_Mul(m_fxv4Min.vz - _point.z, m_fxv4Min.vz - _point.z);
				}
				if (_point.z > m_fxv4Max.vz)
				{
					num += FX_Mul(_point.z - m_fxv4Max.vz, _point.z - m_fxv4Max.vz);
				}
				return num;
			}

			public static explicit operator AABB(ArrayReader src)
			{
				AABB aABB = new AABB();
				aABB.m_fxv4Min = new VecFx32();
				aABB.m_fxv4Max = new VecFx32();
				aABB.m_fxv4Min.vx = src.readInt32();
				aABB.m_fxv4Min.vy = src.readInt32();
				aABB.m_fxv4Min.vz = src.readInt32();
				src.readInt32();
				aABB.m_fxv4Max.vx = src.readInt32();
				aABB.m_fxv4Max.vy = src.readInt32();
				aABB.m_fxv4Max.vz = src.readInt32();
				src.readInt32();
				return aABB;
			}
		}

		public class CAttributeData
		{
			public static uint[] msc_auiMask16 = new uint[2] { 65535u, 4294901760u };

			public static uint[] msc_auiMask8 = new uint[4] { 255u, 65280u, 16711680u, 4278190080u };

			public static int msc_uiAttributeMax = (int)TYPE_MAX;

			public static int msc_uiAttributeArrayNum = msc_uiAttributeMax + 31 >> 5;

			protected uint[] m_auiFlags = new uint[msc_uiAttributeArrayNum];

			public bool isEnableFlag(uint uiIndex)
			{
				if (((m_auiFlags[uiIndex >> 5] >> (int)uiIndex) & 1) == 0)
				{
					return false;
				}
				return true;
			}

			public uint get32(uint uiPos)
			{
				return m_auiFlags[uiPos];
			}

			public uint get16(uint uiPos, uint uiPos2)
			{
				return m_auiFlags[uiPos] & msc_auiMask16[uiPos2];
			}

			public uint get8(uint uiPos, uint uiPos2)
			{
				return m_auiFlags[uiPos] & msc_auiMask8[uiPos2];
			}

			public void copy(CAttributeData src)
			{
				memcpy(m_auiFlags, src.m_auiFlags, msc_uiAttributeArrayNum * 4);
			}

			public void parse(ArrayReader reader)
			{
				reader.read(m_auiFlags, 0, msc_uiAttributeArrayNum);
			}
		}

		public class CPolygonTableData
		{
			protected CPolygonData[] m_acPolygon;

			protected uint m_uiPolygonNum;

			public void initialize()
			{
			}

			public CPolygonData getPolygon(ushort usPolyIndex)
			{
				return m_acPolygon[usPolyIndex];
			}

			public CPolygonData[] getPolygon()
			{
				return m_acPolygon;
			}

			public ushort getNumberOfPolygon()
			{
				return (ushort)m_uiPolygonNum;
			}

			public static explicit operator CPolygonTableData(ArrayReader src)
			{
				CPolygonTableData cPolygonTableData = new CPolygonTableData();
				uint num = src.readUInt32();
				cPolygonTableData.m_uiPolygonNum = src.readUInt32();
				long position = src.getPosition();
				src.setPosition(src.getMark() + num);
				cPolygonTableData.m_acPolygon = new CPolygonData[cPolygonTableData.m_uiPolygonNum];
				for (int i = 0; i < cPolygonTableData.m_uiPolygonNum; i++)
				{
					cPolygonTableData.m_acPolygon[i] = (CPolygonData)src;
				}
				src.setPosition(position);
				return cPolygonTableData;
			}
		}

		public class CBlockData
		{
			protected ushort[] m_ausPolyIndex;

			protected uint m_uiPolyIndexNum;

			public void initialize()
			{
			}

			public ushort getPolygonIndex(ushort usIndexIndex)
			{
				return m_ausPolyIndex[usIndexIndex];
			}

			public ushort getNumberOfPolygon()
			{
				return (ushort)m_uiPolyIndexNum;
			}

			public static explicit operator CBlockData(ArrayReader src)
			{
				CBlockData cBlockData = new CBlockData();
				uint num = src.readUInt32();
				cBlockData.m_uiPolyIndexNum = src.readUInt32();
				long position = src.getPosition();
				src.setPosition(src.getMark() + num);
				cBlockData.m_ausPolyIndex = new ushort[cBlockData.m_uiPolyIndexNum];
				src.read(cBlockData.m_ausPolyIndex, 0, (int)cBlockData.m_uiPolyIndexNum);
				src.setPosition(position);
				return cBlockData;
			}
		}

		public class CBlockTableData
		{
			protected CBlockData[] m_acBlock;

			public VecFx32 m_vfxBlockSize;

			public TVector3<ushort> m_usvBlockNum;

			protected ushort _pad0;

			private ushort m_usBlockNum_YxZ;

			public void initialize()
			{
				for (int i = 0; i < m_usvBlockNum.x * m_usvBlockNum.y * m_usvBlockNum.z; i++)
				{
					m_acBlock[i].initialize();
				}
				m_usBlockNum_YxZ = (ushort)(m_usvBlockNum.y * m_usvBlockNum.z);
			}

			public CBlockData getBlock(TVector3<ushort> usvPos)
			{
				return m_acBlock[usvPos.x * m_usBlockNum_YxZ + usvPos.y * m_usvBlockNum.z + usvPos.z];
			}

			public CBlockData getBlock(ushort x, ushort y, ushort z)
			{
				return m_acBlock[x * m_usBlockNum_YxZ + y * m_usvBlockNum.z + z];
			}

			public TVector3<ushort> getNumberOfBlock()
			{
				return m_usvBlockNum;
			}

			public static explicit operator CBlockTableData(ArrayReader src)
			{
				CBlockTableData cBlockTableData = new CBlockTableData();
				cBlockTableData.m_vfxBlockSize = new VecFx32();
				cBlockTableData.m_usvBlockNum = new TVector3<ushort>();
				uint num = src.readUInt32();
				cBlockTableData.m_vfxBlockSize.parse(src);
				cBlockTableData.m_usvBlockNum.x = src.readUInt16();
				cBlockTableData.m_usvBlockNum.y = src.readUInt16();
				cBlockTableData.m_usvBlockNum.z = src.readUInt16();
				cBlockTableData._pad0 = src.readUInt16();
				long position = src.getPosition();
				src.setPosition(src.getMark() + num);
				int num2 = cBlockTableData.m_usvBlockNum.x * cBlockTableData.m_usvBlockNum.y * cBlockTableData.m_usvBlockNum.z;
				cBlockTableData.m_acBlock = new CBlockData[num2];
				for (int i = 0; i < num2; i++)
				{
					cBlockTableData.m_acBlock[i] = (CBlockData)src;
				}
				src.setPosition(position);
				return cBlockTableData;
			}
		}

		public class CMapCollision
		{
			protected static int msc_iFileType = 541868877;

			protected static int msc_iFileVersion = 1280;

			protected static uint msc_uiIntializedFlag = 2147483648u;

			protected uint m_uiType;

			protected uint m_uiVersion;

			protected CObject[] m_acObject;

			protected uint m_uiObjectNum;

			protected AABB m_AABB;

			public void initialize()
			{
				if ((m_uiVersion & msc_uiIntializedFlag) == 0)
				{
					for (int i = 0; i < m_uiObjectNum; i++)
					{
						m_acObject[i].initialize();
					}
					m_uiVersion |= msc_uiIntializedFlag;
				}
			}

			public CObject getObject(uint uiIndex)
			{
				return m_acObject[uiIndex];
			}

			public uint getNumberOfObject()
			{
				return m_uiObjectNum;
			}

			public static CMapCollision cast(Array src)
			{
				CMapCollision cMapCollision = new CMapCollision();
				ArrayReader arrayReader = new ArrayReader(src);
				cMapCollision.m_uiType = arrayReader.readUInt32();
				cMapCollision.m_uiVersion = arrayReader.readUInt32();
				uint num = arrayReader.readUInt32();
				cMapCollision.m_uiObjectNum = arrayReader.readUInt32();
				cMapCollision.m_AABB = (AABB)arrayReader;
				arrayReader.setPosition(num);
				cMapCollision.m_acObject = new CObject[cMapCollision.m_uiObjectNum];
				for (int i = 0; i < cMapCollision.m_uiObjectNum; i++)
				{
					cMapCollision.m_acObject[i] = (CObject)arrayReader;
				}
				arrayReader.dispose();
				return cMapCollision;
			}

			public static CMapCollision[] ChainPointer(byte[] abyData, int iId)
			{
				ArrayReader arrayReader = new ArrayReader(abyData);
				arrayReader.skip(16L);
				arrayReader.skip(8 * iId);
				uint num = arrayReader.readUInt32();
				arrayReader.readUInt32();
				uint num2 = 1u;
				arrayReader.setPosition(num);
				arrayReader.setMark(num);
				CMapCollision[] array = new CMapCollision[num2];
				for (int i = 0; i < num2; i++)
				{
					array[i] = new CMapCollision();
					array[i].parse(arrayReader);
				}
				return array;
			}

			public void parse(ArrayReader reader)
			{
				m_uiType = reader.readUInt32();
				m_uiVersion = reader.readUInt32();
				uint num = reader.readUInt32();
				m_uiObjectNum = reader.readUInt32();
				m_AABB = (AABB)reader;
				reader.setPosition(reader.getMark() + num);
				m_acObject = new CObject[m_uiObjectNum];
				for (int i = 0; i < m_uiObjectNum; i++)
				{
					m_acObject[i] = (CObject)reader;
				}
			}
		}

		public class QuatFx32
		{
			public int x;

			public int y;

			public int z;

			public int w;

			public static explicit operator QuatFx32(ArrayReader src)
			{
				QuatFx32 quatFx = new QuatFx32();
				quatFx.x = src.readInt32();
				quatFx.y = src.readInt32();
				quatFx.z = src.readInt32();
				quatFx.w = src.readInt32();
				return quatFx;
			}
		}

		public class TVector3<T>
		{
			public T x;

			public T y;

			public T z;

			public T w
			{
				get
				{
					return x;
				}
				set
				{
					x = value;
				}
			}

			public T width
			{
				get
				{
					return x;
				}
				set
				{
					x = value;
				}
			}

			public T h
			{
				get
				{
					return y;
				}
				set
				{
					y = value;
				}
			}

			public T height
			{
				get
				{
					return y;
				}
				set
				{
					y = value;
				}
			}

			public T d
			{
				get
				{
					return z;
				}
				set
				{
					z = value;
				}
			}

			public T depth
			{
				get
				{
					return z;
				}
				set
				{
					z = value;
				}
			}

			public void set(T _x, T _y, T _z)
			{
				x = _x;
				y = _y;
				z = _z;
			}
		}

		public class CMaterialData
		{
			protected CAttributeData m_Attribute = new CAttributeData();

			public CAttributeData getAttribute()
			{
				return m_Attribute;
			}

			public static explicit operator CMaterialData(ArrayReader src)
			{
				CMaterialData cMaterialData = new CMaterialData();
				cMaterialData.m_Attribute.parse(src);
				return cMaterialData;
			}

			public void copy(CMaterialData src)
			{
				m_Attribute.copy(src.m_Attribute);
			}
		}

		public class CPolygonData
		{
			protected ushort[] m_aiVertex = new ushort[3];

			protected ushort m_uiMaterial;

			protected ds.Vector4<int> m_fxvNormal;

			public ushort getMaterial()
			{
				return m_uiMaterial;
			}

			public ushort getVertex(uint uiIndex)
			{
				return m_aiVertex[uiIndex];
			}

			public void getNormal(VecFx32 _fxvNormal)
			{
				_fxvNormal.x = m_fxvNormal.vx;
				_fxvNormal.y = m_fxvNormal.vy;
				_fxvNormal.z = m_fxvNormal.vz;
			}

			public static explicit operator CPolygonData(ArrayReader src)
			{
				CPolygonData cPolygonData = new CPolygonData();
				cPolygonData.m_fxvNormal = new ds.Vector4<int>();
				src.read(cPolygonData.m_aiVertex, 0, 3);
				cPolygonData.m_uiMaterial = src.readUInt16();
				cPolygonData.m_fxvNormal.vx = src.readInt32();
				cPolygonData.m_fxvNormal.vy = src.readInt32();
				cPolygonData.m_fxvNormal.vz = src.readInt32();
				cPolygonData.m_fxvNormal.vw = src.readInt32();
				return cPolygonData;
			}
		}

		public class CollisionResult
		{
			public byte hit;

			public VecFx32 normal = new VecFx32();

			public VecFx32 pos = new VecFx32();

			public int length;

			public CMaterialData material = new CMaterialData();

			public VecFx32 v0 = new VecFx32();

			public VecFx32 v1 = new VecFx32();

			public VecFx32 v2 = new VecFx32();

			public void clean()
			{
				hit = 0;
				VEC_Set(normal, 0, 0, 0);
				VEC_Set(pos, 0, 0, 0);
				length = 0;
				VEC_Set(v0, 0, 0, 0);
				VEC_Set(v1, 0, 0, 0);
				VEC_Set(v2, 0, 0, 0);
			}

			public void copy(CollisionResult src)
			{
				hit = src.hit;
				normal.copy(src.normal);
				pos.copy(src.pos);
				length = src.length;
				material.copy(src.material);
				v0.copy(src.v0);
				v1.copy(src.v1);
				v2.copy(src.v2);
			}
		}

		public enum MCL_ATTRIBUTE
		{
			ATTRIBUTE_NONE,
			ATTRIBUTE_GROUND,
			ATTRIBUTE_WALL_01,
			ATTRIBUTE_WALL_02,
			ATTRIBUTE_WALL_03,
			ATTRIBUTE_WALL_04,
			ATTRIBUTE_WALL_05,
			ATTRIBUTE_CAMERA,
			ATTRIBUTE_LANDFORM_01,
			ATTRIBUTE_LANDFORM_02,
			ATTRIBUTE_LANDFORM_03,
			ATTRIBUTE_LANDFORM_04,
			ATTRIBUTE_LANDFORM_05,
			ATTRIBUTE_LANDFORM_06,
			ATTRIBUTE_LANDFORM_07,
			ATTRIBUTE_LANDFORM_08,
			ATTRIBUTE_LANDFORM_09,
			ATTRIBUTE_LANDFORM_10,
			ATTRIBUTE_LANDFORM_11,
			ATTRIBUTE_LANDFORM_12,
			ATTRIBUTE_MONSTER_01,
			ATTRIBUTE_MONSTER_02,
			ATTRIBUTE_MONSTER_03,
			ATTRIBUTE_MONSTER_04,
			ATTRIBUTE_MONSTER_05,
			ATTRIBUTE_MAPJUMP_01,
			ATTRIBUTE_MAPJUMP_02,
			ATTRIBUTE_MAPJUMP_03,
			ATTRIBUTE_MAPJUMP_04,
			ATTRIBUTE_MAPJUMP_05,
			ATTRIBUTE_MAPJUMP_06,
			ATTRIBUTE_MAPJUMP_07,
			ATTRIBUTE_MAPJUMP_08,
			ATTRIBUTE_MAPJUMP_09,
			ATTRIBUTE_MAPJUMP_10,
			ATTRIBUTE_MAPJUMP_11,
			ATTRIBUTE_MAPJUMP_12,
			ATTRIBUTE_DAMAGE_01,
			ATTRIBUTE_DAMAGE_02,
			ATTRIBUTE_DAMAGE_03,
			ATTRIBUTE_DAMAGE_04,
			ATTRIBUTE_DAMAGE_05,
			ATTRIBUTE_MONSTER_SKY,
			ATTRIBUTE_MAX
		}

		public const MCL_ATTRIBUTE ATTRIBUTE_NONE = MCL_ATTRIBUTE.ATTRIBUTE_NONE;

		public const MCL_ATTRIBUTE ATTRIBUTE_GROUND = MCL_ATTRIBUTE.ATTRIBUTE_GROUND;

		public const MCL_ATTRIBUTE ATTRIBUTE_WALL_01 = MCL_ATTRIBUTE.ATTRIBUTE_WALL_01;

		public const MCL_ATTRIBUTE ATTRIBUTE_WALL_02 = MCL_ATTRIBUTE.ATTRIBUTE_WALL_02;

		public const MCL_ATTRIBUTE ATTRIBUTE_WALL_03 = MCL_ATTRIBUTE.ATTRIBUTE_WALL_03;

		public const MCL_ATTRIBUTE ATTRIBUTE_WALL_04 = MCL_ATTRIBUTE.ATTRIBUTE_WALL_04;

		public const MCL_ATTRIBUTE ATTRIBUTE_WALL_05 = MCL_ATTRIBUTE.ATTRIBUTE_WALL_05;

		public const MCL_ATTRIBUTE ATTRIBUTE_CAMERA = MCL_ATTRIBUTE.ATTRIBUTE_CAMERA;

		public const MCL_ATTRIBUTE ATTRIBUTE_LANDFORM_01 = MCL_ATTRIBUTE.ATTRIBUTE_LANDFORM_01;

		public const MCL_ATTRIBUTE ATTRIBUTE_LANDFORM_02 = MCL_ATTRIBUTE.ATTRIBUTE_LANDFORM_02;

		public const MCL_ATTRIBUTE ATTRIBUTE_LANDFORM_03 = MCL_ATTRIBUTE.ATTRIBUTE_LANDFORM_03;

		public const MCL_ATTRIBUTE ATTRIBUTE_LANDFORM_04 = MCL_ATTRIBUTE.ATTRIBUTE_LANDFORM_04;

		public const MCL_ATTRIBUTE ATTRIBUTE_LANDFORM_05 = MCL_ATTRIBUTE.ATTRIBUTE_LANDFORM_05;

		public const MCL_ATTRIBUTE ATTRIBUTE_LANDFORM_06 = MCL_ATTRIBUTE.ATTRIBUTE_LANDFORM_06;

		public const MCL_ATTRIBUTE ATTRIBUTE_LANDFORM_07 = MCL_ATTRIBUTE.ATTRIBUTE_LANDFORM_07;

		public const MCL_ATTRIBUTE ATTRIBUTE_LANDFORM_08 = MCL_ATTRIBUTE.ATTRIBUTE_LANDFORM_08;

		public const MCL_ATTRIBUTE ATTRIBUTE_LANDFORM_09 = MCL_ATTRIBUTE.ATTRIBUTE_LANDFORM_09;

		public const MCL_ATTRIBUTE ATTRIBUTE_LANDFORM_10 = MCL_ATTRIBUTE.ATTRIBUTE_LANDFORM_10;

		public const MCL_ATTRIBUTE ATTRIBUTE_LANDFORM_11 = MCL_ATTRIBUTE.ATTRIBUTE_LANDFORM_11;

		public const MCL_ATTRIBUTE ATTRIBUTE_LANDFORM_12 = MCL_ATTRIBUTE.ATTRIBUTE_LANDFORM_12;

		public const MCL_ATTRIBUTE ATTRIBUTE_MONSTER_01 = MCL_ATTRIBUTE.ATTRIBUTE_MONSTER_01;

		public const MCL_ATTRIBUTE ATTRIBUTE_MONSTER_02 = MCL_ATTRIBUTE.ATTRIBUTE_MONSTER_02;

		public const MCL_ATTRIBUTE ATTRIBUTE_MONSTER_03 = MCL_ATTRIBUTE.ATTRIBUTE_MONSTER_03;

		public const MCL_ATTRIBUTE ATTRIBUTE_MONSTER_04 = MCL_ATTRIBUTE.ATTRIBUTE_MONSTER_04;

		public const MCL_ATTRIBUTE ATTRIBUTE_MONSTER_05 = MCL_ATTRIBUTE.ATTRIBUTE_MONSTER_05;

		public const MCL_ATTRIBUTE ATTRIBUTE_MAPJUMP_01 = MCL_ATTRIBUTE.ATTRIBUTE_MAPJUMP_01;

		public const MCL_ATTRIBUTE ATTRIBUTE_MAPJUMP_02 = MCL_ATTRIBUTE.ATTRIBUTE_MAPJUMP_02;

		public const MCL_ATTRIBUTE ATTRIBUTE_MAPJUMP_03 = MCL_ATTRIBUTE.ATTRIBUTE_MAPJUMP_03;

		public const MCL_ATTRIBUTE ATTRIBUTE_MAPJUMP_04 = MCL_ATTRIBUTE.ATTRIBUTE_MAPJUMP_04;

		public const MCL_ATTRIBUTE ATTRIBUTE_MAPJUMP_05 = MCL_ATTRIBUTE.ATTRIBUTE_MAPJUMP_05;

		public const MCL_ATTRIBUTE ATTRIBUTE_MAPJUMP_06 = MCL_ATTRIBUTE.ATTRIBUTE_MAPJUMP_06;

		public const MCL_ATTRIBUTE ATTRIBUTE_MAPJUMP_07 = MCL_ATTRIBUTE.ATTRIBUTE_MAPJUMP_07;

		public const MCL_ATTRIBUTE ATTRIBUTE_MAPJUMP_08 = MCL_ATTRIBUTE.ATTRIBUTE_MAPJUMP_08;

		public const MCL_ATTRIBUTE ATTRIBUTE_MAPJUMP_09 = MCL_ATTRIBUTE.ATTRIBUTE_MAPJUMP_09;

		public const MCL_ATTRIBUTE ATTRIBUTE_MAPJUMP_10 = MCL_ATTRIBUTE.ATTRIBUTE_MAPJUMP_10;

		public const MCL_ATTRIBUTE ATTRIBUTE_MAPJUMP_11 = MCL_ATTRIBUTE.ATTRIBUTE_MAPJUMP_11;

		public const MCL_ATTRIBUTE ATTRIBUTE_MAPJUMP_12 = MCL_ATTRIBUTE.ATTRIBUTE_MAPJUMP_12;

		public const MCL_ATTRIBUTE ATTRIBUTE_DAMAGE_01 = MCL_ATTRIBUTE.ATTRIBUTE_DAMAGE_01;

		public const MCL_ATTRIBUTE ATTRIBUTE_DAMAGE_02 = MCL_ATTRIBUTE.ATTRIBUTE_DAMAGE_02;

		public const MCL_ATTRIBUTE ATTRIBUTE_DAMAGE_03 = MCL_ATTRIBUTE.ATTRIBUTE_DAMAGE_03;

		public const MCL_ATTRIBUTE ATTRIBUTE_DAMAGE_04 = MCL_ATTRIBUTE.ATTRIBUTE_DAMAGE_04;

		public const MCL_ATTRIBUTE ATTRIBUTE_DAMAGE_05 = MCL_ATTRIBUTE.ATTRIBUTE_DAMAGE_05;

		public const MCL_ATTRIBUTE ATTRIBUTE_MONSTER_SKY = MCL_ATTRIBUTE.ATTRIBUTE_MONSTER_SKY;

		public const MCL_ATTRIBUTE ATTRIBUTE_MAX = MCL_ATTRIBUTE.ATTRIBUTE_MAX;

		public static Array s_pData = null;

		private static int[] compass = new int[5] { 0, 0, 4096, 0, 0 };

		private static VecFx32 mcl_reuse_normal = new VecFx32(0, 0, 0);

		private static VecFx32 mcl_reuse_toEdge = new VecFx32(0, 0, 0);

		private static VecFx32 mcl_reuse_toTri = new VecFx32(0, 0, 0);

		private static VecFx32 mcl_reuse_toCrossPt = new VecFx32(0, 0, 0);

		private static VecFx32 mcl_reuse_crossPt = new VecFx32(0, 0, 0);

		private static VecFx32 mcl_reuse_d0 = new VecFx32();

		private static VecFx32 mcl_reuse_d1 = new VecFx32();

		private static VecFx32 mcl_reuse_d2 = new VecFx32();

		private static VecFx32 mcl_reuse_c0 = new VecFx32();

		private static VecFx32 mcl_reuse_c1 = new VecFx32();

		private static VecFx32 mcl_reuse_c2 = new VecFx32();

		private static VecFx32 mcl_reuse_pos = new VecFx32();

		private static VecFx32 mcl_reuse_N = new VecFx32();

		private static CBlockData[] mcl_reuse_blockList = new CBlockData[9];

		private static readonly VecFx32[] mcl_reuse_vecList = new VecFx32[8]
		{
			new VecFx32(2365, 2365, 2365),
			new VecFx32(2365, 2365, -2365),
			new VecFx32(-2365, 2365, 2365),
			new VecFx32(-2365, 2365, -2365),
			new VecFx32(2365, -2365, 2365),
			new VecFx32(2365, -2365, -2365),
			new VecFx32(-2365, -2365, 2365),
			new VecFx32(-2365, -2365, -2365)
		};

		private static ds.pri.DSSphere mcl_reuse_sphere = new ds.pri.DSSphere();

		private static ds.pri.DSTriangle mcl_reuse_triangle = new ds.pri.DSTriangle();

		private static ds.pri.DSPlane mcl_reuse_plane = new ds.pri.DSPlane();

		private static ds.pri.DSTriangle mcl_reuse_triCopy = new ds.pri.DSTriangle();

		private static uint TYPE_MAX = 64u;
	}
}
