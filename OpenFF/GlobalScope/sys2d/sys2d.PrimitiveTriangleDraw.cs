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
	public static partial class sys2d
	{
		public class PrimitiveTriangleDraw : PrimitiveDraw
		{
			private VecFx16[] _pos = new VecFx16[3];

			private ushort[] _col = new ushort[3];

			private int _depth;

			private byte _alpha;

			private byte _polyID;

			public PrimitiveTriangleDraw()
			{
				for (byte b = 0; b < 3; b++)
				{
					_pos[b].x = 0;
					_pos[b].y = 0;
					_pos[b].z = 0;
					_col[b] = ds.setGXRgb(31, 31, 31);
				}
				_depth = -4194304;
				_alpha = 31;
				_polyID = 0;
			}

			public override void draw()
			{
				G3_PushMtx();
				G3_Ortho(4096 * -LCD_HEIGHT / 2, 4096 * LCD_HEIGHT / 2, 4096 * -LCD_WIDTH / 2, 4096 * LCD_WIDTH / 2, -4194304, 4194304, null);
				VecFx32 fnd_reuse_pos = GlobalScope.fnd_reuse_pos;
				fnd_reuse_pos.x = 4096 * LCD_WIDTH / 2;
				fnd_reuse_pos.y = 4096 * LCD_HEIGHT / 2;
				fnd_reuse_pos.z = 4096;
				G3_MtxMode(GXMtxMode.GX_MTXMODE_POSITION_VECTOR);
				G3_Identity();
				G3_Scale(fnd_reuse_pos.x, fnd_reuse_pos.y, fnd_reuse_pos.z);
				G3_PolygonAttr(0, GXPolygonMode.GX_POLYGONMODE_MODULATE, GXCull.GX_CULL_NONE, _polyID, _alpha, 0);
				G3_TexImageParam(GXTexFmt.GX_TEXFMT_NONE, 0, GXTexSizeS.GX_TEXSIZE_S8, GXTexSizeT.GX_TEXSIZE_T8, 0, 0, 0, null);
				G3_Begin(GXBegin.GX_BEGIN_TRIANGLES);
				for (byte b = 0; b < 3; b++)
				{
					G3_Color(_col[b]);
					G3_Vtx(_pos[b].x, _pos[b].y, _pos[b].z);
				}
				G3_End();
				G3_PopMtx(1);
			}

			public void setPosition(ds.Vector2<short> v0, ds.Vector2<short> v1, ds.Vector2<short> v2)
			{
				_pos[0].x = (short)FX_Div(4096 * (v0.vx - SCREEN_HALF_WIDTH), 4096 * SCREEN_HALF_WIDTH);
				_pos[0].y = (short)FX_Div(4096 * (v0.vy - SCREEN_HALF_HEIGHT), 4096 * SCREEN_HALF_HEIGHT);
				_pos[0].z = 0;
				_pos[1].x = (short)FX_Div(4096 * (v1.vx - SCREEN_HALF_WIDTH), 4096 * SCREEN_HALF_WIDTH);
				_pos[1].y = (short)FX_Div(4096 * (v1.vy - SCREEN_HALF_HEIGHT), 4096 * SCREEN_HALF_HEIGHT);
				_pos[1].z = 0;
				_pos[2].x = (short)FX_Div(4096 * (v2.vx - SCREEN_HALF_WIDTH), 4096 * SCREEN_HALF_WIDTH);
				_pos[2].y = (short)FX_Div(4096 * (v2.vy - SCREEN_HALF_HEIGHT), 4096 * SCREEN_HALF_HEIGHT);
				_pos[2].z = 0;
			}

			public void getPosition(ds.Vector2<short> pv0, ds.Vector2<short> pv1, ds.Vector2<short> pv2)
			{
				if (pv0 != null)
				{
					pv0.vx = (short)((short)(FX_Mul(_pos[0].x, 4096 * SCREEN_HALF_WIDTH) / 4096) + SCREEN_HALF_WIDTH);
					pv0.vy = (short)((short)(FX_Mul(_pos[0].y, 4096 * SCREEN_HALF_HEIGHT) / 4096) + SCREEN_HALF_HEIGHT);
				}
				if (pv1 != null)
				{
					pv1.vx = (short)((short)(FX_Mul(_pos[1].x, 4096 * SCREEN_HALF_WIDTH) / 4096) + SCREEN_HALF_WIDTH);
					pv1.vy = (short)((short)(FX_Mul(_pos[1].y, 4096 * SCREEN_HALF_HEIGHT) / 4096) + SCREEN_HALF_HEIGHT);
				}
				if (pv2 != null)
				{
					pv2.vx = (short)((short)(FX_Mul(_pos[2].x, 4096 * SCREEN_HALF_WIDTH) / 4096) + SCREEN_HALF_WIDTH);
					pv2.vy = (short)((short)(FX_Mul(_pos[2].y, 4096 * SCREEN_HALF_HEIGHT) / 4096) + SCREEN_HALF_HEIGHT);
				}
			}

			public void setColor(ushort c0, ushort c1, ushort c2)
			{
				_col[0] = c0;
				_col[1] = c1;
				_col[2] = c2;
			}

			public void getColor(ushort[] pc0, ushort[] pc1, ushort[] pc2)
			{
				if (pc0 != null)
				{
					pc0[0] = _col[0];
				}
				if (pc1 != null)
				{
					pc1[0] = _col[1];
				}
				if (pc2 != null)
				{
					pc2[0] = _col[2];
				}
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
