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
						public class MBChocoboBank : menu.MenuBehavior
						{
							public static dgs.UniqueNumber MBChocoboBank_UN = new dgs.UniqueNumber();

							private int mode;

							public override void bmInitialize(menu.Medget M)
							{
								mode = 0;
							}

							public override void bmPostInitialize(menu.Medget M)
							{
							}

							public override void bmBehave(menu.Medget M)
							{
								int num = mode;
								if (num == 3 && (ds.g_Pad.edge() & 2) == 0)
								{
									_ = ds.g_Pad.edge() & 8;
								}
							}

							public override void bmFinalize(menu.Medget M)
							{
							}

							public override bool bmDecide(menu.Medget M)
							{
								return true;
							}

							public override bool bmDirection(menu.Medget M, int dir)
							{
								return true;
							}

							public new static int classIdentifier()
							{
								return MBChocoboBank_UN.number();
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
