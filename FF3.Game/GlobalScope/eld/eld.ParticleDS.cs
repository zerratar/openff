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
	public static partial class eld
	{
		public class ParticleDS : IParticle
		{
			public ds.Vector3<int> _vVelocity = new ds.Vector3<int>();

			public ds.Vector3<int> _vGravity = new ds.Vector3<int>();

			public int _eCircleRadius;

			public int _eCircleAngle;

			~ParticleDS()
			{
				destruct();
			}

			public void destruct()
			{
			}

			public override void update(IGroup group)
			{
			}

			public void update(IGroup group, ds.Vector3<int> offset, spr.Eff_FRGBA color)
			{
				_vVelocity += _vGravity;
				_vBaseCenter += _vVelocity;
				ds.Vector3<int> center = _pPrimitive.Center;
				center.copy(_vBaseCenter);
				center.vx += DsEffMul(_eCircleRadius, DsEffSin(_eCircleAngle));
				center.vz += DsEffMul(_eCircleRadius, DsEffCos(_eCircleAngle));
				ut.setColorToPrimitive(_pPrimitive, color);
				_pPrimitive.Disp = (short)((_pPrimitive.Color[3] != 0) ? 3 : 0);
				base.update(group);
			}

			public void setCircleParam(int radius, int angle)
			{
				_eCircleRadius = radius;
				_eCircleAngle = angle;
			}

			public void addCircleRadius(int radius)
			{
				_eCircleRadius += radius;
			}

			public void addCircleAngle(int angle)
			{
				_eCircleAngle += angle;
			}
		}
	}
}
