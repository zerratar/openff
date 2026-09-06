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
	public static partial class eld
	{
		public class SIDTable
		{
			public class SCategoryInfo
			{
				public uint addr;

				public uint nbMem;
			}

			public uint[] pTable;

			public uint nbCtgrs;

			public SCategoryInfo[] infoCtgrs;

			public SIDTable()
			{
				pTable = null;
				nbCtgrs = 0u;
				infoCtgrs = null;
			}

			public void initialize(uint[] pID)
			{
				pTable = pID;
				nbCtgrs = pID[0];
				infoCtgrs = new SCategoryInfo[nbCtgrs];
				int num = 4;
				for (int i = 0; i < nbCtgrs; i++)
				{
					infoCtgrs[i] = new SCategoryInfo();
					infoCtgrs[i].addr = pID[num++];
					infoCtgrs[i].nbMem = pID[num++];
				}
			}

			public bool isEmpty()
			{
				if (pTable == null)
				{
					return true;
				}
				return false;
			}

			public uint[] getIDTable()
			{
				return pTable;
			}

			public uint getNbCategories()
			{
				return nbCtgrs;
			}

			public uint getTemplateID(uint ctgr, uint mem)
			{
				if (nbCtgrs > ctgr)
				{
					return pTable[(infoCtgrs[ctgr].addr >> 2) + mem];
				}
				return 0u;
			}
		}
	}
}
