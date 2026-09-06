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
		public class IParticle
		{
			public ds.pt.Particle _pPrimitive;

			public ds.Vector3<int> _vBaseCenter = new ds.Vector3<int>();

			public int _enSize;

			public IParticle()
			{
				_pPrimitive = null;
			}

			~IParticle()
			{
			}

			public virtual void update(IGroup group)
			{
				spr.EffSprForm animation = group.getAnimation();
				_pPrimitive.Size.vx = (short)(animation.fSclData.nx * _enSize >> 12);
				_pPrimitive.Size.vy = (short)(animation.fSclData.ny * _enSize >> 12);
				_pPrimitive.St[0].vx = (int)animation.uiUVData[0];
				_pPrimitive.St[0].vy = (int)animation.uiUVData[1];
				_pPrimitive.St[1].vx = (int)animation.uiUVData[2];
				_pPrimitive.St[1].vy = (int)animation.uiUVData[3];
			}

			public void setPrimitive(ds.pt.Particle pPrim)
			{
				_pPrimitive = pPrim;
			}

			public ds.pt.Particle getPrimitive()
			{
				return _pPrimitive;
			}

			public void setCenterPosition(ds.Vector3<int> pos)
			{
				_vBaseCenter.copy(pos);
			}
		}
	}
}
