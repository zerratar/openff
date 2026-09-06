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
using OpenFF.Platform;
using OpenFF.Resources;

internal static partial class GlobalScope
{
	public static partial class eld
	{
		public class CircleController
		{
			private CircleSetup _setup;

			private int _eRadius;

			private int _eRadiusAdd;

			private int _eAngleAdd;

			public void initialize(CircleSetup setup)
			{
				_eRadius = WIN_FX32_TO_F32(setup.nRadius);
				_eRadiusAdd = WIN_FX32_TO_F32(setup.nRadiusAdd);
				_eAngleAdd = WIN_FX32_TO_F32(setup.nAngleAdd);
			}

			public int getStartRadius()
			{
				return _eRadius;
			}

			public int getFrameAddRadius()
			{
				return _eRadiusAdd;
			}

			public int getFrameAddAngle()
			{
				return _eAngleAdd;
			}

			public int getRandomAngle()
			{
				return ExecRand(WIN_FX32_TO_F32(65535));
			}
		}
	}
}
