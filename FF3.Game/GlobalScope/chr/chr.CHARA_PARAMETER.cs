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
	public static partial class chr
	{
		public class CHARA_PARAMETER
		{
			public CHARA_MASS m_Mov = new CHARA_MASS();

			public CHARA_MASS m_Trn = new CHARA_MASS();

			public CHARA_GRV m_Grv = new CHARA_GRV();

			public CHARA_OBJECT m_Obj = new CHARA_OBJECT();

			public CHARA_COLLISION m_Col = new CHARA_COLLISION();

			public void init()
			{
				m_Mov.init();
				m_Trn.init();
				m_Grv.init();
				m_Obj.init();
				m_Col.init();
			}

			public void copy(CHARA_PARAMETER src)
			{
				m_Mov.copy(src.m_Mov);
				m_Trn.copy(src.m_Trn);
				m_Grv.copy(src.m_Grv);
				m_Obj.copy(src.m_Obj);
				m_Col.copy(src.m_Col);
			}
		}
	}
}
