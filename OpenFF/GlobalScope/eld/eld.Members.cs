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
	public static partial class eld
	{
		private const int EFF_ONE = 4096;

		private const short DS_COLOR_MIN = 0;

		private const short DS_COLOR_MAX = 31;

		private const float FDS_COLOR_MIN = 0f;

		private const float FDS_COLOR_MAX = 31f;

		public const int OBJ_STATUS_READY = 0;

		public const int OBJ_STATUS_RUN = 1;

		public const int OBJ_STATUS_PAUSE = 2;

		public const int OBJ_STATUS_STOP_WAIT = 4;

		public const int OBJ_STATUS_STOP = 8;

		public const int OBJ_STATUS_DEAD = 16;

		public const int OBJ_STATUS_STOP_WAITtoDEAD = 32;

		public const int OBJ_STATUS_START_WAIT = 64;

		public const enOBJECT_STATE enSTATE_PLAY = enOBJECT_STATE.enSTATE_PLAY;

		public const enOBJECT_STATE enSTATE_WAITEND = enOBJECT_STATE.enSTATE_WAITEND;

		public const enDSCull enDSCull_All = enDSCull.enDSCull_All;

		public const enDSCull enDSCull_Front = enDSCull.enDSCull_Front;

		public const enDSCull enDSCull_Back = enDSCull.enDSCull_Back;

		public const enDSCull enDSCull_None = enDSCull.enDSCull_None;

		public const enFLAG_MDS enFLAG_MDS_LOOP = enFLAG_MDS.enFLAG_MDS_LOOP;

		public const enFLAG_MDS enFLAG_MDS_ANIME_MATERIAL = enFLAG_MDS.enFLAG_MDS_ANIME_MATERIAL;

		public const enFLAG_MDS enFLAG_MDS_ANIME_TEXTURE_SRT = enFLAG_MDS.enFLAG_MDS_ANIME_TEXTURE_SRT;

		public const enFLAG_MDS enFLAG_MDS_ANIME_TEXTURE_PATTERN = enFLAG_MDS.enFLAG_MDS_ANIME_TEXTURE_PATTERN;

		public const enFLAG_MDS enFLAG_MDS_ANIME_VISIBILITY = enFLAG_MDS.enFLAG_MDS_ANIME_VISIBILITY;

		public const enFLAG_MDS enFLAG_MDS_END = enFLAG_MDS.enFLAG_MDS_END;

		public const enFLAG_PDS enFLAG_LOOP = enFLAG_PDS.enFLAG_LOOP;

		public const enFLAG_PDS enFLAG_AFTERIMAGE = enFLAG_PDS.enFLAG_AFTERIMAGE;

		public const enFLAG_PDS enFLAG_FADE = enFLAG_PDS.enFLAG_FADE;

		public const enFLAG_PDS enFLAG_MOVE_OFFSET = enFLAG_PDS.enFLAG_MOVE_OFFSET;

		public const enFLAG_PDS enFLAG_XLU_DEPTH = enFLAG_PDS.enFLAG_XLU_DEPTH;

		public const enFLAG_PDS enFLAG_TEXOUT_AUTO = enFLAG_PDS.enFLAG_TEXOUT_AUTO;

		public const enFLAG_PDS enFLAG_TEXOUT_A5I3 = enFLAG_PDS.enFLAG_TEXOUT_A5I3;

		public const enFLAG_PDS enFLAG_TEXOUT_A3I5 = enFLAG_PDS.enFLAG_TEXOUT_A3I5;

		public const enFLAG_PDS enFLAG_END = enFLAG_PDS.enFLAG_END;

		public const enSEQ_FLAG enSEQ_FLAG_LOOP = enSEQ_FLAG.enSEQ_FLAG_LOOP;

		public const enSEQ_FLAG enSEQ_FLAG_LOOK_CAMERA = enSEQ_FLAG.enSEQ_FLAG_LOOK_CAMERA;

		public const int PATH_MOVE_NORMAL = 0;

		public const int PATH_MOVE_IDLE = 1;

		public const int PATH_MOVE_REVERSE = 2;

		public const enPATH_FLAG enPATH_FLAG_TYPE_LINE = enPATH_FLAG.enPATH_FLAG_TYPE_LINE;

		public const enPATH_FLAG enPATH_FLAG_TYPE_CURVE = enPATH_FLAG.enPATH_FLAG_TYPE_CURVE;

		public const enPATH_FLAG enPATH_FLAG_TYPE_ITURN = enPATH_FLAG.enPATH_FLAG_TYPE_ITURN;

		public const enPATH_FLAG enPATH_FLAG_TYPE_UTURN = enPATH_FLAG.enPATH_FLAG_TYPE_UTURN;

		public const enPATH_FLAG enPATH_FLAG_TYPE_LOOP = enPATH_FLAG.enPATH_FLAG_TYPE_LOOP;

		public const enPATH_FLAG enPATH_FLAG_TYPE_LOCAL = enPATH_FLAG.enPATH_FLAG_TYPE_LOCAL;

		public const enPATH_FLAG enPATH_FLAG_TYPE_WORLD = enPATH_FLAG.enPATH_FLAG_TYPE_WORLD;

		public const enPATH_FLAG enPATH_FLAG_SET_FIGURE = enPATH_FLAG.enPATH_FLAG_SET_FIGURE;

		public const enPATH_FLAG enPATH_FLAG_ADD_FIGURE = enPATH_FLAG.enPATH_FLAG_ADD_FIGURE;

		public static int CMD_BUFF_MAX = 6;

		public static DSAllocator g_elaloc = new DSAllocator();

		public static DSGL g_elgl = new DSGL();

		public static DSVramManager g_elvmng = new DSVramManager();

		public static uint VER_CURR_MDS = 4112u;

		public static uint VER_1_00_MDS = 4096u;

		public static uint MDS_AMN_NAME_SIZE = 48u;

		public static uint VER_CURR_PDS = 4608u;

		public static uint VER_1_11_PDS = 4368u;

		public static uint VER_1_01_PDS = 4112u;

		public static uint VER_1_00_PDS = 4096u;

		public static uint VER_CURR_BPDS = 4096u;

		public static uint VER_CURR_GDS = 4352u;

		public static uint VER_1_00_GDS = 4096u;

		public static uint VER_CURR_PLDS = 4608u;

		public static uint VER_1_11_PLDS = 4368u;

		public static uint VER_1_01_PLDS = 4112u;

		public static uint VER_1_00_PLDS = 4096u;

		public static uint VER_CURR_BPLDS = 4096u;

		public static uint VER_CURR_SEQ_DS = 4096u;

		public static uint VER_CURR_PATH_DS = 4096u;

		public static uint FILE_TYPE_PATH = 1213481296u;

		public static PolygonIDPublisher g_PolyID = new PolygonIDPublisher();

		public static ServerFF3 g_elsvr = new ServerFF3();

		internal static int DsEffMul(int a, int b)
		{
			return FX_Mul(a, b);
		}

		internal static int DsEffSin(int a)
		{
			return FX_SinIdx((ushort)a);
		}

		internal static int DsEffCos(int a)
		{
			return FX_CosIdx((ushort)a);
		}

		internal static int EffRand(int _max)
		{
			return (int)ds.RandomNumber.rand32((uint)_max);
		}

		internal static float EffRandf(float _max)
		{
			return 0f;
		}

		internal static float EffClamp(float src, float min, float max)
		{
			if (!(src < min))
			{
				if (!(src > max))
				{
					return src;
				}
				return max;
			}
			return min;
		}

		internal static T EffAbs<T>(T src)
		{
			if (!_isL(src, default(T)))
			{
				return src;
			}
			return _neg(src);
		}

		internal static void EffVectorNormalize(ds.Vector3<int> v)
		{
			VEC_Normalize(v, v);
		}

		internal static int EffVectorLength(ds.Vector3<int> v)
		{
			return VEC_Mag(v);
		}

		internal static void EffMulVectorToScalar(ds.Vector3<int> v, int s)
		{
			v.vx = FX_Mul(v.vx, s);
			v.vy = FX_Mul(v.vy, s);
			v.vz = FX_Mul(v.vz, s);
		}

		internal static void EffMulVectorToScalar(ds.Vector4<int> v, int s)
		{
			v.vx = FX_Mul(v.vx, s);
			v.vy = FX_Mul(v.vy, s);
			v.vz = FX_Mul(v.vz, s);
			v.vw = FX_Mul(v.vw, s);
		}

		internal static void EffMulVectorToMatrix(ds.Vector3<int> v, MtxFx43 m)
		{
			MTX_MultVec43(v, m, v);
		}

		internal static void EffLoadIdentity(MtxFx43 m)
		{
			ds.CpuMatrix.identity(m);
		}

		internal static void EffSetRotation(MtxFx43 m, ds.Vector3<int> rot)
		{
			ds.CpuMatrix.setRotate(m, rot);
		}

		internal static T EffPow2<T>(T t)
		{
			return _mul(t, t);
		}

		internal static int EffFxPow2(int t)
		{
			return FX_Mul(t, t);
		}

		internal static T EffPow3<T>(T t)
		{
			return _mul(_mul(t, t), t);
		}

		internal static int EffFxPow3(int t)
		{
			return FX_Mul(t, FX_Mul(t, t));
		}

		internal static Array IServer_Instance_getAllocator_allocateMemory(int bufsize)
		{
			return IServer.Instance().getAllocator().allocateMemory((uint)bufsize);
		}

		internal static void IServer_Instance_getAllocator_deallocateMemory(Array pNbElem)
		{
			IServer.Instance().getAllocator().deallocateMemory(pNbElem);
		}

		internal static uint WrapRound(uint value, uint Max)
		{
			if (value < Max)
			{
				return value;
			}
			return value - Max;
		}

		internal static void enable(ref uint flag, ref uint bit)
		{
			flag |= bit;
		}

		internal static void disable(ref uint flag, ref uint bit)
		{
			flag &= ~bit;
		}

		internal static bool isEnable(ref uint flag, ref uint bit)
		{
			if ((flag & bit) == 0)
			{
				return false;
			}
			return true;
		}

		internal static uint checkSequenceFlag(uint f, enSEQ_FLAG type)
		{
			return f & (uint)type;
		}

	}
}
