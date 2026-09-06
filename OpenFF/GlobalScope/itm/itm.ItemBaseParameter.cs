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
	public static partial class itm
	{
		public class ItemBaseParameter
		{
			protected byte system_;

			protected byte _pad0;

			protected short itemId_;

			protected short nameId_;

			protected short captionId_;

			protected short graphId_;

			protected byte strength_;

			protected byte vitality_;

			protected byte dexterity_;

			protected byte intellect_;

			protected byte mind_;

			protected byte weight_;

			protected byte useBattle_;

			protected byte useField_;

			protected byte allTarget_;

			protected byte _pad1;

			protected short useItemId_;

			protected short targetPossible_;

			protected short targetPosition_;

			protected byte _pad2;

			protected byte _pad3;

			public byte system()
			{
				return system_;
			}

			public short itemId()
			{
				return itemId_;
			}

			public short nameId()
			{
				return nameId_;
			}

			public short captionId()
			{
				return captionId_;
			}

			public short graphId()
			{
				return graphId_;
			}

			public byte strength()
			{
				return strength_;
			}

			public byte vitality()
			{
				return vitality_;
			}

			public byte dexterity()
			{
				return dexterity_;
			}

			public byte intellect()
			{
				return intellect_;
			}

			public byte mind()
			{
				return mind_;
			}

			public byte weight()
			{
				return weight_;
			}

			public byte useBattle()
			{
				return useBattle_;
			}

			public byte useField()
			{
				return useField_;
			}

			public byte allTarget()
			{
				return allTarget_;
			}

			public short useItemId()
			{
				return useItemId_;
			}

			public short targetPossible()
			{
				return targetPossible_;
			}

			public short targetPosition()
			{
				return targetPosition_;
			}
		}
	}
}
