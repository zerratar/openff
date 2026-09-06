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
	public static partial class evt
	{
		public class SEventJumpParameter
		{
			public string m_Title;

			public string m_MapName;

			public float[] m_Pos = new float[3];

			public float[] m_SidPos = new float[3];

			public sbyte m_SidFieldNo;

			public short[] m_PossessionItem;

			public int m_PossessionGold;

			public SCharacterParameter[] m_PC;

			public short m_NPC;

			public short[] m_GlobalFlag;

			public short[] m_TreasureFlag;

			public SEventJumpParameter(string arg0)
			{
				m_Title = arg0;
			}

			public SEventJumpParameter(string arg0, string arg1, float[] arg2, float[] arg3, sbyte arg4, short[] arg5, int arg6, SCharacterParameter[] arg7, short arg8, short[] arg9, short[] arg10)
			{
				m_Title = arg0;
				m_MapName = arg1;
				for (int i = 0; i < 3; i++)
				{
					m_Pos[i] = arg2[i];
				}
				for (int i = 0; i < 3; i++)
				{
					m_SidPos[i] = arg3[i];
				}
				m_SidFieldNo = arg4;
				m_PossessionItem = arg5;
				m_PossessionGold = arg6;
				m_PC = arg7;
				m_NPC = arg8;
				m_GlobalFlag = arg9;
				m_TreasureFlag = arg10;
			}
		}
	}
}
