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
	public static partial class ds
	{
		public static partial class sys3d
		{
			public class CMotNode : CAnimation
			{
				public uint m_Index;

				public ncap.SMotionFileHeader m_pMotData;

				public CMotNode()
				{
					m_Index = 0u;
					m_pMotData = null;
				}

				~CMotNode()
				{
				}

				public uint getIndex()
				{
					return m_Index;
				}

				public void setIndex(uint idx)
				{
					m_Index = idx;
				}
			}
		}
	}
}
