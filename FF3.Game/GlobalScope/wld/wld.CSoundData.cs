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
							public class CSoundData
							{
								public enum SOUND_FLAG
								{
									MAP_JUMP_IN_SE = 1,
									MAP_JUMP_OUT_SE
								}

								public const SOUND_FLAG MAP_JUMP_IN_SE = SOUND_FLAG.MAP_JUMP_IN_SE;

								public const SOUND_FLAG MAP_JUMP_OUT_SE = SOUND_FLAG.MAP_JUMP_OUT_SE;

								private int m_SoundFlag;

								public void initialize()
								{
									m_SoundFlag |= 1;
									m_SoundFlag |= 2;
								}

								public void setSoundFlag(int flag)
								{
									m_SoundFlag = flag;
								}

								public int getSoundFlag()
								{
									return m_SoundFlag;
								}

								public void setDefault()
								{
									m_SoundFlag = 0;
								}

								public void parse(ArrayReader reader)
								{
									m_SoundFlag = reader.readInt32();
								}

								public void store(ArrayWriter writer)
								{
									writer.writeInt32(m_SoundFlag);
								}
							}
	}
}
