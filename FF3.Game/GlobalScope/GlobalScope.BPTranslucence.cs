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
	public class BPTranslucence : Performer
	{
		public static int transSpeed_ = 1;

		public static int transFrameMax_ = 30;

		protected int frame_;

		public static void setting(int frame)
		{
			transSpeed_ = 100 / frame;
			transFrameMax_ = frame;
		}

		public void target(int ctrl_id)
		{
			idList.push_back(ctrl_id);
		}

		public void prepare()
		{
			frame_ = 0;
		}

		public bool progress()
		{
			frame_++;
			for (int i = 0; i < idList.size(); i++)
			{
				int transparencyRate = characterMng.getTransparencyRate(idList.at(i));
				characterMng.setTransparencyRate(idList.at(i), transparencyRate - transSpeed_);
				transparencyRate = characterMng.getShadowAlphaRate(idList.at(i));
				transparencyRate--;
				transparencyRate = ds.max(transparencyRate, 0);
				characterMng.setShadowAlphaRate(idList.at(i), transparencyRate);
			}
			return frame_ < transFrameMax_;
		}

		public void draw()
		{
		}

		public void finish()
		{
			idList.clear();
		}
	}
}
