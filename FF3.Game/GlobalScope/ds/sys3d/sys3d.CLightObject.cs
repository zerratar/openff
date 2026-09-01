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
			public class CLightObject
			{
				private Light[] m_Light = new Light[4];

				private ushort m_Ambient;

				private ushort m_Diffuze;

				private ushort m_Specular;

				private ushort m_Emission;

				public CLightObject()
				{
					for (int i = 0; i < m_Light.Length; i++)
					{
						m_Light[i] = new Light();
					}
					VecFx16 vec = new VecFx16(0, -4096, 0);
					VecFx16 vec2 = new VecFx16(0, 0, -4096);
					m_Light[0].vec = vec;
					m_Light[1].vec = vec;
					m_Light[2].vec = vec2;
					m_Light[3].vec = vec2;
					m_Light[0].r = 22;
					m_Light[0].g = 22;
					m_Light[0].b = 22;
					m_Light[1].r = 22;
					m_Light[1].g = 22;
					m_Light[1].b = 22;
					m_Light[2].r = 22;
					m_Light[2].g = 22;
					m_Light[2].b = 22;
					m_Light[3].r = 22;
					m_Light[3].g = 22;
					m_Light[3].b = 22;
					m_Light[1].r = 0;
					m_Light[1].g = 0;
					m_Light[1].b = 0;
					m_Light[2].r = 0;
					m_Light[2].g = 0;
					m_Light[2].b = 0;
					m_Light[3].r = 0;
					m_Light[3].g = 0;
					m_Light[3].b = 0;
				}

				~CLightObject()
				{
				}

				public void calculate()
				{
					NNS_G3dGlbLightVector(GXLightId.GX_LIGHTID_0, m_Light[0].vec.x, m_Light[0].vec.y, m_Light[0].vec.z);
					NNS_G3dGlbLightVector(GXLightId.GX_LIGHTID_1, m_Light[1].vec.x, m_Light[1].vec.y, m_Light[1].vec.z);
					NNS_G3dGlbLightVector(GXLightId.GX_LIGHTID_2, m_Light[2].vec.x, m_Light[2].vec.y, m_Light[2].vec.z);
					NNS_G3dGlbLightVector(GXLightId.GX_LIGHTID_3, m_Light[3].vec.x, m_Light[3].vec.y, m_Light[3].vec.z);
					NNS_G3dGlbLightColor(GXLightId.GX_LIGHTID_0, GX_RGB(m_Light[0].r, m_Light[0].g, m_Light[0].b));
					NNS_G3dGlbLightColor(GXLightId.GX_LIGHTID_1, GX_RGB(m_Light[1].r, m_Light[1].g, m_Light[1].b));
					NNS_G3dGlbLightColor(GXLightId.GX_LIGHTID_2, GX_RGB(m_Light[2].r, m_Light[2].g, m_Light[2].b));
					NNS_G3dGlbLightColor(GXLightId.GX_LIGHTID_3, GX_RGB(m_Light[3].r, m_Light[3].g, m_Light[3].b));
					NNS_G3dGlbMaterialColorDiffAmb(m_Diffuze, m_Ambient, 0);
					NNS_G3dGlbMaterialColorSpecEmi(m_Specular, m_Emission, 0);
				}

				public void setLightVector(int index, short x, short y, short z)
				{
					m_Light[index].vec.x = x;
					m_Light[index].vec.y = y;
					m_Light[index].vec.z = z;
				}

				public void setLightColor(int index, sbyte r, sbyte g, sbyte b)
				{
					m_Light[index].r = r;
					m_Light[index].g = g;
					m_Light[index].b = b;
				}

				public void setAmbient(sbyte r, sbyte g, sbyte b)
				{
					m_Ambient = GX_RGB(r, g, b);
				}

				public void setDiffuze(sbyte r, sbyte g, sbyte b)
				{
					m_Diffuze = GX_RGB(r, g, b);
				}

				public void setSpecular(sbyte r, sbyte g, sbyte b)
				{
					m_Specular = GX_RGB(r, g, b);
				}

				public void setEmission(sbyte r, sbyte g, sbyte b)
				{
					m_Emission = GX_RGB(r, g, b);
				}

				public void setLight(uint index, Light light)
				{
					m_Light[index] = light;
					switch (index)
					{
					case 0u:
						NNS_G3dGlbLightColor(GXLightId.GX_LIGHTID_0, GX_RGB(m_Light[0].r, m_Light[0].g, m_Light[0].b));
						break;
					case 1u:
						NNS_G3dGlbLightColor(GXLightId.GX_LIGHTID_1, GX_RGB(m_Light[1].r, m_Light[1].g, m_Light[1].b));
						break;
					case 2u:
						NNS_G3dGlbLightColor(GXLightId.GX_LIGHTID_2, GX_RGB(m_Light[2].r, m_Light[2].g, m_Light[2].b));
						break;
					case 3u:
						NNS_G3dGlbLightColor(GXLightId.GX_LIGHTID_3, GX_RGB(m_Light[3].r, m_Light[3].g, m_Light[3].b));
						break;
					}
				}

				public void copy(CLightObject src)
				{
					m_Light[0] = src.m_Light[0];
					m_Light[1] = src.m_Light[1];
					m_Light[2] = src.m_Light[2];
					m_Light[3] = src.m_Light[3];
					m_Ambient = src.m_Ambient;
					m_Diffuze = src.m_Diffuze;
					m_Specular = src.m_Specular;
					m_Emission = src.m_Emission;
				}
			}
		}
	}
}
