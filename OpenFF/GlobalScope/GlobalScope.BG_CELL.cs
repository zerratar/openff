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
	public class BG_CELL
	{
		public uint image;

		public float texScaleU;

		public float texScaleV;

		public float scale;

		/// <summary>PORT: a picture to keep its own shape (a hero's face in the menus): its widths drawn by the window's
		/// square-pixel share (Ff4Ui.SquareX), from its left, rather than stretched with the 800 x 480 space.</summary>
		public bool Square;

		public byte[] color = new byte[4];

		public int x;

		public int y;

		public short[] oam;

		public int numOAM;

		public NNSG2dBGSelect bg;
	}
}
