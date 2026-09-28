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
	public class NNSG2dFont
	{
		public int size;

		/// <summary>PORT: a menu text's own lettering (Crystal Style Sheets: MenuText) - measured with it, so right and centred texts stand where they are drawn; null for the game's.</summary>
		public OpenFF.Content.MenuText style;
	}
}
