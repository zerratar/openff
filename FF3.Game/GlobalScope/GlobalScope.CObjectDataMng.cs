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
	public class CObjectDataMng
	{
		public class _ObjectData
		{
			public uint flag;

			public uint useCount;

			public string name;

			public CFileData MdlData = new CFileData();

			public CFileData AnmData = new CFileData();

			public CFileData TexData = new CFileData();

			public NHObjectData NHMdl = new NHObjectData();

			public NHObjectData NHAnm = new NHObjectData();

			public NHObjectData NHTex = new NHObjectData();
		}

		protected const int object_max = 22;

		public _ObjectData[] ObjectData = new _ObjectData[22];

		protected uint m_totalFileSize;

		public void init()
		{
			m_totalFileSize = 0u;
		}

		public void end()
		{
			for (byte b = 0; b < 22; b++)
			{
				initValue(b);
			}
		}

		public int setData(string mdlname, bool async)
		{
			uint num = (uint)searchNullIndex();
			if (-1 == (int)num)
			{
				return -1;
			}
			ObjectData[num].flag = 1u;
			ObjectData[num].useCount++;
			strcpy(out ObjectData[num].name, mdlname);
			ds.fs.enFDL_FILETYPE type = ds.fs.enFDL_FILETYPE.enFDL_FILETYPE_COMPRESS;
			string arg = "";
			string arg2 = "";
			string arg3 = "";
			sprintf(out arg, "%s.nmdp.lz", mdlname);
			sprintf(out arg2, "%s.namp.lz", mdlname);
			sprintf(out arg3, "%s.nsbtx.lz", mdlname);
			if (async)
			{
				if (!ObjectData[num].MdlData.setupAsync(arg, type, ObjectData[num].NHMdl))
				{
					return -1;
				}
				ObjectData[num].NHMdl.init(async);
				m_totalFileSize += ObjectData[num].MdlData.getSize();
				if (ObjectData[num].AnmData.setupAsync(arg2, type, ObjectData[num].NHAnm))
				{
					ObjectData[num].NHAnm.init(async);
					m_totalFileSize += ObjectData[num].AnmData.getSize();
				}
				if (ObjectData[num].TexData.setupAsync(arg3, type, ObjectData[num].NHTex))
				{
					ObjectData[num].NHTex.init(async);
					m_totalFileSize += ObjectData[num].TexData.getSize();
				}
			}
			else
			{
				if (ds.g_File.getSize(arg) == 0)
				{
					return -1;
				}
				if (!ObjectData[num].MdlData.setup(arg, type))
				{
					return -1;
				}
				m_totalFileSize += ObjectData[num].MdlData.getSize();
				if (ds.g_File.getSize(arg2) != 0 && ObjectData[num].AnmData.setup(arg2, type))
				{
					m_totalFileSize += ObjectData[num].AnmData.getSize();
				}
				if (ds.g_File.getSize(arg3) != 0 && ObjectData[num].TexData.setup(arg3, type))
				{
					m_totalFileSize += ObjectData[num].TexData.getSize();
				}
			}
			return (int)num;
		}

		public void delData(int ctrl)
		{
			ObjectData[ctrl].useCount--;
			if (ObjectData[ctrl].useCount == 0)
			{
				m_totalFileSize -= ObjectData[ctrl].MdlData.getSize();
				m_totalFileSize -= ObjectData[ctrl].AnmData.getSize();
				m_totalFileSize -= ObjectData[ctrl].TexData.getSize();
				initValue(ctrl);
			}
		}

		public int searchNullIndex()
		{
			for (int i = 0; i < 22; i++)
			{
				if (ObjectData[i].flag == 0)
				{
					return i;
				}
			}
			return -1;
		}

		public int searchDataIndex(string name)
		{
			for (int i = 0; i < 22; i++)
			{
				if (ObjectData[i].flag != 0 && strcmp(ObjectData[i].name, name) == 0)
				{
					return i;
				}
			}
			return -1;
		}

		public void initValue(int ctrl)
		{
			if (ctrl >= 0 && 22 > ctrl)
			{
				ObjectData[ctrl].flag = 0u;
				ObjectData[ctrl].useCount = 0u;
				ObjectData[ctrl].name = "";
				ObjectData[ctrl].MdlData.cleanup();
				ObjectData[ctrl].AnmData.cleanup();
				ObjectData[ctrl].TexData.cleanup();
			}
		}

		public CObjectDataMng()
		{
			for (int i = 0; i < ObjectData.Length; i++)
			{
				ObjectData[i] = new _ObjectData();
			}
		}

		public bool isLoaded(int ctrl)
		{
			if (ObjectData[ctrl].NHMdl.isLoaded() && ObjectData[ctrl].NHAnm.isLoaded())
			{
				return ObjectData[ctrl].NHTex.isLoaded();
			}
			return false;
		}

		public uint getTotalSize()
		{
			return m_totalFileSize;
		}
	}
}
