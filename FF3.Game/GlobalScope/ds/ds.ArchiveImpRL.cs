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
		public class ArchiveImpRL : ITypesArchiver
		{
			private int _ctxt;

			~ArchiveImpRL()
			{
			}

			public override Archive.enRESULT uncompress(Array pDest, Archive.CompressInfo info)
			{
				if (Archive.isVramAddress(_pDest))
				{
					MI_UncompressRL16(info.pSrc, pDest);
				}
				else
				{
					MI_UncompressRL8(info.pSrc, pDest);
				}
				return Archive.enRESULT.enRESULT_OK;
			}

			public override Archive.enRESULT prepareReadFile(Array pDest, MICompressionHeader info)
			{
				_hMiComp = info;
				_pDest = pDest;
				MI_InitUncompContextRL(_ctxt, (byte[])pDest, _hMiComp);
				return Archive.enRESULT.enRESULT_OK;
			}

			public override Archive.enRESULT updateReadFile(Array pSrc, uint unReadSize)
			{
				if (((!Archive.isVramAddress(_pDest)) ? MI_ReadUncompRL8(_ctxt, (byte[])pSrc, unReadSize) : MI_ReadUncompRL16(_ctxt, (byte[])pSrc, unReadSize)) != 0)
				{
					return Archive.enRESULT.enRESULT_OK;
				}
				return Archive.enRESULT.enRESULT_STREAMING_END;
			}
		}
	}
}
