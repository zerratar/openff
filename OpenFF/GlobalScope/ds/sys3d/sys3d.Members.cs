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
			public const SHADOW_TYPE SHADOW_TYPE_ERROR = SHADOW_TYPE.SHADOW_TYPE_ERROR;

			public const SHADOW_TYPE SHADOW_TYPE_VOLUME = SHADOW_TYPE.SHADOW_TYPE_VOLUME;

			public const SHADOW_TYPE SHADOW_TYPE_POLYGON = SHADOW_TYPE.SHADOW_TYPE_POLYGON;

			public static int INVALID_ANIME_INDEX = -1;

			private static MtxFx33 sys3d_reuse_mtx = new MtxFx33();

			private static MtxFx33 sys3d_reuse_mtxX = new MtxFx33();

			private static MtxFx33 sys3d_reuse_mtxY = new MtxFx33();

			private static VecFx32 sys3d_reuse_vec = new VecFx32();

			private static readonly VecFx32 sys3d_reuse_zero_vec = new VecFx32(0, 0, 0);

			private static readonly VecFx32 sys3d_reuse_one_vec = new VecFx32(4096, 4096, 4096);

			private static readonly MtxFx33 sys3d_reuse_unit_mat = new MtxFx33(4096, 0, 0, 0, 4096, 0, 0, 0, 4096);

			public static short[] gCubeGeometry = new short[27]
			{
				7200, 0, 7200, 7200, 0, -7200, -7200, 0, 7200, -7200,
				0, -7200, 7200, 14400, 7200, 7200, 14400, -7200, -7200, 14400,
				7200, -7200, 14400, -7200, 0, 0, 0
			};

			private static ModelRef modelRef;

			private static VecFx32 sys3d_reuse_scl = new VecFx32();

			private static VecFx32 sys3d_reuse_pos = new VecFx32();

			private static VecFx32 sys3d_reuse_rot = new VecFx32();

			private static pt.PrimitiveDisplay sys3d_reuse_disp = new pt.PrimitiveDisplay(null);

			internal static void vtx(int idx)
			{
				G3_Vtx(gCubeGeometry[idx * 3], gCubeGeometry[idx * 3 + 1], gCubeGeometry[idx * 3 + 2]);
			}

			internal static void quad(int idx0, int idx1, int idx2, int idx3)
			{
				vtx(idx0);
				vtx(idx1);
				vtx(idx2);
				vtx(idx3);
			}

			internal static void storeJntMtx(NNSG3dRS rs)
			{
				NNSG3dRenderObj nNSG3dRenderObj = NNS_G3dRSGetRenderObj(rs);
				CRenderObject cRenderObject = static_cast<CRenderObject>(nNSG3dRenderObj.ptrUser);
				if (cRenderObject == null)
				{
					return;
				}
				int num = -1;
				byte b;
				for (b = 0; b < 4; b++)
				{
					num = NNS_G3dGetNodeIdxByName(NNS_G3dGetNodeInfo(NNS_G3dRenderObjGetResMdl(nNSG3dRenderObj)), cRenderObject.m_JntMtx[b].nodeName);
					if (num == NNS_G3dRSGetCurrentNodeDescID(rs))
					{
						break;
					}
				}
				if (-1 < num && b < 4)
				{
					MtxFx43 b2 = NNS_G3dGlbGetInvCameraMtx();
					NNS_G3dGetCurrentMtx(cRenderObject.m_JntMtx[b].mtx, null);
					MTX_Concat43(cRenderObject.m_JntMtx[b].mtx, b2, cRenderObject.m_JntMtx[b].mtx);
					cRenderObject.m_JntMtx[b].flag |= 2u;
				}
			}

		}
	}
}
