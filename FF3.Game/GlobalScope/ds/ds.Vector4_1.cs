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
	public static partial class ds
	{
		public class Vector4<V>
		{
			private V[] v = new V[4];

			public V this[int i]
			{
				get
				{
					return v[i];
				}
				set
				{
					v[i] = value;
				}
			}

			public V vx
			{
				get
				{
					return v[0];
				}
				set
				{
					v[0] = value;
				}
			}

			public V vy
			{
				get
				{
					return v[1];
				}
				set
				{
					v[1] = value;
				}
			}

			public V vz
			{
				get
				{
					return v[2];
				}
				set
				{
					v[2] = value;
				}
			}

			public V vw
			{
				get
				{
					return v[3];
				}
				set
				{
					v[3] = value;
				}
			}

			public V cr
			{
				get
				{
					return v[0];
				}
				set
				{
					v[0] = value;
				}
			}

			public V cg
			{
				get
				{
					return v[1];
				}
				set
				{
					v[1] = value;
				}
			}

			public V cb
			{
				get
				{
					return v[2];
				}
				set
				{
					v[2] = value;
				}
			}

			public V ca
			{
				get
				{
					return v[3];
				}
				set
				{
					v[3] = value;
				}
			}

			public Vector4()
			{
			}

			public Vector4(Vector4<V> src)
			{
				copy(src);
			}

			public Vector4(V arg0, V arg1, V arg2, V arg3)
			{
				set(arg0, arg1, arg2, arg3);
			}

			public void copy(Vector4<V> src)
			{
				vx = src.vx;
				vy = src.vy;
				vz = src.vz;
				vw = src.vw;
			}

			public Vector4<V> zero()
			{
				V val = (vw = default(V));
				V val3 = (vz = val);
				V val5 = (vy = val3);
				vx = val5;
				return this;
			}

			public Vector4<V> set(V x, V y, V z, V w)
			{
				vx = x;
				vy = y;
				vz = z;
				vw = w;
				return this;
			}

			public Vector4<V> set(V s)
			{
				set(s, s, s, s);
				return this;
			}

			public Vector4<V> add(V x, V y, V z, V w)
			{
				vx = _add(vx, x);
				vy = _add(vy, y);
				vz = _add(vz, z);
				vw = _add(vw, w);
				return this;
			}

			public static Vector4<V> operator +(Vector4<V> self, V s)
			{
				return self.add(s, s, s, s);
			}

			public static Vector4<V> operator +(Vector4<V> self, Vector4<V> v)
			{
				return self.add(v.vx, v.vy, v.vz, v.vw);
			}

			public Vector4<V> sub(V x, V y, V z, V w)
			{
				vx = _sub(vx, x);
				vy = _sub(vy, y);
				vz = _sub(vz, z);
				vw = _sub(vw, w);
				return this;
			}

			public static Vector4<V> operator -(Vector4<V> self, V s)
			{
				return self.sub(s, s, s, s);
			}

			public static Vector4<V> operator -(Vector4<V> self, Vector4<V> v)
			{
				return self.sub(v.vx, v.vy, v.vz, v.vw);
			}

			public Vector4<V> mul(V x, V y, V z, V w)
			{
				vx = _mul(vx, x);
				vy = _mul(vy, y);
				vz = _mul(vz, z);
				vw = _mul(vw, w);
				return this;
			}

			public static Vector4<V> operator *(Vector4<V> self, V s)
			{
				return self.mul(s, s, s, s);
			}

			public static Vector4<V> operator *(Vector4<V> self, Vector4<V> v)
			{
				return self.mul(v.vx, v.vy, v.vz, v.vw);
			}

			public Vector4<V> div(V x, V y, V z, V w)
			{
				vx = _div(vx, x);
				vy = _div(vy, y);
				vz = _div(vz, z);
				return this;
			}

			public static Vector4<V> operator /(Vector4<V> self, V s)
			{
				return self.div(s, s, s, s);
			}

			public static Vector4<V> operator /(Vector4<V> self, Vector4<V> v)
			{
				return self.div(v.vx, v.vy, v.vz, v.vw);
			}

			public Vector4<V> abs()
			{
				vx = ds.abs(vx);
				vy = ds.abs(vy);
				vz = ds.abs(vz);
				vw = ds.abs(vw);
				return this;
			}

			public V min()
			{
				return ds.min(vx, ds.min(vy, ds.min(vz, vw)));
			}

			public V max()
			{
				return ds.max(vx, ds.max(vy, ds.max(vz, vw)));
			}

			public Vector4<V> min(V v)
			{
				vx = ds.min(vx, v);
				vy = ds.min(vy, v);
				vz = ds.min(vz, v);
				vw = ds.min(vw, v);
				return this;
			}

			public Vector4<V> max(V v)
			{
				vx = ds.max(vx, v);
				vy = ds.max(vy, v);
				vz = ds.max(vz, v);
				vw = ds.max(vw, v);
				return this;
			}

			public Vector4<V> clamp(V vmin, V vmax)
			{
				vx = ds.clamp(vx, vmin, vmax);
				vy = ds.clamp(vy, vmin, vmax);
				vz = ds.clamp(vz, vmin, vmax);
				vw = ds.clamp(vw, vmin, vmax);
				return this;
			}

			public Vector4<V> neg()
			{
				vx = _neg(vx);
				vy = _neg(vy);
				vz = _neg(vz);
				vw = _neg(vw);
				return this;
			}
		}
	}
}
