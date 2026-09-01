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
	public static partial class btl
	{
		public class Damage
		{
			private u2d.PopUpDamageNumber[] damage_ = new u2d.PopUpDamageNumber[12];

			public void setup()
			{
			}

			public void cleanup()
			{
				for (int i = 0; i < 12; i++)
				{
					damage_[i].Release();
				}
			}

			public bool create(int i, int damage, NNSG2dFVec2 pos, int type)
			{
				if (damage_[i].pudnIsExist())
				{
					return false;
				}
				return damage_[i].pudnCreate(damage, pos, type);
			}

			public bool create(int i, int damage, VecFx32 pos, int type)
			{
				NNS_G3dWorldPosToScrPos(pos, out var px, out var py);
				NNSG2dFVec2 nNSG2dFVec = new NNSG2dFVec2();
				nNSG2dFVec.x = ds.S32toFX32(px);
				nNSG2dFVec.y = ds.S32toFX32(py);
				if (damage / 1000 != 0)
				{
					nNSG2dFVec.x -= 73728;
				}
				else if (damage / 100 != 0)
				{
					nNSG2dFVec.x -= 49152;
				}
				else if (damage / 10 != 0)
				{
					nNSG2dFVec.x -= 24576;
				}
				return create(i, damage, nNSG2dFVec, type);
			}

			public bool createPlayers(int i, int damage, VecFx32 pos, int type)
			{
				NNS_G3dWorldPosToScrPos(pos, out var px, out var py);
				NNSG2dFVec2 nNSG2dFVec = new NNSG2dFVec2();
				nNSG2dFVec.x = ds.S32toFX32(px);
				nNSG2dFVec.y = ds.S32toFX32(py);
				nNSG2dFVec.x -= 24576;
				if (damage / 1000 != 0)
				{
					nNSG2dFVec.x -= 36864;
				}
				else if (damage / 100 != 0)
				{
					nNSG2dFVec.x -= 24576;
				}
				else if (damage / 10 != 0)
				{
					nNSG2dFVec.x -= 12288;
				}
				return create(i, damage, nNSG2dFVec, type);
			}

			public Damage()
			{
				for (int i = 0; i < damage_.Length; i++)
				{
					damage_[i] = new u2d.PopUpDamageNumber();
				}
			}

			public u2d.PopUpDamageNumber damage(int i)
			{
				return damage_[i];
			}

			public bool isExist(int i)
			{
				return damage_[i].pudnIsExist();
			}
		}
	}
}
