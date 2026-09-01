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
	public class NNSG3dGlb
	{
		public uint cmd0;

		public uint mtxmode_proj;

		public MtxFx44 projMtx = new MtxFx44();

		public uint mtxmode_posvec;

		public MtxFx43 cameraMtx = new MtxFx43();

		public uint cmd1;

		public uint[] lightVec = new uint[4];

		public uint cmd2;

		public uint prmMatColor0;

		public uint prmMatColor1;

		public uint prmPolygonAttr;

		public uint prmViewPort;

		public uint cmd3;

		public uint[] lightColor = new uint[4];

		public uint cmd4;

		public MtxFx33 prmBaseRot = new MtxFx33();

		public VecFx32 prmBaseTrans = new VecFx32();

		public VecFx32 prmBaseScale = new VecFx32();

		public uint prmTexImageParam;

		public uint flag;

		public MtxFx43 invCameraMtx = new MtxFx43();

		public MtxFx43 srtCameraMtx;

		public MtxFx43 invSrtCameraMtx;

		public MtxFx43 invBaseMtx;

		public MtxFx44 invProjMtx;

		public MtxFx44 invCameraProjMtx;

		public VecFx32 camPos = new VecFx32();

		public VecFx32 camUp = new VecFx32();

		public VecFx32 camTarget = new VecFx32();
	}
}
