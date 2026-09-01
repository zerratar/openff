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
							public class SHoldCharacterData
							{
								public VecFx32 m_Position = new VecFx32();

								public VecFx32 m_Rotation = new VecFx32();

								public void initialize()
								{
									VEC_Set(m_Position, 0, 0, 0);
									VEC_Set(m_Rotation, 0, 0, 0);
								}

								public void parse(ArrayReader reader)
								{
									m_Position.parse(reader);
									m_Rotation.parse(reader);
								}

								public void store(ArrayWriter writer)
								{
									m_Position.store(writer);
									m_Rotation.store(writer);
								}
							}
	}
}
