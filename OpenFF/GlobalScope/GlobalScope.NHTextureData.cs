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
	public class NHTextureData : ds.fs.RequestObject.NotifyHandler
	{
		private bool _async;

		private bool _loaded;

		private CTextureDataMng.texture_data _pTextureData;

		public void init(bool async, CTextureDataMng.texture_data pTex)
		{
			_async = async;
			_loaded = !async;
			_pTextureData = pTex;
		}

		public override void notifyCompletion(ds.fs.enFDL_RESULT ret)
		{
			_loaded = true;
			if (_pTextureData != null)
			{
				CTextureDataMng.texture_data pTextureData = _pTextureData;
				pTextureData.tex.setup(pTextureData.texData.getAddr(), _async);
			}
		}

		public NHTextureData()
		{
			_loaded = true;
			_pTextureData = null;
			_async = false;
		}

		public bool isLoaded()
		{
			return _loaded;
		}
	}
}
