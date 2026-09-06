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
using OpenFF.Platform;
using OpenFF.Resources;

internal static partial class GlobalScope
{
	public class OptionSaveDataGlobal
	{
		public static OptionSaveDataGlobal instance_ = new OptionSaveDataGlobal();

		private card.SaveDataOption OptionSaveData_;

		private bool bSetup_;

		public OptionSaveDataGlobal()
		{
			bSetup_ = false;
			OptionSaveData_ = null;
		}

		~OptionSaveDataGlobal()
		{
			bSetup_ = false;
		}

		public void reflect()
		{
			if (bSetup_)
			{
				OptionSaveData_.sdReflect();
			}
		}

		public static OptionSaveDataGlobal getSingleton()
		{
			return instance_;
		}

		public card.SaveDataOption getOptionSaveData()
		{
			return OptionSaveData_;
		}

		public bool setup()
		{
			if (OptionSaveData_ != null)
			{
				release();
			}
			OptionSaveData_ = new card.SaveDataOption();
			if (OptionSaveData_ == null)
			{
				return false;
			}
			OptionSaveData_.sdaLoad(55424u);
			while (!OptionSaveData_.sdExecute())
			{
			}
			if (OptionSaveData_.sdCheck() && OptionSaveData_.sdaValidity() == card.SSD_SIGN)
			{
				bSetup_ = true;
			}
			else
			{
				bSetup_ = false;
			}
			return bSetup_;
		}

		public void release()
		{
			if (OptionSaveData_ != null)
			{
				ds.CHeap.free_app(OptionSaveData_);
				OptionSaveData_ = null;
			}
		}

		public void invalidate()
		{
			if (OptionSaveData_ != null)
			{
				OptionSaveData_.sdaInvalidate();
				OptionSaveData_.sdaSave(55424u);
				while (!OptionSaveData_.sdExecute())
				{
				}
				bSetup_ = false;
			}
		}

		public bool isProper()
		{
			return bSetup_;
		}
	}
}
