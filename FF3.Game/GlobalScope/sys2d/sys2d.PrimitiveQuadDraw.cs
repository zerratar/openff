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
	public static partial class sys2d
	{
		public class PrimitiveQuadDraw : PrimitiveDraw
		{
			private ds.Vector2<short> _topleft = new ds.Vector2<short>();

			private ds.Vector2<short> _bottomright = new ds.Vector2<short>();

			private int _depth;

			private ushort _colTopleft;

			private ushort _colTopright;

			private ushort _colBottomleft;

			private ushort _colBottomright;

			private byte _alpha;

			private byte _polyID;

			public PrimitiveQuadDraw()
			{
				_topleft.set(0, 0);
				_bottomright.set(0, 0);
				_depth = -4177920;
				_alpha = 31;
				_colTopleft = ds.setGXRgb(31, 31, 31);
				_colTopright = ds.setGXRgb(31, 31, 31);
				_colBottomleft = ds.setGXRgb(31, 31, 31);
				_colBottomright = ds.setGXRgb(31, 31, 31);
				_polyID = 0;
			}

			public override void draw()
			{
				G3_PushMtx();
				G3_Ortho(4096 * (320 - LCD_HEIGHT) / 2, 4096 * (320 + LCD_HEIGHT) / 2, 4096 * (480 - LCD_WIDTH) / 2, 4096 * (480 + LCD_WIDTH) / 2, -4194304, 4194304, null);
				VecFx32 fnd_reuse_pos = GlobalScope.fnd_reuse_pos;
				fnd_reuse_pos.x = 4096 * (_bottomright.vx - _topleft.vx);
				fnd_reuse_pos.y = 4096 * (_bottomright.vy - _topleft.vy);
				G3_MtxMode(GXMtxMode.GX_MTXMODE_POSITION_VECTOR);
				G3_Identity();
				G3_Translate(_topleft.vx * 4096, _topleft.vy * 4096, _depth);
				G3_Scale(fnd_reuse_pos.x, fnd_reuse_pos.y, 4096);
				G3_PolygonAttr(0, GXPolygonMode.GX_POLYGONMODE_MODULATE, GXCull.GX_CULL_NONE, _polyID, _alpha, 0);
				G3_TexImageParam(GXTexFmt.GX_TEXFMT_NONE, 0, GXTexSizeS.GX_TEXSIZE_S8, GXTexSizeT.GX_TEXSIZE_T8, 0, 0, 0, null);
				G3_Begin(GXBegin.GX_BEGIN_QUADS);
				G3_Color(_colTopleft);
				G3_Vtx(0, 0, 0);
				G3_Color(_colBottomleft);
				G3_VtxXY(0, 4096);
				G3_Color(_colBottomright);
				G3_VtxXY(4096, 4096);
				G3_Color(_colTopright);
				G3_VtxXY(4096, 0);
				G3_End();
				G3_PopMtx(1);
			}

			public void setPosition(ds.Vector2<short> topleft, ds.Vector2<short> bottomright)
			{
				_topleft.copy(topleft);
				_bottomright.copy(bottomright);
			}

			public void getPosition(ds.Vector2<short> pTopleft, ds.Vector2<short> pBottomright)
			{
				pTopleft?.copy(_topleft);
				pBottomright?.copy(_bottomright);
			}

			public void setColor(ushort topleft, ushort topright, ushort bottomleft, ushort bottomright)
			{
				_colTopleft = topleft;
				_colTopright = topright;
				_colBottomleft = bottomleft;
				_colBottomright = bottomright;
			}

			public void getColor(ref ushort pTopleft, ref ushort pTopright, ref ushort pBottomleft, ref ushort pBottomright)
			{
				pTopleft = _colTopleft;
				pTopright = _colTopright;
				pBottomleft = _colBottomleft;
				pBottomright = _colBottomright;
			}

			public void setDepth(int depth)
			{
				_depth = depth;
			}

			public int getDepth()
			{
				return _depth;
			}

			public void setAlpha(byte alpha)
			{
				_alpha = alpha;
			}

			public byte getAlpha()
			{
				return _alpha;
			}

			public void setPolygonID(byte _id)
			{
				_polyID = _id;
			}

			public byte getPolygonID()
			{
				return _polyID;
			}
		}
	}
}
