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
	public static partial class ds
	{
		public class ArchiveImpHuffman : ITypesArchiver
		{
			private int _ctxt;

			~ArchiveImpHuffman()
			{
			}

			public override Archive.enRESULT uncompress(Array pDest, Archive.CompressInfo info)
			{
				MI_UncompressHuffman(info.pSrc, pDest);
				return Archive.enRESULT.enRESULT_OK;
			}

			public override Archive.enRESULT prepareReadFile(Array pDest, MICompressionHeader info)
			{
				_hMiComp = info;
				_pDest = pDest;
				MI_InitUncompContextHuffman(_ctxt, (byte[])pDest, _hMiComp);
				return Archive.enRESULT.enRESULT_OK;
			}

			public override Archive.enRESULT updateReadFile(Array pSrc, uint unReadSize)
			{
				if (MI_ReadUncompHuffman(_ctxt, (byte[])pSrc, unReadSize) != 0)
				{
					return Archive.enRESULT.enRESULT_OK;
				}
				return Archive.enRESULT.enRESULT_STREAMING_END;
			}
		}
	}
}
