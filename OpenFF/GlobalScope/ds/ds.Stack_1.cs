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
		public class Stack<value_type>
		{
			public int MaxNumElements;

			private value_type[] elements_;

			private int pointer_;

			public Stack(int NElem)
			{
				MaxNumElements = NElem;
				elements_ = new value_type[MaxNumElements];
				pointer_ = 0;
			}

			public void pop()
			{
				pointer_--;
			}

			public void push(value_type val)
			{
				elements_[pointer_++] = val;
			}

			public int empty()
			{
				if (pointer_ != 0)
				{
					return 0;
				}
				return 1;
			}

			public int size()
			{
				return pointer_;
			}

			public value_type top()
			{
				return elements_[pointer_ - 1];
			}
		}
	}
}
