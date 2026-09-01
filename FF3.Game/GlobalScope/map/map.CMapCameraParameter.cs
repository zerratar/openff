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
	public static partial class map
	{
							public class CMapCameraParameter
							{
								private short m_Collision;

								private short m_ClipNear;

								private short m_ClipFar;

								private short m_Mode;

								private short[] m_PositionOffset = new short[3];

								private short[] m_TargetOffset = new short[3];

								private short m_ZoomOnOff;

								private short m_ZoomType;

								private short m_ZoomMax;

								private short m_ZoomMin;

								private short m_ZoomSpeed;

								public short Collision()
								{
									return m_Collision;
								}

								public short ClipNear()
								{
									return m_ClipNear;
								}

								public short ClipFar()
								{
									return m_ClipFar;
								}

								public short Mode()
								{
									return m_Mode;
								}

								public short PositionOffset(int _index)
								{
									return m_PositionOffset[_index];
								}

								public short TargetOffset(int _index)
								{
									return m_TargetOffset[_index];
								}

								public short ZoomOnOff()
								{
									return m_ZoomOnOff;
								}

								public short ZoomType()
								{
									return m_ZoomType;
								}

								public short ZoomMax()
								{
									return m_ZoomMax;
								}

								public short ZoomMin()
								{
									return m_ZoomMin;
								}

								public short ZoomSpeed()
								{
									return m_ZoomSpeed;
								}

								public static CMapCameraParameter[] ChainPointer(byte[] abyData, int iId)
								{
									ArrayReader arrayReader = new ArrayReader(abyData);
									arrayReader.skip(16L);
									arrayReader.skip(8 * iId);
									uint num = arrayReader.readUInt32();
									uint num2 = arrayReader.readUInt32();
									uint num3 = num2 / 30;
									arrayReader.setPosition(num);
									CMapCameraParameter[] array = new CMapCameraParameter[num3];
									for (int i = 0; i < num3; i++)
									{
										array[i] = new CMapCameraParameter();
										array[i].parse(arrayReader);
									}
									arrayReader.dispose();
									return array;
								}

								public void parse(ArrayReader reader)
								{
									m_Collision = reader.readInt16();
									m_ClipNear = reader.readInt16();
									m_ClipFar = reader.readInt16();
									m_Mode = reader.readInt16();
									reader.read(m_PositionOffset, 0, 3);
									reader.read(m_TargetOffset, 0, 3);
									m_ZoomOnOff = reader.readInt16();
									m_ZoomType = reader.readInt16();
									m_ZoomMax = reader.readInt16();
									m_ZoomMin = reader.readInt16();
									m_ZoomSpeed = reader.readInt16();
								}
							}
	}
}
