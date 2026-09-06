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
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
	public static partial class pl
	{
		public class BindObject
		{
			private CPlayerHuman pTarget_;

			private int charaMngIdx_;

			private MtxFx43 offsetMtx_ = new MtxFx43();

			private string locate_;

			private bool visible_;

			public BindObject(string pObjname, CPlayerHuman pTarget)
			{
				charaMngIdx_ = -1;
				visible_ = true;
				pTarget_ = null;
				MTX_Identity43(offsetMtx_);
				locate_ = "";
				pTarget_ = pTarget;
				charaMngIdx_ = characterMng.setCharacter(const_cast<string>(pObjname), CCharacterMng.PRI_SCENE.PRI_SCENE_SECOND);
				characterMng.setShadowType(charaMngIdx_, 2);
			}

			~BindObject()
			{
			}

			public void destruct()
			{
				characterMng.delCharacter(charaMngIdx_);
			}

			public static BindObject createBindObject(string pObjname, CPlayerHuman pTarget)
			{
				return new BindObject(pObjname, pTarget);
			}

			public static void deleteBindObject(BindObject pBindObj)
			{
				pBindObj.destruct();
				ds.CHeap.free_app(pBindObj);
			}

			public void setJntMtx()
			{
				int characterId = pTarget_.getCharacterId();
				if (!visible_)
				{
					characterMng.setHidden(charaMngIdx_, b: true);
					return;
				}
				MtxFx43 mtxFx = new MtxFx43();
				if (!characterMng.getJntMtx(characterId, locate_, mtxFx))
				{
					characterMng.setHidden(charaMngIdx_, b: true);
					return;
				}
				MtxFx43 mtxFx2 = new MtxFx43();
				MTX_Concat43(offsetMtx_, mtxFx, mtxFx2);
				characterMng.setHidden(charaMngIdx_, b: false);
				characterMng.setPoseMtx(charaMngIdx_, mtxFx2);
			}

			public void reserveJntMtx()
			{
				int characterId = pTarget_.getCharacterId();
				characterMng.reserveToGetJntMtx(characterId, locate_);
			}

			public void setLocate(string pLocate)
			{
				locate_ = pLocate;
			}

			public void setVisible(bool visible)
			{
				visible_ = visible;
			}

			public void setOffsetMtx(MtxFx43 offsetMtx)
			{
				offsetMtx_.copy(offsetMtx);
			}
		}
	}
}
