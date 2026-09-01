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
	public static partial class map
	{
							public class MapSignEffect
							{
								public static int EFFECT_SIGN_CATEGORY = 102;

								public static int EFFECT_SIGN_MEMBER = 10;

								public static short EFFECT_SIGN_FRAME = 15;

								private cmr.CWorldCamera pWldCam_;

								private VecFx32 pos_ = new VecFx32();

								private int eveFlagGroup_;

								private int eveFlagIndex_;

								private int effIdx_;

								private short cnt_;

								private byte flag_;

								public MapSignEffect()
								{
									pWldCam_ = null;
									initValue();
								}

								public void destruct()
								{
								}

								public void setup(cmr.CWorldCamera pWldCam)
								{
									initValue();
									pWldCam_ = pWldCam;
									flag_ = 1;
								}

								public void terminate()
								{
									initValue();
								}

								public void execute()
								{
									if (pWldCam_ != null && (1 & flag_) != 0 && (cnt_ <= 0 || --cnt_ <= 0) && (-1 == eveFlagGroup_ || -1 == eveFlagIndex_ || FlagManager.singleton().get((uint)eveFlagGroup_, (uint)eveFlagIndex_) == 0) && checkCameraZoom())
									{
										effIdx_ = eff.CEffectMng.instance().create(EFFECT_SIGN_CATEGORY, EFFECT_SIGN_MEMBER);
										if (-1 != effIdx_)
										{
											eff.CEffectMng.instance().setPosition(effIdx_, pos_);
											cnt_ = EFFECT_SIGN_FRAME;
										}
									}
								}

								public void setEventFlag(int eveFlagGroup, int eveFlagIndex)
								{
									eveFlagGroup_ = eveFlagGroup;
									eveFlagIndex_ = eveFlagIndex;
								}

								public void setPos(VecFx32 pos)
								{
									pos_.copy(pos);
								}

								public void setEnable(bool enable)
								{
									if (enable)
									{
										flag_ |= 1;
									}
									else
									{
										flag_ &= 254;
									}
								}

								public void erase()
								{
									if (-1 != effIdx_)
									{
										eff.CEffectMng.instance().release(effIdx_);
									}
								}

								public bool checkCameraZoom()
								{
									switch (pWldCam_.m_Type)
									{
									case cmr.CWorldCamera.TYPE.TYPE_FIELD:
									case cmr.CWorldCamera.TYPE.TYPE_TOWN_OUT:
										if (pWldCam_.composit.Zoom() <= -204800)
										{
											return true;
										}
										break;
									case cmr.CWorldCamera.TYPE.TYPE_TOWN_IN:
									case cmr.CWorldCamera.TYPE.TYPE_DUNGEON:
										if (pWldCam_.composit.Zoom() <= -122880)
										{
											return true;
										}
										break;
									}
									return false;
								}

								public void initValue()
								{
									pWldCam_ = null;
									VEC_Set(pos_, 0, 0, 0);
									eveFlagGroup_ = -1;
									eveFlagIndex_ = -1;
									effIdx_ = -1;
									cnt_ = 0;
									flag_ = 0;
								}

								public bool checkVisible()
								{
									if (pWldCam_ == null)
									{
										return false;
									}
									if ((1 & flag_) == 0)
									{
										return false;
									}
									if (-1 != eveFlagGroup_ && -1 != eveFlagIndex_ && FlagManager.singleton().get((uint)eveFlagGroup_, (uint)eveFlagIndex_) != 0)
									{
										return false;
									}
									if (!checkCameraZoom())
									{
										return false;
									}
									return true;
								}
							}
	}
}
