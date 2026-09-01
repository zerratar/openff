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
	public static partial class wld
	{
							public class CParallelWorldSystem
							{
								public virtual void initialize(CBaseSystem unuse0)
								{
								}

								public virtual bool execute(CBaseSystem unuse0)
								{
									return false;
								}

								public virtual void terminate(CBaseSystem unuse0)
								{
								}
							}
	}
}
