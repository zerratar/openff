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
	public class NNSG3dResShpData
	{
		public ushort itemTag;

		public ushort size;

		public uint flag;

		public uint ofsDL;

		public uint sizeDL;

		public uint[] cmd;

		public byte[] cmd_color_r;

		public byte[] cmd_color_g;

		public byte[] cmd_color_b;

		public float[] cmd_coord_u;

		public float[] cmd_coord_v;

		public float[] cmd_vertex_x;

		public float[] cmd_vertex_y;

		public float[] cmd_vertex_z;

		public static explicit operator NNSG3dResShpData(ArrayReader src)
		{
			NNSG3dResShpData nNSG3dResShpData = new NNSG3dResShpData();
			long position = src.getPosition();
			nNSG3dResShpData.itemTag = src.readUInt16();
			nNSG3dResShpData.size = src.readUInt16();
			nNSG3dResShpData.flag = src.readUInt32();
			nNSG3dResShpData.ofsDL = src.readUInt32();
			nNSG3dResShpData.sizeDL = src.readUInt32();
			src.setPosition(position + nNSG3dResShpData.ofsDL);
			int num = (int)(nNSG3dResShpData.sizeDL / 4);
			nNSG3dResShpData.cmd = new uint[num + 1];
			src.read(nNSG3dResShpData.cmd, 0, num);
			return nNSG3dResShpData;
		}

		public void preBuild(int scale)
		{
			int num = 0;
			uint[] array = cmd;
			uint num2 = array[num];
			int num3 = num + 1;
			int num4 = (int)(num + sizeDL / 4);
			int num5 = 0;
			VecFx32 fnd_reuse_pos = GlobalScope.fnd_reuse_pos;
			float num6 = 0f;
			float num7 = 0f;
			byte b = byte.MaxValue;
			byte b2 = byte.MaxValue;
			byte b3 = byte.MaxValue;
			int num8 = 0;
			int num9 = 0;
			int num10 = 0;
			while (num < num4)
			{
				switch (num2 & 0xFF)
				{
				case 20u:
					num3++;
					break;
				case 27u:
					num3 += 3;
					break;
				case 32u:
					num8++;
					num3++;
					break;
				case 33u:
					num3++;
					break;
				case 34u:
					num9++;
					num3++;
					break;
				case 35u:
				case 36u:
				case 37u:
				case 38u:
				case 39u:
				case 40u:
					switch (num2 & 0xFF)
					{
					case 35u:
						num3++;
						num3++;
						break;
					case 36u:
						num3++;
						break;
					case 37u:
						num3++;
						break;
					case 38u:
						num3++;
						break;
					case 39u:
						num3++;
						break;
					case 40u:
						num3++;
						break;
					}
					num10++;
					break;
				case 64u:
					num3++;
					break;
				default:
					OS_Terminate();
					break;
				case 0u:
				case 65u:
					break;
				}
				num++;
				num2 >>= 8;
				if (++num5 == 4)
				{
					num5 = 0;
					num = num3;
					num2 = array[num];
					num3 = num + 1;
				}
			}
			cmd_color_r = new byte[num8];
			cmd_color_g = new byte[num8];
			cmd_color_b = new byte[num8];
			cmd_coord_u = new float[num9];
			cmd_coord_v = new float[num9];
			cmd_vertex_x = new float[num10];
			cmd_vertex_y = new float[num10];
			cmd_vertex_z = new float[num10];
			num = 0;
			array = cmd;
			num2 = array[num];
			num3 = num + 1;
			num4 = (int)(num + sizeDL / 4);
			num5 = 0;
			fnd_reuse_pos = GlobalScope.fnd_reuse_pos;
			num6 = 0f;
			num7 = 0f;
			b = byte.MaxValue;
			b2 = byte.MaxValue;
			b3 = byte.MaxValue;
			num8 = 0;
			num9 = 0;
			num10 = 0;
			while (num < num4)
			{
				switch (num2 & 0xFF)
				{
				case 20u:
					num3++;
					break;
				case 27u:
					num3 += 3;
					break;
				case 32u:
					b = (byte)(array[num3] << 3);
					b2 = (byte)(array[num3] >> 5 << 3);
					b3 = (byte)(array[num3] >> 10 << 3);
					cmd_color_r[num8] = b;
					cmd_color_g[num8] = b2;
					cmd_color_b[num8] = b3;
					num8++;
					num3++;
					break;
				case 33u:
					num3++;
					break;
				case 34u:
					num6 = FX_FX32_TO_F32((short)array[num3] << 8);
					num7 = FX_FX32_TO_F32((short)(array[num3] >> 16) << 8);
					cmd_coord_u[num9] = num6;
					cmd_coord_v[num9] = num7;
					num9++;
					num3++;
					break;
				case 35u:
				case 36u:
				case 37u:
				case 38u:
				case 39u:
				case 40u:
					switch (num2 & 0xFF)
					{
					case 35u:
						fnd_reuse_pos.x = (short)array[num3] << scale;
						fnd_reuse_pos.y = (short)(array[num3] >> 16) << scale;
						num3++;
						fnd_reuse_pos.z = (short)array[num3] << scale;
						num3++;
						break;
					case 36u:
						fnd_reuse_pos.x = (short)(array[num3] << 6) << scale;
						fnd_reuse_pos.y = (short)(array[num3] >> 10 << 6) << scale;
						fnd_reuse_pos.z = (short)(array[num3] >> 20 << 6) << scale;
						num3++;
						break;
					case 37u:
						fnd_reuse_pos.x = (short)array[num3] << scale;
						fnd_reuse_pos.y = (short)(array[num3] >> 16) << scale;
						num3++;
						break;
					case 38u:
						fnd_reuse_pos.x = (short)array[num3] << scale;
						fnd_reuse_pos.z = (short)(array[num3] >> 16) << scale;
						num3++;
						break;
					case 39u:
						fnd_reuse_pos.y = (short)array[num3] << scale;
						fnd_reuse_pos.z = (short)(array[num3] >> 16) << scale;
						num3++;
						break;
					case 40u:
						fnd_reuse_pos.x += (short)(array[num3] << 6) >> 6 << scale;
						fnd_reuse_pos.y += (short)(array[num3] >> 10 << 6) >> 6 << scale;
						fnd_reuse_pos.z += (short)(array[num3] >> 20 << 6) >> 6 << scale;
						num3++;
						break;
					}
					cmd_vertex_x[num10] = FX_FX32_TO_F32(fnd_reuse_pos.x);
					cmd_vertex_y[num10] = FX_FX32_TO_F32(fnd_reuse_pos.y);
					cmd_vertex_z[num10] = FX_FX32_TO_F32(fnd_reuse_pos.z);
					num10++;
					break;
				case 64u:
					num3++;
					break;
				default:
					OS_Terminate();
					break;
				case 0u:
				case 65u:
					break;
				}
				num++;
				num2 >>= 8;
				if (++num5 == 4)
				{
					num5 = 0;
					num = num3;
					num2 = array[num];
					num3 = num + 1;
				}
			}
		}
	}
}
