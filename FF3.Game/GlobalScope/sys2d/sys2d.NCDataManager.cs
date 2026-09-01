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
		public class NCDataManager
		{
			public class _m_LoadData
			{
				public Array pData;

				public uint nData;

				public void setDefault()
				{
					pData = null;
					nData = 0u;
				}
			}

			private const uint LOAD_DATA_MAX = 128u;

			public static NCDataManager g_NCDataManagerInstance = new NCDataManager();

			private _m_LoadData[] m_LoadData = new _m_LoadData[128];

			private uint m_nLoadData;

			public NCDataManager()
			{
				for (int i = 0; i < m_LoadData.Length; i++)
				{
					m_LoadData[i] = new _m_LoadData();
				}
				setDefault();
			}

			~NCDataManager()
			{
			}

			public bool AddData(Array p)
			{
				uint num;
				for (num = 0u; num < m_nLoadData; num++)
				{
					if (m_LoadData[num].pData == p)
					{
						m_LoadData[num].nData++;
						break;
					}
				}
				if (num == m_nLoadData && num < 128)
				{
					m_LoadData[num].pData = const_cast<Array>(p);
					m_LoadData[num].nData = 1u;
					m_nLoadData++;
				}
				return true;
			}

			public bool DeleteData(Array p)
			{
				bool flag = false;
				for (uint num = 0u; num < m_nLoadData; num++)
				{
					if (m_LoadData[num].pData != p)
					{
						continue;
					}
					m_LoadData[num].nData--;
					if (m_LoadData[num].nData == 0)
					{
						uint num2;
						for (num2 = num; num2 < m_nLoadData - 1; num2++)
						{
							m_LoadData[num2] = m_LoadData[num2 + 1];
						}
						m_LoadData[num2].pData = null;
						m_LoadData[num2].nData = 0u;
						m_nLoadData--;
					}
					flag = true;
					break;
				}
				if (!flag)
				{
					return false;
				}
				return true;
			}

			public uint GetDataNum(Array p)
			{
				uint num = 0u;
				for (uint num2 = 0u; num2 < m_nLoadData; num2++)
				{
					if (m_LoadData[num2].pData == p)
					{
						num++;
					}
				}
				return num;
			}

			public void dumpDebugInfo()
			{
			}

			public static NCDataManager GetNCDataManager()
			{
				return g_NCDataManagerInstance;
			}

			public void setDefault()
			{
				for (int i = 0; i < m_LoadData.Length; i++)
				{
					m_LoadData[i].setDefault();
				}
				m_nLoadData = 0u;
			}
		}
	}
}
