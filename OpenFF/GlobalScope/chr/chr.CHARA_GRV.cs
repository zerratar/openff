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
using OpenFF.Platform;
using OpenFF.Resources;

internal static partial class GlobalScope
{
	public static partial class chr
	{
		public class CHARA_GRV
		{
			public bool m_GrvFlag;

			public int m_GrvAcc;

			public void init()
			{
				m_GrvFlag = false;
				m_GrvAcc = 0;
			}

			public void copy(CHARA_GRV src)
			{
				m_GrvFlag = src.m_GrvFlag;
				m_GrvAcc = src.m_GrvAcc;
			}
		}
	}
}
