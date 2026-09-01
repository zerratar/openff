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
	public static partial class dgs
	{
		public const TXT_COLOR TXT_COLOR_NULL = TXT_COLOR.TXT_COLOR_NULL;

		public const TXT_COLOR TXT_COLOR_WHITE = TXT_COLOR.TXT_COLOR_WHITE;

		public const TXT_COLOR TXT_COLOR_BLACK = TXT_COLOR.TXT_COLOR_BLACK;

		public const TXT_COLOR TXT_COLOR_RED = TXT_COLOR.TXT_COLOR_RED;

		public const TXT_COLOR TXT_COLOR_GREEN = TXT_COLOR.TXT_COLOR_GREEN;

		public const TXT_COLOR TXT_COLOR_BLUE = TXT_COLOR.TXT_COLOR_BLUE;

		public const TXT_COLOR TXT_COLOR_CYAN = TXT_COLOR.TXT_COLOR_CYAN;

		public const TXT_COLOR TXT_COLOR_MAGENTA = TXT_COLOR.TXT_COLOR_MAGENTA;

		public const TXT_COLOR TXT_COLOR_YELLOW = TXT_COLOR.TXT_COLOR_YELLOW;

		public const TXT_COLOR TXT_COLOR_PALLID_YELLOW = TXT_COLOR.TXT_COLOR_PALLID_YELLOW;

		public const TXT_COLOR TXT_COLOR_PALLID_BLUE = TXT_COLOR.TXT_COLOR_PALLID_BLUE;

		public const TXT_COLOR TXT_COLOR_PALLID_RED = TXT_COLOR.TXT_COLOR_PALLID_RED;

		public const TXT_COLOR TXT_UCOLOR_3 = TXT_COLOR.TXT_UCOLOR_3;

		public const TXT_COLOR TXT_UCOLOR_4 = TXT_COLOR.TXT_UCOLOR_4;

		public const TXT_COLOR TXT_UCOLOR_5 = TXT_COLOR.TXT_UCOLOR_5;

		public const TXT_COLOR TXT_UCOLOR_6 = TXT_COLOR.TXT_UCOLOR_6;

		public const TXT_COLOR NUMBER_OF_TXT_COLOR = TXT_COLOR.NUMBER_OF_TXT_COLOR;

		public const TXT_COLOR TXT_COLOR_DISABLE = TXT_COLOR.TXT_UCOLOR_4;

		public const TARGETLCD TLCD_MAIN = TARGETLCD.TLCD_MAIN;

		public const TARGETLCD TLCD_SUB = TARGETLCD.TLCD_SUB;

		public static C_C_SET[] ccs = new C_C_SET[22]
		{
			new C_C_SET("lead_player_name", ccpLeadPlayerName),
			new C_C_SET("shuyaku", ccpPlayerName),
			new C_C_SET("player_level", ccpPlayerLevel),
			new C_C_SET("player_expn", ccpPlayerExpNext),
			new C_C_SET("player_exp", ccpPlayerExp),
			new C_C_SET("player_skill", ccpPlayerSkill),
			new C_C_SET("current_hp", ccpCurrentHP),
			new C_C_SET("maximum_hp", ccpMaximumHP),
			new C_C_SET("party_gold", ccpPartyGold),
			new C_C_SET("play_time", ccpPlayTime),
			new C_C_SET("unfixed_player_name", ccpUnFixedPlayerName),
			new C_C_SET("unfixed_item", ccpUnFixedItem),
			new C_C_SET("unfixed_gold", ccpUnFixedGold),
			new C_C_SET("unfixed_exp", ccpUnFixedExp),
			new C_C_SET("unfixed_max_hp", ccpUnFixedMaxHp),
			new C_C_SET("unfixed_hp", ccpUnFixedHp),
			new C_C_SET("inn_price", ccpUnFixedInnPrice),
			new C_C_SET("job_penalty", ccpUnFixedJobPenalty),
			new C_C_SET("job_name", ccpUnFixedJobName),
			new C_C_SET("slot", ccpUnFixedSlot),
			new C_C_SET("friend", ccpUnFixedFriendName),
			new C_C_SET("1", ccpArgument1)
		};

		private static int NUMBER_OF_CCS = ccs.Length;

		private static ushort[] TXTColorPalette = new ushort[16]
		{
			GX_RGB(0, 0, 0),
			GX_RGB(31, 31, 31),
			GX_RGB(0, 0, 0),
			GX_RGB(31, 0, 0),
			GX_RGB(0, 31, 0),
			GX_RGB(0, 0, 31),
			GX_RGB(0, 31, 31),
			GX_RGB(31, 0, 31),
			GX_RGB(31, 31, 0),
			GX_RGB(31, 30, 19),
			GX_RGB(12, 23, 29),
			GX_RGB(27, 18, 19),
			GX_RGB(16, 16, 16),
			GX_RGB(20, 20, 20),
			GX_RGB(24, 24, 24),
			GX_RGB(28, 28, 28)
		};

		private static int LIMIT_OF_DELAYED_ERASE = 256;

		public static ds.Stack<DelayedEraseArea> g_DelayedEraseOrder = new ds.Stack<DelayedEraseArea>(LIMIT_OF_DELAYED_ERASE);

		private static int EXPANDED_MARGIN = 64;

		public static int INVALID_MSDHANDLE = -1;

		private static int INVALID_FONTHANDLE = -1;

		private static int INVALID_CANVASINDEX = -1;

		public static DGSBG0 dgsBG0 = new DGSBG0();

		public static DGSBG1 dgsBG1 = new DGSBG1();

		public static DGSBG2 dgsBG2 = new DGSBG2();

		public static DGSBG3 dgsBG3 = new DGSBG3();

		public static DGSPlane[] dgsPlanes = new DGSPlane[4] { dgsBG0, dgsBG1, dgsBG2, dgsBG3 };

		private static int g_UNCurrentNumber = -1;

		internal static void ccpLeadPlayerName(string code, char[] dest, int iDestOffset)
		{
			strcpy(dest, iDestOffset, pl.PlayerParty.instance().playerForId((byte)wld.CWorldOutSideData.getInstance().PlayerData().getFrontPlayerID()).name());
		}

		internal static void ccpPlayerName(string code, char[] dest, int iDestOffset)
		{
			int num = atoi(code.Substring(7, 1));
			num--;
			if (0 <= num && num <= 3)
			{
				strcpy(dest, iDestOffset, pl.PlayerParty.instance().playerForId((byte)num).name());
			}
		}

		internal static void ccpPlayerLevel(string code, char[] dest, int iDestOffset)
		{
			string after = "";
			int num = atoi(code.Substring(12, 1)) - 1;
			changeValueFont(pl.PlayerParty.instance().playerForId((byte)num).level()
				.get(), ref after);
			strcpy(dest, iDestOffset, after);
		}

		internal static void ccpPlayerExp(string code, char[] dest, int iDestOffset)
		{
			string after = "";
			int num = atoi(code.Substring(10, 1)) - 1;
			changeValueFont(pl.PlayerParty.instance().player((byte)num).exp()
				.get(), ref after);
			strcpy(dest, iDestOffset, after);
		}

		internal static void ccpPlayerExpNext(string code, char[] dest, int iDestOffset)
		{
			string after = "";
			int num = atoi(code.Substring(11, 1)) - 1;
			int value = pl.PlayerParty.instance().playerExp()[0].exp((byte)pl.PlayerParty.instance().player((byte)num).level()
				.get()) - pl.PlayerParty.instance().player((byte)num).exp()
				.get();
			changeValueFont(value, ref after);
			strcpy(dest, iDestOffset, after);
		}

		internal static void ccpPlayerSkill(string code, char[] dest, int iDestOffset)
		{
			string after = "";
			int num = atoi(code.Substring(12, 1)) - 1;
			pl.PlayerJobManager playerJobManager = pl.PlayerParty.instance().player((byte)num).jobManager();
			changeValueFont(playerJobManager.job((pl.JOB_TYPE)playerJobManager.nowJob()).skill().skillLevel()
				.get(), ref after);
			strcpy(dest, iDestOffset, after);
		}

		internal static void ccpCurrentHP(string code, char[] dest, int iDestOffset)
		{
			string after = "";
			int num = atoi(code.Substring(10, 1)) - 1;
			changeValueFont(pl.PlayerParty.instance().player((byte)num).hp()
				.getNow(), ref after);
			strcpy(dest, iDestOffset, after);
		}

		internal static void ccpMaximumHP(string code, char[] dest, int iDestOffset)
		{
			string after = "";
			int num = atoi(code.Substring(10, 1)) - 1;
			changeValueFont(pl.PlayerParty.instance().player((byte)num).hp()
				.getLimit(), ref after);
			strcpy(dest, iDestOffset, after);
		}

		internal static void ccpPartyGold(string code, char[] dest, int iDestOffset)
		{
			string after = "";
			changeValueFont(pl.PlayerParty.instance().gold().get(), ref after);
			strcpy(dest, iDestOffset, after);
		}

		internal static void ccpPlayTime(string code, char[] dest, int iDestOffset)
		{
			string arg = "";
			int[] array = new int[5]
			{
				(int)(pl.PlayerParty.instance().playTime() / 10000),
				(int)(pl.PlayerParty.instance().playTime() % 10000 / 1000),
				(int)(pl.PlayerParty.instance().playTime() % 10000 % 1000 / 100),
				(int)(pl.PlayerParty.instance().playTime() % 10000 % 1000 % 100 / 10),
				(int)(pl.PlayerParty.instance().playTime() % 10000 % 1000 % 100 % 10)
			};
			sprintf(out arg, "%d %d %d : %d %d ", array[0], array[1], array[2], array[3], array[4]);
			strcpy(dest, iDestOffset, arg);
		}

		internal static void ccpUnFixedPlayerName(string code, char[] dest, int iDestOffset)
		{
			int playerId = CCtrlCodeInterface.instance().getPlayerId();
			if (0 <= playerId && playerId <= 3)
			{
				strcpy(dest, iDestOffset, pl.PlayerParty.instance().playerForId((byte)playerId).name());
			}
		}

		internal static void ccpUnFixedItem(string code, char[] dest, int iDestOffset)
		{
			int itemId = CCtrlCodeInterface.instance().getItemId();
			int num = msg.CMessageSys.getInstance().Main().createMessage((uint)itemId, 0, 0, msg.CMessageMng.MSD_HANDLE_KIND.MSD_HANDLE_KIND_GAME_PART, msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_12x12);
			if (num > 0)
			{
				msg.CMessageSys.getInstance().Main().setVisibility(num, _Visibility: true);
				msg.CMessageSys.getInstance().Main().Message(num)
					.setDisplaySpeed(byte.MaxValue);
				msg.CMessageSys.getInstance().Main().Message(num)
					.setDisplayWait(0);
				strcpy(dest, iDestOffset, msg.CMessageSys.getInstance().Main().Message(num)
					.getString());
				msg.CMessageSys.getInstance().Main().releaseMessage(num);
			}
		}

		internal static void ccpUnFixedGold(string code, char[] dest, int iDestOffset)
		{
			string after = "";
			changeValueFont(CCtrlCodeInterface.instance().getGold(), ref after);
			strcpy(dest, iDestOffset, after);
		}

		internal static void ccpUnFixedExp(string code, char[] dest, int iDestOffset)
		{
			string after = "";
			changeValueFont(CCtrlCodeInterface.instance().getExp(), ref after);
			strcpy(dest, iDestOffset, after);
		}

		internal static void ccpUnFixedMaxHp(string code, char[] dest, int iDestOffset)
		{
			string after = "";
			changeValueFont(CCtrlCodeInterface.instance().getMaxHp(), ref after);
			strcpy(dest, iDestOffset, after);
		}

		internal static void ccpUnFixedHp(string code, char[] dest, int iDestOffset)
		{
			string after = "";
			changeValueFont(CCtrlCodeInterface.instance().getHp(), ref after);
			strcpy(dest, iDestOffset, after);
		}

		internal static void ccpUnFixedInnPrice(string code, char[] dest, int iDestOffset)
		{
			string after = "";
			changeValueFont(CCtrlCodeInterface.instance().getInnPrice(), ref after);
			strcpy(dest, iDestOffset, after);
		}

		internal static void ccpUnFixedJobPenalty(string code, char[] dest, int iDestOffset)
		{
			string after = "";
			changeValueFont(CCtrlCodeInterface.instance().getJobPenalty(), ref after);
			strcpy(dest, iDestOffset, after);
		}

		internal static void ccpUnFixedJobName(string code, char[] dest, int iDestOffset)
		{
			int jobMessageID = CCtrlCodeInterface.instance().getJobMessageID();
			int num = msg.CMessageSys.getInstance().Sub().createMessage((uint)jobMessageID, 0, 0, msg.CMessageMng.MSD_HANDLE_KIND.MSD_HANDLE_KIND_GAME_PART, msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_12x12);
			if (num > -1)
			{
				msg.CMessageSys.getInstance().Sub().setVisibility(num, _Visibility: true);
				msg.CMessageSys.getInstance().Sub().Message(num)
					.setDisplaySpeed(byte.MaxValue);
				msg.CMessageSys.getInstance().Sub().Message(num)
					.setDisplayWait(0);
				strcpy(dest, iDestOffset, msg.CMessageSys.getInstance().Sub().Message(num)
					.getString());
				msg.CMessageSys.getInstance().Sub().releaseMessage(num);
			}
		}

		internal static void ccpUnFixedSlot(string code, char[] dest, int iDestOffset)
		{
			int slot = CCtrlCodeInterface.instance().getSlot();
			int num = msg.CMessageSys.getInstance().Sub().createMessage((uint)(50701 + slot), 0, 0, (msg.CMessageMng.MSD_HANDLE_KIND)INVALID_MSDHANDLE, msg.CMessageMng.MSF_HANDLE_KIND.MSF_HANDLE_KIND_12x12);
			if (0 <= num)
			{
				msg.CMessageSys.getInstance().Sub().setVisibility(num, _Visibility: true);
				msg.CMessageSys.getInstance().Sub().Message(num)
					.setDisplaySpeed(byte.MaxValue);
				msg.CMessageSys.getInstance().Sub().Message(num)
					.setDisplayWait(0);
				strcpy(dest, iDestOffset, msg.CMessageSys.getInstance().Sub().Message(num)
					.getString());
				msg.CMessageSys.getInstance().Sub().releaseMessage(num);
			}
		}

		internal static void ccpUnFixedFriendName(string code, char[] dest, int iDestOffset)
		{
			string friendName = CCtrlCodeInterface.instance().getFriendName();
			if (friendName != null)
			{
				strcpy(dest, iDestOffset, CCtrlCodeInterface.instance().getFriendName());
			}
		}

		internal static void ccpArgument1(string code, char[] dest, int iDestOffset)
		{
			string argument = CCtrlCodeInterface.instance().getArgument1();
			if (argument != null)
			{
				strcpy(dest, iDestOffset, argument);
			}
		}

		internal static void changeValueFont(int value, ref string after)
		{
			sprintf(out after, "%d", value);
		}

		public static void CtrlCodeProcessing(char[] str, char[] buffer, ref int iStrOffset, ref int iBufferOffset)
		{
			int num = 0;
			char[] array = new char[64];
			iStrOffset++;
			num = 0;
			while (str[iStrOffset] != '%')
			{
				array[num] = str[iStrOffset];
				iStrOffset++;
				num++;
			}
			array[num] = '\0';
			string text = new string(array, 0, num);
			for (num = 0; num < NUMBER_OF_CCS; num++)
			{
				if (strncmp(ccs[num].code, text, strlen(ccs[num].code)) == 0)
				{
					ccs[num].processor(text, buffer, iBufferOffset);
					break;
				}
			}
			while (buffer[iBufferOffset] != 0)
			{
				iBufferOffset++;
			}
			iBufferOffset--;
		}

		internal static int DGSMessageAssignFont(Array addr)
		{
			dgsmFontVector.push_back(new NNSG2dFont());
			int num = dgsmFontVector.size() - 1;
			NNS_G2dFontInitAuto(dgsmFontVector[num], addr);
			return num;
		}

		internal static void DGSMessageInitializeFont()
		{
			dgsmFontVector.clear();
			while (g_DelayedEraseOrder.empty() == 0)
			{
				g_DelayedEraseOrder.pop();
			}
		}

		internal static void DGSClearDelayedEraseOrder()
		{
			while (g_DelayedEraseOrder.empty() == 0)
			{
				g_DelayedEraseOrder.pop();
			}
		}

		internal static void Restrict()
		{
			for (CRestrictor cRestrictor = (CRestrictor)DGSLinkedList<CRestrictor>.dgsllBase(); cRestrictor != null; cRestrictor = (CRestrictor)cRestrictor.dgsllNext())
			{
				if (cRestrictor.rorActivity())
				{
					for (CRestricted cRestricted = (CRestricted)DGSLinkedList<CRestricted>.dgsllBase(); cRestricted != null; cRestricted = (CRestricted)cRestricted.dgsllNext())
					{
						if (cRestricted.redActivity())
						{
							cRestricted.dgsredAccept(cRestrictor);
						}
					}
				}
			}
		}

	}
}
