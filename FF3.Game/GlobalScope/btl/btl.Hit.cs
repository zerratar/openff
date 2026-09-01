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
	public static partial class btl
	{
		public class Hit
		{
			private u2d.PopUpHitNumber hit_ = new u2d.PopUpHitNumber();

			public void setup()
			{
			}

			public void cleanup()
			{
				hit_.Release();
			}

			public bool create(int hit, NNSG2dFVec2 pos, u2d.PopUpHitNumber.puhnKIND kind)
			{
				if (hit_.puhnIsExist())
				{
					return false;
				}
				return hit_.puhnCreate(hit, pos, kind);
			}

			public bool create(int hit, VecFx32 pos, u2d.PopUpHitNumber.puhnKIND kind)
			{
				NNS_G3dWorldPosToScrPos(pos, out var px, out var py);
				NNSG2dFVec2 nNSG2dFVec = new NNSG2dFVec2();
				nNSG2dFVec.x = ds.S32toFX32(px);
				nNSG2dFVec.y = ds.S32toFX32(py);
				return create(hit, nNSG2dFVec, kind);
			}

			public bool createPlayers(int hit, VecFx32 pos, u2d.PopUpHitNumber.puhnKIND kind)
			{
				NNS_G3dWorldPosToScrPos(pos, out var px, out var py);
				NNSG2dFVec2 nNSG2dFVec = new NNSG2dFVec2();
				nNSG2dFVec.x = ds.S32toFX32(px);
				nNSG2dFVec.y = ds.S32toFX32(py);
				nNSG2dFVec.x -= 49152;
				if (hit / 10 != 0)
				{
					nNSG2dFVec.x -= 24576;
				}
				return create(hit, nNSG2dFVec, kind);
			}

			public bool isExist()
			{
				return hit_.puhnIsExist();
			}
		}
	}
}
