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
	public class BattlePerformer : ds.sys3d.SceneRenderObject
	{
		public static BattlePerformer instance_ = new BattlePerformer();

		private bool state;

		private Performer currentPerformer;

		private BPSlantVanish bpSlantVanish = new BPSlantVanish();

		private BPDivide bpDivide = new BPDivide();

		private BPIronChopper bpIronChopper = new BPIronChopper();

		private BPTranslucence bPTranslucence_ = new BPTranslucence();

		public BattlePerformer()
		{
			initialize();
		}

		public void initialize()
		{
			state = false;
			currentPerformer = null;
		}

		public void start()
		{
			state = true;
			if (currentPerformer != null)
			{
				currentPerformer.prepare();
			}
		}

		public bool isPerforming()
		{
			return state;
		}

		public void end()
		{
			state = false;
			if (currentPerformer != null)
			{
				currentPerformer.finish();
			}
			currentPerformer = null;
		}

		public void progress()
		{
			if (isPerforming() && currentPerformer != null)
			{
				state = currentPerformer.progress();
			}
		}

		public override void draw()
		{
			if (isPerforming() && currentPerformer != null)
			{
				currentPerformer.draw();
			}
		}

		public void selectPerformer(BATTLEPERFORM_TYPE bp_type)
		{
			state = false;
			if (currentPerformer != null)
			{
				currentPerformer.finish();
			}
			switch (bp_type)
			{
			case BATTLEPERFORM_TYPE.BPT_SLANT_VANISH:
				currentPerformer = bpSlantVanish;
				break;
			case BATTLEPERFORM_TYPE.BPT_DIVIDE:
				currentPerformer = bpDivide;
				break;
			case BATTLEPERFORM_TYPE.BPT_IRON_CHOPPER:
				currentPerformer = bpIronChopper;
				break;
			case BATTLEPERFORM_TYPE.BPT_TRANSLUCENCE:
				currentPerformer = bPTranslucence_;
				break;
			}
		}

		public void setTarget(int char_ctrl_id)
		{
			if (currentPerformer != null)
			{
				currentPerformer.target(char_ctrl_id);
			}
		}

		public static BattlePerformer getInstance()
		{
			return instance_;
		}
	}
}
