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
	public static class card
	{
		public class Manager
		{
			private delegate int _pRead(uint arg0, Array arg1, uint arg2);

			public enum DATA_STATE
			{
				dsNOT_CHECK,
				dsRIGHT_AND_NEW,
				dsRIGHT,
				dsBROKEN,
				dsMAX
			}

			private delegate void _m_pProc();

			public const int SAVE_NUM_MAX = 4;

			public const int SAVE_BUFFER_MAX = 2;

			public const DATA_STATE dsNOT_CHECK = DATA_STATE.dsNOT_CHECK;

			public const DATA_STATE dsRIGHT_AND_NEW = DATA_STATE.dsRIGHT_AND_NEW;

			public const DATA_STATE dsRIGHT = DATA_STATE.dsRIGHT;

			public const DATA_STATE dsBROKEN = DATA_STATE.dsBROKEN;

			public const DATA_STATE dsMAX = DATA_STATE.dsMAX;

			public static Manager m_Instance = new Manager();

			private bool m_Valid_1;

			private bool m_Finished_1;

			private bool m_SpecifyAddress_1;

			private byte m_DataNum;

			private byte m_BufferNum;

			private byte m_CurrentData;

			private byte m_CurrentBuffer;

			private byte m_SaveBuffer;

			private int m_CardLockId;

			private CARDBackupType m_BackupType;

			private RESULT m_Result;

			private Array m_pDataAddress;

			private uint m_OneDataSize;

			private _m_pProc m_pProc;

			private byte[,] m_DataState = new byte[4, 2];

			public Manager()
			{
				m_Finished_1 = true;
				m_SpecifyAddress_1 = true;
				m_CardLockId = 0;
				m_BackupType = CARDBackupType.CARD_BACKUP_TYPE_NOT_USE;
				m_Result = RESULT.RESULT_SUCCESS;
				m_Valid_1 = true;
				m_pProc = null;
				m_OneDataSize = 0u;
				m_DataNum = 1;
				m_BufferNum = 1;
				m_CurrentData = 0;
				m_CurrentBuffer = 0;
				for (int i = 0; i < 4; i++)
				{
					for (int j = 0; j < 2; j++)
					{
						m_DataState[i, j] = 0;
					}
				}
			}

			public bool Initialize(CARDBackupType type, uint size, byte num, byte buf, Array pBuffer)
			{
				if (m_CardLockId == 0)
				{
					m_CardLockId = OS_GetLockID();
				}
				setCardPulledOutCallback(defaultCardPulledOutCallback);
				m_OneDataSize = (size + 15) / 16 * 16;
				SetBackupType(type);
				SetDataNum(num, buf);
				CheckFromFactory(pBuffer);
				if (m_Result != RESULT.RESULT_SUCCESS)
				{
					return false;
				}
				return true;
			}

			public bool CheckFromFactory(Array pBuffer)
			{
				bool result = true;
				if (pBuffer != null)
				{
					sbyte[] array = new sbyte[32];
					uint num = GetRomByteSize(GetBackupType()) - 32;
					LoadData(array, 32u, num);
					if (m_Result == RESULT.RESULT_SUCCESS && memcmp(array, ONCE_INITIALIZE_CODE, 32) != 0)
					{
						sbyte[] array2 = static_cast<sbyte[]>(pBuffer);
						memcpy(array2, (int)num, ONCE_INITIALIZE_CODE, 32);
						WriteData(array2, GetRomByteSize(GetBackupType()), 0u);
					}
					if (m_Result != RESULT.RESULT_SUCCESS)
					{
						result = false;
					}
				}
				return result;
			}

			public byte GetAlreadyExistDataNum()
			{
				byte b = 0;
				for (int i = 0; i < 4; i++)
				{
					for (int j = 0; j < 2; j++)
					{
						switch (m_DataState[i, j])
						{
						case 1:
						case 2:
							b++;
							break;
						}
					}
				}
				return b;
			}

			public void WriteData(Array pData, uint data_size, uint offset)
			{
				if (PreviousProcess())
				{
					int num = 1;
					if (CARD_IsBackupEeprom() != 0)
					{
						num = CARD_WriteAndVerifyEeprom(offset, pData, data_size);
					}
					else if (CARD_IsBackupFlash() != 0)
					{
						num = CARD_WriteAndVerifyFlash(offset, pData, data_size);
					}
					PostProcess();
					if (num != 1)
					{
						SetResult(RESULT.RESULT_LOST_CARD);
					}
				}
			}

			public void LoadData(Array pData, uint data_size, uint offset)
			{
				if (PreviousProcess())
				{
					int num = 1;
					if (CARD_IsBackupEeprom() != 0)
					{
						num = CARD_ReadEeprom(offset, pData, data_size);
					}
					else if (CARD_IsBackupFlash() != 0)
					{
						num = CARD_ReadFlash(offset, pData, data_size);
					}
					PostProcess();
					if (num != 1)
					{
						SetResult(RESULT.RESULT_LOST_CARD);
					}
				}
			}

			public void SetDataNum(byte num, byte buf)
			{
				m_DataNum = num;
				m_BufferNum = buf;
				m_CurrentData = 0;
				m_CurrentBuffer = 0;
				for (int i = 0; i < 4; i++)
				{
					for (int j = 0; j < 2; j++)
					{
						m_DataState[i, j] = 0;
					}
				}
				byte currentData = CheckNewestData();
				SetCurrentData(currentData);
			}

			public void SetCurrentData(byte current)
			{
				m_CurrentData = current;
				m_CurrentBuffer = 0;
				for (byte b = 0; b < m_BufferNum; b++)
				{
					if (m_DataState[current, b] == 1)
					{
						m_CurrentBuffer = b;
						break;
					}
				}
			}

			public byte CheckNewestData()
			{
				if (!PreviousProcess())
				{
					return 0;
				}
				_pRead pRead = null;
				if (CARD_IsBackupEeprom() != 0)
				{
					pRead = CARD_ReadEeprom;
				}
				else if (CARD_IsBackupFlash() != 0)
				{
					pRead = CARD_ReadFlash;
				}
				int num = 1;
				byte result = 0;
				uint num2 = 0u;
				byte b = 0;
				while (b < m_DataNum)
				{
					byte b2 = 0;
					uint num3 = 0u;
					for (byte b3 = 0; b3 < m_BufferNum; b3++)
					{
						uint romOffset = GetRomOffset(b, b3);
						Array array = new byte[36];
						num = pRead(romOffset, array, 36u);
						if (num != 1)
						{
							goto end_IL_00fe;
						}
						SaveHeader saveHeader = (SaveHeader)array;
						if (saveHeader.CheckDiscriminationSystemCode(DISCRIMINATION_SYSTEM_CODE))
						{
							m_DataState[b, b3] = 2;
							uint numberOfTimes = saveHeader.GetNumberOfTimes();
							if (numberOfTimes > num2)
							{
								result = b;
								num2 = numberOfTimes;
							}
							if (numberOfTimes > num3)
							{
								b2 = b3;
								num3 = numberOfTimes;
							}
						}
					}
					if (m_DataState[b, b2] == 2)
					{
						m_DataState[b, b2] = 1;
					}
					b++;
					continue;
					end_IL_00fe:
					break;
				}
				PostProcess();
				if (num != 1)
				{
					SetResult(RESULT.RESULT_LOST_CARD);
				}
				return result;
			}

			public uint GetRomByteSize(CARDBackupType type)
			{
				return type switch
				{
					CARDBackupType.CARD_BACKUP_TYPE_EEPROM_4KBITS => KBITStoBYTES(4), 
					CARDBackupType.CARD_BACKUP_TYPE_EEPROM_64KBITS => KBITStoBYTES(64), 
					CARDBackupType.CARD_BACKUP_TYPE_EEPROM_512KBITS => KBITStoBYTES(512), 
					CARDBackupType.CARD_BACKUP_TYPE_FLASH_2MBITS => KBITStoBYTES(2048), 
					CARDBackupType.CARD_BACKUP_TYPE_FLASH_4MBITS => KBITStoBYTES(4096), 
					CARDBackupType.CARD_BACKUP_TYPE_FRAM_256KBITS => KBITStoBYTES(256), 
					_ => 0u, 
				};
			}

			public uint GetRomOffset(byte num, byte buf)
			{
				GetRomByteSize(m_BackupType);
				return m_OneDataSize * m_BufferNum * num + m_OneDataSize * buf;
			}

			public byte GetDataNo(int num)
			{
				if (num == -1)
				{
					return m_CurrentData;
				}
				return (byte)num;
			}

			public byte GetBufferNo(int buf)
			{
				if (buf == -1)
				{
					return m_CurrentBuffer;
				}
				return (byte)buf;
			}

			public byte GetNextBufferNo(int buf)
			{
				if (buf == -1)
				{
					return (byte)((m_CurrentBuffer + 1 < m_BufferNum) ? ((byte)(m_CurrentBuffer + 1)) : 0);
				}
				return (byte)buf;
			}

			public bool StartSaveAddress(Array pData, uint data_size, uint rom_addr)
			{
				m_SpecifyAddress_1 = true;
				if (!PreviousProcess())
				{
					return false;
				}
				memcpy((sbyte[])pData, DISCRIMINATION_SYSTEM_CODE, DISCRIMINATION_SYSTEM_CODE.Length);
				SaveHeader.SetTime((byte[])pData);
				if (CARD_IsBackupEeprom() != 0)
				{
					CARD_WriteAndVerifyEepromAsync(rom_addr, pData, data_size, null, null);
				}
				else
				{
					if (CARD_IsBackupFlash() == 0)
					{
						m_Finished_1 = true;
						m_Result = RESULT.RESULT_SUCCESS;
						return false;
					}
					CARD_WriteAndVerifyFlashAsync(rom_addr, pData, data_size, null, null);
				}
				m_pDataAddress = pData;
				m_pProc = ExecuteSave;
				return true;
			}

			public bool StartSave(Array pData, uint data_size, int num, int buf)
			{
				byte dataNo = GetDataNo(num);
				SetCurrentData(dataNo);
				m_SaveBuffer = GetNextBufferNo(buf);
				uint romOffset = GetRomOffset(dataNo, m_SaveBuffer);
				bool result = StartSaveAddress(pData, data_size, romOffset);
				m_SpecifyAddress_1 = false;
				return result;
			}

			public bool StartLoadAddress(Array pData, uint data_size, uint rom_addr)
			{
				m_SpecifyAddress_1 = true;
				if (!PreviousProcess())
				{
					return false;
				}
				if (CARD_IsBackupEeprom() != 0)
				{
					CARD_ReadEepromAsync(rom_addr, pData, data_size, null, null);
				}
				else
				{
					if (CARD_IsBackupFlash() == 0)
					{
						m_Finished_1 = true;
						m_Result = RESULT.RESULT_SUCCESS;
						return false;
					}
					CARD_ReadFlashAsync(rom_addr, pData, data_size, null, null);
				}
				m_pDataAddress = pData;
				m_pProc = ExecuteLoad;
				return true;
			}

			public bool StartLoad(Array pData, uint data_size, int num, int buf)
			{
				byte dataNo = GetDataNo(num);
				SetCurrentData(dataNo);
				m_SaveBuffer = GetBufferNo(buf);
				uint romOffset = GetRomOffset(dataNo, m_SaveBuffer);
				bool result = StartLoadAddress(pData, data_size, romOffset);
				m_SpecifyAddress_1 = false;
				return result;
			}

			public bool Execute()
			{
				if (m_pProc != null)
				{
					m_pProc();
				}
				return m_Finished_1;
			}

			public bool IsExecute()
			{
				if (!m_Finished_1)
				{
					return true;
				}
				return false;
			}

			public bool IsHardError()
			{
				switch (m_Result)
				{
				case RESULT.RESULT_FAILURE:
				case RESULT.RESULT_INVALID_PARAM:
				case RESULT.RESULT_UNSUPPORTED:
				case RESULT.RESULT_TIMEOUT:
				case RESULT.RESULT_ERROR:
				case RESULT.RESULT_NO_RESPONSE:
				case RESULT.RESULT_LOST_CARD:
					return true;
				default:
					return false;
				}
			}

			public void ExecuteSave()
			{
				if (CARD_TryWaitBackupAsync() == 0)
				{
					return;
				}
				PostProcess();
				if (m_Result == RESULT.RESULT_SUCCESS && !m_SpecifyAddress_1)
				{
					if (m_DataState[m_CurrentData, m_CurrentBuffer] == 1)
					{
						m_DataState[m_CurrentData, m_CurrentBuffer] = 2;
					}
					m_DataState[m_CurrentData, m_SaveBuffer] = 1;
					m_CurrentBuffer = m_SaveBuffer;
				}
			}

			public void ExecuteLoad()
			{
				if (CARD_TryWaitBackupAsync() != 0)
				{
					PostProcess();
					_ = m_Result;
					if (m_Result == RESULT.RESULT_SUCCESS && !m_SpecifyAddress_1)
					{
						m_CurrentBuffer = m_SaveBuffer;
					}
				}
			}

			public bool PreviousProcess()
			{
				if (!m_Valid_1)
				{
					m_Finished_1 = true;
					m_Result = RESULT.RESULT_SUCCESS;
					return false;
				}
				CARD_LockBackup((ushort)m_CardLockId);
				CARD_IdentifyBackup(m_BackupType);
				m_Finished_1 = false;
				return true;
			}

			public void PostProcess()
			{
				m_Result = static_cast<RESULT>(CARD_GetResultCode());
				CARD_UnlockBackup((ushort)m_CardLockId);
				m_Finished_1 = true;
				m_pProc = null;
			}

			public uint CalculateSum(Array pAddr, int pStart, int pEnd)
			{
				uint num = 0u;
				for (int i = pStart; i < pEnd; i++)
				{
					num += (uint)pAddr.GetValue(i);
				}
				return num;
			}

			public bool CheckSum(Array pAddr, int pStart, int pEnd, uint sum)
			{
				if (sum != CalculateSum(pAddr, pStart, pEnd))
				{
					return false;
				}
				return true;
			}

			internal static void EncryptionEncode(byte[] pDst, byte[] pSrc, ushort key, uint size)
			{
			}

			public void setCardPulledOutCallback(CARDPulledOutCallback pCallbackFunc)
			{
				CARD_SetPulledOutCallback(pCallbackFunc);
			}

			public void SetValid(bool valid)
			{
				m_Valid_1 = valid;
			}

			public bool GetValid()
			{
				return m_Valid_1;
			}

			public void SetBackupType(CARDBackupType type)
			{
				m_BackupType = type;
			}

			public CARDBackupType GetBackupType()
			{
				return m_BackupType;
			}

			public uint GetAllDataSize()
			{
				return m_OneDataSize * m_DataNum * m_BufferNum;
			}

			public byte GetDataNum()
			{
				return m_DataNum;
			}

			public byte GetBufferNum()
			{
				return m_BufferNum;
			}

			public byte GetCurrentData()
			{
				return m_CurrentData;
			}

			public byte GetCurrentBuffer()
			{
				return m_CurrentBuffer;
			}

			public bool GetState(int num, int buf)
			{
				switch (m_DataState[GetDataNo(num), GetBufferNo(buf)])
				{
				case 1:
				case 2:
					return true;
				default:
					return false;
				}
			}

			public void SetResult(RESULT result)
			{
				m_Result = result;
			}

			public RESULT GetResult()
			{
				if (m_Valid_1)
				{
					return m_Result;
				}
				return RESULT.RESULT_SUCCESS;
			}

			public static Manager GetInstance()
			{
				return m_Instance;
			}
		}

		public class OmitTime
		{
			private byte Year_7;

			private byte Month_4;

			private byte Day_5;

			private byte Week_3;

			private byte Hour_5;

			private byte Minute_6;

			private byte Second_6;

			public bool IsLess(OmitTime ot)
			{
				uint[] array = new uint[2]
				{
					UnifyDate(),
					ot.UnifyDate()
				};
				if (array[0] > array[1])
				{
					return false;
				}
				if (array[0] == array[1] && UnifyTime() >= ot.UnifyTime())
				{
					return false;
				}
				return true;
			}

			public bool IsGreater(OmitTime ot)
			{
				uint[] array = new uint[2]
				{
					UnifyDate(),
					ot.UnifyDate()
				};
				if (array[0] < array[1])
				{
					return false;
				}
				if (array[0] == array[1] && UnifyTime() <= ot.UnifyTime())
				{
					return false;
				}
				return true;
			}

			public void Set(RTCDate pDate, RTCTime pTime)
			{
				if (pDate != null)
				{
					long absoluteTime = pDate.absoluteTime;
					Year_7 = 0;
					Month_4 = 0;
					Day_5 = (byte)(absoluteTime / 86400);
					Week_3 = 0;
					Hour_5 = (byte)(absoluteTime / 3600 % 24);
					Minute_6 = (byte)(absoluteTime / 60 % 60);
					Second_6 = (byte)(absoluteTime % 60);
				}
			}

			public void Get(RTCDate pDate, RTCTime pTime)
			{
				if (pDate != null)
				{
					pDate.year = Year_7;
					pDate.month = Month_4;
					pDate.day = Day_5;
					pDate.week = static_cast<int>(Week_3);
				}
				if (pTime != null)
				{
					pTime.hour = Hour_5;
					pTime.minute = Minute_6;
					pTime.second = Second_6;
				}
			}

			private uint UnifyDate()
			{
				return (uint)(Year_7 * 12 * 31 + Month_4 * 31 + Day_5);
			}

			private uint UnifyTime()
			{
				return (uint)(Hour_5 * 60 * 60 + Minute_6 * 60 + Second_6);
			}

			public void setDefault()
			{
				Year_7 = 0;
				Month_4 = 0;
				Day_5 = 0;
				Week_3 = 0;
				Hour_5 = 0;
				Minute_6 = 0;
				Second_6 = 0;
			}

			public static byte[] createRaw(long lSecond)
			{
				return new byte[6]
				{
					0,
					0,
					(byte)(lSecond / 86400),
					(byte)(lSecond / 3600 % 24),
					(byte)(lSecond / 60 % 60),
					(byte)(lSecond % 60)
				};
			}

			public void parse(ArrayReader reader)
			{
				Year_7 = reader.readByte();
				Month_4 = reader.readByte();
				Day_5 = reader.readByte();
				Week_3 = 0;
				Hour_5 = reader.readByte();
				Minute_6 = reader.readByte();
				Second_6 = reader.readByte();
			}

			public void store(ArrayWriter writer)
			{
				writer.writeByte(Year_7);
				writer.writeByte(Month_4);
				writer.writeByte(Day_5);
				writer.writeByte(Hour_5);
				writer.writeByte(Minute_6);
				writer.writeByte(Second_6);
			}
		}

		public class SaveHeader
		{
			private static int DISCRIMINATION_SYSTEM_CODE_START = 0;

			private static int DISCRIMINATION_USER_CODE_START = 7;

			private sbyte[] DiscriminationCode = new sbyte[DISCRIMINATION_CODE_LENGTH + DISCRIMINATION_CODE_LENGTH];

			private OmitTime Time = new OmitTime();

			private ushort Version;

			private ushort RandomSeed;

			private uint NumberOfTimes;

			private uint Sum;

			private uint DataSize;

			private byte[] m_abyRaw;

			public SaveHeader()
			{
				Clear();
			}

			public void Clear()
			{
				setDefault();
			}

			public void SetDiscriminationSystemCode(sbyte[] pCode)
			{
				for (int i = 0; i < DISCRIMINATION_CODE_LENGTH; i++)
				{
					DiscriminationCode[DISCRIMINATION_SYSTEM_CODE_START + i] = pCode[i];
				}
			}

			public bool CheckDiscriminationSystemCode(sbyte[] pCode)
			{
				for (int i = 0; i < DISCRIMINATION_CODE_LENGTH; i++)
				{
					if (DiscriminationCode[DISCRIMINATION_SYSTEM_CODE_START + i] != pCode[i])
					{
						return false;
					}
				}
				return true;
			}

			public void SetDiscriminationUserCode(sbyte[] pCode)
			{
				for (int i = 0; i < DISCRIMINATION_CODE_LENGTH; i++)
				{
					DiscriminationCode[DISCRIMINATION_USER_CODE_START + i] = pCode[i];
				}
			}

			public bool CheckDiscriminationUserCode(sbyte[] pCode)
			{
				for (int i = 0; i < DISCRIMINATION_CODE_LENGTH; i++)
				{
					if (DiscriminationCode[DISCRIMINATION_USER_CODE_START + i] != pCode[i])
					{
						return false;
					}
				}
				return true;
			}

			public void SetVersion(ushort ver)
			{
				Version = ver;
			}

			public bool CheckVersion(ushort ver)
			{
				if (Version != ver)
				{
					return false;
				}
				return true;
			}

			public void SetNumberOfTimes(uint num)
			{
				NumberOfTimes = num;
			}

			public void IncrementNumberOfTimes()
			{
				NumberOfTimes++;
			}

			public uint GetNumberOfTimes()
			{
				return NumberOfTimes;
			}

			public void SetRandomSeed(ushort seed)
			{
				RandomSeed = seed;
			}

			public void SetSum(uint sum)
			{
				Sum = sum;
			}

			public uint GetSum()
			{
				return Sum;
			}

			public void SetTime()
			{
				RTCDate rTCDate = new RTCDate();
				RTCTime rTCTime = new RTCTime();
				RTC_GetDateTime(rTCDate, rTCTime);
				Time.Set(rTCDate, rTCTime);
			}

			public static void SetTime(byte[] abyData)
			{
				RTCDate rTCDate = new RTCDate();
				RTCTime time = new RTCTime();
				RTC_GetDateTime(rTCDate, time);
				int arg = (int)(DISCRIMINATION_CODE_LENGTH + DISCRIMINATION_CODE_LENGTH);
				byte[] array = OmitTime.createRaw(rTCDate.absoluteTime);
				memcpy(abyData, arg, array, 0, array.Length);
			}

			public void GetTime(RTCDate pDate, RTCTime pTime)
			{
				Time.Get(pDate, pTime);
			}

			public OmitTime GetOmitTime()
			{
				return Time;
			}

			public virtual Array GetDataStartHeader()
			{
				toRaw();
				return m_abyRaw;
			}

			public void SetDataSize(uint size)
			{
				DataSize = size;
			}

			public uint GetDataSize()
			{
				return DataSize;
			}

			public void setDefault()
			{
				for (int i = 0; i < DiscriminationCode.Length; i++)
				{
					DiscriminationCode[i] = 0;
				}
				Time.setDefault();
				Version = 0;
				RandomSeed = 0;
				NumberOfTimes = 0u;
				Sum = 0u;
				DataSize = 0u;
			}

			public void toRaw()
			{
				ArrayWriter arrayWriter = new ArrayWriter();
				arrayWriter.write(DiscriminationCode, 0, DiscriminationCode.Length);
				Time.store(arrayWriter);
				arrayWriter.writeUInt16(Version);
				arrayWriter.writeUInt16(RandomSeed);
				arrayWriter.writeUInt32(NumberOfTimes);
				arrayWriter.writeUInt32(Sum);
				arrayWriter.writeUInt32(DataSize);
				m_abyRaw = arrayWriter.getBytes();
				arrayWriter.dispose();
			}

			public void parse(ArrayReader reader)
			{
				reader.read(DiscriminationCode, 0, DiscriminationCode.Length);
				Time.parse(reader);
				Version = reader.readUInt16();
				RandomSeed = reader.readUInt16();
				NumberOfTimes = reader.readUInt32();
				Sum = reader.readUInt32();
				DataSize = reader.readUInt32();
			}

			public static explicit operator SaveHeader(Array src)
			{
				SaveHeader saveHeader = new SaveHeader();
				ArrayReader arrayReader = new ArrayReader(src);
				arrayReader.read(saveHeader.DiscriminationCode, 0, saveHeader.DiscriminationCode.Length);
				saveHeader.Time.parse(arrayReader);
				saveHeader.Version = arrayReader.readUInt16();
				saveHeader.RandomSeed = arrayReader.readUInt16();
				saveHeader.NumberOfTimes = arrayReader.readUInt32();
				saveHeader.Sum = arrayReader.readUInt32();
				saveHeader.DataSize = arrayReader.readUInt32();
				arrayReader.dispose();
				return saveHeader;
			}
		}

		public enum RESULT
		{
			RESULT_SUCCESS = 0,
			RESULT_FAILURE = 1,
			RESULT_INVALID_PARAM = 2,
			RESULT_UNSUPPORTED = 3,
			RESULT_TIMEOUT = 4,
			RESULT_ERROR = 5,
			RESULT_NO_RESPONSE = 6,
			RESULT_LOST_CARD = 100,
			RESULT_WRONG_USER_CODE = 200,
			RESULT_WRONG_VERSION = 201,
			RESULT_WRONG_SUM = 202
		}

		public class CCSaveDataOriginal
		{
			protected string _Str;

			public long _npcMailSendTime;

			public mognet.NPCMailData _npcMailData_ = new mognet.NPCMailData();

			public pl.PlayerSaveData _PlayerSaveData = new pl.PlayerSaveData();

			public mon.MonsterManiaManager monsterSaveData_ = new mon.MonsterManiaManager();

			public wld.CWorldOutSideData _WorldOutSideData = new wld.CWorldOutSideData();

			public opt.COptData _OptionData = new opt.COptData();

			public string _strStageName = "";

			public byte[,] _Flags = new byte[3, 1000];

			public ds.Vector<mognet.MNMail, ds.OrderSavedErasePolicy<mognet.MNMail>> _Mails = new ds.Vector<mognet.MNMail, ds.OrderSavedErasePolicy<mognet.MNMail>>(mognet.MAILS_LIST_LENGTH);

			public void sdoCreate()
			{
				strcpy(out _Str, "EUREKA data");
				pl.PlayerParty.instance().sendSaveData(_PlayerSaveData);
				mon.MonsterManager.instance().sendMonsterManiaData(monsterSaveData_);
				wld.WorldPart.getInstance().getWorldSystem().BackUpPosition();
				wld.WorldPart.getInstance().getWorldSystem().BackUpVehiclePosition();
				_WorldOutSideData = wld.CWorldOutSideData.getInstance();
				_WorldOutSideData.MapData().PreBGMIndex_set(-1);
				strcpy(out _strStageName, sceneMng.getStage());
				memcpy(_Flags, getFlagImage(), 3000);
				mognet.MNNPCMailData.getSingleton().storeNPCMailData(_npcMailData_);
				_npcMailSendTime = mognet.MNMemento.getSingleton().mnmGetDateTimeNPC();
				mognet.MNMemento.getSingleton().mnmGetMails(_Mails);
			}

			public void sdoReflect()
			{
				pl.PlayerParty.instance().acceptSaveData(_PlayerSaveData);
				ds.GlobalPlayTimeCounter.getSingleton().set(pl.PlayerParty.instance().playTime());
				ds.GlobalPlayTimeCounter.getSingleton().start();
				mon.MonsterManager.instance().acceptMonsterManiaData(monsterSaveData_);
				wld.CWorldOutSideData.getInstance_set(_WorldOutSideData);
				sceneMng.gotoStage(wld.CWorldOutSideData.getInstance().MapData().getNowMapName());
				sceneMng.getStage_set(_strStageName);
				setFlagImage(_Flags, 3, 1000);
				mognet.MNNPCMailData.getSingleton().loadNPCMailData(_npcMailData_);
				mognet.MNMemento.getSingleton().mnmSetDateTimeNPC(_npcMailSendTime);
				mognet.MNMemento.getSingleton().mnmSetMails(_Mails);
			}

			public string getChrName(int n)
			{
				return _PlayerSaveData.player_[n].name();
			}

			public int getLV(int n)
			{
				return _PlayerSaveData.player_[n].level().get();
			}

			public int getHPLimit(int n)
			{
				return _PlayerSaveData.player_[n].hp().getLimit();
			}

			public int getHP(int n)
			{
				return _PlayerSaveData.player_[n].hp().getNow();
			}

			public dgs.TXT_COLOR getHPColor(int n)
			{
				return _PlayerSaveData.player_[n].checkHpColor();
			}

			public int getJobID(int n)
			{
				return _PlayerSaveData.player_[n].jobManager().nowJob();
			}

			public int getSkill(int n)
			{
				return _PlayerSaveData.player_[n].jobManager().nowJobParameter().skill()
					.skillLevel()
					.get();
			}

			public int getGold()
			{
				return _PlayerSaveData.gold_.get();
			}

			public uint getPlayTime()
			{
				return _PlayerSaveData.playTime_;
			}

			public string getSavePointName()
			{
				return null;
			}

			public int isChrEnable(int n)
			{
				if (_PlayerSaveData.player_[n].isEnable())
				{
					return 0;
				}
				return 1;
			}

			public int getCharID(int n)
			{
				return _PlayerSaveData.player_[n].playerId();
			}

			public int getFormation(int n)
			{
				return _PlayerSaveData.player_[n].formationType();
			}

			public int getNextLevelExp(int n)
			{
				int num = _PlayerSaveData.player_[n].level().get();
				if (99 == num)
				{
					return 0;
				}
				return pl.PlayerParty.instance().playerExp()[0].exp((byte)num) - _PlayerSaveData.player_[n].exp().get();
			}

			public bool checkFlag(int nGroup, int nNumber)
			{
				if (nGroup < 0 || nGroup >= 3)
				{
					return false;
				}
				if (nNumber < 0 || nNumber >= 1000)
				{
					return false;
				}
				if (1 != _Flags[nGroup, nNumber])
				{
					return false;
				}
				return true;
			}

			public void parse(ArrayReader reader)
			{
				byte[] array = new byte[32];
				_npcMailSendTime = reader.readInt64();
				_npcMailData_.parse(reader);
				_PlayerSaveData.parse(reader);
				monsterSaveData_.parse(reader);
				_WorldOutSideData.parse(reader);
				_OptionData.parse(reader);
				reader.read(array, 0, 32);
				_strStageName = StringUtil.createString(array);
				for (int i = 0; i < 3; i++)
				{
					for (int j = 0; j < 1000; j++)
					{
						_Flags[i, j] = reader.readByte();
					}
				}
			}

			public void store(ArrayWriter writer)
			{
				byte[] array = new byte[32];
				byte[] bytes = StringUtil.getBytes(_strStageName);
				memcpy(array, bytes, bytes.Length);
				writer.writeInt64(_npcMailSendTime);
				_npcMailData_.store(writer);
				_PlayerSaveData.store(writer);
				monsterSaveData_.store(writer);
				_WorldOutSideData.store(writer);
				_OptionData.store(writer);
				writer.write(array, 0, 32);
				for (int i = 0; i < 3; i++)
				{
					for (int j = 0; j < 1000; j++)
					{
						writer.writeByte(_Flags[i, j]);
					}
				}
			}
		}

		public class CSaveData : SaveHeader
		{
			public CCSaveDataOriginal composit = new CCSaveDataOriginal();

			private byte[] m_abyRaw;

			public void sdCreate()
			{
				sdDump();
				composit.sdoCreate();
				SetDiscriminationUserCode(sdDISCRIMINATION_USER_CODE);
				SetVersion(sdVERSION);
			}

			public bool sdCheck()
			{
				if (sdGetResult() != RESULT.RESULT_SUCCESS)
				{
					return false;
				}
				if (!CheckDiscriminationUserCode(sdDISCRIMINATION_USER_CODE))
				{
					Manager.GetInstance().SetResult(RESULT.RESULT_WRONG_USER_CODE);
					return false;
				}
				if (!CheckVersion(sdVERSION))
				{
					Manager.GetInstance().SetResult(RESULT.RESULT_WRONG_VERSION);
					return false;
				}
				return true;
			}

			public bool sdReflect()
			{
				composit.sdoReflect();
				// PORT: the loaded party is in place; mastery heroes' commands follow their ladders.
				OpenFF.Client.ProgressionLayer.OnLoaded();
				return true;
			}

			public void sdSave(int num, bool sub)
			{
				IncrementNumberOfTimes();
				Manager.GetInstance().StartSave((sbyte[])GetDataStartHeader(), GetDataSize(), num, -1);
				// PORT: the mods' progression state (job ladders) rides beside the slot.
				OpenFF.Client.ProgressionLayer.OnSave(num);
			}

			public void sdLoad(int num, bool sub)
			{
				int buf = ((!sub) ? (-1) : ((Manager.GetInstance().GetCurrentBuffer() == 0) ? 1 : 0));
				Manager.GetInstance().StartLoad((sbyte[])GetDataStartHeader(), GetDataSize(), num, buf);
				fromRaw();
				// PORT: the mods' progression state from beside the slot.
				OpenFF.Client.ProgressionLayer.OnLoad(num);
			}

			public bool sdExecute()
			{
				return Manager.GetInstance().Execute();
			}

			public RESULT sdGetResult()
			{
				return Manager.GetInstance().GetResult();
			}

			public void sdDump()
			{
			}

			public CSaveData()
			{
				SetDataSize(13842u);
			}

			~CSaveData()
			{
			}

			public override Array GetDataStartHeader()
			{
				toRaw();
				return m_abyRaw;
			}

			public new void toRaw()
			{
				ArrayWriter arrayWriter = new ArrayWriter();
				byte[] array = (byte[])base.GetDataStartHeader();
				arrayWriter.write(array, 0, array.Length);
				composit.store(arrayWriter);
				m_abyRaw = arrayWriter.getBytes();
				arrayWriter.dispose();
			}

			public void fromRaw()
			{
				ArrayReader arrayReader = new ArrayReader(m_abyRaw);
				parse(arrayReader);
				composit.parse(arrayReader);
				arrayReader.dispose();
			}

			public void copy(CSaveData src)
			{
				m_abyRaw = src.m_abyRaw;
				fromRaw();
				toRaw();
			}
		}

		public class CSaveDataOrigin : SaveHeader
		{
			public bool sdCheck()
			{
				if (sdGetResult() != RESULT.RESULT_SUCCESS)
				{
					return false;
				}
				if (!CheckDiscriminationUserCode(sdDISCRIMINATION_USER_CODE))
				{
					Manager.GetInstance().SetResult(RESULT.RESULT_WRONG_USER_CODE);
					return false;
				}
				if (!CheckVersion(sdVERSION))
				{
					Manager.GetInstance().SetResult(RESULT.RESULT_WRONG_VERSION);
					return false;
				}
				return true;
			}

			public bool sdExecute()
			{
				return Manager.GetInstance().Execute();
			}

			public RESULT sdGetResult()
			{
				return Manager.GetInstance().GetResult();
			}

			~CSaveDataOrigin()
			{
			}

			public virtual void sdCreate()
			{
			}

			public virtual bool sdReflect()
			{
				return true;
			}
		}

		public class SaveDataNormal : CSaveDataOrigin
		{
			public CCSaveDataOriginal composit = new CCSaveDataOriginal();

			private byte[] m_abyRaw;

			public override void sdCreate()
			{
				composit.sdoCreate();
				SetDiscriminationUserCode(sdDISCRIMINATION_USER_CODE);
				SetVersion(sdVERSION);
			}

			public override bool sdReflect()
			{
				if (!sdCheck())
				{
					return false;
				}
				composit.sdoReflect();
				return true;
			}

			public void sdnSave(int num, bool sub)
			{
				IncrementNumberOfTimes();
				Manager.GetInstance().StartSave((sbyte[])GetDataStartHeader(), GetDataSize(), num, -1);
			}

			public void sdnLoad(int num, bool sub)
			{
				int buf = ((!sub) ? (-1) : ((Manager.GetInstance().GetCurrentBuffer() == 0) ? 1 : 0));
				Manager.GetInstance().StartLoad((sbyte[])GetDataStartHeader(), GetDataSize(), num, buf);
				fromRaw();
			}

			public SaveDataNormal()
			{
				SetDataSize(13842u);
			}

			public override Array GetDataStartHeader()
			{
				toRaw();
				return m_abyRaw;
			}

			public new void toRaw()
			{
				ArrayWriter arrayWriter = new ArrayWriter();
				byte[] array = (byte[])base.GetDataStartHeader();
				arrayWriter.write(array, 0, array.Length);
				composit.store(arrayWriter);
				m_abyRaw = arrayWriter.getBytes();
				arrayWriter.dispose();
			}

			public void fromRaw()
			{
				ArrayReader arrayReader = new ArrayReader(m_abyRaw);
				parse(arrayReader);
				composit.parse(arrayReader);
				arrayReader.dispose();
			}

			public void copy(SaveDataNormal src)
			{
				m_abyRaw = src.m_abyRaw;
				fromRaw();
				toRaw();
			}
		}

		public class SaveDataAddress : SaveHeader
		{
			private int Validity_;

			public CCSaveDataOriginal composit = new CCSaveDataOriginal();

			private byte[] m_abyRaw;

			~SaveDataAddress()
			{
			}

			public void sdCreate()
			{
				mon.MonsterManager.instance().sendMonsterManiaData(composit.monsterSaveData_);
				pl.PlayerParty.instance().sendSaveData(composit._PlayerSaveData);
				wld.WorldPart.getInstance().getWorldSystem().BackUpPosition();
				wld.WorldPart.getInstance().getWorldSystem().BackUpVehiclePosition();
				composit._WorldOutSideData = wld.CWorldOutSideData.getInstance();
				composit._WorldOutSideData.MapData().PreBGMIndex_set(-1);
				strcpy(out composit._strStageName, sceneMng.getStage());
				memcpy(composit._Flags, getFlagImage(), 3000);
				composit._npcMailSendTime = mognet.MNMemento.getSingleton().mnmGetDateTimeNPC();
				mognet.MNMemento.getSingleton().mnmGetMails(composit._Mails);
				mognet.MNNPCMailData.getSingleton().storeNPCMailData(composit._npcMailData_);
				SetDiscriminationUserCode(sdDISCRIMINATION_USER_CODE);
				SetVersion(sdVERSION);
			}

			public bool sdReflect()
			{
				mon.MonsterManager.instance().acceptMonsterManiaData(composit.monsterSaveData_);
				pl.PlayerParty.instance().acceptSaveData(composit._PlayerSaveData);
				ds.GlobalPlayTimeCounter.getSingleton().set(pl.PlayerParty.instance().playTime());
				ds.GlobalPlayTimeCounter.getSingleton().start();
				wld.CWorldOutSideData.getInstance_set(composit._WorldOutSideData);
				sceneMng.gotoStage(wld.CWorldOutSideData.getInstance().MapData().getNowMapName());
				printWorldOutSide();
				sceneMng.getStage_set(composit._strStageName);
				setFlagImage(composit._Flags, 3, 1000);
				mognet.MNMemento.getSingleton().mnmSetDateTimeNPC(composit._npcMailSendTime);
				mognet.MNMemento.getSingleton().mnmSetMails(composit._Mails);
				mognet.MNNPCMailData.getSingleton().loadNPCMailData(composit._npcMailData_);
				// PORT: the quick-saved party is in place; mastery heroes' commands follow their ladders.
				OpenFF.Client.ProgressionLayer.OnLoaded();
				return true;
			}

			public void sdaLoad(uint rom_addr)
			{
				Manager.GetInstance().StartLoadAddress((sbyte[])GetDataStartHeader(), GetDataSize(), rom_addr);
				fromRaw();
				// PORT: the mods' progression state from beside the quick save (slot -1).
				OpenFF.Client.ProgressionLayer.OnLoad(-1);
			}

			public void sdaSave(uint rom_addr)
			{
				IncrementNumberOfTimes();
				SetDiscriminationUserCode(sdDISCRIMINATION_USER_CODE);
				Manager.GetInstance().StartSaveAddress((sbyte[])GetDataStartHeader(), GetDataSize(), rom_addr);
				// PORT: the mods' progression state beside the quick save (slot -1).
				OpenFF.Client.ProgressionLayer.OnSave(-1);
			}

			public bool sdExecute()
			{
				return Manager.GetInstance().Execute();
			}

			public bool sdCheck()
			{
				if (sdGetResult() != RESULT.RESULT_SUCCESS)
				{
					return false;
				}
				if (!CheckDiscriminationUserCode(sdDISCRIMINATION_USER_CODE))
				{
					Manager.GetInstance().SetResult(RESULT.RESULT_WRONG_USER_CODE);
					return false;
				}
				if (!CheckVersion(sdVERSION))
				{
					Manager.GetInstance().SetResult(RESULT.RESULT_WRONG_VERSION);
					return false;
				}
				return true;
			}

			public RESULT sdGetResult()
			{
				return Manager.GetInstance().GetResult();
			}

			public int sdaValidity()
			{
				return Validity_;
			}

			public void sdaSetValidity(int Validity)
			{
				Validity_ = Validity;
			}

			public void sdaInvalidate()
			{
				Validity_ = 0;
				composit._PlayerSaveData.setDefault();
				composit._WorldOutSideData.setDefault();
				composit._strStageName = "";
				for (int i = 0; i < composit._Flags.GetLength(0); i++)
				{
					for (int j = 0; j < composit._Flags.GetLength(1); j++)
					{
						composit._Flags[i, j] = 0;
					}
				}
				composit._npcMailSendTime = 0L;
				composit._Mails.clear();
			}

			public SaveDataAddress()
			{
				SetDataSize(13846u);
			}

			public override Array GetDataStartHeader()
			{
				toRaw();
				return m_abyRaw;
			}

			public new void toRaw()
			{
				ArrayWriter arrayWriter = new ArrayWriter();
				byte[] array = (byte[])base.GetDataStartHeader();
				arrayWriter.write(array, 0, array.Length);
				arrayWriter.writeInt32(Validity_);
				composit.store(arrayWriter);
				m_abyRaw = arrayWriter.getBytes();
				arrayWriter.dispose();
			}

			public void fromRaw()
			{
				ArrayReader arrayReader = new ArrayReader(m_abyRaw);
				parse(arrayReader);
				Validity_ = arrayReader.readInt32();
				composit.parse(arrayReader);
				arrayReader.dispose();
			}

			public void copy(SaveDataAddress src)
			{
				m_abyRaw = src.m_abyRaw;
				fromRaw();
				toRaw();
			}
		}

		public class SaveDataOption : SaveHeader
		{
			private int Validity_;

			public opt.COptData _OptionData = new opt.COptData();

			private byte[] m_abyRaw;

			~SaveDataOption()
			{
			}

			public void sdCreate()
			{
				opt.COptionManager.getSingleton().storeSaveData(_OptionData);
				SetDiscriminationUserCode(sdDISCRIMINATION_USER_CODE);
				SetVersion(sdVERSION);
			}

			public bool sdReflect()
			{
				opt.COptionManager.getSingleton().restoreSaveData(_OptionData);
				return true;
			}

			public void sdaLoad(uint rom_addr)
			{
				Manager.GetInstance().StartLoadAddress((sbyte[])GetDataStartHeader(), GetDataSize(), rom_addr);
				fromRaw();
			}

			public void sdaSave(uint rom_addr)
			{
				IncrementNumberOfTimes();
				SetDiscriminationUserCode(sdDISCRIMINATION_USER_CODE);
				Manager.GetInstance().StartSaveAddress((sbyte[])GetDataStartHeader(), GetDataSize(), rom_addr);
			}

			public bool sdExecute()
			{
				return Manager.GetInstance().Execute();
			}

			public bool sdCheck()
			{
				if (sdGetResult() != RESULT.RESULT_SUCCESS)
				{
					return false;
				}
				if (!CheckDiscriminationUserCode(sdDISCRIMINATION_USER_CODE))
				{
					Manager.GetInstance().SetResult(RESULT.RESULT_WRONG_USER_CODE);
					return false;
				}
				if (!CheckVersion(sdVERSION))
				{
					Manager.GetInstance().SetResult(RESULT.RESULT_WRONG_VERSION);
					return false;
				}
				return true;
			}

			public RESULT sdGetResult()
			{
				return Manager.GetInstance().GetResult();
			}

			public int sdaValidity()
			{
				return Validity_;
			}

			public void sdaSetValidity(int Validity)
			{
				Validity_ = Validity;
			}

			public void sdaInvalidate()
			{
				Validity_ = 0;
			}

			public SaveDataOption()
			{
				SetDataSize(68u);
			}

			public override Array GetDataStartHeader()
			{
				toRaw();
				return m_abyRaw;
			}

			public new void toRaw()
			{
				ArrayWriter arrayWriter = new ArrayWriter();
				byte[] array = (byte[])base.GetDataStartHeader();
				arrayWriter.write(array, 0, array.Length);
				arrayWriter.writeInt32(Validity_);
				_OptionData.store(arrayWriter);
				m_abyRaw = arrayWriter.getBytes();
				arrayWriter.dispose();
			}

			public void fromRaw()
			{
				ArrayReader arrayReader = new ArrayReader(m_abyRaw);
				parse(arrayReader);
				Validity_ = arrayReader.readInt32();
				_OptionData.parse(arrayReader);
				arrayReader.dispose();
			}

			public void copy(SaveDataOption src)
			{
				m_abyRaw = src.m_abyRaw;
				fromRaw();
				toRaw();
			}
		}

		public const RESULT RESULT_SUCCESS = RESULT.RESULT_SUCCESS;

		public const RESULT RESULT_FAILURE = RESULT.RESULT_FAILURE;

		public const RESULT RESULT_INVALID_PARAM = RESULT.RESULT_INVALID_PARAM;

		public const RESULT RESULT_UNSUPPORTED = RESULT.RESULT_UNSUPPORTED;

		public const RESULT RESULT_TIMEOUT = RESULT.RESULT_TIMEOUT;

		public const RESULT RESULT_ERROR = RESULT.RESULT_ERROR;

		public const RESULT RESULT_NO_RESPONSE = RESULT.RESULT_NO_RESPONSE;

		public const RESULT RESULT_LOST_CARD = RESULT.RESULT_LOST_CARD;

		public const RESULT RESULT_WRONG_USER_CODE = RESULT.RESULT_WRONG_USER_CODE;

		public const RESULT RESULT_WRONG_VERSION = RESULT.RESULT_WRONG_VERSION;

		public const RESULT RESULT_WRONG_SUM = RESULT.RESULT_WRONG_SUM;

		public const uint ONCE_INITIALIZE_CODE_SIZE = 32u;

		public const int MAX_SAVEDATA_SIZE = 16256;

		private static sbyte[] DISCRIMINATION_SYSTEM_CODE = new sbyte[7] { 99, 100, 49, 48, 48, 48, 0 };

		public static sbyte[] ONCE_INITIALIZE_CODE = new sbyte[32]
		{
			84, 104, 105, 115, 32, 99, 97, 114, 100, 32,
			119, 97, 115, 32, 105, 110, 105, 116, 105, 97,
			108, 105, 122, 101, 100, 46, 0, 0, 0, 0,
			0, 0
		};

		private static uint DISCRIMINATION_CODE_LENGTH = 7u;

		public static uint SSD_SIGN = 1146311510u;

		private static sbyte[] sdDISCRIMINATION_USER_CODE = new sbyte[7] { 69, 85, 82, 101, 75, 97, 0 };

		private static ushort sdVERSION = 53;

		internal static int defaultCardPulledOutCallback()
		{
			CARD_TerminateForPulledOut();
			return 0;
		}

		internal static void printWorldOutSide()
		{
			for (int i = 0; 24L > (long)i; i++)
			{
				VecFx32 position = wld.CWorldOutSideData.getInstance().PlayerData().getHoldData(i)
					.m_Position;
				VecFx32 rotation = wld.CWorldOutSideData.getInstance().PlayerData().getHoldData(i)
					.m_Rotation;
				OS_Printf("----------------------------------------.\n");
				OS_Printf("player[ %d ].\n", i);
				OS_Printf("position x = %4d, y = %4d, z = %4d.\n", position.x / 4096, position.y / 4096, position.z / 4096);
				OS_Printf("rotation x = %4d, y = %4d, z = %4d.\n", rotation.x / 4096, rotation.y / 4096, rotation.z / 4096);
				OS_Printf("----------------------------------------.\n");
			}
		}

		internal static bool SaveSuspend()
		{
			SaveDataAddress saveDataAddress = new SaveDataAddress();
			saveDataAddress.sdaSetValidity((int)SSD_SIGN);
			saveDataAddress.sdCreate();
			saveDataAddress.sdaSave(41568u);
			while (!saveDataAddress.sdExecute())
			{
			}
			return saveDataAddress.sdCheck();
		}

		internal static bool SaveOption()
		{
			SaveDataOption saveDataOption = new SaveDataOption();
			saveDataOption.sdaSetValidity((int)SSD_SIGN);
			saveDataOption.sdCreate();
			saveDataOption.sdaSave(55424u);
			while (!saveDataOption.sdExecute())
			{
			}
			return saveDataOption.sdCheck();
		}
	}
}
