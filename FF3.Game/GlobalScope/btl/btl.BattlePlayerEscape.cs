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
using java.io;
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
	public static partial class btl
	{
		public class BattlePlayerEscape : BaseBattle
		{
			public override void initialize(BattleSystem B)
			{
				BattleSE.instance().free();
				BattleBGM.instance().stop(CHANGE_BGM_COUNT_MAX);
				B.characterManager().playerParty().clearPoolSkillExp();
				pl.PlayerParty.instance().mania().escapeNumber()
					.add(1);
			}

			public override void terminate(BattleSystem B)
			{
			}

			public override void execute(BattleSystem B)
			{
				if (MatrixSound.MtxSoundBGM.getSingleton().getState(MatrixSound.enMtxBGMSlot.enMTX_BGM_SLOT0) == MatrixSound.enMtxBGMState.enMTX_BGM_STOP)
				{
					B.onEnd();
					setPhase(Phase.Terminate);
				}
			}
		}
	}
}
