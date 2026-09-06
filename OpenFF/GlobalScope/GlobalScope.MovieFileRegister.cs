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
	public class MovieFileRegister
	{
		public const int MFR_TABLE_SIZE = 16;

		public static MovieFileRegister instance_ = new MovieFileRegister();

		public static int MFR_FNAME_LEN = 32;

		private string[] FName_ = new string[16];

		public void beginning()
		{
			for (int i = 0; i < 16; i++)
			{
				FName_[i] = "\0";
			}
		}

		public void ending()
		{
			for (int i = 0; i < 16; i++)
			{
				FName_[i] = "\0";
			}
		}

		public void regist(int nItem, string strFName)
		{
			if (0 <= nItem && 16 >= nItem && FName_[nItem][0] == '\0' && strlen(strFName) <= MFR_FNAME_LEN - 1)
			{
				strcpy(out FName_[nItem], strFName);
			}
		}

		public void erase(int nItem)
		{
			if (0 <= nItem && 16 >= nItem)
			{
				FName_[nItem] = "\0";
			}
		}

		public string acquire(int nItem)
		{
			if (0 > nItem || 16 < nItem)
			{
				return null;
			}
			if (FName_[nItem][0] == '\0')
			{
				return null;
			}
			return FName_[nItem];
		}

		public static MovieFileRegister getSingleton()
		{
			return instance_;
		}
	}
}
