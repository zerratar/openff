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
						public enum GAMEPART
						{
							GAMEPART_DEBUG_MENU,
							GAMEPART_CAMPANY_LOGO,
							GAMEPART_TITLE,
							GAMEPART_BATTLE,
							GAMEPART_WORLD,
							GAMEPART_SPECIAL,
							GAMEPART_LOAD,
							GAMEPART_SUSPEND_LOAD,
							GAMEPART_MOG_NET,
							GAMEPART_MOVIE,
							GAMEPART_WIFI_UTIL,
							GAMEPART_LINK,
							GAMEPART_MAX
						}
}
