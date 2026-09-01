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
	public static partial class wld
	{
							public class CStateShopEnd : CBaseState
							{
								public override void start(CBaseSystem _sys)
								{
									dgs.CFade.Main().fadeOut(5, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
									dgs.CFade.Sub().fadeOut(5, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
								}

								public override void update(CBaseSystem _sys)
								{
									if (dgs.CFade.Main().isFaded() && dgs.CFade.Sub().isFaded())
									{
										setPhase(PHASE.END);
									}
								}

								public override void end(CBaseSystem _sys)
								{
									stageMng.delStage();
									if (strcmp("prev", sceneMng.getCommonMdl()) == 0)
									{
										switch (shop.CShopManager.Instance().Kind())
										{
										case shop.CShopManager.SHOP_KIND.SHOP_KIND_WEAPON:
											CWorldOutSideData.getInstance().MapData().setCommonMdlNo(1);
											break;
										case shop.CShopManager.SHOP_KIND.SHOP_KIND_ARMOR:
											CWorldOutSideData.getInstance().MapData().setCommonMdlNo(2);
											break;
										case shop.CShopManager.SHOP_KIND.SHOP_KIND_MAGIC:
											CWorldOutSideData.getInstance().MapData().setCommonMdlNo(3);
											break;
										case shop.CShopManager.SHOP_KIND.SHOP_KIND_ITEM:
											CWorldOutSideData.getInstance().MapData().setCommonMdlNo(4);
											break;
										}
									}
									shop.CShopUpDisplayComposition.Instance().terminate();
									shop.CShopManager.Instance().terminate();
									int soundFlag = CWorldOutSideData.getInstance().SoundData().getSoundFlag();
									soundFlag &= -2;
									CWorldOutSideData.getInstance().SoundData().setSoundFlag(soundFlag);
									_sys.setMode(_sys.PreviousMode());
									sys.GGlobal.setNextPart(sys.GGlobal.getPreviousPart());
									_sys.setEnd(_End: true);
									ds.g_Pad.enable();
									ds.g_TouchPanel.enable();
								}

								public override bool canExecuteEvent(CBaseSystem arg0)
								{
									return false;
								}
							}
	}
}
