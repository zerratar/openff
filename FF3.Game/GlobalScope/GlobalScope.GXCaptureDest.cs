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
	public enum GXCaptureDest
	{
		GX_CAPTURE_DEST_VRAM_A_0x00000,
		GX_CAPTURE_DEST_VRAM_B_0x00000,
		GX_CAPTURE_DEST_VRAM_C_0x00000,
		GX_CAPTURE_DEST_VRAM_D_0x00000,
		GX_CAPTURE_DEST_VRAM_A_0x08000,
		GX_CAPTURE_DEST_VRAM_B_0x08000,
		GX_CAPTURE_DEST_VRAM_C_0x08000,
		GX_CAPTURE_DEST_VRAM_D_0x08000,
		GX_CAPTURE_DEST_VRAM_A_0x10000,
		GX_CAPTURE_DEST_VRAM_B_0x10000,
		GX_CAPTURE_DEST_VRAM_C_0x10000,
		GX_CAPTURE_DEST_VRAM_D_0x10000,
		GX_CAPTURE_DEST_VRAM_A_0x18000,
		GX_CAPTURE_DEST_VRAM_B_0x18000,
		GX_CAPTURE_DEST_VRAM_C_0x18000,
		GX_CAPTURE_DEST_VRAM_D_0x18000
	}
}
