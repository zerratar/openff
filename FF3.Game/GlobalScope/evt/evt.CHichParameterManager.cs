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
	public static partial class evt
	{
		public class CHichParameterManager : CHichManParameter
		{
			public enum HICH_KIND
			{
				HICH_ERR = -1,
				HICH_MAN,
				HICH_PARAM_KIND_MAX
			}

			private delegate void _hichSetUp(Array arg0);

			public const HICH_KIND HICH_ERR = HICH_KIND.HICH_ERR;

			public const HICH_KIND HICH_MAN = HICH_KIND.HICH_MAN;

			public const HICH_KIND HICH_PARAM_KIND_MAX = HICH_KIND.HICH_PARAM_KIND_MAX;

			public static CHichParameterManager m_Instance = new CHichParameterManager();

			private _hichSetUp[] hichSetUp = new _hichSetUp[1];

			public static CHichParameterManager getInstance()
			{
				return m_Instance;
			}

			public HICH_MAN_COMPOSITE getHichManParam()
			{
				return getParam();
			}

			public HICH_MAN_INDIVIDUAL getHichManSubParam(int _Index)
			{
				return getSubParam(_Index);
			}

			public new void initialize()
			{
				base.initialize();
				hichSetUp[0] = base.setUp;
			}

			public void setUpAll(Array _Addr_A)
			{
				setUp(0u, _Addr_A);
			}

			public void setUp(uint _Index, Array _Addr)
			{
				hichSetUp[_Index](_Addr);
			}
		}
	}
}
