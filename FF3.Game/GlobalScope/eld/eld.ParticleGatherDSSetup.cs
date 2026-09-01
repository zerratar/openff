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
		public class ParticleGatherDSSetup
		{
			public IParticleSetup isetup;

			public AfterImageSetup suAfter;

			public FadeSetup suFade;

			public GatherSetup suGather;

			public static explicit operator ParticleGatherDSSetup(ArrayReader src)
			{
				ParticleGatherDSSetup particleGatherDSSetup = new ParticleGatherDSSetup();
				particleGatherDSSetup.isetup = (IParticleSetup)src;
				particleGatherDSSetup.suAfter = (AfterImageSetup)src;
				particleGatherDSSetup.suFade = (FadeSetup)src;
				particleGatherDSSetup.suGather = (GatherSetup)src;
				return particleGatherDSSetup;
			}
		}
	}
}
