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
	public static partial class pl
	{
		public class CBaseNPCAi
		{
			protected bool m_Over;

			protected bool m_Wall;

			protected int m_WorkFrame;

			protected int m_SendAct;

			protected int m_Wait;

			protected VecFx32 m_TargetPos = new VecFx32();

			protected CBasePlayer m_pPlayer;

			protected CBasePlayer m_LookPlayer;

			public CBaseNPCAi()
			{
				m_Wall = false;
				m_WorkFrame = 0;
				m_SendAct = 0;
				m_Wait = 0;
				m_pPlayer = null;
				m_LookPlayer = null;
				VEC_Set(m_TargetPos, 0, 0, 0);
			}

			~CBaseNPCAi()
			{
			}

			public bool isOver()
			{
				return m_Over;
			}

			public bool isWall()
			{
				return m_Wall;
			}

			public int SendAct()
			{
				return m_SendAct;
			}

			public void setPtrPlayer(CBasePlayer _pPlayer)
			{
				m_pPlayer = _pPlayer;
			}

			public CBasePlayer PtrPlayer()
			{
				return m_pPlayer;
			}

			public void setLookPlayer(CBasePlayer _LookPlayer)
			{
				m_LookPlayer = _LookPlayer;
			}

			public CBasePlayer LookPlayer()
			{
				return m_LookPlayer;
			}

			public virtual void initialize(CBasePlayer arg0)
			{
			}

			public virtual void execute()
			{
			}

			public virtual void terminate()
			{
			}

			protected virtual void culcWaitFrame()
			{
			}
		}
	}
}
