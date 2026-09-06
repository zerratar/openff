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
		public class ImpSequenceDSFactory : Factory
		{
			private static SGuid guid_data = new SGuid(1666259640u, 16261, 16726, new byte[8] { 188, 196, 138, 145, 142, 90, 157, 243 });

			public ImpSequenceDSFactory()
			{
				m_GUID.Set(guid_data);
			}

			public override IObject createObj(Template pTemplate)
			{
				EffAllocator<ImpSequenceDS> effAllocator = new EffAllocator<ImpSequenceDS>();
				ImpSequenceDS impSequenceDS = effAllocator.allocate(1u);
				if (impSequenceDS != null)
				{
					impSequenceDS.SetTemplate(pTemplate);
					if (!impSequenceDS.OneTimeInit())
					{
						effAllocator.deallocate(impSequenceDS);
						return null;
					}
				}
				return impSequenceDS;
			}

			public override void initTemplate(Template pTemplate)
			{
			}

			public override void disposeTemplate(Template pTemplate)
			{
			}
		}
	}
}
