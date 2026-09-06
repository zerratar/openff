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
	public static class stg
	{
		// PORT: FF4's overworld is FF3's chip layout mirrored along z (see OpenFF.Client.FieldMirror).
		// Set with the stage type; read wherever a chip meets a world position.
		public static bool MirrorZ;

		public class CStageChip
		{
			public enum STATE_TYPE
			{
				STATE_TYPE_ERROR = -1,
				STATE_TYPE_UNLOAD,
				STATE_TYPE_LOADING,
				STATE_TYPE_LOADED,
				STATE_TYPE_PRELOAD,
				STATE_TYPE_WAIT_SETUP,
				STATE_TYPE_ALL
			}

			public const STATE_TYPE STATE_TYPE_ERROR = STATE_TYPE.STATE_TYPE_ERROR;

			public const STATE_TYPE STATE_TYPE_UNLOAD = STATE_TYPE.STATE_TYPE_UNLOAD;

			public const STATE_TYPE STATE_TYPE_LOADING = STATE_TYPE.STATE_TYPE_LOADING;

			public const STATE_TYPE STATE_TYPE_LOADED = STATE_TYPE.STATE_TYPE_LOADED;

			public const STATE_TYPE STATE_TYPE_PRELOAD = STATE_TYPE.STATE_TYPE_PRELOAD;

			public const STATE_TYPE STATE_TYPE_WAIT_SETUP = STATE_TYPE.STATE_TYPE_WAIT_SETUP;

			public const STATE_TYPE STATE_TYPE_ALL = STATE_TYPE.STATE_TYPE_ALL;

			private ds.sys3d.CModelSet m_ModelSet = new ds.sys3d.CModelSet();

			public ds.sys3d.CAnimSet m_AnimSet = new ds.sys3d.CAnimSet();

			public ds.sys3d.CRenderObject m_RdrObject = new ds.sys3d.CRenderObject();

			public dgs.CRestrictor m_Collision = new dgs.CRestrictor();

			private ds.sys3d.Scene m_pScene;

			private ds.sys3d.CModelTexture m_pMdlTex;

			private Array m_pData;

			private Array m_pMdlData;

			private Array m_pAnmData;

			private mcl.CMapCollision[] m_pColData;

			public ChipData m_Chip = new ChipData();

			public STATE_TYPE m_State;

			public ds.StreamArchiver m_pSarc;

			public CStageChip()
			{
				initValue();
			}

			public void initialize()
			{
				m_pData = null;
				m_pData = ds.CHeap.alloc_app(59392u);
			}

			public void terminate()
			{
				if (m_pData != null)
				{
					ds.CHeap.free_app(m_pData);
				}
				m_pData = null;
			}

			public void initValue()
			{
				m_pScene = null;
				m_pMdlTex = null;
				m_pMdlData = null;
				m_pAnmData = null;
				m_pColData = null;
				m_State = STATE_TYPE.STATE_TYPE_UNLOAD;
				m_Chip.Name = "";
				VEC_Set(m_Chip.Pos, 0, 0, 0);
				VEC_Set(m_Chip.Size, 0, 0, 0);
				m_Chip.fnoX = -1;
				m_Chip.fnoZ = -1;
				m_Chip.RelativeX = 0;
				m_Chip.RelativeZ = 0;
				m_pSarc = null;
			}

			public void setup(STAGE_TYPE stageType)
			{
				m_pMdlData = pack.ChainPointer(static_cast<byte[]>(m_pData), 0u);
				m_pAnmData = pack.ChainPointer(static_cast<byte[]>(m_pData), 1u);
				m_pColData = mcl.CMapCollision.ChainPointer(static_cast<byte[]>(m_pData), 2);
				if (pack.ChainPointerSize(static_cast<byte[]>(m_pData), 1u) == 0)
				{
					m_pAnmData = null;
				}
				m_ModelSet.setup(m_pMdlData);
				m_RdrObject.setup(m_ModelSet.getMdlResource());
				m_pScene.addRenderObject(m_RdrObject, 0);
				m_RdrObject.setPosition(m_Chip.Pos);
				if (OpenFF.Client.Options.Get("probe") != null)
				{
					OpenFF.Client.Log.Write(OpenFF.Client.LogChannel.General, "chip: " + m_Chip.Name + " at (" + m_Chip.Pos.x / 4096.0 + "," + m_Chip.Pos.z / 4096.0 + ")" + (MirrorZ ? " mirrored" : ""));
				}
				if (MirrorZ)
				{
					// PORT: the chip's own geometry mirrored about its centre, in the data rather than
					// by a scale of -1 on the pose: a mirroring matrix lost the relief of FF4's
					// multi-node chips somewhere past the transform, while the data mirror keeps every
					// matrix a rotation. DrawModel swaps the cull face for a mirrored model.
					OpenFF.Client.FieldMirror.MirrorModel(m_ModelSet.getMdl(0u).getMdlResource(), m_Chip.Name);
				}
				if (m_pMdlTex != null)
				{
					m_ModelSet.bindReplaceTex(m_pMdlTex);
				}
				if (m_pAnmData != null)
				{
					m_AnimSet.setup(m_pAnmData, m_ModelSet.getMdlResource(), null);
					m_AnimSet.addRenderObject(m_RdrObject.getRenderObject());
					m_AnimSet.setLoop(b: true, ds.sys3d.CAnimSet.enTYPE.enTYPE_END);
					m_AnimSet.start(0, ds.sys3d.CAnimSet.enTYPE.enTYPE_END);
				}
				m_pColData[0].initialize();
				m_Collision.rorAppend(m_pColData[0]);
				m_Collision.rorSetActivity(_b: false);
				if (STAGE_TYPE.STAGE_TYPE_FIELD04 != stageType)
				{
					NNS_G3dMdlSetMdlXLDepthUpdateAll(m_ModelSet.getMdl(0u).getMdlResource(), 1);
				}
				m_State = STATE_TYPE.STATE_TYPE_LOADED;
			}

			public void cleanup()
			{
				if (STATE_TYPE.STATE_TYPE_LOADED == m_State)
				{
					m_ModelSet.cleanup();
					m_AnimSet.cleanup();
					m_pScene.removeRenderObject(m_RdrObject);
					m_RdrObject.cleanup();
					m_Collision.rorRemove();
					initValue();
				}
			}

			public void strongSetup(ds.sys3d.Scene pScene, ds.sys3d.CModelTexture pMdlTex, STAGE_TYPE stageType)
			{
				prepareSetupByStream(pScene, pMdlTex);
				ds.FileArchiver fileArchiver = new ds.FileArchiver();
				ds.Archive.CompressInfo compressInfo = new ds.Archive.CompressInfo();
				string arg = "";
				sprintf(out arg, "%s.flsc.lz", m_Chip.Name);
				fileArchiver.analysisFile(compressInfo, arg);
				if (compressInfo.unExtractSize != 0)
				{
					fileArchiver.uncompressFile(m_pData);
				}
				setup(stageType);
			}

			public void prepareSetupByStream(ds.sys3d.Scene pScene, ds.sys3d.CModelTexture pMdlTex)
			{
				m_State = STATE_TYPE.STATE_TYPE_PRELOAD;
				m_pScene = pScene;
				m_pMdlTex = pMdlTex;
			}

			public void startSetupByStream(ds.StreamArchiver pSarc)
			{
				m_State = STATE_TYPE.STATE_TYPE_LOADING;
				ds.Archive.CompressInfo compressInfo = new ds.Archive.CompressInfo();
				sprintf(out var arg, "%s.flsc.lz", m_Chip.Name);
				m_pSarc = pSarc;
				m_pSarc.analysisReadFile(compressInfo, arg);
				if (0 < compressInfo.unExtractSize && m_pData != null)
				{
					m_pSarc.prepareReadFile(m_pData, COMPSIZE * 1024);
				}
			}

			public void cancelSetupByStream()
			{
				m_State = STATE_TYPE.STATE_TYPE_UNLOAD;
				m_pSarc.cancelReadFile();
				cleanup();
			}

			public void execute()
			{
				switch (m_State)
				{
				case STATE_TYPE.STATE_TYPE_LOADED:
					m_AnimSet.next();
					break;
				case STATE_TYPE.STATE_TYPE_UNLOAD:
				case STATE_TYPE.STATE_TYPE_LOADING:
				case STATE_TYPE.STATE_TYPE_PRELOAD:
					break;
				}
			}

			public void getRelativeSpot(VecFx32 pos, out sbyte x, out sbyte z)
			{
				int num = (m_Chip.Size.x >> 1) - 512;
				int num2 = (m_Chip.Size.z >> 1) - 512;
				int x2 = m_Chip.Pos.x;
				int num3 = m_Chip.Pos.z + num2;
				int num4 = pos.x - x2;
				int num5 = pos.z - num3;
				if (MirrorZ)
				{
					num5 = -(pos.z - m_Chip.Pos.z) - num2;
				}
				int num6 = num;
				int num7 = num2;
				if (num4 < 0)
				{
					num6 *= -1;
				}
				if (num5 < 0)
				{
					num7 *= -1;
				}
				x = (sbyte)(FX_Div(num4 + num6, m_Chip.Size.x) / 4096);
				z = (sbyte)(FX_Div(num5 + num7, m_Chip.Size.z) / 4096);
			}

			public void getSpot(out sbyte x, out sbyte z)
			{
				x = m_Chip.RelativeX;
				z = m_Chip.RelativeZ;
			}

			public void moveSpot(sbyte x, sbyte z)
			{
				m_Chip.RelativeX += x;
				m_Chip.RelativeZ += z;
			}

			public void getFileNo(out sbyte x, out sbyte z)
			{
				x = m_Chip.fnoX;
				z = m_Chip.fnoZ;
			}

			public void getPos(VecFx32 @out)
			{
				@out.copy(m_Chip.Pos);
			}

			public void getWorldSpot(out sbyte x, out sbyte z)
			{
				x = m_Chip.SpotX;
				z = m_Chip.SpotZ;
			}

			public bool isIn(VecFx32 pos)
			{
				int num = m_Chip.Size.x >> 1;
				int num2 = m_Chip.Size.z >> 1;
				bool flag = m_Chip.Pos.x - num <= pos.x && pos.x < m_Chip.Pos.x + num;
				bool result = m_Chip.Pos.z - num2 <= pos.z && pos.z < m_Chip.Pos.z + num2;
				if (flag)
				{
					return result;
				}
				return false;
			}

			public STATE_TYPE getState()
			{
				return m_State;
			}

			public string getName()
			{
				return m_Chip.Name;
			}
		}

		public class StageLoadState
		{
			private static sbyte ROW_NUM = 5;

			private static sbyte COL_NUM = 5;

			private static sbyte ROW_CENTER = (sbyte)(ROW_NUM / 2);

			private static sbyte COL_CENTER = (sbyte)(COL_NUM / 2);

			private static sbyte ROW_MAX = (sbyte)(ROW_CENTER + 1);

			private static sbyte ROW_MIN = (sbyte)(ROW_CENTER - 1);

			private static sbyte COL_MAX = (sbyte)(COL_CENTER + 1);

			private static sbyte COL_MIN = (sbyte)(COL_CENTER - 1);

			private byte[,] _loaded = new byte[COL_NUM, ROW_NUM];

			public void initialize()
			{
				MI_CpuClear8(_loaded, COL_NUM * ROW_NUM);
			}

			public void terminate()
			{
			}

			public bool getUnnecessaryChipNo(ref sbyte x, ref sbyte z)
			{
				for (byte b = 0; b < ROW_NUM; b++)
				{
					if (1 == _loaded[0, b])
					{
						x = (sbyte)(b - ROW_CENTER);
						z = (sbyte)(-COL_CENTER);
						return true;
					}
					if (1 == _loaded[COL_NUM - 1, b])
					{
						x = (sbyte)(b - ROW_CENTER);
						z = (sbyte)(COL_NUM - 1 - COL_CENTER);
						return true;
					}
				}
				for (byte b2 = 0; b2 < COL_NUM; b2++)
				{
					if (1 == _loaded[b2, 0])
					{
						x = (sbyte)(-ROW_CENTER);
						z = (sbyte)(b2 - COL_CENTER);
						return true;
					}
					if (1 == _loaded[b2, ROW_NUM - 1])
					{
						x = (sbyte)(ROW_NUM - 1 - ROW_CENTER);
						z = (sbyte)(b2 - COL_CENTER);
						return true;
					}
				}
				return false;
			}

			public bool getNecessaryChipNo(ref sbyte x, ref sbyte z)
			{
				for (byte b = (byte)COL_MIN; b <= COL_MAX; b++)
				{
					for (byte b2 = (byte)ROW_MIN; b2 <= ROW_MAX; b2++)
					{
						if (_loaded[b, b2] == 0)
						{
							x = (sbyte)(b2 - ROW_CENTER);
							z = (sbyte)(b - COL_CENTER);
							return true;
						}
					}
				}
				return false;
			}

			public void move(sbyte x, sbyte z)
			{
				byte[,] array = new byte[COL_NUM, ROW_NUM];
				MI_CpuClear8(array, COL_NUM * ROW_NUM);
				for (byte b = 0; b < COL_NUM; b++)
				{
					for (byte b2 = 0; b2 < ROW_NUM; b2++)
					{
						if (1 == _loaded[b, b2] && b - z >= 0 && b - z < COL_NUM && b2 - x >= 0 && b2 - x < ROW_NUM)
						{
							array[b - z, b2 - x] = 1;
						}
					}
				}
				MI_CpuCopy8(array, _loaded, COL_NUM * ROW_NUM);
			}

			public void reportLoadedChip(sbyte x, sbyte z)
			{
				if (1 != _loaded[ROW_CENTER + z, COL_CENTER + x])
				{
					_loaded[ROW_CENTER + z, COL_CENTER + x] = 1;
				}
			}

			public void reportUnloadedChip(sbyte x, sbyte z)
			{
				if (_loaded[ROW_CENTER + z, COL_CENTER + x] != 0)
				{
					_loaded[ROW_CENTER + z, COL_CENTER + x] = 0;
				}
			}
		}

		public class CStageMng
		{
			/// <summary>PORT: the name of the stage last loaded (d01_05, f00, t24_01...), for the debug overlay.</summary>
			public static string CurrentName;

			/// <summary>PORT: the stage's profile (chip grid, spots), for the debug overlay.</summary>
			public CStageProfile StageProfile()
			{
				return m_stgPrf;
			}

			/// <summary>PORT: where the hero's feet are, as the world system last told the stage; for the debug overlay.</summary>
			public VecFx32 FootPos()
			{
				return m_FootPos;
			}

			public enum FAKEMATERIAL_TYPE
			{
				TYPE_TOON,
				TYPE_HIGHLIGHT
			}

			public enum STATE_TYPE
			{
				STATE_TYPE_ERROR = -1,
				STATE_TYPE_NORMAL,
				STATE_TYPE_LOAD_WAIT,
				STATE_TYPE_PRELOAD,
				STATE_TYPE_LOADING,
				STATE_TYPE_ALL
			}

			public enum FLAG_TYPE
			{
				FLAG_ENABLE = 1,
				FLAG_FIRST_SET = 2,
				FLAG_OVER_CHIPS = 4,
				FLAG_FIELD = 8,
				FLAG_CHIPCHANGE = 0x10
			}

			public const FAKEMATERIAL_TYPE TYPE_TOON = FAKEMATERIAL_TYPE.TYPE_TOON;

			public const FAKEMATERIAL_TYPE TYPE_HIGHLIGHT = FAKEMATERIAL_TYPE.TYPE_HIGHLIGHT;

			public const STATE_TYPE STATE_TYPE_ERROR = STATE_TYPE.STATE_TYPE_ERROR;

			public const STATE_TYPE STATE_TYPE_NORMAL = STATE_TYPE.STATE_TYPE_NORMAL;

			public const STATE_TYPE STATE_TYPE_LOAD_WAIT = STATE_TYPE.STATE_TYPE_LOAD_WAIT;

			public const STATE_TYPE STATE_TYPE_PRELOAD = STATE_TYPE.STATE_TYPE_PRELOAD;

			public const STATE_TYPE STATE_TYPE_LOADING = STATE_TYPE.STATE_TYPE_LOADING;

			public const STATE_TYPE STATE_TYPE_ALL = STATE_TYPE.STATE_TYPE_ALL;

			public const FLAG_TYPE FLAG_ENABLE = FLAG_TYPE.FLAG_ENABLE;

			public const FLAG_TYPE FLAG_FIRST_SET = FLAG_TYPE.FLAG_FIRST_SET;

			public const FLAG_TYPE FLAG_OVER_CHIPS = FLAG_TYPE.FLAG_OVER_CHIPS;

			public const FLAG_TYPE FLAG_FIELD = FLAG_TYPE.FLAG_FIELD;

			public const FLAG_TYPE FLAG_CHIPCHANGE = FLAG_TYPE.FLAG_CHIPCHANGE;

			private STAGE_TYPE m_StageType;

			private STATE_TYPE m_State;

			private uint m_MngFlag;

			private sbyte m_MidChipIdx;

			private sbyte m_ColChipIdx;

			private string m_ChipName;

			private VecFx32 m_FootPos = new VecFx32();

			private VecFx32 m_PreFootPos = new VecFx32();

			private ds.sys3d.Scene m_pScene;

			private Array m_pTexData;

			private static byte CHIP_NUM = 9;

			private CStageChip[] m_Chips = new CStageChip[CHIP_NUM];

			private sbyte m_LoadedChipsNum;

			private ds.sys3d.CModelTexture m_MdlTex = new ds.sys3d.CModelTexture();

			private ds.StreamArchiver m_Sarc = new ds.StreamArchiver();

			private CStageProfile m_stgPrf = new CStageProfile();

			private StageLoadState m_stgLoadState = new StageLoadState();

			private sbyte m_loadingChip;

			private string m_stagePath;

			private uint m_Flag;

			private CFileData MdlData = new CFileData();

			private CFileData AnmData = new CFileData();

			private CFileData ColData = new CFileData();

			private CFileData LgtData = new CFileData();

			private CFileData BoxData = new CFileData();

			private ds.sys3d.CModelSet m_ModelSet = new ds.sys3d.CModelSet();

			private ds.sys3d.CAnimSet m_AnimSet = new ds.sys3d.CAnimSet();

			private ds.sys3d.CRenderObject m_RdrObject = new ds.sys3d.CRenderObject();

			// PORT (FF4): a scene's map motions - .ncap packs played on the stage model, the way
			// characters play theirs (ce_SetMapMotion / ce_MapStartMotion); FF4's CStageMng has
			// addMotion(const char*), startMotion(int, bool, uint) and startAnimation(uint, enTYPE, int).
			private ds.sys3d.CMotSet m_MotSet;

			private readonly System.Collections.Generic.List<CFileData> m_MotData = new System.Collections.Generic.List<CFileData>();

			public void addMotion(string motname)
			{
				if (m_Flag == 0 || m_ModelSet.getMdlResource() == null)
				{
					return;
				}
				CFileData data = new CFileData();
				data.setup(motname + ".ncap.lz", ds.fs.enFDL_FILETYPE.enFDL_FILETYPE_COMPRESS);
				if (data.getSize() <= 0)
				{
					OpenFF.Client.Log.Write(OpenFF.Client.LogChannel.General, "stage: map motion " + motname + " was not found");
					return;
				}
				if (m_MotSet == null)
				{
					m_MotSet = new ds.sys3d.CMotSet();
					m_MotSet.setup(m_ModelSet.getMdlResource());
					m_MotSet.addRenderObject(m_RdrObject.getRenderObject());
				}
				m_MotData.Add(data);
				m_MotSet.addMotion(data.getAddr<ds.sys3d.ncap.SMotionFileHeader>());
				OpenFF.Client.Log.Write(OpenFF.Client.LogChannel.File, "stage: map motion pack " + motname + " (" + data.getSize() + " bytes) on " + CurrentName);
			}

			public bool startMotion(int id, bool loop, uint blendFrame)
			{
				if (m_MotSet == null)
				{
					return false;
				}
				if (!m_MotSet.isMotion((uint)id))
				{
					OpenFF.Client.Log.Write(OpenFF.Client.LogChannel.General, "stage: map motion " + id + " is not in the loaded packs");
					return false;
				}
				m_MotSet.start((uint)id, loop, blendFrame);
				OpenFF.Client.Log.Write(OpenFF.Client.LogChannel.File, "stage: map motion " + id + (loop ? " looping" : "") + " blend " + blendFrame + " -> index " + m_MotSet.getIndex() + ", " + m_MotSet.getMaxFrame() + " frames, type " + m_StageType);
				return true;
			}

			public bool isEndOfMapMotion()
			{
				return m_MotSet == null || m_MotSet.isEndOfMotion();
			}

			public bool startAnimation(uint index, ds.sys3d.CAnimSet.enTYPE type, int frame)
			{
				if (m_Flag == 0 || type >= ds.sys3d.CAnimSet.enTYPE.enTYPE_END)
				{
					return false;
				}
				bool ok = m_AnimSet.startAnimation(index, type, frame);
				OpenFF.Client.Log.Write(OpenFF.Client.LogChannel.File, "stage: map animation " + index + " type " + type + (ok ? " started" : " not started"));
				return ok;
			}

			private void cleanupMapMotions()
			{
				if (m_MotSet != null)
				{
					m_MotSet.removeRenderObject(m_RdrObject.getRenderObject());
					m_MotSet.cleanup();
					m_MotSet = null;
				}
				foreach (CFileData data in m_MotData)
				{
					data.cleanup();
				}
				m_MotData.Clear();
			}

			private dgs.CRestrictor m_Collision = new dgs.CRestrictor();

			private ds.sys3d.CBoxTest m_BoxTest;

			private byte _fakeMatFlag;

			private ushort _fakeMatColorOrg;

			private ushort _fakeMatColorNow;

			private ushort _fakeMatColorTar;

			private int _fakeMatFrame;

			private int _fakeMatFrameTar;

			private ds.sys3d.Scene pScene;

			private int mapId;

			private int roomId;

			public CStageMng()
			{
				for (int i = 0; i < m_Chips.Length; i++)
				{
					m_Chips[i] = new CStageChip();
				}
				initValue();
				pScene = null;
			}

			public void initialize(ds.sys3d.Scene scn)
			{
				initValue();
				m_Flag = 0u;
				pScene = scn;
				_fakeMatFlag = 0;
				_fakeMatColorOrg = GX_RGB(31, 31, 31);
				_fakeMatColorNow = GX_RGB(31, 31, 31);
				_fakeMatColorTar = GX_RGB(31, 31, 31);
				_fakeMatFrame = 0;
				_fakeMatFrameTar = 0;
				ushort[] array = new ushort[32];
				MI_CpuFill16(array, _fakeMatColorNow, 64);
				G3X_SetToonTable(array);
			}

			public void terminate()
			{
			}

			public void setRotationY(ushort yRot)
			{
				if (m_Flag != 0 && (m_MngFlag & 8) == 0)
				{
					m_RdrObject.setRotation(0, yRot, 0);
				}
			}

			public ushort getRotationY()
			{
				if (m_Flag == 0)
				{
					return 0;
				}
				m_RdrObject.getRotation(out var _, out var y, out var _);
				return y;
			}

			public void setMaterialAlpha(uint matIdx, uint alpha)
			{
				if (m_Flag != 0)
				{
					m_RdrObject.setMaterialAlpha(matIdx, alpha);
				}
			}

			public void setMaterialAlpha(string pMatname, uint alpha)
			{
				if (m_Flag != 0)
				{
					m_RdrObject.setMaterialAlpha(pMatname, alpha);
				}
			}

			public uint getMaterialAlpha(uint matIdx)
			{
				if (m_Flag == 0)
				{
					return 0u;
				}
				return m_RdrObject.getMaterialAlpha(matIdx);
			}

			public uint getMaterialAlpha(string pMatname)
			{
				if (m_Flag == 0)
				{
					return 0u;
				}
				return m_RdrObject.getMaterialAlpha(pMatname);
			}

			public int getMaterialIdByName(string pMatname)
			{
				if (m_Flag == 0)
				{
					return -1;
				}
				return m_RdrObject.getMaterialIdByName(pMatname);
			}

			public void enableFakeMaterialColor(bool enable, FAKEMATERIAL_TYPE type)
			{
				if (enable)
				{
					m_RdrObject.setPolygonMode(GXPolygonMode.GX_POLYGONMODE_TOON);
					_fakeMatFlag = 1;
					int shading = 0;
					if (type == FAKEMATERIAL_TYPE.TYPE_TOON)
					{
						shading = 0;
					}
					if (type == FAKEMATERIAL_TYPE.TYPE_HIGHLIGHT)
					{
						shading = 0;
					}
					G3X_SetShading(shading);
				}
				else
				{
					m_RdrObject.setPolygonMode(GXPolygonMode.GX_POLYGONMODE_MODULATE);
					_fakeMatFrame = 0;
					_fakeMatFrameTar = 0;
					_fakeMatFlag = 0;
				}
			}

			public void setFakeMaterialColor(int frame, ushort col)
			{
				if (_fakeMatFlag != 0)
				{
					_fakeMatColorTar = col;
					_fakeMatColorOrg = _fakeMatColorNow;
					_fakeMatFrameTar = frame;
					_fakeMatFrame = 0;
					if (_fakeMatFrameTar == 0)
					{
						_fakeMatColorNow = _fakeMatColorTar;
						ushort[] array = new ushort[32];
						MI_CpuFill16(array, _fakeMatColorNow, 64);
						G3X_SetToonTable(array);
					}
				}
			}

			public bool isChangedFakeMaterialColor()
			{
				if (_fakeMatFlag != 0)
				{
					return _fakeMatFrame >= _fakeMatFrameTar;
				}
				return false;
			}

			public void executeFakeMaterialColor()
			{
				if (_fakeMatFlag != 0 && _fakeMatFrame < _fakeMatFrameTar)
				{
					_fakeMatFrame++;
					int numer = 4096 * _fakeMatFrame;
					int denom = 4096 * _fakeMatFrameTar;
					int v = FX_Div(numer, denom);
					sbyte b = (sbyte)ds.getGXR(_fakeMatColorTar);
					sbyte b2 = (sbyte)ds.getGXG(_fakeMatColorTar);
					sbyte b3 = (sbyte)ds.getGXB(_fakeMatColorTar);
					sbyte b4 = (sbyte)ds.getGXR(_fakeMatColorOrg);
					sbyte b5 = (sbyte)ds.getGXG(_fakeMatColorOrg);
					sbyte b6 = (sbyte)ds.getGXB(_fakeMatColorOrg);
					sbyte b7 = (sbyte)(b4 + (sbyte)(FX_Mul(4096 * (b - b4), v) / 4096));
					sbyte b8 = (sbyte)(b5 + (sbyte)(FX_Mul(4096 * (b2 - b5), v) / 4096));
					sbyte b9 = (sbyte)(b6 + (sbyte)(FX_Mul(4096 * (b3 - b6), v) / 4096));
					_fakeMatColorNow = ds.setGXRgb((byte)b7, (byte)b8, (byte)b9);
					ushort[] array = new ushort[32];
					MI_CpuFill16(array, _fakeMatColorNow, 64);
					G3X_SetToonTable(array);
					if (_fakeMatFrame >= _fakeMatFrameTar)
					{
						_fakeMatColorNow = _fakeMatColorTar;
					}
				}
			}

			public bool setCollision()
			{
				bool result = false;
				for (byte b = 0; b < CHIP_NUM; b++)
				{
					if (CStageChip.STATE_TYPE.STATE_TYPE_LOADED == m_Chips[b].getState() && m_Chips[b].isIn(m_FootPos))
					{
						if (b != m_ColChipIdx)
						{
							if (-1 != m_ColChipIdx)
							{
								m_Chips[m_ColChipIdx].m_Collision.rorSetActivity(_b: false);
							}
							m_ColChipIdx = (sbyte)b;
							m_Chips[m_ColChipIdx].m_Collision.rorSetActivity(_b: true);
							result = true;
						}
						break;
					}
				}
				return result;
			}

			public void initValue()
			{
				m_StageType = STAGE_TYPE.STAGE_TYPE_ERROR;
				m_State = STATE_TYPE.STATE_TYPE_ERROR;
				m_MngFlag = 0u;
				m_MidChipIdx = -1;
				m_ColChipIdx = -1;
				m_ChipName = "";
				VEC_Set(m_FootPos, 0, 0, 0);
				VEC_Set(m_PreFootPos, 0, 0, 0);
				m_pScene = null;
				m_pTexData = null;
				m_LoadedChipsNum = 0;
				m_loadingChip = -1;
			}

			public string getChipName()
			{
				sbyte spotX = m_Chips[m_ColChipIdx].m_Chip.SpotX;
				sbyte spotZ = m_Chips[m_ColChipIdx].m_Chip.SpotZ;
				sbyte b = (sbyte)m_Chips[m_ColChipIdx].getName()[2];
				string arg = "";
				sprintf(out arg, "f0%c_%x%x", b, spotX, spotZ);
				return arg;
			}

			public void getFileNo(out sbyte x, out sbyte z)
			{
				m_Chips[m_ColChipIdx].getFileNo(out x, out z);
			}

			public short getChipNo()
			{
				m_Chips[m_ColChipIdx].getWorldSpot(out var x, out var z);
				return (short)(z + m_stgPrf.getChipsNumZ() * x);
			}

			public short getChipNo(string pfilename)
			{
				strtol(pfilename.Substring(pfilename.IndexOf('f') + 1, 2), null, 16);
				sbyte b = (sbyte)strtol(pfilename.Substring(pfilename.IndexOf('_') + 1, 1), null, 16);
				sbyte b2 = (sbyte)strtol(pfilename.Substring(pfilename.IndexOf('_') + 2, 1), null, 16);
				if (b >= 0)
				{
					m_stgPrf.getChipsNumX();
				}
				if (b2 >= 0)
				{
					m_stgPrf.getChipsNumZ();
				}
				return (short)(b2 + m_stgPrf.getChipsNumZ() * b);
			}

			public void getWldMtx(MtxFx43 @out)
			{
				VecFx32 stg_reuse_v = stg_reuse_v0;
				stg_reuse_v.set(0, 0, 0);
				if (m_ColChipIdx >= 0)
				{
					m_Chips[m_ColChipIdx].getPos(stg_reuse_v);
				}
				MTX_Identity43(@out);
				if (MirrorZ)
				{
					// PORT: chip-local z runs the other way; the inverse carries queries into the chip.
					@out._22 = -4096;
				}
				@out._30 = stg_reuse_v.x;
				@out._31 = stg_reuse_v.y;
				@out._32 = stg_reuse_v.z;
			}

			public void getInvWldMtx(MtxFx43 @out)
			{
				getWldMtx(@out);
				MTX_Inverse43(@out, @out);
			}

			public VecFx32 getEdgeMin()
			{
				VecFx32 stg_reuse_v = stg_reuse_v0;
				stg_reuse_v.set(0, 0, 0);
				if (m_Flag == 0)
				{
					return stg_reuse_v;
				}
				if ((m_MngFlag & 8) == 0)
				{
					return stg_reuse_v;
				}
				if (MirrorZ)
				{
					VecFx32 edge = m_stgPrf.getEdge();
					VecFx32 size = m_stgPrf.getSize();
					stg_reuse_v.set(edge.x, edge.y, -(edge.z + size.z));
					return stg_reuse_v;
				}
				return m_stgPrf.getEdge();
			}

			public VecFx32 getEdgeMax()
			{
				VecFx32 stg_reuse_v = stg_reuse_v1;
				VecFx32 stg_reuse_v2 = stg.stg_reuse_v2;
				stg_reuse_v.set(0, 0, 0);
				stg_reuse_v2.set(0, 0, 0);
				if (m_Flag == 0)
				{
					return stg_reuse_v;
				}
				if ((m_MngFlag & 8) == 0)
				{
					return stg_reuse_v;
				}
				stg_reuse_v.copy(m_stgPrf.getEdge());
				stg_reuse_v2.copy(m_stgPrf.getSize());
				stg_reuse_v.x += stg_reuse_v2.x;
				stg_reuse_v.y += stg_reuse_v2.y;
				stg_reuse_v.z += stg_reuse_v2.z;
				if (MirrorZ)
				{
					stg_reuse_v.z = -m_stgPrf.getEdge().z;
				}
				return stg_reuse_v;
			}

			public VecFx32 getSize()
			{
				VecFx32 stg_reuse_v = stg_reuse_v3;
				stg_reuse_v.set(0, 0, 0);
				if (m_Flag == 0)
				{
					return stg_reuse_v;
				}
				if ((m_MngFlag & 8) == 0)
				{
					return stg_reuse_v;
				}
				return m_stgPrf.getSize();
			}

			public bool getLoopEnable()
			{
				if (m_Flag == 0)
				{
					return false;
				}
				if ((m_MngFlag & 8) == 0)
				{
					return false;
				}
				return m_stgPrf.getLoopFlag();
			}

			public VecFx32 getMidChipPos()
			{
				VecFx32 stg_reuse_v = stg_reuse_v0;
				stg_reuse_v.set(0, 0, 0);
				if (m_MidChipIdx < 0 || m_MidChipIdx >= 9)
				{
					return stg_reuse_v;
				}
				m_Chips[m_MidChipIdx].getPos(stg_reuse_v);
				return stg_reuse_v;
			}

			public void setFootPos(VecFx32 pos)
			{
				m_PreFootPos.copy(m_FootPos);
				m_FootPos.copy(pos);
				int num = m_FootPos.x - m_PreFootPos.x;
				int num2 = m_FootPos.z - m_PreFootPos.z;
				VecFx32 size = getSize();
				if (size.x == 0 && size.y == 0 && size.z == 0)
				{
					size = getSize();
				}
				sbyte b = 0;
				sbyte b2 = 0;
				if (num >= FX_Div(size.x, 8192))
				{
					b = 1;
				}
				if (num <= -FX_Div(size.x, 8192))
				{
					b = -1;
				}
				if (num2 >= FX_Div(size.z, 8192))
				{
					b2 = 1;
				}
				if (num2 <= -FX_Div(size.z, 8192))
				{
					b2 = -1;
				}
				if (b != 0 || b2 != 0)
				{
					for (sbyte b3 = 0; b3 < CHIP_NUM; b3++)
					{
						switch (b)
						{
						case 1:
							m_Chips[b3].m_Chip.Pos.x += size.x;
							break;
						case -1:
							m_Chips[b3].m_Chip.Pos.x -= size.x;
							break;
						}
						switch (b2)
						{
						case 1:
							m_Chips[b3].m_Chip.Pos.z += size.z;
							break;
						case -1:
							m_Chips[b3].m_Chip.Pos.z -= size.z;
							break;
						}
						if (m_Chips[b3].getState() == CStageChip.STATE_TYPE.STATE_TYPE_LOADED)
						{
							m_Chips[b3].m_RdrObject.setPosition(m_Chips[b3].m_Chip.Pos);
						}
					}
				}
				m_Chips[m_MidChipIdx].getRelativeSpot(m_FootPos, out var x, out var z);
				if (x != 0 || z != 0)
				{
					m_stgLoadState.move(x, z);
					m_MngFlag |= 16u;
					if (m_State == STATE_TYPE.STATE_TYPE_NORMAL)
					{
						m_State = STATE_TYPE.STATE_TYPE_LOAD_WAIT;
					}
					m_MidChipIdx = -1;
					for (sbyte b4 = 0; b4 < CHIP_NUM; b4++)
					{
						if (m_Chips[b4].getState() != CStageChip.STATE_TYPE.STATE_TYPE_UNLOAD)
						{
							m_Chips[b4].moveSpot((sbyte)(-x), (sbyte)(-z));
							if (CStageChip.STATE_TYPE.STATE_TYPE_LOADED == m_Chips[b4].getState())
							{
								m_Chips[b4].getSpot(out var x2, out var z2);
								if (x2 == 0 && z2 == 0)
								{
									m_MidChipIdx = b4;
								}
								m_LoadedChipsNum++;
							}
						}
					}
					if (-1 == m_MidChipIdx)
					{
						loadAllChips();
					}
					sort();
				}
				else
				{
					m_MngFlag &= 4294967279u;
				}
				if (setCollision())
				{
					m_MngFlag |= 16u;
				}
				else
				{
					m_MngFlag &= 4294967279u;
				}
			}

			public void execute()
			{
				switch (m_StageType)
				{
				case STAGE_TYPE.STAGE_TYPE_FIELD01:
				case STAGE_TYPE.STAGE_TYPE_FIELD02:
				case STAGE_TYPE.STAGE_TYPE_FIELD03:
				case STAGE_TYPE.STAGE_TYPE_FIELD04:
				{
					switch (m_State)
					{
					case STATE_TYPE.STATE_TYPE_LOAD_WAIT:
					{
						sbyte x = 0;
						sbyte z = 0;
						sbyte x2 = 0;
						sbyte z2 = 0;
						if (!m_stgLoadState.getNecessaryChipNo(ref x, ref z))
						{
							sort();
							m_State = STATE_TYPE.STATE_TYPE_NORMAL;
						}
						else
						{
							if (!m_stgLoadState.getUnnecessaryChipNo(ref x2, ref z2))
							{
								break;
							}
							for (sbyte b = 0; b < CHIP_NUM; b++)
							{
								if (x2 == m_Chips[b].m_Chip.RelativeX && z2 == m_Chips[b].m_Chip.RelativeZ)
								{
									m_Chips[b].cleanup();
									m_stgPrf.getChipData(m_Chips[m_MidChipIdx].m_Chip, x, z, m_Chips[b].m_Chip);
									m_Chips[b].prepareSetupByStream(pScene, m_MdlTex);
									m_stgLoadState.reportUnloadedChip(x2, z2);
									m_loadingChip = b;
									m_State = STATE_TYPE.STATE_TYPE_PRELOAD;
									break;
								}
							}
						}
						break;
					}
					case STATE_TYPE.STATE_TYPE_PRELOAD:
						FS_ChangeDir(m_stagePath);
						m_Chips[m_loadingChip].startSetupByStream(m_Sarc);
						FS_ChangeDir("/");
						m_State = STATE_TYPE.STATE_TYPE_LOADING;
						break;
					case STATE_TYPE.STATE_TYPE_LOADING:
						m_Chips[m_loadingChip].m_pSarc.uncompressReadFile(COMPSIZE * 1024);
						if (!m_Sarc.isReadFile())
						{
							m_Chips[m_loadingChip].setup(m_StageType);
							m_Chips[m_loadingChip].m_AnimSet.setFrame(m_Chips[m_MidChipIdx].m_AnimSet.getFrame(ds.sys3d.CAnimSet.enTYPE.enTYPE_ITA), ds.sys3d.CAnimSet.enTYPE.enTYPE_END);
							m_stgLoadState.reportLoadedChip(m_Chips[m_loadingChip].m_Chip.RelativeX, m_Chips[m_loadingChip].m_Chip.RelativeZ);
							m_loadingChip = -1;
							m_State = STATE_TYPE.STATE_TYPE_LOAD_WAIT;
						}
						break;
					}
					for (byte b2 = 0; b2 < CHIP_NUM; b2++)
					{
						m_Chips[b2].execute();
					}
					break;
				}
				default:
					if (m_Flag != 0)
					{
						m_AnimSet.next();
						if (m_MotSet != null)
						{
							m_MotSet.next();
						}
						executeFakeMaterialColor();
					}
					break;
				}
			}

			public int setStage(string name)
			{
				string arg = "";
				uint num = 0u;
				sbyte b = 0;
				string arg2 = "";
				char[] array = new char[2];
				int arg3 = 16;
				sbyte b2 = -1;
				sbyte b3 = -1;
				sbyte b4 = 1;
				ds.CHeap.setID_app(20);
				NNS_FndGetTotalFreeSizeForExpHeap(ds.CHeap.getHeapHandle());
				if (ds.CVram.getInstance().getBankForTex() != GX_GetBankForTex())
				{
					ds.CVram.getInstance().setupBankForTex();
				}
				if (ds.CVram.getInstance().getBankForPltt() != GX_GetBankForTexPltt())
				{
					ds.CVram.getInstance().setupBankForPltt();
				}
				if (name.Length < 3)
				{
					mapId = 0;
				}
				else
				{
					mapId = (byte)atoi(name.Substring(1, 2));
				}
				int num2 = 0;
				while (true)
				{
					if (name[num2] != 0)
					{
						if (name[num2] == 'f')
						{
							m_Flag = 1u;
							m_MngFlag = 8u;
							m_stgLoadState.initialize();
							for (byte b5 = 0; b5 < CHIP_NUM; b5++)
							{
								m_Chips[b5].initialize();
							}
							NNS_FndGetTotalFreeSizeForExpHeap(ds.CHeap.getHeapHandle());
							num2++;
							b = (sbyte)strtol(name.Substring(num2, 2), null, 16);
							sprintf(out arg, "f%02d.ntxp", b);
							m_StageType = (STAGE_TYPE)b;
							if (m_StageType == (STAGE_TYPE)0 && OpenFF.Client.GameProfile.IsFf4)
							{
								// PORT: a field's type is its number, and every field behaviour here
								// (chip streaming, collision, the loop) switches on FIELD01..04. FF3's
								// fields are f01..f03; FF4's overworld is f00, which no case matched, so
								// it drew one chip and collided with nothing. Its number stays in the
								// file names above; its type is FF3's first field.
								m_StageType = STAGE_TYPE.STAGE_TYPE_FIELD01;
							}
							MirrorZ = OpenFF.Client.FieldMirror.Active(m_StageType);
							m_State = STATE_TYPE.STATE_TYPE_NORMAL;
							sprintf(out m_stagePath, "/MAP/FIELD/F%02d", b);
							FS_ChangeDir(m_stagePath);
							num = ds.g_File.getSize(arg);
							if (num != 0)
							{
								m_pTexData = ds.CHeap.alloc_app(num);
								_ = m_pTexData;
								ds.g_File.load(m_pTexData, arg);
								m_MdlTex.setup(m_pTexData, tdl: false);
								m_MdlTex.reqReleaseResource();
							}
							sprintf(out arg2, "f%02d.stgprf", b);
							m_stgPrf.setup(arg2);
							num2 = 0;
							while (name[num2] != 0 && name[num2] != '_')
							{
								num2++;
								if (num2 >= name.Length)
								{
									break;
								}
							}
							num2++;
							array[0] = name[num2];
							array[1] = '\0';
							b2 = (sbyte)strtol(new string(array), null, arg3);
							num2++;
							array[0] = name[num2];
							array[1] = '\0';
							b3 = (sbyte)strtol(new string(array), null, arg3);
							m_MidChipIdx = 0;
							m_stgPrf.getMidChipData(b2, b3, m_Chips[m_MidChipIdx].m_Chip);
							m_Chips[m_MidChipIdx].strongSetup(pScene, m_MdlTex, m_StageType);
							for (sbyte b6 = -1; b6 <= 1; b6++)
							{
								for (sbyte b7 = -1; b7 <= 1; b7++)
								{
									m_stgLoadState.reportLoadedChip(b7, b6);
									if (b6 != 0 || b7 != 0)
									{
										m_stgPrf.getChipData(m_Chips[m_MidChipIdx].m_Chip, b7, b6, m_Chips[b4].m_Chip);
										m_Chips[b4].strongSetup(pScene, m_MdlTex, m_StageType);
										m_Chips[b4].m_AnimSet.setFrame(m_Chips[m_MidChipIdx].m_AnimSet.getFrame(ds.sys3d.CAnimSet.enTYPE.enTYPE_ITA), ds.sys3d.CAnimSet.enTYPE.enTYPE_END);
										b4++;
									}
								}
							}
							m_ColChipIdx = 0;
							m_Chips[m_ColChipIdx].m_Collision.rorSetActivity(_b: true);
							m_LoadedChipsNum = 9;
							sort();
							FS_ChangeDir("/");
							break;
						}
						num2++;
						if (num2 < name.Length)
						{
							continue;
						}
					}
					if (name.Length < 6)
					{
						roomId = 0;
					}
					else
					{
						roomId = (byte)atoi(name.Substring(4, 2));
					}
					ds.fs.enFDL_FILETYPE type = ds.fs.enFDL_FILETYPE.enFDL_FILETYPE_COMPRESS;
					TexDivideLoader.getSingleton().tdlForceLoad();
					OS_GetTick();
					MirrorZ = false;
					switch (name[0])
					{
					case 'd':
						m_StageType = STAGE_TYPE.STAGE_TYPE_DUNGEON;
						FS_ChangeDir("/MAP/DUNGEON");
						break;
					case 't':
						m_StageType = STAGE_TYPE.STAGE_TYPE_TOWN;
						FS_ChangeDir("/MAP/TOWN");
						break;
					case 'b':
						m_StageType = STAGE_TYPE.STAGE_TYPE_BATTLE;
						FS_ChangeDir("/MAP/BATTLE");
						break;
					case 's':
						m_StageType = STAGE_TYPE.STAGE_TYPE_SHOP;
						FS_ChangeDir("/MAP/SHOP/");
						break;
					}
					sprintf(out var arg4, "./MODEL/%s.nmdp.lz", name);
					CurrentName = name;
					OpenFF.Client.Log.Write(OpenFF.Client.LogChannel.File, "stage: " + name + " model " + ds.g_File.getSize(arg4) + " bytes, animation "
						+ ds.g_File.getSize("./ANIMATION/" + name + ".namp.lz") + ", collision " + ds.g_File.getSize("./COLLISION/" + name + "_col.mcl.lz"));
					if (ds.g_File.getSize(arg4) != 0)
					{
						MdlData.setup(arg4, type);
						m_ModelSet.setup(MdlData.getAddr());
						m_ModelSet.releaseTexResource();
						m_RdrObject.setup(m_ModelSet.getMdlResource());
						m_RdrObject.setDropShadow(flag: true, m_ModelSet.getMdlResource());
						if (name[0] == 'b')
						{
							m_RdrObject.setPriority(-1);
						}
						if (strcmp(name, "d26_10") == 0)
						{
							m_RdrObject.setPriority(-1);
						}
					}
					sprintf(out arg4, "./ANIMATION/%s.namp.lz", name);
					if (ds.g_File.getSize(arg4) != 0 && OpenFF.Client.Options.Get("noanim") == null)
					{
						AnmData.setup(arg4, type);
						m_AnimSet.setup(AnmData.getAddr(), m_ModelSet.getMdlResource(), null);
						m_AnimSet.addRenderObject(m_RdrObject.getRenderObject());
						m_AnimSet.setLoop(b: true, ds.sys3d.CAnimSet.enTYPE.enTYPE_END);
						m_AnimSet.start(0, ds.sys3d.CAnimSet.enTYPE.enTYPE_END);
					}
					pScene.addRenderObject(m_RdrObject, 0);
					sprintf(out arg4, "./COLLISION/%s_col.mcl.lz", name);
					if (ds.g_File.getSize(arg4) != 0)
					{
						ColData.setup(arg4, type);
						ColData.getAddr<mcl.CMapCollision>().initialize();
						m_Collision.rorAppend(ColData.getAddr<mcl.CMapCollision>());
					}
					FS_ChangeDir("/");
					m_Flag = 1u;
					break;
				}
				ds.CHeap.setID_app(0);
				return 0;
			}

			public void delStage()
			{
				if (m_Flag == 0)
				{
					return;
				}
				if ((m_MngFlag & 8) != 0)
				{
					for (byte b = 0; b < CHIP_NUM; b++)
					{
						pScene.removeRenderObject(m_Chips[b].m_RdrObject);
						m_Chips[b].cleanup();
						m_Chips[b].terminate();
					}
					m_MdlTex.cleanup();
					if (m_pTexData != null)
					{
						ds.CHeap.free_app(m_pTexData);
					}
					m_stgPrf.release();
					initValue();
				}
				else
				{
					cleanupMapMotions();
					m_RdrObject.cleanup();
					pScene.removeRenderObject(m_RdrObject);
					m_Collision.rorRemove();
					ColData.cleanup();
					m_ModelSet.cleanup();
					MdlData.cleanup();
					m_AnimSet.cleanup();
					AnmData.cleanup();
					LgtData.cleanup();
					BoxData.cleanup();
					m_Flag = 0u;
				}
			}

			public void loadAllChips()
			{
				FS_ChangeDir(m_stagePath);
				for (byte b = 0; b < CHIP_NUM; b++)
				{
					m_Chips[b].cleanup();
				}
				string arg = "";
				m_stgPrf.getSpot(m_FootPos, out var spotX, out var spotZ);
				sprintf(out arg, "./f01_%X%X", spotX, spotZ);
				m_MidChipIdx = 0;
				m_stgPrf.getMidChipData(spotX, spotZ, m_Chips[m_MidChipIdx].m_Chip);
				m_Chips[m_MidChipIdx].strongSetup(pScene, m_MdlTex, m_StageType);
				m_stgLoadState.initialize();
				sbyte b2 = 1;
				for (sbyte b3 = -1; b3 <= 1; b3++)
				{
					for (sbyte b4 = -1; b4 <= 1; b4++)
					{
						m_stgLoadState.reportLoadedChip(b4, b3);
						if (b3 != 0 || b4 != 0)
						{
							m_stgPrf.getChipData(m_Chips[m_MidChipIdx].m_Chip, b4, b3, m_Chips[b2].m_Chip);
							m_Chips[b2].strongSetup(pScene, m_MdlTex, m_StageType);
							m_Chips[b2].m_AnimSet.setFrame(m_Chips[m_MidChipIdx].m_AnimSet.getFrame(ds.sys3d.CAnimSet.enTYPE.enTYPE_ITA), ds.sys3d.CAnimSet.enTYPE.enTYPE_END);
							b2++;
						}
					}
				}
				m_LoadedChipsNum = 9;
				sort();
				m_State = STATE_TYPE.STATE_TYPE_LOAD_WAIT;
				FS_ChangeDir("/");
			}

			public void sort()
			{
				sbyte[][] array = new sbyte[10][]
				{
					new sbyte[2] { 1, -1 },
					new sbyte[2] { -1, -1 },
					new sbyte[2] { 0, -1 },
					new sbyte[2] { 1, 0 },
					new sbyte[2] { -1, 0 },
					null,
					null,
					null,
					null,
					null
				};
				sbyte[] array2 = new sbyte[2];
				array[5] = array2;
				array[6] = new sbyte[2] { 1, 1 };
				array[7] = new sbyte[2] { -1, 1 };
				array[8] = new sbyte[2] { 0, 1 };
				sbyte[] array3 = new sbyte[2];
				array[9] = array3;
				sbyte[][] array4 = array;
				for (byte b = 0; b < CHIP_NUM; b++)
				{
					if (m_Chips[b].m_State == CStageChip.STATE_TYPE.STATE_TYPE_LOADED)
					{
						pScene.removeRenderObject(m_Chips[b].m_RdrObject);
					}
				}
				for (byte b2 = 0; b2 < CHIP_NUM; b2++)
				{
					for (byte b3 = 0; b3 < CHIP_NUM; b3++)
					{
						if (m_Chips[b3].m_State == CStageChip.STATE_TYPE.STATE_TYPE_LOADED && m_Chips[b3].m_Chip.RelativeX == array4[b2][0] && m_Chips[b3].m_Chip.RelativeZ == array4[b2][1])
						{
							pScene.addRenderObject(m_Chips[b3].m_RdrObject, 0);
							break;
						}
					}
				}
			}

			public void Patch(string name, bool flag)
			{
			}

			public uint getEnableMesh()
			{
				if (!m_BoxTest.isEnable())
				{
					return 0u;
				}
				return m_BoxTest.getEnableCount();
			}

			public void setFrameRate(int fps)
			{
				m_AnimSet.setFrameRate(fps, ds.sys3d.CAnimSet.enTYPE.enTYPE_END);
			}

			public uint getMeshMax()
			{
				if (!m_BoxTest.isEnable())
				{
					return 0u;
				}
				return m_BoxTest.getCount();
			}

			public sbyte getLoadedChipsNum()
			{
				sbyte b = 0;
				for (byte b2 = 0; b2 < CHIP_NUM; b2++)
				{
					if (CStageChip.STATE_TYPE.STATE_TYPE_LOADED == m_Chips[b2].getState())
					{
						b++;
					}
				}
				return b;
			}

			public sbyte getLoadingChipsNum()
			{
				sbyte b = 0;
				for (byte b2 = 0; b2 < CHIP_NUM; b2++)
				{
					if (CStageChip.STATE_TYPE.STATE_TYPE_LOADING == m_Chips[b2].getState())
					{
						b++;
					}
				}
				return b;
			}

			public sbyte getPreloadChipsNum()
			{
				sbyte b = 0;
				for (byte b2 = 0; b2 < CHIP_NUM; b2++)
				{
					if (CStageChip.STATE_TYPE.STATE_TYPE_PRELOAD == m_Chips[b2].getState())
					{
						b++;
					}
				}
				return b;
			}

			public sbyte getUnloadChipsNum()
			{
				sbyte b = 0;
				for (byte b2 = 0; b2 < CHIP_NUM; b2++)
				{
					if (m_Chips[b2].getState() == CStageChip.STATE_TYPE.STATE_TYPE_UNLOAD)
					{
						b++;
					}
				}
				return b;
			}

			public sbyte getWaitToSetupChipsNum()
			{
				sbyte b = 0;
				for (byte b2 = 0; b2 < CHIP_NUM; b2++)
				{
					if (CStageChip.STATE_TYPE.STATE_TYPE_WAIT_SETUP == m_Chips[b2].getState())
					{
						b++;
					}
				}
				return b;
			}

			public void setAlpha(int alpha)
			{
				if ((m_MngFlag & 8) == 0)
				{
					m_RdrObject.setAlpha(alpha);
				}
			}

			public void setHidden(bool flag)
			{
				switch (m_StageType)
				{
				case STAGE_TYPE.STAGE_TYPE_FIELD01:
				case STAGE_TYPE.STAGE_TYPE_FIELD02:
				case STAGE_TYPE.STAGE_TYPE_FIELD03:
				case STAGE_TYPE.STAGE_TYPE_FIELD04:
				{
					for (int i = 0; i < CHIP_NUM; i++)
					{
						if (CStageChip.STATE_TYPE.STATE_TYPE_LOADED == m_Chips[i].getState())
						{
							m_Chips[i].m_RdrObject.setHidden(flag);
						}
					}
					break;
				}
				default:
					m_RdrObject.setHidden(flag);
					break;
				}
			}

			public bool isHidden()
			{
				switch (m_StageType)
				{
				case STAGE_TYPE.STAGE_TYPE_FIELD01:
				case STAGE_TYPE.STAGE_TYPE_FIELD02:
				case STAGE_TYPE.STAGE_TYPE_FIELD03:
				case STAGE_TYPE.STAGE_TYPE_FIELD04:
				{
					for (int i = 0; i < CHIP_NUM; i++)
					{
						if (CStageChip.STATE_TYPE.STATE_TYPE_LOADED == m_Chips[i].getState())
						{
							return m_Chips[i].m_RdrObject.isHidden();
						}
					}
					return false;
				}
				default:
					return m_RdrObject.isHidden();
				}
			}

			public int getMapId()
			{
				return mapId;
			}

			public int getRoomId()
			{
				switch (m_StageType)
				{
				case STAGE_TYPE.STAGE_TYPE_FIELD01:
				case STAGE_TYPE.STAGE_TYPE_FIELD02:
				case STAGE_TYPE.STAGE_TYPE_FIELD03:
				case STAGE_TYPE.STAGE_TYPE_FIELD04:
					roomId = m_Chips[m_ColChipIdx].m_Chip.SpotZ + 16 * m_Chips[m_ColChipIdx].m_Chip.SpotX;
					return roomId;
				default:
					return roomId;
				}
			}

			public ds.sys3d.CLightObject getLight()
			{
				return null;
			}

			public void initFootPos(VecFx32 pos)
			{
				m_FootPos.copy(pos);
				setFootPos(pos);
			}

			public bool isChipChanged()
			{
				return (m_MngFlag & 0x10) != 0;
			}

			public STAGE_TYPE getStageType()
			{
				return m_StageType;
			}
		}

		public enum STAGE_TYPE
		{
			STAGE_TYPE_ERROR = -1,
			STAGE_TYPE_FIELD01 = 1,
			STAGE_TYPE_FIELD02 = 2,
			STAGE_TYPE_FIELD03 = 3,
			STAGE_TYPE_FIELD04 = 4,
			STAGE_TYPE_TOWN = 5,
			STAGE_TYPE_SHOP = 6,
			STAGE_TYPE_DUNGEON = 7,
			STAGE_TYPE_BATTLE = 8,
			STAGE_TYPE_ALL = 9
		}

		public class CStageProfile
		{
			public class StageProfileData
			{
				public byte stageNo;

				public byte loopFlag;

				public byte centerChipX;

				public byte centerChipZ;

				public sbyte outChipX;

				public sbyte outChipZ;

				public sbyte[] seaChipX = new sbyte[4];

				public sbyte[] seaChipZ = new sbyte[4];

				public byte numChipsX;

				public byte numChipsZ;

				public byte[] pad = new byte[4];

				public int szChipX;

				public int szChipZ;

				public sbyte[] m_pSeaNoList;

				public static explicit operator StageProfileData(Array src)
				{
					StageProfileData stageProfileData = new StageProfileData();
					ArrayReader arrayReader = new ArrayReader(src);
					stageProfileData.stageNo = arrayReader.readByte();
					stageProfileData.loopFlag = arrayReader.readByte();
					stageProfileData.centerChipX = arrayReader.readByte();
					stageProfileData.centerChipZ = arrayReader.readByte();
					stageProfileData.outChipX = arrayReader.readSByte();
					stageProfileData.outChipZ = arrayReader.readSByte();
					arrayReader.read(stageProfileData.seaChipX, 0, 4);
					arrayReader.read(stageProfileData.seaChipZ, 0, 4);
					stageProfileData.numChipsX = arrayReader.readByte();
					stageProfileData.numChipsZ = arrayReader.readByte();
					arrayReader.read(stageProfileData.pad, 0, 4);
					stageProfileData.szChipX = arrayReader.readInt32();
					stageProfileData.szChipZ = arrayReader.readInt32();
					stageProfileData.m_pSeaNoList = new sbyte[arrayReader.rest()];
					arrayReader.read(stageProfileData.m_pSeaNoList, 0, stageProfileData.m_pSeaNoList.Length);
					arrayReader.dispose();
					return stageProfileData;
				}
			}

			public static int OUT_X_MIN = 1;

			public static int OUT_X_MAX = 2;

			public static int OUT_Z_MIN = 4;

			public static int OUT_Z_MAX = 8;

			private StageProfileData m_pData;

			private sbyte[] m_pSeaNoList;

			private VecFx32 m_stageEdge = new VecFx32();

			private VecFx32 m_stageSize = new VecFx32();

			public CStageProfile()
			{
				m_pData = null;
			}

			public void setup(string pfilename)
			{
				if (m_pData == null)
				{
					Array array = null;
					uint size = ds.g_File.getSize(pfilename);
					_ = 0;
					array = ds.CHeap.alloc_app(size);
					ds.g_File.load(array, pfilename);
					m_pData = (StageProfileData)array;
					m_pSeaNoList = m_pData.m_pSeaNoList;
					m_stageSize.x = FX_Mul(m_pData.szChipX, 4096 * m_pData.numChipsX);
					m_stageSize.z = FX_Mul(m_pData.szChipZ, 4096 * m_pData.numChipsZ);
					m_stageEdge.x = -FX_Div(m_pData.szChipX, 8192) + -FX_Mul(m_pData.szChipX, 4096 * m_pData.centerChipX);
					m_stageEdge.z = -FX_Div(m_pData.szChipZ, 8192) + -FX_Mul(m_pData.szChipZ, 4096 * m_pData.centerChipZ);
					m_stageEdge.y = 0;
				}
			}

			public void release()
			{
				if (m_pData != null)
				{
					ds.CHeap.free_app(m_pData);
					m_pData = null;
				}
			}

			public void getChipData(ChipData centerChip, sbyte relativeX, sbyte relativeZ, ChipData @out)
			{
				@out.SpotX = (sbyte)(centerChip.SpotX + relativeX);
				@out.SpotZ = (sbyte)(centerChip.SpotZ + relativeZ);
				@out.RelativeX = relativeX;
				@out.RelativeZ = relativeZ;
				@out.Size.x = m_pData.szChipX;
				@out.Size.y = 0;
				@out.Size.z = m_pData.szChipZ;
				@out.Pos.x = centerChip.Pos.x + @out.Size.x * relativeX;
				@out.Pos.y = 0;
				@out.Pos.z = centerChip.Pos.z + @out.Size.z * (MirrorZ ? -relativeZ : relativeZ);
				if (@out.SpotX < 0 || @out.SpotZ < 0 || @out.SpotX >= m_pData.numChipsX || @out.SpotZ >= m_pData.numChipsZ)
				{
					if (1 == m_pData.loopFlag)
					{
						if (@out.SpotX < 0)
						{
							@out.SpotX = (sbyte)(m_pData.numChipsX + @out.SpotX);
						}
						if (@out.SpotX >= m_pData.numChipsX)
						{
							@out.SpotX = (sbyte)(@out.SpotX - m_pData.numChipsX);
						}
						if (@out.SpotZ < 0)
						{
							@out.SpotZ = (sbyte)(m_pData.numChipsZ + @out.SpotZ);
						}
						if (@out.SpotZ >= m_pData.numChipsZ)
						{
							@out.SpotZ = (sbyte)(@out.SpotZ - m_pData.numChipsZ);
						}
						sbyte b = m_pSeaNoList[@out.SpotX + @out.SpotZ * m_pData.numChipsX];
						if (b == 0)
						{
							@out.fnoX = @out.SpotX;
							@out.fnoZ = @out.SpotZ;
						}
						else
						{
							@out.fnoX = m_pData.seaChipX[b - 1];
							@out.fnoZ = m_pData.seaChipZ[b - 1];
						}
					}
					else
					{
						@out.fnoX = m_pData.outChipX;
						@out.fnoZ = m_pData.outChipZ;
					}
				}
				else
				{
					sbyte b2 = m_pSeaNoList[@out.SpotX + @out.SpotZ * m_pData.numChipsX];
					if (b2 == 0)
					{
						@out.fnoX = @out.SpotX;
						@out.fnoZ = @out.SpotZ;
					}
					else
					{
						@out.fnoX = m_pData.seaChipX[b2 - 1];
						@out.fnoZ = m_pData.seaChipZ[b2 - 1];
					}
				}
				sprintf(out @out.Name, "f%02d_%x%x", m_pData.stageNo, @out.fnoX, @out.fnoZ);
			}

			public void getMidChipData(sbyte SpotX, sbyte SpotZ, ChipData @out)
			{
				@out.SpotX = SpotX;
				@out.SpotZ = SpotZ;
				if (@out.SpotX >= 0 && m_pData.numChipsX > @out.SpotX && @out.SpotZ >= 0)
				{
					_ = m_pData.numChipsZ;
					_ = @out.SpotZ;
				}
				@out.RelativeX = 0;
				@out.RelativeZ = 0;
				@out.Size.x = m_pData.szChipX;
				@out.Size.y = 0;
				@out.Size.z = m_pData.szChipZ;
				@out.Pos.x = @out.Size.x * (@out.SpotX - m_pData.centerChipX);
				@out.Pos.y = 0;
				@out.Pos.z = @out.Size.z * (@out.SpotZ - m_pData.centerChipZ);
				if (isEdgeOfWorld(@out.Pos, out var outFlag))
				{
					if (1 == m_pData.loopFlag)
					{
						if ((OUT_X_MIN & outFlag) != 0)
						{
							@out.fnoX = (sbyte)(@out.SpotX + m_pData.numChipsX);
						}
						if ((OUT_X_MAX & outFlag) != 0)
						{
							@out.fnoX = (sbyte)(@out.SpotX - m_pData.numChipsX);
						}
						if ((OUT_Z_MIN & outFlag) != 0)
						{
							@out.fnoZ = (sbyte)(@out.SpotZ + m_pData.numChipsZ);
						}
						if ((OUT_Z_MAX & outFlag) != 0)
						{
							@out.fnoZ = (sbyte)(@out.SpotZ - m_pData.numChipsZ);
						}
					}
					else
					{
						@out.fnoX = m_pData.outChipX;
						@out.fnoZ = m_pData.outChipZ;
					}
				}
				else
				{
					sbyte b = m_pSeaNoList[@out.SpotX + @out.SpotZ * m_pData.numChipsX];
					if (b == 0)
					{
						@out.fnoX = @out.SpotX;
						@out.fnoZ = @out.SpotZ;
					}
					else
					{
						@out.fnoX = m_pData.seaChipX[b - 1];
						@out.fnoZ = m_pData.seaChipZ[b - 1];
					}
				}
				sprintf(out @out.Name, "f%02d_%x%x", m_pData.stageNo, @out.fnoX, @out.fnoZ);
				if (MirrorZ)
				{
					// PORT: after the edge test, which knows FF3's layout; the world loop is symmetric.
					@out.Pos.z = -@out.Pos.z;
				}
			}

			public bool getLoopFlag()
			{
				if (m_pData == null)
				{
					return false;
				}
				return m_pData.loopFlag != 0;
			}

			public bool isEdgeOfWorld(VecFx32 pos, out byte outFlag)
			{
				VecFx32 vecFx = new VecFx32(m_stageEdge.x + m_stageSize.x, 0, m_stageEdge.z + m_stageSize.z);
				VecFx32 vecFx2 = new VecFx32(FX_Div(m_pData.szChipX, 8192), 0, FX_Div(m_pData.szChipZ, 8192));
				outFlag = 0;
				if (pos.x < m_stageEdge.x + vecFx2.x)
				{
					outFlag |= (byte)OUT_X_MIN;
				}
				if (pos.z < m_stageEdge.z + vecFx2.z)
				{
					outFlag |= (byte)OUT_Z_MIN;
				}
				if (pos.x > vecFx.x - vecFx2.x)
				{
					outFlag |= (byte)OUT_X_MAX;
				}
				if (pos.z > vecFx.z - vecFx2.z)
				{
					outFlag |= (byte)OUT_Z_MAX;
				}
				return (0xFF & outFlag) != 0;
			}

			public void getSpot(VecFx32 pos, out sbyte spotX, out sbyte spotZ)
			{
				VecFx32 edge = getEdge();
				int num = pos.x - edge.x;
				int num2 = (MirrorZ ? -pos.z : pos.z) - edge.z;
				VecFx32 size = getSize();
				if (num < 0)
				{
					num += size.x;
				}
				if (num2 < 0)
				{
					num2 += size.z;
				}
				num += m_pData.szChipX;
				num2 += m_pData.szChipZ;
				if (num > size.x)
				{
					num -= size.x;
				}
				if (num2 > size.z)
				{
					num2 -= size.z;
				}
				spotX = (sbyte)((sbyte)(FX_Div(num, m_pData.szChipX) >> 12) - 1);
				spotZ = (sbyte)((sbyte)(FX_Div(num2, m_pData.szChipZ) >> 12) - 1);
			}

			public VecFx32 getEdge()
			{
				return m_stageEdge;
			}

			public VecFx32 getSize()
			{
				return m_stageSize;
			}

			public byte getChipsNumX()
			{
				return m_pData.numChipsX;
			}

			public byte getChipsNumZ()
			{
				return m_pData.numChipsZ;
			}
		}

		public class ChipData
		{
			public string Name;

			public sbyte fnoX;

			public sbyte fnoZ;

			public sbyte SpotX;

			public sbyte SpotZ;

			public sbyte RelativeX;

			public sbyte RelativeZ;

			public VecFx32 Pos = new VecFx32();

			public VecFx32 Size = new VecFx32();

			public uint Flag;
		}

		public const STAGE_TYPE STAGE_TYPE_ERROR = STAGE_TYPE.STAGE_TYPE_ERROR;

		public const STAGE_TYPE STAGE_TYPE_FIELD01 = STAGE_TYPE.STAGE_TYPE_FIELD01;

		public const STAGE_TYPE STAGE_TYPE_FIELD02 = STAGE_TYPE.STAGE_TYPE_FIELD02;

		public const STAGE_TYPE STAGE_TYPE_FIELD03 = STAGE_TYPE.STAGE_TYPE_FIELD03;

		public const STAGE_TYPE STAGE_TYPE_FIELD04 = STAGE_TYPE.STAGE_TYPE_FIELD04;

		public const STAGE_TYPE STAGE_TYPE_TOWN = STAGE_TYPE.STAGE_TYPE_TOWN;

		public const STAGE_TYPE STAGE_TYPE_SHOP = STAGE_TYPE.STAGE_TYPE_SHOP;

		public const STAGE_TYPE STAGE_TYPE_DUNGEON = STAGE_TYPE.STAGE_TYPE_DUNGEON;

		public const STAGE_TYPE STAGE_TYPE_BATTLE = STAGE_TYPE.STAGE_TYPE_BATTLE;

		public const STAGE_TYPE STAGE_TYPE_ALL = STAGE_TYPE.STAGE_TYPE_ALL;

		private static uint COMPSIZE = 16u;

		private static VecFx32 stg_reuse_v0 = new VecFx32();

		private static VecFx32 stg_reuse_v1 = new VecFx32();

		private static VecFx32 stg_reuse_v2 = new VecFx32();

		private static VecFx32 stg_reuse_v3 = new VecFx32();
	}
}
