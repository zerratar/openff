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
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
	public static partial class eld
	{
		public class Manager
		{
			private List _listObjs = new List();

			private List _listFactories = new List();

			private List _listTemplates = new List();

			private List _listEfps = new List();

			private EffAllocator<IObject> _alcObjs = new EffAllocator<IObject>();

			private uint[] _pID;

			private SIDTable _IDT = new SIDTable();

			public Manager()
			{
				_pID = null;
			}

			~Manager()
			{
				destruct();
			}

			public void destruct()
			{
				terminate();
			}

			public void terminate()
			{
				while (_listObjs.size() != 0)
				{
					IObject obj = (IObject)_listObjs.value(0u);
					_listObjs.erase(obj);
					IServer.Instance().getIGL().eraseObject(obj);
					_alcObjs.deallocate(obj);
				}
				_listFactories.eraseAll();
				_listTemplates.eraseAll();
				destroyEfp();
			}

			public void registerID(uint[] pID)
			{
				_IDT.initialize(pID);
			}

			public bool registerFactory(Factory pFactory)
			{
				return _listFactories.add(pFactory);
			}

			public bool releaseFactory(Factory pFactory)
			{
				return _listFactories.erase(pFactory);
			}

			public bool checkRegisterEfp(Array pEfp)
			{
				for (Node node = _listEfps.front(); node != null; node = node.next())
				{
					Array array = (Array)node.value();
					if (array == pEfp)
					{
						return true;
					}
				}
				return false;
			}

			public bool registerEfp(SFileHeader pEfp)
			{
				Template template = null;
				if (!_listEfps.add(pEfp))
				{
					return false;
				}
				Template[] pTemplate = pEfp.pTemplate;
				int num = 0;
				if (pEfp.uiRegistFlag != 0)
				{
					return true;
				}
				for (uint num2 = 0u; num2 < pEfp.uiNumTemplate; num2++)
				{
					template = pTemplate[num];
					for (uint num3 = 0u; num3 < _listFactories.size(); num3++)
					{
						Factory factory = (Factory)_listFactories.value(num3);
						if (factory.getGUID().Compare(template.getFactoryGUID()))
						{
							factory.initTemplate(template);
							break;
						}
					}
					num++;
				}
				pEfp.uiRegistFlag = 1;
				return true;
			}

			public bool releaseEfp(SFileHeader pEfp)
			{
				Template template = null;
				Template[] pTemplate = pEfp.pTemplate;
				int num = 0;
				for (uint num2 = 0u; num2 < pEfp.uiNumTemplate; num2++)
				{
					template = pTemplate[num];
					for (uint num3 = 0u; num3 < _listFactories.size(); num3++)
					{
						Factory factory = (Factory)_listFactories.value(num3);
						if (factory.getGUID().Compare(template.getFactoryGUID()))
						{
							factory.disposeTemplate(template);
							break;
						}
					}
					num++;
				}
				return _listEfps.erase(pEfp);
			}

			public void destroyEfp()
			{
				while (_listEfps.size() != 0)
				{
					releaseEfp((SFileHeader)_listEfps.value(0u));
				}
			}

			public bool registerTemplate(Template pTemplate)
			{
				return false;
			}

			public bool releaseTemplate(Template pTemplate)
			{
				return false;
			}

			public IObject createObject(uint category, uint member)
			{
				Template template = getTemplate(category, member);
				if (template == null)
				{
					return null;
				}
				IObject obj = createObject(template);
				if (obj != null)
				{
					obj.setNumberInformation((int)category, (int)member);
					obj.Start(0u);
					return obj;
				}
				return null;
			}

			public IObject createObject(uint ID)
			{
				uint num = 0u;
				Template template = (Template)_listTemplates.value(num);
				while (template != null && template.getOwnID() != ID)
				{
					num++;
					template = (Template)_listTemplates.value(num);
				}
				return createObject(template);
			}

			public IObject createObject(Template pTemplate)
			{
				uint num = _listFactories.size();
				for (uint num2 = 0u; num2 < num; num2++)
				{
					Factory factory = (Factory)_listFactories.value(num2);
					if (!pTemplate.getFactoryGUID().Compare(factory.getGUID()))
					{
						continue;
					}
					IObject obj = factory.createObj(pTemplate);
					if (obj != null)
					{
						obj.SetTemplate(pTemplate);
						if (addObject(obj))
						{
							if (IServer.Instance().getIGL().registerObject(obj))
							{
								return obj;
							}
							_listObjs.erase(obj);
						}
						_alcObjs.deallocate(obj);
						return null;
					}
					return null;
				}
				return null;
			}

			public void doExecute()
			{
				uint num = 13u;
				for (uint num2 = 0u; num2 < _listObjs.size(); num2++)
				{
					IObject obj = (IObject)_listObjs.value(num2);
					if ((obj.GetStatus() & num) != 0)
					{
						obj.Calculate();
					}
				}
			}

			public void doUpdate()
			{
				Node node = _listObjs.front();
				while (node != null)
				{
					IObject obj = null;
					IObject obj2 = (IObject)node.value();
					switch (obj2.GetCurrentCommand())
					{
					case 1u:
						if (obj2.StartCheck())
						{
							if (!obj2.Initialize(IServer.Instance().getIGL()))
							{
								obj2.Terminate();
								obj2.Ready();
							}
							else
							{
								obj2.SetStatus(1u);
							}
							obj2.Advance();
						}
						break;
					case 2u:
						obj2.SetStatus(2u);
						obj2.Advance();
						break;
					case 4u:
						if (!obj2.isPlay())
						{
							obj2.Terminate();
							obj2.Advance();
						}
						break;
					case 8u:
						obj2.SetStatus(8u);
						obj2.Advance();
						break;
					case 16u:
						obj = obj2;
						break;
					case 64u:
						if (obj2.isPlay())
						{
							obj2.Advance();
						}
						break;
					}
					node = node.next();
					if (obj != null && _listObjs.erase(obj) && IServer.Instance().getIGL().eraseObject(obj))
					{
						_alcObjs.deallocate(obj);
					}
				}
			}

			public void deleteObject(IObject pObject)
			{
				for (uint num = 0u; num < _listObjs.size(); num++)
				{
					if (pObject == (IObject)_listObjs.value(num))
					{
						pObject.DeleteObject();
					}
				}
			}

			public void releaseObjectsRequest()
			{
				for (uint num = 0u; num < _listObjs.size(); num++)
				{
					((IObject)_listObjs.value(num)).DeleteObjReq();
				}
			}

			public void releaseObjects()
			{
				for (uint num = 0u; num < _listObjs.size(); num++)
				{
					((IObject)_listObjs.value(num)).DeleteObject();
				}
			}

			public void releaseObjectsImmediately()
			{
				while (_listObjs.size() != 0)
				{
					IObject obj = (IObject)_listObjs.value(0u);
					if (_listObjs.erase(obj) && IServer.Instance().getIGL().eraseObject(obj))
					{
						_alcObjs.deallocate(obj);
					}
				}
			}

			public void releaseSpecifyNoObjectRequest(int ctgr, int mem)
			{
				for (uint num = 0u; num < _listObjs.size(); num++)
				{
					IObject obj = (IObject)_listObjs.value(num);
					if (obj != null && obj.getCategoryNo() == ctgr && obj.getMemberNo() == mem)
					{
						obj.DeleteObjReq();
					}
				}
			}

			public void releaseSpecifyNoObject(int ctgr, int mem)
			{
				for (uint num = 0u; num < _listObjs.size(); num++)
				{
					IObject obj = (IObject)_listObjs.value(num);
					if (obj != null && obj.getCategoryNo() == ctgr && obj.getMemberNo() == mem)
					{
						obj.DeleteObject();
					}
				}
			}

			public void pauseObjects()
			{
				for (uint num = 0u; num < _listObjs.size(); num++)
				{
					((IObject)_listObjs.value(num)).Pause();
				}
			}

			public void restartObjects()
			{
				for (uint num = 0u; num < _listObjs.size(); num++)
				{
					((IObject)_listObjs.value(num)).Restart();
				}
			}

			public void showObjects(bool flag)
			{
				for (uint num = 0u; num < _listObjs.size(); num++)
				{
					((IObject)_listObjs.value(num)).setDrawFlag(flag);
				}
			}

			public bool addObject(IObject pObject)
			{
				return _listObjs.add(pObject);
			}

			public Template getTemplate(uint category, uint member)
			{
				uint templateID = getTemplateID(category, member);
				return getTemplate(templateID);
			}

			public Template getTemplate(uint ID)
			{
				for (Node node = _listEfps.front(); node != null; node = node.next())
				{
					SFileHeader sFileHeader = (SFileHeader)node.value();
					SFileHeader sFileHeader2 = sFileHeader;
					Template[] pTemplate = sFileHeader2.pTemplate;
					int num = 0;
					uint num2 = 0u;
					while (num2 < sFileHeader2.uiNumTemplate)
					{
						Template template = pTemplate[num];
						if (ID == template.getOwnID())
						{
							return template;
						}
						num2++;
						num++;
					}
				}
				return null;
			}

			public uint getTemplateID(uint category, uint member)
			{
				if (_IDT.getNbCategories() > category)
				{
					return _IDT.getTemplateID(category, member);
				}
				return 0u;
			}

			public uint[] getID()
			{
				return _IDT.getIDTable();
			}

			public bool isRegisterID()
			{
				if (_IDT.isEmpty())
				{
					return false;
				}
				return true;
			}

			public bool cmpID(uint[] pIDn)
			{
				if (!isRegisterID())
				{
					return false;
				}
				if (pIDn == _IDT.getIDTable())
				{
					return true;
				}
				uint[] iDTable = _IDT.getIDTable();
				uint nbCategories = _IDT.getNbCategories();
				int num = 0;
				int num2 = 0;
				if (nbCategories != pIDn[num])
				{
					return false;
				}
				num2 += 4;
				num += 4;
				for (uint num3 = 0u; num3 < nbCategories; num3++)
				{
					for (uint num4 = 0u; num4 < 2; num4++)
					{
						if (iDTable[num2] != pIDn[num])
						{
							return false;
						}
						num2++;
						num++;
					}
				}
				return true;
			}

			public IObject getObjectFromLogicalIndex(uint index)
			{
				for (uint num = 0u; num < _listObjs.size(); num++)
				{
					IObject obj = (IObject)_listObjs.value(num);
					if (obj != null && obj.getLogicalIndex() == index)
					{
						return obj;
					}
				}
				return null;
			}

			public uint getNbObjects()
			{
				return _listObjs.size();
			}
		}
	}
}
