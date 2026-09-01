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
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
	public class TPData
	{
		public short x;

		public short y;

		public ushort touch;

		public ushort validity;

		public TPState state;

		public int time;

		public ushort tap;

		public ushort drag;

		public ushort hold;

		public ushort cancel;

		public int pinch;

		public int prevPinch;

		public short dragX;

		public short dragY;

		public int flickPos;

		public int flickSpeed;

		public short flickOffset;

		public int flickPosH;

		public int flickSpeedH;

		public short flickOffsetH;

		public void copy(TPData src)
		{
			x = src.x;
			y = src.y;
			touch = src.touch;
			validity = src.validity;
			state = src.state;
			time = src.time;
			tap = src.tap;
			drag = src.drag;
			hold = src.hold;
			cancel = src.cancel;
			pinch = src.pinch;
			prevPinch = src.prevPinch;
			dragX = src.dragX;
			dragY = src.dragY;
			flickPos = src.flickPos;
			flickSpeed = src.flickSpeed;
			flickOffset = src.flickOffset;
			flickPosH = src.flickPosH;
			flickSpeedH = src.flickSpeedH;
			flickOffsetH = src.flickOffsetH;
		}

		public void setDefault()
		{
			x = 0;
			y = 0;
			touch = 0;
			validity = 0;
			state = TPState.TP_UP;
			time = 0;
			tap = 0;
			drag = 0;
			hold = 0;
			cancel = 0;
			pinch = 0;
			prevPinch = 0;
			dragX = 0;
			dragY = 0;
			flickPos = 0;
			flickSpeed = 0;
			flickOffset = 0;
			flickPosH = 0;
			flickSpeedH = 0;
			flickOffsetH = 0;
		}
	}
}
