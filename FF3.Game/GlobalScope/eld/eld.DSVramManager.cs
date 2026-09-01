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
		public class DSVramManager : IVramManager
		{
			private TextureVramManager _mng = new TextureVramManager();

			~DSVramManager()
			{
			}

			public override void initialize()
			{
				uint unMaxSizeTexel = 49152u;
				uint unMaxSizePalette = uint.MaxValue;
				_mng.initialize(unMaxSizeTexel, unMaxSizePalette);
			}

			public override void cleanup()
			{
				_mng.cleanup();
			}

			public override bool registerTexture(ds.Texture pTx)
			{
				return _mng.registerTexture(reinterpret_cast<ds.Texture>(pTx));
			}

			public override void deregisterTexture(ds.Texture pTx)
			{
				_mng.deregisterTexture(reinterpret_cast<ds.Texture>(pTx));
			}

			public override ModelTexture registerModelTexture(Array pTx)
			{
				return _mng.registerModelTexture(pTx);
			}

			public override void deregisterModelTexture(ModelTexture pTx)
			{
				_mng.deregisterModelTexture(pTx);
			}
		}
	}
}
