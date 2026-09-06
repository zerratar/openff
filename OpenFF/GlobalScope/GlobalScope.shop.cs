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
    public static class shop
    {
        public class CBaseShop
        {
            public enum SHOP_STATE
            {
                SHOP_STATE_ERR = -1,
                SHOP_STATE_SELECT_SHOP_COMMAND,
                SHOP_STATE_BUY_ITEM,
                SHOP_STATE_SELL_ITEM,
                SHOP_STATE_SELECT_ITEM_NUM,
                SHOP_STATE_EXIT,
                SHOP_STATE_MAX
            }

            public enum SHOP_BG
            {
                SHOP_BG_A,
                SHOP_BG_B,
                SHOP_BG_C,
                SHOP_BG_MAX
            }

            public enum SHOP_WINDOW
            {
                SHOP_WINDOW_A,
                SHOP_WINDOW_B,
                SHOP_WINDOW_C,
                SHOP_WINDOW_D,
                SHOP_WINDOW_E,
                SHOP_WINDOW_F,
                SHOP_WINDOW_G,
                SHOP_WINDOW_MAX
            }

            public const SHOP_STATE SHOP_STATE_ERR = SHOP_STATE.SHOP_STATE_ERR;

            public const SHOP_STATE SHOP_STATE_SELECT_SHOP_COMMAND = SHOP_STATE.SHOP_STATE_SELECT_SHOP_COMMAND;

            public const SHOP_STATE SHOP_STATE_BUY_ITEM = SHOP_STATE.SHOP_STATE_BUY_ITEM;

            public const SHOP_STATE SHOP_STATE_SELL_ITEM = SHOP_STATE.SHOP_STATE_SELL_ITEM;

            public const SHOP_STATE SHOP_STATE_SELECT_ITEM_NUM = SHOP_STATE.SHOP_STATE_SELECT_ITEM_NUM;

            public const SHOP_STATE SHOP_STATE_EXIT = SHOP_STATE.SHOP_STATE_EXIT;

            public const SHOP_STATE SHOP_STATE_MAX = SHOP_STATE.SHOP_STATE_MAX;

            public const SHOP_BG SHOP_BG_A = SHOP_BG.SHOP_BG_A;

            public const SHOP_BG SHOP_BG_B = SHOP_BG.SHOP_BG_B;

            public const SHOP_BG SHOP_BG_C = SHOP_BG.SHOP_BG_C;

            public const SHOP_BG SHOP_BG_MAX = SHOP_BG.SHOP_BG_MAX;

            public const SHOP_WINDOW SHOP_WINDOW_A = SHOP_WINDOW.SHOP_WINDOW_A;

            public const SHOP_WINDOW SHOP_WINDOW_B = SHOP_WINDOW.SHOP_WINDOW_B;

            public const SHOP_WINDOW SHOP_WINDOW_C = SHOP_WINDOW.SHOP_WINDOW_C;

            public const SHOP_WINDOW SHOP_WINDOW_D = SHOP_WINDOW.SHOP_WINDOW_D;

            public const SHOP_WINDOW SHOP_WINDOW_E = SHOP_WINDOW.SHOP_WINDOW_E;

            public const SHOP_WINDOW SHOP_WINDOW_F = SHOP_WINDOW.SHOP_WINDOW_F;

            public const SHOP_WINDOW SHOP_WINDOW_G = SHOP_WINDOW.SHOP_WINDOW_G;

            public const SHOP_WINDOW SHOP_WINDOW_MAX = SHOP_WINDOW.SHOP_WINDOW_MAX;

            public static bool m_cEnd = false;

            public static bool[] m_BGFlag = new bool[3];

            public static sys2d.Bg[] m_BG = new sys2d.Bg[3]
            {
                                    new sys2d.Bg(),
                                    new sys2d.Bg(),
                                    new sys2d.Bg()
            };

            public static int[] m_WindowID = new int[7];

            protected SHOP_STATE m_State;

            protected SHOP_STATE m_PreviousState;

            protected int m_ItemNum;

            protected ds.Vector2<short> m_ItemPos = new ds.Vector2<short>();

            protected CBaseShopState[] m_pCurrentState = new CBaseShopState[5];

            protected CShopStateCommandSelect m_ShopStateCommandSelect = new CShopStateCommandSelect();

            protected CShopStateBuyItem m_ShopStateBuyItem = new CShopStateBuyItem();

            protected CShopStateSellItem m_ShopStateSellItem = new CShopStateSellItem();

            protected CShopStateSelectItemNum m_ShopStateSelectItemNum = new CShopStateSelectItemNum();

            protected CShopStateExit m_ShopStateExit = new CShopStateExit();

            public void reset()
            {
                m_cEnd = false;
                for (int i = 0; i < 3; i++)
                {
                    m_BGFlag[i] = false;
                }
                for (int j = 0; j < 7; j++)
                {
                    m_WindowID[j] = -1;
                }
            }

            public CBaseShop()
            {
                m_State = SHOP_STATE.SHOP_STATE_ERR;
                m_PreviousState = SHOP_STATE.SHOP_STATE_ERR;
                for (int i = 0; i < 5; i++)
                {
                    m_pCurrentState[i] = null;
                }
            }

            public virtual void initialize()
            {
            }

            public virtual void execute()
            {
            }

            public virtual void terminate()
            {
            }

            public bool End()
            {
                return m_cEnd;
            }

            public void End_set(bool arg0)
            {
                m_cEnd = arg0;
            }

            public bool BGFlag(int _Index)
            {
                if (_Index >= 0)
                {
                    _ = 2;
                }
                return m_BGFlag[_Index];
            }

            public void BGFlag_set(int _Index, bool arg0)
            {
                if (_Index >= 0)
                {
                    _ = 2;
                }
                m_BGFlag[_Index] = arg0;
            }

            public sys2d.Bg BG(int _Index)
            {
                if (_Index >= 0)
                {
                    _ = 2;
                }
                return m_BG[_Index];
            }

            public int WindowID(int _Index)
            {
                if (_Index >= 0)
                {
                    _ = 6;
                }
                return m_WindowID[_Index];
            }

            public void WindowID_set(int _Index, int arg0)
            {
                if (_Index >= 0)
                {
                    _ = 6;
                }
                m_WindowID[_Index] = arg0;
            }

            public void setState(SHOP_STATE _State)
            {
                setPreviousState(m_State);
                m_State = _State;
            }

            public SHOP_STATE getState()
            {
                return m_State;
            }

            public void setPreviousState(SHOP_STATE _PreviousState)
            {
                m_PreviousState = _PreviousState;
            }

            public SHOP_STATE getPreviousState()
            {
                return m_PreviousState;
            }

            public void setItemNum(int _ItemNum)
            {
                m_ItemNum = _ItemNum;
            }

            public int getItemNum()
            {
                return m_ItemNum;
            }

            public void setItemPos(ds.Vector2<short> _ItemPos)
            {
                m_ItemPos.copy(_ItemPos);
            }

            public ds.Vector2<short> getItemPos()
            {
                return m_ItemPos;
            }

            public CBaseShopState pCurrentState()
            {
                return m_pCurrentState[(int)m_State];
            }

            protected virtual void registerShopState()
            {
            }
        }

        public class CBaseShopState
        {
            public enum SHOP_STATE_PHASE
            {
                SHOP_STATE_PHASE_ERR = -1,
                SHOP_STATE_PHASE_START,
                SHOP_STATE_PHASE_UPDATE,
                SHOP_STATE_PHASE_END,
                SHOP_STATE_PHASE_MAX
            }

            public const SHOP_STATE_PHASE SHOP_STATE_PHASE_ERR = SHOP_STATE_PHASE.SHOP_STATE_PHASE_ERR;

            public const SHOP_STATE_PHASE SHOP_STATE_PHASE_START = SHOP_STATE_PHASE.SHOP_STATE_PHASE_START;

            public const SHOP_STATE_PHASE SHOP_STATE_PHASE_UPDATE = SHOP_STATE_PHASE.SHOP_STATE_PHASE_UPDATE;

            public const SHOP_STATE_PHASE SHOP_STATE_PHASE_END = SHOP_STATE_PHASE.SHOP_STATE_PHASE_END;

            public const SHOP_STATE_PHASE SHOP_STATE_PHASE_MAX = SHOP_STATE_PHASE.SHOP_STATE_PHASE_MAX;

            protected SHOP_STATE_PHASE m_Phase;

            public CBaseShopState()
            {
                m_Phase = SHOP_STATE_PHASE.SHOP_STATE_PHASE_ERR;
            }

            public virtual void start(CBaseShop arg0)
            {
            }

            public virtual void update(CBaseShop arg0)
            {
            }

            public virtual void end(CBaseShop arg0)
            {
            }

            public void setPhase(SHOP_STATE_PHASE m_Phase)
            {
                this.m_Phase = m_Phase;
            }

            public SHOP_STATE_PHASE getPhase()
            {
                return m_Phase;
            }
        }

        public class CShopManager
        {
            public enum SHOP_KIND
            {
                SHOP_KIND_ERR = -1,
                SHOP_KIND_WEAPON,
                SHOP_KIND_ARMOR,
                SHOP_KIND_MAGIC,
                SHOP_KIND_ITEM,
                SHOP_KIND_MAX
            }

            public const SHOP_KIND SHOP_KIND_ERR = SHOP_KIND.SHOP_KIND_ERR;

            public const SHOP_KIND SHOP_KIND_WEAPON = SHOP_KIND.SHOP_KIND_WEAPON;

            public const SHOP_KIND SHOP_KIND_ARMOR = SHOP_KIND.SHOP_KIND_ARMOR;

            public const SHOP_KIND SHOP_KIND_MAGIC = SHOP_KIND.SHOP_KIND_MAGIC;

            public const SHOP_KIND SHOP_KIND_ITEM = SHOP_KIND.SHOP_KIND_ITEM;

            public const SHOP_KIND SHOP_KIND_MAX = SHOP_KIND.SHOP_KIND_MAX;

            public static CShopManager c_Instance = new CShopManager();

            private uint m_ShopIndex;

            /// <summary>PORT: a shop table (three letters, e.g. "t01") to use instead of the map's own; null for the map's.</summary>
            public static string OverrideTable;

            private SHOP_KIND m_Kind;

            private CShopParameterManager m_ShopParameterMng = new CShopParameterManager();

            private CBaseShop[] m_pCurrentShop = new CBaseShop[4];

            private CWeaponShop m_WeaponShop = new CWeaponShop();

            private int m_Command;

            public ds.Vector2<short>[] commandPosition_ = new ds.Vector2<short>[3];

            public ds.Vector2<short>[] commandArea_ = new ds.Vector2<short>[3];

            public uint ShopIndex()
            {
                return m_ShopIndex;
            }

            public void ShopIndex_set(uint arg0)
            {
                m_ShopIndex = arg0;
            }

            public void initialize()
            {
                for (int i = 0; i < 3; i++)
                {
                    commandPosition_[i].set(0, 0);
                    commandArea_[i].set(0, 0);
                }
                m_Command = 0;
                changeGlobalDirectory();
                strncpy(out var arg, wld.CWorldOutSideData.getInstance().MapData().getNowShopMapName(), 3);
                sprintf(out arg, "%s.shp", arg);
                // PORT: the engine API opens a shop from any map; it names the table itself.
                if (OverrideTable != null)
                {
                    sprintf(out arg, "%s.shp", OverrideTable);
                }
                if (ds.g_File.getSize(arg) == 0)
                {
                    sprintf(out arg, "%s.shp", "t01");
                }
                else
                {
                    ShopParameterMng().load(arg);
                }
                m_Kind = (SHOP_KIND)ShopParameterMng().ShopParameter(ShopIndex()).ShopKind();
                if (m_Kind > SHOP_KIND.SHOP_KIND_ERR)
                {
                    _ = 4;
                    _ = m_Kind;
                }
                changeCompanyDirectory();
                menu.MenuManager.getSingleton().LoadXbnFile("ShopDefine.xbn");
                menu.MenuManager.getSingleton().Set2d3dMode(2);
                menu.MenuManager.getSingleton().CreateNeedObject(menu.MenuManager.MENU_CREATE_PARAMETER.CREATE_MENU_MODE_2D);
                menu.MenuManager.getSingleton().GetCursor2d().SetShow(show: true);
                setupMessage();
                wmenu.CWMenuManager.Instance().GetMenuButton().initialize();
                GX_Power3D(0);
                menu.MenuManager.getSingleton().Set2d3dMode(2);
                menu.MenuManager.getSingleton().buildMenu(TRANSCODE("shop_sub"));
                changeGlobalDirectory();
                pCurrentShop().BGFlag_set(0, arg0: true);
                pCurrentShop().BG(0).bgLoad("shop_bg00.NSCR", "menu_bg_01.NCGR", "new_menu_bg.NCLR");
                pCurrentShop().BG(0).bgSetUp(NNSG2dBGSelect.NNS_G2D_BGSELECT_SUB0);
                pCurrentShop().BG(0).bgRelease();
                pCurrentShop().BGFlag_set(2, arg0: false);
                pCurrentShop().BG(2).bgLoad("shop_bg01.NSCR", "menu_bg_01.NCGR", "new_menu_bg.NCLR");
                pCurrentShop().BG(2).bgSetUp(NNSG2dBGSelect.NNS_G2D_BGSELECT_SUB3);
                pCurrentShop().BG(2).bgRelease();
                ds.CVram.setSubPlaneVisiblity(bg0: true, bg1: true, bg2: false, bg3: false, obj: true);
                pCurrentShop().initialize();
            }

            public void execute()
            {
                OS_AssignBackButton(1);
                menu.MenuManager.getSingleton().execute();
                pCurrentShop().execute();
            }

            public void terminate()
            {
                menu.MenuManager.getSingleton().ReleaseXbnFile();
                menu.MenuManager.getSingleton().ResetWindowSystem();
                menu.MenuManager.getSingleton().releaseWindowAll();
                menu.MenuManager.getSingleton().release();
                menu.MenuManager.getSingleton().ReleaseItemDataText();
                menu.MenuManager.getSingleton().ReleaseMenuDataText();
                menu.MenuManager.getSingleton().DeleteNeedObject(menu.MenuManager.MENU_CREATE_PARAMETER.CREATE_MENU_MODE_3D);
                GX_Power3D(1);
                for (int i = 0; i < 3; i++)
                {
                    if (pCurrentShop().BGFlag(i))
                    {
                        pCurrentShop().BG(i).bgRelease();
                    }
                }
                for (int j = 0; j < 7; j++)
                {
                    pCurrentShop().WindowID_set(j, -1);
                }
                wmenu.CWMenuManager.Instance().GetMenuButton().terminate();
                cleanupMessage();
                ShopParameterMng().free();
                pCurrentShop().terminate();
            }

            public int getTouchCommand()
            {
                if (!ds.g_TouchPanel.isEdge())
                {
                    return -1;
                }
                ds.g_TouchPanel.getPoint(out var x, out var y);
                for (int i = 0; i < 3; i++)
                {
                    if (commandPosition_[i].vx <= x && x <= commandPosition_[i].vx + commandArea_[i].vx && commandPosition_[i].vy <= y && y <= commandPosition_[i].vy + commandArea_[i].vy)
                    {
                        m_Command = i;
                        return i;
                    }
                }
                return -1;
            }

            public void registerShopKind()
            {
                m_pCurrentShop[0] = m_WeaponShop;
                m_pCurrentShop[1] = m_WeaponShop;
                m_pCurrentShop[2] = m_WeaponShop;
                m_pCurrentShop[3] = m_WeaponShop;
            }

            public void setupMessage()
            {
                dgs.msg.CMessageSys.getInstance().Main().assignBG(1, 0, 0, 32, 24);
                dgs.msg.CMessageSys.getInstance().Sub().assignBG(1, 0, 0, 32, 24);
                menu.MenuManager.getSingleton().Set2d3dMode(3);
                menu.MenuManager.getSingleton().CreateItemDataText();
                menu.MenuManager.getSingleton().Set2d3dMode(2);
                menu.MenuManager.getSingleton().CreateItemDataText();
                menu.MenuManager.getSingleton().Set2d3dMode(3);
                menu.MenuManager.getSingleton().CreateMenuDataText(0);
                menu.MenuManager.getSingleton().Set2d3dMode(2);
                menu.MenuManager.getSingleton().CreateMenuDataText(0);
            }

            public void cleanupMessage()
            {
                menu.MenuManager.getSingleton().Set2d3dMode(3);
                menu.MenuManager.getSingleton().ReleaseItemDataText();
                menu.MenuManager.getSingleton().Set2d3dMode(2);
                menu.MenuManager.getSingleton().ReleaseItemDataText();
                menu.MenuManager.getSingleton().Set2d3dMode(3);
                menu.MenuManager.getSingleton().ReleaseMenuDataText();
                menu.MenuManager.getSingleton().Set2d3dMode(2);
                menu.MenuManager.getSingleton().ReleaseMenuDataText();
            }

            public CShopManager()
            {
                m_ShopIndex = 0u;
                m_Kind = SHOP_KIND.SHOP_KIND_ERR;
                for (int i = 0; i < commandPosition_.Length; i++)
                {
                    commandPosition_[i] = new ds.Vector2<short>();
                }
                for (int j = 0; j < commandArea_.Length; j++)
                {
                    commandArea_[j] = new ds.Vector2<short>();
                }
                for (int k = 0; k < 4; k++)
                {
                    m_pCurrentShop[k] = null;
                }
                registerShopKind();
            }

            public static CShopManager Instance()
            {
                return c_Instance;
            }

            public SHOP_KIND Kind()
            {
                return m_Kind;
            }

            public CShopParameterManager ShopParameterMng()
            {
                return m_ShopParameterMng;
            }

            public CBaseShop pCurrentShop()
            {
                return m_pCurrentShop[(int)m_Kind];
            }

            public int getCommand()
            {
                return m_Command;
            }
        }

        public class CShopParameterManager
        {
            private Array m_FileAddr;

            private CShopParameter[] m_ShopParameter;

            public bool load(string _FileName)
            {
                free();
                bool result = false;
                if (_FileName == null)
                {
                    return result;
                }
                uint size = ds.g_File.getSize(_FileName);
                m_FileAddr = ds.CHeap.alloc_app(size);
                result = ds.g_File.load(m_FileAddr, _FileName);
                m_ShopParameter = CShopParameter.castArray(m_FileAddr);
                return result;
            }

            public void free()
            {
                if (m_FileAddr != null)
                {
                    ds.CHeap.free_app(m_FileAddr);
                    m_FileAddr = null;
                }
            }

            public CShopParameter ShopParameter(uint _Index)
            {
                return m_ShopParameter[_Index];
            }

            public CShopParameterManager()
            {
                m_FileAddr = null;
                m_ShopParameter = null;
            }
        }

        public class CShopParameter
        {
            public const int ITEM_LIST_NUM = 12;

            private short m_ShopKind;

            private short[] m_ItemId = new short[12];

            public short ShopKind()
            {
                return m_ShopKind;
            }

            public short ItemId(int _Index)
            {
                return m_ItemId[_Index];
            }

            public static CShopParameter[] castArray(Array src)
            {
                CShopParameter[] array = new CShopParameter[src.Length / 26];
                ArrayReader arrayReader = new ArrayReader(src);
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = (CShopParameter)arrayReader;
                }
                arrayReader.dispose();
                return array;
            }

            public static explicit operator CShopParameter(ArrayReader src)
            {
                CShopParameter cShopParameter = new CShopParameter();
                cShopParameter.m_ShopKind = src.readInt16();
                src.read(cShopParameter.m_ItemId, 0, 12);
                return cShopParameter;
            }
        }

        public class CShopStateCommandSelect : CBaseShopState
        {
            public enum SHOP_COMMAND
            {
                SHOP_COMMAND_ERR = -1,
                SHOP_COMMAND_BUY,
                SHOP_COMMAND_SELL,
                SHOP_COMMAND_EXIT,
                SHOP_COMMAND_MAX
            }

            public const SHOP_COMMAND SHOP_COMMAND_ERR = SHOP_COMMAND.SHOP_COMMAND_ERR;

            public const SHOP_COMMAND SHOP_COMMAND_BUY = SHOP_COMMAND.SHOP_COMMAND_BUY;

            public const SHOP_COMMAND SHOP_COMMAND_SELL = SHOP_COMMAND.SHOP_COMMAND_SELL;

            public const SHOP_COMMAND SHOP_COMMAND_EXIT = SHOP_COMMAND.SHOP_COMMAND_EXIT;

            public const SHOP_COMMAND SHOP_COMMAND_MAX = SHOP_COMMAND.SHOP_COMMAND_MAX;

            protected SHOP_COMMAND m_Command;

            public override void start(CBaseShop _CurrentShop)
            {
                m_Command = SHOP_COMMAND.SHOP_COMMAND_ERR;
                ds.CVram.setSubPlaneVisiblity(bg0: true, bg1: true, bg2: false, bg3: false, obj: true);
                m_pMedget = menu.MenuManager.getSingleton().getFocuseMedget().parentNode()
                    .nextSibling();
                if (m_pMedget != null)
                {
                    pPlayerGold = static_cast<menu.MBPlayerGold>(m_pMedget.behavior().queryInterface(menu.MBPlayerGold.classIdentifier()));
                }
                GXS_SetVisibleWnd(0);
                menu.MenuManager.getSingleton().SetDecideButtonState(1);
                menu.MenuManager.getSingleton().SetCancelButtonState(1);
                menu.MenuManager.getSingleton().inputPermission(b: false);
                _waitCounter = 3;
                menu.Medget medget = null;
                for (int i = 0; i < 3; i++)
                {
                    string[] array = new string[3] { "buy", "sell", "exit" };
                    medget = menu.MenuManager.getSingleton().root().getNodeByID(array[i]);
                    if (medget != null)
                    {
                        CShopManager.Instance().commandPosition_[i].set(medget.x(), medget.y());
                        CShopManager.Instance().commandArea_[i].set(medget.width(), medget.height());
                    }
                    else
                    {
                        CShopManager.Instance().commandPosition_[i].set(0, 0);
                        CShopManager.Instance().commandArea_[i].set(0, 0);
                    }
                }
            }

            public override void update(CBaseShop _CurrentShop)
            {
                if (_waitCounter > 0)
                {
                    _waitCounter--;
                    if (_waitCounter == 0)
                    {
                        menu.MenuManager.getSingleton().inputPermission(b: true);
                    }
                }
                if (menu.MenuManager.getSingleton().GetCancelButtonState() == 0 || wmenu.CWMenuManager.Instance().GetMenuButton().TouchButtonB())
                {
                    menu.MenuManager.getSingleton().playSECancel();
                    ds.g_Pad.disable();
                    ds.g_TouchPanel.disable();
                    m_Command = SHOP_COMMAND.SHOP_COMMAND_EXIT;
                    setPhase(SHOP_STATE_PHASE.SHOP_STATE_PHASE_END);
                    dummyCursor_.SetShow(show: false);
                }
                else if (menu.MenuManager.getSingleton().GetDecideButtonState() == 0)
                {
                    switch (menu.MenuManager.getSingleton().getFocuseMedget().myTag())
                    {
                        case 0:
                            menu.MenuManager.getSingleton().playSEDecide();
                            m_Command = SHOP_COMMAND.SHOP_COMMAND_BUY;
                            setPhase(SHOP_STATE_PHASE.SHOP_STATE_PHASE_END);
                            break;
                        case 1:
                            menu.MenuManager.getSingleton().playSEDecide();
                            m_Command = SHOP_COMMAND.SHOP_COMMAND_SELL;
                            setPhase(SHOP_STATE_PHASE.SHOP_STATE_PHASE_END);
                            break;
                        case 2:
                            menu.MenuManager.getSingleton().playSECancel();
                            m_Command = SHOP_COMMAND.SHOP_COMMAND_EXIT;
                            setPhase(SHOP_STATE_PHASE.SHOP_STATE_PHASE_END);
                            break;
                    }
                }
                menu.MenuManager.getSingleton().SetDecideButtonState(1);
                menu.MenuManager.getSingleton().SetCancelButtonState(1);
            }

            public override void end(CBaseShop _CurrentShop)
            {
                switch (m_Command)
                {
                    case SHOP_COMMAND.SHOP_COMMAND_BUY:
                        _CurrentShop.setState(CBaseShop.SHOP_STATE.SHOP_STATE_BUY_ITEM);
                        break;
                    case SHOP_COMMAND.SHOP_COMMAND_SELL:
                        _CurrentShop.setState(CBaseShop.SHOP_STATE.SHOP_STATE_SELL_ITEM);
                        break;
                    case SHOP_COMMAND.SHOP_COMMAND_EXIT:
                        _CurrentShop.setState(CBaseShop.SHOP_STATE.SHOP_STATE_EXIT);
                        break;
                }
            }

            public void initializeDummyCursor()
            {
                dummyCursor_.copy(menu.MenuManager.getSingleton().GetCursor2d());
                dummyCursor_.SetCell(0);
                dummyCursor_.SetAnimation(anm: true);
                dummyCursor_.SetPriority(0);
                dummyCursor_.SetPositionI(256, 192);
                dummyCursor_.SetShow(show: false);
                sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(dummyCursor_);
            }

            public void terminateDummyCursor()
            {
                dummyCursor_.Release();
                sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(dummyCursor_);
            }
        }

        public class CShopStateBuyItem : CBaseShopState
        {
            public enum SHOP_BUY
            {
                SHOP_BUY_ERR = -1,
                SHOP_BUY_DECIDE,
                SHOP_BUY_CANSEL,
                SHOP_BUY_COMMAND,
                SHOP_BUY_MAX
            }

            public const SHOP_BUY SHOP_BUY_ERR = SHOP_BUY.SHOP_BUY_ERR;

            public const SHOP_BUY SHOP_BUY_DECIDE = SHOP_BUY.SHOP_BUY_DECIDE;

            public const SHOP_BUY SHOP_BUY_CANSEL = SHOP_BUY.SHOP_BUY_CANSEL;

            public const SHOP_BUY SHOP_BUY_COMMAND = SHOP_BUY.SHOP_BUY_COMMAND;

            public const SHOP_BUY SHOP_BUY_MAX = SHOP_BUY.SHOP_BUY_MAX;

            protected SHOP_BUY m_Buy;

            public override void start(CBaseShop _CurrentShop)
            {
                m_Buy = SHOP_BUY.SHOP_BUY_ERR;
                if (_CurrentShop.getPreviousState() != CBaseShop.SHOP_STATE.SHOP_STATE_SELECT_ITEM_NUM)
                {
                    changeGlobalDirectory();
                    wmenu.CWMenuManager.Instance().GetPcFace().pcfmSetup(NNSG2dBGSelect.NNS_G2D_BGSELECT_SUB1);
                    wmenu.CWMenuManager.Instance().GetPcFace().pcfmCleanup();
                    for (int i = 0; i < 4; i++)
                    {
                        if (pl.PlayerParty.instance().player((byte)i).isEnable())
                        {
                            int num = pl.PlayerParty.instance().player((byte)i).playerId();
                            wmenu.CWMenuManager.Instance().GetPcFace().pcfmSetJob((uint)num, (uint)pl.PlayerParty.instance().player((byte)i).jobManager()
                                .nowJob());
                            wmenu.CWMenuManager.Instance().SetShowPcFace(num, show: true);
                            NNS_G2dSetBGCellScale(8 + num, 0.5714286f);
                        }
                    }
                    menu.Medget nodeByID = menu.MenuManager.getSingleton().root().getNodeByID(TRANSCODE("buy"));
                    dummyCursor_.SetPositionI(nodeByID.cursorX(), nodeByID.cursorY());
                    dummyCursor_.SetPositionI(nodeByID.cursorX(), nodeByID.cursorY());
                    dummyCursor_.SetShow(show: true);
                    menu.MenuManager.getSingleton().Push(TRANSCODE("shop_buy_list"));
                    menu.Medget medget = menu.MenuManager.getSingleton().root().getNodeByID(TRANSCODE("influence"))
                        .childNode();
                    for (int j = 0; j < 4; j++)
                    {
                        if (pl.PlayerParty.instance().player((byte)j).isEnable())
                        {
                            int num2 = pl.PlayerParty.instance().player((byte)j).playerId();
                            NNS_G2dSetBGCellPositon(8 + num2, medget.x() + 8, medget.y() + 4);
                        }
                        medget = medget.nextSibling();
                    }
                    menu.MenuManager.getSingleton().Set2d3dMode(3);
                    string[] array = new string[4]
                    {
                                            TRANSCODE("shop_buy_window_001"),
                                            TRANSCODE("shop_buy_window_002"),
                                            TRANSCODE("shop_buy_window_003"),
                                            TRANSCODE("shop_buy_window_004")
                    };
                    _CurrentShop.WindowID_set(1, menu.MenuManager.getSingleton().buildWindow(TRANSCODE("shop_buy_list"), array[0]));
                    if (_CurrentShop.WindowID(1) != -1)
                    {
                        menu.MenuManager.getSingleton().UpdateWindowState(_CurrentShop.WindowID(1), menu.MenuWindow.MENU_WINDOW_MOVE_TYPE.WINDOW_SIZE_MOVING_LARGE);
                    }
                    _CurrentShop.WindowID_set(2, menu.MenuManager.getSingleton().buildWindow(TRANSCODE("shop_buy_list"), array[1]));
                    if (_CurrentShop.WindowID(2) != -1)
                    {
                        menu.MenuManager.getSingleton().UpdateWindowState(_CurrentShop.WindowID(2), menu.MenuWindow.MENU_WINDOW_MOVE_TYPE.WINDOW_SIZE_MOVING_LARGE);
                    }
                    _CurrentShop.WindowID_set(3, menu.MenuManager.getSingleton().buildWindow(TRANSCODE("shop_buy_list"), array[2]));
                    if (_CurrentShop.WindowID(3) != -1)
                    {
                        menu.MenuManager.getSingleton().UpdateWindowState(_CurrentShop.WindowID(3), menu.MenuWindow.MENU_WINDOW_MOVE_TYPE.WINDOW_SIZE_MOVING_LARGE);
                    }
                    _CurrentShop.WindowID_set(4, menu.MenuManager.getSingleton().buildWindow(TRANSCODE("shop_buy_list"), array[3]));
                    if (_CurrentShop.WindowID(4) != -1)
                    {
                        menu.MenuManager.getSingleton().UpdateWindowState(_CurrentShop.WindowID(4), menu.MenuWindow.MENU_WINDOW_MOVE_TYPE.WINDOW_SIZE_MOVING_LARGE);
                    }
                    menu.MenuManager.getSingleton().Set2d3dMode(2);
                    _CurrentShop.BGFlag_set(1, arg0: true);
                }
                ds.CVram.setSubPlaneVisiblity(bg0: true, bg1: true, bg2: false, bg3: false, obj: true);
                menu.MenuManager.getSingleton().inputPermission(b: false);
                _waitCounter = 3;
                ds.g_Pad.enable();
                ds.g_TouchPanel.enable();
            }

            public override void update(CBaseShop _CurrentShop)
            {
                if (_waitCounter > 0)
                {
                    _waitCounter--;
                    if (_waitCounter == 0)
                    {
                        menu.MenuManager.getSingleton().inputPermission(b: true);
                    }
                }
                if (menu.MenuManager.getSingleton().GetDecideButtonState() == 0)
                {
                    int targetItemNo = menu.MenuManager.getSingleton().GetTargetItemNo();
                    int num = 0;
                    int num2 = 0;
                    for (num2 = 0; num2 < 384 && pl.PlayerParty.instance().item().normalItem(num2)
                        .itemId() != targetItemNo && pl.PlayerParty.instance().item().normalItem(num2)
                        .itemId() > 0; num2++)
                    {
                    }
                    num = pl.PlayerParty.instance().item().normalItem(num2)
                        .itemNumber();
                    if (num < 99)
                    {
                        menu.MenuManager.getSingleton().playSEDecide();
                        m_Buy = SHOP_BUY.SHOP_BUY_DECIDE;
                        setPhase(SHOP_STATE_PHASE.SHOP_STATE_PHASE_END);
                    }
                    else
                    {
                        menu.MenuManager.getSingleton().playSEBeep();
                    }
                }
                else if (CShopManager.Instance().getTouchCommand() == 1)
                {
                    menu.MenuManager.getSingleton().playSEMoveCursor();
                    m_Buy = SHOP_BUY.SHOP_BUY_COMMAND;
                    setPhase(SHOP_STATE_PHASE.SHOP_STATE_PHASE_END);
                    dummyCursor_.SetShow(show: false);
                }
                else if (CShopManager.Instance().getTouchCommand() == 2)
                {
                    menu.MenuManager.getSingleton().playSECancel();
                    m_Buy = SHOP_BUY.SHOP_BUY_CANSEL;
                    setPhase(SHOP_STATE_PHASE.SHOP_STATE_PHASE_END);
                    dummyCursor_.SetShow(show: false);
                }
                else if (wmenu.CWMenuManager.Instance().GetMenuButton().TouchButtonB())
                {
                    menu.MenuManager.getSingleton().playSECancel();
                    m_Buy = SHOP_BUY.SHOP_BUY_CANSEL;
                    setPhase(SHOP_STATE_PHASE.SHOP_STATE_PHASE_END);
                    dummyCursor_.SetShow(show: false);
                }
                else if (menu.MenuManager.getSingleton().GetCancelButtonState() == 0)
                {
                    menu.MenuManager.getSingleton().playSECancel();
                    m_Buy = SHOP_BUY.SHOP_BUY_CANSEL;
                    setPhase(SHOP_STATE_PHASE.SHOP_STATE_PHASE_END);
                    dummyCursor_.SetShow(show: false);
                }
                if (pPlayerGold != null)
                {
                    pPlayerGold.bmBehave(m_pMedget);
                }
                menu.MenuManager.getSingleton().SetDecideButtonState(1);
                menu.MenuManager.getSingleton().SetCancelButtonState(1);
            }

            public override void end(CBaseShop _CurrentShop)
            {
                switch (m_Buy)
                {
                    case SHOP_BUY.SHOP_BUY_CANSEL:
                    case SHOP_BUY.SHOP_BUY_COMMAND:
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                if (_CurrentShop.WindowID(i + 1) != -1)
                                {
                                    menu.MenuManager.getSingleton().releaseWindow(_CurrentShop.WindowID(i + 1));
                                    _CurrentShop.WindowID_set(i + 1, -1);
                                }
                            }
                            if (_CurrentShop.BGFlag(1))
                            {
                                _CurrentShop.BG(1).bgRelease();
                                _CurrentShop.BGFlag_set(1, arg0: false);
                            }
                            menu.MenuManager.getSingleton().Pop();
                            if (m_Buy == SHOP_BUY.SHOP_BUY_COMMAND)
                            {
                                _CurrentShop.setState(CBaseShop.SHOP_STATE.SHOP_STATE_SELL_ITEM);
                            }
                            else
                            {
                                _CurrentShop.setState(CBaseShop.SHOP_STATE.SHOP_STATE_EXIT);
                            }
                            for (int j = 0; j < 4; j++)
                            {
                                wmenu.CWMenuManager.Instance().SetShowPcFace(j, show: false);
                            }
                            break;
                        }
                    case SHOP_BUY.SHOP_BUY_DECIDE:
                        _CurrentShop.setState(CBaseShop.SHOP_STATE.SHOP_STATE_SELECT_ITEM_NUM);
                        break;
                }
            }
        }

        public class CShopStateSellItem : CBaseShopState
        {
            public enum SHOP_SELL
            {
                SHOP_SELL_ERR = -1,
                SHOP_SELL_DECIDE,
                SHOP_SELL_CANSEL,
                SHOP_SELL_COMMAND,
                SHOP_SELL_MAX
            }

            public const SHOP_SELL SHOP_SELL_ERR = SHOP_SELL.SHOP_SELL_ERR;

            public const SHOP_SELL SHOP_SELL_DECIDE = SHOP_SELL.SHOP_SELL_DECIDE;

            public const SHOP_SELL SHOP_SELL_CANSEL = SHOP_SELL.SHOP_SELL_CANSEL;

            public const SHOP_SELL SHOP_SELL_COMMAND = SHOP_SELL.SHOP_SELL_COMMAND;

            public const SHOP_SELL SHOP_SELL_MAX = SHOP_SELL.SHOP_SELL_MAX;

            protected SHOP_SELL m_Sell;

            public override void start(CBaseShop _CurrentShop)
            {
                m_Sell = SHOP_SELL.SHOP_SELL_ERR;
                if (_CurrentShop.getPreviousState() != CBaseShop.SHOP_STATE.SHOP_STATE_SELECT_ITEM_NUM)
                {
                    menu.Medget nodeByID = menu.MenuManager.getSingleton().root().getNodeByID(TRANSCODE("sell"));
                    dummyCursor_.SetPositionI(nodeByID.cursorX(), nodeByID.cursorY());
                    dummyCursor_.SetShow(show: true);
                    menu.MenuManager.getSingleton().Push(TRANSCODE("shop_sell_list"));
                    menu.MenuManager.getSingleton().Set2d3dMode(3);
                    string[] array = new string[4]
                    {
                                            TRANSCODE("shop_buy_window_001"),
                                            TRANSCODE("shop_buy_window_002"),
                                            TRANSCODE("shop_buy_window_003"),
                                            TRANSCODE("shop_buy_window_004")
                    };
                    for (int i = 0; i < 4; i++)
                    {
                        _CurrentShop.WindowID_set(i + 1, menu.MenuManager.getSingleton().buildWindow(TRANSCODE("shop_buy_list"), array[i]));
                        if (_CurrentShop.WindowID(i + 1) != -1)
                        {
                            menu.MenuManager.getSingleton().UpdateWindowState(_CurrentShop.WindowID(i + 1), menu.MenuWindow.MENU_WINDOW_MOVE_TYPE.WINDOW_SIZE_MOVING_LARGE);
                        }
                    }
                    menu.MenuManager.getSingleton().Set2d3dMode(2);
                    _CurrentShop.BGFlag_set(1, arg0: true);
                }
                ds.CVram.setSubPlaneVisiblity(bg0: true, bg1: true, bg2: false, bg3: false, obj: true);
                menu.MenuManager.getSingleton().inputPermission(b: false);
                _waitCounter = 3;
            }

            public override void update(CBaseShop _CurrentShop)
            {
                if (_waitCounter > 0)
                {
                    _waitCounter--;
                    if (_waitCounter == 0)
                    {
                        menu.MenuManager.getSingleton().inputPermission(b: true);
                    }
                }
                ds.g_TouchPanel.getPoint(out var _, out var _);
                if (menu.MenuManager.getSingleton().GetDecideButtonState() == 0)
                {
                    int targetItemNo = menu.MenuManager.getSingleton().GetTargetItemNo();
                    if (targetItemNo > 0)
                    {
                        menu.MenuManager.getSingleton().playSEDecide();
                        m_Sell = SHOP_SELL.SHOP_SELL_DECIDE;
                        setPhase(SHOP_STATE_PHASE.SHOP_STATE_PHASE_END);
                    }
                    else
                    {
                        menu.MenuManager.getSingleton().playSEBeep();
                    }
                }
                else if (CShopManager.Instance().getTouchCommand() == 0)
                {
                    menu.MenuManager.getSingleton().playSEMoveCursor();
                    m_Sell = SHOP_SELL.SHOP_SELL_COMMAND;
                    setPhase(SHOP_STATE_PHASE.SHOP_STATE_PHASE_END);
                    dummyCursor_.SetShow(show: false);
                }
                else if (CShopManager.Instance().getTouchCommand() == 2)
                {
                    menu.MenuManager.getSingleton().playSECancel();
                    m_Sell = SHOP_SELL.SHOP_SELL_CANSEL;
                    setPhase(SHOP_STATE_PHASE.SHOP_STATE_PHASE_END);
                    dummyCursor_.SetShow(show: false);
                }
                else if (wmenu.CWMenuManager.Instance().GetMenuButton().TouchButtonB())
                {
                    menu.MenuManager.getSingleton().playSECancel();
                    m_Sell = SHOP_SELL.SHOP_SELL_CANSEL;
                    setPhase(SHOP_STATE_PHASE.SHOP_STATE_PHASE_END);
                    dummyCursor_.SetShow(show: false);
                }
                else if (menu.MenuManager.getSingleton().GetCancelButtonState() == 0)
                {
                    menu.MenuManager.getSingleton().playSECancel();
                    m_Sell = SHOP_SELL.SHOP_SELL_CANSEL;
                    setPhase(SHOP_STATE_PHASE.SHOP_STATE_PHASE_END);
                    dummyCursor_.SetShow(show: false);
                }
                if (pPlayerGold != null)
                {
                    pPlayerGold.bmBehave(m_pMedget);
                }
                menu.MenuManager.getSingleton().SetDecideButtonState(1);
                menu.MenuManager.getSingleton().SetCancelButtonState(1);
            }

            public override void end(CBaseShop _CurrentShop)
            {
                switch (m_Sell)
                {
                    case SHOP_SELL.SHOP_SELL_CANSEL:
                    case SHOP_SELL.SHOP_SELL_COMMAND:
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                if (_CurrentShop.WindowID(i + 1) != -1)
                                {
                                    menu.MenuManager.getSingleton().releaseWindow(_CurrentShop.WindowID(i + 1));
                                    _CurrentShop.WindowID_set(i + 1, -1);
                                }
                            }
                            if (_CurrentShop.BGFlag(1))
                            {
                                _CurrentShop.BGFlag_set(1, arg0: false);
                            }
                            menu.MenuManager.getSingleton().Pop();
                            if (m_Sell == SHOP_SELL.SHOP_SELL_COMMAND)
                            {
                                _CurrentShop.setState(CBaseShop.SHOP_STATE.SHOP_STATE_BUY_ITEM);
                            }
                            else
                            {
                                _CurrentShop.setState(CBaseShop.SHOP_STATE.SHOP_STATE_EXIT);
                            }
                            break;
                        }
                    case SHOP_SELL.SHOP_SELL_DECIDE:
                        _CurrentShop.setState(CBaseShop.SHOP_STATE.SHOP_STATE_SELECT_ITEM_NUM);
                        break;
                }
            }
        }

        public class CShopStateSelectItemNum : CBaseShopState
        {
            public enum SHOP_SELECT_ITEM_NUM
            {
                SHOP_SELECT_ITEM_NUM_ERR = -1,
                SHOP_SELECT_ITEM_NUM_CANSEL,
                SHOP_SELECT_ITEM_NUM_MAX
            }

            public const SHOP_SELECT_ITEM_NUM SHOP_SELECT_ITEM_NUM_ERR = SHOP_SELECT_ITEM_NUM.SHOP_SELECT_ITEM_NUM_ERR;

            public const SHOP_SELECT_ITEM_NUM SHOP_SELECT_ITEM_NUM_CANSEL = SHOP_SELECT_ITEM_NUM.SHOP_SELECT_ITEM_NUM_CANSEL;

            public const SHOP_SELECT_ITEM_NUM SHOP_SELECT_ITEM_NUM_MAX = SHOP_SELECT_ITEM_NUM.SHOP_SELECT_ITEM_NUM_MAX;

            protected SHOP_SELECT_ITEM_NUM m_SelectItemNum;

            public override void start(CBaseShop _CurrentShop)
            {
                m_SelectItemNum = SHOP_SELECT_ITEM_NUM.SHOP_SELECT_ITEM_NUM_ERR;
                _CurrentShop.setItemNum(1);
                if (_CurrentShop.getPreviousState() == CBaseShop.SHOP_STATE.SHOP_STATE_BUY_ITEM || _CurrentShop.getPreviousState() == CBaseShop.SHOP_STATE.SHOP_STATE_SELL_ITEM)
                {
                    menu.MenuManager.getSingleton().Push(TRANSCODE("shop_number"));
                    menu.MenuManager.getSingleton().Set2d3dMode(2);
                    int num = GXS_GetVisiblePlane();
                    num |= 8;
                    GXS_SetVisiblePlane(num);
                }
                menu.MenuManager.getSingleton().SetDecideButtonState(1);
                menu.MenuManager.getSingleton().SetCancelButtonState(1);
            }

            public override void update(CBaseShop _CurrentShop)
            {
                int targetItemNo = menu.MenuManager.getSingleton().GetTargetItemNo();
                itm.NotImportantParameter notImportantParameter = static_cast<itm.NotImportantParameter>(itm.ItemManager.instance().itemParameter((short)targetItemNo));
                int num = notImportantParameter.buy();
                int num2 = notImportantParameter.price();
                int itemNum = _CurrentShop.getItemNum();
                int num3 = 0;
                int num4 = pl.PlayerParty.instance().gold().get();
                if (wmenu.CWMenuManager.Instance().GetMenuButton().TouchButtonB())
                {
                    menu.MenuManager.getSingleton().playSECancel();
                    m_SelectItemNum = SHOP_SELECT_ITEM_NUM.SHOP_SELECT_ITEM_NUM_CANSEL;
                    setPhase(SHOP_STATE_PHASE.SHOP_STATE_PHASE_END);
                }
                else if (menu.MenuManager.getSingleton().GetCancelButtonState() == 0 || (menu.MenuManager.getSingleton().GetDecideButtonState() == 0 && menu.MenuManager.getSingleton().getFocuseMedget().myTag() == 1))
                {
                    menu.MenuManager.getSingleton().playSECancel();
                    m_SelectItemNum = SHOP_SELECT_ITEM_NUM.SHOP_SELECT_ITEM_NUM_CANSEL;
                    setPhase(SHOP_STATE_PHASE.SHOP_STATE_PHASE_END);
                }
                else if (menu.MenuManager.getSingleton().GetDecideButtonState() == 0 || wmenu.CWMenuManager.Instance().GetMenuButton().TouchButtonA())
                {
                    bool flag = false;
                    if (_CurrentShop.getPreviousState() == CBaseShop.SHOP_STATE.SHOP_STATE_BUY_ITEM)
                    {
                        bool flag2 = false;
                        int i = 0;
                        num3 = num * itemNum;
                        num3 = discount(num3, itemNum);
                        if (num4 < num3)
                        {
                            menu.MenuManager.getSingleton().playSEBeep();
                            menu.MenuManager.getSingleton().SetDecideButtonState(1);
                            menu.MenuManager.getSingleton().SetCancelButtonState(1);
                            return;
                        }
                        if (!flag2)
                        {
                            for (i = 0; i < 384; i++)
                            {
                                if (pl.PlayerParty.instance().item().normalItem(i)
                                    .itemId() == targetItemNo)
                                {
                                    flag2 = true;
                                    break;
                                }
                            }
                        }
                        if (!flag2)
                        {
                            for (i = 0; i < 384; i++)
                            {
                                if (pl.PlayerParty.instance().item().normalItem(i)
                                    .itemId() <= 0)
                                {
                                    pl.PlayerParty.instance().item().normalItem(i)
                                        .setItemId((short)targetItemNo);
                                    flag2 = true;
                                    break;
                                }
                            }
                        }
                        byte b = pl.PlayerParty.instance().item().normalItem(i)
                            .itemNumber();
                        b += (byte)itemNum;
                        if (b > 99)
                        {
                            num3 = num * (99 - pl.PlayerParty.instance().item().normalItem(i)
                                .itemNumber());
                            b = 99;
                        }
                        pl.PlayerParty.instance().item().normalItem(i)
                            .setItemNumber(b);
                        num4 -= num3;
                        pl.PlayerParty.instance().gold().set(num4);
                        flag = true;
                        MatrixSound.MtxSENDS_Play(0, 6, 192, 127);
                    }
                    else if (_CurrentShop.getPreviousState() == CBaseShop.SHOP_STATE.SHOP_STATE_SELL_ITEM)
                    {
                        num3 = num2 * itemNum;
                        int num5 = 0;
                        for (num5 = 0; num5 < 384; num5++)
                        {
                            if (pl.PlayerParty.instance().item().normalItem(num5)
                                .itemId() != targetItemNo)
                            {
                                continue;
                            }
                            int num6 = pl.PlayerParty.instance().item().normalItem(num5)
                                .itemNumber();
                            num6 -= itemNum;
                            if (num6 <= 0)
                            {
                                pl.PlayerParty.instance().item().normalItem(num5)
                                    .setItemId(-1);
                                num6 = 0;
                            }
                            pl.PlayerParty.instance().item().normalItem(num5)
                                .setItemNumber(num6);
                            num4 += num3;
                            pl.PlayerParty.instance().gold().set(num4);
                            if (pl.PlayerParty.instance().gold().get() >= 50000)
                            {
                                UserInfo.AwardAchievement(6);
                                if (pl.PlayerParty.instance().gold().get() >= 500000)
                                {
                                    UserInfo.AwardAchievement(7);
                                }
                            }
                            flag = true;
                            break;
                        }
                        MatrixSound.MtxSENDS_Play(0, 7, 192, 127);
                    }
                    if (flag)
                    {
                        m_SelectItemNum = SHOP_SELECT_ITEM_NUM.SHOP_SELECT_ITEM_NUM_CANSEL;
                        setPhase(SHOP_STATE_PHASE.SHOP_STATE_PHASE_END);
                    }
                }
                menu.MenuManager.getSingleton().SetDecideButtonState(1);
                menu.MenuManager.getSingleton().SetCancelButtonState(1);
            }

            public override void end(CBaseShop _CurrentShop)
            {
                wmenu.CWMenuManager.Instance().GetMenuButton().SetButtonAActivity(b: false);
                if (m_SelectItemNum == SHOP_SELECT_ITEM_NUM.SHOP_SELECT_ITEM_NUM_CANSEL)
                {
                    if (_CurrentShop.WindowID(6) != -1)
                    {
                        menu.MenuManager.getSingleton().releaseWindow(_CurrentShop.WindowID(6));
                        _CurrentShop.WindowID_set(6, -1);
                    }
                    menu.MenuManager.getSingleton().Pop();
                    if (_CurrentShop.BGFlag(2))
                    {
                        _CurrentShop.BGFlag_set(2, arg0: false);
                    }
                    _CurrentShop.setState(_CurrentShop.getPreviousState());
                }
            }
        }

        public class CShopStateExit : CBaseShopState
        {
            public override void start(CBaseShop _CurrentShop)
            {
                menu.MenuManager.getSingleton().setFocuseMedget(CShopManager.Instance().getCommand());
            }

            public override void update(CBaseShop _CurrentShop)
            {
                setPhase(SHOP_STATE_PHASE.SHOP_STATE_PHASE_END);
            }

            public override void end(CBaseShop _CurrentShop)
            {
                menu.MenuManager.getSingleton().inputPermission(b: true);
                _CurrentShop.End_set(arg0: true);
            }
        }

        public class CShopUpDisplayComposition
        {
            public const int DRAW_CHARACTER_NUM = 4;

            public static CShopUpDisplayComposition m_cInstance = new CShopUpDisplayComposition();

            public static VecFx32[] m_cCharacterPosition = new VecFx32[4]
            {
                                    new VecFx32(-61440, 0, 409600),
                                    new VecFx32(-20480, 0, 409600),
                                    new VecFx32(20480, 0, 409600),
                                    new VecFx32(61440, 0, 409600)
            };

            public static ushort[] m_cCharacterRotationY = new ushort[4]
            {
                                    ds.DEGto65536(-32),
                                    ds.DEGto65536(-36),
                                    ds.DEGto65536(-40),
                                    ds.DEGto65536(-44)
            };

            private int[] m_ChrIndex = new int[4];

            public void initialize()
            {
                VecFx32 vecFx = new VecFx32();
                VecFx32 vecFx2 = new VecFx32();
                for (int i = 0; i < 4; i++)
                {
                    pl.Player player = pl.PlayerParty.instance().player((byte)i);
                    m_ChrIndex[i] = -1;
                    if (player.isEnable())
                    {
                        VEC_Set(vecFx, 4096, 4096, 4096);
                        VEC_Set(vecFx2, 4915, 4096, 4915);
                        sprintf(out var arg, "j%d%02d", pl.PlayerParty.instance().player((byte)i).playerId() + 1, pl.PlayerParty.instance().player((byte)i).jobManager()
                            .nowJob() + 1);
                        strcpy(out var arg2, "w_act_man");
                        m_ChrIndex[i] = characterMng.setCharacter(arg, CCharacterMng.PRI_SCENE.PRI_SCENE_FIRST);
                        characterMng.addMotion(m_ChrIndex[i], arg2);
                        characterMng.addMotion(m_ChrIndex[i], "b_b04_001");
                        characterMng.startMotion(m_ChrIndex[i], 4101 + player.playerId(), fLoop: true, 5u);
                        characterMng.setPosition(m_ChrIndex[i], m_cCharacterPosition[i]);
                        characterMng.setRotation(m_ChrIndex[i], 0, m_cCharacterRotationY[i], 0);
                        characterMng.setScale(m_ChrIndex[i], vecFx);
                        characterMng.setShadowScale(m_ChrIndex[i], vecFx2);
                        characterMng.setShadowType(m_ChrIndex[i], 1);
                    }
                }
                TexDivideLoader.getSingleton().tdlForceLoad();
                for (byte b = 0; b < 4; b++)
                {
                    if (-1 != m_ChrIndex[b])
                    {
                        characterMng.setupOrgTex(m_ChrIndex[b]);
                    }
                }
            }

            public void execute()
            {
            }

            public void terminate()
            {
                for (int i = 0; i < 4; i++)
                {
                    if (m_ChrIndex[i] != -1)
                    {
                        characterMng.delCharacter(m_ChrIndex[i]);
                        m_ChrIndex[i] = -1;
                    }
                }
            }

            public static CShopUpDisplayComposition Instance()
            {
                return m_cInstance;
            }

            public int charIndex(int player_num)
            {
                return m_ChrIndex[player_num];
            }
        }

        public class CWeaponShop : CBaseShop
        {
            public override void initialize()
            {
                reset();
                registerShopState();
                End_set(arg0: false);
                m_ShopStateCommandSelect.start(this);
                m_ShopStateCommandSelect.initializeDummyCursor();
                setState(SHOP_STATE.SHOP_STATE_BUY_ITEM);
                setPreviousState(SHOP_STATE.SHOP_STATE_SELECT_SHOP_COMMAND);
                ds.g_Pad.disable();
                ds.g_TouchPanel.disable();
                m_ItemNum = 1;
                pCurrentState().start(this);
                pCurrentState().setPhase(CBaseShopState.SHOP_STATE_PHASE.SHOP_STATE_PHASE_UPDATE);
            }

            public override void execute()
            {
                if (pCurrentState().getPhase() == CBaseShopState.SHOP_STATE_PHASE.SHOP_STATE_PHASE_START)
                {
                    pCurrentState().start(this);
                    pCurrentState().setPhase(CBaseShopState.SHOP_STATE_PHASE.SHOP_STATE_PHASE_UPDATE);
                }
                else if (pCurrentState().getPhase() == CBaseShopState.SHOP_STATE_PHASE.SHOP_STATE_PHASE_UPDATE)
                {
                    pCurrentState().update(this);
                }
                else if (pCurrentState().getPhase() == CBaseShopState.SHOP_STATE_PHASE.SHOP_STATE_PHASE_END)
                {
                    pCurrentState().end(this);
                    pCurrentState().setPhase(CBaseShopState.SHOP_STATE_PHASE.SHOP_STATE_PHASE_START);
                    if (!End())
                    {
                        pCurrentState().start(this);
                        pCurrentState().setPhase(CBaseShopState.SHOP_STATE_PHASE.SHOP_STATE_PHASE_UPDATE);
                    }
                }
            }

            public override void terminate()
            {
                m_ShopStateCommandSelect.terminateDummyCursor();
            }

            protected override void registerShopState()
            {
                m_pCurrentState[0] = m_ShopStateCommandSelect;
                m_pCurrentState[1] = m_ShopStateBuyItem;
                m_pCurrentState[2] = m_ShopStateSellItem;
                m_pCurrentState[3] = m_ShopStateSelectItemNum;
                m_pCurrentState[4] = m_ShopStateExit;
            }
        }

        internal static int discount(int price, int num)
        {
            if (price <= 0)
            {
                return 0;
            }
            int result = price;
            if (4 <= num && num < 10)
            {
                result = (int)((float)price * 0.95f);
            }
            else if (num >= 10)
            {
                float num2 = 0.95f - (float)(num / 5 + 1) * 0.01f;
                if (num2 < 0.8f)
                {
                    num2 = 0.8f;
                }
                result = (int)((float)price * num2);
            }
            return result;
        }
    }
}
