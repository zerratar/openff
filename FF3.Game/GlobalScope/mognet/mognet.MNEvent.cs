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
		public class MNEvent
		{
			public static bool checkSpecialCondition(int type)
			{
				switch (type)
				{
				case 0:
					return spl.MonsterBook.getMonsterRate() >= 25;
				case 1:
					return pl.PlayerParty.instance().mania().enemyBreakNumber()
						.get() >= 650;
				case 2:
					return pl.PlayerParty.instance().gold().get() >= 50000;
				case 3:
					return NPCMailState.NPC_MAIL_YET_READ == MNNPCMailData.getSingleton().getNPCMailState(8);
				case 4:
				{
					int num = 0;
					for (int i = 0; i < 64; i++)
					{
						short num2 = pl.PlayerParty.instance().item().importantItem(i)
							.itemId();
						if (num2 >= 5218 && num2 < 5241)
						{
							num += pl.PlayerParty.instance().item().importantItem(i)
								.itemNumber();
						}
					}
					return num >= 1;
				}
				case 5:
					return pl.PlayerParty.instance().mania().treasureHuntRate()
						.get() >= 80;
				default:
					return false;
				}
			}

			public static bool checkPartyCondition(int type)
			{
				int num = -1;
				for (byte b = 0; b < 4; b++)
				{
					if (pl.PlayerParty.instance().player(b).isEnable() && !pl.PlayerParty.instance().player(b).condition()
						.isDeath() && !pl.PlayerParty.instance().player(b).condition()
						.isStone())
					{
						num = pl.PlayerParty.instance().player(b).playerId();
						break;
					}
				}
				return type switch
				{
					0 => num == 0, 
					1 => num == 2, 
					2 => num == 3, 
					3 => true, 
					4 => true, 
					5 => num == 1, 
					_ => false, 
				};
			}

			public void mneProgress()
			{
				mneProgressTP_();
				mneProgressTJ_();
				mneProgressSA_();
				mneProgressCD_();
				mneProgressFJ_();
				mneProgressAR_();
			}

			public bool mneProgressTP_()
			{
				if (NPCMailState.NPC_MAIL_YET_READ == MNNPCMailData.getSingleton().getNPCMailState(3) && FlagManager.singleton().get(0u, 741u) == 0)
				{
					FlagManager.singleton().set(0u, 741u);
				}
				if (MNNPCMailData.getSingleton().getNPCMailActivity(0) == NPCMailActivity.NPC_MAIL_DEACTIVE)
				{
					return false;
				}
				if (!checkPartyCondition(0))
				{
					return false;
				}
				if (MNNPCMailData.getSingleton().getNPCMailState(0) == NPCMailState.NPC_MAIL_NOT_ARRIVED && FlagManager.singleton().get(0u, 7u) != 0)
				{
					MNNPCMailData.getSingleton().setNPCMailState(0, NPCMailState.NPC_MAIL_NOT_READ);
				}
				else if (MNNPCMailData.getSingleton().getNPCMailState(1) == NPCMailState.NPC_MAIL_NOT_ARRIVED && FlagManager.singleton().get(0u, 70u) != 0 && NPCMailState.NPC_MAIL_YET_READ == MNNPCMailData.getSingleton().getNPCMailState(0))
				{
					MNNPCMailData.getSingleton().setNPCMailState(1, NPCMailState.NPC_MAIL_NOT_READ);
				}
				else if (MNNPCMailData.getSingleton().getNPCMailState(2) == NPCMailState.NPC_MAIL_NOT_ARRIVED && FlagManager.singleton().get(0u, 118u) != 0 && NPCMailState.NPC_MAIL_YET_READ == MNNPCMailData.getSingleton().getNPCMailState(1))
				{
					MNNPCMailData.getSingleton().setNPCMailState(2, NPCMailState.NPC_MAIL_NOT_READ);
				}
				else
				{
					if (MNNPCMailData.getSingleton().getNPCMailState(3) != NPCMailState.NPC_MAIL_NOT_ARRIVED || FlagManager.singleton().get(0u, 135u) == 0 || !checkSpecialCondition(0) || NPCMailState.NPC_MAIL_YET_READ != MNNPCMailData.getSingleton().getNPCMailState(2))
					{
						return false;
					}
					MNNPCMailData.getSingleton().setNPCMailState(3, NPCMailState.NPC_MAIL_NOT_READ);
				}
				MNNPCMailData.getSingleton().setNPCMailActivity(0, NPCMailActivity.NPC_MAIL_DEACTIVE);
				return true;
			}

			public bool mneProgressTJ_()
			{
				if (NPCMailState.NPC_MAIL_YET_READ == MNNPCMailData.getSingleton().getNPCMailState(7) && FlagManager.singleton().get(0u, 751u) == 0)
				{
					FlagManager.singleton().set(0u, 751u);
				}
				if (MNNPCMailData.getSingleton().getNPCMailActivity(1) == NPCMailActivity.NPC_MAIL_DEACTIVE)
				{
					return false;
				}
				if (!checkPartyCondition(1))
				{
					return false;
				}
				if (MNNPCMailData.getSingleton().getNPCMailState(4) == NPCMailState.NPC_MAIL_NOT_ARRIVED)
				{
					if (FlagManager.singleton().get(0u, 54u) == 0)
					{
						goto IL_017c;
					}
					MNNPCMailData.getSingleton().setNPCMailState(4, NPCMailState.NPC_MAIL_NOT_READ);
				}
				else if (MNNPCMailData.getSingleton().getNPCMailState(5) == NPCMailState.NPC_MAIL_NOT_ARRIVED)
				{
					if (FlagManager.singleton().get(0u, 70u) == 0 || NPCMailState.NPC_MAIL_YET_READ != MNNPCMailData.getSingleton().getNPCMailState(4))
					{
						goto IL_017c;
					}
					MNNPCMailData.getSingleton().setNPCMailState(5, NPCMailState.NPC_MAIL_NOT_READ);
				}
				else if (MNNPCMailData.getSingleton().getNPCMailState(6) == NPCMailState.NPC_MAIL_NOT_ARRIVED)
				{
					if (FlagManager.singleton().get(0u, 219u) == 0 || NPCMailState.NPC_MAIL_YET_READ != MNNPCMailData.getSingleton().getNPCMailState(5))
					{
						goto IL_017c;
					}
					MNNPCMailData.getSingleton().setNPCMailState(6, NPCMailState.NPC_MAIL_NOT_READ);
				}
				else if (MNNPCMailData.getSingleton().getNPCMailState(7) == NPCMailState.NPC_MAIL_NOT_ARRIVED)
				{
					if (FlagManager.singleton().get(0u, 293u) == 0 || NPCMailState.NPC_MAIL_YET_READ != MNNPCMailData.getSingleton().getNPCMailState(6))
					{
						goto IL_017c;
					}
					MNNPCMailData.getSingleton().setNPCMailState(7, NPCMailState.NPC_MAIL_NOT_READ);
				}
				else
				{
					if (MNNPCMailData.getSingleton().getNPCMailState(8) != NPCMailState.NPC_MAIL_NOT_ARRIVED || FlagManager.singleton().get(0u, 314u) == 0 || !checkSpecialCondition(1) || NPCMailState.NPC_MAIL_YET_READ != MNNPCMailData.getSingleton().getNPCMailState(7))
					{
						goto IL_017c;
					}
					MNNPCMailData.getSingleton().setNPCMailState(8, NPCMailState.NPC_MAIL_NOT_READ);
				}
				MNNPCMailData.getSingleton().setNPCMailActivity(1, NPCMailActivity.NPC_MAIL_DEACTIVE);
				return true;
				IL_017c:
				return false;
			}

			public bool mneProgressSA_()
			{
				if (NPCMailState.NPC_MAIL_YET_READ == MNNPCMailData.getSingleton().getNPCMailState(12) && FlagManager.singleton().get(0u, 761u) == 0)
				{
					FlagManager.singleton().set(0u, 761u);
				}
				if (MNNPCMailData.getSingleton().getNPCMailActivity(2) == NPCMailActivity.NPC_MAIL_DEACTIVE)
				{
					return false;
				}
				if (!checkPartyCondition(2))
				{
					return false;
				}
				if (MNNPCMailData.getSingleton().getNPCMailState(9) == NPCMailState.NPC_MAIL_NOT_ARRIVED && FlagManager.singleton().get(0u, 94u) != 0)
				{
					MNNPCMailData.getSingleton().setNPCMailState(9, NPCMailState.NPC_MAIL_NOT_READ);
				}
				else if (MNNPCMailData.getSingleton().getNPCMailState(10) == NPCMailState.NPC_MAIL_NOT_ARRIVED && FlagManager.singleton().get(0u, 135u) != 0 && NPCMailState.NPC_MAIL_YET_READ == MNNPCMailData.getSingleton().getNPCMailState(9))
				{
					MNNPCMailData.getSingleton().setNPCMailState(10, NPCMailState.NPC_MAIL_NOT_READ);
				}
				else if (MNNPCMailData.getSingleton().getNPCMailState(11) == NPCMailState.NPC_MAIL_NOT_ARRIVED && FlagManager.singleton().get(0u, 185u) != 0 && NPCMailState.NPC_MAIL_YET_READ == MNNPCMailData.getSingleton().getNPCMailState(10))
				{
					MNNPCMailData.getSingleton().setNPCMailState(11, NPCMailState.NPC_MAIL_NOT_READ);
				}
				else
				{
					if (MNNPCMailData.getSingleton().getNPCMailState(12) != NPCMailState.NPC_MAIL_NOT_ARRIVED || FlagManager.singleton().get(0u, 262u) == 0 || !checkSpecialCondition(2) || NPCMailState.NPC_MAIL_YET_READ != MNNPCMailData.getSingleton().getNPCMailState(11))
					{
						return false;
					}
					MNNPCMailData.getSingleton().setNPCMailState(12, NPCMailState.NPC_MAIL_NOT_READ);
				}
				MNNPCMailData.getSingleton().setNPCMailActivity(2, NPCMailActivity.NPC_MAIL_DEACTIVE);
				return true;
			}

			public bool mneProgressCD_()
			{
				if (NPCMailState.NPC_MAIL_YET_READ == MNNPCMailData.getSingleton().getNPCMailState(16) && FlagManager.singleton().get(0u, 771u) == 0)
				{
					FlagManager.singleton().set(0u, 771u);
				}
				if (MNNPCMailData.getSingleton().getNPCMailActivity(3) == NPCMailActivity.NPC_MAIL_DEACTIVE)
				{
					return false;
				}
				if (!checkPartyCondition(3))
				{
					return false;
				}
				if (MNNPCMailData.getSingleton().getNPCMailState(13) == NPCMailState.NPC_MAIL_NOT_ARRIVED && FlagManager.singleton().get(0u, 118u) != 0)
				{
					MNNPCMailData.getSingleton().setNPCMailState(13, NPCMailState.NPC_MAIL_NOT_READ);
				}
				else if (MNNPCMailData.getSingleton().getNPCMailState(14) == NPCMailState.NPC_MAIL_NOT_ARRIVED && FlagManager.singleton().get(0u, 254u) != 0 && NPCMailState.NPC_MAIL_YET_READ == MNNPCMailData.getSingleton().getNPCMailState(13))
				{
					MNNPCMailData.getSingleton().setNPCMailState(14, NPCMailState.NPC_MAIL_NOT_READ);
				}
				else if (MNNPCMailData.getSingleton().getNPCMailState(15) == NPCMailState.NPC_MAIL_NOT_ARRIVED && NPCMailState.NPC_MAIL_YET_READ == MNNPCMailData.getSingleton().getNPCMailState(14) && 1 == FlagManager.singleton().get(0u, 309u))
				{
					MNNPCMailData.getSingleton().setNPCMailState(15, NPCMailState.NPC_MAIL_NOT_READ);
				}
				else
				{
					if (MNNPCMailData.getSingleton().getNPCMailState(16) != NPCMailState.NPC_MAIL_NOT_ARRIVED || NPCMailState.NPC_MAIL_YET_READ != MNNPCMailData.getSingleton().getNPCMailState(8) || NPCMailState.NPC_MAIL_YET_READ != MNNPCMailData.getSingleton().getNPCMailState(15) || 1 != FlagManager.singleton().get(0u, 763u) || 1 != FlagManager.singleton().get(0u, 331u))
					{
						return false;
					}
					MNNPCMailData.getSingleton().setNPCMailState(16, NPCMailState.NPC_MAIL_NOT_READ);
				}
				MNNPCMailData.getSingleton().setNPCMailActivity(3, NPCMailActivity.NPC_MAIL_DEACTIVE);
				return true;
			}

			public bool mneProgressFJ_()
			{
				if (MNNPCMailData.getSingleton().getNPCMailActivity(4) == NPCMailActivity.NPC_MAIL_DEACTIVE)
				{
					return false;
				}
				if (!checkPartyCondition(4))
				{
					return false;
				}
				if (MNNPCMailData.getSingleton().getNPCMailState(17) == NPCMailState.NPC_MAIL_NOT_ARRIVED && FlagManager.singleton().get(1u, 398u) != 0 && FlagManager.singleton().get(1u, 399u) != 0 && FlagManager.singleton().get(1u, 400u) != 0 && FlagManager.singleton().get(1u, 401u) != 0)
				{
					MNNPCMailData.getSingleton().setNPCMailState(17, NPCMailState.NPC_MAIL_NOT_READ);
				}
				else if (MNNPCMailData.getSingleton().getNPCMailState(18) == NPCMailState.NPC_MAIL_NOT_ARRIVED && NPCMailState.NPC_MAIL_YET_READ == MNNPCMailData.getSingleton().getNPCMailState(17) && FlagManager.singleton().get(1u, 421u) != 0)
				{
					MNNPCMailData.getSingleton().setNPCMailState(18, NPCMailState.NPC_MAIL_NOT_READ);
				}
				else if (MNNPCMailData.getSingleton().getNPCMailState(19) == NPCMailState.NPC_MAIL_NOT_ARRIVED)
				{
					if (NPCMailState.NPC_MAIL_YET_READ != MNNPCMailData.getSingleton().getNPCMailState(18) || FlagManager.singleton().get(1u, 470u) == 0)
					{
						goto IL_0146;
					}
					MNNPCMailData.getSingleton().setNPCMailState(19, NPCMailState.NPC_MAIL_NOT_READ);
				}
				else
				{
					if (MNNPCMailData.getSingleton().getNPCMailState(20) != NPCMailState.NPC_MAIL_NOT_ARRIVED || NPCMailState.NPC_MAIL_YET_READ != MNNPCMailData.getSingleton().getNPCMailState(19) || FlagManager.singleton().get(1u, 489u) == 0 || !checkSpecialCondition(4))
					{
						goto IL_0146;
					}
					MNNPCMailData.getSingleton().setNPCMailState(20, NPCMailState.NPC_MAIL_NOT_READ);
				}
				MNNPCMailData.getSingleton().setNPCMailActivity(4, NPCMailActivity.NPC_MAIL_DEACTIVE);
				return true;
				IL_0146:
				return false;
			}

			public bool mneProgressAR_()
			{
				if (NPCMailState.NPC_MAIL_YET_READ == MNNPCMailData.getSingleton().getNPCMailState(24) && FlagManager.singleton().get(0u, 781u) == 0)
				{
					FlagManager.singleton().set(0u, 781u);
				}
				if (MNNPCMailData.getSingleton().getNPCMailActivity(5) == NPCMailActivity.NPC_MAIL_DEACTIVE)
				{
					return false;
				}
				if (!checkPartyCondition(5))
				{
					return false;
				}
				if (MNNPCMailData.getSingleton().getNPCMailState(21) == NPCMailState.NPC_MAIL_NOT_ARRIVED)
				{
					if (FlagManager.singleton().get(0u, 321u) == 0)
					{
						goto IL_01ad;
					}
					MNNPCMailData.getSingleton().setNPCMailState(21, NPCMailState.NPC_MAIL_NOT_READ);
				}
				else if (MNNPCMailData.getSingleton().getNPCMailState(22) == NPCMailState.NPC_MAIL_NOT_ARRIVED)
				{
					if (FlagManager.singleton().get(0u, 345u) == 0 || NPCMailState.NPC_MAIL_YET_READ != MNNPCMailData.getSingleton().getNPCMailState(21))
					{
						goto IL_01ad;
					}
					MNNPCMailData.getSingleton().setNPCMailState(22, NPCMailState.NPC_MAIL_NOT_READ);
				}
				else if (MNNPCMailData.getSingleton().getNPCMailState(23) == NPCMailState.NPC_MAIL_NOT_ARRIVED)
				{
					if (NPCMailState.NPC_MAIL_YET_READ != MNNPCMailData.getSingleton().getNPCMailState(22) || FlagManager.singleton().get(0u, 353u) == 0)
					{
						goto IL_01ad;
					}
					MNNPCMailData.getSingleton().setNPCMailState(23, NPCMailState.NPC_MAIL_NOT_READ);
				}
				else
				{
					if (MNNPCMailData.getSingleton().getNPCMailState(24) != NPCMailState.NPC_MAIL_NOT_ARRIVED || FlagManager.singleton().get(0u, 365u) == 0 || FlagManager.singleton().get(0u, 366u) == 0 || FlagManager.singleton().get(0u, 367u) == 0 || FlagManager.singleton().get(0u, 368u) == 0 || FlagManager.singleton().get(0u, 369u) == 0 || !checkSpecialCondition(5) || NPCMailState.NPC_MAIL_YET_READ != MNNPCMailData.getSingleton().getNPCMailState(23) || NPCMailState.NPC_MAIL_YET_READ != MNNPCMailData.getSingleton().getNPCMailState(20))
					{
						goto IL_01ad;
					}
					MNNPCMailData.getSingleton().setNPCMailState(24, NPCMailState.NPC_MAIL_NOT_READ);
				}
				MNNPCMailData.getSingleton().setNPCMailActivity(5, NPCMailActivity.NPC_MAIL_DEACTIVE);
				return true;
				IL_01ad:
				return false;
			}
		}
	}
}
