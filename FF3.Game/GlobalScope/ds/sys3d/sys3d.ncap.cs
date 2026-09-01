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
	public static partial class ds
	{
		public static partial class sys3d
		{
			public static class ncap
			{
				public class SMotionFileHeader
				{
					public byte[] ucFileType = new byte[4];

					public uint unVersion;

					public uint[] unReserve = new uint[2];

					public SMotionInfoHeader m_InfoHeader;

					public uint[] m_auiMotIdx;

					public NNSG3dResFileHeader m_ResFileHeaderMot;

					private SMotionFileHeader()
					{
						ucFileType[0] = 78;
						ucFileType[1] = 67;
						ucFileType[2] = 65;
						ucFileType[3] = 80;
						unVersion = NCAP_CURRENT_VERSION;
						unReserve[0] = (unReserve[1] = 0u);
					}

					public static explicit operator SMotionFileHeader(ArrayReader src)
					{
						SMotionFileHeader sMotionFileHeader = new SMotionFileHeader();
						src.read(sMotionFileHeader.ucFileType, 0, 4);
						sMotionFileHeader.unVersion = src.readUInt32();
						src.read(sMotionFileHeader.unReserve, 0, 2);
						sMotionFileHeader.m_InfoHeader = (SMotionInfoHeader)src;
						if (sMotionFileHeader.m_InfoHeader.unOffsetMotionIndices != 0)
						{
							src.setPosition(sMotionFileHeader.m_InfoHeader.unOffsetMotionIndices);
							sMotionFileHeader.m_auiMotIdx = new uint[sMotionFileHeader.m_InfoHeader.ucNbMotions];
							src.read(sMotionFileHeader.m_auiMotIdx, 0, sMotionFileHeader.m_InfoHeader.ucNbMotions);
						}
						if (sMotionFileHeader.m_InfoHeader.unOffsetMotions != 0)
						{
							src.setPosition(sMotionFileHeader.m_InfoHeader.unOffsetMotions);
							sMotionFileHeader.m_ResFileHeaderMot = (NNSG3dResFileHeader)src;
						}
						return sMotionFileHeader;
					}

					public static SMotionFileHeader cast(Array src)
					{
						ArrayReader arrayReader = new ArrayReader(src);
						return (SMotionFileHeader)arrayReader;
					}
				}

				public class SMotionInfoHeader
				{
					public byte ucNbMotions;

					public byte[] ucPadding = new byte[3];

					public uint unFlag;

					public uint unSizeMotions;

					public uint unOffsetMotionIndices;

					public uint unOffsetMotions;

					public uint[] unReserve = new uint[3];

					private SMotionInfoHeader()
					{
						ucPadding[0] = (ucPadding[1] = (ucPadding[2] = 0));
						unReserve[0] = (unReserve[1] = (unReserve[2] = 0u));
					}

					public static explicit operator SMotionInfoHeader(ArrayReader src)
					{
						SMotionInfoHeader sMotionInfoHeader = new SMotionInfoHeader();
						sMotionInfoHeader.ucNbMotions = src.readByte();
						src.read(sMotionInfoHeader.ucPadding, 0, 3);
						sMotionInfoHeader.unFlag = src.readUInt32();
						sMotionInfoHeader.unSizeMotions = src.readUInt32();
						sMotionInfoHeader.unOffsetMotionIndices = src.readUInt32();
						sMotionInfoHeader.unOffsetMotions = src.readUInt32();
						src.read(sMotionInfoHeader.unReserve, 0, 3);
						return sMotionInfoHeader;
					}
				}

				public enum enFLAG
				{
					enFLAG_INITIALIZED = 1,
					enFLAG_OMISSION_SCALE = 2,
					enFLAG_OMISSION_ROTATION = 4,
					enFLAG_OMISSION_TRANSLATION = 8,
					enFLAG_END = 9
				}

				public const enFLAG enFLAG_INITIALIZED = enFLAG.enFLAG_INITIALIZED;

				public const enFLAG enFLAG_OMISSION_SCALE = enFLAG.enFLAG_OMISSION_SCALE;

				public const enFLAG enFLAG_OMISSION_ROTATION = enFLAG.enFLAG_OMISSION_ROTATION;

				public const enFLAG enFLAG_OMISSION_TRANSLATION = enFLAG.enFLAG_OMISSION_TRANSLATION;

				public const enFLAG enFLAG_END = enFLAG.enFLAG_END;

				public static uint NCAP_CURRENT_VERSION = 4096u;
			}
		}
	}
}
