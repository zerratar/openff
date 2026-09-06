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
	public static partial class eld
	{
		public class IParticleLarge
		{
			public ds.pt.LargeParticle _pPrimitive;

			public int _enSize;

			public IParticleLarge()
			{
				_pPrimitive = null;
			}

			~IParticleLarge()
			{
			}

			public virtual void update(IGroupLarge group)
			{
				spr.EffSprForm animation = group.getAnimation();
				_pPrimitive.Size.vx = animation.fSclData.nx * _enSize >> 12;
				_pPrimitive.Size.vy = animation.fSclData.ny * _enSize >> 12;
				_pPrimitive.St[0].vx = (int)animation.uiUVData[0];
				_pPrimitive.St[0].vy = (int)animation.uiUVData[1];
				_pPrimitive.St[1].vx = (int)animation.uiUVData[2];
				_pPrimitive.St[1].vy = (int)animation.uiUVData[3];
			}

			public void setPrimitive(ds.pt.LargeParticle pPrim)
			{
				_pPrimitive = pPrim;
			}

			public ds.pt.LargeParticle getPrimitive()
			{
				return _pPrimitive;
			}
		}
	}
}
