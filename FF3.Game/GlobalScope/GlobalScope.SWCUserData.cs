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
	public class SWCUserData
	{
		public class _pseudo
		{
			public int id_data;

			public int userid_lo32;

			public int playerid;
		}

		public int gs_profile_id;

		public int size;

		public int flag;

		public int gamecode;

		public int crc32;

		private _pseudo pseudo = new _pseudo();

		private _pseudo authentic = new _pseudo();

		public void parse(ArrayReader reader)
		{
			gs_profile_id = reader.readInt32();
			size = reader.readInt32();
			flag = reader.readInt32();
			gamecode = reader.readInt32();
			crc32 = reader.readInt32();
			pseudo.id_data = reader.readInt32();
			pseudo.userid_lo32 = reader.readInt32();
			pseudo.playerid = reader.readInt32();
			authentic.id_data = reader.readInt32();
			authentic.userid_lo32 = reader.readInt32();
			authentic.playerid = reader.readInt32();
		}

		public void setDefault()
		{
			gs_profile_id = 0;
			size = 0;
			flag = 0;
			gamecode = 0;
			crc32 = 0;
			pseudo.id_data = 0;
			pseudo.userid_lo32 = 0;
			pseudo.playerid = 0;
			authentic.id_data = 0;
			authentic.userid_lo32 = 0;
			authentic.playerid = 0;
		}

		public byte[] toByteArray()
		{
			ArrayWriter arrayWriter = new ArrayWriter();
			arrayWriter.writeInt32(gs_profile_id);
			arrayWriter.writeInt32(size);
			arrayWriter.writeInt32(flag);
			arrayWriter.writeInt32(gamecode);
			arrayWriter.writeInt32(crc32);
			arrayWriter.writeInt32(pseudo.id_data);
			arrayWriter.writeInt32(pseudo.userid_lo32);
			arrayWriter.writeInt32(pseudo.playerid);
			arrayWriter.writeInt32(authentic.id_data);
			arrayWriter.writeInt32(authentic.userid_lo32);
			arrayWriter.writeInt32(authentic.playerid);
			return arrayWriter.getBytes();
		}
	}
}
