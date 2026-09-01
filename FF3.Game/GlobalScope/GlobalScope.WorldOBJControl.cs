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
						public class WorldOBJControl
						{
							private static byte WOC_OBJ_NUM = 1;

							private byte alpha_;

							private byte startAlpha_;

							private byte endAlpha_;

							private int alphaSpeed_;

							private int alphaFrame_;

							private int workFrame_;

							private WorldOBJ[] wobj_ = new WorldOBJ[WOC_OBJ_NUM];

							public WorldOBJControl()
							{
								for (int i = 0; i < wobj_.Length; i++)
								{
									wobj_[i] = new WorldOBJ();
								}
							}

							public void wocInitialize()
							{
								for (byte b = 0; b < WOC_OBJ_NUM; b++)
								{
									wobj_[b].woInit();
								}
								alpha_ = 16;
								startAlpha_ = (endAlpha_ = 0);
								alphaSpeed_ = 0;
								alphaFrame_ = (workFrame_ = 0);
								wocSetAlphaImp(alpha_);
							}

							public void wocTerminate()
							{
								for (byte b = 0; b < WOC_OBJ_NUM; b++)
								{
									if (wobj_[b].flag_ != 0)
									{
										wobj_[b].woRelease();
									}
								}
							}

							public void wocSetup(int idx, string filename)
							{
								if (wobj_[idx].flag_ != 0)
								{
									wobj_[idx].woRelease();
								}
								wobj_[idx].obj_.Load2(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_SUB2D, const_cast<string>(filename));
								wobj_[idx].obj_.ceReleaseCgCl();
								sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(wobj_[idx].obj_);
								wobj_[idx].flag_ = 1;
							}

							public void wocExecute()
							{
								if (alphaFrame_ > 0)
								{
									workFrame_++;
									alpha_ = (byte)(FX_Mul(alphaSpeed_, 4096 * workFrame_) / 4096 + startAlpha_);
									if (workFrame_ >= alphaFrame_)
									{
										workFrame_ = (alphaFrame_ = 0);
										alpha_ = endAlpha_;
									}
									wocSetAlphaImp(alpha_);
								}
							}

							public void wocSetPosition(int idx, int x, int y)
							{
								wobj_[idx].obj_.SetPositionI(x, y);
							}

							public void wocGetPosition(int idx, out int x, out int y)
							{
								NNSG2dSVec2 positionI = wobj_[idx].obj_.GetPositionI();
								x = positionI.x;
								y = positionI.y;
							}

							public void wocAddPosition(int idx, int x, int y)
							{
								wocGetPosition(idx, out var x2, out var y2);
								wocSetPosition(idx, x2 + x, y2 + y);
							}

							public void wocSetAlpha(byte alpha)
							{
								alpha_ = alpha;
								wocSetAlphaImp(alpha_);
							}

							public byte wocGetAlpha()
							{
								return alpha_;
							}

							public void wocSetAutoAlpha(int frame, byte alpha)
							{
								workFrame_ = 0;
								alphaFrame_ = frame;
								endAlpha_ = alpha;
								startAlpha_ = alpha_;
								alphaSpeed_ = FX_Div((endAlpha_ - startAlpha_) * 4096, alphaFrame_ * 4096);
							}

							public void wocSetVisible(int idx, bool visible)
							{
								wobj_[idx].obj_.SetShow(visible);
							}

							public void wocSetAlphaImp(byte objAlpha)
							{
								byte ev = (byte)(16 - objAlpha);
								G2S_SetBlendAlpha(16, 46, objAlpha, ev);
							}
						}
}
