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
			if (SENo >= _pBGMInfo.Length || _pBGMInfo[SENo].getSeqNo() < 0)
			{
				// PORT: a tune of a mod's own under a number the table has no entry for (the
				// game's run to 58): its parts are in the content chain as sound/BGMnn_0/_1,
				// and the sequence number is the BGM number, as for every tune in the table.
				string name = string.Format(System.Globalization.CultureInfo.InvariantCulture, "BGM{0:00}", SENo);
				if (OpenFF.Client.OggSound.Has(name + "_1") || OpenFF.Client.OggSound.Has(name + "_0"))
				{
					return new BGMInfo(100 + SENo, SENo);
				}
				return SENo < _pBGMInfo.Length ? _pBGMInfo[SENo] : null;
			}
			return _pBGMInfo[SENo];
		}

		public static BGMInfoMng getSingleton()
		{
			return _Instance;
		}
	}
}
