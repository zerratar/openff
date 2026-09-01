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
	public static partial class eld
	{
		public class Template
		{
			private Guid _FactoryGUID;

			private uint _uiTemplateID;

			private uint _uiVersion;

			private uint _pAnim;

			private uint _pTexture;

			private uint pParameter;

			private Array pAddr;

			public object m_AnimObject;

			public object m_TextureObject;

			public object[] m_CastObject = new object[2];

			public static Template create(Array pData)
			{
				return reinterpret_cast<Template>(pData);
			}

			public void setFactoryGUID(Guid guid)
			{
				_FactoryGUID.Set(guid);
			}

			public Guid getFactoryGUID()
			{
				return _FactoryGUID;
			}

			public uint getOwnID()
			{
				return _uiTemplateID;
			}

			public void setOwnID(uint ID)
			{
				_uiTemplateID = ID;
			}

			public ArrayReader GetParameter()
			{
				ArrayReader arrayReader = new ArrayReader(pAddr);
				arrayReader.setPosition(pParameter);
				return arrayReader;
			}

			public uint GetAnimAddr()
			{
				return _pAnim;
			}

			public T GetAnimAddr<T>()
			{
				if (m_AnimObject == null)
				{
					Type typeFromHandle = typeof(T);
					MethodInfo method = typeFromHandle.GetMethod("cast");
					m_CastObject[0] = pAddr;
					m_CastObject[1] = _pAnim;
					m_AnimObject = method.Invoke(null, m_CastObject);
				}
				return (T)m_AnimObject;
			}

			public void SetAnimAddr(uint addr)
			{
				_pAnim = addr;
			}

			public uint GetTextureAddr()
			{
				return _pTexture;
			}

			public T GetTextureAddr<T>()
			{
				if (m_TextureObject == null)
				{
					Type typeFromHandle = typeof(T);
					if ((object)typeFromHandle == typeof(Array))
					{
						ArrayReader arrayReader = new ArrayReader(pAddr);
						arrayReader.setPosition(_pTexture);
						byte[] array = new byte[arrayReader.rest()];
						arrayReader.read(array, 0, array.Length);
						m_TextureObject = array;
					}
					else
					{
						MethodInfo method = typeFromHandle.GetMethod("cast");
						m_CastObject[0] = pAddr;
						m_CastObject[1] = _pTexture;
						m_TextureObject = method.Invoke(null, m_CastObject);
					}
				}
				return (T)m_TextureObject;
			}

			public void SetTextureAddr(uint addr)
			{
				_pTexture = addr;
			}

			public void SetVersion(uint version)
			{
				_uiVersion = version;
			}

			public uint GetVersion()
			{
				return _uiVersion;
			}

			public static explicit operator Template(ArrayReader src)
			{
				Template template = new Template();
				template._FactoryGUID = (Guid)src;
				template._uiTemplateID = src.readUInt32();
				template._uiVersion = src.readUInt32();
				template._pAnim = src.readUInt32();
				template._pTexture = src.readUInt32();
				template.pParameter = (uint)src.getPosition();
				template.pAddr = src.getBytes();
				return template;
			}
		}
	}
}
