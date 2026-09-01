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
	public class XbnNode
	{
		public enum DATA_TYPE
		{
			DT_VOID,
			DT_STRING,
			DT_DECIMAL,
			NUMBER_OF_DT
		}

		public const DATA_TYPE DT_VOID = DATA_TYPE.DT_VOID;

		public const DATA_TYPE DT_STRING = DATA_TYPE.DT_STRING;

		public const DATA_TYPE DT_DECIMAL = DATA_TYPE.DT_DECIMAL;

		public const DATA_TYPE NUMBER_OF_DT = DATA_TYPE.NUMBER_OF_DT;

		public string nodeName_;

		public DATA_TYPE dataType_;

		public int nodeData_;

		private int numberOfChildren_;

		private int numberOfFamily_;

		private int m_iOffset;

		public string m_strData;

		public XbnNode[] m_Array;

		public int m_iId;

		public XbnNode()
		{
			nodeName_ = null;
			dataType_ = DATA_TYPE.DT_VOID;
			nodeData_ = 0;
			numberOfChildren_ = 0;
			numberOfFamily_ = 0;
		}

		public int nodeValueInt()
		{
			return nodeData_;
		}

		public string nodeValueString()
		{
			return m_strData;
		}

		public XbnNode firstChild()
		{
			if (numberOfChildren() <= 0)
			{
				return null;
			}
			return m_Array[m_iId + 1];
		}

		public XbnNode nextSibling()
		{
			return m_Array[m_iId + (numberOfFamily_ + 1)];
		}

		public bool getNodesByTagNameFromChildren(string tag_name, XbnNodeList list)
		{
			if (numberOfChildren() <= 0)
			{
				return false;
			}
			if (list.empty())
			{
				XbnNode xbnNode = firstChild();
				for (int i = 0; i < numberOfChildren(); i++)
				{
					if (strcmp(xbnNode.nodeName(), tag_name) == 0)
					{
						if (list.size() >= LIMIT_OF_NODELIST)
						{
							list.prevSearchCursor = i;
							return true;
						}
						list.push_back(xbnNode);
					}
					xbnNode = xbnNode.nextSibling();
				}
			}
			else
			{
				XbnNode xbnNode2 = firstChild();
				int j;
				for (j = 0; j < list.prevSearchCursor; j++)
				{
					xbnNode2 = xbnNode2.nextSibling();
				}
				j = list.prevSearchCursor;
				list.clear();
				for (; j < numberOfChildren(); j++)
				{
					if (strcmp(xbnNode2.nodeName(), tag_name) == 0)
					{
						if (list.size() >= LIMIT_OF_NODELIST)
						{
							list.prevSearchCursor = j;
							return true;
						}
						list.push_back(xbnNode2);
					}
					xbnNode2 = xbnNode2.nextSibling();
				}
			}
			return false;
		}

		public bool getNodesByTagName(string tag_name, XbnNodeList list)
		{
			if (numberOfChildren() <= 0)
			{
				return false;
			}
			if (list.empty())
			{
				for (int i = 0; i < numberOfFamily(); i++)
				{
					XbnNode xbnNode = firstChild();
					xbnNode = xbnNode.m_Array[xbnNode.m_iId + i];
					if (strcmp(xbnNode.nodeName(), tag_name) == 0)
					{
						if (list.size() >= LIMIT_OF_NODELIST)
						{
							list.prevSearchCursor = i;
							return true;
						}
						list.push_back(xbnNode);
					}
				}
			}
			else
			{
				int j = list.prevSearchCursor;
				list.clear();
				for (; j < numberOfChildren(); j++)
				{
					XbnNode xbnNode2 = firstChild();
					xbnNode2 = xbnNode2.m_Array[xbnNode2.m_iId + j];
					if (strcmp(xbnNode2.nodeName(), tag_name) == 0)
					{
						if (list.size() >= LIMIT_OF_NODELIST)
						{
							list.prevSearchCursor = j;
							return true;
						}
						list.push_back(xbnNode2);
					}
				}
			}
			return false;
		}

		public XbnNode getFirstNodeByTagNameFromChildren(string tag_name)
		{
			if (numberOfChildren() <= 0)
			{
				return null;
			}
			XbnNode xbnNode = firstChild();
			for (int i = 0; i < numberOfChildren(); i++)
			{
				if (strcmp(xbnNode.nodeName(), tag_name) == 0)
				{
					return xbnNode;
				}
				xbnNode = xbnNode.nextSibling();
			}
			return null;
		}

		public XbnNode getFirstNodeByTagName(string tag_name)
		{
			if (numberOfFamily() <= 0)
			{
				return null;
			}
			for (int i = 0; i < numberOfFamily(); i++)
			{
				XbnNode xbnNode = firstChild();
				xbnNode = xbnNode.m_Array[xbnNode.m_iId + i];
				if (strcmp(xbnNode.nodeName(), tag_name) == 0)
				{
					return xbnNode;
				}
			}
			return null;
		}

		public int countNodesByTagName(string tag_name)
		{
			int num = 0;
			string text = null;
			if (numberOfFamily() <= 0)
			{
				return 0;
			}
			XbnNode firstNodeByTagName = getFirstNodeByTagName(tag_name);
			if (firstNodeByTagName != null)
			{
				text = firstNodeByTagName.nodeName();
			}
			for (int i = 0; i < numberOfFamily(); i++)
			{
				XbnNode xbnNode = firstChild();
				xbnNode = xbnNode.m_Array[xbnNode.m_iId + i];
				if (xbnNode.nodeName() == text)
				{
					num++;
				}
			}
			return num;
		}

		public string nodeName()
		{
			return nodeName_;
		}

		public DATA_TYPE dataType()
		{
			return dataType_;
		}

		public int numberOfChildren()
		{
			return numberOfChildren_;
		}

		public int numberOfFamily()
		{
			return numberOfFamily_;
		}

		public void parse1(ArrayReader reader)
		{
			m_iOffset = reader.readInt32();
			dataType_ = (DATA_TYPE)reader.readInt32();
			nodeData_ = reader.readInt32();
			numberOfChildren_ = reader.readInt32();
			numberOfFamily_ = reader.readInt32();
		}

		public void parse2(ArrayReader reader)
		{
			long position = reader.getPosition();
			reader.skip(m_iOffset);
			nodeName_ = reader.readString("SJIS");
			reader.setPosition(position);
			if (dataType_ == DATA_TYPE.DT_STRING)
			{
				reader.skip(nodeData_);
				m_strData = reader.readString("SJIS");
				reader.setPosition(position);
			}
		}
	}
}
