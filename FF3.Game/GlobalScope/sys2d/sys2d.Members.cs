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
	public static partial class sys2d
	{
		public const DS2D_OBJ_PLANE DS2D_OBJ_PLANE_MAIN3D = DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN3D;

		public const DS2D_OBJ_PLANE DS2D_OBJ_PLANE_MAIN2D = DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN2D;

		public const DS2D_OBJ_PLANE DS2D_OBJ_PLANE_SUB2D = DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_SUB2D;

		public const DS2D_OBJ_PLANE DS2D_OBJ_PLANE_MAX = DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAX;

		public const DS2D_OBJ_PLANE_FLAG DS2D_OBJ_PLANE_FLAG_MAIN3D = DS2D_OBJ_PLANE_FLAG.DS2D_OBJ_PLANE_FLAG_MAIN3D;

		public const DS2D_OBJ_PLANE_FLAG DS2D_OBJ_PLANE_FLAG_MAIN2D = DS2D_OBJ_PLANE_FLAG.DS2D_OBJ_PLANE_FLAG_MAIN2D;

		public const DS2D_OBJ_PLANE_FLAG DS2D_OBJ_PLANE_FLAG_SUB2D = DS2D_OBJ_PLANE_FLAG.DS2D_OBJ_PLANE_FLAG_SUB2D;

		public const DS2D_OBJ_DRAW_KIND DS2D_ODK_MAIN3D_NOT_SR = DS2D_OBJ_DRAW_KIND.DS2D_ODK_MAIN3D_NOT_SR;

		public const DS2D_OBJ_DRAW_KIND DS2D_ODK_MAIN3D_SR = DS2D_OBJ_DRAW_KIND.DS2D_ODK_MAIN3D_SR;

		public const DS2D_OBJ_DRAW_KIND DS2D_ODK_MAIN2D_NOT_SR = DS2D_OBJ_DRAW_KIND.DS2D_ODK_MAIN2D_NOT_SR;

		public const DS2D_OBJ_DRAW_KIND DS2D_ODK_MAIN2D_SR = DS2D_OBJ_DRAW_KIND.DS2D_ODK_MAIN2D_SR;

		public const DS2D_OBJ_DRAW_KIND DS2D_ODK_SUB2D_NOT_SR = DS2D_OBJ_DRAW_KIND.DS2D_ODK_SUB2D_NOT_SR;

		public const DS2D_OBJ_DRAW_KIND DS2D_ODK_SUB2D_SR = DS2D_OBJ_DRAW_KIND.DS2D_ODK_SUB2D_SR;

		public const DS2D_OBJ_DRAW_KIND DS2D_ODK_MAX = DS2D_OBJ_DRAW_KIND.DS2D_ODK_MAX;

		private static int DS2D_BG_MAX = 8;

		private static int DS2D_SPRITE_MAX = 512;

		private static int DS2D_SPRITE3D_MAX = 128;

		private static int DS2D_MAIN_CELL_MAX = 128;

		private static int DS2D_SUB_CELL_MAX = 128;

		private static int DS2D_SCREEN_SX = 0;

		private static int DS2D_SCREEN_SY = 0;

		private static int DS2D_SCREEN_EX = 480;

		private static int DS2D_SCREEN_EY = 320;

		private static NNSG2dViewRect DS2D_VIEW_RECT = new NNSG2dViewRect(ds.S32toFX32(DS2D_SCREEN_SX), ds.S32toFX32(DS2D_SCREEN_SY), ds.S32toFX32(DS2D_SCREEN_EX - DS2D_SCREEN_SX), ds.S32toFX32(DS2D_SCREEN_EY - DS2D_SCREEN_SY));

		private static NNS_G2D_VRAM_TYPE[] DS2D_VRAM_TYPE = new NNS_G2D_VRAM_TYPE[3]
		{
			NNS_G2D_VRAM_TYPE.NNS_G2D_VRAM_TYPE_3DMAIN,
			NNS_G2D_VRAM_TYPE.NNS_G2D_VRAM_TYPE_2DMAIN,
			NNS_G2D_VRAM_TYPE.NNS_G2D_VRAM_TYPE_2DSUB
		};

		private static NNSG2dSurfaceType[] DS2D_SURFACE_TYPE = new NNSG2dSurfaceType[3]
		{
			NNSG2dSurfaceType.NNS_G2D_SURFACETYPE_MAIN3D,
			NNSG2dSurfaceType.NNS_G2D_SURFACETYPE_MAIN2D,
			NNSG2dSurfaceType.NNS_G2D_SURFACETYPE_SUB2D
		};

		private static ushort[,] BG_DISP_SCREEN = new ushort[32, 32];

		private static int SCREEN_HALF_WIDTH = 128;

		private static int SCREEN_HALF_HEIGHT = 96;

	}
}
