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
		public class CNPCWorldParameterManager
		{
			public static CNPCWorldParameterManager m_Instance = new CNPCWorldParameterManager();

			private Array m_FileAddr;

			private CNPCWorldRandomMoveParameter[] m_NPCWorldRandomMove;

			private CNPCWorldAutoFollowParameter[] m_NPCWorldAutoFollow;

			public void Initialize()
			{
				Free();
				m_FileAddr = null;
				m_NPCWorldRandomMove = null;
				m_NPCWorldAutoFollow = null;
			}

			public bool Load(string file_name)
			{
				Free();
				bool flag = false;
				strcpy(out var arg, file_name);
				uint size = ds.g_File.getSize(arg);
				m_FileAddr = ds.CHeap.alloc_app(size);
				flag = ds.g_File.load(m_FileAddr, arg);
				m_NPCWorldRandomMove = CNPCWorldRandomMoveParameter.ChainPointer((byte[])m_FileAddr, 0);
				m_NPCWorldAutoFollow = CNPCWorldAutoFollowParameter.ChainPointer((byte[])m_FileAddr, 1);
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

			public CNPCWorldBaseMoveParameter NPCWorldBaseMoveParameter(int _id)
			{
				return null;
			}

			public CNPCWorldRandomMoveParameter NPCWorldRandomMoveParameter(int _id)
			{
				return m_NPCWorldRandomMove[_id];
			}

			public CNPCWorldAutoFollowParameter NPCWorldAutoFollowParameter(int _id)
			{
				return m_NPCWorldAutoFollow[_id];
			}

			public CNPCWorldParameterManager()
			{
				m_FileAddr = null;
				m_NPCWorldRandomMove = null;
				m_NPCWorldAutoFollow = null;
			}

			public static CNPCWorldParameterManager Instance()
			{
				return m_Instance;
			}
		}
	}
}
