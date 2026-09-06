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
	public static partial class eld
	{
		public class IGL : ds.sys3d.SceneObject
		{
			protected List _listObjs = new List();

			~IGL()
			{
				_listObjs.eraseAll();
			}

			public bool registerObject(IObject pObject)
			{
				return _listObjs.add(pObject);
			}

			public bool eraseObject(IObject pObject)
			{
				return _listObjs.erase(pObject);
			}

			public void drawObjects()
			{
				uint num = _listObjs.size();
				uint num2 = 11u;
				for (uint num3 = 0u; num3 < num; num3++)
				{
					IObject obj = (IObject)_listObjs.value(num3);
					if ((obj.GetStatus() & num2) != 0)
					{
						obj.Render(this);
					}
				}
			}

			public virtual ds.Texture CreateTexture(ds.Texture pAddr)
			{
				return null;
			}

			public virtual void SetTexture(ds.Texture pTexture)
			{
			}
		}
	}
}
