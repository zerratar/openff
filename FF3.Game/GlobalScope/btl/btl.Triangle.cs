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
	public static partial class btl
	{
		public class Triangle
		{
			public const int UP_TRIANGLE = 0;

			public const int DOWN_TRIANGLE = 1;

			public const int TRIANGLE_MAX = 2;

			private sys2d.Sprite3d[] triangle_ = new sys2d.Sprite3d[2];

			private int[] anim_ = new int[2];

			public void setup()
			{
				changeGlobalDirectory();
				triangle_[0].Load2(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN3D, "icon_left");
				triangle_[1].Load2(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN3D, "icon_right");
				for (int i = 0; i < 2; i++)
				{
					sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(triangle_[i]);
					triangle_[i].SetCell(0);
					show(i, flag: false);
					anim_[i] = -1;
				}
			}

			public void cleanup()
			{
				for (int i = 0; i < 2; i++)
				{
					sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(triangle_[i]);
					release(i);
				}
			}

			public bool create()
			{
				for (int i = 0; i < 2; i++)
				{
					triangle_[i].SetPosition(TrianglePosition(i));
					show(i, flag: true);
				}
				return true;
			}

			public void showAll(bool flag)
			{
				for (int i = 0; i < 2; i++)
				{
					show(i, flag);
				}
			}

			public void enable(int i, bool flag)
			{
				int num = ((!flag) ? 1 : 0);
				if (num != anim_[i])
				{
					anim_[i] = num;
					triangle_[i].SetCell((ushort)num);
					triangle_[i].PlayAnimation((ushort)num, NNSG2dAnimationPlayMode.NNS_G2D_ANIMATIONPLAYMODE_FORWARD);
				}
			}

			public int isPush(int x, int y)
			{
				NNSG2dFVec2 nNSG2dFVec = new NNSG2dFVec2();
				nNSG2dFVec.x = 4096 * x;
				nNSG2dFVec.y = 4096 * y;
				for (int i = 0; i < 2; i++)
				{
					if (triangle_[i].IsShow())
					{
						NNSG2dFVec2 nNSG2dFVec2 = new NNSG2dFVec2();
						nNSG2dFVec2.x = TrianglePosition(i).x;
						nNSG2dFVec2.y = TrianglePosition(i).y;
						NNSG2dFVec2 nNSG2dFVec3 = new NNSG2dFVec2();
						nNSG2dFVec3.x = TrianglePosition(i).x + TriangleSize.x;
						nNSG2dFVec3.y = TrianglePosition(i).y + TriangleSize.y;
						OS_Printf("押した X %d\n", nNSG2dFVec.x);
						OS_Printf("押した Y %d\n", nNSG2dFVec.y);
						OS_Printf("左上 X %d\n", nNSG2dFVec2.x);
						OS_Printf("左上 Y %d\n", nNSG2dFVec2.y);
						OS_Printf("右下 X %d\n", nNSG2dFVec3.x);
						OS_Printf("右下 Y %d\n", nNSG2dFVec3.y);
						if (nNSG2dFVec2.x <= nNSG2dFVec.x && nNSG2dFVec2.y <= nNSG2dFVec.y && nNSG2dFVec3.x >= nNSG2dFVec.x && nNSG2dFVec3.y >= nNSG2dFVec.y)
						{
							return i;
						}
					}
				}
				return -1;
			}

			public Triangle()
			{
				for (int i = 0; i < triangle_.Length; i++)
				{
					triangle_[i] = new sys2d.Sprite3d();
				}
			}

			public void release(int i)
			{
				triangle_[i].Release();
			}

			public void show(int i, bool flag)
			{
				triangle_[i].SetShow(flag);
			}
		}
	}
}
