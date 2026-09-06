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
	public static partial class wmenu
	{
							public class CWMenuMemberBase : menu.MenuBehavior
							{
								public enum WMENU_KIND
								{
									WMENU_KIND_ITEM,
									WMENU_KIND_MAGIC,
									WMENU_KIND_EQUIP,
									WMENU_KIND_STATUS,
									WMENU_KIND_FORMATION,
									WMENU_KIND_JOB,
									WMENU_KIND_CONFIG,
									WMENU_KIND_HALF_SAVE,
									WMENU_KIND_SAVE,
									WMENU_KIND_MAIN_MENU,
									WMENU_KIND_PLANE,
									WMENU_KIND_MAGIC_LEARN,
									WMENU_KIND_CONFIG_2,
									WMENU_KIND_TIPS_LIST,
									WMENU_KIND_TIPS_TEXT,
									WMENU_KIND_MAX
								}

								public enum WMENU_PROCESS
								{
									WMENU_PROCESS_CSELECTINITIALIZE,
									WMENU_PROCESS_CSELECTRUN,
									WMENU_PROCESS_CSELECTTERMINATE,
									WMENU_PROCESS_CSELECTFADEOUT,
									WMENU_PROCESS_INITIALIZE,
									WMENU_PROCESS_RUN,
									WMENU_PROCESS_TERMINATE,
									WMENU_PROCESS_FADEOUT,
									WMENU_PROCESS_END
								}

								public const WMENU_KIND WMENU_KIND_ITEM = WMENU_KIND.WMENU_KIND_ITEM;

								public const WMENU_KIND WMENU_KIND_MAGIC = WMENU_KIND.WMENU_KIND_MAGIC;

								public const WMENU_KIND WMENU_KIND_EQUIP = WMENU_KIND.WMENU_KIND_EQUIP;

								public const WMENU_KIND WMENU_KIND_STATUS = WMENU_KIND.WMENU_KIND_STATUS;

								public const WMENU_KIND WMENU_KIND_FORMATION = WMENU_KIND.WMENU_KIND_FORMATION;

								public const WMENU_KIND WMENU_KIND_JOB = WMENU_KIND.WMENU_KIND_JOB;

								public const WMENU_KIND WMENU_KIND_CONFIG = WMENU_KIND.WMENU_KIND_CONFIG;

								public const WMENU_KIND WMENU_KIND_HALF_SAVE = WMENU_KIND.WMENU_KIND_HALF_SAVE;

								public const WMENU_KIND WMENU_KIND_SAVE = WMENU_KIND.WMENU_KIND_SAVE;

								public const WMENU_KIND WMENU_KIND_MAIN_MENU = WMENU_KIND.WMENU_KIND_MAIN_MENU;

								public const WMENU_KIND WMENU_KIND_PLANE = WMENU_KIND.WMENU_KIND_PLANE;

								public const WMENU_KIND WMENU_KIND_MAGIC_LEARN = WMENU_KIND.WMENU_KIND_MAGIC_LEARN;

								public const WMENU_KIND WMENU_KIND_CONFIG_2 = WMENU_KIND.WMENU_KIND_CONFIG_2;

								public const WMENU_KIND WMENU_KIND_TIPS_LIST = WMENU_KIND.WMENU_KIND_TIPS_LIST;

								public const WMENU_KIND WMENU_KIND_TIPS_TEXT = WMENU_KIND.WMENU_KIND_TIPS_TEXT;

								public const WMENU_KIND WMENU_KIND_MAX = WMENU_KIND.WMENU_KIND_MAX;

								public const WMENU_PROCESS WMENU_PROCESS_CSELECTINITIALIZE = WMENU_PROCESS.WMENU_PROCESS_CSELECTINITIALIZE;

								public const WMENU_PROCESS WMENU_PROCESS_CSELECTRUN = WMENU_PROCESS.WMENU_PROCESS_CSELECTRUN;

								public const WMENU_PROCESS WMENU_PROCESS_CSELECTTERMINATE = WMENU_PROCESS.WMENU_PROCESS_CSELECTTERMINATE;

								public const WMENU_PROCESS WMENU_PROCESS_CSELECTFADEOUT = WMENU_PROCESS.WMENU_PROCESS_CSELECTFADEOUT;

								public const WMENU_PROCESS WMENU_PROCESS_INITIALIZE = WMENU_PROCESS.WMENU_PROCESS_INITIALIZE;

								public const WMENU_PROCESS WMENU_PROCESS_RUN = WMENU_PROCESS.WMENU_PROCESS_RUN;

								public const WMENU_PROCESS WMENU_PROCESS_TERMINATE = WMENU_PROCESS.WMENU_PROCESS_TERMINATE;

								public const WMENU_PROCESS WMENU_PROCESS_FADEOUT = WMENU_PROCESS.WMENU_PROCESS_FADEOUT;

								public const WMENU_PROCESS WMENU_PROCESS_END = WMENU_PROCESS.WMENU_PROCESS_END;

								public static bool isEnd;

								protected bool doFinalize;

								public CWMenuMemberBase()
								{
								}

								public CWMenuMemberBase(string behavior_name)
									: base(behavior_name)
								{
								}

								public virtual bool cSelectInitialize()
								{
									return true;
								}

								public virtual void initialize()
								{
								}

								public virtual void run()
								{
								}

								public virtual void terminate()
								{
								}

								public bool isFinalize()
								{
									return doFinalize;
								}

								public void setFinalize(bool val)
								{
									doFinalize = val;
								}
							}
	}
}
