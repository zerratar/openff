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
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
	public static partial class sys2d
	{
		public class DS2DManager
		{
			private class Comparer<T> : IComparer<T> where T : Sprite
			{
				public int Compare(T p1, T p2)
				{
					int num = p2.GetDepth() - p1.GetDepth();
					if (num != 0)
					{
						return num;
					}
					return p1.m_iIndex - p2.m_iIndex;
				}
			}

			public class __DrawSpriteData
			{
				private ushort Start;

				private ushort Number;
			}

			public static DS2DManager g_DS2DManagerInstance = new DS2DManager();

			private static Sprite[] sys2d_reuse_sprite = new Sprite[512];

			private static Comparer<Sprite> m_Comparer = new Comparer<Sprite>();

			private static VecFx32 sys2d_reuse_Eye = new VecFx32(0, 0, 0);

			private static VecFx32 sys2d_reuse_vUp = new VecFx32(0, 4096, 0);

			private static VecFx32 sys2d_reuse_at = new VecFx32(0, 0, -4096);

			private ds.SLList<Bg> _BgList;

			private ds.SLNode<Bg>[] _BgNode = new ds.SLNode<Bg>[DS2D_BG_MAX];

			private ds.SLList<Sprite> _SpriteList = new ds.SLList<Sprite>();

			private ds.SLNode<Sprite>[] _SpriteNode = new ds.SLNode<Sprite>[DS2D_SPRITE_MAX];

			private DS2DObj[] _Obj = new DS2DObj[3];

			private __DrawSpriteData[] _DrawSpriteData = new __DrawSpriteData[6];

			public DS2DManager()
			{
				for (int i = 0; i < _Obj.Length; i++)
				{
					_Obj[i] = new DS2DObj();
				}
				for (int i = 0; i < _SpriteNode.Length; i++)
				{
					_SpriteNode[i] = new ds.SLNode<Sprite>();
				}
			}

			public static int CallBackAddOamMainObj(GXOamAttr pOam, ushort affine_index, int double_affine)
			{
				return NNS_G2dEntryOamManagerOamWithAffineIdx(g_DS2DManagerInstance._Obj[1].GetOamManager(), pOam, affine_index);
			}

			public static int CallBackAddOamSubObj(GXOamAttr pOam, ushort affine_index, int double_affine)
			{
				return NNS_G2dEntryOamManagerOamWithAffineIdx(g_DS2DManagerInstance._Obj[2].GetOamManager(), pOam, affine_index);
			}

			public static ushort CallBackAddAffineMainObj(MtxFx22 pMtx)
			{
				return NNS_G2dEntryOamManagerAffine(g_DS2DManagerInstance._Obj[1].GetOamManager(), pMtx);
			}

			public static ushort CallBackAddAffineSubObj(MtxFx22 pMtx)
			{
				return NNS_G2dEntryOamManagerAffine(g_DS2DManagerInstance._Obj[2].GetOamManager(), pMtx);
			}

			public void d2dInitialize()
			{
				int num = 1;
				NNS_G2dInitOamManagerModule();
				DS2DObj dS2DObj = _Obj[0];
				dS2DObj.ClearCgClOffset();
				dS2DObj.InitializeRenderer(DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN3D);
				dS2DObj = _Obj[1];
				num &= NNS_G2dGetNewOamManagerInstanceAsFastTransferMode(dS2DObj.GetOamManager(), 0, (ushort)DS2D_MAIN_CELL_MAX, NNSG2dOamType.NNS_G2D_OAMTYPE_MAIN);
				dS2DObj.ClearCgClOffset();
				dS2DObj.InitializeRenderer(DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN2D);
				dS2DObj = _Obj[2];
				num &= NNS_G2dGetNewOamManagerInstanceAsFastTransferMode(dS2DObj.GetOamManager(), 0, (ushort)DS2D_SUB_CELL_MAX, NNSG2dOamType.NNS_G2D_OAMTYPE_SUB);
				dS2DObj.ClearCgClOffset();
				dS2DObj.InitializeRenderer(DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_SUB2D);
				d2dClearSprite();
			}

			public void d2dExecute()
			{
			}

			public void d2dDraw()
			{
				DS2DObj dS2DObj = _Obj[1];
				NNS_G2dApplyOamManagerToHW(dS2DObj.GetOamManager());
				NNS_G2dResetOamManagerBuffer(dS2DObj.GetOamManager());
				dS2DObj = _Obj[2];
				NNS_G2dApplyOamManagerToHW(dS2DObj.GetOamManager());
				NNS_G2dResetOamManagerBuffer(dS2DObj.GetOamManager());
			}

			public void d2dDrawScreen(bool depthtest)
			{
				d2dBeginRegistrationSprite();
				Sprite[] array = sys2d_reuse_sprite;
				int num = 0;
				for (ds.SLNode<Sprite> sLNode = _SpriteList.front(); sLNode != null; sLNode = sLNode.next())
				{
					array[num] = sLNode.data();
					array[num].m_iIndex = num;
					num++;
				}
				Array.Sort(array, 0, (int)_SpriteList.size(), m_Comparer);
				NNS_G2dResetMatrix(depthtest);
				for (int num2 = 3; num2 >= 0; num2--)
				{
					NNS_G2dDrawBG(num2);
					NNS_G2dDrawText(num2);
					for (int i = 0; i < _SpriteList.size(); i++)
					{
						if (array[i].GetPriority() == num2)
						{
							d2dRegisterSprite(array[i]);
						}
					}
				}
				array = null;
				d2dFinishRegistrationSprite();
				NNS_G2dResetMatrix(depthtest: false);
			}

			public void d2dUpdate()
			{
				ds.SLNode<Sprite> sLNode = _SpriteList.front();
				while (sLNode != null)
				{
					Sprite sprite = sLNode.data();
					if (sprite.GetCellAnimation() != null)
					{
						NNSG2dAnimController nNSG2dAnimController = NNS_G2dGetCellAnimationAnimCtrl(const_cast<NNSG2dCellAnimation>(sprite.GetCellAnimation()));
						if (sprite.IsAutoDelete() && nNSG2dAnimController.bActive == 0)
						{
							sLNode = sLNode.next();
							sprite.Release();
							d2dDeleteSprite(sprite);
							continue;
						}
					}
					if (sprite.IsAnimation())
					{
						sprite.UpdateAnimation();
					}
					sLNode = sLNode.next();
				}
			}

			public bool d2dAddBg(Bg bg)
			{
				if (_BgList.size() + 1 >= DS2D_BG_MAX)
				{
					return false;
				}
				int i;
				for (i = 0; i < DS2D_BG_MAX && _BgNode[i].data() != null; i++)
				{
				}
				ds.SLNode<Bg> sLNode = _BgNode[i];
				sLNode.setData(bg);
				_BgList.insertFront(new ds.SLNode<Bg>[1] { sLNode }, 1u);
				return true;
			}

			public bool d2dDeleteBg(Bg bg)
			{
				for (int i = 0; i < _BgList.size(); i++)
				{
					ds.SLNode<Bg> sLNode = _BgList.get(i);
					if (sLNode.data() == bg)
					{
						_BgList.erase(sLNode);
						sLNode.setData(null);
						return true;
					}
				}
				return false;
			}

			public void d2dBeginRegistrationSprite()
			{
				VecFx32 camPos = sys2d_reuse_Eye;
				VecFx32 camUp = sys2d_reuse_vUp;
				VecFx32 target = sys2d_reuse_at;
				G3_LookAt(camPos, camUp, target, null);
				NNS_G2dSetupSoftwareSpriteCamera();
				G3_MaterialColorDiffAmb(GX_RGB(31, 31, 31), GX_RGB(16, 16, 16), 1);
				G3_MaterialColorSpecEmi(GX_RGB(16, 16, 16), GX_RGB(0, 0, 0), 0);
			}

			public void d2dFinishRegistrationSprite()
			{
			}

			public bool d2dRegisterSprite(Sprite sp)
			{
				if (!sp.IsShow())
				{
					return false;
				}
				ushort rotation = sp.GetRotation();
				DS2DObj dS2DObj = _Obj[(int)sp.GetPlane()];
				NNSG2dRendererInstance renderer = dS2DObj.GetRenderer();
				NNS_G2dSetRendererImageProxy(renderer, sp.GetImageProxy(), sp.GetPaletteProxy());
				bool flag = sp.GetScale().x != ds.S32toFX32(1) || sp.GetScale().y != ds.S32toFX32(1);
				if (rotation == 0 && !flag)
				{
					NNS_G2dBeginRenderingEx(renderer, 1u);
				}
				else
				{
					NNS_G2dBeginRenderingEx(renderer, 0u);
				}
				NNS_G2dPushMtx();
				if (sp.IsPriority())
				{
					NNS_G2dSetRendererOverwritePriority(renderer, sp.GetPriority());
					NNS_G2dSetRendererOverwriteEnable(renderer, NNSG2dRendererOverwriteParam.NNS_G2D_RND_OVERWRITE_PRIORITY);
				}
				else
				{
					NNS_G2dSetRendererOverwriteDisable(renderer, NNSG2dRendererOverwriteParam.NNS_G2D_RND_OVERWRITE_PRIORITY);
				}
				if (sp.IsChangePalette())
				{
					NNS_G2dSetRendererOverwritePlttNo(renderer, sp.GetPalette());
					NNS_G2dSetRendererOverwriteEnable(renderer, NNSG2dRendererOverwriteParam.NNS_G2D_RND_OVERWRITE_PLTTNO);
				}
				else
				{
					NNS_G2dSetRendererOverwriteDisable(renderer, NNSG2dRendererOverwriteParam.NNS_G2D_RND_OVERWRITE_PLTTNO);
				}
				NNS_G2dTranslate(sp.GetPosition().x, sp.GetPosition().y, 0);
				if (rotation != 0 || flag)
				{
					NNS_G2dRotZ(FX_SinIdx(rotation), FX_CosIdx(rotation));
					NNS_G2dScale(sp.GetScale().x, sp.GetScale().y, 4096);
				}
				NNS_G2dSetRendererSpriteZoffset(renderer, -4);
				G3_PolygonAttr(0, GXPolygonMode.GX_POLYGONMODE_MODULATE, GXCull.GX_CULL_NONE, sp.GetPolygonID(), sp.GetAlpha(), 0);
				uint color = sp.GetColor();
				NNS_G3dSetRenderColor(renderer, (int)(color & 0xFF), (int)((color >> 8) & 0xFF), (int)((color >> 16) & 0xFF), sp.GetAlpha() * 255 / 31);
				if (sp.GetCellAnimation() == null)
				{
					NNS_G2dDrawCell(sp.GetCellData());
				}
				else
				{
					NNS_G2dDrawCellAnimation(sp.GetCellAnimation());
				}
				NNS_G2dPopMtx();
				NNS_G2dEndRendering();
				return true;
			}

			public bool d2dAddSprite(Sprite sp)
			{
				if (_SpriteList.size() + 1 >= DS2D_SPRITE_MAX)
				{
					return false;
				}
				int i;
				for (i = 0; i < DS2D_SPRITE_MAX && _SpriteNode[i].data() != null; i++)
				{
				}
				ds.SLNode<Sprite> sLNode = _SpriteNode[i];
				sLNode.setData(sp);
				_SpriteList.insertFront(new ds.SLNode<Sprite>[1] { sLNode }, 1u);
				return true;
			}

			public bool d2dInsertSprite(Sprite target, Sprite sp)
			{
				if (_SpriteList.size() + 1 >= DS2D_SPRITE_MAX)
				{
					return false;
				}
				int i;
				for (i = 0; i < DS2D_SPRITE_MAX && _SpriteNode[i].data() != null; i++)
				{
				}
				ds.SLNode<Sprite> sLNode = _SpriteNode[i];
				sLNode.setData(sp);
				for (ds.SLNode<Sprite> sLNode2 = _SpriteList.front(); sLNode2 != null; sLNode2 = sLNode2.next())
				{
					if (sLNode2.data() == target)
					{
						_SpriteList.insert(sLNode2, new ds.SLNode<Sprite>[1] { sLNode }, 1u);
						return true;
					}
				}
				_SpriteList.insertFront(new ds.SLNode<Sprite>[1] { sLNode }, 1u);
				return true;
			}

			public bool d2dDeleteSprite(Sprite sp)
			{
				for (ds.SLNode<Sprite> sLNode = _SpriteList.front(); sLNode != null; sLNode = sLNode.next())
				{
					if (sLNode.data() == sp)
					{
						_SpriteList.erase(sLNode);
						sLNode.setData(null);
						return true;
					}
				}
				return false;
			}

			public void d2dClearSprite()
			{
				_SpriteList.initialize();
				for (int i = 0; i < DS2D_SPRITE_MAX; i++)
				{
					_SpriteNode[i].setData(null);
				}
			}

			public int d2dGetEntriedSpriteIndex(Sprite sp)
			{
				int num = 0;
				for (ds.SLNode<Sprite> sLNode = _SpriteList.front(); sLNode != null; sLNode = sLNode.next())
				{
					if (sLNode.data() == sp)
					{
						return num;
					}
					num++;
				}
				return -1;
			}

			public static DS2DManager d2dGetInstance()
			{
				return g_DS2DManagerInstance;
			}

			public DS2DObj d2dGetObjPlaneData(DS2D_OBJ_PLANE plane)
			{
				return _Obj[(int)plane];
			}

			public uint d2dGetBgNumber()
			{
				return _BgList.size();
			}

			public uint d2dGetSpriteNumber()
			{
				return _SpriteList.size();
			}

			public void d2dAddObjCgOffset(DS2D_OBJ_PLANE plane, uint ofs)
			{
				_Obj[(int)plane].AddCgOffset(ofs);
			}

			public void d2dAddObjCbOffset(DS2D_OBJ_PLANE plane, uint ofs)
			{
				d2dAddObjCgOffset(plane, ofs);
			}

			public TexVram d2dGetObjCgOffset(DS2D_OBJ_PLANE plane)
			{
				return _Obj[(int)plane].GetCgOffset();
			}

			public TexVram d2dGetObjCbOffset(DS2D_OBJ_PLANE plane)
			{
				return d2dGetObjCgOffset(plane);
			}

			public void d2dAddObjClOffset(DS2D_OBJ_PLANE plane, uint ofs)
			{
				_Obj[(int)plane].AddClOffset(ofs);
			}

			public uint d2dGetObjClOffset(DS2D_OBJ_PLANE plane)
			{
				return _Obj[(int)plane].GetClOffset();
			}
		}
	}
}
