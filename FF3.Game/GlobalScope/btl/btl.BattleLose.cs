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
		public class BattleLose : BaseBattle
		{
			public const int END_BGM = 0;

			public const int END_LOSE = 1;

			public const int END = 2;

			public const int LOSE_HELP_WINDOW_FRAME_MAX = 60;

			private int frameCounter_;

			private int changeBGMCounter_;

			private int state_;

			public override void initialize(BattleSystem B)
			{
				BattleSE.instance().free();
				pl.PlayerParty.instance().fineAll();
				Battle2DManager.instance().helpWindow().setMsdHandle(0);
				Battle2DManager.instance().helpWindow().createHelpWindow(112, 1, 0);
				frameCounter_ = 0;
				MatrixSound.MtxSoundBGM.getSingleton().stop(CHANGE_BGM_COUNT_MAX, MatrixSound.enMtxBGMSlot.enMTX_BGM_SLOT0);
				state_ = 0;
				B.characterManager().playerParty().clearPoolSkillExp();
			}

			public override void terminate(BattleSystem B)
			{
			}

			public override void execute(BattleSystem B)
			{
				switch (state_)
				{
				case 0:
					endBGM(B);
					break;
				case 1:
					endLose(B);
					break;
				case 2:
					end(B);
					break;
				}
			}

			public void endBGM(BattleSystem B)
			{
				if (MatrixSound.MtxSoundBGM.getSingleton().getState(MatrixSound.enMtxBGMSlot.enMTX_BGM_SLOT0) == MatrixSound.enMtxBGMState.enMTX_BGM_STOP)
				{
					BattleBGM.instance().free();
					BattleBGM.instance().loadAndPlay(2, 0);
					state_ = 1;
				}
			}

			public void endLose(BattleSystem B)
			{
				frameCounter_++;
				frameCounter_ = ds.min(frameCounter_, 999999);
				if (frameCounter_ > 60 && B.isEdgeAButtonAndTouchEdge())
				{
					BattleBGM.instance().stop(CHANGE_BGM_COUNT_MAX);
					state_ = 2;
				}
			}

			public void end(BattleSystem B)
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
