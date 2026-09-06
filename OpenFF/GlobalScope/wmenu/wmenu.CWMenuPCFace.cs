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
	public static partial class wmenu
	{
							public class CWMenuPCFace
							{
								public static uint PCF_JOB_INVALID = 0u;

								public static uint PCF_FLAG_NOT_SHOW = 1u;

								private uint _Flag;

								private uint _PC;

								private uint _Job;

								private NNSG2dSVec2 _Position;

								public CWMenuPCFace()
								{
									_Flag = 0u;
									_PC = 0u;
									_Job = PCF_JOB_INVALID;
								}

								public void pcfSetPosition(NNSG2dSVec2 pos)
								{
									_Position = pos;
								}

								public void pcfSetShow(bool show, NNSG2dBGSelect bgSelect)
								{
									ds.switchFlag(PCF_FLAG_NOT_SHOW, !show, ref _Flag);
									if (pcfIsShow())
									{
										sys2d.Nscr nscr = new sys2d.Nscr();
										nscr.Load("menu_pc.NSCR");
										NNS_G2dBGSetupCell((int)(_PC + 8), nscr.pDataCe(), bgSelect, _Position.x * 8, _Position.y * 8);
									}
									else
									{
										NNS_G2dBGSetupCell((int)(_PC + 8), null, bgSelect);
									}
								}

								public void pcfSetPC(uint no)
								{
									_PC = no;
								}

								public void pcfSetPosition(short x, short y)
								{
									pcfSetPosition(new NNSG2dSVec2(x, y));
								}

								public NNSG2dSVec2 pcfGetPosition()
								{
									return _Position;
								}

								public bool pcfIsShow()
								{
									if (!ds.isFlag(PCF_FLAG_NOT_SHOW, ref _Flag))
									{
										return true;
									}
									return false;
								}

								public uint pcfGetPC()
								{
									return _PC;
								}

								public void pcfSetJob(uint job_no)
								{
									_Job = job_no;
								}

								public uint pcfGetJob()
								{
									return _Job;
								}
							}
	}
}
