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
	public static partial class map
	{
							public class SecretWayParameter
							{
								public string name;

								public byte offAlpha;

								public byte onAlpha;

								public ushort offFrame;

								public ushort onFrame;

								public short[] maxPos = new short[3];

								public short[] minPos = new short[3];

								public static explicit operator SecretWayParameter(ArrayReader src)
								{
									SecretWayParameter secretWayParameter = new SecretWayParameter();
									byte[] array = new byte[16];
									src.read(array, 0, array.Length);
									secretWayParameter.name = StringUtil.createString(array);
									secretWayParameter.offAlpha = src.readByte();
									secretWayParameter.onAlpha = src.readByte();
									secretWayParameter.offFrame = src.readUInt16();
									secretWayParameter.onFrame = src.readUInt16();
									src.read(secretWayParameter.maxPos, 0, 3);
									src.read(secretWayParameter.minPos, 0, 3);
									return secretWayParameter;
								}
							}
	}
}
