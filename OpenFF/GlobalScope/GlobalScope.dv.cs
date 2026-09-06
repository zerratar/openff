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
	public static class dv
	{
		public class CDeviceManager
		{
			public static CDeviceManager m_Instance = new CDeviceManager();

			private pad.CPlayerPad m_Pad = new pad.CPlayerPad();

			private tp.CPlayerTp m_Tp = new tp.CPlayerTp();

			public void initialize()
			{
				m_Pad.initialize();
				m_Pad.registerPad(ds.g_Pad);
				m_Tp.initialize();
			}

			public void execute()
			{
				m_Pad.execute();
				m_Tp.execute();
			}

			public void terminate()
			{
				m_Pad.terminate();
				m_Tp.terminate();
			}

			public bool isPadDecideOrTouchPanel()
			{
				if ((Pad().edge_trs(0) & 0x80) == 0 && !Tp().isEdgeTouch(120))
				{
					return false;
				}
				return true;
			}

			public bool isPadCancel()
			{
				if ((Pad().edge_trs(0) & 0x20) == 0)
				{
					return false;
				}
				return true;
			}

			public static CDeviceManager getInstance()
			{
				return m_Instance;
			}

			public pad.CPlayerPad Pad()
			{
				return m_Pad;
			}

			public tp.CPlayerTp Tp()
			{
				return m_Tp;
			}
		}

		public static class pad
		{
			public class CPlayerPad
			{
				private ds.CPad m_pPad;

				private config.CPadConfig m_Config = new config.CPadConfig();

				private @record.CPadRecord m_Record = new @record.CPadRecord();

				private bool m_Activity;

				public CPlayerPad()
				{
					m_pPad = null;
				}

				~CPlayerPad()
				{
				}

				public void initialize()
				{
					releasePad();
					m_Config.initialize();
					m_Record.initialize();
					m_Activity = true;
				}

				public void execute()
				{
					if (m_Activity)
					{
						recording();
					}
				}

				public void terminate()
				{
					m_Activity = true;
					m_Config.initialize();
					m_Record.initialize();
				}

				public uint pad(int _Num)
				{
					return m_Record.get(_Num, @record.CPadRecordBuffer.FORM.POS_CURRENT).m_Pad;
				}

				public uint edge(int _Num)
				{
					return m_Record.get(_Num, @record.CPadRecordBuffer.FORM.POS_CURRENT).m_Edge;
				}

				public uint repeat(int _Num)
				{
					return m_Record.get(_Num, @record.CPadRecordBuffer.FORM.POS_CURRENT).m_Repeat;
				}

				public uint dblClick(int _Num)
				{
					return m_Record.get(_Num, @record.CPadRecordBuffer.FORM.POS_CURRENT).m_DblClick;
				}

				public uint pad_trs(int _Num)
				{
					return transitPad(pad(_Num));
				}

				public uint edge_trs(int _Num)
				{
					return transitPad(edge(_Num));
				}

				public uint repeat_trs(int _Num)
				{
					return transitPad(repeat(_Num));
				}

				public uint dblClick_trs(int _Num)
				{
					return transitPad(dblClick(_Num));
				}

				public uint transitPad(uint _Data)
				{
					uint num = 0u;
					if ((_Data & m_Config.getAssign(2)) != 0)
					{
						num += 4;
					}
					if ((_Data & m_Config.getAssign(3)) != 0)
					{
						num += 8;
					}
					if ((_Data & m_Config.getAssign(1)) != 0)
					{
						num += 2;
					}
					if ((_Data & m_Config.getAssign(0)) != 0)
					{
						num++;
					}
					if ((_Data & m_Config.getAssign(6)) != 0)
					{
						num += 64;
					}
					if ((_Data & m_Config.getAssign(5)) != 0)
					{
						num += 32;
					}
					if ((_Data & m_Config.getAssign(7)) != 0)
					{
						num += 128;
					}
					if ((_Data & m_Config.getAssign(4)) != 0)
					{
						num += 16;
					}
					if ((_Data & m_Config.getAssign(8)) != 0)
					{
						num += 256;
					}
					if ((_Data & m_Config.getAssign(9)) != 0)
					{
						num += 512;
					}
					if ((_Data & m_Config.getAssign(10)) != 0)
					{
						num += 4096;
					}
					if ((_Data & m_Config.getAssign(11)) != 0)
					{
						num += 8192;
					}
					return num;
				}

				public void recording()
				{
					if (m_pPad != null && !m_Record.isStop() && m_Record.isStart())
					{
						@record.PAD_DATA pAD_DATA = new @record.PAD_DATA();
						pAD_DATA.set(m_pPad.pad(), m_pPad.edge(), m_pPad.repeat(), m_pPad.dblClick());
						m_Record.set(pAD_DATA);
					}
				}

				public void debugPringfPad_Trs(int _Num)
				{
					if ((pad_trs(_Num) & 0x3FF) != 0)
					{
						OS_Printf("PAD ----------------- !!!\n");
						debugPringfPad(pad_trs(_Num));
					}
				}

				public void debugPringfEdge_Trs(int _Num)
				{
					if ((edge_trs(_Num) & 0x3FF) != 0)
					{
						OS_Printf("EDGE ---------------- !!!\n");
						debugPringfPad(edge_trs(_Num));
					}
				}

				public void debugPringfRepeat_Trs(int _Num)
				{
					if ((repeat_trs(_Num) & 0x3FF) != 0)
					{
						OS_Printf("REPEAT -------------- !!!\n");
						debugPringfPad(repeat_trs(_Num));
					}
				}

				public void debugPringfDblClick_Trs(int _Num)
				{
					if ((dblClick_trs(_Num) & 0x3FF) != 0)
					{
						OS_Printf("DBLCLICK ------------ !!!\n");
						debugPringfPad(dblClick_trs(_Num));
					}
				}

				public void debugPringfPad(uint _Actpad)
				{
					if ((_Actpad & 1) != 0)
					{
						OS_Printf("↑ ");
					}
					if ((_Actpad & 2) != 0)
					{
						OS_Printf("↓ ");
					}
					if ((_Actpad & 4) != 0)
					{
						OS_Printf("← ");
					}
					if ((_Actpad & 8) != 0)
					{
						OS_Printf("→ ");
					}
					if ((_Actpad & 0x10) != 0)
					{
						OS_Printf("X ");
					}
					if ((_Actpad & 0x20) != 0)
					{
						OS_Printf("B ");
					}
					if ((_Actpad & 0x40) != 0)
					{
						OS_Printf("Y ");
					}
					if ((_Actpad & 0x80) != 0)
					{
						OS_Printf("A ");
					}
					if ((_Actpad & 0x100) != 0)
					{
						OS_Printf("L1 ");
					}
					if ((_Actpad & 0x200) != 0)
					{
						OS_Printf("R1 ");
					}
					if ((_Actpad & 0x1000) != 0)
					{
						OS_Printf("START ");
					}
					if ((_Actpad & 0x2000) != 0)
					{
						OS_Printf("SELECT ");
					}
				}

				public void registerPad(ds.CPad _pPad)
				{
					m_pPad = _pPad;
				}

				public void releasePad()
				{
					m_pPad = null;
				}

				public ds.CPad isPad()
				{
					return m_pPad;
				}

				public void setActivity(bool b)
				{
					m_Activity = b;
				}

				public bool activity()
				{
					return m_Activity;
				}

				public void recordStop()
				{
					m_Record.stop();
				}

				public void recordStart()
				{
					m_Record.start();
				}
			}

			public static class config
			{
				public class CPadConfig
				{
					private uint[] m_Key = new uint[12];

					public void initialize()
					{
						defaultConfig();
					}

					public void defaultConfig()
					{
						setAssign(0, 64);
						setAssign(1, 128);
						setAssign(2, 32);
						setAssign(3, 16);
						setAssign(4, 1024);
						setAssign(5, 2);
						setAssign(6, 2048);
						setAssign(7, 1);
						setAssign(8, 512);
						setAssign(9, 256);
						setAssign(10, 8);
						setAssign(11, 4);
					}

					public void setAssign(int _n, int _key)
					{
						if (_n < 12)
						{
							m_Key[_n] = (uint)_key;
						}
					}

					public void swapAssign(int _cfg_r, int _cfg_l)
					{
						if (_cfg_r < 12 && _cfg_l < 12)
						{
							uint num = m_Key[_cfg_r];
							m_Key[_cfg_r] = m_Key[_cfg_l];
							m_Key[_cfg_l] = num;
						}
					}

					public uint getAssign(int _key)
					{
						if (_key >= 12)
						{
							return 0u;
						}
						return m_Key[_key];
					}
				}

				public enum KEY_CONFIG
				{
					C_LUP,
					C_LDOWN,
					C_LLEFT,
					C_LRIGHT,
					C_RUP,
					C_RDOWN,
					C_RLEFT,
					C_RRIGHT,
					C_L1,
					C_R1,
					C_START,
					C_SELECT,
					KEY_CONFIG_MAX
				}

				public const KEY_CONFIG C_LUP = KEY_CONFIG.C_LUP;

				public const KEY_CONFIG C_LDOWN = KEY_CONFIG.C_LDOWN;

				public const KEY_CONFIG C_LLEFT = KEY_CONFIG.C_LLEFT;

				public const KEY_CONFIG C_LRIGHT = KEY_CONFIG.C_LRIGHT;

				public const KEY_CONFIG C_RUP = KEY_CONFIG.C_RUP;

				public const KEY_CONFIG C_RDOWN = KEY_CONFIG.C_RDOWN;

				public const KEY_CONFIG C_RLEFT = KEY_CONFIG.C_RLEFT;

				public const KEY_CONFIG C_RRIGHT = KEY_CONFIG.C_RRIGHT;

				public const KEY_CONFIG C_L1 = KEY_CONFIG.C_L1;

				public const KEY_CONFIG C_R1 = KEY_CONFIG.C_R1;

				public const KEY_CONFIG C_START = KEY_CONFIG.C_START;

				public const KEY_CONFIG C_SELECT = KEY_CONFIG.C_SELECT;

				public const KEY_CONFIG KEY_CONFIG_MAX = KEY_CONFIG.KEY_CONFIG_MAX;
			}

			public static class @record
			{
				public class CPadRecordBuffer
				{
					public enum FORM
					{
						POS_CURRENT,
						POS_BEGIN
					}

					public const FORM POS_CURRENT = FORM.POS_CURRENT;

					public const FORM POS_BEGIN = FORM.POS_BEGIN;

					protected short m_Point;

					protected PAD_DATA[] m_Buf = new PAD_DATA[PAD_RECORD_SIZE];

					public CPadRecordBuffer()
					{
						for (int i = 0; i < m_Buf.Length; i++)
						{
							m_Buf[i] = new PAD_DATA();
						}
					}

					public virtual void initialize()
					{
						reset();
					}

					public void reset()
					{
						m_Point = -1;
						for (int i = 0; i < PAD_RECORD_SIZE; i++)
						{
							m_Buf[i].initialize();
						}
					}

					public void set(PAD_DATA data)
					{
						if (m_Point < 0)
						{
							m_Point = 0;
						}
						else
						{
							m_Point++;
							if (m_Point >= PAD_RECORD_SIZE)
							{
								m_Point = 0;
							}
						}

						// PORT: the shipped build advanced the write cursor and then dropped the
						// sample - nothing was ever written into m_Buf. So pad(0) always read back
						// a zeroed record, transitPad turned that into no direction, and the D-pad
						// movement path in pl.CPlayerCharacter could never fire. Harmless on a
						// phone with no D-pad; on Windows it is exactly why WASD and the arrow keys
						// do not move the character. Confirmed against both decompilers, so it is
						// in the shipped binary, not an artefact of decompiling it.
						if (data != null)
						{
							m_Buf[m_Point].set(data.m_Pad, data.m_Edge, data.m_Repeat, data.m_DblClick);
						}
					}

					public PAD_DATA get(int _Offset, FORM _Form)
					{
						int point = m_Point;
						int num = point + _Offset;
						point = ((_Form == FORM.POS_CURRENT) ? ((num >= 0) ? (point + _Offset) : (PAD_RECORD_SIZE + num)) : ((num < PAD_RECORD_SIZE) ? (point + _Offset) : (num - PAD_RECORD_SIZE)));
						return m_Buf[point];
					}
				}

				public class CPadRecord : CPadRecordBuffer
				{
					public enum STATE
					{
						STOP,
						START
					}

					public const STATE STOP = STATE.STOP;

					public const STATE START = STATE.START;

					private STATE m_State;

					public override void initialize()
					{
						reset();
						m_State = STATE.START;
					}

					public void stop()
					{
						m_State = STATE.STOP;
					}

					public void start()
					{
						m_State = STATE.START;
					}

					public bool isStop()
					{
						if (m_State != STATE.STOP)
						{
							return false;
						}
						return true;
					}

					public bool isStart()
					{
						if (m_State != STATE.START)
						{
							return false;
						}
						return true;
					}
				}

				public class PAD_DATA
				{
					private static uint ERR = uint.MaxValue;

					public uint m_Pad;

					public uint m_Edge;

					public uint m_Repeat;

					public uint m_DblClick;

					public void initialize()
					{
						m_Pad = (m_Edge = (m_Repeat = (m_DblClick = 0u)));
					}

					private bool isValid()
					{
						if (m_Pad != ERR && m_Edge != ERR && m_Repeat != ERR && m_DblClick != ERR)
						{
							return true;
						}
						return false;
					}

					public void set(uint _Pad, uint _Edge, uint _Repeat, uint _DblClick)
					{
						m_Pad = _Pad;
						m_Edge = _Edge;
						m_Repeat = _Repeat;
						m_DblClick = _DblClick;
					}
				}

				private static int PAD_RECORD_SIZE = 16;
			}

			public enum KEY_TYPE
			{
				lDown = 128,
				lUp = 64,
				lRight = 16,
				lLeft = 32,
				rRight = 1,
				rDown = 2,
				rUp = 1024,
				rLeft = 2048,
				L1 = 512,
				R1 = 256,
				START = 8,
				SELECT = 4
			}

			public enum KEY_COMPARISON
			{
				KEY_UP = 1,
				KEY_DOWN = 2,
				KEY_LEFT = 4,
				KEY_RIGHT = 8,
				KEY_BUP = 16,
				KEY_BDOWN = 32,
				KEY_BLEFT = 64,
				KEY_BRIGHT = 128,
				KEY_L1 = 256,
				KEY_R1 = 512,
				KEY_START = 4096,
				KEY_SELECT = 8192,
				KEY_UP_RIGHT = 9,
				KEY_UP_LEFT = 5,
				KEY_DOWN_RIGHT = 10,
				KEY_DOWN_LEFT = 6,
				KEY_UDLR = 15,
				KEY_XYBA = 240,
				KEY_LR = 768,
				ANY_KEY = 1023,
				KEY_DECIDE = KEY_BRIGHT,
				KEY_CANCEL = KEY_BDOWN,
				KEY_MESSAGE_NEXT = KEY_XYBA
			}

			public const KEY_TYPE lDown = KEY_TYPE.lDown;

			public const KEY_TYPE lUp = KEY_TYPE.lUp;

			public const KEY_TYPE lRight = KEY_TYPE.lRight;

			public const KEY_TYPE lLeft = KEY_TYPE.lLeft;

			public const KEY_TYPE rRight = KEY_TYPE.rRight;

			public const KEY_TYPE rDown = KEY_TYPE.rDown;

			public const KEY_TYPE rUp = KEY_TYPE.rUp;

			public const KEY_TYPE rLeft = KEY_TYPE.rLeft;

			public const KEY_TYPE L1 = KEY_TYPE.L1;

			public const KEY_TYPE R1 = KEY_TYPE.R1;

			public const KEY_TYPE START = KEY_TYPE.START;

			public const KEY_TYPE SELECT = KEY_TYPE.SELECT;

			public const KEY_COMPARISON KEY_UP = KEY_COMPARISON.KEY_UP;

			public const KEY_COMPARISON KEY_DOWN = KEY_COMPARISON.KEY_DOWN;

			public const KEY_COMPARISON KEY_LEFT = KEY_COMPARISON.KEY_LEFT;

			public const KEY_COMPARISON KEY_RIGHT = KEY_COMPARISON.KEY_RIGHT;

			public const KEY_COMPARISON KEY_BUP = KEY_COMPARISON.KEY_BUP;

			public const KEY_COMPARISON KEY_BDOWN = KEY_COMPARISON.KEY_BDOWN;

			public const KEY_COMPARISON KEY_BLEFT = KEY_COMPARISON.KEY_BLEFT;

			public const KEY_COMPARISON KEY_BRIGHT = KEY_COMPARISON.KEY_BRIGHT;

			public const KEY_COMPARISON KEY_L1 = KEY_COMPARISON.KEY_L1;

			public const KEY_COMPARISON KEY_R1 = KEY_COMPARISON.KEY_R1;

			public const KEY_COMPARISON KEY_START = KEY_COMPARISON.KEY_START;

			public const KEY_COMPARISON KEY_SELECT = KEY_COMPARISON.KEY_SELECT;

			public const KEY_COMPARISON KEY_UP_RIGHT = KEY_COMPARISON.KEY_UP_RIGHT;

			public const KEY_COMPARISON KEY_UP_LEFT = KEY_COMPARISON.KEY_UP_LEFT;

			public const KEY_COMPARISON KEY_DOWN_RIGHT = KEY_COMPARISON.KEY_DOWN_RIGHT;

			public const KEY_COMPARISON KEY_DOWN_LEFT = KEY_COMPARISON.KEY_DOWN_LEFT;

			public const KEY_COMPARISON KEY_UDLR = KEY_COMPARISON.KEY_UDLR;

			public const KEY_COMPARISON KEY_XYBA = KEY_COMPARISON.KEY_XYBA;

			public const KEY_COMPARISON KEY_LR = KEY_COMPARISON.KEY_LR;

			public const KEY_COMPARISON ANY_KEY = KEY_COMPARISON.ANY_KEY;

			public const KEY_COMPARISON KEY_DECIDE = KEY_COMPARISON.KEY_BRIGHT;

			public const KEY_COMPARISON KEY_CANCEL = KEY_COMPARISON.KEY_BDOWN;

			public const KEY_COMPARISON KEY_MESSAGE_NEXT = KEY_COMPARISON.KEY_XYBA;
		}

		public static class tp
		{
			public class CPlayerTp
			{
				public const int TOUCH_EFFECT_LOOP_FRAME = 30;

				private bool m_TouchEffectFlag;

				private uint m_TouchEffectFrame;

				private uint m_TouchEffectLoopFrame;

				private int m_TouchEffectId;

				private int m_TouchEffectCategory;

				private int m_TouchEffectMember;

				private ds.sys3d.CCamera m_pCamera;

				public void initialize()
				{
					m_TouchEffectFlag = false;
					m_TouchEffectFrame = 30u;
					m_TouchEffectLoopFrame = 30u;
					m_TouchEffectId = -1;
					m_TouchEffectCategory = 1;
					m_TouchEffectMember = 0;
					m_pCamera = null;
				}

				public void execute()
				{
					exeTouchEffect();
				}

				public void terminate()
				{
					m_pCamera = null;
				}

				public bool isTouch()
				{
					return ds.g_TouchPanel.isTouch();
				}

				public bool isEdgeTouch(int _Frame)
				{
					return ds.g_TouchPanel.isEdgeTouch(_Frame);
				}

				public bool isRepeatTouch()
				{
					return ds.g_TouchPanel.isRepeatTouch();
				}

				public bool isDblClickTouch()
				{
					return ds.g_TouchPanel.isDblClickTouch();
				}

				public bool isRelease()
				{
					return ds.g_TouchPanel.isRelease();
				}

				public bool TouchPanel_2d(out int x, out int y)
				{
					ds.g_TouchPanel.getPoint(out x, out y);
					return isTouch();
				}

				public bool TouchPanel_3d(VecFx32 _Pos)
				{
					if (m_pCamera == null)
					{
						return false;
					}
					return utl.getTouchPanel3d(_Pos, m_pCamera);
				}

				public int culDistance(VecFx32 _PosA, VecFx32 _PosB)
				{
					return VEC_Distance(_PosA, _PosB) / 4096;
				}

				public void exeTouchEffect()
				{
					if (!m_TouchEffectFlag)
					{
						return;
					}
					VecFx32 fnd_reuse_pos = GlobalScope.fnd_reuse_pos;
					fnd_reuse_pos.x = 0;
					fnd_reuse_pos.y = 0;
					fnd_reuse_pos.z = 0;
					if (TouchPanel_3d(fnd_reuse_pos))
					{
						if (m_TouchEffectFrame++ >= m_TouchEffectLoopFrame)
						{
							m_TouchEffectFrame = 0u;
							m_TouchEffectId = eff.CEffectMng.instance().create(m_TouchEffectCategory, m_TouchEffectMember);
						}
						if (m_TouchEffectId != -1)
						{
							eff.CEffectMng.instance().setPosition(m_TouchEffectId, fnd_reuse_pos);
						}
					}
					else
					{
						m_TouchEffectFrame = 30u;
					}
				}

				public CPlayerTp()
				{
					m_TouchEffectFlag = false;
					m_TouchEffectFrame = 0u;
					m_TouchEffectLoopFrame = 30u;
					m_TouchEffectId = -1;
					m_TouchEffectCategory = 1;
					m_TouchEffectMember = 0;
					m_pCamera = null;
				}

				~CPlayerTp()
				{
				}

				public void setTouchEffectFlag(bool m_TouchEffectFlag)
				{
					this.m_TouchEffectFlag = m_TouchEffectFlag;
				}

				public void setTouchEffectLoopFrame(uint m_TouchEffectLoopFrame)
				{
					this.m_TouchEffectLoopFrame = m_TouchEffectLoopFrame;
				}

				public uint getTouchEffectLoopFrame()
				{
					return m_TouchEffectLoopFrame;
				}

				public void setTouchEffectCategory(int m_TouchEffectCategory)
				{
					this.m_TouchEffectCategory = m_TouchEffectCategory;
				}

				public void setTouchEffectMember(int m_TouchEffectMember)
				{
					this.m_TouchEffectMember = m_TouchEffectMember;
				}

				public void setCamera(ds.sys3d.CCamera m_pCamera)
				{
					this.m_pCamera = m_pCamera;
				}

				public TPData getDispPoint()
				{
					return ds.g_TouchPanel.getDispPoint();
				}
			}
		}
	}
}
