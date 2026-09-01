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
		public class CBaseState
		{
			public enum PHASE
			{
				START,
				UPDATE,
				END,
				PHASE_MAX
			}

			public const PHASE START = PHASE.START;

			public const PHASE UPDATE = PHASE.UPDATE;

			public const PHASE END = PHASE.END;

			public const PHASE PHASE_MAX = PHASE.PHASE_MAX;

			protected PHASE m_phase;

			public CBaseState()
			{
				m_phase = PHASE.START;
			}

			public void setPhase(PHASE _phase)
			{
				m_phase = _phase;
			}

			public PHASE phase()
			{
				return m_phase;
			}

			public void phase_set(PHASE arg0)
			{
				m_phase = arg0;
			}

			public virtual bool canExecuteEvent(CBaseSystem unuse0)
			{
				return false;
			}

			public virtual void start(CBaseSystem unuse0)
			{
			}

			public virtual void update(CBaseSystem unuse0)
			{
			}

			public virtual void end(CBaseSystem unuse0)
			{
			}
		}
	}
}
