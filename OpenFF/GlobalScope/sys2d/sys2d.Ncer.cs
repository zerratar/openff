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
	public static partial class sys2d
	{
		public class Ncer : NCData
		{
			public override bool Load(string pFile_name)
			{
				base.Load(pFile_name);
				if (m_pCell == null)
				{
					m_pCell = new NNSG2dCellDataBank();
				}
				NNS_G2dGetUnpackedCellBank(pData(), m_pCell);
				return true;
			}

			public NNSG2dCellDataBank pDataCe()
			{
				return m_pCell;
			}

			public void copy(Ncer src)
			{
				copy((NCData)src);
			}
		}
	}
}
