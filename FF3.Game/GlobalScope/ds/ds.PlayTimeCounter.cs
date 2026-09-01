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
	public static partial class ds
	{
		public class PlayTimeCounter
		{
			public static uint PTC_MIN = 0u;

			public static uint PTC_MAX = 3599999u;

			public static int PTC_WORKING = 0;

			public static int PTC_PAUSE = 1;

			private RTCDate Date_ = new RTCDate();

			private RTCTime Time_ = new RTCTime();

			private uint nCount_;

			private int nState_;

			public PlayTimeCounter()
			{
				nCount_ = 0u;
				nState_ = 2;
			}

			public int start()
			{
				int num = RTC_GetDateTime(Date_, Time_);
				if (num == 0)
				{
					nState_ = PTC_WORKING;
				}
				return num;
			}

			public void pause(bool b)
			{
				if (b)
				{
					nState_ = PTC_PAUSE;
				}
				else if (!b)
				{
					nState_ = PTC_WORKING;
					RTCDate rTCDate = new RTCDate();
					RTCTime rTCTime = new RTCTime();
					if (RTC_GetDateTime(rTCDate, rTCTime) == 0)
					{
						Date_.copy(rTCDate);
						Time_.copy(rTCTime);
					}
				}
			}

			public void update()
			{
				RTCTime rTCTime = new RTCTime();
				RTCDate rTCDate = new RTCDate();
				if (RTC_GetDateTime(rTCDate, rTCTime) == 0 && PTC_WORKING == nState_)
				{
					long num = RTC_ConvertDateTimeToSecond(rTCDate, rTCTime);
					long num2 = RTC_ConvertDateTimeToSecond(Date_, Time_);
					if (-1 != num && -1 != num2)
					{
						nCount_ += (uint)(int)(num - num2);
						nCount_ %= PTC_MAX;
						Date_.copy(rTCDate);
						Time_.copy(rTCTime);
					}
				}
			}

			public void set(uint nCount)
			{
				nCount_ = nCount % PTC_MAX;
			}

			public uint get()
			{
				return nCount_;
			}

			public uint hour()
			{
				return secondToHH(nCount_);
			}

			public uint minute()
			{
				return secondToMM(nCount_);
			}

			public uint second()
			{
				return secondToSS(nCount_);
			}
		}
	}
}
