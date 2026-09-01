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
	public static partial class wmenu
	{
							public class CWMenuPCFaceManager
							{
								public static uint FCFM_PLAYER_NUM = 4u;

								public static uint PCFM_FLAG_NOT_SHOW = 1u;

								private uint _Flag;

								private sys2d.Bg _Bg = new sys2d.Bg();

								private CWMenuPCFace[] _PCFace = new CWMenuPCFace[FCFM_PLAYER_NUM];

								private NNSG2dBGSelect _BgSelect;

								public CWMenuPCFaceManager()
								{
									for (int i = 0; i < _PCFace.Length; i++)
									{
										_PCFace[i] = new CWMenuPCFace();
									}
									_Flag = 0u;
									for (int j = 0; j < 4; j++)
									{
										_PCFace[j].pcfSetPC((uint)j);
									}
								}

								public void pcfmSetup(NNSG2dBGSelect bg, GXBGScrBase scn_base, GXBGCharBase chr_base)
								{
									_BgSelect = bg;
									pcfmSetStatusDefault(fr0: false, fr1: false, fr2: false, fr3: false);
								}

								public void pcfmSetup(NNSG2dBGSelect bg)
								{
									_BgSelect = bg;
									pcfmSetStatusDefault(fr0: false, fr1: false, fr2: false, fr3: false);
								}

								public void pcfmCleanup()
								{
									_Bg.bgRelease();
								}

								public void pcfmSetPosition(uint pc, NNSG2dSVec2 pos, bool clear)
								{
									bool flag = _PCFace[pc].pcfIsShow();
									if (clear)
									{
										_PCFace[pc].pcfSetShow(show: false, _BgSelect);
									}
									_PCFace[pc].pcfSetPosition(pos);
									if (flag)
									{
										_PCFace[pc].pcfSetShow(show: true, _BgSelect);
									}
								}

								public void pcfmSetPosition(uint pc, short x, short y, bool clear)
								{
									pcfmSetPosition(pc, new NNSG2dSVec2(x, y), clear);
								}

								public void pcfmSetJob(uint pc, uint no)
								{
									_PCFace[pc].pcfSetJob(no);
									sprintf(out var arg, "pc%d_%02d", pc + 1, no + 1);
									sprintf(out var arg2, "%s.%s", arg, "NCGR");
									sys2d.Ncgr ncgr = new sys2d.Ncgr();
									ncgr.LoadBg(arg2);
									NNSG2dCharacterData pChrData = ncgr.pDataCg();
									NNS_G2dBGSetupChar((int)(pc + 8), pChrData);
								}

								public void pcfmClear()
								{
									for (int i = 0; i < 4; i++)
									{
										pcfmSetShow((uint)i, show: false);
									}
								}

								public void pcfmSetShow(bool show)
								{
									_Bg.bgSetShow(show);
									ds.switchFlag(PCFM_FLAG_NOT_SHOW, !show, ref _Flag);
								}

								public void pcfmSetShow(uint pc, bool show)
								{
									_PCFace[pc].pcfSetShow(show, _BgSelect);
								}

								public void pcfmSetStatusDefault(bool[] front)
								{
									byte b = 1;
									byte b2 = 2;
									byte[] array = new byte[4] { 2, 7, 12, 17 };
									for (int i = 0; i < 4; i++)
									{
										pcfmSetPosition((uint)i, front[i] ? b : b2, array[i], clear: false);
									}
								}

								public void pcfmSetPosition(int x, int y)
								{
									_Bg.bgSetPosition(x, y);
								}

								public NNSG2dSVec2 pcfmGetPosition(uint pc)
								{
									return _PCFace[pc].pcfGetPosition();
								}

								public bool pcfmIsShow()
								{
									if (!ds.isFlag(PCFM_FLAG_NOT_SHOW, ref _Flag))
									{
										return true;
									}
									return false;
								}

								public bool pcfmIsShow(uint pc)
								{
									return _PCFace[pc].pcfIsShow();
								}

								public void pcfmSetStatusDefault(bool fr0, bool fr1, bool fr2, bool fr3)
								{
									bool[] front = new bool[4] { fr0, fr1, fr2, fr3 };
									pcfmSetStatusDefault(front);
								}
							}
	}
}
