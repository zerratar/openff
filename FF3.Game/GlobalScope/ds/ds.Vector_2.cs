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
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
	public static partial class ds
	{
		public class Vector<value_type, ErasePolicy> where ErasePolicy : new()
		{
			public int MaxNumElements;

			private value_type[] elements_;

			private int numElements_;

			public value_type this[int pos]
			{
				get
				{
					return at(pos);
				}
				set
				{
					elements_[pos] = value;
				}
			}

			public Vector(int NElem)
			{
				MaxNumElements = NElem;
				elements_ = new value_type[MaxNumElements];
				numElements_ = 0;
			}

			public void insert(int pos, value_type elem)
			{
				if (pos < numElements_)
				{
					if (pos < 0)
					{
						pos = 0;
					}
					if (numElements_ != 0)
					{
						int num = numElements_ - 1;
						while (pos <= num)
						{
							elements_[num + 1] = elements_[num];
							num--;
						}
					}
					elements_[pos] = elem;
				}
				else
				{
					elements_[numElements_] = elem;
				}
				numElements_++;
			}

			public void push_back(value_type elem)
			{
				elements_[numElements_++] = elem;
			}

			public void erase(int pos)
			{
				if (pos < numElements_)
				{
					typeof(ErasePolicy).GetMethod("erase").Invoke(new ErasePolicy(), new object[3] { pos, elements_, numElements_ });
					numElements_--;
				}
			}

			public virtual void clear()
			{
				numElements_ = 0;
				for (int i = 0; i < MaxNumElements; i++)
				{
					elements_[i] = default(value_type);
				}
			}

			public value_type at(int pos)
			{
				return elements_[pos];
			}

			public int size()
			{
				return numElements_;
			}

			public bool empty()
			{
				return numElements_ == 0;
			}

			public value_type begin()
			{
				return elements_[0];
			}

			public value_type end()
			{
				return elements_[numElements_];
			}

			public void copy(Vector<value_type, ErasePolicy> src)
			{
				for (int i = 0; i < src.elements_.Length; i++)
				{
					elements_[i] = src.elements_[i];
				}
				numElements_ = src.numElements_;
			}
		}
	}
}
