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
	public static partial class mognet
	{
		public class MogNetState : menu.MenuBehavior
		{
			public MNSMediator ownerMediator;

			public MogNetState()
			{
				ownerMediator = null;
			}

			public MogNetState(string behavior_name)
				: base(behavior_name)
			{
				ownerMediator = null;
			}

			public virtual void mnsInitialize(MNSMediator unuse0)
			{
			}

			public virtual bool mnsProcess(MNSMediator unuse0)
			{
				return false;
			}

			public virtual void mnsTerminate(MNSMediator unuse0)
			{
			}

			public virtual bool mnsDecide(MNSMediator unuse0)
			{
				return false;
			}

			public virtual bool mnsCancel(MNSMediator unuse0)
			{
				return false;
			}
		}
	}
}
