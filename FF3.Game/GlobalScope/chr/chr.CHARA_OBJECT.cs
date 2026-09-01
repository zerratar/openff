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
	public static partial class chr
	{
		public class CHARA_OBJECT
		{
			public VecFx32 m_Dir = new VecFx32();

			public VecFx32 m_TargetDir = new VecFx32();

			public VecFx32 m_Pos = new VecFx32();

			public VecFx32 m_Rot = new VecFx32();

			public VecFx32 m_Scl = new VecFx32();

			public void init()
			{
				VEC_Set(m_Dir, 0, 0, 1);
				VEC_Set(m_TargetDir, 0, 0, 0);
				VEC_Set(m_Pos, 0, 0, 0);
				VEC_Set(m_Rot, 0, 0, 0);
				VEC_Set(m_Scl, 0, 0, 0);
			}

			public void copy(CHARA_OBJECT src)
			{
				m_Dir.copy(src.m_Dir);
				m_TargetDir.copy(src.m_TargetDir);
				m_Pos.copy(src.m_Pos);
				m_Rot.copy(src.m_Rot);
				m_Scl.copy(src.m_Scl);
			}

			public CHARA_OBJECT()
			{
			}

			public CHARA_OBJECT(CHARA_OBJECT src)
			{
				copy(src);
			}
		}
	}
}
