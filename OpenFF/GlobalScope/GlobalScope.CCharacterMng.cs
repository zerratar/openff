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
	public class CCharacterMng
	{
		public enum PRI_SCENE
		{
			PRI_SCENE_FIRST,
			PRI_SCENE_SECOND,
			PRI_SCENE_MAX
		}

		public enum LOAD_FLAG
		{
			LOAD_OBJECT = 1,
			LOAD_ORGTEX = 2,
			LOAD_MOTION = 4,
			LOAD_TEX = 8,
			LOAD_TEXEL = 0x10,
			LOAD_PLTT = 0x20
		}

		public class CharacterData
		{
			public ds.sys3d.CModelSet modelSet = new ds.sys3d.CModelSet();

			public ds.sys3d.CAnimSet animSet = new ds.sys3d.CAnimSet();

			public ds.sys3d.CMotSet motSet = new ds.sys3d.CMotSet();

			public ds.sys3d.CRenderObject RdrObject = new ds.sys3d.CRenderObject();

			public ds.sys3d.CShadowObject ShadowObject = new ds.sys3d.CShadowObject();

			public ds.sys3d.CLightObject LightObject = new ds.sys3d.CLightObject();

			public ds.sys3d.Light FlashLight = new ds.sys3d.Light();

			public uint flag;

			public uint shadow_type;

			// PORT (FF4): a character whose shadow the script keeps off (a scene cast until
			// ce_ShadowVisiblity says otherwise). Survives the asynchronous setup and setHidden(false),
			// which both turn the shadow on. FF3 never sets it: its shadows behave as they did.
			public bool shadowOff;

			// PORT (FF4): the joint the shadow follows (ce_ShadowSetting's "kosi"); reserved on the
			// render object once the model is set up.
			public string shadowJoint;

			public uint loadFlag;

			public sbyte objDataIdx;

			public sbyte[] motDataIdx = new sbyte[8];

			public byte[] motLoadFlag = new byte[8];

			public sbyte orgTexDataIdx;

			public sbyte texDataIdx;

			public sbyte texelDataIdx;

			public sbyte plttDataIdx;

			public sbyte chainTexDataIdx;

			public string name;

			public PRI_SCENE priScn;
		}

		public const PRI_SCENE PRI_SCENE_FIRST = PRI_SCENE.PRI_SCENE_FIRST;

		public const PRI_SCENE PRI_SCENE_SECOND = PRI_SCENE.PRI_SCENE_SECOND;

		public const PRI_SCENE PRI_SCENE_MAX = PRI_SCENE.PRI_SCENE_MAX;

		private const byte character_max = 22;

		private const byte shadow_max = 3;

		private const byte motion_data_max = 8;

		private const byte chaintexture_data_max = 5;

		private const byte character_flag_enable = 1;

		private const byte character_flag_flash = 2;

		private const byte character_flag_shadow_enable = 4;

		private const byte character_flag_hidden = 8;

		public const LOAD_FLAG LOAD_OBJECT = LOAD_FLAG.LOAD_OBJECT;

		public const LOAD_FLAG LOAD_ORGTEX = LOAD_FLAG.LOAD_ORGTEX;

		public const LOAD_FLAG LOAD_MOTION = LOAD_FLAG.LOAD_MOTION;

		public const LOAD_FLAG LOAD_TEX = LOAD_FLAG.LOAD_TEX;

		public const LOAD_FLAG LOAD_TEXEL = LOAD_FLAG.LOAD_TEXEL;

		public const LOAD_FLAG LOAD_PLTT = LOAD_FLAG.LOAD_PLTT;

		public static ds.sys3d.CLightObject pLight;

		public static int m_FrameRate;

		private static string[] shadow_file_name = new string[3] { "shadow02.nmdp", "shadow03.nmdp", "shadow01.nmdp" };

		private static int LOAD_FLAG_MASK = -1;

		private CharacterData[] Character = new CharacterData[22];

		private CObjectDataMng objectDataMng = new CObjectDataMng();

		private CMotionDataMng motionDataMng = new CMotionDataMng();

		private CTextureDataMng textureDataMng = new CTextureDataMng();

		private sys.ChainTextureManager[] chainTextureDataMng = new sys.ChainTextureManager[5];

		private ds.sys3d.Scene[] pScene = new ds.sys3d.Scene[2];

		private ds.sys3d.CModelSet[] shadow = new ds.sys3d.CModelSet[3];

		private CFileData[] ShadowData = new CFileData[3];

		public CCharacterMng()
		{
			for (int i = 0; i < Character.Length; i++)
			{
				Character[i] = new CharacterData();
			}
			for (int i = 0; i < ShadowData.Length; i++)
			{
				ShadowData[i] = new CFileData();
			}
			for (int i = 0; i < shadow.Length; i++)
			{
				shadow[i] = new ds.sys3d.CModelSet();
			}
			for (int i = 0; i < chainTextureDataMng.Length; i++)
			{
				chainTextureDataMng[i] = new sys.ChainTextureManager();
			}
		}

		~CCharacterMng()
		{
		}

		public void initialize(ds.sys3d.Scene scn1st, ds.sys3d.Scene scn2nd)
		{
			objectDataMng.init();
			motionDataMng.init();
			textureDataMng.init();
			for (int i = 0; i < 22; i++)
			{
				initValue(i);
			}
			pLight = null;
			m_FrameRate = 4096;
			pScene[0] = scn1st;
			pScene[1] = scn2nd;
			for (int j = 0; j < 3; j++)
			{
				ShadowData[j].setup(shadow_file_name[j], ds.fs.enFDL_FILETYPE.enFDL_FILETYPE_NORMAL);
				shadow[j].setup(ShadowData[j].getAddr());
			}
			NNS_G3dMdlSetMdlDiffAll(shadow[0].getMdlResource(), 0);
		}

		public void terminate()
		{
			for (int i = 0; i < 3; i++)
			{
				shadow[i].cleanup();
				ShadowData[i].cleanup();
			}
			for (int j = 0; j < 22; j++)
			{
				if (Character[j].flag != 0)
				{
					Character[j].motSet.cleanup();
					Character[j].animSet.cleanup();
					Character[j].modelSet.cleanup();
					PRI_SCENE priScn = Character[j].priScn;
					pScene[(int)priScn].removeRenderObject(Character[j].RdrObject);
					pScene[(int)priScn].removeRenderObject(Character[j].ShadowObject);
					initValue(j);
				}
			}
			objectDataMng.end();
			motionDataMng.end();
			textureDataMng.end();
			for (byte b = 0; b < 5; b++)
			{
				if (chainTextureDataMng[b].isLoadPackfile())
				{
					chainTextureDataMng[b].unloadTexturePack();
				}
			}
		}

		public void execute()
		{
			for (int i = 0; i < 22; i++)
			{
				if (isValidCharacter(i))
				{
					if ((1 & Character[i].flag) != 0 || (8 & Character[i].flag) == 0)
					{
						Character[i].animSet.next();
						Character[i].motSet.next();
					}
					setupObject(i);
					setupOrgTex(i);
					setupMotion(i);
					setupReplaceTex(i);
					setupReplaceTexel(i);
					setupReplacePltt(i);
				}
			}
			execFlash();
		}

		public bool isLoadingCharaAsync()
		{
			for (int i = 0; i < 22; i++)
			{
				if ((Character[i].loadFlag & 1) == 0 || (Character[i].loadFlag & 2) == 0)
				{
					return true;
				}
			}
			return false;
		}

		public bool isLoadingMotionAsync()
		{
			for (int i = 0; i < 22; i++)
			{
				if ((Character[i].loadFlag & 4) == 0)
				{
					return true;
				}
			}
			return false;
		}

		public bool isLoadingVramAsync()
		{
			return !TexDivideLoader.getSingleton().tdlIsEmpty();
		}

		public int setCharacter(string name, PRI_SCENE pri)
		{
			return setCharacterImp(name, name, pri);
		}

		public int setCharacterWithTexture(string charaName, string texName, PRI_SCENE pri)
		{
			return setCharacterImp(charaName, texName, pri);
		}

		public int setCharacterImp(string pCharaName, string pTexName, PRI_SCENE pri)
		{
			string arg = "";
			sprintf(out arg, "%s", getModelLocate(pCharaName));
			FS_ChangeDir(arg);
			NNS_FndGetTotalFreeSizeForExpHeap(ds.CHeap.getHeapHandle());
			int num = -1;
			int num2 = -1;
			ds.CHeap.setID_app(1);
			setBank();
			num2 = searchCharacterIndex();
			if (num2 == -1)
			{
				FS_ChangeDir("/");
				return -1;
			}
			initValue(num2);
			sprintf(out Character[num2].name, "%s", pCharaName);
			num = objectDataMng.setData(pCharaName, async: false);
			if (num == -1)
			{
				FS_ChangeDir("/");
				return -1;
			}
			Character[num2].objDataIdx = (sbyte)num;
			Character[num2].orgTexDataIdx = (sbyte)textureDataMng.setData(pTexName, async: false);
			Character[num2].flag |= 1u;
			Character[num2].priScn = pri;
			Character[num2].loadFlag |= 8u;
			Character[num2].loadFlag |= 16u;
			Character[num2].loadFlag |= 32u;
			setupCharacter(num2);
			ds.CHeap.setID_app(0);
			FS_ChangeDir("/");
			return num2;
		}

		public int setCharacterAsync(string name, PRI_SCENE pri)
		{
			string arg = "";
			sprintf(out arg, "%s", getModelLocate(name));
			string arg2 = "";
			sprintf(out arg2, "%s/%s", arg, name);
			int num = -1;
			int num2 = -1;
			num2 = searchCharacterIndex();
			if (num2 == -1)
			{
				return -1;
			}
			initValue(num2);
			num = objectDataMng.setData(arg2, async: true);
			if (num == -1)
			{
				return -1;
			}
			Character[num2].objDataIdx = (sbyte)num;
			if (-1 != Character[num2].objDataIdx)
			{
				Character[num2].loadFlag &= 4294967294u;
			}
			Character[num2].orgTexDataIdx = (sbyte)textureDataMng.setData(arg2, async: true);
			if (-1 != Character[num2].orgTexDataIdx)
			{
				Character[num2].loadFlag &= 4294967293u;
			}
			Character[num2].priScn = pri;
			Character[num2].flag |= 1u;
			return num2;
		}

		public void delCharacter(int ctrl)
		{
			NNS_FndGetTotalFreeSizeForExpHeap(ds.CHeap.getHeapHandle());
			if (Character[ctrl].flag == 0)
			{
				return;
			}
			pScene[(int)Character[ctrl].priScn].removeRenderObject(Character[ctrl].RdrObject);
			pScene[(int)Character[ctrl].priScn].removeRenderObject(Character[ctrl].ShadowObject);
			Character[ctrl].modelSet.cleanup();
			Character[ctrl].RdrObject.cleanup();
			Character[ctrl].ShadowObject.cleanup();
			Character[ctrl].animSet.cleanup();
			Character[ctrl].motSet.cleanup();
			objectDataMng.delData(Character[ctrl].objDataIdx);
			for (byte b = 0; b < 8; b++)
			{
				if (-1 != Character[ctrl].motDataIdx[b])
				{
					motionDataMng.delData(Character[ctrl].motDataIdx[b]);
				}
			}
			if (-1 != Character[ctrl].orgTexDataIdx)
			{
				textureDataMng.delData(Character[ctrl].orgTexDataIdx);
			}
			if (-1 != Character[ctrl].texDataIdx)
			{
				textureDataMng.delData(Character[ctrl].texDataIdx);
			}
			if (-1 != Character[ctrl].texelDataIdx)
			{
				textureDataMng.delData(Character[ctrl].texelDataIdx);
			}
			if (-1 != Character[ctrl].plttDataIdx)
			{
				textureDataMng.delData(Character[ctrl].plttDataIdx);
			}
			initValue(ctrl);
		}

		public void setPosition(int ctrl, VecFx32 pos)
		{
			if (isValidCharacter(ctrl))
			{
				Character[ctrl].RdrObject.setPosition(pos);
			}
		}

		public void setPosition(int ctrl, int x, int y, int z)
		{
			if (isValidCharacter(ctrl))
			{
				VecFx32 fnd_reuse_pos = GlobalScope.fnd_reuse_pos;
				fnd_reuse_pos.x = x;
				fnd_reuse_pos.y = y;
				fnd_reuse_pos.z = z;
				Character[ctrl].RdrObject.setPosition(fnd_reuse_pos);
			}
		}

		public void getPosition(int ctrl, VecFx32 pos)
		{
			if (isValidCharacter(ctrl))
			{
				Character[ctrl].RdrObject.getPosition(pos);
			}
		}

		public void setRotation(int ctrl, ushort x, ushort y, ushort z)
		{
			if (isValidCharacter(ctrl))
			{
				Character[ctrl].RdrObject.setRotation(x, y, z);
			}
		}

		public void getRotation(int ctrl, ref ushort x, ref ushort y, ref ushort z)
		{
			if (isValidCharacter(ctrl))
			{
				Character[ctrl].RdrObject.getRotation(out x, out y, out z);
			}
		}

		public void setScale(int ctrl, VecFx32 scale)
		{
			if (isValidCharacter(ctrl))
			{
				Character[ctrl].RdrObject.setScale(scale);
			}
		}

		public void getScale(int ctrl, VecFx32 scale)
		{
			if (isValidCharacter(ctrl))
			{
				Character[ctrl].RdrObject.getScale(scale);
			}
		}

		public void setPoseMtx(int ctrl, MtxFx43 mtx)
		{
			if (isValidCharacter(ctrl))
			{
				Character[ctrl].RdrObject.setPoseMtx(mtx);
			}
		}

		public void getPoseMtx(int ctrl, MtxFx43 @out)
		{
			if (isValidCharacter(ctrl))
			{
				Character[ctrl].RdrObject.getPoseMtx(@out);
			}
		}

		/// <summary>
		/// PORT: something drawn in a character's place (CRenderObject.StandIn) - a mod's glTF in
		/// a weapon's hand. Null puts the model's own draw back. Survives the asynchronous load
		/// (the render object is set up when the model arrives, the stand-in already on it) and
		/// goes with the character when it is deleted.
		/// </summary>
		public void setStandIn(int ctrl, Action<ds.sys3d.CRenderObject> standIn)
		{
			if (isValidCharacter(ctrl))
			{
				Character[ctrl].RdrObject.StandIn = standIn;
			}
		}

		public void initJntMtx(int ctrl)
		{
			if (isValidCharacter(ctrl))
			{
				Character[ctrl].RdrObject.initJntMtx();
			}
		}

		public bool reserveToGetJntMtx(int ctrl, string pNodename)
		{
			if (!isValidCharacter(ctrl))
			{
				return false;
			}
			return Character[ctrl].RdrObject.reserveToGetJntMtx(pNodename);
		}

		public bool getJntMtx(int ctrl, string pNodename, MtxFx43 @out)
		{
			if (!isValidCharacter(ctrl))
			{
				return false;
			}
			return Character[ctrl].RdrObject.getJntMtx(pNodename, @out);
		}

		public void addMotion(int ctrl, string motname)
		{
			if (!isValidCharacter(ctrl))
			{
				return;
			}
			string arg = "";
			sprintf(out arg, "%s", getMotionLocate(motname));
			FS_ChangeDir(arg);
			ds.CHeap.setID_app(2);
			NNS_FndGetTotalFreeSizeForExpHeap(ds.CHeap.getHeapHandle());
			int num = -1;
			for (num = 0; num < 8; num++)
			{
				if (-1 == Character[ctrl].motDataIdx[num])
				{
					Character[ctrl].motDataIdx[num] = (sbyte)motionDataMng.setData(const_cast<string>(motname), async: false);
					if (-1 != Character[ctrl].motDataIdx[num])
					{
						Character[ctrl].motSet.addMotion(motionDataMng.MotionData[Character[ctrl].motDataIdx[num]].motData.getAddr<ds.sys3d.ncap.SMotionFileHeader>());
					}
					break;
				}
			}
			ds.CHeap.setID_app(0);
			FS_ChangeDir("/");
		}

		public void addMotionAsync(int ctrl, string motname)
		{
			if (!isValidCharacter(ctrl))
			{
				return;
			}
			string arg = "";
			sprintf(out arg, "%s/%s", getMotionLocate(motname), motname);
			int num = -1;
			for (num = 0; num < 8; num++)
			{
				if (-1 == Character[ctrl].motDataIdx[num])
				{
					Character[ctrl].motDataIdx[num] = (sbyte)motionDataMng.setData(const_cast<string>(arg), async: true);
					if (-1 != Character[ctrl].motDataIdx[num] && -1 != Character[ctrl].motDataIdx[num])
					{
						Character[ctrl].loadFlag &= 4294967291u;
					}
					break;
				}
			}
		}

		public void removeMotion(int ctrl, string motname)
		{
			if (!isValidCharacter(ctrl))
			{
				return;
			}
			int num = -1;
			num = motionDataMng.searchDataIndex(const_cast<string>(motname));
			if (-1 == num)
			{
				return;
			}
			Character[ctrl].motSet.removeMotion(motionDataMng.MotionData[num].motData.getAddr<ds.sys3d.ncap.SMotionFileHeader>());
			motionDataMng.delData(num);
			for (byte b = 0; b < 8; b++)
			{
				if (num == Character[ctrl].motDataIdx[b])
				{
					Character[ctrl].motDataIdx[b] = -1;
					break;
				}
			}
		}

		public void removeAllMotion(int ctrl)
		{
			if (!isValidCharacter(ctrl))
			{
				return;
			}
			for (byte b = 0; b < 8; b++)
			{
				sbyte b2 = Character[ctrl].motDataIdx[b];
				if (-1 != b2)
				{
					Character[ctrl].motSet.removeMotion(motionDataMng.MotionData[b2].motData.getAddr<ds.sys3d.ncap.SMotionFileHeader>());
					motionDataMng.delData(b2);
					Character[ctrl].motDataIdx[b] = -1;
				}
			}
		}

		public void startMotion(int ctrl, int index, bool fLoop, uint blendFrame)
		{
			if (isValidCharacter(ctrl))
			{
				Character[ctrl].motSet.start((uint)index, fLoop, blendFrame);
			}
		}

		public bool isMotion(int ctrl, int index)
		{
			if (!isValidCharacter(ctrl))
			{
				return false;
			}
			return Character[ctrl].motSet.isMotion((uint)index);
		}

		public bool isEndOfMotion(int ctrl)
		{
			if (!isValidCharacter(ctrl))
			{
				return false;
			}
			return Character[ctrl].motSet.isEndOfMotion();
		}

		public void setCurrentFrame(int ctrl, uint frame)
		{
			if (isValidCharacter(ctrl))
			{
				Character[ctrl].motSet.setFrame(frame);
			}
		}

		public uint getCurrentFrame(int ctrl)
		{
			if (!isValidCharacter(ctrl))
			{
				return 0u;
			}
			return Character[ctrl].motSet.getFrame();
		}

		public uint getMaxFrame(int ctrl)
		{
			if (!isValidCharacter(ctrl))
			{
				return 0u;
			}
			return Character[ctrl].motSet.getMaxFrame();
		}

		public int getMotionIndex(int ctrl)
		{
			if (!isValidCharacter(ctrl))
			{
				return -1;
			}
			return Character[ctrl].motSet.getIndex();
		}

		/// <summary>PORT: the playing motion's name in its pack (b01_001_01), or null.</summary>
		public string getMotionName(int ctrl)
		{
			return isValidCharacter(ctrl) ? Character[ctrl].motSet.getMotionName() : null;
		}

		/// <summary>PORT: the name of the model a character was set up with (j101), or null.</summary>
		public string getModelName(int ctrl)
		{
			if (!isValidCharacter(ctrl)) return null;
			try { return Character[ctrl].RdrObject.ModelRes?.name?.TrimEnd('\0', ' '); } catch (Exception) { return null; }
		}

		/// <summary>PORT: the character drawn through a render object, or -1 (a mod's glTF standing in for a character asks whose motion it plays).</summary>
		public int findByRenderObject(ds.sys3d.CRenderObject ro)
		{
			if (ro == null) return -1;
			for (int i = 0; i < Character.Length; i++)
			{
				if (Character[i] != null && ReferenceEquals(Character[i].RdrObject, ro) && isValidCharacter(i)) return i;
			}
			return -1;
		}

		public int getPreMotionIndex(int ctrl)
		{
			if (!isValidCharacter(ctrl))
			{
				return -1;
			}
			return Character[ctrl].motSet.getPreIndex();
		}

		public void setMotionSpeed(int ctrl, int speed)
		{
			if (isValidCharacter(ctrl))
			{
				Character[ctrl].motSet.setFrameRate(speed);
			}
		}

		public int getMotionSpeed(int ctrl)
		{
			if (!isValidCharacter(ctrl))
			{
				return 0;
			}
			return Character[ctrl].motSet.getFrameRate();
		}

		public void setMotionLoop(int ctrl, bool loop)
		{
			if (isValidCharacter(ctrl))
			{
				Character[ctrl].motSet.setLoop(loop);
			}
		}

		public void setMotionPause(int ctrl, bool b)
		{
			if (isValidCharacter(ctrl))
			{
				Character[ctrl].motSet.setPause(b);
			}
		}

		public void startAnimation(int ctrl, uint idx, ds.sys3d.CAnimSet.enTYPE type, int frame)
		{
			if (isValidCharacter(ctrl))
			{
				Character[ctrl].animSet.startAnimation(idx, type, frame);
			}
		}

		public void setFrameRate(int ctrl, int rate, ds.sys3d.CAnimSet.enTYPE type)
		{
			if (isValidCharacter(ctrl))
			{
				Character[ctrl].animSet.setFrameRate(rate, type);
			}
		}

		public int getFrameRate(int ctrl, ds.sys3d.CAnimSet.enTYPE type)
		{
			if (!isValidCharacter(ctrl))
			{
				return 0;
			}
			return Character[ctrl].animSet.getFrameRate(type);
		}

		public void setFrame(int ctrl, uint frame, ds.sys3d.CAnimSet.enTYPE type)
		{
			if (isValidCharacter(ctrl))
			{
				Character[ctrl].animSet.setFrame(frame, type);
			}
		}

		public uint getFrame(int ctrl, ds.sys3d.CAnimSet.enTYPE type)
		{
			if (!isValidCharacter(ctrl))
			{
				return 0u;
			}
			return Character[ctrl].animSet.getFrame(type);
		}

		public uint getMaxFrame(int ctrl, ds.sys3d.CAnimSet.enTYPE type)
		{
			if (!isValidCharacter(ctrl))
			{
				return 0u;
			}
			return Character[ctrl].animSet.getMaxFrame(type);
		}

		public void setLoop(int ctrl, bool loop, ds.sys3d.CAnimSet.enTYPE type)
		{
			if (isValidCharacter(ctrl))
			{
				Character[ctrl].animSet.setLoop(loop, type);
			}
		}

		public void setPause(int ctrl, bool pause, ds.sys3d.CAnimSet.enTYPE type)
		{
			if (isValidCharacter(ctrl))
			{
				Character[ctrl].animSet.setPause(pause, type);
			}
		}

		public bool isEnd(int ctrl, ds.sys3d.CAnimSet.enTYPE type)
		{
			if (!isValidCharacter(ctrl))
			{
				return false;
			}
			return Character[ctrl].animSet.isEndOfMotion(type);
		}

		public void setShadowScale(int ctrl, VecFx32 scale)
		{
			if (isValidCharacter(ctrl))
			{
				Character[ctrl].ShadowObject.setScale(scale);
			}
		}

		public void getShadowScale(int ctrl, VecFx32 scale)
		{
			if (isValidCharacter(ctrl))
			{
				Character[ctrl].ShadowObject.getScale(scale);
			}
		}

		public void setShadowHeight(int ctrl, int height)
		{
			if (isValidCharacter(ctrl))
			{
				Character[ctrl].ShadowObject.setHeight(height);
			}
		}

		public int getShadowHeight(int ctrl)
		{
			if (!isValidCharacter(ctrl))
			{
				return 0;
			}
			return Character[ctrl].ShadowObject.getHeight();
		}

		public void setShadowAlpha(int ctrl, short alpha)
		{
			if (isValidCharacter(ctrl))
			{
				Character[ctrl].ShadowObject.setAlpha((sbyte)alpha);
			}
		}

		public short getShadowAlpha(int ctrl)
		{
			if (!isValidCharacter(ctrl))
			{
				return -1;
			}
			return Character[ctrl].ShadowObject.getAlpha();
		}

		public void setShadowAlphaRate(int ctrl, int rate)
		{
			if (isValidCharacter(ctrl))
			{
				Character[ctrl].ShadowObject.setAlphaRate((byte)rate);
			}
		}

		public int getShadowAlphaRate(int ctrl)
		{
			if (!isValidCharacter(ctrl))
			{
				return -1;
			}
			return Character[ctrl].ShadowObject.getAlphaRate();
		}

		public void setShadowType(int ctrl, int type)
		{
			if (isValidCharacter(ctrl))
			{
				Character[ctrl].shadow_type = (uint)type;
				switch (type)
				{
				case 0:
					Character[ctrl].ShadowObject.setup(shadow[0].getMdlResource(), Character[ctrl].RdrObject);
					Character[ctrl].ShadowObject.setType(ds.sys3d.SHADOW_TYPE.SHADOW_TYPE_POLYGON);
					break;
				case 1:
					Character[ctrl].ShadowObject.setup(shadow[1].getMdlResource(), Character[ctrl].RdrObject);
					Character[ctrl].ShadowObject.setType(ds.sys3d.SHADOW_TYPE.SHADOW_TYPE_POLYGON);
					break;
				case 2:
					Character[ctrl].ShadowObject.setType(ds.sys3d.SHADOW_TYPE.SHADOW_TYPE_ERROR);
					break;
				case 3:
					Character[ctrl].ShadowObject.setup(shadow[2].getMdlResource(), Character[ctrl].RdrObject);
					Character[ctrl].ShadowObject.setType(ds.sys3d.SHADOW_TYPE.SHADOW_TYPE_VOLUME);
					break;
				case 4:
					Character[ctrl].ShadowObject.setup(shadow[1].getMdlResource(), Character[ctrl].RdrObject);
					Character[ctrl].ShadowObject.setType(ds.sys3d.SHADOW_TYPE.SHADOW_TYPE_POLYGON);
					break;
				}
			}
		}

		public void setShadowEnable(int ctrl, bool b)
		{
			if (isValidCharacter(ctrl))
			{
				Character[ctrl].RdrObject.setShadowEnable(b);
			}
		}

		// PORT (FF4): ce_SetShadingMode switches a scene character between lit and toon polygons.
		public void setPolygonMode(int ctrl, GXPolygonMode mode)
		{
			if (isValidCharacter(ctrl))
			{
				Character[ctrl].RdrObject.setPolygonMode(mode);
			}
		}

		public void setDiffuse(int ctrl, ushort diffuse)
		{
			if (isValidCharacter(ctrl))
			{
				Character[ctrl].modelSet.getMdl(0u).setDiffuse(diffuse);
			}
		}

		public void setAmbient(int ctrl, ushort ambient)
		{
			if (isValidCharacter(ctrl))
			{
				Character[ctrl].modelSet.getMdl(0u).setAmbient(ambient);
			}
		}

		public void setSpecular(int ctrl, ushort specular)
		{
			if (isValidCharacter(ctrl))
			{
				Character[ctrl].modelSet.getMdl(0u).setSpecular(specular);
			}
		}

		public void setEmission(int ctrl, ushort emission)
		{
			if (isValidCharacter(ctrl))
			{
				Character[ctrl].modelSet.getMdl(0u).setEmission(emission);
			}
		}

		public void setLight(int ctrl, ds.sys3d.CLightObject light)
		{
			if (isValidCharacter(ctrl))
			{
				Character[ctrl].LightObject = light;
				Character[ctrl].RdrObject.setLightObject(Character[ctrl].LightObject);
			}
		}

		public void setLightOne(int ctrl, uint index, ds.sys3d.Light light)
		{
			if (isValidCharacter(ctrl))
			{
				Character[ctrl].RdrObject.setLightOne(index, light);
			}
		}

		public void getLight(int ctrl, ds.sys3d.CLightObject light)
		{
			if (isValidCharacter(ctrl))
			{
				light.copy(Character[ctrl].LightObject);
			}
		}

		public void enableLight(int ctrl)
		{
			NNS_G3dMdlSetMdlLightEnableFlagAll(Character[ctrl].modelSet.getMdlResource(), 15);
		}

		public void disableLight(int ctrl)
		{
			NNS_G3dMdlSetMdlLightEnableFlagAll(Character[ctrl].modelSet.getMdlResource(), 0);
		}

		public bool isEnableLight(int ctrl)
		{
			return NNS_G3dMdlGetMdlLightEnableFlag(Character[ctrl].modelSet.getMdlResource(), 0u) != 0;
		}

		public bool isHidden(int ctrl)
		{
			if (!isValidCharacter(ctrl))
			{
				return false;
			}
			if ((8 & Character[ctrl].flag) != 0)
			{
				return true;
			}
			return false;
		}

		public void setHidden(int ctrl, bool b)
		{
			if (!isValidCharacter(ctrl))
			{
				return;
			}
			bool flag = false;
			if ((1 & Character[ctrl].loadFlag) == 0)
			{
				flag = true;
			}
			if (b)
			{
				Character[ctrl].flag |= 8u;
				if (!flag)
				{
					Character[ctrl].RdrObject.setHidden(b: true);
					Character[ctrl].ShadowObject.setEnable(b: false);
				}
			}
			else
			{
				Character[ctrl].flag &= 4294967287u;
				if (!flag)
				{
					Character[ctrl].RdrObject.setHidden(b: false);
					Character[ctrl].ShadowObject.setEnable(!Character[ctrl].shadowOff);
				}
			}
		}

		// PORT (FF4): whether a character's shadow is drawn at all, apart from the character being
		// hidden. FF4's scene casts have none until ce_ShadowSetting / ce_ShadowVisiblity(slot, 1);
		// FF3 characters always have theirs (setupCharacter gives every model a polygon shadow).
		public void setShadowVisible(int ctrl, bool visible)
		{
			if (!isValidCharacter(ctrl))
			{
				return;
			}
			Character[ctrl].shadowOff = !visible;
			if ((1 & Character[ctrl].loadFlag) != 0)
			{
				Character[ctrl].ShadowObject.setEnable(visible && (8 & Character[ctrl].flag) == 0);
			}
		}

		// PORT (FF4): CCharacterMng::setShadowJntName - the shadow's x and z follow this joint's world
		// position (FF4's scene casts stand where their motion's root puts them, not at setPosition's
		// spot). The joint matrix is captured while the model draws (reserveToGetJntMtx).
		public void setShadowJntName(int ctrl, string jointName)
		{
			if (!isValidCharacter(ctrl))
			{
				return;
			}
			Character[ctrl].shadowJoint = string.IsNullOrEmpty(jointName) ? null : jointName;
			Character[ctrl].ShadowObject.setJointName(Character[ctrl].shadowJoint);
			if (Character[ctrl].shadowJoint != null && (1 & Character[ctrl].loadFlag) != 0)
			{
				Character[ctrl].RdrObject.reserveToGetJntMtx(Character[ctrl].shadowJoint);
			}
		}

		public void setTransparency(int ctrl, int alpha)
		{
			if (isValidCharacter(ctrl))
			{
				Character[ctrl].RdrObject.setAlpha(alpha);
			}
		}

		public uint getTransparency(int ctrl)
		{
			if (!isValidCharacter(ctrl))
			{
				return 0u;
			}
			return Character[ctrl].RdrObject.getAlpha();
		}

		public void setTransparencyRate(int ctrl, int rate)
		{
			if (isValidCharacter(ctrl))
			{
				Character[ctrl].RdrObject.setAlphaRate(rate);
			}
		}

		public int getTransparencyRate(int ctrl)
		{
			if (!isValidCharacter(ctrl))
			{
				return 0;
			}
			return Character[ctrl].RdrObject.getAlphaRate();
		}

		public void setMaterialAlpha(int ctrl, uint matIdx, uint alpha)
		{
			if (isValidCharacter(ctrl))
			{
				Character[ctrl].RdrObject.setMaterialAlpha(matIdx, alpha);
			}
		}

		public uint getMaterialAlpha(int ctrl, uint matIdx)
		{
			if (!isValidCharacter(ctrl))
			{
				return 0u;
			}
			return Character[ctrl].RdrObject.getMaterialAlpha(matIdx);
		}

		public void bindMdlTex(int ctrl)
		{
			if (isValidCharacter(ctrl))
			{
				setBank();
				unbindTex(ctrl);
				unbindTexel(ctrl);
				unbindPltt(ctrl);
				if (-1 == Character[ctrl].orgTexDataIdx)
				{
					Character[ctrl].modelSet.bindMdlTex();
				}
				else
				{
					Character[ctrl].modelSet.bindReplaceTex(textureDataMng.getTex(Character[ctrl].orgTexDataIdx));
				}
			}
		}

		public void bindMdlTexel(int ctrl)
		{
			if (isValidCharacter(ctrl))
			{
				setBank();
				unbindTexel(ctrl);
				if (-1 == Character[ctrl].orgTexDataIdx)
				{
					Character[ctrl].modelSet.bindMdlTexel();
				}
				else
				{
					Character[ctrl].modelSet.bindReplaceTexel(textureDataMng.getTex(Character[ctrl].orgTexDataIdx));
				}
			}
		}

		public void bindMdlPltt(int ctrl)
		{
			if (isValidCharacter(ctrl))
			{
				setBank();
				unbindPltt(ctrl);
				if (-1 == Character[ctrl].orgTexDataIdx)
				{
					Character[ctrl].modelSet.bindMdlPltt();
				}
				else
				{
					Character[ctrl].modelSet.bindReplacePltt(textureDataMng.getTex(Character[ctrl].orgTexDataIdx));
				}
			}
		}

		public void bindReplaceTex(int ctrl, string texname)
		{
			if (!isValidCharacter(ctrl))
			{
				return;
			}
			string arg = "";
			sprintf(out arg, "%s", getModelLocate(texname));
			FS_ChangeDir(arg);
			setBank();
			unbindTex(ctrl);
			int num = -1;
			num = textureDataMng.setData(const_cast<string>(texname), async: false);
			if (-1 == num)
			{
				FS_ChangeDir("/");
				return;
			}
			Character[ctrl].texDataIdx = (sbyte)num;
			Character[ctrl].loadFlag |= 2u;
			if (textureDataMng.getTex(Character[ctrl].texDataIdx).isLoadedToVram())
			{
				Character[ctrl].modelSet.bindReplaceTex(textureDataMng.getTex(num));
			}
			else
			{
				Character[ctrl].loadFlag &= 4294967287u;
			}
			FS_ChangeDir("/");
		}

		public void bindReplaceTexel(int ctrl, string texname)
		{
			if (isValidCharacter(ctrl))
			{
				string arg = "";
				sprintf(out arg, "%s", getModelLocate(texname));
				FS_ChangeDir(arg);
				setBank();
				unbindTexel(ctrl);
				int num = -1;
				num = textureDataMng.setData(const_cast<string>(texname), async: false);
				if (-1 == num)
				{
					FS_ChangeDir("/");
					return;
				}
				Character[ctrl].modelSet.bindReplaceTexel(textureDataMng.getTex(num));
				Character[ctrl].texelDataIdx = (sbyte)num;
				FS_ChangeDir("/");
			}
		}

		public void bindReplacePltt(int ctrl, string texname)
		{
			if (!isValidCharacter(ctrl))
			{
				return;
			}
			string arg = "";
			sprintf(out arg, "%s", getModelLocate(texname));
			FS_ChangeDir(arg);
			setBank();
			unbindPltt(ctrl);
			int num = textureDataMng.setData(const_cast<string>(texname), async: false);
			if (-1 == num)
			{
				FS_ChangeDir("/");
				return;
			}
			Character[ctrl].plttDataIdx = (sbyte)num;
			if (textureDataMng.getTex(Character[ctrl].plttDataIdx).isLoadedToVram())
			{
				Character[ctrl].modelSet.bindReplacePltt(textureDataMng.getTex(num));
			}
			else
			{
				Character[ctrl].loadFlag &= 4294967263u;
			}
			FS_ChangeDir("/");
		}

		public void bindReplaceTexAsync(int ctrl, string texname)
		{
			if (isValidCharacter(ctrl))
			{
				string arg = "";
				sprintf(out arg, "%s", getModelLocate(texname));
				string arg2 = "";
				sprintf(out arg2, "%s/%s", arg, texname);
				int num = -1;
				num = textureDataMng.setData(const_cast<string>(arg2), async: true);
				if (-1 != num)
				{
					Character[ctrl].texDataIdx = (sbyte)num;
					Character[ctrl].loadFlag &= 4294967287u;
				}
			}
		}

		public void bindReplaceTexelAsync(int ctrl, string texname)
		{
			if (isValidCharacter(ctrl))
			{
				string arg = "";
				sprintf(out arg, "%s", getModelLocate(texname));
				string arg2 = "";
				sprintf(out arg2, "%s/%s", arg, texname);
				int num = -1;
				num = textureDataMng.setData(const_cast<string>(arg2), async: true);
				if (-1 != num)
				{
					Character[ctrl].texelDataIdx = (sbyte)num;
					Character[ctrl].loadFlag &= 4294967279u;
				}
			}
		}

		public void bindReplacePlttAsync(int ctrl, string texname)
		{
			if (isValidCharacter(ctrl))
			{
				string arg = "";
				sprintf(out arg, "%s", getModelLocate(texname));
				string arg2 = "";
				sprintf(out arg2, "%s/%s", arg, texname);
				int num = -1;
				num = textureDataMng.setData(const_cast<string>(arg2), async: true);
				if (-1 != num)
				{
					Character[ctrl].plttDataIdx = (sbyte)num;
					Character[ctrl].loadFlag &= 4294967263u;
				}
			}
		}

		public void unbindTex(int ctrl)
		{
			if (isValidCharacter(ctrl))
			{
				if (-1 != Character[ctrl].texDataIdx)
				{
					textureDataMng.delData(Character[ctrl].texDataIdx);
					Character[ctrl].texDataIdx = -1;
				}
				Character[ctrl].modelSet.unbindTex();
			}
		}

		public void unbindTexel(int ctrl)
		{
			if (isValidCharacter(ctrl))
			{
				if (-1 != Character[ctrl].texelDataIdx)
				{
					textureDataMng.delData(Character[ctrl].texelDataIdx);
					Character[ctrl].texelDataIdx = -1;
				}
				Character[ctrl].modelSet.unbindTexel();
			}
		}

		public void unbindPltt(int ctrl)
		{
			if (isValidCharacter(ctrl))
			{
				if (-1 != Character[ctrl].plttDataIdx)
				{
					textureDataMng.delData(Character[ctrl].plttDataIdx);
					Character[ctrl].plttDataIdx = -1;
				}
				Character[ctrl].modelSet.unbindPltt();
			}
		}

		public int getReplaceTexId(int ctrl)
		{
			if (!isValidCharacter(ctrl))
			{
				return -1;
			}
			return Character[ctrl].texDataIdx;
		}

		public int getReplaceTexelId(int ctrl)
		{
			if (!isValidCharacter(ctrl))
			{
				return -1;
			}
			return Character[ctrl].texelDataIdx;
		}

		public int getReplacePlttId(int ctrl)
		{
			if (!isValidCharacter(ctrl))
			{
				return -1;
			}
			return Character[ctrl].plttDataIdx;
		}

		public bool setChainTexture(int ctrl, string texname)
		{
			if (!isValidCharacter(ctrl))
			{
				return false;
			}
			if (-1 != Character[ctrl].chainTexDataIdx)
			{
				chainTextureDataMng[Character[ctrl].chainTexDataIdx].unloadTexturePack();
				Character[ctrl].chainTexDataIdx = -1;
			}
			for (byte b = 0; b < 5; b++)
			{
				if (!chainTextureDataMng[b].isLoadPackfile())
				{
					if (!chainTextureDataMng[b].loadTexturePack(texname))
					{
						return false;
					}
					Character[ctrl].chainTexDataIdx = (sbyte)b;
					return true;
				}
			}
			return false;
		}

		public void delChainTexture(int ctrl)
		{
			if (isValidCharacter(ctrl) && -1 != Character[ctrl].chainTexDataIdx)
			{
				chainTextureDataMng[Character[ctrl].chainTexDataIdx].unloadTexturePack();
				Character[ctrl].chainTexDataIdx = -1;
			}
		}

		public void bindChainTexel(int ctrl, uint dataIdx, string nodename)
		{
			if (isValidCharacter(ctrl) && -1 != Character[ctrl].chainTexDataIdx)
			{
				uint chainTexDataIdx = (uint)Character[ctrl].chainTexDataIdx;
				ds.sys3d.CModelSet modelSet = Character[ctrl].modelSet;
				chainTextureDataMng[chainTexDataIdx].replaceTexel(modelSet, dataIdx, nodename);
			}
		}

		public void bindChainPltt(int ctrl, uint dataIdx, string nodename)
		{
			if (isValidCharacter(ctrl) && -1 != Character[ctrl].chainTexDataIdx)
			{
				uint chainTexDataIdx = (uint)Character[ctrl].chainTexDataIdx;
				ds.sys3d.CModelSet modelSet = Character[ctrl].modelSet;
				chainTextureDataMng[chainTexDataIdx].replacePalette(modelSet, dataIdx, nodename);
			}
		}

		public void releaseMdlTexRes(int ctrl)
		{
			if (isValidCharacter(ctrl) && -1 != Character[ctrl].orgTexDataIdx)
			{
				sbyte orgTexDataIdx = Character[ctrl].orgTexDataIdx;
				ds.sys3d.CModelTexture tex = textureDataMng.getTex(orgTexDataIdx);
				tex.reqReleaseResource();
			}
		}

		public void releaseTex(int ctrl)
		{
			if (isValidCharacter(ctrl) && -1 != Character[ctrl].orgTexDataIdx)
			{
				unbindTex(ctrl);
				textureDataMng.delData(Character[ctrl].orgTexDataIdx);
				Character[ctrl].orgTexDataIdx = -1;
			}
		}

		public void setFrameRate(int fps)
		{
			m_FrameRate = fps;
		}

		public bool isClipping(int ctrl)
		{
			if (!isValidCharacter(ctrl))
			{
				return false;
			}
			return Character[ctrl].RdrObject.isClipping();
		}

		public void setLOD(int ctrl, uint lod)
		{
			if (isValidCharacter(ctrl))
			{
				Character[ctrl].RdrObject.setLOD(lod);
				Character[ctrl].RdrObject.setLODCnt((uint)ctrl);
			}
		}

		public ds.sys3d.BoundingBox getBoundingBox(int ctrl)
		{
			return Character[ctrl].modelSet.getBoundingBox();
		}

		public void setFlash(int ctrl)
		{
			if (isValidCharacter(ctrl) && (Character[ctrl].flag & 2) == 0)
			{
				Character[ctrl].flag |= 2u;
				Character[ctrl].FlashLight.vec.x = 4095;
				Character[ctrl].FlashLight.vec.y = 0;
				Character[ctrl].FlashLight.vec.z = 0;
				Character[ctrl].FlashLight.r = 31;
				Character[ctrl].FlashLight.g = 31;
				Character[ctrl].FlashLight.b = 31;
				setLightOne(ctrl, 2u, Character[ctrl].FlashLight);
			}
		}

		public void endFlash(int ctrl)
		{
			if (isValidCharacter(ctrl))
			{
				Character[ctrl].flag &= 4294967293u;
			}
		}

		public void execFlash(int ctrl)
		{
			if (isValidCharacter(ctrl))
			{
				Character[ctrl].FlashLight.r -= 3;
				Character[ctrl].FlashLight.g -= 3;
				Character[ctrl].FlashLight.b -= 3;
				if (Character[ctrl].FlashLight.r <= 0)
				{
					Character[ctrl].FlashLight.r = 0;
				}
				if (Character[ctrl].FlashLight.g <= 0)
				{
					Character[ctrl].FlashLight.g = 0;
				}
				if (Character[ctrl].FlashLight.b <= 0)
				{
					Character[ctrl].FlashLight.b = 0;
				}
				setLightOne(ctrl, 2u, Character[ctrl].FlashLight);
				if (Character[ctrl].FlashLight.r == 0)
				{
					endFlash(ctrl);
				}
			}
		}

		public void execFlash()
		{
			for (int i = 0; i < 22; i++)
			{
				if ((Character[i].flag & 2) != 0)
				{
					execFlash(i);
				}
			}
		}

		public bool isViewVolumeClip(int ctrl)
		{
			if (!isValidCharacter(ctrl))
			{
				return false;
			}
			return Character[ctrl].RdrObject.isEnableViewVolumeClip();
		}

		public void setViewVolumeClip(int ctrl, bool enable)
		{
			Character[ctrl].RdrObject.setEnableViewVolumeClip(enable);
		}

		public void initValue(int ctrl)
		{
			Character[ctrl].flag = 0u;
			Character[ctrl].shadow_type = 1u;
			Character[ctrl].shadowOff = false;
			Character[ctrl].shadowJoint = null;
			Character[ctrl].objDataIdx = -1;
			for (byte b = 0; b < 8; b++)
			{
				Character[ctrl].motDataIdx[b] = -1;
				Character[ctrl].motLoadFlag[b] = 0;
			}
			Character[ctrl].orgTexDataIdx = -1;
			Character[ctrl].texDataIdx = -1;
			Character[ctrl].texelDataIdx = -1;
			Character[ctrl].plttDataIdx = -1;
			Character[ctrl].chainTexDataIdx = -1;
			Character[ctrl].loadFlag = (uint)LOAD_FLAG_MASK;
			Character[ctrl].name = "";
		}

		public bool isValidCharacter(int ctrl)
		{
			if (ctrl >= 0 && ctrl < 22)
			{
				return (Character[ctrl].flag & 1) != 0;
			}
			return false;
		}

		public int searchCharacterIndex()
		{
			for (int i = 0; i < 22; i++)
			{
				if (Character[i].flag == 0)
				{
					return i;
				}
			}
			return -1;
		}

		public void setupCharacter(int ctrl)
		{
			Character[ctrl].loadFlag |= 1u;
			Character[ctrl].loadFlag |= 2u;
			Character[ctrl].modelSet.setup(objectDataMng.ObjectData[Character[ctrl].objDataIdx].MdlData.getAddr());
			Character[ctrl].RdrObject.setup(Character[ctrl].modelSet.getMdlResource());
			Character[ctrl].RdrObject.setBoundingBox(Character[ctrl].modelSet.getBoundingBox());
			if (strcmp(objectDataMng.ObjectData[Character[ctrl].objDataIdx].name, "o064") == 0)
			{
				Character[ctrl].RdrObject.setPriority(1);
			}
			Character[ctrl].ShadowObject.initialize();
			Character[ctrl].ShadowObject.setup(shadow[1].getMdlResource(), Character[ctrl].RdrObject);
			Character[ctrl].ShadowObject.setType(ds.sys3d.SHADOW_TYPE.SHADOW_TYPE_POLYGON);
			if (!Character[ctrl].modelSet.hasMdlTex() && -1 != Character[ctrl].orgTexDataIdx)
			{
				if (textureDataMng.getTex(Character[ctrl].orgTexDataIdx).isLoadedToVram())
				{
					Character[ctrl].modelSet.bindReplaceTex(textureDataMng.getTex(Character[ctrl].orgTexDataIdx));
				}
				else
				{
					Character[ctrl].loadFlag &= 4294967293u;
				}
			}
			Character[ctrl].motSet.setup(Character[ctrl].modelSet.getMdlResource());
			Character[ctrl].motSet.addRenderObject(Character[ctrl].RdrObject.getRenderObject());
			if (objectDataMng.ObjectData[Character[ctrl].objDataIdx].AnmData.getAddr() != null)
			{
				NNSG3dResTex texResData = NNS_G3dGetTex(Character[ctrl].modelSet.getMdlData());
				Character[ctrl].animSet.setup(objectDataMng.ObjectData[Character[ctrl].objDataIdx].AnmData.getAddr(), Character[ctrl].modelSet.getMdlResource(), texResData);
				Character[ctrl].animSet.addRenderObject(Character[ctrl].RdrObject.getRenderObject());
				Character[ctrl].animSet.setLoop(b: true, ds.sys3d.CAnimSet.enTYPE.enTYPE_END);
				Character[ctrl].animSet.start(0, ds.sys3d.CAnimSet.enTYPE.enTYPE_END);
			}
			pScene[(int)Character[ctrl].priScn].addRenderObject(Character[ctrl].RdrObject, ds.sys3d.Scene.LAYER_BOTTOM);
			pScene[(int)Character[ctrl].priScn].addRenderObject(Character[ctrl].ShadowObject, 1);
			if (pLight != null)
			{
				Character[ctrl].RdrObject.setLightObject(pLight);
			}
		}

		public bool isLoadedObject(int ctrl)
		{
			if (!isValidCharacter(ctrl))
			{
				return false;
			}
			return (1 & Character[ctrl].loadFlag) != 0;
		}

		public bool isLoadedOrgTex(int ctrl)
		{
			if (!isValidCharacter(ctrl))
			{
				return false;
			}
			return (2 & Character[ctrl].loadFlag) != 0;
		}

		public bool isLoadedMotion(int ctrl)
		{
			if (!isValidCharacter(ctrl))
			{
				return false;
			}
			return (4 & Character[ctrl].loadFlag) != 0;
		}

		public bool isLoadedReplaceTex(int ctrl)
		{
			if (!isValidCharacter(ctrl))
			{
				return false;
			}
			return (8 & Character[ctrl].loadFlag) != 0;
		}

		public bool isLoadedReplaceTexel(int ctrl)
		{
			if (!isValidCharacter(ctrl))
			{
				return false;
			}
			return (0x10 & Character[ctrl].loadFlag) != 0;
		}

		public bool isLoadedReplacePltt(int ctrl)
		{
			if (!isValidCharacter(ctrl))
			{
				return false;
			}
			return (0x20 & Character[ctrl].loadFlag) != 0;
		}

		public bool setupObject(int ctrl)
		{
			if (!isValidCharacter(ctrl))
			{
				return true;
			}
			if ((1 & Character[ctrl].loadFlag) == 0 && objectDataMng.isLoaded(Character[ctrl].objDataIdx))
			{
				setupCharacter(ctrl);
				setHidden(ctrl, (Character[ctrl].flag & 8) != 0);
				if (Character[ctrl].shadowOff)
				{
					Character[ctrl].ShadowObject.setEnable(b: false);   // PORT (FF4): see shadowOff
				}
				if (Character[ctrl].shadowJoint != null)
				{
					Character[ctrl].ShadowObject.setJointName(Character[ctrl].shadowJoint);   // PORT (FF4): see shadowJoint
					Character[ctrl].RdrObject.reserveToGetJntMtx(Character[ctrl].shadowJoint);
				}
				return true;
			}
			return false;
		}

		public bool setupOrgTex(int ctrl)
		{
			if (!isValidCharacter(ctrl))
			{
				return true;
			}
			if ((2 & Character[ctrl].loadFlag) == 0 && textureDataMng.getTex(Character[ctrl].orgTexDataIdx).isLoadedToVram() && (1 & Character[ctrl].loadFlag) != 0)
			{
				Character[ctrl].modelSet.bindReplaceTex(textureDataMng.getTex(Character[ctrl].orgTexDataIdx));
				Character[ctrl].loadFlag |= 2u;
				return true;
			}
			return false;
		}

		public bool setupMotion(int ctrl)
		{
			if (!isValidCharacter(ctrl))
			{
				return true;
			}
			if ((4 & Character[ctrl].loadFlag) == 0)
			{
				bool flag = false;
				for (byte b = 0; b < 8; b++)
				{
					if (-1 != Character[ctrl].motDataIdx[b])
					{
						if (motionDataMng.isLoaded(Character[ctrl].motDataIdx[b]) && Character[ctrl].motLoadFlag[b] == 0)
						{
							Character[ctrl].motSet.addMotion(motionDataMng.MotionData[Character[ctrl].motDataIdx[b]].motData.getAddr<ds.sys3d.ncap.SMotionFileHeader>());
							Character[ctrl].motLoadFlag[b] = 1;
						}
						else if (!motionDataMng.isLoaded(Character[ctrl].motDataIdx[b]))
						{
							flag = true;
						}
					}
				}
				if (!flag)
				{
					Character[ctrl].loadFlag |= 4u;
					return true;
				}
			}
			return false;
		}

		public bool setupReplaceTex(int ctrl)
		{
			if (!isValidCharacter(ctrl))
			{
				return true;
			}
			if ((8 & Character[ctrl].loadFlag) == 0)
			{
				int texDataIdx = Character[ctrl].texDataIdx;
				if (textureDataMng.getTex(Character[ctrl].texDataIdx).isLoadedToVram() && (1 & Character[ctrl].loadFlag) != 0)
				{
					Character[ctrl].modelSet.bindReplaceTex(textureDataMng.getTex(texDataIdx));
					Character[ctrl].loadFlag |= 8u;
					return true;
				}
			}
			return false;
		}

		public bool setupReplaceTexel(int ctrl)
		{
			if (!isValidCharacter(ctrl))
			{
				return true;
			}
			if ((0x10 & Character[ctrl].loadFlag) == 0)
			{
				int texelDataIdx = Character[ctrl].texelDataIdx;
				if (textureDataMng.isLoaded(texelDataIdx))
				{
					Character[ctrl].modelSet.bindReplaceTexel(textureDataMng.getTex(texelDataIdx));
					Character[ctrl].loadFlag |= 16u;
					return true;
				}
			}
			return false;
		}

		public bool setupReplacePltt(int ctrl)
		{
			if (!isValidCharacter(ctrl))
			{
				return true;
			}
			if ((0x20 & Character[ctrl].loadFlag) == 0)
			{
				int plttDataIdx = Character[ctrl].plttDataIdx;
				if (textureDataMng.getTex(Character[ctrl].plttDataIdx).isLoadedToVram() && (1 & Character[ctrl].loadFlag) != 0 && (8 & Character[ctrl].loadFlag) != 0 && (2 & Character[ctrl].loadFlag) != 0)
				{
					Character[ctrl].modelSet.bindReplacePltt(textureDataMng.getTex(plttDataIdx));
					Character[ctrl].loadFlag |= 32u;
					return true;
				}
			}
			return false;
		}

		public void setBank()
		{
			if (ds.CVram.getInstance().getBankForTex() != GX_GetBankForTex())
			{
				ds.CVram.getInstance().setupBankForTex();
			}
			if (ds.CVram.getInstance().getBankForPltt() != GX_GetBankForTexPltt())
			{
				ds.CVram.getInstance().setupBankForPltt();
			}
		}

		public static void setGlobalLight(ds.sys3d.CLightObject pL)
		{
			pLight = pL;
		}
	}
}
