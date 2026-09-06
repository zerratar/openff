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
	public static partial class btl
	{
		public class GeographyManager
		{
			private GeographyData[] geographyData_;

			private int geographyMax_;

			public void load()
			{
				free();
				Array array = null;
				strcpy(out var arg, "geography.bbd");
				uint num = static_cast<uint>(ds.g_File.getSize(arg));
				array = ds.CHeap.alloc_app(num);
				ds.g_File.load(array, arg);
				geographyData_ = GeographyData.castArray(array);
				geographyMax_ = (int)(num / 44);
			}

			public void free()
			{
				if (geographyData_ != null)
				{
					ds.CHeap.free_app(geographyData_);
					geographyData_ = null;
				}
			}

			public GeographyData geographyData(int map_id)
			{
				for (int i = 0; i < geographyMax_; i++)
				{
					if (geographyData_[i] != null && geographyData_[i].battleMapId() == map_id)
					{
						return geographyData_[i];
					}
				}
				return null;
			}

			public GeographyManager()
			{
				geographyData_ = null;
				geographyMax_ = 0;
			}

			public int geographyMax()
			{
				return geographyMax_;
			}
		}
	}
}
