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
	public enum MICompressionType
	{
		MI_COMPRESSION_LZ = 16,
		MI_COMPRESSION_HUFFMAN = 32,
		MI_COMPRESSION_RL = 48,
		MI_COMPRESSION_DIFF = 128,
		MI_COMPRESSION_TYPE_MASK = 240,
		MI_COMPRESSION_TYPE_EX_MASK = 255
	}
}
