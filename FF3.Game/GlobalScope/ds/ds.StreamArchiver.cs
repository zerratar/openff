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
using java.io;
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
	public static partial class ds
	{
		public class StreamArchiver
		{
			private ArchiveImp _imp;

			private Archive.CompressInfo _info = new Archive.CompressInfo();

			public StreamArchiver()
			{
				_imp = null;
			}

			~StreamArchiver()
			{
				deleteArchiveImplement(_imp);
				_imp = null;
			}

			public Archive.enRESULT analysisReadFile(Archive.CompressInfo info, string szFile)
			{
				safeCreateImplement();
				Archive.enRESULT result = _imp.analysisReadFile(info, szFile);
				_info.copy(info);
				return result;
			}

			public Archive.enRESULT prepareReadFile(Array pDest, uint unWorkSize)
			{
				safeCreateImplement();
				return _imp.prepareReadFile(pDest, unWorkSize, _info);
			}

			public Archive.enRESULT uncompressReadFile(uint unReadSize)
			{
				safeCreateImplement();
				return _imp.uncompressReadFile(unReadSize);
			}

			public void cancelReadFile()
			{
				safeCreateImplement();
				_imp.cancelReadFile();
			}

			public bool isReadFile()
			{
				safeCreateImplement();
				return _imp.isReadFile();
			}

			public void safeCreateImplement()
			{
				if (_imp == null)
				{
					_imp = createArchiveImplement();
				}
			}
		}
	}
}
