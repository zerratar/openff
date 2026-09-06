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
	public static partial class ds
	{
		public class CFile
		{
			public const int enFILE_ERROR_READ = -1;

			private FSFile _hFile;

			public static void initialize()
			{
				if (FS_UnloadTable() == null)
				{
					FS_GetTableSize();
				}
			}

			public void cleanup()
			{
				FS_UnloadTable();
			}

			public bool open(string szFilename)
			{
				close();
				FS_InitFile(_hFile);
				if (FS_OpenFile(_hFile, szFilename) != 0)
				{
					return true;
				}
				return false;
			}

			public void close()
			{
				if (FS_IsFile(_hFile) != 0)
				{
					FS_CloseFile(_hFile);
				}
			}

			public int read(Array dest, int size)
			{
				int result = 0;
				if (FS_IsFile(_hFile) != 0)
				{
					result = FS_ReadFile(_hFile, dest, static_cast<int>(size));
				}
				return result;
			}

			public bool load(Array dest, string filename)
			{
				if (dest == null)
				{
					return false;
				}
				if (getSize(filename) == 0)
				{
					return false;
				}
				return loadHDD(dest, filename);
			}

			public bool loadHDD(Array dest, string filename)
			{
				FSFile file = new FSFile();
				bool result = true;
				FS_InitFile(file);
				if (FS_OpenFile(file, filename) != 0)
				{
					uint num = FS_GetLength(file);
					if (FS_ReadFile(file, dest, (int)num) != num)
					{
						result = false;
					}
					FS_CloseFile(file);
				}
				return result;
			}

			public bool isOpen()
			{
				return static_cast<bool>(FS_IsFile(_hFile));
			}

			public uint getSize(string filename)
			{
				FSFile file = new FSFile();
				uint result = 0u;
				FS_InitFile(file);
				if (FS_OpenFile(file, filename) != 0)
				{
					result = FS_GetLength(file);
					FS_CloseFile(file);
				}
				else
				{
					OpenFF.Client.MissingFiles.Report(filename);
				}
				return result;
			}

			public uint getSize()
			{
				if (FS_IsFile(_hFile) != 0)
				{
					return FS_GetLength(_hFile);
				}
				return 0u;
			}

			public uint getPosition()
			{
				if (FS_IsFile(_hFile) != 0)
				{
					return FS_GetPosition(_hFile);
				}
				return 0u;
			}
		}
	}
}
