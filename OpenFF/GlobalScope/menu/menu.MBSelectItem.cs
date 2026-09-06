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
	public static partial class menu
	{
		public class MBSelectItem : MenuBehavior
		{
			public static dgs.UniqueNumber MBSelectItem_UN = new dgs.UniqueNumber();

			public override bool bmDecide(Medget M)
			{
				if (mbNotifier != null)
				{
					mbNotifier.mbnNotify(this, (uint)MenuManager.getSingleton().getFocuseMedget().myTag(), 0u);
				}
				return true;
			}

			public override bool bmCancel(Medget M)
			{
				if (mbNotifier != null)
				{
					mbNotifier.mbnNotify(this, uint.MaxValue, 0u);
				}
				return true;
			}

			public new static int classIdentifier()
			{
				return MBSelectItem_UN.number();
			}

			public override object queryInterface(int class_id)
			{
				if (class_id == classIdentifier())
				{
					return this;
				}
				return null;
			}

			~MBSelectItem()
			{
			}

			public override void bmInitialize(Medget M)
			{
			}

			public override void bmBehave(Medget M)
			{
			}

			public override void bmFinalize(Medget M)
			{
			}
		}
	}
}
