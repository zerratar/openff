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
		public class Texture
		{
			private static uint FLAG_INITIALIZED = 1u;

			private static uint FLAG_NO_NTFT = 2u;

			private static uint FLAG_NO_NTFP = 4u;

			private static uint FLAG_NO_NTFI = 8u;

			private static uint FLAG_STATIONED = 16u;

			private byte[] _ucFileType = new byte[4];

			private uint _unVersion;

			private uint _unHeaderSize;

			private string _cDatabaseName;

			private uint _unSizeNtft;

			private uint _unSizeNtfp;

			private uint _unSizeNtfi;

			private Array _unOffsetNtft;

			private Array _unOffsetNtfp;

			private Array _unOffsetNtfi;

			private TexVram _unSendAddrNtft;

			private uint _unSendAddrNtfp;

			private uint _unSendAddrNtfi;

			private uint _unFlag;

			private TexVram _unKeyTexel;

			private TexVram _unKeyPalette;

			private GXTexFmt _byFormatType_8;

			private int _byGenType_8;

			private GXTexSizeS _bySizeS_8;

			private GXTexSizeT _bySizeT_8;

			private int _byRepeatType_8;

			private int _byFlipType_8;

			private int _byTransparentZero_8;

			private byte _byReserve;

			public static Texture create(Texture pAddr, TexVram addrNtft, uint addrNtfp)
			{
				Texture texture = reinterpret_cast<Texture>(pAddr);
				texture.initialize(addrNtft, addrNtfp);
				return texture;
			}

			public static Texture createStation(Texture pAddr, TexVram keyTexel, TexVram keyPltt)
			{
				Texture texture = reinterpret_cast<Texture>(pAddr);
				texture.initializeStation(keyTexel, keyPltt);
				return texture;
			}

			public void initialize(TexVram addrNtft, uint addrNtfp)
			{
				if (!isEnable(FLAG_INITIALIZED))
				{
					assertValidateCode();
					if (_unKeyTexel == null)
					{
						_unKeyTexel = NNS_GfdAllocLnkTexVram(0u, 0, 0u);
						addrNtft = NNS_GfdGetTexKeyAddr(_unKeyTexel);
					}
					LoadTexture(_unKeyTexel, _byFormatType_8, _bySizeS_8, _bySizeT_8, _byTransparentZero_8, (ushort[])_unOffsetNtft, (ushort[])_unOffsetNtfp);
					enable(FLAG_INITIALIZED);
					disable(FLAG_STATIONED);
					setAddress(addrNtft, addrNtfp);
				}
			}

			public void initializeStation(TexVram keyTexel, TexVram keyPltt)
			{
				_unKeyTexel = keyTexel;
				_unKeyPalette = keyPltt;
				initialize(NNS_GfdGetTexKeyAddr(keyTexel), NNS_GfdGetPlttKeyAddr(keyPltt));
				setAddress(NNS_GfdGetTexKeyAddr(keyTexel), NNS_GfdGetPlttKeyAddr(keyPltt));
				enable(FLAG_STATIONED);
				_unSizeNtft = NNS_GfdGetTexKeySize(keyTexel);
				_unSizeNtfp = NNS_GfdGetPlttKeySize(keyPltt);
				sendManager();
				sendParam();
			}

			public static bool isTexture(Texture pAddr)
			{
				Texture texture = reinterpret_cast<Texture>(pAddr);
				return validateIDCode(texture._ucFileType, TextureCode);
			}

			public bool isInitialized()
			{
				if (!validateIDCode(_ucFileType, TextureCode))
				{
					return false;
				}
				if (!isEnable(FLAG_INITIALIZED))
				{
					return false;
				}
				return true;
			}

			public void setAddress(TexVram addrNtft, uint addrNtfp)
			{
				assertValidate();
				_unSendAddrNtft = addrNtft;
				_unSendAddrNtfp = addrNtfp;
			}

			public static void getSize(Texture pAddr, out uint sizeTexel, out uint sizePltt)
			{
				Texture texture = reinterpret_cast<Texture>(pAddr);
				texture.getSize(out sizeTexel, out sizePltt);
			}

			public void getSize(out uint sizeTexel, out uint sizePltt)
			{
				sizeTexel = _unSizeNtft;
				sizePltt = _unSizeNtfp;
			}

			public void send()
			{
				sendParam();
				if (!isEnable(FLAG_STATIONED))
				{
					sendDirect();
				}
			}

			public void sendDirect()
			{
				assertValidateAlignment();
				if (!isEnable(FLAG_NO_NTFT))
				{
					GX_BeginLoadTex();
					GX_EndLoadTex();
				}
				if (!isEnable(FLAG_NO_NTFP))
				{
					GX_BeginLoadTexPltt();
					GX_EndLoadTexPltt();
				}
			}

			public void sendParam()
			{
				G3_TexImageParam(_byFormatType_8, _byGenType_8, _bySizeS_8, _bySizeT_8, _byRepeatType_8, _byFlipType_8, _byTransparentZero_8, _unSendAddrNtft);
				G3_TexPlttBase(_unSendAddrNtfp, _byFormatType_8);
			}

			public void sendManager()
			{
				assertValidateAlignment();
				isEnable(FLAG_NO_NTFT);
				isEnable(FLAG_NO_NTFP);
			}

			private Texture()
			{
			}

			public TexVram getKeyTexel()
			{
				return _unKeyTexel;
			}

			public TexVram getKeyPalette()
			{
				return _unKeyPalette;
			}

			public void assertValidateCode()
			{
			}

			public void assertValidateInitialize()
			{
			}

			public void assertValidateAlignment()
			{
			}

			public void assertValidate()
			{
			}

			private void enable(uint codeFlag)
			{
				_unFlag |= codeFlag;
			}

			private void disable(uint codeFlag)
			{
				_unFlag &= ~codeFlag;
			}

			private bool isEnable(uint codeFlag)
			{
				if ((codeFlag & _unFlag) == 0)
				{
					return false;
				}
				return true;
			}

			public static Texture cast(Array src, uint offset)
			{
				Texture texture = new Texture();
				ArrayReader arrayReader = new ArrayReader(src);
				byte[] array = new byte[32];
				arrayReader.setPosition(offset);
				long position = arrayReader.getPosition();
				arrayReader.read(texture._ucFileType, 0, 4);
				texture._unVersion = arrayReader.readUInt32();
				texture._unHeaderSize = arrayReader.readUInt32();
				arrayReader.read(array, 0, 32);
				texture._cDatabaseName = StringUtil.createString(array);
				texture._unSizeNtft = arrayReader.readUInt32();
				texture._unSizeNtfp = arrayReader.readUInt32();
				texture._unSizeNtfi = arrayReader.readUInt32();
				uint num = arrayReader.readUInt32();
				uint num2 = arrayReader.readUInt32();
				uint num3 = arrayReader.readUInt32();
				arrayReader.readUInt32();
				texture._unSendAddrNtfp = arrayReader.readUInt32();
				texture._unSendAddrNtfi = arrayReader.readUInt32();
				texture._unFlag = arrayReader.readUInt32();
				arrayReader.readUInt32();
				arrayReader.readUInt32();
				texture._byFormatType_8 = (GXTexFmt)arrayReader.readByte();
				texture._byGenType_8 = arrayReader.readByte();
				texture._bySizeS_8 = (GXTexSizeS)arrayReader.readByte();
				texture._bySizeT_8 = (GXTexSizeT)arrayReader.readByte();
				texture._byRepeatType_8 = arrayReader.readByte();
				texture._byFlipType_8 = arrayReader.readByte();
				texture._byTransparentZero_8 = arrayReader.readByte();
				texture._byReserve = arrayReader.readByte();
				if (num != 0)
				{
					int num4 = (int)(texture._unSizeNtft / 2);
					texture._unOffsetNtft = new ushort[num4];
					arrayReader.setPosition(position + num);
					arrayReader.read((ushort[])texture._unOffsetNtft, 0, num4);
				}
				if (num2 != 0)
				{
					int num4 = (int)(texture._unSizeNtfp / 2);
					texture._unOffsetNtfp = new ushort[num4];
					arrayReader.setPosition(position + num2);
					arrayReader.read((ushort[])texture._unOffsetNtfp, 0, num4);
				}
				if (num3 != 0)
				{
					int num4 = (int)(texture._unSizeNtfi / 2);
					texture._unOffsetNtfi = new ushort[num4];
					arrayReader.setPosition(position + num3);
					arrayReader.read((ushort[])texture._unOffsetNtfi, 0, num4);
				}
				arrayReader.dispose();
				return texture;
			}

			public bool applyShare(Texture src)
			{
				if (!_cDatabaseName.Equals(src._cDatabaseName))
				{
					return false;
				}
				_unSendAddrNtft = src._unSendAddrNtft;
				_unSendAddrNtfp = src._unSendAddrNtfp;
				_unSendAddrNtfi = src._unSendAddrNtfi;
				_unFlag = src._unFlag;
				_unKeyTexel = src._unKeyTexel;
				_unKeyPalette = src._unKeyPalette;
				return true;
			}
		}
	}
}
