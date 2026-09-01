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
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
	public static partial class ds
	{
		public static partial class sys3d
		{
			public static class namp
			{
				public class SAnimationFileHeader
				{
					public byte[] ucFileType = new byte[4];

					public uint unVersion;

					public uint[] unReserve = new uint[2];

					public SAnimationInfoHeader m_InfoHeader;

					public NNSG3dResFileHeader m_ResFileHeaderIma;

					public NNSG3dResFileHeader m_ResFileHeaderIta;

					public NNSG3dResFileHeader m_ResFileHeaderItp;

					public NNSG3dResFileHeader m_ResFileHeaderIva;

					private SAnimationFileHeader()
					{
						ucFileType[0] = 78;
						ucFileType[1] = 65;
						ucFileType[2] = 77;
						ucFileType[3] = 80;
						unVersion = NAMP_CURRENT_VERSION;
						unReserve[0] = (unReserve[1] = 0u);
					}

					public static explicit operator SAnimationFileHeader(ArrayReader src)
					{
						SAnimationFileHeader sAnimationFileHeader = new SAnimationFileHeader();
						src.read(sAnimationFileHeader.ucFileType, 0, 4);
						sAnimationFileHeader.unVersion = src.readUInt32();
						src.read(sAnimationFileHeader.unReserve, 0, 2);
						sAnimationFileHeader.m_InfoHeader = (SAnimationInfoHeader)src;
						if (sAnimationFileHeader.m_InfoHeader.unOffsetIma != 0)
						{
							src.setPosition(sAnimationFileHeader.m_InfoHeader.unOffsetIma);
							sAnimationFileHeader.m_ResFileHeaderIma = (NNSG3dResFileHeader)src;
						}
						if (sAnimationFileHeader.m_InfoHeader.unOffsetIta != 0)
						{
							src.setPosition(sAnimationFileHeader.m_InfoHeader.unOffsetIta);
							sAnimationFileHeader.m_ResFileHeaderIta = (NNSG3dResFileHeader)src;
						}
						if (sAnimationFileHeader.m_InfoHeader.unOffsetItp != 0)
						{
							src.setPosition(sAnimationFileHeader.m_InfoHeader.unOffsetItp);
							sAnimationFileHeader.m_ResFileHeaderItp = (NNSG3dResFileHeader)src;
						}
						if (sAnimationFileHeader.m_InfoHeader.unOffsetIva != 0)
						{
							src.setPosition(sAnimationFileHeader.m_InfoHeader.unOffsetIva);
							sAnimationFileHeader.m_ResFileHeaderIva = (NNSG3dResFileHeader)src;
						}
						return sAnimationFileHeader;
					}
				}

				public class SAnimationInfoHeader
				{
					public byte ucNbIma;

					public byte ucNbIta;

					public byte ucNbItp;

					public byte ucNbIva;

					public uint unFlag;

					public uint unSizeIma;

					public uint unSizeIta;

					public uint unSizeItp;

					public uint unSizeIva;

					public uint unOffsetIma;

					public uint unOffsetIta;

					public uint unOffsetItp;

					public uint unOffsetIva;

					public uint[] unReserve = new uint[2];

					private SAnimationInfoHeader()
					{
						unReserve[0] = (unReserve[1] = 0u);
					}

					public static explicit operator SAnimationInfoHeader(ArrayReader src)
					{
						SAnimationInfoHeader sAnimationInfoHeader = new SAnimationInfoHeader();
						sAnimationInfoHeader.ucNbIma = src.readByte();
						sAnimationInfoHeader.ucNbIta = src.readByte();
						sAnimationInfoHeader.ucNbItp = src.readByte();
						sAnimationInfoHeader.ucNbIva = src.readByte();
						sAnimationInfoHeader.unFlag = src.readUInt32();
						sAnimationInfoHeader.unSizeIma = src.readUInt32();
						sAnimationInfoHeader.unSizeIta = src.readUInt32();
						sAnimationInfoHeader.unSizeItp = src.readUInt32();
						sAnimationInfoHeader.unSizeIva = src.readUInt32();
						sAnimationInfoHeader.unOffsetIma = src.readUInt32();
						sAnimationInfoHeader.unOffsetIta = src.readUInt32();
						sAnimationInfoHeader.unOffsetItp = src.readUInt32();
						sAnimationInfoHeader.unOffsetIva = src.readUInt32();
						src.read(sAnimationInfoHeader.unReserve, 0, 2);
						return sAnimationInfoHeader;
					}
				}

				public enum enFLAG
				{
					enFLAG_INITIALIZED = 1,
					enFLAG_END
				}

				public const enFLAG enFLAG_INITIALIZED = enFLAG.enFLAG_INITIALIZED;

				public const enFLAG enFLAG_END = enFLAG.enFLAG_END;

				public static uint NAMP_CURRENT_VERSION = 4096u;
			}
		}
	}
}
