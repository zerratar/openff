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
						public class WorldBG
						{
							public byte flag_;

							private short startX_;

							private short startY_;

							private short endX_;

							private short endY_;

							private short scrollFrame_;

							private short workFrame_;

							private int speedX_;

							private int speedY_;

							public sys2d.Bg bg_ = new sys2d.Bg();

							public void wbInit()
							{
								startX_ = (startY_ = (endX_ = (endY_ = 0)));
								scrollFrame_ = 0;
								flag_ = 0;
							}

							public void wbExecute()
							{
								if (flag_ != 0 && scrollFrame_ > 0)
								{
									workFrame_++;
									int x = FX_Mul(speedX_, 4096 * workFrame_) / 4096 + startX_;
									int y = FX_Mul(speedY_, 4096 * workFrame_) / 4096 + startY_;
									if (workFrame_ >= scrollFrame_)
									{
										workFrame_ = (scrollFrame_ = 0);
										x = endX_;
										y = endY_;
									}
									bg_.bgSetPosition(x, y);
								}
							}

							public void wbSetScroll(short frame, short x, short y)
							{
								workFrame_ = 0;
								scrollFrame_ = frame;
								endX_ = x;
								endY_ = y;
								bg_.bgGetPosition(out var x2, out var y2);
								startX_ = (short)x2;
								startY_ = (short)y2;
								speedX_ = FX_Div((endX_ - startX_) * 4096, scrollFrame_ * 4096);
								speedY_ = FX_Div((endY_ - startY_) * 4096, scrollFrame_ * 4096);
							}
						}
}
