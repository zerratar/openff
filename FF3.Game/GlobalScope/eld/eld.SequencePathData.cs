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
	public static partial class eld
	{
		public class SequencePathData
		{
			private SEQUENCE_PATH_HEADER m_pData;

			public void SetData(SEQUENCE_PATH_HEADER pData)
			{
				m_pData = pData;
			}

			public uint GetFileType()
			{
				SEQUENCE_PATH_HEADER pData = m_pData;
				return pData.fileType;
			}

			public ushort GetMajorVersion()
			{
				SEQUENCE_PATH_HEADER pData = m_pData;
				return (ushort)(pData.version >> 16);
			}

			public ushort GetMinorVersion()
			{
				SEQUENCE_PATH_HEADER pData = m_pData;
				return (ushort)(pData.version & 0xFFFF);
			}

			public uint GetPathCount()
			{
				SEQUENCE_PATH_HEADER pData = m_pData;
				return pData.uiNumPathData;
			}

			public SPathBodyHeader GetPathData(uint index)
			{
				SEQUENCE_PATH_HEADER pData = m_pData;
				_ = pData.pIndex;
				return m_pData.pData[index];
			}
		}
	}
}
