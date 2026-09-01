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
		public const MAIN_BG_SCR MBS_DISPLAY = MAIN_BG_SCR.MBS_DISPLAY;

		public const MAIN_BG_SCR NUMBER_OF_MBS = MAIN_BG_SCR.NUMBER_OF_MBS;

		public const SUB_BG_SCR SBS_MAINMENU = SUB_BG_SCR.SBS_MAINMENU;

		public const SUB_BG_SCR SBS_LETTER_EDIT = SUB_BG_SCR.SBS_LETTER_EDIT;

		public const SUB_BG_SCR SBS_LETTER_BROWSE = SUB_BG_SCR.SBS_LETTER_BROWSE;

		public const SUB_BG_SCR SBS_INPUT_FRIENDCODE = SUB_BG_SCR.SBS_INPUT_FRIENDCODE;

		public const SUB_BG_SCR SBS_FRIENLIST = SUB_BG_SCR.SBS_FRIENLIST;

		public const SUB_BG_SCR SBS_MAILLIST = SUB_BG_SCR.SBS_MAILLIST;

		public const SUB_BG_SCR SBS_SELECT_PERSON = SUB_BG_SCR.SBS_SELECT_PERSON;

		public const SUB_BG_SCR SBS_MASTERCARD = SUB_BG_SCR.SBS_MASTERCARD;

		public const SUB_BG_SCR NUMBER_OF_SBS = SUB_BG_SCR.NUMBER_OF_SBS;

		public const MNMAIL_FLAGS MNMF_READ = MNMAIL_FLAGS.MNMF_READ;

		public const NPCMailState NPC_MAIL_ERR = NPCMailState.NPC_MAIL_ERR;

		public const NPCMailState NPC_MAIL_NOT_ARRIVED = NPCMailState.NPC_MAIL_NOT_ARRIVED;

		public const NPCMailState NPC_MAIL_NOT_READ = NPCMailState.NPC_MAIL_NOT_READ;

		public const NPCMailState NPC_MAIL_YET_READ = NPCMailState.NPC_MAIL_YET_READ;

		public const NPCMailActivity NPC_MAIL_DEACTIVE = NPCMailActivity.NPC_MAIL_DEACTIVE;

		public const NPCMailActivity NPC_MAIL_ACTIVE = NPCMailActivity.NPC_MAIL_ACTIVE;

		public const MailNPC NPC_MAIL_TP = MailNPC.NPC_MAIL_TP;

		public const MailNPC NPC_MAIL_TJ = MailNPC.NPC_MAIL_TJ;

		public const MailNPC NPC_MAIL_SA = MailNPC.NPC_MAIL_SA;

		public const MailNPC NPC_MAIL_CD = MailNPC.NPC_MAIL_CD;

		public const MailNPC NPC_MAIL_FJ = MailNPC.NPC_MAIL_FJ;

		public const MailNPC NPC_MAIL_AR = MailNPC.NPC_MAIL_AR;

		public const MailNPC NPC_MAIL_MAX = MailNPC.NPC_MAIL_MAX;

		public const int MNSB_WAIT_FADED = 0;

		public const int MNSB_DELAYED_INITIALIZE = 1;

		public const int MNSB_WAIT_CLEARED = 2;

		public const int MN_LB_FOO = 0;

		public const int MN_LB_BAA = 1;

		public const int MN_LB_END = 2;

		private static NNSG2dBGSelect mainBgSelect = NNSG2dBGSelect.NNS_G2D_BGSELECT_MAIN0;

		private static NNSG2dBGSelect subBgSelect = NNSG2dBGSelect.NNS_G2D_BGSELECT_SUB0;

		private static string[] main_bg_nscr = new string[1] { "mail_panel" };

		private static ds.Vector<SCRSTRUCT, ds.FastErasePolicy<SCRSTRUCT>> vMainNscrPtr = new ds.Vector<SCRSTRUCT, ds.FastErasePolicy<SCRSTRUCT>>(16);

		private static string[] sub_bg_nscr = new string[8] { "mog_nomal.NSCR", "mog_nomal.NSCR", "mog_nomal.NSCR", "mog_nomal.NSCR", "mog_nomal.NSCR", "mog_maillist.NSCR", "mog_okurisaki.NSCR", "mog_nomal.NSCR" };

		private static ds.Vector<SCRSTRUCT, ds.FastErasePolicy<SCRSTRUCT>> vSubNscrPtr = new ds.Vector<SCRSTRUCT, ds.FastErasePolicy<SCRSTRUCT>>(16);

		public static byte[] g_Work = new byte[0];

		private static int[] BUTTON_ICON_POS = new int[2] { 208, 144 };

		private static int[] WIFI_ICON_POS = new int[2] { 112, 32 };

		private static int[] LINKLEVEL_ICON_POS = new int[2] { 240, 16 };

		private static int NUM_NEW_MARK = 6;

		private static long RESTRICT_DURATION = 3600L;

		public static int crc32table = 0;

		private static uint MNMD_SIGNATURE = 1145917005u;

		private static int FRIENDS_LIST_LENGTH = 28;

		public static int MAILS_LIST_LENGTH = 10;

		private static int WIFI_MAIL_SEND_MIN = 0;

		private static int WIFI_MAIL_SEND_MAX = 999;

		private static int NUMBER_OF_RECEIVE_NPC_MAIL = 25;

		public static NPCMailEntry[] g_NpcMailEntry = new NPCMailEntry[25]
		{
			new NPCMailEntry(0, 40500, 40400, -1, 40360),
			new NPCMailEntry(1, 40501, 40401, -1, 40360),
			new NPCMailEntry(2, 40502, 40402, -1, 40360),
			new NPCMailEntry(3, 40503, 40403, -1, 40360),
			new NPCMailEntry(4, 40510, 40405, -1, 40361),
			new NPCMailEntry(5, 40511, 40406, -1, 40361),
			new NPCMailEntry(6, 40512, 40407, -1, 40361),
			new NPCMailEntry(7, 40513, 40408, -1, 40361),
			new NPCMailEntry(8, 40514, 40409, -1, 40361),
			new NPCMailEntry(9, 40520, 40415, -1, 40362),
			new NPCMailEntry(10, 40521, 40416, -1, 40362),
			new NPCMailEntry(11, 40522, 40417, -1, 40362),
			new NPCMailEntry(12, 40523, 40418, -1, 40362),
			new NPCMailEntry(13, 40530, 40410, -1, 40363),
			new NPCMailEntry(14, 40531, 40411, -1, 40363),
			new NPCMailEntry(15, 40532, 40412, -1, 40363),
			new NPCMailEntry(16, 40533, 40413, -1, 40363),
			new NPCMailEntry(17, 40540, 40420, -1, 40365),
			new NPCMailEntry(18, 40541, 40421, -1, 40365),
			new NPCMailEntry(19, 40542, 40422, -1, 40365),
			new NPCMailEntry(20, 40543, 40423, -1, 40365),
			new NPCMailEntry(21, 40550, 40425, -1, 40364),
			new NPCMailEntry(22, 40551, 40426, -1, 40364),
			new NPCMailEntry(23, 40552, 40427, -1, 40364),
			new NPCMailEntry(24, 40553, 40428, -1, 40364)
		};

		private static int[] g_NpcName = new int[6] { 40360, 40361, 40362, 40363, 40365, 40364 };

		public static MBMogNetLetterBrowseFactory __MBMogNetLetterBrowseFactory__ = new MBMogNetLetterBrowseFactory();

		private static int SUPORT_MARK_X = 80;

		private static int SUPORT_MARK_Y = 40;

		public static int PERSON_START_Y = 27;

		public static int PERSON_STR_HEIGHT = 16;

		public static int PERSON_MAX = 6;

		public static string[] id_name = new string[6] { "npc_one", "npc_two", "npc_three", "npc_four", "npc_five", "npc_six" };

		public static short[] display_flag = new short[6] { 36, 49, 45, 54, 254, 291 };

		public static string[] id_help = new string[4] { "help_wifi", "help_wire", "help_npc", "24hour_restriction" };

		internal static Array AllocatorForDWC(int name, uint size, int align)
		{
			Array array = null;
			int state = OS_DisableInterrupts();
			array = ds.CHeap.alloc_app(size);
			OS_RestoreInterrupts(state);
			return array;
		}

		internal static void DeallocatorForDWC(int name, Array ptr, uint size)
		{
			if (ptr != null)
			{
				int state = OS_DisableInterrupts();
				ds.CHeap.free_app(ptr);
				OS_RestoreInterrupts(state);
			}
		}

	}
}
