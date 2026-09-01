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
		public class MBMonsterBossMark : MenuBehavior
		{
			public static dgs.UniqueNumber MBMonsterBossMark_UN = new dgs.UniqueNumber();

			public static sys2d.Cell commonCell_ = new sys2d.Cell();

			private sys2d.Cell cell_ = new sys2d.Cell();

			public override void bmInitialize(Medget M)
			{
				cell_.copy(commonCell_);
				cell_.SetPositionI(M.x(), M.y() + (M.height() - 16) / 2);
				cell_.SetCell(47);
				sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(cell_);
			}

			public override void bmFinalize(Medget M)
			{
				sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(cell_);
				cell_.Release();
			}

			public override void bmBehave(Medget M)
			{
			}

			public override void bmSuspend(Medget M)
			{
				cell_.SetShow(show: false);
			}

			public override void bmResume(Medget M)
			{
				cell_.SetShow(show: true);
			}

			public static void setupMark()
			{
				commonCell_.Load(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_SUB2D, "icon_8dot.NCER", null, "icon_8dot.NCGR", null);
				commonCell_.ceReleaseCgCl();
			}

			public static void releaseMark()
			{
				commonCell_.Release();
			}

			public new static int classIdentifier()
			{
				return MBMonsterBossMark_UN.number();
			}

			public override object queryInterface(int class_id)
			{
				if (class_id == classIdentifier())
				{
					return this;
				}
				return null;
			}

			~MBMonsterBossMark()
			{
			}
		}
	}
}
