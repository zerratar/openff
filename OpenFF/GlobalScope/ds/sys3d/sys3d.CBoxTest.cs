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
		public static partial class sys3d
		{
			public class CBoxTest
			{
				public class BoundingBoxL
				{
					public string name;

					public int scale;

					public VecFx32 offset;

					public VecFx16 pos;

					public VecFx16 size;
				}

				public class BoundingBoxData
				{
					public uint numBB;

					public BoundingBoxL[] BB;

					public static explicit operator BoundingBoxData(Array src)
					{
						BoundingBoxData boundingBoxData = new BoundingBoxData();
						ArrayReader arrayReader = new ArrayReader(src);
						byte[] array = new byte[32];
						boundingBoxData.numBB = arrayReader.readUInt32();
						boundingBoxData.BB = new BoundingBoxL[boundingBoxData.numBB];
						for (int i = 0; i < boundingBoxData.numBB; i++)
						{
							boundingBoxData.BB[i] = new BoundingBoxL();
							arrayReader.read(array, 0, array.Length);
							boundingBoxData.BB[i].name = StringUtil.createString(array);
							boundingBoxData.BB[i].scale = arrayReader.readInt32();
							boundingBoxData.BB[i].offset.x = arrayReader.readInt32();
							boundingBoxData.BB[i].offset.y = arrayReader.readInt32();
							boundingBoxData.BB[i].offset.z = arrayReader.readInt32();
							boundingBoxData.BB[i].pos.x = arrayReader.readInt16();
							boundingBoxData.BB[i].pos.y = arrayReader.readInt16();
							boundingBoxData.BB[i].pos.z = arrayReader.readInt16();
							boundingBoxData.BB[i].size.x = arrayReader.readInt16();
							boundingBoxData.BB[i].size.y = arrayReader.readInt16();
							boundingBoxData.BB[i].size.z = arrayReader.readInt16();
						}
						arrayReader.dispose();
						return boundingBoxData;
					}
				}

				public class BoundingBox
				{
					public sys3d.BoundingBox m_BB;

					public VecFx32 m_Position;
				}

				private const uint boundingbox_max = 256u;

				private BoundingBoxData m_Data;

				private BoundingBox[] dataBB = new BoundingBox[256];

				private bool[] m_Flag = new bool[256];

				public CBoxTest()
				{
					m_Data = null;
				}

				~CBoxTest()
				{
				}

				public void setup(Array pData)
				{
					if (pData == null)
					{
						return;
					}
					m_Data = (BoundingBoxData)pData;
					for (int i = 0; (long)i < 256L; i++)
					{
						m_Flag[i] = false;
					}
					if (m_Data != null)
					{
						for (int j = 0; j < m_Data.numBB; j++)
						{
							dataBB[j].m_BB.pos = m_Data.BB[j].pos;
							dataBB[j].m_BB.size = m_Data.BB[j].size;
							dataBB[j].m_BB.scale = m_Data.BB[j].scale * 100;
							dataBB[j].m_Position.copy(m_Data.BB[j].offset);
						}
					}
				}

				public void exec()
				{
					if (m_Data != null)
					{
						for (int i = 0; i < m_Data.numBB; i++)
						{
							m_Flag[i] = execBB(dataBB[i].m_BB);
						}
					}
				}

				public bool execBB(sys3d.BoundingBox m_BB)
				{
					G3_MtxMode(GXMtxMode.GX_MTXMODE_TEXTURE);
					G3_Identity();
					G3_MtxMode(GXMtxMode.GX_MTXMODE_POSITION_VECTOR);
					G3_PushMtx();
					GXBoxTestParam gXBoxTestParam = new GXBoxTestParam();
					G3_Translate(0, 0, 0);
					G3_Scale(m_BB.scale, m_BB.scale, m_BB.scale);
					G3_PolygonAttr(1, GXPolygonMode.GX_POLYGONMODE_MODULATE, GXCull.GX_CULL_NONE, 0, 0, 12288);
					G3_Begin(GXBegin.GX_BEGIN_TRIANGLES);
					G3_End();
					gXBoxTestParam.x = m_BB.pos.x;
					gXBoxTestParam.y = m_BB.pos.y;
					gXBoxTestParam.z = m_BB.pos.z;
					gXBoxTestParam.width = m_BB.size.x;
					gXBoxTestParam.height = m_BB.size.y;
					gXBoxTestParam.depth = m_BB.size.z;
					G3_BoxTest(gXBoxTestParam);
					int @in;
					while (G3X_GetBoxTestResult(out @in) != 0)
					{
					}
					G3_PopMtx(1);
					if (@in != 0)
					{
						return true;
					}
					return false;
				}

				public void draw()
				{
					if (m_Data != null)
					{
						for (int i = 0; i < m_Data.numBB; i++)
						{
							drawBB(dataBB[i].m_BB, dataBB[i].m_Position, i);
						}
					}
				}

				public void drawBB(sys3d.BoundingBox m_BB, VecFx32 m_Position, int index)
				{
					int[][] array = new int[14][]
					{
						new int[3] { 31, 31, 31 },
						new int[3] { 31, 10, 10 },
						new int[3] { 10, 31, 10 },
						new int[3] { 10, 10, 31 },
						new int[3] { 31, 31, 10 },
						new int[3] { 31, 10, 31 },
						new int[3] { 10, 31, 31 },
						new int[3] { 15, 15, 15 },
						new int[3] { 15, 0, 0 },
						new int[3] { 0, 15, 0 },
						new int[3] { 0, 0, 15 },
						new int[3] { 15, 15, 0 },
						new int[3] { 0, 15, 15 },
						new int[3] { 15, 0, 15 }
					};
					index %= 10;
					gCubeGeometry[0] = m_BB.pos.x;
					gCubeGeometry[1] = m_BB.pos.y;
					gCubeGeometry[2] = m_BB.pos.z;
					gCubeGeometry[3] = m_BB.pos.x;
					gCubeGeometry[4] = m_BB.pos.y;
					gCubeGeometry[5] = (short)(m_BB.pos.z + m_BB.size.z);
					gCubeGeometry[6] = (short)(m_BB.pos.x + m_BB.size.x);
					gCubeGeometry[7] = m_BB.pos.y;
					gCubeGeometry[8] = m_BB.pos.z;
					gCubeGeometry[9] = (short)(m_BB.pos.x + m_BB.size.x);
					gCubeGeometry[10] = m_BB.pos.y;
					gCubeGeometry[11] = (short)(m_BB.pos.z + m_BB.size.z);
					gCubeGeometry[12] = m_BB.pos.x;
					gCubeGeometry[13] = (short)(m_BB.pos.y + m_BB.size.y);
					gCubeGeometry[14] = m_BB.pos.z;
					gCubeGeometry[15] = m_BB.pos.x;
					gCubeGeometry[16] = (short)(m_BB.pos.y + m_BB.size.y);
					gCubeGeometry[17] = (short)(m_BB.pos.z + m_BB.size.z);
					gCubeGeometry[18] = (short)(m_BB.pos.x + m_BB.size.x);
					gCubeGeometry[19] = (short)(m_BB.pos.y + m_BB.size.y);
					gCubeGeometry[20] = m_BB.pos.z;
					gCubeGeometry[21] = (short)(m_BB.pos.x + m_BB.size.x);
					gCubeGeometry[22] = (short)(m_BB.pos.y + m_BB.size.y);
					gCubeGeometry[23] = (short)(m_BB.pos.z + m_BB.size.z);
					G3_PushMtx();
					G3_Translate(m_Position.x, m_Position.y, m_Position.z);
					G3_Scale(m_BB.scale, m_BB.scale, m_BB.scale);
					G3_MaterialColorDiffAmb(GX_RGB(array[index][0], array[index][1], array[index][2]), GX_RGB(16, 16, 16), 1);
					G3_MaterialColorSpecEmi(GX_RGB(16, 16, 16), GX_RGB(0, 0, 0), 0);
					G3_PolygonAttr(0, GXPolygonMode.GX_POLYGONMODE_MODULATE, GXCull.GX_CULL_BACK, 63, 16, 2048);
					G3_Begin(GXBegin.GX_BEGIN_QUADS);
					quad(0, 1, 3, 2);
					quad(4, 5, 7, 6);
					G3_End();
					G3_PopMtx(1);
				}

				public uint getEnableCount()
				{
					uint num = 0u;
					for (int i = 0; (long)i < 256L; i++)
					{
						num += (uint)(m_Flag[i] ? 1 : 0);
					}
					return num;
				}

				public bool getFlag(int index)
				{
					return m_Flag[index];
				}

				public string getName(int index)
				{
					return m_Data.BB[index].name;
				}

				public uint getCount()
				{
					return m_Data.numBB;
				}

				public bool isEnable()
				{
					if (m_Data != null)
					{
						return true;
					}
					return false;
				}
			}
		}
	}
}
