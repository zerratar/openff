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
	public class CTextureDataMng
	{
		public class texture_data
		{
			public uint flag;

			public string name;

			public uint useCount;

			public CFileData texData = new CFileData();

			public NHTextureData NHTex = new NHTextureData();

			public ds.sys3d.CModelTexture tex = new ds.sys3d.CModelTexture();
		}

		protected static int TEXTURE_MAX = 32;

		protected texture_data[] TextureData = new texture_data[TEXTURE_MAX];

		protected uint m_totalFileSize;

		public void init()
		{
			for (sbyte b = 0; b < TEXTURE_MAX; b++)
			{
				initValue(b);
			}
		}

		public void end()
		{
			init();
		}

		public int setData(string texname, bool async)
		{
			int num = -1;
			num = searchNullIndex();
			if (-1 == num)
			{
				return -1;
			}
			ds.fs.enFDL_FILETYPE type = ds.fs.enFDL_FILETYPE.enFDL_FILETYPE_COMPRESS;
			strcpy(out TextureData[num].name, texname);
			sprintf(out var arg, "%s.ntxp.lz", texname);
			if (ds.g_File.getSize(arg) == 0)
			{
				return -1;
			}
			if (async)
			{
				TextureData[num].texData.setupAsync(arg, type, TextureData[num].NHTex);
				if (0 >= TextureData[num].texData.getSize())
				{
					return -1;
				}
				TextureData[num].NHTex.init(async, TextureData[num]);
			}
			else
			{
				TextureData[num].texData.setup(arg, type);
				if (0 >= TextureData[num].texData.getSize())
				{
					return -1;
				}
				TextureData[num].tex.setup(TextureData[num].texData.getAddr(), tdl: true);
			}
			TextureData[num].flag = 1u;
			TextureData[num].useCount = 1u;
			m_totalFileSize += TextureData[num].texData.getSize();
			return num;
		}

		public void delData(int ctrl)
		{
			if (TextureData[ctrl].useCount != 0 && TextureData[ctrl].flag != 0)
			{
				TextureData[ctrl].useCount--;
				if (0 >= TextureData[ctrl].useCount)
				{
					TextureData[ctrl].tex.cleanup();
					m_totalFileSize -= TextureData[ctrl].texData.getSize();
					initValue(ctrl);
				}
			}
		}

		public ds.sys3d.CModelTexture getTex(int ctrl)
		{
			return TextureData[ctrl].tex;
		}

		public int searchNullIndex()
		{
			for (sbyte b = 0; b < TEXTURE_MAX; b++)
			{
				if (TextureData[b].flag == 0)
				{
					return b;
				}
			}
			return -1;
		}

		public int searchDataIndex(string name)
		{
			for (sbyte b = 0; b < TEXTURE_MAX; b++)
			{
				if (TextureData[b].flag != 0 && strcmp(TextureData[b].name, name) == 0)
				{
					return b;
				}
			}
			return -1;
		}

		public void initValue(int ctrl)
		{
			TextureData[ctrl].flag = 0u;
			TextureData[ctrl].name = "";
			TextureData[ctrl].useCount = 0u;
			TextureData[ctrl].tex.cleanup();
			TextureData[ctrl].texData.cleanup();
		}

		public CTextureDataMng()
		{
			for (int i = 0; i < TextureData.Length; i++)
			{
				TextureData[i] = new texture_data();
			}
		}

		public bool isLoaded(int ctrl)
		{
			return TextureData[ctrl].NHTex.isLoaded();
		}

		public uint getTotalSize()
		{
			return m_totalFileSize;
		}
	}
}
