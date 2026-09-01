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
	public class NNSG2dCellAnimation
	{
		public NNSG2dAnimController animCtrl = new NNSG2dAnimController();

		public NNSG2dCellData pCurrentCell;

		public NNSG2dCellDataBank pCellDataBank;

		public uint cellTransferStateHandle;

		public NNSG2dSRTControl srtCtrl;

		public void copy(NNSG2dCellAnimation src)
		{
			animCtrl.copy(src.animCtrl);
			pCurrentCell = src.pCurrentCell;
			pCellDataBank = src.pCellDataBank;
			cellTransferStateHandle = src.cellTransferStateHandle;
			srtCtrl = src.srtCtrl;
		}

		public void setDefault()
		{
			animCtrl.setDefault();
			pCurrentCell = null;
			pCellDataBank = null;
			cellTransferStateHandle = 0u;
			srtCtrl = null;
		}
	}
}
