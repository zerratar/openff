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
	public class BGMInfo
	{
		private int _GroupNo;

		private int _SeqNo;

		public BGMInfo()
		{
			_SeqNo = -1;
			_GroupNo = -1;
		}

		public BGMInfo(int GroupNo, int SeqNo)
		{
			_GroupNo = GroupNo;
			_SeqNo = SeqNo;
		}

		public int getSeqNo()
		{
			return _SeqNo;
		}

		public int getGroupNo()
		{
			return _GroupNo;
		}
	}
}
