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
		public class IParticleExec
		{
			public ds.Vector3<int> posBase = new ds.Vector3<int>();

			public ds.Vector3<int> vScale;

			public ushort nbParticles;

			public ushort numCurrentGroup;

			public ushort usTimeLife;

			public ushort usTimeNextGroup;

			public ushort state;

			public ushort nextID;

			public bool bPlay;
		}
	}
}
