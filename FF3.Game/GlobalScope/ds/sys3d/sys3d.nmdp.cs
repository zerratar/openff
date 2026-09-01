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
		public static partial class sys3d
		{
			public static class nmdp
			{
				public class SModelFileHeader
				{
					public byte[] ucFileType = new byte[4];

					public uint unVersion;

					public uint[] unReserve = new uint[2];

					public SModelInfoHeader m_InfoHeader;

					public NNSG3dResFileHeader m_ResFileHeaderModel;

					private SModelFileHeader()
					{
						ucFileType[0] = 78;
						ucFileType[1] = 77;
						ucFileType[2] = 68;
						ucFileType[3] = 80;
						unVersion = NMDP_CURRENT_VERSION;
						unReserve[0] = (unReserve[1] = 0u);
					}

					public static explicit operator SModelFileHeader(ArrayReader src)
					{
						SModelFileHeader sModelFileHeader = new SModelFileHeader();
						src.read(sModelFileHeader.ucFileType, 0, 4);
						sModelFileHeader.unVersion = src.readUInt32();
						src.read(sModelFileHeader.unReserve, 0, 2);
						sModelFileHeader.m_InfoHeader = (SModelInfoHeader)src;
						if (sModelFileHeader.m_InfoHeader.unOffsetModels != 0)
						{
							src.setPosition(sModelFileHeader.m_InfoHeader.unOffsetModels);
							sModelFileHeader.m_ResFileHeaderModel = (NNSG3dResFileHeader)src;
						}
						return sModelFileHeader;
					}
				}

				public class SModelInfoHeader
				{
					public byte ucNbModels;

					public byte[] ucPadding = new byte[3];

					public uint unFlag;

					public uint unSizeModels;

					public uint unOffsetModels;

					public uint unLodDistance;

					public uint[] unReserve = new uint[3];

					private SModelInfoHeader()
					{
						ucNbModels = (ucPadding[0] = (ucPadding[1] = (ucPadding[2] = 0)));
						unFlag = (unSizeModels = (unOffsetModels = (unLodDistance = 0u)));
						unReserve[0] = (unReserve[1] = (unReserve[2] = 0u));
					}

					public static explicit operator SModelInfoHeader(ArrayReader src)
					{
						SModelInfoHeader sModelInfoHeader = new SModelInfoHeader();
						sModelInfoHeader.ucNbModels = src.readByte();
						src.read(sModelInfoHeader.ucPadding, 0, 3);
						sModelInfoHeader.unFlag = src.readUInt32();
						sModelInfoHeader.unSizeModels = src.readUInt32();
						sModelInfoHeader.unOffsetModels = src.readUInt32();
						sModelInfoHeader.unLodDistance = src.readUInt32();
						src.read(sModelInfoHeader.unReserve, 0, 3);
						return sModelInfoHeader;
					}
				}

				public enum enFLAG
				{
					enFLAG_INITIALIZED = 1,
					enFLAG_MODEL_ONLY = 2,
					enFLAG_TEXTURE_ONLY = 4,
					enFLAG_JOINT_STACK = 8,
					enFLAG_USE_LOD = 16,
					enFLAG_END = 17
				}

				public const enFLAG enFLAG_INITIALIZED = enFLAG.enFLAG_INITIALIZED;

				public const enFLAG enFLAG_MODEL_ONLY = enFLAG.enFLAG_MODEL_ONLY;

				public const enFLAG enFLAG_TEXTURE_ONLY = enFLAG.enFLAG_TEXTURE_ONLY;

				public const enFLAG enFLAG_JOINT_STACK = enFLAG.enFLAG_JOINT_STACK;

				public const enFLAG enFLAG_USE_LOD = enFLAG.enFLAG_USE_LOD;

				public const enFLAG enFLAG_END = enFLAG.enFLAG_END;

				public static uint NMDP_CURRENT_VERSION = 4096u;
			}
		}
	}
}
