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
			public class CViewCamera : CCamera
			{
				public enum enCONTROL_TYPE
				{
					enTYPE_MAP,
					enTYPE_CHARA
				}

				public const enCONTROL_TYPE enTYPE_MAP = enCONTROL_TYPE.enTYPE_MAP;

				public const enCONTROL_TYPE enTYPE_CHARA = enCONTROL_TYPE.enTYPE_CHARA;

				private enCONTROL_TYPE m_Type;

				public override void initialize_usr()
				{
					m_AngleY = short.MinValue;
					m_Distance = 520192;
				}

				public override void move()
				{
					switch (m_Type)
					{
					case enCONTROL_TYPE.enTYPE_MAP:
						move_map_viewer();
						break;
					case enCONTROL_TYPE.enTYPE_CHARA:
						move_chara_viewer();
						break;
					}
				}

				public void move_map_viewer()
				{
					if ((g_Pad.pad() & 2) != 0)
					{
						if ((g_Pad.pad() & 0x40) != 0)
						{
							m_AngleX -= 256;
						}
						if ((g_Pad.pad() & 0x80) != 0)
						{
							m_AngleX += 256;
						}
						if ((g_Pad.pad() & 0x10) != 0)
						{
							m_AngleY -= 256;
						}
						if ((g_Pad.pad() & 0x20) != 0)
						{
							m_AngleY += 256;
						}
						return;
					}
					if ((g_Pad.pad() & 0x400) != 0)
					{
						if ((g_Pad.pad() & 0x40) != 0)
						{
							addDistance(2048);
						}
						if ((g_Pad.pad() & 0x80) != 0)
						{
							addDistance(-2048);
						}
						if ((g_Pad.edge() & 0x800) == 0)
						{
						}
						return;
					}
					if ((g_Pad.pad() & 0x800) != 0)
					{
						if ((g_Pad.pad() & 0x40) != 0)
						{
							m_CamInfo.target.y += 1024;
						}
						if ((g_Pad.pad() & 0x80) != 0)
						{
							m_CamInfo.target.y -= 1024;
						}
						return;
					}
					short sinVal = FX_SinIdx((ushort)m_AngleY);
					short cosVal = FX_CosIdx((ushort)m_AngleY);
					MtxFx33 sys3d_reuse_mtxY = sys3d.sys3d_reuse_mtxY;
					VecFx32 sys3d_reuse_vec = sys3d.sys3d_reuse_vec;
					int z = 0;
					int x = 0;
					if ((g_Pad.pad() & 0x40) != 0)
					{
						z = 2048;
					}
					if ((g_Pad.pad() & 0x80) != 0)
					{
						z = -2048;
					}
					if ((g_Pad.pad() & 0x10) != 0)
					{
						x = -2048;
					}
					if ((g_Pad.pad() & 0x20) != 0)
					{
						x = 2048;
					}
					sys3d_reuse_vec.x = x;
					sys3d_reuse_vec.y = 0;
					sys3d_reuse_vec.z = z;
					MTX_RotY33(sys3d_reuse_mtxY, sinVal, cosVal);
					MTX_MultVec33(sys3d_reuse_vec, sys3d_reuse_mtxY, sys3d_reuse_vec);
					m_CamInfo.target.x += sys3d_reuse_vec.x;
					m_CamInfo.target.y += sys3d_reuse_vec.y;
					m_CamInfo.target.z += sys3d_reuse_vec.z;
				}

				public void move_chara_viewer()
				{
					if ((g_Pad.pad() & 2) != 0)
					{
						if ((g_Pad.pad() & 0x40) != 0)
						{
							m_AngleX -= 128;
						}
						if ((g_Pad.pad() & 0x80) != 0)
						{
							m_AngleX += 128;
						}
						return;
					}
					if ((g_Pad.pad() & 0x400) != 0)
					{
						if ((g_Pad.pad() & 0x40) != 0)
						{
							addDistance(2048);
						}
						if ((g_Pad.pad() & 0x80) != 0)
						{
							addDistance(-2048);
						}
						if ((g_Pad.edge() & 0x800) == 0)
						{
						}
						return;
					}
					if ((g_Pad.pad() & 0x800) != 0)
					{
						if ((g_Pad.pad() & 0x40) != 0)
						{
							m_CamInfo.target.y += 1024;
						}
						if ((g_Pad.pad() & 0x80) != 0)
						{
							m_CamInfo.target.y -= 1024;
						}
						return;
					}
					if ((g_Pad.pad() & 0x10) != 0)
					{
						m_AngleY -= 256;
					}
					if ((g_Pad.pad() & 0x20) != 0)
					{
						m_AngleY += 256;
					}
					if ((g_Pad.pad() & 0x40) != 0)
					{
						addDistance(2048);
					}
					if ((g_Pad.pad() & 0x80) != 0)
					{
						addDistance(-2048);
					}
				}

				public override void calculate()
				{
					short sinVal = FX_SinIdx((ushort)m_AngleX);
					short cosVal = FX_CosIdx((ushort)m_AngleX);
					short sinVal2 = FX_SinIdx((ushort)m_AngleY);
					short cosVal2 = FX_CosIdx((ushort)m_AngleY);
					MtxFx33 sys3d_reuse_mtx = sys3d.sys3d_reuse_mtx;
					MtxFx33 sys3d_reuse_mtxX = sys3d.sys3d_reuse_mtxX;
					MtxFx33 sys3d_reuse_mtxY = sys3d.sys3d_reuse_mtxY;
					VecFx32 sys3d_reuse_vec = sys3d.sys3d_reuse_vec;
					sys3d_reuse_vec.x = 0;
					sys3d_reuse_vec.y = 0;
					sys3d_reuse_vec.z = -4096;
					MTX_RotX33(sys3d_reuse_mtxX, sinVal, cosVal);
					MTX_RotY33(sys3d_reuse_mtxY, sinVal2, cosVal2);
					MTX_Concat33(sys3d_reuse_mtxX, sys3d_reuse_mtxY, sys3d_reuse_mtx);
					MTX_MultVec33(sys3d_reuse_vec, sys3d_reuse_mtx, sys3d_reuse_vec);
					sys3d_reuse_vec.x = sys3d_reuse_vec.x * m_Distance >> 12;
					sys3d_reuse_vec.y = sys3d_reuse_vec.y * m_Distance >> 12;
					sys3d_reuse_vec.z = sys3d_reuse_vec.z * m_Distance >> 12;
					m_CamInfo.position.x = m_CamInfo.target.x + sys3d_reuse_vec.x;
					m_CamInfo.position.y = m_CamInfo.target.y + sys3d_reuse_vec.y;
					m_CamInfo.position.z = m_CamInfo.target.z + sys3d_reuse_vec.z;
					m_CamInfo.camUp.x = 0;
					m_CamInfo.camUp.y = 4096;
					m_CamInfo.camUp.z = 0;
				}

				public void setType(enCONTROL_TYPE type)
				{
					m_Type = type;
				}
			}
		}
	}
}
