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
	public class SuspendSaveDataGlobal
	{
		public static SuspendSaveDataGlobal instance_ = new SuspendSaveDataGlobal();

		private card.SaveDataAddress SuspendSaveData_;

		private bool bSetup_;

		public SuspendSaveDataGlobal()
		{
			bSetup_ = false;
			SuspendSaveData_ = null;
		}

		~SuspendSaveDataGlobal()
		{
			bSetup_ = false;
		}

		public void reflect()
		{
			if (bSetup_)
			{
				SuspendSaveData_.sdReflect();
			}
		}

		public static SuspendSaveDataGlobal getSingleton()
		{
			return instance_;
		}

		public card.SaveDataAddress getSuspendSaveData()
		{
			return SuspendSaveData_;
		}

		public bool setup()
		{
			if (SuspendSaveData_ != null)
			{
				release();
			}
			SuspendSaveData_ = new card.SaveDataAddress();
			if (SuspendSaveData_ == null)
			{
				return false;
			}
			SuspendSaveData_.sdaLoad(41568u);
			while (!SuspendSaveData_.sdExecute())
			{
			}
			if (SuspendSaveData_.sdCheck() && SuspendSaveData_.sdaValidity() == card.SSD_SIGN)
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
			if (SuspendSaveData_ != null)
			{
				ds.CHeap.free_app(SuspendSaveData_);
				SuspendSaveData_ = null;
			}
		}

		public void invalidate()
		{
			if (SuspendSaveData_ != null)
			{
				SuspendSaveData_.sdaInvalidate();
				SuspendSaveData_.sdaSave(41568u);
				while (!SuspendSaveData_.sdExecute())
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
