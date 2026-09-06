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
	public class FlagManager
	{
		public static FlagManager _instance = new FlagManager();

		public static FlagManager singleton()
		{
			return _instance;
		}

		public int get(uint group, uint index)
		{
			if (group == 10)
			{
				group = 2u;
			}
			if (flags[group, index] == 0)
			{
				return 0;
			}
			return 1;
		}

		public void set(uint group, uint index)
		{
			if (group == 10)
			{
				group = 2u;
			}
			flags[group, index] = 1;
			OpenFF.Client.EngineHooks.FlagChanged(group, index, true);
			switch (group)
			{
			case 0u:
				switch (index)
				{
				case 500u:
				case 501u:
				case 502u:
				case 503u:
				case 504u:
				case 505u:
				case 506u:
				case 507u:
					if (flags[0, 500] != 0 && flags[0, 501] != 0 && flags[0, 502] != 0 && flags[0, 503] != 0 && flags[0, 504] != 0 && flags[0, 505] != 0 && flags[0, 506] != 0 && flags[0, 507] != 0)
					{
						UserInfo.AwardAchievement(9);
					}
					break;
				case 754u:
					UserInfo.AwardAchievement(14);
					break;
				case 903u:
					UserInfo.AwardAchievement(0);
					break;
				case 908u:
					UserInfo.AwardAchievement(1);
					break;
				case 913u:
					UserInfo.AwardAchievement(2);
					break;
				case 918u:
					UserInfo.AwardAchievement(3);
					break;
				}
				break;
			case 1u:
				switch (index)
				{
				case 567u:
				case 569u:
				case 571u:
				case 572u:
				case 574u:
				case 575u:
				case 576u:
				case 577u:
				case 580u:
				case 581u:
					if (flags[1, 571] != 0 && flags[1, 574] != 0 && flags[1, 575] != 0 && flags[1, 576] != 0 && flags[1, 577] != 0 && flags[1, 572] != 0 && flags[1, 567] != 0 && flags[1, 569] != 0 && flags[1, 580] != 0 && flags[1, 581] != 0)
					{
						UserInfo.AwardAchievement(5);
					}
					break;
				case 568u:
				case 570u:
				case 573u:
				case 578u:
				case 579u:
					break;
				}
				break;
			}
		}

		public void reset(uint group, uint index)
		{
			if (group == 10)
			{
				group = 2u;
			}
			flags[group, index] = 0;
			OpenFF.Client.EngineHooks.FlagChanged(group, index, false);
		}

		public void reverse(uint group, uint index)
		{
			if (group == 10)
			{
				group = 2u;
			}
			flags[group, index] = ((flags[group, index] == 0) ? ((byte)1) : ((byte)0));
		}

		public FlagManager()
		{
			for (int i = 0; (long)i < 3L; i++)
			{
				for (int j = 0; (long)j < 1000L; j++)
				{
					flags[i, j] = 0;
				}
			}
		}

		~FlagManager()
		{
		}
	}
}
