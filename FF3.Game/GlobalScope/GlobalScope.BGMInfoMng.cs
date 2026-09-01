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
	public class BGMInfoMng
	{
		public static BGMInfoMng _Instance = new BGMInfoMng();

		public BGMInfo[] _pBGMInfo;

		public BGMInfoMng()
		{
			_pBGMInfo = null;
		}

		~BGMInfoMng()
		{
		}

		public bool initialize()
		{
			return initialize_header();
		}

		public bool initialize_header()
		{
			_pBGMInfo = g_BGMInfoTable;
			return true;
		}

		public bool initialize_file()
		{
			return false;
		}

		public void finalize()
		{
			_pBGMInfo = null;
		}

		public BGMInfo getBGMInfo(int SENo)
		{
			if (_pBGMInfo == null)
			{
				return null;
			}
			if (SENo < 0)
			{
				SENo = 0;
			}
			return _pBGMInfo[SENo];
		}

		public static BGMInfoMng getSingleton()
		{
			return _Instance;
		}
	}
}
