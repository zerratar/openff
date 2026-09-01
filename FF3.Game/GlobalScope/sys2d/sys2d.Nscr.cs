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
	public static partial class sys2d
	{
		public class Nscr : NCData
		{
			public override bool Load(string pFile_name)
			{
				base.Load(pFile_name);
				if (NNS_G2dGetUnpackedScreenData(pData(), m_pScreen) == 0)
				{
					if (m_pScreenCell == null)
					{
						m_pScreenCell = new NNSG2dCellDataBank();
					}
					if (NNS_G2dGetUnpackedCellBank(pData(), m_pScreenCell) != 0)
					{
						return true;
					}
				}
				return true;
			}

			public NNSG2dScreenData pDataSc()
			{
				return m_pScreen;
			}

			public NNSG2dCellDataBank pDataCe()
			{
				return m_pScreenCell;
			}
		}
	}
}
