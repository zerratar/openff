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
	public static partial class ds
	{
		public static partial class sys3d
		{
			public class CModelTexture : TexDivideLoader.Handler, ITexture
			{
				private uint m_szTex;

				private uint m_szTex4x4;

				private uint m_szPltt;

				private Array m_pTexData;

				private TexVram m_keyTex;

				private TexVram m_keyTex4x4;

				private TexVram m_keyPltt;

				private NNSG3dResTex m_pResTex;

				private static uint FLAG_LOADED = 1u;

				private static uint FLAG_REQ_RELEASERES = 2u;

				private uint m_flag;

				public CModelTexture()
				{
					clearValue();
				}

				~CModelTexture()
				{
				}

				public void destruct()
				{
				}

				public void setup(Array texData, bool tdl)
				{
					if (m_pResTex == null)
					{
						m_pTexData = texData;
						ArrayReader arrayReader = new ArrayReader(texData);
						nmdp.SModelFileHeader sModelFileHeader = (nmdp.SModelFileHeader)arrayReader;
						nmdp.SModelInfoHeader infoHeader = sModelFileHeader.m_InfoHeader;
						_ = sModelFileHeader.ucFileType;
						if ((infoHeader.unFlag & 1) == 0)
						{
							infoHeader.unFlag |= 1u;
						}
						m_pResTex = NNS_G3dGetTex(sModelFileHeader.m_ResFileHeaderModel);
						m_szTex = NNS_G3dTexGetRequiredSize(m_pResTex);
						m_szTex4x4 = NNS_G3dTex4x4GetRequiredSize(m_pResTex);
						m_szPltt = NNS_G3dPlttGetRequiredSize(m_pResTex);
						NNS_GfdDumpLnkTexVramManager();
						NNS_GfdDumpLnkPlttVramManager();
						m_keyTex = NNS_GfdAllocLnkTexVram(m_szTex, 0, 0u);
						m_keyTex4x4 = NNS_GfdAllocLnkTexVram(m_szTex4x4, 1, 0u);
						m_keyPltt = NNS_GfdAllocLnkPlttVram(m_szPltt, 0, 1u);
						NNS_GfdDumpLnkTexVramManager();
						NNS_GfdDumpLnkPlttVramManager();
						_ = m_keyTex;
						if (m_szTex4x4 != 0)
						{
							_ = m_keyTex4x4;
						}
						_ = m_keyPltt;
						NNS_GfdGetTexKeyAddr(m_keyTex);
						NNS_GfdGetTexKeyAddr(m_keyTex4x4);
						NNS_GfdGetPlttKeyAddr(m_keyPltt);
						NNS_GfdGetTexKey4x4Flag(m_keyTex4x4);
						NNS_G3dTexSetTexKey(m_pResTex, m_keyTex, m_keyTex4x4);
						NNS_G3dPlttSetPlttKey(m_pResTex, m_keyPltt);
						NNS_G3dTexLoad(m_pResTex, 1);
						NNS_G3dPlttLoad(m_pResTex, 1);
						m_flag |= FLAG_LOADED;
						arrayReader.dispose();
					}
				}

				public void cleanup()
				{
					if (m_pResTex != null)
					{
						TexVram texVram = NNS_G3dPlttReleasePlttKey(m_pResTex);
						NNS_G3dTexReleaseTexKey(m_pResTex, out var texKey, out var tex4x4Key);
						if (texVram != null)
						{
							NNS_GfdFreeLnkPlttVram(texVram);
						}
						if (tex4x4Key != null)
						{
							NNS_GfdFreeLnkTexVram(tex4x4Key);
						}
						if (texKey != null)
						{
							NNS_GfdFreeLnkTexVram(texKey);
						}
						clearValue();
					}
				}

				public void bindMdlSet(NNSG3dResMdlSet pResMdlSet)
				{
					NNS_G3dBindMdlSet(pResMdlSet, m_pResTex);
				}

				public void bindMdl(NNSG3dResMdl pResMdl)
				{
					bindMdlToTex(pResMdl);
					bindMdlToPltt(pResMdl);
				}

				public void bindMdlToTex(NNSG3dResMdl pResMdl)
				{
					NNS_G3dBindMdlTex(pResMdl, m_pResTex);
				}

				public void bindMdlToTexByName(NNSG3dResMdl pResMdl, string szTexelname)
				{
					NNSG3dResName nNSG3dResName = new NNSG3dResName();
					strcpy(out nNSG3dResName.name, szTexelname);
					NNS_G3dBindMdlTexEx(pResMdl, m_pResTex, nNSG3dResName);
				}

				public void bindMdlToPltt(NNSG3dResMdl pResMdl)
				{
					NNS_G3dBindMdlPltt(pResMdl, m_pResTex);
				}

				public void bindMdlToPlttByName(NNSG3dResMdl pResMdl, string szPlttname)
				{
					NNSG3dResName nNSG3dResName = new NNSG3dResName();
					strcpy(out nNSG3dResName.name, szPlttname);
					NNS_G3dBindMdlPlttEx(pResMdl, m_pResTex, nNSG3dResName);
				}

				public void releaseMdlSet(NNSG3dResMdlSet pResMdlSet)
				{
					NNS_G3dReleaseMdlSet(pResMdlSet);
				}

				public void releaseMdl(NNSG3dResMdl pResMdl)
				{
					releaseMdlToTex(pResMdl);
					releaseMdlToPltt(pResMdl);
				}

				public void releaseMdlToTex(NNSG3dResMdl pResMdl)
				{
					NNS_G3dReleaseMdlTex(pResMdl);
				}

				public void releaseMdlToTexByName(NNSG3dResMdl pResMdl, string szTexelname)
				{
					NNSG3dResName nNSG3dResName = new NNSG3dResName();
					strcpy(out nNSG3dResName.name, szTexelname);
					NNS_G3dReleaseMdlTexEx(pResMdl, nNSG3dResName);
				}

				public void releaseMdlToPltt(NNSG3dResMdl pResMdl)
				{
					NNS_G3dReleaseMdlPltt(pResMdl);
				}

				public void releaseMdlToPlttByName(NNSG3dResMdl pResMdl, string szPlttname)
				{
					NNSG3dResName nNSG3dResName = new NNSG3dResName();
					strcpy(out nNSG3dResName.name, szPlttname);
					NNS_G3dReleaseMdlPlttEx(pResMdl, nNSG3dResName);
				}

				public void reqReleaseResource()
				{
					if ((FLAG_LOADED & m_flag) != 0)
					{
						releaseResource();
					}
					else
					{
						m_flag |= FLAG_REQ_RELEASERES;
					}
				}

				public override void tdlhCompletion(int receipt_number)
				{
					m_flag |= FLAG_LOADED;
					if ((FLAG_REQ_RELEASERES & m_flag) != 0)
					{
						releaseResource();
					}
				}

				public void releaseResource()
				{
					uint num = 0u;
					_ = 0;
					CHeap.resize_app(m_pTexData, num);
					OS_Printf("\n\n\n\n\n\n\n\n\n\n\n\n");
					OS_Printf("%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%\n");
					OS_Printf(" releaseResource() Succeeded!!\n\n");
				}

				public static bool isModelTexture(Array pTexture)
				{
					ArrayReader arrayReader = new ArrayReader(pTexture);
					nmdp.SModelFileHeader sModelFileHeader = (nmdp.SModelFileHeader)arrayReader;
					nmdp.SModelInfoHeader infoHeader = sModelFileHeader.m_InfoHeader;
					arrayReader.dispose();
					byte[] ucFileType = sModelFileHeader.ucFileType;
					if (ucFileType[0] != 78 || ucFileType[1] != 77 || ucFileType[2] != 68 || ucFileType[3] != 80)
					{
						return false;
					}
					if (nmdp.NMDP_CURRENT_VERSION != sModelFileHeader.unVersion)
					{
						return false;
					}
					if ((infoHeader.unFlag & 4) == 0)
					{
						return false;
					}
					return true;
				}

				public void clearValue()
				{
					m_pResTex = null;
					m_pTexData = null;
					m_szTex = 0u;
					m_szTex4x4 = 0u;
					m_szPltt = 0u;
					m_keyTex = null;
					m_keyTex4x4 = null;
					m_keyPltt = null;
					m_flag = 0u;
				}

				public bool isLoadedToVram()
				{
					return (m_flag & FLAG_LOADED) != 0;
				}

				public NNSG3dResTex getResTex()
				{
					return m_pResTex;
				}

				public static Array ChainPointer(byte[] abyData, int iId)
				{
					ArrayReader arrayReader = new ArrayReader(abyData);
					arrayReader.skip(16L);
					arrayReader.skip(8 * iId);
					uint num = arrayReader.readUInt32();
					arrayReader.readUInt32();
					arrayReader.setPosition(num);
					Array array = new byte[arrayReader.rest()];
					arrayReader.read((byte[])array, 0, array.Length);
					return array;
				}
			}
		}
	}
}
