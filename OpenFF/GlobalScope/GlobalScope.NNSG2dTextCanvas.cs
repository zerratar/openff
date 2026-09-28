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
	public class NNSG2dTextCanvas
	{
		public NNSG2dCharCanvas pCanvas;

		public NNSG2dFont pFont;

		public int hSpace;

		public int vSpace;

		/// <summary>PORT: a colour of the text's own (0xRRGGBBAA, a menu style's #hex) over the palette entry it is drawn in.</summary>
		public uint? rgba;

		/// <summary>PORT: the text's opacity, 0..255 (a menu style's opacity).</summary>
		public byte alpha = 255;

		/// <summary>PORT: the text's own lettering (a menu style's text-shadow, font-family and the rest: MenuText); null for the game's.</summary>
		public OpenFF.Content.MenuText style;
	}
}
