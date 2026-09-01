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
		public class IServer
		{
			public static IServer _pThis;

			protected Manager _pManager;

			protected IGL _pGL;

			protected IAllocator _pAllocator;

			protected IVramManager _pVramMng;

			public static IServer Instance()
			{
				return _pThis;
			}

			public IServer()
			{
				_pManager = null;
				_pGL = null;
				_pAllocator = null;
			}

			~IServer()
			{
				deleteManager();
				_pGL = null;
			}

			public void setIGL(IGL pGL)
			{
				_pGL = pGL;
			}

			public bool createManager()
			{
				EffAllocator<Manager> effAllocator = new EffAllocator<Manager>();
				_pManager = effAllocator.allocate(1u);
				if (_pManager == null)
				{
					return false;
				}
				return true;
			}

			public void deleteManager()
			{
				if (_pManager != null)
				{
					EffAllocator<Manager> effAllocator = new EffAllocator<Manager>();
					effAllocator.deallocate(_pManager);
					_pManager = null;
				}
			}

			public virtual bool registerID(uint[] pID)
			{
				_pManager.registerID(pID);
				return true;
			}

			public uint[] getID()
			{
				return _pManager.getID();
			}

			public bool isRegisterID()
			{
				return _pManager.isRegisterID();
			}

			public bool cmpID(uint[] pID)
			{
				return _pManager.cmpID(pID);
			}

			public bool registerFactory(Factory pFactory)
			{
				return _pManager.registerFactory(pFactory);
			}

			public bool releaseFactory(Factory pFactory)
			{
				return _pManager.releaseFactory(pFactory);
			}

			public virtual bool registerEfp(SFileHeader pEfp)
			{
				if (_pManager.registerEfp(pEfp))
				{
					return true;
				}
				return false;
			}

			public virtual bool releaseEfp(SFileHeader pEfp)
			{
				return _pManager.releaseEfp(pEfp);
			}

			public bool registerTemplate(Template pTemp)
			{
				return _pManager.registerTemplate(pTemp);
			}

			public bool releaseTemplate(Template pTemp)
			{
				if (_pManager != null)
				{
					_pManager.releaseTemplate(pTemp);
					return true;
				}
				return false;
			}

			public virtual void doExecute()
			{
				_pManager.doExecute();
			}

			public virtual void doDraw()
			{
			}

			public virtual void doUpdate()
			{
				_pManager.doUpdate();
			}

			public IObject createObject(uint ctgr, uint mem)
			{
				return _pManager.createObject(ctgr, mem);
			}

			public IObject createObject(uint ID)
			{
				return _pManager.createObject(ID);
			}

			public IObject createObject(Template pTemp)
			{
				return _pManager.createObject(pTemp);
			}

			public void deleteObject(IObject pObject)
			{
				_pManager.deleteObject(pObject);
			}

			public void releaseObjectsRequest()
			{
				_pManager.releaseObjectsRequest();
			}

			public void releaseObjects()
			{
				_pManager.releaseObjects();
			}

			public void releaseObjectsImmediately()
			{
				_pManager.releaseObjectsImmediately();
			}

			public void releaseSpecifyNoObjectRequest(uint ctgr, uint mem)
			{
				_pManager.releaseSpecifyNoObjectRequest((int)ctgr, (int)mem);
			}

			public void releaseSpecifyNoObject(uint ctgr, uint mem)
			{
				_pManager.releaseSpecifyNoObject((int)ctgr, (int)mem);
			}

			public uint getNbObjects()
			{
				return _pManager.getNbObjects();
			}

			public IObject getObjectFromLogicalIndex(uint index)
			{
				return _pManager.getObjectFromLogicalIndex(index);
			}

			public void pauseObjects()
			{
				_pManager.pauseObjects();
			}

			public void restartObject()
			{
				_pManager.restartObjects();
			}

			public void showObject(bool flag)
			{
				_pManager.showObjects(flag);
			}

			public IGL getIGL()
			{
				return _pGL;
			}

			public void setAllocator(IAllocator pAlloc)
			{
				_pAllocator = pAlloc;
			}

			public IAllocator getAllocator()
			{
				return _pAllocator;
			}

			public void setVramManager(IVramManager pMng)
			{
				_pVramMng = pMng;
			}

			public IVramManager getVramManager()
			{
				return _pVramMng;
			}
		}
	}
}
