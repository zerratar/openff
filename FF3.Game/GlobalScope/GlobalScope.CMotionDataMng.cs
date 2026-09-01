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
	public class CMotionDataMng
	{
		public class _MotionData
		{
			public uint flag;

			public string name;

			public uint useCount;

			public CFileData motData = new CFileData();

			public NHMotionData NHMotion = new NHMotionData();
		}

		protected const int MOTION_MAX = 32;

		public _MotionData[] MotionData = new _MotionData[32];

		protected uint m_totalFileSize;

		public void init()
		{
			for (int i = 0; i < 32; i++)
			{
				cleanupData(i);
			}
		}

		public void end()
		{
			for (int i = 0; i < 32; i++)
			{
				cleanupData(i);
			}
		}

		public int setData(string motname, bool async)
		{
			int num = -1;
			num = searchDataIndex(motname);
			if (-1 != num)
			{
				MotionData[num].useCount++;
				return num;
			}
			num = searchNullIndex();
			if (-1 == num)
			{
				return -1;
			}
			ds.fs.enFDL_FILETYPE type = ds.fs.enFDL_FILETYPE.enFDL_FILETYPE_COMPRESS;
			strcpy(out MotionData[num].name, motname);
			sprintf(out var arg, "%s.ncap.lz", motname);
			if (async)
			{
				MotionData[num].motData.setupAsync(arg, type, MotionData[num].NHMotion);
				if (0 >= MotionData[num].motData.getSize())
				{
					return -1;
				}
				MotionData[num].NHMotion.init(async: true);
			}
			else
			{
				MotionData[num].motData.setup(arg, type);
				if (0 >= MotionData[num].motData.getSize())
				{
					return -1;
				}
			}
			MotionData[num].flag = 1u;
			MotionData[num].useCount = 1u;
			m_totalFileSize += MotionData[num].motData.getSize();
			return num;
		}

		public void delData(int ctrl)
		{
			if (MotionData[ctrl].useCount != 0 && MotionData[ctrl].flag != 0)
			{
				MotionData[ctrl].useCount--;
				if (0 >= MotionData[ctrl].useCount)
				{
					m_totalFileSize -= MotionData[ctrl].motData.getSize();
					cleanupData(ctrl);
				}
			}
		}

		public int searchNullIndex()
		{
			for (int i = 0; i < 32; i++)
			{
				if (MotionData[i].flag == 0)
				{
					return i;
				}
			}
			return -1;
		}

		public int searchDataIndex(string name)
		{
			for (int i = 0; i < 32; i++)
			{
				if (MotionData[i].flag != 0 && strcmp(MotionData[i].name, name) == 0)
				{
					return i;
				}
			}
			return -1;
		}

		public void cleanupData(int ctrl)
		{
			MotionData[ctrl].flag = 0u;
			MotionData[ctrl].name = "";
			MotionData[ctrl].useCount = 0u;
			MotionData[ctrl].motData.cleanup();
		}

		public CMotionDataMng()
		{
			for (int i = 0; i < MotionData.Length; i++)
			{
				MotionData[i] = new _MotionData();
			}
		}

		public bool isLoaded(int ctrl)
		{
			return MotionData[ctrl].NHMotion.isLoaded();
		}

		public uint getTotalSize()
		{
			return m_totalFileSize;
		}
	}
}
