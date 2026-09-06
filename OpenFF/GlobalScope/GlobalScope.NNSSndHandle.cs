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
	public class NNSSndHandle
	{
		public uint source;

		public uint[] buffer = new uint[2];

		public uint flag;

		public int request;

		public int volume;

		public int player;

		public int fadeFrame;

		public int fadeTime;

		public int[] fadeVolume = new int[2];

		public NNSSndHandle next;

		public void setDefault()
		{
			source = 0u;
			buffer[0] = 0u;
			buffer[1] = 0u;
			flag = 0u;
			request = 0;
			volume = 0;
			player = 0;
			fadeFrame = 0;
			fadeTime = 0;
			fadeVolume[0] = 0;
			fadeVolume[1] = 0;
		}
	}
}
