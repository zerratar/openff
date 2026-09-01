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
			public class SceneObject
			{
				private Scene _scene;

				public SLNode<SceneObject> _node = new SLNode<SceneObject>();

				private SLList<SceneElement> _list = new SLList<SceneElement>();

				private bool _bShow;

				public SceneObject()
				{
					_scene = null;
					_bShow = true;
					_node.setData(this);
				}

				~SceneObject()
				{
					resetElementList();
				}

				public virtual void drawElementList(Scene scene)
				{
					SLNode<SceneElement> sLNode = _list.front();
					Texture texture = null;
					Texture texture2 = null;
					while (sLNode != null)
					{
						SceneElement sceneElement = sLNode.data();
						if (sceneElement.isShow())
						{
							texture2 = sceneElement.getTextureObject().getTextureBody();
							if (texture2 != null && texture2 != texture)
							{
								sceneElement.getTextureObject().sendTexture();
								texture = texture2;
							}
							if (texture2 != null && G3_CheckTexImage())
							{
								sceneElement.draw(scene);
							}
						}
						sLNode = sLNode.next();
					}
				}

				public void addElement(SceneElement elem)
				{
					_list.insert(null, new SLNode<SceneElement>[1] { elem._node }, 1u);
				}

				public void removeElement(SceneElement elem)
				{
					_list.erase(elem._node);
				}

				public void resetElementList()
				{
					_list.eraseAll();
				}

				public int getNbElements()
				{
					return (int)_list.size();
				}

				public virtual bool isShow()
				{
					return _bShow;
				}

				public virtual void show(bool bShow)
				{
					_bShow = bShow;
				}

				public void setScene(Scene pScene)
				{
					_scene = pScene;
				}
			}
		}
	}
}
