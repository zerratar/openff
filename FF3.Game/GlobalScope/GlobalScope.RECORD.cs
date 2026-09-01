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
	public class RECORD
	{
		public MatrixSound.MtxSEHandle handle;

		public int SeqArcNo;

		public int SeqNo;

		public RECORD(MatrixSound.MtxSEHandle arg0, int arg1, int arg2)
		{
			handle = arg0;
			SeqArcNo = arg1;
			SeqNo = arg2;
		}
	}
}
