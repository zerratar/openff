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
	public static partial class eld
	{
		public class SEQ_BOOT_EFFECT
		{
			public uint command;

			public uint category;

			public uint member;

			public float[] pos = new float[3];

			public float[] figure = new float[3];

			public uint _id;

			public uint uiEmitterID;

			public ushort path_id;

			public byte flag_coord;

			public byte layer;

			public static explicit operator SEQ_BOOT_EFFECT(ArrayReader src)
			{
				SEQ_BOOT_EFFECT sEQ_BOOT_EFFECT = new SEQ_BOOT_EFFECT();
				sEQ_BOOT_EFFECT.command = src.readUInt32();
				sEQ_BOOT_EFFECT.category = src.readUInt32();
				sEQ_BOOT_EFFECT.member = src.readUInt32();
				sEQ_BOOT_EFFECT.pos[0] = src.readSingle();
				sEQ_BOOT_EFFECT.pos[1] = src.readSingle();
				sEQ_BOOT_EFFECT.pos[2] = src.readSingle();
				sEQ_BOOT_EFFECT.figure[0] = src.readSingle();
				sEQ_BOOT_EFFECT.figure[1] = src.readSingle();
				sEQ_BOOT_EFFECT.figure[2] = src.readSingle();
				sEQ_BOOT_EFFECT._id = src.readUInt32();
				sEQ_BOOT_EFFECT.uiEmitterID = src.readUInt32();
				sEQ_BOOT_EFFECT.path_id = src.readUInt16();
				sEQ_BOOT_EFFECT.flag_coord = src.readByte();
				sEQ_BOOT_EFFECT.layer = src.readByte();
				return sEQ_BOOT_EFFECT;
			}
		}
	}
}
