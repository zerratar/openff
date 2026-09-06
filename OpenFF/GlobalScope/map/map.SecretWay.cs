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
	public static partial class map
	{
							public class SecretWay
							{
								private uint matIdx_;

								private byte on_;

								private short frame_;

								private int alpha_;

								private int onSpeed_;

								private int offSpeed_;

								private VecFx32 maxPos_ = new VecFx32();

								private VecFx32 minPos_ = new VecFx32();

								private SecretWayParameter pParam_;

								public void destruct()
								{
								}

								public void initialize(SecretWayParameter pSWParam)
								{
									pParam_ = pSWParam;
									int materialIdByName = stageMng.getMaterialIdByName(pSWParam.name);
									matIdx_ = (uint)materialIdByName;
									on_ = 0;
									alpha_ = 4096 * pParam_.offAlpha;
									frame_ = 0;
									int num = 4096 * (pParam_.onAlpha - pParam_.offAlpha);
									onSpeed_ = FX_Div(num, 4096 * pParam_.onFrame);
									offSpeed_ = FX_Div(-num, 4096 * pParam_.offFrame);
									maxPos_.x = pParam_.maxPos[0] * 4096;
									maxPos_.y = pParam_.maxPos[1] * 4096;
									maxPos_.z = pParam_.maxPos[2] * 4096;
									minPos_.x = pParam_.minPos[0] * 4096;
									minPos_.y = pParam_.minPos[1] * 4096;
									minPos_.z = pParam_.minPos[2] * 4096;
									stageMng.setMaterialAlpha(matIdx_, (uint)(alpha_ / 4096));
								}

								public void terminate()
								{
								}

								public void setPosition(VecFx32 pos)
								{
									if (minPos_.x <= pos.x && pos.x <= maxPos_.x && minPos_.y <= pos.y && pos.y <= maxPos_.y && minPos_.z <= pos.z && pos.z <= maxPos_.z)
									{
										if (on_ == 0)
										{
											on_ = 1;
											frame_ = (short)pParam_.onFrame;
										}
									}
									else if (1 == on_)
									{
										on_ = 0;
										frame_ = (short)pParam_.offFrame;
									}
								}

								public void update()
								{
									bool flag = false;
									if (on_ == 0)
									{
										if (alpha_ != 4096 * pParam_.offAlpha)
										{
											flag = true;
											frame_--;
											alpha_ += offSpeed_;
											if (frame_ <= 0)
											{
												frame_ = 0;
												alpha_ = 4096 * pParam_.offAlpha;
											}
										}
									}
									else if (1 == on_ && alpha_ != 4096 * pParam_.onAlpha)
									{
										flag = true;
										frame_--;
										alpha_ += onSpeed_;
										if (frame_ <= 0)
										{
											frame_ = 0;
											alpha_ = 4096 * pParam_.onAlpha;
										}
									}
									if (flag)
									{
										stageMng.setMaterialAlpha(matIdx_, (uint)(alpha_ / 4096));
									}
								}
							}
	}
}
