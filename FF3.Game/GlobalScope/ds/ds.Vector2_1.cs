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
	public static partial class ds
	{
		public class Vector2<V>
		{
			public V vx;

			public V vy;

			public V w
			{
				get
				{
					return vx;
				}
				set
				{
					vx = value;
				}
			}

			public V h
			{
				get
				{
					return vy;
				}
				set
				{
					vy = value;
				}
			}

			public V s
			{
				get
				{
					return vx;
				}
				set
				{
					vx = value;
				}
			}

			public V t
			{
				get
				{
					return vy;
				}
				set
				{
					vy = value;
				}
			}

			public Vector2()
			{
			}

			public Vector2(V arg0, V arg1)
			{
				set(arg0, arg1);
			}

			public Vector2(Vector2<V> src)
			{
				copy(src);
			}

			public void copy(Vector2<V> src)
			{
				vx = src.vx;
				vy = src.vy;
			}

			public Vector2<V> zero()
			{
				vx = (vy = (vy = default(V)));
				return this;
			}

			public Vector2<V> set(V x, V y)
			{
				vx = x;
				vy = y;
				return this;
			}

			public Vector2<V> set(V s)
			{
				set(s, s);
				return this;
			}

			public Vector2<V> add(V x, V y)
			{
				vx = _add(vx, x);
				vy = _add(vy, y);
				return this;
			}

			public static Vector2<V> operator +(Vector2<V> self, V s)
			{
				return self.add(s, s);
			}

			public static Vector2<V> operator +(Vector2<V> self, Vector2<V> v)
			{
				return self.add(v.vx, v.vy);
			}

			public Vector2<V> sub(V x, V y)
			{
				vx = _sub(vx, x);
				vy = _sub(vy, y);
				return this;
			}

			public static Vector2<V> operator -(Vector2<V> self, V s)
			{
				return self.sub(s, s);
			}

			public static Vector2<V> operator -(Vector2<V> self, Vector2<V> v)
			{
				return self.sub(v.vx, v.vy);
			}

			public Vector2<V> mul(V x, V y)
			{
				vx = _mul(vx, x);
				vy = _mul(vy, y);
				return this;
			}

			public static Vector2<V> operator *(Vector2<V> self, V s)
			{
				return self.mul(s, s);
			}

			public static Vector2<V> operator *(Vector2<V> self, Vector2<V> v)
			{
				return self.mul(v.vx, v.vy);
			}

			public Vector2<V> div(V x, V y)
			{
				vx = _div(vx, x);
				vy = _div(vy, y);
				return this;
			}

			public static Vector2<V> operator /(Vector2<V> self, V s)
			{
				return self.div(s, s);
			}

			public static Vector2<V> operator /(Vector2<V> self, Vector2<V> v)
			{
				return self.div(v.vx, v.vy);
			}

			public Vector2<V> abs()
			{
				vx = ds.abs(vx);
				vy = ds.abs(vy);
				return this;
			}

			public V min()
			{
				return ds.min(vx, vy);
			}

			public V max()
			{
				return ds.max(vx, vy);
			}

			public Vector2<V> min(V v)
			{
				vx = ds.min(vx, v);
				vy = ds.min(vy, v);
				return this;
			}

			public Vector2<V> max(V v)
			{
				vx = ds.max(vx, v);
				vy = ds.max(vy, v);
				return this;
			}

			public Vector2<V> clamp(V vmin, V vmax)
			{
				vx = ds.clamp(vx, vmin, vmax);
				vy = ds.clamp(vy, vmin, vmax);
				return this;
			}

			public Vector2<V> neg()
			{
				vx = _neg(vx);
				vy = _neg(vy);
				return this;
			}
		}
	}
}
