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
	public static partial class mognet
	{
		public class MNMail
		{
			public static int TEXT_LENGTH = 160;

			protected int profile_id_;

			protected int appendage_;

			protected int appendage2_;

			protected int flag_;

			protected string name_;

			protected string text_;

			private sbyte[] reserve = new sbyte[46];

			public MNMail()
			{
				clear();
			}

			public void clear()
			{
				profile_id_ = 0;
				appendage_ = -1;
				flag_ = 0;
				name_ = "";
				text_ = "";
			}

			public void setName(string n)
			{
				strncpy(out name_, n, 32);
			}

			public void setText(string str)
			{
				text_ = "";
				strncpy(out text_, str, TEXT_LENGTH);
			}

			public bool append(int number)
			{
				appendage_ = number;
				return true;
			}

			public bool append2(int number)
			{
				appendage2_ = number;
				return true;
			}

			public int profileID()
			{
				return profile_id_;
			}

			public void setProfileID(int _id)
			{
				profile_id_ = _id;
			}

			public string name()
			{
				return name_;
			}

			public void name_set(string arg0)
			{
				name_ = arg0;
			}

			public string text()
			{
				return text_;
			}

			public int appendage()
			{
				return appendage_;
			}

			public int appendage2()
			{
				return appendage2_;
			}

			public void setFlag(ushort f)
			{
				flag_ |= f;
			}

			public void clearFlag(ushort f)
			{
				flag_ &= ~f;
			}

			public bool checkFlag(MNMAIL_FLAGS f)
			{
				return ((uint)flag_ & (uint)f) != 0;
			}
		}
	}
}
