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
		public class Ncgr : NCData
		{
			public override bool Load(string pFile_name)
			{
				base.Load(pFile_name);
				if (m_pCharacter == null)
				{
					m_pCharacter = new NNSG2dCharacterData();
				}
				NNS_G2dGetUnpackedCharacterData(pData(), m_pCharacter);
				return true;
			}

			public override bool Load(byte[] pFile_name)
			{
				base.Load(pFile_name);
				if (m_pCharacter == null)
				{
					m_pCharacter = new NNSG2dCharacterData();
				}
				NNS_G2dGetUnpackedCharacterData(pData(), m_pCharacter);
				return true;
			}

			public bool LoadBg(string pFile_name)
			{
				base.Load(pFile_name);
				if (m_pCharacter == null)
				{
					m_pCharacter = new NNSG2dCharacterData();
				}
				NNS_G2dGetUnpackedBGCharacterData(pData(), m_pCharacter);
				return true;
			}

			public NNSG2dCharacterData pDataCg()
			{
				return m_pCharacter;
			}

			public void copy(Ncgr src)
			{
				copy((NCData)src);
			}
		}
	}
}
