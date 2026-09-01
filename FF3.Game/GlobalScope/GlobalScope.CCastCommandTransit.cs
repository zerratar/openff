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
	public class CCastCommandTransit
	{
		public class CAST_MAPJUMP
		{
			public bool m_Flag;

			public string m_MapName;

			public sbyte m_CharacterType;

			public VecFx32 m_Pos = new VecFx32();

			public VecFx32 m_Rot = new VecFx32();

			public bool m_Flag2;

			public int m_MapJumpIndex;

			public void initialize()
			{
				m_Flag = false;
				m_MapName = null;
				m_CharacterType = 0;
				m_Pos.x = (m_Pos.y = (m_Pos.z = 0));
				m_Rot.x = (m_Rot.y = (m_Rot.z = 0));
				m_Flag2 = false;
				m_MapJumpIndex = 0;
			}

			public void setUp(string _MapName, sbyte _CharacterType, VecFx32 _Pos, VecFx32 _Rot, bool _Flag)
			{
				strcpy(out m_MapName, _MapName);
				m_Flag = _Flag;
				m_CharacterType = _CharacterType;
				m_Pos.copy(_Pos);
				m_Rot.copy(_Rot);
			}

			public void setUp2(string mapname, int jumpIdx)
			{
				strcpy(out m_MapName, mapname);
				m_MapJumpIndex = jumpIdx;
				m_Flag2 = true;
			}
		}

		public static CCastCommandTransit m_Instance = new CCastCommandTransit();

		private wld.CBaseSystem m_BaseSystem;

		private CAST_MAPJUMP m_CastMapJump = new CAST_MAPJUMP();

		private int m_InnValue;

		private bool m_InnConfirm;

		public int changeHichNumber(uint _HichIndex)
		{
			int num = -1;
			if (_HichIndex == 0)
			{
				return wld.CWorldOutSideData.getInstance().PlayerData().getPlayCharacterIndex();
			}
			num = evt.CHichParameterManager.getInstance().getManCastIndex(_HichIndex);
			_ = 0;
			return evt.CHichParameterManager.getInstance().CharaIndex(num);
		}

		public CCastCommandTransit()
		{
			m_BaseSystem = null;
		}

		public static CCastCommandTransit getInstance()
		{
			return m_Instance;
		}

		public void setCast_BaseSystem(wld.CBaseSystem _sys)
		{
			m_BaseSystem = _sys;
		}

		public wld.CBaseSystem cast_BaseSystem()
		{
			return m_BaseSystem;
		}

		public pl.CPlayerManager cast_PlayerMng()
		{
			return cast_BaseSystem().PlayerMng();
		}

		public cmr.CWorldCamera cast_FieldCamera()
		{
			return cast_BaseSystem().WorldCamera();
		}

		public wld.CWorld2DManager cast_Field2D()
		{
			return cast_BaseSystem().World2DMng();
		}

		public void cast_setInnValue(int Type)
		{
			m_InnValue = Type;
		}

		public int cast_getInnValue()
		{
			return m_InnValue;
		}

		public void cast_setInnConfirm(bool Confirm)
		{
			m_InnConfirm = Confirm;
		}

		public bool cast_getInnConfirm()
		{
			return m_InnConfirm;
		}

		public CAST_MAPJUMP castParam_MapJump()
		{
			return m_CastMapJump;
		}

		public void initialize()
		{
			m_BaseSystem = null;
		}

		public void terminate()
		{
			m_BaseSystem = null;
		}
	}
}
