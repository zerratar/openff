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
	public static partial class evt
	{
		public class CHichManParameter
		{
			public enum KIND
			{
				KIND_ERR = -1,
				KIND_CAST,
				KIND_MAP_LOGIC,
				KIND_EXTRA_LOGIC,
				KIND_MAX
			}

			public class HICH_MAN_INDIVIDUAL
			{
				public uint m_CharaId;

				public string m_CharaName;

				public int m_Id;

				public KIND m_Kind;

				public int m_KindParam;

				public int[] m_Position = new int[4];

				public int[] m_Posture = new int[4];

				public int[] m_Scale = new int[4];
			}

			public class HICH_MAN_COMPOSITE
			{
				public uint m_NumMan;

				public HICH_MAN_INDIVIDUAL[] m_HichInd = new HICH_MAN_INDIVIDUAL[48];

				public HICH_MAN_COMPOSITE()
				{
					for (int i = 0; i < m_HichInd.Length; i++)
					{
						m_HichInd[i] = new HICH_MAN_INDIVIDUAL();
					}
				}

				public static explicit operator HICH_MAN_COMPOSITE(Array src)
				{
					HICH_MAN_COMPOSITE hICH_MAN_COMPOSITE = new HICH_MAN_COMPOSITE();
					ArrayReader arrayReader = new ArrayReader(src);
					byte[] array = new byte[8];
					hICH_MAN_COMPOSITE.m_NumMan = arrayReader.readUInt32();
					hICH_MAN_COMPOSITE.m_HichInd = new HICH_MAN_INDIVIDUAL[hICH_MAN_COMPOSITE.m_NumMan];
					for (int i = 0; i < hICH_MAN_COMPOSITE.m_NumMan; i++)
					{
						hICH_MAN_COMPOSITE.m_HichInd[i] = new HICH_MAN_INDIVIDUAL();
						hICH_MAN_COMPOSITE.m_HichInd[i].m_CharaId = arrayReader.readUInt32();
						arrayReader.read(array, 0, 8);
						hICH_MAN_COMPOSITE.m_HichInd[i].m_CharaName = StringUtil.createString(array);
						hICH_MAN_COMPOSITE.m_HichInd[i].m_Id = arrayReader.readInt32();
						hICH_MAN_COMPOSITE.m_HichInd[i].m_Kind = (KIND)arrayReader.readInt32();
						hICH_MAN_COMPOSITE.m_HichInd[i].m_KindParam = arrayReader.readInt32();
						arrayReader.read(hICH_MAN_COMPOSITE.m_HichInd[i].m_Position, 0, 4);
						arrayReader.read(hICH_MAN_COMPOSITE.m_HichInd[i].m_Posture, 0, 4);
						arrayReader.read(hICH_MAN_COMPOSITE.m_HichInd[i].m_Scale, 0, 4);
					}
					arrayReader.dispose();
					return hICH_MAN_COMPOSITE;
				}
			}

			public const int HICH_MAN_PARAMETER_MAX = 48;

			public const KIND KIND_ERR = KIND.KIND_ERR;

			public const KIND KIND_CAST = KIND.KIND_CAST;

			public const KIND KIND_MAP_LOGIC = KIND.KIND_MAP_LOGIC;

			public const KIND KIND_EXTRA_LOGIC = KIND.KIND_EXTRA_LOGIC;

			public const KIND KIND_MAX = KIND.KIND_MAX;

			protected HICH_MAN_COMPOSITE m_HichCom = new HICH_MAN_COMPOSITE();

			protected int[] m_CharaIndex = new int[48];

			public void setUp(Array _Addr)
			{
				if (_Addr != null)
				{
					HICH_MAN_COMPOSITE hICH_MAN_COMPOSITE = (HICH_MAN_COMPOSITE)_Addr;
					m_HichCom.m_NumMan = hICH_MAN_COMPOSITE.m_NumMan;
					for (int i = 0; i < m_HichCom.m_NumMan; i++)
					{
						m_HichCom.m_HichInd[i] = hICH_MAN_COMPOSITE.m_HichInd[i];
					}
				}
			}

			public HICH_MAN_COMPOSITE getParam()
			{
				return m_HichCom;
			}

			public HICH_MAN_INDIVIDUAL getSubParam(int _Index)
			{
				return m_HichCom.m_HichInd[_Index];
			}

			public int getManCastIndex(uint _Id)
			{
				for (int i = 0; i < 48; i++)
				{
					if (m_HichCom.m_HichInd[i].m_Kind != KIND.KIND_ERR && m_HichCom.m_HichInd[i].m_Id == _Id)
					{
						return i;
					}
				}
				return -1;
			}

			public int getCharaCastIndex(int _Id)
			{
				for (int i = 0; i < 48; i++)
				{
					if (m_CharaIndex[i] == _Id)
					{
						return i;
					}
				}
				return -1;
			}

			public void setCharaIndex(int _HichId, int _CharaIndex)
			{
				m_CharaIndex[_HichId] = _CharaIndex;
			}

			public int CharaIndex(int _HichId)
			{
				return m_CharaIndex[_HichId];
			}

			protected void initialize()
			{
				m_HichCom.m_NumMan = 0u;
				for (int i = 0; i < 48; i++)
				{
					initialize((uint)i);
				}
			}

			protected void initialize(uint _Index)
			{
				m_HichCom.m_HichInd[_Index].m_CharaId = 0u;
				m_HichCom.m_HichInd[_Index].m_Id = 0;
				m_HichCom.m_HichInd[_Index].m_CharaName = "";
				m_HichCom.m_HichInd[_Index].m_Kind = KIND.KIND_ERR;
				m_HichCom.m_HichInd[_Index].m_KindParam = 0;
				m_HichCom.m_HichInd[_Index].m_Position[0] = (m_HichCom.m_HichInd[_Index].m_Position[1] = 0);
				m_HichCom.m_HichInd[_Index].m_Position[2] = (m_HichCom.m_HichInd[_Index].m_Position[3] = 0);
				m_HichCom.m_HichInd[_Index].m_Posture[0] = (m_HichCom.m_HichInd[_Index].m_Posture[1] = 0);
				m_HichCom.m_HichInd[_Index].m_Posture[2] = (m_HichCom.m_HichInd[_Index].m_Posture[3] = 0);
				m_HichCom.m_HichInd[_Index].m_Scale[0] = (m_HichCom.m_HichInd[_Index].m_Scale[1] = 0);
				m_HichCom.m_HichInd[_Index].m_Scale[2] = (m_HichCom.m_HichInd[_Index].m_Scale[3] = 0);
				m_CharaIndex[_Index] = -1;
			}
		}
	}
}
