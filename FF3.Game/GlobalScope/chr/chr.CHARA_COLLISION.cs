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
		public class CHARA_COLLISION
		{
			public int m_ColFlag;

			public int m_ColType;

			public int m_ColRadius;

			public int m_CckRadius;

			public int m_TchRadius;

			public VecFx32 m_ColOffset = new VecFx32();

			public VecFx32 m_CckOffset = new VecFx32();

			public VecFx32 m_TchOffset = new VecFx32();

			public VecFx32 m_ColAabbRadius = new VecFx32();

			public void init()
			{
				m_ColFlag = 0;
				m_ColType = 0;
				m_ColRadius = 0;
				m_CckRadius = 0;
				m_TchRadius = 0;
				VEC_Set(m_ColOffset, 0, 0, 0);
				VEC_Set(m_CckOffset, 0, 0, 0);
				VEC_Set(m_TchOffset, 0, 0, 0);
				VEC_Set(m_ColAabbRadius, 0, 0, 0);
			}

			public void copy(CHARA_COLLISION src)
			{
				m_ColFlag = src.m_ColFlag;
				m_ColType = src.m_ColType;
				m_ColRadius = src.m_ColRadius;
				m_CckRadius = src.m_CckRadius;
				m_TchRadius = src.m_TchRadius;
				m_ColOffset.copy(src.m_ColOffset);
				m_CckOffset.copy(src.m_CckOffset);
				m_TchOffset.copy(src.m_TchOffset);
				m_ColAabbRadius.copy(src.m_ColAabbRadius);
			}
		}
	}
}
