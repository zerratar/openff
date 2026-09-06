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
    public static class ttl
    {
        public class TitlePart : sys.FF3GamePart
        {
            public static TitlePart instance_ = new TitlePart();

            private int partID_;

            protected CTitleSystem tSystem;

            protected int nWait_;

            protected int nState_;

            public static void registerPart()
            {
                sys.GGlobal.registerPart(GAMEPART.GAMEPART_TITLE, instance_);
                instance_.setPartID(0);
            }

            public TitlePart()
            {
                partID_ = 0;
            }

            ~TitlePart()
            {
            }

            public void setPartID(int i)
            {
                partID_ = i;
            }

            protected override void doInitialize()
            {
                TexDivideLoader.getSingleton().tdlCancel();
                ovl.overlayRegister.ChangeOverlay(ovl.OVERLAYINDEX.PART_TITLE);
                ds.CDevice.setup();
                GX_SetDispSelect(GXDispSelect.GX_DISP_SELECT_MAIN_SUB);
                GX_SetBGCharOffset(0);
                GX_SetBGScrOffset(0);
                GXVRamTex gXVRamTex = GXVRamTex.GX_VRAM_TEX_01_AB;
                GXVRamTexPltt gXVRamTexPltt = GXVRamTexPltt.GX_VRAM_TEXPLTT_0123_E;
                GX_SetBankForTex(gXVRamTex);
                GX_SetBankForTexPltt(gXVRamTexPltt);
                ds.CVram.getInstance().setBankForTex(gXVRamTex);
                ds.CVram.getInstance().setBankForPltt(gXVRamTexPltt);
                GX_SetBankForBG(GXVRamBG.GX_VRAM_BG_16_F);
                GX_SetBankForBGExtPltt(GXVRamBGExtPltt.GX_VRAM_BGEXTPLTT_NONE);
                GX_SetBankForOBJ(GXVRamOBJ.GX_VRAM_OBJ_16_G);
                GX_SetBankForOBJExtPltt(GXVRamOBJExtPltt.GX_VRAM_OBJEXTPLTT_NONE);
                GX_SetBankForSubBG(GXVRamSubBG.GX_VRAM_SUB_BG_128_C);
                GX_SetBankForSubBGExtPltt(GXVRamSubBGExtPltt.GX_VRAM_SUB_BGEXTPLTT_0123_H);
                GX_SetBankForSubOBJ(GXVRamSubOBJ.GX_VRAM_SUB_OBJ_128_D);
                GX_SetBankForSubOBJExtPltt(GXVRamSubOBJExtPltt.GX_VRAM_SUB_OBJEXTPLTT_0_I);
                ds.CVram.setMainBGPriority(0, 1, 2, 3);
                ds.CVram.setSubBGPriority(0, 1, 2, 3);
                ds.CVram.getInstance().setupTexVramMng(262144u, 0u, 32u, 0);
                ds.CVram.getInstance().setupPlttVramMng(65536u, 32u, 0);
                GX_SetGraphicsMode(GXDispMode.GX_DISPMODE_GRAPHICS, GXBGMode.GX_BGMODE_0, 0);
                GXS_SetGraphicsMode(GXBGMode.GX_BGMODE_0);
                G2_SetBG1Control(0, 0, GXBGScrBase.GX_BG_SCRBASE_0x0000, GXBGCharBase.GX_BG_CHARBASE_0x04000, 0);
                G2S_SetBG0Control(0, 0, GXBGScrBase.GX_BG_SCRBASE_0x0000, GXBGCharBase.GX_BG_CHARBASE_0x04000, 0);
                G2S_SetBG1Control(0, 0, GXBGScrBase.GX_BG_SCRBASE_0x0800, GXBGCharBase.GX_BG_CHARBASE_0x0c000, 0);
                G2S_SetBG2ControlText(0, 0, GXBGScrBase.GX_BG_SCRBASE_0x1800, GXBGCharBase.GX_BG_CHARBASE_0x1c000);
                G2S_SetBG3ControlText(0, 0, GXBGScrBase.GX_BG_SCRBASE_0x2800, GXBGCharBase.GX_BG_CHARBASE_0x20000);
                sys2d.DS2DManager.d2dGetInstance().d2dInitialize();
                dgs.msg.CMessageSys.getInstance().initialize();
                SuspendSaveDataGlobal.getSingleton().setup();
                tSystem = new CTitleSystem();
                tSystem.Initialize();
                dgs.CCurtain.initialize();
                dgs.CCurtain.Bottom().setEnable(enable: true);
                dgs.CCurtain.Bottom().setColor(0, GX_RGB(0, 0, 0));
                dgs.CCurtain.Bottom().setAlpha(0, 31);
                MatrixSound.MtxSoundBGM.getSingleton().stop(0, MatrixSound.enMtxBGMSlot.enMTX_BGM_SLOT0);
                ds.Sound.ClearGroup();
                MatrixSound.MtxSoundSE.getSingleton().getImplement().finalize();
                MatrixSound.MtxSoundBGM.getSingleton().getImplement().finalize();
                MatrixSound.MtxSoundSE.getSingleton().getImplement().initialize(null);
                MatrixSound.MtxSoundBGM.getSingleton().getImplement().initialize(null);
                MatrixSound.MtxSENDS_Load(0);
                MatrixSound.MtxBGMNDS_Load(0);
                MatrixSound.MtxSoundBGM.getSingleton().play(0, 192, 0, MatrixSound.enMtxBGMSlot.enMTX_BGM_SLOT0);
                _hEnd = false;
                G2_SetBG0Offset(0, 0);
                G2_SetBG1Offset(0, 0);
                G2_SetBG2Offset(0, 0);
                G2_SetBG3Offset(0, 0);
                G2S_SetBG0Offset(0, 0);
                G2S_SetBG1Offset(0, 0);
                G2S_SetBG2Offset(0, 0);
                G2S_SetBG3Offset(0, 0);
                ds.CVram.setMainPlaneVisiblity(bg0: true, bg1: false, bg2: false, bg3: false, obj: false);
                ds.CVram.setSubPlaneVisiblity(bg0: true, bg1: false, bg2: false, bg3: false, obj: true);
                GX_DispOn();
                GXS_DispOn();
                sceneMng.initialize();
            }

            protected override void doUninitialize()
            {
                tSystem.Terminate();
                tSystem.destruct();
                tSystem = null;
                wld.CWorldOutSideData.getInstance().initialize();
                wld.CWorldOutSideData.getInstance().SoundData().setSoundFlag(2);
                evt.CEventManager.getInstance().initialize();
                pl.PlayerParty.instance().item().initialize();
                pl.PlayerParty.instance().npc().initialize();
                pl.PlayerParty.instance().gold().min();
                pl.PlayerParty.instance().gold().set(100);
                pl.PlayerParty.instance().playTime_set(0u);
                dgs.msg.CMessageSys.getInstance().terminate();
                dgs.CCurtain.Bottom().terminate();
                GX_SetGraphicsMode(GXDispMode.GX_DISPMODE_GRAPHICS, GXBGMode.GX_BGMODE_0, 0);
                ds.CVram.getInstance().releaseTexVramMng();
                ds.CVram.getInstance().releasePlttVramMng();
                if (sys.GGlobal.getNextPart() == GAMEPART.GAMEPART_WORLD)
                {
                    SuspendSaveDataGlobal.getSingleton().release();
                }
                else if (sys.GGlobal.getNextPart() == GAMEPART.GAMEPART_MOVIE)
                {
                    SuspendSaveDataGlobal.getSingleton().release();
                }
                G2_SetBG0Offset(0, 0);
                G2_SetBG1Offset(0, 0);
                G2_SetBG2Offset(0, 0);
                G2_SetBG3Offset(0, 0);
                G2S_SetBG0Offset(0, 0);
                G2S_SetBG1Offset(0, 0);
                G2S_SetBG2Offset(0, 0);
                G2S_SetBG3Offset(0, 0);
            }

            protected override void doSleep()
            {
            }

            protected override void doWakeUp()
            {
            }

            protected override void onDrawPart()
            {
                dgs.CCurtain.Bottom().draw();
                tSystem.OnDraw();
                dgs.msg.CMessageSys.getInstance().draw();
            }

            protected override void onExecutePart()
            {
                tSystem.Execute();
                dgs.CCurtain.execute();
                if (!_hEnd)
                {
                    int nextPart = tSystem.GetNextPart();
                    switch (nextPart)
                    {
                        case 0:
                            {
                                MatrixSound.MtxSoundBGM.getSingleton().stop(15, MatrixSound.enMtxBGMSlot.enMTX_BGM_SLOT0);
                                pl.PlayerParty.instance().initialize();
                                evt.CEventManager.getInstance().initialize();
                                mon.MonsterManager.instance().monsterManiaManager().clearMonsterMania();
                                pl.PlayerParty.instance().addPlayer(0);
                                pl.PlayerParty.instance().playerForId(0).changeJob(pl.JOB_TYPE.SUPPINN);
                                pl.PlayerParty.instance().playerForId(0).updateParameter();
                                VecFx32 pos = new VecFx32(0, 0, 0);
                                VecFx32 rot = new VecFx32(0, 0, 0);
                                CCastCommandTransit.getInstance().castParam_MapJump().setUp("d01_05", 0, pos, rot, _Flag: true);
                                sceneMng.gotoStage("d01_05");
                                mognet.MNMemento.getSingleton().mnmClearMail();
                                mognet.MNNPCMailData.getSingleton().clearNPCMailData();
                                ds.GlobalPlayTimeCounter.getSingleton().set(0u);
                                ds.GlobalPlayTimeCounter.getSingleton().start();
                                OptionSaveDataGlobal.getSingleton().setup();
                                if (OptionSaveDataGlobal.getSingleton().isProper())
                                {
                                    OptionSaveDataGlobal.getSingleton().reflect();
                                }
                                else
                                {
                                    opt.COptionManager.getSingleton().initialize();
                                    card.SaveOption();
                                }
                                sys.GGlobal.setNextPart(GAMEPART.GAMEPART_WORLD);
                                break;
                            }
                        case 1:
                            load.LoadPart.getInstance().setAfterPart(GAMEPART.GAMEPART_WORLD);
                            sys.GGlobal.setNextPart(GAMEPART.GAMEPART_LOAD);
                            break;
                        case 2:
                            MatrixSound.MtxSoundBGM.getSingleton().stop(15, MatrixSound.enMtxBGMSlot.enMTX_BGM_SLOT0);
                            sys.GGlobal.setNextPart(GAMEPART.GAMEPART_SUSPEND_LOAD);
                            break;
                        case 3:
                            MatrixSound.MtxSoundBGM.getSingleton().stop(15, MatrixSound.enMtxBGMSlot.enMTX_BGM_SLOT0);
                            movie.MoviePart.getInstance().setAfterPart(GAMEPART.GAMEPART_TITLE);
                            sys.GGlobal.setNextPart(GAMEPART.GAMEPART_MOVIE);
                            break;
                        case 4:
                            MatrixSound.MtxSoundBGM.getSingleton().stop(15, MatrixSound.enMtxBGMSlot.enMTX_BGM_SLOT0);
                            sys.GGlobal.setNextPart(GAMEPART.GAMEPART_TITLE);
                            break;
                        case 5:
                            MatrixSound.MtxSoundBGM.getSingleton().stop(15, MatrixSound.enMtxBGMSlot.enMTX_BGM_SLOT0);
                            sys.GGlobal.setNextPart(GAMEPART.GAMEPART_LINK);
                            break;
                    }
                    if (nextPart >= 0)
                    {
                        dgs.CFade.Main().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
                        dgs.CFade.Sub().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
                        _hEnd = true;
                        if (nextPart != 3)
                        {
                            MatrixSound.MtxSENDS_Play(0, 1, 192, 127);
                        }
                    }
                }
                else if (dgs.CFade.Main().isFaded() && dgs.CFade.Sub().isFaded())
                {
                    if (sys.GGlobal.getNextPart() == GAMEPART.GAMEPART_WORLD || sys.GGlobal.getNextPart() == GAMEPART.GAMEPART_MOVIE || sys.GGlobal.getNextPart() == GAMEPART.GAMEPART_SUSPEND_LOAD)
                    {
                        MatrixSound.MtxBGMNDS_Unload();
                    }
                    abort();
                }
            }

            protected override void onDrawEffector()
            {
                sys2d.DS2DManager.d2dGetInstance().d2dDraw();
                sys2d.DS2DManager.d2dGetInstance().d2dDrawScreen(depthtest: false);
            }

            protected override void onUpdatePart()
            {
                sys2d.DS2DManager.d2dGetInstance().d2dUpdate();
            }

            public bool doKeyProcess(bool __hEnd)
            {
                return true;
            }
        }

        public class LinkPart : sys.FF3GamePart
        {
            public enum EndPhase
            {
                FadeStart,
                NextPart,
                EndPhaseMax
            }

            public const EndPhase FadeStart = EndPhase.FadeStart;

            public const EndPhase NextPart = EndPhase.NextPart;

            public const EndPhase EndPhaseMax = EndPhase.EndPhaseMax;

            public static LinkPart instance_ = new LinkPart();

            private EndPhase m_Phase;

            private sys2d.Bg _backBg = new sys2d.Bg();

            public static void registerPart()
            {
                sys.GGlobal.registerPart(GAMEPART.GAMEPART_LINK, instance_);
            }

            ~LinkPart()
            {
            }

            protected override void doInitialize()
            {
                ds.CHeap.setID_app(0);
                GX_SetDispSelect(GXDispSelect.GX_DISP_SELECT_MAIN_SUB);
                G2_BlendNone();
                sys2d.DS2DManager.d2dGetInstance().d2dInitialize();
                dgs.msg.CMessageSys.getInstance().initialize();
                dgs.msg.CMessageSys.getInstance().Sub().assignBG(0, 0, 0, 32, 24);
                ds.CDevice.singleton().setFPS(ds.CDevice.enFPS.enFPS_30);
                ds.CHeap.setID_app(50);
                menu.MenuManager.getSingleton().Set2d3dMode(2);
                ds.CHeap.setID_app(51);
                changeCompanyDirectory();
                menu.MenuManager.getSingleton().initialize();
                menu.MenuManager.getSingleton().LoadXbnFile("MenuDefine.xbn");
                ds.CHeap.setID_app(52);
                menu.MenuManager.getSingleton().SetUsingMenuType(1);
                ds.CHeap.setID_app(50);
                menu.MenuManager.getSingleton().CreateMenuDataText(0);
                dgs.CFade.Main().fadeIn(15);
                dgs.CFade.Sub().fadeIn(15);
                menu.MenuManager.getSingleton().release();
                menu.MenuManager.getSingleton().buildMenu("link");
                menu.MenuManager.getSingleton().SetImposibleScrollFlag(val: false);
                menu.MenuManager.getSingleton().SetCancelButtonState(-1);
                changeGlobalDirectory();
                ds.CHeap.setID_app(55);
                _backBg.bgLoad("link.NSCR", "menu_bg_01.NCGR", "new_menu_bg.NCLR");
                _backBg.bgSetUp(NNSG2dBGSelect.NNS_G2D_BGSELECT_SUB1);
                _backBg.bgRelease();
                wmenu.CWMenuManager.Instance().GetMenuButton().initialize();
                ds.CVram.setSubPlaneVisiblity(bg0: true, bg1: true, bg2: false, bg3: false, obj: true);
                MatrixSound.MtxSENDS_Load(98);
                GX_DispOn();
                GXS_DispOn();
                m_Phase = EndPhase.FadeStart;
            }

            protected override void doUninitialize()
            {
                menu.MenuManager.getSingleton().Set2d3dMode(2);
                menu.MenuManager.getSingleton().ReleaseMenuDataText();
                menu.MenuManager.getSingleton().release();
                menu.MenuManager.getSingleton().releaseWindowAll();
                menu.MenuManager.getSingleton().ReleaseXbnFile();
                menu.MenuManager.getSingleton().terminate();
                menu.MenuManager.getSingleton().DeleteNeedObject(menu.MenuManager.MENU_CREATE_PARAMETER.CREATE_MENU_MODE_3D);
                menu.MenuManager.getSingleton().Set2d3dMode(3);
                wmenu.CWMenuManager.Instance().GetMenuButton().terminate();
                wmenu.CWMenuManager.Instance().GetPcFace().pcfmCleanup();
                MatrixSound.MtxSENDS_Unload();
                MatrixSound.MtxBGMNDS_Unload();
                wld.CWorldOutSideData.getInstance().SoundData().setSoundFlag(2);
                dgs.msg.CMessageSys.getInstance().terminate();
            }

            protected override void doSleep()
            {
            }

            protected override void doWakeUp()
            {
            }

            protected override void onDrawPart()
            {
                dgs.msg.CMessageSys.getInstance().draw();
            }

            protected override void onExecutePart()
            {
                OS_AssignBackButton(1);
                sys2d.DS2DManager.d2dGetInstance().d2dExecute();
                menu.MenuManager.getSingleton().execute();
                if ((wmenu.CWMenuManager.Instance().GetMenuButton().TouchButtonB() || menu.MenuManager.getSingleton().GetCancelButtonState() == 0) && m_Phase != EndPhase.NextPart)
                {
                    dgs.CFade.Main().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
                    dgs.CFade.Sub().fadeOut(15, dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
                    menu.MenuManager.getSingleton().playSECancel();
                    MatrixSound.MtxSoundBGM.getSingleton().stop(15, MatrixSound.enMtxBGMSlot.enMTX_BGM_SLOT0);
                    m_Phase = EndPhase.NextPart;
                }
                if (m_Phase == EndPhase.NextPart && dgs.CFade.Main().isFaded() && dgs.CFade.Sub().isFaded())
                {
                    sys.GGlobal.setNextPart(GAMEPART.GAMEPART_TITLE);
                    abort();
                }
            }

            protected override void onDrawEffector()
            {
                sys2d.DS2DManager.d2dGetInstance().d2dDraw();
                sys2d.DS2DManager.d2dGetInstance().d2dDrawScreen(depthtest: false);
            }

            protected override void onUpdatePart()
            {
                sys2d.DS2DManager.d2dGetInstance().d2dUpdate();
            }
        }

        public class CTitlePrologue
        {
            private sys2d.Bg art_ = new sys2d.Bg();

            private dgs.MSDINFO msdPrologue_;

            private short state_;

            private short counter_;

            private short fade_;

            private short step_;

            private dgs.DGSMessage msg_;

            private ds.Vector2<short> offset_;

            public CTitlePrologue()
            {
                msdPrologue_ = null;
                msg_ = null;
            }

            ~CTitlePrologue()
            {
            }

            public void tpBegin()
            {
                state_ = 0;
                step_ = 0;
            }

            public void tpEnd()
            {
                if (state_ != 4)
                {
                    step_ = 256;
                    state_ = 3;
                }
            }

            public void tpInitialize()
            {
                tpTerminate();
                changeCompanyDirectory();
                Array array = null;
                string filename = "eureka_prologue.msd";
                uint size = ds.g_File.getSize(filename);
                if (size != 0)
                {
                    array = ds.CHeap.alloc_app(size);
                    if (array != null)
                    {
                        ds.g_File.load(array, filename);
                    }
                    msdPrologue_ = (dgs.MSDINFO)array;
                }
                dgs.msg.CMessageSys.getInstance().Sub().initialize();
                dgs.msg.CMessageSys.getInstance().Sub().assignBG(0, 0, 0, 32, 24);
                dgs.msg.CMessageSys.getInstance().Sub().setUpMSD(msdPrologue_, dgs.msg.CMessageMng.MSD_HANDLE_KIND.MSD_HANDLE_KIND_COMMON);
                G2S_SetBlendBrightness(3, 16);
                G2S_BlendNone();
            }

            public void tpTerminate()
            {
                art_.bgSetShow(show: false);
                art_.bgRelease();
                G2S_ChangeBlendBrightness(0);
                if (msg_ != null)
                {
                    msg_.release();
                }
                msg_ = null;
                dgs.msg.CMessageSys.getInstance().Sub().terminate();
                if (msdPrologue_ != null)
                {
                    ds.CHeap.free_app(msdPrologue_);
                }
                msdPrologue_ = null;
            }

            public bool tpProcess()
            {
                switch (state_)
                {
                    case 0:
                        {
                            dgs.CFade.Main().fadeIn(FADEIN_DURATION);
                            dgs.CFade.Sub().fadeIn(FADEIN_DURATION);
                            changeGlobalDirectory();
                            art_.bgLoad2(artName[step_]);
                            art_.bgSetUp(NNSG2dBGSelect.NNS_G2D_BGSELECT_SUB1);
                            art_.bgRelease();
                            art_.bgSetShow(show: true);
                            if (msg_ != null)
                            {
                                msg_.release();
                            }
                            msg_ = null;
                            dgs.DGSMessageManager dGSMessageManager = dgs.msg.CMessageSys.getInstance().Sub();
                            msg_ = dGSMessageManager.createMessage((uint)msgID[step_], dgs.INVALID_MSDHANDLE, 0);
                            if (msg_ != null)
                            {
                                msg_.setMessageColor(dgs.TXT_COLOR.TXT_COLOR_BLACK);
                                msg_.setStyle(1024u);
                                msg_.setVSpace(10);
                                msg_.setShadow(b: false);
                                msg_.setDisplaySpeed(byte.MaxValue);
                                msg_.setDisplayWait(0);
                                ds.Vector2<short> vector = new ds.Vector2<short>();
                                msg_.getCompleteTextSize(vector);
                                vector.vx = (short)(240 - vector.vx / 2);
                                vector.vy = (short)(120 - vector.vy / 2);
                                msg_.setPosition(vector.vx, vector.vy, erase: true);
                            }
                            SVC_WaitVBlankIntr();
                            state_ = 1;
                            break;
                        }
                    case 1:
                        if (dgs.CFade.Main().isCleared())
                        {
                            G2S_ChangeBlendBrightness(0);
                            counter_ = (short)PAUSE_DURATION;
                            state_ = 2;
                        }
                        break;
                    case 2:
                        if ((ds.g_Pad.edge() & 9) != 0 || ds.g_TouchPanel.isRelease())
                        {
                            MatrixSound.MtxSENDS_Play(0, 3, 192, 127);
                            state_ = 4;
                        }
                        else if (--counter_ < 0)
                        {
                            fade_ = 0;
                            state_ = 3;
                            dgs.CFade.Main().fadeOut(FADEOUT_DURATION, dgs.CFade.FADE_TYPE.FADE_TYPE_WHITE);
                            dgs.CFade.Sub().fadeOut(FADEOUT_DURATION, dgs.CFade.FADE_TYPE.FADE_TYPE_WHITE);
                        }
                        break;
                    case 3:
                        if (!dgs.CFade.Main().isFaded())
                        {
                            break;
                        }
                        if (++step_ >= 5)
                        {
                            if (msg_ != null)
                            {
                                msg_.release();
                            }
                            msg_ = null;
                            art_.bgSetShow(show: false);
                            dgs.CFade.Main().fadeIn(15);
                            dgs.CFade.Sub().fadeIn(15);
                            tpEnd();
                            state_ = 4;
                        }
                        else
                        {
                            state_ = 0;
                        }
                        break;
                    case 4:
                        if (dgs.CFade.Main().isCleared())
                        {
                            return false;
                        }
                        break;
                    default:
                        return false;
                }
                return true;
            }

            public short tpState()
            {
                return state_;
            }
        }

        public class CTitle2D
        {
            public class TITLE_COMMAND
            {
                public int next_part;

                public ds.Vector2<short> pos = new ds.Vector2<short>();

                public sys2d.Cell cell = new sys2d.Cell();
            }

            public const int TITLE_2D_PUSH_START = 0;

            public const int TITLE_2D_NEW_GAME = 1;

            public const int TITLE_2D_LODE_GAME = 2;

            public const int TITLE_2D_CONTINUE = 3;

            public const int TITLE_2D_WIFI = 4;

            public const int TITLE_2D_MAX = 5;

            public const int TITLE_3D_HITO = 0;

            public const int TITLE_3D_ROGO = 1;

            public const int TITLE_3D_ROGO_REVERSE = 2;

            public const int TITLE_3D_MAX = 3;

            public const int FADE_IN_CHECK = 0;

            public const int ROGO_FADE_IN = 1;

            public const int ROGO_FADE_WAIT = 2;

            public const int CHANGE_TITLE_WAIT = 3;

            public const int CHANGE_TITLE = 4;

            public const int TITLE_COMMAND_NEW = 0;

            public const int TITLE_COMMAND_LOAD = 1;

            public const int TITLE_COMMAND_CONTINUE = 2;

            public const int TITLE_COMMAND_WIFI = 3;

            public const int TITLE_COMMAND_MAX = 4;

            public const int PUSH_ALPHA_FLAG_ADD = 0;

            public const int PUSH_ALPHA_FLAG_DEC = 1;

            private int cIndexNo;

            private sys2d.Cell cursor = new sys2d.Cell();

            private sys2d.Cell touch = new sys2d.Cell();

            private sys2d.Cell setting = new sys2d.Cell();

            private sys2d.Cell about = new sys2d.Cell();

            private ds.Vector<TITLE_COMMAND, ds.FastErasePolicy<TITLE_COMMAND>> titleCommands = new ds.Vector<TITLE_COMMAND, ds.FastErasePolicy<TITLE_COMMAND>>(4);

            private sys2d.Sprite3d[] logoSprite = new sys2d.Sprite3d[2]
            {
                                    new sys2d.Sprite3d(),
                                    new sys2d.Sprite3d()
            };

            private int addSpeed;

            private int pushAlphaFlag;

            private int logoAlpha;

            private int localState;

            private int logoWaitCount;

            private CTitlePrologue tPrologue = new CTitlePrologue();

            public void initialize()
            {
                LANGUAGE_CODE lANGUAGE_CODE = languageCode();
                localState = 0;
                logoWaitCount = 0;
                addSpeed = 0;
                logoAlpha = 0;
                cIndexNo = 0;
                titleCommands.clear();
                changeCompanyDirectory();
                about.Load(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_SUB2D, "about.NCER", null, "about.NCGR", "about.NCLR");
                about.SetCell(0);
                about.SetShow(show: false);
                about.SetPriority(0);
                about.SetPositionI(240, 184);
                sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(about);
                logoSprite[0].Load(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN3D, (OS_GetLanguage() == 0) ? "title_gousei_new_jp.NCER" : "title_gousei_new.NCER", null, "title_gousei_new.NCBR", "title_gousei_new.NCLR");
                sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(logoSprite[0]);
                logoSprite[0].SetCell(0);
                logoSprite[0].SetShow(show: false);
                logoSprite[0].SetDepth(0);
                logoSprite[0].SetPositionI(240, 160);
                logoSprite[1].copy(logoSprite[0]);
                logoSprite[1].SetCell(1);
                logoSprite[1].SetShow(show: false);
                logoSprite[1].SetAlpha(1);
                logoSprite[1].SetPositionI(240, 160);
                sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(logoSprite[1]);
                changeGlobalDirectory();
                cursor.Load2(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_SUB2D, "icon_yubi");
                sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(cursor);
                cursor.SetShow(show: false);
                cursor.SetPriority(0);
                cursor.ceReleaseCgCl();
                TITLE_COMMAND tITLE_COMMAND = new TITLE_COMMAND();
                changeCompanyDirectory();
                touch.Load(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_SUB2D, "title_items_i.NCER", null, "title_items_i.NCGR", "title_items_i.NCLR");
                setting.Load(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_SUB2D, "repair.NCER", null, "repair.NCGR", "repair.NCLR");
                if (SuspendSaveDataGlobal.getSingleton().isProper())
                {
                    tITLE_COMMAND = new TITLE_COMMAND();
                    tITLE_COMMAND.next_part = 2;
                    tITLE_COMMAND.pos.vx = (short)CONTINUE_POS_X;
                    tITLE_COMMAND.pos.vy = (short)CONTINUE_POS_Y;
                    tITLE_COMMAND.cell.copy(touch);
                    tITLE_COMMAND.cell.SetCell(3);
                    tITLE_COMMAND.cell.SetShow(show: false);
                    tITLE_COMMAND.cell.SetPriority(0);
                    tITLE_COMMAND.cell.SetPositionI(CONTINUE_POS_X + position_setting_x[(int)lANGUAGE_CODE], CONTINUE_POS_Y);
                    titleCommands.push_back(tITLE_COMMAND);
                    sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(titleCommands[titleCommands.size() - 1].cell);
                }
                else
                {
                    tITLE_COMMAND = new TITLE_COMMAND();
                    tITLE_COMMAND.next_part = -1;
                    tITLE_COMMAND.pos.vx = (short)CONTINUE_POS_X;
                    tITLE_COMMAND.pos.vy = (short)CONTINUE_POS_Y;
                    tITLE_COMMAND.cell.copy(touch);
                    tITLE_COMMAND.cell.SetCell(6);
                    tITLE_COMMAND.cell.SetShow(show: false);
                    tITLE_COMMAND.cell.SetPriority(0);
                    tITLE_COMMAND.cell.SetPositionI(CONTINUE_POS_X + position_setting_x[(int)lANGUAGE_CODE], CONTINUE_POS_Y);
                    titleCommands.push_back(tITLE_COMMAND);
                    sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(titleCommands[titleCommands.size() - 1].cell);
                }
                tITLE_COMMAND = new TITLE_COMMAND();
                tITLE_COMMAND.next_part = 0;
                tITLE_COMMAND.pos.vx = (short)NEW_GAME_POS_X;
                tITLE_COMMAND.pos.vy = (short)NEW_GAME_POS_Y;
                tITLE_COMMAND.cell.copy(touch);
                tITLE_COMMAND.cell.SetCell(1);
                tITLE_COMMAND.cell.SetShow(show: false);
                tITLE_COMMAND.cell.SetPriority(0);
                tITLE_COMMAND.cell.SetPositionI(NEW_GAME_POS_X + position_setting_x[(int)lANGUAGE_CODE], NEW_GAME_POS_Y);
                titleCommands.push_back(tITLE_COMMAND);
                sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(titleCommands[titleCommands.size() - 1].cell);
                tITLE_COMMAND = new TITLE_COMMAND();
                tITLE_COMMAND.next_part = 1;
                tITLE_COMMAND.pos.vx = (short)LOAD_GAME_POS_X;
                tITLE_COMMAND.pos.vy = (short)LOAD_GAME_POS_Y;
                tITLE_COMMAND.cell.copy(touch);
                tITLE_COMMAND.cell.SetCell(2);
                tITLE_COMMAND.cell.SetShow(show: false);
                tITLE_COMMAND.cell.SetPriority(0);
                tITLE_COMMAND.cell.SetPositionI(LOAD_GAME_POS_X + position_setting_x[(int)lANGUAGE_CODE], LOAD_GAME_POS_Y);
                titleCommands.push_back(tITLE_COMMAND);
                sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(titleCommands[titleCommands.size() - 1].cell);
                setting.SetCell(0);
                setting.SetShow(show: false);
                setting.SetPriority(0);
                setting.SetPositionI(8, 268);
                sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(setting);
                // PORT: the phone's fourth entry (network / achievements). On this client it
                // opens the mod list (OpenFF.Client.ModListScreen), which draws its own "MODS" label at
                // the entry's position; so the entry exists whenever the list is available,
                // and otherwise only when its picture does (the Steam build's title bank keeps
                // cells 4 and 5 empty, and a blank line the cursor can land on is no entry).
                if (OpenFF.Client.ModListScreen.Available || OpenFF.Client.SteamCells.CellHasPicture(touch, (UserInfo.confirm_state != 2) ? 4 : 5))
                {
                tITLE_COMMAND = new TITLE_COMMAND();
                tITLE_COMMAND.next_part = ((UserInfo.confirm_state != 2) ? 4 : 5);
                tITLE_COMMAND.pos.vx = (short)WIFI_POS_X;
                tITLE_COMMAND.pos.vy = (short)WIFI_POS_Y;
                tITLE_COMMAND.cell.copy(touch);
                tITLE_COMMAND.cell.SetCell((ushort)((UserInfo.confirm_state != 2) ? 4u : 5u));
                tITLE_COMMAND.cell.SetShow(show: false);
                tITLE_COMMAND.cell.SetPriority(0);
                tITLE_COMMAND.cell.SetPositionI(WIFI_POS_X + position_setting_x[(int)lANGUAGE_CODE], WIFI_POS_Y);
                titleCommands.push_back(tITLE_COMMAND);
                sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(titleCommands[titleCommands.size() - 1].cell);
                }
                touch.SetCell(0);
                touch.SetShow(show: false);
                touch.SetPriority(0);
                int num = TOUCH_TO_START_WIDTH;
                switch (OS_GetLanguage())
                {
                    case 2:
                        num = 160;
                        break;
                    case 3:
                        num = 128;
                        break;
                    case 4:
                        num = 128;
                        break;
                    case 5:
                        num = 136;
                        break;
                }
                // PORT: the widths above are the phone pictures'. The Steam bank draws the
                // prompt as one 300 px strip with the words left-justified inside it, so
                // centring the strip leaves the words off to the left; the words themselves
                // are centred, by what the cell actually paints.
                if (OpenFF.Client.SteamCells.CellVisibleSpan(touch, 0, out int visibleLeft, out int visibleRight))
                {
                    touch.SetPositionI((480 - (visibleRight - visibleLeft)) / 2 - visibleLeft, NEW_GAME_POS_Y);
                    OpenFF.Client.Log.Write(OpenFF.Client.LogChannel.General, "title: start prompt paints " + visibleLeft + ".." + visibleRight + " of its cell, placed at " + ((480 - (visibleRight - visibleLeft)) / 2 - visibleLeft));
                }
                else
                {
                    touch.SetPositionI((480 - num) / 2, NEW_GAME_POS_Y);
                }
                touch.ceReleaseCgCl();
                sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(touch);
                pushAlphaFlag = 1;
            }

            public void terminate()
            {
                logoSprite[0].Release();
                sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(logoSprite[0]);
                logoSprite[1].Release();
                sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(logoSprite[1]);
                tPrologue.tpTerminate();
                OpenFF.Client.ModListScreen.HideTitleLabel();
                cursor.Release();
                sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(cursor);
                touch.Release();
                sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(touch);
                setting.Release();
                sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(setting);
                about.Release();
                sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(about);
                while (!titleCommands.empty())
                {
                    TITLE_COMMAND tITLE_COMMAND = titleCommands[titleCommands.size() - 1];
                    tITLE_COMMAND.cell.Release();
                    sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(tITLE_COMMAND.cell);
                    titleCommands.erase(titleCommands.size() - 1);
                }
            }

            public bool T_ProcessMain()
            {
                if ((ds.g_Pad.edge() & 9) != 0 || ds.g_TouchPanel.isRelease())
                {
                    MatrixSound.MtxSENDS_Play(0, 3, 192, 127);
                    return true;
                }
                return false;
            }

            public bool T_ProcessSelect()
            {
                LANGUAGE_CODE lANGUAGE_CODE = languageCode();
                if (4 == tPrologue.tpState())
                {
                    return false;
                }
                if ((ds.g_Pad.repeat() & 0x40) != 0 || (ds.g_Pad.repeat() & 0x20) != 0)
                {
                    if (--cIndexNo < 0)
                    {
                        cIndexNo = titleCommands.size() - 1;
                    }
                    MatrixSound.MtxSENDS_Play(0, 3, 192, 127);
                }
                else if ((ds.g_Pad.repeat() & 0x80) != 0 || (ds.g_Pad.repeat() & 0x10) != 0)
                {
                    if (++cIndexNo >= titleCommands.size())
                    {
                        cIndexNo = 0;
                    }
                    MatrixSound.MtxSENDS_Play(0, 3, 192, 127);
                }
                else
                {
                    if (ds.g_TouchPanel.isRelease())
                    {
                        return TouchSelectCommand();
                    }
                    if ((ds.g_Pad.edge() & 9) != 0)
                    {
                        if (OpenFF.Client.ModListScreen.Available && titleCommands[cIndexNo].next_part >= 4)
                        {
                            // PORT: the fourth entry is the mod list on this client, not the network part.
                            MatrixSound.MtxSENDS_Play(0, 1, 192, 127);
                            OpenFF.Client.ModListScreen.Open();
                            return false;
                        }
                        cIndexNo = titleCommands[cIndexNo].next_part;
                        return true;
                    }
                }
                cursor.SetPositionI(titleCommands[cIndexNo].pos.vx - 16 + position_setting_x[(int)lANGUAGE_CODE], titleCommands[cIndexNo].pos.vy + title_command_height / 2);
                return false;
            }

            public bool T_ProcessBrou()
            {
                logoWaitCount++;
                switch (localState)
                {
                    case 0:
                        if (dgs.CFade.Main().isCleared() && dgs.CFade.Sub().isCleared())
                        {
                            localState = 1;
                            logoSprite[1].SetShow(show: true);
                            logoAlpha = 1;
                        }
                        break;
                    case 1:
                        logoSprite[1].SetAlpha((byte)logoAlpha);
                        if (++logoAlpha > 30)
                        {
                            logoWaitCount = 0;
                            localState = 2;
                            logoAlpha = 0;
                            logoSprite[1].SetAlpha(31);
                        }
                        break;
                    case 2:
                        if (logoWaitCount > 20)
                        {
                            logoWaitCount = 0;
                            localState = 3;
                            dgs.CFade.Main().fadeOut(5, dgs.CFade.FADE_TYPE.FADE_TYPE_WHITE);
                            dgs.CFade.Sub().fadeOut(5, dgs.CFade.FADE_TYPE.FADE_TYPE_WHITE);
                        }
                        break;
                    case 3:
                        if (dgs.CFade.Main().isFaded())
                        {
                            dgs.CCurtain.Bottom().setColor(0, GX_RGB(31, 31, 31));
                            SetUpMainWait();
                            dgs.CFade.Main().fadeIn(15);
                            dgs.CFade.Sub().fadeIn(15);
                            localState = 4;
                        }
                        break;
                    case 4:
                        if (dgs.CFade.Main().isCleared() && dgs.CFade.Sub().isCleared())
                        {
                            return true;
                        }
                        break;
                }
                return false;
            }

            public void SetUpMainBrou()
            {
            }

            public void SetUpMainWait()
            {
                logoSprite[1].SetShow(show: false);
                logoSprite[0].SetShow(show: true);
                touch.SetShow(show: true);
            }

            public void SetUpMainSelect()
            {
                touch.SetShow(show: false);
                for (int num = titleCommands.size() - 1; num >= 0; num--)
                {
                    titleCommands[num].cell.SetShow(show: true);
                }
                if (SuspendSaveDataGlobal.getSingleton().isProper())
                {
                    cursor.SetPositionI(titleCommands[0].pos.vx - 16, titleCommands[0].pos.vy + title_command_height / 2);
                    cIndexNo = 0;
                }
                else if (0 < card.Manager.GetInstance().GetAlreadyExistDataNum())
                {
                    cursor.SetPositionI(titleCommands[2].pos.vx - 16, titleCommands[2].pos.vy + title_command_height / 2);
                    cIndexNo = 2;
                }
                else
                {
                    cursor.SetPositionI(titleCommands[1].pos.vx - 16, titleCommands[1].pos.vy + title_command_height / 2);
                    cIndexNo = 1;
                }
                cursor.SetShow(show: true);
                setting.SetShow(show: true);
                about.SetShow(show: false);
                if (OpenFF.Client.ModListScreen.Available && !OpenFF.Client.SteamCells.CellHasPicture(touch, (UserInfo.confirm_state != 2) ? 4 : 5))
                {
                    OpenFF.Client.ModListScreen.ShowTitleLabel(WIFI_POS_X + position_setting_x[(int)languageCode()], WIFI_POS_Y);
                }
            }

            public bool TouchSelectCommand()
            {
                LANGUAGE_CODE lANGUAGE_CODE = languageCode();
                ds.g_TouchPanel.getLastPoint(out var x, out var y);
                if (x >= 0 && x < 64 && y >= 256 && y < 320)
                {
                    MatrixSound.MtxSENDS_Play(0, 1, 192, 127);
                    about.SetShow(!about.IsShow());
                    return false;
                }
                if (about.IsShow())
                {
                    if (x >= 70 && x < 420 && y >= 220 && y < 270)
                    {
                        webTo();
                        return false;
                    }
                    return false;
                }
                for (int i = 0; i < titleCommands.size(); i++)
                {
                    if (HitArea(titleCommands[i].pos.vx, titleCommands[i].pos.vy))
                    {
                        if (titleCommands[i].next_part < 0)
                        {
                            cIndexNo = i;
                            cursor.SetPositionI(titleCommands[i].pos.vx - 16 + position_setting_x[(int)lANGUAGE_CODE], titleCommands[i].pos.vy + title_command_height / 2);
                            return false;
                        }
                        cursor.SetPositionI(titleCommands[i].pos.vx - 16 + position_setting_x[(int)lANGUAGE_CODE], titleCommands[i].pos.vy + title_command_height / 2);
                        if (OpenFF.Client.ModListScreen.Available && titleCommands[i].next_part >= 4)
                        {
                            // PORT: the fourth entry is the mod list on this client, not the network part.
                            MatrixSound.MtxSENDS_Play(0, 1, 192, 127);
                            OpenFF.Client.ModListScreen.Open();
                            return false;
                        }
                        cIndexNo = titleCommands[i].next_part;
                        return true;
                    }
                }
                return false;
            }

            public bool HitArea(int x, int y)
            {
                ds.g_TouchPanel.getLastPoint(out var x2, out var y2);
                if (x2 > x && x2 < x + title_commnad_width && y2 > y - 20 && y2 < y + title_command_height + 20)
                {
                    return true;
                }
                return false;
            }

            public void TitleJump()
            {
                dgs.CFade.Main().fadeOut(5, dgs.CFade.FADE_TYPE.FADE_TYPE_WHITE);
                dgs.CFade.Sub().fadeOut(5, dgs.CFade.FADE_TYPE.FADE_TYPE_WHITE);
                localState = 3;
            }

            public CTitlePrologue GetTitlePrologue()
            {
                return tPrologue;
            }

            public int GetSelectNo()
            {
                return cIndexNo;
            }
        }

        public class CTitleSystem
        {
            public const int PART_GAME_START = 0;

            public const int PART_GAME_LOAD_GAME = 1;

            public const int PART_GAME_SEACRET = 2;

            private int m_State;

            private int next_part;

            private ds.sys3d.Scene m_Scene;

            private ds.sys3d.CCamera tCamera;

            private CTitle2D title2d = new CTitle2D();

            private CTitlePrologue titlePrologue = new CTitlePrologue();

            private int count;

            ~CTitleSystem()
            {
            }

            public void destruct()
            {
            }

            public void Initialize()
            {
                m_State = 0;
                title2d.initialize();
                title2d.SetUpMainBrou();
                next_part = -1;
                titlePrologue.tpInitialize();
                dgs.CFade.Main().fadeIn(15);
                dgs.CFade.Sub().fadeIn(15);
                OptionSaveDataGlobal.getSingleton().setup();
                if (OptionSaveDataGlobal.getSingleton().isProper())
                {
                    OptionSaveDataGlobal.getSingleton().reflect();
                    return;
                }
                opt.COptionManager.getSingleton().initialize();
                MatrixSound.MtxBGMNDS_SetBaseVolume(opt.COptionManager.getSingleton().soundOption().bgmVolume());
                GX_FixScreen(0);
                card.SaveOption();
            }

            public void CameraSet()
            {
                tCamera.initialize();
                VecFx32 position = new VecFx32(0, 0, 491520);
                VecFx32 target = new VecFx32(0, 0, 0);
                tCamera.setAngle(0, 0, 0);
                tCamera.setPosition(position);
                tCamera.setTarget(target);
                tCamera.setDistance(520192);
                tCamera.SetM_Mode(1000);
            }

            public bool Execute()
            {
                bool flag = true;
                switch (m_State)
                {
                    case 0:
                        if (title2d.T_ProcessBrou())
                        {
                            titlePrologue.tpBegin();
                            m_State = 1;
                            count = 0;
                        }
                        return true;
                    case 1:
                        if (count++ == 300)
                        {
                            dgs.CFade.Main().fadeOut(30, dgs.CFade.FADE_TYPE.FADE_TYPE_WHITE);
                            dgs.CFade.Sub().fadeOut(30, dgs.CFade.FADE_TYPE.FADE_TYPE_WHITE);
                            m_State = 4;
                        }
                        else if (title2d.T_ProcessMain())
                        {
                            m_State = 2;
                            title2d.SetUpMainSelect();
                        }
                        break;
                    case 2:
                        if (title2d.T_ProcessSelect())
                        {
                            m_State = 3;
                        }
                        break;
                    case 3:
                        next_part = title2d.GetSelectNo();
                        if (next_part == 4)
                        {
                            UserInfo.confirm_state = 1;
                            AppShell.purchaseApp();
                            next_part = -1;
                            m_State = 6;
                        }
                        break;
                    case 4:
                        if (dgs.CFade.Main().isFaded())
                        {
                            title2d.terminate();
                            titlePrologue.tpInitialize();
                            titlePrologue.tpBegin();
                            m_State = 5;
                        }
                        break;
                    case 5:
                        if (!titlePrologue.tpProcess())
                        {
                            next_part = 3;
                            return false;
                        }
                        break;
                    case 6:
                        if (UserInfo.confirm_state == 2 || UserInfo.confirm_state == 3)
                        {
                            next_part = 4;
                            if (UserInfo.confirm_state == 3)
                            {
                                UserInfo.confirm_state = 0;
                            }
                        }
                        break;
                }
                return true;
            }

            public void OnDraw()
            {
                sys2d.DS2DManager.d2dGetInstance().d2dExecute();
                if (ds.CDevice.singleton().getFPS() == ds.CDevice.enFPS.enFPS_30)
                {
                    ulong num = (ulong)OS_GetTick() - ds.CDevice.singleton().getPreVBlankTick();
                    if (num <= 8000)
                    {
                        ds.CDevice.singleton().waitVBlank();
                    }
                }
            }

            public void Terminate()
            {
                titlePrologue.tpTerminate();
                title2d.terminate();
                GX_DisableBankForTex();
                GX_DisableBankForTexPltt();
                GX_DisableBankForBG();
                GX_DisableBankForBGExtPltt();
                GX_DisableBankForOBJ();
                GX_DisableBankForOBJExtPltt();
                GX_DisableBankForSubBG();
                GX_DisableBankForSubBGExtPltt();
                GX_DisableBankForSubOBJ();
                GX_DisableBankForSubOBJExtPltt();
                GX_SetBankForTex(GXVRamTex.GX_VRAM_TEX_NONE);
                GX_SetBankForTexPltt(GXVRamTexPltt.GX_VRAM_TEXPLTT_NONE);
                GX_SetBankForBG(GXVRamBG.GX_VRAM_BG_NONE);
                GX_SetBankForBGExtPltt(GXVRamBGExtPltt.GX_VRAM_BGEXTPLTT_NONE);
                GX_SetBankForOBJ(GXVRamOBJ.GX_VRAM_OBJ_NONE);
                GX_SetBankForOBJExtPltt(GXVRamOBJExtPltt.GX_VRAM_OBJEXTPLTT_NONE);
                GX_SetBankForSubBG(GXVRamSubBG.GX_VRAM_SUB_BG_NONE);
                GX_SetBankForSubBGExtPltt(GXVRamSubBGExtPltt.GX_VRAM_SUB_BGEXTPLTT_NONE);
                GX_SetBankForSubOBJ(GXVRamSubOBJ.GX_VRAM_SUB_OBJ_NONE);
                GX_SetBankForSubOBJExtPltt(GXVRamSubOBJExtPltt.GX_VRAM_SUB_OBJEXTPLTT_NONE);
            }

            public CTitle2D GetTitle2D()
            {
                return title2d;
            }

            public int GetNextPart()
            {
                return next_part;
            }
        }

        public const int TP_STATE_SETUP = 0;

        public const int TP_STATE_FADEIN = 1;

        public const int TP_STATE_PAUSE = 2;

        public const int TP_STATE_FADEOUT = 3;

        public const int TP_STATE_END = 4;

        private const int NUMBER_OF_MSG = 5;

        private const int NUMBER_OF_ARTS = 5;

        public const int T_PROC_FIRST = 0;

        public const int T_PROC_SECOND = 1;

        public const int T_PROC_THIRD = 2;

        public const int T_PROC_FOUR = 3;

        public const int T_PROC_DEMO_START = 4;

        public const int T_PROC_DEMO = 5;

        public const int T_PROC_PURCHASE = 6;

        private static bool _hEnd = false;

        private static int OFFSET_UNIT_X = 4;

        private static int OFFSET_UNIT_Y = 3;

        private static int FADEIN_DURATION = 64;

        private static int FADEOUT_DURATION = 64;

        private static int PAUSE_DURATION = 168;

        private static ds.Vector2<short>[] ofsDirection = new ds.Vector2<short>[5]
        {
                                new ds.Vector2<short>(-1, -1),
                                new ds.Vector2<short>(1, -1),
                                new ds.Vector2<short>(1, 1),
                                new ds.Vector2<short>(-1, 1),
                                new ds.Vector2<short>(0, 0)
        };

        private static string[] artName = new string[5] { "art_00", "art_01", "art_02", "art_03", "art_04" };

        private static int[] msgID = new int[5] { 20000101, 20000102, 20000103, 20000104, 20000105 };

        public static int MENU_ITEM_X_SHIFT = 80;

        public static int TOUCH_TO_START_WIDTH = 96;

        public static int CONTINUE_POS_X = 192 + MENU_ITEM_X_SHIFT;

        public static int CONTINUE_POS_Y = 220;

        public static int NEW_GAME_POS_X = 192 - MENU_ITEM_X_SHIFT;

        public static int NEW_GAME_POS_Y = 220;

        public static int LOAD_GAME_POS_X = 192 - MENU_ITEM_X_SHIFT;

        public static int LOAD_GAME_POS_Y = 260;

        public static int WIFI_POS_X = 192 + MENU_ITEM_X_SHIFT;

        public static int WIFI_POS_Y = 260;

        private static int[] position_setting_x = new int[5] { 0, -20, -20, -20, -20 };

        public static int msg_one_speed = 1;

        public static int title_commnad_width = 96;

        public static int title_command_height = 16;

        private static int relate_max = 300;
    }
}
