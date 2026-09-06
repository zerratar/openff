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
	public static partial class ds
	{
		public static partial class sys3d
		{
			public class PerspectiveInfo
			{
				public int fovySin;

				public int fovyCos;

				public int aspect;

				public int nearClip;

				public int farClip;

				public void copy(PerspectiveInfo src)
				{
					fovySin = src.fovySin;
					fovyCos = src.fovyCos;
					aspect = src.aspect;
					nearClip = src.nearClip;
					farClip = src.farClip;
				}
			}
		}
	}
}
