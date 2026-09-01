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
	public static partial class evt
	{
		public class SCharacterParameter
		{
			public short m_Index;

			public short m_Level;

			public pl.JOB_TYPE m_Job;

			public short[] m_EquipArms = new short[5];

			public short[,] m_EquipMagic = new short[8, pl.MAGIC_ONCE_LEVEL_EQUIP_MAX];

			public SCharacterParameter(short arg0, short arg1, pl.JOB_TYPE arg2, short[] arg3, short[,] arg4)
			{
				m_Index = arg0;
				m_Level = arg1;
				m_Job = arg2;
				for (int i = 0; i < 5; i++)
				{
					m_EquipArms[i] = arg3[i];
				}
				for (int i = 0; i < 8; i++)
				{
					for (int j = 0; j < pl.MAGIC_ONCE_LEVEL_EQUIP_MAX; j++)
					{
						m_EquipMagic[i, j] = arg4[i, j];
					}
				}
			}
		}
	}
}
