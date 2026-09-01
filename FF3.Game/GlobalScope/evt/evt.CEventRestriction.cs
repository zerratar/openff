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
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
	public static partial class evt
	{
		public class CEventRestriction
		{
			protected const int ER_LIMIT_OF_LIST = 8;

			public static CEventRestriction instance_ = new CEventRestriction();

			protected ds.Vector<int, ds.FastErasePolicy<int>> restrictionList_ = new ds.Vector<int, ds.FastErasePolicy<int>>(8);

			protected ds.Vector<int, ds.FastErasePolicy<int>> permissionList_ = new ds.Vector<int, ds.FastErasePolicy<int>>(8);

			public void beginning()
			{
				restrictionList_.clear();
				permissionList_.clear();
			}

			public void ending()
			{
			}

			public void clear()
			{
				restrictionList_.clear();
			}

			public void entry(int _id)
			{
				if (restrictionList_.size() < 8)
				{
					restrictionList_.push_back(_id);
				}
			}

			public bool check(int _id)
			{
				for (int num = restrictionList_.size() - 1; num >= 0; num--)
				{
					if (restrictionList_[num] == _id)
					{
						return true;
					}
				}
				return false;
			}

			public void clearPermission()
			{
				permissionList_.clear();
			}

			public void entryPermission(int _id)
			{
				if (permissionList_.size() < 8)
				{
					permissionList_.push_back(_id);
				}
			}

			public bool checkPermission(int _id)
			{
				for (int num = permissionList_.size() - 1; num >= 0; num--)
				{
					if (permissionList_[num] == _id)
					{
						return true;
					}
				}
				return false;
			}

			public static CEventRestriction getSingleton()
			{
				return instance_;
			}
		}
	}
}
