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
	public static partial class chr
	{
		public class CCharacterMoveSys
		{
			public enum MOVE_TYPE
			{
				MOVE_TYPE_LINEAR,
				MOVE_TYPE_CIRCLE,
				MOVE_TYPE_CORRECTLINEAR
			}

			public enum GRAVITY_TYPE
			{
				GRAVITY_TYPE_NORMAL,
				GRAVITY_TYPE_WAVE
			}

			public const MOVE_TYPE MOVE_TYPE_LINEAR = MOVE_TYPE.MOVE_TYPE_LINEAR;

			public const MOVE_TYPE MOVE_TYPE_CIRCLE = MOVE_TYPE.MOVE_TYPE_CIRCLE;

			public const MOVE_TYPE MOVE_TYPE_CORRECTLINEAR = MOVE_TYPE.MOVE_TYPE_CORRECTLINEAR;

			public const GRAVITY_TYPE GRAVITY_TYPE_NORMAL = GRAVITY_TYPE.GRAVITY_TYPE_NORMAL;

			public const GRAVITY_TYPE GRAVITY_TYPE_WAVE = GRAVITY_TYPE.GRAVITY_TYPE_WAVE;

			private static byte MOVE_TARGETPOINT_NUM = 8;

			private bool m_Flag;

			private byte m_PointNum;

			private byte m_CurrentPoint;

			private int m_MoveFrame;

			private VecFx32[] m_TargetPoint = new VecFx32[MOVE_TARGETPOINT_NUM];

			private CBaseCharacter m_pChara;

			private MOVE_TYPE moveType_;

			private VecFx32 center_ = new VecFx32();

			private int rad_;

			private int xScale_;

			private int zScale_;

			private ushort angle_;

			private ushort w_;

			private bool syncCircle_;

			private ushort yOrgRotate_;

			private bool stop_;

			private GRAVITY_TYPE gravityType_;

			private int amp_;

			private int yBasic_;

			private ushort phase_;

			private ushort phaseSpd_;

			private byte type_;

			public void initialize()
			{
				for (int i = 0; i < m_TargetPoint.Length; i++)
				{
					m_TargetPoint[i] = new VecFx32();
				}
				m_Flag = false;
				m_PointNum = 0;
				m_CurrentPoint = 0;
				m_MoveFrame = 0;
				for (int j = 0; j < MOVE_TARGETPOINT_NUM; j++)
				{
					VEC_Set(m_TargetPoint[j], 0, 0, 0);
				}
				m_pChara = null;
				moveType_ = MOVE_TYPE.MOVE_TYPE_LINEAR;
				VEC_Set(center_, 0, 0, 0);
				rad_ = 0;
				xScale_ = 0;
				zScale_ = 0;
				angle_ = 0;
				w_ = 0;
				syncCircle_ = false;
				yOrgRotate_ = 0;
				stop_ = false;
				amp_ = 0;
				yBasic_ = 0;
				phase_ = 0;
				phaseSpd_ = 0;
				type_ = 0;
				gravityType_ = GRAVITY_TYPE.GRAVITY_TYPE_NORMAL;
			}

			public void execute()
			{
				if (m_pChara != null && !stop_)
				{
					if (m_Flag)
					{
						setMoveTargetDirection();
					}
					if (m_pChara.isAutoPilot())
					{
						normalAutoMove();
					}
				}
			}

			public void terminate()
			{
				reset();
			}

			public void update()
			{
				if (m_pChara == null)
				{
					return;
				}
				if (stop_)
				{
					stop_ = false;
					return;
				}
				switch (moveType_)
				{
				case MOVE_TYPE.MOVE_TYPE_LINEAR:
					updateMoveAcc();
					updateMoveDec();
					updatePosition();
					break;
				case MOVE_TYPE.MOVE_TYPE_CORRECTLINEAR:
					updateMoveAcc();
					updateMoveDec();
					updatePositionCorrect();
					break;
				case MOVE_TYPE.MOVE_TYPE_CIRCLE:
				{
					angle_ += w_;
					int v = FX_SinIdx(angle_);
					int v2 = FX_CosIdx(angle_);
					VecFx32 chr_reuse_v = chr_reuse_v0;
					chr_reuse_v.copy(m_pChara.getPosition());
					chr_reuse_v.x = FX_Mul(FX_Mul(rad_, v2), xScale_) + center_.x;
					chr_reuse_v.z = FX_Mul(FX_Mul(rad_, v), zScale_) + center_.z;
					m_pChara.setPosition(chr_reuse_v);
					if (syncCircle_)
					{
						VecFx32 chr_reuse_v2 = chr_reuse_v1;
						chr_reuse_v2.set(0, (ushort)(angle_ + yOrgRotate_), 0);
						m_pChara.setRotation(chr_reuse_v2);
					}
					break;
				}
				}
				switch (gravityType_)
				{
				case GRAVITY_TYPE.GRAVITY_TYPE_NORMAL:
					updateGravity();
					break;
				case GRAVITY_TYPE.GRAVITY_TYPE_WAVE:
				{
					phase_ += phaseSpd_;
					int v3 = FX_SinIdx(phase_);
					int y = FX_Mul(amp_, v3) + yBasic_;
					m_pChara.getPosition().y = y;
					break;
				}
				}
			}

			public void reset()
			{
				m_Flag = false;
				m_PointNum = 0;
				m_CurrentPoint = 0;
				m_pChara = null;
				for (int i = 0; i < MOVE_TARGETPOINT_NUM; i++)
				{
					m_TargetPoint[i].x = 0;
					m_TargetPoint[i].y = 0;
					m_TargetPoint[i].z = 0;
				}
			}

			public VecFx32 getNextPosition(VecFx32 _pos)
			{
				VecFx32 chr_reuse_v = chr_reuse_v0;
				VecFx32 chr_reuse_v2 = chr_reuse_v1;
				chr_reuse_v.copy(m_pChara.getDirection());
				chr_reuse_v2.copy(m_pChara.getMove());
				int moveAcc = m_pChara.getMoveAcc();
				int moveDec = m_pChara.getMoveDec();
				int moveMax = m_pChara.getMoveMax();
				int grvAcc = m_pChara.getGrvAcc();
				chr_reuse_v2.x += moveAcc;
				chr_reuse_v2.y += moveAcc;
				chr_reuse_v2.z += moveAcc;
				chr_reuse_v2.x -= moveDec;
				chr_reuse_v2.y -= moveDec;
				chr_reuse_v2.z -= moveDec;
				if (chr_reuse_v2.x > moveMax)
				{
					chr_reuse_v2.x = moveMax;
				}
				if (chr_reuse_v2.y > moveMax)
				{
					chr_reuse_v2.y = moveMax;
				}
				if (chr_reuse_v2.z > moveMax)
				{
					chr_reuse_v2.z = moveMax;
				}
				if (chr_reuse_v2.x < 0)
				{
					chr_reuse_v2.x = 0;
				}
				if (chr_reuse_v2.y < 0)
				{
					chr_reuse_v2.y = 0;
				}
				if (chr_reuse_v2.z < 0)
				{
					chr_reuse_v2.z = 0;
				}
				chr_reuse_v.x *= chr_reuse_v2.x;
				chr_reuse_v.y *= chr_reuse_v2.y;
				chr_reuse_v.z *= chr_reuse_v2.z;
				VEC_Add(_pos, chr_reuse_v, _pos);
				if (m_pChara.isGrv())
				{
					grvAcc -= GRAVITY_ACC;
					if (grvAcc > GRAVITY_ACC_MAX)
					{
						grvAcc = GRAVITY_ACC_MAX;
					}
					_pos.y += grvAcc;
				}
				if (_pos.y <= -409600)
				{
					_pos.y = 40960;
				}
				return _pos;
			}

			public void setMoveTargetDirection()
			{
				VecFx32 chr_reuse_v = chr_reuse_v0;
				switch (moveType_)
				{
				default:
					VEC_Subtract(m_TargetPoint[1], m_TargetPoint[0], chr_reuse_v);
					if (!IS_ZERO_NORM(chr_reuse_v))
					{
						VEC_Normalize(chr_reuse_v, chr_reuse_v);
					}
					m_pChara.setTargetDirection(chr_reuse_v);
					break;
				case MOVE_TYPE.MOVE_TYPE_LINEAR:
				{
					VecFx32 chr_reuse_v2 = chr_reuse_v1;
					VecFx32 chr_reuse_v3 = chr.chr_reuse_v2;
					chr_reuse_v2.copy(m_TargetPoint[0]);
					chr_reuse_v3.copy(m_TargetPoint[1]);
					VEC_Subtract(chr_reuse_v3, chr_reuse_v2, chr_reuse_v3);
					if (!IS_ZERO_NORM(chr_reuse_v3))
					{
						VEC_Normalize(chr_reuse_v3, chr_reuse_v3);
					}
					chr_reuse_v3.x /= 682;
					chr_reuse_v3.y /= 682;
					chr_reuse_v3.z /= 682;
					m_pChara.setTargetDirection(chr_reuse_v3);
					break;
				}
				}
			}

			public void normalAutoMove()
			{
				switch (type_)
				{
				case 1:
				{
					VecFx32 chr_reuse_v = chr_reuse_v0;
					VecFx32 chr_reuse_v2 = chr_reuse_v1;
					VEC_Subtract(m_pChara.getPosition(), m_TargetPoint[0], chr_reuse_v);
					VEC_Subtract(m_TargetPoint[1], m_TargetPoint[0], chr_reuse_v2);
					if (VEC_Mag(chr_reuse_v) >= VEC_Mag(chr_reuse_v2))
					{
						m_pChara.setMoveMax(0);
						m_pChara.setMoveAcc(0);
						m_pChara.setMoveDec(0);
						m_pChara.setPosition(m_TargetPoint[1]);
						m_Flag = false;
						type_ = 0;
						moveType_ = MOVE_TYPE.MOVE_TYPE_LINEAR;
					}
					return;
				}
				}
				int num = VEC_Distance(m_pChara.getPosition(), m_TargetPoint[1]) / 4096;
				m_MoveFrame--;
				if (m_MoveFrame < 0)
				{
					m_MoveFrame = 0;
					m_pChara.setMoveMax(0);
					m_pChara.setMoveAcc(0);
					m_pChara.setMoveDec(0);
					m_Flag = false;
				}
				if (num <= 1)
				{
					m_pChara.setMoveMax(0);
					m_pChara.setMoveAcc(0);
					m_pChara.setMoveDec(0);
					m_Flag = false;
				}
			}

			public void bzierAutoMove()
			{
			}

			public void updateMoveAcc()
			{
				VecFx32 chr_reuse_v = chr_reuse_v0;
				chr_reuse_v.copy(m_pChara.getMove());
				int moveAcc = m_pChara.getMoveAcc();
				int moveMax = m_pChara.getMoveMax();
				chr_reuse_v.x += moveAcc;
				chr_reuse_v.y += moveAcc;
				chr_reuse_v.z += moveAcc;
				if (chr_reuse_v.x > moveMax)
				{
					chr_reuse_v.x = moveMax;
				}
				if (chr_reuse_v.y > moveMax)
				{
					chr_reuse_v.y = moveMax;
				}
				if (chr_reuse_v.z > moveMax)
				{
					chr_reuse_v.z = moveMax;
				}
				m_pChara.setMove(chr_reuse_v);
			}

			public void updateMoveDec()
			{
				VecFx32 move = m_pChara.getMove();
				int moveDec = m_pChara.getMoveDec();
				move.x -= moveDec;
				move.y -= moveDec;
				move.z -= moveDec;
				if (move.x < 0)
				{
					move.x = 0;
				}
				if (move.y < 0)
				{
					move.y = 0;
				}
				if (move.z < 0)
				{
					move.z = 0;
				}
				m_pChara.setMove(move);
			}

			public void updatePosition()
			{
				VecFx32 position = m_pChara.getPosition();
				VecFx32 chr_reuse_v = chr_reuse_v0;
				chr_reuse_v.copy(m_pChara.getDirection());
				VecFx32 move = m_pChara.getMove();
				chr_reuse_v.x *= move.x;
				chr_reuse_v.y *= move.y;
				chr_reuse_v.z *= move.z;
				VEC_Add(position, chr_reuse_v, position);
				m_pChara.setPosition(position);
			}

			public void updatePositionCorrect()
			{
				VecFx32 chr_reuse_v = chr_reuse_v0;
				chr_reuse_v.copy(m_pChara.getPosition());
				VEC_MultAdd(m_pChara.getMove().x, m_pChara.getDirection(), chr_reuse_v, chr_reuse_v);
				m_pChara.setPosition(chr_reuse_v);
			}

			public void updateGravity()
			{
				if (m_pChara.isGrv())
				{
					VecFx32 position = m_pChara.getPosition();
					if (position.y <= -819200)
					{
						position.y = 40960;
						m_pChara.getGrvAcc_set(0);
						m_pChara.getPrePosition_set(position);
					}
					m_pChara.setPosition(position);
				}
			}

			public void setLinearMove()
			{
				moveType_ = MOVE_TYPE.MOVE_TYPE_LINEAR;
			}

			public void setCorrectLinearMove()
			{
				moveType_ = MOVE_TYPE.MOVE_TYPE_CORRECTLINEAR;
			}

			public void setCircleMove(VecFx32 center, int xScale, int zScale, byte dir, int frame)
			{
				moveType_ = MOVE_TYPE.MOVE_TYPE_CIRCLE;
				center_.copy(center);
				xScale_ = xScale;
				zScale_ = zScale;
				VecFx32 chr_reuse_v = chr_reuse_v0;
				VEC_Subtract(m_pChara.getPosition(), center_, chr_reuse_v);
				rad_ = VEC_Mag(chr_reuse_v);
				VEC_Normalize(chr_reuse_v, chr_reuse_v);
				angle_ = FX_Atan2Idx(chr_reuse_v.z, chr_reuse_v.x);
				w_ = (ushort)(65536 / frame);
				if (dir == 1)
				{
					w_ *= ushort.MaxValue;
				}
			}

			public void setCircleMoveSyncRotate(VecFx32 center, int xScale, int zScale, byte dir, int frame, ushort yOrgRotate)
			{
				setCircleMove(center, xScale, zScale, dir, frame);
				syncCircle_ = true;
				yOrgRotate_ = yOrgRotate;
			}

			public void setNormalGravity()
			{
				gravityType_ = GRAVITY_TYPE.GRAVITY_TYPE_NORMAL;
			}

			public void setWaveGravity(int yBasic, int amp, int frame)
			{
				gravityType_ = GRAVITY_TYPE.GRAVITY_TYPE_WAVE;
				yBasic_ = yBasic;
				amp_ = amp;
				phase_ = 0;
				phaseSpd_ = (ushort)(65536 / frame);
			}

			public CCharacterMoveSys()
			{
				initialize();
			}

			public void setFlag(bool _Flag)
			{
				m_Flag = _Flag;
			}

			public bool isFlag()
			{
				return m_Flag;
			}

			public void setMoveFrame(int _MoveFrame)
			{
				m_MoveFrame = _MoveFrame;
			}

			public int MoveFrame()
			{
				return m_MoveFrame;
			}

			public void addTargetPoint(int _x, int _y, int _z)
			{
				VecFx32 chr_reuse_v = chr_reuse_v0;
				chr_reuse_v.set(_x, _y, _z);
				addTargetPoint(chr_reuse_v);
			}

			public void addTargetPoint(VecFx32 _TargetPoint)
			{
				if (m_PointNum < MOVE_TARGETPOINT_NUM)
				{
					if (m_PointNum == 0)
					{
						setTargetPoint(0, m_pChara.getPosition());
						m_PointNum++;
					}
					setTargetPoint(m_PointNum, _TargetPoint);
					m_PointNum++;
				}
			}

			public void setTargetPoint(byte _Index, int _x, int _y, int _z)
			{
				VecFx32 chr_reuse_v = chr_reuse_v0;
				chr_reuse_v.set(_x, _y, _z);
				setTargetPoint(_Index, chr_reuse_v);
			}

			public void setTargetPoint(byte _Index, VecFx32 _TargetPoint)
			{
				m_TargetPoint[_Index].copy(_TargetPoint);
			}

			public VecFx32 getTargetPoint(byte _Index)
			{
				return m_TargetPoint[_Index];
			}

			public void setAutoMoveType(byte type)
			{
				type_ = type;
			}

			public void setCharacter(CBaseCharacter _pChara)
			{
				m_pChara = _pChara;
			}

			public CBaseCharacter getCharacter()
			{
				return m_pChara;
			}

			public bool isStop()
			{
				return stop_;
			}

			public void setStop(bool b)
			{
				stop_ = b;
			}
		}
	}
}
