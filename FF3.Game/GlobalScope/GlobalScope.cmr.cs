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
						public static class cmr
						{
							public class CCameraZoom
							{
								public enum ZOOM_STATE
								{
									ZOOM_ERR = -1,
									ZOOM_WAIT,
									ZOOM_MANUAL_IN,
									ZOOM_MANUAL_OUT,
									ZOOM_AUTO_IN,
									ZOOM_AUTO_OUT,
									ZOOM_PINCH,
									ZOOM_STATE_MAX
								}

								public const ZOOM_STATE ZOOM_ERR = ZOOM_STATE.ZOOM_ERR;

								public const ZOOM_STATE ZOOM_WAIT = ZOOM_STATE.ZOOM_WAIT;

								public const ZOOM_STATE ZOOM_MANUAL_IN = ZOOM_STATE.ZOOM_MANUAL_IN;

								public const ZOOM_STATE ZOOM_MANUAL_OUT = ZOOM_STATE.ZOOM_MANUAL_OUT;

								public const ZOOM_STATE ZOOM_AUTO_IN = ZOOM_STATE.ZOOM_AUTO_IN;

								public const ZOOM_STATE ZOOM_AUTO_OUT = ZOOM_STATE.ZOOM_AUTO_OUT;

								public const ZOOM_STATE ZOOM_PINCH = ZOOM_STATE.ZOOM_PINCH;

								public const ZOOM_STATE ZOOM_STATE_MAX = ZOOM_STATE.ZOOM_STATE_MAX;

								private bool m_Enable;

								private ZOOM_STATE m_State;

								private int m_Zoom;

								private int m_ZoomMax;

								private int m_ZoomMin;

								private int m_ZoomSpd;

								private int m_ZoomRec;

								private int m_Pinch;

								public void initialize()
								{
									m_Enable = true;
									m_State = ZOOM_STATE.ZOOM_ERR;
									m_Zoom = 0;
									m_ZoomMax = 0;
									m_ZoomMin = 0;
									m_ZoomSpd = 0;
									m_ZoomRec = 0;
								}

								public void execute()
								{
									if (m_State != ZOOM_STATE.ZOOM_ERR)
									{
										if (m_State == ZOOM_STATE.ZOOM_PINCH)
										{
											m_Zoom += FX_Div((m_Pinch - m_Zoom) * 2, m_ZoomSpd);
											m_State = ZOOM_STATE.ZOOM_WAIT;
										}
										if (m_State == ZOOM_STATE.ZOOM_AUTO_IN || m_State == ZOOM_STATE.ZOOM_MANUAL_IN)
										{
											m_Zoom -= FX_Div(MATH_ABS(m_Zoom - m_ZoomMax), m_ZoomSpd);
										}
										if (m_State == ZOOM_STATE.ZOOM_AUTO_OUT || m_State == ZOOM_STATE.ZOOM_MANUAL_OUT)
										{
											m_Zoom += FX_Div(MATH_ABS(m_Zoom - m_ZoomMin), m_ZoomSpd);
										}
										if (m_State == ZOOM_STATE.ZOOM_MANUAL_IN || m_State == ZOOM_STATE.ZOOM_MANUAL_OUT)
										{
											m_State = ZOOM_STATE.ZOOM_WAIT;
										}
										if (m_Zoom > m_ZoomMin)
										{
											m_State = ZOOM_STATE.ZOOM_WAIT;
											m_Zoom = m_ZoomMin;
										}
										if (m_Zoom < m_ZoomMax)
										{
											m_State = ZOOM_STATE.ZOOM_WAIT;
											m_Zoom = m_ZoomMax;
										}
									}
								}

								public void update(VecFx32 _camPos, VecFx32 _value)
								{
									if (m_State != ZOOM_STATE.ZOOM_ERR)
									{
										_camPos.x += Zoom() * _value.x;
										_camPos.y += Zoom() * _value.y;
										_camPos.z += Zoom() * _value.z;
									}
								}

								public void setZoomEnable(bool b)
								{
									m_Enable = b;
								}

								public CCameraZoom()
								{
									m_Enable = false;
									m_State = ZOOM_STATE.ZOOM_ERR;
									m_Zoom = 0;
									m_ZoomMax = 0;
									m_ZoomMin = 0;
									m_ZoomSpd = 0;
									m_ZoomRec = 0;
									m_Pinch = 0;
								}

								public bool ZoomEnable()
								{
									return m_Enable;
								}

								public void setZoomState(ZOOM_STATE _State)
								{
									m_State = _State;
								}

								public ZOOM_STATE ZoomState()
								{
									return m_State;
								}

								public void ZoomState_set(ZOOM_STATE arg0)
								{
									m_State = arg0;
								}

								public void setZoom(int _Zoom)
								{
									m_Zoom = _Zoom;
								}

								public int Zoom()
								{
									return m_Zoom;
								}

								public void setZoomMax(int _ZoomMax)
								{
									m_ZoomMax = _ZoomMax;
								}

								public int ZoomMax()
								{
									return m_ZoomMax;
								}

								public void setZoomMin(int _ZoomMin)
								{
									m_ZoomMin = _ZoomMin;
								}

								public int ZoomMin()
								{
									return m_ZoomMin;
								}

								public void setZoomSpd(int _ZoomSpd)
								{
									m_ZoomSpd = _ZoomSpd;
								}

								public int ZoomSpd()
								{
									return m_ZoomSpd;
								}

								public void setZoomRec(int _ZoomRec)
								{
									m_ZoomRec = _ZoomRec;
								}

								public int ZoomRec()
								{
									return m_ZoomRec;
								}

								public void setPinch(int _Pinch)
								{
									m_Pinch = _Pinch;
								}

								public int Pinch()
								{
									return m_Pinch;
								}

								public void Pinch_set(int arg0)
								{
									m_Pinch = arg0;
								}

								public void Pinch_add(int arg0)
								{
									m_Pinch += arg0;
								}

								public void copy(CCameraZoom src)
								{
									m_Enable = src.m_Enable;
									m_State = src.m_State;
									m_Zoom = src.m_Zoom;
									m_ZoomMax = src.m_ZoomMax;
									m_ZoomMin = src.m_ZoomMin;
									m_ZoomSpd = src.m_ZoomSpd;
									m_ZoomRec = src.m_ZoomRec;
									m_Pinch = src.m_Pinch;
								}
							}

							public class CCameraVibration
							{
								public enum VIBRATION_STATE
								{
									VIBRATION_ERR = -1,
									VIBRATION_WAIT,
									VIBRATION_EXE_1,
									VIBRATION_EXE_2,
									VIBRATION_STATE_MAX
								}

								public const VIBRATION_STATE VIBRATION_ERR = VIBRATION_STATE.VIBRATION_ERR;

								public const VIBRATION_STATE VIBRATION_WAIT = VIBRATION_STATE.VIBRATION_WAIT;

								public const VIBRATION_STATE VIBRATION_EXE_1 = VIBRATION_STATE.VIBRATION_EXE_1;

								public const VIBRATION_STATE VIBRATION_EXE_2 = VIBRATION_STATE.VIBRATION_EXE_2;

								public const VIBRATION_STATE VIBRATION_STATE_MAX = VIBRATION_STATE.VIBRATION_STATE_MAX;

								private bool m_Turn;

								private VIBRATION_STATE m_State;

								private int m_CountFrame;

								private int m_VibratFrame;

								private int m_VibratSpeed;

								private int m_OneTurnFrame;

								private VecFx32 m_VibratOffset = new VecFx32();

								private VecFx32 m_PeakPos = new VecFx32();

								private VecFx32 m_PeakOffset = new VecFx32();

								public void initialize()
								{
									m_Turn = false;
									m_State = VIBRATION_STATE.VIBRATION_ERR;
									m_CountFrame = 0;
									m_VibratFrame = 0;
									m_VibratSpeed = 0;
									m_OneTurnFrame = 0;
									VEC_Set(m_VibratOffset, 0, 0, 0);
									VEC_Set(m_PeakPos, 0, 0, 0);
									VEC_Set(m_PeakOffset, 0, 0, 0);
								}

								public void execute()
								{
									_ = m_State;
									_ = -1;
								}

								public void update(VecFx32 _camPos)
								{
									if (m_State == VIBRATION_STATE.VIBRATION_ERR)
									{
										return;
									}
									if (m_CountFrame-- < 0)
									{
										initialize();
									}
									else if (m_State == VIBRATION_STATE.VIBRATION_EXE_1)
									{
										VecFx32 cmr_reuse_v = cmr_reuse_v0;
										VecFx32 cmr_reuse_v2 = cmr_reuse_v1;
										cmr_reuse_v2.x = (int)(m_VibratOffset.x * ds.RandomNumber.rand32((uint)m_VibratSpeed));
										cmr_reuse_v2.y = (int)(m_VibratOffset.y * ds.RandomNumber.rand32((uint)m_VibratSpeed));
										cmr_reuse_v2.z = (int)(m_VibratOffset.z * ds.RandomNumber.rand32((uint)m_VibratSpeed));
										if (m_CountFrame % 2 == 0)
										{
											cmr_reuse_v.x = (int)((ds.RandomNumber.rand32(5u) - 2) * cmr_reuse_v2.x);
											cmr_reuse_v.y = (m_CountFrame % 2 * 2 - 1) * cmr_reuse_v2.y;
											cmr_reuse_v.z = (m_CountFrame % 2 * 2 - 1) * cmr_reuse_v2.z;
											VEC_Add(_camPos, cmr_reuse_v, _camPos);
										}
									}
									else if (m_State == VIBRATION_STATE.VIBRATION_EXE_2)
									{
										int num = ((m_CountFrame == 0) ? m_VibratFrame : (m_VibratFrame / m_CountFrame));
										VecFx32 cmr_reuse_v3 = cmr_reuse_v0;
										cmr_reuse_v3.set((num == 0) ? m_VibratOffset.x : (m_VibratOffset.x / num), (num == 0) ? m_VibratOffset.y : (m_VibratOffset.y / num), (num == 0) ? m_VibratOffset.z : (m_VibratOffset.z / num));
										if (m_OneTurnFrame == 0 || m_CountFrame % m_OneTurnFrame == 0)
										{
											m_Turn = !m_Turn;
										}
										VEC_Add(_camPos, m_PeakOffset, _camPos);
										VEC_Set(m_PeakOffset, 0, 0, 0);
										VecFx32 cmr_reuse_v4 = cmr_reuse_v1;
										cmr_reuse_v4.copy(_camPos);
										if (!m_Turn)
										{
											VEC_Subtract(_camPos, cmr_reuse_v3, _camPos);
										}
										else
										{
											VEC_Add(_camPos, cmr_reuse_v3, _camPos);
										}
										if (m_CountFrame > 0 && skipFrame != 0 && VecFx32cmp(m_PeakPos, _camPos))
										{
											VEC_Subtract(_camPos, cmr_reuse_v4, m_PeakOffset);
											_camPos.copy(cmr_reuse_v4);
										}
										if (skipFrame != 0)
										{
											VecFx32cpy(m_PeakPos, _camPos);
										}
									}
								}

								public CCameraVibration()
								{
									m_Turn = false;
									m_State = VIBRATION_STATE.VIBRATION_ERR;
									m_CountFrame = 0;
									m_VibratFrame = 0;
									m_VibratSpeed = 0;
									m_OneTurnFrame = 0;
									VEC_Set(m_VibratOffset, 0, 0, 0);
									VEC_Set(m_PeakPos, 0, 0, 0);
									VEC_Set(m_PeakOffset, 0, 0, 0);
								}

								public void setVibratState(VIBRATION_STATE _State)
								{
									m_State = _State;
								}

								public VIBRATION_STATE VibratState()
								{
									return m_State;
								}

								public void setVibratFrame(int _VibratFrame)
								{
									m_VibratFrame = _VibratFrame;
								}

								public int VibratFrame()
								{
									return m_VibratFrame;
								}

								public void setVibratSpeed(int _VibratSpeed)
								{
									m_VibratSpeed = _VibratSpeed;
								}

								public int VibratSpeed()
								{
									return m_VibratSpeed;
								}

								public void setVibratOffset(VecFx32 _VibratOffset)
								{
									m_VibratOffset.copy(_VibratOffset);
								}

								public VecFx32 VibratOffset()
								{
									return m_VibratOffset;
								}

								public void startVibration(VIBRATION_STATE _State, int _Frame, int _Speed, int _OffsetX, int _OffsetY, int _OffsetZ, bool _Turn)
								{
									m_Turn = _Turn;
									m_State = _State;
									m_CountFrame = (m_VibratFrame = _Frame);
									m_VibratSpeed = _Speed;
									m_OneTurnFrame = m_VibratFrame / m_VibratSpeed;
									VEC_Set(m_VibratOffset, _OffsetX, _OffsetY, _OffsetZ);
									VEC_Set(m_PeakPos, 0, 0, 0);
									VEC_Set(m_PeakOffset, 0, 0, 0);
								}

								public void startVibration1(int _Frame, int _Speed, int _OffsetX, int _OffsetY, int _OffsetZ, bool _Turn)
								{
									startVibration(VIBRATION_STATE.VIBRATION_EXE_1, _Frame, _Speed, _OffsetX, _OffsetY, _OffsetZ, _Turn);
								}

								public void startVibration2(int _Frame, int _Speed, int _OffsetX, int _OffsetY, int _OffsetZ, bool _Turn)
								{
									startVibration(VIBRATION_STATE.VIBRATION_EXE_2, _Frame, _Speed, _OffsetX, _OffsetY, _OffsetZ, _Turn);
								}

								public void copy(CCameraVibration src)
								{
									m_Turn = src.m_Turn;
									m_State = src.m_State;
									m_CountFrame = src.m_CountFrame;
									m_VibratFrame = src.m_VibratFrame;
									m_VibratSpeed = src.m_VibratSpeed;
									m_OneTurnFrame = src.m_OneTurnFrame;
									m_VibratOffset.copy(src.m_VibratOffset);
									m_PeakPos.copy(src.m_PeakPos);
									m_PeakOffset.copy(src.m_PeakOffset);
								}
							}

							public class CWorldCamera : ds.sys3d.CCamera
							{
								public enum MODE
								{
									MODE_ERR = -1,
									MODE_AUTOFOLLOW_DEFAULT,
									MODE_AUTOFOLLOW,
									MODE_FREE,
									MODE_MAX
								}

								public enum TYPE
								{
									TYPE_ERR = -1,
									TYPE_FIELD,
									TYPE_TOWN_OUT,
									TYPE_TOWN_IN,
									TYPE_DUNGEON,
									TYPE_MAX
								}

								public enum MOVE_TYPE
								{
									MOVE_TYPE_ERR = -1,
									MOVE_TYPE_UNI,
									MOVE_TYPE_ACC_DEC,
									MOVE_TYPE_ACC,
									MOVE_TYPE_DEC,
									MOVE_TYPE_MAX
								}

								private delegate void _control();

								public const MODE MODE_ERR = MODE.MODE_ERR;

								public const MODE MODE_AUTOFOLLOW_DEFAULT = MODE.MODE_AUTOFOLLOW_DEFAULT;

								public const MODE MODE_AUTOFOLLOW = MODE.MODE_AUTOFOLLOW;

								public const MODE MODE_FREE = MODE.MODE_FREE;

								public const MODE MODE_MAX = MODE.MODE_MAX;

								public const TYPE TYPE_ERR = TYPE.TYPE_ERR;

								public const TYPE TYPE_FIELD = TYPE.TYPE_FIELD;

								public const TYPE TYPE_TOWN_OUT = TYPE.TYPE_TOWN_OUT;

								public const TYPE TYPE_TOWN_IN = TYPE.TYPE_TOWN_IN;

								public const TYPE TYPE_DUNGEON = TYPE.TYPE_DUNGEON;

								public const TYPE TYPE_MAX = TYPE.TYPE_MAX;

								public const MOVE_TYPE MOVE_TYPE_ERR = MOVE_TYPE.MOVE_TYPE_ERR;

								public const MOVE_TYPE MOVE_TYPE_UNI = MOVE_TYPE.MOVE_TYPE_UNI;

								public const MOVE_TYPE MOVE_TYPE_ACC_DEC = MOVE_TYPE.MOVE_TYPE_ACC_DEC;

								public const MOVE_TYPE MOVE_TYPE_ACC = MOVE_TYPE.MOVE_TYPE_ACC;

								public const MOVE_TYPE MOVE_TYPE_DEC = MOVE_TYPE.MOVE_TYPE_DEC;

								public const MOVE_TYPE MOVE_TYPE_MAX = MOVE_TYPE.MOVE_TYPE_MAX;

								public static uint m_CameraFps = 60u;

								public static bool m_Debug = false;

								private static uint frame = 0u;

								private _control[] control = new _control[3];

								public bool m_IsCollision;

								public bool m_Activity;

								public new MODE m_Mode;

								public TYPE m_Type;

								public bool m_ZoomChange;

								public bool m_NowZoom;

								public bool m_Margin;

								public bool m_isOperateZoom;

								public MOVE_TYPE m_PosOffsetMoveType;

								public MOVE_TYPE m_TrgOffsetMoveType;

								public int m_PosOffsetSpeed;

								public int m_TrgOffsetSpeed;

								public VecFx32 m_SucPosOffset = new VecFx32();

								public VecFx32 m_SucTrgOffset = new VecFx32();

								public VecFx32 m_Pos = new VecFx32();

								public VecFx32 m_PrePos = new VecFx32();

								public VecFx32 m_Trg = new VecFx32();

								public VecFx32 m_PreTrg = new VecFx32();

								public VecFx32 m_SucTrg = new VecFx32();

								public VecFx32 m_Angle = new VecFx32();

								public VecFx32 m_PosOffset = new VecFx32();

								public VecFx32 m_TrgOffset = new VecFx32();

								public VecFx32 m_AngleOffset = new VecFx32();

								public VecFx32 m_TransVec = new VecFx32();

								public VecFx32 m_SavePos = new VecFx32();

								public dv.CDeviceManager m_pDevice;

								public CCameraZoom composit = new CCameraZoom();

								public CCameraVibration composit2 = new CCameraVibration();

								public CWorldCamera()
								{
									m_Mode = MODE.MODE_ERR;
									m_Type = TYPE.TYPE_ERR;
									m_pDevice = dv.CDeviceManager.getInstance();
									m_Margin = true;
								}

								public new void initialize()
								{
									base.initialize();
									setMoveMode(0);
									setMCLCollision(_flag: true);
									composit.initialize();
									composit2.initialize();
									redSetActivity(_b: true);
									m_IsCollision = false;
									m_Activity = true;
									m_PosOffsetMoveType = MOVE_TYPE.MOVE_TYPE_ERR;
									m_TrgOffsetMoveType = MOVE_TYPE.MOVE_TYPE_ERR;
									m_PosOffsetSpeed = 0;
									m_TrgOffsetSpeed = 0;
									VEC_Set(m_SucPosOffset, 0, 0, 0);
									VEC_Set(m_SucTrgOffset, 0, 0, 0);
									m_Mode = MODE.MODE_ERR;
									m_Type = TYPE.TYPE_ERR;
									m_ZoomChange = false;
									m_NowZoom = false;
									m_isOperateZoom = false;
									reset();
									registerMode();
								}

								public new void execute()
								{
									control[(int)m_Mode]();
									calculatePositionOffset();
									calculateTargetOffset();
								}

								public void terminate()
								{
								}

								public void reset()
								{
									VEC_Set(Pos(), 0, 0, 0);
									VEC_Set(PrePos(), 0, 0, 0);
									VEC_Set(Trg(), 0, 0, 0);
									VEC_Set(SucTrg(), 0, 0, 0);
									VEC_Set(Angle(), 0, 0, 0);
									VEC_Set(PosOffset(), 0, 0, 0);
									VEC_Set(TrgOffset(), 0, 0, 0);
									VEC_Set(AngleOffset(), 0, 0, 0);
									VEC_Set(TransVec(), 0, 0, 0);
								}

								public void update()
								{
									if (m_Mode == MODE.MODE_ERR || m_Type == TYPE.TYPE_ERR)
									{
										return;
									}
									PrePos_set(Pos());
									PreTrg_set(Trg());
									m_SavePos.copy(getPosition());
									if (m_Mode == MODE.MODE_AUTOFOLLOW_DEFAULT || m_Mode == MODE.MODE_AUTOFOLLOW)
									{
										VecFx32 cmr_reuse_v = cmr_reuse_v0;
										VecFx32 cmr_reuse_v2 = cmr_reuse_v1;
										VecFx32 cmr_reuse_v3 = cmr.cmr_reuse_v2;
										cmr_reuse_v.copy(Pos());
										cmr_reuse_v2.copy(Trg());
										cmr_reuse_v3.copy(Angle());
										if (cmr_reuse_v.x == 0 && cmr_reuse_v.y == 0 && cmr_reuse_v.z == 0)
										{
											return;
										}
										if (cmr_reuse_v2.x == 0 && cmr_reuse_v2.y == 0 && cmr_reuse_v2.z == 0)
										{
											cmr_reuse_v2.y = 4096;
										}
										if (cmr_reuse_v3.x == 0 && cmr_reuse_v3.y == 0 && cmr_reuse_v3.z == 0)
										{
											m_Angle.y = 4096;
										}
										if (m_PosOffset.x != 0 || m_PosOffset.y != 0 || m_PosOffset.z != 0)
										{
											VEC_Add(cmr_reuse_v, m_PosOffset, cmr_reuse_v);
										}
										if (m_TrgOffset.x != 0 || m_TrgOffset.y != 0 || m_TrgOffset.z != 0)
										{
											VEC_Add(cmr_reuse_v2, m_TrgOffset, cmr_reuse_v2);
										}
										if (m_AngleOffset.x != 0 || m_AngleOffset.y != 0 || m_AngleOffset.z != 0)
										{
											VEC_Add(cmr_reuse_v3, m_AngleOffset, cmr_reuse_v3);
										}
										VecFx32 cmr_reuse_v4 = cmr.cmr_reuse_v3;
										cmr_reuse_v4.set(0, 0, 0);
										if (m_Type == TYPE.TYPE_FIELD)
										{
											VEC_Set(cmr_reuse_v4, 0, 1, 1);
										}
										else if (m_Type == TYPE.TYPE_TOWN_OUT)
										{
											VEC_Set(cmr_reuse_v4, 0, 1, 1);
										}
										else if (m_Type == TYPE.TYPE_TOWN_IN)
										{
											VEC_Set(cmr_reuse_v4, 0, 2, 1);
										}
										else if (m_Type == TYPE.TYPE_DUNGEON)
										{
											VEC_Set(cmr_reuse_v4, 0, 2, 1);
										}
										else if (m_Type == TYPE.TYPE_MAX)
										{
											VEC_Set(cmr_reuse_v4, 0, 0, 1);
										}
										composit.update(cmr_reuse_v, cmr_reuse_v4);
										if (m_Mode == MODE.MODE_AUTOFOLLOW_DEFAULT && composit.ZoomMax() != composit.ZoomMin())
										{
											int num = (int)(16384L * (long)(composit.Zoom() - composit.ZoomMin()) / (composit.ZoomMax() - composit.ZoomMin()));
											cmr_reuse_v.y += num;
											cmr_reuse_v2.y += num;
										}
										setPosition(cmr_reuse_v);
										setTarget(cmr_reuse_v2);
									}
									if (m_Activity)
									{
										base.execute();
									}
									if (strcmp(wld.CWorldOutSideData.getInstance().MapData().getNowMapName(), "t04_08") == 0)
									{
										setFOV(954, 3956);
									}
								}

								public void registerMode()
								{
									control[0] = controlAutoFollowDefault;
									control[1] = controlAutoFollow;
									control[2] = controlFree;
								}

								public void controlAutoFollowDefault()
								{
									VecFx32 vecFx = SucTrg();
									VecFx32 cmr_reuse_v = cmr_reuse_v0;
									int num = 16384;
									VEC_Subtract(Trg(), vecFx, cmr_reuse_v);
									if (cmr_reuse_v.x != 0 || cmr_reuse_v.y != 0 || cmr_reuse_v.z != 0)
									{
										VEC_Normalize(cmr_reuse_v, cmr_reuse_v);
									}
									int num2 = VEC_Distance(vecFx, Trg());
									if (!m_Margin || num2 >= num)
									{
										cmr_reuse_v.x = FX_Mul(cmr_reuse_v.x, num);
										cmr_reuse_v.y = FX_Mul(cmr_reuse_v.y, num);
										cmr_reuse_v.z = FX_Mul(cmr_reuse_v.z, num);
										VEC_Add(vecFx, cmr_reuse_v, Trg());
									}
									composit2.update(Trg());
									Pos_set(Trg());
								}

								public void controlAutoFollow()
								{
									VecFx32 vecFx = SucTrg();
									VecFx32 cmr_reuse_v = cmr_reuse_v0;
									VEC_Subtract(vecFx, Trg(), cmr_reuse_v);
									if (cmr_reuse_v.x != 0 || cmr_reuse_v.y != 0 || cmr_reuse_v.z != 0)
									{
										VEC_Normalize(cmr_reuse_v, cmr_reuse_v);
									}
									cmr_reuse_v.x /= 682;
									cmr_reuse_v.y /= 682;
									cmr_reuse_v.z /= 682;
									int num = VEC_Distance(vecFx, Trg());
									if (!m_Margin || num > 15000)
									{
										int num2 = 0;
										if (m_CameraFps == 60)
										{
											num2 = ((num > 20000) ? 64 : 128);
										}
										else if (m_CameraFps == 30)
										{
											num2 = 64;
										}
										num /= num2;
										cmr_reuse_v.x *= num;
										cmr_reuse_v.y *= num;
										cmr_reuse_v.z *= num;
										VEC_Add(Trg(), cmr_reuse_v, Trg());
									}
									composit2.update(Trg());
									Pos_set(Trg());
								}

								public void controlFree()
								{
									if (!m_Debug)
									{
										return;
									}
									int num = 4096;
									if ((ds.g_Pad.pad() & 4) != 0)
									{
										reset();
									}
									else if ((ds.g_Pad.pad() & 2) != 0)
									{
										if ((ds.g_Pad.pad() & 0x40) != 0)
										{
											Angle().x -= 40;
										}
										if ((ds.g_Pad.pad() & 0x80) != 0)
										{
											Angle().x += 40;
										}
										if ((ds.g_Pad.pad() & 0x20) != 0)
										{
											Angle().y += 40;
										}
										if ((ds.g_Pad.pad() & 0x10) != 0)
										{
											Angle().y -= 40;
										}
									}
									else
									{
										if ((ds.g_Pad.pad() & 0x20) != 0)
										{
											m_TransVec.x = num;
										}
										if ((ds.g_Pad.pad() & 0x10) != 0)
										{
											m_TransVec.x = -num;
										}
										if ((ds.g_Pad.pad() & 1) != 0)
										{
											if ((ds.g_Pad.pad() & 0x40) != 0)
											{
												m_TransVec.y = num;
											}
											if ((ds.g_Pad.pad() & 0x80) != 0)
											{
												m_TransVec.y = -num;
											}
										}
										else
										{
											if ((ds.g_Pad.pad() & 0x40) != 0)
											{
												m_TransVec.z = num;
											}
											if ((ds.g_Pad.pad() & 0x80) != 0)
											{
												m_TransVec.z = -num;
											}
										}
									}
									frame++;
									if (frame >= 120)
									{
										frame = 0u;
									}
								}

								public void calculatePositionOffset()
								{
									if (m_PosOffsetMoveType != MOVE_TYPE.MOVE_TYPE_ERR)
									{
										VecFx32 cmr_reuse_v = cmr_reuse_v0;
										VecFx32 cmr_reuse_v2 = cmr_reuse_v1;
										VecFx32 cmr_reuse_v3 = cmr.cmr_reuse_v2;
										cmr_reuse_v.copy(PosOffset());
										cmr_reuse_v2.copy(m_SucPosOffset);
										int num = 0;
										VEC_Subtract(cmr_reuse_v2, cmr_reuse_v, cmr_reuse_v3);
										num = VEC_Mag(cmr_reuse_v3);
										VEC_Normalize(cmr_reuse_v3, cmr_reuse_v3);
										if (m_PosOffsetMoveType == MOVE_TYPE.MOVE_TYPE_ACC_DEC)
										{
											m_PosOffsetSpeed = FX_Div(num, m_PosOffsetSpeed << 12);
										}
										else if (m_PosOffsetMoveType == MOVE_TYPE.MOVE_TYPE_ACC)
										{
											m_PosOffsetSpeed = num - FX_Div(num, m_PosOffsetSpeed << 12);
										}
										else if (m_PosOffsetMoveType == MOVE_TYPE.MOVE_TYPE_DEC)
										{
											m_PosOffsetSpeed = FX_Div(num, m_PosOffsetSpeed);
										}
										VEC_Set(cmr_reuse_v3, FX_Mul(cmr_reuse_v3.x, m_PosOffsetSpeed), FX_Mul(cmr_reuse_v3.y, m_PosOffsetSpeed), FX_Mul(cmr_reuse_v3.z, m_PosOffsetSpeed));
										VEC_Add(cmr_reuse_v, cmr_reuse_v3, cmr_reuse_v);
										VEC_Subtract(cmr_reuse_v2, cmr_reuse_v, cmr_reuse_v3);
										num = VEC_Mag(cmr_reuse_v3);
										if (num <= 1024)
										{
											m_PosOffsetMoveType = MOVE_TYPE.MOVE_TYPE_ERR;
										}
										setPosOffset(cmr_reuse_v);
									}
								}

								public void calculateTargetOffset()
								{
									if (m_TrgOffsetMoveType != MOVE_TYPE.MOVE_TYPE_ERR)
									{
										VecFx32 cmr_reuse_v = cmr_reuse_v0;
										VecFx32 cmr_reuse_v2 = cmr_reuse_v1;
										VecFx32 cmr_reuse_v3 = cmr.cmr_reuse_v2;
										cmr_reuse_v.copy(TrgOffset());
										cmr_reuse_v2.copy(m_SucTrgOffset);
										int num = 0;
										VEC_Subtract(cmr_reuse_v2, cmr_reuse_v, cmr_reuse_v3);
										num = VEC_Mag(cmr_reuse_v3);
										VEC_Normalize(cmr_reuse_v3, cmr_reuse_v3);
										if (m_TrgOffsetMoveType == MOVE_TYPE.MOVE_TYPE_ACC_DEC)
										{
											m_TrgOffsetSpeed = FX_Div(num, m_TrgOffsetSpeed << 12);
										}
										else if (m_TrgOffsetMoveType == MOVE_TYPE.MOVE_TYPE_ACC)
										{
											m_TrgOffsetSpeed = num - FX_Div(num, m_TrgOffsetSpeed << 12);
										}
										else if (m_TrgOffsetMoveType == MOVE_TYPE.MOVE_TYPE_DEC)
										{
											m_TrgOffsetSpeed = FX_Div(num, m_TrgOffsetSpeed << 12);
										}
										VEC_Set(cmr_reuse_v3, FX_Mul(cmr_reuse_v3.x, m_TrgOffsetSpeed), FX_Mul(cmr_reuse_v3.y, m_TrgOffsetSpeed), FX_Mul(cmr_reuse_v3.z, m_TrgOffsetSpeed));
										VEC_Add(cmr_reuse_v, cmr_reuse_v3, cmr_reuse_v);
										VEC_Subtract(cmr_reuse_v2, cmr_reuse_v, cmr_reuse_v3);
										num = VEC_Mag(cmr_reuse_v3);
										if (num <= 1024)
										{
											m_TrgOffsetMoveType = MOVE_TYPE.MOVE_TYPE_ERR;
										}
										setTrgOffset(cmr_reuse_v);
									}
								}

								public override void move()
								{
									if (m_Mode == MODE.MODE_ERR || m_Type == TYPE.TYPE_ERR)
									{
										return;
									}
									if (composit.ZoomEnable())
									{
										ushort num = 0;
										num = (ushort)((opt.COptionManager.getSingleton().gameOption().menuZoomSetting() != opt.MENU_ZOOM_SETTING.MENU_R_ZOOM_L) ? 512 : 256);
										if ((Pad().pad_trs(0) & num) != 0 || m_isOperateZoom)
										{
											if (!NowZoom())
											{
												composit.ZoomState_set(CCameraZoom.ZOOM_STATE.ZOOM_MANUAL_IN);
											}
											else
											{
												composit.ZoomState_set(CCameraZoom.ZOOM_STATE.ZOOM_MANUAL_OUT);
											}
											ZoomChange_set(arg0: true);
										}
										else if (ZoomChange())
										{
											NowZoom_set(!NowZoom());
											ZoomChange_set(arg0: false);
										}
									}
									composit.execute();
								}

								public override void calculate()
								{
									if (m_Mode == MODE.MODE_ERR || m_Type == TYPE.TYPE_ERR)
									{
										return;
									}
									MtxFx43 cmr_reuse_mtxDisTrans = cmr.cmr_reuse_mtxDisTrans;
									MtxFx43 cmr_reuse_mtxRot = cmr.cmr_reuse_mtxRot;
									MtxFx43 cmr_reuse_mtxRotX = cmr.cmr_reuse_mtxRotX;
									MtxFx43 cmr_reuse_mtxRotY = cmr.cmr_reuse_mtxRotY;
									MtxFx43 cmr_reuse_mtxConv = cmr.cmr_reuse_mtxConv;
									MtxFx43 cmr_reuse_mtxInvRot = cmr.cmr_reuse_mtxInvRot;
									MTX_Identity43(cmr_reuse_mtxDisTrans);
									MTX_Identity43(cmr_reuse_mtxRot);
									MTX_Identity43(cmr_reuse_mtxRotX);
									MTX_Identity43(cmr_reuse_mtxRotY);
									MTX_Identity43(cmr_reuse_mtxConv);
									if (m_Mode != MODE.MODE_AUTOFOLLOW_DEFAULT && m_Mode != MODE.MODE_AUTOFOLLOW && m_Mode == MODE.MODE_FREE)
									{
										MTX_TransApply43(cmr_reuse_mtxDisTrans, cmr_reuse_mtxDisTrans, 0, 0, -m_Distance);
										ds.CpuMatrix.setRotateY(cmr_reuse_mtxRotY, Angle().y);
										ds.CpuMatrix.setRotateX(cmr_reuse_mtxRotX, Angle().x);
										MTX_Concat43(cmr_reuse_mtxRotX, cmr_reuse_mtxRotY, cmr_reuse_mtxRot);
										MTX_Inverse43(cmr_reuse_mtxRot, cmr_reuse_mtxInvRot);
										VEC_Set(m_CamInfo.position, 0, 0, 0);
										MTX_Concat43(cmr_reuse_mtxDisTrans, cmr_reuse_mtxRot, cmr_reuse_mtxConv);
										MTX_MultVec43(m_CamInfo.position, cmr_reuse_mtxConv, m_CamInfo.position);
										MTX_MultVec43(m_TransVec, cmr_reuse_mtxRot, m_TransVec);
										m_CamInfo.target.x += m_TransVec.x;
										m_CamInfo.target.y += m_TransVec.y;
										m_CamInfo.target.z += m_TransVec.z;
										m_CamInfo.position.x += m_CamInfo.target.x;
										m_CamInfo.position.y += m_CamInfo.target.y;
										m_CamInfo.position.z += m_CamInfo.target.z;
										if (strcmp(sceneMng.getStage(), "d01_02_e01") == 0)
										{
											VEC_Set(m_CamInfo.position, -16384, -557056, 0);
										}
										Trg_set(m_CamInfo.target);
										Pos_set(m_CamInfo.position);
										VEC_Set(m_TransVec, 0, 0, 0);
									}
									m_CamInfo.camUp.set(0, 4096, 0);
									MTX_MultVec43(m_CamInfo.camUp, cmr_reuse_mtxRot, m_CamInfo.camUp);
								}

								public override void dgsredAccept(dgs.CRestrictor ror)
								{
									if (m_Mode == MODE.MODE_ERR || m_Type == TYPE.TYPE_ERR)
									{
										return;
									}
									mcl.CollisionResult cmr_reuse_result = cmr.cmr_reuse_result;
									VecFx32 cmr_reuse_v = cmr_reuse_v0;
									VecFx32 cmr_reuse_v2 = cmr_reuse_v1;
									VecFx32 cmr_reuse_v3 = cmr.cmr_reuse_v2;
									cmr_reuse_v.copy(PrePos());
									cmr_reuse_v2.copy(Pos());
									int num = 0;
									bool flag = false;
									int num2 = 216;
									VEC_Subtract(cmr_reuse_v2, cmr_reuse_v, cmr_reuse_v3);
									if (cmr_reuse_v3.x == 0 && cmr_reuse_v3.y == 0 && cmr_reuse_v3.z == 0)
									{
										return;
									}
									VecFx32 cmr_reuse_v4 = cmr.cmr_reuse_v3;
									num = VEC_Mag(cmr_reuse_v3);
									VEC_Normalize(cmr_reuse_v3, cmr_reuse_v4);
									if (ror.rorEvaluateArrow(cmr_reuse_v, cmr_reuse_v4, num, 7, cmr_reuse_result))
									{
										flag = true;
										int num3 = -VEC_DotProduct(cmr_reuse_result.normal, cmr_reuse_v3);
										VecFx32 cmr_reuse_v5 = cmr.cmr_reuse_v4;
										int num4 = 0;
										VEC_Subtract(cmr_reuse_result.v0, cmr_reuse_v, cmr_reuse_v5);
										num4 = -VEC_DotProduct(cmr_reuse_result.normal, cmr_reuse_v5);
										int a = num3 - num4;
										VEC_MultAdd(a, cmr_reuse_result.normal, cmr_reuse_v2, cmr_reuse_v2);
										VEC_MultAdd(-num2, cmr_reuse_v4, cmr_reuse_v2, cmr_reuse_v2);
										VEC_MultAdd(-num2, cmr_reuse_v4, cmr_reuse_result.pos, cmr_reuse_v);
										VEC_Subtract(cmr_reuse_v2, cmr_reuse_v, cmr_reuse_v3);
										num = VEC_Mag(cmr_reuse_v3);
										VEC_Normalize(cmr_reuse_v3, cmr_reuse_v4);
										mcl.CollisionResult cmr_reuse_result2 = cmr.cmr_reuse_result2;
										if (ror.rorEvaluateArrow(cmr_reuse_v, cmr_reuse_v4, num, 7, cmr_reuse_result2))
										{
											int num5 = cmr_reuse_result.v0.z - cmr_reuse_result.v1.z;
											int num6 = cmr_reuse_result.v1.x - cmr_reuse_result.v0.x;
											long num7 = (long)cmr_reuse_result.v0.z * (long)cmr_reuse_result.v0.x - (long)cmr_reuse_result.v0.z * (long)cmr_reuse_result.v1.x - (long)cmr_reuse_result.v0.x * (long)cmr_reuse_result.v0.z + (long)cmr_reuse_result.v0.x * (long)cmr_reuse_result.v1.z;
											int num8 = (int)(num7 >> 12);
											int num9 = cmr_reuse_result2.v0.z - cmr_reuse_result2.v1.z;
											int num10 = cmr_reuse_result2.v1.x - cmr_reuse_result2.v0.x;
											num7 = (long)cmr_reuse_result2.v0.z * (long)cmr_reuse_result2.v0.x - (long)cmr_reuse_result2.v0.z * (long)cmr_reuse_result2.v1.x - (long)cmr_reuse_result2.v0.x * (long)cmr_reuse_result2.v0.z + (long)cmr_reuse_result2.v0.x * (long)cmr_reuse_result2.v1.z;
											int num11 = (int)(num7 >> 12);
											long num12 = (long)num5 * (long)num10 - (long)num9 * (long)num6;
											long num13 = (long)(-num10) * (long)num8 + (long)num6 * (long)num11;
											long num14 = (long)num9 * (long)num8 - (long)num5 * (long)num11;
											if (0 != num12)
											{
												VecFx32 cmr_reuse_v6 = cmr.cmr_reuse_v5;
												cmr_reuse_v6.x = (int)(num13 / num12 << 12);
												cmr_reuse_v6.y = cmr_reuse_v2.y;
												cmr_reuse_v6.z = (int)(num14 / num12 << 12);
												VecFx32 cmr_reuse_v7 = cmr.cmr_reuse_v6;
												cmr_reuse_v7.x = cmr_reuse_result.normal.x + cmr_reuse_result2.normal.x;
												cmr_reuse_v7.y = cmr_reuse_result.normal.y + cmr_reuse_result2.normal.y;
												cmr_reuse_v7.z = cmr_reuse_result.normal.z + cmr_reuse_result2.normal.z;
												VEC_Normalize(cmr_reuse_v7, cmr_reuse_v7);
												VEC_MultAdd(num2, cmr_reuse_v7, cmr_reuse_v6, cmr_reuse_v2);
											}
										}
									}
									if (flag)
									{
										composit.setZoomEnable(b: false);
										composit.setZoomState(CCameraZoom.ZOOM_STATE.ZOOM_AUTO_OUT);
										NowZoom_set(arg0: false);
										Pos_set(cmr_reuse_v2);
										Trg_set(cmr_reuse_v2);
									}
								}

								public bool IsCollision()
								{
									return m_IsCollision;
								}

								public bool getActivity()
								{
									return m_Activity;
								}

								public void setActivity(bool b)
								{
									m_Activity = b;
								}

								public void setMode(MODE _Mode)
								{
									m_Mode = _Mode;
								}

								public MODE Mode()
								{
									return m_Mode;
								}

								public void Mode_set(MODE arg0)
								{
									m_Mode = arg0;
								}

								public void setType(TYPE _Type)
								{
									m_Type = _Type;
								}

								public TYPE Type()
								{
									return m_Type;
								}

								public void Type_set(TYPE arg0)
								{
									m_Type = arg0;
								}

								public bool isMCLCollision()
								{
									return redActivity();
								}

								public void setMCLCollision(bool _flag)
								{
									redSetActivity(_flag);
								}

								public void setPosOffsetMoveType(MOVE_TYPE _PosOffsetMoveType)
								{
									m_PosOffsetMoveType = _PosOffsetMoveType;
								}

								public void setTrgOffsetMoveType(MOVE_TYPE _TrgOffsetMoveType)
								{
									m_TrgOffsetMoveType = _TrgOffsetMoveType;
								}

								public void setPosOffsetSpeed(int _PosOffsetSpeed)
								{
									m_PosOffsetSpeed = _PosOffsetSpeed;
								}

								public void setTrgOffsetSpeed(int _TrgOffsetSpeed)
								{
									m_TrgOffsetSpeed = _TrgOffsetSpeed;
								}

								public void setSucPosOffset(VecFx32 _SucPosOffset)
								{
									m_SucPosOffset.copy(_SucPosOffset);
								}

								public void setSucTrgOffset(VecFx32 _SucTrgOffset)
								{
									m_SucTrgOffset.copy(_SucTrgOffset);
								}

								public void setPos(int _PosX, int _PosY, int _PosZ)
								{
									cmr_reuse_v0.set(_PosX, _PosY, _PosZ);
									setPos(cmr_reuse_v0);
								}

								public void setPos(VecFx32 _Pos)
								{
									m_Pos.copy(_Pos);
								}

								public VecFx32 Pos()
								{
									return m_Pos;
								}

								public void Pos_set(VecFx32 arg0)
								{
									m_Pos.copy(arg0);
								}

								public void setPrePos(int _PrePosX, int _PrePosY, int _PrePosZ)
								{
									cmr_reuse_v0.set(_PrePosX, _PrePosY, _PrePosZ);
									setPrePos(cmr_reuse_v0);
								}

								public void setPrePos(VecFx32 _PrePos)
								{
									m_PrePos.copy(_PrePos);
								}

								public VecFx32 PrePos()
								{
									return m_PrePos;
								}

								public void PrePos_set(VecFx32 arg0)
								{
									m_PrePos.copy(arg0);
								}

								public void setTrg(int _TrgX, int _TrgY, int _TrgZ)
								{
									cmr_reuse_v0.set(_TrgX, _TrgY, _TrgZ);
									setTrg(cmr_reuse_v0);
								}

								public void setTrg(VecFx32 _Trg)
								{
									m_Trg.copy(_Trg);
								}

								public VecFx32 Trg()
								{
									return m_Trg;
								}

								public void Trg_set(VecFx32 arg0)
								{
									m_Trg.copy(arg0);
								}

								public void setPreTrg(int _PreTrgX, int _PreTrgY, int _PreTrgZ)
								{
									cmr_reuse_v0.set(_PreTrgX, _PreTrgY, _PreTrgZ);
									setPreTrg(cmr_reuse_v0);
								}

								public void setPreTrg(VecFx32 _PreTrg)
								{
									m_PreTrg.copy(_PreTrg);
								}

								public VecFx32 PreTrg()
								{
									return m_PreTrg;
								}

								public void PreTrg_set(VecFx32 arg0)
								{
									m_PreTrg.copy(arg0);
								}

								public void setSucTrg(int _SucTrgX, int _SucTrgY, int _SucTrgZ)
								{
									cmr_reuse_v0.set(_SucTrgX, _SucTrgY, _SucTrgZ);
									setSucTrg(cmr_reuse_v0);
								}

								public void setSucTrg(VecFx32 _SucTrg)
								{
									m_SucTrg.copy(_SucTrg);
								}

								public VecFx32 SucTrg()
								{
									return m_SucTrg;
								}

								public new void setAngle(int _AngleX, int _AngleY, int _AngleZ)
								{
									cmr_reuse_v0.set(_AngleX, _AngleY, _AngleZ);
									setAngle(cmr_reuse_v0);
								}

								public void setAngle(VecFx32 _Angle)
								{
									m_Angle.copy(_Angle);
								}

								public VecFx32 Angle()
								{
									return m_Angle;
								}

								public void Angle_set(VecFx32 arg0)
								{
									m_Angle.copy(arg0);
								}

								public void setPosOffset(int _PosOffsetX, int _PosOffsetY, int _PosOffsetZ)
								{
									cmr_reuse_v0.set(_PosOffsetX, _PosOffsetY, _PosOffsetZ);
									setPosOffset(cmr_reuse_v0);
								}

								public void setPosOffset(VecFx32 _PosOffset)
								{
									m_PosOffset.copy(_PosOffset);
								}

								public VecFx32 PosOffset()
								{
									return m_PosOffset;
								}

								public void setTrgOffset(int _TrgOffsetX, int _TrgOffsetY, int _TrgOffsetZ)
								{
									cmr_reuse_v0.set(_TrgOffsetX, _TrgOffsetY, _TrgOffsetZ);
									setTrgOffset(cmr_reuse_v0);
								}

								public void setTrgOffset(VecFx32 _TrgOffset)
								{
									m_TrgOffset.copy(_TrgOffset);
								}

								public VecFx32 TrgOffset()
								{
									return m_TrgOffset;
								}

								public void setAngleOffset(int _AngleOffsetX, int _AngleOffsetY, int _AngleOffsetZ)
								{
									cmr_reuse_v0.set(_AngleOffsetX, _AngleOffsetY, _AngleOffsetZ);
									setTrgOffset(cmr_reuse_v0);
								}

								public void setAngleOffset(VecFx32 _AngleOffset)
								{
									m_AngleOffset.copy(_AngleOffset);
								}

								public VecFx32 AngleOffset()
								{
									return m_AngleOffset;
								}

								public void setTransVec(int _TransVecX, int _TransVecY, int _TransVecZ)
								{
									cmr_reuse_v0.set(_TransVecX, _TransVecY, _TransVecZ);
									setTransVec(cmr_reuse_v0);
								}

								public void setTransVec(VecFx32 _TransVec)
								{
									m_TransVec.copy(_TransVec);
								}

								public VecFx32 TransVec()
								{
									return m_TransVec;
								}

								public bool ZoomChange()
								{
									return m_ZoomChange;
								}

								public void ZoomChange_set(bool arg0)
								{
									m_ZoomChange = arg0;
								}

								public bool NowZoom()
								{
									return m_NowZoom;
								}

								public void NowZoom_set(bool arg0)
								{
									m_NowZoom = arg0;
								}

								public void setOperateZoom(bool b)
								{
									m_isOperateZoom = b;
								}

								public dv.CDeviceManager Dv()
								{
									return m_pDevice;
								}

								public dv.pad.CPlayerPad Pad()
								{
									return m_pDevice.Pad();
								}

								public void applyMargin(bool f)
								{
									m_Margin = f;
								}

								public void copy(CWorldCamera src)
								{
									copy((ds.sys3d.CCamera)src);
									for (int i = 0; i < control.Length; i++)
									{
										control[i] = src.control[i];
									}
									m_IsCollision = src.m_IsCollision;
									m_Activity = src.m_Activity;
									m_Mode = src.m_Mode;
									m_Type = src.m_Type;
									m_ZoomChange = src.m_ZoomChange;
									m_NowZoom = src.m_NowZoom;
									m_Margin = src.m_Margin;
									m_isOperateZoom = src.m_isOperateZoom;
									m_PosOffsetMoveType = src.m_PosOffsetMoveType;
									m_TrgOffsetMoveType = src.m_TrgOffsetMoveType;
									m_PosOffsetSpeed = src.m_PosOffsetSpeed;
									m_TrgOffsetSpeed = src.m_TrgOffsetSpeed;
									m_SucPosOffset.copy(src.m_SucPosOffset);
									m_SucTrgOffset.copy(src.m_SucTrgOffset);
									m_Pos.copy(src.m_Pos);
									m_PrePos.copy(src.m_PrePos);
									m_Trg.copy(src.m_Trg);
									m_PreTrg.copy(src.m_PreTrg);
									m_SucTrg.copy(src.m_SucTrg);
									m_Angle.copy(src.m_Angle);
									m_PosOffset.copy(src.m_PosOffset);
									m_TrgOffset.copy(src.m_TrgOffset);
									m_AngleOffset.copy(src.m_AngleOffset);
									m_TransVec.copy(src.m_TransVec);
									m_SavePos.copy(src.m_SavePos);
									m_pDevice = src.m_pDevice;
									composit.copy(src.composit);
									composit2.copy(src.composit2);
								}
							}

							private static VecFx32 cmr_reuse_v0 = new VecFx32();

							private static VecFx32 cmr_reuse_v1 = new VecFx32();

							private static VecFx32 cmr_reuse_v2 = new VecFx32();

							private static VecFx32 cmr_reuse_v3 = new VecFx32();

							private static VecFx32 cmr_reuse_v4 = new VecFx32();

							private static VecFx32 cmr_reuse_v5 = new VecFx32();

							private static VecFx32 cmr_reuse_v6 = new VecFx32();

							private static MtxFx43 cmr_reuse_mtxDisTrans = new MtxFx43();

							private static MtxFx43 cmr_reuse_mtxRot = new MtxFx43();

							private static MtxFx43 cmr_reuse_mtxRotX = new MtxFx43();

							private static MtxFx43 cmr_reuse_mtxRotY = new MtxFx43();

							private static MtxFx43 cmr_reuse_mtxConv = new MtxFx43();

							private static MtxFx43 cmr_reuse_mtxInvRot = new MtxFx43();

							private static mcl.CollisionResult cmr_reuse_result = new mcl.CollisionResult();

							private static mcl.CollisionResult cmr_reuse_result2 = new mcl.CollisionResult();
						}
}
