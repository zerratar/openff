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
	public static partial class ds
	{
		public class DataArchiver
		{
			private ArchiveImp _imp;

			private Archive.CompressInfo _info;

			public DataArchiver()
			{
				_imp = null;
			}

			~DataArchiver()
			{
				deleteArchiveImplement(_imp);
				_imp = null;
			}

			public Archive.enRESULT analysisData(Archive.CompressInfo info, Array pData)
			{
				safeCreateImplement();
				MICompressionHeader header = static_cast<MICompressionHeader>(pData);
				if (Archive.isSupportCompressType(header))
				{
					setCompressInfo(info, header);
					info.pSrc = pData;
					_info = info;
					return Archive.enRESULT.enRESULT_OK;
				}
				return Archive.enRESULT.enRESULT_INVALID_TYPE;
			}

			public Archive.enRESULT uncompressData(Array pDest)
			{
				safeCreateImplement();
				return _imp.uncompressData(pDest, _info);
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
