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
	public static class act
	{
		public class CActionManager
		{
			private CBaseAction m_pAction;

			public void initialize()
			{
				m_pAction = null;
			}

			public void execute()
			{
				updateAction();
			}

			public void terminate()
			{
				m_pAction = null;
			}

			public void setAction(chr.CCharacterEureka _Chara, CBaseAction _Action)
			{
				if (m_pAction != null)
				{
					m_pAction.end();
				}
				m_pAction = _Action;
				m_pAction.m_End = false;
				m_pAction.m_pCharacter = _Chara;
				m_pAction.start();
			}

			public void endAction()
			{
				if (m_pAction != null)
				{
					m_pAction.end();
				}
			}

			public void updateAction()
			{
				if (m_pAction != null)
				{
					m_pAction.update();
					if (m_pAction.isEnd())
					{
						endAction();
					}
				}
			}

			public CActionManager()
			{
				m_pAction = null;
			}

			public CBaseAction getAction()
			{
				return m_pAction;
			}
		}

		public class CBaseAction
		{
			public bool m_End;

			public chr.CCharacterEureka m_pCharacter;

			public void terminate()
			{
				m_End = true;
			}

			public CBaseAction()
			{
				m_End = false;
				m_pCharacter = null;
			}

			public void destruct()
			{
			}

			public bool isEnd()
			{
				return m_End;
			}

			public chr.CCharacterEureka getCharacter()
			{
				return m_pCharacter;
			}

			public chr.CCharacterEureka character()
			{
				return m_pCharacter;
			}

			public virtual void start()
			{
			}

			public virtual void update()
			{
			}

			public virtual void end()
			{
			}
		}
	}
}
