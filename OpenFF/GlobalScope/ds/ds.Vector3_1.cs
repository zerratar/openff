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
		public class Vector3<V>
		{
			public V[] v = new V[3];

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

			public Vector3()
			{
			}

			public Vector3(Vector3<V> src)
			{
				copy(src);
			}

			public Vector3(V arg0, V arg1, V arg2)
			{
				set(arg0, arg1, arg2);
			}

			public void copy(Vector3<V> src)
			{
				vx = src.vx;
				vy = src.vy;
				vz = src.vz;
			}

			public Vector3<V> zero()
			{
				V val = (vz = default(V));
				V val3 = (vy = val);
				vx = val3;
				return this;
			}

			public Vector3<V> set(V x, V y, V z)
			{
				vx = x;
				vy = y;
				vz = z;
				return this;
			}

			public Vector3<V> set(V s)
			{
				set(s, s, s);
				return this;
			}

			public Vector3<V> add(V x, V y, V z)
			{
				vx = _add(vx, x);
				vy = _add(vy, y);
				vz = _add(vz, z);
				return this;
			}

			public static Vector3<V> operator +(Vector3<V> self, V s)
			{
				return self.add(s, s, s);
			}

			public static Vector3<V> operator +(Vector3<V> self, Vector3<V> v)
			{
				return self.add(v.vx, v.vy, v.vz);
			}

			public Vector3<V> sub(V x, V y, V z)
			{
				vx = _sub(vx, x);
				vy = _sub(vy, y);
				vz = _sub(vz, z);
				return this;
			}

			public static Vector3<V> operator -(Vector3<V> self, V s)
			{
				return self.sub(s, s, s);
			}

			public static Vector3<V> operator -(Vector3<V> self, Vector3<V> v)
			{
				return self.sub(v.vx, v.vy, v.vz);
			}

			public Vector3<V> mul(V x, V y, V z)
			{
				vx = _mul(vx, x);
				vy = _mul(vy, y);
				vz = _mul(vz, z);
				return this;
			}

			public static Vector3<V> operator *(Vector3<V> self, V s)
			{
				return self.mul(s, s, s);
			}

			public static Vector3<V> operator *(Vector3<V> self, Vector3<V> v)
			{
				return self.mul(v.vx, v.vy, v.vz);
			}

			public Vector3<V> div(V x, V y, V z)
			{
				vx = _div(vx, x);
				vy = _div(vy, y);
				vz = _div(vz, z);
				return this;
			}

			public static Vector3<V> operator /(Vector3<V> self, V s)
			{
				return self.div(s, s, s);
			}

			public static Vector3<V> operator /(Vector3<V> self, Vector3<V> v)
			{
				return self.div(v.vx, v.vy, v.vz);
			}

			public Vector3<V> abs()
			{
				vx = ds.abs(vx);
				vy = ds.abs(vy);
				vz = ds.abs(vz);
				return this;
			}

			public V min()
			{
				return ds.min(vx, ds.min(vy, vz));
			}

			public V max()
			{
				return ds.max(vx, ds.max(vy, vz));
			}

			public Vector3<V> min(V v)
			{
				vx = ds.min(vx, v);
				vy = ds.min(vy, v);
				vz = ds.min(vz, v);
				return this;
			}

			public Vector3<V> max(V v)
			{
				vx = ds.max(vx, v);
				vy = ds.max(vy, v);
				vz = ds.max(vz, v);
				return this;
			}

			public Vector3<V> clamp(V vmin, V vmax)
			{
				vx = ds.clamp(vx, vmin, vmax);
				vy = ds.clamp(vy, vmin, vmax);
				vz = ds.clamp(vz, vmin, vmax);
				return this;
			}

			public Vector3<V> neg()
			{
				vx = _neg(vx);
				vy = _neg(vy);
				vz = _neg(vz);
				return this;
			}
		}
	}
}
