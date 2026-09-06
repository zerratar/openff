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
	public static partial class map
	{
							public class CMapParameterManager
							{
								public static CMapParameterManager m_Instance = new CMapParameterManager();

								private Array m_FileAddr;

								private Array m_CurrentAddr;

								private CMapJumpParameter[] m_MapJump;

								private CMapLandFormParameter[] m_MapLandForm;

								private CMapMonsterPartyParameter[] m_MapMonsterParty;

								private CMapSoundParameter[] m_MapSound;

								private CMapEnCountParameter[] m_MapEnCount;

								private CMapCameraParameter[] m_MapCamera;

								private Array m_pSecretWayParameter;

								public void Initialize()
								{
									Free();
									m_FileAddr = null;
									m_CurrentAddr = null;
								}

								public bool Load(string file_name)
								{
									Free();
									bool result = false;
									switch (file_name[0])
									{
									case 'd':
										FS_ChangeDir("/MAP/DUNGEON/PARAMETER");
										break;
									case 't':
										FS_ChangeDir("/MAP/TOWN/PARAMETER");
										break;
									case 's':
										FS_ChangeDir("/MAP/SHOP/PARAMETER");
										break;
									case 'f':
									{
										byte b = (byte)(ushort)stageMng.getStageType();
										string arg = "";
										sprintf(out arg, "/MAP/FIELD/F%02d", b);
										FS_ChangeDir(arg);
										break;
									}
									}
									strcpy(out var arg2, file_name);
									uint size = ds.g_File.getSize(arg2);
									if (size == 0)
									{
										return result;
									}
									m_FileAddr = ds.CHeap.alloc_app(size);
									result = ds.g_File.load(m_FileAddr, arg2);
									FS_ChangeDir("/");
									m_CurrentAddr = m_FileAddr;
									if (file_name[0] == 'f')
									{
										fieldBlockParameter(sceneMng.getStage());
									}
									else
									{
										if (!OpenFF.Client.GameProfile.Ff3MapParameters)
										{
											// PORT: FF4's chains (encounters, landforms, parties, environment)
											// are not FF3's; until they are read, the map has no jumps, no
											// encounters and default cameras.
											m_MapJump = new CMapJumpParameter[16];
											for (int jumpIndex = 0; jumpIndex < m_MapJump.Length; jumpIndex++)
											{
												m_MapJump[jumpIndex] = new CMapJumpParameter();
												m_MapJump[jumpIndex].Kind_set(-1);
											}
											m_MapLandForm = new[] { new CMapLandFormParameter() };
											m_MapMonsterParty = new[] { new CMapMonsterPartyParameter() };
											m_MapSound = new[] { new CMapSoundParameter() };
											m_MapEnCount = new[] { new CMapEnCountParameter() };
											m_MapCamera = new[] { new CMapCameraParameter() };
											m_pSecretWayParameter = null;
										}
										else
										{
										m_MapJump = CMapJumpParameter.ChainPointer((byte[])m_CurrentAddr, 0);
										m_MapLandForm = CMapLandFormParameter.ChainPointer((byte[])m_CurrentAddr, 1);
										m_MapMonsterParty = CMapMonsterPartyParameter.ChainPointer((byte[])m_CurrentAddr, 2);
										m_MapSound = CMapSoundParameter.ChainPointer((byte[])m_CurrentAddr, 3);
										m_MapEnCount = CMapEnCountParameter.ChainPointer((byte[])m_CurrentAddr, 4);
										m_MapCamera = CMapCameraParameter.ChainPointer((byte[])m_CurrentAddr, 5);
										m_pSecretWayParameter = pack.ChainPointer((byte[])m_CurrentAddr, 6u);
										}
									}
									return result;
								}

								public void Free()
								{
									if (m_FileAddr != null)
									{
										ds.CHeap.free_app(m_FileAddr);
										m_FileAddr = null;
									}
								}

								public CMapJumpParameter MapJumpParameter(int _id)
								{
									return Clamp(m_MapJump, _id);
								}

								public CMapLandFormParameter MapLandFormParameter(int _id)
								{
									return Clamp(m_MapLandForm, _id);
								}

								public CMapMonsterPartyParameter MapMonsterPartyParameter(int _id)
								{
									return Clamp(m_MapMonsterParty, _id);
								}

								public CMapSoundParameter MapSoundParameter(int _id)
								{
									return Clamp(m_MapSound, _id);
								}

								public CMapEnCountParameter MapEnCountParameter(int _id)
								{
									return Clamp(m_MapEnCount, _id);
								}

								public CMapCameraParameter MapCameraParameter(int _id)
								{
									return Clamp(m_MapCamera, _id);
								}

								public void fieldBlockParameter(string pFileName)
								{
									if (m_FileAddr != null)
									{
										m_CurrentAddr = m_FileAddr;
										uint num = 0u;
										m_CurrentAddr = pack.ChainPointer(index: (uint)((pFileName != null) ? stageMng.getChipNo(pFileName) : stageMng.getChipNo()), addr: (byte[])m_CurrentAddr);
										if (!OpenFF.Client.GameProfile.Ff3MapParameters)
										{
											// PORT: FF4's chains (encounters, landforms, parties, environment)
											// are not FF3's; until they are read, the map has no jumps, no
											// encounters and default cameras.
											m_MapJump = new CMapJumpParameter[16];
											for (int jumpIndex = 0; jumpIndex < m_MapJump.Length; jumpIndex++)
											{
												m_MapJump[jumpIndex] = new CMapJumpParameter();
												m_MapJump[jumpIndex].Kind_set(-1);
											}
											m_MapLandForm = new[] { new CMapLandFormParameter() };
											m_MapMonsterParty = new[] { new CMapMonsterPartyParameter() };
											m_MapSound = new[] { new CMapSoundParameter() };
											m_MapEnCount = new[] { new CMapEnCountParameter() };
											m_MapCamera = new[] { new CMapCameraParameter() };
											m_pSecretWayParameter = null;
										}
										else
										{
										m_MapJump = CMapJumpParameter.ChainPointer((byte[])m_CurrentAddr, 0);
										m_MapLandForm = CMapLandFormParameter.ChainPointer((byte[])m_CurrentAddr, 1);
										m_MapMonsterParty = CMapMonsterPartyParameter.ChainPointer((byte[])m_CurrentAddr, 2);
										m_MapSound = CMapSoundParameter.ChainPointer((byte[])m_CurrentAddr, 3);
										m_MapEnCount = CMapEnCountParameter.ChainPointer((byte[])m_CurrentAddr, 4);
										m_MapCamera = CMapCameraParameter.ChainPointer((byte[])m_CurrentAddr, 5);
										m_pSecretWayParameter = pack.ChainPointer((byte[])m_CurrentAddr, 6u);
										}
									}
								}

								public int mapJumpNum()
								{
									if (!OpenFF.Client.GameProfile.Ff3MapParameters)
									{
										return 0;
									}
									return (int)(pack.ChainPointerSize((byte[])m_CurrentAddr, 0u) / 44);
								}

								public CMapParameterManager()
								{
									m_FileAddr = null;
								}

								public static CMapParameterManager Instance()
								{
									return m_Instance;
								}

								/// <summary>
								/// PORT: a table shorter than the id asked for answers with its last record
								/// rather than throwing - FF4 maps carry none of these tables yet.
								/// </summary>
								private static T Clamp<T>(T[] table, int _id) where T : class, new()
								{
									if (table == null || table.Length == 0)
									{
										// A map with no parameter file at all (FF4's event maps): a blank record.
										return Blank<T>.Value;
									}
									if (_id < 0)
									{
										_id = 0;
									}
									return table[_id < table.Length ? _id : table.Length - 1];
								}

								private static class Blank<T> where T : class, new()
								{
									public static readonly T Value = new T();
								}

								public bool isLoaded()
								{
									if (m_FileAddr == null)
									{
										return false;
									}
									return true;
								}

								public Array MapSecretWayParameter()
								{
									return m_pSecretWayParameter;
								}
							}
	}
}
