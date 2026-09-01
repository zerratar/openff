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
	public static class opt
	{
		public class CMessageOption
		{
			private MESSAGE_SPEED messageSpeed_;

			public void initialize()
			{
				messageSpeed_ = MESSAGE_SPEED.MESSAGE_SPEED_FAST;
			}

			public MESSAGE_SPEED messageSpeed()
			{
				return messageSpeed_;
			}

			public void setMessageSpeed(MESSAGE_SPEED ms)
			{
				messageSpeed_ = ms;
			}

			public void copy(CMessageOption src)
			{
				messageSpeed_ = src.messageSpeed_;
			}

			public void parse(ArrayReader reader)
			{
				messageSpeed_ = (MESSAGE_SPEED)reader.readInt32();
			}

			public void store(ArrayWriter writer)
			{
				writer.writeInt32((int)messageSpeed_);
			}
		}

		public class CCursorOption
		{
			private CURSOR_POSITION cursorPosition_;

			public void initialize()
			{
				cursorPosition_ = CURSOR_POSITION.CURSOR_POSITION_INITIALIZE;
			}

			public CURSOR_POSITION cursorPosition()
			{
				return cursorPosition_;
			}

			public void setCursorPosition(CURSOR_POSITION cp)
			{
				cursorPosition_ = cp;
			}

			public void copy(CCursorOption src)
			{
				cursorPosition_ = src.cursorPosition_;
			}

			public void parse(ArrayReader reader)
			{
				cursorPosition_ = (CURSOR_POSITION)reader.readInt32();
			}

			public void store(ArrayWriter writer)
			{
				writer.writeInt32((int)cursorPosition_);
			}
		}

		public class CSoundOption
		{
			private int bgmVolume_;

			private int seVolume_;

			public void initialize()
			{
				bgmVolume_ = 127;
				seVolume_ = 127;
			}

			public void setBgmVolume(int bv)
			{
				if (127 < bv)
				{
					bv = 127;
				}
				if (0 > bv)
				{
					bv = 0;
				}
				bgmVolume_ = bv;
				MatrixSound.MtxBGMNDS_SetBaseVolume(bgmVolume_);
			}

			public void setSeVolume(int sv)
			{
				if (127 < sv)
				{
					sv = 127;
				}
				if (0 > sv)
				{
					sv = 0;
				}
				seVolume_ = sv;
			}

			public int bgmVolume()
			{
				return bgmVolume_;
			}

			public int seVolume()
			{
				return seVolume_;
			}

			public int bgmVolumeMax()
			{
				return 127;
			}

			public int seVolumeMax()
			{
				return 127;
			}

			public int bgmVolumeMin()
			{
				return 0;
			}

			public int seVolumeMin()
			{
				return 0;
			}

			public void copy(CSoundOption src)
			{
				bgmVolume_ = src.bgmVolume_;
				seVolume_ = src.seVolume_;
			}

			public void parse(ArrayReader reader)
			{
				bgmVolume_ = reader.readInt32();
				seVolume_ = reader.readInt32();
			}

			public void store(ArrayWriter writer)
			{
				writer.writeInt32(bgmVolume_);
				writer.writeInt32(seVolume_);
			}
		}

		public class CGameOption
		{
			private SUMMONS_MAGIC_DIRECT summonsMagicDirect_;

			private WORLD_MOVE_TYPE worldMoveType_;

			private FIX_SCREEN_SETTING fixScreenSetting_;

			public void initialize()
			{
				summonsMagicDirect_ = SUMMONS_MAGIC_DIRECT.SUMMONS_MAGIC_DIRECT_ON;
				worldMoveType_ = WORLD_MOVE_TYPE.WORLD_MOVE_TYPE_WALK;
				fixScreenSetting_ = FIX_SCREEN_SETTING.FIX_SCREEN_OFF;
			}

			public void setFixScreenSetting(FIX_SCREEN_SETTING setting)
			{
				fixScreenSetting_ = setting;
				GX_FixScreen((int)setting);
			}

			public SUMMONS_MAGIC_DIRECT summonsMagicDirect()
			{
				return summonsMagicDirect_;
			}

			public WORLD_MOVE_TYPE worldMoveType()
			{
				return worldMoveType_;
			}

			public MENU_ZOOM_SETTING menuZoomSetting()
			{
				return MENU_ZOOM_SETTING.MENU_R_ZOOM_L;
			}

			public FIX_SCREEN_SETTING fixScreenSetting()
			{
				return fixScreenSetting_;
			}

			public void setSummonsMagicDirect(SUMMONS_MAGIC_DIRECT smd)
			{
				summonsMagicDirect_ = smd;
			}

			public void setWorldMoveType(WORLD_MOVE_TYPE wmt)
			{
				worldMoveType_ = wmt;
			}

			public void copy(CGameOption src)
			{
				summonsMagicDirect_ = src.summonsMagicDirect_;
				worldMoveType_ = src.worldMoveType_;
				fixScreenSetting_ = src.fixScreenSetting_;
			}

			public void parse(ArrayReader reader)
			{
				summonsMagicDirect_ = (SUMMONS_MAGIC_DIRECT)reader.readInt32();
				worldMoveType_ = (WORLD_MOVE_TYPE)reader.readInt32();
				fixScreenSetting_ = (FIX_SCREEN_SETTING)reader.readInt32();
			}

			public void store(ArrayWriter writer)
			{
				writer.writeInt32((int)summonsMagicDirect_);
				writer.writeInt32((int)worldMoveType_);
				writer.writeInt32((int)fixScreenSetting_);
			}
		}

		public class COptionManager
		{
			public static COptionManager instance_ = new COptionManager();

			private COptData OptData_ = new COptData();

			public void initialize()
			{
				OptData_.messageOption_.initialize();
				OptData_.cursorOption_.initialize();
				OptData_.soundOption_.initialize();
				OptData_.gameOption_.initialize();
			}

			public void storeSaveData(COptData pData)
			{
				pData?.copy(OptData_);
			}

			public void restoreSaveData(COptData pData)
			{
				if (pData != null)
				{
					OptData_.copy(pData);
					MatrixSound.MtxBGMNDS_SetBaseVolume(OptData_.soundOption_.bgmVolume());
					GX_FixScreen((int)OptData_.gameOption_.fixScreenSetting());
				}
			}

			public void dumpOption()
			{
			}

			public static COptionManager getSingleton()
			{
				return instance_;
			}

			public CMessageOption messageOption()
			{
				return OptData_.messageOption_;
			}

			public CCursorOption cursorOption()
			{
				return OptData_.cursorOption_;
			}

			public CSoundOption soundOption()
			{
				return OptData_.soundOption_;
			}

			public CGameOption gameOption()
			{
				return OptData_.gameOption_;
			}
		}

		public class COptData
		{
			public CMessageOption messageOption_ = new CMessageOption();

			public CCursorOption cursorOption_ = new CCursorOption();

			public CSoundOption soundOption_ = new CSoundOption();

			public CGameOption gameOption_ = new CGameOption();

			public void copy(COptData src)
			{
				messageOption_.copy(src.messageOption_);
				cursorOption_.copy(src.cursorOption_);
				soundOption_.copy(src.soundOption_);
				gameOption_.copy(src.gameOption_);
			}

			public void parse(ArrayReader reader)
			{
				messageOption_.parse(reader);
				cursorOption_.parse(reader);
				soundOption_.parse(reader);
				gameOption_.parse(reader);
			}

			public void store(ArrayWriter writer)
			{
				messageOption_.store(writer);
				cursorOption_.store(writer);
				soundOption_.store(writer);
				gameOption_.store(writer);
			}
		}

		public enum MESSAGE_SPEED
		{
			MESSAGE_SPEED_SLOW,
			MESSAGE_SPEED_NORMAL,
			MESSAGE_SPEED_FAST
		}

		public enum CURSOR_POSITION
		{
			CURSOR_POSITION_INITIALIZE,
			CURSOR_POSITION_MEMORY
		}

		public enum BGM_VOLUME
		{
			BGM_VOLUME_MIN = 0,
			BGM_VOLUME_DEFAULT = 63,
			BGM_VOLUME_MAX = 127
		}

		public enum SE_VOLUME
		{
			SE_VOLUME_MIN = 0,
			SE_VOLUME_DEFAULT = 63,
			SE_VOLUME_MAX = 127
		}

		public enum SUMMONS_MAGIC_DIRECT
		{
			SUMMONS_MAGIC_DIRECT_OFF,
			SUMMONS_MAGIC_DIRECT_ON
		}

		public enum WORLD_MOVE_TYPE
		{
			WORLD_MOVE_TYPE_WALK,
			WORLD_MOVE_TYPE_RUN
		}

		public enum MENU_ZOOM_SETTING
		{
			MENU_R_ZOOM_L,
			MENU_L_ZOOM_R
		}

		public enum FIX_SCREEN_SETTING
		{
			FIX_SCREEN_OFF,
			FIX_SCREEN_ON
		}

		public const MESSAGE_SPEED MESSAGE_SPEED_SLOW = MESSAGE_SPEED.MESSAGE_SPEED_SLOW;

		public const MESSAGE_SPEED MESSAGE_SPEED_NORMAL = MESSAGE_SPEED.MESSAGE_SPEED_NORMAL;

		public const MESSAGE_SPEED MESSAGE_SPEED_FAST = MESSAGE_SPEED.MESSAGE_SPEED_FAST;

		public const CURSOR_POSITION CURSOR_POSITION_INITIALIZE = CURSOR_POSITION.CURSOR_POSITION_INITIALIZE;

		public const CURSOR_POSITION CURSOR_POSITION_MEMORY = CURSOR_POSITION.CURSOR_POSITION_MEMORY;

		public const BGM_VOLUME BGM_VOLUME_MIN = BGM_VOLUME.BGM_VOLUME_MIN;

		public const BGM_VOLUME BGM_VOLUME_DEFAULT = BGM_VOLUME.BGM_VOLUME_DEFAULT;

		public const BGM_VOLUME BGM_VOLUME_MAX = BGM_VOLUME.BGM_VOLUME_MAX;

		public const SE_VOLUME SE_VOLUME_MIN = SE_VOLUME.SE_VOLUME_MIN;

		public const SE_VOLUME SE_VOLUME_DEFAULT = SE_VOLUME.SE_VOLUME_DEFAULT;

		public const SE_VOLUME SE_VOLUME_MAX = SE_VOLUME.SE_VOLUME_MAX;

		public const SUMMONS_MAGIC_DIRECT SUMMONS_MAGIC_DIRECT_OFF = SUMMONS_MAGIC_DIRECT.SUMMONS_MAGIC_DIRECT_OFF;

		public const SUMMONS_MAGIC_DIRECT SUMMONS_MAGIC_DIRECT_ON = SUMMONS_MAGIC_DIRECT.SUMMONS_MAGIC_DIRECT_ON;

		public const WORLD_MOVE_TYPE WORLD_MOVE_TYPE_WALK = WORLD_MOVE_TYPE.WORLD_MOVE_TYPE_WALK;

		public const WORLD_MOVE_TYPE WORLD_MOVE_TYPE_RUN = WORLD_MOVE_TYPE.WORLD_MOVE_TYPE_RUN;

		public const MENU_ZOOM_SETTING MENU_R_ZOOM_L = MENU_ZOOM_SETTING.MENU_R_ZOOM_L;

		public const MENU_ZOOM_SETTING MENU_L_ZOOM_R = MENU_ZOOM_SETTING.MENU_L_ZOOM_R;

		public const FIX_SCREEN_SETTING FIX_SCREEN_OFF = FIX_SCREEN_SETTING.FIX_SCREEN_OFF;

		public const FIX_SCREEN_SETTING FIX_SCREEN_ON = FIX_SCREEN_SETTING.FIX_SCREEN_ON;
	}
}
