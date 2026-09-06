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
						public class WorldBGEffect
						{
							private delegate void _funcAlpha(int arg0, int arg1, int arg2, int arg3);

							private delegate void _funcBrightness(int arg0, int arg1);

							private WBE_SCREENSELECT select_;

							private WBE_EFFECTTYPE type_;

							private sbyte nowValue_;

							private sbyte startValue_;

							private sbyte endValue_;

							private int speed_;

							private int frame_;

							private int workFrame_;

							private int planeA_;

							private int planeB_;

							public void wbeInit(WBE_SCREENSELECT select)
							{
								select_ = select;
								type_ = WBE_EFFECTTYPE.WBE_EFFECTTYPE_ALPHA;
								nowValue_ = (startValue_ = (endValue_ = 0));
								speed_ = 0;
								frame_ = (workFrame_ = 0);
								planeA_ = (planeB_ = 0);
							}

							public void wbeExecute()
							{
								if (frame_ > 0)
								{
									workFrame_++;
									nowValue_ = (sbyte)(FX_Mul(speed_, 4096 * workFrame_) / 4096 + startValue_);
									if (workFrame_ >= frame_)
									{
										workFrame_ = (frame_ = 0);
										nowValue_ = endValue_;
									}
									wbeExecuteEffect();
								}
							}

							public void wbeSetEffect(WBE_EFFECTTYPE type, int planeA, int planeB, sbyte startValue, sbyte endValue, int frame)
							{
								type_ = type;
								frame_ = frame;
								workFrame_ = 0;
								nowValue_ = startValue;
								startValue_ = startValue;
								endValue_ = endValue;
								speed_ = FX_Div((endValue_ - startValue_) * 4096, frame_ * 4096);
								planeA_ = planeA;
								planeB_ = planeB;
								if (frame_ == 0)
								{
									nowValue_ = endValue_;
									wbeExecuteEffect();
								}
							}

							public void wbeExecuteEffect()
							{
								_funcAlpha[] array = new _funcAlpha[2] { G2_SetBlendAlpha, G2S_SetBlendAlpha };
								_funcBrightness[] array2 = new _funcBrightness[2] { G2_SetBlendBrightness, G2S_SetBlendBrightness };
								switch (type_)
								{
								case WBE_EFFECTTYPE.WBE_EFFECTTYPE_ALPHA:
								{
									sbyte b = nowValue_;
									sbyte b2 = (sbyte)(31 - nowValue_);
									if (b > 31)
									{
										b = 31;
									}
									if (b < 0)
									{
										b = 0;
									}
									if (b2 > 31)
									{
										b2 = 31;
									}
									if (b2 < 0)
									{
										b2 = 0;
									}
									array[(int)select_](planeA_, planeB_, b, b2);
									break;
								}
								case WBE_EFFECTTYPE.WBE_EFFECTTYPE_BRIGHTNESS:
									array2[(int)select_](planeA_, nowValue_);
									break;
								}
							}
						}
}
