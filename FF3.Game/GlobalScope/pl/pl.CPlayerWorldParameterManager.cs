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
	public static partial class pl
	{
		public class CPlayerWorldParameterManager
		{
			public static CPlayerWorldParameterManager m_Instance = new CPlayerWorldParameterManager();

			private Array m_FileAddr;

			private CPlayerWorldMoveParameter[] m_PlayerWorldMove;

			private CPlayerWorldEnterParameter[] m_PlayerWorldEnter;

			private CPlayerVehicleWorldMoveParameter[] m_PlayerVehicleWorldMove;

			private CPlayerVehicleWorldEnterParameter[] m_PlayerVehicleWorldEnter;

			private CPlayerWorldSeEffectPlayParameter[] m_PlayerWorldSeEffectPlay;

			private CPlayerWorldSeEffectMapParameter[] m_PlayerWorldSeEffectMap;

			public void Initialize()
			{
				Free();
				m_FileAddr = null;
				m_PlayerWorldMove = null;
				m_PlayerWorldEnter = null;
				m_PlayerVehicleWorldMove = null;
				m_PlayerVehicleWorldEnter = null;
			}

			public bool Load(string file_name)
			{
				Free();
				bool flag = false;
				strcpy(out var arg, file_name);
				uint size = ds.g_File.getSize(arg);
				m_FileAddr = ds.CHeap.alloc_app(size);
				flag = ds.g_File.load(m_FileAddr, arg);
				m_PlayerWorldMove = CPlayerWorldMoveParameter.ChainPointer((byte[])m_FileAddr, 0);
				m_PlayerWorldEnter = CPlayerWorldEnterParameter.ChainPointer((byte[])m_FileAddr, 1);
				m_PlayerVehicleWorldMove = CPlayerVehicleWorldMoveParameter.ChainPointer((byte[])m_FileAddr, 2);
				m_PlayerVehicleWorldEnter = CPlayerVehicleWorldEnterParameter.ChainPointer((byte[])m_FileAddr, 3);
				m_PlayerWorldSeEffectPlay = CPlayerWorldSeEffectPlayParameter.ChainPointer((byte[])m_FileAddr, 4);
				m_PlayerWorldSeEffectMap = CPlayerWorldSeEffectMapParameter.ChainPointer((byte[])m_FileAddr, 5);
				return flag;
			}

			public void Free()
			{
				if (m_FileAddr != null)
				{
					ds.CHeap.free_app(m_FileAddr);
					m_FileAddr = null;
				}
			}

			public CPlayerWorldMoveParameter PlayerWorldMoveParameter(int _id)
			{
				if (_id < 0)
				{
					_id = 0;
				}
				return m_PlayerWorldMove[_id];
			}

			public CPlayerWorldEnterParameter PlayerWorldEnterParameter(int _id)
			{
				return m_PlayerWorldEnter[_id];
			}

			public CPlayerVehicleWorldMoveParameter PlayerVehicleWorldMoveParameter(int _id)
			{
				return m_PlayerVehicleWorldMove[_id];
			}

			public CPlayerVehicleWorldEnterParameter PlayerVehicleWorldEnterParameter(int _id)
			{
				return m_PlayerVehicleWorldEnter[_id];
			}

			public CPlayerWorldSeEffectPlayParameter PlayerWorldSeEffectPlayParameter(int _id)
			{
				return m_PlayerWorldSeEffectPlay[_id];
			}

			public CPlayerWorldSeEffectMapParameter PlayerWorldSeEffectMapParameter(int _id)
			{
				return m_PlayerWorldSeEffectMap[_id];
			}

			public CPlayerWorldParameterManager()
			{
				m_FileAddr = null;
				m_PlayerWorldMove = null;
				m_PlayerWorldEnter = null;
				m_PlayerVehicleWorldMove = null;
				m_PlayerVehicleWorldEnter = null;
				m_PlayerWorldSeEffectPlay = null;
				m_PlayerWorldSeEffectMap = null;
			}

			public static CPlayerWorldParameterManager Instance()
			{
				return m_Instance;
			}
		}
	}
}
