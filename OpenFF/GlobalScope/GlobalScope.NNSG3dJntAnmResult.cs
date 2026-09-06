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
	public class NNSG3dJntAnmResult
	{
		private NNSG3dJntAnmResultFlag flag;

		private VecFx32 scale;

		private VecFx32 scaleEx0;

		private VecFx32 scaleEx1;

		private MtxFx33 rot;

		private VecFx32 trans;
	}
}
