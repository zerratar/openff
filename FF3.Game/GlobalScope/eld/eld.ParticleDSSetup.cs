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
		public class ParticleDSSetup
		{
			public IParticleSetup isetup;

			public SpeedSetup suSpeed;

			public GravitySetup suGravity;

			public EmmitSetup suEmmit;

			public AfterImageSetup suAfter;

			public FadeSetup suFade;

			public CircleSetup suCircle;

			public SizeSpreadSetup suSizeSpread;

			public static explicit operator ParticleDSSetup(ArrayReader src)
			{
				ParticleDSSetup particleDSSetup = new ParticleDSSetup();
				particleDSSetup.isetup = (IParticleSetup)src;
				particleDSSetup.suSpeed = (SpeedSetup)src;
				particleDSSetup.suGravity = (GravitySetup)src;
				particleDSSetup.suEmmit = (EmmitSetup)src;
				particleDSSetup.suAfter = (AfterImageSetup)src;
				particleDSSetup.suFade = (FadeSetup)src;
				particleDSSetup.suCircle = (CircleSetup)src;
				particleDSSetup.suSizeSpread = (SizeSpreadSetup)src;
				return particleDSSetup;
			}
		}
	}
}
