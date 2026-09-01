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
	public static partial class menu
	{
		public class MBSuspend : MenuBehavior
		{
			public const int MBSUSPEND_DECIDE = 0;

			public const int MBSUSPEND_CANCEL = 1;

			public static dgs.UniqueNumber MBSuspend_UN = new dgs.UniqueNumber();

			~MBSuspend()
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

			public override bool bmDecide(Medget M)
			{
				if (mbNotifier != null)
				{
					mbNotifier.mbnNotify(this, 0u, 0u);
				}
				return true;
			}

			public override bool bmCancel(Medget M)
			{
				if (mbNotifier != null)
				{
					mbNotifier.mbnNotify(this, 1u, 0u);
				}
				return true;
			}

			public new static int classIdentifier()
			{
				return MBSuspend_UN.number();
			}

			public override object queryInterface(int class_id)
			{
				if (class_id == classIdentifier())
				{
					return this;
				}
				return null;
			}
		}
	}
}
