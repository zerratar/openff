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
		public class Battle2DManager
		{
			public static Battle2DManager instance_ = new Battle2DManager();

			private Cursor cursor_ = new Cursor();

			private Hit critical_ = new Hit();

			private HelpWindow helpWindow_ = new HelpWindow();

			private Triangle triangle_ = new Triangle();

			private Hit[] hit_ = new Hit[12];

			private Damage damage_ = new Damage();

			public void setup()
			{
				cursor().setup();
				critical().setup();
				helpWindow().setup();
				triangle().setup();
				for (int i = 0; i < 12; i++)
				{
					hit(i).setup();
				}
				damage().setup();
			}

			public void cleanup()
			{
				cursor().cleanup();
				critical().cleanup();
				helpWindow().cleanup();
				triangle().cleanup();
				for (int i = 0; i < 12; i++)
				{
					hit(i).cleanup();
				}
				damage().cleanup();
			}

			public void execute()
			{
				helpWindow().execute();
			}

			public Battle2DManager()
			{
				for (int i = 0; i < hit_.Length; i++)
				{
					hit_[i] = new Hit();
				}
			}

			public static Battle2DManager instance()
			{
				return instance_;
			}

			public Cursor cursor()
			{
				return cursor_;
			}

			public Hit critical()
			{
				return critical_;
			}

			public HelpWindow helpWindow()
			{
				return helpWindow_;
			}

			public Triangle triangle()
			{
				return triangle_;
			}

			public Hit hit(int i)
			{
				return hit_[i];
			}

			public Damage damage()
			{
				return damage_;
			}
		}
	}
}
