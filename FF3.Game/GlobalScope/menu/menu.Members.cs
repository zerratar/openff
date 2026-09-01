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
	public static partial class menu
	{
		public const BW_FRAME_KIND BWFK_UL0 = BW_FRAME_KIND.BWFK_UL0;

		public const BW_FRAME_KIND BWFK_UCL0 = BW_FRAME_KIND.BWFK_UCL0;

		public const BW_FRAME_KIND BWFK_UCL1 = BW_FRAME_KIND.BWFK_UCL1;

		public const BW_FRAME_KIND BWFK_UCL2 = BW_FRAME_KIND.BWFK_UCL2;

		public const BW_FRAME_KIND BWFK_UCL3 = BW_FRAME_KIND.BWFK_UCL3;

		public const BW_FRAME_KIND BWFK_UCR0 = BW_FRAME_KIND.BWFK_UCR0;

		public const BW_FRAME_KIND BWFK_UR0 = BW_FRAME_KIND.BWFK_UR0;

		public const BW_FRAME_KIND BWFK_CLU0 = BW_FRAME_KIND.BWFK_CLU0;

		public const BW_FRAME_KIND BWFK_CLU1 = BW_FRAME_KIND.BWFK_CLU1;

		public const BW_FRAME_KIND BWFK_CLD0 = BW_FRAME_KIND.BWFK_CLD0;

		public const BW_FRAME_KIND BWFK_CR0 = BW_FRAME_KIND.BWFK_CR0;

		public const BW_FRAME_KIND BWFK_CR1 = BW_FRAME_KIND.BWFK_CR1;

		public const BW_FRAME_KIND BWFK_DL0 = BW_FRAME_KIND.BWFK_DL0;

		public const BW_FRAME_KIND BWFK_DC0 = BW_FRAME_KIND.BWFK_DC0;

		public const BW_FRAME_KIND BWFK_DC1 = BW_FRAME_KIND.BWFK_DC1;

		public const BW_FRAME_KIND BWFK_DC2 = BW_FRAME_KIND.BWFK_DC2;

		public const BW_FRAME_KIND BWFK_DC3 = BW_FRAME_KIND.BWFK_DC3;

		public const BW_FRAME_KIND BWFK_DR0 = BW_FRAME_KIND.BWFK_DR0;

		public const BW_FRAME_KIND BWFK_MAX = BW_FRAME_KIND.BWFK_MAX;

		public const OPTION_LINE OL_MES = OPTION_LINE.OL_MES;

		public const OPTION_LINE OL_CUR = OPTION_LINE.OL_CUR;

		public const OPTION_LINE OL_BGM = OPTION_LINE.OL_BGM;

		public const OPTION_LINE OL_SE = OPTION_LINE.OL_SE;

		public const OPTION_LINE OL_SMD = OPTION_LINE.OL_SMD;

		public const OPTION_LINE OL_MOV = OPTION_LINE.OL_MOV;

		public const OPTION_LINE OL_MENU = OPTION_LINE.OL_MENU;

		public const OPTION_LINE OL_FIX = OPTION_LINE.OL_FIX;

		public const OPTION_LINE OL_TITLE = OPTION_LINE.OL_TITLE;

		public const OPTION_LINE OL_CONFIRM = OPTION_LINE.OL_CONFIRM;

		public const int SMALLICON = 4;

		public const int LARGEFONT = 8;

		public const int ALIGN_RIGHT = 16;

		public const int ALIGN_CENTER = 32;

		public const int ITEM_BOX_TYPE_NORMAL = 0;

		public const int ITEM_BOX_TYPE_IMPORTANT = 1;

		public const int JOBNAME_VISIBILITY = 1;

		public const int JOBNAME_ACTIVITY = 2;

		public const int JOBNAME_LARGE_FONT = 4;

		public const int UNDEFINED3 = 8;

		public const int UNDEFINED4 = 16;

		public const int UNDEFINED5 = 32;

		public const int SHOP_BUY_DECIDED = 1;

		public const int SHOP_SELL_DECIDED = 1;

		public const int STATUS_MAX = 128;

		public const int STATUS_ICONS_MAX = 8;

		public const int VISIBILITY = 1;

		public const int ACTIVITY = 2;

		public const int LARGE_FONT = 4;

		public const int THROUGH_DIR = 8;

		public const int DISPLAY = 16;

		public const int PUSHED = 32;

		public const int UNDEFINED6 = 64;

		public const int UNDEFINED7 = 128;

		private static sbyte[][] BW_FRAME_ANIM_NO = new sbyte[2][]
		{
			new sbyte[18]
			{
				1, 5, 5, 5, 5, 6, 7, 2, 2, 3,
				8, 8, 4, 10, 10, 10, 10, 9
			},
			new sbyte[18]
			{
				18, 19, 19, 19, 19, 20, 13, 16, 16, 17,
				12, 12, 15, 14, 14, 14, 14, 11
			}
		};

		private static ds.Vector2<short>[][] BW_FRAME_SIZE = new ds.Vector2<short>[2][]
		{
			new ds.Vector2<short>[18]
			{
				new ds.Vector2<short>(16, 16),
				new ds.Vector2<short>(64, 16),
				new ds.Vector2<short>(64, 16),
				new ds.Vector2<short>(64, 16),
				new ds.Vector2<short>(64, 16),
				new ds.Vector2<short>(64, 16),
				new ds.Vector2<short>(16, 16),
				new ds.Vector2<short>(16, 64),
				new ds.Vector2<short>(16, 64),
				new ds.Vector2<short>(16, 16),
				new ds.Vector2<short>(16, 64),
				new ds.Vector2<short>(16, 64),
				new ds.Vector2<short>(16, 16),
				new ds.Vector2<short>(64, 16),
				new ds.Vector2<short>(64, 16),
				new ds.Vector2<short>(64, 16),
				new ds.Vector2<short>(64, 16),
				new ds.Vector2<short>(16, 16)
			},
			new ds.Vector2<short>[18]
			{
				new ds.Vector2<short>(16, 16),
				new ds.Vector2<short>(64, 16),
				new ds.Vector2<short>(64, 16),
				new ds.Vector2<short>(64, 16),
				new ds.Vector2<short>(64, 16),
				new ds.Vector2<short>(32, 16),
				new ds.Vector2<short>(16, 16),
				new ds.Vector2<short>(16, 64),
				new ds.Vector2<short>(16, 64),
				new ds.Vector2<short>(16, 16),
				new ds.Vector2<short>(16, 64),
				new ds.Vector2<short>(16, 64),
				new ds.Vector2<short>(16, 16),
				new ds.Vector2<short>(64, 16),
				new ds.Vector2<short>(64, 16),
				new ds.Vector2<short>(64, 16),
				new ds.Vector2<short>(64, 16),
				new ds.Vector2<short>(16, 16)
			}
		};

		public static MBBattleEquipFactory __MBBattleEquipFactory__ = new MBBattleEquipFactory();

		public static MBCommandFactory __MBCommandFactory__ = new MBCommandFactory();

		public static MBConfigFactory __MBConfigFactory__ = new MBConfigFactory();

		public static MBConfigSoundFactory __MBConfigSoundFactory__ = new MBConfigSoundFactory();

		public static MBConfigExplanationFactory __MBConfigExplanationFactory__ = new MBConfigExplanationFactory();

		public static int SPR_POS_PLUS = 0;

		public static int SPR_POS_PLUS_Y = 0;

		public static MBIconFactory __MBIconFactory__ = new MBIconFactory();

		public static MBItemNameFactory __MBItemNameFactory__ = new MBItemNameFactory();

		public static MBItemWindowFactory __MBItemWindowFactory__ = new MBItemWindowFactory();

		public static MBJobNameFactory __MBJobNameFactory__ = new MBJobNameFactory();

		private static int FIRST_JOBNAME_ID = 50105;

		public static int JOB_NAME_MAX = 32;

		public static MBJobParamListFactory __MBJobParamListFactory__ = new MBJobParamListFactory();

		private static pl.JOB_TYPE[] job_menu_itemu_priority = new pl.JOB_TYPE[23]
		{
			pl.JOB_TYPE.SUPPINN,
			pl.JOB_TYPE.FIGHTER,
			pl.JOB_TYPE.MONK,
			pl.JOB_TYPE.WHITE_MAGICIAN,
			pl.JOB_TYPE.BLACK_MAGICIAN,
			pl.JOB_TYPE.RED_MAGICIAN,
			pl.JOB_TYPE.HUNTER,
			pl.JOB_TYPE.KNIGHT,
			pl.JOB_TYPE.THIEF,
			pl.JOB_TYPE.BOOK_MAN,
			pl.JOB_TYPE.GEOMANCER,
			pl.JOB_TYPE.DRAGON_KNIGHT,
			pl.JOB_TYPE.VIKING,
			pl.JOB_TYPE.EVIL_SOWRDER,
			pl.JOB_TYPE.PHANTOMER,
			pl.JOB_TYPE.BARD,
			pl.JOB_TYPE.KARATE_MASTER,
			pl.JOB_TYPE.IMAM,
			pl.JOB_TYPE.DEVIL_MAN,
			pl.JOB_TYPE.DEVILDOM_PHANTOMER,
			pl.JOB_TYPE.SAGE,
			pl.JOB_TYPE.NINJA,
			pl.JOB_TYPE.ONION_SWORDER
		};

		public static UserInfo.AchievementInfo[] item_list = new UserInfo.AchievementInfo[18];

		public static MBLinkListFactory __MBLinkListFactory__ = new MBLinkListFactory();

		public static ushort SCROLL_BAR_MARGIN = 4;

		public static MBMagicPramFactory __MBMagicPramFactory__ = new MBMagicPramFactory();

		public static MBMonsterListFactory __MBMonsterListFactory__ = new MBMonsterListFactory();

		private static int MONSTER_MAX = 226;

		private static int MONSTER_ID_MAX = 255;

		public static int[] TempMonsterIdBox = new int[MONSTER_MAX];

		public static MBMonsterNewMarkFactory __MBMonsterNewMarkFactory__ = new MBMonsterNewMarkFactory();

		public static MBMonsterBossMarkFactory __MBMonsterBossMarkFactory__ = new MBMonsterBossMarkFactory();

		public static MBPlayerGoldFactory __MBPlayerGoldFactory__ = new MBPlayerGoldFactory();

		public static MBQuestionFactory __MBQuestionFactory__ = new MBQuestionFactory();

		public static MBSelectItemFactory __MBSelectItemFactory__ = new MBSelectItemFactory();

		public static MBSaveLoadFactory __MBSaveLoadFactory__ = new MBSaveLoadFactory();

		private static int level = 0;

		public static MBSelectJobParamFactory __MBSelectJobParamFactory__ = new MBSelectJobParamFactory();

		public static MBShopBuyListFactory __MBShopBuyListFactory__ = new MBShopBuyListFactory();

		public static ushort enable_emission = GX_RGB(31, 31, 31);

		public static ushort disable_emission = GX_RGB(18, 17, 14);

		public static MBShopNameFactory __MBShopNameFactory__ = new MBShopNameFactory();

		public static MBShopNumberSelectFactory __MBShopNumberSelectFactory__ = new MBShopNumberSelectFactory();

		private static int[] cell_anim = new int[2] { 9, 12 };

		public static MBShopSellListFactory __MBShopSellListFactory__ = new MBShopSellListFactory();

		public static MBShopTextFactory __MBShopTextFactory__ = new MBShopTextFactory();

		public static MBSongWindowFactory __MBSongWindowFactory__ = new MBSongWindowFactory();

		private static int[] UseSongList = new int[5] { 6001, 6002, 6003, 6004, 6005 };

		public static MBStatusFactory __MBStatusFactory__ = new MBStatusFactory();

		public static MBSuspendFactory __MBSuspendFactory__ = new MBSuspendFactory();

		public static MBTextFactory __MBTextFactory__ = new MBTextFactory();

		public static MBTipsListFactory __MBTipsListFactory__ = new MBTipsListFactory();

		public static MBItemUseFactory __MBItemUseFactory__ = new MBItemUseFactory();

		public static MBWeaponNameFactory __MBWeaponNameFactory__ = new MBWeaponNameFactory();

		private static string default_id = TRANSCODE("");

		private static int LIMIT_OF_POOL = 256;

		public static dgs.msg.CMessageMng[] mm = new dgs.msg.CMessageMng[2]
		{
			dgs.msg.CMessageSys.getInstance().Main(),
			dgs.msg.CMessageSys.getInstance().Sub()
		};

		internal static void dumpMenuStructure(Medget pM)
		{
		}

	}
}
