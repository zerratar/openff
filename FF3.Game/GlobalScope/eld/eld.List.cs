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
		public class List
		{
			private Node _head;

			private Node _tale;

			private uint _size;

			public void initialize()
			{
				_head = (_tale = null);
				_size = 0u;
			}

			public List()
			{
				initialize();
			}

			~List()
			{
				eraseAll();
			}

			public Node getNode(uint num)
			{
				if (num >= _size)
				{
					return null;
				}
				int num2 = 0;
				Node node = _head;
				while (num2 < num)
				{
					node = node.next();
				}
				return node;
			}

			public object value(uint num)
			{
				if (_head == null)
				{
					return null;
				}
				Node node = _head;
				for (uint num2 = 0u; num2 < num; num2++)
				{
					node = node.next();
					if (node == null)
					{
						return null;
					}
				}
				return node.value();
			}

			public bool add(object pData)
			{
				Node node = IServer.Instance().getAllocator().allocateListNode(1u);
				if (node == null)
				{
					return false;
				}
				if (_head == null)
				{
					_head = (_tale = node);
					_tale.set(pData, null);
				}
				else
				{
					Node node2 = node;
					_tale.connect(node2);
					node2.set(pData, null);
					_tale = node2;
				}
				_size++;
				return true;
			}

			public bool insert(object pData, uint index)
			{
				uint num = size();
				if (num <= index)
				{
					add(pData);
					return true;
				}
				if (index == 0)
				{
					if (_head == null)
					{
						add(pData);
					}
					else
					{
						Node node = IServer.Instance().getAllocator().allocateListNode(1u);
						if (node == null)
						{
							return false;
						}
						node.set(pData, _head);
						_head = node;
					}
				}
				else
				{
					Node node2 = _head;
					for (uint num2 = 0u; num2 < index - 1; num2++)
					{
						node2 = node2.next();
					}
					Node next = node2.next();
					Node node = IServer.Instance().getAllocator().allocateListNode(1u);
					if (node == null)
					{
						return false;
					}
					node.set(pData, next);
					node2.connect(node);
				}
				_size++;
				return true;
			}

			public bool erase(object pData)
			{
				Node node = null;
				if (_head == null)
				{
					return false;
				}
				if (_head.value() == pData)
				{
					Node node2 = _head.next();
					node = _head;
					if (node.next() == null)
					{
						_tale = node2;
					}
					_head = node2;
				}
				else
				{
					Node node2 = _head;
					while (node2.next() != null)
					{
						if (node2.next().value() == pData)
						{
							node = node2.next();
							if (node.next() == null)
							{
								_tale = node2;
							}
							node2.connect(node.next());
							break;
						}
						node2 = node2.next();
					}
				}
				if (node != null)
				{
					IServer.Instance().getAllocator().deallocateListNode(node);
					_size--;
					return true;
				}
				return false;
			}

			public void eraseAll()
			{
				Node node = _head;
				while (node != null)
				{
					Node node2 = node.next();
					IServer.Instance().getAllocator().deallocateListNode(node);
					node = node2;
				}
				_head = null;
				_size = 0u;
			}

			public uint size()
			{
				return _size;
			}

			public Node front()
			{
				return _head;
			}

			public uint getSize()
			{
				return size();
			}

			public object getValue(uint num)
			{
				return value(num);
			}

			public bool Add(object pData)
			{
				return add(pData);
			}

			public bool Erase(object pData)
			{
				return erase(pData);
			}

			public void EraseAll()
			{
				eraseAll();
			}
		}
	}
}
