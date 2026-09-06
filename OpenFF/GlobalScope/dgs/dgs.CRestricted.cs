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
	public static partial class dgs
	{
		public class CRestricted : DGSLinkedList<CRestricted>
		{
			private bool m_bActivity;

			public CRestricted()
			{
				m_bActivity = false;
			}

			public bool redActivity()
			{
				return m_bActivity;
			}

			public void redSetActivity(bool _b)
			{
				if (_b)
				{
					if (!m_bActivity)
					{
						dgsllLink();
					}
				}
				else if (m_bActivity)
				{
					dgsllUnlink();
				}
				m_bActivity = _b;
			}

			public virtual void dgsredAccept(CRestrictor ror)
			{
			}

			public void copy(CRestricted src)
			{
				copy((DGSLinkedList<CRestricted>)src);
				m_bActivity = src.m_bActivity;
			}
		}
	}
}
