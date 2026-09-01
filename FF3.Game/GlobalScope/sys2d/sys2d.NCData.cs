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
		public class NCData
		{
			protected NNSG2dCellDataBank m_pCell;

			protected NNSG2dAnimBankData m_pAnimation;

			protected NNSG2dScreenData m_pScreen;

			protected NNSG2dCharacterData m_pCharacter;

			protected NNSG2dPaletteData m_pPalette;

			protected Array m_pEachData;

			protected NNSG2dCellDataBank m_pScreenCell;

			private Array m_pData;

			private uint m_Size;

			public NCData()
			{
				Initialize();
			}

			~NCData()
			{
				Release();
			}

			public void Initialize()
			{
				m_pData = null;
				m_Size = 0u;
				m_pEachData = null;
				m_pScreenCell = null;
			}

			public virtual bool Load(string pFile_name)
			{
				m_Size = ds.g_File.getSize(pFile_name);
				m_pData = ds.CHeap.alloc_app(m_Size);
				NCDataManager.GetNCDataManager().AddData(m_pData);
				return ds.g_File.load(m_pData, pFile_name);
			}

			public virtual bool Load(byte[] pFile_name)
			{
				m_Size = (uint)pFile_name.Length;
				m_pData = pFile_name;
				NCDataManager.GetNCDataManager().AddData(m_pData);
				return m_pData != null;
			}

			public virtual bool Release()
			{
				bool result = false;
				if (m_pData != null && NCDataManager.GetNCDataManager().DeleteData(m_pData) && NCDataManager.GetNCDataManager().GetDataNum(m_pData) == 0)
				{
					ds.CHeap.free_app(m_pData);
					result = true;
				}
				Initialize();
				return result;
			}

			public Array pData()
			{
				return m_pData;
			}

			public uint Size()
			{
				return m_Size;
			}

			public void copy(NCData src)
			{
				m_pCell = src.m_pCell;
				m_pAnimation = src.m_pAnimation;
				m_pScreen = src.m_pScreen;
				m_pCharacter = src.m_pCharacter;
				m_pPalette = src.m_pPalette;
				m_pEachData = src.m_pEachData;
				m_pScreenCell = src.m_pScreenCell;
				m_pData = src.m_pData;
				m_Size = src.m_Size;
			}
		}
	}
}
