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
	public static partial class wld
	{
		public class LegendarySmith
		{
			public class LSFileHeader
			{
				public byte rate;

				public byte[] pad = new byte[15];

				public string[] locateName;

				public static explicit operator LSFileHeader(Array src)
				{
					LSFileHeader lSFileHeader = new LSFileHeader();
					ArrayReader arrayReader = new ArrayReader(src);
					byte[] array = new byte[LOCATE_NAME_SIZE];
					lSFileHeader.rate = arrayReader.readByte();
					arrayReader.read(lSFileHeader.pad, 0, 15);
					int num = (int)(arrayReader.rest() / LOCATE_NAME_SIZE);
					lSFileHeader.locateName = new string[num];
					for (int i = 0; i < num; i++)
					{
						arrayReader.read(array, 0, LOCATE_NAME_SIZE);
						lSFileHeader.locateName[i] = StringUtil.createString(array);
					}
					arrayReader.dispose();
					return lSFileHeader;
				}
			}

			public static string preLotteryLocate_ = "";

			private static byte LOCATE_NAME_SIZE = 16;

			public void lottery(string nowLocate, GAMEPART prePart)
			{
				evt.CEventManager.getInstance().FlagMng().reset(LS_FLAG_GROUP, LS_FLAG_INDEX);
				if (strcmp(nowLocate, preLotteryLocate_) == 0 || prePart == GAMEPART.GAMEPART_SUSPEND_LOAD)
				{
					return;
				}
				uint size = ds.g_File.getSize(LS_FILENAME);
				Array array = ds.CHeap.alloc_app(size);
				ds.g_File.load(array, LS_FILENAME);
				LSFileHeader lSFileHeader = (LSFileHeader)array;
				bool flag = false;
				for (byte b = 0; b < byte.MaxValue; b++)
				{
					string arg = lSFileHeader.locateName[b];
					if (strcmp("end", arg) == 0)
					{
						break;
					}
					if (strcmp(nowLocate, arg) == 0)
					{
						strncpy(out preLotteryLocate_, nowLocate, LOCATE_NAME_SIZE);
						ushort num = ds.RandomNumber.rand16(100);
						if (num < lSFileHeader.rate)
						{
							flag = true;
						}
						break;
					}
				}
				if (flag)
				{
					evt.CEventManager.getInstance().FlagMng().set(LS_FLAG_GROUP, LS_FLAG_INDEX);
				}
				if (array != null)
				{
					ds.CHeap.free_app(array);
				}
			}
		}
	}
}
