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
	public static partial class menu
	{
		public class MenuManager
		{
			public enum MENU_CREATE_PARAMETER
			{
				CREATE_MENU_MODE_2D = 2,
				CREATE_MENU_MODE_3D = 3,
				CREATE_MENU_WINDOW_MAX = CREATE_MENU_MODE_2D
			}

			public enum MENU_CANCEL_BUTTON
			{
				PUSH_BUTTON,
				REMOVE_BUTTON
			}

			public enum USING_MENU_TYPE
			{
				TYPE_BATTLE,
				TYPE_MENU
			}

			public enum SCROLL_TYPE
			{
				TYPE_WAIT,
				TYPE_UP,
				TYPE_DOWN
			}

			public enum TOUCH_STATE
			{
				NOT_TOUCH,
				NOT_TOUCH_ITEM,
				TOUCH_ITEM
			}

			public class MENU_BACKUP
			{
				public Medget @base;

				public ds.Vector<Medget, ds.FastErasePolicy<Medget>> focusBackup = new ds.Vector<Medget, ds.FastErasePolicy<Medget>>(96);

				public int cursorBackup;

				public MENU_BACKUP(MENU_BACKUP src)
				{
					@base = src.@base;
					focusBackup = src.focusBackup;
					cursorBackup = src.cursorBackup;
				}

				public MENU_BACKUP(Medget arg0, ds.Vector<Medget, ds.FastErasePolicy<Medget>> arg1, int arg2)
				{
					@base = arg0;
					focusBackup = arg1;
					cursorBackup = arg2;
				}
			}

			public const MENU_CREATE_PARAMETER CREATE_MENU_MODE_2D = MENU_CREATE_PARAMETER.CREATE_MENU_MODE_2D;

			public const MENU_CREATE_PARAMETER CREATE_MENU_MODE_3D = MENU_CREATE_PARAMETER.CREATE_MENU_MODE_3D;

			public const MENU_CREATE_PARAMETER CREATE_MENU_WINDOW_MAX = MENU_CREATE_PARAMETER.CREATE_MENU_MODE_2D;

			public const MENU_CANCEL_BUTTON PUSH_BUTTON = MENU_CANCEL_BUTTON.PUSH_BUTTON;

			public const MENU_CANCEL_BUTTON REMOVE_BUTTON = MENU_CANCEL_BUTTON.REMOVE_BUTTON;

			public const USING_MENU_TYPE TYPE_BATTLE = USING_MENU_TYPE.TYPE_BATTLE;

			public const USING_MENU_TYPE TYPE_MENU = USING_MENU_TYPE.TYPE_MENU;

			public const SCROLL_TYPE TYPE_WAIT = SCROLL_TYPE.TYPE_WAIT;

			public const SCROLL_TYPE TYPE_UP = SCROLL_TYPE.TYPE_UP;

			public const SCROLL_TYPE TYPE_DOWN = SCROLL_TYPE.TYPE_DOWN;

			public const int PATERN_NORMAL = 0;

			public const int PATERN_IMPORTANT = 1;

			public const int PATERN_MAGIC = 2;

			public const int PATERN_WEAPON = 3;

			public const int PATERN_PROTECTION = 4;

			public const int PATERN_ATTACK_WEAPON = 5;

			public const int PATERN_GUARD_WEAPON = 6;

			public const int PATERN_HEAD = 7;

			public const int PATERN_BODY = 8;

			public const int PATERN_GUNTRET = 9;

			public const int PATERN_STORED = 10;

			public const int PATERN_BATTLE_ITEM = 11;

			public const TOUCH_STATE NOT_TOUCH = TOUCH_STATE.NOT_TOUCH;

			public const TOUCH_STATE NOT_TOUCH_ITEM = TOUCH_STATE.NOT_TOUCH_ITEM;

			public const TOUCH_STATE TOUCH_ITEM = TOUCH_STATE.TOUCH_ITEM;

			private const int LIMIT_OF_FOCUSLIST = 96;

			public static MenuManager instance_ = new MenuManager();

			private SCROLL_TYPE scrollType;

			private bool trialInitFocuse;

			private bool imposibleScroll;

			private dgs.MSDINFO g_MsdAddr;

			private dgs.MSDINFO g_MenuMsdAddr;

			private dgs.MSDINFO g_SpMsdAddr;

			private dgs.MSDINFO g_MNMsdAddr;

			private int itemDataTextNo;

			private int menuDataTextNo;

			private int specialDataTextNo;

			private int mognetDataTextNo;

			private MenuWindow[] windowObj = new MenuWindow[2]
			{
				new MenuWindow(),
				new MenuWindow()
			};

			private int mode2d3d;

			private int targetItemNo;

			private int targetLineNo;

			private int targetItemTemp;

			private int targetCharNo;

			private uint current;

			private Xbn xbnDocument = new Xbn();

			private Medget baseMedget;

			private int decideButton;

			private int cancelButton;

			private int directionKey;

			private int activateButton;

			private int menu_type;

			private int magicMenu_type;

			private sys2d.Cell cellCursor2d = new sys2d.Cell();

			private sys2d.Sprite3d cellCursor3d = new sys2d.Sprite3d();

			private sys2d.Cell sIcon2d = new sys2d.Cell();

			private sys2d.Sprite3d sIcon3d = new sys2d.Sprite3d();

			private sys2d.Cell mIcon2d = new sys2d.Cell();

			private sys2d.Sprite3d mIcon3d = new sys2d.Sprite3d();

			private sys2d.Cell mIcon2d_2 = new sys2d.Cell();

			private sys2d.Sprite3d mIcon3d_2 = new sys2d.Sprite3d();

			private int itemListPatern;

			private int focusedCursor;

			private Medget focusedMedget;

			private ds.Vector<Medget, ds.FastErasePolicy<Medget>> focusMedgets = new ds.Vector<Medget, ds.FastErasePolicy<Medget>>(96);

			private ds.Stack<MENU_BACKUP> stkMenuLevel = new ds.Stack<MENU_BACKUP>(8);

			private ds.Vector<Medget, ds.OrderSavedErasePolicy<Medget>> appendedMenuList = new ds.Vector<Medget, ds.OrderSavedErasePolicy<Medget>>(4);

			private bool inputPermit;

			private bool touchedFlag;

			private bool prevTouchedFlag_;

			private bool updateMessageFlag_;

			private bool pitchFlag_;

			private bool openDoorFlag_;

			private bool notSEFlag_;

			private TOUCH_STATE touchState_;

			private SaveCursor[] saveCursor_ = new SaveCursor[4];

			private bool battleMode_;

			private bool tempFlag_;

			private bool terminateBehave_;

			public MenuManager()
			{
				for (int i = 0; i < saveCursor_.Length; i++)
				{
					saveCursor_[i] = new SaveCursor();
				}
			}

			public void beginning()
			{
				baseMedget = null;
				inputPermit = true;
				mode2d3d = 3;
				targetItemNo ^= targetItemNo;
				targetCharNo ^= targetCharNo;
				itemListPatern = 0;
				touchState_ = TOUCH_STATE.NOT_TOUCH;
				updateMessageFlag_ = false;
				pitchFlag_ = false;
				battleMode_ = false;
				openDoorFlag_ = false;
				for (int i = 0; i < 4; i++)
				{
					saveCursor_[i].saveBattlePitchLine_ = 0;
					saveCursor_[i].saveBattlePitchTarget_ = 0;
					saveCursor_[i].saveBattleEquipLine_ = 0;
					saveCursor_[i].saveBattleEquipTarget_ = 0;
					saveCursor_[i].saveBattleEquipHand_ = 0;
					saveCursor_[i].saveBattleItemLine_ = 0;
					saveCursor_[i].saveBattleMagicLine_ = 0;
					saveCursor_[i].saveBattleItemTarget_ = 0;
					saveCursor_[i].saveBattleMagicTarget_ = 0;
					saveCursor_[i].saveSongTarget_ = 0;
					saveCursor_[i].saveBattleUseItem_ = 0;
					saveCursor_[i].saveBattleUseItemHand_ = 0;
				}
				decideButton = 1;
				cancelButton = 1;
			}

			public void ending()
			{
			}

			public void initialize()
			{
				loadSprite();
				baseMedget = null;
				while (stkMenuLevel.empty() == 0)
				{
					stkMenuLevel.pop();
				}
				appendedMenuList.clear();
				focusMedgets.clear();
				focusedMedget = null;
				focusedCursor = -1;
				targetItemNo ^= targetItemNo;
				targetCharNo ^= targetCharNo;
				scrollType = SCROLL_TYPE.TYPE_WAIT;
				itemListPatern = 0;
				inputPermit = true;
				trialInitFocuse = true;
				updateMessageFlag_ = false;
				pitchFlag_ = false;
				openDoorFlag_ = false;
				notSEFlag_ = false;
				terminateBehave_ = false;
			}

			public void terminate()
			{
				releaseSprite();
			}

			public void loadSprite()
			{
				if (mode2d3d == 2)
				{
					changeGlobalDirectory();
					cellCursor2d.Load2(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_SUB2D, "icon_yubi");
					cellCursor2d.SetShow(show: false);
					cellCursor2d.SetCell(0);
					cellCursor2d.SetPriority(0);
					cellCursor2d.SetPositionI(LCD_WIDTH, LCD_HEIGHT);
					sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(cellCursor2d);
					sIcon2d.Load(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_SUB2D, "icon_8dot.NCER", null, "icon_8dot.NCGR", "icon_8dot.NCLR");
					sIcon2d.ceReleaseCgCl();
					changeCompanyDirectory();
					mIcon2d.Load2(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_SUB2D, "icon_16dot");
					mIcon2d.SetAnimation(anm: false);
					mIcon2d_2.Load2(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_SUB2D, "icon_16dot_2");
					mIcon2d_2.SetAnimation(anm: false);
				}
				else if (mode2d3d == 3)
				{
					changeGlobalDirectory();
					cellCursor3d.Load2(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN3D, "icon_yubi");
					cellCursor3d.SetShow(show: false);
					cellCursor3d.SetCell(0);
					cellCursor3d.SetDepth(0);
					cellCursor3d.SetPositionI(LCD_WIDTH, LCD_HEIGHT);
					sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(cellCursor3d);
					sIcon3d.Load(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN3D, "icon_8dot.NCER", null, "icon_8dot.NCBR", "icon_8dot.NCLR");
					sIcon3d.s3dReleaseCgCl(vram_free: false);
					changeCompanyDirectory();
					mIcon3d.Load2(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN3D, "icon_16dot");
					mIcon3d.SetAnimation(anm: false);
					mIcon3d_2.Load2(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN3D, "icon_16dot_2");
					mIcon3d_2.SetAnimation(anm: false);
				}
			}

			public void releaseSprite()
			{
				if (mode2d3d == 2)
				{
					NNS_G2dReleaseImageProxy(cellCursor2d.GetImageProxy());
					NNS_G2dReleaseImageProxy(sIcon2d.GetImageProxy());
					NNS_G2dReleaseImageProxy(mIcon2d.GetImageProxy());
					NNS_G2dReleaseImageProxy(mIcon2d_2.GetImageProxy());
					cellCursor2d.Release();
					sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(cellCursor2d);
					sIcon2d.Release();
					sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(sIcon2d);
					mIcon2d.Release();
					sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(mIcon2d);
					mIcon2d_2.Release();
					sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(mIcon2d_2);
				}
				else if (mode2d3d == 3)
				{
					NNS_G2dReleaseImageProxy(cellCursor3d.GetImageProxy());
					NNS_G2dReleaseImageProxy(sIcon3d.GetImageProxy());
					NNS_G2dReleaseImageProxy(mIcon3d.GetImageProxy());
					NNS_G2dReleaseImageProxy(mIcon3d_2.GetImageProxy());
					cellCursor3d.Release();
					sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(cellCursor3d);
					sIcon3d.Release();
					sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(sIcon3d);
					mIcon3d.Release();
					sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(mIcon3d);
					mIcon3d_2.Release();
					sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(mIcon3d_2);
				}
			}

			public void LoadXbnFile(string fileName)
			{
				XbnFile xbnFile = xbnDocument.xbnFinalize();
				if (xbnFile != null)
				{
					ds.CHeap.free_app(xbnFile);
				}
				xbnFile = null;
				Array array = null;
				uint size = ds.g_File.getSize(fileName);
				if (size != 0)
				{
					array = ds.CHeap.alloc_app(size);
					if (array != null)
					{
						ds.g_File.load(array, fileName);
						// PORT: the mods' menu screens ride in MenuDefine.xbn (OpenFF.Client.ModMenus).
						array = OpenFF.Client.ModMenus.Patch(fileName, array);
					}
					xbnFile = (XbnFile)array;
				}
				xbnDocument.xbnInitilaize(xbnFile);
			}

			public void ReleaseXbnFile()
			{
				XbnFile xbnFile = xbnDocument.xbnFinalize();
				if (xbnFile != null)
				{
					ds.CHeap.free_app(xbnFile);
				}
			}

			public void SetWindowSystem()
			{
				BasicWindow.bwInitializeSystem(5u);
				ButtonWindow.bwInitializeSystem(5u);
			}

			public void ResetWindowSystem()
			{
				BasicWindow.bwReleaseSystem();
				ButtonWindow.bwReleaseSystem();
			}

			public void CreateItemDataText()
			{
				Array array = null;
				string filename = "eureka_item.msd";
				uint size = ds.g_File.getSize(filename);
				if (g_MsdAddr == null && size != 0)
				{
					array = ds.CHeap.alloc_app(size);
					if (array != null)
					{
						ds.g_File.load(array, filename);
					}
					g_MsdAddr = (dgs.MSDINFO)array;
				}
				if (g_MsdAddr != null)
				{
					dgs.msg.CMessageSys.getInstance().Sub().setUpMSD(g_MsdAddr, dgs.msg.CMessageMng.MSD_HANDLE_KIND.MSD_HANDLE_KIND_GAME_PART);
					int num = dgs.msg.CMessageSys.getInstance().Sub().m_MsdHandle[1];
					dgs.msg.CMessageSys.getInstance().Main().setUpMSD(g_MsdAddr, dgs.msg.CMessageMng.MSD_HANDLE_KIND.MSD_HANDLE_KIND_GAME_PART);
					num = dgs.msg.CMessageSys.getInstance().Main().m_MsdHandle[1];
					getSingleton().SetItemDataTextNo(num);
				}
			}

			public void ReleaseItemDataText()
			{
				if (g_MsdAddr != null)
				{
					dgs.msg.CMessageSys.getInstance().Main().removeMSD(g_MsdAddr);
					dgs.msg.CMessageSys.getInstance().Sub().removeMSD(g_MsdAddr);
					ds.CHeap.free_app(g_MsdAddr);
					g_MsdAddr = null;
				}
			}

			public void CreateMenuDataText(int i)
			{
				Array array = null;
				string filename = "eureka_menu.msd";
				uint size = ds.g_File.getSize(filename);
				if (g_MenuMsdAddr == null)
				{
					if (size != 0)
					{
						array = ds.CHeap.alloc_app(size);
						if (array != null)
						{
							ds.g_File.load(array, filename);
						}
					}
					g_MenuMsdAddr = (dgs.MSDINFO)array;
				}
				dgs.msg.CMessageMng cMessageMng = null;
				if (g_MenuMsdAddr != null)
				{
					cMessageMng = dgs.msg.CMessageSys.getInstance().Main();
					cMessageMng.setUpMSD(g_MenuMsdAddr, dgs.msg.CMessageMng.MSD_HANDLE_KIND.MSD_HANDLE_KIND_GAME_PART2);
					int num = cMessageMng.m_MsdHandle[2];
					if (i == 0)
					{
						cMessageMng = dgs.msg.CMessageSys.getInstance().Sub();
						cMessageMng.setUpMSD(g_MenuMsdAddr, dgs.msg.CMessageMng.MSD_HANDLE_KIND.MSD_HANDLE_KIND_GAME_PART2);
						num = cMessageMng.m_MsdHandle[2];
					}
					getSingleton().SetMenuDataTextNo(num);
				}
			}

			public void ReleaseMenuDataText()
			{
				if (g_MenuMsdAddr != null)
				{
					dgs.msg.CMessageSys.getInstance().Main().removeMSD(g_MenuMsdAddr);
					dgs.msg.CMessageSys.getInstance().Sub().removeMSD(g_MenuMsdAddr);
					ds.CHeap.free_app(g_MenuMsdAddr);
					g_MenuMsdAddr = null;
				}
			}

			public void CreateSpecialDataText()
			{
				Array array = null;
				string filename = "eureka_special.msd";
				uint size = ds.g_File.getSize(filename);
				if (g_SpMsdAddr == null)
				{
					if (size != 0)
					{
						array = ds.CHeap.alloc_app(size);
						if (array != null)
						{
							ds.g_File.load(array, filename);
						}
					}
					g_SpMsdAddr = (dgs.MSDINFO)array;
				}
				dgs.msg.CMessageMng cMessageMng = null;
				if (mode2d3d == 3)
				{
					cMessageMng = dgs.msg.CMessageSys.getInstance().Main();
				}
				else if (mode2d3d == 2)
				{
					cMessageMng = dgs.msg.CMessageSys.getInstance().Sub();
				}
				if (g_SpMsdAddr != null)
				{
					cMessageMng.setUpMSD(g_SpMsdAddr, dgs.msg.CMessageMng.MSD_HANDLE_KIND.MSD_HANDLE_KIND_GAME_PART2);
					int num = cMessageMng.m_MsdHandle[2];
					getSingleton().SetSpecialDataTextNo(num);
				}
			}

			public void ReleaseSpecialDataText()
			{
				if (g_SpMsdAddr != null)
				{
					dgs.msg.CMessageSys.getInstance().Main().removeMSD(g_SpMsdAddr);
					dgs.msg.CMessageSys.getInstance().Sub().removeMSD(g_SpMsdAddr);
					ds.CHeap.free_app(g_SpMsdAddr);
					g_SpMsdAddr = null;
				}
			}

			public void CreateMognetDataText()
			{
				int num = CreateDataText("eureka_mognet.msd", g_MNMsdAddr, dgs.msg.CMessageMng.MSD_HANDLE_KIND.MSD_HANDLE_KIND_GAME_PART);
				_ = 0;
				SetMognetDataTextNo(num);
			}

			public void ReleaseMognetDataText()
			{
				ReleaseDataText(g_MNMsdAddr);
				g_MNMsdAddr = null;
			}

			public int CreateDataText(string strMSDFName, dgs.MSDINFO pAddr, dgs.msg.CMessageMng.MSD_HANDLE_KIND Kind)
			{
				if (pAddr == null)
				{
					Array array = null;
					uint size = ds.g_File.getSize(strMSDFName);
					if (size != 0)
					{
						array = ds.CHeap.alloc_app(size);
						if (array != null)
						{
							ds.g_File.load(array, strMSDFName);
						}
						pAddr = (dgs.MSDINFO)array;
					}
				}
				dgs.msg.CMessageMng cMessageMng = null;
				if (mode2d3d == 3)
				{
					cMessageMng = dgs.msg.CMessageSys.getInstance().Main();
				}
				else if (mode2d3d == 2)
				{
					cMessageMng = dgs.msg.CMessageSys.getInstance().Sub();
				}
				if (pAddr != null && cMessageMng.setUpMSD(pAddr, Kind))
				{
					return cMessageMng.m_MsdHandle[(int)Kind];
				}
				return -1;
			}

			public void ReleaseDataText(dgs.MSDINFO pAddr)
			{
				if (pAddr != null)
				{
					if (mode2d3d == 3)
					{
						dgs.msg.CMessageSys.getInstance().Main().removeMSD(pAddr);
					}
					else if (mode2d3d == 2)
					{
						dgs.msg.CMessageSys.getInstance().Sub().removeMSD(pAddr);
					}
					ds.CHeap.free_app(pAddr);
				}
			}

			public void CreateNeedObject(MENU_CREATE_PARAMETER type)
			{
				if (type == MENU_CREATE_PARAMETER.CREATE_MENU_MODE_3D)
				{
					cellCursor3d.SetShow(show: true);
				}
				else
				{
					cellCursor2d.SetShow(show: true);
				}
			}

			public void DeleteNeedObject(MENU_CREATE_PARAMETER type)
			{
				cellCursor3d.SetShow(show: false);
				cellCursor2d.SetShow(show: false);
			}

			public void MedgetsInitialize(Medget m)
			{
				if (m != null)
				{
					if (m.behavior() != null)
					{
						m.behavior().bmInitialize(m);
					}
					for (Medget medget = m.childNode(); medget != null; medget = medget.nextSibling())
					{
						MedgetsInitialize(medget);
					}
				}
			}

			public void MedgetsPostInitialize(Medget m)
			{
				if (m != null)
				{
					if (m.behavior() != null)
					{
						m.behavior().bmPostInitialize(m);
					}
					for (Medget medget = m.childNode(); medget != null; medget = medget.nextSibling())
					{
						MedgetsPostInitialize(medget);
					}
				}
			}

			public void MedgetsSuspend(Medget m)
			{
				if (m != null)
				{
					for (Medget medget = m.childNode(); medget != null; medget = medget.nextSibling())
					{
						MedgetsSuspend(medget);
					}
					if (m.behavior() != null)
					{
						m.behavior().bmSuspend(m);
					}
				}
			}

			public void MedgetsResume(Medget m)
			{
				if (m != null)
				{
					if (m.behavior() != null)
					{
						m.behavior().bmResume(m);
					}
					for (Medget medget = m.childNode(); medget != null; medget = medget.nextSibling())
					{
						MedgetsResume(medget);
					}
				}
			}

			public void MedgetsDefaultOwner(Medget m)
			{
				if (m != null)
				{
					if (m.behavior() != null)
					{
						m.behavior().ownerMedget = m;
					}
					for (Medget medget = m.childNode(); medget != null; medget = medget.nextSibling())
					{
						MedgetsDefaultOwner(medget);
					}
				}
			}

			public void MedgetsBehave(Medget m)
			{
				if (m == null)
				{
					return;
				}
				if (m.behavior() != null && m.behavior().mbActivity())
				{
					m.behavior().bmBehave(m);
				}
				if (terminateBehave_)
				{
					return;
				}
				for (Medget medget = m.childNode(); medget != null; medget = medget.nextSibling())
				{
					MedgetsBehave(medget);
					if (terminateBehave_)
					{
						break;
					}
				}
			}

			public void MedgetsTerminateBehave()
			{
				terminateBehave_ = true;
			}

			public void MedgetsFinalize(Medget m)
			{
				if (m != null)
				{
					for (Medget medget = m.childNode(); medget != null; medget = medget.nextSibling())
					{
						MedgetsFinalize(medget);
					}
					if (m.behavior() != null)
					{
						m.behavior().bmFinalize(m);
					}
				}
			}

			public void MedgetsRemove(Medget b, Medget m)
			{
				if (b == null || m == null)
				{
					return;
				}
				for (Medget medget = b.childNode(); medget != null; medget = medget.nextSibling())
				{
					if (medget == m)
					{
						if (medget.prevSibling() != null)
						{
							medget.prevSibling().setNextSibling(m.nextSibling());
						}
						if (medget.nextSibling() != null)
						{
							medget.nextSibling().setPrevSibling(m.prevSibling());
						}
						break;
					}
					MedgetsRemove(medget, m);
				}
			}

			public void MedgetsDelete(Medget m)
			{
				if (m != null)
				{
					Medget medget = m.childNode();
					while (medget != null)
					{
						Medget medget2 = medget.nextSibling();
						MedgetsDelete(medget);
						medget = medget2;
					}
					m.setChildNode(null);
					leaveFocusList(m);
					m.destruct();
				}
			}

			public bool MedgetsDecide(Medget m)
			{
				if (m.behavior() != null && m.behavior().bmDecide(m))
				{
					return true;
				}
				for (Medget medget = m.parentNode(); medget != null; medget = medget.parentNode())
				{
					if (medget.behavior() != null && medget.behavior().bmDecide(m))
					{
						return true;
					}
				}
				return false;
			}

			public bool MedgetsUseTap(Medget m)
			{
				if (m.behavior() != null && m.behavior().bmUseTap(m))
				{
					return true;
				}
				for (Medget medget = m.parentNode(); medget != null; medget = medget.parentNode())
				{
					if (medget.behavior() != null && medget.behavior().bmUseTap(m))
					{
						return true;
					}
				}
				return false;
			}

			public bool MedgetsCancel(Medget m)
			{
				if (m.behavior() != null && m.behavior().bmCancel(m))
				{
					return true;
				}
				for (Medget medget = m.parentNode(); medget != null; medget = medget.parentNode())
				{
					if (medget.behavior() != null && medget.behavior().bmCancel(m))
					{
						return true;
					}
				}
				return false;
			}

			public bool MedgetsDeactivate(Medget m)
			{
				if (m.behavior() != null && m.behavior().mbActivity())
				{
					m.behavior().bmDeactivate(m);
					return true;
				}
				for (Medget medget = m.parentNode(); medget != null; medget = medget.parentNode())
				{
					if (medget.behavior() != null && medget.behavior().mbActivity())
					{
						medget.behavior().bmDeactivate(m);
						return true;
					}
				}
				return false;
			}

			public bool MedgetsActivate(Medget m)
			{
				if (m.behavior() != null && m.behavior().mbActivity())
				{
					m.behavior().bmActivate(m);
					return true;
				}
				for (Medget medget = m.parentNode(); medget != null; medget = medget.parentNode())
				{
					if (medget.behavior() != null && medget.behavior().mbActivity())
					{
						medget.behavior().bmActivate(m);
						return true;
					}
				}
				return false;
			}

			public bool MedgetsDirection(Medget m, int dir)
			{
				if (m.behavior() != null && m.behavior().bmDirection(m, dir))
				{
					return true;
				}
				for (Medget medget = m.parentNode(); medget != null; medget = medget.parentNode())
				{
					if (medget.behavior() != null && medget.behavior().bmDirection(m, dir))
					{
						return true;
					}
				}
				return false;
			}

			public void MedgetsFocus(Medget m, int group)
			{
				if (m == null)
				{
					return;
				}
				if (m.node() != null)
				{
					XbnNode firstNodeByTagNameFromChildren = m.node().getFirstNodeByTagNameFromChildren(TRANSCODE("focus"));
					if (firstNodeByTagNameFromChildren != null)
					{
						XbnNode firstNodeByTagNameFromChildren2 = firstNodeByTagNameFromChildren.getFirstNodeByTagNameFromChildren(TRANSCODE("parameter"));
						if ((firstNodeByTagNameFromChildren2 != null && firstNodeByTagNameFromChildren2.nodeValueInt() == group) || (firstNodeByTagNameFromChildren2 == null && group == 0))
						{
							getSingleton().joinFocusList(m);
						}
					}
				}
				for (Medget medget = m.childNode(); medget != null; medget = medget.nextSibling())
				{
					MedgetsFocus(medget, group);
				}
			}

			public void changeFocusGroup(int group)
			{
				focusMedgets.clear();
				MedgetsFocus(baseMedget, group);
				initFocus(0);
			}

			public void setFocuseMedget(Medget m)
			{
				focusedMedget = m;
				for (int num = focusMedgets.size() - 1; num >= 0; num--)
				{
					if (focusMedgets[num] == m)
					{
						focusedCursor = num;
						break;
					}
				}
				initFocus(focusedCursor);
			}

			public void setFocuseMedget(int cursor)
			{
				if (cursor < 0)
				{
					focusedCursor = 0;
				}
				else if (cursor >= focusMedgets.size())
				{
					focusedCursor = focusMedgets.size() - 1;
				}
				else
				{
					focusedCursor = cursor;
				}
				initFocus(focusedCursor);
			}

			public void joinFocusList(Medget m)
			{
				for (int num = focusMedgets.size() - 1; num >= 0; num--)
				{
					if (focusMedgets[num] == m)
					{
						return;
					}
				}
				focusMedgets.push_back(m);
			}

			public void leaveFocusList(Medget m)
			{
				for (int num = focusMedgets.size() - 1; num >= 0; num--)
				{
					if (focusMedgets[num] == m)
					{
						if (focusedCursor == num)
						{
							if (focusMedgets[focusedCursor].behavior() != null)
							{
								focusMedgets[focusedCursor].behavior().bmDeactivate(focusMedgets[focusedCursor]);
							}
							focusedCursor = 0;
						}
						focusMedgets.erase(num);
						if (focusedCursor >= focusMedgets.size())
						{
							focusedCursor = focusMedgets.size() - 1;
						}
						break;
					}
				}
			}

			public void clearFocusList()
			{
				focusMedgets.clear();
			}

			public void initFocus(int _focusedCursor)
			{
				// PORT: --trace-menu logs every focus move with its caller, for following a layout's focus ring.
				if (OpenFF.Client.Options.Get("trace-menu") != null) OpenFF.Client.Log.Write(OpenFF.Client.LogChannel.File, "menu: initFocus " + _focusedCursor + " (was " + focusedCursor + ", " + focusMedgets.size() + " in the list) from " + new System.Diagnostics.StackTrace().GetFrame(1)?.GetMethod()?.Name + "<" + new System.Diagnostics.StackTrace().GetFrame(2)?.GetMethod()?.Name);
				if (focusedMedget != null)
				{
					if (focusedMedget.behavior() != null)
					{
						focusedMedget.behavior().mbTPRelease(focusedMedget);
					}
					MedgetsDeactivate(focusedMedget);
				}
				if (!focusMedgets.empty())
				{
					focusedCursor = 0;
					for (int i = 0; i < focusMedgets.size(); i++)
					{
						if (focusMedgets[i].myTag() == _focusedCursor)
						{
							focusedCursor = i;
							break;
						}
					}
					focusedMedget = focusMedgets[focusedCursor];
					OpenFF.Client.SteamLayout.Trace(focusedMedget, cellCursor2d);
					if (mode2d3d == 2)
					{
						cellCursor2d.SetPositionI(focusedMedget.cursorX(), focusedMedget.cursorY());
					}
					else if (mode2d3d == 3)
					{
						cellCursor3d.SetPositionI(focusedMedget.cursorX(), focusedMedget.cursorY());
					}
					MedgetsActivate(focusMedgets[focusedCursor]);
				}
				else
				{
					focusedMedget = null;
					if (mode2d3d == 2)
					{
						cellCursor2d.SetPositionI(320, 240);
					}
					else if (mode2d3d == 3)
					{
						cellCursor3d.SetPositionI(320, 240);
					}
				}
			}

			public Medget initFocusM(Medget target)
			{
				if (focusMedgets.empty() || target == null)
				{
					return null;
				}
				int num = focusedCursor;
				for (int num2 = focusMedgets.size() - 1; num2 >= 0; num2--)
				{
					if (focusMedgets[num2] == target)
					{
						focusedCursor = num2;
						break;
					}
				}
				if (num != focusedCursor)
				{
					if (focusedMedget != null)
					{
						if (focusedMedget.behavior() != null)
						{
							focusedMedget.behavior().mbTPRelease(focusedMedget);
						}
						MedgetsDeactivate(focusedMedget);
					}
					if (focusMedgets[num].behavior() != null)
					{
						focusMedgets[num].behavior().bmDeactivate(focusMedgets[num]);
					}
					focusedMedget = focusMedgets[focusedCursor];
					if (focusedMedget.behavior() != null)
					{
						focusedMedget.behavior().bmActivate(focusedMedget);
					}
					OpenFF.Client.SteamLayout.Trace(focusedMedget, cellCursor2d);
					if (mode2d3d == 2)
					{
						cellCursor2d.SetPositionI(focusedMedget.cursorX(), focusedMedget.cursorY());
					}
					else if (mode2d3d == 3)
					{
						cellCursor3d.SetPositionI(focusedMedget.cursorX(), focusedMedget.cursorY());
					}
					return focusMedgets[num];
				}
				return null;
			}

			public Medget makeup(Medget parent, XbnNode node)
			{
				Medget medget = new Medget();
				if (medget != null)
				{
					medget.parentNode_ = parent;
					medget.thisNode_ = node;
				}
				if (parent != null)
				{
					if (parent.childNode_ == null)
					{
						parent.childNode_ = medget;
					}
					else
					{
						Medget medget2 = parent.childNode_;
						if (medget2 != null)
						{
							while (medget2.nextSibling_ != null)
							{
								medget2 = medget2.nextSibling_;
							}
							medget2.nextSibling_ = medget;
							medget.prevSibling_ = medget2;
						}
					}
				}
				XbnNode firstNodeByTagNameFromChildren;
				if ((firstNodeByTagNameFromChildren = node.getFirstNodeByTagNameFromChildren(TRANSCODE("id"))) != null)
				{
					medget.id_ = firstNodeByTagNameFromChildren.nodeValueString();
				}
				if ((firstNodeByTagNameFromChildren = node.getFirstNodeByTagNameFromChildren(TRANSCODE("x"))) != null)
				{
					medget.x_ = (short)firstNodeByTagNameFromChildren.nodeValueInt();
				}
				if ((firstNodeByTagNameFromChildren = node.getFirstNodeByTagNameFromChildren(TRANSCODE("y"))) != null)
				{
					medget.y_ = (short)firstNodeByTagNameFromChildren.nodeValueInt();
				}
				if ((firstNodeByTagNameFromChildren = node.getFirstNodeByTagNameFromChildren(TRANSCODE("width"))) != null)
				{
					medget.width_ = (short)firstNodeByTagNameFromChildren.nodeValueInt();
				}
				if ((firstNodeByTagNameFromChildren = node.getFirstNodeByTagNameFromChildren(TRANSCODE("height"))) != null)
				{
					medget.height_ = (short)firstNodeByTagNameFromChildren.nodeValueInt();
				}
				if ((firstNodeByTagNameFromChildren = node.getFirstNodeByTagNameFromChildren(TRANSCODE("display"))) != null)
				{
					medget.display_ = (sbyte)firstNodeByTagNameFromChildren.nodeValueInt();
				}
				if ((firstNodeByTagNameFromChildren = node.getFirstNodeByTagNameFromChildren(TRANSCODE("work"))) != null)
				{
					medget.space.work_ = (sbyte)firstNodeByTagNameFromChildren.nodeValueInt();
				}
				if ((firstNodeByTagNameFromChildren = node.getFirstNodeByTagNameFromChildren(TRANSCODE("work1"))) != null)
				{
					medget.space.work1_ = (sbyte)firstNodeByTagNameFromChildren.nodeValueInt();
				}
				if ((firstNodeByTagNameFromChildren = node.getFirstNodeByTagNameFromChildren(TRANSCODE("work2"))) != null)
				{
					medget.space.work2_ = (sbyte)firstNodeByTagNameFromChildren.nodeValueInt();
				}
				if ((firstNodeByTagNameFromChildren = node.getFirstNodeByTagNameFromChildren(TRANSCODE("myTag"))) != null)
				{
					medget.myTag_ = (sbyte)firstNodeByTagNameFromChildren.nodeValueInt();
				}
				if ((firstNodeByTagNameFromChildren = node.getFirstNodeByTagNameFromChildren(TRANSCODE("up"))) != null)
				{
					medget.up_ = firstNodeByTagNameFromChildren.nodeValueString();
				}
				if ((firstNodeByTagNameFromChildren = node.getFirstNodeByTagNameFromChildren(TRANSCODE("down"))) != null)
				{
					medget.down_ = firstNodeByTagNameFromChildren.nodeValueString();
				}
				if ((firstNodeByTagNameFromChildren = node.getFirstNodeByTagNameFromChildren(TRANSCODE("left"))) != null)
				{
					medget.left_ = firstNodeByTagNameFromChildren.nodeValueString();
				}
				if ((firstNodeByTagNameFromChildren = node.getFirstNodeByTagNameFromChildren(TRANSCODE("right"))) != null)
				{
					medget.right_ = firstNodeByTagNameFromChildren.nodeValueString();
				}
				if ((firstNodeByTagNameFromChildren = node.getFirstNodeByTagNameFromChildren(TRANSCODE("focus"))) != null)
				{
					XbnNode firstNodeByTagNameFromChildren2 = firstNodeByTagNameFromChildren.getFirstNodeByTagNameFromChildren(TRANSCODE("parameter"));
					if (firstNodeByTagNameFromChildren2 == null || firstNodeByTagNameFromChildren2.nodeValueInt() == 0)
					{
						joinFocusList(medget);
					}
				}
				if (parent != null)
				{
					medget.x_ += parent.x();
					medget.y_ += parent.y();
				}
				if ((firstNodeByTagNameFromChildren = node.getFirstNodeByTagNameFromChildren(TRANSCODE("behavior"))) != null)
				{
					medget.behavior_ = MenuBehaviorFactory.createMenuBehavior(firstNodeByTagNameFromChildren.nodeValueString());
					if (medget.behavior_ != null)
					{
						medget.behavior_.ownerMedget = medget;
					}
				}
				if ((firstNodeByTagNameFromChildren = node.getFirstNodeByTagNameFromChildren(TRANSCODE("table"))) != null)
				{
					int num = 1;
					int num2 = 1;
					sbyte b = 0;
					XbnNode firstNodeByTagNameFromChildren3;
					if ((firstNodeByTagNameFromChildren3 = firstNodeByTagNameFromChildren.getFirstNodeByTagNameFromChildren(TRANSCODE("width"))) != null)
					{
						num = firstNodeByTagNameFromChildren3.nodeValueInt();
					}
					if ((firstNodeByTagNameFromChildren3 = firstNodeByTagNameFromChildren.getFirstNodeByTagNameFromChildren(TRANSCODE("height"))) != null)
					{
						num2 = firstNodeByTagNameFromChildren3.nodeValueInt();
					}
					if ((firstNodeByTagNameFromChildren3 = firstNodeByTagNameFromChildren.getFirstNodeByTagNameFromChildren(TRANSCODE("height"))) != null)
					{
						num2 = firstNodeByTagNameFromChildren3.nodeValueInt();
					}
					if ((firstNodeByTagNameFromChildren3 = node.getFirstNodeByTagNameFromChildren(TRANSCODE("myTag"))) != null)
					{
						b = (sbyte)firstNodeByTagNameFromChildren3.nodeValueInt();
					}
					Medget medget3 = null;
					short num3 = medget.x_;
					short num4 = medget.y_;
					short num5 = (short)(medget.width_ / num);
					short num6 = (short)(medget.height_ / num2);
					Medget medget4 = null;
					Medget medget5 = null;
					Medget medget6 = null;
					Medget medget7 = null;
					for (int i = 0; i < num2; i++)
					{
						for (int j = 0; j < num; j++)
						{
							Medget medget8 = new Medget();
							if (medget4 == null)
							{
								medget4 = medget8;
							}
							if (medget5 == null)
							{
								medget5 = medget8;
							}
							medget8.setLeft(null);
							medget8.setRight(null);
							medget8.setUp(null);
							medget8.setDown(null);
							medget8.parentNode_ = medget;
							medget8.thisNode_ = firstNodeByTagNameFromChildren;
							medget8.x_ = num3;
							medget8.y_ = num4;
							medget8.width_ = num5;
							medget8.height_ = num6;
							medget8.myTag_ = (sbyte)(num * i + j + b);
							if (medget6 != null)
							{
								_ = 0;
							}
							medget6 = medget8;
							if (medget3 == null)
							{
								medget3 = medget8;
								if (medget.childNode_ == null)
								{
									medget.childNode_ = medget3;
								}
								else
								{
									Medget medget9 = medget.childNode_;
									while (medget9.nextSibling_ != null)
									{
										medget9 = medget9.nextSibling_;
									}
									medget9.nextSibling_ = medget3;
								}
							}
							else
							{
								medget3.nextSibling_ = medget8;
								medget3 = medget8;
							}
							num3 += num5;
						}
						num3 = medget.x_;
						num4 += num6;
						medget7 = medget5;
						medget5 = null;
					}
					medget5 = medget4;
					while (medget5.down() != null)
					{
					}
					for (int k = 0; k < num; k++)
					{
					}
				}
				bool flag = true;
				Medget medget10 = null;
				XbnNodeList xbnNodeList = new XbnNodeList();
				do
				{
					flag = node.getNodesByTagNameFromChildren(TRANSCODE("frame"), xbnNodeList);
					for (int l = 0; l < xbnNodeList.size(); l++)
					{
						Medget medget11 = makeup(medget, xbnNodeList[l]);
						if (medget10 != null)
						{
							medget10.nextSibling_ = medget11;
						}
						medget10 = medget11;
					}
				}
				while (flag);
				return medget;
			}

			public void Push(string menu_name)
			{
				if (baseMedget != null)
				{
					ds.Vector<Medget, ds.FastErasePolicy<Medget>> vector = new ds.Vector<Medget, ds.FastErasePolicy<Medget>>(96);
					vector.copy(focusMedgets);
					stkMenuLevel.push(new MENU_BACKUP(baseMedget, vector, focusedCursor));
					focusMedgets.clear();
					MedgetsSuspend(baseMedget);
					buildMenu(menu_name);
				}
			}

			public void Pop()
			{
				if (stkMenuLevel.empty() == 0)
				{
					MedgetsFinalize(baseMedget);
					MedgetsDelete(baseMedget);
					MENU_BACKUP mENU_BACKUP = new MENU_BACKUP(stkMenuLevel.top());
					stkMenuLevel.pop();
					baseMedget = mENU_BACKUP.@base;
					focusMedgets.copy(mENU_BACKUP.focusBackup);
					focusedCursor = mENU_BACKUP.cursorBackup;
					MedgetsDefaultOwner(baseMedget);
					MedgetsResume(baseMedget);
					if (focusMedgets.size() > focusedCursor)
					{
						focusedMedget = focusMedgets[focusedCursor];
					}
					initFocus(focusedCursor);
					Medget.freePool();
					ds.CHeap.free_app(mENU_BACKUP.focusBackup);
				}
			}

			public void ClearBehaviorButton()
			{
				decideButton = 1;
				cancelButton = 1;
			}

			public int buildWindow(string menu_name, string wnd_name)
			{
				XbnNode xbnNode = xbnDocument.root();
				if (xbnNode == null)
				{
					return -1;
				}
				XbnNodeList xbnNodeList = new XbnNodeList();
				xbnNode.getNodesByTagName(TRANSCODE("menu"), xbnNodeList);
				XbnNode xbnNode2 = null;
				for (int num = xbnNodeList.size() - 1; num >= 0; num--)
				{
					XbnNode firstNodeByTagName = xbnNodeList[num].getFirstNodeByTagName(TRANSCODE("name"));
					if (firstNodeByTagName != null && strcmp(firstNodeByTagName.nodeValueString(), menu_name) == 0)
					{
						xbnNode2 = xbnNodeList[num];
						break;
					}
				}
				if (xbnNode2 == null)
				{
					return -1;
				}
				xbnNodeList.clear();
				xbnNode2.getNodesByTagName(TRANSCODE("frame"), xbnNodeList);
				for (int num2 = xbnNodeList.size() - 1; num2 >= 0; num2--)
				{
					ds.Vector2<short> vector = new ds.Vector2<short>();
					ds.Vector2<short> vector2 = new ds.Vector2<short>();
					XbnNode firstNodeByTagNameFromChildren = xbnNodeList[num2].getFirstNodeByTagNameFromChildren(TRANSCODE("window"));
					if (firstNodeByTagNameFromChildren != null)
					{
						firstNodeByTagNameFromChildren = xbnNodeList[num2].getFirstNodeByTagNameFromChildren(TRANSCODE("id"));
						if (strcmp(firstNodeByTagNameFromChildren.nodeValueString(), wnd_name) == 0 && firstNodeByTagNameFromChildren != null)
						{
							firstNodeByTagNameFromChildren = xbnNodeList[num2].getFirstNodeByTagNameFromChildren(TRANSCODE("x"));
							if (firstNodeByTagNameFromChildren != null)
							{
								vector.vx = (short)firstNodeByTagNameFromChildren.nodeValueInt();
								firstNodeByTagNameFromChildren = xbnNodeList[num2].getFirstNodeByTagNameFromChildren(TRANSCODE("y"));
								if (firstNodeByTagNameFromChildren != null)
								{
									vector.vy = (short)firstNodeByTagNameFromChildren.nodeValueInt();
									firstNodeByTagNameFromChildren = xbnNodeList[num2].getFirstNodeByTagNameFromChildren(TRANSCODE("width"));
									if (firstNodeByTagNameFromChildren != null)
									{
										vector2.vx = (short)firstNodeByTagNameFromChildren.nodeValueInt();
										firstNodeByTagNameFromChildren = xbnNodeList[num2].getFirstNodeByTagNameFromChildren(TRANSCODE("height"));
										if (firstNodeByTagNameFromChildren != null)
										{
											vector2.vy = (short)firstNodeByTagNameFromChildren.nodeValueInt();
											firstNodeByTagNameFromChildren = xbnNodeList[num2].getFirstNodeByTagNameFromChildren(TRANSCODE("work"));
											if (firstNodeByTagNameFromChildren != null)
											{
												int flameCount = firstNodeByTagNameFromChildren.nodeValueInt();
												for (int i = 0; i < 2; i++)
												{
													if (GetMenuWindowObj()[i].GetEnable() == -1)
													{
														GetMenuWindowObj()[i].SetMaxWindowPos(vector);
														GetMenuWindowObj()[i].SetMaxWindowSize(vector2);
														GetMenuWindowObj()[i].ClearNowWindowSize();
														GetMenuWindowObj()[i].CalcOneRatio(flameCount);
														vector2.vx = (vector2.vy = 0);
														if (mode2d3d == 2)
														{
															GetMenuWindowObj()[i].GetWindowHandle().bwCreateUL(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_SUB2D, vector, vector2, 3);
														}
														else if (mode2d3d == 3)
														{
															GetMenuWindowObj()[i].GetWindowHandle().bwCreateUL(sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN3D, vector, vector2, 3);
														}
														GetMenuWindowObj()[i].GetWindowHandle().SetPriority(3);
														GetMenuWindowObj()[i].GetWindowHandle().SetShow(show: true, user: true);
														GetMenuWindowObj()[i].SetEnable(i);
														return i;
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
				return -1;
			}

			public bool buildMenu(string menu_name)
			{
				focusMedgets.clear();
				focusedMedget = null;
				XbnNode xbnNode = xbnDocument.root();
				if (xbnNode == null)
				{
					return false;
				}
				XbnNodeList xbnNodeList = new XbnNodeList();
				bool flag = true;
				XbnNode xbnNode2 = null;
				do
				{
					flag = xbnNode.getNodesByTagNameFromChildren(TRANSCODE("menu"), xbnNodeList);
					for (int num = xbnNodeList.size() - 1; num >= 0; num--)
					{
						XbnNode firstNodeByTagName = xbnNodeList[num].getFirstNodeByTagName(TRANSCODE("name"));
						if (firstNodeByTagName != null && strcmp(firstNodeByTagName.nodeValueString(), menu_name) == 0)
						{
							xbnNode2 = xbnNodeList[num];
							flag = false;
							break;
						}
					}
				}
				while (flag);
				if (xbnNode2 == null)
				{
					return false;
				}
				int num2 = xbnNode2.countNodesByTagName(TRANSCODE("frame"));
				OS_Printf("countNodesByTagName = %d\n", num2);
				Medget.allocatePool(num2 + 4);
				xbnNodeList.clear();
				xbnNode2.getNodesByTagNameFromChildren(TRANSCODE("frame"), xbnNodeList);
				baseMedget = new Medget();
				baseMedget.clear();
				for (int i = 0; i < xbnNodeList.size(); i++)
				{
					makeup(baseMedget, xbnNodeList[i]);
				}
				MedgetsInitialize(baseMedget);
				MedgetsPostInitialize(baseMedget);
				if (trialInitFocuse)
				{
					initFocus(0);
				}
				else
				{
					trialInitFocuse = true;
				}
				touchedFlag = true;
				prevTouchedFlag_ = true;
				// PORT: the mods' behaviours on this screen, if any reach it (OpenFF.Client.ModMenus).
				OpenFF.Client.ModMenus.GameScreenBuilt(menu_name);
				return true;
			}

			public void buildMenuSubstant()
			{
				if (baseMedget != null)
				{
					MedgetsInitialize(baseMedget);
					MedgetsPostInitialize(baseMedget);
					initFocus(0);
				}
				touchedFlag = true;
				prevTouchedFlag_ = true;
			}

			public Medget Append(string menu_name)
			{
				XbnNode xbnNode = xbnDocument.root();
				if (xbnNode == null)
				{
					return null;
				}
				XbnNodeList xbnNodeList = new XbnNodeList();
				xbnNode.getNodesByTagName(TRANSCODE("menu"), xbnNodeList);
				XbnNode xbnNode2 = null;
				for (int num = xbnNodeList.size() - 1; num >= 0; num--)
				{
					XbnNode firstNodeByTagName = xbnNodeList[num].getFirstNodeByTagName(TRANSCODE("name"));
					if (firstNodeByTagName != null && strcmp(firstNodeByTagName.nodeValueString(), menu_name) == 0)
					{
						xbnNode2 = xbnNodeList[num];
						break;
					}
				}
				if (xbnNode2 == null)
				{
					return null;
				}
				int num2 = xbnNode2.countNodesByTagName(TRANSCODE("frame"));
				OS_Printf("countNodesByTagName = %d\n", num2);
				Medget.allocatePool(num2 + 4);
				xbnNodeList.clear();
				xbnNode2.getNodesByTagNameFromChildren(TRANSCODE("frame"), xbnNodeList);
				Medget medget = null;
				for (int i = 0; i < xbnNodeList.size(); i++)
				{
					Medget medget2 = makeup(baseMedget, xbnNodeList[i]);
					if (medget == null)
					{
						medget = medget2;
					}
				}
				if (medget == null)
				{
					return null;
				}
				MedgetsInitialize(medget);
				MedgetsPostInitialize(medget);
				appendedMenuList.push_back(medget);
				return medget;
			}

			public void Remove(Medget branch)
			{
				if (appendedMenuList.empty())
				{
					return;
				}
				if (branch == null)
				{
					branch = appendedMenuList[appendedMenuList.size() - 1];
					appendedMenuList.erase(appendedMenuList.size() - 1);
				}
				else
				{
					for (int num = appendedMenuList.size() - 1; num >= 0; num--)
					{
						if (appendedMenuList[num] == branch)
						{
							appendedMenuList.erase(num);
						}
					}
				}
				MedgetsRemove(baseMedget, branch);
				MedgetsFinalize(branch);
				MedgetsDelete(branch);
				Medget.freePool();
			}

			public void release()
			{
				// PORT: the mods' behaviours on the screen going away hear of it.
				OpenFF.Client.ModMenus.GameScreenReleased();
				while (stkMenuLevel.empty() == 0)
				{
					Pop();
				}
				appendedMenuList.clear();
				focusMedgets.clear();
				focusedMedget = null;
				MedgetsFinalize(baseMedget);
				while (dgs.DGSLinkedList<Medget>.dgsllBase() != null)
				{
					dgs.DGSLinkedList<Medget>.dgsllBase().destruct();
				}
				baseMedget = null;
			}

			public void releaseWindow(int no)
			{
				if (no >= 0 && GetMenuWindowObj()[no].GetEnable() != -1)
				{
					GetMenuWindowObj()[no].GetWindowHandle().GetPositionCC();
					GetMenuWindowObj()[no].GetMaxWindowSize();
					GetMenuWindowObj()[no].GetWindowHandle().Release();
					GetMenuWindowObj()[no].SetEnable(-1);
				}
			}

			public void releaseWindowAll()
			{
				for (int i = 0; i < 2; i++)
				{
					releaseWindow(i);
				}
			}

			public void releaseWindowExcludingOne()
			{
				for (int i = 1; i < 2; i++)
				{
					releaseWindow(i);
				}
			}

			public void releaseAll()
			{
				XbnFile xbnFile = xbnDocument.xbnFinalize();
				if (xbnFile != null)
				{
					ds.CHeap.free_app(xbnFile);
				}
			}

			public void execute()
			{
				prevTouchedFlag_ = touchedFlag;
				touchState_ = TOUCH_STATE.NOT_TOUCH;
				SetDirectionKeyState(0);
				if (getSingleton().GetScrollType() != SCROLL_TYPE.TYPE_WAIT)
				{
					getSingleton().SetScrollType(SCROLL_TYPE.TYPE_WAIT);
				}
				if (inputPermit && !focusMedgets.empty() && !ScrollBar.sbCheckTouchPanel())
				{
					if (MedgetsUseTap(focusMedgets[focusedCursor]) ? ds.g_TouchPanel.isTap() : ds.g_TouchPanel.isEdge())
					{
						touchState_ = TOUCH_STATE.NOT_TOUCH_ITEM;
						ds.g_TouchPanel.getPoint(out var x, out var y);
						for (int i = 0; i < focusMedgets.size(); i++)
						{
							if (focusMedgets[i].x() >= x || x > focusMedgets[i].x() + focusMedgets[i].width() || focusMedgets[i].y() >= y || y > focusMedgets[i].y() + focusMedgets[i].height())
							{
								continue;
							}
							if (i == focusedCursor)
							{
								if (focusMedgets[focusedCursor].behavior() != null && focusMedgets[focusedCursor].behavior().bmIsButton(focusMedgets[focusedCursor]))
								{
									focusMedgets[focusedCursor].behavior().mbTPPush(focusMedgets[focusedCursor]);
								}
								else
								{
									if (!MedgetsUseTap(focusMedgets[focusedCursor]))
									{
										TP_CancelTap();
									}
									MedgetsDecide(focusMedgets[focusedCursor]);
								}
							}
							else
							{
								if (!focusMedgets.empty() && MedgetsDeactivate(focusMedgets[focusedCursor]) && focusMedgets[focusedCursor].behavior() != null)
								{
									focusMedgets[focusedCursor].behavior().mbTPRelease(focusMedgets[focusedCursor]);
								}
								focusedCursor = i;
								if (focusMedgets[focusedCursor].behavior() != null)
								{
									focusMedgets[focusedCursor].behavior().mbTPPush(focusMedgets[focusedCursor]);
								}
								initFocus(focusMedgets[focusedCursor].myTag());
								if (MedgetsUseTap(focusMedgets[focusedCursor]))
								{
									TP_CancelTap();
								}
								if (focusMedgets[focusedCursor].node().nodeValueString() != null)
								{
									MedgetsDecide(focusMedgets[focusedCursor]);
								}
								else if (focusMedgets[focusedCursor].behavior() == null || !focusMedgets[focusedCursor].behavior().bmIsButton(focusMedgets[focusedCursor]))
								{
									playSEMoveCursor();
								}
							}
							touchState_ = TOUCH_STATE.TOUCH_ITEM;
							break;
						}
						touchedFlag = true;
					}
					else
					{
						if (touchedFlag && (ds.g_Pad.edge() != 0 || ds.g_TouchPanel.isRelease()))
						{
							if (focusMedgets[focusedCursor].behavior() != null)
							{
								focusMedgets[focusedCursor].behavior().mbTPRelease(focusMedgets[focusedCursor]);
							}
							touchedFlag = false;
						}
						if ((ds.g_Pad.edge() & 1) != 0)
						{
							if (!MedgetsDecide(focusMedgets[focusedCursor]))
							{
								playSEDecide();
							}
						}
						else if ((ds.g_Pad.edge() & 2) != 0)
						{
							if (!MedgetsCancel(focusMedgets[focusedCursor]))
							{
								playSECancel();
							}
						}
						else if ((ds.g_Pad.repeat() & 0xF0) != 0)
						{
							bool flag = false;
							SetDirectionKeyState(1);
							if (!MedgetsDirection(focusMedgets[focusedCursor], 0))
							{
								int num = focusedCursor;
								if ((ds.g_Pad.repeat() & 0x20) != 0 || (ds.g_Pad.repeat() & 0x40) != 0)
								{
									playSEMoveCursor();
									focusedCursor--;
									if (focusedCursor < 0)
									{
										focusedCursor = focusMedgets.size() - 1;
									}
								}
								else if ((ds.g_Pad.repeat() & 0x10) != 0 || (ds.g_Pad.repeat() & 0x80) != 0)
								{
									playSEMoveCursor();
									focusedCursor++;
									if (focusedCursor > focusMedgets.size() - 1)
									{
										focusedCursor = 0;
									}
								}
								if (num != focusedCursor)
								{
									if (focusMedgets[num].behavior() != null)
									{
										focusMedgets[num].behavior().bmDeactivate(focusMedgets[num]);
									}
									if (focusMedgets[focusedCursor].behavior() != null)
									{
										focusMedgets[focusedCursor].behavior().bmActivate(focusMedgets[focusedCursor]);
									}
									if (!focusMedgets.empty())
									{
										if (mode2d3d == 2)
										{
											cellCursor2d.SetPositionI(focusMedgets[focusedCursor].cursorX(), focusMedgets[focusedCursor].cursorY());
										}
										else if (mode2d3d == 3)
										{
											cellCursor3d.SetPositionI(focusMedgets[focusedCursor].cursorX(), focusMedgets[focusedCursor].cursorY());
										}
									}
									else if (mode2d3d == 2)
									{
										cellCursor2d.SetPositionI(320, 240);
									}
									else if (mode2d3d == 3)
									{
										cellCursor3d.SetPositionI(320, 240);
									}
									focusedMedget = focusMedgets[focusedCursor];
								}
							}
						}
					}
				}
				MedgetsBehave(baseMedget);
				terminateBehave_ = false;
				// PORT: the mods' behaviours on one of the game's screens hear the frame's focus, presses and keys.
				OpenFF.Client.ModMenus.GameScreenTick();
			}

			public bool MoveCursor(Medget M, bool PlaySe)
			{
				if (((ds.g_Pad.repeat() & 0x40) | 0x80 | 0x20 | 0x10) == 0)
				{
					return false;
				}
				Medget medget = focusedMedget;
				if ((ds.g_Pad.repeat() & 0x40) != 0)
				{
					string id = M.up();
					Medget nodeByID = M.parentNode().getNodeByID(id);
					if (nodeByID == null)
					{
						getSingleton().SetScrollType(SCROLL_TYPE.TYPE_UP);
					}
					else
					{
						getSingleton().SetScrollType(SCROLL_TYPE.TYPE_WAIT);
						getSingleton().initFocus(nodeByID.myTag());
					}
				}
				else if ((ds.g_Pad.repeat() & 0x80) != 0)
				{
					string id2 = M.down();
					Medget nodeByID2 = M.parentNode().getNodeByID(id2);
					if (nodeByID2 == null)
					{
						getSingleton().SetScrollType(SCROLL_TYPE.TYPE_DOWN);
					}
					else
					{
						getSingleton().SetScrollType(SCROLL_TYPE.TYPE_WAIT);
						getSingleton().initFocus(nodeByID2.myTag());
					}
				}
				else if ((ds.g_Pad.repeat() & 0x10) != 0)
				{
					string id3 = M.right();
					Medget nodeByID3 = M.parentNode().getNodeByID(id3);
					if (nodeByID3 == null)
					{
						getSingleton().SetScrollType(SCROLL_TYPE.TYPE_DOWN);
					}
					else
					{
						getSingleton().SetScrollType(SCROLL_TYPE.TYPE_WAIT);
						getSingleton().initFocus(nodeByID3.myTag());
					}
				}
				else if ((ds.g_Pad.repeat() & 0x20) != 0)
				{
					string id4 = M.left();
					Medget nodeByID4 = M.parentNode().getNodeByID(id4);
					if (nodeByID4 == null)
					{
						getSingleton().SetScrollType(SCROLL_TYPE.TYPE_UP);
					}
					else
					{
						getSingleton().SetScrollType(SCROLL_TYPE.TYPE_WAIT);
						getSingleton().initFocus(nodeByID4.myTag());
					}
				}
				if (PlaySe && medget != focusedMedget)
				{
					playSEMoveCursor();
				}
				return true;
			}

			public bool UpdateWindowState(int no, MenuWindow.MENU_WINDOW_MOVE_TYPE type)
			{
				bool flag = true;
				return GetMenuWindowObj()[no].SizeMoving(type);
			}

			public bool TouchWindowOutArea(int sx, int sy)
			{
				Medget medget = baseMedget;
				if (baseMedget == null)
				{
					return true;
				}
				medget = medget.getNodeByID(TRANSCODE("m_main"));
				int num;
				int num2;
				if (medget == null)
				{
					num = 136;
					num2 = 256;
				}
				else
				{
					medget.x();
					num = medget.y();
					num2 = medget.width();
					medget.height();
				}
				int num3 = 0;
				Medget medget2 = null;
				medget2 = baseMedget.getNodeByID(TRANSCODE("magic_list"));
				if (medget2 != null)
				{
					num3 = 1;
				}
				if (num3 == 0)
				{
					medget2 = null;
					medget2 = baseMedget.getNodeByID(TRANSCODE("equip_list"));
					if (medget2 != null)
					{
						num3 = 1;
					}
				}
				if (num3 == 0)
				{
					medget2 = null;
					medget2 = baseMedget.getNodeByID(TRANSCODE("item_list"));
					if (medget2 != null)
					{
						num3 = 1;
					}
				}
				if (num3 == 1)
				{
					num2 += 16;
				}
				if (sy > num)
				{
					return false;
				}
				return true;
			}

			public void playSEDecide()
			{
				if (!notSEFlag())
				{
					MatrixSound.MtxSENDS_Play(0, 1, 192, 127);
				}
			}

			public void playSECancel()
			{
				MatrixSound.MtxSENDS_Play(0, 2, 192, 127);
			}

			public void playSEBeep()
			{
				MatrixSound.MtxSENDS_Play(0, 0, 192, 127);
			}

			public void playSEMoveCursor()
			{
				MatrixSound.MtxSENDS_Play(0, 3, 192, 127);
			}

			public void SetDirectionKeyState(int val)
			{
				directionKey = val;
			}

			public int GetDirectionKeyState()
			{
				return directionKey;
			}

			public static MenuManager getSingleton()
			{
				return instance_;
			}

			public Medget root()
			{
				return baseMedget;
			}

			public byte getPhase()
			{
				return (byte)stkMenuLevel.size();
			}

			public ds.Vector<Medget, ds.FastErasePolicy<Medget>> getFocusList()
			{
				return focusMedgets;
			}

			public void inputPermission(bool b)
			{
				inputPermit = b;
			}

			public void Set2d3dMode(int val)
			{
				mode2d3d = val;
			}

			public int Get2d3dMode()
			{
				return mode2d3d;
			}

			public void SetScrollType(SCROLL_TYPE val)
			{
				scrollType = val;
			}

			public SCROLL_TYPE GetScrollType()
			{
				return scrollType;
			}

			public void SetItemDataTextNo(int val)
			{
				itemDataTextNo = val;
			}

			public int GetItemDataTextNo()
			{
				return itemDataTextNo;
			}

			public void SetMenuDataTextNo(int val)
			{
				menuDataTextNo = val;
			}

			public int GetMenuDataTextNo()
			{
				return menuDataTextNo;
			}

			public void SetSpecialDataTextNo(int val)
			{
				specialDataTextNo = val;
			}

			public int GetSpecialDataTextNo()
			{
				return specialDataTextNo;
			}

			public void SetMognetDataTextNo(int val)
			{
				mognetDataTextNo = val;
			}

			public int GetMognetDataTextNo()
			{
				return mognetDataTextNo;
			}

			public void ClearTargetItemNo()
			{
				targetItemNo ^= targetItemNo;
			}

			public void SetTargetItemNo(int val)
			{
				targetItemNo = val;
			}

			public int GetTargetItemNo()
			{
				return targetItemNo;
			}

			public int GetTargetItemLine()
			{
				return targetLineNo;
			}

			public void SetTargetItemLine(int val)
			{
				targetLineNo = val;
			}

			public int GetTargetItemTemp()
			{
				return targetItemTemp;
			}

			public void SetTargetItemTemp(int val)
			{
				targetItemTemp = val;
			}

			public void SetTargetCharNo(int val)
			{
				targetCharNo = val;
			}

			public int GetTargetCharNo()
			{
				return targetCharNo;
			}

			public void SetUsingMenuType(int val)
			{
				menu_type = val;
			}

			public int GetUsingMenuType()
			{
				return menu_type;
			}

			public void SetMagicMenuType(int val)
			{
				magicMenu_type = val;
			}

			public int GetMagicMenuType()
			{
				return magicMenu_type;
			}

			public void SetDecideButtonState(int val)
			{
				decideButton = val;
			}

			public int GetDecideButtonState()
			{
				return decideButton;
			}

			public void SetActivateButtonState(int val)
			{
				activateButton = val;
			}

			public int GetActivateButtonState()
			{
				return activateButton;
			}

			public void SetCancelButtonState(int val)
			{
				cancelButton = val;
			}

			public int GetCancelButtonState()
			{
				return cancelButton;
			}

			public MenuWindow[] GetMenuWindowObj()
			{
				return windowObj;
			}

			public void ClearAllWindowEnable()
			{
				for (int i = 0; i < 2; i++)
				{
					GetMenuWindowObj()[i].SetEnable(-1);
					GetMenuWindowObj()[i].SetScrollEnable(val: true);
				}
			}

			public TOUCH_STATE checkTouchState()
			{
				return touchState_;
			}

			public bool updateMessageFlag()
			{
				return updateMessageFlag_;
			}

			public void setUpdateMessageFlag(bool flag)
			{
				updateMessageFlag_ = flag;
			}

			public bool pitchFlag()
			{
				return pitchFlag_;
			}

			public void setPitchFlag(bool flag)
			{
				pitchFlag_ = flag;
			}

			public bool openDoorFlag()
			{
				return openDoorFlag_;
			}

			public void setOpenDoorFlag(bool flag)
			{
				openDoorFlag_ = flag;
			}

			public int saveBattlePitchLine(int _id)
			{
				return saveCursor_[_id].saveBattlePitchLine_;
			}

			public int saveBattlePitchTarget(int _id)
			{
				return saveCursor_[_id].saveBattlePitchTarget_;
			}

			public int saveBattleEquipLine(int _id)
			{
				return saveCursor_[_id].saveBattleEquipLine_;
			}

			public int saveBattleEquipTarget(int _id)
			{
				return saveCursor_[_id].saveBattleEquipTarget_;
			}

			public int saveBattleEquipHand(int _id)
			{
				return saveCursor_[_id].saveBattleEquipHand_;
			}

			public int saveBattleItemLine(int _id)
			{
				return saveCursor_[_id].saveBattleItemLine_;
			}

			public int saveBattleItemTarget(int _id)
			{
				return saveCursor_[_id].saveBattleItemTarget_;
			}

			public int saveBattleMagicLine(int _id)
			{
				return saveCursor_[_id].saveBattleMagicLine_;
			}

			public int saveBattleMagicTarget(int _id)
			{
				return saveCursor_[_id].saveBattleMagicTarget_;
			}

			public int saveSongTarget(int _id)
			{
				return saveCursor_[_id].saveSongTarget_;
			}

			public int saveBattleUseItem(int _id)
			{
				return saveCursor_[_id].saveBattleUseItem_;
			}

			public int saveBattleUseItemHand(int _id)
			{
				return saveCursor_[_id].saveBattleUseItemHand_;
			}

			public void saveBattlePitchLine_set(int _id, int arg0)
			{
				saveCursor_[_id].saveBattlePitchLine_ = arg0;
			}

			public void saveBattlePitchTarget_set(int _id, int arg0)
			{
				saveCursor_[_id].saveBattlePitchTarget_ = arg0;
			}

			public void saveBattleEquipLine_set(int _id, int arg0)
			{
				saveCursor_[_id].saveBattleEquipLine_ = arg0;
			}

			public void saveBattleEquipTarget_set(int _id, int arg0)
			{
				saveCursor_[_id].saveBattleEquipTarget_ = arg0;
			}

			public void saveBattleEquipHand_set(int _id, int arg0)
			{
				saveCursor_[_id].saveBattleEquipHand_ = arg0;
			}

			public void saveBattleItemLine_set(int _id, int arg0)
			{
				saveCursor_[_id].saveBattleItemLine_ = arg0;
			}

			public void saveBattleItemTarget_set(int _id, int arg0)
			{
				saveCursor_[_id].saveBattleItemTarget_ = arg0;
			}

			public void saveBattleMagicLine_set(int _id, int arg0)
			{
				saveCursor_[_id].saveBattleMagicLine_ = arg0;
			}

			public void saveBattleMagicTarget_set(int _id, int arg0)
			{
				saveCursor_[_id].saveBattleMagicTarget_ = arg0;
			}

			public void saveSongTarget_set(int _id, int arg0)
			{
				saveCursor_[_id].saveSongTarget_ = arg0;
			}

			public void saveBattleUseItem_set(int _id, int arg0)
			{
				saveCursor_[_id].saveBattleUseItem_ = arg0;
			}

			public void saveBattleUseItemHand_set(int _id, int arg0)
			{
				saveCursor_[_id].saveBattleUseItemHand_ = arg0;
			}

			public void saveBattlePitchLine_add(int _id, int arg0)
			{
				saveCursor_[_id].saveBattlePitchLine_ += arg0;
			}

			public void saveBattleItemLine_add(int _id, int arg0)
			{
				saveCursor_[_id].saveBattleItemLine_ += arg0;
			}

			public bool battleMode()
			{
				return battleMode_;
			}

			public void setBattleMode(bool flag)
			{
				battleMode_ = flag;
			}

			public bool tempFlag()
			{
				return tempFlag_;
			}

			public void setTempFlag(bool flag)
			{
				tempFlag_ = flag;
			}

			public Medget GetBaseMedget()
			{
				return baseMedget;
			}

			public Medget getFocuseMedget()
			{
				return focusedMedget;
			}

			public void SetTrialInitFocuseFlag(bool val)
			{
				trialInitFocuse = val;
			}

			public sys2d.Cell GetCursor2d()
			{
				return cellCursor2d;
			}

			public sys2d.Sprite3d GetCursor3d()
			{
				return cellCursor3d;
			}

			public sys2d.Cell GetSmallIcon2d()
			{
				return sIcon2d;
			}

			public sys2d.Sprite3d GetSmallIcon3d()
			{
				return sIcon3d;
			}

			public sys2d.Cell GetMenuButtonIcon2d()
			{
				return mIcon2d;
			}

			public sys2d.Sprite3d GetMenuButtonIcon3d()
			{
				return mIcon3d;
			}

			public sys2d.Cell GetMenuButtonIcon2d_2()
			{
				return mIcon2d_2;
			}

			public sys2d.Sprite3d GetMenuButtonIcon3d_2()
			{
				return mIcon3d_2;
			}

			public void SetImposibleScrollFlag(bool val)
			{
				imposibleScroll = val;
			}

			public bool GetImposibleScrollFlag()
			{
				return imposibleScroll;
			}

			public bool GetTouchPanelState()
			{
				return touchedFlag;
			}

			public void SetTouchPanelState(bool val)
			{
				touchedFlag = val;
			}

			public bool getPrevTouchPanelState()
			{
				return prevTouchedFlag_;
			}

			public void setPrevTouchPanelState(bool val)
			{
				prevTouchedFlag_ = val;
			}

			public bool notSEFlag()
			{
				return notSEFlag_;
			}

			public void setNotSEFlag(bool flag)
			{
				notSEFlag_ = flag;
			}

			public int GetItemListPatern()
			{
				return itemListPatern;
			}

			public void SetItemListPatern(int val)
			{
				itemListPatern = val;
			}

			public ds.Stack<MENU_BACKUP> GetBuildMenuStackLevel()
			{
				return stkMenuLevel;
			}

			public XbnNode xbnRoot()
			{
				return xbnDocument.root();
			}
		}
	}
}
