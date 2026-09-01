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
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
	public static partial class eld
	{
		public class Guid
		{
			private SGuid _data = new SGuid();

			public bool Compare(Guid pData)
			{
				if (_data.Data1 != pData._data.Data1)
				{
					return false;
				}
				if (_data.Data2 != pData._data.Data2)
				{
					return false;
				}
				if (_data.Data3 != pData._data.Data3)
				{
					return false;
				}
				if (_data.Data4[0] != pData._data.Data4[0])
				{
					return false;
				}
				if (_data.Data4[1] != pData._data.Data4[1])
				{
					return false;
				}
				if (_data.Data4[2] != pData._data.Data4[2])
				{
					return false;
				}
				if (_data.Data4[3] != pData._data.Data4[3])
				{
					return false;
				}
				if (_data.Data4[4] != pData._data.Data4[4])
				{
					return false;
				}
				if (_data.Data4[5] != pData._data.Data4[5])
				{
					return false;
				}
				if (_data.Data4[6] != pData._data.Data4[6])
				{
					return false;
				}
				if (_data.Data4[7] != pData._data.Data4[7])
				{
					return false;
				}
				return true;
			}

			public void Set(Guid src)
			{
				_data.Data1 = src._data.Data1;
				_data.Data2 = src._data.Data2;
				_data.Data3 = src._data.Data3;
				_data.Data4[0] = src._data.Data4[0];
				_data.Data4[1] = src._data.Data4[1];
				_data.Data4[2] = src._data.Data4[2];
				_data.Data4[3] = src._data.Data4[3];
				_data.Data4[4] = src._data.Data4[4];
				_data.Data4[5] = src._data.Data4[5];
				_data.Data4[6] = src._data.Data4[6];
				_data.Data4[7] = src._data.Data4[7];
			}

			public void Set(SGuid src)
			{
				_data.Data1 = src.Data1;
				_data.Data2 = src.Data2;
				_data.Data3 = src.Data3;
				_data.Data4[0] = src.Data4[0];
				_data.Data4[1] = src.Data4[1];
				_data.Data4[2] = src.Data4[2];
				_data.Data4[3] = src.Data4[3];
				_data.Data4[4] = src.Data4[4];
				_data.Data4[5] = src.Data4[5];
				_data.Data4[6] = src.Data4[6];
				_data.Data4[7] = src.Data4[7];
			}

			~Guid()
			{
			}

			public static explicit operator Guid(ArrayReader src)
			{
				Guid guid = new Guid();
				guid._data = (SGuid)src;
				return guid;
			}
		}
	}
}
