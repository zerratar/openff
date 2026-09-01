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
	public class SEInfoMng
	{
		public static SEInfoMng _instance = new SEInfoMng();

		public SEInfo[] _pSEInfo;

		public SEInfoMng()
		{
			_pSEInfo = null;
		}

		~SEInfoMng()
		{
		}

		public bool initialize()
		{
			return initialize_header();
		}

		public bool initialize_header()
		{
			_pSEInfo = g_SEInfoTable;
			return true;
		}

		public bool initialize_file()
		{
			return true;
		}

		public void finalize()
		{
			_pSEInfo = null;
		}

		public SEInfo getSEInfo(int SENo)
		{
			if (_pSEInfo == null)
			{
				return null;
			}
			return _pSEInfo[SENo];
		}

		public static SEInfoMng getSingleton()
		{
			return _instance;
		}
	}
}
