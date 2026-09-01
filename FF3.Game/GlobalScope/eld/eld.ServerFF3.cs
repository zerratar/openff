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
		public class ServerFF3 : Server
		{
			private List _listLoadEfp = new List();

			private List _listRegsEfp;

			private ds.sys3d.Scene _scene;

			private uint[] _id;

			private List _listReserveEfp = new List();

			private int _nNbReserveEfps;

			private ImpParticleDSFactory _fcParticleDS = new ImpParticleDSFactory();

			private ImpParticleLargeDSFactory _fcParticleLargeDS = new ImpParticleLargeDSFactory();

			private ImpParticleGatherDSFactory _fcParticleGatherDS = new ImpParticleGatherDSFactory();

			private ImpModelDSFactory _fcModelDS = new ImpModelDSFactory();

			private ImpSequenceDSFactory _fcSequenceDS = new ImpSequenceDSFactory();

			public bool setup(DSGL pGL, DSAllocator pAllocator, DSVramManager pVramMng, ds.sys3d.Scene pScene)
			{
				setAllocator(pAllocator);
				pAllocator.initializeNodePool();
				setIGL(pGL);
				pVramMng.initialize();
				setVramManager(pVramMng);
				_scene = pScene;
				_scene.addVisualObject(pGL);
				bool flag = createManager();
				if (flag)
				{
					registerFactories();
				}
				initReserveList(8u);
				return flag;
			}

			public void cleanup()
			{
				eraseObjects();
				_scene.eraseVisualObject(static_cast<DSGL>(_pGL));
				releaseID();
				destroyEfp();
				IServer.Instance().getVramManager().cleanup();
				deregisterFactories();
				destroyReserveList();
				((DSAllocator)_pAllocator).cleanupNodePool();
				deleteManager();
			}

			public bool loadID(string szFilename)
			{
				releaseID();
				uint num = ds.g_File.getSize(const_cast<string>(szFilename)) + 4;
				num >>= 2;
				EffAllocator<uint> effAllocator = new EffAllocator<uint>();
				_id = effAllocator.allocateMemoryArry(num);
				ds.g_File.load(_id, const_cast<string>(szFilename));
				base.registerID(_id);
				return true;
			}

			public override bool registerID(uint[] pID)
			{
				releaseID();
				registerID(pID);
				return true;
			}

			public SFileHeader loadEfp(string szFilename)
			{
				uint size = ds.g_File.getSize(const_cast<string>(szFilename));
				if (size == 0)
				{
					return null;
				}
				Array array = ds.CHeap.alloc_app(size);
				ds.g_File.load(array, const_cast<string>(szFilename));
				SFileHeader sFileHeader = (SFileHeader)array;
				if (_listLoadEfp.add(sFileHeader) && base.registerEfp(sFileHeader))
				{
					return sFileHeader;
				}
				_listLoadEfp.erase(sFileHeader);
				return null;
			}

			public Array divideLoadEfp(string szFilename)
			{
				if (_nNbReserveEfps >= _listReserveEfp.size())
				{
					return null;
				}
				uint size = ds.g_File.getSize(const_cast<string>(szFilename));
				if (size == 0)
				{
					return null;
				}
				Array array = ds.CHeap.alloc_app(size);
				ds.g_File.load(array, const_cast<string>(szFilename));
				_listReserveEfp.getNode((uint)_nNbReserveEfps).setValue(array);
				_nNbReserveEfps++;
				return array;
			}

			public bool unloadEfp(SFileHeader unEfpID)
			{
				base.releaseEfp(unEfpID);
				if (_listLoadEfp.erase(unEfpID))
				{
					return true;
				}
				Node node = _listReserveEfp.front();
				for (int i = 0; i < _nNbReserveEfps; i++)
				{
					if (node.value() == unEfpID)
					{
						sortReserveList(node);
						return true;
					}
					node = node.next();
				}
				return false;
			}

			public override bool registerEfp(SFileHeader pEfp)
			{
				return base.registerEfp(pEfp);
			}

			public override bool releaseEfp(SFileHeader pEfp)
			{
				return base.releaseEfp(pEfp);
			}

			public void destroyEfp()
			{
				eraseObjects();
				while (_listLoadEfp.size() != 0)
				{
					unloadEfp((SFileHeader)_listLoadEfp.value(0u));
				}
				_pManager.destroyEfp();
			}

			public override void doExecute()
			{
				updateReserveList();
				base.doExecute();
			}

			public override void doUpdate()
			{
				base.doUpdate();
			}

			public void eraseObjects()
			{
				releaseObjectsImmediately();
			}

			public void releaseID()
			{
				if (_id != null)
				{
					EffAllocator<uint> effAllocator = new EffAllocator<uint>();
					effAllocator.deallocateMemoryArry(_id);
					_id = null;
				}
			}

			public void registerFactories()
			{
				registerFactory(_fcParticleDS);
				registerFactory(_fcParticleLargeDS);
				registerFactory(_fcParticleGatherDS);
				registerFactory(_fcModelDS);
				registerFactory(_fcSequenceDS);
			}

			public void deregisterFactories()
			{
				releaseFactory(_fcParticleDS);
				releaseFactory(_fcParticleLargeDS);
				releaseFactory(_fcParticleGatherDS);
				releaseFactory(_fcModelDS);
				releaseFactory(_fcSequenceDS);
			}

			public void initReserveList(uint unNbLists)
			{
				destroyReserveList();
				for (int i = 0; i < unNbLists; i++)
				{
					if (!_listReserveEfp.add(null))
					{
						destroyReserveList();
						break;
					}
				}
			}

			public void updateReserveList()
			{
				object obj = null;
				if (_nNbReserveEfps != 0)
				{
					Node node = _listReserveEfp.front();
					obj = node.value();
					if (_listLoadEfp.add(obj) && base.registerEfp((SFileHeader)obj))
					{
						sortReserveList(node);
						return;
					}
				}
				if (obj != null)
				{
					_listLoadEfp.erase(obj);
				}
			}

			public void destroyReserveList()
			{
				while (_listReserveEfp.size() != 0)
				{
					Node node = _listReserveEfp.front();
					node.value();
					_listReserveEfp.erase(node.value());
				}
				_listReserveEfp.eraseAll();
				_nNbReserveEfps = 0;
			}

			public void sortReserveList(Node pNode)
			{
				_nNbReserveEfps--;
				Node node = _listReserveEfp.getNode((uint)_nNbReserveEfps);
				pNode.setValue(node.value());
				node.setValue(null);
			}

			public ds.sys3d.Scene scene()
			{
				return _scene;
			}

			public ServerFF3()
			{
				_scene = null;
				_id = null;
				_nNbReserveEfps = 0;
			}

			~ServerFF3()
			{
			}
		}
	}
}
