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
	public static partial class wld
	{
							public class SHoldDoorData
							{
								public string m_MapName = "";

								public bool m_IsOpen;

								public sbyte m_MaterialIndex;

								public void initialize()
								{
									m_MapName = "";
									m_IsOpen = false;
									m_MaterialIndex = -1;
								}

								public void setDefault()
								{
									m_MapName = "";
									m_IsOpen = false;
									m_MaterialIndex = 0;
								}

								public void parse(ArrayReader reader)
								{
									byte[] array = new byte[16];
									reader.read(array, 0, 16);
									m_MapName = StringUtil.createString(array);
									m_IsOpen = reader.readByte() != 0;
									m_MaterialIndex = reader.readSByte();
								}

								public void store(ArrayWriter writer)
								{
									byte[] array = new byte[16];
									byte[] bytes = StringUtil.getBytes(m_MapName);
									memcpy(array, bytes, bytes.Length);
									writer.write(array, 0, 16);
									writer.writeByte((byte)(m_IsOpen ? 1u : 0u));
									writer.writeSByte(m_MaterialIndex);
								}
							}
	}
}
