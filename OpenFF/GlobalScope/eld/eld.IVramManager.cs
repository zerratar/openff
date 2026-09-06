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
		public class IVramManager
		{
			~IVramManager()
			{
			}

			public virtual void initialize()
			{
			}

			public virtual void cleanup()
			{
			}

			public virtual bool registerTexture(ds.Texture pTx)
			{
				return false;
			}

			public virtual void deregisterTexture(ds.Texture pTx)
			{
			}

			public virtual ModelTexture registerModelTexture(Array pTx)
			{
				return null;
			}

			public virtual void deregisterModelTexture(ModelTexture pTx)
			{
			}
		}
	}
}
