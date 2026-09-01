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
	public static partial class btl
	{
		public class BattleNpcDataManager
		{
			private BattleNpcData[] npcData_;

			private int maxSize_;

			public void load()
			{
				free();
				strcpy(out var arg, "npc_parameter.bbd");
				Array array = null;
				uint num = static_cast<uint>(ds.g_File.getSize(arg));
				array = ds.CHeap.alloc_app(num);
				ds.g_File.load(array, arg);
				npcData_ = BattleNpcData.castArray(array);
				maxSize_ = (int)(num / 40);
			}

			public void free()
			{
				if (npcData_ != null)
				{
					ds.CHeap.free_app(npcData_);
					npcData_ = null;
				}
			}

			public BattleNpcData npcData(int _id)
			{
				if (npcData_ == null)
				{
					return null;
				}
				for (int i = 0; i < maxSize_; i++)
				{
					if (npcData_[i].npcId() == _id)
					{
						return npcData_[i];
					}
				}
				return null;
			}

			public BattleNpcDataManager()
			{
				npcData_ = null;
				maxSize_ = 0;
			}
		}
	}
}
