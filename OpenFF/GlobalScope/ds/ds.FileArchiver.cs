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
	public static partial class ds
	{
		public class FileArchiver
		{
			private ArchiveImp _imp;

			private Archive.CompressInfo _info;

			public FileArchiver()
			{
				_imp = null;
			}

			~FileArchiver()
			{
				cancelFile();
				deleteArchiveImplement(_imp);
				_imp = null;
			}

			public Archive.enRESULT analysisFile(Archive.CompressInfo info, string szFilename)
			{
				safeCreateImplement();
				Archive.enRESULT result = _imp.analysisReadFile(info, szFilename);
				_info = info;
				return result;
			}

			public Archive.enRESULT uncompressFile(Array pDest)
			{
				safeCreateImplement();
				if (!_imp.isOpen())
				{
					return Archive.enRESULT.enRESULT_UNINITIALIZED;
				}
				uint num = 16384u;
				Archive.enRESULT enRESULT = _imp.prepareReadFile(pDest, num, _info);
				if (enRESULT != Archive.enRESULT.enRESULT_OK && enRESULT != Archive.enRESULT.enRESULT_STREAMING_END)
				{
					cancelFile();
					return enRESULT;
				}
				do
				{
					enRESULT = _imp.uncompressReadFile(num);
				}
				while (enRESULT != Archive.enRESULT.enRESULT_STREAMING_END);
				_imp.releaseWork();
				DC_FlushRange(pDest, _info.unExtractSize);
				return enRESULT;
			}

			public void cancelFile()
			{
				safeCreateImplement();
				_imp.cancelReadFile();
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
