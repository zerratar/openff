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
	private static class Operator<T, U>
	{
		private static readonly ParameterExpression arg0 = Expression.Parameter(typeof(T), "arg0");

		private static readonly ParameterExpression arg1 = Expression.Parameter(typeof(U), "arg1");

		public static readonly Func<T, U, T> ADD = Lambda(Expression.Add);

		public static readonly Func<T, U, T> SUB = Lambda(Expression.Subtract);

		public static readonly Func<T, U, T> MUL = Lambda(Expression.Multiply);

		public static readonly Func<T, U, T> DIV = Lambda(Expression.Divide);

		public static readonly Func<T, U, T> SHL = Lambda(Expression.LeftShift);

		public static readonly Func<T, U, T> SHR = Lambda(Expression.RightShift);

		public static readonly Func<T, U, T> AND = Lambda(Expression.And);

		public static readonly Func<T, U, T> OR = Lambda(Expression.Or);

		public static readonly Func<T, T> NOT = Lambda(Expression.Not);

		public static readonly Func<T, T> NEG = Lambda(Expression.Negate);

		public static readonly Func<T, U, T> G = LambdaCond(Expression.GreaterThan);

		public static readonly Func<T, U, T> GE = LambdaCond(Expression.GreaterThanOrEqual);

		public static readonly Func<T, U, T> E = LambdaCond(Expression.Equal);

		public static readonly Func<T, U, T> LE = LambdaCond(Expression.LessThanOrEqual);

		public static readonly Func<T, U, T> L = LambdaCond(Expression.LessThan);

		public static readonly Func<T, U, T> NE = LambdaCond(Expression.NotEqual);

		public static Func<T, U, T> Lambda(Func<ParameterExpression, ParameterExpression, BinaryExpression> op)
		{
			return Expression.Lambda<Func<T, U, T>>(op(arg0, arg1), new ParameterExpression[2] { arg0, arg1 }).Compile();
		}

		public static Func<T, T> Lambda(Func<ParameterExpression, UnaryExpression> op)
		{
			return Expression.Lambda<Func<T, T>>(op(arg0), new ParameterExpression[1] { arg0 }).Compile();
		}

		public static Func<T, U, T> LambdaCond(Func<ParameterExpression, ParameterExpression, BinaryExpression> op)
		{
			ConditionalExpression body = Expression.Condition(op(arg0, arg1), arg0, arg1);
			Expression<Func<T, U, T>> expression = Expression.Lambda<Func<T, U, T>>(body, new ParameterExpression[2] { arg0, arg1 });
			return expression.Compile();
		}
	}
}
