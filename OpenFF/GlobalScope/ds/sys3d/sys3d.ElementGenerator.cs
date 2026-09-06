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
		public static partial class sys3d
		{
			public class ElementGenerator
			{
				public static T newElement<T>(SceneObject objScn)
				{
					return (T)CHeap.alloc_app(typeof(T));
				}

				public static void deleteElement<T>(T elem)
				{
					if (elem != null)
					{
						elem.GetType().GetMethod("destruct").Invoke(elem, m_DummyObject);
						CHeap.free_app(elem);
					}
				}
			}
		}
	}
}
