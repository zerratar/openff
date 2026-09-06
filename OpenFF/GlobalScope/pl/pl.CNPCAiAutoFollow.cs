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
	public static partial class pl
	{
		public class CNPCAiAutoFollow : CBaseNPCAi
		{
			public override void initialize(CBasePlayer _pPlayer)
			{
				setPtrPlayer(_pPlayer);
				m_Wait = 0;
			}

			public override void execute()
			{
				CPlayerCharacter cPlayerCharacter = static_cast<CPlayerCharacter>(LookPlayer());
				int nowAct = LookPlayer().getNowAct();
				int nowAct2 = PtrPlayer().getNowAct();
				int num = ((cPlayerCharacter.getInPutMode() == INPUT_MODE.INPUT_MODE_FIELD) ? 1 : 2);
				int sendAct = 0;
				VecFx32 position = LookPlayer().getPosition();
				VecFx32 position2 = PtrPlayer().getPosition();
				VecFx32 vecFx = new VecFx32();
				VEC_Subtract(position, position2, vecFx);
				int num2 = VEC_Mag(vecFx);
				VEC_Normalize(vecFx, vecFx);
				int num3 = CNPCWorldParameterManager.Instance().NPCWorldAutoFollowParameter((int)PtrPlayer().NPCAutoFollowType()).DistanceWalk();
				int num4 = num3 - 3;
				int num5 = CNPCWorldParameterManager.Instance().NPCWorldAutoFollowParameter((int)PtrPlayer().NPCAutoFollowType()).DistanceRun();
				int num6 = num5 + 8;
				int num7 = 0;
				int num8 = num2 >> 12;
				switch (nowAct2)
				{
				case 0:
				case 1:
					if (num8 < num4)
					{
						sendAct = 0;
						break;
					}
					if (num8 < num5)
					{
						sendAct = 1;
						break;
					}
					switch (nowAct)
					{
					case 2:
						sendAct = num;
						break;
					case 1:
						num7 = num5 << 12;
						sendAct = 1;
						break;
					}
					break;
				case 2:
					if (num8 < num3)
					{
						sendAct = 0;
						break;
					}
					if (num8 < num5)
					{
						sendAct = 1;
						break;
					}
					if (num8 < num6)
					{
						sendAct = num;
						break;
					}
					num7 = num6 << 12;
					sendAct = num;
					break;
				}
				if (num7 != 0)
				{
					VEC_MultAdd(num2 - num7, vecFx, position2, PtrPlayer().getPosition());
				}
				m_SendAct = sendAct;
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
				m_Wait = CNPCWorldParameterManager.Instance().NPCWorldAutoFollowParameter((int)PtrPlayer().NPCAutoFollowType()).StateFrame();
				if (ds.RandomNumber.rand32(2u) != 0)
				{
					int num = (int)ds.RandomNumber.rand32((uint)CNPCWorldParameterManager.Instance().NPCWorldAutoFollowParameter((int)PtrPlayer().NPCAutoFollowType()).RandStateFrame());
					m_Wait += ((ds.RandomNumber.rand32(2u) == 0) ? num : (-num));
				}
				if (m_Wait < 0)
				{
					m_Wait = 0;
				}
			}

			~CNPCAiAutoFollow()
			{
			}
		}
	}
}
