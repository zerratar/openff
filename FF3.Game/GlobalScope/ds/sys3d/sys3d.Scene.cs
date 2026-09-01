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
			public class Scene
			{
				private const int render_obj_max = 32;

				public static byte LAYER_MAX = 4;

				public static byte LAYER_TOP = 0;

				public static byte LAYER_BOTTOM = (byte)(LAYER_MAX - 1);

				private SceneGraph _graph = new SceneGraph();

				private CCamera _camera;

				private Vector<SceneRenderObject, FastErasePolicy<SceneRenderObject>>[] _dlayer = new Vector<SceneRenderObject, FastErasePolicy<SceneRenderObject>>[LAYER_MAX];

				private uint[] _objCount = new uint[LAYER_MAX];

				public Scene()
				{
					_camera = null;
					for (int i = 0; i < _dlayer.Length; i++)
					{
						_dlayer[i] = new Vector<SceneRenderObject, FastErasePolicy<SceneRenderObject>>(32);
					}
					_graph.setScene(this);
					initRenderObjectsInfo();
				}

				~Scene()
				{
				}

				public void initialize()
				{
					initRenderObjectsInfo();
				}

				public void draw(bool bVBlank)
				{
					for (int i = 0; i < _dlayer[0].size(); i++)
					{
						if (_dlayer[0][i].getPriority() == -1)
						{
							_dlayer[0][i].draw();
						}
					}
					NNS_G3dSetDrawMask(1u);
					for (int j = 0; j < _dlayer[0].size(); j++)
					{
						if (_dlayer[0][j].getPriority() == 0)
						{
							_dlayer[0][j].draw();
						}
					}
					NNS_G3dSetDrawMask(0u);
					for (byte b = 1; b < LAYER_MAX; b++)
					{
						for (int k = 0; k < _dlayer[b].size(); k++)
						{
							if (_dlayer[b][k].getPriority() == 0)
							{
								_dlayer[b][k].draw();
							}
						}
					}
					NNS_G3dSetDrawMask(2u);
					for (int l = 0; l < _dlayer[0].size(); l++)
					{
						if (_dlayer[0][l].getPriority() == 0)
						{
							_dlayer[0][l].draw();
						}
					}
					NNS_G3dSetDrawMask(0u);
					NNS_G3dSetDrawMask(0u);
					for (byte b2 = 1; b2 < LAYER_MAX; b2++)
					{
						for (int m = 0; m < _dlayer[b2].size(); m++)
						{
							if (_dlayer[b2][m].getPriority() == 1)
							{
								_dlayer[b2][m].draw();
							}
						}
					}
					_graph.drawObjects();
					if (bVBlank && CDevice.singleton().getFPS() == CDevice.enFPS.enFPS_30)
					{
						ulong num = (ulong)OS_GetTick() - CDevice.singleton().getPreVBlankTick();
						if (num <= 8000)
						{
							CDevice.singleton().waitVBlank();
						}
					}
				}

				public void addVisualObject(SceneObject objScn)
				{
					_graph.addObject(objScn);
					objScn.setScene(this);
				}

				public void eraseVisualObject(SceneObject objScn)
				{
					_graph.eraseObject(objScn);
					objScn.setScene(null);
				}

				public uint getNbVisualObjects()
				{
					return _graph.getNbObjects();
				}

				public void initRenderObjectsInfo()
				{
					for (byte b = 0; b < LAYER_MAX; b++)
					{
						_dlayer[b].clear();
						_objCount[b] = 0u;
					}
				}

				public void addRenderObject(SceneRenderObject pObj, byte layer)
				{
					if (layer < LAYER_MAX)
					{
						_dlayer[layer].push_back(pObj);
						_objCount[layer]++;
					}
				}

				public void removeRenderObject(SceneRenderObject pObj)
				{
					for (byte b = 0; b < LAYER_MAX; b++)
					{
						for (byte b2 = 0; b2 < _dlayer[b].size(); b2++)
						{
							if (pObj == _dlayer[b][b2])
							{
								_dlayer[b].erase(b2);
								_objCount[b]--;
								return;
							}
						}
					}
				}

				public uint getNbRenderObject(byte layer)
				{
					if (layer >= LAYER_MAX)
					{
						return 0u;
					}
					return _objCount[layer];
				}

				public void setCamera(CCamera camera)
				{
					if (camera != null)
					{
						_camera = camera;
					}
				}

				public CCamera getCamera()
				{
					return _camera;
				}
			}
		}
	}
}
