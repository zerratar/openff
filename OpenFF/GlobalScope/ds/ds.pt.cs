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
	public static partial class ds
	{
		public static class pt
		{
			public class PrimitiveDisplay
			{
				public sys3d.Scene _scene;

				public PrimitiveDisplay(sys3d.Scene scene)
				{
					_scene = scene;
				}

				~PrimitiveDisplay()
				{
				}

				public void drawParticles(sys3d.ParticleElement elem)
				{
					sys3d.CCamera camera = _scene.getCamera();
					Vector3<int> pt_reuse_vTrans = pt.pt_reuse_vTrans;
					MtxFx43 pt_reuse_mCamera = pt.pt_reuse_mCamera;
					MtxFx43 pt_reuse_mBill = pt.pt_reuse_mBill;
					uint nbParticles = elem.getNbParticles();
					Particle[] particle = elem.getParticle();
					Vector3<int> centerPosition = elem.getCenterPosition();
					camera.getCameraMatrix(pt_reuse_mCamera);
					CpuMatrix.getTranslate(pt_reuse_vTrans, pt_reuse_mCamera);
					CpuMatrix.setTranslate(pt_reuse_mBill, pt_reuse_vTrans);
					CpuMatrix.resetTranslate(pt_reuse_mCamera);
					G3_LoadMtx43(pt_reuse_mBill);
					for (int i = 0; i < nbParticles; i++)
					{
						if (particle[i].Color.ca != 0)
						{
							setAttribute(particle[i].Disp, particle[i].PolygonID, particle[i].Color.ca);
							pt_reuse_vTrans.vx = centerPosition.vx + particle[i].Center.vx;
							pt_reuse_vTrans.vy = centerPosition.vy + particle[i].Center.vy;
							pt_reuse_vTrans.vz = centerPosition.vz + particle[i].Center.vz;
							MTX_MultVec43(pt_reuse_vTrans, pt_reuse_mCamera, pt_reuse_vTrans);
							G3_Translate(pt_reuse_vTrans.vx, pt_reuse_vTrans.vy, pt_reuse_vTrans.vz);
							G3_Begin(GXBegin.GX_BEGIN_QUADS);
							particle[i].packCommand();
							G3_End();
							G3_Translate(-pt_reuse_vTrans.vx, -pt_reuse_vTrans.vy, -pt_reuse_vTrans.vz);
						}
					}
				}

				public void drawLargeParticles(sys3d.LargeParticleElement elem)
				{
					sys3d.CCamera camera = _scene.getCamera();
					Vector3<int> pt_reuse_vTrans = pt.pt_reuse_vTrans;
					MtxFx43 pt_reuse_mCamera = pt.pt_reuse_mCamera;
					MtxFx43 pt_reuse_mBill = pt.pt_reuse_mBill;
					uint nbParticles = elem.getNbParticles();
					LargeParticle[] particle = elem.getParticle();
					Vector3<int> centerPosition = elem.getCenterPosition();
					camera.getCameraMatrix(pt_reuse_mCamera);
					CpuMatrix.getTranslate(pt_reuse_vTrans, pt_reuse_mCamera);
					CpuMatrix.setTranslate(pt_reuse_mBill, pt_reuse_vTrans);
					CpuMatrix.resetTranslate(pt_reuse_mCamera);
					G3_LoadMtx43(pt_reuse_mBill);
					G3_PushMtx();
					for (int i = 0; i < nbParticles; i++)
					{
						if (particle[i].Color.ca != 0)
						{
							setAttribute(particle[i].Disp, particle[i].PolygonID, particle[i].Color.ca);
							pt_reuse_vTrans.set(centerPosition.vx + particle[i].Center.vx, centerPosition.vy + particle[i].Center.vy, centerPosition.vz + particle[i].Center.vz);
							MTX_MultVec43(pt_reuse_vTrans, pt_reuse_mCamera, pt_reuse_vTrans);
							G3_Translate(pt_reuse_vTrans.vx, pt_reuse_vTrans.vy, pt_reuse_vTrans.vz);
							G3_Scale(particle[i].Size.vx, particle[i].Size.vy, 0);
							G3_Begin(GXBegin.GX_BEGIN_QUADS);
							particle[i].packCommand();
							G3_End();
							G3_RestoreMtx(1);
						}
					}
					G3_PopMtx(1);
				}

				public void drawPolygons(sys3d.PolygonElement elem)
				{
					sys3d.CCamera camera = _scene.getCamera();
					Vector3<int> pt_reuse_vTrans = pt.pt_reuse_vTrans;
					MtxFx43 pt_reuse_mCamera = pt.pt_reuse_mCamera;
					uint nbPolygon = elem.getNbPolygon();
					Polygon[] polygon = elem.getPolygon();
					Vector3<int> centerPosition = elem.getCenterPosition();
					camera.getCameraMatrix(pt_reuse_mCamera);
					CpuMatrix.getTranslate(pt_reuse_vTrans, pt_reuse_mCamera);
					CpuMatrix.resetTranslate(pt_reuse_mCamera);
					G3_PushMtx();
					for (int i = 0; i < nbPolygon; i++)
					{
						if (polygon[i].Color.ca != 0)
						{
							setAttribute((int)polygon[i].Disp, polygon[i].PolygonID, polygon[i].Color.ca);
							pt_reuse_vTrans.set(centerPosition.vx + polygon[i].Center.vx, centerPosition.vy + polygon[i].Center.vy, centerPosition.vz + polygon[i].Center.vz);
							MTX_MultVec43(pt_reuse_vTrans, pt_reuse_mCamera, pt_reuse_vTrans);
							G3_Translate(pt_reuse_vTrans.vx, pt_reuse_vTrans.vy, pt_reuse_vTrans.vz);
							G3_Begin(GXBegin.GX_BEGIN_QUADS);
							polygon[i].packCommand();
							G3_End();
							G3_RestoreMtx(1);
						}
					}
					G3_PopMtx(1);
				}

				public void drawBox(sys3d.BoxElement elem)
				{
					uint nbBox = elem.getNbBox();
					Box[] box = elem.getBox();
					Vector3<int> centerPosition = elem.getCenterPosition();
					G3_Translate(centerPosition.vx, centerPosition.vy, centerPosition.vz);
					BoxDisplay boxDisplay = new BoxDisplay();
					for (int i = 0; i < nbBox; i++)
					{
						boxDisplay.draw(box[i]);
					}
				}

				public void drawLines(Line[] pLines, uint unNbLines)
				{
					G3_Begin(GXBegin.GX_BEGIN_TRIANGLES);
					for (int i = 0; i < unNbLines; i++)
					{
						pLines[i].packCommand();
					}
					G3_End();
				}

				public void setAttribute(int disp, int polyID, int alpha)
				{
					G3_PolygonAttr(0, GXPolygonMode.GX_POLYGONMODE_MODULATE, (GXCull)disp, polyID, alpha, 0);
				}
			}

			public class BoxDisplay
			{
				public void draw(Box box)
				{
					G3_Color(getGXRgb(box.Color));
					G3_PushMtx();
					G3_Translate(box.Center.vx, box.Center.vy, box.Center.vz);
					G3_Scale(box.Size.vx, box.Size.vy, box.Size.vz);
					setAttribute(box.PolygonID[0], box.Color.ca);
					G3_Begin(GXBegin.GX_BEGIN_QUADS);
					drawQuad(7, 6, 5, 4);
					G3_End();
					setAttribute(box.PolygonID[1], box.Color.ca);
					G3_Begin(GXBegin.GX_BEGIN_QUADS);
					drawQuad(0, 1, 2, 3);
					G3_End();
					setAttribute(box.PolygonID[2], box.Color.ca);
					G3_Begin(GXBegin.GX_BEGIN_QUADS);
					drawQuad(0, 1, 5, 4);
					G3_End();
					setAttribute(box.PolygonID[3], box.Color.ca);
					G3_Begin(GXBegin.GX_BEGIN_QUADS);
					drawQuad(2, 3, 7, 6);
					G3_End();
					setAttribute(box.PolygonID[4], box.Color.ca);
					G3_Begin(GXBegin.GX_BEGIN_QUADS);
					drawQuad(0, 3, 7, 4);
					G3_End();
					setAttribute(box.PolygonID[5], box.Color.ca);
					G3_Begin(GXBegin.GX_BEGIN_QUADS);
					drawQuad(1, 2, 6, 5);
					G3_End();
					G3_PopMtx(1);
				}

				public void drawQuad(int v0, int v1, int v2, int v3)
				{
					drawVertex(v0);
					drawVertex(v1);
					drawVertex(v2);
					drawVertex(v3);
				}

				public void drawVertex(int vi)
				{
					int num = vi * 3;
					G3_Vtx(geometry[num], geometry[num + 1], geometry[num + 2]);
				}

				public void setAttribute(int _id, int alpha)
				{
					G3_PolygonAttr(0, GXPolygonMode.GX_POLYGONMODE_MODULATE, GXCull.GX_CULL_NONE, _id, alpha, 0);
				}
			}

			public class Particle
			{
				public Vector3<int> Center = new Vector3<int>();

				public Vector2<short> Size = new Vector2<short>();

				public Vector4<short> Color = new Vector4<short>();

				public Vector2<int>[] St = new Vector2<int>[2]
				{
					new Vector2<int>(),
					new Vector2<int>()
				};

				public short Disp;

				public ushort PolygonID;

				public void destruct()
				{
				}

				public void packCommand()
				{
					G3_Color(getGXRgb(Color));
					G3_TexCoord(St[0].s, St[0].t);
					G3_Vtx((short)(-Size.w), Size.h, 0);
					G3_TexCoord(St[0].s, St[1].t);
					G3_VtxXY((short)(-Size.w), (short)(-Size.h));
					G3_TexCoord(St[1].s, St[1].t);
					G3_VtxXY(Size.w, (short)(-Size.h));
					G3_TexCoord(St[1].s, St[0].t);
					G3_VtxXY(Size.w, Size.h);
				}
			}

			public class LargeParticle
			{
				public Vector3<int> Center = new Vector3<int>();

				public Vector2<int> Size = new Vector2<int>();

				public Vector4<short> Color = new Vector4<short>();

				public Vector2<int>[] St = new Vector2<int>[2]
				{
					new Vector2<int>(),
					new Vector2<int>()
				};

				public short Disp;

				public ushort PolygonID;

				public void destruct()
				{
				}

				public void packCommand()
				{
					G3_Color(getGXRgb(Color));
					G3_TexCoord(St[0].s, St[0].t);
					G3_Vtx(-4096, 4096, 0);
					G3_TexCoord(St[0].s, St[1].t);
					G3_VtxXY(-4096, -4096);
					G3_TexCoord(St[1].s, St[1].t);
					G3_VtxXY(4096, -4096);
					G3_TexCoord(St[1].s, St[0].t);
					G3_VtxXY(4096, 4096);
				}
			}

			public class Polygon
			{
				public Vector3<int> Center;

				public Vector3<short>[] Offset = new Vector3<short>[4];

				public Vector4<short> Color;

				public Vector2<int>[] St = new Vector2<int>[4];

				public enPolyDisp Disp;

				public ushort PolygonID;

				public void packCommand()
				{
					G3_Color(getGXRgb(Color));
					for (int i = 0; i < 4; i++)
					{
						G3_TexCoord(St[i].s, St[i].t);
						G3_Vtx(Offset[i].vx, Offset[i].vy, Offset[i].vz);
					}
				}
			}

			public class Box
			{
				public Vector3<int> Center;

				public Vector3<int> Size;

				public Vector4<short> Color;

				public short Disp;

				public ushort[] PolygonID = new ushort[6];

				public void drawDirect()
				{
					BoxDisplay boxDisplay = new BoxDisplay();
					boxDisplay.draw(this);
				}

				public void setPolygonIDArray(ushort _id)
				{
					for (int i = 0; i < 6; i++)
					{
						PolygonID[i] = _id;
						_id = (ushort)((_id + 1) & 0x3F);
					}
				}
			}

			public class Line
			{
				private Vector3<int>[] Point = new Vector3<int>[2];

				private Vector3<short> Color;

				public void packCommand()
				{
					G3_PushMtx();
					Vector3<int> vector = new Vector3<int>();
					vector.set(Point[1].vx - Point[0].vx, Point[1].vy - Point[0].vy, Point[1].vz - Point[0].vz);
					G3_Color(getGXRgb(Color));
					G3_Translate(Point[0].vx, Point[0].vy, Point[0].vz);
					G3_Vtx(0, 0, 0);
					G3_Translate(vector.vx, vector.vy, vector.vz);
					G3_Vtx(0, 0, 0);
					G3_Translate(-vector.vx, -vector.vy, -vector.vz);
					G3_Vtx(0, 0, 0);
					G3_PopMtx(1);
				}

				public void drawDirect()
				{
					G3_Begin(GXBegin.GX_BEGIN_TRIANGLES);
					packCommand();
					G3_End();
				}
			}

			public enum enPtclDisp
			{
				enPtcl_Off = 0,
				enPtcl_On = 2
			}

			public enum enPolyDisp
			{
				enPoly_Off = 0,
				enPoly_On = 3
			}

			public const int enX = 0;

			public const int enY = 1;

			public const int enZ = 2;

			public const int enW = 3;

			public const int enM = 0;

			public const int enP = 1;

			public const int enS = 0;

			public const int enT = 1;

			private static Vector3<int> pt_reuse_vTrans = new Vector3<int>();

			private static MtxFx43 pt_reuse_mCamera = new MtxFx43();

			private static MtxFx43 pt_reuse_mBill = new MtxFx43();

			public static short[] geometry = new short[24]
			{
				-4096, -4096, -4096, -4096, 4096, -4096, 4096, 4096, -4096, 4096,
				-4096, -4096, -4096, -4096, 4096, -4096, 4096, 4096, 4096, 4096,
				4096, 4096, -4096, 4096
			};
		}
	}
}
