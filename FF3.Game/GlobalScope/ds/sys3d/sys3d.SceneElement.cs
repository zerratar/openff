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
	public static partial class ds
	{
		public static partial class sys3d
		{
			public class SceneElement
			{
				public static BasicTextureObject _objNull = new BasicTextureObject();

				public SLNode<SceneElement> _node = new SLNode<SceneElement>();

				private SceneObject _objScn;

				public SceneElement(SceneObject objScn)
				{
					_node.setData(this);
					_objScn = objScn;
				}

				~SceneElement()
				{
				}

				public virtual void draw(Scene scene)
				{
				}

				public virtual bool isShow()
				{
					return true;
				}

				public virtual TextureObject getTextureObject()
				{
					return _objNull;
				}
			}
		}
	}
}
