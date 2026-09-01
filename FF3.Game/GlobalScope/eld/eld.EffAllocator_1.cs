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
		public class EffAllocator<T> where T : new()
		{
			public T[] allocateMemoryArry(uint nbelem)
			{
				T[] array = new T[nbelem];
				for (uint num = 0u; num < nbelem; num++)
				{
					array[num] = new T();
				}
				return array;
			}

			public void deallocateMemoryArry(T[] ptr)
			{
				uint num = (uint)ptr.Length;
				if (!ptr[0].GetType().IsValueType)
				{
					for (uint num2 = 0u; num2 < num; num2++)
					{
						ptr[num2].GetType().GetMethod("destruct").Invoke(ptr[num2], m_DummyObject);
					}
				}
			}

			public T allocate(uint nbelem)
			{
				return new T();
			}

			public void deallocate(T ptr)
			{
				ptr.GetType().GetMethod("destruct").Invoke(ptr, m_DummyObject);
			}
		}
	}
}
