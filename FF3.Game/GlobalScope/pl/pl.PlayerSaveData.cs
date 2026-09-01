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
	public static partial class pl
	{
		public class PlayerSaveData
		{
			public Player[] player_ = new Player[4];

			public npc.NpcManager npc_ = new npc.NpcManager();

			public itm.PossessionItemManager item_ = new itm.PossessionItemManager();

			public itm.StoredItemManager storedItem_ = new itm.StoredItemManager();

			public ys.ParameterPoint<int> gold_ = new ys.ParameterPoint<int>(0, 9999999);

			public uint playTime_;

			public Mania mania_ = new Mania();

			public PlayerSaveData()
			{
				for (int i = 0; i < player_.Length; i++)
				{
					player_[i] = new Player();
				}
			}

			public void setDefault()
			{
				for (int i = 0; i < player_.Length; i++)
				{
					player_[i].setDefault();
				}
				npc_.setDefault();
				item_.setDefault();
				storedItem_.setDefault();
				gold_.set(0);
				playTime_ = 0u;
				mania_.setDefault();
			}

			public void parse(ArrayReader reader)
			{
				for (int i = 0; i < player_.Length; i++)
				{
					player_[i].parse(reader);
				}
				npc_.parse(reader);
				item_.parse(reader);
				storedItem_.parse(reader);
				gold_.set(reader.readInt32());
				playTime_ = reader.readUInt32();
				mania_.parse(reader);
			}

			public void store(ArrayWriter writer)
			{
				for (int i = 0; i < player_.Length; i++)
				{
					player_[i].store(writer);
				}
				npc_.store(writer);
				item_.store(writer);
				storedItem_.store(writer);
				writer.writeInt32(gold_.get());
				writer.writeUInt32(playTime_);
				mania_.store(writer);
			}
		}
	}
}
