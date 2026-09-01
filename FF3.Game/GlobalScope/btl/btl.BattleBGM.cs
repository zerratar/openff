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
	public static partial class btl
	{
		public class BattleBGM
		{
			public const int NORMAL_BATTLE_BGM = 55;

			public const int EVENT_BATTLE_BGM = 55;

			public const int BOSS_BATTLE_BGM = 56;

			public const int LAST_BOSS_BATTLE_BGM = 57;

			public const int FANFARE = 58;

			public const int COMMON_FADE_FRAME = 15;

			public static BattleBGM instance_ = new BattleBGM();

			public void load(int bgm_no)
			{
				MatrixSound.MtxBGMNDS_Load(bgm_no);
			}

			public void free()
			{
				stop(0);
				MatrixSound.MtxBGMNDS_Unload();
			}

			public void play(int bgm_no, int fade)
			{
				MatrixSound.MtxSoundBGM.getSingleton().play(bgm_no, 192, fade, MatrixSound.enMtxBGMSlot.enMTX_BGM_SLOT0);
			}

			public void stop(int fade)
			{
				MatrixSound.MtxSoundBGM.getSingleton().stop(fade, MatrixSound.enMtxBGMSlot.enMTX_BGM_SLOT0);
			}

			public bool isStop()
			{
				if (MatrixSound.MtxSoundBGM.getSingleton().getState(MatrixSound.enMtxBGMSlot.enMTX_BGM_SLOT0) == MatrixSound.enMtxBGMState.enMTX_BGM_STOP)
				{
					return true;
				}
				return false;
			}

			public void loadAndPlay(int bgm_no, int fade)
			{
				load(bgm_no);
				play(bgm_no, fade);
			}

			public void startBattleBGM()
			{
				MatrixSound.MtxSoundBGM.getSingleton().stop(0, MatrixSound.enMtxBGMSlot.enMTX_BGM_SLOT0);
				switch (OutsideToBattle.getInstance().battleType())
				{
				case BATTLE_TYPE.NORMAL_BATTLE:
					loadAndPlay(55, 0);
					break;
				case BATTLE_TYPE.EVENT_BATTLE:
					loadAndPlay(55, 0);
					break;
				case BATTLE_TYPE.BOSS_BATTLE:
					loadAndPlay(56, 0);
					break;
				case BATTLE_TYPE.LAST_BOSS_BATTLE:
					loadAndPlay(57, 0);
					break;
				}
			}

			public static BattleBGM instance()
			{
				return instance_;
			}
		}
	}
}
