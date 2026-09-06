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
	public static partial class map
	{
							public class SecretWayMng
							{
								public class SWFileHeader
								{
									public byte[] fileType = new byte[4];

									public byte numSecretWay;

									public byte[] pad = new byte[3];

									public SecretWayParameter[] m_aSecretWayParameter;

									public static explicit operator SWFileHeader(Array src)
									{
										SWFileHeader sWFileHeader = new SWFileHeader();
										ArrayReader arrayReader = new ArrayReader(src);
										arrayReader.read(sWFileHeader.fileType, 0, 4);
										sWFileHeader.numSecretWay = arrayReader.readByte();
										arrayReader.read(sWFileHeader.pad, 0, 3);
										sWFileHeader.m_aSecretWayParameter = new SecretWayParameter[sWFileHeader.numSecretWay];
										for (int i = 0; i < sWFileHeader.numSecretWay; i++)
										{
											sWFileHeader.m_aSecretWayParameter[i] = (SecretWayParameter)arrayReader;
										}
										arrayReader.dispose();
										return sWFileHeader;
									}
								}

								private Array pData_;

								private SWFileHeader pFileHeader_;

								private SecretWay[] pSecretWayList_;

								public SecretWayMng()
								{
									pData_ = null;
									pFileHeader_ = null;
									pSecretWayList_ = null;
								}

								~SecretWayMng()
								{
								}

								public void initialize(Array pData)
								{
									pData_ = pData;
									if (pData == null)
									{
										// PORT: no secret-way chain in this map's parameters (FF4).
										pFileHeader_ = null;
										pSecretWayList_ = null;
										return;
									}
									pFileHeader_ = (SWFileHeader)pData_;
									_ = pFileHeader_.fileType;
									byte numSecretWay = pFileHeader_.numSecretWay;
									if (numSecretWay > 0)
									{
										SecretWayParameter[] aSecretWayParameter = pFileHeader_.m_aSecretWayParameter;
										pSecretWayList_ = new SecretWay[numSecretWay];
										for (int i = 0; i < numSecretWay; i++)
										{
											pSecretWayList_[i] = new SecretWay();
											pSecretWayList_[i].initialize(aSecretWayParameter[i]);
										}
									}
								}

								public void terminate()
								{
									if (pSecretWayList_ != null && pFileHeader_ != null)
									{
										for (int i = 0; i < pFileHeader_.numSecretWay; i++)
										{
											pSecretWayList_[i].terminate();
											pSecretWayList_[i].destruct();
										}
										ds.CHeap.free_app(pSecretWayList_);
									}
									pSecretWayList_ = null;
									pData_ = null;
									pFileHeader_ = null;
								}

								public void execute(VecFx32 pos)
								{
									if (pData_ != null && pFileHeader_ != null && pSecretWayList_ != null)
									{
										for (int i = 0; i < pFileHeader_.numSecretWay; i++)
										{
											pSecretWayList_[i].setPosition(pos);
											pSecretWayList_[i].update();
										}
									}
								}
							}
	}
}
