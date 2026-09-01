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
	public static partial class dgs
	{
		public class CCurtain
		{
			public enum CURTAIN_LOCATION
			{
				LOCATION_ERROR = -1,
				LOCATION_TOP,
				LOCATION_MIDDLE,
				LOCATION_BOTTOM,
				LOCATION_MAX
			}

			public enum COLOR_STATE
			{
				COLOR_STATE_ERROR = -1,
				COLOR_STATE_CHANGED,
				COLOR_STATE_CHANGING,
				COLOR_STATE_MAX
			}

			public enum ALPHA_STATE
			{
				ALPHA_STATE_ERROR = -1,
				ALPHA_STATE_CHANGED,
				ALPHA_STATE_CHANGING,
				ALPHA_STATE_MAX
			}

			public const CURTAIN_LOCATION LOCATION_ERROR = CURTAIN_LOCATION.LOCATION_ERROR;

			public const CURTAIN_LOCATION LOCATION_TOP = CURTAIN_LOCATION.LOCATION_TOP;

			public const CURTAIN_LOCATION LOCATION_MIDDLE = CURTAIN_LOCATION.LOCATION_MIDDLE;

			public const CURTAIN_LOCATION LOCATION_BOTTOM = CURTAIN_LOCATION.LOCATION_BOTTOM;

			public const CURTAIN_LOCATION LOCATION_MAX = CURTAIN_LOCATION.LOCATION_MAX;

			public const COLOR_STATE COLOR_STATE_ERROR = COLOR_STATE.COLOR_STATE_ERROR;

			public const COLOR_STATE COLOR_STATE_CHANGED = COLOR_STATE.COLOR_STATE_CHANGED;

			public const COLOR_STATE COLOR_STATE_CHANGING = COLOR_STATE.COLOR_STATE_CHANGING;

			public const COLOR_STATE COLOR_STATE_MAX = COLOR_STATE.COLOR_STATE_MAX;

			public const ALPHA_STATE ALPHA_STATE_ERROR = ALPHA_STATE.ALPHA_STATE_ERROR;

			public const ALPHA_STATE ALPHA_STATE_CHANGED = ALPHA_STATE.ALPHA_STATE_CHANGED;

			public const ALPHA_STATE ALPHA_STATE_CHANGING = ALPHA_STATE.ALPHA_STATE_CHANGING;

			public const ALPHA_STATE ALPHA_STATE_MAX = ALPHA_STATE.ALPHA_STATE_MAX;

			public static CCurtain[] curtain = new CCurtain[4]
			{
				new CCurtain(),
				new CCurtain(),
				new CCurtain(),
				new CCurtain()
			};

			public static int DEFAULT_Z_TOP = 0;

			public static int DEFAULT_Z_MIDDLE = 0;

			public static int DEFAULT_Z_BOTTOM = 0;

			private static int FLAG_ENABLE = 1;

			private static int FLAG_VISIBLE = 2;

			private int _flag;

			private int _polygonID;

			private int _z;

			private int _alphaTime;

			private int _colorTime;

			private int _nowAlphaTime;

			private int _nowColorTime;

			private int _alpha;

			private ushort _colLT;

			private ushort _colLB;

			private ushort _colRT;

			private ushort _colRB;

			private int _startAlpha;

			private ushort _startColLT;

			private ushort _startColLB;

			private ushort _startColRT;

			private ushort _startColRB;

			private int _goalAlpha;

			private ushort _goalColLT;

			private ushort _goalColLB;

			private ushort _goalColRT;

			private ushort _goalColRB;

			private COLOR_STATE _colState;

			private ALPHA_STATE _alphaState;

			public static void initialize()
			{
				curtain[0].initValue();
				curtain[0]._z = DEFAULT_Z_TOP;
				curtain[1].initValue();
				curtain[1]._z = DEFAULT_Z_MIDDLE;
				curtain[2].initValue();
				curtain[2]._z = DEFAULT_Z_BOTTOM;
			}

			public void terminate()
			{
			}

			public void setEnable(bool enable)
			{
				if (enable)
				{
					_flag |= FLAG_ENABLE;
				}
				else
				{
					_flag &= ~FLAG_ENABLE;
				}
			}

			public void setVisible(bool visible)
			{
				if (visible)
				{
					_flag |= FLAG_VISIBLE;
				}
				else
				{
					_flag &= ~FLAG_VISIBLE;
				}
			}

			public void setAlpha(int time, int alpha)
			{
				_alphaState = ALPHA_STATE.ALPHA_STATE_CHANGING;
				_alphaTime = time;
				_nowAlphaTime = 0;
				_startAlpha = _alpha;
				_goalAlpha = alpha;
			}

			public int getAlpha()
			{
				return 0;
			}

			public void setColor(int time, ushort color)
			{
				setColor(time, color, color, color, color);
			}

			public void setColor(int time, ushort leftTop, ushort rightTop, ushort leftBottom, ushort rightBottom)
			{
				_colState = COLOR_STATE.COLOR_STATE_CHANGING;
				_colorTime = time;
				_nowColorTime = 0;
				_startColLT = _colLT;
				_startColLB = _colLB;
				_startColRT = _colRT;
				_startColRB = _colRB;
				_goalColLT = leftTop;
				_goalColLB = leftBottom;
				_goalColRT = rightTop;
				_goalColRB = rightBottom;
			}

			public void getColor(ushort pLeftTop, ushort pRightTop, ushort pLeftBottom, ushort pRightBottom)
			{
			}

			public static void execute()
			{
				for (byte b = 0; b < 3; b++)
				{
					curtain[b].executeCommon();
				}
			}

			public void draw()
			{
				if ((FLAG_ENABLE & _flag) != 0 && (FLAG_VISIBLE & _flag) != 0 && _alpha > 0)
				{
					G3_PushMtx();
					G3_OrthoW(0, 786432, 0, 1048576, -4194304, 4194304, 4194304, null);
					G3_MtxMode(GXMtxMode.GX_MTXMODE_POSITION);
					G3_Identity();
					G3_Translate(-2048, -2048, _z);
					G3_Scale(1052672, 790528, 0);
					G3_PolygonAttr(0, GXPolygonMode.GX_POLYGONMODE_MODULATE, GXCull.GX_CULL_FRONT, _polygonID, _alpha, 0);
					G3_TexImageParam(GXTexFmt.GX_TEXFMT_NONE, 0, GXTexSizeS.GX_TEXSIZE_S8, GXTexSizeT.GX_TEXSIZE_T8, 0, 0, 0, null);
					G3_Begin(GXBegin.GX_BEGIN_QUADS);
					G3_Color(_colLT);
					G3_Vtx(0, 4096, 0);
					G3_Color(_colLB);
					G3_Vtx(0, 0, 0);
					G3_Color(_colRB);
					G3_Vtx(4096, 0, 0);
					G3_Color(_colRT);
					G3_Vtx(4096, 4096, 0);
					G3_End();
					G3_PopMtx(1);
				}
			}

			public void executeCommon()
			{
				if ((_flag & FLAG_ENABLE) == 0)
				{
					return;
				}
				if (ALPHA_STATE.ALPHA_STATE_CHANGING == _alphaState)
				{
					_nowAlphaTime++;
					if (_alphaTime <= _nowAlphaTime)
					{
						_alpha = _goalAlpha;
						_alphaState = ALPHA_STATE.ALPHA_STATE_CHANGED;
					}
					else
					{
						int v = FX_Div(_nowAlphaTime * 4096, _alphaTime * 4096);
						_alpha = (FX_Mul((_goalAlpha - _startAlpha) * 4096, v) >> 12) + _startAlpha;
					}
				}
				if (COLOR_STATE.COLOR_STATE_CHANGING == _colState)
				{
					_nowColorTime++;
					if (_colorTime <= _nowColorTime)
					{
						_colLT = _goalColLT;
						_colRT = _goalColRT;
						_colLB = _goalColLB;
						_colRB = _goalColRB;
						_colState = COLOR_STATE.COLOR_STATE_CHANGED;
						return;
					}
					int v2 = FX_Div(_nowColorTime * 4096, _colorTime * 4096);
					sbyte b = (sbyte)ds.getGXR(_goalColLT);
					sbyte b2 = (sbyte)ds.getGXR(_startColLT);
					byte r = (byte)((FX_Mul((b - b2) * 4096, v2) >> 12) + b2);
					b = (sbyte)ds.getGXG(_goalColLT);
					b2 = (sbyte)ds.getGXG(_startColLT);
					byte g = (byte)((FX_Mul((b - b2) * 4096, v2) >> 12) + b2);
					b = (sbyte)ds.getGXB(_goalColLT);
					b2 = (sbyte)ds.getGXB(_startColLT);
					byte b3 = (byte)((FX_Mul((b - b2) * 4096, v2) >> 12) + b2);
					_colLT = ds.setGXRgb(r, g, b3);
					b = (sbyte)ds.getGXR(_goalColLB);
					b2 = (sbyte)ds.getGXR(_startColLB);
					r = (byte)((FX_Mul((b - b2) * 4096, v2) >> 12) + b2);
					b = (sbyte)ds.getGXG(_goalColLB);
					b2 = (sbyte)ds.getGXG(_startColLB);
					g = (byte)((FX_Mul((b - b2) * 4096, v2) >> 12) + b2);
					b = (sbyte)ds.getGXB(_goalColLB);
					b2 = (sbyte)ds.getGXB(_startColLB);
					b3 = (byte)((FX_Mul((b - b2) * 4096, v2) >> 12) + b2);
					_colLB = ds.setGXRgb(r, g, b3);
					b = (sbyte)ds.getGXR(_goalColRT);
					b2 = (sbyte)ds.getGXR(_startColRT);
					r = (byte)((FX_Mul((b - b2) * 4096, v2) >> 12) + b2);
					b = (sbyte)ds.getGXG(_goalColRT);
					b2 = (sbyte)ds.getGXG(_startColRT);
					g = (byte)((FX_Mul((b - b2) * 4096, v2) >> 12) + b2);
					b = (sbyte)ds.getGXB(_goalColRT);
					b2 = (sbyte)ds.getGXB(_startColRT);
					b3 = (byte)((FX_Mul((b - b2) * 4096, v2) >> 12) + b2);
					_colRT = ds.setGXRgb(r, g, b3);
					b = (sbyte)ds.getGXR(_goalColRB);
					b2 = (sbyte)ds.getGXR(_startColRB);
					r = (byte)((FX_Mul((b - b2) * 4096, v2) >> 12) + b2);
					b = (sbyte)ds.getGXG(_goalColRB);
					b2 = (sbyte)ds.getGXG(_startColRB);
					g = (byte)((FX_Mul((b - b2) * 4096, v2) >> 12) + b2);
					b = (sbyte)ds.getGXB(_goalColRB);
					b2 = (sbyte)ds.getGXB(_startColRB);
					b3 = (byte)((FX_Mul((b - b2) * 4096, v2) >> 12) + b2);
					_colRB = ds.setGXRgb(r, g, b3);
				}
			}

			public void initValue()
			{
				_flag = FLAG_VISIBLE;
				_polygonID = 0;
				_z = 0;
				_alphaTime = 0;
				_colorTime = 0;
				_nowAlphaTime = 0;
				_nowColorTime = 0;
				_alpha = 0;
				_colLT = ds.setGXRgb(31, 31, 31);
				_colLB = ds.setGXRgb(31, 31, 31);
				_colRT = ds.setGXRgb(31, 31, 31);
				_colRB = ds.setGXRgb(31, 31, 31);
				_startAlpha = 0;
				_startColLT = ds.setGXRgb(31, 31, 31);
				_startColLB = ds.setGXRgb(31, 31, 31);
				_startColRT = ds.setGXRgb(31, 31, 31);
				_startColRB = ds.setGXRgb(31, 31, 31);
				_goalAlpha = 0;
				_goalColLT = ds.setGXRgb(31, 31, 31);
				_goalColLB = ds.setGXRgb(31, 31, 31);
				_goalColRT = ds.setGXRgb(31, 31, 31);
				_goalColRB = ds.setGXRgb(31, 31, 31);
				_colState = COLOR_STATE.COLOR_STATE_ERROR;
				_alphaState = ALPHA_STATE.ALPHA_STATE_ERROR;
			}

			public static CCurtain Top()
			{
				return curtain[0];
			}

			public static CCurtain Middle()
			{
				return curtain[1];
			}

			public static CCurtain Bottom()
			{
				return curtain[2];
			}

			public static CCurtain Curtain(CURTAIN_LOCATION location)
			{
				return curtain[(int)location];
			}

			public bool isChangedAlpha()
			{
				return _alphaState == ALPHA_STATE.ALPHA_STATE_CHANGED;
			}

			public bool isChangedColor()
			{
				return _colState == COLOR_STATE.COLOR_STATE_CHANGED;
			}

			public void setPolygonID(int _id)
			{
				_polygonID = _id;
			}

			public int getPolygonID()
			{
				return _polygonID;
			}

			public void setZ(int z)
			{
				_z = z;
			}

			public int getZ()
			{
				return _z;
			}
		}
	}
}
