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
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
						public static class movie
						{
							public class MoviePart : sys.FF3GamePart, ds.MHDSNotifier
							{
								public static MoviePart instance_ = new MoviePart();

								private int ID_;

								private GAMEPART afterPart_;

								private ds.MovieHandleDS MovieHandleDS_;

								public MoviePart()
								{
									MovieHandleDS_ = null;
								}

								public static void registerPart()
								{
									sys.GGlobal.registerPart(GAMEPART.GAMEPART_MOVIE, instance_);
									instance_.setPartID(0);
								}

								public static MoviePart getInstance()
								{
									return instance_;
								}

								protected override void doInitialize()
								{
									ds.CDevice.setEnableCheckSleep(flag: false);
									TexDivideLoader.getSingleton().tdlCancel();
									GX_SetDispSelect(GXDispSelect.GX_DISP_SELECT_MAIN_SUB);
									ovl.overlayRegister.ChangeOverlay(ovl.OVERLAYINDEX.PART_MOVIE);
									ds.DSVX_setVXMalloc(ds.CHeap.alloc_app);
									ds.DSVX_setVXFree(ds.CHeap.free_app);
									ds.DSVX_setSoundMalloc(ds.CHeap.alloc_app);
									ds.DSVX_setSoundFree(ds.CHeap.free_app);
									MatrixSound.MtxSoundImplNDSParam mtxSoundImplNDSParam = MatrixSound.MtxSoundNDS_getParam();
									ds.CHeap.free_app(mtxSoundImplNDSParam._pHeap);
									MatrixSound.MtxSound.getSingleton().finalize();
									ds.DSVX_MovieSetupDualScreen();
									MovieHandleDS_ = new ds.MovieHandleDS();
									if (MovieHandleDS_ != null)
									{
										MovieHandleDS_.init("OPN_upper_stereo.vx", "OPN_lower.vx", this, Loop: false);
									}
									dgs.CFade.Main().fadeIn(0);
									dgs.CFade.Sub().fadeIn(0);
								}

								protected override void doUninitialize()
								{
									if (MovieHandleDS_ != null)
									{
										MovieHandleDS_.destruct();
										MovieHandleDS_ = null;
									}
									ds.DSVX_setVXMalloc(null);
									ds.DSVX_setVXFree(null);
									ds.DSVX_setSoundMalloc(null);
									ds.DSVX_setSoundFree(null);
									OS_SetIrqFunction(1u, ds.CDevice.VBlankIntr);
									ds.CDevice.setEnableCheckSleep(flag: true);
								}

								protected override void onExecutePart()
								{
									MovieHandleDS_.play();
									MovieHandleDS_.final();
									ds.DSVX_MovieCloseDualScreen();
									ds.CHeap.setID_app(255);
									ds.CHeap.chmode_app(b: false);
									MatrixSound.MtxSoundImplNDSParam pArg = new MatrixSound.MtxSoundImplNDSParam(ds.CHeap.alloc_app(524288u), 524288u, 10, "sound_data.sdat");
									MatrixSound.MtxSound.getSingleton().initialize(MatrixSound.g_MtxSoundImplNDS, pArg);
									ds.CHeap.setID_app(0);
									ds.CHeap.chmode_app(b: true);
									sys.GGlobal.setNextPart(afterPart_);
									abort();
								}

								protected override void onDrawPart()
								{
								}

								public void setPartID(int ID)
								{
									ID_ = ID;
								}

								public void setAfterPart(GAMEPART Part)
								{
									afterPart_ = Part;
								}

								public void updateHandler()
								{
									dgs.CFade.execute();
									ds.g_Pad.update();
									ds.g_TouchPanel.update();
									if ((ds.g_Pad.edge() & 8) != 0 || (ds.g_Pad.edge() & 4) != 0 || (ds.g_Pad.edge() & 1) != 0 || (ds.g_Pad.edge() & 2) != 0 || (ds.g_Pad.edge() & 0x400) != 0 || (ds.g_Pad.edge() & 0x800) != 0 || (ds.g_Pad.edge() & 0x200) != 0 || (ds.g_Pad.edge() & 0x100) != 0 || ds.g_TouchPanel.isEdge())
									{
										MovieHandleDS_.stop();
									}
								}
							}
						}
}
