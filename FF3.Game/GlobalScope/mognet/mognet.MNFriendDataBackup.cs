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
	public static partial class mognet
	{
		public class MNFriendDataBackup
		{
			public int[] friends = new int[FRIENDS_LIST_LENGTH];

			public string[] fnames = new string[FRIENDS_LIST_LENGTH];

			public ushort[] flocale = new ushort[FRIENDS_LIST_LENGTH];

			public void copy(MNMementoData src)
			{
				for (int i = 0; i < FRIENDS_LIST_LENGTH; i++)
				{
					friends[i] = src.friends[i];
					fnames[i] = src.fnames[i];
					flocale[i] = src.flocale[i];
				}
			}

			public bool compare(MNMementoData src)
			{
				for (int i = 0; i < FRIENDS_LIST_LENGTH; i++)
				{
					if (friends[i] != src.friends[i])
					{
						return false;
					}
					if (fnames[i] != src.fnames[i])
					{
						return false;
					}
					if (flocale[i] != src.flocale[i])
					{
						return false;
					}
				}
				return true;
			}
		}
	}
}
