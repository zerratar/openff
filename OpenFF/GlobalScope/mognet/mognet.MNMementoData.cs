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
	public static partial class mognet
	{
		public class MNMementoData
		{
			public SWCUserData user = new SWCUserData();

			public string name;

			public int[] friends = new int[FRIENDS_LIST_LENGTH];

			public string[] fnames = new string[FRIENDS_LIST_LENGTH];

			public ushort[] flocale = new ushort[FRIENDS_LIST_LENGTH];

			public RTCDate last_send_date = new RTCDate();

			public RTCTime last_send_time = new RTCTime();

			public uint user_crc;

			public uint friend_crc;

			public uint datetime_crc;

			public static explicit operator MNMementoData(Array src)
			{
				MNMementoData mNMementoData = new MNMementoData();
				ArrayReader arrayReader = new ArrayReader(src);
				byte[] array = new byte[16];
				mNMementoData.user.parse(arrayReader);
				arrayReader.read(array, 0, array.Length);
				mNMementoData.name = StringUtil.createString(array);
				for (int i = 0; i < FRIENDS_LIST_LENGTH; i++)
				{
					mNMementoData.friends[i] = arrayReader.readInt32();
					arrayReader.read(array, 0, array.Length);
					mNMementoData.fnames[i] = StringUtil.createString(array);
					mNMementoData.flocale[i] = arrayReader.readUInt16();
				}
				mNMementoData.last_send_date.parse(arrayReader);
				mNMementoData.last_send_time.parse(arrayReader);
				mNMementoData.user_crc = arrayReader.readUInt32();
				mNMementoData.friend_crc = arrayReader.readUInt32();
				mNMementoData.datetime_crc = arrayReader.readUInt32();
				arrayReader.dispose();
				return mNMementoData;
			}

			public void setDefault()
			{
				user.setDefault();
				name = "";
				for (int i = 0; i < FRIENDS_LIST_LENGTH; i++)
				{
					friends[i] = 0;
					fnames[i] = "";
					flocale[i] = 0;
				}
				last_send_date.setDefault();
				last_send_time.setDefault();
				user_crc = 0u;
				friend_crc = 0u;
				datetime_crc = 0u;
			}
		}
	}
}
