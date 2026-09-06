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
	public class NHMotionData : ds.fs.RequestObject.NotifyHandler
	{
		private bool _loaded;

		public override void notifyCompletion(ds.fs.enFDL_RESULT ret)
		{
			if (ret == ds.fs.enFDL_RESULT.enFDL_RESULT_OK)
			{
				_loaded = true;
			}
		}

		public NHMotionData()
		{
			_loaded = true;
		}

		public void init(bool async)
		{
			_loaded = !async;
		}

		public bool isLoaded()
		{
			return _loaded;
		}
	}
}
