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
	public static partial class chr
	{
		public class CCharacterTurnSys
		{
			public enum TURN_TYPE
			{
				TURN_TYPE_ERR = -1,
				TURN_TYPE_INSTANT,
				TURN_TYPE_RIGHT,
				TURN_TYPE_LEFT,
				TURN_TYPE_NEAR,
				TURN_TYPE_MAX
			}

			public const TURN_TYPE TURN_TYPE_ERR = TURN_TYPE.TURN_TYPE_ERR;

			public const TURN_TYPE TURN_TYPE_INSTANT = TURN_TYPE.TURN_TYPE_INSTANT;

			public const TURN_TYPE TURN_TYPE_RIGHT = TURN_TYPE.TURN_TYPE_RIGHT;

			public const TURN_TYPE TURN_TYPE_LEFT = TURN_TYPE.TURN_TYPE_LEFT;

			public const TURN_TYPE TURN_TYPE_NEAR = TURN_TYPE.TURN_TYPE_NEAR;

			public const TURN_TYPE TURN_TYPE_MAX = TURN_TYPE.TURN_TYPE_MAX;

			private bool m_EndWait;

			private bool m_Loop;

			private bool m_Fixed;

			private bool m_stop;

			private int m_Frame;

			private TURN_TYPE m_TurnType;

			private VecFx32 m_SucSpd = new VecFx32();

			private ushort orgRadY_;

			private ushort frame_;

			private ushort workFrame_;

			private int w_;

			private CBaseCharacter m_pChara;

			public void initialize()
			{
				m_EndWait = false;
				m_Loop = false;
				m_Fixed = false;
				m_stop = false;
				m_Frame = 0;
				m_TurnType = TURN_TYPE.TURN_TYPE_ERR;
				VEC_Set(m_SucSpd, 0, 0, 0);
				m_pChara = null;
			}

			public void execute()
			{
				if (m_pChara != null)
				{
					m_pChara.isAutoPilot();
				}
			}

			public void terminate()
			{
				reset();
			}

			public void update()
			{
				if (m_pChara != null && !m_stop)
				{
					if (m_Loop)
					{
						lastingTurn();
					}
					else
					{
						updateTurn();
					}
				}
			}

			public void reset()
			{
				m_Loop = false;
				m_pChara = null;
			}

			public void lastingTurn()
			{
				VecFx32 vecFx = new VecFx32(m_pChara.getRotation());
				vecFx.y += w_;
				workFrame_++;
				if (workFrame_ >= frame_)
				{
					vecFx.y = orgRadY_;
					workFrame_ = 0;
				}
				m_pChara.setRotation(vecFx);
			}

			public void updateTurn()
			{
				if (m_Fixed)
				{
					m_pChara.setDirection(m_pChara.getTargetDirection());
					return;
				}
				VecFx32 targetDirection = m_pChara.getTargetDirection();
				VecFx32 direction = m_pChara.getDirection();
				if ((direction.x == 0 && direction.y == 0 && direction.z == 0) || (targetDirection.x == 0 && targetDirection.y == 0 && targetDirection.z == 0))
				{
					return;
				}
				VecFx32 rotation = m_pChara.getRotation();
				int y = rotation.y;
				int num = FX_Atan2Idx(targetDirection.x, targetDirection.z);
				int num2 = fitRadian(num - y);
				int turnAcc = m_pChara.getTurnAcc();
				turnAcc = ((turnAcc != 0) ? (turnAcc + ((turnAcc >= 0) ? 1 : (-1))) : 1024);
				if (fitRadian((num2 > -turnAcc && num2 < turnAcc) ? 1 : 0) == 0)
				{
					num2 = ((y < PI / 2 && num >= PI + PI / 2) ? (y - turnAcc) : ((y > PI + PI / 2 && num <= PI / 2) ? (y + turnAcc) : ((y > PI && num <= 0) ? (y + turnAcc) : ((num2 >= 0) ? (y + turnAcc) : (y - turnAcc)))));
				}
				else
				{
					num2 = num;
					if (m_EndWait)
					{
						m_pChara.startMotion(1001, _Loop: true, 5u);
						m_EndWait = false;
					}
				}
				rotation.y = num2;
				m_pChara.setRotation(rotation);
				direction.copy(targetDirection);
				m_pChara.setDirection(direction);
			}

			public void startLastingTurn(ushort frame, int w)
			{
				orgRadY_ = (ushort)m_pChara.getRotation().y;
				frame_ = frame;
				workFrame_ = 0;
				w_ = w;
				m_Loop = true;
			}

			public void endLastingTurn()
			{
				m_Loop = false;
			}

			public CCharacterTurnSys()
			{
				initialize();
			}

			public void setEndWait(bool _EndWait)
			{
				m_EndWait = _EndWait;
			}

			public void setLoop(bool _Loop)
			{
				m_Loop = _Loop;
			}

			public void setFixed(bool _Fixed)
			{
				m_Fixed = _Fixed;
			}

			public void setFrame(int _Frame)
			{
				m_Frame = _Frame;
			}

			public void setTurnType(TURN_TYPE m_TurnType)
			{
				this.m_TurnType = m_TurnType;
			}

			public void setSucSpd(int m_SucSpd_X, int m_SucSpd_Y, int m_SucSpd_Z)
			{
				VecFx32 sucSpd = new VecFx32(m_SucSpd_X, m_SucSpd_Y, m_SucSpd_Z);
				setSucSpd(sucSpd);
			}

			public void setSucSpd(VecFx32 m_SucSpd)
			{
				this.m_SucSpd.copy(m_SucSpd);
			}

			public void setCharacter(CBaseCharacter _pChara)
			{
				m_pChara = _pChara;
			}

			public CBaseCharacter getCharacter()
			{
				return m_pChara;
			}

			public void setStop(bool b)
			{
				m_stop = b;
			}
		}
	}
}
