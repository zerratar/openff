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
	public static partial class pl
	{
		public class CNPCAiRandomMove : CBaseNPCAi
		{
			public override void initialize(CBasePlayer _pPlayer)
			{
				setPtrPlayer(_pPlayer);
				m_Over = false;
				m_Wait = 0;
			}

			public override void execute()
			{
				m_WorkFrame++;
				if (m_WorkFrame >= m_Wait)
				{
					m_WorkFrame = 0;
					m_SendAct = ((m_SendAct == 0) ? 1 : 0);
					culcWaitFrame();
				}
				int num = VEC_Distance(PtrPlayer().MainPos(), PtrPlayer().getPosition()) / 4096;
				int num2 = CNPCWorldParameterManager.Instance().NPCWorldRandomMoveParameter((int)PtrPlayer().NPCRandomMoveType()).Distance(0);
				int num3 = CNPCWorldParameterManager.Instance().NPCWorldRandomMoveParameter((int)PtrPlayer().NPCRandomMoveType()).Distance(1);
				if (num >= num2 || num >= num3)
				{
					m_Over = true;
				}
				else
				{
					m_Over = false;
				}
			}

			public override void terminate()
			{
			}

			public void reset()
			{
				culcWaitFrame();
			}

			protected override void culcWaitFrame()
			{
				NPC_RANDOM_MOVE_TYPE id = PtrPlayer().NPCRandomMoveType();
				CNPCWorldRandomMoveParameter cNPCWorldRandomMoveParameter = CNPCWorldParameterManager.Instance().NPCWorldRandomMoveParameter((int)id);
				if (cNPCWorldRandomMoveParameter == null)
				{
					return;
				}
				m_Wait = cNPCWorldRandomMoveParameter.StateFrame();
				if (ds.RandomNumber.rand32(2u) != 0)
				{
					int num = (int)ds.RandomNumber.rand32((uint)CNPCWorldParameterManager.Instance().NPCWorldRandomMoveParameter((int)PtrPlayer().NPCRandomMoveType()).RandStateFrame());
					if (m_SendAct == 0)
					{
						m_Wait = (m_Wait + num) / 2;
					}
					else
					{
						m_Wait = (m_Wait - num) / 2;
					}
				}
				if (m_Wait < 0)
				{
					m_Wait = 0;
				}
			}

			~CNPCAiRandomMove()
			{
			}
		}
	}
}
