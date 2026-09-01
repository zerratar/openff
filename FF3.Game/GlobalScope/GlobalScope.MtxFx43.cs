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
	public class MtxFx43
	{
		public int[] a = new int[12];

		public int _00
		{
			get
			{
				return a[0];
			}
			set
			{
				a[0] = value;
			}
		}

		public int _01
		{
			get
			{
				return a[1];
			}
			set
			{
				a[1] = value;
			}
		}

		public int _02
		{
			get
			{
				return a[2];
			}
			set
			{
				a[2] = value;
			}
		}

		public int _10
		{
			get
			{
				return a[3];
			}
			set
			{
				a[3] = value;
			}
		}

		public int _11
		{
			get
			{
				return a[4];
			}
			set
			{
				a[4] = value;
			}
		}

		public int _12
		{
			get
			{
				return a[5];
			}
			set
			{
				a[5] = value;
			}
		}

		public int _20
		{
			get
			{
				return a[6];
			}
			set
			{
				a[6] = value;
			}
		}

		public int _21
		{
			get
			{
				return a[7];
			}
			set
			{
				a[7] = value;
			}
		}

		public int _22
		{
			get
			{
				return a[8];
			}
			set
			{
				a[8] = value;
			}
		}

		public int _30
		{
			get
			{
				return a[9];
			}
			set
			{
				a[9] = value;
			}
		}

		public int _31
		{
			get
			{
				return a[10];
			}
			set
			{
				a[10] = value;
			}
		}

		public int _32
		{
			get
			{
				return a[11];
			}
			set
			{
				a[11] = value;
			}
		}

		public static explicit operator MtxFx43(ArrayReader src)
		{
			MtxFx43 mtxFx = new MtxFx43();
			src.read(mtxFx.a, 0, 12);
			return mtxFx;
		}

		public void copy(MtxFx43 src)
		{
			a[0] = src.a[0];
			a[1] = src.a[1];
			a[2] = src.a[2];
			a[3] = src.a[3];
			a[4] = src.a[4];
			a[5] = src.a[5];
			a[6] = src.a[6];
			a[7] = src.a[7];
			a[8] = src.a[8];
			a[9] = src.a[9];
			a[10] = src.a[10];
			a[11] = src.a[11];
		}

		public void copy(MtxFx33 src)
		{
			a[0] = src._00;
			a[1] = src._01;
			a[2] = src._02;
			a[3] = src._10;
			a[4] = src._11;
			a[5] = src._12;
			a[6] = src._20;
			a[7] = src._21;
			a[8] = src._22;
		}

		public MtxFx43()
		{
		}

		public MtxFx43(MtxFx43 src)
		{
			copy(src);
		}

		public MtxFx43(int arg0, int arg1, int arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8, int arg9, int arg10, int arg11)
		{
			a[0] = arg0;
			a[1] = arg1;
			a[2] = arg2;
			a[3] = arg3;
			a[4] = arg4;
			a[5] = arg5;
			a[6] = arg6;
			a[7] = arg7;
			a[8] = arg8;
			a[9] = arg9;
			a[10] = arg10;
			a[11] = arg11;
		}

		public void setDefault()
		{
			a[0] = 0;
			a[1] = 0;
			a[2] = 0;
			a[3] = 0;
			a[4] = 0;
			a[5] = 0;
			a[6] = 0;
			a[7] = 0;
			a[8] = 0;
			a[9] = 0;
			a[10] = 0;
			a[11] = 0;
		}
	}
}
