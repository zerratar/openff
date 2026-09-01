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
	public class NNSG3dRenderObj
	{
		public uint flag;

		public NNSG3dResMdl resMdl;

		public NNSG3dAnmObj anmMat;

		public NNSG3dFuncAnmBlendMat funcBlendMat;

		public NNSG3dAnmObj anmJnt;

		public NNSG3dFuncAnmBlendJnt funcBlendJnt;

		public NNSG3dAnmObj anmVis;

		public NNSG3dFuncAnmBlendVis funcBlendVis;

		public NNSG3dSbcCallBackFunc cbFunc;

		public byte cbCmd;

		public byte cbTiming;

		public ushort dummy_;

		public NNSG3dSbcCallBackFunc cbInitFunc;

		public object ptrUser;

		public byte[] ptrUserSbc;

		public NNSG3dJntAnmResult[] recJntAnm;

		public NNSG3dMatAnmResult recMatAnm;

		public uint[] hintMatAnmExist = new uint[2];

		public uint[] hintJntAnmExist = new uint[2];

		public uint[] hintVisAnmExist = new uint[2];

		public void setDefault()
		{
			flag = 0u;
			resMdl = null;
			anmMat = null;
			funcBlendMat = null;
			anmJnt = null;
			funcBlendJnt = null;
			anmVis = null;
			funcBlendVis = null;
			cbFunc = null;
			cbCmd = 0;
			cbTiming = 0;
			dummy_ = 0;
			cbInitFunc = null;
			ptrUser = null;
			ptrUserSbc = null;
			recJntAnm = null;
			recMatAnm = null;
			for (int i = 0; i < hintMatAnmExist.Length; i++)
			{
				hintMatAnmExist[0] = 0u;
			}
			for (int i = 0; i < hintJntAnmExist.Length; i++)
			{
				hintJntAnmExist[0] = 0u;
			}
			for (int i = 0; i < hintVisAnmExist.Length; i++)
			{
				hintVisAnmExist[0] = 0u;
			}
		}
	}
}
