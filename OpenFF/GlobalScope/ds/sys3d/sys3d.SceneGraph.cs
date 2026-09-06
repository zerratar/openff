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
	public static partial class ds
	{
		public static partial class sys3d
		{
			public class SceneGraph
			{
				private Scene _scene;

				private SLList<SceneObject> _listObj = new SLList<SceneObject>();

				public SceneGraph()
				{
					_scene = null;
				}

				~SceneGraph()
				{
					resetObjectList();
				}

				public void drawObjects()
				{
					SLNode<SceneObject> sLNode = _listObj.front();
					G3_MtxMode(GXMtxMode.GX_MTXMODE_TEXTURE);
					G3_Identity();
					while (sLNode != null)
					{
						SceneObject sceneObject = sLNode.data();
						if (sceneObject.isShow())
						{
							sceneObject.drawElementList(_scene);
						}
						sLNode = sLNode.next();
					}
				}

				public void addObject(SceneObject obj)
				{
					_listObj.insert(null, new SLNode<SceneObject>[1] { obj._node }, 1u);
				}

				public void eraseObject(SceneObject obj)
				{
					_listObj.erase(obj._node);
				}

				public uint getNbObjects()
				{
					return _listObj.size();
				}

				public void resetObjectList()
				{
					_listObj.eraseAll();
				}

				public void setScene(Scene scene)
				{
					_scene = scene;
				}

				public Scene getScene()
				{
					return _scene;
				}
			}
		}
	}
}
