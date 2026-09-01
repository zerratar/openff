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
	public class MIUncompContextLZ
	{
		public byte[] destp;

		public int destCount;

		public uint length;

		public ushort destTmp;

		public byte destTmpCnt;

		public byte flags;

		public byte flagIndex;

		public byte lengthFlg;

		public byte exFormat;

		public byte[] _padding = new byte[1];

		public int m_iOffset;

		public void setDefault()
		{
			destp = null;
			destCount = 0;
			length = 0u;
			destTmp = 0;
			destTmpCnt = 0;
			flags = 0;
			flagIndex = 0;
			lengthFlg = 0;
			exFormat = 0;
			_padding[0] = 0;
			m_iOffset = 0;
		}
	}
}
