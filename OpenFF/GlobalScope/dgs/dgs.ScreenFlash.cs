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
		public class ScreenFlash : sys2d.PrimitiveQuadDraw
		{
			public static uint FLAG_ENABLE = 1u;

			public static uint FLAG_FLASHING = 2u;

			public static uint FLAG_ON = 4u;

			private static int DEFAULT_DEPTH = 0;

			private uint _flag;

			private short _flashFrame;

			private short _intervalFrame;

			private short _flashFrameCount;

			private short _totalFrame;

			private short _totalFrameCount;

			public ScreenFlash()
			{
				initValue();
			}

			public void initialize()
			{
				initValue();
			}

			public void terminate()
			{
			}

			public override void draw()
			{
				if ((_flag & FLAG_FLASHING) == 0)
				{
					return;
				}
				if ((_flag & FLAG_ON) != 0)
				{
					_flashFrameCount++;
					if (_flashFrameCount > _flashFrame)
					{
						_flashFrameCount = 1;
						_flag &= ~FLAG_ON;
					}
				}
				else
				{
					_flashFrameCount++;
					if (_flashFrameCount > _intervalFrame)
					{
						_flashFrameCount = 1;
						_flag |= FLAG_ON;
					}
				}
				if (-1 != _totalFrame)
				{
					_totalFrameCount++;
					if (_totalFrameCount > _totalFrame)
					{
						_flag &= ~FLAG_ON;
						_flag &= ~FLAG_FLASHING;
					}
				}
				if ((_flag & FLAG_ON) != 0)
				{
					base.draw();
				}
			}

			public void setFlash(short frame, byte interval, ushort col)
			{
				if (-1 <= frame && interval != 0)
				{
					_flag = FLAG_ENABLE | FLAG_FLASHING | FLAG_ON;
					_totalFrame = frame;
					_totalFrameCount = 0;
					_flashFrame = interval;
					_intervalFrame = interval;
					_flashFrameCount = 0;
					setColor(col, col, col, col);
				}
			}

			public void setFlashEx(byte flashNum, short flashFrame, short intervalFrame, ushort col)
			{
				_flag = FLAG_ENABLE | FLAG_FLASHING | FLAG_ON;
				_totalFrame = (short)(flashNum * (flashFrame + intervalFrame));
				_totalFrameCount = 0;
				_flashFrame = flashFrame;
				_intervalFrame = intervalFrame;
				_flashFrameCount = 0;
				setColor(col, col, col, col);
			}

			public void initValue()
			{
				_flag = FLAG_ENABLE;
				_flashFrame = 0;
				_intervalFrame = 0;
				_flashFrameCount = 0;
				_totalFrame = 0;
				_totalFrameCount = 0;
				ds.Vector2<short> vector = new ds.Vector2<short>((short)((480 - LCD_WIDTH) / 2), (short)((320 - LCD_HEIGHT) / 2));
				ds.Vector2<short> vector2 = new ds.Vector2<short>((short)((480 + LCD_WIDTH) / 2), (short)((320 + LCD_HEIGHT) / 2));
				ushort num = GX_RGB(31, 31, 31);
				vector.vx--;
				vector.vy--;
				vector2.vx++;
				vector2.vy++;
				setPosition(vector, vector2);
				setDepth(DEFAULT_DEPTH);
				setColor(num, num, num, num);
				setAlpha(31);
			}

			public bool isFlash()
			{
				return (_flag & FLAG_FLASHING) != 0;
			}

			public void endFlash()
			{
				_flag &= ~FLAG_FLASHING;
			}
		}
	}
}
