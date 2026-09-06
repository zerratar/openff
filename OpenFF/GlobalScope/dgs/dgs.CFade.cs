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
	public static partial class dgs
	{
		public class CFade
		{
			public enum FADE_MODE
			{
				FADE_MODE_OUT,
				FADE_MODE_IN
			}

			public enum FADE_TYPE
			{
				FADE_TYPE_BLACK,
				FADE_TYPE_WHITE,
				FADE_TYPE_MAX
			}

			public const FADE_MODE FADE_MODE_OUT = FADE_MODE.FADE_MODE_OUT;

			public const FADE_MODE FADE_MODE_IN = FADE_MODE.FADE_MODE_IN;

			public const FADE_TYPE FADE_TYPE_BLACK = FADE_TYPE.FADE_TYPE_BLACK;

			public const FADE_TYPE FADE_TYPE_WHITE = FADE_TYPE.FADE_TYPE_WHITE;

			public const FADE_TYPE FADE_TYPE_MAX = FADE_TYPE.FADE_TYPE_MAX;

			public static CFade main = new CFade();

			public static CFade sub = new CFade();

			public static int MASTERBRIGHTNESS_MAX = 16;

			public static int MASTERBRIGHTNESS_MIN = -16;

			private bool flag;

			private int fadeTime;

			private int currentTime;

			private FADE_MODE fadeMode;

			private FADE_TYPE fadeType;

			private short Brightness;

			private short Brightness__;

			public CFade()
			{
				flag = false;
				Brightness = 0;
				Brightness__ = 0;
				currentTime = 0;
				fadeMode = FADE_MODE.FADE_MODE_IN;
				fadeType = FADE_TYPE.FADE_TYPE_BLACK;
			}

			public void SetUp()
			{
			}

			public void fadeOut(int time, FADE_TYPE type)
			{
				flag = true;
				fadeMode = FADE_MODE.FADE_MODE_OUT;
				fadeType = type;
				fadeTime = time;
				currentTime = 0;
				Brightness__ = Brightness;
			}

			public void fadeIn(int time)
			{
				flag = true;
				fadeMode = FADE_MODE.FADE_MODE_IN;
				fadeTime = time;
				currentTime = 0;
				Brightness__ = Brightness;
			}

			public static void execute()
			{
				main.ExecuteMain();
				sub.ExecuteSub();
			}

			public void ExecuteCommon()
			{
				int num = 4096;
				if (currentTime < fadeTime)
				{
					currentTime++;
					int num2 = currentTime * 4096;
					num = num2 / fadeTime;
				}
				switch (fadeMode)
				{
				case FADE_MODE.FADE_MODE_OUT:
				{
					int num3 = MASTERBRIGHTNESS_MAX;
					if (fadeType == FADE_TYPE.FADE_TYPE_WHITE)
					{
						num3 = MASTERBRIGHTNESS_MAX;
					}
					if (fadeType == FADE_TYPE.FADE_TYPE_BLACK)
					{
						num3 = MASTERBRIGHTNESS_MIN;
					}
					Brightness = (short)(Brightness__ + num3 * num / 4096);
					if ((fadeType == FADE_TYPE.FADE_TYPE_WHITE && Brightness >= num3) || (fadeType == FADE_TYPE.FADE_TYPE_BLACK && Brightness <= num3))
					{
						Brightness = (short)num3;
						flag = false;
					}
					break;
				}
				case FADE_MODE.FADE_MODE_IN:
					Brightness = (short)(Brightness__ - Brightness__ * num / 4096);
					if (Brightness == 0)
					{
						flag = false;
					}
					break;
				}
			}

			public void ExecuteMain()
			{
				if (flag)
				{
					ExecuteCommon();
					GX_SetMasterBrightness(Brightness);
				}
			}

			public void ExecuteSub()
			{
				if (flag)
				{
					ExecuteCommon();
					GXS_SetMasterBrightness(Brightness);
				}
			}

			public bool isFaded()
			{
				if (Brightness == 16 || Brightness == -16)
				{
					return true;
				}
				return false;
			}

			public bool isCleared()
			{
				if (Brightness != 0)
				{
					return false;
				}
				return true;
			}

			public static CFade Main()
			{
				return main;
			}

			public static CFade Sub()
			{
				return sub;
			}
		}
	}
}
