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
		public class Sprite
		{
			public static uint spFLAG_NOT_SHOW = 1u;

			public static uint spFLAG_PRIORITY = 2u;

			public static uint spFLAG_STOP_ANIMATION = 4u;

			public static uint spFLAG_AUTO_DELETE = 8u;

			public static uint spFLAG_FIX_CELL = 16u;

			public static uint spFLAG_CHANGE_PALETTE = 32u;

			public static uint spFLAG_MASK = 65535u;

			protected Nclr m_Nclr = new Nclr();

			protected Ncer m_Ncer = new Ncer();

			protected Nanr m_Nanr = new Nanr();

			protected NNSG2dCellData m_pCell;

			protected uint m_Flag;

			protected int m_Depth;

			protected byte m_Plane;

			protected byte m_Priority;

			protected byte m_PolygonID;

			protected byte m_Alpha;

			protected ushort m_Rotation;

			protected ushort m_Palette;

			protected NNSG2dFVec2 m_Position = new NNSG2dFVec2();

			protected NNSG2dFVec2 m_Scale = new NNSG2dFVec2();

			protected NNSG2dSVec2 m_PositionI = new NNSG2dSVec2();

			protected uint m_Color;

			protected NNSG2dImageProxy m_ImageProxy = new NNSG2dImageProxy();

			protected NNSG2dImagePaletteProxy m_PaletteProxy = new NNSG2dImagePaletteProxy();

			public int m_iIndex;

			public Sprite()
			{
				m_Flag = 0u;
				m_Position.x = ds.S32toFX32(240);
				m_Position.y = ds.S32toFX32(160);
				m_Scale.x = (m_Scale.y = ds.S32toFX32(1));
				m_Rotation = 0;
				m_Depth = 0;
				m_Priority = 0;
				m_PolygonID = 0;
				m_Alpha = 31;
				m_Color = 16777215u;
				NNS_G2dInitImageProxy(m_ImageProxy);
				NNS_G2dInitImagePaletteProxy(m_PaletteProxy);
				SetShow(show: true);
			}

			~Sprite()
			{
			}

			public void destruct()
			{
			}

			public void LoadCe(string pCe)
			{
				if (pCe != null)
				{
					m_Ncer.Load(pCe);
					// PORT: Steam's cell banks are laid out for another screen; the phone
					// geometry replaces them, mapped onto whatever sheet was just loaded.
					OpenFF.Client.SteamCells.Apply(pCe, m_Ncer.pDataCe());
					OpenFF.Client.Ff4Assets.RemapCells(pCe, m_Ncer.pDataCe());
					m_pCell = NNS_G2dGetCellDataByIdx(m_Ncer.pDataCe(), 0);
				}
			}

			public void LoadAn(string pAn)
			{
				if (pAn != null)
				{
					m_Nanr.Load(pAn);
					NNS_G2dInitCellAnimation(m_Nanr.GetCellAnimation(), NNS_G2dGetAnimSequenceByIdx(m_Nanr.pDataAn(), 0), m_Ncer.pDataCe());
				}
			}

			public void SetCell(ushort index)
			{
				m_pCell = NNS_G2dGetCellDataByIdx(m_Ncer.pDataCe(), index);
				if (GetCellAnimation() != null)
				{
					SetFixCell(fix: true);
					m_Nanr.GetCellAnimation().pCurrentCell = m_pCell;
				}
			}

			public NNSG2dCellAnimation GetCellAnimation()
			{
				if (IsFixCell())
				{
					m_Nanr.GetCellAnimation().pCurrentCell = m_pCell;
				}
				if (m_Nanr.pDataAn() != null)
				{
					return m_Nanr.GetCellAnimation();
				}
				return null;
			}

			public void SetPosition(NNSG2dFVec2 xy)
			{
				m_Position.copy(xy);
			}

			public void SetPositionF(int x, int y)
			{
				m_Position.x = x;
				m_Position.y = y;
			}

			public void SetPositionI(int x, int y)
			{
				SetPositionF(ds.S32toFX32(x), ds.S32toFX32(y));
			}

			public NNSG2dFVec2 GetPosition()
			{
				return m_Position;
			}

			public NNSG2dSVec2 GetPositionI()
			{
				m_PositionI.x = (short)FX_Whole(m_Position.x);
				m_PositionI.y = (short)FX_Whole(m_Position.y);
				return m_PositionI;
			}

			public void SetScale(NNSG2dFVec2 xy)
			{
				m_Scale.copy(xy);
			}

			public void SetScaleF(int x, int y)
			{
				m_Scale.x = x;
				m_Scale.y = y;
			}

			public NNSG2dFVec2 GetScale()
			{
				return m_Scale;
			}

			public void SetPlane(DS2D_OBJ_PLANE plane)
			{
				m_Plane = (byte)plane;
			}

			public DS2D_OBJ_PLANE GetPlane()
			{
				return static_cast<DS2D_OBJ_PLANE>((int)m_Plane);
			}

			public void SetRotation(ushort rot)
			{
				m_Rotation = rot;
			}

			public ushort GetRotation()
			{
				return m_Rotation;
			}

			public void SetShow(bool show)
			{
				ds.switchFlag(spFLAG_NOT_SHOW, !show, ref m_Flag);
			}

			public bool IsShow()
			{
				return !ds.isFlag(spFLAG_NOT_SHOW, ref m_Flag);
			}

			public void SetPriority(byte pri)
			{
				ds.onFlag(spFLAG_PRIORITY, ref m_Flag);
				m_Priority = pri;
			}

			public byte GetPriority()
			{
				return m_Priority;
			}

			public bool IsPriority()
			{
				return ds.isFlag(spFLAG_PRIORITY, ref m_Flag);
			}

			public void InvalidatePriority()
			{
				ds.offFlag(spFLAG_PRIORITY, ref m_Flag);
			}

			public void SetPolygonID(byte _id)
			{
				m_PolygonID = _id;
			}

			public byte GetPolygonID()
			{
				return m_PolygonID;
			}

			public void SetAlpha(byte alpha)
			{
				m_Alpha = alpha;
			}

			public byte GetAlpha()
			{
				return m_Alpha;
			}

			public void SetColor(uint color)
			{
				m_Color = color;
			}

			public uint GetColor()
			{
				return m_Color;
			}

			public void SetPalette(ushort plt)
			{
				ds.onFlag(spFLAG_CHANGE_PALETTE, ref m_Flag);
				m_Palette = plt;
			}

			public ushort GetPalette()
			{
				return m_Palette;
			}

			public bool IsChangePalette()
			{
				return ds.isFlag(spFLAG_CHANGE_PALETTE, ref m_Flag);
			}

			public void InvalidateChangePalette()
			{
				ds.offFlag(spFLAG_CHANGE_PALETTE, ref m_Flag);
			}

			public void SetDepth(int depth)
			{
				m_Depth = depth;
			}

			public int GetDepth()
			{
				return m_Depth;
			}

			public void SetAutoDelete(bool auto_delete)
			{
				ds.switchFlag(spFLAG_AUTO_DELETE, auto_delete, ref m_Flag);
			}

			public bool IsAutoDelete()
			{
				return ds.isFlag(spFLAG_AUTO_DELETE, ref m_Flag);
			}

			public void SetAnimation(bool anm)
			{
				ds.switchFlag(spFLAG_STOP_ANIMATION, !anm, ref m_Flag);
			}

			public bool IsAnimation()
			{
				return !ds.isFlag(spFLAG_STOP_ANIMATION, ref m_Flag);
			}

			public void PlayAnimation(ushort index, NNSG2dAnimationPlayMode mode)
			{
				m_Nanr.Play(index, mode);
			}

			public void UpdateAnimation()
			{
				m_Nanr.Update();
			}

			public void SetFixCell(bool fix)
			{
				ds.switchFlag(spFLAG_FIX_CELL, fix, ref m_Flag);
			}

			public bool IsFixCell()
			{
				return ds.isFlag(spFLAG_FIX_CELL, ref m_Flag);
			}

			public virtual void Load(DS2D_OBJ_PLANE arg0, string arg1, string arg2, string arg3, string arg4)
			{
			}

			public virtual void Load2(DS2D_OBJ_PLANE arg0, string arg1)
			{
			}

			public virtual void Release()
			{
			}

			public Ncer GetCe()
			{
				return m_Ncer;
			}

			public Nanr GetAn()
			{
				return m_Nanr;
			}

			public Nclr GetCl()
			{
				return m_Nclr;
			}

			public NNSG2dCellData GetCellData()
			{
				if (!IsFixCell() && m_Nanr.pDataAn() != null)
				{
					return m_Nanr.GetCellAnimation().pCurrentCell;
				}
				return m_pCell;
			}

			public NNSG2dCellDataBank GetCellBank()
			{
				return m_Ncer.pDataCe();
			}

			public NNSG2dImageProxy GetImageProxy()
			{
				return m_ImageProxy;
			}

			public NNSG2dImagePaletteProxy GetPaletteProxy()
			{
				return m_PaletteProxy;
			}

			public void copy(Sprite src)
			{
				m_Nclr.copy(src.m_Nclr);
				m_Ncer.copy(src.m_Ncer);
				m_Nanr.copy(src.m_Nanr);
				m_pCell = src.m_pCell;
				m_Flag = src.m_Flag;
				m_Depth = src.m_Depth;
				m_Plane = src.m_Plane;
				m_Priority = src.m_Priority;
				m_PolygonID = src.m_PolygonID;
				m_Alpha = src.m_Alpha;
				m_Rotation = src.m_Rotation;
				m_Palette = src.m_Palette;
				m_Position.copy(src.m_Position);
				m_Scale.copy(src.m_Scale);
				m_Color = src.m_Color;
				m_ImageProxy.copy(src.m_ImageProxy);
				m_PaletteProxy.copy(src.m_PaletteProxy);
			}
		}
	}
}
