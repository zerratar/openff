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
		public class MBIcon : MenuBehavior
		{
			public static dgs.UniqueNumber MBIcon_UN = new dgs.UniqueNumber();

			protected sys2d.Cell cell_ = new sys2d.Cell();

			~MBIcon()
			{
			}

			public override void bmInitialize(Medget M)
			{
				XbnNode firstNodeByTagNameFromChildren = ownerMedget.node().getFirstNodeByTagNameFromChildren(TRANSCODE("behavior"));
				if (firstNodeByTagNameFromChildren != null)
				{
					XbnNodeList xbnNodeList = new XbnNodeList();
					firstNodeByTagNameFromChildren.getNodesByTagNameFromChildren(TRANSCODE("parameter"), xbnNodeList);
					cell_.copy(MenuManager.getSingleton().GetSmallIcon2d());
					sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(cell_);
					cell_.SetCell((ushort)GET_DECIMAL_PARAMETER(xbnNodeList, 0));
				}
			}

			public override void bmPostInitialize(Medget M)
			{
				cell_.SetPositionI(ownerMedget.x(), ownerMedget.y());
			}

			public override void bmBehave(Medget M)
			{
			}

			public override void bmFinalize(Medget M)
			{
				sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(cell_);
				cell_.Release();
			}

			public override void bmSuspend(Medget M)
			{
			}

			public override void bmResume(Medget M)
			{
			}

			public bool mbVisibility()
			{
				return cell_.IsShow();
			}

			public void mbSetVisibility(bool v)
			{
				cell_.SetShow(v);
			}

			public void mbSetCell(int num)
			{
				cell_.SetCell((ushort)num);
			}

			public override void mbSetPosition(short x, short y)
			{
				cell_.SetPositionI(x, y);
			}

			public new static int classIdentifier()
			{
				return MBIcon_UN.number();
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
