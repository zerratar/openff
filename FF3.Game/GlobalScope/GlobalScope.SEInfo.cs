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
	public class SEInfo
	{
		private int _SeqArcNo;

		private int _SeqIndex;

		public SEInfo()
		{
			_SeqArcNo = -1;
			_SeqIndex = -1;
		}

		public SEInfo(int SeqArcNo, int SeqIndex)
		{
			_SeqArcNo = SeqArcNo;
			_SeqIndex = SeqIndex;
		}

		public int getSeqArcNo()
		{
			return _SeqArcNo;
		}

		public int getSeqIndex()
		{
			return _SeqIndex;
		}
	}
}
