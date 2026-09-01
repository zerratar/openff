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
		public class VramSendInfo
		{
			private uint _unAddress;

			private uint _unSize;

			private uint _unAlign;

			private bool _bCompress;

			private SLNode<VramSendInfo> _node;

			public VramSendInfo()
			{
				_node.setData(this);
			}

			public void destruct()
			{
			}

			public uint address()
			{
				return _unAddress;
			}

			public uint size()
			{
				return _unSize;
			}

			public uint tale()
			{
				return _unAddress + _unAlign;
			}

			public void set(uint addr, uint size, uint sizeAlign, bool comp)
			{
				_unAddress = addr;
				_unSize = size;
				_unAlign = sizeAlign;
				_bCompress = comp;
			}

			public SLNode<VramSendInfo> node()
			{
				return _node;
			}
		}
	}
}
