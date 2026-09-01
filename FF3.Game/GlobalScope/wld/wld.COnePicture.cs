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
using android.text;
using android.widget;
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
	public static partial class wld
	{
							public class COnePicture
							{
								private sys2d.Bg bg_ = new sys2d.Bg();

								public void setup(NNSG2dBGSelect _BGSelect, string _Name)
								{
									bg_.bgLoad2(_Name);
									bg_.bgSetUp(_BGSelect, GXBGScrBase.GX_BG_SCRBASE_0x7800, GXBGCharBase.GX_BG_CHARBASE_0x14000);
									bg_.bgRelease();
								}

								public void cleanup()
								{
								}
							}
	}
}
