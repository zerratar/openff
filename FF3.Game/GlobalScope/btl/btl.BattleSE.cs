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
	public static partial class btl
	{
		public class BattleSE
		{
			public static BattleSE instance_ = new BattleSE();

			private int asyncLevel_;

			private MatrixSound.MtxSEHandle handle_;

			public void initialize()
			{
				asyncLevel_ = 0;
			}

			public void load(int se_group)
			{
				asyncLevel_ = MatrixSound.MtxSoundNDS_getHeapLV();
				MatrixSound.MtxSENDS_Load(se_group);
			}

			public void loadAsync(int se_group)
			{
				asyncLevel_ = MatrixSound.MtxSoundNDS_getHeapLV();
				MatrixSound.MtxSENDS_LoadAsync(se_group);
			}

			public void loadNew(int se_group)
			{
				MatrixSound.MtxSENDS_Load(se_group);
			}

			public bool isLoadAsync()
			{
				return MatrixSound.MtxSENDS_IsLoadAsync();
			}

			public void free()
			{
				if (asyncLevel_ > 0)
				{
					MatrixSound.MtxSoundNDS_Unload(asyncLevel_);
				}
				asyncLevel_ = 0;
			}

			public void freeNew()
			{
				MatrixSound.MtxSENDS_Unload();
			}

			public void play(int category, int member)
			{
				handle_ = MatrixSound.MtxSENDS_Play(category, member, 192, 127);
			}

			public void stop()
			{
				MatrixSound.MtxSENDS_Stop(handle_, 0);
			}

			public void loadBattleCommonSE()
			{
				load(200);
			}

			public void freeBattleCommonSE()
			{
				MatrixSound.MtxSENDS_Unload();
			}

			public void playMissSE()
			{
				play(200, 0);
			}

			public BattleSE()
			{
				asyncLevel_ = 0;
			}

			public static BattleSE instance()
			{
				return instance_;
			}
		}
	}
}
