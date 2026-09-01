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
	public static partial class evt
	{
		private static short[] GlobalFlag_001 = new short[4] { 901, 953, 850, -1 };

		private static short[] TreasureFlag_001 = new short[1] { -1 };

		private static short[] PossessionItem_001 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_001 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 3, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(-1, -1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(-1, -1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(-1, -1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_001 = new SEventJumpParameter("祭壇の洞窟：落下", "d01_02_e01", new float[3] { -40f, 0f, 50f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_001), 10000, const_cast<SCharacterParameter[]>(Party_001), -1, const_cast<short[]>(GlobalFlag_001), const_cast<short[]>(TreasureFlag_001));

		private static short[] GlobalFlag_002 = new short[4] { 901, 953, 850, -1 };

		private static short[] TreasureFlag_002 = new short[1] { -1 };

		private static short[] PossessionItem_002 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_002 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(-1, -1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(-1, -1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(-1, -1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_002 = new SEventJumpParameter("祭壇の洞窟：落下後", "d01_05", new float[3] { -40f, 0f, 50f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_002), 10000, const_cast<SCharacterParameter[]>(Party_002), -1, const_cast<short[]>(GlobalFlag_002), const_cast<short[]>(TreasureFlag_002));

		private static short[] GlobalFlag_003 = new short[6] { 0, 1, 901, 953, 850, -1 };

		private static short[] TreasureFlag_003 = new short[1] { -1 };

		private static short[] PossessionItem_003 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_003 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 3, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(-1, -1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(-1, -1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(-1, -1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_003 = new SEventJumpParameter("祭壇の洞窟：落下→バトル後", "d01_05", new float[3] { -40f, 0f, 50f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_003), 10000, const_cast<SCharacterParameter[]>(Party_003), -1, const_cast<short[]>(GlobalFlag_003), const_cast<short[]>(TreasureFlag_003));

		private static short[] GlobalFlag_004 = new short[8] { 0, 1, 2, 3, 901, 953, 850, -1 };

		private static short[] TreasureFlag_004 = new short[1] { -1 };

		private static short[] PossessionItem_004 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_004 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 3, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(-1, -1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(-1, -1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(-1, -1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_004 = new SEventJumpParameter("祭壇の洞窟：ランドタートル前", "d01_03", new float[3] { 0f, 0f, 8f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_004), 10000, const_cast<SCharacterParameter[]>(Party_004), -1, const_cast<short[]>(GlobalFlag_004), const_cast<short[]>(TreasureFlag_004));

		private static short[] GlobalFlag_005 = new short[9] { 0, 1, 2, 3, 5, 901, 953, 850, -1 };

		private static short[] TreasureFlag_005 = new short[1] { -1 };

		private static short[] PossessionItem_005 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_005 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(-1, -1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(-1, -1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(-1, -1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_005 = new SEventJumpParameter("祭壇の洞窟：ランドタートル後", "d01_03", new float[3] { -5f, 0f, 250f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_005), 10000, const_cast<SCharacterParameter[]>(Party_005), -1, const_cast<short[]>(GlobalFlag_005), const_cast<short[]>(TreasureFlag_005));

		private static short[] GlobalFlag_006 = new short[11]
		{
			0, 1, 2, 3, 5, 6, 7, 901, 953, 850,
			-1
		};

		private static short[] TreasureFlag_006 = new short[1] { -1 };

		private static short[] PossessionItem_006 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_006 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(-1, -1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(-1, -1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(-1, -1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_006 = new SEventJumpParameter("ワールド：クリスタルイベント後", "f01_63", new float[3] { 461f, 0f, 247f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_006), 10000, const_cast<SCharacterParameter[]>(Party_006), -1, const_cast<short[]>(GlobalFlag_006), const_cast<short[]>(TreasureFlag_006));

		private static short[] GlobalFlag_007 = new short[11]
		{
			0, 1, 2, 3, 5, 6, 7, 901, 953, 850,
			-1
		};

		private static short[] TreasureFlag_007 = new short[1] { -1 };

		private static short[] PossessionItem_007 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_007 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(-1, -1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(-1, -1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(-1, -1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_007 = new SEventJumpParameter("ウル：初回", "t01_01", new float[3] { 0f, 0f, -170f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_007), 10000, const_cast<SCharacterParameter[]>(Party_007), -1, const_cast<short[]>(GlobalFlag_007), const_cast<short[]>(TreasureFlag_007));

		private static short[] GlobalFlag_008 = new short[13]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 901,
			953, 850, -1
		};

		private static short[] TreasureFlag_008 = new short[1] { -1 };

		private static short[] PossessionItem_008 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_008 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(-1, -1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(-1, -1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(-1, -1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_008 = new SEventJumpParameter("ウル：アルクゥイベント後", "t01_01", new float[3] { 0f, 0f, -170f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_008), 10000, const_cast<SCharacterParameter[]>(Party_008), -1, const_cast<short[]>(GlobalFlag_008), const_cast<short[]>(TreasureFlag_008));

		private static short[] GlobalFlag_009 = new short[13]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 901,
			953, 850, -1
		};

		private static short[] TreasureFlag_009 = new short[1] { -1 };

		private static short[] PossessionItem_009 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_009 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(-1, -1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(-1, -1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(-1, -1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_009 = new SEventJumpParameter("ウル：トパパイベント直前", "t01_03", new float[3] { 3f, 0f, 120f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_009), 10000, const_cast<SCharacterParameter[]>(Party_009), -1, const_cast<short[]>(GlobalFlag_009), const_cast<short[]>(TreasureFlag_009));

		private static short[] GlobalFlag_010 = new short[12]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 901, 953,
			850, -1
		};

		private static short[] TreasureFlag_010 = new short[1] { -1 };

		private static short[] PossessionItem_010 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_010 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(-1, -1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(-1, -1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(-1, -1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_010 = new SEventJumpParameter("カズス：アルクゥいじめイベント以前", "t02_01", new float[3] { -155f, 0f, -180f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_010), 10000, const_cast<SCharacterParameter[]>(Party_010), -1, const_cast<short[]>(GlobalFlag_010), const_cast<short[]>(TreasureFlag_010));

		private static short[] GlobalFlag_011 = new short[14]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			901, 953, 850, -1
		};

		private static short[] TreasureFlag_011 = new short[1] { -1 };

		private static short[] PossessionItem_011 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_011 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(-1, -1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(-1, -1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(-1, -1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_011 = new SEventJumpParameter("カズス：アルクゥいじめイベント後", "t02_01", new float[3] { -155f, 0f, -180f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_011), 10000, const_cast<SCharacterParameter[]>(Party_011), -1, const_cast<short[]>(GlobalFlag_011), const_cast<short[]>(TreasureFlag_011));

		private static short[] GlobalFlag_012 = new short[16]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 901, 953, 850, -1
		};

		private static short[] TreasureFlag_012 = new short[1] { -1 };

		private static short[] PossessionItem_012 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_012 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(-1, -1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(-1, -1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_012 = new SEventJumpParameter("カズス：アルクゥ仲間後", "t02_01", new float[3] { -155f, 0f, -180f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_012), 10000, const_cast<SCharacterParameter[]>(Party_012), -1, const_cast<short[]>(GlobalFlag_012), const_cast<short[]>(TreasureFlag_012));

		private static short[] GlobalFlag_013 = new short[16]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 901, 953, 850, -1
		};

		private static short[] TreasureFlag_013 = new short[1] { -1 };

		private static short[] PossessionItem_013 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_013 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(-1, -1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(-1, -1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_013 = new SEventJumpParameter("カズス：シド前", "t02_06", new float[3] { 10f, 0f, -20f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_013), 10000, const_cast<SCharacterParameter[]>(Party_013), -1, const_cast<short[]>(GlobalFlag_013), const_cast<short[]>(TreasureFlag_013));

		private static short[] GlobalFlag_014 = new short[17]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 901, 953, 850, -1
		};

		private static short[] TreasureFlag_014 = new short[1] { -1 };

		private static short[] PossessionItem_014 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_014 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(-1, -1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(-1, -1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_014 = new SEventJumpParameter("カズス：シドに飛空挺の場所を聞いた後", "t02_01", new float[3] { -155f, 0f, -180f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_014), 10000, const_cast<SCharacterParameter[]>(Party_014), -1, const_cast<short[]>(GlobalFlag_014), const_cast<short[]>(TreasureFlag_014));

		private static short[] GlobalFlag_015 = new short[18]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 901, 953, 850, -1
		};

		private static short[] TreasureFlag_015 = new short[1] { -1 };

		private static short[] PossessionItem_015 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_015 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(-1, -1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(-1, -1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_015 = new SEventJumpParameter("飛空挺内部：レフィア起きる前", "t31_01", new float[3] { 0f, 0f, -10f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_015), 10000, const_cast<SCharacterParameter[]>(Party_015), -1, const_cast<short[]>(GlobalFlag_015), const_cast<short[]>(TreasureFlag_015));

		private static short[] GlobalFlag_016 = new short[21]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 901, 953, 951, 850,
			-1
		};

		private static short[] TreasureFlag_016 = new short[1] { -1 };

		private static short[] PossessionItem_016 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_016 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(-1, -1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_016 = new SEventJumpParameter("ワールド：飛空挺入手後", "f01_64", new float[3] { 425f, 0f, 10f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_016), 10000, const_cast<SCharacterParameter[]>(Party_016), -1, const_cast<short[]>(GlobalFlag_016), const_cast<short[]>(TreasureFlag_016));

		private static short[] GlobalFlag_017 = new short[21]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 901, 953, 951, 850,
			-1
		};

		private static short[] TreasureFlag_017 = new short[1] { -1 };

		private static short[] PossessionItem_017 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_017 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(-1, -1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_017 = new SEventJumpParameter("カズス：飛空挺入手後", "t02_01", new float[3] { -155f, 0f, -180f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_017), 10000, const_cast<SCharacterParameter[]>(Party_017), -1, const_cast<short[]>(GlobalFlag_017), const_cast<short[]>(TreasureFlag_017));

		private static short[] GlobalFlag_018 = new short[22]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 20, 901, 953, 951,
			850, -1
		};

		private static short[] TreasureFlag_018 = new short[1] { -1 };

		private static short[] PossessionItem_018 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_018 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(-1, -1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(-1, -1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_018 = new SEventJumpParameter("カズス：飛空挺入手後（レフィア抜け中）", "t02_01", new float[3] { -30f, 0f, -100f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_018), 10000, const_cast<SCharacterParameter[]>(Party_018), -1, const_cast<short[]>(GlobalFlag_018), const_cast<short[]>(TreasureFlag_018));

		private static short[] GlobalFlag_019 = new short[17]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 901, 953, 850, -1
		};

		private static short[] TreasureFlag_019 = new short[1] { -1 };

		private static short[] PossessionItem_019 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_019 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(-1, -1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(-1, -1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_019 = new SEventJumpParameter("サスーン：到着レフィア仲間前", "t03_01", new float[3] { 20f, 0f, -170f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_019), 10000, const_cast<SCharacterParameter[]>(Party_019), -1, const_cast<short[]>(GlobalFlag_019), const_cast<short[]>(TreasureFlag_019));

		private static short[] GlobalFlag_020 = new short[21]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 901, 953, 951, 850,
			-1
		};

		private static short[] TreasureFlag_020 = new short[1] { -1 };

		private static short[] PossessionItem_020 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_020 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(-1, -1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_020 = new SEventJumpParameter("サスーン：到着レフィア仲間後", "t03_01", new float[3] { 20f, 0f, -170f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_020), 10000, const_cast<SCharacterParameter[]>(Party_020), -1, const_cast<short[]>(GlobalFlag_020), const_cast<short[]>(TreasureFlag_020));

		private static short[] GlobalFlag_021 = new short[23]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 901, 953,
			951, 850, -1
		};

		private static short[] TreasureFlag_021 = new short[1] { -1 };

		private static short[] PossessionItem_021 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_021 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(-1, -1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_021 = new SEventJumpParameter("サスーン：フレイグと会話後", "t03_01", new float[3] { 20f, 0f, -170f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_021), 10000, const_cast<SCharacterParameter[]>(Party_021), -1, const_cast<short[]>(GlobalFlag_021), const_cast<short[]>(TreasureFlag_021));

		private static short[] GlobalFlag_022 = new short[23]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 901, 953,
			951, 850, -1
		};

		private static short[] TreasureFlag_022 = new short[1] { -1 };

		private static short[] PossessionItem_022 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_022 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(-1, -1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_022 = new SEventJumpParameter("サスーン：王に会う", "t03_05", new float[3] { 18f, 0f, 30f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_022), 10000, const_cast<SCharacterParameter[]>(Party_022), -1, const_cast<short[]>(GlobalFlag_022), const_cast<short[]>(TreasureFlag_022));

		private static short[] GlobalFlag_023 = new short[24]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 901,
			953, 951, 850, -1
		};

		private static short[] TreasureFlag_023 = new short[1] { -1 };

		private static short[] PossessionItem_023 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_023 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_023 = new SEventJumpParameter("サスーン：フレイグ仲間後", "t03_01", new float[3] { 20f, 0f, -170f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_023), 10000, const_cast<SCharacterParameter[]>(Party_023), -1, const_cast<short[]>(GlobalFlag_023), const_cast<short[]>(TreasureFlag_023));

		private static short[] GlobalFlag_024 = new short[24]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 901,
			953, 951, 850, -1
		};

		private static short[] TreasureFlag_024 = new short[1] { -1 };

		private static short[] PossessionItem_024 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_024 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_024 = new SEventJumpParameter("サスーン：フレイグサラ姫回想", "t03_10", new float[3] { 0f, 0f, -10f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_024), 10000, const_cast<SCharacterParameter[]>(Party_024), -1, const_cast<short[]>(GlobalFlag_024), const_cast<short[]>(TreasureFlag_024));

		private static short[] GlobalFlag_025 = new short[24]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 901,
			953, 951, 850, -1
		};

		private static short[] TreasureFlag_025 = new short[1] { -1 };

		private static short[] PossessionItem_025 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_025 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_025 = new SEventJumpParameter("ワールド：フレイグ仲間後", "f01_53", new float[3] { 240f, 0f, 122f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_025), 10000, const_cast<SCharacterParameter[]>(Party_025), -1, const_cast<short[]>(GlobalFlag_025), const_cast<short[]>(TreasureFlag_025));

		private static short[] GlobalFlag_026 = new short[24]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 901,
			953, 951, 850, -1
		};

		private static short[] TreasureFlag_026 = new short[1] { -1 };

		private static short[] PossessionItem_026 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_026 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_026 = new SEventJumpParameter("封印の洞窟：フレイグ仲間後", "d03_01", new float[3] { -90f, 0f, 120f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_026), 10000, const_cast<SCharacterParameter[]>(Party_026), -1, const_cast<short[]>(GlobalFlag_026), const_cast<short[]>(TreasureFlag_026));

		private static short[] GlobalFlag_027 = new short[25]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			901, 953, 951, 850, -1
		};

		private static short[] TreasureFlag_027 = new short[1] { -1 };

		private static short[] PossessionItem_027 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_027 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 8, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ 4001, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 8, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ 4001, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 8, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ 4001, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 8, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ 4001, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_027 = new SEventJumpParameter("封印の洞窟：サラ姫前", "d03_02", new float[3] { -130f, 0f, 100f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_027), 10000, const_cast<SCharacterParameter[]>(Party_027), -1, const_cast<short[]>(GlobalFlag_027), const_cast<short[]>(TreasureFlag_027));

		private static short[] GlobalFlag_028 = new short[27]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 901, 953, 951, 850, -1
		};

		private static short[] TreasureFlag_028 = new short[1] { -1 };

		private static short[] PossessionItem_028 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_028 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 8, pl.JOB_TYPE.FIGHTER, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ 4001, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 8, pl.JOB_TYPE.RED_MAGICIAN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ 4001, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 8, pl.JOB_TYPE.WHITE_MAGICIAN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ 4001, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 8, pl.JOB_TYPE.BLACK_MAGICIAN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ 4001, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_028 = new SEventJumpParameter("封印の洞窟：ジン前", "d03_03", new float[3] { 50f, 0f, -250f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_028), 10000, const_cast<SCharacterParameter[]>(Party_028), 1, const_cast<short[]>(GlobalFlag_028), const_cast<short[]>(TreasureFlag_028));

		private static short[] GlobalFlag_029 = new short[28]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 901, 953, 951, 850, -1
		};

		private static short[] TreasureFlag_029 = new short[1] { -1 };

		private static short[] PossessionItem_029 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_029 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_029 = new SEventJumpParameter("封印の洞窟：ジン後", "d03_03", new float[3] { 50f, 0f, -250f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_029), 10000, const_cast<SCharacterParameter[]>(Party_029), 1, const_cast<short[]>(GlobalFlag_029), const_cast<short[]>(TreasureFlag_029));

		private static short[] GlobalFlag_030 = new short[29]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 901, 953, 951, 850, -1
		};

		private static short[] TreasureFlag_030 = new short[1] { -1 };

		private static short[] PossessionItem_030 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_030 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_030 = new SEventJumpParameter("祭壇の洞窟：クリスタル前に登場", "d01_03", new float[3] { -5f, 0f, 250f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_030), 10000, const_cast<SCharacterParameter[]>(Party_030), -1, const_cast<short[]>(GlobalFlag_030), const_cast<short[]>(TreasureFlag_030));

		private static short[] GlobalFlag_031 = new short[37]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 901, 903, 904, 905,
			906, 907, 910, 953, 951, 850, -1
		};

		private static short[] TreasureFlag_031 = new short[1] { -1 };

		private static short[] PossessionItem_031 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_031 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_031 = new SEventJumpParameter("祭壇の洞窟：クリスタルイベント後", "d01_03", new float[3] { -5f, 0f, 250f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_031), 10000, const_cast<SCharacterParameter[]>(Party_031), -1, const_cast<short[]>(GlobalFlag_031), const_cast<short[]>(TreasureFlag_031));

		private static short[] GlobalFlag_032 = new short[37]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 901, 903, 904, 905,
			906, 907, 910, 953, 951, 850, -1
		};

		private static short[] TreasureFlag_032 = new short[1] { -1 };

		private static short[] PossessionItem_032 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_032 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_032 = new SEventJumpParameter("ワールド：クリスタルイベント後２", "f01_63", new float[3] { 461f, 0f, 247f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_032), 10000, const_cast<SCharacterParameter[]>(Party_032), -1, const_cast<short[]>(GlobalFlag_032), const_cast<short[]>(TreasureFlag_032));

		private static short[] GlobalFlag_033 = new short[38]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 901, 903, 904,
			905, 906, 907, 910, 953, 951, 850, -1
		};

		private static short[] TreasureFlag_033 = new short[1] { -1 };

		private static short[] PossessionItem_033 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_033 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_033 = new SEventJumpParameter("サスーン：ジン撃破後帰還", "t03_01", new float[3] { 20f, 0f, -170f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_033), 10000, const_cast<SCharacterParameter[]>(Party_033), -1, const_cast<short[]>(GlobalFlag_033), const_cast<short[]>(TreasureFlag_033));

		private static short[] GlobalFlag_034 = new short[39]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 901, 903,
			904, 905, 906, 907, 910, 953, 951, 850, -1
		};

		private static short[] TreasureFlag_034 = new short[1] { -1 };

		private static short[] PossessionItem_034 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_034 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_034 = new SEventJumpParameter("サスーン：ジン撃破後泉（仲間会話見てない）", "t03_06", new float[3] { 0f, 0f, -200f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_034), 10000, const_cast<SCharacterParameter[]>(Party_034), 1, const_cast<short[]>(GlobalFlag_034), const_cast<short[]>(TreasureFlag_034));

		private static short[] GlobalFlag_035 = new short[40]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 901, 903,
			904, 905, 906, 907, 910, 953, 951, 510, 850, -1
		};

		private static short[] TreasureFlag_035 = new short[1] { -1 };

		private static short[] PossessionItem_035 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_035 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_035 = new SEventJumpParameter("サスーン：ジン撃破後泉（仲間会話見た）", "t03_06", new float[3] { 0f, 0f, -200f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_035), 10000, const_cast<SCharacterParameter[]>(Party_035), 1, const_cast<short[]>(GlobalFlag_035), const_cast<short[]>(TreasureFlag_035));

		private static short[] GlobalFlag_036 = new short[40]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 901,
			903, 904, 905, 906, 907, 910, 953, 951, 850, -1
		};

		private static short[] TreasureFlag_036 = new short[1] { -1 };

		private static short[] PossessionItem_036 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_036 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_036 = new SEventJumpParameter("サスーン：指輪投入後", "t03_06", new float[3] { 0f, 0f, -200f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_036), 10000, const_cast<SCharacterParameter[]>(Party_036), -1, const_cast<short[]>(GlobalFlag_036), const_cast<short[]>(TreasureFlag_036));

		private static short[] GlobalFlag_037 = new short[40]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 901,
			903, 904, 905, 906, 907, 910, 953, 951, 850, -1
		};

		private static short[] TreasureFlag_037 = new short[1] { -1 };

		private static short[] PossessionItem_037 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_037 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_037 = new SEventJumpParameter("サスーン：指輪投入後王に会う", "t03_05", new float[3] { 18f, 0f, 30f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_037), 10000, const_cast<SCharacterParameter[]>(Party_037), -1, const_cast<short[]>(GlobalFlag_037), const_cast<short[]>(TreasureFlag_037));

		private static short[] GlobalFlag_038 = new short[42]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			901, 903, 904, 905, 906, 907, 910, 953, 951, 950,
			850, -1
		};

		private static short[] TreasureFlag_038 = new short[1] { -1 };

		private static short[] PossessionItem_038 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_038 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_038 = new SEventJumpParameter("サスーン：カヌー入手後外観", "t03_01", new float[3] { 18f, 25f, -45f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_038), 10000, const_cast<SCharacterParameter[]>(Party_038), -1, const_cast<short[]>(GlobalFlag_038), const_cast<short[]>(TreasureFlag_038));

		private static short[] GlobalFlag_039 = new short[42]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			901, 903, 904, 905, 906, 907, 910, 953, 951, 950,
			850, -1
		};

		private static short[] TreasureFlag_039 = new short[1] { -1 };

		private static short[] PossessionItem_039 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_039 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_039 = new SEventJumpParameter("サスーン：サラ姫とフレイグ", "t03_10", new float[3] { 18f, 0f, 30f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_039), 10000, const_cast<SCharacterParameter[]>(Party_039), -1, const_cast<short[]>(GlobalFlag_039), const_cast<short[]>(TreasureFlag_039));

		private static short[] GlobalFlag_040 = new short[43]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 901, 903, 904, 905, 906, 907, 910, 953, 951,
			950, 850, -1
		};

		private static short[] TreasureFlag_040 = new short[1] { -1 };

		private static short[] PossessionItem_040 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_040 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_040 = new SEventJumpParameter("サスーン：旅立ちの朝", "t03_02", new float[3] { -65f, 0f, 35f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_040), 10000, const_cast<SCharacterParameter[]>(Party_040), -1, const_cast<short[]>(GlobalFlag_040), const_cast<short[]>(TreasureFlag_040));

		private static short[] GlobalFlag_041 = new short[44]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 901, 903, 904, 905, 906, 907, 910, 953,
			951, 950, 850, -1
		};

		private static short[] TreasureFlag_041 = new short[1] { -1 };

		private static short[] PossessionItem_041 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_041 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 5, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_041 = new SEventJumpParameter("サスーン：旅立ちの朝後", "t03_01", new float[3] { 18f, 24f, -40f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_041), 10000, const_cast<SCharacterParameter[]>(Party_041), -1, const_cast<short[]>(GlobalFlag_041), const_cast<short[]>(TreasureFlag_041));

		private static short[] GlobalFlag_042 = new short[45]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 901, 903, 904, 905, 906, 907, 910,
			953, 951, 950, 850, -1
		};

		private static short[] TreasureFlag_042 = new short[1] { -1 };

		private static short[] PossessionItem_042 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_042 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 10, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 10, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 10, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 10, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_042 = new SEventJumpParameter("ワールド：旅立ちの朝後", "f01_53", new float[3] { 240f, 0f, 122f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_042), 10000, const_cast<SCharacterParameter[]>(Party_042), -1, const_cast<short[]>(GlobalFlag_042), const_cast<short[]>(TreasureFlag_042));

		private static short[] GlobalFlag_043 = new short[45]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 901, 903, 904, 905, 906, 907, 910,
			953, 951, 950, 850, -1
		};

		private static short[] TreasureFlag_043 = new short[1] { -1 };

		private static short[] PossessionItem_043 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_043 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 10, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 10, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 10, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 10, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_043 = new SEventJumpParameter("カズス：レフィアと別れる", "t02_01", new float[3] { -155f, 0f, -180f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_043), 10000, const_cast<SCharacterParameter[]>(Party_043), -1, const_cast<short[]>(GlobalFlag_043), const_cast<short[]>(TreasureFlag_043));

		private static short[] GlobalFlag_044 = new short[46]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 901, 903, 904, 905, 906, 907,
			910, 953, 951, 950, 850, -1
		};

		private static short[] TreasureFlag_044 = new short[1] { -1 };

		private static short[] PossessionItem_044 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_044 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 10, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 10, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 10, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(-1, -1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_044 = new SEventJumpParameter("カズス：シドと再会前", "t02_06", new float[3] { 10f, 0f, -20f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_044), 10000, const_cast<SCharacterParameter[]>(Party_044), -1, const_cast<short[]>(GlobalFlag_044), const_cast<short[]>(TreasureFlag_044));

		private static short[] GlobalFlag_045 = new short[47]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 901, 903, 904, 905, 906,
			907, 910, 953, 951, 950, 850, -1
		};

		private static short[] TreasureFlag_045 = new short[1] { -1 };

		private static short[] PossessionItem_045 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_045 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 10, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 10, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 10, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(-1, -1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_045 = new SEventJumpParameter("カズス：飛空挺改造前", "t02_08", new float[3] { 10f, 0f, -10f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_045), 10000, const_cast<SCharacterParameter[]>(Party_045), 0, const_cast<short[]>(GlobalFlag_045), const_cast<short[]>(TreasureFlag_045));

		private static short[] GlobalFlag_046 = new short[48]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 901, 903, 904, 905,
			906, 907, 910, 953, 951, 950, 850, -1
		};

		private static short[] TreasureFlag_046 = new short[1] { -1 };

		private static short[] PossessionItem_046 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_046 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 10, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 10, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 10, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(-1, -1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_046 = new SEventJumpParameter("カズス：飛空挺改造後", "t02_01", new float[3] { -155f, 0f, -180f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_046), 10000, const_cast<SCharacterParameter[]>(Party_046), 0, const_cast<short[]>(GlobalFlag_046), const_cast<short[]>(TreasureFlag_046));

		private static short[] GlobalFlag_047 = new short[48]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 901, 903, 904, 905,
			906, 907, 910, 953, 951, 950, 850, -1
		};

		private static short[] TreasureFlag_047 = new short[1] { -1 };

		private static short[] PossessionItem_047 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_047 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 10, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 10, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 10, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(-1, -1, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_047 = new SEventJumpParameter("飛空挺内部：レフィア再加入", "t31_01", new float[3] { 0f, 0f, -10f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_047), 10000, const_cast<SCharacterParameter[]>(Party_047), 0, const_cast<short[]>(GlobalFlag_047), const_cast<short[]>(TreasureFlag_047));

		private static short[] GlobalFlag_048 = new short[49]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 901, 903, 904,
			905, 906, 907, 910, 953, 951, 950, 850, -1
		};

		private static short[] TreasureFlag_048 = new short[1] { -1 };

		private static short[] PossessionItem_048 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_048 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_048 = new SEventJumpParameter("ワールド：レフィア仲間後", "f01_64", new float[3] { 425f, 0f, 10f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_048), 10000, const_cast<SCharacterParameter[]>(Party_048), 0, const_cast<short[]>(GlobalFlag_048), const_cast<short[]>(TreasureFlag_048));

		private static short[] GlobalFlag_049 = new short[52]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 901, 903,
			904, 905, 906, 907, 910, 953, 951, 950, 952, 850,
			851, -1
		};

		private static short[] TreasureFlag_049 = new short[1] { -1 };

		private static short[] PossessionItem_049 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_049 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_049 = new SEventJumpParameter("ワールド：墜落後", "f01_54", new float[3] { 291f, 0f, -54f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_049), 10000, const_cast<SCharacterParameter[]>(Party_049), 0, const_cast<short[]>(GlobalFlag_049), const_cast<short[]>(TreasureFlag_049));

		private static short[] GlobalFlag_050 = new short[52]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 901, 903,
			904, 905, 906, 907, 910, 953, 951, 950, 952, 850,
			851, -1
		};

		private static short[] TreasureFlag_050 = new short[1] { -1 };

		private static short[] PossessionItem_050 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_050 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_050 = new SEventJumpParameter("カナーン：到着", "t04_01", new float[3] { -10f, 0f, -200f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_050), 10000, const_cast<SCharacterParameter[]>(Party_050), 0, const_cast<short[]>(GlobalFlag_050), const_cast<short[]>(TreasureFlag_050));

		private static short[] GlobalFlag_051 = new short[53]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 901,
			903, 904, 905, 906, 907, 910, 953, 951, 950, 952,
			850, 851, -1
		};

		private static short[] TreasureFlag_051 = new short[1] { -1 };

		private static short[] PossessionItem_051 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_051 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 10, pl.JOB_TYPE.KNIGHT, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 10, pl.JOB_TYPE.MONK, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 10, pl.JOB_TYPE.WHITE_MAGICIAN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ 4001, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 10, pl.JOB_TYPE.BLACK_MAGICIAN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ 4101, 4102, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_051 = new SEventJumpParameter("カナーン：到着後", "t04_01", new float[3] { -10f, 0f, -200f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_051), 10000, const_cast<SCharacterParameter[]>(Party_051), -1, const_cast<short[]>(GlobalFlag_051), const_cast<short[]>(TreasureFlag_051));

		private static short[] GlobalFlag_052 = new short[53]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 901,
			903, 904, 905, 906, 907, 910, 953, 951, 950, 952,
			850, 851, -1
		};

		private static short[] TreasureFlag_052 = new short[1] { -1 };

		private static short[] PossessionItem_052 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_052 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_052 = new SEventJumpParameter("カナーン：シドの家", "t04_08", new float[3] { 40f, 0f, -30f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_052), 10000, const_cast<SCharacterParameter[]>(Party_052), -1, const_cast<short[]>(GlobalFlag_052), const_cast<short[]>(TreasureFlag_052));

		private static short[] GlobalFlag_053 = new short[54]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 58,
			901, 903, 904, 905, 906, 907, 910, 953, 951, 950,
			952, 850, 851, -1
		};

		private static short[] TreasureFlag_053 = new short[1] { -1 };

		private static short[] PossessionItem_053 = new short[2] { 5011, -1 };

		private static SCharacterParameter[] Party_053 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_053 = new SEventJumpParameter("カナーン：エリクサー入手後シドの家", "t04_08", new float[3] { 40f, 0f, -30f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_053), 10000, const_cast<SCharacterParameter[]>(Party_053), -1, const_cast<short[]>(GlobalFlag_053), const_cast<short[]>(TreasureFlag_053));

		private static short[] GlobalFlag_054 = new short[55]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 58,
			60, 901, 903, 904, 905, 906, 907, 910, 953, 951,
			950, 952, 850, 851, -1
		};

		private static short[] TreasureFlag_054 = new short[1] { -1 };

		private static short[] PossessionItem_054 = new short[2] { 5011, -1 };

		private static SCharacterParameter[] Party_054 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_054 = new SEventJumpParameter("カナーン：ばあさん復活後", "t04_08", new float[3] { 40f, 0f, -30f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_054), 10000, const_cast<SCharacterParameter[]>(Party_054), -1, const_cast<short[]>(GlobalFlag_054), const_cast<short[]>(TreasureFlag_054));

		private static short[] GlobalFlag_055 = new short[55]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 58,
			60, 901, 903, 904, 905, 906, 907, 910, 953, 951,
			950, 952, 850, 851, -1
		};

		private static short[] TreasureFlag_055 = new short[1] { -1 };

		private static short[] PossessionItem_055 = new short[2] { 5011, -1 };

		private static SCharacterParameter[] Party_055 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 10, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 10, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 10, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 10, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_055 = new SEventJumpParameter("ワールド：ばあさん復活後～山道", "f01_65", new float[3] { 365f, 0f, -187f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_055), 10000, const_cast<SCharacterParameter[]>(Party_055), -1, const_cast<short[]>(GlobalFlag_055), const_cast<short[]>(TreasureFlag_055));

		private static short[] GlobalFlag_056 = new short[55]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 57,
			66, 901, 903, 904, 905, 906, 907, 910, 953, 951,
			950, 952, 850, 851, -1
		};

		private static short[] TreasureFlag_056 = new short[1] { -1 };

		private static short[] PossessionItem_056 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_056 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_056 = new SEventJumpParameter("山頂へ続く道：さらわれ前", "d04_01", new float[3] { -55f, 60f, 0f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_056), 10000, const_cast<SCharacterParameter[]>(Party_056), -1, const_cast<short[]>(GlobalFlag_056), const_cast<short[]>(TreasureFlag_056));

		private static short[] GlobalFlag_057 = new short[54]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			901, 903, 904, 905, 906, 907, 910, 953, 951, 950,
			952, 850, 851, -1
		};

		private static short[] TreasureFlag_057 = new short[1] { -1 };

		private static short[] PossessionItem_057 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_057 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_057 = new SEventJumpParameter("山頂へ続く道：山頂ボス前", "d04_02", new float[3] { 0f, 0f, -10f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_057), 10000, const_cast<SCharacterParameter[]>(Party_057), -1, const_cast<short[]>(GlobalFlag_057), const_cast<short[]>(TreasureFlag_057));

		private static short[] GlobalFlag_058 = new short[55]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 901, 903, 904, 905, 906, 907, 910, 953, 951,
			950, 952, 850, 851, -1
		};

		private static short[] TreasureFlag_058 = new short[1] { -1 };

		private static short[] PossessionItem_058 = new short[4] { -1, -1, -1, -1 };

		private static SCharacterParameter[] Party_058 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_058 = new SEventJumpParameter("山頂へ続く道：山頂ボス後", "d04_02", new float[3] { 0f, 0f, -10f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_058), 10000, const_cast<SCharacterParameter[]>(Party_058), -1, const_cast<short[]>(GlobalFlag_058), const_cast<short[]>(TreasureFlag_058));

		private static short[] GlobalFlag_059 = new short[57]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 901, 903, 904, 905, 906, 907, 910, 953,
			951, 950, 952, 850, 851, 852, -1
		};

		private static short[] TreasureFlag_059 = new short[1] { -1 };

		private static short[] PossessionItem_059 = new short[2] { 4006, -1 };

		private static SCharacterParameter[] Party_059 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_059 = new SEventJumpParameter("ワールド：バハムート脱出後～回復の森", "f01_65", new float[3] { 447f, 0f, -244f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_059), 10000, const_cast<SCharacterParameter[]>(Party_059), 2, const_cast<short[]>(GlobalFlag_059), const_cast<short[]>(TreasureFlag_059));

		private static short[] GlobalFlag_060 = new short[57]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 901, 903, 904, 905, 906, 907, 910, 953,
			951, 950, 952, 850, 851, 852, -1
		};

		private static short[] TreasureFlag_060 = new short[1] { -1 };

		private static short[] PossessionItem_060 = new short[2] { 4006, -1 };

		private static SCharacterParameter[] Party_060 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_060 = new SEventJumpParameter("回復の森：到着", "t05_01", new float[3] { 0f, 0f, -10f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_060), 10000, const_cast<SCharacterParameter[]>(Party_060), 2, const_cast<short[]>(GlobalFlag_060), const_cast<short[]>(TreasureFlag_060));

		private static short[] GlobalFlag_061 = new short[59]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 73, 74, 901, 903, 904, 905, 906, 907,
			910, 953, 951, 950, 952, 850, 851, 852, -1
		};

		private static short[] TreasureFlag_061 = new short[1] { -1 };

		private static short[] PossessionItem_061 = new short[2] { 4006, -1 };

		private static SCharacterParameter[] Party_061 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_061 = new SEventJumpParameter("ワールド：回復の森～トーザス", "f01_65", new float[3] { 447f, 0f, -244f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_061), 10000, const_cast<SCharacterParameter[]>(Party_061), 2, const_cast<short[]>(GlobalFlag_061), const_cast<short[]>(TreasureFlag_061));

		private static short[] GlobalFlag_062 = new short[57]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 901, 903, 904, 905, 906, 907, 910, 953,
			951, 950, 952, 850, 851, 852, -1
		};

		private static short[] TreasureFlag_062 = new short[1] { -1 };

		private static short[] PossessionItem_062 = new short[2] { 4006, -1 };

		private static SCharacterParameter[] Party_062 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_062 = new SEventJumpParameter("トーザス：到着", "t06_01", new float[3] { 0f, 0f, -10f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_062), 10000, const_cast<SCharacterParameter[]>(Party_062), 2, const_cast<short[]>(GlobalFlag_062), const_cast<short[]>(TreasureFlag_062));

		private static short[] GlobalFlag_063 = new short[58]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 901, 903, 904, 905, 906, 907, 910,
			953, 951, 950, 952, 850, 851, 852, -1
		};

		private static short[] TreasureFlag_063 = new short[1] { -1 };

		private static short[] PossessionItem_063 = new short[3] { 4006, 5003, -1 };

		private static SCharacterParameter[] Party_063 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_063 = new SEventJumpParameter("トーザス：シェルコの家（毒消しあり）", "t06_05", new float[3] { -13f, 0f, 50f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_063), 10000, const_cast<SCharacterParameter[]>(Party_063), 2, const_cast<short[]>(GlobalFlag_063), const_cast<short[]>(TreasureFlag_063));

		private static short[] GlobalFlag_064 = new short[60]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 901, 903, 904, 905, 906,
			907, 910, 953, 951, 950, 952, 850, 851, 852, -1
		};

		private static short[] TreasureFlag_064 = new short[1] { -1 };

		private static short[] PossessionItem_064 = new short[2] { 4006, -1 };

		private static SCharacterParameter[] Party_064 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 11, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 11, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 11, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 11, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_064 = new SEventJumpParameter("トーザス：シェルコ回復後", "t06_01", new float[3] { 0f, 0f, -10f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_064), 10000, const_cast<SCharacterParameter[]>(Party_064), 2, const_cast<short[]>(GlobalFlag_064), const_cast<short[]>(TreasureFlag_064));

		private static short[] GlobalFlag_065 = new short[60]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 901, 903, 904, 905, 906,
			907, 910, 953, 951, 950, 952, 850, 851, 852, -1
		};

		private static short[] TreasureFlag_065 = new short[1] { -1 };

		private static short[] PossessionItem_065 = new short[2] { 4006, -1 };

		private static SCharacterParameter[] Party_065 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_065 = new SEventJumpParameter("トーザスの抜け道：シェルコ回復後", "d05_01", new float[3] { 230f, 0f, 72f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_065), 10000, const_cast<SCharacterParameter[]>(Party_065), 2, const_cast<short[]>(GlobalFlag_065), const_cast<short[]>(TreasureFlag_065));

		private static short[] GlobalFlag_066 = new short[61]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 901, 903, 904, 905,
			906, 907, 910, 953, 951, 950, 952, 850, 851, 852,
			-1
		};

		private static short[] TreasureFlag_066 = new short[1] { -1 };

		private static short[] PossessionItem_066 = new short[2] { 4006, -1 };

		private static SCharacterParameter[] Party_066 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_066 = new SEventJumpParameter("ワールド：抜け道～アジト", "f01_66", new float[3] { 367f, -4f, -360f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_066), 10000, const_cast<SCharacterParameter[]>(Party_066), 2, const_cast<short[]>(GlobalFlag_066), const_cast<short[]>(TreasureFlag_066));

		private static short[] GlobalFlag_067 = new short[61]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 901, 903, 904, 905,
			906, 907, 910, 953, 951, 950, 952, 850, 851, 852,
			-1
		};

		private static short[] TreasureFlag_067 = new short[1] { -1 };

		private static short[] PossessionItem_067 = new short[2] { 4006, -1 };

		private static SCharacterParameter[] Party_067 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_067 = new SEventJumpParameter("バイキングのアジト：到着", "t07_01", new float[3] { -400f, 0f, -185f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_067), 10000, const_cast<SCharacterParameter[]>(Party_067), 2, const_cast<short[]>(GlobalFlag_067), const_cast<short[]>(TreasureFlag_067));

		private static short[] GlobalFlag_068 = new short[62]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 901, 903, 904,
			905, 906, 907, 910, 953, 951, 950, 952, 850, 851,
			852, -1
		};

		private static short[] TreasureFlag_068 = new short[1] { -1 };

		private static short[] PossessionItem_068 = new short[2] { 4006, -1 };

		private static SCharacterParameter[] Party_068 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_068 = new SEventJumpParameter("バイキングのアジト：ボス付近", "t07_02", new float[3] { 125f, 0f, 240f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_068), 10000, const_cast<SCharacterParameter[]>(Party_068), 2, const_cast<short[]>(GlobalFlag_068), const_cast<short[]>(TreasureFlag_068));

		private static short[] GlobalFlag_069 = new short[63]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 85, 901, 903,
			904, 905, 906, 907, 910, 953, 951, 950, 952, 850,
			851, 852, -1
		};

		private static short[] TreasureFlag_069 = new short[1] { -1 };

		private static short[] PossessionItem_069 = new short[2] { 4006, -1 };

		private static SCharacterParameter[] Party_069 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_069 = new SEventJumpParameter("ワールド：アジト～ネプト神殿", "f01_55", new float[3] { 279f, 0f, -241f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_069), 10000, const_cast<SCharacterParameter[]>(Party_069), 2, const_cast<short[]>(GlobalFlag_069), const_cast<short[]>(TreasureFlag_069));

		private static short[] GlobalFlag_070 = new short[64]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 85, 88, 901,
			903, 904, 905, 906, 907, 910, 953, 951, 950, 952,
			850, 851, 852, -1
		};

		private static short[] TreasureFlag_070 = new short[1] { -1 };

		private static short[] PossessionItem_070 = new short[2] { 4006, -1 };

		private static SCharacterParameter[] Party_070 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 12, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 12, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 12, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 12, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_070 = new SEventJumpParameter("ネプト神殿：到着", "d06_01", new float[3] { 6f, 0f, 68f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_070), 10000, const_cast<SCharacterParameter[]>(Party_070), 2, const_cast<short[]>(GlobalFlag_070), const_cast<short[]>(TreasureFlag_070));

		private static short[] GlobalFlag_071 = new short[65]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			901, 903, 904, 905, 906, 907, 910, 953, 951, 950,
			952, 850, 851, 852, -1
		};

		private static short[] TreasureFlag_071 = new short[1] { -1 };

		private static short[] PossessionItem_071 = new short[2] { 4006, -1 };

		private static SCharacterParameter[] Party_071 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 12, pl.JOB_TYPE.BLACK_MAGICIAN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 12, pl.JOB_TYPE.BLACK_MAGICIAN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 12, pl.JOB_TYPE.BLACK_MAGICIAN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 12, pl.JOB_TYPE.WHITE_MAGICIAN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_071 = new SEventJumpParameter("ネプト神殿：大ネズミ前", "d06_05", new float[3] { 121f, 0f, 374f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_071), 10000, const_cast<SCharacterParameter[]>(Party_071), 2, const_cast<short[]>(GlobalFlag_071), const_cast<short[]>(TreasureFlag_071));

		private static short[] GlobalFlag_072 = new short[66]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 901, 903, 904, 905, 906, 907, 910, 953, 951,
			950, 952, 850, 851, 852, -1
		};

		private static short[] TreasureFlag_072 = new short[1] { -1 };

		private static short[] PossessionItem_072 = new short[2] { 4006, -1 };

		private static SCharacterParameter[] Party_072 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_072 = new SEventJumpParameter("ネプト神殿：大ネズミ後", "d06_05", new float[3] { 170f, 0f, 374f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_072), 10000, const_cast<SCharacterParameter[]>(Party_072), 2, const_cast<short[]>(GlobalFlag_072), const_cast<short[]>(TreasureFlag_072));

		private static short[] GlobalFlag_073 = new short[67]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 901, 903, 904, 905, 906, 907, 910, 953,
			951, 950, 952, 850, 851, 852, -1
		};

		private static short[] TreasureFlag_073 = new short[1] { -1 };

		private static short[] PossessionItem_073 = new short[3] { 4006, 5201, -1 };

		private static SCharacterParameter[] Party_073 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_073 = new SEventJumpParameter("ネプト神殿：目を返す前", "d06_01", new float[3] { 6f, 0f, 68f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_073), 10000, const_cast<SCharacterParameter[]>(Party_073), 2, const_cast<short[]>(GlobalFlag_073), const_cast<short[]>(TreasureFlag_073));

		private static short[] GlobalFlag_074 = new short[68]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 901, 903, 904, 905, 906, 907, 910,
			953, 951, 950, 952, 850, 851, 852, -1
		};

		private static short[] TreasureFlag_074 = new short[1] { -1 };

		private static short[] PossessionItem_074 = new short[3] { 4006, 5207, -1 };

		private static SCharacterParameter[] Party_074 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_074 = new SEventJumpParameter("ワールド：ネプト神殿クリア後～アジト", "f01_55", new float[3] { 183f, 0f, -179f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_074), 10000, const_cast<SCharacterParameter[]>(Party_074), 2, const_cast<short[]>(GlobalFlag_074), const_cast<short[]>(TreasureFlag_074));

		private static short[] GlobalFlag_075 = new short[68]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 901, 903, 904, 905, 906, 907, 910,
			953, 951, 950, 952, 850, 851, 852, -1
		};

		private static short[] TreasureFlag_075 = new short[1] { -1 };

		private static short[] PossessionItem_075 = new short[3] { 4006, 5207, -1 };

		private static SCharacterParameter[] Party_075 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_075 = new SEventJumpParameter("バイキングのアジト：ネプトクリア後ボス付近", "t07_02", new float[3] { 125f, 0f, 240f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_075), 10000, const_cast<SCharacterParameter[]>(Party_075), 2, const_cast<short[]>(GlobalFlag_075), const_cast<short[]>(TreasureFlag_075));

		private static short[] GlobalFlag_076 = new short[71]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 901, 903, 904, 905, 906,
			907, 910, 953, 951, 950, 952, 850, 851, 852, 853,
			-1
		};

		private static short[] TreasureFlag_076 = new short[1] { -1 };

		private static short[] PossessionItem_076 = new short[3] { 4006, 5207, -1 };

		private static SCharacterParameter[] Party_076 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_076 = new SEventJumpParameter("ワールド：アジトで船入手後", "f01_55", new float[3] { 183f, 0f, -179f }, new float[3] { 257f, 0f, -279f }, 1, const_cast<short[]>(PossessionItem_076), 10000, const_cast<SCharacterParameter[]>(Party_076), 2, const_cast<short[]>(GlobalFlag_076), const_cast<short[]>(TreasureFlag_076));

		private static short[] GlobalFlag_077 = new short[71]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 901, 903, 904, 905, 906,
			907, 910, 953, 951, 950, 952, 850, 851, 852, 853,
			-1
		};

		private static short[] TreasureFlag_077 = new short[1] { -1 };

		private static short[] PossessionItem_077 = new short[3] { 4006, 5207, -1 };

		private static SCharacterParameter[] Party_077 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_077 = new SEventJumpParameter("カナーン：船入手後サリーナの家", "t04_03", new float[3] { 24f, 0f, -4f }, new float[3] { 294f, 0f, -121f }, 1, const_cast<short[]>(PossessionItem_077), 10000, const_cast<SCharacterParameter[]>(Party_077), 2, const_cast<short[]>(GlobalFlag_077), const_cast<short[]>(TreasureFlag_077));

		private static short[] GlobalFlag_078 = new short[71]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 901, 903, 904, 905, 906,
			907, 910, 953, 951, 950, 952, 850, 851, 852, 853,
			-1
		};

		private static short[] TreasureFlag_078 = new short[1] { -1 };

		private static short[] PossessionItem_078 = new short[3] { 4006, 5207, -1 };

		private static SCharacterParameter[] Party_078 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_078 = new SEventJumpParameter("トックル：２章外観", "t08_01", new float[3] { 0f, 0f, -10f }, new float[3] { -7f, 0f, -274f }, 1, const_cast<short[]>(PossessionItem_078), 10000, const_cast<SCharacterParameter[]>(Party_078), 2, const_cast<short[]>(GlobalFlag_078), const_cast<short[]>(TreasureFlag_078));

		private static short[] GlobalFlag_079 = new short[72]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 102, 901, 903, 904, 905,
			906, 907, 910, 953, 951, 950, 952, 850, 851, 852,
			853, -1
		};

		private static short[] TreasureFlag_079 = new short[1] { -1 };

		private static short[] PossessionItem_079 = new short[3] { 4006, 5207, -1 };

		private static SCharacterParameter[] Party_079 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_079 = new SEventJumpParameter("トックル：民家", "t08_02", new float[3] { 0f, 0f, -15f }, new float[3] { -7f, 0f, -274f }, 1, const_cast<short[]>(PossessionItem_079), 10000, const_cast<SCharacterParameter[]>(Party_079), 2, const_cast<short[]>(GlobalFlag_079), const_cast<short[]>(TreasureFlag_079));

		private static short[] GlobalFlag_080 = new short[73]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 102, 103, 901, 903, 904,
			905, 906, 907, 910, 953, 951, 950, 952, 850, 851,
			852, 853, -1
		};

		private static short[] TreasureFlag_080 = new short[1] { -1 };

		private static short[] PossessionItem_080 = new short[3] { 4006, 5207, -1 };

		private static SCharacterParameter[] Party_080 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_080 = new SEventJumpParameter("トックル：民家暖炉", "t08_02", new float[3] { 0f, 0f, 10f }, new float[3] { -7f, 0f, -274f }, 1, const_cast<short[]>(PossessionItem_080), 10000, const_cast<SCharacterParameter[]>(Party_080), 2, const_cast<short[]>(GlobalFlag_080), const_cast<short[]>(TreasureFlag_080));

		private static short[] GlobalFlag_081 = new short[73]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 102, 103, 901, 903, 904,
			905, 906, 907, 910, 953, 951, 950, 952, 850, 851,
			852, 853, -1
		};

		private static short[] TreasureFlag_081 = new short[1] { -1 };

		private static short[] PossessionItem_081 = new short[3] { 4006, 5207, -1 };

		private static SCharacterParameter[] Party_081 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_081 = new SEventJumpParameter("トックル：村長の家", "t08_04", new float[3] { 20f, 0f, -20f }, new float[3] { -7f, 0f, -274f }, 1, const_cast<short[]>(PossessionItem_081), 10000, const_cast<SCharacterParameter[]>(Party_081), 2, const_cast<short[]>(GlobalFlag_081), const_cast<short[]>(TreasureFlag_081));

		private static short[] GlobalFlag_082 = new short[71]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 901, 903, 904, 905, 906,
			907, 910, 953, 951, 950, 952, 850, 851, 852, 853,
			-1
		};

		private static short[] TreasureFlag_082 = new short[1] { -1 };

		private static short[] PossessionItem_082 = new short[3] { 4006, 5207, -1 };

		private static SCharacterParameter[] Party_082 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_082 = new SEventJumpParameter("古代人の村：２章", "t09_01", new float[3] { -35f, 20f, -200f }, new float[3] { -7f, 0f, -274f }, 1, const_cast<short[]>(PossessionItem_082), 10000, const_cast<SCharacterParameter[]>(Party_082), 2, const_cast<short[]>(GlobalFlag_082), const_cast<short[]>(TreasureFlag_082));

		private static short[] GlobalFlag_083 = new short[76]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 109, 110, 111, 112, 113,
			901, 903, 904, 905, 906, 907, 910, 953, 951, 950,
			952, 850, 851, 852, 853, -1
		};

		private static short[] TreasureFlag_083 = new short[1] { -1 };

		private static short[] PossessionItem_083 = new short[3] { 4006, 5207, -1 };

		private static SCharacterParameter[] Party_083 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_083 = new SEventJumpParameter("古代人の村：情報入手後", "t09_01", new float[3] { -35f, 20f, -200f }, new float[3] { -7f, 0f, -274f }, 1, const_cast<short[]>(PossessionItem_083), 10000, const_cast<SCharacterParameter[]>(Party_083), 2, const_cast<short[]>(GlobalFlag_083), const_cast<short[]>(TreasureFlag_083));

		private static short[] GlobalFlag_084 = new short[71]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 901, 903, 904, 905, 906,
			907, 910, 953, 951, 950, 952, 850, 851, 852, 853,
			-1
		};

		private static short[] TreasureFlag_084 = new short[1] { -1 };

		private static short[] PossessionItem_084 = new short[3] { 4006, 5207, -1 };

		private static SCharacterParameter[] Party_084 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_084 = new SEventJumpParameter("生きている森：２章", "t11_01", new float[3] { -9f, 0f, -75f }, new float[3] { -7f, 0f, -274f }, 1, const_cast<short[]>(PossessionItem_084), 10000, const_cast<SCharacterParameter[]>(Party_084), 2, const_cast<short[]>(GlobalFlag_084), const_cast<short[]>(TreasureFlag_084));

		private static short[] GlobalFlag_085 = new short[71]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 901, 903, 904, 905, 906,
			907, 910, 953, 951, 950, 952, 850, 851, 852, 853,
			-1
		};

		private static short[] TreasureFlag_085 = new short[1] { -1 };

		private static short[] PossessionItem_085 = new short[3] { 4006, 5207, -1 };

		private static SCharacterParameter[] Party_085 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_085 = new SEventJumpParameter("チョコボの森：２章", "t30_01", new float[3] { 0f, 0f, -10f }, new float[3] { -7f, 0f, -274f }, 1, const_cast<short[]>(PossessionItem_085), 10000, const_cast<SCharacterParameter[]>(Party_085), 2, const_cast<short[]>(GlobalFlag_085), const_cast<short[]>(TreasureFlag_085));

		private static short[] GlobalFlag_086 = new short[75]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 122, 123, 124, 125, 901,
			903, 904, 905, 906, 907, 910, 953, 951, 950, 952,
			850, 851, 852, 853, -1
		};

		private static short[] TreasureFlag_086 = new short[1] { -1 };

		private static short[] PossessionItem_086 = new short[3] { 4006, 5207, -1 };

		private static SCharacterParameter[] Party_086 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_086 = new SEventJumpParameter("生きている森：情報入手後", "t11_01", new float[3] { -9f, 0f, -75f }, new float[3] { -7f, 0f, -274f }, 1, const_cast<short[]>(PossessionItem_086), 10000, const_cast<SCharacterParameter[]>(Party_086), 2, const_cast<short[]>(GlobalFlag_086), const_cast<short[]>(TreasureFlag_086));

		private static short[] GlobalFlag_087 = new short[71]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 901, 903, 904, 905, 906,
			907, 910, 953, 951, 950, 952, 850, 851, 852, 853,
			-1
		};

		private static short[] TreasureFlag_087 = new short[1] { -1 };

		private static short[] PossessionItem_087 = new short[3] { 4006, 5207, -1 };

		private static SCharacterParameter[] Party_087 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_087 = new SEventJumpParameter("アーガス城：２章", "t12_01", new float[3] { 25f, 0f, -113f }, new float[3] { -95f, 0f, -5f }, 1, const_cast<short[]>(PossessionItem_087), 10000, const_cast<SCharacterParameter[]>(Party_087), 2, const_cast<short[]>(GlobalFlag_087), const_cast<short[]>(TreasureFlag_087));

		private static short[] GlobalFlag_088 = new short[71]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 901, 903, 904, 905, 906,
			907, 910, 953, 951, 950, 952, 850, 851, 852, 853,
			-1
		};

		private static short[] TreasureFlag_088 = new short[1] { -1 };

		private static short[] PossessionItem_088 = new short[3] { 4006, 5207, -1 };

		private static SCharacterParameter[] Party_088 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_088 = new SEventJumpParameter("グルガン族の谷：到着", "t10_01", new float[3] { -175f, 0f, 100f }, new float[3] { -132f, 0f, 146f }, 1, const_cast<short[]>(PossessionItem_088), 10000, const_cast<SCharacterParameter[]>(Party_088), 2, const_cast<short[]>(GlobalFlag_088), const_cast<short[]>(TreasureFlag_088));

		private static short[] GlobalFlag_089 = new short[72]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 901, 903, 904, 905,
			906, 907, 910, 953, 951, 950, 952, 850, 851, 852,
			853, -1
		};

		private static short[] TreasureFlag_089 = new short[1] { -1 };

		private static short[] PossessionItem_089 = new short[3] { 4006, 5207, -1 };

		private static SCharacterParameter[] Party_089 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_089 = new SEventJumpParameter("グルガン族の谷：Ｂ２", "t10_02", new float[3] { 162f, 0f, 80f }, new float[3] { -132f, 0f, 146f }, 1, const_cast<short[]>(PossessionItem_089), 10000, const_cast<SCharacterParameter[]>(Party_089), 2, const_cast<short[]>(GlobalFlag_089), const_cast<short[]>(TreasureFlag_089));

		private static short[] GlobalFlag_090 = new short[73]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 901, 903, 904,
			905, 906, 907, 910, 953, 951, 950, 952, 850, 851,
			852, 853, -1
		};

		private static short[] TreasureFlag_090 = new short[1] { -1 };

		private static short[] PossessionItem_090 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_090 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_090 = new SEventJumpParameter("ワールド：トード入手後～オーエン", "f01_55", new float[3] { -34f, 0f, 304f }, new float[3] { -34f, 0f, 270f }, 1, const_cast<short[]>(PossessionItem_090), 10000, const_cast<SCharacterParameter[]>(Party_090), 2, const_cast<short[]>(GlobalFlag_090), const_cast<short[]>(TreasureFlag_090));

		private static short[] GlobalFlag_091 = new short[73]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 901, 903, 904,
			905, 906, 907, 910, 953, 951, 950, 952, 850, 851,
			852, 853, -1
		};

		private static short[] TreasureFlag_091 = new short[1] { -1 };

		private static short[] PossessionItem_091 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_091 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 15, pl.JOB_TYPE.RED_MAGICIAN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_091 = new SEventJumpParameter("オーエンの塔：Ｂ１", "d07_01", new float[3] { -10f, 0f, -350f }, new float[3] { -34f, 0f, 268f }, 1, const_cast<short[]>(PossessionItem_091), 10000, const_cast<SCharacterParameter[]>(Party_091), 2, const_cast<short[]>(GlobalFlag_091), const_cast<short[]>(TreasureFlag_091));

		private static short[] GlobalFlag_092 = new short[73]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 901, 903, 904,
			905, 906, 907, 910, 953, 951, 950, 952, 850, 851,
			852, 853, -1
		};

		private static short[] TreasureFlag_092 = new short[1] { -1 };

		private static short[] PossessionItem_092 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_092 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_092 = new SEventJumpParameter("オーエンの塔：１F", "d07_02", new float[3] { -865f, 0f, -155f }, new float[3] { -34f, 0f, 268f }, 1, const_cast<short[]>(PossessionItem_092), 10000, const_cast<SCharacterParameter[]>(Party_092), 2, const_cast<short[]>(GlobalFlag_092), const_cast<short[]>(TreasureFlag_092));

		private static short[] GlobalFlag_093 = new short[75]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 901,
			903, 904, 905, 906, 907, 910, 953, 951, 950, 952,
			850, 851, 852, 853, -1
		};

		private static short[] TreasureFlag_093 = new short[1] { -1 };

		private static short[] PossessionItem_093 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_093 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 15, pl.JOB_TYPE.KNIGHT, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 15, pl.JOB_TYPE.MONK, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 15, pl.JOB_TYPE.WHITE_MAGICIAN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ 4001, -1, -1 },
				{ 4004, -1, -1 },
				{ 4007, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 15, pl.JOB_TYPE.BLACK_MAGICIAN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ 4101, 4102, 4103 },
				{ 4104, -1, -1 },
				{ 4107, 4108, 4109 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_093 = new SEventJumpParameter("オーエンの塔：４F", "d07_05", new float[3] { -900f, 0f, -408f }, new float[3] { -34f, 0f, 268f }, 1, const_cast<short[]>(PossessionItem_093), 10000, const_cast<SCharacterParameter[]>(Party_093), 2, const_cast<short[]>(GlobalFlag_093), const_cast<short[]>(TreasureFlag_093));

		private static short[] GlobalFlag_094 = new short[76]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			901, 903, 904, 905, 906, 907, 910, 953, 951, 950,
			952, 850, 851, 852, 853, -1
		};

		private static short[] TreasureFlag_094 = new short[1] { -1 };

		private static short[] PossessionItem_094 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_094 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 15, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_094 = new SEventJumpParameter("オーエンの塔：５F", "d07_06", new float[3] { -580f, 0f, -407f }, new float[3] { -34f, 0f, 268f }, 1, const_cast<short[]>(PossessionItem_094), 10000, const_cast<SCharacterParameter[]>(Party_094), 2, const_cast<short[]>(GlobalFlag_094), const_cast<short[]>(TreasureFlag_094));

		private static short[] GlobalFlag_095 = new short[78]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 901, 903, 904, 905, 906, 907, 910, 953,
			951, 950, 952, 850, 851, 852, 853, -1
		};

		private static short[] TreasureFlag_095 = new short[1] { -1 };

		private static short[] PossessionItem_095 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_095 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 15, pl.JOB_TYPE.FIGHTER, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 15, pl.JOB_TYPE.MONK, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 15, pl.JOB_TYPE.BLACK_MAGICIAN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 15, pl.JOB_TYPE.WHITE_MAGICIAN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_095 = new SEventJumpParameter("オーエンの塔：メデューサ前", "d07_11", new float[3] { 7f, 0f, -110f }, new float[3] { -34f, 0f, 268f }, 1, const_cast<short[]>(PossessionItem_095), 10000, const_cast<SCharacterParameter[]>(Party_095), 2, const_cast<short[]>(GlobalFlag_095), const_cast<short[]>(TreasureFlag_095));

		private static short[] GlobalFlag_096 = new short[79]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 901, 903, 904, 905, 906, 907, 910,
			953, 951, 950, 952, 850, 851, 852, 853, -1
		};

		private static short[] TreasureFlag_096 = new short[1] { -1 };

		private static short[] PossessionItem_096 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_096 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_096 = new SEventJumpParameter("オーエンの塔：メデューサ後", "d07_11", new float[3] { 7f, 0f, -110f }, new float[3] { -34f, 0f, 268f }, 1, const_cast<short[]>(PossessionItem_096), 10000, const_cast<SCharacterParameter[]>(Party_096), 2, const_cast<short[]>(GlobalFlag_096), const_cast<short[]>(TreasureFlag_096));

		private static short[] GlobalFlag_097 = new short[81]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 137, 901, 903, 904, 905, 906,
			907, 910, 953, 951, 950, 952, 850, 851, 852, 853,
			-1
		};

		private static short[] TreasureFlag_097 = new short[1] { -1 };

		private static short[] PossessionItem_097 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_097 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_097 = new SEventJumpParameter("ワールド：メデューサ後②", "f01_42", new float[3] { 46f, 0f, 310f }, new float[3] { -34f, 0f, 270f }, 1, const_cast<short[]>(PossessionItem_097), 10000, const_cast<SCharacterParameter[]>(Party_097), -1, const_cast<short[]>(GlobalFlag_097), const_cast<short[]>(TreasureFlag_097));

		private static short[] GlobalFlag_098 = new short[82]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 901, 903, 904, 905,
			906, 907, 910, 953, 951, 950, 952, 850, 851, 852,
			853, -1
		};

		private static short[] TreasureFlag_098 = new short[1] { -1 };

		private static short[] PossessionItem_098 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_098 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_098 = new SEventJumpParameter("ウル：メデューサ撃破後（フリー）", "t01_01", new float[3] { 0f, 0f, -170f }, new float[3] { 233f, 0f, -61f }, 1, const_cast<short[]>(PossessionItem_098), 10000, const_cast<SCharacterParameter[]>(Party_098), -1, const_cast<short[]>(GlobalFlag_098), const_cast<short[]>(TreasureFlag_098));

		private static short[] GlobalFlag_099 = new short[82]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 901, 903, 904, 905,
			906, 907, 910, 953, 951, 950, 952, 850, 851, 852,
			853, -1
		};

		private static short[] TreasureFlag_099 = new short[1] { -1 };

		private static short[] PossessionItem_099 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_099 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_099 = new SEventJumpParameter("ギサール：３章", "t13_01", new float[3] { 0f, 0f, -10f }, new float[3] { 593f, 0f, -234f }, 1, const_cast<short[]>(PossessionItem_099), 10000, const_cast<SCharacterParameter[]>(Party_099), -1, const_cast<short[]>(GlobalFlag_099), const_cast<short[]>(TreasureFlag_099));

		private static short[] GlobalFlag_100 = new short[82]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 901, 903, 904, 905,
			906, 907, 910, 953, 951, 950, 952, 850, 851, 852,
			853, -1
		};

		private static short[] TreasureFlag_100 = new short[1] { -1 };

		private static short[] PossessionItem_100 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_100 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_100 = new SEventJumpParameter("ドワーフの洞窟：入り口", "t14_01", new float[3] { -60f, 0f, 125f }, new float[3] { -290f, 0f, 276f }, 1, const_cast<short[]>(PossessionItem_100), 10000, const_cast<SCharacterParameter[]>(Party_100), -1, const_cast<short[]>(GlobalFlag_100), const_cast<short[]>(TreasureFlag_100));

		private static short[] GlobalFlag_101 = new short[83]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 901, 903, 904,
			905, 906, 907, 910, 953, 951, 950, 952, 850, 851,
			852, 853, -1
		};

		private static short[] TreasureFlag_101 = new short[1] { -1 };

		private static short[] PossessionItem_101 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_101 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_101 = new SEventJumpParameter("ドワーフの洞窟：広間", "t14_02", new float[3] { -195f, 0f, -200f }, new float[3] { -290f, 0f, 276f }, 1, const_cast<short[]>(PossessionItem_101), 10000, const_cast<SCharacterParameter[]>(Party_101), -1, const_cast<short[]>(GlobalFlag_101), const_cast<short[]>(TreasureFlag_101));

		private static short[] GlobalFlag_102 = new short[84]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 901, 903,
			904, 905, 906, 907, 910, 953, 951, 950, 952, 850,
			851, 852, 853, -1
		};

		private static short[] TreasureFlag_102 = new short[1] { -1 };

		private static short[] PossessionItem_102 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_102 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_102 = new SEventJumpParameter("ドワーフの洞窟：族長と会話後", "t14_02", new float[3] { -195f, 0f, -200f }, new float[3] { -290f, 0f, 276f }, 1, const_cast<short[]>(PossessionItem_102), 10000, const_cast<SCharacterParameter[]>(Party_102), -1, const_cast<short[]>(GlobalFlag_102), const_cast<short[]>(TreasureFlag_102));

		private static short[] GlobalFlag_103 = new short[84]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 901, 903,
			904, 905, 906, 907, 910, 953, 951, 950, 952, 850,
			851, 852, 853, -1
		};

		private static short[] TreasureFlag_103 = new short[1] { -1 };

		private static short[] PossessionItem_103 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_103 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 18, pl.JOB_TYPE.RED_MAGICIAN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 18, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 18, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 18, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_103 = new SEventJumpParameter("ドワーフの洞窟：地底湖前", "t14_06", new float[3] { 320f, 0f, 50f }, new float[3] { -290f, 0f, 276f }, 1, const_cast<short[]>(PossessionItem_103), 10000, const_cast<SCharacterParameter[]>(Party_103), -1, const_cast<short[]>(GlobalFlag_103), const_cast<short[]>(TreasureFlag_103));

		private static short[] GlobalFlag_104 = new short[86]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			901, 903, 904, 905, 906, 907, 910, 953, 951, 950,
			952, 850, 851, 852, 853, -1
		};

		private static short[] TreasureFlag_104 = new short[1] { -1 };

		private static short[] PossessionItem_104 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_104 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_104 = new SEventJumpParameter("地底湖：Ｂ１", "d08_01", new float[3] { -345f, 0f, -150f }, new float[3] { -290f, 0f, 276f }, 1, const_cast<short[]>(PossessionItem_104), 10000, const_cast<SCharacterParameter[]>(Party_104), -1, const_cast<short[]>(GlobalFlag_104), const_cast<short[]>(TreasureFlag_104));

		private static short[] GlobalFlag_105 = new short[86]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			901, 903, 904, 905, 906, 907, 910, 953, 951, 950,
			952, 850, 851, 852, 853, -1
		};

		private static short[] TreasureFlag_105 = new short[1] { -1 };

		private static short[] PossessionItem_105 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_105 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 18, pl.JOB_TYPE.FIGHTER, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 18, pl.JOB_TYPE.MONK, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 18, pl.JOB_TYPE.BLACK_MAGICIAN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 18, pl.JOB_TYPE.WHITE_MAGICIAN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_105 = new SEventJumpParameter("地底湖：グツコー前", "d08_04", new float[3] { 230f, 0f, 255f }, new float[3] { -290f, 0f, 276f }, 1, const_cast<short[]>(PossessionItem_105), 10000, const_cast<SCharacterParameter[]>(Party_105), -1, const_cast<short[]>(GlobalFlag_105), const_cast<short[]>(TreasureFlag_105));

		private static short[] GlobalFlag_106 = new short[87]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 901, 903, 904, 905, 906, 907, 910, 953, 951,
			950, 952, 850, 851, 852, 853, -1
		};

		private static short[] TreasureFlag_106 = new short[1] { -1 };

		private static short[] PossessionItem_106 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_106 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_106 = new SEventJumpParameter("地底湖：グツコー後", "d08_04", new float[3] { 230f, 0f, 255f }, new float[3] { -290f, 0f, 276f }, 1, const_cast<short[]>(PossessionItem_106), 10000, const_cast<SCharacterParameter[]>(Party_106), -1, const_cast<short[]>(GlobalFlag_106), const_cast<short[]>(TreasureFlag_106));

		private static short[] GlobalFlag_107 = new short[88]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 901, 903, 904, 905, 906, 907, 910, 953,
			951, 950, 952, 850, 851, 852, 853, -1
		};

		private static short[] TreasureFlag_107 = new short[1] { -1 };

		private static short[] PossessionItem_107 = new short[5] { 4006, 5207, 4005, 5202, -1 };

		private static SCharacterParameter[] Party_107 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_107 = new SEventJumpParameter("地底湖：グツコー後フリー", "d08_04", new float[3] { 230f, 0f, 255f }, new float[3] { -290f, 0f, 276f }, 1, const_cast<short[]>(PossessionItem_107), 10000, const_cast<SCharacterParameter[]>(Party_107), 3, const_cast<short[]>(GlobalFlag_107), const_cast<short[]>(TreasureFlag_107));

		private static short[] GlobalFlag_108 = new short[89]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 901, 903, 904, 905, 906, 907, 910,
			953, 951, 950, 952, 850, 851, 852, 853, -1
		};

		private static short[] TreasureFlag_108 = new short[1] { -1 };

		private static short[] PossessionItem_108 = new short[5] { 4006, 5207, 4005, 5202, -1 };

		private static SCharacterParameter[] Party_108 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_108 = new SEventJumpParameter("ドワーフの洞窟：グツコー撃破後広間", "t14_02", new float[3] { -195f, 0f, -200f }, new float[3] { -290f, 0f, 276f }, 1, const_cast<short[]>(PossessionItem_108), 10000, const_cast<SCharacterParameter[]>(Party_108), 3, const_cast<short[]>(GlobalFlag_108), const_cast<short[]>(TreasureFlag_108));

		private static short[] GlobalFlag_109 = new short[90]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 901, 903, 904, 905, 906, 907,
			910, 953, 951, 950, 952, 850, 851, 852, 853, -1
		};

		private static short[] TreasureFlag_109 = new short[1] { -1 };

		private static short[] PossessionItem_109 = new short[5] { 4006, 5207, 4005, 5202, -1 };

		private static SCharacterParameter[] Party_109 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_109 = new SEventJumpParameter("ドワーフの洞窟：祭壇開放中", "t14_02", new float[3] { -195f, 0f, -200f }, new float[3] { -290f, 0f, 276f }, 1, const_cast<short[]>(PossessionItem_109), 10000, const_cast<SCharacterParameter[]>(Party_109), 3, const_cast<short[]>(GlobalFlag_109), const_cast<short[]>(TreasureFlag_109));

		private static short[] GlobalFlag_110 = new short[91]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 901, 903, 904, 905, 906,
			907, 910, 953, 951, 950, 952, 850, 851, 852, 853,
			-1
		};

		private static short[] TreasureFlag_110 = new short[1] { -1 };

		private static short[] PossessionItem_110 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_110 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_110 = new SEventJumpParameter("ドワーフの洞窟：盗難後", "t14_02", new float[3] { -195f, 0f, -200f }, new float[3] { -290f, 0f, 276f }, 1, const_cast<short[]>(PossessionItem_110), 10000, const_cast<SCharacterParameter[]>(Party_110), -1, const_cast<short[]>(GlobalFlag_110), const_cast<short[]>(TreasureFlag_110));

		private static short[] GlobalFlag_111 = new short[91]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 901, 903, 904, 905, 906,
			907, 910, 953, 951, 950, 952, 850, 851, 852, 853,
			-1
		};

		private static short[] TreasureFlag_111 = new short[1] { -1 };

		private static short[] PossessionItem_111 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_111 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 20, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_111 = new SEventJumpParameter("炎の洞窟：Ｂ１", "d09_01", new float[3] { 145f, 0f, 100f }, new float[3] { -337f, 0f, 377f }, 1, const_cast<short[]>(PossessionItem_111), 10000, const_cast<SCharacterParameter[]>(Party_111), -1, const_cast<short[]>(GlobalFlag_111), const_cast<short[]>(TreasureFlag_111));

		private static short[] GlobalFlag_112 = new short[93]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 163, 164, 901, 903, 904,
			905, 906, 907, 910, 953, 951, 950, 952, 850, 851,
			852, 853, -1
		};

		private static short[] TreasureFlag_112 = new short[1] { -1 };

		private static short[] PossessionItem_112 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_112 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 20, pl.JOB_TYPE.FIGHTER, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 20, pl.JOB_TYPE.MONK, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 20, pl.JOB_TYPE.BLACK_MAGICIAN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 20, pl.JOB_TYPE.WHITE_MAGICIAN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_112 = new SEventJumpParameter("炎の洞窟：サラマンダ前", "d09_05", new float[3] { 0f, 0f, -90f }, new float[3] { -337f, 0f, 377f }, 1, const_cast<short[]>(PossessionItem_112), 10000, const_cast<SCharacterParameter[]>(Party_112), -1, const_cast<short[]>(GlobalFlag_112), const_cast<short[]>(TreasureFlag_112));

		private static short[] GlobalFlag_113 = new short[94]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 163, 164, 165, 901, 903,
			904, 905, 906, 907, 910, 953, 951, 950, 952, 850,
			851, 852, 853, -1
		};

		private static short[] TreasureFlag_113 = new short[1] { -1 };

		private static short[] PossessionItem_113 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_113 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_113 = new SEventJumpParameter("炎の洞窟：サラマンダ後", "d09_05", new float[3] { 0f, 0f, -30f }, new float[3] { -337f, 0f, 377f }, 1, const_cast<short[]>(PossessionItem_113), 10000, const_cast<SCharacterParameter[]>(Party_113), -1, const_cast<short[]>(GlobalFlag_113), const_cast<short[]>(TreasureFlag_113));

		private static short[] GlobalFlag_114 = new short[99]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 163, 164, 165, 166, 901,
			903, 904, 905, 906, 907, 910, 908, 909, 911, 912,
			953, 951, 950, 952, 850, 851, 852, 853, -1
		};

		private static short[] TreasureFlag_114 = new short[1] { -1 };

		private static short[] PossessionItem_114 = new short[5] { 4006, 5207, 4005, 5202, -1 };

		private static SCharacterParameter[] Party_114 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_114 = new SEventJumpParameter("炎の洞窟：サラマンダ後フリー", "d09_05", new float[3] { 0f, 0f, -30f }, new float[3] { -337f, 0f, 377f }, 1, const_cast<short[]>(PossessionItem_114), 10000, const_cast<SCharacterParameter[]>(Party_114), -1, const_cast<short[]>(GlobalFlag_114), const_cast<short[]>(TreasureFlag_114));

		private static short[] GlobalFlag_115 = new short[99]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 163, 164, 165, 166, 901,
			903, 904, 905, 906, 907, 910, 908, 909, 911, 912,
			953, 951, 950, 952, 850, 851, 852, 853, -1
		};

		private static short[] TreasureFlag_115 = new short[1] { -1 };

		private static short[] PossessionItem_115 = new short[5] { 4006, 5207, 4005, 5202, -1 };

		private static SCharacterParameter[] Party_115 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_115 = new SEventJumpParameter("ワールド：サラマンダ撃破後", "f01_32", new float[3] { -269f, 0f, 300f }, new float[3] { -290f, 0f, 276f }, 1, const_cast<short[]>(PossessionItem_115), 10000, const_cast<SCharacterParameter[]>(Party_115), -1, const_cast<short[]>(GlobalFlag_115), const_cast<short[]>(TreasureFlag_115));

		private static short[] GlobalFlag_116 = new short[99]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 163, 164, 165, 166, 901,
			903, 904, 905, 906, 907, 910, 908, 909, 911, 912,
			953, 951, 950, 952, 850, 851, 852, 853, -1
		};

		private static short[] TreasureFlag_116 = new short[1] { -1 };

		private static short[] PossessionItem_116 = new short[5] { 4006, 5207, 4005, 5202, -1 };

		private static SCharacterParameter[] Party_116 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_116 = new SEventJumpParameter("ドワーフの洞窟：サラマンダ撃破後広間", "t14_02", new float[3] { -195f, 0f, -200f }, new float[3] { -290f, 0f, 276f }, 1, const_cast<short[]>(PossessionItem_116), 10000, const_cast<SCharacterParameter[]>(Party_116), -1, const_cast<short[]>(GlobalFlag_116), const_cast<short[]>(TreasureFlag_116));

		private static short[] GlobalFlag_117 = new short[101]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
			169, 901, 903, 904, 905, 906, 907, 910, 908, 909,
			911, 912, 953, 951, 950, 952, 850, 851, 852, 853,
			-1
		};

		private static short[] TreasureFlag_117 = new short[1] { -1 };

		private static short[] PossessionItem_117 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_117 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_117 = new SEventJumpParameter("ドワーフの洞窟：族長のお礼後", "t14_02", new float[3] { -195f, 0f, -200f }, new float[3] { -290f, 0f, 276f }, 1, const_cast<short[]>(PossessionItem_117), 10000, const_cast<SCharacterParameter[]>(Party_117), -1, const_cast<short[]>(GlobalFlag_117), const_cast<short[]>(TreasureFlag_117));

		private static short[] GlobalFlag_118 = new short[101]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
			169, 901, 903, 904, 905, 906, 907, 910, 908, 909,
			911, 912, 953, 951, 950, 952, 850, 851, 852, 853,
			-1
		};

		private static short[] TreasureFlag_118 = new short[1] { -1 };

		private static short[] PossessionItem_118 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_118 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_118 = new SEventJumpParameter("ドワーフの洞窟：トックル人死亡前", "t14_01", new float[3] { -150f, 0f, 120f }, new float[3] { -290f, 0f, 276f }, 1, const_cast<short[]>(PossessionItem_118), 10000, const_cast<SCharacterParameter[]>(Party_118), -1, const_cast<short[]>(GlobalFlag_118), const_cast<short[]>(TreasureFlag_118));

		private static short[] GlobalFlag_119 = new short[102]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
			169, 171, 901, 903, 904, 905, 906, 907, 910, 908,
			909, 911, 912, 953, 951, 950, 952, 850, 851, 852,
			853, -1
		};

		private static short[] TreasureFlag_119 = new short[1] { -1 };

		private static short[] PossessionItem_119 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_119 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_119 = new SEventJumpParameter("ワールド：トックル人死亡後～トックル", "f01_32", new float[3] { -269f, 0f, 300f }, new float[3] { -290f, 0f, 276f }, 1, const_cast<short[]>(PossessionItem_119), 10000, const_cast<SCharacterParameter[]>(Party_119), -1, const_cast<short[]>(GlobalFlag_119), const_cast<short[]>(TreasureFlag_119));

		private static short[] GlobalFlag_120 = new short[102]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
			169, 171, 901, 903, 904, 905, 906, 907, 910, 908,
			909, 911, 912, 953, 951, 950, 952, 850, 851, 852,
			853, -1
		};

		private static short[] TreasureFlag_120 = new short[1] { -1 };

		private static short[] PossessionItem_120 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_120 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 23, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 23, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 23, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 23, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_120 = new SEventJumpParameter("トックル：さらわれる", "t08_01", new float[3] { 0f, 0f, -10f }, new float[3] { -290f, 0f, 276f }, 1, const_cast<short[]>(PossessionItem_120), 10000, const_cast<SCharacterParameter[]>(Party_120), -1, const_cast<short[]>(GlobalFlag_120), const_cast<short[]>(TreasureFlag_120));

		private static short[] GlobalFlag_121 = new short[103]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
			169, 171, 175, 901, 903, 904, 905, 906, 907, 910,
			908, 909, 911, 912, 953, 951, 950, 952, 850, 851,
			852, 853, -1
		};

		private static short[] TreasureFlag_121 = new short[1] { -1 };

		private static short[] PossessionItem_121 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_121 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_121 = new SEventJumpParameter("ハインの城：到着", "d10_01", new float[3] { -115f, 0f, 35f }, new float[3] { -290f, 0f, 276f }, 1, const_cast<short[]>(PossessionItem_121), 10000, const_cast<SCharacterParameter[]>(Party_121), -1, const_cast<short[]>(GlobalFlag_121), const_cast<short[]>(TreasureFlag_121));

		private static short[] GlobalFlag_122 = new short[104]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
			169, 171, 175, 177, 901, 903, 904, 905, 906, 907,
			910, 908, 909, 911, 912, 953, 951, 950, 952, 850,
			851, 852, 853, -1
		};

		private static short[] TreasureFlag_122 = new short[1] { -1 };

		private static short[] PossessionItem_122 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_122 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 30, pl.JOB_TYPE.RED_MAGICIAN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_122 = new SEventJumpParameter("ハインの城：到着後", "d10_01", new float[3] { -115f, 0f, 35f }, new float[3] { -290f, 0f, 276f }, 1, const_cast<short[]>(PossessionItem_122), 10000, const_cast<SCharacterParameter[]>(Party_122), -1, const_cast<short[]>(GlobalFlag_122), const_cast<short[]>(TreasureFlag_122));

		private static short[] GlobalFlag_123 = new short[108]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
			169, 171, 175, 177, 178, 179, 180, 186, 901, 903,
			904, 905, 906, 907, 910, 908, 909, 911, 912, 953,
			951, 950, 952, 850, 851, 852, 853, -1
		};

		private static short[] TreasureFlag_123 = new short[1] { -1 };

		private static short[] PossessionItem_123 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_123 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 23, pl.JOB_TYPE.KNIGHT, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 23, pl.JOB_TYPE.BOOK_MAN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 23, pl.JOB_TYPE.BLACK_MAGICIAN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 23, pl.JOB_TYPE.WHITE_MAGICIAN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_123 = new SEventJumpParameter("ハインの城：ハイン前", "d10_12", new float[3] { 0f, 0f, -10f }, new float[3] { -290f, 0f, 276f }, 1, const_cast<short[]>(PossessionItem_123), 10000, const_cast<SCharacterParameter[]>(Party_123), -1, const_cast<short[]>(GlobalFlag_123), const_cast<short[]>(TreasureFlag_123));

		private static short[] GlobalFlag_124 = new short[109]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
			169, 171, 175, 177, 178, 179, 180, 186, 184, 901,
			903, 904, 905, 906, 907, 910, 908, 909, 911, 912,
			953, 951, 950, 952, 850, 851, 852, 853, -1
		};

		private static short[] TreasureFlag_124 = new short[1] { -1 };

		private static short[] PossessionItem_124 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_124 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_124 = new SEventJumpParameter("ハインの城：ハイン後", "d10_12", new float[3] { 0f, 0f, -10f }, new float[3] { -290f, 0f, 276f }, 1, const_cast<short[]>(PossessionItem_124), 10000, const_cast<SCharacterParameter[]>(Party_124), -1, const_cast<short[]>(GlobalFlag_124), const_cast<short[]>(TreasureFlag_124));

		private static short[] GlobalFlag_125 = new short[110]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
			169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
			901, 903, 904, 905, 906, 907, 910, 908, 909, 911,
			912, 953, 951, 950, 952, 850, 851, 852, 853, -1
		};

		private static short[] TreasureFlag_125 = new short[1] { -1 };

		private static short[] PossessionItem_125 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_125 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_125 = new SEventJumpParameter("生きている森：ハイン撃破後", "t11_02", new float[3] { 0f, 0f, -10f }, new float[3] { -193f, 0f, -116f }, 1, const_cast<short[]>(PossessionItem_125), 10000, const_cast<SCharacterParameter[]>(Party_125), -1, const_cast<short[]>(GlobalFlag_125), const_cast<short[]>(TreasureFlag_125));

		private static short[] GlobalFlag_126 = new short[112]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
			169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
			189, 190, 901, 903, 904, 905, 906, 907, 910, 908,
			909, 911, 912, 953, 951, 950, 952, 850, 851, 852,
			853, -1
		};

		private static short[] TreasureFlag_126 = new short[1] { -1 };

		private static short[] PossessionItem_126 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_126 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_126 = new SEventJumpParameter("アーガス城：ハイン撃破後", "t12_01", new float[3] { 25f, 0f, -113f }, new float[3] { -99f, 0f, -7f }, 1, const_cast<short[]>(PossessionItem_126), 10000, const_cast<SCharacterParameter[]>(Party_126), -1, const_cast<short[]>(GlobalFlag_126), const_cast<short[]>(TreasureFlag_126));

		private static short[] GlobalFlag_127 = new short[113]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
			169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
			189, 190, 193, 901, 903, 904, 905, 906, 907, 910,
			908, 909, 911, 912, 953, 951, 950, 952, 850, 851,
			852, 853, -1
		};

		private static short[] TreasureFlag_127 = new short[1] { -1 };

		private static short[] PossessionItem_127 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_127 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_127 = new SEventJumpParameter("アーガス城：王の前", "t12_04", new float[3] { -35f, 0f, 45f }, new float[3] { -99f, 0f, -7f }, 1, const_cast<short[]>(PossessionItem_127), 10000, const_cast<SCharacterParameter[]>(Party_127), -1, const_cast<short[]>(GlobalFlag_127), const_cast<short[]>(TreasureFlag_127));

		private static short[] GlobalFlag_128 = new short[114]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
			169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
			189, 190, 193, 195, 901, 903, 904, 905, 906, 907,
			910, 908, 909, 911, 912, 953, 951, 950, 952, 850,
			851, 852, 853, -1
		};

		private static short[] TreasureFlag_128 = new short[1] { -1 };

		private static short[] PossessionItem_128 = new short[5] { 4006, 5207, 4005, 5203, -1 };

		private static SCharacterParameter[] Party_128 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_128 = new SEventJumpParameter("アーガス城：王後", "t12_04", new float[3] { -35f, 0f, 45f }, new float[3] { -99f, 0f, -7f }, 1, const_cast<short[]>(PossessionItem_128), 10000, const_cast<SCharacterParameter[]>(Party_128), -1, const_cast<short[]>(GlobalFlag_128), const_cast<short[]>(TreasureFlag_128));

		private static short[] GlobalFlag_129 = new short[114]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
			169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
			189, 190, 193, 195, 901, 903, 904, 905, 906, 907,
			910, 908, 909, 911, 912, 953, 951, 950, 952, 850,
			851, 852, 853, -1
		};

		private static short[] TreasureFlag_129 = new short[1] { -1 };

		private static short[] PossessionItem_129 = new short[5] { 4006, 5207, 4005, 5203, -1 };

		private static SCharacterParameter[] Party_129 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_129 = new SEventJumpParameter("カナーン：時の歯車帰還", "t04_01", new float[3] { -10f, 0f, -200f }, new float[3] { 294f, 0f, -116f }, 1, const_cast<short[]>(PossessionItem_129), 10000, const_cast<SCharacterParameter[]>(Party_129), -1, const_cast<short[]>(GlobalFlag_129), const_cast<short[]>(TreasureFlag_129));

		private static short[] GlobalFlag_130 = new short[116]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
			169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
			189, 190, 193, 195, 200, 901, 903, 904, 905, 906,
			907, 910, 908, 909, 911, 912, 953, 951, 950, 952,
			954, 850, 851, 852, 853, -1
		};

		private static short[] TreasureFlag_130 = new short[1] { -1 };

		private static short[] PossessionItem_130 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_130 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_130 = new SEventJumpParameter("カナーン：飛空挺改造済み", "t04_01", new float[3] { -10f, 0f, -200f }, new float[3] { 294f, 0f, -116f }, 1, const_cast<short[]>(PossessionItem_130), 10000, const_cast<SCharacterParameter[]>(Party_130), -1, const_cast<short[]>(GlobalFlag_130), const_cast<short[]>(TreasureFlag_130));

		private static short[] GlobalFlag_131 = new short[116]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
			169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
			189, 190, 193, 195, 200, 901, 903, 904, 905, 906,
			907, 910, 908, 909, 911, 912, 953, 951, 950, 952,
			954, 850, 851, 852, 853, -1
		};

		private static short[] TreasureFlag_131 = new short[1] { -1 };

		private static short[] PossessionItem_131 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_131 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_131 = new SEventJumpParameter("ワールド：飛空挺改造済み", "f01_65", new float[3] { 365f, 0f, -187f }, new float[3] { 294f, 0f, -116f }, 1, const_cast<short[]>(PossessionItem_131), 10000, const_cast<SCharacterParameter[]>(Party_131), -1, const_cast<short[]>(GlobalFlag_131), const_cast<short[]>(TreasureFlag_131));

		private static short[] GlobalFlag_132 = new short[117]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
			169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
			189, 190, 193, 195, 200, 901, 903, 904, 905, 906,
			907, 910, 908, 909, 911, 912, 953, 951, 950, 952,
			954, 850, 851, 852, 853, 854, -1
		};

		private static short[] TreasureFlag_132 = new short[1] { -1 };

		private static short[] PossessionItem_132 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_132 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_132 = new SEventJumpParameter("水没ワールド：飛空挺改造済み（座標未定）", "f02_CA", new float[3] { 1131f, 0f, -500f }, new float[3] { 1130f, 0f, -536f }, 2, const_cast<short[]>(PossessionItem_132), 10000, const_cast<SCharacterParameter[]>(Party_132), -1, const_cast<short[]>(GlobalFlag_132), const_cast<short[]>(TreasureFlag_132));

		private static short[] GlobalFlag_133 = new short[120]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
			169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
			189, 190, 193, 195, 200, 202, 209, 216, 901, 903,
			904, 905, 906, 907, 910, 908, 909, 911, 912, 953,
			951, 950, 952, 954, 850, 851, 852, 853, 854, -1
		};

		private static short[] TreasureFlag_133 = new short[1] { -1 };

		private static short[] PossessionItem_133 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_133 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_133 = new SEventJumpParameter("水の神殿：クリスタルルームエリア不在", "t16_02", new float[3] { 6f, 0f, -39f }, new float[3] { 1130f, 0f, -536f }, 2, const_cast<short[]>(PossessionItem_133), 10000, const_cast<SCharacterParameter[]>(Party_133), -1, const_cast<short[]>(GlobalFlag_133), const_cast<short[]>(TreasureFlag_133));

		private static short[] GlobalFlag_134 = new short[120]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
			169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
			189, 190, 193, 195, 200, 202, 209, 216, 901, 903,
			904, 905, 906, 907, 910, 908, 909, 911, 912, 953,
			951, 950, 952, 954, 850, 851, 852, 853, 854, -1
		};

		private static short[] TreasureFlag_134 = new short[1] { -1 };

		private static short[] PossessionItem_134 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_134 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_134 = new SEventJumpParameter("水の洞窟：Ｂ１ＦＡエリア不在", "d11_01", new float[3] { 138f, 0f, 136f }, new float[3] { 1130f, 0f, -536f }, 2, const_cast<short[]>(PossessionItem_134), 10000, const_cast<SCharacterParameter[]>(Party_134), -1, const_cast<short[]>(GlobalFlag_134), const_cast<short[]>(TreasureFlag_134));

		private static short[] GlobalFlag_135 = new short[119]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
			169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
			189, 190, 193, 195, 200, 202, 209, 901, 903, 904,
			905, 906, 907, 910, 908, 909, 911, 912, 953, 951,
			950, 952, 954, 850, 851, 852, 853, 854, -1
		};

		private static short[] TreasureFlag_135 = new short[1] { -1 };

		private static short[] PossessionItem_135 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_135 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_135 = new SEventJumpParameter("難破船：甲板", "t15_01", new float[3] { 66f, 0f, 206f }, new float[3] { 659f, 0f, 609f }, 2, const_cast<short[]>(PossessionItem_135), 10000, const_cast<SCharacterParameter[]>(Party_135), -1, const_cast<short[]>(GlobalFlag_135), const_cast<short[]>(TreasureFlag_135));

		private static short[] GlobalFlag_136 = new short[120]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
			169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
			189, 190, 193, 195, 200, 202, 209, 210, 901, 903,
			904, 905, 906, 907, 910, 908, 909, 911, 912, 953,
			951, 950, 952, 954, 850, 851, 852, 853, 854, -1
		};

		private static short[] TreasureFlag_136 = new short[1] { -1 };

		private static short[] PossessionItem_136 = new short[5] { 4006, 5207, 4005, 5001, -1 };

		private static SCharacterParameter[] Party_136 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_136 = new SEventJumpParameter("難破船：エリアと老人", "t15_03", new float[3] { 14f, 0f, 32f }, new float[3] { 659f, 0f, 609f }, 2, const_cast<short[]>(PossessionItem_136), 10000, const_cast<SCharacterParameter[]>(Party_136), -1, const_cast<short[]>(GlobalFlag_136), const_cast<short[]>(TreasureFlag_136));

		private static short[] GlobalFlag_137 = new short[122]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
			169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
			189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
			901, 903, 904, 905, 906, 907, 910, 908, 909, 911,
			912, 953, 951, 950, 952, 954, 850, 851, 852, 853,
			854, -1
		};

		private static short[] TreasureFlag_137 = new short[1] { -1 };

		private static short[] PossessionItem_137 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_137 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_137 = new SEventJumpParameter("難破船：エリア仲間後", "t15_03", new float[3] { 50f, 0f, 30f }, new float[3] { 659f, 0f, 609f }, 2, const_cast<short[]>(PossessionItem_137), 10000, const_cast<SCharacterParameter[]>(Party_137), 4, const_cast<short[]>(GlobalFlag_137), const_cast<short[]>(TreasureFlag_137));

		private static short[] GlobalFlag_138 = new short[123]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
			169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
			189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
			218, 901, 903, 904, 905, 906, 907, 910, 908, 909,
			911, 912, 953, 951, 950, 952, 954, 850, 851, 852,
			853, 854, -1
		};

		private static short[] TreasureFlag_138 = new short[1] { -1 };

		private static short[] PossessionItem_138 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_138 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_138 = new SEventJumpParameter("水の神殿：入り口エリア合流後", "t16_01", new float[3] { -76f, 0f, -204f }, new float[3] { 1130f, 0f, -536f }, 2, const_cast<short[]>(PossessionItem_138), 10000, const_cast<SCharacterParameter[]>(Party_138), 4, const_cast<short[]>(GlobalFlag_138), const_cast<short[]>(TreasureFlag_138));

		private static short[] GlobalFlag_139 = new short[123]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
			169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
			189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
			218, 901, 903, 904, 905, 906, 907, 910, 908, 909,
			911, 912, 953, 951, 950, 952, 954, 850, 851, 852,
			853, 854, -1
		};

		private static short[] TreasureFlag_139 = new short[1] { -1 };

		private static short[] PossessionItem_139 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_139 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_139 = new SEventJumpParameter("水の神殿：クリスタルルームエリア合流後", "t16_02", new float[3] { 5f, 0f, -39f }, new float[3] { 1130f, 0f, -536f }, 2, const_cast<short[]>(PossessionItem_139), 10000, const_cast<SCharacterParameter[]>(Party_139), 4, const_cast<short[]>(GlobalFlag_139), const_cast<short[]>(TreasureFlag_139));

		private static short[] GlobalFlag_140 = new short[124]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
			169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
			189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
			218, 219, 901, 903, 904, 905, 906, 907, 910, 908,
			909, 911, 912, 953, 951, 950, 952, 954, 850, 851,
			852, 853, 854, -1
		};

		private static short[] TreasureFlag_140 = new short[1] { -1 };

		private static short[] PossessionItem_140 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_140 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_140 = new SEventJumpParameter("水の洞窟：Ｂ１ＦＡ", "d11_01", new float[3] { 138f, 0f, 136f }, new float[3] { 1130f, 0f, -536f }, 2, const_cast<short[]>(PossessionItem_140), 10000, const_cast<SCharacterParameter[]>(Party_140), 4, const_cast<short[]>(GlobalFlag_140), const_cast<short[]>(TreasureFlag_140));

		private static short[] GlobalFlag_141 = new short[125]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
			169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
			189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
			218, 219, 223, 901, 903, 904, 905, 906, 907, 910,
			908, 909, 911, 912, 953, 951, 950, 952, 954, 850,
			851, 852, 853, 854, -1
		};

		private static short[] TreasureFlag_141 = new short[1] { -1 };

		private static short[] PossessionItem_141 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_141 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_141 = new SEventJumpParameter("水の洞窟：ボス前", "d11_06", new float[3] { 0f, 0f, -126f }, new float[3] { 1130f, 0f, -536f }, 2, const_cast<short[]>(PossessionItem_141), 10000, const_cast<SCharacterParameter[]>(Party_141), 4, const_cast<short[]>(GlobalFlag_141), const_cast<short[]>(TreasureFlag_141));

		private static short[] GlobalFlag_142 = new short[126]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
			169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
			189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
			218, 219, 223, 224, 901, 903, 904, 905, 906, 907,
			910, 908, 909, 911, 912, 953, 951, 950, 952, 954,
			850, 851, 852, 853, 854, -1
		};

		private static short[] TreasureFlag_142 = new short[1] { -1 };

		private static short[] PossessionItem_142 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_142 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_142 = new SEventJumpParameter("水の洞窟：ボス後", "d11_06", new float[3] { 0f, 0f, -10f }, new float[3] { 1130f, 0f, -536f }, 2, const_cast<short[]>(PossessionItem_142), 10000, const_cast<SCharacterParameter[]>(Party_142), -1, const_cast<short[]>(GlobalFlag_142), const_cast<short[]>(TreasureFlag_142));

		private static short[] GlobalFlag_143 = new short[134]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
			169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
			189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
			218, 219, 223, 224, 225, 901, 903, 904, 905, 906,
			907, 910, 908, 909, 911, 912, 913, 914, 915, 916,
			917, 953, 951, 950, 952, 954, 955, 850, 851, 852,
			853, 854, 855, -1
		};

		private static short[] TreasureFlag_143 = new short[1] { -1 };

		private static short[] PossessionItem_143 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_143 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_143 = new SEventJumpParameter("アムル：宿屋目覚め", "t17_03", new float[3] { 0f, 0f, -10f }, new float[3] { 695f, 0f, -820f }, 3, const_cast<short[]>(PossessionItem_143), 10000, const_cast<SCharacterParameter[]>(Party_143), -1, const_cast<short[]>(GlobalFlag_143), const_cast<short[]>(TreasureFlag_143));

		private static short[] GlobalFlag_144 = new short[136]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
			169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
			189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
			218, 219, 223, 224, 225, 239, 240, 901, 903, 904,
			905, 906, 907, 910, 908, 909, 911, 912, 913, 914,
			915, 916, 917, 953, 951, 950, 952, 954, 955, 850,
			851, 852, 853, 854, 855, -1
		};

		private static short[] TreasureFlag_144 = new short[1] { -1 };

		private static short[] PossessionItem_144 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_144 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_144 = new SEventJumpParameter("アムル：目覚め後", "t17_01", new float[3] { -5f, 0f, -170f }, new float[3] { 695f, 0f, -820f }, 3, const_cast<short[]>(PossessionItem_144), 10000, const_cast<SCharacterParameter[]>(Party_144), -1, const_cast<short[]>(GlobalFlag_144), const_cast<short[]>(TreasureFlag_144));

		private static short[] GlobalFlag_145 = new short[136]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
			169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
			189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
			218, 219, 223, 224, 225, 239, 240, 901, 903, 904,
			905, 906, 907, 910, 908, 909, 911, 912, 913, 914,
			915, 916, 917, 953, 951, 950, 952, 954, 955, 850,
			851, 852, 853, 854, 855, -1
		};

		private static short[] TreasureFlag_145 = new short[1] { -1 };

		private static short[] PossessionItem_145 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_145 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_145 = new SEventJumpParameter("アムル：ジルの家", "t17_07", new float[3] { -12f, 0f, -60f }, new float[3] { 695f, 0f, -820f }, 3, const_cast<short[]>(PossessionItem_145), 10000, const_cast<SCharacterParameter[]>(Party_145), -1, const_cast<short[]>(GlobalFlag_145), const_cast<short[]>(TreasureFlag_145));

		private static short[] GlobalFlag_146 = new short[137]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
			169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
			189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
			218, 219, 223, 224, 225, 239, 240, 242, 901, 903,
			904, 905, 906, 907, 910, 908, 909, 911, 912, 913,
			914, 915, 916, 917, 953, 951, 950, 952, 954, 955,
			850, 851, 852, 853, 854, 855, -1
		};

		private static short[] TreasureFlag_146 = new short[1] { -1 };

		private static short[] PossessionItem_146 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_146 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_146 = new SEventJumpParameter("アムル：下水道開通直前", "t17_01", new float[3] { -5f, 0f, -170f }, new float[3] { 695f, 0f, -820f }, 3, const_cast<short[]>(PossessionItem_146), 10000, const_cast<SCharacterParameter[]>(Party_146), -1, const_cast<short[]>(GlobalFlag_146), const_cast<short[]>(TreasureFlag_146));

		private static short[] GlobalFlag_147 = new short[138]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
			169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
			189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
			218, 219, 223, 224, 225, 239, 240, 242, 243, 901,
			903, 904, 905, 906, 907, 910, 908, 909, 911, 912,
			913, 914, 915, 916, 917, 953, 951, 950, 952, 954,
			955, 850, 851, 852, 853, 854, 855, -1
		};

		private static short[] TreasureFlag_147 = new short[1] { -1 };

		private static short[] PossessionItem_147 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_147 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_147 = new SEventJumpParameter("アムル：下水道開通後", "t17_01", new float[3] { -5f, 0f, -170f }, new float[3] { 695f, 0f, -820f }, 3, const_cast<short[]>(PossessionItem_147), 10000, const_cast<SCharacterParameter[]>(Party_147), -1, const_cast<short[]>(GlobalFlag_147), const_cast<short[]>(TreasureFlag_147));

		private static short[] GlobalFlag_148 = new short[139]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
			169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
			189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
			218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
			901, 903, 904, 905, 906, 907, 910, 908, 909, 911,
			912, 913, 914, 915, 916, 917, 953, 951, 950, 952,
			954, 955, 850, 851, 852, 853, 854, 855, -1
		};

		private static short[] TreasureFlag_148 = new short[1] { -1 };

		private static short[] PossessionItem_148 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_148 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_148 = new SEventJumpParameter("下水道：Ｂ１", "d12_01", new float[3] { -60f, 0f, 20f }, new float[3] { 695f, 0f, -820f }, 3, const_cast<short[]>(PossessionItem_148), 10000, const_cast<SCharacterParameter[]>(Party_148), -1, const_cast<short[]>(GlobalFlag_148), const_cast<short[]>(TreasureFlag_148));

		private static short[] GlobalFlag_149 = new short[139]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
			169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
			189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
			218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
			901, 903, 904, 905, 906, 907, 910, 908, 909, 911,
			912, 913, 914, 915, 916, 917, 953, 951, 950, 952,
			954, 955, 850, 851, 852, 853, 854, 855, -1
		};

		private static short[] TreasureFlag_149 = new short[1] { -1 };

		private static short[] PossessionItem_149 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_149 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_149 = new SEventJumpParameter("下水道：４じい前", "d12_03", new float[3] { -161f, 0f, 29f }, new float[3] { 695f, 0f, -820f }, 3, const_cast<short[]>(PossessionItem_149), 10000, const_cast<SCharacterParameter[]>(Party_149), -1, const_cast<short[]>(GlobalFlag_149), const_cast<short[]>(TreasureFlag_149));

		private static short[] GlobalFlag_150 = new short[140]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
			169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
			189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
			218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
			249, 901, 903, 904, 905, 906, 907, 910, 908, 909,
			911, 912, 913, 914, 915, 916, 917, 953, 951, 950,
			952, 954, 955, 850, 851, 852, 853, 854, 855, -1
		};

		private static short[] TreasureFlag_150 = new short[1] { -1 };

		private static short[] PossessionItem_150 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_150 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_150 = new SEventJumpParameter("下水道：４じい後", "d12_03", new float[3] { -70f, 0f, 10f }, new float[3] { 695f, 0f, -820f }, 3, const_cast<short[]>(PossessionItem_150), 10000, const_cast<SCharacterParameter[]>(Party_150), -1, const_cast<short[]>(GlobalFlag_150), const_cast<short[]>(TreasureFlag_150));

		private static short[] GlobalFlag_151 = new short[143]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
			169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
			189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
			218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
			249, 250, 251, 252, 901, 903, 904, 905, 906, 907,
			910, 908, 909, 911, 912, 913, 914, 915, 916, 917,
			953, 951, 950, 952, 954, 955, 850, 851, 852, 853,
			854, 855, -1
		};

		private static short[] TreasureFlag_151 = new short[1] { -1 };

		private static short[] PossessionItem_151 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_151 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 25, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_151 = new SEventJumpParameter("下水道：デリラばあさん前", "d12_07", new float[3] { -126f, 0f, -43f }, new float[3] { 695f, 0f, -820f }, 3, const_cast<short[]>(PossessionItem_151), 10000, const_cast<SCharacterParameter[]>(Party_151), -1, const_cast<short[]>(GlobalFlag_151), const_cast<short[]>(TreasureFlag_151));

		private static short[] GlobalFlag_152 = new short[145]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
			169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
			189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
			218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
			249, 250, 251, 252, 253, 254, 901, 903, 904, 905,
			906, 907, 910, 908, 909, 911, 912, 913, 914, 915,
			916, 917, 953, 951, 950, 952, 954, 955, 850, 851,
			852, 853, 854, 855, -1
		};

		private static short[] TreasureFlag_152 = new short[1] { -1 };

		private static short[] PossessionItem_152 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_152 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_152 = new SEventJumpParameter("アムル：下水道クリア後", "t17_01", new float[3] { -5f, 0f, -170f }, new float[3] { 695f, 0f, -820f }, 3, const_cast<short[]>(PossessionItem_152), 10000, const_cast<SCharacterParameter[]>(Party_152), -1, const_cast<short[]>(GlobalFlag_152), const_cast<short[]>(TreasureFlag_152));

		private static short[] GlobalFlag_153 = new short[138]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
			169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
			189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
			218, 219, 223, 224, 225, 239, 240, 242, 243, 901,
			903, 904, 905, 906, 907, 910, 908, 909, 911, 912,
			913, 914, 915, 916, 917, 953, 951, 950, 952, 954,
			955, 850, 851, 852, 853, 854, 855, -1
		};

		private static short[] TreasureFlag_153 = new short[1] { -1 };

		private static short[] PossessionItem_153 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_153 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_153 = new SEventJumpParameter("ワールド：下水道クリア前（座標未定）", "f03_AB", new float[3] { 715f, 0f, -830f }, new float[3] { 695f, 0f, -820f }, 3, const_cast<short[]>(PossessionItem_153), 10000, const_cast<SCharacterParameter[]>(Party_153), -1, const_cast<short[]>(GlobalFlag_153), const_cast<short[]>(TreasureFlag_153));

		private static short[] GlobalFlag_154 = new short[145]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
			169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
			189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
			218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
			249, 250, 251, 252, 253, 254, 901, 903, 904, 905,
			906, 907, 910, 908, 909, 911, 912, 913, 914, 915,
			916, 917, 953, 951, 950, 952, 954, 955, 850, 851,
			852, 853, 854, 855, -1
		};

		private static short[] TreasureFlag_154 = new short[1] { -1 };

		private static short[] PossessionItem_154 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_154 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_154 = new SEventJumpParameter("ワールド：下水道クリア後（座標未定）", "f03_AB", new float[3] { 715f, 0f, -830f }, new float[3] { 695f, 0f, -820f }, 3, const_cast<short[]>(PossessionItem_154), 10000, const_cast<SCharacterParameter[]>(Party_154), -1, const_cast<short[]>(GlobalFlag_154), const_cast<short[]>(TreasureFlag_154));

		private static short[] GlobalFlag_155 = new short[145]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
			169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
			189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
			218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
			249, 250, 251, 252, 253, 254, 901, 903, 904, 905,
			906, 907, 910, 908, 909, 911, 912, 913, 914, 915,
			916, 917, 953, 951, 950, 952, 954, 955, 850, 851,
			852, 853, 854, 855, -1
		};

		private static short[] TreasureFlag_155 = new short[1] { -1 };

		private static short[] PossessionItem_155 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_155 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 28, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 28, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 28, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 28, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_155 = new SEventJumpParameter("ゴールドルの館：１ＦＡ", "d13_01", new float[3] { 0f, 0f, -145f }, new float[3] { 695f, 0f, -820f }, 3, const_cast<short[]>(PossessionItem_155), 10000, const_cast<SCharacterParameter[]>(Party_155), -1, const_cast<short[]>(GlobalFlag_155), const_cast<short[]>(TreasureFlag_155));

		private static short[] GlobalFlag_156 = new short[146]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
			169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
			189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
			218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
			249, 250, 251, 252, 253, 254, 259, 901, 903, 904,
			905, 906, 907, 910, 908, 909, 911, 912, 913, 914,
			915, 916, 917, 953, 951, 950, 952, 954, 955, 850,
			851, 852, 853, 854, 855, -1
		};

		private static short[] TreasureFlag_156 = new short[1] { -1 };

		private static short[] PossessionItem_156 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_156 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 28, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 28, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 28, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 28, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_156 = new SEventJumpParameter("ゴールドルの館：ボス前", "d13_09", new float[3] { 0f, 0f, -95f }, new float[3] { 695f, 0f, -820f }, 3, const_cast<short[]>(PossessionItem_156), 10000, const_cast<SCharacterParameter[]>(Party_156), -1, const_cast<short[]>(GlobalFlag_156), const_cast<short[]>(TreasureFlag_156));

		private static short[] GlobalFlag_157 = new short[147]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
			169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
			189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
			218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
			249, 250, 251, 252, 253, 254, 259, 260, 901, 903,
			904, 905, 906, 907, 910, 908, 909, 911, 912, 913,
			914, 915, 916, 917, 953, 951, 950, 952, 954, 955,
			850, 851, 852, 853, 854, 855, -1
		};

		private static short[] TreasureFlag_157 = new short[1] { -1 };

		private static short[] PossessionItem_157 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_157 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_157 = new SEventJumpParameter("ゴールドルの館：ボス後", "d13_09", new float[3] { 0f, 0f, -10f }, new float[3] { 695f, 0f, -820f }, 3, const_cast<short[]>(PossessionItem_157), 10000, const_cast<SCharacterParameter[]>(Party_157), -1, const_cast<short[]>(GlobalFlag_157), const_cast<short[]>(TreasureFlag_157));

		private static short[] GlobalFlag_158 = new short[149]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
			169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
			189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
			218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
			249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
			901, 903, 904, 905, 906, 907, 910, 908, 909, 911,
			912, 913, 914, 915, 916, 917, 953, 951, 950, 952,
			954, 955, 850, 851, 852, 853, 854, 855, -1
		};

		private static short[] TreasureFlag_158 = new short[1] { -1 };

		private static short[] PossessionItem_158 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_158 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_158 = new SEventJumpParameter("アムル：ゴールドル撃破後", "t17_01", new float[3] { -5f, 0f, -170f }, new float[3] { 695f, 0f, -820f }, 3, const_cast<short[]>(PossessionItem_158), 10000, const_cast<SCharacterParameter[]>(Party_158), -1, const_cast<short[]>(GlobalFlag_158), const_cast<short[]>(TreasureFlag_158));

		private static short[] GlobalFlag_159 = new short[150]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
			169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
			189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
			218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
			249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
			901, 903, 904, 905, 906, 907, 910, 908, 909, 911,
			912, 913, 914, 915, 916, 917, 953, 951, 950, 952,
			954, 955, 850, 851, 852, 853, 854, 855, 856, -1
		};

		private static short[] TreasureFlag_159 = new short[1] { -1 };

		private static short[] PossessionItem_159 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_159 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_159 = new SEventJumpParameter("ワールド：エンタープライズ開放直前（座標未定）", "f03_AB", new float[3] { 715f, 0f, -830f }, new float[3] { 695f, 0f, -820f }, 3, const_cast<short[]>(PossessionItem_159), 10000, const_cast<SCharacterParameter[]>(Party_159), -1, const_cast<short[]>(GlobalFlag_159), const_cast<short[]>(TreasureFlag_159));

		private static short[] GlobalFlag_160 = new short[152]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
			169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
			189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
			218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
			249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
			265, 901, 903, 904, 905, 906, 907, 910, 908, 909,
			911, 912, 913, 914, 915, 916, 917, 953, 951, 950,
			952, 954, 955, 956, 850, 851, 852, 853, 854, 855,
			856, -1
		};

		private static short[] TreasureFlag_160 = new short[1] { -1 };

		private static short[] PossessionItem_160 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_160 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_160 = new SEventJumpParameter("ワールド：エンタープライズ開放後（座標未定）", "f03_AB", new float[3] { 715f, 0f, -830f }, new float[3] { 695f, 0f, -820f }, 3, const_cast<short[]>(PossessionItem_160), 10000, const_cast<SCharacterParameter[]>(Party_160), -1, const_cast<short[]>(GlobalFlag_160), const_cast<short[]>(TreasureFlag_160));

		private static short[] GlobalFlag_161 = new short[152]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
			169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
			189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
			218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
			249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
			265, 901, 903, 904, 905, 906, 907, 910, 908, 909,
			911, 912, 913, 914, 915, 916, 917, 953, 951, 950,
			952, 954, 955, 956, 850, 851, 852, 853, 854, 855,
			856, -1
		};

		private static short[] TreasureFlag_161 = new short[1] { -1 };

		private static short[] PossessionItem_161 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_161 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_161 = new SEventJumpParameter("アムル：エンタープライズ解放後", "t17_01", new float[3] { -5f, 0f, -170f }, new float[3] { 695f, 0f, -820f }, 3, const_cast<short[]>(PossessionItem_161), 10000, const_cast<SCharacterParameter[]>(Party_161), -1, const_cast<short[]>(GlobalFlag_161), const_cast<short[]>(TreasureFlag_161));

		private static short[] GlobalFlag_162 = new short[152]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
			169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
			189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
			218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
			249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
			265, 901, 903, 904, 905, 906, 907, 910, 908, 909,
			911, 912, 913, 914, 915, 916, 917, 953, 951, 950,
			952, 954, 955, 956, 850, 851, 852, 853, 854, 855,
			856, -1
		};

		private static short[] TreasureFlag_162 = new short[1] { -1 };

		private static short[] PossessionItem_162 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_162 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_162 = new SEventJumpParameter("ダスター：外観", "t18_01", new float[3] { -23f, 0f, -135f }, new float[3] { -98f, 0f, -41f }, 0, const_cast<short[]>(PossessionItem_162), 10000, const_cast<SCharacterParameter[]>(Party_162), -1, const_cast<short[]>(GlobalFlag_162), const_cast<short[]>(TreasureFlag_162));

		private static short[] GlobalFlag_163 = new short[152]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
			169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
			189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
			218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
			249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
			265, 901, 903, 904, 905, 906, 907, 910, 908, 909,
			911, 912, 913, 914, 915, 916, 917, 953, 951, 950,
			952, 954, 955, 956, 850, 851, 852, 853, 854, 855,
			856, -1
		};

		private static short[] TreasureFlag_163 = new short[1] { -1 };

		private static short[] PossessionItem_163 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_163 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_163 = new SEventJumpParameter("レプリト ：外観", "t19_01", new float[3] { -30f, 0f, -135f }, new float[3] { -953f, 0f, 976f }, 0, const_cast<short[]>(PossessionItem_163), 10000, const_cast<SCharacterParameter[]>(Party_163), -1, const_cast<short[]>(GlobalFlag_163), const_cast<short[]>(TreasureFlag_163));

		private static short[] GlobalFlag_164 = new short[153]
		{
			0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
			15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
			29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
			43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
			67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
			92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
			132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
			153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
			169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
			189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
			218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
			249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
			265, 901, 903, 904, 905, 906, 907, 910, 908, 909,
			911, 912, 913, 914, 915, 916, 917, 953, 951, 950,
			952, 954, 955, 956, 957, 850, 851, 852, 853, 854,
			855, 856, -1
		};

		private static short[] TreasureFlag_164 = new short[1] { -1 };

		private static short[] PossessionItem_164 = new short[4] { 4006, 5207, 4005, -1 };

		private static SCharacterParameter[] Party_164 = new SCharacterParameter[4]
		{
			new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			}),
			new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
			{
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 },
				{ -1, -1, -1 }
			})
		};

		private static SEventJumpParameter EventJumpParam_164;

		private static short[] GlobalFlag_165;

		private static short[] TreasureFlag_165;

		private static short[] PossessionItem_165;

		private static SCharacterParameter[] Party_165;

		private static SEventJumpParameter EventJumpParam_165;

		private static short[] GlobalFlag_166;

		private static short[] TreasureFlag_166;

		private static short[] PossessionItem_166;

		private static SCharacterParameter[] Party_166;

		private static SEventJumpParameter EventJumpParam_166;

		private static short[] GlobalFlag_167;

		private static short[] TreasureFlag_167;

		private static short[] PossessionItem_167;

		private static SCharacterParameter[] Party_167;

		private static SEventJumpParameter EventJumpParam_167;

		private static short[] GlobalFlag_168;

		private static short[] TreasureFlag_168;

		private static short[] PossessionItem_168;

		private static SCharacterParameter[] Party_168;

		private static SEventJumpParameter EventJumpParam_168;

		private static short[] GlobalFlag_169;

		private static short[] TreasureFlag_169;

		private static short[] PossessionItem_169;

		private static SCharacterParameter[] Party_169;

		private static SEventJumpParameter EventJumpParam_169;

		private static short[] GlobalFlag_170;

		private static short[] TreasureFlag_170;

		private static short[] PossessionItem_170;

		private static SCharacterParameter[] Party_170;

		private static SEventJumpParameter EventJumpParam_170;

		private static short[] GlobalFlag_171;

		private static short[] TreasureFlag_171;

		private static short[] PossessionItem_171;

		private static SCharacterParameter[] Party_171;

		private static SEventJumpParameter EventJumpParam_171;

		private static short[] GlobalFlag_172;

		private static short[] TreasureFlag_172;

		private static short[] PossessionItem_172;

		private static SCharacterParameter[] Party_172;

		private static SEventJumpParameter EventJumpParam_172;

		private static short[] GlobalFlag_173;

		private static short[] TreasureFlag_173;

		private static short[] PossessionItem_173;

		private static SCharacterParameter[] Party_173;

		private static SEventJumpParameter EventJumpParam_173;

		private static short[] GlobalFlag_174;

		private static short[] TreasureFlag_174;

		private static short[] PossessionItem_174;

		private static SCharacterParameter[] Party_174;

		private static SEventJumpParameter EventJumpParam_174;

		private static short[] GlobalFlag_175;

		private static short[] TreasureFlag_175;

		private static short[] PossessionItem_175;

		private static SCharacterParameter[] Party_175;

		private static SEventJumpParameter EventJumpParam_175;

		private static short[] GlobalFlag_176;

		private static short[] TreasureFlag_176;

		private static short[] PossessionItem_176;

		private static SCharacterParameter[] Party_176;

		private static SEventJumpParameter EventJumpParam_176;

		private static short[] GlobalFlag_177;

		private static short[] TreasureFlag_177;

		private static short[] PossessionItem_177;

		private static SCharacterParameter[] Party_177;

		private static SEventJumpParameter EventJumpParam_177;

		private static short[] GlobalFlag_178;

		private static short[] TreasureFlag_178;

		private static short[] PossessionItem_178;

		private static SCharacterParameter[] Party_178;

		private static SEventJumpParameter EventJumpParam_178;

		private static short[] GlobalFlag_179;

		private static short[] TreasureFlag_179;

		private static short[] PossessionItem_179;

		private static SCharacterParameter[] Party_179;

		private static SEventJumpParameter EventJumpParam_179;

		private static short[] GlobalFlag_180;

		private static short[] TreasureFlag_180;

		private static short[] PossessionItem_180;

		private static SCharacterParameter[] Party_180;

		private static SEventJumpParameter EventJumpParam_180;

		private static short[] GlobalFlag_181;

		private static short[] TreasureFlag_181;

		private static short[] PossessionItem_181;

		private static SCharacterParameter[] Party_181;

		private static SEventJumpParameter EventJumpParam_181;

		private static short[] GlobalFlag_182;

		private static short[] TreasureFlag_182;

		private static short[] PossessionItem_182;

		private static SCharacterParameter[] Party_182;

		private static SEventJumpParameter EventJumpParam_182;

		private static short[] GlobalFlag_183;

		private static short[] TreasureFlag_183;

		private static short[] PossessionItem_183;

		private static SCharacterParameter[] Party_183;

		private static SEventJumpParameter EventJumpParam_183;

		private static short[] GlobalFlag_184;

		private static short[] TreasureFlag_184;

		private static short[] PossessionItem_184;

		private static SCharacterParameter[] Party_184;

		private static SEventJumpParameter EventJumpParam_184;

		private static short[] GlobalFlag_185;

		private static short[] TreasureFlag_185;

		private static short[] PossessionItem_185;

		private static SCharacterParameter[] Party_185;

		private static SEventJumpParameter EventJumpParam_185;

		private static short[] GlobalFlag_186;

		private static short[] TreasureFlag_186;

		private static short[] PossessionItem_186;

		private static SCharacterParameter[] Party_186;

		private static SEventJumpParameter EventJumpParam_186;

		private static short[] GlobalFlag_187;

		private static short[] TreasureFlag_187;

		private static short[] PossessionItem_187;

		private static SCharacterParameter[] Party_187;

		private static SEventJumpParameter EventJumpParam_187;

		private static short[] GlobalFlag_188;

		private static short[] TreasureFlag_188;

		private static short[] PossessionItem_188;

		private static SCharacterParameter[] Party_188;

		private static SEventJumpParameter EventJumpParam_188;

		private static short[] GlobalFlag_189;

		private static short[] TreasureFlag_189;

		private static short[] PossessionItem_189;

		private static SCharacterParameter[] Party_189;

		private static SEventJumpParameter EventJumpParam_189;

		private static short[] GlobalFlag_190;

		private static short[] TreasureFlag_190;

		private static short[] PossessionItem_190;

		private static SCharacterParameter[] Party_190;

		private static SEventJumpParameter EventJumpParam_190;

		private static short[] GlobalFlag_191;

		private static short[] TreasureFlag_191;

		private static short[] PossessionItem_191;

		private static SCharacterParameter[] Party_191;

		private static SEventJumpParameter EventJumpParam_191;

		private static short[] GlobalFlag_192;

		private static short[] TreasureFlag_192;

		private static short[] PossessionItem_192;

		private static SCharacterParameter[] Party_192;

		private static SEventJumpParameter EventJumpParam_192;

		private static short[] GlobalFlag_193;

		private static short[] TreasureFlag_193;

		private static short[] PossessionItem_193;

		private static SCharacterParameter[] Party_193;

		private static SEventJumpParameter EventJumpParam_193;

		private static short[] GlobalFlag_194;

		private static short[] TreasureFlag_194;

		private static short[] PossessionItem_194;

		private static SCharacterParameter[] Party_194;

		private static SEventJumpParameter EventJumpParam_194;

		private static short[] GlobalFlag_195;

		private static short[] TreasureFlag_195;

		private static short[] PossessionItem_195;

		private static SCharacterParameter[] Party_195;

		private static SEventJumpParameter EventJumpParam_195;

		private static short[] GlobalFlag_196;

		private static short[] TreasureFlag_196;

		private static short[] PossessionItem_196;

		private static SCharacterParameter[] Party_196;

		private static SEventJumpParameter EventJumpParam_196;

		private static short[] GlobalFlag_197;

		private static short[] TreasureFlag_197;

		private static short[] PossessionItem_197;

		private static SCharacterParameter[] Party_197;

		private static SEventJumpParameter EventJumpParam_197;

		private static short[] GlobalFlag_198;

		private static short[] TreasureFlag_198;

		private static short[] PossessionItem_198;

		private static SCharacterParameter[] Party_198;

		private static SEventJumpParameter EventJumpParam_198;

		private static short[] GlobalFlag_199;

		private static short[] TreasureFlag_199;

		private static short[] PossessionItem_199;

		private static SCharacterParameter[] Party_199;

		private static SEventJumpParameter EventJumpParam_199;

		private static short[] GlobalFlag_200;

		private static short[] TreasureFlag_200;

		private static short[] PossessionItem_200;

		private static SCharacterParameter[] Party_200;

		private static SEventJumpParameter EventJumpParam_200;

		private static short[] GlobalFlag_201;

		private static short[] TreasureFlag_201;

		private static short[] PossessionItem_201;

		private static SCharacterParameter[] Party_201;

		private static SEventJumpParameter EventJumpParam_201;

		private static short[] GlobalFlag_202;

		private static short[] TreasureFlag_202;

		private static short[] PossessionItem_202;

		private static SCharacterParameter[] Party_202;

		private static SEventJumpParameter EventJumpParam_202;

		private static short[] GlobalFlag_203;

		private static short[] TreasureFlag_203;

		private static short[] PossessionItem_203;

		private static SCharacterParameter[] Party_203;

		private static SEventJumpParameter EventJumpParam_203;

		private static short[] GlobalFlag_204;

		private static short[] TreasureFlag_204;

		private static short[] PossessionItem_204;

		private static SCharacterParameter[] Party_204;

		private static SEventJumpParameter EventJumpParam_204;

		private static short[] GlobalFlag_205;

		private static short[] TreasureFlag_205;

		private static short[] PossessionItem_205;

		private static SCharacterParameter[] Party_205;

		private static SEventJumpParameter EventJumpParam_205;

		private static short[] GlobalFlag_206;

		private static short[] TreasureFlag_206;

		private static short[] PossessionItem_206;

		private static SCharacterParameter[] Party_206;

		private static SEventJumpParameter EventJumpParam_206;

		private static short[] GlobalFlag_207;

		private static short[] TreasureFlag_207;

		private static short[] PossessionItem_207;

		private static SCharacterParameter[] Party_207;

		private static SEventJumpParameter EventJumpParam_207;

		private static short[] GlobalFlag_208;

		private static short[] TreasureFlag_208;

		private static short[] PossessionItem_208;

		private static SCharacterParameter[] Party_208;

		private static SEventJumpParameter EventJumpParam_208;

		private static short[] GlobalFlag_209;

		private static short[] TreasureFlag_209;

		private static short[] PossessionItem_209;

		private static SCharacterParameter[] Party_209;

		private static SEventJumpParameter EventJumpParam_209;

		private static short[] GlobalFlag_210;

		private static short[] TreasureFlag_210;

		private static short[] PossessionItem_210;

		private static SCharacterParameter[] Party_210;

		private static SEventJumpParameter EventJumpParam_210;

		private static short[] GlobalFlag_211;

		private static short[] TreasureFlag_211;

		private static short[] PossessionItem_211;

		private static SCharacterParameter[] Party_211;

		private static SEventJumpParameter EventJumpParam_211;

		private static short[] GlobalFlag_212;

		private static short[] TreasureFlag_212;

		private static short[] PossessionItem_212;

		private static SCharacterParameter[] Party_212;

		private static SEventJumpParameter EventJumpParam_212;

		private static short[] GlobalFlag_213;

		private static short[] TreasureFlag_213;

		private static short[] PossessionItem_213;

		private static SCharacterParameter[] Party_213;

		private static SEventJumpParameter EventJumpParam_213;

		private static short[] GlobalFlag_214;

		private static short[] TreasureFlag_214;

		private static short[] PossessionItem_214;

		private static SCharacterParameter[] Party_214;

		private static SEventJumpParameter EventJumpParam_214;

		private static short[] GlobalFlag_215;

		private static short[] TreasureFlag_215;

		private static short[] PossessionItem_215;

		private static SCharacterParameter[] Party_215;

		private static SEventJumpParameter EventJumpParam_215;

		private static short[] GlobalFlag_216;

		private static short[] TreasureFlag_216;

		private static short[] PossessionItem_216;

		private static SCharacterParameter[] Party_216;

		private static SEventJumpParameter EventJumpParam_216;

		private static short[] GlobalFlag_217;

		private static short[] TreasureFlag_217;

		private static short[] PossessionItem_217;

		private static SCharacterParameter[] Party_217;

		private static SEventJumpParameter EventJumpParam_217;

		private static short[] GlobalFlag_218;

		private static short[] TreasureFlag_218;

		private static short[] PossessionItem_218;

		private static SCharacterParameter[] Party_218;

		private static SEventJumpParameter EventJumpParam_218;

		private static short[] GlobalFlag_219;

		private static short[] TreasureFlag_219;

		private static short[] PossessionItem_219;

		private static SCharacterParameter[] Party_219;

		private static SEventJumpParameter EventJumpParam_219;

		private static short[] GlobalFlag_220;

		private static short[] TreasureFlag_220;

		private static short[] PossessionItem_220;

		private static SCharacterParameter[] Party_220;

		private static SEventJumpParameter EventJumpParam_220;

		private static short[] GlobalFlag_221;

		private static short[] TreasureFlag_221;

		private static short[] PossessionItem_221;

		private static SCharacterParameter[] Party_221;

		private static SEventJumpParameter EventJumpParam_221;

		private static short[] GlobalFlag_222;

		private static short[] TreasureFlag_222;

		private static short[] PossessionItem_222;

		private static SCharacterParameter[] Party_222;

		private static SEventJumpParameter EventJumpParam_222;

		private static short[] GlobalFlag_223;

		private static short[] TreasureFlag_223;

		private static short[] PossessionItem_223;

		private static SCharacterParameter[] Party_223;

		private static SEventJumpParameter EventJumpParam_223;

		private static short[] GlobalFlag_224;

		private static short[] TreasureFlag_224;

		private static short[] PossessionItem_224;

		private static SCharacterParameter[] Party_224;

		private static SEventJumpParameter EventJumpParam_224;

		private static short[] GlobalFlag_225;

		private static short[] TreasureFlag_225;

		private static short[] PossessionItem_225;

		private static SCharacterParameter[] Party_225;

		private static SEventJumpParameter EventJumpParam_225;

		private static short[] GlobalFlag_226;

		private static short[] TreasureFlag_226;

		private static short[] PossessionItem_226;

		private static SCharacterParameter[] Party_226;

		private static SEventJumpParameter EventJumpParam_226;

		private static short[] GlobalFlag_227;

		private static short[] TreasureFlag_227;

		private static short[] PossessionItem_227;

		private static SCharacterParameter[] Party_227;

		private static SEventJumpParameter EventJumpParam_227;

		private static short[] GlobalFlag_228;

		private static short[] TreasureFlag_228;

		private static short[] PossessionItem_228;

		private static SCharacterParameter[] Party_228;

		private static SEventJumpParameter EventJumpParam_228;

		private static short[] GlobalFlag_229;

		private static short[] TreasureFlag_229;

		private static short[] PossessionItem_229;

		private static SCharacterParameter[] Party_229;

		private static SEventJumpParameter EventJumpParam_229;

		private static short[] GlobalFlag_230;

		private static short[] TreasureFlag_230;

		private static short[] PossessionItem_230;

		private static SCharacterParameter[] Party_230;

		private static SEventJumpParameter EventJumpParam_230;

		private static short[] GlobalFlag_231;

		private static short[] TreasureFlag_231;

		private static short[] PossessionItem_231;

		private static SCharacterParameter[] Party_231;

		private static SEventJumpParameter EventJumpParam_231;

		private static short[] GlobalFlag_232;

		private static short[] TreasureFlag_232;

		private static short[] PossessionItem_232;

		private static SCharacterParameter[] Party_232;

		private static SEventJumpParameter EventJumpParam_232;

		private static short[] GlobalFlag_233;

		private static short[] TreasureFlag_233;

		private static short[] PossessionItem_233;

		private static SCharacterParameter[] Party_233;

		private static SEventJumpParameter EventJumpParam_233;

		private static short[] GlobalFlag_234;

		private static short[] TreasureFlag_234;

		private static short[] PossessionItem_234;

		private static SCharacterParameter[] Party_234;

		private static SEventJumpParameter EventJumpParam_234;

		private static short[] GlobalFlag_235;

		private static short[] TreasureFlag_235;

		private static short[] PossessionItem_235;

		private static SCharacterParameter[] Party_235;

		private static SEventJumpParameter EventJumpParam_235;

		private static short[] GlobalFlag_236;

		private static short[] TreasureFlag_236;

		private static short[] PossessionItem_236;

		private static SCharacterParameter[] Party_236;

		private static SEventJumpParameter EventJumpParam_236;

		private static short[] GlobalFlag_237;

		private static short[] TreasureFlag_237;

		private static short[] PossessionItem_237;

		private static SCharacterParameter[] Party_237;

		private static SEventJumpParameter EventJumpParam_237;

		private static short[] GlobalFlag_238;

		private static short[] TreasureFlag_238;

		private static short[] PossessionItem_238;

		private static SCharacterParameter[] Party_238;

		private static SEventJumpParameter EventJumpParam_238;

		private static short[] GlobalFlag_239;

		private static short[] TreasureFlag_239;

		private static short[] PossessionItem_239;

		private static SCharacterParameter[] Party_239;

		private static SEventJumpParameter EventJumpParam_239;

		private static short[] GlobalFlag_240;

		private static short[] TreasureFlag_240;

		private static short[] PossessionItem_240;

		private static SCharacterParameter[] Party_240;

		private static SEventJumpParameter EventJumpParam_240;

		private static short[] GlobalFlag_241;

		private static short[] TreasureFlag_241;

		private static short[] PossessionItem_241;

		private static SCharacterParameter[] Party_241;

		private static SEventJumpParameter EventJumpParam_241;

		private static short[] GlobalFlag_242;

		private static short[] TreasureFlag_242;

		private static short[] PossessionItem_242;

		private static SCharacterParameter[] Party_242;

		private static SEventJumpParameter EventJumpParam_242;

		private static short[] GlobalFlag_243;

		private static short[] TreasureFlag_243;

		private static short[] PossessionItem_243;

		private static SCharacterParameter[] Party_243;

		private static SEventJumpParameter EventJumpParam_243;

		private static short[] GlobalFlag_244;

		private static short[] TreasureFlag_244;

		private static short[] PossessionItem_244;

		private static SCharacterParameter[] Party_244;

		private static SEventJumpParameter EventJumpParam_244;

		private static short[] GlobalFlag_245;

		private static short[] TreasureFlag_245;

		private static short[] PossessionItem_245;

		private static SCharacterParameter[] Party_245;

		private static SEventJumpParameter EventJumpParam_245;

		private static short[] GlobalFlag_246;

		private static short[] TreasureFlag_246;

		private static short[] PossessionItem_246;

		private static SCharacterParameter[] Party_246;

		private static SEventJumpParameter EventJumpParam_246;

		private static short[] GlobalFlag_247;

		private static short[] TreasureFlag_247;

		private static short[] PossessionItem_247;

		private static SCharacterParameter[] Party_247;

		private static SEventJumpParameter EventJumpParam_247;

		private static short[] GlobalFlag_248;

		private static short[] TreasureFlag_248;

		private static short[] PossessionItem_248;

		private static SCharacterParameter[] Party_248;

		private static SEventJumpParameter EventJumpParam_248;

		private static short[] GlobalFlag_249;

		private static short[] TreasureFlag_249;

		private static short[] PossessionItem_249;

		private static SCharacterParameter[] Party_249;

		private static SEventJumpParameter EventJumpParam_249;

		private static short[] GlobalFlag_250;

		private static short[] TreasureFlag_250;

		private static short[] PossessionItem_250;

		private static SCharacterParameter[] Party_250;

		private static SEventJumpParameter EventJumpParam_250;

		private static short[] GlobalFlag_251;

		private static short[] TreasureFlag_251;

		private static short[] PossessionItem_251;

		private static SCharacterParameter[] Party_251;

		private static SEventJumpParameter EventJumpParam_251;

		private static short[] GlobalFlag_252;

		private static short[] TreasureFlag_252;

		private static short[] PossessionItem_252;

		private static SCharacterParameter[] Party_252;

		private static SEventJumpParameter EventJumpParam_252;

		private static short[] GlobalFlag_253;

		private static short[] TreasureFlag_253;

		private static short[] PossessionItem_253;

		private static SCharacterParameter[] Party_253;

		private static SEventJumpParameter EventJumpParam_253;

		private static short[] GlobalFlag_254;

		private static short[] TreasureFlag_254;

		private static short[] PossessionItem_254;

		private static SCharacterParameter[] Party_254;

		private static SEventJumpParameter EventJumpParam_254;

		private static short[] GlobalFlag_255;

		private static short[] TreasureFlag_255;

		private static short[] PossessionItem_255;

		private static SCharacterParameter[] Party_255;

		private static SEventJumpParameter EventJumpParam_255;

		private static short[] GlobalFlag_256;

		private static short[] TreasureFlag_256;

		private static short[] PossessionItem_256;

		private static SCharacterParameter[] Party_256;

		private static SEventJumpParameter EventJumpParam_256;

		private static short[] GlobalFlag_257;

		private static short[] TreasureFlag_257;

		private static short[] PossessionItem_257;

		private static SCharacterParameter[] Party_257;

		private static SEventJumpParameter EventJumpParam_257;

		private static short[] GlobalFlag_258;

		private static short[] TreasureFlag_258;

		private static short[] PossessionItem_258;

		private static SCharacterParameter[] Party_258;

		private static SEventJumpParameter EventJumpParam_258;

		private static short[] GlobalFlag_259;

		private static short[] TreasureFlag_259;

		private static short[] PossessionItem_259;

		private static SCharacterParameter[] Party_259;

		private static SEventJumpParameter EventJumpParam_259;

		private static short[] GlobalFlag_260;

		private static short[] TreasureFlag_260;

		private static short[] PossessionItem_260;

		private static SCharacterParameter[] Party_260;

		private static SEventJumpParameter EventJumpParam_260;

		private static short[] GlobalFlag_261;

		private static short[] TreasureFlag_261;

		private static short[] PossessionItem_261;

		private static SCharacterParameter[] Party_261;

		private static SEventJumpParameter EventJumpParam_261;

		private static short[] GlobalFlag_262;

		private static short[] TreasureFlag_262;

		private static short[] PossessionItem_262;

		private static SCharacterParameter[] Party_262;

		private static SEventJumpParameter EventJumpParam_262;

		private static short[] GlobalFlag_263;

		private static short[] TreasureFlag_263;

		private static short[] PossessionItem_263;

		private static SCharacterParameter[] Party_263;

		private static SEventJumpParameter EventJumpParam_263;

		private static short[] GlobalFlag_264;

		private static short[] TreasureFlag_264;

		private static short[] PossessionItem_264;

		private static SCharacterParameter[] Party_264;

		private static SEventJumpParameter EventJumpParam_264;

		private static short[] GlobalFlag_265;

		private static short[] TreasureFlag_265;

		private static short[] PossessionItem_265;

		private static SCharacterParameter[] Party_265;

		private static SEventJumpParameter EventJumpParam_265;

		private static short[] GlobalFlag_266;

		private static short[] TreasureFlag_266;

		private static short[] PossessionItem_266;

		private static SCharacterParameter[] Party_266;

		private static SEventJumpParameter EventJumpParam_266;

		private static short[] GlobalFlag_267;

		private static short[] TreasureFlag_267;

		private static short[] PossessionItem_267;

		private static SCharacterParameter[] Party_267;

		private static SEventJumpParameter EventJumpParam_267;

		private static short[] GlobalFlag_268;

		private static short[] TreasureFlag_268;

		private static short[] PossessionItem_268;

		private static SCharacterParameter[] Party_268;

		private static SEventJumpParameter EventJumpParam_268;

		private static short[] GlobalFlag_269;

		private static short[] TreasureFlag_269;

		private static short[] PossessionItem_269;

		private static SCharacterParameter[] Party_269;

		private static SEventJumpParameter EventJumpParam_269;

		private static short[] GlobalFlag_270;

		private static short[] TreasureFlag_270;

		private static short[] PossessionItem_270;

		private static SCharacterParameter[] Party_270;

		private static SEventJumpParameter EventJumpParam_270;

		private static short[] GlobalFlag_271;

		private static short[] TreasureFlag_271;

		private static short[] PossessionItem_271;

		private static SCharacterParameter[] Party_271;

		private static SEventJumpParameter EventJumpParam_271;

		private static short[] GlobalFlag_272;

		private static short[] TreasureFlag_272;

		private static short[] PossessionItem_272;

		private static SCharacterParameter[] Party_272;

		private static SEventJumpParameter EventJumpParam_272;

		private static short[] GlobalFlag_273;

		private static short[] TreasureFlag_273;

		private static short[] PossessionItem_273;

		private static SCharacterParameter[] Party_273;

		private static SEventJumpParameter EventJumpParam_273;

		private static short[] GlobalFlag_274;

		private static short[] TreasureFlag_274;

		private static short[] PossessionItem_274;

		private static SCharacterParameter[] Party_274;

		private static SEventJumpParameter EventJumpParam_274;

		private static short[] GlobalFlag_275;

		private static short[] TreasureFlag_275;

		private static short[] PossessionItem_275;

		private static SCharacterParameter[] Party_275;

		private static SEventJumpParameter EventJumpParam_275;

		private static short[] GlobalFlag_276;

		private static short[] TreasureFlag_276;

		private static short[] PossessionItem_276;

		private static SCharacterParameter[] Party_276;

		private static SEventJumpParameter EventJumpParam_276;

		private static short[] GlobalFlag_277;

		private static short[] TreasureFlag_277;

		private static short[] PossessionItem_277;

		private static SCharacterParameter[] Party_277;

		private static SEventJumpParameter EventJumpParam_277;

		private static short[] GlobalFlag_278;

		private static short[] TreasureFlag_278;

		private static short[] PossessionItem_278;

		private static SCharacterParameter[] Party_278;

		private static SEventJumpParameter EventJumpParam_278;

		private static short[] GlobalFlag_279;

		private static short[] TreasureFlag_279;

		private static short[] PossessionItem_279;

		private static SCharacterParameter[] Party_279;

		private static SEventJumpParameter EventJumpParam_279;

		private static short[] GlobalFlag_280;

		private static short[] TreasureFlag_280;

		private static short[] PossessionItem_280;

		private static SCharacterParameter[] Party_280;

		private static SEventJumpParameter EventJumpParam_280;

		private static short[] GlobalFlag_281;

		private static short[] TreasureFlag_281;

		private static short[] PossessionItem_281;

		private static SCharacterParameter[] Party_281;

		private static SEventJumpParameter EventJumpParam_281;

		private static short[] GlobalFlag_282;

		private static short[] TreasureFlag_282;

		private static short[] PossessionItem_282;

		private static SCharacterParameter[] Party_282;

		private static SEventJumpParameter EventJumpParam_282;

		private static short[] GlobalFlag_283;

		private static short[] TreasureFlag_283;

		private static short[] PossessionItem_283;

		private static SCharacterParameter[] Party_283;

		private static SEventJumpParameter EventJumpParam_283;

		private static short[] GlobalFlag_284;

		private static short[] TreasureFlag_284;

		private static short[] PossessionItem_284;

		private static SCharacterParameter[] Party_284;

		private static SEventJumpParameter EventJumpParam_284;

		private static short[] GlobalFlag_285;

		private static short[] TreasureFlag_285;

		private static short[] PossessionItem_285;

		private static SCharacterParameter[] Party_285;

		private static SEventJumpParameter EventJumpParam_285;

		private static short[] GlobalFlag_286;

		private static short[] TreasureFlag_286;

		private static short[] PossessionItem_286;

		private static SCharacterParameter[] Party_286;

		private static SEventJumpParameter EventJumpParam_286;

		private static short[] GlobalFlag_287;

		private static short[] TreasureFlag_287;

		private static short[] PossessionItem_287;

		private static SCharacterParameter[] Party_287;

		private static SEventJumpParameter EventJumpParam_287;

		private static short[] GlobalFlag_288;

		private static short[] TreasureFlag_288;

		private static short[] PossessionItem_288;

		private static SCharacterParameter[] Party_288;

		private static SEventJumpParameter EventJumpParam_288;

		private static short[] GlobalFlag_289;

		private static short[] TreasureFlag_289;

		private static short[] PossessionItem_289;

		private static SCharacterParameter[] Party_289;

		private static SEventJumpParameter EventJumpParam_289;

		private static short[] GlobalFlag_290;

		private static short[] TreasureFlag_290;

		private static short[] PossessionItem_290;

		private static SCharacterParameter[] Party_290;

		private static SEventJumpParameter EventJumpParam_290;

		private static short[] GlobalFlag_291;

		private static short[] TreasureFlag_291;

		private static short[] PossessionItem_291;

		private static SCharacterParameter[] Party_291;

		private static SEventJumpParameter EventJumpParam_291;

		private static short[] GlobalFlag_292;

		private static short[] TreasureFlag_292;

		private static short[] PossessionItem_292;

		private static SCharacterParameter[] Party_292;

		private static SEventJumpParameter EventJumpParam_292;

		private static short[] GlobalFlag_293;

		private static short[] TreasureFlag_293;

		private static short[] PossessionItem_293;

		private static SCharacterParameter[] Party_293;

		private static SEventJumpParameter EventJumpParam_293;

		private static short[] GlobalFlag_294;

		private static short[] TreasureFlag_294;

		private static short[] PossessionItem_294;

		private static SCharacterParameter[] Party_294;

		private static SEventJumpParameter EventJumpParam_294;

		private static short[] GlobalFlag_295;

		private static short[] TreasureFlag_295;

		private static short[] PossessionItem_295;

		private static SCharacterParameter[] Party_295;

		private static SEventJumpParameter EventJumpParam_295;

		private static short[] GlobalFlag_296;

		private static short[] TreasureFlag_296;

		private static short[] PossessionItem_296;

		private static SCharacterParameter[] Party_296;

		private static SEventJumpParameter EventJumpParam_296;

		private static short[] GlobalFlag_297;

		private static short[] TreasureFlag_297;

		private static short[] PossessionItem_297;

		private static SCharacterParameter[] Party_297;

		private static SEventJumpParameter EventJumpParam_297;

		private static short[] GlobalFlag_298;

		private static short[] TreasureFlag_298;

		private static short[] PossessionItem_298;

		private static SCharacterParameter[] Party_298;

		private static SEventJumpParameter EventJumpParam_298;

		private static short[] GlobalFlag_299;

		private static short[] TreasureFlag_299;

		private static short[] PossessionItem_299;

		private static SCharacterParameter[] Party_299;

		private static SEventJumpParameter EventJumpParam_299;

		private static short[] GlobalFlag_300;

		private static short[] TreasureFlag_300;

		private static short[] PossessionItem_300;

		private static SCharacterParameter[] Party_300;

		private static SEventJumpParameter EventJumpParam_300;

		private static short[] GlobalFlag_301;

		private static short[] TreasureFlag_301;

		private static short[] PossessionItem_301;

		private static SCharacterParameter[] Party_301;

		private static SEventJumpParameter EventJumpParam_301;

		private static short[] GlobalFlag_302;

		private static short[] TreasureFlag_302;

		private static short[] PossessionItem_302;

		private static SCharacterParameter[] Party_302;

		private static SEventJumpParameter EventJumpParam_302;

		private static short[] GlobalFlag_303;

		private static short[] TreasureFlag_303;

		private static short[] PossessionItem_303;

		private static SCharacterParameter[] Party_303;

		private static SEventJumpParameter EventJumpParam_303;

		private static short[] GlobalFlag_304;

		private static short[] TreasureFlag_304;

		private static short[] PossessionItem_304;

		private static SCharacterParameter[] Party_304;

		private static SEventJumpParameter EventJumpParam_304;

		private static short[] GlobalFlag_305;

		private static short[] TreasureFlag_305;

		private static short[] PossessionItem_305;

		private static SCharacterParameter[] Party_305;

		private static SEventJumpParameter EventJumpParam_305;

		private static short[] GlobalFlag_306;

		private static short[] TreasureFlag_306;

		private static short[] PossessionItem_306;

		private static SCharacterParameter[] Party_306;

		private static SEventJumpParameter EventJumpParam_306;

		private static short[] GlobalFlag_307;

		private static short[] TreasureFlag_307;

		private static short[] PossessionItem_307;

		private static SCharacterParameter[] Party_307;

		private static SEventJumpParameter EventJumpParam_307;

		private static short[] GlobalFlag_308;

		private static short[] TreasureFlag_308;

		private static short[] PossessionItem_308;

		private static SCharacterParameter[] Party_308;

		private static SEventJumpParameter EventJumpParam_308;

		private static short[] GlobalFlag_309;

		private static short[] TreasureFlag_309;

		private static short[] PossessionItem_309;

		private static SCharacterParameter[] Party_309;

		private static SEventJumpParameter EventJumpParam_309;

		private static short[] GlobalFlag_310;

		private static short[] TreasureFlag_310;

		private static short[] PossessionItem_310;

		private static SCharacterParameter[] Party_310;

		private static SEventJumpParameter EventJumpParam_310;

		private static short[] GlobalFlag_311;

		private static short[] TreasureFlag_311;

		private static short[] PossessionItem_311;

		private static SCharacterParameter[] Party_311;

		private static SEventJumpParameter EventJumpParam_311;

		public static SEventJumpParameter[] EventJumpParamList;

		public static int[] EVENT_CHAPTER_FLAG;

		public static int[] EVENT_JOB_FLAG;

		public static int[] EVENT_VEHICLE_FLAG;

		static evt()
		{
			float[] arg = new float[3] { 0f, 0f, 10f };
			float[] arg2 = new float[3];
			EventJumpParam_164 = new SEventJumpParameter("サロニアにらみ合い：墜落", "t24_11", arg, arg2, 0, const_cast<short[]>(PossessionItem_164), 10000, const_cast<SCharacterParameter[]>(Party_164), -1, const_cast<short[]>(GlobalFlag_164), const_cast<short[]>(TreasureFlag_164));
			GlobalFlag_165 = new short[155]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 277, 901, 903, 904, 905, 906, 907, 910,
				908, 909, 911, 912, 913, 914, 915, 916, 917, 953,
				951, 950, 952, 954, 955, 956, 957, 850, 851, 852,
				853, 854, 855, 856, -1
			};
			TreasureFlag_165 = new short[1] { -1 };
			PossessionItem_165 = new short[4] { 4006, 5207, 4005, -1 };
			Party_165 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg3 = new float[3] { 0f, 0f, 10f };
			arg2 = new float[3];
			EventJumpParam_165 = new SEventJumpParameter("サロニアにらみ合い：墜落後", "t24_11", arg3, arg2, 0, const_cast<short[]>(PossessionItem_165), 10000, const_cast<SCharacterParameter[]>(Party_165), -1, const_cast<short[]>(GlobalFlag_165), const_cast<short[]>(TreasureFlag_165));
			GlobalFlag_166 = new short[155]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 277, 901, 903, 904, 905, 906, 907, 910,
				908, 909, 911, 912, 913, 914, 915, 916, 917, 953,
				951, 950, 952, 954, 955, 956, 957, 850, 851, 852,
				853, 854, 855, 856, -1
			};
			TreasureFlag_166 = new short[1] { -1 };
			PossessionItem_166 = new short[4] { 4006, 5207, 4005, -1 };
			Party_166 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg4 = new float[3] { -18f, 0f, -225f };
			arg2 = new float[3];
			EventJumpParam_166 = new SEventJumpParameter("サロニア 南西：外観", "t20_01", arg4, arg2, 0, const_cast<short[]>(PossessionItem_166), 10000, const_cast<SCharacterParameter[]>(Party_166), -1, const_cast<short[]>(GlobalFlag_166), const_cast<short[]>(TreasureFlag_166));
			GlobalFlag_167 = new short[155]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 277, 901, 903, 904, 905, 906, 907, 910,
				908, 909, 911, 912, 913, 914, 915, 916, 917, 953,
				951, 950, 952, 954, 955, 956, 957, 850, 851, 852,
				853, 854, 855, 856, -1
			};
			TreasureFlag_167 = new short[1] { -1 };
			PossessionItem_167 = new short[4] { 4006, 5207, 4005, -1 };
			Party_167 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg5 = new float[3] { -6f, 0f, -220f };
			arg2 = new float[3];
			EventJumpParam_167 = new SEventJumpParameter("サロニア 南東：外観", "t21_01", arg5, arg2, 0, const_cast<short[]>(PossessionItem_167), 10000, const_cast<SCharacterParameter[]>(Party_167), -1, const_cast<short[]>(GlobalFlag_167), const_cast<short[]>(TreasureFlag_167));
			GlobalFlag_168 = new short[155]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 277, 901, 903, 904, 905, 906, 907, 910,
				908, 909, 911, 912, 913, 914, 915, 916, 917, 953,
				951, 950, 952, 954, 955, 956, 957, 850, 851, 852,
				853, 854, 855, 856, -1
			};
			TreasureFlag_168 = new short[1] { -1 };
			PossessionItem_168 = new short[4] { 4006, 5207, 4005, -1 };
			Party_168 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg6 = new float[3] { 1f, 0f, -210f };
			arg2 = new float[3];
			EventJumpParam_168 = new SEventJumpParameter("サロニア 北西：外観", "t22_01", arg6, arg2, 0, const_cast<short[]>(PossessionItem_168), 10000, const_cast<SCharacterParameter[]>(Party_168), -1, const_cast<short[]>(GlobalFlag_168), const_cast<short[]>(TreasureFlag_168));
			GlobalFlag_169 = new short[155]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 277, 901, 903, 904, 905, 906, 907, 910,
				908, 909, 911, 912, 913, 914, 915, 916, 917, 953,
				951, 950, 952, 954, 955, 956, 957, 850, 851, 852,
				853, 854, 855, 856, -1
			};
			TreasureFlag_169 = new short[1] { -1 };
			PossessionItem_169 = new short[4] { 4006, 5207, 4005, -1 };
			Party_169 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg7 = new float[3] { 0f, 0f, -180f };
			arg2 = new float[3];
			EventJumpParam_169 = new SEventJumpParameter("サロニア 北東：外観", "t23_01", arg7, arg2, 0, const_cast<short[]>(PossessionItem_169), 10000, const_cast<SCharacterParameter[]>(Party_169), -1, const_cast<short[]>(GlobalFlag_169), const_cast<short[]>(TreasureFlag_169));
			GlobalFlag_170 = new short[155]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 277, 901, 903, 904, 905, 906, 907, 910,
				908, 909, 911, 912, 913, 914, 915, 916, 917, 953,
				951, 950, 952, 954, 955, 956, 957, 850, 851, 852,
				853, 854, 855, 856, -1
			};
			TreasureFlag_170 = new short[1] { -1 };
			PossessionItem_170 = new short[4] { 4006, 5207, 4005, -1 };
			Party_170 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg8 = new float[3] { 25f, 0f, -150f };
			arg2 = new float[3];
			EventJumpParam_170 = new SEventJumpParameter("サロニア城：外観", "t24_01", arg8, arg2, 0, const_cast<short[]>(PossessionItem_170), 10000, const_cast<SCharacterParameter[]>(Party_170), -1, const_cast<short[]>(GlobalFlag_170), const_cast<short[]>(TreasureFlag_170));
			GlobalFlag_171 = new short[155]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 277, 901, 903, 904, 905, 906, 907, 910,
				908, 909, 911, 912, 913, 914, 915, 916, 917, 953,
				951, 950, 952, 954, 955, 956, 957, 850, 851, 852,
				853, 854, 855, 856, -1
			};
			TreasureFlag_171 = new short[1] { -1 };
			PossessionItem_171 = new short[4] { 4006, 5207, 4005, -1 };
			Party_171 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg9 = new float[3] { -3f, 0f, -15f };
			arg2 = new float[3];
			EventJumpParam_171 = new SEventJumpParameter("サロニア 南西：酒場", "t20_05", arg9, arg2, 0, const_cast<short[]>(PossessionItem_171), 10000, const_cast<SCharacterParameter[]>(Party_171), -1, const_cast<short[]>(GlobalFlag_171), const_cast<short[]>(TreasureFlag_171));
			GlobalFlag_172 = new short[156]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 277, 280, 901, 903, 904, 905, 906, 907,
				910, 908, 909, 911, 912, 913, 914, 915, 916, 917,
				953, 951, 950, 952, 954, 955, 956, 957, 850, 851,
				852, 853, 854, 855, 856, -1
			};
			TreasureFlag_172 = new short[1] { -1 };
			PossessionItem_172 = new short[4] { 4006, 5207, 4005, -1 };
			Party_172 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg10 = new float[3] { -3f, 0f, -15f };
			arg2 = new float[3];
			EventJumpParam_172 = new SEventJumpParameter("サロニア 南西：酒場バトル後", "t20_05", arg10, arg2, 0, const_cast<short[]>(PossessionItem_172), 10000, const_cast<SCharacterParameter[]>(Party_172), -1, const_cast<short[]>(GlobalFlag_172), const_cast<short[]>(TreasureFlag_172));
			GlobalFlag_173 = new short[157]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 277, 280, 281, 901, 903, 904, 905, 906,
				907, 910, 908, 909, 911, 912, 913, 914, 915, 916,
				917, 953, 951, 950, 952, 954, 955, 956, 957, 850,
				851, 852, 853, 854, 855, 856, -1
			};
			TreasureFlag_173 = new short[1] { -1 };
			PossessionItem_173 = new short[4] { 4006, 5207, 4005, -1 };
			Party_173 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg11 = new float[3] { 50f, 0f, -40f };
			arg2 = new float[3];
			EventJumpParam_173 = new SEventJumpParameter("サロニア 南西：アルス合流後", "t20_01", arg11, arg2, 0, const_cast<short[]>(PossessionItem_173), 10000, const_cast<SCharacterParameter[]>(Party_173), 5, const_cast<short[]>(GlobalFlag_173), const_cast<short[]>(TreasureFlag_173));
			GlobalFlag_174 = new short[157]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 277, 280, 281, 901, 903, 904, 905, 906,
				907, 910, 908, 909, 911, 912, 913, 914, 915, 916,
				917, 953, 951, 950, 952, 954, 955, 956, 957, 850,
				851, 852, 853, 854, 855, 856, -1
			};
			TreasureFlag_174 = new short[1] { -1 };
			PossessionItem_174 = new short[4] { 4006, 5207, 4005, -1 };
			Party_174 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg12 = new float[3] { 0f, 0f, -100f };
			arg2 = new float[3];
			EventJumpParam_174 = new SEventJumpParameter("サロニア 南東：アルス合流後", "t21_01", arg12, arg2, 0, const_cast<short[]>(PossessionItem_174), 10000, const_cast<SCharacterParameter[]>(Party_174), 5, const_cast<short[]>(GlobalFlag_174), const_cast<short[]>(TreasureFlag_174));
			GlobalFlag_175 = new short[157]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 277, 280, 281, 901, 903, 904, 905, 906,
				907, 910, 908, 909, 911, 912, 913, 914, 915, 916,
				917, 953, 951, 950, 952, 954, 955, 956, 957, 850,
				851, 852, 853, 854, 855, 856, -1
			};
			TreasureFlag_175 = new short[1] { -1 };
			PossessionItem_175 = new short[4] { 4006, 5207, 4005, -1 };
			Party_175 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg13 = new float[3] { 0f, 0f, -10f };
			arg2 = new float[3];
			EventJumpParam_175 = new SEventJumpParameter("サロニア 北西：アルス合流後", "t22_01", arg13, arg2, 0, const_cast<short[]>(PossessionItem_175), 10000, const_cast<SCharacterParameter[]>(Party_175), 5, const_cast<short[]>(GlobalFlag_175), const_cast<short[]>(TreasureFlag_175));
			GlobalFlag_176 = new short[157]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 277, 280, 281, 901, 903, 904, 905, 906,
				907, 910, 908, 909, 911, 912, 913, 914, 915, 916,
				917, 953, 951, 950, 952, 954, 955, 956, 957, 850,
				851, 852, 853, 854, 855, 856, -1
			};
			TreasureFlag_176 = new short[1] { -1 };
			PossessionItem_176 = new short[4] { 4006, 5207, 4005, -1 };
			Party_176 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg14 = new float[3] { 0f, 0f, -100f };
			arg2 = new float[3];
			EventJumpParam_176 = new SEventJumpParameter("サロニア 北東：アルス合流後", "t23_01", arg14, arg2, 0, const_cast<short[]>(PossessionItem_176), 10000, const_cast<SCharacterParameter[]>(Party_176), 5, const_cast<short[]>(GlobalFlag_176), const_cast<short[]>(TreasureFlag_176));
			GlobalFlag_177 = new short[157]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 277, 280, 281, 901, 903, 904, 905, 906,
				907, 910, 908, 909, 911, 912, 913, 914, 915, 916,
				917, 953, 951, 950, 952, 954, 955, 956, 957, 850,
				851, 852, 853, 854, 855, 856, -1
			};
			TreasureFlag_177 = new short[1] { -1 };
			PossessionItem_177 = new short[4] { 4006, 5207, 4005, -1 };
			Party_177 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg15 = new float[3] { 0f, 0f, 10f };
			arg2 = new float[3];
			EventJumpParam_177 = new SEventJumpParameter("サロニア城：にらみ合いアルス合流後", "t24_11", arg15, arg2, 0, const_cast<short[]>(PossessionItem_177), 10000, const_cast<SCharacterParameter[]>(Party_177), 5, const_cast<short[]>(GlobalFlag_177), const_cast<short[]>(TreasureFlag_177));
			GlobalFlag_178 = new short[157]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 277, 280, 281, 901, 903, 904, 905, 906,
				907, 910, 908, 909, 911, 912, 913, 914, 915, 916,
				917, 953, 951, 950, 952, 954, 955, 956, 957, 850,
				851, 852, 853, 854, 855, 856, -1
			};
			TreasureFlag_178 = new short[1] { -1 };
			PossessionItem_178 = new short[4] { 4006, 5207, 4005, -1 };
			Party_178 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg16 = new float[3] { 25f, 0f, -150f };
			arg2 = new float[3];
			EventJumpParam_178 = new SEventJumpParameter("サロニア城：外観アルス合流後", "t24_01", arg16, arg2, 0, const_cast<short[]>(PossessionItem_178), 10000, const_cast<SCharacterParameter[]>(Party_178), 5, const_cast<short[]>(GlobalFlag_178), const_cast<short[]>(TreasureFlag_178));
			GlobalFlag_179 = new short[163]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 277, 280, 281, 282, 286, 283, 284, 287,
				288, 901, 903, 904, 905, 906, 907, 910, 908, 909,
				911, 912, 913, 914, 915, 916, 917, 953, 951, 950,
				952, 954, 955, 956, 957, 850, 851, 852, 853, 854,
				855, 856, -1
			};
			TreasureFlag_179 = new short[1] { -1 };
			PossessionItem_179 = new short[4] { 4006, 5207, 4005, -1 };
			Party_179 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg17 = new float[3] { 0f, 0f, -10f };
			arg2 = new float[3];
			EventJumpParam_179 = new SEventJumpParameter("サロニア城：ボス前", "t24_02", arg17, arg2, 0, const_cast<short[]>(PossessionItem_179), 10000, const_cast<SCharacterParameter[]>(Party_179), 5, const_cast<short[]>(GlobalFlag_179), const_cast<short[]>(TreasureFlag_179));
			GlobalFlag_180 = new short[164]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 277, 280, 281, 282, 286, 283, 284, 287,
				288, 290, 901, 903, 904, 905, 906, 907, 910, 908,
				909, 911, 912, 913, 914, 915, 916, 917, 953, 951,
				950, 952, 954, 955, 956, 957, 850, 851, 852, 853,
				854, 855, 856, -1
			};
			TreasureFlag_180 = new short[1] { -1 };
			PossessionItem_180 = new short[4] { 4006, 5207, 4005, -1 };
			Party_180 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg18 = new float[3] { 0f, 0f, -10f };
			arg2 = new float[3];
			EventJumpParam_180 = new SEventJumpParameter("サロニア城：ボス後", "t24_02", arg18, arg2, 0, const_cast<short[]>(PossessionItem_180), 10000, const_cast<SCharacterParameter[]>(Party_180), -1, const_cast<short[]>(GlobalFlag_180), const_cast<short[]>(TreasureFlag_180));
			GlobalFlag_181 = new short[165]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 277, 280, 281, 282, 286, 283, 284, 287,
				288, 290, 291, 901, 903, 904, 905, 906, 907, 910,
				908, 909, 911, 912, 913, 914, 915, 916, 917, 953,
				951, 950, 952, 954, 955, 956, 957, 850, 851, 852,
				853, 854, 855, 856, -1
			};
			TreasureFlag_181 = new short[1] { -1 };
			PossessionItem_181 = new short[4] { 4006, 5207, 4005, -1 };
			Party_181 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg19 = new float[3] { 0f, 0f, -10f };
			arg2 = new float[3];
			EventJumpParam_181 = new SEventJumpParameter("サロニア城：アルス王", "t24_04", arg19, arg2, 0, const_cast<short[]>(PossessionItem_181), 10000, const_cast<SCharacterParameter[]>(Party_181), -1, const_cast<short[]>(GlobalFlag_181), const_cast<short[]>(TreasureFlag_181));
			GlobalFlag_182 = new short[165]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 277, 280, 281, 282, 286, 283, 284, 287,
				288, 290, 291, 901, 903, 904, 905, 906, 907, 910,
				908, 909, 911, 912, 913, 914, 915, 916, 917, 953,
				951, 950, 952, 954, 955, 956, 957, 850, 851, 852,
				853, 854, 855, 856, -1
			};
			TreasureFlag_182 = new short[1] { -1 };
			PossessionItem_182 = new short[4] { 4006, 5207, 4005, -1 };
			Party_182 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg20 = new float[3] { 0f, 0f, -45f };
			arg2 = new float[3];
			EventJumpParam_182 = new SEventJumpParameter("サロニア城：技師の部屋", "t24_06", arg20, arg2, 0, const_cast<short[]>(PossessionItem_182), 10000, const_cast<SCharacterParameter[]>(Party_182), -1, const_cast<short[]>(GlobalFlag_182), const_cast<short[]>(TreasureFlag_182));
			GlobalFlag_183 = new short[169]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 277, 280, 281, 282, 286, 283, 284, 287,
				288, 290, 291, 293, 294, 901, 903, 904, 905, 906,
				907, 910, 908, 909, 911, 912, 913, 914, 915, 916,
				917, 953, 951, 950, 952, 954, 955, 956, 957, 958,
				850, 851, 852, 853, 854, 855, 856, 857, -1
			};
			TreasureFlag_183 = new short[1] { -1 };
			PossessionItem_183 = new short[4] { 4006, 5207, 4005, -1 };
			Party_183 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg21 = new float[3] { 25f, 0f, -150f };
			arg2 = new float[3];
			EventJumpParam_183 = new SEventJumpParameter("サロニア城：ノーチラス入手後", "t24_01", arg21, arg2, 0, const_cast<short[]>(PossessionItem_183), 10000, const_cast<SCharacterParameter[]>(Party_183), -1, const_cast<short[]>(GlobalFlag_183), const_cast<short[]>(TreasureFlag_183));
			GlobalFlag_184 = new short[168]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 901, 903, 904, 905, 906, 907,
				910, 908, 909, 911, 912, 913, 914, 915, 916, 917,
				953, 951, 950, 952, 954, 955, 956, 957, 958, 850,
				851, 852, 853, 854, 855, 856, 857, -1
			};
			TreasureFlag_184 = new short[1] { -1 };
			PossessionItem_184 = new short[4] { 4006, 5207, 4005, -1 };
			Party_184 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg22 = new float[3] { 50f, 0f, -40f };
			arg2 = new float[3];
			EventJumpParam_184 = new SEventJumpParameter("サロニア 南西：アルス王様後", "t20_01", arg22, arg2, 0, const_cast<short[]>(PossessionItem_184), 10000, const_cast<SCharacterParameter[]>(Party_184), -1, const_cast<short[]>(GlobalFlag_184), const_cast<short[]>(TreasureFlag_184));
			GlobalFlag_185 = new short[168]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 901, 903, 904, 905, 906, 907,
				910, 908, 909, 911, 912, 913, 914, 915, 916, 917,
				953, 951, 950, 952, 954, 955, 956, 957, 958, 850,
				851, 852, 853, 854, 855, 856, 857, -1
			};
			TreasureFlag_185 = new short[1] { -1 };
			PossessionItem_185 = new short[4] { 4006, 5207, 4005, -1 };
			Party_185 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg23 = new float[3] { 0f, 0f, -100f };
			arg2 = new float[3];
			EventJumpParam_185 = new SEventJumpParameter("サロニア 南東：アルス王様後", "t21_01", arg23, arg2, 0, const_cast<short[]>(PossessionItem_185), 10000, const_cast<SCharacterParameter[]>(Party_185), -1, const_cast<short[]>(GlobalFlag_185), const_cast<short[]>(TreasureFlag_185));
			GlobalFlag_186 = new short[168]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 901, 903, 904, 905, 906, 907,
				910, 908, 909, 911, 912, 913, 914, 915, 916, 917,
				953, 951, 950, 952, 954, 955, 956, 957, 958, 850,
				851, 852, 853, 854, 855, 856, 857, -1
			};
			TreasureFlag_186 = new short[1] { -1 };
			PossessionItem_186 = new short[4] { 4006, 5207, 4005, -1 };
			Party_186 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg24 = new float[3] { 0f, 0f, -10f };
			arg2 = new float[3];
			EventJumpParam_186 = new SEventJumpParameter("サロニア 北西：アルス王様後", "t22_01", arg24, arg2, 0, const_cast<short[]>(PossessionItem_186), 10000, const_cast<SCharacterParameter[]>(Party_186), -1, const_cast<short[]>(GlobalFlag_186), const_cast<short[]>(TreasureFlag_186));
			GlobalFlag_187 = new short[168]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 901, 903, 904, 905, 906, 907,
				910, 908, 909, 911, 912, 913, 914, 915, 916, 917,
				953, 951, 950, 952, 954, 955, 956, 957, 958, 850,
				851, 852, 853, 854, 855, 856, 857, -1
			};
			TreasureFlag_187 = new short[1] { -1 };
			PossessionItem_187 = new short[4] { 4006, 5207, 4005, -1 };
			Party_187 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg25 = new float[3] { 0f, 0f, -100f };
			arg2 = new float[3];
			EventJumpParam_187 = new SEventJumpParameter("サロニア 北東：アルス王様後", "t23_01", arg25, arg2, 0, const_cast<short[]>(PossessionItem_187), 10000, const_cast<SCharacterParameter[]>(Party_187), -1, const_cast<short[]>(GlobalFlag_187), const_cast<short[]>(TreasureFlag_187));
			GlobalFlag_188 = new short[168]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 901, 903, 904, 905, 906, 907,
				910, 908, 909, 911, 912, 913, 914, 915, 916, 917,
				953, 951, 950, 952, 954, 955, 956, 957, 958, 850,
				851, 852, 853, 854, 855, 856, 857, -1
			};
			TreasureFlag_188 = new short[1] { -1 };
			PossessionItem_188 = new short[4] { 4006, 5207, 4005, -1 };
			Party_188 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg26 = new float[3] { -468f, 0f, 538f };
			arg2 = new float[3];
			EventJumpParam_188 = new SEventJumpParameter("ワールド：サロニアクリア後（座標未定）", "f03_54", arg26, arg2, 0, const_cast<short[]>(PossessionItem_188), 10000, const_cast<SCharacterParameter[]>(Party_188), -1, const_cast<short[]>(GlobalFlag_188), const_cast<short[]>(TreasureFlag_188));
			GlobalFlag_189 = new short[168]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 901, 903, 904, 905, 906, 907,
				910, 908, 909, 911, 912, 913, 914, 915, 916, 917,
				953, 951, 950, 952, 954, 955, 956, 957, 958, 850,
				851, 852, 853, 854, 855, 856, 857, -1
			};
			TreasureFlag_189 = new short[1] { -1 };
			PossessionItem_189 = new short[4] { 4006, 5207, 4005, -1 };
			Party_189 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg27 = new float[3] { -287f, 0f, -1332f };
			arg2 = new float[3];
			EventJumpParam_189 = new SEventJumpParameter("ワールド：逆風付近（座標未定）", "f03_6E", arg27, arg2, 0, const_cast<short[]>(PossessionItem_189), 10000, const_cast<SCharacterParameter[]>(Party_189), -1, const_cast<short[]>(GlobalFlag_189), const_cast<short[]>(TreasureFlag_189));
			GlobalFlag_190 = new short[169]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 901, 903, 904, 905, 906,
				907, 910, 908, 909, 911, 912, 913, 914, 915, 916,
				917, 953, 951, 950, 952, 954, 955, 956, 957, 958,
				850, 851, 852, 853, 854, 855, 856, 857, -1
			};
			TreasureFlag_190 = new short[1] { -1 };
			PossessionItem_190 = new short[4] { 4006, 5207, 4005, -1 };
			Party_190 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg28 = new float[3] { 35f, 0f, 110f };
			arg2 = new float[3];
			EventJumpParam_190 = new SEventJumpParameter("ドーガの館：入り口", "t25_01", arg28, arg2, 0, const_cast<short[]>(PossessionItem_190), 10000, const_cast<SCharacterParameter[]>(Party_190), -1, const_cast<short[]>(GlobalFlag_190), const_cast<short[]>(TreasureFlag_190));
			GlobalFlag_191 = new short[170]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 901, 903, 904, 905,
				906, 907, 910, 908, 909, 911, 912, 913, 914, 915,
				916, 917, 953, 951, 950, 952, 954, 955, 956, 957,
				958, 850, 851, 852, 853, 854, 855, 856, 857, -1
			};
			TreasureFlag_191 = new short[1] { -1 };
			PossessionItem_191 = new short[4] { 4006, 5207, 4005, -1 };
			Party_191 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg29 = new float[3] { 35f, 0f, 110f };
			arg2 = new float[3];
			EventJumpParam_191 = new SEventJumpParameter("ドーガの館：ドーガ仲間後", "t25_01", arg29, arg2, 0, const_cast<short[]>(PossessionItem_191), 10000, const_cast<SCharacterParameter[]>(Party_191), 6, const_cast<short[]>(GlobalFlag_191), const_cast<short[]>(TreasureFlag_191));
			GlobalFlag_192 = new short[171]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 901, 903, 904,
				905, 906, 907, 910, 908, 909, 911, 912, 913, 914,
				915, 916, 917, 953, 951, 950, 952, 954, 955, 956,
				957, 958, 850, 851, 852, 853, 854, 855, 856, 857,
				-1
			};
			TreasureFlag_192 = new short[1] { -1 };
			PossessionItem_192 = new short[4] { 4006, 5207, 4005, -1 };
			Party_192 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg30 = new float[3] { 128f, 12f, 32f };
			arg2 = new float[3];
			EventJumpParam_192 = new SEventJumpParameter("魔方陣の洞窟：Ｂ１", "d14_01", arg30, arg2, 0, const_cast<short[]>(PossessionItem_192), 10000, const_cast<SCharacterParameter[]>(Party_192), 6, const_cast<short[]>(GlobalFlag_192), const_cast<short[]>(TreasureFlag_192));
			GlobalFlag_193 = new short[171]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 901, 903, 904,
				905, 906, 907, 910, 908, 909, 911, 912, 913, 914,
				915, 916, 917, 953, 951, 950, 952, 954, 955, 956,
				957, 958, 850, 851, 852, 853, 854, 855, 856, 857,
				-1
			};
			TreasureFlag_193 = new short[1] { -1 };
			PossessionItem_193 = new short[4] { 4006, 5207, 4005, -1 };
			Party_193 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 30, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg31 = new float[3] { 0f, 12f, -65f };
			arg2 = new float[3];
			EventJumpParam_193 = new SEventJumpParameter("魔方陣の洞窟：魔方陣前", "d14_04", arg31, arg2, 0, const_cast<short[]>(PossessionItem_193), 10000, const_cast<SCharacterParameter[]>(Party_193), 6, const_cast<short[]>(GlobalFlag_193), const_cast<short[]>(TreasureFlag_193));
			GlobalFlag_194 = new short[173]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 309, 901, 903,
				904, 905, 906, 907, 910, 908, 909, 911, 912, 913,
				914, 915, 916, 917, 953, 951, 950, 952, 954, 955,
				956, 957, 958, 959, 850, 851, 852, 853, 854, 855,
				856, 857, -1
			};
			TreasureFlag_194 = new short[1] { -1 };
			PossessionItem_194 = new short[4] { 4006, 5207, 4005, -1 };
			Party_194 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg32 = new float[3] { 110f, 0f, -926f };
			arg2 = new float[3];
			EventJumpParam_194 = new SEventJumpParameter("ワールド：魔方陣の洞窟脱出後（座標未定）", "f03_8C", arg32, arg2, 0, const_cast<short[]>(PossessionItem_194), 10000, const_cast<SCharacterParameter[]>(Party_194), -1, const_cast<short[]>(GlobalFlag_194), const_cast<short[]>(TreasureFlag_194));
			GlobalFlag_195 = new short[174]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 309, 310, 901,
				903, 904, 905, 906, 907, 910, 908, 909, 911, 912,
				913, 914, 915, 916, 917, 953, 951, 950, 952, 954,
				955, 956, 957, 958, 959, 850, 851, 852, 853, 854,
				855, 856, 857, -1
			};
			TreasureFlag_195 = new short[1] { -1 };
			PossessionItem_195 = new short[4] { 4006, 5207, 4005, -1 };
			Party_195 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg33 = new float[3] { 35f, 0f, 110f };
			arg2 = new float[3];
			EventJumpParam_195 = new SEventJumpParameter("ドーガの館：魔方陣クリア後", "t25_01", arg33, arg2, 0, const_cast<short[]>(PossessionItem_195), 10000, const_cast<SCharacterParameter[]>(Party_195), -1, const_cast<short[]>(GlobalFlag_195), const_cast<short[]>(TreasureFlag_195));
			GlobalFlag_196 = new short[174]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 309, 310, 901,
				903, 904, 905, 906, 907, 910, 908, 909, 911, 912,
				913, 914, 915, 916, 917, 953, 951, 950, 952, 954,
				955, 956, 957, 958, 959, 850, 851, 852, 853, 854,
				855, 856, 857, -1
			};
			TreasureFlag_196 = new short[1] { -1 };
			PossessionItem_196 = new short[4] { 4006, 5207, 4005, -1 };
			Party_196 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg34 = new float[3] { 716f, 0f, -826f };
			arg2 = new float[3];
			EventJumpParam_196 = new SEventJumpParameter("ワールド：アムル付近（座標未定）", "f03_AB", arg34, arg2, 0, const_cast<short[]>(PossessionItem_196), 10000, const_cast<SCharacterParameter[]>(Party_196), -1, const_cast<short[]>(GlobalFlag_196), const_cast<short[]>(TreasureFlag_196));
			GlobalFlag_197 = new short[176]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				300, 901, 903, 904, 905, 906, 907, 910, 908, 909,
				911, 912, 913, 914, 915, 916, 917, 953, 951, 950,
				952, 954, 955, 956, 957, 958, 959, 850, 851, 852,
				853, 854, 855, 856, 857, -1
			};
			TreasureFlag_197 = new short[1] { -1 };
			PossessionItem_197 = new short[4] { 4006, 5207, 4005, -1 };
			Party_197 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg35 = new float[3] { 0f, 0f, 20f };
			arg2 = new float[3];
			EventJumpParam_197 = new SEventJumpParameter("ドーガの村：外観", "t26_01", arg35, arg2, 0, const_cast<short[]>(PossessionItem_197), 10000, const_cast<SCharacterParameter[]>(Party_197), -1, const_cast<short[]>(GlobalFlag_197), const_cast<short[]>(TreasureFlag_197));
			GlobalFlag_198 = new short[175]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				901, 903, 904, 905, 906, 907, 910, 908, 909, 911,
				912, 913, 914, 915, 916, 917, 953, 951, 950, 952,
				954, 955, 956, 957, 958, 959, 850, 851, 852, 853,
				854, 855, 856, 857, -1
			};
			TreasureFlag_198 = new short[1] { -1 };
			PossessionItem_198 = new short[4] { 4006, 5207, 4005, -1 };
			Party_198 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg36 = new float[3] { 108f, 0f, 55f };
			arg2 = new float[3];
			EventJumpParam_198 = new SEventJumpParameter("海底洞窟：Ｂ１", "d15_01", arg36, arg2, 0, const_cast<short[]>(PossessionItem_198), 10000, const_cast<SCharacterParameter[]>(Party_198), -1, const_cast<short[]>(GlobalFlag_198), const_cast<short[]>(TreasureFlag_198));
			GlobalFlag_199 = new short[175]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				901, 903, 904, 905, 906, 907, 910, 908, 909, 911,
				912, 913, 914, 915, 916, 917, 953, 951, 950, 952,
				954, 955, 956, 957, 958, 959, 850, 851, 852, 853,
				854, 855, 856, 857, -1
			};
			TreasureFlag_199 = new short[1] { -1 };
			PossessionItem_199 = new short[4] { 4006, 5207, 4005, -1 };
			Party_199 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 32, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 32, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 32, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 32, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg37 = new float[3] { 0f, 0f, -10f };
			arg2 = new float[3];
			EventJumpParam_199 = new SEventJumpParameter("サロニア地下：１Ｆ", "d16_01", arg37, arg2, 0, const_cast<short[]>(PossessionItem_199), 10000, const_cast<SCharacterParameter[]>(Party_199), -1, const_cast<short[]>(GlobalFlag_199), const_cast<short[]>(TreasureFlag_199));
			GlobalFlag_200 = new short[175]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				901, 903, 904, 905, 906, 907, 910, 908, 909, 911,
				912, 913, 914, 915, 916, 917, 953, 951, 950, 952,
				954, 955, 956, 957, 958, 959, 850, 851, 852, 853,
				854, 855, 856, 857, -1
			};
			TreasureFlag_200 = new short[1] { -1 };
			PossessionItem_200 = new short[4] { 4006, 5207, 4005, -1 };
			Party_200 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 32, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 32, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 32, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 32, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg38 = new float[3] { 0f, 0f, -5f };
			arg2 = new float[3];
			EventJumpParam_200 = new SEventJumpParameter("サロニア地下：5Ｆ", "d16_05", arg38, arg2, 0, const_cast<short[]>(PossessionItem_200), 10000, const_cast<SCharacterParameter[]>(Party_200), -1, const_cast<short[]>(GlobalFlag_200), const_cast<short[]>(TreasureFlag_200));
			GlobalFlag_201 = new short[176]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				901, 903, 904, 905, 906, 907, 910, 908, 909, 911,
				912, 913, 914, 915, 916, 917, 953, 951, 950, 952,
				954, 955, 956, 957, 958, 959, 850, 851, 852, 853,
				854, 855, 856, 857, 971, -1
			};
			TreasureFlag_201 = new short[1] { -1 };
			PossessionItem_201 = new short[4] { 4006, 5207, 4005, -1 };
			Party_201 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg39 = new float[3] { 0f, 0f, -5f };
			arg2 = new float[3];
			EventJumpParam_201 = new SEventJumpParameter("サロニア地下：5Ｆオーディーン戦後", "d16_05", arg39, arg2, 0, const_cast<short[]>(PossessionItem_201), 10000, const_cast<SCharacterParameter[]>(Party_201), -1, const_cast<short[]>(GlobalFlag_201), const_cast<short[]>(TreasureFlag_201));
			GlobalFlag_202 = new short[175]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				901, 903, 904, 905, 906, 907, 910, 908, 909, 911,
				912, 913, 914, 915, 916, 917, 953, 951, 950, 952,
				954, 955, 956, 957, 958, 959, 850, 851, 852, 853,
				854, 855, 856, 857, -1
			};
			TreasureFlag_202 = new short[1] { -1 };
			PossessionItem_202 = new short[4] { 4006, 5207, 4005, -1 };
			Party_202 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 32, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 32, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 32, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 32, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg40 = new float[3] { 252f, 0f, 105f };
			arg2 = new float[3];
			EventJumpParam_202 = new SEventJumpParameter("時の神殿：Ｂ１", "d17_01", arg40, arg2, 0, const_cast<short[]>(PossessionItem_202), 10000, const_cast<SCharacterParameter[]>(Party_202), -1, const_cast<short[]>(GlobalFlag_202), const_cast<short[]>(TreasureFlag_202));
			GlobalFlag_203 = new short[178]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 901, 903, 904, 905, 906, 907, 910,
				908, 909, 911, 912, 913, 914, 915, 916, 917, 953,
				951, 950, 952, 954, 955, 956, 957, 958, 959, 850,
				851, 852, 853, 854, 855, 856, 857, -1
			};
			TreasureFlag_203 = new short[1] { -1 };
			PossessionItem_203 = new short[4] { 4006, 5207, 4005, -1 };
			Party_203 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 32, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 32, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 32, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 32, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg41 = new float[3] { 6f, 0f, -15f };
			arg2 = new float[3];
			EventJumpParam_203 = new SEventJumpParameter("時の神殿：リュート前", "d17_11", arg41, arg2, 0, const_cast<short[]>(PossessionItem_203), 10000, const_cast<SCharacterParameter[]>(Party_203), -1, const_cast<short[]>(GlobalFlag_203), const_cast<short[]>(TreasureFlag_203));
			GlobalFlag_204 = new short[179]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 901, 903, 904, 905, 906, 907,
				910, 908, 909, 911, 912, 913, 914, 915, 916, 917,
				953, 951, 950, 952, 954, 955, 956, 957, 958, 959,
				850, 851, 852, 853, 854, 855, 856, 857, -1
			};
			TreasureFlag_204 = new short[1] { -1 };
			PossessionItem_204 = new short[4] { 4006, 5207, 4005, -1 };
			Party_204 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg42 = new float[3] { -5f, 0f, -170f };
			arg2 = new float[3];
			EventJumpParam_204 = new SEventJumpParameter("アムル：リュート入手後", "t17_01", arg42, arg2, 0, const_cast<short[]>(PossessionItem_204), 10000, const_cast<SCharacterParameter[]>(Party_204), -1, const_cast<short[]>(GlobalFlag_204), const_cast<short[]>(TreasureFlag_204));
			GlobalFlag_205 = new short[178]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 901, 903, 904, 905, 906, 907, 910,
				908, 909, 911, 912, 913, 914, 915, 916, 917, 953,
				951, 950, 952, 954, 955, 956, 957, 958, 959, 850,
				851, 852, 853, 854, 855, 856, 857, -1
			};
			TreasureFlag_205 = new short[1] { -1 };
			PossessionItem_205 = new short[4] { 4006, 5207, 4005, -1 };
			Party_205 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg43 = new float[3] { 0f, 0f, 50f };
			arg2 = new float[3];
			EventJumpParam_205 = new SEventJumpParameter("ウネのほこら：リュート入手前", "t27_01", arg43, arg2, 0, const_cast<short[]>(PossessionItem_205), 10000, const_cast<SCharacterParameter[]>(Party_205), -1, const_cast<short[]>(GlobalFlag_205), const_cast<short[]>(TreasureFlag_205));
			GlobalFlag_206 = new short[179]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 901, 903, 904, 905, 906, 907,
				910, 908, 909, 911, 912, 913, 914, 915, 916, 917,
				953, 951, 950, 952, 954, 955, 956, 957, 958, 959,
				850, 851, 852, 853, 854, 855, 856, 857, -1
			};
			TreasureFlag_206 = new short[1] { -1 };
			PossessionItem_206 = new short[5] { 4006, 5207, 4005, 5204, -1 };
			Party_206 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg44 = new float[3] { 0f, 0f, 50f };
			arg2 = new float[3];
			EventJumpParam_206 = new SEventJumpParameter("ウネのほこら：リュート入手後", "t27_01", arg44, arg2, 0, const_cast<short[]>(PossessionItem_206), 10000, const_cast<SCharacterParameter[]>(Party_206), -1, const_cast<short[]>(GlobalFlag_206), const_cast<short[]>(TreasureFlag_206));
			GlobalFlag_207 = new short[183]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 901, 903,
				904, 905, 906, 907, 910, 908, 909, 911, 912, 913,
				914, 915, 916, 917, 953, 951, 950, 952, 954, 955,
				956, 957, 958, 959, 850, 851, 852, 853, 854, 855,
				856, 857, -1
			};
			TreasureFlag_207 = new short[1] { -1 };
			PossessionItem_207 = new short[5] { 4006, 5207, 4005, 5204, -1 };
			Party_207 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg45 = new float[3] { 0f, 0f, 50f };
			arg2 = new float[3];
			EventJumpParam_207 = new SEventJumpParameter("ウネのほこら：ウネ仲間後", "t27_01", arg45, arg2, 0, const_cast<short[]>(PossessionItem_207), 10000, const_cast<SCharacterParameter[]>(Party_207), 7, const_cast<short[]>(GlobalFlag_207), const_cast<short[]>(TreasureFlag_207));
			GlobalFlag_208 = new short[183]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 901, 903,
				904, 905, 906, 907, 910, 908, 909, 911, 912, 913,
				914, 915, 916, 917, 953, 951, 950, 952, 954, 955,
				956, 957, 958, 959, 850, 851, 852, 853, 854, 855,
				856, 857, -1
			};
			TreasureFlag_208 = new short[1] { -1 };
			PossessionItem_208 = new short[5] { 4006, 5207, 4005, 5204, -1 };
			Party_208 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg46 = new float[3] { 716f, 0f, -826f };
			arg2 = new float[3];
			EventJumpParam_208 = new SEventJumpParameter("ワールド：アムル付近（座標未定）", "f03_AB", arg46, arg2, 0, const_cast<short[]>(PossessionItem_208), 10000, const_cast<SCharacterParameter[]>(Party_208), 7, const_cast<short[]>(GlobalFlag_208), const_cast<short[]>(TreasureFlag_208));
			GlobalFlag_209 = new short[179]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 901, 903, 904, 905, 906, 907,
				910, 908, 909, 911, 912, 913, 914, 915, 916, 917,
				953, 951, 950, 952, 954, 955, 956, 957, 958, 959,
				850, 851, 852, 853, 854, 855, 856, 857, -1
			};
			TreasureFlag_209 = new short[1] { -1 };
			PossessionItem_209 = new short[5] { 4006, 5207, 4005, 5204, -1 };
			Party_209 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 35, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 35, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 35, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 35, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg47 = new float[3] { 45f, 0f, 17f };
			arg2 = new float[3];
			EventJumpParam_209 = new SEventJumpParameter("古代遺跡：ウネ仲間前", "d18_01", arg47, arg2, 0, const_cast<short[]>(PossessionItem_209), 10000, const_cast<SCharacterParameter[]>(Party_209), -1, const_cast<short[]>(GlobalFlag_209), const_cast<short[]>(TreasureFlag_209));
			GlobalFlag_210 = new short[183]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 901, 903,
				904, 905, 906, 907, 910, 908, 909, 911, 912, 913,
				914, 915, 916, 917, 953, 951, 950, 952, 954, 955,
				956, 957, 958, 959, 850, 851, 852, 853, 854, 855,
				856, 857, -1
			};
			TreasureFlag_210 = new short[1] { -1 };
			PossessionItem_210 = new short[5] { 4006, 5207, 4005, 5204, -1 };
			Party_210 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 35, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 35, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 35, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 35, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg48 = new float[3] { 45f, 0f, 17f };
			arg2 = new float[3];
			EventJumpParam_210 = new SEventJumpParameter("古代遺跡：ウネ仲間後", "d18_01", arg48, arg2, 0, const_cast<short[]>(PossessionItem_210), 10000, const_cast<SCharacterParameter[]>(Party_210), 7, const_cast<short[]>(GlobalFlag_210), const_cast<short[]>(TreasureFlag_210));
			GlobalFlag_211 = new short[189]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 901, 903, 904, 905, 906, 907, 910,
				908, 909, 911, 912, 913, 914, 915, 916, 917, 953,
				951, 950, 952, 954, 955, 956, 957, 958, 959, 960,
				850, 851, 852, 853, 854, 855, 856, 857, -1
			};
			TreasureFlag_211 = new short[1] { -1 };
			PossessionItem_211 = new short[5] { 4006, 5207, 4005, 5204, -1 };
			Party_211 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg49 = new float[3] { 80f, 0f, 0f };
			arg2 = new float[3];
			EventJumpParam_211 = new SEventJumpParameter("インビンシブル：初到着", "t28_01", arg49, arg2, 0, const_cast<short[]>(PossessionItem_211), 10000, const_cast<SCharacterParameter[]>(Party_211), 7, const_cast<short[]>(GlobalFlag_211), const_cast<short[]>(TreasureFlag_211));
			GlobalFlag_212 = new short[190]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 901, 903, 904, 905, 906, 907,
				910, 908, 909, 911, 912, 913, 914, 915, 916, 917,
				953, 951, 950, 952, 954, 955, 956, 957, 958, 959,
				960, 850, 851, 852, 853, 854, 855, 856, 857, -1
			};
			TreasureFlag_212 = new short[1] { -1 };
			PossessionItem_212 = new short[5] { 4006, 5207, 4005, 5204, -1 };
			Party_212 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg50 = new float[3] { -5f, 0f, -170f };
			arg2 = new float[3];
			EventJumpParam_212 = new SEventJumpParameter("ワールド：インビンシブル浮上（座標未定）", "f03_01", arg50, arg2, 0, const_cast<short[]>(PossessionItem_212), 10000, const_cast<SCharacterParameter[]>(Party_212), 7, const_cast<short[]>(GlobalFlag_212), const_cast<short[]>(TreasureFlag_212));
			GlobalFlag_213 = new short[191]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 901, 903, 904, 905, 906,
				907, 910, 908, 909, 911, 912, 913, 914, 915, 916,
				917, 953, 951, 950, 952, 954, 955, 956, 957, 958,
				959, 960, 850, 851, 852, 853, 854, 855, 856, 857,
				-1
			};
			TreasureFlag_213 = new short[1] { -1 };
			PossessionItem_213 = new short[5] { 4006, 5207, 4005, 5204, -1 };
			Party_213 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg51 = new float[3] { 80f, 0f, 0f };
			arg2 = new float[3];
			EventJumpParam_213 = new SEventJumpParameter("インビンシブル：初到着", "t28_01", arg51, arg2, 0, const_cast<short[]>(PossessionItem_213), 10000, const_cast<SCharacterParameter[]>(Party_213), 7, const_cast<short[]>(GlobalFlag_213), const_cast<short[]>(TreasureFlag_213));
			GlobalFlag_214 = new short[192]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 901, 903, 904, 905,
				906, 907, 910, 908, 909, 911, 912, 913, 914, 915,
				916, 917, 953, 951, 950, 952, 954, 955, 956, 957,
				958, 959, 960, 850, 851, 852, 853, 854, 855, 856,
				857, -1
			};
			TreasureFlag_214 = new short[1] { -1 };
			PossessionItem_214 = new short[5] { 4006, 5207, 4005, 5204, -1 };
			Party_214 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg52 = new float[3] { -5f, 0f, -170f };
			arg2 = new float[3];
			EventJumpParam_214 = new SEventJumpParameter("ワールド：インビンシブル浮上（座標未定）", "f03_01", arg52, arg2, 0, const_cast<short[]>(PossessionItem_214), 10000, const_cast<SCharacterParameter[]>(Party_214), 7, const_cast<short[]>(GlobalFlag_214), const_cast<short[]>(TreasureFlag_214));
			GlobalFlag_215 = new short[193]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 901, 903, 904, 905,
				906, 907, 910, 908, 909, 911, 912, 913, 914, 915,
				916, 917, 953, 951, 950, 952, 954, 955, 956, 957,
				958, 959, 960, 850, 851, 852, 853, 854, 855, 856,
				857, 858, -1
			};
			TreasureFlag_215 = new short[1] { -1 };
			PossessionItem_215 = new short[5] { 4006, 5207, 4005, 5204, -1 };
			Party_215 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg53 = new float[3] { -5f, 0f, -170f };
			arg2 = new float[3];
			EventJumpParam_215 = new SEventJumpParameter("アムル：インビンシブル入手後", "t17_01", arg53, arg2, 0, const_cast<short[]>(PossessionItem_215), 10000, const_cast<SCharacterParameter[]>(Party_215), -1, const_cast<short[]>(GlobalFlag_215), const_cast<short[]>(TreasureFlag_215));
			GlobalFlag_216 = new short[193]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 901, 903, 904, 905,
				906, 907, 910, 908, 909, 911, 912, 913, 914, 915,
				916, 917, 953, 951, 950, 952, 954, 955, 956, 957,
				958, 959, 960, 850, 851, 852, 853, 854, 855, 856,
				857, 858, -1
			};
			TreasureFlag_216 = new short[1] { -1 };
			PossessionItem_216 = new short[5] { 4006, 5207, 4005, 5204, -1 };
			Party_216 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 34, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 34, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 34, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 34, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg54 = new float[3] { -373f, 0f, -12f };
			arg2 = new float[3];
			EventJumpParam_216 = new SEventJumpParameter("ワールド：ドールの湖付近（座標未定）", "f01_24", arg54, arg2, 0, const_cast<short[]>(PossessionItem_216), 10000, const_cast<SCharacterParameter[]>(Party_216), -1, const_cast<short[]>(GlobalFlag_216), const_cast<short[]>(TreasureFlag_216));
			GlobalFlag_217 = new short[193]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 901, 903, 904, 905,
				906, 907, 910, 908, 909, 911, 912, 913, 914, 915,
				916, 917, 953, 951, 950, 952, 954, 955, 956, 957,
				958, 959, 960, 850, 851, 852, 853, 854, 855, 856,
				857, 858, -1
			};
			TreasureFlag_217 = new short[1] { -1 };
			PossessionItem_217 = new short[5] { 4006, 5207, 4005, 5204, -1 };
			Party_217 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 34, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 34, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 34, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 34, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg55 = new float[3] { -100f, 0f, 100f };
			arg2 = new float[3];
			EventJumpParam_217 = new SEventJumpParameter("ドールの湖：Ｂ１", "d19_01", arg55, arg2, 0, const_cast<short[]>(PossessionItem_217), 10000, const_cast<SCharacterParameter[]>(Party_217), -1, const_cast<short[]>(GlobalFlag_217), const_cast<short[]>(TreasureFlag_217));
			GlobalFlag_218 = new short[196]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 901, 903, 904, 905,
				906, 907, 910, 908, 909, 911, 912, 913, 914, 915,
				916, 917, 953, 951, 950, 952, 954, 955, 956, 957,
				958, 959, 960, 423, 424, 425, 850, 851, 852, 853,
				854, 855, 856, 857, 858, -1
			};
			TreasureFlag_218 = new short[1] { -1 };
			PossessionItem_218 = new short[5] { 4006, 5207, 4005, 5204, -1 };
			Party_218 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 34, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 34, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 34, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 34, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg56 = new float[3] { 114f, 0f, 125f };
			arg2 = new float[3];
			EventJumpParam_218 = new SEventJumpParameter("ドールの湖：リヴァイアサン前", "d19_04", arg56, arg2, 0, const_cast<short[]>(PossessionItem_218), 10000, const_cast<SCharacterParameter[]>(Party_218), -1, const_cast<short[]>(GlobalFlag_218), const_cast<short[]>(TreasureFlag_218));
			GlobalFlag_219 = new short[197]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 901, 903, 904, 905,
				906, 907, 910, 908, 909, 911, 912, 913, 914, 915,
				916, 917, 953, 951, 950, 952, 954, 955, 956, 957,
				958, 959, 960, 423, 424, 425, 972, 850, 851, 852,
				853, 854, 855, 856, 857, 858, -1
			};
			TreasureFlag_219 = new short[1] { -1 };
			PossessionItem_219 = new short[5] { 4006, 5207, 4005, 5204, -1 };
			Party_219 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg57 = new float[3] { 0f, 0f, -50f };
			arg2 = new float[3];
			EventJumpParam_219 = new SEventJumpParameter("ドールの湖：リヴァイアサン後", "d19_04", arg57, arg2, 0, const_cast<short[]>(PossessionItem_219), 10000, const_cast<SCharacterParameter[]>(Party_219), -1, const_cast<short[]>(GlobalFlag_219), const_cast<short[]>(TreasureFlag_219));
			GlobalFlag_220 = new short[193]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 901, 903, 904, 905,
				906, 907, 910, 908, 909, 911, 912, 913, 914, 915,
				916, 917, 953, 951, 950, 952, 954, 955, 956, 957,
				958, 959, 960, 850, 851, 852, 853, 854, 855, 856,
				857, 858, -1
			};
			TreasureFlag_220 = new short[1] { -1 };
			PossessionItem_220 = new short[5] { 4006, 5207, 4005, 5204, -1 };
			Party_220 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 35, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 35, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 35, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 35, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg58 = new float[3] { 408f, 0f, -460f };
			arg2 = new float[3];
			EventJumpParam_220 = new SEventJumpParameter("ワールド：バハムートの洞窟付近（座標未定）", "f01_66", arg58, arg2, 0, const_cast<short[]>(PossessionItem_220), 10000, const_cast<SCharacterParameter[]>(Party_220), -1, const_cast<short[]>(GlobalFlag_220), const_cast<short[]>(TreasureFlag_220));
			GlobalFlag_221 = new short[193]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 901, 903, 904, 905,
				906, 907, 910, 908, 909, 911, 912, 913, 914, 915,
				916, 917, 953, 951, 950, 952, 954, 955, 956, 957,
				958, 959, 960, 850, 851, 852, 853, 854, 855, 856,
				857, 858, -1
			};
			TreasureFlag_221 = new short[1] { -1 };
			PossessionItem_221 = new short[5] { 4006, 5207, 4005, 5204, -1 };
			Party_221 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 35, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 35, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 35, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 35, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg59 = new float[3] { 0f, 0f, 135f };
			arg2 = new float[3];
			EventJumpParam_221 = new SEventJumpParameter("バハムートの洞窟：Ｂ１", "d20_01", arg59, arg2, 0, const_cast<short[]>(PossessionItem_221), 10000, const_cast<SCharacterParameter[]>(Party_221), -1, const_cast<short[]>(GlobalFlag_221), const_cast<short[]>(TreasureFlag_221));
			GlobalFlag_222 = new short[194]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 901, 903, 904, 905,
				906, 907, 910, 908, 909, 911, 912, 913, 914, 915,
				916, 917, 953, 951, 950, 952, 954, 955, 956, 957,
				958, 959, 960, 850, 851, 852, 853, 854, 855, 856,
				857, 858, 430, -1
			};
			TreasureFlag_222 = new short[1] { -1 };
			PossessionItem_222 = new short[5] { 4006, 5207, 4005, 5204, -1 };
			Party_222 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 35, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 35, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 35, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 35, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg60 = new float[3] { -11f, 0f, 205f };
			arg2 = new float[3];
			EventJumpParam_222 = new SEventJumpParameter("バハムートの洞窟：バハムート前", "d20_03", arg60, arg2, 0, const_cast<short[]>(PossessionItem_222), 10000, const_cast<SCharacterParameter[]>(Party_222), -1, const_cast<short[]>(GlobalFlag_222), const_cast<short[]>(TreasureFlag_222));
			GlobalFlag_223 = new short[195]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 901, 903, 904, 905,
				906, 907, 910, 908, 909, 911, 912, 913, 914, 915,
				916, 917, 953, 951, 950, 952, 954, 955, 956, 957,
				958, 959, 960, 430, 973, 850, 851, 852, 853, 854,
				855, 856, 857, 858, -1
			};
			TreasureFlag_223 = new short[1] { -1 };
			PossessionItem_223 = new short[5] { 4006, 5207, 4005, 5204, -1 };
			Party_223 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg61 = new float[3] { 0f, 0f, 150f };
			arg2 = new float[3];
			EventJumpParam_223 = new SEventJumpParameter("バハムートの洞窟：バハムート後", "d20_03", arg61, arg2, 0, const_cast<short[]>(PossessionItem_223), 10000, const_cast<SCharacterParameter[]>(Party_223), -1, const_cast<short[]>(GlobalFlag_223), const_cast<short[]>(TreasureFlag_223));
			GlobalFlag_224 = new short[193]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 901, 903, 904, 905,
				906, 907, 910, 908, 909, 911, 912, 913, 914, 915,
				916, 917, 953, 951, 950, 952, 954, 955, 956, 957,
				958, 959, 960, 850, 851, 852, 853, 854, 855, 856,
				857, 858, -1
			};
			TreasureFlag_224 = new short[1] { -1 };
			PossessionItem_224 = new short[5] { 4006, 5207, 4005, 5204, -1 };
			Party_224 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 35, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 35, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 35, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 35, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg62 = new float[3] { -100f, 0f, -80f };
			arg2 = new float[3];
			EventJumpParam_224 = new SEventJumpParameter("ファルガバード：外観", "t29_01", arg62, arg2, 0, const_cast<short[]>(PossessionItem_224), 10000, const_cast<SCharacterParameter[]>(Party_224), -1, const_cast<short[]>(GlobalFlag_224), const_cast<short[]>(TreasureFlag_224));
			GlobalFlag_225 = new short[193]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 901, 903, 904, 905,
				906, 907, 910, 908, 909, 911, 912, 913, 914, 915,
				916, 917, 953, 951, 950, 952, 954, 955, 956, 957,
				958, 959, 960, 850, 851, 852, 853, 854, 855, 856,
				857, 858, -1
			};
			TreasureFlag_225 = new short[1] { -1 };
			PossessionItem_225 = new short[5] { 4006, 5207, 4005, 5204, -1 };
			Party_225 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 35, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 35, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 35, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 35, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg63 = new float[3] { 5f, 0f, 60f };
			arg2 = new float[3];
			EventJumpParam_225 = new SEventJumpParameter("ファルガバード：滝の裏", "t29_07", arg63, arg2, 0, const_cast<short[]>(PossessionItem_225), 10000, const_cast<SCharacterParameter[]>(Party_225), -1, const_cast<short[]>(GlobalFlag_225), const_cast<short[]>(TreasureFlag_225));
			GlobalFlag_226 = new short[195]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 901, 903,
				904, 905, 906, 907, 910, 908, 909, 911, 912, 913,
				914, 915, 916, 917, 953, 951, 950, 952, 954, 955,
				956, 957, 958, 959, 960, 850, 851, 852, 853, 854,
				855, 856, 857, 858, -1
			};
			TreasureFlag_226 = new short[1] { -1 };
			PossessionItem_226 = new short[5] { 4006, 5207, 4005, 5204, -1 };
			Party_226 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 36, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 36, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 36, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 36, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg64 = new float[3] { -95f, 0f, 120f };
			arg2 = new float[3];
			EventJumpParam_226 = new SEventJumpParameter("暗黒の洞窟：B1", "d21_01", arg64, arg2, 0, const_cast<short[]>(PossessionItem_226), 10000, const_cast<SCharacterParameter[]>(Party_226), -1, const_cast<short[]>(GlobalFlag_226), const_cast<short[]>(TreasureFlag_226));
			GlobalFlag_227 = new short[197]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				901, 903, 904, 905, 906, 907, 910, 908, 909, 911,
				912, 913, 914, 915, 916, 917, 953, 951, 950, 952,
				954, 955, 956, 957, 958, 959, 960, 850, 851, 852,
				853, 854, 855, 856, 857, 858, -1
			};
			TreasureFlag_227 = new short[1] { -1 };
			PossessionItem_227 = new short[5] { 4006, 5207, 4005, 5204, -1 };
			Party_227 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 36, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 36, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 36, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 36, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg65 = new float[3] { 0f, 0f, -260f };
			arg2 = new float[3];
			EventJumpParam_227 = new SEventJumpParameter("暗黒の洞窟：ボス前", "d21_09", arg65, arg2, 0, const_cast<short[]>(PossessionItem_227), 10000, const_cast<SCharacterParameter[]>(Party_227), -1, const_cast<short[]>(GlobalFlag_227), const_cast<short[]>(TreasureFlag_227));
			GlobalFlag_228 = new short[198]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 901, 903, 904, 905, 906, 907, 910, 908, 909,
				911, 912, 913, 914, 915, 916, 917, 953, 951, 950,
				952, 954, 955, 956, 957, 958, 959, 960, 850, 851,
				852, 853, 854, 855, 856, 857, 858, -1
			};
			TreasureFlag_228 = new short[1] { -1 };
			PossessionItem_228 = new short[5] { 4006, 5207, 4005, 5204, -1 };
			Party_228 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg66 = new float[3] { 0f, 0f, -10f };
			arg2 = new float[3];
			EventJumpParam_228 = new SEventJumpParameter("暗黒の洞窟：ボス後", "d21_09", arg66, arg2, 0, const_cast<short[]>(PossessionItem_228), 10000, const_cast<SCharacterParameter[]>(Party_228), -1, const_cast<short[]>(GlobalFlag_228), const_cast<short[]>(TreasureFlag_228));
			GlobalFlag_229 = new short[200]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 901, 903, 904, 905, 906, 907, 910,
				908, 909, 911, 912, 913, 914, 915, 916, 917, 953,
				951, 950, 952, 954, 955, 956, 957, 958, 959, 960,
				850, 851, 852, 853, 854, 855, 856, 857, 858, -1
			};
			TreasureFlag_229 = new short[1] { -1 };
			PossessionItem_229 = new short[5] { 4006, 5207, 4005, 5204, -1 };
			Party_229 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 38, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 38, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 38, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 38, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg67 = new float[3] { 35f, 0f, 110f };
			arg2 = new float[3];
			EventJumpParam_229 = new SEventJumpParameter("ドーガの館：暗黒洞窟クリア後", "t25_01", arg67, arg2, 0, const_cast<short[]>(PossessionItem_229), 10000, const_cast<SCharacterParameter[]>(Party_229), -1, const_cast<short[]>(GlobalFlag_229), const_cast<short[]>(TreasureFlag_229));
			GlobalFlag_230 = new short[200]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 901, 903, 904, 905, 906, 907, 910,
				908, 909, 911, 912, 913, 914, 915, 916, 917, 953,
				951, 950, 952, 954, 955, 956, 957, 958, 959, 960,
				850, 851, 852, 853, 854, 855, 856, 857, 858, -1
			};
			TreasureFlag_230 = new short[1] { -1 };
			PossessionItem_230 = new short[5] { 4006, 5207, 4005, 5204, -1 };
			Party_230 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 38, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 38, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 38, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 38, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg68 = new float[3] { 125f, 0f, -50f };
			arg2 = new float[3];
			EventJumpParam_230 = new SEventJumpParameter("ドーガの洞窟：Ｂ１", "d22_01", arg68, arg2, 0, const_cast<short[]>(PossessionItem_230), 10000, const_cast<SCharacterParameter[]>(Party_230), -1, const_cast<short[]>(GlobalFlag_230), const_cast<short[]>(TreasureFlag_230));
			GlobalFlag_231 = new short[201]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 901, 903, 904, 905, 906, 907,
				910, 908, 909, 911, 912, 913, 914, 915, 916, 917,
				953, 951, 950, 952, 954, 955, 956, 957, 958, 959,
				960, 850, 851, 852, 853, 854, 855, 856, 857, 858,
				-1
			};
			TreasureFlag_231 = new short[1] { -1 };
			PossessionItem_231 = new short[5] { 4006, 5207, 4005, 5204, -1 };
			Party_231 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 38, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 38, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 38, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 38, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg69 = new float[3] { 5f, 0f, -20f };
			arg2 = new float[3];
			EventJumpParam_231 = new SEventJumpParameter("ドーガの洞窟：ドーガ戦前", "d22_08", arg69, arg2, 0, const_cast<short[]>(PossessionItem_231), 10000, const_cast<SCharacterParameter[]>(Party_231), -1, const_cast<short[]>(GlobalFlag_231), const_cast<short[]>(TreasureFlag_231));
			GlobalFlag_232 = new short[202]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 901, 903, 904, 905, 906,
				907, 910, 908, 909, 911, 912, 913, 914, 915, 916,
				917, 953, 951, 950, 952, 954, 955, 956, 957, 958,
				959, 960, 850, 851, 852, 853, 854, 855, 856, 857,
				858, -1
			};
			TreasureFlag_232 = new short[1] { -1 };
			PossessionItem_232 = new short[5] { 4006, 5207, 4005, 5204, -1 };
			Party_232 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 38, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 38, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 38, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 38, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg70 = new float[3] { 5f, 0f, -20f };
			arg2 = new float[3];
			EventJumpParam_232 = new SEventJumpParameter("ドーガの洞窟：ドーガ戦後", "d22_08", arg70, arg2, 0, const_cast<short[]>(PossessionItem_232), 10000, const_cast<SCharacterParameter[]>(Party_232), -1, const_cast<short[]>(GlobalFlag_232), const_cast<short[]>(TreasureFlag_232));
			GlobalFlag_233 = new short[204]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 901, 903, 904,
				905, 906, 907, 910, 908, 909, 911, 912, 913, 914,
				915, 916, 917, 953, 951, 950, 952, 954, 955, 956,
				957, 958, 959, 960, 850, 851, 852, 853, 854, 855,
				856, 857, 858, -1
			};
			TreasureFlag_233 = new short[1] { -1 };
			PossessionItem_233 = new short[5] { 4006, 5207, 4005, 5204, -1 };
			Party_233 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 38, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 38, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 38, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 38, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg71 = new float[3] { 5f, 0f, -20f };
			arg2 = new float[3];
			EventJumpParam_233 = new SEventJumpParameter("ドーガの洞窟：ウネ戦後", "d22_08", arg71, arg2, 0, const_cast<short[]>(PossessionItem_233), 10000, const_cast<SCharacterParameter[]>(Party_233), -1, const_cast<short[]>(GlobalFlag_233), const_cast<short[]>(TreasureFlag_233));
			GlobalFlag_234 = new short[208]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 901, 903, 904, 905, 906, 907, 910, 908, 909,
				911, 912, 913, 914, 915, 916, 917, 953, 951, 950,
				952, 954, 955, 956, 957, 958, 959, 960, 850, 851,
				852, 853, 854, 855, 856, 857, 858, -1
			};
			TreasureFlag_234 = new short[1] { -1 };
			PossessionItem_234 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_234 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg72 = new float[3] { -5f, 0f, -170f };
			arg2 = new float[3];
			EventJumpParam_234 = new SEventJumpParameter("アムル：ドガウネ死亡後", "t17_01", arg72, arg2, 0, const_cast<short[]>(PossessionItem_234), 10000, const_cast<SCharacterParameter[]>(Party_234), -1, const_cast<short[]>(GlobalFlag_234), const_cast<short[]>(TreasureFlag_234));
			GlobalFlag_235 = new short[208]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 901, 903, 904, 905, 906, 907, 910, 908, 909,
				911, 912, 913, 914, 915, 916, 917, 953, 951, 950,
				952, 954, 955, 956, 957, 958, 959, 960, 850, 851,
				852, 853, 854, 855, 856, 857, 858, -1
			};
			TreasureFlag_235 = new short[1] { -1 };
			PossessionItem_235 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_235 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg73 = new float[3] { 489f, 0f, 379f };
			arg2 = new float[3];
			EventJumpParam_235 = new SEventJumpParameter("ワールド：石像付近（座標未定）", "f03_95", arg73, arg2, 0, const_cast<short[]>(PossessionItem_235), 10000, const_cast<SCharacterParameter[]>(Party_235), -1, const_cast<short[]>(GlobalFlag_235), const_cast<short[]>(TreasureFlag_235));
			GlobalFlag_236 = new short[212]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 901, 903, 904, 905, 906,
				907, 910, 908, 909, 911, 912, 913, 914, 915, 916,
				917, 953, 951, 950, 952, 954, 955, 956, 957, 958,
				959, 960, 850, 851, 852, 853, 854, 855, 856, 857,
				858, -1
			};
			TreasureFlag_236 = new short[1] { -1 };
			PossessionItem_236 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_236 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg74 = new float[3] { 469f, 0f, 593f };
			arg2 = new float[3];
			EventJumpParam_236 = new SEventJumpParameter("ワールド：石像破壊後（座標未定）", "f03_94", arg74, arg2, 0, const_cast<short[]>(PossessionItem_236), 10000, const_cast<SCharacterParameter[]>(Party_236), -1, const_cast<short[]>(GlobalFlag_236), const_cast<short[]>(TreasureFlag_236));
			GlobalFlag_237 = new short[212]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 901, 903, 904, 905, 906,
				907, 910, 908, 909, 911, 912, 913, 914, 915, 916,
				917, 953, 951, 950, 952, 954, 955, 956, 957, 958,
				959, 960, 850, 851, 852, 853, 854, 855, 856, 857,
				858, -1
			};
			TreasureFlag_237 = new short[1] { -1 };
			PossessionItem_237 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_237 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg75 = new float[3] { 0f, 0f, -175f };
			arg2 = new float[3];
			EventJumpParam_237 = new SEventJumpParameter("古代の民の迷宮：入り口", "d23_01", arg75, arg2, 0, const_cast<short[]>(PossessionItem_237), 10000, const_cast<SCharacterParameter[]>(Party_237), -1, const_cast<short[]>(GlobalFlag_237), const_cast<short[]>(TreasureFlag_237));
			GlobalFlag_238 = new short[212]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 901, 903, 904, 905, 906,
				907, 910, 908, 909, 911, 912, 913, 914, 915, 916,
				917, 953, 951, 950, 952, 954, 955, 956, 957, 958,
				959, 960, 850, 851, 852, 853, 854, 855, 856, 857,
				858, -1
			};
			TreasureFlag_238 = new short[1] { -1 };
			PossessionItem_238 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_238 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg76 = new float[3] { 5f, 0f, -40f };
			arg2 = new float[3];
			EventJumpParam_238 = new SEventJumpParameter("古代の民の迷宮：ボス前", "d23_02", arg76, arg2, 0, const_cast<short[]>(PossessionItem_238), 10000, const_cast<SCharacterParameter[]>(Party_238), -1, const_cast<short[]>(GlobalFlag_238), const_cast<short[]>(TreasureFlag_238));
			GlobalFlag_239 = new short[213]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 901, 903, 904, 905,
				906, 907, 910, 908, 909, 911, 912, 913, 914, 915,
				916, 917, 953, 951, 950, 952, 954, 955, 956, 957,
				958, 959, 960, 850, 851, 852, 853, 854, 855, 856,
				857, 858, -1
			};
			TreasureFlag_239 = new short[1] { -1 };
			PossessionItem_239 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_239 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg77 = new float[3] { 5f, 0f, -40f };
			arg2 = new float[3];
			EventJumpParam_239 = new SEventJumpParameter("古代の民の迷宮：ボス後", "d23_02", arg77, arg2, 0, const_cast<short[]>(PossessionItem_239), 10000, const_cast<SCharacterParameter[]>(Party_239), -1, const_cast<short[]>(GlobalFlag_239), const_cast<short[]>(TreasureFlag_239));
			GlobalFlag_240 = new short[221]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 901, 903,
				904, 905, 906, 907, 910, 908, 909, 911, 912, 913,
				914, 915, 916, 917, 918, 919, 920, 921, 922, 923,
				953, 951, 950, 952, 954, 955, 956, 957, 958, 959,
				960, 850, 851, 852, 853, 854, 855, 856, 857, 858,
				-1
			};
			TreasureFlag_240 = new short[1] { -1 };
			PossessionItem_240 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_240 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg78 = new float[3] { 0f, 0f, -10f };
			arg2 = new float[3];
			EventJumpParam_240 = new SEventJumpParameter("クリスタルタワー：入り口", "d25_01", arg78, arg2, 0, const_cast<short[]>(PossessionItem_240), 10000, const_cast<SCharacterParameter[]>(Party_240), -1, const_cast<short[]>(GlobalFlag_240), const_cast<short[]>(TreasureFlag_240));
			GlobalFlag_241 = new short[221]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 901, 903,
				904, 905, 906, 907, 910, 908, 909, 911, 912, 913,
				914, 915, 916, 917, 918, 919, 920, 921, 922, 923,
				953, 951, 950, 952, 954, 955, 956, 957, 958, 959,
				960, 850, 851, 852, 853, 854, 855, 856, 857, 858,
				-1
			};
			TreasureFlag_241 = new short[1] { -1 };
			PossessionItem_241 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_241 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 42, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 42, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 42, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 42, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg79 = new float[3] { 0f, 0f, -10f };
			arg2 = new float[3];
			EventJumpParam_241 = new SEventJumpParameter("クリスタルタワー：エウレカへの扉", "d25_02", arg79, arg2, 0, const_cast<short[]>(PossessionItem_241), 10000, const_cast<SCharacterParameter[]>(Party_241), -1, const_cast<short[]>(GlobalFlag_241), const_cast<short[]>(TreasureFlag_241));
			GlobalFlag_242 = new short[222]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 375, 901,
				903, 904, 905, 906, 907, 910, 908, 909, 911, 912,
				913, 914, 915, 916, 917, 918, 919, 920, 921, 922,
				923, 953, 951, 950, 952, 954, 955, 956, 957, 958,
				959, 960, 850, 851, 852, 853, 854, 855, 856, 857,
				858, -1
			};
			TreasureFlag_242 = new short[1] { -1 };
			PossessionItem_242 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_242 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 42, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 42, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 42, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 42, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg80 = new float[3] { 0f, 6f, -215f };
			arg2 = new float[3];
			EventJumpParam_242 = new SEventJumpParameter("エウレカ：Ｂ１", "d24_01", arg80, arg2, 0, const_cast<short[]>(PossessionItem_242), 10000, const_cast<SCharacterParameter[]>(Party_242), -1, const_cast<short[]>(GlobalFlag_242), const_cast<short[]>(TreasureFlag_242));
			GlobalFlag_243 = new short[222]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 375, 901,
				903, 904, 905, 906, 907, 910, 908, 909, 911, 912,
				913, 914, 915, 916, 917, 918, 919, 920, 921, 922,
				923, 953, 951, 950, 952, 954, 955, 956, 957, 958,
				959, 960, 850, 851, 852, 853, 854, 855, 856, 857,
				858, -1
			};
			TreasureFlag_243 = new short[1] { -1 };
			PossessionItem_243 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_243 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 42, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 42, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 42, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 42, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg81 = new float[3] { 0f, 0f, 205f };
			arg2 = new float[3];
			EventJumpParam_243 = new SEventJumpParameter("エウレカ：Ｂ２Ａ", "d24_02", arg81, arg2, 0, const_cast<short[]>(PossessionItem_243), 10000, const_cast<SCharacterParameter[]>(Party_243), -1, const_cast<short[]>(GlobalFlag_243), const_cast<short[]>(TreasureFlag_243));
			GlobalFlag_244 = new short[222]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 375, 901,
				903, 904, 905, 906, 907, 910, 908, 909, 911, 912,
				913, 914, 915, 916, 917, 918, 919, 920, 921, 922,
				923, 953, 951, 950, 952, 954, 955, 956, 957, 958,
				959, 960, 850, 851, 852, 853, 854, 855, 856, 857,
				858, -1
			};
			TreasureFlag_244 = new short[1] { -1 };
			PossessionItem_244 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_244 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 42, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 42, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 42, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 42, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg82 = new float[3] { -120f, 24f, 195f };
			arg2 = new float[3];
			EventJumpParam_244 = new SEventJumpParameter("エウレカ：Ｂ３", "d24_05", arg82, arg2, 0, const_cast<short[]>(PossessionItem_244), 10000, const_cast<SCharacterParameter[]>(Party_244), -1, const_cast<short[]>(GlobalFlag_244), const_cast<short[]>(TreasureFlag_244));
			GlobalFlag_245 = new short[222]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 375, 901,
				903, 904, 905, 906, 907, 910, 908, 909, 911, 912,
				913, 914, 915, 916, 917, 918, 919, 920, 921, 922,
				923, 953, 951, 950, 952, 954, 955, 956, 957, 958,
				959, 960, 850, 851, 852, 853, 854, 855, 856, 857,
				858, -1
			};
			TreasureFlag_245 = new short[1] { -1 };
			PossessionItem_245 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_245 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 42, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 42, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 42, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 42, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg83 = new float[3] { -120f, 0f, 195f };
			arg2 = new float[3];
			EventJumpParam_245 = new SEventJumpParameter("エウレカ：Ｂ４", "d24_06", arg83, arg2, 0, const_cast<short[]>(PossessionItem_245), 10000, const_cast<SCharacterParameter[]>(Party_245), -1, const_cast<short[]>(GlobalFlag_245), const_cast<short[]>(TreasureFlag_245));
			GlobalFlag_246 = new short[222]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 375, 901,
				903, 904, 905, 906, 907, 910, 908, 909, 911, 912,
				913, 914, 915, 916, 917, 918, 919, 920, 921, 922,
				923, 953, 951, 950, 952, 954, 955, 956, 957, 958,
				959, 960, 850, 851, 852, 853, 854, 855, 856, 857,
				858, -1
			};
			TreasureFlag_246 = new short[1] { -1 };
			PossessionItem_246 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_246 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 42, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 42, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 42, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 42, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg84 = new float[3] { 0f, 0f, 195f };
			arg2 = new float[3];
			EventJumpParam_246 = new SEventJumpParameter("エウレカ：Ｂ５", "d24_07", arg84, arg2, 0, const_cast<short[]>(PossessionItem_246), 10000, const_cast<SCharacterParameter[]>(Party_246), -1, const_cast<short[]>(GlobalFlag_246), const_cast<short[]>(TreasureFlag_246));
			GlobalFlag_247 = new short[222]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 375, 901,
				903, 904, 905, 906, 907, 910, 908, 909, 911, 912,
				913, 914, 915, 916, 917, 918, 919, 920, 921, 922,
				923, 953, 951, 950, 952, 954, 955, 956, 957, 958,
				959, 960, 850, 851, 852, 853, 854, 855, 856, 857,
				858, -1
			};
			TreasureFlag_247 = new short[1] { -1 };
			PossessionItem_247 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_247 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 42, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 42, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 42, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 42, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg85 = new float[3] { 0f, 0f, 40f };
			arg2 = new float[3];
			EventJumpParam_247 = new SEventJumpParameter("エウレカ：Ｂ６Ａ", "d24_08", arg85, arg2, 0, const_cast<short[]>(PossessionItem_247), 10000, const_cast<SCharacterParameter[]>(Party_247), -1, const_cast<short[]>(GlobalFlag_247), const_cast<short[]>(TreasureFlag_247));
			GlobalFlag_248 = new short[222]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 375, 901,
				903, 904, 905, 906, 907, 910, 908, 909, 911, 912,
				913, 914, 915, 916, 917, 918, 919, 920, 921, 922,
				923, 953, 951, 950, 952, 954, 955, 956, 957, 958,
				959, 960, 850, 851, 852, 853, 854, 855, 856, 857,
				858, -1
			};
			TreasureFlag_248 = new short[1] { -1 };
			PossessionItem_248 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_248 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 42, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 42, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 42, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 42, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg86 = new float[3] { 0f, 0f, -70f };
			arg2 = new float[3];
			EventJumpParam_248 = new SEventJumpParameter("エウレカ：Ｂ６Ｂ", "d24_09", arg86, arg2, 0, const_cast<short[]>(PossessionItem_248), 10000, const_cast<SCharacterParameter[]>(Party_248), -1, const_cast<short[]>(GlobalFlag_248), const_cast<short[]>(TreasureFlag_248));
			GlobalFlag_249 = new short[225]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 375, 368,
				369, 901, 903, 904, 905, 906, 907, 910, 908, 909,
				911, 912, 913, 914, 915, 916, 917, 918, 919, 920,
				921, 922, 923, 902, 953, 951, 950, 952, 954, 955,
				956, 957, 958, 959, 960, 850, 851, 852, 853, 854,
				855, 856, 857, 858, -1
			};
			TreasureFlag_249 = new short[3] { 576, 577, -1 };
			PossessionItem_249 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_249 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 42, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 42, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 42, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 42, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg87 = new float[3] { 0f, 0f, -85f };
			arg2 = new float[3];
			EventJumpParam_249 = new SEventJumpParameter("エウレカ：Ｂ６Ｃ（最深部）", "d24_10", arg87, arg2, 0, const_cast<short[]>(PossessionItem_249), 10000, const_cast<SCharacterParameter[]>(Party_249), -1, const_cast<short[]>(GlobalFlag_249), const_cast<short[]>(TreasureFlag_249));
			GlobalFlag_250 = new short[226]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 375, 368,
				369, 376, 901, 903, 904, 905, 906, 907, 910, 908,
				909, 911, 912, 913, 914, 915, 916, 917, 918, 919,
				920, 921, 922, 923, 902, 953, 951, 950, 952, 954,
				955, 956, 957, 958, 959, 960, 850, 851, 852, 853,
				854, 855, 856, 857, 858, -1
			};
			TreasureFlag_250 = new short[3] { 576, 577, -1 };
			PossessionItem_250 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_250 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg88 = new float[3] { 0f, -36f, -124f };
			arg2 = new float[3];
			EventJumpParam_250 = new SEventJumpParameter("クリスタルタワー：７ＦＢ", "d25_09", arg88, arg2, 0, const_cast<short[]>(PossessionItem_250), 10000, const_cast<SCharacterParameter[]>(Party_250), -1, const_cast<short[]>(GlobalFlag_250), const_cast<short[]>(TreasureFlag_250));
			GlobalFlag_251 = new short[227]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 375, 368,
				369, 376, 377, 901, 903, 904, 905, 906, 907, 910,
				908, 909, 911, 912, 913, 914, 915, 916, 917, 918,
				919, 920, 921, 922, 923, 902, 953, 951, 950, 952,
				954, 955, 956, 957, 958, 959, 960, 850, 851, 852,
				853, 854, 855, 856, 857, 858, -1
			};
			TreasureFlag_251 = new short[3] { 576, 577, -1 };
			PossessionItem_251 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_251 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg89 = new float[3] { 0f, 0f, -10f };
			arg2 = new float[3];
			EventJumpParam_251 = new SEventJumpParameter("サスーン外観：ドーガの光（サラ姫）", "t03_01", arg89, arg2, 0, const_cast<short[]>(PossessionItem_251), 10000, const_cast<SCharacterParameter[]>(Party_251), -1, const_cast<short[]>(GlobalFlag_251), const_cast<short[]>(TreasureFlag_251));
			GlobalFlag_252 = new short[227]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 375, 368,
				369, 376, 377, 901, 903, 904, 905, 906, 907, 910,
				908, 909, 911, 912, 913, 914, 915, 916, 917, 918,
				919, 920, 921, 922, 923, 902, 953, 951, 950, 952,
				954, 955, 956, 957, 958, 959, 960, 850, 851, 852,
				853, 854, 855, 856, 857, 858, -1
			};
			TreasureFlag_252 = new short[3] { 576, 577, -1 };
			PossessionItem_252 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_252 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg90 = new float[3] { 0f, 0f, -10f };
			arg2 = new float[3];
			EventJumpParam_252 = new SEventJumpParameter("サスーン右の塔４Ｆ：ドーガの光（サラ姫）", "t03_10", arg90, arg2, 0, const_cast<short[]>(PossessionItem_252), 10000, const_cast<SCharacterParameter[]>(Party_252), -1, const_cast<short[]>(GlobalFlag_252), const_cast<short[]>(TreasureFlag_252));
			GlobalFlag_253 = new short[227]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 375, 368,
				369, 376, 377, 901, 903, 904, 905, 906, 907, 910,
				908, 909, 911, 912, 913, 914, 915, 916, 917, 918,
				919, 920, 921, 922, 923, 902, 953, 951, 950, 952,
				954, 955, 956, 957, 958, 959, 960, 850, 851, 852,
				853, 854, 855, 856, 857, 858, -1
			};
			TreasureFlag_253 = new short[3] { 576, 577, -1 };
			PossessionItem_253 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_253 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg91 = new float[3] { 0f, 0f, -10f };
			arg2 = new float[3];
			EventJumpParam_253 = new SEventJumpParameter("カナーン外観：ドーガの光（シド）", "t04_01", arg91, arg2, 0, const_cast<short[]>(PossessionItem_253), 10000, const_cast<SCharacterParameter[]>(Party_253), -1, const_cast<short[]>(GlobalFlag_253), const_cast<short[]>(TreasureFlag_253));
			GlobalFlag_254 = new short[227]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 375, 368,
				369, 376, 377, 901, 903, 904, 905, 906, 907, 910,
				908, 909, 911, 912, 913, 914, 915, 916, 917, 918,
				919, 920, 921, 922, 923, 902, 953, 951, 950, 952,
				954, 955, 956, 957, 958, 959, 960, 850, 851, 852,
				853, 854, 855, 856, 857, 858, -1
			};
			TreasureFlag_254 = new short[3] { 576, 577, -1 };
			PossessionItem_254 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_254 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg92 = new float[3] { 0f, 0f, -10f };
			arg2 = new float[3];
			EventJumpParam_254 = new SEventJumpParameter("カナーンシドの家：ドーガの光（シド）", "t04_08", arg92, arg2, 0, const_cast<short[]>(PossessionItem_254), 10000, const_cast<SCharacterParameter[]>(Party_254), -1, const_cast<short[]>(GlobalFlag_254), const_cast<short[]>(TreasureFlag_254));
			GlobalFlag_255 = new short[227]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 375, 368,
				369, 376, 377, 901, 903, 904, 905, 906, 907, 910,
				908, 909, 911, 912, 913, 914, 915, 916, 917, 918,
				919, 920, 921, 922, 923, 902, 953, 951, 950, 952,
				954, 955, 956, 957, 958, 959, 960, 850, 851, 852,
				853, 854, 855, 856, 857, 858, -1
			};
			TreasureFlag_255 = new short[3] { 576, 577, -1 };
			PossessionItem_255 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_255 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg93 = new float[3] { 0f, 0f, -10f };
			arg2 = new float[3];
			EventJumpParam_255 = new SEventJumpParameter("オーエンの塔３Ｆ：ドーガの光（デッシュ）", "d07_04", arg93, arg2, 0, const_cast<short[]>(PossessionItem_255), 10000, const_cast<SCharacterParameter[]>(Party_255), -1, const_cast<short[]>(GlobalFlag_255), const_cast<short[]>(TreasureFlag_255));
			GlobalFlag_256 = new short[227]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 375, 368,
				369, 376, 377, 901, 903, 904, 905, 906, 907, 910,
				908, 909, 911, 912, 913, 914, 915, 916, 917, 918,
				919, 920, 921, 922, 923, 902, 953, 951, 950, 952,
				954, 955, 956, 957, 958, 959, 960, 850, 851, 852,
				853, 854, 855, 856, 857, 858, -1
			};
			TreasureFlag_256 = new short[3] { 576, 577, -1 };
			PossessionItem_256 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_256 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg94 = new float[3] { 0f, 0f, -10f };
			arg2 = new float[3];
			EventJumpParam_256 = new SEventJumpParameter("オーエンの塔１０Ｆ：ドーガの光（デッシュ）", "d07_11", arg94, arg2, 0, const_cast<short[]>(PossessionItem_256), 10000, const_cast<SCharacterParameter[]>(Party_256), -1, const_cast<short[]>(GlobalFlag_256), const_cast<short[]>(TreasureFlag_256));
			GlobalFlag_257 = new short[227]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 375, 368,
				369, 376, 377, 901, 903, 904, 905, 906, 907, 910,
				908, 909, 911, 912, 913, 914, 915, 916, 917, 918,
				919, 920, 921, 922, 923, 902, 953, 951, 950, 952,
				954, 955, 956, 957, 958, 959, 960, 850, 851, 852,
				853, 854, 855, 856, 857, 858, -1
			};
			TreasureFlag_257 = new short[3] { 576, 577, -1 };
			PossessionItem_257 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_257 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg95 = new float[3] { 0f, 0f, -10f };
			arg2 = new float[3];
			EventJumpParam_257 = new SEventJumpParameter("サロニア城外観：ドーガの光（アルス）", "t24_01", arg95, arg2, 0, const_cast<short[]>(PossessionItem_257), 10000, const_cast<SCharacterParameter[]>(Party_257), -1, const_cast<short[]>(GlobalFlag_257), const_cast<short[]>(TreasureFlag_257));
			GlobalFlag_258 = new short[227]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 375, 368,
				369, 376, 377, 901, 903, 904, 905, 906, 907, 910,
				908, 909, 911, 912, 913, 914, 915, 916, 917, 918,
				919, 920, 921, 922, 923, 902, 953, 951, 950, 952,
				954, 955, 956, 957, 958, 959, 960, 850, 851, 852,
				853, 854, 855, 856, 857, 858, -1
			};
			TreasureFlag_258 = new short[3] { 576, 577, -1 };
			PossessionItem_258 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_258 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg96 = new float[3] { 0f, 0f, -10f };
			arg2 = new float[3];
			EventJumpParam_258 = new SEventJumpParameter("サロニア城王の間：ドーガの光（アルス）", "t24_04", arg96, arg2, 0, const_cast<short[]>(PossessionItem_258), 10000, const_cast<SCharacterParameter[]>(Party_258), -1, const_cast<short[]>(GlobalFlag_258), const_cast<short[]>(TreasureFlag_258));
			GlobalFlag_259 = new short[227]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 375, 368,
				369, 376, 377, 901, 903, 904, 905, 906, 907, 910,
				908, 909, 911, 912, 913, 914, 915, 916, 917, 918,
				919, 920, 921, 922, 923, 902, 953, 951, 950, 952,
				954, 955, 956, 957, 958, 959, 960, 850, 851, 852,
				853, 854, 855, 856, 857, 858, -1
			};
			TreasureFlag_259 = new short[3] { 576, 577, -1 };
			PossessionItem_259 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_259 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg97 = new float[3] { 0f, 0f, -10f };
			arg2 = new float[3];
			EventJumpParam_259 = new SEventJumpParameter("アムル外観：ドーガの光（４じい）", "t17_01", arg97, arg2, 0, const_cast<short[]>(PossessionItem_259), 10000, const_cast<SCharacterParameter[]>(Party_259), -1, const_cast<short[]>(GlobalFlag_259), const_cast<short[]>(TreasureFlag_259));
			GlobalFlag_260 = new short[227]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 375, 368,
				369, 376, 377, 901, 903, 904, 905, 906, 907, 910,
				908, 909, 911, 912, 913, 914, 915, 916, 917, 918,
				919, 920, 921, 922, 923, 902, 953, 951, 950, 952,
				954, 955, 956, 957, 958, 959, 960, 850, 851, 852,
				853, 854, 855, 856, 857, 858, -1
			};
			TreasureFlag_260 = new short[3] { 576, 577, -1 };
			PossessionItem_260 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_260 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg98 = new float[3] { 0f, 0f, -10f };
			arg2 = new float[3];
			EventJumpParam_260 = new SEventJumpParameter("クリスタルタワー：７ＦＢ（ドーガの光終了）", "d25_09", arg98, arg2, 0, const_cast<short[]>(PossessionItem_260), 10000, const_cast<SCharacterParameter[]>(Party_260), -1, const_cast<short[]>(GlobalFlag_260), const_cast<short[]>(TreasureFlag_260));
			GlobalFlag_261 = new short[228]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 375, 368,
				369, 376, 377, 378, 901, 903, 904, 905, 906, 907,
				910, 908, 909, 911, 912, 913, 914, 915, 916, 917,
				918, 919, 920, 921, 922, 923, 902, 953, 951, 950,
				952, 954, 955, 956, 957, 958, 959, 960, 850, 851,
				852, 853, 854, 855, 856, 857, 858, -1
			};
			TreasureFlag_261 = new short[3] { 576, 577, -1 };
			PossessionItem_261 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_261 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg99 = new float[3] { 0f, -36f, -79f };
			arg2 = new float[3];
			EventJumpParam_261 = new SEventJumpParameter("クリスタルタワー：闇への道（ザンデ前）", "d25_10", arg99, arg2, 0, const_cast<short[]>(PossessionItem_261), 10000, const_cast<SCharacterParameter[]>(Party_261), -1, const_cast<short[]>(GlobalFlag_261), const_cast<short[]>(TreasureFlag_261));
			GlobalFlag_262 = new short[230]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 375, 368,
				369, 376, 377, 378, 387, 388, 901, 903, 904, 905,
				906, 907, 910, 908, 909, 911, 912, 913, 914, 915,
				916, 917, 918, 919, 920, 921, 922, 923, 902, 953,
				951, 950, 952, 954, 955, 956, 957, 958, 959, 960,
				850, 851, 852, 853, 854, 855, 856, 857, 858, -1
			};
			TreasureFlag_262 = new short[3] { 576, 577, -1 };
			PossessionItem_262 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_262 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg100 = new float[3] { 0f, 0f, -10f };
			arg2 = new float[3];
			EventJumpParam_262 = new SEventJumpParameter("クリスタルタワー：闇への道（ザンデ後）", "d25_10", arg100, arg2, 0, const_cast<short[]>(PossessionItem_262), 10000, const_cast<SCharacterParameter[]>(Party_262), -1, const_cast<short[]>(GlobalFlag_262), const_cast<short[]>(TreasureFlag_262));
			GlobalFlag_263 = new short[231]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 375, 368,
				369, 376, 377, 378, 387, 388, 389, 901, 903, 904,
				905, 906, 907, 910, 908, 909, 911, 912, 913, 914,
				915, 916, 917, 918, 919, 920, 921, 922, 923, 902,
				953, 951, 950, 952, 954, 955, 956, 957, 958, 959,
				960, 850, 851, 852, 853, 854, 855, 856, 857, 858,
				-1
			};
			TreasureFlag_263 = new short[3] { 576, 577, -1 };
			PossessionItem_263 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_263 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg101 = new float[3] { 0f, 0f, -10f };
			arg2 = new float[3];
			EventJumpParam_263 = new SEventJumpParameter("クリスタルタワー：闇への道（くも後）", "d25_10", arg101, arg2, 0, const_cast<short[]>(PossessionItem_263), 10000, const_cast<SCharacterParameter[]>(Party_263), -1, const_cast<short[]>(GlobalFlag_263), const_cast<short[]>(TreasureFlag_263));
			GlobalFlag_264 = new short[232]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 375, 368,
				369, 376, 377, 378, 387, 388, 389, 390, 901, 903,
				904, 905, 906, 907, 910, 908, 909, 911, 912, 913,
				914, 915, 916, 917, 918, 919, 920, 921, 922, 923,
				902, 953, 951, 950, 952, 954, 955, 956, 957, 958,
				959, 960, 850, 851, 852, 853, 854, 855, 856, 857,
				858, -1
			};
			TreasureFlag_264 = new short[3] { 576, 577, -1 };
			PossessionItem_264 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_264 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 40, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg102 = new float[3] { 0f, -36f, -79f };
			arg2 = new float[3];
			EventJumpParam_264 = new SEventJumpParameter("クリスタルタワー：闇への道（くも後フリー）", "d25_10", arg102, arg2, 0, const_cast<short[]>(PossessionItem_264), 10000, const_cast<SCharacterParameter[]>(Party_264), -1, const_cast<short[]>(GlobalFlag_264), const_cast<short[]>(TreasureFlag_264));
			GlobalFlag_265 = new short[232]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 375, 368,
				369, 376, 377, 378, 387, 388, 389, 390, 901, 903,
				904, 905, 906, 907, 910, 908, 909, 911, 912, 913,
				914, 915, 916, 917, 918, 919, 920, 921, 922, 923,
				902, 953, 951, 950, 952, 954, 955, 956, 957, 958,
				959, 960, 850, 851, 852, 853, 854, 855, 856, 857,
				858, -1
			};
			TreasureFlag_265 = new short[3] { 576, 577, -1 };
			PossessionItem_265 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_265 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 48, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 48, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 48, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 48, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg103 = new float[3] { -128f, 0f, 116f };
			arg2 = new float[3];
			EventJumpParam_265 = new SEventJumpParameter("闇の世界：１Ｆ", "d26_01", arg103, arg2, 0, const_cast<short[]>(PossessionItem_265), 10000, const_cast<SCharacterParameter[]>(Party_265), -1, const_cast<short[]>(GlobalFlag_265), const_cast<short[]>(TreasureFlag_265));
			GlobalFlag_266 = new short[233]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 375, 368,
				369, 376, 377, 378, 387, 388, 389, 390, 396, 901,
				903, 904, 905, 906, 907, 910, 908, 909, 911, 912,
				913, 914, 915, 916, 917, 918, 919, 920, 921, 922,
				923, 902, 953, 951, 950, 952, 954, 955, 956, 957,
				958, 959, 960, 850, 851, 852, 853, 854, 855, 856,
				857, 858, -1
			};
			TreasureFlag_266 = new short[3] { 576, 577, -1 };
			PossessionItem_266 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_266 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 48, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 48, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 48, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 48, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg104 = new float[3] { 0f, 0f, -80f };
			arg2 = new float[3];
			EventJumpParam_266 = new SEventJumpParameter("闇の世界：風ボス前", "d26_06", arg104, arg2, 0, const_cast<short[]>(PossessionItem_266), 10000, const_cast<SCharacterParameter[]>(Party_266), -1, const_cast<short[]>(GlobalFlag_266), const_cast<short[]>(TreasureFlag_266));
			GlobalFlag_267 = new short[234]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 375, 368,
				369, 376, 377, 378, 387, 388, 389, 390, 396, 397,
				901, 903, 904, 905, 906, 907, 910, 908, 909, 911,
				912, 913, 914, 915, 916, 917, 918, 919, 920, 921,
				922, 923, 902, 953, 951, 950, 952, 954, 955, 956,
				957, 958, 959, 960, 850, 851, 852, 853, 854, 855,
				856, 857, 858, -1
			};
			TreasureFlag_267 = new short[3] { 576, 577, -1 };
			PossessionItem_267 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_267 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 48, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 48, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 48, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 48, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg105 = new float[3] { 0f, 0f, -80f };
			arg2 = new float[3];
			EventJumpParam_267 = new SEventJumpParameter("闇の世界：風ボス後", "d26_06", arg105, arg2, 0, const_cast<short[]>(PossessionItem_267), 10000, const_cast<SCharacterParameter[]>(Party_267), -1, const_cast<short[]>(GlobalFlag_267), const_cast<short[]>(TreasureFlag_267));
			GlobalFlag_268 = new short[236]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 375, 368,
				369, 376, 377, 378, 387, 388, 389, 390, 396, 397,
				398, 399, 901, 903, 904, 905, 906, 907, 910, 908,
				909, 911, 912, 913, 914, 915, 916, 917, 918, 919,
				920, 921, 922, 923, 902, 953, 951, 950, 952, 954,
				955, 956, 957, 958, 959, 960, 850, 851, 852, 853,
				854, 855, 856, 857, 858, -1
			};
			TreasureFlag_268 = new short[3] { 576, 577, -1 };
			PossessionItem_268 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_268 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 48, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 48, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 48, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 48, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg106 = new float[3] { 0f, 0f, -80f };
			arg2 = new float[3];
			EventJumpParam_268 = new SEventJumpParameter("闇の世界：火ボス前", "d26_07", arg106, arg2, 0, const_cast<short[]>(PossessionItem_268), 10000, const_cast<SCharacterParameter[]>(Party_268), -1, const_cast<short[]>(GlobalFlag_268), const_cast<short[]>(TreasureFlag_268));
			GlobalFlag_269 = new short[237]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 375, 368,
				369, 376, 377, 378, 387, 388, 389, 390, 396, 397,
				398, 399, 401, 901, 903, 904, 905, 906, 907, 910,
				908, 909, 911, 912, 913, 914, 915, 916, 917, 918,
				919, 920, 921, 922, 923, 902, 953, 951, 950, 952,
				954, 955, 956, 957, 958, 959, 960, 850, 851, 852,
				853, 854, 855, 856, 857, 858, -1
			};
			TreasureFlag_269 = new short[3] { 576, 577, -1 };
			PossessionItem_269 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_269 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 48, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 48, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 48, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 48, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg107 = new float[3] { 0f, 0f, -80f };
			arg2 = new float[3];
			EventJumpParam_269 = new SEventJumpParameter("闇の世界：火ボス後", "d26_07", arg107, arg2, 0, const_cast<short[]>(PossessionItem_269), 10000, const_cast<SCharacterParameter[]>(Party_269), -1, const_cast<short[]>(GlobalFlag_269), const_cast<short[]>(TreasureFlag_269));
			GlobalFlag_270 = new short[239]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 375, 368,
				369, 376, 377, 378, 387, 388, 389, 390, 396, 397,
				398, 399, 401, 402, 403, 901, 903, 904, 905, 906,
				907, 910, 908, 909, 911, 912, 913, 914, 915, 916,
				917, 918, 919, 920, 921, 922, 923, 902, 953, 951,
				950, 952, 954, 955, 956, 957, 958, 959, 960, 850,
				851, 852, 853, 854, 855, 856, 857, 858, -1
			};
			TreasureFlag_270 = new short[3] { 576, 577, -1 };
			PossessionItem_270 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_270 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 48, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 48, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 48, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 48, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg108 = new float[3] { 0f, 0f, -80f };
			arg2 = new float[3];
			EventJumpParam_270 = new SEventJumpParameter("闇の世界：水ボス前", "d26_08", arg108, arg2, 0, const_cast<short[]>(PossessionItem_270), 10000, const_cast<SCharacterParameter[]>(Party_270), -1, const_cast<short[]>(GlobalFlag_270), const_cast<short[]>(TreasureFlag_270));
			GlobalFlag_271 = new short[240]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 375, 368,
				369, 376, 377, 378, 387, 388, 389, 390, 396, 397,
				398, 399, 401, 402, 403, 405, 901, 903, 904, 905,
				906, 907, 910, 908, 909, 911, 912, 913, 914, 915,
				916, 917, 918, 919, 920, 921, 922, 923, 902, 953,
				951, 950, 952, 954, 955, 956, 957, 958, 959, 960,
				850, 851, 852, 853, 854, 855, 856, 857, 858, -1
			};
			TreasureFlag_271 = new short[3] { 576, 577, -1 };
			PossessionItem_271 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_271 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 48, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 48, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 48, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 48, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg109 = new float[3] { 0f, 0f, -80f };
			arg2 = new float[3];
			EventJumpParam_271 = new SEventJumpParameter("闇の世界：水ボス後", "d26_08", arg109, arg2, 0, const_cast<short[]>(PossessionItem_271), 10000, const_cast<SCharacterParameter[]>(Party_271), -1, const_cast<short[]>(GlobalFlag_271), const_cast<short[]>(TreasureFlag_271));
			GlobalFlag_272 = new short[242]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 375, 368,
				369, 376, 377, 378, 387, 388, 389, 390, 396, 397,
				398, 399, 401, 402, 403, 405, 406, 407, 901, 903,
				904, 905, 906, 907, 910, 908, 909, 911, 912, 913,
				914, 915, 916, 917, 918, 919, 920, 921, 922, 923,
				902, 953, 951, 950, 952, 954, 955, 956, 957, 958,
				959, 960, 850, 851, 852, 853, 854, 855, 856, 857,
				858, -1
			};
			TreasureFlag_272 = new short[3] { 576, 577, -1 };
			PossessionItem_272 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_272 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 48, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 48, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 48, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 48, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg110 = new float[3] { 0f, 0f, -80f };
			arg2 = new float[3];
			EventJumpParam_272 = new SEventJumpParameter("闇の世界：土ボス前", "d26_09", arg110, arg2, 0, const_cast<short[]>(PossessionItem_272), 10000, const_cast<SCharacterParameter[]>(Party_272), -1, const_cast<short[]>(GlobalFlag_272), const_cast<short[]>(TreasureFlag_272));
			GlobalFlag_273 = new short[243]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 375, 368,
				369, 376, 377, 378, 387, 388, 389, 390, 396, 397,
				398, 399, 401, 402, 403, 405, 406, 407, 409, 901,
				903, 904, 905, 906, 907, 910, 908, 909, 911, 912,
				913, 914, 915, 916, 917, 918, 919, 920, 921, 922,
				923, 902, 953, 951, 950, 952, 954, 955, 956, 957,
				958, 959, 960, 850, 851, 852, 853, 854, 855, 856,
				857, 858, -1
			};
			TreasureFlag_273 = new short[3] { 576, 577, -1 };
			PossessionItem_273 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_273 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 48, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 48, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 48, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 48, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg111 = new float[3] { 0f, 0f, -80f };
			arg2 = new float[3];
			EventJumpParam_273 = new SEventJumpParameter("闇の世界：土ボス後", "d26_09", arg111, arg2, 0, const_cast<short[]>(PossessionItem_273), 10000, const_cast<SCharacterParameter[]>(Party_273), -1, const_cast<short[]>(GlobalFlag_273), const_cast<short[]>(TreasureFlag_273));
			GlobalFlag_274 = new short[233]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 375, 368,
				369, 376, 377, 378, 387, 388, 389, 390, 396, 901,
				903, 904, 905, 906, 907, 910, 908, 909, 911, 912,
				913, 914, 915, 916, 917, 918, 919, 920, 921, 922,
				923, 902, 953, 951, 950, 952, 954, 955, 956, 957,
				958, 959, 960, 850, 851, 852, 853, 854, 855, 856,
				857, 858, -1
			};
			TreasureFlag_274 = new short[3] { 576, 577, -1 };
			PossessionItem_274 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_274 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 50, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 50, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 50, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 50, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg112 = new float[3] { 0f, 0f, -10f };
			arg2 = new float[3];
			EventJumpParam_274 = new SEventJumpParameter("闇の世界：ラスボス（フラグＮＧ）", "d26_11", arg112, arg2, 0, const_cast<short[]>(PossessionItem_274), 10000, const_cast<SCharacterParameter[]>(Party_274), -1, const_cast<short[]>(GlobalFlag_274), const_cast<short[]>(TreasureFlag_274));
			GlobalFlag_275 = new short[245]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 375, 368,
				369, 376, 377, 378, 387, 388, 389, 390, 396, 397,
				398, 399, 401, 402, 403, 405, 406, 407, 409, 410,
				411, 901, 903, 904, 905, 906, 907, 910, 908, 909,
				911, 912, 913, 914, 915, 916, 917, 918, 919, 920,
				921, 922, 923, 902, 953, 951, 950, 952, 954, 955,
				956, 957, 958, 959, 960, 850, 851, 852, 853, 854,
				855, 856, 857, 858, -1
			};
			TreasureFlag_275 = new short[3] { 576, 577, -1 };
			PossessionItem_275 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_275 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 50, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 50, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 50, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 50, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg113 = new float[3] { 0f, 0f, -10f };
			arg2 = new float[3];
			EventJumpParam_275 = new SEventJumpParameter("闇の世界：ラスボス（フラグＯＫ）", "d26_11", arg113, arg2, 0, const_cast<short[]>(PossessionItem_275), 10000, const_cast<SCharacterParameter[]>(Party_275), -1, const_cast<short[]>(GlobalFlag_275), const_cast<short[]>(TreasureFlag_275));
			GlobalFlag_276 = new short[246]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 375, 368,
				369, 376, 377, 378, 387, 388, 389, 390, 396, 397,
				398, 399, 401, 402, 403, 405, 406, 407, 409, 410,
				411, 416, 901, 903, 904, 905, 906, 907, 910, 908,
				909, 911, 912, 913, 914, 915, 916, 917, 918, 919,
				920, 921, 922, 923, 902, 953, 951, 950, 952, 954,
				955, 956, 957, 958, 959, 960, 850, 851, 852, 853,
				854, 855, 856, 857, 858, -1
			};
			TreasureFlag_276 = new short[3] { 576, 577, -1 };
			PossessionItem_276 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_276 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 50, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 50, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 50, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 50, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg114 = new float[3] { 0f, 0f, -10f };
			arg2 = new float[3];
			EventJumpParam_276 = new SEventJumpParameter("闇の世界：エンディング開始", "d26_11", arg114, arg2, 0, const_cast<short[]>(PossessionItem_276), 10000, const_cast<SCharacterParameter[]>(Party_276), -1, const_cast<short[]>(GlobalFlag_276), const_cast<short[]>(TreasureFlag_276));
			GlobalFlag_277 = new short[248]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 375, 368,
				369, 376, 377, 378, 387, 388, 389, 390, 396, 397,
				398, 399, 401, 402, 403, 405, 406, 407, 409, 410,
				411, 416, 439, 440, 901, 903, 904, 905, 906, 907,
				910, 908, 909, 911, 912, 913, 914, 915, 916, 917,
				918, 919, 920, 921, 922, 923, 902, 953, 951, 950,
				952, 954, 955, 956, 957, 958, 959, 960, 850, 851,
				852, 853, 854, 855, 856, 857, 858, -1
			};
			TreasureFlag_277 = new short[3] { 576, 577, -1 };
			PossessionItem_277 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_277 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg115 = new float[3] { 0f, 0f, -10f };
			arg2 = new float[3];
			EventJumpParam_277 = new SEventJumpParameter("エンディング：クリスタルタワー闇への道", "d25_10", arg115, arg2, 0, const_cast<short[]>(PossessionItem_277), 10000, const_cast<SCharacterParameter[]>(Party_277), -1, const_cast<short[]>(GlobalFlag_277), const_cast<short[]>(TreasureFlag_277));
			GlobalFlag_278 = new short[248]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 375, 368,
				369, 376, 377, 378, 387, 388, 389, 390, 396, 397,
				398, 399, 401, 402, 403, 405, 406, 407, 409, 410,
				411, 416, 439, 440, 901, 903, 904, 905, 906, 907,
				910, 908, 909, 911, 912, 913, 914, 915, 916, 917,
				918, 919, 920, 921, 922, 923, 902, 953, 951, 950,
				952, 954, 955, 956, 957, 958, 959, 960, 850, 851,
				852, 853, 854, 855, 856, 857, 858, -1
			};
			TreasureFlag_278 = new short[3] { 576, 577, -1 };
			PossessionItem_278 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_278 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg116 = new float[3] { -5f, 0f, -170f };
			arg2 = new float[3];
			EventJumpParam_278 = new SEventJumpParameter("エンディング：ワールドインビンシブル", "f03_01", arg116, arg2, 0, const_cast<short[]>(PossessionItem_278), 10000, const_cast<SCharacterParameter[]>(Party_278), -1, const_cast<short[]>(GlobalFlag_278), const_cast<short[]>(TreasureFlag_278));
			GlobalFlag_279 = new short[249]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 375, 368,
				369, 376, 377, 378, 387, 388, 389, 390, 396, 397,
				398, 399, 401, 402, 403, 405, 406, 407, 409, 410,
				411, 416, 439, 440, 441, 901, 903, 904, 905, 906,
				907, 910, 908, 909, 911, 912, 913, 914, 915, 916,
				917, 918, 919, 920, 921, 922, 923, 902, 953, 951,
				950, 952, 954, 955, 956, 957, 958, 959, 960, 850,
				851, 852, 853, 854, 855, 856, 857, 858, -1
			};
			TreasureFlag_279 = new short[3] { 576, 577, -1 };
			PossessionItem_279 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_279 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg117 = new float[3] { 80f, 0f, 0f };
			arg2 = new float[3];
			EventJumpParam_279 = new SEventJumpParameter("エンディング：インビンシブル内部", "t28_01", arg117, arg2, 0, const_cast<short[]>(PossessionItem_279), 10000, const_cast<SCharacterParameter[]>(Party_279), -1, const_cast<short[]>(GlobalFlag_279), const_cast<short[]>(TreasureFlag_279));
			GlobalFlag_280 = new short[250]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 375, 368,
				369, 376, 377, 378, 387, 388, 389, 390, 396, 397,
				398, 399, 401, 402, 403, 405, 406, 407, 409, 410,
				411, 416, 439, 440, 441, 442, 901, 903, 904, 905,
				906, 907, 910, 908, 909, 911, 912, 913, 914, 915,
				916, 917, 918, 919, 920, 921, 922, 923, 902, 953,
				951, 950, 952, 954, 955, 956, 957, 958, 959, 960,
				850, 851, 852, 853, 854, 855, 856, 857, 858, -1
			};
			TreasureFlag_280 = new short[3] { 576, 577, -1 };
			PossessionItem_280 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_280 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg118 = new float[3] { -5f, 0f, -170f };
			arg2 = new float[3];
			EventJumpParam_280 = new SEventJumpParameter("エンディング：アムル", "t17_01", arg118, arg2, 0, const_cast<short[]>(PossessionItem_280), 10000, const_cast<SCharacterParameter[]>(Party_280), -1, const_cast<short[]>(GlobalFlag_280), const_cast<short[]>(TreasureFlag_280));
			GlobalFlag_281 = new short[250]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 375, 368,
				369, 376, 377, 378, 387, 388, 389, 390, 396, 397,
				398, 399, 401, 402, 403, 405, 406, 407, 409, 410,
				411, 416, 439, 440, 441, 442, 901, 903, 904, 905,
				906, 907, 910, 908, 909, 911, 912, 913, 914, 915,
				916, 917, 918, 919, 920, 921, 922, 923, 902, 953,
				951, 950, 952, 954, 955, 956, 957, 958, 959, 960,
				850, 851, 852, 853, 854, 855, 856, 857, 858, -1
			};
			TreasureFlag_281 = new short[3] { 576, 577, -1 };
			PossessionItem_281 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_281 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg119 = new float[3] { 80f, 0f, 0f };
			arg2 = new float[3];
			EventJumpParam_281 = new SEventJumpParameter("エンディング：インビンシブル内部②", "t28_01", arg119, arg2, 0, const_cast<short[]>(PossessionItem_281), 10000, const_cast<SCharacterParameter[]>(Party_281), -1, const_cast<short[]>(GlobalFlag_281), const_cast<short[]>(TreasureFlag_281));
			GlobalFlag_282 = new short[250]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 375, 368,
				369, 376, 377, 378, 387, 388, 389, 390, 396, 397,
				398, 399, 401, 402, 403, 405, 406, 407, 409, 410,
				411, 416, 439, 440, 441, 442, 901, 903, 904, 905,
				906, 907, 910, 908, 909, 911, 912, 913, 914, 915,
				916, 917, 918, 919, 920, 921, 922, 923, 902, 953,
				951, 950, 952, 954, 955, 956, 957, 958, 959, 960,
				850, 851, 852, 853, 854, 855, 856, 857, 858, -1
			};
			TreasureFlag_282 = new short[3] { 576, 577, -1 };
			PossessionItem_282 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_282 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg120 = new float[3] { 0f, 0f, -10f };
			arg2 = new float[3];
			EventJumpParam_282 = new SEventJumpParameter("エンディング：サロニア王の部屋", "t24_04", arg120, arg2, 0, const_cast<short[]>(PossessionItem_282), 10000, const_cast<SCharacterParameter[]>(Party_282), -1, const_cast<short[]>(GlobalFlag_282), const_cast<short[]>(TreasureFlag_282));
			GlobalFlag_283 = new short[250]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 375, 368,
				369, 376, 377, 378, 387, 388, 389, 390, 396, 397,
				398, 399, 401, 402, 403, 405, 406, 407, 409, 410,
				411, 416, 439, 440, 441, 442, 901, 903, 904, 905,
				906, 907, 910, 908, 909, 911, 912, 913, 914, 915,
				916, 917, 918, 919, 920, 921, 922, 923, 902, 953,
				951, 950, 952, 954, 955, 956, 957, 958, 959, 960,
				850, 851, 852, 853, 854, 855, 856, 857, 858, -1
			};
			TreasureFlag_283 = new short[3] { 576, 577, -1 };
			PossessionItem_283 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_283 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg121 = new float[3] { -5f, 0f, -170f };
			arg2 = new float[3];
			EventJumpParam_283 = new SEventJumpParameter("エンディング：ワールドノーチラス→浮遊大陸", "f03_01", arg121, arg2, 0, const_cast<short[]>(PossessionItem_283), 10000, const_cast<SCharacterParameter[]>(Party_283), -1, const_cast<short[]>(GlobalFlag_283), const_cast<short[]>(TreasureFlag_283));
			GlobalFlag_284 = new short[250]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 375, 368,
				369, 376, 377, 378, 387, 388, 389, 390, 396, 397,
				398, 399, 401, 402, 403, 405, 406, 407, 409, 410,
				411, 416, 439, 440, 441, 442, 901, 903, 904, 905,
				906, 907, 910, 908, 909, 911, 912, 913, 914, 915,
				916, 917, 918, 919, 920, 921, 922, 923, 902, 953,
				951, 950, 952, 954, 955, 956, 957, 958, 959, 960,
				850, 851, 852, 853, 854, 855, 856, 857, 858, -1
			};
			TreasureFlag_284 = new short[3] { 576, 577, -1 };
			PossessionItem_284 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_284 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg122 = new float[3] { 0f, 0f, -10f };
			arg2 = new float[3];
			EventJumpParam_284 = new SEventJumpParameter("エンディング：ノーチラス内部（WT）", "world_talk_01", arg122, arg2, 0, const_cast<short[]>(PossessionItem_284), 10000, const_cast<SCharacterParameter[]>(Party_284), -1, const_cast<short[]>(GlobalFlag_284), const_cast<short[]>(TreasureFlag_284));
			GlobalFlag_285 = new short[251]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 375, 368,
				369, 376, 377, 378, 387, 388, 389, 390, 396, 397,
				398, 399, 401, 402, 403, 405, 406, 407, 409, 410,
				411, 416, 439, 440, 441, 442, 443, 901, 903, 904,
				905, 906, 907, 910, 908, 909, 911, 912, 913, 914,
				915, 916, 917, 918, 919, 920, 921, 922, 923, 902,
				953, 951, 950, 952, 954, 955, 956, 957, 958, 959,
				960, 850, 851, 852, 853, 854, 855, 856, 857, 858,
				-1
			};
			TreasureFlag_285 = new short[3] { 576, 577, -1 };
			PossessionItem_285 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_285 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg123 = new float[3] { -269f, 0f, 300f };
			arg2 = new float[3];
			EventJumpParam_285 = new SEventJumpParameter("エンディング：ワールドノーチラス②", "f01_32", arg123, arg2, 0, const_cast<short[]>(PossessionItem_285), 10000, const_cast<SCharacterParameter[]>(Party_285), -1, const_cast<short[]>(GlobalFlag_285), const_cast<short[]>(TreasureFlag_285));
			GlobalFlag_286 = new short[252]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 375, 368,
				369, 376, 377, 378, 387, 388, 389, 390, 396, 397,
				398, 399, 401, 402, 403, 405, 406, 407, 409, 410,
				411, 416, 439, 440, 441, 442, 443, 444, 901, 903,
				904, 905, 906, 907, 910, 908, 909, 911, 912, 913,
				914, 915, 916, 917, 918, 919, 920, 921, 922, 923,
				902, 953, 951, 950, 952, 954, 955, 956, 957, 958,
				959, 960, 850, 851, 852, 853, 854, 855, 856, 857,
				858, -1
			};
			TreasureFlag_286 = new short[3] { 576, 577, -1 };
			PossessionItem_286 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_286 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg124 = new float[3] { -10f, 0f, -200f };
			arg2 = new float[3];
			EventJumpParam_286 = new SEventJumpParameter("エンディング：カナーン", "t04_01", arg124, arg2, 0, const_cast<short[]>(PossessionItem_286), 10000, const_cast<SCharacterParameter[]>(Party_286), -1, const_cast<short[]>(GlobalFlag_286), const_cast<short[]>(TreasureFlag_286));
			GlobalFlag_287 = new short[252]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 375, 368,
				369, 376, 377, 378, 387, 388, 389, 390, 396, 397,
				398, 399, 401, 402, 403, 405, 406, 407, 409, 410,
				411, 416, 439, 440, 441, 442, 443, 444, 901, 903,
				904, 905, 906, 907, 910, 908, 909, 911, 912, 913,
				914, 915, 916, 917, 918, 919, 920, 921, 922, 923,
				902, 953, 951, 950, 952, 954, 955, 956, 957, 958,
				959, 960, 850, 851, 852, 853, 854, 855, 856, 857,
				858, -1
			};
			TreasureFlag_287 = new short[3] { 576, 577, -1 };
			PossessionItem_287 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_287 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg125 = new float[3] { -269f, 0f, 300f };
			arg2 = new float[3];
			EventJumpParam_287 = new SEventJumpParameter("エンディング：ワールドノーチラス③", "f01_32", arg125, arg2, 0, const_cast<short[]>(PossessionItem_287), 10000, const_cast<SCharacterParameter[]>(Party_287), -1, const_cast<short[]>(GlobalFlag_287), const_cast<short[]>(TreasureFlag_287));
			GlobalFlag_288 = new short[253]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 375, 368,
				369, 376, 377, 378, 387, 388, 389, 390, 396, 397,
				398, 399, 401, 402, 403, 405, 406, 407, 409, 410,
				411, 416, 439, 440, 441, 442, 443, 444, 445, 901,
				903, 904, 905, 906, 907, 910, 908, 909, 911, 912,
				913, 914, 915, 916, 917, 918, 919, 920, 921, 922,
				923, 902, 953, 951, 950, 952, 954, 955, 956, 957,
				958, 959, 960, 850, 851, 852, 853, 854, 855, 856,
				857, 858, -1
			};
			TreasureFlag_288 = new short[3] { 576, 577, -1 };
			PossessionItem_288 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_288 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg126 = new float[3] { 80f, 0f, 0f };
			arg2 = new float[3];
			EventJumpParam_288 = new SEventJumpParameter("エンディング：ノーチラス内部（WT）②", "world_talk_01", arg126, arg2, 0, const_cast<short[]>(PossessionItem_288), 10000, const_cast<SCharacterParameter[]>(Party_288), -1, const_cast<short[]>(GlobalFlag_288), const_cast<short[]>(TreasureFlag_288));
			GlobalFlag_289 = new short[253]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 375, 368,
				369, 376, 377, 378, 387, 388, 389, 390, 396, 397,
				398, 399, 401, 402, 403, 405, 406, 407, 409, 410,
				411, 416, 439, 440, 441, 442, 443, 444, 445, 901,
				903, 904, 905, 906, 907, 910, 908, 909, 911, 912,
				913, 914, 915, 916, 917, 918, 919, 920, 921, 922,
				923, 902, 953, 951, 950, 952, 954, 955, 956, 957,
				958, 959, 960, 850, 851, 852, 853, 854, 855, 856,
				857, 858, -1
			};
			TreasureFlag_289 = new short[3] { 576, 577, -1 };
			PossessionItem_289 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_289 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg127 = new float[3] { -269f, 0f, 300f };
			arg2 = new float[3];
			EventJumpParam_289 = new SEventJumpParameter("エンディング：ワールドノーチラス④", "f01_32", arg127, arg2, 0, const_cast<short[]>(PossessionItem_289), 10000, const_cast<SCharacterParameter[]>(Party_289), -1, const_cast<short[]>(GlobalFlag_289), const_cast<short[]>(TreasureFlag_289));
			GlobalFlag_290 = new short[253]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 375, 368,
				369, 376, 377, 378, 387, 388, 389, 390, 396, 397,
				398, 399, 401, 402, 403, 405, 406, 407, 409, 410,
				411, 416, 439, 440, 441, 442, 443, 444, 445, 901,
				903, 904, 905, 906, 907, 910, 908, 909, 911, 912,
				913, 914, 915, 916, 917, 918, 919, 920, 921, 922,
				923, 902, 953, 951, 950, 952, 954, 955, 956, 957,
				958, 959, 960, 850, 851, 852, 853, 854, 855, 856,
				857, 858, -1
			};
			TreasureFlag_290 = new short[3] { 576, 577, -1 };
			PossessionItem_290 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_290 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 60, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg128 = new float[3] { 0f, 0f, -170f };
			arg2 = new float[3];
			EventJumpParam_290 = new SEventJumpParameter("エンディング：ウルの夜明け", "t01_01", arg128, arg2, 0, const_cast<short[]>(PossessionItem_290), 10000, const_cast<SCharacterParameter[]>(Party_290), -1, const_cast<short[]>(GlobalFlag_290), const_cast<short[]>(TreasureFlag_290));
			GlobalFlag_291 = new short[222]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 901, 903,
				904, 905, 906, 907, 910, 908, 909, 911, 912, 913,
				914, 915, 916, 917, 918, 919, 920, 921, 922, 923,
				953, 951, 950, 952, 954, 955, 956, 957, 958, 959,
				960, 850, 851, 852, 853, 854, 855, 856, 857, 858,
				741, -1
			};
			TreasureFlag_291 = new short[1] { -1 };
			PossessionItem_291 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_291 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg129 = new float[3] { 3f, 0f, 120f };
			arg2 = new float[3];
			EventJumpParam_291 = new SEventJumpParameter("Wi-Fiイベント：トパパの前", "t01_03", arg129, arg2, 0, const_cast<short[]>(PossessionItem_291), 10000, const_cast<SCharacterParameter[]>(Party_291), -1, const_cast<short[]>(GlobalFlag_291), const_cast<short[]>(TreasureFlag_291));
			GlobalFlag_292 = new short[223]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 901, 903,
				904, 905, 906, 907, 910, 908, 909, 911, 912, 913,
				914, 915, 916, 917, 918, 919, 920, 921, 922, 923,
				953, 951, 950, 952, 954, 955, 956, 957, 958, 959,
				960, 850, 851, 852, 853, 854, 855, 856, 857, 858,
				741, 742, -1
			};
			TreasureFlag_292 = new short[1] { -1 };
			PossessionItem_292 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_292 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg130 = new float[3] { -19f, 0f, -43f };
			arg2 = new float[3];
			EventJumpParam_292 = new SEventJumpParameter("Wi-Fiイベント：祭壇の洞窟：救出前", "d01_02", arg130, arg2, 0, const_cast<short[]>(PossessionItem_292), 10000, const_cast<SCharacterParameter[]>(Party_292), -1, const_cast<short[]>(GlobalFlag_292), const_cast<short[]>(TreasureFlag_292));
			GlobalFlag_293 = new short[224]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 901, 903,
				904, 905, 906, 907, 910, 908, 909, 911, 912, 913,
				914, 915, 916, 917, 918, 919, 920, 921, 922, 923,
				953, 951, 950, 952, 954, 955, 956, 957, 958, 959,
				960, 850, 851, 852, 853, 854, 855, 856, 857, 858,
				741, 742, 743, -1
			};
			TreasureFlag_293 = new short[1] { -1 };
			PossessionItem_293 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_293 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg131 = new float[3] { -19f, 0f, -43f };
			arg2 = new float[3];
			EventJumpParam_293 = new SEventJumpParameter("Wi-Fiイベント：祭壇の洞窟：救出後", "d01_02", arg131, arg2, 0, const_cast<short[]>(PossessionItem_293), 10000, const_cast<SCharacterParameter[]>(Party_293), -1, const_cast<short[]>(GlobalFlag_293), const_cast<short[]>(TreasureFlag_293));
			GlobalFlag_294 = new short[225]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 901, 903,
				904, 905, 906, 907, 910, 908, 909, 911, 912, 913,
				914, 915, 916, 917, 918, 919, 920, 921, 922, 923,
				953, 951, 950, 952, 954, 955, 956, 957, 958, 959,
				960, 850, 851, 852, 853, 854, 855, 856, 857, 858,
				741, 742, 743, 744, -1
			};
			TreasureFlag_294 = new short[1] { -1 };
			PossessionItem_294 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_294 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg132 = new float[3] { -19f, 0f, -43f };
			arg2 = new float[3];
			EventJumpParam_294 = new SEventJumpParameter("Wi-Fiイベント：祭壇の洞窟：救出後フリー", "d01_02", arg132, arg2, 0, const_cast<short[]>(PossessionItem_294), 10000, const_cast<SCharacterParameter[]>(Party_294), -1, const_cast<short[]>(GlobalFlag_294), const_cast<short[]>(TreasureFlag_294));
			GlobalFlag_295 = new short[228]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 901, 903,
				904, 905, 906, 907, 910, 908, 909, 911, 912, 913,
				914, 915, 916, 917, 918, 919, 920, 921, 922, 923,
				953, 951, 950, 952, 954, 955, 956, 957, 958, 959,
				960, 850, 851, 852, 853, 854, 855, 856, 857, 858,
				741, 742, 743, 744, 745, 751, 761, -1
			};
			TreasureFlag_295 = new short[1] { -1 };
			PossessionItem_295 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_295 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg133 = new float[3] { 0f, 0f, -8f };
			arg2 = new float[3];
			EventJumpParam_295 = new SEventJumpParameter("Wi-Fiイベント：ペンダント受け取り前", "t03_10", arg133, arg2, 0, const_cast<short[]>(PossessionItem_295), 10000, const_cast<SCharacterParameter[]>(Party_295), -1, const_cast<short[]>(GlobalFlag_295), const_cast<short[]>(TreasureFlag_295));
			GlobalFlag_296 = new short[229]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 901, 903,
				904, 905, 906, 907, 910, 908, 909, 911, 912, 913,
				914, 915, 916, 917, 918, 919, 920, 921, 922, 923,
				953, 951, 950, 952, 954, 955, 956, 957, 958, 959,
				960, 850, 851, 852, 853, 854, 855, 856, 857, 858,
				741, 742, 743, 744, 745, 751, 761, 762, -1
			};
			TreasureFlag_296 = new short[1] { -1 };
			PossessionItem_296 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_296 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg134 = new float[3] { 10f, 0f, -10f };
			arg2 = new float[3];
			EventJumpParam_296 = new SEventJumpParameter("Wi-Fiイベント：カズス：タカの前", "t02_08", arg134, arg2, 0, const_cast<short[]>(PossessionItem_296), 10000, const_cast<SCharacterParameter[]>(Party_296), -1, const_cast<short[]>(GlobalFlag_296), const_cast<short[]>(TreasureFlag_296));
			GlobalFlag_297 = new short[229]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 901, 903,
				904, 905, 906, 907, 910, 908, 909, 911, 912, 913,
				914, 915, 916, 917, 918, 919, 920, 921, 922, 923,
				953, 951, 950, 952, 954, 955, 956, 957, 958, 959,
				960, 850, 851, 852, 853, 854, 855, 856, 857, 858,
				741, 742, 743, 744, 745, 751, 761, 762, -1
			};
			TreasureFlag_297 = new short[1] { -1 };
			PossessionItem_297 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_297 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg135 = new float[3] { 144f, 0f, -95f };
			arg2 = new float[3];
			EventJumpParam_297 = new SEventJumpParameter("Wi-Fiイベント：鍛冶屋前：フラグＮＧ", "t22_01", arg135, arg2, 0, const_cast<short[]>(PossessionItem_297), 10000, const_cast<SCharacterParameter[]>(Party_297), -1, const_cast<short[]>(GlobalFlag_297), const_cast<short[]>(TreasureFlag_297));
			GlobalFlag_298 = new short[230]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 901, 903,
				904, 905, 906, 907, 910, 908, 909, 911, 912, 913,
				914, 915, 916, 917, 918, 919, 920, 921, 922, 923,
				953, 951, 950, 952, 954, 955, 956, 957, 958, 959,
				960, 850, 851, 852, 853, 854, 855, 856, 857, 858,
				741, 742, 743, 744, 745, 751, 761, 762, 791, -1
			};
			TreasureFlag_298 = new short[1] { -1 };
			PossessionItem_298 = new short[8] { 4006, 5207, 4005, 5204, 5205, 5206, 5214, -1 };
			Party_298 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg136 = new float[3] { 144f, 0f, -95f };
			arg2 = new float[3];
			EventJumpParam_298 = new SEventJumpParameter("Wi-Fiイベント：鍛冶屋前：フラグＯＫ", "t22_01", arg136, arg2, 0, const_cast<short[]>(PossessionItem_298), 10000, const_cast<SCharacterParameter[]>(Party_298), -1, const_cast<short[]>(GlobalFlag_298), const_cast<short[]>(TreasureFlag_298));
			GlobalFlag_299 = new short[231]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 901, 903,
				904, 905, 906, 907, 910, 908, 909, 911, 912, 913,
				914, 915, 916, 917, 918, 919, 920, 921, 922, 923,
				953, 951, 950, 952, 954, 955, 956, 957, 958, 959,
				960, 850, 851, 852, 853, 854, 855, 856, 857, 858,
				741, 742, 743, 744, 745, 751, 761, 762, 763, 791,
				-1
			};
			TreasureFlag_299 = new short[1] { -1 };
			PossessionItem_299 = new short[8] { 4006, 5207, 4005, 5204, 5205, 5206, 5215, -1 };
			Party_299 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg137 = new float[3] { 0f, 0f, -8f };
			arg2 = new float[3];
			EventJumpParam_299 = new SEventJumpParameter("Wi-Fiイベント：サスーン：ペンダント修理後", "t03_10", arg137, arg2, 0, const_cast<short[]>(PossessionItem_299), 10000, const_cast<SCharacterParameter[]>(Party_299), -1, const_cast<short[]>(GlobalFlag_299), const_cast<short[]>(TreasureFlag_299));
			GlobalFlag_300 = new short[233]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 901, 903,
				904, 905, 906, 907, 910, 908, 909, 911, 912, 913,
				914, 915, 916, 917, 918, 919, 920, 921, 922, 923,
				953, 951, 950, 952, 954, 955, 956, 957, 958, 959,
				960, 850, 851, 852, 853, 854, 855, 856, 857, 858,
				741, 742, 743, 744, 745, 751, 761, 762, 763, 764,
				771, 791, -1
			};
			TreasureFlag_300 = new short[1] { -1 };
			PossessionItem_300 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_300 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg138 = new float[3] { 3f, 0f, -35f };
			arg2 = new float[3];
			EventJumpParam_300 = new SEventJumpParameter("Wi-Fiイベント：シドの家：依頼前", "t04_08", arg138, arg2, 0, const_cast<short[]>(PossessionItem_300), 10000, const_cast<SCharacterParameter[]>(Party_300), -1, const_cast<short[]>(GlobalFlag_300), const_cast<short[]>(TreasureFlag_300));
			GlobalFlag_301 = new short[234]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 901, 903,
				904, 905, 906, 907, 910, 908, 909, 911, 912, 913,
				914, 915, 916, 917, 918, 919, 920, 921, 922, 923,
				953, 951, 950, 952, 954, 955, 956, 957, 958, 959,
				960, 850, 851, 852, 853, 854, 855, 856, 857, 858,
				741, 742, 743, 744, 745, 751, 761, 762, 763, 764,
				771, 772, 791, -1
			};
			TreasureFlag_301 = new short[1] { -1 };
			PossessionItem_301 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_301 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg139 = new float[3] { 30f, 0f, 47f };
			arg2 = new float[3];
			EventJumpParam_301 = new SEventJumpParameter("Wi-Fiイベント：シドの家の地下：会話後", "t04_09", arg139, arg2, 0, const_cast<short[]>(PossessionItem_301), 10000, const_cast<SCharacterParameter[]>(Party_301), -1, const_cast<short[]>(GlobalFlag_301), const_cast<short[]>(TreasureFlag_301));
			GlobalFlag_302 = new short[235]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 901, 903,
				904, 905, 906, 907, 910, 908, 909, 911, 912, 913,
				914, 915, 916, 917, 918, 919, 920, 921, 922, 923,
				953, 951, 950, 952, 954, 955, 956, 957, 958, 959,
				960, 850, 851, 852, 853, 854, 855, 856, 857, 858,
				741, 742, 743, 744, 745, 751, 761, 762, 763, 764,
				771, 772, 773, 791, -1
			};
			TreasureFlag_302 = new short[1] { -1 };
			PossessionItem_302 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_302 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg140 = new float[3] { 30f, 0f, 47f };
			arg2 = new float[3];
			EventJumpParam_302 = new SEventJumpParameter("Wi-Fiイベント：シドの家の地下：ボス後", "t04_09", arg140, arg2, 0, const_cast<short[]>(PossessionItem_302), 10000, const_cast<SCharacterParameter[]>(Party_302), -1, const_cast<short[]>(GlobalFlag_302), const_cast<short[]>(TreasureFlag_302));
			GlobalFlag_303 = new short[237]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 901, 903,
				904, 905, 906, 907, 910, 908, 909, 911, 912, 913,
				914, 915, 916, 917, 918, 919, 920, 921, 922, 923,
				953, 951, 950, 952, 954, 955, 956, 957, 958, 959,
				960, 850, 851, 852, 853, 854, 855, 856, 857, 858,
				741, 742, 743, 744, 745, 751, 761, 762, 763, 764,
				771, 772, 773, 774, 775, 791, -1
			};
			TreasureFlag_303 = new short[1] { -1 };
			PossessionItem_303 = new short[8] { 4006, 5207, 4005, 5204, 5205, 5206, 5216, -1 };
			Party_303 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg141 = new float[3] { 3f, 0f, -35f };
			arg2 = new float[3];
			EventJumpParam_303 = new SEventJumpParameter("Wi-Fiイベント：シドの家：ボス後シド前", "t04_08", arg141, arg2, 0, const_cast<short[]>(PossessionItem_303), 10000, const_cast<SCharacterParameter[]>(Party_303), -1, const_cast<short[]>(GlobalFlag_303), const_cast<short[]>(TreasureFlag_303));
			GlobalFlag_304 = new short[239]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 375, 901,
				903, 904, 905, 906, 907, 910, 908, 909, 911, 912,
				913, 914, 915, 916, 917, 918, 919, 920, 921, 922,
				923, 953, 951, 950, 952, 954, 955, 956, 957, 958,
				959, 960, 850, 851, 852, 853, 854, 855, 856, 857,
				858, 741, 742, 743, 744, 745, 751, 761, 762, 763,
				764, 771, 772, 773, 774, 775, 776, 791, -1
			};
			TreasureFlag_304 = new short[1] { -1 };
			PossessionItem_304 = new short[8] { 4006, 5207, 4005, 5204, 5205, 5206, 5217, -1 };
			Party_304 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg142 = new float[3] { 32f, 0f, -45f };
			arg2 = new float[3];
			EventJumpParam_304 = new SEventJumpParameter("Wi-Fiイベント：オリハルコン渡す前", "t29_01", arg142, arg2, 0, const_cast<short[]>(PossessionItem_304), 10000, const_cast<SCharacterParameter[]>(Party_304), -1, const_cast<short[]>(GlobalFlag_304), const_cast<short[]>(TreasureFlag_304));
			GlobalFlag_305 = new short[239]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 901, 903,
				904, 905, 906, 907, 910, 908, 909, 911, 912, 913,
				914, 915, 916, 917, 918, 919, 920, 921, 922, 923,
				953, 951, 950, 952, 954, 955, 956, 957, 958, 959,
				960, 850, 851, 852, 853, 854, 855, 856, 857, 858,
				741, 742, 743, 744, 745, 751, 761, 762, 763, 764,
				771, 772, 773, 774, 775, 776, 752, 791, -1
			};
			TreasureFlag_305 = new short[1] { -1 };
			PossessionItem_305 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_305 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg143 = new float[3] { 32f, 0f, -45f };
			arg2 = new float[3];
			EventJumpParam_305 = new SEventJumpParameter("Wi-Fiイベント：渡した後：時間未経過", "t29_01", arg143, arg2, 0, const_cast<short[]>(PossessionItem_305), 10000, const_cast<SCharacterParameter[]>(Party_305), -1, const_cast<short[]>(GlobalFlag_305), const_cast<short[]>(TreasureFlag_305));
			GlobalFlag_306 = new short[240]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 901, 903,
				904, 905, 906, 907, 910, 908, 909, 911, 912, 913,
				914, 915, 916, 917, 918, 919, 920, 921, 922, 923,
				953, 951, 950, 952, 954, 955, 956, 957, 958, 959,
				960, 850, 851, 852, 853, 854, 855, 856, 857, 858,
				741, 742, 743, 744, 745, 751, 761, 762, 763, 764,
				771, 772, 773, 774, 775, 776, 752, 753, 791, -1
			};
			TreasureFlag_306 = new short[1] { -1 };
			PossessionItem_306 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_306 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg144 = new float[3] { 32f, 0f, -45f };
			arg2 = new float[3];
			EventJumpParam_306 = new SEventJumpParameter("Wi-Fiイベント：渡した後：時間経過後", "t29_01", arg144, arg2, 0, const_cast<short[]>(PossessionItem_306), 10000, const_cast<SCharacterParameter[]>(Party_306), -1, const_cast<short[]>(GlobalFlag_306), const_cast<short[]>(TreasureFlag_306));
			GlobalFlag_307 = new short[241]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 901, 903,
				904, 905, 906, 907, 910, 908, 909, 911, 912, 913,
				914, 915, 916, 917, 918, 919, 920, 921, 922, 923,
				953, 951, 950, 952, 954, 955, 956, 957, 958, 959,
				960, 850, 851, 852, 853, 854, 855, 856, 857, 858,
				741, 742, 743, 744, 745, 751, 761, 762, 763, 764,
				771, 772, 773, 774, 775, 776, 752, 753, 754, 791,
				-1
			};
			TreasureFlag_307 = new short[1] { -1 };
			PossessionItem_307 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_307 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg145 = new float[3] { 32f, 0f, -45f };
			arg2 = new float[3];
			EventJumpParam_307 = new SEventJumpParameter("Wi-Fiイベント：アルテマブレイド受け取り後", "t29_01", arg145, arg2, 0, const_cast<short[]>(PossessionItem_307), 10000, const_cast<SCharacterParameter[]>(Party_307), -1, const_cast<short[]>(GlobalFlag_307), const_cast<short[]>(TreasureFlag_307));
			GlobalFlag_308 = new short[242]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 901, 903,
				904, 905, 906, 907, 910, 908, 909, 911, 912, 913,
				914, 915, 916, 917, 918, 919, 920, 921, 922, 923,
				953, 951, 950, 952, 954, 955, 956, 957, 958, 959,
				960, 850, 851, 852, 853, 854, 855, 856, 857, 858,
				741, 742, 743, 744, 745, 751, 761, 762, 763, 764,
				771, 772, 773, 774, 775, 776, 752, 753, 754, 781,
				791, -1
			};
			TreasureFlag_308 = new short[1] { -1 };
			PossessionItem_308 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_308 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg146 = new float[3] { 110f, 0f, -926f };
			arg2 = new float[3];
			EventJumpParam_308 = new SEventJumpParameter("Wi-Fiイベント：隠しダンジョン解放後ワールド", "f03_8C", arg146, arg2, 0, const_cast<short[]>(PossessionItem_308), 10000, const_cast<SCharacterParameter[]>(Party_308), -1, const_cast<short[]>(GlobalFlag_308), const_cast<short[]>(TreasureFlag_308));
			GlobalFlag_309 = new short[242]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 901, 903,
				904, 905, 906, 907, 910, 908, 909, 911, 912, 913,
				914, 915, 916, 917, 918, 919, 920, 921, 922, 923,
				953, 951, 950, 952, 954, 955, 956, 957, 958, 959,
				960, 850, 851, 852, 853, 854, 855, 856, 857, 858,
				741, 742, 743, 744, 745, 751, 761, 762, 763, 764,
				771, 772, 773, 774, 775, 776, 752, 753, 754, 781,
				791, -1
			};
			TreasureFlag_309 = new short[1] { -1 };
			PossessionItem_309 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_309 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg147 = new float[3] { 6f, 0f, -50f };
			arg2 = new float[3];
			EventJumpParam_309 = new SEventJumpParameter("Wi-Fiイベント：隠しダンジョン：ボス前", "d28_01", arg147, arg2, 0, const_cast<short[]>(PossessionItem_309), 10000, const_cast<SCharacterParameter[]>(Party_309), -1, const_cast<short[]>(GlobalFlag_309), const_cast<short[]>(TreasureFlag_309));
			GlobalFlag_310 = new short[243]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 901, 903,
				904, 905, 906, 907, 910, 908, 909, 911, 912, 913,
				914, 915, 916, 917, 918, 919, 920, 921, 922, 923,
				953, 951, 950, 952, 954, 955, 956, 957, 958, 959,
				960, 850, 851, 852, 853, 854, 855, 856, 857, 858,
				741, 742, 743, 744, 745, 751, 761, 762, 763, 764,
				771, 772, 773, 774, 775, 776, 752, 753, 754, 781,
				782, 791, -1
			};
			TreasureFlag_310 = new short[1] { -1 };
			PossessionItem_310 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_310 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			float[] arg148 = new float[3] { 6f, 0f, -50f };
			arg2 = new float[3];
			EventJumpParam_310 = new SEventJumpParameter("Wi-Fiイベント：隠しダンジョン：ボス後", "d28_01", arg148, arg2, 0, const_cast<short[]>(PossessionItem_310), 10000, const_cast<SCharacterParameter[]>(Party_310), -1, const_cast<short[]>(GlobalFlag_310), const_cast<short[]>(TreasureFlag_310));
			GlobalFlag_311 = new short[244]
			{
				0, 1, 2, 3, 5, 6, 7, 10, 14, 11,
				15, 16, 18, 21, 22, 23, 24, 25, 26, 28,
				29, 30, 31, 32, 35, 36, 37, 39, 40, 41,
				43, 44, 45, 47, 46, 48, 49, 52, 54, 66,
				67, 70, 75, 76, 77, 80, 82, 88, 90, 91,
				92, 93, 94, 97, 100, 117, 118, 127, 130, 131,
				132, 133, 134, 135, 136, 137, 146, 147, 148, 152,
				153, 154, 157, 158, 159, 163, 164, 165, 166, 170,
				169, 171, 175, 177, 178, 179, 180, 186, 184, 185,
				189, 190, 193, 195, 200, 202, 209, 210, 211, 212,
				218, 219, 223, 224, 225, 239, 240, 242, 243, 244,
				249, 250, 251, 252, 253, 254, 259, 260, 262, 263,
				265, 276, 280, 281, 282, 286, 283, 284, 287, 288,
				290, 291, 293, 294, 295, 305, 306, 310, 311, 309,
				313, 315, 316, 314, 319, 320, 321, 322, 324, 325,
				326, 327, 328, 329, 330, 331, 333, 339, 338, 334,
				342, 343, 345, 346, 347, 348, 349, 350, 351, 352,
				353, 361, 362, 363, 364, 357, 358, 359, 440, 901,
				903, 904, 905, 906, 907, 910, 908, 909, 911, 912,
				913, 914, 915, 916, 917, 918, 919, 920, 921, 922,
				923, 953, 951, 950, 952, 954, 955, 956, 957, 958,
				959, 960, 850, 851, 852, 853, 854, 855, 856, 857,
				858, 741, 742, 743, 744, 745, 751, 761, 762, 763,
				764, 771, 772, 773, 774, 775, 776, 752, 753, 754,
				781, 782, 791, -1
			};
			TreasureFlag_311 = new short[1] { -1 };
			PossessionItem_311 = new short[7] { 4006, 5207, 4005, 5204, 5205, 5206, -1 };
			Party_311 = new SCharacterParameter[4]
			{
				new SCharacterParameter(0, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(1, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(2, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				}),
				new SCharacterParameter(3, 45, pl.JOB_TYPE.SUPPINN, new short[5] { -1, -1, -1, -1, -1 }, new short[8, 3]
				{
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 },
					{ -1, -1, -1 }
				})
			};
			arg2 = new float[3];
			float[] arg149 = arg2;
			arg2 = new float[3];
			EventJumpParam_311 = new SEventJumpParameter("スタッフロール", "t03_12", arg149, arg2, 0, const_cast<short[]>(PossessionItem_311), 10000, const_cast<SCharacterParameter[]>(Party_311), -1, const_cast<short[]>(GlobalFlag_311), const_cast<short[]>(TreasureFlag_311));
			EventJumpParamList = new SEventJumpParameter[312]
			{
				EventJumpParam_001,
				EventJumpParam_002,
				EventJumpParam_003,
				EventJumpParam_004,
				EventJumpParam_005,
				EventJumpParam_006,
				EventJumpParam_007,
				EventJumpParam_008,
				EventJumpParam_009,
				EventJumpParam_010,
				EventJumpParam_011,
				EventJumpParam_012,
				EventJumpParam_013,
				EventJumpParam_014,
				EventJumpParam_015,
				EventJumpParam_016,
				EventJumpParam_017,
				EventJumpParam_018,
				EventJumpParam_019,
				EventJumpParam_020,
				EventJumpParam_021,
				EventJumpParam_022,
				EventJumpParam_023,
				EventJumpParam_024,
				EventJumpParam_025,
				EventJumpParam_026,
				EventJumpParam_027,
				EventJumpParam_028,
				EventJumpParam_029,
				EventJumpParam_030,
				EventJumpParam_031,
				EventJumpParam_032,
				EventJumpParam_033,
				EventJumpParam_034,
				EventJumpParam_035,
				EventJumpParam_036,
				EventJumpParam_037,
				EventJumpParam_038,
				EventJumpParam_039,
				EventJumpParam_040,
				EventJumpParam_041,
				EventJumpParam_042,
				EventJumpParam_043,
				EventJumpParam_044,
				EventJumpParam_045,
				EventJumpParam_046,
				EventJumpParam_047,
				EventJumpParam_048,
				EventJumpParam_049,
				EventJumpParam_050,
				EventJumpParam_051,
				EventJumpParam_052,
				EventJumpParam_053,
				EventJumpParam_054,
				EventJumpParam_055,
				EventJumpParam_056,
				EventJumpParam_057,
				EventJumpParam_058,
				EventJumpParam_059,
				EventJumpParam_060,
				EventJumpParam_061,
				EventJumpParam_062,
				EventJumpParam_063,
				EventJumpParam_064,
				EventJumpParam_065,
				EventJumpParam_066,
				EventJumpParam_067,
				EventJumpParam_068,
				EventJumpParam_069,
				EventJumpParam_070,
				EventJumpParam_071,
				EventJumpParam_072,
				EventJumpParam_073,
				EventJumpParam_074,
				EventJumpParam_075,
				EventJumpParam_076,
				EventJumpParam_077,
				EventJumpParam_078,
				EventJumpParam_079,
				EventJumpParam_080,
				EventJumpParam_081,
				EventJumpParam_082,
				EventJumpParam_083,
				EventJumpParam_084,
				EventJumpParam_085,
				EventJumpParam_086,
				EventJumpParam_087,
				EventJumpParam_088,
				EventJumpParam_089,
				EventJumpParam_090,
				EventJumpParam_091,
				EventJumpParam_092,
				EventJumpParam_093,
				EventJumpParam_094,
				EventJumpParam_095,
				EventJumpParam_096,
				EventJumpParam_097,
				EventJumpParam_098,
				EventJumpParam_099,
				EventJumpParam_100,
				EventJumpParam_101,
				EventJumpParam_102,
				EventJumpParam_103,
				EventJumpParam_104,
				EventJumpParam_105,
				EventJumpParam_106,
				EventJumpParam_107,
				EventJumpParam_108,
				EventJumpParam_109,
				EventJumpParam_110,
				EventJumpParam_111,
				EventJumpParam_112,
				EventJumpParam_113,
				EventJumpParam_114,
				EventJumpParam_115,
				EventJumpParam_116,
				EventJumpParam_117,
				EventJumpParam_118,
				EventJumpParam_119,
				EventJumpParam_120,
				EventJumpParam_121,
				EventJumpParam_122,
				EventJumpParam_123,
				EventJumpParam_124,
				EventJumpParam_125,
				EventJumpParam_126,
				EventJumpParam_127,
				EventJumpParam_128,
				EventJumpParam_129,
				EventJumpParam_130,
				EventJumpParam_131,
				EventJumpParam_132,
				EventJumpParam_133,
				EventJumpParam_134,
				EventJumpParam_135,
				EventJumpParam_136,
				EventJumpParam_137,
				EventJumpParam_138,
				EventJumpParam_139,
				EventJumpParam_140,
				EventJumpParam_141,
				EventJumpParam_142,
				EventJumpParam_143,
				EventJumpParam_144,
				EventJumpParam_145,
				EventJumpParam_146,
				EventJumpParam_147,
				EventJumpParam_148,
				EventJumpParam_149,
				EventJumpParam_150,
				EventJumpParam_151,
				EventJumpParam_152,
				EventJumpParam_153,
				EventJumpParam_154,
				EventJumpParam_155,
				EventJumpParam_156,
				EventJumpParam_157,
				EventJumpParam_158,
				EventJumpParam_159,
				EventJumpParam_160,
				EventJumpParam_161,
				EventJumpParam_162,
				EventJumpParam_163,
				EventJumpParam_164,
				EventJumpParam_165,
				EventJumpParam_166,
				EventJumpParam_167,
				EventJumpParam_168,
				EventJumpParam_169,
				EventJumpParam_170,
				EventJumpParam_171,
				EventJumpParam_172,
				EventJumpParam_173,
				EventJumpParam_174,
				EventJumpParam_175,
				EventJumpParam_176,
				EventJumpParam_177,
				EventJumpParam_178,
				EventJumpParam_179,
				EventJumpParam_180,
				EventJumpParam_181,
				EventJumpParam_182,
				EventJumpParam_183,
				EventJumpParam_184,
				EventJumpParam_185,
				EventJumpParam_186,
				EventJumpParam_187,
				EventJumpParam_188,
				EventJumpParam_189,
				EventJumpParam_190,
				EventJumpParam_191,
				EventJumpParam_192,
				EventJumpParam_193,
				EventJumpParam_194,
				EventJumpParam_195,
				EventJumpParam_196,
				EventJumpParam_197,
				EventJumpParam_198,
				EventJumpParam_199,
				EventJumpParam_200,
				EventJumpParam_201,
				EventJumpParam_202,
				EventJumpParam_203,
				EventJumpParam_204,
				EventJumpParam_205,
				EventJumpParam_206,
				EventJumpParam_207,
				EventJumpParam_208,
				EventJumpParam_209,
				EventJumpParam_210,
				EventJumpParam_211,
				EventJumpParam_212,
				EventJumpParam_213,
				EventJumpParam_214,
				EventJumpParam_215,
				EventJumpParam_216,
				EventJumpParam_217,
				EventJumpParam_218,
				EventJumpParam_219,
				EventJumpParam_220,
				EventJumpParam_221,
				EventJumpParam_222,
				EventJumpParam_223,
				EventJumpParam_224,
				EventJumpParam_225,
				EventJumpParam_226,
				EventJumpParam_227,
				EventJumpParam_228,
				EventJumpParam_229,
				EventJumpParam_230,
				EventJumpParam_231,
				EventJumpParam_232,
				EventJumpParam_233,
				EventJumpParam_234,
				EventJumpParam_235,
				EventJumpParam_236,
				EventJumpParam_237,
				EventJumpParam_238,
				EventJumpParam_239,
				EventJumpParam_240,
				EventJumpParam_241,
				EventJumpParam_242,
				EventJumpParam_243,
				EventJumpParam_244,
				EventJumpParam_245,
				EventJumpParam_246,
				EventJumpParam_247,
				EventJumpParam_248,
				EventJumpParam_249,
				EventJumpParam_250,
				EventJumpParam_251,
				EventJumpParam_252,
				EventJumpParam_253,
				EventJumpParam_254,
				EventJumpParam_255,
				EventJumpParam_256,
				EventJumpParam_257,
				EventJumpParam_258,
				EventJumpParam_259,
				EventJumpParam_260,
				EventJumpParam_261,
				EventJumpParam_262,
				EventJumpParam_263,
				EventJumpParam_264,
				EventJumpParam_265,
				EventJumpParam_266,
				EventJumpParam_267,
				EventJumpParam_268,
				EventJumpParam_269,
				EventJumpParam_270,
				EventJumpParam_271,
				EventJumpParam_272,
				EventJumpParam_273,
				EventJumpParam_274,
				EventJumpParam_275,
				EventJumpParam_276,
				EventJumpParam_277,
				EventJumpParam_278,
				EventJumpParam_279,
				EventJumpParam_280,
				EventJumpParam_281,
				EventJumpParam_282,
				EventJumpParam_283,
				EventJumpParam_284,
				EventJumpParam_285,
				EventJumpParam_286,
				EventJumpParam_287,
				EventJumpParam_288,
				EventJumpParam_289,
				EventJumpParam_290,
				EventJumpParam_291,
				EventJumpParam_292,
				EventJumpParam_293,
				EventJumpParam_294,
				EventJumpParam_295,
				EventJumpParam_296,
				EventJumpParam_297,
				EventJumpParam_298,
				EventJumpParam_299,
				EventJumpParam_300,
				EventJumpParam_301,
				EventJumpParam_302,
				EventJumpParam_303,
				EventJumpParam_304,
				EventJumpParam_305,
				EventJumpParam_306,
				EventJumpParam_307,
				EventJumpParam_308,
				EventJumpParam_309,
				EventJumpParam_310,
				EventJumpParam_311,
				new SEventJumpParameter(".")
			};
			EVENT_CHAPTER_FLAG = new int[7] { 52, 136, 209, 225, 295, 364, 390 };
			EVENT_JOB_FLAG = new int[23]
			{
				901, 902, 903, 904, 905, 906, 907, 908, 909, 910,
				911, 912, 913, 914, 915, 916, 917, 918, 919, 920,
				921, 922, 923
			};
			EVENT_VEHICLE_FLAG = new int[11]
			{
				950, 951, 952, 953, 954, 955, 956, 957, 958, 959,
				960
			};
		}

	}
}
