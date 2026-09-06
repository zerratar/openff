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
	internal class HostEnv
	{
		public sbyte[] GetByteArrayElements(Array array, byte[] isCopy)
		{
			return (sbyte[])array;
		}

		public int[] GetIntArrayElements(Array array, byte[] isCopy)
		{
			return (int[])array;
		}

		public int GetArrayLength(Array array)
		{
			return array.Length;
		}

		public void ReleaseByteArrayElements(Array array, sbyte[] elems, int mode)
		{
		}

		public void ReleaseIntArrayElements(Array array, int[] elems, int mode)
		{
		}
	}
}
