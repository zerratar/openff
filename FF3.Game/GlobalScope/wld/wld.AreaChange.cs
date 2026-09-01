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
	public static partial class wld
	{
		public class AreaChange
		{
			public class Shutter
			{
				public const int DEFINE_Z = -4177920;

				public bool _isEnable;

				public VecFx32 _p = new VecFx32();

				public int _width;

				public int _height;

				public ushort _col;

				~Shutter()
				{
				}

				public void initValue()
				{
					_isEnable = false;
					_width = 0;
					_height = 0;
					_col = GX_RGB(0, 0, 0);
					VEC_Set(_p, 0, 0, -4177920);
				}

				public void draw()
				{
					if (_isEnable)
					{
						G3_PushMtx();
						G3_OrthoW(0, 786432, 0, 1048576, -4194304, 4194304, 4194304, null);
						G3_MtxMode(GXMtxMode.GX_MTXMODE_POSITION_VECTOR);
						G3_Identity();
						G3_Translate(_p.x - 2048, _p.y - 2048, _p.z);
						G3_Scale(_width + 4096, _height + 4096, 0);
						G3_PolygonAttr(0, GXPolygonMode.GX_POLYGONMODE_MODULATE, GXCull.GX_CULL_NONE, 0, 31, 0);
						G3_TexImageParam(GXTexFmt.GX_TEXFMT_NONE, 0, GXTexSizeS.GX_TEXSIZE_S8, GXTexSizeT.GX_TEXSIZE_T8, 0, 0, 0, null);
						G3_Begin(GXBegin.GX_BEGIN_QUADS);
						G3_Color(_col);
						G3_Vtx(0, 0, 0);
						G3_Vtx(0, 4096, 0);
						G3_Vtx(4096, 4096, 0);
						G3_Vtx(4096, 0, 0);
						G3_End();
						G3_PopMtx(1);
					}
				}
			}

			public enum SWITCH_TYPE
			{
				SWITCH_TYPE_ERROR = -1,
				SWITCH_TYPE_CENTER,
				SWITCH_TYPE_OUTER,
				SWITCH_TYPE_MAX
			}

			public enum SWITCH_STATE
			{
				SWITCH_STATE_ERROR = -1,
				SWITCH_STATE_OPENED,
				SWITCH_STATE_CLOSED,
				SWITCH_STATE_OPENING,
				SWITCH_STATE_CLOSING,
				SWITCH_STATE_MAX
			}

			public const SWITCH_TYPE SWITCH_TYPE_ERROR = SWITCH_TYPE.SWITCH_TYPE_ERROR;

			public const SWITCH_TYPE SWITCH_TYPE_CENTER = SWITCH_TYPE.SWITCH_TYPE_CENTER;

			public const SWITCH_TYPE SWITCH_TYPE_OUTER = SWITCH_TYPE.SWITCH_TYPE_OUTER;

			public const SWITCH_TYPE SWITCH_TYPE_MAX = SWITCH_TYPE.SWITCH_TYPE_MAX;

			private const byte SHUTTER_NUM = 2;

			public const SWITCH_STATE SWITCH_STATE_ERROR = SWITCH_STATE.SWITCH_STATE_ERROR;

			public const SWITCH_STATE SWITCH_STATE_OPENED = SWITCH_STATE.SWITCH_STATE_OPENED;

			public const SWITCH_STATE SWITCH_STATE_CLOSED = SWITCH_STATE.SWITCH_STATE_CLOSED;

			public const SWITCH_STATE SWITCH_STATE_OPENING = SWITCH_STATE.SWITCH_STATE_OPENING;

			public const SWITCH_STATE SWITCH_STATE_CLOSING = SWITCH_STATE.SWITCH_STATE_CLOSING;

			public const SWITCH_STATE SWITCH_STATE_MAX = SWITCH_STATE.SWITCH_STATE_MAX;

			public static AreaChange _instance = new AreaChange();

			private static uint FLAG_ENABLE = 1u;

			private Shutter[] _shutter = new Shutter[2];

			private uint _flag;

			private int _switchTime;

			private int _nowSwitchTime;

			private SWITCH_STATE _switchState;

			private SWITCH_TYPE _switchType;

			public AreaChange()
			{
				for (int i = 0; i < _shutter.Length; i++)
				{
					_shutter[i] = new Shutter();
				}
			}

			public void initialize()
			{
				initValue();
			}

			public void terminate()
			{
			}

			public void draw()
			{
				for (byte b = 0; b < 2; b++)
				{
					_shutter[b].draw();
				}
			}

			public void execute()
			{
				if (_switchType == SWITCH_TYPE.SWITCH_TYPE_ERROR || _switchType == SWITCH_TYPE.SWITCH_TYPE_MAX)
				{
					return;
				}
				switch (_switchState)
				{
				case SWITCH_STATE.SWITCH_STATE_OPENING:
					if (executeOpen())
					{
						_switchState = SWITCH_STATE.SWITCH_STATE_OPENED;
					}
					break;
				case SWITCH_STATE.SWITCH_STATE_CLOSING:
					if (executeClose())
					{
						_switchState = SWITCH_STATE.SWITCH_STATE_CLOSED;
					}
					break;
				}
			}

			public bool setOpen(int time, SWITCH_TYPE stype)
			{
				if (SWITCH_STATE.SWITCH_STATE_CLOSED != _switchState)
				{
					return false;
				}
				if (stype <= SWITCH_TYPE.SWITCH_TYPE_ERROR || SWITCH_TYPE.SWITCH_TYPE_MAX <= stype)
				{
					return false;
				}
				_switchState = SWITCH_STATE.SWITCH_STATE_OPENING;
				_switchType = stype;
				_switchTime = time;
				_nowSwitchTime = 0;
				_shutter[0]._isEnable = true;
				_shutter[0]._width = 1048576;
				_shutter[0]._height = 393216;
				_shutter[0]._p.x = 0;
				_shutter[0]._p.y = 0;
				_shutter[1]._isEnable = true;
				_shutter[1]._width = 1048576;
				_shutter[1]._height = 393216;
				_shutter[1]._p.x = 0;
				_shutter[1]._p.y = 393216;
				return true;
			}

			public bool setClose(int time, SWITCH_TYPE stype)
			{
				if (_switchState != SWITCH_STATE.SWITCH_STATE_OPENED)
				{
					return false;
				}
				if (stype <= SWITCH_TYPE.SWITCH_TYPE_ERROR || SWITCH_TYPE.SWITCH_TYPE_MAX <= stype)
				{
					return false;
				}
				_switchState = SWITCH_STATE.SWITCH_STATE_CLOSING;
				_switchType = stype;
				_switchTime = time;
				_nowSwitchTime = 0;
				_shutter[0]._isEnable = true;
				_shutter[0]._width = 1048576;
				_shutter[0]._height = 0;
				_shutter[0]._p.x = 0;
				_shutter[1]._isEnable = true;
				_shutter[1]._width = 1048576;
				_shutter[1]._height = 0;
				_shutter[1]._p.x = 0;
				switch (_switchType)
				{
				case SWITCH_TYPE.SWITCH_TYPE_CENTER:
					_shutter[0]._p.y = 0;
					_shutter[1]._p.y = 786432;
					break;
				case SWITCH_TYPE.SWITCH_TYPE_OUTER:
					_shutter[0]._p.y = 393216;
					_shutter[1]._p.y = 393216;
					break;
				}
				return true;
			}

			public bool setOpenStrong()
			{
				_shutter[0]._isEnable = false;
				_shutter[1]._isEnable = false;
				_switchState = SWITCH_STATE.SWITCH_STATE_OPENED;
				return true;
			}

			public bool setCloseStrong()
			{
				_switchState = SWITCH_STATE.SWITCH_STATE_CLOSED;
				_shutter[0]._isEnable = true;
				_shutter[0]._p.x = 0;
				_shutter[0]._p.y = 0;
				_shutter[0]._width = 1048576;
				_shutter[0]._height = 393216;
				_shutter[1]._isEnable = true;
				_shutter[1]._p.x = 0;
				_shutter[1]._p.y = 393216;
				_shutter[1]._width = 1048576;
				_shutter[1]._height = 393216;
				return true;
			}

			public void setColor(ushort col)
			{
				_shutter[0]._col = col;
				_shutter[1]._col = col;
			}

			public ushort getColor()
			{
				return _shutter[0]._col;
			}

			public bool executeOpen()
			{
				_nowSwitchTime++;
				if (_nowSwitchTime >= _switchTime)
				{
					_nowSwitchTime = _switchTime;
					_shutter[0]._isEnable = false;
					_shutter[1]._isEnable = false;
				}
				int v = 4096 - FX_Div(4096 * _nowSwitchTime, 4096 * _switchTime);
				switch (_switchType)
				{
				case SWITCH_TYPE.SWITCH_TYPE_CENTER:
					_shutter[0]._height = FX_Mul(393216, v);
					_shutter[0]._p.y = 393216 - _shutter[0]._height;
					_shutter[1]._height = _shutter[0]._height;
					break;
				case SWITCH_TYPE.SWITCH_TYPE_OUTER:
					_shutter[0]._height = FX_Mul(393216, v);
					_shutter[1]._height = _shutter[0]._height;
					_shutter[1]._p.y = 786432 - _shutter[1]._height;
					break;
				}
				return _nowSwitchTime >= _switchTime;
			}

			public bool executeClose()
			{
				_nowSwitchTime++;
				if (_nowSwitchTime >= _switchTime)
				{
					_nowSwitchTime = _switchTime;
				}
				int v = FX_Div(4096 * _nowSwitchTime, 4096 * _switchTime);
				switch (_switchType)
				{
				case SWITCH_TYPE.SWITCH_TYPE_CENTER:
					_shutter[0]._height = FX_Mul(393216, v);
					_shutter[1]._height = _shutter[0]._height;
					_shutter[1]._p.y = 786432 - _shutter[1]._height;
					break;
				case SWITCH_TYPE.SWITCH_TYPE_OUTER:
					_shutter[0]._height = FX_Mul(393216, v);
					_shutter[0]._p.y = 393216 - _shutter[0]._height;
					_shutter[1]._height = _shutter[0]._height;
					break;
				}
				return _nowSwitchTime >= _switchTime;
			}

			public void initValue()
			{
				for (byte b = 0; b < 2; b++)
				{
					_shutter[b].initValue();
				}
				_flag = FLAG_ENABLE;
				_switchTime = 0;
				_nowSwitchTime = 0;
				_switchState = SWITCH_STATE.SWITCH_STATE_OPENED;
				_switchType = SWITCH_TYPE.SWITCH_TYPE_ERROR;
			}

			public bool isOpened()
			{
				return _switchState == SWITCH_STATE.SWITCH_STATE_OPENED;
			}

			public bool isClosed()
			{
				return _switchState == SWITCH_STATE.SWITCH_STATE_CLOSED;
			}

			public SWITCH_TYPE getSwitchType()
			{
				return _switchType;
			}

			public static AreaChange getInstance()
			{
				return _instance;
			}
		}
	}
}
