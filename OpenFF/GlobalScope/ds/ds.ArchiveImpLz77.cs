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
		public class ArchiveImpLz77 : ITypesArchiver
		{
			private MIUncompContextLZ _ctxt = new MIUncompContextLZ();

			~ArchiveImpLz77()
			{
			}

			public override Archive.enRESULT uncompress(Array pDest, Archive.CompressInfo info)
			{
				if (Archive.isVramAddress(pDest))
				{
					MI_UncompressLZ16(info.pSrc, pDest);
				}
				else
				{
					MI_UncompressLZ8(info.pSrc, pDest);
				}
				return Archive.enRESULT.enRESULT_OK;
			}

			public override Archive.enRESULT prepareReadFile(Array pDest, MICompressionHeader info)
			{
				_hMiComp = info;
				_pDest = pDest;
				MI_InitUncompContextLZ(_ctxt, (byte[])pDest, _hMiComp);
				return Archive.enRESULT.enRESULT_OK;
			}

			public override Archive.enRESULT updateReadFile(Array pSrc, uint unReadSize)
			{
				if (((!Archive.isVramAddress(_pDest)) ? MI_ReadUncompLZ8(_ctxt, (byte[])pSrc, unReadSize) : MI_ReadUncompLZ16(_ctxt, (byte[])pSrc, unReadSize)) != 0)
				{
					return Archive.enRESULT.enRESULT_OK;
				}
				return Archive.enRESULT.enRESULT_STREAMING_END;
			}
		}
	}
}
