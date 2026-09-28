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
	public class TEXT_DATA
	{
		public short x;

		public short y;

		public int color;

		public sbyte lcd;

		public sbyte priority;

		public short size;

		public uint flags;

		public string text;

		/// <summary>PORT: the text's own opacity, 0..255 (a menu style's opacity), over the screen's text alpha.</summary>
		public byte alpha = 255;

		/// <summary>PORT: the text's own lettering (MenuText): drawn by DrawStyledText; null for the game's.</summary>
		public OpenFF.Content.MenuText style;
	}
}
