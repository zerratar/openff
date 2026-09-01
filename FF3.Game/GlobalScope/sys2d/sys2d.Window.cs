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
using java.io;
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
	public static partial class sys2d
	{
		public class Window
		{
			public enum WINDOW_STATE
			{
				wsCLOSED,
				wsOPEN,
				wsOPENED,
				wsCLOSE,
				wsMAX,
				wsINVALID
			}

			public const WINDOW_STATE wsCLOSED = WINDOW_STATE.wsCLOSED;

			public const WINDOW_STATE wsOPEN = WINDOW_STATE.wsOPEN;

			public const WINDOW_STATE wsOPENED = WINDOW_STATE.wsOPENED;

			public const WINDOW_STATE wsCLOSE = WINDOW_STATE.wsCLOSE;

			public const WINDOW_STATE wsMAX = WINDOW_STATE.wsMAX;

			public const WINDOW_STATE wsINVALID = WINDOW_STATE.wsINVALID;

			public static byte WINDOW_SYSTEM_SHOW = 1;

			public static byte WINDOW_USER_SHOW = 2;

			public static byte WINDOW_SHOW = (byte)(WINDOW_SYSTEM_SHOW | WINDOW_USER_SHOW);

			protected ds.Vector2<short> m_Position = new ds.Vector2<short>();

			protected ds.Vector2<short> m_Size = new ds.Vector2<short>();

			protected byte m_Show;

			protected byte m_Priority;

			protected int m_Depth;

			protected ds.Vector2<short> m_PositionWork = new ds.Vector2<short>();

			private WINDOW_STATE m_State;

			~Window()
			{
			}

			public virtual void Initialize()
			{
				m_Show = WINDOW_SHOW;
			}

			public bool CreateCC(ds.Vector2<short> cc, ds.Vector2<short> size)
			{
				m_Position.copy(cc);
				m_Size.copy(size);
				m_Show = WINDOW_SHOW;
				m_State = WINDOW_STATE.wsOPENED;
				return true;
			}

			public virtual void Release()
			{
				m_State = WINDOW_STATE.wsCLOSED;
			}

			public virtual void Kill()
			{
				m_State = WINDOW_STATE.wsCLOSED;
			}

			public WINDOW_STATE GetState()
			{
				return m_State;
			}

			public virtual ds.Vector2<short> GetPositionCC()
			{
				return m_Position;
			}

			public virtual ds.Vector2<short> GetPositionUL()
			{
				return GetPositionULfromCC(m_Position, m_Size);
			}

			public virtual ds.Vector2<short> GetSize()
			{
				return m_Size;
			}

			public virtual byte GetPriority()
			{
				return m_Priority;
			}

			public virtual int GetDepth()
			{
				return m_Depth;
			}

			public virtual bool IsShow()
			{
				if (m_Show != WINDOW_SHOW)
				{
					return false;
				}
				return true;
			}

			protected virtual void SetPositionCC(ds.Vector2<short> cc)
			{
				m_Position.copy(cc);
			}

			public virtual void SetPositionUL(ds.Vector2<short> ul)
			{
				SetPositionCC(GetPositionCCfromUL(ul, m_Size));
			}

			public virtual void SetSize(ds.Vector2<short> size, bool update)
			{
				m_Size.copy(size);
			}

			public virtual void SetPriority(byte pri)
			{
			}

			protected virtual void SetDepth(int depth)
			{
			}

			public virtual void SetShow(bool show, bool user)
			{
				if (user)
				{
					if (show)
					{
						m_Show |= WINDOW_USER_SHOW;
					}
					else
					{
						m_Show &= (byte)(~WINDOW_USER_SHOW);
					}
				}
				else if (show)
				{
					m_Show |= WINDOW_SYSTEM_SHOW;
				}
				else
				{
					m_Show &= (byte)(~WINDOW_SYSTEM_SHOW);
				}
			}

			protected ds.Vector2<short> GetPositionULfromCC(ds.Vector2<short> cc, ds.Vector2<short> size)
			{
				ds.Vector2<short> positionWork = m_PositionWork;
				positionWork.set(size.vx, size.vy);
				positionWork.vx /= -2;
				positionWork.vy /= -2;
				positionWork.vx += cc.vx;
				positionWork.vy += cc.vy;
				return positionWork;
			}

			protected ds.Vector2<short> GetPositionCCfromUL(ds.Vector2<short> ul, ds.Vector2<short> size)
			{
				ds.Vector2<short> positionWork = m_PositionWork;
				positionWork.set(size.vx, size.vy);
				positionWork.vx /= 2;
				positionWork.vy /= 2;
				positionWork.vx += ul.vx;
				positionWork.vy += ul.vy;
				return positionWork;
			}

			protected bool CreateUL(ds.Vector2<short> ul, ds.Vector2<short> size)
			{
				ds.Vector2<short> positionWork = m_PositionWork;
				positionWork.set(size.vx, size.vy);
				positionWork.vx /= 2;
				positionWork.vy /= 2;
				positionWork.vx += ul.vx;
				positionWork.vy += ul.vy;
				return CreateCC(positionWork, size);
			}
		}
	}
}
