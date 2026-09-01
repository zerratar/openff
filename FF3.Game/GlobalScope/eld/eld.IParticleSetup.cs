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
		public class IParticleSetup
		{
			public uint version;

			public uint flag;

			public RangeSetup suRange;

			public SizeSetup suSize;

			public ushort usTimePlay;

			public ushort usTimeGroupLife;

			public ushort usTimeInterval;

			public ushort nbChilds;

			public ushort nbGroups;

			public ushort _pad0;

			public static explicit operator IParticleSetup(ArrayReader src)
			{
				IParticleSetup particleSetup = new IParticleSetup();
				particleSetup.version = src.readUInt32();
				particleSetup.flag = src.readUInt32();
				particleSetup.suRange = (RangeSetup)src;
				particleSetup.suSize = (SizeSetup)src;
				particleSetup.usTimePlay = src.readUInt16();
				particleSetup.usTimeGroupLife = src.readUInt16();
				particleSetup.usTimeInterval = src.readUInt16();
				particleSetup.nbChilds = src.readUInt16();
				particleSetup.nbGroups = src.readUInt16();
				particleSetup._pad0 = src.readUInt16();
				return particleSetup;
			}
		}
	}
}
