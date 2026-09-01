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
	public static class snd
	{
		public class CSoundBGM
		{
			private ds.StrmHandle strmHandle_;

			public void playBGM(int _id, int loop)
			{
				UseSoundMngMessage();
			}

			public void stopBGM(int frame)
			{
				UseSoundMngMessage();
			}

			public void setVolumeBGM(int volume, int frame)
			{
				UseSoundMngMessage();
			}

			public bool isBGM()
			{
				UseSoundMngMessage();
				return false;
			}
		}

		public class CSoundSE
		{
			private ds.SEHandle seHandle_;

			public void registerSE(uint group_id)
			{
				UseSoundMngMessage();
			}

			public void unregisterSE()
			{
				UseSoundMngMessage();
			}

			public void playSE(int no, int index, int volume)
			{
				UseSoundMngMessage();
			}

			public void setVolumeSE(int volume)
			{
				UseSoundMngMessage();
			}

			public bool isSE()
			{
				UseSoundMngMessage();
				return false;
			}
		}

		public class CSound
		{
			private uint bgmVolume_;

			private uint seVolume_;

			public CSoundBGM composit = new CSoundBGM();

			public CSoundSE composit2 = new CSoundSE();

			public void setMute(bool mute)
			{
				UseSoundMngMessage();
			}
		}

		public class CSoundMng
		{
			public static CSoundMng instance_ = new CSoundMng();

			private CSound sound_;

			public void setup()
			{
				UseSoundMngMessage();
			}

			public void cleanup()
			{
				UseSoundMngMessage();
			}

			public uint getHeapSize()
			{
				UseSoundMngMessage();
				return uint.MaxValue;
			}

			public Array getHeapAddr()
			{
				UseSoundMngMessage();
				return null;
			}

			public static CSoundMng instance()
			{
				return instance_;
			}

			public CSound Sound()
			{
				return sound_;
			}
		}

		internal static void UseSoundMngMessage()
		{
		}
	}
}
