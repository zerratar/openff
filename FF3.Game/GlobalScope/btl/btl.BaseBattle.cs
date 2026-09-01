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
		public class BaseBattle
		{
			public enum Phase
			{
				Initialize,
				Execute,
				Terminate,
				PhaseMax
			}

			public const Phase Initialize = Phase.Initialize;

			public const Phase Execute = Phase.Execute;

			public const Phase Terminate = Phase.Terminate;

			public const Phase PhaseMax = Phase.PhaseMax;

			protected Phase phase_;

			public BaseBattle()
			{
				phase_ = Phase.Initialize;
			}

			public virtual void initialize(BattleSystem B)
			{
			}

			public virtual void terminate(BattleSystem B)
			{
			}

			public virtual void execute(BattleSystem B)
			{
			}

			public void setPhase(Phase phase)
			{
				phase_ = phase;
			}

			public Phase phase()
			{
				return phase_;
			}
		}
	}
}
