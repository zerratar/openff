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
	public static partial class itm
	{
		public enum WEAPON_SYSTEM
		{
			WEAPON_UNARMED,
			WEAPON_KNIFE,
			WEAPON_SWORD,
			WEAPON_CLUB,
			WEAPON_MACE,
			WEAPON_STICK,
			WEAPON_ROD,
			WEAPON_BOW,
			WEAPON_ARROW,
			WEAPON_BOOK,
			WEAPON_CLAW,
			WEAPON_HAMMER,
			WEAPON_AXE,
			WEAPON_SPEAR,
			WEAPON_THROW,
			WEAPON_BELL,
			WEAPON_HARP,
			WEAPON_DARKSWORD,
			WEAPON_NINJA_STAR,
			WEAPON_SYSTEM_MAX,
			NON_WEAPON
		}
	}
}
