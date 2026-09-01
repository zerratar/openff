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
		public class ITypesArchiver
		{
			protected MICompressionHeader _hMiComp;

			protected Array _pDest;

			~ITypesArchiver()
			{
			}

			public virtual Archive.enRESULT uncompress(Array pDest, Archive.CompressInfo info)
			{
				return Archive.enRESULT.enRESULT_OK;
			}

			public virtual Archive.enRESULT prepareReadFile(Array pDest, MICompressionHeader info)
			{
				return Archive.enRESULT.enRESULT_OK;
			}

			public virtual Archive.enRESULT updateReadFile(Array pSrc, uint unReadSize)
			{
				return Archive.enRESULT.enRESULT_OK;
			}
		}
	}
}
