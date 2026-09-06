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
		public class ParticleLargeDS : IParticleLarge
		{
			public ds.Vector3<int> _vVelocity = new ds.Vector3<int>();

			public ds.Vector3<int> _vGravity = new ds.Vector3<int>();

			~ParticleLargeDS()
			{
				destruct();
			}

			public void destruct()
			{
			}

			public override void update(IGroupLarge group)
			{
				base.update(group);
				_pPrimitive.Disp = (short)((_pPrimitive.Color[3] != 0) ? 3 : 0);
				_vVelocity += _vGravity;
				_pPrimitive.Center[0] += _vVelocity.vx;
				_pPrimitive.Center[1] += _vVelocity.vy;
				_pPrimitive.Center[2] += _vVelocity.vz;
			}

			public void update(IGroupLarge group, spr.Eff_FRGBA color)
			{
				ut.setColorToPrimitive(_pPrimitive, color);
				update(group);
			}
		}
	}
}
