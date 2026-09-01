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
	public static partial class ds
	{
		public class DSSound
		{
			public const int SE_0 = 0;

			public const int SE_1 = 1;

			public const int SE_2 = 2;

			public const int SE_3 = 3;

			public const int SE_4 = 4;

			public const int SE_5 = 5;

			public const int SE_6 = 6;

			public const int SE_7 = 7;

			public static SEHandle[] SEHandles = new SEHandle[NUM_SE];

			public static StrmHandle[] StrmHandles = new StrmHandle[NUM_STRM];

			public static int NUM_SE = 8;

			public static int NUM_STRM = 4;

			public void PlaySE(int SeqArc, int Seq, int Handle)
			{
				SEHandles[Handle].Play(SeqArc, Seq, 127, 0);
			}

			public void StopSE(int Handle, int FadeFrame)
			{
				SEHandles[Handle].Stop(FadeFrame);
			}

			public bool IsPlayingSE(int Handle)
			{
				return SEHandles[Handle].IsPlaying();
			}

			public void PlayStrm(int Strm, int Handle)
			{
				StrmHandles[Handle].Prepare(Strm, bWaitPrepared: false, -1, -1, 0u);
				StrmHandles[Handle].Play();
			}

			public void StopStrm(int Handle, int FadeFrame)
			{
				StrmHandles[Handle].Stop(FadeFrame);
			}
		}
	}
}
