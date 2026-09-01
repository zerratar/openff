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
	public static partial class ds
	{
		public static partial class sys3d
		{
			public class CCamera : dgs.CRestricted
			{
				protected const int CALC_MODE_FREE = 0;

				protected const int CALC_MODE_SET = 1;

				protected const int PROJ_PERSPECTIVE = 0;

				protected const int PROJ_FRUSTUM = 1;

				protected PerspectiveInfo m_PersInfo = new PerspectiveInfo();

				protected CameraInfo m_CamInfo = new CameraInfo();

				protected int m_Distance;

				protected int m_DistMin;

				protected int m_DistMax;

				protected int m_Mode;

				protected int m_ProjMode;

				protected short m_AngleX;

				protected short m_AngleY;

				protected short m_AngleZ;

				protected ushort m_Temp;

				protected ushort m_Temp2;

				protected VecFx32 m_Offset = new VecFx32();

				public void initialize()
				{
					m_PersInfo.fovySin = 214;
					m_PersInfo.fovyCos = 4090;
					m_PersInfo.aspect = 5461;
					m_PersInfo.nearClip = 4096;
					m_PersInfo.farClip = 8388608;
					m_CamInfo.target.x = 0;
					m_CamInfo.target.y = 0;
					m_CamInfo.target.z = 0;
					m_CamInfo.position.x = 0;
					m_CamInfo.position.y = 0;
					m_CamInfo.position.z = 0;
					m_AngleX = 0;
					m_AngleY = 0;
					m_AngleZ = 0;
					m_Distance = 65536;
					m_DistMin = 16;
					m_DistMax = 8323072;
					initialize_usr();
					calculate();
					m_ProjMode = 0;
					VEC_Set(m_Offset, 0, 0, 0);
				}

				public virtual void initialize_usr()
				{
				}

				public void execute()
				{
					CameraInfo camInfo = m_CamInfo;
					PerspectiveInfo persInfo = m_PersInfo;
					if (m_Mode == 0)
					{
						move();
						calculate();
					}
					calc_direction();
					if (m_ProjMode == 0)
					{
						NNS_G3dGlbPerspective(persInfo.fovySin, persInfo.fovyCos, persInfo.aspect, persInfo.nearClip, persInfo.farClip);
					}
					else
					{
						int v = FX_Div(persInfo.nearClip, persInfo.fovyCos);
						v = FX_Mul(v, persInfo.fovySin);
						int num = FX_Mul(v, FX_Mul(m_Offset.y, 8192));
						int num2 = FX_Mul(v, persInfo.aspect);
						int num3 = FX_Mul(num2, FX_Mul(m_Offset.x, 8192));
						NNS_G3dGlbFrustum(v + num, -v + num, -num2 + num3, num2 + num3, persInfo.nearClip, persInfo.farClip);
					}
					NNS_G3dGlbLookAt(camInfo.position, camInfo.camUp, camInfo.target);
				}

				public void calc_direction()
				{
					m_CamInfo.direction.x = m_CamInfo.target.x - m_CamInfo.position.x;
					m_CamInfo.direction.y = m_CamInfo.target.y - m_CamInfo.position.y;
					m_CamInfo.direction.z = m_CamInfo.target.z - m_CamInfo.position.z;
					VEC_Normalize(m_CamInfo.direction, m_CamInfo.direction);
				}

				public void setFOV(int sin, int cos)
				{
					m_PersInfo.fovySin = sin;
					m_PersInfo.fovyCos = cos;
				}

				public void getFOV(out int sin, out int cos)
				{
					sin = m_PersInfo.fovySin;
					cos = m_PersInfo.fovyCos;
				}

				public void setAspect(int aspect)
				{
					m_PersInfo.aspect = aspect;
				}

				public void getAspect(out int aspect)
				{
					aspect = m_PersInfo.aspect;
				}

				public void setClip(int near, int far)
				{
					m_PersInfo.nearClip = near;
					m_PersInfo.farClip = far;
				}

				public void getClip(out int near, out int far)
				{
					near = m_PersInfo.nearClip;
					far = m_PersInfo.farClip;
				}

				public void setPosition(VecFx32 pos)
				{
					m_CamInfo.position.copy(pos);
				}

				public void setPosition(int x, int y, int z)
				{
					m_CamInfo.position.x = x;
					m_CamInfo.position.y = y;
					m_CamInfo.position.z = z;
				}

				public VecFx32 getPosition()
				{
					return m_CamInfo.position;
				}

				public VecFx32 getDirection()
				{
					return m_CamInfo.direction;
				}

				public void setTarget(VecFx32 pos)
				{
					m_CamInfo.target.copy(pos);
				}

				public VecFx32 getTarget()
				{
					return m_CamInfo.target;
				}

				public void setTarget(int x, int y, int z)
				{
					m_CamInfo.target.x = x;
					m_CamInfo.target.y = y;
					m_CamInfo.target.z = z;
				}

				public void setAngle(int x, int y, int z)
				{
					m_AngleX = (short)x;
					m_AngleY = (short)y;
					m_AngleZ = (short)z;
				}

				public void setCamUp(int x, int y, int z)
				{
					m_CamInfo.camUp.x = x;
					m_CamInfo.camUp.y = y;
					m_CamInfo.camUp.z = z;
				}

				public VecFx32 getCamUp()
				{
					return m_CamInfo.camUp;
				}

				public virtual void move()
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
						z = 1024;
					}
					if ((g_Pad.pad() & 0x80) != 0)
					{
						z = -1024;
					}
					if ((g_Pad.pad() & 0x10) != 0)
					{
						x = -1024;
					}
					if ((g_Pad.pad() & 0x20) != 0)
					{
						x = 1024;
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

				public virtual void calculate()
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

				public void setDistance(int dis)
				{
					m_Distance = dis;
				}

				public void addDistance(int dis)
				{
					m_Distance += dis;
					m_Distance = clamp(m_Distance, m_DistMin, m_DistMax);
				}

				public void setDistanceRange(int min, int max)
				{
					m_DistMin = min;
					m_DistMax = max;
				}

				public void getCameraMatrix(MtxFx43 m)
				{
					m.copy(NNS_G3dGlbGetCameraMtx());
				}

				public int GetM_Mode()
				{
					return m_Mode;
				}

				public void SetM_Mode(int val)
				{
					m_Mode = val;
				}

				public void setDirection(VecFx32 dir)
				{
					m_CamInfo.direction.copy(dir);
				}

				public short getAngleX()
				{
					return m_AngleX;
				}

				public short getAngleY()
				{
					return m_AngleY;
				}

				public short getAngleZ()
				{
					return m_AngleZ;
				}

				public int getDistance()
				{
					return m_Distance;
				}

				public void setOffset(VecFx32 ofs)
				{
					m_Offset.copy(ofs);
				}

				public void getOffset(VecFx32 ofs)
				{
					ofs.copy(m_Offset);
				}

				public void setMoveMode(int md)
				{
					m_Mode = md;
				}

				public void setProjMode(int md)
				{
					m_ProjMode = md;
				}

				public CameraInfo getCameraInfo()
				{
					return m_CamInfo;
				}

				public void copy(CCamera src)
				{
					copy((dgs.CRestricted)src);
					m_PersInfo.copy(src.m_PersInfo);
					m_CamInfo.copy(src.m_CamInfo);
					m_Distance = src.m_Distance;
					m_DistMin = src.m_DistMin;
					m_DistMax = src.m_DistMax;
					m_Mode = src.m_Mode;
					m_ProjMode = src.m_ProjMode;
					m_AngleX = src.m_AngleX;
					m_AngleY = src.m_AngleY;
					m_AngleZ = src.m_AngleZ;
					m_Temp = src.m_Temp;
					m_Temp2 = src.m_Temp2;
					m_Offset.copy(src.m_Offset);
				}
			}
		}
	}
}
