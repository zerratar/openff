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
	public static partial class mognet
	{
		public class MNSBridge : MogNetState
		{
			protected int localState;

			public override void mnsInitialize(MNSMediator M)
			{
				localState = 0;
				dgs.CFade.Main().fadeOut(15, M.fadeColor_);
				dgs.CFade.Sub().fadeOut(15, M.fadeColor_);
				ds.g_Pad.disable();
				ds.g_TouchPanel.disable();
				M.mnsmCommonInterface(b: false, bLR: false);
			}

			public override bool mnsProcess(MNSMediator M)
			{
				switch (localState)
				{
				case 0:
					if (dgs.CFade.Main().isFaded() && dgs.CFade.Sub().isFaded())
					{
						if (M.prevState != null)
						{
							M.prevState.mnsTerminate(M);
						}
						localState = 1;
					}
					break;
				case 1:
					if (M.nextState != null)
					{
						M.nextState.mnsInitialize(M);
					}
					dgs.CFade.Main().fadeIn(15);
					dgs.CFade.Sub().fadeIn(15);
					localState = 2;
					break;
				case 2:
					if (dgs.CFade.Main().isCleared() && dgs.CFade.Sub().isCleared())
					{
						M.currentState = M.nextState;
						ds.g_Pad.enable();
						ds.g_TouchPanel.enable();
					}
					break;
				}
				return true;
			}

			public override void mnsTerminate(MNSMediator M)
			{
				ds.g_Pad.enable();
			}
		}
	}
}
