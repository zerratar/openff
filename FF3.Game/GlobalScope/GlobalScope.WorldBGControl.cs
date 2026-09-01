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
						public class WorldBGControl
						{
							private delegate void _pSetPriority(int arg0);

							public class WBCFileHeader
							{
								public byte[] fileType_ = new byte[4];

								public byte numBg_;

								public byte haveFontData_;

								public byte[] pad = new byte[2];

								public int[] reserve_ = new int[2];

								public WBFontData m_FontData;

								public WBData[] m_aData;

								public static explicit operator WBCFileHeader(Array src)
								{
									WBCFileHeader wBCFileHeader = new WBCFileHeader();
									ArrayReader arrayReader = new ArrayReader(src);
									arrayReader.read(wBCFileHeader.fileType_, 0, 4);
									wBCFileHeader.numBg_ = arrayReader.readByte();
									wBCFileHeader.haveFontData_ = arrayReader.readByte();
									arrayReader.read(wBCFileHeader.pad, 0, 2);
									arrayReader.read(wBCFileHeader.reserve_, 0, 2);
									if (wBCFileHeader.haveFontData_ != 0)
									{
										wBCFileHeader.m_FontData = new WBFontData();
										wBCFileHeader.m_FontData.parse(arrayReader);
									}
									wBCFileHeader.m_aData = new WBData[wBCFileHeader.numBg_];
									for (int i = 0; i < wBCFileHeader.numBg_; i++)
									{
										wBCFileHeader.m_aData[i] = new WBData();
										wBCFileHeader.m_aData[i].parse(arrayReader);
									}
									arrayReader.dispose();
									return wBCFileHeader;
								}
							}

							public class WBFontData
							{
								private byte bgSelect_;

								private byte scrBase_;

								private byte chrBase_;

								private byte priority_;

								private byte[] pad = new byte[4];

								public void parse(ArrayReader reader)
								{
									bgSelect_ = reader.readByte();
									scrBase_ = reader.readByte();
									chrBase_ = reader.readByte();
									priority_ = reader.readByte();
									reader.read(pad, 0, 4);
								}
							}

							public class WBData
							{
								public string filename_;

								public byte bgSelect_;

								public byte scrBase_;

								public byte chrBase_;

								public byte priority_;

								public byte[] pad = new byte[4];

								public void parse(ArrayReader reader)
								{
									byte[] array = new byte[16];
									reader.read(array, 0, array.Length);
									filename_ = StringUtil.createString(array);
									bgSelect_ = reader.readByte();
									scrBase_ = reader.readByte();
									chrBase_ = reader.readByte();
									priority_ = reader.readByte();
									reader.read(pad, 0, 4);
								}
							}

							private static byte WBC_FLAG_LOADED = 1;

							private byte flag_;

							private WorldBG[] wbg_ = new WorldBG[8];

							private WorldBGEffect[] effect_ = new WorldBGEffect[2];

							public WorldBGControl()
							{
								flag_ = 0;
								for (int i = 0; i < wbg_.Length; i++)
								{
									wbg_[i] = new WorldBG();
								}
								for (int i = 0; i < effect_.Length; i++)
								{
									effect_[i] = new WorldBGEffect();
								}
							}

							public void wbcInitialize()
							{
								for (byte b = 0; b < 8; b++)
								{
									wbg_[b].wbInit();
								}
								flag_ = 0;
								effect_[0].wbeInit(WBE_SCREENSELECT.WBE_SCREENSELECT_MAIN);
								effect_[1].wbeInit(WBE_SCREENSELECT.WBE_SCREENSELECT_SUB);
							}

							public void wbcSetup(string pFilename)
							{
								if ((flag_ & WBC_FLAG_LOADED) == 0)
								{
									string arg = "";
									sprintf(out arg, "%s.wbc", pFilename);
									uint size = ds.g_File.getSize(arg);
									Array array = ds.CHeap.alloc_app(size);
									ds.g_File.load(array, arg);
									WBCFileHeader wBCFileHeader = (WBCFileHeader)array;
									WBData[] array2 = null;
									if (1 == wBCFileHeader.haveFontData_)
									{
										_ = wBCFileHeader.m_FontData;
										array2 = wBCFileHeader.m_aData;
									}
									else
									{
										array2 = wBCFileHeader.m_aData;
									}
									_ = wBCFileHeader.fileType_;
									for (int i = 0; i < wBCFileHeader.numBg_; i++)
									{
										WBData wbData = array2[i];
										wbcSetupBG(wbData);
									}
									ds.CHeap.free_app(array);
								}
							}

							public void wbcExecute()
							{
								for (byte b = 0; b < 8; b++)
								{
									wbg_[b].wbExecute();
								}
								effect_[0].wbeExecute();
								effect_[1].wbeExecute();
							}

							public void wbcSetPosition(NNSG2dBGSelect select, int x, int y)
							{
								wbg_[(int)select].bg_.bgSetPosition(x, y);
							}

							public void wbcGetPosition(NNSG2dBGSelect select, out int x, out int y)
							{
								wbg_[(int)select].bg_.bgGetPosition(out x, out y);
							}

							public void wbcAddPosition(NNSG2dBGSelect select, int x, int y)
							{
								wbg_[(int)select].bg_.bgGetPosition(out var x2, out var y2);
								wbg_[(int)select].bg_.bgSetPosition(x2 + x, y2 + y);
							}

							public void wbcSetScroll(NNSG2dBGSelect select, int frame, int x, int y)
							{
								wbg_[(int)select].wbSetScroll((short)frame, (short)x, (short)y);
							}

							public void wbcSetVisible(NNSG2dBGSelect select, bool visible)
							{
								wbg_[(int)select].bg_.bgSetShow(visible);
							}

							public void wbcSetEffect(WBE_SCREENSELECT select, WBE_EFFECTTYPE type, int planeA, int planeB, sbyte startValue, sbyte endValue, int frame)
							{
								effect_[(int)select].wbeSetEffect(type, planeA, planeB, startValue, endValue, frame);
							}

							public void wbcSetupBG(WBData wbData)
							{
								wbg_[wbData.bgSelect_].wbInit();
								wbg_[wbData.bgSelect_].flag_ = 1;
								wbg_[wbData.bgSelect_].bg_.bgLoad2(const_cast<string>(wbData.filename_));
								wbg_[wbData.bgSelect_].bg_.bgSetUp((NNSG2dBGSelect)wbData.bgSelect_, (GXBGScrBase)wbData.scrBase_, (GXBGCharBase)wbData.chrBase_);
								wbg_[wbData.bgSelect_].bg_.bgRelease();
								wbg_[wbData.bgSelect_].bg_.bgSetPosition(0, 0);
								_pSetPriority[] array = new _pSetPriority[8] { G2_SetBG0Priority, G2_SetBG1Priority, G2_SetBG2Priority, G2_SetBG3Priority, G2S_SetBG0Priority, G2S_SetBG1Priority, G2S_SetBG2Priority, G2S_SetBG3Priority };
								array[wbData.bgSelect_](wbData.priority_);
							}
						}
}
