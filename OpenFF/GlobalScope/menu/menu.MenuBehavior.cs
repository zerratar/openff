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
	public static partial class menu
	{
		public class MenuBehavior : MenuBehaviorFactory
		{
			public Medget ownerMedget;

			protected ushort behaviorFlag;

			protected bool m_bActivity;

			protected MenuBehavedNotifier mbNotifier;

			public MenuBehavior()
				: base("")
			{
				ownerMedget = null;
				behaviorFlag = 0;
				m_bActivity = true;
				mbNotifier = null;
			}

			public MenuBehavior(string behavior_name)
				: base(behavior_name)
			{
				ownerMedget = null;
				behaviorFlag = 0;
				m_bActivity = true;
				mbNotifier = null;
			}

			public static int classIdentifier()
			{
				return dgs.UniqueNumber.INVALID_NUMBER;
			}

			public virtual object queryInterface(int class_id)
			{
				return null;
			}

			public virtual void mbDelete()
			{
			}

			public virtual void bmInitialize(Medget arg0)
			{
			}

			public virtual void bmPostInitialize(Medget arg0)
			{
			}

			public virtual void bmBehave(Medget arg0)
			{
			}

			public virtual void bmFinalize(Medget arg0)
			{
			}

			public virtual void bmSuspend(Medget arg0)
			{
			}

			public virtual void bmResume(Medget arg0)
			{
			}

			public virtual bool bmDecide(Medget arg0)
			{
				return false;
			}

			public virtual bool bmCancel(Medget arg0)
			{
				return false;
			}

			public virtual bool bmDirection(Medget arg0, int arg1)
			{
				return false;
			}

			public virtual bool bmUseTap(Medget arg0)
			{
				return false;
			}

			public virtual bool bmIsButton(Medget arg0)
			{
				return false;
			}

			public virtual int bmGetCursorX(Medget arg0)
			{
				return 0;
			}

			public virtual void bmSetPriority(int arg0)
			{
			}

			public virtual void bmActivate(Medget arg0)
			{
			}

			public virtual void bmDeactivate(Medget arg0)
			{
			}

			public virtual void mbTPPush(Medget arg0)
			{
			}

			public virtual void mbTPRelease(Medget arg0)
			{
			}

			public virtual void mbPause()
			{
				m_bActivity = false;
			}

			public virtual void mbRestart()
			{
				m_bActivity = true;
			}

			public virtual bool mbActivity()
			{
				return m_bActivity;
			}

			public Medget mbGetOwner()
			{
				return ownerMedget;
			}

			public virtual void mbSetPosition(short x, short y)
			{
			}

			public void mbSetNotifier(MenuBehavedNotifier n)
			{
				mbNotifier = n;
			}

			protected bool flagCheck(ushort f)
			{
				return (behaviorFlag & f) != 0;
			}

			protected void flagOn(ushort f)
			{
				behaviorFlag |= f;
			}

			protected void flagOff(ushort f)
			{
				behaviorFlag &= (ushort)(~f);
			}
		}
	}
}
