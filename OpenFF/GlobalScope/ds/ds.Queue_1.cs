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
		public class Queue<value_type>
		{
			public int MaxNumElements;

			private value_type[] elements_;

			private int front_;

			private int back_;

			public Queue(int NElem)
			{
				MaxNumElements = NElem;
				elements_ = new value_type[MaxNumElements + 1];
				front_ = 0;
				back_ = 0;
			}

			public void pop()
			{
				if (front_ < MaxNumElements)
				{
					front_++;
				}
				else
				{
					front_ = 0;
				}
			}

			public void push(value_type val)
			{
				elements_[back_++] = val;
				if (MaxNumElements < back_)
				{
					back_ = 0;
				}
			}

			public int empty()
			{
				if (back_ != front_)
				{
					return 0;
				}
				return 1;
			}

			public int size()
			{
				if (front_ > back_)
				{
					return MaxNumElements - front_ + back_ + 1;
				}
				return back_ - front_;
			}

			public value_type front()
			{
				return elements_[front_];
			}

			public value_type back()
			{
				if (0 >= back_)
				{
					return elements_[MaxNumElements];
				}
				return elements_[back_ - 1];
			}
		}
	}
}
