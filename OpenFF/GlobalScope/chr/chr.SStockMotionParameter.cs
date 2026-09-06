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
		public class SStockMotionParameter
		{
			public bool m_StockMotionEnd;

			public bool m_Loop;

			public int m_MotionIndex;

			public int m_MotionBlend;

			public int m_PlayFrame;

			public void initialize()
			{
				m_StockMotionEnd = false;
				m_Loop = true;
				m_MotionIndex = -1;
				m_MotionBlend = 5;
				m_PlayFrame = -1;
			}

			public void setup(int _MotionIndex, int _PlayFrame, bool _Loop, int _MotionBlend)
			{
				m_StockMotionEnd = _PlayFrame == 0;
				m_Loop = _Loop;
				m_MotionIndex = _MotionIndex;
				m_MotionBlend = _MotionBlend;
				m_PlayFrame = _PlayFrame;
			}
		}
	}
}
