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
	public static partial class eld
	{
		public class Factory
		{
			protected Guid m_GUID = new Guid();

			~Factory()
			{
			}

			public virtual IObject createObj(Template pTemplate)
			{
				return null;
			}

			public Guid getGUID()
			{
				return m_GUID;
			}

			public virtual void initTemplate(Template pTemplate)
			{
			}

			public virtual void disposeTemplate(Template pTemplate)
			{
			}
		}
	}
}
