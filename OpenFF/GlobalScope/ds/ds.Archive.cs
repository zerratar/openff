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
		public class Archive
		{
			public enum enCOMP_TYPE
			{
				enCOMP_LZ,
				enCOMP_HUFFMAN,
				enCOMP_RL,
				enCOMP_DIFF,
				enCOMP_INVALID
			}

			public enum enRESULT
			{
				enRESULT_OK,
				enRESULT_INVALID_TYPE,
				enRESULT_NOT_FOUND_FILE,
				enRESULT_UNINITIALIZED,
				enRESULT_STREAMING_END
			}

			public class CompressInfo
			{
				public enCOMP_TYPE enCompType;

				public uint unCompParam;

				public uint unExtractSize;

				public Array pSrc;

				public void copy(CompressInfo src)
				{
					enCompType = src.enCompType;
					unCompParam = src.unCompParam;
					unExtractSize = src.unExtractSize;
					pSrc = src.pSrc;
				}
			}

			public const enCOMP_TYPE enCOMP_LZ = enCOMP_TYPE.enCOMP_LZ;

			public const enCOMP_TYPE enCOMP_HUFFMAN = enCOMP_TYPE.enCOMP_HUFFMAN;

			public const enCOMP_TYPE enCOMP_RL = enCOMP_TYPE.enCOMP_RL;

			public const enCOMP_TYPE enCOMP_DIFF = enCOMP_TYPE.enCOMP_DIFF;

			public const enCOMP_TYPE enCOMP_INVALID = enCOMP_TYPE.enCOMP_INVALID;

			public const enRESULT enRESULT_OK = enRESULT.enRESULT_OK;

			public const enRESULT enRESULT_INVALID_TYPE = enRESULT.enRESULT_INVALID_TYPE;

			public const enRESULT enRESULT_NOT_FOUND_FILE = enRESULT.enRESULT_NOT_FOUND_FILE;

			public const enRESULT enRESULT_UNINITIALIZED = enRESULT.enRESULT_UNINITIALIZED;

			public const enRESULT enRESULT_STREAMING_END = enRESULT.enRESULT_STREAMING_END;

			public static bool isSupportCompressType(MICompressionHeader header)
			{
				return true;
			}

			public static bool isVramAddress(Array pDest)
			{
				return false;
			}
		}
	}
}
