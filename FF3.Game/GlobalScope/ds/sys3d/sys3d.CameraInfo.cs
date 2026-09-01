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
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
	public static partial class ds
	{
		public static partial class sys3d
		{
			public class CameraInfo
			{
				public VecFx32 position = new VecFx32();

				public VecFx32 target = new VecFx32();

				public VecFx32 direction = new VecFx32();

				public VecFx32 camUp = new VecFx32();

				public MtxFx43 matrix;

				public void copy(CameraInfo src)
				{
					position.copy(src.position);
					target.copy(src.target);
					direction.copy(src.direction);
					camUp.copy(src.camUp);
					matrix = src.matrix;
				}
			}
		}
	}
}
